// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Commands.Utilities;
using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Commands.Migrations
{
    public class CSharpMigrationCodeGenerator : MigrationCodeGenerator
    {
        public CSharpMigrationCodeGenerator([NotNull] CSharpModelCodeGenerator modelCodeGenerator)
            : base(Check.NotNull(modelCodeGenerator, "modelCodeGenerator"))
        {
        }

        public static string Generate<T>([NotNull] T migrationOperation)
            where T : MigrationOperation
        {
            Check.NotNull(migrationOperation, "migrationOperation");

            var generator = new CSharpMigrationCodeGenerator(new CSharpModelCodeGenerator());
            var stringBuilder = new IndentedStringBuilder();

            migrationOperation.GenerateCode(generator, stringBuilder);

            return stringBuilder.ToString();
        }

        public override void GenerateMigrationClass(
            string @namespace,
            string className,
            MigrationInfo migration,
            IndentedStringBuilder stringBuilder)
        {
            Check.NotEmpty(@namespace, "namespace");
            Check.NotEmpty(className, "className");
            Check.NotNull(migration, "migration");
            Check.NotNull(stringBuilder, "stringBuilder");

            var operations = migration.UpgradeOperations.Concat(migration.DowngradeOperations);

            foreach (var ns in GetNamespaces(operations).OrderBy(n => n).Distinct())
            {
                stringBuilder
                    .Append("using ")
                    .Append(ns)
                    .AppendLine(";");
            }

            stringBuilder
                .AppendLine()
                .Append("namespace ")
                .AppendLine(@namespace)
                .AppendLine("{");

            using (stringBuilder.Indent())
            {
                stringBuilder
                    .Append("public partial class ")
                    .Append(className)
                    .AppendLine(" : Migration")
                    .AppendLine("{");

                using (stringBuilder.Indent())
                {
                    GenerateMigrationMethod("Up", migration.UpgradeOperations, stringBuilder);

                    stringBuilder.AppendLine().AppendLine();

                    GenerateMigrationMethod("Down", migration.DowngradeOperations, stringBuilder);
                }

                stringBuilder
                    .AppendLine()
                    .Append("}");
            }

            stringBuilder
                .AppendLine()
                .Append("}");
        }

        public override void GenerateMigrationMetadataClass(
            string @namespace,
            string className,
            MigrationInfo migration,
            Type contextType,
            IndentedStringBuilder stringBuilder)
        {
            Check.NotEmpty(@namespace, "namespace");
            Check.NotEmpty(className, "className");
            Check.NotNull(migration, "migration");
            Check.NotNull(contextType, "contextType");
            Check.NotNull(stringBuilder, "stringBuilder");

            foreach (var ns in GetMetadataNamespaces(migration, contextType).OrderBy(n => n).Distinct())
            {
                stringBuilder
                    .Append("using ")
                    .Append(ns)
                    .AppendLine(";");
            }

            stringBuilder
                .AppendLine()
                .Append("namespace ")
                .AppendLine(@namespace)
                .AppendLine("{");

            using (stringBuilder.Indent())
            {
                stringBuilder
                    .Append("[ContextType(typeof(")
                    .Append(contextType.GetNestedName())
                    .AppendLine("))]")
                    .Append("public partial class ")
                    .Append(className)
                    .AppendLine(" : IMigrationMetadata")
                    .AppendLine("{");

                using (stringBuilder.Indent())
                {
                    GenerateMigrationProperty(
                        "string IMigrationMetadata.MigrationId",
                        () => stringBuilder
                            .Append("return ")
                            .Append(GenerateLiteral(migration.MigrationId))
                            .Append(";"),
                        stringBuilder);

                    stringBuilder.AppendLine().AppendLine();

                    GenerateMigrationProperty(
                        "string IMigrationMetadata.ProductVersion",
                        () => stringBuilder
                            .Append("return ")
                            .Append(GenerateLiteral(migration.ProductVersion))
                            .Append(";"),
                        stringBuilder);

                    stringBuilder.AppendLine().AppendLine();

                    GenerateMigrationProperty(
                        "IModel IMigrationMetadata.TargetModel",
                        () => ModelCodeGenerator.Generate(migration.TargetModel, stringBuilder),
                        stringBuilder);
                }

                stringBuilder
                    .AppendLine()
                    .Append("}");
            }

            stringBuilder
                .AppendLine()
                .Append("}");
        }

        protected virtual void GenerateMigrationMethod(
            [NotNull] string methodName,
            [NotNull] IReadOnlyList<MigrationOperation> migrationOperations,
            [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotEmpty(methodName, "methodName");
            Check.NotNull(migrationOperations, "migrationOperations");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append("public override void ")
                .Append(methodName)
                .AppendLine("(MigrationBuilder migrationBuilder)")
                .AppendLine("{");

            using (stringBuilder.Indent())
            {
                for (var i = 0; i < migrationOperations.Count; i++)
                {
                    if (i > 0)
                    {
                        stringBuilder.AppendLine();
                    }

                    stringBuilder.Append("migrationBuilder.");
                    migrationOperations[i].GenerateCode(this, stringBuilder);
                    stringBuilder.AppendLine(";");
                }
            }

            stringBuilder.Append("}");
        }

        protected virtual void GenerateMigrationProperty(
            [NotNull] string signature,
            [NotNull] Action generateCode,
            [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotEmpty(signature, "signature");
            Check.NotNull(generateCode, "generateCode");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .AppendLine(signature)
                .AppendLine("{");

            using (stringBuilder.Indent())
            {
                stringBuilder
                    .AppendLine("get")
                    .AppendLine("{");

                using (stringBuilder.Indent())
                {
                    generateCode();
                }

                stringBuilder
                    .AppendLine()
                    .AppendLine("}");
            }

            stringBuilder.Append("}");
        }

        public override void Generate(CreateDatabaseOperation createDatabaseOperation, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(createDatabaseOperation, "createDatabaseOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append("CreateDatabase(")
                .Append(GenerateLiteral(createDatabaseOperation.DatabaseName))
                .Append(")");
        }

        public override void Generate(DropDatabaseOperation dropDatabaseOperation, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(dropDatabaseOperation, "dropDatabaseOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append("DropDatabase(")
                .Append(GenerateLiteral(dropDatabaseOperation.DatabaseName))
                .Append(")");
        }

        public override void Generate(CreateSequenceOperation createSequenceOperation, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(createSequenceOperation, "createSequenceOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append("CreateSequence(")
                .Append(GenerateLiteral(createSequenceOperation.SequenceName));

            int paramCount;

            if (createSequenceOperation.Type != Sequence.DefaultType)
            {
                paramCount = 5;
            }
            else if (createSequenceOperation.MaxValue.HasValue)
            {
                paramCount = 4;
            }
            else if (createSequenceOperation.MinValue.HasValue)
            {
                paramCount = 3;
            }
            else if (createSequenceOperation.IncrementBy != Sequence.DefaultIncrement)
            {
                paramCount = 2;
            }
            else if (createSequenceOperation.StartValue != Sequence.DefaultStartValue)
            {
                paramCount = 1;
            }
            else
            {
                paramCount = 0;
            }

            if (paramCount > 0)
            {
                stringBuilder
                    .Append(", ")
                    .Append(GenerateLiteral(createSequenceOperation.StartValue));

                if (paramCount > 1)
                {
                    stringBuilder
                        .Append(", ")
                        .Append(GenerateLiteral(createSequenceOperation.IncrementBy));

                    if (paramCount > 2)
                    {
                        stringBuilder
                            .Append(", ")
                            .Append(createSequenceOperation.MinValue.HasValue ? GenerateLiteral(createSequenceOperation.MinValue) : "null");

                        if (paramCount > 3)
                        {
                            stringBuilder
                                .Append(", ")
                                .Append(createSequenceOperation.MaxValue.HasValue ? GenerateLiteral(createSequenceOperation.MaxValue) : "null");

                            if (paramCount > 4)
                            {
                                stringBuilder
                                    .Append(", typeof(")
                                    .Append(createSequenceOperation.Type.GetTypeName())
                                    .Append(")");
                            }

                        }
                    }
                }
            }

            stringBuilder.Append(")");
        }

        public override void Generate(DropSequenceOperation dropSequenceOperation, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(dropSequenceOperation, "dropSequenceOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append("DropSequence(")
                .Append(GenerateLiteral(dropSequenceOperation.SequenceName))
                .Append(")");
        }

        public override void Generate(RenameSequenceOperation renameSequenceOperation, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(renameSequenceOperation, "renameSequenceOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append("RenameSequence(")
                .Append(GenerateLiteral(renameSequenceOperation.SequenceName))
                .Append(", ")
                .Append(GenerateLiteral(renameSequenceOperation.NewSequenceName))
                .Append(")");
        }

        public override void Generate(MoveSequenceOperation moveSequenceOperation, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(moveSequenceOperation, "moveSequenceOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append("MoveSequence(")
                .Append(GenerateLiteral(moveSequenceOperation.SequenceName))
                .Append(", ")
                .Append(GenerateLiteral(moveSequenceOperation.NewSchema))
                .Append(")");
        }

        public override void Generate(AlterSequenceOperation alterSequenceOperation, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(alterSequenceOperation, "alterSequenceOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append("AlterSequence(")
                .Append(GenerateLiteral(alterSequenceOperation.SequenceName))
                .Append(", ")
                .Append(GenerateLiteral(alterSequenceOperation.NewIncrementBy))
                .Append(")");
        }

        public override void Generate(CreateTableOperation createTableOperation, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(createTableOperation, "createTableOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append("CreateTable(")
                .Append(GenerateLiteral(createTableOperation.TableName))
                .AppendLine(",");

            using (stringBuilder.Indent())
            {
                GenerateColumns(createTableOperation.Columns, stringBuilder);

                stringBuilder.Append(")");

                var addPrimaryKeyOperation = createTableOperation.PrimaryKey;
                if (addPrimaryKeyOperation != null)
                {
                    stringBuilder
                        .AppendLine()
                        .Append(".PrimaryKey(")
                        .Append(GenerateLiteral(addPrimaryKeyOperation.PrimaryKeyName))
                        .Append(", ");

                    GenerateColumnReferences(addPrimaryKeyOperation.ColumnNames, stringBuilder);

                    stringBuilder.Append(")");
                }

                foreach (var uniqueConstraintOperation in createTableOperation.UniqueConstraints)
                {
                    stringBuilder
                        .AppendLine()
                        .Append(".UniqueConstraint(")
                        .Append(GenerateLiteral(uniqueConstraintOperation.UniqueConstraintName))
                        .Append(", ");

                    GenerateColumnReferences(uniqueConstraintOperation.ColumnNames, stringBuilder);

                    stringBuilder.Append(")");
                }
            }
        }

        public override void Generate(DropTableOperation dropTableOperation, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(dropTableOperation, "dropTableOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append("DropTable(")
                .Append(GenerateLiteral(dropTableOperation.TableName))
                .Append(")");
        }

        public override void Generate(RenameTableOperation renameTableOperation, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(renameTableOperation, "renameTableOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append("RenameTable(")
                .Append(GenerateLiteral(renameTableOperation.TableName))
                .Append(", ")
                .Append(GenerateLiteral(renameTableOperation.NewTableName))
                .Append(")");
        }

        public override void Generate(MoveTableOperation moveTableOperation, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(moveTableOperation, "moveTableOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append("MoveTable(")
                .Append(GenerateLiteral(moveTableOperation.TableName))
                .Append(", ")
                .Append(GenerateLiteral(moveTableOperation.NewSchema))
                .Append(")");
        }

        public override void Generate(AddColumnOperation addColumnOperation, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(addColumnOperation, "addColumnOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append("AddColumn(")
                .Append(GenerateLiteral(addColumnOperation.TableName))
                .Append(", ")
                .Append(GenerateLiteral(addColumnOperation.Column.Name))
                .Append(", c => ");

            GenerateColumn(addColumnOperation.Column, stringBuilder, emitName: false);

            stringBuilder.Append(")");
        }

        public override void Generate(DropColumnOperation dropColumnOperation, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(dropColumnOperation, "dropColumnOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append("DropColumn(")
                .Append(GenerateLiteral(dropColumnOperation.TableName))
                .Append(", ")
                .Append(GenerateLiteral(dropColumnOperation.ColumnName))
                .Append(")");
        }

        public override void Generate(RenameColumnOperation renameColumnOperation, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(renameColumnOperation, "renameColumnOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append("RenameColumn(")
                .Append(GenerateLiteral(renameColumnOperation.TableName))
                .Append(", ")
                .Append(GenerateLiteral(renameColumnOperation.ColumnName))
                .Append(", ")
                .Append(GenerateLiteral(renameColumnOperation.NewColumnName))
                .Append(")");
        }

        public override void Generate(AlterColumnOperation alterColumnOperation, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(alterColumnOperation, "alterColumnOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append("AlterColumn(")
                .Append(GenerateLiteral(alterColumnOperation.TableName))
                .Append(", ")
                .Append(GenerateLiteral(alterColumnOperation.NewColumn.Name))
                .Append(", c => ");

            GenerateColumn(alterColumnOperation.NewColumn, stringBuilder, emitName: false);

            stringBuilder.Append(")");
        }

        public override void Generate(AddDefaultConstraintOperation addDefaultConstraintOperation, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(addDefaultConstraintOperation, "addDefaultConstraintOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            if (addDefaultConstraintOperation.DefaultValue != null)
            {
                stringBuilder
                    .Append("AddDefaultValue(")
                    .Append(GenerateLiteral(addDefaultConstraintOperation.TableName))
                    .Append(", ")
                    .Append(GenerateLiteral(addDefaultConstraintOperation.ColumnName))
                    .Append(", ")
                    .Append(GenerateLiteral((dynamic)addDefaultConstraintOperation.DefaultValue));
            }
            else
            {
                stringBuilder
                    .Append("AddDefaultExpression(")
                    .Append(GenerateLiteral(addDefaultConstraintOperation.TableName))
                    .Append(", ")
                    .Append(GenerateLiteral(addDefaultConstraintOperation.ColumnName))
                    .Append(", ")
                    .Append(GenerateLiteral(addDefaultConstraintOperation.DefaultSql));
            }

            stringBuilder.Append(")");
        }

        public override void Generate(DropDefaultConstraintOperation dropDefaultConstraintOperation, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(dropDefaultConstraintOperation, "dropDefaultConstraintOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append("DropDefaultConstraint(")
                .Append(GenerateLiteral(dropDefaultConstraintOperation.TableName))
                .Append(", ")
                .Append(GenerateLiteral(dropDefaultConstraintOperation.ColumnName))
                .Append(")");
        }

        public override void Generate(AddPrimaryKeyOperation addPrimaryKeyOperation, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(addPrimaryKeyOperation, "addPrimaryKeyOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append("AddPrimaryKey(")
                .Append(GenerateLiteral(addPrimaryKeyOperation.TableName))
                .Append(", ")
                .Append(GenerateLiteral(addPrimaryKeyOperation.PrimaryKeyName))
                .Append(", new[] { ")
                .Append(addPrimaryKeyOperation.ColumnNames.Select(GenerateLiteral).Join())
                .Append(" }, isClustered: ")
                .Append(GenerateLiteral(addPrimaryKeyOperation.IsClustered))
                .Append(")");
        }

        public override void Generate(DropPrimaryKeyOperation dropPrimaryKeyOperation, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(dropPrimaryKeyOperation, "dropPrimaryKeyOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append("DropPrimaryKey(")
                .Append(GenerateLiteral(dropPrimaryKeyOperation.TableName))
                .Append(", ")
                .Append(GenerateLiteral(dropPrimaryKeyOperation.PrimaryKeyName))
                .Append(")");
        }

        public override void Generate(AddUniqueConstraintOperation addUniqueConstraintOperation, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(addUniqueConstraintOperation, "addUniqueConstraintOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append("AddUniqueConstraint(")
                .Append(GenerateLiteral(addUniqueConstraintOperation.TableName))
                .Append(", ")
                .Append(GenerateLiteral(addUniqueConstraintOperation.UniqueConstraintName))
                .Append(", new[] { ")
                .Append(addUniqueConstraintOperation.ColumnNames.Select(GenerateLiteral).Join())
                .Append(" })");
        }

        public override void Generate(DropUniqueConstraintOperation dropUniqueConstraintOperation, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(dropUniqueConstraintOperation, "dropUniqueConstraintOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append("DropUniqueConstraint(")
                .Append(GenerateLiteral(dropUniqueConstraintOperation.TableName))
                .Append(", ")
                .Append(GenerateLiteral(dropUniqueConstraintOperation.UniqueConstraintName))
                .Append(")");
        }

        public override void Generate(AddForeignKeyOperation addForeignKeyOperation, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(addForeignKeyOperation, "addForeignKeyOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder.AppendLine("AddForeignKey(");

            using (stringBuilder.Indent())
            {
                stringBuilder
                    .Append(GenerateLiteral(addForeignKeyOperation.TableName))
                    .AppendLine(",")
                    .Append(GenerateLiteral(addForeignKeyOperation.ForeignKeyName))
                    .AppendLine(",")
                    .Append("new[] { ")
                    .Append(addForeignKeyOperation.ColumnNames.Select(GenerateLiteral).Join())
                    .AppendLine(" },")
                    .Append(GenerateLiteral(addForeignKeyOperation.ReferencedTableName))
                    .AppendLine(",")
                    .Append("new[] { ")
                    .Append(addForeignKeyOperation.ReferencedColumnNames.Select(GenerateLiteral).Join())
                    .AppendLine(" },")
                    .Append("cascadeDelete: ")
                    .Append(GenerateLiteral(addForeignKeyOperation.CascadeDelete))
                    .Append(")");
            }
        }

        public override void Generate(DropForeignKeyOperation dropForeignKeyOperation, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(dropForeignKeyOperation, "dropForeignKeyOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append("DropForeignKey(")
                .Append(GenerateLiteral(dropForeignKeyOperation.TableName))
                .Append(", ")
                .Append(GenerateLiteral(dropForeignKeyOperation.ForeignKeyName))
                .Append(")");
        }

        public override void Generate(CreateIndexOperation createIndexOperation, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(createIndexOperation, "createIndexOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append("CreateIndex(")
                .Append(GenerateLiteral(createIndexOperation.TableName))
                .Append(", ")
                .Append(GenerateLiteral(createIndexOperation.IndexName))
                .Append(", new[] { ")
                .Append(createIndexOperation.ColumnNames.Select(GenerateLiteral).Join())
                .Append(" }, isUnique: ")
                .Append(GenerateLiteral(createIndexOperation.IsUnique))
                .Append(", isClustered: ")
                .Append(GenerateLiteral(createIndexOperation.IsClustered))
                .Append(")");
        }

        public override void Generate(DropIndexOperation dropIndexOperation, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(dropIndexOperation, "dropIndexOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append("DropIndex(")
                .Append(GenerateLiteral(dropIndexOperation.TableName))
                .Append(", ")
                .Append(GenerateLiteral(dropIndexOperation.IndexName))
                .Append(")");
        }

        public override void Generate(RenameIndexOperation renameIndexOperation, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(renameIndexOperation, "renameIndexOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append("RenameIndex(")
                .Append(GenerateLiteral(renameIndexOperation.TableName))
                .Append(", ")
                .Append(GenerateLiteral(renameIndexOperation.IndexName))
                .Append(", ")
                .Append(GenerateLiteral(renameIndexOperation.NewIndexName))
                .Append(")");
        }

        public override void Generate(CopyDataOperation copyDataOperation, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(copyDataOperation, "copyDataOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append("CopyData(")
                .Append(GenerateLiteral(copyDataOperation.SourceTableName))
                .Append(", new[] { ")
                .Append(copyDataOperation.SourceColumnNames.Select(GenerateLiteral).Join())
                .Append(" }, ")
                .Append(GenerateLiteral(copyDataOperation.TargetTableName))
                .Append(", new[] { ")
                .Append(copyDataOperation.TargetColumnNames.Select(GenerateLiteral).Join())
                .Append(" })");
        }

        public override void Generate(SqlOperation sqlOperation, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(sqlOperation, "sqlOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append("Sql(")
                .Append(GenerateVerbatimStringLiteral(sqlOperation.Sql))
                .Append(")");
        }

        public virtual string EscapeString([NotNull] string str)
        {
            Check.NotEmpty(str, "str");

            return str.Replace("\"", "\\\"");
        }

        public virtual string EscapeVerbatimString([NotNull] string str)
        {
            Check.NotEmpty(str, "str");

            return str.Replace("\"", "\"\"");
        }

        public virtual string GenerateLiteral([NotNull] byte[] value)
        {
            Check.NotNull(value, "value");

            return "new byte[] {" + string.Join(", ", value) + "}";
        }

        public virtual string GenerateLiteral(bool value)
        {
            return value ? "true" : "false";
        }

        public virtual string GenerateLiteral(long value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        public virtual string GenerateLiteral(decimal value)
        {
            return value.ToString(CultureInfo.InvariantCulture) + "m";
        }

        public virtual string GenerateLiteral(float value)
        {
            return value.ToString(CultureInfo.InvariantCulture) + "f";
        }

        public virtual string GenerateLiteral(TimeSpan value)
        {
            return "new TimeSpan(" + value.Ticks + ")";
        }

        public virtual string GenerateLiteral(DateTime value)
        {
            return "new DateTime(" + value.Ticks + ", DateTimeKind."
                   + Enum.GetName(typeof(DateTimeKind), value.Kind) + ")";
        }

        public virtual string GenerateLiteral(DateTimeOffset value)
        {
            return "new DateTimeOffset(" + value.Ticks + ", "
                   + GenerateLiteral(value.Offset) + ")";
        }

        public virtual string GenerateLiteral(Guid value)
        {
            return "new Guid(" + GenerateLiteral(value.ToString()) + ")";
        }

        public virtual string GenerateLiteral([NotNull] string value)
        {
            Check.NotNull(value, "value");

            return "\"" + EscapeString(value) + "\"";
        }

        public virtual string GenerateVerbatimStringLiteral([NotNull] string value)
        {
            Check.NotNull(value, "value");

            return "@\"" + EscapeVerbatimString(value) + "\"";
        }

        public virtual string GenerateLiteral([NotNull] object value)
        {
            Check.NotNull(value, "value");

            if (value.GetType().GetTypeInfo().IsEnum)
            {
                return Enum.Format(value.GetType(), value, "D");
            }

            return string.Format(CultureInfo.InvariantCulture, "{0}", value);
        }

        protected virtual void GenerateColumns([NotNull] IReadOnlyList<Column> columns, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(columns, "columns");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder.AppendLine("c => new");

            using (stringBuilder.Indent())
            {
                stringBuilder.AppendLine("{");

                using (stringBuilder.Indent())
                {
                    for (var i = 0; i < columns.Count; i++)
                    {
                        var column = columns[i];
                        var columnIdentifier = GenerateColumnIdentifier(column.Name);
                        var emitName = !string.Equals(columnIdentifier, column.Name, StringComparison.Ordinal);

                        if (i > 0)
                        {
                            stringBuilder.AppendLine(",");
                        }

                        stringBuilder
                            .Append(columnIdentifier)
                            .Append(" = ");

                        GenerateColumn(column, stringBuilder, emitName);
                    }
                }

                stringBuilder
                    .AppendLine()
                    .Append("}");
            }
        }

        protected virtual void GenerateColumn([NotNull] Column column, [NotNull] IndentedStringBuilder stringBuilder, bool emitName)
        {
            Check.NotNull(column, "column");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append("c.")
                .Append(TranslateColumnType(column.ClrType))
                .Append("(");

            var args = new List<string>();

            if (emitName)
            {
                args.Add("name: " + GenerateLiteral(column.Name));
            }

            if (!string.IsNullOrWhiteSpace(column.DataType))
            {
                args.Add("dataType: " + GenerateLiteral(column.DataType));
            }

            if (!column.IsNullable)
            {
                args.Add("nullable: " + GenerateLiteral(false));
            }

            if (column.MaxLength.HasValue)
            {
                args.Add("maxLength: " + GenerateLiteral(column.MaxLength.Value));
            }

            if (column.Precision.HasValue)
            {
                args.Add("precision: " + GenerateLiteral(column.Precision.Value));
            }

            if (column.Scale.HasValue)
            {
                args.Add("scale: " + GenerateLiteral(column.Scale.Value));
            }

            if (column.IsFixedLength.HasValue)
            {
                args.Add("fixedLength: " + GenerateLiteral(column.IsFixedLength.Value));
            }

            if (column.IsUnicode.HasValue)
            {
                args.Add("unicode: " + GenerateLiteral(column.IsUnicode.Value));
            }

            if (column.IsIdentity)
            {
                args.Add("identity: " + GenerateLiteral(true));
            }

            if (column.DefaultValue != null)
            {
                args.Add("defaultValue: " + GenerateLiteral((dynamic)column.DefaultValue));
            }

            if (!string.IsNullOrWhiteSpace(column.DefaultSql))
            {
                args.Add("defaultSql: " + GenerateLiteral(column.DefaultSql));
            }

            if (column.IsComputed)
            {
                args.Add("computed: " + GenerateLiteral(true));
            }

            if (column.IsTimestamp)
            {
                args.Add("timestamp: " + GenerateLiteral(true));
            }

            stringBuilder
                .Append(string.Join(", ", args))
                .Append(")");
        }

        protected virtual void GenerateColumnReferences(
            [NotNull] IReadOnlyList<string> columnNames, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(columnNames, "columnNames");
            Check.NotNull(stringBuilder, "stringBuilder");

            if (columnNames.Count == 1)
            {
                stringBuilder
                    .Append("t => t.")
                    .Append(GenerateColumnIdentifier(columnNames[0]));

                return;
            }

            stringBuilder
                .Append("t => new { ")
                .Append(columnNames.Select(n => "t." + GenerateColumnIdentifier(n)).Join())
                .Append(" }");
        }

        protected virtual string GenerateColumnIdentifier([NotNull] string columnName)
        {
            Check.NotEmpty(columnName, "columnName");

            var invalidCharsRegex
                = new Regex(@"[^\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Nd}\p{Nl}\p{Mn}\p{Mc}\p{Cf}\p{Pc}\p{Lm}]");

            var identifier = invalidCharsRegex.Replace(columnName, string.Empty);
            var firstChar = identifier[0];

            if ((!char.IsLetter(firstChar) && firstChar != '_'))
            {
                identifier = "_" + identifier;
            }

            // TODO: Validate identifiers (not keywords, unique).

            return identifier;
        }

        protected virtual string TranslateColumnType([NotNull] Type clrType)
        {
            Check.NotNull(clrType, "clrType");

            clrType = clrType.UnwrapNullableType().UnwrapEnumType();

            if (clrType == typeof(short))
            {
                return "Short";
            }

            if (clrType == typeof(int))
            {
                return "Int";
            }

            if (clrType == typeof(long))
            {
                return "Long";
            }

            if (clrType == typeof(byte[]))
            {
                return "Binary";
            }

            if (clrType == typeof(TimeSpan))
            {
                return "Time";
            }

            return clrType.Name;
        }
    }
}
