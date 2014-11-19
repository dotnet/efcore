// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.SQLite
{
    public class SQLiteDatabase : MigrationsEnabledDatabase
    {
        public SQLiteDatabase(
            [NotNull] LazyRef<IModel> model,
            [NotNull] SQLiteDataStoreCreator dataStoreCreator,
            [NotNull] SQLiteConnection connection,
            [NotNull] SQLiteMigrator migrator,
            [NotNull] ILoggerFactory loggerFactory)
            : base(model, dataStoreCreator, connection, migrator, loggerFactory)
        {
        }

        public new virtual SQLiteConnection Connection
        {
            get { return (SQLiteConnection)base.Connection; }
        }
    }
}
