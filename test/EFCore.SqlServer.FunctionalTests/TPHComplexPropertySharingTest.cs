// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class TPHComplexPropertySharingTest(TPHComplexPropertySharingTest.TPHComplexPropertySharingFixture fixture)
    : IClassFixture<TPHComplexPropertySharingTest.TPHComplexPropertySharingFixture>
{
    protected TPHComplexPropertySharingFixture Fixture { get; } = fixture;

    [ConditionalFact]
    public virtual async Task Can_save_and_query_TPH_with_shared_complex_property_column()
    {
        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        context.Items.Add(new Item1 { Name = "Item 1" });
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();
        var item = await context.Items.FirstAsync();
        Assert.NotNull(item);
        Assert.Equal("Item 1", item.Name);
    }

    [ConditionalFact]
    public virtual async Task Can_read_original_values_with_shared_complex_property_column()
    {
        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        context.Items.Add(new Item1 { Name = "Item 1" });
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();
        var item = await context.Items.FirstAsync();
        var entry = context.ChangeTracker.Entries().First();
        var originalItem = (Item)entry.OriginalValues.ToObject();
        
        Assert.NotNull(originalItem);
        Assert.Equal("Item 1", originalItem.Name);
    }

    [ConditionalFact]
    public virtual async Task Can_save_and_query_TPH_with_shared_complex_property_column_with_value()
    {
        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        context.Items.Add(new Item1 { Name = "Item 1", Pricing = new ItemPrice { Amount = "10.99" } });
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();
        var item = await context.Items.OfType<Item1>().FirstAsync();
        Assert.NotNull(item.Pricing);
        Assert.Equal("10.99", item.Pricing.Amount);
    }

    [ConditionalFact]
    public virtual async Task Can_read_original_values_with_shared_complex_property_column_with_value()
    {
        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        context.Items.Add(new Item1 { Name = "Item 1", Pricing = new ItemPrice { Amount = "10.99" } });
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();
        var item = await context.Items.OfType<Item1>().FirstAsync();
        var entry = context.ChangeTracker.Entries().First();
        var originalItem = (Item)entry.OriginalValues.ToObject();
        
        Assert.NotNull(originalItem);
        Assert.Equal("Item 1", originalItem.Name);
        var item1 = Assert.IsType<Item1>(originalItem);
        Assert.NotNull(item1.Pricing);
        Assert.Equal("10.99", item1.Pricing.Amount);
    }

    protected ItemContext CreateContext()
        => Fixture.CreateContext();

    public class TPHComplexPropertySharingFixture : SharedStoreFixtureBase<ItemContext>
    {
        protected override string StoreName
            => "TPHComplexPropertySharingTest";

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;
    }
}

public class ItemContext(DbContextOptions<ItemContext> options) : DbContext(options)
{
    public required DbSet<Item> Items { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Item>().HasDiscriminator<string>("Discriminator")
            .HasValue<Item1>("Item1")
            .HasValue<Item2>("Item2");
        
        modelBuilder.Entity<Item1>().ComplexProperty(
            x => x.Pricing, 
            p => p.Property(a => a.Amount).HasColumnName("Price"));
        
        modelBuilder.Entity<Item2>().ComplexProperty(
            x => x.Pricing, 
            p => p.Property(a => a.Amount).HasColumnName("Price"));
    }
}

public abstract class Item
{
    public int Id { get; set; }
    public required string Name { get; set; }
}

public class Item1 : Item
{
    public ItemPrice? Pricing { get; set; }
}

public class Item2 : Item
{
    public ItemPrice? Pricing { get; set; }
}

public class ItemPrice
{
    public required string Amount { get; init; }
}
