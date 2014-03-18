// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Migrations;
using Microsoft.Data.Migrations.Model;
using Microsoft.Data.SqlServer.Utilities;

namespace Microsoft.Data.SqlServer
{
    public class SqlServerMigrationOperationSqlGenerator : MigrationOperationSqlGenerator
    {
        public new static string Generate<T>([NotNull] T migrationOperation)
            where T : MigrationOperation
        {
            Check.NotNull(migrationOperation, "migrationOperation");

            var stringBuilder = new IndentedStringBuilder();

            migrationOperation
                .GenerateOperationSql(
                    new SqlServerMigrationOperationSqlGenerator(),
                    stringBuilder,
                    generateIdempotentSql: true);

            return stringBuilder.ToString();
        }

        public override void Generate(
            AddPrimaryKeyOperation addPrimaryKeyOperation, IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(addPrimaryKeyOperation, "addPrimaryKeyOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            // TODO: Handle generateIdempotentSql

            stringBuilder.Append("ALTER TABLE ");
            stringBuilder.Append(DelimitIdentifier(addPrimaryKeyOperation.Table.Name));
            stringBuilder.Append(" ADD CONSTRAINT ");
            stringBuilder.Append(DelimitIdentifier(addPrimaryKeyOperation.PrimaryKey.Name));
            stringBuilder.Append(" PRIMARY KEY ");

            if (!addPrimaryKeyOperation.PrimaryKey.IsClustered) // TODO: Use annotation
            {
                stringBuilder.Append("NONCLUSTERED ");
            }

            stringBuilder.Append("(");
            stringBuilder.Append(addPrimaryKeyOperation.PrimaryKey.Columns.Select(c => DelimitIdentifier(c.Name)).Join());
            stringBuilder.Append(")");
        }

        public override void Generate(
            CreateSequenceOperation createSequenceOperation,
            IndentedStringBuilder stringBuilder,
            bool generateIdempotentSql)
        {
            Check.NotNull(createSequenceOperation, "createSequenceOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            if (generateIdempotentSql)
            {
                stringBuilder
                    .Append(@"IF NOT EXISTS (SELECT * FROM sys.sequences WHERE name = ")
                    .Append(DelimitLiteral(createSequenceOperation.Sequence.Name.Name))
                    .Append(")");

                if (createSequenceOperation.Sequence.Name.IsSchemaQualified)
                {
                    stringBuilder
                        .Append(" AND schema_id = SCHEMA_ID(")
                        .Append(DelimitLiteral(createSequenceOperation.Sequence.Name.Schema))
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
