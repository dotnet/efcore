// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class NonGeneratedMaterializerSqliteTest(NonSharedFixture fixture) : NonSharedModelTestBase(fixture), IClassFixture<NonSharedFixture>
{
    protected override string NonSharedStoreName
        => "NonGeneratedMaterializerTest";

    protected override ITestStoreFactory NonSharedTestStoreFactory
        => SqliteTestStoreFactory.Instance;

    [ConditionalFact]
    public async Task Simple_entity_materialization()
    {
        var contextFactory = await InitializeNonSharedTest<SimpleEntityContext>(
            seed: async ctx =>
            {
                ctx.AddRange(
                    new SimpleEntity { Id = 1, Name = "Alice", Age = 30, OptionalDescription = "Description 1" },
                    new SimpleEntity { Id = 2, Name = "Bob", Age = 25, OptionalDescription = null },
                    new SimpleEntity { Id = 3, Name = "Charlie", Age = 35, OptionalDescription = "Description 3" });
                await ctx.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var results = await context.SimpleEntities
            .AsNoTracking()
            .OrderBy(e => e.Id)
            .ToListAsync();

        Assert.Equal(3, results.Count);

        Assert.Equal(1, results[0].Id);
        Assert.Equal("Alice", results[0].Name);
        Assert.Equal(30, results[0].Age);
        Assert.Equal("Description 1", results[0].OptionalDescription);

        Assert.Equal(2, results[1].Id);
        Assert.Equal("Bob", results[1].Name);
        Assert.Equal(25, results[1].Age);
        Assert.Null(results[1].OptionalDescription);

        Assert.Equal(3, results[2].Id);
        Assert.Equal("Charlie", results[2].Name);
        Assert.Equal(35, results[2].Age);
        Assert.Equal("Description 3", results[2].OptionalDescription);
    }

    [ConditionalFact]
    public async Task Simple_entity_materialization_sync()
    {
        var contextFactory = await InitializeNonSharedTest<SimpleEntityContext>(
            seed: async ctx =>
            {
                ctx.AddRange(
                    new SimpleEntity { Id = 1, Name = "Alice", Age = 30, OptionalDescription = null });
                await ctx.SaveChangesAsync();
            });

        using var context = contextFactory.CreateDbContext();

        var results = context.SimpleEntities
            .AsNoTracking()
            .OrderBy(e => e.Id)
            .ToList();

        Assert.Single(results);
        Assert.Equal(1, results[0].Id);
        Assert.Equal("Alice", results[0].Name);
        Assert.Equal(30, results[0].Age);
        Assert.Null(results[0].OptionalDescription);
    }

    [ConditionalFact]
    public async Task Simple_entity_with_where_clause()
    {
        var contextFactory = await InitializeNonSharedTest<SimpleEntityContext>(
            seed: async ctx =>
            {
                ctx.AddRange(
                    new SimpleEntity { Id = 1, Name = "Alice", Age = 30, OptionalDescription = null },
                    new SimpleEntity { Id = 2, Name = "Bob", Age = 25, OptionalDescription = null });
                await ctx.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var results = await context.SimpleEntities
            .AsNoTracking()
            .Where(e => e.Name == "Bob")
            .ToListAsync();

        var result = Assert.Single(results);
        Assert.Equal(2, result.Id);
        Assert.Equal("Bob", result.Name);
        Assert.Equal(25, result.Age);
    }

    [ConditionalFact]
    public async Task Tracking_query_returns_tracked_entities()
    {
        var contextFactory = await InitializeNonSharedTest<SimpleEntityContext>(
            seed: async ctx =>
            {
                ctx.AddRange(
                    new SimpleEntity { Id = 1, Name = "Alice", Age = 30, OptionalDescription = null },
                    new SimpleEntity { Id = 2, Name = "Bob", Age = 25, OptionalDescription = "Desc" });
                await ctx.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var results = await context.SimpleEntities
            .OrderBy(e => e.Id)
            .ToListAsync();

        Assert.Equal(2, results.Count);
        Assert.Equal("Alice", results[0].Name);
        Assert.Equal("Bob", results[1].Name);

        // Verify entities are tracked
        Assert.Equal(2, context.ChangeTracker.Entries().Count());
        Assert.All(context.ChangeTracker.Entries(), e => Assert.Equal(EntityState.Unchanged, e.State));
    }

    [ConditionalFact]
    public async Task Tracking_query_identity_resolution()
    {
        var contextFactory = await InitializeNonSharedTest<SimpleEntityContext>(
            seed: async ctx =>
            {
                ctx.AddRange(
                    new SimpleEntity { Id = 1, Name = "Alice", Age = 30, OptionalDescription = null });
                await ctx.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        // Query the same entity twice — should get the same instance
        var firstResults = await context.SimpleEntities.ToListAsync();
        var secondResults = await context.SimpleEntities.ToListAsync();

        Assert.Same(Assert.Single(firstResults), Assert.Single(secondResults));
    }

    protected class SimpleEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int Age { get; set; }
        public string? OptionalDescription { get; set; }
    }

    protected class SimpleEntityContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<SimpleEntity> SimpleEntities
            => Set<SimpleEntity>();
    }

    #region TPH tests

    [ConditionalFact]
    public async Task TPH_materializes_correct_derived_types()
    {
        var contextFactory = await InitializeNonSharedTest<AnimalContext>(
            seed: async ctx =>
            {
                ctx.AddRange(
                    new Dog { Id = 1, Name = "Rex", Breed = "Labrador" },
                    new Cat { Id = 2, Name = "Whiskers", Indoor = true },
                    new Dog { Id = 3, Name = "Buddy", Breed = "Poodle" });
                await ctx.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var results = await context.Animals
            .AsNoTracking()
            .OrderBy(e => e.Id)
            .ToListAsync();

        Assert.Equal(3, results.Count);

        var rex = Assert.IsType<Dog>(results[0]);
        Assert.Equal("Rex", rex.Name);
        Assert.Equal("Labrador", rex.Breed);

        var whiskers = Assert.IsType<Cat>(results[1]);
        Assert.Equal("Whiskers", whiskers.Name);
        Assert.True(whiskers.Indoor);

        var buddy = Assert.IsType<Dog>(results[2]);
        Assert.Equal("Buddy", buddy.Name);
        Assert.Equal("Poodle", buddy.Breed);
    }

    [ConditionalFact]
    public async Task TPH_tracking_with_identity_resolution()
    {
        var contextFactory = await InitializeNonSharedTest<AnimalContext>(
            seed: async ctx =>
            {
                ctx.AddRange(
                    new Dog { Id = 1, Name = "Rex", Breed = "Labrador" },
                    new Cat { Id = 2, Name = "Whiskers", Indoor = true });
                await ctx.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var results = await context.Animals.OrderBy(e => e.Id).ToListAsync();

        Assert.Equal(2, results.Count);
        Assert.IsType<Dog>(results[0]);
        Assert.IsType<Cat>(results[1]);

        // Verify tracked
        Assert.Equal(2, context.ChangeTracker.Entries().Count());

        // Query again — identity resolution
        var secondQuery = await context.Animals.OrderBy(e => e.Id).ToListAsync();
        Assert.Same(results[0], secondQuery[0]);
        Assert.Same(results[1], secondQuery[1]);
    }

    protected class Animal
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }

    protected class Dog : Animal
    {
        public string Breed { get; set; } = null!;
    }

    protected class Cat : Animal
    {
        public bool Indoor { get; set; }
    }

    protected class AnimalContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Animal> Animals => Set<Animal>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Dog>();
            modelBuilder.Entity<Cat>();
        }
    }

    #endregion

    #region Reference Include tests

    [ConditionalFact]
    public async Task Reference_include_no_tracking()
    {
        var contextFactory = await InitializeNonSharedTest<BlogPostContext>(
            seed: async ctx =>
            {
                var blog = new Blog { Id = 1, Title = "EF Blog" };
                ctx.Posts.AddRange(
                    new Post { Id = 1, Title = "Post 1", Blog = blog },
                    new Post { Id = 2, Title = "Post 2", Blog = blog });
                await ctx.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var results = await context.Posts
            .Include(p => p.Blog)
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .ToListAsync();

        Assert.Equal(2, results.Count);

        Assert.Equal("Post 1", results[0].Title);
        Assert.NotNull(results[0].Blog);
        Assert.Equal("EF Blog", results[0].Blog!.Title);

        Assert.Equal("Post 2", results[1].Title);
        Assert.NotNull(results[1].Blog);
        Assert.Equal("EF Blog", results[1].Blog!.Title);
    }

    [ConditionalFact]
    public async Task Reference_include_tracking()
    {
        var contextFactory = await InitializeNonSharedTest<BlogPostContext>(
            seed: async ctx =>
            {
                var blog = new Blog { Id = 1, Title = "EF Blog" };
                ctx.Posts.AddRange(
                    new Post { Id = 1, Title = "Post 1", Blog = blog },
                    new Post { Id = 2, Title = "Post 2", Blog = blog });
                await ctx.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var results = await context.Posts
            .Include(p => p.Blog)
            .OrderBy(p => p.Id)
            .ToListAsync();

        Assert.Equal(2, results.Count);
        Assert.NotNull(results[0].Blog);
        Assert.NotNull(results[1].Blog);

        // Same Blog instance via identity resolution
        Assert.Same(results[0].Blog, results[1].Blog);

        // All tracked
        Assert.Equal(3, context.ChangeTracker.Entries().Count());
    }

    [ConditionalFact]
    public async Task Reference_include_null_related_entity()
    {
        var contextFactory = await InitializeNonSharedTest<OptionalBlogPostContext>(
            seed: async ctx =>
            {
                ctx.Posts.Add(new OptionalPost { Id = 1, Title = "Orphan Post" });
                await ctx.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var results = await context.Posts
            .Include(p => p.Blog)
            .AsNoTracking()
            .ToListAsync();

        var result = Assert.Single(results);

        Assert.Equal("Orphan Post", result.Title);
        Assert.Null(result.Blog);
    }

    protected class OptionalPost
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public int? BlogId { get; set; }
        public Blog? Blog { get; set; }
    }

    protected class OptionalBlogPostContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Blog> Blogs => Set<Blog>();
        public DbSet<OptionalPost> Posts => Set<OptionalPost>();
    }

    protected class Blog
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public List<Post> Posts { get; set; } = [];
    }

    protected class Post
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public int BlogId { get; set; }
        public Blog? Blog { get; set; }
    }

    protected class BlogPostContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Blog> Blogs => Set<Blog>();
        public DbSet<Post> Posts => Set<Post>();
    }

    #endregion

    #region Collection Include tests

    [ConditionalFact]
    public async Task Collection_include_no_tracking()
    {
        var contextFactory = await InitializeNonSharedTest<BlogPostContext>(
            seed: async ctx =>
            {
                var blog1 = new Blog { Id = 1, Title = "Blog 1" };
                var blog2 = new Blog { Id = 2, Title = "Blog 2" };
                ctx.Posts.AddRange(
                    new Post { Id = 1, Title = "Post 1", Blog = blog1 },
                    new Post { Id = 2, Title = "Post 2", Blog = blog1 },
                    new Post { Id = 3, Title = "Post 3", Blog = blog2 });
                await ctx.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var results = await context.Blogs
            .Include(b => b.Posts)
            .AsNoTracking()
            .OrderBy(b => b.Id)
            .ToListAsync();

        Assert.Equal(2, results.Count);

        Assert.Equal("Blog 1", results[0].Title);
        Assert.Equal(2, results[0].Posts.Count);
        Assert.Contains(results[0].Posts, p => p.Title == "Post 1");
        Assert.Contains(results[0].Posts, p => p.Title == "Post 2");

        Assert.Equal("Blog 2", results[1].Title);
        Assert.Single(results[1].Posts);
        Assert.Equal("Post 3", results[1].Posts[0].Title);
    }

    [ConditionalFact]
    public async Task Collection_include_tracking()
    {
        var contextFactory = await InitializeNonSharedTest<BlogPostContext>(
            seed: async ctx =>
            {
                var blog = new Blog { Id = 1, Title = "Blog 1" };
                ctx.Posts.AddRange(
                    new Post { Id = 1, Title = "Post 1", Blog = blog },
                    new Post { Id = 2, Title = "Post 2", Blog = blog });
                await ctx.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var results = await context.Blogs
            .Include(b => b.Posts)
            .ToListAsync();

        Assert.Single(results);
        Assert.Equal(2, results[0].Posts.Count);

        // All entities tracked
        Assert.Equal(3, context.ChangeTracker.Entries().Count());
    }

    [ConditionalFact]
    public async Task Collection_include_empty_collection()
    {
        var contextFactory = await InitializeNonSharedTest<BlogPostContext>(
            seed: async ctx =>
            {
                ctx.Blogs.Add(new Blog { Id = 1, Title = "Empty Blog" });
                await ctx.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var results = await context.Blogs
            .Include(b => b.Posts)
            .AsNoTracking()
            .ToListAsync();

        var result = Assert.Single(results);

        Assert.Equal("Empty Blog", result.Title);
        Assert.Empty(result.Posts);
    }

    #endregion

    #region Scalar projection tests

    [ConditionalFact]
    public async Task Scalar_projection_string()
    {
        var contextFactory = await InitializeNonSharedTest<SimpleEntityContext>(
            seed: async ctx =>
            {
                ctx.AddRange(
                    new SimpleEntity { Id = 1, Name = "Alice", Age = 30, OptionalDescription = null },
                    new SimpleEntity { Id = 2, Name = "Bob", Age = 25, OptionalDescription = null });
                await ctx.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var results = await context.SimpleEntities
            .OrderBy(e => e.Id)
            .Select(e => e.Name)
            .ToListAsync();

        Assert.Equal(["Alice", "Bob"], results);
    }

    [ConditionalFact]
    public async Task Scalar_projection_int()
    {
        var contextFactory = await InitializeNonSharedTest<SimpleEntityContext>(
            seed: async ctx =>
            {
                ctx.AddRange(
                    new SimpleEntity { Id = 1, Name = "Alice", Age = 30, OptionalDescription = null },
                    new SimpleEntity { Id = 2, Name = "Bob", Age = 25, OptionalDescription = null });
                await ctx.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var results = await context.SimpleEntities
            .OrderBy(e => e.Id)
            .Select(e => e.Age)
            .ToListAsync();

        Assert.Equal([30, 25], results);
    }

    #endregion

    #region Scalar aggregate tests

    [ConditionalFact]
    public async Task Max_int_async()
    {
        var contextFactory = await InitializeNonSharedTest<SimpleEntityContext>(
            seed: async ctx =>
            {
                ctx.AddRange(
                    new SimpleEntity { Id = 1, Name = "Alice", Age = 30, OptionalDescription = null },
                    new SimpleEntity { Id = 2, Name = "Bob", Age = 25, OptionalDescription = null });
                await ctx.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var maxAge = await context.SimpleEntities.MaxAsync(e => e.Age);

        Assert.Equal(30, maxAge);
    }

    [ConditionalFact]
    public async Task Max_int_sync()
    {
        var contextFactory = await InitializeNonSharedTest<SimpleEntityContext>(
            seed: async ctx =>
            {
                ctx.AddRange(
                    new SimpleEntity { Id = 1, Name = "Alice", Age = 30, OptionalDescription = null },
                    new SimpleEntity { Id = 2, Name = "Bob", Age = 25, OptionalDescription = null });
                await ctx.SaveChangesAsync();
            });

        using var context = contextFactory.CreateDbContext();

        var maxAge = context.SimpleEntities.Max(e => e.Age);

        Assert.Equal(30, maxAge);
    }

    [ConditionalFact]
    public async Task Count_async()
    {
        var contextFactory = await InitializeNonSharedTest<SimpleEntityContext>(
            seed: async ctx =>
            {
                ctx.AddRange(
                    new SimpleEntity { Id = 1, Name = "Alice", Age = 30, OptionalDescription = null },
                    new SimpleEntity { Id = 2, Name = "Bob", Age = 25, OptionalDescription = null });
                await ctx.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var count = await context.SimpleEntities.CountAsync();

        Assert.Equal(2, count);
    }

    [ConditionalFact]
    public async Task SingleAsync_entity()
    {
        var contextFactory = await InitializeNonSharedTest<SimpleEntityContext>(
            seed: async ctx =>
            {
                ctx.AddRange(
                    new SimpleEntity { Id = 1, Name = "Alice", Age = 30, OptionalDescription = null });
                await ctx.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var entity = await context.SimpleEntities.SingleAsync();

        Assert.Equal("Alice", entity.Name);
    }

    #endregion

    #region Anonymous type projection tests

    [ConditionalFact]
    public async Task Anonymous_type_projection_two_scalars()
    {
        var contextFactory = await InitializeNonSharedTest<SimpleEntityContext>(
            seed: async ctx =>
            {
                ctx.AddRange(
                    new SimpleEntity { Id = 1, Name = "Alice", Age = 30, OptionalDescription = null },
                    new SimpleEntity { Id = 2, Name = "Bob", Age = 25, OptionalDescription = null });
                await ctx.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var results = await context.SimpleEntities
            .OrderBy(e => e.Id)
            .Select(e => new { e.Id, e.Name })
            .ToListAsync();

        Assert.Equal(2, results.Count);
        Assert.Equal(1, results[0].Id);
        Assert.Equal("Alice", results[0].Name);
        Assert.Equal(2, results[1].Id);
        Assert.Equal("Bob", results[1].Name);
    }

    [ConditionalFact]
    public async Task Anonymous_type_projection_with_nullable_property()
    {
        var contextFactory = await InitializeNonSharedTest<SimpleEntityContext>(
            seed: async ctx =>
            {
                ctx.AddRange(
                    new SimpleEntity { Id = 1, Name = "Alice", Age = 30, OptionalDescription = "Desc1" },
                    new SimpleEntity { Id = 2, Name = "Bob", Age = 25, OptionalDescription = null });
                await ctx.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var results = await context.SimpleEntities
            .OrderBy(e => e.Id)
            .Select(e => new { e.Name, e.OptionalDescription })
            .ToListAsync();

        Assert.Equal(2, results.Count);
        Assert.Equal("Alice", results[0].Name);
        Assert.Equal("Desc1", results[0].OptionalDescription);
        Assert.Equal("Bob", results[1].Name);
        Assert.Null(results[1].OptionalDescription);
    }

    [ConditionalFact]
    public async Task Anonymous_type_projection_mixed_value_and_reference_types()
    {
        var contextFactory = await InitializeNonSharedTest<SimpleEntityContext>(
            seed: async ctx =>
            {
                ctx.AddRange(
                    new SimpleEntity { Id = 1, Name = "Alice", Age = 30, OptionalDescription = null },
                    new SimpleEntity { Id = 2, Name = "Bob", Age = 25, OptionalDescription = null });
                await ctx.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var results = await context.SimpleEntities
            .OrderBy(e => e.Id)
            .Select(e => new { e.Id, e.Name, e.Age })
            .ToListAsync();

        Assert.Equal(2, results.Count);
        Assert.Equal(1, results[0].Id);
        Assert.Equal("Alice", results[0].Name);
        Assert.Equal(30, results[0].Age);
        Assert.Equal(2, results[1].Id);
        Assert.Equal("Bob", results[1].Name);
        Assert.Equal(25, results[1].Age);
    }

    [ConditionalFact]
    public async Task Anonymous_type_projection_with_entity()
    {
        var contextFactory = await InitializeNonSharedTest<BlogPostContext>(
            seed: async ctx =>
            {
                var blog1 = new Blog { Id = 1, Title = "Blog 1" };
                var blog2 = new Blog { Id = 2, Title = "Blog 2" };
                ctx.Posts.AddRange(
                    new Post { Id = 1, Title = "Post 1", Blog = blog1 },
                    new Post { Id = 2, Title = "Post 2", Blog = blog2 });
                await ctx.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var results = await context.Posts
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .Select(p => new { p.Title, p.Blog })
            .ToListAsync();

        Assert.Equal(2, results.Count);
        Assert.Equal("Post 1", results[0].Title);
        Assert.NotNull(results[0].Blog);
        Assert.Equal("Blog 1", results[0].Blog!.Title);
        Assert.Equal("Post 2", results[1].Title);
        Assert.NotNull(results[1].Blog);
        Assert.Equal("Blog 2", results[1].Blog!.Title);
    }

    [ConditionalFact]
    public async Task Anonymous_type_projection_nested()
    {
        var contextFactory = await InitializeNonSharedTest<BlogPostContext>(
            seed: async ctx =>
            {
                var blog = new Blog { Id = 1, Title = "Blog 1" };
                ctx.Posts.AddRange(
                    new Post { Id = 1, Title = "Post 1", Blog = blog },
                    new Post { Id = 2, Title = "Post 2", Blog = blog });
                await ctx.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var results = await context.Posts
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .Select(p => new { Outer = new { p.Id, p.Title }, p.BlogId })
            .ToListAsync();

        Assert.Equal(2, results.Count);
        Assert.Equal(1, results[0].Outer.Id);
        Assert.Equal("Post 1", results[0].Outer.Title);
        Assert.Equal(1, results[0].BlogId);
        Assert.Equal(2, results[1].Outer.Id);
        Assert.Equal("Post 2", results[1].Outer.Title);
        Assert.Equal(1, results[1].BlogId);
    }

    [ConditionalFact]
    public async Task Anonymous_type_projection_nested_anonymous_type()
    {
        var contextFactory = await InitializeNonSharedTest<BlogPostContext>(
            seed: async ctx =>
            {
                var blog = new Blog { Id = 1, Title = "Blog 1" };
                ctx.Posts.AddRange(
                    new Post { Id = 1, Title = "Post 1", Blog = blog },
                    new Post { Id = 2, Title = "Post 2", Blog = blog });
                await ctx.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var results = await context.Posts
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .Select(p => new { p.Id, Inner = new { p.Title, p.BlogId } })
            .ToListAsync();

        Assert.Equal(2, results.Count);
        Assert.Equal(1, results[0].Id);
        Assert.Equal("Post 1", results[0].Inner.Title);
        Assert.Equal(1, results[0].Inner.BlogId);
        Assert.Equal(2, results[1].Id);
        Assert.Equal("Post 2", results[1].Inner.Title);
        Assert.Equal(1, results[1].Inner.BlogId);
    }

    #endregion

    #region Value converter tests

    [ConditionalFact]
    public async Task Entity_with_value_converter_string_to_enum()
    {
        var contextFactory = await InitializeNonSharedTest<ValueConverterContext>(
            seed: async ctx =>
            {
                ctx.AddRange(
                    new OrderEntity { Id = 1, Description = "First order", Status = OrderStatus.Pending },
                    new OrderEntity { Id = 2, Description = "Second order", Status = OrderStatus.Shipped },
                    new OrderEntity { Id = 3, Description = "Third order", Status = OrderStatus.Delivered });
                await ctx.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var results = await context.Orders
            .AsNoTracking()
            .OrderBy(e => e.Id)
            .ToListAsync();

        Assert.Equal(3, results.Count);

        Assert.Equal(1, results[0].Id);
        Assert.Equal("First order", results[0].Description);
        Assert.Equal(OrderStatus.Pending, results[0].Status);

        Assert.Equal(2, results[1].Id);
        Assert.Equal("Second order", results[1].Description);
        Assert.Equal(OrderStatus.Shipped, results[1].Status);

        Assert.Equal(3, results[2].Id);
        Assert.Equal("Third order", results[2].Description);
        Assert.Equal(OrderStatus.Delivered, results[2].Status);
    }

    [ConditionalFact]
    public async Task Entity_with_value_converter_nullable_property()
    {
        var contextFactory = await InitializeNonSharedTest<ValueConverterContext>(
            seed: async ctx =>
            {
                ctx.AddRange(
                    new OrderEntity { Id = 1, Description = "First", Status = OrderStatus.Pending, CancelledStatus = OrderStatus.Pending },
                    new OrderEntity { Id = 2, Description = "Second", Status = OrderStatus.Shipped, CancelledStatus = null });
                await ctx.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var results = await context.Orders
            .AsNoTracking()
            .OrderBy(e => e.Id)
            .ToListAsync();

        Assert.Equal(2, results.Count);

        Assert.Equal(OrderStatus.Pending, results[0].CancelledStatus);
        Assert.Null(results[1].CancelledStatus);
    }

    [ConditionalFact]
    public async Task Entity_with_value_converter_tracked()
    {
        var contextFactory = await InitializeNonSharedTest<ValueConverterContext>(
            seed: async ctx =>
            {
                ctx.AddRange(
                    new OrderEntity { Id = 1, Description = "First", Status = OrderStatus.Shipped });
                await ctx.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var entity = await context.Orders.SingleAsync();

        Assert.Equal(OrderStatus.Shipped, entity.Status);
        Assert.Equal(EntityState.Unchanged, context.Entry(entity).State);
    }

    protected enum OrderStatus
    {
        Pending,
        Shipped,
        Delivered
    }

    protected class OrderEntity
    {
        public int Id { get; set; }
        public string Description { get; set; } = null!;
        public OrderStatus Status { get; set; }
        public OrderStatus? CancelledStatus { get; set; }
    }

    protected class ValueConverterContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<OrderEntity> Orders => Set<OrderEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderEntity>(b =>
            {
                b.Property(e => e.Status)
                    .HasConversion<string>();
                b.Property(e => e.CancelledStatus)
                    .HasConversion<string>();
            });
        }
    }

    #endregion
}
