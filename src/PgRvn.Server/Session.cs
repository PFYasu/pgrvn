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
        private Memory<byte> _buffer;
        private int _processId;

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
            await using var stream = _client.GetStream();
            using var bufferOwner = MemoryPool<byte>.Shared.Rent(512);
            _buffer = bufferOwner.Memory;
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

            await writer.WriteAsync(AuthenticationOkMessage(), _token);
            await writer.WriteAsync(ParameterStatusMessages(new Dictionary<string, string>
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

            await writer.WriteAsync(BackendKeyData(), _token);

            while (_token.IsCancellationRequested ==false)
            {
                await writer.WriteAsync(ReadyForQuery(), _token);
                await ReadMessage(reader);
            }
        }

        private async Task ReadMessage(PipeReader reader)
        {
            var read = await reader.ReadAsync(_token);
            var msgType = (char)read.Buffer.First.Span[0];
        }

        private ReadOnlyMemory<byte> ReadyForQuery(bool insideTransaction = false)
        {
            const int messageLen = 6;
            _buffer.Span[0] = (byte)'Z';
            var payload = MemoryMarshal.Cast<byte, int>(_buffer.Span[1..]);
            payload[0] = IPAddress.HostToNetworkOrder(5);
            _buffer.Span[5] = insideTransaction ? (byte) 'T' : (byte) 'I'; // TODO: 'E' if in a failed transaction block
            return _buffer[..messageLen];
        }

        private ReadOnlyMemory<byte> AuthenticationOkMessage()
        {
            const int messageLen = 9;
            _buffer.Span[0] = (byte)'R';
            var payload = MemoryMarshal.Cast<byte, int>(_buffer.Span[1..]);
            payload[0]  = IPAddress.HostToNetworkOrder(8);
            payload[1] = 0;
            return _buffer[..messageLen];
        }

        private ReadOnlyMemory<byte> BackendKeyData()
        {
            const int messageLen = 13;
            _buffer.Span[0] = (byte)'K';
            var payload = MemoryMarshal.Cast<byte, int>(_buffer.Span[1..]);
            payload[0] = IPAddress.HostToNetworkOrder(messageLen - 1);
            payload[1] = IPAddress.HostToNetworkOrder(_processId);
            payload[2] = IPAddress.HostToNetworkOrder(_identifier);
            return _buffer[..messageLen];
        }

        private Memory<byte> ParameterStatusMessages(Dictionary<string, string> status)
        {
            int pos = 0;
            foreach (var (key, val) in status)
            {
                pos += ParameterStatus(key, val, _buffer.Span[pos..]);
            }

            return _buffer[..pos];
        }

        private Memory<byte> ParameterStatus(string key, string val)
        {
            _buffer.Span[0] = (byte)'S';
            int pos = 5;

            pos += Encoding.UTF8.GetBytes(key, _buffer.Span[pos..]);
            _buffer.Span[pos++] = 0; // null terminator

            pos += Encoding.UTF8.GetBytes(val, _buffer.Span[pos..]);
            _buffer.Span[pos++] = 0; // null terminator

            var asInts = MemoryMarshal.Cast<byte, int>(_buffer.Span[1..]);
            asInts[0] = IPAddress.NetworkToHostOrder(pos -1);
            return _buffer[..pos];
        }

        private int ParameterStatus(string key, string val, Span<byte> buffer)
        {
            buffer[0] = (byte)'S';
            int pos = 5;

            pos += Encoding.UTF8.GetBytes(key, buffer[pos..]);
            buffer[pos++] = 0; // null terminator

            pos += Encoding.UTF8.GetBytes(val, buffer[pos..]);
            buffer[pos++] = 0; // null terminator

            var asInts = MemoryMarshal.Cast<byte, int>(buffer[1..]);
            asInts[0] = IPAddress.NetworkToHostOrder(pos - 1);
            return pos;
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
