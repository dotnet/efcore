// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace Microsoft.EntityFrameworkCore.Query;

public class BatchQuerySqlServerTest : BatchQueryTestBase<NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
{
    public BatchQuerySqlServerTest(NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
        => Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);

    public override async Task Batch_with_same_parameter_name_in_both_queries(bool async)
    {
        await base.Batch_with_same_parameter_name_in_both_queries(async);

        AssertSql(
            """
@b0_city='London' (Size = 15)
@b1_city='Seattle' (Size = 15)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @b0_city;

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[City] = @b1_city
""");
    }

    public override async Task Batch_with_execute_delete(bool async)
    {
        await base.Batch_with_execute_delete(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London';

SET NOCOUNT OFF;
DELETE FROM [o]
FROM [Order Details] AS [o]
WHERE [o].[OrderID] = 10248;
SELECT @@ROWCOUNT;

SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE [o].[OrderID] = 10248
""");
    }

    protected override DbParameter CreateDbParameter(string name, object value)
        => new SqlParameter { ParameterName = name, Value = value };

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
