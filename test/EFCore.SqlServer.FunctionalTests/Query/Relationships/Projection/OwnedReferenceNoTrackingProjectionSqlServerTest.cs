// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Projection;

public class OwnedReferenceNoTrackingProjectionSqlServerTest
    : OwnedReferenceNoTrackingProjectionRelationalTestBase<OwnedRelationshipsSqlServerFixture>
{
    public OwnedReferenceNoTrackingProjectionSqlServerTest(OwnedRelationshipsSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
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
SELECT [r].[Id], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [s0].[RelationshipsRootEntityId], [s0].[Id1], [s0].[Name], [s0].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s0].[RelationshipsTrunkEntityId1], [s0].[Id10], [s0].[Name0], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityId1], [s0].[RelationshipsBranchEntityId1], [s0].[Id100], [s0].[Name00], [s0].[OptionalReferenceLeaf_Name], [s0].[RequiredReferenceLeaf_Name], [s0].[OptionalReferenceBranch_Name], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId0], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityId10], [s0].[Id11], [s0].[Name1], [s0].[OptionalReferenceBranch_OptionalReferenceLeaf_Name], [s0].[OptionalReferenceBranch_RequiredReferenceLeaf_Name], [s0].[RequiredReferenceBranch_Name], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId1], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityId11], [s0].[Id12], [s0].[Name2], [s0].[RequiredReferenceBranch_OptionalReferenceLeaf_Name], [s0].[RequiredReferenceBranch_RequiredReferenceLeaf_Name], [r].[OptionalReferenceTrunk_Name], [s1].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s1].[Id1], [s1].[Name], [s1].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s1].[RelationshipsBranchEntityId1], [s1].[Id10], [s1].[Name0], [s1].[OptionalReferenceLeaf_Name], [s1].[RequiredReferenceLeaf_Name], [r].[OptionalReferenceTrunk_OptionalReferenceBranch_Name], [r7].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r7].[Id1], [r7].[Name], [r].[OptionalReferenceTrunk_OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r].[OptionalReferenceTrunk_OptionalReferenceBranch_RequiredReferenceLeaf_Name], [r].[OptionalReferenceTrunk_RequiredReferenceBranch_Name], [r8].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r8].[Id1], [r8].[Name], [r].[OptionalReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r].[OptionalReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf_Name], [r].[RequiredReferenceTrunk_Name], [s2].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s2].[Id1], [s2].[Name], [s2].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s2].[RelationshipsBranchEntityId1], [s2].[Id10], [s2].[Name0], [s2].[OptionalReferenceLeaf_Name], [s2].[RequiredReferenceLeaf_Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_Name], [r11].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r11].[Id1], [r11].[Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_RequiredReferenceLeaf_Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_Name], [r12].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r12].[Id1], [r12].[Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf_Name]
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
LEFT JOIN (
    SELECT [r5].[RelationshipsTrunkEntityRelationshipsRootEntityId], [r5].[Id1], [r5].[Name], [r6].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r6].[RelationshipsBranchEntityId1], [r6].[Id1] AS [Id10], [r6].[Name] AS [Name0], [r5].[OptionalReferenceLeaf_Name], [r5].[RequiredReferenceLeaf_Name]
    FROM [Root_OptionalReferenceTrunk_CollectionBranch] AS [r5]
    LEFT JOIN [Root_OptionalReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r6] ON [r5].[RelationshipsTrunkEntityRelationshipsRootEntityId] = [r6].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AND [r5].[Id1] = [r6].[RelationshipsBranchEntityId1]
) AS [s1] ON CASE
    WHEN [r].[OptionalReferenceTrunk_Name] IS NOT NULL THEN [r].[Id]
END = [s1].[RelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN [Root_OptionalReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r7] ON CASE
    WHEN [r].[OptionalReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r7].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN [Root_OptionalReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r8] ON CASE
    WHEN [r].[OptionalReferenceTrunk_RequiredReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r8].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN (
    SELECT [r9].[RelationshipsTrunkEntityRelationshipsRootEntityId], [r9].[Id1], [r9].[Name], [r10].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r10].[RelationshipsBranchEntityId1], [r10].[Id1] AS [Id10], [r10].[Name] AS [Name0], [r9].[OptionalReferenceLeaf_Name], [r9].[RequiredReferenceLeaf_Name]
    FROM [Root_RequiredReferenceTrunk_CollectionBranch] AS [r9]
    LEFT JOIN [Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r10] ON [r9].[RelationshipsTrunkEntityRelationshipsRootEntityId] = [r10].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AND [r9].[Id1] = [r10].[RelationshipsBranchEntityId1]
) AS [s2] ON [r].[Id] = [s2].[RelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN [Root_RequiredReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r11] ON CASE
    WHEN [r].[RequiredReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r11].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN [Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r12] ON [r].[Id] = [r12].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
ORDER BY [r].[Id], [s0].[RelationshipsRootEntityId], [s0].[Id1], [s0].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s0].[RelationshipsTrunkEntityId1], [s0].[Id10], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityId1], [s0].[RelationshipsBranchEntityId1], [s0].[Id100], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId0], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityId10], [s0].[Id11], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId1], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityId11], [s0].[Id12], [s1].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s1].[Id1], [s1].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s1].[RelationshipsBranchEntityId1], [s1].[Id10], [r7].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r7].[Id1], [r8].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r8].[Id1], [s2].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s2].[Id1], [s2].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s2].[RelationshipsBranchEntityId1], [s2].[Id10], [r11].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r11].[Id1], [r12].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
""");
    }

    public override async Task Select_trunk_optional(bool async)
    {
        await base.Select_trunk_optional(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[OptionalReferenceTrunk_Name], [s].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s].[Id1], [s].[Name], [s].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s].[RelationshipsBranchEntityId1], [s].[Id10], [s].[Name0], [s].[OptionalReferenceLeaf_Name], [s].[RequiredReferenceLeaf_Name], [r].[OptionalReferenceTrunk_OptionalReferenceBranch_Name], [r2].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r2].[Id1], [r2].[Name], [r].[OptionalReferenceTrunk_OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r].[OptionalReferenceTrunk_OptionalReferenceBranch_RequiredReferenceLeaf_Name], [r].[OptionalReferenceTrunk_RequiredReferenceBranch_Name], [r3].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r3].[Id1], [r3].[Name], [r].[OptionalReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r].[OptionalReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf_Name]
FROM [RootEntities] AS [r]
LEFT JOIN (
    SELECT [r0].[RelationshipsTrunkEntityRelationshipsRootEntityId], [r0].[Id1], [r0].[Name], [r1].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r1].[RelationshipsBranchEntityId1], [r1].[Id1] AS [Id10], [r1].[Name] AS [Name0], [r0].[OptionalReferenceLeaf_Name], [r0].[RequiredReferenceLeaf_Name]
    FROM [Root_OptionalReferenceTrunk_CollectionBranch] AS [r0]
    LEFT JOIN [Root_OptionalReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r1] ON [r0].[RelationshipsTrunkEntityRelationshipsRootEntityId] = [r1].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AND [r0].[Id1] = [r1].[RelationshipsBranchEntityId1]
) AS [s] ON CASE
    WHEN [r].[OptionalReferenceTrunk_Name] IS NOT NULL THEN [r].[Id]
END = [s].[RelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN [Root_OptionalReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r2] ON CASE
    WHEN [r].[OptionalReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r2].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN [Root_OptionalReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r3] ON CASE
    WHEN [r].[OptionalReferenceTrunk_RequiredReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r3].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
ORDER BY [r].[Id], [s].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s].[Id1], [s].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s].[RelationshipsBranchEntityId1], [s].[Id10], [r2].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r2].[Id1], [r3].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
""");
    }

    public override async Task Select_trunk_required(bool async)
    {
        await base.Select_trunk_required(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[RequiredReferenceTrunk_Name], [s].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s].[Id1], [s].[Name], [s].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s].[RelationshipsBranchEntityId1], [s].[Id10], [s].[Name0], [s].[OptionalReferenceLeaf_Name], [s].[RequiredReferenceLeaf_Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_Name], [r2].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r2].[Id1], [r2].[Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_RequiredReferenceLeaf_Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_Name], [r3].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r3].[Id1], [r3].[Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf_Name]
FROM [RootEntities] AS [r]
LEFT JOIN (
    SELECT [r0].[RelationshipsTrunkEntityRelationshipsRootEntityId], [r0].[Id1], [r0].[Name], [r1].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r1].[RelationshipsBranchEntityId1], [r1].[Id1] AS [Id10], [r1].[Name] AS [Name0], [r0].[OptionalReferenceLeaf_Name], [r0].[RequiredReferenceLeaf_Name]
    FROM [Root_RequiredReferenceTrunk_CollectionBranch] AS [r0]
    LEFT JOIN [Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r1] ON [r0].[RelationshipsTrunkEntityRelationshipsRootEntityId] = [r1].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AND [r0].[Id1] = [r1].[RelationshipsBranchEntityId1]
) AS [s] ON [r].[Id] = [s].[RelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN [Root_RequiredReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r2] ON CASE
    WHEN [r].[RequiredReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r2].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN [Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r3] ON [r].[Id] = [r3].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
ORDER BY [r].[Id], [s].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s].[Id1], [s].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s].[RelationshipsBranchEntityId1], [s].[Id10], [r2].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r2].[Id1], [r3].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
""");
    }

    public override async Task Select_branch_required_required(bool async)
    {
        await base.Select_branch_required_required(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_Name], [r0].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r0].[Id1], [r0].[Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf_Name]
FROM [RootEntities] AS [r]
LEFT JOIN [Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r0] ON [r].[Id] = [r0].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
ORDER BY [r].[Id], [r0].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
""");
    }

    public override async Task Select_branch_required_optional(bool async)
    {
        await base.Select_branch_required_optional(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_Name], [r0].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r0].[Id1], [r0].[Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_RequiredReferenceLeaf_Name]
FROM [RootEntities] AS [r]
LEFT JOIN [Root_RequiredReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r0] ON CASE
    WHEN [r].[RequiredReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r0].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
ORDER BY [r].[Id], [r0].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
""");
    }

    public override async Task Select_branch_optional_required(bool async)
    {
        await base.Select_branch_optional_required(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_Name], [r0].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r0].[Id1], [r0].[Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf_Name]
FROM [RootEntities] AS [r]
LEFT JOIN [Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r0] ON [r].[Id] = [r0].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
ORDER BY [r].[Id], [r0].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
""");
    }

    public override async Task Select_branch_optional_optional(bool async)
    {
        await base.Select_branch_optional_optional(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_Name], [r0].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r0].[Id1], [r0].[Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_RequiredReferenceLeaf_Name]
FROM [RootEntities] AS [r]
LEFT JOIN [Root_RequiredReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r0] ON CASE
    WHEN [r].[RequiredReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r0].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
ORDER BY [r].[Id], [r0].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
""");
    }

    public override async Task Select_root_duplicated(bool async)
    {
        await base.Select_root_duplicated(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [s0].[RelationshipsRootEntityId], [s0].[Id1], [s0].[Name], [s0].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s0].[RelationshipsTrunkEntityId1], [s0].[Id10], [s0].[Name0], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityId1], [s0].[RelationshipsBranchEntityId1], [s0].[Id100], [s0].[Name00], [s0].[OptionalReferenceLeaf_Name], [s0].[RequiredReferenceLeaf_Name], [s0].[OptionalReferenceBranch_Name], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId0], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityId10], [s0].[Id11], [s0].[Name1], [s0].[OptionalReferenceBranch_OptionalReferenceLeaf_Name], [s0].[OptionalReferenceBranch_RequiredReferenceLeaf_Name], [s0].[RequiredReferenceBranch_Name], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId1], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityId11], [s0].[Id12], [s0].[Name2], [s0].[RequiredReferenceBranch_OptionalReferenceLeaf_Name], [s0].[RequiredReferenceBranch_RequiredReferenceLeaf_Name], [r].[OptionalReferenceTrunk_Name], [s1].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s1].[Id1], [s1].[Name], [s1].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s1].[RelationshipsBranchEntityId1], [s1].[Id10], [s1].[Name0], [s1].[OptionalReferenceLeaf_Name], [s1].[RequiredReferenceLeaf_Name], [r].[OptionalReferenceTrunk_OptionalReferenceBranch_Name], [r7].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r7].[Id1], [r7].[Name], [r].[OptionalReferenceTrunk_OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r].[OptionalReferenceTrunk_OptionalReferenceBranch_RequiredReferenceLeaf_Name], [r].[OptionalReferenceTrunk_RequiredReferenceBranch_Name], [r8].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r8].[Id1], [r8].[Name], [r].[OptionalReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r].[OptionalReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf_Name], [r].[RequiredReferenceTrunk_Name], [s2].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s2].[Id1], [s2].[Name], [s2].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s2].[RelationshipsBranchEntityId1], [s2].[Id10], [s2].[Name0], [s2].[OptionalReferenceLeaf_Name], [s2].[RequiredReferenceLeaf_Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_Name], [r11].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r11].[Id1], [r11].[Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_RequiredReferenceLeaf_Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_Name], [r12].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r12].[Id1], [r12].[Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf_Name], [s4].[RelationshipsRootEntityId], [s4].[Id1], [s4].[Name], [s4].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s4].[RelationshipsTrunkEntityId1], [s4].[Id10], [s4].[Name0], [s4].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s4].[RelationshipsBranchEntityRelationshipsTrunkEntityId1], [s4].[RelationshipsBranchEntityId1], [s4].[Id100], [s4].[Name00], [s4].[OptionalReferenceLeaf_Name], [s4].[RequiredReferenceLeaf_Name], [s4].[OptionalReferenceBranch_Name], [s4].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId0], [s4].[RelationshipsBranchEntityRelationshipsTrunkEntityId10], [s4].[Id11], [s4].[Name1], [s4].[OptionalReferenceBranch_OptionalReferenceLeaf_Name], [s4].[OptionalReferenceBranch_RequiredReferenceLeaf_Name], [s4].[RequiredReferenceBranch_Name], [s4].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId1], [s4].[RelationshipsBranchEntityRelationshipsTrunkEntityId11], [s4].[Id12], [s4].[Name2], [s4].[RequiredReferenceBranch_OptionalReferenceLeaf_Name], [s4].[RequiredReferenceBranch_RequiredReferenceLeaf_Name], [s5].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s5].[Id1], [s5].[Name], [s5].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s5].[RelationshipsBranchEntityId1], [s5].[Id10], [s5].[Name0], [s5].[OptionalReferenceLeaf_Name], [s5].[RequiredReferenceLeaf_Name], [r20].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r20].[Id1], [r20].[Name], [r21].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r21].[Id1], [r21].[Name], [s6].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s6].[Id1], [s6].[Name], [s6].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s6].[RelationshipsBranchEntityId1], [s6].[Id10], [s6].[Name0], [s6].[OptionalReferenceLeaf_Name], [s6].[RequiredReferenceLeaf_Name], [r24].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r24].[Id1], [r24].[Name], [r25].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r25].[Id1], [r25].[Name]
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
LEFT JOIN (
    SELECT [r5].[RelationshipsTrunkEntityRelationshipsRootEntityId], [r5].[Id1], [r5].[Name], [r6].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r6].[RelationshipsBranchEntityId1], [r6].[Id1] AS [Id10], [r6].[Name] AS [Name0], [r5].[OptionalReferenceLeaf_Name], [r5].[RequiredReferenceLeaf_Name]
    FROM [Root_OptionalReferenceTrunk_CollectionBranch] AS [r5]
    LEFT JOIN [Root_OptionalReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r6] ON [r5].[RelationshipsTrunkEntityRelationshipsRootEntityId] = [r6].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AND [r5].[Id1] = [r6].[RelationshipsBranchEntityId1]
) AS [s1] ON CASE
    WHEN [r].[OptionalReferenceTrunk_Name] IS NOT NULL THEN [r].[Id]
END = [s1].[RelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN [Root_OptionalReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r7] ON CASE
    WHEN [r].[OptionalReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r7].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN [Root_OptionalReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r8] ON CASE
    WHEN [r].[OptionalReferenceTrunk_RequiredReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r8].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN (
    SELECT [r9].[RelationshipsTrunkEntityRelationshipsRootEntityId], [r9].[Id1], [r9].[Name], [r10].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r10].[RelationshipsBranchEntityId1], [r10].[Id1] AS [Id10], [r10].[Name] AS [Name0], [r9].[OptionalReferenceLeaf_Name], [r9].[RequiredReferenceLeaf_Name]
    FROM [Root_RequiredReferenceTrunk_CollectionBranch] AS [r9]
    LEFT JOIN [Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r10] ON [r9].[RelationshipsTrunkEntityRelationshipsRootEntityId] = [r10].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AND [r9].[Id1] = [r10].[RelationshipsBranchEntityId1]
) AS [s2] ON [r].[Id] = [s2].[RelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN [Root_RequiredReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r11] ON CASE
    WHEN [r].[RequiredReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r11].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN [Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r12] ON [r].[Id] = [r12].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN (
    SELECT [r13].[RelationshipsRootEntityId], [r13].[Id1], [r13].[Name], [s3].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s3].[RelationshipsTrunkEntityId1], [s3].[Id1] AS [Id10], [s3].[Name] AS [Name0], [s3].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s3].[RelationshipsBranchEntityRelationshipsTrunkEntityId1], [s3].[RelationshipsBranchEntityId1], [s3].[Id10] AS [Id100], [s3].[Name0] AS [Name00], [s3].[OptionalReferenceLeaf_Name], [s3].[RequiredReferenceLeaf_Name], [r13].[OptionalReferenceBranch_Name], [r16].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AS [RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId0], [r16].[RelationshipsBranchEntityRelationshipsTrunkEntityId1] AS [RelationshipsBranchEntityRelationshipsTrunkEntityId10], [r16].[Id1] AS [Id11], [r16].[Name] AS [Name1], [r13].[OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r13].[OptionalReferenceBranch_RequiredReferenceLeaf_Name], [r13].[RequiredReferenceBranch_Name], [r17].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AS [RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId1], [r17].[RelationshipsBranchEntityRelationshipsTrunkEntityId1] AS [RelationshipsBranchEntityRelationshipsTrunkEntityId11], [r17].[Id1] AS [Id12], [r17].[Name] AS [Name2], [r13].[RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r13].[RequiredReferenceBranch_RequiredReferenceLeaf_Name]
    FROM [Root_CollectionTrunk] AS [r13]
    LEFT JOIN (
        SELECT [r14].[RelationshipsTrunkEntityRelationshipsRootEntityId], [r14].[RelationshipsTrunkEntityId1], [r14].[Id1], [r14].[Name], [r15].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r15].[RelationshipsBranchEntityRelationshipsTrunkEntityId1], [r15].[RelationshipsBranchEntityId1], [r15].[Id1] AS [Id10], [r15].[Name] AS [Name0], [r14].[OptionalReferenceLeaf_Name], [r14].[RequiredReferenceLeaf_Name]
        FROM [Root_CollectionTrunk_CollectionBranch] AS [r14]
        LEFT JOIN [Root_CollectionTrunk_CollectionBranch_CollectionLeaf] AS [r15] ON [r14].[RelationshipsTrunkEntityRelationshipsRootEntityId] = [r15].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AND [r14].[RelationshipsTrunkEntityId1] = [r15].[RelationshipsBranchEntityRelationshipsTrunkEntityId1] AND [r14].[Id1] = [r15].[RelationshipsBranchEntityId1]
    ) AS [s3] ON [r13].[RelationshipsRootEntityId] = [s3].[RelationshipsTrunkEntityRelationshipsRootEntityId] AND [r13].[Id1] = [s3].[RelationshipsTrunkEntityId1]
    LEFT JOIN [Root_CollectionTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r16] ON CASE
        WHEN [r13].[OptionalReferenceBranch_Name] IS NOT NULL THEN [r13].[RelationshipsRootEntityId]
    END = [r16].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AND CASE
        WHEN [r13].[OptionalReferenceBranch_Name] IS NOT NULL THEN [r13].[Id1]
    END = [r16].[RelationshipsBranchEntityRelationshipsTrunkEntityId1]
    LEFT JOIN [Root_CollectionTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r17] ON [r13].[RelationshipsRootEntityId] = [r17].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AND [r13].[Id1] = [r17].[RelationshipsBranchEntityRelationshipsTrunkEntityId1]
) AS [s4] ON [r].[Id] = [s4].[RelationshipsRootEntityId]
LEFT JOIN (
    SELECT [r18].[RelationshipsTrunkEntityRelationshipsRootEntityId], [r18].[Id1], [r18].[Name], [r19].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r19].[RelationshipsBranchEntityId1], [r19].[Id1] AS [Id10], [r19].[Name] AS [Name0], [r18].[OptionalReferenceLeaf_Name], [r18].[RequiredReferenceLeaf_Name]
    FROM [Root_OptionalReferenceTrunk_CollectionBranch] AS [r18]
    LEFT JOIN [Root_OptionalReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r19] ON [r18].[RelationshipsTrunkEntityRelationshipsRootEntityId] = [r19].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AND [r18].[Id1] = [r19].[RelationshipsBranchEntityId1]
) AS [s5] ON CASE
    WHEN [r].[OptionalReferenceTrunk_Name] IS NOT NULL THEN [r].[Id]
END = [s5].[RelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN [Root_OptionalReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r20] ON CASE
    WHEN [r].[OptionalReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r20].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN [Root_OptionalReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r21] ON CASE
    WHEN [r].[OptionalReferenceTrunk_RequiredReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r21].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN (
    SELECT [r22].[RelationshipsTrunkEntityRelationshipsRootEntityId], [r22].[Id1], [r22].[Name], [r23].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r23].[RelationshipsBranchEntityId1], [r23].[Id1] AS [Id10], [r23].[Name] AS [Name0], [r22].[OptionalReferenceLeaf_Name], [r22].[RequiredReferenceLeaf_Name]
    FROM [Root_RequiredReferenceTrunk_CollectionBranch] AS [r22]
    LEFT JOIN [Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r23] ON [r22].[RelationshipsTrunkEntityRelationshipsRootEntityId] = [r23].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AND [r22].[Id1] = [r23].[RelationshipsBranchEntityId1]
) AS [s6] ON [r].[Id] = [s6].[RelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN [Root_RequiredReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r24] ON CASE
    WHEN [r].[RequiredReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r24].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN [Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r25] ON [r].[Id] = [r25].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
ORDER BY [r].[Id], [s0].[RelationshipsRootEntityId], [s0].[Id1], [s0].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s0].[RelationshipsTrunkEntityId1], [s0].[Id10], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityId1], [s0].[RelationshipsBranchEntityId1], [s0].[Id100], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId0], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityId10], [s0].[Id11], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId1], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityId11], [s0].[Id12], [s1].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s1].[Id1], [s1].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s1].[RelationshipsBranchEntityId1], [s1].[Id10], [r7].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r7].[Id1], [r8].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r8].[Id1], [s2].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s2].[Id1], [s2].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s2].[RelationshipsBranchEntityId1], [s2].[Id10], [r11].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r11].[Id1], [r12].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r12].[Id1], [s4].[RelationshipsRootEntityId], [s4].[Id1], [s4].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s4].[RelationshipsTrunkEntityId1], [s4].[Id10], [s4].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s4].[RelationshipsBranchEntityRelationshipsTrunkEntityId1], [s4].[RelationshipsBranchEntityId1], [s4].[Id100], [s4].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId0], [s4].[RelationshipsBranchEntityRelationshipsTrunkEntityId10], [s4].[Id11], [s4].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId1], [s4].[RelationshipsBranchEntityRelationshipsTrunkEntityId11], [s4].[Id12], [s5].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s5].[Id1], [s5].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s5].[RelationshipsBranchEntityId1], [s5].[Id10], [r20].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r20].[Id1], [r21].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r21].[Id1], [s6].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s6].[Id1], [s6].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s6].[RelationshipsBranchEntityId1], [s6].[Id10], [r24].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r24].[Id1], [r25].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
""");
    }

    public override async Task Select_trunk_and_branch_duplicated(bool async)
    {
        await base.Select_trunk_and_branch_duplicated(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[OptionalReferenceTrunk_Name], [s].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s].[Id1], [s].[Name], [s].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s].[RelationshipsBranchEntityId1], [s].[Id10], [s].[Name0], [s].[OptionalReferenceLeaf_Name], [s].[RequiredReferenceLeaf_Name], [r].[OptionalReferenceTrunk_OptionalReferenceBranch_Name], [r2].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r2].[Id1], [r2].[Name], [r].[OptionalReferenceTrunk_OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r].[OptionalReferenceTrunk_OptionalReferenceBranch_RequiredReferenceLeaf_Name], [r].[OptionalReferenceTrunk_RequiredReferenceBranch_Name], [r3].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r3].[Id1], [r3].[Name], [r].[OptionalReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r].[OptionalReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf_Name], [r4].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r4].[Id1], [r4].[Name], [s0].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s0].[Id1], [s0].[Name], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s0].[RelationshipsBranchEntityId1], [s0].[Id10], [s0].[Name0], [s0].[OptionalReferenceLeaf_Name], [s0].[RequiredReferenceLeaf_Name], [r7].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r7].[Id1], [r7].[Name], [r8].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r8].[Id1], [r8].[Name], [r9].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r9].[Id1], [r9].[Name]
FROM [RootEntities] AS [r]
LEFT JOIN (
    SELECT [r0].[RelationshipsTrunkEntityRelationshipsRootEntityId], [r0].[Id1], [r0].[Name], [r1].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r1].[RelationshipsBranchEntityId1], [r1].[Id1] AS [Id10], [r1].[Name] AS [Name0], [r0].[OptionalReferenceLeaf_Name], [r0].[RequiredReferenceLeaf_Name]
    FROM [Root_OptionalReferenceTrunk_CollectionBranch] AS [r0]
    LEFT JOIN [Root_OptionalReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r1] ON [r0].[RelationshipsTrunkEntityRelationshipsRootEntityId] = [r1].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AND [r0].[Id1] = [r1].[RelationshipsBranchEntityId1]
) AS [s] ON CASE
    WHEN [r].[OptionalReferenceTrunk_Name] IS NOT NULL THEN [r].[Id]
END = [s].[RelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN [Root_OptionalReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r2] ON CASE
    WHEN [r].[OptionalReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r2].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN [Root_OptionalReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r3] ON CASE
    WHEN [r].[OptionalReferenceTrunk_RequiredReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r3].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN [Root_OptionalReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r4] ON CASE
    WHEN [r].[OptionalReferenceTrunk_RequiredReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r4].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN (
    SELECT [r5].[RelationshipsTrunkEntityRelationshipsRootEntityId], [r5].[Id1], [r5].[Name], [r6].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r6].[RelationshipsBranchEntityId1], [r6].[Id1] AS [Id10], [r6].[Name] AS [Name0], [r5].[OptionalReferenceLeaf_Name], [r5].[RequiredReferenceLeaf_Name]
    FROM [Root_OptionalReferenceTrunk_CollectionBranch] AS [r5]
    LEFT JOIN [Root_OptionalReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r6] ON [r5].[RelationshipsTrunkEntityRelationshipsRootEntityId] = [r6].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AND [r5].[Id1] = [r6].[RelationshipsBranchEntityId1]
) AS [s0] ON CASE
    WHEN [r].[OptionalReferenceTrunk_Name] IS NOT NULL THEN [r].[Id]
END = [s0].[RelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN [Root_OptionalReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r7] ON CASE
    WHEN [r].[OptionalReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r7].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN [Root_OptionalReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r8] ON CASE
    WHEN [r].[OptionalReferenceTrunk_RequiredReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r8].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN [Root_OptionalReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r9] ON CASE
    WHEN [r].[OptionalReferenceTrunk_RequiredReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r9].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
ORDER BY [r].[Id], [s].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s].[Id1], [s].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s].[RelationshipsBranchEntityId1], [s].[Id10], [r2].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r2].[Id1], [r3].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r3].[Id1], [r4].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r4].[Id1], [s0].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s0].[Id1], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s0].[RelationshipsBranchEntityId1], [s0].[Id10], [r7].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r7].[Id1], [r8].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r8].[Id1], [r9].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
""");
    }

    public override async Task Select_trunk_and_trunk_duplicated(bool async)
    {
        await base.Select_trunk_and_trunk_duplicated(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[RequiredReferenceTrunk_Name], [s].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s].[Id1], [s].[Name], [s].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s].[RelationshipsBranchEntityId1], [s].[Id10], [s].[Name0], [s].[OptionalReferenceLeaf_Name], [s].[RequiredReferenceLeaf_Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_Name], [r2].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r2].[Id1], [r2].[Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_RequiredReferenceLeaf_Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_Name], [r3].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r3].[Id1], [r3].[Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf_Name], [s0].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s0].[Id1], [s0].[Name], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s0].[RelationshipsBranchEntityId1], [s0].[Id10], [s0].[Name0], [s0].[OptionalReferenceLeaf_Name], [s0].[RequiredReferenceLeaf_Name], [r6].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r6].[Id1], [r6].[Name], [r7].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r7].[Id1], [r7].[Name]
FROM [RootEntities] AS [r]
LEFT JOIN (
    SELECT [r0].[RelationshipsTrunkEntityRelationshipsRootEntityId], [r0].[Id1], [r0].[Name], [r1].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r1].[RelationshipsBranchEntityId1], [r1].[Id1] AS [Id10], [r1].[Name] AS [Name0], [r0].[OptionalReferenceLeaf_Name], [r0].[RequiredReferenceLeaf_Name]
    FROM [Root_RequiredReferenceTrunk_CollectionBranch] AS [r0]
    LEFT JOIN [Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r1] ON [r0].[RelationshipsTrunkEntityRelationshipsRootEntityId] = [r1].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AND [r0].[Id1] = [r1].[RelationshipsBranchEntityId1]
) AS [s] ON [r].[Id] = [s].[RelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN [Root_RequiredReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r2] ON CASE
    WHEN [r].[RequiredReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r2].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN [Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r3] ON [r].[Id] = [r3].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN (
    SELECT [r4].[RelationshipsTrunkEntityRelationshipsRootEntityId], [r4].[Id1], [r4].[Name], [r5].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r5].[RelationshipsBranchEntityId1], [r5].[Id1] AS [Id10], [r5].[Name] AS [Name0], [r4].[OptionalReferenceLeaf_Name], [r4].[RequiredReferenceLeaf_Name]
    FROM [Root_RequiredReferenceTrunk_CollectionBranch] AS [r4]
    LEFT JOIN [Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r5] ON [r4].[RelationshipsTrunkEntityRelationshipsRootEntityId] = [r5].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AND [r4].[Id1] = [r5].[RelationshipsBranchEntityId1]
) AS [s0] ON [r].[Id] = [s0].[RelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN [Root_RequiredReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r6] ON CASE
    WHEN [r].[RequiredReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r6].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN [Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r7] ON [r].[Id] = [r7].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
ORDER BY [r].[Id], [s].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s].[Id1], [s].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s].[RelationshipsBranchEntityId1], [s].[Id10], [r2].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r2].[Id1], [r3].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r3].[Id1], [s0].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s0].[Id1], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s0].[RelationshipsBranchEntityId1], [s0].[Id10], [r6].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r6].[Id1], [r7].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
""");
    }

    public override async Task Select_leaf_trunk_root(bool async)
    {
        await base.Select_leaf_trunk_root(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf_Name], [r].[RequiredReferenceTrunk_Name], [s].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s].[Id1], [s].[Name], [s].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s].[RelationshipsBranchEntityId1], [s].[Id10], [s].[Name0], [s].[OptionalReferenceLeaf_Name], [s].[RequiredReferenceLeaf_Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_Name], [r2].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r2].[Id1], [r2].[Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_RequiredReferenceLeaf_Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_Name], [r3].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r3].[Id1], [r3].[Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [s1].[RelationshipsRootEntityId], [s1].[Id1], [s1].[Name], [s1].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s1].[RelationshipsTrunkEntityId1], [s1].[Id10], [s1].[Name0], [s1].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s1].[RelationshipsBranchEntityRelationshipsTrunkEntityId1], [s1].[RelationshipsBranchEntityId1], [s1].[Id100], [s1].[Name00], [s1].[OptionalReferenceLeaf_Name], [s1].[RequiredReferenceLeaf_Name], [s1].[OptionalReferenceBranch_Name], [s1].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId0], [s1].[RelationshipsBranchEntityRelationshipsTrunkEntityId10], [s1].[Id11], [s1].[Name1], [s1].[OptionalReferenceBranch_OptionalReferenceLeaf_Name], [s1].[OptionalReferenceBranch_RequiredReferenceLeaf_Name], [s1].[RequiredReferenceBranch_Name], [s1].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId1], [s1].[RelationshipsBranchEntityRelationshipsTrunkEntityId11], [s1].[Id12], [s1].[Name2], [s1].[RequiredReferenceBranch_OptionalReferenceLeaf_Name], [s1].[RequiredReferenceBranch_RequiredReferenceLeaf_Name], [r].[OptionalReferenceTrunk_Name], [s2].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s2].[Id1], [s2].[Name], [s2].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s2].[RelationshipsBranchEntityId1], [s2].[Id10], [s2].[Name0], [s2].[OptionalReferenceLeaf_Name], [s2].[RequiredReferenceLeaf_Name], [r].[OptionalReferenceTrunk_OptionalReferenceBranch_Name], [r11].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r11].[Id1], [r11].[Name], [r].[OptionalReferenceTrunk_OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r].[OptionalReferenceTrunk_OptionalReferenceBranch_RequiredReferenceLeaf_Name], [r].[OptionalReferenceTrunk_RequiredReferenceBranch_Name], [r12].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r12].[Id1], [r12].[Name], [r].[OptionalReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r].[OptionalReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf_Name], [s3].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s3].[Id1], [s3].[Name], [s3].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s3].[RelationshipsBranchEntityId1], [s3].[Id10], [s3].[Name0], [s3].[OptionalReferenceLeaf_Name], [s3].[RequiredReferenceLeaf_Name], [r15].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r15].[Id1], [r15].[Name], [r16].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r16].[Id1], [r16].[Name]
FROM [RootEntities] AS [r]
LEFT JOIN (
    SELECT [r0].[RelationshipsTrunkEntityRelationshipsRootEntityId], [r0].[Id1], [r0].[Name], [r1].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r1].[RelationshipsBranchEntityId1], [r1].[Id1] AS [Id10], [r1].[Name] AS [Name0], [r0].[OptionalReferenceLeaf_Name], [r0].[RequiredReferenceLeaf_Name]
    FROM [Root_RequiredReferenceTrunk_CollectionBranch] AS [r0]
    LEFT JOIN [Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r1] ON [r0].[RelationshipsTrunkEntityRelationshipsRootEntityId] = [r1].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AND [r0].[Id1] = [r1].[RelationshipsBranchEntityId1]
) AS [s] ON [r].[Id] = [s].[RelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN [Root_RequiredReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r2] ON CASE
    WHEN [r].[RequiredReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r2].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN [Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r3] ON [r].[Id] = [r3].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN (
    SELECT [r4].[RelationshipsRootEntityId], [r4].[Id1], [r4].[Name], [s0].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s0].[RelationshipsTrunkEntityId1], [s0].[Id1] AS [Id10], [s0].[Name] AS [Name0], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s0].[RelationshipsBranchEntityRelationshipsTrunkEntityId1], [s0].[RelationshipsBranchEntityId1], [s0].[Id10] AS [Id100], [s0].[Name0] AS [Name00], [s0].[OptionalReferenceLeaf_Name], [s0].[RequiredReferenceLeaf_Name], [r4].[OptionalReferenceBranch_Name], [r7].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AS [RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId0], [r7].[RelationshipsBranchEntityRelationshipsTrunkEntityId1] AS [RelationshipsBranchEntityRelationshipsTrunkEntityId10], [r7].[Id1] AS [Id11], [r7].[Name] AS [Name1], [r4].[OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r4].[OptionalReferenceBranch_RequiredReferenceLeaf_Name], [r4].[RequiredReferenceBranch_Name], [r8].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AS [RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId1], [r8].[RelationshipsBranchEntityRelationshipsTrunkEntityId1] AS [RelationshipsBranchEntityRelationshipsTrunkEntityId11], [r8].[Id1] AS [Id12], [r8].[Name] AS [Name2], [r4].[RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r4].[RequiredReferenceBranch_RequiredReferenceLeaf_Name]
    FROM [Root_CollectionTrunk] AS [r4]
    LEFT JOIN (
        SELECT [r5].[RelationshipsTrunkEntityRelationshipsRootEntityId], [r5].[RelationshipsTrunkEntityId1], [r5].[Id1], [r5].[Name], [r6].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r6].[RelationshipsBranchEntityRelationshipsTrunkEntityId1], [r6].[RelationshipsBranchEntityId1], [r6].[Id1] AS [Id10], [r6].[Name] AS [Name0], [r5].[OptionalReferenceLeaf_Name], [r5].[RequiredReferenceLeaf_Name]
        FROM [Root_CollectionTrunk_CollectionBranch] AS [r5]
        LEFT JOIN [Root_CollectionTrunk_CollectionBranch_CollectionLeaf] AS [r6] ON [r5].[RelationshipsTrunkEntityRelationshipsRootEntityId] = [r6].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AND [r5].[RelationshipsTrunkEntityId1] = [r6].[RelationshipsBranchEntityRelationshipsTrunkEntityId1] AND [r5].[Id1] = [r6].[RelationshipsBranchEntityId1]
    ) AS [s0] ON [r4].[RelationshipsRootEntityId] = [s0].[RelationshipsTrunkEntityRelationshipsRootEntityId] AND [r4].[Id1] = [s0].[RelationshipsTrunkEntityId1]
    LEFT JOIN [Root_CollectionTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r7] ON CASE
        WHEN [r4].[OptionalReferenceBranch_Name] IS NOT NULL THEN [r4].[RelationshipsRootEntityId]
    END = [r7].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AND CASE
        WHEN [r4].[OptionalReferenceBranch_Name] IS NOT NULL THEN [r4].[Id1]
    END = [r7].[RelationshipsBranchEntityRelationshipsTrunkEntityId1]
    LEFT JOIN [Root_CollectionTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r8] ON [r4].[RelationshipsRootEntityId] = [r8].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AND [r4].[Id1] = [r8].[RelationshipsBranchEntityRelationshipsTrunkEntityId1]
) AS [s1] ON [r].[Id] = [s1].[RelationshipsRootEntityId]
LEFT JOIN (
    SELECT [r9].[RelationshipsTrunkEntityRelationshipsRootEntityId], [r9].[Id1], [r9].[Name], [r10].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r10].[RelationshipsBranchEntityId1], [r10].[Id1] AS [Id10], [r10].[Name] AS [Name0], [r9].[OptionalReferenceLeaf_Name], [r9].[RequiredReferenceLeaf_Name]
    FROM [Root_OptionalReferenceTrunk_CollectionBranch] AS [r9]
    LEFT JOIN [Root_OptionalReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r10] ON [r9].[RelationshipsTrunkEntityRelationshipsRootEntityId] = [r10].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AND [r9].[Id1] = [r10].[RelationshipsBranchEntityId1]
) AS [s2] ON CASE
    WHEN [r].[OptionalReferenceTrunk_Name] IS NOT NULL THEN [r].[Id]
END = [s2].[RelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN [Root_OptionalReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r11] ON CASE
    WHEN [r].[OptionalReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r11].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN [Root_OptionalReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r12] ON CASE
    WHEN [r].[OptionalReferenceTrunk_RequiredReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r12].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN (
    SELECT [r13].[RelationshipsTrunkEntityRelationshipsRootEntityId], [r13].[Id1], [r13].[Name], [r14].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r14].[RelationshipsBranchEntityId1], [r14].[Id1] AS [Id10], [r14].[Name] AS [Name0], [r13].[OptionalReferenceLeaf_Name], [r13].[RequiredReferenceLeaf_Name]
    FROM [Root_RequiredReferenceTrunk_CollectionBranch] AS [r13]
    LEFT JOIN [Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r14] ON [r13].[RelationshipsTrunkEntityRelationshipsRootEntityId] = [r14].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId] AND [r13].[Id1] = [r14].[RelationshipsBranchEntityId1]
) AS [s3] ON [r].[Id] = [s3].[RelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN [Root_RequiredReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r15] ON CASE
    WHEN [r].[RequiredReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r15].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
LEFT JOIN [Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r16] ON [r].[Id] = [r16].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
ORDER BY [r].[Id], [s].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s].[Id1], [s].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s].[RelationshipsBranchEntityId1], [s].[Id10], [r2].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r2].[Id1], [r3].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r3].[Id1], [s1].[RelationshipsRootEntityId], [s1].[Id1], [s1].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s1].[RelationshipsTrunkEntityId1], [s1].[Id10], [s1].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s1].[RelationshipsBranchEntityRelationshipsTrunkEntityId1], [s1].[RelationshipsBranchEntityId1], [s1].[Id100], [s1].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId0], [s1].[RelationshipsBranchEntityRelationshipsTrunkEntityId10], [s1].[Id11], [s1].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId1], [s1].[RelationshipsBranchEntityRelationshipsTrunkEntityId11], [s1].[Id12], [s2].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s2].[Id1], [s2].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s2].[RelationshipsBranchEntityId1], [s2].[Id10], [r11].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r11].[Id1], [r12].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r12].[Id1], [s3].[RelationshipsTrunkEntityRelationshipsRootEntityId], [s3].[Id1], [s3].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [s3].[RelationshipsBranchEntityId1], [s3].[Id10], [r15].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r15].[Id1], [r16].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
""");
    }

    public override async Task Select_subquery_root_set_required_trunk_FirstOrDefault_branch(bool async)
    {
        await base.Select_subquery_root_set_required_trunk_FirstOrDefault_branch(async);

        AssertSql(
            """
SELECT [r2].[Id], [r2].[RequiredReferenceTrunk_RequiredReferenceBranch_Name], [r].[Id], [r1].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r1].[Id1], [r1].[Name], [r2].[RequiredReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r2].[RequiredReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf_Name]
FROM [RootEntities] AS [r]
OUTER APPLY (
    SELECT TOP(1) [r0].[Id], [r0].[RequiredReferenceTrunk_RequiredReferenceBranch_Name], [r0].[RequiredReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r0].[RequiredReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf_Name]
    FROM [RootEntities] AS [r0]
    ORDER BY [r0].[Id]
) AS [r2]
LEFT JOIN [Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r1] ON [r2].[Id] = [r1].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
ORDER BY [r].[Id], [r2].[Id], [r1].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
""");
    }

    public override async Task Select_subquery_root_set_optional_trunk_FirstOrDefault_branch(bool async)
    {
        await base.Select_subquery_root_set_optional_trunk_FirstOrDefault_branch(async);

        AssertSql(
            """
SELECT [r2].[Id], [r2].[OptionalReferenceTrunk_OptionalReferenceBranch_Name], [r].[Id], [r1].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId], [r1].[Id1], [r1].[Name], [r2].[OptionalReferenceTrunk_OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r2].[OptionalReferenceTrunk_OptionalReferenceBranch_RequiredReferenceLeaf_Name]
FROM [RootEntities] AS [r]
OUTER APPLY (
    SELECT TOP(1) [r0].[Id], [r0].[OptionalReferenceTrunk_OptionalReferenceBranch_Name], [r0].[OptionalReferenceTrunk_OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r0].[OptionalReferenceTrunk_OptionalReferenceBranch_RequiredReferenceLeaf_Name]
    FROM [RootEntities] AS [r0]
    ORDER BY [r0].[Id]
) AS [r2]
LEFT JOIN [Root_OptionalReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r1] ON CASE
    WHEN [r2].[OptionalReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r2].[Id]
END = [r1].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
ORDER BY [r].[Id], [r2].[Id], [r1].[RelationshipsBranchEntityRelationshipsTrunkEntityRelationshipsRootEntityId]
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
