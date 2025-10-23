// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.MergeOptionFeature;

public class RefreshFromDb_ValueConverters_SqlServer_Test : IClassFixture<RefreshFromDb_ValueConverters_SqlServer_Test.ValueConvertersFixture>
{
    private readonly ValueConvertersFixture _fixture;

    public RefreshFromDb_ValueConverters_SqlServer_Test(ValueConvertersFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task Test_PropertiesWithValueConverters()
    {
        using var ctx = _fixture.CreateContext();

        var product = await ctx.Products.OrderBy(c => c.Name).FirstAsync();
        var originalStatus = product.Status;
        var originalTags = product.Tags.ToList();

        try
        {
            // Simulate external change to enum stored as string
            var newStatusString = "Discontinued";
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Products] SET [Status] = {0} WHERE [Id] = {1}",
                newStatusString, product.Id);

            // Simulate external change to collection stored as JSON
            var newTags = new List<string> { "electronics", "mobile", "smartphone" };
            var newTagsJson = JsonSerializer.Serialize(newTags);
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Products] SET [Tags] = {0} WHERE [Id] = {1}",
                newTagsJson, product.Id);

            // Refresh the entity
            await ctx.Entry(product).ReloadAsync();

            // Assert that value converters work correctly on refresh
            Assert.Equal(ProductStatus.Discontinued, product.Status);
            Assert.Equal(3, product.Tags.Count);
            Assert.Contains("electronics", product.Tags);
            Assert.Contains("mobile", product.Tags);
            Assert.Contains("smartphone", product.Tags);
        }
        finally
        {
            // Cleanup
            var originalTagsJson = JsonSerializer.Serialize(originalTags);
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Products] SET [Status] = {0}, [Tags] = {1} WHERE [Id] = {2}",
                originalStatus.ToString(), originalTagsJson, product.Id);
        }
    }

    [Fact]
    public async Task Test_DateTimeValueConverter()
    {
        using var ctx = _fixture.CreateContext();

        var user = await ctx.Users.OrderBy(c => c.Name).FirstAsync();
        var originalBirthDate = user.BirthDate;

        try
        {
            // Simulate external change to DateTime stored as string
            var newBirthDateString = "1990-05-15";
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Users] SET [BirthDate] = {0} WHERE [Id] = {1}",
                newBirthDateString, user.Id);

            // Refresh the entity
            await ctx.Entry(user).ReloadAsync();

            // Assert that DateTime converter works
            Assert.Equal(new DateTime(1990, 5, 15), user.BirthDate);
        }
        finally
        {
            // Cleanup
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Users] SET [BirthDate] = {0} WHERE [Id] = {1}",
                originalBirthDate.ToString("yyyy-MM-dd"), user.Id);
        }
    }

    [Fact]
    public async Task Test_GuidValueConverter()
    {
        using var ctx = _fixture.CreateContext();

        var user = await ctx.Users.OrderBy(c => c.Name).FirstAsync();
        var originalExternalId = user.ExternalId;

        try
        {
            // Simulate external change to Guid stored as string
            var newGuid = Guid.NewGuid();
            var newGuidString = newGuid.ToString();
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Users] SET [ExternalId] = {0} WHERE [Id] = {1}",
                newGuidString, user.Id);

            // Refresh the entity
            await ctx.Entry(user).ReloadAsync();

            // Assert that Guid converter works
            Assert.Equal(newGuid, user.ExternalId);
        }
        finally
        {
            // Cleanup
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Users] SET [ExternalId] = {0} WHERE [Id] = {1}",
                originalExternalId.ToString(), user.Id);
        }
    }

    [Fact]
    public async Task Test_CustomValueObjectConverter()
    {
        using var ctx = _fixture.CreateContext();

        var order = await ctx.Orders.OrderBy(c => c.OrderNumber).FirstAsync();
        var originalPrice = order.Price;

        try
        {
            // Simulate external change to Money value object stored as decimal
            var newPriceValue = 299.99m;
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Orders] SET [Price] = {0} WHERE [Id] = {1}",
                newPriceValue, order.Id);

            // Refresh the entity
            await ctx.Entry(order).ReloadAsync();

            // Assert that Money converter works
            Assert.Equal(new Money(newPriceValue), order.Price);
        }
        finally
        {
            // Cleanup
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Orders] SET [Price] = {0} WHERE [Id] = {1}",
                originalPrice.Value, order.Id);
        }
    }

    public class ValueConvertersFixture : SharedStoreFixtureBase<ValueConvertersContext>
    {
        protected override string StoreName
            => "ValueConvertersRefreshFromDb";

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).EnableSensitiveDataLogging();

        protected override Task SeedAsync(ValueConvertersContext context)
        {
            var product = new Product
            {
                Name = "Laptop",
                Status = ProductStatus.Active,
                Tags = ["electronics", "computer"]
            };

            var user = new User
            {
                Name = "John Doe",
                BirthDate = new DateTime(1985, 3, 10),
                ExternalId = Guid.NewGuid()
            };

            var order = new Order
            {
                OrderNumber = "ORD001",
                Price = new Money(199.99m)
            };

            context.Products.Add(product);
            context.Users.Add(user);
            context.Orders.Add(order);

            return context.SaveChangesAsync();
        }
    }

    public class ValueConvertersContext : DbContext
    {
        public ValueConvertersContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                // Enum to string converter
                entity.Property(p => p.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                // List<string> to JSON converter with value comparer
                entity.Property(p => p.Tags)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
                    .HasColumnType("nvarchar(max)")
                    .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                        (c1, c2) => c1!.SequenceEqual(c2!),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()));
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);

                entity.Property(u => u.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                // DateTime to string converter
                entity.Property(u => u.BirthDate)
                    .HasConversion(
                        v => v.ToString("yyyy-MM-dd"),
                        v => DateTime.ParseExact(v, "yyyy-MM-dd", null))
                    .HasMaxLength(10);

                // Guid to string converter
                entity.Property(u => u.ExternalId)
                    .HasConversion(
                        v => v.ToString(),
                        v => Guid.Parse(v))
                    .HasMaxLength(36);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);

                entity.Property(o => o.OrderNumber)
                    .HasMaxLength(50)
                    .IsRequired();

                // Money value object converter
                entity.Property(o => o.Price)
                    .HasConversion(
                        v => v.Value,
                        v => new Money(v))
                    .HasColumnType("decimal(18,2)");
            });
        }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public ProductStatus Status { get; set; }
        public List<string> Tags { get; set; } = [];
    }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public DateTime BirthDate { get; set; }
        public Guid ExternalId { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = "";
        public Money Price { get; set; } = new(0);
    }

    public enum ProductStatus
    {
        Active,
        Inactive,
        Discontinued
    }

    public readonly record struct Money(decimal Value)
    {
        public static implicit operator decimal(Money money) => money.Value;
        public static implicit operator Money(decimal value) => new(value);

        public override string ToString() => Value.ToString("C");
    }
}
