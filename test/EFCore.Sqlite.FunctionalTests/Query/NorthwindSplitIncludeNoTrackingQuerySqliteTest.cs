﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindSplitIncludeNoTrackingQuerySqliteTest : NorthwindSplitIncludeNoTrackingQueryTestBase<
        NorthwindQuerySqliteFixture<NoopModelCustomizer>>
    {
        public NorthwindSplitIncludeNoTrackingQuerySqliteTest(
            NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
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
    }
}
