// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.InProjection;

public class ComplexRelationshipsInProjectionQuerySqlServerTest
    : ComplexRelationshipsInProjectionQueryRelationalTestBase<ComplexRelationshipsQuerySqlServerFixture>
{
    public ComplexRelationshipsInProjectionQuerySqlServerTest(ComplexRelationshipsQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
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
SELECT [r].[Id], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [t].[Id], [t].[CollectionRootId], [t].[Name], [t].[OptionalReferenceBranchId], [t].[RequiredReferenceBranchId], [t].[RequiredReferenceBranch_Name], [t].[RequiredReferenceBranch_RequiredReferenceLeaf_Name]
FROM [RootEntities] AS [r]
INNER JOIN [TrunkEntities] AS [t] ON [r].[Id] = [t].[Id]
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
SELECT [t].[Id], [t].[CollectionRootId], [t].[Name], [t].[OptionalReferenceBranchId], [t].[RequiredReferenceBranchId], [t].[RequiredReferenceBranch_Name], [t].[RequiredReferenceBranch_RequiredReferenceLeaf_Name]
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
SELECT [t].[Id], [t].[CollectionRootId], [t].[Name], [t].[OptionalReferenceBranchId], [t].[RequiredReferenceBranchId], [t].[RequiredReferenceBranch_Name], [t].[RequiredReferenceBranch_RequiredReferenceLeaf_Name]
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
SELECT [r].[Id], [t].[Id], [t].[CollectionRootId], [t].[Name], [t].[OptionalReferenceBranchId], [t].[RequiredReferenceBranchId], [t].[RequiredReferenceBranch_Name], [t].[RequiredReferenceBranch_RequiredReferenceLeaf_Name]
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
SELECT [t].[RequiredReferenceBranch_Name], [t].[RequiredReferenceBranch_RequiredReferenceLeaf_Name]
FROM [RootEntities] AS [r]
INNER JOIN [TrunkEntities] AS [t] ON [r].[RequiredReferenceTrunkId] = [t].[Id]
ORDER BY [r].[Id]
""");
    }

    public override async Task Project_branch_required_optional(bool async)
    {
        await base.Project_branch_required_optional(async);

        AssertSql();
    }

    public override async Task Project_branch_required_collection(bool async)
    {
        await base.Project_branch_required_collection(async);

        AssertSql();
    }       

    public override async Task Project_branch_optional_required(bool async)
    {
        await base.Project_branch_optional_required(async);

        AssertSql();
    }

    public override async Task Project_branch_optional_optional(bool async)
    {
        await base.Project_branch_optional_optional(async);

        AssertSql();
    }

    public override async Task Project_branch_optional_collection(bool async)
    {
        await base.Project_branch_optional_collection(async);

        AssertSql();
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
