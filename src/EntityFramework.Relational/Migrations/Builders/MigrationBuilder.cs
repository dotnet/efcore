// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Relational.Migrations.Operations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Migrations.Builders
{
    public class MigrationBuilder
    {
        private readonly List<MigrationOperation> _operations = new List<MigrationOperation>();

        // TODO: Expose collection directly?
        public virtual IReadOnlyList<MigrationOperation> Operations => _operations;

        // TODO: Hide?
        public virtual void AddOperation([NotNull] MigrationOperation operation) => _operations.Add(operation);

        // TODO: Cycle option?
        public virtual void CreateSequence(
            [NotNull] string name,
            [CanBeNull] string schema = null,
            [CanBeNull] string storeType = null,
            long startValue = Sequence.DefaultStartValue,
            int incrementBy = Sequence.DefaultIncrement,
            [CanBeNull] long? minValue = null,
            [CanBeNull] long? maxValue = null,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null) =>
                AddOperation(
                    new CreateSequenceOperation(
                        name,
                        schema,
                        startValue,
                        incrementBy,
                        minValue,
                        maxValue,
                        storeType,
                        annotations));

        public virtual void DropSequence(
            [NotNull] string name,
            [CanBeNull] string schema = null,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null) =>
                AddOperation(new DropSequenceOperation(name, schema, annotations));

        public virtual void RenameSequence(
            [NotNull] string name,
            [NotNull] string newName,
            [CanBeNull] string schema = null,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null) =>
                AddOperation(new RenameSequenceOperation(name, schema, newName, annotations));

        public virtual void MoveSequence(
            [NotNull] string name,
            [NotNull] string newSchema,
            [CanBeNull] string schema = null,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null) =>
                AddOperation(new MoveSequenceOperation(name, schema, newSchema, annotations));

        // TODO: Type, start?
        public virtual void AlterSequence(
            [NotNull] string name,
            [CanBeNull] string schema = null,
            int incrementBy = Sequence.DefaultIncrement,
            [CanBeNull] long? minValue = null,
            [CanBeNull] long? maxValue = null,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null) =>
                AddOperation(new AlterSequenceOperation(name, schema, incrementBy, minValue, maxValue, annotations));

        public virtual TableBuilder<TColumns> CreateTable<TColumns>(
            [NotNull] string name,
            [NotNull] Func<ColumnBuilder, TColumns> columnBuilder,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null) =>
                CreateTable(name, /*schema:*/ null, columnBuilder, annotations);

        public virtual TableBuilder<TColumns> CreateTable<TColumns>(
            [NotNull] string name,
            [CanBeNull] string schema,
            [NotNull] Func<ColumnBuilder, TColumns> columnsAction,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null)
        {
            Check.NotNull(columnsAction, nameof(columnsAction));

            var createTableOperation = new CreateTableOperation(name, schema, annotations);
            AddOperation(createTableOperation);

            // TODO: Can this be simplified?
            IDictionary<PropertyInfo, ColumnModel> propertyInfoToColumnMap;
            var columns = GetColumns(columnsAction(new ColumnBuilder()), out propertyInfoToColumnMap);
            createTableOperation.Columns.AddRange(columns);

            return new TableBuilder<TColumns>(createTableOperation, propertyInfoToColumnMap);
        }

        private static IReadOnlyList<ColumnModel> GetColumns<TColumns>(
            TColumns columnSpec,
            out IDictionary<PropertyInfo, ColumnModel> propertyInfoToColumnMap)
        {
            var columns = new List<ColumnModel>();
            propertyInfoToColumnMap = new Dictionary<PropertyInfo, ColumnModel>();

            var properties = columnSpec.GetType().GetRuntimeProperties()
                .Where(p => IsPublic(p) && !p.GetIndexParameters().Any());
            foreach (var propertyInfo in properties)
            {
                var column = propertyInfo.GetValue(columnSpec, null) as ColumnModel;

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

        private static bool IsPublic(PropertyInfo property)
        {
            var getter = property.GetMethod;
            var getterAccess = getter == null ? MethodAttributes.Private : (getter.Attributes & MethodAttributes.MemberAccessMask);

            var setter = property.SetMethod;
            var setterAccess = setter == null ? MethodAttributes.Private : (setter.Attributes & MethodAttributes.MemberAccessMask);

            var propertyAccess = getterAccess > setterAccess ? getterAccess : setterAccess;

            return propertyAccess == MethodAttributes.Public;
        }

        public virtual void DropTable(
            [NotNull] string name,
            [CanBeNull] string schema = null,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null) =>
                AddOperation(new DropTableOperation(name, schema, annotations));

        public virtual void RenameTable(
            [NotNull] string name,
            [NotNull] string newName,
            [CanBeNull] string schema = null,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null) =>
                AddOperation(new RenameTableOperation(name, schema, newName, annotations));

        public virtual void MoveTable(
            [NotNull] string name,
            [NotNull] string newSchema,
            [CanBeNull] string schema = null,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null) =>
                AddOperation(new MoveTableOperation(name, schema, newSchema, annotations));

        public virtual void AlterTable(
            [NotNull] string name,
            [CanBeNull] string schema = null,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null) =>
                AddOperation(new AlterTableOperation(name, schema, annotations));

        public virtual void AddColumn(
            [NotNull] string table,
            [NotNull] string name,
            [NotNull] Func<ColumnBuilder, ColumnModel> columnAction,
            [CanBeNull] string schema = null,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null)
        {
            Check.NotNull(columnAction, nameof(columnAction));

            var column = columnAction(new ColumnBuilder());
            column.Name = name;

            AddOperation(new AddColumnOperation(table, schema, column, annotations));
        }

        public virtual void DropColumn(
            [NotNull] string table,
            [NotNull] string name,
            [CanBeNull] string schema = null,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null) =>
                AddOperation(new DropColumnOperation(table, schema, name, annotations));

        public virtual void RenameColumn(
            [NotNull] string table,
            [NotNull] string name,
            [NotNull] string newName,
            [CanBeNull] string schema = null,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null) =>
                AddOperation(new RenameColumnOperation(table, schema, name, newName, annotations));

        public virtual void AlterColumn(
            [NotNull] string table,
            [NotNull] string name,
            [NotNull] Func<ColumnBuilder, ColumnModel> columnAction,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null) =>
                AlterColumn(table, /*schema:*/ null, name, columnAction, annotations);

        public virtual void AlterColumn(
            [NotNull] string table,
            [CanBeNull] string schema,
            [NotNull] string name,
            [NotNull] Func<ColumnBuilder, ColumnModel> columnAction,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null)
        {
            Check.NotNull(columnAction, nameof(columnAction));

            // TODO: Throw on attempts to rename
            var column = columnAction(new ColumnBuilder());
            column.Name = name;

            AddOperation(new AlterColumnOperation(table, schema, column, annotations));
        }

        public virtual void AddPrimaryKey(
            [NotNull] string table,
            [NotNull] string column,
            [CanBeNull] string schema = null,
            [CanBeNull] string name = null,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null) =>
                AddPrimaryKey(table, new[] { column }, schema, name, annotations);

        public virtual void AddPrimaryKey(
            [NotNull] string table,
            [NotNull] IReadOnlyList<string> columns,
            [CanBeNull] string schema = null,
            [CanBeNull] string name = null,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null) =>
                AddOperation(new AddPrimaryKeyOperation(table, schema, name, columns, annotations));

        public virtual void DropPrimaryKey(
            [NotNull] string table,
            [NotNull] string name,
            [CanBeNull] string schema = null,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null) =>
                AddOperation(new DropPrimaryKeyOperation(table, schema, name, annotations));

        public virtual void AddUniqueConstraint(
            [NotNull] string table,
            [NotNull] string column,
            [CanBeNull] string schema = null,
            [CanBeNull] string name = null,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null) =>
                AddUniqueConstraint(table, new[] { column }, schema, name, annotations);

        public virtual void AddUniqueConstraint(
            [NotNull] string table,
            [NotNull] IReadOnlyList<string> columns,
            [CanBeNull] string schema = null,
            [CanBeNull] string name = null,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null) =>
                AddOperation(new AddUniqueConstraintOperation(table, schema, name, columns, annotations));

        public virtual void DropUniqueConstraint(
            [NotNull] string table,
            [NotNull] string name,
            [CanBeNull] string schema = null,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null) =>
                AddOperation(new DropUniqueConstraintOperation(table, schema, name, annotations));

        public virtual void AddForeignKey(
            [NotNull] string dependentTable,
            [NotNull] string dependentColumn,
            [NotNull] string principalTable,
            [CanBeNull] string dependentSchema = null,
            [CanBeNull] string principalSchema = null,
            [CanBeNull] string principalColumn = null,
            bool cascadeDelete = false,
            [CanBeNull] string name = null,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null) =>
                AddForeignKey(
                    dependentTable,
                    new[] { dependentColumn },
                    principalTable,
                    dependentSchema,
                    principalSchema,
                    new[] { principalColumn },
                    cascadeDelete,
                    name,
                    annotations);

        public virtual void AddForeignKey(
            [NotNull] string dependentTable,
            [NotNull] IReadOnlyList<string> dependentColumns,
            [NotNull] string principalTable,
            [CanBeNull] string dependentSchema = null,
            [CanBeNull] string principalSchema = null,
            [CanBeNull] IReadOnlyList<string> principalColumns = null,
            bool cascadeDelete = false,
            [CanBeNull] string name = null,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null) =>
                AddOperation(
                    new AddForeignKeyOperation(
                        dependentTable,
                        dependentSchema,
                        name,
                        dependentColumns,
                        principalTable,
                        principalSchema,
                        principalColumns,
                        cascadeDelete,
                        annotations));

        public virtual void DropForeignKey(
            [NotNull] string table,
            [NotNull] string name,
            [CanBeNull] string schema = null,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null) =>
                AddOperation(new DropForeignKeyOperation(table, schema, name, annotations));

        public virtual void CreateIndex(
            [NotNull] string name,
            [NotNull] string table,
            [NotNull] string column,
            [CanBeNull] string schema = null,
            bool clustered = false,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null) =>
                CreateIndex(name, table, new[] { column }, schema, clustered, annotations);

        // TODO: Is name really required?
        public virtual void CreateIndex(
            [NotNull] string name,
            [NotNull] string table,
            [NotNull] IReadOnlyList<string> columns,
            [CanBeNull] string schema = null,
            bool unique = false,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null) =>
                AddOperation(new CreateIndexOperation(name, table, schema, columns, unique, annotations));

        // TODO: Is table really required?
        public virtual void DropIndex(
            [NotNull] string name,
            [NotNull] string table,
            [CanBeNull] string schema = null,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null) =>
                AddOperation(new DropIndexOperation(name, table, schema, annotations));

        // TODO: Is table really required?
        public virtual void RenameIndex(
            [NotNull] string name,
            [NotNull] string newName,
            [NotNull] string table,
            [CanBeNull] string schema = null,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null) =>
                AddOperation(new RenameIndexOperation(table, schema, name, newName, annotations));

        // TODO: SqlFile, SqlResource
        public virtual void Sql(
            [NotNull] string sql,
            bool suppressTransaction = false,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null) =>
                AddOperation(new SqlOperation(sql, suppressTransaction, annotations));
    }
}
