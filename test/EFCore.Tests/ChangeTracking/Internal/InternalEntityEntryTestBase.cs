// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.InMemory.ValueGeneration.Internal;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable AssignNullToNotNullAttribute
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

public abstract class InternalEntityEntryTestBase<
    TSomeEntity,
    TSomeSimpleEntityBase,
    TSomeDependentEntity,
    TSomeMoreDependentEntity,
    TRoot,
    TFirstDependent,
    TSecondDependent,
    TCompositeRoot,
    TCompositeFirstDependent,
    TSomeCompositeEntityBase,
    TCompositeSecondDependent,
    TKContext,
    TKSnapContext>
    where TSomeEntity : class, new()
    where TSomeSimpleEntityBase : class, new()
    where TSomeDependentEntity : class, new()
    where TSomeMoreDependentEntity : class, new()
    where TRoot : class, IRoot, new()
    where TFirstDependent : class, IFirstDependent, new()
    where TCompositeRoot : class, ICompositeRoot, new()
    where TCompositeFirstDependent : class, ICompositeFirstDependent, new()
    where TSecondDependent : class, ISecondDependent, new()
    where TSomeCompositeEntityBase : class, new()
    where TCompositeSecondDependent : class, ICompositeSecondDependent, new()
    where TKContext : DbContext, new()
    where TKSnapContext : DbContext, new()
{
    [ConditionalFact]
    public virtual void Store_setting_null_for_non_nullable_store_generated_property_throws()
    {
        using var context = new TKContext();
        var entry = context.Add(new TSomeEntity()).GetInfrastructure();
        var keyProperty = entry.EntityType.FindProperty("Id");

        entry.PrepareToSave();

        Assert.Equal(
            CoreStrings.ValueCannotBeNull("Id", "SomeSimpleEntityBase", "int"),
            Assert.Throws<InvalidOperationException>(
                () => entry.SetStoreGeneratedValue(keyProperty, null)).Message);
    }

    [ConditionalFact]
    public virtual void Changing_state_from_Unknown_causes_entity_to_start_tracking()
    {
        using var context = new TKContext();
        var entry = context.Entry(new TSomeEntity()).GetInfrastructure();
        var keyProperty = entry.EntityType.FindProperty("Id");

        entry[keyProperty] = 1;

        entry.SetEntityState(EntityState.Added);

        Assert.Equal(EntityState.Added, entry.EntityState);
        Assert.Contains(entry, context.GetService<IStateManager>().Entries);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Changing_state_to_Unknown_causes_entity_to_stop_tracking(bool useTempValue)
    {
        using var context = new TKContext();
        var entry1 = context.Entry(new TSomeEntity()).GetInfrastructure();
        var keyProperty = entry1.EntityType.FindProperty("Id");

        if (useTempValue)
        {
            entry1.SetTemporaryValue(keyProperty, -1, setModified: false);
        }
        else
        {
            entry1[keyProperty] = -1;
        }

        entry1.SetEntityState(EntityState.Added);
        entry1.SetEntityState(EntityState.Detached);

        Assert.Equal(EntityState.Detached, entry1.EntityState);
        Assert.DoesNotContain(entry1, context.GetService<IStateManager>().Entries);

        var entry2 = context.Entry(new TSomeEntity()).GetInfrastructure();
        entry2[keyProperty] = -1;

        entry2.SetEntityState(EntityState.Added);
        entry2.SetEntityState(EntityState.Detached);

        Assert.Equal(EntityState.Detached, entry2.EntityState);
        Assert.DoesNotContain(entry2, context.GetService<IStateManager>().Entries);
    }

    [ConditionalFact]
    public virtual void Changing_state_to_Unknown_causes_entity_with_temporary_key_to_stop_tracking()
    {
        using var context = new TKContext();
        var entry = context.Entry(new TSomeEntity()).GetInfrastructure();
        var keyProperty = entry.EntityType.FindProperty("Id");

        entry.SetTemporaryValue(keyProperty, -1, setModified: false);

        entry.SetEntityState(EntityState.Added);
        entry.SetEntityState(EntityState.Detached);

        Assert.Equal(EntityState.Detached, entry.EntityState);
        Assert.DoesNotContain(entry, context.GetService<IStateManager>().Entries);
    }

    [ConditionalFact] // GitHub #251, #1247
    public virtual void Changing_state_from_Added_to_Deleted_does_what_you_ask()
    {
        using var context = new TKContext();
        var entry = context.Add(new TSomeEntity()).GetInfrastructure();

        entry.SetEntityState(EntityState.Added);
        entry.SetEntityState(EntityState.Deleted);

        Assert.Equal(EntityState.Deleted, entry.EntityState);
        Assert.Contains(entry, context.GetService<IStateManager>().Entries);
    }

    [ConditionalFact]
    public virtual void Changing_state_to_Modified_or_Unchanged_causes_all_properties_to_be_marked_accordingly()
    {
        using var context = new TKContext();
        var entry = context.Add(new TSomeEntity()).GetInfrastructure();

        var keyProperty = entry.EntityType.FindProperty("Id");
        var nonKeyProperty = entry.EntityType.FindProperty("Name");

        Assert.False(entry.IsModified(keyProperty));
        Assert.False(entry.IsModified(nonKeyProperty));

        entry.SetEntityState(EntityState.Modified);

        Assert.False(entry.IsModified(keyProperty));
        Assert.NotEqual(nonKeyProperty.IsShadowProperty(), entry.IsModified(nonKeyProperty));

        entry.SetEntityState(EntityState.Unchanged, true);

        Assert.False(entry.IsModified(keyProperty));
        Assert.False(entry.IsModified(nonKeyProperty));

        entry.SetPropertyModified(nonKeyProperty);

        Assert.Equal(EntityState.Modified, entry.EntityState);
        Assert.False(entry.IsModified(keyProperty));
        Assert.True(entry.IsModified(nonKeyProperty));
    }

    [ConditionalFact]
    public virtual void Key_properties_throw_immediately_if_modified()
    {
        using var context = new TKContext();
        var entry = context.Add(new TSomeEntity()).GetInfrastructure();
        var keyProperty = entry.EntityType.FindProperty("Id");

        entry.SetEntityState(EntityState.Modified);

        Assert.False(entry.IsModified(keyProperty));

        entry.SetEntityState(EntityState.Unchanged, true);

        Assert.False(entry.IsModified(keyProperty));

        Assert.Equal(
            CoreStrings.KeyReadOnly("Id", "SomeEntity"),
            Assert.Throws<InvalidOperationException>(
                () => entry.SetPropertyModified(keyProperty)).Message);

        Assert.Equal(EntityState.Unchanged, entry.EntityState);
        Assert.False(entry.IsModified(keyProperty));

        Assert.Equal(
            CoreStrings.KeyReadOnly("Id", "SomeEntity"),
            Assert.Throws<InvalidOperationException>(
                () => entry[keyProperty] = 2).Message);

        Assert.Equal(EntityState.Unchanged, entry.EntityState);
        Assert.False(entry.IsModified(keyProperty));
    }

    [ConditionalFact]
    public virtual void Added_entities_can_have_temporary_values()
    {
        using var context = new TKContext();
        var entry = context.Add(new TSomeEntity()).GetInfrastructure();
        var keyProperty = entry.EntityType.FindProperty("Id");
        var nonKeyProperty = entry.EntityType.FindProperty("Name");

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

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Deleted)]
    public virtual void Changing_state_with_temp_value_throws(EntityState targetState)
    {
        using var context = new TKContext();
        var entry = context.Add(new TSomeEntity()).GetInfrastructure();
        var keyProperty = entry.EntityType.FindProperty("Id");

        entry.SetEntityState(EntityState.Added);
        entry.SetTemporaryValue(keyProperty, -1);

        Assert.Equal(
            CoreStrings.TempValuePersists("Id", "SomeEntity", targetState.ToString()),
            Assert.Throws<InvalidOperationException>(() => entry.SetEntityState(targetState)).Message);
    }

    [ConditionalFact]
    public virtual void Detaching_with_temp_values_does_not_throw()
    {
        using var context = new TKContext();
        var entry = context.Add(new TSomeEntity()).GetInfrastructure();
        var keyProperty = entry.EntityType.FindProperty("Id");

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

    [ConditionalFact]
    public virtual void Setting_an_explicit_value_marks_property_as_not_temporary()
    {
        using var context = new TKContext();
        var entry = context.Add(new TSomeEntity()).GetInfrastructure();
        var keyProperty = entry.EntityType.FindProperty("Id");

        entry.SetEntityState(EntityState.Added);
        entry.SetTemporaryValue(keyProperty, -1);

        Assert.True(entry.HasTemporaryValue(keyProperty));

        entry[keyProperty] = 77;

        Assert.False(entry.HasTemporaryValue(keyProperty));

        entry.SetEntityState(EntityState.Unchanged); // Does not throw
    }

    [ConditionalFact]
    public virtual void Key_properties_share_value_generation_space_with_base()
    {
        using var context = new TKContext();
        var entry = context.Add(new TSomeEntity()).GetInfrastructure();
        var keyProperty = entry.EntityType.FindProperty("Id");
        var altKeyProperty = entry.EntityType.FindProperty("NonId");

        Assert.NotEqual(0, entry[keyProperty]);
        Assert.Equal(entry[keyProperty], entry[altKeyProperty]);

        var baseEntry = context.Add(new TSomeSimpleEntityBase()).GetInfrastructure();

        Assert.NotNull(baseEntry[keyProperty]);
        Assert.NotEqual(0, baseEntry[keyProperty]);
        Assert.Equal(baseEntry[keyProperty], baseEntry[altKeyProperty]);
        Assert.NotEqual(entry[keyProperty], baseEntry[keyProperty]);
        Assert.NotEqual(entry[altKeyProperty], baseEntry[altKeyProperty]);
    }

    [ConditionalFact]
    public virtual void Value_generation_does_not_happen_if_property_has_non_default_value()
    {
        using var context = new TKContext();
        var entry = context.Add(new TSomeEntity()).GetInfrastructure();
        var keyProperty = entry.EntityType.FindProperty("Id");

        entry[keyProperty] = 31143;

        entry.SetEntityState(EntityState.Added);

        Assert.Equal(31143, entry[keyProperty]);
    }

    [ConditionalFact]
    public virtual void Modified_values_are_reset_when_entity_is_changed_to_Added()
    {
        using var context = new TKContext();
        var entry = context.Update(new TSomeEntity()).GetInfrastructure();
        var property = entry.EntityType.FindProperty("Name");

        entry[entry.EntityType.FindProperty("Id")] = 1;

        entry.SetEntityState(EntityState.Modified);
        entry.SetPropertyModified(property);

        entry.SetEntityState(EntityState.Added);

        Assert.False(entry.HasTemporaryValue(property));
    }

    [ConditionalFact]
    public virtual void Changing_state_to_Added_triggers_value_generation_for_any_property()
    {
        using var context = new TKContext();
        var entry = context.Entry(new TSomeDependentEntity()).GetInfrastructure();

        var entityType = entry.EntityType;
        entry[entityType.FindProperty("Id1")] = 77;
        entry[entityType.FindProperty("Id2")] = "Ready Salted";
        entry.SetEntityState(EntityState.Added);

        var property = entityType.FindProperty("JustAProperty");

        Assert.NotEqual(0, entry[property]);
    }

    [ConditionalFact]
    public virtual void Notification_that_an_FK_property_has_changed_updates_the_snapshot()
    {
        using var context = new TKContext();
        var entry = context.Entry(new TSomeDependentEntity()).GetInfrastructure();

        var entityType = entry.EntityType;
        entry[entityType.FindProperty("Id1")] = 77;
        entry[entityType.FindProperty("Id2")] = "Ready Salted";
        entry.SetEntityState(EntityState.Added);

        var fkProperty = entityType.FindProperty("SomeEntityId");

        entry[fkProperty] = 77;
        entry.SetRelationshipSnapshotValue(fkProperty, 78);

        entry[fkProperty] = 79;

        var keyValue = entry.GetRelationshipSnapshotValue(entityType.GetForeignKeys().Single().Properties.Single());
        Assert.Equal(79, keyValue);
    }

    [ConditionalFact]
    public virtual void Setting_property_to_the_same_value_does_not_update_the_snapshot()
    {
        using var context = new TKContext();
        var entry = context.Entry(new TSomeDependentEntity()).GetInfrastructure();

        var entityType = entry.EntityType;
        entry[entityType.FindProperty("Id1")] = 77;
        entry[entityType.FindProperty("Id2")] = "Ready Salted";
        entry.SetEntityState(EntityState.Unchanged);

        var fkProperty = entityType.FindProperty("SomeEntityId");

        entry[fkProperty] = 77;
        entry.SetRelationshipSnapshotValue(fkProperty, 78);

        entry[fkProperty] = 77;

        var keyValue = entry.GetRelationshipSnapshotValue(entityType.GetForeignKeys().Single().Properties.Single());
        Assert.Equal(78, keyValue);
    }

    [ConditionalFact]
    public virtual void Can_get_property_value_after_creation_from_value_buffer()
    {
        using var context = new TKContext();
        var stateManager = context.GetService<IStateManager>();
        var entityType = context.Model.FindEntityType(typeof(TSomeEntity));

        var entry = stateManager.CreateEntry(
            new Dictionary<string, object> { { "Id", 1 }, { "Name", "Kool" } },
            entityType
        );

        var keyProperty = entityType.FindProperty("Id");
        var property = entityType.FindProperty("Name");

        Assert.Equal(1, entry[keyProperty]);
        Assert.Equal("Kool", entry[property]);
    }

    [ConditionalFact]
    public virtual void Can_set_property_value_after_creation_from_value_buffer()
    {
        using var context = new TKContext();
        var stateManager = context.GetService<IStateManager>();
        var entityType = context.Model.FindEntityType(typeof(TSomeEntity));

        var entry = stateManager.CreateEntry(
            new Dictionary<string, object> { { "Id", 1 }, { "Name", "Kool" } },
            entityType
        );

        var nameProperty = entityType.FindProperty("Name");
        entry[nameProperty] = "Mule";

        Assert.Equal("Mule", entry[nameProperty]);
    }

    [ConditionalFact]
    public virtual void Can_get_value_buffer_from_properties()
    {
        using var context = new TKContext();
        var entry = context.Add(new TSomeEntity()).GetInfrastructure();
        var keyProperty = entry.EntityType.FindProperty("Id");
        var nonKeyProperty = entry.EntityType.FindProperty("Name");

        entry[keyProperty] = 77;
        entry[nonKeyProperty] = "Magic Tree House";

        Assert.Equal(
            [77, "SomeEntity", 1, "Magic Tree House"],
            CreateValueBuffer(entry));
    }

    private static object[] CreateValueBuffer(IUpdateEntry entry)
        => entry.EntityType.GetProperties().Select(entry.GetCurrentValue).ToArray();

    protected void AllOriginalValuesTest(object entity)
    {
        using var context = new TKSnapContext();
        var entry = context.Entry(entity).GetInfrastructure();
        var idProperty = entry.EntityType.FindProperty("Id");
        var nameProperty = entry.EntityType.FindProperty("Name");

        entry[idProperty] = 1;
        entry[nameProperty] = "Kool";
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

    [ConditionalFact]
    public virtual void Required_original_values_can_be_accessed_for_entity_that_does_no_notification()
        => OriginalValuesTest(new TSomeEntity());

    protected void OriginalValuesTest(object entity)
    {
        using var context = new TKContext();
        var entry = context.Entry(entity).GetInfrastructure();
        entry[entry.EntityType.FindProperty("Id")] = 1;
        var nameProperty = entry.EntityType.FindProperty("Name");
        entry[nameProperty] = "Kool";
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

    [ConditionalFact]
    public virtual void Required_original_values_can_be_accessed_generically_for_entity_that_does_no_notification()
        => GenericOriginalValuesTest(new TSomeEntity());

    protected void GenericOriginalValuesTest(object entity)
    {
        using var context = new TKContext();
        var entry = context.Entry(entity).GetInfrastructure();
        var idProperty = entry.EntityType.FindProperty("Id");
        var nameProperty = entry.EntityType.FindProperty("Name");

        entry[idProperty] = 77;
        entry[nameProperty] = "Kool";
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

    [ConditionalFact]
    public virtual void Null_original_values_are_handled_for_entity_that_does_no_notification()
        => NullOriginalValuesTest(new TSomeEntity());

    protected void NullOriginalValuesTest(object entity)
    {
        using var context = new TKContext();
        var entry = context.Entry(entity).GetInfrastructure();
        var idProperty = entry.EntityType.FindProperty("Id");
        var nameProperty = entry.EntityType.FindProperty("Name");

        entry[idProperty] = 77;
        entry.SetEntityState(EntityState.Unchanged);

        Assert.Null(entry.GetOriginalValue(nameProperty));
        Assert.Null(entry[nameProperty]);

        entry[nameProperty] = "Beans";

        Assert.Equal(nameProperty.IsShadowProperty() ? "Beans" : null, entry.GetOriginalValue(nameProperty));
        Assert.Equal("Beans", entry[nameProperty]);

        entry.SetOriginalValue(nameProperty, "Franks");

        Assert.Equal("Franks", entry.GetOriginalValue(nameProperty));
        Assert.Equal("Beans", entry[nameProperty]);

        entry.SetOriginalValue(nameProperty, null);

        Assert.Null(entry.GetOriginalValue(nameProperty));
        Assert.Equal("Beans", entry[nameProperty]);
    }

    [ConditionalFact]
    public virtual void Null_original_values_are_handled_generically_for_entity_that_does_no_notification()
        => GenericNullOriginalValuesTest(new TSomeEntity());

    protected void GenericNullOriginalValuesTest(object entity)
    {
        using var context = new TKContext();
        var entry = context.Entry(entity).GetInfrastructure();
        var idProperty = entry.EntityType.FindProperty("Id");
        var nameProperty = entry.EntityType.FindProperty("Name");

        entry[idProperty] = 77;
        entry.SetEntityState(EntityState.Unchanged);

        Assert.Null(entry.GetOriginalValue<string>(nameProperty));
        Assert.Null(entry.GetCurrentValue<string>(nameProperty));

        entry[nameProperty] = "Beans";

        Assert.Equal(nameProperty.IsShadowProperty() ? "Beans" : null, entry.GetOriginalValue<string>(nameProperty));
        Assert.Equal("Beans", entry.GetCurrentValue<string>(nameProperty));

        entry.SetOriginalValue(nameProperty, "Franks");

        Assert.Equal("Franks", entry.GetOriginalValue<string>(nameProperty));
        Assert.Equal("Beans", entry.GetCurrentValue<string>(nameProperty));

        entry.SetOriginalValue(nameProperty, null);

        Assert.Null(entry.GetOriginalValue<string>(nameProperty));
        Assert.Equal("Beans", entry.GetCurrentValue<string>(nameProperty));
    }

    [ConditionalFact]
    public virtual void Setting_property_using_state_entry_always_marks_as_modified_no_notifications()
        => SetPropertyInternalEntityEntryTest(new TSomeEntity());

    protected void SetPropertyInternalEntityEntryTest(object entity)
    {
        using var context = new TKContext();
        var entry = context.Entry(entity).GetInfrastructure();
        var idProperty = entry.EntityType.FindProperty("Id");
        var nameProperty = entry.EntityType.FindProperty("Name");

        entry[idProperty] = 77;
        entry[nameProperty] = "Kool";
        entry.SetEntityState(EntityState.Unchanged);

        Assert.False(entry.IsModified(idProperty));
        Assert.False(entry.IsModified(nameProperty));
        Assert.Equal(EntityState.Unchanged, entry.EntityState);

        entry[nameProperty] = "Kool";

        Assert.False(entry.IsModified(nameProperty));
        Assert.Equal(EntityState.Unchanged, entry.EntityState);

        entry[nameProperty] = "Beans";

        Assert.True(entry.IsModified(nameProperty));
        Assert.Equal(EntityState.Modified, entry.EntityState);
    }

    [ConditionalFact]
    public void Can_get_entity()
    {
        using var context = new TKContext();
        var entity = new TSomeEntity();
        var entry = context.Attach(entity).GetInfrastructure();

        Assert.Same(entity, entry.Entity);
    }

    [ConditionalFact]
    public void Can_set_and_get_property_value_from_CLR_object()
    {
        using var context = new TKContext();
        var entity = new TSomeEntity();
        var entry = context.Entry(entity).GetInfrastructure();
        var entityType = entry.EntityType;
        var keyProperty = entityType.FindProperty("Id");
        var nonKeyProperty = entityType.FindProperty("Name");

        entry[keyProperty] = 77;
        entry[nonKeyProperty] = "Magic Tree House";
        entry.SetEntityState(EntityState.Added);

        Assert.Equal(77, entry[keyProperty]);
        Assert.Equal("Magic Tree House", entry[nonKeyProperty]);

        entry[keyProperty] = 78;
        entry[nonKeyProperty] = "Normal Tree House";

        Assert.Equal(78, entry[keyProperty]);
        Assert.Equal("Normal Tree House", entry[nonKeyProperty]);
    }

    [ConditionalFact]
    public void All_original_values_can_be_accessed_for_entity_that_does_no_notification()
        => AllOriginalValuesTest(new TSomeEntity());

    [ConditionalFact]
    public virtual void AcceptChanges_does_nothing_for_unchanged_entities()
        => AcceptChangesNoop(EntityState.Unchanged);

    [ConditionalFact]
    public virtual void AcceptChanges_does_nothing_for_unknown_entities()
        => AcceptChangesNoop(EntityState.Detached);

    private void AcceptChangesNoop(EntityState entityState)
    {
        using var context = new TKContext();
        var entry = context.Entry(new TSomeEntity()).GetInfrastructure();
        var entityType = entry.EntityType;
        var keyProperty = entityType.FindProperty("Id");
        var nonKeyProperty = entityType.FindProperty("Name");
        entry[keyProperty] = 1;
        entry[nonKeyProperty] = "Kool";

        entry.SetEntityState(entityState);

        entry.AcceptChanges();

        Assert.Equal(entityState, entry.EntityState);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Added)]
    public void AcceptChanges_makes_entities_Unchanged(EntityState entityState)
    {
        using var context = new TKContext();
        var entry = context.Entry(new TSomeEntity()).GetInfrastructure();
        var entityType = entry.EntityType;
        var keyProperty = entityType.FindProperty("Id");
        var nameProperty = entityType.FindProperty("Name");

        entry[keyProperty] = 1;
        entry[nameProperty] = "Kool";

        entry.SetEntityState(entityState);

        entry[nameProperty] = "Pickle";
        entry.SetOriginalValue(nameProperty, "Cheese");

        entry.AcceptChanges();

        Assert.Equal(EntityState.Unchanged, entry.EntityState);
        Assert.Equal("Pickle", entry[nameProperty]);
        Assert.Equal("Pickle", entry.GetOriginalValue(nameProperty));
    }

    [ConditionalFact]
    public virtual void AcceptChanges_makes_Modified_entities_Unchanged_and_effectively_resets_unused_original_values()
    {
        using var context = new TKContext();
        var entry = context.Entry(new TSomeEntity()).GetInfrastructure();
        var entityType = entry.EntityType;
        var keyProperty = entityType.FindProperty("Id");
        var nameProperty = entityType.FindProperty("Name");

        entry[keyProperty] = 1;
        entry[nameProperty] = "Kool";

        entry.SetEntityState(EntityState.Modified);

        entry[nameProperty] = "Pickle";

        entry.AcceptChanges();

        Assert.Equal(EntityState.Unchanged, entry.EntityState);
        Assert.Equal("Pickle", entry[nameProperty]);
        Assert.Equal("Pickle", entry.GetOriginalValue(nameProperty));
    }

    [ConditionalFact]
    public virtual void AcceptChanges_detaches_Deleted_entities()
    {
        using var context = new TKContext();
        var entry = context.Entry(new TSomeEntity()).GetInfrastructure();
        var entityType = entry.EntityType;
        var keyProperty = entityType.FindProperty("Id");
        var nameProperty = entityType.FindProperty("Name");

        entry[keyProperty] = 1;
        entry[nameProperty] = "Kool";

        entry.SetEntityState(EntityState.Deleted);

        entry.AcceptChanges();

        Assert.Equal(EntityState.Detached, entry.EntityState);
    }

    [ConditionalFact]
    public void Unchanged_entity_with_conceptually_null_FK_with_cascade_delete_is_marked_Deleted()
    {
        using var context = new KcContext();
        var entry = context.Entry(new TSecondDependent()).GetInfrastructure();
        var fkProperty = entry.EntityType.FindProperty("Id");

        entry[fkProperty] = 77;
        entry.SetEntityState(EntityState.Unchanged);

        entry[fkProperty] = null;
        entry.HandleConceptualNulls(false, force: false, isCascadeDelete: false);

        Assert.Equal(EntityState.Deleted, entry.EntityState);
    }

    [ConditionalFact]
    public void Added_entity_with_conceptually_null_FK_with_cascade_delete_is_detached()
    {
        using var context = new KcContext();
        var entry = context.Entry(new TSecondDependent()).GetInfrastructure();
        var fkProperty = entry.EntityType.FindProperty("Id");

        entry[fkProperty] = 77;
        entry.SetEntityState(EntityState.Added);

        entry[fkProperty] = null;
        entry.HandleConceptualNulls(false, force: false, isCascadeDelete: false);

        Assert.Equal(EntityState.Detached, entry.EntityState);
    }

    [ConditionalFact]
    public void Entity_with_partially_null_composite_FK_with_cascade_delete_is_marked_Deleted()
    {
        using var context = new KcrContext();
        var entry = context.Entry(new TCompositeSecondDependent()).GetInfrastructure();
        var entityType = entry.EntityType;
        var fkProperty1 = entityType.FindProperty("FirstId1");
        var fkProperty2 = entityType.FindProperty("FirstId2");

        entry[entityType.FindProperty("Id1")] = 66;
        entry[entityType.FindProperty("Id2")] = "Bar";
        entry[fkProperty1] = 77;
        entry[fkProperty2] = "Foo";
        entry.SetEntityState(EntityState.Unchanged);

        entry[fkProperty1] = null;
        entry.HandleConceptualNulls(false, force: false, isCascadeDelete: false);

        Assert.Equal(EntityState.Deleted, entry.EntityState);
    }

    [ConditionalFact]
    public void Entity_with_partially_null_composite_FK_without_cascade_delete_is_orphaned()
    {
        using var context = new KcContext();
        var entry = context.Entry(new TCompositeSecondDependent()).GetInfrastructure();
        var entityType = entry.EntityType;
        var fkProperty1 = entityType.FindProperty("FirstId1");
        var fkProperty2 = entityType.FindProperty("FirstId2");

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

    public class KContext : DbContext
    {
        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TSomeSimpleEntityBase>(
                b =>
                {
                    b.Property<int>("Id");
                    b.HasKey("Id");
                    b.Property<int>("NonId").ValueGeneratedOnAdd();
                    b.HasAlternateKey("NonId");
                });

            modelBuilder.Entity<TSomeEntity>().Property<string>("Name").IsConcurrencyToken().ValueGeneratedOnAdd();

            modelBuilder.Entity<TSomeCompositeEntityBase>(
                b =>
                {
                    b.Property<int>("Id1");
                    b.Property<string>("Id2");
                    b.HasKey("Id1", "Id2");
                });

            modelBuilder.Entity<TSomeDependentEntity>(
                b =>
                {
                    b.Property<int>("SomeEntityId");
                    b.HasOne<TSomeEntity>().WithMany().HasForeignKey("SomeEntityId");
                    b.Property<int>("JustAProperty").HasValueGenerator((p, e) => new InMemoryIntegerValueGenerator<int>(p.GetIndex()));
                });

            modelBuilder.Entity<TSomeMoreDependentEntity>(
                b =>
                {
                    b.Property<int>("Fk11");
                    b.Property<string>("Fk2");
                    b.HasOne<TSomeDependentEntity>().WithMany().HasForeignKey("Fk1", "Fk2");
                });
        }
    }

    public class KcContext : KContext
    {
        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TFirstDependent>(
                b =>
                {
                    b.Property<int>("Id");
                    b.HasOne(e => (TSecondDependent)e.Second)
                        .WithOne(e => (TFirstDependent)e.First)
                        .HasForeignKey<TSecondDependent>("Id")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder
                .Entity<TRoot>(
                    b =>
                    {
                        b.Property<int>("Id").ValueGeneratedNever();
                        b.HasOne(e => (TFirstDependent)e.First)
                            .WithOne(e => (TRoot)e.Root)
                            .HasForeignKey<TFirstDependent>("Id");
                    });

            modelBuilder.Entity<TCompositeRoot>(
                b =>
                {
                    b.Property<int>("Id1");
                    b.Property<string>("Id2");
                    b.HasKey("Id1", "Id2");
                });

            modelBuilder.Entity<TCompositeFirstDependent>(
                b =>
                {
                    b.Property<int>("Id1");
                    b.Property<string>("Id2");
                    b.Property<int>("RootId1");
                    b.Property<string>("RootId2");
                    b.HasKey("Id1", "Id2");
                });

            modelBuilder.Entity<TCompositeSecondDependent>(
                b =>
                {
                    b.Property<int>("FirstId1");
                    b.Property<string>("FirstId2");
                    b.Property<int>("Id1");
                    b.Property<string>("Id2");
                    b.HasKey("Id1", "Id2");
                });

            modelBuilder.Entity<TCompositeRoot>(
                b =>
                {
                    b.HasOne(e => (TCompositeFirstDependent)e.First)
                        .WithOne(e => (TCompositeRoot)e.Root)
                        .HasForeignKey<TCompositeFirstDependent>("RootId1", "RootId2")
                        .IsRequired(false);
                });

            modelBuilder.Entity<TCompositeFirstDependent>(
                b =>
                {
                    b.HasOne(e => (TCompositeSecondDependent)e.Second)
                        .WithOne(e => (TCompositeFirstDependent)e.First)
                        .HasForeignKey<TCompositeSecondDependent>("FirstId1", "FirstId2")
                        .IsRequired(false);
                });
        }
    }

    public class KcrContext : KcContext
    {
        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TCompositeRoot>(
                b =>
                {
                    b.Property<int>("Id1");
                    b.Property<string>("Id2");
                    b.HasKey("Id1", "Id2");
                    b.HasOne(e => (TCompositeFirstDependent)e.First)
                        .WithOne(e => (TCompositeRoot)e.Root)
                        .HasForeignKey<TCompositeFirstDependent>("RootId1", "RootId2")
                        .IsRequired();
                });

            modelBuilder.Entity<TCompositeFirstDependent>(
                b =>
                {
                    b.HasOne(e => (TCompositeSecondDependent)e.Second)
                        .WithOne(e => (TCompositeFirstDependent)e.First)
                        .HasForeignKey<TCompositeSecondDependent>("FirstId1", "FirstId2")
                        .IsRequired();
                });
        }
    }

    public class KSnapContext : KContext
    {
        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<TSomeEntity>(
                b =>
                {
                    b.Property<int>("Id");
                    b.HasKey("Id");
                    b.Property<string>("Name").IsConcurrencyToken();
                    b.HasChangeTrackingStrategy(ChangeTrackingStrategy.Snapshot);
                });
    }
}

public interface IRoot
{
    IFirstDependent First { get; set; }
}

public interface ICompositeSecondDependent
{
    ICompositeFirstDependent First { get; set; }
}

public interface IFirstDependent
{
    IRoot Root { get; set; }
    ISecondDependent Second { get; set; }
}

public interface ISecondDependent
{
    IFirstDependent First { get; set; }
}

public interface ICompositeRoot
{
    ICompositeFirstDependent First { get; set; }
}

public interface ICompositeFirstDependent
{
    ICompositeRoot Root { get; set; }
    ICompositeSecondDependent Second { get; set; }
}
