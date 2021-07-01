using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PgRvn.Server
{
    public enum MessageType : byte
    {
        // Received
        Parse = (byte)'P',
        Bind = (byte)'B',
        Describe = (byte)'D',
        Execute = (byte)'E',
        Sync = (byte)'S',

        // Sent
        ParameterStatus = (byte)'S',
        BackendKeyData = (byte)'K',
        AuthenticationOk = (byte)'R',
        ReadyForQuery = (byte)'Z',
        ErrorResponse = (byte)'E',

        ParseComplete = (byte)'1',
        BindComplete = (byte)'2',
        ParameterDescription = (byte)'t',
        RowDescription = (byte)'T',
        DataRow = (byte)'D',
        CommandComplete = (byte)'C',
    }

    public enum PgObjectType : byte
    {
        PreparedStatement = (byte)'S',
        Portal = (byte)'P'
    }

    /// <remarks>
    /// See <see href="https://www.postgresql.org/docs/current/protocol-error-fields.html"/>
    /// </remarks>
    public enum PgErrorField : byte
    {
        Severity = (byte)'S',
        SeverityNotLocalized = (byte)'V',
        SqlState = (byte)'C',
        Message = (byte)'M',
        Description = (byte)'D',
        Hint = (byte)'H',
        Position = (byte)'P',
        PositionInternal = (byte)'p',
        QueryInternal = (byte)'q',
        Where = (byte)'W',
        SchemaName = (byte)'s',
        TableName = (byte)'t',
        ColumnName = (byte)'c',
        DataTypeName = (byte)'d',
        ConstraintName = (byte)'n',
        FileName = (byte)'f',
        Line = (byte)'L',
        Routine = (byte)'R'
    }

    public class PgColumn
    {
        public string Name;
        /// <summary>
        /// If the field can be identified as a column of a specific table, the object ID of the table; otherwise zero.
        /// </summary>
        public int TableObjectId;
        /// <summary>
        /// If the field can be identified as a column of a specific table, the attribute number of the column; otherwise zero.
        /// </summary>
        public short ColumnIndex;
        public int TypeObjectId;
        public short DataTypeSize;
        public int TypeModifier;
        public PgFormat FormatCode;
    }

    public class PgColumnData
    {
        public static readonly PgColumnData Null = new PgColumnData {IsNull = true, Data = ReadOnlyMemory<byte>.Empty};
        public bool IsNull = false;
        public ReadOnlyMemory<byte> Data;
    }

    public enum PgFormat : short
    {
        Text = 0,
        Binary = 1
    }

    public class PgSeverity
    {
        // In ErrorResponse messages
        public const string Error = "ERROR";
        public const string Fatal = "FATAL";
        public const string Panic = "PANIC";

        // In NoticeResponse messages
        public const string Warning = "WARNING";
        public const string Notice = "NOTICE";
        public const string Debug = "DEBUG";
        public const string Info = "INFO";
        public const string Log = "LOG";
    }

    public abstract class Message
    {
    }

    public class Parse : Message
    {
        public string StatementName;
        public string Query;

        /// <summary>
        /// The number of parameter data types specified (can be zero).
        /// </summary>
        /// <remarks>
        /// Note that this is not an indication of the number of parameters that might appear
        /// in the query string, only the number that the frontend wants to prespecify types for.
        /// </remarks>
        public int[] ParametersDataTypeOid;
    }

    public class Bind : Message
    {
        public string PortalName;
        public string StatementName;
        public short[] ParameterFormatCodes;
        public List<byte[]> Parameters;
        public short[] ResultColumnFormatCodes;
    }

    public class Describe : Message
    {

        /// <summary>
        /// Type of Postgres object to describe (Portal/Statement)
        /// </summary>
        public PgObjectType PgObjectType;
        public string ObjectName;
    }

    public class Execute : Message
    {
        public string PortalName;
        public int MaxRows;
    }

    public class Sync : Message
    {
    }
}
