// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.InProjection;

public class OwnedRelationshipsInProjectionNoTrackingQuerySqliteTest
    : OwnedRelationshipsInProjectionNoTrackingQueryRelationalTestBase<OwnedRelationshipsQuerySqliteFixture>
{
    public OwnedRelationshipsInProjectionNoTrackingQuerySqliteTest(OwnedRelationshipsQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task Project_root(bool async)
        => base.Project_root(async);

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task Project_trunk_optional(bool async)
        => base.Project_trunk_optional(async);

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task Project_trunk_required(bool async)
        => base.Project_trunk_required(async);

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task Project_trunk_collection(bool async)
        => base.Project_trunk_collection(async);

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task Project_branch_required_required(bool async)
        => base.Project_branch_required_required(async);

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task Project_branch_required_optional(bool async)
        => base.Project_branch_required_optional(async);

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task Project_branch_required_collection(bool async)
        => base.Project_branch_required_collection(async);

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task Project_branch_optional_required(bool async)
        => base.Project_branch_optional_required(async);

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task Project_branch_optional_optional(bool async)
        => base.Project_branch_optional_optional(async);

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task Project_branch_optional_collection(bool async)
        => base.Project_branch_optional_collection(async);

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task Project_root_duplicated(bool async)
        => base.Project_root_duplicated(async);

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task Project_trunk_and_branch_duplicated(bool async)
        => base.Project_trunk_and_branch_duplicated(async);

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task Project_trunk_and_trunk_duplicated(bool async)
        => base.Project_trunk_and_trunk_duplicated(async);

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task Project_multiple_branch_leaf(bool async)
        => base.Project_multiple_branch_leaf(async);

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task Project_leaf_trunk_root(bool async)
        => base.Project_leaf_trunk_root(async);

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task Project_subquery_root_set_required_trunk_FirstOrDefault_branch(bool async)
        => base.Project_subquery_root_set_required_trunk_FirstOrDefault_branch(async);

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task Project_subquery_root_set_optional_trunk_FirstOrDefault_branch(bool async)
        => base.Project_subquery_root_set_optional_trunk_FirstOrDefault_branch(async);

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task Project_subquery_root_set_trunk_FirstOrDefault_collection(bool async)
        => base.Project_subquery_root_set_trunk_FirstOrDefault_collection(async);

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task Project_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(bool async)
        => base.Project_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(async);

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task Project_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(bool async)
        => base.Project_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(async);

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task SelectMany_trunk_collection(bool async)
        => base.SelectMany_trunk_collection(async);

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task SelectMany_required_trunk_reference_branch_collection(bool async)
        => base.SelectMany_required_trunk_reference_branch_collection(async);

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task SelectMany_optional_trunk_reference_branch_collection(bool async)
        => base.SelectMany_optional_trunk_reference_branch_collection(async);
}
