// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations;

public abstract class AssociationsMiscellaneousTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : AssociationsQueryFixtureBase, new()
{
    #region Simple filters

    [ConditionalFact]
    public virtual Task Where_on_associate_scalar_property()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate.Int == 8));

    [ConditionalFact]
    public virtual Task Where_on_optional_associate_scalar_property()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e => e.OptionalAssociate!.Int == 8));

    [ConditionalFact]
    public virtual Task Where_on_nested_associate_scalar_property()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate.RequiredNestedAssociate.Int == 8));

    #endregion Simple filters
}
