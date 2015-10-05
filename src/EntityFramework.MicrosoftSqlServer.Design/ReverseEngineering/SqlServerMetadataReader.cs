// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Relational.Design;
using Microsoft.Data.Entity.Relational.Design.Model;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Design.ReverseEngineering
{
    public class SqlServerMetadataReader : IMetadataReader
    {
        private SqlConnection _connection;
        private TableSelectionSet _tableSelectionSet;
        private SchemaInfo _schemaInfo;
        private Dictionary<string, Table> _tables;
        private Dictionary<string, Column> _tableColumns;

        private static string TableKey(Table table) => TableKey(table.Name, table.SchemaName);
        private static string TableKey(string name, string schema = null) => "[" + (schema ?? "") + "].[" + name + "]";
        private static string ColumnKey(Table table, string columnName) => TableKey(table) + ".[" + columnName + "]";

        private void ResetState()
        {
            _connection = null;
            _tableSelectionSet = null;
            _schemaInfo = new SchemaInfo();
            _tables = new Dictionary<string, Table>();
            _tableColumns = new Dictionary<string, Column>(StringComparer.OrdinalIgnoreCase);
        }

        public virtual SchemaInfo GetSchema(string connectionString, TableSelectionSet tableSelectionSet)
        {
            Check.NotEmpty(connectionString, nameof(connectionString));
            Check.NotNull(tableSelectionSet, nameof(tableSelectionSet));

            ResetState();

            using (_connection = new SqlConnection(connectionString))
            {
                _connection.Open();
                _tableSelectionSet = tableSelectionSet;

                _schemaInfo.DatabaseName = _connection.DataSource;

                GetTables();
                GetColumns();
                GetIndexes();
                GetForeignKeys();
                return _schemaInfo;
            }
        }

        private void GetTables()
        {
            var command = _connection.CreateCommand();
            command.CommandText = "SELECT schema_name(t.schema_id) as [schema], t.name FROM sys.tables AS t " +
                                  $"WHERE t.name <> '{HistoryRepository.DefaultTableName}'";
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var table = new Table
                    {
                        SchemaName = reader.GetString(0),
                        Name = reader.GetString(1)
                    };

                    if (_tableSelectionSet.Allows(table.SchemaName, table.Name))
                    {
                        _schemaInfo.Tables.Add(table);
                        _tables[TableKey(table)] = table;
                    }
                }
            }
        }

        private void GetColumns()
        {
            var command = _connection.CreateCommand();
            command.CommandText = @"SELECT DISTINCT 
    schema_name(t.schema_id) as [schema], 
    t.name as [table], 
    type_name(c.user_type_id) as [typename],
    c.name as [column_name], 
    c.column_id as [ordinal],
    c.is_nullable as [nullable],
    i.is_primary_key,
	object_definition(c.default_object_id) as [default_sql],
    CAST(c.precision as int) AS [precision],
    CAST(c.scale as int) AS [scale],
    CAST(c.max_length as int) AS [max_length],
    c.is_identity
FROM sys.indexes AS i
	INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
	RIGHT JOIN sys.columns c ON ic.object_id = c.object_id AND c.column_id = ic.column_id
JOIN sys.tables AS t ON t.object_id = c.object_id
WHERE t.name <> '" + HistoryRepository.DefaultTableName + "'";

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var schemaName = reader.GetString(0);
                    var tableName = reader.GetString(1);
                    if (!_tableSelectionSet.Allows(schemaName, tableName))
                    {
                        continue;
                    }

                    var dataTypeName = reader.GetString(2);
                    var nullable = reader.GetBoolean(5);

                    var precision = reader.GetInt32(8); 
                    // TODO make sure DateTime precision works correctly
                    var scale = reader.GetInt32(9);
                    var maxLength = reader.GetInt32(10);

                    if (dataTypeName == "nvarchar"
                        || dataTypeName == "nchar")
                    {
                        maxLength /= 2;
                    }
                    var table = _tables[TableKey(tableName, schemaName)];
                    var column = new Column
                    {
                        Table = table,
                        DataType = dataTypeName,
                        Name = reader.GetString(3),
                        Ordinal = reader.GetInt32(4) - 1,
                        IsNullable = nullable,
                        IsPrimaryKey = !reader.IsDBNull(6) && reader.GetBoolean(6),
                        DefaultValue = reader.IsDBNull(7) ? null : reader.GetString(7),
                        Precision = (precision != 0) ? precision : default(int?),
                        Scale = (scale != 0) ? scale : default(int?),
                        MaxLength = (maxLength > 0) ? maxLength : default(int?),
                        IsIdentity = !reader.IsDBNull(11) && reader.GetBoolean(11)
                    };

                    table.Columns.Add(column);
                    _tableColumns.Add(ColumnKey(table, column.Name), column);
                }
            }
        }

        private void GetIndexes()
        {
            var command = _connection.CreateCommand();
            command.CommandText = @"SELECT 
    i.name AS [index_name],
    schema_name(t.schema_id) as [schema_name],
    t.name AS [table_name],
	i.is_unique,
    c.name AS [column_name]
FROM sys.indexes i
    inner join sys.index_columns ic  ON i.object_id = ic.object_id AND i.index_id = ic.index_id
    inner join sys.columns c ON ic.object_id = c.object_id AND c.column_id = ic.column_id
JOIN sys.tables AS t ON t.object_id = c.object_id
WHERE i.type != 1 AND t.name <> '" + HistoryRepository.DefaultTableName + @"'
ORDER BY i.name, ic.key_ordinal";

            using (var reader = command.ExecuteReader())
            {
                Index index = null;
                while (reader.Read())
                {
                    var indexName = reader.GetString(0);
                    var schemaName = reader.GetString(1);
                    var tableName = reader.GetString(2);

                    if (!_tableSelectionSet.Allows(schemaName, tableName))
                    {
                        continue;
                    }

                    if (index == null
                        || index.Name != indexName)
                    {
                        var table = _tables[TableKey(tableName, schemaName)];
                        index = new Index
                        {
                            Table = table,
                            Name = indexName,
                            IsUnique = reader.GetBoolean(3)
                        };
                        table.Indexes.Add(index);
                    }
                    var columnName = reader.GetString(4);
                    var column = _tableColumns[ColumnKey(index.Table, columnName)];
                    index.Columns.Add(column);
                }
            }
        }

        private void GetForeignKeys()
        {
            var command = _connection.CreateCommand();
            command.CommandText = @"SELECT 
    f.name AS foreign_key_name,
    schema_name(f.schema_id) as [schema_name],
    object_name(f.parent_object_id) AS table_name,
    object_schema_name(f.referenced_object_id) as principal_table_schema_name,
    object_name(f.referenced_object_id) AS principal_table_name,
    col_name(fc.parent_object_id, fc.parent_column_id) AS constraint_column_name,
    col_name(fc.referenced_object_id, fc.referenced_column_id) AS referenced_column_name,
    is_disabled,
    delete_referential_action_desc,
    update_referential_action_desc
FROM sys.foreign_keys AS f
INNER JOIN sys.foreign_key_columns AS fc 
   ON f.object_id = fc.constraint_object_id";
            using (var reader = command.ExecuteReader())
            {
                var lastFkName = "";
                ForeignKey fkInfo = null;
                while (reader.Read())
                {
                    var fkName = reader.GetString(0);
                    var schemaName = reader.GetString(1);
                    var tableName = reader.GetString(2);

                    if (!_tableSelectionSet.Allows(schemaName, tableName))
                    {
                        continue;
                    }
                    if (fkInfo == null
                        || lastFkName != fkName)
                    {
                        lastFkName = fkName;
                        var principalSchemaTableName = reader.GetString(3);
                        var principalTableName = reader.GetString(4);
                        var table = _tables[TableKey(tableName, schemaName)];
                        fkInfo = new ForeignKey
                        {
                            Table = table,
                            PrincipalTable = _tables[TableKey(principalTableName, principalSchemaTableName)]
                        };
                        table.ForeignKeys.Add(fkInfo);
                    }
                    var fromColumnName = reader.GetString(5);
                    var fromColumn = _tableColumns[ColumnKey(fkInfo.Table, fromColumnName)];
                    fkInfo.From.Add(fromColumn);

                    var toColumnName = reader.GetString(6);
                    var toColumn = _tableColumns[ColumnKey(fkInfo.PrincipalTable, toColumnName)];
                    fkInfo.To.Add(toColumn);
                }
            }
        }
    }
}
