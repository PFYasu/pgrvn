using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace PgRvn.Server
{
    public class HardcodedQuery : PgQuery
    {
        private PgTable _result;

        public HardcodedQuery(string queryString, int[] parametersDataTypes) : base(queryString, parametersDataTypes)
        {
        }

        // TODO: Make this the only constructor, add a TryParse method and rename this class - HardcodedQuery ? Maybe not.
        public HardcodedQuery(string queryString, int[] parametersDataTypes, PgTable results) : base(queryString, parametersDataTypes)
        {
            _result = results;
        }

        public bool Parse(bool allowMultipleStatements)
        {
            // TODO: Make this better, shouldn't have separate logic for previously given result..
            if (_result != null)
            {
                return true;
            }

            var sqlStatements = TSQLStatementReader.ParseStatements(QueryString);
            if (allowMultipleStatements == false && sqlStatements.Count != 1)
            {
                throw new InvalidOperationException("Didn't expect more than one SQL statement in queryString, got: " + sqlStatements.Count);
            }

            foreach (var stmt in sqlStatements)
            {
                var powerBIMatch = PowerBIConfig.TypesQuery;
                if (QueryString.Equals(powerBIMatch))
                {
                    _result = PowerBIConfig.TypesResponse;
                    return true;
                }

                powerBIMatch = PowerBIConfig.CompositeTypesQuery;

                if (QueryString.Equals(powerBIMatch))
                {
                    _result = PowerBIConfig.CompositeTypesResponse;
                    return true;
                }

                powerBIMatch = PowerBIConfig.EnumTypesQuery;

                if (QueryString.Equals(powerBIMatch))
                {
                    _result = PowerBIConfig.EnumTypesResponse;
                    return true;
                }

                powerBIMatch = PowerBIConfig.TableSchemaQuery;
                if (QueryString.StartsWith(powerBIMatch))
                {
                    _result = PowerBIConfig.TableSchemaResponse;
                }

                powerBIMatch = PowerBIConfig.TableSchemaSecondaryQuery;
                if (QueryString.StartsWith(powerBIMatch))
                {
                    _result = PowerBIConfig.TableSchemaSecondaryResponse;
                }

                powerBIMatch = PowerBIConfig.ConstraintsQuery;
                if (QueryString.StartsWith(powerBIMatch))
                {
                    _result = PowerBIConfig.ConstraintsResponse;
                }


                powerBIMatch = PowerBIConfig.VersionQuery;
                if (QueryString.Equals(powerBIMatch, StringComparison.OrdinalIgnoreCase))
                {
                    _result = PowerBIConfig.VersionResponse;
                }

                // TODO: Treat the results like any other data source and acknowledge the results format
                var resultsFormat = GetDefaultResultsFormat();

                powerBIMatch = "select character_set_name from INFORMATION_SCHEMA.character_sets";
                if (QueryString.Equals(powerBIMatch))
                {
                    _result = new PgTable
                    {
                        Columns = new List<PgColumn>
                        {
                            new PgColumn("character_set_name", 0, PgTypeOIDs.Name, -1, resultsFormat),
                        },
                        Data = new List<PgDataRow>
                        {
                            new()
                            {
                                ColumnData = new ReadOnlyMemory<byte>?[]
                                {
                                    Encoding.UTF8.GetBytes("UTF8"),
                                }
                            },
                        }
                    };

                    return true;
                }
            }

            return true;
        }

        public override async Task<ICollection<PgColumn>> Init(bool allowMultipleStatements)
        {
            if (IsEmptyQuery)
            {
                return default;
            }

            Parse(allowMultipleStatements); // todo: handle error (return value)

            if (_result != null)
            {
                return _result.Columns;
            }

            return Array.Empty<PgColumn>();
        }

        public override async Task Execute(MessageBuilder builder, PipeWriter writer, CancellationToken token)
        {
            if (_result?.Data != null)
            {
                foreach (var dataRow in _result.Data)
                {
                    await writer.WriteAsync(builder.DataRow(dataRow.ColumnData.Span), token);
                }
            }

            await writer.WriteAsync(builder.CommandComplete($"SELECT {_result?.Data?.Count ?? 0}"), token);
        }

        public override void Dispose()
        {
        }
    }
}