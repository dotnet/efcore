// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Cosmos.TestUtilities
{
    public class CosmosNorthwindTestStoreFactory : CosmosTestStoreFactory
    {
        private const string Name = "Northwind";

        public static new CosmosNorthwindTestStoreFactory Instance { get; }
            = new CosmosNorthwindTestStoreFactory();

        protected CosmosNorthwindTestStoreFactory()
        {
        }

        public override TestStore GetOrCreate(string storeName)
            => CosmosTestStore.GetOrCreate(Name, "Northwind.json");
    }
}
