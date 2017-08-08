// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Utilities
{
    public class PrivateCacheSqliteTestStoreFactory : SqliteTestStoreFactory
    {
        public new static PrivateCacheSqliteTestStoreFactory Instance { get; } = new PrivateCacheSqliteTestStoreFactory();

        protected PrivateCacheSqliteTestStoreFactory()
        {
        }

        public override SqliteTestStore CreateShared(string storeName)
            => SqliteTestStore.GetOrCreate(storeName, sharedCache: false);
    }
}
