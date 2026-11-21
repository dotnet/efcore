// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Associations.ComplexProperties;

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexJson;

public abstract class ComplexJsonProjectionRelationalTestBase<TFixture> : ComplexPropertiesProjectionTestBase<TFixture>
    where TFixture : ComplexJsonRelationalFixtureBase, new()
{
    public ComplexJsonProjectionRelationalTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Select_complex_properties_with_identity_resolution(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RootEntity>().Select(e => new { e.Id, e.RequiredAssociate, e.OptionalAssociate }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                AssertEqual(e.RequiredAssociate, a.RequiredAssociate);
                AssertEqual(e.OptionalAssociate, a.OptionalAssociate);
            },
            queryTrackingBehavior: QueryTrackingBehavior.NoTrackingWithIdentityResolution);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Select_complex_properties_with_identity_resolution_member_init(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RootEntity>().Select(e => new RootEntity
            {
                Id = e.Id,
                Name = e.Name,
                RequiredAssociate = e.RequiredAssociate,
                OptionalAssociate = e.OptionalAssociate
            }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.Name, a.Name);
                AssertEqual(e.RequiredAssociate, a.RequiredAssociate);
                AssertEqual(e.OptionalAssociate, a.OptionalAssociate);
            },
            queryTrackingBehavior: QueryTrackingBehavior.NoTrackingWithIdentityResolution);

    [Theory, MemberData(nameof(TrackingData))]
    public virtual Task Select_associate_collection_on_GroupBy_First(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<RootEntity>()
                .GroupBy(e => e.Id % 2)
                .Select(g => g.OrderBy(e => e.Id).First(e => e.Id > 0))
                .Select(e => e.AssociateCollection),
            elementSorter: c => string.Join(",", c.Select(a => a.Id)),
            elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: r => r.Id),
            queryTrackingBehavior: queryTrackingBehavior);

    [Fact]
    public virtual Task Select_associate_collection_Count_on_GroupBy_First()
        => AssertQuery(ss => ss.Set<RootEntity>()
            .GroupBy(e => e.Id % 2)
            .Select(g => g.OrderBy(e => e.Id).First(e => e.Id > 0))
            .Select(e => e.AssociateCollection.Count));

    protected void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
