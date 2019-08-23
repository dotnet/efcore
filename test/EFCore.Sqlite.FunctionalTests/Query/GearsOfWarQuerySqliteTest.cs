// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class GearsOfWarQuerySqliteTest : GearsOfWarQueryTestBase<GearsOfWarQuerySqliteFixture>
    {
        public GearsOfWarQuerySqliteTest(GearsOfWarQuerySqliteFixture fixture)
            : base(fixture)
        {
        }

        // SQLite client-eval
        public override async Task Where_datetimeoffset_date_component(bool isAsync)
        {
            Assert.Equal(
                CoreStrings.TranslationFailed("Where<Mission>(    source: DbSet<Mission>,     predicate: (m) => m.Timeline.Date > (Unhandled parameter: __Date_0))"),
                RemoveNewLines((await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Where_datetimeoffset_date_component(isAsync)))
                .Message));
        }

        // SQLite client-eval
        public override async Task Where_datetimeoffset_day_component(bool isAsync)
        {
            Assert.Equal(
                CoreStrings.TranslationFailed("Where<Mission>(    source: DbSet<Mission>,     predicate: (m) => m.Timeline.Day == 2)"),
                RemoveNewLines((await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Where_datetimeoffset_day_component(isAsync)))
                .Message));
        }

        // SQLite client-eval
        public override async Task Where_datetimeoffset_dayofyear_component(bool isAsync)
        {
            Assert.Equal(
                CoreStrings.TranslationFailed("Where<Mission>(    source: DbSet<Mission>,     predicate: (m) => m.Timeline.DayOfYear == 2)"),
                RemoveNewLines((await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Where_datetimeoffset_dayofyear_component(isAsync)))
                .Message));
        }

        // SQLite client-eval
        public override async Task Where_datetimeoffset_hour_component(bool isAsync)
        {
            Assert.Equal(
                CoreStrings.TranslationFailed("Where<Mission>(    source: DbSet<Mission>,     predicate: (m) => m.Timeline.Hour == 10)"),
                RemoveNewLines((await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Where_datetimeoffset_hour_component(isAsync)))
                .Message));
        }

        // SQLite client-eval
        public override async Task Where_datetimeoffset_millisecond_component(bool isAsync)
        {
            Assert.Equal(
                CoreStrings.TranslationFailed("Where<Mission>(    source: DbSet<Mission>,     predicate: (m) => m.Timeline.Millisecond == 0)"),
                RemoveNewLines((await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Where_datetimeoffset_millisecond_component(isAsync)))
                .Message));
        }

        // SQLite client-eval
        public override async Task Where_datetimeoffset_minute_component(bool isAsync)
        {
            Assert.Equal(
                CoreStrings.TranslationFailed("Where<Mission>(    source: DbSet<Mission>,     predicate: (m) => m.Timeline.Minute == 0)"),
                RemoveNewLines((await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Where_datetimeoffset_minute_component(isAsync)))
                .Message));
        }

        // SQLite client-eval
        public override async Task Where_datetimeoffset_month_component(bool isAsync)
        {
            Assert.Equal(
                CoreStrings.TranslationFailed("Where<Mission>(    source: DbSet<Mission>,     predicate: (m) => m.Timeline.Month == 1)"),
                RemoveNewLines((await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Where_datetimeoffset_month_component(isAsync)))
                .Message));
        }

        // SQLite client-eval
        public override async Task Where_datetimeoffset_now(bool isAsync)
        {
            Assert.Equal(
                CoreStrings.TranslationFailed("Where<Mission>(    source: DbSet<Mission>,     predicate: (m) => m.Timeline != DateTimeOffset.Now)"),
                RemoveNewLines((await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Where_datetimeoffset_now(isAsync)))
                .Message));
        }

        // SQLite client-eval
        public override async Task Where_datetimeoffset_second_component(bool isAsync)
        {
            Assert.Equal(
                CoreStrings.TranslationFailed("Where<Mission>(    source: DbSet<Mission>,     predicate: (m) => m.Timeline.Second == 0)"),
                RemoveNewLines((await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Where_datetimeoffset_second_component(isAsync)))
                .Message));
        }

        // SQLite client-eval
        public override async Task Where_datetimeoffset_utcnow(bool isAsync)
        {
            Assert.Equal(
                CoreStrings.TranslationFailed("Where<Mission>(    source: DbSet<Mission>,     predicate: (m) => m.Timeline != DateTimeOffset.UtcNow)"),
                RemoveNewLines((await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Where_datetimeoffset_utcnow(isAsync)))
                .Message));
        }

        // SQLite client-eval
        public override async Task Where_datetimeoffset_year_component(bool isAsync)
        {
            Assert.Equal(
                CoreStrings.TranslationFailed("Where<Mission>(    source: DbSet<Mission>,     predicate: (m) => m.Timeline.Year == 2)"),
                RemoveNewLines((await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Where_datetimeoffset_year_component(isAsync)))
                .Message));
        }

        // SQLite client-eval
        public override async Task DateTimeOffset_Contains_Less_than_Greater_than(bool isAsync)
        {
            Assert.Equal(
                CoreStrings.TranslationFailed(
                    "Where<Mission>(    source: DbSet<Mission>,     predicate: (m) => (Unhandled parameter: __start_0) <= (DateTimeOffset)m.Timeline.Date && m.Timeline < (Unhandled parameter: __end_1) && Contains<DateTimeOffset>(        source: (Unhandled parameter: __dates_2),         value: m.Timeline))"),
                RemoveNewLines(
                    (await Assert.ThrowsAsync<InvalidOperationException>(
                        () => base.DateTimeOffset_Contains_Less_than_Greater_than(isAsync)))
                    .Message));
        }

        private string RemoveNewLines(string message)
            => message.Replace("\n", "").Replace("\r", "");

        // Sqlite does not support cross/outer apply
        public override Task Correlated_collections_inner_subquery_predicate_references_outer_qsre(bool isAsync) => null;

        public override Task Correlated_collections_inner_subquery_selector_references_outer_qsre(bool isAsync) => null;

        public override Task Correlated_collections_nested_inner_subquery_references_outer_qsre_one_level_up(bool isAsync) => null;

        public override Task Correlated_collections_nested_inner_subquery_references_outer_qsre_two_levels_up(bool isAsync) => null;

        public override Task Outer_parameter_in_group_join_with_DefaultIfEmpty(bool isAsync) => null;

        public override Task Outer_parameter_in_join_key(bool isAsync) => null;

        public override Task Outer_parameter_in_join_key_inner_and_outer(bool isAsync) => null;
    }
}
