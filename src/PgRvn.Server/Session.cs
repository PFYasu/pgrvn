using Raven.Client.Documents;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace PgRvn.Server
{
    public class PgFatalException : Exception
    {
        public string ErrorCode;
        public string Description;

        /// <summary>
        /// Creates an Postgres exception to be sent back to the client
        /// </summary>
        /// <param name="errorCode">A Postgres error code (SqlState). See <see cref="PgErrorCodes"/></param>
        /// <param name="errorMessage">Error message</param>
        /// <param name="description">Error description</param>
        /// <returns>ErrorResponse message</returns>
        public PgFatalException(string errorCode, string errorMessage, string description = null) : base(errorMessage)
        {
            ErrorCode = errorCode;
            Description = description;
        }
    }
    public class Session
    {
        private readonly TcpClient _client;
        private readonly CancellationToken _token;
        private readonly int _identifier;
        private readonly int _processId;

        public Session(TcpClient client, CancellationToken token, int identifier, int processId)
        {
            _client = client;
            _token = token;
            _identifier = identifier;
            _processId = processId;
        }

        public async Task Run()
        {
            using var _ = _client;
            using var messageBuilder = new MessageBuilder();
            var messageReader = new MessageReader();
            await using var stream = _client.GetStream();

            var reader = PipeReader.Create(stream);
            var writer = PipeWriter.Create(stream);

            var initialMessage = await messageReader.ReadInitialMessage(reader, _token);
            if (initialMessage is SSLRequest)
            {
                if (!await TryHandleTlsConnection())
                {
                    return;
                }
                initialMessage = await messageReader.ReadInitialMessage(reader, _token);
            }

            Dictionary<string, string> clientOptions;
            switch (initialMessage)
            {
                case StartupMessage startupMessage:
                    clientOptions = startupMessage.ClientOptions;
                    break;
                case SSLRequest:
                    await writer.WriteAsync(messageBuilder.ErrorResponse(
                        PgSeverity.Fatal, 
                        PgErrorCodes.ProtocolViolation, 
                        "SSLRequest received twice"), _token);
                    return;
                case Cancel:
                    // TODO: Support Cancel message
                    await writer.WriteAsync(messageBuilder.ErrorResponse(
                        PgSeverity.Fatal,
                        PgErrorCodes.FeatureNotSupported,
                        "Cancel message support not implemented."), _token);
                    return;
                default:
                    await writer.WriteAsync(messageBuilder.ErrorResponse(
                        PgSeverity.Fatal,
                        PgErrorCodes.ProtocolViolation,
                        "Invalid first message received"), _token);
                    return;
            }

            DocumentStore docStore;
            try
            {
                docStore = new DocumentStore
                {
                    Urls = new[] { "http://localhost:8080" },
                    Database = clientOptions["database"]
                };
                docStore.Initialize();
            }
            catch (Exception e)
            {
                await writer.WriteAsync(messageBuilder.ErrorResponse(
                    PgSeverity.Fatal,
                    PgErrorCodes.ConnectionFailure,
                    "Failed to connect to database",
                    e.Message), _token);
                return;
            }

            // TODO: Send a NoData message from Describe if no rows will be returned
            // TODO: Send a EmptyQueryResponse message from Execute if no rows were returned

            try
            {
                var transaction = new Transaction(docStore);

                await writer.WriteAsync(messageBuilder.AuthenticationOk(), _token);
                await writer.WriteAsync(messageBuilder.ParameterStatusMessages(PgConfig.ParameterStatusList), _token);
                await writer.WriteAsync(messageBuilder.BackendKeyData(_processId, _identifier), _token);
                await writer.WriteAsync(messageBuilder.ReadyForQuery(transaction.State), _token);

                while (_token.IsCancellationRequested == false)
                {
                    var message = await messageReader.ReadMessage(reader, _token);

                    if (message is Terminate)
                        break;

                    if (transaction.State == TransactionState.Failed && message is not Sync)
                        continue;

                    var response = message switch
                    {
                        Parse parse => transaction.Parse(parse, messageBuilder),
                        Bind bind => transaction.Bind(bind, messageBuilder),
                        Sync => transaction.Sync(messageBuilder),
                        Describe describe => await transaction.Describe(describe, messageBuilder, writer, _token),
                        Execute => await transaction.Execute(messageBuilder, writer, _token),
                        Query query => await transaction.Query(query, messageBuilder, writer, _token),
                        Close close => transaction.Close(close, messageBuilder),
                        Flush => await transaction.Flush(writer, _token),
                        _ => throw new PgFatalException(PgErrorCodes.ProtocolViolation, "Unrecognized message type")
                    };

                    await writer.WriteAsync(response, _token);
                }
            }
            catch (PgFatalException e)
            {
                await writer.WriteAsync(messageBuilder.ErrorResponse(
                    PgSeverity.Fatal,
                    e.ErrorCode,
                    e.Message,
                    e.Description), _token);
            }
            catch (Exception e)
            {
                try
                {
                    await writer.WriteAsync(messageBuilder.ErrorResponse(
                        PgSeverity.Fatal,
                        PgErrorCodes.InternalError,
                        e.ToString()), _token);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        private async Task<bool> TryHandleTlsConnection()
        {
            // TODO: Establish SSL, respond with 'S' if willing to perform SSL or 'N' otherwise, etc.
            return false;
        }
    }
}
