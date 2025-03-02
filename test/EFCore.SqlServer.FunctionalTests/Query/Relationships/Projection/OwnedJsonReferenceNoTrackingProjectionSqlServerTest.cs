// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Projection;

public class OwnedJsonReferenceNoTrackingProjectionSqlServerTest
    : OwnedJsonReferenceNoTrackingProjectionRelationalTestBase<OwnedJsonRelationshipsSqlServerFixture>
{
    public OwnedJsonReferenceNoTrackingProjectionSqlServerTest(OwnedJsonRelationshipsSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
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
SELECT [r].[Id], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [r].[CollectionTrunk], [r].[OptionalReferenceTrunk], [r].[RequiredReferenceTrunk]
FROM [RootEntities] AS [r]
""");
    }

    public override async Task Select_trunk_optional(bool async)
    {
        await base.Select_trunk_optional(async);

        AssertSql(
            """
SELECT [r].[OptionalReferenceTrunk], [r].[Id]
FROM [RootEntities] AS [r]
ORDER BY [r].[Id]
""");
    }

    public override async Task Select_trunk_required(bool async)
    {
        await base.Select_trunk_required(async);

        AssertSql(
            """
SELECT [r].[RequiredReferenceTrunk], [r].[Id]
FROM [RootEntities] AS [r]
ORDER BY [r].[Id]
""");
    }

    public override async Task Select_branch_required_required(bool async)
    {
        await base.Select_branch_required_required(async);

        AssertSql(
            """
SELECT JSON_QUERY([r].[RequiredReferenceTrunk], '$.RequiredReferenceBranch'), [r].[Id]
FROM [RootEntities] AS [r]
ORDER BY [r].[Id]
""");
    }

    public override async Task Select_branch_required_optional(bool async)
    {
        await base.Select_branch_required_optional(async);

        AssertSql(
            """
SELECT JSON_QUERY([r].[RequiredReferenceTrunk], '$.OptionalReferenceBranch'), [r].[Id]
FROM [RootEntities] AS [r]
ORDER BY [r].[Id]
""");
    }

    public override async Task Select_branch_optional_required(bool async)
    {
        await base.Select_branch_optional_required(async);

        AssertSql(
            """
SELECT JSON_QUERY([r].[RequiredReferenceTrunk], '$.RequiredReferenceBranch'), [r].[Id]
FROM [RootEntities] AS [r]
ORDER BY [r].[Id]
""");
    }

    public override async Task Select_branch_optional_optional(bool async)
    {
        await base.Select_branch_optional_optional(async);

        AssertSql(
            """
SELECT JSON_QUERY([r].[RequiredReferenceTrunk], '$.OptionalReferenceBranch'), [r].[Id]
FROM [RootEntities] AS [r]
ORDER BY [r].[Id]
""");
    }

    public override async Task Select_root_duplicated(bool async)
    {
        await base.Select_root_duplicated(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [r].[CollectionTrunk], [r].[OptionalReferenceTrunk], [r].[RequiredReferenceTrunk], [r].[CollectionTrunk], [r].[OptionalReferenceTrunk], [r].[RequiredReferenceTrunk]
FROM [RootEntities] AS [r]
""");
    }

    public override async Task Select_trunk_and_branch_duplicated(bool async)
    {
        await base.Select_trunk_and_branch_duplicated(async);

        AssertSql(
            """
SELECT [r].[OptionalReferenceTrunk], [r].[Id], JSON_QUERY([r].[OptionalReferenceTrunk], '$.RequiredReferenceBranch'), [r].[OptionalReferenceTrunk], JSON_QUERY([r].[OptionalReferenceTrunk], '$.RequiredReferenceBranch')
FROM [RootEntities] AS [r]
ORDER BY [r].[Id]
""");
    }

    public override async Task Select_trunk_and_trunk_duplicated(bool async)
    {
        await base.Select_trunk_and_trunk_duplicated(async);

        AssertSql(
            """
SELECT [r].[RequiredReferenceTrunk], [r].[Id], JSON_QUERY([r].[RequiredReferenceTrunk], '$.OptionalReferenceBranch.RequiredReferenceLeaf'), [r].[RequiredReferenceTrunk], JSON_QUERY([r].[RequiredReferenceTrunk], '$.OptionalReferenceBranch.RequiredReferenceLeaf')
FROM [RootEntities] AS [r]
ORDER BY [r].[Id]
""");
    }

    public override async Task Select_leaf_trunk_root(bool async)
    {
        await base.Select_leaf_trunk_root(async);

        AssertSql(
            """
SELECT JSON_QUERY([r].[RequiredReferenceTrunk], '$.RequiredReferenceBranch.RequiredReferenceLeaf'), [r].[Id], [r].[RequiredReferenceTrunk], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [r].[CollectionTrunk], [r].[OptionalReferenceTrunk], [r].[RequiredReferenceTrunk]
FROM [RootEntities] AS [r]
""");
    }

    public override async Task Select_subquery_root_set_required_trunk_FirstOrDefault_branch(bool async)
    {
        await base.Select_subquery_root_set_required_trunk_FirstOrDefault_branch(async);

        AssertSql(
            """
SELECT [r1].[c], [r1].[Id]
FROM [RootEntities] AS [r]
OUTER APPLY (
    SELECT TOP(1) JSON_QUERY([r0].[RequiredReferenceTrunk], '$.RequiredReferenceBranch') AS [c], [r0].[Id]
    FROM [RootEntities] AS [r0]
    ORDER BY [r0].[Id]
) AS [r1]
ORDER BY [r].[Id]
""");
    }

    public override async Task Select_subquery_root_set_optional_trunk_FirstOrDefault_branch(bool async)
    {
        await base.Select_subquery_root_set_optional_trunk_FirstOrDefault_branch(async);

        AssertSql(
            """
SELECT [r1].[c], [r1].[Id]
FROM [RootEntities] AS [r]
OUTER APPLY (
    SELECT TOP(1) JSON_QUERY([r0].[OptionalReferenceTrunk], '$.OptionalReferenceBranch') AS [c], [r0].[Id]
    FROM [RootEntities] AS [r0]
    ORDER BY [r0].[Id]
) AS [r1]
ORDER BY [r].[Id]
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
