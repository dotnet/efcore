﻿// Licensed to the .NET Foundation under one or more agreements.
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

        AssertSql();
    }

    public override async Task Project_trunk_and_trunk_duplicated(bool async)
    {
        await base.Project_trunk_and_trunk_duplicated(async);

        AssertSql();
    }

    public override async Task Project_multiple_branch_leaf(bool async)
    {
        await base.Project_multiple_branch_leaf(async);

        AssertSql();
    }

    public override async Task Project_leaf_trunk_root(bool async)
    {
        await base.Project_leaf_trunk_root(async);

        AssertSql(
            """
SELECT [t].[RequiredReferenceBranch_RequiredReferenceLeaf_Name], [t].[Id], [t].[CollectionRootId], [t].[Name], [t].[OptionalReferenceBranchId], [t].[RequiredReferenceBranchId], [t].[RequiredReferenceBranch_Name], [r].[Id], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId]
FROM [RootEntities] AS [r]
INNER JOIN [TrunkEntities] AS [t] ON [r].[RequiredReferenceTrunkId] = [t].[Id]
""");
    }

    public override async Task Project_subquery_root_set_required_trunk_FirstOrDefault_branch(bool async)
    {
        await base.Project_subquery_root_set_required_trunk_FirstOrDefault_branch(async);

        AssertSql();
    }

    public override async Task Project_subquery_root_set_optional_trunk_FirstOrDefault_branch(bool async)
    {
        await base.Project_subquery_root_set_optional_trunk_FirstOrDefault_branch(async);

        AssertSql();
    }

    public override async Task Project_subquery_root_set_trunk_FirstOrDefault_collection(bool async)
    {
        await base.Project_subquery_root_set_trunk_FirstOrDefault_collection(async);

        AssertSql();
    }

    public override async Task Project_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(bool async)
    {
        await base.Project_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(async);

        AssertSql();
    }

    public override async Task Project_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(bool async)
    {
        await base.Project_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(async);

        AssertSql();
    }

    public override async Task SelectMany_trunk_collection(bool async)
    {
        await base.SelectMany_trunk_collection(async);

        AssertSql();
    }

    public override async Task SelectMany_required_trunk_reference_branch_collection(bool async)
    {
        await base.SelectMany_required_trunk_reference_branch_collection(async);

        AssertSql();
    }

    public override async Task SelectMany_optional_trunk_reference_branch_collection(bool async)
    {
        await base.SelectMany_optional_trunk_reference_branch_collection(async);

        AssertSql();
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
