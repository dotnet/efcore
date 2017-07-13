// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable VirtualMemberCallInConstructor
namespace Microsoft.EntityFrameworkCore
{
    public abstract class SharedStoreFixtureBase<TContext> : IDisposable
        where TContext : DbContext
    {
        public IServiceProvider ServiceProvider { get; }
        public TestStore TestStore { get; }
        protected abstract ITestStoreFactory<TestStore> TestStoreFactory { get; }
        protected abstract string StoreName { get; }
        protected virtual Type ContextType => typeof(TContext);

        protected SharedStoreFixtureBase()
        {
            TestStore = TestStoreFactory.CreateShared(StoreName);

            ServiceProvider =
                AddServices(
                    TestStore.AddProviderServices(new ServiceCollection()))
                    .AddDbContext(
                        ContextType,
                        (s, b) => ConfigureOptions(s, b),
                        ServiceLifetime.Transient,
                        ServiceLifetime.Singleton)
                    .BuildServiceProvider(validateScopes: true);

            TestStore.Initialize(ServiceProvider, CreateContext, c => Seed((TContext)c));
        }

        public TContext CreateContext() => (TContext)ServiceProvider.GetRequiredService(ContextType);

        protected virtual IServiceCollection AddServices(IServiceCollection serviceCollection)
            => serviceCollection.AddSingleton(TestModelSource.GetFactory(OnModelCreating));

        public DbContextOptions CreateOptions()
            => ConfigureOptions(ServiceProvider, new DbContextOptionsBuilder()).Options;

        private DbContextOptionsBuilder ConfigureOptions(IServiceProvider serviceProvider, DbContextOptionsBuilder optionsBuilder)
            => AddOptions(TestStore.AddProviderOptions(optionsBuilder))
                .UseInternalServiceProvider(serviceProvider);

        protected virtual DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => builder
                .EnableSensitiveDataLogging()
                .ConfigureWarnings(b => b.Default(WarningBehavior.Throw).Log(CoreEventId.SensitiveDataLoggingEnabledWarning));

        protected virtual void Seed(TContext context)
        {
        }

        protected abstract void OnModelCreating(ModelBuilder modelBuilder, DbContext context);

        public void Dispose() => TestStore.Dispose();
    }
}
