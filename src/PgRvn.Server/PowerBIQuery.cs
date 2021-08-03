using Raven.Client.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PgRvn.Server
{
    public class PowerBIQuery : RqlQuery //TODO: Is PowerBI query always RQL?
    {
        public PowerBIQuery(string queryString, int[] parametersDataTypes, IDocumentStore documentStore, int limit) 
            : base(queryString, parametersDataTypes, documentStore, limit)
        {
        }

        public static bool TryParse(string queryText, int[] parametersDataTypes, IDocumentStore documentStore, out PowerBIQuery powerBiQuery)
        {
            // Match RQL queries sent by PowerBI, the RQL is wrapped in an SQL statement (e.g. select * from ( from Orders ) "_" limit 0)
            var regexStr = @"(?i)^[ |\n|\r|\t]*(?:select[ |\n|\r|\t]+(?:.|\n|\r|\t| )*[ |\n|\r|\t]+from[ |\n|\r|\t]+\([ |\n|\r|\t]*)(?<rql>.*)[ |\n|\r|\t]*\)[ |\n|\r|\t]+""(?:\$Table|_)""[ |\n|\r|\t]+limit[ |\n|\r|\t]+(?<limit>[0-9]+)[ |\n|\r|\t]*$";
            var match = new Regex(regexStr).Match(queryText);

            if (match.Success == false)
            {
                powerBiQuery = null;
                return false;
            }

            var rql = match.Groups["rql"].Value;
            var limit = int.Parse(match.Groups["limit"].Value);

            // TODO: Consider returning RqlQuery instead, this class might be useless if no extra state/actions needs to be handled
            powerBiQuery = new PowerBIQuery(rql, parametersDataTypes, documentStore, limit);
            return true;
        }
    }
}
