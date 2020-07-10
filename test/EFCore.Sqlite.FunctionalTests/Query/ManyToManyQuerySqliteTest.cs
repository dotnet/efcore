// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ManyToManyQuerySqliteTest : ManyToManyQueryRelationalTestBase<ManyToManyQuerySqliteFixture>
    {
        public ManyToManyQuerySqliteTest(ManyToManyQuerySqliteFixture fixture)
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

        [ConditionalTheory(Skip = "Issue#21541")]
        public override Task Left_join_with_skip_navigation(bool async)
            => Task.CompletedTask;
    }
}
