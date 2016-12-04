// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations.Design
{
    public class CSharpMigrationOperationGenerator
    {
        private readonly CSharpHelper _code;

        public CSharpMigrationOperationGenerator([NotNull] CSharpHelper codeHelper)
        {
            Check.NotNull(codeHelper, nameof(codeHelper));

            _code = codeHelper;
        }

        public virtual void Generate(
            [NotNull] string builderName,
            [NotNull] IReadOnlyList<MigrationOperation> operations,
            [NotNull] IndentedStringBuilder builder)
        {
            Check.NotEmpty(builderName, nameof(builderName));
            Check.NotNull(operations, nameof(operations));
            Check.NotNull(builder, nameof(builder));

            var first = true;
            foreach (var operation in operations)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    builder
                        .AppendLine()
                        .AppendLine();
                }

                builder.Append(builderName);
                Generate((dynamic)operation, builder);
                builder.Append(";");
            }
        }

        protected virtual void Generate([NotNull] MigrationOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            throw new InvalidOperationException(DesignStrings.UnknownOperation(operation.GetType()));
        }

        protected virtual void Generate([NotNull] AddColumnOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(".AddColumn<")
                .Append(_code.Reference(operation.ClrType))
                .AppendLine(">(");

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
                    .Append(_code.Literal(operation.Table));

                if (operation.ColumnType != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("type: ")
                        .Append(_code.Literal(operation.ColumnType));
                }

                if (operation.IsUnicode == false)
                {
                    builder
                        .AppendLine(",")
                        .Append("unicode: false");
                }

                if (operation.MaxLength.HasValue)
                {
                    builder
                        .AppendLine(",")
                        .Append("maxLength: ")
                        .Append(_code.Literal(operation.MaxLength.Value));
                }

                if (operation.IsRowVersion)
                {
                    builder
                        .AppendLine(",")
                        .Append("rowVersion: true");
                }

                builder.AppendLine(",")
                    .Append("nullable: ")
                    .Append(_code.Literal(operation.IsNullable));

                if (operation.DefaultValueSql != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("defaultValueSql: ")
                        .Append(_code.Literal(operation.DefaultValueSql));
                }
                else if (operation.ComputedColumnSql != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("computedColumnSql: ")
                        .Append(_code.UnknownLiteral(operation.ComputedColumnSql));
                }
                else if (operation.DefaultValue != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("defaultValue: ")
                        .Append(_code.UnknownLiteral(operation.DefaultValue));
                }

                builder.Append(")");

                Annotations(operation.GetAnnotations(), builder);
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

                if (operation.PrincipalSchema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("principalSchema: ")
                        .Append(_code.Literal(operation.PrincipalSchema));
                }

                builder
                    .AppendLine(",")
                    .Append("principalTable: ")
                    .Append(_code.Literal(operation.PrincipalTable))
                    .AppendLine(",")
                    .Append(
                        operation.PrincipalColumns.Length == 1
                            ? "principalColumn: "
                            : "principalColumns: ")
                    .Append(_code.Literal(operation.PrincipalColumns));

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

                Annotations(operation.GetAnnotations(), builder);
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

                Annotations(operation.GetAnnotations(), builder);
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

                Annotations(operation.GetAnnotations(), builder);
            }
        }

        protected virtual void Generate([NotNull] AlterColumnOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(".AlterColumn<")
                .Append(_code.Reference(operation.ClrType))
                .AppendLine(">(");

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
                    .Append(_code.Literal(operation.Table));

                if (operation.ColumnType != null)
                {
                    builder.AppendLine(",")
                        .Append("type: ")
                        .Append(_code.Literal(operation.ColumnType));
                }

                if (operation.IsUnicode == false)
                {
                    builder
                        .AppendLine(",")
                        .Append("unicode: false");
                }

                if (operation.MaxLength.HasValue)
                {
                    builder.AppendLine(",")
                        .Append("maxLength: ")
                        .Append(_code.Literal(operation.MaxLength.Value));
                }

                if (operation.IsRowVersion)
                {
                    builder
                        .AppendLine(",")
                        .Append("rowVersion: true");
                }

                builder.AppendLine(",")
                    .Append("nullable: ")
                    .Append(_code.Literal(operation.IsNullable));

                if (operation.DefaultValueSql != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("defaultValueSql: ")
                        .Append(_code.Literal(operation.DefaultValueSql));
                }
                else if (operation.ComputedColumnSql != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("computedColumnSql: ")
                        .Append(_code.UnknownLiteral(operation.ComputedColumnSql));
                }
                else if (operation.DefaultValue != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("defaultValue: ")
                        .Append(_code.UnknownLiteral(operation.DefaultValue));
                }

                if (operation.OldColumn.ClrType != null)
                {
                    builder.AppendLine(",")
                        .Append("oldClrType: typeof(")
                        .Append(_code.Reference(operation.OldColumn.ClrType))
                        .Append(")");
                }

                if (operation.OldColumn.ColumnType != null)
                {
                    builder.AppendLine(",")
                        .Append("oldType: ")
                        .Append(_code.Literal(operation.OldColumn.ColumnType));
                }

                if (operation.OldColumn.IsUnicode == false)
                {
                    builder
                        .AppendLine(",")
                        .Append("oldUnicode: false");
                }

                if (operation.OldColumn.MaxLength.HasValue)
                {
                    builder.AppendLine(",")
                        .Append("oldMaxLength: ")
                        .Append(_code.Literal(operation.OldColumn.MaxLength.Value));
                }

                if (operation.OldColumn.IsRowVersion)
                {
                    builder
                        .AppendLine(",")
                        .Append("oldRowVersion: true");
                }

                if (operation.OldColumn.IsNullable)
                {
                    builder.AppendLine(",")
                        .Append("oldNullable: true");
                }

                if (operation.OldColumn.DefaultValueSql != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("oldDefaultValueSql: ")
                        .Append(_code.Literal(operation.OldColumn.DefaultValueSql));
                }
                else if (operation.OldColumn.ComputedColumnSql != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("oldComputedColumnSql: ")
                        .Append(_code.UnknownLiteral(operation.OldColumn.ComputedColumnSql));
                }
                else if (operation.OldColumn.DefaultValue != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("oldDefaultValue: ")
                        .Append(_code.UnknownLiteral(operation.OldColumn.DefaultValue));
                }

                builder.Append(")");

                Annotations(operation.GetAnnotations(), builder);
                OldAnnotations(operation.OldColumn.GetAnnotations(), builder);
            }
        }

        protected virtual void Generate([NotNull] AlterDatabaseOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.Append(".AlterDatabase()");

            using (builder.Indent())
            {
                Annotations(operation.GetAnnotations(), builder);
                OldAnnotations(operation.OldDatabase.GetAnnotations(), builder);
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

                if (operation.IncrementBy != 1)
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

                if (operation.IsCyclic)
                {
                    builder
                        .AppendLine(",")
                        .Append("cyclic: true");
                }

                if (operation.OldSequence.IncrementBy != 1)
                {
                    builder
                        .AppendLine(",")
                        .Append("oldIncrementBy: ")
                        .Append(_code.Literal(operation.OldSequence.IncrementBy));
                }

                if (operation.OldSequence.MinValue != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("oldMinValue: ")
                        .Append(_code.Literal(operation.OldSequence.MinValue));
                }

                if (operation.OldSequence.MaxValue != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("oldMaxValue: ")
                        .Append(_code.Literal(operation.OldSequence.MaxValue));
                }

                if (operation.OldSequence.IsCyclic)
                {
                    builder
                        .AppendLine(",")
                        .Append("oldCyclic: true");
                }

                builder.Append(")");

                Annotations(operation.GetAnnotations(), builder);
                OldAnnotations(operation.OldSequence.GetAnnotations(), builder);
            }
        }

        protected virtual void Generate([NotNull] AlterTableOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".AlterTable(");

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

                builder.Append(")");

                Annotations(operation.GetAnnotations(), builder);
                OldAnnotations(operation.OldTable.GetAnnotations(), builder);
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

                if (operation.Filter != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("filter: ")
                        .Append(operation.Filter);
                }

                builder.Append(")");

                Annotations(operation.GetAnnotations(), builder);
            }
        }

        protected virtual void Generate([NotNull] EnsureSchemaOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".EnsureSchema(");

            using (builder.Indent())
            {
                builder
                    .Append("name: ")
                    .Append(_code.Literal(operation.Name))
                    .Append(")");

                Annotations(operation.GetAnnotations(), builder);
            }
        }

        protected virtual void Generate([NotNull] CreateSequenceOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.Append(".CreateSequence");

            if (operation.ClrType != typeof(long))
            {
                builder
                    .Append("<")
                    .Append(_code.Reference(operation.ClrType))
                    .Append(">");
            }

            builder.AppendLine("(");

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

                if (operation.StartValue != 1L)
                {
                    builder
                        .AppendLine(",")
                        .Append("startValue: ")
                        .Append(_code.Literal(operation.StartValue));
                }

                if (operation.IncrementBy != 1)
                {
                    builder
                        .AppendLine(",")
                        .Append("incrementBy: ")
                        .Append(_code.Literal(operation.IncrementBy));
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

                if (operation.IsCyclic)
                {
                    builder
                        .AppendLine(",")
                        .Append("cyclic: true");
                }

                builder.Append(")");

                Annotations(operation.GetAnnotations(), builder);
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
                            .Append(" = table.Column<")
                            .Append(_code.Reference(column.ClrType))
                            .Append(">(");

                        if (propertyName != column.Name)
                        {
                            builder
                                .Append("name: ")
                                .Append(_code.Literal(column.Name))
                                .Append(", ");
                        }

                        if (column.ColumnType != null)
                        {
                            builder
                                .Append("type: ")
                                .Append(_code.Literal(column.ColumnType))
                                .Append(", ");
                        }

                        if (column.IsUnicode == false)
                        {
                            builder.Append("unicode: false, ");
                        }

                        if (column.MaxLength.HasValue)
                        {
                            builder
                                .Append("maxLength: ")
                                .Append(_code.Literal(column.MaxLength.Value))
                                .Append(", ");
                        }

                        if (column.IsRowVersion)
                        {
                            builder.Append("rowVersion: true, ");
                        }

                        builder.Append("nullable: ")
                            .Append(_code.Literal(column.IsNullable));

                        if (column.DefaultValueSql != null)
                        {
                            builder
                                .Append(", defaultValueSql: ")
                                .Append(_code.Literal(column.DefaultValueSql));
                        }
                        else if (column.ComputedColumnSql != null)
                        {
                            builder
                                .Append(", computedColumnSql: ")
                                .Append(_code.Literal(column.ComputedColumnSql));
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
                            Annotations(column.GetAnnotations(), builder);
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
                            Annotations(operation.PrimaryKey.GetAnnotations(), builder);
                        }

                        builder.AppendLine(";");
                    }

                    foreach (var uniqueConstraint in operation.UniqueConstraints)
                    {
                        builder
                            .Append("table.UniqueConstraint(")
                            .Append(_code.Literal(uniqueConstraint.Name))
                            .Append(", ")
                            .Append(_code.Lambda(uniqueConstraint.Columns.Select(c => map[c]).ToList()))
                            .Append(")");

                        using (builder.Indent())
                        {
                            Annotations(uniqueConstraint.GetAnnotations(), builder);
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
                                .Append(foreignKey.Columns.Length == 1
                                    ? "column: "
                                    : "columns: ")
                                .Append(_code.Lambda(foreignKey.Columns.Select(c => map[c]).ToList()));

                            if (foreignKey.PrincipalSchema != null)
                            {
                                builder
                                    .AppendLine(",")
                                    .Append("principalSchema: ")
                                    .Append(_code.Literal(foreignKey.PrincipalSchema));
                            }

                            builder
                                .AppendLine(",")
                                .Append("principalTable: ")
                                .Append(_code.Literal(foreignKey.PrincipalTable))
                                .AppendLine(",")
                                .Append(
                                    foreignKey.PrincipalColumns.Length == 1
                                        ? "principalColumn: "
                                        : "principalColumns: ")
                                .Append(_code.Literal(foreignKey.PrincipalColumns));

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

                            Annotations(foreignKey.GetAnnotations(), builder);
                        }

                        builder.AppendLine(";");
                    }
                }

                builder.Append("})");

                Annotations(operation.GetAnnotations(), builder);
            }
        }

        protected virtual void Generate([NotNull] DropColumnOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".DropColumn(");

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
                    .Append(")");

                Annotations(operation.GetAnnotations(), builder);
            }
        }

        protected virtual void Generate([NotNull] DropForeignKeyOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".DropForeignKey(");

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
                    .Append(")");

                Annotations(operation.GetAnnotations(), builder);
            }
        }

        protected virtual void Generate([NotNull] DropIndexOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".DropIndex(");

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
                    .Append(")");

                Annotations(operation.GetAnnotations(), builder);
            }
        }

        protected virtual void Generate([NotNull] DropPrimaryKeyOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".DropPrimaryKey(");

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
                    .Append(")");

                Annotations(operation.GetAnnotations(), builder);
            }
        }

        protected virtual void Generate([NotNull] DropSchemaOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".DropSchema(");

            using (builder.Indent())
            {
                builder
                    .Append("name: ")
                    .Append(_code.Literal(operation.Name))
                    .Append(")");

                Annotations(operation.GetAnnotations(), builder);
            }
        }

        protected virtual void Generate([NotNull] DropSequenceOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".DropSequence(");

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

                builder.Append(")");

                Annotations(operation.GetAnnotations(), builder);
            }
        }

        protected virtual void Generate([NotNull] DropTableOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".DropTable(");

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

                builder.Append(")");

                Annotations(operation.GetAnnotations(), builder);
            }
        }

        protected virtual void Generate([NotNull] DropUniqueConstraintOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".DropUniqueConstraint(");

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
                    .Append(")");

                Annotations(operation.GetAnnotations(), builder);
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

                Annotations(operation.GetAnnotations(), builder);
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

                Annotations(operation.GetAnnotations(), builder);
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

                Annotations(operation.GetAnnotations(), builder);
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

                Annotations(operation.GetAnnotations(), builder);
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
                    .Append("startValue: ")
                    .Append(_code.Literal(operation.StartValue))
                    .Append(")");

                Annotations(operation.GetAnnotations(), builder);
            }
        }

        protected virtual void Generate([NotNull] SqlOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(".Sql(")
                .Append(_code.Literal(operation.Sql))
                .Append(")");

            using (builder.Indent())
            {
                Annotations(operation.GetAnnotations(), builder);
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

        protected virtual void OldAnnotations(
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
                    .Append(".OldAnnotation(")
                    .Append(_code.Literal(annotation.Name))
                    .Append(", ")
                    .Append(_code.UnknownLiteral(annotation.Value))
                    .Append(")");
            }
        }
    }
}
