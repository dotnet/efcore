// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class TPTInheritanceQuerySqlServerTest(TPTInheritanceQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : TPTInheritanceQueryTestBase<TPTInheritanceQuerySqlServerFixture>(fixture, testOutputHelper)
{
    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Byte_enum_value_constant_used_in_projection(bool async)
    {
        await base.Byte_enum_value_constant_used_in_projection(async);

        AssertSql(
            """
SELECT CASE
    WHEN [b].[IsFlightless] = CAST(1 AS bit) THEN CAST(0 AS tinyint)
    ELSE CAST(1 AS tinyint)
END
FROM [Animals] AS [a]
INNER JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
INNER JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
""");
    }

    public override async Task Can_filter_all_animals(bool async)
    {
        await base.Can_filter_all_animals(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Name], [a].[Species], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [k].[Id] IS NOT NULL THEN N'Kiwi'
    WHEN [e].[Id] IS NOT NULL THEN N'Eagle'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
LEFT JOIN [Eagle] AS [e] ON [a].[Id] = [e].[Id]
LEFT JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
WHERE [a].[Name] = N'Great spotted kiwi'
ORDER BY [a].[Species]
""");
    }

    public override async Task Can_include_animals(bool async)
    {
        await base.Can_include_animals(async);

        AssertSql(
            """
SELECT [c].[Id], [c].[Name], [s].[Id], [s].[CountryId], [s].[Name], [s].[Species], [s].[EagleId], [s].[IsFlightless], [s].[Group], [s].[FoundOn], [s].[Discriminator]
FROM [Countries] AS [c]
LEFT JOIN (
    SELECT [a].[Id], [a].[CountryId], [a].[Name], [a].[Species], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
        WHEN [k].[Id] IS NOT NULL THEN N'Kiwi'
        WHEN [e].[Id] IS NOT NULL THEN N'Eagle'
    END AS [Discriminator]
    FROM [Animals] AS [a]
    LEFT JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
    LEFT JOIN [Eagle] AS [e] ON [a].[Id] = [e].[Id]
    LEFT JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
) AS [s] ON [c].[Id] = [s].[CountryId]
ORDER BY [c].[Name], [c].[Id]
""");
    }

    public override async Task Can_include_prey(bool async)
    {
        await base.Can_include_prey(async);

        AssertSql(
            """
SELECT [s].[Id], [s].[CountryId], [s].[Name], [s].[Species], [s].[EagleId], [s].[IsFlightless], [s].[Group], [s0].[Id], [s0].[CountryId], [s0].[Name], [s0].[Species], [s0].[EagleId], [s0].[IsFlightless], [s0].[Group], [s0].[FoundOn], [s0].[Discriminator]
FROM (
    SELECT TOP(2) [a].[Id], [a].[CountryId], [a].[Name], [a].[Species], [b].[EagleId], [b].[IsFlightless], [e].[Group]
    FROM [Animals] AS [a]
    INNER JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
    INNER JOIN [Eagle] AS [e] ON [a].[Id] = [e].[Id]
) AS [s]
LEFT JOIN (
    SELECT [a0].[Id], [a0].[CountryId], [a0].[Name], [a0].[Species], [b0].[EagleId], [b0].[IsFlightless], [e0].[Group], [k].[FoundOn], CASE
        WHEN [k].[Id] IS NOT NULL THEN N'Kiwi'
        WHEN [e0].[Id] IS NOT NULL THEN N'Eagle'
    END AS [Discriminator]
    FROM [Animals] AS [a0]
    INNER JOIN [Birds] AS [b0] ON [a0].[Id] = [b0].[Id]
    LEFT JOIN [Eagle] AS [e0] ON [a0].[Id] = [e0].[Id]
    LEFT JOIN [Kiwi] AS [k] ON [a0].[Id] = [k].[Id]
) AS [s0] ON [s].[Id] = [s0].[EagleId]
ORDER BY [s].[Id]
""");
    }

    public override Task Can_insert_update_delete()
        => base.Can_insert_update_delete();

    public override async Task Can_query_all_animals(bool async)
    {
        await base.Can_query_all_animals(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Name], [a].[Species], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [k].[Id] IS NOT NULL THEN N'Kiwi'
    WHEN [e].[Id] IS NOT NULL THEN N'Eagle'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
LEFT JOIN [Eagle] AS [e] ON [a].[Id] = [e].[Id]
LEFT JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
ORDER BY [a].[Species]
""");
    }

    public override async Task Can_query_all_birds(bool async)
    {
        await base.Can_query_all_birds(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Name], [a].[Species], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [k].[Id] IS NOT NULL THEN N'Kiwi'
    WHEN [e].[Id] IS NOT NULL THEN N'Eagle'
END AS [Discriminator]
FROM [Animals] AS [a]
INNER JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
LEFT JOIN [Eagle] AS [e] ON [a].[Id] = [e].[Id]
LEFT JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
ORDER BY [a].[Species]
""");
    }

    public override async Task Can_query_all_plants(bool async)
    {
        await base.Can_query_all_plants(async);

        AssertSql(
            """
SELECT [p].[Species], [p].[CountryId], [p].[Genus], [p].[Name], [r].[HasThorns], [d].[AdditionalInfo_Nickname], [d].[AdditionalInfo_LeafStructure_AreLeavesBig], [d].[AdditionalInfo_LeafStructure_NumLeaves], CASE
    WHEN [r].[Species] IS NOT NULL THEN N'Rose'
    WHEN [d].[Species] IS NOT NULL THEN N'Daisy'
END AS [Discriminator]
FROM [Plants] AS [p]
LEFT JOIN [Daisies] AS [d] ON [p].[Species] = [d].[Species]
LEFT JOIN [Roses] AS [r] ON [p].[Species] = [r].[Species]
ORDER BY [p].[Species]
""");
    }

    public override async Task Filter_on_property_inside_complex_type_on_derived_type(bool async)
    {
        await base.Filter_on_property_inside_complex_type_on_derived_type(async);

        AssertSql(
            """
SELECT [p].[Species], [p].[CountryId], [p].[Genus], [p].[Name], [d].[AdditionalInfo_Nickname], [d].[AdditionalInfo_LeafStructure_AreLeavesBig], [d].[AdditionalInfo_LeafStructure_NumLeaves]
FROM [Plants] AS [p]
INNER JOIN [Flowers] AS [f] ON [p].[Species] = [f].[Species]
INNER JOIN [Daisies] AS [d] ON [p].[Species] = [d].[Species]
WHERE [d].[AdditionalInfo_LeafStructure_AreLeavesBig] = CAST(1 AS bit)
""");
    }

    public override async Task Can_query_all_types_when_shared_column(bool async)
    {
        await base.Can_query_all_types_when_shared_column(async);

        AssertSql(
            """
SELECT [d].[Id], [d].[SortIndex], [c].[CaffeineGrams], [c].[CokeCO2], [c].[SugarGrams], [l].[LiltCO2], [l].[SugarGrams], [t].[CaffeineGrams], [t].[HasMilk], CASE
    WHEN [t].[Id] IS NOT NULL THEN N'Tea'
    WHEN [l].[Id] IS NOT NULL THEN N'Lilt'
    WHEN [c].[Id] IS NOT NULL THEN N'Coke'
END AS [Discriminator]
FROM [Drinks] AS [d]
LEFT JOIN [Coke] AS [c] ON [d].[Id] = [c].[Id]
LEFT JOIN [Lilt] AS [l] ON [d].[Id] = [l].[Id]
LEFT JOIN [Tea] AS [t] ON [d].[Id] = [t].[Id]
""");
    }

    public override async Task Can_query_just_kiwis(bool async)
    {
        await base.Can_query_just_kiwis(async);

        AssertSql(
            """
SELECT TOP(2) [a].[Id], [a].[CountryId], [a].[Name], [a].[Species], [b].[EagleId], [b].[IsFlightless], [k].[FoundOn]
FROM [Animals] AS [a]
INNER JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
INNER JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
""");
    }

    public override async Task Can_query_just_roses(bool async)
    {
        await base.Can_query_just_roses(async);

        AssertSql(
            """
SELECT TOP(2) [p].[Species], [p].[CountryId], [p].[Genus], [p].[Name], [r].[HasThorns]
FROM [Plants] AS [p]
INNER JOIN [Flowers] AS [f] ON [p].[Species] = [f].[Species]
INNER JOIN [Roses] AS [r] ON [p].[Species] = [r].[Species]
""");
    }

    public override async Task Can_query_when_shared_column(bool async)
    {
        await base.Can_query_when_shared_column(async);

        AssertSql(
            """
SELECT TOP(2) [d].[Id], [d].[SortIndex], [c].[CaffeineGrams], [c].[CokeCO2], [c].[SugarGrams]
FROM [Drinks] AS [d]
INNER JOIN [Coke] AS [c] ON [d].[Id] = [c].[Id]
""",
            //
            """
SELECT TOP(2) [d].[Id], [d].[SortIndex], [l].[LiltCO2], [l].[SugarGrams]
FROM [Drinks] AS [d]
INNER JOIN [Lilt] AS [l] ON [d].[Id] = [l].[Id]
""",
            //
            """
SELECT TOP(2) [d].[Id], [d].[SortIndex], [t].[CaffeineGrams], [t].[HasMilk]
FROM [Drinks] AS [d]
INNER JOIN [Tea] AS [t] ON [d].[Id] = [t].[Id]
""");
    }

    public override async Task Can_use_backwards_is_animal(bool async)
    {
        await base.Can_use_backwards_is_animal(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Name], [a].[Species], [b].[EagleId], [b].[IsFlightless], [k].[FoundOn]
FROM [Animals] AS [a]
INNER JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
INNER JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
""");
    }

    public override async Task Can_use_backwards_of_type_animal(bool async)
    {
        await base.Can_use_backwards_of_type_animal(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Name], [a].[Species], [b].[EagleId], [b].[IsFlightless], [k].[FoundOn]
FROM [Animals] AS [a]
INNER JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
INNER JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
""");
    }

    public override async Task Can_use_is_kiwi(bool async)
    {
        await base.Can_use_is_kiwi(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Name], [a].[Species], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [k].[Id] IS NOT NULL THEN N'Kiwi'
    WHEN [e].[Id] IS NOT NULL THEN N'Eagle'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
LEFT JOIN [Eagle] AS [e] ON [a].[Id] = [e].[Id]
LEFT JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
WHERE [k].[Id] IS NOT NULL
""");
    }

    public override async Task Can_use_is_kiwi_with_cast(bool async)
    {
        await base.Can_use_is_kiwi_with_cast(async);

        AssertSql(
            """
SELECT CASE
    WHEN [k].[Id] IS NOT NULL THEN [k].[FoundOn]
    ELSE CAST(0 AS tinyint)
END AS [Value]
FROM [Animals] AS [a]
LEFT JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
""");
    }

    public override async Task Can_use_is_kiwi_in_projection(bool async)
    {
        await base.Can_use_is_kiwi_in_projection(async);

        AssertSql(
            """
SELECT CASE
    WHEN [k].[Id] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Animals] AS [a]
LEFT JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
""");
    }

    public override async Task Can_use_is_kiwi_with_other_predicate(bool async)
    {
        await base.Can_use_is_kiwi_with_other_predicate(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Name], [a].[Species], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [k].[Id] IS NOT NULL THEN N'Kiwi'
    WHEN [e].[Id] IS NOT NULL THEN N'Eagle'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
LEFT JOIN [Eagle] AS [e] ON [a].[Id] = [e].[Id]
LEFT JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
WHERE [k].[Id] IS NOT NULL AND [a].[CountryId] = 1
""");
    }

    public override async Task Can_use_of_type_animal(bool async)
    {
        await base.Can_use_of_type_animal(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Name], [a].[Species], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [k].[Id] IS NOT NULL THEN N'Kiwi'
    WHEN [e].[Id] IS NOT NULL THEN N'Eagle'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
LEFT JOIN [Eagle] AS [e] ON [a].[Id] = [e].[Id]
LEFT JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
ORDER BY [a].[Species]
""");
    }

    public override async Task Can_use_of_type_bird(bool async)
    {
        await base.Can_use_of_type_bird(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Name], [a].[Species], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [k].[Id] IS NOT NULL THEN N'Kiwi'
    WHEN [e].[Id] IS NOT NULL THEN N'Eagle'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
LEFT JOIN [Eagle] AS [e] ON [a].[Id] = [e].[Id]
LEFT JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
WHERE [k].[Id] IS NOT NULL OR [e].[Id] IS NOT NULL
ORDER BY [a].[Species]
""");
    }

    public override async Task Can_use_of_type_bird_first(bool async)
    {
        await base.Can_use_of_type_bird_first(async);

        AssertSql(
            """
SELECT TOP(1) [a].[Id], [a].[CountryId], [a].[Name], [a].[Species], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [k].[Id] IS NOT NULL THEN N'Kiwi'
    WHEN [e].[Id] IS NOT NULL THEN N'Eagle'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
LEFT JOIN [Eagle] AS [e] ON [a].[Id] = [e].[Id]
LEFT JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
WHERE [k].[Id] IS NOT NULL OR [e].[Id] IS NOT NULL
ORDER BY [a].[Species]
""");
    }

    public override async Task Can_use_of_type_bird_predicate(bool async)
    {
        await base.Can_use_of_type_bird_predicate(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Name], [a].[Species], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [k].[Id] IS NOT NULL THEN N'Kiwi'
    WHEN [e].[Id] IS NOT NULL THEN N'Eagle'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
LEFT JOIN [Eagle] AS [e] ON [a].[Id] = [e].[Id]
LEFT JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
WHERE [a].[CountryId] = 1 AND ([k].[Id] IS NOT NULL OR [e].[Id] IS NOT NULL)
ORDER BY [a].[Species]
""");
    }

    public override async Task Can_use_of_type_bird_with_projection(bool async)
    {
        await base.Can_use_of_type_bird_with_projection(async);

        AssertSql(
            """
SELECT [b].[EagleId]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
LEFT JOIN [Eagle] AS [e] ON [a].[Id] = [e].[Id]
LEFT JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
WHERE [k].[Id] IS NOT NULL OR [e].[Id] IS NOT NULL
""");
    }

    public override async Task Can_use_of_type_kiwi(bool async)
    {
        await base.Can_use_of_type_kiwi(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Name], [a].[Species], [b].[EagleId], [b].[IsFlightless], [k].[FoundOn], CASE
    WHEN [k].[Id] IS NOT NULL THEN N'Kiwi'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
LEFT JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
WHERE [k].[Id] IS NOT NULL
""");
    }

    public override async Task Can_use_of_type_kiwi_where_north_on_derived_property(bool async)
    {
        await base.Can_use_of_type_kiwi_where_north_on_derived_property(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Name], [a].[Species], [b].[EagleId], [b].[IsFlightless], [k].[FoundOn], CASE
    WHEN [k].[Id] IS NOT NULL THEN N'Kiwi'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
LEFT JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
WHERE [k].[Id] IS NOT NULL AND [k].[FoundOn] = CAST(0 AS tinyint)
""");
    }

    public override async Task Can_use_of_type_kiwi_where_south_on_derived_property(bool async)
    {
        await base.Can_use_of_type_kiwi_where_south_on_derived_property(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Name], [a].[Species], [b].[EagleId], [b].[IsFlightless], [k].[FoundOn], CASE
    WHEN [k].[Id] IS NOT NULL THEN N'Kiwi'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
LEFT JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
WHERE [k].[Id] IS NOT NULL AND [k].[FoundOn] = CAST(1 AS tinyint)
""");
    }

    public override async Task Can_use_of_type_rose(bool async)
    {
        await base.Can_use_of_type_rose(async);

        AssertSql(
            """
SELECT [p].[Species], [p].[CountryId], [p].[Genus], [p].[Name], [r].[HasThorns], CASE
    WHEN [r].[Species] IS NOT NULL THEN N'Rose'
END AS [Discriminator]
FROM [Plants] AS [p]
LEFT JOIN [Roses] AS [r] ON [p].[Species] = [r].[Species]
WHERE [r].[Species] IS NOT NULL
""");
    }

    public override async Task Member_access_on_intermediate_type_works()
    {
        await base.Member_access_on_intermediate_type_works();

        AssertSql(
            """
SELECT [a].[Name]
FROM [Animals] AS [a]
INNER JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
INNER JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
ORDER BY [a].[Name]
""");
    }

    public override async Task OfType_Union_OfType(bool async)
    {
        await base.OfType_Union_OfType(async);

        AssertSql(" ");
    }

    public override async Task OfType_Union_subquery(bool async)
    {
        await base.OfType_Union_subquery(async);

        AssertSql(" ");
    }

    public override async Task Setting_foreign_key_to_a_different_type_throws()
    {
        await base.Setting_foreign_key_to_a_different_type_throws();

        AssertSql(
            """
SELECT TOP(2) [a].[Id], [a].[CountryId], [a].[Name], [a].[Species], [b].[EagleId], [b].[IsFlightless], [k].[FoundOn]
FROM [Animals] AS [a]
INNER JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
INNER JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
""",
            //
            """
@p0='0'
@p1='Bald eagle' (Size = 4000)
@p2='Haliaeetus leucocephalus' (Size = 100)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [Animals] ([CountryId], [Name], [Species])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1, @p2);
""");
    }

    public override async Task Subquery_OfType(bool async)
    {
        await base.Subquery_OfType(async);

        AssertSql(
            """
@__p_0='5'

SELECT DISTINCT [s].[Id], [s].[CountryId], [s].[Name], [s].[Species], [s].[EagleId], [s].[IsFlightless], [s].[FoundOn], [s].[Discriminator]
FROM (
    SELECT TOP(@__p_0) [a].[Id], [a].[CountryId], [a].[Name], [a].[Species], [b].[EagleId], [b].[IsFlightless], [k].[FoundOn], CASE
        WHEN [k].[Id] IS NOT NULL THEN N'Kiwi'
        WHEN [e].[Id] IS NOT NULL THEN N'Eagle'
    END AS [Discriminator]
    FROM [Animals] AS [a]
    INNER JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
    LEFT JOIN [Eagle] AS [e] ON [a].[Id] = [e].[Id]
    LEFT JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
    ORDER BY [a].[Species]
) AS [s]
WHERE [s].[Discriminator] = N'Kiwi'
""");
    }

    public override async Task Union_entity_equality(bool async)
    {
        await base.Union_entity_equality(async);

        AssertSql(" ");
    }

    public override async Task Union_siblings_with_duplicate_property_in_subquery(bool async)
    {
        await base.Union_siblings_with_duplicate_property_in_subquery(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Name], [a].[Species], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [k].[Id] IS NOT NULL THEN N'Kiwi'
    WHEN [e].[Id] IS NOT NULL THEN N'Eagle'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
LEFT JOIN [Eagle] AS [e] ON [a].[Id] = [e].[Id]
LEFT JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT TOP(1) [a0].[Id], [a0].[CountryId], [a0].[Name], [a0].[Species], [b0].[EagleId], [b0].[IsFlightless], [e0].[Group], [k0].[FoundOn], CASE
            WHEN [k0].[Id] IS NOT NULL THEN N'Kiwi'
            WHEN [e0].[Id] IS NOT NULL THEN N'Eagle'
        END AS [Discriminator], [k0].[Id] AS [Id0]
        FROM [Animals] AS [a0]
        LEFT JOIN [Birds] AS [b0] ON [a0].[Id] = [b0].[Id]
        LEFT JOIN [Eagle] AS [e0] ON [a0].[Id] = [e0].[Id]
        LEFT JOIN [Kiwi] AS [k0] ON [a0].[Id] = [k0].[Id]
        WHERE [a0].[Name] = N'Great spotted kiwi'
    ) AS [t]
    WHERE [t].[Id0] IS NOT NULL)
ORDER BY [a].[Species]
""");
    }

    public override async Task Is_operator_on_result_of_FirstOrDefault(bool async)
    {
        await base.Is_operator_on_result_of_FirstOrDefault(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Name], [a].[Species], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [k].[Id] IS NOT NULL THEN N'Kiwi'
    WHEN [e].[Id] IS NOT NULL THEN N'Eagle'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
LEFT JOIN [Eagle] AS [e] ON [a].[Id] = [e].[Id]
LEFT JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT TOP(1) [k0].[Id] AS [Id0]
        FROM [Animals] AS [a0]
        LEFT JOIN [Kiwi] AS [k0] ON [a0].[Id] = [k0].[Id]
        WHERE [a0].[Name] = N'Great spotted kiwi'
    ) AS [s]
    WHERE [s].[Id0] IS NOT NULL)
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
""");
    }

    public override async Task Selecting_only_base_properties_on_derived_type(bool async)
    {
        await base.Selecting_only_base_properties_on_derived_type(async);

        AssertSql(
            """
SELECT [a].[Name]
FROM [Animals] AS [a]
INNER JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
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
SELECT [a].[Id], [a].[CountryId], [a].[Name], [a].[Species], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [k].[Id] IS NOT NULL THEN N'Kiwi'
    WHEN [e].[Id] IS NOT NULL THEN N'Eagle'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
LEFT JOIN [Eagle] AS [e] ON [a].[Id] = [e].[Id]
LEFT JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
WHERE [k].[Id] IS NOT NULL AND [e].[Id] IS NOT NULL
""");
    }

    public override async Task Using_is_operator_with_of_type_on_multiple_type_with_no_result(bool async)
    {
        await base.Using_is_operator_with_of_type_on_multiple_type_with_no_result(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Name], [a].[Species], [b].[EagleId], [b].[IsFlightless], [e].[Group], CASE
    WHEN [e].[Id] IS NOT NULL THEN N'Eagle'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
LEFT JOIN [Eagle] AS [e] ON [a].[Id] = [e].[Id]
LEFT JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
WHERE [k].[Id] IS NOT NULL AND [e].[Id] IS NOT NULL
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
SELECT [a].[Id], [a].[CountryId], [a].[Name], [a].[Species], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [k].[Id] IS NOT NULL THEN N'Kiwi'
    WHEN [e].[Id] IS NOT NULL THEN N'Eagle'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
LEFT JOIN [Eagle] AS [e] ON [a].[Id] = [e].[Id]
LEFT JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
WHERE 0 = 1
""");
    }

    public override async Task GetType_in_hierarchy_in_intermediate_type(bool async)
    {
        await base.GetType_in_hierarchy_in_intermediate_type(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Name], [a].[Species], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [k].[Id] IS NOT NULL THEN N'Kiwi'
    WHEN [e].[Id] IS NOT NULL THEN N'Eagle'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
LEFT JOIN [Eagle] AS [e] ON [a].[Id] = [e].[Id]
LEFT JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
WHERE 0 = 1
""");
    }

    public override async Task GetType_in_hierarchy_in_leaf_type_with_sibling(bool async)
    {
        await base.GetType_in_hierarchy_in_leaf_type_with_sibling(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Name], [a].[Species], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [k].[Id] IS NOT NULL THEN N'Kiwi'
    WHEN [e].[Id] IS NOT NULL THEN N'Eagle'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
LEFT JOIN [Eagle] AS [e] ON [a].[Id] = [e].[Id]
LEFT JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
WHERE [e].[Id] IS NOT NULL
""");
    }

    public override async Task GetType_in_hierarchy_in_leaf_type_with_sibling2(bool async)
    {
        await base.GetType_in_hierarchy_in_leaf_type_with_sibling2(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Name], [a].[Species], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [k].[Id] IS NOT NULL THEN N'Kiwi'
    WHEN [e].[Id] IS NOT NULL THEN N'Eagle'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
LEFT JOIN [Eagle] AS [e] ON [a].[Id] = [e].[Id]
LEFT JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
WHERE [k].[Id] IS NOT NULL
""");
    }

    public override async Task GetType_in_hierarchy_in_leaf_type_with_sibling2_reverse(bool async)
    {
        await base.GetType_in_hierarchy_in_leaf_type_with_sibling2_reverse(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Name], [a].[Species], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [k].[Id] IS NOT NULL THEN N'Kiwi'
    WHEN [e].[Id] IS NOT NULL THEN N'Eagle'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
LEFT JOIN [Eagle] AS [e] ON [a].[Id] = [e].[Id]
LEFT JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
WHERE [k].[Id] IS NOT NULL
""");
    }

    public override async Task GetType_in_hierarchy_in_leaf_type_with_sibling2_not_equal(bool async)
    {
        await base.GetType_in_hierarchy_in_leaf_type_with_sibling2_not_equal(async);

        AssertSql(
            """
SELECT [a].[Id], [a].[CountryId], [a].[Name], [a].[Species], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [k].[Id] IS NOT NULL THEN N'Kiwi'
    WHEN [e].[Id] IS NOT NULL THEN N'Eagle'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
LEFT JOIN [Eagle] AS [e] ON [a].[Id] = [e].[Id]
LEFT JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
WHERE [k].[Id] IS NULL
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
