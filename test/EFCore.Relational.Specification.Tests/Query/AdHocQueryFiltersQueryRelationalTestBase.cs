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

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task GroupBy_aggregates_of_every_kind_over_required_navigation_with_query_filter(bool async)
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
                Sum = g.Sum(d => (int?)d.Principal.Value),
                Average = g.Average(d => (double?)d.Principal.Value),
                Min = g.Min(d => (int?)d.Principal.Value),
                Large = g.Count(d => d.Principal.Value > 15),
                AnyLarge = g.Any(d => d.Principal.Value > 15)
            });

        var results = async
            ? await query.ToListAsync()
            : query.ToList();

        // Every aggregate kind folds the filtered-out principal away as an empty aggregate rather than as a
        // missing row. These are the values the correlated subquery translation produces, the coalesce to 0
        // for SUM included.
        Assert.Collection(
            results.OrderBy(e => e.Key),
            result =>
            {
                Assert.Equal(1, result.Key);
                Assert.Equal(1, result.Count);
                Assert.Equal(10, result.Sum);
                Assert.Equal(10d, result.Average);
                Assert.Equal(10, result.Min);
                Assert.Equal(0, result.Large);
                Assert.False(result.AnyLarge);
            },
            result =>
            {
                Assert.Equal(2, result.Key);
                Assert.Equal(1, result.Count);
                Assert.Equal(0, result.Sum);
                Assert.Null(result.Average);
                Assert.Null(result.Min);
                Assert.Equal(0, result.Large);
                Assert.False(result.AnyLarge);
            });
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task GroupBy_All_over_required_navigation_with_query_filter(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context38965>(
            onConfiguring: b => b.ConfigureWarnings(
                w => w.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning)),
            seed: c => c.SeedAsync());
        using var context = contextFactory.CreateDbContext();

        var query = context.Dependents
            .GroupBy(d => d.GroupId)
            .Select(g => new { g.Key, AllLarge = g.All(d => d.Principal.Value > 15) });

        var results = async
            ? await query.ToListAsync()
            : query.ToList();

        // All() over an empty set is true, so a group whose only rows have a filtered-out principal answers
        // true. The lifted form reaches the same answer because the row the outer join keeps for that
        // principal is excluded from the aggregate.
        Assert.Collection(
            results.OrderBy(e => e.Key),
            result =>
            {
                Assert.Equal(1, result.Key);
                Assert.False(result.AllLarge);
            },
            result =>
            {
                Assert.Equal(2, result.Key);
                Assert.True(result.AllLarge);
            });
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task GroupBy_aggregate_over_required_navigation_keeps_the_principals_own_filter_exact(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context38965_FilterThroughNavigation>(
            onConfiguring: b => b.ConfigureWarnings(
                w => w.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning)),
            seed: c => c.SeedAsync());
        using var context = contextFactory.CreateDbContext();

        var query = context.Set<CategorizedDependent38965>()
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

        // The principal carries a filter that reaches through a required navigation of its own and is
        // null-tolerant. Relaxing that join as well would let a principal in a deleted category pass its own
        // filter, so the outer join stays confined to the aggregate's own traversal.
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
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task GroupBy_aggregates_over_filtered_and_unfiltered_required_navigations(bool async)
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
                MaxValue = g.Max(d => (int?)d.Principal.Value),
                MaxUnfiltered = g.Max(d => (int?)d.Unfiltered.Value)
            });

        var results = async
            ? await query.ToListAsync()
            : query.ToList();

        // Only the filtered principal needs the outer join; the unfiltered one keeps its inner join, and
        // neither loses a group.
        Assert.Collection(
            results.OrderBy(e => e.Key),
            result =>
            {
                Assert.Equal(1, result.Key);
                Assert.Equal(1, result.Count);
                Assert.Equal(10, result.MaxValue);
                Assert.Equal(100, result.MaxUnfiltered);
            },
            result =>
            {
                Assert.Equal(2, result.Key);
                Assert.Equal(1, result.Count);
                Assert.Null(result.MaxValue);
                Assert.Equal(200, result.MaxUnfiltered);
            });
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task GroupBy_aggregate_over_optional_navigation_with_query_filter(bool async)
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
                MaxOptional = g.Max(d => (int?)d.OptionalPrincipal!.Value)
            });

        var results = async
            ? await query.ToListAsync()
            : query.ToList();

        // An optional navigation already joined as an outer join, so the filter could never remove a row
        // here; this pins that the lift still holds every group for that shape.
        Assert.Collection(
            results.OrderBy(e => e.Key),
            result =>
            {
                Assert.Equal(1, result.Key);
                Assert.Equal(1, result.Count);
                Assert.Equal(30, result.MaxOptional);
            },
            result =>
            {
                Assert.Equal(2, result.Key);
                Assert.Equal(1, result.Count);
                Assert.Null(result.MaxOptional);
            });
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task GroupBy_key_over_filtered_required_navigation_removes_the_rows(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context38965>(
            onConfiguring: b => b.ConfigureWarnings(
                w => w.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning)),
            seed: c => c.SeedAsync());
        using var context = contextFactory.CreateDbContext();

        var query = context.Dependents
            .GroupBy(d => d.Principal.Value)
            .Select(g => new
            {
                g.Key,
                Count = g.Count(),
                MaxValue = g.Max(d => (int?)d.Principal.Value)
            });

        var results = async
            ? await query.ToListAsync()
            : query.ToList();

        // Grouping *by* a filtered principal's column has always removed those rows - the key cannot be
        // formed without the principal - so the row with the filtered principal does not reach the grouping
        // and forms no group. Only the aggregate's own traversal is made row-preserving.
        Assert.Collection(
            results.OrderBy(e => e.Key),
            result =>
            {
                Assert.Equal(10, result.Key);
                Assert.Equal(1, result.Count);
                Assert.Equal(10, result.MaxValue);
            });
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task GroupBy_aggregate_over_chain_with_query_filter_on_the_far_principal(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context38965_Chain>(
            onConfiguring: b => b.ConfigureWarnings(
                w => w.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning)),
            seed: c => c.SeedAsync());
        using var context = contextFactory.CreateDbContext();

        var query = context.Set<ChainDependent38965>()
            .GroupBy(d => d.GroupId)
            .Select(g => new
            {
                g.Key,
                Count = g.Count(),
                MaxValue = g.Max(d => (int?)d.Middle.Leaf.Value)
            });

        var results = async
            ? await query.ToListAsync()
            : query.ToList();

        // The filter is two hops out, on the far end of the chain; the unfiltered hop in between keeps its
        // inner join and only the filtered one is relaxed.
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
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task GroupBy_aggregate_over_self_referencing_navigation_with_query_filter(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context38965_SelfReference>(
            onConfiguring: b => b.ConfigureWarnings(
                w => w.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning)),
            seed: c => c.SeedAsync());
        using var context = contextFactory.CreateDbContext();

        var query = context.Set<Employee38965>()
            .GroupBy(e => e.DepartmentId)
            .Select(g => new
            {
                g.Key,
                Count = g.Count(),
                MaxManagerSalary = g.Max(e => (int?)e.Manager.Salary)
            });

        var results = async
            ? await query.ToListAsync()
            : query.ToList();

        // The filter applies to the principal and the dependent alike, since they are the same entity type:
        // the deleted employee is gone from the grouping source, and the employee it manages keeps its group
        // with no manager salary.
        Assert.Collection(
            results.OrderBy(e => e.Key),
            result =>
            {
                Assert.Equal(1, result.Key);
                Assert.Equal(1, result.Count);
                Assert.Equal(100, result.MaxManagerSalary);
            },
            result =>
            {
                Assert.Equal(2, result.Key);
                Assert.Equal(1, result.Count);
                Assert.Null(result.MaxManagerSalary);
            });
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_aggregate_over_required_navigation_to_TPH_principal_with_query_filter(bool async)
        => AssertInheritedPrincipalKeepsItsGroups<Context38965_Tph>(async);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_aggregate_over_required_navigation_to_TPT_principal_with_query_filter(bool async)
        => AssertInheritedPrincipalKeepsItsGroups<Context38965_Tpt>(async);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_aggregate_over_required_navigation_to_TPC_principal_with_query_filter(bool async)
        => AssertInheritedPrincipalKeepsItsGroups<Context38965_Tpc>(async);

    private async Task AssertInheritedPrincipalKeepsItsGroups<TContext>(bool async)
        where TContext : InheritanceContext38965
    {
        var contextFactory = await InitializeNonSharedTest<TContext>(
            onConfiguring: b => b.ConfigureWarnings(
                w => w.Ignore(
                    CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning,
                    // TPC cannot represent a foreign key targeting the root of the hierarchy; the query
                    // behaviour under test does not depend on the constraint existing.
                    RelationalEventId.ForeignKeyTpcPrincipalWarning)),
            seed: c => c.SeedAsync());
        using var context = contextFactory.CreateDbContext();

        var query = context.Set<InheritanceDependent38965>()
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

        // The filter is declared on the root of the hierarchy, whatever the mapping strategy, so the group
        // whose principal is filtered out survives with an empty aggregate rather than disappearing.
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
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task GroupBy_aggregates_sharing_one_filtered_navigation(bool async)
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
                MaxValue = g.Max(d => (int?)d.Principal.Value),
                MinValue = g.Min(d => (int?)d.Principal.Value)
            });

        var results = async
            ? await query.ToListAsync()
            : query.ToList();

        // Two aggregates over the same navigation share a single join rather than joining the principal
        // twice - see the SQL baselines.
        Assert.Collection(
            results.OrderBy(e => e.Key),
            result =>
            {
                Assert.Equal(1, result.Key);
                Assert.Equal(10, result.MaxValue);
                Assert.Equal(10, result.MinValue);
            },
            result =>
            {
                Assert.Equal(2, result.Key);
                Assert.Null(result.MaxValue);
                Assert.Null(result.MinValue);
            });
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task GroupBy_aggregate_over_filtered_navigation_after_Take(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context38965>(
            onConfiguring: b => b.ConfigureWarnings(
                w => w.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning)),
            seed: c => c.SeedAsync());
        using var context = contextFactory.CreateDbContext();

        var query = context.Dependents
            .OrderBy(d => d.GroupId)
            .Take(2)
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

        // The join belongs on top of the composed source: it must not be pushed underneath Take, which
        // would change which rows are taken before they are grouped.
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
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task GroupBy_aggregate_over_required_navigation_with_null_observing_selector(bool async)
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
                SumOrFive = g.Sum(d => ((int?)d.Principal.Value) ?? 5),
                CountOfZero = g.Count(d => (((int?)d.Principal.Value) ?? 0) == 0),
                AnyZero = g.Any(d => (((int?)d.Principal.Value) ?? 0) == 0)
            });

        var results = async
            ? await query.ToListAsync()
            : query.ToList();

        // A selector or predicate which turns null into a value must not see the row the outer join keeps
        // for a filtered-out principal: the correlated subquery had no row there at all, so the group whose
        // principal is filtered out sums to 0 rather than 5, and matches nothing.
        Assert.Collection(
            results.OrderBy(e => e.Key),
            result =>
            {
                Assert.Equal(1, result.Key);
                Assert.Equal(10, result.SumOrFive);
                Assert.Equal(0, result.CountOfZero);
                Assert.False(result.AnyZero);
            },
            result =>
            {
                Assert.Equal(2, result.Key);
                Assert.Equal(0, result.SumOrFive);
                Assert.Equal(0, result.CountOfZero);
                Assert.False(result.AnyZero);
            });
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task GroupBy_aggregate_reaching_the_principal_through_EF_Property(bool async)
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
                MaxValue = g.Max(d => (int?)d.Principal.Value),
                SumOrFive = g.Sum(d => EF.Property<int?>(d.Principal, "Value") ?? 5)
            });

        var results = async
            ? await query.ToListAsync()
            : query.ToList();

        // The second selector reaches the principal through EF.Property rather than a member chain. The
        // first one is what turns the outer join on, and the second has to be guarded all the same - the
        // guard follows the join, not the shape of the selector.
        Assert.Collection(
            results.OrderBy(e => e.Key),
            result =>
            {
                Assert.Equal(1, result.Key);
                Assert.Equal(10, result.MaxValue);
                Assert.Equal(10, result.SumOrFive);
            },
            result =>
            {
                Assert.Equal(2, result.Key);
                Assert.Null(result.MaxValue);
                Assert.Equal(0, result.SumOrFive);
            });
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task GroupBy_aggregate_over_chain_with_two_filtered_principals(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context38965_ChainBothFiltered>(
            onConfiguring: b => b.ConfigureWarnings(
                w => w.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning)),
            seed: c => c.SeedAsync());
        using var context = contextFactory.CreateDbContext();

        var query = context.Set<ChainDependent38965>()
            .GroupBy(d => d.GroupId)
            .Select(g => new
            {
                g.Key,
                SumOrFive = g.Sum(d => ((int?)d.Middle.Leaf.Value) ?? 5),
                AllBig = g.All(d => d.Middle.Leaf.Value > 100)
            });

        var results = async
            ? await query.ToListAsync()
            : query.ToList();

        // Both hops are filtered and both join as outer joins, so the guard has to cover both: in group 2
        // the middle survives its filter and the leaf does not, and the row the leaf's join keeps must stay
        // invisible to either aggregate.
        Assert.Collection(
            results.OrderBy(e => e.Key),
            result =>
            {
                Assert.Equal(1, result.Key);
                Assert.Equal(10, result.SumOrFive);
                Assert.False(result.AllBig);
            },
            result =>
            {
                Assert.Equal(2, result.Key);
                Assert.Equal(0, result.SumOrFive);
                Assert.True(result.AllBig);
            });
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task GroupBy_aggregate_over_filtered_principal_lifted_by_an_unfiltered_sibling(bool async)
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
                MaxUnfiltered = g.Max(d => (int?)d.Unfiltered.Value),
                SumOrFive = g.Sum(d => EF.Property<int?>(d.Principal, "Value") ?? 5)
            });

        var results = async
            ? await query.ToListAsync()
            : query.ToList();

        // Nothing here reaches a filtered principal through a member chain: the lift is enabled by an
        // aggregate over an unfiltered navigation, and the filtered one is reached through EF.Property. The
        // join to it still has to be relaxed, or the group disappears exactly as in #38965.
        Assert.Collection(
            results.OrderBy(e => e.Key),
            result =>
            {
                Assert.Equal(1, result.Key);
                Assert.Equal(1, result.Count);
                Assert.Equal(100, result.MaxUnfiltered);
                Assert.Equal(10, result.SumOrFive);
            },
            result =>
            {
                Assert.Equal(2, result.Key);
                Assert.Equal(1, result.Count);
                Assert.Equal(200, result.MaxUnfiltered);
                Assert.Equal(0, result.SumOrFive);
            });
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task GroupBy_aggregate_over_filtered_principal_behind_an_optional_navigation(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context38965_OptionalThenFiltered>(
            onConfiguring: b => b.ConfigureWarnings(
                w => w.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning)),
            seed: c => c.SeedAsync());
        using var context = contextFactory.CreateDbContext();

        var query = context.Set<OptionalChainDependent38965>()
            .GroupBy(d => d.GroupId)
            .Select(g => new { g.Key, SumOrFive = g.Sum(d => ((int?)d.Middle!.Leaf.Value) ?? 5) });

        var results = async
            ? await query.ToListAsync()
            : query.ToList();

        // The leaf sits behind an ordinary optional navigation, so its join is outer whatever this handling
        // does, and the null it contributes is visible to every other translation of the same query. Leaving
        // it unguarded keeps this answer the same as the unlifted one - 5, not 0.
        Assert.Collection(
            results.OrderBy(e => e.Key),
            result =>
            {
                Assert.Equal(1, result.Key);
                Assert.Equal(10, result.SumOrFive);
            },
            result =>
            {
                Assert.Equal(2, result.Key);
                Assert.Equal(5, result.SumOrFive);
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

            modelBuilder.Entity<Dependent38965>(
                b =>
                {
                    b.HasOne(d => d.Principal)
                        .WithMany()
                        .HasForeignKey(d => d.PrincipalId)
                        .IsRequired();

                    // A second required principal, this one unfiltered, and an optional filtered one.
                    b.HasOne(d => d.Unfiltered)
                        .WithMany()
                        .HasForeignKey(d => d.UnfilteredId)
                        .IsRequired();

                    b.HasOne(d => d.OptionalPrincipal)
                        .WithMany()
                        .HasForeignKey(d => d.OptionalPrincipalId);
                });
        }

        public Task SeedAsync()
        {
            Dependents.AddRange(
                new Dependent38965
                {
                    GroupId = 1,
                    Principal = new Principal38965 { Value = 10 },
                    Unfiltered = new Unfiltered38965 { Value = 100 },
                    OptionalPrincipal = new Principal38965 { Value = 30 }
                },
                new Dependent38965
                {
                    GroupId = 2,
                    Principal = new Principal38965
                    {
                        Value = 20,
                        Filtered = true
                    },
                    Unfiltered = new Unfiltered38965 { Value = 200 },
                    OptionalPrincipal = new Principal38965
                    {
                        Value = 40,
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
        public int UnfilteredId { get; set; }
        public Unfiltered38965 Unfiltered { get; set; } = null!;
        public int? OptionalPrincipalId { get; set; }
        public Principal38965? OptionalPrincipal { get; set; }
    }

    protected class Unfiltered38965
    {
        public int Id { get; set; }
        public int Value { get; set; }
    }

    protected class Context38965_FilterThroughNavigation(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category38965>().HasQueryFilter(c => c.DeletedOn == null);

            modelBuilder.Entity<CategorizedPrincipal38965>(
                b =>
                {
                    b.HasOne(p => p.Category).WithMany().HasForeignKey(p => p.CategoryId).IsRequired();
                    b.HasQueryFilter(p => p.Category.DeletedOn == null);
                });

            modelBuilder.Entity<CategorizedDependent38965>()
                .HasOne(d => d.Principal)
                .WithMany()
                .HasForeignKey(d => d.PrincipalId)
                .IsRequired();
        }

        public Task SeedAsync()
        {
            AddRange(
                new CategorizedDependent38965
                {
                    GroupId = 1,
                    Principal = new CategorizedPrincipal38965 { Value = 10, Category = new Category38965() }
                },
                new CategorizedDependent38965
                {
                    GroupId = 2,
                    Principal = new CategorizedPrincipal38965
                    {
                        Value = 20,
                        Category = new Category38965 { DeletedOn = new DateTime(2026, 1, 1) }
                    }
                });

            return SaveChangesAsync();
        }
    }

    protected class Category38965
    {
        public int Id { get; set; }
        public DateTime? DeletedOn { get; set; }
    }

    protected class CategorizedPrincipal38965
    {
        public int Id { get; set; }
        public int Value { get; set; }
        public int CategoryId { get; set; }
        public Category38965 Category { get; set; } = null!;
    }

    protected class CategorizedDependent38965
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int PrincipalId { get; set; }
        public CategorizedPrincipal38965 Principal { get; set; } = null!;
    }

    protected class Context38965_Chain(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Only the far end of the chain is filtered; the hop in between is not.
            modelBuilder.Entity<ChainLeaf38965>().HasQueryFilter(l => !l.Filtered);

            modelBuilder.Entity<ChainMiddle38965>()
                .HasOne(m => m.Leaf).WithMany().HasForeignKey(m => m.LeafId).IsRequired();

            modelBuilder.Entity<ChainDependent38965>()
                .HasOne(d => d.Middle).WithMany().HasForeignKey(d => d.MiddleId).IsRequired();
        }

        public Task SeedAsync()
        {
            AddRange(
                new ChainDependent38965
                {
                    GroupId = 1,
                    Middle = new ChainMiddle38965 { Leaf = new ChainLeaf38965 { Value = 10 } }
                },
                new ChainDependent38965
                {
                    GroupId = 2,
                    Middle = new ChainMiddle38965
                    {
                        Leaf = new ChainLeaf38965 { Value = 20, Filtered = true }
                    }
                });

            return SaveChangesAsync();
        }
    }

    protected class ChainLeaf38965
    {
        public int Id { get; set; }
        public int Value { get; set; }
        public bool Filtered { get; set; }
    }

    protected class ChainMiddle38965
    {
        public int Id { get; set; }
        public bool Filtered { get; set; }
        public int LeafId { get; set; }
        public ChainLeaf38965 Leaf { get; set; } = null!;
    }

    protected class Context38965_OptionalThenFiltered(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChainLeaf38965>().HasQueryFilter(l => !l.Filtered);

            modelBuilder.Entity<ChainMiddle38965>()
                .HasOne(m => m.Leaf).WithMany().HasForeignKey(m => m.LeafId).IsRequired();

            // Optional, so the leaf behind it joins as an outer join for reasons of its own.
            modelBuilder.Entity<OptionalChainDependent38965>()
                .HasOne(d => d.Middle).WithMany().HasForeignKey(d => d.MiddleId);
        }

        public Task SeedAsync()
        {
            AddRange(
                new OptionalChainDependent38965
                {
                    GroupId = 1,
                    Middle = new ChainMiddle38965 { Leaf = new ChainLeaf38965 { Value = 10 } }
                },
                new OptionalChainDependent38965
                {
                    GroupId = 2,
                    Middle = new ChainMiddle38965
                    {
                        Leaf = new ChainLeaf38965 { Value = 20, Filtered = true }
                    }
                });

            return SaveChangesAsync();
        }
    }

    protected class OptionalChainDependent38965
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int? MiddleId { get; set; }
        public ChainMiddle38965? Middle { get; set; }
    }

    protected class Context38965_ChainBothFiltered(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Both hops of the chain are filtered this time.
            modelBuilder.Entity<ChainLeaf38965>().HasQueryFilter(l => !l.Filtered);

            modelBuilder.Entity<ChainMiddle38965>(
                b =>
                {
                    b.HasQueryFilter(m => !m.Filtered);
                    b.HasOne(m => m.Leaf).WithMany().HasForeignKey(m => m.LeafId).IsRequired();
                });

            modelBuilder.Entity<ChainDependent38965>()
                .HasOne(d => d.Middle).WithMany().HasForeignKey(d => d.MiddleId).IsRequired();
        }

        public Task SeedAsync()
        {
            // Group 2's middle survives its filter; only its leaf is filtered out.
            AddRange(
                new ChainDependent38965
                {
                    GroupId = 1,
                    Middle = new ChainMiddle38965 { Leaf = new ChainLeaf38965 { Value = 10 } }
                },
                new ChainDependent38965
                {
                    GroupId = 2,
                    Middle = new ChainMiddle38965
                    {
                        Leaf = new ChainLeaf38965 { Value = 20, Filtered = true }
                    }
                });

            return SaveChangesAsync();
        }
    }

    protected class ChainDependent38965
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int MiddleId { get; set; }
        public ChainMiddle38965 Middle { get; set; } = null!;
    }

    protected class Context38965_SelfReference(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Employee38965>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.HasQueryFilter(e => !e.Deleted);
                    b.HasOne(e => e.Manager)
                        .WithMany()
                        .HasForeignKey(e => e.ManagerId)
                        .IsRequired()
                        // A self-referencing required FK cannot cascade on SQL Server.
                        .OnDelete(DeleteBehavior.NoAction);
                });

        public Task SeedAsync()
        {
            // 1 manages itself and 2; 3 is managed by 2, which is soft-deleted.
            AddRange(
                new Employee38965 { Id = 1, ManagerId = 1, DepartmentId = 1, Salary = 100 },
                new Employee38965 { Id = 2, ManagerId = 1, DepartmentId = 1, Salary = 200, Deleted = true },
                new Employee38965 { Id = 3, ManagerId = 2, DepartmentId = 2, Salary = 300 });

            return SaveChangesAsync();
        }
    }

    protected class Employee38965
    {
        public int Id { get; set; }
        public int DepartmentId { get; set; }
        public int Salary { get; set; }
        public bool Deleted { get; set; }
        public int ManagerId { get; set; }
        public Employee38965 Manager { get; set; } = null!;
    }

    protected abstract class InheritanceContext38965(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InheritancePrincipal38965>(
                b =>
                {
                    b.Property(p => p.Id).ValueGeneratedNever();
                    b.HasQueryFilter(p => !p.Filtered);
                });

            modelBuilder.Entity<InheritanceDerivedPrincipal38965>();

            modelBuilder.Entity<InheritanceDependent38965>(
                b =>
                {
                    b.Property(d => d.Id).ValueGeneratedNever();
                    b.HasOne(d => d.Principal).WithMany().HasForeignKey(d => d.PrincipalId).IsRequired();
                });
        }

        public Task SeedAsync()
        {
            AddRange(
                new InheritanceDependent38965
                {
                    Id = 1,
                    GroupId = 1,
                    Principal = new InheritanceDerivedPrincipal38965 { Id = 1, Value = 10, Extra = 1 }
                },
                new InheritanceDependent38965
                {
                    Id = 2,
                    GroupId = 2,
                    Principal = new InheritanceDerivedPrincipal38965
                    {
                        Id = 2,
                        Value = 20,
                        Extra = 2,
                        Filtered = true
                    }
                });

            return SaveChangesAsync();
        }
    }

    protected class Context38965_Tph(DbContextOptions options) : InheritanceContext38965(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<InheritancePrincipal38965>().UseTphMappingStrategy();
        }
    }

    protected class Context38965_Tpt(DbContextOptions options) : InheritanceContext38965(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<InheritancePrincipal38965>().UseTptMappingStrategy();
        }
    }

    protected class Context38965_Tpc(DbContextOptions options) : InheritanceContext38965(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<InheritancePrincipal38965>().UseTpcMappingStrategy();
        }
    }

    protected class InheritancePrincipal38965
    {
        public int Id { get; set; }
        public int Value { get; set; }
        public bool Filtered { get; set; }
    }

    protected class InheritanceDerivedPrincipal38965 : InheritancePrincipal38965
    {
        public int Extra { get; set; }
    }

    protected class InheritanceDependent38965
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int PrincipalId { get; set; }
        public InheritancePrincipal38965 Principal { get; set; } = null!;
    }

    #endregion
}
