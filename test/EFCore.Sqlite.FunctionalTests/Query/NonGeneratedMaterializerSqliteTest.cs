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

        var result = await context.SimpleEntities
            .AsNoTracking()
            .Where(e => e.Name == "Bob")
            .SingleAsync();

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
        var first = await context.SimpleEntities.SingleAsync();
        var second = await context.SimpleEntities.SingleAsync();

        Assert.Same(first, second);
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
}
