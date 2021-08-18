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
    public class MessageBuilder : IDisposable
    {
        private IMemoryOwner<byte> _bufferOwner;
        private Memory<byte> Buffer => _bufferOwner.Memory;

        public MessageBuilder()
        {
            _bufferOwner = MemoryPool<byte>.Shared.Rent(32*1024); // TODO: need to manage sizes here
        }

        public ReadOnlyMemory<byte> ReadyForQuery(TransactionState transactionState)
        {
            int pos = 0;
            WriteByte((byte)MessageType.ReadyForQuery, Buffer.Span, ref pos);

            // Skip length
            int tempPos = pos;
            pos += sizeof(int);

            WriteByte((byte)transactionState, Buffer.Span, ref pos);

            // Write length
            WriteInt32(pos - sizeof(byte), Buffer.Span, ref tempPos);

            return Buffer[..pos];
        }

        public ReadOnlyMemory<byte> EmptyQueryResponse()
        {
            int pos = 0;

            WriteByte((byte)MessageType.EmptyQueryResponse, Buffer.Span, ref pos);
            WriteInt32(pos + sizeof(int) - sizeof(byte), Buffer.Span, ref pos);

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

        /// <summary>
        /// Creates an error response message.
        /// </summary>
        /// <param name="severity">A Postgres severity string. See <see cref="PgSeverity"/></param>
        /// <param name="errorCode">A Postgres error code (SqlState). See <see cref="PgErrorCodes"/></param>
        /// <param name="errorMessage">Error message</param>
        /// <param name="description">Error description</param>
        /// <remarks>
        /// More fields that aren't currently supported - https://www.postgresql.org/docs/current/protocol-error-fields.html
        /// </remarks>
        /// <returns>ErrorResponse message</returns>
        public ReadOnlyMemory<byte> ErrorResponse(string severity, string errorCode, string errorMessage, string description=null)
        {
            int pos = 0;
            WriteByte((byte)MessageType.ErrorResponse, Buffer.Span, ref pos);

            // Skip length
            int tempPos = pos;
            pos += sizeof(int);

            WriteByte((byte)PgErrorField.Severity, Buffer.Span, ref pos);
            WriteNullTerminatedString(severity, Buffer.Span, ref pos);

            WriteByte((byte)PgErrorField.SeverityNotLocalized, Buffer.Span, ref pos);
            WriteNullTerminatedString(severity, Buffer.Span, ref pos);

            WriteByte((byte)PgErrorField.SqlState, Buffer.Span, ref pos);
            WriteNullTerminatedString(errorCode, Buffer.Span, ref pos);

            WriteByte((byte)PgErrorField.Message, Buffer.Span, ref pos);
            WriteNullTerminatedString(errorMessage, Buffer.Span, ref pos);

            if (description != null)
            {
                WriteByte((byte)PgErrorField.Description, Buffer.Span, ref pos);
                WriteNullTerminatedString(description, Buffer.Span, ref pos);
            }

            WriteByte(0, Buffer.Span, ref pos);

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

        public ReadOnlyMemory<byte> CloseComplete()
        {
            int pos = 0;

            WriteByte((byte)MessageType.CloseComplete, Buffer.Span, ref pos);
            WriteInt32(pos + sizeof(int) - sizeof(byte), Buffer.Span, ref pos);

            return Buffer[..pos];
        }

        public ReadOnlyMemory<byte> CommandComplete(string tag)
        {
            int pos = 0;
            WriteByte((byte)MessageType.CommandComplete, Buffer.Span, ref pos);

            // Skip length
            int tempPos = pos;
            pos += sizeof(int);

            WriteNullTerminatedString(tag, Buffer.Span, ref pos);

            // Write length
            WriteInt32(pos - sizeof(byte), Buffer.Span, ref tempPos);

            return Buffer[..pos];
        }

        public ReadOnlyMemory<byte> DataRow(Span<ReadOnlyMemory<byte>?> columns)
        {
            var pos = DataRow(columns, Buffer.Span);
            return Buffer[..pos];
        }

        private int DataRow(Span<ReadOnlyMemory<byte>?> columns, Span<byte> buffer)
        {
            int pos = 0;
            WriteByte((byte)MessageType.DataRow, buffer, ref pos);

            // Skip length
            int tempPos = pos;
            pos += sizeof(int);

            WriteInt16((short)columns.Length, buffer, ref pos);

            foreach (var column in columns)
            {
                WriteInt32(column?.Length ?? -1, buffer, ref pos);
                WriteBytes(column?? ReadOnlyMemory<byte>.Empty, buffer, ref pos);
            }

            // Write length
            WriteInt32(pos - sizeof(byte), buffer, ref tempPos);

            return pos;
        }

        public ReadOnlyMemory<byte> RowDescription(ICollection<PgColumn> columns)
        {
            int pos = 0;
            WriteByte((byte)MessageType.RowDescription, Buffer.Span, ref pos);

            // Skip length
            int tempPos = pos;
            pos += sizeof(int);

            if (!ConvertToShort(columns.Count, out var columnsCount))
            {
                throw new InvalidCastException($"Columns list is too long to be contained in the message ({columnsCount}).");
            }

            WriteInt16(columnsCount, Buffer.Span, ref pos);

            foreach (var field in columns)
            {
                WriteNullTerminatedString(field.Name, Buffer.Span, ref pos);
                WriteInt32(field.TableObjectId, Buffer.Span, ref pos);
                WriteInt16(field.ColumnIndex, Buffer.Span, ref pos);
                WriteInt32(field.PgType.Oid, Buffer.Span, ref pos);
                WriteInt16(field.PgType.Size, Buffer.Span, ref pos);
                WriteInt32(field.TypeModifier, Buffer.Span, ref pos);
                WriteInt16((short)field.FormatCode, Buffer.Span, ref pos);
            }

            // Write length
            WriteInt32(pos - sizeof(byte), Buffer.Span, ref tempPos);

            return Buffer[..pos];
        }

        public ReadOnlyMemory<byte> NoData()
        {
            int pos = 0;

            WriteByte((byte)MessageType.NoData, Buffer.Span, ref pos);
            WriteInt32(pos + sizeof(int) - sizeof(byte), Buffer.Span, ref pos);

            return Buffer[..pos];
        }

        public ReadOnlyMemory<byte> ParameterDescription(IReadOnlyList<int> parametersDataTypeObjectIds)
        {
            int pos = 0;
            WriteByte((byte)MessageType.ParameterDescription, Buffer.Span, ref pos);

            // Skip length
            int tempPos = pos;
            pos += sizeof(int);

            if (!ConvertToShort(parametersDataTypeObjectIds.Count, out var paramCount))
            {
                throw new InvalidCastException($"Parameter data type list is too long to be contained " +
                                               $"in the message ({paramCount}).");
            }

            WriteInt16(paramCount, Buffer.Span, ref pos);

            foreach (var t in parametersDataTypeObjectIds)
            {
                WriteInt32(t, Buffer.Span, ref pos);
            }

            // Write length
            WriteInt32(pos - sizeof(byte), Buffer.Span, ref tempPos);

            return Buffer[..pos];
        }

        public ReadOnlyMemory<byte> SSLResponse(bool acceptSSL)
        {
            int pos = 0;

            WriteByte(acceptSSL ? (byte)'S' : (byte)'N', Buffer.Span, ref pos);

            return Buffer[..pos];
        }

        private bool ConvertToShort(int value, out short outVal)
        {
            if (value < short.MinValue || value > short.MaxValue)
            {
                outVal = 0;
                return false;
            }

            outVal = (short)value;
            return true;
        }

        private void WriteBytes(ReadOnlyMemory<byte> value, Span<byte> buffer, ref int pos)
        {
            value.Span.CopyTo(buffer[pos..]);
            pos += value.Length;
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
            // TODO: Work on this for every function
            if(pos +1 > buffer.Length)
            {
                var oldOwner = _bufferOwner;
                _bufferOwner = MemoryPool<byte>.Shared.Rent(buffer.Length * 2);
                var newBuf = _bufferOwner.Memory.Span;
                buffer.CopyTo(newBuf);
                buffer = newBuf;
                oldOwner.Dispose();
            }
            buffer[pos] = value;
            pos += sizeof(byte);
        }

        public void Dispose()
        {
            _bufferOwner?.Dispose();
        }
    }
}
