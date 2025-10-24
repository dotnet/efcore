// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.MergeOptionFeature;

public class RefreshFromDb_PrimitiveCollections_SqlServer_Test : IClassFixture<RefreshFromDb_PrimitiveCollections_SqlServer_Test.PrimitiveCollectionsFixture>
{
    private readonly PrimitiveCollectionsFixture _fixture;

    public RefreshFromDb_PrimitiveCollections_SqlServer_Test(PrimitiveCollectionsFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task Test_PrimitiveCollections()
    {
        using var ctx = _fixture.CreateContext();

        // Get a product with its tags collection
        var product = await ctx.Products.OrderBy(c => c.Id).FirstAsync();
        var originalTagCount = product.Tags.Count;
        var originalTags = product.Tags.ToList();

        try
        {
            // Simulate external change to primitive collection by updating JSON
            var newTags = new List<string>(originalTags) { "NewTag", "AnotherTag" };
            var newTagsJson = System.Text.Json.JsonSerializer.Serialize(newTags);

            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Products] SET [Tags] = {0} WHERE [Id] = {1}",
                newTagsJson, product.Id);

            // Refresh the entity
            await ctx.Entry(product).ReloadAsync();

            // Assert that the primitive collection is updated
            Assert.Equal(originalTagCount + 2, product.Tags.Count);
            Assert.Contains("NewTag", product.Tags);
            Assert.Contains("AnotherTag", product.Tags);
        }
        finally
        {
            // Cleanup - restore original tags
            var originalTagsJson = System.Text.Json.JsonSerializer.Serialize(originalTags);
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Products] SET [Tags] = {0} WHERE [Id] = {1}",
                originalTagsJson, product.Id);
        }
    }

    [Fact]
    public async Task Test_PrimitiveCollections_Numbers()
    {
        using var ctx = _fixture.CreateContext();

        var blog = await ctx.Blogs.OrderBy(c => c.Id).FirstAsync();
        var originalRatings = blog.Ratings.ToList();
        var originalCount = blog.Ratings.Count;

        try
        {
            // Add new ratings to the collection
            var newRatings = new List<int>(originalRatings) { 5, 4 };
            var newRatingsJson = System.Text.Json.JsonSerializer.Serialize(newRatings);

            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Blogs] SET [Ratings] = {0} WHERE [Id] = {1}",
                newRatingsJson, blog.Id);

            // Refresh the entity
            await ctx.Entry(blog).ReloadAsync();

            // Assert that the primitive collection is updated
            Assert.Equal(originalCount + 2, blog.Ratings.Count);
            Assert.Contains(5, blog.Ratings);
            Assert.Contains(4, blog.Ratings);
        }
        finally
        {
            // Cleanup
            var originalRatingsJson = System.Text.Json.JsonSerializer.Serialize(originalRatings);
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Blogs] SET [Ratings] = {0} WHERE [Id] = {1}",
                originalRatingsJson, blog.Id);
        }
    }

    [Fact]
    public async Task Test_PrimitiveCollections_Grids()
    {
        using var ctx = _fixture.CreateContext();

        var user = await ctx.Users.OrderBy(c => c.Id).FirstAsync();
        var originalIds = user.RelatedIds.ToList();

        try
        {
            // Add new GUIDs to the collection
            var newGuid1 = Guid.NewGuid();
            var newGuid2 = Guid.NewGuid();
            var newIds = new List<Guid>(originalIds) { newGuid1, newGuid2 };
            var newIdsJson = System.Text.Json.JsonSerializer.Serialize(newIds);

            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Users] SET [RelatedIds] = {0} WHERE [Id] = {1}",
                newIdsJson, user.Id);

            // Refresh the entity
            await ctx.Entry(user).ReloadAsync();

            // Assert that the primitive collection is updated
            Assert.Equal(originalIds.Count + 2, user.RelatedIds.Count);
            Assert.Contains(newGuid1, user.RelatedIds);
            Assert.Contains(newGuid2, user.RelatedIds);
        }
        finally
        {
            // Cleanup
            var originalIdsJson = System.Text.Json.JsonSerializer.Serialize(originalIds);
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Users] SET [RelatedIds] = {0} WHERE [Id] = {1}",
                originalIdsJson, user.Id);
        }
    }

    [Fact]
    public async Task Test_PrimitiveCollections_EmptyCollection()
    {
        using var ctx = _fixture.CreateContext();

        var product = await ctx.Products.OrderBy(c => c.Id).FirstAsync();
        var originalTags = product.Tags.ToList();

        try
        {
            // Set collection to empty
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Products] SET [Tags] = {0} WHERE [Id] = {1}",
                "[]", product.Id);

            // Refresh the entity
            await ctx.Entry(product).ReloadAsync();

            // Assert that the collection is now empty
            Assert.Empty(product.Tags);
        }
        finally
        {
            // Cleanup
            var originalTagsJson = System.Text.Json.JsonSerializer.Serialize(originalTags);
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Products] SET [Tags] = {0} WHERE [Id] = {1}",
                originalTagsJson, product.Id);
        }
    }

    public class PrimitiveCollectionsFixture : SharedStoreFixtureBase<PrimitiveCollectionsContext>
    {
        protected override string StoreName
            => "PrimitiveCollectionsRefreshFromDb";

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).EnableSensitiveDataLogging();

        protected override Task SeedAsync(PrimitiveCollectionsContext context)
        {
            var product1 = new Product
            {
                Name = "Laptop",
                Tags = ["Electronics", "Computer", "Portable"]
            };

            var product2 = new Product
            {
                Name = "Smartphone",
                Tags = ["Electronics", "Mobile", "Communication"]
            };

            var blog1 = new Blog
            {
                Title = "Tech Blog",
                Ratings = [5, 4, 5, 3, 4]
            };

            var blog2 = new Blog
            {
                Title = "Cooking Blog",
                Ratings = [4, 5, 4, 4, 5]
            };

            var user1 = new User
            {
                Name = "John Doe",
                RelatedIds = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]
            };

            var user2 = new User
            {
                Name = "Jane Smith",
                RelatedIds = [Guid.NewGuid(), Guid.NewGuid()]
            };

            context.Products.AddRange(product1, product2);
            context.Blogs.AddRange(blog1, blog2);
            context.Users.AddRange(user1, user2);

            return context.SaveChangesAsync();
        }
    }

    public class PrimitiveCollectionsContext : DbContext
    {
        public PrimitiveCollectionsContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Blog> Blogs { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);
                
                entity.Property(p => p.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                // Configure primitive collection for strings
                entity.PrimitiveCollection(p => p.Tags)
                    .ElementType().HasMaxLength(50);
            });

            modelBuilder.Entity<Blog>(entity =>
            {
                entity.HasKey(b => b.Id);
                
                entity.Property(b => b.Title)
                    .HasMaxLength(200)
                    .IsRequired();

                // Configure primitive collection for integers
                entity.PrimitiveCollection(b => b.Ratings);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                
                entity.Property(u => u.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                // Configure primitive collection for GUIDs
                entity.PrimitiveCollection(u => u.RelatedIds);
            });
        }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public List<string> Tags { get; set; } = [];
    }

    public class Blog
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public List<int> Ratings { get; set; } = [];
    }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public List<Guid> RelatedIds { get; set; } = [];
    }
}
