// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.TestUtilities
{
    public class CosmosSqlNorthwindTestStoreFactory : CosmosSqlTestStoreFactory
    {
        private const string Name = "Northwind";

        public new static CosmosSqlNorthwindTestStoreFactory Instance { get; }
            = new CosmosSqlNorthwindTestStoreFactory();

        protected CosmosSqlNorthwindTestStoreFactory()
        {
        }

        public override TestStore GetOrCreate(string storeName)
            => CosmosSqlTestStore.GetOrCreate(Name, "Northwind.json");
    }
}
