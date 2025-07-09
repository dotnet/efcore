// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Navigations;

public class NavigationsIncludeSqlServerTest
    : NavigationsIncludeRelationalTestBase<NavigationsSqlServerFixture>
{
    public NavigationsIncludeSqlServerTest(NavigationsSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Include_trunk_required(bool async)
    {
        await base.Include_trunk_required(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [t].[Id], [t].[CollectionRootId], [t].[Name], [t].[OptionalReferenceBranchId], [t].[RequiredReferenceBranchId]
FROM [RootEntities] AS [r]
INNER JOIN [TrunkEntities] AS [t] ON [r].[RequiredReferenceTrunkId] = [t].[Id]
""");
    }

    public override async Task Include_trunk_optional(bool async)
    {
        await base.Include_trunk_optional(async);

        AssertSql(
"""
SELECT [r].[Id], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [t].[Id], [t].[CollectionRootId], [t].[Name], [t].[OptionalReferenceBranchId], [t].[RequiredReferenceBranchId]
FROM [RootEntities] AS [r]
LEFT JOIN [TrunkEntities] AS [t] ON [r].[OptionalReferenceTrunkId] = [t].[Id]
""");
    }

    public override async Task Include_trunk_collection(bool async)
    {
        await base.Include_trunk_collection(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [t].[Id], [t].[CollectionRootId], [t].[Name], [t].[OptionalReferenceBranchId], [t].[RequiredReferenceBranchId]
FROM [RootEntities] AS [r]
LEFT JOIN [TrunkEntities] AS [t] ON [r].[Id] = [t].[CollectionRootId]
ORDER BY [r].[Id]
""");
    }

    public override async Task Include_trunk_required_optional_and_collection(bool async)
    {
        await base.Include_trunk_required_optional_and_collection(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [t].[Id], [t].[CollectionRootId], [t].[Name], [t].[OptionalReferenceBranchId], [t].[RequiredReferenceBranchId], [t0].[Id], [t0].[CollectionRootId], [t0].[Name], [t0].[OptionalReferenceBranchId], [t0].[RequiredReferenceBranchId], [t1].[Id], [t1].[CollectionRootId], [t1].[Name], [t1].[OptionalReferenceBranchId], [t1].[RequiredReferenceBranchId]
FROM [RootEntities] AS [r]
INNER JOIN [TrunkEntities] AS [t] ON [r].[RequiredReferenceTrunkId] = [t].[Id]
LEFT JOIN [TrunkEntities] AS [t0] ON [r].[OptionalReferenceTrunkId] = [t0].[Id]
LEFT JOIN [TrunkEntities] AS [t1] ON [r].[Id] = [t1].[CollectionRootId]
ORDER BY [r].[Id], [t].[Id], [t0].[Id]
""");
    }

    public override async Task Include_branch_required_required(bool async)
    {
        await base.Include_branch_required_required(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [t].[Id], [t].[CollectionRootId], [t].[Name], [t].[OptionalReferenceBranchId], [t].[RequiredReferenceBranchId], [b].[Id], [b].[CollectionTrunkId], [b].[Name], [b].[OptionalReferenceLeafId], [b].[RequiredReferenceLeafId]
FROM [RootEntities] AS [r]
INNER JOIN [TrunkEntities] AS [t] ON [r].[RequiredReferenceTrunkId] = [t].[Id]
INNER JOIN [BranchEntities] AS [b] ON [t].[RequiredReferenceBranchId] = [b].[Id]
""");
    }

    public override async Task Include_branch_required_collection(bool async)
    {
        await base.Include_branch_required_collection(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [t].[Id], [t].[CollectionRootId], [t].[Name], [t].[OptionalReferenceBranchId], [t].[RequiredReferenceBranchId], [b].[Id], [b].[CollectionTrunkId], [b].[Name], [b].[OptionalReferenceLeafId], [b].[RequiredReferenceLeafId]
FROM [RootEntities] AS [r]
INNER JOIN [TrunkEntities] AS [t] ON [r].[RequiredReferenceTrunkId] = [t].[Id]
LEFT JOIN [BranchEntities] AS [b] ON [t].[Id] = [b].[CollectionTrunkId]
ORDER BY [r].[Id], [t].[Id]
""");
    }

    public override async Task Include_branch_optional_optional(bool async)
    {
        await base.Include_branch_optional_optional(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [t].[Id], [t].[CollectionRootId], [t].[Name], [t].[OptionalReferenceBranchId], [t].[RequiredReferenceBranchId], [b].[Id], [b].[CollectionTrunkId], [b].[Name], [b].[OptionalReferenceLeafId], [b].[RequiredReferenceLeafId]
FROM [RootEntities] AS [r]
LEFT JOIN [TrunkEntities] AS [t] ON [r].[OptionalReferenceTrunkId] = [t].[Id]
LEFT JOIN [BranchEntities] AS [b] ON [t].[OptionalReferenceBranchId] = [b].[Id]
""");
    }

    public override async Task Include_branch_optional_collection(bool async)
    {
        await base.Include_branch_optional_collection(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [t].[Id], [t].[CollectionRootId], [t].[Name], [t].[OptionalReferenceBranchId], [t].[RequiredReferenceBranchId], [b].[Id], [b].[CollectionTrunkId], [b].[Name], [b].[OptionalReferenceLeafId], [b].[RequiredReferenceLeafId]
FROM [RootEntities] AS [r]
LEFT JOIN [TrunkEntities] AS [t] ON [r].[OptionalReferenceTrunkId] = [t].[Id]
LEFT JOIN [BranchEntities] AS [b] ON [t].[Id] = [b].[CollectionTrunkId]
ORDER BY [r].[Id], [t].[Id]
""");
    }

    public override async Task Include_branch_collection_collection(bool async)
    {
        await base.Include_branch_collection_collection(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [s].[Id], [s].[CollectionRootId], [s].[Name], [s].[OptionalReferenceBranchId], [s].[RequiredReferenceBranchId], [s].[Id0], [s].[CollectionTrunkId], [s].[Name0], [s].[OptionalReferenceLeafId], [s].[RequiredReferenceLeafId]
FROM [RootEntities] AS [r]
LEFT JOIN (
    SELECT [t].[Id], [t].[CollectionRootId], [t].[Name], [t].[OptionalReferenceBranchId], [t].[RequiredReferenceBranchId], [b].[Id] AS [Id0], [b].[CollectionTrunkId], [b].[Name] AS [Name0], [b].[OptionalReferenceLeafId], [b].[RequiredReferenceLeafId]
    FROM [TrunkEntities] AS [t]
    LEFT JOIN [BranchEntities] AS [b] ON [t].[Id] = [b].[CollectionTrunkId]
) AS [s] ON [r].[Id] = [s].[CollectionRootId]
ORDER BY [r].[Id], [s].[Id]
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
