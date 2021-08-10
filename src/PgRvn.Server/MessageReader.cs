using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PgRvn.Server
{
    public class MessageReader
    {
        public async Task<Message> ReadInitialMessage(PipeReader reader, CancellationToken token)
        {
            var msgLen = await ReadInt32Async(reader, token) - sizeof(int);
            var protocolVersion = await ReadInt32Async(reader, token);

            return protocolVersion switch
            {
                (int) ProtocolVersion.CancelMessage => await Cancel(msgLen, reader, token),
                (int) ProtocolVersion.TlsConnection => new SSLRequest(),
                _ => await StartupMessage(protocolVersion, msgLen, reader, token)
            };
        }

        private async Task<StartupMessage> StartupMessage(int version, int msgLen, PipeReader reader, CancellationToken token)
        {
            if (version != (int)ProtocolVersion.Version3)
            {
                throw new PgFatalException(PgErrorCodes.ProtocolViolation, "Unsupported protocol version: " + version);
            }

            msgLen -= sizeof(int);
            var clientOptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            while (msgLen > 0)
            {
                var (key, keyLenInBytes) = await ReadNullTerminatedString(reader, token);
                msgLen -= keyLenInBytes;
                if (msgLen == 0)
                    break;
                var (val, valLenInBytes) = await ReadNullTerminatedString(reader, token);
                clientOptions[key] = val;
                msgLen -= valLenInBytes;
            }

            if (clientOptions.TryGetValue("client_encoding", out var encoding) && !encoding.Equals("UTF8", StringComparison.OrdinalIgnoreCase))
                throw new PgFatalException(PgErrorCodes.FeatureNotSupported,"Only UTF8 encoding is supported, but got: " + encoding);

            if (clientOptions.TryGetValue("database", out _) == false)
                throw new PgFatalException(PgErrorCodes.ConnectionException, "The database wasn't specified, but is mandatory");

            return new StartupMessage
            {
                ProtocolVersion = ProtocolVersion.Version3,
                ClientOptions = clientOptions
            };
        }

        private async Task<Cancel> Cancel(int msgLen, PipeReader reader, CancellationToken token)
        {
            // Length field
            msgLen -= sizeof(int);

            var processId = await ReadInt32Async(reader, token);
            msgLen -= sizeof(int);

            var sessionId = await ReadInt32Async(reader, token);
            msgLen -= sizeof(int);

            if (msgLen != 0)
            {
                throw new PgFatalException(PgErrorCodes.ProtocolViolation,
                    $"Message is bigger than specified in msgLen field - {msgLen} extra bytes in message.");
            }

            return new Cancel
            {
                ProcessId = processId,
                SessionId = sessionId
            };
        }

        public async Task<Message> ReadMessage(PipeReader reader, CancellationToken token)
        {
            var msgType = await ReadByteAsync(reader, token);
            var msgLen = await ReadInt32Async(reader, token) - sizeof(int);
            int bytesRead = 0;

            Message message;
            switch (msgType)
            {
                case (byte)MessageType.Parse:
                    (message, bytesRead) = await ReadParse(reader, token);
                    break;

                case (byte)MessageType.Bind:
                    (message, bytesRead) = await ReadBind(reader, token);
                    break;

                case (byte)MessageType.Describe:
                    (message, bytesRead) = await ReadDescribe(reader, token);
                    break;

                case (byte)MessageType.Execute:
                    (message, bytesRead) = await ReadExecute(reader, token);
                    break;

                case (byte)MessageType.Sync:
                    message = new Sync();
                    break;

                case (byte)MessageType.Terminate:
                    message = new Terminate();
                    break;

                case (byte)MessageType.Query:
                    (message, bytesRead) = await ReadQuery(reader, token);
                    break;
                case (byte)MessageType.Close:
                    (message, bytesRead) = await ReadClose(reader, token);
                    break;
                case (byte)MessageType.Flush:
                    message = new Flush();
                    break;
                default:
                    // TODO: Catch cases of messages that are real but *unsupported* and skip them if we can instead of fatally exiting
                    throw new PgFatalException(PgErrorCodes.ProtocolViolation, "Message type unrecognized: " + (char)msgType);
            }

            if (msgLen != bytesRead)
            {
                throw new PgFatalException(PgErrorCodes.ProtocolViolation,
                    $"Message is larger than specified in msgLen field, {msgLen} extra bytes in message.");
            }

            return message;
        }

        private async Task<(Parse, int)> ReadParse(PipeReader reader, CancellationToken token)
        {
            int len = 0;

            var (statementName, statementLength) = await ReadNullTerminatedString(reader, token);
            len += statementLength;

            var (query, queryLength) = await ReadNullTerminatedString(reader, token);
            len += queryLength;

            var parametersCount = await ReadInt16Async(reader, token);
            len += sizeof(short);

            var parameters = new int[parametersCount];
            for (int i = 0; i < parametersCount; i++)
            {
                parameters[i] = await ReadInt32Async(reader, token);
                len += sizeof(int);
            }

            return (new Parse
            {
                StatementName = statementName,
                Query = query,
                ParametersDataTypes = parameters
            }, len);
        }

        private async Task<(Bind, int)> ReadBind(PipeReader reader, CancellationToken token)
        {
            int len = 0;

            var (destPortalName, destPortalLength) = await ReadNullTerminatedString(reader, token);
            len += destPortalLength;

            var (preparedStatementName, preparedStatementLength) = await ReadNullTerminatedString(reader, token);
            len += preparedStatementLength;

            var parameterFormatCodeCount = await ReadInt16Async(reader, token);
            len += sizeof(short);

            var parameterCodes = new short[parameterFormatCodeCount];
            for (var i = 0; i < parameterFormatCodeCount; i++)
            {
                parameterCodes[i] = await ReadInt16Async(reader, token);
                len += sizeof(short);
            }

            var parametersCount = await ReadInt16Async(reader, token);
            len += sizeof(short);

            var parameters = new List<byte[]>(parametersCount);
            for (var i = 0; i < parametersCount; i++)
            {
                var parameterLength = await ReadInt32Async(reader, token);
                len += sizeof(int);

                // TODO: Is it okay to allocate up to 2GB of data based on external output?
                parameters.Add(await ReadBytesAsync(reader, parameterLength, token));
                len += parameterLength;
            }

            var resultColumnFormatCodesCount = await ReadInt16Async(reader, token);
            len += sizeof(short);

            var resultColumnFormatCodes = new short[resultColumnFormatCodesCount];
            for (var i = 0; i < resultColumnFormatCodesCount; i++)
            {
                resultColumnFormatCodes[i] = await ReadInt16Async(reader, token);
                len += sizeof(short);
            }

            return (new Bind
            {
                PortalName = destPortalName,
                StatementName = preparedStatementName,
                ParameterFormatCodes = parameterCodes,
                Parameters = parameters,
                ResultColumnFormatCodes = resultColumnFormatCodes
            }, len);
        }

        private async Task<(Describe, int)> ReadDescribe(PipeReader reader, CancellationToken token)
        {
            int len = 0;

            var describeObjectType = await ReadByteAsync(reader, token);
            len += sizeof(byte);

            var pgObjectType = describeObjectType switch
            {
                (byte)PgObjectType.Portal => PgObjectType.Portal,
                (byte)PgObjectType.PreparedStatement => PgObjectType.PreparedStatement,
                _ => throw new PgFatalException(PgErrorCodes.ProtocolViolation,
                    "Expected valid object type ('S' or 'P'), got: '" + describeObjectType)
            };

            var(describedName, describedNameLength) = await ReadNullTerminatedString(reader, token);
            len += describedNameLength;

            return (new Describe
            {
                PgObjectType = pgObjectType,
                ObjectName = describedName
            }, len);
        }

        private async Task<(Close, int)> ReadClose(PipeReader reader, CancellationToken token)
        {
            int len = 0;

            var objectType = await ReadByteAsync(reader, token);
            len += sizeof(byte);

            var pgObjectType = objectType switch
            {
                (byte)PgObjectType.Portal => PgObjectType.Portal,
                (byte)PgObjectType.PreparedStatement => PgObjectType.PreparedStatement,
                _ => throw new PgFatalException(PgErrorCodes.ProtocolViolation,
                    "Expected valid object type ('S' or 'P') but got: '" + objectType)
            };

            var (objectName, objectNameLength) = await ReadNullTerminatedString(reader, token);
            len += objectNameLength;

            return (new Close
            {
                PgObjectType = pgObjectType,
                ObjectName = objectName
            }, len);
        }

        private async Task<(Execute, int)> ReadExecute(PipeReader reader, CancellationToken token)
        {
            int len = 0;

            var (portalName, portalNameLength) = await ReadNullTerminatedString(reader, token);
            len += portalNameLength;

            var maxRowsToReturn = await ReadInt32Async(reader, token);
            len += sizeof(int);

            return (new Execute
            {
                PortalName = portalName,
                MaxRows = maxRowsToReturn
            }, len);
        }

        private async Task<(Query, int)> ReadQuery(PipeReader reader, CancellationToken token)
        {
            int len = 0;

            var (queryString, queryStringLength) = await ReadNullTerminatedString(reader, token);
            len += queryStringLength;

            return (new Query
            {
                QueryString = queryString
            }, len);
        }

        private async Task<(string String, int LengthInBytes)> ReadNullTerminatedString(PipeReader reader, CancellationToken token)
        {
            ReadResult read;
            SequencePosition? end;

            while (true)
            {
                read = await reader.ReadAsync(token);
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

        private async Task<ReadResult> ReadMinimumOf(PipeReader reader, int minSizeRequired, CancellationToken token)
        {
            var read = await reader.ReadAsync(token);

            while (read.Buffer.Length < minSizeRequired)
            {
                reader.AdvanceTo(read.Buffer.Start, read.Buffer.End);
                read = await reader.ReadAsync(token);
            }

            return read;
        }

        private async Task<byte[]> ReadBytesAsync(PipeReader reader, int length, CancellationToken token)
        {
            var read = await ReadMinimumOf(reader, length, token);
            return ReadBytes(read.Buffer, reader, length);
        }

        private async Task<int> ReadInt32Async(PipeReader reader, CancellationToken token)
        {
            var read = await ReadMinimumOf(reader, sizeof(int), token);
            return ReadInt32(read.Buffer, reader);
        }

        private async Task<short> ReadInt16Async(PipeReader reader, CancellationToken token)
        {
            var read = await ReadMinimumOf(reader, sizeof(int), token);
            return ReadInt16(read.Buffer, reader);
        }

        private async Task<byte> ReadByteAsync(PipeReader reader, CancellationToken token)
        {
            var read = await ReadMinimumOf(reader, sizeof(byte), token);
            return ReadByte(read.Buffer, reader);
        }

        private byte[] ReadBytes(ReadOnlySequence<byte> readBuffer, PipeReader reader, int length)
        {
            var sequence = readBuffer.Slice(0, length);
            var buffer = new byte[length];
            sequence.CopyTo(buffer);
            reader.AdvanceTo(sequence.End);
            return buffer;
        }

        private int ReadInt32(ReadOnlySequence<byte> readBuffer, PipeReader reader)
        {
            var sequence = readBuffer.Slice(0, sizeof(int));
            Span<byte> buffer = stackalloc byte[sizeof(int)];
            sequence.CopyTo(buffer);
            reader.AdvanceTo(sequence.End);
            return IPAddress.NetworkToHostOrder(MemoryMarshal.AsRef<int>(buffer));
        }

        private short ReadInt16(ReadOnlySequence<byte> readBuffer, PipeReader reader)
        {
            var sequence = readBuffer.Slice(0, sizeof(short));
            Span<byte> buffer = stackalloc byte[sizeof(short)];
            sequence.CopyTo(buffer);
            reader.AdvanceTo(sequence.End);
            return IPAddress.NetworkToHostOrder(MemoryMarshal.AsRef<short>(buffer));
        }
        private byte ReadByte(ReadOnlySequence<byte> readBuffer, PipeReader reader)
        {
            var charByte = readBuffer.First.Span[0];
            reader.AdvanceTo(readBuffer.GetPosition(sizeof(byte), readBuffer.Start));
            return charByte;
        }
    }
}
