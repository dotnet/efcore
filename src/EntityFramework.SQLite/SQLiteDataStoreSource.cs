// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Sqlite
{
    public class SqliteDataStoreSource : DataStoreSource<SqliteDataStoreServices, SqliteOptionsExtension>
    {
        public SqliteDataStoreSource([NotNull] DbContextConfiguration configuration, [NotNull] LazyRef<IDbContextOptions> options)
            : base(configuration, options)
        {
        }

        public override string Name
        {
            get { return typeof(SqliteDataStore).Name; }
        }
    }
}
