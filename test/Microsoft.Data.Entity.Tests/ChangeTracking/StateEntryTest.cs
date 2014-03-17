// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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
            var entityTypeMock = CreateEntityTypeMock();
            var entry = CreateStateEntry(CreateManagerMock(entityTypeMock).Object, entityTypeMock.Object, new Random());

            Assert.Equal(
                "property",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => entry.IsPropertyModified(null)).ParamName);

            Assert.Equal(
                "property",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => entry.SetPropertyModified(null, true)).ParamName);
        }

        [Fact]
        public void Changing_state_from_Unknown_causes_entity_to_start_tracking()
        {
            var entityTypeMock = CreateEntityTypeMock();
            var managerMock = CreateManagerMock(entityTypeMock);
            var entry = CreateStateEntry(managerMock.Object, entityTypeMock.Object, new Random());

            entry.SetEntityStateAsync(EntityState.Added, CancellationToken.None).Wait();

            managerMock.Verify(m => m.StartTracking(entry));
            Assert.Equal(EntityState.Added, entry.EntityState);
        }

        [Fact]
        public void Changing_state_to_Unknown_causes_entity_to_stop_tracking()
        {
            var entityTypeMock = CreateEntityTypeMock();
            var managerMock = CreateManagerMock(entityTypeMock);
            var entry = CreateStateEntry(managerMock.Object, entityTypeMock.Object, new Random());

            entry.SetEntityStateAsync(EntityState.Added, CancellationToken.None).Wait();
            entry.SetEntityStateAsync(EntityState.Unknown, CancellationToken.None).Wait();

            managerMock.Verify(m => m.StopTracking(entry));
            Assert.Equal(EntityState.Unknown, entry.EntityState);
        }

        [Fact]
        public void Changing_state_to_Modified_or_Unchanged_causes_all_properties_to_be_marked_accordingly()
        {
            var keyMock = new Mock<IProperty>();
            var nonKeyMock = new Mock<IProperty>();
            var entityTypeMock = CreateEntityTypeMock(keyMock, nonKeyMock);
            var managerMock = CreateManagerMock(entityTypeMock);
            var entry = CreateStateEntry(managerMock.Object, entityTypeMock.Object, new Random());

            Assert.False(entry.IsPropertyModified(keyMock.Object));
            Assert.False(entry.IsPropertyModified(nonKeyMock.Object));

            entry.SetEntityStateAsync(EntityState.Modified, CancellationToken.None).Wait();

            Assert.True(entry.IsPropertyModified(keyMock.Object));
            Assert.True(entry.IsPropertyModified(nonKeyMock.Object));

            entry.SetEntityStateAsync(EntityState.Unchanged, CancellationToken.None).Wait();

            Assert.False(entry.IsPropertyModified(keyMock.Object));
            Assert.False(entry.IsPropertyModified(nonKeyMock.Object));
        }

        [Fact]
        public void Changing_state_to_Added_triggers_key_generation()
        {
            var keyMock = new Mock<IProperty>();
            var entityTypeMock = CreateEntityTypeMock(keyMock);
            var managerMock = CreateManagerMock(entityTypeMock);

            var keyValue = new object();
            var generatorMock = new Mock<IIdentityGenerator>();
            generatorMock.Setup(m => m.NextAsync(CancellationToken.None)).Returns(Task.FromResult(keyValue));

            managerMock.Setup(m => m.GetIdentityGenerator(keyMock.Object)).Returns(generatorMock.Object);

            var setterMock = new Mock<IClrPropertySetter>();
            managerMock.Setup(m => m.GetClrPropertySetter(keyMock.Object)).Returns(setterMock.Object);

            var entity = new Random();
            var entry = CreateStateEntry(managerMock.Object, entityTypeMock.Object, entity);
            entry.SetEntityStateAsync(EntityState.Added, CancellationToken.None).Wait();

            if (keyMock.Object.IsClrProperty)
            {
                setterMock.Verify(m => m.SetClrValue(entity, keyValue));
            }
            else
            {
                Assert.Same(keyValue, entry.GetPropertyValue(keyMock.Object));
            }
        }

        [Fact]
        public void Can_create_primary_key()
        {
            var propertyMock1 = new Mock<IProperty>();

            var entityTypeMock = CreateEntityTypeMock(propertyMock1);

            var managerMock = new Mock<StateManager>();
            managerMock.Setup(m => m.Model).Returns(Mock.Of<IModel>());
            managerMock
                .Setup(m => m.CreateKey(It.IsAny<IEntityType>(), It.IsAny<IReadOnlyList<IProperty>>(), It.IsAny<StateEntry>()))
                .Returns(new SimpleEntityKey<string>(entityTypeMock.Object, "Atmosphere"));

            var getterMock = new Mock<IClrPropertyGetter>();
            getterMock.Setup(m => m.GetClrValue(It.IsAny<object>())).Returns("Atmosphere");
            managerMock.Setup(m => m.GetClrPropertyGetter(propertyMock1.Object)).Returns(getterMock.Object);

            var setterMock = new Mock<IClrPropertySetter>();
            managerMock.Setup(m => m.GetClrPropertySetter(propertyMock1.Object)).Returns(setterMock.Object);

            var entry = CreateStateEntry(managerMock.Object, entityTypeMock.Object, new Random());
            entry.SetPropertyValue(propertyMock1.Object, "Atmosphere");

            var keyValue = entry.GetPrimaryKeyValue();
            Assert.IsType<SimpleEntityKey<string>>(keyValue);
            Assert.Equal("Atmosphere", keyValue.Value);

            managerMock.Verify(m => m.CreateKey(entityTypeMock.Object, entityTypeMock.Object.GetKey().Properties, entry));
        }

        [Fact]
        public void Can_create_foreign_key_value_based_on_dependent_values()
        {
            var principalProp = new Mock<IProperty>();
            var dependentProp = new Mock<IProperty>();

            var principalProps = new[] { principalProp.Object };
            var dependentProps = new[] { dependentProp.Object };

            var principalTypeMock = CreateEntityTypeMock(new Mock<IProperty>(), principalProp);
            var dependentTypeMock = CreateEntityTypeMock(new Mock<IProperty>(), dependentProp);

            var managerMock = new Mock<StateManager>();
            managerMock.Setup(m => m.Model).Returns(Mock.Of<IModel>());
            managerMock
                .Setup(m => m.CreateKey(principalTypeMock.Object, It.IsAny<IReadOnlyList<IProperty>>(), It.IsAny<StateEntry>()))
                .Returns(new SimpleEntityKey<string>(principalTypeMock.Object, "On"));

            var getterMock1 = new Mock<IClrPropertyGetter>();
            getterMock1.Setup(m => m.GetClrValue(It.IsAny<object>())).Returns("Wax");

            var getterMock2 = new Mock<IClrPropertyGetter>();
            getterMock2.Setup(m => m.GetClrValue(It.IsAny<object>())).Returns("On");

            managerMock.Setup(m => m.GetClrPropertyGetter(principalProp.Object)).Returns(getterMock1.Object);
            managerMock.Setup(m => m.GetClrPropertyGetter(dependentProp.Object)).Returns(getterMock2.Object);

            var setterMock = new Mock<IClrPropertySetter>();
            managerMock.Setup(m => m.GetClrPropertySetter(principalProp.Object)).Returns(setterMock.Object);
            managerMock.Setup(m => m.GetClrPropertySetter(dependentProp.Object)).Returns(setterMock.Object);

            var foreignKeyMock = new Mock<IForeignKey>();
            foreignKeyMock.Setup(m => m.ReferencedEntityType).Returns(principalTypeMock.Object);
            foreignKeyMock.Setup(m => m.EntityType).Returns(dependentTypeMock.Object);
            foreignKeyMock.Setup(m => m.ReferencedProperties).Returns(principalProps);
            foreignKeyMock.Setup(m => m.Properties).Returns(dependentProps);

            var entry = CreateStateEntry(managerMock.Object, dependentTypeMock.Object, new Random());
            entry.SetPropertyValue(dependentProp.Object, "On");

            var keyValue = entry.GetDependentKeyValue(foreignKeyMock.Object);
            Assert.IsType<SimpleEntityKey<string>>(keyValue);
            Assert.Equal("On", keyValue.Value);

            managerMock.Verify(m => m.CreateKey(principalTypeMock.Object, foreignKeyMock.Object.Properties, entry));
        }

        [Fact]
        public void Can_create_foreign_key_value_based_on_principal_end_values()
        {
            var principalProp = new Mock<IProperty>();
            var dependentProp = new Mock<IProperty>();

            var principalProps = new[] { principalProp.Object };
            var dependentProps = new[] { dependentProp.Object };

            var principalTypeMock = CreateEntityTypeMock(new Mock<IProperty>(), principalProp);
            var dependentTypeMock = CreateEntityTypeMock(new Mock<IProperty>(), dependentProp);

            var managerMock = new Mock<StateManager>();
            managerMock.Setup(m => m.Model).Returns(Mock.Of<IModel>());
            managerMock
                .Setup(m => m.CreateKey(principalTypeMock.Object, It.IsAny<IReadOnlyList<IProperty>>(), It.IsAny<StateEntry>()))
                .Returns(new SimpleEntityKey<string>(dependentTypeMock.Object, "Wax"));

            var getterMock1 = new Mock<IClrPropertyGetter>();
            getterMock1.Setup(m => m.GetClrValue(It.IsAny<object>())).Returns("Wax");

            var getterMock2 = new Mock<IClrPropertyGetter>();
            getterMock2.Setup(m => m.GetClrValue(It.IsAny<object>())).Returns("Off");

            managerMock.Setup(m => m.GetClrPropertyGetter(principalProp.Object)).Returns(getterMock1.Object);
            managerMock.Setup(m => m.GetClrPropertyGetter(dependentProp.Object)).Returns(getterMock2.Object);

            var setterMock = new Mock<IClrPropertySetter>();
            managerMock.Setup(m => m.GetClrPropertySetter(principalProp.Object)).Returns(setterMock.Object);
            managerMock.Setup(m => m.GetClrPropertySetter(dependentProp.Object)).Returns(setterMock.Object);

            var foreignKeyMock = new Mock<IForeignKey>();
            foreignKeyMock.Setup(m => m.ReferencedEntityType).Returns(principalTypeMock.Object);
            foreignKeyMock.Setup(m => m.EntityType).Returns(dependentTypeMock.Object);
            foreignKeyMock.Setup(m => m.ReferencedProperties).Returns(principalProps);
            foreignKeyMock.Setup(m => m.Properties).Returns(dependentProps);

            var entry = CreateStateEntry(managerMock.Object, principalTypeMock.Object, new Random());
            entry.SetPropertyValue(principalProp.Object, "Wax");

            var keyValue = entry.GetPrincipalKeyValue(foreignKeyMock.Object);
            Assert.IsType<SimpleEntityKey<string>>(keyValue);
            Assert.Equal("Wax", keyValue.Value);

            managerMock.Verify(m => m.CreateKey(principalTypeMock.Object, foreignKeyMock.Object.ReferencedProperties, entry));
        }

        protected virtual StateEntry CreateStateEntry(StateManager stateManager, IEntityType entityType, object entity)
        {
            return new MixedStateEntry(stateManager, entityType, entity);
        }

        protected virtual Mock<StateManager> CreateManagerMock(Mock<IEntityType> entityTypeMock)
        {
            var modelMock = new Mock<IModel>();
            modelMock.Setup(m => m.GetEntityType(typeof(Random))).Returns(entityTypeMock.Object);

            var managerMock = new Mock<StateManager>();
            managerMock.Setup(m => m.Model).Returns(modelMock.Object);
            return managerMock;
        }

        protected virtual Mock<IEntityType> CreateEntityTypeMock(Mock<IProperty> key = null, Mock<IProperty> nonKey = null)
        {
            key = key ?? new Mock<IProperty>();
            key.Setup(m => m.Index).Returns(0);
            key.Setup(m => m.IsClrProperty).Returns(true);
            var keys = new[] { key.Object };
            nonKey = nonKey ?? new Mock<IProperty>();
            nonKey.Setup(m => m.Index).Returns(1);
            nonKey.Setup(m => m.IsClrProperty).Returns(true);

            var entityTypeMock = new Mock<IEntityType>();
            entityTypeMock.Setup(m => m.GetKey().Properties).Returns(keys);
            entityTypeMock.Setup(m => m.Properties).Returns(keys.Concat(new[] { nonKey.Object }).ToArray());

            return entityTypeMock;
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
