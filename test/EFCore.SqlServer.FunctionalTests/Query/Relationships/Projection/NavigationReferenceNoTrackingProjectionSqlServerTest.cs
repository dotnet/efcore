// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Projection;

public class NavigationReferenceNoTrackingProjectionSqlServerTest
    : NavigationReferenceNoTrackingProjectionRelationalTestBase<NavigationRelationshipsSqlServerFixture>
{
    public NavigationReferenceNoTrackingProjectionSqlServerTest(NavigationRelationshipsSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Select_everything(bool async)
    {
        await base.Select_everything(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [t].[Id], [t].[CollectionRootId], [t].[Name], [t].[OptionalReferenceBranchId], [t].[RequiredReferenceBranchId], [b].[Id], [b].[CollectionTrunkId], [b].[Name], [b].[OptionalReferenceLeafId], [b].[RequiredReferenceLeafId], [l].[Id], [l].[CollectionBranchId], [l].[Name]
FROM [RootEntities] AS [r]
INNER JOIN [TrunkEntities] AS [t] ON [r].[Id] = [t].[Id]
INNER JOIN [BranchEntities] AS [b] ON [t].[Id] = [b].[Id]
INNER JOIN [LeafEntities] AS [l] ON [b].[Id] = [l].[Id]
""");
    }

    public override async Task Select_root(bool async)
    {
        await base.Select_root(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId]
FROM [RootEntities] AS [r]
""");
    }

    public override async Task Select_trunk_optional(bool async)
    {
        await base.Select_trunk_optional(async);

        AssertSql(
            """
SELECT [t].[Id], [t].[CollectionRootId], [t].[Name], [t].[OptionalReferenceBranchId], [t].[RequiredReferenceBranchId]
FROM [RootEntities] AS [r]
LEFT JOIN [TrunkEntities] AS [t] ON [r].[OptionalReferenceTrunkId] = [t].[Id]
ORDER BY [r].[Id]
""");
    }

    public override async Task Select_trunk_required(bool async)
    {
        await base.Select_trunk_required(async);

        AssertSql(
            """
SELECT [t].[Id], [t].[CollectionRootId], [t].[Name], [t].[OptionalReferenceBranchId], [t].[RequiredReferenceBranchId]
FROM [RootEntities] AS [r]
INNER JOIN [TrunkEntities] AS [t] ON [r].[RequiredReferenceTrunkId] = [t].[Id]
ORDER BY [r].[Id]
""");
    }

    public override async Task Select_branch_required_required(bool async)
    {
        await base.Select_branch_required_required(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[CollectionTrunkId], [b].[Name], [b].[OptionalReferenceLeafId], [b].[RequiredReferenceLeafId]
FROM [RootEntities] AS [r]
INNER JOIN [TrunkEntities] AS [t] ON [r].[RequiredReferenceTrunkId] = [t].[Id]
INNER JOIN [BranchEntities] AS [b] ON [t].[RequiredReferenceBranchId] = [b].[Id]
ORDER BY [r].[Id]
""");
    }

    public override async Task Select_branch_required_optional(bool async)
    {
        await base.Select_branch_required_optional(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[CollectionTrunkId], [b].[Name], [b].[OptionalReferenceLeafId], [b].[RequiredReferenceLeafId]
FROM [RootEntities] AS [r]
INNER JOIN [TrunkEntities] AS [t] ON [r].[RequiredReferenceTrunkId] = [t].[Id]
LEFT JOIN [BranchEntities] AS [b] ON [t].[OptionalReferenceBranchId] = [b].[Id]
ORDER BY [r].[Id]
""");
    }

    public override async Task Select_branch_optional_required(bool async)
    {
        await base.Select_branch_optional_required(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[CollectionTrunkId], [b].[Name], [b].[OptionalReferenceLeafId], [b].[RequiredReferenceLeafId]
FROM [RootEntities] AS [r]
INNER JOIN [TrunkEntities] AS [t] ON [r].[RequiredReferenceTrunkId] = [t].[Id]
INNER JOIN [BranchEntities] AS [b] ON [t].[RequiredReferenceBranchId] = [b].[Id]
ORDER BY [r].[Id]
""");
    }

    public override async Task Select_branch_optional_optional(bool async)
    {
        await base.Select_branch_optional_optional(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[CollectionTrunkId], [b].[Name], [b].[OptionalReferenceLeafId], [b].[RequiredReferenceLeafId]
FROM [RootEntities] AS [r]
INNER JOIN [TrunkEntities] AS [t] ON [r].[RequiredReferenceTrunkId] = [t].[Id]
LEFT JOIN [BranchEntities] AS [b] ON [t].[OptionalReferenceBranchId] = [b].[Id]
ORDER BY [r].[Id]
""");
    }

    public override async Task Select_root_duplicated(bool async)
    {
        await base.Select_root_duplicated(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId]
FROM [RootEntities] AS [r]
""");
    }

    public override async Task Select_trunk_and_branch_duplicated(bool async)
    {
        await base.Select_trunk_and_branch_duplicated(async);

        AssertSql(
            """
SELECT [t].[Id], [t].[CollectionRootId], [t].[Name], [t].[OptionalReferenceBranchId], [t].[RequiredReferenceBranchId], [b].[Id], [b].[CollectionTrunkId], [b].[Name], [b].[OptionalReferenceLeafId], [b].[RequiredReferenceLeafId]
FROM [RootEntities] AS [r]
LEFT JOIN [TrunkEntities] AS [t] ON [r].[OptionalReferenceTrunkId] = [t].[Id]
LEFT JOIN [BranchEntities] AS [b] ON [t].[RequiredReferenceBranchId] = [b].[Id]
ORDER BY [r].[Id]
""");
    }

    public override async Task Select_trunk_and_trunk_duplicated(bool async)
    {
        await base.Select_trunk_and_trunk_duplicated(async);

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

    public override async Task Select_leaf_trunk_root(bool async)
    {
        await base.Select_leaf_trunk_root(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[CollectionBranchId], [l].[Name], [t].[Id], [t].[CollectionRootId], [t].[Name], [t].[OptionalReferenceBranchId], [t].[RequiredReferenceBranchId], [r].[Id], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId]
FROM [RootEntities] AS [r]
INNER JOIN [TrunkEntities] AS [t] ON [r].[RequiredReferenceTrunkId] = [t].[Id]
INNER JOIN [BranchEntities] AS [b] ON [t].[RequiredReferenceBranchId] = [b].[Id]
INNER JOIN [LeafEntities] AS [l] ON [b].[RequiredReferenceLeafId] = [l].[Id]
""");
    }

    public override async Task Select_subquery_root_set_required_trunk_FirstOrDefault_branch(bool async)
    {
        await base.Select_subquery_root_set_required_trunk_FirstOrDefault_branch(async);

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

    public override async Task Select_subquery_root_set_optional_trunk_FirstOrDefault_branch(bool async)
    {
        await base.Select_subquery_root_set_optional_trunk_FirstOrDefault_branch(async);

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

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
