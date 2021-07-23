﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
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
WHERE [a].[CountryId] = 1
ORDER BY [a].[Species]");
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
WHERE ([a].[CountryId] = 1) AND [k].[Species] IS NOT NULL");
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
WHERE ([a].[CountryId] = 1) AND ([k].[Species] IS NOT NULL AND ([a].[CountryId] = 1))");
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
LEFT JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
WHERE [a].[CountryId] = 1");
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
WHERE ([a].[CountryId] = 1) AND ([k].[Species] IS NOT NULL OR [e].[Species] IS NOT NULL)
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
WHERE (([a].[CountryId] = 1) AND ([a].[CountryId] = 1)) AND ([k].[Species] IS NOT NULL OR [e].[Species] IS NOT NULL)
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
WHERE ([a].[CountryId] = 1) AND ([k].[Species] IS NOT NULL OR [e].[Species] IS NOT NULL)");
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
WHERE ([a].[CountryId] = 1) AND ([k].[Species] IS NOT NULL OR [e].[Species] IS NOT NULL)
ORDER BY [a].[Species]");
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
WHERE ([a].[CountryId] = 1) AND [k].[Species] IS NOT NULL");
        }

        public override async Task Can_use_derived_set(bool async)
        {
            await base.Can_use_derived_set(async);

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [e].[Group]
FROM [Animals] AS [a]
INNER JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
INNER JOIN [Eagle] AS [e] ON [a].[Species] = [e].[Species]
WHERE [a].[CountryId] = 1");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
