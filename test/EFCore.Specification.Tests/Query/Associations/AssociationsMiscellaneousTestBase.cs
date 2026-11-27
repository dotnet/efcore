// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations;

public abstract class AssociationsMiscellaneousTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : AssociationsQueryFixtureBase, new()
{
    #region Simple filters

    [Fact]
    public virtual Task Where_on_associate_scalar_property()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate.Int == 8));

    [Fact]
    public virtual Task Where_on_optional_associate_scalar_property()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e => e.OptionalAssociate!.Int == 8));

    [Fact]
    public virtual Task Where_on_nested_associate_scalar_property()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate.RequiredNestedAssociate.Int == 8));

    #endregion Simple filters

    #region GroupBy

    [Fact]
    public virtual Task GroupBy_OrderByDescending_First_on_associate_scalar_property()
        => AssertQuery(ss => ss.Set<RootEntity>()
            .GroupBy(e => e.Id % 2)
            .Select(g => g.OrderByDescending(e => e.RequiredAssociate.Int).First()));

    [Fact]
    public virtual Task GroupBy_MaxBy_on_associate_scalar_property()
        => AssertQuery(ss => ss.Set<RootEntity>()
            .GroupBy(e => e.Id % 2)
            .Select(g => g.MaxBy(e => e.RequiredAssociate.Int)!));

    [Fact]
    public virtual Task GroupBy_Max_on_associate_scalar_property()
        => AssertQuery(
            ss => ss.Set<RootEntity>()
                .GroupBy(e => e.Id % 2)
                .Select(g => new { g.Key, Max = g.Max(e => e.RequiredAssociate.Int) }),
            elementSorter: e => e.Key);

    [Fact]
    public virtual Task GroupBy_Count_with_predicate_on_associate_scalar_property()
        => AssertQuery(
            ss => ss.Set<RootEntity>()
                .GroupBy(e => e.Id % 2)
                .Select(g => new { g.Key, Count = g.Count(e => e.RequiredAssociate.Int == 8) }),
            elementSorter: e => e.Key);

    #endregion GroupBy
}
