// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.InProjection;

/// <summary>
/// Tests for using navigations in projection - mostly to test shaper code around
/// </summary>
public abstract class RelationshipsNoCollectionsInProjectionQueryTestBase<TFixture>(TFixture fixture) : RelationshipsInProjectionQueryTestBase<TFixture>(fixture)
    where TFixture : RelationshipsQueryFixtureBase, new()
{
    public sealed override Task Project_trunk_collection(bool async)
        => Task.CompletedTask;

    public sealed override Task Project_branch_required_collection(bool async)
        => Task.CompletedTask;

    public sealed override Task Project_branch_optional_collection(bool async)
        => Task.CompletedTask;

    public sealed override Task Project_multiple_branch_leaf(bool async)
        => Task.CompletedTask;

    public sealed override Task Project_subquery_root_set_trunk_FirstOrDefault_collection(bool async)
        => Task.CompletedTask;

    public sealed override Task Project_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(bool async)
        => Task.CompletedTask;

    public sealed override Task Project_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(bool async)
        => Task.CompletedTask;

    public sealed override Task SelectMany_trunk_collection(bool async)
        => Task.CompletedTask;

    public sealed override Task SelectMany_required_trunk_reference_branch_collection(bool async)
        => Task.CompletedTask;

    public sealed override Task SelectMany_optional_trunk_reference_branch_collection(bool async)
        => Task.CompletedTask;
}
