// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Operations;
using Microsoft.Data.Entity.Relational.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Migrations.Sql
{
    public abstract class MigrationSqlGenerator : IMigrationSqlGenerator
    {
        private readonly IUpdateSqlGenerator _sql;

        protected MigrationSqlGenerator([NotNull] IUpdateSqlGenerator sqlGenerator)
        {
            Check.NotNull(sqlGenerator, nameof(sqlGenerator));

            _sql = sqlGenerator;
        }

        public virtual IReadOnlyList<SqlBatch> Generate(
            IReadOnlyList<MigrationOperation> operations,
            IModel model = null)
        {
            Check.NotNull(operations, nameof(operations));

            var builder = new SqlBatchBuilder();
            foreach (var operation in operations)
            {
                // TODO: Too magic?
                ((dynamic)this).Generate((dynamic)operation, model, builder);
                builder.AppendLine(_sql.BatchCommandSeparator);
            }

            builder.EndBatch();

            return builder.SqlBatches;
        }

        public virtual void Generate(
            [NotNull] MigrationOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            throw new InvalidOperationException(Strings.UnknownOperation(GetType().Name, operation.GetType().Name));
        }

        public virtual void Generate(
            [NotNull] AddColumnOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER TABLE ")
                .Append(_sql.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" ADD ");
            ColumnDefinition(operation, model, builder);
        }

        public virtual void Generate(
            [NotNull] AddForeignKeyOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER TABLE ")
                .Append(_sql.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" ADD ");
            ForeignKeyConstraint(operation, model, builder);
        }

        public virtual void Generate(
            [NotNull] AddPrimaryKeyOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER TABLE ")
                .Append(_sql.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" ADD ");
            PrimaryKeyConstraint(operation, model, builder);
        }

        public virtual void Generate(
            [NotNull] AddUniqueConstraintOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER TABLE ")
                .Append(_sql.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" ADD ");
            UniqueConstraint(operation, model, builder);
        }

        public abstract void Generate(
            [NotNull] AlterColumnOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder);

        public abstract void Generate(
            [NotNull] RenameIndexOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder);

        public virtual void Generate(
            [NotNull] AlterSequenceOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER SEQUENCE ")
                .Append(_sql.DelimitIdentifier(operation.Name, operation.Schema));

            SequenceOptions(operation, model, builder);
        }

        public abstract void Generate(
            [NotNull] RenameTableOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder);

        public virtual void Generate(
            [NotNull] CreateIndexOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.Append("CREATE ");

            if (operation.IsUnique)
            {
                builder.Append("UNIQUE ");
            }

            IndexTraits(operation, model, builder);

            builder
                .Append("INDEX ")
                .Append(_sql.DelimitIdentifier(operation.Name))
                .Append(" ON ")
                .Append(_sql.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" (")
                .Append(ColumnList(operation.Columns))
                .Append(")");
        }

        public abstract void Generate(
            [NotNull] CreateSchemaOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder);

        public virtual void Generate(
            [NotNull] CreateSequenceOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("CREATE SEQUENCE ")
                .Append(_sql.DelimitIdentifier(operation.Name, operation.Schema));

            if (operation.Type != null)
            {
                builder
                    .Append(" AS ")
                    .Append(operation.Type);
            }

            builder
                .Append(" START WITH ")
                .Append(_sql.GenerateLiteral(operation.StartWith));
            SequenceOptions(operation, model, builder);
        }

        public virtual void Generate(
            [NotNull] CreateTableOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("CREATE TABLE ")
                .Append(_sql.DelimitIdentifier(operation.Name, operation.Schema))
                .AppendLine(" (");

            using (builder.Indent())
            {
                for (var i = 0; i < operation.Columns.Count; i++)
                {
                    var column = operation.Columns[i];
                    ColumnDefinition(column, model, builder);

                    if (i != operation.Columns.Count - 1)
                    {
                        builder.AppendLine(",");
                    }
                }

                if (operation.PrimaryKey != null)
                {
                    builder.AppendLine(",");
                    PrimaryKeyConstraint(operation.PrimaryKey, model, builder);
                }

                foreach (var uniqueConstraint in operation.UniqueConstraints)
                {
                    builder.AppendLine(",");
                    UniqueConstraint(uniqueConstraint, model, builder);
                }

                foreach (var foreignKey in operation.ForeignKeys)
                {
                    builder.AppendLine(",");
                    ForeignKeyConstraint(foreignKey, model, builder);
                }

                builder.AppendLine();
            }

            builder.Append(")");
        }

        public virtual void Generate(
            [NotNull] DropColumnOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER TABLE ")
                .Append(_sql.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" DROP COLUMN ")
                .Append(_sql.DelimitIdentifier(operation.Name));
        }

        public virtual void Generate(
            [NotNull] DropForeignKeyOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER TABLE ")
                .Append(_sql.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" DROP CONSTRAINT ")
                .Append(_sql.DelimitIdentifier(operation.Name));
        }

        public abstract void Generate(
            [NotNull] DropIndexOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder);

        public virtual void Generate(
            [NotNull] DropPrimaryKeyOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER TABLE ")
                .Append(_sql.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" DROP CONSTRAINT ")
                .Append(_sql.DelimitIdentifier(operation.Name));
        }

        public virtual void Generate(
            [NotNull] DropSchemaOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("DROP SCHEMA ")
                .Append(_sql.DelimitIdentifier(operation.Name));
        }

        public virtual void Generate(
            [NotNull] DropSequenceOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("DROP SEQUENCE ")
                .Append(_sql.DelimitIdentifier(operation.Name, operation.Schema));
        }

        public virtual void Generate(
            [NotNull] DropTableOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("DROP TABLE ")
                .Append(_sql.DelimitIdentifier(operation.Name, operation.Schema));
        }

        public virtual void Generate(
            [NotNull] DropUniqueConstraintOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER TABLE ")
                .Append(_sql.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" DROP CONSTRAINT ")
                .Append(_sql.DelimitIdentifier(operation.Name));
        }

        public abstract void Generate(
            [NotNull] RenameColumnOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder);

        public abstract void Generate(
            [NotNull] RenameSequenceOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder);

        public virtual void Generate(
            [NotNull] RestartSequenceOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER SEQUENCE ")
                .Append(_sql.DelimitIdentifier(operation.Name, operation.Schema))
                .Append(" RESTART WITH ")
                .Append(_sql.GenerateLiteral(operation.RestartWith));
        }

        public virtual void Generate(
            [NotNull] SqlOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.Append(operation.Sql, operation.SuppressTransaction);
        }

        public virtual void SequenceOptions(
            [NotNull] AlterSequenceOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder) =>
                SequenceOptions(
                    operation.Schema,
                    operation.Name,
                    operation.IncrementBy,
                    operation.MinValue,
                    operation.MaxValue,
                    operation.Cycle,
                    model,
                    builder);

        public virtual void SequenceOptions(
            [NotNull] CreateSequenceOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder) =>
                SequenceOptions(
                    operation.Schema,
                    operation.Name,
                    operation.IncrementBy,
                    operation.MinValue,
                    operation.MaxValue,
                    operation.Cycle,
                    model,
                    builder);

        private void SequenceOptions(
            [CanBeNull] string schema,
            [NotNull] string name,
            [NotNull] int? increment,
            long? minimumValue,
            long? maximumValue,
            [NotNull] bool? cycle,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(increment, nameof(increment));
            Check.NotNull(cycle, nameof(cycle));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(" INCREMENT BY ")
                .Append(_sql.GenerateLiteral(increment));

            if (minimumValue != null)
            {
                builder
                    .Append(" MINVALUE ")
                    .Append(_sql.GenerateLiteral(minimumValue));
            }
            else
            {
                builder.Append(" NO MINVALUE");
            }

            if (maximumValue != null)
            {
                builder
                    .Append(" MAXVALUE ")
                    .Append(_sql.GenerateLiteral(maximumValue));
            }
            else
            {
                builder.Append(" NO MAXVALUE");
            }

            builder.Append(cycle.Value ? " CYCLE" : " NO CYCLE");
        }

        public virtual void ColumnDefinition(
            [NotNull] AddColumnOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder) =>
                ColumnDefinition(
                    operation.Schema,
                    operation.Table,
                    operation.Name,
                    operation.Type,
                    operation.IsNullable,
                    operation.DefaultValue,
                    operation.DefaultValueSql,
                    operation,
                    model,
                    builder);

        public virtual void ColumnDefinition(
            [CanBeNull] string schema,
            [CanBeNull] string table,
            [NotNull] string name,
            [NotNull] string type,
            bool nullable,
            [CanBeNull] object defaultValue,
            [CanBeNull] string defaultExpression,
            [NotNull] IAnnotatable annotatable,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(type, nameof(type));
            Check.NotNull(annotatable, nameof(annotatable));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(_sql.DelimitIdentifier(name))
                .Append(" ")
                .Append(type);

            if (!nullable)
            {
                builder.Append(" NOT NULL");
            }

            if (defaultExpression != null)
            {
                builder
                    .Append(" DEFAULT (")
                    .Append(defaultExpression)
                    .Append(")");
            }
            else if (defaultValue != null)
            {
                builder
                    .Append(" DEFAULT ")
                    .Append(_sql.GenerateLiteral((dynamic)defaultValue));
            }
        }

        public virtual void ForeignKeyConstraint(
            [NotNull] AddForeignKeyOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (operation.Name != null)
            {
                builder
                    .Append("CONSTRAINT ")
                    .Append(_sql.DelimitIdentifier(operation.Name))
                    .Append(" ");
            }

            builder
                .Append("FOREIGN KEY (")
                .Append(ColumnList(operation.Columns))
                .Append(") REFERENCES ")
                .Append(_sql.DelimitIdentifier(operation.ReferencedTable, operation.ReferencedSchema));

            if (operation.ReferencedColumns != null
                && operation.ReferencedColumns.Length != 0)
            {
                builder
                    .Append(" (")
                    .Append(ColumnList(operation.ReferencedColumns))
                    .Append(")");
            }

            if (operation.OnUpdate != ReferentialAction.NoAction)
            {
                builder.Append(" ON UPDATE ");
                ForeignKeyAction(operation.OnUpdate, builder);
            }

            if (operation.OnDelete != ReferentialAction.NoAction)
            {
                builder.Append(" ON DELETE ");
                ForeignKeyAction(operation.OnDelete, builder);
            }
        }

        public virtual void PrimaryKeyConstraint(
            [NotNull] AddPrimaryKeyOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (operation.Name != null)
            {
                builder
                    .Append("CONSTRAINT ")
                    .Append(_sql.DelimitIdentifier(operation.Name))
                    .Append(" ");
            }

            builder
                .Append("PRIMARY KEY ");

            IndexTraits(operation, model, builder);

            builder.Append("(")
                .Append(ColumnList(operation.Columns))
                .Append(")");
        }

        public virtual void UniqueConstraint(
            [NotNull] AddUniqueConstraintOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (operation.Name != null)
            {
                builder
                    .Append("CONSTRAINT ")
                    .Append(_sql.DelimitIdentifier(operation.Name))
                    .Append(" ");
            }

            builder
                .Append("UNIQUE ");

            IndexTraits(operation, model, builder);

            builder.Append("(")
                .Append(ColumnList(operation.Columns))
                .Append(")");
        }

        public virtual void IndexTraits(
            [NotNull] MigrationOperation operation,
            [CanBeNull] IModel model,
            [NotNull] SqlBatchBuilder builder)
        {
        }

        public virtual void ForeignKeyAction(
            ReferentialAction referentialAction,
            [NotNull] SqlBatchBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            switch (referentialAction)
            {
                case ReferentialAction.Restrict:
                    builder.Append("RESTRICT");
                    break;
                case ReferentialAction.Cascade:
                    builder.Append("CASCADE");
                    break;
                case ReferentialAction.SetNull:
                    builder.Append("SET NULL");
                    break;
                case ReferentialAction.SetDefault:
                    builder.Append("SET DEFAULT");
                    break;
            }
        }

        private string ColumnList(string[] columns) => string.Join(", ", columns.Select(_sql.DelimitIdentifier));
    }
}
