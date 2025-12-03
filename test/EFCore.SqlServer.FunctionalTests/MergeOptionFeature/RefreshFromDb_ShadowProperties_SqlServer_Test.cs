// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.MergeOptionFeature;

public class RefreshFromDb_ShadowProperties_SqlServer_Test : IClassFixture<RefreshFromDb_ShadowProperties_SqlServer_Test.ShadowPropertiesFixture>
{
    private readonly ShadowPropertiesFixture _fixture;

    public RefreshFromDb_ShadowProperties_SqlServer_Test(ShadowPropertiesFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task Test_ShadowProperties()
    {
        using var ctx = _fixture.CreateContext();

        var product = await ctx.Products.OrderBy(c => c.Id).FirstAsync();
        var originalCreatedBy = (string?)ctx.Entry(product).Property("CreatedBy").CurrentValue;
        var originalCreatedAt = (DateTime?)ctx.Entry(product).Property("CreatedAt").CurrentValue;

        try
        {
            // Simulate external change to shadow properties
            var newCreatedBy = "NewUser";
            var newCreatedAt = DateTime.Now.AddDays(-1);

            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Products] SET [CreatedBy] = {0}, [CreatedAt] = {1} WHERE [Id] = {2}",
                newCreatedBy, newCreatedAt, product.Id);

            // Refresh the entity
            await ctx.Entry(product).ReloadAsync();

            // Assert that shadow properties are updated
            Assert.Equal(newCreatedBy, ctx.Entry(product).Property("CreatedBy").CurrentValue);
            Assert.Equal(newCreatedAt, ctx.Entry(product).Property("CreatedAt").CurrentValue);
        }
        finally
        {
            // Cleanup
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Products] SET [CreatedBy] = {0}, [CreatedAt] = {1} WHERE [Id] = {2}",
                originalCreatedBy ?? "System", originalCreatedAt ?? DateTime.Now, product.Id);
        }
    }

    [Fact]
    public async Task Test_ShadowProperties_WithRegularProperties()
    {
        using var ctx = _fixture.CreateContext();

        var product = await ctx.Products.OrderBy(c => c.Id).FirstAsync();
        var originalName = product.Name;
        var originalLastModified = (DateTime?)ctx.Entry(product).Property("LastModified").CurrentValue;

        try
        {
            // Update both regular and shadow properties externally
            var newName = "Updated Product Name";
            var newLastModified = DateTime.Now;

            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Products] SET [Name] = {0}, [LastModified] = {1} WHERE [Id] = {2}",
                newName, newLastModified, product.Id);

            // Refresh the entity
            await ctx.Entry(product).ReloadAsync();

            // Assert both regular and shadow properties are updated
            Assert.Equal(newName, product.Name);
            Assert.Equal(newLastModified, ctx.Entry(product).Property("LastModified").CurrentValue);
        }
        finally
        {
            // Cleanup
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Products] SET [Name] = {0}, [LastModified] = {1} WHERE [Id] = {2}",
                originalName, originalLastModified ?? (object)DBNull.Value, product.Id);
        }
    }

    [Fact]
    public async Task Test_ShadowForeignKey()
    {
        using var ctx = _fixture.CreateContext();

        var order = await ctx.Orders.OrderBy(c => c.Id).FirstAsync();
        var originalCustomerId = (int?)ctx.Entry(order).Property("CustomerId").CurrentValue;

        // Get a fallback customer ID in case originalCustomerId is null
        var fallbackCustomerId = originalCustomerId ?? await ctx.Customers.Select(c => c.Id).FirstAsync();

        try
        {
            // Get a valid customer ID that exists in the database
            var newCustomerId = originalCustomerId.HasValue
                ? await ctx.Customers
                    .Where(c => c.Id != originalCustomerId.Value)
                    .OrderBy(c => c.Id)
                    .Select(c => c.Id)
                    .FirstAsync()
                : await ctx.Customers
                    .OrderBy(c => c.Id)
                    .Select(c => c.Id)
                    .FirstAsync();

            // Update shadow foreign key externally
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Orders] SET [CustomerId] = {0} WHERE [Id] = {1}",
                newCustomerId, order.Id);

            // Refresh the entity
            await ctx.Entry(order).ReloadAsync();

            // Assert shadow foreign key is updated
            Assert.Equal(newCustomerId, ctx.Entry(order).Property("CustomerId").CurrentValue);
        }
        finally
        {
            // Cleanup - use the fallback customer ID (no await in finally block)
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Orders] SET [CustomerId] = {0} WHERE [Id] = {1}",
                fallbackCustomerId, order.Id);
        }
    }

    [Fact]
    public async Task Test_ShadowProperties_InQuery()
    {
        using var ctx = _fixture.CreateContext();

        // Query using shadow properties
        var recentProducts = await ctx.Products
            .Where(p => EF.Property<DateTime>(p, "CreatedAt") > DateTime.Now.AddMonths(-1))
            .ToListAsync();

        Assert.NotEmpty(recentProducts);

        // Verify shadow properties are loaded
        foreach (var product in recentProducts)
        {
            var createdAt = (DateTime?)ctx.Entry(product).Property("CreatedAt").CurrentValue;
            var createdBy = (string?)ctx.Entry(product).Property("CreatedBy").CurrentValue;
            
            Assert.NotNull(createdAt);
            Assert.NotNull(createdBy);
        }
    }

    public class ShadowPropertiesFixture : SharedStoreFixtureBase<ShadowPropertiesContext>
    {
        protected override string StoreName
            => "ShadowPropertiesRefreshFromDb";

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).EnableSensitiveDataLogging();

        protected override async Task SeedAsync(ShadowPropertiesContext context)
        {
            // First, seed and save customers to get generated IDs
            var customer1 = new Customer { Name = "Customer 1", Email = "customer1@example.com" };
            var customer2 = new Customer { Name = "Customer 2", Email = "customer2@example.com" };

            context.Customers.AddRange(customer1, customer2);
            await context.SaveChangesAsync();

            // Then, seed products and set their shadow properties
            var product1 = new Product { Name = "Product 1", Price = 100.00m };
            var product2 = new Product { Name = "Product 2", Price = 200.00m };

            // Now set shadow properties for products AFTER they are saved
            context.Entry(product1).Property("CreatedBy").CurrentValue = "System";
            context.Entry(product1).Property("CreatedAt").CurrentValue = DateTime.Now.AddMonths(-2);
            context.Entry(product1).Property("LastModified").CurrentValue = DateTime.Now.AddDays(-1);

            context.Entry(product2).Property("CreatedBy").CurrentValue = "Admin";
            context.Entry(product2).Property("CreatedAt").CurrentValue = DateTime.Now.AddDays(-15);
            context.Entry(product2).Property("LastModified").CurrentValue = DateTime.Now.AddHours(-6);

            context.Products.AddRange(product1, product2);
            await context.SaveChangesAsync();

            // Create orders with shadow foreign key properties set BEFORE adding to context
            var order1 = new Order { OrderDate = DateTime.Now.AddDays(-10), TotalAmount = 150.00m };
            var order2 = new Order { OrderDate = DateTime.Now.AddDays(-5), TotalAmount = 300.00m };

            // Add orders to context
            context.Orders.AddRange(order1, order2);

            // Set shadow foreign key properties IMMEDIATELY after adding to context but BEFORE SaveChanges
            context.Entry(order1).Property("CustomerId").CurrentValue = customer1.Id;
            context.Entry(order2).Property("CustomerId").CurrentValue = customer2.Id;

            // Save all changes
            await context.SaveChangesAsync();
        }
    }

    public class ShadowPropertiesContext : DbContext
    {
        public ShadowPropertiesContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Customer> Customers { get; set; } = null!;
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

                // Configure shadow properties
                entity.Property<string>("CreatedBy")
                    .HasMaxLength(50)
                    .IsRequired();
                
                entity.Property<DateTime>("CreatedAt")
                    .IsRequired();
                
                entity.Property<DateTime?>("LastModified");
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(c => c.Id);
                
                entity.Property(c => c.Name)
                    .HasMaxLength(100)
                    .IsRequired();
                
                entity.Property(c => c.Email)
                    .HasMaxLength(255)
                    .IsRequired();
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);
                
                entity.Property(o => o.OrderDate)
                    .IsRequired();
                
                entity.Property(o => o.TotalAmount)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                // Configure shadow foreign key
                entity.Property<int>("CustomerId")
                    .IsRequired();

                // Configure relationship using shadow foreign key
                entity.HasOne<Customer>()
                    .WithMany()
                    .HasForeignKey("CustomerId")
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        // Shadow properties: CreatedBy (string), CreatedAt (DateTime), LastModified (DateTime?)
    }

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
    }

    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        // Shadow properties: CustomerId (int) - foreign key
    }
}
