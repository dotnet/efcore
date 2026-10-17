// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.Navigations;

public abstract class NavigationsCollectionTestBase<TFixture>(TFixture fixture) : AssociationsCollectionTestBase<TFixture>(fixture)
    where TFixture : NavigationsFixtureBase, new()
{
}
