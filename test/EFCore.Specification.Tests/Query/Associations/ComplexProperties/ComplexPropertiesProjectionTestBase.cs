// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexProperties;

public abstract class ComplexPropertiesProjectionTestBase<TFixture>(TFixture fixture)
    : AssociationsProjectionTestBase<TFixture>(fixture)
    where TFixture : ComplexPropertiesFixtureBase, new()
{
    #region Value types

    [ConditionalTheory]
    [MemberData(nameof(TrackingData))]
    public virtual Task Select_root_with_value_types(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<ValueRootEntity>(),
            queryTrackingBehavior: queryTrackingBehavior);


    [ConditionalTheory]
    [MemberData(nameof(TrackingData))]
    public virtual Task Select_non_nullable_value_type(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<ValueRootEntity>().OrderBy(e => e.Id).Select(x => x.RequiredRelated),
            assertOrder: true,
            queryTrackingBehavior: queryTrackingBehavior);


    [ConditionalTheory]
    [MemberData(nameof(TrackingData))]
    public virtual Task Select_nullable_value_type(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<ValueRootEntity>().OrderBy(e => e.Id).Select(x => x.OptionalRelated),
            assertOrder: true,
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalTheory]
    [MemberData(nameof(TrackingData))]
    public virtual Task Select_nullable_value_type_with_Value(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<ValueRootEntity>().OrderBy(e => e.Id).Select(x => x.OptionalRelated!.Value),
            ss => ss.Set<ValueRootEntity>().OrderBy(e => e.Id).Select(x => x.OptionalRelated == null ? default : x.OptionalRelated!.Value),
            assertOrder: true,
            queryTrackingBehavior: queryTrackingBehavior);

    #endregion Value types
}
