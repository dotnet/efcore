// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedNavigations;

public abstract class OwnedNavigationsMiscellaneousTestBase<TFixture>(TFixture fixture)
    : AssociationsMiscellaneousTestBase<TFixture>(fixture)
    where TFixture : OwnedNavigationsFixtureBase, new()
{
}
