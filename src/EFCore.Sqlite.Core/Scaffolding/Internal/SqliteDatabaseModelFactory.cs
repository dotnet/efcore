// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using static SQLitePCL.raw;

namespace Microsoft.EntityFrameworkCore.Sqlite.Scaffolding.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteDatabaseModelFactory : DatabaseModelFactory
{
    private static readonly HashSet<Type?> _defaultClrTypes =
    [
        typeof(long),
        typeof(string),
        typeof(byte[]),
        typeof(double)
    ];

    private static readonly HashSet<string> _boolTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "BIT",
        "BOOL",
        "BOOLEAN",
        "LOGICAL",
        "YESNO"
    };

    private static readonly HashSet<string> _uintTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "MEDIUMUINT",
        "UINT",
        "UINT32",
        "UNSIGNEDINTEGER32"
    };

    private static readonly HashSet<string> _ulongTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "BIGUINT",
        "UINT64",
        "ULONG",
        "UNSIGNEDINTEGER",
        "UNSIGNEDINTEGER64"
    };

    private static readonly HashSet<string> _byteTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "BYTE",
        "TINYINT",
        "UINT8",
        "UNSIGNEDINTEGER8"
    };

    private static readonly HashSet<string> _shortTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "INT16",
        "INTEGER16",
        "SHORT",
        "SMALLINT"
    };

    private static readonly HashSet<string> _longTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "BIGINT",
        "INT64",
        "INTEGER64",
        "LONG"
    };

    private static readonly HashSet<string> _sbyteTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "INT8",
        "INTEGER8",
        "SBYTE",
        "TINYSINT"
    };

    private static readonly HashSet<string> _floatTypes = new(StringComparer.OrdinalIgnoreCase) { "SINGLE" };

    private static readonly HashSet<string> _decimalTypes = new(StringComparer.OrdinalIgnoreCase) { "DECIMAL" };

    private static readonly HashSet<string> _ushortTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "SMALLUINT",
        "UINT16",
        "UNSIGNEDINTEGER16",
        "USHORT"
    };

    private static readonly HashSet<string> _timeOnlyTypes = new(StringComparer.OrdinalIgnoreCase) { "TIMEONLY" };

    private static readonly Dictionary<string, Type> _typesByName = new Dictionary<string, Type>
        {
            { "CURRENCY", typeof(decimal) },
            { "DATE", typeof(DateTime) },
            { "DATEONLY", typeof(DateOnly) },
            { "DATETIME", typeof(DateTime) },
            { "DATETIME2", typeof(DateTime) },
            { "DATETIMEOFFSET", typeof(DateTimeOffset) },
            { "GUID", typeof(Guid) },
            { "JSON", typeof(string) },
            { "MONEY", typeof(decimal) },
            { "NUMBER", typeof(decimal) },
            { "NUMERIC", typeof(decimal) },
            { "SMALLDATE", typeof(DateTime) },
            { "SMALLMONEY", typeof(decimal) },
            { "STRING", typeof(string) },
            { "TIME", typeof(TimeSpan) },
            { "TIMESPAN", typeof(TimeSpan) },
            { "TIMESTAMP", typeof(DateTime) },
            { "UNIQUEIDENTIFIER", typeof(Guid) },
            { "UUID", typeof(Guid) },
            { "XML", typeof(string) }
        }
        .Concat(_boolTypes.Select(t => KeyValuePair.Create(t, typeof(bool))))
        .Concat(_byteTypes.Select(t => KeyValuePair.Create(t, typeof(byte))))
        .Concat(_shortTypes.Select(t => KeyValuePair.Create(t, typeof(short))))
        .Concat(_sbyteTypes.Select(t => KeyValuePair.Create(t, typeof(sbyte))))
        .Concat(_floatTypes.Select(t => KeyValuePair.Create(t, typeof(float))))
        .Concat(_decimalTypes.Select(t => KeyValuePair.Create(t, typeof(decimal))))
        .Concat(_timeOnlyTypes.Select(t => KeyValuePair.Create(t, typeof(TimeOnly))))
        .Concat(_ushortTypes.Select(t => KeyValuePair.Create(t, typeof(ushort))))
        .Concat(_uintTypes.Select(t => KeyValuePair.Create(t, typeof(uint))))
        .Concat(_ulongTypes.Select(t => KeyValuePair.Create(t, typeof(ulong))))
        .ToDictionary(i => i.Key, i => i.Value, StringComparer.OrdinalIgnoreCase);

    private readonly IDiagnosticsLogger<DbLoggerCategory.Scaffolding> _logger;
    private readonly IRelationalTypeMappingSource _typeMappingSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteDatabaseModelFactory(
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
        using var connection = new SqliteConnection(connectionString);
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
        if (options.Schemas.Any())
        {
            _logger.SchemasNotSupportedWarning();
        }

        var databaseModel = new DatabaseModel();

        var connectionStartedOpen = connection.State == ConnectionState.Open;
        if (!connectionStartedOpen)
        {
            connection.Open();

            if (HasGeometryColumns(connection))
            {
                SpatialiteLoader.TryLoad(connection);
            }
        }

        try
        {
            databaseModel.DatabaseName = GetDatabaseName(connection);

            GetTables(connection, databaseModel, options.Tables);

            foreach (var table in databaseModel.Tables)
            {
                GetForeignKeys(connection, table, databaseModel.Tables);
            }

            var nullableKeyColumns = databaseModel.Tables
                .SelectMany(t => t.PrimaryKey?.Columns ?? [])
                .Concat(databaseModel.Tables.SelectMany(t => t.ForeignKeys).SelectMany(fk => fk.PrincipalColumns))
                .Where(c => c.IsNullable)
                .Distinct();
            foreach (var column in nullableKeyColumns)
            {
                // TODO: Consider warning
                column.IsNullable = false;
            }
        }
        finally
        {
            if (!connectionStartedOpen)
            {
                connection.Close();
            }
        }

        return databaseModel;
    }

    private static bool HasGeometryColumns(DbConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText =
            """
SELECT COUNT(*)
FROM "sqlite_master"
WHERE "name" = 'geometry_columns' AND "type" = 'table'
""";

        return (long)command.ExecuteScalar()! != 0L;
    }

    private static string GetDatabaseName(DbConnection connection)
    {
        var name = Path.GetFileNameWithoutExtension(connection.DataSource);
        if (string.IsNullOrEmpty(name))
        {
            name = "Main";
        }

        return name;
    }

    private void GetTables(DbConnection connection, DatabaseModel databaseModel, IEnumerable<string> tables)
    {
        var tablesToSelect = new HashSet<string>(tables.ToList(), StringComparer.OrdinalIgnoreCase);
        var selectedTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        using (var command = connection.CreateCommand())
        {
            command.CommandText =
                $"""
SELECT "name", "type"
FROM "sqlite_master"
WHERE "type" IN ('table', 'view') AND instr("name", 'sqlite_') <> 1 AND "name" NOT IN (
'{HistoryRepository.DefaultTableName}',
'ElementaryGeometries', 'geometry_columns', 'geometry_columns_auth',
'geometry_columns_field_infos', 'geometry_columns_statistics', 'geometry_columns_time',
'spatial_ref_sys', 'spatial_ref_sys_aux', 'SpatialIndex', 'spatialite_history',
'sql_statements_log', 'vector_layers', 'vector_layers_auth', 'vector_layers_statistics',
'vector_layers_field_infos', 'views_geometry_columns', 'views_geometry_columns_auth',
'views_geometry_columns_field_infos', 'views_geometry_columns_statistics',
'virts_geometry_columns', 'virts_geometry_columns_auth',
'geom_cols_ref_sys', 'spatial_ref_sys_all',
'virts_geometry_columns_field_infos', 'virts_geometry_columns_statistics')
""";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var name = reader.GetString(0);
                if (!AllowsTable(tablesToSelect, selectedTables, name))
                {
                    continue;
                }

                _logger.TableFound(name);

                var type = reader.GetString(1);
                var table = type == "table"
                    ? new DatabaseTable { Database = databaseModel, Name = name }
                    : new DatabaseView { Database = databaseModel, Name = name };

                GetColumns(connection, table);
                GetPrimaryKey(connection, table);
                GetUniqueConstraints(connection, table);
                GetIndexes(connection, table);

                databaseModel.Tables.Add(table);
            }
        }

        foreach (var table in tablesToSelect.Except(selectedTables, StringComparer.OrdinalIgnoreCase))
        {
            _logger.MissingTableWarning(table);
        }
    }

    private static bool AllowsTable(HashSet<string> tables, HashSet<string> selectedTables, string name)
    {
        if (tables.Count == 0)
        {
            return true;
        }

        if (tables.Contains(name))
        {
            selectedTables.Add(name);
            return true;
        }

        return false;
    }

    private void GetColumns(DbConnection connection, DatabaseTable table)
    {
        using var command = connection.CreateCommand();
        command.CommandText =
            """
SELECT "name", "type", "notnull", "dflt_value", "hidden"
FROM pragma_table_xinfo(@table)
WHERE "hidden" IN (0, 2, 3)
ORDER BY "cid"
""";

        var parameter = command.CreateParameter();
        parameter.ParameterName = "@table";
        parameter.Value = table.Name;
        command.Parameters.Add(parameter);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var columnName = reader.GetString(0);
            var dataType = reader.GetString(1);
            var notNull = reader.GetBoolean(2);
            var defaultValueSql = !reader.IsDBNull(3) ? reader.GetString(3) : null;
            var hidden = reader.GetInt64(4);

            _logger.ColumnFound(table.Name, columnName, dataType, notNull, defaultValueSql);

            string? collation = null;
            var autoIncrement = 0;
            if (connection is SqliteConnection sqliteConnection
                && table is not DatabaseView)
            {
                var db = sqliteConnection.Handle;
                var rc = sqlite3_table_column_metadata(
                    db,
                    connection.Database,
                    table.Name,
                    columnName,
                    out _,
                    out collation,
                    out _,
                    out _,
                    out autoIncrement);
                SqliteException.ThrowExceptionForRC(rc, db);
            }

            table.Columns.Add(
                new DatabaseColumn
                {
                    Table = table,
                    Name = columnName,
                    StoreType = dataType,
                    IsNullable = !notNull,
                    DefaultValueSql = defaultValueSql,
                    ValueGenerated = autoIncrement != 0
                        ? ValueGenerated.OnAdd
                        : default(ValueGenerated?),
                    ComputedColumnSql = hidden != 2L && hidden != 3L
                        ? null
                        : string.Empty,
                    IsStored = hidden != 3L
                        ? default(bool?)
                        : true,
                    Collation = string.Equals(collation, "BINARY", StringComparison.OrdinalIgnoreCase)
                        ? null
                        : collation
                });
        }

        InferClrTypes(connection, table);

        ParseClrDefaults(table);
    }

    private void ParseClrDefaults(DatabaseTable table)
    {
        foreach (var column in table.Columns)
        {
            var defaultValueSql = column.DefaultValueSql;
            defaultValueSql = defaultValueSql?.Trim();
            if (string.IsNullOrEmpty(defaultValueSql))
            {
                continue;
            }

            var typeHint = (Type?)column["ClrType"];
            var type = typeHint is null
                ? _typeMappingSource.FindMapping(column.StoreType!)?.ClrType
                : _typeMappingSource.FindMapping(typeHint, column.StoreType)?.ClrType;
            if (type == null)
            {
                continue;
            }

            Unwrap();

            if (defaultValueSql.Equals("NULL", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (type == typeof(bool)
                && int.TryParse(defaultValueSql, out var intValue))
            {
                column.DefaultValue = intValue != 0;
            }
            else if (type.IsInteger()
                     || type == typeof(float)
                     || type == typeof(double))
            {
                try
                {
                    column.DefaultValue = Convert.ChangeType(defaultValueSql, type);
                }
                catch
                {
                    // Ignored
                }
            }
            else if (defaultValueSql.StartsWith('\'')
                     && defaultValueSql.EndsWith('\''))
            {
                defaultValueSql = defaultValueSql.Substring(1, defaultValueSql.Length - 2);

                if (type == typeof(string))
                {
                    column.DefaultValue = defaultValueSql;
                }
                else if (type == typeof(Guid)
                         && Guid.TryParse(defaultValueSql, out var guid))
                {
                    column.DefaultValue = guid;
                }
                else if (type == typeof(DateTime)
                         && DateTime.TryParse(defaultValueSql, out var dateTime))
                {
                    column.DefaultValue = dateTime;
                }
                else if (type == typeof(DateOnly)
                         && DateOnly.TryParse(defaultValueSql, out var dateOnly))
                {
                    column.DefaultValue = dateOnly;
                }
                else if (type == typeof(TimeOnly)
                         && TimeOnly.TryParse(defaultValueSql, out var timeOnly))
                {
                    column.DefaultValue = timeOnly;
                }
                else if (type == typeof(DateTimeOffset)
                         && DateTimeOffset.TryParse(defaultValueSql, out var dateTimeOffset))
                {
                    column.DefaultValue = dateTimeOffset;
                }
                else if (type == typeof(decimal)
                         && decimal.TryParse(defaultValueSql, out var decimalValue))
                {
                    column.DefaultValue = decimalValue;
                }
            }

            void Unwrap()
            {
                while (defaultValueSql.StartsWith('(') && defaultValueSql.EndsWith(')'))
                {
                    defaultValueSql = (defaultValueSql.Substring(1, defaultValueSql.Length - 2)).Trim();
                }
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual void InferClrTypes(DbConnection connection, DatabaseTable table)
    {
        var command = connection.CreateCommand();
        var commandText = new StringBuilder();
        commandText.Append("SELECT");

        var i = 0;
        var dictionary = new Dictionary<DatabaseColumn, (int offset, Type? defaultClrType)>();
        foreach (var column in table.Columns)
        {
            if (string.Equals(column.StoreType, "BLOB", StringComparison.OrdinalIgnoreCase)
                || string.Equals(column.StoreType, "REAL", StringComparison.OrdinalIgnoreCase))
            {
                // Trust the column type (for perf)
                continue;
            }

            var defaultClrType = _typeMappingSource.FindMapping(column.StoreType!)?.ClrType;
            if (!_defaultClrTypes.Contains(defaultClrType))
            {
                // Handled by a plugin
                continue;
            }

            if (i != 0)
            {
                commandText.Append(",");
            }

            var columnIdentifier = DelimitIdentifier(column.Name);
            commandText
                .Append(" typeof(max(")
                .Append(columnIdentifier)
                .Append(")), min(")
                .Append(columnIdentifier)
                .Append("), max(")
                .Append(columnIdentifier)
                .Append(")");

            dictionary.Add(column, (i, defaultClrType));
            i += 3;
        }

        if (dictionary.Count == 0)
        {
            return;
        }

        commandText
            .Append(" FROM (SELECT * FROM ")
            .Append(DelimitIdentifier(table.Name))
            .Append(" LIMIT 65537)");

        command.CommandText = commandText.ToString();

        _logger.InferringTypes(table.Name);

        using var reader = command.ExecuteReader();
        var read = reader.Read();
        Check.DebugAssert(read, "No results");

        foreach (var (column, (offset, defaultClrTpe)) in dictionary)
        {
            var valueType = reader.GetString(offset + 0);

            var index = column.StoreType!.IndexOf("(", StringComparison.OrdinalIgnoreCase);
            var baseColumnType = index == -1
                ? column.StoreType
                : column.StoreType.Substring(0, index);

            if (string.Equals(valueType, "INTEGER", StringComparison.OrdinalIgnoreCase))
            {
                var min = reader.GetInt64(offset + 1);
                var max = reader.GetInt64(offset + 2);

                if (_boolTypes.Contains(baseColumnType))
                {
                    if (min >= 0L
                        && max <= 1L)
                    {
                        column["ClrType"] = typeof(bool);

                        continue;
                    }

                    _logger.OutOfRangeWarning(column.Name, table.Name, "bool");
                }

                if (_byteTypes.Contains(baseColumnType))
                {
                    if (min >= byte.MinValue
                        && max <= byte.MaxValue)
                    {
                        column["ClrType"] = typeof(byte);

                        continue;
                    }

                    _logger.OutOfRangeWarning(column.Name, table.Name, "byte");
                }

                if (_shortTypes.Contains(baseColumnType))
                {
                    if (min >= short.MinValue
                        && max <= short.MaxValue)
                    {
                        column["ClrType"] = typeof(short);

                        continue;
                    }

                    _logger.OutOfRangeWarning(column.Name, table.Name, "short");
                }

                if (_longTypes.Contains(baseColumnType))
                {
                    if (defaultClrTpe != typeof(long))
                    {
                        column["ClrType"] = typeof(long);
                    }

                    continue;
                }

                if (_sbyteTypes.Contains(baseColumnType))
                {
                    if (min >= sbyte.MinValue
                        && max <= sbyte.MaxValue)
                    {
                        column["ClrType"] = typeof(sbyte);

                        continue;
                    }

                    _logger.OutOfRangeWarning(column.Name, table.Name, "sbyte");
                }

                if (_ushortTypes.Contains(baseColumnType))
                {
                    if (min >= ushort.MinValue
                        && max <= ushort.MaxValue)
                    {
                        column["ClrType"] = typeof(ushort);

                        continue;
                    }

                    _logger.OutOfRangeWarning(column.Name, table.Name, "ushort");
                }

                if (_uintTypes.Contains(baseColumnType))
                {
                    if (min >= uint.MinValue
                        && max <= uint.MaxValue)
                    {
                        column["ClrType"] = typeof(uint);

                        continue;
                    }

                    _logger.OutOfRangeWarning(column.Name, table.Name, "uint");
                }

                if (_ulongTypes.Contains(baseColumnType))
                {
                    column["ClrType"] = typeof(ulong);

                    continue;
                }

                if (min < int.MinValue
                    || max > int.MaxValue)
                {
                    if (defaultClrTpe != typeof(long))
                    {
                        column["ClrType"] = typeof(long);
                    }

                    continue;
                }

                column["ClrType"] = typeof(int);

                continue;
            }

            if (string.Equals(valueType, "TEXT", StringComparison.OrdinalIgnoreCase))
            {
                var min = reader.GetString(offset + 1);
                var max = reader.GetString(offset + 2);

                if (Regex.IsMatch(max, @"^\d{4}-\d{2}-\d{2}$", default, TimeSpan.FromMilliseconds(1000.0)))
                {
                    column["ClrType"] = typeof(DateOnly);

                    continue;
                }

                if (Regex.IsMatch(max, @"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}(\.\d{1,7})?$", default, TimeSpan.FromMilliseconds(1000.0)))
                {
                    column["ClrType"] = typeof(DateTime);

                    continue;
                }

                if (Regex.IsMatch(
                        max, @"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}(\.\d{1,7})?[-+]\d{2}:\d{2}$", default,
                        TimeSpan.FromMilliseconds(1000.0)))
                {
                    column["ClrType"] = typeof(DateTimeOffset);

                    continue;
                }

                if (Regex.IsMatch(max, @"^-?\d+\.\d{1,28}$", default, TimeSpan.FromMilliseconds(1000.0)))
                {
                    column["ClrType"] = typeof(decimal);

                    continue;
                }

                if (Regex.IsMatch(
                        max, @"^(\d|[A-F]){8}-(\d|[A-F]){4}-(\d|[A-F]){4}-(\d|[A-F]){4}-(\d|[A-F]){12}$", default,
                        TimeSpan.FromMilliseconds(1000.0)))
                {
                    column["ClrType"] = typeof(Guid);

                    continue;
                }

                if (Regex.IsMatch(max, @"^-?(\d+\.)?\d{2}:\d{2}:\d{2}(\.\d{1,7})?$", default, TimeSpan.FromMilliseconds(1000.0)))
                {
                    if (_timeOnlyTypes.Contains(baseColumnType))
                    {
                        if (TimeSpan.TryParse(min, out var minTimeSpan)
                            && TimeSpan.TryParse(max, out var maxTimeSpan)
                            && minTimeSpan >= TimeOnly.MinValue.ToTimeSpan()
                            && maxTimeSpan <= TimeOnly.MaxValue.ToTimeSpan())
                        {
                            column["ClrType"] = typeof(TimeOnly);

                            continue;
                        }

                        _logger.OutOfRangeWarning(column.Name, table.Name, "TimeOnly");
                    }

                    column["ClrType"] = typeof(TimeSpan);

                    continue;
                }

                if (DateOnly.TryParse(max, out _))
                {
                    _logger.FormatWarning(column.Name, table.Name, "DateOnly");
                }
                else if (DateTime.TryParse(max, out _))
                {
                    _logger.FormatWarning(column.Name, table.Name, "DateTime");
                }
                else if (DateTimeOffset.TryParse(max, out _))
                {
                    _logger.FormatWarning(column.Name, table.Name, "DateTimeOffset");
                }
                else if (decimal.TryParse(max, out _))
                {
                    _logger.FormatWarning(column.Name, table.Name, "decimal");
                }
                else if (Guid.TryParse(max, out _))
                {
                    _logger.FormatWarning(column.Name, table.Name, "Guid");
                }
                else if (TimeSpan.TryParse(max, out _))
                {
                    _logger.FormatWarning(
                        column.Name,
                        table.Name,
                        _timeOnlyTypes.Contains(baseColumnType)
                            ? "TimeOnly"
                            : "TimeSpan");
                }

                if (defaultClrTpe != typeof(string))
                {
                    column["ClrType"] = typeof(string);
                }

                continue;
            }

            if (string.Equals(valueType, "BLOB", StringComparison.OrdinalIgnoreCase))
            {
                if (defaultClrTpe != typeof(byte[]))
                {
                    column["ClrType"] = typeof(byte[]);
                }

                continue;
            }

            if (string.Equals(valueType, "REAL", StringComparison.OrdinalIgnoreCase))
            {
                var min = reader.GetDouble(offset + 1);
                var max = reader.GetDouble(offset + 2);

                if (_floatTypes.Contains(baseColumnType))
                {
                    if (min >= float.MinValue
                        && max <= float.MaxValue)
                    {
                        column["ClrType"] = typeof(float);

                        continue;
                    }

                    _logger.OutOfRangeWarning(column.Name, table.Name, "float");
                }

                if (_decimalTypes.Contains(baseColumnType))
                {
                    column["ClrType"] = typeof(decimal);

                    continue;
                }

                if (defaultClrTpe != typeof(double))
                {
                    column["ClrType"] = typeof(double);
                }

                continue;
            }

            Check.DebugAssert(
                string.Equals(valueType, "NULL", StringComparison.OrdinalIgnoreCase),
                "Unexpected type: " + valueType);

            if (_typesByName.TryGetValue(baseColumnType, out var type))
            {
                Check.DebugAssert(defaultClrTpe != type, "Unnecessary mapping for " + baseColumnType);

                column["ClrType"] = type;

                continue;
            }

            if (baseColumnType.Contains("INT", StringComparison.OrdinalIgnoreCase)
                && !_longTypes.Contains(baseColumnType))
            {
                column["ClrType"] = typeof(int);
            }
        }

        static string DelimitIdentifier(string name)
            => @$"""{name.Replace(@"""", @"""""")}""";
    }

    private void GetPrimaryKey(DbConnection connection, DatabaseTable table)
    {
        using var command = connection.CreateCommand();
        command.CommandText =
            """
SELECT "name"
FROM pragma_index_list(@table)
WHERE "origin" = 'pk'
ORDER BY "seq"
""";

        var parameter = command.CreateParameter();
        parameter.ParameterName = "@table";
        parameter.Value = table.Name;
        command.Parameters.Add(parameter);

        var name = (string?)command.ExecuteScalar();
        if (name == null)
        {
            GetRowidPrimaryKey(connection, table);
            return;
        }

        var primaryKey = new DatabasePrimaryKey
        {
            Table = table, Name = name.StartsWith("sqlite_", StringComparison.Ordinal) ? string.Empty : name
        };

        _logger.PrimaryKeyFound(name, table.Name);

        command.CommandText =
            """
SELECT "name"
FROM pragma_index_info(@index)
ORDER BY "seqno"
""";

        parameter.ParameterName = "@index";
        parameter.Value = name;

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var columnName = reader.GetString(0);
            var column = table.Columns.FirstOrDefault(c => c.Name == columnName)
                ?? table.Columns.FirstOrDefault(c => c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
            Check.DebugAssert(column != null, "column is null.");

            primaryKey.Columns.Add(column);
        }

        table.PrimaryKey = primaryKey;
    }

    private static void GetRowidPrimaryKey(
        DbConnection connection,
        DatabaseTable table)
    {
        using var command = connection.CreateCommand();
        command.CommandText =
            """
SELECT "name"
FROM pragma_table_info(@table)
WHERE "pk" = 1
""";

        var parameter = command.CreateParameter();
        parameter.ParameterName = "@table";
        parameter.Value = table.Name;
        command.Parameters.Add(parameter);

        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return;
        }

        var columnName = reader.GetString(0);
        var column = table.Columns.FirstOrDefault(c => c.Name == columnName)
            ?? table.Columns.FirstOrDefault(c => c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
        Check.DebugAssert(column != null, "column is null.");

        Check.DebugAssert(!reader.Read(), "Unexpected composite primary key.");

        table.PrimaryKey = new DatabasePrimaryKey
        {
            Table = table,
            Name = string.Empty,
            Columns = { column }
        };
    }

    private void GetUniqueConstraints(DbConnection connection, DatabaseTable table)
    {
        using var command1 = connection.CreateCommand();
        command1.CommandText =
            """
SELECT "name"
FROM pragma_index_list(@table)
WHERE "origin" = 'u'
ORDER BY "seq"
""";

        var parameter1 = command1.CreateParameter();
        parameter1.ParameterName = "@table";
        parameter1.Value = table.Name;
        command1.Parameters.Add(parameter1);

        using var reader1 = command1.ExecuteReader();
        while (reader1.Read())
        {
            var constraintName = reader1.GetString(0);
            var uniqueConstraint = new DatabaseUniqueConstraint
            {
                Table = table, Name = constraintName.StartsWith("sqlite_", StringComparison.Ordinal) ? string.Empty : constraintName
            };

            _logger.UniqueConstraintFound(constraintName, table.Name);

            using (var command2 = connection.CreateCommand())
            {
                command2.CommandText =
                    """
SELECT "name"
FROM pragma_index_info(@index)
ORDER BY "seqno"
""";

                var parameter2 = command2.CreateParameter();
                parameter2.ParameterName = "@index";
                parameter2.Value = constraintName;
                command2.Parameters.Add(parameter2);

                using var reader2 = command2.ExecuteReader();
                while (reader2.Read())
                {
                    var columnName = reader2.GetString(0);
                    var column = table.Columns.FirstOrDefault(c => c.Name == columnName)
                        ?? table.Columns.FirstOrDefault(
                            c => c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
                    Check.DebugAssert(column != null, "column is null.");

                    uniqueConstraint.Columns.Add(column);
                }
            }

            table.UniqueConstraints.Add(uniqueConstraint);
        }
    }

    private void GetIndexes(DbConnection connection, DatabaseTable table)
    {
        using var command1 = connection.CreateCommand();
        command1.CommandText =
            """
SELECT "name", "unique"
FROM pragma_index_list(@table)
WHERE "origin" = 'c' AND instr("name", 'sqlite_') <> 1
ORDER BY "seq"
""";

        var parameter1 = command1.CreateParameter();
        parameter1.ParameterName = "@table";
        parameter1.Value = table.Name;
        command1.Parameters.Add(parameter1);

        using var reader1 = command1.ExecuteReader();
        while (reader1.Read())
        {
            var index = new DatabaseIndex
            {
                Table = table,
                Name = reader1.GetString(0),
                IsUnique = reader1.GetBoolean(1)
            };

            _logger.IndexFound(index.Name, table.Name, index.IsUnique);

            using (var command2 = connection.CreateCommand())
            {
                command2.CommandText =
                    """
SELECT "name", "desc"
FROM pragma_index_xinfo(@index)
WHERE key = 1
ORDER BY "seqno"
""";

                var parameter2 = command2.CreateParameter();
                parameter2.ParameterName = "@index";
                parameter2.Value = index.Name;
                command2.Parameters.Add(parameter2);

                using var reader2 = command2.ExecuteReader();
                while (reader2.Read())
                {
                    var name = reader2.GetString(0);
                    var column = table.Columns.FirstOrDefault(c => c.Name == name)
                        ?? table.Columns.FirstOrDefault(c => c.Name.Equals(name, StringComparison.Ordinal));
                    Check.DebugAssert(column != null, "column is null.");

                    index.Columns.Add(column);
                    index.IsDescending.Add(reader2.GetBoolean(1));
                }
            }

            table.Indexes.Add(index);
        }
    }

    private void GetForeignKeys(DbConnection connection, DatabaseTable table, IList<DatabaseTable> tables)
    {
        using var command1 = connection.CreateCommand();
        command1.CommandText =
            """
SELECT DISTINCT "id", "table", "on_delete"
FROM pragma_foreign_key_list(@table)
ORDER BY "id"
""";

        var parameter1 = command1.CreateParameter();
        parameter1.ParameterName = "@table";
        parameter1.Value = table.Name;
        command1.Parameters.Add(parameter1);

        using var reader1 = command1.ExecuteReader();
        while (reader1.Read())
        {
            var id = reader1.GetInt64(0);
            var principalTableName = reader1.GetString(1);
            var onDelete = reader1.GetString(2);
            var principalTable = tables.FirstOrDefault(t => t.Name == principalTableName)
                ?? tables.FirstOrDefault(
                    t => t.Name.Equals(principalTableName, StringComparison.OrdinalIgnoreCase));

            _logger.ForeignKeyFound(table.Name, id, principalTableName, onDelete);

            if (principalTable == null)
            {
                _logger.ForeignKeyReferencesMissingTableWarning(id.ToString(), table.Name, principalTableName);
                continue;
            }

            var foreignKey = new DatabaseForeignKey
            {
                Table = table,
                Name = string.Empty,
                PrincipalTable = principalTable,
                OnDelete = ConvertToReferentialAction(onDelete)
            };

            using var command2 = connection.CreateCommand();
            command2.CommandText =
                """
SELECT "seq", "from", "to"
FROM pragma_foreign_key_list(@table)
WHERE "id" = @id
ORDER BY "seq"
""";

            var parameter2 = command2.CreateParameter();
            parameter2.ParameterName = "@table";
            parameter2.Value = table.Name;
            command2.Parameters.Add(parameter2);

            var parameter3 = command2.CreateParameter();
            parameter3.ParameterName = "@id";
            parameter3.Value = id;
            command2.Parameters.Add(parameter3);

            var invalid = false;

            using (var reader2 = command2.ExecuteReader())
            {
                while (reader2.Read())
                {
                    var columnName = reader2.GetString(1);
                    var column = table.Columns.FirstOrDefault(c => c.Name == columnName)
                        ?? table.Columns.FirstOrDefault(
                            c => c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
                    Check.DebugAssert(column != null, "column is null.");

                    var principalColumnName = reader2.IsDBNull(2) ? null : reader2.GetString(2);
                    DatabaseColumn? principalColumn = null;
                    if (principalColumnName != null)
                    {
                        principalColumn =
                            foreignKey.PrincipalTable.Columns.FirstOrDefault(c => c.Name == principalColumnName)
                            ?? foreignKey.PrincipalTable.Columns.FirstOrDefault(
                                c => c.Name.Equals(principalColumnName, StringComparison.OrdinalIgnoreCase));
                    }
                    else if (principalTable?.PrimaryKey != null)
                    {
                        var seq = reader2.GetInt32(0);
                        principalColumn = principalTable.PrimaryKey.Columns[seq];
                    }

                    if (principalColumn == null)
                    {
                        invalid = true;
                        _logger.ForeignKeyPrincipalColumnMissingWarning(
                            id.ToString(), table.Name, principalColumnName, principalTableName);
                        break;
                    }

                    foreignKey.Columns.Add(column);
                    foreignKey.PrincipalColumns.Add(principalColumn);
                }
            }

            if (!invalid)
            {
                table.ForeignKeys.Add(foreignKey);
            }
        }
    }

    private static ReferentialAction? ConvertToReferentialAction(string value)
        => value switch
        {
            "RESTRICT" => ReferentialAction.Restrict,
            "CASCADE" => ReferentialAction.Cascade,
            "SET NULL" => ReferentialAction.SetNull,
            "SET DEFAULT" => ReferentialAction.SetDefault,
            "NO ACTION" => ReferentialAction.NoAction,
            _ => null
        };
}
