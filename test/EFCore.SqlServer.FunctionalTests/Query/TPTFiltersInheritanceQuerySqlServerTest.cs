// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class TPTFiltersInheritanceQuerySqlServerTest : TPTFiltersInheritanceQueryTestBase<TPTFiltersInheritanceQuerySqlServerFixture>
    {
        public TPTFiltersInheritanceQuerySqlServerTest(
            TPTFiltersInheritanceQuerySqlServerFixture fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override void Can_use_of_type_animal()
        {
            base.Can_use_of_type_animal();

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [k].[Species] IS NOT NULL THEN N'Kiwi'
    WHEN [e].[Species] IS NOT NULL THEN N'Eagle'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
LEFT JOIN [Eagle] AS [e] ON [a].[Species] = [e].[Species]
LEFT JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
WHERE [a].[CountryId] = 1
ORDER BY [a].[Species]");
        }

        public override void Can_use_is_kiwi()
        {
            base.Can_use_is_kiwi();

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [k].[Species] IS NOT NULL THEN N'Kiwi'
    WHEN [e].[Species] IS NOT NULL THEN N'Eagle'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
LEFT JOIN [Eagle] AS [e] ON [a].[Species] = [e].[Species]
LEFT JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
WHERE ([a].[CountryId] = 1) AND [k].[Species] IS NOT NULL");
        }

        public override void Can_use_is_kiwi_with_other_predicate()
        {
            base.Can_use_is_kiwi_with_other_predicate();

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [k].[Species] IS NOT NULL THEN N'Kiwi'
    WHEN [e].[Species] IS NOT NULL THEN N'Eagle'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
LEFT JOIN [Eagle] AS [e] ON [a].[Species] = [e].[Species]
LEFT JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
WHERE ([a].[CountryId] = 1) AND ([k].[Species] IS NOT NULL AND ([a].[CountryId] = 1))");
        }

        public override void Can_use_is_kiwi_in_projection()
        {
            base.Can_use_is_kiwi_in_projection();

            AssertSql(
                @"SELECT CASE
    WHEN [k].[Species] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Animals] AS [a]
LEFT JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
WHERE [a].[CountryId] = 1");
        }

        public override void Can_use_of_type_bird()
        {
            base.Can_use_of_type_bird();

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

        public override void Can_use_of_type_bird_predicate()
        {
            base.Can_use_of_type_bird_predicate();

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [k].[Species] IS NOT NULL THEN N'Kiwi'
    WHEN [e].[Species] IS NOT NULL THEN N'Eagle'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
LEFT JOIN [Eagle] AS [e] ON [a].[Species] = [e].[Species]
LEFT JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
WHERE (([a].[CountryId] = 1) AND ([a].[CountryId] = 1)) AND ([k].[Species] IS NOT NULL OR [e].[Species] IS NOT NULL)
ORDER BY [a].[Species]");
        }

        public override void Can_use_of_type_bird_with_projection()
        {
            base.Can_use_of_type_bird_with_projection();

            AssertSql(
                @"SELECT [b].[EagleId]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
LEFT JOIN [Eagle] AS [e] ON [a].[Species] = [e].[Species]
LEFT JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
WHERE ([a].[CountryId] = 1) AND ([k].[Species] IS NOT NULL OR [e].[Species] IS NOT NULL)");
        }

        public override void Can_use_of_type_bird_first()
        {
            base.Can_use_of_type_bird_first();

            AssertSql(
                @"SELECT TOP(1) [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
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

        public override void Can_use_of_type_kiwi()
        {
            base.Can_use_of_type_kiwi();

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [k].[FoundOn], CASE
    WHEN [k].[Species] IS NOT NULL THEN N'Kiwi'
END AS [Discriminator]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
LEFT JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
WHERE ([a].[CountryId] = 1) AND [k].[Species] IS NOT NULL");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
