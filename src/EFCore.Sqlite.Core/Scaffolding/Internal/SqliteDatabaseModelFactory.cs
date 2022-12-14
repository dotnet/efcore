// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using System.Text;
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
                .SelectMany(t => t.PrimaryKey?.Columns ?? Array.Empty<DatabaseColumn>())
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
            var defaultValue = !reader.IsDBNull(3)
                ? FilterClrDefaults(dataType, notNull, reader.GetString(3))
                : null;
            var hidden = reader.GetInt64(4);

            _logger.ColumnFound(table.Name, columnName, dataType, notNull, defaultValue);

            string? collation = null;
            var autoIncrement = 0;
            if (connection is SqliteConnection sqliteConnection
                && !(table is DatabaseView))
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
                    DefaultValueSql = defaultValue,
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
    }

    private string? FilterClrDefaults(string dataType, bool notNull, string defaultValue)
    {
        if (string.Equals(defaultValue, "null", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (notNull
            && defaultValue == "0"
            && _typeMappingSource.FindMapping(dataType)?.ClrType.IsNumeric() == true)
        {
            return null;
        }

        return defaultValue;
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
