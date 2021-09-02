﻿using PgRvn.Server.Exceptions;
using PgRvn.Server.Messages;
using Raven.Client.Documents;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
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
        private Dictionary<string, string> _clientOptions;

        public Session(TcpClient client, CancellationToken token, int identifier, int processId)
        {
            _client = client;
            _token = token;
            _identifier = identifier;
            _processId = processId;
            _clientOptions = null;
        }

        private async Task HandleInitialMessage(Stream stream, PipeReader reader, PipeWriter writer, MessageBuilder messageBuilder)
        {
            var messageReader = new MessageReader();

            var initialMessage = await messageReader.ReadInitialMessage(reader, _token);
            if (initialMessage is SSLRequest)
            {
                await TryHandleTlsConnection(stream, writer, messageBuilder, _token);
                initialMessage = await messageReader.ReadInitialMessage(reader, _token);
            }

            switch (initialMessage)
            {
                case StartupMessage startupMessage:
                    _clientOptions = startupMessage.ClientOptions;
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
        }

        public async Task Run()
        {
            using var _ = _client;
            using var messageBuilder = new MessageBuilder();
            await using var stream = _client.GetStream();

            var reader = PipeReader.Create(stream);
            var writer = PipeWriter.Create(stream);

            await HandleInitialMessage(stream, reader, writer, messageBuilder);

            DocumentStore docStore;
            try
            {
                docStore = new DocumentStore
                {
                    Urls = new[] { "https://a.pgrvn.development.run" },
                    Database = _clientOptions["database"]
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
                using var transaction = new Transaction(docStore, new MessageReader());

                // Authentication
                await writer.WriteAsync(messageBuilder.AuthenticationCleartextPassword(), _token);
                var authMessage = await transaction.MessageReader.GetUninitializedMessage(reader, _token);
                await authMessage.Init(transaction.MessageReader, reader, _token);
                await authMessage.Handle(transaction, messageBuilder, reader, writer, _token);


                await writer.WriteAsync(messageBuilder.ParameterStatusMessages(PgConfig.ParameterStatusList), _token);
                await writer.WriteAsync(messageBuilder.BackendKeyData(_processId, _identifier), _token);
                await writer.WriteAsync(messageBuilder.ReadyForQuery(transaction.State), _token);

                while (_token.IsCancellationRequested == false)
                {
                    var message = await transaction.MessageReader.GetUninitializedMessage(reader, _token);

                    try
                    {
                        await message.Init(transaction.MessageReader, reader, _token);
                        await message.Handle(transaction, messageBuilder, reader, writer, _token);
                    }
                    catch (PgErrorException e)
                    {
                        await message.HandleError(e, transaction, messageBuilder, writer, _token);
                    }
                }
            }
            catch (PgFatalException e)
            {
                await writer.WriteAsync(messageBuilder.ErrorResponse(
                    PgSeverity.Fatal,
                    e.ErrorCode,
                    e.Message,
                    e.ToString()), _token);
            }
            catch (PgErrorException e)
            {
                // Shouldn't get to this point, PgErrorExceptions shouldn't be fatal
                await writer.WriteAsync(messageBuilder.ErrorResponse(
                    PgSeverity.Error,
                    e.ErrorCode,
                    e.Message,
                    e.ToString()), _token);
            }
            catch (PgTerminateReceivedException)
            {
                // Terminate silently
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
        
        private async Task TryHandleTlsConnection(Stream stream, PipeWriter writer, MessageBuilder builder, CancellationToken token)
        {
            // Refuse SSL
            await writer.WriteAsync(builder.SSLResponse(false), token);
            return;
            var sslStream = new SslStream(stream, false, (sender, certificate, chain, errors) =>
            {
                return true;
            }
            // it is fine that the client doesn't have a cert, we just care that they
            // are connecting to us securely. At any rate, we'll ensure that if certificate
            // is required, we'll validate that it is one of the expected ones on the server
            // and that the client is authorized to do so.
            // Otherwise, we'll generate an error, but we'll do that at a higher level then
            // SSL, because that generate a nicer error for the user to read then just aborted
            // connection because SSL negotiation failed.
            );
            const SslProtocols SupportedSslProtocols = SslProtocols.Tls13 | SslProtocols.Tls12;
            await sslStream.AuthenticateAsServerAsync(new SslServerAuthenticationOptions
            {
                ServerCertificate = new X509Certificate2(@"D:\Code\RavenDB2\Server\cluster.server.certificate.pgrvn.pfx"),
                ClientCertificateRequired = true,
                CertificateRevocationCheckMode = X509RevocationMode.NoCheck,
                EncryptionPolicy = EncryptionPolicy.RequireEncryption,
                EnabledSslProtocols = SupportedSslProtocols,
                CipherSuitesPolicy = null
            }, _token);

            // TODO: Establish SSL, respond with 'S' if willing to perform SSL or 'N' otherwise, etc.
        }
    }
}
