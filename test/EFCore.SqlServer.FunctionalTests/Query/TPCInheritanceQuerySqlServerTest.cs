﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query;

public class TPCInheritanceQuerySqlServerTest : TPCInheritanceQueryTestBase<TPCInheritanceQuerySqlServerFixture>
{
    public TPCInheritanceQuerySqlServerTest(TPCInheritanceQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Byte_enum_value_constant_used_in_projection(bool async)
    {
        await base.Byte_enum_value_constant_used_in_projection(async);

        AssertSql(
            @"SELECT CASE
    WHEN [k].[IsFlightless] = CAST(1 AS bit) THEN CAST(0 AS tinyint)
    ELSE CAST(1 AS tinyint)
END
FROM [Kiwi] AS [k]");
    }

    public override async Task Can_filter_all_animals(bool async)
    {
        await base.Can_filter_all_animals(async);

        AssertSql(
            @"SELECT [t].[Species], [t].[CountryId], [t].[Name], [t].[EagleId], [t].[IsFlightless], [t].[Group], [t].[FoundOn], [t].[Discriminator]
FROM (
    SELECT [e].[Species], [e].[CountryId], [e].[Name], [e].[EagleId], [e].[IsFlightless], [e].[Group], NULL AS [FoundOn], N'Eagle' AS [Discriminator]
    FROM [Eagle] AS [e]
    UNION ALL
    SELECT [k].[Species], [k].[CountryId], [k].[Name], [k].[EagleId], [k].[IsFlightless], NULL AS [Group], [k].[FoundOn], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [t]
WHERE [t].[Name] = N'Great spotted kiwi'
ORDER BY [t].[Species]");
    }

    public override async Task Can_include_animals(bool async)
    {
        await base.Can_include_animals(async);

        AssertSql(
            @"SELECT [c].[Id], [c].[Name], [t0].[Species], [t0].[CountryId], [t0].[Name], [t0].[EagleId], [t0].[IsFlightless], [t0].[Group], [t0].[FoundOn], [t0].[Discriminator]
FROM [Countries] AS [c]
LEFT JOIN (
    SELECT [e].[Species], [e].[CountryId], [e].[Name], [e].[EagleId], [e].[IsFlightless], [e].[Group], NULL AS [FoundOn], N'Eagle' AS [Discriminator]
    FROM [Eagle] AS [e]
    UNION ALL
    SELECT [k].[Species], [k].[CountryId], [k].[Name], [k].[EagleId], [k].[IsFlightless], NULL AS [Group], [k].[FoundOn], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [t0] ON [c].[Id] = [t0].[CountryId]
ORDER BY [c].[Name], [c].[Id]");
    }

    public override async Task Can_include_prey(bool async)
    {
        await base.Can_include_prey(async);

        AssertSql(
            @"SELECT [t].[Species], [t].[CountryId], [t].[Name], [t].[EagleId], [t].[IsFlightless], [t].[Group], [t0].[Species], [t0].[CountryId], [t0].[Name], [t0].[EagleId], [t0].[IsFlightless], [t0].[Group], [t0].[FoundOn], [t0].[Discriminator]
FROM (
    SELECT TOP(2) [e].[Species], [e].[CountryId], [e].[Name], [e].[EagleId], [e].[IsFlightless], [e].[Group]
    FROM [Eagle] AS [e]
) AS [t]
LEFT JOIN (
    SELECT [e0].[Species], [e0].[CountryId], [e0].[Name], [e0].[EagleId], [e0].[IsFlightless], [e0].[Group], NULL AS [FoundOn], N'Eagle' AS [Discriminator]
    FROM [Eagle] AS [e0]
    UNION ALL
    SELECT [k].[Species], [k].[CountryId], [k].[Name], [k].[EagleId], [k].[IsFlightless], NULL AS [Group], [k].[FoundOn], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [t0] ON [t].[Species] = [t0].[EagleId]
ORDER BY [t].[Species]");
    }

    public override void Can_insert_update_delete()
    {
        base.Can_insert_update_delete();

        AssertSql(
            @"SELECT TOP(2) [c].[Id], [c].[Name]
FROM [Countries] AS [c]
WHERE [c].[Id] = 1",
            //
            @"@p0='Apteryx owenii' (Nullable = false) (Size = 100)
@p1='1'
@p2=NULL (Size = 100)
@p3='0' (Size = 1)
@p4='True'
@p5='Little spotted kiwi' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [Kiwi] ([Species], [CountryId], [EagleId], [FoundOn], [IsFlightless], [Name])
VALUES (@p0, @p1, @p2, @p3, @p4, @p5);",
            //
            @"SELECT TOP(2) [k].[Species], [k].[CountryId], [k].[Name], [k].[EagleId], [k].[IsFlightless], [k].[FoundOn]
FROM [Kiwi] AS [k]
WHERE [k].[Species] LIKE N'%owenii'",
            //
            @"@p1='Apteryx owenii' (Nullable = false) (Size = 100)
@p0='Aquila chrysaetos canadensis' (Size = 100)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [Kiwi] SET [EagleId] = @p0
OUTPUT 1
WHERE [Species] = @p1;",
            //
            @"SELECT TOP(2) [k].[Species], [k].[CountryId], [k].[Name], [k].[EagleId], [k].[IsFlightless], [k].[FoundOn]
FROM [Kiwi] AS [k]
WHERE [k].[Species] LIKE N'%owenii'",
            //
            @"@p0='Apteryx owenii' (Nullable = false) (Size = 100)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
DELETE FROM [Kiwi]
OUTPUT 1
WHERE [Species] = @p0;",
            //
            @"SELECT COUNT(*)
FROM [Kiwi] AS [k]
WHERE [k].[Species] LIKE N'%owenii'");
    }

    public override async Task Can_query_all_animals(bool async)
    {
        await base.Can_query_all_animals(async);

        AssertSql(
            @"SELECT [t].[Species], [t].[CountryId], [t].[Name], [t].[EagleId], [t].[IsFlightless], [t].[Group], [t].[FoundOn], [t].[Discriminator]
FROM (
    SELECT [e].[Species], [e].[CountryId], [e].[Name], [e].[EagleId], [e].[IsFlightless], [e].[Group], NULL AS [FoundOn], N'Eagle' AS [Discriminator]
    FROM [Eagle] AS [e]
    UNION ALL
    SELECT [k].[Species], [k].[CountryId], [k].[Name], [k].[EagleId], [k].[IsFlightless], NULL AS [Group], [k].[FoundOn], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [t]
ORDER BY [t].[Species]");
    }

    public override async Task Can_query_all_birds(bool async)
    {
        await base.Can_query_all_birds(async);

        AssertSql(
            @"SELECT [t].[Species], [t].[CountryId], [t].[Name], [t].[EagleId], [t].[IsFlightless], [t].[Group], [t].[FoundOn], [t].[Discriminator]
FROM (
    SELECT [e].[Species], [e].[CountryId], [e].[Name], [e].[EagleId], [e].[IsFlightless], [e].[Group], NULL AS [FoundOn], N'Eagle' AS [Discriminator]
    FROM [Eagle] AS [e]
    UNION ALL
    SELECT [k].[Species], [k].[CountryId], [k].[Name], [k].[EagleId], [k].[IsFlightless], NULL AS [Group], [k].[FoundOn], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [t]
ORDER BY [t].[Species]");
    }

    public override async Task Can_query_all_plants(bool async)
    {
        await base.Can_query_all_plants(async);

        AssertSql(
            @"SELECT [t].[Species], [t].[CountryId], [t].[Genus], [t].[Name], [t].[HasThorns], [t].[Discriminator]
FROM (
    SELECT [d].[Species], [d].[CountryId], [d].[Genus], [d].[Name], NULL AS [HasThorns], N'Daisy' AS [Discriminator]
    FROM [Daisies] AS [d]
    UNION ALL
    SELECT [r].[Species], [r].[CountryId], [r].[Genus], [r].[Name], [r].[HasThorns], N'Rose' AS [Discriminator]
    FROM [Roses] AS [r]
) AS [t]
ORDER BY [t].[Species]");
    }

    public override async Task Can_query_all_types_when_shared_column(bool async)
    {
        await base.Can_query_all_types_when_shared_column(async);

        AssertSql(
            @"SELECT [d].[Id], NULL AS [CaffeineGrams], NULL AS [CokeCO2], NULL AS [SugarGrams], NULL AS [LiltCO2], NULL AS [SugarGrams0], NULL AS [CaffeineGrams0], NULL AS [HasMilk], N'Drink' AS [Discriminator]
FROM [Drinks] AS [d]
UNION ALL
SELECT [c].[Id], [c].[CaffeineGrams], [c].[CokeCO2], [c].[SugarGrams], NULL AS [LiltCO2], NULL AS [SugarGrams0], NULL AS [CaffeineGrams0], NULL AS [HasMilk], N'Coke' AS [Discriminator]
FROM [Coke] AS [c]
UNION ALL
SELECT [l].[Id], NULL AS [CaffeineGrams], NULL AS [CokeCO2], NULL AS [SugarGrams], [l].[LiltCO2], [l].[SugarGrams] AS [SugarGrams0], NULL AS [CaffeineGrams0], NULL AS [HasMilk], N'Lilt' AS [Discriminator]
FROM [Lilt] AS [l]
UNION ALL
SELECT [t0].[Id], NULL AS [CaffeineGrams], NULL AS [CokeCO2], NULL AS [SugarGrams], NULL AS [LiltCO2], NULL AS [SugarGrams0], [t0].[CaffeineGrams] AS [CaffeineGrams0], [t0].[HasMilk], N'Tea' AS [Discriminator]
FROM [Tea] AS [t0]");
    }

    public override async Task Can_query_just_kiwis(bool async)
    {
        await base.Can_query_just_kiwis(async);

        AssertSql(
            @"SELECT TOP(2) [k].[Species], [k].[CountryId], [k].[Name], [k].[EagleId], [k].[IsFlightless], [k].[FoundOn]
FROM [Kiwi] AS [k]");
    }

    public override async Task Can_query_just_roses(bool async)
    {
        await base.Can_query_just_roses(async);

        AssertSql(
            @"SELECT TOP(2) [r].[Species], [r].[CountryId], [r].[Genus], [r].[Name], [r].[HasThorns]
FROM [Roses] AS [r]");
    }

    public override async Task Can_query_when_shared_column(bool async)
    {
        await base.Can_query_when_shared_column(async);

        AssertSql(
            @"SELECT TOP(2) [c].[Id], [c].[CaffeineGrams], [c].[CokeCO2], [c].[SugarGrams]
FROM [Coke] AS [c]",
            //
            @"SELECT TOP(2) [l].[Id], [l].[LiltCO2], [l].[SugarGrams]
FROM [Lilt] AS [l]",
            //
            @"SELECT TOP(2) [t].[Id], [t].[CaffeineGrams], [t].[HasMilk]
FROM [Tea] AS [t]");
    }

    public override async Task Can_use_backwards_is_animal(bool async)
    {
        await base.Can_use_backwards_is_animal(async);

        AssertSql(
            @"SELECT [k].[Species], [k].[CountryId], [k].[Name], [k].[EagleId], [k].[IsFlightless], [k].[FoundOn]
FROM [Kiwi] AS [k]");
    }

    public override async Task Can_use_backwards_of_type_animal(bool async)
    {
        await base.Can_use_backwards_of_type_animal(async);

        AssertSql(
            @"SELECT [k].[Species], [k].[CountryId], [k].[Name], [k].[EagleId], [k].[IsFlightless], [k].[FoundOn]
FROM [Kiwi] AS [k]");
    }

    public override async Task Can_use_is_kiwi(bool async)
    {
        await base.Can_use_is_kiwi(async);

        AssertSql(
            @"SELECT [k].[Species], [k].[CountryId], [k].[Name], [k].[EagleId], [k].[IsFlightless], NULL AS [Group], [k].[FoundOn], N'Kiwi' AS [Discriminator]
FROM [Kiwi] AS [k]");
    }

    public override async Task Can_use_is_kiwi_with_cast(bool async)
    {
        await base.Can_use_is_kiwi_with_cast(async);

        AssertSql(
            @"SELECT CASE
    WHEN [t].[Discriminator] = N'Kiwi' THEN [t].[FoundOn]
    ELSE CAST(0 AS tinyint)
END AS [Value]
FROM (
    SELECT NULL AS [FoundOn], N'Eagle' AS [Discriminator]
    FROM [Eagle] AS [e]
    UNION ALL
    SELECT [k].[FoundOn], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [t]");
    }

    public override async Task Can_use_is_kiwi_in_projection(bool async)
    {
        await base.Can_use_is_kiwi_in_projection(async);

        AssertSql(
            @"SELECT CASE
    WHEN [t].[Discriminator] = N'Kiwi' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM (
    SELECT N'Eagle' AS [Discriminator]
    FROM [Eagle] AS [e]
    UNION ALL
    SELECT N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [t]");
    }

    public override async Task Can_use_is_kiwi_with_other_predicate(bool async)
    {
        await base.Can_use_is_kiwi_with_other_predicate(async);

        AssertSql(
            @"SELECT [t].[Species], [t].[CountryId], [t].[Name], [t].[EagleId], [t].[IsFlightless], [t].[Group], [t].[FoundOn], [t].[Discriminator]
FROM (
    SELECT [e].[Species], [e].[CountryId], [e].[Name], [e].[EagleId], [e].[IsFlightless], [e].[Group], NULL AS [FoundOn], N'Eagle' AS [Discriminator]
    FROM [Eagle] AS [e]
    UNION ALL
    SELECT [k].[Species], [k].[CountryId], [k].[Name], [k].[EagleId], [k].[IsFlightless], NULL AS [Group], [k].[FoundOn], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [t]
WHERE [t].[Discriminator] = N'Kiwi' AND [t].[CountryId] = 1");
    }

    public override async Task Can_use_of_type_animal(bool async)
    {
        await base.Can_use_of_type_animal(async);

        AssertSql(
            @"SELECT [t].[Species], [t].[CountryId], [t].[Name], [t].[EagleId], [t].[IsFlightless], [t].[Group], [t].[FoundOn], [t].[Discriminator]
FROM (
    SELECT [e].[Species], [e].[CountryId], [e].[Name], [e].[EagleId], [e].[IsFlightless], [e].[Group], NULL AS [FoundOn], N'Eagle' AS [Discriminator]
    FROM [Eagle] AS [e]
    UNION ALL
    SELECT [k].[Species], [k].[CountryId], [k].[Name], [k].[EagleId], [k].[IsFlightless], NULL AS [Group], [k].[FoundOn], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [t]
ORDER BY [t].[Species]");
    }

    public override async Task Can_use_of_type_bird(bool async)
    {
        await base.Can_use_of_type_bird(async);

        AssertSql(
            @"SELECT [t].[Species], [t].[CountryId], [t].[Name], [t].[EagleId], [t].[IsFlightless], [t].[Group], [t].[FoundOn], [t].[Discriminator]
FROM (
    SELECT [e].[Species], [e].[CountryId], [e].[Name], [e].[EagleId], [e].[IsFlightless], [e].[Group], NULL AS [FoundOn], N'Eagle' AS [Discriminator]
    FROM [Eagle] AS [e]
    UNION ALL
    SELECT [k].[Species], [k].[CountryId], [k].[Name], [k].[EagleId], [k].[IsFlightless], NULL AS [Group], [k].[FoundOn], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [t]
WHERE [t].[Discriminator] IN (N'Eagle', N'Kiwi')
ORDER BY [t].[Species]");
    }

    public override async Task Can_use_of_type_bird_first(bool async)
    {
        await base.Can_use_of_type_bird_first(async);

        AssertSql(
            @"SELECT TOP(1) [t].[Species], [t].[CountryId], [t].[Name], [t].[EagleId], [t].[IsFlightless], [t].[Group], [t].[FoundOn], [t].[Discriminator]
FROM (
    SELECT [e].[Species], [e].[CountryId], [e].[Name], [e].[EagleId], [e].[IsFlightless], [e].[Group], NULL AS [FoundOn], N'Eagle' AS [Discriminator]
    FROM [Eagle] AS [e]
    UNION ALL
    SELECT [k].[Species], [k].[CountryId], [k].[Name], [k].[EagleId], [k].[IsFlightless], NULL AS [Group], [k].[FoundOn], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [t]
WHERE [t].[Discriminator] IN (N'Eagle', N'Kiwi')
ORDER BY [t].[Species]");
    }

    public override async Task Can_use_of_type_bird_predicate(bool async)
    {
        await base.Can_use_of_type_bird_predicate(async);

        AssertSql(
            @"SELECT [t].[Species], [t].[CountryId], [t].[Name], [t].[EagleId], [t].[IsFlightless], [t].[Group], [t].[FoundOn], [t].[Discriminator]
FROM (
    SELECT [e].[Species], [e].[CountryId], [e].[Name], [e].[EagleId], [e].[IsFlightless], [e].[Group], NULL AS [FoundOn], N'Eagle' AS [Discriminator]
    FROM [Eagle] AS [e]
    UNION ALL
    SELECT [k].[Species], [k].[CountryId], [k].[Name], [k].[EagleId], [k].[IsFlightless], NULL AS [Group], [k].[FoundOn], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [t]
WHERE [t].[CountryId] = 1 AND [t].[Discriminator] IN (N'Eagle', N'Kiwi')
ORDER BY [t].[Species]");
    }

    public override async Task Can_use_of_type_bird_with_projection(bool async)
    {
        await base.Can_use_of_type_bird_with_projection(async);

        AssertSql(
            @"SELECT [t].[EagleId]
FROM (
    SELECT [e].[EagleId], N'Eagle' AS [Discriminator]
    FROM [Eagle] AS [e]
    UNION ALL
    SELECT [k].[EagleId], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [t]
WHERE [t].[Discriminator] IN (N'Eagle', N'Kiwi')");
    }

    public override async Task Can_use_of_type_kiwi(bool async)
    {
        await base.Can_use_of_type_kiwi(async);

        AssertSql(
            @"SELECT [k].[Species], [k].[CountryId], [k].[Name], [k].[EagleId], [k].[IsFlightless], [k].[FoundOn], N'Kiwi' AS [Discriminator]
FROM [Kiwi] AS [k]");
    }

    public override async Task Can_use_of_type_kiwi_where_north_on_derived_property(bool async)
    {
        await base.Can_use_of_type_kiwi_where_north_on_derived_property(async);

        AssertSql(
            @"SELECT [t].[Species], [t].[CountryId], [t].[Name], [t].[EagleId], [t].[IsFlightless], [t].[FoundOn], [t].[Discriminator]
FROM (
    SELECT [k].[Species], [k].[CountryId], [k].[Name], [k].[EagleId], [k].[IsFlightless], [k].[FoundOn], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [t]
WHERE [t].[FoundOn] = CAST(0 AS tinyint)");
    }

    public override async Task Can_use_of_type_kiwi_where_south_on_derived_property(bool async)
    {
        await base.Can_use_of_type_kiwi_where_south_on_derived_property(async);

        AssertSql(
            @"SELECT [t].[Species], [t].[CountryId], [t].[Name], [t].[EagleId], [t].[IsFlightless], [t].[FoundOn], [t].[Discriminator]
FROM (
    SELECT [k].[Species], [k].[CountryId], [k].[Name], [k].[EagleId], [k].[IsFlightless], [k].[FoundOn], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [t]
WHERE [t].[FoundOn] = CAST(1 AS tinyint)");
    }

    public override async Task Can_use_of_type_rose(bool async)
    {
        await base.Can_use_of_type_rose(async);

        AssertSql(
            @"SELECT [r].[Species], [r].[CountryId], [r].[Genus], [r].[Name], [r].[HasThorns], N'Rose' AS [Discriminator]
FROM [Roses] AS [r]");
    }

    public override void Member_access_on_intermediate_type_works()
    {
        base.Member_access_on_intermediate_type_works();

        AssertSql(
            @"SELECT [k].[Name]
FROM [Kiwi] AS [k]
ORDER BY [k].[Name]");
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

    public override void Setting_foreign_key_to_a_different_type_throws()
    {
        base.Setting_foreign_key_to_a_different_type_throws();

        AssertSql(
            @"SELECT TOP(2) [k].[Species], [k].[CountryId], [k].[Name], [k].[EagleId], [k].[IsFlightless], [k].[FoundOn]
FROM [Kiwi] AS [k]",
            //
            @"@p0='Haliaeetus leucocephalus' (Nullable = false) (Size = 100)
@p1='0'
@p2='Apteryx haastii' (Size = 100)
@p3='1'
@p4='False'
@p5='Bald eagle' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [Eagle] ([Species], [CountryId], [EagleId], [Group], [IsFlightless], [Name])
VALUES (@p0, @p1, @p2, @p3, @p4, @p5);");
    }

    public override async Task Subquery_OfType(bool async)
    {
        await base.Subquery_OfType(async);

        AssertSql(
            @"@__p_0='5'

SELECT DISTINCT [t0].[Species], [t0].[CountryId], [t0].[Name], [t0].[EagleId], [t0].[IsFlightless], [t0].[FoundOn], [t0].[Discriminator]
FROM (
    SELECT TOP(@__p_0) [t].[Species], [t].[CountryId], [t].[Name], [t].[EagleId], [t].[IsFlightless], [t].[FoundOn], [t].[Discriminator]
    FROM (
        SELECT [e].[Species], [e].[CountryId], [e].[Name], [e].[EagleId], [e].[IsFlightless], NULL AS [FoundOn], N'Eagle' AS [Discriminator]
        FROM [Eagle] AS [e]
        UNION ALL
        SELECT [k].[Species], [k].[CountryId], [k].[Name], [k].[EagleId], [k].[IsFlightless], [k].[FoundOn], N'Kiwi' AS [Discriminator]
        FROM [Kiwi] AS [k]
    ) AS [t]
    ORDER BY [t].[Species]
) AS [t0]
WHERE [t0].[Discriminator] = N'Kiwi'");
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
            @"SELECT [t].[Species], [t].[CountryId], [t].[Name], [t].[EagleId], [t].[IsFlightless], [t].[Group], [t].[FoundOn], [t].[Discriminator]
FROM (
    SELECT [e].[Species], [e].[CountryId], [e].[Name], [e].[EagleId], [e].[IsFlightless], [e].[Group], NULL AS [FoundOn], N'Eagle' AS [Discriminator]
    FROM [Eagle] AS [e]
    UNION ALL
    SELECT [k].[Species], [k].[CountryId], [k].[Name], [k].[EagleId], [k].[IsFlightless], NULL AS [Group], [k].[FoundOn], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [t]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT TOP(1) [t1].[Species], [t1].[CountryId], [t1].[Name], [t1].[EagleId], [t1].[IsFlightless], [t1].[Group], [t1].[FoundOn], [t1].[Discriminator]
        FROM (
            SELECT [e0].[Species], [e0].[CountryId], [e0].[Name], [e0].[EagleId], [e0].[IsFlightless], [e0].[Group], NULL AS [FoundOn], N'Eagle' AS [Discriminator]
            FROM [Eagle] AS [e0]
            UNION ALL
            SELECT [k0].[Species], [k0].[CountryId], [k0].[Name], [k0].[EagleId], [k0].[IsFlightless], NULL AS [Group], [k0].[FoundOn], N'Kiwi' AS [Discriminator]
            FROM [Kiwi] AS [k0]
        ) AS [t1]
        WHERE [t1].[Name] = N'Great spotted kiwi'
    ) AS [t0]
    WHERE [t0].[Discriminator] = N'Kiwi')
ORDER BY [t].[Species]");
    }

    public override async Task Selecting_only_base_properties_on_base_type(bool async)
    {
        await base.Selecting_only_base_properties_on_base_type(async);

        AssertSql(
            @"SELECT [e].[Name]
FROM [Eagle] AS [e]
UNION ALL
SELECT [k].[Name]
FROM [Kiwi] AS [k]");
    }

    public override async Task Selecting_only_base_properties_on_derived_type(bool async)
    {
        await base.Selecting_only_base_properties_on_derived_type(async);

        AssertSql(
            @"SELECT [e].[Name]
FROM [Eagle] AS [e]
UNION ALL
SELECT [k].[Name]
FROM [Kiwi] AS [k]");
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
            @"SELECT [t].[Species], [t].[CountryId], [t].[Name], [t].[EagleId], [t].[IsFlightless], [t].[Group], [t].[FoundOn], [t].[Discriminator]
FROM (
    SELECT [k].[Species], [k].[CountryId], [k].[Name], [k].[EagleId], [k].[IsFlightless], NULL AS [Group], [k].[FoundOn], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [t]
WHERE [t].[Discriminator] = N'Eagle'");
    }

    public override async Task Using_is_operator_with_of_type_on_multiple_type_with_no_result(bool async)
    {
        await base.Using_is_operator_with_of_type_on_multiple_type_with_no_result(async);

        AssertSql(
            @"SELECT [t].[Species], [t].[CountryId], [t].[Name], [t].[EagleId], [t].[IsFlightless], [t].[Group], [t].[Discriminator]
FROM (
    SELECT [k].[Species], [k].[CountryId], [k].[Name], [k].[EagleId], [k].[IsFlightless], NULL AS [Group], N'Kiwi' AS [Discriminator]
    FROM [Kiwi] AS [k]
) AS [t]
WHERE [t].[Discriminator] = N'Eagle'");
    }

    public override async Task Using_OfType_on_multiple_type_with_no_result(bool async)
    {
        await base.Using_OfType_on_multiple_type_with_no_result(async);

        AssertSql();
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
