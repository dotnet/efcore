// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational;

namespace Microsoft.Data.Entity.SQLite
{
    public class SQLiteMigrator : Migrator
    {
        public SQLiteMigrator(
            [NotNull] DbContextConfiguration contextConfiguration,
            [NotNull] HistoryRepository historyRepository,
            [NotNull] MigrationAssembly migrationAssembly,
            [NotNull] ModelDiffer modelDiffer,
            [NotNull] SQLiteMigrationOperationSqlGeneratorFactory ddlSqlGeneratorFactory,
            [NotNull] SQLiteSqlGenerator dmlSqlGenerator,
            [NotNull] SqlStatementExecutor sqlExecutor)
            : base(
                contextConfiguration,
                historyRepository,
                migrationAssembly,
                modelDiffer,
                ddlSqlGeneratorFactory,
                dmlSqlGenerator,
                sqlExecutor)
        {
        }
    }
}
