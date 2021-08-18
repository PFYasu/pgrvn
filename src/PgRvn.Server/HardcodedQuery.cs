using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PgRvn.Server.Types;
using TSQL;


namespace PgRvn.Server
{
    public class HardcodedQuery : PgQuery
    {
        private PgTable _result;

        public HardcodedQuery(string queryString, int[] parametersDataTypes) : base(queryString, parametersDataTypes)
        {
        }

        public void Parse(bool allowMultipleStatements)
        {
            var sqlStatements = TSQLStatementReader.ParseStatements(QueryString);
            if (allowMultipleStatements == false && sqlStatements.Count != 1)
            {
                throw new InvalidOperationException("Didn't expect more than one SQL statement in queryString, got: " + sqlStatements.Count);
            }

            foreach (var stmt in sqlStatements)
            {
                var powerBiMatch = PowerBIConfig.TypesQuery;
                if (QueryString.Equals(powerBiMatch))
                {
                    _result = PowerBIConfig.TypesResponse;
                    return;
                }

                powerBiMatch = PowerBIConfig.CompositeTypesQuery;
                if (QueryString.Equals(powerBiMatch))
                {
                    _result = PowerBIConfig.CompositeTypesResponse;
                    return;
                }

                powerBiMatch = PowerBIConfig.EnumTypesQuery;

                if (QueryString.Equals(powerBiMatch))
                {
                    _result = PowerBIConfig.EnumTypesResponse;
                    return;
                }

                powerBiMatch = PowerBIConfig.TableSchemaQuery;
                if (QueryString.StartsWith(powerBiMatch))
                {
                    _result = PowerBIConfig.TableSchemaResponse;
                    return;
                }

                powerBiMatch = PowerBIConfig.TableSchemaSecondaryQuery;
                if (QueryString.StartsWith(powerBiMatch))
                {
                    _result = PowerBIConfig.TableSchemaSecondaryResponse;
                    return;
                }

                powerBiMatch = PowerBIConfig.ConstraintsQuery;
                if (QueryString.StartsWith(powerBiMatch))
                {
                    _result = PowerBIConfig.ConstraintsResponse;
                    return;
                }

                powerBiMatch = PowerBIConfig.VersionQuery;
                if (QueryString.Equals(powerBiMatch, StringComparison.OrdinalIgnoreCase))
                {
                    _result = PowerBIConfig.VersionResponse;
                    return;
                }

                // TODO: Treat the results like any other data source and acknowledge the results format
                var resultsFormat = GetDefaultResultsFormat();

                powerBiMatch = "select character_set_name from INFORMATION_SCHEMA.character_sets";
                if (QueryString.Equals(powerBiMatch))
                {
                    _result = new PgTable
                    {
                        Columns = new List<PgColumn>
                        {
                            new PgColumn("character_set_name", 0, PgName.Default, resultsFormat),
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

                    return;
                }
            }
        }

        public override async Task<ICollection<PgColumn>> Init(bool allowMultipleStatements)
        {
            if (IsEmptyQuery)
            {
                return default;
            }

            Parse(allowMultipleStatements);

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