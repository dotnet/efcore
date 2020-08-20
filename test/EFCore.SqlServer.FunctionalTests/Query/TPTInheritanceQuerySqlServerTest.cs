// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit.Abstractions;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public class TPTInheritanceQuerySqlServerTest : TPTInheritanceQueryTestBase<TPTInheritanceQuerySqlServerFixture>
    {
        public TPTInheritanceQuerySqlServerTest(TPTInheritanceQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task Byte_enum_value_constant_used_in_projection(bool async)
        {
            await base.Byte_enum_value_constant_used_in_projection(async);

            AssertSql(
                @"SELECT CASE
    WHEN [b].[IsFlightless] = CAST(1 AS bit) THEN CAST(0 AS tinyint)
    ELSE CAST(1 AS tinyint)
END
FROM [Animals] AS [a]
INNER JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
INNER JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]");
        }

        public override async Task Can_filter_all_animals(bool async)
        {
            await base.Can_filter_all_animals(async);

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [k].[Species] IS NOT NULL THEN N'Kiwi'
    WHEN [e].[Species] IS NOT NULL THEN N'Eagle'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
LEFT JOIN [Eagle] AS [e] ON [a].[Species] = [e].[Species]
LEFT JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
WHERE [a].[Name] = N'Great spotted kiwi'
ORDER BY [a].[Species]");
        }

        public override async Task Can_include_animals(bool async)
        {
            await base.Can_include_animals(async);

            AssertSql(
                @"SELECT [c].[Id], [c].[Name], [t].[Species], [t].[CountryId], [t].[Name], [t].[EagleId], [t].[IsFlightless], [t].[Group], [t].[FoundOn], [t].[Discriminator]
FROM [Countries] AS [c]
LEFT JOIN (
    SELECT [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
        WHEN [k].[Species] IS NOT NULL THEN N'Kiwi'
        WHEN [e].[Species] IS NOT NULL THEN N'Eagle'
    END AS [Discriminator]
    FROM [Animals] AS [a]
    LEFT JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
    LEFT JOIN [Eagle] AS [e] ON [a].[Species] = [e].[Species]
    LEFT JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
) AS [t] ON [c].[Id] = [t].[CountryId]
ORDER BY [c].[Name], [c].[Id], [t].[Species]");
        }

        public override async Task Can_include_prey(bool async)
        {
            await base.Can_include_prey(async);

            AssertSql(
                @"SELECT [t].[Species], [t].[CountryId], [t].[Name], [t].[EagleId], [t].[IsFlightless], [t].[Group], [t0].[Species], [t0].[CountryId], [t0].[Name], [t0].[EagleId], [t0].[IsFlightless], [t0].[Group], [t0].[FoundOn], [t0].[Discriminator]
FROM (
    SELECT TOP(2) [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [e].[Group]
    FROM [Animals] AS [a]
    INNER JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
    INNER JOIN [Eagle] AS [e] ON [a].[Species] = [e].[Species]
) AS [t]
LEFT JOIN (
    SELECT [a0].[Species], [a0].[CountryId], [a0].[Name], [b0].[EagleId], [b0].[IsFlightless], [e0].[Group], [k].[FoundOn], CASE
        WHEN [k].[Species] IS NOT NULL THEN N'Kiwi'
        WHEN [e0].[Species] IS NOT NULL THEN N'Eagle'
    END AS [Discriminator]
    FROM [Animals] AS [a0]
    INNER JOIN [Birds] AS [b0] ON [a0].[Species] = [b0].[Species]
    LEFT JOIN [Eagle] AS [e0] ON [a0].[Species] = [e0].[Species]
    LEFT JOIN [Kiwi] AS [k] ON [a0].[Species] = [k].[Species]
) AS [t0] ON [t].[Species] = [t0].[EagleId]
ORDER BY [t].[Species], [t0].[Species]");
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
@p2='Little spotted kiwi' (Size = 4000)

SET NOCOUNT ON;
INSERT INTO [Animals] ([Species], [CountryId], [Name])
VALUES (@p0, @p1, @p2);",
                //
                @"@p3='Apteryx owenii' (Nullable = false) (Size = 100)
@p4=NULL (Size = 100)
@p5='True'

SET NOCOUNT ON;
INSERT INTO [Birds] ([Species], [EagleId], [IsFlightless])
VALUES (@p3, @p4, @p5);",
                //
                @"@p6='Apteryx owenii' (Nullable = false) (Size = 100)
@p7='0' (Size = 1)

SET NOCOUNT ON;
INSERT INTO [Kiwi] ([Species], [FoundOn])
VALUES (@p6, @p7);",
                //
                @"SELECT TOP(2) [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [k].[FoundOn]
FROM [Animals] AS [a]
INNER JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
INNER JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
WHERE [a].[Species] LIKE N'%owenii'",
                //
                @"@p1='Apteryx owenii' (Nullable = false) (Size = 100)
@p0='Aquila chrysaetos canadensis' (Size = 100)

SET NOCOUNT ON;
UPDATE [Birds] SET [EagleId] = @p0
WHERE [Species] = @p1;
SELECT @@ROWCOUNT;",
                //
                @"SELECT TOP(2) [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [k].[FoundOn]
FROM [Animals] AS [a]
INNER JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
INNER JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
WHERE [a].[Species] LIKE N'%owenii'",
                //
                @"@p0='Apteryx owenii' (Nullable = false) (Size = 100)

SET NOCOUNT ON;
DELETE FROM [Kiwi]
WHERE [Species] = @p0;
SELECT @@ROWCOUNT;",
                //
                @"@p1='Apteryx owenii' (Nullable = false) (Size = 100)

SET NOCOUNT ON;
DELETE FROM [Birds]
WHERE [Species] = @p1;
SELECT @@ROWCOUNT;",
                //
                @"@p2='Apteryx owenii' (Nullable = false) (Size = 100)

SET NOCOUNT ON;
DELETE FROM [Animals]
WHERE [Species] = @p2;
SELECT @@ROWCOUNT;",
                //
                @"SELECT COUNT(*)
FROM [Animals] AS [a]
INNER JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
INNER JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
WHERE [a].[Species] LIKE N'%owenii'");
        }

        public override async Task Can_query_all_animals(bool async)
        {
            await base.Can_query_all_animals(async);

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [k].[Species] IS NOT NULL THEN N'Kiwi'
    WHEN [e].[Species] IS NOT NULL THEN N'Eagle'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
LEFT JOIN [Eagle] AS [e] ON [a].[Species] = [e].[Species]
LEFT JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
ORDER BY [a].[Species]");
        }

        public override async Task Can_query_all_birds(bool async)
        {
            await base.Can_query_all_birds(async);

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [k].[Species] IS NOT NULL THEN N'Kiwi'
    WHEN [e].[Species] IS NOT NULL THEN N'Eagle'
END AS [Discriminator]
FROM [Animals] AS [a]
INNER JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
LEFT JOIN [Eagle] AS [e] ON [a].[Species] = [e].[Species]
LEFT JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
ORDER BY [a].[Species]");
        }

        public override async Task Can_query_all_plants(bool async)
        {
            await base.Can_query_all_plants(async);

            AssertSql(
                @"SELECT [p].[Species], [p].[CountryId], [p].[Genus], [p].[Name], [r].[HasThorns], CASE
    WHEN [r].[Species] IS NOT NULL THEN N'Rose'
    WHEN [d].[Species] IS NOT NULL THEN N'Daisy'
END AS [Discriminator]
FROM [Plants] AS [p]
LEFT JOIN [Daisies] AS [d] ON [p].[Species] = [d].[Species]
LEFT JOIN [Roses] AS [r] ON [p].[Species] = [r].[Species]
ORDER BY [p].[Species]");
        }

        public override async Task Can_query_all_types_when_shared_column(bool async)
        {
            await base.Can_query_all_types_when_shared_column(async);

            AssertSql(
                @"SELECT [d].[Id], [c].[CaffeineGrams], [c].[CokeCO2], [c].[SugarGrams], [l].[LiltCO2], [l].[SugarGrams], [t].[CaffeineGrams], [t].[HasMilk], CASE
    WHEN [t].[Id] IS NOT NULL THEN N'Tea'
    WHEN [l].[Id] IS NOT NULL THEN N'Lilt'
    WHEN [c].[Id] IS NOT NULL THEN N'Coke'
END AS [Discriminator]
FROM [Drinks] AS [d]
LEFT JOIN [Coke] AS [c] ON [d].[Id] = [c].[Id]
LEFT JOIN [Lilt] AS [l] ON [d].[Id] = [l].[Id]
LEFT JOIN [Tea] AS [t] ON [d].[Id] = [t].[Id]");
        }

        public override async Task Can_query_just_kiwis(bool async)
        {
            await base.Can_query_just_kiwis(async);

            AssertSql(
                @"SELECT TOP(2) [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [k].[FoundOn]
FROM [Animals] AS [a]
INNER JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
INNER JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]");
        }

        public override async Task Can_query_just_roses(bool async)
        {
            await base.Can_query_just_roses(async);

            AssertSql(
                @"SELECT TOP(2) [p].[Species], [p].[CountryId], [p].[Genus], [p].[Name], [r].[HasThorns]
FROM [Plants] AS [p]
INNER JOIN [Flowers] AS [f] ON [p].[Species] = [f].[Species]
INNER JOIN [Roses] AS [r] ON [p].[Species] = [r].[Species]");
        }

        public override async Task Can_query_when_shared_column(bool async)
        {
            await base.Can_query_when_shared_column(async);

            AssertSql(
                @"SELECT TOP(2) [d].[Id], [c].[CaffeineGrams], [c].[CokeCO2], [c].[SugarGrams]
FROM [Drinks] AS [d]
INNER JOIN [Coke] AS [c] ON [d].[Id] = [c].[Id]",
                //
                @"SELECT TOP(2) [d].[Id], [l].[LiltCO2], [l].[SugarGrams]
FROM [Drinks] AS [d]
INNER JOIN [Lilt] AS [l] ON [d].[Id] = [l].[Id]",
                //
                @"SELECT TOP(2) [d].[Id], [t].[CaffeineGrams], [t].[HasMilk]
FROM [Drinks] AS [d]
INNER JOIN [Tea] AS [t] ON [d].[Id] = [t].[Id]");
        }

        public override async Task Can_use_backwards_is_animal(bool async)
        {
            await base.Can_use_backwards_is_animal(async);

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [k].[FoundOn]
FROM [Animals] AS [a]
INNER JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
INNER JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]");
        }

        public override async Task Can_use_backwards_of_type_animal(bool async)
        {
            await base.Can_use_backwards_of_type_animal(async);

            AssertSql(" ");
        }

        public override async Task Can_use_is_kiwi(bool async)
        {
            await base.Can_use_is_kiwi(async);

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [k].[Species] IS NOT NULL THEN N'Kiwi'
    WHEN [e].[Species] IS NOT NULL THEN N'Eagle'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
LEFT JOIN [Eagle] AS [e] ON [a].[Species] = [e].[Species]
LEFT JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
WHERE [k].[Species] IS NOT NULL");
        }

        public override async Task Can_use_is_kiwi_in_projection(bool async)
        {
            await base.Can_use_is_kiwi_in_projection(async);

            AssertSql(
                @"SELECT CASE
    WHEN [k].[Species] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Animals] AS [a]
LEFT JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]");
        }

        public override async Task Can_use_is_kiwi_with_other_predicate(bool async)
        {
            await base.Can_use_is_kiwi_with_other_predicate(async);

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [k].[Species] IS NOT NULL THEN N'Kiwi'
    WHEN [e].[Species] IS NOT NULL THEN N'Eagle'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
LEFT JOIN [Eagle] AS [e] ON [a].[Species] = [e].[Species]
LEFT JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
WHERE [k].[Species] IS NOT NULL AND ([a].[CountryId] = 1)");
        }

        public override async Task Can_use_of_type_animal(bool async)
        {
            await base.Can_use_of_type_animal(async);

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [k].[Species] IS NOT NULL THEN N'Kiwi'
    WHEN [e].[Species] IS NOT NULL THEN N'Eagle'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
LEFT JOIN [Eagle] AS [e] ON [a].[Species] = [e].[Species]
LEFT JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
ORDER BY [a].[Species]");
        }

        public override async Task Can_use_of_type_bird(bool async)
        {
            await base.Can_use_of_type_bird(async);

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [k].[Species] IS NOT NULL THEN N'Kiwi'
    WHEN [e].[Species] IS NOT NULL THEN N'Eagle'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
LEFT JOIN [Eagle] AS [e] ON [a].[Species] = [e].[Species]
LEFT JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
WHERE [k].[Species] IS NOT NULL OR [e].[Species] IS NOT NULL
ORDER BY [a].[Species]");
        }

        public override async Task Can_use_of_type_bird_first(bool async)
        {
            await base.Can_use_of_type_bird_first(async);

            AssertSql(
                @"SELECT TOP(1) [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [k].[Species] IS NOT NULL THEN N'Kiwi'
    WHEN [e].[Species] IS NOT NULL THEN N'Eagle'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
LEFT JOIN [Eagle] AS [e] ON [a].[Species] = [e].[Species]
LEFT JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
WHERE [k].[Species] IS NOT NULL OR [e].[Species] IS NOT NULL
ORDER BY [a].[Species]");
        }

        public override async Task Can_use_of_type_bird_predicate(bool async)
        {
            await base.Can_use_of_type_bird_predicate(async);

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [k].[Species] IS NOT NULL THEN N'Kiwi'
    WHEN [e].[Species] IS NOT NULL THEN N'Eagle'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
LEFT JOIN [Eagle] AS [e] ON [a].[Species] = [e].[Species]
LEFT JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
WHERE ([a].[CountryId] = 1) AND ([k].[Species] IS NOT NULL OR [e].[Species] IS NOT NULL)
ORDER BY [a].[Species]");
        }

        public override async Task Can_use_of_type_bird_with_projection(bool async)
        {
            await base.Can_use_of_type_bird_with_projection(async);

            AssertSql(
                @"SELECT [b].[EagleId]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
LEFT JOIN [Eagle] AS [e] ON [a].[Species] = [e].[Species]
LEFT JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
WHERE [k].[Species] IS NOT NULL OR [e].[Species] IS NOT NULL");
        }

        public override async Task Can_use_of_type_kiwi(bool async)
        {
            await base.Can_use_of_type_kiwi(async);

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [k].[FoundOn], CASE
    WHEN [k].[Species] IS NOT NULL THEN N'Kiwi'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
LEFT JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
WHERE [k].[Species] IS NOT NULL");
        }

        public override async Task Can_use_of_type_kiwi_where_north_on_derived_property(bool async)
        {
            await base.Can_use_of_type_kiwi_where_north_on_derived_property(async);

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [k].[FoundOn], CASE
    WHEN [k].[Species] IS NOT NULL THEN N'Kiwi'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
LEFT JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
WHERE [k].[Species] IS NOT NULL AND ([k].[FoundOn] = CAST(0 AS tinyint))");
        }

        public override async Task Can_use_of_type_kiwi_where_south_on_derived_property(bool async)
        {
            await base.Can_use_of_type_kiwi_where_south_on_derived_property(async);

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [k].[FoundOn], CASE
    WHEN [k].[Species] IS NOT NULL THEN N'Kiwi'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
LEFT JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
WHERE [k].[Species] IS NOT NULL AND ([k].[FoundOn] = CAST(1 AS tinyint))");
        }

        public override async Task Can_use_of_type_rose(bool async)
        {
            await base.Can_use_of_type_rose(async);

            AssertSql(
                @"SELECT [p].[Species], [p].[CountryId], [p].[Genus], [p].[Name], [r].[HasThorns], CASE
    WHEN [r].[Species] IS NOT NULL THEN N'Rose'
END AS [Discriminator]
FROM [Plants] AS [p]
LEFT JOIN [Roses] AS [r] ON [p].[Species] = [r].[Species]
WHERE [r].[Species] IS NOT NULL");
        }

        public override void Member_access_on_intermediate_type_works()
        {
            base.Member_access_on_intermediate_type_works();

            AssertSql(
                @"SELECT [a].[Name]
FROM [Animals] AS [a]
INNER JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
INNER JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
ORDER BY [a].[Name]");
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

        public override void Setting_foreign_key_to_a_different_type_throws()
        {
            base.Setting_foreign_key_to_a_different_type_throws();

            AssertSql(
                @"SELECT TOP(2) [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [k].[FoundOn]
FROM [Animals] AS [a]
INNER JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
INNER JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]",
                //
                @"@p0='Haliaeetus leucocephalus' (Nullable = false) (Size = 100)
@p1='0'
@p2='Bald eagle' (Size = 4000)

SET NOCOUNT ON;
INSERT INTO [Animals] ([Species], [CountryId], [Name])
VALUES (@p0, @p1, @p2);");
        }

        public override async Task Subquery_OfType(bool async)
        {
            await base.Subquery_OfType(async);

            AssertSql(
                @"@__p_0='5'

SELECT DISTINCT [t].[Species], [t].[CountryId], [t].[Name], [t].[EagleId], [t].[IsFlightless], [t].[FoundOn], [t].[Discriminator]
FROM (
    SELECT TOP(@__p_0) [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [k].[FoundOn], CASE
        WHEN [k].[Species] IS NOT NULL THEN N'Kiwi'
        WHEN [e].[Species] IS NOT NULL THEN N'Eagle'
    END AS [Discriminator]
    FROM [Animals] AS [a]
    INNER JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
    LEFT JOIN [Eagle] AS [e] ON [a].[Species] = [e].[Species]
    LEFT JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
    ORDER BY [a].[Species]
) AS [t]
WHERE [t].[Discriminator] = N'Kiwi'");
        }

        public override async Task Union_entity_equality(bool async)
        {
            await base.Union_entity_equality(async);

            AssertSql(" ");
        }

        public override async Task Union_siblings_with_duplicate_property_in_subquery(bool async)
        {
            await base.Union_siblings_with_duplicate_property_in_subquery(async);

            AssertSql(" ");
        }

        public override async Task Is_operator_on_result_of_FirstOrDefault(bool async)
        {
            await base.Is_operator_on_result_of_FirstOrDefault(async);

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [k].[Species] IS NOT NULL THEN N'Kiwi'
    WHEN [e].[Species] IS NOT NULL THEN N'Eagle'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
LEFT JOIN [Eagle] AS [e] ON [a].[Species] = [e].[Species]
LEFT JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT TOP(1) [a0].[Species], [a0].[CountryId], [a0].[Name], [b0].[EagleId], [b0].[IsFlightless], [e0].[Group], [k0].[FoundOn], CASE
            WHEN [k0].[Species] IS NOT NULL THEN N'Kiwi'
            WHEN [e0].[Species] IS NOT NULL THEN N'Eagle'
        END AS [Discriminator], [k0].[Species] AS [Species0]
        FROM [Animals] AS [a0]
        LEFT JOIN [Birds] AS [b0] ON [a0].[Species] = [b0].[Species]
        LEFT JOIN [Eagle] AS [e0] ON [a0].[Species] = [e0].[Species]
        LEFT JOIN [Kiwi] AS [k0] ON [a0].[Species] = [k0].[Species]
        WHERE [a0].[Name] = N'Great spotted kiwi'
    ) AS [t]
    WHERE [t].[Species0] IS NOT NULL)
ORDER BY [a].[Species]");
        }

        public override async Task Selecting_only_base_properties_on_base_type(bool async)
        {
            await base.Selecting_only_base_properties_on_base_type(async);

            AssertSql(
                @"SELECT [a].[Name]
FROM [Animals] AS [a]");
        }

        public override async Task Selecting_only_base_properties_on_derived_type(bool async)
        {
            await base.Selecting_only_base_properties_on_derived_type(async);

            AssertSql(
                @"SELECT [a].[Name]
FROM [Animals] AS [a]
INNER JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
