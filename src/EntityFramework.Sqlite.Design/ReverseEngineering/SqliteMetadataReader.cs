// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Design;
using Microsoft.Data.Entity.Relational.Design.Model;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Internal;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Sqlite;

namespace Microsoft.Data.Entity.Sqlite.Design.ReverseEngineering
{
    public class SqliteMetadataReader : IMetadataReader
    {
        private SqliteConnection _connection;
        private TableSelectionSet _tableSelectionSet;
        private SchemaInfo _schemaInfo;
        private Dictionary<string, Table> _tables;
        private Dictionary<string, string> _indexDefinitions;
        private Dictionary<string, string> _tableDefinitions;
        private Dictionary<string, Column> _tableColumns;

        private static string ColumnKey(Table table, string columnName) => "[" + table.Name + "].[" + columnName + "]";

        private void ResetState()
        {
            _connection = null;
            _tableSelectionSet = null;
            _schemaInfo = new SchemaInfo();
            _tables = new Dictionary<string, Table>(StringComparer.OrdinalIgnoreCase);
            _tableColumns = new Dictionary<string, Column>(StringComparer.OrdinalIgnoreCase);
            _tableDefinitions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _indexDefinitions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public virtual SchemaInfo GetSchema([NotNull] string connectionString, [NotNull] TableSelectionSet tableSelectionSet)
        {
            Check.NotEmpty(connectionString, nameof(connectionString));
            Check.NotNull(tableSelectionSet, nameof(tableSelectionSet));

            ResetState();

            using (_connection = new SqliteConnection(connectionString))
            {
                _connection.Open();
                _tableSelectionSet = tableSelectionSet;

                _schemaInfo.DatabaseName = _connection.DataSource;

                GetSqliteMaster();
                GetColumns();
                GetIndexes();

                foreach (var table in _schemaInfo.Tables)
                {
                    SqliteDmlParser.ParseTableDefinition(table, _tableDefinitions[table.Name]);
                }

                GetForeignKeys();
                return _schemaInfo;
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
                        var table = new Table
                        {
                            Name = name
                        };
                        _schemaInfo.Tables.Add(table);
                        _tables.Add(name, table);
                        _tableDefinitions[name] = sql;
                    }
                    else if (type == "index"
                             && _tables.ContainsKey(tableName))
                    {
                        var table = _tables[tableName];

                        table.Indexes.Add(new Index
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
            foreach (var table in _schemaInfo.Tables)
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

                        var column = new Column
                        {
                            Table = table,
                            Name = reader.GetString((int)TableInfoColumns.Name),
                            DataType = typeName,
                            IsPrimaryKey = reader.GetBoolean((int)TableInfoColumns.Pk),
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
            foreach (var table in _schemaInfo.Tables)
            {
                foreach (var index in table.Indexes)
                {
                    var indexInfo = _connection.CreateCommand();
                    indexInfo.CommandText = $"PRAGMA index_info(\"{index.Name.Replace("\"", "\"\"")}\");";

                    index.Columns = new List<Column>();
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
            foreach (var dependentTable in _schemaInfo.Tables)
            {
                var fkList = _connection.CreateCommand();
                fkList.CommandText = $"PRAGMA foreign_key_list(\"{dependentTable.Name.Replace("\"", "\"\"")}\");";

                var tableForeignKeys = new Dictionary<int, ForeignKey>();

                using (var reader = fkList.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = reader.GetInt32((int)ForeignKeyList.Id);
                        var principalTableName = reader.GetString((int)ForeignKeyList.Table);
    
                        ForeignKey foreignKey;
                        if (!tableForeignKeys.TryGetValue(id, out foreignKey))
                        {
                            Table principalTable;
                            _tables.TryGetValue(principalTableName, out principalTable);
                            foreignKey = new ForeignKey
                            {
                                Table = dependentTable,
                                PrincipalTable = principalTable
                            };
                            tableForeignKeys.Add(id, foreignKey);
                        }

                        var fromColumnName = reader.GetString((int)ForeignKeyList.From);
                        foreignKey.From.Add(_tableColumns[ColumnKey(dependentTable, fromColumnName)]);

                        if (foreignKey.PrincipalTable != null)
                        {
                            var toColumnName = reader.GetString((int)ForeignKeyList.To);
                            Column toColumn;
                            if(!_tableColumns.TryGetValue(ColumnKey(foreignKey.PrincipalTable, toColumnName), out toColumn))
                            {
                                toColumn = new Column { Name = toColumnName };
                            }
                            foreignKey.To.Add(toColumn);
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
}
