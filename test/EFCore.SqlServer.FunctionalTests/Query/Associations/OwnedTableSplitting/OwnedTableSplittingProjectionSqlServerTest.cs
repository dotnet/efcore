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
SELECT [r].[Id], [r].[Name], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[OptionalNestedAssociate_Id], [s].[OptionalNestedAssociate_Int], [s].[OptionalNestedAssociate_Ints], [s].[OptionalNestedAssociate_Name], [s].[OptionalNestedAssociate_String], [s].[RequiredNestedAssociate_Id], [s].[RequiredNestedAssociate_Int], [s].[RequiredNestedAssociate_Ints], [s].[RequiredNestedAssociate_Name], [s].[RequiredNestedAssociate_String], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [o].[AssociateTypeRootEntityId], [o].[Id], [o].[Int], [o].[Ints], [o].[Name], [o].[String], [r].[OptionalAssociate_OptionalNestedAssociate_Id], [r].[OptionalAssociate_OptionalNestedAssociate_Int], [r].[OptionalAssociate_OptionalNestedAssociate_Ints], [r].[OptionalAssociate_OptionalNestedAssociate_Name], [r].[OptionalAssociate_OptionalNestedAssociate_String], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r2].[AssociateTypeRootEntityId], [r2].[Id], [r2].[Int], [r2].[Ints], [r2].[Name], [r2].[String], [r].[RequiredAssociate_OptionalNestedAssociate_Id], [r].[RequiredAssociate_OptionalNestedAssociate_Int], [r].[RequiredAssociate_OptionalNestedAssociate_Ints], [r].[RequiredAssociate_OptionalNestedAssociate_Name], [r].[RequiredAssociate_OptionalNestedAssociate_String], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
LEFT JOIN (
    SELECT [r0].[RootEntityId], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[String], [r1].[AssociateTypeRootEntityId], [r1].[AssociateTypeId], [r1].[Id] AS [Id0], [r1].[Int] AS [Int0], [r1].[Ints] AS [Ints0], [r1].[Name] AS [Name0], [r1].[String] AS [String0], [r0].[OptionalNestedAssociate_Id], [r0].[OptionalNestedAssociate_Int], [r0].[OptionalNestedAssociate_Ints], [r0].[OptionalNestedAssociate_Name], [r0].[OptionalNestedAssociate_String], [r0].[RequiredNestedAssociate_Id], [r0].[RequiredNestedAssociate_Int], [r0].[RequiredNestedAssociate_Ints], [r0].[RequiredNestedAssociate_Name], [r0].[RequiredNestedAssociate_String]
    FROM [RelatedCollection] AS [r0]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r1] ON [r0].[RootEntityId] = [r1].[AssociateTypeRootEntityId] AND [r0].[Id] = [r1].[AssociateTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o] ON CASE
    WHEN [r].[OptionalAssociate_Id] IS NOT NULL AND [r].[OptionalAssociate_Int] IS NOT NULL AND [r].[OptionalAssociate_Ints] IS NOT NULL AND [r].[OptionalAssociate_Name] IS NOT NULL AND [r].[OptionalAssociate_String] IS NOT NULL THEN [r].[Id]
END = [o].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r2] ON [r].[Id] = [r2].[AssociateTypeRootEntityId]
ORDER BY [r].[Id], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[Id0], [o].[AssociateTypeRootEntityId], [o].[Id], [r2].[AssociateTypeRootEntityId]
""");
    }

    #region Scalar properties

    public override async Task Select_scalar_property_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_scalar_property_on_required_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[RequiredAssociate_String]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_property_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_property_on_optional_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[OptionalAssociate_String]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_value_type_property_on_null_associate_throws(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_value_type_property_on_null_associate_throws(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[OptionalAssociate_Int]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_nullable_value_type_property_on_null_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nullable_value_type_property_on_null_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[OptionalAssociate_Int]
FROM [RootEntity] AS [r]
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
SELECT [r].[Id], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r0].[AssociateTypeRootEntityId], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[String], [r].[RequiredAssociate_OptionalNestedAssociate_Id], [r].[RequiredAssociate_OptionalNestedAssociate_Int], [r].[RequiredAssociate_OptionalNestedAssociate_Ints], [r].[RequiredAssociate_OptionalNestedAssociate_Name], [r].[RequiredAssociate_OptionalNestedAssociate_String], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r0] ON [r].[Id] = [r0].[AssociateTypeRootEntityId]
ORDER BY [r].[Id], [r0].[AssociateTypeRootEntityId]
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
SELECT [r].[Id], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [o].[AssociateTypeRootEntityId], [o].[Id], [o].[Int], [o].[Ints], [o].[Name], [o].[String], [r].[OptionalAssociate_OptionalNestedAssociate_Id], [r].[OptionalAssociate_OptionalNestedAssociate_Int], [r].[OptionalAssociate_OptionalNestedAssociate_Ints], [r].[OptionalAssociate_OptionalNestedAssociate_Name], [r].[OptionalAssociate_OptionalNestedAssociate_String], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o] ON CASE
    WHEN [r].[OptionalAssociate_Id] IS NOT NULL AND [r].[OptionalAssociate_Int] IS NOT NULL AND [r].[OptionalAssociate_Ints] IS NOT NULL AND [r].[OptionalAssociate_Name] IS NOT NULL AND [r].[OptionalAssociate_String] IS NOT NULL THEN [r].[Id]
END = [o].[AssociateTypeRootEntityId]
ORDER BY [r].[Id], [o].[AssociateTypeRootEntityId]
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
SELECT [r].[Id], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
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
SELECT [r].[Id], [r].[RequiredAssociate_OptionalNestedAssociate_Id], [r].[RequiredAssociate_OptionalNestedAssociate_Int], [r].[RequiredAssociate_OptionalNestedAssociate_Ints], [r].[RequiredAssociate_OptionalNestedAssociate_Name], [r].[RequiredAssociate_OptionalNestedAssociate_String]
FROM [RootEntity] AS [r]
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
SELECT [r].[Id], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
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
SELECT [r].[Id], [r].[OptionalAssociate_OptionalNestedAssociate_Id], [r].[OptionalAssociate_OptionalNestedAssociate_Int], [r].[OptionalAssociate_OptionalNestedAssociate_Ints], [r].[OptionalAssociate_OptionalNestedAssociate_Name], [r].[OptionalAssociate_OptionalNestedAssociate_String]
FROM [RootEntity] AS [r]
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
SELECT [r0].[Id], [r0].[RequiredAssociate_Id], [r0].[RequiredAssociate_Int], [r0].[RequiredAssociate_Ints], [r0].[RequiredAssociate_Name], [r0].[RequiredAssociate_String], [r].[Id], [r1].[AssociateTypeRootEntityId], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[String], [r0].[RequiredAssociate_OptionalNestedAssociate_Id], [r0].[RequiredAssociate_OptionalNestedAssociate_Int], [r0].[RequiredAssociate_OptionalNestedAssociate_Ints], [r0].[RequiredAssociate_OptionalNestedAssociate_Name], [r0].[RequiredAssociate_OptionalNestedAssociate_String], [r0].[RequiredAssociate_RequiredNestedAssociate_Id], [r0].[RequiredAssociate_RequiredNestedAssociate_Int], [r0].[RequiredAssociate_RequiredNestedAssociate_Ints], [r0].[RequiredAssociate_RequiredNestedAssociate_Name], [r0].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootReferencingEntity] AS [r]
LEFT JOIN [RootEntity] AS [r0] ON [r].[RootEntityId] = [r0].[Id]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r1] ON [r0].[Id] = [r1].[AssociateTypeRootEntityId]
ORDER BY [r].[Id], [r0].[Id], [r1].[AssociateTypeRootEntityId]
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
SELECT [r].[Id], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r0].[AssociateTypeRootEntityId], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[String], [r].[RequiredAssociate_OptionalNestedAssociate_Id], [r].[RequiredAssociate_OptionalNestedAssociate_Int], [r].[RequiredAssociate_OptionalNestedAssociate_Ints], [r].[RequiredAssociate_OptionalNestedAssociate_Name], [r].[RequiredAssociate_OptionalNestedAssociate_String], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r0] ON [r].[Id] = [r0].[AssociateTypeRootEntityId]
ORDER BY [r].[Id], [r0].[AssociateTypeRootEntityId]
""");
        }
    }

    public override async Task Select_untranslatable_method_on_associate_scalar_property(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_untranslatable_method_on_associate_scalar_property(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[RequiredAssociate_Int]
FROM [RootEntity] AS [r]
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
SELECT [r].[Id], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[OptionalNestedAssociate_Id], [s].[OptionalNestedAssociate_Int], [s].[OptionalNestedAssociate_Ints], [s].[OptionalNestedAssociate_Name], [s].[OptionalNestedAssociate_String], [s].[RequiredNestedAssociate_Id], [s].[RequiredNestedAssociate_Int], [s].[RequiredNestedAssociate_Ints], [s].[RequiredNestedAssociate_Name], [s].[RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
LEFT JOIN (
    SELECT [r0].[RootEntityId], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[String], [r1].[AssociateTypeRootEntityId], [r1].[AssociateTypeId], [r1].[Id] AS [Id0], [r1].[Int] AS [Int0], [r1].[Ints] AS [Ints0], [r1].[Name] AS [Name0], [r1].[String] AS [String0], [r0].[OptionalNestedAssociate_Id], [r0].[OptionalNestedAssociate_Int], [r0].[OptionalNestedAssociate_Ints], [r0].[OptionalNestedAssociate_Name], [r0].[OptionalNestedAssociate_String], [r0].[RequiredNestedAssociate_Id], [r0].[RequiredNestedAssociate_Int], [r0].[RequiredNestedAssociate_Ints], [r0].[RequiredNestedAssociate_Name], [r0].[RequiredNestedAssociate_String]
    FROM [RelatedCollection] AS [r0]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r1] ON [r0].[RootEntityId] = [r1].[AssociateTypeRootEntityId] AND [r0].[Id] = [r1].[AssociateTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
ORDER BY [r].[Id], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId]
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
SELECT [r].[Id], [r0].[AssociateTypeRootEntityId], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r0] ON [r].[Id] = [r0].[AssociateTypeRootEntityId]
ORDER BY [r].[Id], [r0].[AssociateTypeRootEntityId]
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
SELECT [r].[Id], [o].[AssociateTypeRootEntityId], [o].[Id], [o].[Int], [o].[Ints], [o].[Name], [o].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o] ON CASE
    WHEN [r].[OptionalAssociate_Id] IS NOT NULL AND [r].[OptionalAssociate_Int] IS NOT NULL AND [r].[OptionalAssociate_Ints] IS NOT NULL AND [r].[OptionalAssociate_Name] IS NOT NULL AND [r].[OptionalAssociate_String] IS NOT NULL THEN [r].[Id]
END = [o].[AssociateTypeRootEntityId]
ORDER BY [r].[Id], [o].[AssociateTypeRootEntityId]
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
SELECT [r0].[RootEntityId], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[String], [r].[Id], [r1].[AssociateTypeRootEntityId], [r1].[AssociateTypeId], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[String], [r0].[OptionalNestedAssociate_Id], [r0].[OptionalNestedAssociate_Int], [r0].[OptionalNestedAssociate_Ints], [r0].[OptionalNestedAssociate_Name], [r0].[OptionalNestedAssociate_String], [r0].[RequiredNestedAssociate_Id], [r0].[RequiredNestedAssociate_Int], [r0].[RequiredNestedAssociate_Ints], [r0].[RequiredNestedAssociate_Name], [r0].[RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
INNER JOIN [RelatedCollection] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
LEFT JOIN [RelatedCollection_NestedCollection] AS [r1] ON [r0].[RootEntityId] = [r1].[AssociateTypeRootEntityId] AND [r0].[Id] = [r1].[AssociateTypeId]
ORDER BY [r].[Id], [r0].[RootEntityId], [r0].[Id], [r1].[AssociateTypeRootEntityId], [r1].[AssociateTypeId]
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
SELECT [r0].[AssociateTypeRootEntityId], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[String]
FROM [RootEntity] AS [r]
INNER JOIN [RequiredRelated_NestedCollection] AS [r0] ON [r].[Id] = [r0].[AssociateTypeRootEntityId]
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
SELECT [o].[AssociateTypeRootEntityId], [o].[Id], [o].[Int], [o].[Ints], [o].[Name], [o].[String]
FROM [RootEntity] AS [r]
INNER JOIN [OptionalRelated_NestedCollection] AS [o] ON CASE
    WHEN [r].[OptionalAssociate_Id] IS NOT NULL AND [r].[OptionalAssociate_Int] IS NOT NULL AND [r].[OptionalAssociate_Ints] IS NOT NULL AND [r].[OptionalAssociate_Name] IS NOT NULL AND [r].[OptionalAssociate_String] IS NOT NULL THEN [r].[Id]
END = [o].[AssociateTypeRootEntityId]
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
SELECT [r].[Id], [r].[Name], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[OptionalNestedAssociate_Id], [s].[OptionalNestedAssociate_Int], [s].[OptionalNestedAssociate_Ints], [s].[OptionalNestedAssociate_Name], [s].[OptionalNestedAssociate_String], [s].[RequiredNestedAssociate_Id], [s].[RequiredNestedAssociate_Int], [s].[RequiredNestedAssociate_Ints], [s].[RequiredNestedAssociate_Name], [s].[RequiredNestedAssociate_String], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [o].[AssociateTypeRootEntityId], [o].[Id], [o].[Int], [o].[Ints], [o].[Name], [o].[String], [r].[OptionalAssociate_OptionalNestedAssociate_Id], [r].[OptionalAssociate_OptionalNestedAssociate_Int], [r].[OptionalAssociate_OptionalNestedAssociate_Ints], [r].[OptionalAssociate_OptionalNestedAssociate_Name], [r].[OptionalAssociate_OptionalNestedAssociate_String], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r2].[AssociateTypeRootEntityId], [r2].[Id], [r2].[Int], [r2].[Ints], [r2].[Name], [r2].[String], [r].[RequiredAssociate_OptionalNestedAssociate_Id], [r].[RequiredAssociate_OptionalNestedAssociate_Int], [r].[RequiredAssociate_OptionalNestedAssociate_Ints], [r].[RequiredAssociate_OptionalNestedAssociate_Name], [r].[RequiredAssociate_OptionalNestedAssociate_String], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String], [s0].[RootEntityId], [s0].[Id], [s0].[Int], [s0].[Ints], [s0].[Name], [s0].[String], [s0].[AssociateTypeRootEntityId], [s0].[AssociateTypeId], [s0].[Id0], [s0].[Int0], [s0].[Ints0], [s0].[Name0], [s0].[String0], [s0].[OptionalNestedAssociate_Id], [s0].[OptionalNestedAssociate_Int], [s0].[OptionalNestedAssociate_Ints], [s0].[OptionalNestedAssociate_Name], [s0].[OptionalNestedAssociate_String], [s0].[RequiredNestedAssociate_Id], [s0].[RequiredNestedAssociate_Int], [s0].[RequiredNestedAssociate_Ints], [s0].[RequiredNestedAssociate_Name], [s0].[RequiredNestedAssociate_String], [o0].[AssociateTypeRootEntityId], [o0].[Id], [o0].[Int], [o0].[Ints], [o0].[Name], [o0].[String], [r5].[AssociateTypeRootEntityId], [r5].[Id], [r5].[Int], [r5].[Ints], [r5].[Name], [r5].[String]
FROM [RootEntity] AS [r]
LEFT JOIN (
    SELECT [r0].[RootEntityId], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[String], [r1].[AssociateTypeRootEntityId], [r1].[AssociateTypeId], [r1].[Id] AS [Id0], [r1].[Int] AS [Int0], [r1].[Ints] AS [Ints0], [r1].[Name] AS [Name0], [r1].[String] AS [String0], [r0].[OptionalNestedAssociate_Id], [r0].[OptionalNestedAssociate_Int], [r0].[OptionalNestedAssociate_Ints], [r0].[OptionalNestedAssociate_Name], [r0].[OptionalNestedAssociate_String], [r0].[RequiredNestedAssociate_Id], [r0].[RequiredNestedAssociate_Int], [r0].[RequiredNestedAssociate_Ints], [r0].[RequiredNestedAssociate_Name], [r0].[RequiredNestedAssociate_String]
    FROM [RelatedCollection] AS [r0]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r1] ON [r0].[RootEntityId] = [r1].[AssociateTypeRootEntityId] AND [r0].[Id] = [r1].[AssociateTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o] ON CASE
    WHEN [r].[OptionalAssociate_Id] IS NOT NULL AND [r].[OptionalAssociate_Int] IS NOT NULL AND [r].[OptionalAssociate_Ints] IS NOT NULL AND [r].[OptionalAssociate_Name] IS NOT NULL AND [r].[OptionalAssociate_String] IS NOT NULL THEN [r].[Id]
END = [o].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r2] ON [r].[Id] = [r2].[AssociateTypeRootEntityId]
LEFT JOIN (
    SELECT [r3].[RootEntityId], [r3].[Id], [r3].[Int], [r3].[Ints], [r3].[Name], [r3].[String], [r4].[AssociateTypeRootEntityId], [r4].[AssociateTypeId], [r4].[Id] AS [Id0], [r4].[Int] AS [Int0], [r4].[Ints] AS [Ints0], [r4].[Name] AS [Name0], [r4].[String] AS [String0], [r3].[OptionalNestedAssociate_Id], [r3].[OptionalNestedAssociate_Int], [r3].[OptionalNestedAssociate_Ints], [r3].[OptionalNestedAssociate_Name], [r3].[OptionalNestedAssociate_String], [r3].[RequiredNestedAssociate_Id], [r3].[RequiredNestedAssociate_Int], [r3].[RequiredNestedAssociate_Ints], [r3].[RequiredNestedAssociate_Name], [r3].[RequiredNestedAssociate_String]
    FROM [RelatedCollection] AS [r3]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r4] ON [r3].[RootEntityId] = [r4].[AssociateTypeRootEntityId] AND [r3].[Id] = [r4].[AssociateTypeId]
) AS [s0] ON [r].[Id] = [s0].[RootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o0] ON CASE
    WHEN [r].[OptionalAssociate_Id] IS NOT NULL AND [r].[OptionalAssociate_Int] IS NOT NULL AND [r].[OptionalAssociate_Ints] IS NOT NULL AND [r].[OptionalAssociate_Name] IS NOT NULL AND [r].[OptionalAssociate_String] IS NOT NULL THEN [r].[Id]
END = [o0].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r5] ON [r].[Id] = [r5].[AssociateTypeRootEntityId]
ORDER BY [r].[Id], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[Id0], [o].[AssociateTypeRootEntityId], [o].[Id], [r2].[AssociateTypeRootEntityId], [r2].[Id], [s0].[RootEntityId], [s0].[Id], [s0].[AssociateTypeRootEntityId], [s0].[AssociateTypeId], [s0].[Id0], [o0].[AssociateTypeRootEntityId], [o0].[Id], [r5].[AssociateTypeRootEntityId]
""");
    }

    public override async Task Select_associate_and_target_to_index_based_binding_via_closure(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_associate_and_target_to_index_based_binding_via_closure(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r0].[AssociateTypeRootEntityId], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[String], [r].[RequiredAssociate_OptionalNestedAssociate_Id], [r].[RequiredAssociate_OptionalNestedAssociate_Int], [r].[RequiredAssociate_OptionalNestedAssociate_Ints], [r].[RequiredAssociate_OptionalNestedAssociate_Name], [r].[RequiredAssociate_OptionalNestedAssociate_String], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r0] ON [r].[Id] = [r0].[AssociateTypeRootEntityId]
ORDER BY [r].[Id], [r0].[AssociateTypeRootEntityId]
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
SELECT [r1].[Id], [r1].[RequiredAssociate_RequiredNestedAssociate_Id], [r1].[RequiredAssociate_RequiredNestedAssociate_Int], [r1].[RequiredAssociate_RequiredNestedAssociate_Ints], [r1].[RequiredAssociate_RequiredNestedAssociate_Name], [r1].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
OUTER APPLY (
    SELECT TOP(1) [r0].[Id], [r0].[RequiredAssociate_RequiredNestedAssociate_Id], [r0].[RequiredAssociate_RequiredNestedAssociate_Int], [r0].[RequiredAssociate_RequiredNestedAssociate_Ints], [r0].[RequiredAssociate_RequiredNestedAssociate_Name], [r0].[RequiredAssociate_RequiredNestedAssociate_String]
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
SELECT [r1].[Id], [r1].[OptionalAssociate_RequiredNestedAssociate_Id], [r1].[OptionalAssociate_RequiredNestedAssociate_Int], [r1].[OptionalAssociate_RequiredNestedAssociate_Ints], [r1].[OptionalAssociate_RequiredNestedAssociate_Name], [r1].[OptionalAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
OUTER APPLY (
    SELECT TOP(1) [r0].[Id], [r0].[OptionalAssociate_RequiredNestedAssociate_Id], [r0].[OptionalAssociate_RequiredNestedAssociate_Int], [r0].[OptionalAssociate_RequiredNestedAssociate_Ints], [r0].[OptionalAssociate_RequiredNestedAssociate_Name], [r0].[OptionalAssociate_RequiredNestedAssociate_String]
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
