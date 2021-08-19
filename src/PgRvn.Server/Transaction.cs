using PgRvn.Server.Messages;
using PgRvn.Server.Types;
using Raven.Client.Documents;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PgRvn.Server
{
    public enum TransactionState : byte
    {
        Idle = (byte)'I',
        InTransaction = (byte)'T',
        Failed = (byte)'E'
    }

    public class Transaction
    {
        public TransactionState State { get; private set; } = TransactionState.Idle;
        public IDocumentStore DocumentStore { get; }

        private PgQuery _currentQuery;
        
        public Transaction(IDocumentStore documentStore)
        {
            DocumentStore = documentStore;
        }

        public void Init(string cleanQueryText, int[] parametersDataTypes)
        {
            _currentQuery?.Dispose();
            _currentQuery = PgQuery.CreateInstance(cleanQueryText, parametersDataTypes, DocumentStore);
            State = TransactionState.InTransaction;
        }

        public void Bind(ICollection<byte[]> parameters, short[] parameterFormatCodes, short[] resultColumnFormatCodes)
        {
            _currentQuery.Bind(parameters, parameterFormatCodes, resultColumnFormatCodes);
        }

        public async Task<(ICollection<PgColumn> schema, int[] parameterDataTypes)> Describe()
        {
            return (await _currentQuery.Init(), _currentQuery.ParametersDataTypes);
        }

        public async Task Execute(MessageBuilder messageBuilder, PipeWriter writer, CancellationToken token)
        {
            await _currentQuery.Execute(messageBuilder, writer, token);
            // TODO: Remove this comment once confirmed working - State = TransactionState.Idle;
        }

        public void Fail()
        {
            State = TransactionState.Failed;
        }

        public void Close()
        {
            State = TransactionState.Idle;
            _currentQuery?.Dispose();
            _currentQuery = null;
        }

        public void Sync()
        {
            State = TransactionState.Idle;
            _currentQuery?.Dispose();
            _currentQuery = null;
        }
    }
}
