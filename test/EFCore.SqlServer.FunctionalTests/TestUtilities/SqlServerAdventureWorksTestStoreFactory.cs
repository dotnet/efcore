// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class SqlServerAdventureWorksTestStoreFactory : SqlServerTestStoreFactory
    {
        public static new SqlServerAdventureWorksTestStoreFactory Instance { get; } = new SqlServerAdventureWorksTestStoreFactory();

        protected SqlServerAdventureWorksTestStoreFactory()
        {
        }

        public override TestStore GetOrCreate(string storeName)
            => SqlServerTestStore.GetOrCreate(
                "adventureworks",
                Path.Combine("SqlAzure", "adventureworks.sql"));
    }
}
