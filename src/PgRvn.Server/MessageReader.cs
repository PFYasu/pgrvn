// using System;
// using System.Collections.Generic;
// using System.IO.Pipelines;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
//
// namespace PgRvn.Server
// {
//     class MessageReader
//     {
//         public MessageReader()
//         {
//
//         }
//
//         public Message ReadMessage(PipeReader reader)
//         {
//             var msgType = await ReadCharAsync(reader);
//             var msgLen = await ReadInt32Async(reader) - sizeof(int);
//
//             switch (msgType)
//             {
//                 case (char)MessageType.Parse:
//
//                     var (statementName, statementLength) = await ReadNullTerminatedString(reader);
//                     msgLen -= statementLength;
//
//                     var (query, queryLength) = await ReadNullTerminatedString(reader);
//                     msgLen -= queryLength;
//
//                     var parametersCount = await ReadInt16Async(reader);
//                     msgLen -= sizeof(short);
//
//                     var parameters = new int[parametersCount];
//                     for (int i = 0; i < parametersCount; i++)
//                     {
//                         parameters[i] = await ReadInt32Async(reader);
//                     }
//
//                     // Parse SQL
//                     //var tsqlStatements = TSQLStatementReader.ParseStatements(query);
//                     //int offset = 1;
//                     //SelectField(tsqlStatements[0], ref offset);
//
//                     if (msgLen != 0)
//                         throw new InvalidOperationException("Wrong size?");
//
//                     return new Parse
//                     {
//                         StatementName = statementName,
//                         Query = query,
//                         ParametersDataTypeOid = parameters
//                     };
//
//                 case (char)MessageType.Bind:
//
//                     break;
//
//                 default:
//                     throw new NotSupportedException("Message type unsupported: " + (char)msgType);
//             }
//         }
//     }
// }
