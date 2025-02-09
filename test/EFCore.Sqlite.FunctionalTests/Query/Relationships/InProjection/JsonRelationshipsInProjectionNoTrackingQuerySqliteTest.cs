// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Internal;

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

    public override async Task Project_branch_required_required(bool async)
    {
        await base.Project_branch_required_required(async);

        AssertSql(
            """
SELECT "r"."RequiredReferenceTrunk" ->> 'RequiredReferenceBranch', "r"."Id"
FROM "RootEntities" AS "r"
ORDER BY "r"."Id"
""");
    }

    public override async Task Project_branch_required_optional(bool async)
    {
        await base.Project_branch_required_optional(async);

        AssertSql(
            """
SELECT "r"."RequiredReferenceTrunk" ->> 'OptionalReferenceBranch', "r"."Id"
FROM "RootEntities" AS "r"
ORDER BY "r"."Id"
""");
    }

    public override async Task Project_branch_required_collection(bool async)
    {
        await base.Project_branch_required_collection(async);

        AssertSql(
            """
SELECT "r"."RequiredReferenceTrunk" ->> 'CollectionBranch', "r"."Id"
FROM "RootEntities" AS "r"
ORDER BY "r"."Id"
""");
    }

    public override async Task Project_branch_optional_required(bool async)
    {
        await base.Project_branch_optional_required(async);

        AssertSql(
            """
SELECT "r"."RequiredReferenceTrunk" ->> 'RequiredReferenceBranch', "r"."Id"
FROM "RootEntities" AS "r"
ORDER BY "r"."Id"
""");
    }

    public override async Task Project_branch_optional_optional(bool async)
    {
        await base.Project_branch_optional_optional(async);

        AssertSql(
            """
SELECT "r"."RequiredReferenceTrunk" ->> 'OptionalReferenceBranch', "r"."Id"
FROM "RootEntities" AS "r"
ORDER BY "r"."Id"
""");
    }

    public override async Task Project_branch_optional_collection(bool async)
    {
        await base.Project_branch_optional_collection(async);

        AssertSql(
            """
SELECT "r"."RequiredReferenceTrunk" ->> 'CollectionBranch', "r"."Id"
FROM "RootEntities" AS "r"
ORDER BY "r"."Id"
""");
    }

    public override async Task Project_root_duplicated(bool async)
    {
        await base.Project_root_duplicated(async);

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."OptionalReferenceTrunkId", "r"."RequiredReferenceTrunkId", "r"."CollectionTrunk", "r"."OptionalReferenceTrunk", "r"."RequiredReferenceTrunk", "r"."CollectionTrunk", "r"."OptionalReferenceTrunk", "r"."RequiredReferenceTrunk"
FROM "RootEntities" AS "r"
""");
    }

    public override async Task Project_trunk_and_branch_duplicated(bool async)
    {
        await base.Project_trunk_and_branch_duplicated(async);

        AssertSql(
            """
SELECT "r"."OptionalReferenceTrunk", "r"."Id", "r"."OptionalReferenceTrunk" ->> 'RequiredReferenceBranch', "r"."OptionalReferenceTrunk", "r"."OptionalReferenceTrunk" ->> 'RequiredReferenceBranch'
FROM "RootEntities" AS "r"
ORDER BY "r"."Id"
""");
    }

    public override async Task Project_trunk_and_trunk_duplicated(bool async)
    {
        await base.Project_trunk_and_trunk_duplicated(async);

        AssertSql(
            """
SELECT "r"."RequiredReferenceTrunk", "r"."Id", "r"."RequiredReferenceTrunk" ->> '$.OptionalReferenceBranch.RequiredReferenceLeaf', "r"."RequiredReferenceTrunk", "r"."RequiredReferenceTrunk" ->> '$.OptionalReferenceBranch.RequiredReferenceLeaf'
FROM "RootEntities" AS "r"
ORDER BY "r"."Id"
""");
    }

    public override async Task Project_multiple_branch_leaf(bool async)
    {
        await base.Project_multiple_branch_leaf(async);

        AssertSql(
            """
SELECT "r"."Id", "r"."RequiredReferenceTrunk" ->> 'RequiredReferenceBranch', "r"."RequiredReferenceTrunk" ->> '$.RequiredReferenceBranch.OptionalReferenceLeaf', "r"."RequiredReferenceTrunk" ->> '$.RequiredReferenceBranch.CollectionLeaf', "r"."RequiredReferenceTrunk" ->> 'CollectionBranch', "r"."RequiredReferenceTrunk" ->> '$.RequiredReferenceBranch.OptionalReferenceLeaf.Name'
FROM "RootEntities" AS "r"
""");
    }

    public override async Task Project_leaf_trunk_root(bool async)
    {
        await base.Project_leaf_trunk_root(async);

        AssertSql(
            """
SELECT "r"."RequiredReferenceTrunk" ->> '$.RequiredReferenceBranch.RequiredReferenceLeaf', "r"."Id", "r"."RequiredReferenceTrunk", "r"."Name", "r"."OptionalReferenceTrunkId", "r"."RequiredReferenceTrunkId", "r"."CollectionTrunk", "r"."OptionalReferenceTrunk", "r"."RequiredReferenceTrunk"
FROM "RootEntities" AS "r"
""");
    }

    public override async Task Project_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Project_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(async)))
            .Message);

    public override async Task Project_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Project_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(async)))
            .Message);

    public override async Task Project_subquery_root_set_optional_trunk_FirstOrDefault_branch(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Project_subquery_root_set_optional_trunk_FirstOrDefault_branch(async)))
            .Message);

    public override async Task Project_subquery_root_set_required_trunk_FirstOrDefault_branch(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Project_subquery_root_set_required_trunk_FirstOrDefault_branch(async)))
            .Message);

    public override async Task Project_subquery_root_set_trunk_FirstOrDefault_collection(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Project_subquery_root_set_trunk_FirstOrDefault_collection(async)))
            .Message);

    public override async Task SelectMany_optional_trunk_reference_branch_collection(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.SelectMany_optional_trunk_reference_branch_collection(async)))
            .Message);


    public override async Task SelectMany_required_trunk_reference_branch_collection(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.SelectMany_required_trunk_reference_branch_collection(async)))
            .Message);


    public override async Task SelectMany_trunk_collection(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.SelectMany_trunk_collection(async)))
            .Message);

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
