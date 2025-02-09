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

    [ConditionalTheory(Skip = "issue 3535")]
    public override Task Project_root(bool async)
        => base.Project_root(async);

    [ConditionalTheory(Skip = "issue 3535")]
    public override Task Project_trunk_optional(bool async)
        => base.Project_trunk_optional(async);

    [ConditionalTheory(Skip = "issue 3535")]
    public override Task Project_trunk_required(bool async)
        => base.Project_trunk_required(async);

    [ConditionalTheory(Skip = "issue 3535")]
    public override Task Project_trunk_collection(bool async)
        => base.Project_trunk_collection(async);

    [ConditionalTheory(Skip = "issue 3535")]
    public override Task Project_branch_required_required(bool async)
        => base.Project_branch_required_required(async);

    [ConditionalTheory(Skip = "issue 3535")]
    public override Task Project_branch_required_optional(bool async)
        => base.Project_branch_required_optional(async);

    [ConditionalTheory(Skip = "issue 3535")]
    public override Task Project_branch_required_collection(bool async)
        => base.Project_branch_required_collection(async);

    [ConditionalTheory(Skip = "issue 3535")]
    public override Task Project_branch_optional_required(bool async)
        => base.Project_branch_optional_required(async);

    [ConditionalTheory(Skip = "issue 3535")]
    public override Task Project_branch_optional_optional(bool async)
        => base.Project_branch_optional_optional(async);

    [ConditionalTheory(Skip = "issue 3535")]
    public override Task Project_branch_optional_collection(bool async)
        => base.Project_branch_optional_collection(async);
}
