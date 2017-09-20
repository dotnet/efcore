// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class AsyncGroupByQuerySqlServerTest : AsyncGroupByQueryTestBase<NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
    {
        // ReSharper disable once UnusedParameter.Local
        public AsyncGroupByQuerySqlServerTest(NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task GroupBy_Composite_Select_Average()
        {
            await base.GroupBy_Composite_Select_Average();

            AssertSql(
                @"SELECT AVG(CAST([o].[OrderID] AS float))
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID], [o].[EmployeeID]");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
