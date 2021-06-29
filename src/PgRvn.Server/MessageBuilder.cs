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
            int pos = 0;
            WriteByte((byte)MessageType.ReadyForQuery, Buffer.Span, ref pos);

            // Skip length for now
            pos += 4;

            // TODO: 'E' if in a failed transaction block
            WriteByte(insideTransaction ? (byte)'T' : (byte)'I', Buffer.Span, ref pos);

            // Write length
            int tempPos = 1;
            WriteInt32(pos - 1, Buffer.Span, ref tempPos);

            return Buffer[..pos];
        }

        public ReadOnlyMemory<byte> AuthenticationOk()
        {
            const int messageLen = 9;
            Buffer.Span[0] = (byte)MessageType.AuthenticationOk;

            var payload = MemoryMarshal.Cast<byte, int>(Buffer.Span[1..]);
            payload[0] = IPAddress.HostToNetworkOrder(messageLen - 1);
            payload[1] = 0;

            return Buffer[..messageLen];
        }


        public ReadOnlyMemory<byte> BackendKeyData(int processId, int sessionId)
        {
            const int messageLen = 13;
            Buffer.Span[0] = (byte)MessageType.BackendKeyData;

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

        private int ParameterStatus(string key, string value, Span<byte> buffer)
        {
            int pos = 0;
            WriteByte((byte)MessageType.ParameterStatus, buffer, ref pos);

            // Skip length field for now
            pos += 4;

            WriteNullTerminatedString(key, buffer, ref pos);
            WriteNullTerminatedString(value, buffer, ref pos);

            // Write length that was skipped
            int tempPos = 1;
            WriteInt32(pos - 1, buffer, ref tempPos);

            return pos;
        }

        private void WriteNullTerminatedString(string value, Span<byte> buffer, ref int pos)
        {
            pos += Encoding.UTF8.GetBytes(value, buffer[pos..]);
            buffer[pos++] = 0; // null terminator
        }

        private void WriteInt32(int value, Span<byte> buffer, ref int pos)
        {
            var tableObjectIdPayload = MemoryMarshal.Cast<byte, int>(buffer[pos..]);
            tableObjectIdPayload[0] = IPAddress.HostToNetworkOrder(value);
            pos += sizeof(int);
        }

        private void WriteInt16(short value, Span<byte> buffer, ref int pos)
        {
            var tableObjectIdPayload = MemoryMarshal.Cast<byte, short>(buffer[pos..]);
            tableObjectIdPayload[0] = IPAddress.HostToNetworkOrder(value);
            pos += sizeof(short);
        }

        private void WriteByte(byte value, Span<byte> buffer, ref int pos)
        {
            buffer[pos] = value;
            pos += sizeof(byte);
        }

        private int RowDescription(IReadOnlyList<PgField> fields, Span<byte> buffer)
        {
            // TODO: Make sure parametersDataTypeObjectIds length can be cast to short

            buffer[0] = (byte)MessageType.RowDescription;

            int pos = 5;
            var fieldsPayload = MemoryMarshal.Cast<byte, short>(buffer[pos..]);
            fieldsPayload[0] = IPAddress.HostToNetworkOrder((short)fields.Count);
            pos += sizeof(short);

            foreach (var field in fields)
            {
                WriteNullTerminatedString(field.Name, buffer, ref pos);
                WriteInt32(field.TableObjectId, buffer, ref pos);
                WriteInt16(field.ColumnAttributeNumber, buffer, ref pos);
                WriteInt32(field.DataTypeObjectId, buffer, ref pos);
                WriteInt16(field.DataTypeSize, buffer, ref pos);
                WriteInt32(field.TypeModifier, buffer, ref pos);
                WriteInt16(field.FormatCode, buffer, ref pos);
            }

            var sizePayload = MemoryMarshal.Cast<byte, int>(buffer[1..]);
            sizePayload[0] = IPAddress.HostToNetworkOrder(pos - 1);

            return pos;
        }

        private int ParameterDescription(IReadOnlyList<int> parametersDataTypeObjectIds, Span<byte> buffer)
        {
            // TODO: Make sure parametersDataTypeObjectIds.Count can be cast to short

            buffer[0] = (byte)MessageType.ParameterDescription;

            int pos = 5;
            var paramPayload = MemoryMarshal.Cast<byte, short>(buffer[pos..]);
            paramPayload[0] = IPAddress.HostToNetworkOrder((short)parametersDataTypeObjectIds.Count);
            pos += sizeof(short);

            var objectIdPayload = MemoryMarshal.Cast<byte, int>(buffer[pos..]);

            for (var i = 0; i < parametersDataTypeObjectIds.Count; i++)
            {
                objectIdPayload[i] = IPAddress.HostToNetworkOrder(parametersDataTypeObjectIds[i]);
                pos += sizeof(int);
            }

            var sizePayload = MemoryMarshal.Cast<byte, int>(buffer[1..]);
            sizePayload[0] = IPAddress.HostToNetworkOrder(pos - 1);

            return pos;
        }

        public ReadOnlyMemory<byte> ParseComplete()
        {
            const int messageLen = 5;
            Buffer.Span[0] = (byte)MessageType.ParseComplete;

            var payload = MemoryMarshal.Cast<byte, int>(Buffer.Span[1..]);
            payload[0] = IPAddress.HostToNetworkOrder(messageLen - 1);

            return Buffer[..messageLen];
        }

        public ReadOnlyMemory<byte> BindComplete()
        {
            const int messageLen = 5;
            Buffer.Span[0] = (byte)MessageType.BindComplete;

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
