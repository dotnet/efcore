// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class ServiceProviderFixtureBase : FixtureBase
    {
        public IServiceProvider ServiceProvider { get; }
        protected abstract ITestStoreFactory<TestStore> TestStoreFactory { get; }

        protected ServiceProviderFixtureBase()
        {
            ServiceProvider = AddServices(TestStoreFactory.AddProviderServices(new ServiceCollection()))
                .BuildServiceProvider(validateScopes: true);
        }

        public DbContextOptions CreateOptions(TestStore testStore)
            => AddOptions(testStore.AddProviderOptions(new DbContextOptionsBuilder()))
                .UseInternalServiceProvider(ServiceProvider)
                .Options;
    }
}
