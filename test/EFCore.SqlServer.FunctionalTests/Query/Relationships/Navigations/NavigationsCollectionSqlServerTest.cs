// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Navigations;

public class NavigationsCollectionSqlServerTest(NavigationsSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : NavigationsCollectionRelationalTestBase<NavigationsSqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Count()
    {
        await base.Count();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelatedId], [r].[RequiredRelatedId]
FROM [RootEntity] AS [r]
WHERE (
    SELECT COUNT(*)
    FROM [RelatedType] AS [r0]
    WHERE [r].[Id] = [r0].[CollectionRootId]) = 2
""");
    }

    public override async Task Where()
    {
        await base.Where();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelatedId], [r].[RequiredRelatedId]
FROM [RootEntity] AS [r]
WHERE (
    SELECT COUNT(*)
    FROM [RelatedType] AS [r0]
    WHERE [r].[Id] = [r0].[CollectionRootId] AND [r0].[Int] <> 8) = 2
""");
    }

    public override async Task OrderBy_ElementAt()
    {
        await base.OrderBy_ElementAt();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelatedId], [r].[RequiredRelatedId]
FROM [RootEntity] AS [r]
WHERE (
    SELECT [r0].[Int]
    FROM [RelatedType] AS [r0]
    WHERE [r].[Id] = [r0].[CollectionRootId]
    ORDER BY [r0].[Id]
    OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY) = 8
""");
    }

    #region Distinct

    public override async Task Distinct()
    {
        await base.Distinct();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelatedId], [r].[RequiredRelatedId]
FROM [RootEntity] AS [r]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT DISTINCT [r0].[Id], [r0].[CollectionRootId], [r0].[Int], [r0].[Name], [r0].[OptionalNestedId], [r0].[RequiredNestedId], [r0].[String]
        FROM [RelatedType] AS [r0]
        WHERE [r].[Id] = [r0].[CollectionRootId]
    ) AS [r1]) = 2
""");
    }

    public override async Task Distinct_projected(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Distinct_projected(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r1].[Id], [r1].[CollectionRootId], [r1].[Int], [r1].[Name], [r1].[OptionalNestedId], [r1].[RequiredNestedId], [r1].[String]
FROM [RootEntity] AS [r]
LEFT JOIN (
    SELECT DISTINCT [r0].[Id], [r0].[CollectionRootId], [r0].[Int], [r0].[Name], [r0].[OptionalNestedId], [r0].[RequiredNestedId], [r0].[String]
    FROM [RelatedType] AS [r0]
) AS [r1] ON [r].[Id] = [r1].[CollectionRootId]
ORDER BY [r].[Id]
""");
    }

    public override async Task Distinct_over_projected_nested_collection()
    {
        await base.Distinct_over_projected_nested_collection();

        AssertSql();
    }

    public override async Task Distinct_over_projected_filtered_nested_collection()
    {
        await base.Distinct_over_projected_filtered_nested_collection();

        AssertSql();
    }

    #endregion Distinct

    public override async Task Index_constant()
    {
        await base.Index_constant();

        AssertSql();
    }

    public override async Task Index_parameter()
    {
        await base.Index_parameter();

        AssertSql();
    }

    public override async Task Index_column()
    {
        await base.Index_column();

        AssertSql();
    }

    public override async Task Index_out_of_bounds()
    {
        await base.Index_out_of_bounds();

        AssertSql();
    }

    public override async Task Select_within_Select_within_Select_with_aggregates()
    {
        await base.Select_within_Select_within_Select_with_aggregates();

        AssertSql(
            """
SELECT (
    SELECT COALESCE(SUM([s].[value]), 0)
    FROM [RelatedType] AS [r0]
    OUTER APPLY (
        SELECT MAX([n].[Int]) AS [value]
        FROM [NestedType] AS [n]
        WHERE [r0].[Id] = [n].[CollectionRelatedId]
    ) AS [s]
    WHERE [r].[Id] = [r0].[CollectionRootId])
FROM [RootEntity] AS [r]
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
