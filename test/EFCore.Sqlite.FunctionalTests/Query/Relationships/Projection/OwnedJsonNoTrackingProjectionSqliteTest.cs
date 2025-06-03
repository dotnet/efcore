// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Projection;

public class OwnedJsonNoTrackingProjectionSqliteTest
    : OwnedJsonNoTrackingProjectionRelationalTestBase<OwnedJsonRelationshipsSqliteFixture>
{
    public OwnedJsonNoTrackingProjectionSqliteTest(OwnedJsonRelationshipsSqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Select_trunk_collection(bool async)
    {
        await base.Select_trunk_collection(async);

        AssertSql(
            """
SELECT "r"."CollectionTrunk", "r"."Id"
FROM "RootEntities" AS "r"
ORDER BY "r"."Id"
""");
    }

    public override async Task Select_branch_required_collection(bool async)
    {
        await base.Select_branch_required_collection(async);

        AssertSql(
            """
SELECT "r"."RequiredReferenceTrunk" ->> 'CollectionBranch', "r"."Id"
FROM "RootEntities" AS "r"
ORDER BY "r"."Id"
""");
    }

    public override async Task Select_branch_optional_collection(bool async)
    {
        await base.Select_branch_optional_collection(async);

        AssertSql(
            """
SELECT "r"."RequiredReferenceTrunk" ->> 'CollectionBranch', "r"."Id"
FROM "RootEntities" AS "r"
ORDER BY "r"."Id"
""");
    }

    public override async Task Select_multiple_branch_leaf(bool async)
    {
        await base.Select_multiple_branch_leaf(async);

        AssertSql(
            """
SELECT "r"."Id", "r"."RequiredReferenceTrunk" ->> 'RequiredReferenceBranch', "r"."RequiredReferenceTrunk" ->> '$.RequiredReferenceBranch.OptionalReferenceLeaf', "r"."RequiredReferenceTrunk" ->> '$.RequiredReferenceBranch.CollectionLeaf', "r"."RequiredReferenceTrunk" ->> 'CollectionBranch', "r"."RequiredReferenceTrunk" ->> '$.RequiredReferenceBranch.OptionalReferenceLeaf.Name'
FROM "RootEntities" AS "r"
""");
    }

    public override async Task Select_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Select_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(async)))
            .Message);

    public override async Task Select_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Select_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(async)))
            .Message);

    public override async Task Select_subquery_root_set_trunk_FirstOrDefault_collection(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Select_subquery_root_set_trunk_FirstOrDefault_collection(async)))
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
