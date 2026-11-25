// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Associations.ComplexProperties;

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexTableSplittingNestedJson;

public abstract class ComplexTableSplittingNestedJsonMiscellaneousRelationalTestBase<TFixture> : ComplexPropertiesMiscellaneousTestBase<TFixture>
    where TFixture : ComplexTableSplittingNestedJsonRelationalFixtureBase, new()
{
    public ComplexTableSplittingNestedJsonMiscellaneousRelationalTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [Fact]
    public virtual Task FromSql_on_root()
        => RelationalAssociationsTests.FromSql_on_root(this, Fixture);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task FromSql_on_root_composed(bool async)
        => AssertQuery(
            async,
            ss => ((DbSet<RootEntity>)ss.Set<RootEntity>())
                .FromSqlRaw(((RelationalTestStore)Fixture.TestStore).NormalizeDelimitersInRawString("SELECT * FROM [RootEntity]"))
                .Where(e => e.RequiredAssociate.Int == 8),
            ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate.Int == 8));

    // The associates are compared column by column, including the JSON columns of their nested associates and nested collection
    [Fact]
    public virtual Task Two_associates_equality()
        => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate == e.OptionalAssociate),
            ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate.Equals(e.OptionalAssociate)));

    #region Left join

    [Fact]
    public virtual Task Where_on_associate_scalar_property_over_left_join()
        => AssertQuery(
            ss => from r in ss.Set<RootEntity>()
                  join n in ss.Set<RootEntity>() on r.Id + 1 equals n.Id into next
                  from n in next.DefaultIfEmpty()
                  where n!.RequiredAssociate.Int != 8
                  select r.Id,
            ss => from r in ss.Set<RootEntity>()
                  join n in ss.Set<RootEntity>() on r.Id + 1 equals n.Id into next
                  from n in next.DefaultIfEmpty()
                  where n == null || n.RequiredAssociate.Int != 8
                  select r.Id);

    [Fact]
    public virtual Task Where_on_nested_associate_scalar_property_over_left_join()
        => AssertQuery(
            ss => from r in ss.Set<RootEntity>()
                  join n in ss.Set<RootEntity>() on r.Id + 1 equals n.Id into next
                  from n in next.DefaultIfEmpty()
                  where n!.RequiredAssociate.RequiredNestedAssociate.Int != 8
                  select r.Id,
            ss => from r in ss.Set<RootEntity>()
                  join n in ss.Set<RootEntity>() on r.Id + 1 equals n.Id into next
                  from n in next.DefaultIfEmpty()
                  where n == null || n.RequiredAssociate.RequiredNestedAssociate.Int != 8
                  select r.Id);

    [Fact]
    public virtual Task Where_on_associate_scalar_property_over_left_join_subquery()
        => AssertQuery(
            ss => from r in ss.Set<RootEntity>()
                  join n in ss.Set<RootEntity>().OrderBy(e => e.Id).Take(5) on r.Id + 1 equals n.Id into next
                  from n in next.DefaultIfEmpty()
                  where n!.RequiredAssociate.Int != 8
                  select r.Id,
            ss => from r in ss.Set<RootEntity>()
                  join n in ss.Set<RootEntity>().OrderBy(e => e.Id).Take(5) on r.Id + 1 equals n.Id into next
                  from n in next.DefaultIfEmpty()
                  where n == null || n.RequiredAssociate.Int != 8
                  select r.Id);

    #endregion Left join

    protected void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
