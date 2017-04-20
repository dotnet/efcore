// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    public class MigrationsSqlGenerator : IMigrationsSqlGenerator
    {
        private static readonly IReadOnlyDictionary<Type, Action<MigrationsSqlGenerator, MigrationOperation, IModel, MigrationCommandListBuilder>> _generateActions =
            new Dictionary<Type, Action<MigrationsSqlGenerator, MigrationOperation, IModel, MigrationCommandListBuilder>>
            {
                { typeof(AddColumnOperation), (g, o, m, b) => g.Generate((AddColumnOperation)o, m, b) },
                { typeof(AddForeignKeyOperation), (g, o, m, b) => g.Generate((AddForeignKeyOperation)o, m, b) },
                { typeof(AddPrimaryKeyOperation), (g, o, m, b) => g.Generate((AddPrimaryKeyOperation)o, m, b) },
                { typeof(AddUniqueConstraintOperation), (g, o, m, b) => g.Generate((AddUniqueConstraintOperation)o, m, b) },
                { typeof(AlterColumnOperation), (g, o, m, b) => g.Generate((AlterColumnOperation)o, m, b) },
                { typeof(AlterDatabaseOperation), (g, o, m, b) => g.Generate((AlterDatabaseOperation)o, m, b) },
                { typeof(AlterSequenceOperation), (g, o, m, b) => g.Generate((AlterSequenceOperation)o, m, b) },
                { typeof(AlterTableOperation), (g, o, m, b) => g.Generate((AlterTableOperation)o, m, b) },
                { typeof(CreateIndexOperation), (g, o, m, b) => g.Generate((CreateIndexOperation)o, m, b) },
                { typeof(CreateSequenceOperation), (g, o, m, b) => g.Generate((CreateSequenceOperation)o, m, b) },
                { typeof(CreateTableOperation), (g, o, m, b) => g.Generate((CreateTableOperation)o, m, b) },
                { typeof(DropColumnOperation), (g, o, m, b) => g.Generate((DropColumnOperation)o, m, b) },
                { typeof(DropForeignKeyOperation), (g, o, m, b) => g.Generate((DropForeignKeyOperation)o, m, b) },
                { typeof(DropIndexOperation), (g, o, m, b) => g.Generate((DropIndexOperation)o, m, b) },
                { typeof(DropPrimaryKeyOperation), (g, o, m, b) => g.Generate((DropPrimaryKeyOperation)o, m, b) },
                { typeof(DropSchemaOperation), (g, o, m, b) => g.Generate((DropSchemaOperation)o, m, b) },
                { typeof(DropSequenceOperation), (g, o, m, b) => g.Generate((DropSequenceOperation)o, m, b) },
                { typeof(DropTableOperation), (g, o, m, b) => g.Generate((DropTableOperation)o, m, b) },
                { typeof(DropUniqueConstraintOperation), (g, o, m, b) => g.Generate((DropUniqueConstraintOperation)o, m, b) },
                { typeof(EnsureSchemaOperation), (g, o, m, b) => g.Generate((EnsureSchemaOperation)o, m, b) },
                { typeof(RenameColumnOperation), (g, o, m, b) => g.Generate((RenameColumnOperation)o, m, b) },
                { typeof(RenameIndexOperation), (g, o, m, b) => g.Generate((RenameIndexOperation)o, m, b) },
                { typeof(RenameSequenceOperation), (g, o, m, b) => g.Generate((RenameSequenceOperation)o, m, b) },
                { typeof(RenameTableOperation), (g, o, m, b) => g.Generate((RenameTableOperation)o, m, b) },
                { typeof(RestartSequenceOperation), (g, o, m, b) => g.Generate((RestartSequenceOperation)o, m, b) },
                { typeof(SqlOperation), (g, o, m, b) => g.Generate((SqlOperation)o, m, b) },
                { typeof(InsertOperation), (g, o, m, b) => g.Generate((InsertOperation)o, m, b) },
                { typeof(DeleteOperation), (g, o, m, b) => g.Generate((DeleteOperation)o, m, b) },
                { typeof(UpdateOperation), (g, o, m, b) => g.Generate((UpdateOperation)o, m, b) }
            };

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public MigrationsSqlGenerator([NotNull] MigrationsSqlGeneratorDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing dependencies for this service.
        /// </summary>
        protected virtual MigrationsSqlGeneratorDependencies Dependencies { get; }

        public virtual IReadOnlyList<MigrationCommand> Generate(
            IReadOnlyList<MigrationOperation> operations,
            IModel model = null)
        {
            Check.NotNull(operations, nameof(operations));

            var builder = new MigrationCommandListBuilder(Dependencies.CommandBuilderFactory);
            foreach (var operation in operations)
            {
                Generate(operation, model, builder);
            }

            return builder.GetCommandList();
        }

        protected virtual void Generate(
            [NotNull] MigrationOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            var operationType = operation.GetType();
            Action<MigrationsSqlGenerator, MigrationOperation, IModel, MigrationCommandListBuilder> generateAction;
            if (!_generateActions.TryGetValue(operationType, out generateAction))
            {
                throw new InvalidOperationException(RelationalStrings.UnknownOperation(GetType().ShortDisplayName(), operationType));
            }

            generateAction(this, operation, model, builder);
        }

        protected virtual void Generate(
            [NotNull] AddColumnOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
            => Generate(operation, model, builder, terminate: true);

        protected virtual void Generate(
            [NotNull] AddColumnOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder,
            bool terminate)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER TABLE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" ADD ");

            ColumnDefinition(operation, model, builder);

            if (terminate)
            {
                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                EndStatement(builder);
            }
        }

        protected virtual void Generate(
            [NotNull] AddForeignKeyOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
            => Generate(operation, model, builder, terminate: true);

        protected virtual void Generate(
            [NotNull] AddForeignKeyOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder,
            bool terminate)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER TABLE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" ADD ");

            ForeignKeyConstraint(operation, model, builder);

            if (terminate)
            {
                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                EndStatement(builder);
            }
        }

        protected virtual void Generate(
            [NotNull] AddPrimaryKeyOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
            => Generate(operation, model, builder, terminate: true);

        protected virtual void Generate(
            [NotNull] AddPrimaryKeyOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder,
            bool terminate)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER TABLE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" ADD ");
            PrimaryKeyConstraint(operation, model, builder);

            if (terminate)
            {
                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                EndStatement(builder);
            }
        }

        protected virtual void Generate(
            [NotNull] AddUniqueConstraintOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER TABLE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" ADD ");
            UniqueConstraint(operation, model, builder);
            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            EndStatement(builder);
        }

        protected virtual void Generate(
            [NotNull] AlterColumnOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            throw new NotImplementedException();
        }

        protected virtual void Generate(
            [NotNull] AlterDatabaseOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
        }

        protected virtual void Generate(
            [NotNull] RenameIndexOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            throw new NotImplementedException();
        }

        protected virtual void Generate(
            [NotNull] AlterSequenceOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER SEQUENCE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema));

            SequenceOptions(operation, model, builder);

            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            EndStatement(builder);
        }

        protected virtual void Generate(
            [NotNull] AlterTableOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
        }

        protected virtual void Generate(
            [NotNull] RenameTableOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            throw new NotImplementedException();
        }

        protected virtual void Generate(
            [NotNull] CreateIndexOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
            => Generate(operation, model, builder, terminate: true);

        protected virtual void Generate(
            [NotNull] CreateIndexOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder,
            bool terminate)
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
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .Append(" ON ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" (")
                .Append(ColumnList(operation.Columns))
                .Append(")");

            if (operation.Filter != null)
            {
                builder
                    .Append(" WHERE ")
                    .Append(operation.Filter);
            }

            if (terminate)
            {
                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                EndStatement(builder);
            }
        }

        protected virtual void Generate(
            [NotNull] EnsureSchemaOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            throw new NotImplementedException();
        }

        protected virtual void Generate(
            [NotNull] CreateSequenceOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("CREATE SEQUENCE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema));

            if (operation.ClrType != typeof(long))
            {
                builder
                    .Append(" AS ")
                    .Append(Dependencies.TypeMapper.GetMapping(operation.ClrType).StoreType);
            }

            builder
                .Append(" START WITH ")
                .Append(Dependencies.SqlGenerationHelper.GenerateLiteral(operation.StartValue));

            SequenceOptions(operation, model, builder);

            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            EndStatement(builder);
        }

        protected virtual void Generate(
            [NotNull] CreateTableOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
            => Generate(operation, model, builder, terminate: true);

        protected virtual void Generate(
            [NotNull] CreateTableOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder,
            bool terminate)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("CREATE TABLE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
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

            if (terminate)
            {
                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                EndStatement(builder);
            }
        }

        protected virtual void Generate(
            [NotNull] DropColumnOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
            => Generate(operation, model, builder, terminate: true);

        protected virtual void Generate(
            [NotNull] DropColumnOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder,
            bool terminate)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER TABLE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" DROP COLUMN ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));

            if (terminate)
            {
                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                EndStatement(builder);
            }
        }

        protected virtual void Generate(
            [NotNull] DropForeignKeyOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
            => Generate(operation, model, builder, terminate: true);

        protected virtual void Generate(
            [NotNull] DropForeignKeyOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder,
            bool terminate)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER TABLE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" DROP CONSTRAINT ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));

            if (terminate)
            {
                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                EndStatement(builder);
            }
        }

        protected virtual void Generate(
            [NotNull] DropIndexOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            throw new NotImplementedException();
        }

        protected virtual void Generate(
            [NotNull] DropPrimaryKeyOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
            => Generate(operation, model, builder, terminate: true);

        protected virtual void Generate(
            [NotNull] DropPrimaryKeyOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder,
            bool terminate)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER TABLE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" DROP CONSTRAINT ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));

            if (terminate)
            {
                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                EndStatement(builder);
            }
        }

        protected virtual void Generate(
            [NotNull] DropSchemaOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("DROP SCHEMA ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            EndStatement(builder);
        }

        protected virtual void Generate(
            [NotNull] DropSequenceOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("DROP SEQUENCE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            EndStatement(builder);
        }

        protected virtual void Generate(
            [NotNull] DropTableOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
            => Generate(operation, model, builder, terminate: true);

        protected virtual void Generate(
            [NotNull] DropTableOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder,
            bool terminate)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("DROP TABLE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema));

            if (terminate)
            {
                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                EndStatement(builder);
            }
        }

        protected virtual void Generate(
            [NotNull] DropUniqueConstraintOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER TABLE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" DROP CONSTRAINT ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            EndStatement(builder);
        }

        protected virtual void Generate(
            [NotNull] RenameColumnOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            throw new NotImplementedException();
        }

        protected virtual void Generate(
            [NotNull] RenameSequenceOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            throw new NotImplementedException();
        }

        protected virtual void Generate(
            [NotNull] RestartSequenceOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER SEQUENCE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
                .Append(" RESTART WITH ")
                .Append(Dependencies.SqlGenerationHelper.GenerateLiteral(operation.StartValue))
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            EndStatement(builder);
        }

        protected virtual void Generate(
            [NotNull] SqlOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(operation.Sql);

            EndStatement(builder, operation.SuppressTransaction);
        }

        protected virtual void Generate(
            [NotNull] InsertOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (operation.Values.Length == 0)
            {
                return;
            }

            builder
                .Append("INSERT INTO ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" (")
                .Append(ColumnList(operation.Columns))
                .AppendLine(")")
                .Append("VALUES ");

            var rowCount = operation.Values.GetLength(0);
            var valueCount = operation.Values.GetLength(1);
            for (var i = 0; i < rowCount; i++)
            {
                if (i != 0)
                {
                    builder
                        .AppendLine(",")
                        .Append("       ");
                }

                builder.Append("(");
                for (var j = 0; j < valueCount; j++)
                {
                    if (j != 0)
                    {
                        builder.Append(", ");
                    }

                    builder.Append(Dependencies.SqlGenerationHelper.GenerateLiteral(operation.Values[i, j]));
                }

                builder.Append(")");
            }

            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            EndStatement(builder);
        }

        protected virtual void Generate(
            [NotNull] DeleteOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (operation.KeyValues.Length == 0)
            {
                return;
            }

            builder
                .Append("DELETE FROM ")
                .AppendLine(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema));

            builder.Append("WHERE ");

            var rowCount = operation.KeyValues.GetLength(0);
            var valueCount = operation.KeyValues.GetLength(1);
            for (var i = 0; i < rowCount; i++)
            {
                if (i != 0)
                {
                    builder
                        .AppendLine(" OR")
                        .Append("      ");
                }

                builder.Append("(");
                for (var j = 0; j < valueCount; j++)
                {
                    if (j != 0)
                    {
                        builder.Append(" AND ");
                    }

                    builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.KeyColumns[j]));

                    var value = operation.KeyValues[i, j];
                    if (value == null)
                    {
                        builder.Append(" IS NULL");
                    }
                    else
                    {
                        builder
                            .Append(" = ")
                            .Append(Dependencies.SqlGenerationHelper.GenerateLiteral(value));
                    }
                }

                builder.Append(")");
            }

            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            EndStatement(builder);
        }

        protected virtual void Generate(
            [NotNull] UpdateOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (operation.Values.Length == 0)
            {
                return;
            }

            var rowCount = operation.Values.GetLength(0);
            var valueCount = operation.Values.GetLength(1);
            var keyValueCount = operation.KeyValues.GetLength(1);
            for (var i = 0; i < rowCount; i++)
            {
                builder
                    .Append("UPDATE ")
                    .AppendLine(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                    .Append("SET ");

                for (var j = 0; j < valueCount; j++)
                {
                    if (j != 0)
                    {
                        builder
                            .AppendLine(",")
                            .Append("    ");
                    }

                    builder.Append(
                        Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Columns[j]) +
                        " = " +
                        Dependencies.SqlGenerationHelper.GenerateLiteral(operation.Values[i, j]));
                }

                builder
                    .AppendLine()
                    .Append("WHERE (");
                for (var j = 0; j < keyValueCount; j++)
                {
                    if (j != 0)
                    {
                        builder.Append(" AND ");
                    }

                    builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.KeyColumns[j]));

                    var value = operation.KeyValues[i, j];
                    if (value == null)
                    {
                        builder.Append(" IS NULL");
                    }
                    else
                    {
                        builder
                            .Append(" = ")
                            .Append(Dependencies.SqlGenerationHelper.GenerateLiteral(value));
                    }
                }

                builder
                    .Append(")")
                    .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

                EndStatement(builder);
            }
        }

        protected virtual void SequenceOptions(
            [NotNull] AlterSequenceOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
            => SequenceOptions(
                operation.Schema,
                operation.Name,
                operation.IncrementBy,
                operation.MinValue,
                operation.MaxValue,
                operation.IsCyclic,
                model,
                builder);

        protected virtual void SequenceOptions(
            [NotNull] CreateSequenceOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
            => SequenceOptions(
                operation.Schema,
                operation.Name,
                operation.IncrementBy,
                operation.MinValue,
                operation.MaxValue,
                operation.IsCyclic,
                model,
                builder);

        protected virtual void SequenceOptions(
            [CanBeNull] string schema,
            [NotNull] string name,
            int increment,
            long? minimumValue,
            long? maximumValue,
            bool cycle,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(increment, nameof(increment));
            Check.NotNull(cycle, nameof(cycle));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(" INCREMENT BY ")
                .Append(Dependencies.SqlGenerationHelper.GenerateLiteral(increment));

            if (minimumValue != null)
            {
                builder
                    .Append(" MINVALUE ")
                    .Append(Dependencies.SqlGenerationHelper.GenerateLiteral(minimumValue));
            }
            else
            {
                builder.Append(" NO MINVALUE");
            }

            if (maximumValue != null)
            {
                builder
                    .Append(" MAXVALUE ")
                    .Append(Dependencies.SqlGenerationHelper.GenerateLiteral(maximumValue));
            }
            else
            {
                builder.Append(" NO MAXVALUE");
            }

            builder.Append(cycle ? " CYCLE" : " NO CYCLE");
        }

        protected virtual void ColumnDefinition(
            [NotNull] AddColumnOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
            => ColumnDefinition(
                operation.Schema,
                operation.Table,
                operation.Name,
                operation.ClrType,
                operation.ColumnType,
                operation.IsUnicode,
                operation.MaxLength,
                operation.IsRowVersion,
                operation.IsNullable,
                operation.DefaultValue,
                operation.DefaultValueSql,
                operation.ComputedColumnSql,
                operation,
                model,
                builder);

        protected virtual void ColumnDefinition(
            [CanBeNull] string schema,
            [NotNull] string table,
            [NotNull] string name,
            [NotNull] Type clrType,
            [CanBeNull] string type,
            [CanBeNull] bool? unicode,
            [CanBeNull] int? maxLength,
            bool rowVersion,
            bool nullable,
            [CanBeNull] object defaultValue,
            [CanBeNull] string defaultValueSql,
            [CanBeNull] string computedColumnSql,
            [NotNull] IAnnotatable annotatable,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(clrType, nameof(clrType));
            Check.NotNull(annotatable, nameof(annotatable));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(name))
                .Append(" ")
                .Append(type ?? GetColumnType(schema, table, name, clrType, unicode, maxLength, rowVersion, model));

            builder.Append(nullable ? " NULL" : " NOT NULL");

            DefaultValue(defaultValue, defaultValueSql, builder);
        }

        protected virtual string GetColumnType(
            [CanBeNull] string schema,
            [NotNull] string table,
            [NotNull] string name,
            [NotNull] Type clrType,
            [CanBeNull] bool? unicode,
            [CanBeNull] int? maxLength,
            bool rowVersion,
            [CanBeNull] IModel model)
        {
            Check.NotEmpty(table, nameof(table));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(clrType, nameof(clrType));

            var keyOrIndex = false;

            var property = FindProperty(model, schema, table, name);
            if (property != null)
            {
                if (unicode == property.IsUnicode()
                    && maxLength == property.GetMaxLength()
                    && rowVersion == (property.IsConcurrencyToken && property.ValueGenerated == ValueGenerated.OnAddOrUpdate))
                {
                    return Dependencies.TypeMapper.GetMapping(property).StoreType;
                }

                keyOrIndex = property.IsKey() || property.IsForeignKey();
            }

            return (clrType == typeof(string)
                       ? Dependencies.TypeMapper.StringMapper?.FindMapping(unicode ?? true, keyOrIndex, maxLength)?.StoreType
                       : clrType == typeof(byte[])
                           ? Dependencies.TypeMapper.ByteArrayMapper?.FindMapping(rowVersion, keyOrIndex, maxLength)?.StoreType
                           : null)
                   ?? Dependencies.TypeMapper.GetMapping(clrType).StoreType;
        }

        protected virtual void DefaultValue(
            [CanBeNull] object defaultValue,
            [CanBeNull] string defaultValueSql,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            if (defaultValueSql != null)
            {
                builder
                    .Append(" DEFAULT (")
                    .Append(defaultValueSql)
                    .Append(")");
            }
            else if (defaultValue != null)
            {
                builder
                    .Append(" DEFAULT ")
                    .Append(Dependencies.SqlGenerationHelper.GenerateLiteral(defaultValue));
            }
        }

        protected virtual void ForeignKeyConstraint(
            [NotNull] AddForeignKeyOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (operation.Name != null)
            {
                builder
                    .Append("CONSTRAINT ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                    .Append(" ");
            }

            builder
                .Append("FOREIGN KEY (")
                .Append(ColumnList(operation.Columns))
                .Append(") REFERENCES ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.PrincipalTable, operation.PrincipalSchema));

            if (operation.PrincipalColumns != null)
            {
                builder
                    .Append(" (")
                    .Append(ColumnList(operation.PrincipalColumns))
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

        protected virtual void PrimaryKeyConstraint(
            [NotNull] AddPrimaryKeyOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (operation.Name != null)
            {
                builder
                    .Append("CONSTRAINT ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                    .Append(" ");
            }

            builder
                .Append("PRIMARY KEY ");

            IndexTraits(operation, model, builder);

            builder.Append("(")
                .Append(ColumnList(operation.Columns))
                .Append(")");
        }

        protected virtual void UniqueConstraint(
            [NotNull] AddUniqueConstraintOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (operation.Name != null)
            {
                builder
                    .Append("CONSTRAINT ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                    .Append(" ");
            }

            builder
                .Append("UNIQUE ");

            IndexTraits(operation, model, builder);

            builder.Append("(")
                .Append(ColumnList(operation.Columns))
                .Append(")");
        }

        protected virtual void IndexTraits(
            [NotNull] MigrationOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
        }

        protected virtual void ForeignKeyAction(
            ReferentialAction referentialAction,
            [NotNull] MigrationCommandListBuilder builder)
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
                default:
                    Debug.Assert(
                        referentialAction == ReferentialAction.NoAction,
                        "Unexpected value: " + referentialAction);
                    break;
            }
        }

        protected virtual IEnumerable<IEntityType> FindEntityTypes(
            [CanBeNull] IModel model,
            [CanBeNull] string schema,
            [NotNull] string tableName)
            => model?.GetEntityTypes().Where(
                t => Dependencies.Annotations.For(t).TableName == tableName && Dependencies.Annotations.For(t).Schema == schema);

        protected virtual IProperty FindProperty(
            [CanBeNull] IModel model,
            [CanBeNull] string schema,
            [NotNull] string tableName,
            [NotNull] string columnName
            // Any property that maps to the column will work because model validator has
            // checked that all properties result in the same column definition.
            )
            => FindEntityTypes(model, schema, tableName)?.SelectMany(e => e.GetDeclaredProperties())
                 .FirstOrDefault(p => Dependencies.Annotations.For(p).ColumnName == columnName);

        protected virtual void EndStatement(
            [NotNull] MigrationCommandListBuilder builder,
            bool suppressTransaction = false)
        {
            Check.NotNull(builder, nameof(builder));

            builder.EndCommand(suppressTransaction);
        }

        protected virtual string ColumnList([NotNull] string[] columns)
            => string.Join(", ", columns.Select(Dependencies.SqlGenerationHelper.DelimitIdentifier));

        protected virtual bool IsOldColumnSupported([CanBeNull] IModel model)
        {
            var versionString = model?[CoreAnnotationNames.ProductVersionAnnotation] as string;
            if (versionString == null)
            {
                return false;
            }

            var prereleaseIndex = versionString.IndexOf("-", StringComparison.Ordinal);
            if (prereleaseIndex != -1)
            {
                versionString = versionString.Substring(0, prereleaseIndex);
            }

            return new Version(versionString) >= new Version(1, 1, 0);
        }
    }
}
