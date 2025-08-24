// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations;

public abstract class AssociationsSetOperationsTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : AssociationsQueryFixtureBase, new()
{
    [ConditionalFact]
    public virtual Task On_related()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e =>
            e.RelatedCollection.Where(r => r.Int == 8)
                .Concat(e.RelatedCollection.Where(r => r.String == "foo"))
                .Count()
            == 4));

    [ConditionalTheory, MemberData(nameof(TrackingData))]
    public virtual Task On_related_projected(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<RootEntity>().Select(e =>
                e.RelatedCollection.Where(r => r.Int == 8).Concat(e.RelatedCollection.Where(r => r.String == "foo"))),
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalTheory, MemberData(nameof(TrackingData))]
    public virtual Task On_related_Select_nested_with_aggregates(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<RootEntity>().Select(e =>
                e.RelatedCollection.Where(r => r.Int == 8)
                    .Concat(e.RelatedCollection.Where(r => r.String == "foo"))
                    .Select(r => r.NestedCollection.Select(n => n.Int).Sum())
                    .Sum()),
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalFact]
    public virtual Task On_nested()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e =>
            e.RequiredRelated.NestedCollection.Where(r => r.Int == 8)
                .Concat(e.RequiredRelated.NestedCollection.Where(r => r.String == "foo"))
                .Count()
            == 4));

    [ConditionalFact]
    public virtual Task Over_different_collection_properties()
        => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e =>
                e.RequiredRelated.NestedCollection.Concat(e.OptionalRelated!.NestedCollection).Count() == 4),
            ss => ss.Set<RootEntity>().Where(e =>
                e.RequiredRelated.NestedCollection
                    .Concat(e.OptionalRelated == null ? new List<NestedType>() : e.OptionalRelated.NestedCollection).Count()
                == 4));
}
