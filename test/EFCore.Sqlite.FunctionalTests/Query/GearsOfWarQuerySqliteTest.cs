// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class GearsOfWarQuerySqliteTest : GearsOfWarQueryTestBase<GearsOfWarQuerySqliteFixture>
    {
        public GearsOfWarQuerySqliteTest(GearsOfWarQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override Task Where_datetimeoffset_date_component(bool async)
            => AssertTranslationFailed(() => base.Where_datetimeoffset_date_component(async));

        public override Task Where_datetimeoffset_day_component(bool async)
            => AssertTranslationFailed(() => base.Where_datetimeoffset_date_component(async));

        public override Task Where_datetimeoffset_dayofyear_component(bool async)
            => AssertTranslationFailed(() => base.Where_datetimeoffset_dayofyear_component(async));

        public override Task Where_datetimeoffset_hour_component(bool async)
            => AssertTranslationFailed(() => base.Where_datetimeoffset_hour_component(async));

        public override Task Where_datetimeoffset_millisecond_component(bool async)
            => AssertTranslationFailed(() => base.Where_datetimeoffset_millisecond_component(async));

        public override Task Where_datetimeoffset_minute_component(bool async)
            => AssertTranslationFailed(() => base.Where_datetimeoffset_minute_component(async));

        public override Task Where_datetimeoffset_month_component(bool async)
            => AssertTranslationFailed(() => base.Where_datetimeoffset_month_component(async));

        public override Task Where_datetimeoffset_now(bool async)
            => AssertTranslationFailed(() => base.Where_datetimeoffset_now(async));

        public override Task Where_datetimeoffset_second_component(bool async)
            => AssertTranslationFailed(() => base.Where_datetimeoffset_second_component(async));

        public override Task Where_datetimeoffset_utcnow(bool async)
            => AssertTranslationFailed(() => base.Where_datetimeoffset_utcnow(async));

        public override Task Where_datetimeoffset_year_component(bool async)
            => AssertTranslationFailed(() => base.Where_datetimeoffset_year_component(async));

        public override Task DateTimeOffset_Contains_Less_than_Greater_than(bool async)
            => AssertTranslationFailed(() => base.DateTimeOffset_Contains_Less_than_Greater_than(async));

        public override Task DateTimeOffset_Date_returns_datetime(bool async)
            => AssertTranslationFailed(() => base.DateTimeOffset_Date_returns_datetime(async));

        // Sqlite does not support cross/outer apply
        public override Task Correlated_collections_inner_subquery_predicate_references_outer_qsre(bool async) => null;

        public override Task Correlated_collections_inner_subquery_selector_references_outer_qsre(bool async) => null;

        public override Task Correlated_collections_nested_inner_subquery_references_outer_qsre_one_level_up(bool async) => null;

        public override Task Correlated_collections_nested_inner_subquery_references_outer_qsre_two_levels_up(bool async) => null;

        public override Task Outer_parameter_in_group_join_with_DefaultIfEmpty(bool async) => null;

        public override Task Outer_parameter_in_join_key(bool async) => null;

        public override Task Outer_parameter_in_join_key_inner_and_outer(bool async) => null;

        [ConditionalTheory(Skip = "Issue #17230")]
        public override Task Project_collection_navigation_nested_with_take_composite_key(bool async)
            => base.Project_collection_navigation_nested_with_take_composite_key(async);

        public override async Task Select_datetimeoffset_comparison_in_projection(bool async)
        {
            await base.Select_datetimeoffset_comparison_in_projection(async);

            AssertSql(
                @"SELECT ""m"".""Timeline""
FROM ""Missions"" AS ""m""");
        }

        public override async Task Byte_array_contains_literal(bool async)
        {
            await base.Byte_array_contains_literal(async);

            AssertSql(
                @"SELECT ""s"".""Id"", ""s"".""Banner"", ""s"".""Banner5"", ""s"".""InternalNumber"", ""s"".""Name""
FROM ""Squads"" AS ""s""
WHERE instr(""s"".""Banner"", X'01') > 0");
        }

        public override async Task Byte_array_contains_parameter(bool async)
        {
            await base.Byte_array_contains_parameter(async);

            AssertSql(
                @"@__someByte_0='1' (DbType = String)

SELECT ""s"".""Id"", ""s"".""Banner"", ""s"".""Banner5"", ""s"".""InternalNumber"", ""s"".""Name""
FROM ""Squads"" AS ""s""
WHERE instr(""s"".""Banner"", char(@__someByte_0)) > 0");
        }

        public override async Task Byte_array_filter_by_length_literal(bool async)
        {
            await base.Byte_array_filter_by_length_literal(async);

            AssertSql(@"SELECT ""s"".""Id"", ""s"".""Banner"", ""s"".""Banner5"", ""s"".""InternalNumber"", ""s"".""Name""
FROM ""Squads"" AS ""s""
WHERE length(""s"".""Banner"") = 1");
        }

        public override async Task Byte_array_filter_by_length_parameter(bool async)
        {
            await base.Byte_array_filter_by_length_parameter(async);

            AssertSql(@"@__p_0='1' (DbType = String)

SELECT ""s"".""Id"", ""s"".""Banner"", ""s"".""Banner5"", ""s"".""InternalNumber"", ""s"".""Name""
FROM ""Squads"" AS ""s""
WHERE length(""s"".""Banner"") = @__p_0");
        }

        public override void Byte_array_filter_by_length_parameter_compiled()
        {
            base.Byte_array_filter_by_length_parameter_compiled();

            AssertSql(
                @"@__byteArrayParam='0x2A80' (Size = 2) (DbType = String)

SELECT COUNT(*)
FROM ""Squads"" AS ""s""
WHERE length(""s"".""Banner"") = length(@__byteArrayParam)");
        }

        public override async Task Byte_array_filter_by_SequenceEqual(bool async)
        {
            await base.Byte_array_filter_by_SequenceEqual(async);

            AssertSql(@"@__byteArrayParam_0='0x0405060708' (Size = 5) (DbType = String)

SELECT ""s"".""Id"", ""s"".""Banner"", ""s"".""Banner5"", ""s"".""InternalNumber"", ""s"".""Name""
FROM ""Squads"" AS ""s""
WHERE ""s"".""Banner5"" = @__byteArrayParam_0");
        }

        public override async Task TimeSpan_Hours(bool async)
        {
            await base.TimeSpan_Hours(async);

            AssertSql(
                @"SELECT ef_mod(ef_days(""m"".""Duration""), 1.0) * 24.0
FROM ""Missions"" AS ""m""");
        }

        public override async Task TimeSpan_Minutes(bool async)
        {
            await base.TimeSpan_Minutes(async);

            AssertSql(
                @"SELECT ef_mod(ef_days(""m"".""Duration""), 0.041666666666666664) * 1440.0
FROM ""Missions"" AS ""m""");
        }

        public override async Task TimeSpan_Seconds(bool async)
        {
            await base.TimeSpan_Seconds(async);

            AssertSql(
                @"SELECT round(ef_mod(ef_days(""m"".""Duration""), 0.00069444444444444447) * 86400.0, 3)
FROM ""Missions"" AS ""m""");
        }

        public override async Task TimeSpan_Milliseconds(bool async)
        {
            await base.TimeSpan_Milliseconds(async);

            AssertSql(
                @"SELECT ef_mod(ef_days(""m"".""Duration""), 1.1574074074074073E-05) * 86400000.0
FROM ""Missions"" AS ""m""");
        }

        public override async Task Where_TimeSpan_Days(bool async)
        {
            await base.Where_TimeSpan_Days(async);

            AssertSql(
                @"SELECT ""m"".""Id"", ""m"".""CodeName"", ""m"".""Duration"", ""m"".""Rating"", ""m"".""Timeline""
FROM ""Missions"" AS ""m""
WHERE CAST(ef_days(""m"".""Duration"") AS INTEGER) = 1");
        }

        public override async Task Where_TimeSpan_Ticks(bool async)
        {
            await base.Where_TimeSpan_Ticks(async);

            AssertSql(
                @"SELECT ""m"".""Id"", ""m"".""CodeName"", ""m"".""Duration"", ""m"".""Rating"", ""m"".""Timeline""
FROM ""Missions"" AS ""m""
WHERE CAST((ef_days(""m"".""Duration"") * 864000000000.0) AS INTEGER) = 1");
        }

        // TODO
        // JulianDay collapses strftime
        // JulianDay collapses strftime from double without modifiers
        // Days collapses timespan

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
