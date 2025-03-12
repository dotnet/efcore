// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Projection;

public class OwnedJsonNoTrackingProjectionCosmosTest : OwnedJsonNoTrackingProjectionTestBase<OwnedJsonRelationshipsCosmosFixture>
{
    private readonly TrackingRewriter _trackingRewriter = new();

    public OwnedJsonNoTrackingProjectionCosmosTest(OwnedJsonRelationshipsCosmosFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected override Expression RewriteServerQueryExpression(Expression serverQueryExpression)
    {
        var rewritten = _trackingRewriter.Visit(serverQueryExpression);

        return rewritten;
    }

    public override Task Select_trunk_collection(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_trunk_collection(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
            });

    public override async Task Select_branch_required_collection(bool async)
    {
        if (async)
        {
            //issue #31696
            await Assert.ThrowsAsync<NullReferenceException>(
                () => base.Select_branch_required_collection(async));

            AssertSql(
                """
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
        }
    }

    public override async Task Select_branch_optional_collection(bool async)
    {
        if (async)
        {
            //issue #31696
            await Assert.ThrowsAsync<NullReferenceException>(
                () => base.Select_branch_optional_collection(async));

            AssertSql(
                """
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
        }
    }

    public override async Task Project_branch_collection_element_using_indexer_constant(bool async)
    {
        if (async)
        {
            //issue #31696
            await Assert.ThrowsAsync<NullReferenceException>(
                () => base.Project_branch_collection_element_using_indexer_constant(async));

            AssertSql(
                """
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
        }
    }

    public override async Task Select_multiple_branch_leaf(bool async)
    {
        if (async)
        {
            //issue #35702
            await Assert.ThrowsAsync<ArgumentException>(
                () => base.Select_multiple_branch_leaf(async));

            AssertSql();
        }
    }

    public override Task Select_subquery_root_set_trunk_FirstOrDefault_collection(bool async)
        => AssertTranslationFailed(
            () => base.Select_subquery_root_set_trunk_FirstOrDefault_collection(async));

    public override Task Select_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(bool async)
        => AssertTranslationFailed(
            () => base.Select_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(async));

    public override Task Select_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(bool async)
        => AssertTranslationFailed(
            () => base.Select_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(async));

    public override async Task SelectMany_trunk_collection(bool async)
    {
        if (async)
        {
            //issue #34349
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => base.SelectMany_trunk_collection(async));

            AssertSql();
        }
    }

    public override async Task SelectMany_required_trunk_reference_branch_collection(bool async)
    {
        if (async)
        {
            //issue #34349
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => base.SelectMany_required_trunk_reference_branch_collection(async));

            AssertSql();
        }
    }

    public override async Task SelectMany_optional_trunk_reference_branch_collection(bool async)
    {
        if (async)
        {
            //issue #34349
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => base.SelectMany_optional_trunk_reference_branch_collection(async));

            AssertSql();
        }
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
