using Raven.Client.Documents;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TSQL;
using TSQL.Statements;
using TSQL.Tokens;

namespace PgRvn.Server
{
    class Session
    {
        private readonly TcpClient _client;
        private readonly CancellationToken _token;
        private readonly int _identifier;
        private Dictionary<string, string> _clientOptions;
        private readonly int _processId;
        private readonly DocumentStore _docStore;

        public Session(TcpClient client, CancellationToken token, int identifier, int processId, DocumentStore docStore)
        {
            _client = client;
            _token = token;
            _identifier = identifier;
            _processId = processId;
            this._docStore = docStore;
        }

        public enum ProtocolVersion
        {
            Version3 = 0x00030000,
            TlsConnection = 80877103,
            CancelMessage = 080877102
        }

        public async Task Run()
        {
            using var _ = _client;
            using var messageBuilder = new MessageBuilder();
            await using var stream = _client.GetStream();

            var reader = PipeReader.Create(stream);
            var writer = PipeWriter.Create(stream);

            var msgLen = await ReadInt32Async(reader) - sizeof(int);
            var protocolVersion = await ReadInt32Async(reader);

            protocolVersion = await HandleTlsConnection(protocolVersion);

            if (protocolVersion == (int) ProtocolVersion.CancelMessage)
            {
                // TODO: Handle Cancel message
                throw new NotSupportedException("Will handle later");
            }
            else
            {
                await HandleHandshake(protocolVersion, msgLen, reader);
            }

            var transaction = new Transaction(_docStore);

            await writer.WriteAsync(messageBuilder.AuthenticationOk(), _token);
            await writer.WriteAsync(messageBuilder.ParameterStatusMessages(PgConfig.ParameterStatusList), _token);
            await writer.WriteAsync(messageBuilder.BackendKeyData(_processId, _identifier), _token);
            await writer.WriteAsync(messageBuilder.ReadyForQuery(transaction.State), _token);

            while (_token.IsCancellationRequested == false)
            {
                var message = await ReadMessage(reader);

                if (message is Terminate)
                    break;

                // TODO: Should maybe move this to inside each transaction.Function ?
                if (transaction.State == TransactionState.Failed && message is not Sync)
                    continue;

                var response = message switch
                {
                    Parse parse => transaction.Parse(parse, messageBuilder),
                    Bind bind => transaction.Bind(bind, messageBuilder),
                    Sync => transaction.Sync(messageBuilder),
                    Describe => await transaction.Describe(messageBuilder, writer, _token),
                    Execute => await transaction.Execute(messageBuilder, writer, _token),
                    _ => throw new NotSupportedException()
                };

                if (!response.IsEmpty)
                {
                    await writer.WriteAsync(response, _token);
                    continue;
                }
            }
        }

        private async Task<int> HandleTlsConnection(int protocolVersion)
        {
            if (protocolVersion == (int)ProtocolVersion.TlsConnection)
            {
                // TODO: Respond with 'S' if willing to perform SSL or 'N' otherwise
                throw new NotSupportedException("Will handle later");
                // establish ssl
                // await HandleHandshake(version, msgLen, reader
                // Get new protocol version
                // protocolVersion = givenVersion
            }

            return protocolVersion;
        }

        private async Task<Message> ReadMessage(PipeReader reader)
        {
            var msgType = await ReadByteAsync(reader);
            var msgLen = await ReadInt32Async(reader) - sizeof(int);

            switch (msgType)
            {
                case (byte)MessageType.Parse:
                {
                    var (statementName, statementLength) = await ReadNullTerminatedString(reader);
                    msgLen -= statementLength;

                    var (query, queryLength) = await ReadNullTerminatedString(reader);
                    msgLen -= queryLength;

                    var parametersCount = await ReadInt16Async(reader);
                    msgLen -= sizeof(short);

                    var parameters = new int[parametersCount];
                    for (int i = 0; i < parametersCount; i++)
                    {
                        parameters[i] = await ReadInt32Async(reader);
                        msgLen -= sizeof(int);
                    }

                    if (msgLen != 0)
                        throw new InvalidOperationException("Wrong size?");

                    return new Parse
                    {
                        StatementName = statementName,
                        Query = query,
                        ParametersDataTypes = parameters
                    };
                }

                case (byte)MessageType.Bind:
                {
                    var (destPortalName, destPortalLength) = await ReadNullTerminatedString(reader);
                    msgLen -= destPortalLength;

                    var (preparedStatementName, preparedStatementLength) = await ReadNullTerminatedString(reader);
                    msgLen -= preparedStatementLength;

                    var parameterFormatCodeCount = await ReadInt16Async(reader);
                    msgLen -= sizeof(short);

                    var parameterCodes = new short[parameterFormatCodeCount];
                    for (var i = 0; i < parameterFormatCodeCount; i++)
                    {
                        parameterCodes[i] = await ReadInt16Async(reader);
                        msgLen -= sizeof(short);
                    }

                    var parametersCount = await ReadInt16Async(reader);
                    msgLen -= sizeof(short);

                    var parameters = new List<byte[]>(parametersCount);
                    for (var i = 0; i < parametersCount; i++)
                    {
                        var parameterLength = await ReadInt32Async(reader);
                        msgLen -= sizeof(int);

                        // TODO: Is it okay to allocate up to 2GB of data based on external output?
                        // TODO: Format as bytes vs. text depending on the format codes given
                        parameters.Add(await ReadBytesAsync(reader, parameterLength));
                        msgLen -= parameterLength;
                    }

                    var resultColumnFormatCodesCount = await ReadInt16Async(reader);
                    msgLen -= sizeof(short);

                    var resultColumnFormatCodes = new short[resultColumnFormatCodesCount];
                    for (var i = 0; i < resultColumnFormatCodesCount; i++)
                    {
                        resultColumnFormatCodes[i] = await ReadInt16Async(reader);
                        msgLen -= sizeof(short);
                    }

                    if (msgLen != 0)
                        throw new InvalidOperationException("Wrong size?");

                    return new Bind
                    {
                        PortalName = destPortalName,
                        StatementName = preparedStatementName,
                        ParameterFormatCodes = parameterCodes,
                        Parameters = parameters,
                        ResultColumnFormatCodes = resultColumnFormatCodes
                    };
                }

                case (byte)MessageType.Describe:
                {
                    var describeObjectType = await ReadByteAsync(reader);
                    msgLen -= sizeof(byte);

                    var pgObjectType = describeObjectType switch
                    {
                        (byte)PgObjectType.Portal => PgObjectType.Portal,
                        (byte)PgObjectType.PreparedStatement => PgObjectType.PreparedStatement,
                        _ => throw new InvalidOperationException("Expected valid object type ('S' or 'P'), got: '" +
                                                                 describeObjectType)
                    };

                    var (describedName, describedNameLength) = await ReadNullTerminatedString(reader);
                    msgLen -= describedNameLength;

                    if (msgLen != 0)
                        throw new InvalidOperationException("Wrong size?");

                    return new Describe
                    {
                        PgObjectType = pgObjectType,
                        ObjectName = describedName
                    };
                }

                case (byte)MessageType.Execute:
                {
                    var (portalName, portalNameLength) = await ReadNullTerminatedString(reader);
                    msgLen -= portalNameLength;

                    var maxRowsToReturn = await ReadInt32Async(reader);
                    msgLen -= sizeof(int);

                    if (msgLen != 0)
                        throw new InvalidOperationException("Wrong size?");

                    return new Execute
                    {
                        PortalName = portalName,
                        MaxRows = maxRowsToReturn
                    };
                }

                case (byte) MessageType.Sync:
                {
                    if (msgLen != 0)
                        throw new InvalidOperationException("Wrong size?");

                    return new Sync();
                }

                case (byte) MessageType.Terminate:
                {
                    if (msgLen != 0)
                        throw new InvalidOperationException("Wrong size?");

                    return new Terminate();
                }

                default:
                    throw new NotSupportedException("Message type unsupported: " + (char)msgType);
            }
        }

        private async Task HandleHandshake(int version, int msgLen, PipeReader reader)
        {
            if (version != (int)ProtocolVersion.Version3)
            {
                throw new InvalidOperationException("Unexpected protocol version: " + version);
            }

            msgLen -= sizeof(int);
            _clientOptions = new Dictionary<string, string>();
            while (msgLen > 0)
            {
                var (key, keyLenInBytes) = await ReadNullTerminatedString(reader);
                msgLen -= keyLenInBytes;
                if (msgLen == 0)
                    break;
                var (val, valLenInBytes) = await ReadNullTerminatedString(reader);
                _clientOptions[key] = val;
                msgLen -= valLenInBytes;
            }

            if (_clientOptions.TryGetValue("client_encoding", out var encoding) && encoding != "UTF8")
                throw new InvalidOperationException("Only UTF8 encoding is supported, but got: " + encoding);

            if (_clientOptions.TryGetValue("database", out var db) == false)
                throw new InvalidOperationException("The database wasn't specified, but is mandatory");
        }

        private async Task<(string String, int LengthInBytes)> ReadNullTerminatedString(PipeReader reader)
        {
            ReadResult read;
            SequencePosition? end;

            while (true)
            {
                read = await reader.ReadAsync(_token);
                end = read.Buffer.PositionOf((byte)0);
                if (end != null)
                    break;

                reader.AdvanceTo(read.Buffer.Start, read.Buffer.End);
            }

            var match = read.Buffer.Slice(0, end.Value);
            var result = Encoding.UTF8.GetString(match);
            reader.AdvanceTo(read.Buffer.GetPosition(1, end.Value));

            return (result, (int)match.Length + 1);
        }
        private async Task<ReadResult> ReadMinimumOf(PipeReader reader, int minSizeRequired)
        {
            var read = await reader.ReadAsync(_token);

            while (read.Buffer.Length < minSizeRequired)
            {
                reader.AdvanceTo(read.Buffer.Start, read.Buffer.End);
                read = await reader.ReadAsync(_token);
            }

            return read;
        }

        private async Task<byte[]> ReadBytesAsync(PipeReader reader, int length)
        {
            var read = await ReadMinimumOf(reader, length);
            return ReadBytes(read, reader, length);
        }

        private async Task<int> ReadInt32Async(PipeReader reader)
        {
            var read = await ReadMinimumOf(reader, sizeof(int));
            return ReadInt32(read, reader);
        }

        private async Task<short> ReadInt16Async(PipeReader reader)
        {
            var read = await ReadMinimumOf(reader, sizeof(int));
            return ReadInt16(read, reader);
        }

        private async Task<byte> ReadByteAsync(PipeReader reader)
        {
            var read = await ReadMinimumOf(reader, sizeof(byte));
            return ReadByte(read, reader);
        }

        private static byte[] ReadBytes(ReadResult read, PipeReader reader, int length)
        {
            var sequence = read.Buffer.Slice(0, length);
            var buffer = new byte[length];
            sequence.CopyTo(buffer);
            reader.AdvanceTo(sequence.End);
            return buffer;
        }

        private static int ReadInt32(ReadResult read, PipeReader reader)
        {
            var sequence = read.Buffer.Slice(0, sizeof(int));
            Span<byte> buffer = stackalloc byte[sizeof(int)];
            sequence.CopyTo(buffer);
            reader.AdvanceTo(sequence.End);
            return IPAddress.NetworkToHostOrder(MemoryMarshal.AsRef<int>(buffer));
        }

        private static short ReadInt16(ReadResult read, PipeReader reader)
        {
            var sequence = read.Buffer.Slice(0, sizeof(short));
            Span<byte> buffer = stackalloc byte[sizeof(short)];
            sequence.CopyTo(buffer);
            reader.AdvanceTo(sequence.End);
            return IPAddress.NetworkToHostOrder(MemoryMarshal.AsRef<short>(buffer));
        }
        private static byte ReadByte(ReadResult read, PipeReader reader)
        {
            var charByte = read.Buffer.First.Span[0];
            reader.AdvanceTo(read.Buffer.GetPosition(sizeof(byte), read.Buffer.Start));
            return charByte;
        }
    }
}
