// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedNavigations;

public class OwnedNavigationsProjectionSqlServerTest(OwnedNavigationsSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : OwnedNavigationsProjectionRelationalTestBase<OwnedNavigationsSqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Select_root(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_root(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [o].[RootEntityId], [o].[Id], [o].[Int], [o].[Name], [o].[String], [o0].[RelatedTypeRootEntityId], [o1].[RelatedTypeRootEntityId], [r0].[RootEntityId], [r1].[RelatedTypeRootEntityId], [r2].[RelatedTypeRootEntityId], [o2].[RelatedTypeRootEntityId], [o2].[Id], [o2].[Int], [o2].[Name], [o2].[String], [o0].[Id], [o0].[Int], [o0].[Name], [o0].[String], [o1].[Id], [o1].[Int], [o1].[Name], [o1].[String], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Name], [s].[String], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[RelatedTypeRootEntityId0], [s].[RelatedTypeId0], [s].[RelatedTypeRootEntityId1], [s].[RelatedTypeId1], [s].[Id0], [s].[Int0], [s].[Name0], [s].[String0], [s].[Id1], [s].[Int1], [s].[Name1], [s].[String1], [s].[Id2], [s].[Int2], [s].[Name2], [s].[String2], [r0].[Id], [r0].[Int], [r0].[Name], [r0].[String], [r7].[RelatedTypeRootEntityId], [r7].[Id], [r7].[Int], [r7].[Name], [r7].[String], [r1].[Id], [r1].[Int], [r1].[Name], [r1].[String], [r2].[Id], [r2].[Int], [r2].[Name], [r2].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
LEFT JOIN [OptionalRelated_OptionalNested] AS [o0] ON [o].[RootEntityId] = [o0].[RelatedTypeRootEntityId]
LEFT JOIN [OptionalRelated_RequiredNested] AS [o1] ON [o].[RootEntityId] = [o1].[RelatedTypeRootEntityId]
LEFT JOIN [RequiredRelated] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
LEFT JOIN [RequiredRelated_OptionalNested] AS [r1] ON [r0].[RootEntityId] = [r1].[RelatedTypeRootEntityId]
LEFT JOIN [RequiredRelated_RequiredNested] AS [r2] ON [r0].[RootEntityId] = [r2].[RelatedTypeRootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o2] ON [o].[RootEntityId] = [o2].[RelatedTypeRootEntityId]
LEFT JOIN (
    SELECT [r3].[RootEntityId], [r3].[Id], [r3].[Int], [r3].[Name], [r3].[String], [r4].[RelatedTypeRootEntityId], [r4].[RelatedTypeId], [r5].[RelatedTypeRootEntityId] AS [RelatedTypeRootEntityId0], [r5].[RelatedTypeId] AS [RelatedTypeId0], [r6].[RelatedTypeRootEntityId] AS [RelatedTypeRootEntityId1], [r6].[RelatedTypeId] AS [RelatedTypeId1], [r6].[Id] AS [Id0], [r6].[Int] AS [Int0], [r6].[Name] AS [Name0], [r6].[String] AS [String0], [r4].[Id] AS [Id1], [r4].[Int] AS [Int1], [r4].[Name] AS [Name1], [r4].[String] AS [String1], [r5].[Id] AS [Id2], [r5].[Int] AS [Int2], [r5].[Name] AS [Name2], [r5].[String] AS [String2]
    FROM [RelatedCollection] AS [r3]
    LEFT JOIN [RelatedCollection_OptionalNested] AS [r4] ON [r3].[RootEntityId] = [r4].[RelatedTypeRootEntityId] AND [r3].[Id] = [r4].[RelatedTypeId]
    LEFT JOIN [RelatedCollection_RequiredNested] AS [r5] ON [r3].[RootEntityId] = [r5].[RelatedTypeRootEntityId] AND [r3].[Id] = [r5].[RelatedTypeId]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r6] ON [r3].[RootEntityId] = [r6].[RelatedTypeRootEntityId] AND [r3].[Id] = [r6].[RelatedTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r7] ON [r0].[RootEntityId] = [r7].[RelatedTypeRootEntityId]
ORDER BY [r].[Id], [o].[RootEntityId], [o0].[RelatedTypeRootEntityId], [o1].[RelatedTypeRootEntityId], [r0].[RootEntityId], [r1].[RelatedTypeRootEntityId], [r2].[RelatedTypeRootEntityId], [o2].[RelatedTypeRootEntityId], [o2].[Id], [s].[RootEntityId], [s].[Id], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[RelatedTypeRootEntityId0], [s].[RelatedTypeId0], [s].[RelatedTypeRootEntityId1], [s].[RelatedTypeId1], [s].[Id0], [r7].[RelatedTypeRootEntityId]
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
LEFT JOIN [RequiredRelated] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
""");
    }

    public override async Task Select_property_on_optional_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_property_on_optional_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT [o].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
""");
    }

    public override async Task Select_value_type_property_on_null_related_throws(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_value_type_property_on_null_related_throws(queryTrackingBehavior);

        AssertSql(
            """
SELECT [o].[Int]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
""");
    }

    public override async Task Select_nullable_value_type_property_on_null_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nullable_value_type_property_on_null_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT [o].[Int]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
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
SELECT [r0].[RootEntityId], [r0].[Id], [r0].[Int], [r0].[Name], [r0].[String], [r].[Id], [r1].[RelatedTypeRootEntityId], [r2].[RelatedTypeRootEntityId], [r3].[RelatedTypeRootEntityId], [r3].[Id], [r3].[Int], [r3].[Name], [r3].[String], [r1].[Id], [r1].[Int], [r1].[Name], [r1].[String], [r2].[Id], [r2].[Int], [r2].[Name], [r2].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [RequiredRelated] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
LEFT JOIN [RequiredRelated_OptionalNested] AS [r1] ON [r0].[RootEntityId] = [r1].[RelatedTypeRootEntityId]
LEFT JOIN [RequiredRelated_RequiredNested] AS [r2] ON [r0].[RootEntityId] = [r2].[RelatedTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r3] ON [r0].[RootEntityId] = [r3].[RelatedTypeRootEntityId]
ORDER BY [r].[Id], [r0].[RootEntityId], [r1].[RelatedTypeRootEntityId], [r2].[RelatedTypeRootEntityId], [r3].[RelatedTypeRootEntityId]
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
SELECT [o].[RootEntityId], [o].[Id], [o].[Int], [o].[Name], [o].[String], [r].[Id], [o0].[RelatedTypeRootEntityId], [o1].[RelatedTypeRootEntityId], [o2].[RelatedTypeRootEntityId], [o2].[Id], [o2].[Int], [o2].[Name], [o2].[String], [o0].[Id], [o0].[Int], [o0].[Name], [o0].[String], [o1].[Id], [o1].[Int], [o1].[Name], [o1].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
LEFT JOIN [OptionalRelated_OptionalNested] AS [o0] ON [o].[RootEntityId] = [o0].[RelatedTypeRootEntityId]
LEFT JOIN [OptionalRelated_RequiredNested] AS [o1] ON [o].[RootEntityId] = [o1].[RelatedTypeRootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o2] ON [o].[RootEntityId] = [o2].[RelatedTypeRootEntityId]
ORDER BY [r].[Id], [o].[RootEntityId], [o0].[RelatedTypeRootEntityId], [o1].[RelatedTypeRootEntityId], [o2].[RelatedTypeRootEntityId]
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
SELECT [r1].[RelatedTypeRootEntityId], [r1].[Id], [r1].[Int], [r1].[Name], [r1].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [RequiredRelated] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
LEFT JOIN [RequiredRelated_RequiredNested] AS [r1] ON [r0].[RootEntityId] = [r1].[RelatedTypeRootEntityId]
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
SELECT [r1].[RelatedTypeRootEntityId], [r1].[Id], [r1].[Int], [r1].[Name], [r1].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [RequiredRelated] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
LEFT JOIN [RequiredRelated_OptionalNested] AS [r1] ON [r0].[RootEntityId] = [r1].[RelatedTypeRootEntityId]
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
SELECT [o0].[RelatedTypeRootEntityId], [o0].[Id], [o0].[Int], [o0].[Name], [o0].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
LEFT JOIN [OptionalRelated_RequiredNested] AS [o0] ON [o].[RootEntityId] = [o0].[RelatedTypeRootEntityId]
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
SELECT [o0].[RelatedTypeRootEntityId], [o0].[Id], [o0].[Int], [o0].[Name], [o0].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
LEFT JOIN [OptionalRelated_OptionalNested] AS [o0] ON [o].[RootEntityId] = [o0].[RelatedTypeRootEntityId]
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
SELECT [r1].[RootEntityId], [r1].[Id], [r1].[Int], [r1].[Name], [r1].[String], [r].[Id], [r0].[Id], [r2].[RelatedTypeRootEntityId], [r3].[RelatedTypeRootEntityId], [r4].[RelatedTypeRootEntityId], [r4].[Id], [r4].[Int], [r4].[Name], [r4].[String], [r2].[Id], [r2].[Int], [r2].[Name], [r2].[String], [r3].[Id], [r3].[Int], [r3].[Name], [r3].[String]
FROM [RootReferencingEntity] AS [r]
LEFT JOIN [RootEntity] AS [r0] ON [r].[RootEntityId] = [r0].[Id]
LEFT JOIN [RequiredRelated] AS [r1] ON [r0].[Id] = [r1].[RootEntityId]
LEFT JOIN [RequiredRelated_OptionalNested] AS [r2] ON [r1].[RootEntityId] = [r2].[RelatedTypeRootEntityId]
LEFT JOIN [RequiredRelated_RequiredNested] AS [r3] ON [r1].[RootEntityId] = [r3].[RelatedTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r4] ON [r1].[RootEntityId] = [r4].[RelatedTypeRootEntityId]
ORDER BY [r].[Id], [r0].[Id], [r1].[RootEntityId], [r2].[RelatedTypeRootEntityId], [r3].[RelatedTypeRootEntityId], [r4].[RelatedTypeRootEntityId]
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
SELECT [r].[Id], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Name], [s].[String], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[RelatedTypeRootEntityId0], [s].[RelatedTypeId0], [s].[RelatedTypeRootEntityId1], [s].[RelatedTypeId1], [s].[Id0], [s].[Int0], [s].[Name0], [s].[String0], [s].[Id1], [s].[Int1], [s].[Name1], [s].[String1], [s].[Id2], [s].[Int2], [s].[Name2], [s].[String2]
FROM [RootEntity] AS [r]
LEFT JOIN (
    SELECT [r0].[RootEntityId], [r0].[Id], [r0].[Int], [r0].[Name], [r0].[String], [r1].[RelatedTypeRootEntityId], [r1].[RelatedTypeId], [r2].[RelatedTypeRootEntityId] AS [RelatedTypeRootEntityId0], [r2].[RelatedTypeId] AS [RelatedTypeId0], [r3].[RelatedTypeRootEntityId] AS [RelatedTypeRootEntityId1], [r3].[RelatedTypeId] AS [RelatedTypeId1], [r3].[Id] AS [Id0], [r3].[Int] AS [Int0], [r3].[Name] AS [Name0], [r3].[String] AS [String0], [r1].[Id] AS [Id1], [r1].[Int] AS [Int1], [r1].[Name] AS [Name1], [r1].[String] AS [String1], [r2].[Id] AS [Id2], [r2].[Int] AS [Int2], [r2].[Name] AS [Name2], [r2].[String] AS [String2]
    FROM [RelatedCollection] AS [r0]
    LEFT JOIN [RelatedCollection_OptionalNested] AS [r1] ON [r0].[RootEntityId] = [r1].[RelatedTypeRootEntityId] AND [r0].[Id] = [r1].[RelatedTypeId]
    LEFT JOIN [RelatedCollection_RequiredNested] AS [r2] ON [r0].[RootEntityId] = [r2].[RelatedTypeRootEntityId] AND [r0].[Id] = [r2].[RelatedTypeId]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r3] ON [r0].[RootEntityId] = [r3].[RelatedTypeRootEntityId] AND [r0].[Id] = [r3].[RelatedTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
ORDER BY [r].[Id], [s].[RootEntityId], [s].[Id], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[RelatedTypeRootEntityId0], [s].[RelatedTypeId0], [s].[RelatedTypeRootEntityId1], [s].[RelatedTypeId1]
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
SELECT [r].[Id], [r0].[RootEntityId], [r1].[RelatedTypeRootEntityId], [r1].[Id], [r1].[Int], [r1].[Name], [r1].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [RequiredRelated] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r1] ON [r0].[RootEntityId] = [r1].[RelatedTypeRootEntityId]
ORDER BY [r].[Id], [r0].[RootEntityId], [r1].[RelatedTypeRootEntityId]
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
SELECT [r].[Id], [o].[RootEntityId], [o0].[RelatedTypeRootEntityId], [o0].[Id], [o0].[Int], [o0].[Name], [o0].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o0] ON [o].[RootEntityId] = [o0].[RelatedTypeRootEntityId]
ORDER BY [r].[Id], [o].[RootEntityId], [o0].[RelatedTypeRootEntityId]
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
SELECT [r0].[RootEntityId], [r0].[Id], [r0].[Int], [r0].[Name], [r0].[String], [r].[Id], [r1].[RelatedTypeRootEntityId], [r1].[RelatedTypeId], [r2].[RelatedTypeRootEntityId], [r2].[RelatedTypeId], [r3].[RelatedTypeRootEntityId], [r3].[RelatedTypeId], [r3].[Id], [r3].[Int], [r3].[Name], [r3].[String], [r1].[Id], [r1].[Int], [r1].[Name], [r1].[String], [r2].[Id], [r2].[Int], [r2].[Name], [r2].[String]
FROM [RootEntity] AS [r]
INNER JOIN [RelatedCollection] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
LEFT JOIN [RelatedCollection_OptionalNested] AS [r1] ON [r0].[RootEntityId] = [r1].[RelatedTypeRootEntityId] AND [r0].[Id] = [r1].[RelatedTypeId]
LEFT JOIN [RelatedCollection_RequiredNested] AS [r2] ON [r0].[RootEntityId] = [r2].[RelatedTypeRootEntityId] AND [r0].[Id] = [r2].[RelatedTypeId]
LEFT JOIN [RelatedCollection_NestedCollection] AS [r3] ON [r0].[RootEntityId] = [r3].[RelatedTypeRootEntityId] AND [r0].[Id] = [r3].[RelatedTypeId]
ORDER BY [r].[Id], [r0].[RootEntityId], [r0].[Id], [r1].[RelatedTypeRootEntityId], [r1].[RelatedTypeId], [r2].[RelatedTypeRootEntityId], [r2].[RelatedTypeId], [r3].[RelatedTypeRootEntityId], [r3].[RelatedTypeId]
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
SELECT [r1].[RelatedTypeRootEntityId], [r1].[Id], [r1].[Int], [r1].[Name], [r1].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [RequiredRelated] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
INNER JOIN [RequiredRelated_NestedCollection] AS [r1] ON [r0].[RootEntityId] = [r1].[RelatedTypeRootEntityId]
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
SELECT [o0].[RelatedTypeRootEntityId], [o0].[Id], [o0].[Int], [o0].[Name], [o0].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
INNER JOIN [OptionalRelated_NestedCollection] AS [o0] ON [o].[RootEntityId] = [o0].[RelatedTypeRootEntityId]
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
SELECT [r].[Id], [r].[Name], [o].[RootEntityId], [o].[Id], [o].[Int], [o].[Name], [o].[String], [o0].[RelatedTypeRootEntityId], [o1].[RelatedTypeRootEntityId], [r0].[RootEntityId], [r1].[RelatedTypeRootEntityId], [r2].[RelatedTypeRootEntityId], [o2].[RelatedTypeRootEntityId], [o2].[Id], [o2].[Int], [o2].[Name], [o2].[String], [o0].[Id], [o0].[Int], [o0].[Name], [o0].[String], [o1].[Id], [o1].[Int], [o1].[Name], [o1].[String], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Name], [s].[String], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[RelatedTypeRootEntityId0], [s].[RelatedTypeId0], [s].[RelatedTypeRootEntityId1], [s].[RelatedTypeId1], [s].[Id0], [s].[Int0], [s].[Name0], [s].[String0], [s].[Id1], [s].[Int1], [s].[Name1], [s].[String1], [s].[Id2], [s].[Int2], [s].[Name2], [s].[String2], [r0].[Id], [r0].[Int], [r0].[Name], [r0].[String], [r7].[RelatedTypeRootEntityId], [r7].[Id], [r7].[Int], [r7].[Name], [r7].[String], [r1].[Id], [r1].[Int], [r1].[Name], [r1].[String], [r2].[Id], [r2].[Int], [r2].[Name], [r2].[String], [o3].[RelatedTypeRootEntityId], [o3].[Id], [o3].[Int], [o3].[Name], [o3].[String], [s0].[RootEntityId], [s0].[Id], [s0].[Int], [s0].[Name], [s0].[String], [s0].[RelatedTypeRootEntityId], [s0].[RelatedTypeId], [s0].[RelatedTypeRootEntityId0], [s0].[RelatedTypeId0], [s0].[RelatedTypeRootEntityId1], [s0].[RelatedTypeId1], [s0].[Id0], [s0].[Int0], [s0].[Name0], [s0].[String0], [s0].[Id1], [s0].[Int1], [s0].[Name1], [s0].[String1], [s0].[Id2], [s0].[Int2], [s0].[Name2], [s0].[String2], [r12].[RelatedTypeRootEntityId], [r12].[Id], [r12].[Int], [r12].[Name], [r12].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
LEFT JOIN [OptionalRelated_OptionalNested] AS [o0] ON [o].[RootEntityId] = [o0].[RelatedTypeRootEntityId]
LEFT JOIN [OptionalRelated_RequiredNested] AS [o1] ON [o].[RootEntityId] = [o1].[RelatedTypeRootEntityId]
LEFT JOIN [RequiredRelated] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
LEFT JOIN [RequiredRelated_OptionalNested] AS [r1] ON [r0].[RootEntityId] = [r1].[RelatedTypeRootEntityId]
LEFT JOIN [RequiredRelated_RequiredNested] AS [r2] ON [r0].[RootEntityId] = [r2].[RelatedTypeRootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o2] ON [o].[RootEntityId] = [o2].[RelatedTypeRootEntityId]
LEFT JOIN (
    SELECT [r3].[RootEntityId], [r3].[Id], [r3].[Int], [r3].[Name], [r3].[String], [r4].[RelatedTypeRootEntityId], [r4].[RelatedTypeId], [r5].[RelatedTypeRootEntityId] AS [RelatedTypeRootEntityId0], [r5].[RelatedTypeId] AS [RelatedTypeId0], [r6].[RelatedTypeRootEntityId] AS [RelatedTypeRootEntityId1], [r6].[RelatedTypeId] AS [RelatedTypeId1], [r6].[Id] AS [Id0], [r6].[Int] AS [Int0], [r6].[Name] AS [Name0], [r6].[String] AS [String0], [r4].[Id] AS [Id1], [r4].[Int] AS [Int1], [r4].[Name] AS [Name1], [r4].[String] AS [String1], [r5].[Id] AS [Id2], [r5].[Int] AS [Int2], [r5].[Name] AS [Name2], [r5].[String] AS [String2]
    FROM [RelatedCollection] AS [r3]
    LEFT JOIN [RelatedCollection_OptionalNested] AS [r4] ON [r3].[RootEntityId] = [r4].[RelatedTypeRootEntityId] AND [r3].[Id] = [r4].[RelatedTypeId]
    LEFT JOIN [RelatedCollection_RequiredNested] AS [r5] ON [r3].[RootEntityId] = [r5].[RelatedTypeRootEntityId] AND [r3].[Id] = [r5].[RelatedTypeId]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r6] ON [r3].[RootEntityId] = [r6].[RelatedTypeRootEntityId] AND [r3].[Id] = [r6].[RelatedTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r7] ON [r0].[RootEntityId] = [r7].[RelatedTypeRootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o3] ON [o].[RootEntityId] = [o3].[RelatedTypeRootEntityId]
LEFT JOIN (
    SELECT [r8].[RootEntityId], [r8].[Id], [r8].[Int], [r8].[Name], [r8].[String], [r9].[RelatedTypeRootEntityId], [r9].[RelatedTypeId], [r10].[RelatedTypeRootEntityId] AS [RelatedTypeRootEntityId0], [r10].[RelatedTypeId] AS [RelatedTypeId0], [r11].[RelatedTypeRootEntityId] AS [RelatedTypeRootEntityId1], [r11].[RelatedTypeId] AS [RelatedTypeId1], [r11].[Id] AS [Id0], [r11].[Int] AS [Int0], [r11].[Name] AS [Name0], [r11].[String] AS [String0], [r9].[Id] AS [Id1], [r9].[Int] AS [Int1], [r9].[Name] AS [Name1], [r9].[String] AS [String1], [r10].[Id] AS [Id2], [r10].[Int] AS [Int2], [r10].[Name] AS [Name2], [r10].[String] AS [String2]
    FROM [RelatedCollection] AS [r8]
    LEFT JOIN [RelatedCollection_OptionalNested] AS [r9] ON [r8].[RootEntityId] = [r9].[RelatedTypeRootEntityId] AND [r8].[Id] = [r9].[RelatedTypeId]
    LEFT JOIN [RelatedCollection_RequiredNested] AS [r10] ON [r8].[RootEntityId] = [r10].[RelatedTypeRootEntityId] AND [r8].[Id] = [r10].[RelatedTypeId]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r11] ON [r8].[RootEntityId] = [r11].[RelatedTypeRootEntityId] AND [r8].[Id] = [r11].[RelatedTypeId]
) AS [s0] ON [r].[Id] = [s0].[RootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r12] ON [r0].[RootEntityId] = [r12].[RelatedTypeRootEntityId]
ORDER BY [r].[Id], [o].[RootEntityId], [o0].[RelatedTypeRootEntityId], [o1].[RelatedTypeRootEntityId], [r0].[RootEntityId], [r1].[RelatedTypeRootEntityId], [r2].[RelatedTypeRootEntityId], [o2].[RelatedTypeRootEntityId], [o2].[Id], [s].[RootEntityId], [s].[Id], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[RelatedTypeRootEntityId0], [s].[RelatedTypeId0], [s].[RelatedTypeRootEntityId1], [s].[RelatedTypeId1], [s].[Id0], [r7].[RelatedTypeRootEntityId], [r7].[Id], [o3].[RelatedTypeRootEntityId], [o3].[Id], [s0].[RootEntityId], [s0].[Id], [s0].[RelatedTypeRootEntityId], [s0].[RelatedTypeId], [s0].[RelatedTypeRootEntityId0], [s0].[RelatedTypeId0], [s0].[RelatedTypeRootEntityId1], [s0].[RelatedTypeId1], [s0].[Id0], [r12].[RelatedTypeRootEntityId]
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
SELECT [s].[RelatedTypeRootEntityId], [s].[Id], [s].[Int], [s].[Name], [s].[String]
FROM [RootEntity] AS [r]
OUTER APPLY (
    SELECT TOP(1) [r2].[RelatedTypeRootEntityId], [r2].[Id], [r2].[Int], [r2].[Name], [r2].[String]
    FROM [RootEntity] AS [r0]
    LEFT JOIN [RequiredRelated] AS [r1] ON [r0].[Id] = [r1].[RootEntityId]
    LEFT JOIN [RequiredRelated_RequiredNested] AS [r2] ON [r1].[RootEntityId] = [r2].[RelatedTypeRootEntityId]
    ORDER BY [r0].[Id]
) AS [s]
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
SELECT [s].[RelatedTypeRootEntityId], [s].[Id], [s].[Int], [s].[Name], [s].[String]
FROM [RootEntity] AS [r]
OUTER APPLY (
    SELECT TOP(1) [o0].[RelatedTypeRootEntityId], [o0].[Id], [o0].[Int], [o0].[Name], [o0].[String]
    FROM [RootEntity] AS [r0]
    LEFT JOIN [OptionalRelated] AS [o] ON [r0].[Id] = [o].[RootEntityId]
    LEFT JOIN [OptionalRelated_RequiredNested] AS [o0] ON [o].[RootEntityId] = [o0].[RelatedTypeRootEntityId]
    ORDER BY [r0].[Id]
) AS [s]
""");
        }
    }

    #endregion Subquery

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
