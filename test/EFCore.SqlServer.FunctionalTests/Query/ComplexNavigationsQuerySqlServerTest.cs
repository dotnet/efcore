// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ComplexNavigationsQuerySqlServerTest
        : ComplexNavigationsQueryTestBase<SqlServerTestStore, ComplexNavigationsQuerySqlServerFixture>
    {
        public ComplexNavigationsQuerySqlServerTest(
            ComplexNavigationsQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }

        private bool SupportsOffset => TestEnvironment.GetFlag(nameof(SqlServerCondition.SupportsOffset)) ?? true;

        public override void Entity_equality_empty()
        {
            base.Entity_equality_empty();

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_InverseId], [l].[OneToMany_Required_Self_InverseId], [l].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l]
LEFT JOIN [Level2] AS [l.OneToOne_Optional_FK] ON [l].[Id] = [l.OneToOne_Optional_FK].[Level1_Optional_Id]
WHERE [l.OneToOne_Optional_FK].[Id] = 0");
        }

        public override void Key_equality_when_sentinel_ef_property()
        {
            base.Key_equality_when_sentinel_ef_property();

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_InverseId], [l].[OneToMany_Required_Self_InverseId], [l].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l]
LEFT JOIN [Level2] AS [l.OneToOne_Optional_FK] ON [l].[Id] = [l.OneToOne_Optional_FK].[Level1_Optional_Id]
WHERE [l.OneToOne_Optional_FK].[Id] = 0");
        }

        public override void Key_equality_using_property_method_required()
        {
            base.Key_equality_using_property_method_required();

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_InverseId], [l].[OneToMany_Required_Self_InverseId], [l].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l]
LEFT JOIN [Level2] AS [l.OneToOne_Required_FK] ON [l].[Id] = [l.OneToOne_Required_FK].[Level1_Required_Id]
WHERE [l.OneToOne_Required_FK].[Id] > 7");
        }

        public override void Key_equality_using_property_method_required2()
        {
            base.Key_equality_using_property_method_required2();

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Name], [l].[OneToMany_Optional_InverseId], [l].[OneToMany_Optional_Self_InverseId], [l].[OneToMany_Required_InverseId], [l].[OneToMany_Required_Self_InverseId], [l].[OneToOne_Optional_PK_InverseId], [l].[OneToOne_Optional_SelfId]
FROM [Level2] AS [l]
WHERE [l].[Level1_Required_Id] > 7");
        }

        public override void Key_equality_using_property_method_nested()
        {
            base.Key_equality_using_property_method_nested();

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_InverseId], [l].[OneToMany_Required_Self_InverseId], [l].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l]
LEFT JOIN [Level2] AS [l.OneToOne_Required_FK] ON [l].[Id] = [l.OneToOne_Required_FK].[Level1_Required_Id]
WHERE [l.OneToOne_Required_FK].[Id] = 7");
        }

        public override void Key_equality_using_property_method_nested2()
        {
            base.Key_equality_using_property_method_nested2();

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Name], [l].[OneToMany_Optional_InverseId], [l].[OneToMany_Optional_Self_InverseId], [l].[OneToMany_Required_InverseId], [l].[OneToMany_Required_Self_InverseId], [l].[OneToOne_Optional_PK_InverseId], [l].[OneToOne_Optional_SelfId]
FROM [Level2] AS [l]
WHERE [l].[Level1_Required_Id] = 7");
        }

        public override void Key_equality_using_property_method_and_member_expression1()
        {
            base.Key_equality_using_property_method_and_member_expression1();

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_InverseId], [l].[OneToMany_Required_Self_InverseId], [l].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l]
LEFT JOIN [Level2] AS [l.OneToOne_Required_FK] ON [l].[Id] = [l.OneToOne_Required_FK].[Level1_Required_Id]
WHERE [l.OneToOne_Required_FK].[Id] = 7");
        }

        public override void Key_equality_using_property_method_and_member_expression2()
        {
            base.Key_equality_using_property_method_and_member_expression2();

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_InverseId], [l].[OneToMany_Required_Self_InverseId], [l].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l]
LEFT JOIN [Level2] AS [l.OneToOne_Required_FK] ON [l].[Id] = [l.OneToOne_Required_FK].[Level1_Required_Id]
WHERE [l.OneToOne_Required_FK].[Id] = 7");
        }

        public override void Key_equality_using_property_method_and_member_expression3()
        {
            base.Key_equality_using_property_method_and_member_expression3();

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Name], [l].[OneToMany_Optional_InverseId], [l].[OneToMany_Optional_Self_InverseId], [l].[OneToMany_Required_InverseId], [l].[OneToMany_Required_Self_InverseId], [l].[OneToOne_Optional_PK_InverseId], [l].[OneToOne_Optional_SelfId]
FROM [Level2] AS [l]
WHERE [l].[Level1_Required_Id] = 7");
        }

        public override void Key_equality_navigation_converted_to_FK()
        {
            base.Key_equality_navigation_converted_to_FK();

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Name], [l].[OneToMany_Optional_InverseId], [l].[OneToMany_Optional_Self_InverseId], [l].[OneToMany_Required_InverseId], [l].[OneToMany_Required_Self_InverseId], [l].[OneToOne_Optional_PK_InverseId], [l].[OneToOne_Optional_SelfId]
FROM [Level2] AS [l]
WHERE [l].[Level1_Required_Id] = 1");
        }

        public override void Key_equality_two_conditions_on_same_navigation()
        {
            base.Key_equality_two_conditions_on_same_navigation();

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_InverseId], [l].[OneToMany_Required_Self_InverseId], [l].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l]
LEFT JOIN [Level2] AS [l.OneToOne_Required_FK] ON [l].[Id] = [l.OneToOne_Required_FK].[Level1_Required_Id]
WHERE ([l.OneToOne_Required_FK].[Id] = 1) OR ([l.OneToOne_Required_FK].[Id] = 2)");
        }

        public override void Key_equality_two_conditions_on_same_navigation2()
        {
            base.Key_equality_two_conditions_on_same_navigation2();

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Name], [l].[OneToMany_Optional_InverseId], [l].[OneToMany_Optional_Self_InverseId], [l].[OneToMany_Required_InverseId], [l].[OneToMany_Required_Self_InverseId], [l].[OneToOne_Optional_PK_InverseId], [l].[OneToOne_Optional_SelfId]
FROM [Level2] AS [l]
WHERE ([l].[Level1_Required_Id] = 1) OR ([l].[Level1_Required_Id] = 2)");
        }

        public override void Multi_level_include_one_to_many_optional_and_one_to_many_optional_produces_valid_sql()
        {
            base.Multi_level_include_one_to_many_optional_and_one_to_many_optional_produces_valid_sql();

            AssertSql(
                @"SELECT [e].[Id], [e].[Date], [e].[Name], [e].[OneToMany_Optional_Self_InverseId], [e].[OneToMany_Required_Self_InverseId], [e].[OneToOne_Optional_SelfId]
FROM [Level1] AS [e]
ORDER BY [e].[Id]",
                //
                @"SELECT [e.OneToMany_Optional].[Id], [e.OneToMany_Optional].[Date], [e.OneToMany_Optional].[Level1_Optional_Id], [e.OneToMany_Optional].[Level1_Required_Id], [e.OneToMany_Optional].[Name], [e.OneToMany_Optional].[OneToMany_Optional_InverseId], [e.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [e.OneToMany_Optional].[OneToMany_Required_InverseId], [e.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [e.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [e.OneToMany_Optional].[OneToOne_Optional_SelfId]
FROM [Level2] AS [e.OneToMany_Optional]
INNER JOIN (
    SELECT [e0].[Id]
    FROM [Level1] AS [e0]
) AS [t] ON [e.OneToMany_Optional].[OneToMany_Optional_InverseId] = [t].[Id]
ORDER BY [t].[Id], [e.OneToMany_Optional].[Id]",
                //
                @"SELECT [e.OneToMany_Optional.OneToMany_Optional].[Id], [e.OneToMany_Optional.OneToMany_Optional].[Level2_Optional_Id], [e.OneToMany_Optional.OneToMany_Optional].[Level2_Required_Id], [e.OneToMany_Optional.OneToMany_Optional].[Name], [e.OneToMany_Optional.OneToMany_Optional].[OneToMany_Optional_InverseId], [e.OneToMany_Optional.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [e.OneToMany_Optional.OneToMany_Optional].[OneToMany_Required_InverseId], [e.OneToMany_Optional.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [e.OneToMany_Optional.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [e.OneToMany_Optional.OneToMany_Optional].[OneToOne_Optional_SelfId]
FROM [Level3] AS [e.OneToMany_Optional.OneToMany_Optional]
INNER JOIN (
    SELECT DISTINCT [e.OneToMany_Optional0].[Id], [t0].[Id] AS [Id0]
    FROM [Level2] AS [e.OneToMany_Optional0]
    INNER JOIN (
        SELECT [e1].[Id]
        FROM [Level1] AS [e1]
    ) AS [t0] ON [e.OneToMany_Optional0].[OneToMany_Optional_InverseId] = [t0].[Id]
) AS [t1] ON [e.OneToMany_Optional.OneToMany_Optional].[OneToMany_Optional_InverseId] = [t1].[Id]
ORDER BY [t1].[Id0], [t1].[Id]");
        }

        public override void Multi_level_include_correct_PK_is_chosen_as_the_join_predicate_for_queries_that_join_same_table_multiple_times()
        {
            base.Multi_level_include_correct_PK_is_chosen_as_the_join_predicate_for_queries_that_join_same_table_multiple_times();

            AssertSql(
                @"SELECT [e].[Id], [e].[Date], [e].[Name], [e].[OneToMany_Optional_Self_InverseId], [e].[OneToMany_Required_Self_InverseId], [e].[OneToOne_Optional_SelfId]
FROM [Level1] AS [e]
ORDER BY [e].[Id]",
                //
                @"SELECT [e.OneToMany_Optional].[Id], [e.OneToMany_Optional].[Date], [e.OneToMany_Optional].[Level1_Optional_Id], [e.OneToMany_Optional].[Level1_Required_Id], [e.OneToMany_Optional].[Name], [e.OneToMany_Optional].[OneToMany_Optional_InverseId], [e.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [e.OneToMany_Optional].[OneToMany_Required_InverseId], [e.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [e.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [e.OneToMany_Optional].[OneToOne_Optional_SelfId]
FROM [Level2] AS [e.OneToMany_Optional]
INNER JOIN (
    SELECT [e0].[Id]
    FROM [Level1] AS [e0]
) AS [t] ON [e.OneToMany_Optional].[OneToMany_Optional_InverseId] = [t].[Id]
ORDER BY [t].[Id], [e.OneToMany_Optional].[Id]",
                //
                @"SELECT [e.OneToMany_Optional.OneToMany_Optional].[Id], [e.OneToMany_Optional.OneToMany_Optional].[Level2_Optional_Id], [e.OneToMany_Optional.OneToMany_Optional].[Level2_Required_Id], [e.OneToMany_Optional.OneToMany_Optional].[Name], [e.OneToMany_Optional.OneToMany_Optional].[OneToMany_Optional_InverseId], [e.OneToMany_Optional.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [e.OneToMany_Optional.OneToMany_Optional].[OneToMany_Required_InverseId], [e.OneToMany_Optional.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [e.OneToMany_Optional.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [e.OneToMany_Optional.OneToMany_Optional].[OneToOne_Optional_SelfId], [l.OneToMany_Required_Inverse].[Id], [l.OneToMany_Required_Inverse].[Date], [l.OneToMany_Required_Inverse].[Level1_Optional_Id], [l.OneToMany_Required_Inverse].[Level1_Required_Id], [l.OneToMany_Required_Inverse].[Name], [l.OneToMany_Required_Inverse].[OneToMany_Optional_InverseId], [l.OneToMany_Required_Inverse].[OneToMany_Optional_Self_InverseId], [l.OneToMany_Required_Inverse].[OneToMany_Required_InverseId], [l.OneToMany_Required_Inverse].[OneToMany_Required_Self_InverseId], [l.OneToMany_Required_Inverse].[OneToOne_Optional_PK_InverseId], [l.OneToMany_Required_Inverse].[OneToOne_Optional_SelfId]
FROM [Level3] AS [e.OneToMany_Optional.OneToMany_Optional]
INNER JOIN [Level2] AS [l.OneToMany_Required_Inverse] ON [e.OneToMany_Optional.OneToMany_Optional].[OneToMany_Required_InverseId] = [l.OneToMany_Required_Inverse].[Id]
INNER JOIN (
    SELECT DISTINCT [e.OneToMany_Optional0].[Id], [t0].[Id] AS [Id0]
    FROM [Level2] AS [e.OneToMany_Optional0]
    INNER JOIN (
        SELECT [e1].[Id]
        FROM [Level1] AS [e1]
    ) AS [t0] ON [e.OneToMany_Optional0].[OneToMany_Optional_InverseId] = [t0].[Id]
) AS [t1] ON [e.OneToMany_Optional.OneToMany_Optional].[OneToMany_Optional_InverseId] = [t1].[Id]
ORDER BY [t1].[Id0], [t1].[Id], [l.OneToMany_Required_Inverse].[Id]",
                //
                @"SELECT [l.OneToMany_Required_Inverse.OneToMany_Optional].[Id], [l.OneToMany_Required_Inverse.OneToMany_Optional].[Level2_Optional_Id], [l.OneToMany_Required_Inverse.OneToMany_Optional].[Level2_Required_Id], [l.OneToMany_Required_Inverse.OneToMany_Optional].[Name], [l.OneToMany_Required_Inverse.OneToMany_Optional].[OneToMany_Optional_InverseId], [l.OneToMany_Required_Inverse.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [l.OneToMany_Required_Inverse.OneToMany_Optional].[OneToMany_Required_InverseId], [l.OneToMany_Required_Inverse.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [l.OneToMany_Required_Inverse.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [l.OneToMany_Required_Inverse.OneToMany_Optional].[OneToOne_Optional_SelfId]
FROM [Level3] AS [l.OneToMany_Required_Inverse.OneToMany_Optional]
INNER JOIN (
    SELECT DISTINCT [l.OneToMany_Required_Inverse0].[Id], [t3].[Id0], [t3].[Id] AS [Id1]
    FROM [Level3] AS [e.OneToMany_Optional.OneToMany_Optional0]
    INNER JOIN [Level2] AS [l.OneToMany_Required_Inverse0] ON [e.OneToMany_Optional.OneToMany_Optional0].[OneToMany_Required_InverseId] = [l.OneToMany_Required_Inverse0].[Id]
    INNER JOIN (
        SELECT DISTINCT [e.OneToMany_Optional1].[Id], [t2].[Id] AS [Id0]
        FROM [Level2] AS [e.OneToMany_Optional1]
        INNER JOIN (
            SELECT [e2].[Id]
            FROM [Level1] AS [e2]
        ) AS [t2] ON [e.OneToMany_Optional1].[OneToMany_Optional_InverseId] = [t2].[Id]
    ) AS [t3] ON [e.OneToMany_Optional.OneToMany_Optional0].[OneToMany_Optional_InverseId] = [t3].[Id]
) AS [t4] ON [l.OneToMany_Required_Inverse.OneToMany_Optional].[OneToMany_Optional_InverseId] = [t4].[Id]
ORDER BY [t4].[Id0], [t4].[Id1], [t4].[Id]");
        }

        public override void Multi_level_include_with_short_circuiting()
        {
            base.Multi_level_include_with_short_circuiting();

            AssertSql(
                @"SELECT [x].[Name], [x].[LabelDefaultText], [x].[PlaceholderDefaultText], [x.Placeholder].[DefaultText], [x.Label].[DefaultText]
FROM [ComplexNavigationField] AS [x]
LEFT JOIN [ComplexNavigationString] AS [x.Placeholder] ON [x].[PlaceholderDefaultText] = [x.Placeholder].[DefaultText]
LEFT JOIN [ComplexNavigationString] AS [x.Label] ON [x].[LabelDefaultText] = [x.Label].[DefaultText]
ORDER BY [x.Label].[DefaultText], [x.Placeholder].[DefaultText]",
                //
                @"SELECT [x.Label.Globalizations].[Text], [x.Label.Globalizations].[ComplexNavigationStringDefaultText], [x.Label.Globalizations].[LanguageName], [c.Language].[Name], [c.Language].[CultureString]
FROM [ComplexNavigationGlobalization] AS [x.Label.Globalizations]
LEFT JOIN [ComplexNavigationLanguage] AS [c.Language] ON [x.Label.Globalizations].[LanguageName] = [c.Language].[Name]
INNER JOIN (
    SELECT DISTINCT [x.Label0].[DefaultText]
    FROM [ComplexNavigationField] AS [x0]
    LEFT JOIN [ComplexNavigationString] AS [x.Placeholder0] ON [x0].[PlaceholderDefaultText] = [x.Placeholder0].[DefaultText]
    LEFT JOIN [ComplexNavigationString] AS [x.Label0] ON [x0].[LabelDefaultText] = [x.Label0].[DefaultText]
) AS [t] ON [x.Label.Globalizations].[ComplexNavigationStringDefaultText] = [t].[DefaultText]
ORDER BY [t].[DefaultText]",
                //
                @"SELECT [x.Placeholder.Globalizations].[Text], [x.Placeholder.Globalizations].[ComplexNavigationStringDefaultText], [x.Placeholder.Globalizations].[LanguageName], [c.Language0].[Name], [c.Language0].[CultureString]
FROM [ComplexNavigationGlobalization] AS [x.Placeholder.Globalizations]
LEFT JOIN [ComplexNavigationLanguage] AS [c.Language0] ON [x.Placeholder.Globalizations].[LanguageName] = [c.Language0].[Name]
INNER JOIN (
    SELECT DISTINCT [x.Placeholder1].[DefaultText], [x.Label1].[DefaultText] AS [DefaultText0]
    FROM [ComplexNavigationField] AS [x1]
    LEFT JOIN [ComplexNavigationString] AS [x.Placeholder1] ON [x1].[PlaceholderDefaultText] = [x.Placeholder1].[DefaultText]
    LEFT JOIN [ComplexNavigationString] AS [x.Label1] ON [x1].[LabelDefaultText] = [x.Label1].[DefaultText]
) AS [t0] ON [x.Placeholder.Globalizations].[ComplexNavigationStringDefaultText] = [t0].[DefaultText]
ORDER BY [t0].[DefaultText0], [t0].[DefaultText]");
        }

        public override void Join_navigation_key_access_optional()
        {
            base.Join_navigation_key_access_optional();

            AssertSql(
                @"SELECT [e1].[Id] AS [Id1], [e2].[Id] AS [Id2]
FROM [Level1] AS [e1]
INNER JOIN [Level2] AS [e2] ON [e1].[Id] = [e2].[Level1_Optional_Id]");
        }

        public override void Join_navigation_key_access_required()
        {
            base.Join_navigation_key_access_required();

            AssertSql(
                @"SELECT [e1].[Id] AS [Id1], [e2].[Id] AS [Id2]
FROM [Level1] AS [e1]
INNER JOIN [Level2] AS [e2] ON [e1].[Id] = [e2].[Level1_Required_Id]");
        }

        public override void Navigation_key_access_optional_comparison()
        {
            base.Navigation_key_access_optional_comparison();

            AssertSql(
                @"SELECT [e2].[Id]
FROM [Level2] AS [e2]
WHERE [e2].[OneToOne_Optional_PK_InverseId] > 5");
        }

        public override void Navigation_key_access_required_comparison()
        {
            base.Navigation_key_access_required_comparison();

            AssertSql(
                @"SELECT [e2].[Id]
FROM [Level2] AS [e2]
WHERE [e2].[Id] > 5");
        }

        public override void Navigation_inside_method_call_translated_to_join()
        {
            base.Navigation_inside_method_call_translated_to_join();

            AssertSql(
                @"SELECT [e1].[Id], [e1].[Date], [e1].[Name], [e1].[OneToMany_Optional_Self_InverseId], [e1].[OneToMany_Required_Self_InverseId], [e1].[OneToOne_Optional_SelfId]
FROM [Level1] AS [e1]
LEFT JOIN [Level2] AS [e1.OneToOne_Required_FK] ON [e1].[Id] = [e1.OneToOne_Required_FK].[Level1_Required_Id]
WHERE [e1.OneToOne_Required_FK].[Name] LIKE N'L' + N'%' AND (LEFT([e1.OneToOne_Required_FK].[Name], LEN(N'L')) = N'L')");
        }

        public override void Navigation_inside_method_call_translated_to_join2()
        {
            base.Navigation_inside_method_call_translated_to_join2();

            AssertSql(
                @"SELECT [e3].[Id], [e3].[Level2_Optional_Id], [e3].[Level2_Required_Id], [e3].[Name], [e3].[OneToMany_Optional_InverseId], [e3].[OneToMany_Optional_Self_InverseId], [e3].[OneToMany_Required_InverseId], [e3].[OneToMany_Required_Self_InverseId], [e3].[OneToOne_Optional_PK_InverseId], [e3].[OneToOne_Optional_SelfId]
FROM [Level3] AS [e3]
INNER JOIN [Level2] AS [e3.OneToOne_Required_FK_Inverse] ON [e3].[Level2_Required_Id] = [e3.OneToOne_Required_FK_Inverse].[Id]
WHERE [e3.OneToOne_Required_FK_Inverse].[Name] LIKE N'L' + N'%' AND (LEFT([e3.OneToOne_Required_FK_Inverse].[Name], LEN(N'L')) = N'L')");
        }

        public override void Optional_navigation_inside_method_call_translated_to_join()
        {
            base.Optional_navigation_inside_method_call_translated_to_join();

            AssertSql(
                @"SELECT [e1].[Id], [e1].[Date], [e1].[Name], [e1].[OneToMany_Optional_Self_InverseId], [e1].[OneToMany_Required_Self_InverseId], [e1].[OneToOne_Optional_SelfId]
FROM [Level1] AS [e1]
LEFT JOIN [Level2] AS [e1.OneToOne_Optional_FK] ON [e1].[Id] = [e1.OneToOne_Optional_FK].[Level1_Optional_Id]
WHERE [e1.OneToOne_Optional_FK].[Name] LIKE N'L' + N'%' AND (LEFT([e1.OneToOne_Optional_FK].[Name], LEN(N'L')) = N'L')");
        }

        public override void Optional_navigation_inside_property_method_translated_to_join()
        {
            base.Optional_navigation_inside_property_method_translated_to_join();

            AssertSql(
                @"SELECT [e1].[Id], [e1].[Date], [e1].[Name], [e1].[OneToMany_Optional_Self_InverseId], [e1].[OneToMany_Required_Self_InverseId], [e1].[OneToOne_Optional_SelfId]
FROM [Level1] AS [e1]
LEFT JOIN [Level2] AS [e1.OneToOne_Optional_FK] ON [e1].[Id] = [e1.OneToOne_Optional_FK].[Level1_Optional_Id]
WHERE [e1.OneToOne_Optional_FK].[Name] = N'L2 01'");
        }

        public override void Optional_navigation_inside_nested_method_call_translated_to_join()
        {
            base.Optional_navigation_inside_nested_method_call_translated_to_join();

            AssertSql(
                @"SELECT [e1].[Id], [e1].[Date], [e1].[Name], [e1].[OneToMany_Optional_Self_InverseId], [e1].[OneToMany_Required_Self_InverseId], [e1].[OneToOne_Optional_SelfId]
FROM [Level1] AS [e1]
LEFT JOIN [Level2] AS [e1.OneToOne_Optional_FK] ON [e1].[Id] = [e1.OneToOne_Optional_FK].[Level1_Optional_Id]
WHERE UPPER([e1.OneToOne_Optional_FK].[Name]) LIKE N'L' + N'%' AND (LEFT(UPPER([e1.OneToOne_Optional_FK].[Name]), LEN(N'L')) = N'L')");
        }

        public override void Method_call_on_optional_navigation_translates_to_null_conditional_properly_for_arguments()
        {
            base.Method_call_on_optional_navigation_translates_to_null_conditional_properly_for_arguments();

            AssertSql(
                @"SELECT [e1].[Id], [e1].[Date], [e1].[Name], [e1].[OneToMany_Optional_Self_InverseId], [e1].[OneToMany_Required_Self_InverseId], [e1].[OneToOne_Optional_SelfId]
FROM [Level1] AS [e1]
LEFT JOIN [Level2] AS [e1.OneToOne_Optional_FK] ON [e1].[Id] = [e1.OneToOne_Optional_FK].[Level1_Optional_Id]
WHERE ([e1.OneToOne_Optional_FK].[Name] LIKE [e1.OneToOne_Optional_FK].[Name] + N'%' AND (LEFT([e1.OneToOne_Optional_FK].[Name], LEN([e1.OneToOne_Optional_FK].[Name])) = [e1.OneToOne_Optional_FK].[Name])) OR ([e1.OneToOne_Optional_FK].[Name] = N'')");
        }

        public override void Optional_navigation_inside_method_call_translated_to_join_keeps_original_nullability()
        {
            base.Optional_navigation_inside_method_call_translated_to_join_keeps_original_nullability();

            AssertSql(
                @"SELECT [e1].[Id], [e1].[Date], [e1].[Name], [e1].[OneToMany_Optional_Self_InverseId], [e1].[OneToMany_Required_Self_InverseId], [e1].[OneToOne_Optional_SelfId]
FROM [Level1] AS [e1]
LEFT JOIN [Level2] AS [e1.OneToOne_Optional_FK] ON [e1].[Id] = [e1.OneToOne_Optional_FK].[Level1_Optional_Id]
WHERE DATEADD(day, 10E0, [e1.OneToOne_Optional_FK].[Date]) > '2000-02-01T00:00:00.000'");
        }

        public override void Optional_navigation_inside_nested_method_call_translated_to_join_keeps_original_nullability()
        {
            base.Optional_navigation_inside_nested_method_call_translated_to_join_keeps_original_nullability();

            AssertSql(
                @"SELECT [e1].[Id], [e1].[Date], [e1].[Name], [e1].[OneToMany_Optional_Self_InverseId], [e1].[OneToMany_Required_Self_InverseId], [e1].[OneToOne_Optional_SelfId]
FROM [Level1] AS [e1]
LEFT JOIN [Level2] AS [e1.OneToOne_Optional_FK] ON [e1].[Id] = [e1.OneToOne_Optional_FK].[Level1_Optional_Id]
WHERE DATEADD(month, 2, DATEADD(day, 15E0, DATEADD(day, 10E0, [e1.OneToOne_Optional_FK].[Date]))) > '2002-02-01T00:00:00.000'");
        }

        public override void Optional_navigation_inside_nested_method_call_translated_to_join_keeps_original_nullability_also_for_arguments()
        {
            base.Optional_navigation_inside_nested_method_call_translated_to_join_keeps_original_nullability_also_for_arguments();

            AssertSql(
                @"SELECT [e1].[Id], [e1].[Date], [e1].[Name], [e1].[OneToMany_Optional_Self_InverseId], [e1].[OneToMany_Required_Self_InverseId], [e1].[OneToOne_Optional_SelfId]
FROM [Level1] AS [e1]
LEFT JOIN [Level2] AS [e1.OneToOne_Optional_FK] ON [e1].[Id] = [e1.OneToOne_Optional_FK].[Level1_Optional_Id]
WHERE DATEADD(day, [e1.OneToOne_Optional_FK].[Id], DATEADD(day, 15E0, [e1.OneToOne_Optional_FK].[Date])) > '2002-02-01T00:00:00.000'");
        }

        public override void Join_navigation_in_outer_selector_translated_to_extra_join()
        {
            base.Join_navigation_in_outer_selector_translated_to_extra_join();

            AssertSql(
                @"SELECT [e1].[Id] AS [Id1], [e2].[Id] AS [Id2]
FROM [Level1] AS [e1]
LEFT JOIN [Level2] AS [e1.OneToOne_Optional_FK] ON [e1].[Id] = [e1.OneToOne_Optional_FK].[Level1_Optional_Id]
INNER JOIN [Level2] AS [e2] ON [e1.OneToOne_Optional_FK].[Id] = [e2].[Id]");
        }

        public override void Join_navigation_in_outer_selector_translated_to_extra_join_nested()
        {
            base.Join_navigation_in_outer_selector_translated_to_extra_join_nested();

            AssertSql(
                @"SELECT [e1].[Id] AS [Id1], [e3].[Id] AS [Id3]
FROM [Level1] AS [e1]
LEFT JOIN [Level2] AS [e1.OneToOne_Required_FK] ON [e1].[Id] = [e1.OneToOne_Required_FK].[Level1_Required_Id]
LEFT JOIN [Level3] AS [e1.OneToOne_Required_FK.OneToOne_Optional_FK] ON [e1.OneToOne_Required_FK].[Id] = [e1.OneToOne_Required_FK.OneToOne_Optional_FK].[Level2_Optional_Id]
INNER JOIN [Level3] AS [e3] ON [e1.OneToOne_Required_FK.OneToOne_Optional_FK].[Id] = [e3].[Id]");
        }

        public override void Join_navigation_in_outer_selector_translated_to_extra_join_nested2()
        {
            base.Join_navigation_in_outer_selector_translated_to_extra_join_nested2();

            AssertSql(
                @"SELECT [e3].[Id] AS [Id3], [e1].[Id] AS [Id1]
FROM [Level3] AS [e3]
INNER JOIN [Level2] AS [e3.OneToOne_Required_FK_Inverse] ON [e3].[Level2_Required_Id] = [e3.OneToOne_Required_FK_Inverse].[Id]
LEFT JOIN [Level1] AS [e3.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK_Inverse] ON [e3.OneToOne_Required_FK_Inverse].[Level1_Optional_Id] = [e3.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK_Inverse].[Id]
INNER JOIN [Level1] AS [e1] ON [e3.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK_Inverse].[Id] = [e1].[Id]");
        }

        public override void Join_navigation_in_inner_selector_translated_to_subquery()
        {
            base.Join_navigation_in_inner_selector_translated_to_subquery();

            AssertSql(
                @"SELECT [e2].[Id] AS [Id2], [e1].[Id] AS [Id1]
FROM [Level2] AS [e2]
INNER JOIN [Level1] AS [e1] ON [e2].[Id] = (
    SELECT TOP(1) [subQuery0].[Id]
    FROM [Level2] AS [subQuery0]
    WHERE [subQuery0].[Level1_Optional_Id] = [e1].[Id]
)");
        }

        public override void Join_navigations_in_inner_selector_translated_to_multiple_subquery_without_collision()
        {
            base.Join_navigations_in_inner_selector_translated_to_multiple_subquery_without_collision();

            AssertSql(
                @"SELECT [e2].[Id] AS [Id2], [e1].[Id] AS [Id1], [e3].[Id] AS [Id3]
FROM [Level2] AS [e2]
INNER JOIN [Level1] AS [e1] ON [e2].[Id] = (
    SELECT TOP(1) [subQuery0].[Id]
    FROM [Level2] AS [subQuery0]
    WHERE [subQuery0].[Level1_Optional_Id] = [e1].[Id]
)
INNER JOIN [Level3] AS [e3] ON [e2].[Id] = [e3].[Level2_Optional_Id]");
        }

        public override void Join_navigation_translated_to_subquery_non_key_join()
        {
            base.Join_navigation_translated_to_subquery_non_key_join();

            AssertSql(
                @"SELECT [e2].[Id] AS [Id2], [e2].[Name] AS [Name2], [e1].[Id] AS [Id1], [e1].[Name] AS [Name1]
FROM [Level2] AS [e2]
INNER JOIN [Level1] AS [e1] ON [e2].[Name] = (
    SELECT TOP(1) [subQuery0].[Name]
    FROM [Level2] AS [subQuery0]
    WHERE [subQuery0].[Level1_Optional_Id] = [e1].[Id]
)");
        }

        public override void Join_navigation_translated_to_subquery_self_ref()
        {
            base.Join_navigation_translated_to_subquery_self_ref();

            AssertSql(
                @"SELECT [e1].[Id] AS [Id1], [e2].[Id] AS [Id2]
FROM [Level1] AS [e1]
INNER JOIN [Level1] AS [e2] ON [e1].[Id] = [e2].[OneToMany_Optional_Self_InverseId]");
        }

        public override void Join_navigation_translated_to_subquery_nested()
        {
            base.Join_navigation_translated_to_subquery_nested();

            AssertSql(
                @"SELECT [e3].[Id] AS [Id3], [e1].[Id] AS [Id1]
FROM [Level3] AS [e3]
INNER JOIN [Level1] AS [e1] ON [e3].[Id] = (
    SELECT TOP(1) [subQuery.OneToOne_Optional_FK0].[Id]
    FROM [Level2] AS [subQuery0]
    LEFT JOIN [Level3] AS [subQuery.OneToOne_Optional_FK0] ON [subQuery0].[Id] = [subQuery.OneToOne_Optional_FK0].[Level2_Optional_Id]
    WHERE [subQuery0].[Level1_Required_Id] = [e1].[Id]
)");
        }

        public override void Join_navigation_translated_to_subquery_deeply_nested_non_key_join()
        {
            base.Join_navigation_translated_to_subquery_deeply_nested_non_key_join();

            AssertSql(
                @"SELECT [e4].[Id] AS [Id4], [e4].[Name] AS [Name4], [e1].[Id] AS [Id1], [e1].[Name] AS [Name1]
FROM [Level4] AS [e4]
INNER JOIN [Level1] AS [e1] ON [e4].[Name] = (
    SELECT TOP(1) [subQuery.OneToOne_Optional_FK.OneToOne_Required_PK0].[Name]
    FROM [Level2] AS [subQuery0]
    LEFT JOIN [Level3] AS [subQuery.OneToOne_Optional_FK0] ON [subQuery0].[Id] = [subQuery.OneToOne_Optional_FK0].[Level2_Optional_Id]
    LEFT JOIN [Level4] AS [subQuery.OneToOne_Optional_FK.OneToOne_Required_PK0] ON [subQuery.OneToOne_Optional_FK0].[Id] = [subQuery.OneToOne_Optional_FK.OneToOne_Required_PK0].[Id]
    WHERE [subQuery0].[Level1_Required_Id] = [e1].[Id]
)");
        }

        public override void Join_navigation_translated_to_subquery_deeply_nested_required()
        {
            base.Join_navigation_translated_to_subquery_deeply_nested_required();

            AssertSql(
                @"SELECT [e4].[Id] AS [Id4], [e4].[Name] AS [Name4], [e1].[Id] AS [Id1], [e1].[Name] AS [Name1]
FROM [Level1] AS [e1]
INNER JOIN [Level4] AS [e4] ON [e1].[Name] = (
    SELECT TOP(1) [subQuery.OneToOne_Required_FK_Inverse.OneToOne_Required_PK_Inverse0].[Name]
    FROM [Level3] AS [subQuery0]
    INNER JOIN [Level2] AS [subQuery.OneToOne_Required_FK_Inverse0] ON [subQuery0].[Level2_Required_Id] = [subQuery.OneToOne_Required_FK_Inverse0].[Id]
    INNER JOIN [Level1] AS [subQuery.OneToOne_Required_FK_Inverse.OneToOne_Required_PK_Inverse0] ON [subQuery.OneToOne_Required_FK_Inverse0].[Id] = [subQuery.OneToOne_Required_FK_Inverse.OneToOne_Required_PK_Inverse0].[Id]
    WHERE [subQuery0].[Id] = [e4].[Level3_Required_Id]
)");
        }

        public override void Multiple_complex_includes()
        {
            base.Multiple_complex_includes();

            AssertSql(
                @"SELECT [e].[Id], [e].[Date], [e].[Name], [e].[OneToMany_Optional_Self_InverseId], [e].[OneToMany_Required_Self_InverseId], [e].[OneToOne_Optional_SelfId], [e.OneToOne_Optional_FK].[Id], [e.OneToOne_Optional_FK].[Date], [e.OneToOne_Optional_FK].[Level1_Optional_Id], [e.OneToOne_Optional_FK].[Level1_Required_Id], [e.OneToOne_Optional_FK].[Name], [e.OneToOne_Optional_FK].[OneToMany_Optional_InverseId], [e.OneToOne_Optional_FK].[OneToMany_Optional_Self_InverseId], [e.OneToOne_Optional_FK].[OneToMany_Required_InverseId], [e.OneToOne_Optional_FK].[OneToMany_Required_Self_InverseId], [e.OneToOne_Optional_FK].[OneToOne_Optional_PK_InverseId], [e.OneToOne_Optional_FK].[OneToOne_Optional_SelfId]
FROM [Level1] AS [e]
LEFT JOIN [Level2] AS [e.OneToOne_Optional_FK] ON [e].[Id] = [e.OneToOne_Optional_FK].[Level1_Optional_Id]
ORDER BY [e.OneToOne_Optional_FK].[Id], [e].[Id]",
                //
                @"SELECT [e.OneToMany_Optional].[Id], [e.OneToMany_Optional].[Date], [e.OneToMany_Optional].[Level1_Optional_Id], [e.OneToMany_Optional].[Level1_Required_Id], [e.OneToMany_Optional].[Name], [e.OneToMany_Optional].[OneToMany_Optional_InverseId], [e.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [e.OneToMany_Optional].[OneToMany_Required_InverseId], [e.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [e.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [e.OneToMany_Optional].[OneToOne_Optional_SelfId], [l.OneToOne_Optional_FK].[Id], [l.OneToOne_Optional_FK].[Level2_Optional_Id], [l.OneToOne_Optional_FK].[Level2_Required_Id], [l.OneToOne_Optional_FK].[Name], [l.OneToOne_Optional_FK].[OneToMany_Optional_InverseId], [l.OneToOne_Optional_FK].[OneToMany_Optional_Self_InverseId], [l.OneToOne_Optional_FK].[OneToMany_Required_InverseId], [l.OneToOne_Optional_FK].[OneToMany_Required_Self_InverseId], [l.OneToOne_Optional_FK].[OneToOne_Optional_PK_InverseId], [l.OneToOne_Optional_FK].[OneToOne_Optional_SelfId]
FROM [Level2] AS [e.OneToMany_Optional]
LEFT JOIN [Level3] AS [l.OneToOne_Optional_FK] ON [e.OneToMany_Optional].[Id] = [l.OneToOne_Optional_FK].[Level2_Optional_Id]
INNER JOIN (
    SELECT DISTINCT [e1].[Id], [e.OneToOne_Optional_FK1].[Id] AS [Id0]
    FROM [Level1] AS [e1]
    LEFT JOIN [Level2] AS [e.OneToOne_Optional_FK1] ON [e1].[Id] = [e.OneToOne_Optional_FK1].[Level1_Optional_Id]
) AS [t0] ON [e.OneToMany_Optional].[OneToMany_Optional_InverseId] = [t0].[Id]
ORDER BY [t0].[Id0], [t0].[Id]",
                //
                @"SELECT [e.OneToOne_Optional_FK.OneToMany_Optional].[Id], [e.OneToOne_Optional_FK.OneToMany_Optional].[Level2_Optional_Id], [e.OneToOne_Optional_FK.OneToMany_Optional].[Level2_Required_Id], [e.OneToOne_Optional_FK.OneToMany_Optional].[Name], [e.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Optional_InverseId], [e.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [e.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Required_InverseId], [e.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [e.OneToOne_Optional_FK.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [e.OneToOne_Optional_FK.OneToMany_Optional].[OneToOne_Optional_SelfId]
FROM [Level3] AS [e.OneToOne_Optional_FK.OneToMany_Optional]
INNER JOIN (
    SELECT DISTINCT [e.OneToOne_Optional_FK0].[Id]
    FROM [Level1] AS [e0]
    LEFT JOIN [Level2] AS [e.OneToOne_Optional_FK0] ON [e0].[Id] = [e.OneToOne_Optional_FK0].[Level1_Optional_Id]
) AS [t] ON [e.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Optional_InverseId] = [t].[Id]
ORDER BY [t].[Id]");
        }

        public override void Multiple_complex_includes_self_ref()
        {
            base.Multiple_complex_includes_self_ref();

            AssertSql(
                @"SELECT [e].[Id], [e].[Date], [e].[Name], [e].[OneToMany_Optional_Self_InverseId], [e].[OneToMany_Required_Self_InverseId], [e].[OneToOne_Optional_SelfId], [e.OneToOne_Optional_Self].[Id], [e.OneToOne_Optional_Self].[Date], [e.OneToOne_Optional_Self].[Name], [e.OneToOne_Optional_Self].[OneToMany_Optional_Self_InverseId], [e.OneToOne_Optional_Self].[OneToMany_Required_Self_InverseId], [e.OneToOne_Optional_Self].[OneToOne_Optional_SelfId]
FROM [Level1] AS [e]
LEFT JOIN [Level1] AS [e.OneToOne_Optional_Self] ON [e].[OneToOne_Optional_SelfId] = [e.OneToOne_Optional_Self].[Id]
ORDER BY [e.OneToOne_Optional_Self].[Id], [e].[Id]",
                //
                @"SELECT [e.OneToMany_Optional_Self].[Id], [e.OneToMany_Optional_Self].[Date], [e.OneToMany_Optional_Self].[Name], [e.OneToMany_Optional_Self].[OneToMany_Optional_Self_InverseId], [e.OneToMany_Optional_Self].[OneToMany_Required_Self_InverseId], [e.OneToMany_Optional_Self].[OneToOne_Optional_SelfId], [l.OneToOne_Optional_Self].[Id], [l.OneToOne_Optional_Self].[Date], [l.OneToOne_Optional_Self].[Name], [l.OneToOne_Optional_Self].[OneToMany_Optional_Self_InverseId], [l.OneToOne_Optional_Self].[OneToMany_Required_Self_InverseId], [l.OneToOne_Optional_Self].[OneToOne_Optional_SelfId]
FROM [Level1] AS [e.OneToMany_Optional_Self]
LEFT JOIN [Level1] AS [l.OneToOne_Optional_Self] ON [e.OneToMany_Optional_Self].[OneToOne_Optional_SelfId] = [l.OneToOne_Optional_Self].[Id]
INNER JOIN (
    SELECT DISTINCT [e1].[Id], [e.OneToOne_Optional_Self1].[Id] AS [Id0]
    FROM [Level1] AS [e1]
    LEFT JOIN [Level1] AS [e.OneToOne_Optional_Self1] ON [e1].[OneToOne_Optional_SelfId] = [e.OneToOne_Optional_Self1].[Id]
) AS [t0] ON [e.OneToMany_Optional_Self].[OneToMany_Optional_Self_InverseId] = [t0].[Id]
ORDER BY [t0].[Id0], [t0].[Id]",
                //
                @"SELECT [e.OneToOne_Optional_Self.OneToMany_Optional_Self].[Id], [e.OneToOne_Optional_Self.OneToMany_Optional_Self].[Date], [e.OneToOne_Optional_Self.OneToMany_Optional_Self].[Name], [e.OneToOne_Optional_Self.OneToMany_Optional_Self].[OneToMany_Optional_Self_InverseId], [e.OneToOne_Optional_Self.OneToMany_Optional_Self].[OneToMany_Required_Self_InverseId], [e.OneToOne_Optional_Self.OneToMany_Optional_Self].[OneToOne_Optional_SelfId]
FROM [Level1] AS [e.OneToOne_Optional_Self.OneToMany_Optional_Self]
INNER JOIN (
    SELECT DISTINCT [e.OneToOne_Optional_Self0].[Id]
    FROM [Level1] AS [e0]
    LEFT JOIN [Level1] AS [e.OneToOne_Optional_Self0] ON [e0].[OneToOne_Optional_SelfId] = [e.OneToOne_Optional_Self0].[Id]
) AS [t] ON [e.OneToOne_Optional_Self.OneToMany_Optional_Self].[OneToMany_Optional_Self_InverseId] = [t].[Id]
ORDER BY [t].[Id]");
        }

        public override void Multiple_complex_include_select()
        {
            base.Multiple_complex_include_select();

            AssertSql(
                @"SELECT [e].[Id], [e].[Date], [e].[Name], [e].[OneToMany_Optional_Self_InverseId], [e].[OneToMany_Required_Self_InverseId], [e].[OneToOne_Optional_SelfId], [e.OneToOne_Optional_FK].[Id], [e.OneToOne_Optional_FK].[Date], [e.OneToOne_Optional_FK].[Level1_Optional_Id], [e.OneToOne_Optional_FK].[Level1_Required_Id], [e.OneToOne_Optional_FK].[Name], [e.OneToOne_Optional_FK].[OneToMany_Optional_InverseId], [e.OneToOne_Optional_FK].[OneToMany_Optional_Self_InverseId], [e.OneToOne_Optional_FK].[OneToMany_Required_InverseId], [e.OneToOne_Optional_FK].[OneToMany_Required_Self_InverseId], [e.OneToOne_Optional_FK].[OneToOne_Optional_PK_InverseId], [e.OneToOne_Optional_FK].[OneToOne_Optional_SelfId]
FROM [Level1] AS [e]
LEFT JOIN [Level2] AS [e.OneToOne_Optional_FK] ON [e].[Id] = [e.OneToOne_Optional_FK].[Level1_Optional_Id]
ORDER BY [e.OneToOne_Optional_FK].[Id], [e].[Id]",
                //
                @"SELECT [e.OneToMany_Optional].[Id], [e.OneToMany_Optional].[Date], [e.OneToMany_Optional].[Level1_Optional_Id], [e.OneToMany_Optional].[Level1_Required_Id], [e.OneToMany_Optional].[Name], [e.OneToMany_Optional].[OneToMany_Optional_InverseId], [e.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [e.OneToMany_Optional].[OneToMany_Required_InverseId], [e.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [e.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [e.OneToMany_Optional].[OneToOne_Optional_SelfId], [l.OneToOne_Optional_FK].[Id], [l.OneToOne_Optional_FK].[Level2_Optional_Id], [l.OneToOne_Optional_FK].[Level2_Required_Id], [l.OneToOne_Optional_FK].[Name], [l.OneToOne_Optional_FK].[OneToMany_Optional_InverseId], [l.OneToOne_Optional_FK].[OneToMany_Optional_Self_InverseId], [l.OneToOne_Optional_FK].[OneToMany_Required_InverseId], [l.OneToOne_Optional_FK].[OneToMany_Required_Self_InverseId], [l.OneToOne_Optional_FK].[OneToOne_Optional_PK_InverseId], [l.OneToOne_Optional_FK].[OneToOne_Optional_SelfId]
FROM [Level2] AS [e.OneToMany_Optional]
LEFT JOIN [Level3] AS [l.OneToOne_Optional_FK] ON [e.OneToMany_Optional].[Id] = [l.OneToOne_Optional_FK].[Level2_Optional_Id]
INNER JOIN (
    SELECT DISTINCT [e1].[Id], [e.OneToOne_Optional_FK1].[Id] AS [Id0]
    FROM [Level1] AS [e1]
    LEFT JOIN [Level2] AS [e.OneToOne_Optional_FK1] ON [e1].[Id] = [e.OneToOne_Optional_FK1].[Level1_Optional_Id]
) AS [t0] ON [e.OneToMany_Optional].[OneToMany_Optional_InverseId] = [t0].[Id]
ORDER BY [t0].[Id0], [t0].[Id]",
                //
                @"SELECT [e.OneToOne_Optional_FK.OneToMany_Optional].[Id], [e.OneToOne_Optional_FK.OneToMany_Optional].[Level2_Optional_Id], [e.OneToOne_Optional_FK.OneToMany_Optional].[Level2_Required_Id], [e.OneToOne_Optional_FK.OneToMany_Optional].[Name], [e.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Optional_InverseId], [e.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [e.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Required_InverseId], [e.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [e.OneToOne_Optional_FK.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [e.OneToOne_Optional_FK.OneToMany_Optional].[OneToOne_Optional_SelfId]
FROM [Level3] AS [e.OneToOne_Optional_FK.OneToMany_Optional]
INNER JOIN (
    SELECT DISTINCT [e.OneToOne_Optional_FK0].[Id]
    FROM [Level1] AS [e0]
    LEFT JOIN [Level2] AS [e.OneToOne_Optional_FK0] ON [e0].[Id] = [e.OneToOne_Optional_FK0].[Level1_Optional_Id]
) AS [t] ON [e.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Optional_InverseId] = [t].[Id]
ORDER BY [t].[Id]");
        }

        public override void Select_nav_prop_reference_optional1()
        {
            base.Select_nav_prop_reference_optional1();

            AssertSql(
                @"SELECT [e.OneToOne_Optional_FK].[Name]
FROM [Level1] AS [e]
LEFT JOIN [Level2] AS [e.OneToOne_Optional_FK] ON [e].[Id] = [e.OneToOne_Optional_FK].[Level1_Optional_Id]");
        }

        public override void Select_nav_prop_reference_optional1_via_DefaultIfEmpty()
        {
            base.Select_nav_prop_reference_optional1_via_DefaultIfEmpty();

            AssertSql(
                @"SELECT [l2].[Name]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l2] ON [l1].[Id] = [l2].[Level1_Optional_Id]");
        }

        public override void Select_nav_prop_reference_optional2()
        {
            base.Select_nav_prop_reference_optional2();

            AssertSql(
                @"SELECT [e.OneToOne_Optional_FK].[Id]
FROM [Level1] AS [e]
LEFT JOIN [Level2] AS [e.OneToOne_Optional_FK] ON [e].[Id] = [e.OneToOne_Optional_FK].[Level1_Optional_Id]");
        }

        public override void Select_nav_prop_reference_optional2_via_DefaultIfEmpty()
        {
            base.Select_nav_prop_reference_optional2_via_DefaultIfEmpty();

            AssertSql(
                @"SELECT [l2].[Id]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l2] ON [l1].[Id] = [l2].[Level1_Optional_Id]");
        }

        public override void Select_nav_prop_reference_optional3()
        {
            base.Select_nav_prop_reference_optional3();

            AssertSql(
                @"SELECT [e.OneToOne_Optional_FK_Inverse].[Name]
FROM [Level2] AS [e]
LEFT JOIN [Level1] AS [e.OneToOne_Optional_FK_Inverse] ON [e].[Level1_Optional_Id] = [e.OneToOne_Optional_FK_Inverse].[Id]");
        }

        public override void Where_nav_prop_reference_optional1()
        {
            base.Where_nav_prop_reference_optional1();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Level1] AS [e]
LEFT JOIN [Level2] AS [e.OneToOne_Optional_FK] ON [e].[Id] = [e.OneToOne_Optional_FK].[Level1_Optional_Id]
WHERE [e.OneToOne_Optional_FK].[Name] IN (N'L2 05', N'L2 07')");
        }

        public override void Where_nav_prop_reference_optional1_via_DefaultIfEmpty()
        {
            base.Where_nav_prop_reference_optional1_via_DefaultIfEmpty();

            AssertSql(
                @"SELECT [l1].[Id]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l2Left] ON [l1].[Id] = [l2Left].[Level1_Optional_Id]
LEFT JOIN [Level2] AS [l2Right] ON [l1].[Id] = [l2Right].[Level1_Optional_Id]
WHERE ([l2Left].[Name] = N'L2 05') OR ([l2Right].[Name] = N'L2 07')");
        }

        public override void Where_nav_prop_reference_optional2()
        {
            base.Where_nav_prop_reference_optional2();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Level1] AS [e]
LEFT JOIN [Level2] AS [e.OneToOne_Optional_FK] ON [e].[Id] = [e.OneToOne_Optional_FK].[Level1_Optional_Id]
WHERE ([e.OneToOne_Optional_FK].[Name] = N'L2 05') OR (([e.OneToOne_Optional_FK].[Name] <> N'L2 42') OR [e.OneToOne_Optional_FK].[Name] IS NULL)");
        }

        public override void Where_nav_prop_reference_optional2_via_DefaultIfEmpty()
        {
            base.Where_nav_prop_reference_optional2_via_DefaultIfEmpty();

            AssertSql(
                @"SELECT [l1].[Id]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l2Left] ON [l1].[Id] = [l2Left].[Level1_Optional_Id]
LEFT JOIN [Level2] AS [l2Right] ON [l1].[Id] = [l2Right].[Level1_Optional_Id]
WHERE ([l2Left].[Name] = N'L2 05') OR (([l2Right].[Name] <> N'L2 42') OR [l2Right].[Name] IS NULL)");
        }

        public override void Select_multiple_nav_prop_reference_optional()
        {
            base.Select_multiple_nav_prop_reference_optional();

            AssertSql(
                @"SELECT [e.OneToOne_Optional_FK.OneToOne_Optional_FK].[Id]
FROM [Level1] AS [e]
LEFT JOIN [Level2] AS [e.OneToOne_Optional_FK] ON [e].[Id] = [e.OneToOne_Optional_FK].[Level1_Optional_Id]
LEFT JOIN [Level3] AS [e.OneToOne_Optional_FK.OneToOne_Optional_FK] ON [e.OneToOne_Optional_FK].[Id] = [e.OneToOne_Optional_FK.OneToOne_Optional_FK].[Level2_Optional_Id]");
        }

        public override void Where_multiple_nav_prop_reference_optional_member_compared_to_value()
        {
            base.Where_multiple_nav_prop_reference_optional_member_compared_to_value();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_InverseId], [l1].[OneToMany_Required_Self_InverseId], [l1].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l1.OneToOne_Optional_FK] ON [l1].[Id] = [l1.OneToOne_Optional_FK].[Level1_Optional_Id]
LEFT JOIN [Level3] AS [l1.OneToOne_Optional_FK.OneToOne_Optional_FK] ON [l1.OneToOne_Optional_FK].[Id] = [l1.OneToOne_Optional_FK.OneToOne_Optional_FK].[Level2_Optional_Id]
WHERE ([l1.OneToOne_Optional_FK.OneToOne_Optional_FK].[Name] <> N'L3 05') OR [l1.OneToOne_Optional_FK.OneToOne_Optional_FK].[Name] IS NULL");
        }

        public override void Where_multiple_nav_prop_reference_optional_member_compared_to_null()
        {
            base.Where_multiple_nav_prop_reference_optional_member_compared_to_null();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_InverseId], [l1].[OneToMany_Required_Self_InverseId], [l1].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l1.OneToOne_Optional_FK] ON [l1].[Id] = [l1.OneToOne_Optional_FK].[Level1_Optional_Id]
LEFT JOIN [Level3] AS [l1.OneToOne_Optional_FK.OneToOne_Optional_FK] ON [l1.OneToOne_Optional_FK].[Id] = [l1.OneToOne_Optional_FK.OneToOne_Optional_FK].[Level2_Optional_Id]
WHERE [l1.OneToOne_Optional_FK.OneToOne_Optional_FK].[Name] IS NOT NULL");
        }

        public override void Where_multiple_nav_prop_reference_optional_compared_to_null1()
        {
            base.Where_multiple_nav_prop_reference_optional_compared_to_null1();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_InverseId], [l1].[OneToMany_Required_Self_InverseId], [l1].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l1.OneToOne_Optional_FK] ON [l1].[Id] = [l1.OneToOne_Optional_FK].[Level1_Optional_Id]
LEFT JOIN [Level3] AS [l1.OneToOne_Optional_FK.OneToOne_Optional_FK] ON [l1.OneToOne_Optional_FK].[Id] = [l1.OneToOne_Optional_FK.OneToOne_Optional_FK].[Level2_Optional_Id]
WHERE [l1.OneToOne_Optional_FK.OneToOne_Optional_FK].[Id] IS NULL");
        }

        public override void Where_multiple_nav_prop_reference_optional_compared_to_null2()
        {
            base.Where_multiple_nav_prop_reference_optional_compared_to_null2();

            AssertSql(
                @"SELECT [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Name], [l3].[OneToMany_Optional_InverseId], [l3].[OneToMany_Optional_Self_InverseId], [l3].[OneToMany_Required_InverseId], [l3].[OneToMany_Required_Self_InverseId], [l3].[OneToOne_Optional_PK_InverseId], [l3].[OneToOne_Optional_SelfId]
FROM [Level3] AS [l3]
LEFT JOIN [Level2] AS [l3.OneToOne_Optional_FK_Inverse] ON [l3].[Level2_Optional_Id] = [l3.OneToOne_Optional_FK_Inverse].[Id]
WHERE [l3.OneToOne_Optional_FK_Inverse].[Level1_Optional_Id] IS NULL");
        }

        public override void Where_multiple_nav_prop_reference_optional_compared_to_null3()
        {
            base.Where_multiple_nav_prop_reference_optional_compared_to_null3();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_InverseId], [l1].[OneToMany_Required_Self_InverseId], [l1].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l1.OneToOne_Optional_FK] ON [l1].[Id] = [l1.OneToOne_Optional_FK].[Level1_Optional_Id]
LEFT JOIN [Level3] AS [l1.OneToOne_Optional_FK.OneToOne_Optional_FK] ON [l1.OneToOne_Optional_FK].[Id] = [l1.OneToOne_Optional_FK.OneToOne_Optional_FK].[Level2_Optional_Id]
WHERE [l1.OneToOne_Optional_FK.OneToOne_Optional_FK].[Id] IS NOT NULL");
        }

        public override void Where_multiple_nav_prop_reference_optional_compared_to_null4()
        {
            base.Where_multiple_nav_prop_reference_optional_compared_to_null4();

            AssertSql(
                @"SELECT [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Name], [l3].[OneToMany_Optional_InverseId], [l3].[OneToMany_Optional_Self_InverseId], [l3].[OneToMany_Required_InverseId], [l3].[OneToMany_Required_Self_InverseId], [l3].[OneToOne_Optional_PK_InverseId], [l3].[OneToOne_Optional_SelfId]
FROM [Level3] AS [l3]
LEFT JOIN [Level2] AS [l3.OneToOne_Optional_FK_Inverse] ON [l3].[Level2_Optional_Id] = [l3.OneToOne_Optional_FK_Inverse].[Id]
WHERE [l3.OneToOne_Optional_FK_Inverse].[Level1_Optional_Id] IS NOT NULL");
        }

        public override void Where_multiple_nav_prop_reference_optional_compared_to_null5()
        {
            base.Where_multiple_nav_prop_reference_optional_compared_to_null5();

            AssertSql(
                @"SELECT [e].[Id], [e].[Date], [e].[Name], [e].[OneToMany_Optional_Self_InverseId], [e].[OneToMany_Required_Self_InverseId], [e].[OneToOne_Optional_SelfId]
FROM [Level1] AS [e]
LEFT JOIN [Level2] AS [e.OneToOne_Optional_FK] ON [e].[Id] = [e.OneToOne_Optional_FK].[Level1_Optional_Id]
LEFT JOIN [Level3] AS [e.OneToOne_Optional_FK.OneToOne_Required_FK] ON [e.OneToOne_Optional_FK].[Id] = [e.OneToOne_Optional_FK.OneToOne_Required_FK].[Level2_Required_Id]
LEFT JOIN [Level4] AS [e.OneToOne_Optional_FK.OneToOne_Required_FK.OneToOne_Required_FK] ON [e.OneToOne_Optional_FK.OneToOne_Required_FK].[Id] = [e.OneToOne_Optional_FK.OneToOne_Required_FK.OneToOne_Required_FK].[Level3_Required_Id]
WHERE [e.OneToOne_Optional_FK.OneToOne_Required_FK.OneToOne_Required_FK].[Id] IS NULL");
        }

        public override void Select_multiple_nav_prop_reference_required()
        {
            base.Select_multiple_nav_prop_reference_required();

            AssertSql(
                @"SELECT [e.OneToOne_Required_FK.OneToOne_Required_FK].[Id]
FROM [Level1] AS [e]
LEFT JOIN [Level2] AS [e.OneToOne_Required_FK] ON [e].[Id] = [e.OneToOne_Required_FK].[Level1_Required_Id]
LEFT JOIN [Level3] AS [e.OneToOne_Required_FK.OneToOne_Required_FK] ON [e.OneToOne_Required_FK].[Id] = [e.OneToOne_Required_FK.OneToOne_Required_FK].[Level2_Required_Id]");
        }

        public override void Select_multiple_nav_prop_reference_required2()
        {
            base.Select_multiple_nav_prop_reference_required2();

            AssertSql(
                @"SELECT [e.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Id]
FROM [Level3] AS [e]
INNER JOIN [Level2] AS [e.OneToOne_Required_FK_Inverse] ON [e].[Level2_Required_Id] = [e.OneToOne_Required_FK_Inverse].[Id]
INNER JOIN [Level1] AS [e.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse] ON [e.OneToOne_Required_FK_Inverse].[Level1_Required_Id] = [e.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Id]");
        }

        public override void Select_multiple_nav_prop_optional_required()
        {
            base.Select_multiple_nav_prop_optional_required();

            AssertSql(
                @"SELECT [l1.OneToOne_Optional_FK.OneToOne_Required_FK].[Id]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l1.OneToOne_Optional_FK] ON [l1].[Id] = [l1.OneToOne_Optional_FK].[Level1_Optional_Id]
LEFT JOIN [Level3] AS [l1.OneToOne_Optional_FK.OneToOne_Required_FK] ON [l1.OneToOne_Optional_FK].[Id] = [l1.OneToOne_Optional_FK.OneToOne_Required_FK].[Level2_Required_Id]");
        }

        public override void Where_multiple_nav_prop_optional_required()
        {
            base.Where_multiple_nav_prop_optional_required();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_InverseId], [l1].[OneToMany_Required_Self_InverseId], [l1].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l1.OneToOne_Optional_FK] ON [l1].[Id] = [l1.OneToOne_Optional_FK].[Level1_Optional_Id]
LEFT JOIN [Level3] AS [l1.OneToOne_Optional_FK.OneToOne_Required_FK] ON [l1.OneToOne_Optional_FK].[Id] = [l1.OneToOne_Optional_FK.OneToOne_Required_FK].[Level2_Required_Id]
WHERE ([l1.OneToOne_Optional_FK.OneToOne_Required_FK].[Name] <> N'L3 05') OR [l1.OneToOne_Optional_FK.OneToOne_Required_FK].[Name] IS NULL");
        }

        public override void SelectMany_navigation_comparison1()
        {
            base.SelectMany_navigation_comparison1();

            AssertSql(
                @"SELECT [l11].[Id] AS [Id1], [l12].[Id] AS [Id2]
FROM [Level1] AS [l11]
CROSS JOIN [Level1] AS [l12]
WHERE [l11].[Id] = [l12].[Id]");
        }

        public override void SelectMany_navigation_comparison2()
        {
            base.SelectMany_navigation_comparison2();

            AssertSql(
                @"SELECT [l1].[Id] AS [Id1], [l2].[Id] AS [Id2]
FROM [Level1] AS [l1]
CROSS JOIN [Level2] AS [l2]
WHERE [l1].[Id] = [l2].[Level1_Optional_Id]");
        }

        public override void SelectMany_navigation_comparison3()
        {
            base.SelectMany_navigation_comparison3();

            AssertSql(
                @"SELECT [l1].[Id] AS [Id1], [l2].[Id] AS [Id2]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l1.OneToOne_Optional_FK] ON [l1].[Id] = [l1.OneToOne_Optional_FK].[Level1_Optional_Id]
CROSS JOIN [Level2] AS [l2]
WHERE [l1.OneToOne_Optional_FK].[Id] = [l2].[Id]");
        }

        public override void Where_complex_predicate_with_with_nav_prop_and_OrElse1()
        {
            base.Where_complex_predicate_with_with_nav_prop_and_OrElse1();

            AssertSql(
                @"SELECT [l1].[Id] AS [Id1], [l2].[Id] AS [Id2]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l1.OneToOne_Optional_FK] ON [l1].[Id] = [l1.OneToOne_Optional_FK].[Level1_Optional_Id]
CROSS JOIN [Level2] AS [l2]
INNER JOIN [Level1] AS [l2.OneToOne_Required_FK_Inverse] ON [l2].[Level1_Required_Id] = [l2.OneToOne_Required_FK_Inverse].[Id]
WHERE ([l1.OneToOne_Optional_FK].[Name] = N'L2 01') OR (([l2.OneToOne_Required_FK_Inverse].[Name] <> N'Bar') OR [l2.OneToOne_Required_FK_Inverse].[Name] IS NULL)");
        }

        public override void Where_complex_predicate_with_with_nav_prop_and_OrElse2()
        {
            base.Where_complex_predicate_with_with_nav_prop_and_OrElse2();

            AssertSql(
                @"SELECT [l1].[Id]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l1.OneToOne_Optional_FK] ON [l1].[Id] = [l1.OneToOne_Optional_FK].[Level1_Optional_Id]
LEFT JOIN [Level3] AS [l1.OneToOne_Optional_FK.OneToOne_Required_FK] ON [l1.OneToOne_Optional_FK].[Id] = [l1.OneToOne_Optional_FK.OneToOne_Required_FK].[Level2_Required_Id]
WHERE ([l1.OneToOne_Optional_FK.OneToOne_Required_FK].[Name] = N'L3 05') OR (([l1.OneToOne_Optional_FK].[Name] <> N'L2 05') OR [l1.OneToOne_Optional_FK].[Name] IS NULL)");
        }

        public override void Where_complex_predicate_with_with_nav_prop_and_OrElse3()
        {
            base.Where_complex_predicate_with_with_nav_prop_and_OrElse3();

            AssertSql(
                @"SELECT [l1].[Id]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l1.OneToOne_Required_FK] ON [l1].[Id] = [l1.OneToOne_Required_FK].[Level1_Required_Id]
LEFT JOIN [Level3] AS [l1.OneToOne_Required_FK.OneToOne_Optional_FK] ON [l1.OneToOne_Required_FK].[Id] = [l1.OneToOne_Required_FK.OneToOne_Optional_FK].[Level2_Optional_Id]
LEFT JOIN [Level2] AS [l1.OneToOne_Optional_FK] ON [l1].[Id] = [l1.OneToOne_Optional_FK].[Level1_Optional_Id]
WHERE (([l1.OneToOne_Optional_FK].[Name] <> N'L2 05') OR [l1.OneToOne_Optional_FK].[Name] IS NULL) OR ([l1.OneToOne_Required_FK.OneToOne_Optional_FK].[Name] = N'L3 05')");
        }

        public override void Where_complex_predicate_with_with_nav_prop_and_OrElse4()
        {
            base.Where_complex_predicate_with_with_nav_prop_and_OrElse4();

            AssertSql(
                @"SELECT [l3].[Id]
FROM [Level3] AS [l3]
INNER JOIN [Level2] AS [l3.OneToOne_Required_FK_Inverse] ON [l3].[Level2_Required_Id] = [l3.OneToOne_Required_FK_Inverse].[Id]
LEFT JOIN [Level1] AS [l3.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK_Inverse] ON [l3.OneToOne_Required_FK_Inverse].[Level1_Optional_Id] = [l3.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK_Inverse].[Id]
LEFT JOIN [Level2] AS [l3.OneToOne_Optional_FK_Inverse] ON [l3].[Level2_Optional_Id] = [l3.OneToOne_Optional_FK_Inverse].[Id]
WHERE (([l3.OneToOne_Optional_FK_Inverse].[Name] <> N'L2 05') OR [l3.OneToOne_Optional_FK_Inverse].[Name] IS NULL) OR ([l3.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK_Inverse].[Name] = N'L1 05')");
        }

        public override void Complex_navigations_with_predicate_projected_into_anonymous_type()
        {
            base.Complex_navigations_with_predicate_projected_into_anonymous_type();

            AssertSql(
                @"SELECT [e].[Name], [e.OneToOne_Required_FK.OneToOne_Optional_FK].[Id]
FROM [Level1] AS [e]
LEFT JOIN [Level2] AS [e.OneToOne_Required_FK] ON [e].[Id] = [e.OneToOne_Required_FK].[Level1_Required_Id]
LEFT JOIN [Level3] AS [e.OneToOne_Required_FK.OneToOne_Required_FK] ON [e.OneToOne_Required_FK].[Id] = [e.OneToOne_Required_FK.OneToOne_Required_FK].[Level2_Required_Id]
LEFT JOIN [Level3] AS [e.OneToOne_Required_FK.OneToOne_Optional_FK] ON [e.OneToOne_Required_FK].[Id] = [e.OneToOne_Required_FK.OneToOne_Optional_FK].[Level2_Optional_Id]
WHERE ((([e.OneToOne_Required_FK.OneToOne_Required_FK].[Id] = [e.OneToOne_Required_FK.OneToOne_Optional_FK].[Id]) AND ([e.OneToOne_Required_FK.OneToOne_Required_FK].[Id] IS NOT NULL AND [e.OneToOne_Required_FK.OneToOne_Optional_FK].[Id] IS NOT NULL)) OR ([e.OneToOne_Required_FK.OneToOne_Required_FK].[Id] IS NULL AND [e.OneToOne_Required_FK.OneToOne_Optional_FK].[Id] IS NULL)) AND (([e.OneToOne_Required_FK.OneToOne_Optional_FK].[Id] <> 7) OR [e.OneToOne_Required_FK.OneToOne_Optional_FK].[Id] IS NULL)");
        }

        public override void Complex_navigations_with_predicate_projected_into_anonymous_type2()
        {
            base.Complex_navigations_with_predicate_projected_into_anonymous_type2();

            AssertSql(
                @"SELECT [e].[Name], [e.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK_Inverse].[Id]
FROM [Level3] AS [e]
INNER JOIN [Level2] AS [e.OneToOne_Required_FK_Inverse] ON [e].[Level2_Required_Id] = [e.OneToOne_Required_FK_Inverse].[Id]
INNER JOIN [Level1] AS [e.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse] ON [e.OneToOne_Required_FK_Inverse].[Level1_Required_Id] = [e.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Id]
LEFT JOIN [Level1] AS [e.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK_Inverse] ON [e.OneToOne_Required_FK_Inverse].[Level1_Optional_Id] = [e.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK_Inverse].[Id]
WHERE ([e.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Id] = [e.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK_Inverse].[Id]) AND (([e.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK_Inverse].[Id] <> 7) OR [e.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK_Inverse].[Id] IS NULL)");
        }

        public override void Optional_navigation_projected_into_DTO()
        {
            base.Optional_navigation_projected_into_DTO();

            AssertSql(
                @"SELECT [e].[Id], [e].[Name], CASE
    WHEN [e.OneToOne_Optional_FK].[Id] IS NOT NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END, [e.OneToOne_Optional_FK].[Id] AS [Id0], [e.OneToOne_Optional_FK].[Name] AS [Name0]
FROM [Level1] AS [e]
LEFT JOIN [Level2] AS [e.OneToOne_Optional_FK] ON [e].[Id] = [e.OneToOne_Optional_FK].[Level1_Optional_Id]");
        }

        public override void OrderBy_nav_prop_reference_optional()
        {
            base.OrderBy_nav_prop_reference_optional();

            AssertSql(
                @"SELECT [e].[Id]
FROM [Level1] AS [e]
LEFT JOIN [Level2] AS [e.OneToOne_Optional_FK] ON [e].[Id] = [e.OneToOne_Optional_FK].[Level1_Optional_Id]
ORDER BY [e.OneToOne_Optional_FK].[Name], [e].[Id]");
        }

        public override void OrderBy_nav_prop_reference_optional_via_DefaultIfEmpty()
        {
            base.OrderBy_nav_prop_reference_optional_via_DefaultIfEmpty();

            AssertSql(
                @"SELECT [l1].[Id]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l2] ON [l1].[Id] = [l2].[Level1_Optional_Id]
ORDER BY [l2].[Name], [l1].[Id]");
        }

        public override void Result_operator_nav_prop_reference_optional_Sum()
        {
            base.Result_operator_nav_prop_reference_optional_Sum();

            AssertSql(
                @"SELECT SUM([e.OneToOne_Optional_FK].[Level1_Required_Id])
FROM [Level1] AS [e]
LEFT JOIN [Level2] AS [e.OneToOne_Optional_FK] ON [e].[Id] = [e.OneToOne_Optional_FK].[Level1_Optional_Id]");
        }

        public override void Result_operator_nav_prop_reference_optional_Min()
        {
            base.Result_operator_nav_prop_reference_optional_Min();

            AssertSql(
                @"SELECT MIN([e.OneToOne_Optional_FK].[Level1_Required_Id])
FROM [Level1] AS [e]
LEFT JOIN [Level2] AS [e.OneToOne_Optional_FK] ON [e].[Id] = [e.OneToOne_Optional_FK].[Level1_Optional_Id]");
        }

        public override void Result_operator_nav_prop_reference_optional_Max()
        {
            base.Result_operator_nav_prop_reference_optional_Max();

            AssertSql(
                @"SELECT MAX([e.OneToOne_Optional_FK].[Level1_Required_Id])
FROM [Level1] AS [e]
LEFT JOIN [Level2] AS [e.OneToOne_Optional_FK] ON [e].[Id] = [e.OneToOne_Optional_FK].[Level1_Optional_Id]");
        }

        public override void Result_operator_nav_prop_reference_optional_Average()
        {
            base.Result_operator_nav_prop_reference_optional_Average();

            AssertSql(
                @"SELECT AVG(CAST([e.OneToOne_Optional_FK].[Level1_Required_Id] AS float))
FROM [Level1] AS [e]
LEFT JOIN [Level2] AS [e.OneToOne_Optional_FK] ON [e].[Id] = [e.OneToOne_Optional_FK].[Level1_Optional_Id]");
        }

        public override void Result_operator_nav_prop_reference_optional_via_DefaultIfEmpty()
        {
            base.Result_operator_nav_prop_reference_optional_via_DefaultIfEmpty();

            AssertSql(
                @"SELECT SUM(CASE
    WHEN [l2].[Id] IS NULL
    THEN 0 ELSE [l2].[Level1_Required_Id]
END)
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l2] ON [l1].[Id] = [l2].[Level1_Optional_Id]");
        }

        public override void Include_with_optional_navigation()
        {
            base.Include_with_optional_navigation();

            AssertSql(
                @"SELECT [e].[Id], [e].[Date], [e].[Name], [e].[OneToMany_Optional_Self_InverseId], [e].[OneToMany_Required_Self_InverseId], [e].[OneToOne_Optional_SelfId], [e.OneToOne_Optional_FK].[Id], [e.OneToOne_Optional_FK].[Date], [e.OneToOne_Optional_FK].[Level1_Optional_Id], [e.OneToOne_Optional_FK].[Level1_Required_Id], [e.OneToOne_Optional_FK].[Name], [e.OneToOne_Optional_FK].[OneToMany_Optional_InverseId], [e.OneToOne_Optional_FK].[OneToMany_Optional_Self_InverseId], [e.OneToOne_Optional_FK].[OneToMany_Required_InverseId], [e.OneToOne_Optional_FK].[OneToMany_Required_Self_InverseId], [e.OneToOne_Optional_FK].[OneToOne_Optional_PK_InverseId], [e.OneToOne_Optional_FK].[OneToOne_Optional_SelfId]
FROM [Level1] AS [e]
LEFT JOIN [Level2] AS [e.OneToOne_Optional_FK] ON [e].[Id] = [e.OneToOne_Optional_FK].[Level1_Optional_Id]
WHERE ([e.OneToOne_Optional_FK].[Name] <> N'L2 05') OR [e.OneToOne_Optional_FK].[Name] IS NULL");
        }

        public override void Include_nested_with_optional_navigation()
        {
            base.Include_nested_with_optional_navigation();

            AssertSql(
                @"SELECT [e].[Id], [e].[Date], [e].[Name], [e].[OneToMany_Optional_Self_InverseId], [e].[OneToMany_Required_Self_InverseId], [e].[OneToOne_Optional_SelfId], [e.OneToOne_Optional_FK].[Id], [e.OneToOne_Optional_FK].[Date], [e.OneToOne_Optional_FK].[Level1_Optional_Id], [e.OneToOne_Optional_FK].[Level1_Required_Id], [e.OneToOne_Optional_FK].[Name], [e.OneToOne_Optional_FK].[OneToMany_Optional_InverseId], [e.OneToOne_Optional_FK].[OneToMany_Optional_Self_InverseId], [e.OneToOne_Optional_FK].[OneToMany_Required_InverseId], [e.OneToOne_Optional_FK].[OneToMany_Required_Self_InverseId], [e.OneToOne_Optional_FK].[OneToOne_Optional_PK_InverseId], [e.OneToOne_Optional_FK].[OneToOne_Optional_SelfId]
FROM [Level1] AS [e]
LEFT JOIN [Level2] AS [e.OneToOne_Optional_FK] ON [e].[Id] = [e.OneToOne_Optional_FK].[Level1_Optional_Id]
WHERE ([e.OneToOne_Optional_FK].[Name] <> N'L2 09') OR [e.OneToOne_Optional_FK].[Name] IS NULL
ORDER BY [e.OneToOne_Optional_FK].[Id]",
                //
                @"SELECT [e.OneToOne_Optional_FK.OneToMany_Required].[Id], [e.OneToOne_Optional_FK.OneToMany_Required].[Level2_Optional_Id], [e.OneToOne_Optional_FK.OneToMany_Required].[Level2_Required_Id], [e.OneToOne_Optional_FK.OneToMany_Required].[Name], [e.OneToOne_Optional_FK.OneToMany_Required].[OneToMany_Optional_InverseId], [e.OneToOne_Optional_FK.OneToMany_Required].[OneToMany_Optional_Self_InverseId], [e.OneToOne_Optional_FK.OneToMany_Required].[OneToMany_Required_InverseId], [e.OneToOne_Optional_FK.OneToMany_Required].[OneToMany_Required_Self_InverseId], [e.OneToOne_Optional_FK.OneToMany_Required].[OneToOne_Optional_PK_InverseId], [e.OneToOne_Optional_FK.OneToMany_Required].[OneToOne_Optional_SelfId], [l.OneToOne_Required_FK].[Id], [l.OneToOne_Required_FK].[Level3_Optional_Id], [l.OneToOne_Required_FK].[Level3_Required_Id], [l.OneToOne_Required_FK].[Name], [l.OneToOne_Required_FK].[OneToMany_Optional_InverseId], [l.OneToOne_Required_FK].[OneToMany_Optional_Self_InverseId], [l.OneToOne_Required_FK].[OneToMany_Required_InverseId], [l.OneToOne_Required_FK].[OneToMany_Required_Self_InverseId], [l.OneToOne_Required_FK].[OneToOne_Optional_PK_InverseId], [l.OneToOne_Required_FK].[OneToOne_Optional_SelfId]
FROM [Level3] AS [e.OneToOne_Optional_FK.OneToMany_Required]
LEFT JOIN [Level4] AS [l.OneToOne_Required_FK] ON [e.OneToOne_Optional_FK.OneToMany_Required].[Id] = [l.OneToOne_Required_FK].[Level3_Required_Id]
INNER JOIN (
    SELECT DISTINCT [e.OneToOne_Optional_FK0].[Id]
    FROM [Level1] AS [e0]
    LEFT JOIN [Level2] AS [e.OneToOne_Optional_FK0] ON [e0].[Id] = [e.OneToOne_Optional_FK0].[Level1_Optional_Id]
    WHERE ([e.OneToOne_Optional_FK0].[Name] <> N'L2 09') OR [e.OneToOne_Optional_FK0].[Name] IS NULL
) AS [t] ON [e.OneToOne_Optional_FK.OneToMany_Required].[OneToMany_Required_InverseId] = [t].[Id]
ORDER BY [t].[Id]");
        }

        public override void Include_with_groupjoin_skip_and_take()
        {
            base.Include_with_groupjoin_skip_and_take();

            if (SupportsOffset)
            {
                AssertContainsSql(
                    @"SELECT [e].[Id], [e].[Date], [e].[Name], [e].[OneToMany_Optional_Self_InverseId], [e].[OneToMany_Required_Self_InverseId], [e].[OneToOne_Optional_SelfId], [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_InverseId], [l2].[OneToMany_Optional_Self_InverseId], [l2].[OneToMany_Required_InverseId], [l2].[OneToMany_Required_Self_InverseId], [l2].[OneToOne_Optional_PK_InverseId], [l2].[OneToOne_Optional_SelfId]
FROM [Level1] AS [e]
LEFT JOIN [Level2] AS [l2] ON [e].[Id] = [l2].[Level1_Optional_Id]
WHERE ([e].[Name] <> N'L1 03') OR [e].[Name] IS NULL
ORDER BY [e].[Id]",
                    //
                    @"SELECT [e1].[Id], [e1].[Date], [e1].[Name], [e1].[OneToMany_Optional_Self_InverseId], [e1].[OneToMany_Required_Self_InverseId], [e1].[OneToOne_Optional_SelfId], [l21].[Id], [l21].[Date], [l21].[Level1_Optional_Id], [l21].[Level1_Required_Id], [l21].[Name], [l21].[OneToMany_Optional_InverseId], [l21].[OneToMany_Optional_Self_InverseId], [l21].[OneToMany_Required_InverseId], [l21].[OneToMany_Required_Self_InverseId], [l21].[OneToOne_Optional_PK_InverseId], [l21].[OneToOne_Optional_SelfId]
FROM [Level1] AS [e1]
LEFT JOIN [Level2] AS [l21] ON [e1].[Id] = [l21].[Level1_Optional_Id]
WHERE ([e1].[Name] <> N'L1 03') OR [e1].[Name] IS NULL
ORDER BY [e1].[Id]",
                    //
                    @"SELECT [e.OneToMany_Optional].[Id], [e.OneToMany_Optional].[Date], [e.OneToMany_Optional].[Level1_Optional_Id], [e.OneToMany_Optional].[Level1_Required_Id], [e.OneToMany_Optional].[Name], [e.OneToMany_Optional].[OneToMany_Optional_InverseId], [e.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [e.OneToMany_Optional].[OneToMany_Required_InverseId], [e.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [e.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [e.OneToMany_Optional].[OneToOne_Optional_SelfId], [l.OneToOne_Optional_FK].[Id], [l.OneToOne_Optional_FK].[Level2_Optional_Id], [l.OneToOne_Optional_FK].[Level2_Required_Id], [l.OneToOne_Optional_FK].[Name], [l.OneToOne_Optional_FK].[OneToMany_Optional_InverseId], [l.OneToOne_Optional_FK].[OneToMany_Optional_Self_InverseId], [l.OneToOne_Optional_FK].[OneToMany_Required_InverseId], [l.OneToOne_Optional_FK].[OneToMany_Required_Self_InverseId], [l.OneToOne_Optional_FK].[OneToOne_Optional_PK_InverseId], [l.OneToOne_Optional_FK].[OneToOne_Optional_SelfId]
FROM [Level2] AS [e.OneToMany_Optional]
LEFT JOIN [Level3] AS [l.OneToOne_Optional_FK] ON [e.OneToMany_Optional].[Id] = [l.OneToOne_Optional_FK].[Level2_Optional_Id]");
            }
        }

        public override void Join_flattening_bug_4539()
        {
            base.Join_flattening_bug_4539();

            AssertSql(
                @"SELECT [l1_Optional].[Id], [l1_Optional].[Date], [l1_Optional].[Level1_Optional_Id], [l1_Optional].[Level1_Required_Id], [l1_Optional].[Name], [l1_Optional].[OneToMany_Optional_InverseId], [l1_Optional].[OneToMany_Optional_Self_InverseId], [l1_Optional].[OneToMany_Required_InverseId], [l1_Optional].[OneToMany_Required_Self_InverseId], [l1_Optional].[OneToOne_Optional_PK_InverseId], [l1_Optional].[OneToOne_Optional_SelfId], [l2_Required_Reverse].[Id], [l2_Required_Reverse].[Date], [l2_Required_Reverse].[Name], [l2_Required_Reverse].[OneToMany_Optional_Self_InverseId], [l2_Required_Reverse].[OneToMany_Required_Self_InverseId], [l2_Required_Reverse].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l1_Optional] ON [l1].[Id] = [l1_Optional].[Level1_Optional_Id]
CROSS JOIN [Level2] AS [l2]
INNER JOIN [Level1] AS [l2_Required_Reverse] ON [l2].[Level1_Required_Id] = [l2_Required_Reverse].[Id]");
        }

        public override void Query_source_materialization_bug_4547()
        {
            base.Query_source_materialization_bug_4547();

            AssertSql(
                @"SELECT [e1].[Id]
FROM [Level3] AS [e3]
INNER JOIN [Level1] AS [e1] ON [e3].[Id] = (
    SELECT TOP(1) [subQuery30].[Id]
    FROM [Level2] AS [subQuery20]
    LEFT JOIN [Level3] AS [subQuery30] ON [subQuery20].[Id] = [subQuery30].[Level2_Optional_Id]
    ORDER BY [subQuery30].[Id]
)");
        }

        public override void SelectMany_navigation_property()
        {
            base.SelectMany_navigation_property();

            AssertSql(
                @"SELECT [l1.OneToMany_Optional].[Id], [l1.OneToMany_Optional].[Date], [l1.OneToMany_Optional].[Level1_Optional_Id], [l1.OneToMany_Optional].[Level1_Required_Id], [l1.OneToMany_Optional].[Name], [l1.OneToMany_Optional].[OneToMany_Optional_InverseId], [l1.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [l1.OneToMany_Optional].[OneToMany_Required_InverseId], [l1.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [l1.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [l1.OneToMany_Optional].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
INNER JOIN [Level2] AS [l1.OneToMany_Optional] ON [l1].[Id] = [l1.OneToMany_Optional].[OneToMany_Optional_InverseId]");
        }

        public override void SelectMany_navigation_property_and_projection()
        {
            base.SelectMany_navigation_property_and_projection();

            AssertSql(
                @"SELECT [l1.OneToMany_Optional].[Name]
FROM [Level1] AS [l1]
INNER JOIN [Level2] AS [l1.OneToMany_Optional] ON [l1].[Id] = [l1.OneToMany_Optional].[OneToMany_Optional_InverseId]");
        }

        public override void SelectMany_navigation_property_and_filter_before()
        {
            base.SelectMany_navigation_property_and_filter_before();

            AssertSql(
                @"SELECT [e.OneToMany_Optional].[Id], [e.OneToMany_Optional].[Date], [e.OneToMany_Optional].[Level1_Optional_Id], [e.OneToMany_Optional].[Level1_Required_Id], [e.OneToMany_Optional].[Name], [e.OneToMany_Optional].[OneToMany_Optional_InverseId], [e.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [e.OneToMany_Optional].[OneToMany_Required_InverseId], [e.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [e.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [e.OneToMany_Optional].[OneToOne_Optional_SelfId]
FROM [Level1] AS [e]
INNER JOIN [Level2] AS [e.OneToMany_Optional] ON [e].[Id] = [e.OneToMany_Optional].[OneToMany_Optional_InverseId]
WHERE [e].[Id] = 1");
        }

        public override void SelectMany_navigation_property_and_filter_after()
        {
            base.SelectMany_navigation_property_and_filter_after();

            AssertSql(
                @"SELECT [l1.OneToMany_Optional].[Id], [l1.OneToMany_Optional].[Date], [l1.OneToMany_Optional].[Level1_Optional_Id], [l1.OneToMany_Optional].[Level1_Required_Id], [l1.OneToMany_Optional].[Name], [l1.OneToMany_Optional].[OneToMany_Optional_InverseId], [l1.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [l1.OneToMany_Optional].[OneToMany_Required_InverseId], [l1.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [l1.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [l1.OneToMany_Optional].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
INNER JOIN [Level2] AS [l1.OneToMany_Optional] ON [l1].[Id] = [l1.OneToMany_Optional].[OneToMany_Optional_InverseId]
WHERE [l1.OneToMany_Optional].[Id] <> 6");
        }

        public override void SelectMany_nested_navigation_property_required()
        {
            base.SelectMany_nested_navigation_property_required();

            AssertSql(
                @"SELECT [l1.OneToOne_Required_FK.OneToMany_Optional].[Id], [l1.OneToOne_Required_FK.OneToMany_Optional].[Level2_Optional_Id], [l1.OneToOne_Required_FK.OneToMany_Optional].[Level2_Required_Id], [l1.OneToOne_Required_FK.OneToMany_Optional].[Name], [l1.OneToOne_Required_FK.OneToMany_Optional].[OneToMany_Optional_InverseId], [l1.OneToOne_Required_FK.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [l1.OneToOne_Required_FK.OneToMany_Optional].[OneToMany_Required_InverseId], [l1.OneToOne_Required_FK.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [l1.OneToOne_Required_FK.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [l1.OneToOne_Required_FK.OneToMany_Optional].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
INNER JOIN [Level2] AS [l1.OneToOne_Required_FK] ON [l1].[Id] = [l1.OneToOne_Required_FK].[Level1_Required_Id]
INNER JOIN [Level3] AS [l1.OneToOne_Required_FK.OneToMany_Optional] ON [l1.OneToOne_Required_FK].[Id] = [l1.OneToOne_Required_FK.OneToMany_Optional].[OneToMany_Optional_InverseId]");
        }

        public override void SelectMany_nested_navigation_property_optional_and_projection()
        {
            base.SelectMany_nested_navigation_property_optional_and_projection();

            AssertSql(
                @"SELECT [l1.OneToOne_Optional_FK.OneToMany_Optional].[Name]
FROM [Level1] AS [l1]
INNER JOIN [Level2] AS [l1.OneToOne_Optional_FK] ON [l1].[Id] = [l1.OneToOne_Optional_FK].[Level1_Optional_Id]
INNER JOIN [Level3] AS [l1.OneToOne_Optional_FK.OneToMany_Optional] ON [l1.OneToOne_Optional_FK].[Id] = [l1.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Optional_InverseId]");
        }

        public override void Multiple_SelectMany_calls()
        {
            base.Multiple_SelectMany_calls();

            AssertSql(
                @"SELECT [e.OneToMany_Optional.OneToMany_Optional].[Id], [e.OneToMany_Optional.OneToMany_Optional].[Level2_Optional_Id], [e.OneToMany_Optional.OneToMany_Optional].[Level2_Required_Id], [e.OneToMany_Optional.OneToMany_Optional].[Name], [e.OneToMany_Optional.OneToMany_Optional].[OneToMany_Optional_InverseId], [e.OneToMany_Optional.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [e.OneToMany_Optional.OneToMany_Optional].[OneToMany_Required_InverseId], [e.OneToMany_Optional.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [e.OneToMany_Optional.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [e.OneToMany_Optional.OneToMany_Optional].[OneToOne_Optional_SelfId]
FROM [Level1] AS [e]
INNER JOIN [Level2] AS [e.OneToMany_Optional] ON [e].[Id] = [e.OneToMany_Optional].[OneToMany_Optional_InverseId]
INNER JOIN [Level3] AS [e.OneToMany_Optional.OneToMany_Optional] ON [e.OneToMany_Optional].[Id] = [e.OneToMany_Optional.OneToMany_Optional].[OneToMany_Optional_InverseId]");
        }

        public override void SelectMany_navigation_property_with_another_navigation_in_subquery()
        {
            base.SelectMany_navigation_property_with_another_navigation_in_subquery();

            AssertSql(
                @"SELECT [l1.OneToMany_Optional.OneToOne_Optional_FK].[Id], [l1.OneToMany_Optional.OneToOne_Optional_FK].[Level2_Optional_Id], [l1.OneToMany_Optional.OneToOne_Optional_FK].[Level2_Required_Id], [l1.OneToMany_Optional.OneToOne_Optional_FK].[Name], [l1.OneToMany_Optional.OneToOne_Optional_FK].[OneToMany_Optional_InverseId], [l1.OneToMany_Optional.OneToOne_Optional_FK].[OneToMany_Optional_Self_InverseId], [l1.OneToMany_Optional.OneToOne_Optional_FK].[OneToMany_Required_InverseId], [l1.OneToMany_Optional.OneToOne_Optional_FK].[OneToMany_Required_Self_InverseId], [l1.OneToMany_Optional.OneToOne_Optional_FK].[OneToOne_Optional_PK_InverseId], [l1.OneToMany_Optional.OneToOne_Optional_FK].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
INNER JOIN [Level2] AS [l1.OneToMany_Optional] ON [l1].[Id] = [l1.OneToMany_Optional].[OneToMany_Optional_InverseId]
LEFT JOIN [Level3] AS [l1.OneToMany_Optional.OneToOne_Optional_FK] ON [l1.OneToMany_Optional].[Id] = [l1.OneToMany_Optional.OneToOne_Optional_FK].[Level2_Optional_Id]");
        }

        [Fact]
        public void Multiple_complex_includes_from_sql()
        {
            using (var context = CreateContext())
            {
                var query = context.LevelOne.FromSql("SELECT * FROM [Level1]")
                    .Include(e => e.OneToOne_Optional_FK)
                    .ThenInclude(e => e.OneToMany_Optional)
                    .Include(e => e.OneToMany_Optional)
                    .ThenInclude(e => e.OneToOne_Optional_FK);

                var results = query.ToList();

                Assert.Equal(13, results.Count);

                AssertSql(
                    @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_InverseId], [l].[OneToMany_Required_Self_InverseId], [l].[OneToOne_Optional_SelfId], [l.OneToOne_Optional_FK].[Id], [l.OneToOne_Optional_FK].[Date], [l.OneToOne_Optional_FK].[Level1_Optional_Id], [l.OneToOne_Optional_FK].[Level1_Required_Id], [l.OneToOne_Optional_FK].[Name], [l.OneToOne_Optional_FK].[OneToMany_Optional_InverseId], [l.OneToOne_Optional_FK].[OneToMany_Optional_Self_InverseId], [l.OneToOne_Optional_FK].[OneToMany_Required_InverseId], [l.OneToOne_Optional_FK].[OneToMany_Required_Self_InverseId], [l.OneToOne_Optional_FK].[OneToOne_Optional_PK_InverseId], [l.OneToOne_Optional_FK].[OneToOne_Optional_SelfId]
FROM (
    SELECT * FROM [Level1]
) AS [l]
LEFT JOIN [Level2] AS [l.OneToOne_Optional_FK] ON [l].[Id] = [l.OneToOne_Optional_FK].[Level1_Optional_Id]
ORDER BY [l.OneToOne_Optional_FK].[Id], [l].[Id]",
                    //
                    @"SELECT [l.OneToMany_Optional].[Id], [l.OneToMany_Optional].[Date], [l.OneToMany_Optional].[Level1_Optional_Id], [l.OneToMany_Optional].[Level1_Required_Id], [l.OneToMany_Optional].[Name], [l.OneToMany_Optional].[OneToMany_Optional_InverseId], [l.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [l.OneToMany_Optional].[OneToMany_Required_InverseId], [l.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [l.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [l.OneToMany_Optional].[OneToOne_Optional_SelfId], [l.OneToOne_Optional_FK1].[Id], [l.OneToOne_Optional_FK1].[Level2_Optional_Id], [l.OneToOne_Optional_FK1].[Level2_Required_Id], [l.OneToOne_Optional_FK1].[Name], [l.OneToOne_Optional_FK1].[OneToMany_Optional_InverseId], [l.OneToOne_Optional_FK1].[OneToMany_Optional_Self_InverseId], [l.OneToOne_Optional_FK1].[OneToMany_Required_InverseId], [l.OneToOne_Optional_FK1].[OneToMany_Required_Self_InverseId], [l.OneToOne_Optional_FK1].[OneToOne_Optional_PK_InverseId], [l.OneToOne_Optional_FK1].[OneToOne_Optional_SelfId]
FROM [Level2] AS [l.OneToMany_Optional]
LEFT JOIN [Level3] AS [l.OneToOne_Optional_FK1] ON [l.OneToMany_Optional].[Id] = [l.OneToOne_Optional_FK1].[Level2_Optional_Id]
INNER JOIN (
    SELECT DISTINCT [l1].[Id], [l.OneToOne_Optional_FK2].[Id] AS [Id0]
    FROM (
        SELECT * FROM [Level1]
    ) AS [l1]
    LEFT JOIN [Level2] AS [l.OneToOne_Optional_FK2] ON [l1].[Id] = [l.OneToOne_Optional_FK2].[Level1_Optional_Id]
) AS [t0] ON [l.OneToMany_Optional].[OneToMany_Optional_InverseId] = [t0].[Id]
ORDER BY [t0].[Id0], [t0].[Id]",
                    //
                    @"SELECT [l.OneToOne_Optional_FK.OneToMany_Optional].[Id], [l.OneToOne_Optional_FK.OneToMany_Optional].[Level2_Optional_Id], [l.OneToOne_Optional_FK.OneToMany_Optional].[Level2_Required_Id], [l.OneToOne_Optional_FK.OneToMany_Optional].[Name], [l.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Optional_InverseId], [l.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [l.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Required_InverseId], [l.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [l.OneToOne_Optional_FK.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [l.OneToOne_Optional_FK.OneToMany_Optional].[OneToOne_Optional_SelfId]
FROM [Level3] AS [l.OneToOne_Optional_FK.OneToMany_Optional]
INNER JOIN (
    SELECT DISTINCT [l.OneToOne_Optional_FK0].[Id]
    FROM (
        SELECT * FROM [Level1]
    ) AS [l0]
    LEFT JOIN [Level2] AS [l.OneToOne_Optional_FK0] ON [l0].[Id] = [l.OneToOne_Optional_FK0].[Level1_Optional_Id]
) AS [t] ON [l.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Optional_InverseId] = [t].[Id]
ORDER BY [t].[Id]");
            }
        }

        public override void Where_navigation_property_to_collection()
        {
            base.Where_navigation_property_to_collection();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_InverseId], [l1].[OneToMany_Required_Self_InverseId], [l1].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l1.OneToOne_Required_FK] ON [l1].[Id] = [l1.OneToOne_Required_FK].[Level1_Required_Id]
WHERE (
    SELECT COUNT(*)
    FROM [Level3] AS [l]
    WHERE [l1.OneToOne_Required_FK].[Id] = [l].[OneToMany_Optional_InverseId]
) > 0");
        }

        public override void Where_navigation_property_to_collection2()
        {
            base.Where_navigation_property_to_collection2();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_InverseId], [l1].[OneToMany_Optional_Self_InverseId], [l1].[OneToMany_Required_InverseId], [l1].[OneToMany_Required_Self_InverseId], [l1].[OneToOne_Optional_PK_InverseId], [l1].[OneToOne_Optional_SelfId]
FROM [Level3] AS [l1]
INNER JOIN [Level2] AS [l1.OneToOne_Required_FK_Inverse] ON [l1].[Level2_Required_Id] = [l1.OneToOne_Required_FK_Inverse].[Id]
WHERE (
    SELECT COUNT(*)
    FROM [Level3] AS [l]
    WHERE [l1.OneToOne_Required_FK_Inverse].[Id] = [l].[OneToMany_Optional_InverseId]
) > 0");
        }

        public override void Where_navigation_property_to_collection_of_original_entity_type()
        {
            base.Where_navigation_property_to_collection_of_original_entity_type();

            AssertSql(
                @"SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_InverseId], [l2].[OneToMany_Optional_Self_InverseId], [l2].[OneToMany_Required_InverseId], [l2].[OneToMany_Required_Self_InverseId], [l2].[OneToOne_Optional_PK_InverseId], [l2].[OneToOne_Optional_SelfId]
FROM [Level2] AS [l2]
INNER JOIN [Level1] AS [l2.OneToMany_Required_Inverse] ON [l2].[OneToMany_Required_InverseId] = [l2.OneToMany_Required_Inverse].[Id]
WHERE (
    SELECT COUNT(*)
    FROM [Level2] AS [l]
    WHERE [l2.OneToMany_Required_Inverse].[Id] = [l].[OneToMany_Optional_InverseId]
) > 0");
        }

        public override void Complex_multi_include_with_order_by_and_paging()
        {
            base.Complex_multi_include_with_order_by_and_paging();

            if (SupportsOffset)
            {
                AssertSql(
                    @"@__p_0='0'
@__p_1='10'

SELECT [e].[Id], [e].[Date], [e].[Name], [e].[OneToMany_Optional_Self_InverseId], [e].[OneToMany_Required_Self_InverseId], [e].[OneToOne_Optional_SelfId], [e.OneToOne_Required_FK].[Id], [e.OneToOne_Required_FK].[Date], [e.OneToOne_Required_FK].[Level1_Optional_Id], [e.OneToOne_Required_FK].[Level1_Required_Id], [e.OneToOne_Required_FK].[Name], [e.OneToOne_Required_FK].[OneToMany_Optional_InverseId], [e.OneToOne_Required_FK].[OneToMany_Optional_Self_InverseId], [e.OneToOne_Required_FK].[OneToMany_Required_InverseId], [e.OneToOne_Required_FK].[OneToMany_Required_Self_InverseId], [e.OneToOne_Required_FK].[OneToOne_Optional_PK_InverseId], [e.OneToOne_Required_FK].[OneToOne_Optional_SelfId]
FROM [Level1] AS [e]
LEFT JOIN [Level2] AS [e.OneToOne_Required_FK] ON [e].[Id] = [e.OneToOne_Required_FK].[Level1_Required_Id]
ORDER BY [e].[Name], [e.OneToOne_Required_FK].[Id]
OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY",
                    //
                    @"@__p_0='0'
@__p_1='10'

SELECT [e.OneToOne_Required_FK.OneToMany_Optional].[Id], [e.OneToOne_Required_FK.OneToMany_Optional].[Level2_Optional_Id], [e.OneToOne_Required_FK.OneToMany_Optional].[Level2_Required_Id], [e.OneToOne_Required_FK.OneToMany_Optional].[Name], [e.OneToOne_Required_FK.OneToMany_Optional].[OneToMany_Optional_InverseId], [e.OneToOne_Required_FK.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [e.OneToOne_Required_FK.OneToMany_Optional].[OneToMany_Required_InverseId], [e.OneToOne_Required_FK.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [e.OneToOne_Required_FK.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [e.OneToOne_Required_FK.OneToMany_Optional].[OneToOne_Optional_SelfId]
FROM [Level3] AS [e.OneToOne_Required_FK.OneToMany_Optional]
INNER JOIN (
    SELECT DISTINCT [t].*
    FROM (
        SELECT [e.OneToOne_Required_FK0].[Id], [e0].[Name]
        FROM [Level1] AS [e0]
        LEFT JOIN [Level2] AS [e.OneToOne_Required_FK0] ON [e0].[Id] = [e.OneToOne_Required_FK0].[Level1_Required_Id]
        ORDER BY [e0].[Name], [e.OneToOne_Required_FK0].[Id]
        OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
    ) AS [t]
) AS [t0] ON [e.OneToOne_Required_FK.OneToMany_Optional].[OneToMany_Optional_InverseId] = [t0].[Id]
ORDER BY [t0].[Name], [t0].[Id]",
                    //
                    @"@__p_0='0'
@__p_1='10'

SELECT [e.OneToOne_Required_FK.OneToMany_Required].[Id], [e.OneToOne_Required_FK.OneToMany_Required].[Level2_Optional_Id], [e.OneToOne_Required_FK.OneToMany_Required].[Level2_Required_Id], [e.OneToOne_Required_FK.OneToMany_Required].[Name], [e.OneToOne_Required_FK.OneToMany_Required].[OneToMany_Optional_InverseId], [e.OneToOne_Required_FK.OneToMany_Required].[OneToMany_Optional_Self_InverseId], [e.OneToOne_Required_FK.OneToMany_Required].[OneToMany_Required_InverseId], [e.OneToOne_Required_FK.OneToMany_Required].[OneToMany_Required_Self_InverseId], [e.OneToOne_Required_FK.OneToMany_Required].[OneToOne_Optional_PK_InverseId], [e.OneToOne_Required_FK.OneToMany_Required].[OneToOne_Optional_SelfId]
FROM [Level3] AS [e.OneToOne_Required_FK.OneToMany_Required]
INNER JOIN (
    SELECT DISTINCT [t1].*
    FROM (
        SELECT [e.OneToOne_Required_FK1].[Id], [e1].[Name]
        FROM [Level1] AS [e1]
        LEFT JOIN [Level2] AS [e.OneToOne_Required_FK1] ON [e1].[Id] = [e.OneToOne_Required_FK1].[Level1_Required_Id]
        ORDER BY [e1].[Name], [e.OneToOne_Required_FK1].[Id]
        OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
    ) AS [t1]
) AS [t2] ON [e.OneToOne_Required_FK.OneToMany_Required].[OneToMany_Required_InverseId] = [t2].[Id]
ORDER BY [t2].[Name], [t2].[Id]");
            }
        }

        public override void Complex_multi_include_with_order_by_and_paging_joins_on_correct_key()
        {
            base.Complex_multi_include_with_order_by_and_paging_joins_on_correct_key();

            if (SupportsOffset)
            {
                AssertSql(
                    @"@__p_0='0'
@__p_1='10'

SELECT [e].[Id], [e].[Date], [e].[Name], [e].[OneToMany_Optional_Self_InverseId], [e].[OneToMany_Required_Self_InverseId], [e].[OneToOne_Optional_SelfId], [e.OneToOne_Required_FK].[Id], [e.OneToOne_Required_FK].[Date], [e.OneToOne_Required_FK].[Level1_Optional_Id], [e.OneToOne_Required_FK].[Level1_Required_Id], [e.OneToOne_Required_FK].[Name], [e.OneToOne_Required_FK].[OneToMany_Optional_InverseId], [e.OneToOne_Required_FK].[OneToMany_Optional_Self_InverseId], [e.OneToOne_Required_FK].[OneToMany_Required_InverseId], [e.OneToOne_Required_FK].[OneToMany_Required_Self_InverseId], [e.OneToOne_Required_FK].[OneToOne_Optional_PK_InverseId], [e.OneToOne_Required_FK].[OneToOne_Optional_SelfId], [e.OneToOne_Optional_FK].[Id], [e.OneToOne_Optional_FK].[Date], [e.OneToOne_Optional_FK].[Level1_Optional_Id], [e.OneToOne_Optional_FK].[Level1_Required_Id], [e.OneToOne_Optional_FK].[Name], [e.OneToOne_Optional_FK].[OneToMany_Optional_InverseId], [e.OneToOne_Optional_FK].[OneToMany_Optional_Self_InverseId], [e.OneToOne_Optional_FK].[OneToMany_Required_InverseId], [e.OneToOne_Optional_FK].[OneToMany_Required_Self_InverseId], [e.OneToOne_Optional_FK].[OneToOne_Optional_PK_InverseId], [e.OneToOne_Optional_FK].[OneToOne_Optional_SelfId]
FROM [Level1] AS [e]
LEFT JOIN [Level2] AS [e.OneToOne_Required_FK] ON [e].[Id] = [e.OneToOne_Required_FK].[Level1_Required_Id]
LEFT JOIN [Level2] AS [e.OneToOne_Optional_FK] ON [e].[Id] = [e.OneToOne_Optional_FK].[Level1_Optional_Id]
ORDER BY [e].[Name], [e.OneToOne_Optional_FK].[Id], [e.OneToOne_Required_FK].[Id]
OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY",
                    //
                    @"@__p_0='0'
@__p_1='10'

SELECT [e.OneToOne_Required_FK.OneToMany_Required].[Id], [e.OneToOne_Required_FK.OneToMany_Required].[Level2_Optional_Id], [e.OneToOne_Required_FK.OneToMany_Required].[Level2_Required_Id], [e.OneToOne_Required_FK.OneToMany_Required].[Name], [e.OneToOne_Required_FK.OneToMany_Required].[OneToMany_Optional_InverseId], [e.OneToOne_Required_FK.OneToMany_Required].[OneToMany_Optional_Self_InverseId], [e.OneToOne_Required_FK.OneToMany_Required].[OneToMany_Required_InverseId], [e.OneToOne_Required_FK.OneToMany_Required].[OneToMany_Required_Self_InverseId], [e.OneToOne_Required_FK.OneToMany_Required].[OneToOne_Optional_PK_InverseId], [e.OneToOne_Required_FK.OneToMany_Required].[OneToOne_Optional_SelfId]
FROM [Level3] AS [e.OneToOne_Required_FK.OneToMany_Required]
INNER JOIN (
    SELECT DISTINCT [t1].*
    FROM (
        SELECT [e.OneToOne_Required_FK1].[Id], [e1].[Name], [e.OneToOne_Optional_FK1].[Id] AS [Id0]
        FROM [Level1] AS [e1]
        LEFT JOIN [Level2] AS [e.OneToOne_Required_FK1] ON [e1].[Id] = [e.OneToOne_Required_FK1].[Level1_Required_Id]
        LEFT JOIN [Level2] AS [e.OneToOne_Optional_FK1] ON [e1].[Id] = [e.OneToOne_Optional_FK1].[Level1_Optional_Id]
        ORDER BY [e1].[Name], [e.OneToOne_Optional_FK1].[Id], [e.OneToOne_Required_FK1].[Id]
        OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
    ) AS [t1]
) AS [t2] ON [e.OneToOne_Required_FK.OneToMany_Required].[OneToMany_Required_InverseId] = [t2].[Id]
ORDER BY [t2].[Name], [t2].[Id0], [t2].[Id]",
                    //
                    @"@__p_0='0'
@__p_1='10'

SELECT [e.OneToOne_Optional_FK.OneToMany_Optional].[Id], [e.OneToOne_Optional_FK.OneToMany_Optional].[Level2_Optional_Id], [e.OneToOne_Optional_FK.OneToMany_Optional].[Level2_Required_Id], [e.OneToOne_Optional_FK.OneToMany_Optional].[Name], [e.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Optional_InverseId], [e.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [e.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Required_InverseId], [e.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [e.OneToOne_Optional_FK.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [e.OneToOne_Optional_FK.OneToMany_Optional].[OneToOne_Optional_SelfId]
FROM [Level3] AS [e.OneToOne_Optional_FK.OneToMany_Optional]
INNER JOIN (
    SELECT DISTINCT [t].*
    FROM (
        SELECT [e.OneToOne_Optional_FK0].[Id], [e0].[Name]
        FROM [Level1] AS [e0]
        LEFT JOIN [Level2] AS [e.OneToOne_Required_FK0] ON [e0].[Id] = [e.OneToOne_Required_FK0].[Level1_Required_Id]
        LEFT JOIN [Level2] AS [e.OneToOne_Optional_FK0] ON [e0].[Id] = [e.OneToOne_Optional_FK0].[Level1_Optional_Id]
        ORDER BY [e0].[Name], [e.OneToOne_Optional_FK0].[Id]
        OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
    ) AS [t]
) AS [t0] ON [e.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Optional_InverseId] = [t0].[Id]
ORDER BY [t0].[Name], [t0].[Id]");
            }
        }

        public override void Complex_multi_include_with_order_by_and_paging_joins_on_correct_key2()
        {
            base.Complex_multi_include_with_order_by_and_paging_joins_on_correct_key2();

            if (SupportsOffset)
            {
                AssertSql(
                    @"@__p_0='0'
@__p_1='10'

SELECT [e].[Id], [e].[Date], [e].[Name], [e].[OneToMany_Optional_Self_InverseId], [e].[OneToMany_Required_Self_InverseId], [e].[OneToOne_Optional_SelfId], [e.OneToOne_Optional_FK].[Id], [e.OneToOne_Optional_FK].[Date], [e.OneToOne_Optional_FK].[Level1_Optional_Id], [e.OneToOne_Optional_FK].[Level1_Required_Id], [e.OneToOne_Optional_FK].[Name], [e.OneToOne_Optional_FK].[OneToMany_Optional_InverseId], [e.OneToOne_Optional_FK].[OneToMany_Optional_Self_InverseId], [e.OneToOne_Optional_FK].[OneToMany_Required_InverseId], [e.OneToOne_Optional_FK].[OneToMany_Required_Self_InverseId], [e.OneToOne_Optional_FK].[OneToOne_Optional_PK_InverseId], [e.OneToOne_Optional_FK].[OneToOne_Optional_SelfId], [e.OneToOne_Optional_FK.OneToOne_Required_FK].[Id], [e.OneToOne_Optional_FK.OneToOne_Required_FK].[Level2_Optional_Id], [e.OneToOne_Optional_FK.OneToOne_Required_FK].[Level2_Required_Id], [e.OneToOne_Optional_FK.OneToOne_Required_FK].[Name], [e.OneToOne_Optional_FK.OneToOne_Required_FK].[OneToMany_Optional_InverseId], [e.OneToOne_Optional_FK.OneToOne_Required_FK].[OneToMany_Optional_Self_InverseId], [e.OneToOne_Optional_FK.OneToOne_Required_FK].[OneToMany_Required_InverseId], [e.OneToOne_Optional_FK.OneToOne_Required_FK].[OneToMany_Required_Self_InverseId], [e.OneToOne_Optional_FK.OneToOne_Required_FK].[OneToOne_Optional_PK_InverseId], [e.OneToOne_Optional_FK.OneToOne_Required_FK].[OneToOne_Optional_SelfId]
FROM [Level1] AS [e]
LEFT JOIN [Level2] AS [e.OneToOne_Optional_FK] ON [e].[Id] = [e.OneToOne_Optional_FK].[Level1_Optional_Id]
LEFT JOIN [Level3] AS [e.OneToOne_Optional_FK.OneToOne_Required_FK] ON [e.OneToOne_Optional_FK].[Id] = [e.OneToOne_Optional_FK.OneToOne_Required_FK].[Level2_Required_Id]
ORDER BY [e].[Name], [e.OneToOne_Optional_FK.OneToOne_Required_FK].[Id]
OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY",
                    //
                    @"@__p_0='0'
@__p_1='10'

SELECT [e.OneToOne_Optional_FK.OneToOne_Required_FK.OneToMany_Optional].[Id], [e.OneToOne_Optional_FK.OneToOne_Required_FK.OneToMany_Optional].[Level3_Optional_Id], [e.OneToOne_Optional_FK.OneToOne_Required_FK.OneToMany_Optional].[Level3_Required_Id], [e.OneToOne_Optional_FK.OneToOne_Required_FK.OneToMany_Optional].[Name], [e.OneToOne_Optional_FK.OneToOne_Required_FK.OneToMany_Optional].[OneToMany_Optional_InverseId], [e.OneToOne_Optional_FK.OneToOne_Required_FK.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [e.OneToOne_Optional_FK.OneToOne_Required_FK.OneToMany_Optional].[OneToMany_Required_InverseId], [e.OneToOne_Optional_FK.OneToOne_Required_FK.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [e.OneToOne_Optional_FK.OneToOne_Required_FK.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [e.OneToOne_Optional_FK.OneToOne_Required_FK.OneToMany_Optional].[OneToOne_Optional_SelfId]
FROM [Level4] AS [e.OneToOne_Optional_FK.OneToOne_Required_FK.OneToMany_Optional]
INNER JOIN (
    SELECT DISTINCT [t].*
    FROM (
        SELECT [e.OneToOne_Optional_FK.OneToOne_Required_FK0].[Id], [e0].[Name]
        FROM [Level1] AS [e0]
        LEFT JOIN [Level2] AS [e.OneToOne_Optional_FK0] ON [e0].[Id] = [e.OneToOne_Optional_FK0].[Level1_Optional_Id]
        LEFT JOIN [Level3] AS [e.OneToOne_Optional_FK.OneToOne_Required_FK0] ON [e.OneToOne_Optional_FK0].[Id] = [e.OneToOne_Optional_FK.OneToOne_Required_FK0].[Level2_Required_Id]
        ORDER BY [e0].[Name], [e.OneToOne_Optional_FK.OneToOne_Required_FK0].[Id]
        OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
    ) AS [t]
) AS [t0] ON [e.OneToOne_Optional_FK.OneToOne_Required_FK.OneToMany_Optional].[OneToMany_Optional_InverseId] = [t0].[Id]
ORDER BY [t0].[Name], [t0].[Id]");
            }
        }

        public override void Multiple_include_with_multiple_optional_navigations()
        {
            base.Multiple_include_with_multiple_optional_navigations();

            AssertSql(
                @"SELECT [e].[Id], [e].[Date], [e].[Name], [e].[OneToMany_Optional_Self_InverseId], [e].[OneToMany_Required_Self_InverseId], [e].[OneToOne_Optional_SelfId], [e.OneToOne_Optional_FK].[Id], [e.OneToOne_Optional_FK].[Date], [e.OneToOne_Optional_FK].[Level1_Optional_Id], [e.OneToOne_Optional_FK].[Level1_Required_Id], [e.OneToOne_Optional_FK].[Name], [e.OneToOne_Optional_FK].[OneToMany_Optional_InverseId], [e.OneToOne_Optional_FK].[OneToMany_Optional_Self_InverseId], [e.OneToOne_Optional_FK].[OneToMany_Required_InverseId], [e.OneToOne_Optional_FK].[OneToMany_Required_Self_InverseId], [e.OneToOne_Optional_FK].[OneToOne_Optional_PK_InverseId], [e.OneToOne_Optional_FK].[OneToOne_Optional_SelfId], [e.OneToOne_Optional_FK.OneToOne_Optional_FK].[Id], [e.OneToOne_Optional_FK.OneToOne_Optional_FK].[Level2_Optional_Id], [e.OneToOne_Optional_FK.OneToOne_Optional_FK].[Level2_Required_Id], [e.OneToOne_Optional_FK.OneToOne_Optional_FK].[Name], [e.OneToOne_Optional_FK.OneToOne_Optional_FK].[OneToMany_Optional_InverseId], [e.OneToOne_Optional_FK.OneToOne_Optional_FK].[OneToMany_Optional_Self_InverseId], [e.OneToOne_Optional_FK.OneToOne_Optional_FK].[OneToMany_Required_InverseId], [e.OneToOne_Optional_FK.OneToOne_Optional_FK].[OneToMany_Required_Self_InverseId], [e.OneToOne_Optional_FK.OneToOne_Optional_FK].[OneToOne_Optional_PK_InverseId], [e.OneToOne_Optional_FK.OneToOne_Optional_FK].[OneToOne_Optional_SelfId], [e.OneToOne_Required_FK].[Id], [e.OneToOne_Required_FK].[Date], [e.OneToOne_Required_FK].[Level1_Optional_Id], [e.OneToOne_Required_FK].[Level1_Required_Id], [e.OneToOne_Required_FK].[Name], [e.OneToOne_Required_FK].[OneToMany_Optional_InverseId], [e.OneToOne_Required_FK].[OneToMany_Optional_Self_InverseId], [e.OneToOne_Required_FK].[OneToMany_Required_InverseId], [e.OneToOne_Required_FK].[OneToMany_Required_Self_InverseId], [e.OneToOne_Required_FK].[OneToOne_Optional_PK_InverseId], [e.OneToOne_Required_FK].[OneToOne_Optional_SelfId], [e.OneToOne_Required_FK.OneToOne_Optional_FK].[Id], [e.OneToOne_Required_FK.OneToOne_Optional_FK].[Level2_Optional_Id], [e.OneToOne_Required_FK.OneToOne_Optional_FK].[Level2_Required_Id], [e.OneToOne_Required_FK.OneToOne_Optional_FK].[Name], [e.OneToOne_Required_FK.OneToOne_Optional_FK].[OneToMany_Optional_InverseId], [e.OneToOne_Required_FK.OneToOne_Optional_FK].[OneToMany_Optional_Self_InverseId], [e.OneToOne_Required_FK.OneToOne_Optional_FK].[OneToMany_Required_InverseId], [e.OneToOne_Required_FK.OneToOne_Optional_FK].[OneToMany_Required_Self_InverseId], [e.OneToOne_Required_FK.OneToOne_Optional_FK].[OneToOne_Optional_PK_InverseId], [e.OneToOne_Required_FK.OneToOne_Optional_FK].[OneToOne_Optional_SelfId]
FROM [Level1] AS [e]
LEFT JOIN [Level2] AS [e.OneToOne_Optional_FK] ON [e].[Id] = [e.OneToOne_Optional_FK].[Level1_Optional_Id]
LEFT JOIN [Level3] AS [e.OneToOne_Optional_FK.OneToOne_Optional_FK] ON [e.OneToOne_Optional_FK].[Id] = [e.OneToOne_Optional_FK.OneToOne_Optional_FK].[Level2_Optional_Id]
LEFT JOIN [Level2] AS [e.OneToOne_Required_FK] ON [e].[Id] = [e.OneToOne_Required_FK].[Level1_Required_Id]
LEFT JOIN [Level3] AS [e.OneToOne_Required_FK.OneToOne_Optional_PK] ON [e.OneToOne_Required_FK].[Id] = [e.OneToOne_Required_FK.OneToOne_Optional_PK].[OneToOne_Optional_PK_InverseId]
LEFT JOIN [Level3] AS [e.OneToOne_Required_FK.OneToOne_Optional_FK] ON [e.OneToOne_Required_FK].[Id] = [e.OneToOne_Required_FK.OneToOne_Optional_FK].[Level2_Optional_Id]
WHERE ([e.OneToOne_Required_FK.OneToOne_Optional_PK].[Name] <> N'Foo') OR [e.OneToOne_Required_FK.OneToOne_Optional_PK].[Name] IS NULL
ORDER BY [e].[Id], [e.OneToOne_Required_FK].[Id]",
                //
                @"SELECT [e.OneToOne_Required_FK.OneToMany_Optional].[Id], [e.OneToOne_Required_FK.OneToMany_Optional].[Level2_Optional_Id], [e.OneToOne_Required_FK.OneToMany_Optional].[Level2_Required_Id], [e.OneToOne_Required_FK.OneToMany_Optional].[Name], [e.OneToOne_Required_FK.OneToMany_Optional].[OneToMany_Optional_InverseId], [e.OneToOne_Required_FK.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [e.OneToOne_Required_FK.OneToMany_Optional].[OneToMany_Required_InverseId], [e.OneToOne_Required_FK.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [e.OneToOne_Required_FK.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [e.OneToOne_Required_FK.OneToMany_Optional].[OneToOne_Optional_SelfId]
FROM [Level3] AS [e.OneToOne_Required_FK.OneToMany_Optional]
INNER JOIN (
    SELECT DISTINCT [e.OneToOne_Required_FK0].[Id], [e0].[Id] AS [Id0]
    FROM [Level1] AS [e0]
    LEFT JOIN [Level2] AS [e.OneToOne_Optional_FK0] ON [e0].[Id] = [e.OneToOne_Optional_FK0].[Level1_Optional_Id]
    LEFT JOIN [Level3] AS [e.OneToOne_Optional_FK.OneToOne_Optional_FK0] ON [e.OneToOne_Optional_FK0].[Id] = [e.OneToOne_Optional_FK.OneToOne_Optional_FK0].[Level2_Optional_Id]
    LEFT JOIN [Level2] AS [e.OneToOne_Required_FK0] ON [e0].[Id] = [e.OneToOne_Required_FK0].[Level1_Required_Id]
    LEFT JOIN [Level3] AS [e.OneToOne_Required_FK.OneToOne_Optional_PK0] ON [e.OneToOne_Required_FK0].[Id] = [e.OneToOne_Required_FK.OneToOne_Optional_PK0].[OneToOne_Optional_PK_InverseId]
    LEFT JOIN [Level3] AS [e.OneToOne_Required_FK.OneToOne_Optional_FK0] ON [e.OneToOne_Required_FK0].[Id] = [e.OneToOne_Required_FK.OneToOne_Optional_FK0].[Level2_Optional_Id]
    WHERE ([e.OneToOne_Required_FK.OneToOne_Optional_PK0].[Name] <> N'Foo') OR [e.OneToOne_Required_FK.OneToOne_Optional_PK0].[Name] IS NULL
) AS [t] ON [e.OneToOne_Required_FK.OneToMany_Optional].[OneToMany_Optional_InverseId] = [t].[Id]
ORDER BY [t].[Id0], [t].[Id]");
        }

        public override void Correlated_subquery_doesnt_project_unnecessary_columns_in_top_level()
        {
            base.Correlated_subquery_doesnt_project_unnecessary_columns_in_top_level();

            AssertSql(
                @"SELECT DISTINCT [l1].[Name]
FROM [Level1] AS [l1]
WHERE EXISTS (
    SELECT 1
    FROM [Level2] AS [l2]
    WHERE [l2].[Level1_Required_Id] = [l1].[Id])");
        }

        public override void Correlated_subquery_doesnt_project_unnecessary_columns_in_top_level_join()
        {
            base.Correlated_subquery_doesnt_project_unnecessary_columns_in_top_level_join();

            AssertSql(
                @"SELECT [e1].[Name] AS [Name1], [e2].[Id] AS [Id2]
FROM [Level1] AS [e1]
INNER JOIN [Level2] AS [e2] ON [e1].[Id] = [e2].[Level1_Optional_Id]
WHERE EXISTS (
    SELECT 1
    FROM [Level2] AS [l2]
    WHERE [l2].[Level1_Required_Id] = [e1].[Id])");
        }

        public override void Correlated_nested_subquery_doesnt_project_unnecessary_columns_in_top_level()
        {
            base.Correlated_nested_subquery_doesnt_project_unnecessary_columns_in_top_level();

            AssertSql(
                @"SELECT DISTINCT [l1].[Name]
FROM [Level1] AS [l1]
WHERE EXISTS (
    SELECT 1
    FROM [Level2] AS [l2]
    WHERE EXISTS (
        SELECT 1
        FROM [Level3] AS [l3]))");
        }

        public override void Correlated_nested_two_levels_up_subquery_doesnt_project_unnecessary_columns_in_top_level()
        {
            base.Correlated_nested_two_levels_up_subquery_doesnt_project_unnecessary_columns_in_top_level();

            AssertSql(
                @"SELECT DISTINCT [l1].[Name]
FROM [Level1] AS [l1]
WHERE EXISTS (
    SELECT 1
    FROM [Level2] AS [l2]
    WHERE EXISTS (
        SELECT 1
        FROM [Level3] AS [l3]))");
        }

        public override void GroupJoin_on_subquery_and_set_operation_on_grouping_but_nothing_from_grouping_is_projected()
        {
            base.GroupJoin_on_subquery_and_set_operation_on_grouping_but_nothing_from_grouping_is_projected();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_InverseId], [l1].[OneToMany_Required_Self_InverseId], [l1].[OneToOne_Optional_SelfId], [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_InverseId], [t].[OneToMany_Optional_Self_InverseId], [t].[OneToMany_Required_InverseId], [t].[OneToMany_Required_Self_InverseId], [t].[OneToOne_Optional_PK_InverseId], [t].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_InverseId], [l2].[OneToMany_Optional_Self_InverseId], [l2].[OneToMany_Required_InverseId], [l2].[OneToMany_Required_Self_InverseId], [l2].[OneToOne_Optional_PK_InverseId], [l2].[OneToOne_Optional_SelfId]
    FROM [Level2] AS [l2]
    WHERE ([l2].[Name] <> N'L2 01') OR [l2].[Name] IS NULL
) AS [t] ON [l1].[Id] = [t].[Level1_Optional_Id]
ORDER BY [l1].[Id]");
        }

        public override void GroupJoin_on_complex_subquery_and_set_operation_on_grouping_but_nothing_from_grouping_is_projected()
        {
            base.GroupJoin_on_complex_subquery_and_set_operation_on_grouping_but_nothing_from_grouping_is_projected();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_InverseId], [l1].[OneToMany_Required_Self_InverseId], [l1].[OneToOne_Optional_SelfId], [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_InverseId], [t].[OneToMany_Optional_Self_InverseId], [t].[OneToMany_Required_InverseId], [t].[OneToMany_Required_Self_InverseId], [t].[OneToOne_Optional_PK_InverseId], [t].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
LEFT JOIN (
    SELECT [l1.OneToOne_Required_FK].[Id], [l1.OneToOne_Required_FK].[Date], [l1.OneToOne_Required_FK].[Level1_Optional_Id], [l1.OneToOne_Required_FK].[Level1_Required_Id], [l1.OneToOne_Required_FK].[Name], [l1.OneToOne_Required_FK].[OneToMany_Optional_InverseId], [l1.OneToOne_Required_FK].[OneToMany_Optional_Self_InverseId], [l1.OneToOne_Required_FK].[OneToMany_Required_InverseId], [l1.OneToOne_Required_FK].[OneToMany_Required_Self_InverseId], [l1.OneToOne_Required_FK].[OneToOne_Optional_PK_InverseId], [l1.OneToOne_Required_FK].[OneToOne_Optional_SelfId]
    FROM [Level1] AS [l10]
    LEFT JOIN [Level2] AS [l1.OneToOne_Required_FK] ON [l10].[Id] = [l1.OneToOne_Required_FK].[Level1_Required_Id]
    WHERE ([l10].[Name] <> N'L1 01') OR [l10].[Name] IS NULL
) AS [t] ON [l1].[Id] = [t].[Level1_Optional_Id]
ORDER BY [l1].[Id]");
        }

        public override void Null_protection_logic_work_for_inner_key_access_of_manually_created_GroupJoin1()
        {
            base.Null_protection_logic_work_for_inner_key_access_of_manually_created_GroupJoin1();

            AssertContainsSql(
                @"SELECT [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_InverseId], [t].[OneToMany_Optional_Self_InverseId], [t].[OneToMany_Required_InverseId], [t].[OneToMany_Required_Self_InverseId], [t].[OneToOne_Optional_PK_InverseId], [t].[OneToOne_Optional_SelfId]
FROM (
    SELECT [l1.OneToOne_Required_FK].[Id], [l1.OneToOne_Required_FK].[Date], [l1.OneToOne_Required_FK].[Level1_Optional_Id], [l1.OneToOne_Required_FK].[Level1_Required_Id], [l1.OneToOne_Required_FK].[Name], [l1.OneToOne_Required_FK].[OneToMany_Optional_InverseId], [l1.OneToOne_Required_FK].[OneToMany_Optional_Self_InverseId], [l1.OneToOne_Required_FK].[OneToMany_Required_InverseId], [l1.OneToOne_Required_FK].[OneToMany_Required_Self_InverseId], [l1.OneToOne_Required_FK].[OneToOne_Optional_PK_InverseId], [l1.OneToOne_Required_FK].[OneToOne_Optional_SelfId]
    FROM [Level1] AS [l10]
    LEFT JOIN [Level2] AS [l1.OneToOne_Required_FK] ON [l10].[Id] = [l1.OneToOne_Required_FK].[Level1_Required_Id]
) AS [t]",
                //
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_InverseId], [l1].[OneToMany_Required_Self_InverseId], [l1].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]");
        }

        public override void Null_protection_logic_work_for_inner_key_access_of_manually_created_GroupJoin2()
        {
            base.Null_protection_logic_work_for_inner_key_access_of_manually_created_GroupJoin2();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_InverseId], [l1].[OneToMany_Required_Self_InverseId], [l1].[OneToOne_Optional_SelfId], [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_InverseId], [t].[OneToMany_Optional_Self_InverseId], [t].[OneToMany_Required_InverseId], [t].[OneToMany_Required_Self_InverseId], [t].[OneToOne_Optional_PK_InverseId], [t].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
LEFT JOIN (
    SELECT [l1.OneToOne_Required_FK].[Id], [l1.OneToOne_Required_FK].[Date], [l1.OneToOne_Required_FK].[Level1_Optional_Id], [l1.OneToOne_Required_FK].[Level1_Required_Id], [l1.OneToOne_Required_FK].[Name], [l1.OneToOne_Required_FK].[OneToMany_Optional_InverseId], [l1.OneToOne_Required_FK].[OneToMany_Optional_Self_InverseId], [l1.OneToOne_Required_FK].[OneToMany_Required_InverseId], [l1.OneToOne_Required_FK].[OneToMany_Required_Self_InverseId], [l1.OneToOne_Required_FK].[OneToOne_Optional_PK_InverseId], [l1.OneToOne_Required_FK].[OneToOne_Optional_SelfId]
    FROM [Level1] AS [l10]
    LEFT JOIN [Level2] AS [l1.OneToOne_Required_FK] ON [l10].[Id] = [l1.OneToOne_Required_FK].[Level1_Required_Id]
) AS [t] ON [l1].[Id] = [t].[Level1_Optional_Id]
ORDER BY [l1].[Id]");
        }

        public override void Null_protection_logic_work_for_outer_key_access_of_manually_created_GroupJoin()
        {
            base.Null_protection_logic_work_for_outer_key_access_of_manually_created_GroupJoin();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_InverseId], [l1].[OneToMany_Required_Self_InverseId], [l1].[OneToOne_Optional_SelfId], [l1.OneToOne_Required_FK].[Id], [l1.OneToOne_Required_FK].[Date], [l1.OneToOne_Required_FK].[Level1_Optional_Id], [l1.OneToOne_Required_FK].[Level1_Required_Id], [l1.OneToOne_Required_FK].[Name], [l1.OneToOne_Required_FK].[OneToMany_Optional_InverseId], [l1.OneToOne_Required_FK].[OneToMany_Optional_Self_InverseId], [l1.OneToOne_Required_FK].[OneToMany_Required_InverseId], [l1.OneToOne_Required_FK].[OneToMany_Required_Self_InverseId], [l1.OneToOne_Required_FK].[OneToOne_Optional_PK_InverseId], [l1.OneToOne_Required_FK].[OneToOne_Optional_SelfId], [l10].[Id], [l10].[Date], [l10].[Name], [l10].[OneToMany_Optional_Self_InverseId], [l10].[OneToMany_Required_Self_InverseId], [l10].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l1.OneToOne_Required_FK] ON [l1].[Id] = [l1.OneToOne_Required_FK].[Level1_Required_Id]
LEFT JOIN [Level1] AS [l10] ON [l1.OneToOne_Required_FK].[Level1_Optional_Id] = [l10].[Id]");
        }

        public override void SelectMany_where_with_subquery()
        {
            base.SelectMany_where_with_subquery();

            AssertSql(
                @"SELECT [l1.OneToMany_Required].[Id], [l1.OneToMany_Required].[Date], [l1.OneToMany_Required].[Level1_Optional_Id], [l1.OneToMany_Required].[Level1_Required_Id], [l1.OneToMany_Required].[Name], [l1.OneToMany_Required].[OneToMany_Optional_InverseId], [l1.OneToMany_Required].[OneToMany_Optional_Self_InverseId], [l1.OneToMany_Required].[OneToMany_Required_InverseId], [l1.OneToMany_Required].[OneToMany_Required_Self_InverseId], [l1.OneToMany_Required].[OneToOne_Optional_PK_InverseId], [l1.OneToMany_Required].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
INNER JOIN [Level2] AS [l1.OneToMany_Required] ON [l1].[Id] = [l1.OneToMany_Required].[OneToMany_Required_InverseId]
WHERE EXISTS (
    SELECT 1
    FROM [Level3] AS [l]
    WHERE [l1.OneToMany_Required].[Id] = [l].[OneToMany_Required_InverseId])");
        }

        public override void Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access1()
        {
            base.Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access1();

            AssertSql(
                @"SELECT [l3.OneToOne_Required_FK_Inverse].[Id], [l3.OneToOne_Required_FK_Inverse].[Date], [l3.OneToOne_Required_FK_Inverse].[Level1_Optional_Id], [l3.OneToOne_Required_FK_Inverse].[Level1_Required_Id], [l3.OneToOne_Required_FK_Inverse].[Name], [l3.OneToOne_Required_FK_Inverse].[OneToMany_Optional_InverseId], [l3.OneToOne_Required_FK_Inverse].[OneToMany_Optional_Self_InverseId], [l3.OneToOne_Required_FK_Inverse].[OneToMany_Required_InverseId], [l3.OneToOne_Required_FK_Inverse].[OneToMany_Required_Self_InverseId], [l3.OneToOne_Required_FK_Inverse].[OneToOne_Optional_PK_InverseId], [l3.OneToOne_Required_FK_Inverse].[OneToOne_Optional_SelfId]
FROM [Level3] AS [l3]
INNER JOIN [Level2] AS [l3.OneToOne_Required_FK_Inverse] ON [l3].[Level2_Required_Id] = [l3.OneToOne_Required_FK_Inverse].[Id]
ORDER BY [l3.OneToOne_Required_FK_Inverse].[Id]");
        }

        public override void Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access2()
        {
            base.Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access2();

            AssertSql(
                @"SELECT [l3.OneToOne_Required_FK_Inverse].[Id], [l3.OneToOne_Required_FK_Inverse].[Date], [l3.OneToOne_Required_FK_Inverse].[Level1_Optional_Id], [l3.OneToOne_Required_FK_Inverse].[Level1_Required_Id], [l3.OneToOne_Required_FK_Inverse].[Name], [l3.OneToOne_Required_FK_Inverse].[OneToMany_Optional_InverseId], [l3.OneToOne_Required_FK_Inverse].[OneToMany_Optional_Self_InverseId], [l3.OneToOne_Required_FK_Inverse].[OneToMany_Required_InverseId], [l3.OneToOne_Required_FK_Inverse].[OneToMany_Required_Self_InverseId], [l3.OneToOne_Required_FK_Inverse].[OneToOne_Optional_PK_InverseId], [l3.OneToOne_Required_FK_Inverse].[OneToOne_Optional_SelfId]
FROM [Level3] AS [l3]
INNER JOIN [Level2] AS [l3.OneToOne_Required_FK_Inverse] ON [l3].[Level2_Required_Id] = [l3.OneToOne_Required_FK_Inverse].[Id]
ORDER BY [l3.OneToOne_Required_FK_Inverse].[Id]");
        }

        public override void Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access3()
        {
            base.Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access3();

            AssertSql(
                @"SELECT [l3.OneToOne_Required_FK_Inverse].[Id], [l3.OneToOne_Required_FK_Inverse].[Date], [l3.OneToOne_Required_FK_Inverse].[Level1_Optional_Id], [l3.OneToOne_Required_FK_Inverse].[Level1_Required_Id], [l3.OneToOne_Required_FK_Inverse].[Name], [l3.OneToOne_Required_FK_Inverse].[OneToMany_Optional_InverseId], [l3.OneToOne_Required_FK_Inverse].[OneToMany_Optional_Self_InverseId], [l3.OneToOne_Required_FK_Inverse].[OneToMany_Required_InverseId], [l3.OneToOne_Required_FK_Inverse].[OneToMany_Required_Self_InverseId], [l3.OneToOne_Required_FK_Inverse].[OneToOne_Optional_PK_InverseId], [l3.OneToOne_Required_FK_Inverse].[OneToOne_Optional_SelfId]
FROM [Level3] AS [l3]
INNER JOIN [Level2] AS [l3.OneToOne_Required_FK_Inverse] ON [l3].[Level2_Required_Id] = [l3.OneToOne_Required_FK_Inverse].[Id]
ORDER BY [l3.OneToOne_Required_FK_Inverse].[Id]");
        }

        public override void Order_by_key_of_navigation_similar_to_projected_gets_optimized_into_FK_access()
        {
            base.Order_by_key_of_navigation_similar_to_projected_gets_optimized_into_FK_access();

            AssertSql(
                @"SELECT [l3.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Id], [l3.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Date], [l3.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Name], [l3.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[OneToMany_Optional_Self_InverseId], [l3.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[OneToMany_Required_Self_InverseId], [l3.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[OneToOne_Optional_SelfId]
FROM [Level3] AS [l3]
INNER JOIN [Level2] AS [l3.OneToOne_Required_FK_Inverse] ON [l3].[Level2_Required_Id] = [l3.OneToOne_Required_FK_Inverse].[Id]
INNER JOIN [Level1] AS [l3.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse] ON [l3.OneToOne_Required_FK_Inverse].[Level1_Required_Id] = [l3.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Id]
ORDER BY [l3].[Level2_Required_Id]");
        }

        public override void Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access_subquery()
        {
            base.Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access_subquery();

            AssertSql(
                @"@__p_0='10'

SELECT TOP(@__p_0) [l3.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Name]
FROM [Level3] AS [l3]
INNER JOIN [Level2] AS [l3.OneToOne_Required_FK_Inverse] ON [l3].[Level2_Required_Id] = [l3.OneToOne_Required_FK_Inverse].[Id]
INNER JOIN [Level1] AS [l3.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse] ON [l3.OneToOne_Required_FK_Inverse].[Level1_Required_Id] = [l3.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Id]
ORDER BY [l3].[Level2_Required_Id]");
        }

        public override void Order_by_key_of_anonymous_type_projected_navigation_doesnt_get_optimized_into_FK_access_subquery()
        {
            base.Order_by_key_of_anonymous_type_projected_navigation_doesnt_get_optimized_into_FK_access_subquery();

            AssertSql(
                @"@__p_0='10'

SELECT TOP(@__p_0) [l3.OneToOne_Required_FK_Inverse].[Id], [l3.OneToOne_Required_FK_Inverse].[Date], [l3.OneToOne_Required_FK_Inverse].[Level1_Optional_Id], [l3.OneToOne_Required_FK_Inverse].[Level1_Required_Id], [l3.OneToOne_Required_FK_Inverse].[Name], [l3.OneToOne_Required_FK_Inverse].[OneToMany_Optional_InverseId], [l3.OneToOne_Required_FK_Inverse].[OneToMany_Optional_Self_InverseId], [l3.OneToOne_Required_FK_Inverse].[OneToMany_Required_InverseId], [l3.OneToOne_Required_FK_Inverse].[OneToMany_Required_Self_InverseId], [l3.OneToOne_Required_FK_Inverse].[OneToOne_Optional_PK_InverseId], [l3.OneToOne_Required_FK_Inverse].[OneToOne_Optional_SelfId], [l3].[Name] AS [name0]
FROM [Level3] AS [l3]
INNER JOIN [Level2] AS [l3.OneToOne_Required_FK_Inverse] ON [l3].[Level2_Required_Id] = [l3.OneToOne_Required_FK_Inverse].[Id]
ORDER BY [l3].[Level2_Required_Id]");
        }

        public override void Optional_navigation_take_optional_navigation()
        {
            base.Optional_navigation_take_optional_navigation();

            AssertSql(
                @"@__p_0='10'

SELECT TOP(@__p_0) [l1.OneToOne_Optional_FK.OneToOne_Optional_FK].[Name]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l1.OneToOne_Optional_FK] ON [l1].[Id] = [l1.OneToOne_Optional_FK].[Level1_Optional_Id]
LEFT JOIN [Level3] AS [l1.OneToOne_Optional_FK.OneToOne_Optional_FK] ON [l1.OneToOne_Optional_FK].[Id] = [l1.OneToOne_Optional_FK.OneToOne_Optional_FK].[Level2_Optional_Id]
ORDER BY [l1.OneToOne_Optional_FK].[Id]");
        }

        public override void Projection_select_correct_table_from_subquery_when_materialization_is_not_required()
        {
            base.Projection_select_correct_table_from_subquery_when_materialization_is_not_required();

            AssertSql(
                @"@__p_0='3'

SELECT TOP(@__p_0) [l2].[Name]
FROM [Level2] AS [l2]
INNER JOIN [Level1] AS [l2.OneToOne_Required_FK_Inverse] ON [l2].[Level1_Required_Id] = [l2.OneToOne_Required_FK_Inverse].[Id]
WHERE [l2.OneToOne_Required_FK_Inverse].[Name] = N'L1 03'");
        }

        public override void Projection_select_correct_table_with_anonymous_projection_in_subquery()
        {
            base.Projection_select_correct_table_with_anonymous_projection_in_subquery();

            AssertSql(
                @"@__p_0='3'

SELECT TOP(@__p_0) [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_InverseId], [l2].[OneToMany_Optional_Self_InverseId], [l2].[OneToMany_Required_InverseId], [l2].[OneToMany_Required_Self_InverseId], [l2].[OneToOne_Optional_PK_InverseId], [l2].[OneToOne_Optional_SelfId], [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_InverseId], [l1].[OneToMany_Required_Self_InverseId], [l1].[OneToOne_Optional_SelfId]
FROM [Level2] AS [l2]
INNER JOIN [Level1] AS [l1] ON [l2].[Level1_Required_Id] = [l1].[Id]
INNER JOIN [Level3] AS [l3] ON [l1].[Id] = [l3].[Level2_Required_Id]
WHERE ([l1].[Name] = N'L1 03') AND ([l3].[Name] = N'L3 08')");
        }

        public override void Projection_select_correct_table_in_subquery_when_materialization_is_not_required_in_multiple_joins()
        {
            base.Projection_select_correct_table_in_subquery_when_materialization_is_not_required_in_multiple_joins();

            AssertSql(
                @"@__p_0='3'

SELECT TOP(@__p_0) [l1].[Name]
FROM [Level2] AS [l2]
INNER JOIN [Level1] AS [l1] ON [l2].[Level1_Required_Id] = [l1].[Id]
INNER JOIN [Level3] AS [l3] ON [l1].[Id] = [l3].[Level2_Required_Id]
WHERE ([l1].[Name] = N'L1 03') AND ([l3].[Name] = N'L3 08')");
        }

        public override void Where_predicate_on_optional_reference_navigation()
        {
            base.Where_predicate_on_optional_reference_navigation();

            AssertSql(
                @"@__p_0='3'

SELECT TOP(@__p_0) [l1].[Name]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l1.OneToOne_Required_FK] ON [l1].[Id] = [l1.OneToOne_Required_FK].[Level1_Required_Id]
WHERE [l1.OneToOne_Required_FK].[Name] = N'L2 03'");
        }

        public override void SelectMany_with_Include1()
        {
            base.SelectMany_with_Include1();

            AssertSql(
                @"SELECT [l1.OneToMany_Optional].[Id], [l1.OneToMany_Optional].[Date], [l1.OneToMany_Optional].[Level1_Optional_Id], [l1.OneToMany_Optional].[Level1_Required_Id], [l1.OneToMany_Optional].[Name], [l1.OneToMany_Optional].[OneToMany_Optional_InverseId], [l1.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [l1.OneToMany_Optional].[OneToMany_Required_InverseId], [l1.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [l1.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [l1.OneToMany_Optional].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
INNER JOIN [Level2] AS [l1.OneToMany_Optional] ON [l1].[Id] = [l1.OneToMany_Optional].[OneToMany_Optional_InverseId]
ORDER BY [l1.OneToMany_Optional].[Id]",
                //
                @"SELECT [l1.OneToMany_Optional.OneToMany_Optional].[Id], [l1.OneToMany_Optional.OneToMany_Optional].[Level2_Optional_Id], [l1.OneToMany_Optional.OneToMany_Optional].[Level2_Required_Id], [l1.OneToMany_Optional.OneToMany_Optional].[Name], [l1.OneToMany_Optional.OneToMany_Optional].[OneToMany_Optional_InverseId], [l1.OneToMany_Optional.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [l1.OneToMany_Optional.OneToMany_Optional].[OneToMany_Required_InverseId], [l1.OneToMany_Optional.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [l1.OneToMany_Optional.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [l1.OneToMany_Optional.OneToMany_Optional].[OneToOne_Optional_SelfId]
FROM [Level3] AS [l1.OneToMany_Optional.OneToMany_Optional]
INNER JOIN (
    SELECT DISTINCT [l1.OneToMany_Optional0].[Id]
    FROM [Level1] AS [l10]
    INNER JOIN [Level2] AS [l1.OneToMany_Optional0] ON [l10].[Id] = [l1.OneToMany_Optional0].[OneToMany_Optional_InverseId]
) AS [t] ON [l1.OneToMany_Optional.OneToMany_Optional].[OneToMany_Optional_InverseId] = [t].[Id]
ORDER BY [t].[Id]");
        }

        public override void SelectMany_with_Include2()
        {
            base.SelectMany_with_Include2();

            AssertSql(
                @"SELECT [l1.OneToMany_Optional].[Id], [l1.OneToMany_Optional].[Date], [l1.OneToMany_Optional].[Level1_Optional_Id], [l1.OneToMany_Optional].[Level1_Required_Id], [l1.OneToMany_Optional].[Name], [l1.OneToMany_Optional].[OneToMany_Optional_InverseId], [l1.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [l1.OneToMany_Optional].[OneToMany_Required_InverseId], [l1.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [l1.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [l1.OneToMany_Optional].[OneToOne_Optional_SelfId], [l1.OneToMany_Optional.OneToOne_Required_FK].[Id], [l1.OneToMany_Optional.OneToOne_Required_FK].[Level2_Optional_Id], [l1.OneToMany_Optional.OneToOne_Required_FK].[Level2_Required_Id], [l1.OneToMany_Optional.OneToOne_Required_FK].[Name], [l1.OneToMany_Optional.OneToOne_Required_FK].[OneToMany_Optional_InverseId], [l1.OneToMany_Optional.OneToOne_Required_FK].[OneToMany_Optional_Self_InverseId], [l1.OneToMany_Optional.OneToOne_Required_FK].[OneToMany_Required_InverseId], [l1.OneToMany_Optional.OneToOne_Required_FK].[OneToMany_Required_Self_InverseId], [l1.OneToMany_Optional.OneToOne_Required_FK].[OneToOne_Optional_PK_InverseId], [l1.OneToMany_Optional.OneToOne_Required_FK].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
INNER JOIN [Level2] AS [l1.OneToMany_Optional] ON [l1].[Id] = [l1.OneToMany_Optional].[OneToMany_Optional_InverseId]
LEFT JOIN [Level3] AS [l1.OneToMany_Optional.OneToOne_Required_FK] ON [l1.OneToMany_Optional].[Id] = [l1.OneToMany_Optional.OneToOne_Required_FK].[Level2_Required_Id]");
        }

        public override void SelectMany_with_Include_ThenInclude()
        {
            base.SelectMany_with_Include_ThenInclude();

            AssertSql(
                @"SELECT [l1.OneToMany_Optional].[Id], [l1.OneToMany_Optional].[Date], [l1.OneToMany_Optional].[Level1_Optional_Id], [l1.OneToMany_Optional].[Level1_Required_Id], [l1.OneToMany_Optional].[Name], [l1.OneToMany_Optional].[OneToMany_Optional_InverseId], [l1.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [l1.OneToMany_Optional].[OneToMany_Required_InverseId], [l1.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [l1.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [l1.OneToMany_Optional].[OneToOne_Optional_SelfId], [l1.OneToMany_Optional.OneToOne_Required_FK].[Id], [l1.OneToMany_Optional.OneToOne_Required_FK].[Level2_Optional_Id], [l1.OneToMany_Optional.OneToOne_Required_FK].[Level2_Required_Id], [l1.OneToMany_Optional.OneToOne_Required_FK].[Name], [l1.OneToMany_Optional.OneToOne_Required_FK].[OneToMany_Optional_InverseId], [l1.OneToMany_Optional.OneToOne_Required_FK].[OneToMany_Optional_Self_InverseId], [l1.OneToMany_Optional.OneToOne_Required_FK].[OneToMany_Required_InverseId], [l1.OneToMany_Optional.OneToOne_Required_FK].[OneToMany_Required_Self_InverseId], [l1.OneToMany_Optional.OneToOne_Required_FK].[OneToOne_Optional_PK_InverseId], [l1.OneToMany_Optional.OneToOne_Required_FK].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
INNER JOIN [Level2] AS [l1.OneToMany_Optional] ON [l1].[Id] = [l1.OneToMany_Optional].[OneToMany_Optional_InverseId]
LEFT JOIN [Level3] AS [l1.OneToMany_Optional.OneToOne_Required_FK] ON [l1.OneToMany_Optional].[Id] = [l1.OneToMany_Optional.OneToOne_Required_FK].[Level2_Required_Id]
ORDER BY [l1.OneToMany_Optional.OneToOne_Required_FK].[Id]",
                //
                @"SELECT [l1.OneToMany_Optional.OneToOne_Required_FK.OneToMany_Optional].[Id], [l1.OneToMany_Optional.OneToOne_Required_FK.OneToMany_Optional].[Level3_Optional_Id], [l1.OneToMany_Optional.OneToOne_Required_FK.OneToMany_Optional].[Level3_Required_Id], [l1.OneToMany_Optional.OneToOne_Required_FK.OneToMany_Optional].[Name], [l1.OneToMany_Optional.OneToOne_Required_FK.OneToMany_Optional].[OneToMany_Optional_InverseId], [l1.OneToMany_Optional.OneToOne_Required_FK.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [l1.OneToMany_Optional.OneToOne_Required_FK.OneToMany_Optional].[OneToMany_Required_InverseId], [l1.OneToMany_Optional.OneToOne_Required_FK.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [l1.OneToMany_Optional.OneToOne_Required_FK.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [l1.OneToMany_Optional.OneToOne_Required_FK.OneToMany_Optional].[OneToOne_Optional_SelfId]
FROM [Level4] AS [l1.OneToMany_Optional.OneToOne_Required_FK.OneToMany_Optional]
INNER JOIN (
    SELECT DISTINCT [l1.OneToMany_Optional.OneToOne_Required_FK0].[Id]
    FROM [Level1] AS [l10]
    INNER JOIN [Level2] AS [l1.OneToMany_Optional0] ON [l10].[Id] = [l1.OneToMany_Optional0].[OneToMany_Optional_InverseId]
    LEFT JOIN [Level3] AS [l1.OneToMany_Optional.OneToOne_Required_FK0] ON [l1.OneToMany_Optional0].[Id] = [l1.OneToMany_Optional.OneToOne_Required_FK0].[Level2_Required_Id]
) AS [t] ON [l1.OneToMany_Optional.OneToOne_Required_FK.OneToMany_Optional].[OneToMany_Optional_InverseId] = [t].[Id]
ORDER BY [t].[Id]");
        }

        public override void Multiple_SelectMany_with_Include()
        {
            base.Multiple_SelectMany_with_Include();

            AssertSql(
                @"SELECT [l1.OneToMany_Optional.OneToMany_Optional].[Id], [l1.OneToMany_Optional.OneToMany_Optional].[Level2_Optional_Id], [l1.OneToMany_Optional.OneToMany_Optional].[Level2_Required_Id], [l1.OneToMany_Optional.OneToMany_Optional].[Name], [l1.OneToMany_Optional.OneToMany_Optional].[OneToMany_Optional_InverseId], [l1.OneToMany_Optional.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [l1.OneToMany_Optional.OneToMany_Optional].[OneToMany_Required_InverseId], [l1.OneToMany_Optional.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [l1.OneToMany_Optional.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [l1.OneToMany_Optional.OneToMany_Optional].[OneToOne_Optional_SelfId], [l1.OneToMany_Optional.OneToMany_Optional.OneToOne_Required_FK].[Id], [l1.OneToMany_Optional.OneToMany_Optional.OneToOne_Required_FK].[Level3_Optional_Id], [l1.OneToMany_Optional.OneToMany_Optional.OneToOne_Required_FK].[Level3_Required_Id], [l1.OneToMany_Optional.OneToMany_Optional.OneToOne_Required_FK].[Name], [l1.OneToMany_Optional.OneToMany_Optional.OneToOne_Required_FK].[OneToMany_Optional_InverseId], [l1.OneToMany_Optional.OneToMany_Optional.OneToOne_Required_FK].[OneToMany_Optional_Self_InverseId], [l1.OneToMany_Optional.OneToMany_Optional.OneToOne_Required_FK].[OneToMany_Required_InverseId], [l1.OneToMany_Optional.OneToMany_Optional.OneToOne_Required_FK].[OneToMany_Required_Self_InverseId], [l1.OneToMany_Optional.OneToMany_Optional.OneToOne_Required_FK].[OneToOne_Optional_PK_InverseId], [l1.OneToMany_Optional.OneToMany_Optional.OneToOne_Required_FK].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
INNER JOIN [Level2] AS [l1.OneToMany_Optional] ON [l1].[Id] = [l1.OneToMany_Optional].[OneToMany_Optional_InverseId]
INNER JOIN [Level3] AS [l1.OneToMany_Optional.OneToMany_Optional] ON [l1.OneToMany_Optional].[Id] = [l1.OneToMany_Optional.OneToMany_Optional].[OneToMany_Optional_InverseId]
LEFT JOIN [Level4] AS [l1.OneToMany_Optional.OneToMany_Optional.OneToOne_Required_FK] ON [l1.OneToMany_Optional.OneToMany_Optional].[Id] = [l1.OneToMany_Optional.OneToMany_Optional.OneToOne_Required_FK].[Level3_Required_Id]
ORDER BY [l1.OneToMany_Optional.OneToMany_Optional].[Id]",
                //
                @"SELECT [l1.OneToMany_Optional.OneToMany_Optional.OneToMany_Optional].[Id], [l1.OneToMany_Optional.OneToMany_Optional.OneToMany_Optional].[Level3_Optional_Id], [l1.OneToMany_Optional.OneToMany_Optional.OneToMany_Optional].[Level3_Required_Id], [l1.OneToMany_Optional.OneToMany_Optional.OneToMany_Optional].[Name], [l1.OneToMany_Optional.OneToMany_Optional.OneToMany_Optional].[OneToMany_Optional_InverseId], [l1.OneToMany_Optional.OneToMany_Optional.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [l1.OneToMany_Optional.OneToMany_Optional.OneToMany_Optional].[OneToMany_Required_InverseId], [l1.OneToMany_Optional.OneToMany_Optional.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [l1.OneToMany_Optional.OneToMany_Optional.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [l1.OneToMany_Optional.OneToMany_Optional.OneToMany_Optional].[OneToOne_Optional_SelfId]
FROM [Level4] AS [l1.OneToMany_Optional.OneToMany_Optional.OneToMany_Optional]
INNER JOIN (
    SELECT DISTINCT [l1.OneToMany_Optional.OneToMany_Optional0].[Id]
    FROM [Level1] AS [l10]
    INNER JOIN [Level2] AS [l1.OneToMany_Optional0] ON [l10].[Id] = [l1.OneToMany_Optional0].[OneToMany_Optional_InverseId]
    INNER JOIN [Level3] AS [l1.OneToMany_Optional.OneToMany_Optional0] ON [l1.OneToMany_Optional0].[Id] = [l1.OneToMany_Optional.OneToMany_Optional0].[OneToMany_Optional_InverseId]
    LEFT JOIN [Level4] AS [l1.OneToMany_Optional.OneToMany_Optional.OneToOne_Required_FK0] ON [l1.OneToMany_Optional.OneToMany_Optional0].[Id] = [l1.OneToMany_Optional.OneToMany_Optional.OneToOne_Required_FK0].[Level3_Required_Id]
) AS [t] ON [l1.OneToMany_Optional.OneToMany_Optional.OneToMany_Optional].[OneToMany_Optional_InverseId] = [t].[Id]
ORDER BY [t].[Id]");
        }

        public override void SelectMany_with_string_based_Include1()
        {
            base.SelectMany_with_string_based_Include1();

            AssertSql(
                @"SELECT [l1.OneToMany_Optional].[Id], [l1.OneToMany_Optional].[Date], [l1.OneToMany_Optional].[Level1_Optional_Id], [l1.OneToMany_Optional].[Level1_Required_Id], [l1.OneToMany_Optional].[Name], [l1.OneToMany_Optional].[OneToMany_Optional_InverseId], [l1.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [l1.OneToMany_Optional].[OneToMany_Required_InverseId], [l1.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [l1.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [l1.OneToMany_Optional].[OneToOne_Optional_SelfId], [l1.OneToMany_Optional.OneToOne_Required_FK].[Id], [l1.OneToMany_Optional.OneToOne_Required_FK].[Level2_Optional_Id], [l1.OneToMany_Optional.OneToOne_Required_FK].[Level2_Required_Id], [l1.OneToMany_Optional.OneToOne_Required_FK].[Name], [l1.OneToMany_Optional.OneToOne_Required_FK].[OneToMany_Optional_InverseId], [l1.OneToMany_Optional.OneToOne_Required_FK].[OneToMany_Optional_Self_InverseId], [l1.OneToMany_Optional.OneToOne_Required_FK].[OneToMany_Required_InverseId], [l1.OneToMany_Optional.OneToOne_Required_FK].[OneToMany_Required_Self_InverseId], [l1.OneToMany_Optional.OneToOne_Required_FK].[OneToOne_Optional_PK_InverseId], [l1.OneToMany_Optional.OneToOne_Required_FK].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
INNER JOIN [Level2] AS [l1.OneToMany_Optional] ON [l1].[Id] = [l1.OneToMany_Optional].[OneToMany_Optional_InverseId]
LEFT JOIN [Level3] AS [l1.OneToMany_Optional.OneToOne_Required_FK] ON [l1.OneToMany_Optional].[Id] = [l1.OneToMany_Optional.OneToOne_Required_FK].[Level2_Required_Id]");
        }

        public override void SelectMany_with_string_based_Include2()
        {
            base.SelectMany_with_string_based_Include2();

            AssertSql(
                @"SELECT [l1.OneToMany_Optional].[Id], [l1.OneToMany_Optional].[Date], [l1.OneToMany_Optional].[Level1_Optional_Id], [l1.OneToMany_Optional].[Level1_Required_Id], [l1.OneToMany_Optional].[Name], [l1.OneToMany_Optional].[OneToMany_Optional_InverseId], [l1.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [l1.OneToMany_Optional].[OneToMany_Required_InverseId], [l1.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [l1.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [l1.OneToMany_Optional].[OneToOne_Optional_SelfId], [l1.OneToMany_Optional.OneToOne_Required_FK].[Id], [l1.OneToMany_Optional.OneToOne_Required_FK].[Level2_Optional_Id], [l1.OneToMany_Optional.OneToOne_Required_FK].[Level2_Required_Id], [l1.OneToMany_Optional.OneToOne_Required_FK].[Name], [l1.OneToMany_Optional.OneToOne_Required_FK].[OneToMany_Optional_InverseId], [l1.OneToMany_Optional.OneToOne_Required_FK].[OneToMany_Optional_Self_InverseId], [l1.OneToMany_Optional.OneToOne_Required_FK].[OneToMany_Required_InverseId], [l1.OneToMany_Optional.OneToOne_Required_FK].[OneToMany_Required_Self_InverseId], [l1.OneToMany_Optional.OneToOne_Required_FK].[OneToOne_Optional_PK_InverseId], [l1.OneToMany_Optional.OneToOne_Required_FK].[OneToOne_Optional_SelfId], [l1.OneToMany_Optional.OneToOne_Required_FK.OneToOne_Required_FK].[Id], [l1.OneToMany_Optional.OneToOne_Required_FK.OneToOne_Required_FK].[Level3_Optional_Id], [l1.OneToMany_Optional.OneToOne_Required_FK.OneToOne_Required_FK].[Level3_Required_Id], [l1.OneToMany_Optional.OneToOne_Required_FK.OneToOne_Required_FK].[Name], [l1.OneToMany_Optional.OneToOne_Required_FK.OneToOne_Required_FK].[OneToMany_Optional_InverseId], [l1.OneToMany_Optional.OneToOne_Required_FK.OneToOne_Required_FK].[OneToMany_Optional_Self_InverseId], [l1.OneToMany_Optional.OneToOne_Required_FK.OneToOne_Required_FK].[OneToMany_Required_InverseId], [l1.OneToMany_Optional.OneToOne_Required_FK.OneToOne_Required_FK].[OneToMany_Required_Self_InverseId], [l1.OneToMany_Optional.OneToOne_Required_FK.OneToOne_Required_FK].[OneToOne_Optional_PK_InverseId], [l1.OneToMany_Optional.OneToOne_Required_FK.OneToOne_Required_FK].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
INNER JOIN [Level2] AS [l1.OneToMany_Optional] ON [l1].[Id] = [l1.OneToMany_Optional].[OneToMany_Optional_InverseId]
LEFT JOIN [Level3] AS [l1.OneToMany_Optional.OneToOne_Required_FK] ON [l1.OneToMany_Optional].[Id] = [l1.OneToMany_Optional.OneToOne_Required_FK].[Level2_Required_Id]
LEFT JOIN [Level4] AS [l1.OneToMany_Optional.OneToOne_Required_FK.OneToOne_Required_FK] ON [l1.OneToMany_Optional.OneToOne_Required_FK].[Id] = [l1.OneToMany_Optional.OneToOne_Required_FK.OneToOne_Required_FK].[Level3_Required_Id]");
        }

        public override void Multiple_SelectMany_with_string_based_Include()
        {
            base.Multiple_SelectMany_with_string_based_Include();

            AssertSql(
                @"SELECT [l1.OneToMany_Optional.OneToMany_Optional].[Id], [l1.OneToMany_Optional.OneToMany_Optional].[Level2_Optional_Id], [l1.OneToMany_Optional.OneToMany_Optional].[Level2_Required_Id], [l1.OneToMany_Optional.OneToMany_Optional].[Name], [l1.OneToMany_Optional.OneToMany_Optional].[OneToMany_Optional_InverseId], [l1.OneToMany_Optional.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [l1.OneToMany_Optional.OneToMany_Optional].[OneToMany_Required_InverseId], [l1.OneToMany_Optional.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [l1.OneToMany_Optional.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [l1.OneToMany_Optional.OneToMany_Optional].[OneToOne_Optional_SelfId], [l1.OneToMany_Optional.OneToMany_Optional.OneToOne_Required_FK].[Id], [l1.OneToMany_Optional.OneToMany_Optional.OneToOne_Required_FK].[Level3_Optional_Id], [l1.OneToMany_Optional.OneToMany_Optional.OneToOne_Required_FK].[Level3_Required_Id], [l1.OneToMany_Optional.OneToMany_Optional.OneToOne_Required_FK].[Name], [l1.OneToMany_Optional.OneToMany_Optional.OneToOne_Required_FK].[OneToMany_Optional_InverseId], [l1.OneToMany_Optional.OneToMany_Optional.OneToOne_Required_FK].[OneToMany_Optional_Self_InverseId], [l1.OneToMany_Optional.OneToMany_Optional.OneToOne_Required_FK].[OneToMany_Required_InverseId], [l1.OneToMany_Optional.OneToMany_Optional.OneToOne_Required_FK].[OneToMany_Required_Self_InverseId], [l1.OneToMany_Optional.OneToMany_Optional.OneToOne_Required_FK].[OneToOne_Optional_PK_InverseId], [l1.OneToMany_Optional.OneToMany_Optional.OneToOne_Required_FK].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
INNER JOIN [Level2] AS [l1.OneToMany_Optional] ON [l1].[Id] = [l1.OneToMany_Optional].[OneToMany_Optional_InverseId]
INNER JOIN [Level3] AS [l1.OneToMany_Optional.OneToMany_Optional] ON [l1.OneToMany_Optional].[Id] = [l1.OneToMany_Optional.OneToMany_Optional].[OneToMany_Optional_InverseId]
LEFT JOIN [Level4] AS [l1.OneToMany_Optional.OneToMany_Optional.OneToOne_Required_FK] ON [l1.OneToMany_Optional.OneToMany_Optional].[Id] = [l1.OneToMany_Optional.OneToMany_Optional.OneToOne_Required_FK].[Level3_Required_Id]");
        }

        public override void Required_navigation_with_Include()
        {
            base.Required_navigation_with_Include();

            AssertSql(
                @"SELECT [l3.OneToOne_Required_FK_Inverse].[Id], [l3.OneToOne_Required_FK_Inverse].[Date], [l3.OneToOne_Required_FK_Inverse].[Level1_Optional_Id], [l3.OneToOne_Required_FK_Inverse].[Level1_Required_Id], [l3.OneToOne_Required_FK_Inverse].[Name], [l3.OneToOne_Required_FK_Inverse].[OneToMany_Optional_InverseId], [l3.OneToOne_Required_FK_Inverse].[OneToMany_Optional_Self_InverseId], [l3.OneToOne_Required_FK_Inverse].[OneToMany_Required_InverseId], [l3.OneToOne_Required_FK_Inverse].[OneToMany_Required_Self_InverseId], [l3.OneToOne_Required_FK_Inverse].[OneToOne_Optional_PK_InverseId], [l3.OneToOne_Required_FK_Inverse].[OneToOne_Optional_SelfId], [l3.OneToOne_Required_FK_Inverse.OneToMany_Required_Inverse].[Id], [l3.OneToOne_Required_FK_Inverse.OneToMany_Required_Inverse].[Date], [l3.OneToOne_Required_FK_Inverse.OneToMany_Required_Inverse].[Name], [l3.OneToOne_Required_FK_Inverse.OneToMany_Required_Inverse].[OneToMany_Optional_Self_InverseId], [l3.OneToOne_Required_FK_Inverse.OneToMany_Required_Inverse].[OneToMany_Required_Self_InverseId], [l3.OneToOne_Required_FK_Inverse.OneToMany_Required_Inverse].[OneToOne_Optional_SelfId]
FROM [Level3] AS [l3]
INNER JOIN [Level2] AS [l3.OneToOne_Required_FK_Inverse] ON [l3].[Level2_Required_Id] = [l3.OneToOne_Required_FK_Inverse].[Id]
INNER JOIN [Level1] AS [l3.OneToOne_Required_FK_Inverse.OneToMany_Required_Inverse] ON [l3.OneToOne_Required_FK_Inverse].[OneToMany_Required_InverseId] = [l3.OneToOne_Required_FK_Inverse.OneToMany_Required_Inverse].[Id]");
        }

        public override void Required_navigation_with_Include_ThenInclude()
        {
            base.Required_navigation_with_Include_ThenInclude();

            AssertSql(
                @"SELECT [l4.OneToOne_Required_FK_Inverse].[Id], [l4.OneToOne_Required_FK_Inverse].[Level2_Optional_Id], [l4.OneToOne_Required_FK_Inverse].[Level2_Required_Id], [l4.OneToOne_Required_FK_Inverse].[Name], [l4.OneToOne_Required_FK_Inverse].[OneToMany_Optional_InverseId], [l4.OneToOne_Required_FK_Inverse].[OneToMany_Optional_Self_InverseId], [l4.OneToOne_Required_FK_Inverse].[OneToMany_Required_InverseId], [l4.OneToOne_Required_FK_Inverse].[OneToMany_Required_Self_InverseId], [l4.OneToOne_Required_FK_Inverse].[OneToOne_Optional_PK_InverseId], [l4.OneToOne_Required_FK_Inverse].[OneToOne_Optional_SelfId], [l4.OneToOne_Required_FK_Inverse.OneToMany_Required_Inverse].[Id], [l4.OneToOne_Required_FK_Inverse.OneToMany_Required_Inverse].[Date], [l4.OneToOne_Required_FK_Inverse.OneToMany_Required_Inverse].[Level1_Optional_Id], [l4.OneToOne_Required_FK_Inverse.OneToMany_Required_Inverse].[Level1_Required_Id], [l4.OneToOne_Required_FK_Inverse.OneToMany_Required_Inverse].[Name], [l4.OneToOne_Required_FK_Inverse.OneToMany_Required_Inverse].[OneToMany_Optional_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToMany_Required_Inverse].[OneToMany_Optional_Self_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToMany_Required_Inverse].[OneToMany_Required_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToMany_Required_Inverse].[OneToMany_Required_Self_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToMany_Required_Inverse].[OneToOne_Optional_PK_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToMany_Required_Inverse].[OneToOne_Optional_SelfId], [l4.OneToOne_Required_FK_Inverse.OneToMany_Required_Inverse.OneToMany_Optional_Inverse].[Id], [l4.OneToOne_Required_FK_Inverse.OneToMany_Required_Inverse.OneToMany_Optional_Inverse].[Date], [l4.OneToOne_Required_FK_Inverse.OneToMany_Required_Inverse.OneToMany_Optional_Inverse].[Name], [l4.OneToOne_Required_FK_Inverse.OneToMany_Required_Inverse.OneToMany_Optional_Inverse].[OneToMany_Optional_Self_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToMany_Required_Inverse.OneToMany_Optional_Inverse].[OneToMany_Required_Self_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToMany_Required_Inverse.OneToMany_Optional_Inverse].[OneToOne_Optional_SelfId]
FROM [Level4] AS [l4]
INNER JOIN [Level3] AS [l4.OneToOne_Required_FK_Inverse] ON [l4].[Level3_Required_Id] = [l4.OneToOne_Required_FK_Inverse].[Id]
INNER JOIN [Level2] AS [l4.OneToOne_Required_FK_Inverse.OneToMany_Required_Inverse] ON [l4.OneToOne_Required_FK_Inverse].[OneToMany_Required_InverseId] = [l4.OneToOne_Required_FK_Inverse.OneToMany_Required_Inverse].[Id]
LEFT JOIN [Level1] AS [l4.OneToOne_Required_FK_Inverse.OneToMany_Required_Inverse.OneToMany_Optional_Inverse] ON [l4.OneToOne_Required_FK_Inverse.OneToMany_Required_Inverse].[OneToMany_Optional_InverseId] = [l4.OneToOne_Required_FK_Inverse.OneToMany_Required_Inverse.OneToMany_Optional_Inverse].[Id]");
        }

        public override void Multiple_required_navigations_with_Include()
        {
            base.Multiple_required_navigations_with_Include();

            AssertSql(
                @"SELECT [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Id], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Date], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Level1_Optional_Id], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Level1_Required_Id], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Name], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[OneToMany_Optional_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[OneToMany_Optional_Self_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[OneToMany_Required_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[OneToMany_Required_Self_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[OneToOne_Optional_PK_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[OneToOne_Optional_SelfId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[Id], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[Level2_Optional_Id], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[Level2_Required_Id], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[Name], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[OneToMany_Optional_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[OneToMany_Optional_Self_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[OneToMany_Required_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[OneToMany_Required_Self_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[OneToOne_Optional_PK_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[OneToOne_Optional_SelfId]
FROM [Level4] AS [l4]
INNER JOIN [Level3] AS [l4.OneToOne_Required_FK_Inverse] ON [l4].[Level3_Required_Id] = [l4.OneToOne_Required_FK_Inverse].[Id]
INNER JOIN [Level2] AS [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse] ON [l4.OneToOne_Required_FK_Inverse].[Level2_Required_Id] = [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Id]
LEFT JOIN [Level3] AS [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK] ON [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Id] = [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[Level2_Optional_Id]");
        }

        public override void Multiple_required_navigation_using_multiple_selects_with_Include()
        {
            base.Multiple_required_navigation_using_multiple_selects_with_Include();

            AssertSql(
                @"SELECT [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Id], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Date], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Level1_Optional_Id], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Level1_Required_Id], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Name], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[OneToMany_Optional_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[OneToMany_Optional_Self_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[OneToMany_Required_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[OneToMany_Required_Self_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[OneToOne_Optional_PK_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[OneToOne_Optional_SelfId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[Id], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[Level2_Optional_Id], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[Level2_Required_Id], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[Name], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[OneToMany_Optional_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[OneToMany_Optional_Self_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[OneToMany_Required_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[OneToMany_Required_Self_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[OneToOne_Optional_PK_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[OneToOne_Optional_SelfId]
FROM [Level4] AS [l4]
INNER JOIN [Level3] AS [l4.OneToOne_Required_FK_Inverse] ON [l4].[Level3_Required_Id] = [l4.OneToOne_Required_FK_Inverse].[Id]
INNER JOIN [Level2] AS [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse] ON [l4.OneToOne_Required_FK_Inverse].[Level2_Required_Id] = [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Id]
LEFT JOIN [Level3] AS [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK] ON [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Id] = [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[Level2_Optional_Id]");
        }

        public override void Multiple_required_navigation_with_string_based_Include()
        {
            base.Multiple_required_navigation_with_string_based_Include();

            AssertSql(
                @"SELECT [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Id], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Date], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Level1_Optional_Id], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Level1_Required_Id], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Name], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[OneToMany_Optional_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[OneToMany_Optional_Self_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[OneToMany_Required_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[OneToMany_Required_Self_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[OneToOne_Optional_PK_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[OneToOne_Optional_SelfId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[Id], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[Level2_Optional_Id], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[Level2_Required_Id], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[Name], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[OneToMany_Optional_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[OneToMany_Optional_Self_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[OneToMany_Required_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[OneToMany_Required_Self_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[OneToOne_Optional_PK_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[OneToOne_Optional_SelfId]
FROM [Level4] AS [l4]
INNER JOIN [Level3] AS [l4.OneToOne_Required_FK_Inverse] ON [l4].[Level3_Required_Id] = [l4.OneToOne_Required_FK_Inverse].[Id]
INNER JOIN [Level2] AS [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse] ON [l4.OneToOne_Required_FK_Inverse].[Level2_Required_Id] = [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Id]
LEFT JOIN [Level3] AS [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK] ON [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Id] = [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[Level2_Optional_Id]");
        }

        public override void Multiple_required_navigation_using_multiple_selects_with_string_based_Include()
        {
            base.Multiple_required_navigation_using_multiple_selects_with_string_based_Include();

            AssertSql(
                @"SELECT [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Id], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Date], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Level1_Optional_Id], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Level1_Required_Id], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Name], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[OneToMany_Optional_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[OneToMany_Optional_Self_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[OneToMany_Required_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[OneToMany_Required_Self_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[OneToOne_Optional_PK_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[OneToOne_Optional_SelfId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[Id], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[Level2_Optional_Id], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[Level2_Required_Id], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[Name], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[OneToMany_Optional_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[OneToMany_Optional_Self_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[OneToMany_Required_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[OneToMany_Required_Self_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[OneToOne_Optional_PK_InverseId], [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[OneToOne_Optional_SelfId]
FROM [Level4] AS [l4]
INNER JOIN [Level3] AS [l4.OneToOne_Required_FK_Inverse] ON [l4].[Level3_Required_Id] = [l4.OneToOne_Required_FK_Inverse].[Id]
INNER JOIN [Level2] AS [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse] ON [l4.OneToOne_Required_FK_Inverse].[Level2_Required_Id] = [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Id]
LEFT JOIN [Level3] AS [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK] ON [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Id] = [l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[Level2_Optional_Id]");
        }

        public override void Optional_navigation_with_Include()
        {
            base.Optional_navigation_with_Include();

            AssertSql(
                @"SELECT [l1.OneToOne_Optional_FK].[Id], [l1.OneToOne_Optional_FK].[Date], [l1.OneToOne_Optional_FK].[Level1_Optional_Id], [l1.OneToOne_Optional_FK].[Level1_Required_Id], [l1.OneToOne_Optional_FK].[Name], [l1.OneToOne_Optional_FK].[OneToMany_Optional_InverseId], [l1.OneToOne_Optional_FK].[OneToMany_Optional_Self_InverseId], [l1.OneToOne_Optional_FK].[OneToMany_Required_InverseId], [l1.OneToOne_Optional_FK].[OneToMany_Required_Self_InverseId], [l1.OneToOne_Optional_FK].[OneToOne_Optional_PK_InverseId], [l1.OneToOne_Optional_FK].[OneToOne_Optional_SelfId], [l1.OneToOne_Optional_FK.OneToOne_Optional_FK].[Id], [l1.OneToOne_Optional_FK.OneToOne_Optional_FK].[Level2_Optional_Id], [l1.OneToOne_Optional_FK.OneToOne_Optional_FK].[Level2_Required_Id], [l1.OneToOne_Optional_FK.OneToOne_Optional_FK].[Name], [l1.OneToOne_Optional_FK.OneToOne_Optional_FK].[OneToMany_Optional_InverseId], [l1.OneToOne_Optional_FK.OneToOne_Optional_FK].[OneToMany_Optional_Self_InverseId], [l1.OneToOne_Optional_FK.OneToOne_Optional_FK].[OneToMany_Required_InverseId], [l1.OneToOne_Optional_FK.OneToOne_Optional_FK].[OneToMany_Required_Self_InverseId], [l1.OneToOne_Optional_FK.OneToOne_Optional_FK].[OneToOne_Optional_PK_InverseId], [l1.OneToOne_Optional_FK.OneToOne_Optional_FK].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l1.OneToOne_Optional_FK] ON [l1].[Id] = [l1.OneToOne_Optional_FK].[Level1_Optional_Id]
LEFT JOIN [Level3] AS [l1.OneToOne_Optional_FK.OneToOne_Optional_FK] ON [l1.OneToOne_Optional_FK].[Id] = [l1.OneToOne_Optional_FK.OneToOne_Optional_FK].[Level2_Optional_Id]");
        }

        public override void Optional_navigation_with_Include_ThenInclude()
        {
            base.Optional_navigation_with_Include_ThenInclude();

            AssertSql(
                @"SELECT [l1.OneToOne_Optional_FK].[Id], [l1.OneToOne_Optional_FK].[Date], [l1.OneToOne_Optional_FK].[Level1_Optional_Id], [l1.OneToOne_Optional_FK].[Level1_Required_Id], [l1.OneToOne_Optional_FK].[Name], [l1.OneToOne_Optional_FK].[OneToMany_Optional_InverseId], [l1.OneToOne_Optional_FK].[OneToMany_Optional_Self_InverseId], [l1.OneToOne_Optional_FK].[OneToMany_Required_InverseId], [l1.OneToOne_Optional_FK].[OneToMany_Required_Self_InverseId], [l1.OneToOne_Optional_FK].[OneToOne_Optional_PK_InverseId], [l1.OneToOne_Optional_FK].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l1.OneToOne_Optional_FK] ON [l1].[Id] = [l1.OneToOne_Optional_FK].[Level1_Optional_Id]
ORDER BY [l1.OneToOne_Optional_FK].[Id]",
                //
                @"SELECT [l1.OneToOne_Optional_FK.OneToMany_Optional].[Id], [l1.OneToOne_Optional_FK.OneToMany_Optional].[Level2_Optional_Id], [l1.OneToOne_Optional_FK.OneToMany_Optional].[Level2_Required_Id], [l1.OneToOne_Optional_FK.OneToMany_Optional].[Name], [l1.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Optional_InverseId], [l1.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [l1.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Required_InverseId], [l1.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [l1.OneToOne_Optional_FK.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [l1.OneToOne_Optional_FK.OneToMany_Optional].[OneToOne_Optional_SelfId], [l.OneToOne_Optional_FK].[Id], [l.OneToOne_Optional_FK].[Level3_Optional_Id], [l.OneToOne_Optional_FK].[Level3_Required_Id], [l.OneToOne_Optional_FK].[Name], [l.OneToOne_Optional_FK].[OneToMany_Optional_InverseId], [l.OneToOne_Optional_FK].[OneToMany_Optional_Self_InverseId], [l.OneToOne_Optional_FK].[OneToMany_Required_InverseId], [l.OneToOne_Optional_FK].[OneToMany_Required_Self_InverseId], [l.OneToOne_Optional_FK].[OneToOne_Optional_PK_InverseId], [l.OneToOne_Optional_FK].[OneToOne_Optional_SelfId]
FROM [Level3] AS [l1.OneToOne_Optional_FK.OneToMany_Optional]
LEFT JOIN [Level4] AS [l.OneToOne_Optional_FK] ON [l1.OneToOne_Optional_FK.OneToMany_Optional].[Id] = [l.OneToOne_Optional_FK].[Level3_Optional_Id]
INNER JOIN (
    SELECT DISTINCT [l1.OneToOne_Optional_FK0].[Id]
    FROM [Level1] AS [l10]
    LEFT JOIN [Level2] AS [l1.OneToOne_Optional_FK0] ON [l10].[Id] = [l1.OneToOne_Optional_FK0].[Level1_Optional_Id]
) AS [t] ON [l1.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Optional_InverseId] = [t].[Id]
ORDER BY [t].[Id]");
        }

        public override void Multiple_optional_navigation_with_Include()
        {
            base.Multiple_optional_navigation_with_Include();

            AssertSql(
                @"SELECT [l1.OneToOne_Optional_FK.OneToOne_Optional_PK].[Id], [l1.OneToOne_Optional_FK.OneToOne_Optional_PK].[Level2_Optional_Id], [l1.OneToOne_Optional_FK.OneToOne_Optional_PK].[Level2_Required_Id], [l1.OneToOne_Optional_FK.OneToOne_Optional_PK].[Name], [l1.OneToOne_Optional_FK.OneToOne_Optional_PK].[OneToMany_Optional_InverseId], [l1.OneToOne_Optional_FK.OneToOne_Optional_PK].[OneToMany_Optional_Self_InverseId], [l1.OneToOne_Optional_FK.OneToOne_Optional_PK].[OneToMany_Required_InverseId], [l1.OneToOne_Optional_FK.OneToOne_Optional_PK].[OneToMany_Required_Self_InverseId], [l1.OneToOne_Optional_FK.OneToOne_Optional_PK].[OneToOne_Optional_PK_InverseId], [l1.OneToOne_Optional_FK.OneToOne_Optional_PK].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l1.OneToOne_Optional_FK] ON [l1].[Id] = [l1.OneToOne_Optional_FK].[Level1_Optional_Id]
LEFT JOIN [Level3] AS [l1.OneToOne_Optional_FK.OneToOne_Optional_PK] ON [l1.OneToOne_Optional_FK].[Id] = [l1.OneToOne_Optional_FK.OneToOne_Optional_PK].[OneToOne_Optional_PK_InverseId]
ORDER BY [l1.OneToOne_Optional_FK.OneToOne_Optional_PK].[Id]",
                //
                @"SELECT [l1.OneToOne_Optional_FK.OneToOne_Optional_PK.OneToMany_Optional].[Id], [l1.OneToOne_Optional_FK.OneToOne_Optional_PK.OneToMany_Optional].[Level3_Optional_Id], [l1.OneToOne_Optional_FK.OneToOne_Optional_PK.OneToMany_Optional].[Level3_Required_Id], [l1.OneToOne_Optional_FK.OneToOne_Optional_PK.OneToMany_Optional].[Name], [l1.OneToOne_Optional_FK.OneToOne_Optional_PK.OneToMany_Optional].[OneToMany_Optional_InverseId], [l1.OneToOne_Optional_FK.OneToOne_Optional_PK.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [l1.OneToOne_Optional_FK.OneToOne_Optional_PK.OneToMany_Optional].[OneToMany_Required_InverseId], [l1.OneToOne_Optional_FK.OneToOne_Optional_PK.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [l1.OneToOne_Optional_FK.OneToOne_Optional_PK.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [l1.OneToOne_Optional_FK.OneToOne_Optional_PK.OneToMany_Optional].[OneToOne_Optional_SelfId]
FROM [Level4] AS [l1.OneToOne_Optional_FK.OneToOne_Optional_PK.OneToMany_Optional]
INNER JOIN (
    SELECT DISTINCT [l1.OneToOne_Optional_FK.OneToOne_Optional_PK0].[Id]
    FROM [Level1] AS [l10]
    LEFT JOIN [Level2] AS [l1.OneToOne_Optional_FK0] ON [l10].[Id] = [l1.OneToOne_Optional_FK0].[Level1_Optional_Id]
    LEFT JOIN [Level3] AS [l1.OneToOne_Optional_FK.OneToOne_Optional_PK0] ON [l1.OneToOne_Optional_FK0].[Id] = [l1.OneToOne_Optional_FK.OneToOne_Optional_PK0].[OneToOne_Optional_PK_InverseId]
) AS [t] ON [l1.OneToOne_Optional_FK.OneToOne_Optional_PK.OneToMany_Optional].[OneToMany_Optional_InverseId] = [t].[Id]
ORDER BY [t].[Id]");
        }

        public override void Multiple_optional_navigation_with_string_based_Include()
        {
            base.Multiple_optional_navigation_with_string_based_Include();

            AssertSql(
                @"SELECT [l1.OneToOne_Optional_FK.OneToOne_Optional_PK].[Id], [l1.OneToOne_Optional_FK.OneToOne_Optional_PK].[Level2_Optional_Id], [l1.OneToOne_Optional_FK.OneToOne_Optional_PK].[Level2_Required_Id], [l1.OneToOne_Optional_FK.OneToOne_Optional_PK].[Name], [l1.OneToOne_Optional_FK.OneToOne_Optional_PK].[OneToMany_Optional_InverseId], [l1.OneToOne_Optional_FK.OneToOne_Optional_PK].[OneToMany_Optional_Self_InverseId], [l1.OneToOne_Optional_FK.OneToOne_Optional_PK].[OneToMany_Required_InverseId], [l1.OneToOne_Optional_FK.OneToOne_Optional_PK].[OneToMany_Required_Self_InverseId], [l1.OneToOne_Optional_FK.OneToOne_Optional_PK].[OneToOne_Optional_PK_InverseId], [l1.OneToOne_Optional_FK.OneToOne_Optional_PK].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l1.OneToOne_Optional_FK] ON [l1].[Id] = [l1.OneToOne_Optional_FK].[Level1_Optional_Id]
LEFT JOIN [Level3] AS [l1.OneToOne_Optional_FK.OneToOne_Optional_PK] ON [l1.OneToOne_Optional_FK].[Id] = [l1.OneToOne_Optional_FK.OneToOne_Optional_PK].[OneToOne_Optional_PK_InverseId]
ORDER BY [l1.OneToOne_Optional_FK.OneToOne_Optional_PK].[Id]",
                //
                @"SELECT [l1.OneToOne_Optional_FK.OneToOne_Optional_PK.OneToMany_Optional].[Id], [l1.OneToOne_Optional_FK.OneToOne_Optional_PK.OneToMany_Optional].[Level3_Optional_Id], [l1.OneToOne_Optional_FK.OneToOne_Optional_PK.OneToMany_Optional].[Level3_Required_Id], [l1.OneToOne_Optional_FK.OneToOne_Optional_PK.OneToMany_Optional].[Name], [l1.OneToOne_Optional_FK.OneToOne_Optional_PK.OneToMany_Optional].[OneToMany_Optional_InverseId], [l1.OneToOne_Optional_FK.OneToOne_Optional_PK.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [l1.OneToOne_Optional_FK.OneToOne_Optional_PK.OneToMany_Optional].[OneToMany_Required_InverseId], [l1.OneToOne_Optional_FK.OneToOne_Optional_PK.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [l1.OneToOne_Optional_FK.OneToOne_Optional_PK.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [l1.OneToOne_Optional_FK.OneToOne_Optional_PK.OneToMany_Optional].[OneToOne_Optional_SelfId]
FROM [Level4] AS [l1.OneToOne_Optional_FK.OneToOne_Optional_PK.OneToMany_Optional]
INNER JOIN (
    SELECT DISTINCT [l1.OneToOne_Optional_FK.OneToOne_Optional_PK0].[Id]
    FROM [Level1] AS [l10]
    LEFT JOIN [Level2] AS [l1.OneToOne_Optional_FK0] ON [l10].[Id] = [l1.OneToOne_Optional_FK0].[Level1_Optional_Id]
    LEFT JOIN [Level3] AS [l1.OneToOne_Optional_FK.OneToOne_Optional_PK0] ON [l1.OneToOne_Optional_FK0].[Id] = [l1.OneToOne_Optional_FK.OneToOne_Optional_PK0].[OneToOne_Optional_PK_InverseId]
) AS [t] ON [l1.OneToOne_Optional_FK.OneToOne_Optional_PK.OneToMany_Optional].[OneToMany_Optional_InverseId] = [t].[Id]
ORDER BY [t].[Id]");
        }

        public override void SelectMany_with_navigation_and_explicit_DefaultIfEmpty()
        {
            base.SelectMany_with_navigation_and_explicit_DefaultIfEmpty();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_InverseId], [l1].[OneToMany_Required_Self_InverseId], [l1].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l1.OneToMany_Optional] ON [l1].[Id] = [l1.OneToMany_Optional].[OneToMany_Optional_InverseId]
WHERE [l1.OneToMany_Optional].[Id] IS NOT NULL");
        }

        public override void SelectMany_with_navigation_and_Distinct()
        {
            base.SelectMany_with_navigation_and_Distinct();

            AssertContainsSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_InverseId], [l].[OneToMany_Required_Self_InverseId], [l].[OneToOne_Optional_SelfId], [l.OneToMany_Optional].[Id], [l.OneToMany_Optional].[Date], [l.OneToMany_Optional].[Level1_Optional_Id], [l.OneToMany_Optional].[Level1_Required_Id], [l.OneToMany_Optional].[Name], [l.OneToMany_Optional].[OneToMany_Optional_InverseId], [l.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [l.OneToMany_Optional].[OneToMany_Required_InverseId], [l.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [l.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [l.OneToMany_Optional].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l]
LEFT JOIN [Level2] AS [l.OneToMany_Optional] ON [l].[Id] = [l.OneToMany_Optional].[OneToMany_Optional_InverseId]
ORDER BY [l].[Id]",
                //
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_InverseId], [l1].[OneToMany_Required_Self_InverseId], [l1].[OneToOne_Optional_SelfId], [l.OneToMany_Optional2].[Id], [l.OneToMany_Optional2].[Date], [l.OneToMany_Optional2].[Level1_Optional_Id], [l.OneToMany_Optional2].[Level1_Required_Id], [l.OneToMany_Optional2].[Name], [l.OneToMany_Optional2].[OneToMany_Optional_InverseId], [l.OneToMany_Optional2].[OneToMany_Optional_Self_InverseId], [l.OneToMany_Optional2].[OneToMany_Required_InverseId], [l.OneToMany_Optional2].[OneToMany_Required_Self_InverseId], [l.OneToMany_Optional2].[OneToOne_Optional_PK_InverseId], [l.OneToMany_Optional2].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l.OneToMany_Optional2] ON [l1].[Id] = [l.OneToMany_Optional2].[OneToMany_Optional_InverseId]
ORDER BY [l1].[Id]",
                //
                @"SELECT [l.OneToMany_Optional0].[Id], [l.OneToMany_Optional0].[Date], [l.OneToMany_Optional0].[Level1_Optional_Id], [l.OneToMany_Optional0].[Level1_Required_Id], [l.OneToMany_Optional0].[Name], [l.OneToMany_Optional0].[OneToMany_Optional_InverseId], [l.OneToMany_Optional0].[OneToMany_Optional_Self_InverseId], [l.OneToMany_Optional0].[OneToMany_Required_InverseId], [l.OneToMany_Optional0].[OneToMany_Required_Self_InverseId], [l.OneToMany_Optional0].[OneToOne_Optional_PK_InverseId], [l.OneToMany_Optional0].[OneToOne_Optional_SelfId]
FROM [Level2] AS [l.OneToMany_Optional0]");
        }

        public override void SelectMany_with_navigation_filter_and_explicit_DefaultIfEmpty()
        {
            base.SelectMany_with_navigation_filter_and_explicit_DefaultIfEmpty();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_InverseId], [l1].[OneToMany_Required_Self_InverseId], [l1].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
LEFT JOIN (
    SELECT [l1.OneToMany_Optional].*
    FROM [Level2] AS [l1.OneToMany_Optional]
    WHERE [l1.OneToMany_Optional].[Id] > 5
) AS [t] ON [l1].[Id] = [t].[OneToMany_Optional_InverseId]
WHERE [t].[Id] IS NOT NULL");
        }

        public override void SelectMany_with_nested_navigation_and_explicit_DefaultIfEmpty()
        {
            base.SelectMany_with_nested_navigation_and_explicit_DefaultIfEmpty();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_InverseId], [l1].[OneToMany_Required_Self_InverseId], [l1].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
INNER JOIN [Level2] AS [l1.OneToOne_Required_FK] ON [l1].[Id] = [l1.OneToOne_Required_FK].[Level1_Required_Id]
LEFT JOIN [Level3] AS [l1.OneToOne_Required_FK.OneToMany_Optional] ON [l1.OneToOne_Required_FK].[Id] = [l1.OneToOne_Required_FK.OneToMany_Optional].[OneToMany_Optional_InverseId]
WHERE [l1.OneToOne_Required_FK.OneToMany_Optional].[Id] IS NOT NULL");
        }

        public override void SelectMany_with_nested_navigation_filter_and_explicit_DefaultIfEmpty()
        {
            base.SelectMany_with_nested_navigation_filter_and_explicit_DefaultIfEmpty();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_InverseId], [l1].[OneToMany_Required_Self_InverseId], [l1].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
INNER JOIN [Level2] AS [l1.OneToOne_Optional_FK] ON [l1].[Id] = [l1.OneToOne_Optional_FK].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l1.OneToOne_Optional_FK.OneToMany_Optional].*
    FROM [Level3] AS [l1.OneToOne_Optional_FK.OneToMany_Optional]
    WHERE [l1.OneToOne_Optional_FK.OneToMany_Optional].[Id] > 5
) AS [t] ON [l1.OneToOne_Optional_FK].[Id] = [t].[OneToMany_Optional_InverseId]
WHERE [t].[Id] IS NOT NULL");
        }

        public override void Multiple_SelectMany_with_navigation_and_explicit_DefaultIfEmpty()
        {
            base.Multiple_SelectMany_with_navigation_and_explicit_DefaultIfEmpty();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_InverseId], [l1].[OneToMany_Required_Self_InverseId], [l1].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
INNER JOIN [Level2] AS [l1.OneToMany_Optional] ON [l1].[Id] = [l1.OneToMany_Optional].[OneToMany_Optional_InverseId]
LEFT JOIN (
    SELECT [l1.OneToMany_Optional.OneToMany_Optional].*
    FROM [Level3] AS [l1.OneToMany_Optional.OneToMany_Optional]
    WHERE [l1.OneToMany_Optional.OneToMany_Optional].[Id] > 5
) AS [t] ON [l1.OneToMany_Optional].[Id] = [t].[OneToMany_Optional_InverseId]
WHERE [t].[Id] IS NOT NULL");
        }

        public override void SelectMany_with_navigation_filter_paging_and_explicit_DefaultIfEmpty()
        {
            base.SelectMany_with_navigation_filter_paging_and_explicit_DefaultIfEmpty();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_InverseId], [l1].[OneToMany_Required_Self_InverseId], [l1].[OneToOne_Optional_SelfId], [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_InverseId], [t].[OneToMany_Optional_Self_InverseId], [t].[OneToMany_Required_InverseId], [t].[OneToMany_Required_Self_InverseId], [t].[OneToOne_Optional_PK_InverseId], [t].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
LEFT JOIN (
    SELECT [l1.OneToMany_Required].[Id], [l1.OneToMany_Required].[Date], [l1.OneToMany_Required].[Level1_Optional_Id], [l1.OneToMany_Required].[Level1_Required_Id], [l1.OneToMany_Required].[Name], [l1.OneToMany_Required].[OneToMany_Optional_InverseId], [l1.OneToMany_Required].[OneToMany_Optional_Self_InverseId], [l1.OneToMany_Required].[OneToMany_Required_InverseId], [l1.OneToMany_Required].[OneToMany_Required_Self_InverseId], [l1.OneToMany_Required].[OneToOne_Optional_PK_InverseId], [l1.OneToMany_Required].[OneToOne_Optional_SelfId]
    FROM [Level2] AS [l1.OneToMany_Required]
    WHERE [l1.OneToMany_Required].[Id] > 5
) AS [t] ON [l1].[Id] = [t].[OneToMany_Required_InverseId]
ORDER BY [l1].[Id]");
        }

        public override void Select_join_subquery_containing_filter_and_distinct()
        {
            base.Select_join_subquery_containing_filter_and_distinct();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_InverseId], [l1].[OneToMany_Required_Self_InverseId], [l1].[OneToOne_Optional_SelfId], [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_InverseId], [t].[OneToMany_Optional_Self_InverseId], [t].[OneToMany_Required_InverseId], [t].[OneToMany_Required_Self_InverseId], [t].[OneToOne_Optional_PK_InverseId], [t].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
INNER JOIN (
    SELECT DISTINCT [l].[Id], [l].[Date], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Name], [l].[OneToMany_Optional_InverseId], [l].[OneToMany_Optional_Self_InverseId], [l].[OneToMany_Required_InverseId], [l].[OneToMany_Required_Self_InverseId], [l].[OneToOne_Optional_PK_InverseId], [l].[OneToOne_Optional_SelfId]
    FROM [Level2] AS [l]
    WHERE [l].[Id] > 2
) AS [t] ON [l1].[Id] = [t].[Level1_Optional_Id]");
        }

        public override void Select_join_with_key_selector_being_a_subquery()
        {
            base.Select_join_with_key_selector_being_a_subquery();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_InverseId], [l1].[OneToMany_Required_Self_InverseId], [l1].[OneToOne_Optional_SelfId], [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_InverseId], [l2].[OneToMany_Optional_Self_InverseId], [l2].[OneToMany_Required_InverseId], [l2].[OneToMany_Required_Self_InverseId], [l2].[OneToOne_Optional_PK_InverseId], [l2].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
INNER JOIN [Level2] AS [l2] ON [l1].[Id] = (
    SELECT TOP(1) [l0].[Id]
    FROM [Level2] AS [l0]
    ORDER BY [l0].[Id]
)");
        }

        public override void Contains_with_subquery_optional_navigation_and_constant_item()
        {
            base.Contains_with_subquery_optional_navigation_and_constant_item();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_InverseId], [l1].[OneToMany_Required_Self_InverseId], [l1].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l1.OneToOne_Optional_FK] ON [l1].[Id] = [l1.OneToOne_Optional_FK].[Level1_Optional_Id]
WHERE 1 IN (
    SELECT DISTINCT [l3].[Id]
    FROM [Level3] AS [l3]
    WHERE [l1.OneToOne_Optional_FK].[Id] = [l3].[OneToMany_Optional_InverseId]
)");
        }

        public override void Complex_query_with_optional_navigations_and_client_side_evaluation()
        {
            base.Complex_query_with_optional_navigations_and_client_side_evaluation();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_InverseId], [l1].[OneToMany_Required_Self_InverseId], [l1].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
WHERE [l1].[Id] < 3",
                //
                @"@_outer_Id='1'

SELECT [l2.OneToOne_Optional_FK.OneToOne_Optional_FK0].[Id], [l2.OneToOne_Optional_FK.OneToOne_Optional_FK0].[Id]
FROM [Level2] AS [l20]
LEFT JOIN [Level3] AS [l2.OneToOne_Optional_FK0] ON [l20].[Id] = [l2.OneToOne_Optional_FK0].[Level2_Optional_Id]
LEFT JOIN [Level4] AS [l2.OneToOne_Optional_FK.OneToOne_Optional_FK0] ON [l2.OneToOne_Optional_FK0].[Id] = [l2.OneToOne_Optional_FK.OneToOne_Optional_FK0].[Level3_Optional_Id]
WHERE @_outer_Id = [l20].[OneToMany_Optional_InverseId]",
                //
                @"@_outer_Id='2'

SELECT [l2.OneToOne_Optional_FK.OneToOne_Optional_FK0].[Id], [l2.OneToOne_Optional_FK.OneToOne_Optional_FK0].[Id]
FROM [Level2] AS [l20]
LEFT JOIN [Level3] AS [l2.OneToOne_Optional_FK0] ON [l20].[Id] = [l2.OneToOne_Optional_FK0].[Level2_Optional_Id]
LEFT JOIN [Level4] AS [l2.OneToOne_Optional_FK.OneToOne_Optional_FK0] ON [l2.OneToOne_Optional_FK0].[Id] = [l2.OneToOne_Optional_FK.OneToOne_Optional_FK0].[Level3_Optional_Id]
WHERE @_outer_Id = [l20].[OneToMany_Optional_InverseId]");
        }

        public override void Required_navigation_on_a_subquery_with_First_in_projection()
        {
            base.Required_navigation_on_a_subquery_with_First_in_projection();

            AssertSql(
                @"SELECT 1
FROM [Level2] AS [l2o]
WHERE [l2o].[Id] = 7",
                //
                @"SELECT TOP(1) [l2i.OneToOne_Required_FK_Inverse0].[Name]
FROM [Level2] AS [l2i0]
INNER JOIN [Level1] AS [l2i.OneToOne_Required_FK_Inverse0] ON [l2i0].[Level1_Required_Id] = [l2i.OneToOne_Required_FK_Inverse0].[Id]
ORDER BY [l2i0].[Id]");
        }

        public override void Required_navigation_on_a_subquery_with_complex_projection_and_First()
        {
            base.Required_navigation_on_a_subquery_with_complex_projection_and_First();

            AssertSql(
                @"SELECT 1
FROM [Level2] AS [l2o]
WHERE [l2o].[Id] = 7",
                //
                @"SELECT TOP(1) [l2i.OneToOne_Required_FK_Inverse].[Id], [l2i.OneToOne_Required_FK_Inverse].[Date], [l2i.OneToOne_Required_FK_Inverse].[Name], [l2i.OneToOne_Required_FK_Inverse].[OneToMany_Optional_Self_InverseId], [l2i.OneToOne_Required_FK_Inverse].[OneToMany_Required_Self_InverseId], [l2i.OneToOne_Required_FK_Inverse].[OneToOne_Optional_SelfId]
FROM [Level2] AS [l2i]
INNER JOIN [Level1] AS [l2i.OneToOne_Required_FK_Inverse] ON [l2i].[Level1_Required_Id] = [l2i.OneToOne_Required_FK_Inverse].[Id]
INNER JOIN [Level1] AS [l1i] ON [l2i].[Level1_Required_Id] = [l1i].[Id]
ORDER BY [l2i].[Id]");
        }

        public override void Required_navigation_on_a_subquery_with_First_in_predicate()
        {
            base.Required_navigation_on_a_subquery_with_First_in_predicate();

            AssertSql(
                @"SELECT [l2o].[Id], [l2o].[Date], [l2o].[Level1_Optional_Id], [l2o].[Level1_Required_Id], [l2o].[Name], [l2o].[OneToMany_Optional_InverseId], [l2o].[OneToMany_Optional_Self_InverseId], [l2o].[OneToMany_Required_InverseId], [l2o].[OneToMany_Required_Self_InverseId], [l2o].[OneToOne_Optional_PK_InverseId], [l2o].[OneToOne_Optional_SelfId]
FROM [Level2] AS [l2o]
WHERE [l2o].[Id] = 7",
                //
                @"SELECT TOP(1) [l2i.OneToOne_Required_FK_Inverse0].[Name]
FROM [Level2] AS [l2i0]
INNER JOIN [Level1] AS [l2i.OneToOne_Required_FK_Inverse0] ON [l2i0].[Level1_Required_Id] = [l2i.OneToOne_Required_FK_Inverse0].[Id]
ORDER BY [l2i0].[Id]");
        }

        public override void Manually_created_left_join_propagates_nullability_to_navigations()
        {
            base.Manually_created_left_join_propagates_nullability_to_navigations();

            AssertSql(
                @"SELECT [l2_manual.OneToOne_Required_FK_Inverse].[Name]
FROM [Level1] AS [l1_manual]
LEFT JOIN [Level2] AS [l2_manual] ON [l1_manual].[Id] = [l2_manual].[Level1_Optional_Id]
LEFT JOIN [Level1] AS [l2_manual.OneToOne_Required_FK_Inverse] ON [l2_manual].[Level1_Required_Id] = [l2_manual.OneToOne_Required_FK_Inverse].[Id]
WHERE ([l2_manual.OneToOne_Required_FK_Inverse].[Name] <> N'L3 02') OR [l2_manual.OneToOne_Required_FK_Inverse].[Name] IS NULL");
        }

        public override void Optional_navigation_propagates_nullability_to_manually_created_left_join1()
        {
            base.Optional_navigation_propagates_nullability_to_manually_created_left_join1();

            AssertSql(
                @"SELECT [ll.OneToOne_Optional_FK].[Id] AS [Id1], [l1].[Id] AS [Id2]
FROM [Level1] AS [ll]
LEFT JOIN [Level2] AS [ll.OneToOne_Optional_FK] ON [ll].[Id] = [ll.OneToOne_Optional_FK].[Level1_Optional_Id]
LEFT JOIN [Level2] AS [l1] ON [ll.OneToOne_Optional_FK].[Level1_Required_Id] = [l1].[Id]");
        }

        public override void Optional_navigation_propagates_nullability_to_manually_created_left_join2()
        {
            base.Optional_navigation_propagates_nullability_to_manually_created_left_join2();

            AssertSql(
                @"SELECT [l3].[Name] AS [Name1], [t].[Name] AS [Name2]
FROM [Level3] AS [l3]
LEFT JOIN (
    SELECT [ll.OneToOne_Optional_FK].*
    FROM [Level1] AS [ll]
    LEFT JOIN [Level2] AS [ll.OneToOne_Optional_FK] ON [ll].[Id] = [ll.OneToOne_Optional_FK].[Level1_Optional_Id]
) AS [t] ON [l3].[Level2_Required_Id] = [t].[Id]");
        }

        public override void Null_reference_protection_complex()
        {
            base.Null_reference_protection_complex();

            AssertSql(
                @"SELECT [t].[Name]
FROM [Level3] AS [l3]
LEFT JOIN (
    SELECT [l2_inner].*
    FROM [Level1] AS [l1_inner]
    LEFT JOIN [Level2] AS [l2_inner] ON [l1_inner].[Id] = [l2_inner].[Level1_Optional_Id]
) AS [t] ON [l3].[Level2_Required_Id] = [t].[Id]");
        }

        public override void Null_reference_protection_complex_materialization()
        {
            base.Null_reference_protection_complex_materialization();

            AssertSql(
                @"SELECT [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name] AS [property], [t].[OneToMany_Optional_InverseId], [t].[OneToMany_Optional_Self_InverseId], [t].[OneToMany_Required_InverseId], [t].[OneToMany_Required_Self_InverseId], [t].[OneToOne_Optional_PK_InverseId], [t].[OneToOne_Optional_SelfId]
FROM [Level3] AS [l3]
LEFT JOIN (
    SELECT [l2_inner].[Id], [l2_inner].[Date], [l2_inner].[Level1_Optional_Id], [l2_inner].[Level1_Required_Id], [l2_inner].[Name], [l2_inner].[OneToMany_Optional_InverseId], [l2_inner].[OneToMany_Optional_Self_InverseId], [l2_inner].[OneToMany_Required_InverseId], [l2_inner].[OneToMany_Required_Self_InverseId], [l2_inner].[OneToOne_Optional_PK_InverseId], [l2_inner].[OneToOne_Optional_SelfId]
    FROM [Level1] AS [l1_inner]
    LEFT JOIN [Level2] AS [l2_inner] ON [l1_inner].[Id] = [l2_inner].[Level1_Optional_Id]
) AS [t] ON [l3].[Level2_Required_Id] = [t].[Id]");
        }

        public override void Null_reference_protection_complex_client_eval()
        {
            base.Null_reference_protection_complex_client_eval();

            AssertSql(
                @"SELECT [t].[Name]
FROM [Level3] AS [l3]
LEFT JOIN (
    SELECT [l2_inner].*
    FROM [Level1] AS [l1_inner]
    LEFT JOIN [Level2] AS [l2_inner] ON [l1_inner].[Id] = [l2_inner].[Level1_Optional_Id]
) AS [t] ON [l3].[Level2_Required_Id] = [t].[Id]");
        }

        public override void GroupJoin_with_complex_subquery_with_joins_does_not_get_flattened()
        {
            base.GroupJoin_with_complex_subquery_with_joins_does_not_get_flattened();

            AssertSql(
                @"SELECT [t].[Id]
FROM [Level1] AS [l1_outer]
LEFT JOIN (
    SELECT [l2_inner].*
    FROM [Level2] AS [l2_inner]
    INNER JOIN [Level1] AS [l1_inner] ON [l2_inner].[Level1_Required_Id] = [l1_inner].[Id]
) AS [t] ON [l1_outer].[Id] = [t].[Level1_Optional_Id]");
        }

        public override void GroupJoin_with_complex_subquery_with_joins_does_not_get_flattened2()
        {
            base.GroupJoin_with_complex_subquery_with_joins_does_not_get_flattened2();

            AssertSql(
                @"SELECT [t].[Id]
FROM [Level1] AS [l1_outer]
LEFT JOIN (
    SELECT [l2_inner].*
    FROM [Level2] AS [l2_inner]
    INNER JOIN [Level1] AS [l1_inner] ON [l2_inner].[Level1_Required_Id] = [l1_inner].[Id]
) AS [t] ON [l1_outer].[Id] = [t].[Level1_Optional_Id]");
        }

        public override void GroupJoin_with_complex_subquery_with_joins_does_not_get_flattened3()
        {
            base.GroupJoin_with_complex_subquery_with_joins_does_not_get_flattened3();

            AssertSql(
                @"SELECT [t].[Id]
FROM [Level1] AS [l1_outer]
LEFT JOIN (
    SELECT [l2_inner].*
    FROM [Level2] AS [l2_inner]
    LEFT JOIN [Level1] AS [l1_inner] ON [l2_inner].[Level1_Required_Id] = [l1_inner].[Id]
) AS [t] ON [l1_outer].[Id] = [t].[Level1_Required_Id]");
        }

        public override void GroupJoin_with_complex_subquery_with_joins_with_reference_to_grouping1()
        {
            base.GroupJoin_with_complex_subquery_with_joins_with_reference_to_grouping1();

            AssertSql(
                @"SELECT [l1_outer].[Id], [l1_outer].[Date], [l1_outer].[Name], [l1_outer].[OneToMany_Optional_Self_InverseId], [l1_outer].[OneToMany_Required_Self_InverseId], [l1_outer].[OneToOne_Optional_SelfId], [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_InverseId], [t].[OneToMany_Optional_Self_InverseId], [t].[OneToMany_Required_InverseId], [t].[OneToMany_Required_Self_InverseId], [t].[OneToOne_Optional_PK_InverseId], [t].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1_outer]
LEFT JOIN (
    SELECT [l2_inner].[Id], [l2_inner].[Date], [l2_inner].[Level1_Optional_Id], [l2_inner].[Level1_Required_Id], [l2_inner].[Name], [l2_inner].[OneToMany_Optional_InverseId], [l2_inner].[OneToMany_Optional_Self_InverseId], [l2_inner].[OneToMany_Required_InverseId], [l2_inner].[OneToMany_Required_Self_InverseId], [l2_inner].[OneToOne_Optional_PK_InverseId], [l2_inner].[OneToOne_Optional_SelfId]
    FROM [Level2] AS [l2_inner]
    INNER JOIN [Level1] AS [l1_inner] ON [l2_inner].[Level1_Required_Id] = [l1_inner].[Id]
) AS [t] ON [l1_outer].[Id] = [t].[Level1_Optional_Id]
ORDER BY [l1_outer].[Id]");
        }

        public override void GroupJoin_with_complex_subquery_with_joins_with_reference_to_grouping2()
        {
            base.GroupJoin_with_complex_subquery_with_joins_with_reference_to_grouping2();

            AssertSql(
                @"SELECT [l1_outer].[Id], [l1_outer].[Date], [l1_outer].[Name], [l1_outer].[OneToMany_Optional_Self_InverseId], [l1_outer].[OneToMany_Required_Self_InverseId], [l1_outer].[OneToOne_Optional_SelfId], [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_InverseId], [t].[OneToMany_Optional_Self_InverseId], [t].[OneToMany_Required_InverseId], [t].[OneToMany_Required_Self_InverseId], [t].[OneToOne_Optional_PK_InverseId], [t].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1_outer]
LEFT JOIN (
    SELECT [l2_inner].[Id], [l2_inner].[Date], [l2_inner].[Level1_Optional_Id], [l2_inner].[Level1_Required_Id], [l2_inner].[Name], [l2_inner].[OneToMany_Optional_InverseId], [l2_inner].[OneToMany_Optional_Self_InverseId], [l2_inner].[OneToMany_Required_InverseId], [l2_inner].[OneToMany_Required_Self_InverseId], [l2_inner].[OneToOne_Optional_PK_InverseId], [l2_inner].[OneToOne_Optional_SelfId]
    FROM [Level2] AS [l2_inner]
    INNER JOIN [Level1] AS [l1_inner] ON [l2_inner].[Level1_Required_Id] = [l1_inner].[Id]
) AS [t] ON [l1_outer].[Id] = [t].[Level1_Optional_Id]
ORDER BY [l1_outer].[Id]");
        }

        public override void GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_outer()
        {
            base.GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_outer();

            AssertSql(
                @"");
        }

        public override void GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_outer_with_client_method()
        {
            base.GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_outer_with_client_method();

            AssertSql(
                @"");
        }

        public override void GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_inner()
        {
            base.GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_inner();

            AssertSql(
                @"");
        }

        public override void GroupJoin_on_left_side_being_a_subquery()
        {
            base.GroupJoin_on_left_side_being_a_subquery();

            AssertSql(
                @"@__p_0='2'

SELECT TOP(@__p_0) [l1].[Id], [l1.OneToOne_Optional_FK].[Name] AS [Brand]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l1.OneToOne_Optional_FK] ON [l1].[Id] = [l1.OneToOne_Optional_FK].[Level1_Optional_Id]
ORDER BY [l1.OneToOne_Optional_FK].[Name], [l1].[Id]");
        }

        public override void GroupJoin_on_right_side_being_a_subquery()
        {
            base.GroupJoin_on_right_side_being_a_subquery();

            AssertSql(
                @"@__p_0='2'

SELECT [l2].[Id], [t].[Name]
FROM [Level2] AS [l2]
LEFT JOIN (
    SELECT TOP(@__p_0) [x].*
    FROM [Level1] AS [x]
    LEFT JOIN [Level2] AS [x.OneToOne_Optional_FK] ON [x].[Id] = [x.OneToOne_Optional_FK].[Level1_Optional_Id]
    ORDER BY [x.OneToOne_Optional_FK].[Name]
) AS [t] ON [l2].[Level1_Optional_Id] = [t].[Id]");
        }

        public override void GroupJoin_in_subquery_with_client_result_operator()
        {
            base.GroupJoin_in_subquery_with_client_result_operator();

            AssertSql(
                @"SELECT [l1].[Name]
FROM [Level1] AS [l1]
WHERE ((
    SELECT COUNT(*)
    FROM (
        SELECT DISTINCT [l1_inner].*
        FROM [Level1] AS [l1_inner]
        LEFT JOIN [Level2] AS [l2_inner] ON [l1_inner].[Id] = [l2_inner].[Level1_Optional_Id]
    ) AS [t]
) > 7) AND ([l1].[Id] < 3)");
        }

        public override void GroupJoin_in_subquery_with_client_projection()
        {
            base.GroupJoin_in_subquery_with_client_projection();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Name]
FROM [Level1] AS [l1]
WHERE [l1].[Id] < 3",
                //
                @"SELECT COUNT(*)
FROM [Level1] AS [l1_inner0]
LEFT JOIN [Level2] AS [l2_inner0] ON [l1_inner0].[Id] = [l2_inner0].[Level1_Optional_Id]",
                //
                @"SELECT COUNT(*)
FROM [Level1] AS [l1_inner0]
LEFT JOIN [Level2] AS [l2_inner0] ON [l1_inner0].[Id] = [l2_inner0].[Level1_Optional_Id]");
        }

        public override void GroupJoin_in_subquery_with_client_projection_nested1()
        {
            base.GroupJoin_in_subquery_with_client_projection_nested1();

            AssertSql(
                @"SELECT [l1_outer].[Id], [l1_outer].[Name]
FROM [Level1] AS [l1_outer]
WHERE [l1_outer].[Id] < 2",
                //
                @"SELECT 1
FROM [Level1] AS [l1_middle0]
LEFT JOIN [Level2] AS [l2_middle0] ON [l1_middle0].[Id] = [l2_middle0].[Level1_Optional_Id]",
                //
                @"SELECT COUNT(*)
FROM [Level1] AS [l1_inner2]
LEFT JOIN [Level2] AS [l2_inner2] ON [l1_inner2].[Id] = [l2_inner2].[Level1_Optional_Id]",
                //
                @"SELECT COUNT(*)
FROM [Level1] AS [l1_inner2]
LEFT JOIN [Level2] AS [l2_inner2] ON [l1_inner2].[Id] = [l2_inner2].[Level1_Optional_Id]");
        }

        public override void GroupJoin_in_subquery_with_client_projection_nested2()
        {
            base.GroupJoin_in_subquery_with_client_projection_nested2();

            AssertSql(
                @"SELECT [l1_outer].[Id], [l1_outer].[Name]
FROM [Level1] AS [l1_outer]
WHERE [l1_outer].[Id] < 2",
                //
                @"SELECT COUNT(*)
FROM [Level1] AS [l1_middle0]
LEFT JOIN [Level2] AS [l2_middle0] ON [l1_middle0].[Id] = [l2_middle0].[Level1_Optional_Id]
WHERE (
    SELECT COUNT(*)
    FROM [Level1] AS [l1_inner0]
    LEFT JOIN [Level2] AS [l2_inner0] ON [l1_inner0].[Id] = [l2_inner0].[Level1_Optional_Id]
) > 7");
        }

        public override void GroupJoin_reference_to_group_in_OrderBy()
        {
            base.GroupJoin_reference_to_group_in_OrderBy();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_InverseId], [l1].[OneToMany_Required_Self_InverseId], [l1].[OneToOne_Optional_SelfId], [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_InverseId], [l2].[OneToMany_Optional_Self_InverseId], [l2].[OneToMany_Required_InverseId], [l2].[OneToMany_Required_Self_InverseId], [l2].[OneToOne_Optional_PK_InverseId], [l2].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l2] ON [l1].[Id] = [l2].[Level1_Optional_Id]
ORDER BY [l1].[Id]");
        }

        public override void GroupJoin_client_method_on_outer()
        {
            base.GroupJoin_client_method_on_outer();

            AssertSql(
                @"SELECT [l1].[Id]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l2] ON [l1].[Id] = [l2].[Level1_Optional_Id]");
        }

        public override void GroupJoin_client_method_in_OrderBy()
        {
            base.GroupJoin_client_method_in_OrderBy();

            AssertSql(
                @"SELECT [l1].[Id], [l2].[Id]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l2] ON [l1].[Id] = [l2].[Level1_Optional_Id]");
        }

        public override void GroupJoin_without_DefaultIfEmpty()
        {
            base.GroupJoin_without_DefaultIfEmpty();

            AssertSql(
                @"SELECT [l1].[Id]
FROM [Level1] AS [l1]
INNER JOIN [Level2] AS [l2] ON [l1].[Id] = [l2].[Level1_Optional_Id]");
        }

        public override void GroupJoin_with_subquery_on_inner()
        {
            base.GroupJoin_with_subquery_on_inner();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_InverseId], [l1].[OneToMany_Required_Self_InverseId], [l1].[OneToOne_Optional_SelfId], [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_InverseId], [t].[OneToMany_Optional_Self_InverseId], [t].[OneToMany_Required_InverseId], [t].[OneToMany_Required_Self_InverseId], [t].[OneToOne_Optional_PK_InverseId], [t].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_InverseId], [l2].[OneToMany_Optional_Self_InverseId], [l2].[OneToMany_Required_InverseId], [l2].[OneToMany_Required_Self_InverseId], [l2].[OneToOne_Optional_PK_InverseId], [l2].[OneToOne_Optional_SelfId]
    FROM [Level2] AS [l2]
    WHERE [l2].[Id] > 0
) AS [t] ON [l1].[Id] = [t].[Level1_Optional_Id]
ORDER BY [l1].[Id]");
        }

        public override void GroupJoin_with_subquery_on_inner_and_no_DefaultIfEmpty()
        {
            base.GroupJoin_with_subquery_on_inner_and_no_DefaultIfEmpty();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_InverseId], [l1].[OneToMany_Required_Self_InverseId], [l1].[OneToOne_Optional_SelfId], [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_InverseId], [t].[OneToMany_Optional_Self_InverseId], [t].[OneToMany_Required_InverseId], [t].[OneToMany_Required_Self_InverseId], [t].[OneToOne_Optional_PK_InverseId], [t].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_InverseId], [l2].[OneToMany_Optional_Self_InverseId], [l2].[OneToMany_Required_InverseId], [l2].[OneToMany_Required_Self_InverseId], [l2].[OneToOne_Optional_PK_InverseId], [l2].[OneToOne_Optional_SelfId]
    FROM [Level2] AS [l2]
    WHERE [l2].[Id] > 0
) AS [t] ON [l1].[Id] = [t].[Level1_Optional_Id]
ORDER BY [l1].[Id]");
        }

        public override void Optional_navigation_in_subquery_with_unrelated_projection()
        {
            base.Optional_navigation_in_subquery_with_unrelated_projection();

            AssertSql(
                @"@__p_0='15'

SELECT TOP(@__p_0) [l1].[Id]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l1.OneToOne_Optional_FK] ON [l1].[Id] = [l1.OneToOne_Optional_FK].[Level1_Optional_Id]
WHERE ([l1.OneToOne_Optional_FK].[Name] <> N'Foo') OR [l1.OneToOne_Optional_FK].[Name] IS NULL");
        }

        public override void Explicit_GroupJoin_in_subquery_with_unrelated_projection()
        {
            base.Explicit_GroupJoin_in_subquery_with_unrelated_projection();

            AssertSql(
                @"@__p_0='15'

SELECT TOP(@__p_0) [l1].[Id]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l2] ON [l1].[Id] = [l2].[Level1_Optional_Id]
WHERE ([l2].[Name] <> N'Foo') OR [l2].[Name] IS NULL");
        }

        public override void Explicit_GroupJoin_in_subquery_with_unrelated_projection2()
        {
            base.Explicit_GroupJoin_in_subquery_with_unrelated_projection2();

            AssertSql(
                @"SELECT [t].[Id]
FROM (
    SELECT DISTINCT [l1].*
    FROM [Level1] AS [l1]
    LEFT JOIN [Level2] AS [l2] ON [l1].[Id] = [l2].[Level1_Optional_Id]
    WHERE ([l2].[Name] <> N'Foo') OR [l2].[Name] IS NULL
) AS [t]");
        }

        public override void Explicit_GroupJoin_in_subquery_with_unrelated_projection3()
        {
            base.Explicit_GroupJoin_in_subquery_with_unrelated_projection3();

            AssertSql(
                @"SELECT DISTINCT [l1].[Id]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l2] ON [l1].[Id] = [l2].[Level1_Optional_Id]
WHERE ([l2].[Name] <> N'Foo') OR [l2].[Name] IS NULL");
        }

        public override void Explicit_GroupJoin_in_subquery_with_unrelated_projection4()
        {
            base.Explicit_GroupJoin_in_subquery_with_unrelated_projection4();

            AssertSql(
                @"@__p_0='20'

SELECT DISTINCT TOP(@__p_0) [l1].[Id]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l2] ON [l1].[Id] = [l2].[Level1_Optional_Id]
WHERE ([l2].[Name] <> N'Foo') OR [l2].[Name] IS NULL");
        }

        public override void Explicit_GroupJoin_in_subquery_with_scalar_result_operator()
        {
            base.Explicit_GroupJoin_in_subquery_with_scalar_result_operator();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_InverseId], [l1].[OneToMany_Required_Self_InverseId], [l1].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
WHERE (
    SELECT COUNT(*)
    FROM [Level1] AS [l1_inner]
    LEFT JOIN [Level2] AS [l2] ON [l1_inner].[Id] = [l2].[Level1_Optional_Id]
) > 4");
        }

        public override void Explicit_GroupJoin_in_subquery_with_multiple_result_operator_distinct_count_materializes_main_clause()
        {
            base.Explicit_GroupJoin_in_subquery_with_multiple_result_operator_distinct_count_materializes_main_clause();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_InverseId], [l1].[OneToMany_Required_Self_InverseId], [l1].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT DISTINCT [l1_inner].*
        FROM [Level1] AS [l1_inner]
        LEFT JOIN [Level2] AS [l2] ON [l1_inner].[Id] = [l2].[Level1_Optional_Id]
    ) AS [t]
) > 4");
        }

        public override void Where_on_multilevel_reference_in_subquery_with_outer_projection()
        {
            base.Where_on_multilevel_reference_in_subquery_with_outer_projection();

            AssertSql(
                @"@__p_0='0'
@__p_1='10'

SELECT [l3].[Name]
FROM [Level3] AS [l3]
INNER JOIN [Level2] AS [l3.OneToMany_Required_Inverse] ON [l3].[OneToMany_Required_InverseId] = [l3.OneToMany_Required_Inverse].[Id]
INNER JOIN [Level1] AS [l3.OneToMany_Required_Inverse.OneToOne_Required_FK_Inverse] ON [l3.OneToMany_Required_Inverse].[Level1_Required_Id] = [l3.OneToMany_Required_Inverse.OneToOne_Required_FK_Inverse].[Id]
WHERE [l3.OneToMany_Required_Inverse.OneToOne_Required_FK_Inverse].[Name] = N'L1 03'
ORDER BY [l3].[Level2_Required_Id]
OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY");
        }

        public override void Join_condition_optimizations_applied_correctly_when_anonymous_type_with_single_property()
        {
            base.Join_condition_optimizations_applied_correctly_when_anonymous_type_with_single_property();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_InverseId], [l1].[OneToMany_Required_Self_InverseId], [l1].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
INNER JOIN [Level2] AS [l2] ON ([l1].[OneToMany_Optional_Self_InverseId] = [l2].[Level1_Optional_Id]) OR ([l1].[OneToMany_Optional_Self_InverseId] IS NULL AND [l2].[Level1_Optional_Id] IS NULL)");
        }

        public override void Join_condition_optimizations_applied_correctly_when_anonymous_type_with_multiple_properties()
        {
            base.Join_condition_optimizations_applied_correctly_when_anonymous_type_with_multiple_properties();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_InverseId], [l1].[OneToMany_Required_Self_InverseId], [l1].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
INNER JOIN [Level2] AS [l2] ON (([l1].[OneToMany_Optional_Self_InverseId] = [l2].[Level1_Optional_Id]) OR ([l1].[OneToMany_Optional_Self_InverseId] IS NULL AND [l2].[Level1_Optional_Id] IS NULL)) AND (([l1].[OneToOne_Optional_SelfId] = [l2].[OneToMany_Optional_Self_InverseId]) OR ([l1].[OneToOne_Optional_SelfId] IS NULL AND [l2].[OneToMany_Optional_Self_InverseId] IS NULL))");
        }

        public override void Navigation_filter_navigation_grouping_ordering_by_group_key()
        {
            base.Navigation_filter_navigation_grouping_ordering_by_group_key();

            AssertSql(
                @"@__level1Id_0='1'

SELECT [l20].[Id], [l20].[Date], [l20].[Level1_Optional_Id], [l20].[Level1_Required_Id], [l20].[Name], [l20].[OneToMany_Optional_InverseId], [l20].[OneToMany_Optional_Self_InverseId], [l20].[OneToMany_Required_InverseId], [l20].[OneToMany_Required_Self_InverseId], [l20].[OneToOne_Optional_PK_InverseId], [l20].[OneToOne_Optional_SelfId], [l2.OneToMany_Required_Self_Inverse0].[Name]
FROM [Level2] AS [l20]
INNER JOIN [Level2] AS [l2.OneToMany_Required_Self_Inverse0] ON [l20].[OneToMany_Required_Self_InverseId] = [l2.OneToMany_Required_Self_Inverse0].[Id]
WHERE [l20].[OneToMany_Required_InverseId] = @__level1Id_0
ORDER BY [l2.OneToMany_Required_Self_Inverse0].[Name]");
        }

        public override void Nested_group_join_with_take()
        {
            base.Nested_group_join_with_take();

            AssertSql(
                @"@__p_0='2'

SELECT [l2_outer].[Name]
FROM (
    SELECT TOP(@__p_0) [l2_inner].*
    FROM [Level1] AS [l1_inner]
    LEFT JOIN [Level2] AS [l2_inner] ON [l1_inner].[Id] = [l2_inner].[Level1_Optional_Id]
) AS [t]
LEFT JOIN [Level2] AS [l2_outer] ON [t].[Id] = [l2_outer].[Level1_Optional_Id]");
        }

        public override void Navigation_with_same_navigation_compared_to_null()
        {
            base.Navigation_with_same_navigation_compared_to_null();

            AssertSql(
                @"SELECT [l2].[Id]
FROM [Level2] AS [l2]
INNER JOIN [Level1] AS [l2.OneToMany_Required_Inverse] ON [l2].[OneToMany_Required_InverseId] = [l2.OneToMany_Required_Inverse].[Id]
WHERE (([l2.OneToMany_Required_Inverse].[Name] <> N'L1 07') OR [l2.OneToMany_Required_Inverse].[Name] IS NULL) AND [l2].[OneToMany_Required_InverseId] IS NOT NULL");
        }

        public override void Multi_level_navigation_compared_to_null()
        {
            base.Multi_level_navigation_compared_to_null();

            AssertSql(
                @"SELECT [l3].[Id]
FROM [Level3] AS [l3]
LEFT JOIN [Level2] AS [l3.OneToMany_Optional_Inverse] ON [l3].[OneToMany_Optional_InverseId] = [l3.OneToMany_Optional_Inverse].[Id]
WHERE [l3.OneToMany_Optional_Inverse].[Level1_Required_Id] IS NOT NULL");
        }

        public override void Multi_level_navigation_with_same_navigation_compared_to_null()
        {
            base.Multi_level_navigation_with_same_navigation_compared_to_null();

            AssertSql(
                @"SELECT [l3].[Id]
FROM [Level3] AS [l3]
LEFT JOIN [Level2] AS [l3.OneToMany_Optional_Inverse] ON [l3].[OneToMany_Optional_InverseId] = [l3.OneToMany_Optional_Inverse].[Id]
LEFT JOIN [Level1] AS [l3.OneToMany_Optional_Inverse.OneToOne_Required_FK_Inverse] ON [l3.OneToMany_Optional_Inverse].[Level1_Required_Id] = [l3.OneToMany_Optional_Inverse.OneToOne_Required_FK_Inverse].[Id]
WHERE (([l3.OneToMany_Optional_Inverse.OneToOne_Required_FK_Inverse].[Name] <> N'L1 07') OR [l3.OneToMany_Optional_Inverse.OneToOne_Required_FK_Inverse].[Name] IS NULL) AND [l3.OneToMany_Optional_Inverse].[Level1_Required_Id] IS NOT NULL");
        }

        public override void Navigations_compared_to_each_other1()
        {
            base.Navigations_compared_to_each_other1();

            AssertSql(
                @"SELECT [l2].[Name]
FROM [Level2] AS [l2]
WHERE [l2].[OneToMany_Required_InverseId] = [l2].[OneToMany_Required_InverseId]");
        }

        public override void Navigations_compared_to_each_other2()
        {
            base.Navigations_compared_to_each_other2();

            AssertSql(
                @"SELECT [l2].[Name]
FROM [Level2] AS [l2]
WHERE [l2].[OneToMany_Required_InverseId] = [l2].[OneToOne_Optional_PK_InverseId]");
        }

        public override void Navigations_compared_to_each_other3()
        {
            base.Navigations_compared_to_each_other3();

            AssertSql(
                @"SELECT [l2].[Name]
FROM [Level2] AS [l2]
WHERE EXISTS (
    SELECT 1
    FROM [Level3] AS [i]
    WHERE [l2].[Id] = [i].[OneToMany_Optional_InverseId])");
        }

        public override void Navigations_compared_to_each_other4()
        {
            base.Navigations_compared_to_each_other4();

            AssertSql(
                @"SELECT [l2].[Name]
FROM [Level2] AS [l2]
LEFT JOIN [Level3] AS [l2.OneToOne_Required_FK] ON [l2].[Id] = [l2.OneToOne_Required_FK].[Level2_Required_Id]
WHERE EXISTS (
    SELECT 1
    FROM [Level4] AS [i]
    WHERE [l2.OneToOne_Required_FK].[Id] = [i].[OneToMany_Optional_InverseId])");
        }

        public override void Navigations_compared_to_each_other5()
        {
            base.Navigations_compared_to_each_other5();

            AssertSql(
                @"SELECT [l2].[Name]
FROM [Level2] AS [l2]
LEFT JOIN [Level3] AS [l2.OneToOne_Optional_PK] ON [l2].[Id] = [l2.OneToOne_Optional_PK].[OneToOne_Optional_PK_InverseId]
LEFT JOIN [Level3] AS [l2.OneToOne_Required_FK] ON [l2].[Id] = [l2.OneToOne_Required_FK].[Level2_Required_Id]
WHERE EXISTS (
    SELECT 1
    FROM [Level4] AS [i]
    WHERE [l2.OneToOne_Required_FK].[Id] = [i].[OneToMany_Optional_InverseId])");
        }

        public override void Level4_Include()
        {
            base.Level4_Include();

            AssertSql(
                @"SELECT [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Id], [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Date], [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Level1_Optional_Id], [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Level1_Required_Id], [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Name], [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[OneToMany_Optional_InverseId], [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[OneToMany_Optional_Self_InverseId], [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[OneToMany_Required_InverseId], [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[OneToMany_Required_Self_InverseId], [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[OneToOne_Optional_PK_InverseId], [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[OneToOne_Optional_SelfId], [OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[Id], [OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[Level2_Optional_Id], [OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[Level2_Required_Id], [OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[Name], [OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[OneToMany_Optional_InverseId], [OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[OneToMany_Optional_Self_InverseId], [OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[OneToMany_Required_InverseId], [OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[OneToMany_Required_Self_InverseId], [OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[OneToOne_Optional_PK_InverseId], [OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l1.OneToOne_Required_PK] ON [l1].[Id] = [l1.OneToOne_Required_PK].[Id]
LEFT JOIN [Level3] AS [l1.OneToOne_Required_PK.OneToOne_Required_PK] ON [l1.OneToOne_Required_PK].[Id] = [l1.OneToOne_Required_PK.OneToOne_Required_PK].[Id]
LEFT JOIN [Level4] AS [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK] ON [l1.OneToOne_Required_PK.OneToOne_Required_PK].[Id] = [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK].[Id]
LEFT JOIN [Level3] AS [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse] ON [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK].[Level3_Required_Id] = [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse].[Id]
LEFT JOIN [Level2] AS [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse] ON [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse].[Level2_Required_Id] = [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Id]
LEFT JOIN [Level3] AS [OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK] ON [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Id] = [OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[Level2_Optional_Id]
WHERE ([l1.OneToOne_Required_PK].[Id] IS NOT NULL AND [l1.OneToOne_Required_PK.OneToOne_Required_PK].[Id] IS NOT NULL) AND [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK].[Id] IS NOT NULL");
        }

        public override void Comparing_collection_navigation_on_optional_reference_to_null()
        {
            base.Comparing_collection_navigation_on_optional_reference_to_null();

            AssertSql(
                @"SELECT [l1].[Id]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l1.OneToOne_Optional_FK] ON [l1].[Id] = [l1.OneToOne_Optional_FK].[Level1_Optional_Id]
WHERE [l1.OneToOne_Optional_FK].[Id] IS NULL");
        }

        public override void Select_subquery_with_client_eval_and_navigation1()
        {
            base.Select_subquery_with_client_eval_and_navigation1();

            AssertContainsSql(
                @"SELECT 1
FROM [Level2] AS [l2]",
                //
                @"SELECT TOP(1) [l.OneToOne_Required_FK_Inverse0].[Name]
FROM [Level2] AS [l0]
INNER JOIN [Level1] AS [l.OneToOne_Required_FK_Inverse0] ON [l0].[Level1_Required_Id] = [l.OneToOne_Required_FK_Inverse0].[Id]
ORDER BY [l0].[Id]");
        }

        public override void Select_subquery_with_client_eval_and_navigation2()
        {
            base.Select_subquery_with_client_eval_and_navigation2();

            AssertContainsSql(
                @"SELECT 1
FROM [Level2] AS [l2]",
                //
                @"SELECT TOP(1) [l.OneToOne_Required_FK_Inverse1].[Name]
FROM [Level2] AS [l1]
INNER JOIN [Level1] AS [l.OneToOne_Required_FK_Inverse1] ON [l1].[Level1_Required_Id] = [l.OneToOne_Required_FK_Inverse1].[Id]
ORDER BY [l1].[Id]");
        }

        public override void Select_subquery_with_client_eval_and_multi_level_navigation()
        {
            base.Select_subquery_with_client_eval_and_multi_level_navigation();

            AssertSql(
                @"");
        }

        public override void Member_doesnt_get_pushed_down_into_subquery_with_result_operator()
        {
            base.Member_doesnt_get_pushed_down_into_subquery_with_result_operator();

            AssertSql(
                @"SELECT (
    SELECT [t].[Name]
    FROM (
        SELECT DISTINCT [l3].*
        FROM [Level3] AS [l3]
    ) AS [t]
    ORDER BY [t].[Id]
    OFFSET 1 ROWS FETCH NEXT 1 ROWS ONLY
)
FROM [Level1] AS [l1]
WHERE [l1].[Id] < 3");
        }

        public override void Subquery_with_Distinct_Skip_FirstOrDefault_without_OrderBy()
        {
            base.Subquery_with_Distinct_Skip_FirstOrDefault_without_OrderBy();

            AssertSql(
                @"");
        }

        public override void Project_collection_navigation()
        {
            base.Project_collection_navigation();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_InverseId], [l1].[OneToMany_Required_Self_InverseId], [l1].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
ORDER BY [l1].[Id]",
                //
                @"SELECT [l1.OneToMany_Optional].[Id], [l1.OneToMany_Optional].[Date], [l1.OneToMany_Optional].[Level1_Optional_Id], [l1.OneToMany_Optional].[Level1_Required_Id], [l1.OneToMany_Optional].[Name], [l1.OneToMany_Optional].[OneToMany_Optional_InverseId], [l1.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [l1.OneToMany_Optional].[OneToMany_Required_InverseId], [l1.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [l1.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [l1.OneToMany_Optional].[OneToOne_Optional_SelfId]
FROM [Level2] AS [l1.OneToMany_Optional]
INNER JOIN (
    SELECT [l10].[Id]
    FROM [Level1] AS [l10]
) AS [t] ON [l1.OneToMany_Optional].[OneToMany_Optional_InverseId] = [t].[Id]
ORDER BY [t].[Id]");
        }

        public override void Project_collection_navigation_nested()
        {
            base.Project_collection_navigation_nested();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_InverseId], [l1].[OneToMany_Required_Self_InverseId], [l1].[OneToOne_Optional_SelfId], [l1.OneToOne_Optional_FK].[Id], [l1.OneToOne_Optional_FK].[Date], [l1.OneToOne_Optional_FK].[Level1_Optional_Id], [l1.OneToOne_Optional_FK].[Level1_Required_Id], [l1.OneToOne_Optional_FK].[Name], [l1.OneToOne_Optional_FK].[OneToMany_Optional_InverseId], [l1.OneToOne_Optional_FK].[OneToMany_Optional_Self_InverseId], [l1.OneToOne_Optional_FK].[OneToMany_Required_InverseId], [l1.OneToOne_Optional_FK].[OneToMany_Required_Self_InverseId], [l1.OneToOne_Optional_FK].[OneToOne_Optional_PK_InverseId], [l1.OneToOne_Optional_FK].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l1.OneToOne_Optional_FK] ON [l1].[Id] = [l1.OneToOne_Optional_FK].[Level1_Optional_Id]
ORDER BY [l1.OneToOne_Optional_FK].[Id]",
                //
                @"SELECT [l1.OneToOne_Optional_FK.OneToMany_Optional].[Id], [l1.OneToOne_Optional_FK.OneToMany_Optional].[Level2_Optional_Id], [l1.OneToOne_Optional_FK.OneToMany_Optional].[Level2_Required_Id], [l1.OneToOne_Optional_FK.OneToMany_Optional].[Name], [l1.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Optional_InverseId], [l1.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [l1.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Required_InverseId], [l1.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [l1.OneToOne_Optional_FK.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [l1.OneToOne_Optional_FK.OneToMany_Optional].[OneToOne_Optional_SelfId]
FROM [Level3] AS [l1.OneToOne_Optional_FK.OneToMany_Optional]
INNER JOIN (
    SELECT DISTINCT [l1.OneToOne_Optional_FK0].[Id]
    FROM [Level1] AS [l10]
    LEFT JOIN [Level2] AS [l1.OneToOne_Optional_FK0] ON [l10].[Id] = [l1.OneToOne_Optional_FK0].[Level1_Optional_Id]
) AS [t] ON [l1.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Optional_InverseId] = [t].[Id]
ORDER BY [t].[Id]");
        }

        public override void Project_collection_navigation_using_ef_property()
        {
            base.Project_collection_navigation_using_ef_property();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_InverseId], [l1].[OneToMany_Required_Self_InverseId], [l1].[OneToOne_Optional_SelfId], [l1.OneToOne_Optional_FK].[Id], [l1.OneToOne_Optional_FK].[Date], [l1.OneToOne_Optional_FK].[Level1_Optional_Id], [l1.OneToOne_Optional_FK].[Level1_Required_Id], [l1.OneToOne_Optional_FK].[Name], [l1.OneToOne_Optional_FK].[OneToMany_Optional_InverseId], [l1.OneToOne_Optional_FK].[OneToMany_Optional_Self_InverseId], [l1.OneToOne_Optional_FK].[OneToMany_Required_InverseId], [l1.OneToOne_Optional_FK].[OneToMany_Required_Self_InverseId], [l1.OneToOne_Optional_FK].[OneToOne_Optional_PK_InverseId], [l1.OneToOne_Optional_FK].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l1.OneToOne_Optional_FK] ON [l1].[Id] = [l1.OneToOne_Optional_FK].[Level1_Optional_Id]
ORDER BY [l1.OneToOne_Optional_FK].[Id]",
                //
                @"SELECT [l1.OneToOne_Optional_FK.OneToMany_Optional].[Id], [l1.OneToOne_Optional_FK.OneToMany_Optional].[Level2_Optional_Id], [l1.OneToOne_Optional_FK.OneToMany_Optional].[Level2_Required_Id], [l1.OneToOne_Optional_FK.OneToMany_Optional].[Name], [l1.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Optional_InverseId], [l1.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [l1.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Required_InverseId], [l1.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [l1.OneToOne_Optional_FK.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [l1.OneToOne_Optional_FK.OneToMany_Optional].[OneToOne_Optional_SelfId]
FROM [Level3] AS [l1.OneToOne_Optional_FK.OneToMany_Optional]
INNER JOIN (
    SELECT DISTINCT [l1.OneToOne_Optional_FK0].[Id]
    FROM [Level1] AS [l10]
    LEFT JOIN [Level2] AS [l1.OneToOne_Optional_FK0] ON [l10].[Id] = [l1.OneToOne_Optional_FK0].[Level1_Optional_Id]
) AS [t] ON [l1.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Optional_InverseId] = [t].[Id]
ORDER BY [t].[Id]");
        }

        public override void Project_collection_navigation_nested_anonymous()
        {
            base.Project_collection_navigation_nested_anonymous();

            AssertSql(
                @"SELECT [l1].[Id] AS [Id0], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_InverseId], [l1].[OneToMany_Required_Self_InverseId], [l1].[OneToOne_Optional_SelfId], [l1.OneToOne_Optional_FK].[Id], [l1.OneToOne_Optional_FK].[Date], [l1.OneToOne_Optional_FK].[Level1_Optional_Id], [l1.OneToOne_Optional_FK].[Level1_Required_Id], [l1.OneToOne_Optional_FK].[Name], [l1.OneToOne_Optional_FK].[OneToMany_Optional_InverseId], [l1.OneToOne_Optional_FK].[OneToMany_Optional_Self_InverseId], [l1.OneToOne_Optional_FK].[OneToMany_Required_InverseId], [l1.OneToOne_Optional_FK].[OneToMany_Required_Self_InverseId], [l1.OneToOne_Optional_FK].[OneToOne_Optional_PK_InverseId], [l1.OneToOne_Optional_FK].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l1.OneToOne_Optional_FK] ON [l1].[Id] = [l1.OneToOne_Optional_FK].[Level1_Optional_Id]
ORDER BY [l1.OneToOne_Optional_FK].[Id]",
                //
                @"SELECT [l1.OneToOne_Optional_FK.OneToMany_Optional].[Id], [l1.OneToOne_Optional_FK.OneToMany_Optional].[Level2_Optional_Id], [l1.OneToOne_Optional_FK.OneToMany_Optional].[Level2_Required_Id], [l1.OneToOne_Optional_FK.OneToMany_Optional].[Name], [l1.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Optional_InverseId], [l1.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [l1.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Required_InverseId], [l1.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [l1.OneToOne_Optional_FK.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [l1.OneToOne_Optional_FK.OneToMany_Optional].[OneToOne_Optional_SelfId]
FROM [Level3] AS [l1.OneToOne_Optional_FK.OneToMany_Optional]
INNER JOIN (
    SELECT DISTINCT [l1.OneToOne_Optional_FK0].[Id]
    FROM [Level1] AS [l10]
    LEFT JOIN [Level2] AS [l1.OneToOne_Optional_FK0] ON [l10].[Id] = [l1.OneToOne_Optional_FK0].[Level1_Optional_Id]
) AS [t] ON [l1.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Optional_InverseId] = [t].[Id]
ORDER BY [t].[Id]");
        }

        public override void Project_collection_navigation_count()
        {
            base.Project_collection_navigation_count();

            AssertSql(
                @"SELECT [l1].[Id], (
    SELECT COUNT(*)
    FROM [Level3] AS [l]
    WHERE [l1.OneToOne_Optional_FK].[Id] = [l].[OneToMany_Optional_InverseId]
) AS [Count]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l1.OneToOne_Optional_FK] ON [l1].[Id] = [l1.OneToOne_Optional_FK].[Level1_Optional_Id]");
        }

        public override void Project_collection_navigation_composed()
        {
            base.Project_collection_navigation_composed();

            AssertSql(
                @"SELECT [l1].[Id]
FROM [Level1] AS [l1]
WHERE [l1].[Id] < 3",
                //
                @"@_outer_Id='1'

SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_InverseId], [l2].[OneToMany_Optional_Self_InverseId], [l2].[OneToMany_Required_InverseId], [l2].[OneToMany_Required_Self_InverseId], [l2].[OneToOne_Optional_PK_InverseId], [l2].[OneToOne_Optional_SelfId]
FROM [Level2] AS [l2]
WHERE (([l2].[Name] <> N'Foo') OR [l2].[Name] IS NULL) AND (@_outer_Id = [l2].[OneToMany_Optional_InverseId])",
                //
                @"@_outer_Id='2'

SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_InverseId], [l2].[OneToMany_Optional_Self_InverseId], [l2].[OneToMany_Required_InverseId], [l2].[OneToMany_Required_Self_InverseId], [l2].[OneToOne_Optional_PK_InverseId], [l2].[OneToOne_Optional_SelfId]
FROM [Level2] AS [l2]
WHERE (([l2].[Name] <> N'Foo') OR [l2].[Name] IS NULL) AND (@_outer_Id = [l2].[OneToMany_Optional_InverseId])");
        }

        public override void Project_collection_and_root_entity()
        {
            base.Project_collection_and_root_entity();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_InverseId], [l1].[OneToMany_Required_Self_InverseId], [l1].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
ORDER BY [l1].[Id]",
                //
                @"SELECT [l1.OneToMany_Optional].[Id], [l1.OneToMany_Optional].[Date], [l1.OneToMany_Optional].[Level1_Optional_Id], [l1.OneToMany_Optional].[Level1_Required_Id], [l1.OneToMany_Optional].[Name], [l1.OneToMany_Optional].[OneToMany_Optional_InverseId], [l1.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [l1.OneToMany_Optional].[OneToMany_Required_InverseId], [l1.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [l1.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [l1.OneToMany_Optional].[OneToOne_Optional_SelfId]
FROM [Level2] AS [l1.OneToMany_Optional]
INNER JOIN (
    SELECT [l10].[Id]
    FROM [Level1] AS [l10]
) AS [t] ON [l1.OneToMany_Optional].[OneToMany_Optional_InverseId] = [t].[Id]
ORDER BY [t].[Id]");
        }

        public override void Project_collection_and_include()
        {
            base.Project_collection_and_include();

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_InverseId], [l].[OneToMany_Required_Self_InverseId], [l].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l]
ORDER BY [l].[Id]",
                //
                @"SELECT [l.OneToMany_Optional].[Id], [l.OneToMany_Optional].[Date], [l.OneToMany_Optional].[Level1_Optional_Id], [l.OneToMany_Optional].[Level1_Required_Id], [l.OneToMany_Optional].[Name], [l.OneToMany_Optional].[OneToMany_Optional_InverseId], [l.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [l.OneToMany_Optional].[OneToMany_Required_InverseId], [l.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [l.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [l.OneToMany_Optional].[OneToOne_Optional_SelfId]
FROM [Level2] AS [l.OneToMany_Optional]
INNER JOIN (
    SELECT [l0].[Id]
    FROM [Level1] AS [l0]
) AS [t] ON [l.OneToMany_Optional].[OneToMany_Optional_InverseId] = [t].[Id]
ORDER BY [t].[Id]");
        }

        public override void Project_navigation_and_collection()
        {
            base.Project_navigation_and_collection();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_InverseId], [l1].[OneToMany_Required_Self_InverseId], [l1].[OneToOne_Optional_SelfId], [l1.OneToOne_Optional_FK].[Id], [l1.OneToOne_Optional_FK].[Date], [l1.OneToOne_Optional_FK].[Level1_Optional_Id], [l1.OneToOne_Optional_FK].[Level1_Required_Id], [l1.OneToOne_Optional_FK].[Name], [l1.OneToOne_Optional_FK].[OneToMany_Optional_InverseId], [l1.OneToOne_Optional_FK].[OneToMany_Optional_Self_InverseId], [l1.OneToOne_Optional_FK].[OneToMany_Required_InverseId], [l1.OneToOne_Optional_FK].[OneToMany_Required_Self_InverseId], [l1.OneToOne_Optional_FK].[OneToOne_Optional_PK_InverseId], [l1.OneToOne_Optional_FK].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l1.OneToOne_Optional_FK] ON [l1].[Id] = [l1.OneToOne_Optional_FK].[Level1_Optional_Id]
ORDER BY [l1.OneToOne_Optional_FK].[Id]",
                //
                @"SELECT [l1.OneToOne_Optional_FK.OneToMany_Optional].[Id], [l1.OneToOne_Optional_FK.OneToMany_Optional].[Level2_Optional_Id], [l1.OneToOne_Optional_FK.OneToMany_Optional].[Level2_Required_Id], [l1.OneToOne_Optional_FK.OneToMany_Optional].[Name], [l1.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Optional_InverseId], [l1.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [l1.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Required_InverseId], [l1.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [l1.OneToOne_Optional_FK.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [l1.OneToOne_Optional_FK.OneToMany_Optional].[OneToOne_Optional_SelfId]
FROM [Level3] AS [l1.OneToOne_Optional_FK.OneToMany_Optional]
INNER JOIN (
    SELECT DISTINCT [l1.OneToOne_Optional_FK0].[Id]
    FROM [Level1] AS [l10]
    LEFT JOIN [Level2] AS [l1.OneToOne_Optional_FK0] ON [l10].[Id] = [l1.OneToOne_Optional_FK0].[Level1_Optional_Id]
) AS [t] ON [l1.OneToOne_Optional_FK.OneToMany_Optional].[OneToMany_Optional_InverseId] = [t].[Id]
ORDER BY [t].[Id]");
        }

        public override void Include_inside_subquery()
        {
            base.Include_inside_subquery();

            AssertSql(
                @"");
        }

        public override void Select_optional_navigation_property_string_concat()
        {
            base.Select_optional_navigation_property_string_concat();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_InverseId], [l1].[OneToMany_Required_Self_InverseId], [l1].[OneToOne_Optional_SelfId], [l1.OneToMany_Optional].[Id], [l1.OneToMany_Optional].[Date], [l1.OneToMany_Optional].[Level1_Optional_Id], [l1.OneToMany_Optional].[Level1_Required_Id], [l1.OneToMany_Optional].[Name], [l1.OneToMany_Optional].[OneToMany_Optional_InverseId], [l1.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [l1.OneToMany_Optional].[OneToMany_Required_InverseId], [l1.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [l1.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [l1.OneToMany_Optional].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l1]
LEFT JOIN [Level2] AS [l1.OneToMany_Optional] ON [l1].[Id] = [l1.OneToMany_Optional].[OneToMany_Optional_InverseId]
ORDER BY [l1].[Id]");
        }

        public override void Include_collection_with_multiple_orderbys_member()
        {
            base.Include_collection_with_multiple_orderbys_member();

            AssertSql(
                @"SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_InverseId], [l2].[OneToMany_Optional_Self_InverseId], [l2].[OneToMany_Required_InverseId], [l2].[OneToMany_Required_Self_InverseId], [l2].[OneToOne_Optional_PK_InverseId], [l2].[OneToOne_Optional_SelfId]
FROM [Level2] AS [l2]
ORDER BY [l2].[Name], [l2].[Level1_Required_Id], [l2].[Id]",
                //
                @"SELECT [l2.OneToMany_Optional].[Id], [l2.OneToMany_Optional].[Level2_Optional_Id], [l2.OneToMany_Optional].[Level2_Required_Id], [l2.OneToMany_Optional].[Name], [l2.OneToMany_Optional].[OneToMany_Optional_InverseId], [l2.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [l2.OneToMany_Optional].[OneToMany_Required_InverseId], [l2.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [l2.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [l2.OneToMany_Optional].[OneToOne_Optional_SelfId]
FROM [Level3] AS [l2.OneToMany_Optional]
INNER JOIN (
    SELECT [l20].[Id], [l20].[Name], [l20].[Level1_Required_Id]
    FROM [Level2] AS [l20]
) AS [t] ON [l2.OneToMany_Optional].[OneToMany_Optional_InverseId] = [t].[Id]
ORDER BY [t].[Name], [t].[Level1_Required_Id], [t].[Id]");
        }

        public override void Include_collection_with_multiple_orderbys_property()
        {
            base.Include_collection_with_multiple_orderbys_property();

            AssertSql(
                @"SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_InverseId], [l2].[OneToMany_Optional_Self_InverseId], [l2].[OneToMany_Required_InverseId], [l2].[OneToMany_Required_Self_InverseId], [l2].[OneToOne_Optional_PK_InverseId], [l2].[OneToOne_Optional_SelfId]
FROM [Level2] AS [l2]
ORDER BY [l2].[Level1_Required_Id], [l2].[Name], [l2].[Id]",
                //
                @"SELECT [l2.OneToMany_Optional].[Id], [l2.OneToMany_Optional].[Level2_Optional_Id], [l2.OneToMany_Optional].[Level2_Required_Id], [l2.OneToMany_Optional].[Name], [l2.OneToMany_Optional].[OneToMany_Optional_InverseId], [l2.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [l2.OneToMany_Optional].[OneToMany_Required_InverseId], [l2.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [l2.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [l2.OneToMany_Optional].[OneToOne_Optional_SelfId]
FROM [Level3] AS [l2.OneToMany_Optional]
INNER JOIN (
    SELECT [l20].[Id], [l20].[Level1_Required_Id], [l20].[Name]
    FROM [Level2] AS [l20]
) AS [t] ON [l2.OneToMany_Optional].[OneToMany_Optional_InverseId] = [t].[Id]
ORDER BY [t].[Level1_Required_Id], [t].[Name], [t].[Id]");
        }

        public override void Include_collection_with_multiple_orderbys_methodcall()
        {
            base.Include_collection_with_multiple_orderbys_methodcall();

            AssertSql(
                @"SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_InverseId], [l2].[OneToMany_Optional_Self_InverseId], [l2].[OneToMany_Required_InverseId], [l2].[OneToMany_Required_Self_InverseId], [l2].[OneToOne_Optional_PK_InverseId], [l2].[OneToOne_Optional_SelfId]
FROM [Level2] AS [l2]
ORDER BY ABS([l2].[Level1_Required_Id]), [l2].[Name], [l2].[Id]",
                //
                @"SELECT [l2.OneToMany_Optional].[Id], [l2.OneToMany_Optional].[Level2_Optional_Id], [l2.OneToMany_Optional].[Level2_Required_Id], [l2.OneToMany_Optional].[Name], [l2.OneToMany_Optional].[OneToMany_Optional_InverseId], [l2.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [l2.OneToMany_Optional].[OneToMany_Required_InverseId], [l2.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [l2.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [l2.OneToMany_Optional].[OneToOne_Optional_SelfId]
FROM [Level3] AS [l2.OneToMany_Optional]
INNER JOIN (
    SELECT [l20].[Id], ABS([l20].[Level1_Required_Id]) AS [c], [l20].[Name], [l20].[Level1_Required_Id]
    FROM [Level2] AS [l20]
) AS [t] ON [l2.OneToMany_Optional].[OneToMany_Optional_InverseId] = [t].[Id]
ORDER BY [t].[c], [t].[Name], [t].[Id]");
        }

        public override void Include_collection_with_multiple_orderbys_complex()
        {
            base.Include_collection_with_multiple_orderbys_complex();

            AssertSql(
                @"SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_InverseId], [l2].[OneToMany_Optional_Self_InverseId], [l2].[OneToMany_Required_InverseId], [l2].[OneToMany_Required_Self_InverseId], [l2].[OneToOne_Optional_PK_InverseId], [l2].[OneToOne_Optional_SelfId]
FROM [Level2] AS [l2]
ORDER BY ABS([l2].[Level1_Required_Id]) + 7, [l2].[Name], [l2].[Id]",
                //
                @"SELECT [l2.OneToMany_Optional].[Id], [l2.OneToMany_Optional].[Level2_Optional_Id], [l2.OneToMany_Optional].[Level2_Required_Id], [l2.OneToMany_Optional].[Name], [l2.OneToMany_Optional].[OneToMany_Optional_InverseId], [l2.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [l2.OneToMany_Optional].[OneToMany_Required_InverseId], [l2.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [l2.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [l2.OneToMany_Optional].[OneToOne_Optional_SelfId]
FROM [Level3] AS [l2.OneToMany_Optional]
INNER JOIN (
    SELECT [l20].[Id], ABS([l20].[Level1_Required_Id]) + 7 AS [c], [l20].[Name], [l20].[Level1_Required_Id]
    FROM [Level2] AS [l20]
) AS [t] ON [l2.OneToMany_Optional].[OneToMany_Optional_InverseId] = [t].[Id]
ORDER BY [t].[c], [t].[Name], [t].[Id]");
        }

        public override void Include_collection_with_multiple_orderbys_complex_repeated()
        {
            base.Include_collection_with_multiple_orderbys_complex_repeated();

            AssertSql(
                @"SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_InverseId], [l2].[OneToMany_Optional_Self_InverseId], [l2].[OneToMany_Required_InverseId], [l2].[OneToMany_Required_Self_InverseId], [l2].[OneToOne_Optional_PK_InverseId], [l2].[OneToOne_Optional_SelfId]
FROM [Level2] AS [l2]
ORDER BY -[l2].[Level1_Required_Id], [l2].[Name], [l2].[Id]",
                //
                @"SELECT [l2.OneToMany_Optional].[Id], [l2.OneToMany_Optional].[Level2_Optional_Id], [l2.OneToMany_Optional].[Level2_Required_Id], [l2.OneToMany_Optional].[Name], [l2.OneToMany_Optional].[OneToMany_Optional_InverseId], [l2.OneToMany_Optional].[OneToMany_Optional_Self_InverseId], [l2.OneToMany_Optional].[OneToMany_Required_InverseId], [l2.OneToMany_Optional].[OneToMany_Required_Self_InverseId], [l2.OneToMany_Optional].[OneToOne_Optional_PK_InverseId], [l2.OneToMany_Optional].[OneToOne_Optional_SelfId]
FROM [Level3] AS [l2.OneToMany_Optional]
INNER JOIN (
    SELECT [l20].[Id], -[l20].[Level1_Required_Id] AS [c], [l20].[Name], [l20].[Level1_Required_Id]
    FROM [Level2] AS [l20]
) AS [t] ON [l2.OneToMany_Optional].[OneToMany_Optional_InverseId] = [t].[Id]
ORDER BY [t].[c], [t].[Name], [t].[Id]");
        }

        public override void Include_reference_with_groupby_in_subquery()
        {
            base.Include_reference_with_groupby_in_subquery();

            AssertSql(
                @"SELECT [l10].[Id], [l10].[Date], [l10].[Name], [l10].[OneToMany_Optional_Self_InverseId], [l10].[OneToMany_Required_Self_InverseId], [l10].[OneToOne_Optional_SelfId], [l1.OneToOne_Optional_FK0].[Id], [l1.OneToOne_Optional_FK0].[Date], [l1.OneToOne_Optional_FK0].[Level1_Optional_Id], [l1.OneToOne_Optional_FK0].[Level1_Required_Id], [l1.OneToOne_Optional_FK0].[Name], [l1.OneToOne_Optional_FK0].[OneToMany_Optional_InverseId], [l1.OneToOne_Optional_FK0].[OneToMany_Optional_Self_InverseId], [l1.OneToOne_Optional_FK0].[OneToMany_Required_InverseId], [l1.OneToOne_Optional_FK0].[OneToMany_Required_Self_InverseId], [l1.OneToOne_Optional_FK0].[OneToOne_Optional_PK_InverseId], [l1.OneToOne_Optional_FK0].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l10]
LEFT JOIN [Level2] AS [l1.OneToOne_Optional_FK0] ON [l10].[Id] = [l1.OneToOne_Optional_FK0].[Level1_Optional_Id]
ORDER BY [l10].[Name]");
        }

        public override void Include_collection_with_groupby_in_subquery()
        {
            base.Include_collection_with_groupby_in_subquery();

            AssertSql(
                @"SELECT [l11].[Id], [l11].[Date], [l11].[Name], [l11].[OneToMany_Optional_Self_InverseId], [l11].[OneToMany_Required_Self_InverseId], [l11].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l11]
ORDER BY [l11].[Name], [l11].[Id]",
                //
                @"SELECT [l1.OneToMany_Optional0].[Id], [l1.OneToMany_Optional0].[Date], [l1.OneToMany_Optional0].[Level1_Optional_Id], [l1.OneToMany_Optional0].[Level1_Required_Id], [l1.OneToMany_Optional0].[Name], [l1.OneToMany_Optional0].[OneToMany_Optional_InverseId], [l1.OneToMany_Optional0].[OneToMany_Optional_Self_InverseId], [l1.OneToMany_Optional0].[OneToMany_Required_InverseId], [l1.OneToMany_Optional0].[OneToMany_Required_Self_InverseId], [l1.OneToMany_Optional0].[OneToOne_Optional_PK_InverseId], [l1.OneToMany_Optional0].[OneToOne_Optional_SelfId]
FROM [Level2] AS [l1.OneToMany_Optional0]
INNER JOIN (
    SELECT [l12].[Id], [l12].[Name]
    FROM [Level1] AS [l12]
) AS [t0] ON [l1.OneToMany_Optional0].[OneToMany_Optional_InverseId] = [t0].[Id]
ORDER BY [t0].[Name], [t0].[Id]");
        }

        public override void Multi_include_with_groupby_in_subquery()
        {
            base.Multi_include_with_groupby_in_subquery();

            AssertSql(
                @"SELECT [l11].[Id], [l11].[Date], [l11].[Name], [l11].[OneToMany_Optional_Self_InverseId], [l11].[OneToMany_Required_Self_InverseId], [l11].[OneToOne_Optional_SelfId], [l1.OneToOne_Optional_FK1].[Id], [l1.OneToOne_Optional_FK1].[Date], [l1.OneToOne_Optional_FK1].[Level1_Optional_Id], [l1.OneToOne_Optional_FK1].[Level1_Required_Id], [l1.OneToOne_Optional_FK1].[Name], [l1.OneToOne_Optional_FK1].[OneToMany_Optional_InverseId], [l1.OneToOne_Optional_FK1].[OneToMany_Optional_Self_InverseId], [l1.OneToOne_Optional_FK1].[OneToMany_Required_InverseId], [l1.OneToOne_Optional_FK1].[OneToMany_Required_Self_InverseId], [l1.OneToOne_Optional_FK1].[OneToOne_Optional_PK_InverseId], [l1.OneToOne_Optional_FK1].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l11]
LEFT JOIN [Level2] AS [l1.OneToOne_Optional_FK1] ON [l11].[Id] = [l1.OneToOne_Optional_FK1].[Level1_Optional_Id]
ORDER BY [l11].[Name], [l1.OneToOne_Optional_FK1].[Id]",
                //
                @"SELECT [l1.OneToOne_Optional_FK.OneToMany_Optional0].[Id], [l1.OneToOne_Optional_FK.OneToMany_Optional0].[Level2_Optional_Id], [l1.OneToOne_Optional_FK.OneToMany_Optional0].[Level2_Required_Id], [l1.OneToOne_Optional_FK.OneToMany_Optional0].[Name], [l1.OneToOne_Optional_FK.OneToMany_Optional0].[OneToMany_Optional_InverseId], [l1.OneToOne_Optional_FK.OneToMany_Optional0].[OneToMany_Optional_Self_InverseId], [l1.OneToOne_Optional_FK.OneToMany_Optional0].[OneToMany_Required_InverseId], [l1.OneToOne_Optional_FK.OneToMany_Optional0].[OneToMany_Required_Self_InverseId], [l1.OneToOne_Optional_FK.OneToMany_Optional0].[OneToOne_Optional_PK_InverseId], [l1.OneToOne_Optional_FK.OneToMany_Optional0].[OneToOne_Optional_SelfId]
FROM [Level3] AS [l1.OneToOne_Optional_FK.OneToMany_Optional0]
INNER JOIN (
    SELECT DISTINCT [l1.OneToOne_Optional_FK2].[Id], [l12].[Name]
    FROM [Level1] AS [l12]
    LEFT JOIN [Level2] AS [l1.OneToOne_Optional_FK2] ON [l12].[Id] = [l1.OneToOne_Optional_FK2].[Level1_Optional_Id]
) AS [t0] ON [l1.OneToOne_Optional_FK.OneToMany_Optional0].[OneToMany_Optional_InverseId] = [t0].[Id]
ORDER BY [t0].[Name], [t0].[Id]");
        }

        public override void Include_collection_with_groupby_in_subquery_and_filter_before_groupby()
        {
            base.Include_collection_with_groupby_in_subquery_and_filter_before_groupby();

            AssertSql(
                @"SELECT [l11].[Id], [l11].[Date], [l11].[Name], [l11].[OneToMany_Optional_Self_InverseId], [l11].[OneToMany_Required_Self_InverseId], [l11].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l11]
WHERE [l11].[Id] > 3
ORDER BY [l11].[Name], [l11].[Id]",
                //
                @"SELECT [l1.OneToMany_Optional0].[Id], [l1.OneToMany_Optional0].[Date], [l1.OneToMany_Optional0].[Level1_Optional_Id], [l1.OneToMany_Optional0].[Level1_Required_Id], [l1.OneToMany_Optional0].[Name], [l1.OneToMany_Optional0].[OneToMany_Optional_InverseId], [l1.OneToMany_Optional0].[OneToMany_Optional_Self_InverseId], [l1.OneToMany_Optional0].[OneToMany_Required_InverseId], [l1.OneToMany_Optional0].[OneToMany_Required_Self_InverseId], [l1.OneToMany_Optional0].[OneToOne_Optional_PK_InverseId], [l1.OneToMany_Optional0].[OneToOne_Optional_SelfId]
FROM [Level2] AS [l1.OneToMany_Optional0]
INNER JOIN (
    SELECT [l12].[Id], [l12].[Name]
    FROM [Level1] AS [l12]
    WHERE [l12].[Id] > 3
) AS [t0] ON [l1.OneToMany_Optional0].[OneToMany_Optional_InverseId] = [t0].[Id]
ORDER BY [t0].[Name], [t0].[Id]");
        }

        public override void Include_collection_with_groupby_in_subquery_and_filter_after_groupby()
        {
            base.Include_collection_with_groupby_in_subquery_and_filter_after_groupby();

            AssertSql(
                @"SELECT [l11].[Id], [l11].[Date], [l11].[Name], [l11].[OneToMany_Optional_Self_InverseId], [l11].[OneToMany_Required_Self_InverseId], [l11].[OneToOne_Optional_SelfId]
FROM [Level1] AS [l11]
ORDER BY [l11].[Name], [l11].[Id]",
                //
                @"SELECT [l1.OneToMany_Optional0].[Id], [l1.OneToMany_Optional0].[Date], [l1.OneToMany_Optional0].[Level1_Optional_Id], [l1.OneToMany_Optional0].[Level1_Required_Id], [l1.OneToMany_Optional0].[Name], [l1.OneToMany_Optional0].[OneToMany_Optional_InverseId], [l1.OneToMany_Optional0].[OneToMany_Optional_Self_InverseId], [l1.OneToMany_Optional0].[OneToMany_Required_InverseId], [l1.OneToMany_Optional0].[OneToMany_Required_Self_InverseId], [l1.OneToMany_Optional0].[OneToOne_Optional_PK_InverseId], [l1.OneToMany_Optional0].[OneToOne_Optional_SelfId]
FROM [Level2] AS [l1.OneToMany_Optional0]
INNER JOIN (
    SELECT [l12].[Id], [l12].[Name]
    FROM [Level1] AS [l12]
) AS [t0] ON [l1.OneToMany_Optional0].[OneToMany_Optional_InverseId] = [t0].[Id]
ORDER BY [t0].[Name], [t0].[Id]");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        private void AssertContainsSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected, assertOrder: false);
    }
}
