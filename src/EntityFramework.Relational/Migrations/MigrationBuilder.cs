// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Migrations.Operations;
using Microsoft.Data.Entity.Migrations.Operations.Builders;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Migrations
{
    public class MigrationBuilder
    {
        public MigrationBuilder([CanBeNull] string activeProvider)
        {
            ActiveProvider = activeProvider;
        }

        public virtual string ActiveProvider { get; }
        public virtual List<MigrationOperation> Operations { get; } = new List<MigrationOperation>();

        public virtual OperationBuilder<AddColumnOperation> AddColumn<T>(
            [NotNull] string name,
            [NotNull] string table,
            [CanBeNull] string type = null,
            [CanBeNull] string schema = null,
            bool nullable = false,
            [CanBeNull] object defaultValue = null,
            [CanBeNull] string defaultValueSql = null,
            [CanBeNull] string computedColumnSql = null)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(table, nameof(table));

            var operation = new AddColumnOperation
            {
                Schema = schema,
                Table = table,
                Name = name,
                ClrType = typeof(T),
                ColumnType = type,
                IsNullable = nullable,
                DefaultValue = defaultValue,
                DefaultValueSql = defaultValueSql,
                ComputedColumnSql = computedColumnSql
            };
            Operations.Add(operation);

            return new OperationBuilder<AddColumnOperation>(operation);
        }

        public virtual OperationBuilder<AddForeignKeyOperation> AddForeignKey(
            [NotNull] string name,
            [NotNull] string table,
            [NotNull] string column,
            [NotNull] string principalTable,
            [CanBeNull] string schema = null,
            [CanBeNull] string principalSchema = null,
            [CanBeNull] string principalColumn = null,
            ReferentialAction onUpdate = ReferentialAction.NoAction,
            ReferentialAction onDelete = ReferentialAction.NoAction)
            => AddForeignKey(
                name,
                table,
                new[] { column },
                principalTable,
                schema,
                principalSchema,
                new[] { principalColumn },
                onUpdate,
                onDelete);

        public virtual OperationBuilder<AddForeignKeyOperation> AddForeignKey(
            [NotNull] string name,
            [NotNull] string table,
            [NotNull] string[] columns,
            [NotNull] string principalTable,
            [CanBeNull] string schema = null,
            [CanBeNull] string principalSchema = null,
            [CanBeNull] string[] principalColumns = null,
            ReferentialAction onUpdate = ReferentialAction.NoAction,
            ReferentialAction onDelete = ReferentialAction.NoAction)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(table, nameof(table));
            Check.NotEmpty(columns, nameof(columns));
            Check.NotEmpty(principalTable, nameof(principalTable));

            var operation = new AddForeignKeyOperation
            {
                Schema = schema,
                Table = table,
                Name = name,
                Columns = columns,
                PrincipalSchema = principalSchema,
                PrincipalTable = principalTable,
                PrincipalColumns = principalColumns,
                OnUpdate = onUpdate,
                OnDelete = onDelete
            };
            Operations.Add(operation);

            return new OperationBuilder<AddForeignKeyOperation>(operation);
        }

        public virtual OperationBuilder<AddPrimaryKeyOperation> AddPrimaryKey(
            [NotNull] string name,
            [NotNull] string table,
            [NotNull] string column,
            [CanBeNull] string schema = null)
            => AddPrimaryKey(
                name,
                table,
                new[] { column },
                schema);

        public virtual OperationBuilder<AddPrimaryKeyOperation> AddPrimaryKey(
            [NotNull] string name,
            [NotNull] string table,
            [NotNull] string[] columns,
            [CanBeNull] string schema = null)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(table, nameof(table));
            Check.NotEmpty(columns, nameof(columns));

            var operation = new AddPrimaryKeyOperation
            {
                Schema = schema,
                Table = table,
                Name = name,
                Columns = columns
            };
            Operations.Add(operation);

            return new OperationBuilder<AddPrimaryKeyOperation>(operation);
        }

        public virtual OperationBuilder<AddUniqueConstraintOperation> AddUniqueConstraint(
            [NotNull] string name,
            [NotNull] string table,
            [NotNull] string column,
            [CanBeNull] string schema = null)
            => AddUniqueConstraint(
                name,
                table,
                new[] { column },
                schema);

        public virtual OperationBuilder<AddUniqueConstraintOperation> AddUniqueConstraint(
            [NotNull] string name,
            [NotNull] string table,
            [NotNull] string[] columns,
            [CanBeNull] string schema = null)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(table, nameof(table));
            Check.NotEmpty(columns, nameof(columns));

            var operation = new AddUniqueConstraintOperation
            {
                Schema = schema,
                Table = table,
                Name = name,
                Columns = columns
            };
            Operations.Add(operation);

            return new OperationBuilder<AddUniqueConstraintOperation>(operation);
        }

        public virtual OperationBuilder<AlterColumnOperation> AlterColumn<T>(
            [NotNull] string name,
            [NotNull] string table,
            [CanBeNull] string type = null,
            [CanBeNull] string schema = null,
            bool nullable = false,
            [CanBeNull] object defaultValue = null,
            [CanBeNull] string defaultValueSql = null,
            [CanBeNull] string computedColumnSql = null)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(table, nameof(table));

            var operation = new AlterColumnOperation
            {
                Schema = schema,
                Table = table,
                Name = name,
                ClrType = typeof(T),
                ColumnType = type,
                IsNullable = nullable,
                DefaultValue = defaultValue,
                DefaultValueSql = defaultValueSql,
                ComputedColumnSql = computedColumnSql
            };
            Operations.Add(operation);

            return new OperationBuilder<AlterColumnOperation>(operation);
        }

        public virtual OperationBuilder<AlterSequenceOperation> AlterSequence(
            [NotNull] string name,
            [CanBeNull] string schema = null,
            [CanBeNull] int incrementBy = 1,
            [CanBeNull] long? minValue = null,
            [CanBeNull] long? maxValue = null,
            bool cyclic = false)
        {
            Check.NotEmpty(name, nameof(name));

            var operation = new AlterSequenceOperation
            {
                Schema = schema,
                Name = name,
                IncrementBy = incrementBy,
                MinValue = minValue,
                MaxValue = maxValue,
                IsCyclic = cyclic
            };
            Operations.Add(operation);

            return new OperationBuilder<AlterSequenceOperation>(operation);
        }

        public virtual OperationBuilder<CreateIndexOperation> CreateIndex(
            [NotNull] string name,
            [NotNull] string table,
            [NotNull] string column,
            [CanBeNull] string schema = null,
            bool unique = false)
            => CreateIndex(
                name,
                table,
                new[] { column },
                schema,
                unique);

        public virtual OperationBuilder<CreateIndexOperation> CreateIndex(
            [NotNull] string name,
            [NotNull] string table,
            [NotNull] string[] columns,
            [CanBeNull] string schema = null,
            bool unique = false)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(table, nameof(table));
            Check.NotEmpty(columns, nameof(columns));

            var operation = new CreateIndexOperation
            {
                Schema = schema,
                Table = table,
                Name = name,
                Columns = columns,
                IsUnique = unique
            };
            Operations.Add(operation);

            return new OperationBuilder<CreateIndexOperation>(operation);
        }

        public virtual OperationBuilder<EnsureSchemaOperation> EnsureSchema(
            [NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            var operation = new EnsureSchemaOperation
            {
                Name = name
            };
            Operations.Add(operation);

            return new OperationBuilder<EnsureSchemaOperation>(operation);
        }

        public virtual OperationBuilder<CreateSequenceOperation> CreateSequence(
            [NotNull] string name,
            [CanBeNull] string schema = null,
            [CanBeNull] long startValue = 1L,
            [CanBeNull] int incrementBy = 1,
            [CanBeNull] long? minValue = null,
            [CanBeNull] long? maxValue = null,
            bool cyclic = false)
            => CreateSequence<long>(name, schema, startValue, incrementBy, minValue, maxValue, cyclic);

        public virtual OperationBuilder<CreateSequenceOperation> CreateSequence<T>(
            [NotNull] string name,
            [CanBeNull] string schema = null,
            [CanBeNull] long startValue = 1L,
            [CanBeNull] int incrementBy = 1,
            [CanBeNull] long? minValue = null,
            [CanBeNull] long? maxValue = null,
            bool cyclic = false)
        {
            Check.NotEmpty(name, nameof(name));

            var operation = new CreateSequenceOperation
            {
                Schema = schema,
                Name = name,
                ClrType = typeof(T),
                StartValue = startValue,
                IncrementBy = incrementBy,
                MinValue = minValue,
                MaxValue = maxValue,
                IsCyclic = cyclic
            };
            Operations.Add(operation);

            return new OperationBuilder<CreateSequenceOperation>(operation);
        }

        public virtual CreateTableBuilder<TColumns> CreateTable<TColumns>(
            [NotNull] string name,
            [NotNull] Func<ColumnsBuilder, TColumns> columns,
            [CanBeNull] string schema = null,
            [CanBeNull] Action<CreateTableBuilder<TColumns>> constraints = null)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(columns, nameof(columns));

            var createTableOperation = new CreateTableOperation
            {
                Schema = schema,
                Name = name
            };

            var columnsBuilder = new ColumnsBuilder(createTableOperation);
            var columnsObject = columns(columnsBuilder);
            var columnMap = new Dictionary<PropertyInfo, AddColumnOperation>();
            foreach (var property in typeof(TColumns).GetTypeInfo().DeclaredProperties)
            {
                var addColumnOperation = ((IAccessor<AddColumnOperation>)property.GetMethod.Invoke(columnsObject, null)).Service;
                if (addColumnOperation.Name == null)
                {
                    addColumnOperation.Name = property.Name;
                }
                columnMap.Add(property, addColumnOperation);
            }

            var builder = new CreateTableBuilder<TColumns>(createTableOperation, columnMap);
            if (constraints != null)
            {
                constraints(builder);
            }

            Operations.Add(createTableOperation);

            return builder;
        }

        public virtual OperationBuilder<DropColumnOperation> DropColumn(
            [NotNull] string name,
            [NotNull] string table,
            [CanBeNull] string schema = null)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(table, nameof(table));

            var operation = new DropColumnOperation
            {
                Schema = schema,
                Table = table,
                Name = name
            };
            Operations.Add(operation);

            return new OperationBuilder<DropColumnOperation>(operation);
        }

        public virtual OperationBuilder<DropForeignKeyOperation> DropForeignKey(
            [NotNull] string name,
            [NotNull] string table,
            [CanBeNull] string schema = null)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(table, nameof(table));

            var operation = new DropForeignKeyOperation
            {
                Schema = schema,
                Table = table,
                Name = name
            };
            Operations.Add(operation);

            return new OperationBuilder<DropForeignKeyOperation>(operation);
        }

        public virtual OperationBuilder<DropIndexOperation> DropIndex(
            [NotNull] string name,
            [CanBeNull] string table = null,
            [CanBeNull] string schema = null)
        {
            Check.NotEmpty(name, nameof(name));

            var operation = new DropIndexOperation
            {
                Schema = schema,
                Table = table,
                Name = name
            };
            Operations.Add(operation);

            return new OperationBuilder<DropIndexOperation>(operation);
        }

        public virtual OperationBuilder<DropPrimaryKeyOperation> DropPrimaryKey(
            [NotNull] string name,
            [NotNull] string table,
            [CanBeNull] string schema = null)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(table, nameof(table));

            var operation = new DropPrimaryKeyOperation
            {
                Schema = schema,
                Table = table,
                Name = name
            };
            Operations.Add(operation);

            return new OperationBuilder<DropPrimaryKeyOperation>(operation);
        }

        public virtual OperationBuilder<DropSchemaOperation> DropSchema(
            [NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            var operation = new DropSchemaOperation
            {
                Name = name
            };
            Operations.Add(operation);

            return new OperationBuilder<DropSchemaOperation>(operation);
        }

        public virtual OperationBuilder<DropSequenceOperation> DropSequence(
            [NotNull] string name,
            [CanBeNull] string schema = null)
        {
            Check.NotEmpty(name, nameof(name));

            var operation = new DropSequenceOperation
            {
                Schema = schema,
                Name = name
            };
            Operations.Add(operation);

            return new OperationBuilder<DropSequenceOperation>(operation);
        }

        public virtual OperationBuilder<DropTableOperation> DropTable(
            [NotNull] string name,
            [CanBeNull] string schema = null)
        {
            Check.NotEmpty(name, nameof(name));

            var operation = new DropTableOperation
            {
                Schema = schema,
                Name = name
            };
            Operations.Add(operation);

            return new OperationBuilder<DropTableOperation>(operation);
        }

        public virtual OperationBuilder<DropUniqueConstraintOperation> DropUniqueConstraint(
            [NotNull] string name,
            [NotNull] string table,
            [CanBeNull] string schema = null)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(table, nameof(table));

            var operation = new DropUniqueConstraintOperation
            {
                Schema = schema,
                Table = table,
                Name = name
            };
            Operations.Add(operation);

            return new OperationBuilder<DropUniqueConstraintOperation>(operation);
        }

        public virtual OperationBuilder<RenameColumnOperation> RenameColumn(
            [NotNull] string name,
            [NotNull] string table,
            [NotNull] string newName,
            [CanBeNull] string schema = null)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(table, nameof(table));
            Check.NotEmpty(newName, nameof(newName));

            var operation = new RenameColumnOperation
            {
                Name = name,
                Schema = schema,
                Table = table,
                NewName = newName
            };
            Operations.Add(operation);

            return new OperationBuilder<RenameColumnOperation>(operation);
        }

        public virtual OperationBuilder<RenameIndexOperation> RenameIndex(
            [NotNull] string name,
            [NotNull] string newName,
            [CanBeNull] string table = null,
            [CanBeNull] string schema = null)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(newName, nameof(newName));

            var operation = new RenameIndexOperation
            {
                Schema = schema,
                Table = table,
                Name = name,
                NewName = newName
            };
            Operations.Add(operation);

            return new OperationBuilder<RenameIndexOperation>(operation);
        }

        public virtual OperationBuilder<RenameSequenceOperation> RenameSequence(
            [NotNull] string name,
            [CanBeNull] string schema = null,
            [CanBeNull] string newName = null,
            [CanBeNull] string newSchema = null)
        {
            Check.NotEmpty(name, nameof(name));

            var operation = new RenameSequenceOperation
            {
                Name = name,
                Schema = schema,
                NewName = newName,
                NewSchema = newSchema
            };
            Operations.Add(operation);

            return new OperationBuilder<RenameSequenceOperation>(operation);
        }

        public virtual OperationBuilder<RenameTableOperation> RenameTable(
            [NotNull] string name,
            [CanBeNull] string schema = null,
            [CanBeNull] string newName = null,
            [CanBeNull] string newSchema = null)
        {
            Check.NotEmpty(name, nameof(name));

            var operation = new RenameTableOperation
            {
                Schema = schema,
                Name = name,
                NewName = newName,
                NewSchema = newSchema
            };
            Operations.Add(operation);

            return new OperationBuilder<RenameTableOperation>(operation);
        }

        public virtual OperationBuilder<RestartSequenceOperation> RestartSequence(
            [NotNull] string name,
            long startValue = 1L,
            [CanBeNull] string schema = null)
        {
            Check.NotEmpty(name, nameof(name));

            var operation = new RestartSequenceOperation
            {
                Name = name,
                Schema = schema,
                StartValue = startValue
            };
            Operations.Add(operation);

            return new OperationBuilder<RestartSequenceOperation>(operation);
        }

        public virtual OperationBuilder<SqlOperation> Sql(
            [NotNull] string sql,
            bool suppressTransaction = false)
        {
            Check.NotEmpty(sql, nameof(sql));

            var operation = new SqlOperation
            {
                Sql = sql
            };
            Operations.Add(operation);

            return new OperationBuilder<SqlOperation>(operation);
        }
    }
}
