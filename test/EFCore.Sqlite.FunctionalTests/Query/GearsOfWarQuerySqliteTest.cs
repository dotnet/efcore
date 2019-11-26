// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class GearsOfWarQuerySqliteTest : GearsOfWarQueryTestBase<GearsOfWarQuerySqliteFixture>
    {
        public GearsOfWarQuerySqliteTest(GearsOfWarQuerySqliteFixture fixture)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
        }

        // SQLite client-eval
        public override Task Where_datetimeoffset_date_component(bool async)
        {
            return AssertTranslationFailed(() => base.Where_datetimeoffset_date_component(async));
        }

        // SQLite client-eval
        public override Task Where_datetimeoffset_day_component(bool async)
        {
            return AssertTranslationFailed(() => base.Where_datetimeoffset_date_component(async));
        }

        // SQLite client-eval
        public override Task Where_datetimeoffset_dayofyear_component(bool async)
        {
            return AssertTranslationFailed(() => base.Where_datetimeoffset_dayofyear_component(async));
        }

        // SQLite client-eval
        public override Task Where_datetimeoffset_hour_component(bool async)
        {
            return AssertTranslationFailed(() => base.Where_datetimeoffset_hour_component(async));
        }

        // SQLite client-eval
        public override Task Where_datetimeoffset_millisecond_component(bool async)
        {
            return AssertTranslationFailed(() => base.Where_datetimeoffset_millisecond_component(async));
        }

        // SQLite client-eval
        public override Task Where_datetimeoffset_minute_component(bool async)
        {
            return AssertTranslationFailed(() => base.Where_datetimeoffset_minute_component(async));
        }

        // SQLite client-eval
        public override Task Where_datetimeoffset_month_component(bool async)
        {
            return AssertTranslationFailed(() => base.Where_datetimeoffset_month_component(async));
        }

        // SQLite client-eval
        public override Task Where_datetimeoffset_now(bool async)
        {
            return AssertTranslationFailed(() => base.Where_datetimeoffset_now(async));
        }

        // SQLite client-eval
        public override Task Where_datetimeoffset_second_component(bool async)
        {
            return AssertTranslationFailed(() => base.Where_datetimeoffset_second_component(async));
        }

        // SQLite client-eval
        public override Task Where_datetimeoffset_utcnow(bool async)
        {
            return AssertTranslationFailed(() => base.Where_datetimeoffset_utcnow(async));
        }

        // SQLite client-eval
        public override Task Where_datetimeoffset_year_component(bool async)
        {
            return AssertTranslationFailed(() => base.Where_datetimeoffset_year_component(async));
        }

        // SQLite client-eval
        public override Task DateTimeOffset_Contains_Less_than_Greater_than(bool async)
        {
            return AssertTranslationFailed(() => base.DateTimeOffset_Contains_Less_than_Greater_than(async));
        }

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
        {
            return base.Project_collection_navigation_nested_with_take_composite_key(async);
        }

        public override async Task Select_datetimeoffset_comparison_in_projection(bool async)
        {
            await base.Select_datetimeoffset_comparison_in_projection(async);

            AssertSql(
                @"SELECT ""m"".""Timeline""
FROM ""Missions"" AS ""m""");
        }

        public override Task Byte_array_contains_literal(bool async) => null;

        public override Task Byte_array_contains_parameter(bool async) => null;

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
