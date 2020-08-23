// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindAggregateOperatorsQuerySqliteTest : NorthwindAggregateOperatorsQueryRelationalTestBase<
        NorthwindQuerySqliteFixture<NoopModelCustomizer>>
    {
        public NorthwindAggregateOperatorsQuerySqliteTest(
            NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            ClearLog();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override Task Sum_with_division_on_decimal(bool async)
            => Assert.ThrowsAsync<NotSupportedException>(() => base.Sum_with_division_on_decimal(async));

        public override Task Sum_with_division_on_decimal_no_significant_digits(bool async)
            => Assert.ThrowsAsync<NotSupportedException>(() => base.Sum_with_division_on_decimal_no_significant_digits(async));

        public override Task Average_with_division_on_decimal(bool async)
            => Assert.ThrowsAsync<NotSupportedException>(() => base.Average_with_division_on_decimal(async));

        public override Task Average_with_division_on_decimal_no_significant_digits(bool async)
            => Assert.ThrowsAsync<NotSupportedException>(() => base.Average_with_division_on_decimal_no_significant_digits(async));

        public override async Task Multiple_collection_navigation_with_FirstOrDefault_chained(bool async)
        {
            Assert.Equal(
                SqliteStrings.ApplyNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Multiple_collection_navigation_with_FirstOrDefault_chained(async))).Message);
        }
    }
}
