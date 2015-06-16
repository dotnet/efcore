// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.ValueGeneration;
using Microsoft.Data.Entity.InMemory;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Framework.DependencyInjection;
using Moq;
using Xunit;
using Strings = Microsoft.Data.Entity.Internal.Strings;

namespace Microsoft.Data.Entity.Tests.ChangeTracking
{
    public class InternalEntityEntryTest
    {
        [Fact]
        public void Changing_state_from_Unknown_causes_entity_to_start_tracking()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.GetProperty("Id");

            var contextServices = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(contextServices, entityType, new SomeEntity());
            entry[keyProperty] = 1;

            entry.SetEntityState(EntityState.Added);

            Assert.Equal(EntityState.Added, entry.EntityState);
            Assert.Contains(entry, contextServices.GetRequiredService<IStateManager>().Entries);
        }

        [Fact]
        public void Changing_state_to_Unknown_causes_entity_to_stop_tracking()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.GetProperty("Id");

            var contextServices = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(contextServices, entityType, new SomeEntity());
            entry[keyProperty] = 1;

            entry.SetEntityState(EntityState.Added);
            entry.SetEntityState(EntityState.Detached);

            Assert.Equal(EntityState.Detached, entry.EntityState);
            Assert.DoesNotContain(entry, contextServices.GetRequiredService<IStateManager>().Entries);
        }

        [Fact] // GitHub #251, #1247
        public void Changing_state_from_Added_to_Deleted_does_what_you_ask()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.GetProperty("Id");

            var contextServices = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(contextServices, entityType, new SomeEntity());
            entry[keyProperty] = 1;

            entry.SetEntityState(EntityState.Added);
            entry.SetEntityState(EntityState.Deleted);

            Assert.Equal(EntityState.Deleted, entry.EntityState);
            Assert.Contains(entry, contextServices.GetRequiredService<IStateManager>().Entries);
        }

        [Fact]
        public void Changing_state_to_Modified_or_Unchanged_causes_all_properties_to_be_marked_accordingly()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.GetProperty("Id");
            var nonKeyProperty = entityType.GetProperty("Name");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeEntity());
            entry[keyProperty] = 1;

            Assert.False(entry.IsPropertyModified(keyProperty));
            Assert.False(entry.IsPropertyModified(nonKeyProperty));

            entry.SetEntityState(EntityState.Modified);

            Assert.False(entry.IsPropertyModified(keyProperty));
            Assert.True(entry.IsPropertyModified(nonKeyProperty));

            entry.SetEntityState(EntityState.Unchanged, true);

            Assert.False(entry.IsPropertyModified(keyProperty));
            Assert.False(entry.IsPropertyModified(nonKeyProperty));

            entry.SetPropertyModified(nonKeyProperty);

            Assert.Equal(EntityState.Modified, entry.EntityState);
            Assert.False(entry.IsPropertyModified(keyProperty));
            Assert.True(entry.IsPropertyModified(nonKeyProperty));
        }

        [Fact]
        public void Read_only_before_save_properties_throw_if_not_null_or_temp()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.GetProperty("Id");
            var nonKeyProperty = entityType.GetProperty("Name");
            nonKeyProperty.IsReadOnlyBeforeSave = true;
            keyProperty.IsReadOnlyBeforeSave = true;
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeEntity());

            entry.SetEntityState(EntityState.Added);
            entry.MarkAsTemporary(keyProperty);

            entry[nonKeyProperty] = "Jillybean";

            Assert.Equal(
                Strings.PropertyReadOnlyBeforeSave("Name", typeof(SomeEntity).Name),
                Assert.Throws<InvalidOperationException>(() => entry.PrepareToSave()).Message);

            entry[nonKeyProperty] = null;

            entry[keyProperty] = 2;

            Assert.Equal(
                Strings.PropertyReadOnlyBeforeSave("Id", typeof(SomeEntity).Name),
                Assert.Throws<InvalidOperationException>(() => entry.PrepareToSave()).Message);
        }

        [Fact]
        public void Read_only_after_save_properties_throw_if_modified()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);
            var nonKeyProperty = entityType.GetProperty("Name");
            nonKeyProperty.IsReadOnlyAfterSave = true;
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeEntity());

            entry[entityType.GetProperty("Id")] = 1;
            entry[nonKeyProperty] = "Jillybean";

            entry.SetEntityState(EntityState.Modified);

            Assert.False(entry.IsPropertyModified(nonKeyProperty));

            entry.SetEntityState(EntityState.Unchanged, true);

            Assert.False(entry.IsPropertyModified(nonKeyProperty));

            entry.SetPropertyModified(nonKeyProperty);

            Assert.True(entry.IsPropertyModified(nonKeyProperty));

            Assert.Equal(
                Strings.PropertyReadOnlyAfterSave("Name", typeof(SomeEntity).Name),
                Assert.Throws<InvalidOperationException>(() => entry.PrepareToSave()).Message);

            entry.SetPropertyModified(nonKeyProperty, isModified: false);

            entry[nonKeyProperty] = "Beanjilly";

            Assert.True(entry.IsPropertyModified(nonKeyProperty));

            Assert.Equal(
                Strings.PropertyReadOnlyAfterSave("Name", typeof(SomeEntity).Name),
                Assert.Throws<InvalidOperationException>(() => entry.PrepareToSave()).Message);
        }

        [Fact]
        public void Key_properties_throw_immediately_if_modified()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.GetProperty("Id");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeEntity());

            entry[keyProperty] = 1;

            entry.SetEntityState(EntityState.Modified);

            Assert.False(entry.IsPropertyModified(keyProperty));

            entry.SetEntityState(EntityState.Unchanged, true);

            Assert.False(entry.IsPropertyModified(keyProperty));

            Assert.Equal(
                Strings.KeyReadOnly("Id", typeof(SomeEntity).Name),
                Assert.Throws<NotSupportedException>(() => entry.SetPropertyModified(keyProperty)).Message);

            Assert.Equal(EntityState.Unchanged, entry.EntityState);
            Assert.False(entry.IsPropertyModified(keyProperty));

            Assert.Equal(
                Strings.KeyReadOnly("Id", typeof(SomeEntity).Name),
                Assert.Throws<NotSupportedException>(() => entry[keyProperty] = 2).Message);


            Assert.Equal(EntityState.Unchanged, entry.EntityState);
            Assert.False(entry.IsPropertyModified(keyProperty));
        }

        [Fact]
        public void Added_entities_can_have_temporary_values()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.GetProperty("Id");
            var nonKeyProperty = entityType.GetProperty("Name");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeEntity());
            entry[keyProperty] = 1;

            Assert.False(entry.HasTemporaryValue(keyProperty));
            Assert.False(entry.HasTemporaryValue(nonKeyProperty));
            Assert.False(entry.IsPropertyModified(keyProperty));
            Assert.False(entry.IsPropertyModified(nonKeyProperty));

            entry.SetEntityState(EntityState.Added);

            Assert.False(entry.HasTemporaryValue(keyProperty));
            Assert.False(entry.HasTemporaryValue(nonKeyProperty));
            Assert.False(entry.IsPropertyModified(keyProperty));
            Assert.False(entry.IsPropertyModified(nonKeyProperty));

            entry.MarkAsTemporary(keyProperty);

            Assert.True(entry.HasTemporaryValue(keyProperty));
            Assert.False(entry.HasTemporaryValue(nonKeyProperty));
            Assert.False(entry.IsPropertyModified(keyProperty));
            Assert.False(entry.IsPropertyModified(nonKeyProperty));

            entry.MarkAsTemporary(nonKeyProperty);
            entry.MarkAsTemporary(keyProperty, isTemporary: false);

            Assert.False(entry.HasTemporaryValue(keyProperty));
            Assert.True(entry.HasTemporaryValue(nonKeyProperty));
            Assert.False(entry.IsPropertyModified(keyProperty));
            Assert.False(entry.IsPropertyModified(nonKeyProperty));

            entry[nonKeyProperty] = "I Am A Real Person!";

            entry.SetEntityState(EntityState.Unchanged);

            Assert.False(entry.HasTemporaryValue(keyProperty));
            Assert.False(entry.HasTemporaryValue(nonKeyProperty));
            Assert.False(entry.IsPropertyModified(keyProperty));
            Assert.False(entry.IsPropertyModified(nonKeyProperty));

            entry.MarkAsTemporary(keyProperty);
            entry.MarkAsTemporary(nonKeyProperty);

            Assert.False(entry.HasTemporaryValue(keyProperty));
            Assert.False(entry.HasTemporaryValue(nonKeyProperty));
            Assert.False(entry.IsPropertyModified(keyProperty));
            Assert.False(entry.IsPropertyModified(nonKeyProperty));

            entry.SetEntityState(EntityState.Added);

            Assert.False(entry.HasTemporaryValue(keyProperty));
            Assert.False(entry.HasTemporaryValue(nonKeyProperty));
            Assert.False(entry.IsPropertyModified(keyProperty));
            Assert.False(entry.IsPropertyModified(nonKeyProperty));
        }

        [Theory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public void Changing_state_with_temp_value_throws(EntityState targetState)
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.GetProperty("Id");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeEntity());

            entry.SetEntityState(EntityState.Added);
            entry.MarkAsTemporary(keyProperty);

            Assert.Equal(
                Strings.TempValuePersists("Id", "SomeEntity", targetState.ToString()),
                Assert.Throws<InvalidOperationException>(() => entry.SetEntityState(targetState)).Message);
        }

        [Fact]
        public void Detaching_with_temp_values_does_not_throw()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.GetProperty("Id");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeEntity());
            entry[keyProperty] = 1;

            entry.SetEntityState(EntityState.Added);
            entry.MarkAsTemporary(keyProperty);

            entry.SetEntityState(EntityState.Detached);

            Assert.False(entry.HasTemporaryValue(keyProperty));

            entry[keyProperty] = 1;
            entry.SetEntityState(EntityState.Unchanged);

            Assert.False(entry.HasTemporaryValue(keyProperty));
        }

        [Fact]
        public void Setting_an_explicit_value_marks_property_as_not_temporary()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.GetProperty("Id");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeEntity());

            entry.SetEntityState(EntityState.Added);
            entry.MarkAsTemporary(keyProperty);

            Assert.True(entry.HasTemporaryValue(keyProperty));

            entry[keyProperty] = 77;

            Assert.False(entry.HasTemporaryValue(keyProperty));

            entry.SetEntityState(EntityState.Unchanged); // Does not throw
        }

        [Fact]
        public void Changing_state_to_Added_triggers_key_generation()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.GetProperty("Id");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeEntity());

            if (keyProperty.IsShadowProperty)
            {
                Assert.Null(entry[keyProperty]);
            }
            else
            {
                Assert.Equal(0, entry[keyProperty]);
            }

            entry.SetEntityState(EntityState.Added);

            Assert.NotNull(entry[keyProperty]);
            Assert.NotEqual(0, entry[keyProperty]);
        }

        [Fact]
        public void Value_generation_does_not_happen_if_property_has_non_default_value()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.GetProperty("Id");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeEntity());

            entry[keyProperty] = 31143;

            entry.SetEntityState(EntityState.Added);

            Assert.Equal(31143, entry[keyProperty]);
        }

        [Fact]
        public void Temporary_values_are_reset_when_entity_is_detached()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.GetProperty("Id");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeEntity());

            entry.SetEntityState(EntityState.Added);
            entry.MarkAsTemporary(keyProperty);

            Assert.NotNull(entry[keyProperty]);
            Assert.NotEqual(0, entry[keyProperty]);

            entry.SetEntityState(EntityState.Detached);

            Assert.Equal(0, entry[keyProperty]);
        }

        [Fact]
        public void Modified_values_are_reset_when_entity_is_changed_to_Added()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);
            var property = entityType.GetProperty("Name");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeEntity());
            entry[entityType.GetProperty("Id")] = 1;

            entry.SetEntityState(EntityState.Modified);
            entry.SetPropertyModified(property);

            entry.SetEntityState(EntityState.Added);

            Assert.False(entry.HasTemporaryValue(property));
        }

        [Fact]
        public void Changing_state_to_Added_triggers_value_generation_for_any_property()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeDependentEntity).FullName);
            var keyProperties = new[] { entityType.GetProperty("Id1"), entityType.GetProperty("Id2") };
            var fkProperty = entityType.GetProperty("SomeEntityId");
            var property = entityType.GetProperty("JustAProperty");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeDependentEntity());
            entry[keyProperties[0]] = 77;
            entry[keyProperties[1]] = "ReadySalted";
            entry[fkProperty] = 0;

            if (property.IsShadowProperty)
            {
                Assert.Null(entry[property]);
            }
            else
            {
                Assert.Equal(0, entry[property]);
            }

            entry.SetEntityState(EntityState.Added);

            Assert.NotNull(entry[property]);
            Assert.NotEqual(0, entry[property]);
        }

        [Fact]
        public void Can_create_primary_key()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.GetProperty("Id");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeEntity());
            entry[keyProperty] = 77;

            var keyValue = entry.GetPrimaryKeyValue();
            Assert.IsType<SimpleEntityKey<int>>(keyValue);
            Assert.Equal(77, keyValue.Value);
        }

        [Fact]
        public void Can_create_composite_primary_key()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeDependentEntity).FullName);
            var keyProperties = new[] { entityType.GetProperty("Id1"), entityType.GetProperty("Id2") };
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeDependentEntity());
            entry[keyProperties[0]] = 77;
            entry[keyProperties[1]] = "SmokeyBacon";

            var keyValue = (CompositeEntityKey)entry.GetPrimaryKeyValue();
            Assert.Equal(77, keyValue.Value[0]);
            Assert.Equal("SmokeyBacon", keyValue.Value[1]);
        }

        [Fact]
        public void Can_create_foreign_key_value_based_on_dependent_values()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeDependentEntity).FullName);
            var fkProperty = entityType.GetProperty("SomeEntityId");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeDependentEntity());
            entry[fkProperty] = 77;
            entry.RelationshipsSnapshot[fkProperty] = 78;

            var keyValue = entry.GetDependentKeyValue(entityType.GetForeignKeys().Single());
            Assert.IsType<SimpleEntityKey<int>>(keyValue);
            Assert.Equal(77, keyValue.Value);
        }

        [Fact]
        public void Can_create_foreign_key_value_based_on_snapshot_dependent_values()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeDependentEntity).FullName);
            var fkProperty = entityType.GetProperty("SomeEntityId");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeDependentEntity());
            entry[fkProperty] = 77;
            entry.RelationshipsSnapshot[fkProperty] = 78;

            var keyValue = entry.GetDependentKeySnapshot(entityType.GetForeignKeys().Single());
            Assert.IsType<SimpleEntityKey<int>>(keyValue);
            Assert.Equal(78, keyValue.Value);
        }

        [Fact]
        public void Can_create_foreign_key_value_based_on_snapshot_dependent_values_if_value_not_yet_snapshotted()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeDependentEntity).FullName);
            var fkProperty = entityType.GetProperty("SomeEntityId");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeDependentEntity());
            entry[fkProperty] = 77;

            var keyValue = entry.GetDependentKeySnapshot(entityType.GetForeignKeys().Single());
            Assert.IsType<SimpleEntityKey<int>>(keyValue);
            Assert.Equal(77, keyValue.Value);
        }

        [Fact]
        public void Notification_that_an_FK_property_has_changed_updates_the_snapshot()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeDependentEntity).FullName);
            var fkProperty = entityType.GetProperty("SomeEntityId");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeDependentEntity());
            entry[fkProperty] = 77;
            entry.RelationshipsSnapshot[fkProperty] = 78;

            entry[fkProperty] = 79;

            var keyValue = entry.GetDependentKeySnapshot(entityType.GetForeignKeys().Single());
            Assert.IsType<SimpleEntityKey<int>>(keyValue);
            Assert.Equal(79, keyValue.Value);
        }

        [Fact]
        public void Setting_property_to_the_same_value_does_not_update_the_snapshot()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeDependentEntity).FullName);
            var fkProperty = entityType.GetProperty("SomeEntityId");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeDependentEntity());
            entry[fkProperty] = 77;
            entry.RelationshipsSnapshot[fkProperty] = 78;

            entry[fkProperty] = 77;

            var keyValue = entry.GetDependentKeySnapshot(entityType.GetForeignKeys().Single());
            Assert.IsType<SimpleEntityKey<int>>(keyValue);
            Assert.Equal(78, keyValue.Value);
        }

        [Fact]
        public void Can_create_foreign_key_value_based_on_principal_end_values()
        {
            var model = BuildModel();
            var principalType = model.GetEntityType(typeof(SomeEntity).FullName);
            var dependentType = model.GetEntityType(typeof(SomeDependentEntity).FullName);
            var key = principalType.GetProperty("Id");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, principalType, new SomeEntity());
            entry[key] = 77;

            var keyValue = entry.GetPrincipalKeyValue(dependentType.GetForeignKeys().Single());
            Assert.IsType<SimpleEntityKey<int>>(keyValue);
            Assert.Equal(77, keyValue.Value);
        }

        [Fact]
        public void Can_create_composite_foreign_key_value_based_on_dependent_values()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeMoreDependentEntity).FullName);
            var fkProperties = new[] { entityType.GetProperty("Fk1"), entityType.GetProperty("Fk2") };
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeMoreDependentEntity());
            entry[fkProperties[0]] = 77;
            entry[fkProperties[1]] = "CheeseAndOnion";

            var keyValue = (CompositeEntityKey)entry.GetDependentKeyValue(entityType.GetForeignKeys().Single());
            Assert.Equal(77, keyValue.Value[0]);
            Assert.Equal("CheeseAndOnion", keyValue.Value[1]);
        }

        [Fact]
        public void Can_create_composite_foreign_key_value_based_on_principal_end_values()
        {
            var model = BuildModel();
            var principalType = model.GetEntityType(typeof(SomeDependentEntity).FullName);
            var dependentType = model.GetEntityType(typeof(SomeMoreDependentEntity).FullName);
            var keyProperties = new[] { principalType.GetProperty("Id1"), principalType.GetProperty("Id2") };
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, principalType, new SomeDependentEntity());
            entry[keyProperties[0]] = 77;
            entry[keyProperties[1]] = "PrawnCocktail";

            var keyValue = (CompositeEntityKey)entry.GetPrincipalKeyValue(dependentType.GetForeignKeys().Single());
            Assert.Equal(77, keyValue.Value[0]);
            Assert.Equal("PrawnCocktail", keyValue.Value[1]);
        }

        [Fact]
        public void Can_create_composite_foreign_key_value_based_on_principal_end_values_with_nulls()
        {
            var model = BuildModel();
            var principalType = model.GetEntityType(typeof(SomeDependentEntity).FullName);
            var dependentType = model.GetEntityType(typeof(SomeMoreDependentEntity).FullName);
            var keyProperties = new[] { principalType.GetProperty("Id1"), principalType.GetProperty("Id2") };
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, principalType, new SomeDependentEntity());
            entry[keyProperties[0]] = 77;
            entry[keyProperties[1]] = null;

            Assert.Same(EntityKey.InvalidEntityKey, entry.GetPrincipalKeyValue(dependentType.GetForeignKeys().Single()));
        }

        [Fact]
        public void Can_get_property_value_after_creation_from_value_buffer()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.GetProperty("Id");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(
                configuration,
                entityType,
                new SomeEntity { Id = 1, Name = "Kool" },
                new ValueBuffer(new object[] { 1, "Kool" }));

            Assert.Equal(1, entry[keyProperty]);
        }

        [Fact]
        public void Can_set_property_value_after_creation_from_value_buffer()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.GetProperty("Id");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(
                configuration,
                entityType,
                new SomeEntity { Id = 1, Name = "Kool" },
                new ValueBuffer(new object[] { 1, "Kool" }));

            entry[keyProperty] = 77;

            Assert.Equal(77, entry[keyProperty]);
        }
        [Fact]
        public void Can_set_and_get_property_values()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.GetProperty("Id");
            var nonKeyProperty = entityType.GetProperty("Name");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeEntity());

            entry[keyProperty] = 77;
            entry[nonKeyProperty] = "Magic Tree House";

            Assert.Equal(77, entry[keyProperty]);
            Assert.Equal("Magic Tree House", entry[nonKeyProperty]);
        }

        [Fact]
        public void Can_get_value_buffer_from_properties()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.GetProperty("Id");
            var nonKeyProperty = entityType.GetProperty("Name");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeEntity());

            entry[keyProperty] = 77;
            entry[nonKeyProperty] = "Magic Tree House";

            Assert.Equal(new object[] { 77, "Magic Tree House" }, entry.GetValueBuffer());
        }

        [Fact]
        public void All_original_values_can_be_accessed_for_entity_that_does_full_change_tracking_if_eager_values_on()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(FullNotificationEntity).FullName);
            entityType.UseEagerSnapshots = true;

            AllOriginalValuesTest(model, entityType, new FullNotificationEntity { Id = 1, Name = "Kool" });
        }

        protected void AllOriginalValuesTest(IModel model, IEntityType entityType, object entity)
        {
            var idProperty = entityType.GetProperty("Id");
            var nameProperty = entityType.GetProperty("Name");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(
                configuration, 
                entityType, 
                entity, 
                new ValueBuffer(new object[] { 1, "Kool" }));

            entry.SetEntityState(EntityState.Unchanged);

            Assert.Equal(1, entry.OriginalValues[idProperty]);
            Assert.Equal("Kool", entry.OriginalValues[nameProperty]);
            Assert.Equal(1, entry[idProperty]);
            Assert.Equal("Kool", entry[nameProperty]);

            entry[nameProperty] = "Beans";

            Assert.Equal(1, entry.OriginalValues[idProperty]);
            Assert.Equal("Kool", entry.OriginalValues[nameProperty]);
            Assert.Equal(1, entry[idProperty]);
            Assert.Equal("Beans", entry[nameProperty]);

            entry.OriginalValues[idProperty] = 3;
            entry.OriginalValues[nameProperty] = "Franks";

            Assert.Equal(3, entry.OriginalValues[idProperty]);
            Assert.Equal("Franks", entry.OriginalValues[nameProperty]);
            Assert.Equal(1, entry[idProperty]);
            Assert.Equal("Beans", entry[nameProperty]);
        }

        [Fact]
        public void Required_original_values_can_be_accessed_for_entity_that_does_full_change_tracking()
        {
            var model = BuildModel();
            OriginalValuesTest(model, model.GetEntityType(typeof(FullNotificationEntity).FullName), new FullNotificationEntity { Id = 1, Name = "Kool" });
        }

        [Fact]
        public void Required_original_values_can_be_accessed_for_entity_that_does_changed_only_notification()
        {
            var model = BuildModel();
            OriginalValuesTest(model, model.GetEntityType(typeof(ChangedOnlyEntity).FullName), new ChangedOnlyEntity { Id = 1, Name = "Kool" });
        }

        [Fact]
        public void Required_original_values_can_be_accessed_for_entity_that_does_no_notification()
        {
            var model = BuildModel();
            OriginalValuesTest(model, model.GetEntityType(typeof(SomeEntity).FullName), new SomeEntity { Id = 1, Name = "Kool" });
        }

        protected void OriginalValuesTest(IModel model, IEntityType entityType, object entity)
        {
            var nameProperty = entityType.GetProperty("Name");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, entity, new ValueBuffer(new object[] { 1, "Kool" }));
            entry.SetEntityState(EntityState.Unchanged);

            Assert.Equal("Kool", entry.OriginalValues[nameProperty]);
            Assert.Equal("Kool", entry[nameProperty]);

            entry[nameProperty] = "Beans";

            Assert.Equal("Kool", entry.OriginalValues[nameProperty]);
            Assert.Equal("Beans", entry[nameProperty]);

            entry.OriginalValues[nameProperty] = "Franks";

            Assert.Equal("Franks", entry.OriginalValues[nameProperty]);
            Assert.Equal("Beans", entry[nameProperty]);
        }

        [Fact]
        public void Null_original_values_are_handled_for_entity_that_does_full_change_tracking()
        {
            var model = BuildModel();
            NullOriginalValuesTest(model, model.GetEntityType(typeof(FullNotificationEntity).FullName), new FullNotificationEntity { Id = 1 });
        }

        [Fact]
        public void Null_original_values_are_handled_for_entity_that_does_changed_only_notification()
        {
            var model = BuildModel();
            NullOriginalValuesTest(model, model.GetEntityType(typeof(ChangedOnlyEntity).FullName), new ChangedOnlyEntity { Id = 1 });
        }

        [Fact]
        public void Null_original_values_are_handled_for_entity_that_does_no_notification()
        {
            var model = BuildModel();
            NullOriginalValuesTest(model, model.GetEntityType(typeof(SomeEntity).FullName), new SomeEntity { Id = 1 });
        }

        protected void NullOriginalValuesTest(IModel model, IEntityType entityType, object entity)
        {
            var nameProperty = entityType.GetProperty("Name");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, entity, new ValueBuffer(new object[] { 1, null }));
            entry.SetEntityState(EntityState.Unchanged);

            Assert.Null(entry.OriginalValues[nameProperty]);
            Assert.Null(entry[nameProperty]);

            entry[nameProperty] = "Beans";

            Assert.Null(entry.OriginalValues[nameProperty]);
            Assert.Equal("Beans", entry[nameProperty]);

            entry.OriginalValues[nameProperty] = "Franks";

            Assert.Equal("Franks", entry.OriginalValues[nameProperty]);
            Assert.Equal("Beans", entry[nameProperty]);

            entry.OriginalValues[nameProperty] = null;

            Assert.Null(entry.OriginalValues[nameProperty]);
            Assert.Equal("Beans", entry[nameProperty]);
        }

        [Fact]
        public void Setting_property_using_state_entry_always_marks_as_modified()
        {
            var model = BuildModel();

            SetPropertyInternalEntityEntryTest(model, model.GetEntityType(typeof(FullNotificationEntity).FullName), new FullNotificationEntity { Id = 1, Name = "Kool" });
            SetPropertyInternalEntityEntryTest(model, model.GetEntityType(typeof(ChangedOnlyEntity).FullName), new ChangedOnlyEntity { Id = 1, Name = "Kool" });
            SetPropertyInternalEntityEntryTest(model, model.GetEntityType(typeof(SomeEntity).FullName), new SomeEntity { Id = 1, Name = "Kool" });
        }

        protected void SetPropertyInternalEntityEntryTest(IModel model, IEntityType entityType, object entity)
        {
            var idProperty = entityType.GetProperty("Id");
            var nameProperty = entityType.GetProperty("Name");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, entity, new ValueBuffer(new object[] { 1, "Kool" }));
            entry.SetEntityState(EntityState.Unchanged);

            Assert.False(entry.IsPropertyModified(idProperty));
            Assert.False(entry.IsPropertyModified(nameProperty));
            Assert.Equal(EntityState.Unchanged, entry.EntityState);

            entry[idProperty] = 1;
            entry[nameProperty] = "Kool";

            Assert.False(entry.IsPropertyModified(idProperty));
            Assert.False(entry.IsPropertyModified(nameProperty));
            Assert.Equal(EntityState.Unchanged, entry.EntityState);

            entry[nameProperty] = "Beans";

            Assert.False(entry.IsPropertyModified(idProperty));
            Assert.True(entry.IsPropertyModified(nameProperty));
            Assert.Equal(EntityState.Modified, entry.EntityState);
        }

        protected void SetPropertyClrTest<TEntity>(TEntity entity, bool needsDetectChanges)
            where TEntity : ISomeEntity
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(TEntity));
            var nameProperty = entityType.GetProperty("Name");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, entity, new ValueBuffer(new object[] { 1, "Kool" }));
            entry.SetEntityState(EntityState.Unchanged);

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

                configuration.GetRequiredService<IChangeDetector>().DetectChanges(entry);
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
            AcceptChangesNoop(EntityState.Detached);
        }

        private void AcceptChangesNoop(EntityState entityState)
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(
                configuration, 
                entityType, 
                new SomeEntity { Id = 1, Name = "Kool" }, 
                new ValueBuffer(new object[] { 1, "Kool" }));

            entry.SetEntityState(entityState);

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
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);
            var nameProperty = entityType.GetProperty("Name");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(
                configuration, 
                entityType, 
                new SomeEntity { Id = 1, Name = "Kool" }, 
                new ValueBuffer(new object[] { 1, "Kool" }));

            entry.SetEntityState(entityState);

            entry[nameProperty] = "Pickle";
            entry.OriginalValues[nameProperty] = "Cheese";

            entry.AcceptChanges();

            Assert.Equal(EntityState.Unchanged, entry.EntityState);
            Assert.Equal("Pickle", entry[nameProperty]);
            Assert.Equal("Pickle", entry.OriginalValues[nameProperty]);
        }

        [Fact]
        public void AcceptChanges_makes_Modified_entities_Unchanged_and_effectively_resets_unused_original_values()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);
            var nameProperty = entityType.GetProperty("Name");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(
                configuration, 
                entityType, 
                new SomeEntity { Id = 1, Name = "Kool" }, 
                new ValueBuffer(new object[] { 1, "Kool" }));

            entry.SetEntityState(EntityState.Modified);

            entry[nameProperty] = "Pickle";

            entry.AcceptChanges();

            Assert.Equal(EntityState.Unchanged, entry.EntityState);
            Assert.Equal("Pickle", entry[nameProperty]);
            Assert.Equal("Pickle", entry.OriginalValues[nameProperty]);
        }

        [Fact]
        public void AcceptChanges_detaches_Deleted_entities()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(
                configuration, 
                entityType, 
                new SomeEntity { Id = 1, Name = "Kool" }, 
                new ValueBuffer(new object[] { 1, "Kool" }));

            entry.SetEntityState(EntityState.Deleted);

            entry.AcceptChanges();

            Assert.Equal(EntityState.Detached, entry.EntityState);
        }

        [Fact]
        public void Can_add_and_remove_sidecars()
        {
            var model = BuildModel();
            var entry = CreateInternalEntry(
                TestHelpers.Instance.CreateContextServices(model),
                model.GetEntityType(typeof(SomeEntity).FullName),
                new SomeEntity { Id = 1, Name = "Kool" }, 
                new ValueBuffer(new object[] { 1, "Kool" }));

            var sidecarMock1 = new Mock<Sidecar>(entry);
            sidecarMock1.Setup(m => m.Name).Returns("IMZ-Ural");
            sidecarMock1.Setup(m => m.AutoCommit).Returns(true);

            var sidecarMock2 = new Mock<Sidecar>(entry);
            sidecarMock2.Setup(m => m.Name).Returns("GG Duetto");

            var originalValues = entry.TryGetSidecar(Sidecar.WellKnownNames.OriginalValues);

            Assert.True(entry.EntityType.HasClrType() == (originalValues != null));
            Assert.Null(entry.TryGetSidecar("IMZ-Ural"));
            Assert.Null(entry.TryGetSidecar("GG Duetto"));

            entry.AddSidecar(sidecarMock1.Object);
            entry.AddSidecar(sidecarMock2.Object);

            Assert.Same(originalValues, entry.TryGetSidecar(Sidecar.WellKnownNames.OriginalValues));
            Assert.Same(sidecarMock1.Object, entry.TryGetSidecar("IMZ-Ural"));
            Assert.Same(sidecarMock2.Object, entry.TryGetSidecar("GG Duetto"));

            entry.RemoveSidecar("IMZ-Ural");
            entry.RemoveSidecar("GG Duetto");

            Assert.Same(originalValues, entry.TryGetSidecar(Sidecar.WellKnownNames.OriginalValues));
            Assert.Null(entry.TryGetSidecar("IMZ-Ural"));
            Assert.Null(entry.TryGetSidecar("GG Duetto"));

            entry.RemoveSidecar("IMZ-Ural");
            entry.RemoveSidecar("GG Duetto");

            Assert.Same(originalValues, entry.TryGetSidecar(Sidecar.WellKnownNames.OriginalValues));
            Assert.Null(entry.TryGetSidecar("IMZ-Ural"));
            Assert.Null(entry.TryGetSidecar("GG Duetto"));

            entry.RemoveSidecar(Sidecar.WellKnownNames.OriginalValues);

            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.OriginalValues));
            Assert.Null(entry.TryGetSidecar("IMZ-Ural"));
            Assert.Null(entry.TryGetSidecar("GG Duetto"));

            entry.RemoveSidecar("IMZ-Ural");
            entry.RemoveSidecar("GG Duetto");

            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.OriginalValues));
            Assert.Null(entry.TryGetSidecar("IMZ-Ural"));
            Assert.Null(entry.TryGetSidecar("GG Duetto"));

            entry.AddSidecar(sidecarMock1.Object);

            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.OriginalValues));
            Assert.Same(sidecarMock1.Object, entry.TryGetSidecar("IMZ-Ural"));
            Assert.Null(entry.TryGetSidecar("GG Duetto"));
        }

        [Fact]
        public void Non_transparent_sidecar_does_not_intercept_normal_property_read_and_write()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);
            var idProperty = entityType.GetProperty("Id");
            var nameProperty = entityType.GetProperty("Name");

            var entry = CreateInternalEntry(
                TestHelpers.Instance.CreateContextServices(model),
                entityType, 
                new SomeEntity { Id = 1, Name = "Kool" }, 
                new ValueBuffer(new object[] { 1, "Kool" }));

            var sidecar = entry.AddSidecar(new TheWasp(entry, new[] { idProperty }));

            Assert.Equal(1, entry[idProperty]);
            Assert.Equal("Kool", entry[nameProperty]);

            sidecar[idProperty] = 7;

            Assert.Equal(1, entry[idProperty]);
            Assert.Equal("Kool", entry[nameProperty]);

            entry[idProperty] = 77;

            Assert.Equal(77, entry[idProperty]);
            Assert.Equal("Kool", entry[nameProperty]);
        }

        [Fact]
        public void Can_read_values_from_sidecar_transparently()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);
            var idProperty = entityType.GetProperty("Id");
            var nameProperty = entityType.GetProperty("Name");

            var entry = CreateInternalEntry(
                TestHelpers.Instance.CreateContextServices(model),
                entityType, 
                new SomeEntity { Id = 1, Name = "Kool" }, 
                new ValueBuffer(new object[] { 1, "Kool" }));

            var sidecar = entry.AddSidecar(new TheWasp(entry, new[] { idProperty }, transparentRead: true));

            Assert.Equal(1, entry[idProperty]);
            Assert.Equal(1, sidecar[idProperty]);
            Assert.Equal("Kool", entry[nameProperty]);

            sidecar[idProperty] = 7;

            Assert.Equal(7, entry[idProperty]);
            Assert.Equal(7, sidecar[idProperty]);
            Assert.Equal("Kool", entry[nameProperty]);

            entry[idProperty] = 77;

            Assert.Equal(7, entry[idProperty]);
            Assert.Equal(7, sidecar[idProperty]);
            Assert.Equal("Kool", entry[nameProperty]);

            entry.RemoveSidecar(sidecar.Name);

            Assert.Equal(77, entry[idProperty]);
            Assert.Equal("Kool", entry[nameProperty]);
        }

        [Fact]
        public void Can_write_values_to_sidecar_transparently()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);
            var idProperty = entityType.GetProperty("Id");
            var nameProperty = entityType.GetProperty("Name");

            var entry = CreateInternalEntry(
                TestHelpers.Instance.CreateContextServices(model),
                entityType, 
                new SomeEntity { Id = 1, Name = "Kool" }, 
                new ValueBuffer(new object[] { 1, "Kool" }));

            var sidecar = entry.AddSidecar(new TheWasp(entry, new[] { idProperty }, transparentWrite: true));

            Assert.Equal(1, entry[idProperty]);
            Assert.Equal("Kool", entry[nameProperty]);

            entry[idProperty] = 7;

            Assert.Equal(1, entry[idProperty]);
            Assert.Equal(7, sidecar[idProperty]);
            Assert.Equal("Kool", entry[nameProperty]);

            sidecar[idProperty] = 77;

            Assert.Equal(1, entry[idProperty]);
            Assert.Equal(77, sidecar[idProperty]);
            Assert.Equal("Kool", entry[nameProperty]);

            entry.RemoveSidecar(sidecar.Name);

            Assert.Equal(1, entry[idProperty]);
            Assert.Equal("Kool", entry[nameProperty]);
        }

        [Fact]
        public void Can_auto_commit_sidecars()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);
            var idProperty = entityType.GetProperty("Id");
            var nameProperty = entityType.GetProperty("Name");

            var entry = CreateInternalEntry(
                TestHelpers.Instance.CreateContextServices(model),
                entityType,
                new SomeEntity { Id = 1, Name = "Kool" },
                new ValueBuffer(new object[] { 1, "Kool" }));

            var sidecar = entry.AddSidecar(new TheWasp(entry, new[] { idProperty }, autoCommit: true));

            sidecar[idProperty] = 77;

            Assert.Equal(1, entry[idProperty]);
            Assert.Equal(77, sidecar[idProperty]);
            Assert.Equal("Kool", entry[nameProperty]);

            entry.AutoCommitSidecars();

            Assert.Equal(77, entry[idProperty]);

            Assert.Null(entry.TryGetSidecar(sidecar.Name));
        }

        [Fact]
        public void Can_auto_rollback_sidecars()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);
            var idProperty = entityType.GetProperty("Id");
            var nameProperty = entityType.GetProperty("Name");

            var entry = CreateInternalEntry(
                TestHelpers.Instance.CreateContextServices(model),
                entityType,
                new SomeEntity { Id = 1, Name = "Kool" },
                new ValueBuffer(new object[] { 1, "Kool" }));

            var sidecar = entry.AddSidecar(new TheWasp(entry, new[] { idProperty }, autoCommit: true));

            sidecar[idProperty] = 77;

            Assert.Equal(1, entry[idProperty]);
            Assert.Equal(77, sidecar[idProperty]);
            Assert.Equal("Kool", entry[nameProperty]);

            entry.AutoRollbackSidecars();

            Assert.Equal(1, entry[idProperty]);

            Assert.Null(entry.TryGetSidecar(sidecar.Name));
        }

        private class TheWasp : DictionarySidecar
        {
            public TheWasp(
                InternalEntityEntry entry, IEnumerable<IProperty> properties,
                bool transparentRead = false, bool transparentWrite = false, bool autoCommit = false)
                : base(entry, properties)
            {
                TransparentRead = transparentRead;
                TransparentWrite = transparentWrite;
                AutoCommit = autoCommit;
            }

            public override string Name
            {
                get { return "Wasp Motorcycles"; }
            }

            public override bool TransparentRead { get; }

            public override bool TransparentWrite { get; }

            public override bool AutoCommit { get; }
        }

        [Fact]
        public void Sidecars_are_added_for_store_generated_values_when_preparing_to_save()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);

            var customServices = new ServiceCollection()
                .AddScoped<IValueGeneratorSelector, TestInMemoryValueGeneratorSelector>();

            var entry = CreateInternalEntry(
                TestHelpers.Instance.CreateContextServices(customServices, model),
                entityType,
                new SomeEntity { Id = 1, Name = "Kool" },
                new ValueBuffer(new object[] { 1, "Kool" }));

            entry.SetEntityState(EntityState.Added);
            entry.PrepareToSave();

            var storeGenValues = entry.TryGetSidecar(Sidecar.WellKnownNames.StoreGeneratedValues);
            Assert.NotNull(storeGenValues);
            Assert.True(storeGenValues.CanStoreValue(entityType.GetProperty("Id")));
            Assert.False(storeGenValues.CanStoreValue(entityType.GetProperty("Name")));
        }

        [Fact]
        public void Sidecars_are_not_added_for_when_no_store_generated_values_will_be_used()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);

            var entry = CreateInternalEntry(
                TestHelpers.Instance.CreateContextServices(model),
                entityType,
                new SomeEntity { Id = 1, Name = "Kool" },
                new ValueBuffer(new object[] { 1, "Kool" }));

            entry.SetEntityState(EntityState.Added);
            entry.PrepareToSave();

            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.StoreGeneratedValues));
        }

        [Fact]
        public void Sidecars_are_added_for_computed_properties_when_preparing_to_save()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);
            entityType.GetProperty("Name").StoreGeneratedPattern = StoreGeneratedPattern.Computed;

            var entry = CreateInternalEntry(
                TestHelpers.Instance.CreateContextServices(model),
                entityType,
                new SomeEntity { Id = 1 },
                new ValueBuffer(new object[] { 1, null }));

            entry.SetEntityState(EntityState.Added);
            entry.PrepareToSave();

            var storeGenValues = entry.TryGetSidecar(Sidecar.WellKnownNames.StoreGeneratedValues);
            Assert.NotNull(storeGenValues);
            Assert.False(storeGenValues.CanStoreValue(entityType.GetProperty("Id")));
            Assert.True(storeGenValues.CanStoreValue(entityType.GetProperty("Name")));
        }

        [Fact]
        public void Sidecars_are_added_for_store_default_properties_when_preparing_to_save()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);
            entityType.GetProperty("Name").StoreGeneratedPattern = StoreGeneratedPattern.Computed;
            entityType.GetProperty("Name").IsReadOnlyAfterSave = false;

            var entry = CreateInternalEntry(
                TestHelpers.Instance.CreateContextServices(model),
                entityType,
                new SomeEntity { Id = 1 },
                new ValueBuffer(new object[] { 1, null }));

            entry.SetEntityState(EntityState.Added);
            entry.PrepareToSave();

            var storeGenValues = entry.TryGetSidecar(Sidecar.WellKnownNames.StoreGeneratedValues);
            Assert.NotNull(storeGenValues);
            Assert.False(storeGenValues.CanStoreValue(entityType.GetProperty("Id")));
            Assert.True(storeGenValues.CanStoreValue(entityType.GetProperty("Name")));
        }

        [Fact]
        public void Sidecars_are_added_for_foreign_key_properties_with_root_principals_that_may_get_store_generated_values()
        {
            var model = BuildOneToOneModel();
            var entityType = model.GetEntityType(typeof(SecondDependent));

            var customServices = new ServiceCollection()
                .AddScoped<IValueGeneratorSelector, TestInMemoryValueGeneratorSelector>();

            var entry = CreateInternalEntry(
                TestHelpers.Instance.CreateContextServices(customServices, model),
                entityType,
                new SecondDependent { Id = 1 },
                new ValueBuffer(new object[] { 1 }));

            entry.SetEntityState(EntityState.Added);
            entry.PrepareToSave();

            var storeGenValues = entry.TryGetSidecar(Sidecar.WellKnownNames.StoreGeneratedValues);

            Assert.NotNull(storeGenValues);
            Assert.True(storeGenValues.CanStoreValue(entityType.GetProperty("Id")));
        }

        [Fact]
        public void Sidecars_are_not_added_for_foreign_key_properties_with_root_principals_that_will_not_get_store_generated_values()
        {
            var model = BuildOneToOneModel();
            var entityType = model.GetEntityType(typeof(SecondDependent));

            var entry = CreateInternalEntry(
                TestHelpers.Instance.CreateContextServices(model),
                entityType,
                new SecondDependent { Id = 1 },
                new ValueBuffer(new object[] { 1 }));

            entry.SetEntityState(EntityState.Added);
            entry.PrepareToSave();

            Assert.Null(entry.TryGetSidecar(Sidecar.WellKnownNames.StoreGeneratedValues));
        }

        private static IModel BuildOneToOneModel()
        {
            var modelBuilder = TestHelpers.Instance.CreateConventionBuilder();

            modelBuilder
                .Entity<FirstDependent>()
                .Reference(e => e.Second)
                .InverseReference(e => e.First)
                .ForeignKey<SecondDependent>(e => e.Id);

            modelBuilder
                .Entity<Root>(b =>
                    {
                        b.Property(e => e.Id)
                            .StoreGeneratedPattern(StoreGeneratedPattern.None);

                        b.Reference(e => e.First)
                            .InverseReference(e => e.Root)
                            .ForeignKey<FirstDependent>(e => e.Id);

                    });


            return modelBuilder.Model;
        }

        public class TestInMemoryValueGeneratorSelector : InMemoryValueGeneratorSelector
        {
            private readonly TemporaryNumberValueGeneratorFactory _inMemoryFactory = new TemporaryNumberValueGeneratorFactory();

            public TestInMemoryValueGeneratorSelector(IValueGeneratorCache cache)
                : base(cache)
            {
            }

            public override ValueGenerator Create(IProperty property, IEntityType entityType)
            {
                return property.ClrType == typeof(int)
                    ? _inMemoryFactory.Create(property)
                    : base.Create(property, entityType);
            }
        }

        private class Root
        {
            public int Id { get; set; }

            public FirstDependent First { get; set; }
        }

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

        protected virtual InternalEntityEntry CreateInternalEntry(IServiceProvider contextServices, IEntityType entityType, object entity)
        {
            return contextServices.GetRequiredService<IInternalEntityEntrySubscriber>().SnapshotAndSubscribe(
                new InternalEntityEntryFactory(
                    contextServices.GetRequiredService<IEntityEntryMetadataServices>())
                    .Create(contextServices.GetRequiredService<IStateManager>(), entityType, entity));
        }

        protected virtual InternalEntityEntry CreateInternalEntry(IServiceProvider contextServices, IEntityType entityType, object entity, ValueBuffer valueBuffer)
        {
            return contextServices.GetRequiredService<IInternalEntityEntrySubscriber>().SnapshotAndSubscribe(
                new InternalEntityEntryFactory(
                    contextServices.GetRequiredService<IEntityEntryMetadataServices>())
                    .Create(contextServices.GetRequiredService<IStateManager>(), entityType, entity, valueBuffer));
        }

        protected virtual Model BuildModel()
        {
            var model = new Model();

            var entityType1 = model.AddEntityType(typeof(SomeEntity));
            var key1 = entityType1.GetOrAddProperty("Id", typeof(int));
            key1.IsValueGeneratedOnAdd = true;
            entityType1.GetOrSetPrimaryKey(key1);
            entityType1.GetOrAddProperty("Name", typeof(string)).IsConcurrencyToken = true;

            var entityType2 = model.AddEntityType(typeof(SomeDependentEntity));
            var key2a = entityType2.GetOrAddProperty("Id1", typeof(int));
            var key2b = entityType2.GetOrAddProperty("Id2", typeof(string));
            entityType2.GetOrSetPrimaryKey(new[] { key2a, key2b });
            var fk = entityType2.GetOrAddProperty("SomeEntityId", typeof(int));
            entityType2.GetOrAddForeignKey(new[] { fk }, entityType1.GetPrimaryKey());
            var justAProperty = entityType2.GetOrAddProperty("JustAProperty", typeof(int));
            justAProperty.IsValueGeneratedOnAdd = true;

            var entityType3 = model.AddEntityType(typeof(FullNotificationEntity));
            entityType3.GetOrSetPrimaryKey(entityType3.GetOrAddProperty("Id", typeof(int)));
            entityType3.GetOrAddProperty("Name", typeof(string)).IsConcurrencyToken = true;

            var entityType4 = model.AddEntityType(typeof(ChangedOnlyEntity));
            entityType4.GetOrSetPrimaryKey(entityType4.GetOrAddProperty("Id", typeof(int)));
            entityType4.GetOrAddProperty("Name", typeof(string)).IsConcurrencyToken = true;

            var entityType5 = model.AddEntityType(typeof(SomeMoreDependentEntity));
            var key5 = entityType5.GetOrAddProperty("Id", typeof(int));
            entityType5.GetOrSetPrimaryKey(key5);
            var fk5a = entityType5.GetOrAddProperty("Fk1", typeof(int));
            var fk5b = entityType5.GetOrAddProperty("Fk2", typeof(string));
            entityType5.GetOrAddForeignKey(new[] { fk5a, fk5b }, entityType2.GetPrimaryKey());

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
            public int Id1 { get; set; }
            public string Id2 { get; set; }
            public int SomeEntityId { get; set; }
            public int JustAProperty { get; set; }
        }

        protected class SomeMoreDependentEntity
        {
            public int Id { get; set; }
            public int Fk1 { get; set; }
            public string Fk2 { get; set; }
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
    }
}
