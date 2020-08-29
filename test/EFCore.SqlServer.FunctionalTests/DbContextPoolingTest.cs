// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable VirtualMemberCallInConstructor
namespace Microsoft.EntityFrameworkCore
{
    public class DbContextPoolingTest : IClassFixture<NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
    {
        private static IServiceProvider BuildServiceProvider<TContextService, TContext>(int poolSize = 32)
            where TContextService : class
            where TContext : DbContext, TContextService
            => new ServiceCollection()
                .AddDbContextPool<TContextService, TContext>(
                    ob =>
                        ob.UseSqlServer(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString)
                            .EnableServiceProviderCaching(false),
                    poolSize)
                .AddDbContextPool<ISecondContext, SecondContext>(
                    ob =>
                        ob.UseSqlServer(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString)
                            .EnableServiceProviderCaching(false),
                    poolSize).BuildServiceProvider();

        private static IServiceProvider BuildServiceProvider<TContext>(int poolSize = 32)
            where TContext : DbContext
            => new ServiceCollection()
                .AddDbContextPool<TContext>(
                    ob =>
                        ob.UseSqlServer(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString)
                            .EnableServiceProviderCaching(false),
                    poolSize)
                .AddDbContextPool<SecondContext>(
                    ob =>
                        ob.UseSqlServer(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString)
                            .EnableServiceProviderCaching(false),
                    poolSize)
                .BuildServiceProvider();

        private static IServiceProvider BuildServiceProviderWithFactory<TContext>(int poolSize = 32)
            where TContext : DbContext
            => new ServiceCollection()
                .AddPooledDbContextFactory<TContext>(
                    ob =>
                        ob.UseSqlServer(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString)
                            .EnableServiceProviderCaching(false),
                    poolSize)
                .AddDbContextPool<SecondContext>(
                    ob =>
                        ob.UseSqlServer(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString)
                            .EnableServiceProviderCaching(false),
                    poolSize)
                .BuildServiceProvider();

        private interface IPooledContext
        {
        }

        private class DefaultOptionsPooledContext : DbContext
        {
            public DefaultOptionsPooledContext(DbContextOptions options)
                : base(options)
            {
            }
        }

        private class PooledContext : DbContext, IPooledContext
        {
            public static int DisposedCount;
            public static int InstanceCount;

            public static bool ModifyOptions;

            public PooledContext(DbContextOptions options)
                : base(options)
            {
                Interlocked.Increment(ref InstanceCount);

                ChangeTracker.AutoDetectChangesEnabled = false;
                ChangeTracker.LazyLoadingEnabled = false;
                Database.AutoTransactionsEnabled = false;
                ChangeTracker.CascadeDeleteTiming = CascadeTiming.Never;
                ChangeTracker.DeleteOrphansTiming = CascadeTiming.Never;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                if (ModifyOptions)
                {
                    optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                }
            }

            public DbSet<Customer> Customers { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Customer>().ToTable("Customers");

            public override void Dispose()
            {
                base.Dispose();

                Interlocked.Increment(ref DisposedCount);
            }

            public class Customer
            {
                public string CustomerId { get; set; }
                public string CompanyName { get; set; }
            }
        }

        private interface ISecondContext
        {
        }

        private class SecondContext : DbContext, ISecondContext
        {
            public SecondContext(DbContextOptions options)
                : base(options)
            {
            }
        }

        [ConditionalFact]
        public void Invalid_pool_size()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => BuildServiceProvider<PooledContext>(poolSize: 0));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => BuildServiceProvider<PooledContext>(poolSize: -1));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => BuildServiceProviderWithFactory<PooledContext>(poolSize: 0));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => BuildServiceProviderWithFactory<PooledContext>(poolSize: -1));
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public void Options_modified_in_on_configuring(bool useInterface)
        {
            var serviceProvider = useInterface
                ? BuildServiceProvider<IPooledContext, PooledContext>()
                : BuildServiceProvider<PooledContext>();

            var scopedProvider = serviceProvider.CreateScope().ServiceProvider;

            PooledContext.ModifyOptions = true;

            try
            {
                Assert.Throws<InvalidOperationException>(
                    () => useInterface
                        ? scopedProvider.GetService<IPooledContext>()
                        : scopedProvider.GetService<PooledContext>());
            }
            finally
            {
                PooledContext.ModifyOptions = false;
            }
        }

        [ConditionalFact]
        public void Options_modified_in_on_configuring_with_factory()
        {
            var serviceProvider = BuildServiceProviderWithFactory<PooledContext>();
            var scopedProvider = serviceProvider.CreateScope().ServiceProvider;

            PooledContext.ModifyOptions = true;

            try
            {
                var factory = scopedProvider.GetService<IDbContextFactory<PooledContext>>();
                Assert.Throws<InvalidOperationException>(() => factory.CreateDbContext());
            }
            finally
            {
                PooledContext.ModifyOptions = false;
            }
        }

        private class BadCtorContext : DbContext
        {
        }

        [ConditionalFact]
        public void Throws_when_used_with_parameterless_constructor_context()
        {
            var serviceCollection = new ServiceCollection();

            Assert.Equal(
                CoreStrings.DbContextMissingConstructor(nameof(BadCtorContext)),
                Assert.Throws<ArgumentException>(
                    () => serviceCollection.AddDbContextPool<BadCtorContext>(
                        _ => { })).Message);

            Assert.Equal(
                CoreStrings.DbContextMissingConstructor(nameof(BadCtorContext)),
                Assert.Throws<ArgumentException>(
                    () => serviceCollection.AddDbContextPool<BadCtorContext>(
                        (_, __) => { })).Message);

            Assert.Equal(
                CoreStrings.DbContextMissingConstructor(nameof(BadCtorContext)),
                Assert.Throws<ArgumentException>(
                    () => serviceCollection.AddPooledDbContextFactory<BadCtorContext>(
                        (_, __) => { })).Message);
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public async Task Can_pool_non_derived_context(bool useFactory, bool async)
        {
            var serviceProvider = useFactory
                ? BuildServiceProviderWithFactory<DbContext>()
                : BuildServiceProvider<DbContext>();

            var serviceScope1 = serviceProvider.CreateScope();
            var context1 = GetContext(serviceScope1);

            var serviceScope2 = serviceProvider.CreateScope();
            var context2 = GetContext(serviceScope2);

            Assert.NotSame(context1, context2);

            var id1 = context1.ContextId;
            var id2 = context2.ContextId;

            Assert.NotEqual(default, id1.InstanceId);
            Assert.NotEqual(id1, id2);
            Assert.NotEqual(id1.InstanceId, id2.InstanceId);
            Assert.Equal(1, id1.Lease);
            Assert.Equal(1, id2.Lease);

            if (useFactory)
            {
                await Dispose(context1, async);
            }

            await Dispose(serviceScope1, async);
            await Dispose(serviceScope2, async);

            if (useFactory)
            {
                await Dispose(context2, async);
            }

            var id1d = context1.ContextId;
            var id2d = context2.ContextId;

            Assert.Equal(id1, id1d);
            Assert.Equal(id1.InstanceId, id1d.InstanceId);
            Assert.Equal(1, id1d.Lease);
            Assert.Equal(1, id2d.Lease);

            var serviceScope3 = serviceProvider.CreateScope();
            var context3 = GetContext(serviceScope3);

            var id1r = context3.ContextId;

            Assert.Same(context1, context3);
            Assert.Equal(id1.InstanceId, id1r.InstanceId);
            Assert.NotEqual(default, id1r.InstanceId);
            Assert.NotEqual(id1, id1r);
            Assert.Equal(2, id1r.Lease);

            var serviceScope4 = serviceProvider.CreateScope();
            var context4 = GetContext(serviceScope4);

            var id2r = context4.ContextId;

            Assert.Same(context2, context4);
            Assert.Equal(id2.InstanceId, id2r.InstanceId);
            Assert.NotEqual(default, id2r.InstanceId);
            Assert.NotEqual(id2, id2r);
            Assert.Equal(2, id2r.Lease);

            DbContext GetContext(IServiceScope serviceScope)
                => useFactory
                    ? serviceScope.ServiceProvider.GetService<IDbContextFactory<DbContext>>().CreateDbContext()
                    : serviceScope.ServiceProvider.GetService<DbContext>();
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ContextIds_make_sense_when_not_pooling(bool async)
        {
            var serviceProvider = new ServiceCollection()
                .AddDbContext<DbContext>(
                    ob
                        => ob.UseSqlServer(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString)
                            .EnableServiceProviderCaching(false))
                .BuildServiceProvider();

            var serviceScope1 = serviceProvider.CreateScope();
            var context1 = serviceScope1.ServiceProvider.GetService<DbContext>();

            var serviceScope2 = serviceProvider.CreateScope();
            var context2 = serviceScope2.ServiceProvider.GetService<DbContext>();

            Assert.NotSame(context1, context2);

            var id1 = context1.ContextId;
            var id2 = context2.ContextId;

            Assert.NotEqual(default, id1.InstanceId);
            Assert.NotEqual(default, id2.InstanceId);

            Assert.NotEqual(id1, id2);
            Assert.Equal(0, id1.Lease);
            Assert.Equal(0, id2.Lease);

            await Dispose(serviceScope1, async);
            await Dispose(serviceScope2, async);

            var id1d = context1.ContextId;
            var id2d = context2.ContextId;

            Assert.Equal(id1.InstanceId, id1d.InstanceId);
            Assert.Equal(id2.InstanceId, id2d.InstanceId);
            Assert.Equal(0, id1d.Lease);
            Assert.Equal(0, id2d.Lease);

            var serviceScope3 = serviceProvider.CreateScope();
            var context3 = serviceScope3.ServiceProvider.GetService<DbContext>();

            var id1r = context3.ContextId;

            Assert.NotSame(context1, context3);
            Assert.NotEqual(default, id1r.InstanceId);
            Assert.NotEqual(id1.InstanceId, id1r.InstanceId);
            Assert.NotEqual(id1, id1r);
            Assert.Equal(0, id1r.Lease);

            var serviceScope4 = serviceProvider.CreateScope();
            var context4 = serviceScope4.ServiceProvider.GetService<DbContext>();

            var id2r = context4.ContextId;

            Assert.NotSame(context2, context4);
            Assert.NotEqual(default, id2r.InstanceId);
            Assert.NotEqual(id2.InstanceId, id2r.InstanceId);
            Assert.NotEqual(id2, id2r);
            Assert.Equal(0, id2r.Lease);
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public async Task Contexts_are_pooled(bool useInterface, bool async)
        {
            var serviceProvider = useInterface
                ? BuildServiceProvider<IPooledContext, PooledContext>()
                : BuildServiceProvider<PooledContext>();

            var serviceScope1 = serviceProvider.CreateScope();
            var scopedProvider1 = serviceScope1.ServiceProvider;

            var context1 = useInterface
                ? scopedProvider1.GetService<IPooledContext>()
                : scopedProvider1.GetService<PooledContext>();

            var secondContext1 = useInterface
                ? scopedProvider1.GetService<ISecondContext>()
                : scopedProvider1.GetService<SecondContext>();

            var serviceScope2 = serviceProvider.CreateScope();
            var scopedProvider2 = serviceScope2.ServiceProvider;

            var context2 = useInterface
                ? scopedProvider2.GetService<IPooledContext>()
                : scopedProvider2.GetService<PooledContext>();

            var secondContext2 = useInterface
                ? scopedProvider2.GetService<ISecondContext>()
                : scopedProvider2.GetService<SecondContext>();

            Assert.NotSame(context1, context2);
            Assert.NotSame(secondContext1, secondContext2);

            await Dispose(serviceScope1, async);
            await Dispose(serviceScope2, async);

            var serviceScope3 = serviceProvider.CreateScope();
            var scopedProvider3 = serviceScope3.ServiceProvider;

            var context3 = useInterface
                ? scopedProvider3.GetService<IPooledContext>()
                : scopedProvider3.GetService<PooledContext>();

            var secondContext3 = useInterface
                ? scopedProvider3.GetService<ISecondContext>()
                : scopedProvider3.GetService<SecondContext>();

            Assert.Same(context1, context3);
            Assert.Same(secondContext1, secondContext3);

            var serviceScope4 = serviceProvider.CreateScope();
            var scopedProvider4 = serviceScope4.ServiceProvider;

            var context4 = useInterface
                ? scopedProvider4.GetService<IPooledContext>()
                : scopedProvider4.GetService<PooledContext>();

            var secondContext4 = useInterface
                ? scopedProvider4.GetService<ISecondContext>()
                : scopedProvider4.GetService<SecondContext>();

            Assert.Same(context2, context4);
            Assert.Same(secondContext2, secondContext4);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Contexts_are_pooled_with_factory(bool async)
        {
            var factory = BuildServiceProviderWithFactory<PooledContext>().GetService<IDbContextFactory<PooledContext>>();

            var context1 = factory.CreateDbContext();
            var secondContext1 = factory.CreateDbContext();

            var context2 = factory.CreateDbContext();
            var secondContext2 = factory.CreateDbContext();

            Assert.NotSame(context1, context2);
            Assert.NotSame(secondContext1, secondContext2);

            await Dispose(context1, async);
            await Dispose(secondContext1, async);
            await Dispose(context2, async);
            await Dispose(secondContext2, async);

            var context3 = factory.CreateDbContext();
            var secondContext3 = factory.CreateDbContext();

            Assert.Same(context1, context3);
            Assert.Same(secondContext1, secondContext3);

            var context4 = factory.CreateDbContext();
            var secondContext4 = factory.CreateDbContext();

            Assert.Same(context2, context4);
            Assert.Same(secondContext2, secondContext4);
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public async Task Context_configuration_is_reset(bool useInterface, bool async)
        {
            var serviceProvider = useInterface
                ? BuildServiceProvider<IPooledContext, PooledContext>()
                : BuildServiceProvider<PooledContext>();

            var serviceScope = serviceProvider.CreateScope();
            var scopedProvider = serviceScope.ServiceProvider;

            var context1 = useInterface
                ? (DbContext)scopedProvider.GetService<IPooledContext>()
                : scopedProvider.GetService<PooledContext>();

            context1.ChangeTracker.AutoDetectChangesEnabled = true;
            context1.ChangeTracker.LazyLoadingEnabled = true;
            context1.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            context1.ChangeTracker.CascadeDeleteTiming = CascadeTiming.Immediate;
            context1.ChangeTracker.DeleteOrphansTiming = CascadeTiming.Immediate;
            context1.Database.AutoTransactionsEnabled = true;

            await Dispose(serviceScope, async);

            serviceScope = serviceProvider.CreateScope();
            scopedProvider = serviceScope.ServiceProvider;

            var context2 = useInterface
                ? (DbContext)scopedProvider.GetService<IPooledContext>()
                : scopedProvider.GetService<PooledContext>();

            Assert.Same(context1, context2);

            Assert.False(context2.ChangeTracker.AutoDetectChangesEnabled);
            Assert.False(context2.ChangeTracker.LazyLoadingEnabled);
            Assert.Equal(QueryTrackingBehavior.TrackAll, context2.ChangeTracker.QueryTrackingBehavior);
            Assert.Equal(CascadeTiming.Never, context2.ChangeTracker.CascadeDeleteTiming);
            Assert.Equal(CascadeTiming.Never, context2.ChangeTracker.DeleteOrphansTiming);
            Assert.False(context2.Database.AutoTransactionsEnabled);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Context_configuration_is_reset_with_factory(bool async)
        {
            var factory = BuildServiceProviderWithFactory<PooledContext>().GetService<IDbContextFactory<PooledContext>>();

            var context1 = factory.CreateDbContext();

            context1.ChangeTracker.AutoDetectChangesEnabled = true;
            context1.ChangeTracker.LazyLoadingEnabled = true;
            context1.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            context1.ChangeTracker.CascadeDeleteTiming = CascadeTiming.Immediate;
            context1.ChangeTracker.DeleteOrphansTiming = CascadeTiming.Immediate;
            context1.Database.AutoTransactionsEnabled = true;

            await Dispose(context1, async);

            var context2 = factory.CreateDbContext();

            Assert.Same(context1, context2);

            Assert.False(context2.ChangeTracker.AutoDetectChangesEnabled);
            Assert.False(context2.ChangeTracker.LazyLoadingEnabled);
            Assert.Equal(QueryTrackingBehavior.TrackAll, context2.ChangeTracker.QueryTrackingBehavior);
            Assert.Equal(CascadeTiming.Never, context2.ChangeTracker.CascadeDeleteTiming);
            Assert.Equal(CascadeTiming.Never, context2.ChangeTracker.DeleteOrphansTiming);
            Assert.False(context2.Database.AutoTransactionsEnabled);
        }

        [ConditionalFact]
        public void Change_tracker_can_be_cleared_without_resetting_context_config()
        {
            var context = new PooledContext(
                new DbContextOptionsBuilder().UseSqlServer(
                    SqlServerNorthwindTestStoreFactory.NorthwindConnectionString).Options);

            context.ChangeTracker.AutoDetectChangesEnabled = true;
            context.ChangeTracker.LazyLoadingEnabled = true;
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            context.ChangeTracker.CascadeDeleteTiming = CascadeTiming.Immediate;
            context.ChangeTracker.DeleteOrphansTiming = CascadeTiming.Immediate;
            context.Database.AutoTransactionsEnabled = true;
            context.ChangeTracker.Tracked += ChangeTracker_OnTracked;
            context.ChangeTracker.StateChanged += ChangeTracker_OnStateChanged;

            context.ChangeTracker.Clear();

            Assert.True(context.ChangeTracker.AutoDetectChangesEnabled);
            Assert.True(context.ChangeTracker.LazyLoadingEnabled);
            Assert.Equal(QueryTrackingBehavior.NoTracking, context.ChangeTracker.QueryTrackingBehavior);
            Assert.Equal(CascadeTiming.Immediate, context.ChangeTracker.CascadeDeleteTiming);
            Assert.Equal(CascadeTiming.Immediate, context.ChangeTracker.DeleteOrphansTiming);
            Assert.True(context.Database.AutoTransactionsEnabled);

            Assert.False(_changeTracker_OnTracked);
            Assert.False(_changeTracker_OnStateChanged);

            context.Customers.Attach(
                new PooledContext.Customer { CustomerId = "C" }).State = EntityState.Modified;

            Assert.True(_changeTracker_OnTracked);
            Assert.True(_changeTracker_OnStateChanged);
        }

        private bool _changeTracker_OnTracked;

        private void ChangeTracker_OnTracked(object sender, EntityTrackedEventArgs e)
            => _changeTracker_OnTracked = true;

        private bool _changeTracker_OnStateChanged;

        private void ChangeTracker_OnStateChanged(object sender, EntityStateChangedEventArgs e)
            => _changeTracker_OnStateChanged = true;

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Default_Context_configuration_is_reset(bool async)
        {
            var serviceProvider = BuildServiceProvider<DefaultOptionsPooledContext>();

            var serviceScope = serviceProvider.CreateScope();
            var scopedProvider = serviceScope.ServiceProvider;

            var context1 = scopedProvider.GetService<DefaultOptionsPooledContext>();

            context1.ChangeTracker.AutoDetectChangesEnabled = false;
            context1.ChangeTracker.LazyLoadingEnabled = false;
            context1.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            context1.Database.AutoTransactionsEnabled = false;
            context1.ChangeTracker.CascadeDeleteTiming = CascadeTiming.Immediate;
            context1.ChangeTracker.DeleteOrphansTiming = CascadeTiming.Immediate;

            await Dispose(serviceScope, async);

            serviceScope = serviceProvider.CreateScope();
            scopedProvider = serviceScope.ServiceProvider;

            var context2 = scopedProvider.GetService<DefaultOptionsPooledContext>();

            Assert.Same(context1, context2);

            Assert.True(context2.ChangeTracker.AutoDetectChangesEnabled);
            Assert.True(context2.ChangeTracker.LazyLoadingEnabled);
            Assert.Equal(QueryTrackingBehavior.TrackAll, context2.ChangeTracker.QueryTrackingBehavior);
            Assert.Equal(CascadeTiming.Immediate, context2.ChangeTracker.CascadeDeleteTiming);
            Assert.Equal(CascadeTiming.Immediate, context2.ChangeTracker.DeleteOrphansTiming);
            Assert.True(context2.Database.AutoTransactionsEnabled);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Default_Context_configuration_is_reset_with_factory(bool async)
        {
            var factory = BuildServiceProviderWithFactory<DefaultOptionsPooledContext>()
                .GetService<IDbContextFactory<DefaultOptionsPooledContext>>();

            var context1 = factory.CreateDbContext();

            context1.ChangeTracker.AutoDetectChangesEnabled = false;
            context1.ChangeTracker.LazyLoadingEnabled = false;
            context1.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            context1.Database.AutoTransactionsEnabled = false;
            context1.ChangeTracker.CascadeDeleteTiming = CascadeTiming.Immediate;
            context1.ChangeTracker.DeleteOrphansTiming = CascadeTiming.Immediate;

            await Dispose(context1, async);

            var context2 = factory.CreateDbContext();

            Assert.Same(context1, context2);

            Assert.True(context2.ChangeTracker.AutoDetectChangesEnabled);
            Assert.True(context2.ChangeTracker.LazyLoadingEnabled);
            Assert.Equal(QueryTrackingBehavior.TrackAll, context2.ChangeTracker.QueryTrackingBehavior);
            Assert.Equal(CascadeTiming.Immediate, context2.ChangeTracker.CascadeDeleteTiming);
            Assert.Equal(CascadeTiming.Immediate, context2.ChangeTracker.DeleteOrphansTiming);
            Assert.True(context2.Database.AutoTransactionsEnabled);
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public async Task State_manager_is_reset(bool useInterface, bool async)
        {
            var weakRef = await Scoper(
                async () =>
                {
                    var serviceProvider = useInterface
                        ? BuildServiceProvider<IPooledContext, PooledContext>()
                        : BuildServiceProvider<PooledContext>();

                    var serviceScope = serviceProvider.CreateScope();
                    var scopedProvider = serviceScope.ServiceProvider;

                    var context1 = useInterface
                        ? (PooledContext)scopedProvider.GetService<IPooledContext>()
                        : scopedProvider.GetService<PooledContext>();

                    var entity = context1.Customers.First(c => c.CustomerId == "ALFKI");

                    Assert.Single(context1.ChangeTracker.Entries());

                    await Dispose(serviceScope, async);

                    serviceScope = serviceProvider.CreateScope();
                    scopedProvider = serviceScope.ServiceProvider;

                    var context2 = useInterface
                        ? (PooledContext)scopedProvider.GetService<IPooledContext>()
                        : scopedProvider.GetService<PooledContext>();

                    Assert.Same(context1, context2);
                    Assert.Empty(context2.ChangeTracker.Entries());

                    return new WeakReference(entity);
                });

            GC.Collect();

            Assert.False(weakRef.IsAlive);
        }

        private static async Task<T> Scoper<T>(Func<Task<T>> getter)
            => await getter();

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task State_manager_is_reset_with_factory(bool async)
        {
            var weakRef = await Scoper(
                async () =>
                {
                    var factory = BuildServiceProviderWithFactory<PooledContext>()
                        .GetService<IDbContextFactory<PooledContext>>();

                    var context1 = factory.CreateDbContext();

                    var entity = context1.Customers.First(c => c.CustomerId == "ALFKI");

                    Assert.Single(context1.ChangeTracker.Entries());

                    await Dispose(context1, async);

                    var context2 = factory.CreateDbContext();

                    Assert.Same(context1, context2);
                    Assert.Empty(context2.ChangeTracker.Entries());

                    return new WeakReference(entity);
                });

            GC.Collect();

            Assert.False(weakRef.IsAlive);
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public async Task Pool_disposes_context_when_context_not_pooled(bool useInterface, bool async)
        {
            var serviceProvider = useInterface
                ? BuildServiceProvider<IPooledContext, PooledContext>()
                : BuildServiceProvider<PooledContext>();

            var serviceScope1 = serviceProvider.CreateScope();
            var scopedProvider1 = serviceScope1.ServiceProvider;

            if (useInterface)
            {
                scopedProvider1.GetService<IPooledContext>();
            }
            else
            {
                scopedProvider1.GetService<PooledContext>();
            }

            var serviceScope2 = serviceProvider.CreateScope();
            var scopedProvider2 = serviceScope2.ServiceProvider;

            var context = useInterface
                ? (PooledContext)scopedProvider2.GetService<IPooledContext>()
                : scopedProvider2.GetService<PooledContext>();

            await Dispose(serviceScope1, async);
            await Dispose(serviceScope2, async);

            Assert.Throws<ObjectDisposedException>(() => context.Customers.ToList());
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public async Task Pool_disposes_contexts_when_disposed(bool useInterface, bool async)
        {
            var serviceProvider = useInterface
                ? BuildServiceProvider<IPooledContext, PooledContext>()
                : BuildServiceProvider<PooledContext>();

            var serviceScope = serviceProvider.CreateScope();
            var scopedProvider = serviceScope.ServiceProvider;

            var context = useInterface
                ? (PooledContext)scopedProvider.GetService<IPooledContext>()
                : scopedProvider.GetService<PooledContext>();

            await Dispose(serviceScope, async);

            await Dispose((IDisposable)serviceProvider, async);

            Assert.Throws<ObjectDisposedException>(() => context.Customers.ToList());
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public async Task Object_in_pool_is_disposed(bool useInterface, bool async)
        {
            var serviceProvider = useInterface
                ? BuildServiceProvider<IPooledContext, PooledContext>()
                : BuildServiceProvider<PooledContext>();

            var serviceScope = serviceProvider.CreateScope();
            var scopedProvider = serviceScope.ServiceProvider;

            var context = useInterface
                ? (PooledContext)scopedProvider.GetService<IPooledContext>()
                : scopedProvider.GetService<PooledContext>();

            await Dispose(serviceScope, async);

            Assert.Throws<ObjectDisposedException>(() => context.Customers.ToList());
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public async Task Double_dispose_does_not_enter_pool_twice(bool useInterface, bool async)
        {
            var serviceProvider = useInterface
                ? BuildServiceProvider<IPooledContext, PooledContext>()
                : BuildServiceProvider<PooledContext>();

            var scope = serviceProvider.CreateScope();
            var lease = scope.ServiceProvider.GetRequiredService<IScopedDbContextLease<PooledContext>>();
            var context = lease.Context;

            await Dispose(scope, async);
            await Dispose(scope, async);

            using var scope1 = serviceProvider.CreateScope();
            var lease1 = scope1.ServiceProvider.GetRequiredService<IScopedDbContextLease<PooledContext>>();

            using var scope2 = serviceProvider.CreateScope();
            var lease2 = scope2.ServiceProvider.GetRequiredService<IScopedDbContextLease<PooledContext>>();

            Assert.Same(context, lease1.Context);
            Assert.NotSame(lease1.Context, lease2.Context);
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public async Task Double_dispose_with_standalone_lease_does_not_enter_pool_twice(bool useInterface, bool async)
        {
            var serviceProvider = useInterface
                ? BuildServiceProvider<IPooledContext, PooledContext>()
                : BuildServiceProvider<PooledContext>();

            var pool = serviceProvider.GetRequiredService<IDbContextPool<PooledContext>>();
            var lease = new DbContextLease(pool, standalone: true);
            var context = lease.Context;

            await Dispose(context, async);
            await Dispose(context, async);

            using var context1 = new DbContextLease(pool, standalone: true).Context;
            using var context2 = new DbContextLease(pool, standalone: true).Context;

            Assert.Same(context, context1);
            Assert.NotSame(context1, context2);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Can_double_dispose_with_factory(bool async)
        {
            var factory = BuildServiceProviderWithFactory<PooledContext>()
                .GetService<IDbContextFactory<PooledContext>>();

            var context = factory.CreateDbContext();

            context.Customers.Load();

            await Dispose(context, async);

            Assert.Throws<ObjectDisposedException>(() => context.Customers.ToList());

            await Dispose(context, async);

            Assert.Throws<ObjectDisposedException>(() => context.Customers.ToList());
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public async Task Provider_services_are_reset(bool useInterface, bool async)
        {
            var serviceProvider = useInterface
                ? BuildServiceProvider<IPooledContext, PooledContext>()
                : BuildServiceProvider<PooledContext>();

            var serviceScope = serviceProvider.CreateScope();
            var scopedProvider = serviceScope.ServiceProvider;

            var context1 = useInterface
                ? (PooledContext)scopedProvider.GetService<IPooledContext>()
                : scopedProvider.GetService<PooledContext>();

            context1.Database.BeginTransaction();

            Assert.NotNull(context1.Database.CurrentTransaction);

            await Dispose(serviceScope, async);

            serviceScope = serviceProvider.CreateScope();
            scopedProvider = serviceScope.ServiceProvider;

            var context2 = useInterface
                ? (PooledContext)scopedProvider.GetService<IPooledContext>()
                : scopedProvider.GetService<PooledContext>();

            Assert.Same(context1, context2);
            Assert.Null(context2.Database.CurrentTransaction);

            context2.Database.BeginTransaction();

            Assert.NotNull(context2.Database.CurrentTransaction);

            await Dispose(serviceScope, async);

            serviceScope = serviceProvider.CreateScope();
            scopedProvider = serviceScope.ServiceProvider;

            var context3 = useInterface
                ? (PooledContext)scopedProvider.GetService<IPooledContext>()
                : scopedProvider.GetService<PooledContext>();

            Assert.Same(context2, context3);
            Assert.Null(context3.Database.CurrentTransaction);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Provider_services_are_reset_with_factory(bool async)
        {
            var factory = BuildServiceProviderWithFactory<PooledContext>()
                .GetService<IDbContextFactory<PooledContext>>();

            var context1 = factory.CreateDbContext();

            context1.Database.BeginTransaction();

            Assert.NotNull(context1.Database.CurrentTransaction);

            await Dispose(context1, async);

            var context2 = factory.CreateDbContext();

            Assert.Same(context1, context2);
            Assert.Null(context2.Database.CurrentTransaction);

            context2.Database.BeginTransaction();

            Assert.NotNull(context2.Database.CurrentTransaction);

            await Dispose(context2, async);

            var context3 = factory.CreateDbContext();

            Assert.Same(context2, context3);
            Assert.Null(context3.Database.CurrentTransaction);
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        [PlatformSkipCondition(TestPlatform.Linux, SkipReason = "Test is flaky on CI.")]
        public void Double_dispose_concurrency_test(bool useInterface)
        {
            var serviceProvider = useInterface
                ? BuildServiceProvider<IPooledContext, PooledContext>()
                : BuildServiceProvider<PooledContext>();

            Parallel.For(
                fromInclusive: 0, toExclusive: 32, body: s =>
                {
                    using var scope = serviceProvider.CreateScope();
                    var scopedProvider = scope.ServiceProvider;

                    var context = useInterface
                        ? (PooledContext)scopedProvider.GetService<IPooledContext>()
                        : scopedProvider.GetService<PooledContext>();

                    var _ = context.Customers.ToList();

                    context.Dispose();
                });
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [PlatformSkipCondition(TestPlatform.Linux, SkipReason = "Test is flaky on CI.")]
        public async Task Concurrency_test(bool useInterface, bool async)
        {
            PooledContext.InstanceCount = 0;
            PooledContext.DisposedCount = 0;

            var results = WriteResults();

            var serviceProvider = useInterface
                ? BuildServiceProvider<IPooledContext, PooledContext>()
                : BuildServiceProvider<PooledContext>();

            async Task ProcessRequest()
            {
                while (_stopwatch.IsRunning)
                {
                    using var serviceScope = serviceProvider.CreateScope();
                    var scopedProvider = serviceScope.ServiceProvider;

                    var context = useInterface
                        ? (PooledContext)scopedProvider.GetService<IPooledContext>()
                        : scopedProvider.GetService<PooledContext>();

                    await context.Customers.AsNoTracking().FirstAsync(c => c.CustomerId == "ALFKI");

                    Interlocked.Increment(ref _requests);
                }
            }

            var tasks = new Task[32];

            for (var i = 0; i < 32; i++)
            {
                tasks[i] = ProcessRequest();
            }

            await Task.WhenAll(tasks);
            await results;

            Assert.Equal(_requests, PooledContext.DisposedCount);
            Assert.InRange(PooledContext.InstanceCount, low: 32, high: 64);
        }

        private readonly TimeSpan _duration = TimeSpan.FromSeconds(value: 10);

        private int _stopwatchStarted;

        private readonly Stopwatch _stopwatch = new Stopwatch();

        private long _requests;

        private async Task WriteResults()
        {
            if (Interlocked.Exchange(ref _stopwatchStarted, value: 1) == 0)
            {
                _stopwatch.Start();
            }

            var lastRequests = (long)0;
            var lastElapsed = TimeSpan.Zero;

            while (_stopwatch.IsRunning)
            {
                await Task.Delay(TimeSpan.FromSeconds(value: 1));

                var currentRequests = _requests - lastRequests;
                lastRequests = _requests;

                var elapsed = _stopwatch.Elapsed;
                var currentElapsed = elapsed - lastElapsed;
                lastElapsed = elapsed;

                _testOutputHelper?
                    .WriteLine(
                        $"[{DateTime.Now:HH:mm:ss.fff}] Requests: {_requests}, "
                        + $"RPS: {Math.Round(currentRequests / currentElapsed.TotalSeconds)}");

                if (elapsed > _duration)
                {
                    _testOutputHelper?.WriteLine(message: "");
                    _testOutputHelper?.WriteLine($"Average RPS: {Math.Round(_requests / elapsed.TotalSeconds)}");

                    _stopwatch.Stop();
                }
            }
        }

        private async Task Dispose(IDisposable disposable, bool async)
        {
            if (async)
            {
                await ((IAsyncDisposable)disposable).DisposeAsync();
            }
            else
            {
                disposable.Dispose();
            }
        }

        private readonly ITestOutputHelper _testOutputHelper = null;

        // ReSharper disable once UnusedParameter.Local
        public DbContextPoolingTest(NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
        {
            //_testOutputHelper = testOutputHelper;
        }
    }
}
