// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.InProjection;

public class JsonRelationshipsInProjectionNoTrackingQueryCosmosTest : JsonRelationshipsInProjectionQueryTestBase<JsonRelationshipsQueryCosmosFixture>
{
    private readonly NoTrackingRewriter _noTrackingRewriter = new();

    public JsonRelationshipsInProjectionNoTrackingQueryCosmosTest(JsonRelationshipsQueryCosmosFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected override Expression RewriteServerQueryExpression(Expression serverQueryExpression)
    {
        var rewritten = _noTrackingRewriter.Visit(serverQueryExpression);

        return rewritten;
    }

    public override Task Project_root(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Project_root(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
""");
            });

    public override Task Project_trunk_optional(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Project_trunk_optional(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
            });

    public override Task Project_trunk_required(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Project_trunk_required(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
            });

    public override Task Project_trunk_collection(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Project_trunk_collection(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
            });

    public override Task Project_branch_required_required(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Project_branch_required_required(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
            });

    public override Task Project_branch_required_optional(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Project_branch_required_optional(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
            });

    public override async Task Project_branch_required_collection(bool async)
    {
        if (async)
        {
            //issue #31696
            await Assert.ThrowsAsync<NullReferenceException>(
                () => base.Project_branch_required_collection(async));

            AssertSql(
                """
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
        }
    }

    public override Task Project_branch_optional_required(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Project_branch_optional_required(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
            });

    public override Task Project_branch_optional_optional(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Project_branch_optional_optional(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
            });

    public override async Task Project_branch_optional_collection(bool async)
    {
        if (async)
        {
            //issue #31696
            await Assert.ThrowsAsync<NullReferenceException>(
                () => base.Project_branch_optional_collection(async));

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

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
