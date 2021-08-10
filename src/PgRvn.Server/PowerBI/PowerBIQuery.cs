using Raven.Client.Documents;
using Raven.Client.Documents.Operations;
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
    public static class PowerBIQuery
    {
        public static bool TryParse(string queryText, int[] parametersDataTypes, IDocumentStore documentStore, out PgQuery pgQuery)
        {
            if (PBIFetchQuery.TryParse(queryText, parametersDataTypes, documentStore, out pgQuery))
            {
                return true;
            }

            if (PBIAllCollectionsQuery.TryParse(queryText, parametersDataTypes, documentStore, out pgQuery))
            {
                return true;
            }

            if (PBIPreviewQuery.TryParse(queryText, parametersDataTypes, documentStore, out pgQuery))
            {
                return true;
            }

            pgQuery = null;
            return false;
        }
    }
}
