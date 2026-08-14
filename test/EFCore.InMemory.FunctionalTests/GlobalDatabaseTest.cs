// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local

namespace Microsoft.EntityFrameworkCore;

public class GlobalDatabaseTest
{
    private static readonly InMemoryDatabaseRoot _databaseRoot = new();

    [ConditionalFact]
    public void Different_stores_are_used_when_options_force_different_internal_service_provider()
    {
        using (var context = new BooFooContext(
                   new DbContextOptionsBuilder()
                       .UseInMemoryDatabase(nameof(BooFooContext))
                       .Options))
        {
            context.Add(new Foo());
            context.SaveChanges();
        }

        using (var context = new BooFooContext(
                   new DbContextOptionsBuilder()
                       .UseInMemoryDatabase(nameof(BooFooContext))
                       .EnableSensitiveDataLogging()
                       .Options))
        {
            Assert.Empty(context.Foos.ToList());
        }
    }

    [ConditionalFact]
    public void AddDbContext_does_not_force_different_internal_service_provider()
    {
        using (var context = new BooFooContext(
                   new DbContextOptionsBuilder()
                       .UseInMemoryDatabase(nameof(BooFooContext))
                       .Options))
        {
            context.Add(new Foo());
            context.SaveChanges();
        }

        var serviceProvider = new ServiceCollection()
            .AddDbContext<BooFooContext>(
                b => b.UseInMemoryDatabase(nameof(BooFooContext)))
            .BuildServiceProvider(validateScopes: true);

        using var scope = serviceProvider.CreateScope();
        {
            var context = scope.ServiceProvider.GetService<BooFooContext>();
            Assert.NotEmpty(context.Foos.ToList());
        }
    }

    [ConditionalFact]
    public void Global_store_can_be_used_when_options_force_different_internal_service_provider()
    {
        using (var context = new BooFooContext(
                   new DbContextOptionsBuilder()
                       .EnableServiceProviderCaching(false)
                       .UseInMemoryDatabase(nameof(BooFooContext), _databaseRoot)
                       .Options))
        {
            context.Add(new Foo());
            context.SaveChanges();
        }

        using (var context = new BooFooContext(
                   new DbContextOptionsBuilder()
                       .EnableServiceProviderCaching(false)
                       .UseInMemoryDatabase(nameof(BooFooContext), _databaseRoot)
                       .EnableSensitiveDataLogging()
                       .Options))
        {
            Assert.Equal(1, context.Foos.Count());
        }
    }

    [ConditionalFact]
    public void Owned_types_are_found_correctly_with_database_root()
    {
        var options = new DbContextOptionsBuilder()
            .UseInMemoryDatabase("20784", _databaseRoot)
            .Options;

        using (var context = new BooFooContext(options))
        {
            context.Add(new Foo { Goo1 = null, Goo2 = new Goo() });
            context.Add(new Boo { Goo1 = new Goo(), Goo2 = new Goo() });
            context.SaveChanges();

            var foos = context.Foos.Single();
            Assert.Null(foos.Goo1);
            Assert.NotNull(foos.Goo2);

            var boos = context.Boos.Single();
            Assert.NotNull(boos.Goo1);
            Assert.NotNull(boos.Goo2);
            Assert.NotSame(boos.Goo1, boos.Goo2);
        }

        using (var context = new BooFooContext(options))
        {
            var foos = context.Foos.Single();
            Assert.Null(foos.Goo1);
            Assert.NotNull(foos.Goo2);

            var boos = context.Boos.Single();
            Assert.NotNull(boos.Goo1);
            Assert.NotNull(boos.Goo2);
            Assert.NotSame(boos.Goo1, boos.Goo2);
        }
    }

    [ConditionalFact]
    public void Global_store_can_be_used_when_AddDbContext_force_different_internal_service_provider()
    {
        using (var context = new BooFooContext(
                   new DbContextOptionsBuilder()
                       .EnableServiceProviderCaching(false)
                       .UseInMemoryDatabase(nameof(BooFooContext), _databaseRoot)
                       .Options))
        {
            context.Add(new Boo());
            context.SaveChanges();
        }

        var serviceProvider = new ServiceCollection()
            .AddDbContext<BooFooContext>(
                b =>
                    b.UseInMemoryDatabase(nameof(BooFooContext), _databaseRoot)
                        .EnableServiceProviderCaching(false))
            .BuildServiceProvider(validateScopes: true);

        using var scope = serviceProvider.CreateScope();
        {
            var context = scope.ServiceProvider.GetService<BooFooContext>();
            Assert.Equal(1, context.Boos.Count());
        }
    }

    [ConditionalFact]
    public void EnableNullChecks_forces_different_internal_service_provider()
    {
        using var context1 = new ChangeNullabilityChecksContext(enableNullChecks: true);
        using var context2 = new ChangeNullabilityChecksContext(enableNullChecks: false);
        Assert.NotSame(((IInfrastructure<IServiceProvider>)context1).Instance, ((IInfrastructure<IServiceProvider>)context2).Instance);
    }

    private class ChangeNullabilityChecksContext(bool enableNullChecks) : DbContext
    {
        private readonly bool _enableNullChecks = enableNullChecks;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInMemoryDatabase(
                    nameof(ChangeNullabilityChecksCacheContext),
                    b => b.EnableNullChecks(_enableNullChecks));
    }

    [ConditionalFact]
    public void Throws_changing_global_store_in_OnConfiguring_when_UseInternalServiceProvider()
    {
        using (var context = new ChangeSdlCacheContext(false))
        {
            Assert.NotNull(context.Model);
        }

        using (var context = new ChangeSdlCacheContext(true))
        {
            Assert.Equal(
                CoreStrings.SingletonOptionChanged(
                    nameof(InMemoryDbContextOptionsExtensions.UseInMemoryDatabase),
                    nameof(DbContextOptionsBuilder.UseInternalServiceProvider)),
                Assert.Throws<InvalidOperationException>(() => context.Model).Message);
        }
    }

    private class ChangeSdlCacheContext(bool on) : DbContext
    {
        private static readonly IServiceProvider _serviceProvider
            = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider(validateScopes: true);

        private readonly bool _on = on;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(_serviceProvider)
                .UseInMemoryDatabase(nameof(ChangeSdlCacheContext), _on ? _databaseRoot : null);
    }

    [ConditionalFact]
    public void Throws_changing_nullability_checks_in_OnConfiguring_when_UseInternalServiceProvider()
    {
        using (var context = new ChangeNullabilityChecksCacheContext(false))
        {
            Assert.NotNull(context.Model);
        }

        using (var context = new ChangeNullabilityChecksCacheContext(true))
        {
            Assert.Equal(
                CoreStrings.SingletonOptionChanged(
                    nameof(InMemoryDbContextOptionsBuilder.EnableNullChecks),
                    nameof(DbContextOptionsBuilder.UseInternalServiceProvider)),
                Assert.Throws<InvalidOperationException>(() => context.Model).Message);
        }
    }

    private class ChangeNullabilityChecksCacheContext(bool on) : DbContext
    {
        private static readonly IServiceProvider _serviceProvider
            = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider(validateScopes: true);

        private readonly bool _on = on;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(_serviceProvider)
                .UseInMemoryDatabase(nameof(ChangeSdlCacheContext), b => b.EnableNullChecks(_on));
    }

    private class BooFooContext(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Foo>(
                b =>
                {
                    b.OwnsOne(e => e.Goo1);
                    b.OwnsOne(e => e.Goo2);
                });

            modelBuilder.Entity<Boo>(
                b =>
                {
                    b.OwnsOne(e => e.Goo1);
                    b.OwnsOne(e => e.Goo2);
                });
        }

        public DbSet<Foo> Foos { get; set; }
        public DbSet<Boo> Boos { get; set; }
    }

    private class Foo
    {
        public int Id { get; set; }
        public Goo Goo1 { get; set; }
        public Goo Goo2 { get; set; }
    }

    private class Boo
    {
        public int Id { get; set; }
        public Goo Goo1 { get; set; }
        public Goo Goo2 { get; set; }
    }

    private class Goo
    {
        public string Goop { get; set; }
    }
}
