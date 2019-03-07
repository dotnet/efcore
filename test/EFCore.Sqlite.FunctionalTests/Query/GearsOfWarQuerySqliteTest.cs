// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class GearsOfWarQuerySqliteTest : GearsOfWarQueryTestBase<GearsOfWarQuerySqliteFixture>
    {
        public GearsOfWarQuerySqliteTest(GearsOfWarQuerySqliteFixture fixture)
            : base(fixture)
        {
        }

        // Skip for SQLite. Issue #14935. Cannot eval 'where ([m].Timeline.Date > 1/1/0001 12:00:00 AM)'
        public override Task String_concat_with_null_conditional_argument2(bool isAsync) => null;

        // Skip for SQLite. Issue #14935. Cannot eval 'where ([m].Timeline.Date > 1/1/0001 12:00:00 AM)'
        public override Task Where_datetimeoffset_date_component(bool isAsync) => null;

        // Skip for SQLite. Issue #14935. Cannot eval 'where ([m].Timeline.Day == 2)'
        public override Task Where_datetimeoffset_day_component(bool isAsync) => null;

        // Skip for SQLite. Issue #14935. Cannot eval 'where ([m].Timeline.DayOfYear == 2)'
        public override Task Where_datetimeoffset_dayofyear_component(bool isAsync) => null;

        // Skip for SQLite. Issue #14935. Cannot eval 'where ([m].Timeline.Hour == 10)'
        public override Task Where_datetimeoffset_hour_component(bool isAsync) => null;

        // Skip for SQLite. Issue #14935. Cannot eval 'where ([m].Timeline.Millisecond == 0)'
        public override Task Where_datetimeoffset_millisecond_component(bool isAsync) => null;

        // Skip for SQLite. Issue #14935. Cannot eval 'where ([m].Timeline.Minute == 0)'
        public override Task Where_datetimeoffset_minute_component(bool isAsync) => null;

        // Skip for SQLite. Issue #14935. Cannot eval 'where ([m].Timeline.Month == 1)'
        public override Task Where_datetimeoffset_month_component(bool isAsync) => null;

        // Skip for SQLite. Issue #14935. Cannot eval 'where ([m].Timeline != DateTimeOffset.Now)'
        public override Task Where_datetimeoffset_now(bool isAsync) => null;

        // Skip for SQLite. Issue #14935. Cannot eval 'where ([m].Timeline.Second == 0)'
        public override Task Where_datetimeoffset_second_component(bool isAsync) => null;

        // Skip for SQLite. Issue #14935. Cannot eval 'where ([m].Timeline != DateTimeOffset.UtcNow)'
        public override Task Where_datetimeoffset_utcnow(bool isAsync) => null;

        // Skip for SQLite. Issue #14935. Cannot eval 'where ([m].Timeline.Year == 2)'
        public override Task Where_datetimeoffset_year_component(bool isAsync) => null;
    }
}
