// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Test for SQLite AUTOINCREMENT with value converters, specifically for issues #30699 and #29519.
/// </summary>
public class SqliteAutoincrementWithConverterTest : IClassFixture<SqliteAutoincrementWithConverterTest.SqliteAutoincrementWithConverterFixture>
{
    private const string DatabaseName = "AutoincrementWithConverter";

    public SqliteAutoincrementWithConverterTest(SqliteAutoincrementWithConverterFixture fixture)
    {
        Fixture = fixture;
    }

    protected SqliteAutoincrementWithConverterFixture Fixture { get; }

    [ConditionalFact]
    public virtual async Task Strongly_typed_id_with_converter_gets_autoincrement()
    {
        await using var context = (PoolableDbContext)CreateContext();
        
        // Ensure the database is created
        await context.Database.EnsureCreatedAsync();
        
        // Check that the SQL contains AUTOINCREMENT for the strongly-typed ID
        var sql = context.Database.GenerateCreateScript();
        Assert.Contains("\"Id\" INTEGER NOT NULL CONSTRAINT \"PK_Products\" PRIMARY KEY AUTOINCREMENT", sql);
    }

    [ConditionalFact]
    public virtual async Task Insert_with_strongly_typed_id_generates_value()
    {
        await using var context = (PoolableDbContext)CreateContext();
        await context.Database.EnsureCreatedAsync();
        
        // Insert a product with strongly-typed ID
        var product = new Product { Name = "Test Product" };
        context.Products.Add(product);
        await context.SaveChangesAsync();
        
        // The ID should have been generated
        Assert.True(product.Id.Value > 0);
        
        // Insert another product
        var product2 = new Product { Name = "Test Product 2" };
        context.Products.Add(product2);
        await context.SaveChangesAsync();
        
        // The second ID should be different
        Assert.True(product2.Id.Value > product.Id.Value);
    }

    [ConditionalFact]
    public virtual async Task Migration_consistency_with_value_converter()
    {
        await using var context = (PoolableDbContext)CreateContext();
        
        // This test ensures that migrations don't generate repeated AlterColumn operations
        // by checking that the model annotation is consistent
        var property = context.Model.FindEntityType(typeof(Product))!.FindProperty(nameof(Product.Id))!;
        var strategy = property.GetValueGenerationStrategy();
        
        Assert.Equal(SqliteValueGenerationStrategy.Autoincrement, strategy);
    }

    [ConditionalFact]
    public virtual async Task Explicit_autoincrement_configuration_is_honored()
    {
        await using var context = (PoolableDbContext)CreateContext();
        
        // Check that explicitly configured AUTOINCREMENT is honored despite having a converter
        var property = context.Model.FindEntityType(typeof(Product))!.FindProperty(nameof(Product.Id))!;
        var strategy = property.GetValueGenerationStrategy();
        
        Assert.Equal(SqliteValueGenerationStrategy.Autoincrement, strategy);
        
        // Verify in the actual SQL generation
        var sql = context.Database.GenerateCreateScript();
        Assert.Contains("AUTOINCREMENT", sql);
    }

    protected virtual DbContext CreateContext()
        => Fixture.CreateContext();

    public class SqliteAutoincrementWithConverterFixture : SharedStoreFixtureBase<SqliteAutoincrementWithConverterTest.PoolableDbContext>
    {
        protected override string StoreName
            => DatabaseName;

        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;

        protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
            => base.AddServices(serviceCollection);

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder);

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<Product>(b =>
            {
                b.Property(e => e.Id).HasConversion(
                    v => v.Value,
                    v => new ProductId(v));
                b.Property(e => e.Id).UseAutoincrement(); // Explicit configuration
            });

            modelBuilder.Entity<Category>(); // Standard int ID for comparison
        }
    }

    // Test entities
    public record struct ProductId(int Value);

    public class Product
    {
        public ProductId Id { get; set; }
        public required string Name { get; set; }
    }

    public class Category
    {
        public int Id { get; set; }
        public required string Name { get; set; }
    }

    public class PoolableDbContext : DbContext
    {
        public PoolableDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Category> Categories => Set<Category>();
    }
}