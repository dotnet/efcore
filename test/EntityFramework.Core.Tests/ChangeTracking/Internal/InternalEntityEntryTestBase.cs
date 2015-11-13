// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Update;
using Microsoft.Data.Entity.ValueGeneration;
using Microsoft.Data.Entity.ValueGeneration.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking
{
    public abstract class InternalEntityEntryTestBase
    {
        [Fact]
        public virtual void Changing_state_from_Unknown_causes_entity_to_start_tracking()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.FindProperty("Id");

            var contextServices = TestHelpers.Instance.CreateContextServices(model);

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

            var contextServices = TestHelpers.Instance.CreateContextServices(model);

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

            var contextServices = TestHelpers.Instance.CreateContextServices(model);

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
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeEntity());
            entry[keyProperty] = 1;

            Assert.False(entry.IsModified(keyProperty));
            Assert.False(entry.IsModified(nonKeyProperty));

            entry.SetEntityState(EntityState.Modified);

            Assert.False(entry.IsModified(keyProperty));
            Assert.True(entry.IsModified(nonKeyProperty));

            entry.SetEntityState(EntityState.Unchanged, true);

            Assert.False(entry.IsModified(keyProperty));
            Assert.False(entry.IsModified(nonKeyProperty));

            entry.SetPropertyModified(nonKeyProperty);

            Assert.Equal(EntityState.Modified, entry.EntityState);
            Assert.False(entry.IsModified(keyProperty));
            Assert.True(entry.IsModified(nonKeyProperty));
        }

        [Fact]
        public virtual void Read_only_before_save_properties_throw_if_not_null_or_temp()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.FindProperty("Id");
            var nonKeyProperty = entityType.FindProperty("Name");
            nonKeyProperty.IsReadOnlyBeforeSave = true;
            keyProperty.IsReadOnlyBeforeSave = true;
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeEntity());

            entry.SetEntityState(EntityState.Added);
            entry.MarkAsTemporary(keyProperty);

            entry[nonKeyProperty] = "Jillybean";

            Assert.Equal(
                CoreStrings.PropertyReadOnlyBeforeSave("Name", typeof(SomeEntity).Name),
                Assert.Throws<InvalidOperationException>(() => entry.PrepareToSave()).Message);

            entry[nonKeyProperty] = null;

            entry[keyProperty] = 2;

            Assert.Equal(
                CoreStrings.PropertyReadOnlyBeforeSave("Id", typeof(SomeEntity).Name),
                Assert.Throws<InvalidOperationException>(() => entry.PrepareToSave()).Message);
        }

        [Fact]
        public virtual void Read_only_after_save_properties_throw_if_modified()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var nonKeyProperty = entityType.FindProperty("Name");
            nonKeyProperty.IsReadOnlyAfterSave = true;
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeEntity());

            entry[entityType.FindProperty("Id")] = 1;
            entry[nonKeyProperty] = "Jillybean";

            entry.SetEntityState(EntityState.Modified);

            Assert.False(entry.IsModified(nonKeyProperty));

            entry.SetEntityState(EntityState.Unchanged, true);

            Assert.False(entry.IsModified(nonKeyProperty));

            entry.SetPropertyModified(nonKeyProperty);

            Assert.True(entry.IsModified(nonKeyProperty));

            Assert.Equal(
                CoreStrings.PropertyReadOnlyAfterSave("Name", typeof(SomeEntity).Name),
                Assert.Throws<InvalidOperationException>(() => entry.PrepareToSave()).Message);

            entry.SetPropertyModified(nonKeyProperty, isModified: false);

            entry[nonKeyProperty] = "Beanjilly";

            Assert.True(entry.IsModified(nonKeyProperty));

            Assert.Equal(
                CoreStrings.PropertyReadOnlyAfterSave("Name", typeof(SomeEntity).Name),
                Assert.Throws<InvalidOperationException>(() => entry.PrepareToSave()).Message);
        }

        [Fact]
        public virtual void Key_properties_throw_immediately_if_modified()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.FindProperty("Id");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeEntity());

            entry[keyProperty] = 1;

            entry.SetEntityState(EntityState.Modified);

            Assert.False(entry.IsModified(keyProperty));

            entry.SetEntityState(EntityState.Unchanged, true);

            Assert.False(entry.IsModified(keyProperty));

            Assert.Equal(
                CoreStrings.KeyReadOnly("Id", typeof(SomeEntity).Name),
                Assert.Throws<NotSupportedException>(() => entry.SetPropertyModified(keyProperty)).Message);

            Assert.Equal(EntityState.Unchanged, entry.EntityState);
            Assert.False(entry.IsModified(keyProperty));

            Assert.Equal(
                CoreStrings.KeyReadOnly("Id", typeof(SomeEntity).Name),
                Assert.Throws<NotSupportedException>(() => entry[keyProperty] = 2).Message);

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
            var configuration = TestHelpers.Instance.CreateContextServices(model);

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

            entry.MarkAsTemporary(keyProperty);

            Assert.True(entry.HasTemporaryValue(keyProperty));
            Assert.False(entry.HasTemporaryValue(nonKeyProperty));
            Assert.False(entry.IsModified(keyProperty));
            Assert.False(entry.IsModified(nonKeyProperty));

            entry.MarkAsTemporary(nonKeyProperty);
            entry.MarkAsTemporary(keyProperty, isTemporary: false);

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

            entry.MarkAsTemporary(keyProperty);
            entry.MarkAsTemporary(nonKeyProperty);

            Assert.False(entry.HasTemporaryValue(keyProperty));
            Assert.False(entry.HasTemporaryValue(nonKeyProperty));
            Assert.False(entry.IsModified(keyProperty));
            Assert.False(entry.IsModified(nonKeyProperty));

            entry.SetEntityState(EntityState.Added);

            Assert.False(entry.HasTemporaryValue(keyProperty));
            Assert.False(entry.HasTemporaryValue(nonKeyProperty));
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
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeEntity());

            entry.SetEntityState(EntityState.Added);
            entry.MarkAsTemporary(keyProperty);

            Assert.Equal(
                CoreStrings.TempValuePersists("Id", "SomeEntity", targetState.ToString()),
                Assert.Throws<InvalidOperationException>(() => entry.SetEntityState(targetState)).Message);
        }

        [Fact]
        public virtual void Detaching_with_temp_values_does_not_throw()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.FindProperty("Id");
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
        public virtual void Setting_an_explicit_value_marks_property_as_not_temporary()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.FindProperty("Id");
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
        public virtual void Key_properties_share_value_generation_space_with_base()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var keyProperty = (IProperty)entityType.FindProperty("Id");
            var baseEntityType = model.FindEntityType(typeof(SomeSimpleEntityBase).FullName);
            var nonKeyProperty = baseEntityType.AddProperty("NonId", typeof(int));
            nonKeyProperty.RequiresValueGenerator = true;
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

            Assert.Null(entry[nonKeyProperty]);

            entry.SetEntityState(EntityState.Added);

            Assert.NotNull(entry[keyProperty]);
            Assert.NotEqual(0, entry[keyProperty]);
            Assert.Equal(entry[keyProperty], entry[nonKeyProperty]);

            var baseEntry = CreateInternalEntry(configuration, baseEntityType, new SomeSimpleEntityBase());

            baseEntry.SetEntityState(EntityState.Added);

            Assert.NotNull(baseEntry[keyProperty]);
            Assert.NotEqual(0, baseEntry[keyProperty]);
            Assert.NotEqual(entry[keyProperty], baseEntry[keyProperty]);
            Assert.Equal(entry[nonKeyProperty], baseEntry[nonKeyProperty]);
        }

        [Fact]
        public virtual void Value_generation_does_not_happen_if_property_has_non_default_value()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.FindProperty("Id");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeEntity());

            entry[keyProperty] = 31143;

            entry.SetEntityState(EntityState.Added);

            Assert.Equal(31143, entry[keyProperty]);
        }

        [Fact]
        public virtual void Temporary_values_are_reset_when_entity_is_detached()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.FindProperty("Id");
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
        public virtual void Modified_values_are_reset_when_entity_is_changed_to_Added()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var property = entityType.FindProperty("Name");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

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
            var property = (IProperty)entityType.FindProperty("JustAProperty");
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
        public virtual void Can_create_primary_key()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.FindProperty("Id");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeEntity());
            entry[keyProperty] = 77;

            var keyValue = entry.GetPrimaryKeyValue();
            Assert.IsType<KeyValue<int>>(keyValue);
            Assert.Equal(77, keyValue.Value);
            Assert.Same(entityType.FindPrimaryKey(), keyValue.Key);
        }

        [Fact]
        public virtual void Can_create_composite_primary_key()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeDependentEntity).FullName);
            var keyProperties = new[] { entityType.FindProperty("Id1"), entityType.FindProperty("Id2") };
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeDependentEntity());
            entry[keyProperties[0]] = 77;
            entry[keyProperties[1]] = "SmokeyBacon";

            var entityKey = (KeyValue<object[]>)entry.GetPrimaryKeyValue();
            var keyValue = (object[])entityKey.Value;

            Assert.Equal(77, keyValue[0]);
            Assert.Equal("SmokeyBacon", keyValue[1]);
            Assert.Same(entityType.FindPrimaryKey(), entityKey.Key);
        }

        [Fact]
        public virtual void Can_create_foreign_key_value_based_on_dependent_values()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeDependentEntity).FullName);
            var fk = entityType.GetForeignKeys().Single();
            var fkProperty = fk.Properties.Single();
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeDependentEntity());
            entry[fkProperty] = 77;
            entry.SetValue(fkProperty, 78, ValueSource.RelationshipSnapshot);

            var keyValue = entry.GetDependentKeyValue(fk);
            Assert.IsType<KeyValue<int>>(keyValue);
            Assert.Equal(77, keyValue.Value);
            Assert.Same(fk.PrincipalEntityType.FindPrimaryKey(), keyValue.Key);
        }

        [Fact]
        public virtual void Can_create_foreign_key_value_based_on_snapshot_dependent_values()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeDependentEntity).FullName);
            var fk = entityType.GetForeignKeys().Single();
            var fkProperty = fk.Properties.Single();
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeDependentEntity());
            entry[fkProperty] = 77;
            entry.SetValue(fkProperty, 78, ValueSource.RelationshipSnapshot);

            var keyValue = entry.GetDependentKeyValue(fk, ValueSource.RelationshipSnapshot);
            Assert.IsType<KeyValue<int>>(keyValue);
            Assert.Equal(78, keyValue.Value);
            Assert.Same(fk.PrincipalEntityType.FindPrimaryKey(), keyValue.Key);
        }

        [Fact]
        public virtual void Can_create_foreign_key_value_based_on_snapshot_dependent_values_if_value_not_yet_snapshotted()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeDependentEntity).FullName);
            var fk = entityType.GetForeignKeys().Single();
            var fkProperty = fk.Properties.Single();
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeDependentEntity());
            entry[fkProperty] = 77;

            var keyValue = entry.GetDependentKeyValue(fk, ValueSource.RelationshipSnapshot);
            Assert.IsType<KeyValue<int>>(keyValue);
            Assert.Equal(77, keyValue.Value);
            Assert.Same(fk.PrincipalEntityType.FindPrimaryKey(), keyValue.Key);
        }

        [Fact]
        public virtual void Notification_that_an_FK_property_has_changed_updates_the_snapshot()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeDependentEntity).FullName);
            var fkProperty = entityType.FindProperty("SomeEntityId");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeDependentEntity());
            entry[fkProperty] = 77;
            entry.SetValue(fkProperty, 78, ValueSource.RelationshipSnapshot);

            entry[fkProperty] = 79;

            var keyValue = entry.GetDependentKeyValue(entityType.GetForeignKeys().Single(), ValueSource.RelationshipSnapshot);
            Assert.IsType<KeyValue<int>>(keyValue);
            Assert.Equal(79, keyValue.Value);
        }

        [Fact]
        public virtual void Setting_property_to_the_same_value_does_not_update_the_snapshot()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeDependentEntity).FullName);
            var fkProperty = entityType.FindProperty("SomeEntityId");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeDependentEntity());
            entry[fkProperty] = 77;
            entry.SetValue(fkProperty, 78, ValueSource.RelationshipSnapshot);

            entry[fkProperty] = 77;

            var keyValue = entry.GetDependentKeyValue(entityType.GetForeignKeys().Single(), ValueSource.RelationshipSnapshot);
            Assert.IsType<KeyValue<int>>(keyValue);
            Assert.Equal(78, keyValue.Value);
        }

        [Fact]
        public virtual void Can_create_foreign_key_value_based_on_principal_end_values()
        {
            var model = BuildModel();
            var principalType = model.FindEntityType(typeof(SomeEntity).FullName);
            var dependentType = model.FindEntityType(typeof(SomeDependentEntity).FullName);
            var key = principalType.FindProperty("Id");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, principalType, new SomeEntity());
            entry[key] = 77;

            var fk = dependentType.GetForeignKeys().Single();
            var keyValue = entry.GetPrincipalKeyValue(fk);
            Assert.IsType<KeyValue<int>>(keyValue);
            Assert.Equal(77, keyValue.Value);
            Assert.Same(fk.PrincipalEntityType.FindPrimaryKey(), keyValue.Key);
        }

        [Fact]
        public virtual void Can_create_composite_foreign_key_value_based_on_dependent_values()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeMoreDependentEntity).FullName);
            var fk = entityType.GetForeignKeys().Single();
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeMoreDependentEntity());
            entry[fk.Properties[0]] = 77;
            entry[fk.Properties[1]] = "CheeseAndOnion";

            var entityKey = (KeyValue<object[]>)entry.GetDependentKeyValue(fk);
            var keyValue = (object[])entityKey.Value;

            Assert.Equal(77, keyValue[0]);
            Assert.Equal("CheeseAndOnion", keyValue[1]);
            Assert.Same(fk.PrincipalEntityType.FindPrimaryKey(), entityKey.Key);
        }

        [Fact]
        public virtual void Can_create_composite_foreign_key_value_based_on_principal_end_values()
        {
            var model = BuildModel();
            var principalType = model.FindEntityType(typeof(SomeDependentEntity).FullName);
            var dependentType = model.FindEntityType(typeof(SomeMoreDependentEntity).FullName);
            var keyProperties = new[] { principalType.FindProperty("Id1"), principalType.FindProperty("Id2") };
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, principalType, new SomeDependentEntity());
            entry[keyProperties[0]] = 77;
            entry[keyProperties[1]] = "PrawnCocktail";

            var entityKey = (KeyValue<object[]>)entry.GetPrincipalKeyValue(dependentType.GetForeignKeys().Single());
            var keyValue = (object[])entityKey.Value;

            Assert.Equal(77, keyValue[0]);
            Assert.Equal("PrawnCocktail", keyValue[1]);
            Assert.Same(principalType.FindPrimaryKey(), entityKey.Key);
        }

        [Fact]
        public virtual void Can_create_composite_foreign_key_value_based_on_principal_end_values_with_nulls()
        {
            var model = BuildModel();
            var principalType = model.FindEntityType(typeof(SomeDependentEntity).FullName);
            var dependentType = model.FindEntityType(typeof(SomeMoreDependentEntity).FullName);
            var keyProperties = new[] { principalType.FindProperty("Id1"), principalType.FindProperty("Id2") };
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, principalType, new SomeDependentEntity());
            entry[keyProperties[0]] = 77;
            entry[keyProperties[1]] = null;

            var fk = dependentType.GetForeignKeys().Single();
            var keyValue = entry.GetPrincipalKeyValue(fk);
            Assert.True(keyValue.IsInvalid);
            Assert.Null(keyValue.Key);
        }

        [Fact]
        public virtual void Can_get_property_value_after_creation_from_value_buffer()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.FindProperty("Id");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(
                configuration,
                entityType,
                new SomeEntity { Id = 1, Name = "Kool" },
                new ValueBuffer(new object[] { 1, "Kool" }));

            Assert.Equal(1, entry[keyProperty]);
        }

        [Fact]
        public virtual void Can_set_property_value_after_creation_from_value_buffer()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.FindProperty("Id");
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
        public virtual void Can_set_and_get_property_values()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.FindProperty("Id");
            var nonKeyProperty = entityType.FindProperty("Name");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeEntity());

            entry[keyProperty] = 77;
            entry[nonKeyProperty] = "Magic Tree House";

            Assert.Equal(77, entry[keyProperty]);
            Assert.Equal("Magic Tree House", entry[nonKeyProperty]);
        }

        [Fact]
        public virtual void Can_get_value_buffer_from_properties()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.FindProperty("Id");
            var nonKeyProperty = entityType.FindProperty("Name");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeEntity());

            entry[keyProperty] = 77;
            entry[nonKeyProperty] = "Magic Tree House";

            Assert.Equal(new object[] { 77, "Magic Tree House" }, CreateValueBuffer(entry));
        }

        private static object[] CreateValueBuffer(IUpdateEntry entry)
            => entry.EntityType.GetProperties().Select(p => entry.GetValue(p)).ToArray();

        [Fact]
        public virtual void All_original_values_can_be_accessed_for_entity_that_does_full_change_tracking_if_eager_values_on()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(FullNotificationEntity).FullName);
            entityType.UseEagerSnapshots = true;

            AllOriginalValuesTest(model, entityType, new FullNotificationEntity { Id = 1, Name = "Kool" });
        }

        protected void AllOriginalValuesTest(IModel model, IEntityType entityType, object entity)
        {
            var idProperty = entityType.FindProperty("Id");
            var nameProperty = entityType.FindProperty("Name");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(
                configuration,
                entityType,
                entity,
                new ValueBuffer(new object[] { 1, "Kool" }));

            entry.SetEntityState(EntityState.Unchanged);

            Assert.Equal(1, entry.GetValue(idProperty, ValueSource.Original));
            Assert.Equal("Kool", entry.GetValue(nameProperty, ValueSource.Original));
            Assert.Equal(1, entry[idProperty]);
            Assert.Equal("Kool", entry[nameProperty]);

            entry[nameProperty] = "Beans";

            Assert.Equal(1, entry.GetValue(idProperty, ValueSource.Original));
            Assert.Equal("Kool", entry.GetValue(nameProperty, ValueSource.Original));
            Assert.Equal(1, entry[idProperty]);
            Assert.Equal("Beans", entry[nameProperty]);

            entry.SetValue(idProperty, 3, ValueSource.Original);
            entry.SetValue(nameProperty, "Franks", ValueSource.Original);

            Assert.Equal(3, entry.GetValue(idProperty, ValueSource.Original));
            Assert.Equal("Franks", entry.GetValue(nameProperty, ValueSource.Original));
            Assert.Equal(1, entry[idProperty]);
            Assert.Equal("Beans", entry[nameProperty]);
        }

        [Fact]
        public virtual void Required_original_values_can_be_accessed_for_entity_that_does_full_change_tracking()
        {
            var model = BuildModel();
            OriginalValuesTest(model, model.FindEntityType(typeof(FullNotificationEntity).FullName), new FullNotificationEntity { Id = 1, Name = "Kool" });
        }

        [Fact]
        public virtual void Required_original_values_can_be_accessed_for_entity_that_does_changed_only_notification()
        {
            var model = BuildModel();
            OriginalValuesTest(model, model.FindEntityType(typeof(ChangedOnlyEntity).FullName), new ChangedOnlyEntity { Id = 1, Name = "Kool" });
        }

        [Fact]
        public virtual void Required_original_values_can_be_accessed_for_entity_that_does_no_notification()
        {
            var model = BuildModel();
            OriginalValuesTest(model, model.FindEntityType(typeof(SomeEntity).FullName), new SomeEntity { Id = 1, Name = "Kool" });
        }

        protected void OriginalValuesTest(IModel model, IEntityType entityType, object entity)
        {
            var nameProperty = entityType.FindProperty("Name");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, entity, new ValueBuffer(new object[] { 1, "Kool" }));
            entry.SetEntityState(EntityState.Unchanged);

            Assert.Equal("Kool", entry.GetValue(nameProperty, ValueSource.Original));
            Assert.Equal("Kool", entry[nameProperty]);

            entry[nameProperty] = "Beans";

            Assert.Equal("Kool", entry.GetValue(nameProperty, ValueSource.Original));
            Assert.Equal("Beans", entry[nameProperty]);

            entry.SetValue(nameProperty, "Franks", ValueSource.Original);

            Assert.Equal("Franks", entry.GetValue(nameProperty, ValueSource.Original));
            Assert.Equal("Beans", entry[nameProperty]);
        }

        [Fact]
        public virtual void Null_original_values_are_handled_for_entity_that_does_full_change_tracking()
        {
            var model = BuildModel();
            NullOriginalValuesTest(model, model.FindEntityType(typeof(FullNotificationEntity).FullName), new FullNotificationEntity { Id = 1 });
        }

        [Fact]
        public virtual void Null_original_values_are_handled_for_entity_that_does_changed_only_notification()
        {
            var model = BuildModel();
            NullOriginalValuesTest(model, model.FindEntityType(typeof(ChangedOnlyEntity).FullName), new ChangedOnlyEntity { Id = 1 });
        }

        [Fact]
        public virtual void Null_original_values_are_handled_for_entity_that_does_no_notification()
        {
            var model = BuildModel();
            NullOriginalValuesTest(model, model.FindEntityType(typeof(SomeEntity).FullName), new SomeEntity { Id = 1 });
        }

        protected void NullOriginalValuesTest(IModel model, IEntityType entityType, object entity)
        {
            var nameProperty = entityType.FindProperty("Name");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, entity, new ValueBuffer(new object[] { 1, null }));
            entry.SetEntityState(EntityState.Unchanged);

            Assert.Null(entry.GetValue(nameProperty, ValueSource.Original));
            Assert.Null(entry[nameProperty]);

            entry[nameProperty] = "Beans";

            Assert.Null(entry.GetValue(nameProperty, ValueSource.Original));
            Assert.Equal("Beans", entry[nameProperty]);

            entry.SetValue(nameProperty, "Franks", ValueSource.Original);

            Assert.Equal("Franks", entry.GetValue(nameProperty, ValueSource.Original));
            Assert.Equal("Beans", entry[nameProperty]);

            entry.SetValue(nameProperty, null, ValueSource.Original);

            Assert.Null(entry.GetValue(nameProperty, ValueSource.Original));
            Assert.Equal("Beans", entry[nameProperty]);
        }

        [Fact]
        public virtual void Setting_property_using_state_entry_always_marks_as_modified()
        {
            var model = BuildModel();

            SetPropertyInternalEntityEntryTest(model, model.FindEntityType(typeof(FullNotificationEntity).FullName), new FullNotificationEntity { Id = 1, Name = "Kool" });
            SetPropertyInternalEntityEntryTest(model, model.FindEntityType(typeof(ChangedOnlyEntity).FullName), new ChangedOnlyEntity { Id = 1, Name = "Kool" });
            SetPropertyInternalEntityEntryTest(model, model.FindEntityType(typeof(SomeEntity).FullName), new SomeEntity { Id = 1, Name = "Kool" });
        }

        protected void SetPropertyInternalEntityEntryTest(IModel model, IEntityType entityType, object entity)
        {
            var idProperty = entityType.FindProperty("Id");
            var nameProperty = entityType.FindProperty("Name");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

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
            var configuration = TestHelpers.Instance.CreateContextServices(model);

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
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(
                configuration,
                entityType,
                new SomeEntity { Id = 1, Name = "Kool" },
                new ValueBuffer(new object[] { 1, "Kool" }));

            entry.SetEntityState(entityState);

            entry[nameProperty] = "Pickle";
            entry.SetValue(nameProperty, "Cheese", ValueSource.Original);

            entry.AcceptChanges();

            Assert.Equal(EntityState.Unchanged, entry.EntityState);
            Assert.Equal("Pickle", entry[nameProperty]);
            Assert.Equal("Pickle", entry.GetValue(nameProperty, ValueSource.Original));
        }

        [Fact]
        public virtual void AcceptChanges_makes_Modified_entities_Unchanged_and_effectively_resets_unused_original_values()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var nameProperty = entityType.FindProperty("Name");
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
            Assert.Equal("Pickle", entry.GetValue(nameProperty, ValueSource.Original));
        }

        [Fact]
        public virtual void AcceptChanges_detaches_Deleted_entities()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
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
        public virtual void Non_transparent_sidecar_does_not_intercept_normal_property_read_and_write()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeEntity).FullName);
            var idProperty = entityType.FindProperty("Id");
            var nameProperty = entityType.FindProperty("Name");

            var entry = CreateInternalEntry(
                TestHelpers.Instance.CreateContextServices(model),
                entityType,
                new SomeEntity { Id = 1, Name = "Kool" },
                new ValueBuffer(new object[] { 1, "Kool" }));

            Assert.Equal(1, entry[idProperty]);
            Assert.Equal("Kool", entry[nameProperty]);

            entry.SetValue(idProperty, 7, ValueSource.Original);

            Assert.Equal(1, entry[idProperty]);
            Assert.Equal("Kool", entry[nameProperty]);

            entry[idProperty] = 77;

            Assert.Equal(77, entry[idProperty]);
            Assert.Equal("Kool", entry[nameProperty]);
        }

        private static IModel BuildOneToOneModel()
        {
            var modelBuilder = TestHelpers.Instance.CreateConventionBuilder();

            modelBuilder
                .Entity<FirstDependent>()
                .HasOne(e => e.Second)
                .WithOne(e => e.First)
                .HasForeignKey<SecondDependent>(e => e.Id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder
                .Entity<Root>(b =>
                    {
                        b.Property(e => e.Id).ValueGeneratedNever();

                        b.HasOne(e => e.First)
                            .WithOne(e => e.Root)
                            .HasForeignKey<FirstDependent>(e => e.Id);
                    });

            return modelBuilder.Model;
        }

        [Fact]
        public void Unchanged_entity_with_conceptually_null_FK_with_cascade_delete_is_marked_Deleted()
        {
            var model = BuildOneToOneModel();
            var entityType = model.FindEntityType(typeof(SecondDependent).FullName);
            var fkProperty = entityType.FindProperty("Id");

            var entry = CreateInternalEntry(TestHelpers.Instance.CreateContextServices(model), entityType, new SecondDependent());

            entry[fkProperty] = 77;
            entry.SetEntityState(EntityState.Unchanged);

            entry[fkProperty] = null;
            entry.HandleConceptualNulls();

            Assert.Equal(EntityState.Deleted, entry.EntityState);
        }

        [Fact]
        public void Added_entity_with_conceptually_null_FK_with_cascade_delete_is_detached()
        {
            var model = BuildOneToOneModel();
            var entityType = model.FindEntityType(typeof(SecondDependent).FullName);
            var fkProperty = entityType.FindProperty("Id");

            var entry = CreateInternalEntry(TestHelpers.Instance.CreateContextServices(model), entityType, new SecondDependent());

            entry[fkProperty] = 77;
            entry.SetEntityState(EntityState.Added);

            entry[fkProperty] = null;
            entry.HandleConceptualNulls();

            Assert.Equal(EntityState.Detached, entry.EntityState);
        }

        [Fact]
        public void Unchanged_entity_with_conceptually_null_FK_without_cascade_delete_throws()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeDependentEntity).FullName);
            var keyProperties = new[] { entityType.FindProperty("Id1"), entityType.FindProperty("Id2") };
            var fkProperty = entityType.FindProperty("SomeEntityId");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeDependentEntity());
            entry[keyProperties[0]] = 77;
            entry[keyProperties[1]] = "ReadySalted";
            entry[fkProperty] = 99;

            entry.SetEntityState(EntityState.Unchanged);
            entry[fkProperty] = null;

            Assert.Equal(
                CoreStrings.RelationshipConceptualNull("SomeEntity", "SomeDependentEntity"),
                Assert.Throws<InvalidOperationException>(() => entry.HandleConceptualNulls()).Message);
        }

        [Fact]
        public void Unchanged_entity_with_conceptually_null_non_FK_property_throws()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(SomeDependentEntity).FullName);
            var keyProperties = new[] { entityType.FindProperty("Id1"), entityType.FindProperty("Id2") };
            var property = entityType.FindProperty("JustAProperty");
            var configuration = TestHelpers.Instance.CreateContextServices(model);

            var entry = CreateInternalEntry(configuration, entityType, new SomeDependentEntity());
            entry[keyProperties[0]] = 77;
            entry[keyProperties[1]] = "ReadySalted";
            entry[property] = 99;

            entry.SetEntityState(EntityState.Unchanged);
            entry[property] = null;

            Assert.Equal(
                CoreStrings.PropertyConceptualNull("JustAProperty", "SomeDependentEntity"),
                Assert.Throws<InvalidOperationException>(() => entry.HandleConceptualNulls()).Message);
        }

        public class TestInMemoryValueGeneratorSelector : InMemoryValueGeneratorSelector
        {
            private readonly TemporaryNumberValueGeneratorFactory _inMemoryFactory = new TemporaryNumberValueGeneratorFactory();

            public TestInMemoryValueGeneratorSelector(IValueGeneratorCache cache)
                : base(cache)
            {
            }

            public override ValueGenerator Create(IProperty property, IEntityType entityType)
                => property.ClrType == typeof(int)
                    ? _inMemoryFactory.Create(property)
                    : base.Create(property, entityType);
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
            => contextServices.GetRequiredService<IInternalEntityEntrySubscriber>().SnapshotAndSubscribe(
                new InternalEntityEntryFactory()
                    .Create(contextServices.GetRequiredService<IStateManager>(), entityType, entity), null);

        protected virtual InternalEntityEntry CreateInternalEntry(IServiceProvider contextServices, IEntityType entityType, object entity, ValueBuffer valueBuffer)
            => contextServices.GetRequiredService<IInternalEntityEntrySubscriber>().SnapshotAndSubscribe(
                new InternalEntityEntryFactory()
                    .Create(contextServices.GetRequiredService<IStateManager>(), entityType, entity, valueBuffer), null);

        protected virtual Model BuildModel()
        {
            var model = new Model();

            var someSimpleEntityType = model.AddEntityType(typeof(SomeSimpleEntityBase));
            var simpleKeyProperty = someSimpleEntityType.AddProperty("Id", typeof(int));
            simpleKeyProperty.IsShadowProperty = false;
            simpleKeyProperty.RequiresValueGenerator = true;
            someSimpleEntityType.GetOrSetPrimaryKey(simpleKeyProperty);

            var someCompositeEntityType = model.AddEntityType(typeof(SomeCompositeEntityBase));
            var compositeKeyProperty1 = someCompositeEntityType.AddProperty("Id1", typeof(int));
            compositeKeyProperty1.IsShadowProperty = false;
            var compositeKeyProperty2 = someCompositeEntityType.AddProperty("Id2", typeof(string));
            compositeKeyProperty2.IsShadowProperty = false;
            someCompositeEntityType.GetOrSetPrimaryKey(new[] { compositeKeyProperty1, compositeKeyProperty2 });

            var entityType1 = model.AddEntityType(typeof(SomeEntity));
            entityType1.HasBaseType(someSimpleEntityType);
            var property3 = entityType1.AddProperty("Name", typeof(string));
            property3.IsShadowProperty = false;
            property3.IsConcurrencyToken = true;

            var entityType2 = model.AddEntityType(typeof(SomeDependentEntity));
            entityType2.HasBaseType(someCompositeEntityType);
            var fk = entityType2.AddProperty("SomeEntityId", typeof(int));
            fk.IsShadowProperty = false;
            entityType2.GetOrAddForeignKey(new[] { fk }, entityType1.FindPrimaryKey(), entityType1);
            var justAProperty = entityType2.AddProperty("JustAProperty", typeof(int));
            justAProperty.IsShadowProperty = false;
            justAProperty.RequiresValueGenerator = true;

            var entityType3 = model.AddEntityType(typeof(FullNotificationEntity));
            var property6 = entityType3.AddProperty("Id", typeof(int));
            property6.IsShadowProperty = false;
            entityType3.GetOrSetPrimaryKey(property6);
            var property7 = entityType3.AddProperty("Name", typeof(string));
            property7.IsShadowProperty = false;
            property7.IsConcurrencyToken = true;

            var entityType4 = model.AddEntityType(typeof(ChangedOnlyEntity));
            var property8 = entityType4.AddProperty("Id", typeof(int));
            property8.IsShadowProperty = false;
            entityType4.GetOrSetPrimaryKey(property8);
            var property9 = entityType4.AddProperty("Name", typeof(string));
            property9.IsShadowProperty = false;
            property9.IsConcurrencyToken = true;

            var entityType5 = model.AddEntityType(typeof(SomeMoreDependentEntity));
            entityType5.HasBaseType(someSimpleEntityType);
            var fk5a = entityType5.AddProperty("Fk1", typeof(int));
            fk5a.IsShadowProperty = false;
            var fk5b = entityType5.AddProperty("Fk2", typeof(string));
            fk5b.IsShadowProperty = false;
            entityType5.GetOrAddForeignKey(new[] { fk5a, fk5b }, entityType2.FindPrimaryKey(), entityType2);

            return model;
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

            private void NotifyChanged([CallerMemberName] string propertyName = "")
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }

            private void NotifyChanging([CallerMemberName] string propertyName = "")
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

            private void NotifyChanged([CallerMemberName] string propertyName = "")
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }
    }
}
