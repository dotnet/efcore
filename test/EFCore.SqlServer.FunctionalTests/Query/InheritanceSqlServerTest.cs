// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestModels.Inheritance;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public class InheritanceSqlServerTest : InheritanceRelationalTestBase<InheritanceSqlServerFixture>
    {
#pragma warning disable IDE0060 // Remove unused parameter
        public InheritanceSqlServerTest(InheritanceSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
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

            Assert.Equal("SugarGrams", cokeType.FindProperty("SugarGrams").GetColumnName());
            Assert.Equal("CaffeineGrams", cokeType.FindProperty("CaffeineGrams").GetColumnName());
            Assert.Equal("CokeCO2", cokeType.FindProperty("Carbonation").GetColumnName());

            Assert.Equal("SugarGrams", liltType.FindProperty("SugarGrams").GetColumnName());
            Assert.Equal("LiltCO2", liltType.FindProperty("Carbonation").GetColumnName());

            Assert.Equal("CaffeineGrams", teaType.FindProperty("CaffeineGrams").GetColumnName());
            Assert.Equal("HasMilk", teaType.FindProperty("HasMilk").GetColumnName());
        }

        public override void Can_query_when_shared_column()
        {
            base.Can_query_when_shared_column();

            AssertSql(
                @"SELECT TOP(2) [d].[Id], [d].[Discriminator], [d].[CaffeineGrams], [d].[CokeCO2], [d].[SugarGrams]
FROM [Drink] AS [d]
WHERE [d].[Discriminator] = N'Coke'",
                //
                @"SELECT TOP(2) [d].[Id], [d].[Discriminator], [d].[LiltCO2], [d].[SugarGrams]
FROM [Drink] AS [d]
WHERE [d].[Discriminator] = N'Lilt'",
                //
                @"SELECT TOP(2) [d].[Id], [d].[Discriminator], [d].[CaffeineGrams], [d].[HasMilk]
FROM [Drink] AS [d]
WHERE [d].[Discriminator] = N'Tea'");
        }

        public override void FromSql_on_root()
        {
            base.FromSql_on_root();

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM (
    select * from ""Animal""
) AS [a]
WHERE [a].[Discriminator] IN (N'Eagle', N'Kiwi')");
        }

        public override void FromSql_on_derived()
        {
            base.FromSql_on_derived();

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[Group]
FROM (
    select * from ""Animal""
) AS [a]
WHERE [a].[Discriminator] = N'Eagle'");
        }

        public override void Can_query_all_types_when_shared_column()
        {
            base.Can_query_all_types_when_shared_column();

            AssertSql(
                @"SELECT [d].[Id], [d].[Discriminator], [d].[CaffeineGrams], [d].[CokeCO2], [d].[SugarGrams], [d].[LiltCO2], [d].[HasMilk]
FROM [Drink] AS [d]
WHERE [d].[Discriminator] IN (N'Drink', N'Coke', N'Lilt', N'Tea')");
        }

        public override void Can_use_of_type_animal()
        {
            base.Can_use_of_type_animal();

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animal] AS [a]
WHERE [a].[Discriminator] IN (N'Eagle', N'Kiwi')
ORDER BY [a].[Species]");
        }

        public override void Can_use_is_kiwi()
        {
            base.Can_use_is_kiwi();

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animal] AS [a]
WHERE [a].[Discriminator] IN (N'Eagle', N'Kiwi') AND ([a].[Discriminator] = N'Kiwi')");
        }

        public override void Can_use_is_kiwi_with_other_predicate()
        {
            base.Can_use_is_kiwi_with_other_predicate();

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animal] AS [a]
WHERE [a].[Discriminator] IN (N'Eagle', N'Kiwi') AND (([a].[Discriminator] = N'Kiwi') AND ([a].[CountryId] = 1))");
        }

        public override void Can_use_is_kiwi_in_projection()
        {
            base.Can_use_is_kiwi_in_projection();

            AssertSql(
                @"SELECT CASE
    WHEN [a].[Discriminator] = N'Kiwi' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Animal] AS [a]
WHERE [a].[Discriminator] IN (N'Eagle', N'Kiwi')");
        }

        public override void Can_use_of_type_bird()
        {
            base.Can_use_of_type_bird();

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animal] AS [a]
WHERE [a].[Discriminator] IN (N'Eagle', N'Kiwi') AND [a].[Discriminator] IN (N'Eagle', N'Kiwi')
ORDER BY [a].[Species]");
        }

        public override void Can_use_of_type_bird_predicate()
        {
            base.Can_use_of_type_bird_predicate();

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animal] AS [a]
WHERE ([a].[Discriminator] IN (N'Eagle', N'Kiwi') AND ([a].[CountryId] = 1)) AND [a].[Discriminator] IN (N'Eagle', N'Kiwi')
ORDER BY [a].[Species]");
        }

        public override void Can_use_of_type_bird_with_projection()
        {
            base.Can_use_of_type_bird_with_projection();

            AssertSql(
                @"SELECT [a].[EagleId]
FROM [Animal] AS [a]
WHERE [a].[Discriminator] IN (N'Eagle', N'Kiwi') AND [a].[Discriminator] IN (N'Eagle', N'Kiwi')");
        }

        public override void Can_use_of_type_bird_first()
        {
            base.Can_use_of_type_bird_first();

            AssertSql(
                @"SELECT TOP(1) [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animal] AS [a]
WHERE [a].[Discriminator] IN (N'Eagle', N'Kiwi') AND [a].[Discriminator] IN (N'Eagle', N'Kiwi')
ORDER BY [a].[Species]");
        }

        public override void Can_use_of_type_kiwi()
        {
            base.Can_use_of_type_kiwi();

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[FoundOn]
FROM [Animal] AS [a]
WHERE [a].[Discriminator] IN (N'Eagle', N'Kiwi') AND ([a].[Discriminator] = N'Kiwi')");
        }

        public override void Can_use_of_type_rose()
        {
            base.Can_use_of_type_rose();

            AssertSql(
                @"SELECT [p].[Species], [p].[CountryId], [p].[Genus], [p].[Name], [p].[HasThorns]
FROM [Plant] AS [p]
WHERE [p].[Genus] IN (1, 0) AND ([p].[Genus] = 0)");
        }

        public override void Can_query_all_animals()
        {
            base.Can_query_all_animals();

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animal] AS [a]
WHERE [a].[Discriminator] IN (N'Eagle', N'Kiwi')
ORDER BY [a].[Species]");
        }

        public override void Can_query_all_animal_views()
        {
            base.Can_query_all_animal_views();

            AssertSql(
                @"SELECT [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM (
    SELECT * FROM Animal
) AS [a]
WHERE [a].[Discriminator] IN (N'Eagle', N'Kiwi')
ORDER BY [a].[CountryId]");
        }

        public override void Can_query_all_plants()
        {
            base.Can_query_all_plants();

            AssertSql(
                @"SELECT [p].[Species], [p].[CountryId], [p].[Genus], [p].[Name], [p].[HasThorns]
FROM [Plant] AS [p]
WHERE [p].[Genus] IN (1, 0)
ORDER BY [p].[Species]");
        }

        public override void Can_filter_all_animals()
        {
            base.Can_filter_all_animals();

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animal] AS [a]
WHERE [a].[Discriminator] IN (N'Eagle', N'Kiwi') AND ([a].[Name] = N'Great spotted kiwi')
ORDER BY [a].[Species]");
        }

        public override void Can_query_all_birds()
        {
            base.Can_query_all_birds();

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animal] AS [a]
WHERE [a].[Discriminator] IN (N'Eagle', N'Kiwi')
ORDER BY [a].[Species]");
        }

        public override void Can_query_just_kiwis()
        {
            base.Can_query_just_kiwis();

            AssertSql(
                @"SELECT TOP(2) [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[FoundOn]
FROM [Animal] AS [a]
WHERE [a].[Discriminator] = N'Kiwi'");
        }

        public override void Can_query_just_roses()
        {
            base.Can_query_just_roses();

            AssertSql(
                @"SELECT TOP(2) [p].[Species], [p].[CountryId], [p].[Genus], [p].[Name], [p].[HasThorns]
FROM [Plant] AS [p]
WHERE [p].[Genus] = 0"
            );
        }

        public override void Can_include_prey()
        {
            base.Can_include_prey();

            AssertSql(
                @"SELECT [t].[Species], [t].[CountryId], [t].[Discriminator], [t].[Name], [t].[EagleId], [t].[IsFlightless], [t].[Group], [t0].[Species], [t0].[CountryId], [t0].[Discriminator], [t0].[Name], [t0].[EagleId], [t0].[IsFlightless], [t0].[Group], [t0].[FoundOn]
FROM (
    SELECT TOP(2) [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[Group]
    FROM [Animal] AS [a]
    WHERE [a].[Discriminator] = N'Eagle'
) AS [t]
LEFT JOIN (
    SELECT [a0].[Species], [a0].[CountryId], [a0].[Discriminator], [a0].[Name], [a0].[EagleId], [a0].[IsFlightless], [a0].[Group], [a0].[FoundOn]
    FROM [Animal] AS [a0]
    WHERE [a0].[Discriminator] IN (N'Eagle', N'Kiwi')
) AS [t0] ON [t].[Species] = [t0].[EagleId]
ORDER BY [t].[Species], [t0].[Species]");
        }

        public override void Can_include_animals()
        {
            base.Can_include_animals();

            AssertSql(
                @"SELECT [c].[Id], [c].[Name], [t].[Species], [t].[CountryId], [t].[Discriminator], [t].[Name], [t].[EagleId], [t].[IsFlightless], [t].[Group], [t].[FoundOn]
FROM [Country] AS [c]
LEFT JOIN (
    SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
    FROM [Animal] AS [a]
    WHERE [a].[Discriminator] IN (N'Eagle', N'Kiwi')
) AS [t] ON [c].[Id] = [t].[CountryId]
ORDER BY [c].[Name], [c].[Id], [t].[Species]");
        }

        public override void Can_use_of_type_kiwi_where_north_on_derived_property()
        {
            base.Can_use_of_type_kiwi_where_north_on_derived_property();

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[FoundOn]
FROM [Animal] AS [a]
WHERE ([a].[Discriminator] IN (N'Eagle', N'Kiwi') AND ([a].[Discriminator] = N'Kiwi')) AND ([a].[FoundOn] = CAST(0 AS tinyint))");
        }

        public override void Can_use_of_type_kiwi_where_south_on_derived_property()
        {
            base.Can_use_of_type_kiwi_where_south_on_derived_property();

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[FoundOn]
FROM [Animal] AS [a]
WHERE ([a].[Discriminator] IN (N'Eagle', N'Kiwi') AND ([a].[Discriminator] = N'Kiwi')) AND ([a].[FoundOn] = CAST(1 AS tinyint))");
        }

        public override void Discriminator_used_when_projection_over_derived_type()
        {
            base.Discriminator_used_when_projection_over_derived_type();

            AssertSql(
                @"SELECT [a].[FoundOn]
FROM [Animal] AS [a]
WHERE [a].[Discriminator] = N'Kiwi'");
        }

        public override void Discriminator_used_when_projection_over_derived_type2()
        {
            base.Discriminator_used_when_projection_over_derived_type2();

            AssertSql(
                @"SELECT [a].[IsFlightless], [a].[Discriminator]
FROM [Animal] AS [a]
WHERE [a].[Discriminator] IN (N'Eagle', N'Kiwi')");
        }

        public override void Discriminator_used_when_projection_over_of_type()
        {
            base.Discriminator_used_when_projection_over_of_type();

            AssertSql(
                @"SELECT [a].[FoundOn]
FROM [Animal] AS [a]
WHERE [a].[Discriminator] IN (N'Eagle', N'Kiwi') AND ([a].[Discriminator] = N'Kiwi')");
        }

        public override void Can_insert_update_delete()
        {
            base.Can_insert_update_delete();

            AssertSql(
                @"SELECT TOP(2) [c].[Id], [c].[Name]
FROM [Country] AS [c]
WHERE [c].[Id] = 1",
                //
                @"@p0='Apteryx owenii' (Nullable = false) (Size = 100)
@p1='1'
@p2='Kiwi' (Nullable = false) (Size = 4000)
@p3='Little spotted kiwi' (Size = 4000)
@p4=NULL (Size = 100)
@p5='True'
@p6='0' (Size = 1)

SET NOCOUNT ON;
INSERT INTO [Animal] ([Species], [CountryId], [Discriminator], [Name], [EagleId], [IsFlightless], [FoundOn])
VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6);",
                //
                @"SELECT TOP(2) [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[FoundOn]
FROM [Animal] AS [a]
WHERE ([a].[Discriminator] = N'Kiwi') AND ([a].[Species] LIKE N'%owenii')",
                //
                @"@p1='Apteryx owenii' (Nullable = false) (Size = 100)
@p0='Aquila chrysaetos canadensis' (Size = 100)

SET NOCOUNT ON;
UPDATE [Animal] SET [EagleId] = @p0
WHERE [Species] = @p1;
SELECT @@ROWCOUNT;",
                //
                @"SELECT TOP(2) [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[FoundOn]
FROM [Animal] AS [a]
WHERE ([a].[Discriminator] = N'Kiwi') AND ([a].[Species] LIKE N'%owenii')",
                //
                @"@p0='Apteryx owenii' (Nullable = false) (Size = 100)

SET NOCOUNT ON;
DELETE FROM [Animal]
WHERE [Species] = @p0;
SELECT @@ROWCOUNT;",
                //
                @"SELECT COUNT(*)
FROM [Animal] AS [a]
WHERE ([a].[Discriminator] = N'Kiwi') AND ([a].[Species] LIKE N'%owenii')");
        }

        public override void Byte_enum_value_constant_used_in_projection()
        {
            base.Byte_enum_value_constant_used_in_projection();

            AssertSql(
                @"SELECT CASE
    WHEN [a].[IsFlightless] = CAST(1 AS bit) THEN CAST(0 AS tinyint)
    ELSE CAST(1 AS tinyint)
END
FROM [Animal] AS [a]
WHERE [a].[Discriminator] = N'Kiwi'");
        }

        public override void Union_siblings_with_duplicate_property_in_subquery()
        {
            base.Union_siblings_with_duplicate_property_in_subquery();

            AssertSql(
                @"SELECT [t].[Id], [t].[Discriminator], [t].[CaffeineGrams], [t].[CokeCO2], [t].[SugarGrams], [t].[Carbonation], [t].[SugarGrams0], [t].[CaffeineGrams0], [t].[HasMilk]
FROM (
    SELECT [d].[Id], [d].[Discriminator], [d].[CaffeineGrams], [d].[CokeCO2], [d].[SugarGrams], NULL AS [CaffeineGrams0], NULL AS [HasMilk], NULL AS [Carbonation], NULL AS [SugarGrams0]
    FROM [Drink] AS [d]
    WHERE [d].[Discriminator] = N'Coke'
    UNION
    SELECT [d0].[Id], [d0].[Discriminator], NULL AS [CaffeineGrams], NULL AS [CokeCO2], NULL AS [SugarGrams], [d0].[CaffeineGrams] AS [CaffeineGrams0], [d0].[HasMilk], NULL AS [Carbonation], NULL AS [SugarGrams0]
    FROM [Drink] AS [d0]
    WHERE [d0].[Discriminator] = N'Tea'
) AS [t]
WHERE [t].[Id] > 0");
        }

        public override void OfType_Union_subquery()
        {
            base.OfType_Union_subquery();

            AssertSql(
                @"SELECT [t].[Species], [t].[CountryId], [t].[Discriminator], [t].[Name], [t].[EagleId], [t].[IsFlightless], [t].[FoundOn]
FROM (
    SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[FoundOn]
    FROM [Animal] AS [a]
    WHERE [a].[Discriminator] IN (N'Eagle', N'Kiwi') AND ([a].[Discriminator] = N'Kiwi')
    UNION
    SELECT [a0].[Species], [a0].[CountryId], [a0].[Discriminator], [a0].[Name], [a0].[EagleId], [a0].[IsFlightless], [a0].[FoundOn]
    FROM [Animal] AS [a0]
    WHERE [a0].[Discriminator] IN (N'Eagle', N'Kiwi') AND ([a0].[Discriminator] = N'Kiwi')
) AS [t]
WHERE ([t].[FoundOn] = CAST(0 AS tinyint)) AND [t].[FoundOn] IS NOT NULL");
        }

        public override void OfType_Union_OfType()
        {
            base.OfType_Union_OfType();

            AssertSql(" ");
        }

        public override void Subquery_OfType()
        {
            base.Subquery_OfType();

            AssertSql(
                @"@__p_0='5'

SELECT DISTINCT [t].[Species], [t].[CountryId], [t].[Discriminator], [t].[Name], [t].[EagleId], [t].[IsFlightless], [t].[FoundOn]
FROM (
    SELECT TOP(@__p_0) [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
    FROM [Animal] AS [a]
    WHERE [a].[Discriminator] IN (N'Eagle', N'Kiwi')
) AS [t]
WHERE [t].[Discriminator] = N'Kiwi'");
        }

        public override void Union_entity_equality()
        {
            base.Union_entity_equality();

            AssertSql(
                @"SELECT [t].[Species], [t].[CountryId], [t].[Discriminator], [t].[Name], [t].[EagleId], [t].[IsFlightless], [t].[Group], [t].[FoundOn]
FROM (
    SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[FoundOn], NULL AS [Group]
    FROM [Animal] AS [a]
    WHERE [a].[Discriminator] = N'Kiwi'
    UNION
    SELECT [a0].[Species], [a0].[CountryId], [a0].[Discriminator], [a0].[Name], [a0].[EagleId], [a0].[IsFlightless], NULL AS [FoundOn], [a0].[Group]
    FROM [Animal] AS [a0]
    WHERE [a0].[Discriminator] = N'Eagle'
) AS [t]
WHERE 0 = 1");
        }

        public override void Member_access_on_intermediate_type_works()
        {
            base.Member_access_on_intermediate_type_works();

            AssertSql(
                @"SELECT [a].[Name]
FROM [Animal] AS [a]
WHERE [a].[Discriminator] = N'Kiwi'
ORDER BY [a].[Name]");
        }

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
