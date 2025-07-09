// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.ComplexTableSplitting;

public class ComplexTableSplittingProjectionSqlServerTest
    : ComplexTableSplittingProjectionRelationalTestBase<ComplexTableSplittingSqlServerFixture>
{
    public ComplexTableSplittingProjectionSqlServerTest(ComplexTableSplittingSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
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
SELECT [r].[Id], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId]
FROM [RootEntities] AS [r]
""");
    }

    public override async Task Select_trunk_optional(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_trunk_optional(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [t].[Id], [t].[CollectionRootId], [t].[Name], [t].[OptionalReferenceBranchId], [t].[RequiredReferenceBranchId], [t].[RequiredReferenceBranch_Name], [t].[RequiredReferenceBranch_RequiredReferenceLeaf_Name]
FROM [RootEntities] AS [r]
LEFT JOIN [TrunkEntities] AS [t] ON [r].[OptionalReferenceTrunkId] = [t].[Id]
ORDER BY [r].[Id]
""");
    }

    public override async Task Select_trunk_required(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_trunk_required(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [t].[Id], [t].[CollectionRootId], [t].[Name], [t].[OptionalReferenceBranchId], [t].[RequiredReferenceBranchId], [t].[RequiredReferenceBranch_Name], [t].[RequiredReferenceBranch_RequiredReferenceLeaf_Name]
FROM [RootEntities] AS [r]
INNER JOIN [TrunkEntities] AS [t] ON [r].[RequiredReferenceTrunkId] = [t].[Id]
ORDER BY [r].[Id]
""");
    }

    public override async Task Select_trunk_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_trunk_collection(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [t].[Id], [t].[CollectionRootId], [t].[Name], [t].[OptionalReferenceBranchId], [t].[RequiredReferenceBranchId], [t].[RequiredReferenceBranch_Name], [t].[RequiredReferenceBranch_RequiredReferenceLeaf_Name]
FROM [RootEntities] AS [r]
LEFT JOIN [TrunkEntities] AS [t] ON [r].[Id] = [t].[CollectionRootId]
ORDER BY [r].[Id]
""");
    }

    public override async Task Select_branch_required_required(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_branch_required_required(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [t].[RequiredReferenceBranch_Name], [t].[RequiredReferenceBranch_RequiredReferenceLeaf_Name]
FROM [RootEntities] AS [r]
INNER JOIN [TrunkEntities] AS [t] ON [r].[RequiredReferenceTrunkId] = [t].[Id]
ORDER BY [r].[Id]
""");
    }

    public override async Task Select_branch_required_optional(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_branch_required_optional(async, queryTrackingBehavior);

        AssertSql();
    }

    public override async Task Select_branch_optional_required(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_branch_optional_required(async, queryTrackingBehavior);

        AssertSql();
    }

    public override async Task Select_branch_optional_optional(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_branch_optional_optional(async, queryTrackingBehavior);

        AssertSql();
    }

    public override async Task Select_branch_required_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_branch_required_collection(async, queryTrackingBehavior);

        AssertSql();
    }

    public override async Task Select_branch_optional_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_branch_optional_collection(async, queryTrackingBehavior);

        AssertSql();
    }

    #region Multiple

    public override async Task Select_root_duplicated(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_root_duplicated(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId]
FROM [RootEntities] AS [r]
""");
    }

    public override async Task Select_trunk_and_branch_duplicated(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_trunk_and_branch_duplicated(async, queryTrackingBehavior);

        AssertSql();
    }

    public override async Task Select_trunk_and_trunk_duplicated(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_trunk_and_trunk_duplicated(async, queryTrackingBehavior);

        AssertSql();
    }

    public override async Task Select_leaf_trunk_root(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_leaf_trunk_root(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [t].[RequiredReferenceBranch_RequiredReferenceLeaf_Name], [t].[Id], [t].[CollectionRootId], [t].[Name], [t].[OptionalReferenceBranchId], [t].[RequiredReferenceBranchId], [t].[RequiredReferenceBranch_Name], [r].[Id], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId]
FROM [RootEntities] AS [r]
INNER JOIN [TrunkEntities] AS [t] ON [r].[RequiredReferenceTrunkId] = [t].[Id]
""");
    }

    public override async Task Select_multiple_branch_leaf(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_multiple_branch_leaf(async, queryTrackingBehavior);

        AssertSql();
    }

    #endregion Multiple

    #region Subquery

    public override async Task Select_subquery_root_set_required_trunk_FirstOrDefault_branch(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_subquery_root_set_required_trunk_FirstOrDefault_branch(async, queryTrackingBehavior);

        AssertSql();
    }

    public override async Task Select_subquery_root_set_optional_trunk_FirstOrDefault_branch(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_subquery_root_set_optional_trunk_FirstOrDefault_branch(async, queryTrackingBehavior);

        AssertSql();
    }

    public override async Task Select_everything(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_everything(async, queryTrackingBehavior);

        AssertSql(
"""
SELECT [r].[Id], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [t].[Id], [t].[CollectionRootId], [t].[Name], [t].[OptionalReferenceBranchId], [t].[RequiredReferenceBranchId], [t].[RequiredReferenceBranch_Name], [t].[RequiredReferenceBranch_RequiredReferenceLeaf_Name]
FROM [RootEntities] AS [r]
INNER JOIN [TrunkEntities] AS [t] ON [r].[Id] = [t].[Id]
""");
    }

    public override async Task Select_subquery_root_set_trunk_FirstOrDefault_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_subquery_root_set_trunk_FirstOrDefault_collection(async, queryTrackingBehavior);

        AssertSql();
    }

    public override async Task Select_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(async, queryTrackingBehavior);

        AssertSql();
    }

    public override async Task Select_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(async, queryTrackingBehavior);

        AssertSql();
    }

    #endregion Subquery

    #region SelectMany

    public override async Task SelectMany_trunk_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_trunk_collection(async, queryTrackingBehavior);

        AssertSql();
    }

    public override async Task SelectMany_required_trunk_reference_branch_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_required_trunk_reference_branch_collection(async, queryTrackingBehavior);

        AssertSql();
    }

    public override async Task SelectMany_optional_trunk_reference_branch_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_optional_trunk_reference_branch_collection(async, queryTrackingBehavior);

        AssertSql();
    }

    #endregion SelectMany

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
