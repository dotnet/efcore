// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ComplexNavigationsCollectionsSplitSharedTypeQuerySqliteTest : ComplexNavigationsCollectionsSplitSharedQueryTypeRelationalTestBase<
        ComplexNavigationsSharedTypeQuerySqliteFixture>
    {
        public ComplexNavigationsCollectionsSplitSharedTypeQuerySqliteTest(ComplexNavigationsSharedTypeQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
        }

        public override async Task Filtered_include_outer_parameter_used_inside_filter(bool async)
            => Assert.Equal(
                SqliteStrings.ApplyNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Filtered_include_outer_parameter_used_inside_filter(async))).Message);

        public override async Task Filtered_include_and_non_filtered_include_followed_by_then_include_on_same_navigation(bool async)
            => Assert.Equal(
                SqliteStrings.ApplyNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Filtered_include_and_non_filtered_include_followed_by_then_include_on_same_navigation(async))).Message);

        public override async Task Filtered_include_multiple_multi_level_includes_with_first_level_using_filter_include_on_one_of_the_chains_only(
            bool async)
            => Assert.Equal(
                SqliteStrings.ApplyNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Filtered_include_multiple_multi_level_includes_with_first_level_using_filter_include_on_one_of_the_chains_only(async))).Message);

        public override async Task Filtered_include_same_filter_set_on_same_navigation_twice_followed_by_ThenIncludes(bool async)
            => Assert.Equal(
                SqliteStrings.ApplyNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Filtered_include_same_filter_set_on_same_navigation_twice_followed_by_ThenIncludes(async))).Message);

        public override async Task Complex_query_with_let_collection_projection_FirstOrDefault(bool async)
            => Assert.Equal(
                SqliteStrings.ApplyNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Complex_query_with_let_collection_projection_FirstOrDefault(async))).Message);

        public override async Task Take_Select_collection_Take(bool async)
            => Assert.Equal(
                SqliteStrings.ApplyNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Take_Select_collection_Take(async))).Message);

        public override async Task Skip_Take_Select_collection_Skip_Take(bool async)
            => Assert.Equal(
                SqliteStrings.ApplyNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Skip_Take_Select_collection_Skip_Take(async))).Message);
    }
}
