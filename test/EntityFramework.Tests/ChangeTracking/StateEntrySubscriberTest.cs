// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class StateEntrySubscriberTest
    {
        [Fact]
        public void Snapshot_is_performed_when_not_using_eager_original_values()
        {
            var entityTypeMock = new Mock<IEntityType>();
            entityTypeMock.Setup(m => m.UseLazyOriginalValues).Returns(false);

            var originalValuesMock = new Mock<OriginalValues>();
            var fkSnapshotMock = new Mock<RelationshipsSnapshot>();
            var entryMock = new Mock<StateEntry>();
            entryMock.Setup(m => m.EntityType).Returns(entityTypeMock.Object);
            entryMock.Setup(m => m.OriginalValues).Returns(originalValuesMock.Object);
            entryMock.Setup(m => m.RelationshipsSnapshot).Returns(fkSnapshotMock.Object);

            CreateSubscriber().SnapshotAndSubscribe(entryMock.Object);

            originalValuesMock.Verify(m => m.TakeSnapshot());
            fkSnapshotMock.Verify(m => m.TakeSnapshot());
        }

        [Fact]
        public void Snapshot_is_not_performed_when_not_using_lazy_original_values()
        {
            var entityTypeMock = new Mock<IEntityType>();
            entityTypeMock.Setup(m => m.UseLazyOriginalValues).Returns(true);

            var originalValuesMock = new Mock<OriginalValues>();
            var entryMock = new Mock<StateEntry>();
            entryMock.Setup(m => m.EntityType).Returns(entityTypeMock.Object);
            entryMock.Setup(m => m.OriginalValues).Returns(originalValuesMock.Object);

            CreateSubscriber().SnapshotAndSubscribe(entryMock.Object);

            originalValuesMock.Verify(m => m.TakeSnapshot(), Times.Never);
        }

        [Fact]
        public void Entry_subscribes_to_INotifyPropertyChanging_and_INotifyPropertyChanged()
        {
            var entityType = new EntityType(typeof(FullNotificationEntity));
            var property = entityType.AddProperty("Name", typeof(string));

            var entity = new FullNotificationEntity();

            var entryMock = new Mock<StateEntry>();
            entryMock.Setup(m => m.EntityType).Returns(entityType);
            entryMock.Setup(m => m.Entity).Returns(entity);

            var detectorMock = new Mock<ChangeDetector>();
            new StateEntrySubscriber(detectorMock.Object).SnapshotAndSubscribe(entryMock.Object);

            entity.Name = "George";

            detectorMock.Verify(m => m.PropertyChanging(entryMock.Object, property));
            detectorMock.Verify(m => m.PropertyChanged(entryMock.Object, property));
        }

        [Fact]
        public void Subscriptions_to_INotifyPropertyChanging_and_INotifyPropertyChanged_ignore_unmapped_properties()
        {
            var entityType = new EntityType(typeof(FullNotificationEntity));
            entityType.AddProperty("Name", typeof(string));

            var entity = new FullNotificationEntity();

            var entryMock = new Mock<StateEntry>();
            entryMock.Setup(m => m.EntityType).Returns(entityType);
            entryMock.Setup(m => m.Entity).Returns(entity);

            var detectorMock = new Mock<ChangeDetector>();
            new StateEntrySubscriber(detectorMock.Object).SnapshotAndSubscribe(entryMock.Object);

            entity.NotMapped = "Formby";

            detectorMock.Verify(m => m.PropertyChanging(It.IsAny<StateEntry>(), It.IsAny<IProperty>()), Times.Never);
            detectorMock.Verify(m => m.PropertyChanged(It.IsAny<StateEntry>(), It.IsAny<IProperty>()), Times.Never);
        }

        private static StateEntrySubscriber CreateSubscriber()
        {
            return new StateEntrySubscriber(new ChangeDetector(Mock.Of<DbContextConfiguration>(), Mock.Of<StateEntryNotifier>()));
        }

        private class FullNotificationEntity : INotifyPropertyChanging, INotifyPropertyChanged
        {
            private string _name;

            public string Name
            {
                get { return _name; }
                set
                {
                    if (_name != value)
                    {
                        NotifyChanging();
                        _name = value;
                        NotifyChanged();
                    }
                }
            }

            private string _notMapped;

            public string NotMapped
            {
                get { return _notMapped; }
                set
                {
                    if (_notMapped != value)
                    {
                        NotifyChanging();
                        _notMapped = value;
                        NotifyChanged();
                    }
                }
            }

            public event PropertyChangingEventHandler PropertyChanging;
            public event PropertyChangedEventHandler PropertyChanged;

            private void NotifyChanged([CallerMemberName] String propertyName = "")
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }

            private void NotifyChanging([CallerMemberName] String propertyName = "")
            {
                if (PropertyChanging != null)
                {
                    PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
                }
            }
        }
    }
}
