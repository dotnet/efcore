// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.References.InProjection;

public abstract class OwnedReferenceRelationshipsInProjectionQueryTestBase<TFixture>(TFixture fixture)
    : ReferenceRelationshipsInProjectionQueryTestBase<TFixture>(fixture)
        where TFixture : OwnedRelationshipsQueryFixtureBase, new()
{
}
