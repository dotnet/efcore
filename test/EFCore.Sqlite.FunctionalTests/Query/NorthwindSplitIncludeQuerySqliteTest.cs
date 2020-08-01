// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindSplitIncludeQuerySqliteTest : NorthwindSplitIncludeQueryTestBase<NorthwindQuerySqliteFixture<NoopModelCustomizer>>
    {
        public NorthwindSplitIncludeQuerySqliteTest(NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestSqlLoggerFactory.CaptureOutput(testOutputHelper);
        }

        public override async Task Include_collection_with_cross_apply_with_filter(bool async)
            => Assert.Equal(
                SqliteStrings.ApplyNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Include_collection_with_cross_apply_with_filter(async))).Message);

        public override async Task Include_collection_with_outer_apply_with_filter(bool async)
            => Assert.Equal(
                SqliteStrings.ApplyNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Include_collection_with_outer_apply_with_filter(async))).Message);

        public override async Task Filtered_include_with_multiple_ordering(bool async)
            => Assert.Equal(
                SqliteStrings.ApplyNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Filtered_include_with_multiple_ordering(async))).Message);

        public override async Task Include_collection_with_outer_apply_with_filter_non_equality(bool async)
            => Assert.Equal(
                SqliteStrings.ApplyNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Include_collection_with_outer_apply_with_filter_non_equality(async))).Message);

        public override async Task Include_collection_with_last_no_orderby(bool async)
        {
            var expectedMessage = CoreStrings.TranslationFailedWithDetails("DbSet<Customer>()    .Reverse()",
                RelationalStrings.MissingOrderingInSqlExpression);

            var exception = (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => AssertLast(
                        async,
                        ss => ss.Set<Customer>().Include(c => c.Orders),
                        entryCount: 8)));

            Assert.Equal(
                expectedMessage,
                exception.Message.Replace("\r","").Replace("\n",""));
        }
    }
}
