// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Scaffolding.Internal.Model;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Sqlite;

namespace Microsoft.Data.Entity.Scaffolding.Internal
{
    public class SqliteMetadataReader
    {
        private readonly SqliteReverseTypeMapper _typeMapper;

        public SqliteMetadataReader([NotNull] SqliteReverseTypeMapper typeMapper)
        {
            _typeMapper = typeMapper;
        }

        public virtual DatabaseInfo ReadDatabaseInfo([NotNull] SqliteConnection connection, [CanBeNull] TableSelectionSet tableSelectionSet = null)
        {
            Check.NotNull(connection, nameof(connection));

            var databaseInfo = new DatabaseInfo();
            GetSqliteMaster(connection, databaseInfo, tableSelectionSet ?? TableSelectionSet.InclusiveAll);
            GetColumns(connection, databaseInfo);
            GetIndexes(connection, databaseInfo);

            foreach (var table in databaseInfo.Tables)
            {
                SqliteDmlParser.ParseTableDefinition(databaseInfo, table);
            }

            GetForeignKeys(connection, databaseInfo);
            return databaseInfo;
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

        private void GetColumns(SqliteConnection connection, DatabaseInfo databaseInfo)
        {
            databaseInfo.Columns = new List<ColumnInfo>();

            foreach (var table in databaseInfo.Tables)
            {
                var command = connection.CreateCommand();
                command.CommandText = $"PRAGMA table_info(\"{table.Name.Replace("\"", "\"\"")}\");";

                using (var reader = command.ExecuteReader())
                {
                    var ordinal = 0;
                    while (reader.Read())
                    {
                        var isPk = reader.GetBoolean((int)TableInfoColumns.Pk);
                        var typeName = reader.GetString((int)TableInfoColumns.Type);
                        var notNull = isPk || reader.GetBoolean((int)TableInfoColumns.NotNull);

                        var column = new ColumnInfo
                        {
                            TableName = table.Name,
                            Name = reader.GetString((int)TableInfoColumns.Name),
                            DataType = typeName,
                            ClrType = _typeMapper.GetClrType(typeName, nullable: !notNull),
                            IsPrimaryKey = reader.GetBoolean((int)TableInfoColumns.Pk),
                            IsNullable = !notNull,
                            DefaultValue = reader.GetValue((int)TableInfoColumns.DefaultValue) as string,
                            Ordinal = ordinal++
                        };

                        databaseInfo.Columns.Add(column);
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

        private void GetIndexes(SqliteConnection connection, DatabaseInfo databaseInfo)
        {
            foreach (var index in databaseInfo.Indexes)
            {
                var indexInfo = connection.CreateCommand();
                indexInfo.CommandText = $"PRAGMA index_info(\"{index.Name.Replace("\"", "\"\"")}\");";

                index.Columns = new List<string>();
                using (var reader = indexInfo.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var name = reader.GetValue((int)IndexInfoColumns.Name) as string;
                        if (!string.IsNullOrEmpty(name))
                        {
                            index.Columns.Add(name);
                        }

                        if (!string.IsNullOrEmpty(index.CreateStatement))
                        {
                            var uniqueKeyword = index.CreateStatement.IndexOf("UNIQUE", StringComparison.OrdinalIgnoreCase);
                            var indexKeyword = index.CreateStatement.IndexOf("INDEX", StringComparison.OrdinalIgnoreCase);

                            index.IsUnique = uniqueKeyword > 0 && uniqueKeyword < indexKeyword;
                        }
                    }
                }
            }

            databaseInfo.Indexes = databaseInfo.Indexes.Where(i => i.Columns.Count > 0).ToList();
        }

        private void GetSqliteMaster(SqliteConnection connection, DatabaseInfo databaseInfo, TableSelectionSet tableSelectionSet)
        {
            var command = connection.CreateCommand();
            command.CommandText = "SELECT type, name, sql, tbl_name FROM sqlite_master";
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
                        && tableSelectionSet.Allows(TableSelection.Any, name))
                    {
                        databaseInfo.Tables.Add(new TableInfo
                        {
                            Name = name,
                            CreateStatement = sql
                        });
                    }
                    else if (type == "index" && tableSelectionSet.Allows(TableSelection.Any, tableName))
                    {
                        databaseInfo.Indexes.Add(new IndexInfo
                        {
                            Name = name,
                            TableName = tableName,
                            CreateStatement = sql
                        });
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

        private void GetForeignKeys(SqliteConnection connection, DatabaseInfo databaseInfo)
        {
            foreach (var dependentTable in databaseInfo.Tables)
            {
                var fkList = connection.CreateCommand();
                fkList.CommandText = $"PRAGMA foreign_key_list(\"{dependentTable.Name.Replace("\"", "\"\"")}\");";

                var tableForeignKeys = new Dictionary<int, ForeignKeyInfo>();

                using (var reader = fkList.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = reader.GetInt32((int)ForeignKeyList.Id);
                        var principalTable = reader.GetString((int)ForeignKeyList.Table);
                        ForeignKeyInfo foreignKey;
                        if (!tableForeignKeys.TryGetValue(id, out foreignKey))
                        {
                            foreignKey = new ForeignKeyInfo
                            {
                                TableName = dependentTable.Name,
                                PrincipalTableName = principalTable
                            };
                            tableForeignKeys.Add(id, foreignKey);
                        }
                        foreignKey.From.Add(reader.GetString((int)ForeignKeyList.From));
                        foreignKey.To.Add(reader.GetString((int)ForeignKeyList.To));
                    }
                }

                databaseInfo.ForeignKeys.AddRange(tableForeignKeys.Values);
            }
        }
    }
}
