// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.InProjection;

public class JsonRelationshipsInProjectionNoTrackingQuerySqliteTest
    : JsonRelationshipsInProjectionNoTrackingQueryRelationalTestBase<JsonRelationshipsQuerySqliteFixture>
{
    public JsonRelationshipsInProjectionNoTrackingQuerySqliteTest(JsonRelationshipsQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
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
SELECT "r"."Id", "r"."Name", "r"."OptionalReferenceTrunkId", "r"."RequiredReferenceTrunkId", "r"."CollectionTrunk", "r"."OptionalReferenceTrunk", "r"."RequiredReferenceTrunk"
FROM "RootEntities" AS "r"
""");
    }

    public override async Task Project_trunk_optional(bool async)
    {
        await base.Project_trunk_optional(async);

        AssertSql(
            """
SELECT "r"."OptionalReferenceTrunk", "r"."Id"
FROM "RootEntities" AS "r"
ORDER BY "r"."Id"
""");
    }

    public override async Task Project_trunk_required(bool async)
    {
        await base.Project_trunk_required(async);

        AssertSql(
            """
SELECT "r"."RequiredReferenceTrunk", "r"."Id"
FROM "RootEntities" AS "r"
ORDER BY "r"."Id"
""");
    }

    public override async Task Project_trunk_collection(bool async)
    {
        await base.Project_trunk_collection(async);

        AssertSql(
            """
SELECT "r"."CollectionTrunk", "r"."Id"
FROM "RootEntities" AS "r"
ORDER BY "r"."Id"
""");
    }

    public override async Task Project_trunk_required_required(bool async)
    {
        await base.Project_trunk_required_required(async);

        AssertSql(
            """
SELECT "r"."RequiredReferenceTrunk" ->> 'RequiredReferenceBranch', "r"."Id"
FROM "RootEntities" AS "r"
ORDER BY "r"."Id"
""");
    }

    public override async Task Project_trunk_required_optional(bool async)
    {
        await base.Project_trunk_required_optional(async);

        AssertSql(
            """
SELECT "r"."RequiredReferenceTrunk" ->> 'OptionalReferenceBranch', "r"."Id"
FROM "RootEntities" AS "r"
ORDER BY "r"."Id"
""");
    }

    public override async Task Project_trunk_required_collection(bool async)
    {
        await base.Project_trunk_required_collection(async);

        AssertSql(
            """
SELECT "r"."RequiredReferenceTrunk" ->> 'CollectionBranch', "r"."Id"
FROM "RootEntities" AS "r"
ORDER BY "r"."Id"
""");
    }

    public override async Task Project_trunk_optional_required(bool async)
    {
        await base.Project_trunk_optional_required(async);

        AssertSql(
            """
SELECT "r"."RequiredReferenceTrunk" ->> 'RequiredReferenceBranch', "r"."Id"
FROM "RootEntities" AS "r"
ORDER BY "r"."Id"
""");
    }

    public override async Task Project_trunk_optional_optional(bool async)
    {
        await base.Project_trunk_optional_optional(async);

        AssertSql(
            """
SELECT "r"."RequiredReferenceTrunk" ->> 'OptionalReferenceBranch', "r"."Id"
FROM "RootEntities" AS "r"
ORDER BY "r"."Id"
""");
    }

    public override async Task Project_trunk_optional_collection(bool async)
    {
        await base.Project_trunk_optional_collection(async);

        AssertSql(
            """
SELECT "r"."RequiredReferenceTrunk" ->> 'CollectionBranch', "r"."Id"
FROM "RootEntities" AS "r"
ORDER BY "r"."Id"
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
