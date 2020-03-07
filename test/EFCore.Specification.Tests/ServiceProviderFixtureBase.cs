// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class ServiceProviderFixtureBase : FixtureBase
    {
        public IServiceProvider ServiceProvider { get; }
        protected abstract ITestStoreFactory TestStoreFactory { get; }

        private ListLoggerFactory _listLoggerFactory;

        public ListLoggerFactory ListLoggerFactory
            => _listLoggerFactory
                ?? (_listLoggerFactory = (ListLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>());

        protected ServiceProviderFixtureBase()
        {
            ServiceProvider = AddServices(TestStoreFactory.AddProviderServices(new ServiceCollection()))
                .BuildServiceProvider(validateScopes: true);
        }

        public DbContextOptions CreateOptions(TestStore testStore)
            => AddOptions(testStore.AddProviderOptions(new DbContextOptionsBuilder()))
                .EnableDetailedErrors()
                .UseInternalServiceProvider(ServiceProvider)
                .EnableServiceProviderCaching(false)
                .Options;

        protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
            => base.AddServices(serviceCollection)
                .AddSingleton<ILoggerFactory>(TestStoreFactory.CreateListLoggerFactory(ShouldLogCategory));

        protected virtual bool ShouldLogCategory(string logCategory) => false;
    }
}
