// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.SQLite.Utilities;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.SQLite
{
    public class SQLiteDataStoreSource
        : DataStoreSource<
            SQLiteDataStore,
            SQLiteOptionsExtension,
            SQLiteDataStoreCreator,
            SQLiteConnectionConnection,
            SQLiteValueGeneratorCache,
            RelationalDatabase>
    {
        public override bool IsAvailable(DbContextConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            return IsConfigured(configuration);
        }

        public override string Name
        {
            get { return typeof(SQLiteDataStore).Name; }
        }
    }
}
