// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

public class SqlServerMemoryOptimizedTablesConventionTest
{
    [ConditionalFact]
    public void Keys_and_indexes_are_nonclustered_for_memory_optimized_tables()
    {
        var modelBuilder = SqlServerTestHelpers.Instance.CreateConventionBuilder();

        modelBuilder.Entity<Order>();

        Assert.True(modelBuilder.Model.FindEntityType(typeof(Order)).GetKeys().All(k => k.IsClustered() == null));
        Assert.True(modelBuilder.Model.FindEntityType(typeof(Order)).GetIndexes().All(k => k.IsClustered() == null));

        modelBuilder.Entity<Order>().ToTable(tb => tb.IsMemoryOptimized());

        modelBuilder.Entity<Order>().HasKey(
            o => new { o.Id, o.CustomerId });
        modelBuilder.Entity<Order>().HasIndex(o => o.CustomerId);

        Assert.True(modelBuilder.Model.FindEntityType(typeof(Order)).GetKeys().All(k => k.IsClustered() == false));
        Assert.True(modelBuilder.Model.FindEntityType(typeof(Order)).GetIndexes().All(k => k.IsClustered() == false));

        modelBuilder.Entity<Order>().ToTable(tb => tb.IsMemoryOptimized(false));

        Assert.True(modelBuilder.Model.FindEntityType(typeof(Order)).GetKeys().All(k => k.IsClustered() == null));
        Assert.True(modelBuilder.Model.FindEntityType(typeof(Order)).GetIndexes().All(k => k.IsClustered() == null));
    }

    private class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
    }
}
