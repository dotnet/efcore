// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
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
                context.Model.GetChangeTrackingStrategy());
        }

        [ConditionalFact]
        public void Default_change_tracking_strategy_doesnt_overwrite_entity_strategy()
        {
            using var context = new ChangeContext<ChangeValueEntity>(
                entityBuilderAction: b =>
                {
                    b.HasChangeTrackingStrategy(ChangeTrackingStrategy.Snapshot);
                });

            var entityType = context.Model.FindEntityType(typeof(ChangeValueEntity));
            Assert.Equal(
                ChangeTrackingStrategy.Snapshot,
                entityType.GetChangeTrackingStrategy());
        }

        private static readonly Type changeInterface = typeof(INotifyPropertyChanged);
        private static readonly Type changingInterface = typeof(INotifyPropertyChanging);

        [ConditionalFact]
        public void Proxies_correct_interfaces_for_Snapshot()
        {
            using var context = new ProxyGenerationContext(ChangeTrackingStrategy.Snapshot);
            var proxy = context.CreateProxy<ChangeValueEntity>();
            var proxyType = proxy.GetType();

            Assert.False(changeInterface.IsAssignableFrom(proxyType));
            Assert.False(changingInterface.IsAssignableFrom(proxyType));
        }

        [ConditionalFact]
        public void Proxies_correct_interfaces_for_ChangedNotifications()
        {
            using var context = new ProxyGenerationContext(ChangeTrackingStrategy.ChangedNotifications);
            var proxy = context.CreateProxy<ChangeValueEntity>();
            var proxyType = proxy.GetType();

            Assert.True(changeInterface.IsAssignableFrom(proxyType));
            Assert.False(changingInterface.IsAssignableFrom(proxyType));
        }

        [ConditionalFact]
        public void Proxies_correct_interfaces_for_ChangingAndChangedNotifications()
        {
            using var context = new ProxyGenerationContext(ChangeTrackingStrategy.ChangingAndChangedNotifications);
            var proxy = context.CreateProxy<ChangeValueEntity>();
            var proxyType = proxy.GetType();

            Assert.True(changeInterface.IsAssignableFrom(proxyType));
            Assert.True(changingInterface.IsAssignableFrom(proxyType));
        }

        [ConditionalFact]
        public void Proxies_correct_interfaces_for_ChangingAndChangedNotificationsWithOriginalValues()
        {
            using var context = new ProxyGenerationContext(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues);
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

        private class ChangeContext<TEntity> : TestContext<TEntity>
            where TEntity : class
        {
            private readonly Action<EntityTypeBuilder<TEntity>> _entityBuilderAction;

            public ChangeContext(bool useLazyLoading = false, bool checkEquality = true, Action<EntityTypeBuilder<TEntity>> entityBuilderAction = null)
                : base(dbName: "ChangeDetectionContext", useLazyLoading: useLazyLoading, useChangeDetection: true, checkEquality: checkEquality)
            {
                _entityBuilderAction = entityBuilderAction;
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                var builder = modelBuilder.Entity<TEntity>();
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

        private class ProxyGenerationContext : TestContext<ChangeValueEntity>
        {
            public ProxyGenerationContext(
                ChangeTrackingStrategy changeTrackingStrategy)
                : base("ProxyGenerationContext", false, true, true, changeTrackingStrategy)
            {
            }
        }
    }
}
