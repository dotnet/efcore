﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.InProjection;

public class EntityRelationshipsInProjectionQuerySqlServerTest
    : EntityRelationshipsInProjectionQueryRelationalTestBase<EntityRelationshipsQuerySqlServerFixture>
{
    public EntityRelationshipsInProjectionQuerySqlServerTest(EntityRelationshipsQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Project_everything(bool async)
    {
        await base.Project_everything(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [t].[Id], [t].[CollectionRootId], [t].[Name], [t].[OptionalReferenceBranchId], [t].[RequiredReferenceBranchId], [b].[Id], [b].[CollectionTrunkId], [b].[Name], [b].[OptionalReferenceLeafId], [b].[RequiredReferenceLeafId], [l].[Id], [l].[CollectionBranchId], [l].[Name]
FROM [RootEntities] AS [r]
INNER JOIN [TrunkEntities] AS [t] ON [r].[Id] = [t].[Id]
INNER JOIN [BranchEntities] AS [b] ON [t].[Id] = [b].[Id]
INNER JOIN [LeafEntities] AS [l] ON [b].[Id] = [l].[Id]
""");
    }

    public override async Task Project_root(bool async)
    {
        await base.Project_root(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId]
FROM [RootEntities] AS [r]
""");
    }

    public override async Task Project_trunk_optional(bool async)
    {
        await base.Project_trunk_optional(async);

        AssertSql(
            """
SELECT [t].[Id], [t].[CollectionRootId], [t].[Name], [t].[OptionalReferenceBranchId], [t].[RequiredReferenceBranchId]
FROM [RootEntities] AS [r]
LEFT JOIN [TrunkEntities] AS [t] ON [r].[OptionalReferenceTrunkId] = [t].[Id]
ORDER BY [r].[Id]
""");
    }

    public override async Task Project_trunk_required(bool async)
    {
        await base.Project_trunk_required(async);

        AssertSql(
            """
SELECT [t].[Id], [t].[CollectionRootId], [t].[Name], [t].[OptionalReferenceBranchId], [t].[RequiredReferenceBranchId]
FROM [RootEntities] AS [r]
INNER JOIN [TrunkEntities] AS [t] ON [r].[RequiredReferenceTrunkId] = [t].[Id]
ORDER BY [r].[Id]
""");
    }

    public override async Task Project_trunk_collection(bool async)
    {
        await base.Project_trunk_collection(async);

        AssertSql(
            """
SELECT [r].[Id], [t].[Id], [t].[CollectionRootId], [t].[Name], [t].[OptionalReferenceBranchId], [t].[RequiredReferenceBranchId]
FROM [RootEntities] AS [r]
LEFT JOIN [TrunkEntities] AS [t] ON [r].[Id] = [t].[CollectionRootId]
ORDER BY [r].[Id]
""");
    }

    public override async Task Project_branch_required_required(bool async)
    {
        await base.Project_branch_required_required(async);

        AssertSql(
"""
SELECT [b].[Id], [b].[CollectionTrunkId], [b].[Name], [b].[OptionalReferenceLeafId], [b].[RequiredReferenceLeafId]
FROM [RootEntities] AS [r]
INNER JOIN [TrunkEntities] AS [t] ON [r].[RequiredReferenceTrunkId] = [t].[Id]
INNER JOIN [BranchEntities] AS [b] ON [t].[RequiredReferenceBranchId] = [b].[Id]
ORDER BY [r].[Id]
""");
    }

    public override async Task Project_branch_required_optional(bool async)
    {
        await base.Project_branch_required_optional(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[CollectionTrunkId], [b].[Name], [b].[OptionalReferenceLeafId], [b].[RequiredReferenceLeafId]
FROM [RootEntities] AS [r]
INNER JOIN [TrunkEntities] AS [t] ON [r].[RequiredReferenceTrunkId] = [t].[Id]
LEFT JOIN [BranchEntities] AS [b] ON [t].[OptionalReferenceBranchId] = [b].[Id]
ORDER BY [r].[Id]
""");
    }

    public override async Task Project_branch_required_collection(bool async)
    {
        await base.Project_branch_required_collection(async);

        AssertSql(
            """
SELECT [r].[Id], [t].[Id], [b].[Id], [b].[CollectionTrunkId], [b].[Name], [b].[OptionalReferenceLeafId], [b].[RequiredReferenceLeafId]
FROM [RootEntities] AS [r]
INNER JOIN [TrunkEntities] AS [t] ON [r].[RequiredReferenceTrunkId] = [t].[Id]
LEFT JOIN [BranchEntities] AS [b] ON [t].[Id] = [b].[CollectionTrunkId]
ORDER BY [r].[Id], [t].[Id]
""");
    }

    public override async Task Project_branch_optional_required(bool async)
    {
        await base.Project_branch_optional_required(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[CollectionTrunkId], [b].[Name], [b].[OptionalReferenceLeafId], [b].[RequiredReferenceLeafId]
FROM [RootEntities] AS [r]
INNER JOIN [TrunkEntities] AS [t] ON [r].[RequiredReferenceTrunkId] = [t].[Id]
INNER JOIN [BranchEntities] AS [b] ON [t].[RequiredReferenceBranchId] = [b].[Id]
ORDER BY [r].[Id]
""");
    }

    public override async Task Project_branch_optional_optional(bool async)
    {
        await base.Project_branch_optional_optional(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[CollectionTrunkId], [b].[Name], [b].[OptionalReferenceLeafId], [b].[RequiredReferenceLeafId]
FROM [RootEntities] AS [r]
INNER JOIN [TrunkEntities] AS [t] ON [r].[RequiredReferenceTrunkId] = [t].[Id]
LEFT JOIN [BranchEntities] AS [b] ON [t].[OptionalReferenceBranchId] = [b].[Id]
ORDER BY [r].[Id]
""");
    }

    public override async Task Project_branch_optional_collection(bool async)
    {
        await base.Project_branch_optional_collection(async);

        AssertSql(
            """
SELECT [r].[Id], [t].[Id], [b].[Id], [b].[CollectionTrunkId], [b].[Name], [b].[OptionalReferenceLeafId], [b].[RequiredReferenceLeafId]
FROM [RootEntities] AS [r]
INNER JOIN [TrunkEntities] AS [t] ON [r].[RequiredReferenceTrunkId] = [t].[Id]
LEFT JOIN [BranchEntities] AS [b] ON [t].[Id] = [b].[CollectionTrunkId]
ORDER BY [r].[Id], [t].[Id]
""");
    }

    public override async Task Project_root_duplicated(bool async)
    {
        await base.Project_root_duplicated(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId]
FROM [RootEntities] AS [r]
""");
    }

    public override async Task Project_trunk_and_branch_duplicated(bool async)
    {
        await base.Project_trunk_and_branch_duplicated(async);

        AssertSql(
            """
SELECT [t].[Id], [t].[CollectionRootId], [t].[Name], [t].[OptionalReferenceBranchId], [t].[RequiredReferenceBranchId], [b].[Id], [b].[CollectionTrunkId], [b].[Name], [b].[OptionalReferenceLeafId], [b].[RequiredReferenceLeafId]
FROM [RootEntities] AS [r]
LEFT JOIN [TrunkEntities] AS [t] ON [r].[OptionalReferenceTrunkId] = [t].[Id]
LEFT JOIN [BranchEntities] AS [b] ON [t].[RequiredReferenceBranchId] = [b].[Id]
ORDER BY [r].[Id]
""");
    }

    public override async Task Project_trunk_and_trunk_duplicated(bool async)
    {
        await base.Project_trunk_and_trunk_duplicated(async);

        AssertSql(
            """
SELECT [t].[Id], [t].[CollectionRootId], [t].[Name], [t].[OptionalReferenceBranchId], [t].[RequiredReferenceBranchId], [l].[Id], [l].[CollectionBranchId], [l].[Name]
FROM [RootEntities] AS [r]
INNER JOIN [TrunkEntities] AS [t] ON [r].[RequiredReferenceTrunkId] = [t].[Id]
LEFT JOIN [BranchEntities] AS [b] ON [t].[OptionalReferenceBranchId] = [b].[Id]
LEFT JOIN [LeafEntities] AS [l] ON [b].[RequiredReferenceLeafId] = [l].[Id]
ORDER BY [r].[Id]
""");
    }

    public override async Task Project_multiple_branch_leaf(bool async)
    {
        await base.Project_multiple_branch_leaf(async);

        AssertSql(
            """
SELECT [r].[Id], [b].[Id], [b].[CollectionTrunkId], [b].[Name], [b].[OptionalReferenceLeafId], [b].[RequiredReferenceLeafId], [l].[Id], [l].[CollectionBranchId], [l].[Name], [t].[Id], [l0].[Id], [l0].[CollectionBranchId], [l0].[Name], [b0].[Id], [b0].[CollectionTrunkId], [b0].[Name], [b0].[OptionalReferenceLeafId], [b0].[RequiredReferenceLeafId]
FROM [RootEntities] AS [r]
INNER JOIN [TrunkEntities] AS [t] ON [r].[RequiredReferenceTrunkId] = [t].[Id]
INNER JOIN [BranchEntities] AS [b] ON [t].[RequiredReferenceBranchId] = [b].[Id]
LEFT JOIN [LeafEntities] AS [l] ON [b].[OptionalReferenceLeafId] = [l].[Id]
LEFT JOIN [LeafEntities] AS [l0] ON [b].[Id] = [l0].[CollectionBranchId]
LEFT JOIN [BranchEntities] AS [b0] ON [t].[Id] = [b0].[CollectionTrunkId]
ORDER BY [r].[Id], [t].[Id], [b].[Id], [l].[Id], [l0].[Id]
""");
    }

    public override async Task Project_leaf_trunk_root(bool async)
    {
        await base.Project_leaf_trunk_root(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[CollectionBranchId], [l].[Name], [t].[Id], [t].[CollectionRootId], [t].[Name], [t].[OptionalReferenceBranchId], [t].[RequiredReferenceBranchId], [r].[Id], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId]
FROM [RootEntities] AS [r]
INNER JOIN [TrunkEntities] AS [t] ON [r].[RequiredReferenceTrunkId] = [t].[Id]
INNER JOIN [BranchEntities] AS [b] ON [t].[RequiredReferenceBranchId] = [b].[Id]
INNER JOIN [LeafEntities] AS [l] ON [b].[RequiredReferenceLeafId] = [l].[Id]
""");
    }

    public override async Task Project_subquery_root_set_required_trunk_FirstOrDefault_branch(bool async)
    {
        await base.Project_subquery_root_set_required_trunk_FirstOrDefault_branch(async);

        AssertSql(
            """
SELECT [s].[Id], [s].[CollectionTrunkId], [s].[Name], [s].[OptionalReferenceLeafId], [s].[RequiredReferenceLeafId]
FROM [RootEntities] AS [r]
OUTER APPLY (
    SELECT TOP(1) [b].[Id], [b].[CollectionTrunkId], [b].[Name], [b].[OptionalReferenceLeafId], [b].[RequiredReferenceLeafId]
    FROM [RootEntities] AS [r0]
    INNER JOIN [TrunkEntities] AS [t] ON [r0].[RequiredReferenceTrunkId] = [t].[Id]
    INNER JOIN [BranchEntities] AS [b] ON [t].[RequiredReferenceBranchId] = [b].[Id]
    ORDER BY [r0].[Id]
) AS [s]
ORDER BY [r].[Id]
""");
    }

    public override async Task Project_subquery_root_set_optional_trunk_FirstOrDefault_branch(bool async)
    {
        await base.Project_subquery_root_set_optional_trunk_FirstOrDefault_branch(async);

        AssertSql(
            """
SELECT [s].[Id], [s].[CollectionTrunkId], [s].[Name], [s].[OptionalReferenceLeafId], [s].[RequiredReferenceLeafId]
FROM [RootEntities] AS [r]
OUTER APPLY (
    SELECT TOP(1) [b].[Id], [b].[CollectionTrunkId], [b].[Name], [b].[OptionalReferenceLeafId], [b].[RequiredReferenceLeafId]
    FROM [RootEntities] AS [r0]
    LEFT JOIN [TrunkEntities] AS [t] ON [r0].[OptionalReferenceTrunkId] = [t].[Id]
    LEFT JOIN [BranchEntities] AS [b] ON [t].[OptionalReferenceBranchId] = [b].[Id]
    ORDER BY [r0].[Id]
) AS [s]
ORDER BY [r].[Id]
""");
    }

    public override async Task Project_subquery_root_set_trunk_FirstOrDefault_collection(bool async)
    {
        await base.Project_subquery_root_set_trunk_FirstOrDefault_collection(async);

        AssertSql(
            """
SELECT [r].[Id], [s].[Id], [s].[Id0], [b].[Id], [b].[CollectionTrunkId], [b].[Name], [b].[OptionalReferenceLeafId], [b].[RequiredReferenceLeafId], [s].[c]
FROM [RootEntities] AS [r]
OUTER APPLY (
    SELECT TOP(1) 1 AS [c], [r0].[Id], [t].[Id] AS [Id0]
    FROM [RootEntities] AS [r0]
    INNER JOIN [TrunkEntities] AS [t] ON [r0].[RequiredReferenceTrunkId] = [t].[Id]
    ORDER BY [r0].[Id]
) AS [s]
LEFT JOIN [BranchEntities] AS [b] ON [s].[Id0] = [b].[CollectionTrunkId]
ORDER BY [r].[Id], [s].[Id], [s].[Id0]
""");
    }

    public override async Task Project_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(bool async)
    {
        await base.Project_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(async);

        AssertSql(
            """
SELECT [r].[Id], [t].[Id], [b].[Id], [s].[Id1], [s].[Id], [s].[Id0], [b1].[Id], [b1].[CollectionTrunkId], [b1].[Name], [b1].[OptionalReferenceLeafId], [b1].[RequiredReferenceLeafId], [s].[CollectionRootId], [s].[Name], [s].[OptionalReferenceBranchId], [s].[RequiredReferenceBranchId], [s].[CollectionTrunkId], [s].[Name0], [s].[OptionalReferenceLeafId], [s].[RequiredReferenceLeafId], [s].[Name1], [s].[c]
FROM [RootEntities] AS [r]
INNER JOIN [TrunkEntities] AS [t] ON [r].[RequiredReferenceTrunkId] = [t].[Id]
INNER JOIN [BranchEntities] AS [b] ON [t].[RequiredReferenceBranchId] = [b].[Id]
OUTER APPLY (
    SELECT TOP(1) [t0].[Id], [t0].[CollectionRootId], [t0].[Name], [t0].[OptionalReferenceBranchId], [t0].[RequiredReferenceBranchId], [b0].[Id] AS [Id0], [b0].[CollectionTrunkId], [b0].[Name] AS [Name0], [b0].[OptionalReferenceLeafId], [b0].[RequiredReferenceLeafId], [b].[Name] AS [Name1], 1 AS [c], [r0].[Id] AS [Id1]
    FROM [RootEntities] AS [r0]
    INNER JOIN [TrunkEntities] AS [t0] ON [r0].[RequiredReferenceTrunkId] = [t0].[Id]
    INNER JOIN [BranchEntities] AS [b0] ON [t0].[RequiredReferenceBranchId] = [b0].[Id]
    ORDER BY [r0].[Id]
) AS [s]
LEFT JOIN [BranchEntities] AS [b1] ON [t].[Id] = [b1].[CollectionTrunkId]
ORDER BY [r].[Id], [t].[Id], [b].[Id], [s].[Id1], [s].[Id], [s].[Id0]
""");
    }

    public override async Task Project_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(bool async)
    {
        await base.Project_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(async);

        AssertSql(
            """
SELECT [r].[Id], [t].[Id], [r1].[Id], [b].[Id], [b].[CollectionTrunkId], [b].[Name], [b].[OptionalReferenceLeafId], [b].[RequiredReferenceLeafId], [r1].[c]
FROM [RootEntities] AS [r]
INNER JOIN [TrunkEntities] AS [t] ON [r].[RequiredReferenceTrunkId] = [t].[Id]
OUTER APPLY (
    SELECT TOP(1) 1 AS [c], [r0].[Id]
    FROM [RootEntities] AS [r0]
    ORDER BY [r0].[Id]
) AS [r1]
LEFT JOIN [BranchEntities] AS [b] ON [t].[Id] = [b].[CollectionTrunkId]
ORDER BY [r].[Id], [t].[Id], [r1].[Id]
""");
    }

    public override async Task SelectMany_trunk_collection(bool async)
    {
        await base.SelectMany_trunk_collection(async);

        AssertSql(
            """
SELECT [t].[Id], [t].[CollectionRootId], [t].[Name], [t].[OptionalReferenceBranchId], [t].[RequiredReferenceBranchId]
FROM [RootEntities] AS [r]
INNER JOIN [TrunkEntities] AS [t] ON [r].[Id] = [t].[CollectionRootId]
""");
    }

    public override async Task SelectMany_required_trunk_reference_branch_collection(bool async)
    {
        await base.SelectMany_required_trunk_reference_branch_collection(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[CollectionTrunkId], [b].[Name], [b].[OptionalReferenceLeafId], [b].[RequiredReferenceLeafId]
FROM [RootEntities] AS [r]
INNER JOIN [TrunkEntities] AS [t] ON [r].[RequiredReferenceTrunkId] = [t].[Id]
INNER JOIN [BranchEntities] AS [b] ON [t].[Id] = [b].[CollectionTrunkId]
""");
    }

    public override async Task SelectMany_optional_trunk_reference_branch_collection(bool async)
    {
        await base.SelectMany_optional_trunk_reference_branch_collection(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[CollectionTrunkId], [b].[Name], [b].[OptionalReferenceLeafId], [b].[RequiredReferenceLeafId]
FROM [RootEntities] AS [r]
LEFT JOIN [TrunkEntities] AS [t] ON [r].[OptionalReferenceTrunkId] = [t].[Id]
INNER JOIN [BranchEntities] AS [b] ON [t].[Id] = [b].[CollectionTrunkId]
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
