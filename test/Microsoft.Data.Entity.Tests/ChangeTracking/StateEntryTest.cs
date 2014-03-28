// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
        public void Changing_state_from_Unknown_causes_entity_to_start_tracking()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType("SomeEntity");
            var keyProperty = entityType.GetProperty("Id");

            var configuration = CreateConfiguration(model);

            var entry = CreateStateEntry(configuration, entityType, new SomeEntity());
            entry.SetPropertyValue(keyProperty, 1);

            entry.SetEntityStateAsync(EntityState.Added, CancellationToken.None).Wait();

            Assert.Equal(EntityState.Added, entry.EntityState);
            Assert.Contains(entry, configuration.StateManager.StateEntries);
        }

        [Fact]
        public void Changing_state_to_Unknown_causes_entity_to_stop_tracking()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType("SomeEntity");
            var keyProperty = entityType.GetProperty("Id");

            var configuration = CreateConfiguration(model);

            var entry = CreateStateEntry(configuration, entityType, new SomeEntity());
            entry.SetPropertyValue(keyProperty, 1);

            entry.SetEntityStateAsync(EntityState.Added, CancellationToken.None).Wait();
            entry.SetEntityStateAsync(EntityState.Unknown, CancellationToken.None).Wait();

            Assert.Equal(EntityState.Unknown, entry.EntityState);
            Assert.DoesNotContain(entry, configuration.StateManager.StateEntries);
        }

        [Fact]
        public void Changing_state_to_Modified_or_Unchanged_causes_all_properties_to_be_marked_accordingly()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType("SomeEntity");
            var keyProperty = entityType.GetProperty("Id");
            var nonKeyProperty = entityType.GetProperty("Kool");
            var configuration = CreateConfiguration(model);

            var entry = CreateStateEntry(configuration, entityType, new SomeEntity());
            entry.SetPropertyValue(keyProperty, 1);

            Assert.False(entry.IsPropertyModified(keyProperty));
            Assert.False(entry.IsPropertyModified(nonKeyProperty));

            entry.SetEntityStateAsync(EntityState.Modified, CancellationToken.None).Wait();

            Assert.True(entry.IsPropertyModified(keyProperty));
            Assert.True(entry.IsPropertyModified(nonKeyProperty));

            entry.SetEntityStateAsync(EntityState.Unchanged, CancellationToken.None).Wait();

            Assert.False(entry.IsPropertyModified(keyProperty));
            Assert.False(entry.IsPropertyModified(nonKeyProperty));
        }

        [Fact]
        public void Changing_state_to_Added_triggers_key_generation()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType("SomeEntity");
            var keyProperty = entityType.GetProperty("Id");

            var generatorMock = new Mock<IIdentityGenerator>();
            generatorMock.Setup(m => m.NextAsync(CancellationToken.None)).Returns(Task.FromResult((object)77));

            var generatorFactory = new Mock<IdentityGeneratorFactory>();
            generatorFactory.Setup(m => m.Create(keyProperty)).Returns(generatorMock.Object);

            var configuration = new EntityContext(
                new EntityConfigurationBuilder()
                    .UseModel(model)
                    .UseIdentityGeneratorFactory(generatorFactory.Object)
                    .BuildConfiguration()).Configuration;

            var entry = CreateStateEntry(configuration, entityType, new SomeEntity());

            entry.SetEntityStateAsync(EntityState.Added, CancellationToken.None).Wait();

            Assert.Equal(77, entry.GetPropertyValue(keyProperty));
        }

        [Fact]
        public void Can_create_primary_key()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType("SomeEntity");
            var keyProperty = entityType.GetProperty("Id");
            var configuration = CreateConfiguration(model);

            var entry = CreateStateEntry(configuration, entityType, new SomeEntity());
            entry.SetPropertyValue(keyProperty, 77);

            var keyValue = entry.GetPrimaryKeyValue();
            Assert.IsType<SimpleEntityKey<int>>(keyValue);
            Assert.Equal(77, keyValue.Value);
        }

        [Fact]
        public void Can_create_foreign_key_value_based_on_dependent_values()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType("SomeDependentEntity");
            var fkProperty = entityType.GetProperty("SomeEntityId");
            var configuration = CreateConfiguration(model);

            var entry = CreateStateEntry(configuration, entityType, new SomeDependentEntity());
            entry.SetPropertyValue(fkProperty, 77);

            var keyValue = entry.GetDependentKeyValue(entityType.ForeignKeys.Single());
            Assert.IsType<SimpleEntityKey<int>>(keyValue);
            Assert.Equal(77, keyValue.Value);
        }

        [Fact]
        public void Can_create_foreign_key_value_based_on_principal_end_values()
        {
            var model = BuildModel();
            var principalType = model.GetEntityType("SomeEntity");
            var dependentType = model.GetEntityType("SomeDependentEntity");
            var key = principalType.GetProperty("Id");
            var configuration = CreateConfiguration(model);

            var entry = CreateStateEntry(configuration, principalType, new SomeEntity());
            entry.SetPropertyValue(key, 77);

            var keyValue = entry.GetPrincipalKeyValue(dependentType.ForeignKeys.Single());
            Assert.IsType<SimpleEntityKey<int>>(keyValue);
            Assert.Equal(77, keyValue.Value);
        }

        [Fact]
        public void Can_get_property_value_without_materializing_entity()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType("SomeEntity");
            var keyProperty = entityType.GetProperty("Id");
            var configuration = CreateConfiguration(model);

            var entry = CreateStateEntry(configuration, entityType, new object[] { 1, "Kool" });

            Assert.Equal(1, entry.GetPropertyValue(keyProperty));
        }

        [Fact]
        public void Can_set_property_value_without_materializing_entity()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType("SomeEntity");
            var keyProperty = entityType.GetProperty("Id");
            var configuration = CreateConfiguration(model);

            var entry = CreateStateEntry(configuration, entityType, new object[] { 1, "Kool" });

            entry.SetPropertyValue(keyProperty, 77);

            Assert.Equal(77, entry.GetPropertyValue(keyProperty));
        }

        [Fact]
        public void Can_set_and_get_property_values()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType("SomeEntity");
            var keyProperty = entityType.GetProperty("Id");
            var nonKeyProperty = entityType.GetProperty("Kool");
            var configuration = CreateConfiguration(model);

            var entry = CreateStateEntry(configuration, entityType, new SomeEntity());

            entry.SetPropertyValue(keyProperty, 77);
            entry.SetPropertyValue(nonKeyProperty, "Magic Tree House");

            Assert.Equal(77, entry.GetPropertyValue(keyProperty));
            Assert.Equal("Magic Tree House", entry.GetPropertyValue(nonKeyProperty));
        }

        [Fact]
        public void Can_get_value_buffer_from_properties()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType("SomeEntity");
            var keyProperty = entityType.GetProperty("Id");
            var nonKeyProperty = entityType.GetProperty("Kool");
            var configuration = CreateConfiguration(model);

            var entry = CreateStateEntry(configuration, entityType, new SomeEntity());

            entry.SetPropertyValue(keyProperty, 77);
            entry.SetPropertyValue(nonKeyProperty, "Magic Tree House");

            Assert.Equal(new object[] { 77, "Magic Tree House" }, entry.GetValueBuffer());
        }

        protected virtual StateEntry CreateStateEntry(ContextConfiguration stateManager, IEntityType entityType, object entity)
        {
            return new MixedStateEntry(stateManager, entityType, entity);
        }

        protected virtual StateEntry CreateStateEntry(ContextConfiguration stateManager, IEntityType entityType, object[] valueBuffer)
        {
            return new MixedStateEntry(stateManager, entityType, valueBuffer);
        }

        protected virtual ContextConfiguration CreateConfiguration(IModel model)
        {
            return new EntityContext(
                new EntityConfigurationBuilder().UseModel(model).BuildConfiguration()).Configuration;
        }

        protected virtual IModel BuildModel()
        {
            var model = new Model();

            var entityType1 = new EntityType(typeof(SomeEntity));
            model.AddEntityType(entityType1);
            var key1 = entityType1.AddProperty("Id", typeof(int), shadowProperty: false);
            entityType1.SetKey(key1);
            entityType1.AddProperty("Kool", typeof(string), shadowProperty: false);

            var entityType2 = new EntityType(typeof(SomeDependentEntity));
            model.AddEntityType(entityType2);
            var key2 = entityType2.AddProperty("Id", typeof(int), shadowProperty: false);
            entityType2.SetKey(key2);
            var fk = entityType2.AddProperty("SomeEntityId", typeof(int), shadowProperty: false);
            entityType2.AddForeignKey(entityType1.GetKey(), new[] { fk });

            return model;
        }

        protected class SomeEntity
        {
            public int Id { get; set; }
            public string Kool { get; set; }
        }

        protected class SomeDependentEntity
        {
            public int Id { get; set; }
            public int SomeEntityId { get; set; }
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
