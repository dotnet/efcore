// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Migrations;
using Microsoft.Data.Migrations.Model;
using Microsoft.Data.SqlServer.Utilities;

namespace Microsoft.Data.SqlServer
{
    public class SqlServerDdlSqlGenerator : DdlSqlGenerator
    {
        public new static string Generate<T>([NotNull] T migrationOperation)
            where T : MigrationOperation
        {
            Check.NotNull(migrationOperation, "migrationOperation");

            var indentedStringBuilder = new IndentedStringBuilder();

            dynamic sqlServerDdlSqlGenerator = new SqlServerDdlSqlGenerator();

            sqlServerDdlSqlGenerator.Generate(migrationOperation, indentedStringBuilder, true);

            return indentedStringBuilder.ToString();
        }

        public override void Generate(
            CreateSequenceOperation createSequenceOperation,
            IndentedStringBuilder stringBuilder,
            bool idempotent)
        {
            Check.NotNull(createSequenceOperation, "createSequenceOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            if (idempotent)
            {
                stringBuilder
                    .Append(@"IF NOT EXISTS (SELECT * FROM sys.sequences WHERE name = ")
                    .Append(DelimitLiteral(createSequenceOperation.SchemaQualifiedName.Name))
                    .Append(")");

                if (createSequenceOperation.SchemaQualifiedName.IsSchemaQualified)
                {
                    stringBuilder
                        .Append(" AND schema_id = SCHEMA_ID(")
                        .Append(DelimitLiteral(createSequenceOperation.SchemaQualifiedName.Schema))
                        .Append(")");
                }

                using (stringBuilder.AppendLine().Indent())
                {
                    base.Generate(createSequenceOperation, stringBuilder, false);
                }
            }
            else
            {
                base.Generate(createSequenceOperation, stringBuilder, false);
            }
        }
    }
}
