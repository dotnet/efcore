// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.SQLite.Utilities;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SQLite
{
    public class SQLiteMigrationOperationSqlGenerator : MigrationOperationSqlGenerator
    {
        public SQLiteMigrationOperationSqlGenerator([NotNull] SQLiteTypeMapper typeMapper)
            : base(typeMapper)
        {
        }

        public override IEnumerable<SqlStatement> Generate(IEnumerable<MigrationOperation> migrationOperations)
        {
            Check.NotNull(migrationOperations, "migrationOperations");

            return base.Generate(FilterOperations(migrationOperations.ToArray()));
        }

        // Internal for testing
        protected internal virtual IEnumerable<MigrationOperation> FilterOperations(
            IEnumerable<MigrationOperation> operations)
        {
            var createTableOperations = operations.OfType<CreateTableOperation>().ToArray();

            foreach (var operation in operations)
            {
                // Remove add foreign key operations with corresponding create table operations
                // TODO: Consider making this more robust. (E.g. Handle interim drops and renames)
                var addForeignKeyOperation = operation as AddForeignKeyOperation;
                if (addForeignKeyOperation != null
                    && createTableOperations.Any(
                        o => o.Table.Name == addForeignKeyOperation.TableName
                             && o.Table.ForeignKeys.Any(k => k.Name == addForeignKeyOperation.ForeignKeyName)))
                {
                    continue;
                }

                yield return operation;
            }
        }

        public override void Generate(
            CreateDatabaseOperation createDatabaseOperation,
            IndentedStringBuilder stringBuilder)
        {
            throw new NotSupportedException();
        }

        public override void Generate(
            DropDatabaseOperation dropDatabaseOperation,
            IndentedStringBuilder stringBuilder)
        {
            throw new NotSupportedException();
        }

        public override void Generate(
            CreateSequenceOperation createSequenceOperation,
            IndentedStringBuilder stringBuilder)
        {
            throw new NotSupportedException();
        }

        public override void Generate(DropSequenceOperation dropSequenceOperation, IndentedStringBuilder stringBuilder)
        {
        }

        protected override void GenerateTableConstraints(
            CreateTableOperation createTableOperation,
            IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(createTableOperation, "createTableOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            foreach (var foreignKey in createTableOperation.Table.ForeignKeys)
            {
                stringBuilder.AppendLine(",");
                GenerateForeignKey(
                    new AddForeignKeyOperation(
                        foreignKey.Table.Name,
                        foreignKey.Name,
                        foreignKey.Columns.Select(c => c.Name).ToArray(),
                        foreignKey.ReferencedTable.Name,
                        foreignKey.ReferencedColumns.Select(c => c.Name).ToArray(),
                        foreignKey.CascadeDelete),
                    stringBuilder);
            }
        }

        public override void Generate(RenameTableOperation renameTableOperation, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(renameTableOperation, "renameTableOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            GenerateRenameTable(
                renameTableOperation.TableName,
                new SchemaQualifiedName(renameTableOperation.NewTableName, renameTableOperation.TableName.Schema),
                stringBuilder);
        }

        public override void Generate(MoveTableOperation moveTableOperation, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(moveTableOperation, "moveTableOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            GenerateRenameTable(
                moveTableOperation.TableName,
                new SchemaQualifiedName(moveTableOperation.TableName.Name, moveTableOperation.NewSchema),
                stringBuilder);
        }

        protected virtual void GenerateRenameTable(
            [NotNull] SchemaQualifiedName tableName,
            [NotNull] SchemaQualifiedName newTableName,
            [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(tableName, "tableName");
            Check.NotNull(newTableName, "newTableName");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append("ALTER TABLE ")
                .Append(DelimitIdentifier(tableName))
                .Append(" RENAME TO ")
                .Append(DelimitIdentifier(newTableName));
        }

        public override void Generate(DropColumnOperation dropColumnOperation, IndentedStringBuilder stringBuilder)
        {
            // TODO: Rebuild table
            throw new NotImplementedException();
        }

        public override void Generate(AlterColumnOperation alterColumnOperation, IndentedStringBuilder stringBuilder)
        {
            // TODO: Rebuild table
            throw new NotImplementedException();
        }

        public override void Generate(
            AddDefaultConstraintOperation addDefaultConstraintOperation,
            IndentedStringBuilder stringBuilder)
        {
            // TODO: Rebuild table
            throw new NotImplementedException();
        }

        public override void Generate(
            DropDefaultConstraintOperation dropDefaultConstraintOperation,
            IndentedStringBuilder stringBuilder)
        {
            // TODO: Rebuild table
            throw new NotImplementedException();
        }

        public override void Generate(RenameColumnOperation renameColumnOperation, IndentedStringBuilder stringBuilder)
        {
            // TODO: Rebuild table
            throw new NotImplementedException();
        }

        public override void Generate(
            AddPrimaryKeyOperation addPrimaryKeyOperation,
            IndentedStringBuilder stringBuilder)
        {
            // TODO: Rebuild table
            throw new NotImplementedException();
        }

        public override void Generate(
            DropPrimaryKeyOperation dropPrimaryKeyOperation,
            IndentedStringBuilder stringBuilder)
        {
            // TODO: Rebuild table
            throw new NotImplementedException();
        }

        public override void Generate(
            AddForeignKeyOperation addForeignKeyOperation,
            IndentedStringBuilder stringBuilder)
        {
            // TODO: Rebuild table
            throw new NotImplementedException();
        }

        public override void Generate(
            DropForeignKeyOperation dropForeignKeyOperation,
            IndentedStringBuilder stringBuilder)
        {
            // TODO: Rebuild table
            throw new NotImplementedException();
        }

        public override void Generate(RenameIndexOperation renameIndexOperation, IndentedStringBuilder stringBuilder)
        {
            // TODO: Rebuild index
            throw new NotImplementedException();
        }

        public override string GenerateLiteral(byte[] value)
        {
            Check.NotNull(value, "value");

            var stringBuilder = new StringBuilder("X'");

            foreach (var @byte in value)
            {
                stringBuilder.Append(@byte.ToString("X2", CultureInfo.InvariantCulture));
            }

            stringBuilder.Append("'");

            return stringBuilder.ToString();
        }

        public override string DelimitIdentifier(SchemaQualifiedName schemaQualifiedName)
        {
            return DelimitIdentifier(
                (schemaQualifiedName.IsSchemaQualified
                    ? schemaQualifiedName.Schema + "."
                    : string.Empty)
                + schemaQualifiedName.Name);
        }
    }
}
