// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Migrations.Internal
{
    public class SqliteHistoryRepository : HistoryRepository
    {
        public SqliteHistoryRepository(
            [NotNull] IDatabaseCreator databaseCreator,
            [NotNull] IRawSqlCommandBuilder rawSqlCommandBuilder,
            [NotNull] IRelationalConnection connection,
            [NotNull] IDbContextOptions options,
            [NotNull] IMigrationsModelDiffer modelDiffer,
            [NotNull] IMigrationsSqlGenerator migrationsSqlGenerator,
            [NotNull] IRelationalAnnotationProvider annotations,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper)
            : base(
                databaseCreator,
                rawSqlCommandBuilder,
                connection,
                options,
                modelDiffer,
                migrationsSqlGenerator,
                annotations,
                sqlGenerationHelper)
        {
        }

        protected override string ExistsSql
            => "SELECT COUNT(*) FROM \"sqlite_master\" WHERE \"name\" = '" +
               SqlGenerationHelper.EscapeLiteral(TableName) +
               "' AND \"type\" = 'table';";

        protected override bool InterpretExistsResult(object value) => (long)value != 0L;

        public override string GetCreateIfNotExistsScript()
        {
            var script = GetCreateScript();

            return script.Insert(script.IndexOf("CREATE TABLE") + 12, " IF NOT EXISTS");
        }

        public override string GetBeginIfNotExistsScript(string migrationId)
        {
            throw new NotSupportedException(SqliteStrings.MigrationScriptGenerationNotSupported);
        }

        public override string GetBeginIfExistsScript(string migrationId)
        {
            throw new NotSupportedException(SqliteStrings.MigrationScriptGenerationNotSupported);
        }

        public override string GetEndIfScript()
        {
            throw new NotSupportedException(SqliteStrings.MigrationScriptGenerationNotSupported);
        }
    }
}
