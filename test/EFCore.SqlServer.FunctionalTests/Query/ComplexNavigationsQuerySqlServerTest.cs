// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public class ComplexNavigationsQuerySqlServerTest : ComplexNavigationsQueryRelationalTestBase<ComplexNavigationsQuerySqlServerFixture>
    {
        public ComplexNavigationsQuerySqlServerTest(
            ComplexNavigationsQuerySqlServerFixture fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        protected override bool CanExecuteQueryString
            => true;

        public override async Task Entity_equality_empty(bool async)
        {
            await base.Entity_equality_empty(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
WHERE [l0].[Id] = 0");
        }

        public override async Task Key_equality_when_sentinel_ef_property(bool async)
        {
            await base.Key_equality_when_sentinel_ef_property(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
WHERE [l0].[Id] = 0");
        }

        public override async Task Key_equality_using_property_method_required(bool async)
        {
            await base.Key_equality_using_property_method_required(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Required_Id]
WHERE [l0].[Id] > 7");
        }

        public override async Task Key_equality_using_property_method_required2(bool async)
        {
            await base.Key_equality_using_property_method_required2(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Name], [l].[OneToMany_Optional_Inverse2Id], [l].[OneToMany_Optional_Self_Inverse2Id], [l].[OneToMany_Required_Inverse2Id], [l].[OneToMany_Required_Self_Inverse2Id], [l].[OneToOne_Optional_PK_Inverse2Id], [l].[OneToOne_Optional_Self2Id]
FROM [LevelTwo] AS [l]
INNER JOIN [LevelOne] AS [l0] ON [l].[Level1_Required_Id] = [l0].[Id]
WHERE [l0].[Id] > 7");
        }

        public override async Task Key_equality_using_property_method_nested(bool async)
        {
            await base.Key_equality_using_property_method_nested(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Required_Id]
WHERE [l0].[Id] = 7");
        }

        public override async Task Key_equality_using_property_method_nested2(bool async)
        {
            await base.Key_equality_using_property_method_nested2(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Name], [l].[OneToMany_Optional_Inverse2Id], [l].[OneToMany_Optional_Self_Inverse2Id], [l].[OneToMany_Required_Inverse2Id], [l].[OneToMany_Required_Self_Inverse2Id], [l].[OneToOne_Optional_PK_Inverse2Id], [l].[OneToOne_Optional_Self2Id]
FROM [LevelTwo] AS [l]
INNER JOIN [LevelOne] AS [l0] ON [l].[Level1_Required_Id] = [l0].[Id]
WHERE [l0].[Id] = 7");
        }

        public override async Task Key_equality_using_property_method_and_member_expression1(bool async)
        {
            await base.Key_equality_using_property_method_and_member_expression1(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Required_Id]
WHERE [l0].[Id] = 7");
        }

        public override async Task Key_equality_using_property_method_and_member_expression2(bool async)
        {
            await base.Key_equality_using_property_method_and_member_expression2(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Required_Id]
WHERE [l0].[Id] = 7");
        }

        public override async Task Key_equality_using_property_method_and_member_expression3(bool async)
        {
            await base.Key_equality_using_property_method_and_member_expression3(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Name], [l].[OneToMany_Optional_Inverse2Id], [l].[OneToMany_Optional_Self_Inverse2Id], [l].[OneToMany_Required_Inverse2Id], [l].[OneToMany_Required_Self_Inverse2Id], [l].[OneToOne_Optional_PK_Inverse2Id], [l].[OneToOne_Optional_Self2Id]
FROM [LevelTwo] AS [l]
INNER JOIN [LevelOne] AS [l0] ON [l].[Level1_Required_Id] = [l0].[Id]
WHERE [l0].[Id] = 7");
        }

        public override async Task Key_equality_navigation_converted_to_FK(bool async)
        {
            await base.Key_equality_navigation_converted_to_FK(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Name], [l].[OneToMany_Optional_Inverse2Id], [l].[OneToMany_Optional_Self_Inverse2Id], [l].[OneToMany_Required_Inverse2Id], [l].[OneToMany_Required_Self_Inverse2Id], [l].[OneToOne_Optional_PK_Inverse2Id], [l].[OneToOne_Optional_Self2Id]
FROM [LevelTwo] AS [l]
INNER JOIN [LevelOne] AS [l0] ON [l].[Level1_Required_Id] = [l0].[Id]
WHERE [l0].[Id] = 1");
        }

        public override async Task Key_equality_two_conditions_on_same_navigation(bool async)
        {
            await base.Key_equality_two_conditions_on_same_navigation(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Required_Id]
WHERE [l0].[Id] IN (1, 2)");
        }

        public override async Task Key_equality_two_conditions_on_same_navigation2(bool async)
        {
            await base.Key_equality_two_conditions_on_same_navigation2(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Name], [l].[OneToMany_Optional_Inverse2Id], [l].[OneToMany_Optional_Self_Inverse2Id], [l].[OneToMany_Required_Inverse2Id], [l].[OneToMany_Required_Self_Inverse2Id], [l].[OneToOne_Optional_PK_Inverse2Id], [l].[OneToOne_Optional_Self2Id]
FROM [LevelTwo] AS [l]
INNER JOIN [LevelOne] AS [l0] ON [l].[Level1_Required_Id] = [l0].[Id]
WHERE [l0].[Id] IN (1, 2)");
        }

        public override void Multi_level_include_with_short_circuiting()
        {
            base.Multi_level_include_with_short_circuiting();

            AssertSql(
                @"SELECT [f].[Name], [f].[LabelDefaultText], [f].[PlaceholderDefaultText], [m].[DefaultText], [m0].[DefaultText], [t].[Text], [t].[ComplexNavigationStringDefaultText], [t].[LanguageName], [t].[Name], [t].[CultureString], [t0].[Text], [t0].[ComplexNavigationStringDefaultText], [t0].[LanguageName], [t0].[Name], [t0].[CultureString]
FROM [Fields] AS [f]
LEFT JOIN [MultilingualStrings] AS [m] ON [f].[LabelDefaultText] = [m].[DefaultText]
LEFT JOIN [MultilingualStrings] AS [m0] ON [f].[PlaceholderDefaultText] = [m0].[DefaultText]
LEFT JOIN (
    SELECT [g].[Text], [g].[ComplexNavigationStringDefaultText], [g].[LanguageName], [l].[Name], [l].[CultureString]
    FROM [Globalizations] AS [g]
    LEFT JOIN [Languages] AS [l] ON [g].[LanguageName] = [l].[Name]
) AS [t] ON [m].[DefaultText] = [t].[ComplexNavigationStringDefaultText]
LEFT JOIN (
    SELECT [g0].[Text], [g0].[ComplexNavigationStringDefaultText], [g0].[LanguageName], [l0].[Name], [l0].[CultureString]
    FROM [Globalizations] AS [g0]
    LEFT JOIN [Languages] AS [l0] ON [g0].[LanguageName] = [l0].[Name]
) AS [t0] ON [m0].[DefaultText] = [t0].[ComplexNavigationStringDefaultText]
ORDER BY [f].[Name], [m].[DefaultText], [m0].[DefaultText], [t].[Text], [t].[Name], [t0].[Text], [t0].[Name]");
        }

        public override async Task Join_navigation_key_access_optional(bool async)
        {
            await base.Join_navigation_key_access_optional(async);

            AssertSql(
                @"SELECT [l].[Id] AS [Id1], [t].[Id] AS [Id2]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l1].[Id] AS [Id0]
    FROM [LevelTwo] AS [l0]
    LEFT JOIN [LevelOne] AS [l1] ON [l0].[Level1_Optional_Id] = [l1].[Id]
) AS [t] ON [l].[Id] = [t].[Id0]");
        }

        public override async Task Join_navigation_key_access_required(bool async)
        {
            await base.Join_navigation_key_access_required(async);

            AssertSql(
                @"SELECT [l].[Id] AS [Id1], [t].[Id] AS [Id2]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l1].[Id] AS [Id0]
    FROM [LevelTwo] AS [l0]
    INNER JOIN [LevelOne] AS [l1] ON [l0].[Level1_Required_Id] = [l1].[Id]
) AS [t] ON [l].[Id] = [t].[Id0]");
        }

        public override async Task Navigation_key_access_optional_comparison(bool async)
        {
            await base.Navigation_key_access_optional_comparison(async);

            AssertSql(
                @"SELECT [l].[Id]
FROM [LevelTwo] AS [l]
LEFT JOIN [LevelOne] AS [l0] ON [l].[OneToOne_Optional_PK_Inverse2Id] = [l0].[Id]
WHERE [l0].[Id] > 5");
        }

        public override async Task Navigation_key_access_required_comparison(bool async)
        {
            await base.Navigation_key_access_required_comparison(async);

            AssertSql(
                @"SELECT [l].[Id]
FROM [LevelTwo] AS [l]
INNER JOIN [LevelOne] AS [l0] ON [l].[Id] = [l0].[Id]
WHERE [l0].[Id] > 5");
        }

        public override async Task Navigation_inside_method_call_translated_to_join(bool async)
        {
            await base.Navigation_inside_method_call_translated_to_join(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Required_Id]
WHERE [l0].[Name] IS NOT NULL AND ([l0].[Name] LIKE N'L%')");
        }

        public override async Task Navigation_inside_method_call_translated_to_join2(bool async)
        {
            await base.Navigation_inside_method_call_translated_to_join2(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Level2_Optional_Id], [l].[Level2_Required_Id], [l].[Name], [l].[OneToMany_Optional_Inverse3Id], [l].[OneToMany_Optional_Self_Inverse3Id], [l].[OneToMany_Required_Inverse3Id], [l].[OneToMany_Required_Self_Inverse3Id], [l].[OneToOne_Optional_PK_Inverse3Id], [l].[OneToOne_Optional_Self3Id]
FROM [LevelThree] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Level2_Required_Id] = [l0].[Id]
WHERE [l0].[Name] IS NOT NULL AND ([l0].[Name] LIKE N'L%')");
        }

        public override async Task Optional_navigation_inside_method_call_translated_to_join(bool async)
        {
            await base.Optional_navigation_inside_method_call_translated_to_join(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
WHERE [l0].[Name] IS NOT NULL AND ([l0].[Name] LIKE N'L%')");
        }

        public override async Task Optional_navigation_inside_property_method_translated_to_join(bool async)
        {
            await base.Optional_navigation_inside_property_method_translated_to_join(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
WHERE [l0].[Name] = N'L2 01'");
        }

        public override async Task Optional_navigation_inside_nested_method_call_translated_to_join(bool async)
        {
            await base.Optional_navigation_inside_nested_method_call_translated_to_join(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
WHERE [l0].[Name] IS NOT NULL AND (UPPER([l0].[Name]) LIKE N'L%')");
        }

        public override async Task Method_call_on_optional_navigation_translates_to_null_conditional_properly_for_arguments(bool async)
        {
            await base.Method_call_on_optional_navigation_translates_to_null_conditional_properly_for_arguments(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
WHERE ([l0].[Name] = N'') OR ([l0].[Name] IS NOT NULL AND (LEFT([l0].[Name], LEN([l0].[Name])) = [l0].[Name]))");
        }

        public override async Task Optional_navigation_inside_method_call_translated_to_join_keeps_original_nullability(bool async)
        {
            await base.Optional_navigation_inside_method_call_translated_to_join_keeps_original_nullability(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
WHERE DATEADD(day, CAST(10.0E0 AS int), [l0].[Date]) > '2000-02-01T00:00:00.0000000'");
        }

        public override async Task Optional_navigation_inside_nested_method_call_translated_to_join_keeps_original_nullability(bool async)
        {
            await base.Optional_navigation_inside_nested_method_call_translated_to_join_keeps_original_nullability(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
WHERE DATEADD(month, CAST(2 AS int), DATEADD(day, CAST(15.0E0 AS int), DATEADD(day, CAST(10.0E0 AS int), [l0].[Date]))) > '2002-02-01T00:00:00.0000000'");
        }

        public override async Task
            Optional_navigation_inside_nested_method_call_translated_to_join_keeps_original_nullability_also_for_arguments(bool async)
        {
            await base.Optional_navigation_inside_nested_method_call_translated_to_join_keeps_original_nullability_also_for_arguments(
                async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
WHERE DATEADD(day, CAST(CAST([l0].[Id] AS float) AS int), DATEADD(day, CAST(15.0E0 AS int), [l0].[Date])) > '2002-02-01T00:00:00.0000000'");
        }

        public override async Task Join_navigation_in_outer_selector_translated_to_extra_join(bool async)
        {
            await base.Join_navigation_in_outer_selector_translated_to_extra_join(async);

            AssertSql(
                @"SELECT [l].[Id] AS [Id1], [l1].[Id] AS [Id2]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
INNER JOIN [LevelTwo] AS [l1] ON [l0].[Id] = [l1].[Id]");
        }

        public override async Task Join_navigation_in_outer_selector_translated_to_extra_join_nested(bool async)
        {
            await base.Join_navigation_in_outer_selector_translated_to_extra_join_nested(async);

            AssertSql(
                @"SELECT [l].[Id] AS [Id1], [l2].[Id] AS [Id3]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Required_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Optional_Id]
INNER JOIN [LevelThree] AS [l2] ON [l1].[Id] = [l2].[Id]");
        }

        public override async Task Join_navigation_in_outer_selector_translated_to_extra_join_nested2(bool async)
        {
            await base.Join_navigation_in_outer_selector_translated_to_extra_join_nested2(async);

            AssertSql(
                @"SELECT [l].[Id] AS [Id3], [l2].[Id] AS [Id1]
FROM [LevelThree] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Level2_Required_Id] = [l0].[Id]
LEFT JOIN [LevelOne] AS [l1] ON [l0].[Level1_Optional_Id] = [l1].[Id]
INNER JOIN [LevelOne] AS [l2] ON [l1].[Id] = [l2].[Id]");
        }

        public override async Task Join_navigation_in_inner_selector(bool async)
        {
            await base.Join_navigation_in_inner_selector(async);

            AssertSql(
                @"SELECT [l].[Id] AS [Id2], [t].[Id] AS [Id1]
FROM [LevelTwo] AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l1].[Id] AS [Id0]
    FROM [LevelOne] AS [l0]
    LEFT JOIN [LevelTwo] AS [l1] ON [l0].[Id] = [l1].[Level1_Optional_Id]
) AS [t] ON [l].[Id] = [t].[Id0]");
        }

        public override async Task Join_navigations_in_inner_selector_translated_without_collision(bool async)
        {
            await base.Join_navigations_in_inner_selector_translated_without_collision(async);

            AssertSql(
                @"SELECT [l].[Id] AS [Id2], [t].[Id] AS [Id1], [t0].[Id] AS [Id3]
FROM [LevelTwo] AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l1].[Id] AS [Id0]
    FROM [LevelOne] AS [l0]
    LEFT JOIN [LevelTwo] AS [l1] ON [l0].[Id] = [l1].[Level1_Optional_Id]
) AS [t] ON [l].[Id] = [t].[Id0]
INNER JOIN (
    SELECT [l2].[Id], [l3].[Id] AS [Id0]
    FROM [LevelThree] AS [l2]
    LEFT JOIN [LevelTwo] AS [l3] ON [l2].[Level2_Optional_Id] = [l3].[Id]
) AS [t0] ON [l].[Id] = [t0].[Id0]");
        }

        public override async Task Join_navigation_non_key_join(bool async)
        {
            await base.Join_navigation_non_key_join(async);

            AssertSql(
                @"SELECT [l].[Id] AS [Id2], [l].[Name] AS [Name2], [t].[Id] AS [Id1], [t].[Name] AS [Name1]
FROM [LevelTwo] AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[Name], [l1].[Name] AS [Name0]
    FROM [LevelOne] AS [l0]
    LEFT JOIN [LevelTwo] AS [l1] ON [l0].[Id] = [l1].[Level1_Optional_Id]
) AS [t] ON [l].[Name] = [t].[Name0]");
        }

        public override async Task Join_with_orderby_on_inner_sequence_navigation_non_key_join(bool async)
        {
            await base.Join_with_orderby_on_inner_sequence_navigation_non_key_join(async);

            AssertSql(
                @"SELECT [l].[Id] AS [Id2], [l].[Name] AS [Name2], [t].[Id] AS [Id1], [t].[Name] AS [Name1]
FROM [LevelTwo] AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[Name], [l1].[Name] AS [Name0]
    FROM [LevelOne] AS [l0]
    LEFT JOIN [LevelTwo] AS [l1] ON [l0].[Id] = [l1].[Level1_Optional_Id]
) AS [t] ON [l].[Name] = [t].[Name0]");
        }

        public override async Task Join_navigation_self_ref(bool async)
        {
            await base.Join_navigation_self_ref(async);

            AssertSql(
                @"SELECT [l].[Id] AS [Id1], [t].[Id] AS [Id2]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l1].[Id] AS [Id0]
    FROM [LevelOne] AS [l0]
    LEFT JOIN [LevelOne] AS [l1] ON [l0].[OneToMany_Optional_Self_Inverse1Id] = [l1].[Id]
) AS [t] ON [l].[Id] = [t].[Id0]");
        }

        public override async Task Join_navigation_nested(bool async)
        {
            await base.Join_navigation_nested(async);

            AssertSql(
                @"SELECT [l].[Id] AS [Id3], [t].[Id] AS [Id1]
FROM [LevelThree] AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l2].[Id] AS [Id1]
    FROM [LevelOne] AS [l0]
    LEFT JOIN [LevelTwo] AS [l1] ON [l0].[Id] = [l1].[Level1_Required_Id]
    LEFT JOIN [LevelThree] AS [l2] ON [l1].[Id] = [l2].[Level2_Optional_Id]
) AS [t] ON [l].[Id] = [t].[Id1]");
        }

        public override async Task Join_navigation_nested2(bool async)
        {
            await base.Join_navigation_nested2(async);

            AssertSql(
                @"SELECT [l].[Id] AS [Id3], [t].[Id] AS [Id1]
FROM [LevelThree] AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l2].[Id] AS [Id1]
    FROM [LevelOne] AS [l0]
    LEFT JOIN [LevelTwo] AS [l1] ON [l0].[Id] = [l1].[Level1_Required_Id]
    LEFT JOIN [LevelThree] AS [l2] ON [l1].[Id] = [l2].[Level2_Optional_Id]
) AS [t] ON [l].[Id] = [t].[Id1]");
        }

        public override async Task Join_navigation_deeply_nested_non_key_join(bool async)
        {
            await base.Join_navigation_deeply_nested_non_key_join(async);

            AssertSql(
                @"SELECT [l].[Id] AS [Id4], [l].[Name] AS [Name4], [t].[Id] AS [Id1], [t].[Name] AS [Name1]
FROM [LevelFour] AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[Name], [l3].[Name] AS [Name2]
    FROM [LevelOne] AS [l0]
    LEFT JOIN [LevelTwo] AS [l1] ON [l0].[Id] = [l1].[Level1_Required_Id]
    LEFT JOIN [LevelThree] AS [l2] ON [l1].[Id] = [l2].[Level2_Optional_Id]
    LEFT JOIN [LevelFour] AS [l3] ON [l2].[Id] = [l3].[Id]
) AS [t] ON [l].[Name] = [t].[Name2]");
        }

        public override async Task Join_navigation_deeply_nested_required(bool async)
        {
            await base.Join_navigation_deeply_nested_required(async);

            AssertSql(
                @"SELECT [t].[Id] AS [Id4], [t].[Name] AS [Name4], [l].[Id] AS [Id1], [l].[Name] AS [Name1]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[Name], [l3].[Name] AS [Name2]
    FROM [LevelFour] AS [l0]
    INNER JOIN [LevelThree] AS [l1] ON [l0].[Level3_Required_Id] = [l1].[Id]
    INNER JOIN [LevelTwo] AS [l2] ON [l1].[Level2_Required_Id] = [l2].[Id]
    INNER JOIN [LevelOne] AS [l3] ON [l2].[Id] = [l3].[Id]
) AS [t] ON [l].[Name] = [t].[Name2]");
        }

        public override async Task Include_reference_and_project_into_anonymous_type(bool async)
        {
            await base.Include_reference_and_project_into_anonymous_type(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]");
        }

        public override async Task Select_nav_prop_reference_optional1(bool async)
        {
            await base.Select_nav_prop_reference_optional1(async);

            AssertSql(
                @"SELECT [l0].[Name]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]");
        }

        public override async Task Select_nav_prop_reference_optional1_via_DefaultIfEmpty(bool async)
        {
            await base.Select_nav_prop_reference_optional1_via_DefaultIfEmpty(async);

            AssertSql(
                @"SELECT [l0].[Name]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]");
        }

        public override async Task Select_nav_prop_reference_optional2(bool async)
        {
            await base.Select_nav_prop_reference_optional2(async);

            AssertSql(
                @"SELECT [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]");
        }

        public override async Task Select_nav_prop_reference_optional2_via_DefaultIfEmpty(bool async)
        {
            await base.Select_nav_prop_reference_optional2_via_DefaultIfEmpty(async);

            AssertSql(
                @"SELECT [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]");
        }

        public override async Task Select_nav_prop_reference_optional3(bool async)
        {
            await base.Select_nav_prop_reference_optional3(async);

            AssertSql(
                @"SELECT [l0].[Name]
FROM [LevelTwo] AS [l]
LEFT JOIN [LevelOne] AS [l0] ON [l].[Level1_Optional_Id] = [l0].[Id]");
        }

        public override async Task Where_nav_prop_reference_optional1(bool async)
        {
            await base.Where_nav_prop_reference_optional1(async);

            AssertSql(
                @"SELECT [l].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
WHERE [l0].[Name] IN (N'L2 05', N'L2 07')");
        }

        public override async Task Where_nav_prop_reference_optional1_via_DefaultIfEmpty(bool async)
        {
            await base.Where_nav_prop_reference_optional1_via_DefaultIfEmpty(async);

            AssertSql(
                @"SELECT [l].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelTwo] AS [l1] ON [l].[Id] = [l1].[Level1_Optional_Id]
WHERE ([l0].[Name] = N'L2 05') OR ([l1].[Name] = N'L2 07')");
        }

        public override async Task Where_nav_prop_reference_optional2(bool async)
        {
            await base.Where_nav_prop_reference_optional2(async);

            AssertSql(
                @"SELECT [l].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
WHERE ([l0].[Name] = N'L2 05') OR (([l0].[Name] <> N'L2 42') OR [l0].[Name] IS NULL)");
        }

        public override async Task Where_nav_prop_reference_optional2_via_DefaultIfEmpty(bool async)
        {
            await base.Where_nav_prop_reference_optional2_via_DefaultIfEmpty(async);

            AssertSql(
                @"SELECT [l].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelTwo] AS [l1] ON [l].[Id] = [l1].[Level1_Optional_Id]
WHERE ([l0].[Name] = N'L2 05') OR (([l1].[Name] <> N'L2 42') OR [l1].[Name] IS NULL)");
        }

        public override async Task Select_multiple_nav_prop_reference_optional(bool async)
        {
            await base.Select_multiple_nav_prop_reference_optional(async);

            AssertSql(
                @"SELECT [l1].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Optional_Id]");
        }

        public override async Task Where_multiple_nav_prop_reference_optional_member_compared_to_value(bool async)
        {
            await base.Where_multiple_nav_prop_reference_optional_member_compared_to_value(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Optional_Id]
WHERE ([l1].[Name] <> N'L3 05') OR [l1].[Name] IS NULL");
        }

        public override async Task Where_multiple_nav_prop_reference_optional_member_compared_to_null(bool async)
        {
            await base.Where_multiple_nav_prop_reference_optional_member_compared_to_null(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Optional_Id]
WHERE [l1].[Name] IS NOT NULL");
        }

        public override async Task Where_multiple_nav_prop_reference_optional_compared_to_null1(bool async)
        {
            await base.Where_multiple_nav_prop_reference_optional_compared_to_null1(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Optional_Id]
WHERE [l1].[Id] IS NULL");
        }

        public override async Task Where_multiple_nav_prop_reference_optional_compared_to_null2(bool async)
        {
            await base.Where_multiple_nav_prop_reference_optional_compared_to_null2(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Level2_Optional_Id], [l].[Level2_Required_Id], [l].[Name], [l].[OneToMany_Optional_Inverse3Id], [l].[OneToMany_Optional_Self_Inverse3Id], [l].[OneToMany_Required_Inverse3Id], [l].[OneToMany_Required_Self_Inverse3Id], [l].[OneToOne_Optional_PK_Inverse3Id], [l].[OneToOne_Optional_Self3Id]
FROM [LevelThree] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Level2_Optional_Id] = [l0].[Id]
LEFT JOIN [LevelOne] AS [l1] ON [l0].[Level1_Optional_Id] = [l1].[Id]
WHERE [l1].[Id] IS NULL");
        }

        public override async Task Where_multiple_nav_prop_reference_optional_compared_to_null3(bool async)
        {
            await base.Where_multiple_nav_prop_reference_optional_compared_to_null3(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Optional_Id]
WHERE [l1].[Id] IS NOT NULL");
        }

        public override async Task Where_multiple_nav_prop_reference_optional_compared_to_null4(bool async)
        {
            await base.Where_multiple_nav_prop_reference_optional_compared_to_null4(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Level2_Optional_Id], [l].[Level2_Required_Id], [l].[Name], [l].[OneToMany_Optional_Inverse3Id], [l].[OneToMany_Optional_Self_Inverse3Id], [l].[OneToMany_Required_Inverse3Id], [l].[OneToMany_Required_Self_Inverse3Id], [l].[OneToOne_Optional_PK_Inverse3Id], [l].[OneToOne_Optional_Self3Id]
FROM [LevelThree] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Level2_Optional_Id] = [l0].[Id]
LEFT JOIN [LevelOne] AS [l1] ON [l0].[Level1_Optional_Id] = [l1].[Id]
WHERE [l1].[Id] IS NOT NULL");
        }

        public override async Task Where_multiple_nav_prop_reference_optional_compared_to_null5(bool async)
        {
            await base.Where_multiple_nav_prop_reference_optional_compared_to_null5(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Required_Id]
LEFT JOIN [LevelFour] AS [l2] ON [l1].[Id] = [l2].[Level3_Required_Id]
WHERE [l2].[Id] IS NULL");
        }

        public override async Task Select_multiple_nav_prop_reference_required(bool async)
        {
            await base.Select_multiple_nav_prop_reference_required(async);

            AssertSql(
                @"SELECT [l1].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Required_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Required_Id]");
        }

        public override async Task Select_multiple_nav_prop_reference_required2(bool async)
        {
            await base.Select_multiple_nav_prop_reference_required2(async);

            AssertSql(
                @"SELECT [l1].[Id]
FROM [LevelThree] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Level2_Required_Id] = [l0].[Id]
INNER JOIN [LevelOne] AS [l1] ON [l0].[Level1_Required_Id] = [l1].[Id]");
        }

        public override async Task Select_multiple_nav_prop_optional_required(bool async)
        {
            await base.Select_multiple_nav_prop_optional_required(async);

            AssertSql(
                @"SELECT [l1].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Required_Id]");
        }

        public override async Task Where_multiple_nav_prop_optional_required(bool async)
        {
            await base.Where_multiple_nav_prop_optional_required(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Required_Id]
WHERE ([l1].[Name] <> N'L3 05') OR [l1].[Name] IS NULL");
        }

        public override async Task SelectMany_navigation_comparison1(bool async)
        {
            await base.SelectMany_navigation_comparison1(async);

            AssertSql(
                @"SELECT [l].[Id] AS [Id1], [l0].[Id] AS [Id2]
FROM [LevelOne] AS [l]
CROSS JOIN [LevelOne] AS [l0]
WHERE [l].[Id] = [l0].[Id]");
        }

        public override async Task SelectMany_navigation_comparison2(bool async)
        {
            await base.SelectMany_navigation_comparison2(async);

            AssertSql(
                @"SELECT [l].[Id] AS [Id1], [l0].[Id] AS [Id2]
FROM [LevelOne] AS [l]
CROSS JOIN [LevelTwo] AS [l0]
LEFT JOIN [LevelOne] AS [l1] ON [l0].[Level1_Optional_Id] = [l1].[Id]
WHERE [l].[Id] = [l1].[Id]");
        }

        public override async Task SelectMany_navigation_comparison3(bool async)
        {
            await base.SelectMany_navigation_comparison3(async);

            AssertSql(
                @"SELECT [l].[Id] AS [Id1], [l0].[Id] AS [Id2]
FROM [LevelOne] AS [l]
CROSS JOIN [LevelTwo] AS [l0]
LEFT JOIN [LevelTwo] AS [l1] ON [l].[Id] = [l1].[Level1_Optional_Id]
WHERE [l1].[Id] = [l0].[Id]");
        }

        public override async Task Where_complex_predicate_with_with_nav_prop_and_OrElse1(bool async)
        {
            await base.Where_complex_predicate_with_with_nav_prop_and_OrElse1(async);

            AssertSql(
                @"SELECT [l].[Id] AS [Id1], [l0].[Id] AS [Id2]
FROM [LevelOne] AS [l]
CROSS JOIN [LevelTwo] AS [l0]
LEFT JOIN [LevelTwo] AS [l1] ON [l].[Id] = [l1].[Level1_Optional_Id]
INNER JOIN [LevelOne] AS [l2] ON [l0].[Level1_Required_Id] = [l2].[Id]
WHERE ([l1].[Name] = N'L2 01') OR (([l2].[Name] <> N'Bar') OR [l2].[Name] IS NULL)");
        }

        public override async Task Where_complex_predicate_with_with_nav_prop_and_OrElse2(bool async)
        {
            await base.Where_complex_predicate_with_with_nav_prop_and_OrElse2(async);

            AssertSql(
                @"SELECT [l].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Required_Id]
WHERE ([l1].[Name] = N'L3 05') OR (([l0].[Name] <> N'L2 05') OR [l0].[Name] IS NULL)");
        }

        public override async Task Where_complex_predicate_with_with_nav_prop_and_OrElse3(bool async)
        {
            await base.Where_complex_predicate_with_with_nav_prop_and_OrElse3(async);

            AssertSql(
                @"SELECT [l].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelTwo] AS [l1] ON [l].[Id] = [l1].[Level1_Required_Id]
LEFT JOIN [LevelThree] AS [l2] ON [l1].[Id] = [l2].[Level2_Optional_Id]
WHERE (([l0].[Name] <> N'L2 05') OR [l0].[Name] IS NULL) OR ([l2].[Name] = N'L3 05')");
        }

        public override async Task Where_complex_predicate_with_with_nav_prop_and_OrElse4(bool async)
        {
            await base.Where_complex_predicate_with_with_nav_prop_and_OrElse4(async);

            AssertSql(
                @"SELECT [l].[Id]
FROM [LevelThree] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Level2_Optional_Id] = [l0].[Id]
INNER JOIN [LevelTwo] AS [l1] ON [l].[Level2_Required_Id] = [l1].[Id]
LEFT JOIN [LevelOne] AS [l2] ON [l1].[Level1_Optional_Id] = [l2].[Id]
WHERE (([l0].[Name] <> N'L2 05') OR [l0].[Name] IS NULL) OR ([l2].[Name] = N'L1 05')");
        }

        public override async Task Complex_navigations_with_predicate_projected_into_anonymous_type(bool async)
        {
            await base.Complex_navigations_with_predicate_projected_into_anonymous_type(async);

            AssertSql(
                @"SELECT [l].[Name], [l2].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Required_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Required_Id]
LEFT JOIN [LevelThree] AS [l2] ON [l0].[Id] = [l2].[Level2_Optional_Id]
WHERE (([l1].[Id] = [l2].[Id]) OR ([l1].[Id] IS NULL AND [l2].[Id] IS NULL)) AND (([l2].[Id] <> 7) OR [l2].[Id] IS NULL)");
        }

        public override async Task Complex_navigations_with_predicate_projected_into_anonymous_type2(bool async)
        {
            await base.Complex_navigations_with_predicate_projected_into_anonymous_type2(async);

            AssertSql(
                @"SELECT [l].[Name], [l2].[Id]
FROM [LevelThree] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Level2_Required_Id] = [l0].[Id]
INNER JOIN [LevelOne] AS [l1] ON [l0].[Level1_Required_Id] = [l1].[Id]
LEFT JOIN [LevelOne] AS [l2] ON [l0].[Level1_Optional_Id] = [l2].[Id]
WHERE ([l1].[Id] = [l2].[Id]) AND (([l2].[Id] <> 7) OR [l2].[Id] IS NULL)");
        }

        public override async Task Optional_navigation_projected_into_DTO(bool async)
        {
            await base.Optional_navigation_projected_into_DTO(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Name], CASE
    WHEN [l0].[Id] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [l0].[Id], [l0].[Name]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]");
        }

        public override async Task OrderBy_nav_prop_reference_optional(bool async)
        {
            await base.OrderBy_nav_prop_reference_optional(async);

            AssertSql(
                @"SELECT [l].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
ORDER BY [l0].[Name], [l].[Id]");
        }

        public override async Task OrderBy_nav_prop_reference_optional_via_DefaultIfEmpty(bool async)
        {
            await base.OrderBy_nav_prop_reference_optional_via_DefaultIfEmpty(async);

            AssertSql(
                @"SELECT [l].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
ORDER BY [l0].[Name], [l].[Id]");
        }

        public override async Task Result_operator_nav_prop_reference_optional_Sum(bool async)
        {
            await base.Result_operator_nav_prop_reference_optional_Sum(async);

            AssertSql(
                @"SELECT COALESCE(SUM([l0].[Level1_Required_Id]), 0)
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]");
        }

        public override async Task Result_operator_nav_prop_reference_optional_Min(bool async)
        {
            await base.Result_operator_nav_prop_reference_optional_Min(async);

            AssertSql(
                @"SELECT MIN([l0].[Level1_Required_Id])
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]");
        }

        public override async Task Result_operator_nav_prop_reference_optional_Max(bool async)
        {
            await base.Result_operator_nav_prop_reference_optional_Max(async);

            AssertSql(
                @"SELECT MAX([l0].[Level1_Required_Id])
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]");
        }

        public override async Task Result_operator_nav_prop_reference_optional_Average(bool async)
        {
            await base.Result_operator_nav_prop_reference_optional_Average(async);

            AssertSql(
                @"SELECT AVG(CAST([l0].[Level1_Required_Id] AS float))
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]");
        }

        public override async Task Result_operator_nav_prop_reference_optional_Average_with_identity_selector(bool async)
        {
            await base.Result_operator_nav_prop_reference_optional_Average_with_identity_selector(async);

            AssertSql(
                @"SELECT AVG(CAST([l0].[Level1_Required_Id] AS float))
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]");
        }

        public override async Task Result_operator_nav_prop_reference_optional_Average_without_selector(bool async)
        {
            await base.Result_operator_nav_prop_reference_optional_Average_without_selector(async);

            AssertSql(
                @"SELECT AVG(CAST([l0].[Level1_Required_Id] AS float))
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]");
        }

        public override async Task Result_operator_nav_prop_reference_optional_via_DefaultIfEmpty(bool async)
        {
            await base.Result_operator_nav_prop_reference_optional_via_DefaultIfEmpty(async);

            AssertSql(
                @"SELECT COALESCE(SUM(CASE
    WHEN [l0].[Id] IS NULL THEN 0
    ELSE [l0].[Level1_Required_Id]
END), 0)
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]");
        }

        public override async Task Include_with_optional_navigation(bool async)
        {
            await base.Include_with_optional_navigation(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
WHERE ([l0].[Name] <> N'L2 05') OR [l0].[Name] IS NULL");
        }

        public override async Task Join_flattening_bug_4539(bool async)
        {
            await base.Join_flattening_bug_4539(async);

            AssertSql(
                @"SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l2].[Id], [l2].[Date], [l2].[Name], [l2].[OneToMany_Optional_Self_Inverse1Id], [l2].[OneToMany_Required_Self_Inverse1Id], [l2].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
CROSS JOIN [LevelTwo] AS [l1]
INNER JOIN [LevelOne] AS [l2] ON [l1].[Level1_Required_Id] = [l2].[Id]");
        }

        public override async Task Query_source_materialization_bug_4547(bool async)
        {
            await base.Query_source_materialization_bug_4547(async);

            AssertSql(
                @"SELECT [l0].[Id]
FROM [LevelThree] AS [l]
INNER JOIN [LevelOne] AS [l0] ON [l].[Id] = (
    SELECT TOP(1) [l2].[Id]
    FROM [LevelTwo] AS [l1]
    LEFT JOIN [LevelThree] AS [l2] ON [l1].[Id] = [l2].[Level2_Optional_Id]
    ORDER BY [l2].[Id])");
        }

        public override async Task SelectMany_navigation_property(bool async)
        {
            await base.SelectMany_navigation_property(async);

            AssertSql(
                @"SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]");
        }

        public override async Task SelectMany_navigation_property_and_projection(bool async)
        {
            await base.SelectMany_navigation_property_and_projection(async);

            AssertSql(
                @"SELECT [l0].[Name]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]");
        }

        public override async Task SelectMany_navigation_property_and_filter_before(bool async)
        {
            await base.SelectMany_navigation_property_and_filter_before(async);

            AssertSql(
                @"SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
WHERE [l].[Id] = 1");
        }

        public override async Task SelectMany_navigation_property_and_filter_after(bool async)
        {
            await base.SelectMany_navigation_property_and_filter_after(async);

            AssertSql(
                @"SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
WHERE [l0].[Id] <> 6");
        }

        public override async Task SelectMany_nested_navigation_property_required(bool async)
        {
            await base.SelectMany_nested_navigation_property_required(async);

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Required_Id]
INNER JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]");
        }

        public override async Task SelectMany_nested_navigation_property_optional_and_projection(bool async)
        {
            await base.SelectMany_nested_navigation_property_optional_and_projection(async);

            AssertSql(
                @"SELECT [l1].[Name]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
INNER JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]");
        }

        public override async Task Multiple_SelectMany_calls(bool async)
        {
            await base.Multiple_SelectMany_calls(async);

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
INNER JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]");
        }

        public override async Task SelectMany_navigation_property_with_another_navigation_in_subquery(bool async)
        {
            await base.SelectMany_navigation_property_with_another_navigation_in_subquery(async);

            AssertSql(
                @"SELECT [t].[Id], [t].[Level2_Optional_Id], [t].[Level2_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse3Id], [t].[OneToMany_Optional_Self_Inverse3Id], [t].[OneToMany_Required_Inverse3Id], [t].[OneToMany_Required_Self_Inverse3Id], [t].[OneToOne_Optional_PK_Inverse3Id], [t].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l0].[OneToMany_Optional_Inverse2Id]
    FROM [LevelTwo] AS [l0]
    LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Optional_Id]
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]");
        }

        [ConditionalFact]
        public void Multiple_complex_includes_from_sql()
        {
            using var context = CreateContext();
            var query = context.LevelOne.FromSqlRaw("SELECT * FROM [LevelOne]")
                .Include(e => e.OneToOne_Optional_FK1)
                .ThenInclude(e => e.OneToMany_Optional2)
                .Include(e => e.OneToMany_Optional1)
                .ThenInclude(e => e.OneToOne_Optional_FK2);

            var results = query.ToList();

            Assert.Equal(13, results.Count);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id], [t].[Id0], [t].[Level2_Optional_Id], [t].[Level2_Required_Id], [t].[Name0], [t].[OneToMany_Optional_Inverse3Id], [t].[OneToMany_Optional_Self_Inverse3Id], [t].[OneToMany_Required_Inverse3Id], [t].[OneToMany_Required_Self_Inverse3Id], [t].[OneToOne_Optional_PK_Inverse3Id], [t].[OneToOne_Optional_Self3Id]
FROM (
    SELECT * FROM [LevelOne]
) AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id], [l3].[Id] AS [Id0], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Name] AS [Name0], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Optional_Self_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToMany_Required_Self_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id], [l3].[OneToOne_Optional_Self3Id]
    FROM [LevelTwo] AS [l2]
    LEFT JOIN [LevelThree] AS [l3] ON [l2].[Id] = [l3].[Level2_Optional_Id]
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l0].[Id], [l1].[Id], [t].[Id], [t].[Id0]");
        }

        public override async Task Where_navigation_property_to_collection(bool async)
        {
            await base.Where_navigation_property_to_collection(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Required_Id]
WHERE (
    SELECT COUNT(*)
    FROM [LevelThree] AS [l1]
    WHERE [l0].[Id] IS NOT NULL AND ([l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id])) > 0");
        }

        public override async Task Where_navigation_property_to_collection2(bool async)
        {
            await base.Where_navigation_property_to_collection2(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Level2_Optional_Id], [l].[Level2_Required_Id], [l].[Name], [l].[OneToMany_Optional_Inverse3Id], [l].[OneToMany_Optional_Self_Inverse3Id], [l].[OneToMany_Required_Inverse3Id], [l].[OneToMany_Required_Self_Inverse3Id], [l].[OneToOne_Optional_PK_Inverse3Id], [l].[OneToOne_Optional_Self3Id]
FROM [LevelThree] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Level2_Required_Id] = [l0].[Id]
WHERE (
    SELECT COUNT(*)
    FROM [LevelThree] AS [l1]
    WHERE [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]) > 0");
        }

        public override async Task Where_navigation_property_to_collection_of_original_entity_type(bool async)
        {
            await base.Where_navigation_property_to_collection_of_original_entity_type(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Name], [l].[OneToMany_Optional_Inverse2Id], [l].[OneToMany_Optional_Self_Inverse2Id], [l].[OneToMany_Required_Inverse2Id], [l].[OneToMany_Required_Self_Inverse2Id], [l].[OneToOne_Optional_PK_Inverse2Id], [l].[OneToOne_Optional_Self2Id]
FROM [LevelTwo] AS [l]
INNER JOIN [LevelOne] AS [l0] ON [l].[OneToMany_Required_Inverse2Id] = [l0].[Id]
WHERE (
    SELECT COUNT(*)
    FROM [LevelTwo] AS [l1]
    WHERE [l0].[Id] = [l1].[OneToMany_Optional_Inverse2Id]) > 0");
        }

        public override async Task Correlated_subquery_doesnt_project_unnecessary_columns_in_top_level(bool async)
        {
            await base.Correlated_subquery_doesnt_project_unnecessary_columns_in_top_level(async);

            AssertSql(
                @"SELECT DISTINCT [l].[Name]
FROM [LevelOne] AS [l]
WHERE EXISTS (
    SELECT 1
    FROM [LevelTwo] AS [l0]
    WHERE [l0].[Level1_Required_Id] = [l].[Id])");
        }

        public override async Task Correlated_subquery_doesnt_project_unnecessary_columns_in_top_level_join(bool async)
        {
            await base.Correlated_subquery_doesnt_project_unnecessary_columns_in_top_level_join(async);

            AssertSql(
                @"SELECT [l].[Name] AS [Name1], [t].[Id] AS [Id2]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l1].[Id] AS [Id0]
    FROM [LevelTwo] AS [l0]
    LEFT JOIN [LevelOne] AS [l1] ON [l0].[Level1_Optional_Id] = [l1].[Id]
) AS [t] ON [l].[Id] = [t].[Id0]
WHERE EXISTS (
    SELECT 1
    FROM [LevelTwo] AS [l2]
    WHERE [l2].[Level1_Required_Id] = [l].[Id])");
        }

        public override async Task Correlated_nested_subquery_doesnt_project_unnecessary_columns_in_top_level(bool async)
        {
            await base.Correlated_nested_subquery_doesnt_project_unnecessary_columns_in_top_level(async);

            AssertSql(
                @"SELECT DISTINCT [l].[Name]
FROM [LevelOne] AS [l]
WHERE EXISTS (
    SELECT 1
    FROM [LevelTwo] AS [l0]
    WHERE EXISTS (
        SELECT 1
        FROM [LevelThree] AS [l1]))");
        }

        public override async Task Correlated_nested_two_levels_up_subquery_doesnt_project_unnecessary_columns_in_top_level(bool async)
        {
            await base.Correlated_nested_two_levels_up_subquery_doesnt_project_unnecessary_columns_in_top_level(async);

            AssertSql(
                @"SELECT DISTINCT [l].[Name]
FROM [LevelOne] AS [l]
WHERE EXISTS (
    SELECT 1
    FROM [LevelTwo] AS [l0]
    WHERE EXISTS (
        SELECT 1
        FROM [LevelThree] AS [l1]))");
        }

        public override async Task SelectMany_where_with_subquery(bool async)
        {
            await base.SelectMany_where_with_subquery(async);

            AssertSql(
                @"SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Required_Inverse2Id]
WHERE EXISTS (
    SELECT 1
    FROM [LevelThree] AS [l1]
    WHERE [l0].[Id] = [l1].[OneToMany_Required_Inverse3Id])");
        }

        public override async Task Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access1(bool async)
        {
            await base.Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access1(async);

            AssertSql(
                @"SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
FROM [LevelThree] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Level2_Required_Id] = [l0].[Id]
ORDER BY [l0].[Id]");
        }

        public override async Task Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access2(bool async)
        {
            await base.Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access2(async);

            AssertSql(
                @"SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
FROM [LevelThree] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Level2_Required_Id] = [l0].[Id]
ORDER BY [l0].[Id]");
        }

        public override async Task Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access3(bool async)
        {
            await base.Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access3(async);

            AssertSql(
                @"SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
FROM [LevelThree] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Level2_Required_Id] = [l0].[Id]
ORDER BY [l0].[Id]");
        }

        public override async Task Order_by_key_of_navigation_similar_to_projected_gets_optimized_into_FK_access(bool async)
        {
            await base.Order_by_key_of_navigation_similar_to_projected_gets_optimized_into_FK_access(async);

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_Inverse1Id], [l1].[OneToMany_Required_Self_Inverse1Id], [l1].[OneToOne_Optional_Self1Id]
FROM [LevelThree] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Level2_Required_Id] = [l0].[Id]
INNER JOIN [LevelOne] AS [l1] ON [l0].[Level1_Required_Id] = [l1].[Id]
ORDER BY [l0].[Id]");
        }

        public override async Task Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access_subquery(bool async)
        {
            await base.Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access_subquery(async);

            AssertSql(
                @"@__p_0='10'

SELECT [l1].[Name]
FROM (
    SELECT TOP(@__p_0) [l0].[Id] AS [Id0], [l0].[Level1_Required_Id]
    FROM [LevelThree] AS [l]
    INNER JOIN [LevelTwo] AS [l0] ON [l].[Level2_Required_Id] = [l0].[Id]
    ORDER BY [l0].[Id]
) AS [t]
INNER JOIN [LevelOne] AS [l1] ON [t].[Level1_Required_Id] = [l1].[Id]
ORDER BY [t].[Id0]");
        }

        public override async Task Order_by_key_of_anonymous_type_projected_navigation_doesnt_get_optimized_into_FK_access_subquery(
            bool async)
        {
            await base.Order_by_key_of_anonymous_type_projected_navigation_doesnt_get_optimized_into_FK_access_subquery(async);

            AssertSql(
                @"@__p_0='10'

SELECT TOP(@__p_0) [l0].[Name]
FROM [LevelThree] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Level2_Required_Id] = [l0].[Id]
ORDER BY [l0].[Id]");
        }

        public override async Task Optional_navigation_take_optional_navigation(bool async)
        {
            await base.Optional_navigation_take_optional_navigation(async);

            AssertSql(
                @"@__p_0='10'

SELECT [l1].[Name]
FROM (
    SELECT TOP(@__p_0) [l0].[Id] AS [Id0]
    FROM [LevelOne] AS [l]
    LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
    ORDER BY [l0].[Id]
) AS [t]
LEFT JOIN [LevelThree] AS [l1] ON [t].[Id0] = [l1].[Level2_Optional_Id]
ORDER BY [t].[Id0]");
        }

        public override async Task Projection_select_correct_table_from_subquery_when_materialization_is_not_required(bool async)
        {
            await base.Projection_select_correct_table_from_subquery_when_materialization_is_not_required(async);

            AssertSql(
                @"@__p_0='3'

SELECT TOP(@__p_0) [l].[Name]
FROM [LevelTwo] AS [l]
INNER JOIN [LevelOne] AS [l0] ON [l].[Level1_Required_Id] = [l0].[Id]
WHERE [l0].[Name] = N'L1 03'
ORDER BY [l].[Id]");
        }

        public override async Task Projection_select_correct_table_with_anonymous_projection_in_subquery(bool async)
        {
            await base.Projection_select_correct_table_with_anonymous_projection_in_subquery(async);

            AssertSql(
                @"@__p_0='3'

SELECT TOP(@__p_0) [l].[Name]
FROM [LevelTwo] AS [l]
INNER JOIN [LevelOne] AS [l0] ON [l].[Level1_Required_Id] = [l0].[Id]
INNER JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Required_Id]
WHERE ([l0].[Name] = N'L1 03') AND ([l1].[Name] = N'L3 08')
ORDER BY [l0].[Id]");
        }

        public override async Task Projection_select_correct_table_in_subquery_when_materialization_is_not_required_in_multiple_joins(
            bool async)
        {
            await base.Projection_select_correct_table_in_subquery_when_materialization_is_not_required_in_multiple_joins(async);

            AssertSql(
                @"@__p_0='3'

SELECT TOP(@__p_0) [l0].[Name]
FROM [LevelTwo] AS [l]
INNER JOIN [LevelOne] AS [l0] ON [l].[Level1_Required_Id] = [l0].[Id]
INNER JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Required_Id]
WHERE ([l0].[Name] = N'L1 03') AND ([l1].[Name] = N'L3 08')
ORDER BY [l0].[Id]");
        }

        public override async Task Where_predicate_on_optional_reference_navigation(bool async)
        {
            await base.Where_predicate_on_optional_reference_navigation(async);

            AssertSql(
                @"@__p_0='3'

SELECT TOP(@__p_0) [l].[Name]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Required_Id]
WHERE [l0].[Name] = N'L2 03'
ORDER BY [l].[Id]");
        }

        public override async Task SelectMany_with_string_based_Include1(bool async)
        {
            await base.SelectMany_with_string_based_Include1(async);

            AssertSql(
                @"SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Required_Id]");
        }

        public override async Task SelectMany_with_string_based_Include2(bool async)
        {
            await base.SelectMany_with_string_based_Include2(async);

            AssertSql(
                @"SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l2].[Id], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Optional_Self_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToMany_Required_Self_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id], [l2].[OneToOne_Optional_Self4Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Required_Id]
LEFT JOIN [LevelFour] AS [l2] ON [l1].[Id] = [l2].[Level3_Required_Id]");
        }

        public override async Task Multiple_SelectMany_with_string_based_Include(bool async)
        {
            await base.Multiple_SelectMany_with_string_based_Include(async);

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l2].[Id], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Optional_Self_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToMany_Required_Self_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id], [l2].[OneToOne_Optional_Self4Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
INNER JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
LEFT JOIN [LevelFour] AS [l2] ON [l1].[Id] = [l2].[Level3_Required_Id]");
        }

        public override async Task Multiple_required_navigations_with_Include(bool async)
        {
            await base.Multiple_required_navigations_with_Include(async);

            AssertSql(
                @"SELECT [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3].[Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3].[Date], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3].[Level1_Optional_Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3].[Level1_Required_Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3].[Name], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3].[OneToMany_Optional_Inverse2Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3].[OneToMany_Optional_Self_Inverse2Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3].[OneToMany_Required_Inverse2Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3].[OneToMany_Required_Self_Inverse2Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3].[OneToOne_Optional_PK_Inverse2Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3].[OneToOne_Optional_Self2Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK2].[Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK2].[Level2_Optional_Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK2].[Level2_Required_Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK2].[Name], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK2].[OneToMany_Optional_Inverse3Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK2].[OneToMany_Optional_Self_Inverse3Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK2].[OneToMany_Required_Inverse3Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK2].[OneToMany_Required_Self_Inverse3Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK2].[OneToOne_Optional_PK_Inverse3Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK2].[OneToOne_Optional_Self3Id]
FROM [LevelFour] AS [l4]
INNER JOIN [LevelThree] AS [l4.OneToOne_Required_FK_Inverse4] ON [l4].[Level3_Required_Id] = [l4.OneToOne_Required_FK_Inverse4].[Id]
INNER JOIN [LevelTwo] AS [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3] ON [l4.OneToOne_Required_FK_Inverse4].[Level2_Required_Id] = [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3].[Id]
LEFT JOIN [LevelThree] AS [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK2] ON [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3].[Id] = [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK2].[Level2_Optional_Id]");
        }

        public override async Task Multiple_required_navigation_using_multiple_selects_with_Include(bool async)
        {
            await base.Multiple_required_navigation_using_multiple_selects_with_Include(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Name], [l].[OneToMany_Optional_Inverse2Id], [l].[OneToMany_Optional_Self_Inverse2Id], [l].[OneToMany_Required_Inverse2Id], [l].[OneToMany_Required_Self_Inverse2Id], [l].[OneToOne_Optional_PK_Inverse2Id], [l].[OneToOne_Optional_Self2Id], [l0].[Id], [l0].[Level2_Optional_Id], [l0].[Level2_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse3Id], [l0].[OneToMany_Optional_Self_Inverse3Id], [l0].[OneToMany_Required_Inverse3Id], [l0].[OneToMany_Required_Self_Inverse3Id], [l0].[OneToOne_Optional_PK_Inverse3Id], [l0].[OneToOne_Optional_Self3Id]
FROM [LevelFour] AS [l1]
INNER JOIN [LevelThree] AS [l2] ON [l1].[Level3_Required_Id] = [l2].[Id]
INNER JOIN [LevelTwo] AS [l] ON [l2].[Level2_Required_Id] = [l].[Id]
LEFT JOIN [LevelThree] AS [l0] ON [l].[Id] = [l0].[Level2_Optional_Id]");
        }

        public override async Task Multiple_required_navigation_with_string_based_Include(bool async)
        {
            await base.Multiple_required_navigation_with_string_based_Include(async);

            AssertSql(
                @"SELECT [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3].[Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3].[Date], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3].[Level1_Optional_Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3].[Level1_Required_Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3].[Name], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3].[OneToMany_Optional_Inverse2Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3].[OneToMany_Optional_Self_Inverse2Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3].[OneToMany_Required_Inverse2Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3].[OneToMany_Required_Self_Inverse2Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3].[OneToOne_Optional_PK_Inverse2Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3].[OneToOne_Optional_Self2Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK2].[Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK2].[Level2_Optional_Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK2].[Level2_Required_Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK2].[Name], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK2].[OneToMany_Optional_Inverse3Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK2].[OneToMany_Optional_Self_Inverse3Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK2].[OneToMany_Required_Inverse3Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK2].[OneToMany_Required_Self_Inverse3Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK2].[OneToOne_Optional_PK_Inverse3Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK2].[OneToOne_Optional_Self3Id]
FROM [LevelFour] AS [l4]
INNER JOIN [LevelThree] AS [l4.OneToOne_Required_FK_Inverse4] ON [l4].[Level3_Required_Id] = [l4.OneToOne_Required_FK_Inverse4].[Id]
INNER JOIN [LevelTwo] AS [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3] ON [l4.OneToOne_Required_FK_Inverse4].[Level2_Required_Id] = [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3].[Id]
LEFT JOIN [LevelThree] AS [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK2] ON [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3].[Id] = [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK2].[Level2_Optional_Id]");
        }

        public override async Task Multiple_required_navigation_using_multiple_selects_with_string_based_Include(bool async)
        {
            await base.Multiple_required_navigation_using_multiple_selects_with_string_based_Include(async);

            AssertSql(
                @"SELECT [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3].[Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3].[Date], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3].[Level1_Optional_Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3].[Level1_Required_Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3].[Name], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3].[OneToMany_Optional_Inverse2Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3].[OneToMany_Optional_Self_Inverse2Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3].[OneToMany_Required_Inverse2Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3].[OneToMany_Required_Self_Inverse2Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3].[OneToOne_Optional_PK_Inverse2Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3].[OneToOne_Optional_Self2Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK2].[Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK2].[Level2_Optional_Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK2].[Level2_Required_Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK2].[Name], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK2].[OneToMany_Optional_Inverse3Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK2].[OneToMany_Optional_Self_Inverse3Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK2].[OneToMany_Required_Inverse3Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK2].[OneToMany_Required_Self_Inverse3Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK2].[OneToOne_Optional_PK_Inverse3Id], [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK2].[OneToOne_Optional_Self3Id]
FROM [LevelFour] AS [l4]
INNER JOIN [LevelThree] AS [l4.OneToOne_Required_FK_Inverse4] ON [l4].[Level3_Required_Id] = [l4.OneToOne_Required_FK_Inverse4].[Id]
INNER JOIN [LevelTwo] AS [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3] ON [l4.OneToOne_Required_FK_Inverse4].[Level2_Required_Id] = [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3].[Id]
LEFT JOIN [LevelThree] AS [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK2] ON [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3].[Id] = [l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK2].[Level2_Optional_Id]");
        }

        public override async Task Optional_navigation_with_Include(bool async)
        {
            await base.Optional_navigation_with_Include(async);

            AssertSql(
                @"SELECT [l1.OneToOne_Optional_FK1].[Id], [l1.OneToOne_Optional_FK1].[Date], [l1.OneToOne_Optional_FK1].[Level1_Optional_Id], [l1.OneToOne_Optional_FK1].[Level1_Required_Id], [l1.OneToOne_Optional_FK1].[Name], [l1.OneToOne_Optional_FK1].[OneToMany_Optional_Inverse2Id], [l1.OneToOne_Optional_FK1].[OneToMany_Optional_Self_Inverse2Id], [l1.OneToOne_Optional_FK1].[OneToMany_Required_Inverse2Id], [l1.OneToOne_Optional_FK1].[OneToMany_Required_Self_Inverse2Id], [l1.OneToOne_Optional_FK1].[OneToOne_Optional_PK_Inverse2Id], [l1.OneToOne_Optional_FK1].[OneToOne_Optional_Self2Id], [l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2].[Id], [l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2].[Level2_Optional_Id], [l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2].[Level2_Required_Id], [l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2].[Name], [l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2].[OneToMany_Optional_Inverse3Id], [l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2].[OneToMany_Optional_Self_Inverse3Id], [l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2].[OneToMany_Required_Inverse3Id], [l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2].[OneToMany_Required_Self_Inverse3Id], [l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2].[OneToOne_Optional_PK_Inverse3Id], [l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l1]
LEFT JOIN [LevelTwo] AS [l1.OneToOne_Optional_FK1] ON [l1].[Id] = [l1.OneToOne_Optional_FK1].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2] ON [l1.OneToOne_Optional_FK1].[Id] = [l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2].[Level2_Optional_Id]");
        }

        public override async Task SelectMany_with_navigation_and_explicit_DefaultIfEmpty(bool async)
        {
            await base.SelectMany_with_navigation_and_explicit_DefaultIfEmpty(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
WHERE [l0].[Id] IS NOT NULL");
        }

        public override async Task SelectMany_with_navigation_filter_and_explicit_DefaultIfEmpty(bool async)
        {
            await base.SelectMany_with_navigation_filter_and_explicit_DefaultIfEmpty(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToMany_Optional_Inverse2Id]
    FROM [LevelTwo] AS [l0]
    WHERE [l0].[Id] > 5
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
WHERE [t].[Id] IS NOT NULL");
        }

        public override async Task SelectMany_with_nested_navigation_and_explicit_DefaultIfEmpty(bool async)
        {
            await base.SelectMany_with_nested_navigation_and_explicit_DefaultIfEmpty(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Required_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
WHERE [l1].[Id] IS NOT NULL");
        }

        public override async Task SelectMany_with_nested_navigation_filter_and_explicit_DefaultIfEmpty(bool async)
        {
            await base.SelectMany_with_nested_navigation_filter_and_explicit_DefaultIfEmpty(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l1].[Id], [l1].[OneToMany_Optional_Inverse3Id]
    FROM [LevelThree] AS [l1]
    WHERE [l1].[Id] > 5
) AS [t] ON [l0].[Id] = [t].[OneToMany_Optional_Inverse3Id]
WHERE [t].[Id] IS NOT NULL");
        }

        public override async Task SelectMany_with_nested_required_navigation_filter_and_explicit_DefaultIfEmpty(bool async)
        {
            await base.SelectMany_with_nested_required_navigation_filter_and_explicit_DefaultIfEmpty(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Required_Id]
LEFT JOIN (
    SELECT [l1].[Id], [l1].[OneToMany_Required_Inverse3Id]
    FROM [LevelThree] AS [l1]
    WHERE [l1].[Id] > 5
) AS [t] ON [l0].[Id] = [t].[OneToMany_Required_Inverse3Id]
WHERE [t].[Id] IS NOT NULL");
        }

        public override async Task SelectMany_with_nested_navigations_and_additional_joins_outside_of_SelectMany(bool async)
        {
            await base.SelectMany_with_nested_navigations_and_additional_joins_outside_of_SelectMany(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [t].[Id2], [t].[Date0], [t].[Level1_Optional_Id0], [t].[Level1_Required_Id0], [t].[Name2], [t].[OneToMany_Optional_Inverse2Id0], [t].[OneToMany_Optional_Self_Inverse2Id0], [t].[OneToMany_Required_Inverse2Id0], [t].[OneToMany_Required_Self_Inverse2Id0], [t].[OneToOne_Optional_PK_Inverse2Id0], [t].[OneToOne_Optional_Self2Id0]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l3].[Id] AS [Id2], [l3].[Date] AS [Date0], [l3].[Level1_Optional_Id] AS [Level1_Optional_Id0], [l3].[Level1_Required_Id] AS [Level1_Required_Id0], [l3].[Name] AS [Name2], [l3].[OneToMany_Optional_Inverse2Id] AS [OneToMany_Optional_Inverse2Id0], [l3].[OneToMany_Optional_Self_Inverse2Id] AS [OneToMany_Optional_Self_Inverse2Id0], [l3].[OneToMany_Required_Inverse2Id] AS [OneToMany_Required_Inverse2Id0], [l3].[OneToMany_Required_Self_Inverse2Id] AS [OneToMany_Required_Self_Inverse2Id0], [l3].[OneToOne_Optional_PK_Inverse2Id] AS [OneToOne_Optional_PK_Inverse2Id0], [l3].[OneToOne_Optional_Self2Id] AS [OneToOne_Optional_Self2Id0]
    FROM [LevelFour] AS [l0]
    INNER JOIN [LevelThree] AS [l1] ON [l0].[Level3_Required_Id] = [l1].[Id]
    LEFT JOIN [LevelTwo] AS [l2] ON [l1].[Level2_Optional_Id] = [l2].[Id]
    INNER JOIN [LevelTwo] AS [l3] ON [l2].[Id] = [l3].[OneToMany_Required_Self_Inverse2Id]
) AS [t] ON [l].[Id] = [t].[Level1_Optional_Id0]");
        }

        public override async Task SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany(
            bool async)
        {
            await base.SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [t].[Id2], [t].[Date0], [t].[Level1_Optional_Id0], [t].[Level1_Required_Id0], [t].[Name2], [t].[OneToMany_Optional_Inverse2Id0], [t].[OneToMany_Optional_Self_Inverse2Id0], [t].[OneToMany_Required_Inverse2Id0], [t].[OneToMany_Required_Self_Inverse2Id0], [t].[OneToOne_Optional_PK_Inverse2Id0], [t].[OneToOne_Optional_Self2Id0]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l3].[Id] AS [Id2], [l3].[Date] AS [Date0], [l3].[Level1_Optional_Id] AS [Level1_Optional_Id0], [l3].[Level1_Required_Id] AS [Level1_Required_Id0], [l3].[Name] AS [Name2], [l3].[OneToMany_Optional_Inverse2Id] AS [OneToMany_Optional_Inverse2Id0], [l3].[OneToMany_Optional_Self_Inverse2Id] AS [OneToMany_Optional_Self_Inverse2Id0], [l3].[OneToMany_Required_Inverse2Id] AS [OneToMany_Required_Inverse2Id0], [l3].[OneToMany_Required_Self_Inverse2Id] AS [OneToMany_Required_Self_Inverse2Id0], [l3].[OneToOne_Optional_PK_Inverse2Id] AS [OneToOne_Optional_PK_Inverse2Id0], [l3].[OneToOne_Optional_Self2Id] AS [OneToOne_Optional_Self2Id0]
    FROM [LevelFour] AS [l0]
    INNER JOIN [LevelThree] AS [l1] ON [l0].[Level3_Required_Id] = [l1].[Id]
    LEFT JOIN [LevelTwo] AS [l2] ON [l1].[Level2_Optional_Id] = [l2].[Id]
    LEFT JOIN [LevelTwo] AS [l3] ON [l2].[Id] = [l3].[OneToMany_Required_Self_Inverse2Id]
) AS [t] ON [l].[Id] = [t].[Level1_Optional_Id0]");
        }

        public override async Task SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany2(
            bool async)
        {
            await base.SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany2(async);

            AssertSql(
                @"SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id], [l3].[Id], [l3].[Date], [l3].[Name], [l3].[OneToMany_Optional_Self_Inverse1Id], [l3].[OneToMany_Required_Self_Inverse1Id], [l3].[OneToOne_Optional_Self1Id]
FROM [LevelFour] AS [l]
INNER JOIN [LevelThree] AS [l0] ON [l].[Level3_Required_Id] = [l0].[Id]
LEFT JOIN [LevelTwo] AS [l1] ON [l0].[Level2_Optional_Id] = [l1].[Id]
LEFT JOIN [LevelTwo] AS [l2] ON [l1].[Id] = [l2].[OneToMany_Required_Self_Inverse2Id]
INNER JOIN [LevelOne] AS [l3] ON [l2].[Level1_Optional_Id] = [l3].[Id]");
        }

        public override async Task SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany3(
            bool async)
        {
            await base.SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany3(async);

            AssertSql(
                @"SELECT [l2].[Id], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Optional_Self_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToMany_Required_Self_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id], [l2].[OneToOne_Optional_Self4Id], [l3].[Id], [l3].[Date], [l3].[Level1_Optional_Id], [l3].[Level1_Required_Id], [l3].[Name], [l3].[OneToMany_Optional_Inverse2Id], [l3].[OneToMany_Optional_Self_Inverse2Id], [l3].[OneToMany_Required_Inverse2Id], [l3].[OneToMany_Required_Self_Inverse2Id], [l3].[OneToOne_Optional_PK_Inverse2Id], [l3].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Required_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Optional_Id]
LEFT JOIN [LevelFour] AS [l2] ON [l1].[Id] = [l2].[OneToMany_Required_Inverse4Id]
INNER JOIN [LevelTwo] AS [l3] ON [l2].[Id] = [l3].[Id]");
        }

        public override async Task SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany4(
            bool async)
        {
            await base.SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany4(async);

            AssertSql(
                @"SELECT [l2].[Id], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Optional_Self_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToMany_Required_Self_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id], [l2].[OneToOne_Optional_Self4Id], [l3].[Id], [l3].[Date], [l3].[Level1_Optional_Id], [l3].[Level1_Required_Id], [l3].[Name], [l3].[OneToMany_Optional_Inverse2Id], [l3].[OneToMany_Optional_Self_Inverse2Id], [l3].[OneToMany_Required_Inverse2Id], [l3].[OneToMany_Required_Self_Inverse2Id], [l3].[OneToOne_Optional_PK_Inverse2Id], [l3].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Required_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Optional_Id]
LEFT JOIN [LevelFour] AS [l2] ON [l1].[Id] = [l2].[OneToMany_Required_Inverse4Id]
LEFT JOIN [LevelTwo] AS [l3] ON [l2].[Id] = [l3].[Id]");
        }

        public override async Task Multiple_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_joined_together(bool async)
        {
            await base.Multiple_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_joined_together(async);

            AssertSql(
                @"SELECT [l2].[Id], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Optional_Self_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToMany_Required_Self_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id], [l2].[OneToOne_Optional_Self4Id], [t].[Id2], [t].[Date0], [t].[Level1_Optional_Id0], [t].[Level1_Required_Id0], [t].[Name2], [t].[OneToMany_Optional_Inverse2Id0], [t].[OneToMany_Optional_Self_Inverse2Id0], [t].[OneToMany_Required_Inverse2Id0], [t].[OneToMany_Required_Self_Inverse2Id0], [t].[OneToOne_Optional_PK_Inverse2Id0], [t].[OneToOne_Optional_Self2Id0]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Required_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Optional_Id]
LEFT JOIN [LevelFour] AS [l2] ON [l1].[Id] = [l2].[OneToMany_Required_Inverse4Id]
INNER JOIN (
    SELECT [l6].[Id] AS [Id2], [l6].[Date] AS [Date0], [l6].[Level1_Optional_Id] AS [Level1_Optional_Id0], [l6].[Level1_Required_Id] AS [Level1_Required_Id0], [l6].[Name] AS [Name2], [l6].[OneToMany_Optional_Inverse2Id] AS [OneToMany_Optional_Inverse2Id0], [l6].[OneToMany_Optional_Self_Inverse2Id] AS [OneToMany_Optional_Self_Inverse2Id0], [l6].[OneToMany_Required_Inverse2Id] AS [OneToMany_Required_Inverse2Id0], [l6].[OneToMany_Required_Self_Inverse2Id] AS [OneToMany_Required_Self_Inverse2Id0], [l6].[OneToOne_Optional_PK_Inverse2Id] AS [OneToOne_Optional_PK_Inverse2Id0], [l6].[OneToOne_Optional_Self2Id] AS [OneToOne_Optional_Self2Id0]
    FROM [LevelFour] AS [l3]
    INNER JOIN [LevelThree] AS [l4] ON [l3].[Level3_Required_Id] = [l4].[Id]
    LEFT JOIN [LevelTwo] AS [l5] ON [l4].[Level2_Optional_Id] = [l5].[Id]
    LEFT JOIN [LevelTwo] AS [l6] ON [l5].[Id] = [l6].[OneToMany_Required_Self_Inverse2Id]
) AS [t] ON [l2].[Id] = [t].[Id2]");
        }

        public override async Task
            SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_followed_by_Select_required_navigation_using_same_navs(
                bool async)
        {
            await base
                .SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_followed_by_Select_required_navigation_using_same_navs(
                    async);

            AssertSql(
                @"SELECT [l4].[Id], [l4].[Date], [l4].[Name], [l4].[OneToMany_Optional_Self_Inverse1Id], [l4].[OneToMany_Required_Self_Inverse1Id], [l4].[OneToOne_Optional_Self1Id]
FROM [LevelFour] AS [l]
INNER JOIN [LevelThree] AS [l0] ON [l].[Level3_Required_Id] = [l0].[Id]
INNER JOIN [LevelTwo] AS [l1] ON [l0].[Level2_Required_Id] = [l1].[Id]
LEFT JOIN [LevelThree] AS [l2] ON [l1].[Id] = [l2].[OneToMany_Required_Inverse3Id]
LEFT JOIN [LevelTwo] AS [l3] ON [l2].[Level2_Required_Id] = [l3].[Id]
LEFT JOIN [LevelOne] AS [l4] ON [l3].[Id] = [l4].[Id]");
        }

        public override async Task
            SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_followed_by_Select_required_navigation_using_different_navs(
                bool async)
        {
            await base
                .SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_followed_by_Select_required_navigation_using_different_navs(
                    async);

            AssertSql(
                @"SELECT [l3].[Id], [l3].[Date], [l3].[Name], [l3].[OneToMany_Optional_Self_Inverse1Id], [l3].[OneToMany_Required_Self_Inverse1Id], [l3].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
LEFT JOIN [LevelTwo] AS [l2] ON [l1].[Level2_Required_Id] = [l2].[Id]
LEFT JOIN [LevelOne] AS [l3] ON [l2].[Id] = [l3].[Id]");
        }

        public override async Task Multiple_SelectMany_with_navigation_and_explicit_DefaultIfEmpty(bool async)
        {
            await base.Multiple_SelectMany_with_navigation_and_explicit_DefaultIfEmpty(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
LEFT JOIN (
    SELECT [l1].[Id], [l1].[OneToMany_Optional_Inverse3Id]
    FROM [LevelThree] AS [l1]
    WHERE [l1].[Id] > 5
) AS [t] ON [l0].[Id] = [t].[OneToMany_Optional_Inverse3Id]
WHERE [t].[Id] IS NOT NULL");
        }

        public override async Task SelectMany_with_navigation_filter_paging_and_explicit_DefaultIfEmpty(bool async)
        {
            await base.SelectMany_with_navigation_filter_paging_and_explicit_DefaultIfEmpty(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [t].[Id], [t].[OneToMany_Required_Inverse2Id]
    FROM (
        SELECT [l0].[Id], [l0].[OneToMany_Required_Inverse2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Required_Inverse2Id] ORDER BY [l0].[Id]) AS [row]
        FROM [LevelTwo] AS [l0]
        WHERE [l0].[Id] > 5
    ) AS [t]
    WHERE [t].[row] <= 3
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Required_Inverse2Id]
WHERE [t0].[Id] IS NOT NULL");
        }

        public override async Task Select_join_subquery_containing_filter_and_distinct(bool async)
        {
            await base.Select_join_subquery_containing_filter_and_distinct(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT DISTINCT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
    FROM [LevelTwo] AS [l0]
    WHERE [l0].[Id] > 2
) AS [t] ON [l].[Id] = [t].[Level1_Optional_Id]");
        }

        public override async Task Select_join_with_key_selector_being_a_subquery(bool async)
        {
            await base.Select_join_with_key_selector_being_a_subquery(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = COALESCE((
    SELECT TOP(1) [l1].[Id]
    FROM [LevelTwo] AS [l1]
    ORDER BY [l1].[Id]), 0)");
        }

        public override async Task Contains_with_subquery_optional_navigation_and_constant_item(bool async)
        {
            await base.Contains_with_subquery_optional_navigation_and_constant_item(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT DISTINCT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
        FROM [LevelThree] AS [l1]
        WHERE [l0].[Id] IS NOT NULL AND ([l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id])
    ) AS [t]
    WHERE [t].[Id] = 1)");
        }

        public override async Task Contains_with_subquery_optional_navigation_scalar_distinct_and_constant_item(bool async)
        {
            await base.Contains_with_subquery_optional_navigation_scalar_distinct_and_constant_item(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
WHERE EXISTS (
    SELECT DISTINCT 1
    FROM [LevelThree] AS [l1]
    WHERE ([l0].[Id] IS NOT NULL AND ([l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id])) AND (CAST(LEN([l1].[Name]) AS int) = 1))");
        }

        public override async Task Required_navigation_on_a_subquery_with_First_in_projection(bool async)
        {
            await base.Required_navigation_on_a_subquery_with_First_in_projection(async);

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [l1].[Name]
    FROM [LevelTwo] AS [l0]
    INNER JOIN [LevelOne] AS [l1] ON [l0].[Level1_Required_Id] = [l1].[Id]
    ORDER BY [l0].[Id])
FROM [LevelTwo] AS [l]
WHERE [l].[Id] = 7");
        }

        public override async Task Required_navigation_on_a_subquery_with_complex_projection_and_First(bool async)
        {
            await base.Required_navigation_on_a_subquery_with_complex_projection_and_First(async);

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [l2].[Name]
    FROM [LevelTwo] AS [l0]
    INNER JOIN [LevelOne] AS [l1] ON [l0].[Level1_Required_Id] = [l1].[Id]
    INNER JOIN [LevelOne] AS [l2] ON [l0].[Level1_Required_Id] = [l2].[Id]
    ORDER BY [l0].[Id])
FROM [LevelTwo] AS [l]
WHERE [l].[Id] = 7");
        }

        public override async Task Required_navigation_on_a_subquery_with_First_in_predicate(bool async)
        {
            await base.Required_navigation_on_a_subquery_with_First_in_predicate(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Name], [l].[OneToMany_Optional_Inverse2Id], [l].[OneToMany_Optional_Self_Inverse2Id], [l].[OneToMany_Required_Inverse2Id], [l].[OneToMany_Required_Self_Inverse2Id], [l].[OneToOne_Optional_PK_Inverse2Id], [l].[OneToOne_Optional_Self2Id]
FROM [LevelTwo] AS [l]
WHERE ([l].[Id] = 7) AND ((
    SELECT TOP(1) [l1].[Name]
    FROM [LevelTwo] AS [l0]
    INNER JOIN [LevelOne] AS [l1] ON [l0].[Level1_Required_Id] = [l1].[Id]
    ORDER BY [l0].[Id]) = N'L1 02')");
        }

        public override async Task Manually_created_left_join_propagates_nullability_to_navigations(bool async)
        {
            await base.Manually_created_left_join_propagates_nullability_to_navigations(async);

            AssertSql(
                @"SELECT [l1].[Name]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelOne] AS [l1] ON [l0].[Level1_Required_Id] = [l1].[Id]
WHERE ([l1].[Name] <> N'L3 02') OR [l1].[Name] IS NULL");
        }

        public override async Task Optional_navigation_propagates_nullability_to_manually_created_left_join1(bool async)
        {
            await base.Optional_navigation_propagates_nullability_to_manually_created_left_join1(async);

            AssertSql(
                @"SELECT [l0].[Id] AS [Id1], [l1].[Id] AS [Id2]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelTwo] AS [l1] ON [l0].[Level1_Required_Id] = [l1].[Id]");
        }

        public override async Task Optional_navigation_propagates_nullability_to_manually_created_left_join2(bool async)
        {
            await base.Optional_navigation_propagates_nullability_to_manually_created_left_join2(async);

            AssertSql(
                @"SELECT [l].[Name] AS [Name1], [t].[Name0] AS [Name2]
FROM [LevelThree] AS [l]
LEFT JOIN (
    SELECT [l1].[Id] AS [Id0], [l1].[Name] AS [Name0]
    FROM [LevelOne] AS [l0]
    LEFT JOIN [LevelTwo] AS [l1] ON [l0].[Id] = [l1].[Level1_Optional_Id]
) AS [t] ON [l].[Level2_Required_Id] = [t].[Id0]");
        }

        public override async Task Null_reference_protection_complex(bool async)
        {
            await base.Null_reference_protection_complex(async);

            AssertSql(
                @"SELECT [t].[Name0]
FROM [LevelThree] AS [l]
LEFT JOIN (
    SELECT [l1].[Id] AS [Id0], [l1].[Name] AS [Name0]
    FROM [LevelOne] AS [l0]
    LEFT JOIN [LevelTwo] AS [l1] ON [l0].[Id] = [l1].[Level1_Optional_Id]
) AS [t] ON [l].[Level2_Required_Id] = [t].[Id0]");
        }

        public override async Task Null_reference_protection_complex_materialization(bool async)
        {
            await base.Null_reference_protection_complex_materialization(async);

            AssertSql(
                @"SELECT [t].[Id0], [t].[Date0], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name0], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id]
FROM [LevelThree] AS [l]
LEFT JOIN (
    SELECT [l1].[Id] AS [Id0], [l1].[Date] AS [Date0], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name] AS [Name0], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id]
    FROM [LevelOne] AS [l0]
    LEFT JOIN [LevelTwo] AS [l1] ON [l0].[Id] = [l1].[Level1_Optional_Id]
) AS [t] ON [l].[Level2_Required_Id] = [t].[Id0]");
        }

        public override async Task Null_reference_protection_complex_client_eval(bool async)
        {
            await base.Null_reference_protection_complex_client_eval(async);

            AssertSql(
                @"SELECT [t].[Name0]
FROM [LevelThree] AS [l]
LEFT JOIN (
    SELECT [l1].[Id] AS [Id0], [l1].[Name] AS [Name0]
    FROM [LevelOne] AS [l0]
    LEFT JOIN [LevelTwo] AS [l1] ON [l0].[Id] = [l1].[Level1_Optional_Id]
) AS [t] ON [l].[Level2_Required_Id] = [t].[Id0]");
        }

        public override async Task GroupJoin_with_complex_subquery_with_joins_does_not_get_flattened(bool async)
        {
            await base.GroupJoin_with_complex_subquery_with_joins_does_not_get_flattened(async);

            AssertSql(
                @"SELECT [t].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[Level1_Optional_Id]
    FROM [LevelTwo] AS [l0]
    INNER JOIN [LevelOne] AS [l1] ON [l0].[Level1_Required_Id] = [l1].[Id]
) AS [t] ON [l].[Id] = [t].[Level1_Optional_Id]");
        }

        public override async Task GroupJoin_with_complex_subquery_with_joins_does_not_get_flattened2(bool async)
        {
            await base.GroupJoin_with_complex_subquery_with_joins_does_not_get_flattened2(async);

            AssertSql(
                @"SELECT [t].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[Level1_Optional_Id]
    FROM [LevelTwo] AS [l0]
    INNER JOIN [LevelOne] AS [l1] ON [l0].[Level1_Required_Id] = [l1].[Id]
) AS [t] ON [l].[Id] = [t].[Level1_Optional_Id]");
        }

        public override async Task GroupJoin_with_complex_subquery_with_joins_does_not_get_flattened3(bool async)
        {
            await base.GroupJoin_with_complex_subquery_with_joins_does_not_get_flattened3(async);

            AssertSql(
                @"SELECT [t].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[Level1_Required_Id]
    FROM [LevelTwo] AS [l0]
    LEFT JOIN [LevelOne] AS [l1] ON [l0].[Level1_Required_Id] = [l1].[Id]
) AS [t] ON [l].[Id] = [t].[Level1_Required_Id]");
        }

        public override async Task GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_outer(bool async)
        {
            await base.GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_outer(async);

            AssertSql(
                @"@__p_0='2'

SELECT [l1].[Name]
FROM (
    SELECT TOP(@__p_0) [l].[Id]
    FROM [LevelOne] AS [l]
    LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
    ORDER BY [l].[Id]
) AS [t]
LEFT JOIN [LevelTwo] AS [l1] ON [t].[Id] = [l1].[Level1_Optional_Id]
ORDER BY [t].[Id]");
        }

        public override async Task GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_outer_with_client_method(bool async)
        {
            await base.GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_outer_with_client_method(async);

            AssertSql(
                @"SELECT [l2_outer].[Level1_Optional_Id], [l2_outer].[Name]
FROM [LevelTwo] AS [l2_outer]",
                //
                @"@__p_0='2'

SELECT TOP(@__p_0) [l10].[Id], [l10].[Date], [l10].[Name], [l10].[OneToMany_Optional_Self_Inverse1Id], [l10].[OneToMany_Required_Self_Inverse1Id], [l10].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l10]
LEFT JOIN [LevelTwo] AS [l20] ON [l10].[Id] = [l20].[Level1_Optional_Id]
ORDER BY [l10].[Id]");
        }

        public override async Task GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_inner(bool async)
        {
            await base.GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_inner(async);

            AssertSql(
                @"@__p_0='2'

SELECT [l1].[Name]
FROM (
    SELECT TOP(@__p_0) [l].[Id], [l0].[Level1_Optional_Id]
    FROM [LevelOne] AS [l]
    LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
    ORDER BY [l].[Id]
) AS [t]
LEFT JOIN [LevelOne] AS [l1] ON [t].[Level1_Optional_Id] = [l1].[Id]
ORDER BY [t].[Id]");
        }

        public override async Task GroupJoin_on_a_subquery_containing_another_GroupJoin_with_orderby_on_inner_sequence_projecting_inner(
            bool async)
        {
            await base.GroupJoin_on_a_subquery_containing_another_GroupJoin_with_orderby_on_inner_sequence_projecting_inner(async);

            AssertSql(
                @"@__p_0='2'

SELECT [l1].[Name]
FROM (
    SELECT TOP(@__p_0) [l].[Id], [t].[Level1_Optional_Id]
    FROM [LevelOne] AS [l]
    LEFT JOIN (
        SELECT [l0].[Level1_Optional_Id]
        FROM [LevelTwo] AS [l0]
    ) AS [t] ON [l].[Id] = [t].[Level1_Optional_Id]
    ORDER BY [l].[Id]
) AS [t0]
LEFT JOIN [LevelOne] AS [l1] ON [t0].[Level1_Optional_Id] = [l1].[Id]
ORDER BY [t0].[Id]");
        }

        public override async Task GroupJoin_on_left_side_being_a_subquery(bool async)
        {
            await base.GroupJoin_on_left_side_being_a_subquery(async);

            AssertSql(
                @"@__p_0='2'

SELECT TOP(@__p_0) [l].[Id], [l0].[Name] AS [Brand]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
ORDER BY [l0].[Name], [l].[Id]");
        }

        public override async Task GroupJoin_on_right_side_being_a_subquery(bool async)
        {
            await base.GroupJoin_on_right_side_being_a_subquery(async);

            AssertSql(
                @"@__p_0='2'

SELECT [l].[Id], [t].[Name]
FROM [LevelTwo] AS [l]
LEFT JOIN (
    SELECT TOP(@__p_0) [l0].[Id], [l0].[Name]
    FROM [LevelOne] AS [l0]
    LEFT JOIN [LevelTwo] AS [l1] ON [l0].[Id] = [l1].[Level1_Optional_Id]
    ORDER BY [l1].[Name]
) AS [t] ON [l].[Level1_Optional_Id] = [t].[Id]");
        }

        public override async Task GroupJoin_in_subquery_with_client_result_operator(bool async)
        {
            await base.GroupJoin_in_subquery_with_client_result_operator(async);

            AssertSql(
                @"SELECT [l].[Name]
FROM [LevelOne] AS [l]
WHERE ((
    SELECT COUNT(*)
    FROM (
        SELECT DISTINCT [l0].[Id], [l0].[Date], [l0].[Name], [l0].[OneToMany_Optional_Self_Inverse1Id], [l0].[OneToMany_Required_Self_Inverse1Id], [l0].[OneToOne_Optional_Self1Id]
        FROM [LevelOne] AS [l0]
        LEFT JOIN [LevelTwo] AS [l1] ON [l0].[Id] = [l1].[Level1_Optional_Id]
    ) AS [t]) > 7) AND ([l].[Id] < 3)");
        }

        public override async Task GroupJoin_in_subquery_with_client_projection(bool async)
        {
            await base.GroupJoin_in_subquery_with_client_projection(async);

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Name]
FROM [LevelOne] AS [l1]
WHERE [l1].[Id] < 3",
                //
                @"SELECT COUNT(*)
FROM [LevelOne] AS [l1_inner0]
LEFT JOIN [LevelTwo] AS [l2_inner0] ON [l1_inner0].[Id] = [l2_inner0].[Level1_Optional_Id]",
                //
                @"SELECT COUNT(*)
FROM [LevelOne] AS [l1_inner0]
LEFT JOIN [LevelTwo] AS [l2_inner0] ON [l1_inner0].[Id] = [l2_inner0].[Level1_Optional_Id]");
        }

        public override async Task GroupJoin_in_subquery_with_client_projection_nested1(bool async)
        {
            await base.GroupJoin_in_subquery_with_client_projection_nested1(async);

            AssertSql(
                @"SELECT [l1_outer].[Id], [l1_outer].[Name]
FROM [LevelOne] AS [l1_outer]
WHERE [l1_outer].[Id] < 2",
                //
                @"SELECT 1
FROM [LevelOne] AS [l1_middle0]
LEFT JOIN [LevelTwo] AS [l2_middle0] ON [l1_middle0].[Id] = [l2_middle0].[Level1_Optional_Id]
ORDER BY [l1_middle0].[Id]",
                //
                @"SELECT COUNT(*)
FROM [LevelOne] AS [l1_inner2]
LEFT JOIN [LevelTwo] AS [l2_inner2] ON [l1_inner2].[Id] = [l2_inner2].[Level1_Optional_Id]",
                //
                @"SELECT COUNT(*)
FROM [LevelOne] AS [l1_inner2]
LEFT JOIN [LevelTwo] AS [l2_inner2] ON [l1_inner2].[Id] = [l2_inner2].[Level1_Optional_Id]");
        }

        public override async Task GroupJoin_in_subquery_with_client_projection_nested2(bool async)
        {
            await base.GroupJoin_in_subquery_with_client_projection_nested2(async);

            AssertSql(
                @"SELECT [l1_outer].[Id], [l1_outer].[Name]
FROM [LevelOne] AS [l1_outer]
WHERE [l1_outer].[Id] < 2",
                //
                @"SELECT COUNT(*)
FROM [LevelOne] AS [l1_middle0]
LEFT JOIN [LevelTwo] AS [l2_middle0] ON [l1_middle0].[Id] = [l2_middle0].[Level1_Optional_Id]
WHERE (
    SELECT COUNT(*)
    FROM [LevelOne] AS [l1_inner0]
    LEFT JOIN [LevelTwo] AS [l2_inner0] ON [l1_inner0].[Id] = [l2_inner0].[Level1_Optional_Id]
) > 7");
        }

        public override async Task GroupJoin_client_method_on_outer(bool async)
        {
            await base.GroupJoin_client_method_on_outer(async);

            AssertSql(
                @"SELECT [l].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]");
        }

        public override async Task GroupJoin_client_method_in_OrderBy(bool async)
        {
            await base.GroupJoin_client_method_in_OrderBy(async);

            AssertSql(
                @"SELECT [l1].[Id], [l2].[Id]
FROM [LevelOne] AS [l1]
LEFT JOIN [LevelTwo] AS [l2] ON [l1].[Id] = [l2].[Level1_Optional_Id]");
        }

        public override async Task GroupJoin_without_DefaultIfEmpty(bool async)
        {
            await base.GroupJoin_without_DefaultIfEmpty(async);

            AssertSql(
                @"SELECT [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]");
        }

        public override async Task GroupJoin_with_subquery_on_inner(bool async)
        {
            await base.GroupJoin_with_subquery_on_inner(async);

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_Inverse1Id], [l1].[OneToMany_Required_Self_Inverse1Id], [l1].[OneToOne_Optional_Self1Id], [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l1]
LEFT JOIN [LevelTwo] AS [l2] ON [l1].[Id] = [l2].[Level1_Optional_Id]
ORDER BY [l1].[Id]");
        }

        public override async Task GroupJoin_with_subquery_on_inner_and_no_DefaultIfEmpty(bool async)
        {
            await base.GroupJoin_with_subquery_on_inner_and_no_DefaultIfEmpty(async);

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_Inverse1Id], [l1].[OneToMany_Required_Self_Inverse1Id], [l1].[OneToOne_Optional_Self1Id], [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l1]
LEFT JOIN [LevelTwo] AS [l2] ON [l1].[Id] = [l2].[Level1_Optional_Id]
ORDER BY [l1].[Id]");
        }

        public override async Task Optional_navigation_in_subquery_with_unrelated_projection(bool async)
        {
            await base.Optional_navigation_in_subquery_with_unrelated_projection(async);

            AssertSql(
                @"@__p_0='15'

SELECT TOP(@__p_0) [l].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
WHERE ([l0].[Name] <> N'Foo') OR [l0].[Name] IS NULL
ORDER BY [l].[Id]");
        }

        public override async Task Explicit_GroupJoin_in_subquery_with_unrelated_projection(bool async)
        {
            await base.Explicit_GroupJoin_in_subquery_with_unrelated_projection(async);

            AssertSql(
                @"@__p_0='15'

SELECT TOP(@__p_0) [l].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
WHERE ([l0].[Name] <> N'Foo') OR [l0].[Name] IS NULL
ORDER BY [l].[Id]");
        }

        public override async Task Explicit_GroupJoin_in_subquery_with_unrelated_projection2(bool async)
        {
            await base.Explicit_GroupJoin_in_subquery_with_unrelated_projection2(async);

            AssertSql(
                @"SELECT [t].[Id]
FROM (
    SELECT DISTINCT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
    FROM [LevelOne] AS [l]
    LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
    WHERE ([l0].[Name] <> N'Foo') OR [l0].[Name] IS NULL
) AS [t]");
        }

        public override async Task Explicit_GroupJoin_in_subquery_with_unrelated_projection3(bool async)
        {
            await base.Explicit_GroupJoin_in_subquery_with_unrelated_projection3(async);

            AssertSql(
                @"SELECT DISTINCT [l].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
WHERE ([l0].[Name] <> N'Foo') OR [l0].[Name] IS NULL");
        }

        public override async Task Explicit_GroupJoin_in_subquery_with_unrelated_projection4(bool async)
        {
            await base.Explicit_GroupJoin_in_subquery_with_unrelated_projection4(async);

            AssertSql(
                @"@__p_0='20'

SELECT TOP(@__p_0) [t].[Id]
FROM (
    SELECT DISTINCT [l].[Id]
    FROM [LevelOne] AS [l]
    LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
    WHERE ([l0].[Name] <> N'Foo') OR [l0].[Name] IS NULL
) AS [t]
ORDER BY [t].[Id]");
        }

        public override async Task Explicit_GroupJoin_in_subquery_with_scalar_result_operator(bool async)
        {
            await base.Explicit_GroupJoin_in_subquery_with_scalar_result_operator(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
WHERE (
    SELECT COUNT(*)
    FROM [LevelOne] AS [l0]
    LEFT JOIN [LevelTwo] AS [l1] ON [l0].[Id] = [l1].[Level1_Optional_Id]) > 4");
        }

        public override async Task Explicit_GroupJoin_in_subquery_with_multiple_result_operator_distinct_count_materializes_main_clause(
            bool async)
        {
            await base.Explicit_GroupJoin_in_subquery_with_multiple_result_operator_distinct_count_materializes_main_clause(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT DISTINCT [l0].[Id], [l0].[Date], [l0].[Name], [l0].[OneToMany_Optional_Self_Inverse1Id], [l0].[OneToMany_Required_Self_Inverse1Id], [l0].[OneToOne_Optional_Self1Id]
        FROM [LevelOne] AS [l0]
        LEFT JOIN [LevelTwo] AS [l1] ON [l0].[Id] = [l1].[Level1_Optional_Id]
    ) AS [t]) > 4");
        }

        public override async Task Where_on_multilevel_reference_in_subquery_with_outer_projection(bool async)
        {
            await base.Where_on_multilevel_reference_in_subquery_with_outer_projection(async);

            AssertSql(
                @"@__p_0='0'
@__p_1='10'

SELECT [l].[Name]
FROM [LevelThree] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[OneToMany_Required_Inverse3Id] = [l0].[Id]
INNER JOIN [LevelOne] AS [l1] ON [l0].[Level1_Required_Id] = [l1].[Id]
WHERE [l1].[Name] = N'L1 03'
ORDER BY [l].[Level2_Required_Id]
OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY");
        }

        public override async Task Join_condition_optimizations_applied_correctly_when_anonymous_type_with_single_property(bool async)
        {
            await base.Join_condition_optimizations_applied_correctly_when_anonymous_type_with_single_property(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON ([l].[OneToMany_Optional_Self_Inverse1Id] = [l0].[Level1_Optional_Id]) OR ([l].[OneToMany_Optional_Self_Inverse1Id] IS NULL AND [l0].[Level1_Optional_Id] IS NULL)");
        }

        public override async Task Join_condition_optimizations_applied_correctly_when_anonymous_type_with_multiple_properties(bool async)
        {
            await base.Join_condition_optimizations_applied_correctly_when_anonymous_type_with_multiple_properties(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON (([l].[OneToMany_Optional_Self_Inverse1Id] = [l0].[Level1_Optional_Id]) OR ([l].[OneToMany_Optional_Self_Inverse1Id] IS NULL AND [l0].[Level1_Optional_Id] IS NULL)) AND (([l].[OneToOne_Optional_Self1Id] = [l0].[OneToMany_Optional_Self_Inverse2Id]) OR ([l].[OneToOne_Optional_Self1Id] IS NULL AND [l0].[OneToMany_Optional_Self_Inverse2Id] IS NULL))");
        }

        public override async Task Nested_group_join_with_take(bool async)
        {
            await base.Nested_group_join_with_take(async);

            AssertSql(
                @"@__p_0='2'

SELECT [l1].[Name]
FROM (
    SELECT TOP(@__p_0) [l].[Id], [l0].[Id] AS [Id0]
    FROM [LevelOne] AS [l]
    LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
    ORDER BY [l].[Id]
) AS [t]
LEFT JOIN [LevelTwo] AS [l1] ON [t].[Id0] = [l1].[Level1_Optional_Id]
ORDER BY [t].[Id]");
        }

        public override async Task Navigation_with_same_navigation_compared_to_null(bool async)
        {
            await base.Navigation_with_same_navigation_compared_to_null(async);

            AssertSql(
                @"SELECT [l].[Id]
FROM [LevelTwo] AS [l]
INNER JOIN [LevelOne] AS [l0] ON [l].[OneToMany_Required_Inverse2Id] = [l0].[Id]
WHERE ([l0].[Name] <> N'L1 07') OR [l0].[Name] IS NULL");
        }

        public override async Task Multi_level_navigation_compared_to_null(bool async)
        {
            await base.Multi_level_navigation_compared_to_null(async);

            AssertSql(
                @"SELECT [l].[Id]
FROM [LevelThree] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[OneToMany_Optional_Inverse3Id] = [l0].[Id]
LEFT JOIN [LevelOne] AS [l1] ON [l0].[Level1_Required_Id] = [l1].[Id]
WHERE [l1].[Id] IS NOT NULL");
        }

        public override async Task Multi_level_navigation_with_same_navigation_compared_to_null(bool async)
        {
            await base.Multi_level_navigation_with_same_navigation_compared_to_null(async);

            AssertSql(
                @"SELECT [l].[Id]
FROM [LevelThree] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[OneToMany_Optional_Inverse3Id] = [l0].[Id]
LEFT JOIN [LevelOne] AS [l1] ON [l0].[Level1_Required_Id] = [l1].[Id]
WHERE (([l1].[Name] <> N'L1 07') OR [l1].[Name] IS NULL) AND [l1].[Id] IS NOT NULL");
        }

        public override async Task Navigations_compared_to_each_other1(bool async)
        {
            await base.Navigations_compared_to_each_other1(async);

            AssertSql(
                @"SELECT [l].[Name]
FROM [LevelTwo] AS [l]
INNER JOIN [LevelOne] AS [l0] ON [l].[OneToMany_Required_Inverse2Id] = [l0].[Id]");
        }

        public override async Task Navigations_compared_to_each_other2(bool async)
        {
            await base.Navigations_compared_to_each_other2(async);

            AssertSql(
                @"SELECT [l].[Name]
FROM [LevelTwo] AS [l]
INNER JOIN [LevelOne] AS [l0] ON [l].[OneToMany_Required_Inverse2Id] = [l0].[Id]
LEFT JOIN [LevelOne] AS [l1] ON [l].[OneToOne_Optional_PK_Inverse2Id] = [l1].[Id]
WHERE [l0].[Id] = [l1].[Id]");
        }

        public override async Task Navigations_compared_to_each_other3(bool async)
        {
            await base.Navigations_compared_to_each_other3(async);

            AssertSql(
                @"SELECT [l].[Name]
FROM [LevelTwo] AS [l]
WHERE EXISTS (
    SELECT 1
    FROM [LevelThree] AS [l0]
    WHERE [l].[Id] = [l0].[OneToMany_Optional_Inverse3Id])");
        }

        public override async Task Navigations_compared_to_each_other4(bool async)
        {
            await base.Navigations_compared_to_each_other4(async);

            AssertSql(
                @"SELECT [l].[Name]
FROM [LevelTwo] AS [l]
LEFT JOIN [LevelThree] AS [l0] ON [l].[Id] = [l0].[Level2_Required_Id]
WHERE EXISTS (
    SELECT 1
    FROM [LevelFour] AS [l1]
    WHERE [l0].[Id] IS NOT NULL AND ([l0].[Id] = [l1].[OneToMany_Optional_Inverse4Id]))");
        }

        public override async Task Navigations_compared_to_each_other5(bool async)
        {
            await base.Navigations_compared_to_each_other5(async);

            AssertSql(
                @"SELECT [l].[Name]
FROM [LevelTwo] AS [l]
LEFT JOIN [LevelThree] AS [l0] ON [l].[Id] = [l0].[Level2_Required_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l].[Id] = [l1].[OneToOne_Optional_PK_Inverse3Id]
WHERE EXISTS (
    SELECT 1
    FROM [LevelFour] AS [l2]
    WHERE [l0].[Id] IS NOT NULL AND ([l0].[Id] = [l2].[OneToMany_Optional_Inverse4Id]))");
        }

        public override async Task Level4_Include(bool async)
        {
            await base.Level4_Include(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Name], [l].[OneToMany_Optional_Inverse2Id], [l].[OneToMany_Optional_Self_Inverse2Id], [l].[OneToMany_Required_Inverse2Id], [l].[OneToMany_Required_Self_Inverse2Id], [l].[OneToOne_Optional_PK_Inverse2Id], [l].[OneToOne_Optional_Self2Id], [l0].[Id], [l0].[Level2_Optional_Id], [l0].[Level2_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse3Id], [l0].[OneToMany_Optional_Self_Inverse3Id], [l0].[OneToMany_Required_Inverse3Id], [l0].[OneToMany_Required_Self_Inverse3Id], [l0].[OneToOne_Optional_PK_Inverse3Id], [l0].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l1]
LEFT JOIN [LevelTwo] AS [l2] ON [l1].[Id] = [l2].[Id]
LEFT JOIN [LevelThree] AS [l3] ON [l2].[Id] = [l3].[Id]
LEFT JOIN [LevelFour] AS [l4] ON [l3].[Id] = [l4].[Id]
LEFT JOIN [LevelThree] AS [l5] ON [l4].[Level3_Required_Id] = [l5].[Id]
LEFT JOIN [LevelTwo] AS [l] ON [l5].[Level2_Required_Id] = [l].[Id]
LEFT JOIN [LevelThree] AS [l0] ON [l].[Id] = [l0].[Level2_Optional_Id]
WHERE ([l2].[Id] IS NOT NULL AND [l3].[Id] IS NOT NULL) AND [l4].[Id] IS NOT NULL");
        }

        public override async Task Comparing_collection_navigation_on_optional_reference_to_null(bool async)
        {
            await base.Comparing_collection_navigation_on_optional_reference_to_null(async);

            AssertSql(
                @"SELECT [l].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
WHERE [l0].[Id] IS NULL");
        }

        public override async Task Select_subquery_with_client_eval_and_navigation1(bool async)
        {
            await base.Select_subquery_with_client_eval_and_navigation1(async);

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [l1].[Name]
    FROM [LevelTwo] AS [l0]
    INNER JOIN [LevelOne] AS [l1] ON [l0].[Level1_Required_Id] = [l1].[Id]
    ORDER BY [l0].[Id])
FROM [LevelTwo] AS [l]");
        }

        public override async Task Select_subquery_with_client_eval_and_navigation2(bool async)
        {
            await base.Select_subquery_with_client_eval_and_navigation2(async);

            AssertSql(
                @"SELECT CASE
    WHEN ((
        SELECT TOP(1) [l1].[Name]
        FROM [LevelTwo] AS [l0]
        INNER JOIN [LevelOne] AS [l1] ON [l0].[Level1_Required_Id] = [l1].[Id]
        ORDER BY [l0].[Id]) = N'L1 02') AND (
        SELECT TOP(1) [l1].[Name]
        FROM [LevelTwo] AS [l0]
        INNER JOIN [LevelOne] AS [l1] ON [l0].[Level1_Required_Id] = [l1].[Id]
        ORDER BY [l0].[Id]) IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [LevelTwo] AS [l]");
        }

        public override async Task Select_subquery_with_client_eval_and_multi_level_navigation(bool async)
        {
            await base.Select_subquery_with_client_eval_and_multi_level_navigation(async);

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [l2].[Name]
    FROM [LevelThree] AS [l0]
    INNER JOIN [LevelTwo] AS [l1] ON [l0].[Level2_Required_Id] = [l1].[Id]
    INNER JOIN [LevelOne] AS [l2] ON [l1].[Level1_Required_Id] = [l2].[Id]
    ORDER BY [l0].[Id])
FROM [LevelThree] AS [l]");
        }

        public override async Task Member_doesnt_get_pushed_down_into_subquery_with_result_operator(bool async)
        {
            await base.Member_doesnt_get_pushed_down_into_subquery_with_result_operator(async);

            AssertSql(
                @"SELECT (
    SELECT [t].[Name]
    FROM (
        SELECT DISTINCT [l0].[Id], [l0].[Level2_Optional_Id], [l0].[Level2_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse3Id], [l0].[OneToMany_Optional_Self_Inverse3Id], [l0].[OneToMany_Required_Inverse3Id], [l0].[OneToMany_Required_Self_Inverse3Id], [l0].[OneToOne_Optional_PK_Inverse3Id], [l0].[OneToOne_Optional_Self3Id]
        FROM [LevelThree] AS [l0]
    ) AS [t]
    ORDER BY [t].[Id]
    OFFSET 1 ROWS FETCH NEXT 1 ROWS ONLY)
FROM [LevelOne] AS [l]
WHERE [l].[Id] < 3");
        }

        public override async Task Subquery_with_Distinct_Skip_FirstOrDefault_without_OrderBy(bool async)
        {
            await base.Subquery_with_Distinct_Skip_FirstOrDefault_without_OrderBy(async);

            AssertSql(
                "");
        }

        public override async Task Project_collection_navigation_count(bool async)
        {
            await base.Project_collection_navigation_count(async);

            AssertSql(
                @"SELECT [l].[Id], (
    SELECT COUNT(*)
    FROM [LevelThree] AS [l1]
    WHERE [l0].[Id] IS NOT NULL AND ([l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id])) AS [Count]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]");
        }

        public override async Task Select_optional_navigation_property_string_concat(bool async)
        {
            await base.Select_optional_navigation_property_string_concat(async);

            AssertSql(
                @"SELECT (COALESCE([l].[Name], N'') + N' ') + COALESCE(CASE
    WHEN [t].[Id] IS NOT NULL THEN [t].[Name]
    ELSE N'NULL'
END, N'')
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id]
    FROM [LevelTwo] AS [l0]
    WHERE [l0].[Id] > 5
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]");
        }

        public override async Task String_include_multiple_derived_navigation_with_same_name_and_same_type(bool async)
        {
            await base.String_include_multiple_derived_navigation_with_same_name_and_same_type(async);

            AssertSql(
                @"SELECT [i].[Id], [i].[Discriminator], [i].[InheritanceBase2Id], [i].[InheritanceBase2Id1], [i].[Name], [i0].[Id], [i0].[DifferentTypeReference_InheritanceDerived1Id], [i0].[InheritanceDerived1Id], [i0].[InheritanceDerived1Id1], [i0].[InheritanceDerived2Id], [i0].[Name], [i0].[SameTypeReference_InheritanceDerived1Id], [i0].[SameTypeReference_InheritanceDerived2Id], [i1].[Id], [i1].[DifferentTypeReference_InheritanceDerived1Id], [i1].[InheritanceDerived1Id], [i1].[InheritanceDerived1Id1], [i1].[InheritanceDerived2Id], [i1].[Name], [i1].[SameTypeReference_InheritanceDerived1Id], [i1].[SameTypeReference_InheritanceDerived2Id]
FROM [InheritanceOne] AS [i]
LEFT JOIN [InheritanceLeafOne] AS [i0] ON [i].[Id] = [i0].[SameTypeReference_InheritanceDerived1Id]
LEFT JOIN [InheritanceLeafOne] AS [i1] ON [i].[Id] = [i1].[SameTypeReference_InheritanceDerived2Id]");
        }

        public override async Task String_include_multiple_derived_navigation_with_same_name_and_different_type(bool async)
        {
            await base.String_include_multiple_derived_navigation_with_same_name_and_different_type(async);

            AssertSql(
                @"SELECT [i].[Id], [i].[Discriminator], [i].[InheritanceBase2Id], [i].[InheritanceBase2Id1], [i].[Name], [i0].[Id], [i0].[DifferentTypeReference_InheritanceDerived1Id], [i0].[InheritanceDerived1Id], [i0].[InheritanceDerived1Id1], [i0].[InheritanceDerived2Id], [i0].[Name], [i0].[SameTypeReference_InheritanceDerived1Id], [i0].[SameTypeReference_InheritanceDerived2Id], [i1].[Id], [i1].[DifferentTypeReference_InheritanceDerived2Id], [i1].[InheritanceDerived2Id], [i1].[Name]
FROM [InheritanceOne] AS [i]
LEFT JOIN [InheritanceLeafOne] AS [i0] ON [i].[Id] = [i0].[DifferentTypeReference_InheritanceDerived1Id]
LEFT JOIN [InheritanceLeafTwo] AS [i1] ON [i].[Id] = [i1].[DifferentTypeReference_InheritanceDerived2Id]");
        }

        public override async Task
            String_include_multiple_derived_navigation_with_same_name_and_different_type_nested_also_includes_partially_matching_navigation_chains(
                bool async)
        {
            await base
                .String_include_multiple_derived_navigation_with_same_name_and_different_type_nested_also_includes_partially_matching_navigation_chains(
                    async);

            AssertSql(
                @"SELECT [i].[Id], [i].[Discriminator], [i].[InheritanceBase2Id], [i].[InheritanceBase2Id1], [i].[Name], [i0].[Id], [i0].[DifferentTypeReference_InheritanceDerived1Id], [i0].[InheritanceDerived1Id], [i0].[InheritanceDerived1Id1], [i0].[InheritanceDerived2Id], [i0].[Name], [i0].[SameTypeReference_InheritanceDerived1Id], [i0].[SameTypeReference_InheritanceDerived2Id], [i1].[Id], [i1].[DifferentTypeReference_InheritanceDerived2Id], [i1].[InheritanceDerived2Id], [i1].[Name], [i2].[Id], [i2].[InheritanceLeaf2Id], [i2].[Name]
FROM [InheritanceOne] AS [i]
LEFT JOIN [InheritanceLeafOne] AS [i0] ON [i].[Id] = [i0].[DifferentTypeReference_InheritanceDerived1Id]
LEFT JOIN [InheritanceLeafTwo] AS [i1] ON [i].[Id] = [i1].[DifferentTypeReference_InheritanceDerived2Id]
LEFT JOIN [InheritanceTwo] AS [i2] ON [i1].[Id] = [i2].[InheritanceLeaf2Id]
ORDER BY [i].[Id], [i0].[Id], [i1].[Id], [i2].[Id]");
        }

        public override async Task String_include_multiple_derived_collection_navigation_with_same_name_and_same_type(bool async)
        {
            await base.String_include_multiple_derived_collection_navigation_with_same_name_and_same_type(async);

            AssertSql(
                @"SELECT [i].[Id], [i].[Discriminator], [i].[InheritanceBase2Id], [i].[InheritanceBase2Id1], [i].[Name], [i0].[Id], [i0].[DifferentTypeReference_InheritanceDerived1Id], [i0].[InheritanceDerived1Id], [i0].[InheritanceDerived1Id1], [i0].[InheritanceDerived2Id], [i0].[Name], [i0].[SameTypeReference_InheritanceDerived1Id], [i0].[SameTypeReference_InheritanceDerived2Id], [i1].[Id], [i1].[DifferentTypeReference_InheritanceDerived1Id], [i1].[InheritanceDerived1Id], [i1].[InheritanceDerived1Id1], [i1].[InheritanceDerived2Id], [i1].[Name], [i1].[SameTypeReference_InheritanceDerived1Id], [i1].[SameTypeReference_InheritanceDerived2Id]
FROM [InheritanceOne] AS [i]
LEFT JOIN [InheritanceLeafOne] AS [i0] ON [i].[Id] = [i0].[InheritanceDerived1Id1]
LEFT JOIN [InheritanceLeafOne] AS [i1] ON [i].[Id] = [i1].[InheritanceDerived2Id]
ORDER BY [i].[Id], [i0].[Id], [i1].[Id]");
        }

        public override async Task String_include_multiple_derived_collection_navigation_with_same_name_and_different_type(bool async)
        {
            await base.String_include_multiple_derived_collection_navigation_with_same_name_and_different_type(async);

            AssertSql(
                @"SELECT [i].[Id], [i].[Discriminator], [i].[InheritanceBase2Id], [i].[InheritanceBase2Id1], [i].[Name], [i0].[Id], [i0].[DifferentTypeReference_InheritanceDerived1Id], [i0].[InheritanceDerived1Id], [i0].[InheritanceDerived1Id1], [i0].[InheritanceDerived2Id], [i0].[Name], [i0].[SameTypeReference_InheritanceDerived1Id], [i0].[SameTypeReference_InheritanceDerived2Id], [i1].[Id], [i1].[DifferentTypeReference_InheritanceDerived2Id], [i1].[InheritanceDerived2Id], [i1].[Name]
FROM [InheritanceOne] AS [i]
LEFT JOIN [InheritanceLeafOne] AS [i0] ON [i].[Id] = [i0].[InheritanceDerived1Id]
LEFT JOIN [InheritanceLeafTwo] AS [i1] ON [i].[Id] = [i1].[InheritanceDerived2Id]
ORDER BY [i].[Id], [i0].[Id], [i1].[Id]");
        }

        public override async Task
            String_include_multiple_derived_collection_navigation_with_same_name_and_different_type_nested_also_includes_partially_matching_navigation_chains(
                bool async)
        {
            await base
                .String_include_multiple_derived_collection_navigation_with_same_name_and_different_type_nested_also_includes_partially_matching_navigation_chains(
                    async);

            AssertSql(
                @"SELECT [i].[Id], [i].[Discriminator], [i].[InheritanceBase2Id], [i].[InheritanceBase2Id1], [i].[Name], [i0].[Id], [i0].[DifferentTypeReference_InheritanceDerived1Id], [i0].[InheritanceDerived1Id], [i0].[InheritanceDerived1Id1], [i0].[InheritanceDerived2Id], [i0].[Name], [i0].[SameTypeReference_InheritanceDerived1Id], [i0].[SameTypeReference_InheritanceDerived2Id], [t].[Id], [t].[DifferentTypeReference_InheritanceDerived2Id], [t].[InheritanceDerived2Id], [t].[Name], [t].[Id0], [t].[InheritanceLeaf2Id], [t].[Name0]
FROM [InheritanceOne] AS [i]
LEFT JOIN [InheritanceLeafOne] AS [i0] ON [i].[Id] = [i0].[InheritanceDerived1Id]
LEFT JOIN (
    SELECT [i1].[Id], [i1].[DifferentTypeReference_InheritanceDerived2Id], [i1].[InheritanceDerived2Id], [i1].[Name], [i2].[Id] AS [Id0], [i2].[InheritanceLeaf2Id], [i2].[Name] AS [Name0]
    FROM [InheritanceLeafTwo] AS [i1]
    LEFT JOIN [InheritanceTwo] AS [i2] ON [i1].[Id] = [i2].[InheritanceLeaf2Id]
) AS [t] ON [i].[Id] = [t].[InheritanceDerived2Id]
ORDER BY [i].[Id], [i0].[Id], [t].[Id], [t].[Id0]");
        }

        public override async Task String_include_multiple_derived_navigations_complex(bool async)
        {
            await base.String_include_multiple_derived_navigations_complex(async);

            AssertSql(
                @"SELECT [i].[Id], [i].[InheritanceLeaf2Id], [i].[Name], [i0].[Id], [i0].[Discriminator], [i0].[InheritanceBase2Id], [i0].[InheritanceBase2Id1], [i0].[Name], [i1].[Id], [i1].[DifferentTypeReference_InheritanceDerived1Id], [i1].[InheritanceDerived1Id], [i1].[InheritanceDerived1Id1], [i1].[InheritanceDerived2Id], [i1].[Name], [i1].[SameTypeReference_InheritanceDerived1Id], [i1].[SameTypeReference_InheritanceDerived2Id], [i2].[Id], [i2].[DifferentTypeReference_InheritanceDerived2Id], [i2].[InheritanceDerived2Id], [i2].[Name], [t].[Id], [t].[Discriminator], [t].[InheritanceBase2Id], [t].[InheritanceBase2Id1], [t].[Name], [t].[Id0], [t].[DifferentTypeReference_InheritanceDerived1Id], [t].[InheritanceDerived1Id], [t].[InheritanceDerived1Id1], [t].[InheritanceDerived2Id], [t].[Name0], [t].[SameTypeReference_InheritanceDerived1Id], [t].[SameTypeReference_InheritanceDerived2Id], [t].[Id1], [t].[DifferentTypeReference_InheritanceDerived1Id0], [t].[InheritanceDerived1Id0], [t].[InheritanceDerived1Id10], [t].[InheritanceDerived2Id0], [t].[Name1], [t].[SameTypeReference_InheritanceDerived1Id0], [t].[SameTypeReference_InheritanceDerived2Id0]
FROM [InheritanceTwo] AS [i]
LEFT JOIN [InheritanceOne] AS [i0] ON [i].[Id] = [i0].[InheritanceBase2Id]
LEFT JOIN [InheritanceLeafOne] AS [i1] ON [i0].[Id] = [i1].[InheritanceDerived1Id]
LEFT JOIN [InheritanceLeafTwo] AS [i2] ON [i0].[Id] = [i2].[InheritanceDerived2Id]
LEFT JOIN (
    SELECT [i3].[Id], [i3].[Discriminator], [i3].[InheritanceBase2Id], [i3].[InheritanceBase2Id1], [i3].[Name], [i4].[Id] AS [Id0], [i4].[DifferentTypeReference_InheritanceDerived1Id], [i4].[InheritanceDerived1Id], [i4].[InheritanceDerived1Id1], [i4].[InheritanceDerived2Id], [i4].[Name] AS [Name0], [i4].[SameTypeReference_InheritanceDerived1Id], [i4].[SameTypeReference_InheritanceDerived2Id], [i5].[Id] AS [Id1], [i5].[DifferentTypeReference_InheritanceDerived1Id] AS [DifferentTypeReference_InheritanceDerived1Id0], [i5].[InheritanceDerived1Id] AS [InheritanceDerived1Id0], [i5].[InheritanceDerived1Id1] AS [InheritanceDerived1Id10], [i5].[InheritanceDerived2Id] AS [InheritanceDerived2Id0], [i5].[Name] AS [Name1], [i5].[SameTypeReference_InheritanceDerived1Id] AS [SameTypeReference_InheritanceDerived1Id0], [i5].[SameTypeReference_InheritanceDerived2Id] AS [SameTypeReference_InheritanceDerived2Id0]
    FROM [InheritanceOne] AS [i3]
    LEFT JOIN [InheritanceLeafOne] AS [i4] ON [i3].[Id] = [i4].[SameTypeReference_InheritanceDerived1Id]
    LEFT JOIN [InheritanceLeafOne] AS [i5] ON [i3].[Id] = [i5].[SameTypeReference_InheritanceDerived2Id]
) AS [t] ON [i].[Id] = [t].[InheritanceBase2Id1]
ORDER BY [i].[Id], [i0].[Id], [i1].[Id], [i2].[Id], [t].[Id], [t].[Id0], [t].[Id1]");
        }

        public override async Task Nav_rewrite_doesnt_apply_null_protection_for_function_arguments(bool async)
        {
            await base.Nav_rewrite_doesnt_apply_null_protection_for_function_arguments(async);

            AssertSql(
                @"SELECT [l0].[Level1_Required_Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToOne_Optional_PK_Inverse2Id]
WHERE [l0].[Id] IS NOT NULL");
        }

        public override async Task Accessing_optional_property_inside_result_operator_subquery(bool async)
        {
            await base.Accessing_optional_property_inside_result_operator_subquery(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
WHERE [l0].[Name] NOT IN (N'Name1', N'Name2') OR [l0].[Name] IS NULL");
        }

        public override async Task Include1(bool async)
        {
            await base.Include1(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]");
        }

        public override async Task Include2(bool async)
        {
            await base.Include2(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]");
        }

        public override async Task Include3(bool async)
        {
            await base.Include3(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelTwo] AS [l1] ON [l].[Id] = [l1].[OneToOne_Optional_PK_Inverse2Id]");
        }

        public override async Task Include4(bool async)
        {
            await base.Include4(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToOne_Optional_PK_Inverse3Id]");
        }

        public override async Task Include5(bool async)
        {
            await base.Include5(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToOne_Optional_PK_Inverse3Id]");
        }

        public override async Task Include6(bool async)
        {
            await base.Include6(async);

            AssertSql(
                @"SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToOne_Optional_PK_Inverse3Id]");
        }

        public override async Task Include7(bool async)
        {
            await base.Include7(async);

            AssertSql(
                @"SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToOne_Optional_PK_Inverse2Id]");
        }

        public override async Task Include8(bool async)
        {
            await base.Include8(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Name], [l].[OneToMany_Optional_Inverse2Id], [l].[OneToMany_Optional_Self_Inverse2Id], [l].[OneToMany_Required_Inverse2Id], [l].[OneToMany_Required_Self_Inverse2Id], [l].[OneToOne_Optional_PK_Inverse2Id], [l].[OneToOne_Optional_Self2Id], [l0].[Id], [l0].[Date], [l0].[Name], [l0].[OneToMany_Optional_Self_Inverse1Id], [l0].[OneToMany_Required_Self_Inverse1Id], [l0].[OneToOne_Optional_Self1Id]
FROM [LevelTwo] AS [l]
LEFT JOIN [LevelOne] AS [l0] ON [l].[Level1_Optional_Id] = [l0].[Id]
WHERE ([l0].[Name] <> N'Fubar') OR [l0].[Name] IS NULL");
        }

        public override async Task Include9(bool async)
        {
            await base.Include9(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Name], [l].[OneToMany_Optional_Inverse2Id], [l].[OneToMany_Optional_Self_Inverse2Id], [l].[OneToMany_Required_Inverse2Id], [l].[OneToMany_Required_Self_Inverse2Id], [l].[OneToOne_Optional_PK_Inverse2Id], [l].[OneToOne_Optional_Self2Id], [l0].[Id], [l0].[Date], [l0].[Name], [l0].[OneToMany_Optional_Self_Inverse1Id], [l0].[OneToMany_Required_Self_Inverse1Id], [l0].[OneToOne_Optional_Self1Id]
FROM [LevelTwo] AS [l]
LEFT JOIN [LevelOne] AS [l0] ON [l].[Level1_Optional_Id] = [l0].[Id]
WHERE ([l0].[Name] <> N'Fubar') OR [l0].[Name] IS NULL");
        }

        public override async Task Include10(bool async)
        {
            await base.Include10(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id], [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Optional_Self_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToMany_Required_Self_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id], [l3].[OneToOne_Optional_Self3Id], [l4].[Id], [l4].[Level3_Optional_Id], [l4].[Level3_Required_Id], [l4].[Name], [l4].[OneToMany_Optional_Inverse4Id], [l4].[OneToMany_Optional_Self_Inverse4Id], [l4].[OneToMany_Required_Inverse4Id], [l4].[OneToMany_Required_Self_Inverse4Id], [l4].[OneToOne_Optional_PK_Inverse4Id], [l4].[OneToOne_Optional_Self4Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToOne_Optional_PK_Inverse3Id]
LEFT JOIN [LevelTwo] AS [l2] ON [l].[Id] = [l2].[OneToOne_Optional_PK_Inverse2Id]
LEFT JOIN [LevelThree] AS [l3] ON [l2].[Id] = [l3].[Level2_Optional_Id]
LEFT JOIN [LevelFour] AS [l4] ON [l3].[Id] = [l4].[OneToOne_Optional_PK_Inverse4Id]");
        }

        public override async Task Include11(bool async)
        {
            await base.Include11(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Optional_Self_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToMany_Required_Self_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[OneToOne_Optional_Self3Id], [l3].[Id], [l3].[Date], [l3].[Level1_Optional_Id], [l3].[Level1_Required_Id], [l3].[Name], [l3].[OneToMany_Optional_Inverse2Id], [l3].[OneToMany_Optional_Self_Inverse2Id], [l3].[OneToMany_Required_Inverse2Id], [l3].[OneToMany_Required_Self_Inverse2Id], [l3].[OneToOne_Optional_PK_Inverse2Id], [l3].[OneToOne_Optional_Self2Id], [l4].[Id], [l4].[Level2_Optional_Id], [l4].[Level2_Required_Id], [l4].[Name], [l4].[OneToMany_Optional_Inverse3Id], [l4].[OneToMany_Optional_Self_Inverse3Id], [l4].[OneToMany_Required_Inverse3Id], [l4].[OneToMany_Required_Self_Inverse3Id], [l4].[OneToOne_Optional_PK_Inverse3Id], [l4].[OneToOne_Optional_Self3Id], [l5].[Id], [l5].[Level3_Optional_Id], [l5].[Level3_Required_Id], [l5].[Name], [l5].[OneToMany_Optional_Inverse4Id], [l5].[OneToMany_Optional_Self_Inverse4Id], [l5].[OneToMany_Required_Inverse4Id], [l5].[OneToMany_Required_Self_Inverse4Id], [l5].[OneToOne_Optional_PK_Inverse4Id], [l5].[OneToOne_Optional_Self4Id], [l6].[Id], [l6].[Level3_Optional_Id], [l6].[Level3_Required_Id], [l6].[Name], [l6].[OneToMany_Optional_Inverse4Id], [l6].[OneToMany_Optional_Self_Inverse4Id], [l6].[OneToMany_Required_Inverse4Id], [l6].[OneToMany_Required_Self_Inverse4Id], [l6].[OneToOne_Optional_PK_Inverse4Id], [l6].[OneToOne_Optional_Self4Id], [l7].[Id], [l7].[Level2_Optional_Id], [l7].[Level2_Required_Id], [l7].[Name], [l7].[OneToMany_Optional_Inverse3Id], [l7].[OneToMany_Optional_Self_Inverse3Id], [l7].[OneToMany_Required_Inverse3Id], [l7].[OneToMany_Required_Self_Inverse3Id], [l7].[OneToOne_Optional_PK_Inverse3Id], [l7].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Optional_Id]
LEFT JOIN [LevelThree] AS [l2] ON [l0].[Id] = [l2].[OneToOne_Optional_PK_Inverse3Id]
LEFT JOIN [LevelTwo] AS [l3] ON [l].[Id] = [l3].[OneToOne_Optional_PK_Inverse2Id]
LEFT JOIN [LevelThree] AS [l4] ON [l3].[Id] = [l4].[Level2_Optional_Id]
LEFT JOIN [LevelFour] AS [l5] ON [l4].[Id] = [l5].[Level3_Optional_Id]
LEFT JOIN [LevelFour] AS [l6] ON [l4].[Id] = [l6].[OneToOne_Optional_PK_Inverse4Id]
LEFT JOIN [LevelThree] AS [l7] ON [l3].[Id] = [l7].[OneToOne_Optional_PK_Inverse3Id]");
        }

        public override async Task Include12(bool async)
        {
            await base.Include12(async);

            AssertSql(
                @"SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Optional_Id]");
        }

        public override async Task Include13(bool async)
        {
            await base.Include13(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]");
        }

        public override async Task Include14(bool async)
        {
            await base.Include14(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Optional_Self_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToMany_Required_Self_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[OneToOne_Optional_Self3Id], [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelTwo] AS [l1] ON [l].[Id] = [l1].[OneToOne_Optional_PK_Inverse2Id]
LEFT JOIN [LevelThree] AS [l2] ON [l0].[Id] = [l2].[Level2_Optional_Id]");
        }

        public override void Include17()
        {
            base.Include17();

            AssertSql(
                @"SELECT [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id], [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Optional_Self_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToMany_Required_Self_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[OneToOne_Optional_Self3Id], [t].[Id0], [t].[Date0], [t].[Level1_Optional_Id0], [t].[Level1_Required_Id0], [t].[Name0], [t].[OneToMany_Optional_Inverse2Id0], [t].[OneToMany_Optional_Self_Inverse2Id0], [t].[OneToMany_Required_Inverse2Id0], [t].[OneToMany_Required_Self_Inverse2Id0], [t].[OneToOne_Optional_PK_Inverse2Id0], [t].[OneToOne_Optional_Self2Id0]
FROM (
    SELECT DISTINCT [l].[Id], [l].[Date], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Name], [l].[OneToMany_Optional_Inverse2Id], [l].[OneToMany_Optional_Self_Inverse2Id], [l].[OneToMany_Required_Inverse2Id], [l].[OneToMany_Required_Self_Inverse2Id], [l].[OneToOne_Optional_PK_Inverse2Id], [l].[OneToOne_Optional_Self2Id], [l0].[Id] AS [Id0], [l0].[Date] AS [Date0], [l0].[Level1_Optional_Id] AS [Level1_Optional_Id0], [l0].[Level1_Required_Id] AS [Level1_Required_Id0], [l0].[Name] AS [Name0], [l0].[OneToMany_Optional_Inverse2Id] AS [OneToMany_Optional_Inverse2Id0], [l0].[OneToMany_Optional_Self_Inverse2Id] AS [OneToMany_Optional_Self_Inverse2Id0], [l0].[OneToMany_Required_Inverse2Id] AS [OneToMany_Required_Inverse2Id0], [l0].[OneToMany_Required_Self_Inverse2Id] AS [OneToMany_Required_Self_Inverse2Id0], [l0].[OneToOne_Optional_PK_Inverse2Id] AS [OneToOne_Optional_PK_Inverse2Id0], [l0].[OneToOne_Optional_Self2Id] AS [OneToOne_Optional_Self2Id0]
    FROM [LevelOne] AS [l1]
    LEFT JOIN [LevelTwo] AS [l] ON [l1].[Id] = [l].[Level1_Optional_Id]
    LEFT JOIN [LevelTwo] AS [l0] ON [l1].[Id] = [l0].[OneToOne_Optional_PK_Inverse2Id]
) AS [t]
LEFT JOIN [LevelThree] AS [l2] ON [t].[Id] = [l2].[Level2_Optional_Id]");
        }

        public override async Task Include18_1(bool async)
        {
            await base.Include18_1(async);

            AssertSql(
                @"SELECT [t].[Id], [t].[Date], [t].[Name], [t].[OneToMany_Optional_Self_Inverse1Id], [t].[OneToMany_Required_Self_Inverse1Id], [t].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
FROM (
    SELECT DISTINCT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
    FROM [LevelOne] AS [l]
) AS [t]
LEFT JOIN [LevelTwo] AS [l0] ON [t].[Id] = [l0].[Level1_Optional_Id]");
        }

        public override async Task Include18_1_1(bool async)
        {
            await base.Include18_1_1(async);

            AssertSql(
                @"@__p_0='10'

SELECT [t].[Id], [t].[Date], [t].[Name], [t].[OneToMany_Optional_Self_Inverse1Id], [t].[OneToMany_Required_Self_Inverse1Id], [t].[OneToOne_Optional_Self1Id], [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id]
FROM (
    SELECT TOP(@__p_0) [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Name] AS [Name0]
    FROM [LevelOne] AS [l]
    LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Required_Id]
    ORDER BY [l0].[Name]
) AS [t]
LEFT JOIN [LevelTwo] AS [l1] ON [t].[Id] = [l1].[Level1_Optional_Id]
ORDER BY [t].[Name0]");
        }

        public override async Task Include18_2(bool async)
        {
            await base.Include18_2(async);

            AssertSql(
                @"SELECT [t].[Id], [t].[Date], [t].[Name], [t].[OneToMany_Optional_Self_Inverse1Id], [t].[OneToMany_Required_Self_Inverse1Id], [t].[OneToOne_Optional_Self1Id], [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id]
FROM (
    SELECT DISTINCT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
    FROM [LevelOne] AS [l]
    LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Required_Id]
    WHERE ([l0].[Name] <> N'Foo') OR [l0].[Name] IS NULL
) AS [t]
LEFT JOIN [LevelTwo] AS [l1] ON [t].[Id] = [l1].[Level1_Optional_Id]");
        }

        public override void Include18_3()
        {
            base.Include18_3();

            // issue #15783
            AssertSql(
                @"@__p_0='10'

SELECT [t].[Id], [t].[Date], [t].[Name], [t].[OneToMany_Optional_Self_Inverse1Id], [t].[OneToMany_Required_Self_Inverse1Id], [t].[OneToOne_Optional_Self1Id], [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id]
FROM (
    SELECT TOP(@__p_0) [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Name] AS [Name0]
    FROM [LevelOne] AS [l]
    LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Required_Id]
    ORDER BY [l0].[Name]
) AS [t]
LEFT JOIN [LevelTwo] AS [l1] ON [t].[Id] = [l1].[Level1_Optional_Id]
ORDER BY [t].[Name0]");
        }

        public override void Include18_3_1()
        {
            base.Include18_3_1();

            // issue #15783
            AssertSql(
                @"@__p_0='10'

SELECT [t].[Id], [t].[Date], [t].[Name], [t].[OneToMany_Optional_Self_Inverse1Id], [t].[OneToMany_Required_Self_Inverse1Id], [t].[OneToOne_Optional_Self1Id], [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id]
FROM (
    SELECT TOP(@__p_0) [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Name] AS [Name0]
    FROM [LevelOne] AS [l]
    LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Required_Id]
    ORDER BY [l0].[Name]
) AS [t]
LEFT JOIN [LevelTwo] AS [l1] ON [t].[Id] = [l1].[Level1_Optional_Id]
ORDER BY [t].[Name0]");
        }

        public override void Include18_3_2()
        {
            base.Include18_3_2();

            // issue #15783
            AssertSql(
                @"@__p_0='10'

SELECT [t].[Id], [t].[Date], [t].[Name], [t].[OneToMany_Optional_Self_Inverse1Id], [t].[OneToMany_Required_Self_Inverse1Id], [t].[OneToOne_Optional_Self1Id], [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id]
FROM (
    SELECT TOP(@__p_0) [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Name] AS [Name0]
    FROM [LevelOne] AS [l]
    LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Required_Id]
    ORDER BY [l0].[Name]
) AS [t]
LEFT JOIN [LevelTwo] AS [l1] ON [t].[Id] = [l1].[Level1_Optional_Id]
ORDER BY [t].[Name0]");
        }

        public override async Task Include18_3_3(bool async)
        {
            await base.Include18_3_3(async);

            AssertSql(
                @"SELECT [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
FROM (
    SELECT DISTINCT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
    FROM [LevelOne] AS [l]
    LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
) AS [t]
LEFT JOIN [LevelThree] AS [l1] ON [t].[Id] = [l1].[Level2_Optional_Id]");
        }

        public override void Include18_4()
        {
            base.Include18_4();

            // issue #15783
            AssertSql(
                @"SELECT [t].[Id], [t].[Date], [t].[Name], [t].[OneToMany_Optional_Self_Inverse1Id], [t].[OneToMany_Required_Self_Inverse1Id], [t].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id]
FROM (
    SELECT DISTINCT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
    FROM [LevelOne] AS [l]
) AS [t]
LEFT JOIN [LevelTwo] AS [l0] ON [t].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelTwo] AS [l1] ON [t].[Id] = [l1].[Level1_Optional_Id]");
        }

        public override void Include18()
        {
            base.Include18();

            AssertSql(
                @"@__p_0='10'

SELECT [t].[Id], [t].[Date], [t].[Name], [t].[OneToMany_Optional_Self_Inverse1Id], [t].[OneToMany_Required_Self_Inverse1Id], [t].[OneToOne_Optional_Self1Id], [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
FROM (
    SELECT TOP(@__p_0) [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
    FROM [LevelOne] AS [l]
    ORDER BY [l].[Id]
) AS [t]
LEFT JOIN [LevelTwo] AS [l0] ON [t].[Id] = [l0].[OneToOne_Optional_PK_Inverse2Id]
LEFT JOIN [LevelTwo] AS [l1] ON [t].[Id] = [l1].[Level1_Optional_Id]
ORDER BY [t].[Id]");
        }

        public override void Include19()
        {
            base.Include19();

            AssertSql(
                @"SELECT [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id], [t].[Id0], [t].[Date0], [t].[Level1_Optional_Id0], [t].[Level1_Required_Id0], [t].[Name0], [t].[OneToMany_Optional_Inverse2Id0], [t].[OneToMany_Optional_Self_Inverse2Id0], [t].[OneToMany_Required_Inverse2Id0], [t].[OneToMany_Required_Self_Inverse2Id0], [t].[OneToOne_Optional_PK_Inverse2Id0], [t].[OneToOne_Optional_Self2Id0]
FROM (
    SELECT DISTINCT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id] AS [Id0], [l1].[Date] AS [Date0], [l1].[Level1_Optional_Id] AS [Level1_Optional_Id0], [l1].[Level1_Required_Id] AS [Level1_Required_Id0], [l1].[Name] AS [Name0], [l1].[OneToMany_Optional_Inverse2Id] AS [OneToMany_Optional_Inverse2Id0], [l1].[OneToMany_Optional_Self_Inverse2Id] AS [OneToMany_Optional_Self_Inverse2Id0], [l1].[OneToMany_Required_Inverse2Id] AS [OneToMany_Required_Inverse2Id0], [l1].[OneToMany_Required_Self_Inverse2Id] AS [OneToMany_Required_Self_Inverse2Id0], [l1].[OneToOne_Optional_PK_Inverse2Id] AS [OneToOne_Optional_PK_Inverse2Id0], [l1].[OneToOne_Optional_Self2Id] AS [OneToOne_Optional_Self2Id0]
    FROM [LevelOne] AS [l]
    LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
    LEFT JOIN [LevelTwo] AS [l1] ON [l].[Id] = [l1].[OneToOne_Optional_PK_Inverse2Id]
) AS [t]");
        }

        public override async Task Include_with_all_method_include_gets_ignored(bool isAsnc)
        {
            await base.Include_with_all_method_include_gets_ignored(isAsnc);

            AssertSql(
                @"SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [LevelOne] AS [l]
        WHERE ([l].[Name] = N'Foo') AND [l].[Name] IS NOT NULL) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task Join_with_navigations_in_the_result_selector1(bool async)
        {
            await base.Join_with_navigations_in_the_result_selector1(async);

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Required_Id]
LEFT JOIN [LevelTwo] AS [l1] ON [l].[Id] = [l1].[Level1_Optional_Id]");
        }

        public override void Join_with_navigations_in_the_result_selector2()
        {
            base.Join_with_navigations_in_the_result_selector2();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id], [l].[Id], [l0].[Id], [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Optional_Self_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToMany_Required_Self_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Required_Id]
LEFT JOIN [LevelTwo] AS [l1] ON [l].[Id] = [l1].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l2] ON [l0].[Id] = [l2].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id], [l1].[Id], [l2].[Id]");
        }

        public override void GroupJoin_with_navigations_in_the_result_selector()
        {
            base.GroupJoin_with_navigations_in_the_result_selector();

            AssertSql(
                @"");
        }

        public override void Member_pushdown_chain_3_levels_deep()
        {
            base.Member_pushdown_chain_3_levels_deep();

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
WHERE ((
    SELECT TOP(1) (
        SELECT TOP(1) (
            SELECT TOP(1) [l2].[Name]
            FROM [LevelFour] AS [l2]
            WHERE [l2].[Level3_Required_Id] = [l1].[Id]
            ORDER BY [l2].[Id])
        FROM [LevelThree] AS [l1]
        WHERE [l1].[Level2_Required_Id] = [l0].[Id]
        ORDER BY [l1].[Id])
    FROM [LevelTwo] AS [l0]
    WHERE [l0].[Level1_Optional_Id] = [l].[Id]
    ORDER BY [l0].[Id]) <> N'Foo') OR (
    SELECT TOP(1) (
        SELECT TOP(1) (
            SELECT TOP(1) [l2].[Name]
            FROM [LevelFour] AS [l2]
            WHERE [l2].[Level3_Required_Id] = [l1].[Id]
            ORDER BY [l2].[Id])
        FROM [LevelThree] AS [l1]
        WHERE [l1].[Level2_Required_Id] = [l0].[Id]
        ORDER BY [l1].[Id])
    FROM [LevelTwo] AS [l0]
    WHERE [l0].[Level1_Optional_Id] = [l].[Id]
    ORDER BY [l0].[Id]) IS NULL
ORDER BY [l].[Id]");
        }

        public override void Member_pushdown_chain_3_levels_deep_entity()
        {
            base.Member_pushdown_chain_3_levels_deep_entity();

            AssertSql(
                @"SELECT [t0].[c], [t1].[c], [t3].[Id], [t3].[Level3_Optional_Id], [t3].[Level3_Required_Id], [t3].[Name], [t3].[OneToMany_Optional_Inverse4Id], [t3].[OneToMany_Optional_Self_Inverse4Id], [t3].[OneToMany_Required_Inverse4Id], [t3].[OneToMany_Required_Self_Inverse4Id], [t3].[OneToOne_Optional_PK_Inverse4Id], [t3].[OneToOne_Optional_Self4Id]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [t].[c], [t].[Id], [t].[Level1_Optional_Id]
    FROM (
        SELECT 1 AS [c], [l0].[Id], [l0].[Level1_Optional_Id], ROW_NUMBER() OVER(PARTITION BY [l0].[Level1_Optional_Id] ORDER BY [l0].[Id]) AS [row]
        FROM [LevelTwo] AS [l0]
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [l].[Id] = [t0].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [t2].[c], [t2].[Id], [t2].[Level2_Required_Id]
    FROM (
        SELECT 1 AS [c], [l1].[Id], [l1].[Level2_Required_Id], ROW_NUMBER() OVER(PARTITION BY [l1].[Level2_Required_Id] ORDER BY [l1].[Id]) AS [row]
        FROM [LevelThree] AS [l1]
    ) AS [t2]
    WHERE [t2].[row] <= 1
) AS [t1] ON [t0].[Id] = [t1].[Level2_Required_Id]
LEFT JOIN (
    SELECT [t4].[Id], [t4].[Level3_Optional_Id], [t4].[Level3_Required_Id], [t4].[Name], [t4].[OneToMany_Optional_Inverse4Id], [t4].[OneToMany_Optional_Self_Inverse4Id], [t4].[OneToMany_Required_Inverse4Id], [t4].[OneToMany_Required_Self_Inverse4Id], [t4].[OneToOne_Optional_PK_Inverse4Id], [t4].[OneToOne_Optional_Self4Id]
    FROM (
        SELECT [l2].[Id], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Optional_Self_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToMany_Required_Self_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id], [l2].[OneToOne_Optional_Self4Id], ROW_NUMBER() OVER(PARTITION BY [l2].[Level3_Required_Id] ORDER BY [l2].[Id]) AS [row]
        FROM [LevelFour] AS [l2]
    ) AS [t4]
    WHERE [t4].[row] <= 1
) AS [t3] ON [t1].[Id] = [t3].[Level3_Required_Id]
ORDER BY [l].[Id]");
        }

        public override void Member_pushdown_with_collection_navigation_in_the_middle()
        {
            base.Member_pushdown_with_collection_navigation_in_the_middle();

            AssertSql(
                @"SELECT (
    SELECT TOP(1) (
        SELECT TOP(1) (
            SELECT TOP(1) [l2].[Name]
            FROM [LevelFour] AS [l2]
            WHERE [l2].[Level3_Required_Id] = [l1].[Id]
            ORDER BY [l2].[Id])
        FROM [LevelThree] AS [l1]
        WHERE [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id])
    FROM [LevelTwo] AS [l0]
    WHERE [l0].[Level1_Required_Id] = [l].[Id]
    ORDER BY [l0].[Id])
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]");
        }

        public override async Task Member_pushdown_with_multiple_collections(bool async)
        {
            await base.Member_pushdown_with_multiple_collections(async);

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [l0].[Name]
    FROM [LevelThree] AS [l0]
    WHERE (
        SELECT TOP(1) [l1].[Id]
        FROM [LevelTwo] AS [l1]
        WHERE [l].[Id] = [l1].[OneToMany_Optional_Inverse2Id]
        ORDER BY [l1].[Id]) IS NOT NULL AND (((
        SELECT TOP(1) [l2].[Id]
        FROM [LevelTwo] AS [l2]
        WHERE [l].[Id] = [l2].[OneToMany_Optional_Inverse2Id]
        ORDER BY [l2].[Id]) = [l0].[OneToMany_Optional_Inverse3Id]) OR ((
        SELECT TOP(1) [l2].[Id]
        FROM [LevelTwo] AS [l2]
        WHERE [l].[Id] = [l2].[OneToMany_Optional_Inverse2Id]
        ORDER BY [l2].[Id]) IS NULL AND [l0].[OneToMany_Optional_Inverse3Id] IS NULL))
    ORDER BY [l0].[Id])
FROM [LevelOne] AS [l]");
        }

        public override async Task Null_check_removal_applied_recursively(bool async)
        {
            await base.Null_check_removal_applied_recursively(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Optional_Id]
LEFT JOIN [LevelFour] AS [l2] ON [l1].[Id] = [l2].[Level3_Optional_Id]
WHERE [l2].[Name] = N'L4 01'");
        }

        public override async Task Null_check_different_structure_does_not_remove_null_checks(bool async)
        {
            await base.Null_check_different_structure_does_not_remove_null_checks(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Optional_Id]
LEFT JOIN [LevelFour] AS [l2] ON [l1].[Id] = [l2].[Level3_Optional_Id]
WHERE CASE
    WHEN [l0].[Id] IS NULL THEN NULL
    WHEN [l1].[Id] IS NULL THEN NULL
    ELSE [l2].[Name]
END = N'L4 01'");
        }

        public override async Task Union_over_entities_with_different_nullability(bool async)
        {
            await base.Union_over_entities_with_different_nullability(async);

            AssertSql(
                @"SELECT [t].[Id]
FROM (
    SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id] AS [Id0], [l0].[Date] AS [Date0], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name] AS [Name0], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
    FROM [LevelOne] AS [l]
    LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
    UNION ALL
    SELECT [l2].[Id], [l2].[Date], [l2].[Name], [l2].[OneToMany_Optional_Self_Inverse1Id], [l2].[OneToMany_Required_Self_Inverse1Id], [l2].[OneToOne_Optional_Self1Id], [l1].[Id] AS [Id0], [l1].[Date] AS [Date0], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name] AS [Name0], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id]
    FROM [LevelTwo] AS [l1]
    LEFT JOIN [LevelOne] AS [l2] ON [l1].[Level1_Optional_Id] = [l2].[Id]
    WHERE [l2].[Id] IS NULL
) AS [t]");
        }

        public override async Task Including_reference_navigation_and_projecting_collection_navigation_2(bool async)
        {
            await base.Including_reference_navigation_and_projecting_collection_navigation_2(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id], [t0].[Id], [t0].[Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Optional_Self_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToMany_Required_Self_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Required_Id]
LEFT JOIN [LevelTwo] AS [l1] ON [l].[Id] = [l1].[OneToMany_Required_Inverse2Id]
LEFT JOIN (
    SELECT [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id], ROW_NUMBER() OVER(PARTITION BY [l2].[OneToMany_Required_Inverse2Id] ORDER BY [l2].[Id] DESC) AS [row]
        FROM [LevelTwo] AS [l2]
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Required_Inverse2Id]
ORDER BY [l].[Id], [l0].[Id], [l1].[Id]");
        }

        public override async Task OrderBy_collection_count_ThenBy_reference_navigation(bool async)
        {
            await base.OrderBy_collection_count_ThenBy_reference_navigation(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Required_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Required_Id]
ORDER BY (
    SELECT COUNT(*)
    FROM [LevelThree] AS [l2]
    WHERE [l0].[Id] IS NOT NULL AND ([l0].[Id] = [l2].[OneToMany_Required_Inverse3Id])), [l1].[Name]");
        }

        public override async Task Null_conditional_is_not_applied_explicitly_for_optional_navigation(bool async)
        {
            await base.Null_conditional_is_not_applied_explicitly_for_optional_navigation(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
WHERE [l0].[Id] IS NOT NULL AND ([l0].[Name] = N'L2 01')");
        }

        public override async Task Sum_with_selector_cast_using_as(bool async)
        {
            await base.Sum_with_selector_cast_using_as(async);

            AssertSql(
                @"SELECT COALESCE(SUM([l].[Id]), 0)
FROM [LevelOne] AS [l]");
        }

        public override async Task Sum_with_filter_with_include_selector_cast_using_as(bool async)
        {
            await base.Sum_with_filter_with_include_selector_cast_using_as(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
WHERE [l].[Id] > (
    SELECT SUM([l1].[Id])
    FROM [LevelTwo] AS [l1]
    WHERE [l].[Id] = [l1].[OneToMany_Optional_Inverse2Id])
ORDER BY [l].[Id], [l0].[Id]");
        }

        public override async Task Select_with_joined_where_clause_cast_using_as(bool async)
        {
            await base.Select_with_joined_where_clause_cast_using_as(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
WHERE [l].[Id] = [l0].[Id]");
        }

        public override async Task SelectMany_with_outside_reference_to_joined_table_correctly_translated_to_apply(bool async)
        {
            await base.SelectMany_with_outside_reference_to_joined_table_correctly_translated_to_apply(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Required_Id]
INNER JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Required_Id]
INNER JOIN [LevelFour] AS [l2] ON [l1].[Id] = [l2].[Level3_Required_Id]
LEFT JOIN [LevelOne] AS [l3] ON ([l0].[Id] >= [l3].[Id]) AND (([l2].[Name] = [l3].[Name]) OR ([l2].[Name] IS NULL AND [l3].[Name] IS NULL))");
        }

        public override async Task Nested_SelectMany_correlated_with_join_table_correctly_translated_to_apply(bool async)
        {
            await base.Nested_SelectMany_correlated_with_join_table_correctly_translated_to_apply(async);

            AssertSql(
                @"");
        }

        public override void Contains_over_optional_navigation_with_null_constant()
        {
            base.Contains_over_optional_navigation_with_null_constant();

            AssertSql(
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [LevelOne] AS [l]
        LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
        WHERE [l0].[Id] IS NULL) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task Contains_over_optional_navigation_with_null_parameter(bool async)
        {
            await base.Contains_over_optional_navigation_with_null_parameter(async);

            AssertSql(
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [LevelOne] AS [l]
        LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
        WHERE [l0].[Id] IS NULL) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task Contains_over_optional_navigation_with_null_column(bool async)
        {
            await base.Contains_over_optional_navigation_with_null_column(async);

            AssertSql(
                @"SELECT [l].[Name], [l0].[Name] AS [OptionalName], CASE
    WHEN EXISTS (
        SELECT 1
        FROM [LevelOne] AS [l1]
        LEFT JOIN [LevelTwo] AS [l2] ON [l1].[Id] = [l2].[Level1_Optional_Id]
        WHERE ([l2].[Name] = [l0].[Name]) OR ([l2].[Name] IS NULL AND [l0].[Name] IS NULL)) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Contains]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]");
        }

        public override async Task Contains_over_optional_navigation_with_null_entity_reference(bool async)
        {
            await base.Contains_over_optional_navigation_with_null_entity_reference(async);

            AssertSql(
                @"SELECT [l].[Name], [l0].[Name] AS [OptionalName], CASE
    WHEN EXISTS (
        SELECT 1
        FROM [LevelOne] AS [l2]
        LEFT JOIN [LevelTwo] AS [l3] ON [l2].[Id] = [l3].[Level1_Optional_Id]
        WHERE ([l3].[Id] = [l1].[Id]) OR ([l3].[Id] IS NULL AND [l1].[Id] IS NULL)) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Contains]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelTwo] AS [l1] ON [l].[Id] = [l1].[OneToOne_Optional_PK_Inverse2Id]");
        }

        public override async Task Element_selector_with_coalesce_repeated_in_aggregate(bool async)
        {
            await base.Element_selector_with_coalesce_repeated_in_aggregate(async);

            AssertSql(
                @"SELECT COUNT(*)
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Id]
GROUP BY [l1].[Name]
HAVING MIN(COALESCE([l0].[Id], 0) + COALESCE([l0].[Id], 0)) > 0");
        }

        public override async Task Nested_object_constructed_from_group_key_properties(bool async)
        {
            await base.Nested_object_constructed_from_group_key_properties(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Name], [l].[Date], [l0].[Id], [l1].[Name], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], COALESCE(SUM(CAST(LEN([l].[Name]) AS int)), 0) AS [Aggregate]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelTwo] AS [l1] ON [l].[Id] = [l1].[Level1_Required_Id]
WHERE [l0].[Id] IS NOT NULL
GROUP BY [l].[Id], [l].[Date], [l].[Name], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l1].[Name]");
        }

        public override async Task GroupBy_aggregate_where_required_relationship(bool async)
        {
            await base.GroupBy_aggregate_where_required_relationship(async);

            AssertSql(
                @"SELECT [l0].[Id] AS [Key], MAX([l].[Id]) AS [Max]
FROM [LevelTwo] AS [l]
INNER JOIN [LevelOne] AS [l0] ON [l].[OneToMany_Required_Inverse2Id] = [l0].[Id]
GROUP BY [l0].[Id]
HAVING (MAX([l].[Id]) <> 2) OR MAX([l].[Id]) IS NULL");
        }

        public override async Task GroupBy_aggregate_where_required_relationship_2(bool async)
        {
            await base.GroupBy_aggregate_where_required_relationship_2(async);

            AssertSql(
                @"SELECT [l0].[Id] AS [Key], MAX([l].[Id]) AS [Max]
FROM [LevelTwo] AS [l]
INNER JOIN [LevelOne] AS [l0] ON [l].[OneToMany_Required_Inverse2Id] = [l0].[Id]
GROUP BY [l0].[Id]
HAVING (MAX([l].[Id]) < 2) OR (MAX([l].[Id]) > 2)");
        }

        public override async Task Member_over_null_check_ternary_and_nested_dto_type(bool async)
        {
            await base.Member_over_null_check_ternary_and_nested_dto_type(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Name], CASE
    WHEN [l0].[Id] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [l0].[Id], [l0].[Name]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
ORDER BY [l0].[Name], [l].[Id]");
        }

        public override async Task Member_over_null_check_ternary_and_nested_anonymous_type(bool async)
        {
            await base.Member_over_null_check_ternary_and_nested_anonymous_type(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Name], CASE
    WHEN [l0].[Id] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [l0].[Id], [l0].[Name], CASE
    WHEN [l1].[Id] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [l1].[Id], [l1].[Name]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Optional_Id]
WHERE (CASE
    WHEN [l0].[Id] IS NULL THEN NULL
    ELSE [l1].[Name]
END <> N'L') OR CASE
    WHEN [l0].[Id] IS NULL THEN NULL
    ELSE [l1].[Name]
END IS NULL");
        }

        public override async Task Distinct_skip_without_orderby(bool async)
        {
            await base.Distinct_skip_without_orderby(async);

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [t0].[Name]
    FROM (
        SELECT [t].[Id], [t].[Level2_Optional_Id], [t].[Level2_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse3Id], [t].[OneToMany_Optional_Self_Inverse3Id], [t].[OneToMany_Required_Inverse3Id], [t].[OneToMany_Required_Self_Inverse3Id], [t].[OneToOne_Optional_PK_Inverse3Id], [t].[OneToOne_Optional_Self3Id]
        FROM (
            SELECT DISTINCT [l0].[Id], [l0].[Level2_Optional_Id], [l0].[Level2_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse3Id], [l0].[OneToMany_Optional_Self_Inverse3Id], [l0].[OneToMany_Required_Inverse3Id], [l0].[OneToMany_Required_Self_Inverse3Id], [l0].[OneToOne_Optional_PK_Inverse3Id], [l0].[OneToOne_Optional_Self3Id]
            FROM [LevelThree] AS [l0]
        ) AS [t]
        ORDER BY (SELECT 1)
        OFFSET 1 ROWS
    ) AS [t0]
    ORDER BY [t0].[Id])
FROM [LevelOne] AS [l]
WHERE [l].[Id] < 3");
        }

        public override async Task Distinct_take_without_orderby(bool async)
        {
            await base.Distinct_take_without_orderby(async);

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [t].[Name]
    FROM (
        SELECT DISTINCT TOP(1) [l0].[Id], [l0].[Level2_Optional_Id], [l0].[Level2_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse3Id], [l0].[OneToMany_Optional_Self_Inverse3Id], [l0].[OneToMany_Required_Inverse3Id], [l0].[OneToMany_Required_Self_Inverse3Id], [l0].[OneToOne_Optional_PK_Inverse3Id], [l0].[OneToOne_Optional_Self3Id]
        FROM [LevelThree] AS [l0]
    ) AS [t]
    ORDER BY [t].[Id])
FROM [LevelOne] AS [l]
WHERE [l].[Id] < 3");
        }

        public override async Task Let_let_contains_from_outer_let(bool async)
        {
            await base.Let_let_contains_from_outer_let(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [t].[Id], [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id], [t].[Level2_Optional_Id], [t].[Level2_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse3Id], [t].[OneToMany_Optional_Self_Inverse3Id], [t].[OneToMany_Required_Inverse3Id], [t].[OneToMany_Required_Self_Inverse3Id], [t].[OneToOne_Optional_PK_Inverse3Id], [t].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
OUTER APPLY (
    SELECT [l0].[Id], [l0].[Level2_Optional_Id], [l0].[Level2_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse3Id], [l0].[OneToMany_Optional_Self_Inverse3Id], [l0].[OneToMany_Required_Inverse3Id], [l0].[OneToMany_Required_Self_Inverse3Id], [l0].[OneToOne_Optional_PK_Inverse3Id], [l0].[OneToOne_Optional_Self3Id]
    FROM [LevelThree] AS [l0]
    WHERE EXISTS (
        SELECT 1
        FROM [LevelTwo] AS [l1]
        WHERE ([l].[Id] = [l1].[OneToMany_Required_Inverse2Id]) AND ([l1].[Id] = [l0].[Level2_Required_Id]))
) AS [t]
LEFT JOIN [LevelTwo] AS [l2] ON [l].[Id] = [l2].[OneToMany_Required_Inverse2Id]
ORDER BY [l].[Id], [t].[Id], [l2].[Id]");
        }

        public override async Task Multiple_conditionals_in_projection(bool async)
        {
            await base.Multiple_conditionals_in_projection(async);

            AssertSql(
                @"SELECT [l].[Id], [l0].[Name], CASE
    WHEN [l1].[Id] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [LevelTwo] AS [l]
LEFT JOIN [LevelThree] AS [l0] ON [l].[Id] = [l0].[Level2_Optional_Id]
LEFT JOIN [LevelOne] AS [l1] ON [l].[Level1_Optional_Id] = [l1].[Id]");
        }

        public override async Task Composite_key_join_on_groupby_aggregate_projecting_only_grouping_key(bool async)
        {
            await base.Composite_key_join_on_groupby_aggregate_projecting_only_grouping_key(async);

            AssertSql(
                @"SELECT [t].[Key]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l0].[Id] % 3 AS [Key], COALESCE(SUM([l0].[Id]), 0) AS [Sum]
    FROM [LevelTwo] AS [l0]
    GROUP BY [l0].[Id] % 3
) AS [t] ON ([l].[Id] = [t].[Key]) AND (CAST(1 AS bit) = CASE
    WHEN [t].[Sum] > 10 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END)");
        }

        public override async Task Multiple_joins_groupby_predicate(bool async)
        {
            await base.Multiple_joins_groupby_predicate(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Name], CASE
    WHEN [l0].[Id] IS NULL THEN N'Foo'
    ELSE N'Bar'
END AS [Foo]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l1].[Name] AS [Key], COUNT(*) AS [Count]
    FROM [LevelThree] AS [l1]
    GROUP BY [l1].[Name]
) AS [t] ON [l].[Name] = [t].[Key]
WHERE [l0].[Name] IS NOT NULL OR ([t].[Count] > 0)");
        }

        public override async Task Collection_FirstOrDefault_property_accesses_in_projection(bool async)
        {
            await base.Collection_FirstOrDefault_property_accesses_in_projection(async);

            AssertSql(
                @"SELECT [l].[Id], (
    SELECT TOP(1) [l0].[Name]
    FROM [LevelTwo] AS [l0]
    WHERE ([l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]) AND ([l0].[Name] = N'L2 02')) AS [Pushdown]
FROM [LevelOne] AS [l]
WHERE [l].[Id] < 3");
        }

        public override async Task Collection_FirstOrDefault_entity_reference_accesses_in_projection(bool async)
        {
            await base.Collection_FirstOrDefault_entity_reference_accesses_in_projection(async);

            AssertSql(
                @"SELECT [l].[Id], [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Optional_Self_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToMany_Required_Self_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [t].[Id], [t].[Level2_Optional_Id], [t].[Level2_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse3Id], [t].[OneToMany_Optional_Self_Inverse3Id], [t].[OneToMany_Required_Inverse3Id], [t].[OneToMany_Required_Self_Inverse3Id], [t].[OneToOne_Optional_PK_Inverse3Id], [t].[OneToOne_Optional_Self3Id], [t].[OneToMany_Optional_Inverse2Id]
    FROM (
        SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l0].[OneToMany_Optional_Inverse2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Id], [l1].[Id]) AS [row]
        FROM [LevelTwo] AS [l0]
        LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Optional_Id]
        WHERE [l0].[Name] = N'L2 02'
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
WHERE [l].[Id] < 3");
        }

        public override async Task Collection_FirstOrDefault_entity_collection_accesses_in_projection(bool async)
        {
            await base.Collection_FirstOrDefault_entity_collection_accesses_in_projection(async);

            AssertSql(
                @"");
        }

        public override async Task Multiple_collection_FirstOrDefault_followed_by_member_access_in_projection(bool async)
        {
            await base.Multiple_collection_FirstOrDefault_followed_by_member_access_in_projection(async);

            AssertSql(
                @"SELECT [l].[Id], (
    SELECT TOP(1) [l0].[Name]
    FROM [LevelThree] AS [l0]
    WHERE (
        SELECT TOP(1) [l1].[Id]
        FROM [LevelTwo] AS [l1]
        WHERE ([l].[Id] = [l1].[OneToMany_Optional_Inverse2Id]) AND ([l1].[Name] = N'L2 02')) IS NOT NULL AND (((
        SELECT TOP(1) [l2].[Id]
        FROM [LevelTwo] AS [l2]
        WHERE ([l].[Id] = [l2].[OneToMany_Optional_Inverse2Id]) AND ([l2].[Name] = N'L2 02')) = [l0].[OneToMany_Optional_Inverse3Id]) OR ((
        SELECT TOP(1) [l2].[Id]
        FROM [LevelTwo] AS [l2]
        WHERE ([l].[Id] = [l2].[OneToMany_Optional_Inverse2Id]) AND ([l2].[Name] = N'L2 02')) IS NULL AND [l0].[OneToMany_Optional_Inverse3Id] IS NULL))
    ORDER BY [l0].[Id]) AS [Pushdown]
FROM [LevelOne] AS [l]
WHERE [l].[Id] < 2");
        }

        public override async Task Projecting_columns_with_same_name_from_different_entities_making_sure_aliasing_works_after_Distinct(bool async)
        {
            await base.Projecting_columns_with_same_name_from_different_entities_making_sure_aliasing_works_after_Distinct(async);

            AssertSql(
                @"@__p_0='10'

SELECT [t].[Id1] AS [Foo], [t].[Id2] AS [Bar], [t].[Id3] AS [Baz]
FROM (
    SELECT DISTINCT TOP(@__p_0) [l].[Id] AS [Id1], [l0].[Id] AS [Id2], [l1].[Id] AS [Id3], [l].[Name] AS [Name1], [l0].[Name] AS [Name2]
    FROM [LevelOne] AS [l]
    INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
    INNER JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Optional_Id]
) AS [t]");
        }

        public override async Task Complex_query_with_let_collection_SelectMany(bool async)
        {
            await base.Complex_query_with_let_collection_SelectMany(async);

            AssertSql(
                @"SELECT [t].[Id], [t].[Date], [t].[Name], [t].[OneToMany_Optional_Self_Inverse1Id], [t].[OneToMany_Required_Self_Inverse1Id], [t].[OneToOne_Optional_Self1Id], [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[Date], [l0].[Name], [l0].[OneToMany_Optional_Self_Inverse1Id], [l0].[OneToMany_Required_Self_Inverse1Id], [l0].[OneToOne_Optional_Self1Id]
    FROM [LevelOne] AS [l0]
    WHERE [l0].[Id] > 5
) AS [t] ON [l].[Id] = [t].[Id]
WHERE [l].[Name] IS NOT NULL AND ([l].[Name] LIKE N'L1 0%')");
        }

        public override async Task SelectMany_without_collection_selector_returning_queryable(bool async)
        {
            await base.SelectMany_without_collection_selector_returning_queryable(async);

            AssertSql(
                @"SELECT [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
CROSS JOIN (
    SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
    FROM [LevelTwo] AS [l0]
    WHERE [l0].[Id] < 10
) AS [t]");
        }

        public override async Task Select_projecting_queryable_followed_by_SelectMany(bool async)
        {
            await base.Select_projecting_queryable_followed_by_SelectMany(async);

            AssertSql(
                @"SELECT [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
CROSS JOIN (
    SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
    FROM [LevelTwo] AS [l0]
    WHERE [l0].[Id] < 10
) AS [t]");
        }

        public override async Task Select_projecting_queryable_followed_by_Join(bool async)
        {
            await base.Select_projecting_queryable_followed_by_Join(async);

            AssertSql(
                @"SELECT [l0].[Id], [l0].[Level2_Optional_Id], [l0].[Level2_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse3Id], [l0].[OneToMany_Optional_Self_Inverse3Id], [l0].[OneToMany_Required_Inverse3Id], [l0].[OneToMany_Required_Self_Inverse3Id], [l0].[OneToOne_Optional_PK_Inverse3Id], [l0].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelThree] AS [l0] ON 7 = [l0].[Id]");
        }

        public override async Task Select_projecting_queryable_in_anonymous_projection_followed_by_Join(bool async)
        {
            await base.Select_projecting_queryable_in_anonymous_projection_followed_by_Join(async);

            AssertSql(
                @"SELECT [l0].[Id], [l0].[Level2_Optional_Id], [l0].[Level2_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse3Id], [l0].[OneToMany_Optional_Self_Inverse3Id], [l0].[OneToMany_Required_Inverse3Id], [l0].[OneToMany_Required_Self_Inverse3Id], [l0].[OneToOne_Optional_PK_Inverse3Id], [l0].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelThree] AS [l0] ON 7 = [l0].[Id]");
        }

        public override async Task Project_shadow_properties(bool async)
        {
            await base.Project_shadow_properties(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[OneToOne_Optional_Self1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToMany_Optional_Self_Inverse1Id]
FROM [LevelOne] AS [l]",
                //
                @"SELECT [l].[Id], [l].[OneToOne_Optional_PK_Inverse2Id], [l].[OneToMany_Required_Inverse2Id], [l].[OneToMany_Optional_Inverse2Id], [l].[OneToOne_Optional_Self2Id], [l].[OneToMany_Required_Self_Inverse2Id], [l].[OneToMany_Optional_Self_Inverse2Id]
FROM [LevelTwo] AS [l]",
                //
                @"SELECT [l].[Id], [l].[OneToOne_Optional_PK_Inverse3Id], [l].[OneToMany_Required_Inverse3Id], [l].[OneToMany_Optional_Inverse3Id], [l].[OneToOne_Optional_Self3Id], [l].[OneToMany_Required_Self_Inverse3Id], [l].[OneToMany_Optional_Self_Inverse3Id]
FROM [LevelThree] AS [l]",
                //
                @"SELECT [l].[Id], [l].[OneToOne_Optional_PK_Inverse4Id], [l].[OneToMany_Required_Inverse4Id], [l].[OneToMany_Optional_Inverse4Id], [l].[OneToOne_Optional_Self4Id], [l].[OneToMany_Required_Self_Inverse4Id], [l].[OneToMany_Optional_Self_Inverse4Id]
FROM [LevelFour] AS [l]",
                //
                @"SELECT [i].[Id], [i].[InheritanceBase2Id], [i].[InheritanceBase2Id1]
FROM [InheritanceOne] AS [i]",
                //
                @"SELECT [i].[Id], [i].[InheritanceBase2Id], [i].[InheritanceBase2Id1]
FROM [InheritanceOne] AS [i]
WHERE [i].[Discriminator] = N'InheritanceDerived1'",
                //
                @"SELECT [i].[Id], [i].[InheritanceBase2Id], [i].[InheritanceBase2Id1]
FROM [InheritanceOne] AS [i]
WHERE [i].[Discriminator] = N'InheritanceDerived2'",
                //
                @"SELECT [i].[Id], [i].[InheritanceLeaf2Id]
FROM [InheritanceTwo] AS [i]",
                //
                @"SELECT [i].[Id], [i].[DifferentTypeReference_InheritanceDerived1Id], [i].[InheritanceDerived1Id], [i].[InheritanceDerived1Id1], [i].[InheritanceDerived2Id], [i].[SameTypeReference_InheritanceDerived1Id], [i].[SameTypeReference_InheritanceDerived2Id]
FROM [InheritanceLeafOne] AS [i]",
                //
                @"SELECT [i].[Id], [i].[DifferentTypeReference_InheritanceDerived2Id], [i].[InheritanceDerived2Id]
FROM [InheritanceLeafTwo] AS [i]");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
