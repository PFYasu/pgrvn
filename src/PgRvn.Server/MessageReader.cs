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
    public enum ProtocolVersion
    {
        Version3 = 0x00030000,
        TlsConnection = 80877103,
        CancelMessage = 080877102
    }

    public class MessageReader
    {
        public async Task<Message> ReadInitialMessage(PipeReader reader, CancellationToken token)
        {
            var msgLen = await ReadInt32Async(reader, token) - sizeof(int);
            var protocolVersion = await ReadInt32Async(reader, token);

            return protocolVersion switch
            {
                (int) ProtocolVersion.CancelMessage => new Cancel(),
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
            var clientOptions = new Dictionary<string, string>();
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

            if (clientOptions.TryGetValue("client_encoding", out var encoding) && encoding != "UTF8")
                throw new PgFatalException(PgErrorCodes.FeatureNotSupported,"Only UTF8 encoding is supported, but got: " + encoding);

            if (clientOptions.TryGetValue("database", out _) == false)
                throw new PgFatalException(PgErrorCodes.ConnectionException, "The database wasn't specified, but is mandatory");

            return new StartupMessage
            {
                ProtocolVersion = ProtocolVersion.Version3,
                ClientOptions = clientOptions
            };
        }

        public async Task<Message> ReadMessage(PipeReader reader, CancellationToken token)
        {
            var msgType = await ReadByteAsync(reader, token);
            var msgLen = await ReadInt32Async(reader, token) - sizeof(int);

            Message message;
            switch (msgType)
            {
                case (byte)MessageType.Parse:
                    (message, msgLen) = await ReadParse(msgLen, reader, token);
                    break;

                case (byte)MessageType.Bind:
                    (message, msgLen) = await ReadBind(msgLen, reader, token);
                    break;

                case (byte)MessageType.Describe:
                    (message, msgLen) = await ReadDescribe(msgLen, reader, token);
                    break;

                case (byte)MessageType.Execute:
                    (message, msgLen) = await ReadExecute(msgLen, reader, token);
                    break;

                case (byte)MessageType.Sync:
                    message = new Sync();
                    break;

                case (byte)MessageType.Terminate:
                    message = new Terminate();
                    break;

                case (byte)MessageType.Query:
                    (message, msgLen) = await ReadQuery(msgLen, reader, token);
                    break;
                case (byte)MessageType.Close:
                    (message, msgLen) = await ReadClose(msgLen, reader, token);
                    break;
                case (byte)MessageType.Flush:
                    message = new Flush();
                    break;
                default:
                    // TODO: Catch cases of messages that are real but *unsupported* and skip them if we can instead of fatally exiting
                    throw new PgFatalException(PgErrorCodes.ProtocolViolation, "Message type unrecognized: " + (char)msgType);
            }

            if (msgLen != 0)
            {
                throw new PgFatalException(PgErrorCodes.ProtocolViolation,
                    $"Message is larger than specified in msgLen field, {msgLen} extra bytes in message.");
            }

            return message;
        }

        private async Task<(Parse, int)> ReadParse(int msgLen, PipeReader reader, CancellationToken token)
        {
            var (statementName, statementLength) = await ReadNullTerminatedString(reader, token);
            msgLen -= statementLength;

            var (query, queryLength) = await ReadNullTerminatedString(reader, token);
            msgLen -= queryLength;

            var parametersCount = await ReadInt16Async(reader, token);
            msgLen -= sizeof(short);

            var parameters = new int[parametersCount];
            for (int i = 0; i < parametersCount; i++)
            {
                parameters[i] = await ReadInt32Async(reader, token);
                msgLen -= sizeof(int);
            }

            return (new Parse
            {
                StatementName = statementName,
                Query = query,
                ParametersDataTypes = parameters
            }, msgLen);
        }

        private async Task<(Bind, int)> ReadBind(int msgLen, PipeReader reader, CancellationToken token)
        {
            var (destPortalName, destPortalLength) = await ReadNullTerminatedString(reader, token);
            msgLen -= destPortalLength;

            var (preparedStatementName, preparedStatementLength) = await ReadNullTerminatedString(reader, token);
            msgLen -= preparedStatementLength;

            var parameterFormatCodeCount = await ReadInt16Async(reader, token);
            msgLen -= sizeof(short);

            var parameterCodes = new short[parameterFormatCodeCount];
            for (var i = 0; i < parameterFormatCodeCount; i++)
            {
                parameterCodes[i] = await ReadInt16Async(reader, token);
                msgLen -= sizeof(short);
            }

            var parametersCount = await ReadInt16Async(reader, token);
            msgLen -= sizeof(short);

            var parameters = new List<byte[]>(parametersCount);
            for (var i = 0; i < parametersCount; i++)
            {
                var parameterLength = await ReadInt32Async(reader, token);
                msgLen -= sizeof(int);

                // TODO: Is it okay to allocate up to 2GB of data based on external output?
                // TODO: Format as bytes vs. text depending on the format codes given
                parameters.Add(await ReadBytesAsync(reader, parameterLength, token));
                msgLen -= parameterLength;
            }

            var resultColumnFormatCodesCount = await ReadInt16Async(reader, token);
            msgLen -= sizeof(short);

            var resultColumnFormatCodes = new short[resultColumnFormatCodesCount];
            for (var i = 0; i < resultColumnFormatCodesCount; i++)
            {
                resultColumnFormatCodes[i] = await ReadInt16Async(reader, token);
                msgLen -= sizeof(short);
            }

            return (new Bind
            {
                PortalName = destPortalName,
                StatementName = preparedStatementName,
                ParameterFormatCodes = parameterCodes,
                Parameters = parameters,
                ResultColumnFormatCodes = resultColumnFormatCodes
            }, msgLen);
        }

        private async Task<(Describe, int)> ReadDescribe(int msgLen, PipeReader reader, CancellationToken token)
        {
            var describeObjectType = await ReadByteAsync(reader, token);
            msgLen -= sizeof(byte);

            var pgObjectType = describeObjectType switch
            {
                (byte)PgObjectType.Portal => PgObjectType.Portal,
                (byte)PgObjectType.PreparedStatement => PgObjectType.PreparedStatement,
                _ => throw new PgFatalException(PgErrorCodes.ProtocolViolation,
                    "Expected valid object type ('S' or 'P'), got: '" + describeObjectType)
            };

            var(describedName, describedNameLength) = await ReadNullTerminatedString(reader, token);
            msgLen -= describedNameLength;

            return (new Describe
            {
                PgObjectType = pgObjectType,
                ObjectName = describedName
            }, msgLen);
        }

        private async Task<(Close, int)> ReadClose(int msgLen, PipeReader reader, CancellationToken token)
        {
            var objectType = await ReadByteAsync(reader, token);
            msgLen -= sizeof(byte);

            var pgObjectType = objectType switch
            {
                (byte)PgObjectType.Portal => PgObjectType.Portal,
                (byte)PgObjectType.PreparedStatement => PgObjectType.PreparedStatement,
                _ => throw new PgFatalException(PgErrorCodes.ProtocolViolation,
                    "Expected valid object type ('S' or 'P') but got: '" + objectType)
            };

            var (objectName, objectNameLength) = await ReadNullTerminatedString(reader, token);
            msgLen -= objectNameLength;

            return (new Close
            {
                PgObjectType = pgObjectType,
                ObjectName = objectName
            }, msgLen);
        }

        private async Task<(Execute, int)> ReadExecute(int msgLen, PipeReader reader, CancellationToken token)
        {
            var (portalName, portalNameLength) = await ReadNullTerminatedString(reader, token);
            msgLen -= portalNameLength;

            var maxRowsToReturn = await ReadInt32Async(reader, token);
            msgLen -= sizeof(int);

            return (new Execute
            {
                PortalName = portalName,
                MaxRows = maxRowsToReturn
            }, msgLen);
        }

        private async Task<(Query, int)> ReadQuery(int msgLen, PipeReader reader, CancellationToken token)
        {
            var (queryString, queryStringLength) = await ReadNullTerminatedString(reader, token);
            msgLen -= queryStringLength;

            return (new Query
            {
                QueryString = queryString
            }, msgLen);
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
            return ReadBytes(read, reader, length);
        }

        private async Task<int> ReadInt32Async(PipeReader reader, CancellationToken token)
        {
            var read = await ReadMinimumOf(reader, sizeof(int), token);
            return ReadInt32(read, reader);
        }

        private async Task<short> ReadInt16Async(PipeReader reader, CancellationToken token)
        {
            var read = await ReadMinimumOf(reader, sizeof(int), token);
            return ReadInt16(read, reader);
        }

        private async Task<byte> ReadByteAsync(PipeReader reader, CancellationToken token)
        {
            var read = await ReadMinimumOf(reader, sizeof(byte), token);
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
