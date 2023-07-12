// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

#nullable enable

public class SqlServerOutputClauseConventionTest
{
    [ConditionalFact]
    public void Output_clause_is_enabled_by_default()
    {
        var modelBuilder = SqlServerTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<Order>();
        var model = modelBuilder.Model;

        var entityType = model.FindEntityType(typeof(Order))!;
        var tableIdentifier = StoreObjectIdentifier.Create(entityType, StoreObjectType.Table)!.Value;

        Assert.True(entityType.IsSqlOutputClauseUsed(tableIdentifier));
    }

    [ConditionalFact]
    public void Trigger_disables_output_clause()
    {
        var modelBuilder = SqlServerTestHelpers.Instance.CreateConventionBuilder();
        var model = modelBuilder.Model;

        var entityTypeBuilder = modelBuilder.Entity<Order>();
        var entityType = model.FindEntityType(typeof(Order))!;
        var tableIdentifier = StoreObjectIdentifier.Create(entityType, StoreObjectType.Table)!.Value;

        Assert.True(entityType.IsSqlOutputClauseUsed(tableIdentifier));

        entityTypeBuilder.ToTable(t => t.HasTrigger("Trigger1"));
        Assert.False(entityType.IsSqlOutputClauseUsed(tableIdentifier));
        entityTypeBuilder.ToTable(t => t.HasTrigger("Trigger2"));
        Assert.False(entityType.IsSqlOutputClauseUsed(tableIdentifier));

        entityTypeBuilder.Metadata.RemoveTrigger("Trigger1");
        Assert.False(entityType.IsSqlOutputClauseUsed(tableIdentifier));
        entityTypeBuilder.Metadata.RemoveTrigger("Trigger2");
        Assert.True(entityType.IsSqlOutputClauseUsed(tableIdentifier));
    }

    private class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
    }

    private class SpecialOrder : Order
    {
        public int SpecialProperty { get; set; }
    }
}
