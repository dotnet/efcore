// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.Navigations;

public class NavigationsProjectionSqlServerTest(NavigationsSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : NavigationsProjectionRelationalTestBase<NavigationsSqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Select_root(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_root(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelatedId], [r].[RequiredRelatedId], [r0].[Id], [r0].[CollectionRootId], [r0].[Int], [r0].[Name], [r0].[OptionalNestedId], [r0].[RequiredNestedId], [r0].[String], [n].[Id], [n0].[Id], [r1].[Id], [n1].[Id], [n2].[Id], [n3].[Id], [n3].[CollectionRelatedId], [n3].[Int], [n3].[Name], [n3].[String], [n].[CollectionRelatedId], [n].[Int], [n].[Name], [n].[String], [n0].[CollectionRelatedId], [n0].[Int], [n0].[Name], [n0].[String], [s].[Id], [s].[CollectionRootId], [s].[Int], [s].[Name], [s].[OptionalNestedId], [s].[RequiredNestedId], [s].[String], [s].[Id0], [s].[Id1], [s].[Id2], [s].[CollectionRelatedId], [s].[Int0], [s].[Name0], [s].[String0], [s].[CollectionRelatedId0], [s].[Int1], [s].[Name1], [s].[String1], [s].[CollectionRelatedId1], [s].[Int2], [s].[Name2], [s].[String2], [r1].[CollectionRootId], [r1].[Int], [r1].[Name], [r1].[OptionalNestedId], [r1].[RequiredNestedId], [r1].[String], [n7].[Id], [n7].[CollectionRelatedId], [n7].[Int], [n7].[Name], [n7].[String], [n1].[CollectionRelatedId], [n1].[Int], [n1].[Name], [n1].[String], [n2].[CollectionRelatedId], [n2].[Int], [n2].[Name], [n2].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [RelatedType] AS [r0] ON [r].[OptionalRelatedId] = [r0].[Id]
LEFT JOIN [NestedType] AS [n] ON [r0].[OptionalNestedId] = [n].[Id]
LEFT JOIN [NestedType] AS [n0] ON [r0].[RequiredNestedId] = [n0].[Id]
INNER JOIN [RelatedType] AS [r1] ON [r].[RequiredRelatedId] = [r1].[Id]
LEFT JOIN [NestedType] AS [n1] ON [r1].[OptionalNestedId] = [n1].[Id]
INNER JOIN [NestedType] AS [n2] ON [r1].[RequiredNestedId] = [n2].[Id]
LEFT JOIN [NestedType] AS [n3] ON [r0].[Id] = [n3].[CollectionRelatedId]
LEFT JOIN (
    SELECT [r2].[Id], [r2].[CollectionRootId], [r2].[Int], [r2].[Name], [r2].[OptionalNestedId], [r2].[RequiredNestedId], [r2].[String], [n4].[Id] AS [Id0], [n5].[Id] AS [Id1], [n6].[Id] AS [Id2], [n6].[CollectionRelatedId], [n6].[Int] AS [Int0], [n6].[Name] AS [Name0], [n6].[String] AS [String0], [n4].[CollectionRelatedId] AS [CollectionRelatedId0], [n4].[Int] AS [Int1], [n4].[Name] AS [Name1], [n4].[String] AS [String1], [n5].[CollectionRelatedId] AS [CollectionRelatedId1], [n5].[Int] AS [Int2], [n5].[Name] AS [Name2], [n5].[String] AS [String2]
    FROM [RelatedType] AS [r2]
    LEFT JOIN [NestedType] AS [n4] ON [r2].[OptionalNestedId] = [n4].[Id]
    INNER JOIN [NestedType] AS [n5] ON [r2].[RequiredNestedId] = [n5].[Id]
    LEFT JOIN [NestedType] AS [n6] ON [r2].[Id] = [n6].[CollectionRelatedId]
) AS [s] ON [r].[Id] = [s].[CollectionRootId]
LEFT JOIN [NestedType] AS [n7] ON [r1].[Id] = [n7].[CollectionRelatedId]
ORDER BY [r].[Id], [r0].[Id], [n].[Id], [n0].[Id], [r1].[Id], [n1].[Id], [n2].[Id], [n3].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2]
""");
    }

    #region Simple properties

    public override async Task Select_property_on_required_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_property_on_required_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r0].[String]
FROM [RootEntity] AS [r]
INNER JOIN [RelatedType] AS [r0] ON [r].[RequiredRelatedId] = [r0].[Id]
""");
    }

    public override async Task Select_property_on_optional_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_property_on_optional_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r0].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [RelatedType] AS [r0] ON [r].[OptionalRelatedId] = [r0].[Id]
""");
    }

    public override async Task Select_value_type_property_on_null_related_throws(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_value_type_property_on_null_related_throws(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r0].[Int]
FROM [RootEntity] AS [r]
LEFT JOIN [RelatedType] AS [r0] ON [r].[OptionalRelatedId] = [r0].[Id]
""");
    }

    public override async Task Select_nullable_value_type_property_on_null_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nullable_value_type_property_on_null_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r0].[Int]
FROM [RootEntity] AS [r]
LEFT JOIN [RelatedType] AS [r0] ON [r].[OptionalRelatedId] = [r0].[Id]
""");
    }

    #endregion Simple properties

    #region Non-collection

    public override async Task Select_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r0].[Id], [r0].[CollectionRootId], [r0].[Int], [r0].[Name], [r0].[OptionalNestedId], [r0].[RequiredNestedId], [r0].[String], [r].[Id], [n].[Id], [n0].[Id], [n1].[Id], [n1].[CollectionRelatedId], [n1].[Int], [n1].[Name], [n1].[String], [n].[CollectionRelatedId], [n].[Int], [n].[Name], [n].[String], [n0].[CollectionRelatedId], [n0].[Int], [n0].[Name], [n0].[String]
FROM [RootEntity] AS [r]
INNER JOIN [RelatedType] AS [r0] ON [r].[RequiredRelatedId] = [r0].[Id]
LEFT JOIN [NestedType] AS [n] ON [r0].[OptionalNestedId] = [n].[Id]
INNER JOIN [NestedType] AS [n0] ON [r0].[RequiredNestedId] = [n0].[Id]
LEFT JOIN [NestedType] AS [n1] ON [r0].[Id] = [n1].[CollectionRelatedId]
ORDER BY [r].[Id], [r0].[Id], [n].[Id], [n0].[Id]
""");
    }

    public override async Task Select_optional_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r0].[Id], [r0].[CollectionRootId], [r0].[Int], [r0].[Name], [r0].[OptionalNestedId], [r0].[RequiredNestedId], [r0].[String], [r].[Id], [n].[Id], [n0].[Id], [n1].[Id], [n1].[CollectionRelatedId], [n1].[Int], [n1].[Name], [n1].[String], [n].[CollectionRelatedId], [n].[Int], [n].[Name], [n].[String], [n0].[CollectionRelatedId], [n0].[Int], [n0].[Name], [n0].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [RelatedType] AS [r0] ON [r].[OptionalRelatedId] = [r0].[Id]
LEFT JOIN [NestedType] AS [n] ON [r0].[OptionalNestedId] = [n].[Id]
LEFT JOIN [NestedType] AS [n0] ON [r0].[RequiredNestedId] = [n0].[Id]
LEFT JOIN [NestedType] AS [n1] ON [r0].[Id] = [n1].[CollectionRelatedId]
ORDER BY [r].[Id], [r0].[Id], [n].[Id], [n0].[Id]
""");
    }

    public override async Task Select_required_nested_on_required_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_nested_on_required_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT [n].[Id], [n].[CollectionRelatedId], [n].[Int], [n].[Name], [n].[String]
FROM [RootEntity] AS [r]
INNER JOIN [RelatedType] AS [r0] ON [r].[RequiredRelatedId] = [r0].[Id]
INNER JOIN [NestedType] AS [n] ON [r0].[RequiredNestedId] = [n].[Id]
""");
    }

    public override async Task Select_optional_nested_on_required_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_nested_on_required_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT [n].[Id], [n].[CollectionRelatedId], [n].[Int], [n].[Name], [n].[String]
FROM [RootEntity] AS [r]
INNER JOIN [RelatedType] AS [r0] ON [r].[RequiredRelatedId] = [r0].[Id]
LEFT JOIN [NestedType] AS [n] ON [r0].[OptionalNestedId] = [n].[Id]
""");
    }

    public override async Task Select_required_nested_on_optional_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_nested_on_optional_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT [n].[Id], [n].[CollectionRelatedId], [n].[Int], [n].[Name], [n].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [RelatedType] AS [r0] ON [r].[OptionalRelatedId] = [r0].[Id]
LEFT JOIN [NestedType] AS [n] ON [r0].[RequiredNestedId] = [n].[Id]
""");
    }

    public override async Task Select_optional_nested_on_optional_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_nested_on_optional_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT [n].[Id], [n].[CollectionRelatedId], [n].[Int], [n].[Name], [n].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [RelatedType] AS [r0] ON [r].[OptionalRelatedId] = [r0].[Id]
LEFT JOIN [NestedType] AS [n] ON [r0].[OptionalNestedId] = [n].[Id]
""");
    }

    public override async Task Select_required_related_via_optional_navigation(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_related_via_optional_navigation(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r1].[Id], [r1].[CollectionRootId], [r1].[Int], [r1].[Name], [r1].[OptionalNestedId], [r1].[RequiredNestedId], [r1].[String], [r].[Id], [r0].[Id], [n].[Id], [n0].[Id], [n1].[Id], [n1].[CollectionRelatedId], [n1].[Int], [n1].[Name], [n1].[String], [n].[CollectionRelatedId], [n].[Int], [n].[Name], [n].[String], [n0].[CollectionRelatedId], [n0].[Int], [n0].[Name], [n0].[String]
FROM [RootReferencingEntity] AS [r]
LEFT JOIN [RootEntity] AS [r0] ON [r].[RootEntityId] = [r0].[Id]
LEFT JOIN [RelatedType] AS [r1] ON [r0].[RequiredRelatedId] = [r1].[Id]
LEFT JOIN [NestedType] AS [n] ON [r1].[OptionalNestedId] = [n].[Id]
LEFT JOIN [NestedType] AS [n0] ON [r1].[RequiredNestedId] = [n0].[Id]
LEFT JOIN [NestedType] AS [n1] ON [r1].[Id] = [n1].[CollectionRelatedId]
ORDER BY [r].[Id], [r0].[Id], [r1].[Id], [n].[Id], [n0].[Id]
""");
    }

    #endregion Non-collection

    #region Collection

    public override async Task Select_related_collection(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_related_collection(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [s].[Id], [s].[CollectionRootId], [s].[Int], [s].[Name], [s].[OptionalNestedId], [s].[RequiredNestedId], [s].[String], [s].[Id0], [s].[Id1], [s].[Id2], [s].[CollectionRelatedId], [s].[Int0], [s].[Name0], [s].[String0], [s].[CollectionRelatedId0], [s].[Int1], [s].[Name1], [s].[String1], [s].[CollectionRelatedId1], [s].[Int2], [s].[Name2], [s].[String2]
FROM [RootEntity] AS [r]
LEFT JOIN (
    SELECT [r0].[Id], [r0].[CollectionRootId], [r0].[Int], [r0].[Name], [r0].[OptionalNestedId], [r0].[RequiredNestedId], [r0].[String], [n].[Id] AS [Id0], [n0].[Id] AS [Id1], [n1].[Id] AS [Id2], [n1].[CollectionRelatedId], [n1].[Int] AS [Int0], [n1].[Name] AS [Name0], [n1].[String] AS [String0], [n].[CollectionRelatedId] AS [CollectionRelatedId0], [n].[Int] AS [Int1], [n].[Name] AS [Name1], [n].[String] AS [String1], [n0].[CollectionRelatedId] AS [CollectionRelatedId1], [n0].[Int] AS [Int2], [n0].[Name] AS [Name2], [n0].[String] AS [String2]
    FROM [RelatedType] AS [r0]
    LEFT JOIN [NestedType] AS [n] ON [r0].[OptionalNestedId] = [n].[Id]
    INNER JOIN [NestedType] AS [n0] ON [r0].[RequiredNestedId] = [n0].[Id]
    LEFT JOIN [NestedType] AS [n1] ON [r0].[Id] = [n1].[CollectionRelatedId]
) AS [s] ON [r].[Id] = [s].[CollectionRootId]
ORDER BY [r].[Id], [s].[Id], [s].[Id0], [s].[Id1]
""");
    }

    public override async Task Select_nested_collection_on_required_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nested_collection_on_required_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r0].[Id], [n].[Id], [n].[CollectionRelatedId], [n].[Int], [n].[Name], [n].[String]
FROM [RootEntity] AS [r]
INNER JOIN [RelatedType] AS [r0] ON [r].[RequiredRelatedId] = [r0].[Id]
LEFT JOIN [NestedType] AS [n] ON [r0].[Id] = [n].[CollectionRelatedId]
ORDER BY [r].[Id], [r0].[Id]
""");
    }

    public override async Task Select_nested_collection_on_optional_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nested_collection_on_optional_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r0].[Id], [n].[Id], [n].[CollectionRelatedId], [n].[Int], [n].[Name], [n].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [RelatedType] AS [r0] ON [r].[OptionalRelatedId] = [r0].[Id]
LEFT JOIN [NestedType] AS [n] ON [r0].[Id] = [n].[CollectionRelatedId]
ORDER BY [r].[Id], [r0].[Id]
""");
    }

    public override async Task SelectMany_related_collection(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_related_collection(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r0].[Id], [r0].[CollectionRootId], [r0].[Int], [r0].[Name], [r0].[OptionalNestedId], [r0].[RequiredNestedId], [r0].[String], [r].[Id], [n].[Id], [n0].[Id], [n1].[Id], [n1].[CollectionRelatedId], [n1].[Int], [n1].[Name], [n1].[String], [n].[CollectionRelatedId], [n].[Int], [n].[Name], [n].[String], [n0].[CollectionRelatedId], [n0].[Int], [n0].[Name], [n0].[String]
FROM [RootEntity] AS [r]
INNER JOIN [RelatedType] AS [r0] ON [r].[Id] = [r0].[CollectionRootId]
LEFT JOIN [NestedType] AS [n] ON [r0].[OptionalNestedId] = [n].[Id]
INNER JOIN [NestedType] AS [n0] ON [r0].[RequiredNestedId] = [n0].[Id]
LEFT JOIN [NestedType] AS [n1] ON [r0].[Id] = [n1].[CollectionRelatedId]
ORDER BY [r].[Id], [r0].[Id], [n].[Id], [n0].[Id]
""");
    }

    public override async Task SelectMany_nested_collection_on_required_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_nested_collection_on_required_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT [n].[Id], [n].[CollectionRelatedId], [n].[Int], [n].[Name], [n].[String]
FROM [RootEntity] AS [r]
INNER JOIN [RelatedType] AS [r0] ON [r].[RequiredRelatedId] = [r0].[Id]
INNER JOIN [NestedType] AS [n] ON [r0].[Id] = [n].[CollectionRelatedId]
""");
    }

    public override async Task SelectMany_nested_collection_on_optional_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_nested_collection_on_optional_related(queryTrackingBehavior);

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

    public override async Task Select_root_duplicated(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_root_duplicated(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelatedId], [r].[RequiredRelatedId], [r0].[Id], [r0].[CollectionRootId], [r0].[Int], [r0].[Name], [r0].[OptionalNestedId], [r0].[RequiredNestedId], [r0].[String], [n].[Id], [n0].[Id], [r1].[Id], [n1].[Id], [n2].[Id], [n3].[Id], [n3].[CollectionRelatedId], [n3].[Int], [n3].[Name], [n3].[String], [n].[CollectionRelatedId], [n].[Int], [n].[Name], [n].[String], [n0].[CollectionRelatedId], [n0].[Int], [n0].[Name], [n0].[String], [s].[Id], [s].[CollectionRootId], [s].[Int], [s].[Name], [s].[OptionalNestedId], [s].[RequiredNestedId], [s].[String], [s].[Id0], [s].[Id1], [s].[Id2], [s].[CollectionRelatedId], [s].[Int0], [s].[Name0], [s].[String0], [s].[CollectionRelatedId0], [s].[Int1], [s].[Name1], [s].[String1], [s].[CollectionRelatedId1], [s].[Int2], [s].[Name2], [s].[String2], [r1].[CollectionRootId], [r1].[Int], [r1].[Name], [r1].[OptionalNestedId], [r1].[RequiredNestedId], [r1].[String], [n7].[Id], [n7].[CollectionRelatedId], [n7].[Int], [n7].[Name], [n7].[String], [n1].[CollectionRelatedId], [n1].[Int], [n1].[Name], [n1].[String], [n2].[CollectionRelatedId], [n2].[Int], [n2].[Name], [n2].[String], [n8].[Id], [n8].[CollectionRelatedId], [n8].[Int], [n8].[Name], [n8].[String], [s0].[Id], [s0].[CollectionRootId], [s0].[Int], [s0].[Name], [s0].[OptionalNestedId], [s0].[RequiredNestedId], [s0].[String], [s0].[Id0], [s0].[Id1], [s0].[Id2], [s0].[CollectionRelatedId], [s0].[Int0], [s0].[Name0], [s0].[String0], [s0].[CollectionRelatedId0], [s0].[Int1], [s0].[Name1], [s0].[String1], [s0].[CollectionRelatedId1], [s0].[Int2], [s0].[Name2], [s0].[String2], [n12].[Id], [n12].[CollectionRelatedId], [n12].[Int], [n12].[Name], [n12].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [RelatedType] AS [r0] ON [r].[OptionalRelatedId] = [r0].[Id]
LEFT JOIN [NestedType] AS [n] ON [r0].[OptionalNestedId] = [n].[Id]
LEFT JOIN [NestedType] AS [n0] ON [r0].[RequiredNestedId] = [n0].[Id]
INNER JOIN [RelatedType] AS [r1] ON [r].[RequiredRelatedId] = [r1].[Id]
LEFT JOIN [NestedType] AS [n1] ON [r1].[OptionalNestedId] = [n1].[Id]
INNER JOIN [NestedType] AS [n2] ON [r1].[RequiredNestedId] = [n2].[Id]
LEFT JOIN [NestedType] AS [n3] ON [r0].[Id] = [n3].[CollectionRelatedId]
LEFT JOIN (
    SELECT [r2].[Id], [r2].[CollectionRootId], [r2].[Int], [r2].[Name], [r2].[OptionalNestedId], [r2].[RequiredNestedId], [r2].[String], [n4].[Id] AS [Id0], [n5].[Id] AS [Id1], [n6].[Id] AS [Id2], [n6].[CollectionRelatedId], [n6].[Int] AS [Int0], [n6].[Name] AS [Name0], [n6].[String] AS [String0], [n4].[CollectionRelatedId] AS [CollectionRelatedId0], [n4].[Int] AS [Int1], [n4].[Name] AS [Name1], [n4].[String] AS [String1], [n5].[CollectionRelatedId] AS [CollectionRelatedId1], [n5].[Int] AS [Int2], [n5].[Name] AS [Name2], [n5].[String] AS [String2]
    FROM [RelatedType] AS [r2]
    LEFT JOIN [NestedType] AS [n4] ON [r2].[OptionalNestedId] = [n4].[Id]
    INNER JOIN [NestedType] AS [n5] ON [r2].[RequiredNestedId] = [n5].[Id]
    LEFT JOIN [NestedType] AS [n6] ON [r2].[Id] = [n6].[CollectionRelatedId]
) AS [s] ON [r].[Id] = [s].[CollectionRootId]
LEFT JOIN [NestedType] AS [n7] ON [r1].[Id] = [n7].[CollectionRelatedId]
LEFT JOIN [NestedType] AS [n8] ON [r0].[Id] = [n8].[CollectionRelatedId]
LEFT JOIN (
    SELECT [r3].[Id], [r3].[CollectionRootId], [r3].[Int], [r3].[Name], [r3].[OptionalNestedId], [r3].[RequiredNestedId], [r3].[String], [n9].[Id] AS [Id0], [n10].[Id] AS [Id1], [n11].[Id] AS [Id2], [n11].[CollectionRelatedId], [n11].[Int] AS [Int0], [n11].[Name] AS [Name0], [n11].[String] AS [String0], [n9].[CollectionRelatedId] AS [CollectionRelatedId0], [n9].[Int] AS [Int1], [n9].[Name] AS [Name1], [n9].[String] AS [String1], [n10].[CollectionRelatedId] AS [CollectionRelatedId1], [n10].[Int] AS [Int2], [n10].[Name] AS [Name2], [n10].[String] AS [String2]
    FROM [RelatedType] AS [r3]
    LEFT JOIN [NestedType] AS [n9] ON [r3].[OptionalNestedId] = [n9].[Id]
    INNER JOIN [NestedType] AS [n10] ON [r3].[RequiredNestedId] = [n10].[Id]
    LEFT JOIN [NestedType] AS [n11] ON [r3].[Id] = [n11].[CollectionRelatedId]
) AS [s0] ON [r].[Id] = [s0].[CollectionRootId]
LEFT JOIN [NestedType] AS [n12] ON [r1].[Id] = [n12].[CollectionRelatedId]
ORDER BY [r].[Id], [r0].[Id], [n].[Id], [n0].[Id], [r1].[Id], [n1].[Id], [n2].[Id], [n3].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2], [n7].[Id], [n8].[Id], [s0].[Id], [s0].[Id0], [s0].[Id1], [s0].[Id2]
""");
    }

    #endregion Multiple

    #region Subquery

    public override async Task Select_subquery_required_related_FirstOrDefault(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_subquery_required_related_FirstOrDefault(queryTrackingBehavior);

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

    public override async Task Select_subquery_optional_related_FirstOrDefault(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_subquery_optional_related_FirstOrDefault(queryTrackingBehavior);

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
