// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class SqliteNorthwindTestStoreFactory : SqliteTestStoreFactory
{
    public static new SqliteNorthwindTestStoreFactory Instance { get; } = new();

    protected SqliteNorthwindTestStoreFactory()
    {
    }

    public override TestStore GetOrCreate(string storeName)
        => SqliteTestStore.GetExisting("northwind");
}
