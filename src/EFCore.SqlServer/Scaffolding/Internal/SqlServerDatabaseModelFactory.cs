// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.SqlServer.Extensions.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.Scaffolding.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerDatabaseModelFactory : DatabaseModelFactory
{
    private readonly IDiagnosticsLogger<DbLoggerCategory.Scaffolding> _logger;
    private readonly IRelationalTypeMappingSource _typeMappingSource;

    private static readonly ISet<string> DateTimePrecisionTypes = new HashSet<string>
    {
        "datetimeoffset",
        "datetime2",
        "time"
    };

    private static readonly ISet<string> MaxLengthRequiredTypes
        = new HashSet<string>
        {
            "binary",
            "varbinary",
            "char",
            "varchar",
            "nchar",
            "nvarchar"
        };

    private enum EngineEdition
    {
        SqlDataWarehouse = 6,
        SqlOnDemand = 11,
        DynamicsTdsEndpoint = 1000,
    }

    private const string NamePartRegex
        = @"(?:(?:\[(?<part{0}>(?:(?:\]\])|[^\]])+)\])|(?<part{0}>[^\.\[\]]+))";

    private static readonly Regex PartExtractor
        = new(
            string.Format(
                CultureInfo.InvariantCulture,
                @"^{0}(?:\.{1})?$",
                string.Format(CultureInfo.InvariantCulture, NamePartRegex, 1),
                string.Format(CultureInfo.InvariantCulture, NamePartRegex, 2)),
            RegexOptions.Compiled,
            TimeSpan.FromMilliseconds(1000));

    // see https://msdn.microsoft.com/en-us/library/ff878091.aspx
    // decimal/numeric are excluded because default value varies based on the precision.
    private static readonly Dictionary<string, long[]> DefaultSequenceMinMax =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { "tinyint", [0L, 255L] },
            { "smallint", [-32768L, 32767L] },
            { "int", [-2147483648L, 2147483647L] },
            { "bigint", [-9223372036854775808L, 9223372036854775807L] }
        };

    private byte? _compatibilityLevel;
    private EngineEdition? _engineEdition;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerDatabaseModelFactory(
        IDiagnosticsLogger<DbLoggerCategory.Scaffolding> logger,
        IRelationalTypeMappingSource typeMappingSource)
    {
        _logger = logger;
        _typeMappingSource = typeMappingSource;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override DatabaseModel Create(string connectionString, DatabaseModelFactoryOptions options)
    {
        using var connection = new SqlConnection(connectionString);
        return Create(connection, options);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override DatabaseModel Create(DbConnection connection, DatabaseModelFactoryOptions options)
    {
        var databaseModel = new DatabaseModel();

        var connectionStartedOpen = connection.State == ConnectionState.Open;
        if (!connectionStartedOpen)
        {
            connection.Open();
        }

        try
        {
            CheckViewDefinitionRights(connection);

            _compatibilityLevel = GetCompatibilityLevel(connection);
            _engineEdition = GetEngineEdition(connection);

            databaseModel.DatabaseName = connection.Database;
            databaseModel.DefaultSchema = GetDefaultSchema(connection);

            var serverCollation = GetServerCollation(connection);
            var databaseCollation = GetDatabaseCollation(connection);
            if (databaseCollation is not null && databaseCollation != serverCollation)
            {
                databaseModel.Collation = databaseCollation;
            }

            var typeAliases = GetTypeAliases(connection);

            var schemaList = options.Schemas.ToList();
            var schemaFilter = GenerateSchemaFilter(schemaList);
            var tableList = options.Tables.ToList();
            var tableFilter = GenerateTableFilter(tableList.Select(Parse).ToList(), schemaFilter);

            if (SupportsSequences())
            {
                GetSequences(connection, databaseModel, schemaFilter, typeAliases);
            }

            GetTables(connection, databaseModel, tableFilter, typeAliases, databaseCollation);

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
            _compatibilityLevel = null;
            _engineEdition = null;

            if (!connectionStartedOpen)
            {
                connection.Close();
            }
        }

        static EngineEdition GetEngineEdition(DbConnection connection)
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT SERVERPROPERTY('EngineEdition');";
            var result = command.ExecuteScalar();
            return result != null ? (EngineEdition)Convert.ToInt32(result) : 0;
        }

        static byte GetCompatibilityLevel(DbConnection connection)
        {
            using var command = connection.CreateCommand();
            command.CommandText =
                $"""
SELECT compatibility_level
FROM sys.databases
WHERE name = '{connection.Database}'
""";

            var result = command.ExecuteScalar();
            return result != null ? Convert.ToByte(result) : (byte)0;
        }

        static string? GetServerCollation(DbConnection connection)
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT SERVERPROPERTY('Collation');";
            return command.ExecuteScalar() as string;
        }

        static string? GetDatabaseCollation(DbConnection connection)
        {
            using var command = connection.CreateCommand();
            command.CommandText =
                $"""
SELECT collation_name
FROM sys.databases
WHERE name = '{connection.Database}';
""";

            return command.ExecuteScalar() as string;
        }
    }

    private void CheckViewDefinitionRights(DbConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT HAS_PERMS_BY_NAME(QUOTENAME(DB_NAME()), 'DATABASE', 'VIEW DEFINITION');";
        var hasAccess = (int)command.ExecuteScalar()!;

        if (hasAccess == 0)
        {
            _logger.MissingViewDefinitionRightsWarning();
        }
    }

    private string? GetDefaultSchema(DbConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT SCHEMA_NAME();";

        if (command.ExecuteScalar() is string schema)
        {
            _logger.DefaultSchemaFound(schema);

            return schema;
        }

        return null;
    }

    private static Func<string, string>? GenerateSchemaFilter(IReadOnlyList<string> schemas)
        => schemas.Count > 0
            ? (s =>
            {
                var schemaFilterBuilder = new StringBuilder();
                schemaFilterBuilder.Append(s);
                schemaFilterBuilder.Append(" IN (");
                schemaFilterBuilder.AppendJoin(", ", schemas.Select(EscapeLiteral));
                schemaFilterBuilder.Append(')');
                return schemaFilterBuilder.ToString();
            })
            : null;

    private static (string? Schema, string Table) Parse(string table)
    {
        var match = PartExtractor.Match(table.Trim());

        if (!match.Success)
        {
            throw new InvalidOperationException(SqlServerStrings.InvalidTableToIncludeInScaffolding(table));
        }

        var part1 = match.Groups["part1"].Value.Replace("]]", "]");
        var part2 = match.Groups["part2"].Value.Replace("]]", "]");

        return string.IsNullOrEmpty(part2) ? (null, part1) : (part1, part2);
    }

    private static Func<string, string, string>? GenerateTableFilter(
        IReadOnlyList<(string? Schema, string Table)> tables,
        Func<string, string>? schemaFilter)
        => schemaFilter != null
            || tables.Count > 0
                ? ((s, t) =>
                {
                    var tableFilterBuilder = new StringBuilder();

                    var openBracket = false;
                    if (schemaFilter != null)
                    {
                        tableFilterBuilder
                            .Append('(')
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
                            tableFilterBuilder.Append('(');
                            openBracket = true;
                        }

                        var tablesWithoutSchema = tables.Where(e => string.IsNullOrEmpty(e.Schema)).ToList();
                        if (tablesWithoutSchema.Count > 0)
                        {
                            tableFilterBuilder.Append(t);
                            tableFilterBuilder.Append(" IN (");
                            tableFilterBuilder.AppendJoin(", ", tablesWithoutSchema.Select(e => EscapeLiteral(e.Table)));
                            tableFilterBuilder.Append(')');
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
                            tableFilterBuilder.AppendJoin(", ", tablesWithSchema.Select(e => EscapeLiteral(e.Table)));
                            tableFilterBuilder.Append(") AND (");
                            tableFilterBuilder.Append(s);
                            tableFilterBuilder.Append(" + N'.' + ");
                            tableFilterBuilder.Append(t);
                            tableFilterBuilder.Append(") IN (");
                            tableFilterBuilder.AppendJoin(
                                ", ", tablesWithSchema.Select(e => EscapeLiteral($"{e.Schema}.{e.Table}")));
                            tableFilterBuilder.Append(')');
                        }
                    }

                    if (openBracket)
                    {
                        tableFilterBuilder.Append(')');
                    }

                    return tableFilterBuilder.ToString();
                })
                : null;

    private static string EscapeLiteral(string s)
        => $"N'{s.Replace("'", "''")}'";

    private IReadOnlyDictionary<string, (string, string)> GetTypeAliases(DbConnection connection)
    {
        using var command = connection.CreateCommand();
        var typeAliasMap = new Dictionary<string, (string, string)>(StringComparer.OrdinalIgnoreCase);

        command.CommandText =
            """
SELECT
    SCHEMA_NAME([t].[schema_id]) AS [schema_name],
    [t].[name] AS [type_name],
    [t2].[name] AS [underlying_system_type],
    CAST([t].[max_length] AS int) AS [max_length],
    CAST([t].[precision] AS int) AS [precision],
    CAST([t].[scale] AS int) AS [scale]
FROM [sys].[types] AS [t]
JOIN [sys].[types] AS [t2] ON [t].[system_type_id] = [t2].[user_type_id]
WHERE [t].[is_user_defined] = 1 OR [t].[system_type_id] <> [t].[user_type_id];
""";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var schema = reader.GetValueOrDefault<string>("schema_name");
            var userType = reader.GetFieldValue<string>("type_name");
            var systemType = reader.GetFieldValue<string>("underlying_system_type");
            var maxLength = reader.GetValueOrDefault<int>("max_length");
            var precision = reader.GetValueOrDefault<int>("precision");
            var scale = reader.GetValueOrDefault<int>("scale");

            var storeType = GetStoreType(systemType, maxLength, precision, scale);

            _logger.TypeAliasFound(DisplayName(schema, userType), storeType);

            typeAliasMap.Add($"[{schema}].[{userType}]", (storeType, systemType));
        }

        return typeAliasMap;
    }

    private void GetSequences(
        DbConnection connection,
        DatabaseModel databaseModel,
        Func<string, string>? schemaFilter,
        IReadOnlyDictionary<string, (string storeType, string)> typeAliases)
    {
        using var command = connection.CreateCommand();
        command.CommandText =
            """
SELECT
    OBJECT_SCHEMA_NAME([s].[object_id]) AS [schema_name],
    [s].[name],
    SCHEMA_NAME([t].[schema_id]) AS [type_schema],
    TYPE_NAME([s].[user_type_id]) AS [type_name],
    CAST([s].[precision] AS int) AS [precision],
    CAST([s].[scale] AS int) AS [scale],
    [s].[is_cycling],
    CAST([s].[increment] AS int) AS [increment],
    CAST(CASE
        WHEN [s].[start_value] >  9223372036854775807 THEN  9223372036854775807
        WHEN [s].[start_value] < -9223372036854775808 THEN -9223372036854775808
        ELSE [s].[start_value]
        END AS bigint) AS start_value,
    CAST(CASE
        WHEN [s].[minimum_value] >  9223372036854775807 THEN  9223372036854775807
        WHEN [s].[minimum_value] < -9223372036854775808 THEN -9223372036854775808
        ELSE [s].[minimum_value]
        END AS bigint) AS minimum_value,
    CAST(CASE
        WHEN [s].[maximum_value] >  9223372036854775807 THEN  9223372036854775807
        WHEN [s].[maximum_value] < -9223372036854775808 THEN -9223372036854775808
        ELSE [s].[maximum_value]
        END AS bigint) AS maximum_value,
    [s].[is_cached],
    [s].[cache_size]
FROM [sys].[sequences] AS [s]
JOIN [sys].[types] AS [t] ON [s].[user_type_id] = [t].[user_type_id]
""";

        if (schemaFilter != null)
        {
            command.CommandText += @"
WHERE "
                + schemaFilter("OBJECT_SCHEMA_NAME([s].[object_id])");
        }

        command.CommandText += ';';

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var schema = reader.GetValueOrDefault<string>("schema_name");
            var name = reader.GetString("name");
            var storeTypeSchema = reader.GetValueOrDefault<string>("type_schema");
            var storeType = reader.GetString("type_name");
            var precision = reader.GetValueOrDefault<int>("precision");
            var scale = reader.GetValueOrDefault<int>("scale");
            var cyclic = reader.GetValueOrDefault<bool>("is_cycling");
            var incrementBy = reader.GetValueOrDefault<int>("increment");
            var startValue = reader.GetValueOrDefault<long>("start_value");
            var minValue = reader.GetValueOrDefault<long>("minimum_value");
            var maxValue = reader.GetValueOrDefault<long>("maximum_value");
            var cached = reader.GetValueOrDefault<bool>("is_cached");
            var cacheSize = reader.GetValueOrDefault<int?>("cache_size");

            // Swap store type if type alias is used
            if (typeAliases.TryGetValue($"[{storeTypeSchema}].[{storeType}]", out var value))
            {
                storeType = value.storeType;
            }

            storeType = GetStoreType(storeType, maxLength: 0, precision: precision, scale: scale);

            _logger.SequenceFound(DisplayName(schema, name), storeType, cyclic, incrementBy, startValue, minValue, maxValue, cached, cacheSize);

            var sequence = new DatabaseSequence
            {
                Database = databaseModel,
                Name = name,
                Schema = schema,
                StoreType = storeType,
                IsCyclic = cyclic,
                IncrementBy = incrementBy,
                StartValue = startValue,
                MinValue = minValue,
                MaxValue = maxValue,
                IsCached = cached,
                CacheSize = cacheSize
            };

            if (DefaultSequenceMinMax.TryGetValue(storeType, out var defaultMinMax))
            {
                var defaultMin = defaultMinMax[0];
                sequence.MinValue = sequence.MinValue == defaultMin ? null : sequence.MinValue;
                sequence.StartValue = sequence.StartValue == defaultMin ? null : sequence.StartValue;

                sequence.MaxValue = sequence.MaxValue == defaultMinMax[1]
                    ? null
                    : sequence.MaxValue;
            }

            databaseModel.Sequences.Add(sequence);
        }
    }

    private void GetTables(
        DbConnection connection,
        DatabaseModel databaseModel,
        Func<string, string, string>? tableFilter,
        IReadOnlyDictionary<string, (string, string)> typeAliases,
        string? databaseCollation)
    {
        using var command = connection.CreateCommand();
        var tables = new List<DatabaseTable>();

        var supportsMemoryOptimizedTable = SupportsMemoryOptimizedTable();
        var supportsTemporalTable = SupportsTemporalTable();

        var builder = new StringBuilder(
            """
SELECT
    SCHEMA_NAME([t].[schema_id]) AS [schema],
    [t].[name],
    CAST([e].[value] AS nvarchar(MAX)) AS [comment],
    'table' AS [type]
""");

        if (supportsMemoryOptimizedTable)
        {
            builder.AppendLine(",").Append("    [t].[is_memory_optimized]");
        }

        if (supportsTemporalTable)
        {
            builder.AppendLine(",").Append(
                """
    [t].[temporal_type],
    (SELECT [t2].[name] FROM [sys].[tables] AS t2 WHERE [t2].[object_id] = [t].[history_table_id]) AS [history_table_name],
    (SELECT SCHEMA_NAME([t2].[schema_id]) FROM [sys].[tables] AS t2 WHERE [t2].[object_id] = [t].[history_table_id]) AS [history_table_schema],
    (SELECT [c].[name] FROM [sys].[columns] as [c] WHERE [c].[object_id] = [t].[object_id] AND [c].[generated_always_type] = 1) as [period_start_column],
    (SELECT [c].[name] FROM [sys].[columns] as [c] WHERE [c].[object_id] = [t].[object_id] AND [c].[generated_always_type] = 2) as [period_end_column]
""");
        }

        builder.AppendLine().Append(
            """
FROM [sys].[tables] AS [t]
LEFT JOIN [sys].[extended_properties] AS [e] ON [e].[major_id] = [t].[object_id] AND [e].[minor_id] = 0 AND [e].[class] = 1 AND [e].[name] = 'MS_Description'
""");

        var tableFilterBuilder = new StringBuilder(
            $"""
[t].[is_ms_shipped] = 0
AND NOT EXISTS (SELECT *
    FROM [sys].[extended_properties] AS [ep]
    WHERE [ep].[major_id] = [t].[object_id]
        AND [ep].[minor_id] = 0
        AND [ep].[class] = 1
        AND [ep].[name] = N'microsoft_database_tools_support'
    )
AND [t].[name] <> '{HistoryRepository.DefaultTableName}'
""");

        if (supportsTemporalTable)
        {
            tableFilterBuilder.AppendLine().Append("AND [t].[temporal_type] <> 1");
        }

        if (tableFilter != null)
        {
            tableFilterBuilder
                .AppendLine()
                .Append("AND ")
                .Append(tableFilter("SCHEMA_NAME([t].[schema_id])", "[t].[name]"));
        }

        var tableFilterSql = tableFilterBuilder.ToString();

        builder.AppendLine().Append("WHERE ").Append(tableFilterSql);

        // If views are supported, scaffold them too.
        string? viewFilter = null;

        if (SupportsViews())
        {
            builder.AppendLine().Append(
                """
UNION
SELECT
    SCHEMA_NAME([v].[schema_id]) AS [schema],
    [v].[name],
    CAST([e].[value] AS nvarchar(MAX)) AS [comment],
    'view' AS [type]
""");

            if (supportsMemoryOptimizedTable)
            {
                builder.AppendLine(",").Append("     CAST(0 AS bit) AS [is_memory_optimized]");
            }

            if (supportsTemporalTable)
            {
                builder.AppendLine(",").Append(
                    """
     1 AS [temporal_type],
     NULL AS [history_table_name],
     NULL AS [history_table_schema],
     NULL AS [period_start_column],
     NULL AS [period_end_column]
""");
            }

            builder.Append(
                """
FROM [sys].[views] AS [v]
LEFT JOIN [sys].[extended_properties] AS [e] ON [e].[major_id] = [v].[object_id] AND [e].[minor_id] = 0 AND [e].[class] = 1 AND [e].[name] = 'MS_Description'
""");

            var viewFilterBuilder = new StringBuilder(
                """
[v].[is_ms_shipped] = 0
AND [v].[is_date_correlation_view] = 0
""");

            if (tableFilter != null)
            {
                viewFilterBuilder
                    .AppendLine()
                    .Append("AND ")
                    .Append(tableFilter("SCHEMA_NAME([v].[schema_id])", "[v].[name]"));
            }

            viewFilter = viewFilterBuilder.ToString();

            builder.AppendLine().Append("WHERE ").Append(viewFilter);
        }

        builder.Append(";");
        command.CommandText = builder.ToString();

        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                var schema = reader.GetValueOrDefault<string>("schema");
                var name = reader.GetString("name");
                var comment = reader.GetValueOrDefault<string>("comment");
                var type = reader.GetString("type");

                _logger.TableFound(DisplayName(schema, name));

                var table = type == "table"
                    ? new DatabaseTable { Database = databaseModel, Name = name }
                    : new DatabaseView { Database = databaseModel, Name = name };

                table.Schema = schema;
                table.Comment = comment;

                if (supportsMemoryOptimizedTable)
                {
                    if (reader.GetValueOrDefault<bool>("is_memory_optimized"))
                    {
                        table[SqlServerAnnotationNames.MemoryOptimized] = true;
                    }
                }

                if (supportsTemporalTable)
                {
                    if (reader.GetValueOrDefault<int>("temporal_type") == 2)
                    {
                        table[SqlServerAnnotationNames.IsTemporal] = true;

                        var historyTableName = reader.GetValueOrDefault<string>("history_table_name");
                        table[SqlServerAnnotationNames.TemporalHistoryTableName] = historyTableName;

                        var historyTableSchema = reader.GetValueOrDefault<string>("history_table_schema");
                        table[SqlServerAnnotationNames.TemporalHistoryTableSchema] = historyTableSchema;

                        var periodStartColumnName = reader.GetValueOrDefault<string>("period_start_column");
                        table[SqlServerAnnotationNames.TemporalPeriodStartPropertyName] = periodStartColumnName;

                        var periodEndColumnName = reader.GetValueOrDefault<string>("period_end_column");
                        table[SqlServerAnnotationNames.TemporalPeriodEndPropertyName] = periodEndColumnName;
                    }
                }

                tables.Add(table);
            }
        }

        // This is done separately due to MARS property may be turned off
        GetColumns(connection, tables, tableFilterSql, viewFilter, typeAliases, databaseCollation);

        if (SupportsIndexes())
        {
            GetIndexes(connection, tables, tableFilterSql);
        }

        GetForeignKeys(connection, tables, tableFilterSql);

        if (SupportsTriggers())
        {
            GetTriggers(connection, tables, tableFilterSql);
        }

        foreach (var table in tables)
        {
            databaseModel.Tables.Add(table);
        }
    }

    private void GetColumns(
        DbConnection connection,
        IReadOnlyList<DatabaseTable> tables,
        string tableFilter,
        string? viewFilter,
        IReadOnlyDictionary<string, (string storeType, string typeName)> typeAliases,
        string? databaseCollation)
    {
        using var command = connection.CreateCommand();
        var builder = new StringBuilder(
            $"""
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
    [cc].[is_persisted] AS [computed_is_persisted],
    CAST([e].[value] AS nvarchar(MAX)) AS [comment],
    [c].[collation_name],
    [c].[is_sparse]
FROM
(
    SELECT [t].[name], [t].[object_id], [t].[schema_id]
    FROM [sys].[tables] t
    WHERE {tableFilter}
""");

        if (SupportsViews())
        {
            Check.DebugAssert(viewFilter is not null, "viewFilter is not null");

            builder.AppendLine().Append(
                $"""
    UNION ALL
    SELECT[v].[name], [v].[object_id], [v].[schema_id]
    FROM [sys].[views] v
    WHERE {viewFilter}
""");
        }

        builder.AppendLine().Append(
            """
) o
JOIN [sys].[columns] AS [c] ON [o].[object_id] = [c].[object_id]
LEFT JOIN [sys].[types] AS [tp] ON [c].[user_type_id] = [tp].[user_type_id]
LEFT JOIN [sys].[extended_properties] AS [e] ON [e].[major_id] = [o].[object_id] AND [e].[minor_id] = [c].[column_id] AND [e].[class] = 1 AND [e].[name] = 'MS_Description'
LEFT JOIN [sys].[computed_columns] AS [cc] ON [c].[object_id] = [cc].[object_id] AND [c].[column_id] = [cc].[column_id]
LEFT JOIN [sys].[default_constraints] AS [dc] ON [c].[object_id] = [dc].[parent_object_id] AND [c].[column_id] = [dc].[parent_column_id]
""");

        if (SupportsTemporalTable())
        {
            builder.AppendLine().Append("WHERE [c].[generated_always_type] <> 1 AND [c].[generated_always_type] <> 2");
        }

        builder.AppendLine().Append("ORDER BY [table_schema], [table_name], [c].[column_id];");

        command.CommandText = builder.ToString();

        using var reader = command.ExecuteReader();
        var tableColumnGroups = reader.Cast<DbDataRecord>()
            .GroupBy(
                ddr => (tableSchema: ddr.GetValueOrDefault<string>("table_schema"),
                    tableName: ddr.GetFieldValue<string>("table_name")));

        foreach (var tableColumnGroup in tableColumnGroups)
        {
            var tableSchema = tableColumnGroup.Key.tableSchema;
            var tableName = tableColumnGroup.Key.tableName;

            var table = tables.Single(t => t.Schema == tableSchema && t.Name == tableName);

            foreach (var dataRecord in tableColumnGroup)
            {
                var columnName = dataRecord.GetFieldValue<string>("column_name");
                var ordinal = dataRecord.GetFieldValue<int>("ordinal");
                var dataTypeSchemaName = dataRecord.GetValueOrDefault<string>("type_schema");
                var dataTypeName = dataRecord.GetValueOrDefault<string>("type_name");
                var maxLength = dataRecord.GetValueOrDefault<int>("max_length");
                var precision = dataRecord.GetValueOrDefault<int>("precision");
                var scale = dataRecord.GetValueOrDefault<int>("scale");
                var nullable = dataRecord.GetValueOrDefault<bool>("is_nullable");
                var isIdentity = dataRecord.GetValueOrDefault<bool>("is_identity");
                var defaultValueSql = dataRecord.GetValueOrDefault<string>("default_sql");
                var computedValue = dataRecord.GetValueOrDefault<string>("computed_sql");
                var computedIsPersisted = dataRecord.GetValueOrDefault<bool>("computed_is_persisted");
                var comment = dataRecord.GetValueOrDefault<string>("comment");
                var collation = dataRecord.GetValueOrDefault<string>("collation_name");
                var isSparse = dataRecord.GetValueOrDefault<bool>("is_sparse");

                if (dataTypeName is null)
                {
                    _logger.ColumnWithoutTypeWarning(DisplayName(tableSchema, tableName), columnName);
                    continue;
                }

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
                    defaultValueSql,
                    computedValue,
                    computedIsPersisted);

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

                var column = new DatabaseColumn
                {
                    Table = table,
                    Name = columnName,
                    StoreType = storeType,
                    IsNullable = nullable,
                    DefaultValue = TryParseClrDefault(systemTypeName, defaultValueSql),
                    DefaultValueSql = defaultValueSql,
                    ComputedColumnSql = computedValue,
                    IsStored = computedIsPersisted,
                    Comment = comment,
                    Collation = collation == databaseCollation ? null : collation,
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

                if (isSparse)
                {
                    column[SqlServerAnnotationNames.Sparse] = true;
                }

                table.Columns.Add(column);
            }
        }
    }

    private object? TryParseClrDefault(string dataTypeName, string? defaultValueSql)
    {
        defaultValueSql = defaultValueSql?.Trim();
        if (string.IsNullOrEmpty(defaultValueSql))
        {
            return null;
        }

        var mapping = _typeMappingSource.FindMapping(dataTypeName);
        if (mapping == null)
        {
            return null;
        }

        Unwrap();
        if (defaultValueSql.StartsWith("CONVERT", StringComparison.OrdinalIgnoreCase))
        {
            defaultValueSql = defaultValueSql.Substring(defaultValueSql.IndexOf(',') + 1);
            defaultValueSql = defaultValueSql.Substring(0, defaultValueSql.LastIndexOf(')'));
            Unwrap();
        }

        if (defaultValueSql.Equals("NULL", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var type = mapping.ClrType;
        if (type == typeof(bool)
            && int.TryParse(defaultValueSql, out var intValue))
        {
            return intValue != 0;
        }

        if (type.IsNumeric())
        {
            try
            {
                return Convert.ChangeType(defaultValueSql, type);
            }
            catch
            {
                // Ignored
                return null;
            }
        }

        if ((defaultValueSql.StartsWith('\'') || defaultValueSql.StartsWith("N'", StringComparison.OrdinalIgnoreCase))
            && defaultValueSql.EndsWith('\''))
        {
            var startIndex = defaultValueSql.IndexOf('\'');
            defaultValueSql = defaultValueSql.Substring(startIndex + 1, defaultValueSql.Length - (startIndex + 2));

            if (type == typeof(string))
            {
                return defaultValueSql;
            }

            if (type == typeof(bool)
                && bool.TryParse(defaultValueSql, out var boolValue))
            {
                return boolValue;
            }

            if (type == typeof(Guid)
                && Guid.TryParse(defaultValueSql, out var guid))
            {
                return guid;
            }

            if (type == typeof(DateTime)
                && DateTime.TryParse(defaultValueSql, out var dateTime))
            {
                return dateTime;
            }

            if (type == typeof(DateOnly)
                && DateOnly.TryParse(defaultValueSql, out var dateOnly))
            {
                return dateOnly;
            }

            if (type == typeof(TimeOnly)
                && TimeOnly.TryParse(defaultValueSql, out var timeOnly))
            {
                return timeOnly;
            }

            if (type == typeof(DateTimeOffset)
                && DateTimeOffset.TryParse(defaultValueSql, out var dateTimeOffset))
            {
                return dateTimeOffset;
            }
        }

        return null;

        void Unwrap()
        {
            while (defaultValueSql.StartsWith('(') && defaultValueSql.EndsWith(')'))
            {
                defaultValueSql = (defaultValueSql.Substring(1, defaultValueSql.Length - 2)).Trim();
            }
        }
    }

    private static string GetStoreType(string dataTypeName, int maxLength, int precision, int scale)
    {
        if (dataTypeName == "timestamp")
        {
            return "rowversion";
        }

        if (dataTypeName is "decimal" or "numeric")
        {
            return $"{dataTypeName}({precision}, {scale})";
        }

        if (DateTimePrecisionTypes.Contains(dataTypeName)
            && scale != 7)
        {
            return $"{dataTypeName}({scale})";
        }

        if (MaxLengthRequiredTypes.Contains(dataTypeName))
        {
            if (maxLength == -1)
            {
                return $"{dataTypeName}(max)";
            }

            if (dataTypeName is "nvarchar" or "nchar")
            {
                maxLength /= 2;
            }

            return $"{dataTypeName}({maxLength})";
        }

        return dataTypeName;
    }

    private void GetIndexes(DbConnection connection, IReadOnlyList<DatabaseTable> tables, string tableFilter)
    {
        using var command = connection.CreateCommand();
        var commandText = @"
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
    [i].[fill_factor],
    COL_NAME([ic].[object_id], [ic].[column_id]) AS [column_name],
    [ic].[is_descending_key],
    [ic].[is_included_column]
FROM [sys].[indexes] AS [i]
JOIN [sys].[tables] AS [t] ON [i].[object_id] = [t].[object_id]
JOIN [sys].[index_columns] AS [ic] ON [i].[object_id] = [ic].[object_id] AND [i].[index_id] = [ic].[index_id]
JOIN [sys].[columns] AS [c] ON [ic].[object_id] = [c].[object_id] AND [ic].[column_id] = [c].[column_id]
WHERE [i].[is_hypothetical] = 0
AND "
            + tableFilter;

        if (SupportsTemporalTable())
        {
            commandText += @"
AND CAST([i].[object_id] AS nvarchar(12)) + '#' + CAST([i].[index_id] AS nvarchar(12)) NOT IN
(
   SELECT CAST([i].[object_id] AS nvarchar(12)) + '#' + CAST([i].[index_id] AS nvarchar(12))
   FROM [sys].[indexes] i
   JOIN [sys].[tables] AS [t] ON [i].[object_id] = [t].[object_id]
   JOIN [sys].[index_columns] AS [ic] ON [i].[object_id] = [ic].[object_id] AND [i].[index_id] = [ic].[index_id]
   JOIN [sys].[columns] AS [c] ON [ic].[object_id] = [c].[object_id] AND [ic].[column_id] = [c].[column_id]
   WHERE "
                + tableFilter;

            commandText += @"
   AND [c].[is_hidden] = 1
   AND [i].[is_hypothetical] = 0
)";
        }

        commandText += @"
ORDER BY [table_schema], [table_name], [index_name], [ic].[key_ordinal];";

        command.CommandText = commandText;

        using var reader = command.ExecuteReader();
        var tableIndexGroups = reader.Cast<DbDataRecord>()
            .GroupBy(
                ddr => (tableSchema: ddr.GetValueOrDefault<string>("table_schema"),
                    tableName: ddr.GetFieldValue<string>("table_name")));

        foreach (var tableIndexGroup in tableIndexGroups)
        {
            var tableSchema = tableIndexGroup.Key.tableSchema;
            var tableName = tableIndexGroup.Key.tableName;

            var table = tables.Single(t => t.Schema == tableSchema && t.Name == tableName);

            var primaryKeyGroups = tableIndexGroup
                .Where(ddr => ddr.GetValueOrDefault<bool>("is_primary_key"))
                .GroupBy(
                    ddr =>
                        (Name: ddr.GetFieldValue<string>("index_name"),
                            TypeDesc: ddr.GetValueOrDefault<string>("type_desc"),
                            FillFactor: ddr.GetValueOrDefault<byte>("fill_factor")))
                .ToArray();

            Check.DebugAssert(primaryKeyGroups.Length is 0 or 1, "Multiple primary keys found");

            if (primaryKeyGroups.Length == 1)
            {
                if (TryGetPrimaryKey(primaryKeyGroups[0], out var primaryKey))
                {
                    _logger.PrimaryKeyFound(primaryKey.Name!, DisplayName(tableSchema, tableName));
                    table.PrimaryKey = primaryKey;
                }
            }

            var uniqueConstraintGroups = tableIndexGroup
                .Where(ddr => ddr.GetValueOrDefault<bool>("is_unique_constraint"))
                .GroupBy(
                    ddr =>
                        (Name: ddr.GetValueOrDefault<string>("index_name"),
                            TypeDesc: ddr.GetValueOrDefault<string>("type_desc"),
                            FillFactor: ddr.GetValueOrDefault<byte>("fill_factor")))
                .ToArray();

            foreach (var uniqueConstraintGroup in uniqueConstraintGroups)
            {
                if (TryGetUniqueConstraint(uniqueConstraintGroup, out var uniqueConstraint))
                {
                    _logger.UniqueConstraintFound(uniqueConstraintGroup.Key.Name!, DisplayName(tableSchema, tableName));
                    table.UniqueConstraints.Add(uniqueConstraint);
                }
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
                            FilterDefinition: ddr.GetValueOrDefault<string>("filter_definition"),
                            FillFactor: ddr.GetValueOrDefault<byte>("fill_factor")))
                .ToArray();

            foreach (var indexGroup in indexGroups)
            {
                if (TryGetIndex(indexGroup, out var index))
                {
                    _logger.IndexFound(indexGroup.Key.Name!, DisplayName(tableSchema, tableName), indexGroup.Key.IsUnique);
                    table.Indexes.Add(index);
                }
            }

            bool TryGetPrimaryKey(
                IGrouping<(string Name, string? TypeDesc, byte FillFactor), DbDataRecord> primaryKeyGroup,
                [NotNullWhen(true)] out DatabasePrimaryKey? primaryKey)
            {
                primaryKey = new DatabasePrimaryKey { Table = table, Name = primaryKeyGroup.Key.Name };

                if (primaryKeyGroup.Key.TypeDesc == "NONCLUSTERED")
                {
                    primaryKey[SqlServerAnnotationNames.Clustered] = false;
                }

                if (primaryKeyGroup.Key.FillFactor is > 0 and <= 100)
                {
                    primaryKey[SqlServerAnnotationNames.FillFactor] = (int)primaryKeyGroup.Key.FillFactor;
                }

                foreach (var dataRecord in primaryKeyGroup)
                {
                    var columnName = dataRecord.GetValueOrDefault<string>("column_name");
                    var column = table.Columns.FirstOrDefault(c => c.Name == columnName)
                        ?? table.Columns.FirstOrDefault(
                            c => c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));

                    if (column is null)
                    {
                        return false;
                    }

                    primaryKey.Columns.Add(column);
                }

                return true;
            }

            bool TryGetUniqueConstraint(
                IGrouping<(string? Name, string? TypeDesc, byte FillFactor), DbDataRecord> uniqueConstraintGroup,
                [NotNullWhen(true)] out DatabaseUniqueConstraint? uniqueConstraint)
            {
                uniqueConstraint = new DatabaseUniqueConstraint { Table = table, Name = uniqueConstraintGroup.Key.Name };

                if (uniqueConstraintGroup.Key.TypeDesc == "CLUSTERED")
                {
                    uniqueConstraint[SqlServerAnnotationNames.Clustered] = true;
                }

                if (uniqueConstraintGroup.Key.FillFactor is > 0 and <= 100)
                {
                    uniqueConstraint[SqlServerAnnotationNames.FillFactor] = (int)uniqueConstraintGroup.Key.FillFactor;
                }

                foreach (var dataRecord in uniqueConstraintGroup)
                {
                    var columnName = dataRecord.GetValueOrDefault<string>("column_name");
                    var column = table.Columns.FirstOrDefault(c => c.Name == columnName)
                        ?? table.Columns.FirstOrDefault(
                            c => c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));

                    if (column is null)
                    {
                        return false;
                    }

                    uniqueConstraint.Columns.Add(column);
                }

                return true;
            }

            bool TryGetIndex(
                IGrouping<(string? Name, string? TypeDesc, bool IsUnique, bool HasFilter, string? FilterDefinition, byte FillFactor),
                    DbDataRecord> indexGroup,
                [NotNullWhen(true)] out DatabaseIndex? index)
            {
                index = new DatabaseIndex
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

                if (indexGroup.Key.FillFactor is > 0 and <= 100)
                {
                    index[SqlServerAnnotationNames.FillFactor] = (int)indexGroup.Key.FillFactor;
                }

                foreach (var dataRecord in indexGroup)
                {
                    var columnName = dataRecord.GetValueOrDefault<string>("column_name");

                    var isIncludedColumn = dataRecord.GetValueOrDefault<bool>("is_included_column");
                    if (isIncludedColumn)
                    {
                        continue;
                    }

                    var column = table.Columns.FirstOrDefault(c => c.Name == columnName)
                        ?? table.Columns.FirstOrDefault(
                            c => c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));

                    if (column is null)
                    {
                        return false;
                    }

                    index.IsDescending.Add(dataRecord.GetValueOrDefault<bool>("is_descending_key"));

                    index.Columns.Add(column);
                }

                return index.Columns.Count > 0;
            }
        }
    }

    private void GetForeignKeys(DbConnection connection, IReadOnlyList<DatabaseTable> tables, string tableFilter)
    {
        using var command = connection.CreateCommand();
        command.CommandText =
            $"""
SELECT
    SCHEMA_NAME([t].[schema_id]) AS [table_schema],
    [t].[name] AS [table_name],
    [f].[name],
	SCHEMA_NAME(tab2.[schema_id]) AS [principal_table_schema],
	[tab2].name AS [principal_table_name],
	[f].[delete_referential_action_desc],
    [col1].[name] AS [column_name],
    [col2].[name] AS [referenced_column_name]
FROM [sys].[foreign_keys] AS [f]
JOIN [sys].[foreign_key_columns] AS fc ON [fc].[constraint_object_id] = [f].[object_id]
JOIN [sys].[tables] AS [t] ON [t].[object_id] = [fc].[parent_object_id]
JOIN [sys].[columns] AS [col1] ON [col1].[column_id] = [fc].[parent_column_id] AND [col1].[object_id] = [t].[object_id]
JOIN [sys].[tables] AS [tab2] ON [tab2].[object_id] = [fc].[referenced_object_id]
JOIN [sys].[columns] AS [col2] ON [col2].[column_id] = [fc].[referenced_column_id] AND [col2].[object_id] = [tab2].[object_id]
WHERE {tableFilter}
ORDER BY [table_schema], [table_name], [f].[name], [fc].[constraint_column_id];
""";

        using var reader = command.ExecuteReader();
        var tableForeignKeyGroups = reader.Cast<DbDataRecord>()
            .GroupBy(
                ddr => (tableSchema: ddr.GetValueOrDefault<string>("table_schema"),
                    tableName: ddr.GetFieldValue<string>("table_name")));

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

                if (principalTableName == null)
                {
                    _logger.ForeignKeyReferencesUnknownPrincipalTableWarning(
                        fkName,
                        DisplayName(table.Schema, table.Name));

                    continue;
                }

                _logger.ForeignKeyFound(
                    fkName!,
                    DisplayName(table.Schema, table.Name),
                    DisplayName(principalTableSchema, principalTableName),
                    onDeleteAction!);

                var principalTable = tables.FirstOrDefault(
                        t => t.Schema == principalTableSchema
                            && t.Name == principalTableName)
                    ?? tables.FirstOrDefault(
                        t => t.Schema?.Equals(principalTableSchema, StringComparison.OrdinalIgnoreCase) == true
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
                    Table = table,
                    Name = fkName,
                    PrincipalTable = principalTable,
                    OnDelete = ConvertToReferentialAction(onDeleteAction)
                };

                var invalid = false;

                foreach (var dataRecord in foreignKeyGroup)
                {
                    var columnName = dataRecord.GetValueOrDefault<string>("column_name");
                    var column = table.Columns.FirstOrDefault(c => c.Name == columnName)
                        ?? table.Columns.FirstOrDefault(
                            c => c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
                    Check.DebugAssert(column != null, "column is null.");

                    var principalColumnName = dataRecord.GetValueOrDefault<string>("referenced_column_name");
                    var principalColumn = foreignKey.PrincipalTable.Columns.FirstOrDefault(c => c.Name == principalColumnName)
                        ?? foreignKey.PrincipalTable.Columns.FirstOrDefault(
                            c => c.Name.Equals(principalColumnName, StringComparison.OrdinalIgnoreCase));
                    if (principalColumn == null)
                    {
                        invalid = true;
                        _logger.ForeignKeyPrincipalColumnMissingWarning(
                            fkName!,
                            DisplayName(table.Schema, table.Name),
                            principalColumnName!,
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
                            foreignKey.Name!,
                            DisplayName(table.Schema, table.Name));
                    }
                    else
                    {
                        var duplicated = table.ForeignKeys
                            .FirstOrDefault(
                                k => k.Columns.SequenceEqual(foreignKey.Columns)
                                    && k.PrincipalColumns.SequenceEqual(foreignKey.PrincipalColumns)
                                    && k.PrincipalTable.Equals(foreignKey.PrincipalTable));
                        if (duplicated != null)
                        {
                            _logger.DuplicateForeignKeyConstraintIgnored(
                                foreignKey.Name!,
                                DisplayName(table.Schema, table.Name),
                                duplicated.Name!);
                            continue;
                        }

                        table.ForeignKeys.Add(foreignKey);
                    }
                }
            }
        }
    }

    private void GetTriggers(DbConnection connection, IReadOnlyList<DatabaseTable> tables, string tableFilter)
    {
        using var command = connection.CreateCommand();
        command.CommandText =
            $"""
SELECT
    SCHEMA_NAME([t].[schema_id]) AS [table_schema],
    [t].[name] AS [table_name],
    [tr].[name] AS [trigger_name]
FROM [sys].[triggers] AS [tr]
JOIN [sys].[tables] AS [t] ON [tr].[parent_id] = [t].[object_id]
WHERE {tableFilter}
ORDER BY [table_schema], [table_name], [tr].[name];
""";

        using var reader = command.ExecuteReader();
        var tableGroups = reader.Cast<DbDataRecord>()
            .GroupBy(
                ddr => (tableSchema: ddr.GetValueOrDefault<string>("table_schema"),
                    tableName: ddr.GetFieldValue<string>("table_name")));

        foreach (var tableGroup in tableGroups)
        {
            var tableSchema = tableGroup.Key.tableSchema;
            var tableName = tableGroup.Key.tableName;

            var table = tables.Single(t => t.Schema == tableSchema && t.Name == tableName);

            foreach (var triggerRecord in tableGroup)
            {
                var triggerName = triggerRecord.GetFieldValue<string>("trigger_name");

                // We don't actually scaffold anything beyond the fact that there's a trigger with a given name.
                // This is to modify the SaveChanges logic to not use OUTPUT without INTO, which is incompatible with triggers.
                table.Triggers.Add(new DatabaseTrigger { Name = triggerName });
            }
        }
    }

    private bool SupportsTemporalTable()
        => _compatibilityLevel >= 130 && IsFullFeaturedEngineEdition();

    private bool SupportsMemoryOptimizedTable()
        => _compatibilityLevel >= 120 && IsFullFeaturedEngineEdition();

    private bool SupportsSequences()
        => _compatibilityLevel >= 110 && IsFullFeaturedEngineEdition();

    private bool SupportsIndexes()
        => _engineEdition != EngineEdition.DynamicsTdsEndpoint;

    private bool SupportsViews()
        => _engineEdition != EngineEdition.DynamicsTdsEndpoint;

    private bool SupportsTriggers()
        => IsFullFeaturedEngineEdition();

    private bool IsFullFeaturedEngineEdition()
        => _engineEdition is not EngineEdition.SqlDataWarehouse and not EngineEdition.SqlOnDemand and not EngineEdition.DynamicsTdsEndpoint;

    private static string DisplayName(string? schema, string name)
        => (!string.IsNullOrEmpty(schema) ? schema + "." : "") + name;

    private static ReferentialAction? ConvertToReferentialAction(string? onDeleteAction)
        => onDeleteAction switch
        {
            "NO_ACTION" => ReferentialAction.NoAction,
            "CASCADE" => ReferentialAction.Cascade,
            "SET_NULL" => ReferentialAction.SetNull,
            "SET_DEFAULT" => ReferentialAction.SetDefault,
            _ => null
        };
}
