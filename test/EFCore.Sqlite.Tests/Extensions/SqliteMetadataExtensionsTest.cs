// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class SqliteMetadataExtensionsTest
{
    [ConditionalFact]
    public void Can_get_and_set_srid()
    {
        var modelBuilder = new ModelBuilder();

        var property = modelBuilder
            .Entity<Customer>()
            .Property(e => e.Geometry)
            .Metadata;

        Assert.Null(property.GetSrid());

        property.SetSrid(1);

        Assert.Equal(1, property.GetSrid());

        property.SetSrid(null);

        Assert.Null(property.GetSrid());
    }

    [ConditionalFact]
    public void Can_get_and_set_value_generation_strategy()
    {
        var modelBuilder = new ModelBuilder();

        var property = modelBuilder
            .Entity<Customer>()
            .Property(e => e.Id)
            .Metadata;

        Assert.Equal(SqliteValueGenerationStrategy.None, property.GetValueGenerationStrategy());

        property.SetValueGenerationStrategy(SqliteValueGenerationStrategy.Autoincrement);

        Assert.Equal(SqliteValueGenerationStrategy.Autoincrement, property.GetValueGenerationStrategy());

        property.SetValueGenerationStrategy(null);

        Assert.Equal(SqliteValueGenerationStrategy.None, property.GetValueGenerationStrategy());
    }

    [ConditionalFact]
    public void Can_set_value_generation_strategy_on_mutable_property()
    {
        var modelBuilder = new ModelBuilder();

        var property = (IMutableProperty)modelBuilder
            .Entity<Customer>()
            .Property(e => e.Id)
            .Metadata;

        Assert.Equal(SqliteValueGenerationStrategy.None, property.GetValueGenerationStrategy());

        ((IMutableProperty)property).SetValueGenerationStrategy(SqliteValueGenerationStrategy.Autoincrement);

        Assert.Equal(SqliteValueGenerationStrategy.Autoincrement, property.GetValueGenerationStrategy());
    }

    [ConditionalFact]
    public void Default_value_generation_strategy_for_integer_primary_key()
    {
        var modelBuilder = new ModelBuilder();

        var property = modelBuilder
            .Entity<Customer>()
            .Property(e => e.Id)
            .Metadata;

        // Without conventions, the default should be None
        Assert.Equal(SqliteValueGenerationStrategy.None, property.GetValueGenerationStrategy());
    }

    [ConditionalFact]
    public void No_autoincrement_for_non_primary_key()
    {
        var modelBuilder = new ModelBuilder();

        var property = modelBuilder
            .Entity<Customer>()
            .Property(e => e.OtherId)
            .Metadata;

        Assert.Equal(SqliteValueGenerationStrategy.None, property.GetValueGenerationStrategy());
    }

    [ConditionalFact]
    public void No_autoincrement_for_non_integer_primary_key()
    {
        var modelBuilder = new ModelBuilder();

        var property = modelBuilder
            .Entity<CustomerWithStringKey>()
            .Property(e => e.Id)
            .Metadata;

        Assert.Equal(SqliteValueGenerationStrategy.None, property.GetValueGenerationStrategy());
    }

    [ConditionalFact]
    public void No_autoincrement_for_composite_primary_key()
    {
        var modelBuilder = new ModelBuilder();

        modelBuilder
            .Entity<CustomerWithCompositeKey>(b =>
            {
                b.HasKey(e => new { e.Id1, e.Id2 });
            });

        var property1 = modelBuilder.Entity<CustomerWithCompositeKey>().Property(e => e.Id1).Metadata;
        var property2 = modelBuilder.Entity<CustomerWithCompositeKey>().Property(e => e.Id2).Metadata;

        Assert.Equal(SqliteValueGenerationStrategy.None, property1.GetValueGenerationStrategy());
        Assert.Equal(SqliteValueGenerationStrategy.None, property2.GetValueGenerationStrategy());
    }

    [ConditionalFact]
    public void No_autoincrement_when_default_value_set()
    {
        var modelBuilder = new ModelBuilder();

        var property = modelBuilder
            .Entity<Customer>()
            .Property(e => e.Id)
            .HasDefaultValue(42)
            .Metadata;

        Assert.Equal(SqliteValueGenerationStrategy.None, property.GetValueGenerationStrategy());
    }

    [ConditionalFact]
    public void No_autoincrement_when_default_value_sql_set()
    {
        var modelBuilder = new ModelBuilder();

        var property = modelBuilder
            .Entity<Customer>()
            .Property(e => e.Id)
            .HasDefaultValueSql("1")
            .Metadata;

        Assert.Equal(SqliteValueGenerationStrategy.None, property.GetValueGenerationStrategy());
    }

    [ConditionalFact]
    public void No_autoincrement_when_computed_column_sql_set()
    {
        var modelBuilder = new ModelBuilder();

        var property = modelBuilder
            .Entity<Customer>()
            .Property(e => e.Id)
            .HasComputedColumnSql("1")
            .Metadata;

        Assert.Equal(SqliteValueGenerationStrategy.None, property.GetValueGenerationStrategy());
    }

    [ConditionalFact]
    public void No_autoincrement_when_property_is_foreign_key()
    {
        var modelBuilder = new ModelBuilder();

        modelBuilder.Entity<Order>(b =>
        {
            b.HasKey(e => e.Id);
            b.Property(e => e.CustomerId);
            b.HasOne<Customer>()
                .WithMany()
                .HasForeignKey(e => e.CustomerId);
        });

        var property = modelBuilder.Entity<Order>().Property(e => e.CustomerId).Metadata;

        Assert.Equal(SqliteValueGenerationStrategy.None, property.GetValueGenerationStrategy());
    }

    private class Customer
    {
        public int Id { get; set; }
        public int OtherId { get; set; }
        public string? Name { get; set; }
        public string? Geometry { get; set; }
    }

    private class CustomerWithStringKey
    {
        public string Id { get; set; } = null!;
        public string? Name { get; set; }
    }

    private class CustomerWithCompositeKey
    {
        public int Id1 { get; set; }
        public int Id2 { get; set; }
        public string? Name { get; set; }
    }

    private class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
    }

    private class CustomerNoPK
    {
        public int Id { get; set; }
        public int OtherId { get; set; }
        public string? Name { get; set; }
        public string? Geometry { get; set; }
    }
}
