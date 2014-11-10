// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.SQLite
{
    public class SQLiteMigrator : Migrator
    {
        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected SQLiteMigrator()
        {
        }

        public SQLiteMigrator(
            [NotNull] DbContextConfiguration contextConfiguration,
            [NotNull] HistoryRepository historyRepository,
            [NotNull] MigrationAssembly migrationAssembly,
            [NotNull] SQLiteModelDiffer modelDiffer,
            [NotNull] SQLiteMigrationOperationSqlGeneratorFactory ddlSqlGeneratorFactory,
            [NotNull] SQLiteSqlGenerator dmlSqlGenerator,
            [NotNull] SqlStatementExecutor sqlExecutor,
            [NotNull] ILoggerFactory loggerFactory)
            : base(
                contextConfiguration,
                historyRepository,
                migrationAssembly,
                modelDiffer,
                ddlSqlGeneratorFactory,
                dmlSqlGenerator,
                sqlExecutor,
                loggerFactory)
        {
        }
    }
}
