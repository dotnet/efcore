// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class GearsOfWarQuerySqliteTest : GearsOfWarQueryTestBase<GearsOfWarQuerySqliteFixture>
    {
        public GearsOfWarQuerySqliteTest(GearsOfWarQuerySqliteFixture fixture)
            : base(fixture)
        {
        }

        [Fact(Skip = "Partially implementing for SQLite")]
        public override void DateTimeOffset_DateAdd_AddYears()
        {
            base.DateTimeOffset_DateAdd_AddYears();
        }

        [Fact(Skip = "Partially implementing for SQLite")]
        public override void DateTimeOffset_DateAdd_AddMinutes()
        {
            base.DateTimeOffset_DateAdd_AddMinutes();
        }

        [Fact(Skip = "Partially implementing for SQLite")]
        public override void DateTimeOffset_DateAdd_AddSeconds()
        {
            base.DateTimeOffset_DateAdd_AddSeconds();
        }

        [Fact(Skip = "Partially implementing for SQLite")]
        public override void DateTimeOffset_DateAdd_AddHours()
        {
            base.DateTimeOffset_DateAdd_AddHours();
        }

        [Fact(Skip = "Partially implementing for SQLite")]
        public override void DateTimeOffset_DateAdd_AddMonths()
        {
            base.DateTimeOffset_DateAdd_AddMonths();
        }

        [Fact(Skip = "Partially implementing for SQLite")]
        public override void DateTimeOffset_DateAdd_AddDays()
        {
            base.DateTimeOffset_DateAdd_AddDays();
        }

        [Fact(Skip = "Partially implementing for SQLite")]
        public override void DateTimeOffset_DateAdd_AddMilliseconds()
        {
            base.DateTimeOffset_DateAdd_AddMilliseconds();
        }
    }
}
