// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations.Design
{
    /// <summary>
    ///     Used to generate C# for creating <see cref="MigrationOperation" /> objects.
    /// </summary>
    public class CSharpMigrationOperationGenerator : ICSharpMigrationOperationGenerator
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CSharpMigrationOperationGenerator" /> class.
        /// </summary>
        /// <param name="dependencies"> The dependencies. </param>
        public CSharpMigrationOperationGenerator([NotNull] CSharpMigrationOperationGeneratorDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing dependencies for this service.
        /// </summary>
        protected virtual CSharpMigrationOperationGeneratorDependencies Dependencies { get; }

        private ICSharpHelper Code => Dependencies.CSharpHelper;

        /// <summary>
        ///     Generates code for creating <see cref="MigrationOperation" /> objects.
        /// </summary>
        /// <param name="builderName"> The <see cref="MigrationOperation" /> variable name. </param>
        /// <param name="operations"> The operations. </param>
        /// <param name="builder"> The builder code is added to. </param>
        public virtual void Generate(
            string builderName,
            IReadOnlyList<MigrationOperation> operations,
            IndentedStringBuilder builder)
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

        /// <summary>
        ///     Generates code for an unknown <see cref="MigrationOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="builder"> The builder code is added to. </param>
        protected virtual void Generate([NotNull] MigrationOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            throw new InvalidOperationException(DesignStrings.UnknownOperation(operation.GetType()));
        }

        /// <summary>
        ///     Generates code for an <see cref="AddColumnOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="builder"> The builder code is added to. </param>
        protected virtual void Generate([NotNull] AddColumnOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(".AddColumn<")
                .Append(Code.Reference(operation.ClrType))
                .AppendLine(">(");

            using (builder.Indent())
            {
                builder
                    .Append("name: ")
                    .Append(Code.Literal(operation.Name));

                if (operation.Schema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("schema: ")
                        .Append(Code.Literal(operation.Schema));
                }

                builder
                    .AppendLine(",")
                    .Append("table: ")
                    .Append(Code.Literal(operation.Table));

                if (operation.ColumnType != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("type: ")
                        .Append(Code.Literal(operation.ColumnType));
                }

                if (operation.IsUnicode == false)
                {
                    builder
                        .AppendLine(",")
                        .Append("unicode: false");
                }

                if (operation.IsFixedLength == true)
                {
                    builder
                        .AppendLine(",")
                        .Append("fixedLength: true");
                }

                if (operation.MaxLength.HasValue)
                {
                    builder
                        .AppendLine(",")
                        .Append("maxLength: ")
                        .Append(Code.Literal(operation.MaxLength.Value));
                }

                if (operation.IsRowVersion)
                {
                    builder
                        .AppendLine(",")
                        .Append("rowVersion: true");
                }

                builder.AppendLine(",")
                    .Append("nullable: ")
                    .Append(Code.Literal(operation.IsNullable));

                if (operation.DefaultValueSql != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("defaultValueSql: ")
                        .Append(Code.Literal(operation.DefaultValueSql));
                }
                else if (operation.ComputedColumnSql != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("computedColumnSql: ")
                        .Append(Code.UnknownLiteral(operation.ComputedColumnSql));
                }
                else if (operation.DefaultValue != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("defaultValue: ")
                        .Append(Code.UnknownLiteral(operation.DefaultValue));
                }

                builder.Append(")");

                Annotations(operation.GetAnnotations(), builder);
            }
        }

        /// <summary>
        ///     Generates code for an <see cref="AddForeignKeyOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="builder"> The builder code is added to. </param>
        protected virtual void Generate([NotNull] AddForeignKeyOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".AddForeignKey(");

            using (builder.Indent())
            {
                builder
                    .Append("name: ")
                    .Append(Code.Literal(operation.Name));

                if (operation.Schema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("schema: ")
                        .Append(Code.Literal(operation.Schema));
                }

                builder
                    .AppendLine(",")
                    .Append("table: ")
                    .Append(Code.Literal(operation.Table))
                    .AppendLine(",");

                if (operation.Columns.Length == 1)
                {
                    builder
                        .Append("column: ")
                        .Append(Code.Literal(operation.Columns[0]));
                }
                else
                {
                    builder
                        .Append("columns: ")
                        .Append(Code.Literal(operation.Columns));
                }

                if (operation.PrincipalSchema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("principalSchema: ")
                        .Append(Code.Literal(operation.PrincipalSchema));
                }

                builder
                    .AppendLine(",")
                    .Append("principalTable: ")
                    .Append(Code.Literal(operation.PrincipalTable))
                    .AppendLine(",");

                if (operation.PrincipalColumns.Length == 1)
                {
                    builder
                        .Append("principalColumn: ")
                        .Append(Code.Literal(operation.PrincipalColumns[0]));
                }
                else
                {
                    builder
                        .Append("principalColumns: ")
                        .Append(Code.Literal(operation.PrincipalColumns));
                }

                if (operation.OnUpdate != ReferentialAction.NoAction)
                {
                    builder
                        .AppendLine(",")
                        .Append("onUpdate: ")
                        .Append(Code.Literal(operation.OnUpdate));
                }

                if (operation.OnDelete != ReferentialAction.NoAction)
                {
                    builder
                        .AppendLine(",")
                        .Append("onDelete: ")
                        .Append(Code.Literal(operation.OnDelete));
                }

                builder.Append(")");

                Annotations(operation.GetAnnotations(), builder);
            }
        }

        /// <summary>
        ///     Generates code for an <see cref="AddPrimaryKeyOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="builder"> The builder code is added to. </param>
        protected virtual void Generate([NotNull] AddPrimaryKeyOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".AddPrimaryKey(");

            using (builder.Indent())
            {
                builder
                    .Append("name: ")
                    .Append(Code.Literal(operation.Name));

                if (operation.Schema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("schema: ")
                        .Append(Code.Literal(operation.Schema));
                }

                builder
                    .AppendLine(",")
                    .Append("table: ")
                    .Append(Code.Literal(operation.Table))
                    .AppendLine(",");

                if (operation.Columns.Length == 1)
                {
                    builder
                        .Append("column: ")
                        .Append(Code.Literal(operation.Columns[0]));
                }
                else
                {
                    builder
                        .Append("columns: ")
                        .Append(Code.Literal(operation.Columns));
                }

                builder.Append(")");

                Annotations(operation.GetAnnotations(), builder);
            }
        }

        /// <summary>
        ///     Generates code for an <see cref="AddUniqueConstraintOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="builder"> The builder code is added to. </param>
        protected virtual void Generate([NotNull] AddUniqueConstraintOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".AddUniqueConstraint(");

            using (builder.Indent())
            {
                builder
                    .Append("name: ")
                    .Append(Code.Literal(operation.Name));

                if (operation.Schema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("schema: ")
                        .Append(Code.Literal(operation.Schema));
                }

                builder
                    .AppendLine(",")
                    .Append("table: ")
                    .Append(Code.Literal(operation.Table))
                    .AppendLine(",");

                if (operation.Columns.Length == 1)
                {
                    builder
                        .Append("column: ")
                        .Append(Code.Literal(operation.Columns[0]));
                }
                else
                {
                    builder
                        .Append("columns: ")
                        .Append(Code.Literal(operation.Columns));
                }

                builder.Append(")");

                Annotations(operation.GetAnnotations(), builder);
            }
        }

        /// <summary>
        ///     Generates code for an <see cref="AlterColumnOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="builder"> The builder code is added to. </param>
        protected virtual void Generate([NotNull] AlterColumnOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(".AlterColumn<")
                .Append(Code.Reference(operation.ClrType))
                .AppendLine(">(");

            using (builder.Indent())
            {
                builder
                    .Append("name: ")
                    .Append(Code.Literal(operation.Name));

                if (operation.Schema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("schema: ")
                        .Append(Code.Literal(operation.Schema));
                }

                builder
                    .AppendLine(",")
                    .Append("table: ")
                    .Append(Code.Literal(operation.Table));

                if (operation.ColumnType != null)
                {
                    builder.AppendLine(",")
                        .Append("type: ")
                        .Append(Code.Literal(operation.ColumnType));
                }

                if (operation.IsUnicode == false)
                {
                    builder
                        .AppendLine(",")
                        .Append("unicode: false");
                }

                if (operation.IsFixedLength == true)
                {
                    builder
                        .AppendLine(",")
                        .Append("fixedLength: true");
                }

                if (operation.MaxLength.HasValue)
                {
                    builder.AppendLine(",")
                        .Append("maxLength: ")
                        .Append(Code.Literal(operation.MaxLength.Value));
                }

                if (operation.IsRowVersion)
                {
                    builder
                        .AppendLine(",")
                        .Append("rowVersion: true");
                }

                builder.AppendLine(",")
                    .Append("nullable: ")
                    .Append(Code.Literal(operation.IsNullable));

                if (operation.DefaultValueSql != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("defaultValueSql: ")
                        .Append(Code.Literal(operation.DefaultValueSql));
                }
                else if (operation.ComputedColumnSql != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("computedColumnSql: ")
                        .Append(Code.UnknownLiteral(operation.ComputedColumnSql));
                }
                else if (operation.DefaultValue != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("defaultValue: ")
                        .Append(Code.UnknownLiteral(operation.DefaultValue));
                }

                if (operation.OldColumn.ClrType != null)
                {
                    builder.AppendLine(",")
                        .Append("oldClrType: typeof(")
                        .Append(Code.Reference(operation.OldColumn.ClrType))
                        .Append(")");
                }

                if (operation.OldColumn.ColumnType != null)
                {
                    builder.AppendLine(",")
                        .Append("oldType: ")
                        .Append(Code.Literal(operation.OldColumn.ColumnType));
                }

                if (operation.OldColumn.IsUnicode == false)
                {
                    builder
                        .AppendLine(",")
                        .Append("oldUnicode: false");
                }

                if (operation.OldColumn.IsFixedLength == true)
                {
                    builder
                        .AppendLine(",")
                        .Append("oldFixedLength: true");
                }

                if (operation.OldColumn.MaxLength.HasValue)
                {
                    builder.AppendLine(",")
                        .Append("oldMaxLength: ")
                        .Append(Code.Literal(operation.OldColumn.MaxLength.Value));
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
                        .Append(Code.Literal(operation.OldColumn.DefaultValueSql));
                }
                else if (operation.OldColumn.ComputedColumnSql != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("oldComputedColumnSql: ")
                        .Append(Code.UnknownLiteral(operation.OldColumn.ComputedColumnSql));
                }
                else if (operation.OldColumn.DefaultValue != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("oldDefaultValue: ")
                        .Append(Code.UnknownLiteral(operation.OldColumn.DefaultValue));
                }

                builder.Append(")");

                Annotations(operation.GetAnnotations(), builder);
                OldAnnotations(operation.OldColumn.GetAnnotations(), builder);
            }
        }

        /// <summary>
        ///     Generates code for an <see cref="AlterDatabaseOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="builder"> The builder code is added to. </param>
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

        /// <summary>
        ///     Generates code for an <see cref="AlterSequenceOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="builder"> The builder code is added to. </param>
        protected virtual void Generate([NotNull] AlterSequenceOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".AlterSequence(");

            using (builder.Indent())
            {
                builder
                    .Append("name: ")
                    .Append(Code.Literal(operation.Name));

                if (operation.Schema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("schema: ")
                        .Append(Code.Literal(operation.Schema));
                }

                if (operation.IncrementBy != 1)
                {
                    builder
                        .AppendLine(",")
                        .Append("incrementBy: ")
                        .Append(Code.Literal(operation.IncrementBy));
                }

                if (operation.MinValue != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("minValue: ")
                        .Append(Code.Literal(operation.MinValue));
                }

                if (operation.MaxValue != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("maxValue: ")
                        .Append(Code.Literal(operation.MaxValue));
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
                        .Append(Code.Literal(operation.OldSequence.IncrementBy));
                }

                if (operation.OldSequence.MinValue != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("oldMinValue: ")
                        .Append(Code.Literal(operation.OldSequence.MinValue));
                }

                if (operation.OldSequence.MaxValue != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("oldMaxValue: ")
                        .Append(Code.Literal(operation.OldSequence.MaxValue));
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

        /// <summary>
        ///     Generates code for an <see cref="AlterTableOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="builder"> The builder code is added to. </param>
        protected virtual void Generate([NotNull] AlterTableOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".AlterTable(");

            using (builder.Indent())
            {
                builder
                    .Append("name: ")
                    .Append(Code.Literal(operation.Name));

                if (operation.Schema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("schema: ")
                        .Append(Code.Literal(operation.Schema));
                }

                builder.Append(")");

                Annotations(operation.GetAnnotations(), builder);
                OldAnnotations(operation.OldTable.GetAnnotations(), builder);
            }
        }

        /// <summary>
        ///     Generates code for a <see cref="CreateIndexOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="builder"> The builder code is added to. </param>
        protected virtual void Generate([NotNull] CreateIndexOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".CreateIndex(");

            using (builder.Indent())
            {
                builder
                    .Append("name: ")
                    .Append(Code.Literal(operation.Name));

                if (operation.Schema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("schema: ")
                        .Append(Code.Literal(operation.Schema));
                }

                builder
                    .AppendLine(",")
                    .Append("table: ")
                    .Append(Code.Literal(operation.Table))
                    .AppendLine(",");

                if (operation.Columns.Length == 1)
                {
                    builder
                        .Append("column: ")
                        .Append(Code.Literal(operation.Columns[0]));
                }
                else
                {
                    builder
                        .Append("columns: ")
                        .Append(Code.Literal(operation.Columns));
                }

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
                        .Append(Code.Literal(operation.Filter));
                }

                builder.Append(")");

                Annotations(operation.GetAnnotations(), builder);
            }
        }

        /// <summary>
        ///     Generates code for an <see cref="EnsureSchemaOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="builder"> The builder code is added to. </param>
        protected virtual void Generate([NotNull] EnsureSchemaOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".EnsureSchema(");

            using (builder.Indent())
            {
                builder
                    .Append("name: ")
                    .Append(Code.Literal(operation.Name))
                    .Append(")");

                Annotations(operation.GetAnnotations(), builder);
            }
        }

        /// <summary>
        ///     Generates code for a <see cref="CreateSequenceOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="builder"> The builder code is added to. </param>
        protected virtual void Generate([NotNull] CreateSequenceOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.Append(".CreateSequence");

            if (operation.ClrType != typeof(long))
            {
                builder
                    .Append("<")
                    .Append(Code.Reference(operation.ClrType))
                    .Append(">");
            }

            builder.AppendLine("(");

            using (builder.Indent())
            {
                builder
                    .Append("name: ")
                    .Append(Code.Literal(operation.Name));

                if (operation.Schema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("schema: ")
                        .Append(Code.Literal(operation.Schema));
                }

                if (operation.StartValue != 1L)
                {
                    builder
                        .AppendLine(",")
                        .Append("startValue: ")
                        .Append(Code.Literal(operation.StartValue));
                }

                if (operation.IncrementBy != 1)
                {
                    builder
                        .AppendLine(",")
                        .Append("incrementBy: ")
                        .Append(Code.Literal(operation.IncrementBy));
                }

                if (operation.MinValue.HasValue)
                {
                    builder
                        .AppendLine(",")
                        .Append("minValue: ")
                        .Append(Code.Literal(operation.MinValue.Value));
                }

                if (operation.MaxValue.HasValue)
                {
                    builder
                        .AppendLine(",")
                        .Append("maxValue: ")
                        .Append(Code.Literal(operation.MaxValue.Value));
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

        /// <summary>
        ///     Generates code for a <see cref="CreateTableOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="builder"> The builder code is added to. </param>
        protected virtual void Generate([NotNull] CreateTableOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".CreateTable(");

            using (builder.Indent())
            {
                builder
                    .Append("name: ")
                    .Append(Code.Literal(operation.Name));

                if (operation.Schema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("schema: ")
                        .Append(Code.Literal(operation.Schema));
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
                        var propertyName = Code.Identifier(column.Name, scope);
                        map.Add(column.Name, propertyName);

                        builder
                            .Append(propertyName)
                            .Append(" = table.Column<")
                            .Append(Code.Reference(column.ClrType))
                            .Append(">(");

                        if (propertyName != column.Name)
                        {
                            builder
                                .Append("name: ")
                                .Append(Code.Literal(column.Name))
                                .Append(", ");
                        }

                        if (column.ColumnType != null)
                        {
                            builder
                                .Append("type: ")
                                .Append(Code.Literal(column.ColumnType))
                                .Append(", ");
                        }

                        if (column.IsUnicode == false)
                        {
                            builder.Append("unicode: false, ");
                        }

                        if (column.IsFixedLength == true)
                        {
                            builder.Append("fixedLength: true, ");
                        }

                        if (column.MaxLength.HasValue)
                        {
                            builder
                                .Append("maxLength: ")
                                .Append(Code.Literal(column.MaxLength.Value))
                                .Append(", ");
                        }

                        if (column.IsRowVersion)
                        {
                            builder.Append("rowVersion: true, ");
                        }

                        builder.Append("nullable: ")
                            .Append(Code.Literal(column.IsNullable));

                        if (column.DefaultValueSql != null)
                        {
                            builder
                                .Append(", defaultValueSql: ")
                                .Append(Code.Literal(column.DefaultValueSql));
                        }
                        else if (column.ComputedColumnSql != null)
                        {
                            builder
                                .Append(", computedColumnSql: ")
                                .Append(Code.Literal(column.ComputedColumnSql));
                        }
                        else if (column.DefaultValue != null)
                        {
                            builder
                                .Append(", defaultValue: ")
                                .Append(Code.UnknownLiteral(column.DefaultValue));
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
                            .Append(Code.Literal(operation.PrimaryKey.Name))
                            .Append(", ")
                            .Append(Code.Lambda(operation.PrimaryKey.Columns.Select(c => map[c]).ToList()))
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
                            .Append(Code.Literal(uniqueConstraint.Name))
                            .Append(", ")
                            .Append(Code.Lambda(uniqueConstraint.Columns.Select(c => map[c]).ToList()))
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
                                .Append(Code.Literal(foreignKey.Name))
                                .AppendLine(",")
                                .Append(
                                    foreignKey.Columns.Length == 1
                                        ? "column: "
                                        : "columns: ")
                                .Append(Code.Lambda(foreignKey.Columns.Select(c => map[c]).ToList()));

                            if (foreignKey.PrincipalSchema != null)
                            {
                                builder
                                    .AppendLine(",")
                                    .Append("principalSchema: ")
                                    .Append(Code.Literal(foreignKey.PrincipalSchema));
                            }

                            builder
                                .AppendLine(",")
                                .Append("principalTable: ")
                                .Append(Code.Literal(foreignKey.PrincipalTable))
                                .AppendLine(",");

                            if (foreignKey.PrincipalColumns.Length == 1)
                            {
                                builder
                                    .Append("principalColumn: ")
                                    .Append(Code.Literal(foreignKey.PrincipalColumns[0]));
                            }
                            else
                            {
                                builder
                                    .Append("principalColumns: ")
                                    .Append(Code.Literal(foreignKey.PrincipalColumns));
                            }

                            if (foreignKey.OnUpdate != ReferentialAction.NoAction)
                            {
                                builder
                                    .AppendLine(",")
                                    .Append("onUpdate: ")
                                    .Append(Code.Literal(foreignKey.OnUpdate));
                            }

                            if (foreignKey.OnDelete != ReferentialAction.NoAction)
                            {
                                builder
                                    .AppendLine(",")
                                    .Append("onDelete: ")
                                    .Append(Code.Literal(foreignKey.OnDelete));
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

        /// <summary>
        ///     Generates code for a <see cref="DropColumnOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="builder"> The builder code is added to. </param>
        protected virtual void Generate([NotNull] DropColumnOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".DropColumn(");

            using (builder.Indent())
            {
                builder
                    .Append("name: ")
                    .Append(Code.Literal(operation.Name));

                if (operation.Schema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("schema: ")
                        .Append(Code.Literal(operation.Schema));
                }

                builder
                    .AppendLine(",")
                    .Append("table: ")
                    .Append(Code.Literal(operation.Table))
                    .Append(")");

                Annotations(operation.GetAnnotations(), builder);
            }
        }

        /// <summary>
        ///     Generates code for a <see cref="DropForeignKeyOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="builder"> The builder code is added to. </param>
        protected virtual void Generate([NotNull] DropForeignKeyOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".DropForeignKey(");

            using (builder.Indent())
            {
                builder
                    .Append("name: ")
                    .Append(Code.Literal(operation.Name));

                if (operation.Schema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("schema: ")
                        .Append(Code.Literal(operation.Schema));
                }

                builder
                    .AppendLine(",")
                    .Append("table: ")
                    .Append(Code.Literal(operation.Table))
                    .Append(")");

                Annotations(operation.GetAnnotations(), builder);
            }
        }

        /// <summary>
        ///     Generates code for a <see cref="DropIndexOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="builder"> The builder code is added to. </param>
        protected virtual void Generate([NotNull] DropIndexOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".DropIndex(");

            using (builder.Indent())
            {
                builder
                    .Append("name: ")
                    .Append(Code.Literal(operation.Name));

                if (operation.Schema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("schema: ")
                        .Append(Code.Literal(operation.Schema));
                }

                builder
                    .AppendLine(",")
                    .Append("table: ")
                    .Append(Code.Literal(operation.Table))
                    .Append(")");

                Annotations(operation.GetAnnotations(), builder);
            }
        }

        /// <summary>
        ///     Generates code for a <see cref="DropPrimaryKeyOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="builder"> The builder code is added to. </param>
        protected virtual void Generate([NotNull] DropPrimaryKeyOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".DropPrimaryKey(");

            using (builder.Indent())
            {
                builder
                    .Append("name: ")
                    .Append(Code.Literal(operation.Name));

                if (operation.Schema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("schema: ")
                        .Append(Code.Literal(operation.Schema));
                }

                builder
                    .AppendLine(",")
                    .Append("table: ")
                    .Append(Code.Literal(operation.Table))
                    .Append(")");

                Annotations(operation.GetAnnotations(), builder);
            }
        }

        /// <summary>
        ///     Generates code for a <see cref="DropSchemaOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="builder"> The builder code is added to. </param>
        protected virtual void Generate([NotNull] DropSchemaOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".DropSchema(");

            using (builder.Indent())
            {
                builder
                    .Append("name: ")
                    .Append(Code.Literal(operation.Name))
                    .Append(")");

                Annotations(operation.GetAnnotations(), builder);
            }
        }

        /// <summary>
        ///     Generates code for a <see cref="DropSequenceOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="builder"> The builder code is added to. </param>
        protected virtual void Generate([NotNull] DropSequenceOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".DropSequence(");

            using (builder.Indent())
            {
                builder
                    .Append("name: ")
                    .Append(Code.Literal(operation.Name));

                if (operation.Schema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("schema: ")
                        .Append(Code.Literal(operation.Schema));
                }

                builder.Append(")");

                Annotations(operation.GetAnnotations(), builder);
            }
        }

        /// <summary>
        ///     Generates code for a <see cref="DropTableOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="builder"> The builder code is added to. </param>
        protected virtual void Generate([NotNull] DropTableOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".DropTable(");

            using (builder.Indent())
            {
                builder
                    .Append("name: ")
                    .Append(Code.Literal(operation.Name));

                if (operation.Schema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("schema: ")
                        .Append(Code.Literal(operation.Schema));
                }

                builder.Append(")");

                Annotations(operation.GetAnnotations(), builder);
            }
        }

        /// <summary>
        ///     Generates code for a <see cref="DropUniqueConstraintOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="builder"> The builder code is added to. </param>
        protected virtual void Generate([NotNull] DropUniqueConstraintOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".DropUniqueConstraint(");

            using (builder.Indent())
            {
                builder
                    .Append("name: ")
                    .Append(Code.Literal(operation.Name));

                if (operation.Schema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("schema: ")
                        .Append(Code.Literal(operation.Schema));
                }

                builder
                    .AppendLine(",")
                    .Append("table: ")
                    .Append(Code.Literal(operation.Table))
                    .Append(")");

                Annotations(operation.GetAnnotations(), builder);
            }
        }

        /// <summary>
        ///     Generates code for a <see cref="RenameColumnOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="builder"> The builder code is added to. </param>
        protected virtual void Generate([NotNull] RenameColumnOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".RenameColumn(");

            using (builder.Indent())
            {
                builder
                    .Append("name: ")
                    .Append(Code.Literal(operation.Name));

                if (operation.Schema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("schema: ")
                        .Append(Code.Literal(operation.Schema));
                }

                builder
                    .AppendLine(",")
                    .Append("table: ")
                    .Append(Code.Literal(operation.Table))
                    .AppendLine(",")
                    .Append("newName: ")
                    .Append(Code.Literal(operation.NewName))
                    .Append(")");

                Annotations(operation.GetAnnotations(), builder);
            }
        }

        /// <summary>
        ///     Generates code for a <see cref="RenameIndexOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="builder"> The builder code is added to. </param>
        protected virtual void Generate([NotNull] RenameIndexOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".RenameIndex(");

            using (builder.Indent())
            {
                builder
                    .Append("name: ")
                    .Append(Code.Literal(operation.Name));

                if (operation.Schema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("schema: ")
                        .Append(Code.Literal(operation.Schema));
                }

                builder
                    .AppendLine(",")
                    .Append("table: ")
                    .Append(Code.Literal(operation.Table))
                    .AppendLine(",")
                    .Append("newName: ")
                    .Append(Code.Literal(operation.NewName))
                    .Append(")");

                Annotations(operation.GetAnnotations(), builder);
            }
        }

        /// <summary>
        ///     Generates code for a <see cref="RenameSequenceOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="builder"> The builder code is added to. </param>
        protected virtual void Generate([NotNull] RenameSequenceOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".RenameSequence(");

            using (builder.Indent())
            {
                builder
                    .Append("name: ")
                    .Append(Code.Literal(operation.Name));

                if (operation.Schema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("schema: ")
                        .Append(Code.Literal(operation.Schema));
                }

                if (operation.NewName != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("newName: ")
                        .Append(Code.Literal(operation.NewName));
                }

                if (operation.NewSchema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("newSchema: ")
                        .Append(Code.Literal(operation.NewSchema));
                }

                builder.Append(")");

                Annotations(operation.GetAnnotations(), builder);
            }
        }

        /// <summary>
        ///     Generates code for a <see cref="RenameTableOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="builder"> The builder code is added to. </param>
        protected virtual void Generate([NotNull] RenameTableOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".RenameTable(");

            using (builder.Indent())
            {
                builder
                    .Append("name: ")
                    .Append(Code.Literal(operation.Name));

                if (operation.Schema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("schema: ")
                        .Append(Code.Literal(operation.Schema));
                }

                if (operation.NewName != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("newName: ")
                        .Append(Code.Literal(operation.NewName));
                }

                if (operation.NewSchema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("newSchema: ")
                        .Append(Code.Literal(operation.NewSchema));
                }

                builder.Append(")");

                Annotations(operation.GetAnnotations(), builder);
            }
        }

        /// <summary>
        ///     Generates code for a <see cref="RestartSequenceOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="builder"> The builder code is added to. </param>
        protected virtual void Generate([NotNull] RestartSequenceOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".RestartSequence(");

            using (builder.Indent())
            {
                builder
                    .Append("name: ")
                    .Append(Code.Literal(operation.Name));

                if (operation.Schema != null)
                {
                    builder
                        .AppendLine(",")
                        .Append("schema: ")
                        .Append(Code.Literal(operation.Schema));
                }
                builder
                    .AppendLine(",")
                    .Append("startValue: ")
                    .Append(Code.Literal(operation.StartValue))
                    .Append(")");

                Annotations(operation.GetAnnotations(), builder);
            }
        }

        /// <summary>
        ///     Generates code for a <see cref="SqlOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="builder"> The builder code is added to. </param>
        protected virtual void Generate([NotNull] SqlOperation operation, [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(".Sql(")
                .Append(Code.Literal(operation.Sql))
                .Append(")");

            using (builder.Indent())
            {
                Annotations(operation.GetAnnotations(), builder);
            }
        }

        /// <summary>
        ///     Generates code for an <see cref="InsertDataOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="builder"> The builder code is added to. </param>
        protected virtual void Generate(
            [NotNull] InsertDataOperation operation,
            [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".InsertData(");

            using (builder.Indent())
            {
                if (operation.Schema != null)
                {
                    builder
                        .Append("schema: ")
                        .Append(Code.Literal(operation.Schema))
                        .AppendLine(",");
                }

                builder
                    .Append("table: ")
                    .Append(Code.Literal(operation.Table))
                    .AppendLine(",");

                if (operation.Columns.Length == 1)
                {
                    builder
                        .Append("column: ")
                        .Append(Code.Literal(operation.Columns[0]));
                }
                else
                {
                    builder
                        .Append("columns: ")
                        .Append(Code.Literal(operation.Columns));
                }

                builder.AppendLine(",");

                if (operation.Values.GetLength(0) == 1
                    && operation.Values.GetLength(1) == 1)
                {
                    builder
                        .Append("value: ")
                        .Append(Code.UnknownLiteral(operation.Values[0, 0]));
                }
                else if (operation.Values.GetLength(0) == 1)
                {
                    builder
                        .Append("values: ")
                        .Append(Code.Literal(ToOnedimensionalArray(operation.Values)));
                }
                else if (operation.Values.GetLength(1) == 1)
                {
                    builder
                        .Append("values: ")
                        .AppendLines(
                            Code.Literal(
                                ToOnedimensionalArray(operation.Values, firstDimension: true),
                                vertical: true),
                            skipFinalNewline: true);
                }
                else
                {
                    builder
                        .Append("values: ")
                        .AppendLines(Code.Literal(operation.Values), skipFinalNewline: true);
                }

                builder.Append(")");
            }
        }

        /// <summary>
        ///     Generates code for a <see cref="DeleteDataOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="builder"> The builder code is added to. </param>
        protected virtual void Generate(
            [NotNull] DeleteDataOperation operation,
            [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".DeleteData(");

            using (builder.Indent())
            {
                if (operation.Schema != null)
                {
                    builder
                        .Append("schema: ")
                        .Append(Code.Literal(operation.Schema))
                        .AppendLine(",");
                }

                builder
                    .Append("table: ")
                    .Append(Code.Literal(operation.Table))
                    .AppendLine(",");

                if (operation.KeyColumns.Length == 1)
                {
                    builder
                        .Append("keyColumn: ")
                        .Append(Code.Literal(operation.KeyColumns[0]));
                }
                else
                {
                    builder
                        .Append("keyColumns: ")
                        .Append(Code.Literal(operation.KeyColumns));
                }

                builder.AppendLine(",");

                if (operation.KeyValues.GetLength(0) == 1
                    && operation.KeyValues.GetLength(1) == 1)
                {
                    builder
                        .Append("keyValue: ")
                        .Append(Code.UnknownLiteral(operation.KeyValues[0, 0]));
                }
                else if (operation.KeyValues.GetLength(0) == 1)
                {
                    builder
                        .Append("keyValues: ")
                        .Append(Code.Literal(ToOnedimensionalArray(operation.KeyValues)));
                }
                else if (operation.KeyValues.GetLength(1) == 1)
                {
                    builder
                        .Append("keyValues: ")
                        .AppendLines(
                            Code.Literal(
                                ToOnedimensionalArray(operation.KeyValues, firstDimension: true),
                                vertical: true),
                            skipFinalNewline: true);
                }
                else
                {
                    builder
                        .Append("keyValues: ")
                        .AppendLines(Code.Literal(operation.KeyValues), skipFinalNewline: true);
                }

                builder.Append(")");
            }
        }

        /// <summary>
        ///     Generates code for an <see cref="UpdateDataOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="builder"> The builder code is added to. </param>
        protected virtual void Generate(
            [NotNull] UpdateDataOperation operation,
            [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine(".UpdateData(");

            using (builder.Indent())
            {
                if (operation.Schema != null)
                {
                    builder
                        .Append("schema: ")
                        .Append(Code.Literal(operation.Schema))
                        .AppendLine(",");
                }

                builder
                    .Append("table: ")
                    .Append(Code.Literal(operation.Table))
                    .AppendLine(",");

                if (operation.KeyColumns.Length == 1)
                {
                    builder
                        .Append("keyColumn: ")
                        .Append(Code.Literal(operation.KeyColumns[0]));
                }
                else
                {
                    builder
                        .Append("keyColumns: ")
                        .Append(Code.Literal(operation.KeyColumns));
                }

                builder.AppendLine(",");

                if (operation.KeyValues.GetLength(0) == 1
                    && operation.KeyValues.GetLength(1) == 1)
                {
                    builder
                        .Append("keyValue: ")
                        .Append(Code.UnknownLiteral(operation.KeyValues[0, 0]));
                }
                else if (operation.KeyValues.GetLength(0) == 1)
                {
                    builder
                        .Append("keyValues: ")
                        .Append(Code.Literal(ToOnedimensionalArray(operation.KeyValues)));
                }
                else if (operation.KeyValues.GetLength(1) == 1)
                {
                    builder
                        .Append("keyValues: ")
                        .AppendLines(
                            Code.Literal(
                                ToOnedimensionalArray(operation.KeyValues, firstDimension: true),
                                vertical: true),
                            skipFinalNewline: true);
                }
                else
                {
                    builder
                        .Append("keyValues: ")
                        .AppendLines(Code.Literal(operation.KeyValues), skipFinalNewline: true);
                }

                builder.AppendLine(",");

                if (operation.Columns.Length == 1)
                {
                    builder
                        .Append("column: ")
                        .Append(Code.Literal(operation.Columns[0]));
                }
                else
                {
                    builder
                        .Append("columns: ")
                        .Append(Code.Literal(operation.Columns));
                }

                builder.AppendLine(",");

                if (operation.Values.GetLength(0) == 1
                    && operation.Values.GetLength(1) == 1)
                {
                    builder
                        .Append("value: ")
                        .Append(Code.UnknownLiteral(operation.Values[0, 0]));
                }
                else if (operation.Values.GetLength(0) == 1)
                {
                    builder
                        .Append("values: ")
                        .Append(Code.Literal(ToOnedimensionalArray(operation.Values)));
                }
                else if (operation.Values.GetLength(1) == 1)
                {
                    builder
                        .Append("values: ")
                        .AppendLines(
                            Code.Literal(
                                ToOnedimensionalArray(operation.Values, firstDimension: true),
                                vertical: true),
                            skipFinalNewline: true);
                }
                else
                {
                    builder
                        .Append("values: ")
                        .AppendLines(Code.Literal(operation.Values), skipFinalNewline: true);
                }

                builder.Append(")");
            }
        }

        /// <summary>
        ///     Generates code for <see cref="Annotation" /> objects.
        /// </summary>
        /// <param name="annotations"> The annotations. </param>
        /// <param name="builder"> The builder code is added to. </param>
        protected virtual void Annotations(
            [NotNull] IEnumerable<Annotation> annotations,
            [NotNull] IndentedStringBuilder builder)
        {
            Check.NotNull(annotations, nameof(annotations));
            Check.NotNull(builder, nameof(builder));

            foreach (var annotation in annotations)
            {
                if (annotation.Value == null)
                {
                    continue;
                }

                // TODO: Give providers an opportunity to render these as provider-specific extension methods
                builder
                    .AppendLine()
                    .Append(".Annotation(")
                    .Append(Code.Literal(annotation.Name))
                    .Append(", ")
                    .Append(Code.UnknownLiteral(annotation.Value))
                    .Append(")");
            }
        }

        /// <summary>
        ///     Generates code for removed <see cref="Annotation" /> objects.
        /// </summary>
        /// <param name="annotations"> The annotations. </param>
        /// <param name="builder"> The builder code is added to. </param>
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
                    .Append(Code.Literal(annotation.Name))
                    .Append(", ")
                    .Append(Code.UnknownLiteral(annotation.Value))
                    .Append(")");
            }
        }

        private static object[] ToOnedimensionalArray(object[,] values, bool firstDimension = false)
        {
            Debug.Assert(
                values.GetLength(firstDimension ? 1 : 0) == 1,
                $"Length of dimension {(firstDimension ? 1 : 0)} is not 1.");

            var result = new object[values.Length];
            for (var i = 0; i < values.Length; i++)
            {
                result[i] = firstDimension
                    ? values[i, 0]
                    : values[0, i];
            }

            return result;
        }
    }
}
