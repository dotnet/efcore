// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Include;

public class NavigationIncludeTestBase<TFixture>(TFixture fixture)
    : IncludeTestBase<TFixture>(fixture)
        where TFixture : NavigationRelationshipsFixtureBase, new()
{
}
