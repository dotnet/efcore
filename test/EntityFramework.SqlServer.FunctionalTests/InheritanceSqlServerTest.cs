// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Relational.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class InheritanceSqlServerTest : InheritanceTestBase<InheritanceSqlServerFixture>
    {
        public override void Can_use_of_type_animal()
        {
            base.Can_use_of_type_animal();

            Assert.Equal(
                @"SELECT [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animal] AS [a]
WHERE ([a].[Discriminator] = 'Kiwi' OR [a].[Discriminator] = 'Eagle')
ORDER BY [a].[Species]",
                Sql);
        }

        public override void Can_use_of_type_bird()
        {
            base.Can_use_of_type_bird();

            Assert.Equal(
                @"SELECT [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animal] AS [a]
WHERE ([a].[Discriminator] = 'Kiwi' OR [a].[Discriminator] = 'Eagle')
ORDER BY [a].[Species]",
                Sql);
        }

        public override void Can_use_of_type_kiwi()
        {
            base.Can_use_of_type_kiwi();

            Assert.Equal(
                @"SELECT [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animal] AS [a]
WHERE [a].[Discriminator] = 'Kiwi'",
                Sql);
        }

        public override void Can_query_all_animals()
        {
            base.Can_query_all_animals();

            Assert.Equal(
                @"SELECT [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animal] AS [a]
WHERE ([a].[Discriminator] = 'Kiwi' OR [a].[Discriminator] = 'Eagle')
ORDER BY [a].[Species]",
                Sql);
        }

        public override void Can_filter_all_animals()
        {
            base.Can_filter_all_animals();

            Assert.Equal(
                @"SELECT [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animal] AS [a]
WHERE (([a].[Discriminator] = 'Kiwi' OR [a].[Discriminator] = 'Eagle') AND [a].[Name] = 'Great spotted kiwi')
ORDER BY [a].[Species]",
                Sql);
        }

        public override void Can_query_all_birds()
        {
            base.Can_query_all_birds();

            Assert.Equal(
                @"SELECT [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animal] AS [a]
WHERE ([a].[Discriminator] = 'Kiwi' OR [a].[Discriminator] = 'Eagle')
ORDER BY [a].[Species]",
                Sql);
        }

        public override void Can_query_just_kiwis()
        {
            base.Can_query_just_kiwis();

            Assert.Equal(
                @"SELECT TOP(2) [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[FoundOn]
FROM [Animal] AS [a]
WHERE [a].[Discriminator] = 'Kiwi'",
                Sql);
        }

        public override void Can_include_prey()
        {
            base.Can_include_prey();

            Assert.Equal(
                @"SELECT TOP(2) [e].[CountryId], [e].[Discriminator], [e].[Name], [e].[Species], [e].[EagleId], [e].[IsFlightless], [e].[Group]
FROM [Animal] AS [e]
WHERE [e].[Discriminator] = 'Eagle'
ORDER BY [e].[Species]

SELECT [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animal] AS [a]
INNER JOIN (
    SELECT DISTINCT TOP(2) [e].[Species]
    FROM [Animal] AS [e]
    WHERE [e].[Discriminator] = 'Eagle'
) AS [e] ON [a].[EagleId] = [e].[Species]
WHERE ([a].[Discriminator] = 'Kiwi' OR [a].[Discriminator] = 'Eagle')
ORDER BY [e].[Species]",
                Sql);
        }

        public override void Can_include_animals()
        {
            base.Can_include_animals();

            Assert.Equal(
                @"SELECT [c].[Id], [c].[Name]
FROM [Country] AS [c]
ORDER BY [c].[Name], [c].[Id]

SELECT [a].[CountryId], [a].[Discriminator], [a].[Name], [a].[Species], [a].[EagleId], [a].[IsFlightless], [a].[Group], [a].[FoundOn]
FROM [Animal] AS [a]
INNER JOIN (
    SELECT DISTINCT [c].[Name], [c].[Id]
    FROM [Country] AS [c]
) AS [c] ON [a].[CountryId] = [c].[Id]
WHERE ([a].[Discriminator] = 'Kiwi' OR [a].[Discriminator] = 'Eagle')
ORDER BY [c].[Name], [c].[Id]",
                Sql);
        }

        public InheritanceSqlServerTest(InheritanceSqlServerFixture fixture)
            : base(fixture)
        {
        }

        private static string Sql => TestSqlLoggerFactory.Sql;
    }
}
