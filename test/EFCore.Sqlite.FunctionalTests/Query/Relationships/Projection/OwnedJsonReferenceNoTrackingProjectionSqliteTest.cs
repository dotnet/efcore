// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Projection;

public class OwnedJsonReferenceNoTrackingProjectionSqliteTest
    : OwnedJsonReferenceNoTrackingProjectionRelationalTestBase<OwnedJsonRelationshipsSqliteFixture>
{
    public OwnedJsonReferenceNoTrackingProjectionSqliteTest(OwnedJsonRelationshipsSqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Select_root(bool async)
    {
        await base.Select_root(async);

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."OptionalReferenceTrunkId", "r"."RequiredReferenceTrunkId", "r"."CollectionTrunk", "r"."OptionalReferenceTrunk", "r"."RequiredReferenceTrunk"
FROM "RootEntities" AS "r"
""");
    }

    public override async Task Select_trunk_optional(bool async)
    {
        await base.Select_trunk_optional(async);

        AssertSql(
            """
SELECT "r"."OptionalReferenceTrunk", "r"."Id"
FROM "RootEntities" AS "r"
ORDER BY "r"."Id"
""");
    }

    public override async Task Select_trunk_required(bool async)
    {
        await base.Select_trunk_required(async);

        AssertSql(
            """
SELECT "r"."RequiredReferenceTrunk", "r"."Id"
FROM "RootEntities" AS "r"
ORDER BY "r"."Id"
""");
    }

    public override async Task Select_branch_required_required(bool async)
    {
        await base.Select_branch_required_required(async);

        AssertSql(
            """
SELECT "r"."RequiredReferenceTrunk" ->> 'RequiredReferenceBranch', "r"."Id"
FROM "RootEntities" AS "r"
ORDER BY "r"."Id"
""");
    }

    public override async Task Select_branch_required_optional(bool async)
    {
        await base.Select_branch_required_optional(async);

        AssertSql(
            """
SELECT "r"."RequiredReferenceTrunk" ->> 'OptionalReferenceBranch', "r"."Id"
FROM "RootEntities" AS "r"
ORDER BY "r"."Id"
""");
    }

    public override async Task Select_branch_optional_required(bool async)
    {
        await base.Select_branch_optional_required(async);

        AssertSql(
            """
SELECT "r"."RequiredReferenceTrunk" ->> 'RequiredReferenceBranch', "r"."Id"
FROM "RootEntities" AS "r"
ORDER BY "r"."Id"
""");
    }

    public override async Task Select_branch_optional_optional(bool async)
    {
        await base.Select_branch_optional_optional(async);

        AssertSql(
            """
SELECT "r"."RequiredReferenceTrunk" ->> 'OptionalReferenceBranch', "r"."Id"
FROM "RootEntities" AS "r"
ORDER BY "r"."Id"
""");
    }

    public override async Task Select_root_duplicated(bool async)
    {
        await base.Select_root_duplicated(async);

        AssertSql(
            """
SELECT "r"."Id", "r"."Name", "r"."OptionalReferenceTrunkId", "r"."RequiredReferenceTrunkId", "r"."CollectionTrunk", "r"."OptionalReferenceTrunk", "r"."RequiredReferenceTrunk", "r"."CollectionTrunk", "r"."OptionalReferenceTrunk", "r"."RequiredReferenceTrunk"
FROM "RootEntities" AS "r"
""");
    }

    public override async Task Select_trunk_and_branch_duplicated(bool async)
    {
        await base.Select_trunk_and_branch_duplicated(async);

        AssertSql(
            """
SELECT "r"."OptionalReferenceTrunk", "r"."Id", "r"."OptionalReferenceTrunk" ->> 'RequiredReferenceBranch', "r"."OptionalReferenceTrunk", "r"."OptionalReferenceTrunk" ->> 'RequiredReferenceBranch'
FROM "RootEntities" AS "r"
ORDER BY "r"."Id"
""");
    }

    public override async Task Select_trunk_and_trunk_duplicated(bool async)
    {
        await base.Select_trunk_and_trunk_duplicated(async);

        AssertSql(
            """
SELECT "r"."RequiredReferenceTrunk", "r"."Id", "r"."RequiredReferenceTrunk" ->> '$.OptionalReferenceBranch.RequiredReferenceLeaf', "r"."RequiredReferenceTrunk", "r"."RequiredReferenceTrunk" ->> '$.OptionalReferenceBranch.RequiredReferenceLeaf'
FROM "RootEntities" AS "r"
ORDER BY "r"."Id"
""");
    }

    public override async Task Select_leaf_trunk_root(bool async)
    {
        await base.Select_leaf_trunk_root(async);

        AssertSql(
            """
SELECT "r"."RequiredReferenceTrunk" ->> '$.RequiredReferenceBranch.RequiredReferenceLeaf', "r"."Id", "r"."RequiredReferenceTrunk", "r"."Name", "r"."OptionalReferenceTrunkId", "r"."RequiredReferenceTrunkId", "r"."CollectionTrunk", "r"."OptionalReferenceTrunk", "r"."RequiredReferenceTrunk"
FROM "RootEntities" AS "r"
""");
    }

    public override async Task Select_subquery_root_set_optional_trunk_FirstOrDefault_branch(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Select_subquery_root_set_optional_trunk_FirstOrDefault_branch(async)))
            .Message);

    public override async Task Select_subquery_root_set_required_trunk_FirstOrDefault_branch(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Select_subquery_root_set_required_trunk_FirstOrDefault_branch(async)))
            .Message);

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
