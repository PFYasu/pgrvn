using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PgRvn.Server
{
    class MessageBuilder : IDisposable
    {
        private readonly IMemoryOwner<byte> _bufferOwner;
        private Memory<byte> Buffer => _bufferOwner.Memory;

        public MessageBuilder()
        {
            _bufferOwner = MemoryPool<byte>.Shared.Rent(512);
        }

        public ReadOnlyMemory<byte> ReadyForQuery(bool insideTransaction = false)
        {
            const int messageLen = 6;
            Buffer.Span[0] = (byte)'Z';

            var payload = MemoryMarshal.Cast<byte, int>(Buffer.Span[1..]);
            payload[0] = IPAddress.HostToNetworkOrder(messageLen - 1);

            Buffer.Span[5] = insideTransaction ? (byte)'T' : (byte)'I'; // TODO: 'E' if in a failed transaction block
            return Buffer[..messageLen];
        }

        public ReadOnlyMemory<byte> AuthenticationOk()
        {
            const int messageLen = 9;
            Buffer.Span[0] = (byte)'R';

            var payload = MemoryMarshal.Cast<byte, int>(Buffer.Span[1..]);
            payload[0] = IPAddress.HostToNetworkOrder(messageLen - 1);
            payload[1] = 0;

            return Buffer[..messageLen];
        }


        public ReadOnlyMemory<byte> BackendKeyData(int processId, int sessionId)
        {
            const int messageLen = 13;
            Buffer.Span[0] = (byte)'K';

            var payload = MemoryMarshal.Cast<byte, int>(Buffer.Span[1..]);
            payload[0] = IPAddress.HostToNetworkOrder(messageLen - 1);
            payload[1] = IPAddress.HostToNetworkOrder(processId);
            payload[2] = IPAddress.HostToNetworkOrder(sessionId);

            return Buffer[..messageLen];
        }

        public ReadOnlyMemory<byte> ParameterStatusMessages(Dictionary<string, string> status)
        {
            int pos = 0;
            foreach (var (key, val) in status)
            {
                pos += ParameterStatus(key, val, Buffer.Span[pos..]);
            }

            return Buffer[..pos];
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

        public ReadOnlyMemory<byte> ParseComplete()
        {
            const int messageLen = 5;
            Buffer.Span[0] = (byte)'1';

            var payload = MemoryMarshal.Cast<byte, int>(Buffer.Span[1..]);
            payload[0] = IPAddress.HostToNetworkOrder(messageLen - 1);

            return Buffer[..messageLen];
        }

        public void Dispose()
        {
            _bufferOwner?.Dispose();
        }
    }
}
