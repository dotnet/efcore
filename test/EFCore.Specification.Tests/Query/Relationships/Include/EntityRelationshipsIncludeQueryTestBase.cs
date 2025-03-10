// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Include;

public abstract class EntityRelationshipsIncludeQueryTestBase<TFixture>(TFixture fixture)
    : RelationshipsIncludeQueryTestBase<TFixture>(fixture)
        where TFixture : EntityRelationshipsQueryFixtureBase, new()
{
}
