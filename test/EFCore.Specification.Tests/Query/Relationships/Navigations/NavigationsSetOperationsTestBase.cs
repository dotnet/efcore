// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Navigations;

public abstract class NavigationsSetOperationsTestBase<TFixture>(TFixture fixture)
    : RelationshipsSetOperationsTestBase<TFixture>(fixture)
    where TFixture : NavigationsFixtureBase, new()
{
}
