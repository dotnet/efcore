// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking
{
    public class StateEntryTest
    {
        [Fact]
        public void Members_check_arguments()
        {
            Assert.Equal(
                "stateManager",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => new StateEntry(null, new Random())).ParamName);
            Assert.Equal(
                "entity",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => new StateEntry(new Mock<StateManager>().Object, null)).ParamName);

            var entry = new StateEntry(CreateManagerMock().Object, new Random());

            Assert.Equal(
                Strings.FormatArgumentIsEmpty("propertyName"),
                Assert.Throws<ArgumentException>(() => entry.IsPropertyModified("")).Message);

            Assert.Equal(
                Strings.FormatArgumentIsEmpty("propertyName"),
                Assert.Throws<ArgumentException>(() => entry.SetPropertyModified("", true)).Message);
        }

        [Fact]
        public void Constructor_throws_for_entity_not_in_the_model()
        {
            Assert.Equal(
                Strings.FormatEntityTypeNotFound("System.Random"),
                Assert.Throws<InvalidOperationException>(
                    () =>
                        new StateEntry(
                            new StateManager(
                                new Model(), new Mock<ActiveIdentityGenerators>().Object, Enumerable.Empty<IEntityStateListener>()),
                            new Random())).Message);
        }

        [Fact]
        public void Can_get_entity()
        {
            var entity = new Random();
            Assert.Same(entity, new StateEntry(CreateManagerMock().Object, entity).Entity);
        }

        [Fact]
        public void Can_get_entity_type()
        {
            var entityTypeMock = new Mock<IEntityType>();
            Assert.Same(entityTypeMock.Object, new StateEntry(CreateManagerMock(entityTypeMock).Object, new Random()).EntityType);
        }

        [Fact]
        public void Changing_state_from_Unknown_causes_entity_to_start_tracking()
        {
            var managerMock = CreateManagerMock();
            var entry = new StateEntry(managerMock.Object, new Random());

            entry.SetEntityStateAsync(EntityState.Added, CancellationToken.None).Wait();

            managerMock.Verify(m => m.StartTracking(entry));
            Assert.Equal(EntityState.Added, entry.EntityState);
        }

        [Fact]
        public void Changing_state_to_Unknown_causes_entity_to_stop_tracking()
        {
            var managerMock = CreateManagerMock();
            var entry = new StateEntry(managerMock.Object, new Random());

            entry.SetEntityStateAsync(EntityState.Added, CancellationToken.None).Wait();
            entry.SetEntityStateAsync(EntityState.Unknown, CancellationToken.None).Wait();

            managerMock.Verify(m => m.StopTracking(entry));
            Assert.Equal(EntityState.Unknown, entry.EntityState);
        }

        [Fact]
        public void Changing_state_to_Modified_or_Unchanged_causes_all_properties_to_be_marked_accordingly()
        {
            var entry = new StateEntry(CreateManagerMock().Object, new Random());

            Assert.False(entry.IsPropertyModified("Foo"));
            Assert.False(entry.IsPropertyModified("Goo"));

            entry.SetEntityStateAsync(EntityState.Modified, CancellationToken.None).Wait();

            Assert.True(entry.IsPropertyModified("Foo"));
            Assert.True(entry.IsPropertyModified("Goo"));

            entry.SetEntityStateAsync(EntityState.Unchanged, CancellationToken.None).Wait();

            Assert.False(entry.IsPropertyModified("Foo"));
            Assert.False(entry.IsPropertyModified("Goo"));
        }

        [Fact]
        public void Changing_state_to_Added_triggers_key_generation()
        {
            var keyMock = new Mock<IProperty>();
            var managerMock = CreateManagerMock(new Mock<IEntityType>(), keyMock);

            var keyValue = new object();
            var generatorMock = new Mock<IIdentityGenerator>();
            generatorMock.Setup(m => m.NextAsync(CancellationToken.None)).Returns(Task.FromResult(keyValue));

            managerMock.Setup(m => m.GetIdentityGenerator(keyMock.Object)).Returns(generatorMock.Object);

            var entity = new Random();
            var entry = new StateEntry(managerMock.Object, entity);
            entry.SetEntityStateAsync(EntityState.Added, CancellationToken.None).Wait();

            keyMock.Verify(m => m.SetValue(entity, keyValue));
        }

        private static Mock<StateManager> CreateManagerMock(Mock<IEntityType> entityTypeMock = null, Mock<IProperty> key = null)
        {
            key = key ?? new Mock<IProperty>();
            var keys = new[] { key.Object };

            entityTypeMock = entityTypeMock ?? new Mock<IEntityType>();
            entityTypeMock.Setup(m => m.Key).Returns(keys);
            entityTypeMock.Setup(m => m.Properties).Returns(keys.Concat(new[] { new Mock<IProperty>().Object }).ToArray());
            entityTypeMock.Setup(m => m.PropertyIndex("Foo")).Returns(0);
            entityTypeMock.Setup(m => m.PropertyIndex("Goo")).Returns(1);

            var modelMock = new Mock<IModel>();
            modelMock.Setup(m => m.GetEntityType(typeof(Random))).Returns(entityTypeMock.Object);

            var managerMock = new Mock<StateManager>();
            managerMock.Setup(m => m.Model).Returns(modelMock.Object);
            return managerMock;
        }

        public class StateDataTest
        {
            [Fact]
            public void Can_read_and_manipulate_property_state()
            {
                for (var i = 0; i < 70; i++)
                {
                    PropertyManipulation(i);
                }
            }

            public void PropertyManipulation(int propertyCount)
            {
                var data = new StateEntry.StateData(propertyCount);

                Assert.False(data.AnyPropertiesModified());

                for (var i = 0; i < propertyCount; i++)
                {
                    data.SetPropertyModified(i, true);

                    for (var j = 0; j < propertyCount; j++)
                    {
                        Assert.Equal(j <= i, data.IsPropertyModified(j));
                    }

                    Assert.True(data.AnyPropertiesModified());
                }

                for (var i = 0; i < propertyCount; i++)
                {
                    data.SetPropertyModified(i, false);

                    for (var j = 0; j < propertyCount; j++)
                    {
                        Assert.Equal(j > i, data.IsPropertyModified(j));
                    }

                    Assert.Equal(i < propertyCount - 1, data.AnyPropertiesModified());
                }

                for (var i = 0; i < propertyCount; i++)
                {
                    Assert.False(data.IsPropertyModified(i));
                }

                data.SetAllPropertiesModified(propertyCount);

                Assert.Equal(propertyCount > 0, data.AnyPropertiesModified());

                for (var i = 0; i < propertyCount; i++)
                {
                    Assert.True(data.IsPropertyModified(i));
                }
            }

            [Fact]
            public void Can_get_and_set_EntityState()
            {
                var data = new StateEntry.StateData(70);

                Assert.Equal(EntityState.Unknown, data.EntityState);

                data.EntityState = EntityState.Unchanged;
                Assert.Equal(EntityState.Unchanged, data.EntityState);

                data.EntityState = EntityState.Modified;
                Assert.Equal(EntityState.Modified, data.EntityState);

                data.EntityState = EntityState.Added;
                Assert.Equal(EntityState.Added, data.EntityState);

                data.EntityState = EntityState.Deleted;
                Assert.Equal(EntityState.Deleted, data.EntityState);

                data.SetAllPropertiesModified(70);

                Assert.Equal(EntityState.Deleted, data.EntityState);

                data.EntityState = EntityState.Unchanged;
                Assert.Equal(EntityState.Unchanged, data.EntityState);

                data.EntityState = EntityState.Modified;
                Assert.Equal(EntityState.Modified, data.EntityState);

                data.EntityState = EntityState.Added;
                Assert.Equal(EntityState.Added, data.EntityState);

                data.EntityState = EntityState.Unknown;
                Assert.Equal(EntityState.Unknown, data.EntityState);
            }
        }
    }
}
