// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable VirtualMemberCallInConstructor
namespace Microsoft.EntityFrameworkCore
{
    public abstract class SharedStoreFixtureBase<TContext> : FixtureBase, IDisposable
        where TContext : DbContext
    {
        protected virtual Type ContextType => typeof(TContext);
        public IServiceProvider ServiceProvider { get; }
        protected abstract string StoreName { get; }
        protected abstract ITestStoreFactory<TestStore> TestStoreFactory { get; }
        public TestStore TestStore { get; }

        protected SharedStoreFixtureBase()
        {
            TestStore = TestStoreFactory.CreateShared(StoreName);

            ServiceProvider =
                AddServices(
                    TestStoreFactory.AddProviderServices(new ServiceCollection()))
                    .AddDbContext(
                        ContextType,
                        (s, b) => ConfigureOptions(s, b),
                        ServiceLifetime.Transient,
                        ServiceLifetime.Singleton)
                    .BuildServiceProvider(validateScopes: true);

            TestStore.Initialize(ServiceProvider, CreateContext, c => Seed((TContext)c));
        }

        public virtual TContext CreateContext() => (TContext)ServiceProvider.GetRequiredService(ContextType);

        public DbContextOptions CreateOptions()
            => ConfigureOptions(ServiceProvider, new DbContextOptionsBuilder()).Options;

        private DbContextOptionsBuilder ConfigureOptions(IServiceProvider serviceProvider, DbContextOptionsBuilder optionsBuilder)
            => AddOptions(TestStore.AddProviderOptions(optionsBuilder))
                .UseInternalServiceProvider(serviceProvider);

        public virtual void Reseed()
        {
            using (var context = CreateContext())
            {
                TestStore.Clean(context);
                Seed(context);
            }
        }

        protected virtual void Seed(TContext context)
        {
        }

        public void Dispose() => TestStore.Dispose();
    }
}
