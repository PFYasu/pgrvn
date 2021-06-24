using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
            TlsConnection = 0x4d2162f
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
            else
            {
                await HandleHandshake(version, msgLen, reader);
            }

            await writer.WriteAsync(messageBuilder.AuthenticationOkMessage(), _token);
            await writer.WriteAsync(messageBuilder.ParameterStatusMessages(new Dictionary<string, string>
            {
                ["client_encoding"] =  "UTF8",
                ["server_encoding"] =  "UTF8",
                ["server_version"] = "13.3", // TODO
                ["application_name"] = "",
                ["DataStyle"] = "ISO, DMY",
                ["integer_datetimes"] = "on",
                ["IntervalStyle"] = "postgres",
                ["is_superuser"] = "on", // TODO
                ["session_authorization"] = "postgres",
                ["standard_conforming_strings"] = "on",
                ["TimeZone"] = "Asia/Jerusalem", // TODO
            }), _token);

            await writer.WriteAsync(messageBuilder.BackendKeyData(_processId, _identifier), _token);

            while (_token.IsCancellationRequested ==false)
            {
                await writer.WriteAsync(messageBuilder.ReadyForQuery(), _token);
                await ReadMessage(reader);
            }
        }

        private async Task ReadMessage(PipeReader reader)
        {
            var read = await reader.ReadAsync(_token);
            var len = read.Buffer.Length;
            var msgType = (char)read.Buffer.First.Span[0];
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

        private static int ReadInt32(ReadResult read, PipeReader reader)
        {
            var sequence = read.Buffer.Slice(0, sizeof(int));
            Span<byte> buffer = stackalloc byte[sizeof(int)];
            sequence.CopyTo(buffer);
            reader.AdvanceTo(sequence.End);
            var len = IPAddress.NetworkToHostOrder(MemoryMarshal.AsRef<int>(buffer));
            return len;
        }
    }
}
