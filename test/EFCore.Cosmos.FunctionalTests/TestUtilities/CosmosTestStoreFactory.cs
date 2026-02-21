// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

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
    public virtual ListLoggerFactory CreateListLoggerFactory(Func<string, bool> shouldLogCategory)
        => new TestSqlLoggerFactory(shouldLogCategory);

    public CosmosTestStore Create(string storeName)
        => Create(storeName, false);
    public virtual CosmosTestStore GetOrCreate(string storeName)
        => Create(storeName, true);

    public CosmosTestStore Create(string storeName, bool shared = false, Action<CosmosDbContextOptionsBuilder>? extensionConfiguration = null)
        => TestEnvironment.IsEmulator ?
            new CosmosEmulatorTestStore(storeName, shared, extensionConfiguration)
          : new CosmosTestStore(storeName, shared, extensionConfiguration);

    public async Task<CosmosTestStore> CreateInitializedAsync(string name, Action<CosmosDbContextOptionsBuilder>? extensionConfiguration = null)
    {
        var testStore = Create(name, false, extensionConfiguration);
        try
        {
            await testStore.InitializeAsync(null, (Func<DbContext>?)null).ConfigureAwait(false);
        }
        catch
        {
            await testStore.DisposeAsync().ConfigureAwait(false);
            throw;
        }

        return testStore;
    }

    TestStore ITestStoreFactory.Create(string storeName) => Create(storeName);
    TestStore ITestStoreFactory.GetOrCreate(string storeName) => GetOrCreate(storeName);
}
