// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Migrations.History;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Sqlite.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Update;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Sqlite.Migrations
{
    public class SqliteHistoryRepository : HistoryRepository
    {
        private readonly SqliteUpdateSqlGenerator _sql;

        public SqliteHistoryRepository(
            [NotNull] IDatabaseCreator databaseCreator,
            [NotNull] ISqlStatementExecutor executor,
            [NotNull] IRelationalConnection connection,
            [NotNull] IMigrationModelFactory modelFactory,
            [NotNull] IDbContextOptions options,
            [NotNull] IModelDiffer modelDiffer,
            [NotNull] SqliteMigrationSqlGenerator migrationSqlGenerator,
            [NotNull] SqliteMetadataExtensionProvider annotations,
            [NotNull] SqliteUpdateSqlGenerator updateSqlGenerator,
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
            => "SELECT COUNT(*) FROM \"sqlite_master\" WHERE \"name\" = '" +
                _sql.EscapeLiteral(TableName) +
                "' AND \"type\" = 'table';";

        protected override bool Exists(object value) => (long)value != 0L;

        public override string GetCreateIfNotExistsScript()
        {
            var script = GetCreateScript();

            return script.Insert(script.IndexOf("CREATE TABLE") + 12, " IF NOT EXISTS");
        }

        public override string GetBeginIfNotExistsScript(string migrationId)
        {
            throw new NotSupportedException(Strings.MigrationScriptGenerationNotSupported);
        }

        public override string GetBeginIfExistsScript(string migrationId)
        {
            throw new NotSupportedException(Strings.MigrationScriptGenerationNotSupported);
        }

        public override string GetEndIfScript()
        {
            throw new NotSupportedException(Strings.MigrationScriptGenerationNotSupported);
        }
    }
}
