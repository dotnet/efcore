// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Update.Internal;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Local
namespace Microsoft.EntityFrameworkCore.Update;

public class ModificationCommandTest
{
    [ConditionalFact]
    public void ModificationCommand_initialized_correctly_for_added_entities_with_temp_generated_key()
    {
        var entry = CreateEntry(EntityState.Added, generateKeyValues: true);
        entry.SetTemporaryValue(entry.EntityType.FindPrimaryKey().Properties[0], -1);

        var command = CreateModificationCommand(entry, new ParameterNameGenerator().GenerateNext, false, null);
        command.AddEntry(entry, true);

        Assert.Equal("T1", command.TableName);
        Assert.Null(command.Schema);
        Assert.Equal(EntityState.Added, command.EntityState);
        Assert.Equal(3, command.ColumnModifications.Count);

        var columnMod = command.ColumnModifications[0];

        Assert.Equal("Col1", columnMod.ColumnName);
        Assert.Same(entry, columnMod.Entry);
        Assert.Equal("Id", columnMod.Property.Name);
        Assert.False(columnMod.IsCondition);
        Assert.True(columnMod.IsKey);
        Assert.True(columnMod.IsRead);
        Assert.False(columnMod.IsWrite);

        columnMod = command.ColumnModifications[1];

        Assert.Equal("Col2", columnMod.ColumnName);
        Assert.Same(entry, columnMod.Entry);
        Assert.Equal("Name1", columnMod.Property.Name);
        Assert.False(columnMod.IsCondition);
        Assert.False(columnMod.IsKey);
        Assert.False(columnMod.IsRead);
        Assert.True(columnMod.IsWrite);

        columnMod = command.ColumnModifications[2];

        Assert.Equal("Col3", columnMod.ColumnName);
        Assert.Same(entry, columnMod.Entry);
        Assert.Equal("Name2", columnMod.Property.Name);
        Assert.False(columnMod.IsCondition);
        Assert.False(columnMod.IsKey);
        Assert.False(columnMod.IsRead);
        Assert.True(columnMod.IsWrite);
    }

    [ConditionalFact]
    public void ModificationCommand_initialized_correctly_for_added_entities_with_non_temp_generated_key()
    {
        var entry = CreateEntry(EntityState.Added, generateKeyValues: true);

        var command = CreateModificationCommand(entry, new ParameterNameGenerator().GenerateNext, false, null);
        command.AddEntry(entry, true);

        Assert.Equal("T1", command.TableName);
        Assert.Null(command.Schema);
        Assert.Equal(EntityState.Added, command.EntityState);
        Assert.Equal(3, command.ColumnModifications.Count);

        var columnMod = command.ColumnModifications[0];

        Assert.Equal("Col1", columnMod.ColumnName);
        Assert.Same(entry, columnMod.Entry);
        Assert.Equal("Id", columnMod.Property.Name);
        Assert.False(columnMod.IsCondition);
        Assert.True(columnMod.IsKey);
        Assert.False(columnMod.IsRead);
        Assert.True(columnMod.IsWrite);

        columnMod = command.ColumnModifications[1];

        Assert.Equal("Col2", columnMod.ColumnName);
        Assert.Same(entry, columnMod.Entry);
        Assert.Equal("Name1", columnMod.Property.Name);
        Assert.False(columnMod.IsCondition);
        Assert.False(columnMod.IsKey);
        Assert.False(columnMod.IsRead);
        Assert.True(columnMod.IsWrite);

        columnMod = command.ColumnModifications[2];

        Assert.Equal("Col3", columnMod.ColumnName);
        Assert.Same(entry, columnMod.Entry);
        Assert.Equal("Name2", columnMod.Property.Name);
        Assert.False(columnMod.IsCondition);
        Assert.False(columnMod.IsKey);
        Assert.False(columnMod.IsRead);
        Assert.True(columnMod.IsWrite);
    }

    [ConditionalFact]
    public void ModificationCommand_initialized_correctly_for_added_entities_with_explicitly_specified_key_value()
    {
        var entry = CreateEntry(EntityState.Added);

        var command = CreateModificationCommand(entry, new ParameterNameGenerator().GenerateNext, false, null);
        command.AddEntry(entry, true);

        Assert.Equal("T1", command.TableName);
        Assert.Null(command.Schema);
        Assert.Equal(EntityState.Added, command.EntityState);
        Assert.Equal(3, command.ColumnModifications.Count);

        var columnMod = command.ColumnModifications[0];

        Assert.Equal("Col1", columnMod.ColumnName);
        Assert.Same(entry, columnMod.Entry);
        Assert.Equal("Id", columnMod.Property.Name);
        Assert.False(columnMod.IsCondition);
        Assert.True(columnMod.IsKey);
        Assert.False(columnMod.IsRead);
        Assert.True(columnMod.IsWrite);

        columnMod = command.ColumnModifications[1];

        Assert.Equal("Col2", columnMod.ColumnName);
        Assert.Same(entry, columnMod.Entry);
        Assert.Equal("Name1", columnMod.Property.Name);
        Assert.False(columnMod.IsCondition);
        Assert.False(columnMod.IsKey);
        Assert.False(columnMod.IsRead);
        Assert.True(columnMod.IsWrite);

        columnMod = command.ColumnModifications[2];

        Assert.Equal("Col3", columnMod.ColumnName);
        Assert.Same(entry, columnMod.Entry);
        Assert.Equal("Name2", columnMod.Property.Name);
        Assert.False(columnMod.IsCondition);
        Assert.False(columnMod.IsKey);
        Assert.False(columnMod.IsRead);
        Assert.True(columnMod.IsWrite);
    }

    [ConditionalFact]
    public void ModificationCommand_initialized_correctly_for_modified_entities_with_identity_key()
    {
        var entry = CreateEntry(EntityState.Modified, generateKeyValues: true);

        var command = CreateModificationCommand(entry, new ParameterNameGenerator().GenerateNext, false, null);
        command.AddEntry(entry, true);

        Assert.Equal("T1", command.TableName);
        Assert.Null(command.Schema);
        Assert.Equal(EntityState.Modified, command.EntityState);
        Assert.Equal(3, command.ColumnModifications.Count);

        var columnMod = command.ColumnModifications[0];

        Assert.Equal("Col1", columnMod.ColumnName);
        Assert.Same(entry, columnMod.Entry);
        Assert.Equal("Id", columnMod.Property.Name);
        Assert.True(columnMod.IsCondition);
        Assert.True(columnMod.IsKey);
        Assert.False(columnMod.IsRead);
        Assert.False(columnMod.IsWrite);

        columnMod = command.ColumnModifications[1];

        Assert.Equal("Col2", columnMod.ColumnName);
        Assert.Same(entry, columnMod.Entry);
        Assert.Equal("Name1", columnMod.Property.Name);
        Assert.False(columnMod.IsCondition);
        Assert.False(columnMod.IsKey);
        Assert.False(columnMod.IsRead);
        Assert.True(columnMod.IsWrite);

        columnMod = command.ColumnModifications[2];

        Assert.Equal("Col3", columnMod.ColumnName);
        Assert.Same(entry, columnMod.Entry);
        Assert.Equal("Name2", columnMod.Property.Name);
        Assert.False(columnMod.IsCondition);
        Assert.False(columnMod.IsKey);
        Assert.False(columnMod.IsRead);
        Assert.True(columnMod.IsWrite);
    }

    [ConditionalFact]
    public void ModificationCommand_initialized_correctly_for_modified_entities_with_client_generated_key()
    {
        var entry = CreateEntry(EntityState.Modified);

        var command = CreateModificationCommand(entry, new ParameterNameGenerator().GenerateNext, false, null);
        command.AddEntry(entry, true);

        Assert.Equal("T1", command.TableName);
        Assert.Null(command.Schema);
        Assert.Equal(EntityState.Modified, command.EntityState);
        Assert.Equal(3, command.ColumnModifications.Count);

        var columnMod = command.ColumnModifications[0];

        Assert.Equal("Col1", columnMod.ColumnName);
        Assert.Same(entry, columnMod.Entry);
        Assert.Equal("Id", columnMod.Property.Name);
        Assert.True(columnMod.IsCondition);
        Assert.True(columnMod.IsKey);
        Assert.False(columnMod.IsRead);
        Assert.False(columnMod.IsWrite);

        columnMod = command.ColumnModifications[1];

        Assert.Equal("Col2", columnMod.ColumnName);
        Assert.Same(entry, columnMod.Entry);
        Assert.Equal("Name1", columnMod.Property.Name);
        Assert.False(columnMod.IsCondition);
        Assert.False(columnMod.IsKey);
        Assert.False(columnMod.IsRead);
        Assert.True(columnMod.IsWrite);

        columnMod = command.ColumnModifications[2];

        Assert.Equal("Col3", columnMod.ColumnName);
        Assert.Same(entry, columnMod.Entry);
        Assert.Equal("Name2", columnMod.Property.Name);
        Assert.False(columnMod.IsCondition);
        Assert.False(columnMod.IsKey);
        Assert.False(columnMod.IsRead);
        Assert.True(columnMod.IsWrite);
    }

    [ConditionalFact]
    public void ModificationCommand_initialized_correctly_for_modified_entities_with_concurrency_token()
    {
        var entry = CreateEntry(EntityState.Modified, computeNonKeyValue: true);

        var command = CreateModificationCommand(entry, new ParameterNameGenerator().GenerateNext, false, null);
        command.AddEntry(entry, true);

        Assert.Equal("T1", command.TableName);
        Assert.Null(command.Schema);
        Assert.Equal(EntityState.Modified, command.EntityState);
        Assert.Equal(3, command.ColumnModifications.Count);

        var columnMod = command.ColumnModifications[0];

        Assert.Equal("Col1", columnMod.ColumnName);
        Assert.Same(entry, columnMod.Entry);
        Assert.Equal("Id", columnMod.Property.Name);
        Assert.True(columnMod.IsCondition);
        Assert.True(columnMod.IsKey);
        Assert.False(columnMod.IsRead);
        Assert.False(columnMod.IsWrite);

        columnMod = command.ColumnModifications[1];

        Assert.Equal("Col2", columnMod.ColumnName);
        Assert.Same(entry, columnMod.Entry);
        Assert.Equal("Name1", columnMod.Property.Name);
        Assert.True(columnMod.IsCondition);
        Assert.False(columnMod.IsKey);
        Assert.True(columnMod.IsRead);
        Assert.False(columnMod.IsWrite);

        columnMod = command.ColumnModifications[2];

        Assert.Equal("Col3", columnMod.ColumnName);
        Assert.Same(entry, columnMod.Entry);
        Assert.Equal("Name2", columnMod.Property.Name);
        Assert.True(columnMod.IsCondition);
        Assert.False(columnMod.IsKey);
        Assert.True(columnMod.IsRead);
        Assert.False(columnMod.IsWrite);
    }

    [ConditionalFact]
    public void ModificationCommand_initialized_correctly_for_deleted_entities()
    {
        var entry = CreateEntry(EntityState.Deleted);

        var command = CreateModificationCommand(entry, new ParameterNameGenerator().GenerateNext, false, null);
        command.AddEntry(entry, true);

        Assert.Equal("T1", command.TableName);
        Assert.Null(command.Schema);
        Assert.Equal(EntityState.Deleted, command.EntityState);
        Assert.Equal(1, command.ColumnModifications.Count);

        var columnMod = command.ColumnModifications[0];

        Assert.Equal("Col1", columnMod.ColumnName);
        Assert.Same(entry, columnMod.Entry);
        Assert.Equal("Id", columnMod.Property.Name);
        Assert.True(columnMod.IsCondition);
        Assert.True(columnMod.IsKey);
        Assert.False(columnMod.IsRead);
        Assert.False(columnMod.IsWrite);
    }

    [ConditionalFact]
    public void ModificationCommand_initialized_correctly_for_deleted_entities_with_concurrency_token()
    {
        var entry = CreateEntry(EntityState.Deleted, computeNonKeyValue: true);

        var command = CreateModificationCommand(entry, new ParameterNameGenerator().GenerateNext, false, null);
        command.AddEntry(entry, true);

        Assert.Equal("T1", command.TableName);
        Assert.Null(command.Schema);
        Assert.Equal(EntityState.Deleted, command.EntityState);
        Assert.Equal(3, command.ColumnModifications.Count);

        var columnMod = command.ColumnModifications[0];

        Assert.Equal("Col1", columnMod.ColumnName);
        Assert.Same(entry, columnMod.Entry);
        Assert.Equal("Id", columnMod.Property.Name);
        Assert.True(columnMod.IsCondition);
        Assert.True(columnMod.IsKey);
        Assert.False(columnMod.IsRead);
        Assert.False(columnMod.IsWrite);

        columnMod = command.ColumnModifications[1];

        Assert.Equal("Col2", columnMod.ColumnName);
        Assert.Same(entry, columnMod.Entry);
        Assert.Equal("Name1", columnMod.Property.Name);
        Assert.True(columnMod.IsCondition);
        Assert.False(columnMod.IsKey);
        Assert.False(columnMod.IsRead);
        Assert.False(columnMod.IsWrite);

        columnMod = command.ColumnModifications[2];

        Assert.Equal("Col3", columnMod.ColumnName);
        Assert.Same(entry, columnMod.Entry);
        Assert.Equal("Name2", columnMod.Property.Name);
        Assert.True(columnMod.IsCondition);
        Assert.False(columnMod.IsKey);
        Assert.False(columnMod.IsRead);
        Assert.False(columnMod.IsWrite);
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void ModificationCommand_throws_for_unchanged_entities(bool sensitive)
    {
        var entry = CreateEntry(EntityState.Unchanged);

        var command = CreateModificationCommand(entry, new ParameterNameGenerator().GenerateNext, sensitive, null);

        Assert.Equal(
            sensitive
                ? RelationalStrings.ModificationCommandInvalidEntityStateSensitive("T1", "{Id: 1}", EntityState.Unchanged)
                : RelationalStrings.ModificationCommandInvalidEntityState("T1", EntityState.Unchanged),
            Assert.Throws<InvalidOperationException>(() => command.AddEntry(entry, true)).Message);
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void ModificationCommand_throws_for_unknown_entities(bool sensitive)
    {
        var entry = CreateEntry(EntityState.Detached);

        var command = CreateModificationCommand(entry, new ParameterNameGenerator().GenerateNext, sensitive, null);

        Assert.Equal(
            sensitive
                ? RelationalStrings.ModificationCommandInvalidEntityStateSensitive("T1", "{Id: 1}", EntityState.Detached)
                : RelationalStrings.ModificationCommandInvalidEntityState("T1", EntityState.Detached),
            Assert.Throws<InvalidOperationException>(() => command.AddEntry(entry, true)).Message);
    }

    private class T1
    {
        public int Id { get; set; }
        public string Name1 { get; set; }
        public string Name2 { get; set; }
    }

    private static IModel BuildModel(bool generateKeyValues, bool computeNonKeyValue)
    {
        var modelBuilder = FakeRelationalTestHelpers.Instance.CreateConventionBuilder();
        var model = modelBuilder.Model;
        var entityType = model.AddEntityType(typeof(T1));

        var key = entityType.FindProperty("Id");
        key.ValueGenerated = generateKeyValues ? ValueGenerated.OnAdd : ValueGenerated.Never;
        key.SetColumnName("Col1");
        entityType.SetPrimaryKey(key);

        var nonKey1 = entityType.FindProperty("Name1");
        nonKey1.IsConcurrencyToken = computeNonKeyValue;

        nonKey1.SetColumnName("Col2");
        nonKey1.ValueGenerated = computeNonKeyValue ? ValueGenerated.OnAddOrUpdate : ValueGenerated.Never;

        var nonKey2 = entityType.FindProperty("Name2");
        nonKey2.IsConcurrencyToken = computeNonKeyValue;

        nonKey2.SetColumnName("Col3");
        nonKey2.ValueGenerated = computeNonKeyValue ? ValueGenerated.OnUpdate : ValueGenerated.Never;

        return modelBuilder.FinalizeModel();
    }

    private static InternalEntityEntry CreateEntry(
        EntityState entityState,
        bool generateKeyValues = false,
        bool computeNonKeyValue = false)
    {
        var model = BuildModel(generateKeyValues, computeNonKeyValue);

        return FakeRelationalTestHelpers.Instance.CreateInternalEntry(
            model,
            entityState,
            new
                T1
                {
                    Id = 1,
                    Name1 = computeNonKeyValue ? null : "Test",
                    Name2 = computeNonKeyValue ? null : "Test"
                });
    }

    private static IModificationCommand CreateModificationCommand(
        InternalEntityEntry entry,
        Func<string> generateParameterName,
        bool sensitiveLoggingEnabled,
        IComparer<IUpdateEntry> comparer)
        => new ModificationCommandFactory().CreateModificationCommand(
            new ModificationCommandParameters(
                entry.EntityType.GetTableMappings().Single().Table,
                sensitiveLoggingEnabled,
                detailedErrorsEnabled: false,
                comparer,
                generateParameterName));
}
