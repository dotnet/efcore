// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class SharedCacheSqliteTestStoreFactory : SqliteTestStoreFactory
{
    public static new SharedCacheSqliteTestStoreFactory Instance { get; } = new();

    protected SharedCacheSqliteTestStoreFactory()
    {
    }

    public override TestStore GetOrCreate(string storeName)
        => SqliteTestStore.GetOrCreate(storeName, sharedCache: true);
}
