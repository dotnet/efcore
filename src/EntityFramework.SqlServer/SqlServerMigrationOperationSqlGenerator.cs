// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.SqlServer.Utilities;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerMigrationOperationSqlGenerator : MigrationOperationSqlGenerator
    {
        private int _variableCount;

        public SqlServerMigrationOperationSqlGenerator([NotNull] SqlServerTypeMapper typeMapper)
            : base(typeMapper)
        {
        }

        public override IEnumerable<SqlStatement> Generate(IEnumerable<MigrationOperation> migrationOperations)
        {
            Check.NotNull(migrationOperations, "migrationOperations");

            var preProcessor = new SqlServerMigrationOperationPreProcessor();
            var preProcessorContext = new SqlServerMigrationOperationPreProcessor.Context(this);

            foreach (var operation in migrationOperations)
            {
                operation.Accept(preProcessor, preProcessorContext);
            }

            return preProcessorContext.Statements;
        }

        public override void Generate(RenameSequenceOperation renameSequenceOperation, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(renameSequenceOperation, "renameSequenceOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            GenerateRename(
                renameSequenceOperation.SequenceName, 
                renameSequenceOperation.NewSequenceName, 
                "OBJECT", 
                stringBuilder);
        }

        public override void Generate(MoveSequenceOperation moveSequenceOperation, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(moveSequenceOperation, "moveSequenceOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            GenerateMove(
                moveSequenceOperation.SequenceName,
                moveSequenceOperation.NewSchema,
                stringBuilder);
        }

        public override void Generate(RenameTableOperation renameTableOperation, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(renameTableOperation, "renameTableOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            GenerateRename(
                renameTableOperation.TableName, 
                renameTableOperation.NewTableName, 
                "OBJECT", 
                stringBuilder);
        }

        public override void Generate(MoveTableOperation moveTableOperation, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(moveTableOperation, "moveTableOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            GenerateMove(
                moveTableOperation.TableName,
                moveTableOperation.NewSchema,
                stringBuilder);
        }

        public override void Generate(AddDefaultConstraintOperation addDefaultConstraintOperation, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(addDefaultConstraintOperation, "addDefaultConstraintOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            var tableName = addDefaultConstraintOperation.TableName;
            var columnName = addDefaultConstraintOperation.ColumnName;

            stringBuilder
                .Append("ALTER TABLE ")
                .Append(DelimitIdentifier(tableName))
                .Append(" ADD CONSTRAINT ")
                .Append(DelimitIdentifier("DF_" + tableName + "_" + columnName))
                .Append(" DEFAULT ");

            stringBuilder.Append(addDefaultConstraintOperation.DefaultSql ?? GenerateLiteral(addDefaultConstraintOperation.DefaultValue));

            stringBuilder
                .Append(" FOR ")
                .Append(DelimitIdentifier(columnName));
        }

        public override void Generate(DropDefaultConstraintOperation dropDefaultConstraintOperation, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(dropDefaultConstraintOperation, "dropDefaultConstraintOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            var constraintNameVariable = "@var" + _variableCount++;

            stringBuilder
                .Append("DECLARE ")
                .Append(constraintNameVariable)
                .AppendLine(" nvarchar(128)");

            stringBuilder
                .Append("SELECT ")
                .Append(constraintNameVariable)
                .Append(" = name FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID(N")
                .Append(DelimitLiteral(dropDefaultConstraintOperation.TableName))
                .Append(") AND COL_NAME(parent_object_id, parent_column_id) = N")
                .AppendLine(DelimitLiteral(dropDefaultConstraintOperation.ColumnName));

            stringBuilder
                .Append("EXECUTE('ALTER TABLE ")
                .Append(DelimitIdentifier(dropDefaultConstraintOperation.TableName))
                .Append(" DROP CONSTRAINT \"' + ")
                .Append(constraintNameVariable)
                .Append(" + '\"')");
        }

        public override void Generate(RenameColumnOperation renameColumnOperation, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(renameColumnOperation, "renameColumnOperation");

            GenerateRename(
                string.Concat(
                    EscapeLiteral(renameColumnOperation.TableName), 
                    ".",
                    EscapeLiteral(renameColumnOperation.ColumnName)),
                renameColumnOperation.NewColumnName,
                "COLUMN",
                stringBuilder);
        }

        public override void Generate(RenameIndexOperation renameIndexOperation, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(renameIndexOperation, "renameIndexOperation");

            stringBuilder
                .Append("EXECUTE sp_rename @objname = N'")
                .Append(EscapeLiteral(renameIndexOperation.TableName))
                .Append(".")
                .Append(EscapeLiteral(renameIndexOperation.IndexName))
                .Append("', @newname = N")
                .Append(DelimitLiteral(renameIndexOperation.NewIndexName))
                .Append(", @objtype = N'INDEX'");
        }

        public override string DelimitIdentifier(string identifier)
        {
            Check.NotEmpty(identifier, "identifier");

            return "[" + EscapeIdentifier(identifier) + "]";
        }

        public override string EscapeIdentifier(string identifier)
        {
            Check.NotEmpty(identifier, "identifier");

            return identifier.Replace("]", "]]");
        }

        protected override void GenerateColumnTraits(Column column, IndentedStringBuilder stringBuilder)
        {
            // TODO: This is essentially duplicated logic from the selector; combine if possible
            if (column.ValueGenerationStrategy == ValueGeneration.OnAdd)
            {
                // TODO: This can't use the normal APIs because all the annotations have been
                // copied from the core metadata into the relational model.

                var strategy = column[SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ValueGeneration];

                if (strategy == SqlServerValueGenerationStrategy.Identity.ToString()
                    || (strategy == null
                        && column.ClrType.IsInteger()))
                {
                    stringBuilder.Append(" IDENTITY");
                }
            }
        }

        protected override void GeneratePrimaryKeyTraits(
            AddPrimaryKeyOperation primaryKeyOperation,
            IndentedStringBuilder stringBuilder)
        {
            if (!primaryKeyOperation.IsClustered)
            {
                stringBuilder.Append(" NONCLUSTERED");
            }
        }

        public override void Generate(DropIndexOperation dropIndexOperation, IndentedStringBuilder stringBuilder)
        {
            base.Generate(dropIndexOperation, stringBuilder);

            stringBuilder
                .Append(" ON ")
                .Append(DelimitIdentifier(dropIndexOperation.TableName));
        }

        protected virtual void GenerateRename([NotNull] string name, [NotNull] string newName, 
            [NotNull] string objectType, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotEmpty(name, "name");
            Check.NotEmpty(newName, "newName");
            Check.NotEmpty(objectType, "objectType");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append("EXECUTE sp_rename @objname = N")
                .Append(DelimitLiteral(name))
                .Append(", @newname = N")
                .Append(DelimitLiteral(newName))
                .Append(", @objtype = N")
                .Append(DelimitLiteral(objectType));
        }

        protected virtual void GenerateMove(SchemaQualifiedName name, [NotNull] string newSchema,
            [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotEmpty(newSchema, "newSchema");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append("ALTER SCHEMA ")
                .Append(DelimitIdentifier(newSchema))
                .Append(" TRANSFER ")
                .Append(DelimitIdentifier(name));
        }
    }
}
