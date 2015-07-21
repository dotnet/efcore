// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Migrations.History;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Update;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Migrations
{
    public class SqlServerHistoryRepository : HistoryRepository
    {
        private readonly ISqlServerUpdateSqlGenerator _sql;

        public SqlServerHistoryRepository(
            [NotNull] IDatabaseCreator databaseCreator,
            [NotNull] ISqlStatementExecutor executor,
            [NotNull] ISqlServerConnection connection,
            [NotNull] IMigrationModelFactory modelFactory,
            [NotNull] IDbContextOptions options,
            [NotNull] IModelDiffer modelDiffer,
            [NotNull] SqlServerMigrationSqlGenerator migrationSqlGenerator,
            [NotNull] SqlServerMetadataExtensionProvider annotations,
            [NotNull] ISqlServerUpdateSqlGenerator updateSqlGenerator,
            [NotNull] IServiceProvider serviceProvider)
            : base(
                  databaseCreator,
                  executor,
                  connection,
                  modelFactory,
                  options,
                  modelDiffer,
                  migrationSqlGenerator,
                  annotations,
                  updateSqlGenerator,
                  serviceProvider)
        {
            Check.NotNull(updateSqlGenerator, nameof(updateSqlGenerator));

            _sql = updateSqlGenerator;
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
                        .Append(_sql.EscapeLiteral(TableSchema))
                        .Append(".");
                }

                builder
                    .Append(_sql.EscapeLiteral(TableName))
                    .Append("');");

                return builder.ToString();
            }
        }

        protected override bool Exists(object value) => value != DBNull.Value;

        public override string GetInsertScript(HistoryRow row)
        {
            Check.NotNull(row, nameof(row));

            return new StringBuilder().Append("INSERT INTO ")
                .Append(_sql.DelimitIdentifier(TableName, TableSchema))
                .Append(" (")
                .Append(_sql.DelimitIdentifier(MigrationIdColumnName))
                .Append(", ")
                .Append(_sql.DelimitIdentifier(ProductVersionColumnName))
                .AppendLine(")")
                .Append("VALUES (N'")
                .Append(_sql.EscapeLiteral(row.MigrationId))
                .Append("', N'")
                .Append(_sql.EscapeLiteral(row.ProductVersion))
                .Append("');")
                .ToString();
        }

        public override string GetDeleteScript(string migrationId)
        {
            Check.NotEmpty(migrationId, nameof(migrationId));

            return new StringBuilder().Append("DELETE FROM ")
                .AppendLine(_sql.DelimitIdentifier(TableName, TableSchema))
                .Append("WHERE ")
                .Append(_sql.DelimitIdentifier(MigrationIdColumnName))
                .Append(" = N'")
                .Append(_sql.EscapeLiteral(migrationId))
                .Append("';")
                .ToString();
        }

        public override string GetCreateIfNotExistsScript()
        {
            var builder = new IndentedStringBuilder();

            builder.Append("IF OBJECT_ID(N'");

            if (TableSchema != null)
            {
                builder
                    .Append(_sql.EscapeLiteral(TableSchema))
                    .Append(".");
            }

            builder
                .Append(_sql.EscapeLiteral(TableName))
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
                .Append(_sql.DelimitIdentifier(TableName, TableSchema))
                .Append(" WHERE ")
                .Append(_sql.DelimitIdentifier(MigrationIdColumnName))
                .Append(" = N'")
                .Append(_sql.EscapeLiteral(migrationId))
                .AppendLine("')")
                .Append("BEGIN")
                .ToString();
        }

        public override string GetBeginIfExistsScript(string migrationId)
        {
            Check.NotEmpty(migrationId, nameof(migrationId));

            return new StringBuilder()
                .Append("IF EXISTS(SELECT * FROM ")
                .Append(_sql.DelimitIdentifier(TableName, TableSchema))
                .Append(" WHERE ")
                .Append(_sql.DelimitIdentifier(MigrationIdColumnName))
                .Append(" = N'")
                .Append(_sql.EscapeLiteral(migrationId))
                .AppendLine("')")
                .Append("BEGIN")
                .ToString();
        }

        public override string GetEndIfScript() => "END";
    }
}
