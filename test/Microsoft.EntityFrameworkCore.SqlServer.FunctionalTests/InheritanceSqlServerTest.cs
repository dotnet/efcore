// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Inheritance;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class InheritanceSqlServerTest : InheritanceTestBase<InheritanceSqlServerFixture>
    {
        [Fact]
        public virtual void Common_property_shares_column()
        {
            using (var context = CreateContext())
            {
                var liltType = context.Model.FindEntityType(typeof(Lilt));
                var cokeType = context.Model.FindEntityType(typeof(Coke));
                var teaType = context.Model.FindEntityType(typeof(Tea));

                Assert.Equal("SugarGrams", cokeType.FindProperty("SugarGrams").Relational().ColumnName);
                Assert.Equal("CaffeineGrams", cokeType.FindProperty("CaffeineGrams").Relational().ColumnName);
                Assert.Equal("CokeCO2", cokeType.FindProperty("Carbination").Relational().ColumnName);

                Assert.Equal("SugarGrams", liltType.FindProperty("SugarGrams").Relational().ColumnName);
                Assert.Equal("LiltCO2", liltType.FindProperty("Carbination").Relational().ColumnName);

                Assert.Equal("CaffeineGrams", teaType.FindProperty("CaffeineGrams").Relational().ColumnName);
                Assert.Equal("HasMilk", teaType.FindProperty("HasMilk").Relational().ColumnName);
            }
        }

        [Fact]
        public override void Can_query_when_shared_column()
        {
            base.Can_query_when_shared_column();

            Assert.Equal(
                @"SELECT TOP(2) [d].[Id], [d].[Discriminator], [d].[CaffeineGrams], [d].[CokeCO2], [d].[SugarGrams]
FROM [Drink] AS [d]
WHERE [d].[Discriminator] = N'Coke'

SELECT TOP(2) [d].[Id], [d].[Discriminator], [d].[LiltCO2], [d].[SugarGrams]
FROM [Drink] AS [d]
WHERE [d].[Discriminator] = N'Lilt'

SELECT TOP(2) [d].[Id], [d].[Discriminator], [d].[CaffeineGrams], [d].[HasMilk]
FROM [Drink] AS [d]
WHERE [d].[Discriminator] = N'Tea'",
                Sql);
        }

        [Fact]
        public override void Can_query_all_types_when_shared_column()
        {
            base.Can_query_all_types_when_shared_column();

            Assert.Equal(
                @"SELECT [d].[Id], [d].[Discriminator], [d].[CaffeineGrams], [d].[CokeCO2], [d].[SugarGrams], [d].[LiltCO2], [d].[HasMilk]
FROM [Drink] AS [d]
WHERE [d].[Discriminator] IN (N'Tea', N'Lilt', N'Coke', N'Drink')",
                Sql);
        }

        public override void Can_use_of_type_animal()
        {
            base.Can_use_of_type_animal();

            Assert.Equal(
                @"SELECT [t].[Species], [t].[CountryId], [t].[Discriminator], [t].[Name], [t].[EagleId], [t].[IsFlightless], [t].[Group], [t].[FoundOn]
FROM (
    SELECT [a0].[Species], [a0].[CountryId], [a0].[Discriminator], [a0].[Name], [a0].[EagleId], [a0].[IsFlightless], [a0].[Group], [a0].[FoundOn]
    FROM [Animal] AS [a0]
    WHERE [a0].[Discriminator] IN (N'Kiwi', N'Eagle')
) AS [t]
ORDER BY [t].[Species]",
                Sql);
        }

        public override void Can_use_is_kiwi()
        {
            base.Can_use_is_kiwi();

            Assert.Equal(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animal] AS [a]
WHERE [a].[Discriminator] IN (N'Kiwi', N'Eagle') AND ([a].[Discriminator] = N'Kiwi')",
                Sql);
        }

        public override void Can_use_is_kiwi_with_other_predicate()
        {
            base.Can_use_is_kiwi_with_other_predicate();

            Assert.Equal(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animal] AS [a]
WHERE [a].[Discriminator] IN (N'Kiwi', N'Eagle') AND (([a].[Discriminator] = N'Kiwi') AND ([a].[CountryId] = 1))",
                Sql);
        }

        public override void Can_use_is_kiwi_in_projection()
        {
            base.Can_use_is_kiwi_in_projection();

            Assert.Equal(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animal] AS [a]
WHERE [a].[Discriminator] IN (N'Kiwi', N'Eagle')",
                Sql);
        }

        public override void Can_use_of_type_bird()
        {
            base.Can_use_of_type_bird();

            Assert.Equal(
                @"SELECT [t].[Species], [t].[CountryId], [t].[Discriminator], [t].[Name], [t].[EagleId], [t].[IsFlightless], [t].[Group], [t].[FoundOn]
FROM (
    SELECT [a0].[Species], [a0].[CountryId], [a0].[Discriminator], [a0].[Name], [a0].[EagleId], [a0].[IsFlightless], [a0].[Group], [a0].[FoundOn]
    FROM [Animal] AS [a0]
    WHERE [a0].[Discriminator] IN (N'Kiwi', N'Eagle')
) AS [t]
ORDER BY [t].[Species]",
                Sql);
        }

        public override void Can_use_of_type_bird_predicate()
        {
            base.Can_use_of_type_bird_predicate();

            Assert.Equal(
                @"SELECT [t].[Species], [t].[CountryId], [t].[Discriminator], [t].[Name], [t].[EagleId], [t].[IsFlightless], [t].[Group], [t].[FoundOn]
FROM (
    SELECT [a0].[Species], [a0].[CountryId], [a0].[Discriminator], [a0].[Name], [a0].[EagleId], [a0].[IsFlightless], [a0].[Group], [a0].[FoundOn]
    FROM [Animal] AS [a0]
    WHERE [a0].[Discriminator] IN (N'Kiwi', N'Eagle') AND ([a0].[CountryId] = 1)
) AS [t]
ORDER BY [t].[Species]",
                Sql);
        }

        public override void Can_use_of_type_bird_with_projection()
        {
            base.Can_use_of_type_bird_with_projection();

            Assert.Equal(
                @"SELECT [t].[EagleId]
FROM (
    SELECT [a0].[Species], [a0].[CountryId], [a0].[Discriminator], [a0].[Name], [a0].[EagleId], [a0].[IsFlightless], [a0].[Group], [a0].[FoundOn]
    FROM [Animal] AS [a0]
    WHERE [a0].[Discriminator] IN (N'Kiwi', N'Eagle')
) AS [t]",
                Sql);
        }

        public override void Can_use_of_type_bird_first()
        {
            base.Can_use_of_type_bird_first();

            Assert.Equal(
                @"SELECT TOP(1) [t].[Species], [t].[CountryId], [t].[Discriminator], [t].[Name], [t].[EagleId], [t].[IsFlightless], [t].[Group], [t].[FoundOn]
FROM (
    SELECT [a0].[Species], [a0].[CountryId], [a0].[Discriminator], [a0].[Name], [a0].[EagleId], [a0].[IsFlightless], [a0].[Group], [a0].[FoundOn]
    FROM [Animal] AS [a0]
    WHERE [a0].[Discriminator] IN (N'Kiwi', N'Eagle')
) AS [t]
ORDER BY [t].[Species]",
                Sql);
        }

        public override void Can_use_of_type_kiwi()
        {
            base.Can_use_of_type_kiwi();

            Assert.Equal(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animal] AS [a]
WHERE [a].[Discriminator] = N'Kiwi'",
                Sql);
        }

        public override void Can_use_of_type_rose()
        {
            base.Can_use_of_type_rose();

            Assert.Equal(
                @"SELECT [p].[Species], [p].[CountryId], [p].[Genus], [p].[Name], [p].[HasThorns]
FROM [Plant] AS [p]
WHERE [p].[Genus] = 0",
                Sql);
        }

        public override void Can_query_all_animals()
        {
            base.Can_query_all_animals();

            Assert.Equal(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animal] AS [a]
WHERE [a].[Discriminator] IN (N'Kiwi', N'Eagle')
ORDER BY [a].[Species]",
                Sql);
        }

        public override void Can_query_all_plants()
        {
            base.Can_query_all_plants();

            Assert.Equal(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Genus], [a].[Name], [a].[HasThorns]
FROM [Plant] AS [a]
WHERE [a].[Genus] IN (0, 1)
ORDER BY [a].[Species]",
                Sql);
        }

        public override void Can_filter_all_animals()
        {
            base.Can_filter_all_animals();

            Assert.Equal(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animal] AS [a]
WHERE [a].[Discriminator] IN (N'Kiwi', N'Eagle') AND ([a].[Name] = N'Great spotted kiwi')
ORDER BY [a].[Species]",
                Sql);
        }

        public override void Can_query_all_birds()
        {
            base.Can_query_all_birds();

            Assert.Equal(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animal] AS [a]
WHERE [a].[Discriminator] IN (N'Kiwi', N'Eagle')
ORDER BY [a].[Species]",
                Sql);
        }

        public override void Can_query_just_kiwis()
        {
            base.Can_query_just_kiwis();

            Assert.Equal(
                @"SELECT TOP(2) [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[FoundOn]
FROM [Animal] AS [a]
WHERE [a].[Discriminator] = N'Kiwi'",
                Sql);
        }

        public override void Can_query_just_roses()
        {
            base.Can_query_just_roses();

            Assert.Equal(
                @"SELECT TOP(2) [p].[Species], [p].[CountryId], [p].[Genus], [p].[Name], [p].[HasThorns]
FROM [Plant] AS [p]
WHERE [p].[Genus] = 0",
                Sql
            );
        }

        public override void Can_include_prey()
        {
            base.Can_include_prey();

            Assert.Equal(
                @"SELECT TOP(2) [e].[Species], [e].[CountryId], [e].[Discriminator], [e].[Name], [e].[EagleId], [e].[IsFlightless], [e].[Group]
FROM [Animal] AS [e]
WHERE [e].[Discriminator] = N'Eagle'
ORDER BY [e].[Species]

SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animal] AS [a]
INNER JOIN (
    SELECT DISTINCT TOP(2) [e].[Species]
    FROM [Animal] AS [e]
    WHERE [e].[Discriminator] = N'Eagle'
    ORDER BY [e].[Species]
) AS [e0] ON [a].[EagleId] = [e0].[Species]
WHERE [a].[Discriminator] IN (N'Kiwi', N'Eagle')
ORDER BY [e0].[Species]",
                Sql);
        }

        public override void Can_include_animals()
        {
            base.Can_include_animals();

            Assert.Equal(
                @"SELECT [c].[Id], [c].[Name]
FROM [Country] AS [c]
ORDER BY [c].[Name], [c].[Id]

SELECT [a].[Species], [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animal] AS [a]
INNER JOIN (
    SELECT DISTINCT [c].[Name], [c].[Id]
    FROM [Country] AS [c]
) AS [c0] ON [a].[CountryId] = [c0].[Id]
WHERE [a].[Discriminator] IN (N'Kiwi', N'Eagle')
ORDER BY [c0].[Name], [c0].[Id]",
                Sql);
        }

        public override void Discriminator_used_when_projection_over_derived_type()
        {
            base.Discriminator_used_when_projection_over_derived_type();

            Assert.Equal(
                @"SELECT [k].[FoundOn]
FROM [Animal] AS [k]
WHERE [k].[Discriminator] = N'Kiwi'",
                Sql);
        }

        public override void Discriminator_used_when_projection_over_derived_type2()
        {
            base.Discriminator_used_when_projection_over_derived_type2();

            Assert.Equal(
                @"SELECT [b].[IsFlightless], [b].[Discriminator]
FROM [Animal] AS [b]
WHERE [b].[Discriminator] IN (N'Kiwi', N'Eagle')",
                Sql);
        }

        public override void Discriminator_used_when_projection_over_of_type()
        {
            base.Discriminator_used_when_projection_over_of_type();

            Assert.Equal(
                @"SELECT [t].[FoundOn]
FROM (
    SELECT [a0].[Species], [a0].[CountryId], [a0].[Discriminator], [a0].[Name], [a0].[EagleId], [a0].[IsFlightless], [a0].[Group], [a0].[FoundOn]
    FROM [Animal] AS [a0]
    WHERE [a0].[Discriminator] = N'Kiwi'
) AS [t]",
                Sql);
        }

        public override void Can_insert_update_delete()
        {
            base.Can_insert_update_delete();

            Assert.Equal(
                @"SELECT TOP(2) [c].[Id], [c].[Name]
FROM [Country] AS [c]
WHERE [c].[Id] = 1

@p0: Apteryx owenii (Nullable = false) (Size = 100)
@p1: 1
@p2: Kiwi (Nullable = false) (Size = 4000)
@p3: Little spotted kiwi (Size = 4000)
@p4:  (Size = 100) (DbType = String)
@p5: True
@p6: North

SET NOCOUNT ON;
INSERT INTO [Animal] ([Species], [CountryId], [Discriminator], [Name], [EagleId], [IsFlightless], [FoundOn])
VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6);

SELECT TOP(2) [k].[Species], [k].[CountryId], [k].[Discriminator], [k].[Name], [k].[EagleId], [k].[IsFlightless], [k].[FoundOn]
FROM [Animal] AS [k]
WHERE ([k].[Discriminator] = N'Kiwi') AND (RIGHT([k].[Species], LEN(N'owenii')) = N'owenii')

@p1: Apteryx owenii (Nullable = false) (Size = 100)
@p0: Aquila chrysaetos canadensis (Size = 100)

SET NOCOUNT ON;
UPDATE [Animal] SET [EagleId] = @p0
WHERE [Species] = @p1;
SELECT @@ROWCOUNT;

SELECT TOP(2) [k].[Species], [k].[CountryId], [k].[Discriminator], [k].[Name], [k].[EagleId], [k].[IsFlightless], [k].[FoundOn]
FROM [Animal] AS [k]
WHERE ([k].[Discriminator] = N'Kiwi') AND (RIGHT([k].[Species], LEN(N'owenii')) = N'owenii')

@p0: Apteryx owenii (Nullable = false) (Size = 100)

SET NOCOUNT ON;
DELETE FROM [Animal]
WHERE [Species] = @p0;
SELECT @@ROWCOUNT;

SELECT COUNT(*)
FROM [Animal] AS [k]
WHERE ([k].[Discriminator] = N'Kiwi') AND (RIGHT([k].[Species], LEN(N'owenii')) = N'owenii')",
                Sql);
        }

        public InheritanceSqlServerTest(InheritanceSqlServerFixture fixture)
            : base(fixture)
        {
        }

        private const string FileLineEnding = @"
";

        private static string Sql => TestSqlLoggerFactory.Sql.Replace(Environment.NewLine, FileLineEnding);
    }
}
