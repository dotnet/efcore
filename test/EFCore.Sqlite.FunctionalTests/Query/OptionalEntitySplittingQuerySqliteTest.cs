// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class OptionalEntitySplittingQuerySqliteTest : NonSharedModelTestBase, IClassFixture<NonSharedFixture>
{
    public OptionalEntitySplittingQuerySqliteTest(NonSharedFixture fixture)
        : base(fixture)
    {
    }

    protected override string NonSharedStoreName
        => "OptionalEntitySplittingQueryTest";

    protected override ITestStoreFactory NonSharedTestStoreFactory
        => SqliteTestStoreFactory.Instance;

    private static void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.Entity<Customer>(b =>
        {
            b.ToTable("Customers");
            b.Property(c => c.Id).ValueGeneratedNever();

            // A required fragment: always joined with INNER JOIN.
            b.SplitToTable("CustomerStats", t => t.Property(c => c.VisitCount));

            // An optional fragment: joined with LEFT JOIN, row may be absent.
            b.SplitToTable(
                "CustomerDetails", t =>
                {
                    t.IsOptional();
                    t.Property(c => c.Description);
                    t.Property(c => c.Score);
                });
        });

    private static async Task Seed(CustomerContext context)
    {
        context.Customers.AddRange(
            new Customer { Id = 1, Name = "Alice", VisitCount = 5 },
            new Customer { Id = 2, Name = "Bob", VisitCount = 3 },
            new Customer { Id = 3, Name = "Carol", VisitCount = 1 });
        await context.SaveChangesAsync();

        // Customer 1: optional fragment row present, with non-null values.
        await context.Database.ExecuteSqlRawAsync(
            "INSERT INTO CustomerDetails (Id, Description, Score) VALUES (1, 'Has details', 42)");

        // Customer 2: optional fragment row absent entirely.

        // Customer 3: optional fragment row present, but its payload is null.
        await context.Database.ExecuteSqlRawAsync(
            "INSERT INTO CustomerDetails (Id, Description, Score) VALUES (3, NULL, NULL)");
    }

    private async Task<ContextFactory<CustomerContext>> InitializeContextAsync()
        => await InitializeNonSharedTest<CustomerContext>(OnModelCreating, seed: Seed, shouldLogCategory: _ => true);

    [Fact]
    public async Task Missing_optional_fragment_row_still_returns_entity()
    {
        var contextFactory = await InitializeContextAsync();
        await using var context = contextFactory.CreateDbContext();

        var customers = await context.Customers.OrderBy(c => c.Id).ToListAsync();

        Assert.Equal(3, customers.Count);

        Assert.Equal("Has details", customers[0].Description);
        Assert.Equal(42, customers[0].Score);
        Assert.Equal(5, customers[0].VisitCount);

        // Absent row: entity still materializes, missing columns are null.
        Assert.Null(customers[1].Description);
        Assert.Null(customers[1].Score);
        Assert.Equal(3, customers[1].VisitCount);

        // Existing row with null payload: same shape as absent row.
        Assert.Null(customers[2].Description);
        Assert.Null(customers[2].Score);
        Assert.Equal(1, customers[2].VisitCount);
    }

    [Fact]
    public async Task Required_fragment_uses_inner_join_and_optional_fragment_uses_left_join()
    {
        var contextFactory = await InitializeContextAsync();
        await using var context = contextFactory.CreateDbContext();

        var queryString = context.Customers.ToQueryString();

        Assert.Contains("INNER JOIN \"CustomerStats\"", queryString);
        Assert.Contains("LEFT JOIN \"CustomerDetails\"", queryString);
    }

    [Fact]
    public async Task Predicate_on_optional_fragment_value()
    {
        var contextFactory = await InitializeContextAsync();
        await using var context = contextFactory.CreateDbContext();

        var customers = await context.Customers.Where(c => c.Description == "Has details").ToListAsync();

        Assert.Equal([1], customers.Select(c => c.Id));
    }

    [Fact]
    public async Task Predicate_comparing_optional_fragment_property_to_null_matches_absent_and_null_row()
    {
        var contextFactory = await InitializeContextAsync();
        await using var context = contextFactory.CreateDbContext();

        var customers = await context.Customers.Where(c => c.Description == null).OrderBy(c => c.Id).ToListAsync();

        Assert.Equal([2, 3], customers.Select(c => c.Id));
    }

    [Fact]
    public async Task Projection_containing_optional_fragment_properties()
    {
        var contextFactory = await InitializeContextAsync();
        await using var context = contextFactory.CreateDbContext();

        var results = await context.Customers.OrderBy(c => c.Id)
            .Select(c => new { c.Id, c.Description, c.Score })
            .ToListAsync();

        Assert.Equal(3, results.Count);
        Assert.Equal("Has details", results[0].Description);
        Assert.Null(results[1].Description);
        Assert.Null(results[2].Description);
    }

    [Fact]
    public async Task NoTracking_query_materializes_optional_fragment_correctly()
    {
        var contextFactory = await InitializeContextAsync();
        await using var context = contextFactory.CreateDbContext();

        var customers = await context.Customers.AsNoTracking().OrderBy(c => c.Id).ToListAsync();

        Assert.Equal("Has details", customers[0].Description);
        Assert.Null(customers[1].Description);
        Assert.Null(customers[2].Description);

        Assert.All(customers, c => Assert.Equal(EntityState.Detached, context.Entry(c).State));
    }

    [Fact]
    public async Task Tracking_query_materializes_optional_fragment_correctly()
    {
        var contextFactory = await InitializeContextAsync();
        await using var context = contextFactory.CreateDbContext();

        var customers = await context.Customers.OrderBy(c => c.Id).ToListAsync();

        Assert.All(customers, c => Assert.Equal(EntityState.Unchanged, context.Entry(c).State));
        Assert.Null(customers[1].Description);
    }

    protected class CustomerContext(DbContextOptions options) : PoolableDbContext(options)
    {
        public DbSet<Customer> Customers { get; set; }
    }

    protected class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int VisitCount { get; set; }
        public string Description { get; set; }
        public int? Score { get; set; }
    }
}
