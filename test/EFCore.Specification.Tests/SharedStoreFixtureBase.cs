// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

// ReSharper disable VirtualMemberCallInConstructor
namespace Microsoft.EntityFrameworkCore
{
    public abstract class SharedStoreFixtureBase<TContext> : FixtureBase, IDisposable, IAsyncLifetime
        where TContext : DbContext
    {
        protected virtual Type ContextType { get; } = typeof(TContext);

        private IServiceProvider _serviceProvider;
        public IServiceProvider ServiceProvider
            => _serviceProvider ?? throw new InvalidOperationException($"You must override the {nameof(InitializeAsync)} method and call `await base.{nameof(InitializeAsync)}();`. At this point the {nameof(ServiceProvider)} property will be available.");

        protected abstract string StoreName { get; }
        protected abstract ITestStoreFactory TestStoreFactory { get; }

        private TestStore _testStore;
        public TestStore TestStore
            => _testStore ?? throw new InvalidOperationException($"You must override the {nameof(InitializeAsync)} method and call `await base.{nameof(InitializeAsync)}();`. At this point the {nameof(TestStore)} property will be available.");

        protected virtual bool UsePooling
            => true;

        private IDbContextPool _contextPool;

        private IDbContextPool ContextPool
            => _contextPool ??= (IDbContextPool)ServiceProvider
                .GetRequiredService(typeof(IDbContextPool<>).MakeGenericType(ContextType));

        private ListLoggerFactory _listLoggerFactory;

        public ListLoggerFactory ListLoggerFactory
            => _listLoggerFactory ??= (ListLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();

        public virtual Task InitializeAsync()
        {
            _testStore = TestStoreFactory.GetOrCreate(StoreName);

            var services = AddServices(TestStoreFactory.AddProviderServices(new ServiceCollection()));
            if (UsePooling)
            {
                services = services.AddDbContextPool(ContextType, (s, b) => ConfigureOptions(s, b));
            }
            else
            {
                services = services.AddDbContext(
                    ContextType,
                    (s, b) => ConfigureOptions(s, b),
                    ServiceLifetime.Transient,
                    ServiceLifetime.Singleton);
            }

            _serviceProvider = services.BuildServiceProvider(validateScopes: true);

            TestStore.Initialize(ServiceProvider, CreateContext, c => Seed((TContext)c), c => Clean(c));

            return Task.CompletedTask;
        }

        public virtual TContext CreateContext()
            => UsePooling
                ? (TContext)new DbContextLease(ContextPool, standalone: true).Context
                : (TContext)ServiceProvider.GetRequiredService(ContextType);

        public DbContextOptions CreateOptions()
            => ConfigureOptions(ServiceProvider, new DbContextOptionsBuilder()).Options;

        private DbContextOptionsBuilder ConfigureOptions(IServiceProvider serviceProvider, DbContextOptionsBuilder optionsBuilder)
            => AddOptions(TestStore.AddProviderOptions(optionsBuilder))
                .UseInternalServiceProvider(serviceProvider);

        protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
            => base.AddServices(serviceCollection)
                .AddSingleton<ILoggerFactory>(TestStoreFactory.CreateListLoggerFactory(ShouldLogCategory));

        protected virtual bool ShouldLogCategory(string logCategory)
            => false;

        public virtual void Reseed()
        {
            using var context = CreateContext();
            Clean(context);
            TestStore.Clean(context);
            Seed(context);
        }

        public virtual async Task ReseedAsync()
        {
            using var context = CreateContext();
            await CleanAsync(context);
            await TestStore.CleanAsync(context);
            await SeedAsync(context);
        }

        protected virtual void Seed(TContext context)
        {
        }

        protected virtual Task SeedAsync(TContext context)
        {
            Seed(context);
            return Task.CompletedTask;
        }

        protected virtual void Clean(DbContext context)
        {
        }

        protected virtual Task CleanAsync(DbContext context)
        {
            Clean(context);
            return Task.CompletedTask;
        }

        // Called after DisposeAsync
        public virtual void Dispose()
        {
        }

        public virtual Task DisposeAsync()
            => TestStore.DisposeAsync();
    }
}
