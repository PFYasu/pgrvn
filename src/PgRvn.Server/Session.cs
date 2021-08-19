using PgRvn.Server.Exceptions;
using PgRvn.Server.Messages;
using Raven.Client.Documents;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace PgRvn.Server
{

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
                await TryHandleTlsConnection(writer, messageBuilder, _token);
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
                case Cancel cancel:
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

            try
            {
                var transaction = new Transaction(docStore);

                await writer.WriteAsync(messageBuilder.AuthenticationOk(), _token);
                await writer.WriteAsync(messageBuilder.ParameterStatusMessages(PgConfig.ParameterStatusList), _token);
                await writer.WriteAsync(messageBuilder.BackendKeyData(_processId, _identifier), _token);
                await writer.WriteAsync(messageBuilder.ReadyForQuery(transaction.State), _token);

                while (_token.IsCancellationRequested == false)
                {
                    Message message = await messageReader.ReadMessage(reader, _token);
                    await message.Handle(transaction, messageBuilder, writer, _token);
                }
            }
            catch (PgErrorException e)
            {
                await writer.WriteAsync(messageBuilder.ErrorResponse(
                    PgSeverity.Error,
                    e.ErrorCode,
                    e.Message,
                    e.ToString()), _token);
            }
            catch (PgFatalException e)
            {
                await writer.WriteAsync(messageBuilder.ErrorResponse(
                    PgSeverity.Fatal,
                    e.ErrorCode,
                    e.Message,
                    e.ToString()), _token);
            }
            catch (PgTerminateReceivedException)
            {
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

        private async Task TryHandleTlsConnection(PipeWriter writer, MessageBuilder builder, CancellationToken token)
        {
            // Refuse SSL
            await writer.WriteAsync(builder.SSLResponse(false), token);

            // TODO: Establish SSL, respond with 'S' if willing to perform SSL or 'N' otherwise, etc.
        }
    }
}
