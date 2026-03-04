// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Update.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Update;

#nullable disable

public abstract class UpdateSqlGeneratorTestBase
{
    [ConditionalFact]
    public virtual void AppendDeleteOperation_creates_full_delete_command_text()
    {
        var stringBuilder = new StringBuilder();
        var command = CreateDeleteCommand(false);

        CreateSqlGenerator().AppendDeleteOperation(stringBuilder, command, 0);

        AppendDeleteOperation_creates_full_delete_command_text_verification(stringBuilder);
    }

    protected abstract void AppendDeleteOperation_creates_full_delete_command_text_verification(StringBuilder stringBuilder);

    [ConditionalFact]
    public virtual void AppendDeleteOperation_creates_full_delete_command_text_with_concurrency_check()
    {
        var stringBuilder = new StringBuilder();
        var command = CreateDeleteCommand();

        CreateSqlGenerator().AppendDeleteOperation(stringBuilder, command, 0);

        AppendDeleteOperation_creates_full_delete_command_text_with_concurrency_check_verification(stringBuilder);
    }

    protected abstract void AppendDeleteOperation_creates_full_delete_command_text_with_concurrency_check_verification(
        StringBuilder stringBuilder);

    [ConditionalFact]
    public virtual void AppendInsertOperation_insert_if_store_generated_columns_exist()
    {
        var stringBuilder = new StringBuilder();
        var command = CreateInsertCommand();

        CreateSqlGenerator().AppendInsertOperation(stringBuilder, command, 0);

        AppendInsertOperation_insert_if_store_generated_columns_exist_verification(stringBuilder);
    }

    protected abstract void AppendInsertOperation_insert_if_store_generated_columns_exist_verification(StringBuilder stringBuilder);

    [ConditionalFact]
    public virtual void
        AppendInsertOperation_appends_insert_and_select_rowcount_if_no_store_generated_columns_exist_or_conditions_exist()
    {
        var stringBuilder = new StringBuilder();
        var command = CreateInsertCommand(false, false);

        CreateSqlGenerator().AppendInsertOperation(stringBuilder, command, 0);

        Assert.Equal(
            "INSERT INTO "
            + SchemaPrefix
            + OpenDelimiter
            + "Ducks"
            + CloseDelimiter
            + " ("
            + OpenDelimiter
            + "Id"
            + CloseDelimiter
            + ", "
            + OpenDelimiter
            + "Name"
            + CloseDelimiter
            + ", "
            + OpenDelimiter
            + "Quacks"
            + CloseDelimiter
            + ", "
            + OpenDelimiter
            + "ConcurrencyToken"
            + CloseDelimiter
            + ")"
            + Environment.NewLine
            + "VALUES (@p0, @p1, @p2, @p3);"
            + Environment.NewLine,
            stringBuilder.ToString());
    }

    [ConditionalFact]
    public virtual void AppendInsertOperation_for_store_generated_columns_but_no_identity()
    {
        var stringBuilder = new StringBuilder();
        var command = CreateInsertCommand(false);

        CreateSqlGenerator().AppendInsertOperation(stringBuilder, command, 0);

        AppendInsertOperation_for_store_generated_columns_but_no_identity_verification(stringBuilder);
    }

    protected abstract void AppendInsertOperation_for_store_generated_columns_but_no_identity_verification(StringBuilder stringBuilder);

    [ConditionalFact]
    public virtual void AppendInsertOperation_for_only_identity()
    {
        var stringBuilder = new StringBuilder();
        var command = CreateInsertCommand(true, false);

        CreateSqlGenerator().AppendInsertOperation(stringBuilder, command, 0);

        AppendInsertOperation_for_only_identity_verification(stringBuilder);
    }

    protected abstract void AppendInsertOperation_for_only_identity_verification(StringBuilder stringBuilder);

    [ConditionalFact]
    public virtual void AppendInsertOperation_for_all_store_generated_columns()
    {
        var stringBuilder = new StringBuilder();
        var command = CreateInsertCommand(true, true, true);

        CreateSqlGenerator().AppendInsertOperation(stringBuilder, command, 0);

        AppendInsertOperation_for_all_store_generated_columns_verification(stringBuilder);
    }

    protected abstract void AppendInsertOperation_for_all_store_generated_columns_verification(StringBuilder stringBuilder);

    [ConditionalFact]
    public virtual void AppendInsertOperation_for_only_single_identity_columns()
    {
        var stringBuilder = new StringBuilder();
        var command = CreateInsertCommand(identityKey: true, isComputed: false, defaultsOnly: true);

        CreateSqlGenerator().AppendInsertOperation(stringBuilder, command, 0);

        AppendInsertOperation_for_only_single_identity_columns_verification(stringBuilder);
    }

    protected abstract void AppendInsertOperation_for_only_single_identity_columns_verification(StringBuilder stringBuilder);

    [ConditionalFact]
    public virtual void AppendUpdateOperation_if_store_generated_columns_exist()
    {
        var stringBuilder = new StringBuilder();
        var command = CreateUpdateCommand();

        CreateSqlGenerator().AppendUpdateOperation(stringBuilder, command, 0);

        AppendUpdateOperation_if_store_generated_columns_exist_verification(stringBuilder);
    }

    protected abstract void AppendUpdateOperation_if_store_generated_columns_exist_verification(StringBuilder stringBuilder);

    [ConditionalFact]
    public virtual void AppendUpdateOperation_if_store_generated_columns_dont_exist()
    {
        var stringBuilder = new StringBuilder();
        var command = CreateUpdateCommand(false, false);

        CreateSqlGenerator().AppendUpdateOperation(stringBuilder, command, 0);

        AppendUpdateOperation_if_store_generated_columns_dont_exist_verification(stringBuilder);
    }

    protected abstract void AppendUpdateOperation_if_store_generated_columns_dont_exist_verification(StringBuilder stringBuilder);

    [ConditionalFact]
    public virtual void AppendUpdateOperation_appends_where_for_concurrency_token()
    {
        var stringBuilder = new StringBuilder();
        var command = CreateUpdateCommand(false);

        CreateSqlGenerator().AppendUpdateOperation(stringBuilder, command, 0);

        AppendUpdateOperation_appends_where_for_concurrency_token_verification(stringBuilder);
    }

    protected abstract void AppendUpdateOperation_appends_where_for_concurrency_token_verification(StringBuilder stringBuilder);

    [ConditionalFact]
    public virtual void AppendUpdateOperation_for_computed_property()
    {
        var stringBuilder = new StringBuilder();
        var command = CreateUpdateCommand(true, false);

        CreateSqlGenerator().AppendUpdateOperation(stringBuilder, command, 0);

        AppendUpdateOperation_for_computed_property_verification(stringBuilder);
    }

    protected abstract void AppendUpdateOperation_for_computed_property_verification(StringBuilder stringBuilder);

    [ConditionalFact]
    public virtual void GenerateNextSequenceValueOperation_returns_statement_with_sanitized_sequence()
    {
        var statement = CreateSqlGenerator().GenerateNextSequenceValueOperation("sequence" + CloseDelimiter + "; --", null);

        Assert.Equal(
            "SELECT NEXT VALUE FOR " + OpenDelimiter + "sequence" + CloseDelimiter + CloseDelimiter + "; --" + CloseDelimiter,
            statement);
    }

    [ConditionalFact]
    public virtual void GenerateNextSequenceValueOperation_correctly_handles_schemas()
    {
        var statement = CreateSqlGenerator().GenerateNextSequenceValueOperation("mysequence", "dbo");

        Assert.Equal(
            "SELECT NEXT VALUE FOR " + SchemaPrefix + OpenDelimiter + "mysequence" + CloseDelimiter,
            statement);
    }

    protected abstract IUpdateSqlGenerator CreateSqlGenerator();

    protected abstract string RowsAffected { get; }

    protected virtual IModificationCommandFactory CreateMutableModificationCommandFactory()
        => new ModificationCommandFactory();

    protected virtual string Identity
        => throw new NotImplementedException();

    protected virtual string OpenDelimiter
        => "\"";

    protected virtual string CloseDelimiter
        => "\"";

    protected virtual string Schema
        => "dbo";

    protected virtual string SchemaPrefix
        => string.IsNullOrEmpty(Schema) ? string.Empty : OpenDelimiter + Schema + CloseDelimiter + ".";

    protected virtual string GetIdentityWhereCondition(string columnName)
        => OpenDelimiter + columnName + CloseDelimiter + " = " + Identity;

    protected IModificationCommand CreateInsertCommand(bool identityKey = true, bool isComputed = true, bool defaultsOnly = false)
    {
        var model = GetDuckModel();
        var stateManager = TestHelpers.CreateContextServices(model).GetRequiredService<IStateManager>();
        var entry = stateManager.GetOrCreateEntry(new Duck());
        entry.SetEntityState(EntityState.Added);
        var generator = new ParameterNameGenerator();

        var duckType = entry.EntityType;
        var idProperty = duckType.FindProperty(nameof(Duck.Id));
        var nameProperty = duckType.FindProperty(nameof(Duck.Name));
        var quacksProperty = duckType.FindProperty(nameof(Duck.Quacks));
        var computedProperty = duckType.FindProperty(nameof(Duck.Computed));
        var concurrencyProperty = duckType.FindProperty(nameof(Duck.ConcurrencyToken));

        var columnModifications = new[]
        {
            new ColumnModificationParameters(
                entry, idProperty, idProperty.GetTableColumnMappings().Single().Column, generator.GenerateNext,
                idProperty.GetTableColumnMappings().Single().TypeMapping, identityKey, !identityKey, true, false, true),
            new ColumnModificationParameters(
                entry, nameProperty, nameProperty.GetTableColumnMappings().Single().Column, generator.GenerateNext,
                nameProperty.GetTableColumnMappings().Single().TypeMapping, false, true, false, false, true),
            new ColumnModificationParameters(
                entry, quacksProperty, quacksProperty.GetTableColumnMappings().Single().Column, generator.GenerateNext,
                quacksProperty.GetTableColumnMappings().Single().TypeMapping, false, true, false, false, true),
            new ColumnModificationParameters(
                entry, computedProperty, computedProperty.GetTableColumnMappings().Single().Column, generator.GenerateNext,
                computedProperty.GetTableColumnMappings().Single().TypeMapping, isComputed, false, false, false, true),
            new ColumnModificationParameters(
                entry, concurrencyProperty, concurrencyProperty.GetTableColumnMappings().Single().Column, generator.GenerateNext,
                concurrencyProperty.GetTableColumnMappings().Single().TypeMapping, false, true, false, false, true)
        };

        if (defaultsOnly)
        {
            columnModifications = columnModifications.Where(c => !c.IsWrite).ToArray();
        }

        return CreateModificationCommand(entry, columnModifications, false);
    }

    protected IModificationCommand CreateUpdateCommand(bool isComputed = true, bool concurrencyToken = true)
    {
        var model = GetDuckModel();
        var stateManager = TestHelpers.CreateContextServices(model).GetRequiredService<IStateManager>();
        var entry = stateManager.GetOrCreateEntry(new Duck());
        entry.SetEntityState(EntityState.Modified);
        var generator = new ParameterNameGenerator();

        var duckType = entry.EntityType;
        var idProperty = duckType.FindProperty(nameof(Duck.Id));
        var nameProperty = duckType.FindProperty(nameof(Duck.Name));
        var quacksProperty = duckType.FindProperty(nameof(Duck.Quacks));
        var computedProperty = duckType.FindProperty(nameof(Duck.Computed));
        var concurrencyProperty = duckType.FindProperty(nameof(Duck.ConcurrencyToken));

        var columnModifications = new[]
        {
            new ColumnModificationParameters(
                entry, idProperty, idProperty.GetTableColumnMappings().Single().Column, generator.GenerateNext,
                idProperty.GetTableColumnMappings().Single().TypeMapping, false, false, true, true, true),
            new ColumnModificationParameters(
                entry, nameProperty, nameProperty.GetTableColumnMappings().Single().Column, generator.GenerateNext,
                nameProperty.GetTableColumnMappings().Single().TypeMapping, false, true, false, false, true),
            new ColumnModificationParameters(
                entry, quacksProperty, quacksProperty.GetTableColumnMappings().Single().Column, generator.GenerateNext,
                quacksProperty.GetTableColumnMappings().Single().TypeMapping, false, true, false, false, true),
            new ColumnModificationParameters(
                entry, computedProperty, computedProperty.GetTableColumnMappings().Single().Column, generator.GenerateNext,
                computedProperty.GetTableColumnMappings().Single().TypeMapping, isComputed, false, false, false, true),
            new ColumnModificationParameters(
                entry, concurrencyProperty, concurrencyProperty.GetTableColumnMappings().Single().Column, generator.GenerateNext,
                concurrencyProperty.GetTableColumnMappings().Single().TypeMapping, false, true, false, concurrencyToken, true)
        };

        return CreateModificationCommand(entry, columnModifications, false);
    }

    protected IModificationCommand CreateDeleteCommand(bool concurrencyToken = true)
    {
        var stateManager = TestHelpers.CreateContextServices(GetDuckModel()).GetRequiredService<IStateManager>();
        var entry = stateManager.GetOrCreateEntry(new Duck());
        entry.SetEntityState(EntityState.Deleted);
        var generator = new ParameterNameGenerator();

        var duckType = entry.EntityType;
        var idProperty = duckType.FindProperty(nameof(Duck.Id));
        var concurrencyProperty = duckType.FindProperty(nameof(Duck.ConcurrencyToken));

        var columnModifications = new[]
        {
            new ColumnModificationParameters(
                entry, idProperty, idProperty.GetTableColumnMappings().Single().Column, generator.GenerateNext,
                idProperty.GetTableColumnMappings().Single().TypeMapping, false, false, true, true, true),
            new ColumnModificationParameters(
                entry, concurrencyProperty, concurrencyProperty.GetTableColumnMappings().Single().Column, generator.GenerateNext,
                concurrencyProperty.GetTableColumnMappings().Single().TypeMapping, false, false, false, concurrencyToken, true)
        };

        return CreateModificationCommand(entry, columnModifications, false);
    }

    protected abstract TestHelpers TestHelpers { get; }

    private IModel GetDuckModel()
    {
        var modelBuilder = TestHelpers.CreateConventionBuilder();
        modelBuilder.Entity<Duck>().ToTable("Ducks", Schema).Property(e => e.Id).ValueGeneratedNever();
        return modelBuilder.Model.FinalizeModel();
    }

    protected class Duck
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Quacks { get; set; }
        public Guid Computed { get; set; }
        public byte[] ConcurrencyToken { get; set; }
    }

    private IModificationCommand CreateModificationCommand(
        InternalEntityEntry entry,
        IReadOnlyList<ColumnModificationParameters> columnModifications,
        bool sensitiveLoggingEnabled)
    {
        var modificationCommandParameters = new ModificationCommandParameters(
            entry.EntityType.GetTableMappings().Single().Table, sensitiveLoggingEnabled);
        var modificationCommand = CreateMutableModificationCommandFactory().CreateModificationCommand(
            modificationCommandParameters);

        modificationCommand.AddEntry(entry, mainEntry: true);

        foreach (var columnModification in columnModifications)
        {
            ((INonTrackedModificationCommand)modificationCommand).AddColumnModification(columnModification);
        }

        return modificationCommand;
    }
}
