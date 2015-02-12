// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Commands.Utilities;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Relational.Migrations.Operations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Commands.Migrations
{
    public class CSharpMigrationOperationGenerator
    {
        private readonly CSharpHelper _code;

        public CSharpMigrationOperationGenerator([NotNull] CSharpHelper code)
        {
            Check.NotNull(code, nameof(code));

            _code = code;
        }

        public virtual void Generate(
            [NotNull] string variable,
            [NotNull] IReadOnlyList<MigrationOperation> operations,
            [NotNull] IndentedStringBuilder builder)
        {
            Check.NotEmpty(variable, nameof(variable));
            Check.NotNull(operations, nameof(operations));
            Check.NotNull(builder, nameof(builder));

            foreach (var operation in operations)
            {
                builder.Append(variable);
                Generate((dynamic)operation, builder);
                builder.AppendLine(";");
            }
        }

        public virtual void Generate([NotNull] AddColumnOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            var column = operation.Column;
            var columnName = column.Name;

            builder
                .Append(".AddColumn(")
                .Append(_code.Literal(operation.Table))
                .Append(", ")
                .Append(_code.Literal(columnName))
                .Append(", ")
                .Append("x => ");
            Generate(column, "x", columnName, builder);

            if (operation.Schema != null)
            {
                builder
                    .Append(", schema: ")
                    .Append(_code.Literal(operation.Schema));
            }

            if (operation.Annotations.Any())
            {
                builder
                    .Append(", annotations: ")
                    .Append(_code.Literal(operation.Annotations));
            }

            builder.Append(")");
        }

        public virtual void Generate([NotNull] AddForeignKeyOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(".AddForeignKey(")
                .Append(_code.Literal(operation.DependentTable))
                .Append(", ")
                .Append(_code.Literal(operation.DependentColumns))
                .Append(", ")
                .Append(_code.Literal(operation.PrincipalTable));

            if (operation.DependentSchema != null)
            {
                builder
                    .Append(", dependentSchema: ")
                    .Append(_code.Literal(operation.DependentSchema));
            }

            if (operation.PrincipalSchema != null)
            {
                builder
                    .Append(", principalSchema: ")
                    .Append(_code.Literal(operation.PrincipalSchema));
            }

            if (operation.PrincipalColumns.Any())
            {
                builder
                    .Append(", principalColumns: ")
                    .Append(_code.Literal(operation.PrincipalColumns));
            }

            if (operation.CascadeDelete)
            {
                builder.Append(", cascadeDelete: true");
            }

            if (operation.Name != null)
            {
                builder
                    .Append(", name: ")
                    .Append(_code.Literal(operation.Name));
            }

            if (operation.Annotations.Any())
            {
                builder
                    .Append(", annotations: ")
                    .Append(_code.Literal(operation.Annotations));
            }

            builder.Append(")");
        }

        public virtual void Generate(
            [NotNull] AddPrimaryKeyOperation operation,
            [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(".AddPrimaryKey(")
                .Append(_code.Literal(operation.Table))
                .Append(", ")
                .Append(_code.Literal(operation.Columns));

            if (operation.Schema != null)
            {
                builder
                    .Append(", schema: ")
                    .Append(_code.Literal(operation.Schema));
            }

            if (operation.Name != null)
            {
                builder
                    .Append(", name: ")
                    .Append(_code.Literal(operation.Name));
            }

            if (operation.Annotations.Any())
            {
                builder
                    .Append(", annotations: ")
                    .Append(_code.Literal(operation.Annotations));
            }

            builder.Append(")");
        }

        public virtual void Generate(
            [NotNull] AddUniqueConstraintOperation operation,
            [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(".AddUniqueConstraint(")
                .Append(_code.Literal(operation.Table))
                .Append(", ")
                .Append(_code.Literal(operation.Columns));

            if (operation.Schema != null)
            {
                builder
                    .Append(", schema: ")
                    .Append(_code.Literal(operation.Schema));
            }

            if (operation.Name != null)
            {
                builder
                    .Append(", name: ")
                    .Append(_code.Literal(operation.Name));
            }

            if (operation.Annotations.Any())
            {
                builder
                    .Append(", annotations: ")
                    .Append(_code.Literal(operation.Annotations));
            }

            builder.Append(")");
        }

        public virtual void Generate([NotNull] AlterColumnOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(".AlterColumn(")
                .Append(_code.Literal(operation.Table));

            if (operation.Schema != null)
            {
                builder
                    .Append(", ")
                    .Append(_code.Literal(operation.Schema));
            }

            var column = operation.Column;
            var name = column.Name;

            builder
                .Append(", ")
                .Append(_code.Literal(name))
                .Append(", x => ");
            Generate(column, "x", name, builder);

            if (operation.Annotations.Any())
            {
                builder
                    .Append(", annotations: ")
                    .Append(_code.Literal(operation.Annotations));
            }

            builder.Append(")");
        }

        public virtual void Generate(
            [NotNull] AlterSequenceOperation operation,
            [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(".AlterSequence(")
                .Append(_code.Literal(operation.Name));

            if (operation.Schema != null)
            {
                builder
                    .Append(", ")
                    .Append(_code.Literal(operation.Schema));
            }

            if (operation.IncrementBy != Sequence.DefaultIncrement)
            {
                builder
                    .Append(", incrementBy: ")
                    .Append(_code.Literal(operation.IncrementBy));
            }

            if (operation.MinValue.HasValue)
            {
                builder
                    .Append(", minValue: ")
                    .Append(_code.Literal(operation.MinValue.Value));
            }

            if (operation.MaxValue.HasValue)
            {
                builder
                    .Append(", maxValue: ")
                    .Append(_code.Literal(operation.MaxValue.Value));
            }

            if (operation.Annotations.Any())
            {
                builder
                    .Append(", annotations: ")
                    .Append(_code.Literal(operation.Annotations));
            }

            builder.Append(")");
        }

        public virtual void Generate([NotNull] AlterTableOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("AlterTable(")
                .Append(_code.Literal(operation.Name));

            if (operation.Schema != null)
            {
                builder
                    .Append(", schema: ")
                    .Append(_code.Literal(operation.Schema));
            }

            if (operation.Annotations.Any())
            {
                builder
                    .Append(", annotations: ")
                    .Append(_code.Literal(operation.Annotations));
            }

            builder.Append(")");
        }

        public virtual void Generate([NotNull] CreateIndexOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(".CreateIndex(")
                .Append(_code.Literal(operation.Name))
                .Append(", ")
                .Append(_code.Literal(operation.Table))
                .Append(", ")
                .Append(_code.Literal(operation.Columns));

            if (operation.Schema != null)
            {
                builder
                    .Append(", schema: ")
                    .Append(_code.Literal(operation.Schema));
            }

            if (operation.Unique)
            {
                builder.Append(", unique: true");
            }

            if (operation.Annotations.Any())
            {
                builder
                    .Append(", annotations: ")
                    .Append(_code.Literal(operation.Annotations));
            }

            builder.Append(")");
        }

        public virtual void Generate(
            [NotNull] CreateSequenceOperation operation,
            [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(".CreateSequence(")
                .Append(_code.Literal(operation.Name));

            if (operation.Schema != null)
            {
                builder
                    .Append(", ")
                    .Append(_code.Literal(operation.Schema));
            }

            if (operation.StoreType != null)
            {
                builder
                    .Append(", storeType: ")
                    .Append(_code.Literal(operation.StoreType));
            }

            if (operation.StartValue != Sequence.DefaultStartValue)
            {
                builder
                    .Append(", startValue: ")
                    .Append(_code.Literal(operation.StartValue));
            }

            if (operation.IncrementBy != Sequence.DefaultIncrement)
            {
                builder
                    .Append(", incrementBy: ")
                    .Append(_code.Literal(operation.IncrementBy));
            }

            if (operation.MinValue.HasValue)
            {
                builder
                    .Append(", minValue: ")
                    .Append(_code.Literal(operation.MinValue.Value));
            }

            if (operation.MaxValue.HasValue)
            {
                builder
                    .Append(", maxValue: ")
                    .Append(_code.Literal(operation.MaxValue.Value));
            }

            if (operation.Annotations.Any())
            {
                builder
                    .Append(", annotations: ")
                    .Append(_code.Literal(operation.Annotations));
            }

            builder.Append(")");
        }

        public virtual void Generate([NotNull] CreateTableOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .AppendLine(".CreateTable(");

            using (builder.Indent())
            {
                builder.Append(_code.Literal(operation.Name));

                if (operation.Schema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append(_code.Literal(operation.Schema));
                }

                builder
                    .AppendLine(",")
                    .AppendLine("x => new")
                    .AppendLine("{");

                var propertyMap = new Dictionary<string, string>();

                using (builder.Indent())
                {
                    var scope = new List<string>();
                    for (var i = 0; i < operation.Columns.Count; i++)
                    {
                        var column = operation.Columns[i];
                        var identifier = _code.Identifier(column.Name, scope);
                        propertyMap.Add(column.Name, identifier);

                        builder
                            .Append(identifier)
                            .Append(" = ");
                        Generate(column, "x", identifier, builder);

                        if (i != operation.Columns.Count - 1)
                        {
                            builder.AppendLine(",");
                        }
                    }
                }

                builder.Append("}");

                if (operation.Annotations.Any())
                {
                    builder
                        .AppendLine(",")
                        .Append("annotations: ")
                        .Append(_code.Literal(operation.Annotations));
                }

                builder.Append(")");

                var primaryKey = operation.PrimaryKey;
                if (primaryKey != null)
                {
                    // TODO: Move to method
                    builder
                        .AppendLine()
                        .Append(".PrimaryKey(")
                        .Append(_code.Lambda(primaryKey.Columns.Select(c => propertyMap[c]).ToList()));

                    if (primaryKey.Name != null)
                    {
                        builder
                            .Append(", name: ")
                            .Append(_code.Literal(primaryKey.Name));
                    }

                    if (operation.Annotations.Any())
                    {
                        builder
                            .Append(", annotations: ")
                            .Append(_code.Literal(operation.Annotations));
                    }

                    builder.Append(")");
                }

                foreach (var uniqueConstraint in operation.UniqueConstraints)
                {
                    // TODO: Move to method
                    builder
                        .AppendLine()
                        .Append(".UniqueConstraint(")
                        .Append(_code.Lambda(uniqueConstraint.Columns.Select(c => propertyMap[c]).ToList()));

                    if (uniqueConstraint.Name != null)
                    {
                        builder
                            .Append(", name: ")
                            .Append(_code.Literal(uniqueConstraint.Name));
                    }

                    if (operation.Annotations.Any())
                    {
                        builder
                            .Append(", annotations: ")
                            .Append(_code.Literal(operation.Annotations));
                    }

                    builder.Append(")");
                }

                foreach (var foreignKey in operation.ForeignKeys)
                {
                    // TODO: Move to method
                    builder
                        .AppendLine()
                        .Append(".ForeignKey(")
                        .Append(_code.Lambda(foreignKey.DependentColumns.Select(c => propertyMap[c]).ToList()))
                        .Append(", ")
                        .Append(_code.Literal(foreignKey.PrincipalTable));

                    if (foreignKey.PrincipalSchema != null)
                    {
                        builder
                            .Append(", ")
                            .Append(_code.Literal(foreignKey.PrincipalSchema));
                    }

                    if (foreignKey.PrincipalColumns.Any())
                    {
                        builder
                            .Append(", ")
                            .Append(_code.Literal(foreignKey.PrincipalColumns));
                    }

                    if (foreignKey.CascadeDelete)
                    {
                        builder.Append(", cascadeDelete: true");
                    }

                    if (foreignKey.Name != null)
                    {
                        builder
                            .Append(", name: ")
                            .Append(_code.Literal(foreignKey.Name));
                    }

                    if (operation.Annotations.Any())
                    {
                        builder
                            .Append(", annotations: ")
                            .Append(_code.Literal(operation.Annotations));
                    }

                    builder.Append(")");
                }

                foreach (var index in operation.Indexes)
                {
                    // TODO: Move to method
                    builder
                        .AppendLine()
                        .Append(".Index(")
                        .Append(_code.Literal(index.Name))
                        .Append(", ")
                        .Append(_code.Lambda(index.Columns.Select(c => propertyMap[c]).ToList()));

                    if (index.Unique)
                    {
                        builder.Append(", unique: true");
                    }

                    if (operation.Annotations.Any())
                    {
                        builder
                            .Append(", annotations: ")
                            .Append(_code.Literal(operation.Annotations));
                    }

                    builder.Append(")");
                }
            }
        }

        public virtual void Generate([NotNull] DropColumnOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(".DropColumn(")
                .Append(_code.Literal(operation.Table))
                .Append(", ")
                .Append(_code.Literal(operation.Name));

            if (operation.Schema != null)
            {
                builder
                    .Append(", schema: ")
                    .Append(_code.Literal(operation.Schema));
            }

            if (operation.Annotations.Any())
            {
                builder
                    .Append(", annotations: ")
                    .Append(_code.Literal(operation.Annotations));
            }

            builder.Append(")");
        }

        public virtual void Generate(
            [NotNull] DropForeignKeyOperation operation,
            [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(".DropForeignKey(")
                .Append(_code.Literal(operation.Table))
                .Append(", ")
                .Append(_code.Literal(operation.Name));

            if (operation.Schema != null)
            {
                builder
                    .Append(", schema: ")
                    .Append(_code.Literal(operation.Schema));
            }

            if (operation.Annotations.Any())
            {
                builder
                    .Append(", annotations: ")
                    .Append(_code.Literal(operation.Annotations));
            }

            builder.Append(")");
        }

        public virtual void Generate([NotNull] DropIndexOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(".DropIndex(")
                .Append(_code.Literal(operation.Table))
                .Append(", ")
                .Append(_code.Literal(operation.Name));

            if (operation.Schema != null)
            {
                builder
                    .Append(", schema: ")
                    .Append(_code.Literal(operation.Schema));
            }

            if (operation.Annotations.Any())
            {
                builder
                    .Append(", annotations: ")
                    .Append(_code.Literal(operation.Annotations));
            }

            builder.Append(")");
        }

        public virtual void Generate(
            [NotNull] DropPrimaryKeyOperation operation,
            [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(".DropPrimaryKey(")
                .Append(_code.Literal(operation.Table))
                .Append(", ")
                .Append(_code.Literal(operation.Name));

            if (operation.Schema != null)
            {
                builder
                    .Append(", schema: ")
                    .Append(_code.Literal(operation.Schema));
            }

            if (operation.Annotations.Any())
            {
                builder
                    .Append(", annotations: ")
                    .Append(_code.Literal(operation.Annotations));
            }

            builder.Append(")");
        }

        public virtual void Generate([NotNull] DropSequenceOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(".DropSequence(")
                .Append(_code.Literal(operation.Name));

            if (operation.Schema != null)
            {
                builder
                    .Append(", ")
                    .Append(_code.Literal(operation.Schema));
            }

            if (operation.Annotations.Any())
            {
                builder
                    .Append(", annotations: ")
                    .Append(_code.Literal(operation.Annotations));
            }

            builder.Append(")");
        }

        public virtual void Generate([NotNull] DropTableOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(".DropTable(")
                .Append(_code.Literal(operation.Name));

            if (operation.Schema != null)
            {
                builder
                    .Append(", ")
                    .Append(_code.Literal(operation.Schema));
            }

            if (operation.Annotations.Any())
            {
                builder
                    .Append(", annotations: ")
                    .Append(_code.Literal(operation.Annotations));
            }

            builder.Append(")");
        }

        public virtual void Generate(
            [NotNull] DropUniqueConstraintOperation operation,
            [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(".DropUniqueConstraint(")
                .Append(_code.Literal(operation.Table))
                .Append(", ")
                .Append(_code.Literal(operation.Name));

            if (operation.Schema != null)
            {
                builder
                    .Append(", schema: ")
                    .Append(_code.Literal(operation.Schema));
            }

            if (operation.Annotations.Any())
            {
                builder
                    .Append(", annotations: ")
                    .Append(_code.Literal(operation.Annotations));
            }

            builder.Append(")");
        }

        public virtual void Generate([NotNull] MoveSequenceOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(".MoveSequence(")
                .Append(_code.Literal(operation.Name))
                .Append(", ")
                .Append(_code.Literal(operation.NewSchema));

            if (operation.Schema != null)
            {
                builder
                    .Append(", schema: ")
                    .Append(_code.Literal(operation.Schema));
            }

            if (operation.Annotations.Any())
            {
                builder
                    .Append(", annotations: ")
                    .Append(_code.Literal(operation.Annotations));
            }

            builder.Append(")");
        }

        public virtual void Generate([NotNull] MoveTableOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(".MoveTable(")
                .Append(_code.Literal(operation.Name))
                .Append(", ")
                .Append(_code.Literal(operation.NewSchema));

            if (operation.Schema != null)
            {
                builder
                    .Append(", schema: ")
                    .Append(_code.Literal(operation.Schema));
            }

            if (operation.Annotations.Any())
            {
                builder
                    .Append(", annotations: ")
                    .Append(_code.Literal(operation.Annotations));
            }

            builder.Append(")");
        }

        public virtual void Generate([NotNull] RenameColumnOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(".RenameColumn(")
                .Append(_code.Literal(operation.Table))
                .Append(", ")
                .Append(_code.Literal(operation.Name))
                .Append(", ")
                .Append(_code.Literal(operation.NewName));

            if (operation.Schema != null)
            {
                builder
                    .Append(", schema: ")
                    .Append(_code.Literal(operation.Schema));
            }

            if (operation.Annotations.Any())
            {
                builder
                    .Append(", annotations: ")
                    .Append(_code.Literal(operation.Annotations));
            }

            builder.Append(")");
        }

        public virtual void Generate([NotNull] RenameIndexOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(".RenameIndex(")
                .Append(_code.Literal(operation.Table))
                .Append(", ")
                .Append(_code.Literal(operation.Name))
                .Append(", ")
                .Append(_code.Literal(operation.NewName));

            if (operation.Schema != null)
            {
                builder
                    .Append(", schema: ")
                    .Append(_code.Literal(operation.Schema));
            }

            if (operation.Annotations.Any())
            {
                builder
                    .Append(", annotations: ")
                    .Append(_code.Literal(operation.Annotations));
            }

            builder.Append(")");
        }

        public virtual void Generate(
            [NotNull] RenameSequenceOperation operation,
            [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(".RenameSequence(")
                .Append(_code.Literal(operation.Name))
                .Append(", ")
                .Append(_code.Literal(operation.NewName));

            if (operation.Schema != null)
            {
                builder
                    .Append(", schema: ")
                    .Append(_code.Literal(operation.Schema));
            }

            if (operation.Annotations.Any())
            {
                builder
                    .Append(", annotations: ")
                    .Append(_code.Literal(operation.Annotations));
            }

            builder.Append(")");
        }

        public virtual void Generate([NotNull] RenameTableOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(".RenameTable(")
                .Append(_code.Literal(operation.Name))
                .Append(", ")
                .Append(_code.Literal(operation.NewName));

            if (operation.Schema != null)
            {
                builder
                    .Append(", schema: ")
                    .Append(_code.Literal(operation.Schema));
            }

            if (operation.Annotations.Any())
            {
                builder
                    .Append(", annotations: ")
                    .Append(_code.Literal(operation.Annotations));
            }

            builder.Append(")");
        }

        public virtual void Generate([NotNull] SqlOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(".Sql(")
                .Append(_code.Literal(operation.Sql));

            if (operation.SuppressTransaction)
            {
                builder.Append(", suppressTransaction: true");
            }

            if (operation.Annotations.Any())
            {
                builder
                    .Append(", annotations: ")
                    .Append(_code.Literal(operation.Annotations));
            }

            builder.Append(")");
        }

        public virtual void Generate(
            [NotNull] ColumnModel column,
            [NotNull] string variable,
            [NotNull] string defaultName,
            [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(column, nameof(column));
            Check.NotEmpty(variable, nameof(variable));
            Check.NotEmpty(defaultName, nameof(defaultName));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(variable)
                .Append(".Column(")
                .Append(_code.Literal(column.StoreType));

            if (column.Name != defaultName)
            {
                builder
                    .Append(", ")
                    .Append("name: ")
                    .Append(_code.Literal(column.Name));
            }

            if (column.Nullable)
            {
                builder
                    .Append(", ")
                    .Append("nullable: true");
            }

            if (column.DefaultValueSql != null)
            {
                builder
                    .Append(", ")
                    .Append("defaultValueSql: ")
                    .Append(_code.Literal(column.DefaultValueSql));
            }
            else if (column.DefaultValue != null)
            {
                builder
                    .Append(", ")
                    .Append("defaultValue: ")
                    .Append(_code.Literal((dynamic)column.DefaultValue));
            }

            if (column.Annotations.Any())
            {
                builder
                    .Append(", annotations: ")
                    .Append(_code.Literal(column.Annotations));
            }

            builder.Append(")");
        }

        protected virtual void Generate(
            [NotNull] MigrationOperation operation,
            [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            throw new InvalidOperationException(Strings.UnknownOperation(operation.GetType()));
        }
    }
}
