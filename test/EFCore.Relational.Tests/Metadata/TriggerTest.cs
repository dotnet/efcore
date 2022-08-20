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

        var trigger = entityType.FindDeclaredTrigger("Customer_Trigger");

        Assert.NotNull(trigger);
        Assert.Same(entityType, trigger.EntityType);
        Assert.Equal("Customer_Trigger", trigger.GetDatabaseName());
        Assert.Equal("Customer", trigger.GetTableName());
        Assert.Null(trigger.GetTableSchema());
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

        var trigger = entityType.FindDeclaredTrigger("Customer_Trigger");

        Assert.NotNull(trigger);
        Assert.Same(entityType, trigger.EntityType);
        Assert.Equal("Customer_Trigger", trigger.GetDatabaseName());
        Assert.Equal("CustomerTable", trigger.GetTableName());
        Assert.Equal("dbo", trigger.GetTableSchema());
        Assert.Equal(ConfigurationSource.Explicit, ((IConventionTrigger)trigger).GetConfigurationSource());
    }

    [ConditionalFact]
    public void AddTrigger_with_duplicate_names_throws_exception()
    {
        var entityTypeBuilder = CreateConventionModelBuilder().Entity<Customer>();
        var entityType = entityTypeBuilder.Metadata;

        entityType.AddTrigger("SomeTrigger").SetTableName("SomeTable");

        Assert.Equal(
            CoreStrings.DuplicateTrigger("SomeTrigger", entityType.DisplayName(), entityType.DisplayName()),
            Assert.Throws<InvalidOperationException>(
                () => entityType.AddTrigger("SomeTrigger")).Message);
    }

    [ConditionalFact]
    public void RemoveTrigger_returns_trigger_when_trigger_exists()
    {
        var entityTypeBuilder = CreateConventionModelBuilder().Entity<Customer>();
        var entityType = entityTypeBuilder.Metadata;

        var trigger = entityType.AddTrigger("SomeTrigger");
        trigger.SetTableName("SomeTable");

        Assert.Same(trigger, entityType.RemoveTrigger("SomeTrigger"));
    }

    [ConditionalFact]
    public void RemoveTrigger_returns_null_when_trigger_is_missing()
    {
        var entityTypeBuilder = CreateConventionModelBuilder().Entity<Customer>();
        var entityType = entityTypeBuilder.Metadata;

        Assert.Null(entityType.RemoveTrigger("SomeTrigger"));
    }

    protected virtual ModelBuilder CreateConventionModelBuilder()
        => FakeRelationalTestHelpers.Instance.CreateConventionBuilder();

    private class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
