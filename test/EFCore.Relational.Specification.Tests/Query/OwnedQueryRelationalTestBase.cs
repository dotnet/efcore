// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class OwnedQueryRelationalTestBase<TFixture>(TFixture fixture) : OwnedQueryTestBase<TFixture>(fixture)
    where TFixture : OwnedQueryRelationalTestBase<TFixture>.RelationalOwnedQueryFixture, new()
{
    public override Task Contains_over_owned_collection(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Contains_over_owned_collection(async));

    // The query uses a row limiting operator ('Skip'/'Take') without an 'OrderBy' operator.
    public override Task ElementAt_over_owned_collection(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.ElementAt_over_owned_collection(async));

    // The query uses a row limiting operator ('Skip'/'Take') without an 'OrderBy' operator.
    public override Task ElementAtOrDefault_over_owned_collection(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.ElementAtOrDefault_over_owned_collection(async));

    // The query uses a row limiting operator ('Skip'/'Take') without an 'OrderBy' operator.
    public override Task Skip_Take_over_owned_collection(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Skip_Take_over_owned_collection(async));

    // This test is non-deterministic on relational, since FirstOrDefault is used without an ordering.
    // Since this is FirstOrDefault with a filter, we don't issue our usual "missing ordering" warning (see #33997).
    public override Task FirstOrDefault_over_owned_collection(bool async)
        => Task.CompletedTask;

    // Relational only: the in-memory provider throws reading the missing dependent's value instead of treating it as absent.
    // Both Bartons land in one group, so the aggregates run over a present and an absent dependent together.
    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_aggregate_on_optional_owned_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Barton>()
                .GroupBy(e => e.Simple != null)
                .Select(g => new
                {
                    g.Key,
                    Sum = g.Sum(e => e.Throned!.Value),
                    Above = g.Count(e => e.Throned!.Value > 40)
                }),
            ss => ss.Set<Barton>()
                .GroupBy(e => e.Simple != null)
                .Select(g => new
                {
                    g.Key,
                    Sum = g.Sum(e => e.Throned == null ? 0 : e.Throned.Value),
                    Above = g.Count(e => e.Throned != null && e.Throned.Value > 40)
                }),
            elementSorter: e => e.Key);

    // Relational only: Cosmos cannot reference a second root entity type in one query.
    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_aggregate_on_navigation_reached_through_owned_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OwnedPerson>()
                .GroupBy(e => (int)e.PersonAddress!["ZipCode"] > 20000)
                .Select(g => new
                {
                    g.Key,
                    Sum = g.Sum(e => (int)e.PersonAddress!["ZipCode"]),
                    Planet = g.Max(e => e.PersonAddress!.Country!.Planet!.Name)
                }),
            elementSorter: e => e.Key);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Query_for_base_type_loads_all_owned_navs_split(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OwnedPerson>().AsSplitQuery());

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Query_for_branch_type_loads_all_owned_navs_split(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Branch>().AsSplitQuery());

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Query_when_subquery_split(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OwnedPerson>().Distinct().AsSplitQuery()
                .OrderBy(p => p.Id)
                .Take(5)
                .Select(op => new { op }),
            assertOrder: true,
            elementAsserter: (e, a) => AssertEqual(e.op, a.op));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Project_multiple_owned_navigations_split(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OwnedPerson>().OrderBy(p => p.Id).AsSplitQuery()
                .Select(p => new
                {
                    p.Orders,
                    p.PersonAddress,
                    p.PersonAddress!.Country!.Planet
                }),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                AssertCollection(e.Orders, a.Orders);
                AssertEqual(e.PersonAddress, a.PersonAddress);
                AssertEqual(e.Planet, a.Planet);
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection_split(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OwnedPerson>().OrderBy(p => p.Id).Select(p => p.PersonAddress!.Country!.Planet!.Moons).AsSplitQuery(),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Query_with_OfType_eagerly_loads_correct_owned_navigations_split(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OwnedPerson>().OfType<LeafA>().AsSplitQuery());

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Unmapped_property_projection_loads_owned_navigations_split(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OwnedPerson>().Where(e => e.Id == 1).AsTracking().Select(e => new { e.ReadOnlyProperty }).AsSplitQuery());

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Can_query_on_indexer_properties_split(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OwnedPerson>().Where(c => (string)c["Name"] == "Mona Cy").AsSplitQuery());

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Using_from_sql_on_owner_generates_join_with_table_for_owned_shared_dependents(bool async)
    {
        using var context = CreateContext();
        var query = context.Set<OwnedPerson>().FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [OwnedPerson]"));

        if (async)
        {
            await query.ToListAsync();
        }
        else
        {
            query.ToList();
        }
    }

    protected string NormalizeDelimitersInRawString(string sql)
        => Fixture.TestStore.NormalizeDelimitersInRawString(sql);

    protected FormattableString NormalizeDelimitersInInterpolatedString(FormattableString sql)
        => Fixture.TestStore.NormalizeDelimitersInInterpolatedString(sql);

    public abstract class RelationalOwnedQueryFixture : OwnedQueryFixtureBase, ITestSqlLoggerFactory
    {
        public new RelationalTestStore TestStore
            => (RelationalTestStore)base.TestStore;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            => base.OnModelCreating(modelBuilder, context);
        // TODO: See issue#20334
        //modelBuilder.Entity<OwnedPerson>().OwnsOne(e => e.PersonAddress).Property(e => e.PlaceType).HasColumnName("PlaceType");
        //modelBuilder.Entity<Branch>().OwnsOne(e => e.BranchAddress).Property(e => e.PlaceType).HasColumnName("PlaceType");
        //modelBuilder.Entity<LeafA>().OwnsOne(e => e.LeafAAddress).Property(e => e.PlaceType).HasColumnName("PlaceType");
        //modelBuilder.Entity<LeafB>().OwnsOne(e => e.LeafBAddress).Property(e => e.PlaceType).HasColumnName("PlaceType");
    }
}
