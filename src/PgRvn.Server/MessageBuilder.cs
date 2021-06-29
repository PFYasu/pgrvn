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

            // Skip length
            int tempPos = pos;
            pos += sizeof(int);

            // TODO: 'E' if in a failed transaction block
            WriteByte(insideTransaction ? (byte)'T' : (byte)'I', Buffer.Span, ref pos);

            // Write length
            WriteInt32(pos - sizeof(byte), Buffer.Span, ref tempPos);

            return Buffer[..pos];
        }

        public ReadOnlyMemory<byte> AuthenticationOk()
        {
            int pos = 0;
            WriteByte((byte)MessageType.AuthenticationOk, Buffer.Span, ref pos);

            // Skip length
            int tempPos = pos;
            pos += sizeof(int);

            WriteInt32(0, Buffer.Span, ref pos);

            // Write length
            WriteInt32(pos - sizeof(byte), Buffer.Span, ref tempPos);
            
            return Buffer[..pos];
        }


        public ReadOnlyMemory<byte> BackendKeyData(int processId, int sessionId)
        {
            int pos = 0;
            WriteByte((byte)MessageType.BackendKeyData, Buffer.Span, ref pos);

            // Skip length
            int tempPos = pos;
            pos += sizeof(int);

            WriteInt32(processId, Buffer.Span, ref pos);
            WriteInt32(sessionId, Buffer.Span, ref pos);

            // Write length
            WriteInt32(pos - sizeof(byte), Buffer.Span, ref tempPos);

            return Buffer[..pos];
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

            // Skip length
            int tempPos = pos;
            pos += sizeof(int);

            WriteNullTerminatedString(key, buffer, ref pos);
            WriteNullTerminatedString(value, buffer, ref pos);

            // Write length
            WriteInt32(pos - sizeof(byte), buffer, ref tempPos);

            return pos;
        }

        private int RowDescription(IReadOnlyList<PgField> fields, Span<byte> buffer)
        {
            // TODO: Make sure parametersDataTypeObjectIds length can be cast to short
            int pos = 0;
            WriteByte((byte)MessageType.RowDescription, buffer, ref pos);

            // Skip length
            int tempPos = pos;
            pos += sizeof(int);

            // TODO: Make sure fields.Count can be cast to short
            WriteInt16((short)fields.Count, buffer, ref pos);

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

            WriteInt32(pos - sizeof(byte), buffer, ref tempPos);
            return pos;
        }

        private int ParameterDescription(IReadOnlyList<int> parametersDataTypeObjectIds, Span<byte> buffer)
        {
            int pos = 0;
            WriteByte((byte)MessageType.ParameterDescription, buffer, ref pos);

            // Skip length
            int tempPos = pos;
            pos += sizeof(int);

            // TODO: Make sure parametersDataTypeObjectIds.Count can be cast to short
            WriteInt16((short)parametersDataTypeObjectIds.Count, buffer, ref pos);

            foreach (var t in parametersDataTypeObjectIds)
            {
                WriteInt32(t, buffer, ref pos);
            }

            WriteInt32(pos - 1, buffer, ref tempPos);

            return pos;
        }

        public ReadOnlyMemory<byte> ParseComplete()
        {
            int pos = 0;

            WriteByte((byte)MessageType.ParseComplete, Buffer.Span, ref pos);
            WriteInt32(pos + sizeof(int) - sizeof(byte), Buffer.Span, ref pos);

            return Buffer[..pos];
        }

        public ReadOnlyMemory<byte> BindComplete()
        {
            int pos = 0;

            WriteByte((byte)MessageType.BindComplete, Buffer.Span, ref pos);
            WriteInt32(pos + sizeof(int) - sizeof(byte), Buffer.Span, ref pos);

            return Buffer[..pos];
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

        public void Dispose()
        {
            _bufferOwner?.Dispose();
        }
    }
}
