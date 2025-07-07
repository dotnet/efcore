// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Projection;

public class OwnedTableSplittingNoTrackingProjectionSqliteTest
    : OwnedTableSplittingNoTrackingProjectionRelationalTestBase<OwnedTableSplittingRelationshipsSqliteFixture>
{
    public OwnedTableSplittingNoTrackingProjectionSqliteTest(OwnedTableSplittingRelationshipsSqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task Select_trunk_collection(bool async)
        => base.Select_trunk_collection(async);

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task Select_branch_required_collection(bool async)
        => base.Select_branch_required_collection(async);

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task Select_branch_optional_collection(bool async)
        => base.Select_branch_optional_collection(async);

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task Select_multiple_branch_leaf(bool async)
        => base.Select_multiple_branch_leaf(async);

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task Select_subquery_root_set_trunk_FirstOrDefault_collection(bool async)
        => base.Select_subquery_root_set_trunk_FirstOrDefault_collection(async);

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task Select_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(bool async)
        => base.Select_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(async);

    [ConditionalTheory(Skip = "issue 26708")]
    public override Task Select_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(bool async)
        => base.Select_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(async);

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
