// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Storage.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Migrations.Internal
{
    public class SqlServerHistoryRepository : HistoryRepository
    {
        public SqlServerHistoryRepository(
            [NotNull] IDatabaseCreator databaseCreator,
            [NotNull] ISqlStatementExecutor executor,
            [NotNull] ISqlServerConnection connection,
            [NotNull] IDbContextOptions options,
            [NotNull] IMigrationsModelDiffer modelDiffer,
            [NotNull] IMigrationsSqlGenerator migrationsSqlGenerator,
            [NotNull] IRelationalAnnotationProvider annotations,
            [NotNull] ISqlGenerator sqlGenerator)
            : base(
                databaseCreator,
                executor,
                connection,
                options,
                modelDiffer,
                migrationsSqlGenerator,
                annotations,
                sqlGenerator)
        {
        }

        protected override string ExistsSql
        {
            get
            {
                var builder = new StringBuilder();

                builder.Append("SELECT OBJECT_ID(N'");

                if (TableSchema != null)
                {
                    builder
                        .Append(SqlGenerator.EscapeLiteral(TableSchema))
                        .Append(".");
                }

                builder
                    .Append(SqlGenerator.EscapeLiteral(TableName))
                    .Append("');");

                return builder.ToString();
            }
        }

        protected override bool InterpretExistsResult(object value) => value != DBNull.Value;

        public override string GetInsertScript(HistoryRow row)
        {
            Check.NotNull(row, nameof(row));

            return new StringBuilder().Append("INSERT INTO ")
                .Append(SqlGenerator.DelimitIdentifier(TableName, TableSchema))
                .Append(" (")
                .Append(SqlGenerator.DelimitIdentifier(MigrationIdColumnName))
                .Append(", ")
                .Append(SqlGenerator.DelimitIdentifier(ProductVersionColumnName))
                .AppendLine(")")
                .Append("VALUES (N'")
                .Append(SqlGenerator.EscapeLiteral(row.MigrationId))
                .Append("', N'")
                .Append(SqlGenerator.EscapeLiteral(row.ProductVersion))
                .AppendLine("');")
                .ToString();
        }

        public override string GetDeleteScript(string migrationId)
        {
            Check.NotEmpty(migrationId, nameof(migrationId));

            return new StringBuilder().Append("DELETE FROM ")
                .AppendLine(SqlGenerator.DelimitIdentifier(TableName, TableSchema))
                .Append("WHERE ")
                .Append(SqlGenerator.DelimitIdentifier(MigrationIdColumnName))
                .Append(" = N'")
                .Append(SqlGenerator.EscapeLiteral(migrationId))
                .AppendLine("';")
                .ToString();
        }

        public override string GetCreateIfNotExistsScript()
        {
            var builder = new IndentedStringBuilder();

            builder.Append("IF OBJECT_ID(N'");

            if (TableSchema != null)
            {
                builder
                    .Append(SqlGenerator.EscapeLiteral(TableSchema))
                    .Append(".");
            }

            builder
                .Append(SqlGenerator.EscapeLiteral(TableName))
                .AppendLine("') IS NULL");
            using (builder.Indent())
            {
                builder.AppendLines(GetCreateScript());
            }

            return builder.ToString();
        }

        public override string GetBeginIfNotExistsScript(string migrationId)
        {
            Check.NotEmpty(migrationId, nameof(migrationId));

            return new StringBuilder()
                .Append("IF NOT EXISTS(SELECT * FROM ")
                .Append(SqlGenerator.DelimitIdentifier(TableName, TableSchema))
                .Append(" WHERE ")
                .Append(SqlGenerator.DelimitIdentifier(MigrationIdColumnName))
                .Append(" = N'")
                .Append(SqlGenerator.EscapeLiteral(migrationId))
                .AppendLine("')")
                .Append("BEGIN")
                .ToString();
        }

        public override string GetBeginIfExistsScript(string migrationId)
        {
            Check.NotEmpty(migrationId, nameof(migrationId));

            return new StringBuilder()
                .Append("IF EXISTS(SELECT * FROM ")
                .Append(SqlGenerator.DelimitIdentifier(TableName, TableSchema))
                .Append(" WHERE ")
                .Append(SqlGenerator.DelimitIdentifier(MigrationIdColumnName))
                .Append(" = N'")
                .Append(SqlGenerator.EscapeLiteral(migrationId))
                .AppendLine("')")
                .Append("BEGIN")
                .ToString();
        }

        public override string GetEndIfScript() => "END" + Environment.NewLine;
    }
}
