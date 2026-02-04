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
SELECT [r].[Id], [r].[Name], [r].[OptionalAssociateId], [r].[RequiredAssociateId], [a].[Id], [n].[Id], [n0].[Id], [a0].[Id], [n1].[Id], [n2].[Id], [s].[Id], [s].[CollectionRootId], [s].[Int], [s].[Ints], [s].[Name], [s].[OptionalNestedAssociateId], [s].[RequiredNestedAssociateId], [s].[String], [s].[Id0], [s].[Id1], [s].[Id2], [s].[CollectionAssociateId], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[CollectionAssociateId0], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[CollectionAssociateId1], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [a].[CollectionRootId], [a].[Int], [a].[Ints], [a].[Name], [a].[OptionalNestedAssociateId], [a].[RequiredNestedAssociateId], [a].[String], [n6].[Id], [n6].[CollectionAssociateId], [n6].[Int], [n6].[Ints], [n6].[Name], [n6].[String], [n].[CollectionAssociateId], [n].[Int], [n].[Ints], [n].[Name], [n].[String], [n0].[CollectionAssociateId], [n0].[Int], [n0].[Ints], [n0].[Name], [n0].[String], [a0].[CollectionRootId], [a0].[Int], [a0].[Ints], [a0].[Name], [a0].[OptionalNestedAssociateId], [a0].[RequiredNestedAssociateId], [a0].[String], [n7].[Id], [n7].[CollectionAssociateId], [n7].[Int], [n7].[Ints], [n7].[Name], [n7].[String], [n1].[CollectionAssociateId], [n1].[Int], [n1].[Ints], [n1].[Name], [n1].[String], [n2].[CollectionAssociateId], [n2].[Int], [n2].[Ints], [n2].[Name], [n2].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [AssociateType] AS [a] ON [r].[OptionalAssociateId] = [a].[Id]
LEFT JOIN [NestedAssociateType] AS [n] ON [a].[OptionalNestedAssociateId] = [n].[Id]
LEFT JOIN [NestedAssociateType] AS [n0] ON [a].[RequiredNestedAssociateId] = [n0].[Id]
INNER JOIN [AssociateType] AS [a0] ON [r].[RequiredAssociateId] = [a0].[Id]
LEFT JOIN [NestedAssociateType] AS [n1] ON [a0].[OptionalNestedAssociateId] = [n1].[Id]
INNER JOIN [NestedAssociateType] AS [n2] ON [a0].[RequiredNestedAssociateId] = [n2].[Id]
LEFT JOIN (
    SELECT [a1].[Id], [a1].[CollectionRootId], [a1].[Int], [a1].[Ints], [a1].[Name], [a1].[OptionalNestedAssociateId], [a1].[RequiredNestedAssociateId], [a1].[String], [n3].[Id] AS [Id0], [n4].[Id] AS [Id1], [n5].[Id] AS [Id2], [n5].[CollectionAssociateId], [n5].[Int] AS [Int0], [n5].[Ints] AS [Ints0], [n5].[Name] AS [Name0], [n5].[String] AS [String0], [n3].[CollectionAssociateId] AS [CollectionAssociateId0], [n3].[Int] AS [Int1], [n3].[Ints] AS [Ints1], [n3].[Name] AS [Name1], [n3].[String] AS [String1], [n4].[CollectionAssociateId] AS [CollectionAssociateId1], [n4].[Int] AS [Int2], [n4].[Ints] AS [Ints2], [n4].[Name] AS [Name2], [n4].[String] AS [String2]
    FROM [AssociateType] AS [a1]
    LEFT JOIN [NestedAssociateType] AS [n3] ON [a1].[OptionalNestedAssociateId] = [n3].[Id]
    INNER JOIN [NestedAssociateType] AS [n4] ON [a1].[RequiredNestedAssociateId] = [n4].[Id]
    LEFT JOIN [NestedAssociateType] AS [n5] ON [a1].[Id] = [n5].[CollectionAssociateId]
) AS [s] ON [r].[Id] = [s].[CollectionRootId]
LEFT JOIN [NestedAssociateType] AS [n6] ON [a].[Id] = [n6].[CollectionAssociateId]
LEFT JOIN [NestedAssociateType] AS [n7] ON [a0].[Id] = [n7].[CollectionAssociateId]
ORDER BY [r].[Id], [a].[Id], [n].[Id], [n0].[Id], [a0].[Id], [n1].[Id], [n2].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2], [n6].[Id]
""");
    }

    #region Scalar properties

    public override async Task Select_scalar_property_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_scalar_property_on_required_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT [a].[String]
FROM [RootEntity] AS [r]
INNER JOIN [AssociateType] AS [a] ON [r].[RequiredAssociateId] = [a].[Id]
""");
    }

    public override async Task Select_property_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_property_on_optional_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT [a].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [AssociateType] AS [a] ON [r].[OptionalAssociateId] = [a].[Id]
""");
    }

    public override async Task Select_value_type_property_on_null_associate_throws(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_value_type_property_on_null_associate_throws(queryTrackingBehavior);

        AssertSql(
            """
SELECT [a].[Int]
FROM [RootEntity] AS [r]
LEFT JOIN [AssociateType] AS [a] ON [r].[OptionalAssociateId] = [a].[Id]
""");
    }

    public override async Task Select_nullable_value_type_property_on_null_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nullable_value_type_property_on_null_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT [a].[Int]
FROM [RootEntity] AS [r]
LEFT JOIN [AssociateType] AS [a] ON [r].[OptionalAssociateId] = [a].[Id]
""");
    }

    #endregion Scalar properties

    #region Structural properties

    public override async Task Select_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT [a].[Id], [a].[CollectionRootId], [a].[Int], [a].[Ints], [a].[Name], [a].[OptionalNestedAssociateId], [a].[RequiredNestedAssociateId], [a].[String], [r].[Id], [n].[Id], [n0].[Id], [n1].[Id], [n1].[CollectionAssociateId], [n1].[Int], [n1].[Ints], [n1].[Name], [n1].[String], [n].[CollectionAssociateId], [n].[Int], [n].[Ints], [n].[Name], [n].[String], [n0].[CollectionAssociateId], [n0].[Int], [n0].[Ints], [n0].[Name], [n0].[String]
FROM [RootEntity] AS [r]
INNER JOIN [AssociateType] AS [a] ON [r].[RequiredAssociateId] = [a].[Id]
LEFT JOIN [NestedAssociateType] AS [n] ON [a].[OptionalNestedAssociateId] = [n].[Id]
INNER JOIN [NestedAssociateType] AS [n0] ON [a].[RequiredNestedAssociateId] = [n0].[Id]
LEFT JOIN [NestedAssociateType] AS [n1] ON [a].[Id] = [n1].[CollectionAssociateId]
ORDER BY [r].[Id], [a].[Id], [n].[Id], [n0].[Id]
""");
    }

    public override async Task Select_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT [a].[Id], [a].[CollectionRootId], [a].[Int], [a].[Ints], [a].[Name], [a].[OptionalNestedAssociateId], [a].[RequiredNestedAssociateId], [a].[String], [r].[Id], [n].[Id], [n0].[Id], [n1].[Id], [n1].[CollectionAssociateId], [n1].[Int], [n1].[Ints], [n1].[Name], [n1].[String], [n].[CollectionAssociateId], [n].[Int], [n].[Ints], [n].[Name], [n].[String], [n0].[CollectionAssociateId], [n0].[Int], [n0].[Ints], [n0].[Name], [n0].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [AssociateType] AS [a] ON [r].[OptionalAssociateId] = [a].[Id]
LEFT JOIN [NestedAssociateType] AS [n] ON [a].[OptionalNestedAssociateId] = [n].[Id]
LEFT JOIN [NestedAssociateType] AS [n0] ON [a].[RequiredNestedAssociateId] = [n0].[Id]
LEFT JOIN [NestedAssociateType] AS [n1] ON [a].[Id] = [n1].[CollectionAssociateId]
ORDER BY [r].[Id], [a].[Id], [n].[Id], [n0].[Id]
""");
    }

    public override async Task Select_required_nested_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_nested_on_required_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT [n].[Id], [n].[CollectionAssociateId], [n].[Int], [n].[Ints], [n].[Name], [n].[String]
FROM [RootEntity] AS [r]
INNER JOIN [AssociateType] AS [a] ON [r].[RequiredAssociateId] = [a].[Id]
INNER JOIN [NestedAssociateType] AS [n] ON [a].[RequiredNestedAssociateId] = [n].[Id]
""");
    }

    public override async Task Select_optional_nested_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_nested_on_required_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT [n].[Id], [n].[CollectionAssociateId], [n].[Int], [n].[Ints], [n].[Name], [n].[String]
FROM [RootEntity] AS [r]
INNER JOIN [AssociateType] AS [a] ON [r].[RequiredAssociateId] = [a].[Id]
LEFT JOIN [NestedAssociateType] AS [n] ON [a].[OptionalNestedAssociateId] = [n].[Id]
""");
    }

    public override async Task Select_required_nested_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_nested_on_optional_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT [n].[Id], [n].[CollectionAssociateId], [n].[Int], [n].[Ints], [n].[Name], [n].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [AssociateType] AS [a] ON [r].[OptionalAssociateId] = [a].[Id]
LEFT JOIN [NestedAssociateType] AS [n] ON [a].[RequiredNestedAssociateId] = [n].[Id]
""");
    }

    public override async Task Select_optional_nested_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_nested_on_optional_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT [n].[Id], [n].[CollectionAssociateId], [n].[Int], [n].[Ints], [n].[Name], [n].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [AssociateType] AS [a] ON [r].[OptionalAssociateId] = [a].[Id]
LEFT JOIN [NestedAssociateType] AS [n] ON [a].[OptionalNestedAssociateId] = [n].[Id]
""");
    }

    public override async Task Select_required_associate_via_optional_navigation(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_associate_via_optional_navigation(queryTrackingBehavior);

        AssertSql(
            """
SELECT [a].[Id], [a].[CollectionRootId], [a].[Int], [a].[Ints], [a].[Name], [a].[OptionalNestedAssociateId], [a].[RequiredNestedAssociateId], [a].[String], [r].[Id], [r0].[Id], [n].[Id], [n0].[Id], [n1].[Id], [n1].[CollectionAssociateId], [n1].[Int], [n1].[Ints], [n1].[Name], [n1].[String], [n].[CollectionAssociateId], [n].[Int], [n].[Ints], [n].[Name], [n].[String], [n0].[CollectionAssociateId], [n0].[Int], [n0].[Ints], [n0].[Name], [n0].[String]
FROM [RootReferencingEntity] AS [r]
LEFT JOIN [RootEntity] AS [r0] ON [r].[RootEntityId] = [r0].[Id]
LEFT JOIN [AssociateType] AS [a] ON [r0].[RequiredAssociateId] = [a].[Id]
LEFT JOIN [NestedAssociateType] AS [n] ON [a].[OptionalNestedAssociateId] = [n].[Id]
LEFT JOIN [NestedAssociateType] AS [n0] ON [a].[RequiredNestedAssociateId] = [n0].[Id]
LEFT JOIN [NestedAssociateType] AS [n1] ON [a].[Id] = [n1].[CollectionAssociateId]
ORDER BY [r].[Id], [r0].[Id], [a].[Id], [n].[Id], [n0].[Id]
""");
    }

    public override async Task Select_unmapped_associate_scalar_property(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_unmapped_associate_scalar_property(queryTrackingBehavior);

        AssertSql(
            """
SELECT [a].[Id], [a].[CollectionRootId], [a].[Int], [a].[Ints], [a].[Name], [a].[OptionalNestedAssociateId], [a].[RequiredNestedAssociateId], [a].[String], [r].[Id], [n].[Id], [n0].[Id], [n1].[Id], [n1].[CollectionAssociateId], [n1].[Int], [n1].[Ints], [n1].[Name], [n1].[String], [n].[CollectionAssociateId], [n].[Int], [n].[Ints], [n].[Name], [n].[String], [n0].[CollectionAssociateId], [n0].[Int], [n0].[Ints], [n0].[Name], [n0].[String]
FROM [RootEntity] AS [r]
INNER JOIN [AssociateType] AS [a] ON [r].[RequiredAssociateId] = [a].[Id]
LEFT JOIN [NestedAssociateType] AS [n] ON [a].[OptionalNestedAssociateId] = [n].[Id]
INNER JOIN [NestedAssociateType] AS [n0] ON [a].[RequiredNestedAssociateId] = [n0].[Id]
LEFT JOIN [NestedAssociateType] AS [n1] ON [a].[Id] = [n1].[CollectionAssociateId]
ORDER BY [r].[Id], [a].[Id], [n].[Id], [n0].[Id]
""");
    }

    public override async Task Select_untranslatable_method_on_associate_scalar_property(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_untranslatable_method_on_associate_scalar_property(queryTrackingBehavior);

        AssertSql(
            """
SELECT [a].[Int]
FROM [RootEntity] AS [r]
INNER JOIN [AssociateType] AS [a] ON [r].[RequiredAssociateId] = [a].[Id]
""");
    }

    #endregion Structural properties

    #region Structural collection properties

    public override async Task Select_associate_collection(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_associate_collection(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [s].[Id], [s].[CollectionRootId], [s].[Int], [s].[Ints], [s].[Name], [s].[OptionalNestedAssociateId], [s].[RequiredNestedAssociateId], [s].[String], [s].[Id0], [s].[Id1], [s].[Id2], [s].[CollectionAssociateId], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[CollectionAssociateId0], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[CollectionAssociateId1], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2]
FROM [RootEntity] AS [r]
LEFT JOIN (
    SELECT [a].[Id], [a].[CollectionRootId], [a].[Int], [a].[Ints], [a].[Name], [a].[OptionalNestedAssociateId], [a].[RequiredNestedAssociateId], [a].[String], [n].[Id] AS [Id0], [n0].[Id] AS [Id1], [n1].[Id] AS [Id2], [n1].[CollectionAssociateId], [n1].[Int] AS [Int0], [n1].[Ints] AS [Ints0], [n1].[Name] AS [Name0], [n1].[String] AS [String0], [n].[CollectionAssociateId] AS [CollectionAssociateId0], [n].[Int] AS [Int1], [n].[Ints] AS [Ints1], [n].[Name] AS [Name1], [n].[String] AS [String1], [n0].[CollectionAssociateId] AS [CollectionAssociateId1], [n0].[Int] AS [Int2], [n0].[Ints] AS [Ints2], [n0].[Name] AS [Name2], [n0].[String] AS [String2]
    FROM [AssociateType] AS [a]
    LEFT JOIN [NestedAssociateType] AS [n] ON [a].[OptionalNestedAssociateId] = [n].[Id]
    INNER JOIN [NestedAssociateType] AS [n0] ON [a].[RequiredNestedAssociateId] = [n0].[Id]
    LEFT JOIN [NestedAssociateType] AS [n1] ON [a].[Id] = [n1].[CollectionAssociateId]
) AS [s] ON [r].[Id] = [s].[CollectionRootId]
ORDER BY [r].[Id], [s].[Id], [s].[Id0], [s].[Id1]
""");
    }

    public override async Task Select_nested_collection_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nested_collection_on_required_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [a].[Id], [n].[Id], [n].[CollectionAssociateId], [n].[Int], [n].[Ints], [n].[Name], [n].[String]
FROM [RootEntity] AS [r]
INNER JOIN [AssociateType] AS [a] ON [r].[RequiredAssociateId] = [a].[Id]
LEFT JOIN [NestedAssociateType] AS [n] ON [a].[Id] = [n].[CollectionAssociateId]
ORDER BY [r].[Id], [a].[Id]
""");
    }

    public override async Task Select_nested_collection_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nested_collection_on_optional_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [a].[Id], [n].[Id], [n].[CollectionAssociateId], [n].[Int], [n].[Ints], [n].[Name], [n].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [AssociateType] AS [a] ON [r].[OptionalAssociateId] = [a].[Id]
LEFT JOIN [NestedAssociateType] AS [n] ON [a].[Id] = [n].[CollectionAssociateId]
ORDER BY [r].[Id], [a].[Id]
""");
    }

    public override async Task SelectMany_associate_collection(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_associate_collection(queryTrackingBehavior);

        AssertSql(
            """
SELECT [a].[Id], [a].[CollectionRootId], [a].[Int], [a].[Ints], [a].[Name], [a].[OptionalNestedAssociateId], [a].[RequiredNestedAssociateId], [a].[String], [r].[Id], [n].[Id], [n0].[Id], [n1].[Id], [n1].[CollectionAssociateId], [n1].[Int], [n1].[Ints], [n1].[Name], [n1].[String], [n].[CollectionAssociateId], [n].[Int], [n].[Ints], [n].[Name], [n].[String], [n0].[CollectionAssociateId], [n0].[Int], [n0].[Ints], [n0].[Name], [n0].[String]
FROM [RootEntity] AS [r]
INNER JOIN [AssociateType] AS [a] ON [r].[Id] = [a].[CollectionRootId]
LEFT JOIN [NestedAssociateType] AS [n] ON [a].[OptionalNestedAssociateId] = [n].[Id]
INNER JOIN [NestedAssociateType] AS [n0] ON [a].[RequiredNestedAssociateId] = [n0].[Id]
LEFT JOIN [NestedAssociateType] AS [n1] ON [a].[Id] = [n1].[CollectionAssociateId]
ORDER BY [r].[Id], [a].[Id], [n].[Id], [n0].[Id]
""");
    }

    public override async Task SelectMany_nested_collection_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_nested_collection_on_required_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT [n].[Id], [n].[CollectionAssociateId], [n].[Int], [n].[Ints], [n].[Name], [n].[String]
FROM [RootEntity] AS [r]
INNER JOIN [AssociateType] AS [a] ON [r].[RequiredAssociateId] = [a].[Id]
INNER JOIN [NestedAssociateType] AS [n] ON [a].[Id] = [n].[CollectionAssociateId]
""");
    }

    public override async Task SelectMany_nested_collection_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_nested_collection_on_optional_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT [n].[Id], [n].[CollectionAssociateId], [n].[Int], [n].[Ints], [n].[Name], [n].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [AssociateType] AS [a] ON [r].[OptionalAssociateId] = [a].[Id]
INNER JOIN [NestedAssociateType] AS [n] ON [a].[Id] = [n].[CollectionAssociateId]
""");
    }

    #endregion Structural collection properties

    #region Multiple

    public override async Task Select_root_duplicated(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_root_duplicated(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalAssociateId], [r].[RequiredAssociateId], [a].[Id], [n].[Id], [n0].[Id], [a0].[Id], [n1].[Id], [n2].[Id], [s].[Id], [s].[CollectionRootId], [s].[Int], [s].[Ints], [s].[Name], [s].[OptionalNestedAssociateId], [s].[RequiredNestedAssociateId], [s].[String], [s].[Id0], [s].[Id1], [s].[Id2], [s].[CollectionAssociateId], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[CollectionAssociateId0], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[CollectionAssociateId1], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [a].[CollectionRootId], [a].[Int], [a].[Ints], [a].[Name], [a].[OptionalNestedAssociateId], [a].[RequiredNestedAssociateId], [a].[String], [n6].[Id], [n6].[CollectionAssociateId], [n6].[Int], [n6].[Ints], [n6].[Name], [n6].[String], [n].[CollectionAssociateId], [n].[Int], [n].[Ints], [n].[Name], [n].[String], [n0].[CollectionAssociateId], [n0].[Int], [n0].[Ints], [n0].[Name], [n0].[String], [a0].[CollectionRootId], [a0].[Int], [a0].[Ints], [a0].[Name], [a0].[OptionalNestedAssociateId], [a0].[RequiredNestedAssociateId], [a0].[String], [n7].[Id], [n7].[CollectionAssociateId], [n7].[Int], [n7].[Ints], [n7].[Name], [n7].[String], [n1].[CollectionAssociateId], [n1].[Int], [n1].[Ints], [n1].[Name], [n1].[String], [n2].[CollectionAssociateId], [n2].[Int], [n2].[Ints], [n2].[Name], [n2].[String], [s0].[Id], [s0].[CollectionRootId], [s0].[Int], [s0].[Ints], [s0].[Name], [s0].[OptionalNestedAssociateId], [s0].[RequiredNestedAssociateId], [s0].[String], [s0].[Id0], [s0].[Id1], [s0].[Id2], [s0].[CollectionAssociateId], [s0].[Int0], [s0].[Ints0], [s0].[Name0], [s0].[String0], [s0].[CollectionAssociateId0], [s0].[Int1], [s0].[Ints1], [s0].[Name1], [s0].[String1], [s0].[CollectionAssociateId1], [s0].[Int2], [s0].[Ints2], [s0].[Name2], [s0].[String2], [n11].[Id], [n11].[CollectionAssociateId], [n11].[Int], [n11].[Ints], [n11].[Name], [n11].[String], [n12].[Id], [n12].[CollectionAssociateId], [n12].[Int], [n12].[Ints], [n12].[Name], [n12].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [AssociateType] AS [a] ON [r].[OptionalAssociateId] = [a].[Id]
LEFT JOIN [NestedAssociateType] AS [n] ON [a].[OptionalNestedAssociateId] = [n].[Id]
LEFT JOIN [NestedAssociateType] AS [n0] ON [a].[RequiredNestedAssociateId] = [n0].[Id]
INNER JOIN [AssociateType] AS [a0] ON [r].[RequiredAssociateId] = [a0].[Id]
LEFT JOIN [NestedAssociateType] AS [n1] ON [a0].[OptionalNestedAssociateId] = [n1].[Id]
INNER JOIN [NestedAssociateType] AS [n2] ON [a0].[RequiredNestedAssociateId] = [n2].[Id]
LEFT JOIN (
    SELECT [a1].[Id], [a1].[CollectionRootId], [a1].[Int], [a1].[Ints], [a1].[Name], [a1].[OptionalNestedAssociateId], [a1].[RequiredNestedAssociateId], [a1].[String], [n3].[Id] AS [Id0], [n4].[Id] AS [Id1], [n5].[Id] AS [Id2], [n5].[CollectionAssociateId], [n5].[Int] AS [Int0], [n5].[Ints] AS [Ints0], [n5].[Name] AS [Name0], [n5].[String] AS [String0], [n3].[CollectionAssociateId] AS [CollectionAssociateId0], [n3].[Int] AS [Int1], [n3].[Ints] AS [Ints1], [n3].[Name] AS [Name1], [n3].[String] AS [String1], [n4].[CollectionAssociateId] AS [CollectionAssociateId1], [n4].[Int] AS [Int2], [n4].[Ints] AS [Ints2], [n4].[Name] AS [Name2], [n4].[String] AS [String2]
    FROM [AssociateType] AS [a1]
    LEFT JOIN [NestedAssociateType] AS [n3] ON [a1].[OptionalNestedAssociateId] = [n3].[Id]
    INNER JOIN [NestedAssociateType] AS [n4] ON [a1].[RequiredNestedAssociateId] = [n4].[Id]
    LEFT JOIN [NestedAssociateType] AS [n5] ON [a1].[Id] = [n5].[CollectionAssociateId]
) AS [s] ON [r].[Id] = [s].[CollectionRootId]
LEFT JOIN [NestedAssociateType] AS [n6] ON [a].[Id] = [n6].[CollectionAssociateId]
LEFT JOIN [NestedAssociateType] AS [n7] ON [a0].[Id] = [n7].[CollectionAssociateId]
LEFT JOIN (
    SELECT [a2].[Id], [a2].[CollectionRootId], [a2].[Int], [a2].[Ints], [a2].[Name], [a2].[OptionalNestedAssociateId], [a2].[RequiredNestedAssociateId], [a2].[String], [n8].[Id] AS [Id0], [n9].[Id] AS [Id1], [n10].[Id] AS [Id2], [n10].[CollectionAssociateId], [n10].[Int] AS [Int0], [n10].[Ints] AS [Ints0], [n10].[Name] AS [Name0], [n10].[String] AS [String0], [n8].[CollectionAssociateId] AS [CollectionAssociateId0], [n8].[Int] AS [Int1], [n8].[Ints] AS [Ints1], [n8].[Name] AS [Name1], [n8].[String] AS [String1], [n9].[CollectionAssociateId] AS [CollectionAssociateId1], [n9].[Int] AS [Int2], [n9].[Ints] AS [Ints2], [n9].[Name] AS [Name2], [n9].[String] AS [String2]
    FROM [AssociateType] AS [a2]
    LEFT JOIN [NestedAssociateType] AS [n8] ON [a2].[OptionalNestedAssociateId] = [n8].[Id]
    INNER JOIN [NestedAssociateType] AS [n9] ON [a2].[RequiredNestedAssociateId] = [n9].[Id]
    LEFT JOIN [NestedAssociateType] AS [n10] ON [a2].[Id] = [n10].[CollectionAssociateId]
) AS [s0] ON [r].[Id] = [s0].[CollectionRootId]
LEFT JOIN [NestedAssociateType] AS [n11] ON [a].[Id] = [n11].[CollectionAssociateId]
LEFT JOIN [NestedAssociateType] AS [n12] ON [a0].[Id] = [n12].[CollectionAssociateId]
ORDER BY [r].[Id], [a].[Id], [n].[Id], [n0].[Id], [a0].[Id], [n1].[Id], [n2].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2], [n6].[Id], [n7].[Id], [s0].[Id], [s0].[Id0], [s0].[Id1], [s0].[Id2], [n11].[Id]
""");
    }

    public override async Task Select_associate_and_target_to_index_based_binding_via_closure(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_associate_and_target_to_index_based_binding_via_closure(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [a].[Id], [a].[CollectionRootId], [a].[Int], [a].[Ints], [a].[Name], [a].[OptionalNestedAssociateId], [a].[RequiredNestedAssociateId], [a].[String], [n].[Id], [n0].[Id], [n1].[Id], [n1].[CollectionAssociateId], [n1].[Int], [n1].[Ints], [n1].[Name], [n1].[String], [n].[CollectionAssociateId], [n].[Int], [n].[Ints], [n].[Name], [n].[String], [n0].[CollectionAssociateId], [n0].[Int], [n0].[Ints], [n0].[Name], [n0].[String]
FROM [RootEntity] AS [r]
INNER JOIN [AssociateType] AS [a] ON [r].[RequiredAssociateId] = [a].[Id]
LEFT JOIN [NestedAssociateType] AS [n] ON [a].[OptionalNestedAssociateId] = [n].[Id]
INNER JOIN [NestedAssociateType] AS [n0] ON [a].[RequiredNestedAssociateId] = [n0].[Id]
LEFT JOIN [NestedAssociateType] AS [n1] ON [a].[Id] = [n1].[CollectionAssociateId]
ORDER BY [r].[Id], [a].[Id], [n].[Id], [n0].[Id]
""");
    }

    #endregion Multiple

    #region Subquery

    public override async Task Select_subquery_required_related_FirstOrDefault(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_subquery_required_related_FirstOrDefault(queryTrackingBehavior);

        AssertSql(
            """
SELECT [s].[Id], [s].[CollectionAssociateId], [s].[Int], [s].[Ints], [s].[Name], [s].[String]
FROM [RootEntity] AS [r]
OUTER APPLY (
    SELECT TOP(1) [n].[Id], [n].[CollectionAssociateId], [n].[Int], [n].[Ints], [n].[Name], [n].[String]
    FROM [RootEntity] AS [r0]
    INNER JOIN [AssociateType] AS [a] ON [r0].[RequiredAssociateId] = [a].[Id]
    INNER JOIN [NestedAssociateType] AS [n] ON [a].[RequiredNestedAssociateId] = [n].[Id]
    ORDER BY [r0].[Id]
) AS [s]
""");
    }

    public override async Task Select_subquery_optional_related_FirstOrDefault(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_subquery_optional_related_FirstOrDefault(queryTrackingBehavior);

        AssertSql(
            """
SELECT [s].[Id], [s].[CollectionAssociateId], [s].[Int], [s].[Ints], [s].[Name], [s].[String]
FROM [RootEntity] AS [r]
OUTER APPLY (
    SELECT TOP(1) [n].[Id], [n].[CollectionAssociateId], [n].[Int], [n].[Ints], [n].[Name], [n].[String]
    FROM [RootEntity] AS [r0]
    LEFT JOIN [AssociateType] AS [a] ON [r0].[OptionalAssociateId] = [a].[Id]
    LEFT JOIN [NestedAssociateType] AS [n] ON [a].[RequiredNestedAssociateId] = [n].[Id]
    ORDER BY [r0].[Id]
) AS [s]
""");
    }

    #endregion Subquery

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
