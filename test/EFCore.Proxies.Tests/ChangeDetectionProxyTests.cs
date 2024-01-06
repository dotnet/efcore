// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore;

public class ChangeDetectionProxyTests
{
    [ConditionalFact]
    public void Throws_if_sealed_class()
    {
        using var context = new ChangeContext<ChangeSealedEntity>();
        Assert.Equal(
            ProxiesStrings.ItsASeal(nameof(ChangeSealedEntity)),
            Assert.Throws<InvalidOperationException>(
                () => context.Model).Message);
    }

    [ConditionalFact]
    public void Throws_if_non_virtual_property()
    {
        using var context = new ChangeContext<ChangeNonVirtualPropEntity>();
        Assert.Equal(
            ProxiesStrings.NonVirtualProperty(nameof(ChangeNonVirtualPropEntity.Id), nameof(ChangeNonVirtualPropEntity)),
            Assert.Throws<InvalidOperationException>(
                () => context.Model).Message);
    }

    [ConditionalFact]
    public void Throws_if_non_virtual_indexer_property()
    {
        using var context = new ChangeContext<ChangeNonVirtualIndexer>(entityBuilderAction: b => b.IndexerProperty<int>("Snoopy"));
        Assert.Equal(
            ProxiesStrings.NonVirtualIndexerProperty(nameof(ChangeNonVirtualIndexer)),
            Assert.Throws<InvalidOperationException>(() => context.Model).Message);
    }

    [ConditionalFact]
    public void Does_not_throw_when_non_virtual_indexer_not_mapped()
    {
        using var context = new ChangeContext<ChangeNonVirtualIndexerNotUsed>();

        Assert.DoesNotContain(
            context.Model.FindEntityType(typeof(ChangeNonVirtualIndexerNotUsed)).GetProperties(), e => e.IsIndexerProperty());
    }

    [ConditionalFact]
    public void Does_not_throw_if_dictionary_type_with_only_PKs()
    {
        using var context = new SharedChangeContext<Dictionary<string, int>>();

        Assert.True(context.Model.IsShared(typeof(Dictionary<string, int>)));
    }

    [ConditionalFact]
    public void Throws_if_dictionary_type_with_additional_properties()
    {
        using var context = new SharedChangeContext<Dictionary<string, int>>(b => b.IndexerProperty<int>("Snoopy"));

        Assert.Equal(
            ProxiesStrings.DictionaryCannotBeProxied(
                typeof(Dictionary<string, int>).ShortDisplayName(), "STET (Dictionary<string, int>)",
                typeof(IDictionary<string, int>).ShortDisplayName()),
            Assert.Throws<InvalidOperationException>(() => context.Model).Message);
    }

    [ConditionalFact]
    public void Throws_if_non_virtual_navigation()
    {
        using var context = new ChangeContext<ChangeNonVirtualNavEntity>();
        Assert.Equal(
            ProxiesStrings.NonVirtualProperty(nameof(ChangeNonVirtualNavEntity.SelfRef), nameof(ChangeNonVirtualNavEntity)),
            Assert.Throws<InvalidOperationException>(
                () => context.Model).Message);
    }

    [ConditionalFact]
    public void Sets_default_change_tracking_strategy()
    {
        using var context = new ChangeContext<ChangeValueEntity>();

        Assert.Equal(
            ChangeTrackingStrategy.ChangingAndChangedNotifications,
            context.GetService<IDesignTimeModel>().Model.GetChangeTrackingStrategy());
    }

    [ConditionalFact]
    public void Default_change_tracking_strategy_doesnt_overwrite_entity_strategy()
    {
        using var context = new ChangingAndChangedNotificationsWithOriginalValuesContext();

        var entityType = context.Model.FindEntityType(typeof(ChangeValueEntity));

        Assert.Equal(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues, entityType.GetChangeTrackingStrategy());
    }

    private static readonly Type changeInterface = typeof(INotifyPropertyChanged);
    private static readonly Type changingInterface = typeof(INotifyPropertyChanging);

    [ConditionalFact]
    public void Throws_when_proxies_are_used_with_snapshot_tracking()
    {
        using var context = new SnapshotContext();

        Assert.Equal(
            CoreStrings.FullChangeTrackingRequired(
                nameof(ChangeValueEntity), nameof(ChangeTrackingStrategy.Snapshot),
                nameof(ChangeTrackingStrategy.ChangingAndChangedNotifications),
                nameof(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues)),
            Assert.Throws<InvalidOperationException>(() => _ = context.Model).Message);
    }

    [ConditionalFact]
    public void Throws_when_proxies_are_used_with_changed_only_tracking()
    {
        using var context = new ChangedNotificationsContext();

        Assert.Equal(
            CoreStrings.FullChangeTrackingRequired(
                nameof(ChangeValueEntity), nameof(ChangeTrackingStrategy.ChangedNotifications),
                nameof(ChangeTrackingStrategy.ChangingAndChangedNotifications),
                nameof(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues)),
            Assert.Throws<InvalidOperationException>(() => _ = context.Model).Message);
    }

    [ConditionalFact]
    public void Proxies_correct_interfaces_for_default_strategy()
    {
        using var context = new DefaultContext();
        var proxy = context.CreateProxy<ChangeValueEntity>();
        var proxyType = proxy.GetType();

        Assert.True(changeInterface.IsAssignableFrom(proxyType));
        Assert.True(changingInterface.IsAssignableFrom(proxyType));
    }

    [ConditionalFact]
    public void Proxies_correct_interfaces_for_ChangingAndChangedNotifications()
    {
        using var context = new ChangingAndChangedNotificationsContext();
        var proxy = context.CreateProxy<ChangeValueEntity>();
        var proxyType = proxy.GetType();

        Assert.True(changeInterface.IsAssignableFrom(proxyType));
        Assert.True(changingInterface.IsAssignableFrom(proxyType));
    }

    [ConditionalFact]
    public void Proxies_correct_interfaces_for_ChangingAndChangedNotificationsWithOriginalValues()
    {
        using var context = new ChangingAndChangedNotificationsWithOriginalValuesContext();
        var proxy = context.CreateProxy<ChangeValueEntity>();
        var proxyType = proxy.GetType();

        Assert.True(changeInterface.IsAssignableFrom(proxyType));
        Assert.True(changingInterface.IsAssignableFrom(proxyType));
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Raises_changed_event_when_changed(bool useLazyLoading)
    {
        using var context = new ChangeContext<ChangeValueEntity>(useLazyLoading: useLazyLoading);
        var proxy = context.CreateProxy<ChangeValueEntity>();
        context.Add(proxy);
        context.SaveChanges();

        var eventRaised = false;

        ((INotifyPropertyChanged)proxy).PropertyChanged += (s, e) =>
        {
            eventRaised = true;

            Assert.Equal(proxy, s);

            Assert.Equal(
                nameof(ChangeValueEntity.Value),
                e.PropertyName);

            Assert.Equal(
                10,
                ((ChangeValueEntity)s).Value);
        };

        proxy.Value = 10;
        Assert.True(eventRaised);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Raises_changing_event_before_change(bool useLazyLoading)
    {
        using var context = new ChangeContext<ChangeValueEntity>(useLazyLoading: useLazyLoading);
        var proxy = context.CreateProxy<ChangeValueEntity>();
        proxy.Value = 5;
        context.Add(proxy);
        context.SaveChanges();

        var eventRaised = false;

        ((INotifyPropertyChanging)proxy).PropertyChanging += (s, e) =>
        {
            eventRaised = true;

            Assert.Equal(proxy, s);

            Assert.Equal(
                nameof(ChangeValueEntity.Value),
                e.PropertyName);

            Assert.Equal(
                5,
                ((ChangeValueEntity)s).Value);
        };

        proxy.Value = 10;
        Assert.True(eventRaised);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Doesnt_raise_change_event_when_equal_and_check_equality_true(bool useLazyLoading)
    {
        using var context = new ChangeContext<ChangeValueEntity>(useLazyLoading: useLazyLoading, checkEquality: true);
        var proxy = context.CreateProxy<ChangeValueEntity>();
        proxy.Value = 10;
        context.Add(proxy);
        context.SaveChanges();

        var eventRaised = false;

        ((INotifyPropertyChanged)proxy).PropertyChanged += (s, e) =>
        {
            eventRaised = true;
        };

        proxy.Value = 10;
        Assert.False(eventRaised);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Doesnt_raise_changing_event_when_equal_and_check_equality_true(bool useLazyLoading)
    {
        using var context = new ChangeContext<ChangeValueEntity>(useLazyLoading: useLazyLoading, checkEquality: true);
        var proxy = context.CreateProxy<ChangeValueEntity>();
        proxy.Value = 10;
        context.Add(proxy);
        context.SaveChanges();

        var eventRaised = false;

        ((INotifyPropertyChanging)proxy).PropertyChanging += (s, e) =>
        {
            eventRaised = true;
        };

        proxy.Value = 10;
        Assert.False(eventRaised);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Raises_change_event_when_equal_and_check_equality_false(bool useLazyLoading)
    {
        using var context = new ChangeContext<ChangeValueEntity>(useLazyLoading: useLazyLoading, checkEquality: false);
        var proxy = context.CreateProxy<ChangeValueEntity>();
        proxy.Value = 10;
        context.Add(proxy);
        context.SaveChanges();

        var eventRaised = false;

        ((INotifyPropertyChanged)proxy).PropertyChanged += (s, e) =>
        {
            eventRaised = true;
        };

        proxy.Value = 10;
        Assert.True(eventRaised);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Raises_changing_event_when_equal_and_check_equality_false(bool useLazyLoading)
    {
        using var context = new ChangeContext<ChangeValueEntity>(useLazyLoading: useLazyLoading, checkEquality: false);
        var proxy = context.CreateProxy<ChangeValueEntity>();
        proxy.Value = 10;
        context.Add(proxy);
        context.SaveChanges();

        var eventRaised = false;

        ((INotifyPropertyChanging)proxy).PropertyChanging += (s, e) =>
        {
            eventRaised = true;
        };

        proxy.Value = 10;
        Assert.True(eventRaised);
    }

    private class ChangeContext<TEntity>(
        bool useLazyLoading = false,
        bool checkEquality = true,
        Action<EntityTypeBuilder<TEntity>> entityBuilderAction = null) : TestContext<TEntity>(
            dbName: "ChangeDetectionContext", useLazyLoading: useLazyLoading, useChangeDetection: true,
            checkEquality: checkEquality)
        where TEntity : class
    {
        private readonly Action<EntityTypeBuilder<TEntity>> _entityBuilderAction = entityBuilderAction;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var builder = modelBuilder.Entity<TEntity>();
            _entityBuilderAction?.Invoke(builder);
        }
    }

    private class SharedChangeContext<TEntity>(Action<EntityTypeBuilder<TEntity>> entityBuilderAction = null) : DbContext
        where TEntity : class
    {
        private readonly Action<EntityTypeBuilder<TEntity>> _entityBuilderAction = entityBuilderAction;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseChangeTrackingProxies()
                .UseInMemoryDatabase(GetType().ShortDisplayName());

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var builder = modelBuilder.SharedTypeEntity<TEntity>("STET");
            builder.Property<int>("Id");
            _entityBuilderAction?.Invoke(builder);
        }
    }

    public sealed class ChangeSealedEntity
    {
        public int Id { get; set; }
    }

    public class ChangeNonVirtualPropEntity
    {
        public int Id { get; set; }

        public virtual ChangeNonVirtualPropEntity SelfRef { get; set; }
    }

    public class ChangeNonVirtualNavEntity
    {
        public virtual int Id { get; set; }

        public ChangeNonVirtualNavEntity SelfRef { get; set; }
    }

    public class ChangeValueEntity
    {
        public virtual int Id { get; set; }

        public virtual int Value { get; set; }
    }

    public class ChangeSelfRefEntity
    {
        public virtual int Id { get; set; }

        public virtual ChangeSelfRefEntity SelfRef { get; set; }
    }

    public class ChangeNonVirtualIndexer
    {
        private readonly Dictionary<string, object> _keyValuePairs = new();

        public virtual int Id { get; set; }

        public object this[string key]
        {
            get => _keyValuePairs[key];
            set => _keyValuePairs[key] = value;
        }
    }

    public class ChangeNonVirtualIndexerNotUsed
    {
        private readonly Dictionary<string, object> _keyValuePairs = new();

        public virtual int Id { get; set; }

        public object this[string key]
        {
            get => _keyValuePairs[key];
            set => _keyValuePairs[key] = value;
        }
    }

    private class DefaultContext : TestContext<ChangeValueEntity>
    {
        public DefaultContext()
            : base(nameof(DefaultContext), false, true)
        {
        }
    }

    private class SnapshotContext : TestContext<ChangeValueEntity>
    {
        public SnapshotContext()
            : base(nameof(SnapshotContext), false, true, true, ChangeTrackingStrategy.Snapshot)
        {
        }
    }

    private class ChangedNotificationsContext : TestContext<ChangeValueEntity>
    {
        public ChangedNotificationsContext()
            : base(nameof(ChangedNotificationsContext), false, true, true, ChangeTrackingStrategy.ChangedNotifications)
        {
        }
    }

    private class ChangingAndChangedNotificationsContext : TestContext<ChangeValueEntity>
    {
        public ChangingAndChangedNotificationsContext()
            : base(
                nameof(ChangingAndChangedNotificationsContext), false, true, true,
                ChangeTrackingStrategy.ChangingAndChangedNotifications)
        {
        }
    }

    private class ChangingAndChangedNotificationsWithOriginalValuesContext : TestContext<ChangeValueEntity>
    {
        public ChangingAndChangedNotificationsWithOriginalValuesContext()
            : base(
                nameof(ChangingAndChangedNotificationsWithOriginalValuesContext), false, true, true,
                ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues)
        {
        }
    }
}
