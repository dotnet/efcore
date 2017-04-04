// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class FromSqlQuerySqlServerTest : FromSqlQueryTestBase<NorthwindQuerySqlServerFixture>
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public override void From_sql_queryable_simple()
        {
            base.From_sql_queryable_simple();

            AssertSql(
                @"SELECT * FROM ""Customers"" WHERE ""ContactName"" LIKE '%z%'",
                Sql);
        }

        public override void From_sql_queryable_simple_columns_out_of_order()
        {
            base.From_sql_queryable_simple_columns_out_of_order();

            AssertSql(
                @"SELECT ""Region"", ""PostalCode"", ""Phone"", ""Fax"", ""CustomerID"", ""Country"", ""ContactTitle"", ""ContactName"", ""CompanyName"", ""City"", ""Address"" FROM ""Customers""",
                Sql);
        }

        public override void From_sql_queryable_simple_columns_out_of_order_and_extra_columns()
        {
            base.From_sql_queryable_simple_columns_out_of_order_and_extra_columns();

            AssertSql(
                @"SELECT ""Region"", ""PostalCode"", ""PostalCode"" AS ""Foo"", ""Phone"", ""Fax"", ""CustomerID"", ""Country"", ""ContactTitle"", ""ContactName"", ""CompanyName"", ""City"", ""Address"" FROM ""Customers""",
                Sql);
        }

        public override void From_sql_queryable_composed()
        {
            base.From_sql_queryable_composed();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM (
    SELECT * FROM ""Customers""
) AS [c]
WHERE CHARINDEX(N'z', [c].[ContactName]) > 0",
                Sql);
        }

        public override void From_sql_queryable_composed_after_removing_whitespaces()
        {
            base.From_sql_queryable_composed_after_removing_whitespaces();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM (

        


    SELECT
    * FROM ""Customers""
) AS [c]
WHERE CHARINDEX(N'z', [c].[ContactName]) > 0",
                Sql);
        }

        public override void From_sql_queryable_composed_compiled()
        {
            base.From_sql_queryable_composed_compiled();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM (
    SELECT * FROM ""Customers""
) AS [c]
WHERE CHARINDEX(N'z', [c].[ContactName]) > 0",
                Sql);
        }

        public override void From_sql_composed_contains()
        {
            base.From_sql_composed_contains();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [o].[CustomerID]
    FROM (
        SELECT * FROM ""Orders""
    ) AS [o]
)",
                Sql);
        }

        public override void From_sql_composed_contains2()
        {
            base.From_sql_composed_contains2();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[CustomerID] = N'ALFKI') AND [c].[CustomerID] IN (
    SELECT [o].[CustomerID]
    FROM (
        SELECT * FROM ""Orders""
    ) AS [o]
)",
                Sql);
        }

        public override void From_sql_queryable_multiple_composed()
        {
            base.From_sql_queryable_multiple_composed();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT * FROM ""Customers""
) AS [c]
CROSS JOIN (
    SELECT * FROM ""Orders""
) AS [o]
WHERE [c].[CustomerID] = [o].[CustomerID]",
                Sql);
        }

        public override void From_sql_queryable_multiple_composed_with_closure_parameters()
        {
            base.From_sql_queryable_multiple_composed_with_closure_parameters();

            AssertSql(
                @"@__8__locals1_startDate_1: 01/01/1997 00:00:00
@__8__locals1_endDate_2: 01/01/1998 00:00:00

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT * FROM ""Customers""
) AS [c]
CROSS JOIN (
    SELECT * FROM ""Orders"" WHERE ""OrderDate"" BETWEEN @__8__locals1_startDate_1 AND @__8__locals1_endDate_2
) AS [o]
WHERE [c].[CustomerID] = [o].[CustomerID]",
                Sql);
        }

        public override void From_sql_queryable_multiple_composed_with_parameters_and_closure_parameters()
        {
            base.From_sql_queryable_multiple_composed_with_parameters_and_closure_parameters();

            AssertSql(
                @"@p0: London (Size = 4000)
@__8__locals1_startDate_1: 01/01/1997 00:00:00
@__8__locals1_endDate_2: 01/01/1998 00:00:00

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT * FROM ""Customers"" WHERE ""City"" = @p0
) AS [c]
CROSS JOIN (
    SELECT * FROM ""Orders"" WHERE ""OrderDate"" BETWEEN @__8__locals1_startDate_1 AND @__8__locals1_endDate_2
) AS [o]
WHERE [c].[CustomerID] = [o].[CustomerID]",
                Sql);
        }

        public override void From_sql_queryable_multiple_line_query()
        {
            base.From_sql_queryable_multiple_line_query();

            AssertSql(
                @"SELECT *
FROM ""Customers""
WHERE ""City"" = 'London'",
                Sql);
        }

        public override void From_sql_queryable_composed_multiple_line_query()
        {
            base.From_sql_queryable_composed_multiple_line_query();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM (
    SELECT *
    FROM ""Customers""
) AS [c]
WHERE [c].[City] = N'London'",
                Sql);
        }

        public override void From_sql_queryable_with_parameters()
        {
            base.From_sql_queryable_with_parameters();

            AssertSql(
                @"@p0: London (Size = 4000)
@p1: Sales Representative (Size = 4000)

SELECT * FROM ""Customers"" WHERE ""City"" = @p0 AND ""ContactTitle"" = @p1",
                Sql);
        }

        public override void From_sql_queryable_with_parameters_inline()
        {
            base.From_sql_queryable_with_parameters_inline();

            AssertSql(
                @"@p0: London (Size = 4000)
@p1: Sales Representative (Size = 4000)

SELECT * FROM ""Customers"" WHERE ""City"" = @p0 AND ""ContactTitle"" = @p1",
                Sql);
        }

        public override void From_sql_queryable_with_null_parameter()
        {
            base.From_sql_queryable_with_null_parameter();

            AssertSql(
                @"@p0:  (Nullable = false) (DbType = String)

SELECT * FROM ""Employees"" WHERE ""ReportsTo"" = @p0 OR (""ReportsTo"" IS NULL AND @p0 IS NULL)",
                Sql);
        }

        public override void From_sql_queryable_with_parameters_and_closure()
        {
            base.From_sql_queryable_with_parameters_and_closure();

            AssertSql(
                @"@p0: London (Size = 4000)
@__contactTitle_1: Sales Representative (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM (
    SELECT * FROM ""Customers"" WHERE ""City"" = @p0
) AS [c]
WHERE [c].[ContactTitle] = @__contactTitle_1",
                Sql);
        }

        public override void From_sql_queryable_simple_cache_key_includes_query_string()
        {
            base.From_sql_queryable_simple_cache_key_includes_query_string();

            AssertSql(
                @"SELECT * FROM ""Customers"" WHERE ""City"" = 'London'

SELECT * FROM ""Customers"" WHERE ""City"" = 'Seattle'",
                Sql);
        }

        public override void From_sql_queryable_with_parameters_cache_key_includes_parameters()
        {
            base.From_sql_queryable_with_parameters_cache_key_includes_parameters();

            AssertSql(
                @"@p0: London (Size = 4000)
@p1: Sales Representative (Size = 4000)

SELECT * FROM ""Customers"" WHERE ""City"" = @p0 AND ""ContactTitle"" = @p1

@p0: Madrid (Size = 4000)
@p1: Accounting Manager (Size = 4000)

SELECT * FROM ""Customers"" WHERE ""City"" = @p0 AND ""ContactTitle"" = @p1",
                Sql);
        }

        public override void From_sql_queryable_simple_as_no_tracking_not_composed()
        {
            base.From_sql_queryable_simple_as_no_tracking_not_composed();

            AssertSql(
                @"SELECT * FROM ""Customers""",
                Sql);
        }

        public override void From_sql_queryable_simple_projection_composed()
        {
            base.From_sql_queryable_simple_projection_composed();

            AssertSql(
                @"SELECT [p].[ProductName]
FROM (
    SELECT *
    FROM Products
    WHERE Discontinued <> 1
    AND ((UnitsInStock + UnitsOnOrder) < ReorderLevel)
) AS [p]",
                Sql);
        }

        public override void From_sql_queryable_simple_include()
        {
            base.From_sql_queryable_simple_include();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM (
    SELECT * FROM ""Customers""
) AS [c]
ORDER BY [c].[CustomerID]

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT [c0].[CustomerID]
    FROM (
        SELECT * FROM ""Customers""
    ) AS [c0]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID]",
                Sql);
        }

        public override void From_sql_queryable_simple_composed_include()
        {
            base.From_sql_queryable_simple_composed_include();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM (
    SELECT * FROM ""Customers""
) AS [c]
WHERE [c].[City] = N'London'
ORDER BY [c].[CustomerID]

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT [c0].[CustomerID]
    FROM (
        SELECT * FROM ""Customers""
    ) AS [c0]
    WHERE [c0].[City] = N'London'
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID]",
                Sql);
        }

        public override void From_sql_annotations_do_not_affect_successive_calls()
        {
            base.From_sql_annotations_do_not_affect_successive_calls();

            AssertSql(
                @"SELECT * FROM ""Customers"" WHERE ""ContactName"" LIKE '%z%'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void From_sql_composed_with_nullable_predicate()
        {
            base.From_sql_composed_with_nullable_predicate();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM (
    SELECT * FROM ""Customers""
) AS [c]
WHERE ([c].[ContactName] = [c].[CompanyName]) OR ([c].[ContactName] IS NULL AND [c].[CompanyName] IS NULL)",
                Sql);
        }

        public override void From_sql_with_dbParameter()
        {
            base.From_sql_with_dbParameter();

            AssertSql(
                @"@city: London (Nullable = false) (Size = 6)

SELECT * FROM ""Customers"" WHERE ""City"" = @city",
                Sql);
        }

        public override void From_sql_with_dbParameter_mixed()
        {
            base.From_sql_with_dbParameter_mixed();

            AssertSql(
                @"@p0: London (Size = 4000)
@title: Sales Representative (Nullable = false) (Size = 20)

SELECT * FROM ""Customers"" WHERE ""City"" = @p0 AND ""ContactTitle"" = @title

@city: London (Nullable = false) (Size = 6)
@p1: Sales Representative (Size = 4000)

SELECT * FROM ""Customers"" WHERE ""City"" = @city AND ""ContactTitle"" = @p1",
                Sql);
        }

        public override void From_sql_with_db_parameters_called_multiple_times()
        {
            base.From_sql_with_db_parameters_called_multiple_times();

            AssertSql(
                @"@id: ALFKI (Nullable = false) (Size = 5)

SELECT * FROM ""Customers"" WHERE ""CustomerID"" = @id

@id: ALFKI (Nullable = false) (Size = 5)

SELECT * FROM ""Customers"" WHERE ""CustomerID"" = @id",
                Sql);
        }

        public override void From_sql_with_SelectMany_and_include()
        {
            base.From_sql_with_SelectMany_and_include();

            AssertSql(
                @"SELECT [c1].[CustomerID], [c1].[Address], [c1].[City], [c1].[CompanyName], [c1].[ContactName], [c1].[ContactTitle], [c1].[Country], [c1].[Fax], [c1].[Phone], [c1].[PostalCode], [c1].[Region], [c2].[CustomerID], [c2].[Address], [c2].[City], [c2].[CompanyName], [c2].[ContactName], [c2].[ContactTitle], [c2].[Country], [c2].[Fax], [c2].[Phone], [c2].[PostalCode], [c2].[Region]
FROM (
    SELECT * FROM ""Customers"" WHERE ""CustomerID"" = 'ALFKI'
) AS [c1]
CROSS JOIN (
    SELECT * FROM ""Customers"" WHERE ""CustomerID"" = 'AROUT'
) AS [c2]
ORDER BY [c2].[CustomerID]

SELECT [c2.Orders].[OrderID], [c2.Orders].[CustomerID], [c2.Orders].[EmployeeID], [c2.Orders].[OrderDate]
FROM [Orders] AS [c2.Orders]
INNER JOIN (
    SELECT DISTINCT [c20].[CustomerID]
    FROM (
        SELECT * FROM ""Customers"" WHERE ""CustomerID"" = 'ALFKI'
    ) AS [c10]
    CROSS JOIN (
        SELECT * FROM ""Customers"" WHERE ""CustomerID"" = 'AROUT'
    ) AS [c20]
) AS [t] ON [c2.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID]",
                Sql);

        }

        public override void From_sql_with_join_and_include()
        {
            base.From_sql_with_join_and_include();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT * FROM ""Customers"" WHERE ""CustomerID"" = 'ALFKI'
) AS [c]
INNER JOIN (
    SELECT * FROM ""Orders"" WHERE ""OrderID"" <> 1
) AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [o].[OrderID]

SELECT [o.OrderDetails].[OrderID], [o.OrderDetails].[ProductID], [o.OrderDetails].[Discount], [o.OrderDetails].[Quantity], [o.OrderDetails].[UnitPrice]
FROM [Order Details] AS [o.OrderDetails]
INNER JOIN (
    SELECT DISTINCT [o0].[OrderID]
    FROM (
        SELECT * FROM ""Customers"" WHERE ""CustomerID"" = 'ALFKI'
    ) AS [c0]
    INNER JOIN (
        SELECT * FROM ""Orders"" WHERE ""OrderID"" <> 1
    ) AS [o0] ON [c0].[CustomerID] = [o0].[CustomerID]
) AS [t] ON [o.OrderDetails].[OrderID] = [t].[OrderID]
ORDER BY [t].[OrderID]",
                Sql);
        }

        public FromSqlQuerySqlServerTest(NorthwindQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            _testOutputHelper = testOutputHelper;

            //TestSqlLoggerFactory.CaptureOutput(testOutputHelper);
        }

        protected override DbParameter CreateDbParameter(string name, object value)
            => new SqlParameter
            {
                ParameterName = name,
                Value = value
            };

        private const string FileLineEnding = @"
";

        private static string Sql => TestSqlLoggerFactory.Sql.Replace(Environment.NewLine, FileLineEnding);

        private void AssertSql(string expected, string actual)
        {
            TestHelpers.AssertBaseline(expected, actual, _testOutputHelper);
        }
    }
}
