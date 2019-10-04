// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
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
    public class DbContextPoolingTest
    {
        private static IServiceProvider BuildServiceProvider<TContextService, TContext>(int poolSize = 32)
            where TContextService : class
            where TContext : DbContext, TContextService
            => new ServiceCollection()
                .AddDbContextPool<TContextService, TContext>(
                    ob => ob.UseSqlServer(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString),
                    poolSize)
                .AddDbContextPool<ISecondContext, SecondContext>(
                    ob => ob.UseSqlServer(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString),
                    poolSize)
                .BuildServiceProvider();

        private static IServiceProvider BuildServiceProvider<TContext>(int poolSize = 32)
            where TContext : DbContext
            => new ServiceCollection()
                .AddDbContextPool<TContext>(
                    ob => ob.UseSqlServer(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString),
                    poolSize)
                .AddDbContextPool<SecondContext>(
                    ob => ob.UseSqlServer(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString),
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
                //ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
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

        [Fact]
        public void Invalid_pool_size()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => BuildServiceProvider<PooledContext>(poolSize: 0));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => BuildServiceProvider<PooledContext>(poolSize: -1));
        }

        [Theory]
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

        private class BadCtorContext : DbContext
        {
        }

        [Fact]
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
        }

        [Fact]
        public void Can_pool_non_derived_context()
        {
            var serviceProvider = BuildServiceProvider<DbContext>();

            var serviceScope1 = serviceProvider.CreateScope();

            var context1 = serviceScope1.ServiceProvider.GetService<DbContext>();

            var serviceScope2 = serviceProvider.CreateScope();

            var context2 = serviceScope2.ServiceProvider.GetService<DbContext>();

            Assert.NotSame(context1, context2);

            serviceScope1.Dispose();
            serviceScope2.Dispose();

            var serviceScope3 = serviceProvider.CreateScope();

            var context3 = serviceScope3.ServiceProvider.GetService<DbContext>();

            Assert.Same(context1, context3);

            var serviceScope4 = serviceProvider.CreateScope();

            var context4 = serviceScope4.ServiceProvider.GetService<DbContext>();

            Assert.Same(context2, context4);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Contexts_are_pooled(bool useInterface)
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

            serviceScope1.Dispose();
            serviceScope2.Dispose();

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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Context_configuration_is_reset(bool useInterface)
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
            context1.Database.AutoTransactionsEnabled = true;

            serviceScope.Dispose();

            serviceScope = serviceProvider.CreateScope();
            scopedProvider = serviceScope.ServiceProvider;

            var context2 = useInterface
                ? (DbContext)scopedProvider.GetService<IPooledContext>()
                : scopedProvider.GetService<PooledContext>();

            Assert.Same(context1, context2);

            Assert.False(context2.ChangeTracker.AutoDetectChangesEnabled);
            Assert.False(context2.ChangeTracker.LazyLoadingEnabled);
            Assert.Equal(QueryTrackingBehavior.TrackAll, context2.ChangeTracker.QueryTrackingBehavior);
            Assert.False(context2.Database.AutoTransactionsEnabled);
        }

        [Fact]
        public void Default_Context_configuration__is_reset()
        {
            var serviceProvider = BuildServiceProvider<DefaultOptionsPooledContext>();

            var serviceScope = serviceProvider.CreateScope();
            var scopedProvider = serviceScope.ServiceProvider;

            var context1 = scopedProvider.GetService<DefaultOptionsPooledContext>();

            context1.ChangeTracker.AutoDetectChangesEnabled = false;
            context1.ChangeTracker.LazyLoadingEnabled = false;
            context1.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            context1.Database.AutoTransactionsEnabled = false;

            serviceScope.Dispose();

            serviceScope = serviceProvider.CreateScope();
            scopedProvider = serviceScope.ServiceProvider;

            var context2 = scopedProvider.GetService<DefaultOptionsPooledContext>();

            Assert.Same(context1, context2);

            Assert.True(context2.ChangeTracker.AutoDetectChangesEnabled);
            Assert.True(context2.ChangeTracker.LazyLoadingEnabled);
            Assert.Equal(QueryTrackingBehavior.TrackAll, context2.ChangeTracker.QueryTrackingBehavior);
            Assert.True(context2.Database.AutoTransactionsEnabled);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void State_manager_is_reset(bool useInterface)
        {
            var weakRef = Scoper(
                () =>
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

                    Assert.Equal(expected: 1, actual: context1.ChangeTracker.Entries().Count());

                    serviceScope.Dispose();

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

        private static T Scoper<T>(Func<T> getter) => getter();

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Pool_disposes_context_when_context_not_pooled(bool useInterface)
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

            serviceScope1.Dispose();
            serviceScope2.Dispose();

            Assert.Throws<ObjectDisposedException>(() => context.Customers.ToList());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Pool_disposes_contexts_when_disposed(bool useInterface)
        {
            var serviceProvider = useInterface
                ? BuildServiceProvider<IPooledContext, PooledContext>()
                : BuildServiceProvider<PooledContext>();

            var serviceScope = serviceProvider.CreateScope();
            var scopedProvider = serviceScope.ServiceProvider;

            var context = useInterface
                ? (PooledContext)scopedProvider.GetService<IPooledContext>()
                : scopedProvider.GetService<PooledContext>();

            serviceScope.Dispose();

            ((IDisposable)serviceProvider).Dispose();

            Assert.Throws<ObjectDisposedException>(() => context.Customers.ToList());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Object_in_pool_is_disposed(bool useInterface)
        {
            var serviceProvider = useInterface
                ? BuildServiceProvider<IPooledContext, PooledContext>()
                : BuildServiceProvider<PooledContext>();

            var serviceScope = serviceProvider.CreateScope();
            var scopedProvider = serviceScope.ServiceProvider;

            var context = useInterface
                ? (PooledContext)scopedProvider.GetService<IPooledContext>()
                : scopedProvider.GetService<PooledContext>();

            serviceScope.Dispose();

            Assert.Throws<ObjectDisposedException>(() => context.Customers.ToList());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Double_dispose_does_not_enter_pool_twice(bool useInterface)
        {
            var serviceProvider = useInterface
                ? BuildServiceProvider<IPooledContext, PooledContext>()
                : BuildServiceProvider<PooledContext>();

            var contextPool = serviceProvider.GetService<DbContextPool<PooledContext>>();

            var context = contextPool.Rent();

            context.Dispose();
            context.Dispose();

            var context1 = contextPool.Rent();
            var context2 = contextPool.Rent();

            Assert.NotSame(context1, context2);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Provider_services_are_reset(bool useInterface)
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

            serviceScope.Dispose();

            serviceScope = serviceProvider.CreateScope();
            scopedProvider = serviceScope.ServiceProvider;

            var context2 = useInterface
                ? (PooledContext)scopedProvider.GetService<IPooledContext>()
                : scopedProvider.GetService<PooledContext>();

            Assert.Same(context1, context2);
            Assert.Null(context2.Database.CurrentTransaction);

            context2.Database.BeginTransaction();

            Assert.NotNull(context2.Database.CurrentTransaction);

            serviceScope.Dispose();

            serviceScope = serviceProvider.CreateScope();
            scopedProvider = serviceScope.ServiceProvider;

            var context3 = useInterface
                ? (PooledContext)scopedProvider.GetService<IPooledContext>()
                : scopedProvider.GetService<PooledContext>();

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
                fromInclusive: 0, toExclusive: 100, body: s =>
                {
                    using (var scope = serviceProvider.CreateScope())
                    {
                        var scopedProvider = scope.ServiceProvider;

                        var context = useInterface
                            ? (PooledContext)scopedProvider.GetService<IPooledContext>()
                            : scopedProvider.GetService<PooledContext>();

                        var _ = context.Customers.ToList();

                        context.Dispose();
                    }
                });
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        [PlatformSkipCondition(TestPlatform.Linux, SkipReason = "Test is flaky on CI.")]
        public async Task Concurrency_test(bool useInterface)
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
                    using (var serviceScope = serviceProvider.CreateScope())
                    {
                        var scopedProvider = serviceScope.ServiceProvider;

                        var context = useInterface
                            ? (PooledContext)scopedProvider.GetService<IPooledContext>()
                            : scopedProvider.GetService<PooledContext>();

                        await context.Customers.AsNoTracking().FirstAsync(c => c.CustomerId == "ALFKI");

                        Interlocked.Increment(ref _requests);
                    }
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

        private readonly ITestOutputHelper _testOutputHelper = null;

        // ReSharper disable once UnusedParameter.Local
        public DbContextPoolingTest(ITestOutputHelper testOutputHelper)
        {
            //_testOutputHelper = testOutputHelper;
        }
    }
}
