// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class SqliteTestStoreFactory : RelationalTestStoreFactory
{
    public static SqliteTestStoreFactory Instance { get; } = new();

    protected SqliteTestStoreFactory()
    {
    }

    public override TestStore Create(string storeName)
        => SqliteTestStore.Create(storeName);

    public override TestStore GetOrCreate(string storeName)
        => SqliteTestStore.GetOrCreate(storeName);

    public override IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
        => serviceCollection
            .AddEntityFrameworkSqlite()
            .AddEntityFrameworkSqliteNetTopologySuite();
}
