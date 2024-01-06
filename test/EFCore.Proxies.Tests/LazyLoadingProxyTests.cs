// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore;

public class LazyLoadingProxyTests
{
    [ConditionalFact]
    public void Throws_if_sealed_class()
    {
        using var context = new LazyContext<LazySealedEntity>();
        Assert.Equal(
            ProxiesStrings.ItsASeal(nameof(LazySealedEntity)),
            Assert.Throws<InvalidOperationException>(
                () => context.Model).Message);
    }

    [ConditionalFact]
    public void Throws_if_non_virtual_navigation_to_non_owned_type()
    {
        using var context = new LazyContext<LazyNonVirtualNavEntity>();
        Assert.Equal(
            ProxiesStrings.NonVirtualProperty(nameof(LazyNonVirtualNavEntity.SelfRef), nameof(LazyNonVirtualNavEntity)),
            Assert.Throws<InvalidOperationException>(
                () => context.Model).Message);
    }

    [ConditionalFact]
    public void Does_not_throw_if_non_virtual_navigation_to_non_owned_type_is_allowed()
    {
        using var context = new LazyContextIgnoreVirtuals<LazyNonVirtualNavEntity>();
        Assert.NotNull(
            context.Model.FindEntityType(typeof(LazyNonVirtualNavEntity))!.FindNavigation(nameof(LazyNonVirtualNavEntity.SelfRef)));
    }

    [ConditionalFact]
    public void Does_not_throw_if_field_navigation_to_non_owned_type_is_allowed()
    {
        using var context = new LazyContextAllowingFieldNavigation();
        Assert.NotNull(
            context.Model.FindEntityType(typeof(LazyFieldNavEntity))!.FindNavigation(nameof(LazyFieldNavEntity.SelfRef)));
    }

    [ConditionalFact]
    public void Does_not_throw_if_non_virtual_navigation_is_set_to_not_eager_load()
    {
        using var context = new LazyContextDisabledNavigation();
        Assert.NotNull(
            context.Model.FindEntityType(typeof(LazyNonVirtualNavEntity))!.FindNavigation(nameof(LazyNonVirtualNavEntity.SelfRef)));
    }

    [ConditionalFact]
    public void Does_not_throw_if_field_navigation_is_set_to_not_eager_load()
    {
        using var context = new LazyContextDisabledFieldNavigation();
        Assert.NotNull(
            context.Model.FindEntityType(typeof(LazyFieldNavEntity))!.FindNavigation(nameof(LazyFieldNavEntity.SelfRef)));
    }

    [ConditionalFact]
    public void Does_not_throw_if_non_virtual_navigation_to_owned_type()
    {
        using var context = new LazyContext<LazyNonVirtualOwnedNavEntity>();
        Assert.NotNull(
            context.Model.FindEntityType(typeof(LazyNonVirtualOwnedNavEntity))!.FindNavigation(
                nameof(LazyNonVirtualOwnedNavEntity.NavigationToOwned)));
    }

    [ConditionalFact]
    public void Does_not_throw_if_field_navigation_to_owned_type()
    {
        using var context = new LazyContextOwnedFieldNavigation();
        Assert.NotNull(
            context.Model.FindEntityType(typeof(LazyFieldOwnedNavEntity))!.FindNavigation(
                nameof(LazyFieldOwnedNavEntity.NavigationToOwned)));
    }

    [ConditionalFact]
    public void Throws_if_no_field_found()
    {
        using var context = new LazyContext<LazyHiddenFieldEntity>();
        Assert.Equal(
            CoreStrings.NoBackingFieldLazyLoading(nameof(LazyHiddenFieldEntity.SelfRef), nameof(LazyHiddenFieldEntity)),
            Assert.Throws<InvalidOperationException>(
                () => context.Model).Message);
    }

    [ConditionalFact]
    public void Throws_when_context_is_disposed()
    {
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase()
            .AddEntityFrameworkProxies()
            .AddDbContext<JammieDodgerContext>(
                (p, b) =>
                    b.UseInMemoryDatabase("Jammie")
                        .UseInternalServiceProvider(p)
                        .UseLazyLoadingProxies())
            .BuildServiceProvider(validateScopes: true);

        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetService<JammieDodgerContext>();
            context.Add(new Phone());
            context.SaveChanges();
        }

        Phone phone;
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetService<JammieDodgerContext>();
            phone = context.Set<Phone>().Single();
        }

        Assert.Equal(
            CoreStrings.WarningAsErrorTemplate(
                CoreEventId.LazyLoadOnDisposedContextWarning.ToString(),
                CoreResources.LogLazyLoadOnDisposedContext(new TestLogger<TestLoggingDefinitions>())
                    .GenerateMessage("PhoneProxy", "Texts"),
                "CoreEventId.LazyLoadOnDisposedContextWarning"),
            Assert.Throws<InvalidOperationException>(
                () => phone.Texts).Message);
    }

    private class LazyContextIgnoreVirtuals<TEntity> : TestContext<TEntity>
        where TEntity : class
    {
        public LazyContextIgnoreVirtuals()
            : base(dbName: "LazyLoadingContext", useLazyLoading: true, useChangeDetection: false, ignoreNonVirtualNavigations: true)
        {
        }
    }

    private class LazyContext<TEntity> : TestContext<TEntity>
        where TEntity : class
    {
        public LazyContext()
            : base(dbName: "LazyLoadingContext", useLazyLoading: true, useChangeDetection: false)
        {
        }
    }

    private class LazyContextDisabledNavigation : TestContext<LazyNonVirtualNavEntity>
    {
        public LazyContextDisabledNavigation()
            : base(dbName: "LazyLoadingContext", useLazyLoading: true, useChangeDetection: false)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<LazyNonVirtualNavEntity>().Navigation(e => e.SelfRef).EnableLazyLoading(false);
        }
    }

    private class LazyContextAllowingFieldNavigation : TestContext<LazyFieldNavEntity>
    {
        public LazyContextAllowingFieldNavigation()
            : base(dbName: "LazyLoadingContext", useLazyLoading: true, useChangeDetection: false, ignoreNonVirtualNavigations: true)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<LazyFieldNavEntity>().HasOne(e => e.SelfRef).WithOne();
        }
    }

    private class LazyContextDisabledFieldNavigation : TestContext<LazyFieldNavEntity>
    {
        public LazyContextDisabledFieldNavigation()
            : base(dbName: "LazyLoadingContext", useLazyLoading: true, useChangeDetection: false)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<LazyFieldNavEntity>().HasOne(e => e.SelfRef).WithOne();
            modelBuilder.Entity<LazyFieldNavEntity>().Navigation(e => e.SelfRef).EnableLazyLoading(false);
        }
    }

    private class LazyContextOwnedFieldNavigation : TestContext<LazyFieldOwnedNavEntity>
    {
        public LazyContextOwnedFieldNavigation()
            : base(dbName: "LazyLoadingContext", useLazyLoading: true, useChangeDetection: false)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<LazyFieldOwnedNavEntity>().OwnsOne(e => e.NavigationToOwned).WithOwner(e => e.Owner);
        }
    }

    public sealed class LazySealedEntity
    {
        public int Id { get; set; }
    }

    public class LazyNonVirtualNavEntity
    {
        public int Id { get; set; }

        public LazyNonVirtualNavEntity SelfRef { get; set; }
    }

    public class LazyFieldNavEntity
    {
        public int Id { get; set; }

        public LazyFieldNavEntity SelfRef;
    }

    public class LazyNonVirtualOwnedNavEntity
    {
        public int Id { get; set; }

        public OwnedNavEntity NavigationToOwned { get; set; }
    }

    [Owned]
    public class OwnedNavEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public LazyNonVirtualOwnedNavEntity Owner { get; set; }
    }

    public class LazyFieldOwnedNavEntity
    {
        public int Id { get; set; }

        public OwnedFieldNavEntity NavigationToOwned;
    }

    [Owned]
    public class OwnedFieldNavEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public LazyFieldOwnedNavEntity Owner;
    }

    public class LazyHiddenFieldEntity
    {
        private LazyHiddenFieldEntity _hiddenBackingField;

        public int Id { get; set; }

        // ReSharper disable once ConvertToAutoProperty
        public virtual LazyHiddenFieldEntity SelfRef
        {
            get => _hiddenBackingField;
            set => _hiddenBackingField = value;
        }
    }

    private class JammieDodgerContext(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Phone>();
    }

    public class Phone
    {
        public int Id { get; set; }
        public virtual ICollection<Text> Texts { get; set; }
    }

    public class Text
    {
        public int Id { get; set; }
    }
}
