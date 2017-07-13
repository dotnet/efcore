// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Utilities
{
    public class SqliteNorthwindTestStoreFactory : SqliteTestStoreFactory
    {
        public new static SqliteNorthwindTestStoreFactory Instance { get; } = new SqliteNorthwindTestStoreFactory();

        protected SqliteNorthwindTestStoreFactory()
        {
        }

        public override SqliteTestStore CreateShared(string storeName)
            => SqliteTestStore.GetExisting("northwind");
    }
}
