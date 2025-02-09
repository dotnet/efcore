// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.InProjection;

public abstract class JsonRelationshipsInProjectionNoTrackingQueryRelationalTestBase<TFixture>(TFixture fixture)
    : JsonRelationshipsInProjectionNoTrackingQueryTestBase<TFixture>(fixture)
        where TFixture : JsonRelationshipsQueryRelationalFixtureBase, new()
{
}
