// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Sqlite
{
    public class SqliteDatabase : MigrationsEnabledDatabase
    {
        public SqliteDatabase(
            [NotNull] DbContextService<IModel> model,
            [NotNull] SqliteDataStoreCreator dataStoreCreator,
            [NotNull] SqliteConnection connection,
            [NotNull] SqliteMigrator migrator,
            [NotNull] ILoggerFactory loggerFactory)
            : base(model, dataStoreCreator, connection, migrator, loggerFactory)
        {
        }

        public new virtual SqliteConnection Connection
        {
            get { return (SqliteConnection)base.Connection; }
        }
    }
}
