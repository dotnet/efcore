// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Utilities
{
    public class SqlServerAdventureWorksTestStoreFactory : SqlServerTestStoreFactory
    {
        public new static SqlServerAdventureWorksTestStoreFactory Instance { get; } = new SqlServerAdventureWorksTestStoreFactory();

        protected SqlServerAdventureWorksTestStoreFactory()
        {
        }

        public override SqlServerTestStore CreateShared(string storeName)
            => SqlServerTestStore.GetOrCreateInitialized("adventureworks",
                Path.Combine("SqlAzure","adventureworks.sql"));
    }
}
