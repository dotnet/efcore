// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Relationships.Include;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.References.Include;

public abstract class ReferenceRelationshipsIncludeQueryTestBase<TFixture>(TFixture fixture) : RelationshipsIncludeQueryTestBase<TFixture>(fixture)
    where TFixture : RelationshipsQueryFixtureBase, new()
{
    public override Task Include_trunk_collection(bool async)
        => Task.CompletedTask;

    public override Task Include_trunk_required_optional_and_collection(bool async)
        => Task.CompletedTask;

    public override Task Include_branch_required_collection(bool async)
        => Task.CompletedTask;

    public override Task Include_branch_optional_collection(bool async)
        => Task.CompletedTask;

    public override Task Include_branch_collection_collection(bool async)
        => Task.CompletedTask;
}
