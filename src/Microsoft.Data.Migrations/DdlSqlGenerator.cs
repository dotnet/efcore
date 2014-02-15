// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Migrations.Model;
using Microsoft.Data.Migrations.Utilities;
using Microsoft.Data.Relational;

namespace Microsoft.Data.Migrations
{
    public class DdlSqlGenerator
    {
        public static string Generate<T>([NotNull] T migrationOperation)
            where T : MigrationOperation
        {
            Check.NotNull(migrationOperation, "migrationOperation");

            var indentedStringBuilder = new IndentedStringBuilder();

            dynamic sqlServerDdlSqlGenerator = new DdlSqlGenerator();

            sqlServerDdlSqlGenerator.Generate(migrationOperation, indentedStringBuilder, true);

            return indentedStringBuilder.ToString();
        }

        public virtual void Generate(
            [NotNull] CreateSequenceOperation createSequenceOperation,
            [NotNull] IndentedStringBuilder stringBuilder,
            bool idempotent)
        {
            Check.NotNull(createSequenceOperation, "createSequenceOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder.Append("CREATE SEQUENCE ");

            if (createSequenceOperation.SchemaQualifiedName.IsSchemaQualified)
            {
                stringBuilder
                    .Append(DelimitIdentifier(createSequenceOperation.SchemaQualifiedName.Schema))
                    .Append(".");
            }

            stringBuilder
                .Append(DelimitIdentifier(createSequenceOperation.SchemaQualifiedName.Name))
                .Append(" AS ")
                .Append(createSequenceOperation.DataType)
                .Append(" START WITH ")
                .Append(createSequenceOperation.StartWith)
                .Append(" INCREMENT BY ")
                .Append(createSequenceOperation.IncrementBy);
        }

        public virtual string DelimitIdentifier(SchemaQualifiedName schemaQualifiedName)
        {
            Check.NotEmpty(schemaQualifiedName, "schemaQualifiedName");

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
