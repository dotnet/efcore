// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class PrivateCacheSqliteTestStoreFactory : SqliteTestStoreFactory
{
    public static new PrivateCacheSqliteTestStoreFactory Instance { get; } = new();

    protected PrivateCacheSqliteTestStoreFactory()
    {
    }

    public override TestStore GetOrCreate(string storeName)
        => SqliteTestStore.GetOrCreate(storeName, sharedCache: false);
}
