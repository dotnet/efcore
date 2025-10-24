// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.MergeOptionFeature;

public class RefreshFromDb_ComputedColumns_SqlServer_Test : IClassFixture<RefreshFromDb_ComputedColumns_SqlServer_Test.ComputedColumnsFixture>
{
    private readonly ComputedColumnsFixture _fixture;

    public RefreshFromDb_ComputedColumns_SqlServer_Test(ComputedColumnsFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task Test_PropertiesMappedToComputedColumns()
    {
        using var ctx = _fixture.CreateContext();

        // Get a product and its original computed values
        var product = await ctx.Products.OrderBy(c => c.Id).FirstAsync();
        var originalTotalValue = product.TotalValue;
        var originalDescription = product.Description;

        try
        {
            // Simulate external changes to base columns that affect computed columns
            var newPrice = 150.00m;
            var newQuantity = 25;
            var newName = "Updated Product";

            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Products] SET [Price] = {0}, [Quantity] = {1}, [Name] = {2} WHERE [Id] = {3}",
                newPrice, newQuantity, newName, product.Id);

            // Refresh the entity from the database
            await ctx.Entry(product).ReloadAsync();

            // Assert that computed columns are updated
            var expectedTotalValue = newPrice * newQuantity;
            var expectedDescription = $"{newName} - ${newPrice:F2}";

            Assert.Equal(expectedTotalValue, product.TotalValue);
            Assert.Equal(expectedDescription, product.Description);
            Assert.NotEqual(originalTotalValue, product.TotalValue);
            Assert.NotEqual(originalDescription, product.Description);
        }
        finally
        {
            // Cleanup - restore original values
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Products] SET [Price] = {0}, [Quantity] = {1}, [Name] = {2} WHERE [Id] = {3}",
                100.00m, 10, "Test Product", product.Id);
        }
    }

    [Fact]
    public async Task Test_ComputedColumnsInQuery()
    {
        using var ctx = _fixture.CreateContext();

        // Query entities and verify computed columns are populated
        var products = await ctx.Products
            .Where(p => p.TotalValue > 500)
            .ToListAsync();

        Assert.NotEmpty(products);

        foreach (var product in products)
        {
            // Verify computed values are correct
            Assert.Equal(product.Price * product.Quantity, product.TotalValue);
            Assert.Equal($"{product.Name} - ${product.Price:F2}", product.Description);
        }
    }

    [Fact]
    public async Task Test_ComputedColumnsWithDatabaseGenerated()
    {
        using var ctx = _fixture.CreateContext();

        var order = await ctx.Orders.OrderBy(c => c.Id).FirstAsync();
        var originalOrderDate = order.OrderDate;
        var originalFormattedDate = order.FormattedOrderDate;

        try
        {
            // Update the OrderDate which should trigger the computed column
            var newOrderDate = DateTime.Now.AddDays(-5);

            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Orders] SET [OrderDate] = {0} WHERE [Id] = {1}",
                newOrderDate, order.Id);

            // Refresh the entity
            await ctx.Entry(order).ReloadAsync();

            // Verify the computed formatted date is updated
            Assert.NotEqual(originalFormattedDate, order.FormattedOrderDate);
            Assert.Contains(newOrderDate.ToString("yyyy-MM-dd"), order.FormattedOrderDate);
        }
        finally
        {
            // Cleanup
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Orders] SET [OrderDate] = {0} WHERE [Id] = {1}",
                originalOrderDate, order.Id);
        }
    }

    public class ComputedColumnsFixture : SharedStoreFixtureBase<ComputedColumnsContext>
    {
        protected override string StoreName
            => "ComputedColumnsRefreshFromDb";

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).EnableSensitiveDataLogging();

        protected override Task SeedAsync(ComputedColumnsContext context)
        {
            var product1 = new Product
            {
                Name = "Test Product",
                Price = 100.00m,
                Quantity = 10
            };

            var product2 = new Product
            {
                Name = "Expensive Product",
                Price = 250.00m,
                Quantity = 3
            };

            var order1 = new Order
            {
                OrderDate = DateTime.Now.AddDays(-10),
                CustomerName = "Test Customer 1"
            };

            var order2 = new Order
            {
                OrderDate = DateTime.Now.AddDays(-5),
                CustomerName = "Test Customer 2"
            };

            context.Products.AddRange(product1, product2);
            context.Orders.AddRange(order1, order2);
            return context.SaveChangesAsync();
        }
    }

    public class ComputedColumnsContext : DbContext
    {
        public ComputedColumnsContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(p => p.Price)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(p => p.Quantity)
                    .IsRequired();

                // TotalValue is a computed column: Price * Quantity
                entity.Property(p => p.TotalValue)
                    .HasColumnType("decimal(18,2)")
                    .HasComputedColumnSql("[Price] * [Quantity]");

                // Description is a computed column with string concatenation
                entity.Property(p => p.Description)
                    .HasMaxLength(200)
                    .HasComputedColumnSql("[Name] + ' - $' + CAST([Price] AS NVARCHAR(20))");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);

                entity.Property(o => o.OrderDate)
                    .IsRequired();

                entity.Property(o => o.CustomerName)
                    .HasMaxLength(100)
                    .IsRequired();

                // FormattedOrderDate is a computed column that formats the date
                entity.Property(o => o.FormattedOrderDate)
                    .HasMaxLength(100)
                    .HasComputedColumnSql("'Order Date: ' + CONVERT(NVARCHAR(10), [OrderDate], 120)");
            });
        }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal TotalValue { get; set; } // Computed: Price * Quantity
        public string Description { get; set; } = ""; // Computed: Name + Price formatted
    }

    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; } = "";
        public string FormattedOrderDate { get; set; } = ""; // Computed: formatted date string
    }
}
