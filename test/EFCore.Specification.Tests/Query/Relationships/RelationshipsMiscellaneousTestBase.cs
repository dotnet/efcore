// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships;

public abstract class RelationshipsMiscellaneousTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : RelationshipsQueryFixtureBase, new()
{
    #region Simple filters

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_related_property(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RootEntity>().Where(e => e.RequiredRelated.Int == 8));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_optional_related_property(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RootEntity>().Where(e => e.OptionalRelated!.Int == 9));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_nested_related_property(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RootEntity>().Where(e => e.RequiredRelated.RequiredNested.Int == 50));

    #endregion Simple filters
}
