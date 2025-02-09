// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.InProjection;

public class OwnedRelationshipsInProjectionQuerySqlServerTest
    : OwnedRelationshipsInProjectionQueryRelationalTestBase<OwnedRelationshipsQuerySqlServerFixture>
{
    public OwnedRelationshipsInProjectionQuerySqlServerTest(OwnedRelationshipsQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
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

    public override Task Project_trunk_optional(bool async)
        => AssertCantTrackOwned(() => base.Project_trunk_optional(async));

    public override Task Project_trunk_required(bool async)
        => AssertCantTrackOwned(() => base.Project_trunk_required(async));

    public override Task Project_trunk_collection(bool async)
        => AssertCantTrackOwned(() => base.Project_trunk_collection(async));

    public override Task Project_trunk_required_required(bool async)
        => AssertCantTrackOwned(() => base.Project_trunk_required_required(async));

    public override Task Project_trunk_required_optional(bool async)
        => AssertCantTrackOwned(() => base.Project_trunk_required_optional(async));

    public override Task Project_trunk_required_collection(bool async)
        => AssertCantTrackOwned(() => base.Project_trunk_required_collection(async));

    public override Task Project_trunk_optional_required(bool async)
        => AssertCantTrackOwned(() => base.Project_trunk_optional_required(async));

    public override Task Project_trunk_optional_optional(bool async)
        => AssertCantTrackOwned(() => base.Project_trunk_optional_optional(async));

    public override Task Project_trunk_optional_collection(bool async)
        => AssertCantTrackOwned(() => base.Project_trunk_optional_collection(async));

    private async Task AssertCantTrackOwned(Func<Task> test)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(test)).Message;

        Assert.Equal(CoreStrings.OwnedEntitiesCannotBeTrackedWithoutTheirOwner, message);
        AssertSql();
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
