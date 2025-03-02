// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.References.Include;

public class EntityReferenceRelationshipsludeQueryTestBase<TFixture>(TFixture fixture)
    : ReferenceRelationshipsIncludeQueryTestBase<TFixture>(fixture)
        where TFixture : EntityRelationshipsQueryFixtureBase, new()
{
}
