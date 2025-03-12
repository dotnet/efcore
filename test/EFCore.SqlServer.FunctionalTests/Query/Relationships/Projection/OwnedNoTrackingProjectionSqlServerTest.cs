// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Projection;

public class OwnedNoTrackingProjectionSqlServerTest
    : OwnedNoTrackingProjectionRelationalTestBase<OwnedRelationshipsSqlServerFixture>
{
    public OwnedNoTrackingProjectionSqlServerTest(OwnedRelationshipsSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
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
SELECT [r].[Id], [s0].[RelationshipsRootEntityId], [s0].[Id1], [s0].[Name], [s0].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s0].[RelationshipsTrunkEntityId1], [s0].[Id10], [s0].[Name0], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityId1], [s0].[RelationshipsBranchEntityId1], [s0].[Id100], [s0].[Name00], [s0].[OptionalReferenceLeaf_Name], [s0].[RequiredReferenceLeaf_Name], [s0].[OptionalReferenceBranch_Name], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId0], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityId10], [s0].[Id11], [s0].[Name1], [s0].[OptionalReferenceBranch_OptionalReferenceLeaf_Name], [s0].[OptionalReferenceBranch_RequiredReferenceLeaf_Name], [s0].[RequiredReferenceBranch_Name], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId1], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityId11], [s0].[Id12], [s0].[Name2], [s0].[RequiredReferenceBranch_OptionalReferenceLeaf_Name], [s0].[RequiredReferenceBranch_RequiredReferenceLeaf_Name]
FROM [RootEntities] AS [r]
LEFT JOIN (
    SELECT [r0].[RelationshipsRootEntityId], [r0].[Id1], [r0].[Name], [s].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s].[RelationshipsTrunkEntityId1], [s].[Id1] AS [Id10], [s].[Name] AS [Name0], [s].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s].[RelationshipsBranchEntityRelationshipsTrunkEntityId1], [s].[RelationshipsBranchEntityId1], [s].[Id10] AS [Id100], [s].[Name0] AS [Name00], [s].[OptionalReferenceLeaf_Name], [s].[RequiredReferenceLeaf_Name], [r0].[OptionalReferenceBranch_Name], [r3].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AS [RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId0], [r3].[RelationshipsBranchEntityRelationshipsTrunkEntityId1] AS [RelationshipsBranchEntityRelationshipsTrunkEntityId10], [r3].[Id1] AS [Id11], [r3].[Name] AS [Name1], [r0].[OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r0].[OptionalReferenceBranch_RequiredReferenceLeaf_Name], [r0].[RequiredReferenceBranch_Name], [r4].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AS [RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId1], [r4].[RelationshipsBranchEntityRelationshipsTrunkEntityId1] AS [RelationshipsBranchEntityRelationshipsTrunkEntityId11], [r4].[Id1] AS [Id12], [r4].[Name] AS [Name2], [r0].[RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r0].[RequiredReferenceBranch_RequiredReferenceLeaf_Name]
    FROM [Root_CollectionTrunk] AS [r0]
    LEFT JOIN (
        SELECT [r1].[RelationshipsTrunkEntityRelationshipsRootEntityId], [r1].[RelationshipsTrunkEntityId1], [r1].[Id1], [r1].[Name], [r2].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r2].[RelationshipsBranchEntityRelationshipsTrunkEntityId1], [r2].[RelationshipsBranchEntityId1], [r2].[Id1] AS [Id10], [r2].[Name] AS [Name0], [r1].[OptionalReferenceLeaf_Name], [r1].[RequiredReferenceLeaf_Name]
        FROM [Root_CollectionTrunk_CollectionBranch] AS [r1]
        LEFT JOIN [Root_CollectionTrunk_CollectionBranch_CollectionLeaf] AS [r2] ON [r1].[RelationshipsTrunkEntityRelationshipsRootEntityId] = [r2].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AND [r1].[RelationshipsTrunkEntityId1] = [r2].[RelationshipsBranchEntityRelationshipsTrunkEntityId1] AND [r1].[Id1] = [r2].[RelationshipsBranchEntityId1]
    ) AS [s] ON [r0].[RelationshipsRootEntityId] = [s].[RelationshipsTrunkEntityRelationshipsRootEntityId] AND [r0].[Id1] = [s].[RelationshipsTrunkEntityId1]
    LEFT JOIN [Root_CollectionTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r3] ON CASE
        WHEN [r0].[OptionalReferenceBranch_Name] IS NOT NULL THEN [r0].[RelationshipsRootEntityId]
    END = [r3].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AND CASE
        WHEN [r0].[OptionalReferenceBranch_Name] IS NOT NULL THEN [r0].[Id1]
    END = [r3].[RelationshipsBranchEntityRelationshipsTrunkEntityId1]
    LEFT JOIN [Root_CollectionTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r4] ON [r0].[RelationshipsRootEntityId] = [r4].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AND [r0].[Id1] = [r4].[RelationshipsBranchEntityRelationshipsTrunkEntityId1]
) AS [s0] ON [r].[Id] = [s0].[RelationshipsRootEntityId]
ORDER BY [r].[Id], [s0].[RelationshipsRootEntityId], [s0].[Id1], [s0].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s0].[RelationshipsTrunkEntityId1], [s0].[Id10], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityId1], [s0].[RelationshipsBranchEntityId1], [s0].[Id100], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId0], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityId10], [s0].[Id11], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId1], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityId11]
""");
    }

    public override async Task Select_branch_required_collection(bool async)
    {
        await base.Select_branch_required_collection(async);

        AssertSql(
"""
SELECT [r].[Id], [s].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s].[Id1], [s].[Name], [s].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s].[RelationshipsBranchEntityId1], [s].[Id10], [s].[Name0], [s].[OptionalReferenceLeaf_Name], [s].[RequiredReferenceLeaf_Name]
FROM [RootEntities] AS [r]
LEFT JOIN (
    SELECT [r0].[RelationshipsTrunkEntityRelationshipsRootEntityId], [r0].[Id1], [r0].[Name], [r1].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r1].[RelationshipsBranchEntityId1], [r1].[Id1] AS [Id10], [r1].[Name] AS [Name0], [r0].[OptionalReferenceLeaf_Name], [r0].[RequiredReferenceLeaf_Name]
    FROM [Root_RequiredReferenceTrunk_CollectionBranch] AS [r0]
    LEFT JOIN [Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r1] ON [r0].[RelationshipsTrunkEntityRelationshipsRootEntityId] = [r1].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AND [r0].[Id1] = [r1].[RelationshipsBranchEntityId1]
) AS [s] ON [r].[Id] = [s].[RelationshipsTrunkEntityRelationshipsRootEntityId]
ORDER BY [r].[Id], [s].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s].[Id1], [s].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s].[RelationshipsBranchEntityId1]
""");
    }

    public override async Task Select_branch_optional_collection(bool async)
    {
        await base.Select_branch_optional_collection(async);

        AssertSql(
            """
SELECT [r].[Id], [s].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s].[Id1], [s].[Name], [s].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s].[RelationshipsBranchEntityId1], [s].[Id10], [s].[Name0], [s].[OptionalReferenceLeaf_Name], [s].[RequiredReferenceLeaf_Name]
FROM [RootEntities] AS [r]
LEFT JOIN (
    SELECT [r0].[RelationshipsTrunkEntityRelationshipsRootEntityId], [r0].[Id1], [r0].[Name], [r1].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r1].[RelationshipsBranchEntityId1], [r1].[Id1] AS [Id10], [r1].[Name] AS [Name0], [r0].[OptionalReferenceLeaf_Name], [r0].[RequiredReferenceLeaf_Name]
    FROM [Root_RequiredReferenceTrunk_CollectionBranch] AS [r0]
    LEFT JOIN [Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r1] ON [r0].[RelationshipsTrunkEntityRelationshipsRootEntityId] = [r1].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AND [r0].[Id1] = [r1].[RelationshipsBranchEntityId1]
) AS [s] ON [r].[Id] = [s].[RelationshipsTrunkEntityRelationshipsRootEntityId]
ORDER BY [r].[Id], [s].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s].[Id1], [s].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s].[RelationshipsBranchEntityId1]
""");
    }

    public override async Task Select_multiple_branch_leaf(bool async)
    {
        await base.Select_multiple_branch_leaf(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_Name], [r0].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r0].[Id1], [r0].[Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf_Name], [r1].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r1].[Id1], [r1].[Name], [s].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s].[Id1], [s].[Name], [s].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s].[RelationshipsBranchEntityId1], [s].[Id10], [s].[Name0], [s].[OptionalReferenceLeaf_Name], [s].[RequiredReferenceLeaf_Name]
FROM [RootEntities] AS [r]
LEFT JOIN [Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r0] ON [r].[Id] = [r0].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN [Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r1] ON [r].[Id] = [r1].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN (
    SELECT [r2].[RelationshipsTrunkEntityRelationshipsRootEntityId], [r2].[Id1], [r2].[Name], [r3].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r3].[RelationshipsBranchEntityId1], [r3].[Id1] AS [Id10], [r3].[Name] AS [Name0], [r2].[OptionalReferenceLeaf_Name], [r2].[RequiredReferenceLeaf_Name]
    FROM [Root_RequiredReferenceTrunk_CollectionBranch] AS [r2]
    LEFT JOIN [Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r3] ON [r2].[RelationshipsTrunkEntityRelationshipsRootEntityId] = [r3].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AND [r2].[Id1] = [r3].[RelationshipsBranchEntityId1]
) AS [s] ON [r].[Id] = [s].[RelationshipsTrunkEntityRelationshipsRootEntityId]
ORDER BY [r].[Id], [r0].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r0].[Id1], [r1].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r1].[Id1], [s].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s].[Id1], [s].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s].[RelationshipsBranchEntityId1]
""");
    }

    public override async Task Select_subquery_root_set_trunk_FirstOrDefault_collection(bool async)
    {
        await base.Select_subquery_root_set_trunk_FirstOrDefault_collection(async);

        AssertSql(
            """
SELECT [r].[Id], [r3].[Id], [s].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s].[Id1], [s].[Name], [s].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s].[RelationshipsBranchEntityId1], [s].[Id10], [s].[Name0], [s].[OptionalReferenceLeaf_Name], [s].[RequiredReferenceLeaf_Name], [r3].[c]
FROM [RootEntities] AS [r]
OUTER APPLY (
    SELECT TOP(1) 1 AS [c], [r0].[Id]
    FROM [RootEntities] AS [r0]
    ORDER BY [r0].[Id]
) AS [r3]
LEFT JOIN (
    SELECT [r1].[RelationshipsTrunkEntityRelationshipsRootEntityId], [r1].[Id1], [r1].[Name], [r2].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r2].[RelationshipsBranchEntityId1], [r2].[Id1] AS [Id10], [r2].[Name] AS [Name0], [r1].[OptionalReferenceLeaf_Name], [r1].[RequiredReferenceLeaf_Name]
    FROM [Root_RequiredReferenceTrunk_CollectionBranch] AS [r1]
    LEFT JOIN [Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r2] ON [r1].[RelationshipsTrunkEntityRelationshipsRootEntityId] = [r2].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AND [r1].[Id1] = [r2].[RelationshipsBranchEntityId1]
) AS [s] ON [r3].[Id] = [s].[RelationshipsTrunkEntityRelationshipsRootEntityId]
ORDER BY [r].[Id], [r3].[Id], [s].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s].[Id1], [s].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s].[RelationshipsBranchEntityId1]
""");
    }

    public override async Task Select_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(bool async)
    {
        await base.Select_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(async);

        AssertSql(
            """
SELECT [r].[Id], [r8].[Id], [s].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s].[Id1], [s].[Name], [s].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s].[RelationshipsBranchEntityId1], [s].[Id10], [s].[Name0], [s].[OptionalReferenceLeaf_Name], [s].[RequiredReferenceLeaf_Name], [r8].[RequiredReferenceTrunk_Name], [s0].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s0].[Id1], [s0].[Name], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s0].[RelationshipsBranchEntityId1], [s0].[Id10], [s0].[Name0], [s0].[OptionalReferenceLeaf_Name], [s0].[RequiredReferenceLeaf_Name], [r8].[RequiredReferenceTrunk_OptionalReferenceBranch_Name], [r5].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r5].[Id1], [r5].[Name], [r8].[RequiredReferenceTrunk_OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r8].[RequiredReferenceTrunk_OptionalReferenceBranch_RequiredReferenceLeaf_Name], [r8].[RequiredReferenceTrunk_RequiredReferenceBranch_Name], [r6].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r6].[Id1], [r6].[Name], [r8].[RequiredReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r8].[RequiredReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf_Name], [r7].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r7].[Id1], [r7].[Name], [r8].[RequiredReferenceTrunk_RequiredReferenceBranch_Name0], [r8].[c]
FROM [RootEntities] AS [r]
OUTER APPLY (
    SELECT TOP(1) [r0].[Id], [r0].[RequiredReferenceTrunk_Name], [r0].[RequiredReferenceTrunk_OptionalReferenceBranch_Name], [r0].[RequiredReferenceTrunk_OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r0].[RequiredReferenceTrunk_OptionalReferenceBranch_RequiredReferenceLeaf_Name], [r0].[RequiredReferenceTrunk_RequiredReferenceBranch_Name], [r0].[RequiredReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r0].[RequiredReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf_Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_Name] AS [RequiredReferenceTrunk_RequiredReferenceBranch_Name0], 1 AS [c]
    FROM [RootEntities] AS [r0]
    ORDER BY [r0].[Id]
) AS [r8]
LEFT JOIN (
    SELECT [r1].[RelationshipsTrunkEntityRelationshipsRootEntityId], [r1].[Id1], [r1].[Name], [r2].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r2].[RelationshipsBranchEntityId1], [r2].[Id1] AS [Id10], [r2].[Name] AS [Name0], [r1].[OptionalReferenceLeaf_Name], [r1].[RequiredReferenceLeaf_Name]
    FROM [Root_RequiredReferenceTrunk_CollectionBranch] AS [r1]
    LEFT JOIN [Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r2] ON [r1].[RelationshipsTrunkEntityRelationshipsRootEntityId] = [r2].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AND [r1].[Id1] = [r2].[RelationshipsBranchEntityId1]
) AS [s] ON [r].[Id] = [s].[RelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN (
    SELECT [r3].[RelationshipsTrunkEntityRelationshipsRootEntityId], [r3].[Id1], [r3].[Name], [r4].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r4].[RelationshipsBranchEntityId1], [r4].[Id1] AS [Id10], [r4].[Name] AS [Name0], [r3].[OptionalReferenceLeaf_Name], [r3].[RequiredReferenceLeaf_Name]
    FROM [Root_RequiredReferenceTrunk_CollectionBranch] AS [r3]
    LEFT JOIN [Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r4] ON [r3].[RelationshipsTrunkEntityRelationshipsRootEntityId] = [r4].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AND [r3].[Id1] = [r4].[RelationshipsBranchEntityId1]
) AS [s0] ON [r8].[Id] = [s0].[RelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN [Root_RequiredReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r5] ON CASE
    WHEN [r8].[RequiredReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r8].[Id]
END = [r5].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN [Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r6] ON [r8].[Id] = [r6].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN [Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r7] ON [r8].[Id] = [r7].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
ORDER BY [r].[Id], [r8].[Id], [s].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s].[Id1], [s].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s].[RelationshipsBranchEntityId1], [s].[Id10], [s0].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s0].[Id1], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s0].[RelationshipsBranchEntityId1], [s0].[Id10], [r5].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r5].[Id1], [r6].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r6].[Id1], [r7].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
""");
    }

    public override async Task Select_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(bool async)
    {
        await base.Select_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(async);

        AssertSql(
            """
SELECT [r].[Id], [r3].[Id], [s].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s].[Id1], [s].[Name], [s].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s].[RelationshipsBranchEntityId1], [s].[Id10], [s].[Name0], [s].[OptionalReferenceLeaf_Name], [s].[RequiredReferenceLeaf_Name], [r3].[c]
FROM [RootEntities] AS [r]
OUTER APPLY (
    SELECT TOP(1) 1 AS [c], [r0].[Id]
    FROM [RootEntities] AS [r0]
    ORDER BY [r0].[Id]
) AS [r3]
LEFT JOIN (
    SELECT [r1].[RelationshipsTrunkEntityRelationshipsRootEntityId], [r1].[Id1], [r1].[Name], [r2].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r2].[RelationshipsBranchEntityId1], [r2].[Id1] AS [Id10], [r2].[Name] AS [Name0], [r1].[OptionalReferenceLeaf_Name], [r1].[RequiredReferenceLeaf_Name]
    FROM [Root_RequiredReferenceTrunk_CollectionBranch] AS [r1]
    LEFT JOIN [Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r2] ON [r1].[RelationshipsTrunkEntityRelationshipsRootEntityId] = [r2].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AND [r1].[Id1] = [r2].[RelationshipsBranchEntityId1]
) AS [s] ON [r].[Id] = [s].[RelationshipsTrunkEntityRelationshipsRootEntityId]
ORDER BY [r].[Id], [r3].[Id], [s].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s].[Id1], [s].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s].[RelationshipsBranchEntityId1]
""");
    }

    public override async Task SelectMany_trunk_collection(bool async)
    {
        await base.SelectMany_trunk_collection(async);

        AssertSql(
            """
SELECT [r0].[RelationshipsRootEntityId], [r0].[Id1], [r0].[Name], [r].[Id], [s].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s].[RelationshipsTrunkEntityId1], [s].[Id1], [s].[Name], [s].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s].[RelationshipsBranchEntityRelationshipsTrunkEntityId1], [s].[RelationshipsBranchEntityId1], [s].[Id10], [s].[Name0], [s].[OptionalReferenceLeaf_Name], [s].[RequiredReferenceLeaf_Name], [r0].[OptionalReferenceBranch_Name], [r3].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r3].[RelationshipsBranchEntityRelationshipsTrunkEntityId1], [r3].[Id1], [r3].[Name], [r0].[OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r0].[OptionalReferenceBranch_RequiredReferenceLeaf_Name], [r0].[RequiredReferenceBranch_Name], [r4].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r4].[RelationshipsBranchEntityRelationshipsTrunkEntityId1], [r4].[Id1], [r4].[Name], [r0].[RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r0].[RequiredReferenceBranch_RequiredReferenceLeaf_Name]
FROM [RootEntities] AS [r]
INNER JOIN [Root_CollectionTrunk] AS [r0] ON [r].[Id] = [r0].[RelationshipsRootEntityId]
LEFT JOIN (
    SELECT [r1].[RelationshipsTrunkEntityRelationshipsRootEntityId], [r1].[RelationshipsTrunkEntityId1], [r1].[Id1], [r1].[Name], [r2].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r2].[RelationshipsBranchEntityRelationshipsTrunkEntityId1], [r2].[RelationshipsBranchEntityId1], [r2].[Id1] AS [Id10], [r2].[Name] AS [Name0], [r1].[OptionalReferenceLeaf_Name], [r1].[RequiredReferenceLeaf_Name]
    FROM [Root_CollectionTrunk_CollectionBranch] AS [r1]
    LEFT JOIN [Root_CollectionTrunk_CollectionBranch_CollectionLeaf] AS [r2] ON [r1].[RelationshipsTrunkEntityRelationshipsRootEntityId] = [r2].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AND [r1].[RelationshipsTrunkEntityId1] = [r2].[RelationshipsBranchEntityRelationshipsTrunkEntityId1] AND [r1].[Id1] = [r2].[RelationshipsBranchEntityId1]
) AS [s] ON [r0].[RelationshipsRootEntityId] = [s].[RelationshipsTrunkEntityRelationshipsRootEntityId] AND [r0].[Id1] = [s].[RelationshipsTrunkEntityId1]
LEFT JOIN [Root_CollectionTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r3] ON CASE
    WHEN [r0].[OptionalReferenceBranch_Name] IS NOT NULL THEN [r0].[RelationshipsRootEntityId]
END = [r3].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AND CASE
    WHEN [r0].[OptionalReferenceBranch_Name] IS NOT NULL THEN [r0].[Id1]
END = [r3].[RelationshipsBranchEntityRelationshipsTrunkEntityId1]
LEFT JOIN [Root_CollectionTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r4] ON [r0].[RelationshipsRootEntityId] = [r4].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AND [r0].[Id1] = [r4].[RelationshipsBranchEntityRelationshipsTrunkEntityId1]
ORDER BY [r].[Id], [r0].[RelationshipsRootEntityId], [r0].[Id1], [s].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s].[RelationshipsTrunkEntityId1], [s].[Id1], [s].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s].[RelationshipsBranchEntityRelationshipsTrunkEntityId1], [s].[RelationshipsBranchEntityId1], [s].[Id10], [r3].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r3].[RelationshipsBranchEntityRelationshipsTrunkEntityId1], [r3].[Id1], [r4].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r4].[RelationshipsBranchEntityRelationshipsTrunkEntityId1]
""");
    }

    public override async Task SelectMany_required_trunk_reference_branch_collection(bool async)
    {
        await base.SelectMany_required_trunk_reference_branch_collection(async);

        AssertSql(
            """
SELECT [r0].[RelationshipsTrunkEntityRelationshipsRootEntityId], [r0].[Id1], [r0].[Name], [r].[Id], [r1].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r1].[RelationshipsBranchEntityId1], [r1].[Id1], [r1].[Name], [r0].[OptionalReferenceLeaf_Name], [r0].[RequiredReferenceLeaf_Name]
FROM [RootEntities] AS [r]
INNER JOIN [Root_RequiredReferenceTrunk_CollectionBranch] AS [r0] ON [r].[Id] = [r0].[RelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN [Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r1] ON [r0].[RelationshipsTrunkEntityRelationshipsRootEntityId] = [r1].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AND [r0].[Id1] = [r1].[RelationshipsBranchEntityId1]
ORDER BY [r].[Id], [r0].[RelationshipsTrunkEntityRelationshipsRootEntityId], [r0].[Id1], [r1].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r1].[RelationshipsBranchEntityId1]
""");
    }

    public override async Task SelectMany_optional_trunk_reference_branch_collection(bool async)
    {
        await base.SelectMany_optional_trunk_reference_branch_collection(async);

        AssertSql(
            """
SELECT [r0].[RelationshipsTrunkEntityRelationshipsRootEntityId], [r0].[Id1], [r0].[Name], [r].[Id], [r1].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r1].[RelationshipsBranchEntityId1], [r1].[Id1], [r1].[Name], [r0].[OptionalReferenceLeaf_Name], [r0].[RequiredReferenceLeaf_Name]
FROM [RootEntities] AS [r]
INNER JOIN [Root_OptionalReferenceTrunk_CollectionBranch] AS [r0] ON CASE
    WHEN [r].[OptionalReferenceTrunk_Name] IS NOT NULL THEN [r].[Id]
END = [r0].[RelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN [Root_OptionalReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r1] ON [r0].[RelationshipsTrunkEntityRelationshipsRootEntityId] = [r1].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AND [r0].[Id1] = [r1].[RelationshipsBranchEntityId1]
ORDER BY [r].[Id], [r0].[RelationshipsTrunkEntityRelationshipsRootEntityId], [r0].[Id1], [r1].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r1].[RelationshipsBranchEntityId1]
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
