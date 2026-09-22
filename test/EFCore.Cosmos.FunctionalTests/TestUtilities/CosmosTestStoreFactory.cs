// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

#nullable disable

public class CosmosTestStoreFactory : ITestStoreFactory
{
    public static CosmosTestStoreFactory Instance { get; } = new();

    protected CosmosTestStoreFactory()
    {
    }

    public IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
        => serviceCollection
            .AddEntityFrameworkCosmos()
            .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory());

    public TestStore Create(string storeName)
        => CosmosTestStore.Create(storeName);

    public virtual TestStore GetOrCreate(string storeName)
        => CosmosTestStore.GetOrCreate(storeName);

    public virtual ListLoggerFactory CreateListLoggerFactory(Func<string, bool> shouldLogCategory)
        => new TestSqlLoggerFactory(shouldLogCategory);
}
