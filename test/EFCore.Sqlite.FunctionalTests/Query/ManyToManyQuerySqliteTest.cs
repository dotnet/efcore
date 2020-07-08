// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ManyToManyQuerySqliteTest : ManyToManyQueryRelationalTestBase<ManyToManyQuerySqliteFixture>
    {
        public ManyToManyQuerySqliteTest(ManyToManyQuerySqliteFixture fixture)
            : base(fixture)
        {
        }

        // Sqlite does not support Apply operations

        public override Task Skip_navigation_order_by_single_or_default(bool async)
            => Task.CompletedTask;

        public override Task Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where(bool async)
            => Task.CompletedTask;

        [ConditionalTheory(Skip = "Issue#21541")]
        public override Task Left_join_with_skip_navigation(bool async)
            => Task.CompletedTask;

        [ConditionalTheory(Skip = "TODO: query translation #19003")]
        public override Task Filter_include_on_skip_navigation_combined_split(bool async)
            => Task.CompletedTask;

        [ConditionalTheory(Skip = "TODO: query translation #19003")]
        public override Task Filter_include_on_skip_navigation_combined_with_filtered_then_includes_split(bool async)
            => Task.CompletedTask;

        [ConditionalTheory(Skip = "TODO: query translation #19003")]
        public override Task Filtered_include_on_navigation_then_filtered_include_on_skip_navigation_split(bool async)
            => Task.CompletedTask;

        [ConditionalTheory(Skip = "TODO: query translation #19003")]
        public override Task Filtered_include_on_skip_navigation_then_filtered_include_on_navigation_split(bool async)
            => Task.CompletedTask;

        [ConditionalTheory(Skip = "TODO: query translation #19003")]
        public override Task Filtered_include_skip_navigation_order_by_skip_split(bool async)
            => Task.CompletedTask;

        [ConditionalTheory(Skip = "TODO: query translation #19003")]
        public override Task Filtered_include_skip_navigation_order_by_skip_take_split(bool async)
            => Task.CompletedTask;

        [ConditionalTheory(Skip = "TODO: query translation #19003")]
        public override Task Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where_split(bool async)
            => Task.CompletedTask;

        [ConditionalTheory(Skip = "TODO: query translation #19003")]
        public override Task Filtered_include_skip_navigation_order_by_split(bool async)
            => Task.CompletedTask;

        [ConditionalTheory(Skip = "TODO: query translation #19003")]
        public override Task Filtered_include_skip_navigation_order_by_take_split(bool async)
            => Task.CompletedTask;

        [ConditionalTheory(Skip = "TODO: query translation #19003")]
        public override Task Filtered_include_skip_navigation_where_split(bool async)
            => Task.CompletedTask;

        [ConditionalTheory(Skip = "TODO: query translation #19003")]
        public override Task Filtered_include_skip_navigation_where_then_include_skip_navigation_order_by_skip_take_split(bool async)
            => Task.CompletedTask;

        [ConditionalTheory(Skip = "TODO: query translation #19003")]
        public override Task Filtered_include_skip_navigation_where_then_include_skip_navigation_split(bool async)
            => Task.CompletedTask;

        [ConditionalTheory(Skip = "TODO: query translation #19003")]
        public override Task Filtered_then_include_skip_navigation_order_by_skip_take_split(bool async)
            => Task.CompletedTask;

        [ConditionalTheory(Skip = "TODO: query translation #19003")]
        public override Task Filtered_then_include_skip_navigation_where_split(bool async)
            => Task.CompletedTask;

        [ConditionalTheory(Skip = "TODO: query translation #19003")]
        public override Task Filtered_include_skip_navigation_where(bool async)
            => Task.CompletedTask;

        [ConditionalTheory(Skip = "TODO: query translation #19003")]
        public override Task Filtered_include_skip_navigation_order_by(bool async)
            => Task.CompletedTask;

        [ConditionalTheory(Skip = "TODO: query translation #19003")]
        public override Task Filtered_include_skip_navigation_order_by_skip(bool async)
            => Task.CompletedTask;

        [ConditionalTheory(Skip = "TODO: query translation #19003")]
        public override Task Filtered_include_skip_navigation_order_by_take(bool async)
            => Task.CompletedTask;

        [ConditionalTheory(Skip = "TODO: query translation #19003")]
        public override Task Filtered_include_skip_navigation_order_by_skip_take(bool async)
            => Task.CompletedTask;

        [ConditionalTheory(Skip = "TODO: query translation #19003")]
        public override Task Filtered_then_include_skip_navigation_where(bool async)
            => Task.CompletedTask;

        [ConditionalTheory(Skip = "TODO: query translation #19003")]
        public override Task Filtered_then_include_skip_navigation_order_by_skip_take(bool async)
            => Task.CompletedTask;

        [ConditionalTheory(Skip = "TODO: query translation #19003")]
        public override Task Filtered_include_skip_navigation_where_then_include_skip_navigation(bool async)
            => Task.CompletedTask;

        [ConditionalTheory(Skip = "TODO: query translation #19003")]
        public override Task Filtered_include_skip_navigation_where_then_include_skip_navigation_order_by_skip_take(bool async)
            => Task.CompletedTask;

        [ConditionalTheory(Skip = "TODO: query translation #19003")]
        public override Task Filter_include_on_skip_navigation_combined(bool async)
            => Task.CompletedTask;

        [ConditionalTheory(Skip = "TODO: query translation #19003")]
        public override Task Filter_include_on_skip_navigation_combined_with_filtered_then_includes(bool async)
            => Task.CompletedTask;

        [ConditionalTheory(Skip = "TODO: query translation #19003")]
        public override Task Filtered_include_on_skip_navigation_then_filtered_include_on_navigation(bool async)
            => Task.CompletedTask;

        [ConditionalTheory(Skip = "TODO: query translation #19003")]
        public override Task Filtered_include_on_navigation_then_filtered_include_on_skip_navigation(bool async)
            => Task.CompletedTask;

        public override Task Include_skip_navigation_then_include_inverse_throws_in_no_tracking(bool async)
            => Task.CompletedTask;
    }
}
