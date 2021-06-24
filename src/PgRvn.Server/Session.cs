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

        public Session(TcpClient client, CancellationToken token, int identifier, int processId)
        {
            _client = client;
            _token = token;
            _identifier = identifier;
            _processId = processId;
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
            var version = await ReadInt32Async(reader);

            if (version == (int) ProtocolVersion.TlsConnection)
            {
                // TODO: Respond with 'S' if willing to perform SSL or 'N' otherwise
                throw new NotSupportedException("Will handle later");
                // establish ssl
                // await HandleHandshake(version, msgLen, reader
            }

            if (version == (int) ProtocolVersion.CancelMessage)
            {
                // TODO: Check if Cancel message
                throw new NotSupportedException("Will handle later");
            }
            else
            {
                await HandleHandshake(version, msgLen, reader);
            }
            
            await writer.WriteAsync(messageBuilder.AuthenticationOk(), _token);
            await writer.WriteAsync(messageBuilder.ParameterStatusMessages(PgConfig.ParameterStatusList), _token);
            await writer.WriteAsync(messageBuilder.BackendKeyData(_processId, _identifier), _token);

            while (_token.IsCancellationRequested == false)
            {
                await writer.WriteAsync(messageBuilder.ReadyForQuery(), _token);
                await ReadMessage(reader);
            }
        }

        private void ExecuteStatement(TSQLStatement stmt)
        {
            if (stmt.AsSelect.From == null)
            {
                var offset = 1;
                SelectField(stmt, ref offset);
            }
        }

        private static void SelectField(TSQLStatement stmt, ref int offset)
        {
            var identifier = stmt.AsSelect.Select.Tokens[offset];
            switch (identifier.Type)
            {
                case TSQLTokenType.Identifier:
                    if (offset + 1 < stmt.AsSelect.Select.Tokens.Count)
                    {
                        if (stmt.AsSelect.Select.Tokens[offset + 1].Text == "(")
                        {
                            offset += 2;
                            var result = methods[identifier.Text](stmt.AsSelect.Select.Tokens, ref offset); 
                        }
                    }
                    break;
                default:
                    throw new NotSupportedException(identifier.ToString());
            }
        }

        private delegate object SqlMethodDelegate(List<TSQLToken> tokens, ref int offset);

        private static Dictionary<string, SqlMethodDelegate> methods = new(StringComparer.OrdinalIgnoreCase)
        {
            ["version"] = VersionMethod
        };

        private static object VersionMethod(List<TSQLToken> tokens, ref int offset)
        {
            offset++;// todo: validate
            return "0.10-alpga2";
        }
        

        private async Task<Message> ReadMessage(PipeReader reader)
        {
            var read = await reader.ReadAsync(_token);
            if (read.Buffer.IsEmpty)
                throw new EndOfStreamException();

            var msgType = read.Buffer.First.Span[0];
            reader.AdvanceTo(read.Buffer.GetPosition(1, read.Buffer.Start));
            switch (msgType)
            {
                case (byte)'P':
                    var len = await ReadInt32Async(reader) - sizeof(int);
                    var (statementName, statementLength) = await ReadNullTerminatedString(reader);
                    len -= statementLength;
                    var (query, queryLength) = await ReadNullTerminatedString(reader);
                    len -= queryLength;
                    var parametersCount = await ReadInt16Async(reader);
                    len -= sizeof(short);
                    var parameters = new int[parametersCount];
                    for (int i = 0; i < parametersCount; i++)
                    {
                        parameters[i] = await ReadInt32Async(reader);
                    }

                    var tsqlStatements = TSQLStatementReader.ParseStatements(query);
                    int offset = 1;
                    SelectField(tsqlStatements[0], ref offset);
                    if (len != 0)
                        throw new InvalidOperationException("Wrong size?");
                    return new Parse
                    {
                        Key = statementName,
                        Query = query,
                        Parameters = parameters
                    };
                default:
                    throw new NotSupportedException("Message type: " + (char) msgType);
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
    }

    public abstract class Message
    {
        
    }

    public class Parse : Message
    {
        public string Key;
        public string Query;
        public int[] Parameters;
    }
}
