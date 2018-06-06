// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class WithNoLocalSqlServerSimpleTest : WithNoLockSimpleTest<NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
    {
        public WithNoLocalSqlServerSimpleTest(NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
        }

        public override void With_nolock_default()
        {
            base.With_nolock_default();

            AssertSql(@"SELECT [p].[CustomerID], [p].[Address], [p].[City], [p].[CompanyName], [p].[ContactName], [p].[ContactTitle], [p].[Country], [p].[Fax], [p].[Phone], [p].[PostalCode], [p].[Region]
FROM [Customers] AS [p] WITH (NOLOCK) 
WHERE ([p].[CustomerID] = N'ALFKI') AND ([p].[PostalCode] = N'12209')");
        }

        public override void With_nolock_parameter_false()
        {
            base.With_nolock_parameter_false();

            AssertSql(@"SELECT [p].[CustomerID], [p].[Address], [p].[City], [p].[CompanyName], [p].[ContactName], [p].[ContactTitle], [p].[Country], [p].[Fax], [p].[Phone], [p].[PostalCode], [p].[Region]
FROM [Customers] AS [p]
WHERE ([p].[CustomerID] = N'ALFKI') AND ([p].[PostalCode] = N'12209')");
        }

        public override void With_nolock_parameter_true()
        {
            base.With_nolock_parameter_true();

            AssertSql(@"SELECT [p].[CustomerID], [p].[Address], [p].[City], [p].[CompanyName], [p].[ContactName], [p].[ContactTitle], [p].[Country], [p].[Fax], [p].[Phone], [p].[PostalCode], [p].[Region]
FROM [Customers] AS [p] WITH (NOLOCK) 
WHERE ([p].[CustomerID] = N'ALFKI') AND ([p].[PostalCode] = N'12209')");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
