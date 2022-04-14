// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class CosmosNorthwindTestStoreFactory : CosmosTestStoreFactory
{
    private const string Name = "Northwind";

    public static new CosmosNorthwindTestStoreFactory Instance { get; } = new();

    protected CosmosNorthwindTestStoreFactory()
    {
    }

    public override TestStore GetOrCreate(string storeName)
        => CosmosTestStore.GetOrCreate(Name, "Northwind.json");
}
