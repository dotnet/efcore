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
        public override Task Where_datetimeoffset_date_component(bool isAsync)
        {
            return AssertTranslationFailed(() => base.Where_datetimeoffset_date_component(isAsync));
        }

        // SQLite client-eval
        public override Task Where_datetimeoffset_day_component(bool isAsync)
        {
            return AssertTranslationFailed(() => base.Where_datetimeoffset_date_component(isAsync));
        }

        // SQLite client-eval
        public override Task Where_datetimeoffset_dayofyear_component(bool isAsync)
        {
            return AssertTranslationFailed(() => base.Where_datetimeoffset_dayofyear_component(isAsync));
        }

        // SQLite client-eval
        public override Task Where_datetimeoffset_hour_component(bool isAsync)
        {
            return AssertTranslationFailed(() => base.Where_datetimeoffset_hour_component(isAsync));
        }

        // SQLite client-eval
        public override Task Where_datetimeoffset_millisecond_component(bool isAsync)
        {
            return AssertTranslationFailed(() => base.Where_datetimeoffset_millisecond_component(isAsync));
        }

        // SQLite client-eval
        public override Task Where_datetimeoffset_minute_component(bool isAsync)
        {
            return AssertTranslationFailed(() => base.Where_datetimeoffset_minute_component(isAsync));
        }

        // SQLite client-eval
        public override Task Where_datetimeoffset_month_component(bool isAsync)
        {
            return AssertTranslationFailed(() => base.Where_datetimeoffset_month_component(isAsync));
        }

        // SQLite client-eval
        public override Task Where_datetimeoffset_now(bool isAsync)
        {
            return AssertTranslationFailed(() => base.Where_datetimeoffset_now(isAsync));
        }

        // SQLite client-eval
        public override Task Where_datetimeoffset_second_component(bool isAsync)
        {
            return AssertTranslationFailed(() => base.Where_datetimeoffset_second_component(isAsync));
        }

        // SQLite client-eval
        public override Task Where_datetimeoffset_utcnow(bool isAsync)
        {
            return AssertTranslationFailed(() => base.Where_datetimeoffset_utcnow(isAsync));
        }

        // SQLite client-eval
        public override Task Where_datetimeoffset_year_component(bool isAsync)
        {
            return AssertTranslationFailed(() => base.Where_datetimeoffset_year_component(isAsync));
        }

        // SQLite client-eval
        public override Task DateTimeOffset_Contains_Less_than_Greater_than(bool isAsync)
        {
            return AssertTranslationFailed(() => base.DateTimeOffset_Contains_Less_than_Greater_than(isAsync));
        }

        // Sqlite does not support cross/outer apply
        public override Task Correlated_collections_inner_subquery_predicate_references_outer_qsre(bool isAsync) => null;

        public override Task Correlated_collections_inner_subquery_selector_references_outer_qsre(bool isAsync) => null;

        public override Task Correlated_collections_nested_inner_subquery_references_outer_qsre_one_level_up(bool isAsync) => null;

        public override Task Correlated_collections_nested_inner_subquery_references_outer_qsre_two_levels_up(bool isAsync) => null;

        public override Task Outer_parameter_in_group_join_with_DefaultIfEmpty(bool isAsync) => null;

        public override Task Outer_parameter_in_join_key(bool isAsync) => null;

        public override Task Outer_parameter_in_join_key_inner_and_outer(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #17230")]
        public override Task Project_collection_navigation_nested_with_take_composite_key(bool isAsync)
        {
            return base.Project_collection_navigation_nested_with_take_composite_key(isAsync);
        }

        public override async Task Select_datetimeoffset_comparison_in_projection(bool isAsync)
        {
            await base.Select_datetimeoffset_comparison_in_projection(isAsync);

            AssertSql(
                @"SELECT ""m"".""Timeline""
FROM ""Missions"" AS ""m""");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
