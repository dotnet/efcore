// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class IncompleteMappingInheritanceQuerySqlServerTest(
    IncompleteMappingInheritanceQuerySqlServerFixture fixture,
    ITestOutputHelper testOutputHelper)
    : TPHInheritanceQueryTestBase<IncompleteMappingInheritanceQuerySqlServerFixture>(fixture, testOutputHelper)
{
    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    [ConditionalFact]
    public virtual void Common_property_shares_column()
    {
        using var context = CreateContext();
        var liltType = context.Model.FindEntityType(typeof(Lilt));
        var cokeType = context.Model.FindEntityType(typeof(Coke));
        var teaType = context.Model.FindEntityType(typeof(Tea));

        Assert.Equal("SugarGrams", cokeType.FindProperty("SugarGrams").GetColumnName());
        Assert.Equal("CaffeineGrams", cokeType.FindProperty("CaffeineGrams").GetColumnName());
        Assert.Equal("CokeCO2", cokeType.FindProperty("Carbonation").GetColumnName());

        Assert.Equal("SugarGrams", liltType.FindProperty("SugarGrams").GetColumnName());
        Assert.Equal("LiltCO2", liltType.FindProperty("Carbonation").GetColumnName());

        Assert.Equal("CaffeineGrams", teaType.FindProperty("CaffeineGrams").GetColumnName());
        Assert.Equal("HasMilk", teaType.FindProperty("HasMilk").GetColumnName());
    }

    public override async Task Can_query_when_shared_column(bool async)
    {
        await base.Can_query_when_shared_column(async);

        AssertSql(
            """
SELECT TOP(2) [d].[Id], [d].[Discriminator], [d].[SortIndex], [d].[CaffeineGrams], [d].[CokeCO2], [d].[SugarGrams]
FROM [Drinks] AS [d]
WHERE [d].[Discriminator] = 1
""",
            //
            """
SELECT TOP(2) [d].[Id], [d].[Discriminator], [d].[SortIndex], [d].[LiltCO2], [d].[SugarGrams]
FROM [Drinks] AS [d]
WHERE [d].[Discriminator] = 2
""",
            //
            """
SELECT TOP(2) [d].[Id], [d].[Discriminator], [d].[SortIndex], [d].[CaffeineGrams], [d].[HasMilk]
FROM [Drinks] AS [d]
WHERE [d].[Discriminator] = 3
""");
    }

    public override void FromSql_on_root()
    {
        base.FromSql_on_root();

        AssertSql(
            """
SELECT [m].[Id], [m].[CountryId], [m].[Discriminator], [m].[Name], [m].[Species], [m].[EagleId], [m].[IsFlightless], [m].[Group], [m].[FoundOn]
FROM (
    select * from "Animals"
) AS [m]
WHERE [m].[Discriminator] IN (N'Eagle', N'Kiwi')
""");
    }

    public override void FromSql_on_derived()
    {
        base.FromSql_on_derived();

        AssertSql(
            """
SELECT [m].[Id], [m].[CountryId], [m].[Discriminator], [m].[Name], [m].[Species], [m].[EagleId], [m].[IsFlightless], [m].[Group]
FROM (
    select * from "Animals"
) AS [m]
WHERE [m].[Discriminator] = N'Eagle'
""");
    }

    public override async Task Can_query_all_types_when_shared_column(bool async)
    {
        await base.Can_query_all_types_when_shared_column(async);

        AssertSql(
            """
SELECT [d].[Id], [d].[Discriminator], [d].[SortIndex], [d].[CaffeineGrams], [d].[CokeCO2], [d].[SugarGrams], [d].[LiltCO2], [d].[HasMilk]
FROM [Drinks] AS [d]
WHERE [d].[Discriminator] IN (0, 1, 2, 3)
""");
    }

    public override async Task Can_use_of_type_animal(bool async)
    {
        await base.Can_use_of_type_animal(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animals] AS [a]
WHERE [a].[Discriminator] IN (N'Eagle', N'Kiwi')
ORDER BY [a].[Species]
""");
    }

    public override async Task Can_use_is_kiwi(bool async)
    {
        await base.Can_use_is_kiwi(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animals] AS [a]
WHERE [a].[Discriminator] = N'Kiwi'
""");
    }

    public override async Task Can_use_is_kiwi_with_cast(bool async)
    {
        await base.Can_use_is_kiwi_with_cast(async);

        AssertSql(
            """
SELECT CASE
    WHEN [a].[Discriminator] = N'Kiwi' THEN [a].[FoundOn]
    ELSE CAST(0 AS tinyint)
END AS [Value]
FROM [Animals] AS [a]
WHERE [a].[Discriminator] IN (N'Eagle', N'Kiwi')
""");
    }

    public override async Task Can_use_is_kiwi_with_other_predicate(bool async)
    {
        await base.Can_use_is_kiwi_with_other_predicate(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animals] AS [a]
WHERE [a].[Discriminator] IN (N'Eagle', N'Kiwi') AND [a].[Discriminator] = N'Kiwi' AND [a].[CountryId] = 1
""");
    }

    public override async Task Can_use_is_kiwi_in_projection(bool async)
    {
        await base.Can_use_is_kiwi_in_projection(async);

        AssertSql(
            """
SELECT CASE
    WHEN [a].[Discriminator] = N'Kiwi' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Animals] AS [a]
WHERE [a].[Discriminator] IN (N'Eagle', N'Kiwi')
""");
    }

    public override async Task Can_use_of_type_bird(bool async)
    {
        await base.Can_use_of_type_bird(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animals] AS [a]
WHERE [a].[Discriminator] IN (N'Eagle', N'Kiwi')
ORDER BY [a].[Species]
""");
    }

    public override async Task Can_use_of_type_bird_predicate(bool async)
    {
        await base.Can_use_of_type_bird_predicate(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animals] AS [a]
WHERE [a].[Discriminator] IN (N'Eagle', N'Kiwi') AND [a].[CountryId] = 1 AND [a].[Discriminator] IN (N'Eagle', N'Kiwi')
ORDER BY [a].[Species]
""");
    }

    public override async Task Can_use_of_type_bird_with_projection(bool async)
    {
        await base.Can_use_of_type_bird_with_projection(async);

        AssertSql(
            """
SELECT [a].[EagleId]
FROM [Animals] AS [a]
WHERE [a].[Discriminator] IN (N'Eagle', N'Kiwi')
""");
    }

    public override async Task Can_use_of_type_bird_first(bool async)
    {
        await base.Can_use_of_type_bird_first(async);

        AssertSql(
            """
SELECT TOP(1) [a].[Id], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animals] AS [a]
WHERE [a].[Discriminator] IN (N'Eagle', N'Kiwi')
ORDER BY [a].[Species]
""");
    }

    public override async Task Can_use_of_type_kiwi(bool async)
    {
        await base.Can_use_of_type_kiwi(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[FoundOn]
FROM [Animals] AS [a]
WHERE [a].[Discriminator] = N'Kiwi'
""");
    }

    public override async Task Can_use_of_type_rose(bool async)
    {
        await base.Can_use_of_type_rose(async);

        AssertSql(
            """
SELECT [p].[Species], [p].[CountryId], [p].[Genus], [p].[Name], [p].[HasThorns]
FROM [Plants] AS [p]
WHERE [p].[Genus] = 0
""");
    }

    public override async Task Can_query_all_animals(bool async)
    {
        await base.Can_query_all_animals(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animals] AS [a]
WHERE [a].[Discriminator] IN (N'Eagle', N'Kiwi')
ORDER BY [a].[Species]
""");
    }

    public override async Task Can_query_all_animal_views(bool async)
    {
        await base.Can_query_all_animal_views(async);

        AssertSql(
            """
SELECT [m].[CountryId], [m].[Discriminator], [m].[Name], [m].[EagleId], [m].[IsFlightless], [m].[Group], [m].[FoundOn]
FROM (
    SELECT * FROM Animals
) AS [m]
ORDER BY [m].[CountryId]
""");
    }

    public override async Task Can_query_all_plants(bool async)
    {
        await base.Can_query_all_plants(async);

        AssertSql(
            """
SELECT [p].[Species], [p].[CountryId], [p].[Genus], [p].[Name], [p].[HasThorns]
FROM [Plants] AS [p]
WHERE [p].[Genus] IN (1, 0)
ORDER BY [p].[Species]
""");
    }

    public override async Task Filter_on_property_inside_complex_type_on_derived_type(bool async)
    {
        await base.Filter_on_property_inside_complex_type_on_derived_type(async);

        AssertSql();
    }

    public override async Task Can_filter_all_animals(bool async)
    {
        await base.Can_filter_all_animals(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animals] AS [a]
WHERE [a].[Discriminator] IN (N'Eagle', N'Kiwi') AND [a].[Name] = N'Great spotted kiwi'
ORDER BY [a].[Species]
""");
    }

    public override async Task Can_query_all_birds(bool async)
    {
        await base.Can_query_all_birds(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animals] AS [a]
WHERE [a].[Discriminator] IN (N'Eagle', N'Kiwi')
ORDER BY [a].[Species]
""");
    }

    public override async Task Can_query_just_kiwis(bool async)
    {
        await base.Can_query_just_kiwis(async);

        AssertSql(
            """
SELECT TOP(2) [a].[Id], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[FoundOn]
FROM [Animals] AS [a]
WHERE [a].[Discriminator] = N'Kiwi'
""");
    }

    public override async Task Can_query_just_roses(bool async)
    {
        await base.Can_query_just_roses(async);

        AssertSql(
            """
SELECT TOP(2) [p].[Species], [p].[CountryId], [p].[Genus], [p].[Name], [p].[HasThorns]
FROM [Plants] AS [p]
WHERE [p].[Genus] = 0
""");
    }

    public override async Task Can_include_prey(bool async)
    {
        await base.Can_include_prey(async);

        AssertSql(
            """
SELECT [a1].[Id], [a1].[CountryId], [a1].[Discriminator], [a1].[Name], [a1].[Species], [a1].[EagleId], [a1].[IsFlightless], [a1].[Group], [a2].[Id], [a2].[CountryId], [a2].[Discriminator], [a2].[Name], [a2].[Species], [a2].[EagleId], [a2].[IsFlightless], [a2].[Group], [a2].[FoundOn]
FROM (
    SELECT TOP(2) [a].[Id], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[Group]
    FROM [Animals] AS [a]
    WHERE [a].[Discriminator] = N'Eagle'
) AS [a1]
LEFT JOIN (
    SELECT [a0].[Id], [a0].[CountryId], [a0].[Discriminator], [a0].[Name], [a0].[Species], [a0].[EagleId], [a0].[IsFlightless], [a0].[Group], [a0].[FoundOn]
    FROM [Animals] AS [a0]
    WHERE [a0].[Discriminator] IN (N'Eagle', N'Kiwi')
) AS [a2] ON [a1].[Id] = [a2].[EagleId]
ORDER BY [a1].[Id]
""");
    }

    public override async Task Can_include_animals(bool async)
    {
        await base.Can_include_animals(async);

        AssertSql(
            """
SELECT [c].[Id], [c].[Name], [a0].[Id], [a0].[CountryId], [a0].[Discriminator], [a0].[Name], [a0].[Species], [a0].[EagleId], [a0].[IsFlightless], [a0].[Group], [a0].[FoundOn]
FROM [Countries] AS [c]
LEFT JOIN (
    SELECT [a].[Id], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
    FROM [Animals] AS [a]
    WHERE [a].[Discriminator] IN (N'Eagle', N'Kiwi')
) AS [a0] ON [c].[Id] = [a0].[CountryId]
ORDER BY [c].[Name], [c].[Id]
""");
    }

    public override async Task Can_use_of_type_kiwi_where_north_on_derived_property(bool async)
    {
        await base.Can_use_of_type_kiwi_where_north_on_derived_property(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[FoundOn]
FROM [Animals] AS [a]
WHERE [a].[Discriminator] = N'Kiwi' AND [a].[FoundOn] = CAST(0 AS tinyint)
""");
    }

    public override async Task Can_use_of_type_kiwi_where_south_on_derived_property(bool async)
    {
        await base.Can_use_of_type_kiwi_where_south_on_derived_property(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[FoundOn]
FROM [Animals] AS [a]
WHERE [a].[Discriminator] = N'Kiwi' AND [a].[FoundOn] = CAST(1 AS tinyint)
""");
    }

    public override async Task Discriminator_used_when_projection_over_derived_type(bool async)
    {
        await base.Discriminator_used_when_projection_over_derived_type(async);

        AssertSql(
            """
SELECT [a].[FoundOn]
FROM [Animals] AS [a]
WHERE [a].[Discriminator] = N'Kiwi'
""");
    }

    public override async Task Discriminator_used_when_projection_over_derived_type2(bool async)
    {
        await base.Discriminator_used_when_projection_over_derived_type2(async);

        AssertSql(
            """
SELECT [a].[IsFlightless], [a].[Discriminator]
FROM [Animals] AS [a]
WHERE [a].[Discriminator] IN (N'Eagle', N'Kiwi')
""");
    }

    public override async Task Discriminator_used_when_projection_over_of_type(bool async)
    {
        await base.Discriminator_used_when_projection_over_of_type(async);

        AssertSql(
            """
SELECT [a].[FoundOn]
FROM [Animals] AS [a]
WHERE [a].[Discriminator] = N'Kiwi'
""");
    }

    public override Task Can_insert_update_delete()
        => base.Can_insert_update_delete();

    public override async Task Byte_enum_value_constant_used_in_projection(bool async)
    {
        await base.Byte_enum_value_constant_used_in_projection(async);

        AssertSql(
            """
SELECT CASE
    WHEN [a].[IsFlightless] = CAST(1 AS bit) THEN CAST(0 AS tinyint)
    ELSE CAST(1 AS tinyint)
END
FROM [Animals] AS [a]
WHERE [a].[Discriminator] = N'Kiwi'
""");
    }

    public override async Task Union_siblings_with_duplicate_property_in_subquery(bool async)
    {
        await base.Union_siblings_with_duplicate_property_in_subquery(async);

        AssertSql(
            """
SELECT [t].[Id], [t].[Discriminator], [t].[CaffeineGrams], [t].[CokeCO2], [t].[SugarGrams], [t].[Carbonation], [t].[SugarGrams0], [t].[CaffeineGrams0], [t].[HasMilk]
FROM (
    SELECT [d].[Id], [d].[Discriminator], [d].[CaffeineGrams], [d].[CokeCO2], [d].[SugarGrams], NULL AS [CaffeineGrams0], NULL AS [HasMilk], NULL AS [Carbonation], NULL AS [SugarGrams0]
    FROM [Drinks] AS [d]
    WHERE [d].[Discriminator] = N'Coke'
    UNION
    SELECT [d0].[Id], [d0].[Discriminator], NULL AS [CaffeineGrams], NULL AS [CokeCO2], NULL AS [SugarGrams], [d0].[CaffeineGrams] AS [CaffeineGrams0], [d0].[HasMilk], NULL AS [Carbonation], NULL AS [SugarGrams0]
    FROM [Drinks] AS [d0]
    WHERE [d0].[Discriminator] = N'Tea'
) AS [t]
WHERE [t].[Id] > 0
""");
    }

    public override async Task OfType_Union_subquery(bool async)
    {
        await base.OfType_Union_subquery(async);

        AssertSql(
            """
SELECT [t].[Species], [t].[CountryId], [t].[Discriminator], [t].[Name], [t].[EagleId], [t].[IsFlightless], [t].[FoundOn]
FROM (
    SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[FoundOn]
    FROM [Animals] AS [a]
    WHERE [a].[Discriminator] IN (N'Eagle', N'Kiwi') AND ([a].[Discriminator] = N'Kiwi')
    UNION
    SELECT [a0].[Species], [a0].[CountryId], [a0].[Discriminator], [a0].[Name], [a0].[EagleId], [a0].[IsFlightless], [a0].[FoundOn]
    FROM [Animals] AS [a0]
    WHERE [a0].[Discriminator] IN (N'Eagle', N'Kiwi') AND ([a0].[Discriminator] = N'Kiwi')
) AS [t]
WHERE ([t].[FoundOn] = CAST(0 AS tinyint)) AND [t].[FoundOn] IS NOT NULL
""");
    }

    public override async Task OfType_Union_OfType(bool async)
    {
        await base.OfType_Union_OfType(async);

        AssertSql(" ");
    }

    public override async Task Subquery_OfType(bool async)
    {
        await base.Subquery_OfType(async);

        AssertSql(
            """
@__p_0='5'

SELECT DISTINCT [a0].[Id], [a0].[CountryId], [a0].[Discriminator], [a0].[Name], [a0].[Species], [a0].[EagleId], [a0].[IsFlightless], [a0].[FoundOn]
FROM (
    SELECT TOP(@__p_0) [a].[Id], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[FoundOn]
    FROM [Animals] AS [a]
    WHERE [a].[Discriminator] IN (N'Eagle', N'Kiwi')
    ORDER BY [a].[Species]
) AS [a0]
WHERE [a0].[Discriminator] = N'Kiwi'
""");
    }

    public override async Task Union_entity_equality(bool async)
    {
        await base.Union_entity_equality(async);

        AssertSql(
            """
SELECT [t].[Species], [t].[CountryId], [t].[Discriminator], [t].[Name], [t].[EagleId], [t].[IsFlightless], [t].[Group], [t].[FoundOn]
FROM (
    SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[FoundOn], NULL AS [Group]
    FROM [Animals] AS [a]
    WHERE [a].[Discriminator] = N'Kiwi'
    UNION
    SELECT [a0].[Species], [a0].[CountryId], [a0].[Discriminator], [a0].[Name], [a0].[EagleId], [a0].[IsFlightless], NULL AS [FoundOn], [a0].[Group]
    FROM [Animals] AS [a0]
    WHERE [a0].[Discriminator] = N'Eagle'
) AS [t]
WHERE 0 = 1
""");
    }

    public override async Task Member_access_on_intermediate_type_works()
    {
        await base.Member_access_on_intermediate_type_works();

        AssertSql(
            """
SELECT [a].[Name]
FROM [Animals] AS [a]
WHERE [a].[Discriminator] = N'Kiwi'
ORDER BY [a].[Name]
""");
    }

    public override void Casting_to_base_type_joining_with_query_type_works()
    {
        base.Casting_to_base_type_joining_with_query_type_works();

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[Group], [m].[CountryId], [m].[Discriminator], [m].[Name], [m].[EagleId], [m].[IsFlightless], [m].[Group], [m].[FoundOn]
FROM [Animals] AS [a]
INNER JOIN (
    Select * from "Animals"
) AS [m] ON [a].[Name] = [m].[Name]
WHERE [a].[Discriminator] = N'Eagle'
""");
    }

    public override async Task Is_operator_on_result_of_FirstOrDefault(bool async)
    {
        await base.Is_operator_on_result_of_FirstOrDefault(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animals] AS [a]
WHERE [a].[Discriminator] IN (N'Eagle', N'Kiwi') AND (
    SELECT TOP(1) [a0].[Discriminator]
    FROM [Animals] AS [a0]
    WHERE [a0].[Discriminator] IN (N'Eagle', N'Kiwi') AND [a0].[Name] = N'Great spotted kiwi') = N'Kiwi'
ORDER BY [a].[Species]
""");
    }

    public override async Task Selecting_only_base_properties_on_base_type(bool async)
    {
        await base.Selecting_only_base_properties_on_base_type(async);

        AssertSql(
            """
SELECT [a].[Name]
FROM [Animals] AS [a]
WHERE [a].[Discriminator] IN (N'Eagle', N'Kiwi')
""");
    }

    public override async Task Selecting_only_base_properties_on_derived_type(bool async)
    {
        await base.Selecting_only_base_properties_on_derived_type(async);

        AssertSql(
            """
SELECT [a].[Name]
FROM [Animals] AS [a]
WHERE [a].[Discriminator] IN (N'Eagle', N'Kiwi')
""");
    }

    public override async Task Can_use_backwards_is_animal(bool async)
    {
        await base.Can_use_backwards_is_animal(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[FoundOn]
FROM [Animals] AS [a]
WHERE [a].[Discriminator] = N'Kiwi'
""");
    }

    public override async Task Can_use_backwards_of_type_animal(bool async)
    {
        await base.Can_use_backwards_of_type_animal(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[FoundOn]
FROM [Animals] AS [a]
WHERE [a].[Discriminator] = N'Kiwi'
""");
    }

    public override async Task Discriminator_with_cast_in_shadow_property(bool async)
    {
        await base.Discriminator_with_cast_in_shadow_property(async);

        AssertSql(
            """
SELECT [a].[Name] AS [Predator]
FROM [Animals] AS [a]
WHERE [a].[Discriminator] = N'Kiwi'
""");
    }

    public override async Task Setting_foreign_key_to_a_different_type_throws()
    {
        await base.Setting_foreign_key_to_a_different_type_throws();

        AssertSql(
            """
SELECT TOP(2) [a].[Id], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[FoundOn]
FROM [Animals] AS [a]
WHERE [a].[Discriminator] = N'Kiwi'
""",
            //
            """
@p0='0'
@p1='Eagle' (Nullable = false) (Size = 8)
@p2='2' (Nullable = true)
@p3='1' (Nullable = true)
@p4='False' (Nullable = true)
@p5='Bald eagle' (Size = 4000)
@p6='Haliaeetus leucocephalus' (Size = 100)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [Animals] ([CountryId], [Discriminator], [EagleId], [Group], [IsFlightless], [Name], [Species])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6);
""");
    }

    public override async Task Using_is_operator_on_multiple_type_with_no_result(bool async)
    {
        await base.Using_is_operator_on_multiple_type_with_no_result(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animals] AS [a]
WHERE 0 = 1
""");
    }

    public override async Task Using_is_operator_with_of_type_on_multiple_type_with_no_result(bool async)
    {
        await base.Using_is_operator_with_of_type_on_multiple_type_with_no_result(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[Group]
FROM [Animals] AS [a]
WHERE 0 = 1
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
SELECT [a].[Id], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animals] AS [a]
WHERE 0 = 1
""");
    }

    public override async Task GetType_in_hierarchy_in_intermediate_type(bool async)
    {
        await base.GetType_in_hierarchy_in_intermediate_type(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animals] AS [a]
WHERE 0 = 1
""");
    }

    public override async Task GetType_in_hierarchy_in_leaf_type_with_sibling(bool async)
    {
        await base.GetType_in_hierarchy_in_leaf_type_with_sibling(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animals] AS [a]
WHERE [a].[Discriminator] = N'Eagle'
""");
    }

    public override async Task GetType_in_hierarchy_in_leaf_type_with_sibling2(bool async)
    {
        await base.GetType_in_hierarchy_in_leaf_type_with_sibling2(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animals] AS [a]
WHERE [a].[Discriminator] = N'Kiwi'
""");
    }

    public override async Task GetType_in_hierarchy_in_leaf_type_with_sibling2_reverse(bool async)
    {
        await base.GetType_in_hierarchy_in_leaf_type_with_sibling2_reverse(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animals] AS [a]
WHERE [a].[Discriminator] = N'Kiwi'
""");
    }

    public override async Task GetType_in_hierarchy_in_leaf_type_with_sibling2_not_equal(bool async)
    {
        await base.GetType_in_hierarchy_in_leaf_type_with_sibling2_not_equal(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animals] AS [a]
WHERE [a].[Discriminator] IN (N'Eagle', N'Kiwi') AND [a].[Discriminator] <> N'Kiwi'
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
