// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Castle.DynamicProxy;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore;

public class ProxyTests
{
    [ConditionalFact]
    public void Materialization_uses_parameterless_constructor()
    {
        using (var context = new NeweyContext(nameof(Materialization_uses_parameterless_constructor)))
        {
            context.Add(new March82GGtp());
            context.SaveChanges();
        }

        using (var context = new NeweyContext(nameof(Materialization_uses_parameterless_constructor)))
        {
            Assert.Same(typeof(March82GGtp), context.Set<March82GGtp>().Single().GetType().BaseType);
        }
    }

    [ConditionalFact]
    public void Materialization_uses_parameterized_constructor()
    {
        using (var context = new NeweyContext(nameof(Materialization_uses_parameterized_constructor)))
        {
            context.Add(new March881(77, "Leyton House"));
            context.SaveChanges();
        }

        using (var context = new NeweyContext(nameof(Materialization_uses_parameterized_constructor)))
        {
            var proxy = context.Set<March881>().Single();

            Assert.Same(typeof(March881), proxy.GetType().BaseType);
            Assert.Equal(77, proxy.Id);
            Assert.Equal("Leyton House", proxy.Sponsor);
        }
    }

    [ConditionalFact]
    public void Materialization_uses_parameterized_constructor_taking_context()
    {
        using (var context = new NeweyContext(nameof(Materialization_uses_parameterized_constructor_taking_context)))
        {
            context.Add(new WilliamsFw14(context, 6, "Canon"));
            context.SaveChanges();
        }

        using (var context = new NeweyContext(nameof(Materialization_uses_parameterized_constructor_taking_context)))
        {
            var proxy = context.Set<WilliamsFw14>().Single();

            Assert.Same(typeof(WilliamsFw14), proxy.GetType().BaseType);
            Assert.Same(context, proxy.Context);
            Assert.Equal(6, proxy.Id);
            Assert.Equal("Canon", proxy.Sponsor);
        }
    }

    [ConditionalFact]
    public void CreateProxy_works_for_shared_type_entity_types()
    {
        using var context = new NeweyContext();

        Assert.Same(typeof(SharedTypeEntityType), context.Set<SharedTypeEntityType>("STET1").CreateProxy().GetType().BaseType);
        Assert.Same(typeof(SharedTypeEntityType), context.Set<SharedTypeEntityType>("STET1").CreateProxy(_ => { }).GetType().BaseType);
    }

    [ConditionalFact]
    public void CreateProxy_works_for_record_with_base_type_entity_types()
    {
        using var context = new NeweyContext();

        Assert.Same(typeof(March86C), context.Set<March86C>().CreateProxy().GetType().BaseType);
        Assert.Same(typeof(March86C), context.Set<March86C>().CreateProxy(_ => { }).GetType().BaseType);
    }

    [ConditionalFact]
    public void CreateProxy_throws_for_shared_type_entity_types_when_entity_type_name_not_known()
    {
        using var context = new NeweyContext();

        Assert.Equal(
            ProxiesStrings.EntityTypeNotFoundShared(nameof(SharedTypeEntityType)),
            Assert.Throws<InvalidOperationException>(() => context.CreateProxy<SharedTypeEntityType>()).Message);

        Assert.Equal(
            ProxiesStrings.EntityTypeNotFoundShared(nameof(SharedTypeEntityType)),
            Assert.Throws<InvalidOperationException>(() => context.CreateProxy<SharedTypeEntityType>(_ => { })).Message);

        Assert.Equal(
            ProxiesStrings.EntityTypeNotFoundShared(nameof(SharedTypeEntityType)),
            Assert.Throws<InvalidOperationException>(() => context.CreateProxy(typeof(SharedTypeEntityType))).Message);
    }

    [ConditionalFact]
    public void CreateProxy_works_for_owned_but_not_weak_entity_types()
    {
        using var context = new NeweyContext();

        Assert.Same(typeof(IsOwnedButNotWeak), context.CreateProxy<IsOwnedButNotWeak>().GetType().BaseType);
        Assert.Same(typeof(IsOwnedButNotWeak), context.CreateProxy<IsOwnedButNotWeak>(_ => { }).GetType().BaseType);
        Assert.Same(typeof(IsOwnedButNotWeak), context.CreateProxy(typeof(IsOwnedButNotWeak)).GetType().BaseType);
    }

    [ConditionalFact]
    public void CreateProxy_uses_parameterless_constructor()
    {
        using var context = new NeweyContext();
        Assert.Same(typeof(March82GGtp), context.CreateProxy<March82GGtp>().GetType().BaseType);
    }

    [ConditionalFact]
    public void CreateProxy_uses_parameterized_constructor()
    {
        using var context = new NeweyContext();
        var proxy = context.CreateProxy<March881>(77, "Leyton House");

        Assert.Same(typeof(March881), proxy.GetType().BaseType);
        Assert.Equal(77, proxy.Id);
        Assert.Equal("Leyton House", proxy.Sponsor);
    }

    [ConditionalFact]
    public void CreateProxy_uses_parameterized_constructor_taking_context()
    {
        using var context = new NeweyContext();
        var proxy = context.CreateProxy<WilliamsFw14>(context, 6, "Canon");

        Assert.Same(typeof(WilliamsFw14), proxy.GetType().BaseType);
        Assert.Same(context, proxy.Context);
        Assert.Equal(6, proxy.Id);
        Assert.Equal("Canon", proxy.Sponsor);
    }

    [ConditionalFact]
    public void Proxies_only_created_if_Use_called()
    {
        using (var context = new NeweyContext(nameof(Proxies_only_created_if_Use_called), false))
        {
            context.Add(new March82GGtp());
            context.SaveChanges();
        }

        using (var context = new NeweyContext(nameof(Proxies_only_created_if_Use_called), false))
        {
            Assert.Same(typeof(March82GGtp), context.Set<March82GGtp>().Single().GetType());
        }

        using (var context = new NeweyContext(nameof(Proxies_only_created_if_Use_called)))
        {
            Assert.Same(typeof(March82GGtp), context.Set<March82GGtp>().Single().GetType().BaseType);
        }

        using (var context = new NeweyContext(nameof(Proxies_only_created_if_Use_called), false, true))
        {
            Assert.Same(typeof(March82GGtp), context.Set<March82GGtp>().Single().GetType().BaseType);
        }

        using (var context = new NeweyContext(nameof(Proxies_only_created_if_Use_called), true, true))
        {
            Assert.Same(typeof(March82GGtp), context.Set<March82GGtp>().Single().GetType().BaseType);
        }
    }

    [ConditionalFact]
    public void Proxy_services_must_be_available()
    {
        var withoutProxies = new ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase()
            .BuildServiceProvider(validateScopes: true);

        using (var context = new NeweyContext(withoutProxies, nameof(Proxy_services_must_be_available), false))
        {
            context.Add(new March82GGtp());
            context.SaveChanges();
        }

        using (var context = new NeweyContext(withoutProxies, nameof(Proxy_services_must_be_available), false))
        {
            Assert.Same(typeof(March82GGtp), context.Set<March82GGtp>().Single().GetType());
        }

        using (var context = new NeweyContext(nameof(Proxy_services_must_be_available)))
        {
            Assert.Same(typeof(March82GGtp), context.Set<March82GGtp>().Single().GetType().BaseType);
        }

        using (var context = new NeweyContext(withoutProxies, nameof(Proxy_services_must_be_available)))
        {
            Assert.Equal(
                ProxiesStrings.ProxyServicesMissing,
                Assert.Throws<InvalidOperationException>(
                    () => context.Model).Message);
        }
    }

    [ConditionalFact]
    public void Throws_if_type_not_available_to_Castle()
    {
        using var context = new NeweyContextN4();
        Assert.Throws<ArgumentException>(() => context.CreateProxy<McLarenMp421>());
    }

    [ConditionalFact]
    public void Throws_if_constructor_not_available_to_Castle()
    {
        using var context = new NeweyContextN5();
        Assert.Throws<ArgumentException>(() => context.CreateProxy<RedBullRb3>());
    }

    [ConditionalFact]
    public void CreateProxy_throws_if_constructor_args_do_not_match()
    {
        using var context = new NeweyContext();
        Assert.Throws<ArgumentException>(() => context.CreateProxy<March881>(77, 88));
    }

    [ConditionalFact]
    public void CreateProxy_throws_if_wrong_number_of_constructor_args()
    {
        using var context = new NeweyContext();
        Assert.Throws<ArgumentException>(() => context.CreateProxy<March881>(77, 88, 99));
    }

    [ConditionalFact]
    public void Throws_if_create_proxy_for_non_mapped_type()
    {
        using var context = new NeweyContextN();
        Assert.Equal(
            CoreStrings.EntityTypeNotFound(nameof(RedBullRb3)),
            Assert.Throws<InvalidOperationException>(
                () => context.CreateProxy<RedBullRb3>()).Message);
    }

    [ConditionalFact]
    public void Throws_if_create_proxy_when_proxies_not_used()
    {
        using var context = new NeweyContextN6();
        Assert.Equal(
            ProxiesStrings.ProxiesNotEnabled(nameof(RedBullRb3)),
            Assert.Throws<InvalidOperationException>(
                () => context.CreateProxy<RedBullRb3>()).Message);
    }

    [ConditionalFact]
    public void Throws_if_create_proxy_when_proxies_not_enabled()
    {
        using var context = new NeweyContextN7();
        Assert.Equal(
            ProxiesStrings.ProxiesNotEnabled(nameof(RedBullRb3)),
            Assert.Throws<InvalidOperationException>(
                () => context.CreateProxy<RedBullRb3>()).Message);
    }

    [ConditionalFact]
    public void Throws_if_attempt_to_create_EntityType_based_on_proxy_class()
    {
        var model = new Model();
        var generator = new ProxyGenerator();
        var proxy = generator.CreateClassProxy<ClassToBeProxied>();

        Assert.Equal(
            CoreStrings.AddingProxyTypeAsEntityType("Castle.Proxies.ClassToBeProxiedProxy"),
            Assert.Throws<ArgumentException>(
                () => new EntityType(proxy.GetType(), model, owned: false, ConfigurationSource.Explicit)).Message);
    }

    // tests scenario in https://github.com/dotnet/efcore/issues/15958
    [ConditionalFact]
    public void Throws_if_attempt_to_add_proxy_type_to_model_builder()
        => Assert.Equal(
            CoreStrings.AddingProxyTypeAsEntityType("Castle.Proxies.ClassToBeProxiedProxy"),
            Assert.Throws<ArgumentException>(
                () =>
                {
                    var context = new CannotAddProxyTypeToModel();
                    context.Set<ClassToBeProxied>().Add(new ClassToBeProxied { Id = 0 });
                }).Message);

    public class March82GGtp
    {
        public virtual int Id { get; set; }
    }

    public class March881(int id, string sponsor)
    {
        public virtual int Id { get; set; } = id;

        public virtual string Sponsor { get; set; } = sponsor;
    }

    public class WilliamsFw14(DbContext context, int id, string sponsor)
    {
        public DbContext Context { get; set; } = context;

        public virtual int Id { get; set; } = id;

        public virtual string Sponsor { get; set; } = sponsor;
    }

    public class SharedTypeEntityType
    {
        public virtual int Id { get; set; }
    }

    public class WithWeak
    {
        public virtual int Id { get; set; }

        public virtual IsOwnedButNotWeak Owned { get; set; }

        public virtual IsWeak Weak1 { get; set; }
        public virtual IsWeak Weak2 { get; set; }
    }

    [Owned]
    public class IsWeak;

    [Owned]
    public class IsOwnedButNotWeak;

    public record March86C : IndyCar
    {
        public virtual int Id { get; init; }
    }

    public record IndyCar;

    private class NeweyContext(string dbName = null, bool useLazyLoading = true, bool useChangeDetection = false) : DbContext
    {
        private readonly IServiceProvider _internalServiceProvider
                = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .AddEntityFrameworkProxies()
                    .BuildServiceProvider(validateScopes: true);
        private static readonly InMemoryDatabaseRoot _dbRoot = new();
        private readonly bool _useLazyLoadingProxies = useLazyLoading;
        private readonly bool _useChangeDetectionProxies = useChangeDetection;
        private readonly string _dbName = dbName;

        public NeweyContext(
            IServiceProvider internalServiceProvider,
            string dbName = null,
            bool useLazyLoading = true,
            bool useChangeDetection = false)
            : this(dbName, useLazyLoading, useChangeDetection)
        {
            _internalServiceProvider = internalServiceProvider;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (_useLazyLoadingProxies)
            {
                optionsBuilder.UseLazyLoadingProxies();
            }

            if (_useChangeDetectionProxies)
            {
                optionsBuilder.UseChangeTrackingProxies();
            }

            if (_internalServiceProvider != null)
            {
                optionsBuilder.UseInternalServiceProvider(_internalServiceProvider);
            }

            optionsBuilder.UseInMemoryDatabase(_dbName ?? nameof(NeweyContext), _dbRoot);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<March82GGtp>();

            modelBuilder.Entity<March881>(
                b =>
                {
                    b.Property(e => e.Id);
                    b.Property(e => e.Sponsor);
                });

            modelBuilder.Entity<WilliamsFw14>(
                b =>
                {
                    b.Property(e => e.Id);
                    b.Property(e => e.Sponsor);
                });

            modelBuilder.SharedTypeEntity<SharedTypeEntityType>("STET1");
            modelBuilder.SharedTypeEntity<SharedTypeEntityType>("STET2");

            modelBuilder.Entity<WithWeak>();

            modelBuilder.Entity<March86C>();
        }
    }

    private class NeweyContextN : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseLazyLoadingProxies()
                .UseInternalServiceProvider(
                    new ServiceCollection()
                        .AddEntityFrameworkInMemoryDatabase()
                        .AddEntityFrameworkProxies()
                        .BuildServiceProvider(validateScopes: true))
                .UseInMemoryDatabase(Guid.NewGuid().ToString());
    }

    internal class McLarenMp421
    {
        public virtual int Id { get; set; }
    }

    private class NeweyContextN4 : NeweyContextN
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<McLarenMp421>();
    }

    public class RedBullRb3
    {
        internal RedBullRb3()
        {
        }

        public virtual int Id { get; set; }
    }

    private class NeweyContextN5 : NeweyContextN
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<RedBullRb3>();
    }

    private class NeweyContextN6 : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(
                    new ServiceCollection()
                        .AddEntityFrameworkInMemoryDatabase()
                        .AddEntityFrameworkProxies()
                        .BuildServiceProvider(validateScopes: true))
                .UseInMemoryDatabase(Guid.NewGuid().ToString());

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<March82GGtp>();
    }

    private class NeweyContextN7 : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseLazyLoadingProxies(false)
                .UseInternalServiceProvider(
                    new ServiceCollection()
                        .AddEntityFrameworkInMemoryDatabase()
                        .AddEntityFrameworkProxies()
                        .BuildServiceProvider(validateScopes: true))
                .UseInMemoryDatabase(Guid.NewGuid().ToString());

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<March82GGtp>();
    }

    public class ClassToBeProxied
    {
        public virtual int Id { get; set; }
    }

    private class CannotAddProxyTypeToModel : DbContext
    {
        public DbSet<ClassToBeProxied> _entityToBeProxied { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseLazyLoadingProxies()
                .UseInternalServiceProvider(
                    new ServiceCollection()
                        .AddEntityFrameworkInMemoryDatabase()
                        .AddEntityFrameworkProxies()
                        .BuildServiceProvider(validateScopes: true))
                .UseInMemoryDatabase(Guid.NewGuid().ToString());

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var generator = new ProxyGenerator();
            var proxy = generator.CreateClassProxy<ClassToBeProxied>();

            // below should throw
            modelBuilder.Entity(proxy.GetType());
        }
    }
}
