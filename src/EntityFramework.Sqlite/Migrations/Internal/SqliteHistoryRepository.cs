// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Sqlite.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Update.Internal;

namespace Microsoft.Data.Entity.Migrations.Internal
{
    public class SqliteHistoryRepository : HistoryRepository
    {
        public SqliteHistoryRepository(
            [NotNull] IDatabaseCreator databaseCreator,
            [NotNull] ISqlStatementExecutor executor,
            [NotNull] IRelationalConnection connection,
            [NotNull] IDbContextOptions options,
            [NotNull] IMigrationsModelDiffer modelDiffer,
            [NotNull] SqliteMigrationsSqlGenerator sqlGenerator,
            [NotNull] SqliteAnnotationProvider annotations,
            [NotNull] SqliteUpdateSqlGenerator sql)
            : base(
                databaseCreator,
                executor,
                connection,
                options,
                modelDiffer,
                sqlGenerator,
                annotations,
                sql)
        {
        }

        protected override string ExistsSql
            => "SELECT COUNT(*) FROM \"sqlite_master\" WHERE \"name\" = '" +
               Sql.EscapeLiteral(TableName) +
               "' AND \"type\" = 'table';";

        protected override bool InterpretExistsResult(object value) => (long)value != 0L;

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
