// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Commands.Utilities;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Migrations.Operations;
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

        protected virtual void Generate([NotNull] MigrationOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            throw new InvalidOperationException(Strings.UnknownOperation(operation.GetType()));
        }

        protected virtual void Generate([NotNull] AddColumnOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".AddColumn(");

            using (builder.Indent())
            {
                builder
                    .Append("name: ")
                    .Append(_code.Literal(operation.Name));

                if (operation.Schema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("schema: ")
                        .Append(_code.Literal(operation.Schema));
                }

                builder
                    .AppendLine(",")
                    .Append("table: ")
                    .Append(_code.Literal(operation.Table))
                    .AppendLine(",")
                    .Append("type: ")
                    .Append(_code.Literal(operation.Type))
                    .AppendLine(",")
                    .Append("nullable: ")
                    .Append(_code.Literal(operation.IsNullable));

                if (operation.DefaultValueSql != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("defaultExpression: ")
                        .Append(_code.Literal(operation.DefaultValueSql));
                }
                else if (operation.DefaultValue != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("defaultValue: ")
                        .Append(_code.UnknownLiteral(operation.DefaultValue));
                }

                builder.Append(")");

                Annotations(operation.Annotations, builder);
            }
        }

        protected virtual void Generate([NotNull] AddForeignKeyOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".AddForeignKey(");

            using (builder.Indent())
            {
                builder
                    .Append("name: ")
                    .Append(_code.Literal(operation.Name));

                if (operation.Schema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("schema: ")
                        .Append(_code.Literal(operation.Schema));
                }

                builder
                    .AppendLine(",")
                    .Append("table: ")
                    .Append(_code.Literal(operation.Table))
                    .AppendLine(",")
                    .Append(
                        operation.Columns.Length == 1
                            ? "column: "
                            : "columns: ")
                    .Append(_code.Literal(operation.Columns));

                if (operation.ReferencedSchema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("referencedSchema: ")
                        .Append(_code.Literal(operation.ReferencedSchema));
                }

                builder
                    .AppendLine(",")
                    .Append("referencedTable: ")
                    .Append(_code.Literal(operation.ReferencedTable));

                if (operation.ReferencedColumns != null)
                {
                    builder
                        .AppendLine(",")
                        .Append(
                            operation.ReferencedColumns.Length == 1
                                ? "referencedColumn: "
                                : "referencedColumns: ")
                        .Append(_code.Literal(operation.ReferencedColumns));
                }

                if (operation.OnUpdate != ReferentialAction.NoAction)
                {
                    builder
                        .AppendLine(",")
                        .Append("onUpdate: ")
                        .Append(_code.Literal(operation.OnUpdate));
                }

                if (operation.OnDelete != ReferentialAction.NoAction)
                {
                    builder
                        .AppendLine(",")
                        .Append("onDelete: ")
                        .Append(_code.Literal(operation.OnDelete));
                }

                builder.Append(")");

                Annotations(operation.Annotations, builder);
            }
        }

        protected virtual void Generate([NotNull] AddPrimaryKeyOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".AddPrimaryKey(");

            using (builder.Indent())
            {
                builder
                    .Append("name: ")
                    .Append(_code.Literal(operation.Name));

                if (operation.Schema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("schema: ")
                        .Append(_code.Literal(operation.Schema));
                }

                builder
                    .AppendLine(",")
                    .Append("table: ")
                    .Append(_code.Literal(operation.Table))
                    .AppendLine(",")
                    .Append(
                        operation.Columns.Length == 1
                            ? "column: "
                            : "columns: ")
                    .Append(_code.Literal(operation.Columns))
                    .Append(")");

                Annotations(operation.Annotations, builder);
            }
        }

        protected virtual void Generate([NotNull] AddUniqueConstraintOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".AddUniqueConstraint(");

            using (builder.Indent())
            {
                builder
                    .Append("name: ")
                    .Append(_code.Literal(operation.Name));

                if (operation.Schema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("schema: ")
                        .Append(_code.Literal(operation.Schema));
                }

                builder
                    .AppendLine(",")
                    .Append("table: ")
                    .Append(_code.Literal(operation.Table))
                    .AppendLine(",")
                    .Append(
                        operation.Columns.Length == 1
                            ? "column: "
                            : "columns: ")
                    .Append(_code.Literal(operation.Columns))
                    .Append(")");

                Annotations(operation.Annotations, builder);
            }
        }

        protected virtual void Generate([NotNull] AlterColumnOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".AlterColumn(");

            using (builder.Indent())
            {
                builder
                    .Append("name: ")
                    .Append(_code.Literal(operation.Name));

                if (operation.Schema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("schema: ")
                        .Append(_code.Literal(operation.Schema));
                }

                builder
                    .AppendLine(",")
                    .Append("table: ")
                    .Append(_code.Literal(operation.Table))
                    .AppendLine(",")
                    .Append("type: ")
                    .Append(_code.Literal(operation.Type))
                    .AppendLine(",")
                    .Append("nullable: ")
                    .Append(_code.Literal(operation.IsNullable));

                if (operation.DefaultValueSql != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("defaultExpression: ")
                        .Append(_code.Literal(operation.DefaultValueSql));
                }
                else if (operation.DefaultValue != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("defaultValue: ")
                        .Append(_code.UnknownLiteral(operation.DefaultValue));
                }

                builder.Append(")");

                Annotations(operation.Annotations, builder);
            }
        }

        protected virtual void Generate([NotNull] AlterSequenceOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".AlterSequence(");

            using (builder.Indent())
            {
                builder
                    .Append("name: ")
                    .Append(_code.Literal(operation.Name));

                if (operation.Schema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("schema: ")
                        .Append(_code.Literal(operation.Schema));
                }

                if (operation.IncrementBy != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("incrementBy: ")
                        .Append(_code.Literal(operation.IncrementBy));
                }

                if (operation.MinValue != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("minValue: ")
                        .Append(_code.Literal(operation.MinValue));
                }

                if (operation.MaxValue != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("maxValue: ")
                        .Append(_code.Literal(operation.MaxValue));
                }

                if (operation.Cycle)
                {
                    builder
                        .AppendLine(",")
                        .Append("cycle: true");
                }

                builder.Append(")");

                Annotations(operation.Annotations, builder);
            }
        }

        protected virtual void Generate([NotNull] CreateIndexOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".CreateIndex(");

            using (builder.Indent())
            {
                builder
                    .Append("name: ")
                    .Append(_code.Literal(operation.Name));

                if (operation.Schema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("schema: ")
                        .Append(_code.Literal(operation.Schema));
                }

                builder
                    .AppendLine(",")
                    .Append("table: ")
                    .Append(_code.Literal(operation.Table))
                    .AppendLine(",")
                    .Append(
                        operation.Columns.Length == 1
                            ? "column: "
                            : "columns: ")
                    .Append(_code.Literal(operation.Columns));

                if (operation.IsUnique)
                {
                    builder
                        .AppendLine(",")
                        .Append("unique: true");
                }

                builder.Append(")");

                Annotations(operation.Annotations, builder);
            }
        }

        protected virtual void Generate([NotNull] CreateSchemaOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(".CreateSchema(")
                .Append(_code.Literal(operation.Name))
                .Append(")");

            using (builder.Indent())
            {
                Annotations(operation.Annotations, builder);
            }
        }

        protected virtual void Generate([NotNull] CreateSequenceOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".CreateSequence(");

            using (builder.Indent())
            {
                builder
                    .Append("name: ")
                    .Append(_code.Literal(operation.Name));

                if (operation.Schema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("schema: ")
                        .Append(_code.Literal(operation.Schema));
                }

                if (operation.Type != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("type: ")
                        .Append(_code.Literal(operation.Type));
                }

                if (operation.StartWith.HasValue)
                {
                    builder
                        .AppendLine(",")
                        .Append("startWith: ")
                        .Append(_code.Literal(operation.StartWith.Value));
                }

                if (operation.IncrementBy.HasValue)
                {
                    builder
                        .AppendLine(",")
                        .Append("incrementBy: ")
                        .Append(_code.Literal(operation.IncrementBy.Value));
                }

                if (operation.MinValue.HasValue)
                {
                    builder
                        .AppendLine(",")
                        .Append("minValue: ")
                        .Append(_code.Literal(operation.MinValue.Value));
                }

                if (operation.MaxValue.HasValue)
                {
                    builder
                        .AppendLine(",")
                        .Append("maxValue: ")
                        .Append(_code.Literal(operation.MaxValue.Value));
                }

                if (operation.Cycle)
                {
                    builder
                        .AppendLine(",")
                        .Append("cycle: true");
                }

                builder.Append(")");

                Annotations(operation.Annotations, builder);
            }
        }

        protected virtual void Generate([NotNull] CreateTableOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".CreateTable(");

            using (builder.Indent())
            {
                builder
                    .Append("name: ")
                    .Append(_code.Literal(operation.Name));

                if (operation.Schema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("schema: ")
                        .Append(_code.Literal(operation.Schema));
                }

                builder
                    .AppendLine(",")
                    .AppendLine("columns: table => new")
                    .AppendLine("{");

                var map = new Dictionary<string, string>();
                using (builder.Indent())
                {
                    var scope = new List<string>();
                    for (var i = 0; i < operation.Columns.Count; i++)
                    {
                        var column = operation.Columns[i];
                        var propertyName = _code.Identifier(column.Name, scope);
                        map.Add(column.Name, propertyName);

                        builder
                            .Append(propertyName)
                            .Append(" = table.Column(");

                        if (propertyName != column.Name)
                        {
                            builder
                                .Append("name: ")
                                .Append(_code.Literal(column.Name))
                                .Append(", ");
                        }

                        builder
                            .Append("type: ")
                            .Append(_code.Literal(column.Type))
                            .Append(", nullable: ")
                            .Append(_code.Literal(column.IsNullable));

                        if (column.DefaultValueSql != null)
                        {
                            builder
                                .Append(", defaultExpression: ")
                                .Append(_code.Literal(column.DefaultValueSql));
                        }
                        else if (column.DefaultValue != null)
                        {
                            builder
                                .Append(", defaultValue: ")
                                .Append(_code.UnknownLiteral(column.DefaultValue));
                        }

                        builder.Append(")");

                        using (builder.Indent())
                        {
                            Annotations(column.Annotations, builder);
                        }

                        if (i != operation.Columns.Count - 1)
                        {
                            builder.Append(",");
                        }

                        builder.AppendLine();
                    }
                }

                builder
                    .AppendLine("},")
                    .AppendLine("constraints: table =>")
                    .AppendLine("{");

                using (builder.Indent())
                {
                    if (operation.PrimaryKey != null)
                    {
                        builder
                            .Append("table.PrimaryKey(")
                            .Append(_code.Literal(operation.PrimaryKey.Name))
                            .Append(", ")
                            .Append(_code.Lambda(operation.PrimaryKey.Columns.Select(c => map[c]).ToList()))
                            .Append(")");

                        using (builder.Indent())
                        {
                            Annotations(operation.PrimaryKey.Annotations, builder);
                        }

                        builder.AppendLine(";");
                    }

                    foreach (var uniqueConstraint in operation.UniqueConstraints)
                    {
                        builder
                            .Append("table.Unique(")
                            .Append(_code.Literal(uniqueConstraint.Name))
                            .Append(", ")
                            .Append(_code.Lambda(uniqueConstraint.Columns.Select(c => map[c]).ToList()))
                            .Append(")");

                        using (builder.Indent())
                        {
                            Annotations(uniqueConstraint.Annotations, builder);
                        }

                        builder.AppendLine(";");
                    }

                    foreach (var foreignKey in operation.ForeignKeys)
                    {
                        builder.AppendLine("table.ForeignKey(");

                        using (builder.Indent())
                        {
                            builder
                                .Append("name: ")
                                .Append(_code.Literal(foreignKey.Name))
                                .AppendLine(",")
                                .Append("columns: ")
                                .Append(_code.Lambda(foreignKey.Columns.Select(c => map[c]).ToList()));

                            if (foreignKey.ReferencedSchema != null)
                            {
                                builder
                                    .AppendLine(",")
                                    .Append("referencedSchema: ")
                                    .Append(_code.Literal(foreignKey.ReferencedSchema));
                            }

                            builder
                                .AppendLine(",")
                                .Append("referencedTable: ")
                                .Append(_code.Literal(foreignKey.ReferencedTable));

                            if (foreignKey.ReferencedColumns != null)
                            {
                                builder
                                    .AppendLine(",")
                                    .Append(
                                        foreignKey.ReferencedColumns.Length == 1
                                            ? "referencedColumn: "
                                            : "referencedColumns: ")
                                    .Append(_code.Literal(foreignKey.ReferencedColumns));
                            }

                            if (foreignKey.OnUpdate != ReferentialAction.NoAction)
                            {
                                builder
                                    .AppendLine(",")
                                    .Append("onUpdate: ")
                                    .Append(_code.Literal(foreignKey.OnUpdate));
                            }

                            if (foreignKey.OnDelete != ReferentialAction.NoAction)
                            {
                                builder
                                    .AppendLine(",")
                                    .Append("onDelete: ")
                                    .Append(_code.Literal(foreignKey.OnDelete));
                            }

                            builder.Append(")");

                            Annotations(foreignKey.Annotations, builder);
                        }

                        builder.AppendLine(";");
                    }
                }

                builder.Append("})");

                Annotations(operation.Annotations, builder);
            }
        }

        protected virtual void Generate([NotNull] DropColumnOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(".DropColumn(name: ")
                .Append(_code.Literal(operation.Name));

            if (operation.Schema != null)
            {
                builder
                    .Append(", schema: ")
                    .Append(_code.Literal(operation.Schema));
            }

            builder
                .Append(", table: ")
                .Append(_code.Literal(operation.Table))
                .Append(")");

            using (builder.Indent())
            {
                Annotations(operation.Annotations, builder);
            }
        }

        protected virtual void Generate([NotNull] DropForeignKeyOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(".DropForeignKey(name: ")
                .Append(_code.Literal(operation.Name));

            if (operation.Schema != null)
            {
                builder
                    .Append(", schema: ")
                    .Append(_code.Literal(operation.Schema));
            }

            builder
                .Append(", table: ")
                .Append(_code.Literal(operation.Table))
                .Append(")");

            using (builder.Indent())
            {
                Annotations(operation.Annotations, builder);
            }
        }

        protected virtual void Generate([NotNull] DropIndexOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(".DropIndex(name: ")
                .Append(_code.Literal(operation.Name));

            if (operation.Schema != null)
            {
                builder
                    .Append(", schema: ")
                    .Append(_code.Literal(operation.Schema));
            }

            builder
                .Append(", table: ")
                .Append(_code.Literal(operation.Table))
                .Append(")");

            using (builder.Indent())
            {
                Annotations(operation.Annotations, builder);
            }
        }

        protected virtual void Generate([NotNull] DropPrimaryKeyOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(".DropPrimaryKey(name: ")
                .Append(_code.Literal(operation.Name));

            if (operation.Schema != null)
            {
                builder
                    .Append(", schema: ")
                    .Append(_code.Literal(operation.Schema));
            }

            builder
                .Append(", table: ")
                .Append(_code.Literal(operation.Table))
                .Append(")");

            using (builder.Indent())
            {
                Annotations(operation.Annotations, builder);
            }
        }

        protected virtual void Generate([NotNull] DropSchemaOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(".DropSchema(")
                .Append(_code.Literal(operation.Name))
                .Append(")");

            using (builder.Indent())
            {
                Annotations(operation.Annotations, builder);
            }
        }

        protected virtual void Generate([NotNull] DropSequenceOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.Append(".DropSequence(");

            if (operation.Schema != null)
            {
                builder.Append("name: ");
            }

            builder.Append(_code.Literal(operation.Name));

            if (operation.Schema != null)
            {
                builder
                    .Append(", schema: ")
                    .Append(_code.Literal(operation.Schema));
            }

            builder.Append(")");

            using (builder.Indent())
            {
                Annotations(operation.Annotations, builder);
            }
        }

        protected virtual void Generate([NotNull] DropTableOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.Append(".DropTable(");

            if (operation.Schema != null)
            {
                builder.Append("name: ");
            }

            builder.Append(_code.Literal(operation.Name));

            if (operation.Schema != null)
            {
                builder
                    .Append(", schema: ")
                    .Append(_code.Literal(operation.Schema));
            }

            builder.Append(")");

            using (builder.Indent())
            {
                Annotations(operation.Annotations, builder);
            }
        }

        protected virtual void Generate([NotNull] DropUniqueConstraintOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(".DropUniqueConstraint(name: ")
                .Append(_code.Literal(operation.Name));

            if (operation.Schema != null)
            {
                builder
                    .Append(", schema: ")
                    .Append(_code.Literal(operation.Schema));
            }

            builder
                .Append(", table: ")
                .Append(_code.Literal(operation.Table))
                .Append(")");

            using (builder.Indent())
            {
                Annotations(operation.Annotations, builder);
            }
        }

        protected virtual void Generate([NotNull] RenameColumnOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".RenameColumn(");

            using (builder.Indent())
            {
                builder
                    .Append("name: ")
                    .Append(_code.Literal(operation.Name));

                if (operation.Schema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("schema: ")
                        .Append(_code.Literal(operation.Schema));
                }

                builder
                    .AppendLine(",")
                    .Append("table: ")
                    .Append(_code.Literal(operation.Table))
                    .AppendLine(",")
                    .Append("newName: ")
                    .Append(_code.Literal(operation.NewName))
                    .Append(")");

                Annotations(operation.Annotations, builder);
            }
        }

        protected virtual void Generate([NotNull] RenameIndexOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".RenameIndex(");

            using (builder.Indent())
            {
                builder
                    .Append("name: ")
                    .Append(_code.Literal(operation.Name));

                if (operation.Schema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("schema: ")
                        .Append(_code.Literal(operation.Schema));
                }

                builder
                    .AppendLine(",")
                    .Append("table: ")
                    .Append(_code.Literal(operation.Table))
                    .AppendLine(",")
                    .Append("newName: ")
                    .Append(_code.Literal(operation.NewName))
                    .Append(")");

                Annotations(operation.Annotations, builder);
            }
        }

        protected virtual void Generate([NotNull] RenameSequenceOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".RenameSequence(");

            using (builder.Indent())
            {
                builder
                    .Append("name: ")
                    .Append(_code.Literal(operation.Name));

                if (operation.Schema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("schema: ")
                        .Append(_code.Literal(operation.Schema));
                }

                if (operation.NewName != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("newName: ")
                        .Append(_code.Literal(operation.NewName));
                }

                if (operation.NewSchema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("newSchema: ")
                        .Append(_code.Literal(operation.NewSchema));
                }

                builder.Append(")");

                Annotations(operation.Annotations, builder);
            }
        }

        protected virtual void Generate([NotNull] RenameTableOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".RenameTable(");

            using (builder.Indent())
            {
                builder
                    .Append("name: ")
                    .Append(_code.Literal(operation.Name));

                if (operation.Schema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("schema: ")
                        .Append(_code.Literal(operation.Schema));
                }

                if (operation.NewName != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("newName: ")
                        .Append(_code.Literal(operation.NewName));
                }

                if (operation.NewSchema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("newSchema: ")
                        .Append(_code.Literal(operation.NewSchema));
                }

                builder.Append(")");

                Annotations(operation.Annotations, builder);
            }
        }

        protected virtual void Generate([NotNull] RestartSequenceOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".RestartSequence(");

            using (builder.Indent())
            {
                builder
                    .Append("name: ")
                    .Append(_code.Literal(operation.Name));

                if (operation.Schema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("schema: ")
                        .Append(_code.Literal(operation.Schema));
                }
                builder
                    .AppendLine(",")
                    .Append("with: ")
                    .Append(_code.Literal(operation.RestartWith))
                    .Append(")");

                Annotations(operation.Annotations, builder);
            }
        }

        protected virtual void Generate([NotNull] SqlOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".Sql(");

            using (builder.Indent())
            {
                if (operation.SuppressTransaction)
                {
                    builder.Append("sql: ");
                }

                builder.Append(_code.Literal(operation.Sql));

                if (operation.SuppressTransaction)
                {
                    builder
                        .AppendLine(",")
                        .Append("suppressTransaction: true");
                }

                builder.Append(")");

                Annotations(operation.Annotations, builder);
            }
        }

        protected virtual void Annotations(
            [NotNull] IEnumerable<Annotation> annotations,
            [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(annotations, nameof(annotations));
            Check.NotNull(builder, nameof(builder));

            foreach (var annotation in annotations)
            {
                // TODO: Give providers an opportunity to render these as provider-specific extension methods
                builder
                    .AppendLine()
                    .Append(".Annotation(")
                    .Append(_code.Literal(annotation.Name))
                    .Append(", ")
                    .Append(_code.UnknownLiteral(annotation.Value))
                    .Append(")");
            }
        }
    }
}
