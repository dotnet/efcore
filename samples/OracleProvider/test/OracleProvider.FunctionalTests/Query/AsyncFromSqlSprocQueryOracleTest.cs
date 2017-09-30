// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class AsyncFromSqlSprocQueryOracleTest : AsyncFromSqlSprocQueryTestBase<NorthwindQueryOracleFixture<NoopModelCustomizer>>
    {
        public AsyncFromSqlSprocQueryOracleTest(NorthwindQueryOracleFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
        }

        protected override string TenMostExpensiveProductsSproc
            => "BEGIN \"Ten Most Expensive Products\"(:cur); END;";

        protected override string CustomerOrderHistorySproc => "BEGIN \"CustOrderHist\"(:p0, :cur); END;";
    }
}
