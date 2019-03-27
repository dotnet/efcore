// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class PrivateCacheSqliteTestStoreFactory : SqliteTestStoreFactory
    {
        public static new PrivateCacheSqliteTestStoreFactory Instance { get; } = new PrivateCacheSqliteTestStoreFactory();

        static PrivateCacheSqliteTestStoreFactory()
        {
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = new System.Globalization.CultureInfo("cs-CZ");
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("cs-CZ");
        }

        protected PrivateCacheSqliteTestStoreFactory()
        {
        }

        public override TestStore GetOrCreate(string storeName)
            => SqliteTestStore.GetOrCreate(storeName, sharedCache: false);
    }
}
