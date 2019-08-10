// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    public class InternalEntryEntrySubscriberTest
    {
        [ConditionalTheory]
        [InlineData(ChangeTrackingStrategy.Snapshot)]
        [InlineData(ChangeTrackingStrategy.ChangedNotifications)]
        public void Original_and_relationship_values_recorded_when_no_changing_notifications(
            ChangeTrackingStrategy changeTrackingStrategy)
        {
            var entry = InMemoryTestHelpers.Instance.CreateInternalEntry<FullNotificationEntity>(
                BuildModel(changeTrackingStrategy));

            entry.SetEntityState(EntityState.Unchanged);

            Assert.True(entry.HasOriginalValuesSnapshot);
            Assert.True(entry.HasRelationshipSnapshot);
        }

        [ConditionalTheory]
        [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotifications)]
        [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues)]
        public void Original_and_relationship_values_not_recorded_when_full_notifications(
            ChangeTrackingStrategy changeTrackingStrategy)
        {
            var entry = InMemoryTestHelpers.Instance.CreateInternalEntry<FullNotificationEntity>(
                BuildModel(changeTrackingStrategy));

            entry.SetEntityState(EntityState.Unchanged);

            Assert.False(entry.HasOriginalValuesSnapshot);
            Assert.False(entry.HasRelationshipSnapshot);
        }

        [ConditionalFact]
        public void Notifying_collections_are_not_created_when_snapshot_tracking()
        {
            var entry = InMemoryTestHelpers.Instance.CreateInternalEntry<FullNotificationEntity>(
                BuildModel(ChangeTrackingStrategy.Snapshot));

            entry.SetEntityState(EntityState.Unchanged);

            Assert.Null(((FullNotificationEntity)entry.Entity).RelatedCollection);
        }

        [ConditionalTheory]
        [InlineData(ChangeTrackingStrategy.ChangedNotifications)]
        [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotifications)]
        [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues)]
        public void Notifying_collections_are_created_when_notification_tracking(
            ChangeTrackingStrategy changeTrackingStrategy)
        {
            var entry = InMemoryTestHelpers.Instance.CreateInternalEntry<FullNotificationEntity>(
                BuildModel(changeTrackingStrategy));

            entry.SetEntityState(EntityState.Unchanged);

            Assert.IsType<ObservableHashSet<ChangedOnlyNotificationEntity>>(
                ((FullNotificationEntity)entry.Entity).RelatedCollection);
        }

        [ConditionalFact]
        public void Non_notifying_collection_acceptable_when_snapshot_tracking()
        {
            var entry = InMemoryTestHelpers.Instance.CreateInternalEntry<FullNotificationEntity>(
                BuildModel(ChangeTrackingStrategy.Snapshot));

            var collection = new List<ChangedOnlyNotificationEntity>();
            ((FullNotificationEntity)entry.Entity).RelatedCollection = collection;

            entry.SetEntityState(EntityState.Unchanged);

            Assert.Same(collection, ((FullNotificationEntity)entry.Entity).RelatedCollection);
        }

        [ConditionalTheory]
        [InlineData(ChangeTrackingStrategy.ChangedNotifications)]
        [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotifications)]
        [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues)]
        public void Non_notifying_collections_not_acceptable_when_notification_tracking(
            ChangeTrackingStrategy changeTrackingStrategy)
        {
            var entry = InMemoryTestHelpers.Instance.CreateInternalEntry<FullNotificationEntity>(
                BuildModel(changeTrackingStrategy));

            ((FullNotificationEntity)entry.Entity).RelatedCollection = new List<ChangedOnlyNotificationEntity>();

            Assert.Equal(
                CoreStrings.NonNotifyingCollection("RelatedCollection", "FullNotificationEntity", changeTrackingStrategy),
                Assert.Throws<InvalidOperationException>(
                    () => entry.SetEntityState(EntityState.Unchanged)).Message);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Entry_subscribes_to_INotifyCollectionChanged_for_Add(bool ourCollection)
        {
            var collection = CreateCollection(ourCollection);
            var testListener = SetupTestCollectionListener(collection);

            var item = new ChangedOnlyNotificationEntity();
            collection.Add(item);

            Assert.Equal("RelatedCollection", testListener.CollectionChanged.Single().Item2.Name);
            Assert.Same(item, testListener.CollectionChanged.Single().Item3.Single());
            Assert.Empty(testListener.CollectionChanged.Single().Item4);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Entry_subscribes_to_INotifyCollectionChanged_for_Remove(bool ourCollection)
        {
            var item = new ChangedOnlyNotificationEntity();
            var collection = CreateCollection(ourCollection, item);
            var testListener = SetupTestCollectionListener(collection);

            collection.Remove(item);

            Assert.Equal("RelatedCollection", testListener.CollectionChanged.Single().Item2.Name);
            Assert.Empty(testListener.CollectionChanged.Single().Item3);
            Assert.Same(item, testListener.CollectionChanged.Single().Item4.Single());
        }

        [ConditionalFact]
        public void Entry_subscribes_to_INotifyCollectionChanged_for_Replace()
        {
            var item1 = new ChangedOnlyNotificationEntity();
            var collection = new ObservableCollection<ChangedOnlyNotificationEntity>
            {
                item1
            };
            var testListener = SetupTestCollectionListener(collection);

            var item2 = new ChangedOnlyNotificationEntity();
            collection[0] = item2;

            Assert.Equal("RelatedCollection", testListener.CollectionChanged.Single().Item2.Name);
            Assert.Same(item2, testListener.CollectionChanged.Single().Item3.Single());
            Assert.Same(item1, testListener.CollectionChanged.Single().Item4.Single());
        }

        [ConditionalFact]
        public void Entry_ignores_INotifyCollectionChanged_for_Move()
        {
            var item1 = new ChangedOnlyNotificationEntity();
            var item2 = new ChangedOnlyNotificationEntity();
            var collection = new ObservableCollection<ChangedOnlyNotificationEntity>
            {
                item1,
                item2
            };
            var testListener = SetupTestCollectionListener(collection);

            collection.Move(0, 1);

            Assert.Empty(testListener.CollectionChanged);
        }

        [ConditionalFact]
        public void Entry_throws_for_INotifyCollectionChanged_Reset()
        {
            var item1 = new ChangedOnlyNotificationEntity();
            var item2 = new ChangedOnlyNotificationEntity();
            var collection = new ObservableCollection<ChangedOnlyNotificationEntity>
            {
                item1,
                item2
            };
            var testListener = SetupTestCollectionListener(collection);

            Assert.Equal(
                CoreStrings.ResetNotSupported,
                Assert.Throws<InvalidOperationException>(() => collection.Clear()).Message);

            Assert.Empty(testListener.CollectionChanged);
        }

        [ConditionalFact]
        public void Entry_handles_clear_as_replace_with_ObservableHashSet()
        {
            var item1 = new ChangedOnlyNotificationEntity();
            var item2 = new ChangedOnlyNotificationEntity();
            var collection = new ObservableHashSet<ChangedOnlyNotificationEntity>
            {
                item1,
                item2
            };
            var testListener = SetupTestCollectionListener(collection);

            collection.Clear();

            Assert.Empty(collection);

            Assert.Equal("RelatedCollection", testListener.CollectionChanged.Single().Item2.Name);
            Assert.Empty(testListener.CollectionChanged.Single().Item3);
            Assert.Same(item1, testListener.CollectionChanged.Single().Item4.First());
            Assert.Same(item2, testListener.CollectionChanged.Single().Item4.Skip(1).Single());
        }

        private static ICollection<ChangedOnlyNotificationEntity> CreateCollection(
            bool ourCollection, params ChangedOnlyNotificationEntity[] items)
            => ourCollection
                ? (ICollection<ChangedOnlyNotificationEntity>)new ObservableHashSet<ChangedOnlyNotificationEntity>(items)
                : new ObservableCollection<ChangedOnlyNotificationEntity>(items);

        private static TestNavigationListener SetupTestCollectionListener(
            ICollection<ChangedOnlyNotificationEntity> collection)
        {
            var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(
                new ServiceCollection().AddScoped<INavigationFixer, TestNavigationListener>(),
                BuildModel());

            var testListener = contextServices
                .GetRequiredService<IEnumerable<INavigationFixer>>()
                .OfType<TestNavigationListener>()
                .Single();

            var entity = new FullNotificationEntity
            {
                RelatedCollection = collection
            };
            var entry = contextServices.GetRequiredService<IStateManager>().GetOrCreateEntry(entity);
            entry.SetEntityState(EntityState.Unchanged);

            return testListener;
        }

        [ConditionalFact]
        public void Entry_subscribes_to_INotifyPropertyChanging_and_INotifyPropertyChanged_for_properties()
        {
            var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(
                new ServiceCollection().AddScoped<IChangeDetector, TestPropertyListener>(),
                BuildModel());

            var testListener = contextServices.GetRequiredService<IEnumerable<IChangeDetector>>().OfType<TestPropertyListener>().Single();

            var entity = new FullNotificationEntity();
            var entry = contextServices.GetRequiredService<IStateManager>().GetOrCreateEntry(entity);
            entry.SetEntityState(EntityState.Unchanged);

            Assert.Empty(testListener.Changing);
            Assert.Empty(testListener.Changed);

            entity.Name = "Palmer";

            var property = entry.EntityType.FindProperty("Name");
            Assert.Same(property, testListener.Changing.Single().Item2);
            Assert.Same(property, testListener.Changed.Single().Item2);
        }

        [ConditionalFact]
        public void Entry_handles_null_or_empty_string_in_INotifyPropertyChanging_and_INotifyPropertyChanged()
        {
            var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(
                new ServiceCollection().AddScoped<IChangeDetector, TestPropertyListener>(),
                BuildModel());

            var testListener = contextServices.GetRequiredService<IEnumerable<IChangeDetector>>().OfType<TestPropertyListener>().Single();

            var entity = new FullNotificationEntity();
            var entry = contextServices.GetRequiredService<IStateManager>().GetOrCreateEntry(entity);
            entry.SetEntityState(EntityState.Unchanged);

            Assert.Empty(testListener.Changing);
            Assert.Empty(testListener.Changed);

            entity.NotifyChanging(null);

            Assert.Equal(
                new[] { "Name", "RelatedCollection" },
                testListener.Changing.Select(e => e.Item2.Name).OrderBy(e => e).ToArray());

            Assert.Empty(testListener.Changed);

            entity.NotifyChanged("");

            Assert.Equal(
                new[] { "Name", "RelatedCollection" },
                testListener.Changed.Select(e => e.Item2.Name).OrderBy(e => e).ToArray());
        }

        [ConditionalFact]
        public void Entry_subscribes_to_INotifyPropertyChanging_and_INotifyPropertyChanged_for_navigations()
        {
            var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(
                new ServiceCollection().AddScoped<IChangeDetector, TestPropertyListener>(),
                BuildModel());

            var testListener = contextServices.GetRequiredService<IEnumerable<IChangeDetector>>().OfType<TestPropertyListener>().Single();

            var entity = new FullNotificationEntity();
            var entry = contextServices.GetRequiredService<IStateManager>().GetOrCreateEntry(entity);
            entry.SetEntityState(EntityState.Unchanged);

            Assert.Empty(testListener.Changing);
            Assert.Empty(testListener.Changed);

            entity.RelatedCollection = new List<ChangedOnlyNotificationEntity>();

            var property = entry.EntityType.FindNavigation("RelatedCollection");
            Assert.Same(property, testListener.Changing.Single().Item2);
            Assert.Same(property, testListener.Changed.Single().Item2);
        }

        [ConditionalFact]
        public void Subscriptions_to_INotifyPropertyChanging_and_INotifyPropertyChanged_ignore_unmapped_properties()
        {
            var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(
                new ServiceCollection().AddScoped<IChangeDetector, TestPropertyListener>(),
                BuildModel());

            var testListener = contextServices.GetRequiredService<IEnumerable<IChangeDetector>>().OfType<TestPropertyListener>().Single();

            var entity = new FullNotificationEntity();
            contextServices.GetRequiredService<IStateManager>().GetOrCreateEntry(entity);

            Assert.Empty(testListener.Changing);
            Assert.Empty(testListener.Changed);

            entity.NotMapped = "Luckey";

            Assert.Empty(testListener.Changing);
            Assert.Empty(testListener.Changed);
        }

        [ConditionalFact]
        public void Entry_unsubscribes_to_INotifyPropertyChanging_and_INotifyPropertyChanged()
        {
            var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(
                new ServiceCollection().AddScoped<IChangeDetector, TestPropertyListener>(),
                BuildModel());

            var testListener = contextServices
                .GetRequiredService<IEnumerable<IChangeDetector>>()
                .OfType<TestPropertyListener>().Single();

            var entities = new List<FullNotificationEntity>();
            var entries = new List<InternalEntityEntry>();
            for (var i = 0; i < 10; i++)
            {
                entities.Add(
                    new FullNotificationEntity
                    {
                        Id = i + 1
                    });
                entries.Add(contextServices.GetRequiredService<IStateManager>().GetOrCreateEntry(entities[i]));
                entries[i].SetEntityState(EntityState.Unchanged);
            }

            var property = entries[0].EntityType.FindProperty("Name");

            Assert.Empty(testListener.Changing);
            Assert.Empty(testListener.Changed);

            entities[2].Name = "Palmer";
            entities[5].Name = "John";

            Assert.Equal(2, testListener.Changing.Count);
            Assert.Equal(2, testListener.Changed.Count);
            Assert.All(testListener.Changing, e => Assert.Same(e.Item2, property));
            Assert.All(testListener.Changed, e => Assert.Same(e.Item2, property));
            Assert.Same(entries[2], testListener.Changing.First().Item1);
            Assert.Same(entries[2], testListener.Changed.First().Item1);
            Assert.Same(entries[5], testListener.Changing.Skip(1).Single().Item1);
            Assert.Same(entries[5], testListener.Changed.Skip(1).Single().Item1);

            entries[5].SetEntityState(EntityState.Detached);

            entities[5].Name = "Carmack";

            Assert.Equal(2, testListener.Changing.Count);
            Assert.Equal(2, testListener.Changed.Count);

            entities[2].Name = "Luckey";

            Assert.Equal(3, testListener.Changing.Count);
            Assert.Equal(3, testListener.Changed.Count);
            Assert.All(testListener.Changing, e => Assert.Same(e.Item2, property));
            Assert.All(testListener.Changed, e => Assert.Same(e.Item2, property));
            Assert.Same(entries[2], testListener.Changing.Skip(2).Single().Item1);
            Assert.Same(entries[2], testListener.Changed.Skip(2).Single().Item1);
        }

        [ConditionalFact]
        public void Entry_unsubscribes_to_INotifyCollectionChanged()
        {
            var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(
                new ServiceCollection().AddScoped<INavigationFixer, TestNavigationListener>(),
                BuildModel());

            var testListener = contextServices
                .GetRequiredService<IEnumerable<INavigationFixer>>()
                .OfType<TestNavigationListener>()
                .Single();

            var entities = new List<FullNotificationEntity>();
            var entries = new List<InternalEntityEntry>();
            for (var i = 0; i < 10; i++)
            {
                entities.Add(
                    new FullNotificationEntity
                    {
                        Id = i + 1,
                        RelatedCollection = new ObservableHashSet<ChangedOnlyNotificationEntity>()
                    });
                entries.Add(contextServices.GetRequiredService<IStateManager>().GetOrCreateEntry(entities[i]));
                entries[i].SetEntityState(EntityState.Unchanged);
            }

            var navigation = entries[0].EntityType.FindNavigation("RelatedCollection");

            Assert.Empty(testListener.CollectionChanged);

            entities[2].RelatedCollection.Add(new ChangedOnlyNotificationEntity());
            entities[5].RelatedCollection.Add(new ChangedOnlyNotificationEntity());

            Assert.Equal(2, testListener.CollectionChanged.Count);
            Assert.All(testListener.CollectionChanged, e => Assert.Same(e.Item2, navigation));
            Assert.Same(entries[2], testListener.CollectionChanged.First().Item1);
            Assert.Same(entries[5], testListener.CollectionChanged.Skip(1).Single().Item1);

            entries[5].SetEntityState(EntityState.Detached);

            entities[5].RelatedCollection.Add(new ChangedOnlyNotificationEntity());

            Assert.Equal(2, testListener.CollectionChanged.Count);

            entities[2].RelatedCollection.Add(new ChangedOnlyNotificationEntity());

            Assert.Equal(3, testListener.CollectionChanged.Count);
            Assert.All(testListener.CollectionChanged, e => Assert.Same(e.Item2, navigation));
            Assert.Same(entries[2], testListener.CollectionChanged.Skip(2).Single().Item1);
        }

        [ConditionalFact]
        public void Entries_are_unsubscribed_when_context_is_disposed()
        {
            var context = InMemoryTestHelpers.Instance.CreateContext(
                new ServiceCollection().AddScoped<IChangeDetector, TestPropertyListener>(),
                BuildModel());

            var testListener = context
                .GetService<IEnumerable<IChangeDetector>>()
                .OfType<TestPropertyListener>().Single();

            var entities = new List<FullNotificationEntity>();
            var entries = new List<EntityEntry>();
            for (var i = 0; i < 10; i++)
            {
                entities.Add(
                    new FullNotificationEntity
                    {
                        Id = i + 1
                    });
                entries.Add(context.Add(entities[i]));
                entries[i].State = EntityState.Unchanged;
            }

            Assert.Empty(testListener.Changing);
            Assert.Empty(testListener.Changed);

            entities[2].Name = "Palmer";
            entities[5].Name = "John";

            Assert.Equal(2, testListener.Changing.Count);
            Assert.Equal(2, testListener.Changed.Count);

            context.Dispose();

            entities[5].Name = "Carmack";
            Assert.Equal(2, testListener.Changing.Count);
            Assert.Equal(2, testListener.Changed.Count);

            entities[2].Name = "Luckey";
            Assert.Equal(2, testListener.Changing.Count);
            Assert.Equal(2, testListener.Changed.Count);
        }

        private class TestPropertyListener : IChangeDetector
        {
            public List<Tuple<InternalEntityEntry, IPropertyBase>> Changing { get; } =
                new List<Tuple<InternalEntityEntry, IPropertyBase>>();

            public List<Tuple<InternalEntityEntry, IPropertyBase>> Changed { get; } = new List<Tuple<InternalEntityEntry, IPropertyBase>>();

            public void PropertyChanged(InternalEntityEntry entry, IPropertyBase property, bool setModified)
                => Changed.Add(Tuple.Create(entry, property));

            public void PropertyChanging(InternalEntityEntry entry, IPropertyBase property)
                => Changing.Add(Tuple.Create(entry, property));

            public void DetectChanges(IStateManager stateManager)
            {
            }

            public void DetectChanges(InternalEntityEntry entry)
            {
            }

            public void Suspend()
            {
            }

            public void Resume()
            {
            }
        }

        private class TestNavigationListener : INavigationFixer
        {
            public List<Tuple<InternalEntityEntry, INavigation, IEnumerable<object>, IEnumerable<object>>> CollectionChanged { get; }
                = new List<Tuple<InternalEntityEntry, INavigation, IEnumerable<object>, IEnumerable<object>>>();

            public void NavigationReferenceChanged(
                InternalEntityEntry entry, INavigation navigation, object oldValue, object newValue)
            {
            }

            public void NavigationCollectionChanged(
                InternalEntityEntry entry, INavigation navigation, IEnumerable<object> added, IEnumerable<object> removed)
                => CollectionChanged.Add(Tuple.Create(entry, navigation, added, removed));

            public void TrackedFromQuery(InternalEntityEntry entry)
            {
            }

            public void StateChanging(InternalEntityEntry entry, EntityState newState)
            {
            }

            public void StateChanged(InternalEntityEntry entry, EntityState oldState, bool fromQuery)
            {
            }

            public void KeyPropertyChanged(
                InternalEntityEntry entry, IProperty property, IReadOnlyList<IKey> containingPrincipalKeys, IReadOnlyList<IForeignKey> containingForeignKeys,
                object oldValue, object newValue)
            {
            }
        }

        private static IModel BuildModel(
            ChangeTrackingStrategy changeTrackingStrategy = ChangeTrackingStrategy.ChangingAndChangedNotifications)
        {
            var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

            builder.Entity<FullNotificationEntity>(
                b =>
                {
                    b.Ignore(e => e.NotMapped);
                    b.HasMany(e => e.RelatedCollection).WithOne(e => e.Related).HasForeignKey(e => e.Fk);
                    b.HasChangeTrackingStrategy(changeTrackingStrategy);
                });

            return builder.Model.FinalizeModel();
        }

        private class FullNotificationEntity : INotifyPropertyChanging, INotifyPropertyChanged
        {
            private int _id;
            private string _name;
            private string _notMapped;
            private ICollection<ChangedOnlyNotificationEntity> _relatedCollection;

            public int Id
            {
                get => _id;
                set => SetWithNotify(value, ref _id);
            }

            public string Name
            {
                get => _name;
                set => SetWithNotify(value, ref _name);
            }

            public string NotMapped
            {
                get => _notMapped;
                set => SetWithNotify(value, ref _notMapped);
            }

            public ICollection<ChangedOnlyNotificationEntity> RelatedCollection
            {
                get => _relatedCollection;
                set => SetWithNotify(value, ref _relatedCollection);
            }

            private void SetWithNotify<T>(T value, ref T field, [CallerMemberName] string propertyName = "")
            {
                if (!StructuralComparisons.StructuralEqualityComparer.Equals(field, value))
                {
                    NotifyChanging(propertyName);
                    field = value;
                    NotifyChanged(propertyName);
                }
            }

            public event PropertyChangingEventHandler PropertyChanging;
            public event PropertyChangedEventHandler PropertyChanged;

            public void NotifyChanged(string propertyName)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            public void NotifyChanging(string propertyName)
                => PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
        }

        private class ChangedOnlyNotificationEntity : INotifyPropertyChanged
        {
            private int _id;
            private string _name;
            private int _fk;
            private FullNotificationEntity _related;

            public int Id
            {
                get => _id;
                set => SetWithNotify(value, ref _id);
            }

            public string Name
            {
                get => _name;
                set => SetWithNotify(value, ref _name);
            }

            public int Fk
            {
                get => _fk;
                set => SetWithNotify(value, ref _fk);
            }

            public FullNotificationEntity Related
            {
                get => _related;
                set => SetWithNotify(value, ref _related);
            }

            private void SetWithNotify<T>(T value, ref T field, [CallerMemberName] string propertyName = "")
            {
                if (!StructuralComparisons.StructuralEqualityComparer.Equals(field, value))
                {
                    field = value;
                    NotifyChanged(propertyName);
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private void NotifyChanged(string propertyName)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
