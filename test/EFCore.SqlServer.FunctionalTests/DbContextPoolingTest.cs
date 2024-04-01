// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Internal;

// ReSharper disable MethodHasAsyncOverload

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable VirtualMemberCallInConstructor
namespace Microsoft.EntityFrameworkCore;

#nullable disable

#pragma warning disable CS9113 // Parameter is unread.
public class DbContextPoolingTest(NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper) : IClassFixture<NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
#pragma warning restore CS9113 // Parameter is unread.
{
    private static DbContextOptionsBuilder<TContext> ConfigureOptions<TContext>(DbContextOptionsBuilder<TContext> optionsBuilder)
        where TContext : DbContext
        => optionsBuilder
            .UseSqlServer(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString)
            .EnableServiceProviderCaching(false);

    private static DbContextOptionsBuilder ConfigureOptions(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder
            .UseSqlServer(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString)
            .EnableServiceProviderCaching(false);

    private static IServiceProvider BuildServiceProvider<TContextService, TContext>(Action<DbContextOptionsBuilder> optionsAction = null)
        where TContextService : class
        where TContext : DbContext, TContextService
        => new ServiceCollection()
            .AddDbContextPool<TContextService, TContext>(
                ob =>
                {
                    var builder = ConfigureOptions(ob);
                    if (optionsAction != null)
                    {
                        optionsAction(builder);
                    }
                })
            .AddDbContextPool<ISecondContext, SecondContext>(
                ob =>
                {
                    var builder = ConfigureOptions(ob);
                    if (optionsAction != null)
                    {
                        optionsAction(builder);
                    }
                })
            .BuildServiceProvider(validateScopes: true);

    private static IServiceProvider BuildServiceProvider<TContext>(Action<DbContextOptionsBuilder> optionsAction = null)
        where TContext : DbContext
        => new ServiceCollection()
            .AddDbContextPool<TContext>(
                ob =>
                {
                    var builder = ConfigureOptions(ob);
                    if (optionsAction != null)
                    {
                        optionsAction(builder);
                    }
                })
            .AddDbContextPool<SecondContext>(
                ob =>
                {
                    var builder = ConfigureOptions(ob);
                    if (optionsAction != null)
                    {
                        optionsAction(builder);
                    }
                })
            .BuildServiceProvider(validateScopes: true);

    private static IServiceProvider BuildServiceProviderWithFactory<TContext>()
        where TContext : DbContext
        => new ServiceCollection()
            .AddPooledDbContextFactory<TContext>(ob => ConfigureOptions(ob))
            .AddDbContextPool<SecondContext>(ob => ConfigureOptions(ob))
            .BuildServiceProvider(validateScopes: true);

    private static IServiceProvider BuildServiceProvider<TContextService, TContext>(int poolSize)
        where TContextService : class
        where TContext : DbContext, TContextService
        => new ServiceCollection()
            .AddDbContextPool<TContextService, TContext>(ob => ConfigureOptions(ob), poolSize)
            .AddDbContextPool<ISecondContext, SecondContext>(ob => ConfigureOptions(ob), poolSize)
            .BuildServiceProvider(validateScopes: true);

    private static IServiceProvider BuildServiceProvider<TContext>(int poolSize)
        where TContext : DbContext
        => new ServiceCollection()
            .AddDbContextPool<TContext>(ob => ConfigureOptions(ob), poolSize)
            .AddDbContextPool<SecondContext>(ob => ConfigureOptions(ob), poolSize)
            .BuildServiceProvider(validateScopes: true);

    private static IServiceProvider BuildServiceProviderWithFactory<TContext>(int poolSize)
        where TContext : DbContext
        => new ServiceCollection()
            .AddPooledDbContextFactory<TContext>(ob => ConfigureOptions(ob), poolSize)
            .AddDbContextPool<SecondContext>(ob => ConfigureOptions(ob), poolSize)
            .BuildServiceProvider(validateScopes: true);

    private static IDbContextFactory<TContext> BuildFactory<TContext>(bool withDependencyInjection)
        where TContext : DbContext
        => withDependencyInjection
            ? BuildServiceProviderWithFactory<TContext>().GetService<IDbContextFactory<TContext>>()
            : new PooledDbContextFactory<TContext>(ConfigureOptions(new DbContextOptionsBuilder<TContext>()).Options);

    private static IDbContextFactory<TContext> BuildFactory<TContext>(bool withDependencyInjection, int poolSize)
        where TContext : DbContext
        => withDependencyInjection
            ? BuildServiceProviderWithFactory<TContext>(poolSize).GetService<IDbContextFactory<TContext>>()
            : new PooledDbContextFactory<TContext>(ConfigureOptions(new DbContextOptionsBuilder<TContext>()).Options, poolSize);

    private interface IPooledContext;

    private class DefaultOptionsPooledContext(DbContextOptions options) : DbContext(options);

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
            Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
            Database.AutoSavepointsEnabled = false;
            ChangeTracker.CascadeDeleteTiming = CascadeTiming.Never;
            ChangeTracker.DeleteOrphansTiming = CascadeTiming.Never;
            SavingChanges += (sender, args) => { };
            SavedChanges += (sender, args) => { };
            SaveChangesFailed += (sender, args) => { };
            ChangeTracker.Tracking += (sender, args) => { };
            ChangeTracker.Tracked += (sender, args) => { };
            ChangeTracker.StateChanging += (sender, args) => { };
            ChangeTracker.StateChanged += (sender, args) => { };
            ChangeTracker.DetectingAllChanges += (sender, args) => { };
            ChangeTracker.DetectedAllChanges += (sender, args) => { };
            ChangeTracker.DetectingEntityChanges += (sender, args) => { };
            ChangeTracker.DetectedEntityChanges += (sender, args) => { };
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
        {
            modelBuilder.Entity<Customer>().ToTable("Customers");
            modelBuilder.Entity<Order>().ToTable("Orders");
        }

        public override void Dispose()
        {
            base.Dispose();

            Interlocked.Increment(ref DisposedCount);
        }
    }

    private class PooledContextWithOverrides(DbContextOptions options) : DbContext(options), IPooledContext
    {
        public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().ToTable("Customers");
            modelBuilder.Entity<Order>().ToTable("Orders");
        }
    }

    public class Customer
    {
        public string CustomerId { get; set; }
        public string CompanyName { get; set; }
        public ILazyLoader LazyLoader { get; set; }
        public ObservableCollection<Order> Orders { get; } = [];
    }

    public class Order
    {
        public int OrderId { get; set; }
        public ILazyLoader LazyLoader { get; set; }
        public string CustomerId { get; set; }
        public Customer Customer { get; set; }
    }

    private interface ISecondContext;

    private class SecondContext(DbContextOptions options) : DbContext(options), ISecondContext
    {
        public DbSet<Blog> Blogs { get; set; }

        public class Blog
        {
            public int Id { get; set; }
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
            () => BuildServiceProvider<IPooledContext, PooledContext>(poolSize: 0));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => BuildServiceProvider<IPooledContext, PooledContext>(poolSize: -1));
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Invalid_pool_size_with_factory(bool withDependencyInjection)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => BuildFactory<PooledContext>(withDependencyInjection, poolSize: 0));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => BuildFactory<PooledContext>(withDependencyInjection, poolSize: -1));
    }

    [ConditionalFact]
    public void Validate_pool_size()
    {
        var serviceProvider = BuildServiceProvider<PooledContext>(poolSize: 64);

        using var scope = serviceProvider.CreateScope();

        Assert.Equal(
            64,
            scope.ServiceProvider
                .GetRequiredService<PooledContext>()
                .GetService<IDbContextOptions>()
                .FindExtension<CoreOptionsExtension>()!.MaxPoolSize);
    }

    [ConditionalFact]
    public void Validate_pool_size_with_service_interface()
    {
        var serviceProvider = BuildServiceProvider<IPooledContext, PooledContext>(poolSize: 64);

        using var scope = serviceProvider.CreateScope();

        Assert.Equal(
            64,
            ((DbContext)scope.ServiceProvider
                .GetRequiredService<IPooledContext>())
            .GetService<IDbContextOptions>()
            .FindExtension<CoreOptionsExtension>()!.MaxPoolSize);
    }

    [ConditionalFact]
    public void Validate_pool_size_with_factory()
    {
        var serviceProvider = BuildServiceProviderWithFactory<PooledContext>(poolSize: 64);

        using var context = serviceProvider.GetRequiredService<IDbContextFactory<PooledContext>>().CreateDbContext();

        Assert.Equal(
            64,
            context.GetService<IDbContextOptions>()
                .FindExtension<CoreOptionsExtension>()!.MaxPoolSize);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Validate_pool_size_behavior_with_factory(bool withDependencyInjection)
    {
        var factory = BuildFactory<PooledContext>(withDependencyInjection, poolSize: 1);

        var (ctx1, ctx2) = (factory.CreateDbContext(), factory.CreateDbContext());
        ctx1.Dispose();
        ctx2.Dispose();

        using var ctx3 = factory.CreateDbContext();
        using var ctx4 = factory.CreateDbContext();
        Assert.Same(ctx1, ctx3);
        Assert.NotSame(ctx2, ctx4);
    }

    [ConditionalFact]
    public void Validate_pool_size_default()
    {
        var serviceProvider = BuildServiceProvider<PooledContext>();

        using var scope = serviceProvider.CreateScope();

        Assert.Equal(
            1024,
            scope.ServiceProvider
                .GetRequiredService<PooledContext>()
                .GetService<IDbContextOptions>()
                .FindExtension<CoreOptionsExtension>()!.MaxPoolSize);
    }

    [ConditionalFact]
    public void Validate_pool_size_with_service_interface_default()
    {
        var serviceProvider = BuildServiceProvider<IPooledContext, PooledContext>();

        using var scope = serviceProvider.CreateScope();

        Assert.Equal(
            1024,
            ((DbContext)scope.ServiceProvider
                .GetRequiredService<IPooledContext>())
            .GetService<IDbContextOptions>()
            .FindExtension<CoreOptionsExtension>()!.MaxPoolSize);
    }

    [ConditionalFact]
    public void Pool_can_get_context_by_concrete_type_even_when_service_interface_is_used()
    {
        var serviceProvider = BuildServiceProvider<IPooledContext, PooledContext>();

        using var scope = serviceProvider.CreateScope();

        Assert.Same(
            scope.ServiceProvider.GetRequiredService<IPooledContext>(),
            scope.ServiceProvider.GetRequiredService<PooledContext>());
    }

    [ConditionalFact]
    public void Validate_pool_size_with_factory_default()
    {
        var serviceProvider = BuildServiceProviderWithFactory<PooledContext>();

        using var context = serviceProvider.GetRequiredService<IDbContextFactory<PooledContext>>().CreateDbContext();

        Assert.Equal(
            1024,
            context.GetService<IDbContextOptions>()
                .FindExtension<CoreOptionsExtension>()!.MaxPoolSize);
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
            Assert.Throws<InvalidOperationException>(() => factory!.CreateDbContext());
        }
        finally
        {
            PooledContext.ModifyOptions = false;
        }
    }

    private class BadCtorContext : DbContext;

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

    [ConditionalFact]
    public void Throws_when_pooled_context_constructor_has_second_parameter_that_cannot_be_resolved_from_service_provider()
    {
        var serviceProvider
            = new ServiceCollection().AddDbContextPool<TwoParameterConstructorContext>(_ => { })
                .BuildServiceProvider(validateScopes: true);

        using var scope = serviceProvider.CreateScope();

        Assert.Throws<InvalidOperationException>(() => scope.ServiceProvider.GetService<TwoParameterConstructorContext>());
    }

    private class TwoParameterConstructorContext(DbContextOptions options, string x) : DbContext(options)
    {
        public string StringParameter { get; } = x;
    }

    [ConditionalFact]
    public void Throws_when_pooled_context_constructor_has_single_parameter_that_cannot_be_resolved_from_service_provider()
    {
        var serviceProvider
            = new ServiceCollection().AddDbContextPool<WrongParameterConstructorContext>(_ => { })
                .BuildServiceProvider(validateScopes: true);

        using var scope = serviceProvider.CreateScope();

        Assert.Throws<InvalidOperationException>(() => scope.ServiceProvider.GetService<WrongParameterConstructorContext>());
    }

#pragma warning disable CS9113 // Parameter 'x' is unread
    private class WrongParameterConstructorContext(string x) : DbContext(new DbContextOptions<WrongParameterConstructorContext>());
#pragma warning restore CS9113

    [ConditionalFact]
    public void Throws_when_pooled_context_constructor_has_scoped_service()
    {
        var serviceProvider
            = new ServiceCollection()
                .AddDbContextPool<TwoParameterConstructorContext>(_ => { })
                .AddScoped(sp => "string")
                .BuildServiceProvider(validateScopes: true);

        using var scope = serviceProvider.CreateScope();

        Assert.Throws<InvalidOperationException>(() => scope.ServiceProvider.GetService<TwoParameterConstructorContext>());
    }

    [ConditionalFact]
    public void Does_not_throw_when_pooled_context_constructor_has_singleton_service()
    {
        var serviceProvider
            = new ServiceCollection()
                .AddDbContextPool<TwoParameterConstructorContext>(_ => { })
                .AddSingleton("string")
                .BuildServiceProvider(validateScopes: true);

        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetService<TwoParameterConstructorContext>();

        Assert.Equal("string", context.StringParameter);
    }

    [ConditionalFact]
    public void Does_not_throw_when_parameterless_and_correct_constructor()
    {
        var serviceProvider
            = new ServiceCollection().AddDbContextPool<WithParameterlessConstructorContext>(_ => { })
                .BuildServiceProvider(validateScopes: true);

        using var scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<WithParameterlessConstructorContext>();

        Assert.Equal("Options", context.ConstructorUsed);
    }

    [ConditionalFact]
    public void Does_not_throw_when_parameterless_and_correct_constructor_using_factory_pool()
    {
        var serviceProvider
            = new ServiceCollection().AddPooledDbContextFactory<WithParameterlessConstructorContext>(_ => { })
                .BuildServiceProvider(validateScopes: true);

        using var scope = serviceProvider.CreateScope();

        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<WithParameterlessConstructorContext>>();
        using var context = factory.CreateDbContext();

        Assert.Equal("Options", context.ConstructorUsed);
    }

    private class WithParameterlessConstructorContext : DbContext
    {
        public string ConstructorUsed { get; }

        public WithParameterlessConstructorContext()
        {
            ConstructorUsed = "Parameterless";
        }

        public WithParameterlessConstructorContext(DbContextOptions<WithParameterlessConstructorContext> options)
            : base(options)
        {
            ConstructorUsed = "Options";
        }
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
        var context1 = await GetContextAsync(serviceScope1);

        var serviceScope2 = serviceProvider.CreateScope();
        var context2 = await GetContextAsync(serviceScope2);

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
        var context3 = await GetContextAsync(serviceScope3);

        var id1r = context3.ContextId;

        Assert.Same(context1, context3);
        Assert.Equal(id1.InstanceId, id1r.InstanceId);
        Assert.NotEqual(default, id1r.InstanceId);
        Assert.NotEqual(id1, id1r);
        Assert.Equal(2, id1r.Lease);

        var serviceScope4 = serviceProvider.CreateScope();
        var context4 = await GetContextAsync(serviceScope4);

        var id2r = context4.ContextId;

        Assert.Same(context2, context4);
        Assert.Equal(id2.InstanceId, id2r.InstanceId);
        Assert.NotEqual(default, id2r.InstanceId);
        Assert.NotEqual(id2, id2r);
        Assert.Equal(2, id2r.Lease);

        async Task<DbContext> GetContextAsync(IServiceScope serviceScope)
            => useFactory
                ? async
                    ? await serviceScope.ServiceProvider.GetService<IDbContextFactory<DbContext>>()!.CreateDbContextAsync()
                    : serviceScope.ServiceProvider.GetService<IDbContextFactory<DbContext>>()!.CreateDbContext()
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
            .BuildServiceProvider(validateScopes: true);

        var serviceScope1 = serviceProvider.CreateScope();
        var context1 = serviceScope1.ServiceProvider.GetService<DbContext>();

        var serviceScope2 = serviceProvider.CreateScope();
        var context2 = serviceScope2.ServiceProvider.GetService<DbContext>();

        Assert.NotSame(context1, context2);

        var id1 = context1!.ContextId;
        var id2 = context2!.ContextId;

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

        await Dispose(serviceScope3, async);

        await Dispose(serviceScope4, async);
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Contexts_are_pooled_with_factory(bool async, bool withDependencyInjection)
    {
        var factory = BuildFactory<PooledContext>(withDependencyInjection);

        var context1 = async ? await factory.CreateDbContextAsync() : factory.CreateDbContext();
        var secondContext1 = async ? await factory.CreateDbContextAsync() : factory.CreateDbContext();

        var context2 = async ? await factory.CreateDbContextAsync() : factory.CreateDbContext();
        var secondContext2 = async ? await factory.CreateDbContextAsync() : factory.CreateDbContext();

        Assert.NotSame(context1, context2);
        Assert.NotSame(secondContext1, secondContext2);

        await Dispose(context1, async);
        await Dispose(secondContext1, async);
        await Dispose(context2, async);
        await Dispose(secondContext2, async);

        var context3 = async ? await factory.CreateDbContextAsync() : factory.CreateDbContext();
        var secondContext3 = async ? await factory.CreateDbContextAsync() : factory.CreateDbContext();

        Assert.Same(context1, context3);
        Assert.Same(secondContext1, secondContext3);

        var context4 = async ? await factory.CreateDbContextAsync() : factory.CreateDbContext();
        var secondContext4 = async ? await factory.CreateDbContextAsync() : factory.CreateDbContext();

        Assert.Same(context2, context4);
        Assert.Same(secondContext2, secondContext4);

        await Dispose(context1, async);
        await Dispose(secondContext1, async);
        await Dispose(context2, async);
        await Dispose(secondContext2, async);
    }

    [ConditionalTheory]
    [InlineData(false, false, null)]
    [InlineData(true, false, null)]
    [InlineData(false, true, null)]
    [InlineData(true, true, null)]
    [InlineData(false, false, QueryTrackingBehavior.TrackAll)]
    [InlineData(true, false, QueryTrackingBehavior.TrackAll)]
    [InlineData(false, true, QueryTrackingBehavior.TrackAll)]
    [InlineData(true, true, QueryTrackingBehavior.TrackAll)]
    [InlineData(false, false, QueryTrackingBehavior.NoTracking)]
    [InlineData(true, false, QueryTrackingBehavior.NoTracking)]
    [InlineData(false, true, QueryTrackingBehavior.NoTracking)]
    [InlineData(true, true, QueryTrackingBehavior.NoTracking)]
    [InlineData(false, false, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(true, false, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(false, true, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(true, true, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public async Task Context_configuration_is_reset(bool useInterface, bool async, QueryTrackingBehavior? queryTrackingBehavior)
    {
        var serviceProvider = useInterface
            ? BuildServiceProvider<IPooledContext, PooledContext>(b => UseQueryTrackingBehavior(b, queryTrackingBehavior))
            : BuildServiceProvider<PooledContext>(b => UseQueryTrackingBehavior(b, queryTrackingBehavior));

        var serviceScope = serviceProvider.CreateScope();
        var scopedProvider = serviceScope.ServiceProvider;

        var context1 = useInterface
            ? (PooledContext)scopedProvider.GetService<IPooledContext>()
            : scopedProvider.GetService<PooledContext>();

        Assert.Null(context1!.Database.GetCommandTimeout());

        var set = context1.Customers;
        var localView = set.Local;
        localView.PropertyChanged += LocalView_OnPropertyChanged;
        localView.PropertyChanging += LocalView_OnPropertyChanging;
        localView.CollectionChanged += LocalView_OnCollectionChanged;
        var customer1 = new Customer { CustomerId = "C" };
        context1.Customers.Attach(customer1);
        Assert.Equal(1, localView.Count);
        Assert.Same(customer1, localView.ToBindingList().Single());
        Assert.Same(customer1, localView.ToObservableCollection().Single());
        Assert.True(_localView_OnPropertyChanging);
        Assert.True(_localView_OnPropertyChanged);
        Assert.True(_localView_OnCollectionChanged);

        context1.ChangeTracker.AutoDetectChangesEnabled = true;
        context1.ChangeTracker.LazyLoadingEnabled = true;
        context1.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        context1.ChangeTracker.CascadeDeleteTiming = CascadeTiming.Immediate;
        context1.ChangeTracker.DeleteOrphansTiming = CascadeTiming.Immediate;
        context1.Database.AutoTransactionBehavior = AutoTransactionBehavior.WhenNeeded;
        context1.Database.AutoSavepointsEnabled = true;
        context1.Database.SetCommandTimeout(1);
        context1.ChangeTracker.Tracking += ChangeTracker_OnTracking;
        context1.ChangeTracker.Tracked += ChangeTracker_OnTracked;
        context1.ChangeTracker.StateChanging += ChangeTracker_OnStateChanging;
        context1.ChangeTracker.StateChanged += ChangeTracker_OnStateChanged;
        context1.ChangeTracker.DetectingAllChanges += ChangeTracker_OnDetectingAllChanges;
        context1.ChangeTracker.DetectedAllChanges += ChangeTracker_OnDetectedAllChanges;
        context1.ChangeTracker.DetectingEntityChanges += ChangeTracker_OnDetectingEntityChanges;
        context1.ChangeTracker.DetectedEntityChanges += ChangeTracker_OnDetectedEntityChanges;
        context1.SavingChanges += Context_OnSavingChanges;
        context1.SavedChanges += Context_OnSavedChanges;
        context1.SaveChangesFailed += Context_OnSaveChangesFailed;

        _localView_OnPropertyChanging = false;
        _localView_OnPropertyChanged = false;
        _localView_OnCollectionChanged = false;

        await Dispose(serviceScope, async);

        serviceScope = serviceProvider.CreateScope();
        scopedProvider = serviceScope.ServiceProvider;

        var context2 = useInterface
            ? (PooledContext)scopedProvider.GetService<IPooledContext>()
            : scopedProvider.GetService<PooledContext>();

        Assert.Same(context1, context2);

        Assert.False(context2!.ChangeTracker.AutoDetectChangesEnabled);
        Assert.False(context2.ChangeTracker.LazyLoadingEnabled);
        Assert.Equal(queryTrackingBehavior ?? QueryTrackingBehavior.TrackAll, context2.ChangeTracker.QueryTrackingBehavior);
        Assert.Equal(CascadeTiming.Never, context2.ChangeTracker.CascadeDeleteTiming);
        Assert.Equal(CascadeTiming.Never, context2.ChangeTracker.DeleteOrphansTiming);
        Assert.Equal(AutoTransactionBehavior.Never, context2.Database.AutoTransactionBehavior);
        Assert.False(context2.Database.AutoSavepointsEnabled);
        Assert.Null(context1.Database.GetCommandTimeout());

        Assert.Empty(localView);
        Assert.Empty(localView.ToBindingList());
        Assert.Empty(localView.ToObservableCollection());

        var customer2 = new Customer { CustomerId = "C" };
        context2.Customers.Attach(customer2).State = EntityState.Modified;
        context2.Customers.Attach(customer2).State = EntityState.Unchanged;

        Assert.False(_changeTracker_OnTracking);
        Assert.False(_changeTracker_OnTracked);
        Assert.False(_changeTracker_OnStateChanging);
        Assert.False(_changeTracker_OnStateChanged);

        context2.SaveChanges();

        Assert.False(_changeTracker_OnDetectingAllChanges);
        Assert.False(_changeTracker_OnDetectedAllChanges);
        Assert.False(_changeTracker_OnDetectingEntityChanges);
        Assert.False(_changeTracker_OnDetectedEntityChanges);
        Assert.False(_context_OnSavedChanges);
        Assert.False(_context_OnSavingChanges);
        Assert.False(_context_OnSaveChangesFailed);

        Assert.Same(set, context2!.Customers);
        Assert.Same(localView, context2!.Customers.Local);
        Assert.Equal(1, localView.Count);
        Assert.Same(customer2, localView.ToBindingList().Single());
        Assert.Same(customer2, localView.ToObservableCollection().Single());
        Assert.False(_localView_OnPropertyChanging);
        Assert.False(_localView_OnPropertyChanged);
        Assert.False(_localView_OnCollectionChanged);
    }

    [ConditionalTheory]
    [InlineData(false, null)]
    [InlineData(true, null)]
    [InlineData(false, QueryTrackingBehavior.TrackAll)]
    [InlineData(true, QueryTrackingBehavior.TrackAll)]
    [InlineData(false, QueryTrackingBehavior.NoTracking)]
    [InlineData(true, QueryTrackingBehavior.NoTracking)]
    [InlineData(false, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(true, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public async Task Uninitialized_context_configuration_is_reset_properly(bool async, QueryTrackingBehavior? queryTrackingBehavior)
    {
        var serviceProvider = BuildServiceProvider<SecondContext>(b => UseQueryTrackingBehavior(b, queryTrackingBehavior));

        var serviceScope = serviceProvider.CreateScope();
        var ctx = serviceScope.ServiceProvider.GetRequiredService<SecondContext>();
        await Dispose(ctx, async);
        await Dispose(serviceScope, async);

        serviceScope = serviceProvider.CreateScope();
        var ctx2 = serviceScope.ServiceProvider.GetRequiredService<SecondContext>();
        Assert.Same(ctx, ctx2);
        ctx2.Blogs.Add(new SecondContext.Blog());
        await Dispose(ctx2, async);
        await Dispose(serviceScope, async);

        serviceScope = serviceProvider.CreateScope();
        var ctx3 = serviceScope.ServiceProvider.GetRequiredService<SecondContext>();
        Assert.Same(ctx, ctx3);
        Assert.Empty(ctx3.ChangeTracker.Entries());
        await Dispose(ctx2, async);
        await Dispose(serviceScope, async);
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Context_configuration_is_reset_with_factory(bool async, bool withDependencyInjection)
    {
        var factory = BuildFactory<PooledContext>(withDependencyInjection);

        var context1 = async ? await factory.CreateDbContextAsync() : factory.CreateDbContext();
        var set = context1.Customers;

        var localView = set.Local;
        localView.PropertyChanged += LocalView_OnPropertyChanged;
        localView.PropertyChanging += LocalView_OnPropertyChanging;
        localView.CollectionChanged += LocalView_OnCollectionChanged;
        var customer1 = new Customer { CustomerId = "C" };
        context1.Customers.Attach(customer1);
        Assert.Equal(1, localView.Count);
        Assert.Same(customer1, localView.ToBindingList().Single());
        Assert.Same(customer1, localView.ToObservableCollection().Single());
        Assert.True(_localView_OnPropertyChanging);
        Assert.True(_localView_OnPropertyChanged);
        Assert.True(_localView_OnCollectionChanged);

        context1.ChangeTracker.AutoDetectChangesEnabled = true;
        context1.ChangeTracker.LazyLoadingEnabled = true;
        context1.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        context1.ChangeTracker.CascadeDeleteTiming = CascadeTiming.Immediate;
        context1.ChangeTracker.DeleteOrphansTiming = CascadeTiming.Immediate;
        context1.Database.AutoTransactionBehavior = AutoTransactionBehavior.WhenNeeded;
        context1.Database.AutoSavepointsEnabled = true;
        context1.ChangeTracker.Tracking += ChangeTracker_OnTracking;
        context1.ChangeTracker.Tracked += ChangeTracker_OnTracked;
        context1.ChangeTracker.StateChanging += ChangeTracker_OnStateChanging;
        context1.ChangeTracker.StateChanged += ChangeTracker_OnStateChanged;
        context1.ChangeTracker.DetectingAllChanges += ChangeTracker_OnDetectingAllChanges;
        context1.ChangeTracker.DetectedAllChanges += ChangeTracker_OnDetectedAllChanges;
        context1.ChangeTracker.DetectingEntityChanges += ChangeTracker_OnDetectingEntityChanges;
        context1.ChangeTracker.DetectedEntityChanges += ChangeTracker_OnDetectedEntityChanges;
        context1.SavingChanges += Context_OnSavingChanges;
        context1.SavedChanges += Context_OnSavedChanges;
        context1.SaveChangesFailed += Context_OnSaveChangesFailed;

        _localView_OnPropertyChanging = false;
        _localView_OnPropertyChanged = false;
        _localView_OnCollectionChanged = false;

        await Dispose(context1, async);

        var context2 = async ? await factory.CreateDbContextAsync() : factory.CreateDbContext();

        Assert.Same(context1, context2);

        Assert.Empty(localView);
        Assert.Empty(localView.ToBindingList());
        Assert.Empty(localView.ToObservableCollection());

        var customer2 = new Customer { CustomerId = "C" };
        context2.Customers.Attach(customer2).State = EntityState.Modified;
        context2.Customers.Attach(customer2).State = EntityState.Unchanged;

        Assert.False(_changeTracker_OnTracking);
        Assert.False(_changeTracker_OnTracked);
        Assert.False(_changeTracker_OnStateChanging);
        Assert.False(_changeTracker_OnStateChanged);

        context2.SaveChanges();

        Assert.False(_changeTracker_OnDetectingAllChanges);
        Assert.False(_changeTracker_OnDetectedAllChanges);
        Assert.False(_changeTracker_OnDetectingEntityChanges);
        Assert.False(_changeTracker_OnDetectedEntityChanges);
        Assert.False(_context_OnSavedChanges);
        Assert.False(_context_OnSavingChanges);
        Assert.False(_context_OnSaveChangesFailed);

        Assert.Same(set, context2!.Customers);
        Assert.Same(localView, context2!.Customers.Local);
        Assert.Equal(1, localView.Count);
        Assert.Same(customer2, localView.ToBindingList().Single());
        Assert.Same(customer2, localView.ToObservableCollection().Single());
        Assert.False(_localView_OnPropertyChanging);
        Assert.False(_localView_OnPropertyChanged);
        Assert.False(_localView_OnCollectionChanged);
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
        context.Database.AutoTransactionBehavior = AutoTransactionBehavior.WhenNeeded;
        context.Database.AutoSavepointsEnabled = true;
        context.ChangeTracker.Tracking += ChangeTracker_OnTracking;
        context.ChangeTracker.Tracked += ChangeTracker_OnTracked;
        context.ChangeTracker.StateChanging += ChangeTracker_OnStateChanging;
        context.ChangeTracker.StateChanged += ChangeTracker_OnStateChanged;
        context.ChangeTracker.DetectingAllChanges += ChangeTracker_OnDetectingAllChanges;
        context.ChangeTracker.DetectedAllChanges += ChangeTracker_OnDetectedAllChanges;
        context.ChangeTracker.DetectingEntityChanges += ChangeTracker_OnDetectingEntityChanges;
        context.ChangeTracker.DetectedEntityChanges += ChangeTracker_OnDetectedEntityChanges;
        context.SavingChanges += Context_OnSavingChanges;
        context.SavedChanges += Context_OnSavedChanges;
        context.SaveChangesFailed += Context_OnSaveChangesFailed;

        context.ChangeTracker.Clear();

        Assert.True(context.ChangeTracker.AutoDetectChangesEnabled);
        Assert.True(context.ChangeTracker.LazyLoadingEnabled);
        Assert.Equal(QueryTrackingBehavior.NoTracking, context.ChangeTracker.QueryTrackingBehavior);
        Assert.Equal(CascadeTiming.Immediate, context.ChangeTracker.CascadeDeleteTiming);
        Assert.Equal(CascadeTiming.Immediate, context.ChangeTracker.DeleteOrphansTiming);
        Assert.Equal(AutoTransactionBehavior.WhenNeeded, context.Database.AutoTransactionBehavior);
        Assert.True(context.Database.AutoSavepointsEnabled);

        Assert.False(_changeTracker_OnTracking);
        Assert.False(_changeTracker_OnTracked);
        Assert.False(_changeTracker_OnStateChanging);
        Assert.False(_changeTracker_OnStateChanged);
        Assert.False(_changeTracker_OnDetectingAllChanges);
        Assert.False(_changeTracker_OnDetectedAllChanges);
        Assert.False(_changeTracker_OnDetectingEntityChanges);
        Assert.False(_changeTracker_OnDetectedEntityChanges);

        var customer = new Customer { CustomerId = "C" };
        context.Customers.Attach(customer).State = EntityState.Modified;
        context.Customers.Attach(customer).State = EntityState.Unchanged;

        Assert.True(_changeTracker_OnTracking);
        Assert.True(_changeTracker_OnTracked);
        Assert.True(_changeTracker_OnStateChanging);
        Assert.True(_changeTracker_OnStateChanged);
        Assert.False(_changeTracker_OnDetectingAllChanges);
        Assert.False(_changeTracker_OnDetectedAllChanges);
        Assert.False(_changeTracker_OnDetectingEntityChanges);
        Assert.False(_changeTracker_OnDetectedEntityChanges);

        context.SaveChanges();

        Assert.True(_changeTracker_OnDetectingAllChanges);
        Assert.True(_changeTracker_OnDetectedAllChanges);
        Assert.True(_changeTracker_OnDetectingEntityChanges);
        Assert.True(_changeTracker_OnDetectedEntityChanges);
        Assert.True(_context_OnSavedChanges);
        Assert.True(_context_OnSavingChanges);
        Assert.False(_context_OnSaveChangesFailed);
    }

    private void Context_OnSavingChanges(object sender, SavingChangesEventArgs e)
        => _context_OnSavingChanges = true;

    private bool _context_OnSavingChanges;

    private void Context_OnSavedChanges(object sender, SavedChangesEventArgs e)
        => _context_OnSavedChanges = true;

    private bool _context_OnSavedChanges;

    private void Context_OnSaveChangesFailed(object sender, SaveChangesFailedEventArgs e)
        => _context_OnSaveChangesFailed = true;

    private bool _context_OnSaveChangesFailed;

    private bool _localView_OnPropertyChanged;

    private void LocalView_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        => _localView_OnPropertyChanged = true;

    private bool _localView_OnPropertyChanging;

    private void LocalView_OnPropertyChanging(object sender, PropertyChangingEventArgs e)
        => _localView_OnPropertyChanging = true;

    private bool _localView_OnCollectionChanged;

    private void LocalView_OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        => _localView_OnCollectionChanged = true;

    private bool _changeTracker_OnTracking;

    private void ChangeTracker_OnTracking(object sender, EntityTrackingEventArgs e)
        => _changeTracker_OnTracking = true;

    private bool _changeTracker_OnTracked;

    private void ChangeTracker_OnTracked(object sender, EntityTrackedEventArgs e)
        => _changeTracker_OnTracked = true;

    private bool _changeTracker_OnStateChanging;

    private void ChangeTracker_OnStateChanging(object sender, EntityStateChangingEventArgs e)
        => _changeTracker_OnStateChanging = true;

    private bool _changeTracker_OnStateChanged;

    private void ChangeTracker_OnStateChanged(object sender, EntityStateChangedEventArgs e)
        => _changeTracker_OnStateChanged = true;

    private bool _changeTracker_OnDetectingAllChanges;

    private void ChangeTracker_OnDetectingAllChanges(object sender, DetectChangesEventArgs e)
        => _changeTracker_OnDetectingAllChanges = true;

    private bool _changeTracker_OnDetectedAllChanges;

    private void ChangeTracker_OnDetectedAllChanges(object sender, DetectedChangesEventArgs e)
        => _changeTracker_OnDetectedAllChanges = true;

    private bool _changeTracker_OnDetectingEntityChanges;

    private void ChangeTracker_OnDetectingEntityChanges(object sender, DetectEntityChangesEventArgs e)
        => _changeTracker_OnDetectingEntityChanges = true;

    private bool _changeTracker_OnDetectedEntityChanges;

    private void ChangeTracker_OnDetectedEntityChanges(object sender, DetectedEntityChangesEventArgs e)
        => _changeTracker_OnDetectedEntityChanges = true;

    [ConditionalTheory]
    [InlineData(false, null)]
    [InlineData(true, null)]
    [InlineData(false, QueryTrackingBehavior.TrackAll)]
    [InlineData(true, QueryTrackingBehavior.TrackAll)]
    [InlineData(false, QueryTrackingBehavior.NoTracking)]
    [InlineData(true, QueryTrackingBehavior.NoTracking)]
    [InlineData(false, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(true, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public async Task Default_Context_configuration_is_reset(bool async, QueryTrackingBehavior? queryTrackingBehavior)
    {
        var serviceProvider = BuildServiceProvider<DefaultOptionsPooledContext>(b => UseQueryTrackingBehavior(b, queryTrackingBehavior));

        var serviceScope = serviceProvider.CreateScope();
        var scopedProvider = serviceScope.ServiceProvider;

        var context1 = scopedProvider.GetService<DefaultOptionsPooledContext>();

        context1!.ChangeTracker.AutoDetectChangesEnabled = false;
        context1.ChangeTracker.LazyLoadingEnabled = false;
        context1.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        context1.Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
        context1.Database.AutoSavepointsEnabled = false;
        context1.ChangeTracker.CascadeDeleteTiming = CascadeTiming.Immediate;
        context1.ChangeTracker.DeleteOrphansTiming = CascadeTiming.Immediate;

        await Dispose(serviceScope, async);

        serviceScope = serviceProvider.CreateScope();
        scopedProvider = serviceScope.ServiceProvider;

        var context2 = scopedProvider.GetService<DefaultOptionsPooledContext>();

        Assert.Same(context1, context2);

        Assert.True(context2!.ChangeTracker.AutoDetectChangesEnabled);
        Assert.True(context2.ChangeTracker.LazyLoadingEnabled);
        Assert.Equal(queryTrackingBehavior ?? QueryTrackingBehavior.TrackAll, context2.ChangeTracker.QueryTrackingBehavior);
        Assert.Equal(CascadeTiming.Immediate, context2.ChangeTracker.CascadeDeleteTiming);
        Assert.Equal(CascadeTiming.Immediate, context2.ChangeTracker.DeleteOrphansTiming);
        Assert.Equal(AutoTransactionBehavior.WhenNeeded, context2.Database.AutoTransactionBehavior);
        Assert.True(context2.Database.AutoSavepointsEnabled);
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Default_Context_configuration_is_reset_with_factory(bool async, bool withDependencyInjection)
    {
        var factory = BuildFactory<DefaultOptionsPooledContext>(withDependencyInjection);

        var context1 = async ? await factory.CreateDbContextAsync() : factory.CreateDbContext();

        context1.ChangeTracker.AutoDetectChangesEnabled = false;
        context1.ChangeTracker.LazyLoadingEnabled = false;
        context1.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        context1.Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
        context1.Database.AutoSavepointsEnabled = false;
        context1.ChangeTracker.CascadeDeleteTiming = CascadeTiming.Immediate;
        context1.ChangeTracker.DeleteOrphansTiming = CascadeTiming.Immediate;

        await Dispose(context1, async);

        var context2 = async ? await factory.CreateDbContextAsync() : factory.CreateDbContext();

        Assert.Same(context1, context2);

        Assert.True(context2.ChangeTracker.AutoDetectChangesEnabled);
        Assert.True(context2.ChangeTracker.LazyLoadingEnabled);
        Assert.Equal(QueryTrackingBehavior.TrackAll, context2.ChangeTracker.QueryTrackingBehavior);
        Assert.Equal(CascadeTiming.Immediate, context2.ChangeTracker.CascadeDeleteTiming);
        Assert.Equal(CascadeTiming.Immediate, context2.ChangeTracker.DeleteOrphansTiming);
        Assert.Equal(AutoTransactionBehavior.WhenNeeded, context2.Database.AutoTransactionBehavior);
        Assert.True(context2.Database.AutoSavepointsEnabled);
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
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task State_manager_is_reset_with_factory(bool async, bool withDependencyInjection)
    {
        var weakRef = await Scoper(
            async () =>
            {
                var factory = BuildFactory<PooledContext>(withDependencyInjection);

                var context1 = async ? await factory.CreateDbContextAsync() : factory.CreateDbContext();

                var entity = context1.Customers.First(c => c.CustomerId == "ALFKI");

                Assert.Single(context1.ChangeTracker.Entries());

                await Dispose(context1, async);

                var context2 = async ? await factory.CreateDbContextAsync() : factory.CreateDbContext();

                Assert.Same(context1, context2);
                Assert.Empty(context2.ChangeTracker.Entries());

                return new WeakReference(entity);
            });

        GC.Collect();

        Assert.False(weakRef.IsAlive);
    }

    [ConditionalTheory] // Issue #25486
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    public async Task Service_properties_are_disposed(bool useInterface, bool async, bool load)
    {
        var serviceProvider = useInterface
            ? BuildServiceProvider<IPooledContext, PooledContext>()
            : BuildServiceProvider<PooledContext>();

        var serviceScope = serviceProvider.CreateScope();
        var scopedProvider = serviceScope.ServiceProvider;

        var context1 = useInterface
            ? (PooledContext)scopedProvider.GetService<IPooledContext>()
            : scopedProvider.GetService<PooledContext>();

        context1.ChangeTracker.LazyLoadingEnabled = true;

        var entity = context1.Customers.First(c => c.CustomerId == "ALFKI");
        var orderLoader = entity.LazyLoader;
        if (load)
        {
            orderLoader.Load(entity, nameof(Customer.Orders));
            Assert.True(orderLoader.IsLoaded(entity, nameof(Customer.Orders)));
        }

        Assert.Equal(load ? 7 : 1, context1.ChangeTracker.Entries().Count());

        await Dispose(serviceScope, async);

        if (load)
        {
            orderLoader.Load(entity, nameof(Customer.Orders));
            Assert.True(orderLoader.IsLoaded(entity, nameof(Customer.Orders)));
            orderLoader.SetLoaded(entity, nameof(Customer.Orders), loaded: false);
        }

        AssertDisposed(() => orderLoader.Load(entity, nameof(Customer.Orders)), "Customer", "Orders");
    }

    [ConditionalTheory] // Issue #25486
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    public async Task Service_properties_are_disposed_with_factory(bool async, bool withDependencyInjection, bool load)
    {
        var factory = BuildFactory<PooledContext>(withDependencyInjection);

        var context1 = async ? await factory.CreateDbContextAsync() : factory.CreateDbContext();

        context1.ChangeTracker.LazyLoadingEnabled = true;

        var entity = context1.Customers.First(c => c.CustomerId == "ALFKI");
        var orderLoader = entity.LazyLoader;
        if (load)
        {
            orderLoader.Load(entity, nameof(Customer.Orders));
            Assert.True(orderLoader.IsLoaded(entity, nameof(Customer.Orders)));
        }

        Assert.Equal(load ? 7 : 1, context1.ChangeTracker.Entries().Count());

        await Dispose(context1, async);

        if (load)
        {
            orderLoader.Load(entity, nameof(Customer.Orders));
            Assert.True(orderLoader.IsLoaded(entity, nameof(Customer.Orders)));
            orderLoader.SetLoaded(entity, nameof(Customer.Orders), loaded: false);
        }

        AssertDisposed(() => orderLoader.Load(entity, nameof(Customer.Orders)), "Customer", "Orders");
    }

    private static void AssertDisposed(Action testCode, string entityTypeName, string navigationName)
        => Assert.Equal(
            CoreStrings.WarningAsErrorTemplate(
                CoreEventId.LazyLoadOnDisposedContextWarning.ToString(),
                CoreResources.LogLazyLoadOnDisposedContext(new TestLogger<TestLoggingDefinitions>())
                    .GenerateMessage(entityTypeName, navigationName),
                "CoreEventId.LazyLoadOnDisposedContextWarning"),
            Assert.Throws<InvalidOperationException>(
                testCode).Message);

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

        var context1 = useInterface
            ? (PooledContext)scopedProvider1.GetService<IPooledContext>()
            : scopedProvider1.GetService<PooledContext>();

        var serviceScope2 = serviceProvider.CreateScope();
        var scopedProvider2 = serviceScope2.ServiceProvider;

        var context2 = useInterface
            ? (PooledContext)scopedProvider2.GetService<IPooledContext>()
            : scopedProvider2.GetService<PooledContext>();

        await Dispose(serviceScope1, async);
        await Dispose(serviceScope2, async);

        Assert.Throws<ObjectDisposedException>(() => context1.Customers.ToList());
        Assert.Throws<ObjectDisposedException>(() => context2.Customers.ToList());
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

        Assert.Throws<ObjectDisposedException>(() => context!.Customers.ToList());
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
        var context = (PooledContext)lease.Context;
        ((IDbContextPoolable)context).SetLease(lease);

        await Dispose(context, async);

        await Dispose(context, async);

        using var context1 = new DbContextLease(pool, standalone: true).Context;
        using var context2 = new DbContextLease(pool, standalone: true).Context;

        Assert.Same(context, context1);
        Assert.NotSame(context1, context2);
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Can_double_dispose_with_factory(bool async, bool withDependencyInjection)
    {
        var factory = BuildFactory<PooledContext>(withDependencyInjection);

        var context = async ? await factory.CreateDbContextAsync() : factory.CreateDbContext();

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
            : BuildServiceProvider<PooledContext>(o => o.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

        var serviceScope = serviceProvider.CreateScope();
        var scopedProvider = serviceScope.ServiceProvider;

        var context1 = useInterface
            ? (PooledContext)scopedProvider.GetService<IPooledContext>()
            : scopedProvider.GetService<PooledContext>();

        context1!.Database.BeginTransaction();

        Assert.NotNull(context1.Database.CurrentTransaction);

        await Dispose(serviceScope, async);

        serviceScope = serviceProvider.CreateScope();
        scopedProvider = serviceScope.ServiceProvider;

        var context2 = useInterface
            ? (PooledContext)scopedProvider.GetService<IPooledContext>()
            : scopedProvider.GetService<PooledContext>();

        Assert.Same(context1, context2);
        Assert.Null(context2!.Database.CurrentTransaction);

        context2.Database.BeginTransaction();

        Assert.NotNull(context2.Database.CurrentTransaction);

        await Dispose(serviceScope, async);

        serviceScope = serviceProvider.CreateScope();
        scopedProvider = serviceScope.ServiceProvider;

        var context3 = useInterface
            ? (PooledContext)scopedProvider.GetService<IPooledContext>()
            : scopedProvider.GetService<PooledContext>();

        Assert.Same(context2, context3);
        Assert.Null(context3!.Database.CurrentTransaction);
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Provider_services_are_reset_with_factory(bool async, bool withDependencyInjection)
    {
        var factory = BuildFactory<PooledContext>(withDependencyInjection);

        var context1 = async ? await factory.CreateDbContextAsync() : factory.CreateDbContext();

        context1.Database.BeginTransaction();

        Assert.NotNull(context1.Database.CurrentTransaction);

        await Dispose(context1, async);

        var context2 = async ? await factory.CreateDbContextAsync() : factory.CreateDbContext();

        Assert.Same(context1, context2);
        Assert.Null(context2.Database.CurrentTransaction);

        context2.Database.BeginTransaction();

        Assert.NotNull(context2.Database.CurrentTransaction);

        await Dispose(context2, async);

        var context3 = async ? await factory.CreateDbContextAsync() : factory.CreateDbContext();

        Assert.Same(context2, context3);
        Assert.Null(context3.Database.CurrentTransaction);
    }

    [ConditionalTheory] // Issue #27308.
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Handle_open_connection_when_returning_to_pool_for_owned_connection(bool async, bool openWithEf)
    {
        var serviceProvider = new ServiceCollection()
            .AddDbContextPool<PooledContext>(
                ob => ob.UseSqlServer(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString)
                    .EnableServiceProviderCaching(false))
            .BuildServiceProvider(validateScopes: true);

        var serviceScope = serviceProvider.CreateScope();
        var scopedProvider = serviceScope.ServiceProvider;

        var context1 = scopedProvider.GetRequiredService<PooledContext>();
        var connection1 = context1.Database.GetDbConnection();

        if (async)
        {
            if (openWithEf)
            {
                await context1.Database.OpenConnectionAsync();
            }
            else
            {
                await connection1.OpenAsync();
            }
        }
        else
        {
            if (openWithEf)
            {
                context1.Database.OpenConnection();
            }
            else
            {
                connection1.Open();
            }
        }

        Assert.Equal(ConnectionState.Open, connection1.State);

        await Dispose(serviceScope, async);

        Assert.Equal(ConnectionState.Closed, connection1.State);

        serviceScope = serviceProvider.CreateScope();
        scopedProvider = serviceScope.ServiceProvider;

        var context2 = scopedProvider.GetRequiredService<PooledContext>();
        Assert.Same(context1, context2);

        var connection2 = context2.Database.GetDbConnection();
        Assert.Same(connection1, connection2);

        Assert.Equal(ConnectionState.Closed, connection1.State);

        await Dispose(serviceScope, async);
    }

    [ConditionalTheory] // Issue #27308.
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, false, true)]
    public async Task Handle_open_connection_when_returning_to_pool_for_external_connection(bool async, bool startsOpen, bool openWithEf)
    {
        using var connection = new SqlConnection(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString);

        if (startsOpen)
        {
            if (async)
            {
                await connection.OpenAsync();
            }
            else
            {
                connection.Open();
            }
        }

        var serviceProvider = new ServiceCollection()
            .AddDbContextPool<PooledContext>(
                ob => ob.UseSqlServer(connection)
                    .EnableServiceProviderCaching(false))
            .BuildServiceProvider(validateScopes: true);

        var serviceScope = serviceProvider.CreateScope();
        var scopedProvider = serviceScope.ServiceProvider;

        var context1 = scopedProvider.GetRequiredService<PooledContext>();
        Assert.Same(connection, context1.Database.GetDbConnection());

        if (!startsOpen)
        {
            if (async)
            {
                if (openWithEf)
                {
                    await context1.Database.OpenConnectionAsync();
                }
                else
                {
                    await connection.OpenAsync();
                }
            }
            else
            {
                if (openWithEf)
                {
                    context1.Database.OpenConnection();
                }
                else
                {
                    connection.Open();
                }
            }
        }

        Assert.Equal(ConnectionState.Open, connection.State);

        await Dispose(serviceScope, async);

        Assert.Equal(ConnectionState.Open, connection.State);

        serviceScope = serviceProvider.CreateScope();
        scopedProvider = serviceScope.ServiceProvider;

        var context2 = scopedProvider.GetRequiredService<PooledContext>();
        Assert.Same(context1, context2);

        Assert.Same(connection, context2.Database.GetDbConnection());

        Assert.Equal(ConnectionState.Open, connection.State);

        await Dispose(serviceScope, async);
    }

    [ConditionalTheory] // Issue #27308.
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    public async Task Handle_open_connection_when_returning_to_pool_for_owned_connection_with_factory(
        bool async,
        bool openWithEf,
        bool withDependencyInjection)
    {
        var options = new DbContextOptionsBuilder<PooledContext>()
            .UseSqlServer(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString)
            .EnableServiceProviderCaching(false)
            .Options;

        var factory =
            withDependencyInjection
                ? new ServiceCollection()
                    .AddPooledDbContextFactory<PooledContext>(
                        ob => ob.UseSqlServer(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString)
                            .EnableServiceProviderCaching(false))
                    .BuildServiceProvider(validateScopes: true)
                    .GetRequiredService<IDbContextFactory<PooledContext>>()
                : new PooledDbContextFactory<PooledContext>(options);

        var context1 = async ? await factory.CreateDbContextAsync() : factory.CreateDbContext();
        var connection1 = context1.Database.GetDbConnection();

        if (async)
        {
            if (openWithEf)
            {
                await context1.Database.OpenConnectionAsync();
            }
            else
            {
                await connection1.OpenAsync();
            }
        }
        else
        {
            if (openWithEf)
            {
                context1.Database.OpenConnection();
            }
            else
            {
                connection1.Open();
            }
        }

        Assert.Equal(ConnectionState.Open, connection1.State);

        await Dispose(context1, async);

        Assert.Equal(ConnectionState.Closed, connection1.State);

        var context2 = async ? await factory.CreateDbContextAsync() : factory.CreateDbContext();
        Assert.Same(context1, context2);

        var connection2 = context2.Database.GetDbConnection();
        Assert.Same(connection1, connection2);

        Assert.Equal(ConnectionState.Closed, connection1.State);

        await Dispose(context2, async);
    }

    [ConditionalTheory] // Issue #27308.
    [InlineData(false, false, false, false)]
    [InlineData(true, false, false, false)]
    [InlineData(false, true, false, false)]
    [InlineData(true, true, false, false)]
    [InlineData(false, false, true, false)]
    [InlineData(true, false, true, false)]
    [InlineData(false, false, false, true)]
    [InlineData(true, false, false, true)]
    [InlineData(false, true, false, true)]
    [InlineData(true, true, false, true)]
    [InlineData(false, false, true, true)]
    [InlineData(true, false, true, true)]
    public async Task Handle_open_connection_when_returning_to_pool_for_external_connection_with_factory(
        bool async,
        bool startsOpen,
        bool openWithEf,
        bool withDependencyInjection)
    {
        using var connection = new SqlConnection(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString);

        if (startsOpen)
        {
            if (async)
            {
                await connection.OpenAsync();
            }
            else
            {
                connection.Open();
            }
        }

        var options = new DbContextOptionsBuilder<PooledContext>()
            .UseSqlServer(connection)
            .EnableServiceProviderCaching(false)
            .Options;

        var factory =
            withDependencyInjection
                ? new ServiceCollection()
                    .AddPooledDbContextFactory<PooledContext>(
                        ob => ob.UseSqlServer(connection)
                            .EnableServiceProviderCaching(false))
                    .BuildServiceProvider(validateScopes: true)
                    .GetRequiredService<IDbContextFactory<PooledContext>>()
                : new PooledDbContextFactory<PooledContext>(options);

        var context1 = async ? await factory.CreateDbContextAsync() : factory.CreateDbContext();
        Assert.Same(connection, context1.Database.GetDbConnection());

        if (!startsOpen)
        {
            if (async)
            {
                if (openWithEf)
                {
                    await context1.Database.OpenConnectionAsync();
                }
                else
                {
                    await connection.OpenAsync();
                }
            }
            else
            {
                if (openWithEf)
                {
                    context1.Database.OpenConnection();
                }
                else
                {
                    connection.Open();
                }
            }
        }

        Assert.Equal(ConnectionState.Open, connection.State);

        await Dispose(context1, async);

        Assert.Equal(ConnectionState.Open, connection.State);

        var context2 = async ? await factory.CreateDbContextAsync() : factory.CreateDbContext();
        Assert.Same(context1, context2);

        Assert.Same(connection, context2.Database.GetDbConnection());

        Assert.Equal(ConnectionState.Open, connection.State);

        await Dispose(context2, async);
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
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

    [ConditionalTheory (Skip = "Issue #32700")]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
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

                await context!.Customers.AsNoTracking().FirstAsync(c => c.CustomerId == "ALFKI");

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

    private readonly Stopwatch _stopwatch = new();

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

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Concurrency_test2(bool async)
    {
        var factory = BuildFactory<PooledContext>(withDependencyInjection: false);

        await Task.WhenAll(
            Enumerable.Range(0, 10).Select(
                _ => Task.Run(
                    async () =>
                    {
                        for (var j = 0; j < 1_000_000; j++)
                        {
                            var ctx = factory.CreateDbContext();

                            if (async)
                            {
                                await ctx.DisposeAsync();
                            }
                            else
                            {
                                ctx.Dispose();
                            }
                        }
                    })));
    }

    private void UseQueryTrackingBehavior(DbContextOptionsBuilder optionsBuilder, QueryTrackingBehavior? queryTrackingBehavior)
    {
        if (queryTrackingBehavior.HasValue)
        {
            optionsBuilder.UseQueryTrackingBehavior(queryTrackingBehavior.Value);
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

    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;
}
