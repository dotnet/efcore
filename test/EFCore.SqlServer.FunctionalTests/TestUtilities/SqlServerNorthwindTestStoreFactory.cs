// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class SqlServerNorthwindTestStoreFactory : SqlServerTestStoreFactory
{
    public const string Name = "Northwind";
    public static readonly string NorthwindConnectionString = SqlServerTestStore.CreateConnectionString(Name);
    public static new SqlServerNorthwindTestStoreFactory Instance { get; } = new();

    protected SqlServerNorthwindTestStoreFactory()
    {
    }

    public override TestStore GetOrCreate(string storeName)
        => SqlServerTestStore.GetOrCreateWithScriptPath(storeName, "Northwind.sql");
}
