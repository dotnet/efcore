// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

public class BlankTriggerAddingConventionTest
{
    [ConditionalFact]
    public virtual void Adds_triggers_with_table_splitting()
    {
        var modelBuilder = CreateModelBuilder();

        modelBuilder.Entity<Order>().SplitToTable("OrderDetails", s => s.Property(o => o.CustomerId));

        var model = modelBuilder.FinalizeModel();

        var entity = model.FindEntityType(typeof(Order))!;

        Assert.Equal(new[] { "OrderDetails_Trigger", "Order_Trigger" }, entity.GetDeclaredTriggers().Select(t => t.ModelName));
    }

    [ConditionalFact]
    public virtual void Does_not_add_triggers_without_tables()
    {
        var modelBuilder = CreateModelBuilder();

        modelBuilder.Entity<Order>().ToView("Orders");
        modelBuilder.Entity<Order>().SplitToView("OrderDetails", s => s.Property(o => o.CustomerId));

        var model = modelBuilder.FinalizeModel();

        var entity = model.FindEntityType(typeof(Order))!;

        Assert.Empty(entity.GetDeclaredTriggers());
    }

    protected class Order
    {
        public int OrderId { get; set; }

        public int? CustomerId { get; set; }
        public Guid AnotherCustomerId { get; set; }
    }

    protected virtual ModelBuilder CreateModelBuilder()
        => FakeRelationalTestHelpers.Instance.CreateConventionBuilder(
            configureModel:
            b => b.Conventions.Add(
                p => new BlankTriggerAddingConvention(
                    p.GetRequiredService<ProviderConventionSetBuilderDependencies>(),
                    p.GetRequiredService<RelationalConventionSetBuilderDependencies>())));
}
