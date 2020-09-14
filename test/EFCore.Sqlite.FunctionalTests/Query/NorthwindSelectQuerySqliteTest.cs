// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindSelectQuerySqliteTest : NorthwindSelectQueryRelationalTestBase<NorthwindQuerySqliteFixture<NoopModelCustomizer>>
    {
        public NorthwindSelectQuerySqliteTest(NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task Select_datetime_year_component(bool async)
        {
            await base.Select_datetime_year_component(async);

            AssertSql(
                @"SELECT CAST(strftime('%Y', ""o"".""OrderDate"") AS INTEGER)
FROM ""Orders"" AS ""o""");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Select_datetime_year_component_composed(bool async)
        {
            await AssertQueryScalar(
                async,
                ss => ss.Set<Order>().Select(o => o.OrderDate.Value.AddYears(1).Year));

            AssertSql(
                @"SELECT CAST(strftime('%Y', ""o"".""OrderDate"", CAST(1 AS TEXT) || ' years') AS INTEGER)
FROM ""Orders"" AS ""o""");
        }

        public override async Task Select_datetime_month_component(bool async)
        {
            await base.Select_datetime_month_component(async);

            AssertSql(
                @"SELECT CAST(strftime('%m', ""o"".""OrderDate"") AS INTEGER)
FROM ""Orders"" AS ""o""");
        }

        public override async Task Select_datetime_day_of_year_component(bool async)
        {
            await base.Select_datetime_day_of_year_component(async);

            AssertSql(
                @"SELECT CAST(strftime('%j', ""o"".""OrderDate"") AS INTEGER)
FROM ""Orders"" AS ""o""");
        }

        public override async Task Select_datetime_day_component(bool async)
        {
            await base.Select_datetime_day_component(async);

            AssertSql(
                @"SELECT CAST(strftime('%d', ""o"".""OrderDate"") AS INTEGER)
FROM ""Orders"" AS ""o""");
        }

        public override async Task Select_datetime_hour_component(bool async)
        {
            await base.Select_datetime_hour_component(async);

            AssertSql(
                @"SELECT CAST(strftime('%H', ""o"".""OrderDate"") AS INTEGER)
FROM ""Orders"" AS ""o""");
        }

        public override async Task Select_datetime_minute_component(bool async)
        {
            await base.Select_datetime_minute_component(async);

            AssertSql(
                @"SELECT CAST(strftime('%M', ""o"".""OrderDate"") AS INTEGER)
FROM ""Orders"" AS ""o""");
        }

        public override async Task Select_datetime_second_component(bool async)
        {
            await base.Select_datetime_second_component(async);

            AssertSql(
                @"SELECT CAST(strftime('%S', ""o"".""OrderDate"") AS INTEGER)
FROM ""Orders"" AS ""o""");
        }

        public override async Task Select_datetime_millisecond_component(bool async)
        {
            await base.Select_datetime_millisecond_component(async);

            AssertSql(
                @"SELECT (CAST(strftime('%f', ""o"".""OrderDate"") AS REAL) * 1000.0) % 1000.0
FROM ""Orders"" AS ""o""");
        }

        public override async Task Select_datetime_DayOfWeek_component(bool async)
        {
            await base.Select_datetime_DayOfWeek_component(async);

            AssertSql(
                @"SELECT CAST(CAST(strftime('%w', ""o"".""OrderDate"") AS INTEGER) AS INTEGER)
FROM ""Orders"" AS ""o""");
        }

        public override async Task Select_datetime_Ticks_component(bool async)
        {
            await base.Select_datetime_Ticks_component(async);

            AssertSql(
                @"SELECT CAST(((julianday(""o"".""OrderDate"") - 1721425.5) * 864000000000.0) AS INTEGER)
FROM ""Orders"" AS ""o""");
        }

        public override async Task Select_datetime_TimeOfDay_component(bool async)
        {
            await base.Select_datetime_TimeOfDay_component(async);

            AssertSql(
                @"SELECT rtrim(rtrim(strftime('%H:%M:%f', ""o"".""OrderDate""), '0'), '.')
FROM ""Orders"" AS ""o""");
        }

        [ConditionalTheory(Skip = "Issue#21541")]
        public override Task Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault_2(bool async)
            => base.Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault_2(async);

        public override async Task SelectMany_correlated_with_outer_1(bool async)
            => Assert.Equal(
                SqliteStrings.ApplyNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.SelectMany_correlated_with_outer_1(async))).Message);

        public override async Task SelectMany_correlated_with_outer_2(bool async)
            => Assert.Equal(
                SqliteStrings.ApplyNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.SelectMany_correlated_with_outer_2(async))).Message);

        public override async Task SelectMany_correlated_with_outer_3(bool async)
            => Assert.Equal(
                SqliteStrings.ApplyNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.SelectMany_correlated_with_outer_3(async))).Message);

        public override async Task SelectMany_correlated_with_outer_4(bool async)
            => Assert.Equal(
                SqliteStrings.ApplyNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.SelectMany_correlated_with_outer_4(async))).Message);

        public override async Task SelectMany_correlated_with_outer_5(bool async)
            => Assert.Equal(
                SqliteStrings.ApplyNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.SelectMany_correlated_with_outer_5(async))).Message);

        public override async Task SelectMany_correlated_with_outer_6(bool async)
            => Assert.Equal(
                SqliteStrings.ApplyNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.SelectMany_correlated_with_outer_6(async))).Message);

        public override async Task SelectMany_correlated_with_outer_7(bool async)
            => Assert.Equal(
                SqliteStrings.ApplyNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.SelectMany_correlated_with_outer_7(async))).Message);

        public override async Task SelectMany_whose_selector_references_outer_source(bool async)
            => Assert.Equal(
                SqliteStrings.ApplyNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.SelectMany_whose_selector_references_outer_source(async))).Message);

        public override async Task Projecting_after_navigation_and_distinct_throws(bool async)
            => Assert.Equal(
                RelationalStrings.InsufficientInformationToIdentifyOuterElementOfCollectionJoin,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Projecting_after_navigation_and_distinct_throws(async))).Message);

        public override async Task Select_nested_collection_deep(bool async)
            => Assert.Equal(
                SqliteStrings.ApplyNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Select_nested_collection_deep(async))).Message);

        [ConditionalTheory(Skip = "Issue#17324")]
        public override Task Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault_2(bool async)
        {
            return base.Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault_2(async);
        }

        public override Task Member_binding_after_ctor_arguments_fails_with_client_eval(bool async)
        {
            return AssertTranslationFailed(() => base.Member_binding_after_ctor_arguments_fails_with_client_eval(async));
        }

        [ConditionalTheory(Skip = "Issue#17230")]
        public override Task SelectMany_with_collection_being_correlated_subquery_which_references_inner_and_outer_entity(bool async)
        {
            return base.SelectMany_with_collection_being_correlated_subquery_which_references_inner_and_outer_entity(async);
        }

        public override Task Reverse_without_explicit_ordering_throws(bool async)
        {
            return AssertTranslationFailedWithDetails(
                () => base.Reverse_without_explicit_ordering_throws(async), RelationalStrings.MissingOrderingInSelectExpression);
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
