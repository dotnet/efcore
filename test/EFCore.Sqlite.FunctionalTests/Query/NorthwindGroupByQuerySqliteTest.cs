// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindGroupByQuerySqliteTest : NorthwindGroupByQueryRelationalTestBase<NorthwindQuerySqliteFixture<NoopModelCustomizer>>
    {
        // ReSharper disable once UnusedParameter.Local
        public NorthwindGroupByQuerySqliteTest(NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task Select_uncorrelated_collection_with_groupby_multiple_collections_work(bool async)
            => Assert.Equal(
                SqliteStrings.ApplyNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Select_uncorrelated_collection_with_groupby_multiple_collections_work(async))).Message);

        public override async Task Select_uncorrelated_collection_with_groupby_works(bool async)
            => Assert.Equal(
                SqliteStrings.ApplyNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Select_uncorrelated_collection_with_groupby_works(async))).Message);

        public override async Task Select_uncorrelated_collection_with_groupby_when_outer_is_distinct(bool async)
            => Assert.Equal(
                SqliteStrings.ApplyNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Select_uncorrelated_collection_with_groupby_when_outer_is_distinct(async))).Message);

        public override async Task AsEnumerable_in_subquery_for_GroupBy(bool async)
            => Assert.Equal(
                SqliteStrings.ApplyNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.AsEnumerable_in_subquery_for_GroupBy(async))).Message);

        public override async Task Complex_query_with_groupBy_in_subquery1(bool async)
            => Assert.Equal(
                SqliteStrings.ApplyNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Complex_query_with_groupBy_in_subquery1(async))).Message);

        public override async Task Complex_query_with_groupBy_in_subquery2(bool async)
            => Assert.Equal(
                SqliteStrings.ApplyNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Complex_query_with_groupBy_in_subquery2(async))).Message);

        public override async Task Complex_query_with_groupBy_in_subquery3(bool async)
            => Assert.Equal(
                SqliteStrings.ApplyNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Complex_query_with_groupBy_in_subquery3(async))).Message);

        public override async Task Select_nested_collection_with_groupby(bool async)
            => Assert.Equal(
                SqliteStrings.ApplyNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Select_nested_collection_with_groupby(async))).Message);
    }
}
