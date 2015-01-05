// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using Microsoft.Data.Entity.Relational.Migrations.Utilities;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Migrations.Builders
{
    public class MigrationBuilder
    {
        private readonly List<MigrationOperation> _operations = new List<MigrationOperation>();

        public virtual IReadOnlyList<MigrationOperation> Operations
        {
            get { return _operations; }
        }

        public virtual void AddOperation([NotNull] MigrationOperation operation)
        {
            Check.NotNull(operation, "operation");

            _operations.Add(operation);
        }

        public virtual void CreateDatabase([NotNull] string databaseName)
        {
            Check.NotEmpty(databaseName, "databaseName");

            AddOperation(new CreateDatabaseOperation(databaseName));
        }

        public virtual void DropDatabase([NotNull] string databaseName)
        {
            Check.NotEmpty(databaseName, "databaseName");

            AddOperation(new DropDatabaseOperation(databaseName));
        }

        public virtual void CreateSequence(
            SchemaQualifiedName sequenceName,
            long startValue = Sequence.DefaultStartValue,
            int incrementBy = Sequence.DefaultIncrement,
            [CanBeNull] long? minValue = null,
            [CanBeNull] long? maxValue = null,
            [CanBeNull] Type type = null)
        {
            AddOperation(new CreateSequenceOperation(sequenceName, startValue, incrementBy, minValue, maxValue, type));
        }

        public virtual void DropSequence(SchemaQualifiedName sequenceName)
        {
            AddOperation(new DropSequenceOperation(sequenceName));
        }

        public virtual void RenameSequence(SchemaQualifiedName sequenceName, [NotNull] string newSequenceName)
        {
            Check.NotEmpty(newSequenceName, "newSequenceName");

            AddOperation(new RenameSequenceOperation(sequenceName, newSequenceName));
        }

        public virtual void MoveSequence(SchemaQualifiedName sequenceName, [NotNull] string newSchema)
        {
            Check.NotEmpty(newSchema, "newSchema");

            AddOperation(new MoveSequenceOperation(sequenceName, newSchema));
        }

        public virtual void AlterSequence(SchemaQualifiedName sequenceName, int newIncrementBy)
        {
            AddOperation(new AlterSequenceOperation(sequenceName, newIncrementBy));
        }

        public virtual TableBuilder<TColumns> CreateTable<TColumns>(SchemaQualifiedName tableName,
            [NotNull] Func<ColumnBuilder, TColumns> columnsSpecFunc)
        {
            Check.NotNull(columnsSpecFunc, "columnsSpecFunc");

            IDictionary<PropertyInfo, Column> propertyInfoToColumnMap;
            var columns = GetColumns(columnsSpecFunc(new ColumnBuilder()), out propertyInfoToColumnMap);
            var createTableOperation = new CreateTableOperation(tableName);

            createTableOperation.Columns.AddRange(columns);

            AddOperation(createTableOperation);

            return new TableBuilder<TColumns>(createTableOperation, propertyInfoToColumnMap);
        }

        public virtual void DropTable(SchemaQualifiedName tableName)
        {
            AddOperation(new DropTableOperation(tableName));
        }

        public virtual void RenameTable(SchemaQualifiedName tableName, [NotNull] string newTableName)
        {
            Check.NotEmpty(newTableName, "newTableName");

            AddOperation(new RenameTableOperation(tableName, newTableName));
        }

        public virtual void MoveTable(SchemaQualifiedName tableName, [NotNull] string newSchema)
        {
            Check.NotEmpty(newSchema, "newSchema");

            AddOperation(new MoveTableOperation(tableName, newSchema));
        }

        public virtual void AddColumn(SchemaQualifiedName tableName, [NotNull] string columnName,
            [NotNull] Func<ColumnBuilder, Column> columnSpecFunc)
        {
            Check.NotEmpty(columnName, "columnName");
            Check.NotNull(columnSpecFunc, "columnSpecFunc");

            var column = columnSpecFunc(new ColumnBuilder());

            column.Name = columnName;

            AddOperation(new AddColumnOperation(tableName, column));
        }

        public virtual void DropColumn(SchemaQualifiedName tableName, [NotNull] string columnName)
        {
            Check.NotEmpty(columnName, "columnName");

            AddOperation(new DropColumnOperation(tableName, columnName));
        }

        public virtual void RenameColumn(SchemaQualifiedName tableName, [NotNull] string columnName,
            [NotNull] string newColumnName)
        {
            Check.NotEmpty(columnName, "columnName");
            Check.NotEmpty(newColumnName, "newColumnName");

            AddOperation(new RenameColumnOperation(tableName, columnName, newColumnName));
        }

        public virtual void AlterColumn(SchemaQualifiedName tableName, [NotNull] string columnName,
            [NotNull] Func<ColumnBuilder, Column> columnSpecFunc)
        {
            Check.NotEmpty(columnName, "columnName");
            Check.NotNull(columnSpecFunc, "columnSpecFunc");

            var newColumn = columnSpecFunc(new ColumnBuilder());

            newColumn.Name = columnName;

            // TODO: Add code to compute the value of isDestructiveChange.
            AddOperation(new AlterColumnOperation(tableName, newColumn, isDestructiveChange: true));
        }

        public virtual void AddDefaultValue(SchemaQualifiedName tableName, [NotNull] string columnName,
            [NotNull] object defaultValue)
        {
            Check.NotEmpty(columnName, "columnName");
            Check.NotNull(defaultValue, "defaultValue");

            AddOperation(new AddDefaultConstraintOperation(tableName, columnName, defaultValue, null));
        }

        public virtual void AddDefaultExpression(SchemaQualifiedName tableName, [NotNull] string columnName,
            [NotNull] string defaultExpression)
        {
            Check.NotEmpty(columnName, "columnName");
            Check.NotEmpty(defaultExpression, "defaultExpression");

            AddOperation(new AddDefaultConstraintOperation(tableName, columnName, null, defaultExpression));
        }

        public virtual void DropDefaultConstraint(SchemaQualifiedName tableName, [NotNull] string columnName)
        {
            Check.NotEmpty(columnName, "columnName");

            AddOperation(new DropDefaultConstraintOperation(tableName, columnName));
        }

        public virtual void AddPrimaryKey(SchemaQualifiedName tableName, [NotNull] string primaryKeyName,
            [NotNull] IReadOnlyList<string> columnNames, bool isClustered)
        {
            Check.NotEmpty(primaryKeyName, "primaryKeyName");
            Check.NotNull(columnNames, "columnNames");

            AddOperation(new AddPrimaryKeyOperation(tableName, primaryKeyName, columnNames, isClustered));
        }

        public virtual void DropPrimaryKey(SchemaQualifiedName tableName, [NotNull] string primaryKeyName)
        {
            Check.NotEmpty(primaryKeyName, "primaryKeyName");

            AddOperation(new DropPrimaryKeyOperation(tableName, primaryKeyName));
        }

        public virtual void AddUniqueConstraint(SchemaQualifiedName tableName, [NotNull] string uniqueConstraintName,
            [NotNull] IReadOnlyList<string> columnNames)
        {
            Check.NotEmpty(uniqueConstraintName, "uniqueConstraintName");
            Check.NotNull(columnNames, "columnNames");

            AddOperation(new AddUniqueConstraintOperation(tableName, uniqueConstraintName, columnNames));
        }

        public virtual void DropUniqueConstraint(SchemaQualifiedName tableName, [NotNull] string uniqueConstraintName)
        {
            Check.NotEmpty(uniqueConstraintName, "uniqueConstraintName");

            AddOperation(new DropUniqueConstraintOperation(tableName, uniqueConstraintName));
        }

        public virtual void AddForeignKey(SchemaQualifiedName tableName, [NotNull] string foreignKeyName,
            [NotNull] IReadOnlyList<string> columnNames, SchemaQualifiedName referencedTableName,
            [NotNull] IReadOnlyList<string> referencedColumnNames, bool cascadeDelete)
        {
            Check.NotEmpty(foreignKeyName, "foreignKeyName");
            Check.NotNull(columnNames, "columnNames");
            Check.NotNull(referencedColumnNames, "referencedColumnNames");

            AddOperation(new AddForeignKeyOperation(tableName, foreignKeyName,
                columnNames, referencedTableName, referencedColumnNames, cascadeDelete));
        }

        public virtual void DropForeignKey(SchemaQualifiedName tableName, [NotNull] string foreignKeyName)
        {
            Check.NotEmpty(foreignKeyName, "foreignKeyName");

            AddOperation(new DropForeignKeyOperation(tableName, foreignKeyName));
        }

        public virtual void CreateIndex(SchemaQualifiedName tableName, [NotNull] string indexName,
            [NotNull] IReadOnlyList<string> columnNames, bool isUnique, bool isClustered)
        {
            Check.NotEmpty(indexName, "indexName");
            Check.NotNull(columnNames, "columnNames");

            AddOperation(new CreateIndexOperation(tableName, indexName, columnNames, isUnique, isClustered));
        }

        public virtual void DropIndex(SchemaQualifiedName tableName, [NotNull] string indexName)
        {
            Check.NotEmpty(indexName, "indexName");

            AddOperation(new DropIndexOperation(tableName, indexName));
        }

        public virtual void RenameIndex(SchemaQualifiedName tableName, [NotNull] string indexName,
            [NotNull] string newIndexName)
        {
            Check.NotEmpty(indexName, "indexName");
            Check.NotEmpty(newIndexName, "newIndexName");

            AddOperation(new RenameIndexOperation(tableName, indexName, newIndexName));
        }

        public virtual void CopyData(
            SchemaQualifiedName sourceTableName, [NotNull] IReadOnlyList<string> sourceColumnNames,
            SchemaQualifiedName targetTableName, [NotNull] IReadOnlyList<string> targetColumnNames)
        {
            Check.NotNull(sourceColumnNames, "sourceColumnNames");
            Check.NotNull(targetColumnNames, "targetColumnNames");

            AddOperation(new CopyDataOperation(sourceTableName, sourceColumnNames, targetTableName, targetColumnNames));
        }

        public virtual void Sql([NotNull] string sql, bool suppressTransaction = false)
        {
            Check.NotEmpty(sql, "sql");

            AddOperation(new SqlOperation(sql) { SuppressTransaction = suppressTransaction });
        }

        private static IReadOnlyList<Column> GetColumns<TColumns>(
            TColumns columnSpec,
            out IDictionary<PropertyInfo, Column> propertyInfoToColumnMap)
        {
            var columns = new List<Column>();
            propertyInfoToColumnMap = new Dictionary<PropertyInfo, Column>();

            foreach (var propertyInfo in columnSpec.GetType().GetNonIndexerProperties())
            {
                var column = propertyInfo.GetValue(columnSpec, null) as Column;

                if (column != null)
                {
                    propertyInfoToColumnMap.Add(propertyInfo, column);

                    if (string.IsNullOrWhiteSpace(column.Name))
                    {
                        column.Name = propertyInfo.Name;
                    }

                    columns.Add(column);
                }
            }

            return columns;
        }
    }
}
