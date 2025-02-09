// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.InProjection;

public abstract class JsonRelationshipsInProjectionQueryRelationalTestBase<TFixture>(TFixture fixture)
    : JsonRelationshipsInProjectionQueryTestBase<TFixture>(fixture)
        where TFixture : JsonRelationshipsQueryRelationalFixtureBase, new()
{
}
