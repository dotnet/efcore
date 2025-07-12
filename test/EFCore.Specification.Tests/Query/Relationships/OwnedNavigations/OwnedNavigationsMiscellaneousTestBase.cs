// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.OwnedNavigations;

public abstract class OwnedNavigationsMiscellaneousTestBase<TFixture>(TFixture fixture)
    : RelationshipsMiscellaneousTestBase<TFixture>(fixture)
        where TFixture : OwnedNavigationsFixtureBase, new()
{
}
