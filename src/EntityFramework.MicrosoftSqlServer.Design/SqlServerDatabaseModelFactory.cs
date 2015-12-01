// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Scaffolding.Internal;
using Microsoft.Data.Entity.Scaffolding.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Scaffolding
{
    public class SqlServerDatabaseModelFactory : IDatabaseModelFactory
    {
        private SqlConnection _connection;
        private TableSelectionSet _tableSelectionSet;
        private DatabaseModel _databaseModel;
        private Dictionary<string, TableModel> _tables;
        private Dictionary<string, ColumnModel> _tableColumns;
        private const int DefaultDateTimePrecision = 7;
        private static readonly ISet<string> _dateTimePrecisionTypes = new HashSet<string> { "datetimeoffset", "datetime2", "time" };

        private static string TableKey(TableModel table) => TableKey(table.Name, table.SchemaName);
        private static string TableKey(string name, string schema = null) => "[" + (schema ?? "") + "].[" + name + "]";
        private static string ColumnKey(TableModel table, string columnName) => TableKey(table) + ".[" + columnName + "]";

        private void ResetState()
        {
            _connection = null;
            _tableSelectionSet = null;
            _databaseModel = new DatabaseModel();
            _tables = new Dictionary<string, TableModel>();
            _tableColumns = new Dictionary<string, ColumnModel>(StringComparer.OrdinalIgnoreCase);
        }

        public virtual DatabaseModel Create(string connectionString, TableSelectionSet tableSelectionSet)
        {
            Check.NotEmpty(connectionString, nameof(connectionString));
            Check.NotNull(tableSelectionSet, nameof(tableSelectionSet));

            ResetState();

            using (_connection = new SqlConnection(connectionString))
            {
                _connection.Open();
                _tableSelectionSet = tableSelectionSet;

                _databaseModel.DatabaseName = _connection.Database;
                // TODO actually load per-user
                _databaseModel.DefaultSchemaName = "dbo";

                GetTables();
                GetColumns();
                GetIndexes();
                GetForeignKeys();
                return _databaseModel;
            }
        }

        private void GetTables()
        {
            var command = _connection.CreateCommand();
            command.CommandText = "SELECT schema_name(t.schema_id) AS [schema], t.name FROM sys.tables AS t " +
                                  $"WHERE t.name <> '{HistoryRepository.DefaultTableName}'";
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var table = new TableModel
                    {
                        SchemaName = reader.GetString(0),
                        Name = reader.GetString(1)
                    };

                    if (_tableSelectionSet.Allows(table.SchemaName, table.Name))
                    {
                        _databaseModel.Tables.Add(table);
                        _tables[TableKey(table)] = table;
                    }
                }
            }
        }

        private void GetColumns()
        {
            var command = _connection.CreateCommand();
            command.CommandText = @"SELECT DISTINCT 
    schema_name(t.schema_id) AS [schema], 
    t.name AS [table], 
    type_name(c.user_type_id) AS [typename],
    c.name AS [column_name], 
    c.column_id AS [ordinal],
    c.is_nullable AS [nullable],
    CAST(ic.key_ordinal AS int) AS [primary_key_ordinal],
	object_definition(c.default_object_id) AS [default_sql],
    CAST(CASE WHEN c.precision <> tp.precision
			THEN c.precision
			ELSE null
		END AS int) AS [precision],
	CAST(CASE WHEN c.scale <> tp.scale
			THEN c.scale
			ELSE null
		END AS int) AS [scale],
    CAST(CASE WHEN c.max_length <> tp.max_length
			THEN c.max_length
			ELSE null
		END AS int) AS [max_length],
    c.is_identity,
    c.is_computed
FROM sys.index_columns ic
	RIGHT JOIN (SELECT * FROM sys.indexes WHERE is_primary_key = 1) AS i ON i.object_id = ic.object_id AND i.index_id = ic.index_id
	RIGHT JOIN sys.columns c ON ic.object_id = c.object_id AND c.column_id = ic.column_id
	RIGHT JOIN sys.types tp ON tp.user_type_id = c.user_type_id
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
                    var scale = reader.IsDBNull(9) ? default(int?) : reader.GetInt32(9);
                    var maxLength = reader.IsDBNull(10) ? default(int?) : reader.GetInt32(10);
                    var precision = reader.IsDBNull(8) ? default(int?) : reader.GetInt32(8);
                    var dateTimePrecision = default(int?);

                    if (dataTypeName == "nvarchar"
                        || dataTypeName == "nchar")
                    {
                        maxLength /= 2;
                    }

                    if (dataTypeName == "decimal"
                        || dataTypeName == "numeric")
                    {
                        // maxlength here represents storage bytes. The server determines this, not the client.
                        maxLength = null;
                    }

                    if (_dateTimePrecisionTypes.Contains(dataTypeName))
                    {
                        dateTimePrecision = scale ?? DefaultDateTimePrecision;
                        scale = null;
                        precision = null;
                    }

                    var isIdentity = !reader.IsDBNull(11) && reader.GetBoolean(11);
                    var isComputed = reader.GetBoolean(12) || dataTypeName == "timestamp";

                    var table = _tables[TableKey(tableName, schemaName)];
                    var column = new ColumnModel
                    {
                        Table = table,
                        DataType = dataTypeName,
                        Name = reader.GetString(3),
                        Ordinal = reader.GetInt32(4) - 1,
                        IsNullable = nullable,
                        PrimaryKeyOrdinal = reader.IsDBNull(6) ? default(int?) : reader.GetInt32(6),
                        DefaultValue = reader.IsDBNull(7) ? null : reader.GetString(7),
                        Precision = precision,
                        Scale = scale,
                        MaxLength = maxLength <= 0 ? default(int?) : maxLength,
                        ValueGenerated = isIdentity ?
                            ValueGenerated.OnAdd :
                            isComputed ?
                                ValueGenerated.OnAddOrUpdate : default(ValueGenerated?)
                    };
                    column.SqlServer().DateTimePrecision = dateTimePrecision;
                    column.SqlServer().IsIdentity = isIdentity;

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
    object_schema_name(i.object_id) AS [schema_name],
    object_name(i.object_id) AS [table_name],
	i.is_unique,
    c.name AS [column_name],
    i.type_desc
FROM sys.indexes i
    inner join sys.index_columns ic  ON i.object_id = ic.object_id AND i.index_id = ic.index_id
    inner join sys.columns c ON ic.object_id = c.object_id AND c.column_id = ic.column_id
WHERE   object_schema_name(i.object_id) <> 'sys' 
    AND i.is_primary_key <> 1
    AND object_name(i.object_id) <> '" + HistoryRepository.DefaultTableName + @"'
ORDER BY i.name, ic.key_ordinal";

            using (var reader = command.ExecuteReader())
            {
                IndexModel index = null;
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
                        TableModel table;
                        if (!_tables.TryGetValue(TableKey(tableName, schemaName), out table))
                        {
                            continue;
                        }

                        index = new IndexModel
                        {
                            Table = table,
                            Name = indexName,
                            IsUnique = reader.GetBoolean(3)
                        };
                        index.SqlServer().IsClustered = reader.GetString(5) == "CLUSTERED";

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
    schema_name(f.schema_id) AS [schema_name],
    object_name(f.parent_object_id) AS table_name,
    object_schema_name(f.referenced_object_id) AS principal_table_schema_name,
    object_name(f.referenced_object_id) AS principal_table_name,
    col_name(fc.parent_object_id, fc.parent_column_id) AS constraint_column_name,
    col_name(fc.referenced_object_id, fc.referenced_column_id) AS referenced_column_name,
    is_disabled,
    delete_referential_action_desc,
    update_referential_action_desc
FROM sys.foreign_keys AS f
INNER JOIN sys.foreign_key_columns AS fc 
   ON f.object_id = fc.constraint_object_id
ORDER BY f.name, fc.constraint_column_id";
            using (var reader = command.ExecuteReader())
            {
                var lastFkName = "";
                ForeignKeyModel fkInfo = null;
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
                        TableModel principalTable;
                        _tables.TryGetValue(TableKey(principalTableName, principalSchemaTableName), out principalTable);

                        fkInfo = new ForeignKeyModel
                        {
                            Table = table,
                            PrincipalTable = principalTable
                        };

                        table.ForeignKeys.Add(fkInfo);
                    }
                    var fromColumnName = reader.GetString(5);
                    var fromColumn = _tableColumns[ColumnKey(fkInfo.Table, fromColumnName)];
                    fkInfo.Columns.Add(fromColumn);

                    if (fkInfo.PrincipalTable != null)
                    {
                        var toColumnName = reader.GetString(6);
                        var toColumn = _tableColumns[ColumnKey(fkInfo.PrincipalTable, toColumnName)];
                        fkInfo.PrincipalColumns.Add(toColumn);
                    }

                    fkInfo.OnDelete = ConvertToReferentialAction(reader.GetString(8));
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
