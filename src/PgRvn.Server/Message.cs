using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PgRvn.Server.Types;

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
        Terminate = (byte)'X',
        Query = (byte)'Q',
        Close = (byte)'C',
        Flush = (byte)'H',

        // Sent
        ParameterStatus = (byte)'S',
        BackendKeyData = (byte)'K',
        AuthenticationOk = (byte)'R',
        ReadyForQuery = (byte)'Z',
        ErrorResponse = (byte)'E',

        ParseComplete = (byte)'1',
        BindComplete = (byte)'2',
        CloseComplete = (byte)'3',
        ParameterDescription = (byte)'t',
        RowDescription = (byte)'T',
        NoData = (byte)'n',
        DataRow = (byte)'D',
        CommandComplete = (byte)'C',
        EmptyQueryResponse = (byte)'I',
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
        FileName = (byte)'F',
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
        public PgType PgType;
        public PgFormat FormatCode;

        public PgColumn(string name, short columnIndex, PgType pgType, PgFormat formatCode, int tableOid = 0)
        {
            Name = name;
            TableObjectId = tableOid;
            ColumnIndex = columnIndex;
            PgType = pgType;
            FormatCode = formatCode;
        }
    }

    public class PgTable
    {
        public List<PgColumn> Columns;
        public List<PgDataRow> Data;
    }

    public class PgDataRow
    {
        public Memory<ReadOnlyMemory<byte>?> ColumnData;
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
        /// Object ID number of parameter data types specified (can be zero).
        /// </summary>
        /// <remarks>
        /// Note that this is not an indication of the number of parameters that might appear
        /// in the query string, only the number that the frontend wants to prespecify types for.
        /// </remarks>
        public int[] ParametersDataTypes;
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

    public class Terminate : Message
    {
    }

    public class StartupMessage : Message
    {
        public ProtocolVersion ProtocolVersion;
        public Dictionary<string, string> ClientOptions;
    }

    public class Cancel : Message
    {
        public int ProcessId;
        public int SessionId;
    }

    public class SSLRequest : Message
    {
    }

    public class Query : Message
    {
        public string QueryString;
    }

    public class Close : Message
    {
        public PgObjectType PgObjectType;
        public string ObjectName;
    }

    public class Flush : Message
    {
    }
}
