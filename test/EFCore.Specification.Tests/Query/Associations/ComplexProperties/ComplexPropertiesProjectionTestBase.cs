// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexProperties;

public abstract class ComplexPropertiesProjectionTestBase<TFixture>(TFixture fixture)
    : AssociationsProjectionTestBase<TFixture>(fixture)
    where TFixture : ComplexPropertiesFixtureBase, new()
{
    public override bool AssertQueryTrackingBehaviour
        => false;

    #region Value types

    [Theory, MemberData(nameof(TrackingData))]
    public virtual Task Select_root_with_value_types(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<ValueRootEntity>(),
            queryTrackingBehavior: queryTrackingBehavior);

    [Theory, MemberData(nameof(TrackingData))]
    public virtual Task Select_non_nullable_value_type(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<ValueRootEntity>().OrderBy(e => e.Id).Select(x => x.RequiredAssociate),
            assertOrder: true,
            queryTrackingBehavior: queryTrackingBehavior);

    [Theory, MemberData(nameof(TrackingData))]
    public virtual Task Select_nullable_value_type(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<ValueRootEntity>().OrderBy(e => e.Id).Select(x => x.OptionalAssociate),
            assertOrder: true,
            queryTrackingBehavior: queryTrackingBehavior);

    [Theory, MemberData(nameof(TrackingData))]
    public virtual Task Select_nullable_value_type_with_Value(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<ValueRootEntity>().OrderBy(e => e.Id).Select(x => x.OptionalAssociate!.Value),
            ss => ss.Set<ValueRootEntity>().OrderBy(e => e.Id)
                .Select(x => x.OptionalAssociate == null ? default : x.OptionalAssociate!.Value),
            assertOrder: true,
            queryTrackingBehavior: queryTrackingBehavior);

    #endregion Value types

    #region Subquery

    [Theory, MemberData(nameof(TrackingData))]
    public virtual Task Select_associate_on_GroupBy_First(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<RootEntity>()
                .GroupBy(e => e.Id % 2)
                .Select(g => g.OrderBy(e => e.Id).First(e => e.Id > 0))
                .Select(e => e.RequiredAssociate),
            queryTrackingBehavior: queryTrackingBehavior);

    [Theory, MemberData(nameof(TrackingData))]
    public virtual Task Select_nested_associate_scalar_property_on_GroupBy_First(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<RootEntity>()
                .GroupBy(e => e.Id % 2)
                .Select(g => g.OrderBy(e => e.Id).First(e => e.Id > 0))
                .Select(e => e.RequiredAssociate.RequiredNestedAssociate.Int),
            queryTrackingBehavior: queryTrackingBehavior);

    [Theory, MemberData(nameof(TrackingData))]
    public virtual Task Select_optional_associate_scalar_property_on_GroupBy_First(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<RootEntity>()
                .GroupBy(e => e.Id)
                .Select(g => g.OrderBy(e => e.Id).First(e => e.Id > 0))
                .Select(e => (int?)e.OptionalAssociate!.Int),
            ss => ss.Set<RootEntity>()
                .GroupBy(e => e.Id)
                .Select(g => g.OrderBy(e => e.Id).First(e => e.Id > 0))
                .Select(e => e.OptionalAssociate == null ? null : (int?)e.OptionalAssociate.Int),
            queryTrackingBehavior: queryTrackingBehavior);

    [Theory, MemberData(nameof(TrackingData))]
    public virtual Task Select_optional_associate_nested_scalar_property_on_GroupBy_First(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<RootEntity>()
                .GroupBy(e => e.Id)
                .Select(g => g.OrderBy(e => e.Id).First(e => e.Id > 0))
                .Select(e => (int?)e.OptionalAssociate!.RequiredNestedAssociate.Int),
            ss => ss.Set<RootEntity>()
                .GroupBy(e => e.Id)
                .Select(g => g.OrderBy(e => e.Id).First(e => e.Id > 0))
                .Select(e => e.OptionalAssociate == null ? null : (int?)e.OptionalAssociate.RequiredNestedAssociate.Int),
            queryTrackingBehavior: queryTrackingBehavior);

    #endregion Subquery
}
