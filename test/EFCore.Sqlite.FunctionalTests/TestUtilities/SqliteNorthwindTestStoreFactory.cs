// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class SqliteNorthwindTestStoreFactory : SqliteTestStoreFactory
    {
        public static new SqliteNorthwindTestStoreFactory Instance { get; } = new SqliteNorthwindTestStoreFactory();

        protected SqliteNorthwindTestStoreFactory()
        {
        }

        public override TestStore GetOrCreate(string storeName)
            => SqliteTestStore.GetExisting("northwind");
    }
}
