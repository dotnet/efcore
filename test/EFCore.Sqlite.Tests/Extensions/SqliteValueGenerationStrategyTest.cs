// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class SqliteValueGenerationStrategyTest
{
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
    public void UseAutoincrement_sets_value_generation_strategy()
    {
        var modelBuilder = new ModelBuilder();

        var propertyBuilder = modelBuilder
            .Entity<Customer>()
            .Property(e => e.Id);

        propertyBuilder.UseAutoincrement();

        Assert.Equal(SqliteValueGenerationStrategy.Autoincrement, propertyBuilder.Metadata.GetValueGenerationStrategy());
    }

    [ConditionalFact]
    public void Generic_UseAutoincrement_sets_value_generation_strategy()
    {
        var modelBuilder = new ModelBuilder();

        var propertyBuilder = modelBuilder
            .Entity<Customer>()
            .Property<int>(e => e.Id);

        propertyBuilder.UseAutoincrement();

        Assert.Equal(SqliteValueGenerationStrategy.Autoincrement, propertyBuilder.Metadata.GetValueGenerationStrategy());
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
        Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
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
        Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
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
        Assert.Equal(ValueGenerated.Never, property1.ValueGenerated);
        Assert.Equal(ValueGenerated.Never, property2.ValueGenerated);
    }

    private class Customer
    {
        public int Id { get; set; }
        public int OtherId { get; set; }
        public string? Name { get; set; }
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
}