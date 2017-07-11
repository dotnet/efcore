// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable VirtualMemberCallInConstructor
namespace Microsoft.EntityFrameworkCore
{
    public abstract class SharedStoreFixtureBase<TTestStore, TContext> : IDisposable
        where TTestStore : TestStore
        where TContext : DbContext
    {
        public IServiceProvider ServiceProvider { get; }
        public TTestStore TestStore { get; }
        protected abstract string StoreName { get; }
        protected abstract TestStoreFactory<TTestStore> TestStoreFactory { get; }

        protected SharedStoreFixtureBase()
        {
            ServiceProvider = AddServices(TestStoreFactory.AddProviderServices(new ServiceCollection()))
                .BuildServiceProvider(validateScopes: true);

            TestStore = TestStoreFactory.CreateShared(StoreName, ServiceProvider, AddOptions, CreateContext, c => Seed((TContext)c));
        }

        public virtual TContext CreateContext() => (TContext)TestStore.CreateContext();
        protected abstract TContext CreateContext(DbContextOptions options);

        protected virtual DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => builder;

        protected virtual IServiceCollection AddServices(IServiceCollection serviceCollection)
            => serviceCollection.AddSingleton(TestModelSource.GetFactory(OnModelCreating));

        protected virtual void Seed(TContext context)
        {
        }

        protected abstract void OnModelCreating(ModelBuilder modelBuilder);

        public void Dispose() => TestStore.Dispose();
    }
}
