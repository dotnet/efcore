// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public class InheritanceQuerySqlServerTest : InheritanceRelationalQueryTestBase<InheritanceQuerySqlServerFixture>
    {
#pragma warning disable IDE0060 // Remove unused parameter
        public InheritanceQuerySqlServerTest(InheritanceQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
#pragma warning restore IDE0060 // Remove unused parameter
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [ConditionalFact]
        public virtual void Common_property_shares_column()
        {
            using var context = CreateContext();
            var liltType = context.Model.FindEntityType(typeof(Lilt));
            var cokeType = context.Model.FindEntityType(typeof(Coke));
            var teaType = context.Model.FindEntityType(typeof(Tea));

            Assert.Equal("SugarGrams", cokeType.FindProperty("SugarGrams").GetColumnBaseName());
            Assert.Equal("CaffeineGrams", cokeType.FindProperty("CaffeineGrams").GetColumnBaseName());
            Assert.Equal("CokeCO2", cokeType.FindProperty("Carbonation").GetColumnBaseName());

            Assert.Equal("SugarGrams", liltType.FindProperty("SugarGrams").GetColumnBaseName());
            Assert.Equal("LiltCO2", liltType.FindProperty("Carbonation").GetColumnBaseName());

            Assert.Equal("CaffeineGrams", teaType.FindProperty("CaffeineGrams").GetColumnBaseName());
            Assert.Equal("HasMilk", teaType.FindProperty("HasMilk").GetColumnBaseName());
        }

        public override async Task Can_query_when_shared_column(bool async)
        {
            await base.Can_query_when_shared_column(async);

            AssertSql(
                @"SELECT TOP(2) [d].[Id], [d].[Discriminator], [d].[CaffeineGrams], [d].[CokeCO2], [d].[SugarGrams]
FROM [Drinks] AS [d]
WHERE [d].[Discriminator] = N'Coke'",
                //
                @"SELECT TOP(2) [d].[Id], [d].[Discriminator], [d].[LiltCO2], [d].[SugarGrams]
FROM [Drinks] AS [d]
WHERE [d].[Discriminator] = N'Lilt'",
                //
                @"SELECT TOP(2) [d].[Id], [d].[Discriminator], [d].[CaffeineGrams], [d].[HasMilk]
FROM [Drinks] AS [d]
WHERE [d].[Discriminator] = N'Tea'");
        }

        public override void FromSql_on_root()
        {
            base.FromSql_on_root();

            AssertSql(
                @"select * from ""Animals""");
        }

        public override void FromSql_on_derived()
        {
            base.FromSql_on_derived();

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[Group]
FROM (
    select * from ""Animals""
) AS [a]
WHERE [a].[Discriminator] = N'Eagle'");
        }

        public override async Task Can_query_all_types_when_shared_column(bool async)
        {
            await base.Can_query_all_types_when_shared_column(async);

            AssertSql(
                @"SELECT [d].[Id], [d].[Discriminator], [d].[CaffeineGrams], [d].[CokeCO2], [d].[SugarGrams], [d].[LiltCO2], [d].[HasMilk]
FROM [Drinks] AS [d]");
        }

        public override async Task Can_use_of_type_animal(bool async)
        {
            await base.Can_use_of_type_animal(async);

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animals] AS [a]
ORDER BY [a].[Species]");
        }

        public override async Task Can_use_is_kiwi(bool async)
        {
            await base.Can_use_is_kiwi(async);

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animals] AS [a]
WHERE [a].[Discriminator] = N'Kiwi'");
        }

        public override async Task Can_use_is_kiwi_with_other_predicate(bool async)
        {
            await base.Can_use_is_kiwi_with_other_predicate(async);

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animals] AS [a]
WHERE ([a].[Discriminator] = N'Kiwi') AND ([a].[CountryId] = 1)");
        }

        public override async Task Can_use_is_kiwi_in_projection(bool async)
        {
            await base.Can_use_is_kiwi_in_projection(async);

            AssertSql(
                @"SELECT CASE
    WHEN [a].[Discriminator] = N'Kiwi' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Animals] AS [a]");
        }

        public override async Task Can_use_of_type_bird(bool async)
        {
            await base.Can_use_of_type_bird(async);

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animals] AS [a]
ORDER BY [a].[Species]");
        }

        public override async Task Can_use_of_type_bird_predicate(bool async)
        {
            await base.Can_use_of_type_bird_predicate(async);

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animals] AS [a]
WHERE [a].[CountryId] = 1
ORDER BY [a].[Species]");
        }

        public override async Task Can_use_of_type_bird_with_projection(bool async)
        {
            await base.Can_use_of_type_bird_with_projection(async);

            AssertSql(
                @"SELECT [a].[EagleId]
FROM [Animals] AS [a]");
        }

        public override async Task Can_use_of_type_bird_first(bool async)
        {
            await base.Can_use_of_type_bird_first(async);

            AssertSql(
                @"SELECT TOP(1) [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animals] AS [a]
ORDER BY [a].[Species]");
        }

        public override async Task Can_use_of_type_kiwi(bool async)
        {
            await base.Can_use_of_type_kiwi(async);

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[FoundOn]
FROM [Animals] AS [a]
WHERE [a].[Discriminator] = N'Kiwi'");
        }

        public override async Task Can_use_of_type_rose(bool async)
        {
            await base.Can_use_of_type_rose(async);

            AssertSql(
                @"SELECT [p].[Species], [p].[CountryId], [p].[Genus], [p].[Name], [p].[HasThorns]
FROM [Plants] AS [p]
WHERE [p].[Genus] = 0");
        }

        public override async Task Can_query_all_animals(bool async)
        {
            await base.Can_query_all_animals(async);

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animals] AS [a]
ORDER BY [a].[Species]");
        }

        public override async Task Can_query_all_animal_views(bool async)
        {
            await base.Can_query_all_animal_views(async);

            AssertSql(
                @"SELECT [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM (
    SELECT * FROM Animals
) AS [a]
ORDER BY [a].[CountryId]");
        }

        public override async Task Can_query_all_plants(bool async)
        {
            await base.Can_query_all_plants(async);

            AssertSql(
                @"SELECT [p].[Species], [p].[CountryId], [p].[Genus], [p].[Name], [p].[HasThorns]
FROM [Plants] AS [p]
ORDER BY [p].[Species]");
        }

        public override async Task Can_filter_all_animals(bool async)
        {
            await base.Can_filter_all_animals(async);

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animals] AS [a]
WHERE [a].[Name] = N'Great spotted kiwi'
ORDER BY [a].[Species]");
        }

        public override async Task Can_query_all_birds(bool async)
        {
            await base.Can_query_all_birds(async);

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animals] AS [a]
ORDER BY [a].[Species]");
        }

        public override async Task Can_query_just_kiwis(bool async)
        {
            await base.Can_query_just_kiwis(async);

            AssertSql(
                @"SELECT TOP(2) [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[FoundOn]
FROM [Animals] AS [a]
WHERE [a].[Discriminator] = N'Kiwi'");
        }

        public override async Task Can_query_just_roses(bool async)
        {
            await base.Can_query_just_roses(async);

            AssertSql(
                @"SELECT TOP(2) [p].[Species], [p].[CountryId], [p].[Genus], [p].[Name], [p].[HasThorns]
FROM [Plants] AS [p]
WHERE [p].[Genus] = 0"
            );
        }

        public override async Task Can_include_prey(bool async)
        {
            await base.Can_include_prey(async);

            AssertSql(
                @"SELECT [t].[Species], [t].[CountryId], [t].[Discriminator], [t].[Name], [t].[EagleId], [t].[IsFlightless], [t].[Group], [a0].[Species], [a0].[CountryId], [a0].[Discriminator], [a0].[Name], [a0].[EagleId], [a0].[IsFlightless], [a0].[Group], [a0].[FoundOn]
FROM (
    SELECT TOP(2) [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[Group]
    FROM [Animals] AS [a]
    WHERE [a].[Discriminator] = N'Eagle'
) AS [t]
LEFT JOIN [Animals] AS [a0] ON [t].[Species] = [a0].[EagleId]
ORDER BY [t].[Species], [a0].[Species]");
        }

        public override async Task Can_include_animals(bool async)
        {
            await base.Can_include_animals(async);

            AssertSql(
                @"SELECT [c].[Id], [c].[Name], [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Countries] AS [c]
LEFT JOIN [Animals] AS [a] ON [c].[Id] = [a].[CountryId]
ORDER BY [c].[Name], [c].[Id], [a].[Species]");
        }

        public override async Task Can_use_of_type_kiwi_where_north_on_derived_property(bool async)
        {
            await base.Can_use_of_type_kiwi_where_north_on_derived_property(async);

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[FoundOn]
FROM [Animals] AS [a]
WHERE ([a].[Discriminator] = N'Kiwi') AND ([a].[FoundOn] = CAST(0 AS tinyint))");
        }

        public override async Task Can_use_of_type_kiwi_where_south_on_derived_property(bool async)
        {
            await base.Can_use_of_type_kiwi_where_south_on_derived_property(async);

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[FoundOn]
FROM [Animals] AS [a]
WHERE ([a].[Discriminator] = N'Kiwi') AND ([a].[FoundOn] = CAST(1 AS tinyint))");
        }

        public override async Task Discriminator_used_when_projection_over_derived_type(bool async)
        {
            await base.Discriminator_used_when_projection_over_derived_type(async);

            AssertSql(
                @"SELECT [a].[FoundOn]
FROM [Animals] AS [a]
WHERE [a].[Discriminator] = N'Kiwi'");
        }

        public override async Task Discriminator_used_when_projection_over_derived_type2(bool async)
        {
            await base.Discriminator_used_when_projection_over_derived_type2(async);

            AssertSql(
                @"SELECT [a].[IsFlightless], [a].[Discriminator]
FROM [Animals] AS [a]");
        }

        public override async Task Discriminator_used_when_projection_over_of_type(bool async)
        {
            await base.Discriminator_used_when_projection_over_of_type(async);

            AssertSql(
                @"SELECT [a].[FoundOn]
FROM [Animals] AS [a]
WHERE [a].[Discriminator] = N'Kiwi'");
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
@p2='Kiwi' (Nullable = false) (Size = 4000)
@p3=NULL (Size = 100)
@p4='0' (Nullable = true) (Size = 1)
@p5='True' (Nullable = true)
@p6='Little spotted kiwi' (Size = 4000)

SET NOCOUNT ON;
INSERT INTO [Animals] ([Species], [CountryId], [Discriminator], [EagleId], [FoundOn], [IsFlightless], [Name])
VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6);",
                //
                @"SELECT TOP(2) [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[FoundOn]
FROM [Animals] AS [a]
WHERE ([a].[Discriminator] = N'Kiwi') AND ([a].[Species] LIKE N'%owenii')",
                //
                @"@p1='Apteryx owenii' (Nullable = false) (Size = 100)
@p0='Aquila chrysaetos canadensis' (Size = 100)

SET NOCOUNT ON;
UPDATE [Animals] SET [EagleId] = @p0
WHERE [Species] = @p1;
SELECT @@ROWCOUNT;",
                //
                @"SELECT TOP(2) [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[FoundOn]
FROM [Animals] AS [a]
WHERE ([a].[Discriminator] = N'Kiwi') AND ([a].[Species] LIKE N'%owenii')",
                //
                @"@p0='Apteryx owenii' (Nullable = false) (Size = 100)

SET NOCOUNT ON;
DELETE FROM [Animals]
WHERE [Species] = @p0;
SELECT @@ROWCOUNT;",
                //
                @"SELECT COUNT(*)
FROM [Animals] AS [a]
WHERE ([a].[Discriminator] = N'Kiwi') AND ([a].[Species] LIKE N'%owenii')");
        }

        public override async Task Byte_enum_value_constant_used_in_projection(bool async)
        {
            await base.Byte_enum_value_constant_used_in_projection(async);

            AssertSql(
                @"SELECT CASE
    WHEN [a].[IsFlightless] = CAST(1 AS bit) THEN CAST(0 AS tinyint)
    ELSE CAST(1 AS tinyint)
END
FROM [Animals] AS [a]
WHERE [a].[Discriminator] = N'Kiwi'");
        }

        public override async Task Union_siblings_with_duplicate_property_in_subquery(bool async)
        {
            await base.Union_siblings_with_duplicate_property_in_subquery(async);

            AssertSql(
                @"SELECT [t].[Id], [t].[Discriminator], [t].[CaffeineGrams], [t].[CokeCO2], [t].[SugarGrams], [t].[Carbonation], [t].[SugarGrams0], [t].[CaffeineGrams0], [t].[HasMilk]
FROM (
    SELECT [d].[Id], [d].[Discriminator], [d].[CaffeineGrams], [d].[CokeCO2], [d].[SugarGrams], NULL AS [CaffeineGrams0], NULL AS [HasMilk], NULL AS [Carbonation], NULL AS [SugarGrams0]
    FROM [Drinks] AS [d]
    WHERE [d].[Discriminator] = N'Coke'
    UNION
    SELECT [d0].[Id], [d0].[Discriminator], NULL AS [CaffeineGrams], NULL AS [CokeCO2], NULL AS [SugarGrams], [d0].[CaffeineGrams] AS [CaffeineGrams0], [d0].[HasMilk], NULL AS [Carbonation], NULL AS [SugarGrams0]
    FROM [Drinks] AS [d0]
    WHERE [d0].[Discriminator] = N'Tea'
) AS [t]
WHERE [t].[Id] > 0");
        }

        public override async Task OfType_Union_subquery(bool async)
        {
            await base.OfType_Union_subquery(async);

            AssertSql(
                @"SELECT [t].[Species], [t].[CountryId], [t].[Discriminator], [t].[Name], [t].[EagleId], [t].[IsFlightless], [t].[FoundOn]
FROM (
    SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[FoundOn]
    FROM [Animals] AS [a]
    WHERE [a].[Discriminator] IN (N'Eagle', N'Kiwi') AND ([a].[Discriminator] = N'Kiwi')
    UNION
    SELECT [a0].[Species], [a0].[CountryId], [a0].[Discriminator], [a0].[Name], [a0].[EagleId], [a0].[IsFlightless], [a0].[FoundOn]
    FROM [Animals] AS [a0]
    WHERE [a0].[Discriminator] IN (N'Eagle', N'Kiwi') AND ([a0].[Discriminator] = N'Kiwi')
) AS [t]
WHERE ([t].[FoundOn] = CAST(0 AS tinyint)) AND [t].[FoundOn] IS NOT NULL");
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
                @"@__p_0='5'

SELECT DISTINCT [t].[Species], [t].[CountryId], [t].[Discriminator], [t].[Name], [t].[EagleId], [t].[IsFlightless], [t].[FoundOn]
FROM (
    SELECT TOP(@__p_0) [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[FoundOn]
    FROM [Animals] AS [a]
    ORDER BY [a].[Species]
) AS [t]
WHERE [t].[Discriminator] = N'Kiwi'");
        }

        public override async Task Union_entity_equality(bool async)
        {
            await base.Union_entity_equality(async);

            AssertSql(
                @"SELECT [t].[Species], [t].[CountryId], [t].[Discriminator], [t].[Name], [t].[EagleId], [t].[IsFlightless], [t].[Group], [t].[FoundOn]
FROM (
    SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[FoundOn], NULL AS [Group]
    FROM [Animals] AS [a]
    WHERE [a].[Discriminator] = N'Kiwi'
    UNION
    SELECT [a0].[Species], [a0].[CountryId], [a0].[Discriminator], [a0].[Name], [a0].[EagleId], [a0].[IsFlightless], NULL AS [FoundOn], [a0].[Group]
    FROM [Animals] AS [a0]
    WHERE [a0].[Discriminator] = N'Eagle'
) AS [t]
WHERE 0 = 1");
        }

        public override void Member_access_on_intermediate_type_works()
        {
            base.Member_access_on_intermediate_type_works();

            AssertSql(
                @"SELECT [a].[Name]
FROM [Animals] AS [a]
WHERE [a].[Discriminator] = N'Kiwi'
ORDER BY [a].[Name]");
        }

        public override void Casting_to_base_type_joining_with_query_type_works()
        {
            base.Casting_to_base_type_joining_with_query_type_works();

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a0].[CountryId], [a0].[Discriminator], [a0].[Name], [a0].[EagleId], [a0].[IsFlightless], [a0].[Group], [a0].[FoundOn]
FROM [Animals] AS [a]
INNER JOIN (
    Select * from ""Animals""
) AS [a0] ON [a].[Name] = [a0].[Name]
WHERE [a].[Discriminator] = N'Eagle'");
        }

        public override async Task Is_operator_on_result_of_FirstOrDefault(bool async)
        {
            await base.Is_operator_on_result_of_FirstOrDefault(async);

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animals] AS [a]
WHERE (
    SELECT TOP(1) [a0].[Discriminator]
    FROM [Animals] AS [a0]
    WHERE [a0].[Name] = N'Great spotted kiwi') = N'Kiwi'
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
FROM [Animals] AS [a]");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
