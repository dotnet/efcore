// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class AsyncFromSqlSprocQuerySqlServerTest : AsyncFromSqlSprocQueryTestBase<NorthwindSprocQuerySqlServerFixture>
    {
        public AsyncFromSqlSprocQuerySqlServerTest(NorthwindSprocQuerySqlServerFixture fixture)
            : base(fixture)
        {
        }

        protected override string TenMostExpensiveProductsSproc => "[dbo].[Ten Most Expensive Products]";

        protected override string CustomerOrderHistorySproc => "[dbo].[CustOrderHist] @CustomerID = {0}";
    }
}
