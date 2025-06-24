// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Navigations;

public abstract class NavigationsProjectionRelationalTestBase<TFixture>(TFixture fixture)
    : NavigationsProjectionTestBase<TFixture>(fixture)
        where TFixture : NavigationsRelationalFixtureBase, new()
{
}
