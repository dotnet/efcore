// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Projection;

public class OwnedJsonReferenceNoTrackingProjectionCosmosTest : OwnedJsonReferenceNoTrackingProjectionTestBase<OwnedJsonRelationshipsCosmosFixture>
{
    private readonly TrackingRewriter _trackingRewriter = new();

    public OwnedJsonReferenceNoTrackingProjectionCosmosTest(OwnedJsonRelationshipsCosmosFixture fixture, ITestOutputHelper testOutputHelper)
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

    public override Task Select_root(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_root(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
""");
            });

    public override Task Select_trunk_optional(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_trunk_optional(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
            });

    public override Task Select_trunk_required(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_trunk_required(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
            });

    public override Task Select_branch_required_required(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_branch_required_required(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
            });

    public override Task Select_branch_required_optional(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_branch_required_optional(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
            });

    public override Task Select_branch_optional_required(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_branch_optional_required(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
            });

    public override Task Select_branch_optional_optional(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_branch_optional_optional(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
            });

    public override Task Select_root_duplicated(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_root_duplicated(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
""");
            });

    public override async Task Select_trunk_and_branch_duplicated(bool async)
    {
        if (async)
        {
            //issue #31696
            await Assert.ThrowsAsync<NullReferenceException>(
                () => base.Select_trunk_and_branch_duplicated(async));

            AssertSql(
                """
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
        }
    }

    public override async Task Select_trunk_and_trunk_duplicated(bool async)
    {
        if (async)
        {
            //issue #31696
            await Assert.ThrowsAsync<NullReferenceException>(
                () => base.Select_trunk_and_trunk_duplicated(async));

            AssertSql(
                """
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
""");
        }
    }

    public override Task Select_leaf_trunk_root(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_leaf_trunk_root(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
""");
            });


    public override Task Select_subquery_root_set_required_trunk_FirstOrDefault_branch(bool async)
        => AssertTranslationFailed(
            () => base.Select_subquery_root_set_required_trunk_FirstOrDefault_branch(async));

    public override Task Select_subquery_root_set_optional_trunk_FirstOrDefault_branch(bool async)
        => AssertTranslationFailed(
            () => base.Select_subquery_root_set_optional_trunk_FirstOrDefault_branch(async));

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
