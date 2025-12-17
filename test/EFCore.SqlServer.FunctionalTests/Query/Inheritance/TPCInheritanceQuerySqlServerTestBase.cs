// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

#nullable disable

public abstract class TPCInheritanceQuerySqlServerTestBase<TFixture>(TFixture fixture, ITestOutputHelper testOutputHelper)
    : TPCInheritanceQueryTestBase<TFixture>(fixture, testOutputHelper)
    where TFixture : TPCInheritanceQuerySqlServerFixtureBase, new()
{
    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType().BaseType);

    public override async Task Byte_enum_value_constant_used_in_projection(bool async)
    {
        await base.Byte_enum_value_constant_used_in_projection(async);

        AssertSql(
            """
SELECT CASE
    WHEN [k].[IsFlightless] = CAST(1 AS bit) THEN CAST(0 AS tinyint)
    ELSE CAST(1 AS tinyint)
END
FROM [Kiwi] AS [k]
""");
    }

    public override async Task Can_filter_all_animals(bool async)
    {
        await base.Can_filter_all_animals(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[CountryId], [u].[Name], [u].[Species], [u].[EagleId], [u].[IsFlightless], [u].[Group], [u].[FoundOn], [u].[Discriminator]
FROM (
    SELECT [e].[Id], [e].[CountryId], [e].[Name], [e].[Species], [e].[EagleId], [e].[IsFlightless], [e].[Group], NULL AS [FoundOn], N'Eagle' AS [Discriminator]
    FROM [Eagle] AS [e]
    UNION ALL
    SELECT [k].[Id], [k].[CountryId], [k].[Name], [k].[Species], [k].[EagleId], [k].[IsFlightless], NULL AS [Group], [k].[FoundOn], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [u]
WHERE [u].[Name] = N'Great spotted kiwi'
ORDER BY [u].[Species]
""");
    }

    public override async Task Can_include_animals(bool async)
    {
        await base.Can_include_animals(async);

        AssertSql(
            """
SELECT [c].[Id], [c].[Name], [u].[Id], [u].[CountryId], [u].[Name], [u].[Species], [u].[EagleId], [u].[IsFlightless], [u].[Group], [u].[FoundOn], [u].[Discriminator]
FROM [Countries] AS [c]
LEFT JOIN (
    SELECT [e].[Id], [e].[CountryId], [e].[Name], [e].[Species], [e].[EagleId], [e].[IsFlightless], [e].[Group], NULL AS [FoundOn], N'Eagle' AS [Discriminator]
    FROM [Eagle] AS [e]
    UNION ALL
    SELECT [k].[Id], [k].[CountryId], [k].[Name], [k].[Species], [k].[EagleId], [k].[IsFlightless], NULL AS [Group], [k].[FoundOn], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [u] ON [c].[Id] = [u].[CountryId]
ORDER BY [c].[Name], [c].[Id]
""");
    }

    public override async Task Can_include_prey(bool async)
    {
        await base.Can_include_prey(async);

        AssertSql(
            """
SELECT [e1].[Id], [e1].[CountryId], [e1].[Name], [e1].[Species], [e1].[EagleId], [e1].[IsFlightless], [e1].[Group], [u].[Id], [u].[CountryId], [u].[Name], [u].[Species], [u].[EagleId], [u].[IsFlightless], [u].[Group], [u].[FoundOn], [u].[Discriminator]
FROM (
    SELECT TOP(2) [e].[Id], [e].[CountryId], [e].[Name], [e].[Species], [e].[EagleId], [e].[IsFlightless], [e].[Group]
    FROM [Eagle] AS [e]
) AS [e1]
LEFT JOIN (
    SELECT [e0].[Id], [e0].[CountryId], [e0].[Name], [e0].[Species], [e0].[EagleId], [e0].[IsFlightless], [e0].[Group], NULL AS [FoundOn], N'Eagle' AS [Discriminator]
    FROM [Eagle] AS [e0]
    UNION ALL
    SELECT [k].[Id], [k].[CountryId], [k].[Name], [k].[Species], [k].[EagleId], [k].[IsFlightless], NULL AS [Group], [k].[FoundOn], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [u] ON [e1].[Id] = [u].[EagleId]
ORDER BY [e1].[Id]
""");
    }

    public override Task Can_insert_update_delete()
        => base.Can_insert_update_delete();

    public override async Task Can_query_all_animals(bool async)
    {
        await base.Can_query_all_animals(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[CountryId], [u].[Name], [u].[Species], [u].[EagleId], [u].[IsFlightless], [u].[Group], [u].[FoundOn], [u].[Discriminator]
FROM (
    SELECT [e].[Id], [e].[CountryId], [e].[Name], [e].[Species], [e].[EagleId], [e].[IsFlightless], [e].[Group], NULL AS [FoundOn], N'Eagle' AS [Discriminator]
    FROM [Eagle] AS [e]
    UNION ALL
    SELECT [k].[Id], [k].[CountryId], [k].[Name], [k].[Species], [k].[EagleId], [k].[IsFlightless], NULL AS [Group], [k].[FoundOn], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [u]
ORDER BY [u].[Species]
""");
    }

    public override async Task Can_query_all_birds(bool async)
    {
        await base.Can_query_all_birds(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[CountryId], [u].[Name], [u].[Species], [u].[EagleId], [u].[IsFlightless], [u].[Group], [u].[FoundOn], [u].[Discriminator]
FROM (
    SELECT [e].[Id], [e].[CountryId], [e].[Name], [e].[Species], [e].[EagleId], [e].[IsFlightless], [e].[Group], NULL AS [FoundOn], N'Eagle' AS [Discriminator]
    FROM [Eagle] AS [e]
    UNION ALL
    SELECT [k].[Id], [k].[CountryId], [k].[Name], [k].[Species], [k].[EagleId], [k].[IsFlightless], NULL AS [Group], [k].[FoundOn], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [u]
ORDER BY [u].[Species]
""");
    }

    public override async Task Can_query_all_plants(bool async)
    {
        await base.Can_query_all_plants(async);

        AssertSql(
            """
SELECT [u].[Species], [u].[CountryId], [u].[Genus], [u].[Name], [u].[HasThorns], [u].[Discriminator]
FROM (
    SELECT [d].[Species], [d].[CountryId], [d].[Genus], [d].[Name], NULL AS [HasThorns], N'Daisy' AS [Discriminator]
    FROM [Daisies] AS [d]
    UNION ALL
    SELECT [r].[Species], [r].[CountryId], [r].[Genus], [r].[Name], [r].[HasThorns], N'Rose' AS [Discriminator]
    FROM [Roses] AS [r]
) AS [u]
ORDER BY [u].[Species]
""");
    }

    public override async Task Can_query_all_types_when_shared_column(bool async)
    {
        await base.Can_query_all_types_when_shared_column(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[SortIndex], [u].[CaffeineGrams], [u].[CokeCO2], [u].[SugarGrams], [u].[LiltCO2], [u].[SugarGrams1], [u].[CaffeineGrams1], [u].[HasMilk], [u].[ComplexTypeCollection], [u].[ParentComplexType_Int], [u].[ParentComplexType_UniqueInt], [u].[ParentComplexType_Nested_NestedInt], [u].[ParentComplexType_Nested_UniqueInt], [u].[ChildComplexType_Int], [u].[ChildComplexType_UniqueInt], [u].[ChildComplexType_Nested_NestedInt], [u].[ChildComplexType_Nested_UniqueInt], [u].[ChildComplexType_Int1], [u].[ChildComplexType_UniqueInt1], [u].[ChildComplexType_Nested_NestedInt1], [u].[ChildComplexType_Nested_UniqueInt1], [u].[Discriminator]
FROM (
    SELECT [d].[Id], [d].[SortIndex], [d].[ComplexTypeCollection], [d].[ParentComplexType_Int], [d].[ParentComplexType_UniqueInt], [d].[ParentComplexType_Nested_NestedInt], [d].[ParentComplexType_Nested_UniqueInt], NULL AS [CaffeineGrams], NULL AS [CokeCO2], NULL AS [SugarGrams], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueInt], NULL AS [ChildComplexType_Nested_NestedInt], NULL AS [ChildComplexType_Nested_UniqueInt], NULL AS [LiltCO2], NULL AS [SugarGrams1], NULL AS [CaffeineGrams1], NULL AS [HasMilk], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueInt1], NULL AS [ChildComplexType_Nested_NestedInt1], NULL AS [ChildComplexType_Nested_UniqueInt1], N'Drink' AS [Discriminator]
    FROM [Drinks] AS [d]
    UNION ALL
    SELECT [c].[Id], [c].[SortIndex], [c].[ComplexTypeCollection], [c].[Int] AS [ParentComplexType_Int], [c].[UniqueInt] AS [ParentComplexType_UniqueInt], [c].[NestedInt] AS [ParentComplexType_Nested_NestedInt], [c].[NestedComplexType_UniqueInt] AS [ParentComplexType_Nested_UniqueInt], [c].[CaffeineGrams], [c].[CokeCO2], [c].[SugarGrams], [c].[ChildComplexType_Int], [c].[ChildComplexType_UniqueInt], [c].[ChildComplexType_Nested_NestedInt], [c].[ChildComplexType_Nested_UniqueInt], NULL AS [LiltCO2], NULL AS [SugarGrams1], NULL AS [CaffeineGrams1], NULL AS [HasMilk], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueInt1], NULL AS [ChildComplexType_Nested_NestedInt1], NULL AS [ChildComplexType_Nested_UniqueInt1], N'Coke' AS [Discriminator]
    FROM [Coke] AS [c]
    UNION ALL
    SELECT [l].[Id], [l].[SortIndex], [l].[ComplexTypeCollection], [l].[Int] AS [ParentComplexType_Int], [l].[UniqueInt] AS [ParentComplexType_UniqueInt], [l].[NestedInt] AS [ParentComplexType_Nested_NestedInt], [l].[NestedComplexType_UniqueInt] AS [ParentComplexType_Nested_UniqueInt], NULL AS [CaffeineGrams], NULL AS [CokeCO2], NULL AS [SugarGrams], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueInt], NULL AS [ChildComplexType_Nested_NestedInt], NULL AS [ChildComplexType_Nested_UniqueInt], [l].[LiltCO2], [l].[SugarGrams] AS [SugarGrams1], NULL AS [CaffeineGrams1], NULL AS [HasMilk], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueInt1], NULL AS [ChildComplexType_Nested_NestedInt1], NULL AS [ChildComplexType_Nested_UniqueInt1], N'Lilt' AS [Discriminator]
    FROM [Lilt] AS [l]
    UNION ALL
    SELECT [t].[Id], [t].[SortIndex], [t].[ComplexTypeCollection], [t].[Int] AS [ParentComplexType_Int], [t].[UniqueInt] AS [ParentComplexType_UniqueInt], [t].[NestedInt] AS [ParentComplexType_Nested_NestedInt], [t].[NestedComplexType_UniqueInt] AS [ParentComplexType_Nested_UniqueInt], NULL AS [CaffeineGrams], NULL AS [CokeCO2], NULL AS [SugarGrams], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueInt], NULL AS [ChildComplexType_Nested_NestedInt], NULL AS [ChildComplexType_Nested_UniqueInt], NULL AS [LiltCO2], NULL AS [SugarGrams1], [t].[CaffeineGrams] AS [CaffeineGrams1], [t].[HasMilk], [t].[ChildComplexType_Int] AS [ChildComplexType_Int1], [t].[ChildComplexType_UniqueInt] AS [ChildComplexType_UniqueInt1], [t].[ChildComplexType_Nested_NestedInt] AS [ChildComplexType_Nested_NestedInt1], [t].[ChildComplexType_Nested_UniqueInt] AS [ChildComplexType_Nested_UniqueInt1], N'Tea' AS [Discriminator]
    FROM [Tea] AS [t]
) AS [u]
""");
    }

    public override async Task Can_query_just_kiwis(bool async)
    {
        await base.Can_query_just_kiwis(async);

        AssertSql(
            """
SELECT TOP(2) [k].[Id], [k].[CountryId], [k].[Name], [k].[Species], [k].[EagleId], [k].[IsFlightless], [k].[FoundOn]
FROM [Kiwi] AS [k]
""");
    }

    public override async Task Can_query_just_roses(bool async)
    {
        await base.Can_query_just_roses(async);

        AssertSql(
            """
SELECT TOP(2) [r].[Species], [r].[CountryId], [r].[Genus], [r].[Name], [r].[HasThorns]
FROM [Roses] AS [r]
""");
    }

    public override async Task Can_query_when_shared_column(bool async)
    {
        await base.Can_query_when_shared_column(async);

        AssertSql(
            """
SELECT TOP(2) [c].[Id], [c].[SortIndex], [c].[CaffeineGrams], [c].[CokeCO2], [c].[SugarGrams], [c].[ComplexTypeCollection], [c].[Int], [c].[UniqueInt], [c].[NestedInt], [c].[NestedComplexType_UniqueInt], [c].[ChildComplexType_Int], [c].[ChildComplexType_UniqueInt], [c].[ChildComplexType_Nested_NestedInt], [c].[ChildComplexType_Nested_UniqueInt]
FROM [Coke] AS [c]
""",
            //
            """
SELECT TOP(2) [l].[Id], [l].[SortIndex], [l].[LiltCO2], [l].[SugarGrams], [l].[ComplexTypeCollection], [l].[Int], [l].[UniqueInt], [l].[NestedInt], [l].[NestedComplexType_UniqueInt]
FROM [Lilt] AS [l]
""",
            //
            """
SELECT TOP(2) [t].[Id], [t].[SortIndex], [t].[CaffeineGrams], [t].[HasMilk], [t].[ComplexTypeCollection], [t].[Int], [t].[UniqueInt], [t].[NestedInt], [t].[NestedComplexType_UniqueInt], [t].[ChildComplexType_Int], [t].[ChildComplexType_UniqueInt], [t].[ChildComplexType_Nested_NestedInt], [t].[ChildComplexType_Nested_UniqueInt]
FROM [Tea] AS [t]
""");
    }

    public override async Task Can_use_backwards_is_animal(bool async)
    {
        await base.Can_use_backwards_is_animal(async);

        AssertSql(
            """
SELECT [k].[Id], [k].[CountryId], [k].[Name], [k].[Species], [k].[EagleId], [k].[IsFlightless], [k].[FoundOn]
FROM [Kiwi] AS [k]
""");
    }

    public override async Task Can_use_backwards_of_type_animal(bool async)
    {
        await base.Can_use_backwards_of_type_animal(async);

        AssertSql(
            """
SELECT [k].[Id], [k].[CountryId], [k].[Name], [k].[Species], [k].[EagleId], [k].[IsFlightless], [k].[FoundOn]
FROM [Kiwi] AS [k]
""");
    }

    public override async Task Can_use_is_kiwi(bool async)
    {
        await base.Can_use_is_kiwi(async);

        AssertSql(
            """
SELECT [k].[Id], [k].[CountryId], [k].[Name], [k].[Species], [k].[EagleId], [k].[IsFlightless], NULL AS [Group], [k].[FoundOn], N'Kiwi' AS [Discriminator]
FROM [Kiwi] AS [k]
""");
    }

    public override async Task Can_use_is_kiwi_with_cast(bool async)
    {
        await base.Can_use_is_kiwi_with_cast(async);

        AssertSql(
            """
SELECT CASE
    WHEN [u].[Discriminator] = N'Kiwi' THEN [u].[FoundOn]
    ELSE CAST(0 AS tinyint)
END AS [Value]
FROM (
    SELECT NULL AS [FoundOn], N'Eagle' AS [Discriminator]
    FROM [Eagle] AS [e]
    UNION ALL
    SELECT [k].[FoundOn], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [u]
""");
    }

    public override async Task Can_use_is_kiwi_in_projection(bool async)
    {
        await base.Can_use_is_kiwi_in_projection(async);

        AssertSql(
            """
SELECT CASE
    WHEN [u].[Discriminator] = N'Kiwi' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM (
    SELECT N'Eagle' AS [Discriminator]
    FROM [Eagle] AS [e]
    UNION ALL
    SELECT N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [u]
""");
    }

    public override async Task Can_use_is_kiwi_with_other_predicate(bool async)
    {
        await base.Can_use_is_kiwi_with_other_predicate(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[CountryId], [u].[Name], [u].[Species], [u].[EagleId], [u].[IsFlightless], [u].[Group], [u].[FoundOn], [u].[Discriminator]
FROM (
    SELECT [e].[Id], [e].[CountryId], [e].[Name], [e].[Species], [e].[EagleId], [e].[IsFlightless], [e].[Group], NULL AS [FoundOn], N'Eagle' AS [Discriminator]
    FROM [Eagle] AS [e]
    UNION ALL
    SELECT [k].[Id], [k].[CountryId], [k].[Name], [k].[Species], [k].[EagleId], [k].[IsFlightless], NULL AS [Group], [k].[FoundOn], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [u]
WHERE [u].[Discriminator] = N'Kiwi' AND [u].[CountryId] = 1
""");
    }

    public override async Task Can_use_of_type_animal(bool async)
    {
        await base.Can_use_of_type_animal(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[CountryId], [u].[Name], [u].[Species], [u].[EagleId], [u].[IsFlightless], [u].[Group], [u].[FoundOn], [u].[Discriminator]
FROM (
    SELECT [e].[Id], [e].[CountryId], [e].[Name], [e].[Species], [e].[EagleId], [e].[IsFlightless], [e].[Group], NULL AS [FoundOn], N'Eagle' AS [Discriminator]
    FROM [Eagle] AS [e]
    UNION ALL
    SELECT [k].[Id], [k].[CountryId], [k].[Name], [k].[Species], [k].[EagleId], [k].[IsFlightless], NULL AS [Group], [k].[FoundOn], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [u]
ORDER BY [u].[Species]
""");
    }

    public override async Task Can_use_of_type_bird(bool async)
    {
        await base.Can_use_of_type_bird(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[CountryId], [u].[Name], [u].[Species], [u].[EagleId], [u].[IsFlightless], [u].[Group], [u].[FoundOn], [u].[Discriminator]
FROM (
    SELECT [e].[Id], [e].[CountryId], [e].[Name], [e].[Species], [e].[EagleId], [e].[IsFlightless], [e].[Group], NULL AS [FoundOn], N'Eagle' AS [Discriminator]
    FROM [Eagle] AS [e]
    UNION ALL
    SELECT [k].[Id], [k].[CountryId], [k].[Name], [k].[Species], [k].[EagleId], [k].[IsFlightless], NULL AS [Group], [k].[FoundOn], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [u]
ORDER BY [u].[Species]
""");
    }

    public override async Task Can_use_of_type_bird_first(bool async)
    {
        await base.Can_use_of_type_bird_first(async);

        AssertSql(
            """
SELECT TOP(1) [u].[Id], [u].[CountryId], [u].[Name], [u].[Species], [u].[EagleId], [u].[IsFlightless], [u].[Group], [u].[FoundOn], [u].[Discriminator]
FROM (
    SELECT [e].[Id], [e].[CountryId], [e].[Name], [e].[Species], [e].[EagleId], [e].[IsFlightless], [e].[Group], NULL AS [FoundOn], N'Eagle' AS [Discriminator]
    FROM [Eagle] AS [e]
    UNION ALL
    SELECT [k].[Id], [k].[CountryId], [k].[Name], [k].[Species], [k].[EagleId], [k].[IsFlightless], NULL AS [Group], [k].[FoundOn], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [u]
ORDER BY [u].[Species]
""");
    }

    public override async Task Can_use_of_type_bird_predicate(bool async)
    {
        await base.Can_use_of_type_bird_predicate(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[CountryId], [u].[Name], [u].[Species], [u].[EagleId], [u].[IsFlightless], [u].[Group], [u].[FoundOn], [u].[Discriminator]
FROM (
    SELECT [e].[Id], [e].[CountryId], [e].[Name], [e].[Species], [e].[EagleId], [e].[IsFlightless], [e].[Group], NULL AS [FoundOn], N'Eagle' AS [Discriminator]
    FROM [Eagle] AS [e]
    UNION ALL
    SELECT [k].[Id], [k].[CountryId], [k].[Name], [k].[Species], [k].[EagleId], [k].[IsFlightless], NULL AS [Group], [k].[FoundOn], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [u]
WHERE [u].[CountryId] = 1
ORDER BY [u].[Species]
""");
    }

    public override async Task Can_use_of_type_bird_with_projection(bool async)
    {
        await base.Can_use_of_type_bird_with_projection(async);

        AssertSql(
            """
SELECT [e].[EagleId]
FROM [Eagle] AS [e]
UNION ALL
SELECT [k].[EagleId]
FROM [Kiwi] AS [k]
""");
    }

    public override async Task Can_use_of_type_kiwi(bool async)
    {
        await base.Can_use_of_type_kiwi(async);

        AssertSql(
            """
SELECT [k].[Id], [k].[CountryId], [k].[Name], [k].[Species], [k].[EagleId], [k].[IsFlightless], [k].[FoundOn], N'Kiwi' AS [Discriminator]
FROM [Kiwi] AS [k]
""");
    }

    public override async Task Can_use_of_type_kiwi_where_north_on_derived_property(bool async)
    {
        await base.Can_use_of_type_kiwi_where_north_on_derived_property(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[CountryId], [u].[Name], [u].[Species], [u].[EagleId], [u].[IsFlightless], [u].[FoundOn], [u].[Discriminator]
FROM (
    SELECT [k].[Id], [k].[CountryId], [k].[Name], [k].[Species], [k].[EagleId], [k].[IsFlightless], [k].[FoundOn], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [u]
WHERE [u].[FoundOn] = CAST(0 AS tinyint)
""");
    }

    public override async Task Can_use_of_type_kiwi_where_south_on_derived_property(bool async)
    {
        await base.Can_use_of_type_kiwi_where_south_on_derived_property(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[CountryId], [u].[Name], [u].[Species], [u].[EagleId], [u].[IsFlightless], [u].[FoundOn], [u].[Discriminator]
FROM (
    SELECT [k].[Id], [k].[CountryId], [k].[Name], [k].[Species], [k].[EagleId], [k].[IsFlightless], [k].[FoundOn], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [u]
WHERE [u].[FoundOn] = CAST(1 AS tinyint)
""");
    }

    public override async Task Can_use_of_type_rose(bool async)
    {
        await base.Can_use_of_type_rose(async);

        AssertSql(
            """
SELECT [r].[Species], [r].[CountryId], [r].[Genus], [r].[Name], [r].[HasThorns], N'Rose' AS [Discriminator]
FROM [Roses] AS [r]
""");
    }

    public override async Task Member_access_on_intermediate_type_works()
    {
        await base.Member_access_on_intermediate_type_works();

        AssertSql(
            """
SELECT [k].[Name]
FROM [Kiwi] AS [k]
ORDER BY [k].[Name]
""");
    }

    public override async Task OfType_Union_OfType(bool async)
    {
        await base.OfType_Union_OfType(async);

        AssertSql();
    }

    public override async Task OfType_Union_subquery(bool async)
    {
        await base.OfType_Union_subquery(async);

        AssertSql();
    }

    public override Task Setting_foreign_key_to_a_different_type_throws()
        => base.Setting_foreign_key_to_a_different_type_throws();

    public override async Task Subquery_OfType(bool async)
    {
        await base.Subquery_OfType(async);

        AssertSql(
            """
@p='5'

SELECT DISTINCT [u0].[Id], [u0].[CountryId], [u0].[Name], [u0].[Species], [u0].[EagleId], [u0].[IsFlightless], [u0].[FoundOn], [u0].[Discriminator]
FROM (
    SELECT TOP(@p) [u].[Id], [u].[CountryId], [u].[Name], [u].[Species], [u].[EagleId], [u].[IsFlightless], [u].[FoundOn], [u].[Discriminator]
    FROM (
        SELECT [e].[Id], [e].[CountryId], [e].[Name], [e].[Species], [e].[EagleId], [e].[IsFlightless], NULL AS [FoundOn], N'Eagle' AS [Discriminator]
        FROM [Eagle] AS [e]
        UNION ALL
        SELECT [k].[Id], [k].[CountryId], [k].[Name], [k].[Species], [k].[EagleId], [k].[IsFlightless], [k].[FoundOn], N'Kiwi' AS [Discriminator]
        FROM [Kiwi] AS [k]
    ) AS [u]
    ORDER BY [u].[Species]
) AS [u0]
WHERE [u0].[Discriminator] = N'Kiwi'
""");
    }

    public override async Task Union_entity_equality(bool async)
    {
        await base.Union_entity_equality(async);

        AssertSql();
    }

    public override async Task Union_siblings_with_duplicate_property_in_subquery(bool async)
    {
        await base.Union_siblings_with_duplicate_property_in_subquery(async);

        AssertSql();
    }

    public override async Task Is_operator_on_result_of_FirstOrDefault(bool async)
    {
        await base.Is_operator_on_result_of_FirstOrDefault(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[CountryId], [u].[Name], [u].[Species], [u].[EagleId], [u].[IsFlightless], [u].[Group], [u].[FoundOn], [u].[Discriminator]
FROM (
    SELECT [e].[Id], [e].[CountryId], [e].[Name], [e].[Species], [e].[EagleId], [e].[IsFlightless], [e].[Group], NULL AS [FoundOn], N'Eagle' AS [Discriminator]
    FROM [Eagle] AS [e]
    UNION ALL
    SELECT [k].[Id], [k].[CountryId], [k].[Name], [k].[Species], [k].[EagleId], [k].[IsFlightless], NULL AS [Group], [k].[FoundOn], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [u]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT TOP(1) [u0].[Discriminator]
        FROM (
            SELECT [e0].[Name], N'Eagle' AS [Discriminator]
            FROM [Eagle] AS [e0]
            UNION ALL
            SELECT [k0].[Name], N'Kiwi' AS [Discriminator]
            FROM [Kiwi] AS [k0]
        ) AS [u0]
        WHERE [u0].[Name] = N'Great spotted kiwi'
    ) AS [u1]
    WHERE [u1].[Discriminator] = N'Kiwi')
ORDER BY [u].[Species]
""");
    }

    public override async Task Selecting_only_base_properties_on_base_type(bool async)
    {
        await base.Selecting_only_base_properties_on_base_type(async);

        AssertSql(
            """
SELECT [e].[Name]
FROM [Eagle] AS [e]
UNION ALL
SELECT [k].[Name]
FROM [Kiwi] AS [k]
""");
    }

    public override async Task Selecting_only_base_properties_on_derived_type(bool async)
    {
        await base.Selecting_only_base_properties_on_derived_type(async);

        AssertSql(
            """
SELECT [e].[Name]
FROM [Eagle] AS [e]
UNION ALL
SELECT [k].[Name]
FROM [Kiwi] AS [k]
""");
    }

    public override async Task Can_query_all_animal_views(bool async)
    {
        await base.Can_query_all_animal_views(async);

        AssertSql();
    }

    public override async Task Discriminator_used_when_projection_over_derived_type(bool async)
    {
        await base.Discriminator_used_when_projection_over_derived_type(async);

        AssertSql();
    }

    public override async Task Discriminator_used_when_projection_over_derived_type2(bool async)
    {
        await base.Discriminator_used_when_projection_over_derived_type2(async);

        AssertSql();
    }

    public override async Task Discriminator_used_when_projection_over_of_type(bool async)
    {
        await base.Discriminator_used_when_projection_over_of_type(async);

        AssertSql();
    }

    public override async Task Discriminator_with_cast_in_shadow_property(bool async)
    {
        await base.Discriminator_with_cast_in_shadow_property(async);

        AssertSql();
    }

    public override void Using_from_sql_throws()
    {
        base.Using_from_sql_throws();

        AssertSql();
    }

    public override async Task Using_is_operator_on_multiple_type_with_no_result(bool async)
    {
        await base.Using_is_operator_on_multiple_type_with_no_result(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[CountryId], [u].[Name], [u].[Species], [u].[EagleId], [u].[IsFlightless], [u].[Group], [u].[FoundOn], [u].[Discriminator]
FROM (
    SELECT [k].[Id], [k].[CountryId], [k].[Name], [k].[Species], [k].[EagleId], [k].[IsFlightless], NULL AS [Group], [k].[FoundOn], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [u]
WHERE [u].[Discriminator] = N'Eagle'
""");
    }

    public override async Task Using_is_operator_with_of_type_on_multiple_type_with_no_result(bool async)
    {
        await base.Using_is_operator_with_of_type_on_multiple_type_with_no_result(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[CountryId], [u].[Name], [u].[Species], [u].[EagleId], [u].[IsFlightless], [u].[Group], [u].[Discriminator]
FROM (
    SELECT [k].[Id], [k].[CountryId], [k].[Name], [k].[Species], [k].[EagleId], [k].[IsFlightless], NULL AS [Group], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [u]
WHERE [u].[Discriminator] = N'Eagle'
""");
    }

    public override async Task Using_OfType_on_multiple_type_with_no_result(bool async)
    {
        await base.Using_OfType_on_multiple_type_with_no_result(async);

        AssertSql();
    }

    public override async Task GetType_in_hierarchy_in_abstract_base_type(bool async)
    {
        await base.GetType_in_hierarchy_in_abstract_base_type(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[CountryId], [u].[Name], [u].[Species], [u].[EagleId], [u].[IsFlightless], [u].[Group], [u].[FoundOn], [u].[Discriminator]
FROM (
    SELECT [e].[Id], [e].[CountryId], [e].[Name], [e].[Species], [e].[EagleId], [e].[IsFlightless], [e].[Group], NULL AS [FoundOn], N'Eagle' AS [Discriminator]
    FROM [Eagle] AS [e]
    UNION ALL
    SELECT [k].[Id], [k].[CountryId], [k].[Name], [k].[Species], [k].[EagleId], [k].[IsFlightless], NULL AS [Group], [k].[FoundOn], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [u]
WHERE 0 = 1
""");
    }

    public override async Task GetType_in_hierarchy_in_intermediate_type(bool async)
    {
        await base.GetType_in_hierarchy_in_intermediate_type(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[CountryId], [u].[Name], [u].[Species], [u].[EagleId], [u].[IsFlightless], [u].[Group], [u].[FoundOn], [u].[Discriminator]
FROM (
    SELECT [e].[Id], [e].[CountryId], [e].[Name], [e].[Species], [e].[EagleId], [e].[IsFlightless], [e].[Group], NULL AS [FoundOn], N'Eagle' AS [Discriminator]
    FROM [Eagle] AS [e]
    UNION ALL
    SELECT [k].[Id], [k].[CountryId], [k].[Name], [k].[Species], [k].[EagleId], [k].[IsFlightless], NULL AS [Group], [k].[FoundOn], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [u]
WHERE 0 = 1
""");
    }

    public override async Task GetType_in_hierarchy_in_leaf_type_with_sibling(bool async)
    {
        await base.GetType_in_hierarchy_in_leaf_type_with_sibling(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[CountryId], [e].[Name], [e].[Species], [e].[EagleId], [e].[IsFlightless], [e].[Group], NULL AS [FoundOn], N'Eagle' AS [Discriminator]
FROM [Eagle] AS [e]
""");
    }

    public override async Task GetType_in_hierarchy_in_leaf_type_with_sibling2(bool async)
    {
        await base.GetType_in_hierarchy_in_leaf_type_with_sibling2(async);

        AssertSql(
            """
SELECT [k].[Id], [k].[CountryId], [k].[Name], [k].[Species], [k].[EagleId], [k].[IsFlightless], NULL AS [Group], [k].[FoundOn], N'Kiwi' AS [Discriminator]
FROM [Kiwi] AS [k]
""");
    }

    public override async Task GetType_in_hierarchy_in_leaf_type_with_sibling2_reverse(bool async)
    {
        await base.GetType_in_hierarchy_in_leaf_type_with_sibling2_reverse(async);

        AssertSql(
            """
SELECT [k].[Id], [k].[CountryId], [k].[Name], [k].[Species], [k].[EagleId], [k].[IsFlightless], NULL AS [Group], [k].[FoundOn], N'Kiwi' AS [Discriminator]
FROM [Kiwi] AS [k]
""");
    }

    public override async Task GetType_in_hierarchy_in_leaf_type_with_sibling2_not_equal(bool async)
    {
        await base.GetType_in_hierarchy_in_leaf_type_with_sibling2_not_equal(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[CountryId], [u].[Name], [u].[Species], [u].[EagleId], [u].[IsFlightless], [u].[Group], [u].[FoundOn], [u].[Discriminator]
FROM (
    SELECT [e].[Id], [e].[CountryId], [e].[Name], [e].[Species], [e].[EagleId], [e].[IsFlightless], [e].[Group], NULL AS [FoundOn], N'Eagle' AS [Discriminator]
    FROM [Eagle] AS [e]
    UNION ALL
    SELECT [k].[Id], [k].[CountryId], [k].[Name], [k].[Species], [k].[EagleId], [k].[IsFlightless], NULL AS [Group], [k].[FoundOn], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [u]
WHERE [u].[Discriminator] <> N'Kiwi'
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
