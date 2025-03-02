// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.InProjection;

public class JsonRelationshipsInProjectionQuerySqlServerTest
    : JsonRelationshipsInProjectionQueryRelationalTestBase<JsonRelationshipsQuerySqlServerFixture>
{
    public JsonRelationshipsInProjectionQuerySqlServerTest(JsonRelationshipsQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Project_root(bool async)
    {
        await base.Project_root(async);

        AssertSql(
"""
SELECT [r].[Id], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [r].[CollectionTrunk], [r].[OptionalReferenceTrunk], [r].[RequiredReferenceTrunk]
FROM [RootEntities] AS [r]
""");
    }

    public override async Task Project_root_duplicated(bool async)
    {
        await base.Project_root_duplicated(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [r].[CollectionTrunk], [r].[OptionalReferenceTrunk], [r].[RequiredReferenceTrunk], [r].[CollectionTrunk], [r].[OptionalReferenceTrunk], [r].[RequiredReferenceTrunk]
FROM [RootEntities] AS [r]
""");
    }

    public override Task Project_trunk_optional(bool async)
        => AssertCantTrackJson(() => base.Project_trunk_optional(async));

    public override Task Project_trunk_required(bool async)
        => AssertCantTrackJson(() => base.Project_trunk_required(async));

    public override Task Project_trunk_collection(bool async)
        => AssertCantTrackJson(() => base.Project_trunk_collection(async));

    public override Task Project_branch_required_required(bool async)
        => AssertCantTrackJson(() => base.Project_branch_required_required(async));

    public override Task Project_branch_required_optional(bool async)
        => AssertCantTrackJson(() => base.Project_branch_required_optional(async));

    public override Task Project_branch_required_collection(bool async)
        => AssertCantTrackJson(() => base.Project_branch_required_collection(async));

    public override  Task Project_branch_optional_required(bool async)
        => AssertCantTrackJson(() => base.Project_branch_optional_required(async));

    public override Task Project_branch_optional_optional(bool async)
        => AssertCantTrackJson(() => base.Project_branch_optional_optional(async));

    public override Task Project_branch_optional_collection(bool async)
        => AssertCantTrackJson(() => base.Project_branch_optional_collection(async));

    public override Task Project_branch_collection_element_using_indexer_constant(bool async)
        => AssertCantTrackJson(() => base.Project_branch_collection_element_using_indexer_constant(async));

    public override Task Project_trunk_and_branch_duplicated(bool async)
        => AssertCantTrackJson(() => base.Project_trunk_and_branch_duplicated(async));

    public override Task Project_trunk_and_trunk_duplicated(bool async)
        => AssertCantTrackJson(() => base.Project_trunk_and_trunk_duplicated(async));

    public override Task Project_multiple_branch_leaf(bool async)
        => AssertCantTrackJson(() => base.Project_multiple_branch_leaf(async));

    public override Task Project_leaf_trunk_root(bool async)
        => AssertCantTrackJson(() => base.Project_leaf_trunk_root(async));

    public override Task Project_subquery_root_set_required_trunk_FirstOrDefault_branch(bool async)
        => AssertCantTrackJson(() => base.Project_subquery_root_set_required_trunk_FirstOrDefault_branch(async));

    public override Task Project_subquery_root_set_optional_trunk_FirstOrDefault_branch(bool async)
        => AssertCantTrackJson(() => base.Project_subquery_root_set_optional_trunk_FirstOrDefault_branch(async));

    public override Task Project_subquery_root_set_trunk_FirstOrDefault_collection(bool async)
        => AssertCantTrackJson(() => base.Project_subquery_root_set_trunk_FirstOrDefault_collection(async));

    public override Task Project_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(bool async)
        => AssertCantTrackJson(() => base.Project_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(async));

    public override Task Project_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(bool async)
        => AssertCantTrackJson(() => base.Project_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(async));

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
