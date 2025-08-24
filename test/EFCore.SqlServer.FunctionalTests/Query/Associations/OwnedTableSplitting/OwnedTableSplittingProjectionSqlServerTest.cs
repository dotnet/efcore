// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedTableSplitting;

public class OwnedTableSplittingProjectionSqlServerTest(OwnedTableSplittingSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : OwnedTableSplittingProjectionRelationalTestBase<OwnedTableSplittingSqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Select_root(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_root(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated_Id], [r].[OptionalRelated_Int], [r].[OptionalRelated_Name], [r].[OptionalRelated_String], [o].[RelatedTypeRootEntityId], [o].[Id], [o].[Int], [o].[Name], [o].[String], [r].[OptionalRelated_OptionalNested_Id], [r].[OptionalRelated_OptionalNested_Int], [r].[OptionalRelated_OptionalNested_Name], [r].[OptionalRelated_OptionalNested_String], [r].[OptionalRelated_RequiredNested_Id], [r].[OptionalRelated_RequiredNested_Int], [r].[OptionalRelated_RequiredNested_Name], [r].[OptionalRelated_RequiredNested_String], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Name], [s].[String], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[Id0], [s].[Int0], [s].[Name0], [s].[String0], [s].[OptionalNested_Id], [s].[OptionalNested_Int], [s].[OptionalNested_Name], [s].[OptionalNested_String], [s].[RequiredNested_Id], [s].[RequiredNested_Int], [s].[RequiredNested_Name], [s].[RequiredNested_String], [r].[RequiredRelated_Id], [r].[RequiredRelated_Int], [r].[RequiredRelated_Name], [r].[RequiredRelated_String], [r2].[RelatedTypeRootEntityId], [r2].[Id], [r2].[Int], [r2].[Name], [r2].[String], [r].[RequiredRelated_OptionalNested_Id], [r].[RequiredRelated_OptionalNested_Int], [r].[RequiredRelated_OptionalNested_Name], [r].[RequiredRelated_OptionalNested_String], [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o] ON CASE
    WHEN [r].[OptionalRelated_Id] IS NOT NULL AND [r].[OptionalRelated_Int] IS NOT NULL AND [r].[OptionalRelated_Name] IS NOT NULL AND [r].[OptionalRelated_String] IS NOT NULL THEN [r].[Id]
END = [o].[RelatedTypeRootEntityId]
LEFT JOIN (
    SELECT [r0].[RootEntityId], [r0].[Id], [r0].[Int], [r0].[Name], [r0].[String], [r1].[RelatedTypeRootEntityId], [r1].[RelatedTypeId], [r1].[Id] AS [Id0], [r1].[Int] AS [Int0], [r1].[Name] AS [Name0], [r1].[String] AS [String0], [r0].[OptionalNested_Id], [r0].[OptionalNested_Int], [r0].[OptionalNested_Name], [r0].[OptionalNested_String], [r0].[RequiredNested_Id], [r0].[RequiredNested_Int], [r0].[RequiredNested_Name], [r0].[RequiredNested_String]
    FROM [RelatedCollection] AS [r0]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r1] ON [r0].[RootEntityId] = [r1].[RelatedTypeRootEntityId] AND [r0].[Id] = [r1].[RelatedTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r2] ON [r].[Id] = [r2].[RelatedTypeRootEntityId]
ORDER BY [r].[Id], [o].[RelatedTypeRootEntityId], [o].[Id], [s].[RootEntityId], [s].[Id], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[Id0], [r2].[RelatedTypeRootEntityId]
""");
    }

    #region Simple properties

    public override async Task Select_property_on_required_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_property_on_required_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[RequiredRelated_String]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_property_on_optional_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_property_on_optional_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[OptionalRelated_String]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_value_type_property_on_null_related_throws(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_value_type_property_on_null_related_throws(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[OptionalRelated_Int]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_nullable_value_type_property_on_null_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nullable_value_type_property_on_null_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[OptionalRelated_Int]
FROM [RootEntity] AS [r]
""");
    }

    #endregion Simple properties

    #region Non-collection

    public override async Task Select_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_related(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[RequiredRelated_Id], [r].[RequiredRelated_Int], [r].[RequiredRelated_Name], [r].[RequiredRelated_String], [r0].[RelatedTypeRootEntityId], [r0].[Id], [r0].[Int], [r0].[Name], [r0].[String], [r].[RequiredRelated_OptionalNested_Id], [r].[RequiredRelated_OptionalNested_Int], [r].[RequiredRelated_OptionalNested_Name], [r].[RequiredRelated_OptionalNested_String], [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r0] ON [r].[Id] = [r0].[RelatedTypeRootEntityId]
ORDER BY [r].[Id], [r0].[RelatedTypeRootEntityId]
""");
        }
    }

    public override async Task Select_optional_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_related(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[OptionalRelated_Id], [r].[OptionalRelated_Int], [r].[OptionalRelated_Name], [r].[OptionalRelated_String], [o].[RelatedTypeRootEntityId], [o].[Id], [o].[Int], [o].[Name], [o].[String], [r].[OptionalRelated_OptionalNested_Id], [r].[OptionalRelated_OptionalNested_Int], [r].[OptionalRelated_OptionalNested_Name], [r].[OptionalRelated_OptionalNested_String], [r].[OptionalRelated_RequiredNested_Id], [r].[OptionalRelated_RequiredNested_Int], [r].[OptionalRelated_RequiredNested_Name], [r].[OptionalRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o] ON CASE
    WHEN [r].[OptionalRelated_Id] IS NOT NULL AND [r].[OptionalRelated_Int] IS NOT NULL AND [r].[OptionalRelated_Name] IS NOT NULL AND [r].[OptionalRelated_String] IS NOT NULL THEN [r].[Id]
END = [o].[RelatedTypeRootEntityId]
ORDER BY [r].[Id], [o].[RelatedTypeRootEntityId]
""");
        }
    }

    public override async Task Select_required_nested_on_required_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_nested_on_required_related(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
""");
        }
    }

    public override async Task Select_optional_nested_on_required_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_nested_on_required_related(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[RequiredRelated_OptionalNested_Id], [r].[RequiredRelated_OptionalNested_Int], [r].[RequiredRelated_OptionalNested_Name], [r].[RequiredRelated_OptionalNested_String]
FROM [RootEntity] AS [r]
""");
        }
    }

    public override async Task Select_required_nested_on_optional_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_nested_on_optional_related(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[OptionalRelated_RequiredNested_Id], [r].[OptionalRelated_RequiredNested_Int], [r].[OptionalRelated_RequiredNested_Name], [r].[OptionalRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
""");
        }
    }

    public override async Task Select_optional_nested_on_optional_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_nested_on_optional_related(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[OptionalRelated_OptionalNested_Id], [r].[OptionalRelated_OptionalNested_Int], [r].[OptionalRelated_OptionalNested_Name], [r].[OptionalRelated_OptionalNested_String]
FROM [RootEntity] AS [r]
""");
        }
    }

    public override async Task Select_required_related_via_optional_navigation(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_related_via_optional_navigation(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT [r0].[Id], [r0].[RequiredRelated_Id], [r0].[RequiredRelated_Int], [r0].[RequiredRelated_Name], [r0].[RequiredRelated_String], [r].[Id], [r1].[RelatedTypeRootEntityId], [r1].[Id], [r1].[Int], [r1].[Name], [r1].[String], [r0].[RequiredRelated_OptionalNested_Id], [r0].[RequiredRelated_OptionalNested_Int], [r0].[RequiredRelated_OptionalNested_Name], [r0].[RequiredRelated_OptionalNested_String], [r0].[RequiredRelated_RequiredNested_Id], [r0].[RequiredRelated_RequiredNested_Int], [r0].[RequiredRelated_RequiredNested_Name], [r0].[RequiredRelated_RequiredNested_String]
FROM [RootReferencingEntity] AS [r]
LEFT JOIN [RootEntity] AS [r0] ON [r].[RootEntityId] = [r0].[Id]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r1] ON [r0].[Id] = [r1].[RelatedTypeRootEntityId]
ORDER BY [r].[Id], [r0].[Id], [r1].[RelatedTypeRootEntityId]
""");
        }
    }

    #endregion Non-collection

    #region Collection

    public override async Task Select_related_collection(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_related_collection(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT [r].[Id], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Name], [s].[String], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[Id0], [s].[Int0], [s].[Name0], [s].[String0], [s].[OptionalNested_Id], [s].[OptionalNested_Int], [s].[OptionalNested_Name], [s].[OptionalNested_String], [s].[RequiredNested_Id], [s].[RequiredNested_Int], [s].[RequiredNested_Name], [s].[RequiredNested_String]
FROM [RootEntity] AS [r]
LEFT JOIN (
    SELECT [r0].[RootEntityId], [r0].[Id], [r0].[Int], [r0].[Name], [r0].[String], [r1].[RelatedTypeRootEntityId], [r1].[RelatedTypeId], [r1].[Id] AS [Id0], [r1].[Int] AS [Int0], [r1].[Name] AS [Name0], [r1].[String] AS [String0], [r0].[OptionalNested_Id], [r0].[OptionalNested_Int], [r0].[OptionalNested_Name], [r0].[OptionalNested_String], [r0].[RequiredNested_Id], [r0].[RequiredNested_Int], [r0].[RequiredNested_Name], [r0].[RequiredNested_String]
    FROM [RelatedCollection] AS [r0]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r1] ON [r0].[RootEntityId] = [r1].[RelatedTypeRootEntityId] AND [r0].[Id] = [r1].[RelatedTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
ORDER BY [r].[Id], [s].[RootEntityId], [s].[Id], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId]
""");
        }
    }

    public override async Task Select_nested_collection_on_required_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nested_collection_on_required_related(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT [r].[Id], [r0].[RelatedTypeRootEntityId], [r0].[Id], [r0].[Int], [r0].[Name], [r0].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r0] ON [r].[Id] = [r0].[RelatedTypeRootEntityId]
ORDER BY [r].[Id], [r0].[RelatedTypeRootEntityId]
""");
        }
    }

    public override async Task Select_nested_collection_on_optional_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nested_collection_on_optional_related(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT [r].[Id], [o].[RelatedTypeRootEntityId], [o].[Id], [o].[Int], [o].[Name], [o].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o] ON CASE
    WHEN [r].[OptionalRelated_Id] IS NOT NULL AND [r].[OptionalRelated_Int] IS NOT NULL AND [r].[OptionalRelated_Name] IS NOT NULL AND [r].[OptionalRelated_String] IS NOT NULL THEN [r].[Id]
END = [o].[RelatedTypeRootEntityId]
ORDER BY [r].[Id], [o].[RelatedTypeRootEntityId]
""");
        }
    }

    public override async Task SelectMany_related_collection(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_related_collection(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT [r0].[RootEntityId], [r0].[Id], [r0].[Int], [r0].[Name], [r0].[String], [r].[Id], [r1].[RelatedTypeRootEntityId], [r1].[RelatedTypeId], [r1].[Id], [r1].[Int], [r1].[Name], [r1].[String], [r0].[OptionalNested_Id], [r0].[OptionalNested_Int], [r0].[OptionalNested_Name], [r0].[OptionalNested_String], [r0].[RequiredNested_Id], [r0].[RequiredNested_Int], [r0].[RequiredNested_Name], [r0].[RequiredNested_String]
FROM [RootEntity] AS [r]
INNER JOIN [RelatedCollection] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
LEFT JOIN [RelatedCollection_NestedCollection] AS [r1] ON [r0].[RootEntityId] = [r1].[RelatedTypeRootEntityId] AND [r0].[Id] = [r1].[RelatedTypeId]
ORDER BY [r].[Id], [r0].[RootEntityId], [r0].[Id], [r1].[RelatedTypeRootEntityId], [r1].[RelatedTypeId]
""");
        }
    }

    public override async Task SelectMany_nested_collection_on_required_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_nested_collection_on_required_related(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT [r0].[RelatedTypeRootEntityId], [r0].[Id], [r0].[Int], [r0].[Name], [r0].[String]
FROM [RootEntity] AS [r]
INNER JOIN [RequiredRelated_NestedCollection] AS [r0] ON [r].[Id] = [r0].[RelatedTypeRootEntityId]
""");
        }
    }

    public override async Task SelectMany_nested_collection_on_optional_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_nested_collection_on_optional_related(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT [o].[RelatedTypeRootEntityId], [o].[Id], [o].[Int], [o].[Name], [o].[String]
FROM [RootEntity] AS [r]
INNER JOIN [OptionalRelated_NestedCollection] AS [o] ON CASE
    WHEN [r].[OptionalRelated_Id] IS NOT NULL AND [r].[OptionalRelated_Int] IS NOT NULL AND [r].[OptionalRelated_Name] IS NOT NULL AND [r].[OptionalRelated_String] IS NOT NULL THEN [r].[Id]
END = [o].[RelatedTypeRootEntityId]
""");
        }
    }

    #endregion Collection

    #region Multiple

    public override async Task Select_root_duplicated(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_root_duplicated(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated_Id], [r].[OptionalRelated_Int], [r].[OptionalRelated_Name], [r].[OptionalRelated_String], [o].[RelatedTypeRootEntityId], [o].[Id], [o].[Int], [o].[Name], [o].[String], [r].[OptionalRelated_OptionalNested_Id], [r].[OptionalRelated_OptionalNested_Int], [r].[OptionalRelated_OptionalNested_Name], [r].[OptionalRelated_OptionalNested_String], [r].[OptionalRelated_RequiredNested_Id], [r].[OptionalRelated_RequiredNested_Int], [r].[OptionalRelated_RequiredNested_Name], [r].[OptionalRelated_RequiredNested_String], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Name], [s].[String], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[Id0], [s].[Int0], [s].[Name0], [s].[String0], [s].[OptionalNested_Id], [s].[OptionalNested_Int], [s].[OptionalNested_Name], [s].[OptionalNested_String], [s].[RequiredNested_Id], [s].[RequiredNested_Int], [s].[RequiredNested_Name], [s].[RequiredNested_String], [r].[RequiredRelated_Id], [r].[RequiredRelated_Int], [r].[RequiredRelated_Name], [r].[RequiredRelated_String], [r2].[RelatedTypeRootEntityId], [r2].[Id], [r2].[Int], [r2].[Name], [r2].[String], [r].[RequiredRelated_OptionalNested_Id], [r].[RequiredRelated_OptionalNested_Int], [r].[RequiredRelated_OptionalNested_Name], [r].[RequiredRelated_OptionalNested_String], [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String], [o0].[RelatedTypeRootEntityId], [o0].[Id], [o0].[Int], [o0].[Name], [o0].[String], [s0].[RootEntityId], [s0].[Id], [s0].[Int], [s0].[Name], [s0].[String], [s0].[RelatedTypeRootEntityId], [s0].[RelatedTypeId], [s0].[Id0], [s0].[Int0], [s0].[Name0], [s0].[String0], [s0].[OptionalNested_Id], [s0].[OptionalNested_Int], [s0].[OptionalNested_Name], [s0].[OptionalNested_String], [s0].[RequiredNested_Id], [s0].[RequiredNested_Int], [s0].[RequiredNested_Name], [s0].[RequiredNested_String], [r5].[RelatedTypeRootEntityId], [r5].[Id], [r5].[Int], [r5].[Name], [r5].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o] ON CASE
    WHEN [r].[OptionalRelated_Id] IS NOT NULL AND [r].[OptionalRelated_Int] IS NOT NULL AND [r].[OptionalRelated_Name] IS NOT NULL AND [r].[OptionalRelated_String] IS NOT NULL THEN [r].[Id]
END = [o].[RelatedTypeRootEntityId]
LEFT JOIN (
    SELECT [r0].[RootEntityId], [r0].[Id], [r0].[Int], [r0].[Name], [r0].[String], [r1].[RelatedTypeRootEntityId], [r1].[RelatedTypeId], [r1].[Id] AS [Id0], [r1].[Int] AS [Int0], [r1].[Name] AS [Name0], [r1].[String] AS [String0], [r0].[OptionalNested_Id], [r0].[OptionalNested_Int], [r0].[OptionalNested_Name], [r0].[OptionalNested_String], [r0].[RequiredNested_Id], [r0].[RequiredNested_Int], [r0].[RequiredNested_Name], [r0].[RequiredNested_String]
    FROM [RelatedCollection] AS [r0]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r1] ON [r0].[RootEntityId] = [r1].[RelatedTypeRootEntityId] AND [r0].[Id] = [r1].[RelatedTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r2] ON [r].[Id] = [r2].[RelatedTypeRootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o0] ON CASE
    WHEN [r].[OptionalRelated_Id] IS NOT NULL AND [r].[OptionalRelated_Int] IS NOT NULL AND [r].[OptionalRelated_Name] IS NOT NULL AND [r].[OptionalRelated_String] IS NOT NULL THEN [r].[Id]
END = [o0].[RelatedTypeRootEntityId]
LEFT JOIN (
    SELECT [r3].[RootEntityId], [r3].[Id], [r3].[Int], [r3].[Name], [r3].[String], [r4].[RelatedTypeRootEntityId], [r4].[RelatedTypeId], [r4].[Id] AS [Id0], [r4].[Int] AS [Int0], [r4].[Name] AS [Name0], [r4].[String] AS [String0], [r3].[OptionalNested_Id], [r3].[OptionalNested_Int], [r3].[OptionalNested_Name], [r3].[OptionalNested_String], [r3].[RequiredNested_Id], [r3].[RequiredNested_Int], [r3].[RequiredNested_Name], [r3].[RequiredNested_String]
    FROM [RelatedCollection] AS [r3]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r4] ON [r3].[RootEntityId] = [r4].[RelatedTypeRootEntityId] AND [r3].[Id] = [r4].[RelatedTypeId]
) AS [s0] ON [r].[Id] = [s0].[RootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r5] ON [r].[Id] = [r5].[RelatedTypeRootEntityId]
ORDER BY [r].[Id], [o].[RelatedTypeRootEntityId], [o].[Id], [s].[RootEntityId], [s].[Id], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[Id0], [r2].[RelatedTypeRootEntityId], [r2].[Id], [o0].[RelatedTypeRootEntityId], [o0].[Id], [s0].[RootEntityId], [s0].[Id], [s0].[RelatedTypeRootEntityId], [s0].[RelatedTypeId], [s0].[Id0], [r5].[RelatedTypeRootEntityId]
""");
    }

    #endregion Multiple

    #region Subquery

    public override async Task Select_subquery_required_related_FirstOrDefault(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_subquery_required_related_FirstOrDefault(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT [r1].[Id], [r1].[RequiredRelated_RequiredNested_Id], [r1].[RequiredRelated_RequiredNested_Int], [r1].[RequiredRelated_RequiredNested_Name], [r1].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
OUTER APPLY (
    SELECT TOP(1) [r0].[Id], [r0].[RequiredRelated_RequiredNested_Id], [r0].[RequiredRelated_RequiredNested_Int], [r0].[RequiredRelated_RequiredNested_Name], [r0].[RequiredRelated_RequiredNested_String]
    FROM [RootEntity] AS [r0]
    ORDER BY [r0].[Id]
) AS [r1]
""");
        }
    }

    public override async Task Select_subquery_optional_related_FirstOrDefault(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_subquery_optional_related_FirstOrDefault(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT [r1].[Id], [r1].[OptionalRelated_RequiredNested_Id], [r1].[OptionalRelated_RequiredNested_Int], [r1].[OptionalRelated_RequiredNested_Name], [r1].[OptionalRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
OUTER APPLY (
    SELECT TOP(1) [r0].[Id], [r0].[OptionalRelated_RequiredNested_Id], [r0].[OptionalRelated_RequiredNested_Int], [r0].[OptionalRelated_RequiredNested_Name], [r0].[OptionalRelated_RequiredNested_String]
    FROM [RootEntity] AS [r0]
    ORDER BY [r0].[Id]
) AS [r1]
""");
        }
    }

    #endregion Subquery

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
