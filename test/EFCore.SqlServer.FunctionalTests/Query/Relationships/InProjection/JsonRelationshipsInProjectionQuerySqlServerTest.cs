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

    public override Task Project_trunk_optional(bool async)
        => AssertCantTrackJson(() => base.Project_trunk_optional(async));

    public override Task Project_trunk_required(bool async)
        => AssertCantTrackJson(() => base.Project_trunk_required(async));

    public override Task Project_trunk_collection(bool async)
        => AssertCantTrackJson(() => base.Project_trunk_collection(async));

    public override Task Project_trunk_required_required(bool async)
        => AssertCantTrackJson(() => base.Project_trunk_required_required(async));

    public override Task Project_trunk_required_optional(bool async)
        => AssertCantTrackJson(() => base.Project_trunk_required_optional(async));

    public override Task Project_trunk_required_collection(bool async)
        => AssertCantTrackJson(() => base.Project_trunk_required_collection(async));

    public override  Task Project_trunk_optional_required(bool async)
        => AssertCantTrackJson(() => base.Project_trunk_optional_required(async));

    public override Task Project_trunk_optional_optional(bool async)
        => AssertCantTrackJson(() => base.Project_trunk_optional_optional(async));

    public override Task Project_trunk_optional_collection(bool async)
        => AssertCantTrackJson(() => base.Project_trunk_optional_collection(async));

    private async Task AssertCantTrackJson(Func<Task> test)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(test)).Message;

        Assert.Equal(RelationalStrings.JsonEntityOrCollectionProjectedAtRootLevelInTrackingQuery("AsNoTracking"), message);
        AssertSql();
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
