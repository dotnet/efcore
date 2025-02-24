// Licensed to the .NET Foundation under one or more agreements.
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

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
