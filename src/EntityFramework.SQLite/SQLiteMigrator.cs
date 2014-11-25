// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Sqlite
{
    public class SqliteMigrator : Migrator
    {
        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected SqliteMigrator()
        {
        }

        public SqliteMigrator(
            [NotNull] HistoryRepository historyRepository,
            [NotNull] MigrationAssembly migrationAssembly,
            [NotNull] SqliteModelDiffer modelDiffer,
            [NotNull] SqliteMigrationOperationSqlGeneratorFactory ddlSqlGeneratorFactory,
            [NotNull] SqliteSqlGenerator dmlSqlGenerator,
            [NotNull] SqlStatementExecutor sqlExecutor,
            [NotNull] SqliteDataStoreCreator storeCreator,
            [NotNull] SqliteConnection connection,
            [NotNull] ILoggerFactory loggerFactory)
            : base(
                historyRepository,
                migrationAssembly,
                modelDiffer,
                ddlSqlGeneratorFactory,
                dmlSqlGenerator,
                sqlExecutor,
                storeCreator,
                connection,
                loggerFactory)
        {
        }
    }
}
