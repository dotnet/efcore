// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class OwnedQueryRelationalTestBase<TFixture> : OwnedQueryTestBase<TFixture>
    where TFixture : OwnedQueryRelationalTestBase<TFixture>.RelationalOwnedQueryFixture, new()
{
    protected OwnedQueryRelationalTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Query_for_base_type_loads_all_owned_navs_split(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OwnedPerson>().AsSplitQuery());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Query_for_branch_type_loads_all_owned_navs_split(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Branch>().AsSplitQuery());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Query_when_subquery_split(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OwnedPerson>().Distinct().AsSplitQuery()
                .OrderBy(p => p.Id)
                .Take(5)
                .Select(op => new { op }),
            assertOrder: true,
            elementAsserter: (e, a) => AssertEqual(e.op, a.op));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_multiple_owned_navigations_split(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OwnedPerson>().OrderBy(p => p.Id).AsSplitQuery()
                .Select(
                    p => new
                    {
                        p.Orders,
                        p.PersonAddress,
                        p.PersonAddress.Country.Planet
                    }),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                AssertCollection(e.Orders, a.Orders);
                AssertEqual(e.PersonAddress, a.PersonAddress);
                AssertEqual(e.Planet, a.Planet);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection_split(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OwnedPerson>().OrderBy(p => p.Id).Select(p => p.PersonAddress.Country.Planet.Moons).AsSplitQuery(),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Query_with_OfType_eagerly_loads_correct_owned_navigations_split(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OwnedPerson>().OfType<LeafA>().AsSplitQuery());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Unmapped_property_projection_loads_owned_navigations_split(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OwnedPerson>().Where(e => e.Id == 1).AsTracking().Select(e => new { e.ReadOnlyProperty }).AsSplitQuery());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Can_query_on_indexer_properties_split(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OwnedPerson>().Where(c => (string)c["Name"] == "Mona Cy").AsSplitQuery());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
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

    protected override QueryAsserter CreateQueryAsserter(TFixture fixture)
        => new RelationalQueryAsserter(
            fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

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
