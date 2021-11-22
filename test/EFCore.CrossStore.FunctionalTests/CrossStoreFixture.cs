// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels;

namespace Microsoft.EntityFrameworkCore;

public class CrossStoreFixture : FixtureBase
{
    public DbContextOptions CreateOptions(TestStore testStore)
        => AddOptions(testStore.AddProviderOptions(new DbContextOptionsBuilder()))
            .UseInternalServiceProvider(testStore.ServiceProvider)
            .Options;

    public CrossStoreContext CreateContext(TestStore testStore)
        => new(CreateOptions(testStore));

    public TestStore CreateTestStore(ITestStoreFactory testStoreFactory, string storeName, Action<CrossStoreContext> seed = null)
        => testStoreFactory.GetOrCreate(storeName)
            .Initialize(
                AddServices(testStoreFactory.AddProviderServices(new ServiceCollection())).BuildServiceProvider(validateScopes: true),
                CreateContext,
                seed);
}
