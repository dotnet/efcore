// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Utilities
{
    public class SqlServerNorthwindTestStoreFactory : SqlServerTestStoreFactory
    {
        public const string Name = "Northwind";
        public static readonly string NorthwindConnectionString = SqlServerTestStore.CreateConnectionString(Name);
        public new static SqlServerNorthwindTestStoreFactory Instance { get; } = new SqlServerNorthwindTestStoreFactory();

        protected SqlServerNorthwindTestStoreFactory()
        {
        }

        public override SqlServerTestStore CreateShared(string storeName)
            => SqlServerTestStore.GetOrCreateInitialized(Name, "Northwind.sql");
    }
}
