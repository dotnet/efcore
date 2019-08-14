// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Sqlite.Scaffolding.Internal
{
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
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Scaffolding> logger,
            [NotNull] IRelationalTypeMappingSource typeMappingSource)
        {
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(typeMappingSource, nameof(typeMappingSource));

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
            Check.NotNull(connectionString, nameof(connectionString));
            Check.NotNull(options, nameof(options));

            using (var connection = new SqliteConnection(connectionString))
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

            if (options.Schemas.Any())
            {
                _logger.SchemasNotSupportedWarning();
            }

            var databaseModel = new DatabaseModel();

            var connectionStartedOpen = connection.State == ConnectionState.Open;
            if (!connectionStartedOpen)
            {
                connection.Open();

                SpatialiteLoader.TryLoad(connection);
            }

            try
            {
                databaseModel.DatabaseName = GetDatabaseName(connection);

                foreach (var table in GetTables(connection, options.Tables))
                {
                    table.Database = databaseModel;
                    databaseModel.Tables.Add(table);
                }

                foreach (var table in databaseModel.Tables)
                {
                    foreach (var foreignKey in GetForeignKeys(connection, table, databaseModel.Tables))
                    {
                        foreignKey.Table = table;
                        table.ForeignKeys.Add(foreignKey);
                    }
                }

                var nullableKeyColumns = databaseModel.Tables
                    .Where(t => t.PrimaryKey != null).SelectMany(t => t.PrimaryKey.Columns)
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

        private static string GetDatabaseName(DbConnection connection)
        {
            var name = Path.GetFileNameWithoutExtension(connection.DataSource);
            if (string.IsNullOrEmpty(name))
            {
                name = "Main";
            }

            return name;
        }

        private IEnumerable<DatabaseTable> GetTables(DbConnection connection, IEnumerable<string> tables)
        {
            var tablesToSelect = new HashSet<string>(tables.ToList(), StringComparer.OrdinalIgnoreCase);
            var selectedTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            using (var command = connection.CreateCommand())
            {
                command.CommandText = new StringBuilder()
                    .AppendLine("SELECT \"name\", \"type\"")
                    .AppendLine("FROM \"sqlite_master\"")
                    .Append("WHERE \"type\" IN ('table', 'view') AND instr(\"name\", 'sqlite_') <> 1 AND \"name\" NOT IN ('")
                    .Append(HistoryRepository.DefaultTableName)
                    .Append("', 'ElementaryGeometries', 'geometry_columns', 'geometry_columns_auth', ")
                    .Append("'geometry_columns_field_infos', 'geometry_columns_statistics', 'geometry_columns_time', ")
                    .Append("'spatial_ref_sys', 'spatial_ref_sys_aux', 'SpatialIndex', 'spatialite_history', ")
                    .Append("'sql_statements_log', 'views_geometry_columns', 'views_geometry_columns_auth', ")
                    .Append("'views_geometry_columns_field_infos', 'views_geometry_columns_statistics', ")
                    .Append("'virts_geometry_columns', 'virts_geometry_columns_auth', ")
                    .Append("'geom_cols_ref_sys', 'spatial_ref_sys_all', ")
                    .AppendLine("'virts_geometry_columns_field_infos', 'virts_geometry_columns_statistics');")
                    .ToString();

                using (var reader = command.ExecuteReader())
                {
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
                            ? new DatabaseTable()
                            : new DatabaseView();

                        table.Name = name;

                        foreach (var column in GetColumns(connection, name))
                        {
                            column.Table = table;
                            table.Columns.Add(column);
                        }

                        var primaryKey = GetPrimaryKey(connection, name, table.Columns);
                        if (primaryKey != null)
                        {
                            primaryKey.Table = table;
                            table.PrimaryKey = primaryKey;
                        }

                        foreach (var uniqueConstraints in GetUniqueConstraints(connection, name, table.Columns))
                        {
                            uniqueConstraints.Table = table;
                            table.UniqueConstraints.Add(uniqueConstraints);
                        }

                        foreach (var index in GetIndexes(connection, name, table.Columns))
                        {
                            index.Table = table;
                            table.Indexes.Add(index);
                        }

                        yield return table;
                    }
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

        private IEnumerable<DatabaseColumn> GetColumns(DbConnection connection, string table)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = new StringBuilder()
                    .AppendLine("SELECT \"name\", \"type\", \"notnull\", \"dflt_value\"")
                    .AppendLine("FROM pragma_table_info(@table)")
                    .AppendLine("ORDER BY \"cid\";")
                    .ToString();

                var parameter = command.CreateParameter();
                parameter.ParameterName = "@table";
                parameter.Value = table;
                command.Parameters.Add(parameter);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var columnName = reader.GetString(0);
                        var dataType = reader.GetString(1);
                        var notNull = reader.GetBoolean(2);
                        var defaultValue = !reader.IsDBNull(3)
                            ? FilterClrDefaults(dataType, notNull, reader.GetString(3))
                            : null;

                        _logger.ColumnFound(table, columnName, dataType, notNull, defaultValue);

                        yield return new DatabaseColumn
                        {
                            Name = columnName,
                            StoreType = dataType,
                            IsNullable = !notNull,
                            DefaultValueSql = defaultValue
                        };
                    }
                }
            }
        }

        private string FilterClrDefaults(string dataType, bool notNull, string defaultValue)
        {
            if (string.Equals(defaultValue, "null", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (notNull && defaultValue == "0")
            {
                var normalizedType = _typeMappingSource.FindMapping(dataType).StoreType;
                if (normalizedType == "INTEGER"
                    || normalizedType == "REAL")
                {
                    return null;
                }
            }

            return defaultValue;
        }

        private DatabasePrimaryKey GetPrimaryKey(DbConnection connection, string table, IList<DatabaseColumn> columns)
        {
            var primaryKey = new DatabasePrimaryKey();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = new StringBuilder()
                    .AppendLine("SELECT \"name\"")
                    .AppendLine("FROM pragma_index_list(@table)")
                    .AppendLine("WHERE \"origin\" = 'pk'")
                    .AppendLine("ORDER BY \"seq\";")
                    .ToString();

                var parameter = command.CreateParameter();
                parameter.ParameterName = "@table";
                parameter.Value = table;
                command.Parameters.Add(parameter);

                var name = (string)command.ExecuteScalar();
                if (name == null)
                {
                    return GetRowidPrimaryKey(connection, table, columns);
                }

                if (!name.StartsWith("sqlite_", StringComparison.Ordinal))
                {
                    primaryKey.Name = name;
                }

                _logger.PrimaryKeyFound(name, table);

                command.CommandText = new StringBuilder()
                    .AppendLine("SELECT \"name\"")
                    .AppendLine("FROM pragma_index_info(@index)")
                    .AppendLine("ORDER BY \"seqno\";")
                    .ToString();

                parameter.ParameterName = "@index";
                parameter.Value = name;

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var columnName = reader.GetString(0);
                        var column = columns.FirstOrDefault(c => c.Name == columnName)
                                     ?? columns.FirstOrDefault(c => c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
                        Debug.Assert(column != null, "column is null.");

                        primaryKey.Columns.Add(column);
                    }
                }
            }

            return primaryKey;
        }

        private static DatabasePrimaryKey GetRowidPrimaryKey(
            DbConnection connection,
            string table,
            IList<DatabaseColumn> columns)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = new StringBuilder()
                    .AppendLine("SELECT \"name\"")
                    .AppendLine("FROM pragma_table_info(@table)")
                    .AppendLine("WHERE \"pk\" = 1;")
                    .ToString();

                var parameter = command.CreateParameter();
                parameter.ParameterName = "@table";
                parameter.Value = table;
                command.Parameters.Add(parameter);

                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    var columnName = reader.GetString(0);
                    var column = columns.FirstOrDefault(c => c.Name == columnName)
                                 ?? columns.FirstOrDefault(c => c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
                    Debug.Assert(column != null, "column is null.");

                    Debug.Assert(!reader.Read(), "Unexpected composite primary key.");

                    return new DatabasePrimaryKey
                    {
                        Columns =
                        {
                            column
                        }
                    };
                }
            }
        }

        private IEnumerable<DatabaseUniqueConstraint> GetUniqueConstraints(
            DbConnection connection,
            string table,
            IList<DatabaseColumn> columns)
        {
            using (var command1 = connection.CreateCommand())
            {
                command1.CommandText = new StringBuilder()
                    .AppendLine("SELECT \"name\"")
                    .AppendLine("FROM pragma_index_list(@table)")
                    .AppendLine("WHERE \"origin\" = 'u'")
                    .AppendLine("ORDER BY \"seq\";")
                    .ToString();

                var parameter1 = command1.CreateParameter();
                parameter1.ParameterName = "@table";
                parameter1.Value = table;
                command1.Parameters.Add(parameter1);

                using (var reader1 = command1.ExecuteReader())
                {
                    while (reader1.Read())
                    {
                        var uniqueConstraint = new DatabaseUniqueConstraint();
                        var name = reader1.GetString(0);
                        if (!name.StartsWith("sqlite_", StringComparison.Ordinal))
                        {
                            uniqueConstraint.Name = name;
                        }

                        _logger.UniqueConstraintFound(name, table);

                        using (var command2 = connection.CreateCommand())
                        {
                            command2.CommandText = new StringBuilder()
                                .AppendLine("SELECT \"name\"")
                                .AppendLine("FROM pragma_index_info(@index)")
                                .AppendLine("ORDER BY \"seqno\";")
                                .ToString();

                            var parameter2 = command2.CreateParameter();
                            parameter2.ParameterName = "@index";
                            parameter2.Value = name;
                            command2.Parameters.Add(parameter2);

                            using (var reader2 = command2.ExecuteReader())
                            {
                                while (reader2.Read())
                                {
                                    var columnName = reader2.GetString(0);
                                    var column = columns.FirstOrDefault(c => c.Name == columnName)
                                                 ?? columns.FirstOrDefault(c => c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
                                    Debug.Assert(column != null, "column is null.");

                                    uniqueConstraint.Columns.Add(column);
                                }
                            }
                        }

                        yield return uniqueConstraint;
                    }
                }
            }
        }

        private IEnumerable<DatabaseIndex> GetIndexes(
            DbConnection connection,
            string table,
            IList<DatabaseColumn> columns)
        {
            using (var command1 = connection.CreateCommand())
            {
                command1.CommandText = new StringBuilder()
                    .AppendLine("SELECT \"name\", \"unique\"")
                    .AppendLine("FROM pragma_index_list(@table)")
                    .AppendLine("WHERE \"origin\" = 'c' AND instr(\"name\", 'sqlite_') <> 1")
                    .AppendLine("ORDER BY \"seq\";")
                    .ToString();

                var parameter1 = command1.CreateParameter();
                parameter1.ParameterName = "@table";
                parameter1.Value = table;
                command1.Parameters.Add(parameter1);

                using (var reader1 = command1.ExecuteReader())
                {
                    while (reader1.Read())
                    {
                        var index = new DatabaseIndex
                        {
                            Name = reader1.GetString(0),
                            IsUnique = reader1.GetBoolean(1)
                        };

                        _logger.IndexFound(index.Name, table, index.IsUnique);

                        using (var command2 = connection.CreateCommand())
                        {
                            command2.CommandText = new StringBuilder()
                                .AppendLine("SELECT \"name\"")
                                .AppendLine("FROM pragma_index_info(@index)")
                                .AppendLine("ORDER BY \"seqno\";")
                                .ToString();

                            var parameter2 = command2.CreateParameter();
                            parameter2.ParameterName = "@index";
                            parameter2.Value = index.Name;
                            command2.Parameters.Add(parameter2);

                            using (var reader2 = command2.ExecuteReader())
                            {
                                while (reader2.Read())
                                {
                                    var name = reader2.GetString(0);
                                    var column = columns.FirstOrDefault(c => c.Name == name)
                                                 ?? columns.FirstOrDefault(c => c.Name.Equals(name, StringComparison.Ordinal));
                                    Debug.Assert(column != null, "column is null.");

                                    index.Columns.Add(column);
                                }
                            }
                        }

                        yield return index;
                    }
                }
            }
        }

        private IEnumerable<DatabaseForeignKey> GetForeignKeys(DbConnection connection, DatabaseTable table, IList<DatabaseTable> tables)
        {
            using (var command1 = connection.CreateCommand())
            {
                command1.CommandText = new StringBuilder()
                    .AppendLine("SELECT DISTINCT \"id\", \"table\", \"on_delete\"")
                    .AppendLine("FROM pragma_foreign_key_list(@table)")
                    .AppendLine("ORDER BY \"id\";")
                    .ToString();

                var parameter1 = command1.CreateParameter();
                parameter1.ParameterName = "@table";
                parameter1.Value = table.Name;
                command1.Parameters.Add(parameter1);

                using (var reader1 = command1.ExecuteReader())
                {
                    while (reader1.Read())
                    {
                        var id = reader1.GetInt64(0);
                        var principalTableName = reader1.GetString(1);
                        var onDelete = reader1.GetString(2);
                        var foreignKey = new DatabaseForeignKey
                        {
                            PrincipalTable = tables.FirstOrDefault(t => t.Name == principalTableName)
                                             ?? tables.FirstOrDefault(t => t.Name.Equals(principalTableName, StringComparison.OrdinalIgnoreCase)),
                            OnDelete = ConvertToReferentialAction(onDelete)
                        };

                        _logger.ForeignKeyFound(table.Name, id, principalTableName, onDelete);

                        if (foreignKey.PrincipalTable == null)
                        {
                            _logger.ForeignKeyReferencesMissingTableWarning(id.ToString());
                            continue;
                        }

                        using (var command2 = connection.CreateCommand())
                        {
                            command2.CommandText = new StringBuilder()
                                .AppendLine("SELECT \"from\", \"to\"")
                                .AppendLine("FROM pragma_foreign_key_list(@table)")
                                .AppendLine("WHERE \"id\" = @id")
                                .AppendLine("ORDER BY \"seq\";")
                                .ToString();

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
                                    var columnName = reader2.GetString(0);
                                    var column = table.Columns.FirstOrDefault(c => c.Name == columnName)
                                                 ?? table.Columns.FirstOrDefault(c => c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
                                    Debug.Assert(column != null, "column is null.");

                                    var principalColumnName = reader2.GetString(1);
                                    var principalColumn = foreignKey.PrincipalTable.Columns.FirstOrDefault(c => c.Name == principalColumnName)
                                                          ?? foreignKey.PrincipalTable.Columns.FirstOrDefault(c => c.Name.Equals(principalColumnName, StringComparison.OrdinalIgnoreCase));
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
                                yield return foreignKey;
                            }
                        }
                    }
                }
            }
        }

        private static ReferentialAction? ConvertToReferentialAction(string value)
        {
            switch (value)
            {
                case "RESTRICT":
                    return ReferentialAction.Restrict;

                case "CASCADE":
                    return ReferentialAction.Cascade;

                case "SET NULL":
                    return ReferentialAction.SetNull;

                case "SET DEFAULT":
                    return ReferentialAction.SetDefault;

                case "NO ACTION":
                    return ReferentialAction.NoAction;

                default:
                    Debug.Assert(value == "NONE", "Unexpected value: " + value);
                    return null;
            }
        }
    }
}
