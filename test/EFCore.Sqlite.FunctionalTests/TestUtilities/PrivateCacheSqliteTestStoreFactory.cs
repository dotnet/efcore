// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class PrivateCacheSqliteTestStoreFactory : SqliteTestStoreFactory
    {
        public static new PrivateCacheSqliteTestStoreFactory Instance { get; } = new PrivateCacheSqliteTestStoreFactory();

        protected PrivateCacheSqliteTestStoreFactory()
        {
        }

        public override TestStore GetOrCreate(string storeName)
            => SqliteTestStore.GetOrCreate(storeName, sharedCache: false);
    }
}
