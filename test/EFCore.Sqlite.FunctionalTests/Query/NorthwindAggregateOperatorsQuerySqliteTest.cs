// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
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

        public override Task Sum_with_division_on_decimal(bool async)
            => AssertTranslationFailed(() => base.Sum_with_division_on_decimal(async));

        public override Task Sum_with_division_on_decimal_no_significant_digits(bool async)
            => AssertTranslationFailed(() => base.Sum_with_division_on_decimal_no_significant_digits(async));

        public override Task Average_with_division_on_decimal(bool async)
            => AssertTranslationFailed(() => base.Average_with_division_on_decimal(async));

        public override Task Average_with_division_on_decimal_no_significant_digits(bool async)
            => AssertTranslationFailed(() => base.Average_with_division_on_decimal_no_significant_digits(async));
    }
}
