// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.InProjection;

public abstract class EntityRelationshipsInProjectionNoTrackingQueryTestBase<TFixture>(TFixture fixture)
    : EntityRelationshipsInProjectionQueryTestBase<TFixture>(fixture)
        where TFixture : EntityRelationshipsQueryFixtureBase, new()
{
}
