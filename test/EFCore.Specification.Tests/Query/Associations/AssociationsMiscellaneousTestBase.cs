// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations;

public abstract class AssociationsMiscellaneousTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : AssociationsQueryFixtureBase, new()
{
    #region Simple filters

    [ConditionalFact]
    public virtual Task Where_related_property()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e => e.RequiredRelated.Int == 8));

    [ConditionalFact]
    public virtual Task Where_optional_related_property()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e => e.OptionalRelated!.Int == 8));

    [ConditionalFact]
    public virtual Task Where_nested_related_property()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e => e.RequiredRelated.RequiredNested.Int == 8));

    #endregion Simple filters
}
