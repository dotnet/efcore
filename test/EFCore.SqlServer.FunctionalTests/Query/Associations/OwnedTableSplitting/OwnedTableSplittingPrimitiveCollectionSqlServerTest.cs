// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedTableSplitting;

public class OwnedTableSplittingPrimitiveCollectionSqlServerTest(
    OwnedTableSplittingSqlServerFixture fixture,
    ITestOutputHelper testOutputHelper)
    : OwnedTableSplittingPrimitiveCollectionRelationalTestBase<OwnedTableSplittingSqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Count()
    {
        await base.Count();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated_Id], [r].[OptionalRelated_Int], [r].[OptionalRelated_Ints], [r].[OptionalRelated_Name], [r].[OptionalRelated_String], [o].[RelatedTypeRootEntityId], [o].[Id], [o].[Int], [o].[Ints], [o].[Name], [o].[String], [r].[OptionalRelated_OptionalNested_Id], [r].[OptionalRelated_OptionalNested_Int], [r].[OptionalRelated_OptionalNested_Ints], [r].[OptionalRelated_OptionalNested_Name], [r].[OptionalRelated_OptionalNested_String], [r].[OptionalRelated_RequiredNested_Id], [r].[OptionalRelated_RequiredNested_Int], [r].[OptionalRelated_RequiredNested_Ints], [r].[OptionalRelated_RequiredNested_Name], [r].[OptionalRelated_RequiredNested_String], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[OptionalNested_Id], [s].[OptionalNested_Int], [s].[OptionalNested_Ints], [s].[OptionalNested_Name], [s].[OptionalNested_String], [s].[RequiredNested_Id], [s].[RequiredNested_Int], [s].[RequiredNested_Ints], [s].[RequiredNested_Name], [s].[RequiredNested_String], [r].[RequiredRelated_Id], [r].[RequiredRelated_Int], [r].[RequiredRelated_Ints], [r].[RequiredRelated_Name], [r].[RequiredRelated_String], [r3].[RelatedTypeRootEntityId], [r3].[Id], [r3].[Int], [r3].[Ints], [r3].[Name], [r3].[String], [r].[RequiredRelated_OptionalNested_Id], [r].[RequiredRelated_OptionalNested_Int], [r].[RequiredRelated_OptionalNested_Ints], [r].[RequiredRelated_OptionalNested_Name], [r].[RequiredRelated_OptionalNested_String], [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Ints], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o] ON CASE
    WHEN [r].[OptionalRelated_Id] IS NOT NULL AND [r].[OptionalRelated_Int] IS NOT NULL AND [r].[OptionalRelated_Ints] IS NOT NULL AND [r].[OptionalRelated_Name] IS NOT NULL AND [r].[OptionalRelated_String] IS NOT NULL THEN [r].[Id]
END = [o].[RelatedTypeRootEntityId]
LEFT JOIN (
    SELECT [r1].[RootEntityId], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[String], [r2].[RelatedTypeRootEntityId], [r2].[RelatedTypeId], [r2].[Id] AS [Id0], [r2].[Int] AS [Int0], [r2].[Ints] AS [Ints0], [r2].[Name] AS [Name0], [r2].[String] AS [String0], [r1].[OptionalNested_Id], [r1].[OptionalNested_Int], [r1].[OptionalNested_Ints], [r1].[OptionalNested_Name], [r1].[OptionalNested_String], [r1].[RequiredNested_Id], [r1].[RequiredNested_Int], [r1].[RequiredNested_Ints], [r1].[RequiredNested_Name], [r1].[RequiredNested_String]
    FROM [RelatedCollection] AS [r1]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r2] ON [r1].[RootEntityId] = [r2].[RelatedTypeRootEntityId] AND [r1].[Id] = [r2].[RelatedTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r3] ON [r].[Id] = [r3].[RelatedTypeRootEntityId]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([r].[RequiredRelated_Ints]) AS [r0]) = 3
ORDER BY [r].[Id], [o].[RelatedTypeRootEntityId], [o].[Id], [s].[RootEntityId], [s].[Id], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[Id0], [r3].[RelatedTypeRootEntityId]
""");
    }

    public override async Task Index()
    {
        await base.Index();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated_Id], [r].[OptionalRelated_Int], [r].[OptionalRelated_Ints], [r].[OptionalRelated_Name], [r].[OptionalRelated_String], [o].[RelatedTypeRootEntityId], [o].[Id], [o].[Int], [o].[Ints], [o].[Name], [o].[String], [r].[OptionalRelated_OptionalNested_Id], [r].[OptionalRelated_OptionalNested_Int], [r].[OptionalRelated_OptionalNested_Ints], [r].[OptionalRelated_OptionalNested_Name], [r].[OptionalRelated_OptionalNested_String], [r].[OptionalRelated_RequiredNested_Id], [r].[OptionalRelated_RequiredNested_Int], [r].[OptionalRelated_RequiredNested_Ints], [r].[OptionalRelated_RequiredNested_Name], [r].[OptionalRelated_RequiredNested_String], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[OptionalNested_Id], [s].[OptionalNested_Int], [s].[OptionalNested_Ints], [s].[OptionalNested_Name], [s].[OptionalNested_String], [s].[RequiredNested_Id], [s].[RequiredNested_Int], [s].[RequiredNested_Ints], [s].[RequiredNested_Name], [s].[RequiredNested_String], [r].[RequiredRelated_Id], [r].[RequiredRelated_Int], [r].[RequiredRelated_Ints], [r].[RequiredRelated_Name], [r].[RequiredRelated_String], [r2].[RelatedTypeRootEntityId], [r2].[Id], [r2].[Int], [r2].[Ints], [r2].[Name], [r2].[String], [r].[RequiredRelated_OptionalNested_Id], [r].[RequiredRelated_OptionalNested_Int], [r].[RequiredRelated_OptionalNested_Ints], [r].[RequiredRelated_OptionalNested_Name], [r].[RequiredRelated_OptionalNested_String], [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Ints], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o] ON CASE
    WHEN [r].[OptionalRelated_Id] IS NOT NULL AND [r].[OptionalRelated_Int] IS NOT NULL AND [r].[OptionalRelated_Ints] IS NOT NULL AND [r].[OptionalRelated_Name] IS NOT NULL AND [r].[OptionalRelated_String] IS NOT NULL THEN [r].[Id]
END = [o].[RelatedTypeRootEntityId]
LEFT JOIN (
    SELECT [r0].[RootEntityId], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[String], [r1].[RelatedTypeRootEntityId], [r1].[RelatedTypeId], [r1].[Id] AS [Id0], [r1].[Int] AS [Int0], [r1].[Ints] AS [Ints0], [r1].[Name] AS [Name0], [r1].[String] AS [String0], [r0].[OptionalNested_Id], [r0].[OptionalNested_Int], [r0].[OptionalNested_Ints], [r0].[OptionalNested_Name], [r0].[OptionalNested_String], [r0].[RequiredNested_Id], [r0].[RequiredNested_Int], [r0].[RequiredNested_Ints], [r0].[RequiredNested_Name], [r0].[RequiredNested_String]
    FROM [RelatedCollection] AS [r0]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r1] ON [r0].[RootEntityId] = [r1].[RelatedTypeRootEntityId] AND [r0].[Id] = [r1].[RelatedTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r2] ON [r].[Id] = [r2].[RelatedTypeRootEntityId]
WHERE CAST(JSON_VALUE([r].[RequiredRelated_Ints], '$[0]') AS int) = 1
ORDER BY [r].[Id], [o].[RelatedTypeRootEntityId], [o].[Id], [s].[RootEntityId], [s].[Id], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[Id0], [r2].[RelatedTypeRootEntityId]
""");
    }

    public override async Task Contains()
    {
        await base.Contains();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated_Id], [r].[OptionalRelated_Int], [r].[OptionalRelated_Ints], [r].[OptionalRelated_Name], [r].[OptionalRelated_String], [o].[RelatedTypeRootEntityId], [o].[Id], [o].[Int], [o].[Ints], [o].[Name], [o].[String], [r].[OptionalRelated_OptionalNested_Id], [r].[OptionalRelated_OptionalNested_Int], [r].[OptionalRelated_OptionalNested_Ints], [r].[OptionalRelated_OptionalNested_Name], [r].[OptionalRelated_OptionalNested_String], [r].[OptionalRelated_RequiredNested_Id], [r].[OptionalRelated_RequiredNested_Int], [r].[OptionalRelated_RequiredNested_Ints], [r].[OptionalRelated_RequiredNested_Name], [r].[OptionalRelated_RequiredNested_String], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[OptionalNested_Id], [s].[OptionalNested_Int], [s].[OptionalNested_Ints], [s].[OptionalNested_Name], [s].[OptionalNested_String], [s].[RequiredNested_Id], [s].[RequiredNested_Int], [s].[RequiredNested_Ints], [s].[RequiredNested_Name], [s].[RequiredNested_String], [r].[RequiredRelated_Id], [r].[RequiredRelated_Int], [r].[RequiredRelated_Ints], [r].[RequiredRelated_Name], [r].[RequiredRelated_String], [r3].[RelatedTypeRootEntityId], [r3].[Id], [r3].[Int], [r3].[Ints], [r3].[Name], [r3].[String], [r].[RequiredRelated_OptionalNested_Id], [r].[RequiredRelated_OptionalNested_Int], [r].[RequiredRelated_OptionalNested_Ints], [r].[RequiredRelated_OptionalNested_Name], [r].[RequiredRelated_OptionalNested_String], [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Ints], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o] ON CASE
    WHEN [r].[OptionalRelated_Id] IS NOT NULL AND [r].[OptionalRelated_Int] IS NOT NULL AND [r].[OptionalRelated_Ints] IS NOT NULL AND [r].[OptionalRelated_Name] IS NOT NULL AND [r].[OptionalRelated_String] IS NOT NULL THEN [r].[Id]
END = [o].[RelatedTypeRootEntityId]
LEFT JOIN (
    SELECT [r1].[RootEntityId], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[String], [r2].[RelatedTypeRootEntityId], [r2].[RelatedTypeId], [r2].[Id] AS [Id0], [r2].[Int] AS [Int0], [r2].[Ints] AS [Ints0], [r2].[Name] AS [Name0], [r2].[String] AS [String0], [r1].[OptionalNested_Id], [r1].[OptionalNested_Int], [r1].[OptionalNested_Ints], [r1].[OptionalNested_Name], [r1].[OptionalNested_String], [r1].[RequiredNested_Id], [r1].[RequiredNested_Int], [r1].[RequiredNested_Ints], [r1].[RequiredNested_Name], [r1].[RequiredNested_String]
    FROM [RelatedCollection] AS [r1]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r2] ON [r1].[RootEntityId] = [r2].[RelatedTypeRootEntityId] AND [r1].[Id] = [r2].[RelatedTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r3] ON [r].[Id] = [r3].[RelatedTypeRootEntityId]
WHERE 3 IN (
    SELECT [r0].[value]
    FROM OPENJSON([r].[RequiredRelated_Ints]) WITH ([value] int '$') AS [r0]
)
ORDER BY [r].[Id], [o].[RelatedTypeRootEntityId], [o].[Id], [s].[RootEntityId], [s].[Id], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[Id0], [r3].[RelatedTypeRootEntityId]
""");
    }

    public override async Task Any_predicate()
    {
        await base.Any_predicate();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated_Id], [r].[OptionalRelated_Int], [r].[OptionalRelated_Ints], [r].[OptionalRelated_Name], [r].[OptionalRelated_String], [o].[RelatedTypeRootEntityId], [o].[Id], [o].[Int], [o].[Ints], [o].[Name], [o].[String], [r].[OptionalRelated_OptionalNested_Id], [r].[OptionalRelated_OptionalNested_Int], [r].[OptionalRelated_OptionalNested_Ints], [r].[OptionalRelated_OptionalNested_Name], [r].[OptionalRelated_OptionalNested_String], [r].[OptionalRelated_RequiredNested_Id], [r].[OptionalRelated_RequiredNested_Int], [r].[OptionalRelated_RequiredNested_Ints], [r].[OptionalRelated_RequiredNested_Name], [r].[OptionalRelated_RequiredNested_String], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[OptionalNested_Id], [s].[OptionalNested_Int], [s].[OptionalNested_Ints], [s].[OptionalNested_Name], [s].[OptionalNested_String], [s].[RequiredNested_Id], [s].[RequiredNested_Int], [s].[RequiredNested_Ints], [s].[RequiredNested_Name], [s].[RequiredNested_String], [r].[RequiredRelated_Id], [r].[RequiredRelated_Int], [r].[RequiredRelated_Ints], [r].[RequiredRelated_Name], [r].[RequiredRelated_String], [r3].[RelatedTypeRootEntityId], [r3].[Id], [r3].[Int], [r3].[Ints], [r3].[Name], [r3].[String], [r].[RequiredRelated_OptionalNested_Id], [r].[RequiredRelated_OptionalNested_Int], [r].[RequiredRelated_OptionalNested_Ints], [r].[RequiredRelated_OptionalNested_Name], [r].[RequiredRelated_OptionalNested_String], [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Ints], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o] ON CASE
    WHEN [r].[OptionalRelated_Id] IS NOT NULL AND [r].[OptionalRelated_Int] IS NOT NULL AND [r].[OptionalRelated_Ints] IS NOT NULL AND [r].[OptionalRelated_Name] IS NOT NULL AND [r].[OptionalRelated_String] IS NOT NULL THEN [r].[Id]
END = [o].[RelatedTypeRootEntityId]
LEFT JOIN (
    SELECT [r1].[RootEntityId], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[String], [r2].[RelatedTypeRootEntityId], [r2].[RelatedTypeId], [r2].[Id] AS [Id0], [r2].[Int] AS [Int0], [r2].[Ints] AS [Ints0], [r2].[Name] AS [Name0], [r2].[String] AS [String0], [r1].[OptionalNested_Id], [r1].[OptionalNested_Int], [r1].[OptionalNested_Ints], [r1].[OptionalNested_Name], [r1].[OptionalNested_String], [r1].[RequiredNested_Id], [r1].[RequiredNested_Int], [r1].[RequiredNested_Ints], [r1].[RequiredNested_Name], [r1].[RequiredNested_String]
    FROM [RelatedCollection] AS [r1]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r2] ON [r1].[RootEntityId] = [r2].[RelatedTypeRootEntityId] AND [r1].[Id] = [r2].[RelatedTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r3] ON [r].[Id] = [r3].[RelatedTypeRootEntityId]
WHERE 2 IN (
    SELECT [r0].[value]
    FROM OPENJSON([r].[RequiredRelated_Ints]) WITH ([value] int '$') AS [r0]
)
ORDER BY [r].[Id], [o].[RelatedTypeRootEntityId], [o].[Id], [s].[RootEntityId], [s].[Id], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[Id0], [r3].[RelatedTypeRootEntityId]
""");
    }

    public override async Task Nested_Count()
    {
        await base.Nested_Count();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated_Id], [r].[OptionalRelated_Int], [r].[OptionalRelated_Ints], [r].[OptionalRelated_Name], [r].[OptionalRelated_String], [o].[RelatedTypeRootEntityId], [o].[Id], [o].[Int], [o].[Ints], [o].[Name], [o].[String], [r].[OptionalRelated_OptionalNested_Id], [r].[OptionalRelated_OptionalNested_Int], [r].[OptionalRelated_OptionalNested_Ints], [r].[OptionalRelated_OptionalNested_Name], [r].[OptionalRelated_OptionalNested_String], [r].[OptionalRelated_RequiredNested_Id], [r].[OptionalRelated_RequiredNested_Int], [r].[OptionalRelated_RequiredNested_Ints], [r].[OptionalRelated_RequiredNested_Name], [r].[OptionalRelated_RequiredNested_String], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[OptionalNested_Id], [s].[OptionalNested_Int], [s].[OptionalNested_Ints], [s].[OptionalNested_Name], [s].[OptionalNested_String], [s].[RequiredNested_Id], [s].[RequiredNested_Int], [s].[RequiredNested_Ints], [s].[RequiredNested_Name], [s].[RequiredNested_String], [r].[RequiredRelated_Id], [r].[RequiredRelated_Int], [r].[RequiredRelated_Ints], [r].[RequiredRelated_Name], [r].[RequiredRelated_String], [r3].[RelatedTypeRootEntityId], [r3].[Id], [r3].[Int], [r3].[Ints], [r3].[Name], [r3].[String], [r].[RequiredRelated_OptionalNested_Id], [r].[RequiredRelated_OptionalNested_Int], [r].[RequiredRelated_OptionalNested_Ints], [r].[RequiredRelated_OptionalNested_Name], [r].[RequiredRelated_OptionalNested_String], [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Ints], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o] ON CASE
    WHEN [r].[OptionalRelated_Id] IS NOT NULL AND [r].[OptionalRelated_Int] IS NOT NULL AND [r].[OptionalRelated_Ints] IS NOT NULL AND [r].[OptionalRelated_Name] IS NOT NULL AND [r].[OptionalRelated_String] IS NOT NULL THEN [r].[Id]
END = [o].[RelatedTypeRootEntityId]
LEFT JOIN (
    SELECT [r1].[RootEntityId], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[String], [r2].[RelatedTypeRootEntityId], [r2].[RelatedTypeId], [r2].[Id] AS [Id0], [r2].[Int] AS [Int0], [r2].[Ints] AS [Ints0], [r2].[Name] AS [Name0], [r2].[String] AS [String0], [r1].[OptionalNested_Id], [r1].[OptionalNested_Int], [r1].[OptionalNested_Ints], [r1].[OptionalNested_Name], [r1].[OptionalNested_String], [r1].[RequiredNested_Id], [r1].[RequiredNested_Int], [r1].[RequiredNested_Ints], [r1].[RequiredNested_Name], [r1].[RequiredNested_String]
    FROM [RelatedCollection] AS [r1]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r2] ON [r1].[RootEntityId] = [r2].[RelatedTypeRootEntityId] AND [r1].[Id] = [r2].[RelatedTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r3] ON [r].[Id] = [r3].[RelatedTypeRootEntityId]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([r].[RequiredRelated_RequiredNested_Ints]) AS [r0]) = 3
ORDER BY [r].[Id], [o].[RelatedTypeRootEntityId], [o].[Id], [s].[RootEntityId], [s].[Id], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[Id0], [r3].[RelatedTypeRootEntityId]
""");
    }

    public override async Task Select_Sum()
    {
        await base.Select_Sum();

        AssertSql(
            """
SELECT (
    SELECT COALESCE(SUM([r1].[value]), 0)
    FROM OPENJSON([r].[RequiredRelated_Ints]) WITH ([value] int '$') AS [r1])
FROM [RootEntity] AS [r]
WHERE (
    SELECT COALESCE(SUM([r0].[value]), 0)
    FROM OPENJSON([r].[RequiredRelated_Ints]) WITH ([value] int '$') AS [r0]) >= 6
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
