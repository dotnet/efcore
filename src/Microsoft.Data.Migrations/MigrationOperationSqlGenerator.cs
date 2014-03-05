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
            [NotNull] AddPrimaryKeyOperation addPrimaryKeyOperation,
            [NotNull] IndentedStringBuilder stringBuilder,
            bool generateIdempotentSql)
        {
            Check.NotNull(addPrimaryKeyOperation, "addPrimaryKeyOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder.Append("ALTER TABLE ");
            stringBuilder.Append(DelimitIdentifier(addPrimaryKeyOperation.Table.Name));
            stringBuilder.Append(" ADD CONSTRAINT ");
            stringBuilder.Append(DelimitIdentifier(addPrimaryKeyOperation.Target.Name));
            stringBuilder.Append(" PRIMARY KEY ");
            stringBuilder.Append("(");
            stringBuilder.Append(addPrimaryKeyOperation.Target.Columns.Select(c => DelimitIdentifier(c.Name)).Join());
            stringBuilder.Append(")");
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
                .Append(DelimitIdentifier(createSequenceOperation.Target.Name))
                .Append(" AS ")
                .Append(createSequenceOperation.Target.DataType)
                .Append(" START WITH ")
                .Append(createSequenceOperation.Target.StartWith)
                .Append(" INCREMENT BY ")
                .Append(createSequenceOperation.Target.IncrementBy);
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
