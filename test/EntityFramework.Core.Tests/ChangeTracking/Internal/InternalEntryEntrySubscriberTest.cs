// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking.Internal
{
    public class InternalEntryEntrySubscriberTest
    {
        [Fact]
        public void Snapshots_are_created_for_entities_without_changing_notifications()
        {
            var entry = TestHelpers.Instance.CreateInternalEntry(
                BuildModel(),
                EntityState.Unchanged,
                new ChangedOnlyNotificationEntity { Name = "Palmer", Id = 1 });

            Assert.True(entry.TryGetSidecar(Sidecar.WellKnownNames.OriginalValues).HasValue(entry.EntityType.GetProperty("Name")));
            Assert.Equal("Palmer", entry.TryGetSidecar(Sidecar.WellKnownNames.OriginalValues)[entry.EntityType.GetProperty("Name")]);
            Assert.True(entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot).HasValue(entry.EntityType.GetProperty("Id")));
            Assert.Equal(1, entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot)[entry.EntityType.GetProperty("Id")]);
        }

        [Fact]
        public void Snapshots_are_not_created_for_full_notification_entities()
        {
            var entry = TestHelpers.Instance.CreateInternalEntry<FullNotificationEntity>(BuildModel());

            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.OriginalValues));
            // TODO: The following assert should be changed to Null once INotifyCollectionChanged is supported (Issue #445)
            Assert.NotNull(entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot));
        }

        [Fact]
        public void Relationship_snapshot_is_created_when_entity_has_non_notifying_collection_instance()
        {
            var entry = TestHelpers.Instance.CreateInternalEntry(
                BuildModel(),
                EntityState.Unchanged,
                new FullNotificationEntity { Name = "Palmer", Id = 1, RelatedCollection = new List<ChangedOnlyNotificationEntity>() });

            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.OriginalValues));

            Assert.False(entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot)
                .HasValue(entry.EntityType.GetProperty("Id")));

            Assert.True(entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot)
                .HasValue(entry.EntityType.GetNavigation("RelatedCollection")));

            Assert.NotNull(entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot)
                [entry.EntityType.GetNavigation("RelatedCollection")]);
        }

        [Fact]
        public void Relationship_snapshot_is_not_created_when_entity_has_notifying_collection()
        {
            var entry = TestHelpers.Instance.CreateInternalEntry(
                BuildModel(),
                EntityState.Unchanged,
                new FullNotificationEntity { Id = -1, Name = "Palmer", RelatedCollection = new ObservableCollection<ChangedOnlyNotificationEntity>() });

            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.OriginalValues));
            // TODO: The following assert should be changed to Null once INotifyCollectionChanged is supported (Issue #445)
            Assert.NotNull(entry.TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot));
        }

        [Fact]
        public void Entry_subscribes_to_INotifyPropertyChanging_and_INotifyPropertyChanged_for_properties()
        {
            var contextServices = TestHelpers.Instance.CreateContextServices(
                new ServiceCollection().AddScoped<IPropertyListener, TestPropertyListener>(),
                BuildModel());

            var testListener = contextServices.GetRequiredService<IEnumerable<IPropertyListener>>().OfType<TestPropertyListener>().Single();

            var entity = new FullNotificationEntity();
            var entry = contextServices.GetRequiredService<IStateManager>().GetOrCreateEntry(entity);

            Assert.Null(testListener.Changing);
            Assert.Null(testListener.Changed);

            entity.Name = "Palmer";

            var property = entry.EntityType.GetProperty("Name");
            Assert.Same(property, testListener.Changing);
            Assert.Same(property, testListener.Changed);
        }

        [Fact]
        public void Entry_subscribes_to_INotifyPropertyChanging_and_INotifyPropertyChanged_for_navigations()
        {
            var contextServices = TestHelpers.Instance.CreateContextServices(
                new ServiceCollection().AddScoped<IPropertyListener, TestPropertyListener>(),
                BuildModel());

            var testListener = contextServices.GetRequiredService<IEnumerable<IPropertyListener>>().OfType<TestPropertyListener>().Single();

            var entity = new FullNotificationEntity();
            var entry = contextServices.GetRequiredService<IStateManager>().GetOrCreateEntry(entity);

            Assert.Null(testListener.Changing);
            Assert.Null(testListener.Changed);

            entity.RelatedCollection = new List<ChangedOnlyNotificationEntity>();

            var property = entry.EntityType.GetNavigation("RelatedCollection");
            Assert.Same(property, testListener.Changing);
            Assert.Same(property, testListener.Changed);
        }

        [Fact]
        public void Subscriptions_to_INotifyPropertyChanging_and_INotifyPropertyChanged_ignore_unmapped_properties()
        {
            var contextServices = TestHelpers.Instance.CreateContextServices(
                new ServiceCollection().AddScoped<IPropertyListener, TestPropertyListener>(),
                BuildModel());

            var testListener = contextServices.GetRequiredService<IEnumerable<IPropertyListener>>().OfType<TestPropertyListener>().Single();

            var entity = new FullNotificationEntity();
            contextServices.GetRequiredService<IStateManager>().GetOrCreateEntry(entity);

            Assert.Null(testListener.Changing);
            Assert.Null(testListener.Changed);

            entity.NotMapped = "Luckey";

            Assert.Null(testListener.Changing);
            Assert.Null(testListener.Changed);
        }

        private class TestPropertyListener : IPropertyListener
        {
            public IPropertyBase Changing { get; set; }
            public IPropertyBase Changed { get; set; }

            public void PropertyChanged(InternalEntityEntry entry, IPropertyBase property)
            {
                Changed = property;
            }

            public void PropertyChanging(InternalEntityEntry entry, IPropertyBase property)
            {
                Changing = property;
            }
        }

        private static IModel BuildModel()
        {
            var builder = TestHelpers.Instance.CreateConventionBuilder();

            builder.Entity<FullNotificationEntity>(b =>
                {
                    b.Ignore(e => e.NotMapped);
                    b.HasMany(e => e.RelatedCollection).WithOne(e => e.Related).HasForeignKey(e => e.Fk);
                });

            return builder.Model;
        }

        private class FullNotificationEntity : INotifyPropertyChanging, INotifyPropertyChanged
        {
            private int _id;
            private string _name;
            private string _notMapped;
            private ICollection<ChangedOnlyNotificationEntity> _relatedCollection;

            public int Id
            {
                get { return _id; }
                set { SetWithNotify(value, ref _id); }
            }

            public string Name
            {
                get { return _name; }
                set { SetWithNotify(value, ref _name); }
            }

            public string NotMapped
            {
                get { return _notMapped; }
                set { SetWithNotify(value, ref _notMapped); }
            }

            public ICollection<ChangedOnlyNotificationEntity> RelatedCollection
            {
                get { return _relatedCollection; }
                set { SetWithNotify(value, ref _relatedCollection); }
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

            private void NotifyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            private void NotifyChanging(string propertyName)
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        private class ChangedOnlyNotificationEntity : INotifyPropertyChanged
        {
            private int _id;
            private string _name;
            private int _fk;
            private FullNotificationEntity _related;

            public int Id
            {
                get { return _id; }
                set { SetWithNotify(value, ref _id); }
            }

            public string Name
            {
                get { return _name; }
                set { SetWithNotify(value, ref _name); }
            }

            public int Fk
            {
                get { return _fk; }
                set { SetWithNotify(value, ref _fk); }
            }

            public FullNotificationEntity Related
            {
                get { return _related; }
                set { SetWithNotify(value, ref _related); }
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
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
