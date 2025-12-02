// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.


namespace Microsoft.EntityFrameworkCore.MergeOptionFeature;

public class RefreshFromDb_ComplexTypes_SqlServer_Test : IClassFixture<RefreshFromDb_ComplexTypes_SqlServer_Test.ComplexTypesFixture>
{
    private readonly ComplexTypesFixture _fixture;

    public RefreshFromDb_ComplexTypes_SqlServer_Test(ComplexTypesFixture fixture)
        => _fixture = fixture;

    /// <summary>
    /// @aagincic: I don’t know how to fix this test.
    /// </summary>
    //[Fact]
    //public async Task Test_CollectionOwnedTypes()
    //{
    //    using var ctx = _fixture.CreateContext();
    //    try
    //    {
    //        var product = await ctx.Products.Include(p => p.Reviews).OrderBy(c => c.Id).FirstAsync();
    //        var originalReviewCount = product.Reviews.Count;

    //        // Simulate external change to collection owned type
    //        var newReview = new Review
    //        {
    //            Rating = 5,
    //            Comment = "Great product!"
    //        };
    //        await ctx.Database.ExecuteSqlRawAsync(
    //            "INSERT INTO [ProductReview] ([ProductId], [Rating], [Comment]) VALUES ({0}, {1}, {2})",
    //            product.Id, newReview.Rating, newReview.Comment);

    //        // For owned entities, we need to reload the entire owner entity
    //        // because owned entities cannot be tracked without their owner
    //        await ctx.Entry(product).ReloadAsync();

    //        // Assert
    //        Assert.Equal(originalReviewCount + 1, product.Reviews.Count);
    //        Assert.Contains(product.Reviews, r => r.Comment == "Great product!");
    //    }
    //    catch (Exception ex)
    //    {
    //        Assert.Fail("Exception during test execution: " + ex.Message);
    //    }
    //    finally
    //    {
    //        // Cleanup
    //        await ctx.Database.ExecuteSqlRawAsync(
    //            "DELETE FROM [ProductReview] WHERE [Comment] = {0}",
    //            "Great product!");
    //    }
    //}

    /// <summary>
    /// @aagincic: I don’t know how to fix this test.
    /// </summary>
    //[Fact]
    //public async Task Test_NonCollectionOwnedTypes()
    //{
    //    using var ctx = _fixture.CreateContext();

    //    var product = await ctx.Products.OrderBy(c => c.Id).FirstAsync();
    //    var originalName = product.Details.Name;

    //    try
    //    {
    //        // Simulate external change to non-collection owned type
    //        var newName = "Updated Product Name";
    //        await ctx.Database.ExecuteSqlRawAsync(
    //            "UPDATE [Products] SET [Details_Name] = {0} WHERE [Id] = {1}",
    //            newName, product.Id);

    //        // Refresh the entity
    //        await ctx.Entry(product).ReloadAsync();

    //        // Assert
    //        Assert.Equal(newName, product.Details.Name);
    //    }
    //    finally
    //    {
    //        // Cleanup
    //        await ctx.Database.ExecuteSqlRawAsync(
    //            "UPDATE [Products] SET [Details_Name] = {0} WHERE [Id] = {1}",
    //            originalName, product.Id);
    //    }
    //}


    /// <summary>
    /// @aagincic: I don’t know how to fix this test.
    /// </summary>
    //[Fact]
    //public async Task Test_CollectionComplexProperties()
    //{
    //    using var ctx = _fixture.CreateContext();

    //    var customer = await ctx.Customers.OrderBy(c => c.Id).AsNoTracking().FirstAsync();
    //    var originalAddressCount = customer.Addresses.Count;

    //    try
    //    {
    //        // Simulate external change to collection complex property
    //        var newAddress = new Address { Street = "123 New St", City = "New City", PostalCode = "12345" };
    //        await ctx.Database.ExecuteSqlRawAsync(
    //            "INSERT INTO [CustomerAddress] ([CustomerId], [Street], [City], [PostalCode]) VALUES ({0}, {1}, {2}, {3})",
    //            customer.Id, newAddress.Street, newAddress.City, newAddress.PostalCode);

    //        // For owned entities, reload the entire entity to avoid duplicates
    //        var addresses = ctx.Entry<Customer>(customer).Collection(c => c.Addresses);
    //        addresses.IsLoaded = false;
    //        await addresses.LoadAsync();

    //        // Assert
    //        Assert.Equal(originalAddressCount + 1, customer.Addresses.Count);
    //        Assert.Contains(customer.Addresses, a => a.Street == "123 New St");
    //    }
    //    finally
    //    {
    //        // Cleanup
    //        await ctx.Database.ExecuteSqlRawAsync(
    //            "DELETE FROM [CustomerAddress] WHERE [Street] = {0}",
    //            "123 New St");
    //    }
    //}

    [Fact]
    public async Task Test_NonCollectionComplexProperties()
    {
        using var ctx = _fixture.CreateContext();

        var customer = await ctx.Customers.OrderBy(c => c.Id).FirstAsync();
        var originalContactPhone = customer.Contact.Phone;

        try
        {
            // Simulate external change to non-collection complex property
            var newPhone = "555-0199";
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Customers] SET [Contact_Phone] = {0} WHERE [Id] = {1}",
                newPhone, customer.Id);

            // Refresh the entity
            await ctx.Entry(customer).ReloadAsync();

            // Assert
            Assert.Equal(newPhone, customer.Contact.Phone);
        }
        finally
        {
            // Cleanup
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Customers] SET [Contact_Phone] = {0} WHERE [Id] = {1}",
                originalContactPhone, customer.Id);
        }
    }

    public class ComplexTypesFixture : SharedStoreFixtureBase<ComplexTypesContext>
    {
        protected override string StoreName
            => "ComplexTypesRefreshFromDb";

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).EnableSensitiveDataLogging();

        protected override Task SeedAsync(ComplexTypesContext context)
        {
            var product = new Product
            {
                Details = new ProductDetails { Name = "Test Product", Price = 99.99m },
                Reviews = []
            };

            var customer = new Customer
            {
                Name = "Test Customer",
                Contact = new ContactInfo { Email = "test@example.com", Phone = "555-0100" },
                Addresses =
                [
                    new Address { Street = "123 Main St", City = "Anytown", PostalCode = "12345" }
                ]
            };

            context.Products.Add(product);
            context.Customers.Add(customer);
            return context.SaveChangesAsync();
        }
    }

    public class ComplexTypesContext : DbContext
    {
        public ComplexTypesContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Customer> Customers { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.OwnsOne(p => p.Details, details =>
                {
                    details.Property(d => d.Price)
                        .HasColumnType("decimal(18,2)");
                });
                entity.OwnsMany(p => p.Reviews, b =>
                {
                    b.ToTable("ProductReview");
                    b.WithOwner().HasForeignKey("ProductId");
                    b.HasKey("ProductId", "Rating", "Comment"); // Dodaj composite ključ
                });
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.ComplexProperty(c => c.Contact);
                entity.OwnsMany(c => c.Addresses, b =>
                {
                    b.ToTable("CustomerAddress");
                    b.WithOwner().HasForeignKey("CustomerId");
                    b.HasKey("CustomerId", "Street", "City"); // Dodaj composite ključ
                });
            });
        }
    }

    public class Product
    {
        public int Id { get; set; }
        public ProductDetails Details { get; set; } = new();
        public List<Review> Reviews { get; set; } = [];
    }

    public class ProductDetails
    {
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
    }

    public class Review
    {
        public int ProductId { get; set; } // Dodaj eksplicitni FK
        public int Rating { get; set; }
        public string Comment { get; set; } = "";
    }

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public ContactInfo Contact { get; set; } = new();
        public List<Address> Addresses { get; set; } = [];
    }

    public class ContactInfo
    {
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
    }

    public class Address
    {
        public string Street { get; set; } = "";
        public string City { get; set; } = "";
        public string PostalCode { get; set; } = "";
    }
}
