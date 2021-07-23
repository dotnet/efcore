﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ComplexNavigationsCollectionsSharedTypeQuerySqliteTest : ComplexNavigationsCollectionsSharedQueryTypeRelationalTestBase<
        ComplexNavigationsSharedTypeQuerySqliteFixture>
    {
        public ComplexNavigationsCollectionsSharedTypeQuerySqliteTest(ComplexNavigationsSharedTypeQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
        }

        public override async Task Filtered_include_after_different_filtered_include_different_level(bool async)
            => Assert.Equal(
                SqliteStrings.ApplyNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Filtered_include_after_different_filtered_include_different_level(async))).Message);

        public override async Task Filtered_include_and_non_filtered_include_followed_by_then_include_on_same_navigation(bool async)
            => Assert.Equal(
                SqliteStrings.ApplyNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Filtered_include_and_non_filtered_include_followed_by_then_include_on_same_navigation(async))).Message);

        public override async Task Filtered_include_complex_three_level_with_middle_having_filter1(bool async)
            => Assert.Equal(
                SqliteStrings.ApplyNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Filtered_include_complex_three_level_with_middle_having_filter1(async))).Message);

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

        public override async Task Filtered_include_complex_three_level_with_middle_having_filter2(bool async)
            => Assert.Equal(
                SqliteStrings.ApplyNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Filtered_include_complex_three_level_with_middle_having_filter2(async))).Message);

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

        public override async Task Filtered_include_Take_with_another_Take_on_top_level(bool async)
            => Assert.Equal(
                SqliteStrings.ApplyNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Filtered_include_Take_with_another_Take_on_top_level(async))).Message);

        public override async Task Filtered_include_Skip_Take_with_another_Skip_Take_on_top_level(bool async)
            => Assert.Equal(
                SqliteStrings.ApplyNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Filtered_include_Skip_Take_with_another_Skip_Take_on_top_level(async))).Message);
    }
}
