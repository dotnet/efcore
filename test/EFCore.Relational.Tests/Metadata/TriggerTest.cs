// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

public class TriggerTest
{
    [ConditionalFact]
    public void Can_create_trigger_for_default_table()
    {
        var modelBuilder = CreateConventionModelBuilder();
        var entityType = modelBuilder.Entity<Customer>().Metadata;

        modelBuilder
            .Entity<Customer>()
            .ToTable(tb => tb.HasTrigger("Customer_Trigger"));

        var trigger = entityType.FindTrigger("Customer_Trigger");

        Assert.NotNull(trigger);
        Assert.Same(entityType, trigger.EntityType);
        Assert.Equal("Customer_Trigger", trigger.Name);
        Assert.Equal("Customer", trigger.TableName);
        Assert.Null(trigger.TableSchema);
        Assert.Equal(ConfigurationSource.Explicit, ((IConventionTrigger)trigger).GetConfigurationSource());
    }

    [ConditionalFact]
    public void Can_create_trigger_for_specific_table()
    {
        var modelBuilder = CreateConventionModelBuilder();
        var entityType = modelBuilder.Entity<Customer>().Metadata;

        modelBuilder
            .Entity<Customer>()
            .ToTable("CustomerTable", "dbo", tb => tb.HasTrigger("Customer_Trigger"));

        var trigger = entityType.FindTrigger("Customer_Trigger");

        Assert.NotNull(trigger);
        Assert.Same(entityType, trigger.EntityType);
        Assert.Equal("Customer_Trigger", trigger.Name);
        Assert.Equal("CustomerTable", trigger.TableName);
        Assert.Equal("dbo", trigger.TableSchema);
        Assert.Equal(ConfigurationSource.Explicit, ((IConventionTrigger)trigger).GetConfigurationSource());
    }

    [ConditionalFact]
    public void Create_trigger_on_unmapped_entity_type_throws()
    {
        var modelBuilder = CreateConventionModelBuilder();

        var exception = Assert.Throws<InvalidOperationException>(() => modelBuilder
            .Entity<Customer>()
            .ToTable((string)null)
            .ToTable(tb => tb.HasTrigger("Customer_Trigger")));

        Assert.Equal(RelationalStrings.TriggerOnUnmappedEntityType("Customer_Trigger", "Customer"), exception.Message);
    }

    [ConditionalFact]
    public void AddTrigger_with_duplicate_names_throws_exception()
    {
        var entityTypeBuilder = CreateConventionModelBuilder().Entity<Customer>();
        var entityType = entityTypeBuilder.Metadata;

        entityType.AddTrigger("SomeTrigger", "SomeTable", null);

        Assert.Equal(
            RelationalStrings.DuplicateTrigger("SomeTrigger", entityType.DisplayName(), entityType.DisplayName()),
            Assert.Throws<InvalidOperationException>(
                () => entityType.AddTrigger("SomeTrigger", "SomeTable")).Message);
    }

    [ConditionalFact]
    public void RemoveTrigger_returns_trigger_when_trigger_exists()
    {
        var entityTypeBuilder = CreateConventionModelBuilder().Entity<Customer>();
        var entityType = entityTypeBuilder.Metadata;

        var constraint = entityType.AddTrigger("SomeTrigger", "SomeTable");

        Assert.Same(constraint, entityType.RemoveTrigger("SomeTrigger"));
    }

    [ConditionalFact]
    public void RemoveTrigger_returns_null_when_trigger_is_missing()
    {
        var entityTypeBuilder = CreateConventionModelBuilder().Entity<Customer>();
        var entityType = entityTypeBuilder.Metadata;

        Assert.Null(entityType.RemoveTrigger("SomeTrigger"));
    }

    protected virtual ModelBuilder CreateConventionModelBuilder()
        => RelationalTestHelpers.Instance.CreateConventionBuilder();

    private class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
