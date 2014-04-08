// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

            entry.EntityState = EntityState.Added;

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

            entry.EntityState = EntityState.Added;
            entry.EntityState = EntityState.Unknown;

            Assert.Equal(EntityState.Unknown, entry.EntityState);
            Assert.DoesNotContain(entry, configuration.StateManager.StateEntries);
        }

        [Fact]
        public void Changing_state_to_Modified_or_Unchanged_causes_all_properties_to_be_marked_accordingly()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType("SomeEntity");
            var keyProperty = entityType.GetProperty("Id");
            var nonKeyProperty = entityType.GetProperty("Name");
            var configuration = CreateConfiguration(model);

            var entry = CreateStateEntry(configuration, entityType, new SomeEntity());
            entry.SetPropertyValue(keyProperty, 1);

            Assert.False(entry.IsPropertyModified(keyProperty));
            Assert.False(entry.IsPropertyModified(nonKeyProperty));

            entry.EntityState = EntityState.Modified;

            Assert.True(entry.IsPropertyModified(keyProperty));
            Assert.True(entry.IsPropertyModified(nonKeyProperty));

            entry.EntityState = EntityState.Unchanged;

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

            entry.EntityState = EntityState.Added;

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
        public void Can_get_property_value_after_creation_from_value_buffer()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType("SomeEntity");
            var keyProperty = entityType.GetProperty("Id");
            var configuration = CreateConfiguration(model);

            var entry = CreateStateEntry(configuration, entityType, new ObjectArrayValueReader(new object[] { 1, "Kool" }));

            Assert.Equal(1, entry.GetPropertyValue(keyProperty));
        }

        [Fact]
        public void Can_set_property_value_after_creation_from_value_buffer()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType("SomeEntity");
            var keyProperty = entityType.GetProperty("Id");
            var configuration = CreateConfiguration(model);

            var entry = CreateStateEntry(configuration, entityType, new ObjectArrayValueReader(new object[] { 1, "Kool" }));

            entry.SetPropertyValue(keyProperty, 77);

            Assert.Equal(77, entry.GetPropertyValue(keyProperty));
        }

        [Fact]
        public void Can_set_and_get_property_values()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType("SomeEntity");
            var keyProperty = entityType.GetProperty("Id");
            var nonKeyProperty = entityType.GetProperty("Name");
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
            var nonKeyProperty = entityType.GetProperty("Name");
            var configuration = CreateConfiguration(model);

            var entry = CreateStateEntry(configuration, entityType, new SomeEntity());

            entry.SetPropertyValue(keyProperty, 77);
            entry.SetPropertyValue(nonKeyProperty, "Magic Tree House");

            Assert.Equal(new object[] { 77, "Magic Tree House" }, entry.GetValueBuffer());
        }

        [Fact]
        public void All_original_values_can_be_accessed_for_entity_that_does_full_change_tracking_if_eager_values_on()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType("FullNotificationEntity");
            entityType.UseLazyOriginalValues = false;

            AllOriginalValuesTest(model, entityType);
        }

        protected void AllOriginalValuesTest(IModel model, IEntityType entityType)
        {
            var idProperty = entityType.GetProperty("Id");
            var nameProperty = entityType.GetProperty("Name");
            var configuration = CreateConfiguration(model);

            var entry = CreateStateEntry(configuration, entityType, new ObjectArrayValueReader(new object[] { 1, "Kool" }));

            Assert.Equal(1, entry.GetPropertyOriginalValue(idProperty));
            Assert.Equal("Kool", entry.GetPropertyOriginalValue(nameProperty));
            Assert.Equal(1, entry.GetPropertyValue(idProperty));
            Assert.Equal("Kool", entry.GetPropertyValue(nameProperty));

            entry.SetPropertyValue(idProperty, 2);
            entry.SetPropertyValue(nameProperty, "Beans");

            Assert.Equal(1, entry.GetPropertyOriginalValue(idProperty));
            Assert.Equal("Kool", entry.GetPropertyOriginalValue(nameProperty));
            Assert.Equal(2, entry.GetPropertyValue(idProperty));
            Assert.Equal("Beans", entry.GetPropertyValue(nameProperty));

            entry.SetPropertyOriginalValue(idProperty, 3);
            entry.SetPropertyOriginalValue(nameProperty, "Franks");

            Assert.Equal(3, entry.GetPropertyOriginalValue(idProperty));
            Assert.Equal("Franks", entry.GetPropertyOriginalValue(nameProperty));
            Assert.Equal(2, entry.GetPropertyValue(idProperty));
            Assert.Equal("Beans", entry.GetPropertyValue(nameProperty));
        }

        [Fact]
        public void Required_original_values_can_be_accessed_for_entity_that_does_full_change_tracking()
        {
            var model = BuildModel();
            OriginalValuesTest(model, model.GetEntityType("FullNotificationEntity"));
        }

        [Fact]
        public void Required_original_values_can_be_accessed_for_entity_that_does_changed_only_notification()
        {
            var model = BuildModel();
            OriginalValuesTest(model, model.GetEntityType("ChangedOnlyEntity"));
        }

        [Fact]
        public void Required_original_values_can_be_accessed_for_entity_that_does_no_notification()
        {
            var model = BuildModel();
            OriginalValuesTest(model, model.GetEntityType("SomeEntity"));
        }

        protected void OriginalValuesTest(IModel model, IEntityType entityType)
        {
            var nameProperty = entityType.GetProperty("Name");
            var configuration = CreateConfiguration(model);

            var entry = CreateStateEntry(configuration, entityType, new ObjectArrayValueReader(new object[] { 1, "Kool" }));

            Assert.Equal("Kool", entry.GetPropertyOriginalValue(nameProperty));
            Assert.Equal("Kool", entry.GetPropertyValue(nameProperty));

            entry.SetPropertyValue(nameProperty, "Beans");

            Assert.Equal("Kool", entry.GetPropertyOriginalValue(nameProperty));
            Assert.Equal("Beans", entry.GetPropertyValue(nameProperty));

            entry.SetPropertyOriginalValue(nameProperty, "Franks");

            Assert.Equal("Franks", entry.GetPropertyOriginalValue(nameProperty));
            Assert.Equal("Beans", entry.GetPropertyValue(nameProperty));
        }

        [Fact]
        public void Null_original_values_are_handled_for_entity_that_does_full_change_tracking()
        {
            var model = BuildModel();
            NullOriginalValuesTest(model, model.GetEntityType("FullNotificationEntity"));
        }

        [Fact]
        public void Null_original_values_are_handled_for_entity_that_does_changed_only_notification()
        {
            var model = BuildModel();
            NullOriginalValuesTest(model, model.GetEntityType("ChangedOnlyEntity"));
        }

        [Fact]
        public void Null_original_values_are_handled_for_entity_that_does_no_notification()
        {
            var model = BuildModel();
            NullOriginalValuesTest(model, model.GetEntityType("SomeEntity"));
        }

        protected void NullOriginalValuesTest(IModel model, IEntityType entityType)
        {
            var nameProperty = entityType.GetProperty("Name");
            var configuration = CreateConfiguration(model);

            var entry = CreateStateEntry(configuration, entityType, new ObjectArrayValueReader(new object[] { 1, null }));

            Assert.Null(entry.GetPropertyOriginalValue(nameProperty));
            Assert.Null(entry.GetPropertyValue(nameProperty));

            entry.SetPropertyValue(nameProperty, "Beans");

            Assert.Null(entry.GetPropertyOriginalValue(nameProperty));
            Assert.Equal("Beans", entry.GetPropertyValue(nameProperty));

            entry.SetPropertyOriginalValue(nameProperty, "Franks");

            Assert.Equal("Franks", entry.GetPropertyOriginalValue(nameProperty));
            Assert.Equal("Beans", entry.GetPropertyValue(nameProperty));

            entry.SetPropertyOriginalValue(nameProperty, null);

            Assert.Null(entry.GetPropertyOriginalValue(nameProperty));
            Assert.Equal("Beans", entry.GetPropertyValue(nameProperty));
        }

        [Fact]
        public void Setting_property_using_state_entry_always_marks_as_modified()
        {
            var model = BuildModel();

            SetPropertyStateEntryTest(model, model.GetEntityType("FullNotificationEntity"));
            SetPropertyStateEntryTest(model, model.GetEntityType("ChangedOnlyEntity"));
            SetPropertyStateEntryTest(model, model.GetEntityType("SomeEntity"));
        }

        protected void SetPropertyStateEntryTest(IModel model, IEntityType entityType)
        {
            var idProperty = entityType.GetProperty("Id");
            var nameProperty = entityType.GetProperty("Name");
            var configuration = CreateConfiguration(model);

            var entry = CreateStateEntry(configuration, entityType, new ObjectArrayValueReader(new object[] { 1, "Kool" }));
            entry.EntityState = EntityState.Unchanged;

            Assert.False(entry.IsPropertyModified(idProperty));
            Assert.False(entry.IsPropertyModified(nameProperty));
            Assert.Equal(EntityState.Unchanged, entry.EntityState);

            entry.SetPropertyValue(idProperty, 1);
            entry.SetPropertyValue(nameProperty, "Kool");

            Assert.False(entry.IsPropertyModified(idProperty));
            Assert.False(entry.IsPropertyModified(nameProperty));
            Assert.Equal(EntityState.Unchanged, entry.EntityState);

            entry.SetPropertyValue(idProperty, 2);
            entry.SetPropertyValue(nameProperty, "Beans");

            Assert.True(entry.IsPropertyModified(idProperty));
            Assert.True(entry.IsPropertyModified(nameProperty));
            Assert.Equal(EntityState.Modified, entry.EntityState);
        }

        protected void SetPropertyClrTest<TEntity>(bool needsDetectChanges)
            where TEntity : ISomeEntity
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(TEntity));
            var nameProperty = entityType.GetProperty("Name");
            var configuration = CreateConfiguration(model);

            var entry = CreateStateEntry(configuration, entityType, new ObjectArrayValueReader(new object[] { 1, "Kool" }));
            entry.EntityState = EntityState.Unchanged;

            var entity = (TEntity)entry.Entity;

            Assert.False(entry.IsPropertyModified(nameProperty));
            Assert.Equal(EntityState.Unchanged, entry.EntityState);

            entity.Name = "Kool";

            Assert.False(entry.IsPropertyModified(nameProperty));
            Assert.Equal(EntityState.Unchanged, entry.EntityState);

            entity.Name = "Beans";

            if (needsDetectChanges)
            {
                Assert.False(entry.IsPropertyModified(nameProperty));
                Assert.Equal(EntityState.Unchanged, entry.EntityState);

                entry.DetectChanges();
            }

            Assert.True(entry.IsPropertyModified(nameProperty));
            Assert.Equal(EntityState.Modified, entry.EntityState);
        }

        [Fact]
        public void AcceptChanges_does_nothing_for_unchanged_entities()
        {
            AcceptChangesNoop(EntityState.Unchanged);
        }

        [Fact]
        public void AcceptChanges_does_nothing_for_unknown_entities()
        {
            AcceptChangesNoop(EntityState.Unknown);
        }

        private void AcceptChangesNoop(EntityState entityState)
        {
            var model = BuildModel();
            var entityType = model.GetEntityType("SomeEntity");
            var configuration = CreateConfiguration(model);

            var entry = CreateStateEntry(configuration, entityType, new ObjectArrayValueReader(new object[] { 1, "Kool" }));
            entry.EntityState = entityState;

            entry.AcceptChanges();

            Assert.Equal(entityState, entry.EntityState);
        }

        [Fact]
        public void AcceptChanges_makes_Modified_entities_Unchanged_and_resets_used_original_values()
        {
            AcceptChangesKeep(EntityState.Modified);
        }

        [Fact]
        public void AcceptChanges_makes_Added_entities_Unchanged()
        {
            AcceptChangesKeep(EntityState.Added);
        }

        private void AcceptChangesKeep(EntityState entityState)
        {
            var model = BuildModel();
            var entityType = model.GetEntityType("SomeEntity");
            var nameProperty = entityType.GetProperty("Name");
            var configuration = CreateConfiguration(model);

            var entry = CreateStateEntry(configuration, entityType, new ObjectArrayValueReader(new object[] { 1, "Kool" }));
            entry.EntityState = entityState;

            entry.SetPropertyValue(nameProperty, "Pickle");
            entry.SetPropertyOriginalValue(nameProperty, "Cheese");

            entry.AcceptChanges();

            Assert.Equal(EntityState.Unchanged, entry.EntityState);
            Assert.Equal("Pickle", entry.GetPropertyValue(nameProperty));
            Assert.Equal("Pickle", entry.GetPropertyOriginalValue(nameProperty));
        }

        [Fact]
        public void AcceptChanges_makes_Modified_entities_Unchanged_and_effectively_resets_unused_original_values()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType("SomeEntity");
            var nameProperty = entityType.GetProperty("Name");
            var configuration = CreateConfiguration(model);

            var entry = CreateStateEntry(configuration, entityType, new ObjectArrayValueReader(new object[] { 1, "Kool" }));
            entry.EntityState = EntityState.Modified;

            entry.SetPropertyValue(nameProperty, "Pickle");

            entry.AcceptChanges();

            Assert.Equal(EntityState.Unchanged, entry.EntityState);
            Assert.Equal("Pickle", entry.GetPropertyValue(nameProperty));
            Assert.Equal("Pickle", entry.GetPropertyOriginalValue(nameProperty));
        }

        [Fact]
        public void AcceptChanges_detaches_Deleted_entities()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType("SomeEntity");
            var configuration = CreateConfiguration(model);

            var entry = CreateStateEntry(configuration, entityType, new ObjectArrayValueReader(new object[] { 1, "Kool" }));
            entry.EntityState = EntityState.Deleted;

            entry.AcceptChanges();

            Assert.Equal(EntityState.Unknown, entry.EntityState);
        }

        protected virtual StateEntry CreateStateEntry(ContextConfiguration configuration, IEntityType entityType, object entity)
        {
            return new StateEntrySubscriber().SnapshotAndSubscribe(
                new StateEntryFactory(configuration, new EntityMaterializerSource(new MemberMapper(new FieldMatcher()))).Create(entityType, entity));
        }

        protected virtual StateEntry CreateStateEntry(ContextConfiguration configuration, IEntityType entityType, IValueReader valueReader)
        {
            return new StateEntrySubscriber().SnapshotAndSubscribe(
                new StateEntryFactory(configuration, new EntityMaterializerSource(new MemberMapper(new FieldMatcher()))).Create(entityType, valueReader));
        }

        protected virtual ContextConfiguration CreateConfiguration(IModel model)
        {
            return new EntityContext(
                new EntityConfigurationBuilder().UseModel(model).BuildConfiguration()).Configuration;
        }

        protected virtual Model BuildModel()
        {
            var model = new Model();

            var entityType1 = new EntityType(typeof(SomeEntity));
            model.AddEntityType(entityType1);
            var key1 = entityType1.AddProperty("Id", typeof(int));
            entityType1.SetKey(key1);
            entityType1.AddProperty("Name", typeof(string), shadowProperty: false, concurrencyToken: true);

            var entityType2 = new EntityType(typeof(SomeDependentEntity));
            model.AddEntityType(entityType2);
            var key2 = entityType2.AddProperty("Id", typeof(int));
            entityType2.SetKey(key2);
            var fk = entityType2.AddProperty("SomeEntityId", typeof(int));
            entityType2.AddForeignKey(entityType1.GetKey(), new[] { fk });

            var entityType3 = new EntityType(typeof(FullNotificationEntity));
            model.AddEntityType(entityType3);
            entityType3.SetKey(entityType3.AddProperty("Id", typeof(int)));
            entityType3.AddProperty("Name", typeof(string), shadowProperty: false, concurrencyToken: true);

            var entityType4 = new EntityType(typeof(ChangedOnlyEntity));
            model.AddEntityType(entityType4);
            entityType4.SetKey(entityType4.AddProperty("Id", typeof(int)));
            entityType4.AddProperty("Name", typeof(string), shadowProperty: false, concurrencyToken: true);

            return model;
        }

        protected interface ISomeEntity
        {
            int Id { get; set; }
            string Name { get; set; }
        }

        protected class SomeEntity : ISomeEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        protected class SomeDependentEntity
        {
            public int Id { get; set; }
            public int SomeEntityId { get; set; }
        }

        protected class FullNotificationEntity : INotifyPropertyChanging, INotifyPropertyChanged, ISomeEntity
        {
            private int _id;
            private string _name;

            public int Id
            {
                get { return _id; }
                set
                {
                    if (_id != value)
                    {
                        NotifyChanging();
                        _id = value;
                        NotifyChanged();
                    }
                }
            }

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

        protected class ChangedOnlyEntity : INotifyPropertyChanged, ISomeEntity
        {
            private int _id;
            private string _name;

            public int Id
            {
                get { return _id; }
                set
                {
                    if (_id != value)
                    {
                        _id = value;
                        NotifyChanged();
                    }
                }
            }

            public string Name
            {
                get { return _name; }
                set
                {
                    if (_name != value)
                    {
                        _name = value;
                        NotifyChanged();
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private void NotifyChanged([CallerMemberName] String propertyName = "")
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }
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
