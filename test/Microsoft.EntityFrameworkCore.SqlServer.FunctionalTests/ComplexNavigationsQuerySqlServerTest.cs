// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.FunctionalTests;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class ComplexNavigationsQuerySqlServerTest : ComplexNavigationsQueryTestBase<SqlServerTestStore, ComplexNavigationsQuerySqlServerFixture>
    {
        public ComplexNavigationsQuerySqlServerTest(ComplexNavigationsQuerySqlServerFixture fixture)
            : base(fixture)
        {
        }

        protected override void ClearLog() => TestSqlLoggerFactory.Reset();

        public override void Multi_level_include_one_to_many_optional_and_one_to_many_optional_produces_valid_sql()
        {
            base.Multi_level_include_one_to_many_optional_and_one_to_many_optional_produces_valid_sql();

            Assert.Equal(
                @"SELECT [e].[Id], [e].[Name], [e].[OneToMany_Optional_Self_InverseId], [e].[OneToMany_Required_Self_InverseId], [e].[OneToOne_Optional_SelfId]
FROM [Level1] AS [e]
ORDER BY [e].[Id]

SELECT [l].[Id], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Name], [l].[OneToMany_Optional_InverseId], [l].[OneToMany_Optional_Self_InverseId], [l].[OneToMany_Required_InverseId], [l].[OneToMany_Required_Self_InverseId], [l].[OneToOne_Optional_PK_InverseId], [l].[OneToOne_Optional_SelfId]
FROM [Level2] AS [l]
INNER JOIN (
    SELECT DISTINCT [e].[Id]
    FROM [Level1] AS [e]
) AS [e] ON [l].[OneToMany_Optional_InverseId] = [e].[Id]
ORDER BY [e].[Id], [l].[Id]

SELECT [l].[Id], [l].[Level2_Optional_Id], [l].[Level2_Required_Id], [l].[Name], [l].[OneToMany_Optional_InverseId], [l].[OneToMany_Optional_Self_InverseId], [l].[OneToMany_Required_InverseId], [l].[OneToMany_Required_Self_InverseId], [l].[OneToOne_Optional_PK_InverseId], [l].[OneToOne_Optional_SelfId]
FROM [Level3] AS [l]
INNER JOIN (
    SELECT DISTINCT [e].[Id], [l].[Id] AS [Id0]
    FROM [Level2] AS [l]
    INNER JOIN (
        SELECT DISTINCT [e].[Id]
        FROM [Level1] AS [e]
    ) AS [e] ON [l].[OneToMany_Optional_InverseId] = [e].[Id]
) AS [l0] ON [l].[OneToMany_Optional_InverseId] = [l0].[Id0]
ORDER BY [l0].[Id], [l0].[Id0]", 
                Sql);
        }

        public override void Multi_level_include_correct_PK_is_chosen_as_the_join_predicate_for_queries_that_join_same_table_multiple_times()
        {
            base.Multi_level_include_correct_PK_is_chosen_as_the_join_predicate_for_queries_that_join_same_table_multiple_times();

            Assert.Equal(
                @"SELECT [e].[Id], [e].[Name], [e].[OneToMany_Optional_Self_InverseId], [e].[OneToMany_Required_Self_InverseId], [e].[OneToOne_Optional_SelfId]
FROM [Level1] AS [e]
ORDER BY [e].[Id]

SELECT [l].[Id], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Name], [l].[OneToMany_Optional_InverseId], [l].[OneToMany_Optional_Self_InverseId], [l].[OneToMany_Required_InverseId], [l].[OneToMany_Required_Self_InverseId], [l].[OneToOne_Optional_PK_InverseId], [l].[OneToOne_Optional_SelfId]
FROM [Level2] AS [l]
INNER JOIN (
    SELECT DISTINCT [e].[Id]
    FROM [Level1] AS [e]
) AS [e] ON [l].[OneToMany_Optional_InverseId] = [e].[Id]
ORDER BY [e].[Id], [l].[Id]

SELECT [l].[Id], [l].[Level2_Optional_Id], [l].[Level2_Required_Id], [l].[Name], [l].[OneToMany_Optional_InverseId], [l].[OneToMany_Optional_Self_InverseId], [l].[OneToMany_Required_InverseId], [l].[OneToMany_Required_Self_InverseId], [l].[OneToOne_Optional_PK_InverseId], [l].[OneToOne_Optional_SelfId], [l1].[Id], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_InverseId], [l1].[OneToMany_Optional_Self_InverseId], [l1].[OneToMany_Required_InverseId], [l1].[OneToMany_Required_Self_InverseId], [l1].[OneToOne_Optional_PK_InverseId], [l1].[OneToOne_Optional_SelfId]
FROM [Level3] AS [l]
INNER JOIN (
    SELECT DISTINCT [e].[Id], [l].[Id] AS [Id0]
    FROM [Level2] AS [l]
    INNER JOIN (
        SELECT DISTINCT [e].[Id]
        FROM [Level1] AS [e]
    ) AS [e] ON [l].[OneToMany_Optional_InverseId] = [e].[Id]
) AS [l0] ON [l].[OneToMany_Optional_InverseId] = [l0].[Id0]
INNER JOIN [Level2] AS [l1] ON [l].[OneToMany_Required_InverseId] = [l1].[Id]
ORDER BY [l0].[Id], [l0].[Id0], [l1].[Id]

SELECT [l].[Id], [l].[Level2_Optional_Id], [l].[Level2_Required_Id], [l].[Name], [l].[OneToMany_Optional_InverseId], [l].[OneToMany_Optional_Self_InverseId], [l].[OneToMany_Required_InverseId], [l].[OneToMany_Required_Self_InverseId], [l].[OneToOne_Optional_PK_InverseId], [l].[OneToOne_Optional_SelfId]
FROM [Level3] AS [l]
INNER JOIN (
    SELECT DISTINCT [l0].[Id], [l0].[Id0], [l1].[Id] AS [Id1]
    FROM [Level3] AS [l]
    INNER JOIN (
        SELECT DISTINCT [e].[Id], [l].[Id] AS [Id0]
        FROM [Level2] AS [l]
        INNER JOIN (
            SELECT DISTINCT [e].[Id]
            FROM [Level1] AS [e]
        ) AS [e] ON [l].[OneToMany_Optional_InverseId] = [e].[Id]
    ) AS [l0] ON [l].[OneToMany_Optional_InverseId] = [l0].[Id0]
    INNER JOIN [Level2] AS [l1] ON [l].[OneToMany_Required_InverseId] = [l1].[Id]
) AS [l1] ON [l].[OneToMany_Optional_InverseId] = [l1].[Id1]
ORDER BY [l1].[Id], [l1].[Id0], [l1].[Id1]", 
                Sql);
        }

        public override void Multi_level_include_with_short_circuiting()
        {
            base.Multi_level_include_with_short_circuiting();

            Assert.Equal(
                @"SELECT [x].[Name], [x].[LabelDefaultText], [x].[PlaceholderDefaultText], [c].[DefaultText], [c0].[DefaultText]
FROM [ComplexNavigationField] AS [x]
LEFT JOIN [ComplexNavigationString] AS [c] ON [x].[LabelDefaultText] = [c].[DefaultText]
LEFT JOIN [ComplexNavigationString] AS [c0] ON [x].[PlaceholderDefaultText] = [c0].[DefaultText]
ORDER BY [c].[DefaultText], [c0].[DefaultText]

SELECT [c].[Text], [c].[ComplexNavigationStringDefaultText], [c].[LanguageName], [c1].[Name], [c1].[CultureString]
FROM [ComplexNavigationGlobalization] AS [c]
INNER JOIN (
    SELECT DISTINCT [c].[DefaultText]
    FROM [ComplexNavigationField] AS [x]
    LEFT JOIN [ComplexNavigationString] AS [c] ON [x].[LabelDefaultText] = [c].[DefaultText]
) AS [c0] ON [c].[ComplexNavigationStringDefaultText] = [c0].[DefaultText]
LEFT JOIN [ComplexNavigationLanguage] AS [c1] ON [c].[LanguageName] = [c1].[Name]
ORDER BY [c0].[DefaultText]

SELECT [c].[Text], [c].[ComplexNavigationStringDefaultText], [c].[LanguageName], [c1].[Name], [c1].[CultureString]
FROM [ComplexNavigationGlobalization] AS [c]
INNER JOIN (
    SELECT DISTINCT [c].[DefaultText], [c0].[DefaultText] AS [DefaultText0]
    FROM [ComplexNavigationField] AS [x]
    LEFT JOIN [ComplexNavigationString] AS [c] ON [x].[LabelDefaultText] = [c].[DefaultText]
    LEFT JOIN [ComplexNavigationString] AS [c0] ON [x].[PlaceholderDefaultText] = [c0].[DefaultText]
) AS [c0] ON [c].[ComplexNavigationStringDefaultText] = [c0].[DefaultText0]
LEFT JOIN [ComplexNavigationLanguage] AS [c1] ON [c].[LanguageName] = [c1].[Name]
ORDER BY [c0].[DefaultText], [c0].[DefaultText0]", 
                Sql);
        }

        public override void Join_navigation_key_access_optional()
        {
            base.Join_navigation_key_access_optional();

            Assert.Equal(
                @"", Sql);
        }

        public override void Join_navigation_key_access_required()
        {
            base.Join_navigation_key_access_required();

            Assert.Equal(
                @"SELECT [e1].[Id], [e2].[Id]
FROM [Level1] AS [e1]
INNER JOIN [Level2] AS [e2] ON [e1].[Id] = [e2].[Level1_Required_Id]", Sql);
        }

        public override void Navigation_key_access_optional_comparison()
        {
            base.Navigation_key_access_optional_comparison();

            Assert.Equal(
                @"", Sql);
        }

        public override void Navigation_key_access_required_comparison()
        {
            base.Navigation_key_access_required_comparison();

            Assert.Equal(
                @"SELECT [e2].[Id]
FROM [Level2] AS [e2]
WHERE [e2].[Id] > 5", Sql);
        }

        public override void Join_navigation_in_outer_selector_translated_to_extra_join()
        {
            base.Join_navigation_in_outer_selector_translated_to_extra_join();

            Assert.Equal(
                @"SELECT [e1].[Id], [e2].[Id]
FROM [Level1] AS [e1]
INNER JOIN [Level2] AS [e1.OneToOne_Optional_FK] ON [e1].[Id] = [e1.OneToOne_Optional_FK].[Level1_Optional_Id]
INNER JOIN [Level2] AS [e2] ON [e1.OneToOne_Optional_FK].[Id] = [e2].[Id]",
                Sql);
        }

        public override void Join_navigation_in_outer_selector_translated_to_extra_join_nested()
        {
            base.Join_navigation_in_outer_selector_translated_to_extra_join_nested();

            Assert.Equal(
                @"SELECT [e1].[Id], [e3].[Id]
FROM [Level1] AS [e1]
INNER JOIN [Level2] AS [e1.OneToOne_Required_FK] ON [e1].[Id] = [e1.OneToOne_Required_FK].[Level1_Required_Id]
INNER JOIN [Level3] AS [e1.OneToOne_Required_FK.OneToOne_Optional_FK] ON [e1.OneToOne_Required_FK].[Id] = [e1.OneToOne_Required_FK.OneToOne_Optional_FK].[Level2_Optional_Id]
INNER JOIN [Level3] AS [e3] ON [e1.OneToOne_Required_FK.OneToOne_Optional_FK].[Id] = [e3].[Id]",
                Sql);
        }


        public override void Join_navigation_in_inner_selector_translated_to_subquery()
        {
            base.Join_navigation_in_inner_selector_translated_to_subquery();

            Assert.Equal(
                @"SELECT [e2].[Id], [e1].[Id]
FROM [Level2] AS [e2]
INNER JOIN [Level1] AS [e1] ON [e2].[Id] = (
    SELECT TOP(1) [subQuery0].[Id]
    FROM [Level2] AS [subQuery0]
    WHERE [subQuery0].[Level1_Optional_Id] = [e1].[Id]
)",
                Sql);
        }

        public override void Join_navigation_translated_to_subquery_non_key_join()
        {
            base.Join_navigation_translated_to_subquery_non_key_join();

            Assert.Equal(
                @"SELECT [e2].[Id], [e2].[Name], [e1].[Id], [e1].[Name]
FROM [Level2] AS [e2]
INNER JOIN [Level1] AS [e1] ON [e2].[Name] = (
    SELECT TOP(1) [subQuery0].[Name]
    FROM [Level2] AS [subQuery0]
    WHERE [subQuery0].[Level1_Optional_Id] = [e1].[Id]
)",
                Sql);
        }

        public override void Join_navigation_translated_to_subquery_self_ref()
        {
            base.Join_navigation_translated_to_subquery_self_ref();
            
            Assert.Equal(
                @"SELECT [e1].[Id], [e2].[Id]
FROM [Level1] AS [e1]
INNER JOIN [Level1] AS [e2] ON [e1].[Id] = (
    SELECT TOP(1) [subQuery0].[Id]
    FROM [Level1] AS [subQuery0]
    WHERE [subQuery0].[Id] = [e2].[OneToMany_Optional_Self_InverseId]
)",
                Sql);
        }

        public override void Join_navigation_translated_to_subquery_nested()
        {
            base.Join_navigation_translated_to_subquery_nested();

            Assert.Equal(
                @"SELECT [e3].[Id], [e1].[Id]
FROM [Level3] AS [e3]
INNER JOIN [Level1] AS [e1] ON [e3].[Id] = (
    SELECT TOP(1) [subQuery0.OneToOne_Optional_FK].[Id]
    FROM [Level2] AS [subQuery0]
    INNER JOIN [Level3] AS [subQuery0.OneToOne_Optional_FK] ON [subQuery0].[Id] = [subQuery0.OneToOne_Optional_FK].[Level2_Optional_Id]
    WHERE [subQuery0].[Level1_Required_Id] = [e1].[Id]
)",
                Sql);
        }

        public override void Join_navigation_translated_to_subquery_deeply_nested_non_key_join()
        {
            base.Join_navigation_translated_to_subquery_deeply_nested_non_key_join();

            Assert.Equal(
                @"SELECT [e4].[Id], [e4].[Name], [e1].[Id], [e1].[Name]
FROM [Level4] AS [e4]
INNER JOIN [Level1] AS [e1] ON [e4].[Name] = (
    SELECT TOP(1) [subQuery0.OneToOne_Optional_FK.OneToOne_Required_PK].[Name]
    FROM [Level2] AS [subQuery0]
    INNER JOIN [Level3] AS [subQuery0.OneToOne_Optional_FK] ON [subQuery0].[Id] = [subQuery0.OneToOne_Optional_FK].[Level2_Optional_Id]
    INNER JOIN [Level4] AS [subQuery0.OneToOne_Optional_FK.OneToOne_Required_PK] ON [subQuery0.OneToOne_Optional_FK].[Id] = [subQuery0.OneToOne_Optional_FK.OneToOne_Required_PK].[Id]
    WHERE [subQuery0].[Level1_Required_Id] = [e1].[Id]
)",
                Sql);
        }

        public override void Multiple_complex_includes()
        {
            base.Multiple_complex_includes();

            Assert.Equal(
                @"SELECT [e].[Id], [e].[Name], [e].[OneToMany_Optional_Self_InverseId], [e].[OneToMany_Required_Self_InverseId], [e].[OneToOne_Optional_SelfId], [l].[Id], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Name], [l].[OneToMany_Optional_InverseId], [l].[OneToMany_Optional_Self_InverseId], [l].[OneToMany_Required_InverseId], [l].[OneToMany_Required_Self_InverseId], [l].[OneToOne_Optional_PK_InverseId], [l].[OneToOne_Optional_SelfId]
FROM [Level1] AS [e]
LEFT JOIN [Level2] AS [l] ON [l].[Level1_Optional_Id] = [e].[Id]
ORDER BY [e].[Id], [l].[Id]

SELECT [l].[Id], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Name], [l].[OneToMany_Optional_InverseId], [l].[OneToMany_Optional_Self_InverseId], [l].[OneToMany_Required_InverseId], [l].[OneToMany_Required_Self_InverseId], [l].[OneToOne_Optional_PK_InverseId], [l].[OneToOne_Optional_SelfId], [l0].[Id], [l0].[Level2_Optional_Id], [l0].[Level2_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_InverseId], [l0].[OneToMany_Optional_Self_InverseId], [l0].[OneToMany_Required_InverseId], [l0].[OneToMany_Required_Self_InverseId], [l0].[OneToOne_Optional_PK_InverseId], [l0].[OneToOne_Optional_SelfId]
FROM [Level2] AS [l]
INNER JOIN (
    SELECT DISTINCT [e].[Id]
    FROM [Level1] AS [e]
) AS [e] ON [l].[OneToMany_Optional_InverseId] = [e].[Id]
LEFT JOIN [Level3] AS [l0] ON [l0].[Level2_Optional_Id] = [l].[Id]
ORDER BY [e].[Id]

SELECT [l].[Id], [l].[Level2_Optional_Id], [l].[Level2_Required_Id], [l].[Name], [l].[OneToMany_Optional_InverseId], [l].[OneToMany_Optional_Self_InverseId], [l].[OneToMany_Required_InverseId], [l].[OneToMany_Required_Self_InverseId], [l].[OneToOne_Optional_PK_InverseId], [l].[OneToOne_Optional_SelfId]
FROM [Level3] AS [l]
INNER JOIN (
    SELECT DISTINCT [e].[Id], [l].[Id] AS [Id0]
    FROM [Level1] AS [e]
    LEFT JOIN [Level2] AS [l] ON [l].[Level1_Optional_Id] = [e].[Id]
) AS [l0] ON [l].[OneToMany_Optional_InverseId] = [l0].[Id0]
ORDER BY [l0].[Id], [l0].[Id0]",
                Sql);
        }

        public override void Multiple_complex_includes_self_ref()
        {
            base.Multiple_complex_includes_self_ref();

            Assert.Equal(
                @"SELECT [e].[Id], [e].[Name], [e].[OneToMany_Optional_Self_InverseId], [e].[OneToMany_Required_Self_InverseId], [e].[OneToOne_Optional_SelfId], [l].[Id], [l].[Name], [l].[OneToMany_Optional_Self_InverseId], [l].[OneToMany_Required_Self_InverseId], [l].[OneToOne_Optional_SelfId]
FROM [Level1] AS [e]
LEFT JOIN [Level1] AS [l] ON [e].[OneToOne_Optional_SelfId] = [l].[Id]
ORDER BY [e].[Id], [l].[Id]

SELECT [l].[Id], [l].[Name], [l].[OneToMany_Optional_Self_InverseId], [l].[OneToMany_Required_Self_InverseId], [l].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l]
INNER JOIN (
    SELECT DISTINCT [e].[Id], [l].[Id] AS [Id0]
    FROM [Level1] AS [e]
    LEFT JOIN [Level1] AS [l] ON [e].[OneToOne_Optional_SelfId] = [l].[Id]
) AS [l0] ON [l].[OneToMany_Optional_Self_InverseId] = [l0].[Id0]
ORDER BY [l0].[Id], [l0].[Id0]

SELECT [l].[Id], [l].[Name], [l].[OneToMany_Optional_Self_InverseId], [l].[OneToMany_Required_Self_InverseId], [l].[OneToOne_Optional_SelfId], [l0].[Id], [l0].[Name], [l0].[OneToMany_Optional_Self_InverseId], [l0].[OneToMany_Required_Self_InverseId], [l0].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l]
INNER JOIN (
    SELECT DISTINCT [e].[Id]
    FROM [Level1] AS [e]
) AS [e] ON [l].[OneToMany_Optional_Self_InverseId] = [e].[Id]
LEFT JOIN [Level1] AS [l0] ON [l].[OneToOne_Optional_SelfId] = [l0].[Id]
ORDER BY [e].[Id]",
                Sql);
        }

        public override void Multiple_complex_include_select()
        {
            base.Multiple_complex_include_select();

            Assert.Equal(
                @"SELECT [e].[Id], [e].[Name], [e].[OneToMany_Optional_Self_InverseId], [e].[OneToMany_Required_Self_InverseId], [e].[OneToOne_Optional_SelfId], [l].[Id], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Name], [l].[OneToMany_Optional_InverseId], [l].[OneToMany_Optional_Self_InverseId], [l].[OneToMany_Required_InverseId], [l].[OneToMany_Required_Self_InverseId], [l].[OneToOne_Optional_PK_InverseId], [l].[OneToOne_Optional_SelfId]
FROM [Level1] AS [e]
LEFT JOIN [Level2] AS [l] ON [l].[Level1_Optional_Id] = [e].[Id]
ORDER BY [e].[Id], [l].[Id]

SELECT [l].[Id], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Name], [l].[OneToMany_Optional_InverseId], [l].[OneToMany_Optional_Self_InverseId], [l].[OneToMany_Required_InverseId], [l].[OneToMany_Required_Self_InverseId], [l].[OneToOne_Optional_PK_InverseId], [l].[OneToOne_Optional_SelfId], [l0].[Id], [l0].[Level2_Optional_Id], [l0].[Level2_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_InverseId], [l0].[OneToMany_Optional_Self_InverseId], [l0].[OneToMany_Required_InverseId], [l0].[OneToMany_Required_Self_InverseId], [l0].[OneToOne_Optional_PK_InverseId], [l0].[OneToOne_Optional_SelfId]
FROM [Level2] AS [l]
INNER JOIN (
    SELECT DISTINCT [e].[Id]
    FROM [Level1] AS [e]
) AS [e] ON [l].[OneToMany_Optional_InverseId] = [e].[Id]
LEFT JOIN [Level3] AS [l0] ON [l0].[Level2_Optional_Id] = [l].[Id]
ORDER BY [e].[Id]

SELECT [l].[Id], [l].[Level2_Optional_Id], [l].[Level2_Required_Id], [l].[Name], [l].[OneToMany_Optional_InverseId], [l].[OneToMany_Optional_Self_InverseId], [l].[OneToMany_Required_InverseId], [l].[OneToMany_Required_Self_InverseId], [l].[OneToOne_Optional_PK_InverseId], [l].[OneToOne_Optional_SelfId]
FROM [Level3] AS [l]
INNER JOIN (
    SELECT DISTINCT [e].[Id], [l].[Id] AS [Id0]
    FROM [Level1] AS [e]
    LEFT JOIN [Level2] AS [l] ON [l].[Level1_Optional_Id] = [e].[Id]
) AS [l0] ON [l].[OneToMany_Optional_InverseId] = [l0].[Id0]
ORDER BY [l0].[Id], [l0].[Id0]",
                Sql);
        }

        public override void Select_nav_prop_reference_optional1()
        {
            base.Select_nav_prop_reference_optional1();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Select_nav_prop_reference_optional1_via_DefaultIfEmpty()
        {
            base.Select_nav_prop_reference_optional1_via_DefaultIfEmpty();

            Assert.Equal(
                @"SELECT [l2].[Id], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_InverseId], [l2].[OneToMany_Optional_Self_InverseId], [l2].[OneToMany_Required_InverseId], [l2].[OneToMany_Required_Self_InverseId], [l2].[OneToOne_Optional_PK_InverseId], [l2].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l2] ON [l1].[Id] = [l2].[Level1_Optional_Id]
ORDER BY [l1].[Id]",
                Sql);
        }

        public override void Select_nav_prop_reference_optional2()
        {
            base.Select_nav_prop_reference_optional2();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Select_nav_prop_reference_optional2_via_DefaultIfEmpty()
        {
            base.Select_nav_prop_reference_optional2_via_DefaultIfEmpty();

            Assert.Equal(
                @"SELECT [l2].[Id], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_InverseId], [l2].[OneToMany_Optional_Self_InverseId], [l2].[OneToMany_Required_InverseId], [l2].[OneToMany_Required_Self_InverseId], [l2].[OneToOne_Optional_PK_InverseId], [l2].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l2] ON [l1].[Id] = [l2].[Level1_Optional_Id]
ORDER BY [l1].[Id]",
                Sql);
        }

        public override void Where_nav_prop_reference_optional1()
        {
            base.Where_nav_prop_reference_optional1();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Where_nav_prop_reference_optional1_via_DefaultIfEmpty()
        {
            base.Where_nav_prop_reference_optional1_via_DefaultIfEmpty();

            Assert.Equal(
                @"SELECT [l2Right].[Id], [l2Right].[Level1_Optional_Id], [l2Right].[Level1_Required_Id], [l2Right].[Name], [l2Right].[OneToMany_Optional_InverseId], [l2Right].[OneToMany_Optional_Self_InverseId], [l2Right].[OneToMany_Required_InverseId], [l2Right].[OneToMany_Required_Self_InverseId], [l2Right].[OneToOne_Optional_PK_InverseId], [l2Right].[OneToOne_Optional_SelfId]
FROM [Level2] AS [l2Right]

SELECT [l2Left].[Id], [l2Left].[Level1_Optional_Id], [l2Left].[Level1_Required_Id], [l2Left].[Name], [l2Left].[OneToMany_Optional_InverseId], [l2Left].[OneToMany_Optional_Self_InverseId], [l2Left].[OneToMany_Required_InverseId], [l2Left].[OneToMany_Required_Self_InverseId], [l2Left].[OneToOne_Optional_PK_InverseId], [l2Left].[OneToOne_Optional_SelfId], [l1].[Id]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l2Left] ON [l1].[Id] = [l2Left].[Level1_Optional_Id]
ORDER BY [l1].[Id]",
                Sql);
        }

        public override void Where_nav_prop_reference_optional2()
        {
            base.Where_nav_prop_reference_optional2();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Where_nav_prop_reference_optional2_via_DefaultIfEmpty()
        {
            base.Where_nav_prop_reference_optional2_via_DefaultIfEmpty();

            Assert.Equal(
                @"SELECT [l2Right].[Id], [l2Right].[Level1_Optional_Id], [l2Right].[Level1_Required_Id], [l2Right].[Name], [l2Right].[OneToMany_Optional_InverseId], [l2Right].[OneToMany_Optional_Self_InverseId], [l2Right].[OneToMany_Required_InverseId], [l2Right].[OneToMany_Required_Self_InverseId], [l2Right].[OneToOne_Optional_PK_InverseId], [l2Right].[OneToOne_Optional_SelfId]
FROM [Level2] AS [l2Right]

SELECT [l2Left].[Id], [l2Left].[Level1_Optional_Id], [l2Left].[Level1_Required_Id], [l2Left].[Name], [l2Left].[OneToMany_Optional_InverseId], [l2Left].[OneToMany_Optional_Self_InverseId], [l2Left].[OneToMany_Required_InverseId], [l2Left].[OneToMany_Required_Self_InverseId], [l2Left].[OneToOne_Optional_PK_InverseId], [l2Left].[OneToOne_Optional_SelfId], [l1].[Id]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l2Left] ON [l1].[Id] = [l2Left].[Level1_Optional_Id]
ORDER BY [l1].[Id]",
                Sql);
        }

        public override void OrderBy_nav_prop_reference_optional()
        {
            base.OrderBy_nav_prop_reference_optional();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void OrderBy_nav_prop_reference_optional_via_DefaultIfEmpty()
        {
            base.OrderBy_nav_prop_reference_optional_via_DefaultIfEmpty();

            Assert.Equal(
                @"SELECT [l2].[Id], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_InverseId], [l2].[OneToMany_Optional_Self_InverseId], [l2].[OneToMany_Required_InverseId], [l2].[OneToMany_Required_Self_InverseId], [l2].[OneToOne_Optional_PK_InverseId], [l2].[OneToOne_Optional_SelfId], [l1].[Id]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l2] ON [l1].[Id] = [l2].[Level1_Optional_Id]
ORDER BY [l1].[Id]",
                Sql);
        }

        public override void Result_operator_nav_prop_reference_optional()
        {
            base.Result_operator_nav_prop_reference_optional();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Result_operator_nav_prop_reference_optional_via_DefaultIfEmpty()
        {
            base.Result_operator_nav_prop_reference_optional_via_DefaultIfEmpty();

            Assert.Equal(
                @"SELECT [l2].[Id], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_InverseId], [l2].[OneToMany_Optional_Self_InverseId], [l2].[OneToMany_Required_InverseId], [l2].[OneToMany_Required_Self_InverseId], [l2].[OneToOne_Optional_PK_InverseId], [l2].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l2] ON [l1].[Id] = [l2].[Level1_Optional_Id]
ORDER BY [l1].[Id]",
                Sql);
        }

        // issue #3491
        //[Fact]
        public virtual void Multiple_complex_includes_from_sql()
        {
            using (var context = CreateContext())
            {
                var query = context.LevelOne.FromSql("SELECT * FROM [Level1]")
                    .Include(e => e.OneToOne_Optional_FK)
                    .ThenInclude(e => e.OneToMany_Optional)
                    .Include(e => e.OneToMany_Optional)
                    .ThenInclude(e => e.OneToOne_Optional_FK);

                var result = query.ToList();
            }

            Assert.Equal(
                @"",
                Sql);
        }

        private static string Sql => TestSqlLoggerFactory.Sql;
    }
}
