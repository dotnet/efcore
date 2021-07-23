﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class TPTManyToManyNoTrackingQuerySqliteTest : TPTManyToManyNoTrackingQueryRelationalTestBase<TPTManyToManyQuerySqliteFixture>
    {
        public TPTManyToManyNoTrackingQuerySqliteTest(TPTManyToManyQuerySqliteFixture fixture)
            : base(fixture)
        {
        }

        public override async Task Skip_navigation_order_by_single_or_default(bool async)
            => Assert.Equal(
                SqliteStrings.ApplyNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Skip_navigation_order_by_single_or_default(async))).Message);

        public override async Task Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where(bool async)
            => Assert.Equal(
                SqliteStrings.ApplyNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where(async))).Message);
    }
}
