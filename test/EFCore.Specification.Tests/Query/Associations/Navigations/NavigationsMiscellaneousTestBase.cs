// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.Navigations;

public abstract class NavigationsMiscellaneousTestBase<TFixture>(TFixture fixture)
    : AssociationsMiscellaneousTestBase<TFixture>(fixture)
    where TFixture : NavigationsFixtureBase, new()
{
}
