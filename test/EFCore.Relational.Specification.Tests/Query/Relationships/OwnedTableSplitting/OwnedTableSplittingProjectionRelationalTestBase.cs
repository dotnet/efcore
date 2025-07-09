// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Relationships.OwnedNavigations;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.OwnedTableSplitting;

public abstract class OwnedTableSplittingProjectionRelationalTestBase<TFixture>(TFixture fixture)
    : OwnedNavigationsProjectionTestBase<TFixture>(fixture)
        where TFixture : OwnedTableSplittingRelationalFixtureBase, new()
{
}
