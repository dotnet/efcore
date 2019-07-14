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
        public IServiceProvider ServiceProvider { get; }
        protected abstract string StoreName { get; }
        protected abstract ITestStoreFactory TestStoreFactory { get; }
        public TestStore TestStore { get; }
        protected virtual bool UsePooling => true;

        private IDbContextPool _contextPool;

        private IDbContextPool ContextPool
            => _contextPool ??= (IDbContextPool)ServiceProvider.GetRequiredService(typeof(DbContextPool<>).MakeGenericType(ContextType));

        private ListLoggerFactory _listLoggerFactory;

        public ListLoggerFactory ListLoggerFactory
            => _listLoggerFactory ??= (ListLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();

        protected SharedStoreFixtureBase()
        {
            TestStore = TestStoreFactory.GetOrCreate(StoreName);

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

            ServiceProvider = services.BuildServiceProvider(validateScopes: true);

            TestStore.Initialize(ServiceProvider, CreateContext, c => Seed((TContext)c), c => Clean(c));
        }

        public virtual Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public virtual TContext CreateContext()
        {
            if (UsePooling)
            {
                var context = (PoolableDbContext)ContextPool.Rent();
                context.SetPool(ContextPool);
                return (TContext)(object)context;
            }

            return (TContext)ServiceProvider.GetRequiredService(ContextType);
        }

        public DbContextOptions CreateOptions()
            => ConfigureOptions(ServiceProvider, new DbContextOptionsBuilder()).Options;

        private DbContextOptionsBuilder ConfigureOptions(IServiceProvider serviceProvider, DbContextOptionsBuilder optionsBuilder)
            => AddOptions(TestStore.AddProviderOptions(optionsBuilder))
                .UseInternalServiceProvider(serviceProvider);

        protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
            => base.AddServices(serviceCollection)
                .AddSingleton<ILoggerFactory>(TestStoreFactory.CreateListLoggerFactory(ShouldLogCategory));

        protected virtual bool ShouldLogCategory(string logCategory) => false;

        public virtual void Reseed()
        {
            using (var context = CreateContext())
            {
                Clean(context);
                TestStore.Clean(context);
                Seed(context);
            }
        }

        public virtual async Task ReseedAsync()
        {
            using (var context = CreateContext())
            {
                await CleanAsync(context);
                await TestStore.CleanAsync(context);
                await SeedAsync(context);
            }
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

        public virtual Task DisposeAsync() => TestStore.DisposeAsync();
    }
}
