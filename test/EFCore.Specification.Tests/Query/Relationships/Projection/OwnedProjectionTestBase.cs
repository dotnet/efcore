// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Projection;

public abstract class OwnedProjectionTestBase<TFixture>(TFixture fixture)
    : ProjectionTestBase<TFixture>(fixture)
        where TFixture : OwnedRelationshipsFixtureBase, new()
{
}
