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

    #region 38965

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task GroupBy_aggregate_over_required_navigation_with_query_filter(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context38965>(
            onConfiguring: b => b.ConfigureWarnings(
                w => w.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning)),
            seed: c => c.SeedAsync());
        using var context = contextFactory.CreateDbContext();

        var query = context.Dependents
            .GroupBy(d => d.GroupId)
            .Select(g => new
            {
                g.Key,
                Count = g.Count(),
                MaxValue = g.Max(d => (int?)d.Principal.Value)
            });

        var results = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Collection(
            results.OrderBy(e => e.Key),
            result =>
            {
                Assert.Equal(1, result.Key);
                Assert.Equal(1, result.Count);
                Assert.Equal(10, result.MaxValue);
            },
            result =>
            {
                Assert.Equal(2, result.Key);
                Assert.Equal(1, result.Count);
                Assert.Null(result.MaxValue);
            });

        var queryWithoutFilters = context.Dependents
            .IgnoreQueryFilters()
            .GroupBy(d => d.GroupId)
            .Select(g => new
            {
                g.Key,
                Count = g.Count(),
                MaxValue = g.Max(d => (int?)d.Principal.Value)
            });

        var resultsWithoutFilters = async
            ? await queryWithoutFilters.ToListAsync()
            : queryWithoutFilters.ToList();

        Assert.Collection(
            resultsWithoutFilters.OrderBy(e => e.Key),
            result =>
            {
                Assert.Equal(1, result.Key);
                Assert.Equal(1, result.Count);
                Assert.Equal(10, result.MaxValue);
            },
            result =>
            {
                Assert.Equal(2, result.Key);
                Assert.Equal(1, result.Count);
                Assert.Equal(20, result.MaxValue);
            });
    }

    protected class Context38965(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Principal38965> Principals
            => Set<Principal38965>();

        public DbSet<Dependent38965> Dependents
            => Set<Dependent38965>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Principal38965>().HasQueryFilter(p => !p.Filtered);

            modelBuilder.Entity<Dependent38965>()
                .HasOne(d => d.Principal)
                .WithMany()
                .HasForeignKey(d => d.PrincipalId)
                .IsRequired();
        }

        public Task SeedAsync()
        {
            Dependents.AddRange(
                new Dependent38965
                {
                    GroupId = 1,
                    Principal = new Principal38965 { Value = 10 }
                },
                new Dependent38965
                {
                    GroupId = 2,
                    Principal = new Principal38965
                    {
                        Value = 20,
                        Filtered = true
                    }
                });

            return SaveChangesAsync();
        }
    }

    protected class Principal38965
    {
        public int Id { get; set; }
        public int Value { get; set; }
        public bool Filtered { get; set; }
    }

    protected class Dependent38965
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int PrincipalId { get; set; }
        public Principal38965 Principal { get; set; } = null!;
    }
    #endregion
    #endregion
}
