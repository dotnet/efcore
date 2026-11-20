// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexJson;

public class ComplexJsonMiscellaneousSqliteTest(ComplexJsonSqliteFixture fixture, ITestOutputHelper testOutputHelper)
    : ComplexJsonMiscellaneousRelationalTestBase<ComplexJsonSqliteFixture>(fixture, testOutputHelper)
{
    public override async Task FromSql_on_root()
    {
        await base.FromSql_on_root();

        AssertSql(
            """
SELECT "m"."Id", "m"."Name", "m"."AssociateCollection", "m"."OptionalAssociate", "m"."RequiredAssociate"
FROM (
    SELECT * FROM "RootEntity"
) AS "m"
""");
    }

    public override async Task FromSql_on_root_composed(bool async)
    {
        await base.FromSql_on_root_composed(async);

        AssertSql(
            """
SELECT "m"."Id", "m"."Name", "m"."AssociateCollection", "m"."OptionalAssociate", "m"."RequiredAssociate"
FROM (
    SELECT * FROM "RootEntity"
) AS "m"
WHERE "m"."RequiredAssociate" ->> 'Int' = 8
""");
    }

    [Fact]
    public virtual async Task Query_json_complex_struct_with_init_only_nested_struct()
    {
        await using var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<Issue39702Context>()
            .UseSqlite(connection)
            .Options;

        await using (var context = new Issue39702Context(options))
        {
            await context.Database.EnsureCreatedAsync();
            context.Add(
                new Issue39702Blog
                {
                    Price = new Issue39702Price { Amount = new Issue39702Amount { Value = 1.5m } },
                });
            await context.SaveChangesAsync();
        }

        await using (var context = new Issue39702Context(options))
        {
            var tracked = await context.Blogs.SingleAsync();
            Assert.Equal(1.5m, tracked.Price.Amount.Value);

            var untracked = await context.Blogs.AsNoTracking().SingleAsync();
            Assert.Equal(1.5m, untracked.Price.Amount.Value);
        }
    }

    private sealed class Issue39702Context(DbContextOptions<Issue39702Context> options) : DbContext(options)
    {
        public DbSet<Issue39702Blog> Blogs => Set<Issue39702Blog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Issue39702Blog>().ComplexProperty(b => b.Price, p => p.ToJson());
    }

    private sealed class Issue39702Blog
    {
        public int Id { get; set; }
        public required Issue39702Price Price { get; init; }
    }

    private readonly record struct Issue39702Price
    {
        public required Issue39702Amount Amount { get; init; }
    }

    private readonly record struct Issue39702Amount
    {
        public required decimal Value { get; init; }
    }
}
