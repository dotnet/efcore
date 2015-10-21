// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Scaffolding.Internal;
using Microsoft.Data.Entity.Scaffolding.Metadata;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Sqlite;

namespace Microsoft.Data.Entity.Scaffolding
{
    public class SqliteDatabaseModelFactory : IDatabaseModelFactory
    {
        private SqliteConnection _connection;
        private TableSelectionSet _tableSelectionSet;
        private DatabaseModel _databaseModel;
        private Dictionary<string, TableModel> _tables;
        private Dictionary<string, string> _indexDefinitions;
        private Dictionary<string, string> _tableDefinitions;
        private Dictionary<string, ColumnModel> _tableColumns;

        private static string ColumnKey(TableModel table, string columnName) => "[" + table.Name + "].[" + columnName + "]";

        private void ResetState()
        {
            _connection = null;
            _tableSelectionSet = null;
            _databaseModel = new DatabaseModel();
            _tables = new Dictionary<string, TableModel>(StringComparer.OrdinalIgnoreCase);
            _tableColumns = new Dictionary<string, ColumnModel>(StringComparer.OrdinalIgnoreCase);
            _tableDefinitions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _indexDefinitions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public virtual DatabaseModel Create(
            [NotNull] string connectionString, [NotNull] TableSelectionSet tableSelectionSet)
        {
            Check.NotEmpty(connectionString, nameof(connectionString));
            Check.NotNull(tableSelectionSet, nameof(tableSelectionSet));

            ResetState();

            using (_connection = new SqliteConnection(connectionString))
            {
                _connection.Open();
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

                _databaseModel.DatabaseName = !string.IsNullOrEmpty(databaseName) ? databaseName : _connection.DataSource;

                GetSqliteMaster();
                GetColumns();
                GetIndexes();

                foreach (var table in _databaseModel.Tables)
                {
                    SqliteDmlParser.ParseTableDefinition(table, _tableDefinitions[table.Name]);
                }

                GetForeignKeys();
                return _databaseModel;
            }
        }

        private void GetSqliteMaster()
        {
            var command = _connection.CreateCommand();
            command.CommandText = "SELECT type, name, sql, tbl_name FROM sqlite_master ORDER BY type DESC";
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var type = reader.GetString(0);
                    var name = reader.GetString(1);
                    var sql = reader.GetValue(2) as string; // can be null
                    var tableName = reader.GetString(3);

                    if (type == "table"
                        && name != "sqlite_sequence"
                        && _tableSelectionSet.Allows(name))
                    {
                        var table = new TableModel
                        {
                            Name = name
                        };
                        _databaseModel.Tables.Add(table);
                        _tables.Add(name, table);
                        _tableDefinitions[name] = sql;
                    }
                    else if (type == "index"
                             && _tables.ContainsKey(tableName))
                    {
                        var table = _tables[tableName];

                        table.Indexes.Add(new IndexModel
                        {
                            Name = name,
                            Table = table
                        });

                        _indexDefinitions[name] = sql;
                    }
                }
            }
        }

        private enum TableInfoColumns
        {
            Cid,
            Name,
            Type,
            NotNull,
            DefaultValue,
            Pk
        }

        private void GetColumns()
        {
            foreach (var table in _databaseModel.Tables)
            {
                var command = _connection.CreateCommand();
                command.CommandText = $"PRAGMA table_info(\"{table.Name.Replace("\"", "\"\"")}\");";

                using (var reader = command.ExecuteReader())
                {
                    var ordinal = 0;
                    while (reader.Read())
                    {
                        var isPk = reader.GetBoolean((int)TableInfoColumns.Pk);
                        var typeName = reader.GetString((int)TableInfoColumns.Type);
                        var notNull = isPk || reader.GetBoolean((int)TableInfoColumns.NotNull);

                        var column = new ColumnModel
                        {
                            Table = table,
                            Name = reader.GetString((int)TableInfoColumns.Name),
                            DataType = typeName,
                            PrimaryKeyOrdinal = isPk ? reader.GetInt32((int)TableInfoColumns.Pk) : default(int?),
                            IsNullable = !notNull,
                            DefaultValue = reader.GetValue((int)TableInfoColumns.DefaultValue) as string,
                            Ordinal = ordinal++
                        };

                        table.Columns.Add(column);
                        _tableColumns[ColumnKey(table, column.Name)] = column;
                    }
                }
            }
        }

        private enum IndexInfoColumns
        {
            Seqno,
            Cid,
            Name
        }

        private void GetIndexes()
        {
            foreach (var table in _databaseModel.Tables)
            {
                foreach (var index in table.Indexes)
                {
                    var indexInfo = _connection.CreateCommand();
                    indexInfo.CommandText = $"PRAGMA index_info(\"{index.Name.Replace("\"", "\"\"")}\");";

                    index.Columns = new List<ColumnModel>();
                    using (var reader = indexInfo.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var columnName = reader.GetValue((int)IndexInfoColumns.Name) as string;
                            if (string.IsNullOrEmpty(columnName))
                            {
                                continue;
                            }

                            var column = _tableColumns[ColumnKey(table, columnName)];

                            index.Columns.Add(column);

                            var sql = _indexDefinitions[index.Name];

                            if (!string.IsNullOrEmpty(sql))
                            {
                                var uniqueKeyword = sql.IndexOf("UNIQUE", StringComparison.OrdinalIgnoreCase);
                                var indexKeyword = sql.IndexOf("INDEX", StringComparison.OrdinalIgnoreCase);

                                index.IsUnique = uniqueKeyword > 0 && uniqueKeyword < indexKeyword;
                            }
                        }
                    }
                }
            }
        }

        private enum ForeignKeyList
        {
            Id,
            Seq,
            Table,
            From,
            To,
            OnUpdate,
            OnDelete,
            Match
        }

        private void GetForeignKeys()
        {
            foreach (var dependentTable in _databaseModel.Tables)
            {
                var fkList = _connection.CreateCommand();
                fkList.CommandText = $"PRAGMA foreign_key_list(\"{dependentTable.Name.Replace("\"", "\"\"")}\");";

                var tableForeignKeys = new Dictionary<int, ForeignKeyModel>();

                using (var reader = fkList.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = reader.GetInt32((int)ForeignKeyList.Id);
                        var principalTableName = reader.GetString((int)ForeignKeyList.Table);
    
                        ForeignKeyModel foreignKey;
                        if (!tableForeignKeys.TryGetValue(id, out foreignKey))
                        {
                            TableModel principalTable;
                            _tables.TryGetValue(principalTableName, out principalTable);
                            foreignKey = new ForeignKeyModel
                            {
                                Table = dependentTable,
                                PrincipalTable = principalTable
                            };
                            tableForeignKeys.Add(id, foreignKey);
                        }

                        var fromColumnName = reader.GetString((int)ForeignKeyList.From);
                        foreignKey.Columns.Add(_tableColumns[ColumnKey(dependentTable, fromColumnName)]);

                        if (foreignKey.PrincipalTable != null)
                        {
                            var toColumnName = reader.GetString((int)ForeignKeyList.To);
                            ColumnModel toColumn;
                            if(!_tableColumns.TryGetValue(ColumnKey(foreignKey.PrincipalTable, toColumnName), out toColumn))
                            {
                                toColumn = new ColumnModel { Name = toColumnName };
                            }
                            foreignKey.PrincipalColumns.Add(toColumn);
                        }

                        foreignKey.OnDelete = ConvertToReferentialAction(
                            reader.GetString((int)ForeignKeyList.OnDelete));
                    }
                }

                foreach (var foreignKey in tableForeignKeys)
                {
                    dependentTable.ForeignKeys.Add(foreignKey.Value);
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

                case "SET_NULL":
                    return ReferentialAction.SetNull;

                case "SET_DEFAULT":
                    return ReferentialAction.SetDefault;

                case "NO_ACTION":
                    return ReferentialAction.NoAction;

                default:
                    return null;
            }
        }
    }
}
