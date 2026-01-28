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
SELECT [r].[Id], [r].[Name], [o].[RootEntityId], [o0].[AssociateTypeRootEntityId], [o1].[AssociateTypeRootEntityId], [r0].[RootEntityId], [r1].[AssociateTypeRootEntityId], [r2].[AssociateTypeRootEntityId], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[Id1], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[Id2], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [o].[Id], [o].[Int], [o].[Ints], [o].[Name], [o].[String], [o2].[AssociateTypeRootEntityId], [o2].[Id], [o2].[Int], [o2].[Ints], [o2].[Name], [o2].[String], [o0].[Id], [o0].[Int], [o0].[Ints], [o0].[Name], [o0].[String], [o1].[Id], [o1].[Int], [o1].[Ints], [o1].[Name], [o1].[String], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[String], [r7].[AssociateTypeRootEntityId], [r7].[Id], [r7].[Int], [r7].[Ints], [r7].[Name], [r7].[String], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[String], [r2].[Id], [r2].[Int], [r2].[Ints], [r2].[Name], [r2].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
LEFT JOIN [OptionalRelated_OptionalNested] AS [o0] ON [o].[RootEntityId] = [o0].[AssociateTypeRootEntityId]
LEFT JOIN [OptionalRelated_RequiredNested] AS [o1] ON [o].[RootEntityId] = [o1].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
LEFT JOIN [RequiredRelated_OptionalNested] AS [r1] ON [r0].[RootEntityId] = [r1].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_RequiredNested] AS [r2] ON [r0].[RootEntityId] = [r2].[AssociateTypeRootEntityId]
LEFT JOIN (
    SELECT [r3].[RootEntityId], [r3].[Id], [r3].[Int], [r3].[Ints], [r3].[Name], [r3].[String], [r4].[AssociateTypeRootEntityId], [r4].[AssociateTypeId], [r5].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId0], [r5].[AssociateTypeId] AS [AssociateTypeId0], [r6].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId1], [r6].[AssociateTypeId] AS [AssociateTypeId1], [r6].[Id] AS [Id0], [r6].[Int] AS [Int0], [r6].[Ints] AS [Ints0], [r6].[Name] AS [Name0], [r6].[String] AS [String0], [r4].[Id] AS [Id1], [r4].[Int] AS [Int1], [r4].[Ints] AS [Ints1], [r4].[Name] AS [Name1], [r4].[String] AS [String1], [r5].[Id] AS [Id2], [r5].[Int] AS [Int2], [r5].[Ints] AS [Ints2], [r5].[Name] AS [Name2], [r5].[String] AS [String2]
    FROM [RelatedCollection] AS [r3]
    LEFT JOIN [RelatedCollection_OptionalNested] AS [r4] ON [r3].[RootEntityId] = [r4].[AssociateTypeRootEntityId] AND [r3].[Id] = [r4].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_RequiredNested] AS [r5] ON [r3].[RootEntityId] = [r5].[AssociateTypeRootEntityId] AND [r3].[Id] = [r5].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r6] ON [r3].[RootEntityId] = [r6].[AssociateTypeRootEntityId] AND [r3].[Id] = [r6].[AssociateTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o2] ON [o].[RootEntityId] = [o2].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r7] ON [r0].[RootEntityId] = [r7].[AssociateTypeRootEntityId]
ORDER BY [r].[Id], [o].[RootEntityId], [o0].[AssociateTypeRootEntityId], [o1].[AssociateTypeRootEntityId], [r0].[RootEntityId], [r1].[AssociateTypeRootEntityId], [r2].[AssociateTypeRootEntityId], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id0], [o2].[AssociateTypeRootEntityId], [o2].[Id], [r7].[AssociateTypeRootEntityId]
""");
    }

    #region Scalar properties

    public override async Task Select_scalar_property_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_scalar_property_on_required_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r0].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [RequiredRelated] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
""");
    }

    public override async Task Select_property_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_property_on_optional_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT [o].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
""");
    }

    public override async Task Select_value_type_property_on_null_associate_throws(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_value_type_property_on_null_associate_throws(queryTrackingBehavior);

        AssertSql(
            """
SELECT [o].[Int]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
""");
    }

    public override async Task Select_nullable_value_type_property_on_null_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nullable_value_type_property_on_null_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT [o].[Int]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
""");
    }

    #endregion Scalar properties

    #region Structural properties

    public override async Task Select_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_associate(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT [r0].[RootEntityId], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[String], [r].[Id], [r1].[AssociateTypeRootEntityId], [r2].[AssociateTypeRootEntityId], [r3].[AssociateTypeRootEntityId], [r3].[Id], [r3].[Int], [r3].[Ints], [r3].[Name], [r3].[String], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[String], [r2].[Id], [r2].[Int], [r2].[Ints], [r2].[Name], [r2].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [RequiredRelated] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
LEFT JOIN [RequiredRelated_OptionalNested] AS [r1] ON [r0].[RootEntityId] = [r1].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_RequiredNested] AS [r2] ON [r0].[RootEntityId] = [r2].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r3] ON [r0].[RootEntityId] = [r3].[AssociateTypeRootEntityId]
ORDER BY [r].[Id], [r0].[RootEntityId], [r1].[AssociateTypeRootEntityId], [r2].[AssociateTypeRootEntityId], [r3].[AssociateTypeRootEntityId]
""");
        }
    }

    public override async Task Select_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_associate(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT [o].[RootEntityId], [o].[Id], [o].[Int], [o].[Ints], [o].[Name], [o].[String], [r].[Id], [o0].[AssociateTypeRootEntityId], [o1].[AssociateTypeRootEntityId], [o2].[AssociateTypeRootEntityId], [o2].[Id], [o2].[Int], [o2].[Ints], [o2].[Name], [o2].[String], [o0].[Id], [o0].[Int], [o0].[Ints], [o0].[Name], [o0].[String], [o1].[Id], [o1].[Int], [o1].[Ints], [o1].[Name], [o1].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
LEFT JOIN [OptionalRelated_OptionalNested] AS [o0] ON [o].[RootEntityId] = [o0].[AssociateTypeRootEntityId]
LEFT JOIN [OptionalRelated_RequiredNested] AS [o1] ON [o].[RootEntityId] = [o1].[AssociateTypeRootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o2] ON [o].[RootEntityId] = [o2].[AssociateTypeRootEntityId]
ORDER BY [r].[Id], [o].[RootEntityId], [o0].[AssociateTypeRootEntityId], [o1].[AssociateTypeRootEntityId], [o2].[AssociateTypeRootEntityId]
""");
        }
    }

    public override async Task Select_required_nested_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_nested_on_required_associate(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT [r1].[AssociateTypeRootEntityId], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [RequiredRelated] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
LEFT JOIN [RequiredRelated_RequiredNested] AS [r1] ON [r0].[RootEntityId] = [r1].[AssociateTypeRootEntityId]
""");
        }
    }

    public override async Task Select_optional_nested_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_nested_on_required_associate(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT [r1].[AssociateTypeRootEntityId], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [RequiredRelated] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
LEFT JOIN [RequiredRelated_OptionalNested] AS [r1] ON [r0].[RootEntityId] = [r1].[AssociateTypeRootEntityId]
""");
        }
    }

    public override async Task Select_required_nested_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_nested_on_optional_associate(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT [o0].[AssociateTypeRootEntityId], [o0].[Id], [o0].[Int], [o0].[Ints], [o0].[Name], [o0].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
LEFT JOIN [OptionalRelated_RequiredNested] AS [o0] ON [o].[RootEntityId] = [o0].[AssociateTypeRootEntityId]
""");
        }
    }

    public override async Task Select_optional_nested_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_nested_on_optional_associate(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT [o0].[AssociateTypeRootEntityId], [o0].[Id], [o0].[Int], [o0].[Ints], [o0].[Name], [o0].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
LEFT JOIN [OptionalRelated_OptionalNested] AS [o0] ON [o].[RootEntityId] = [o0].[AssociateTypeRootEntityId]
""");
        }
    }

    public override async Task Select_required_associate_via_optional_navigation(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_associate_via_optional_navigation(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT [r1].[RootEntityId], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[String], [r].[Id], [r0].[Id], [r2].[AssociateTypeRootEntityId], [r3].[AssociateTypeRootEntityId], [r4].[AssociateTypeRootEntityId], [r4].[Id], [r4].[Int], [r4].[Ints], [r4].[Name], [r4].[String], [r2].[Id], [r2].[Int], [r2].[Ints], [r2].[Name], [r2].[String], [r3].[Id], [r3].[Int], [r3].[Ints], [r3].[Name], [r3].[String]
FROM [RootReferencingEntity] AS [r]
LEFT JOIN [RootEntity] AS [r0] ON [r].[RootEntityId] = [r0].[Id]
LEFT JOIN [RequiredRelated] AS [r1] ON [r0].[Id] = [r1].[RootEntityId]
LEFT JOIN [RequiredRelated_OptionalNested] AS [r2] ON [r1].[RootEntityId] = [r2].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_RequiredNested] AS [r3] ON [r1].[RootEntityId] = [r3].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r4] ON [r1].[RootEntityId] = [r4].[AssociateTypeRootEntityId]
ORDER BY [r].[Id], [r0].[Id], [r1].[RootEntityId], [r2].[AssociateTypeRootEntityId], [r3].[AssociateTypeRootEntityId], [r4].[AssociateTypeRootEntityId]
""");
        }
    }

    public override async Task Select_unmapped_associate_scalar_property(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_unmapped_associate_scalar_property(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT [r0].[RootEntityId], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[String], [r].[Id], [r1].[AssociateTypeRootEntityId], [r2].[AssociateTypeRootEntityId], [r3].[AssociateTypeRootEntityId], [r3].[Id], [r3].[Int], [r3].[Ints], [r3].[Name], [r3].[String], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[String], [r2].[Id], [r2].[Int], [r2].[Ints], [r2].[Name], [r2].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [RequiredRelated] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
LEFT JOIN [RequiredRelated_OptionalNested] AS [r1] ON [r0].[RootEntityId] = [r1].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_RequiredNested] AS [r2] ON [r0].[RootEntityId] = [r2].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r3] ON [r0].[RootEntityId] = [r3].[AssociateTypeRootEntityId]
ORDER BY [r].[Id], [r0].[RootEntityId], [r1].[AssociateTypeRootEntityId], [r2].[AssociateTypeRootEntityId], [r3].[AssociateTypeRootEntityId]
""");
        }
    }

    public override async Task Select_untranslatable_method_on_associate_scalar_property(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_untranslatable_method_on_associate_scalar_property(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r0].[Int]
FROM [RootEntity] AS [r]
LEFT JOIN [RequiredRelated] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
""");
    }

    #endregion Structural properties

    #region Structural collection properties

    public override async Task Select_associate_collection(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_associate_collection(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT [r].[Id], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[Id1], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[Id2], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2]
FROM [RootEntity] AS [r]
LEFT JOIN (
    SELECT [r0].[RootEntityId], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[String], [r1].[AssociateTypeRootEntityId], [r1].[AssociateTypeId], [r2].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId0], [r2].[AssociateTypeId] AS [AssociateTypeId0], [r3].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId1], [r3].[AssociateTypeId] AS [AssociateTypeId1], [r3].[Id] AS [Id0], [r3].[Int] AS [Int0], [r3].[Ints] AS [Ints0], [r3].[Name] AS [Name0], [r3].[String] AS [String0], [r1].[Id] AS [Id1], [r1].[Int] AS [Int1], [r1].[Ints] AS [Ints1], [r1].[Name] AS [Name1], [r1].[String] AS [String1], [r2].[Id] AS [Id2], [r2].[Int] AS [Int2], [r2].[Ints] AS [Ints2], [r2].[Name] AS [Name2], [r2].[String] AS [String2]
    FROM [RelatedCollection] AS [r0]
    LEFT JOIN [RelatedCollection_OptionalNested] AS [r1] ON [r0].[RootEntityId] = [r1].[AssociateTypeRootEntityId] AND [r0].[Id] = [r1].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_RequiredNested] AS [r2] ON [r0].[RootEntityId] = [r2].[AssociateTypeRootEntityId] AND [r0].[Id] = [r2].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r3] ON [r0].[RootEntityId] = [r3].[AssociateTypeRootEntityId] AND [r0].[Id] = [r3].[AssociateTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
ORDER BY [r].[Id], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1]
""");
        }
    }

    public override async Task Select_nested_collection_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nested_collection_on_required_associate(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT [r].[Id], [r0].[RootEntityId], [r1].[AssociateTypeRootEntityId], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [RequiredRelated] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r1] ON [r0].[RootEntityId] = [r1].[AssociateTypeRootEntityId]
ORDER BY [r].[Id], [r0].[RootEntityId], [r1].[AssociateTypeRootEntityId]
""");
        }
    }

    public override async Task Select_nested_collection_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nested_collection_on_optional_associate(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT [r].[Id], [o].[RootEntityId], [o0].[AssociateTypeRootEntityId], [o0].[Id], [o0].[Int], [o0].[Ints], [o0].[Name], [o0].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o0] ON [o].[RootEntityId] = [o0].[AssociateTypeRootEntityId]
ORDER BY [r].[Id], [o].[RootEntityId], [o0].[AssociateTypeRootEntityId]
""");
        }
    }

    public override async Task SelectMany_associate_collection(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_associate_collection(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT [r0].[RootEntityId], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[String], [r].[Id], [r1].[AssociateTypeRootEntityId], [r1].[AssociateTypeId], [r2].[AssociateTypeRootEntityId], [r2].[AssociateTypeId], [r3].[AssociateTypeRootEntityId], [r3].[AssociateTypeId], [r3].[Id], [r3].[Int], [r3].[Ints], [r3].[Name], [r3].[String], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[String], [r2].[Id], [r2].[Int], [r2].[Ints], [r2].[Name], [r2].[String]
FROM [RootEntity] AS [r]
INNER JOIN [RelatedCollection] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
LEFT JOIN [RelatedCollection_OptionalNested] AS [r1] ON [r0].[RootEntityId] = [r1].[AssociateTypeRootEntityId] AND [r0].[Id] = [r1].[AssociateTypeId]
LEFT JOIN [RelatedCollection_RequiredNested] AS [r2] ON [r0].[RootEntityId] = [r2].[AssociateTypeRootEntityId] AND [r0].[Id] = [r2].[AssociateTypeId]
LEFT JOIN [RelatedCollection_NestedCollection] AS [r3] ON [r0].[RootEntityId] = [r3].[AssociateTypeRootEntityId] AND [r0].[Id] = [r3].[AssociateTypeId]
ORDER BY [r].[Id], [r0].[RootEntityId], [r0].[Id], [r1].[AssociateTypeRootEntityId], [r1].[AssociateTypeId], [r2].[AssociateTypeRootEntityId], [r2].[AssociateTypeId], [r3].[AssociateTypeRootEntityId], [r3].[AssociateTypeId]
""");
        }
    }

    public override async Task SelectMany_nested_collection_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_nested_collection_on_required_associate(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT [r1].[AssociateTypeRootEntityId], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [RequiredRelated] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
INNER JOIN [RequiredRelated_NestedCollection] AS [r1] ON [r0].[RootEntityId] = [r1].[AssociateTypeRootEntityId]
""");
        }
    }

    public override async Task SelectMany_nested_collection_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_nested_collection_on_optional_associate(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT [o0].[AssociateTypeRootEntityId], [o0].[Id], [o0].[Int], [o0].[Ints], [o0].[Name], [o0].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
INNER JOIN [OptionalRelated_NestedCollection] AS [o0] ON [o].[RootEntityId] = [o0].[AssociateTypeRootEntityId]
""");
        }
    }

    #endregion Structural collection properties

    #region Multiple

    public override async Task Select_root_duplicated(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_root_duplicated(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [o].[RootEntityId], [o0].[AssociateTypeRootEntityId], [o1].[AssociateTypeRootEntityId], [r0].[RootEntityId], [r1].[AssociateTypeRootEntityId], [r2].[AssociateTypeRootEntityId], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[Id1], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[Id2], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [o].[Id], [o].[Int], [o].[Ints], [o].[Name], [o].[String], [o2].[AssociateTypeRootEntityId], [o2].[Id], [o2].[Int], [o2].[Ints], [o2].[Name], [o2].[String], [o0].[Id], [o0].[Int], [o0].[Ints], [o0].[Name], [o0].[String], [o1].[Id], [o1].[Int], [o1].[Ints], [o1].[Name], [o1].[String], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[String], [r7].[AssociateTypeRootEntityId], [r7].[Id], [r7].[Int], [r7].[Ints], [r7].[Name], [r7].[String], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[String], [r2].[Id], [r2].[Int], [r2].[Ints], [r2].[Name], [r2].[String], [s0].[RootEntityId], [s0].[Id], [s0].[Int], [s0].[Ints], [s0].[Name], [s0].[String], [s0].[AssociateTypeRootEntityId], [s0].[AssociateTypeId], [s0].[AssociateTypeRootEntityId0], [s0].[AssociateTypeId0], [s0].[AssociateTypeRootEntityId1], [s0].[AssociateTypeId1], [s0].[Id0], [s0].[Int0], [s0].[Ints0], [s0].[Name0], [s0].[String0], [s0].[Id1], [s0].[Int1], [s0].[Ints1], [s0].[Name1], [s0].[String1], [s0].[Id2], [s0].[Int2], [s0].[Ints2], [s0].[Name2], [s0].[String2], [o3].[AssociateTypeRootEntityId], [o3].[Id], [o3].[Int], [o3].[Ints], [o3].[Name], [o3].[String], [r12].[AssociateTypeRootEntityId], [r12].[Id], [r12].[Int], [r12].[Ints], [r12].[Name], [r12].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
LEFT JOIN [OptionalRelated_OptionalNested] AS [o0] ON [o].[RootEntityId] = [o0].[AssociateTypeRootEntityId]
LEFT JOIN [OptionalRelated_RequiredNested] AS [o1] ON [o].[RootEntityId] = [o1].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
LEFT JOIN [RequiredRelated_OptionalNested] AS [r1] ON [r0].[RootEntityId] = [r1].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_RequiredNested] AS [r2] ON [r0].[RootEntityId] = [r2].[AssociateTypeRootEntityId]
LEFT JOIN (
    SELECT [r3].[RootEntityId], [r3].[Id], [r3].[Int], [r3].[Ints], [r3].[Name], [r3].[String], [r4].[AssociateTypeRootEntityId], [r4].[AssociateTypeId], [r5].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId0], [r5].[AssociateTypeId] AS [AssociateTypeId0], [r6].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId1], [r6].[AssociateTypeId] AS [AssociateTypeId1], [r6].[Id] AS [Id0], [r6].[Int] AS [Int0], [r6].[Ints] AS [Ints0], [r6].[Name] AS [Name0], [r6].[String] AS [String0], [r4].[Id] AS [Id1], [r4].[Int] AS [Int1], [r4].[Ints] AS [Ints1], [r4].[Name] AS [Name1], [r4].[String] AS [String1], [r5].[Id] AS [Id2], [r5].[Int] AS [Int2], [r5].[Ints] AS [Ints2], [r5].[Name] AS [Name2], [r5].[String] AS [String2]
    FROM [RelatedCollection] AS [r3]
    LEFT JOIN [RelatedCollection_OptionalNested] AS [r4] ON [r3].[RootEntityId] = [r4].[AssociateTypeRootEntityId] AND [r3].[Id] = [r4].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_RequiredNested] AS [r5] ON [r3].[RootEntityId] = [r5].[AssociateTypeRootEntityId] AND [r3].[Id] = [r5].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r6] ON [r3].[RootEntityId] = [r6].[AssociateTypeRootEntityId] AND [r3].[Id] = [r6].[AssociateTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o2] ON [o].[RootEntityId] = [o2].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r7] ON [r0].[RootEntityId] = [r7].[AssociateTypeRootEntityId]
LEFT JOIN (
    SELECT [r8].[RootEntityId], [r8].[Id], [r8].[Int], [r8].[Ints], [r8].[Name], [r8].[String], [r9].[AssociateTypeRootEntityId], [r9].[AssociateTypeId], [r10].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId0], [r10].[AssociateTypeId] AS [AssociateTypeId0], [r11].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId1], [r11].[AssociateTypeId] AS [AssociateTypeId1], [r11].[Id] AS [Id0], [r11].[Int] AS [Int0], [r11].[Ints] AS [Ints0], [r11].[Name] AS [Name0], [r11].[String] AS [String0], [r9].[Id] AS [Id1], [r9].[Int] AS [Int1], [r9].[Ints] AS [Ints1], [r9].[Name] AS [Name1], [r9].[String] AS [String1], [r10].[Id] AS [Id2], [r10].[Int] AS [Int2], [r10].[Ints] AS [Ints2], [r10].[Name] AS [Name2], [r10].[String] AS [String2]
    FROM [RelatedCollection] AS [r8]
    LEFT JOIN [RelatedCollection_OptionalNested] AS [r9] ON [r8].[RootEntityId] = [r9].[AssociateTypeRootEntityId] AND [r8].[Id] = [r9].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_RequiredNested] AS [r10] ON [r8].[RootEntityId] = [r10].[AssociateTypeRootEntityId] AND [r8].[Id] = [r10].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r11] ON [r8].[RootEntityId] = [r11].[AssociateTypeRootEntityId] AND [r8].[Id] = [r11].[AssociateTypeId]
) AS [s0] ON [r].[Id] = [s0].[RootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o3] ON [o].[RootEntityId] = [o3].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r12] ON [r0].[RootEntityId] = [r12].[AssociateTypeRootEntityId]
ORDER BY [r].[Id], [o].[RootEntityId], [o0].[AssociateTypeRootEntityId], [o1].[AssociateTypeRootEntityId], [r0].[RootEntityId], [r1].[AssociateTypeRootEntityId], [r2].[AssociateTypeRootEntityId], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id0], [o2].[AssociateTypeRootEntityId], [o2].[Id], [r7].[AssociateTypeRootEntityId], [r7].[Id], [s0].[RootEntityId], [s0].[Id], [s0].[AssociateTypeRootEntityId], [s0].[AssociateTypeId], [s0].[AssociateTypeRootEntityId0], [s0].[AssociateTypeId0], [s0].[AssociateTypeRootEntityId1], [s0].[AssociateTypeId1], [s0].[Id0], [o3].[AssociateTypeRootEntityId], [o3].[Id], [r12].[AssociateTypeRootEntityId]
""");
    }

    public override async Task Select_associate_and_target_to_index_based_binding_via_closure(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_associate_and_target_to_index_based_binding_via_closure(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT [r].[Id], [r0].[RootEntityId], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[String], [r1].[AssociateTypeRootEntityId], [r2].[AssociateTypeRootEntityId], [r3].[AssociateTypeRootEntityId], [r3].[Id], [r3].[Int], [r3].[Ints], [r3].[Name], [r3].[String], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[String], [r2].[Id], [r2].[Int], [r2].[Ints], [r2].[Name], [r2].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [RequiredRelated] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
LEFT JOIN [RequiredRelated_OptionalNested] AS [r1] ON [r0].[RootEntityId] = [r1].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_RequiredNested] AS [r2] ON [r0].[RootEntityId] = [r2].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r3] ON [r0].[RootEntityId] = [r3].[AssociateTypeRootEntityId]
ORDER BY [r].[Id], [r0].[RootEntityId], [r1].[AssociateTypeRootEntityId], [r2].[AssociateTypeRootEntityId], [r3].[AssociateTypeRootEntityId]
""");
        }
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
SELECT [s].[AssociateTypeRootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String]
FROM [RootEntity] AS [r]
OUTER APPLY (
    SELECT TOP(1) [r2].[AssociateTypeRootEntityId], [r2].[Id], [r2].[Int], [r2].[Ints], [r2].[Name], [r2].[String]
    FROM [RootEntity] AS [r0]
    LEFT JOIN [RequiredRelated] AS [r1] ON [r0].[Id] = [r1].[RootEntityId]
    LEFT JOIN [RequiredRelated_RequiredNested] AS [r2] ON [r1].[RootEntityId] = [r2].[AssociateTypeRootEntityId]
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
SELECT [s].[AssociateTypeRootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String]
FROM [RootEntity] AS [r]
OUTER APPLY (
    SELECT TOP(1) [o0].[AssociateTypeRootEntityId], [o0].[Id], [o0].[Int], [o0].[Ints], [o0].[Name], [o0].[String]
    FROM [RootEntity] AS [r0]
    LEFT JOIN [OptionalRelated] AS [o] ON [r0].[Id] = [o].[RootEntityId]
    LEFT JOIN [OptionalRelated_RequiredNested] AS [o0] ON [o].[RootEntityId] = [o0].[AssociateTypeRootEntityId]
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
