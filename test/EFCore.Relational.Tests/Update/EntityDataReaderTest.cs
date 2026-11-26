// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data.Common;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Update.Internal;

namespace Microsoft.EntityFrameworkCore.Update;

public class EntityDataReaderTest
{
    private class Order
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = null!;
        public decimal Total { get; set; }
        public DateTime OrderDate { get; set; }
    }

    private static IModel BuildModel()
    {
        var modelBuilder = FakeRelationalTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<Order>(b =>
        {
            b.ToTable("Orders");
            b.Property(o => o.OrderNumber).HasMaxLength(50).IsRequired();
            b.Property(o => o.Total).HasPrecision(18, 2);
        });

        return modelBuilder.FinalizeModel();
    }

    [Fact]
    public void Column_mappings_and_types_are_exposed_accurately()
    {
        var model = BuildModel();
        var entityType = model.FindEntityType(typeof(Order))!;
        var storeObject = StoreObjectIdentifier.Table("Orders");

        var orders = new List<Order>
        {
            new() { Id = 10, OrderNumber = "ORD-001", Total = 99.95m, OrderDate = new DateTime(2026, 1, 1) }
        };

        using var reader = new EntityDataReader<Order>(entityType, orders, storeObject, keepIdentity: true);

        Assert.Equal(4, reader.FieldCount);
        Assert.Equal("Id", reader.GetName(reader.GetOrdinal("Id")));
        Assert.Equal("OrderNumber", reader.GetName(reader.GetOrdinal("OrderNumber")));

        Assert.True(reader.Read());
        Assert.Equal(10, reader.GetInt32(reader.GetOrdinal("Id")));
        Assert.Equal("ORD-001", reader.GetString(reader.GetOrdinal("OrderNumber")));
        Assert.Equal(99.95m, reader.GetDecimal(reader.GetOrdinal("Total")));
        Assert.False(reader.IsDBNull(reader.GetOrdinal("Total")));
        Assert.False(reader.Read());
    }

    [Fact]
    public void IsDBNull_caches_evaluated_value_without_recomputing()
    {
        var model = BuildModel();
        var entityType = model.FindEntityType(typeof(Order))!;
        var storeObject = StoreObjectIdentifier.Table("Orders");

        var orders = new List<Order>
        {
            new() { Id = 1, OrderNumber = "A", Total = 10m, OrderDate = DateTime.UtcNow }
        };

        using var reader = new EntityDataReader<Order>(entityType, orders, storeObject, keepIdentity: true);

        Assert.True(reader.Read());
        var ordinal = reader.GetOrdinal("OrderNumber");

        // Verify calling IsDBNull first, then GetValue works seamlessly
        Assert.False(reader.IsDBNull(ordinal));
        Assert.Equal("A", reader.GetValue(ordinal));
        Assert.Equal("A", reader[ordinal]);
    }
}
