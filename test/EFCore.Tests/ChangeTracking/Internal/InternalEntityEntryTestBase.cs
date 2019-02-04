// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable AssignNullToNotNullAttribute
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    public abstract class InternalEntityEntryTestBase
    {
        [Fact]
        public virtual void Store_setting_null_for_non_nullable_store_generated_property_throws()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.FindProperty("Id");
            keyProperty.ValueGenerated = ValueGenerated.OnAdd;

            var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(contextServices, entityType, new SomeEntity());
            entry.SetEntityState(EntityState.Added);
            entry.PrepareToSave();

            Assert.Equal(
                CoreStrings.ValueCannotBeNull("Id", keyProperty.DeclaringEntityType.DisplayName(), typeof(int).DisplayName()),
                Assert.Throws<InvalidOperationException>(() => entry.SetStoreGeneratedValue(keyProperty, null)).Message);
        }

        [Fact]
        public virtual void Changing_state_from_Unknown_causes_entity_to_start_tracking()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.FindProperty("Id");

            var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(contextServices, entityType, new SomeEntity());
            entry[keyProperty] = 1;

            entry.SetEntityState(EntityState.Added);

            Assert.Equal(EntityState.Added, entry.EntityState);
            Assert.Contains(entry, contextServices.GetRequiredService<IStateManager>().Entries);
        }

        [Fact]
        public virtual void Changing_state_to_Unknown_causes_entity_to_stop_tracking()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.FindProperty("Id");

            var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(contextServices, entityType, new SomeEntity());
            entry[keyProperty] = 1;

            entry.SetEntityState(EntityState.Added);
            entry.SetEntityState(EntityState.Detached);

            Assert.Equal(EntityState.Detached, entry.EntityState);
            Assert.DoesNotContain(entry, contextServices.GetRequiredService<IStateManager>().Entries);
        }

        [Fact] // GitHub #251, #1247
        public virtual void Changing_state_from_Added_to_Deleted_does_what_you_ask()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.FindProperty("Id");

            var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(contextServices, entityType, new SomeEntity());
            entry[keyProperty] = 1;

            entry.SetEntityState(EntityState.Added);
            entry.SetEntityState(EntityState.Deleted);

            Assert.Equal(EntityState.Deleted, entry.EntityState);
            Assert.Contains(entry, contextServices.GetRequiredService<IStateManager>().Entries);
        }

        [Fact]
        public virtual void Changing_state_to_Modified_or_Unchanged_causes_all_properties_to_be_marked_accordingly()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.FindProperty("Id");
            var nonKeyProperty = entityType.FindProperty("Name");
            var configuration = InMemoryTestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeEntity());
            entry[keyProperty] = 1;

            Assert.False(entry.IsModified(keyProperty));
            Assert.False(entry.IsModified(nonKeyProperty));

            entry.SetEntityState(EntityState.Modified);

            Assert.False(entry.IsModified(keyProperty));
            Assert.NotEqual(nonKeyProperty.IsShadowProperty, entry.IsModified(nonKeyProperty));

            entry.SetEntityState(EntityState.Unchanged, true);

            Assert.False(entry.IsModified(keyProperty));
            Assert.False(entry.IsModified(nonKeyProperty));

            entry.SetPropertyModified(nonKeyProperty);

            Assert.Equal(EntityState.Modified, entry.EntityState);
            Assert.False(entry.IsModified(keyProperty));
            Assert.True(entry.IsModified(nonKeyProperty));
        }

        [Fact]
        public virtual void Key_properties_throw_immediately_if_modified()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.FindProperty("Id");
            var configuration = InMemoryTestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeEntity());

            entry[keyProperty] = 1;

            entry.SetEntityState(EntityState.Modified);

            Assert.False(entry.IsModified(keyProperty));

            entry.SetEntityState(EntityState.Unchanged, true);

            Assert.False(entry.IsModified(keyProperty));

            Assert.Equal(
                CoreStrings.KeyReadOnly("Id", entityType.DisplayName()),
                Assert.Throws<InvalidOperationException>(() => entry.SetPropertyModified(keyProperty)).Message);

            Assert.Equal(EntityState.Unchanged, entry.EntityState);
            Assert.False(entry.IsModified(keyProperty));

            Assert.Equal(
                CoreStrings.KeyReadOnly("Id", entityType.DisplayName()),
                Assert.Throws<InvalidOperationException>(() => entry[keyProperty] = 2).Message);

            Assert.Equal(EntityState.Unchanged, entry.EntityState);
            Assert.False(entry.IsModified(keyProperty));
        }

        [Fact]
        public virtual void Added_entities_can_have_temporary_values()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.FindProperty("Id");
            var nonKeyProperty = entityType.FindProperty("Name");
            var configuration = InMemoryTestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeEntity());
            entry[keyProperty] = 1;

            Assert.False(entry.HasTemporaryValue(keyProperty));
            Assert.False(entry.HasTemporaryValue(nonKeyProperty));
            Assert.False(entry.IsModified(keyProperty));
            Assert.False(entry.IsModified(nonKeyProperty));

            entry.SetEntityState(EntityState.Added);

            Assert.False(entry.HasTemporaryValue(keyProperty));
            Assert.False(entry.HasTemporaryValue(nonKeyProperty));
            Assert.False(entry.IsModified(keyProperty));
            Assert.False(entry.IsModified(nonKeyProperty));

            entry.SetTemporaryValue(keyProperty, 1);

            Assert.True(entry.HasTemporaryValue(keyProperty));
            Assert.False(entry.HasTemporaryValue(nonKeyProperty));
            Assert.False(entry.IsModified(keyProperty));
            Assert.False(entry.IsModified(nonKeyProperty));

            entry.SetTemporaryValue(nonKeyProperty, "Temp");
            entry[keyProperty] = 1;

            Assert.False(entry.HasTemporaryValue(keyProperty));
            Assert.True(entry.HasTemporaryValue(nonKeyProperty));
            Assert.False(entry.IsModified(keyProperty));
            Assert.False(entry.IsModified(nonKeyProperty));

            entry[nonKeyProperty] = "I Am A Real Person!";

            entry.SetEntityState(EntityState.Unchanged);

            Assert.False(entry.HasTemporaryValue(keyProperty));
            Assert.False(entry.HasTemporaryValue(nonKeyProperty));
            Assert.False(entry.IsModified(keyProperty));
            Assert.False(entry.IsModified(nonKeyProperty));

            // Can't change the key...
            Assert.Throws<InvalidOperationException>(() => entry.SetTemporaryValue(keyProperty, -1));
            entry.SetTemporaryValue(nonKeyProperty, "Temp");

            Assert.True(entry.HasTemporaryValue(keyProperty));
            Assert.True(entry.HasTemporaryValue(nonKeyProperty));
            Assert.False(entry.IsModified(keyProperty));
            Assert.True(entry.IsModified(nonKeyProperty));

            entry.SetEntityState(EntityState.Added);

            Assert.True(entry.HasTemporaryValue(keyProperty));
            Assert.True(entry.HasTemporaryValue(nonKeyProperty));
            Assert.False(entry.IsModified(keyProperty));
            Assert.False(entry.IsModified(nonKeyProperty));
        }

        [Theory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Changing_state_with_temp_value_throws(EntityState targetState)
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.FindProperty("Id");
            var configuration = InMemoryTestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeEntity());

            entry.SetEntityState(EntityState.Added);
            entry.SetTemporaryValue(keyProperty, -1);

            Assert.Equal(
                CoreStrings.TempValuePersists("Id", entityType.DisplayName(), targetState.ToString()),
                Assert.Throws<InvalidOperationException>(() => entry.SetEntityState(targetState)).Message);
        }

        [Fact]
        public virtual void Detaching_with_temp_values_does_not_throw()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.FindProperty("Id");
            var configuration = InMemoryTestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeEntity());
            entry[keyProperty] = 1;

            entry.SetEntityState(EntityState.Added);
            entry.SetTemporaryValue(keyProperty, -1);

            Assert.True(entry.HasTemporaryValue(keyProperty));

            entry.SetEntityState(EntityState.Detached);

            Assert.True(entry.HasTemporaryValue(keyProperty));

            entry[keyProperty] = 1;
            entry.SetEntityState(EntityState.Unchanged);

            Assert.False(entry.HasTemporaryValue(keyProperty));
        }

        [Fact]
        public virtual void Setting_an_explicit_value_marks_property_as_not_temporary()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.FindProperty("Id");
            var configuration = InMemoryTestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeEntity());

            entry.SetEntityState(EntityState.Added);
            entry.SetTemporaryValue(keyProperty, -1);

            Assert.True(entry.HasTemporaryValue(keyProperty));

            entry[keyProperty] = 77;

            Assert.False(entry.HasTemporaryValue(keyProperty));

            entry.SetEntityState(EntityState.Unchanged); // Does not throw
        }

        [Fact]
        public virtual void Key_properties_share_value_generation_space_with_base()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.FindProperty("Id");
            var baseEntityType = model.FindEntityType(typeof(SomeSimpleEntityBase).FullName);
            var altKeyProperty = baseEntityType.AddProperty("NonId", typeof(int));
            altKeyProperty.ValueGenerated = ValueGenerated.OnAdd;
            baseEntityType.AddKey(altKeyProperty);
            var configuration = InMemoryTestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeEntity());

            Assert.Equal(0, entry[keyProperty]);

            Assert.Equal(0, entry[altKeyProperty]);

            entry.SetEntityState(EntityState.Added);

            Assert.NotNull(entry[keyProperty]);
            Assert.NotEqual(0, entry[keyProperty]);
            Assert.Equal(entry[keyProperty], entry[altKeyProperty]);

            var baseEntry = CreateInternalEntry(configuration, baseEntityType, new SomeSimpleEntityBase());

            baseEntry.SetEntityState(EntityState.Added);

            Assert.NotNull(baseEntry[keyProperty]);
            Assert.NotEqual(0, baseEntry[keyProperty]);
            Assert.Equal(baseEntry[keyProperty], baseEntry[altKeyProperty]);
            Assert.NotEqual(entry[keyProperty], baseEntry[keyProperty]);
            Assert.NotEqual(entry[altKeyProperty], baseEntry[altKeyProperty]);
        }

        [Fact]
        public virtual void Value_generation_does_not_happen_if_property_has_non_default_value()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.FindProperty("Id");
            var configuration = InMemoryTestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeEntity());

            entry[keyProperty] = 31143;

            entry.SetEntityState(EntityState.Added);

            Assert.Equal(31143, entry[keyProperty]);
        }

        [Fact]
        public virtual void Temporary_values_are_npt_reset_when_entity_is_detached()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.FindProperty("Id");
            var configuration = InMemoryTestHelpers.Instance.CreateContextServices(model);

            var entity = new SomeEntity();
            var entry = CreateInternalEntry(configuration, entityType, entity);

            entry.SetEntityState(EntityState.Added);
            entry.SetTemporaryValue(keyProperty, -1);

            Assert.NotNull(entry[keyProperty]);
            Assert.Equal(0, entity.Id);
            Assert.Equal(-1, entry[keyProperty]);

            entry.SetEntityState(EntityState.Detached);

            Assert.Equal(0, entity.Id);
            Assert.Equal(-1, entry[keyProperty]);

            entry.SetEntityState(EntityState.Added);

            Assert.Equal(0, entity.Id);
            Assert.Equal(-1, entry[keyProperty]);
        }

        [Fact]
        public virtual void Modified_values_are_reset_when_entity_is_changed_to_Added()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var property = entityType.FindProperty("Name");
            var configuration = InMemoryTestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeEntity());
            entry[entityType.FindProperty("Id")] = 1;

            entry.SetEntityState(EntityState.Modified);
            entry.SetPropertyModified(property);

            entry.SetEntityState(EntityState.Added);

            Assert.False(entry.HasTemporaryValue(property));
        }

        [Fact]
        public virtual void Changing_state_to_Added_triggers_value_generation_for_any_property()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeDependentEntity).FullName);
            var keyProperties = new[] { entityType.FindProperty("Id1"), entityType.FindProperty("Id2") };
            var fkProperty = entityType.FindProperty("SomeEntityId");
            var property = entityType.FindProperty("JustAProperty");
            var configuration = InMemoryTestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeDependentEntity());
            entry[keyProperties[0]] = 77;
            entry[keyProperties[1]] = "ReadySalted";
            entry[fkProperty] = 0;

            Assert.Equal(0, entry[property]);

            entry.SetEntityState(EntityState.Added);

            Assert.NotNull(entry[property]);
            Assert.NotEqual(0, entry[property]);
        }

        [Fact]
        public virtual void Notification_that_an_FK_property_has_changed_updates_the_snapshot()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeDependentEntity).FullName);
            var fkProperty = entityType.FindProperty("SomeEntityId");
            var configuration = InMemoryTestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeDependentEntity());
            entry[entityType.FindProperty("Id1")] = 66;
            entry[entityType.FindProperty("Id2")] = "Bar";
            entry.SetEntityState(EntityState.Added);
            entry[fkProperty] = 77;
            entry.SetRelationshipSnapshotValue(fkProperty, 78);

            entry[fkProperty] = 79;

            var keyValue = entry.GetRelationshipSnapshotValue(entityType.GetForeignKeys().Single().Properties.Single());
            Assert.Equal(79, keyValue);
        }

        [Fact]
        public virtual void Setting_property_to_the_same_value_does_not_update_the_snapshot()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeDependentEntity).FullName);
            var fkProperty = entityType.FindProperty("SomeEntityId");
            var configuration = InMemoryTestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeDependentEntity());
            entry[fkProperty] = 77;
            entry.SetRelationshipSnapshotValue(fkProperty, 78);

            entry[fkProperty] = 77;

            var keyValue = entry.GetRelationshipSnapshotValue(entityType.GetForeignKeys().Single().Properties.Single());
            Assert.Equal(78, keyValue);
        }

        [Fact]
        public virtual void Can_get_property_value_after_creation_from_value_buffer()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.FindProperty("Id");
            var configuration = InMemoryTestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(
                configuration,
                entityType,
                new SomeEntity
                {
                    Id = 1,
                    Name = "Kool"
                },
                new ValueBuffer(new object[] { 1, "Kool" }));

            Assert.Equal(1, entry[keyProperty]);
        }

        [Fact]
        public virtual void Can_set_property_value_after_creation_from_value_buffer()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.FindProperty("Id");
            var configuration = InMemoryTestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(
                configuration,
                entityType,
                new SomeEntity
                {
                    Id = 1,
                    Name = "Kool"
                },
                new ValueBuffer(new object[] { 1, "Kool" }));

            entry[keyProperty] = 77;

            Assert.Equal(77, entry[keyProperty]);
        }

        [Fact]
        public virtual void Can_set_and_get_property_values()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.FindProperty("Id");
            var nonKeyProperty = entityType.FindProperty("Name");
            var configuration = InMemoryTestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeEntity());

            entry[keyProperty] = 77;
            entry[nonKeyProperty] = "Magic Tree House";

            Assert.Equal(77, entry[keyProperty]);
            Assert.Equal("Magic Tree House", entry[nonKeyProperty]);
        }

        [Fact]
        public virtual void Can_set_and_get_property_values_genericly()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.FindProperty("Id");
            var nonKeyProperty = entityType.FindProperty("Name");
            var configuration = InMemoryTestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeEntity());

            entry[keyProperty] = 77;
            entry[nonKeyProperty] = "Magic Tree House";

            Assert.Equal(77, entry.GetCurrentValue<int>(keyProperty));
            Assert.Equal("Magic Tree House", entry.GetCurrentValue<string>(nonKeyProperty));
        }

        [Fact]
        public virtual void Can_get_value_buffer_from_properties()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.FindProperty("Id");
            var nonKeyProperty = entityType.FindProperty("Name");
            var configuration = InMemoryTestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeEntity());

            entry[keyProperty] = 77;
            entry[nonKeyProperty] = "Magic Tree House";

            Assert.Equal(new object[] { 77, "Magic Tree House" }, CreateValueBuffer(entry));
        }

        private static object[] CreateValueBuffer(IUpdateEntry entry)
            => entry.EntityType.GetProperties().Select(entry.GetCurrentValue).ToArray();

        [Fact]
        public virtual void All_original_values_can_be_accessed_for_entity_that_does_full_change_tracking_if_eager_values_on()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(FullNotificationEntity).FullName);
            entityType.ChangeTrackingStrategy = ChangeTrackingStrategy.Snapshot;

            AllOriginalValuesTest(
                model, entityType, new FullNotificationEntity
                {
                    Id = 1,
                    Name = "Kool"
                });
        }

        protected void AllOriginalValuesTest(IModel model, IEntityType entityType, object entity)
        {
            var idProperty = entityType.FindProperty("Id");
            var nameProperty = entityType.FindProperty("Name");
            var configuration = InMemoryTestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(
                configuration,
                entityType,
                entity,
                new ValueBuffer(new object[] { 1, "Kool" }));

            entry.SetEntityState(EntityState.Unchanged);

            Assert.Equal(1, entry.GetOriginalValue(idProperty));
            Assert.Equal("Kool", entry.GetOriginalValue(nameProperty));
            Assert.Equal(1, entry[idProperty]);
            Assert.Equal("Kool", entry[nameProperty]);

            entry[nameProperty] = "Beans";

            Assert.Equal(1, entry.GetOriginalValue(idProperty));
            Assert.Equal("Kool", entry.GetOriginalValue(nameProperty));
            Assert.Equal(1, entry[idProperty]);
            Assert.Equal("Beans", entry[nameProperty]);

            entry.SetOriginalValue(nameProperty, "Franks");

            Assert.Equal(1, entry.GetOriginalValue(idProperty));
            Assert.Equal("Franks", entry.GetOriginalValue(nameProperty));
            Assert.Equal(1, entry[idProperty]);
            Assert.Equal("Beans", entry[nameProperty]);
        }

        [Fact]
        public virtual void Required_original_values_can_be_accessed_for_entity_that_does_full_change_tracking()
        {
            var model = BuildModel();
            OriginalValuesTest(
                model, model.FindEntityType(typeof(FullNotificationEntity).FullName), new FullNotificationEntity
                {
                    Id = 1,
                    Name = "Kool"
                });
        }

        [Fact]
        public virtual void Required_original_values_can_be_accessed_for_entity_that_does_changed_only_notification()
        {
            var model = BuildModel();
            OriginalValuesTest(
                model, model.FindEntityType(typeof(ChangedOnlyEntity).FullName), new ChangedOnlyEntity
                {
                    Id = 1,
                    Name = "Kool"
                });
        }

        [Fact]
        public virtual void Required_original_values_can_be_accessed_for_entity_that_does_no_notification()
        {
            var model = BuildModel();
            OriginalValuesTest(
                model, model.FindEntityType(typeof(SomeEntity).FullName), new SomeEntity
                {
                    Id = 1,
                    Name = "Kool"
                });
        }

        protected void OriginalValuesTest(IModel model, IEntityType entityType, object entity)
        {
            var nameProperty = entityType.FindProperty("Name");
            var configuration = InMemoryTestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, entity, new ValueBuffer(new object[] { 1, "Kool" }));
            entry.SetEntityState(EntityState.Unchanged);

            Assert.Equal("Kool", entry.GetOriginalValue(nameProperty));
            Assert.Equal("Kool", entry[nameProperty]);

            entry[nameProperty] = "Beans";

            Assert.Equal("Kool", entry.GetOriginalValue(nameProperty));
            Assert.Equal("Beans", entry[nameProperty]);

            entry.SetOriginalValue(nameProperty, "Franks");

            Assert.Equal("Franks", entry.GetOriginalValue(nameProperty));
            Assert.Equal("Beans", entry[nameProperty]);
        }

        [Fact]
        public virtual void Required_original_values_can_be_accessed_generically_for_entity_that_does_full_change_tracking()
        {
            var model = BuildModel();
            GenericOriginalValuesTest(
                model, model.FindEntityType(typeof(FullNotificationEntity).FullName), new FullNotificationEntity
                {
                    Id = 1,
                    Name = "Kool"
                });
        }

        [Fact]
        public virtual void Required_original_values_can_be_accessed_generically_for_entity_that_does_changed_only_notification()
        {
            var model = BuildModel();
            GenericOriginalValuesTest(
                model, model.FindEntityType(typeof(ChangedOnlyEntity).FullName), new ChangedOnlyEntity
                {
                    Id = 1,
                    Name = "Kool"
                });
        }

        [Fact]
        public virtual void Required_original_values_can_be_accessed_generically_for_entity_that_does_no_notification()
        {
            var model = BuildModel();
            GenericOriginalValuesTest(
                model, model.FindEntityType(typeof(SomeEntity).FullName), new SomeEntity
                {
                    Id = 1,
                    Name = "Kool"
                });
        }

        protected void GenericOriginalValuesTest(IModel model, IEntityType entityType, object entity)
        {
            var nameProperty = entityType.FindProperty("Name");
            var configuration = InMemoryTestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, entity, new ValueBuffer(new object[] { 1, "Kool" }));
            entry.SetEntityState(EntityState.Unchanged);

            Assert.Equal("Kool", entry.GetOriginalValue<string>(nameProperty));
            Assert.Equal("Kool", entry.GetCurrentValue<string>(nameProperty));

            entry[nameProperty] = "Beans";

            Assert.Equal("Kool", entry.GetOriginalValue<string>(nameProperty));
            Assert.Equal("Beans", entry.GetCurrentValue<string>(nameProperty));

            entry.SetOriginalValue(nameProperty, "Franks");

            Assert.Equal("Franks", entry.GetOriginalValue<string>(nameProperty));
            Assert.Equal("Beans", entry.GetCurrentValue<string>(nameProperty));
        }

        [Fact]
        public virtual void Null_original_values_are_handled_for_entity_that_does_full_change_tracking()
        {
            var model = BuildModel();
            NullOriginalValuesTest(
                model, model.FindEntityType(typeof(FullNotificationEntity).FullName), new FullNotificationEntity
                {
                    Id = 1
                });
        }

        [Fact]
        public virtual void Null_original_values_are_handled_for_entity_that_does_changed_only_notification()
        {
            var model = BuildModel();
            NullOriginalValuesTest(
                model, model.FindEntityType(typeof(ChangedOnlyEntity).FullName), new ChangedOnlyEntity
                {
                    Id = 1
                });
        }

        [Fact]
        public virtual void Null_original_values_are_handled_for_entity_that_does_no_notification()
        {
            var model = BuildModel();
            NullOriginalValuesTest(
                model, model.FindEntityType(typeof(SomeEntity).FullName), new SomeEntity
                {
                    Id = 1
                });
        }

        protected void NullOriginalValuesTest(IModel model, IEntityType entityType, object entity)
        {
            var nameProperty = entityType.FindProperty("Name");
            var configuration = InMemoryTestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, entity, new ValueBuffer(new object[] { 1, null }));
            entry.SetEntityState(EntityState.Unchanged);

            Assert.Null(entry.GetOriginalValue(nameProperty));
            Assert.Null(entry[nameProperty]);

            entry[nameProperty] = "Beans";

            Assert.Null(entry.GetOriginalValue(nameProperty));
            Assert.Equal("Beans", entry[nameProperty]);

            entry.SetOriginalValue(nameProperty, "Franks");

            Assert.Equal("Franks", entry.GetOriginalValue(nameProperty));
            Assert.Equal("Beans", entry[nameProperty]);

            entry.SetOriginalValue(nameProperty, null);

            Assert.Null(entry.GetOriginalValue(nameProperty));
            Assert.Equal("Beans", entry[nameProperty]);
        }

        [Fact]
        public virtual void Null_original_values_are_handled_generically_for_entity_that_does_full_change_tracking()
        {
            var model = BuildModel();
            GenericNullOriginalValuesTest(
                model, model.FindEntityType(typeof(FullNotificationEntity).FullName), new FullNotificationEntity
                {
                    Id = 1
                });
        }

        [Fact]
        public virtual void Null_original_values_are_handled_generically_for_entity_that_does_changed_only_notification()
        {
            var model = BuildModel();
            GenericNullOriginalValuesTest(
                model, model.FindEntityType(typeof(ChangedOnlyEntity).FullName), new ChangedOnlyEntity
                {
                    Id = 1
                });
        }

        [Fact]
        public virtual void Null_original_values_are_handled_generically_for_entity_that_does_no_notification()
        {
            var model = BuildModel();
            GenericNullOriginalValuesTest(
                model, model.FindEntityType(typeof(SomeEntity).FullName), new SomeEntity
                {
                    Id = 1
                });
        }

        protected void GenericNullOriginalValuesTest(IModel model, IEntityType entityType, object entity)
        {
            var nameProperty = entityType.FindProperty("Name");
            var configuration = InMemoryTestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, entity, new ValueBuffer(new object[] { 1, null }));
            entry.SetEntityState(EntityState.Unchanged);

            Assert.Null(entry.GetOriginalValue<string>(nameProperty));
            Assert.Null(entry.GetCurrentValue<string>(nameProperty));

            entry[nameProperty] = "Beans";

            Assert.Null(entry.GetOriginalValue<string>(nameProperty));
            Assert.Equal("Beans", entry.GetCurrentValue<string>(nameProperty));

            entry.SetOriginalValue(nameProperty, "Franks");

            Assert.Equal("Franks", entry.GetOriginalValue<string>(nameProperty));
            Assert.Equal("Beans", entry.GetCurrentValue<string>(nameProperty));

            entry.SetOriginalValue(nameProperty, null);

            Assert.Null(entry.GetOriginalValue<string>(nameProperty));
            Assert.Equal("Beans", entry.GetCurrentValue<string>(nameProperty));
        }

        [Fact]
        public virtual void Setting_property_using_state_entry_always_marks_as_modified()
        {
            var model = BuildModel();

            SetPropertyInternalEntityEntryTest(
                model, model.FindEntityType(typeof(FullNotificationEntity).FullName), new FullNotificationEntity
                {
                    Id = 1,
                    Name = "Kool"
                });
            SetPropertyInternalEntityEntryTest(
                model, model.FindEntityType(typeof(ChangedOnlyEntity).FullName), new ChangedOnlyEntity
                {
                    Id = 1,
                    Name = "Kool"
                });
            SetPropertyInternalEntityEntryTest(
                model, model.FindEntityType(typeof(SomeEntity).FullName), new SomeEntity
                {
                    Id = 1,
                    Name = "Kool"
                });
        }

        protected void SetPropertyInternalEntityEntryTest(IModel model, IEntityType entityType, object entity)
        {
            var idProperty = entityType.FindProperty("Id");
            var nameProperty = entityType.FindProperty("Name");
            var configuration = InMemoryTestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, entity, new ValueBuffer(new object[] { 1, "Kool" }));
            entry.SetEntityState(EntityState.Unchanged);

            Assert.False(entry.IsModified(idProperty));
            Assert.False(entry.IsModified(nameProperty));
            Assert.Equal(EntityState.Unchanged, entry.EntityState);

            entry[idProperty] = 1;
            entry[nameProperty] = "Kool";

            Assert.False(entry.IsModified(idProperty));
            Assert.False(entry.IsModified(nameProperty));
            Assert.Equal(EntityState.Unchanged, entry.EntityState);

            entry[nameProperty] = "Beans";

            Assert.False(entry.IsModified(idProperty));
            Assert.True(entry.IsModified(nameProperty));
            Assert.Equal(EntityState.Modified, entry.EntityState);
        }

        protected void SetPropertyClrTest<TEntity>(TEntity entity, bool needsDetectChanges)
            where TEntity : ISomeEntity
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(TEntity));
            var nameProperty = entityType.FindProperty("Name");
            var configuration = InMemoryTestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, entity, new ValueBuffer(new object[] { 1, "Kool" }));
            entry.SetEntityState(EntityState.Unchanged);

            Assert.False(entry.IsModified(nameProperty));
            Assert.Equal(EntityState.Unchanged, entry.EntityState);

            entity.Name = "Kool";

            Assert.False(entry.IsModified(nameProperty));
            Assert.Equal(EntityState.Unchanged, entry.EntityState);

            entity.Name = "Beans";

            if (needsDetectChanges)
            {
                Assert.False(entry.IsModified(nameProperty));
                Assert.Equal(EntityState.Unchanged, entry.EntityState);

                configuration.GetRequiredService<IChangeDetector>().DetectChanges(entry);
            }

            Assert.True(entry.IsModified(nameProperty));
            Assert.Equal(EntityState.Modified, entry.EntityState);
        }

        [Fact]
        public virtual void AcceptChanges_does_nothing_for_unchanged_entities()
            => AcceptChangesNoop(EntityState.Unchanged);

        [Fact]
        public virtual void AcceptChanges_does_nothing_for_unknown_entities()
            => AcceptChangesNoop(EntityState.Detached);

        private void AcceptChangesNoop(EntityState entityState)
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var configuration = InMemoryTestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(
                configuration,
                entityType,
                new SomeEntity
                {
                    Id = 1,
                    Name = "Kool"
                },
                new ValueBuffer(new object[] { 1, "Kool" }));

            entry.SetEntityState(entityState);

            entry.AcceptChanges();

            Assert.Equal(entityState, entry.EntityState);
        }

        [Fact]
        public virtual void AcceptChanges_makes_Modified_entities_Unchanged_and_resets_used_original_values()
        {
            AcceptChangesKeep(EntityState.Modified);
        }

        [Fact]
        public virtual void AcceptChanges_makes_Added_entities_Unchanged()
        {
            AcceptChangesKeep(EntityState.Added);
        }

        private void AcceptChangesKeep(EntityState entityState)
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var nameProperty = entityType.FindProperty("Name");
            var configuration = InMemoryTestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(
                configuration,
                entityType,
                new SomeEntity
                {
                    Id = 1,
                    Name = "Kool"
                },
                new ValueBuffer(new object[] { 1, "Kool" }));

            entry.SetEntityState(entityState);

            entry[nameProperty] = "Pickle";
            entry.SetOriginalValue(nameProperty, "Cheese");

            entry.AcceptChanges();

            Assert.Equal(EntityState.Unchanged, entry.EntityState);
            Assert.Equal("Pickle", entry[nameProperty]);
            Assert.Equal("Pickle", entry.GetOriginalValue(nameProperty));
        }

        [Fact]
        public virtual void AcceptChanges_makes_Modified_entities_Unchanged_and_effectively_resets_unused_original_values()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var nameProperty = entityType.FindProperty("Name");
            var configuration = InMemoryTestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(
                configuration,
                entityType,
                new SomeEntity
                {
                    Id = 1,
                    Name = "Kool"
                },
                new ValueBuffer(new object[] { 1, "Kool" }));

            entry.SetEntityState(EntityState.Modified);

            entry[nameProperty] = "Pickle";

            entry.AcceptChanges();

            Assert.Equal(EntityState.Unchanged, entry.EntityState);
            Assert.Equal("Pickle", entry[nameProperty]);
            Assert.Equal("Pickle", entry.GetOriginalValue(nameProperty));
        }

        [Fact]
        public virtual void AcceptChanges_detaches_Deleted_entities()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var configuration = InMemoryTestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(
                configuration,
                entityType,
                new SomeEntity
                {
                    Id = 1,
                    Name = "Kool"
                },
                new ValueBuffer(new object[] { 1, "Kool" }));

            entry.SetEntityState(EntityState.Deleted);

            entry.AcceptChanges();

            Assert.Equal(EntityState.Detached, entry.EntityState);
        }

        [Fact]
        public virtual void AcceptChanges_does_nothing_for_unchanged_owned_entities()
            => AcceptChangesOwned(EntityState.Unchanged);

        [Fact]
        public virtual void AcceptChanges_does_nothing_for_unknown_owned_entities()
            => AcceptChangesOwned(EntityState.Detached);

        [Fact]
        public virtual void AcceptChanges_makes_Modified_owned_entities_Unchanged_and_resets_used_original_values()
            => AcceptChangesOwned(EntityState.Modified);

        [Fact]
        public virtual void AcceptChanges_makes_Added_owned_entities_Unchanged()
            => AcceptChangesOwned(EntityState.Added);

        [Fact]
        public virtual void AcceptChanges_detaches_Deleted_owned_entities()
            => AcceptChangesOwned(EntityState.Deleted);

        private void AcceptChangesOwned(EntityState entityState)
        {
            var model = BuildModel();
            var ownerType = model.FindEntityType(typeof(OwnerClass).FullName);
            var ownedType = ownerType.FindNavigation(nameof(OwnerClass.Owned)).GetTargetType();
            var valueProperty = ownedType.FindProperty(nameof(OwnedClass.Value));
            var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(
                contextServices,
                ownedType,
                new OwnedClass
                {
                    Value = "Kool"
                },
                new ValueBuffer(new object[] { 1, "Kool" }));

            entry.SetEntityState(entityState);

            if (entityState != EntityState.Unchanged)
            {
                entry[valueProperty] = "Pickle";
            }

            entry.SetOriginalValue(valueProperty, "Cheese");

            entry.AcceptChanges();

            Assert.Equal(
                entityState == EntityState.Deleted || entityState == EntityState.Detached ? EntityState.Detached : EntityState.Unchanged,
                entry.EntityState);
            if (entityState == EntityState.Unchanged)
            {
                Assert.Equal("Kool", entry[valueProperty]);
                Assert.Equal("Kool", entry.GetOriginalValue(valueProperty));
            }
            else
            {
                Assert.Equal("Pickle", entry[valueProperty]);
                Assert.Equal(
                    entityState == EntityState.Detached || entityState == EntityState.Deleted ? "Cheese" : "Pickle",
                    entry.GetOriginalValue(valueProperty));
            }
        }

        [Fact]
        public virtual void Non_transparent_sidecar_does_not_intercept_normal_property_read_and_write()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var idProperty = entityType.FindProperty("Id");
            var nameProperty = entityType.FindProperty("Name");

            var entry = CreateInternalEntry(
                InMemoryTestHelpers.Instance.CreateContextServices(model),
                entityType,
                new SomeEntity
                {
                    Id = 1,
                    Name = "Kool"
                },
                new ValueBuffer(new object[] { 1, "Kool" }));

            entry.SetEntityState(EntityState.Added);

            Assert.Equal(1, entry[idProperty]);
            Assert.Equal("Kool", entry[nameProperty]);

            entry.SetOriginalValue(idProperty, 7);

            Assert.Equal(1, entry[idProperty]);
            Assert.Equal("Kool", entry[nameProperty]);

            entry[idProperty] = 77;

            Assert.Equal(77, entry[idProperty]);
            Assert.Equal("Kool", entry[nameProperty]);
        }

        private static IModel BuildOneToOneModel()
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

            modelBuilder
                .Entity<FirstDependent>()
                .HasOne(e => e.Second)
                .WithOne(e => e.First)
                .HasForeignKey<SecondDependent>(e => e.Id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder
                .Entity<Root>(
                    b =>
                    {
                        b.Property(e => e.Id).ValueGeneratedNever();

                        b.HasOne(e => e.First)
                            .WithOne(e => e.Root)
                            .HasForeignKey<FirstDependent>(e => e.Id);
                    });

            return modelBuilder.Model;
        }

        private static IModel BuildOneToOneCompositeModel(bool required)
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

            modelBuilder
                .Entity<CompositeRoot>()
                .HasKey(
                    e => new
                    {
                        e.Id1,
                        e.Id2
                    });

            modelBuilder
                .Entity<CompositeFirstDependent>()
                .HasKey(
                    e => new
                    {
                        e.Id1,
                        e.Id2
                    });

            modelBuilder
                .Entity<CompositeSecondDependent>()
                .HasKey(
                    e => new
                    {
                        e.Id1,
                        e.Id2
                    });

            modelBuilder
                .Entity<CompositeRoot>()
                .HasOne(e => e.First)
                .WithOne(e => e.Root)
                .HasForeignKey<CompositeFirstDependent>(
                    e => new
                    {
                        e.RootId1,
                        e.RootId2
                    })
                .IsRequired(required);

            modelBuilder
                .Entity<CompositeFirstDependent>()
                .HasOne(e => e.Second)
                .WithOne(e => e.First)
                .HasForeignKey<CompositeSecondDependent>(
                    e => new
                    {
                        e.FirstId1,
                        e.FirstId2
                    })
                .IsRequired(required);

            return modelBuilder.Model;
        }

        [Fact]
        public void Unchanged_entity_with_conceptually_null_FK_with_cascade_delete_is_marked_Deleted()
        {
            var model = BuildOneToOneModel();
            var entityType = model.FindEntityType(typeof(SecondDependent).FullName);
            var fkProperty = entityType.FindProperty("Id");

            var entry = CreateInternalEntry(InMemoryTestHelpers.Instance.CreateContextServices(model), entityType, new SecondDependent());

            entry[fkProperty] = 77;
            entry.SetEntityState(EntityState.Unchanged);

            entry[fkProperty] = null;
            entry.HandleConceptualNulls(false, force: false, isCascadeDelete: false);

            Assert.Equal(EntityState.Deleted, entry.EntityState);
        }

        [Fact]
        public void Added_entity_with_conceptually_null_FK_with_cascade_delete_is_detached()
        {
            var model = BuildOneToOneModel();
            var entityType = model.FindEntityType(typeof(SecondDependent).FullName);
            var fkProperty = entityType.FindProperty("Id");

            var entry = CreateInternalEntry(InMemoryTestHelpers.Instance.CreateContextServices(model), entityType, new SecondDependent());

            entry[fkProperty] = 77;
            entry.SetEntityState(EntityState.Added);

            entry[fkProperty] = null;
            entry.HandleConceptualNulls(false, force: false, isCascadeDelete: false);

            Assert.Equal(EntityState.Detached, entry.EntityState);
        }

        [Fact]
        public void Entity_with_partially_null_composite_FK_with_cascade_delete_is_marked_Deleted()
        {
            var model = BuildOneToOneCompositeModel(required: true);
            var entityType = model.FindEntityType(typeof(CompositeSecondDependent).FullName);
            var fkProperty1 = entityType.FindProperty("FirstId1");
            var fkProperty2 = entityType.FindProperty("FirstId2");

            var entry = CreateInternalEntry(
                InMemoryTestHelpers.Instance.CreateContextServices(model), entityType, new CompositeSecondDependent());

            entry[entityType.FindProperty("Id1")] = 66;
            entry[entityType.FindProperty("Id2")] = "Bar";
            entry[fkProperty1] = 77;
            entry[fkProperty2] = "Foo";
            entry.SetEntityState(EntityState.Unchanged);

            entry[fkProperty1] = null;
            entry.HandleConceptualNulls(false, force: false, isCascadeDelete: false);

            Assert.Equal(EntityState.Deleted, entry.EntityState);
        }

        [Fact]
        public void Entity_with_partially_null_composite_FK_without_cascade_delete_is_orphaned()
        {
            var model = BuildOneToOneCompositeModel(required: false);
            var entityType = model.FindEntityType(typeof(CompositeSecondDependent).FullName);
            var fkProperty1 = entityType.FindProperty("FirstId1");
            var fkProperty2 = entityType.FindProperty("FirstId2");

            var entry = CreateInternalEntry(
                InMemoryTestHelpers.Instance.CreateContextServices(model), entityType, new CompositeSecondDependent());

            entry[entityType.FindProperty("Id1")] = 66;
            entry[entityType.FindProperty("Id2")] = "Bar";
            entry[fkProperty1] = 77;
            entry[fkProperty2] = "Foo";
            entry.SetEntityState(EntityState.Unchanged);

            entry[fkProperty1] = null;
            entry.HandleConceptualNulls(false, force: false, isCascadeDelete: false);

            Assert.Equal(EntityState.Modified, entry.EntityState);

            Assert.Equal(77, entry[fkProperty1]);
            Assert.Null(entry[fkProperty2]);
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class Root
        {
            public int Id { get; set; }

            public FirstDependent First { get; set; }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class FirstDependent
        {
            public int Id { get; set; }

            public Root Root { get; set; }

            public SecondDependent Second { get; set; }
        }

        private class SecondDependent
        {
            public int Id { get; set; }

            public FirstDependent First { get; set; }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class CompositeRoot
        {
            public int Id1 { get; set; }
            public string Id2 { get; set; }

            public CompositeFirstDependent First { get; set; }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class CompositeFirstDependent
        {
            public int Id1 { get; set; }
            public string Id2 { get; set; }

            public int RootId1 { get; set; }
            public string RootId2 { get; set; }

            // ReSharper disable once MemberHidesStaticFromOuterClass
            public CompositeRoot Root { get; set; }

            public CompositeSecondDependent Second { get; set; }
        }

        private class CompositeSecondDependent
        {
            public int Id1 { get; set; }
            public string Id2 { get; set; }

            public int FirstId1 { get; set; }
            public string FirstId2 { get; set; }
            public CompositeFirstDependent First { get; set; }
        }

        protected virtual InternalEntityEntry CreateInternalEntry(IServiceProvider contextServices, IEntityType entityType, object entity)
        {
            var entry = new InternalEntityEntryFactory()
                .Create(contextServices.GetRequiredService<IStateManager>(), entityType, entity);

            contextServices.GetRequiredService<IInternalEntityEntrySubscriber>().SnapshotAndSubscribe(entry);
            return entry;
        }

        protected virtual InternalEntityEntry CreateInternalEntry(
            IServiceProvider contextServices, IEntityType entityType, object entity, in ValueBuffer valueBuffer)
        {
            var entry = new InternalEntityEntryFactory()
                .Create(contextServices.GetRequiredService<IStateManager>(), entityType, entity, valueBuffer);

            contextServices.GetRequiredService<IInternalEntityEntrySubscriber>().SnapshotAndSubscribe(entry);
            return entry;
        }

        protected virtual Model BuildModel()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());
            var model = modelBuilder.Model;

            var someSimpleEntityType = model.AddEntityType(typeof(SomeSimpleEntityBase));
            var simpleKeyProperty = someSimpleEntityType.AddProperty("Id", typeof(int));
            simpleKeyProperty.ValueGenerated = ValueGenerated.OnAdd;
            someSimpleEntityType.GetOrSetPrimaryKey(simpleKeyProperty);

            var someCompositeEntityType = model.AddEntityType(typeof(SomeCompositeEntityBase));
            var compositeKeyProperty1 = someCompositeEntityType.AddProperty("Id1", typeof(int));
            var compositeKeyProperty2 = someCompositeEntityType.AddProperty("Id2", typeof(string));
            compositeKeyProperty2.IsNullable = false;
            someCompositeEntityType.GetOrSetPrimaryKey(new[] { compositeKeyProperty1, compositeKeyProperty2 });

            var entityType1 = model.AddEntityType(typeof(SomeEntity));
            entityType1.BaseType = someSimpleEntityType;
            var property3 = entityType1.AddProperty("Name", typeof(string));
            property3.IsConcurrencyToken = true;
            property3.ValueGenerated = ValueGenerated.OnAdd;

            var entityType2 = model.AddEntityType(typeof(SomeDependentEntity));
            entityType2.BaseType = someCompositeEntityType;
            var fk = entityType2.AddProperty("SomeEntityId", typeof(int));
            entityType2.GetOrAddForeignKey(new[] { fk }, entityType1.FindPrimaryKey(), entityType1);
            // TODO: declare this on the derived type
            // #2611
            var justAProperty = someCompositeEntityType.AddProperty("JustAProperty", typeof(int));
            justAProperty.ValueGenerated = ValueGenerated.OnAdd;
            someCompositeEntityType.AddKey(justAProperty);

            var entityType3 = model.AddEntityType(typeof(FullNotificationEntity));
            var property6 = entityType3.AddProperty("Id", typeof(int));
            entityType3.GetOrSetPrimaryKey(property6);
            var property7 = entityType3.AddProperty("Name", typeof(string));
            property7.IsConcurrencyToken = true;
            ((EntityType)entityType3).ChangeTrackingStrategy = ChangeTrackingStrategy.ChangingAndChangedNotifications;

            var entityType4 = model.AddEntityType(typeof(ChangedOnlyEntity));
            var property8 = entityType4.AddProperty("Id", typeof(int));
            entityType4.GetOrSetPrimaryKey(property8);
            var property9 = entityType4.AddProperty("Name", typeof(string));
            property9.IsConcurrencyToken = true;
            ((EntityType)entityType4).ChangeTrackingStrategy = ChangeTrackingStrategy.ChangedNotifications;

            var entityType5 = model.AddEntityType(typeof(SomeMoreDependentEntity));
            entityType5.BaseType = someSimpleEntityType;
            var fk5a = entityType5.AddProperty("Fk1", typeof(int));
            var fk5b = entityType5.AddProperty("Fk2", typeof(string));
            entityType5.GetOrAddForeignKey(new[] { fk5a, fk5b }, entityType2.FindPrimaryKey(), entityType2);

            modelBuilder.Entity<OwnerClass>(
                eb =>
                {
                    eb.HasKey(e => e.Id);
                    var owned = eb.OwnsOne(e => e.Owned);
                    owned.WithOwner().HasForeignKey("Id");
                    owned.HasKey("Id");
                    owned.Property(e => e.Value);
                });

            return (Model)model;
        }

        protected interface ISomeEntity
        {
            int Id { get; set; }
            string Name { get; set; }
        }

        protected class SomeSimpleEntityBase
        {
            public int Id { get; set; }
        }

        protected class SomeEntity : SomeSimpleEntityBase, ISomeEntity
        {
            public string Name { get; set; }
        }

        protected class SomeCompositeEntityBase
        {
            public int Id1 { get; set; }
            public string Id2 { get; set; }
        }

        protected class SomeDependentEntity : SomeCompositeEntityBase
        {
            public int SomeEntityId { get; set; }
            public int JustAProperty { get; set; }
        }

        protected class SomeMoreDependentEntity : SomeSimpleEntityBase
        {
            public int Fk1 { get; set; }
            public string Fk2 { get; set; }
        }

        protected class FullNotificationEntity : INotifyPropertyChanging, INotifyPropertyChanged, ISomeEntity
        {
            private int _id;
            private string _name;

            public int Id
            {
                get => _id;
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
                get => _name;
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

            private void NotifyChanged([CallerMemberName] string propertyName = "")
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            private void NotifyChanging([CallerMemberName] string propertyName = "")
                => PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
        }

        protected class ChangedOnlyEntity : INotifyPropertyChanged, ISomeEntity
        {
            private int _id;
            private string _name;

            public int Id
            {
                get => _id;
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
                get => _name;
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

            private void NotifyChanged([CallerMemberName] string propertyName = "")
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected class OwnerClass
        {
            public int Id { get; set; }
            public virtual OwnedClass Owned { get; set; }
        }

        protected class OwnedClass
        {
            public string Value { get; set; }
        }
    }
}
