// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.InProjection;

public class JsonRelationshipsInProjectionNoTrackingQuerySqlServerTest
    : JsonRelationshipsInProjectionNoTrackingQueryRelationalTestBase<JsonRelationshipsQuerySqlServerFixture>
{
    public JsonRelationshipsInProjectionNoTrackingQuerySqlServerTest(JsonRelationshipsQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
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

    public override async Task Project_trunk_optional(bool async)
    {
        await base.Project_trunk_optional(async);

        AssertSql(
            """
SELECT [r].[OptionalReferenceTrunk], [r].[Id]
FROM [RootEntities] AS [r]
ORDER BY [r].[Id]
""");
    }

    public override async Task Project_trunk_required(bool async)
    {
        await base.Project_trunk_required(async);

        AssertSql(
            """
SELECT [r].[RequiredReferenceTrunk], [r].[Id]
FROM [RootEntities] AS [r]
ORDER BY [r].[Id]
""");
    }

    public override async Task Project_trunk_collection(bool async)
    {
        await base.Project_trunk_collection(async);

        AssertSql(
            """
SELECT [r].[CollectionTrunk], [r].[Id]
FROM [RootEntities] AS [r]
ORDER BY [r].[Id]
""");
    }

    public override async Task Project_branch_required_required(bool async)
    {
        await base.Project_branch_required_required(async);

        AssertSql(
            """
SELECT JSON_QUERY([r].[RequiredReferenceTrunk], '$.RequiredReferenceBranch'), [r].[Id]
FROM [RootEntities] AS [r]
ORDER BY [r].[Id]
""");
    }

    public override async Task Project_branch_required_optional(bool async)
    {
        await base.Project_branch_required_optional(async);

        AssertSql(
            """
SELECT JSON_QUERY([r].[RequiredReferenceTrunk], '$.OptionalReferenceBranch'), [r].[Id]
FROM [RootEntities] AS [r]
ORDER BY [r].[Id]
""");
    }

    public override async Task Project_branch_required_collection(bool async)
    {
        await base.Project_branch_required_collection(async);

        AssertSql(
            """
SELECT JSON_QUERY([r].[RequiredReferenceTrunk], '$.CollectionBranch'), [r].[Id]
FROM [RootEntities] AS [r]
ORDER BY [r].[Id]
""");
    }

    public override async Task Project_branch_optional_required(bool async)
    {
        await base.Project_branch_optional_required(async);

        AssertSql(
            """
SELECT JSON_QUERY([r].[RequiredReferenceTrunk], '$.RequiredReferenceBranch'), [r].[Id]
FROM [RootEntities] AS [r]
ORDER BY [r].[Id]
""");
    }

    public override async Task Project_branch_optional_optional(bool async)
    {
        await base.Project_branch_optional_optional(async);

        AssertSql(
            """
SELECT JSON_QUERY([r].[RequiredReferenceTrunk], '$.OptionalReferenceBranch'), [r].[Id]
FROM [RootEntities] AS [r]
ORDER BY [r].[Id]
""");
    }

    public override async Task Project_branch_optional_collection(bool async)
    {
        await base.Project_branch_optional_collection(async);

        AssertSql(
            """
SELECT JSON_QUERY([r].[RequiredReferenceTrunk], '$.CollectionBranch'), [r].[Id]
FROM [RootEntities] AS [r]
ORDER BY [r].[Id]
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

    public override async Task Project_trunk_and_branch_duplicated(bool async)
    {
        await base.Project_trunk_and_branch_duplicated(async);

        AssertSql(
            """
SELECT [r].[OptionalReferenceTrunk], [r].[Id], JSON_QUERY([r].[OptionalReferenceTrunk], '$.RequiredReferenceBranch'), [r].[OptionalReferenceTrunk], JSON_QUERY([r].[OptionalReferenceTrunk], '$.RequiredReferenceBranch')
FROM [RootEntities] AS [r]
ORDER BY [r].[Id]
""");
    }

    public override async Task Project_trunk_and_trunk_duplicated(bool async)
    {
        await base.Project_trunk_and_trunk_duplicated(async);

        AssertSql(
            """
SELECT [r].[RequiredReferenceTrunk], [r].[Id], JSON_QUERY([r].[RequiredReferenceTrunk], '$.OptionalReferenceBranch.RequiredReferenceLeaf'), [r].[RequiredReferenceTrunk], JSON_QUERY([r].[RequiredReferenceTrunk], '$.OptionalReferenceBranch.RequiredReferenceLeaf')
FROM [RootEntities] AS [r]
ORDER BY [r].[Id]
""");
    }

    public override async Task Project_multiple_branch_leaf(bool async)
    {
        await base.Project_multiple_branch_leaf(async);

        AssertSql(
            """
SELECT [r].[Id], JSON_QUERY([r].[RequiredReferenceTrunk], '$.RequiredReferenceBranch'), JSON_QUERY([r].[RequiredReferenceTrunk], '$.RequiredReferenceBranch.OptionalReferenceLeaf'), JSON_QUERY([r].[RequiredReferenceTrunk], '$.RequiredReferenceBranch.CollectionLeaf'), JSON_QUERY([r].[RequiredReferenceTrunk], '$.CollectionBranch'), JSON_VALUE([r].[RequiredReferenceTrunk], '$.RequiredReferenceBranch.OptionalReferenceLeaf.Name')
FROM [RootEntities] AS [r]
""");
    }

    public override async Task Project_leaf_trunk_root(bool async)
    {
        await base.Project_leaf_trunk_root(async);

        AssertSql(
            """
SELECT JSON_QUERY([r].[RequiredReferenceTrunk], '$.RequiredReferenceBranch.RequiredReferenceLeaf'), [r].[Id], [r].[RequiredReferenceTrunk], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [r].[CollectionTrunk], [r].[OptionalReferenceTrunk], [r].[RequiredReferenceTrunk]
FROM [RootEntities] AS [r]
""");
    }

    public override async Task Project_subquery_root_set_required_trunk_FirstOrDefault_branch(bool async)
    {
        await base.Project_subquery_root_set_required_trunk_FirstOrDefault_branch(async);

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

    public override async Task Project_subquery_root_set_optional_trunk_FirstOrDefault_branch(bool async)
    {
        await base.Project_subquery_root_set_optional_trunk_FirstOrDefault_branch(async);

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

    public override async Task Project_subquery_root_set_trunk_FirstOrDefault_collection(bool async)
    {
        await base.Project_subquery_root_set_trunk_FirstOrDefault_collection(async);

        AssertSql(
            """
SELECT [r1].[c], [r1].[Id], [r1].[c0]
FROM [RootEntities] AS [r]
OUTER APPLY (
    SELECT TOP(1) JSON_QUERY([r0].[RequiredReferenceTrunk], '$.CollectionBranch') AS [c], [r0].[Id], 1 AS [c0]
    FROM [RootEntities] AS [r0]
    ORDER BY [r0].[Id]
) AS [r1]
ORDER BY [r].[Id]
""");
    }

    public override async Task Project_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(bool async)
    {
        await base.Project_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(async);

        AssertSql(
            """
SELECT [r1].[c], [r1].[Id], [r1].[c0], [r1].[Id0], [r1].[c1], [r1].[c2], [r1].[c3], [r1].[c4]
FROM [RootEntities] AS [r]
OUTER APPLY (
    SELECT TOP(1) JSON_QUERY([r].[RequiredReferenceTrunk], '$.CollectionBranch') AS [c], [r].[Id], [r0].[RequiredReferenceTrunk] AS [c0], [r0].[Id] AS [Id0], JSON_QUERY([r0].[RequiredReferenceTrunk], '$.RequiredReferenceBranch') AS [c1], JSON_VALUE([r0].[RequiredReferenceTrunk], '$.Name') AS [c2], JSON_VALUE([r].[RequiredReferenceTrunk], '$.RequiredReferenceBranch.Name') AS [c3], 1 AS [c4]
    FROM [RootEntities] AS [r0]
    ORDER BY [r0].[Id]
) AS [r1]
ORDER BY [r].[Id]
""");
    }

    public override async Task Project_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(bool async)
    {
        await base.Project_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(async);

        AssertSql(
            """
SELECT [r1].[c], [r1].[Id], [r1].[c0]
FROM [RootEntities] AS [r]
OUTER APPLY (
    SELECT TOP(1) JSON_QUERY([r].[RequiredReferenceTrunk], '$.CollectionBranch') AS [c], [r].[Id], 1 AS [c0]
    FROM [RootEntities] AS [r0]
    ORDER BY [r0].[Id]
) AS [r1]
ORDER BY [r].[Id]
""");
    }

    public override async Task SelectMany_trunk_collection(bool async)
    {
        await base.SelectMany_trunk_collection(async);

        AssertSql(
            """
SELECT [r].[Id], [c].[Name], [c].[CollectionBranch], [c].[OptionalReferenceBranch], [c].[RequiredReferenceBranch]
FROM [RootEntities] AS [r]
CROSS APPLY OPENJSON([r].[CollectionTrunk], '$') WITH (
    [Name] nvarchar(max) '$.Name',
    [CollectionBranch] nvarchar(max) '$.CollectionBranch' AS JSON,
    [OptionalReferenceBranch] nvarchar(max) '$.OptionalReferenceBranch' AS JSON,
    [RequiredReferenceBranch] nvarchar(max) '$.RequiredReferenceBranch' AS JSON
) AS [c]
""");
    }

    public override async Task SelectMany_required_trunk_reference_branch_collection(bool async)
    {
        await base.SelectMany_required_trunk_reference_branch_collection(async);

        AssertSql(
            """
SELECT [r].[Id], [c].[Name], [c].[CollectionLeaf], [c].[OptionalReferenceLeaf], [c].[RequiredReferenceLeaf]
FROM [RootEntities] AS [r]
CROSS APPLY OPENJSON([r].[RequiredReferenceTrunk], '$.CollectionBranch') WITH (
    [Name] nvarchar(max) '$.Name',
    [CollectionLeaf] nvarchar(max) '$.CollectionLeaf' AS JSON,
    [OptionalReferenceLeaf] nvarchar(max) '$.OptionalReferenceLeaf' AS JSON,
    [RequiredReferenceLeaf] nvarchar(max) '$.RequiredReferenceLeaf' AS JSON
) AS [c]
""");
    }

    public override async Task SelectMany_optional_trunk_reference_branch_collection(bool async)
    {
        await base.SelectMany_optional_trunk_reference_branch_collection(async);

        AssertSql(
            """
SELECT [r].[Id], [c].[Name], [c].[CollectionLeaf], [c].[OptionalReferenceLeaf], [c].[RequiredReferenceLeaf]
FROM [RootEntities] AS [r]
CROSS APPLY OPENJSON([r].[OptionalReferenceTrunk], '$.CollectionBranch') WITH (
    [Name] nvarchar(max) '$.Name',
    [CollectionLeaf] nvarchar(max) '$.CollectionLeaf' AS JSON,
    [OptionalReferenceLeaf] nvarchar(max) '$.OptionalReferenceLeaf' AS JSON,
    [RequiredReferenceLeaf] nvarchar(max) '$.RequiredReferenceLeaf' AS JSON
) AS [c]
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
