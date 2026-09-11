// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class AdHocQueryFiltersQueryRelationalTestBase(NonSharedFixture fixture) : AdHocQueryFiltersQueryTestBase(fixture)
{
    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected void ClearLog()
        => TestSqlLoggerFactory.Clear();

    protected void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    #region 38700

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Query_filter_with_inline_collection_of_navigation_column(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context38700>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateDbContext();

        Context38700.AuthorizedServiceIds = [10];

        var query = context.Children.AsNoTracking().Select(c => c.Label);

        var results = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Equal(["ok"], results);
    }

    protected class Context38700(DbContextOptions options) : DbContext(options)
    {
        public static List<int> AuthorizedServiceIds { get; set; } = [];

        public DbSet<Parent38700> Parents
            => Set<Parent38700>();

        public DbSet<Child38700> Children
            => Set<Child38700>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Parent38700>().HasQueryFilter(
                p => AuthorizedServiceIds.Contains(p.ServiceId));

            // Inline array of a navigation column — triggers VALUES pruning of outer join columns (#38700).
            modelBuilder.Entity<Child38700>().HasQueryFilter(c =>
                new int?[] { c.Parent.ServiceId }
                    .Any(id => id.HasValue && AuthorizedServiceIds.Contains(id.Value)));
        }

        public Task SeedAsync()
        {
            var parent = new Parent38700 { ServiceId = 10 };
            Children.Add(new Child38700 { Parent = parent, Label = "ok" });
            return SaveChangesAsync();
        }
    }

    protected class Parent38700
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
    }

    protected class Child38700
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public Parent38700 Parent { get; set; } = null!;
        public string Label { get; set; } = null!;
    }

    #endregion
}
