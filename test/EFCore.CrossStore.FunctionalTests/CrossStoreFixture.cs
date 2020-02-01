// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.TestModels;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
    public class CrossStoreFixture : FixtureBase
    {
        public DbContextOptions CreateOptions(TestStore testStore)
            => AddOptions(testStore.AddProviderOptions(new DbContextOptionsBuilder()))
                .UseInternalServiceProvider(testStore.ServiceProvider)
                .Options;

        public CrossStoreContext CreateContext(TestStore testStore)
            => new CrossStoreContext(CreateOptions(testStore));

        public TestStore CreateTestStore(ITestStoreFactory testStoreFactory, string storeName, Action<CrossStoreContext> seed = null)
            => testStoreFactory.GetOrCreate(storeName)
                .Initialize(
                    AddServices(testStoreFactory.AddProviderServices(new ServiceCollection())).BuildServiceProvider(validateScopes: true),
                    CreateContext,
                    seed);
    }
}
