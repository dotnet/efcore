// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.Data.SqlClient; // Note: Hard reference to SqlClient here.
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.SqlServer.Scaffolding.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SqlServerDatabaseModelFactory : DatabaseModelFactory
    {
        private readonly IDiagnosticsLogger<DbLoggerCategory.Scaffolding> _logger;

        private static readonly ISet<string> _dateTimePrecisionTypes = new HashSet<string>
        {
            "datetimeoffset",
            "datetime2",
            "time"
        };

        private static readonly ISet<string> _maxLengthRequiredTypes
            = new HashSet<string>
            {
                "binary",
                "varbinary",
                "char",
                "varchar",
                "nchar",
                "nvarchar"
            };

        private const string NamePartRegex
            = @"(?:(?:\[(?<part{0}>(?:(?:\]\])|[^\]])+)\])|(?<part{0}>[^\.\[\]]+))";

        private static readonly Regex _partExtractor
            = new Regex(
                string.Format(
                    CultureInfo.InvariantCulture,
                    @"^{0}(?:\.{1})?$",
                    string.Format(CultureInfo.InvariantCulture, NamePartRegex, 1),
                    string.Format(CultureInfo.InvariantCulture, NamePartRegex, 2)),
                RegexOptions.Compiled,
                TimeSpan.FromMilliseconds(1000));

        // see https://msdn.microsoft.com/en-us/library/ff878091.aspx
        // decimal/numeric are excluded because default value varies based on the precision.
        private static readonly Dictionary<string, long[]> _defaultSequenceMinMax = new Dictionary<string, long[]>(StringComparer.OrdinalIgnoreCase)
        {
            { "tinyint", new[] { 0L, 255L } },
            { "smallint", new[] { -32768L, 32767L } },
            { "int", new[] { -2147483648L, 2147483647L } },
            { "bigint", new[] { -9223372036854775808L, 9223372036854775807L } }
        };

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqlServerDatabaseModelFactory([NotNull] IDiagnosticsLogger<DbLoggerCategory.Scaffolding> logger)
        {
            Check.NotNull(logger, nameof(logger));

            _logger = logger;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override DatabaseModel Create(string connectionString, DatabaseModelFactoryOptions options)
        {
            Check.NotEmpty(connectionString, nameof(connectionString));
            Check.NotNull(options, nameof(options));

            using (var connection = new SqlConnection(connectionString))
            {
                return Create(connection, options);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override DatabaseModel Create(DbConnection connection, DatabaseModelFactoryOptions options)
        {
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(options, nameof(options));

            var databaseModel = new DatabaseModel();

            var connectionStartedOpen = connection.State == ConnectionState.Open;
            if (!connectionStartedOpen)
            {
                connection.Open();
            }

            try
            {
                databaseModel.DatabaseName = connection.Database;
                databaseModel.DefaultSchema = GetDefaultSchema(connection);

                var typeAliases = GetTypeAliases(connection);

                var schemaList = options.Schemas.ToList();
                var schemaFilter = GenerateSchemaFilter(schemaList);
                var tableList = options.Tables.ToList();
                var tableFilter = GenerateTableFilter(tableList.Select(Parse).ToList(), schemaFilter);

                if (SupportsSequences(connection))
                {
                    foreach (var sequence in GetSequences(connection, schemaFilter, typeAliases))
                    {
                        sequence.Database = databaseModel;
                        databaseModel.Sequences.Add(sequence);
                    }
                }

                foreach (var table in GetTables(connection, tableFilter, typeAliases))
                {
                    table.Database = databaseModel;
                    databaseModel.Tables.Add(table);
                }

                foreach (var schema in schemaList
                    .Except(
                        databaseModel.Sequences.Select(s => s.Schema)
                            .Concat(databaseModel.Tables.Select(t => t.Schema))))
                {
                    _logger.MissingSchemaWarning(schema);
                }

                foreach (var table in tableList)
                {
                    var (parsedSchema, parsedTableName) = Parse(table);
                    if (!databaseModel.Tables.Any(
                        t => !string.IsNullOrEmpty(parsedSchema)
                             && t.Schema == parsedSchema
                             || t.Name == parsedTableName))
                    {
                        _logger.MissingTableWarning(table);
                    }
                }

                return databaseModel;
            }
            finally
            {
                if (!connectionStartedOpen)
                {
                    connection.Close();
                }
            }
        }

        private string GetDefaultSchema(DbConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT SCHEMA_NAME()";

                if (command.ExecuteScalar() is string schema)
                {
                    _logger.DefaultSchemaFound(schema);

                    return schema;
                }

                return null;
            }
        }

        private static Func<string, string> GenerateSchemaFilter(IReadOnlyList<string> schemas)
        {
            return schemas.Count > 0
                ? (s =>
                {
                    var schemaFilterBuilder = new StringBuilder();
                    schemaFilterBuilder.Append(s);
                    schemaFilterBuilder.Append(" IN (");
                    schemaFilterBuilder.Append(string.Join(", ", schemas.Select(EscapeLiteral)));
                    schemaFilterBuilder.Append(")");
                    return schemaFilterBuilder.ToString();
                })
                : (Func<string, string>)null;
        }

        private static (string Schema, string Table) Parse(string table)
        {
            var match = _partExtractor.Match(table.Trim());

            if (!match.Success)
            {
                throw new InvalidOperationException(SqlServerStrings.InvalidTableToIncludeInScaffolding(table));
            }

            var part1 = match.Groups["part1"].Value.Replace("]]", "]");
            var part2 = match.Groups["part2"].Value.Replace("]]", "]");

            return string.IsNullOrEmpty(part2) ? (null, part1) : (part1, part2);
        }

        private static Func<string, string, string> GenerateTableFilter(
            IReadOnlyList<(string Schema, string Table)> tables,
            Func<string, string> schemaFilter)
        {
            return schemaFilter != null
                   || tables.Count > 0
                ? ((s, t) =>
                {
                    var tableFilterBuilder = new StringBuilder();

                    var openBracket = false;
                    if (schemaFilter != null)
                    {
                        tableFilterBuilder
                            .Append("(")
                            .Append(schemaFilter(s));
                        openBracket = true;
                    }

                    if (tables.Count > 0)
                    {
                        if (openBracket)
                        {
                            tableFilterBuilder
                                .AppendLine()
                                .Append("OR ");
                        }
                        else
                        {
                            tableFilterBuilder.Append("(");
                            openBracket = true;
                        }

                        var tablesWithoutSchema = tables.Where(e => string.IsNullOrEmpty(e.Schema)).ToList();
                        if (tablesWithoutSchema.Count > 0)
                        {
                            tableFilterBuilder.Append(t);
                            tableFilterBuilder.Append(" IN (");
                            tableFilterBuilder.Append(string.Join(", ", tablesWithoutSchema.Select(e => EscapeLiteral(e.Table))));
                            tableFilterBuilder.Append(")");
                        }

                        var tablesWithSchema = tables.Where(e => !string.IsNullOrEmpty(e.Schema)).ToList();
                        if (tablesWithSchema.Count > 0)
                        {
                            if (tablesWithoutSchema.Count > 0)
                            {
                                tableFilterBuilder.Append(" OR ");
                            }

                            tableFilterBuilder.Append(t);
                            tableFilterBuilder.Append(" IN (");
                            tableFilterBuilder.Append(string.Join(", ", tablesWithSchema.Select(e => EscapeLiteral(e.Table))));
                            tableFilterBuilder.Append(") AND (");
                            tableFilterBuilder.Append(s);
                            tableFilterBuilder.Append(" + N'.' + ");
                            tableFilterBuilder.Append(t);
                            tableFilterBuilder.Append(") IN (");
                            tableFilterBuilder.Append(string.Join(", ", tablesWithSchema.Select(e => EscapeLiteral($"{e.Schema}.{e.Table}"))));
                            tableFilterBuilder.Append(")");
                        }
                    }

                    if (openBracket)
                    {
                        tableFilterBuilder.Append(")");
                    }

                    return tableFilterBuilder.ToString();
                })
                : (Func<string, string, string>)null;
        }

        private static string EscapeLiteral(string s)
        {
            return $"N'{s}'";
        }

        private IReadOnlyDictionary<string, (string, string)> GetTypeAliases(DbConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                var typeAliasMap = new Dictionary<string, (string, string)>(StringComparer.OrdinalIgnoreCase);

                command.CommandText = @"
SELECT
    SCHEMA_NAME([t].[schema_id]) AS [schema_name],
    [t].[name] AS [type_name],
    [t2].[name] AS [underlying_system_type],
    CAST([t].[max_length] AS int) AS [max_length],
    CAST([t].[precision] AS int) AS [precision],
    CAST([t].[scale] AS int) AS [scale]
FROM [sys].[types] AS [t]
JOIN [sys].[types] AS [t2] ON [t].[system_type_id] = [t2].[user_type_id]
WHERE [t].[is_user_defined] = 1 OR [t].[system_type_id] <> [t].[user_type_id]";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var schema = reader.GetValueOrDefault<string>("schema_name");
                        var userType = reader.GetValueOrDefault<string>("type_name");
                        var systemType = reader.GetValueOrDefault<string>("underlying_system_type");
                        var maxLength = reader.GetValueOrDefault<int>("max_length");
                        var precision = reader.GetValueOrDefault<int>("precision");
                        var scale = reader.GetValueOrDefault<int>("scale");

                        var storeType = GetStoreType(systemType, maxLength, precision, scale);

                        _logger.TypeAliasFound(DisplayName(schema, userType), storeType);

                        typeAliasMap.Add($"[{schema}].[{userType}]", (storeType, systemType));
                    }
                }

                return typeAliasMap;
            }
        }

        private IEnumerable<DatabaseSequence> GetSequences(
            DbConnection connection,
            Func<string, string> schemaFilter,
            IReadOnlyDictionary<string, (string storeType, string)> typeAliases)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT
    OBJECT_SCHEMA_NAME([s].[object_id]) AS [schema_name],
    [s].[name],
    SCHEMA_NAME([t].[schema_id]) AS [type_schema],
    TYPE_NAME([s].[user_type_id]) AS [type_name],
    CAST([s].[precision] AS int) AS [precision],
    CAST([s].[scale] AS int) AS [scale],
    [s].[is_cycling],
    CAST([s].[increment] AS int) AS [increment],
    CAST([s].[start_value] AS bigint) AS [start_value],
    CAST([s].[minimum_value] AS bigint) AS [minimum_value],
    CAST([s].[maximum_value] AS bigint) AS [maximum_value]
FROM [sys].[sequences] AS [s]
JOIN [sys].[types] AS [t] ON [s].[user_type_id] = [t].[user_type_id]";

                if (schemaFilter != null)
                {
                    command.CommandText += @"
WHERE " + schemaFilter("OBJECT_SCHEMA_NAME([s].[object_id])");
                }

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var schema = reader.GetValueOrDefault<string>("schema_name");
                        var name = reader.GetValueOrDefault<string>("name");
                        var storeTypeSchema = reader.GetValueOrDefault<string>("type_schema");
                        var storeType = reader.GetValueOrDefault<string>("type_name");
                        var precision = reader.GetValueOrDefault<int>("precision");
                        var scale = reader.GetValueOrDefault<int>("scale");
                        var isCyclic = reader.GetValueOrDefault<bool>("is_cycling");
                        var incrementBy = reader.GetValueOrDefault<int>("increment");
                        var startValue = reader.GetValueOrDefault<long>("start_value");
                        var minValue = reader.GetValueOrDefault<long>("minimum_value");
                        var maxValue = reader.GetValueOrDefault<long>("maximum_value");

                        // Swap store type if type alias is used
                        if (typeAliases.TryGetValue($"[{storeTypeSchema}].[{storeType}]", out var value))
                        {
                            storeType = value.storeType;
                        }

                        storeType = GetStoreType(storeType, maxLength: 0, precision: precision, scale: scale);

                        _logger.SequenceFound(DisplayName(schema, name), storeType, isCyclic, incrementBy, startValue, minValue, maxValue);

                        var sequence = new DatabaseSequence
                        {
                            Schema = schema,
                            Name = name,
                            StoreType = storeType,
                            IsCyclic = isCyclic,
                            IncrementBy = incrementBy,
                            StartValue = startValue,
                            MinValue = minValue,
                            MaxValue = maxValue
                        };

                        if (_defaultSequenceMinMax.ContainsKey(storeType))
                        {
                            var defaultMin = _defaultSequenceMinMax[storeType][0];
                            sequence.MinValue = sequence.MinValue == defaultMin ? null : sequence.MinValue;
                            sequence.StartValue = sequence.StartValue == defaultMin ? null : sequence.StartValue;

                            sequence.MaxValue = sequence.MaxValue == _defaultSequenceMinMax[sequence.StoreType][1] ? null : sequence.MaxValue;
                        }

                        yield return sequence;
                    }
                }
            }
        }

        private IEnumerable<DatabaseTable> GetTables(
            DbConnection connection,
            Func<string, string, string> tableFilter,
            IReadOnlyDictionary<string, (string, string)> typeAliases)
        {
            using (var command = connection.CreateCommand())
            {
                var tables = new List<DatabaseTable>();

                var supportsMemoryOptimizedTable = SupportsMemoryOptimizedTable(connection);
                var supportsTemporalTable = SupportsTemporalTable(connection);

                var commandText = @"
SELECT
    SCHEMA_NAME([t].[schema_id]) AS [schema],
    [t].[name],
    CAST([e].[value] AS nvarchar(MAX)) AS [comment]";

                if (supportsMemoryOptimizedTable)
                {
                    commandText += @",
    [t].[is_memory_optimized]";
                }

                commandText += @"
FROM [sys].[tables] AS [t]
LEFT JOIN [sys].[extended_properties] AS [e] ON [e].[major_id] = [t].[object_id] AND [e].[minor_id] = 0 AND [e].[class] = 1 AND [e].[name] = 'MS_Description'";

                var filter = @"[t].[is_ms_shipped] = 0
AND NOT EXISTS (SELECT *
    FROM [sys].[extended_properties] AS [ep]
    WHERE [ep].[major_id] = [t].[object_id]
        AND [ep].[minor_id] = 0
        AND [ep].[class] = 1
        AND [ep].[name] = N'microsoft_database_tools_support'
    )
AND [t].[name] <> '" + HistoryRepository.DefaultTableName + "'";

                if (supportsTemporalTable)
                {
                    filter += @"
AND [t].[temporal_type] <> 1";
                }

                if (tableFilter != null)
                {
                    filter += @"
AND " + tableFilter("SCHEMA_NAME([t].[schema_id])", "[t].[name]");
                }

                commandText = commandText + @"
WHERE " + filter;

                var viewCommandText = @"
UNION
SELECT
    SCHEMA_NAME([v].[schema_id]) AS [schema],
    [v].[name],
    CAST([e].[value] AS nvarchar(MAX)) AS [comment]";

                if (supportsMemoryOptimizedTable)
                {
                    viewCommandText += @",
    CAST(0 AS bit) AS [is_memory_optimized]";
                }

                viewCommandText += @"
FROM [sys].[views] AS [v]
LEFT JOIN [sys].[extended_properties] AS [e] ON [e].[major_id] = [v].[object_id] AND [e].[minor_id] = 0 AND [e].[class] = 1 AND [e].[name] = 'MS_Description'";

                var viewFilter = @"[v].[is_ms_shipped] = 0
AND [v].[is_date_correlation_view] = 0 ";

                if (tableFilter != null)
                {
                    viewFilter += @"
AND " + tableFilter("SCHEMA_NAME([v].[schema_id])", "[v].[name]");
                }

                viewCommandText = viewCommandText + @"
WHERE " + viewFilter;

                command.CommandText = commandText + viewCommandText;

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var schema = reader.GetValueOrDefault<string>("schema");
                        var name = reader.GetValueOrDefault<string>("name");
                        var comment = reader.GetValueOrDefault<string>("comment");

                        _logger.TableFound(DisplayName(schema, name));

                        var table = new DatabaseTable
                        {
                            Schema = schema,
                            Name = name,
                            Comment = comment,
                        };

                        if (supportsMemoryOptimizedTable)
                        {
                            if (reader.GetValueOrDefault<bool>("is_memory_optimized"))
                            {
                                table[SqlServerAnnotationNames.MemoryOptimized] = true;
                            }
                        }

                        tables.Add(table);
                    }
                }

                // This is done separately due to MARS property may be turned off
                GetColumns(connection, tables, filter, viewFilter, typeAliases);
                GetIndexes(connection, tables, filter);
                GetForeignKeys(connection, tables, filter);

                return tables;
            }
        }

        private void GetColumns(
            DbConnection connection,
            IReadOnlyList<DatabaseTable> tables,
            string tableFilter,
            string viewFilter,
            IReadOnlyDictionary<string, (string storeType, string typeName)> typeAliases)
        {
            using (var command = connection.CreateCommand())
            {
                var commandText = @"
SELECT
    SCHEMA_NAME([o].[schema_id]) AS [table_schema],
    [o].[name] AS [table_name],
    [c].[name] AS [column_name],
    [c].[column_id] AS [ordinal],
    SCHEMA_NAME([tp].[schema_id]) AS [type_schema],
    [tp].[name] AS [type_name],
    CAST([c].[max_length] AS int) AS [max_length],
    CAST([c].[precision] AS int) AS [precision],
    CAST([c].[scale] AS int) AS [scale],
    [c].[is_nullable],
    [c].[is_identity],
    [dc].[definition] AS [default_sql],
    [cc].[definition] AS [computed_sql],
    CAST([e].[value] AS nvarchar(MAX)) AS [comment] 
FROM
(
    SELECT[v].[name], [v].[object_id], [v].[schema_id]
    FROM [sys].[views] v WHERE ";

                commandText += viewFilter;

                commandText += @"
UNION ALL
    SELECT [t].[name], [t].[object_id], [t].[schema_id]
    FROM [sys].[tables] t WHERE ";

                commandText += tableFilter;

                commandText += @"
) o
JOIN [sys].[columns] AS [c] ON [o].[object_id] = [c].[object_id]
JOIN [sys].[types] AS [tp] ON [c].[user_type_id] = [tp].[user_type_id]
LEFT JOIN [sys].[extended_properties] AS [e] ON [e].[major_id] = [o].[object_id] AND [e].[minor_id] = [c].[column_id] AND [e].[class] = 1 AND [e].[name] = 'MS_Description'
LEFT JOIN [sys].[computed_columns] AS [cc] ON [c].[object_id] = [cc].[object_id] AND [c].[column_id] = [cc].[column_id]
LEFT JOIN [sys].[default_constraints] AS [dc] ON [c].[object_id] = [dc].[parent_object_id] AND [c].[column_id] = [dc].[parent_column_id]";

                if (SupportsTemporalTable(connection))
                {
                    commandText += " WHERE [c].[is_hidden] = 0";
                }

                commandText += @"
ORDER BY [table_schema], [table_name], [c].[column_id]";

                command.CommandText = commandText;

                using (var reader = command.ExecuteReader())
                {
                    var tableColumnGroups = reader.Cast<DbDataRecord>()
                        .GroupBy(
                            ddr => (tableSchema: ddr.GetValueOrDefault<string>("table_schema"),
                                tableName: ddr.GetValueOrDefault<string>("table_name")));

                    foreach (var tableColumnGroup in tableColumnGroups)
                    {
                        var tableSchema = tableColumnGroup.Key.tableSchema;
                        var tableName = tableColumnGroup.Key.tableName;

                        var table = tables.Single(t => t.Schema == tableSchema && t.Name == tableName);

                        foreach (var dataRecord in tableColumnGroup)
                        {
                            var columnName = dataRecord.GetValueOrDefault<string>("column_name");
                            var ordinal = dataRecord.GetValueOrDefault<int>("ordinal");
                            var dataTypeSchemaName = dataRecord.GetValueOrDefault<string>("type_schema");
                            var dataTypeName = dataRecord.GetValueOrDefault<string>("type_name");
                            var maxLength = dataRecord.GetValueOrDefault<int>("max_length");
                            var precision = dataRecord.GetValueOrDefault<int>("precision");
                            var scale = dataRecord.GetValueOrDefault<int>("scale");
                            var nullable = dataRecord.GetValueOrDefault<bool>("is_nullable");
                            var isIdentity = dataRecord.GetValueOrDefault<bool>("is_identity");
                            var defaultValue = dataRecord.GetValueOrDefault<string>("default_sql");
                            var computedValue = dataRecord.GetValueOrDefault<string>("computed_sql");
                            var comment = dataRecord.GetValueOrDefault<string>("comment");

                            _logger.ColumnFound(
                                DisplayName(tableSchema, tableName),
                                columnName,
                                ordinal,
                                DisplayName(dataTypeSchemaName, dataTypeName),
                                maxLength,
                                precision,
                                scale,
                                nullable,
                                isIdentity,
                                defaultValue,
                                computedValue);

                            string storeType;
                            string systemTypeName;

                            // Swap store type if type alias is used
                            if (typeAliases.TryGetValue($"[{dataTypeSchemaName}].[{dataTypeName}]", out var value))
                            {
                                storeType = value.storeType;
                                systemTypeName = value.typeName;
                            }
                            else
                            {
                                storeType = GetStoreType(dataTypeName, maxLength, precision, scale);
                                systemTypeName = dataTypeName;
                            }

                            defaultValue = FilterClrDefaults(systemTypeName, nullable, defaultValue);

                            var column = new DatabaseColumn
                            {
                                Table = table,
                                Name = columnName,
                                StoreType = storeType,
                                IsNullable = nullable,
                                DefaultValueSql = defaultValue,
                                ComputedColumnSql = computedValue,
                                Comment = comment,
                                ValueGenerated = isIdentity
                                    ? ValueGenerated.OnAdd
                                    : storeType == "rowversion"
                                        ? ValueGenerated.OnAddOrUpdate
#pragma warning disable IDE0034 // Simplify 'default' expression - Ternary expression causes default(ValueGenerated) which is non-nullable
                                        : default(ValueGenerated?)
#pragma warning restore IDE0034 // Simplify 'default' expression
                            };

                            if (storeType == "rowversion")
                            {
                                // Note: annotation name must match `ScaffoldingAnnotationNames.ConcurrencyToken`
                                column["ConcurrencyToken"] = true;
                            }

                            table.Columns.Add(column);
                        }
                    }
                }
            }
        }

        private static string FilterClrDefaults(string dataTypeName, bool nullable, string defaultValue)
        {
            if (defaultValue == null
                || defaultValue == "(NULL)")
            {
                return null;
            }

            if (nullable)
            {
                return defaultValue;
            }

            if (defaultValue == "((0))")
            {
                if (dataTypeName == "bigint"
                    || dataTypeName == "bit"
                    || dataTypeName == "decimal"
                    || dataTypeName == "float"
                    || dataTypeName == "int"
                    || dataTypeName == "money"
                    || dataTypeName == "numeric"
                    || dataTypeName == "real"
                    || dataTypeName == "smallint"
                    || dataTypeName == "smallmoney"
                    || dataTypeName == "tinyint")
                {
                    return null;
                }
            }
            else if (defaultValue == "((0.0))")
            {
                if (dataTypeName == "decimal"
                    || dataTypeName == "float"
                    || dataTypeName == "money"
                    || dataTypeName == "numeric"
                    || dataTypeName == "real"
                    || dataTypeName == "smallmoney")
                {
                    return null;
                }
            }
            else if ((defaultValue == "(CONVERT([real],(0)))" && dataTypeName == "real")
                     || (defaultValue == "((0.0000000000000000e+000))" && dataTypeName == "float")
                     || (defaultValue == "('0001-01-01')" && dataTypeName == "date")
                     || (defaultValue == "('1900-01-01T00:00:00.000')" && (dataTypeName == "datetime" || dataTypeName == "smalldatetime"))
                     || (defaultValue == "('0001-01-01T00:00:00.000')" && dataTypeName == "datetime2")
                     || (defaultValue == "('0001-01-01T00:00:00.000+00:00')" && dataTypeName == "datetimeoffset")
                     || (defaultValue == "('00:00:00')" && dataTypeName == "time")
                     || (defaultValue == "('00000000-0000-0000-0000-000000000000')" && dataTypeName == "uniqueidentifier"))
            {
                return null;
            }

            return defaultValue;
        }

        private static string GetStoreType(string dataTypeName, int maxLength, int precision, int scale)
        {
            if (dataTypeName == "timestamp")
            {
                return "rowversion";
            }

            if (dataTypeName == "decimal"
                || dataTypeName == "numeric")
            {
                return $"{dataTypeName}({precision}, {scale})";
            }

            if (_dateTimePrecisionTypes.Contains(dataTypeName)
                && scale != 7)
            {
                return $"{dataTypeName}({scale})";
            }

            if (_maxLengthRequiredTypes.Contains(dataTypeName))
            {
                if (maxLength == -1)
                {
                    return $"{dataTypeName}(max)";
                }

                if (dataTypeName == "nvarchar"
                    || dataTypeName == "nchar")
                {
                    maxLength /= 2;
                }

                return $"{dataTypeName}({maxLength})";
            }

            return dataTypeName;
        }

        private void GetIndexes(DbConnection connection, IReadOnlyList<DatabaseTable> tables, string tableFilter)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT
    SCHEMA_NAME([t].[schema_id]) AS [table_schema],
    [t].[name] AS [table_name],
    [i].[name] AS [index_name],
    [i].[type_desc],
    [i].[is_primary_key],
    [i].[is_unique_constraint],
    [i].[is_unique],
    [i].[has_filter],
    [i].[filter_definition],
    COL_NAME([ic].[object_id], [ic].[column_id]) AS [column_name]
FROM [sys].[indexes] AS [i]
JOIN [sys].[tables] AS [t] ON [i].[object_id] = [t].[object_id]
JOIN [sys].[index_columns] AS [ic] ON [i].[object_id] = [ic].[object_id] AND [i].[index_id] = [ic].[index_id]
WHERE " + tableFilter + @"
ORDER BY [table_schema], [table_name], [index_name], [ic].[key_ordinal]";

                using (var reader = command.ExecuteReader())
                {
                    var tableIndexGroups = reader.Cast<DbDataRecord>()
                        .GroupBy(
                            ddr => (tableSchema: ddr.GetValueOrDefault<string>("table_schema"),
                                tableName: ddr.GetValueOrDefault<string>("table_name")));

                    foreach (var tableIndexGroup in tableIndexGroups)
                    {
                        var tableSchema = tableIndexGroup.Key.tableSchema;
                        var tableName = tableIndexGroup.Key.tableName;

                        var table = tables.Single(t => t.Schema == tableSchema && t.Name == tableName);

                        var primaryKeyGroups = tableIndexGroup
                            .Where(ddr => ddr.GetValueOrDefault<bool>("is_primary_key"))
                            .GroupBy(
                                ddr =>
                                    (Name: ddr.GetValueOrDefault<string>("index_name"),
                                        TypeDesc: ddr.GetValueOrDefault<string>("type_desc")))
                            .ToArray();

                        if (primaryKeyGroups.Length == 1)
                        {
                            var primaryKeyGroup = primaryKeyGroups[0];

                            _logger.PrimaryKeyFound(primaryKeyGroup.Key.Name, DisplayName(tableSchema, tableName));

                            var primaryKey = new DatabasePrimaryKey
                            {
                                Table = table,
                                Name = primaryKeyGroup.Key.Name
                            };

                            if (primaryKeyGroup.Key.TypeDesc == "NONCLUSTERED")
                            {
                                primaryKey[SqlServerAnnotationNames.Clustered] = false;
                            }

                            foreach (var dataRecord in primaryKeyGroup)
                            {
                                var columnName = dataRecord.GetValueOrDefault<string>("column_name");
                                var column = table.Columns.FirstOrDefault(c => c.Name == columnName)
                                             ?? table.Columns.FirstOrDefault(c => c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
                                Debug.Assert(column != null, "column is null.");

                                primaryKey.Columns.Add(column);
                            }

                            table.PrimaryKey = primaryKey;
                        }

                        var uniqueConstraintGroups = tableIndexGroup
                            .Where(ddr => ddr.GetValueOrDefault<bool>("is_unique_constraint"))
                            .GroupBy(
                                ddr =>
                                    (Name: ddr.GetValueOrDefault<string>("index_name"),
                                        TypeDesc: ddr.GetValueOrDefault<string>("type_desc")))
                            .ToArray();

                        foreach (var uniqueConstraintGroup in uniqueConstraintGroups)
                        {
                            _logger.UniqueConstraintFound(uniqueConstraintGroup.Key.Name, DisplayName(tableSchema, tableName));

                            var uniqueConstraint = new DatabaseUniqueConstraint
                            {
                                Table = table,
                                Name = uniqueConstraintGroup.Key.Name
                            };

                            if (uniqueConstraintGroup.Key.TypeDesc == "CLUSTERED")
                            {
                                uniqueConstraint[SqlServerAnnotationNames.Clustered] = true;
                            }

                            foreach (var dataRecord in uniqueConstraintGroup)
                            {
                                var columnName = dataRecord.GetValueOrDefault<string>("column_name");
                                var column = table.Columns.FirstOrDefault(c => c.Name == columnName)
                                             ?? table.Columns.FirstOrDefault(c => c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
                                Debug.Assert(column != null, "column is null.");

                                uniqueConstraint.Columns.Add(column);
                            }

                            table.UniqueConstraints.Add(uniqueConstraint);
                        }

                        var indexGroups = tableIndexGroup
                            .Where(
                                ddr => !ddr.GetValueOrDefault<bool>("is_primary_key")
                                       && !ddr.GetValueOrDefault<bool>("is_unique_constraint"))
                            .GroupBy(
                                ddr =>
                                    (Name: ddr.GetValueOrDefault<string>("index_name"),
                                        TypeDesc: ddr.GetValueOrDefault<string>("type_desc"),
                                        IsUnique: ddr.GetValueOrDefault<bool>("is_unique"),
                                        HasFilter: ddr.GetValueOrDefault<bool>("has_filter"),
                                        FilterDefinition: ddr.GetValueOrDefault<string>("filter_definition")))
                            .ToArray();

                        foreach (var indexGroup in indexGroups)
                        {
                            _logger.IndexFound(indexGroup.Key.Name, DisplayName(tableSchema, tableName), indexGroup.Key.IsUnique);

                            var index = new DatabaseIndex
                            {
                                Table = table,
                                Name = indexGroup.Key.Name,
                                IsUnique = indexGroup.Key.IsUnique,
                                Filter = indexGroup.Key.HasFilter ? indexGroup.Key.FilterDefinition : null
                            };

                            if (indexGroup.Key.TypeDesc == "CLUSTERED")
                            {
                                index[SqlServerAnnotationNames.Clustered] = true;
                            }

                            foreach (var dataRecord in indexGroup)
                            {
                                var columnName = dataRecord.GetValueOrDefault<string>("column_name");
                                var column = table.Columns.FirstOrDefault(c => c.Name == columnName)
                                             ?? table.Columns.FirstOrDefault(c => c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
                                Debug.Assert(column != null, "column is null.");

                                index.Columns.Add(column);
                            }

                            table.Indexes.Add(index);
                        }
                    }
                }
            }
        }

        private void GetForeignKeys(DbConnection connection, IReadOnlyList<DatabaseTable> tables, string tableFilter)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT
    SCHEMA_NAME([t].[schema_id]) AS [table_schema],
    [t].[name] AS [table_name],
    [f].[name],
    OBJECT_SCHEMA_NAME([f].[referenced_object_id]) AS [principal_table_schema],
    OBJECT_NAME([f].[referenced_object_id]) AS [principal_table_name],
    [f].[delete_referential_action_desc],
    col_name([fc].[parent_object_id], [fc].[parent_column_id]) AS [column_name],
    col_name([fc].[referenced_object_id], [fc].[referenced_column_id]) AS [referenced_column_name]
FROM [sys].[foreign_keys] AS [f]
JOIN [sys].[tables] AS [t] ON [f].[parent_object_id] = [t].[object_id]
JOIN [sys].[foreign_key_columns] AS [fc] ON [f].[object_id] = [fc].[constraint_object_id]
WHERE " + tableFilter + @"
ORDER BY [table_schema], [table_name], [f].[name], [fc].[constraint_column_id]";

                using (var reader = command.ExecuteReader())
                {
                    var tableForeignKeyGroups = reader.Cast<DbDataRecord>()
                        .GroupBy(
                            ddr => (tableSchema: ddr.GetValueOrDefault<string>("table_schema"),
                                tableName: ddr.GetValueOrDefault<string>("table_name")));

                    foreach (var tableForeignKeyGroup in tableForeignKeyGroups)
                    {
                        var tableSchema = tableForeignKeyGroup.Key.tableSchema;
                        var tableName = tableForeignKeyGroup.Key.tableName;

                        var table = tables.Single(t => t.Schema == tableSchema && t.Name == tableName);

                        var foreignKeyGroups = tableForeignKeyGroup
                            .GroupBy(
                                c => (Name: c.GetValueOrDefault<string>("name"),
                                    PrincipalTableSchema: c.GetValueOrDefault<string>("principal_table_schema"),
                                    PrincipalTableName: c.GetValueOrDefault<string>("principal_table_name"),
                                    OnDeleteAction: c.GetValueOrDefault<string>("delete_referential_action_desc")));

                        foreach (var foreignKeyGroup in foreignKeyGroups)
                        {
                            var fkName = foreignKeyGroup.Key.Name;
                            var principalTableSchema = foreignKeyGroup.Key.PrincipalTableSchema;
                            var principalTableName = foreignKeyGroup.Key.PrincipalTableName;
                            var onDeleteAction = foreignKeyGroup.Key.OnDeleteAction;

                            _logger.ForeignKeyFound(
                                fkName,
                                DisplayName(table.Schema, table.Name),
                                DisplayName(principalTableSchema, principalTableName),
                                onDeleteAction);

                            var principalTable = tables.FirstOrDefault(
                                                     t => t.Schema == principalTableSchema
                                                          && t.Name == principalTableName)
                                                 ?? tables.FirstOrDefault(
                                                     t => t.Schema.Equals(principalTableSchema, StringComparison.OrdinalIgnoreCase)
                                                          && t.Name.Equals(principalTableName, StringComparison.OrdinalIgnoreCase));

                            if (principalTable == null)
                            {
                                _logger.ForeignKeyReferencesMissingPrincipalTableWarning(
                                    fkName,
                                    DisplayName(table.Schema, table.Name),
                                    DisplayName(principalTableSchema, principalTableName));

                                continue;
                            }

                            var foreignKey = new DatabaseForeignKey
                            {
                                Name = fkName,
                                Table = table,
                                PrincipalTable = principalTable,
                                OnDelete = ConvertToReferentialAction(onDeleteAction)
                            };

                            var invalid = false;

                            foreach (var dataRecord in foreignKeyGroup)
                            {
                                var columnName = dataRecord.GetValueOrDefault<string>("column_name");
                                var column = table.Columns.FirstOrDefault(c => c.Name == columnName)
                                             ?? table.Columns.FirstOrDefault(c => c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
                                Debug.Assert(column != null, "column is null.");

                                var principalColumnName = dataRecord.GetValueOrDefault<string>("referenced_column_name");
                                var principalColumn = foreignKey.PrincipalTable.Columns.FirstOrDefault(c => c.Name == principalColumnName)
                                                      ?? foreignKey.PrincipalTable.Columns.FirstOrDefault(c => c.Name.Equals(principalColumnName, StringComparison.OrdinalIgnoreCase));
                                if (principalColumn == null)
                                {
                                    invalid = true;
                                    _logger.ForeignKeyPrincipalColumnMissingWarning(
                                        fkName,
                                        DisplayName(table.Schema, table.Name),
                                        principalColumnName,
                                        DisplayName(principalTableSchema, principalTableName));
                                    break;
                                }

                                foreignKey.Columns.Add(column);
                                foreignKey.PrincipalColumns.Add(principalColumn);
                            }

                            if (!invalid)
                            {
                                if (foreignKey.Columns.SequenceEqual(foreignKey.PrincipalColumns))
                                {
                                    _logger.ReflexiveConstraintIgnored(
                                        foreignKey.Name,
                                        DisplayName(table.Schema, table.Name));
                                }
                                else
                                {
                                    table.ForeignKeys.Add(foreignKey);
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool SupportsTemporalTable(DbConnection connection)
        {
            return CompatibilityLevel(connection) >= 130 && EngineEdition(connection) != 6;
        }

        private bool SupportsMemoryOptimizedTable(DbConnection connection)
        {
            return CompatibilityLevel(connection) >= 120 && EngineEdition(connection) != 6;
        }

        private bool SupportsSequences(DbConnection connection)
        {
            return CompatibilityLevel(connection) >= 110 && EngineEdition(connection) != 6;
        }

        private int EngineEdition(DbConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT SERVERPROPERTY('EngineEdition');";
                return (int)command.ExecuteScalar();
            }
        }

        private byte CompatibilityLevel(DbConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = $@"
SELECT compatibility_level
FROM sys.databases
WHERE name = '{connection.Database}';";

                var result = command.ExecuteScalar();
                return result != null ? Convert.ToByte(result) : (byte)0;
            }
        }

        private static string DisplayName(string schema, string name)
            => (!string.IsNullOrEmpty(schema) ? schema + "." : "") + name;

        private static ReferentialAction? ConvertToReferentialAction(string onDeleteAction)
        {
            switch (onDeleteAction)
            {
                case "NO_ACTION":
                    return ReferentialAction.NoAction;

                case "CASCADE":
                    return ReferentialAction.Cascade;

                case "SET_NULL":
                    return ReferentialAction.SetNull;

                case "SET_DEFAULT":
                    return ReferentialAction.SetDefault;

                default:
                    return null;
            }
        }
    }
}
