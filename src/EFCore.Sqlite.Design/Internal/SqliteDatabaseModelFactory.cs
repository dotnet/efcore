// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqliteDatabaseModelFactory : IInternalDatabaseModelFactory
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqliteDatabaseModelFactory([NotNull] ILogger<SqliteDatabaseModelFactory> logger)
        {
            Check.NotNull(logger, nameof(logger));

            Logger = logger;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ILogger Logger { get; }

        private DbConnection _connection;
        private TableSelectionSet _tableSelectionSet;
        private DatabaseModel _databaseModel;
        private Dictionary<string, TableModel> _tables;
        private Dictionary<string, ColumnModel> _tableColumns;

        private static string ColumnKey(TableModel table, string columnName)
            => "[" + table.Name + "].[" + columnName + "]";

        private void ResetState()
        {
            _connection = null;
            _tableSelectionSet = null;
            _databaseModel = new DatabaseModel();
            _tables = new Dictionary<string, TableModel>(StringComparer.OrdinalIgnoreCase);
            _tableColumns = new Dictionary<string, ColumnModel>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual DatabaseModel Create(string connectionString, TableSelectionSet tableSelectionSet)
        {
            Check.NotEmpty(connectionString, nameof(connectionString));
            Check.NotNull(tableSelectionSet, nameof(tableSelectionSet));

            using (var connection = new SqliteConnection(connectionString))
            {
                return Create(connection, tableSelectionSet);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual DatabaseModel Create(DbConnection connection, TableSelectionSet tableSelectionSet)
        {
            ResetState();

            _connection = connection;

            var connectionStartedOpen = _connection.State == ConnectionState.Open;
            if (!connectionStartedOpen)
            {
                _connection.Open();
            }
            try
            {
                _tableSelectionSet = tableSelectionSet;

                string databaseName = null;
                try
                {
                    databaseName = Path.GetFileNameWithoutExtension(_connection.DataSource);
                }
                catch (ArgumentException)
                {
                    // graceful fallback
                }

                _databaseModel.DatabaseName = !string.IsNullOrEmpty(databaseName)
                    ? databaseName
                    : _connection.DataSource;

                GetTables();
                GetColumns();
                GetIndexes();
                GetForeignKeys();
                return _databaseModel;
            }
            finally
            {
                if (!connectionStartedOpen)
                {
                    _connection.Close();
                }
            }
        }

        private void GetTables()
        {
            using (var command = _connection.CreateCommand())
            {
                command.CommandText =
                    "SELECT name FROM sqlite_master" +
                    " WHERE type = 'table'" +
                    " AND name <> 'sqlite_sequence'" +
                    $" AND name <> '{HistoryRepository.DefaultTableName}'"; // Interpolation okay; strings

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var name = reader.GetValueOrDefault<string>("name");

                        Logger.LogDebug(
                            RelationalDesignEventId.FoundTable,
                            () => SqliteDesignStrings.FoundTable(name));

                        if (_tableSelectionSet.Allows(name))
                        {
                            var table = new TableModel
                            {
                                Database = _databaseModel,
                                Name = name
                            };

                            _databaseModel.Tables.Add(table);
                            _tables.Add(name, table);
                        }
                        else
                        {
                            Logger.LogDebug(
                                RelationalDesignEventId.TableSkipped,
                                () => SqliteDesignStrings.TableNotInSelectionSet(name));
                        }
                    }
                }
            }
        }

        private void GetColumns()
        {
            foreach (var table in _databaseModel.Tables)
            {
                using (var command = _connection.CreateCommand())
                {
                    command.CommandText = $"PRAGMA table_info(\"{table.Name.Replace("\"", "\"\"")}\");"; // Interpolation okay; strings

                    using (var reader = command.ExecuteReader())
                    {
                        var ordinal = 0;
                        while (reader.Read())
                        {
                            var columnName = reader.GetValueOrDefault<string>("name");
                            var dataType = reader.GetValueOrDefault<string>("type");
                            var primaryKeyOrdinal = reader.GetValueOrDefault<int>("pk");
                            var notNull = reader.GetValueOrDefault<bool>("notnull");
                            var defaultValue = reader.GetValueOrDefault<string>("dflt_value");

                            Logger.LogDebug(
                                RelationalDesignEventId.FoundColumn,
                                () => SqliteDesignStrings.FoundColumn(
                                    table.Name, columnName, dataType, ordinal,
                                    notNull, primaryKeyOrdinal, defaultValue));

                            var isPk = primaryKeyOrdinal != 0;
                            var column = new ColumnModel
                            {
                                Table = table,
                                Name = columnName,
                                DataType = dataType,
                                Ordinal = ordinal++,
                                IsNullable = !notNull && !isPk,
                                PrimaryKeyOrdinal = isPk ? primaryKeyOrdinal : default(int?),
                                DefaultValue = defaultValue
                            };

                            table.Columns.Add(column);
                            _tableColumns[ColumnKey(table, column.Name)] = column;
                        }
                    }
                }
            }
        }

        private void GetIndexes()
        {
            foreach (var table in _databaseModel.Tables)
            {
                using (var indexInfo = _connection.CreateCommand())
                {
                    indexInfo.CommandText = $"PRAGMA index_list(\"{table.Name.Replace("\"", "\"\"")}\");"; // Interpolation okay; strings

                    using (var reader = indexInfo.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var index = new IndexModel
                            {
                                Name = reader.GetValueOrDefault<string>("name"),
                                Table = table,
                                IsUnique = reader.GetValueOrDefault<bool>("unique")
                            };

                            Logger.LogDebug(
                                RelationalDesignEventId.FoundIndex,
                                () => SqliteDesignStrings.FoundIndex(index.Name, table.Name, index.IsUnique));

                            table.Indexes.Add(index);
                        }
                    }

                    foreach (var index in table.Indexes)
                    {
                        var indexColumns = _connection.CreateCommand();
                        indexColumns.CommandText = $"PRAGMA index_info(\"{index.Name.Replace("\"", "\"\"")}\");"; // Interpolation okay; strings

                        index.IndexColumns = new List<IndexColumnModel>();
                        using (var reader = indexColumns.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var columnName = reader.GetValueOrDefault<string>("name");
                                var indexOrdinal = reader.GetValueOrDefault<int>("seqno");

                                Logger.LogDebug(
                                    RelationalDesignEventId.FoundIndexColumn,
                                    () => SqliteDesignStrings.FoundIndexColumn(
                                        index.Name, table.Name, columnName, indexOrdinal));

                                if (string.IsNullOrEmpty(columnName))
                                {
                                    Logger.LogWarning(
                                        SqliteDesignEventId.IndexMissingColumnNameWarning,
                                        () => SqliteDesignStrings.ColumnNameEmptyOnIndex(index.Name, table.Name));
                                    continue;
                                }

                                var column = _tableColumns[ColumnKey(index.Table, columnName)];

                                var indexColumn = new IndexColumnModel
                                {
                                    Ordinal = indexOrdinal,
                                    Column = column
                                };

                                index.IndexColumns.Add(indexColumn);
                            }
                        }
                    }
                }
            }
        }

        private void GetForeignKeys()
        {
            foreach (var dependentTable in _databaseModel.Tables)
            {
                using (var fkList = _connection.CreateCommand())
                {
                    fkList.CommandText = $"PRAGMA foreign_key_list(\"{dependentTable.Name.Replace("\"", "\"\"")}\");"; // Interpolation okay; strings

                    var tableForeignKeys = new Dictionary<int, ForeignKeyModel>();

                    using (var reader = fkList.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var id = reader.GetValueOrDefault<int>("id");
                            var principalTableName = reader.GetValueOrDefault<string>("table");
                            var fromColumnName = reader.GetValueOrDefault<string>("from");
                            var toColumnName = reader.GetValueOrDefault<string>("to");
                            var deleteAction = reader.GetValueOrDefault<string>("on_delete");
                            var fkOrdinal = reader.GetValueOrDefault<int>("seq");

                            Logger.LogDebug(
                                RelationalDesignEventId.FoundForeignKeyColumn,
                                () => SqliteDesignStrings.FoundForeignKeyColumn(
                                    dependentTable.Name, id, principalTableName, fromColumnName,
                                    toColumnName, deleteAction, fkOrdinal));

                            ForeignKeyModel foreignKey;
                            if (!tableForeignKeys.TryGetValue(id, out foreignKey))
                            {
                                TableModel principalTable;
                                if (!_tables.TryGetValue(principalTableName, out principalTable))
                                {
                                    Logger.LogDebug(
                                        RelationalDesignEventId.ForeignKeyReferencesMissingTable,
                                        () => SqliteDesignStrings.PrincipalTableNotFound(
                                            id, dependentTable.Name, principalTableName));
                                    continue;
                                }

                                foreignKey = new ForeignKeyModel
                                {
                                    Table = dependentTable,
                                    PrincipalTable = principalTable,
                                    OnDelete = ConvertToReferentialAction(deleteAction)
                                };
                            }

                            var fkColumn = new ForeignKeyColumnModel
                            {
                                Ordinal = fkOrdinal,
                                Column = _tableColumns[ColumnKey(dependentTable, fromColumnName)]
                            };

                            ColumnModel toColumn;
                            if (!_tableColumns.TryGetValue(ColumnKey(foreignKey.PrincipalTable, toColumnName), out toColumn))
                            {
                                Logger.LogDebug(
                                    SqliteDesignEventId.ForeignKeyReferencesMissingColumn,
                                    () => SqliteDesignStrings.PrincipalColumnNotFound(
                                        id, dependentTable.Name, toColumnName, principalTableName));
                                continue;
                            }
                            fkColumn.PrincipalColumn = toColumn;

                            foreignKey.Columns.Add(fkColumn);

                            if (!tableForeignKeys.ContainsKey(id))
                            {
                                tableForeignKeys.Add(id, foreignKey);
                            }
                        }
                    }

                    foreach (var foreignKey in tableForeignKeys)
                    {
                        dependentTable.ForeignKeys.Add(foreignKey.Value);
                    }
                }
            }
        }

        private static ReferentialAction? ConvertToReferentialAction(string onDeleteAction)
        {
            switch (onDeleteAction.ToUpperInvariant())
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
                    return null;
            }
        }
    }
}
