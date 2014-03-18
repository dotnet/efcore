// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Migrations.Model;
using Microsoft.Data.Migrations.Utilities;
using Microsoft.Data.Relational;

namespace Microsoft.Data.Migrations
{
    /// <summary>
    ///     Default migration operation SQL generator, outputs best-effort ANSI-99 compliant SQL.
    /// </summary>
    public class MigrationOperationSqlGenerator
    {
        public static string Generate<T>([NotNull] T migrationOperation)
            where T : MigrationOperation
        {
            Check.NotNull(migrationOperation, "migrationOperation");

            var stringBuilder = new IndentedStringBuilder();

            dynamic sqlServerDdlSqlGenerator = new MigrationOperationSqlGenerator();

            sqlServerDdlSqlGenerator.Generate(migrationOperation, stringBuilder, true);

            return stringBuilder.ToString();
        }

        public virtual void Generate(
            [NotNull] CreateSequenceOperation createSequenceOperation,
            [NotNull] IndentedStringBuilder stringBuilder,
            bool generateIdempotentSql)
        {
            Check.NotNull(createSequenceOperation, "createSequenceOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append("CREATE SEQUENCE ")
                .Append(DelimitIdentifier(createSequenceOperation.Sequence.Name))
                .Append(" AS ")
                .Append(createSequenceOperation.Sequence.DataType)
                .Append(" START WITH ")
                .Append(createSequenceOperation.Sequence.StartWith)
                .Append(" INCREMENT BY ")
                .Append(createSequenceOperation.Sequence.IncrementBy);
        }

        public virtual void Generate(
            [NotNull] DropSequenceOperation dropSequenceOperation,
            [NotNull] IndentedStringBuilder stringBuilder,
            bool generateIdempotentSql)
        {
            Check.NotNull(dropSequenceOperation, "dropSequenceOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            // TODO: Not implemented.
        }

        public virtual void Generate(
            [NotNull] CreateTableOperation createTableOperation,
            [NotNull] IndentedStringBuilder stringBuilder,
            bool generateIdempotentSql)
        {
            Check.NotNull(createTableOperation, "createTableOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            // TODO: Not implemented.
        }

        public virtual void Generate(
            [NotNull] DropTableOperation dropTableOperation,
            [NotNull] IndentedStringBuilder stringBuilder,
            bool generateIdempotentSql)
        {
            Check.NotNull(dropTableOperation, "dropTableOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            // TODO: Not implemented.
        }

        public virtual void Generate(
            [NotNull] RenameTableOperation renameTableOperation,
            [NotNull] IndentedStringBuilder stringBuilder,
            bool generateIdempotentSql)
        {
            Check.NotNull(renameTableOperation, "renameTableOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            // TODO: Not implemented.
        }

        public virtual void Generate(
            [NotNull] MoveTableOperation moveTableOperation,
            [NotNull] IndentedStringBuilder stringBuilder,
            bool generateIdempotentSql)
        {
            Check.NotNull(moveTableOperation, "moveTableOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            // TODO: Not implemented.
        }

        public virtual void Generate(
            [NotNull] AddColumnOperation addColumnOperation,
            [NotNull] IndentedStringBuilder stringBuilder,
            bool generateIdempotentSql)
        {
            Check.NotNull(addColumnOperation, "addColumnOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            // TODO: Not implemented.
        }

        public virtual void Generate(
            [NotNull] DropColumnOperation dropColumnOperation,
            [NotNull] IndentedStringBuilder stringBuilder,
            bool generateIdempotentSql)
        {
            Check.NotNull(dropColumnOperation, "dropColumnOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            // TODO: Not implemented.
        }

        public virtual void Generate(
            [NotNull] RenameColumnOperation renameColumnOperation,
            [NotNull] IndentedStringBuilder stringBuilder,
            bool generateIdempotentSql)
        {
            Check.NotNull(renameColumnOperation, "renameColumnOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            // TODO: Not implemented.
        }

        public virtual void Generate(
            [NotNull] AddPrimaryKeyOperation addPrimaryKeyOperation,
            [NotNull] IndentedStringBuilder stringBuilder,
            bool generateIdempotentSql)
        {
            Check.NotNull(addPrimaryKeyOperation, "addPrimaryKeyOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append("ALTER TABLE ")
                .Append(DelimitIdentifier(addPrimaryKeyOperation.Table.Name))
                .Append(" ADD CONSTRAINT ")
                .Append(DelimitIdentifier(addPrimaryKeyOperation.PrimaryKey.Name))
                .Append(" PRIMARY KEY ")
                .Append("(")
                .Append(addPrimaryKeyOperation.PrimaryKey.Columns.Select(c => DelimitIdentifier(c.Name)).Join())
                .Append(")");
        }

        public virtual void Generate(
            [NotNull] DropPrimaryKeyOperation dropPrimaryKeyOperation,
            [NotNull] IndentedStringBuilder stringBuilder,
            bool generateIdempotentSql)
        {
            Check.NotNull(dropPrimaryKeyOperation, "dropPrimaryKeyOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            // TODO: Not implemented.
        }

        public virtual void Generate(
            [NotNull] MigrationOperation migrationOperation,
            [NotNull] IndentedStringBuilder stringBuilder,
            bool generateIdempotentSql)
        {
            Check.NotNull(migrationOperation, "migrationOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            throw new NotSupportedException(Strings.FormatUnknownOperation(GetType(), migrationOperation.GetType()));
        }

        public virtual string DelimitIdentifier(SchemaQualifiedName schemaQualifiedName)
        {
            return
                (schemaQualifiedName.IsSchemaQualified
                    ? DelimitIdentifier(schemaQualifiedName.Schema) + "."
                    : string.Empty)
                + DelimitIdentifier(schemaQualifiedName.Name);
        }

        public virtual string DelimitIdentifier([NotNull] string identifier)
        {
            Check.NotEmpty(identifier, "identifier");

            return "\"" + EscapeIdentifier(identifier) + "\"";
        }

        public virtual string EscapeIdentifier([NotNull] string identifier)
        {
            Check.NotEmpty(identifier, "identifier");

            return identifier.Replace("\"", "\"\"");
        }

        public virtual string DelimitLiteral([NotNull] string literal)
        {
            Check.NotNull(literal, "literal");

            return "'" + EscapeLiteral(literal) + "'";
        }

        public virtual string EscapeLiteral([NotNull] string literal)
        {
            Check.NotNull(literal, "literal");

            return literal.Replace("'", "''");
        }
    }
}
