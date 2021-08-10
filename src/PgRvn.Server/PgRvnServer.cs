using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Raven.Client.Documents;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace PgRvn.Server
{
    public class PgRvnServer
    {
        private readonly ConcurrentDictionary<TcpClient, Task> _connections = new();
        private Task _listenTask = Task.CompletedTask;
        private readonly CancellationTokenSource _cts = new();
        private TcpListener _tcpListener;
        private int _sessionIdentifier;
        private readonly int _processId;

        public PgRvnServer()
        {
            _processId = Process.GetCurrentProcess().Id;
        }

        public void Initialize()
        {
            _tcpListener = new TcpListener(IPAddress.Loopback, 5433);
            _tcpListener.Start();

            _listenTask = ListenToConnectionsAsync();
        }

        public void Shutdown()
        {
            _tcpListener.Stop();
            _cts.Cancel();
            foreach (var (_, task) in _connections)
            {
                task.Wait();
            }
        }

        private async Task ListenToConnectionsAsync()
        {
            while (_cts.IsCancellationRequested == false)
            {
                TcpClient client;
                try
                {
                    client = await _tcpListener.AcceptTcpClientAsync();
                }
                catch (Exception e)
                {
                    // TODO: error handling here
                    Console.WriteLine(e);
                    throw;
                }

                _connections.TryAdd(client, HandleConnection(client));
            }
        }

        public async Task HandleConnection(TcpClient client)
        {
            try
            {
                var session = new Session(client, _cts.Token, Interlocked.Increment(ref _sessionIdentifier),
                    _processId);
                await session.Run();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
