// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Projection;

public class OwnedJsonProjectionSqlServerTest
    : OwnedJsonProjectionRelationalTestBase<OwnedJsonRelationshipsSqlServerFixture>
{
    public OwnedJsonProjectionSqlServerTest(OwnedJsonRelationshipsSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override Task Select_trunk_collection(bool async)
        => AssertCantTrackJson(() => base.Select_trunk_collection(async));

    public override Task Select_branch_required_collection(bool async)
        => AssertCantTrackJson(() => base.Select_branch_required_collection(async));

    public override Task Select_branch_optional_collection(bool async)
        => AssertCantTrackJson(() => base.Select_branch_optional_collection(async));

    public override Task Project_branch_collection_element_using_indexer_constant(bool async)
        => AssertCantTrackJson(() => base.Project_branch_collection_element_using_indexer_constant(async));

    public override Task Select_multiple_branch_leaf(bool async)
        => AssertCantTrackJson(() => base.Select_multiple_branch_leaf(async));

    public override Task Select_subquery_root_set_trunk_FirstOrDefault_collection(bool async)
        => AssertCantTrackJson(() => base.Select_subquery_root_set_trunk_FirstOrDefault_collection(async));

    public override Task Select_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(bool async)
        => AssertCantTrackJson(() => base.Select_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(async));

    public override Task Select_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(bool async)
        => AssertCantTrackJson(() => base.Select_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(async));

    public override Task SelectMany_trunk_collection(bool async)
        => AssertCantTrackJson(() => base.SelectMany_trunk_collection(async));

    public override Task SelectMany_required_trunk_reference_branch_collection(bool async)
        => AssertCantTrackJson(() => base.SelectMany_required_trunk_reference_branch_collection(async));

    public override Task SelectMany_optional_trunk_reference_branch_collection(bool async)
        => AssertCantTrackJson(() => base.SelectMany_optional_trunk_reference_branch_collection(async));

    private async Task AssertCantTrackJson(Func<Task> test)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(test)).Message;

        Assert.Equal(RelationalStrings.JsonEntityOrCollectionProjectedAtRootLevelInTrackingQuery("AsNoTracking"), message);
        AssertSql();
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
