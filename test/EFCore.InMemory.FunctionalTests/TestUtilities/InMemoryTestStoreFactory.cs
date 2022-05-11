// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class InMemoryTestStoreFactory : ITestStoreFactory
{
    public static InMemoryTestStoreFactory Instance { get; } = new();

    protected InMemoryTestStoreFactory()
    {
    }

    public TestStore Create(string storeName)
        => InMemoryTestStore.Create(storeName);

    public TestStore GetOrCreate(string storeName)
        => InMemoryTestStore.GetOrCreate(storeName);

    public IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
        => serviceCollection.AddEntityFrameworkInMemoryDatabase()
            .AddSingleton<TestStoreIndex>();

    public ListLoggerFactory CreateListLoggerFactory(Func<string, bool> shouldLogCategory)
        => new(shouldLogCategory);
}
