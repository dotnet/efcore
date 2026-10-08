// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations;

public abstract class AssociationsSetOperationsTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : AssociationsQueryFixtureBase, new()
{
    [ConditionalFact]
    public virtual Task Over_associate_collections()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e =>
            e.AssociateCollection.Where(r => r.Int == 8)
                .Concat(e.AssociateCollection.Where(r => r.String == "foo"))
                .Count() == 4));

    [ConditionalTheory, MemberData(nameof(TrackingData))]
    public virtual Task Over_associate_collection_projected(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<RootEntity>().Select(e =>
                e.AssociateCollection.Where(r => r.Int == 8).Concat(e.AssociateCollection.Where(r => r.String == "foo"))),
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalTheory, MemberData(nameof(TrackingData))]
    public virtual Task Over_assocate_collection_Select_nested_with_aggregates_projected(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<RootEntity>().Select(e =>
                e.AssociateCollection.Where(r => r.Int == 8)
                    .Concat(e.AssociateCollection.Where(r => r.String == "foo"))
                    .Select(r => r.NestedCollection.Select(n => n.Int).Sum())
                    .Sum()),
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalFact]
    public virtual Task Over_nested_associate_collection()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e =>
            e.RequiredAssociate.NestedCollection.Where(r => r.Int == 8)
                .Concat(e.RequiredAssociate.NestedCollection.Where(r => r.String == "foo"))
                .Count()
            == 4));

    [ConditionalFact]
    public virtual Task Over_different_collection_properties()
        => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e =>
                e.RequiredAssociate.NestedCollection.Concat(e.OptionalAssociate!.NestedCollection).Count() == 4),
            ss => ss.Set<RootEntity>().Where(e =>
                e.RequiredAssociate.NestedCollection
                    .Concat(e.OptionalAssociate == null ? new List<NestedAssociateType>() : e.OptionalAssociate.NestedCollection).Count()
                == 4));
}
