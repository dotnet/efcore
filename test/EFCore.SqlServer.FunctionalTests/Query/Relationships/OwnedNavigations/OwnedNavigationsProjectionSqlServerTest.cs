// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.OwnedNavigations;

public class OwnedNavigationsProjectionSqlServerTest
    : OwnedNavigationsProjectionRelationalTestBase<OwnedNavigationsSqlServerFixture>
{
    public OwnedNavigationsProjectionSqlServerTest(OwnedNavigationsSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Select_root(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_root(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [s0].[RelationshipsRootId], [s0].[Id1], [s0].[Name], [s0].[RelationshipsTrunkRelationshipsRootId], [s0].[RelationshipsTrunkId1], [s0].[Id10], [s0].[Name0], [s0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s0].[RelationshipsBranchRelationshipsTrunkId1], [s0].[RelationshipsBranchId1], [s0].[Id100], [s0].[Name00], [s0].[OptionalReferenceLeaf_Name], [s0].[RequiredReferenceLeaf_Name], [s0].[OptionalReferenceBranch_Name], [s0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId0], [s0].[RelationshipsBranchRelationshipsTrunkId10], [s0].[Id11], [s0].[Name1], [s0].[OptionalReferenceBranch_OptionalReferenceLeaf_Name], [s0].[OptionalReferenceBranch_RequiredReferenceLeaf_Name], [s0].[RequiredReferenceBranch_Name], [s0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId1], [s0].[RelationshipsBranchRelationshipsTrunkId11], [s0].[Id12], [s0].[Name2], [s0].[RequiredReferenceBranch_OptionalReferenceLeaf_Name], [s0].[RequiredReferenceBranch_RequiredReferenceLeaf_Name], [r].[OptionalReferenceTrunk_Name], [s1].[RelationshipsTrunkRelationshipsRootId], [s1].[Id1], [s1].[Name], [s1].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s1].[RelationshipsBranchId1], [s1].[Id10], [s1].[Name0], [s1].[OptionalReferenceLeaf_Name], [s1].[RequiredReferenceLeaf_Name], [r].[OptionalReferenceTrunk_OptionalReferenceBranch_Name], [r7].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r7].[Id1], [r7].[Name], [r].[OptionalReferenceTrunk_OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r].[OptionalReferenceTrunk_OptionalReferenceBranch_RequiredReferenceLeaf_Name], [r].[OptionalReferenceTrunk_RequiredReferenceBranch_Name], [r8].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r8].[Id1], [r8].[Name], [r].[OptionalReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r].[OptionalReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf_Name], [r].[RequiredReferenceTrunk_Name], [s2].[RelationshipsTrunkRelationshipsRootId], [s2].[Id1], [s2].[Name], [s2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s2].[RelationshipsBranchId1], [s2].[Id10], [s2].[Name0], [s2].[OptionalReferenceLeaf_Name], [s2].[RequiredReferenceLeaf_Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_Name], [r11].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r11].[Id1], [r11].[Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_RequiredReferenceLeaf_Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_Name], [r12].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r12].[Id1], [r12].[Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf_Name]
FROM [RootEntities] AS [r]
LEFT JOIN (
    SELECT [r0].[RelationshipsRootId], [r0].[Id1], [r0].[Name], [s].[RelationshipsTrunkRelationshipsRootId], [s].[RelationshipsTrunkId1], [s].[Id1] AS [Id10], [s].[Name] AS [Name0], [s].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s].[RelationshipsBranchRelationshipsTrunkId1], [s].[RelationshipsBranchId1], [s].[Id10] AS [Id100], [s].[Name0] AS [Name00], [s].[OptionalReferenceLeaf_Name], [s].[RequiredReferenceLeaf_Name], [r0].[OptionalReferenceBranch_Name], [r3].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AS [RelationshipsBranchRelationshipsTrunkRelationshipsRootId0], [r3].[RelationshipsBranchRelationshipsTrunkId1] AS [RelationshipsBranchRelationshipsTrunkId10], [r3].[Id1] AS [Id11], [r3].[Name] AS [Name1], [r0].[OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r0].[OptionalReferenceBranch_RequiredReferenceLeaf_Name], [r0].[RequiredReferenceBranch_Name], [r4].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AS [RelationshipsBranchRelationshipsTrunkRelationshipsRootId1], [r4].[RelationshipsBranchRelationshipsTrunkId1] AS [RelationshipsBranchRelationshipsTrunkId11], [r4].[Id1] AS [Id12], [r4].[Name] AS [Name2], [r0].[RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r0].[RequiredReferenceBranch_RequiredReferenceLeaf_Name]
    FROM [Root_CollectionTrunk] AS [r0]
    LEFT JOIN (
        SELECT [r1].[RelationshipsTrunkRelationshipsRootId], [r1].[RelationshipsTrunkId1], [r1].[Id1], [r1].[Name], [r2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r2].[RelationshipsBranchRelationshipsTrunkId1], [r2].[RelationshipsBranchId1], [r2].[Id1] AS [Id10], [r2].[Name] AS [Name0], [r1].[OptionalReferenceLeaf_Name], [r1].[RequiredReferenceLeaf_Name]
        FROM [Root_CollectionTrunk_CollectionBranch] AS [r1]
        LEFT JOIN [Root_CollectionTrunk_CollectionBranch_CollectionLeaf] AS [r2] ON [r1].[RelationshipsTrunkRelationshipsRootId] = [r2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r1].[RelationshipsTrunkId1] = [r2].[RelationshipsBranchRelationshipsTrunkId1] AND [r1].[Id1] = [r2].[RelationshipsBranchId1]
    ) AS [s] ON [r0].[RelationshipsRootId] = [s].[RelationshipsTrunkRelationshipsRootId] AND [r0].[Id1] = [s].[RelationshipsTrunkId1]
    LEFT JOIN [Root_CollectionTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r3] ON CASE
        WHEN [r0].[OptionalReferenceBranch_Name] IS NOT NULL THEN [r0].[RelationshipsRootId]
    END = [r3].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND CASE
        WHEN [r0].[OptionalReferenceBranch_Name] IS NOT NULL THEN [r0].[Id1]
    END = [r3].[RelationshipsBranchRelationshipsTrunkId1]
    LEFT JOIN [Root_CollectionTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r4] ON [r0].[RelationshipsRootId] = [r4].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r0].[Id1] = [r4].[RelationshipsBranchRelationshipsTrunkId1]
) AS [s0] ON [r].[Id] = [s0].[RelationshipsRootId]
LEFT JOIN (
    SELECT [r5].[RelationshipsTrunkRelationshipsRootId], [r5].[Id1], [r5].[Name], [r6].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r6].[RelationshipsBranchId1], [r6].[Id1] AS [Id10], [r6].[Name] AS [Name0], [r5].[OptionalReferenceLeaf_Name], [r5].[RequiredReferenceLeaf_Name]
    FROM [Root_OptionalReferenceTrunk_CollectionBranch] AS [r5]
    LEFT JOIN [Root_OptionalReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r6] ON [r5].[RelationshipsTrunkRelationshipsRootId] = [r6].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r5].[Id1] = [r6].[RelationshipsBranchId1]
) AS [s1] ON CASE
    WHEN [r].[OptionalReferenceTrunk_Name] IS NOT NULL THEN [r].[Id]
END = [s1].[RelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_OptionalReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r7] ON CASE
    WHEN [r].[OptionalReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r7].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_OptionalReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r8] ON CASE
    WHEN [r].[OptionalReferenceTrunk_RequiredReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r8].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
LEFT JOIN (
    SELECT [r9].[RelationshipsTrunkRelationshipsRootId], [r9].[Id1], [r9].[Name], [r10].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r10].[RelationshipsBranchId1], [r10].[Id1] AS [Id10], [r10].[Name] AS [Name0], [r9].[OptionalReferenceLeaf_Name], [r9].[RequiredReferenceLeaf_Name]
    FROM [Root_RequiredReferenceTrunk_CollectionBranch] AS [r9]
    LEFT JOIN [Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r10] ON [r9].[RelationshipsTrunkRelationshipsRootId] = [r10].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r9].[Id1] = [r10].[RelationshipsBranchId1]
) AS [s2] ON [r].[Id] = [s2].[RelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_RequiredReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r11] ON CASE
    WHEN [r].[RequiredReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r11].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r12] ON [r].[Id] = [r12].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
ORDER BY [r].[Id], [s0].[RelationshipsRootId], [s0].[Id1], [s0].[RelationshipsTrunkRelationshipsRootId], [s0].[RelationshipsTrunkId1], [s0].[Id10], [s0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s0].[RelationshipsBranchRelationshipsTrunkId1], [s0].[RelationshipsBranchId1], [s0].[Id100], [s0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId0], [s0].[RelationshipsBranchRelationshipsTrunkId10], [s0].[Id11], [s0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId1], [s0].[RelationshipsBranchRelationshipsTrunkId11], [s0].[Id12], [s1].[RelationshipsTrunkRelationshipsRootId], [s1].[Id1], [s1].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s1].[RelationshipsBranchId1], [s1].[Id10], [r7].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r7].[Id1], [r8].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r8].[Id1], [s2].[RelationshipsTrunkRelationshipsRootId], [s2].[Id1], [s2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s2].[RelationshipsBranchId1], [s2].[Id10], [r11].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r11].[Id1], [r12].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
""");
    }

    public override async Task Select_trunk_optional(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_trunk_optional(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[OptionalReferenceTrunk_Name], [s].[RelationshipsTrunkRelationshipsRootId], [s].[Id1], [s].[Name], [s].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s].[RelationshipsBranchId1], [s].[Id10], [s].[Name0], [s].[OptionalReferenceLeaf_Name], [s].[RequiredReferenceLeaf_Name], [r].[OptionalReferenceTrunk_OptionalReferenceBranch_Name], [r2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r2].[Id1], [r2].[Name], [r].[OptionalReferenceTrunk_OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r].[OptionalReferenceTrunk_OptionalReferenceBranch_RequiredReferenceLeaf_Name], [r].[OptionalReferenceTrunk_RequiredReferenceBranch_Name], [r3].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r3].[Id1], [r3].[Name], [r].[OptionalReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r].[OptionalReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf_Name]
FROM [RootEntities] AS [r]
LEFT JOIN (
    SELECT [r0].[RelationshipsTrunkRelationshipsRootId], [r0].[Id1], [r0].[Name], [r1].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r1].[RelationshipsBranchId1], [r1].[Id1] AS [Id10], [r1].[Name] AS [Name0], [r0].[OptionalReferenceLeaf_Name], [r0].[RequiredReferenceLeaf_Name]
    FROM [Root_OptionalReferenceTrunk_CollectionBranch] AS [r0]
    LEFT JOIN [Root_OptionalReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r1] ON [r0].[RelationshipsTrunkRelationshipsRootId] = [r1].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r0].[Id1] = [r1].[RelationshipsBranchId1]
) AS [s] ON CASE
    WHEN [r].[OptionalReferenceTrunk_Name] IS NOT NULL THEN [r].[Id]
END = [s].[RelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_OptionalReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r2] ON CASE
    WHEN [r].[OptionalReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_OptionalReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r3] ON CASE
    WHEN [r].[OptionalReferenceTrunk_RequiredReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r3].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
ORDER BY [r].[Id], [s].[RelationshipsTrunkRelationshipsRootId], [s].[Id1], [s].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s].[RelationshipsBranchId1], [s].[Id10], [r2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r2].[Id1], [r3].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
""");
        }
    }

    public override async Task Select_trunk_required(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_trunk_required(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[RequiredReferenceTrunk_Name], [s].[RelationshipsTrunkRelationshipsRootId], [s].[Id1], [s].[Name], [s].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s].[RelationshipsBranchId1], [s].[Id10], [s].[Name0], [s].[OptionalReferenceLeaf_Name], [s].[RequiredReferenceLeaf_Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_Name], [r2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r2].[Id1], [r2].[Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_RequiredReferenceLeaf_Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_Name], [r3].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r3].[Id1], [r3].[Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf_Name]
FROM [RootEntities] AS [r]
LEFT JOIN (
    SELECT [r0].[RelationshipsTrunkRelationshipsRootId], [r0].[Id1], [r0].[Name], [r1].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r1].[RelationshipsBranchId1], [r1].[Id1] AS [Id10], [r1].[Name] AS [Name0], [r0].[OptionalReferenceLeaf_Name], [r0].[RequiredReferenceLeaf_Name]
    FROM [Root_RequiredReferenceTrunk_CollectionBranch] AS [r0]
    LEFT JOIN [Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r1] ON [r0].[RelationshipsTrunkRelationshipsRootId] = [r1].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r0].[Id1] = [r1].[RelationshipsBranchId1]
) AS [s] ON [r].[Id] = [s].[RelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_RequiredReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r2] ON CASE
    WHEN [r].[RequiredReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r3] ON [r].[Id] = [r3].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
ORDER BY [r].[Id], [s].[RelationshipsTrunkRelationshipsRootId], [s].[Id1], [s].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s].[RelationshipsBranchId1], [s].[Id10], [r2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r2].[Id1], [r3].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
""");
        }
    }

    public override async Task Select_trunk_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_trunk_collection(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [s0].[RelationshipsRootId], [s0].[Id1], [s0].[Name], [s0].[RelationshipsTrunkRelationshipsRootId], [s0].[RelationshipsTrunkId1], [s0].[Id10], [s0].[Name0], [s0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s0].[RelationshipsBranchRelationshipsTrunkId1], [s0].[RelationshipsBranchId1], [s0].[Id100], [s0].[Name00], [s0].[OptionalReferenceLeaf_Name], [s0].[RequiredReferenceLeaf_Name], [s0].[OptionalReferenceBranch_Name], [s0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId0], [s0].[RelationshipsBranchRelationshipsTrunkId10], [s0].[Id11], [s0].[Name1], [s0].[OptionalReferenceBranch_OptionalReferenceLeaf_Name], [s0].[OptionalReferenceBranch_RequiredReferenceLeaf_Name], [s0].[RequiredReferenceBranch_Name], [s0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId1], [s0].[RelationshipsBranchRelationshipsTrunkId11], [s0].[Id12], [s0].[Name2], [s0].[RequiredReferenceBranch_OptionalReferenceLeaf_Name], [s0].[RequiredReferenceBranch_RequiredReferenceLeaf_Name]
FROM [RootEntities] AS [r]
LEFT JOIN (
    SELECT [r0].[RelationshipsRootId], [r0].[Id1], [r0].[Name], [s].[RelationshipsTrunkRelationshipsRootId], [s].[RelationshipsTrunkId1], [s].[Id1] AS [Id10], [s].[Name] AS [Name0], [s].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s].[RelationshipsBranchRelationshipsTrunkId1], [s].[RelationshipsBranchId1], [s].[Id10] AS [Id100], [s].[Name0] AS [Name00], [s].[OptionalReferenceLeaf_Name], [s].[RequiredReferenceLeaf_Name], [r0].[OptionalReferenceBranch_Name], [r3].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AS [RelationshipsBranchRelationshipsTrunkRelationshipsRootId0], [r3].[RelationshipsBranchRelationshipsTrunkId1] AS [RelationshipsBranchRelationshipsTrunkId10], [r3].[Id1] AS [Id11], [r3].[Name] AS [Name1], [r0].[OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r0].[OptionalReferenceBranch_RequiredReferenceLeaf_Name], [r0].[RequiredReferenceBranch_Name], [r4].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AS [RelationshipsBranchRelationshipsTrunkRelationshipsRootId1], [r4].[RelationshipsBranchRelationshipsTrunkId1] AS [RelationshipsBranchRelationshipsTrunkId11], [r4].[Id1] AS [Id12], [r4].[Name] AS [Name2], [r0].[RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r0].[RequiredReferenceBranch_RequiredReferenceLeaf_Name]
    FROM [Root_CollectionTrunk] AS [r0]
    LEFT JOIN (
        SELECT [r1].[RelationshipsTrunkRelationshipsRootId], [r1].[RelationshipsTrunkId1], [r1].[Id1], [r1].[Name], [r2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r2].[RelationshipsBranchRelationshipsTrunkId1], [r2].[RelationshipsBranchId1], [r2].[Id1] AS [Id10], [r2].[Name] AS [Name0], [r1].[OptionalReferenceLeaf_Name], [r1].[RequiredReferenceLeaf_Name]
        FROM [Root_CollectionTrunk_CollectionBranch] AS [r1]
        LEFT JOIN [Root_CollectionTrunk_CollectionBranch_CollectionLeaf] AS [r2] ON [r1].[RelationshipsTrunkRelationshipsRootId] = [r2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r1].[RelationshipsTrunkId1] = [r2].[RelationshipsBranchRelationshipsTrunkId1] AND [r1].[Id1] = [r2].[RelationshipsBranchId1]
    ) AS [s] ON [r0].[RelationshipsRootId] = [s].[RelationshipsTrunkRelationshipsRootId] AND [r0].[Id1] = [s].[RelationshipsTrunkId1]
    LEFT JOIN [Root_CollectionTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r3] ON CASE
        WHEN [r0].[OptionalReferenceBranch_Name] IS NOT NULL THEN [r0].[RelationshipsRootId]
    END = [r3].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND CASE
        WHEN [r0].[OptionalReferenceBranch_Name] IS NOT NULL THEN [r0].[Id1]
    END = [r3].[RelationshipsBranchRelationshipsTrunkId1]
    LEFT JOIN [Root_CollectionTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r4] ON [r0].[RelationshipsRootId] = [r4].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r0].[Id1] = [r4].[RelationshipsBranchRelationshipsTrunkId1]
) AS [s0] ON [r].[Id] = [s0].[RelationshipsRootId]
ORDER BY [r].[Id], [s0].[RelationshipsRootId], [s0].[Id1], [s0].[RelationshipsTrunkRelationshipsRootId], [s0].[RelationshipsTrunkId1], [s0].[Id10], [s0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s0].[RelationshipsBranchRelationshipsTrunkId1], [s0].[RelationshipsBranchId1], [s0].[Id100], [s0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId0], [s0].[RelationshipsBranchRelationshipsTrunkId10], [s0].[Id11], [s0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId1], [s0].[RelationshipsBranchRelationshipsTrunkId11]
""");
        }
    }

    public override async Task Select_branch_required_required(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_branch_required_required(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_Name], [r0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r0].[Id1], [r0].[Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf_Name]
FROM [RootEntities] AS [r]
LEFT JOIN [Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r0] ON [r].[Id] = [r0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
ORDER BY [r].[Id], [r0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
""");
        }
    }

    public override async Task Select_branch_required_optional(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_branch_required_optional(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_Name], [r0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r0].[Id1], [r0].[Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_RequiredReferenceLeaf_Name]
FROM [RootEntities] AS [r]
LEFT JOIN [Root_RequiredReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r0] ON CASE
    WHEN [r].[RequiredReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
ORDER BY [r].[Id], [r0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
""");
        }
    }

    public override async Task Select_branch_optional_required(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_branch_optional_required(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_Name], [r0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r0].[Id1], [r0].[Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf_Name]
FROM [RootEntities] AS [r]
LEFT JOIN [Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r0] ON [r].[Id] = [r0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
ORDER BY [r].[Id], [r0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
""");
        }
    }

    public override async Task Select_branch_optional_optional(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_branch_optional_optional(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_Name], [r0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r0].[Id1], [r0].[Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_RequiredReferenceLeaf_Name]
FROM [RootEntities] AS [r]
LEFT JOIN [Root_RequiredReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r0] ON CASE
    WHEN [r].[RequiredReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
ORDER BY [r].[Id], [r0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
""");
        }
    }

    public override async Task Select_branch_required_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_branch_optional_optional(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_Name], [r0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r0].[Id1], [r0].[Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_RequiredReferenceLeaf_Name]
FROM [RootEntities] AS [r]
LEFT JOIN [Root_RequiredReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r0] ON CASE
    WHEN [r].[RequiredReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
ORDER BY [r].[Id], [r0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
""");
        }
    }

    public override async Task Select_branch_optional_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_branch_optional_collection(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [s].[RelationshipsTrunkRelationshipsRootId], [s].[Id1], [s].[Name], [s].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s].[RelationshipsBranchId1], [s].[Id10], [s].[Name0], [s].[OptionalReferenceLeaf_Name], [s].[RequiredReferenceLeaf_Name]
FROM [RootEntities] AS [r]
LEFT JOIN (
    SELECT [r0].[RelationshipsTrunkRelationshipsRootId], [r0].[Id1], [r0].[Name], [r1].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r1].[RelationshipsBranchId1], [r1].[Id1] AS [Id10], [r1].[Name] AS [Name0], [r0].[OptionalReferenceLeaf_Name], [r0].[RequiredReferenceLeaf_Name]
    FROM [Root_RequiredReferenceTrunk_CollectionBranch] AS [r0]
    LEFT JOIN [Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r1] ON [r0].[RelationshipsTrunkRelationshipsRootId] = [r1].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r0].[Id1] = [r1].[RelationshipsBranchId1]
) AS [s] ON [r].[Id] = [s].[RelationshipsTrunkRelationshipsRootId]
ORDER BY [r].[Id], [s].[RelationshipsTrunkRelationshipsRootId], [s].[Id1], [s].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s].[RelationshipsBranchId1]
""");
        }
    }

    #region Multiple

    public override async Task Select_root_duplicated(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_root_duplicated(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [s0].[RelationshipsRootId], [s0].[Id1], [s0].[Name], [s0].[RelationshipsTrunkRelationshipsRootId], [s0].[RelationshipsTrunkId1], [s0].[Id10], [s0].[Name0], [s0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s0].[RelationshipsBranchRelationshipsTrunkId1], [s0].[RelationshipsBranchId1], [s0].[Id100], [s0].[Name00], [s0].[OptionalReferenceLeaf_Name], [s0].[RequiredReferenceLeaf_Name], [s0].[OptionalReferenceBranch_Name], [s0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId0], [s0].[RelationshipsBranchRelationshipsTrunkId10], [s0].[Id11], [s0].[Name1], [s0].[OptionalReferenceBranch_OptionalReferenceLeaf_Name], [s0].[OptionalReferenceBranch_RequiredReferenceLeaf_Name], [s0].[RequiredReferenceBranch_Name], [s0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId1], [s0].[RelationshipsBranchRelationshipsTrunkId11], [s0].[Id12], [s0].[Name2], [s0].[RequiredReferenceBranch_OptionalReferenceLeaf_Name], [s0].[RequiredReferenceBranch_RequiredReferenceLeaf_Name], [r].[OptionalReferenceTrunk_Name], [s1].[RelationshipsTrunkRelationshipsRootId], [s1].[Id1], [s1].[Name], [s1].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s1].[RelationshipsBranchId1], [s1].[Id10], [s1].[Name0], [s1].[OptionalReferenceLeaf_Name], [s1].[RequiredReferenceLeaf_Name], [r].[OptionalReferenceTrunk_OptionalReferenceBranch_Name], [r7].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r7].[Id1], [r7].[Name], [r].[OptionalReferenceTrunk_OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r].[OptionalReferenceTrunk_OptionalReferenceBranch_RequiredReferenceLeaf_Name], [r].[OptionalReferenceTrunk_RequiredReferenceBranch_Name], [r8].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r8].[Id1], [r8].[Name], [r].[OptionalReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r].[OptionalReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf_Name], [r].[RequiredReferenceTrunk_Name], [s2].[RelationshipsTrunkRelationshipsRootId], [s2].[Id1], [s2].[Name], [s2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s2].[RelationshipsBranchId1], [s2].[Id10], [s2].[Name0], [s2].[OptionalReferenceLeaf_Name], [s2].[RequiredReferenceLeaf_Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_Name], [r11].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r11].[Id1], [r11].[Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_RequiredReferenceLeaf_Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_Name], [r12].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r12].[Id1], [r12].[Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf_Name], [s4].[RelationshipsRootId], [s4].[Id1], [s4].[Name], [s4].[RelationshipsTrunkRelationshipsRootId], [s4].[RelationshipsTrunkId1], [s4].[Id10], [s4].[Name0], [s4].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s4].[RelationshipsBranchRelationshipsTrunkId1], [s4].[RelationshipsBranchId1], [s4].[Id100], [s4].[Name00], [s4].[OptionalReferenceLeaf_Name], [s4].[RequiredReferenceLeaf_Name], [s4].[OptionalReferenceBranch_Name], [s4].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId0], [s4].[RelationshipsBranchRelationshipsTrunkId10], [s4].[Id11], [s4].[Name1], [s4].[OptionalReferenceBranch_OptionalReferenceLeaf_Name], [s4].[OptionalReferenceBranch_RequiredReferenceLeaf_Name], [s4].[RequiredReferenceBranch_Name], [s4].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId1], [s4].[RelationshipsBranchRelationshipsTrunkId11], [s4].[Id12], [s4].[Name2], [s4].[RequiredReferenceBranch_OptionalReferenceLeaf_Name], [s4].[RequiredReferenceBranch_RequiredReferenceLeaf_Name], [s5].[RelationshipsTrunkRelationshipsRootId], [s5].[Id1], [s5].[Name], [s5].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s5].[RelationshipsBranchId1], [s5].[Id10], [s5].[Name0], [s5].[OptionalReferenceLeaf_Name], [s5].[RequiredReferenceLeaf_Name], [r20].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r20].[Id1], [r20].[Name], [r21].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r21].[Id1], [r21].[Name], [s6].[RelationshipsTrunkRelationshipsRootId], [s6].[Id1], [s6].[Name], [s6].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s6].[RelationshipsBranchId1], [s6].[Id10], [s6].[Name0], [s6].[OptionalReferenceLeaf_Name], [s6].[RequiredReferenceLeaf_Name], [r24].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r24].[Id1], [r24].[Name], [r25].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r25].[Id1], [r25].[Name]
FROM [RootEntities] AS [r]
LEFT JOIN (
    SELECT [r0].[RelationshipsRootId], [r0].[Id1], [r0].[Name], [s].[RelationshipsTrunkRelationshipsRootId], [s].[RelationshipsTrunkId1], [s].[Id1] AS [Id10], [s].[Name] AS [Name0], [s].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s].[RelationshipsBranchRelationshipsTrunkId1], [s].[RelationshipsBranchId1], [s].[Id10] AS [Id100], [s].[Name0] AS [Name00], [s].[OptionalReferenceLeaf_Name], [s].[RequiredReferenceLeaf_Name], [r0].[OptionalReferenceBranch_Name], [r3].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AS [RelationshipsBranchRelationshipsTrunkRelationshipsRootId0], [r3].[RelationshipsBranchRelationshipsTrunkId1] AS [RelationshipsBranchRelationshipsTrunkId10], [r3].[Id1] AS [Id11], [r3].[Name] AS [Name1], [r0].[OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r0].[OptionalReferenceBranch_RequiredReferenceLeaf_Name], [r0].[RequiredReferenceBranch_Name], [r4].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AS [RelationshipsBranchRelationshipsTrunkRelationshipsRootId1], [r4].[RelationshipsBranchRelationshipsTrunkId1] AS [RelationshipsBranchRelationshipsTrunkId11], [r4].[Id1] AS [Id12], [r4].[Name] AS [Name2], [r0].[RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r0].[RequiredReferenceBranch_RequiredReferenceLeaf_Name]
    FROM [Root_CollectionTrunk] AS [r0]
    LEFT JOIN (
        SELECT [r1].[RelationshipsTrunkRelationshipsRootId], [r1].[RelationshipsTrunkId1], [r1].[Id1], [r1].[Name], [r2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r2].[RelationshipsBranchRelationshipsTrunkId1], [r2].[RelationshipsBranchId1], [r2].[Id1] AS [Id10], [r2].[Name] AS [Name0], [r1].[OptionalReferenceLeaf_Name], [r1].[RequiredReferenceLeaf_Name]
        FROM [Root_CollectionTrunk_CollectionBranch] AS [r1]
        LEFT JOIN [Root_CollectionTrunk_CollectionBranch_CollectionLeaf] AS [r2] ON [r1].[RelationshipsTrunkRelationshipsRootId] = [r2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r1].[RelationshipsTrunkId1] = [r2].[RelationshipsBranchRelationshipsTrunkId1] AND [r1].[Id1] = [r2].[RelationshipsBranchId1]
    ) AS [s] ON [r0].[RelationshipsRootId] = [s].[RelationshipsTrunkRelationshipsRootId] AND [r0].[Id1] = [s].[RelationshipsTrunkId1]
    LEFT JOIN [Root_CollectionTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r3] ON CASE
        WHEN [r0].[OptionalReferenceBranch_Name] IS NOT NULL THEN [r0].[RelationshipsRootId]
    END = [r3].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND CASE
        WHEN [r0].[OptionalReferenceBranch_Name] IS NOT NULL THEN [r0].[Id1]
    END = [r3].[RelationshipsBranchRelationshipsTrunkId1]
    LEFT JOIN [Root_CollectionTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r4] ON [r0].[RelationshipsRootId] = [r4].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r0].[Id1] = [r4].[RelationshipsBranchRelationshipsTrunkId1]
) AS [s0] ON [r].[Id] = [s0].[RelationshipsRootId]
LEFT JOIN (
    SELECT [r5].[RelationshipsTrunkRelationshipsRootId], [r5].[Id1], [r5].[Name], [r6].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r6].[RelationshipsBranchId1], [r6].[Id1] AS [Id10], [r6].[Name] AS [Name0], [r5].[OptionalReferenceLeaf_Name], [r5].[RequiredReferenceLeaf_Name]
    FROM [Root_OptionalReferenceTrunk_CollectionBranch] AS [r5]
    LEFT JOIN [Root_OptionalReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r6] ON [r5].[RelationshipsTrunkRelationshipsRootId] = [r6].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r5].[Id1] = [r6].[RelationshipsBranchId1]
) AS [s1] ON CASE
    WHEN [r].[OptionalReferenceTrunk_Name] IS NOT NULL THEN [r].[Id]
END = [s1].[RelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_OptionalReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r7] ON CASE
    WHEN [r].[OptionalReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r7].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_OptionalReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r8] ON CASE
    WHEN [r].[OptionalReferenceTrunk_RequiredReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r8].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
LEFT JOIN (
    SELECT [r9].[RelationshipsTrunkRelationshipsRootId], [r9].[Id1], [r9].[Name], [r10].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r10].[RelationshipsBranchId1], [r10].[Id1] AS [Id10], [r10].[Name] AS [Name0], [r9].[OptionalReferenceLeaf_Name], [r9].[RequiredReferenceLeaf_Name]
    FROM [Root_RequiredReferenceTrunk_CollectionBranch] AS [r9]
    LEFT JOIN [Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r10] ON [r9].[RelationshipsTrunkRelationshipsRootId] = [r10].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r9].[Id1] = [r10].[RelationshipsBranchId1]
) AS [s2] ON [r].[Id] = [s2].[RelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_RequiredReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r11] ON CASE
    WHEN [r].[RequiredReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r11].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r12] ON [r].[Id] = [r12].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
LEFT JOIN (
    SELECT [r13].[RelationshipsRootId], [r13].[Id1], [r13].[Name], [s3].[RelationshipsTrunkRelationshipsRootId], [s3].[RelationshipsTrunkId1], [s3].[Id1] AS [Id10], [s3].[Name] AS [Name0], [s3].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s3].[RelationshipsBranchRelationshipsTrunkId1], [s3].[RelationshipsBranchId1], [s3].[Id10] AS [Id100], [s3].[Name0] AS [Name00], [s3].[OptionalReferenceLeaf_Name], [s3].[RequiredReferenceLeaf_Name], [r13].[OptionalReferenceBranch_Name], [r16].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AS [RelationshipsBranchRelationshipsTrunkRelationshipsRootId0], [r16].[RelationshipsBranchRelationshipsTrunkId1] AS [RelationshipsBranchRelationshipsTrunkId10], [r16].[Id1] AS [Id11], [r16].[Name] AS [Name1], [r13].[OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r13].[OptionalReferenceBranch_RequiredReferenceLeaf_Name], [r13].[RequiredReferenceBranch_Name], [r17].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AS [RelationshipsBranchRelationshipsTrunkRelationshipsRootId1], [r17].[RelationshipsBranchRelationshipsTrunkId1] AS [RelationshipsBranchRelationshipsTrunkId11], [r17].[Id1] AS [Id12], [r17].[Name] AS [Name2], [r13].[RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r13].[RequiredReferenceBranch_RequiredReferenceLeaf_Name]
    FROM [Root_CollectionTrunk] AS [r13]
    LEFT JOIN (
        SELECT [r14].[RelationshipsTrunkRelationshipsRootId], [r14].[RelationshipsTrunkId1], [r14].[Id1], [r14].[Name], [r15].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r15].[RelationshipsBranchRelationshipsTrunkId1], [r15].[RelationshipsBranchId1], [r15].[Id1] AS [Id10], [r15].[Name] AS [Name0], [r14].[OptionalReferenceLeaf_Name], [r14].[RequiredReferenceLeaf_Name]
        FROM [Root_CollectionTrunk_CollectionBranch] AS [r14]
        LEFT JOIN [Root_CollectionTrunk_CollectionBranch_CollectionLeaf] AS [r15] ON [r14].[RelationshipsTrunkRelationshipsRootId] = [r15].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r14].[RelationshipsTrunkId1] = [r15].[RelationshipsBranchRelationshipsTrunkId1] AND [r14].[Id1] = [r15].[RelationshipsBranchId1]
    ) AS [s3] ON [r13].[RelationshipsRootId] = [s3].[RelationshipsTrunkRelationshipsRootId] AND [r13].[Id1] = [s3].[RelationshipsTrunkId1]
    LEFT JOIN [Root_CollectionTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r16] ON CASE
        WHEN [r13].[OptionalReferenceBranch_Name] IS NOT NULL THEN [r13].[RelationshipsRootId]
    END = [r16].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND CASE
        WHEN [r13].[OptionalReferenceBranch_Name] IS NOT NULL THEN [r13].[Id1]
    END = [r16].[RelationshipsBranchRelationshipsTrunkId1]
    LEFT JOIN [Root_CollectionTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r17] ON [r13].[RelationshipsRootId] = [r17].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r13].[Id1] = [r17].[RelationshipsBranchRelationshipsTrunkId1]
) AS [s4] ON [r].[Id] = [s4].[RelationshipsRootId]
LEFT JOIN (
    SELECT [r18].[RelationshipsTrunkRelationshipsRootId], [r18].[Id1], [r18].[Name], [r19].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r19].[RelationshipsBranchId1], [r19].[Id1] AS [Id10], [r19].[Name] AS [Name0], [r18].[OptionalReferenceLeaf_Name], [r18].[RequiredReferenceLeaf_Name]
    FROM [Root_OptionalReferenceTrunk_CollectionBranch] AS [r18]
    LEFT JOIN [Root_OptionalReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r19] ON [r18].[RelationshipsTrunkRelationshipsRootId] = [r19].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r18].[Id1] = [r19].[RelationshipsBranchId1]
) AS [s5] ON CASE
    WHEN [r].[OptionalReferenceTrunk_Name] IS NOT NULL THEN [r].[Id]
END = [s5].[RelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_OptionalReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r20] ON CASE
    WHEN [r].[OptionalReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r20].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_OptionalReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r21] ON CASE
    WHEN [r].[OptionalReferenceTrunk_RequiredReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r21].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
LEFT JOIN (
    SELECT [r22].[RelationshipsTrunkRelationshipsRootId], [r22].[Id1], [r22].[Name], [r23].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r23].[RelationshipsBranchId1], [r23].[Id1] AS [Id10], [r23].[Name] AS [Name0], [r22].[OptionalReferenceLeaf_Name], [r22].[RequiredReferenceLeaf_Name]
    FROM [Root_RequiredReferenceTrunk_CollectionBranch] AS [r22]
    LEFT JOIN [Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r23] ON [r22].[RelationshipsTrunkRelationshipsRootId] = [r23].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r22].[Id1] = [r23].[RelationshipsBranchId1]
) AS [s6] ON [r].[Id] = [s6].[RelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_RequiredReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r24] ON CASE
    WHEN [r].[RequiredReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r24].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r25] ON [r].[Id] = [r25].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
ORDER BY [r].[Id], [s0].[RelationshipsRootId], [s0].[Id1], [s0].[RelationshipsTrunkRelationshipsRootId], [s0].[RelationshipsTrunkId1], [s0].[Id10], [s0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s0].[RelationshipsBranchRelationshipsTrunkId1], [s0].[RelationshipsBranchId1], [s0].[Id100], [s0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId0], [s0].[RelationshipsBranchRelationshipsTrunkId10], [s0].[Id11], [s0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId1], [s0].[RelationshipsBranchRelationshipsTrunkId11], [s0].[Id12], [s1].[RelationshipsTrunkRelationshipsRootId], [s1].[Id1], [s1].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s1].[RelationshipsBranchId1], [s1].[Id10], [r7].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r7].[Id1], [r8].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r8].[Id1], [s2].[RelationshipsTrunkRelationshipsRootId], [s2].[Id1], [s2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s2].[RelationshipsBranchId1], [s2].[Id10], [r11].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r11].[Id1], [r12].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r12].[Id1], [s4].[RelationshipsRootId], [s4].[Id1], [s4].[RelationshipsTrunkRelationshipsRootId], [s4].[RelationshipsTrunkId1], [s4].[Id10], [s4].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s4].[RelationshipsBranchRelationshipsTrunkId1], [s4].[RelationshipsBranchId1], [s4].[Id100], [s4].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId0], [s4].[RelationshipsBranchRelationshipsTrunkId10], [s4].[Id11], [s4].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId1], [s4].[RelationshipsBranchRelationshipsTrunkId11], [s4].[Id12], [s5].[RelationshipsTrunkRelationshipsRootId], [s5].[Id1], [s5].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s5].[RelationshipsBranchId1], [s5].[Id10], [r20].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r20].[Id1], [r21].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r21].[Id1], [s6].[RelationshipsTrunkRelationshipsRootId], [s6].[Id1], [s6].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s6].[RelationshipsBranchId1], [s6].[Id10], [r24].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r24].[Id1], [r25].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
""");
    }

    public override async Task Select_trunk_and_branch_duplicated(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_trunk_and_branch_duplicated(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[OptionalReferenceTrunk_Name], [s].[RelationshipsTrunkRelationshipsRootId], [s].[Id1], [s].[Name], [s].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s].[RelationshipsBranchId1], [s].[Id10], [s].[Name0], [s].[OptionalReferenceLeaf_Name], [s].[RequiredReferenceLeaf_Name], [r].[OptionalReferenceTrunk_OptionalReferenceBranch_Name], [r2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r2].[Id1], [r2].[Name], [r].[OptionalReferenceTrunk_OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r].[OptionalReferenceTrunk_OptionalReferenceBranch_RequiredReferenceLeaf_Name], [r].[OptionalReferenceTrunk_RequiredReferenceBranch_Name], [r3].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r3].[Id1], [r3].[Name], [r].[OptionalReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r].[OptionalReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf_Name], [r4].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r4].[Id1], [r4].[Name], [s0].[RelationshipsTrunkRelationshipsRootId], [s0].[Id1], [s0].[Name], [s0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s0].[RelationshipsBranchId1], [s0].[Id10], [s0].[Name0], [s0].[OptionalReferenceLeaf_Name], [s0].[RequiredReferenceLeaf_Name], [r7].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r7].[Id1], [r7].[Name], [r8].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r8].[Id1], [r8].[Name], [r9].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r9].[Id1], [r9].[Name]
FROM [RootEntities] AS [r]
LEFT JOIN (
    SELECT [r0].[RelationshipsTrunkRelationshipsRootId], [r0].[Id1], [r0].[Name], [r1].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r1].[RelationshipsBranchId1], [r1].[Id1] AS [Id10], [r1].[Name] AS [Name0], [r0].[OptionalReferenceLeaf_Name], [r0].[RequiredReferenceLeaf_Name]
    FROM [Root_OptionalReferenceTrunk_CollectionBranch] AS [r0]
    LEFT JOIN [Root_OptionalReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r1] ON [r0].[RelationshipsTrunkRelationshipsRootId] = [r1].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r0].[Id1] = [r1].[RelationshipsBranchId1]
) AS [s] ON CASE
    WHEN [r].[OptionalReferenceTrunk_Name] IS NOT NULL THEN [r].[Id]
END = [s].[RelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_OptionalReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r2] ON CASE
    WHEN [r].[OptionalReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_OptionalReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r3] ON CASE
    WHEN [r].[OptionalReferenceTrunk_RequiredReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r3].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_OptionalReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r4] ON CASE
    WHEN [r].[OptionalReferenceTrunk_RequiredReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r4].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
LEFT JOIN (
    SELECT [r5].[RelationshipsTrunkRelationshipsRootId], [r5].[Id1], [r5].[Name], [r6].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r6].[RelationshipsBranchId1], [r6].[Id1] AS [Id10], [r6].[Name] AS [Name0], [r5].[OptionalReferenceLeaf_Name], [r5].[RequiredReferenceLeaf_Name]
    FROM [Root_OptionalReferenceTrunk_CollectionBranch] AS [r5]
    LEFT JOIN [Root_OptionalReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r6] ON [r5].[RelationshipsTrunkRelationshipsRootId] = [r6].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r5].[Id1] = [r6].[RelationshipsBranchId1]
) AS [s0] ON CASE
    WHEN [r].[OptionalReferenceTrunk_Name] IS NOT NULL THEN [r].[Id]
END = [s0].[RelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_OptionalReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r7] ON CASE
    WHEN [r].[OptionalReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r7].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_OptionalReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r8] ON CASE
    WHEN [r].[OptionalReferenceTrunk_RequiredReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r8].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_OptionalReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r9] ON CASE
    WHEN [r].[OptionalReferenceTrunk_RequiredReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r9].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
ORDER BY [r].[Id], [s].[RelationshipsTrunkRelationshipsRootId], [s].[Id1], [s].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s].[RelationshipsBranchId1], [s].[Id10], [r2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r2].[Id1], [r3].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r3].[Id1], [r4].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r4].[Id1], [s0].[RelationshipsTrunkRelationshipsRootId], [s0].[Id1], [s0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s0].[RelationshipsBranchId1], [s0].[Id10], [r7].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r7].[Id1], [r8].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r8].[Id1], [r9].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
""");
        }
    }

    public override async Task Select_trunk_and_trunk_duplicated(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_trunk_and_trunk_duplicated(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[RequiredReferenceTrunk_Name], [s].[RelationshipsTrunkRelationshipsRootId], [s].[Id1], [s].[Name], [s].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s].[RelationshipsBranchId1], [s].[Id10], [s].[Name0], [s].[OptionalReferenceLeaf_Name], [s].[RequiredReferenceLeaf_Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_Name], [r2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r2].[Id1], [r2].[Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_RequiredReferenceLeaf_Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_Name], [r3].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r3].[Id1], [r3].[Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf_Name], [s0].[RelationshipsTrunkRelationshipsRootId], [s0].[Id1], [s0].[Name], [s0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s0].[RelationshipsBranchId1], [s0].[Id10], [s0].[Name0], [s0].[OptionalReferenceLeaf_Name], [s0].[RequiredReferenceLeaf_Name], [r6].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r6].[Id1], [r6].[Name], [r7].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r7].[Id1], [r7].[Name]
FROM [RootEntities] AS [r]
LEFT JOIN (
    SELECT [r0].[RelationshipsTrunkRelationshipsRootId], [r0].[Id1], [r0].[Name], [r1].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r1].[RelationshipsBranchId1], [r1].[Id1] AS [Id10], [r1].[Name] AS [Name0], [r0].[OptionalReferenceLeaf_Name], [r0].[RequiredReferenceLeaf_Name]
    FROM [Root_RequiredReferenceTrunk_CollectionBranch] AS [r0]
    LEFT JOIN [Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r1] ON [r0].[RelationshipsTrunkRelationshipsRootId] = [r1].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r0].[Id1] = [r1].[RelationshipsBranchId1]
) AS [s] ON [r].[Id] = [s].[RelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_RequiredReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r2] ON CASE
    WHEN [r].[RequiredReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r3] ON [r].[Id] = [r3].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
LEFT JOIN (
    SELECT [r4].[RelationshipsTrunkRelationshipsRootId], [r4].[Id1], [r4].[Name], [r5].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r5].[RelationshipsBranchId1], [r5].[Id1] AS [Id10], [r5].[Name] AS [Name0], [r4].[OptionalReferenceLeaf_Name], [r4].[RequiredReferenceLeaf_Name]
    FROM [Root_RequiredReferenceTrunk_CollectionBranch] AS [r4]
    LEFT JOIN [Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r5] ON [r4].[RelationshipsTrunkRelationshipsRootId] = [r5].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r4].[Id1] = [r5].[RelationshipsBranchId1]
) AS [s0] ON [r].[Id] = [s0].[RelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_RequiredReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r6] ON CASE
    WHEN [r].[RequiredReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r6].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r7] ON [r].[Id] = [r7].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
ORDER BY [r].[Id], [s].[RelationshipsTrunkRelationshipsRootId], [s].[Id1], [s].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s].[RelationshipsBranchId1], [s].[Id10], [r2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r2].[Id1], [r3].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r3].[Id1], [s0].[RelationshipsTrunkRelationshipsRootId], [s0].[Id1], [s0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s0].[RelationshipsBranchId1], [s0].[Id10], [r6].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r6].[Id1], [r7].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
""");
        }
    }

    public override async Task Select_leaf_trunk_root(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_leaf_trunk_root(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf_Name], [r].[RequiredReferenceTrunk_Name], [s].[RelationshipsTrunkRelationshipsRootId], [s].[Id1], [s].[Name], [s].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s].[RelationshipsBranchId1], [s].[Id10], [s].[Name0], [s].[OptionalReferenceLeaf_Name], [s].[RequiredReferenceLeaf_Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_Name], [r2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r2].[Id1], [r2].[Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r].[RequiredReferenceTrunk_OptionalReferenceBranch_RequiredReferenceLeaf_Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_Name], [r3].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r3].[Id1], [r3].[Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [s1].[RelationshipsRootId], [s1].[Id1], [s1].[Name], [s1].[RelationshipsTrunkRelationshipsRootId], [s1].[RelationshipsTrunkId1], [s1].[Id10], [s1].[Name0], [s1].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s1].[RelationshipsBranchRelationshipsTrunkId1], [s1].[RelationshipsBranchId1], [s1].[Id100], [s1].[Name00], [s1].[OptionalReferenceLeaf_Name], [s1].[RequiredReferenceLeaf_Name], [s1].[OptionalReferenceBranch_Name], [s1].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId0], [s1].[RelationshipsBranchRelationshipsTrunkId10], [s1].[Id11], [s1].[Name1], [s1].[OptionalReferenceBranch_OptionalReferenceLeaf_Name], [s1].[OptionalReferenceBranch_RequiredReferenceLeaf_Name], [s1].[RequiredReferenceBranch_Name], [s1].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId1], [s1].[RelationshipsBranchRelationshipsTrunkId11], [s1].[Id12], [s1].[Name2], [s1].[RequiredReferenceBranch_OptionalReferenceLeaf_Name], [s1].[RequiredReferenceBranch_RequiredReferenceLeaf_Name], [r].[OptionalReferenceTrunk_Name], [s2].[RelationshipsTrunkRelationshipsRootId], [s2].[Id1], [s2].[Name], [s2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s2].[RelationshipsBranchId1], [s2].[Id10], [s2].[Name0], [s2].[OptionalReferenceLeaf_Name], [s2].[RequiredReferenceLeaf_Name], [r].[OptionalReferenceTrunk_OptionalReferenceBranch_Name], [r11].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r11].[Id1], [r11].[Name], [r].[OptionalReferenceTrunk_OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r].[OptionalReferenceTrunk_OptionalReferenceBranch_RequiredReferenceLeaf_Name], [r].[OptionalReferenceTrunk_RequiredReferenceBranch_Name], [r12].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r12].[Id1], [r12].[Name], [r].[OptionalReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r].[OptionalReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf_Name], [s3].[RelationshipsTrunkRelationshipsRootId], [s3].[Id1], [s3].[Name], [s3].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s3].[RelationshipsBranchId1], [s3].[Id10], [s3].[Name0], [s3].[OptionalReferenceLeaf_Name], [s3].[RequiredReferenceLeaf_Name], [r15].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r15].[Id1], [r15].[Name], [r16].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r16].[Id1], [r16].[Name]
FROM [RootEntities] AS [r]
LEFT JOIN (
    SELECT [r0].[RelationshipsTrunkRelationshipsRootId], [r0].[Id1], [r0].[Name], [r1].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r1].[RelationshipsBranchId1], [r1].[Id1] AS [Id10], [r1].[Name] AS [Name0], [r0].[OptionalReferenceLeaf_Name], [r0].[RequiredReferenceLeaf_Name]
    FROM [Root_RequiredReferenceTrunk_CollectionBranch] AS [r0]
    LEFT JOIN [Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r1] ON [r0].[RelationshipsTrunkRelationshipsRootId] = [r1].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r0].[Id1] = [r1].[RelationshipsBranchId1]
) AS [s] ON [r].[Id] = [s].[RelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_RequiredReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r2] ON CASE
    WHEN [r].[RequiredReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r3] ON [r].[Id] = [r3].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
LEFT JOIN (
    SELECT [r4].[RelationshipsRootId], [r4].[Id1], [r4].[Name], [s0].[RelationshipsTrunkRelationshipsRootId], [s0].[RelationshipsTrunkId1], [s0].[Id1] AS [Id10], [s0].[Name] AS [Name0], [s0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s0].[RelationshipsBranchRelationshipsTrunkId1], [s0].[RelationshipsBranchId1], [s0].[Id10] AS [Id100], [s0].[Name0] AS [Name00], [s0].[OptionalReferenceLeaf_Name], [s0].[RequiredReferenceLeaf_Name], [r4].[OptionalReferenceBranch_Name], [r7].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AS [RelationshipsBranchRelationshipsTrunkRelationshipsRootId0], [r7].[RelationshipsBranchRelationshipsTrunkId1] AS [RelationshipsBranchRelationshipsTrunkId10], [r7].[Id1] AS [Id11], [r7].[Name] AS [Name1], [r4].[OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r4].[OptionalReferenceBranch_RequiredReferenceLeaf_Name], [r4].[RequiredReferenceBranch_Name], [r8].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AS [RelationshipsBranchRelationshipsTrunkRelationshipsRootId1], [r8].[RelationshipsBranchRelationshipsTrunkId1] AS [RelationshipsBranchRelationshipsTrunkId11], [r8].[Id1] AS [Id12], [r8].[Name] AS [Name2], [r4].[RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r4].[RequiredReferenceBranch_RequiredReferenceLeaf_Name]
    FROM [Root_CollectionTrunk] AS [r4]
    LEFT JOIN (
        SELECT [r5].[RelationshipsTrunkRelationshipsRootId], [r5].[RelationshipsTrunkId1], [r5].[Id1], [r5].[Name], [r6].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r6].[RelationshipsBranchRelationshipsTrunkId1], [r6].[RelationshipsBranchId1], [r6].[Id1] AS [Id10], [r6].[Name] AS [Name0], [r5].[OptionalReferenceLeaf_Name], [r5].[RequiredReferenceLeaf_Name]
        FROM [Root_CollectionTrunk_CollectionBranch] AS [r5]
        LEFT JOIN [Root_CollectionTrunk_CollectionBranch_CollectionLeaf] AS [r6] ON [r5].[RelationshipsTrunkRelationshipsRootId] = [r6].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r5].[RelationshipsTrunkId1] = [r6].[RelationshipsBranchRelationshipsTrunkId1] AND [r5].[Id1] = [r6].[RelationshipsBranchId1]
    ) AS [s0] ON [r4].[RelationshipsRootId] = [s0].[RelationshipsTrunkRelationshipsRootId] AND [r4].[Id1] = [s0].[RelationshipsTrunkId1]
    LEFT JOIN [Root_CollectionTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r7] ON CASE
        WHEN [r4].[OptionalReferenceBranch_Name] IS NOT NULL THEN [r4].[RelationshipsRootId]
    END = [r7].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND CASE
        WHEN [r4].[OptionalReferenceBranch_Name] IS NOT NULL THEN [r4].[Id1]
    END = [r7].[RelationshipsBranchRelationshipsTrunkId1]
    LEFT JOIN [Root_CollectionTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r8] ON [r4].[RelationshipsRootId] = [r8].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r4].[Id1] = [r8].[RelationshipsBranchRelationshipsTrunkId1]
) AS [s1] ON [r].[Id] = [s1].[RelationshipsRootId]
LEFT JOIN (
    SELECT [r9].[RelationshipsTrunkRelationshipsRootId], [r9].[Id1], [r9].[Name], [r10].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r10].[RelationshipsBranchId1], [r10].[Id1] AS [Id10], [r10].[Name] AS [Name0], [r9].[OptionalReferenceLeaf_Name], [r9].[RequiredReferenceLeaf_Name]
    FROM [Root_OptionalReferenceTrunk_CollectionBranch] AS [r9]
    LEFT JOIN [Root_OptionalReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r10] ON [r9].[RelationshipsTrunkRelationshipsRootId] = [r10].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r9].[Id1] = [r10].[RelationshipsBranchId1]
) AS [s2] ON CASE
    WHEN [r].[OptionalReferenceTrunk_Name] IS NOT NULL THEN [r].[Id]
END = [s2].[RelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_OptionalReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r11] ON CASE
    WHEN [r].[OptionalReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r11].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_OptionalReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r12] ON CASE
    WHEN [r].[OptionalReferenceTrunk_RequiredReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r12].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
LEFT JOIN (
    SELECT [r13].[RelationshipsTrunkRelationshipsRootId], [r13].[Id1], [r13].[Name], [r14].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r14].[RelationshipsBranchId1], [r14].[Id1] AS [Id10], [r14].[Name] AS [Name0], [r13].[OptionalReferenceLeaf_Name], [r13].[RequiredReferenceLeaf_Name]
    FROM [Root_RequiredReferenceTrunk_CollectionBranch] AS [r13]
    LEFT JOIN [Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r14] ON [r13].[RelationshipsTrunkRelationshipsRootId] = [r14].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r13].[Id1] = [r14].[RelationshipsBranchId1]
) AS [s3] ON [r].[Id] = [s3].[RelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_RequiredReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r15] ON CASE
    WHEN [r].[RequiredReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r].[Id]
END = [r15].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r16] ON [r].[Id] = [r16].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
ORDER BY [r].[Id], [s].[RelationshipsTrunkRelationshipsRootId], [s].[Id1], [s].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s].[RelationshipsBranchId1], [s].[Id10], [r2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r2].[Id1], [r3].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r3].[Id1], [s1].[RelationshipsRootId], [s1].[Id1], [s1].[RelationshipsTrunkRelationshipsRootId], [s1].[RelationshipsTrunkId1], [s1].[Id10], [s1].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s1].[RelationshipsBranchRelationshipsTrunkId1], [s1].[RelationshipsBranchId1], [s1].[Id100], [s1].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId0], [s1].[RelationshipsBranchRelationshipsTrunkId10], [s1].[Id11], [s1].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId1], [s1].[RelationshipsBranchRelationshipsTrunkId11], [s1].[Id12], [s2].[RelationshipsTrunkRelationshipsRootId], [s2].[Id1], [s2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s2].[RelationshipsBranchId1], [s2].[Id10], [r11].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r11].[Id1], [r12].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r12].[Id1], [s3].[RelationshipsTrunkRelationshipsRootId], [s3].[Id1], [s3].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s3].[RelationshipsBranchId1], [s3].[Id10], [r15].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r15].[Id1], [r16].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
""");
        }
    }

    public override async Task Select_multiple_branch_leaf(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_multiple_branch_leaf(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_Name], [r0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r0].[Id1], [r0].[Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf_Name], [r1].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r1].[Id1], [r1].[Name], [s].[RelationshipsTrunkRelationshipsRootId], [s].[Id1], [s].[Name], [s].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s].[RelationshipsBranchId1], [s].[Id10], [s].[Name0], [s].[OptionalReferenceLeaf_Name], [s].[RequiredReferenceLeaf_Name]
FROM [RootEntities] AS [r]
LEFT JOIN [Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r0] ON [r].[Id] = [r0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r1] ON [r].[Id] = [r1].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
LEFT JOIN (
    SELECT [r2].[RelationshipsTrunkRelationshipsRootId], [r2].[Id1], [r2].[Name], [r3].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r3].[RelationshipsBranchId1], [r3].[Id1] AS [Id10], [r3].[Name] AS [Name0], [r2].[OptionalReferenceLeaf_Name], [r2].[RequiredReferenceLeaf_Name]
    FROM [Root_RequiredReferenceTrunk_CollectionBranch] AS [r2]
    LEFT JOIN [Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r3] ON [r2].[RelationshipsTrunkRelationshipsRootId] = [r3].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r2].[Id1] = [r3].[RelationshipsBranchId1]
) AS [s] ON [r].[Id] = [s].[RelationshipsTrunkRelationshipsRootId]
ORDER BY [r].[Id], [r0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r0].[Id1], [r1].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r1].[Id1], [s].[RelationshipsTrunkRelationshipsRootId], [s].[Id1], [s].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s].[RelationshipsBranchId1]
""");
        }
    }

    #endregion Multiple

    #region Subquery

    public override async Task Select_subquery_root_set_required_trunk_FirstOrDefault_branch(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_subquery_root_set_required_trunk_FirstOrDefault_branch(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT [r2].[Id], [r2].[RequiredReferenceTrunk_RequiredReferenceBranch_Name], [r].[Id], [r1].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r1].[Id1], [r1].[Name], [r2].[RequiredReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r2].[RequiredReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf_Name]
FROM [RootEntities] AS [r]
OUTER APPLY (
    SELECT TOP(1) [r0].[Id], [r0].[RequiredReferenceTrunk_RequiredReferenceBranch_Name], [r0].[RequiredReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r0].[RequiredReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf_Name]
    FROM [RootEntities] AS [r0]
    ORDER BY [r0].[Id]
) AS [r2]
LEFT JOIN [Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r1] ON [r2].[Id] = [r1].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
ORDER BY [r].[Id], [r2].[Id], [r1].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
""");
        }
    }

    public override async Task Select_subquery_root_set_optional_trunk_FirstOrDefault_branch(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_subquery_root_set_optional_trunk_FirstOrDefault_branch(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT [r2].[Id], [r2].[OptionalReferenceTrunk_OptionalReferenceBranch_Name], [r].[Id], [r1].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r1].[Id1], [r1].[Name], [r2].[OptionalReferenceTrunk_OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r2].[OptionalReferenceTrunk_OptionalReferenceBranch_RequiredReferenceLeaf_Name]
FROM [RootEntities] AS [r]
OUTER APPLY (
    SELECT TOP(1) [r0].[Id], [r0].[OptionalReferenceTrunk_OptionalReferenceBranch_Name], [r0].[OptionalReferenceTrunk_OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r0].[OptionalReferenceTrunk_OptionalReferenceBranch_RequiredReferenceLeaf_Name]
    FROM [RootEntities] AS [r0]
    ORDER BY [r0].[Id]
) AS [r2]
LEFT JOIN [Root_OptionalReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r1] ON CASE
    WHEN [r2].[OptionalReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r2].[Id]
END = [r1].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
ORDER BY [r].[Id], [r2].[Id], [r1].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
""");
        }
    }

    public override async Task Select_subquery_root_set_trunk_FirstOrDefault_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_subquery_root_set_trunk_FirstOrDefault_collection(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r3].[Id], [s].[RelationshipsTrunkRelationshipsRootId], [s].[Id1], [s].[Name], [s].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s].[RelationshipsBranchId1], [s].[Id10], [s].[Name0], [s].[OptionalReferenceLeaf_Name], [s].[RequiredReferenceLeaf_Name], [r3].[c]
FROM [RootEntities] AS [r]
OUTER APPLY (
    SELECT TOP(1) 1 AS [c], [r0].[Id]
    FROM [RootEntities] AS [r0]
    ORDER BY [r0].[Id]
) AS [r3]
LEFT JOIN (
    SELECT [r1].[RelationshipsTrunkRelationshipsRootId], [r1].[Id1], [r1].[Name], [r2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r2].[RelationshipsBranchId1], [r2].[Id1] AS [Id10], [r2].[Name] AS [Name0], [r1].[OptionalReferenceLeaf_Name], [r1].[RequiredReferenceLeaf_Name]
    FROM [Root_RequiredReferenceTrunk_CollectionBranch] AS [r1]
    LEFT JOIN [Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r2] ON [r1].[RelationshipsTrunkRelationshipsRootId] = [r2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r1].[Id1] = [r2].[RelationshipsBranchId1]
) AS [s] ON [r3].[Id] = [s].[RelationshipsTrunkRelationshipsRootId]
ORDER BY [r].[Id], [r3].[Id], [s].[RelationshipsTrunkRelationshipsRootId], [s].[Id1], [s].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s].[RelationshipsBranchId1]
""");
        }
    }

    public override async Task Select_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r8].[Id], [s].[RelationshipsTrunkRelationshipsRootId], [s].[Id1], [s].[Name], [s].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s].[RelationshipsBranchId1], [s].[Id10], [s].[Name0], [s].[OptionalReferenceLeaf_Name], [s].[RequiredReferenceLeaf_Name], [r8].[RequiredReferenceTrunk_Name], [s0].[RelationshipsTrunkRelationshipsRootId], [s0].[Id1], [s0].[Name], [s0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s0].[RelationshipsBranchId1], [s0].[Id10], [s0].[Name0], [s0].[OptionalReferenceLeaf_Name], [s0].[RequiredReferenceLeaf_Name], [r8].[RequiredReferenceTrunk_OptionalReferenceBranch_Name], [r5].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r5].[Id1], [r5].[Name], [r8].[RequiredReferenceTrunk_OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r8].[RequiredReferenceTrunk_OptionalReferenceBranch_RequiredReferenceLeaf_Name], [r8].[RequiredReferenceTrunk_RequiredReferenceBranch_Name], [r6].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r6].[Id1], [r6].[Name], [r8].[RequiredReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r8].[RequiredReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf_Name], [r7].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r7].[Id1], [r7].[Name], [r8].[RequiredReferenceTrunk_RequiredReferenceBranch_Name0], [r8].[c]
FROM [RootEntities] AS [r]
OUTER APPLY (
    SELECT TOP(1) [r0].[Id], [r0].[RequiredReferenceTrunk_Name], [r0].[RequiredReferenceTrunk_OptionalReferenceBranch_Name], [r0].[RequiredReferenceTrunk_OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r0].[RequiredReferenceTrunk_OptionalReferenceBranch_RequiredReferenceLeaf_Name], [r0].[RequiredReferenceTrunk_RequiredReferenceBranch_Name], [r0].[RequiredReferenceTrunk_RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r0].[RequiredReferenceTrunk_RequiredReferenceBranch_RequiredReferenceLeaf_Name], [r].[RequiredReferenceTrunk_RequiredReferenceBranch_Name] AS [RequiredReferenceTrunk_RequiredReferenceBranch_Name0], 1 AS [c]
    FROM [RootEntities] AS [r0]
    ORDER BY [r0].[Id]
) AS [r8]
LEFT JOIN (
    SELECT [r1].[RelationshipsTrunkRelationshipsRootId], [r1].[Id1], [r1].[Name], [r2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r2].[RelationshipsBranchId1], [r2].[Id1] AS [Id10], [r2].[Name] AS [Name0], [r1].[OptionalReferenceLeaf_Name], [r1].[RequiredReferenceLeaf_Name]
    FROM [Root_RequiredReferenceTrunk_CollectionBranch] AS [r1]
    LEFT JOIN [Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r2] ON [r1].[RelationshipsTrunkRelationshipsRootId] = [r2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r1].[Id1] = [r2].[RelationshipsBranchId1]
) AS [s] ON [r].[Id] = [s].[RelationshipsTrunkRelationshipsRootId]
LEFT JOIN (
    SELECT [r3].[RelationshipsTrunkRelationshipsRootId], [r3].[Id1], [r3].[Name], [r4].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r4].[RelationshipsBranchId1], [r4].[Id1] AS [Id10], [r4].[Name] AS [Name0], [r3].[OptionalReferenceLeaf_Name], [r3].[RequiredReferenceLeaf_Name]
    FROM [Root_RequiredReferenceTrunk_CollectionBranch] AS [r3]
    LEFT JOIN [Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r4] ON [r3].[RelationshipsTrunkRelationshipsRootId] = [r4].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r3].[Id1] = [r4].[RelationshipsBranchId1]
) AS [s0] ON [r8].[Id] = [s0].[RelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_RequiredReferenceTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r5] ON CASE
    WHEN [r8].[RequiredReferenceTrunk_OptionalReferenceBranch_Name] IS NOT NULL THEN [r8].[Id]
END = [r5].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r6] ON [r8].[Id] = [r6].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_RequiredReferenceTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r7] ON [r8].[Id] = [r7].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
ORDER BY [r].[Id], [r8].[Id], [s].[RelationshipsTrunkRelationshipsRootId], [s].[Id1], [s].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s].[RelationshipsBranchId1], [s].[Id10], [s0].[RelationshipsTrunkRelationshipsRootId], [s0].[Id1], [s0].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s0].[RelationshipsBranchId1], [s0].[Id10], [r5].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r5].[Id1], [r6].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r6].[Id1], [r7].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId]
""");
        }
    }

    public override async Task Select_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r3].[Id], [s].[RelationshipsTrunkRelationshipsRootId], [s].[Id1], [s].[Name], [s].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s].[RelationshipsBranchId1], [s].[Id10], [s].[Name0], [s].[OptionalReferenceLeaf_Name], [s].[RequiredReferenceLeaf_Name], [r3].[c]
FROM [RootEntities] AS [r]
OUTER APPLY (
    SELECT TOP(1) 1 AS [c], [r0].[Id]
    FROM [RootEntities] AS [r0]
    ORDER BY [r0].[Id]
) AS [r3]
LEFT JOIN (
    SELECT [r1].[RelationshipsTrunkRelationshipsRootId], [r1].[Id1], [r1].[Name], [r2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r2].[RelationshipsBranchId1], [r2].[Id1] AS [Id10], [r2].[Name] AS [Name0], [r1].[OptionalReferenceLeaf_Name], [r1].[RequiredReferenceLeaf_Name]
    FROM [Root_RequiredReferenceTrunk_CollectionBranch] AS [r1]
    LEFT JOIN [Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r2] ON [r1].[RelationshipsTrunkRelationshipsRootId] = [r2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r1].[Id1] = [r2].[RelationshipsBranchId1]
) AS [s] ON [r].[Id] = [s].[RelationshipsTrunkRelationshipsRootId]
ORDER BY [r].[Id], [r3].[Id], [s].[RelationshipsTrunkRelationshipsRootId], [s].[Id1], [s].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s].[RelationshipsBranchId1]
""");
        }
    }

    #endregion Subquery

    #region SelectMany

    public override async Task SelectMany_trunk_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_trunk_collection(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT [r0].[RelationshipsRootId], [r0].[Id1], [r0].[Name], [r].[Id], [s].[RelationshipsTrunkRelationshipsRootId], [s].[RelationshipsTrunkId1], [s].[Id1], [s].[Name], [s].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s].[RelationshipsBranchRelationshipsTrunkId1], [s].[RelationshipsBranchId1], [s].[Id10], [s].[Name0], [s].[OptionalReferenceLeaf_Name], [s].[RequiredReferenceLeaf_Name], [r0].[OptionalReferenceBranch_Name], [r3].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r3].[RelationshipsBranchRelationshipsTrunkId1], [r3].[Id1], [r3].[Name], [r0].[OptionalReferenceBranch_OptionalReferenceLeaf_Name], [r0].[OptionalReferenceBranch_RequiredReferenceLeaf_Name], [r0].[RequiredReferenceBranch_Name], [r4].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r4].[RelationshipsBranchRelationshipsTrunkId1], [r4].[Id1], [r4].[Name], [r0].[RequiredReferenceBranch_OptionalReferenceLeaf_Name], [r0].[RequiredReferenceBranch_RequiredReferenceLeaf_Name]
FROM [RootEntities] AS [r]
INNER JOIN [Root_CollectionTrunk] AS [r0] ON [r].[Id] = [r0].[RelationshipsRootId]
LEFT JOIN (
    SELECT [r1].[RelationshipsTrunkRelationshipsRootId], [r1].[RelationshipsTrunkId1], [r1].[Id1], [r1].[Name], [r2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r2].[RelationshipsBranchRelationshipsTrunkId1], [r2].[RelationshipsBranchId1], [r2].[Id1] AS [Id10], [r2].[Name] AS [Name0], [r1].[OptionalReferenceLeaf_Name], [r1].[RequiredReferenceLeaf_Name]
    FROM [Root_CollectionTrunk_CollectionBranch] AS [r1]
    LEFT JOIN [Root_CollectionTrunk_CollectionBranch_CollectionLeaf] AS [r2] ON [r1].[RelationshipsTrunkRelationshipsRootId] = [r2].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r1].[RelationshipsTrunkId1] = [r2].[RelationshipsBranchRelationshipsTrunkId1] AND [r1].[Id1] = [r2].[RelationshipsBranchId1]
) AS [s] ON [r0].[RelationshipsRootId] = [s].[RelationshipsTrunkRelationshipsRootId] AND [r0].[Id1] = [s].[RelationshipsTrunkId1]
LEFT JOIN [Root_CollectionTrunk_OptionalReferenceBranch_CollectionLeaf] AS [r3] ON CASE
    WHEN [r0].[OptionalReferenceBranch_Name] IS NOT NULL THEN [r0].[RelationshipsRootId]
END = [r3].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND CASE
    WHEN [r0].[OptionalReferenceBranch_Name] IS NOT NULL THEN [r0].[Id1]
END = [r3].[RelationshipsBranchRelationshipsTrunkId1]
LEFT JOIN [Root_CollectionTrunk_RequiredReferenceBranch_CollectionLeaf] AS [r4] ON [r0].[RelationshipsRootId] = [r4].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r0].[Id1] = [r4].[RelationshipsBranchRelationshipsTrunkId1]
ORDER BY [r].[Id], [r0].[RelationshipsRootId], [r0].[Id1], [s].[RelationshipsTrunkRelationshipsRootId], [s].[RelationshipsTrunkId1], [s].[Id1], [s].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [s].[RelationshipsBranchRelationshipsTrunkId1], [s].[RelationshipsBranchId1], [s].[Id10], [r3].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r3].[RelationshipsBranchRelationshipsTrunkId1], [r3].[Id1], [r4].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r4].[RelationshipsBranchRelationshipsTrunkId1]
""");
        }
    }

    public override async Task SelectMany_required_trunk_reference_branch_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_required_trunk_reference_branch_collection(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT [r0].[RelationshipsTrunkRelationshipsRootId], [r0].[Id1], [r0].[Name], [r].[Id], [r1].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r1].[RelationshipsBranchId1], [r1].[Id1], [r1].[Name], [r0].[OptionalReferenceLeaf_Name], [r0].[RequiredReferenceLeaf_Name]
FROM [RootEntities] AS [r]
INNER JOIN [Root_RequiredReferenceTrunk_CollectionBranch] AS [r0] ON [r].[Id] = [r0].[RelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_RequiredReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r1] ON [r0].[RelationshipsTrunkRelationshipsRootId] = [r1].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r0].[Id1] = [r1].[RelationshipsBranchId1]
ORDER BY [r].[Id], [r0].[RelationshipsTrunkRelationshipsRootId], [r0].[Id1], [r1].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r1].[RelationshipsBranchId1]
""");
        }
    }

    public override async Task SelectMany_optional_trunk_reference_branch_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_optional_trunk_reference_branch_collection(async, queryTrackingBehavior);

        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            AssertSql();
        }
        else
        {
            AssertSql(
                """
SELECT [r0].[RelationshipsTrunkRelationshipsRootId], [r0].[Id1], [r0].[Name], [r].[Id], [r1].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r1].[RelationshipsBranchId1], [r1].[Id1], [r1].[Name], [r0].[OptionalReferenceLeaf_Name], [r0].[RequiredReferenceLeaf_Name]
FROM [RootEntities] AS [r]
INNER JOIN [Root_OptionalReferenceTrunk_CollectionBranch] AS [r0] ON CASE
    WHEN [r].[OptionalReferenceTrunk_Name] IS NOT NULL THEN [r].[Id]
END = [r0].[RelationshipsTrunkRelationshipsRootId]
LEFT JOIN [Root_OptionalReferenceTrunk_CollectionBranch_CollectionLeaf] AS [r1] ON [r0].[RelationshipsTrunkRelationshipsRootId] = [r1].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId] AND [r0].[Id1] = [r1].[RelationshipsBranchId1]
ORDER BY [r].[Id], [r0].[RelationshipsTrunkRelationshipsRootId], [r0].[Id1], [r1].[RelationshipsBranchRelationshipsTrunkRelationshipsRootId], [r1].[RelationshipsBranchId1]
""");
        }
    }

    #endregion SelectMany

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
