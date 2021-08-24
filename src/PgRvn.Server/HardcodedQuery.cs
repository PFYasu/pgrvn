using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PgRvn.Server.Messages;
using PgRvn.Server.Npgsql;
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

            // TODO: Treat the results like any other data source and acknowledge the results format
            var resultsFormat = GetDefaultResultsFormat();

            // TODO: Find a way to support multiple statements - TSQL doesn't seem to provide each statement's string

            var normalizedQuery = QueryString.NormalizeLineEndings();
            if (normalizedQuery.Equals(PowerBIConfig.TypesQuery, StringComparison.OrdinalIgnoreCase))
            {
                _result = PowerBIConfig.TypesResponse;
                return;
            }

            // Npgsql & PowerBI send this
            // TODO: Figure out which versions of npgsql send this
            // 4.1.1
            if (normalizedQuery.StartsWith(NpgsqlConfig.CompositeTypesQuery, StringComparison.OrdinalIgnoreCase))
            {
                _result = PowerBIConfig.CompositeTypesResponse;
                return;
            }

            // Npgsql & PowerBI send this
            // 4.1.1, 4.0.0
            if (normalizedQuery.Equals(NpgsqlConfig.EnumTypesQuery, StringComparison.OrdinalIgnoreCase))
            {
                _result = PowerBIConfig.EnumTypesResponse;
                return;
            }

            if (normalizedQuery.StartsWith(PowerBIConfig.TableSchemaQuery, StringComparison.OrdinalIgnoreCase))
            {
                _result = PowerBIConfig.TableSchemaResponse;
                return;
            }

            if (normalizedQuery.StartsWith(PowerBIConfig.TableSchemaSecondaryQuery, StringComparison.OrdinalIgnoreCase))
            {
                _result = PowerBIConfig.TableSchemaSecondaryResponse;
                return;
            }

            if (normalizedQuery.StartsWith(PowerBIConfig.ConstraintsQuery, StringComparison.OrdinalIgnoreCase))
            {
                _result = PowerBIConfig.ConstraintsResponse;
                return;
            }

            if (normalizedQuery.Equals(PowerBIConfig.CharacterSetsQuery, StringComparison.OrdinalIgnoreCase))
            {
                _result = PowerBIConfig.CharacterSetsResponse;
                return;
            }

            if (normalizedQuery.Replace("\n", "").Equals(PgConfig.VersionQuery, StringComparison.OrdinalIgnoreCase))
            {
                _result = PgConfig.VersionResponse;
                return;
            }

            if (normalizedQuery.Equals(NpgsqlConfig.Npgsql5TypesQuery, StringComparison.OrdinalIgnoreCase))
            {
                _result = PgConfig.Npgsql5TypesResponse;
                return;
            }

            if (normalizedQuery.Equals(NpgsqlConfig.Npgsql5CompositeTypesQuery, StringComparison.OrdinalIgnoreCase))
            {
                _result = PgConfig.Npgsql5CompositeTypesResponse;
                return;
            }

            if (normalizedQuery.Equals(NpgsqlConfig.Npgsql5EnumTypesQuery, StringComparison.OrdinalIgnoreCase))
            {
                _result = PgConfig.Npgsql5EnumTypesResponse;
                return;
            }

            if (normalizedQuery.Equals(NpgsqlConfig.Npgsql4TypesQuery, StringComparison.OrdinalIgnoreCase))
            {
                _result = PgConfig.Npgsql4TypesResponse;
                return;
            }

            if (normalizedQuery.Equals(NpgsqlConfig.Npgsql4_1_2TypesQuery, StringComparison.OrdinalIgnoreCase))
            {
                _result = PgConfig.Npgsql4_1_2TypesResponse;
                return;
            }

            if (normalizedQuery.Equals(NpgsqlConfig.Npgsql4_0_0TypesQuery, StringComparison.OrdinalIgnoreCase))
            {
                _result = PgConfig.Npgsql4_0_0TypesResponse;
                return;
            }

            if (normalizedQuery.Equals(NpgsqlConfig.Npgsql4_0_0CompositeTypesQuery, StringComparison.OrdinalIgnoreCase))
            {
                _result = PgConfig.Npgsql4_0_0CompositeTypesResponse;
                return;
            }

            if (normalizedQuery.Equals(NpgsqlConfig.Npgsql3TypesQuery, StringComparison.OrdinalIgnoreCase))
            {
                _result = PgConfig.Npgsql3TypesResponse;
                return;
            }
        }

        public override async Task<ICollection<PgColumn>> Init(bool allowMultipleStatements = false)
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