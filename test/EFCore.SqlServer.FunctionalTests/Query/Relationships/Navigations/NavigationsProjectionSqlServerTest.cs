// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Navigations;

public class NavigationsProjectionSqlServerTest(NavigationsSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : NavigationsProjectionRelationalTestBase<NavigationsSqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Select_root(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_root(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelatedId], [r].[RequiredRelatedId]
FROM [RootEntity] AS [r]
""");
    }

    #region Simple properties

    public override async Task Select_related_property(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_related_property(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [r0].[String]
FROM [RootEntity] AS [r]
INNER JOIN [RelatedType] AS [r0] ON [r].[RequiredRelatedId] = [r0].[Id]
""");
    }

    public override async Task Select_optional_related_property(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_related_property(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [r0].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [RelatedType] AS [r0] ON [r].[OptionalRelatedId] = [r0].[Id]
""");
    }

    public override async Task Select_optional_related_property_value_type(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_related_property_value_type(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [r0].[Int]
FROM [RootEntity] AS [r]
LEFT JOIN [RelatedType] AS [r0] ON [r].[OptionalRelatedId] = [r0].[Id]
""");
    }

    #endregion Simple properties

    #region Non-collection

    public override async Task Select_related(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_related(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [r0].[Id], [r0].[CollectionRootId], [r0].[Int], [r0].[Name], [r0].[OptionalNestedId], [r0].[RequiredNestedId], [r0].[String]
FROM [RootEntity] AS [r]
INNER JOIN [RelatedType] AS [r0] ON [r].[RequiredRelatedId] = [r0].[Id]
""");
    }

    public override async Task Select_optional_related(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_related(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [r0].[Id], [r0].[CollectionRootId], [r0].[Int], [r0].[Name], [r0].[OptionalNestedId], [r0].[RequiredNestedId], [r0].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [RelatedType] AS [r0] ON [r].[OptionalRelatedId] = [r0].[Id]
""");
    }

    public override async Task Select_required_related_required_nested(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_related_required_nested(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [n].[Id], [n].[CollectionRelatedId], [n].[Int], [n].[Name], [n].[String]
FROM [RootEntity] AS [r]
INNER JOIN [RelatedType] AS [r0] ON [r].[RequiredRelatedId] = [r0].[Id]
INNER JOIN [NestedType] AS [n] ON [r0].[RequiredNestedId] = [n].[Id]
""");
    }

    public override async Task Select_required_related_optional_nested(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_related_optional_nested(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [n].[Id], [n].[CollectionRelatedId], [n].[Int], [n].[Name], [n].[String]
FROM [RootEntity] AS [r]
INNER JOIN [RelatedType] AS [r0] ON [r].[RequiredRelatedId] = [r0].[Id]
LEFT JOIN [NestedType] AS [n] ON [r0].[OptionalNestedId] = [n].[Id]
""");
    }

    public override async Task Select_optional_related_required_nested(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_related_required_nested(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [n].[Id], [n].[CollectionRelatedId], [n].[Int], [n].[Name], [n].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [RelatedType] AS [r0] ON [r].[OptionalRelatedId] = [r0].[Id]
LEFT JOIN [NestedType] AS [n] ON [r0].[RequiredNestedId] = [n].[Id]
""");
    }

    public override async Task Select_optional_related_optional_nested(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_related_optional_nested(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [n].[Id], [n].[CollectionRelatedId], [n].[Int], [n].[Name], [n].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [RelatedType] AS [r0] ON [r].[OptionalRelatedId] = [r0].[Id]
LEFT JOIN [NestedType] AS [n] ON [r0].[OptionalNestedId] = [n].[Id]
""");
    }

    #endregion Non-collection

    #region Collection

    public override async Task Select_related_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_related_collection(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r0].[Id], [r0].[CollectionRootId], [r0].[Int], [r0].[Name], [r0].[OptionalNestedId], [r0].[RequiredNestedId], [r0].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [RelatedType] AS [r0] ON [r].[Id] = [r0].[CollectionRootId]
ORDER BY [r].[Id]
""");
    }

    public override async Task Select_required_related_nested_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_related_nested_collection(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r0].[Id], [n].[Id], [n].[CollectionRelatedId], [n].[Int], [n].[Name], [n].[String]
FROM [RootEntity] AS [r]
INNER JOIN [RelatedType] AS [r0] ON [r].[RequiredRelatedId] = [r0].[Id]
LEFT JOIN [NestedType] AS [n] ON [r0].[Id] = [n].[CollectionRelatedId]
ORDER BY [r].[Id], [r0].[Id]
""");
    }

    public override async Task Select_optional_related_nested_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_related_nested_collection(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r0].[Id], [n].[Id], [n].[CollectionRelatedId], [n].[Int], [n].[Name], [n].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [RelatedType] AS [r0] ON [r].[OptionalRelatedId] = [r0].[Id]
LEFT JOIN [NestedType] AS [n] ON [r0].[Id] = [n].[CollectionRelatedId]
ORDER BY [r].[Id], [r0].[Id]
""");
    }

    public override async Task SelectMany_related_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_related_collection(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [r0].[Id], [r0].[CollectionRootId], [r0].[Int], [r0].[Name], [r0].[OptionalNestedId], [r0].[RequiredNestedId], [r0].[String]
FROM [RootEntity] AS [r]
INNER JOIN [RelatedType] AS [r0] ON [r].[Id] = [r0].[CollectionRootId]
""");
    }

    public override async Task SelectMany_required_related_nested_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_required_related_nested_collection(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [n].[Id], [n].[CollectionRelatedId], [n].[Int], [n].[Name], [n].[String]
FROM [RootEntity] AS [r]
INNER JOIN [RelatedType] AS [r0] ON [r].[RequiredRelatedId] = [r0].[Id]
INNER JOIN [NestedType] AS [n] ON [r0].[Id] = [n].[CollectionRelatedId]
""");
    }

    public override async Task SelectMany_optional_related_nested_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_optional_related_nested_collection(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [n].[Id], [n].[CollectionRelatedId], [n].[Int], [n].[Name], [n].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [RelatedType] AS [r0] ON [r].[OptionalRelatedId] = [r0].[Id]
INNER JOIN [NestedType] AS [n] ON [r0].[Id] = [n].[CollectionRelatedId]
""");
    }

    #endregion Collection

    #region Multiple

    public override async Task Select_root_duplicated(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_root_duplicated(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelatedId], [r].[RequiredRelatedId]
FROM [RootEntity] AS [r]
""");
    }

    #endregion Multiple

    #region Subquery

    public override async Task Select_subquery_required_related_FirstOrDefault(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_subquery_required_related_FirstOrDefault(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [s].[Id], [s].[CollectionRelatedId], [s].[Int], [s].[Name], [s].[String]
FROM [RootEntity] AS [r]
OUTER APPLY (
    SELECT TOP(1) [n].[Id], [n].[CollectionRelatedId], [n].[Int], [n].[Name], [n].[String]
    FROM [RootEntity] AS [r0]
    INNER JOIN [RelatedType] AS [r1] ON [r0].[RequiredRelatedId] = [r1].[Id]
    INNER JOIN [NestedType] AS [n] ON [r1].[RequiredNestedId] = [n].[Id]
    ORDER BY [r0].[Id]
) AS [s]
""");
    }

    public override async Task Select_subquery_optional_related_FirstOrDefault(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_subquery_optional_related_FirstOrDefault(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [s].[Id], [s].[CollectionRelatedId], [s].[Int], [s].[Name], [s].[String]
FROM [RootEntity] AS [r]
OUTER APPLY (
    SELECT TOP(1) [n].[Id], [n].[CollectionRelatedId], [n].[Int], [n].[Name], [n].[String]
    FROM [RootEntity] AS [r0]
    LEFT JOIN [RelatedType] AS [r1] ON [r0].[OptionalRelatedId] = [r1].[Id]
    LEFT JOIN [NestedType] AS [n] ON [r1].[RequiredNestedId] = [n].[Id]
    ORDER BY [r0].[Id]
) AS [s]
""");
    }

    #endregion Subquery

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
