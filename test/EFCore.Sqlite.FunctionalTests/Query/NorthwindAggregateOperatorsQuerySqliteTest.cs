// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindAggregateOperatorsQuerySqliteTest : NorthwindAggregateOperatorsQueryTestBase<NorthwindQuerySqliteFixture<NoopModelCustomizer>>
    {
        public NorthwindAggregateOperatorsQuerySqliteTest(NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            ClearLog();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        // SQLite client-eval
        public override async Task Sum_with_division_on_decimal(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Sum_with_division_on_decimal(async)))
                .Message);
        }

        // SQLite client-eval
        public override async Task Sum_with_division_on_decimal_no_significant_digits(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Sum_with_division_on_decimal_no_significant_digits(async)))
                .Message);
        }

        // SQLite client-eval
        public override async Task Average_with_division_on_decimal(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Average_with_division_on_decimal(async)))
                .Message);
        }

        // SQLite client-eval
        public override async Task Average_with_division_on_decimal_no_significant_digits(bool async)
        {
            Assert.StartsWith(
                "The LINQ expression",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Average_with_division_on_decimal_no_significant_digits(async)))
                .Message);
        }
    }
}
