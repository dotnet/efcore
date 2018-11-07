// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ReSharper disable VirtualMemberCallInConstructor
namespace Microsoft.EntityFrameworkCore
{
    public abstract class SharedStoreFixtureBase<TContext> : FixtureBase, IDisposable
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
            => _contextPool
               ?? (_contextPool = (IDbContextPool)ServiceProvider.GetRequiredService(typeof(DbContextPool<>).MakeGenericType(ContextType)));

        private ListLoggerFactory _listLoggerFactory;

        public ListLoggerFactory ListLoggerFactory
            => _listLoggerFactory
               ?? (_listLoggerFactory = (ListLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>());

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

            TestStore.Initialize(ServiceProvider, CreateContext, c => Seed((TContext)c));
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
                TestStore.Clean(context);
                Seed(context);
            }
        }

        protected virtual void Seed(TContext context)
        {
        }

        public virtual void Dispose() => TestStore.Dispose();
    }
}
