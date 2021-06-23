using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace PgRvn.Server
{
    public class PgRvnServer
    {
        public PgRvnServer()
        {
        }

        private readonly ConcurrentDictionary<TcpClient, Task> _connections = new();


        private Task _listenTask= Task.CompletedTask;
        private readonly CancellationTokenSource _cts = new();
        public void Initialize()
        {
            var tcpListener = new TcpListener(IPAddress.Loopback, 5433);
            tcpListener.Start();

            _listenTask = ListenToConnectionsAsync(tcpListener);
        }

        private async Task ListenToConnectionsAsync(TcpListener tcpListener)
        {
            while (Token.IsCancellationRequested == false)
            {
                //TODO: error handling here
                var client = await tcpListener.AcceptTcpClientAsync();
                _connections.TryAdd(client, HandleConnection(client));
            }
        }

        public CancellationToken Token => _cts.Token;

        public async Task HandleConnection(TcpClient client)
        {
            await using var stream = client.GetStream();
            var reader = PipeReader.Create(stream);
            var writer = PipeWriter.Create(stream);

            var msgLen = await ReadInt32Async(reader) - sizeof(int);
            var version = await ReadInt32Async(reader);

            // TODO: check protocol version
            // TODO: check TLS version match

            msgLen -= sizeof(int);
            var dic = new Dictionary<string, string>();
            while (msgLen > 0)
            {
                (var key, var keyLenInBytes) = await ReadNullTerminatedString(reader);
                msgLen -= keyLenInBytes;
                if (msgLen == 0)
                    break;
                (var val, var valLenInBytes) = await ReadNullTerminatedString(reader);
                dic[key] = val;
                msgLen -= valLenInBytes;
            }

            if (dic.TryGetValue("client_encoding", out var encoding) && encoding != "UTF8")
                throw new InvalidOperationException("Only UTF8 encoding is supported, but got: " + encoding);

            if(dic.TryGetValue("database", out var db) == false)
                throw new InvalidOperationException("The database wasn't specified, but is mandatory");




        }

        private async Task<(string String, int LengthInBytes)> ReadNullTerminatedString(PipeReader reader)
        {
            ReadResult read;
            SequencePosition? end;
            while(true)
            {
                read = await reader.ReadAsync(Token);
                end = read.Buffer.PositionOf((byte)0);
                if (end != null)
                    break;
                reader.AdvanceTo(read.Buffer.Start, read.Buffer.End);
            }

            var match = read.Buffer.Slice(0, end.Value);
            var result = Encoding.UTF8.GetString(match);
            reader.AdvanceTo(read.Buffer.GetPosition(1, end.Value));
            return (result, (int) match.Length + 1);
        }
        private async Task<ReadResult> ReadMinimumOf(PipeReader reader, int minSizeRequired)
        {
            var read = await reader.ReadAsync(Token);

            while (read.Buffer.Length < minSizeRequired)
            {
                reader.AdvanceTo(read.Buffer.Start, read.Buffer.End);
                read = await reader.ReadAsync(Token);
            }

            return read;
        }

        public async Task<int> ReadInt32Async(PipeReader reader)
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
