// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Utilities
{
    public class SqlServerNorthwindTestStoreFactory : SqlServerTestStoreFactory
    {
        public new static SqlServerNorthwindTestStoreFactory Instance { get; } = new SqlServerNorthwindTestStoreFactory();

        protected SqlServerNorthwindTestStoreFactory()
        {
        }

        public override SqlServerTestStore CreateShared(string storeName)
            => SqlServerTestStore.GetOrCreateShared("Northwind",
                Path.Combine(Path.GetDirectoryName(typeof(SqlServerTestStore).GetTypeInfo().Assembly.Location), "Northwind.sql"));
    }
}
