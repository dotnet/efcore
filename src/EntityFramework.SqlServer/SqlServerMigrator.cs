// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerMigrator : Migrator
    {
        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected SqlServerMigrator()
        {
        }

        public SqlServerMigrator(
            [NotNull] HistoryRepository historyRepository,
            [NotNull] MigrationAssembly migrationAssembly,
            [NotNull] SqlServerModelDiffer modelDiffer,
            [NotNull] SqlServerMigrationOperationSqlGeneratorFactory sqlGeneratorFactory,
            [NotNull] SqlServerSqlGenerator sqlGenerator,
            [NotNull] SqlStatementExecutor sqlStatementExecutor,
            [NotNull] SqlServerDataStoreCreator storeCreator,
            [NotNull] SqlServerConnection connection,
            [NotNull] ILoggerFactory loggerFactory)
            : base(
                historyRepository,
                migrationAssembly,
                modelDiffer,
                sqlGeneratorFactory,
                sqlGenerator,
                sqlStatementExecutor,
                storeCreator,
                connection,
                loggerFactory)
        {
        }
    }
}
