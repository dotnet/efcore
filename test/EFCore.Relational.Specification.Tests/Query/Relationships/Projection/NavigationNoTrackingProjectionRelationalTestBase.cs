// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Projection;

public abstract class NavigationNoTrackingProjectionRelationalTestBase<TFixture>(TFixture fixture)
    : NavigationNoTrackingProjectionTestBase<TFixture>(fixture)
        where TFixture : NavigationRelationshipsRelationalFixtureBase, new()
{
}
