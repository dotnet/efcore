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

        public override void Generate(RenameTableOperation renameTableOperation, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(renameTableOperation, "renameTableOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append("EXECUTE sp_rename @objname = N")
                .Append(DelimitLiteral(renameTableOperation.TableName))
                .Append(", @newname = N")
                .Append(DelimitLiteral(renameTableOperation.NewTableName))
                .Append(", @objtype = N'OBJECT'");
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

            stringBuilder
                .Append("EXECUTE sp_rename @objname = N'")
                .Append(EscapeLiteral(renameColumnOperation.TableName))
                .Append(".")
                .Append(EscapeLiteral(renameColumnOperation.ColumnName))
                .Append("', @newname = N")
                .Append(DelimitLiteral(renameColumnOperation.NewColumnName))
                .Append(", @objtype = N'COLUMN'");
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
            if (column.ValueGenerationStrategy == ValueGeneration.OnAdd
                && column.ClrType.IsInteger() && column.ClrType != typeof(byte))
            {
                // TODO: Handle other SQL Server strategies
                stringBuilder.Append(" IDENTITY");
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
    }
}
