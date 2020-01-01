// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
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

        [ConditionalFact]
        public void Raises_changed_event_when_changed()
        {
            using var context = new ChangeContext<ChangeValueEntity>();
            context.Add(new ChangeValueEntity());
            context.SaveChanges();

            var eventRaised = false;

            var entity = context.Set<ChangeValueEntity>().Single();
            ((INotifyPropertyChanged)entity).PropertyChanged += (s, e) =>
            {
                eventRaised = true;

                Assert.Equal(entity, s);

                Assert.Equal(
                    nameof(ChangeValueEntity.Value),
                    e.PropertyName);

                Assert.Equal(
                    10,
                    ((ChangeValueEntity)s).Value);
            };

            entity.Value = 10;
            Assert.True(eventRaised);
        }

        [ConditionalFact]
        public void Raises_changing_event_before_change()
        {
            using var context = new ChangeContext<ChangeValueEntity>();
            context.Add(new ChangeValueEntity { Value = 5 });
            context.SaveChanges();

            var eventRaised = false;

            var entity = context.Set<ChangeValueEntity>().Single();
            ((INotifyPropertyChanging)entity).PropertyChanging += (s, e) =>
            {
                eventRaised = true;

                Assert.Equal(entity, s);

                Assert.Equal(
                    nameof(ChangeValueEntity.Value),
                    e.PropertyName);

                Assert.Equal(
                    5,
                    ((ChangeValueEntity)s).Value);
            };

            entity.Value = 10;
            Assert.True(eventRaised);
        }

        [ConditionalFact]
        public void Doesnt_raise_change_event_when_equal_and_check_equality_true()
        {
            using var context = new ChangeContext<ChangeValueEntity>(checkEquality: true);
            context.Add(new ChangeValueEntity { Value = 10 });
            context.SaveChanges();

            var eventRaised = false;

            var entity = context.Set<ChangeValueEntity>().Single();
            ((INotifyPropertyChanged)entity).PropertyChanged += (s, e) =>
            {
                eventRaised = true;
            };

            entity.Value = 10;
            Assert.False(eventRaised);
        }

        [ConditionalFact]
        public void Doesnt_raise_changing_event_when_equal_and_check_equality_true()
        {
            using var context = new ChangeContext<ChangeValueEntity>(checkEquality: true);
            context.Add(new ChangeValueEntity { Value = 10 });
            context.SaveChanges();

            var eventRaised = false;

            var entity = context.Set<ChangeValueEntity>().Single();
            ((INotifyPropertyChanging)entity).PropertyChanging += (s, e) =>
            {
                eventRaised = true;
            };

            entity.Value = 10;
            Assert.False(eventRaised);
        }

        [ConditionalFact]
        public void Raises_change_event_when_equal_and_check_equality_false()
        {
            using var context = new ChangeContext<ChangeValueEntity>(checkEquality: false);
            context.Add(new ChangeValueEntity { Value = 10 });
            context.SaveChanges();

            var eventRaised = false;

            var entity = context.Set<ChangeValueEntity>().Single();
            ((INotifyPropertyChanged)entity).PropertyChanged += (s, e) =>
            {
                eventRaised = true;
            };

            entity.Value = 10;
            Assert.True(eventRaised);
        }

        [ConditionalFact]
        public void Raises_changing_event_when_equal_and_check_equality_false()
        {
            using var context = new ChangeContext<ChangeValueEntity>(checkEquality: false);
            context.Add(new ChangeValueEntity { Value = 10 });
            context.SaveChanges();

            var eventRaised = false;

            var entity = context.Set<ChangeValueEntity>().Single();
            ((INotifyPropertyChanging)entity).PropertyChanging += (s, e) =>
            {
                eventRaised = true;
            };

            entity.Value = 10;
            Assert.True(eventRaised);
        }

        private class ChangeContext<TEntity> : TestContext<TEntity>
            where TEntity : class
        {
            public ChangeContext(bool checkEquality = true)
                : base(dbName: "ChangeDetectionContext", useLazyLoading: false, useChangeDetection: true, checkEquality: checkEquality)
            {
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
