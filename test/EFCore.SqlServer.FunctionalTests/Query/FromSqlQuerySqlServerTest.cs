// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class FromSqlQuerySqlServerTest : FromSqlQueryTestBase<FromSqlQuerySqlServerTest.FromSqlQuerySqlServerTestFixture>
{
    public FromSqlQuerySqlServerTest(FromSqlQuerySqlServerTestFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task FromSqlRaw_queryable_simple(bool async)
    {
        await base.FromSqlRaw_queryable_simple(async);

        AssertSql(
            """
SELECT * FROM "Customers" WHERE "ContactName" LIKE '%z%'
""");
    }

    public override async Task FromSqlRaw_queryable_simple_columns_out_of_order(bool async)
    {
        await base.FromSqlRaw_queryable_simple_columns_out_of_order(async);

        AssertSql(
            """
SELECT "Region", "PostalCode", "Phone", "Fax", "CustomerID", "Country", "ContactTitle", "ContactName", "CompanyName", "City", "Address" FROM "Customers"
""");
    }

    public override async Task FromSqlRaw_queryable_simple_columns_out_of_order_and_extra_columns(bool async)
    {
        await base.FromSqlRaw_queryable_simple_columns_out_of_order_and_extra_columns(async);

        AssertSql(
            """
SELECT "Region", "PostalCode", "PostalCode" AS "Foo", "Phone", "Fax", "CustomerID", "Country", "ContactTitle", "ContactName", "CompanyName", "City", "Address" FROM "Customers"
""");
    }

    public override async Task FromSqlRaw_queryable_composed(bool async)
    {
        await base.FromSqlRaw_queryable_composed(async);

        AssertSql(
            """
SELECT [m].[CustomerID], [m].[Address], [m].[City], [m].[CompanyName], [m].[ContactName], [m].[ContactTitle], [m].[Country], [m].[Fax], [m].[Phone], [m].[PostalCode], [m].[Region]
FROM (
    SELECT * FROM "Customers"
) AS [m]
WHERE [m].[ContactName] LIKE N'%z%'
""");
    }

    public override async Task FromSqlRaw_queryable_composed_after_removing_whitespaces(bool async)
    {
        await base.FromSqlRaw_queryable_composed_after_removing_whitespaces(async);

        AssertSql(
            """
SELECT [m].[CustomerID], [m].[Address], [m].[City], [m].[CompanyName], [m].[ContactName], [m].[ContactTitle], [m].[Country], [m].[Fax], [m].[Phone], [m].[PostalCode], [m].[Region]
FROM (

        


    SELECT
    * FROM "Customers"
) AS [m]
WHERE [m].[ContactName] LIKE N'%z%'
""");
    }

    public override async Task FromSqlRaw_queryable_composed_compiled(bool async)
    {
        await base.FromSqlRaw_queryable_composed_compiled(async);

        AssertSql(
            """
SELECT [m].[CustomerID], [m].[Address], [m].[City], [m].[CompanyName], [m].[ContactName], [m].[ContactTitle], [m].[Country], [m].[Fax], [m].[Phone], [m].[PostalCode], [m].[Region]
FROM (
    SELECT * FROM "Customers"
) AS [m]
WHERE [m].[ContactName] LIKE N'%z%'
""");
    }

    public override async Task FromSqlRaw_queryable_composed_compiled_with_DbParameter(bool async)
    {
        await base.FromSqlRaw_queryable_composed_compiled_with_DbParameter(async);

        AssertSql(
            """
customer='CONSH' (Nullable = false) (Size = 5)

SELECT [m].[CustomerID], [m].[Address], [m].[City], [m].[CompanyName], [m].[ContactName], [m].[ContactTitle], [m].[Country], [m].[Fax], [m].[Phone], [m].[PostalCode], [m].[Region]
FROM (
    SELECT * FROM "Customers" WHERE "CustomerID" = @customer
) AS [m]
WHERE [m].[ContactName] LIKE N'%z%'
""");
    }

    public override async Task FromSqlRaw_queryable_composed_compiled_with_nameless_DbParameter(bool async)
    {
        await base.FromSqlRaw_queryable_composed_compiled_with_nameless_DbParameter(async);

        AssertSql(
            """
p0='CONSH' (Nullable = false) (Size = 5)

SELECT [m].[CustomerID], [m].[Address], [m].[City], [m].[CompanyName], [m].[ContactName], [m].[ContactTitle], [m].[Country], [m].[Fax], [m].[Phone], [m].[PostalCode], [m].[Region]
FROM (
    SELECT * FROM "Customers" WHERE "CustomerID" = @p0
) AS [m]
WHERE [m].[ContactName] LIKE N'%z%'
""");
    }

    public override async Task FromSqlRaw_queryable_composed_compiled_with_parameter(bool async)
    {
        await base.FromSqlRaw_queryable_composed_compiled_with_parameter(async);

        AssertSql(
            """
SELECT [m].[CustomerID], [m].[Address], [m].[City], [m].[CompanyName], [m].[ContactName], [m].[ContactTitle], [m].[Country], [m].[Fax], [m].[Phone], [m].[PostalCode], [m].[Region]
FROM (
    SELECT * FROM "Customers" WHERE "CustomerID" = N'CONSH'
) AS [m]
WHERE [m].[ContactName] LIKE N'%z%'
""");
    }

    public override async Task FromSqlRaw_composed_contains(bool async)
    {
        await base.FromSqlRaw_composed_contains(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [m].[CustomerID]
    FROM (
        SELECT * FROM "Orders"
    ) AS [m]
)
""");
    }

    public override async Task FromSqlRaw_composed_contains2(bool async)
    {
        await base.FromSqlRaw_composed_contains2(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI' AND [c].[CustomerID] IN (
    SELECT [m].[CustomerID]
    FROM (
        SELECT * FROM "Orders"
    ) AS [m]
)
""");
    }

    public override async Task FromSqlRaw_queryable_multiple_composed(bool async)
    {
        await base.FromSqlRaw_queryable_multiple_composed(async);

        AssertSql(
            """
SELECT [m].[CustomerID], [m].[Address], [m].[City], [m].[CompanyName], [m].[ContactName], [m].[ContactTitle], [m].[Country], [m].[Fax], [m].[Phone], [m].[PostalCode], [m].[Region], [m0].[OrderID], [m0].[CustomerID], [m0].[EmployeeID], [m0].[OrderDate]
FROM (
    SELECT * FROM "Customers"
) AS [m]
CROSS JOIN (
    SELECT * FROM "Orders"
) AS [m0]
WHERE [m].[CustomerID] = [m0].[CustomerID]
""");
    }

    public override async Task FromSqlRaw_queryable_multiple_composed_with_closure_parameters(bool async)
    {
        await base.FromSqlRaw_queryable_multiple_composed_with_closure_parameters(async);

        AssertSql(
            """
p0='1997-01-01T00:00:00.0000000'
p1='1998-01-01T00:00:00.0000000'

SELECT [m].[CustomerID], [m].[Address], [m].[City], [m].[CompanyName], [m].[ContactName], [m].[ContactTitle], [m].[Country], [m].[Fax], [m].[Phone], [m].[PostalCode], [m].[Region], [m0].[OrderID], [m0].[CustomerID], [m0].[EmployeeID], [m0].[OrderDate]
FROM (
    SELECT * FROM "Customers"
) AS [m]
CROSS JOIN (
    SELECT * FROM "Orders" WHERE "OrderDate" BETWEEN @p0 AND @p1
) AS [m0]
WHERE [m].[CustomerID] = [m0].[CustomerID]
""");
    }

    public override async Task FromSqlRaw_queryable_multiple_composed_with_parameters_and_closure_parameters(bool async)
    {
        await base.FromSqlRaw_queryable_multiple_composed_with_parameters_and_closure_parameters(async);

        AssertSql(
            """
p0='London' (Size = 4000)
p1='1997-01-01T00:00:00.0000000'
p2='1998-01-01T00:00:00.0000000'

SELECT [m].[CustomerID], [m].[Address], [m].[City], [m].[CompanyName], [m].[ContactName], [m].[ContactTitle], [m].[Country], [m].[Fax], [m].[Phone], [m].[PostalCode], [m].[Region], [m0].[OrderID], [m0].[CustomerID], [m0].[EmployeeID], [m0].[OrderDate]
FROM (
    SELECT * FROM "Customers" WHERE "City" = @p0
) AS [m]
CROSS JOIN (
    SELECT * FROM "Orders" WHERE "OrderDate" BETWEEN @p1 AND @p2
) AS [m0]
WHERE [m].[CustomerID] = [m0].[CustomerID]
""",
            //
            """
p0='Berlin' (Size = 4000)
p1='1998-04-01T00:00:00.0000000'
p2='1998-05-01T00:00:00.0000000'

SELECT [m].[CustomerID], [m].[Address], [m].[City], [m].[CompanyName], [m].[ContactName], [m].[ContactTitle], [m].[Country], [m].[Fax], [m].[Phone], [m].[PostalCode], [m].[Region], [m0].[OrderID], [m0].[CustomerID], [m0].[EmployeeID], [m0].[OrderDate]
FROM (
    SELECT * FROM "Customers" WHERE "City" = @p0
) AS [m]
CROSS JOIN (
    SELECT * FROM "Orders" WHERE "OrderDate" BETWEEN @p1 AND @p2
) AS [m0]
WHERE [m].[CustomerID] = [m0].[CustomerID]
""");
    }

    public override async Task FromSqlRaw_queryable_multiple_line_query(bool async)
    {
        await base.FromSqlRaw_queryable_multiple_line_query(async);

        AssertSql(
            """
SELECT *
FROM "Customers"
WHERE "City" = 'London'
""");
    }

    public override async Task FromSqlRaw_queryable_composed_multiple_line_query(bool async)
    {
        await base.FromSqlRaw_queryable_composed_multiple_line_query(async);

        AssertSql(
            """
SELECT [m].[CustomerID], [m].[Address], [m].[City], [m].[CompanyName], [m].[ContactName], [m].[ContactTitle], [m].[Country], [m].[Fax], [m].[Phone], [m].[PostalCode], [m].[Region]
FROM (
    SELECT *
    FROM "Customers"
) AS [m]
WHERE [m].[City] = N'London'
""");
    }

    public override async Task FromSqlRaw_queryable_with_parameters(bool async)
    {
        await base.FromSqlRaw_queryable_with_parameters(async);

        AssertSql(
            """
p0='London' (Size = 4000)
p1='Sales Representative' (Size = 4000)

SELECT * FROM "Customers" WHERE "City" = @p0 AND "ContactTitle" = @p1
""");
    }

    public override async Task FromSqlRaw_queryable_with_parameters_inline(bool async)
    {
        await base.FromSqlRaw_queryable_with_parameters_inline(async);

        AssertSql(
            """
p0='London' (Size = 4000)
p1='Sales Representative' (Size = 4000)

SELECT * FROM "Customers" WHERE "City" = @p0 AND "ContactTitle" = @p1
""");
    }

    public override async Task FromSqlInterpolated_queryable_with_parameters_interpolated(bool async)
    {
        await base.FromSqlInterpolated_queryable_with_parameters_interpolated(async);

        AssertSql(
            """
p0='London' (Size = 4000)
p1='Sales Representative' (Size = 4000)

SELECT * FROM "Customers" WHERE "City" = @p0 AND "ContactTitle" = @p1
""");
    }

    public override async Task FromSql_queryable_with_parameters_interpolated(bool async)
    {
        await base.FromSql_queryable_with_parameters_interpolated(async);

        AssertSql(
            """
p0='London' (Size = 4000)
p1='Sales Representative' (Size = 4000)

SELECT * FROM "Customers" WHERE "City" = @p0 AND "ContactTitle" = @p1
""");
    }

    public override async Task FromSqlInterpolated_queryable_with_parameters_inline_interpolated(bool async)
    {
        await base.FromSqlInterpolated_queryable_with_parameters_inline_interpolated(async);

        AssertSql(
            """
p0='London' (Size = 4000)
p1='Sales Representative' (Size = 4000)

SELECT * FROM "Customers" WHERE "City" = @p0 AND "ContactTitle" = @p1
""");
    }

    public override async Task FromSql_queryable_with_parameters_inline_interpolated(bool async)
    {
        await base.FromSql_queryable_with_parameters_inline_interpolated(async);

        AssertSql(
            """
p0='London' (Size = 4000)
p1='Sales Representative' (Size = 4000)

SELECT * FROM "Customers" WHERE "City" = @p0 AND "ContactTitle" = @p1
""");
    }

    public override async Task FromSqlInterpolated_queryable_multiple_composed_with_parameters_and_closure_parameters_interpolated(
        bool async)
    {
        await base.FromSqlInterpolated_queryable_multiple_composed_with_parameters_and_closure_parameters_interpolated(async);

        AssertSql(
            """
p0='London' (Size = 4000)
p1='1997-01-01T00:00:00.0000000'
p2='1998-01-01T00:00:00.0000000'

SELECT [m].[CustomerID], [m].[Address], [m].[City], [m].[CompanyName], [m].[ContactName], [m].[ContactTitle], [m].[Country], [m].[Fax], [m].[Phone], [m].[PostalCode], [m].[Region], [m0].[OrderID], [m0].[CustomerID], [m0].[EmployeeID], [m0].[OrderDate]
FROM (
    SELECT * FROM "Customers" WHERE "City" = @p0
) AS [m]
CROSS JOIN (
    SELECT * FROM "Orders" WHERE "OrderDate" BETWEEN @p1 AND @p2
) AS [m0]
WHERE [m].[CustomerID] = [m0].[CustomerID]
""",
            //
            """
p0='Berlin' (Size = 4000)
p1='1998-04-01T00:00:00.0000000'
p2='1998-05-01T00:00:00.0000000'

SELECT [m].[CustomerID], [m].[Address], [m].[City], [m].[CompanyName], [m].[ContactName], [m].[ContactTitle], [m].[Country], [m].[Fax], [m].[Phone], [m].[PostalCode], [m].[Region], [m0].[OrderID], [m0].[CustomerID], [m0].[EmployeeID], [m0].[OrderDate]
FROM (
    SELECT * FROM "Customers" WHERE "City" = @p0
) AS [m]
CROSS JOIN (
    SELECT * FROM "Orders" WHERE "OrderDate" BETWEEN @p1 AND @p2
) AS [m0]
WHERE [m].[CustomerID] = [m0].[CustomerID]
""");
    }

    public override async Task FromSql_queryable_multiple_composed_with_parameters_and_closure_parameters_interpolated(
        bool async)
    {
        await base.FromSql_queryable_multiple_composed_with_parameters_and_closure_parameters_interpolated(async);

        AssertSql(
            """
p0='London' (Size = 4000)
p1='1997-01-01T00:00:00.0000000'
p2='1998-01-01T00:00:00.0000000'

SELECT [m].[CustomerID], [m].[Address], [m].[City], [m].[CompanyName], [m].[ContactName], [m].[ContactTitle], [m].[Country], [m].[Fax], [m].[Phone], [m].[PostalCode], [m].[Region], [m0].[OrderID], [m0].[CustomerID], [m0].[EmployeeID], [m0].[OrderDate]
FROM (
    SELECT * FROM "Customers" WHERE "City" = @p0
) AS [m]
CROSS JOIN (
    SELECT * FROM "Orders" WHERE "OrderDate" BETWEEN @p1 AND @p2
) AS [m0]
WHERE [m].[CustomerID] = [m0].[CustomerID]
""",
            //
            """
p0='Berlin' (Size = 4000)
p1='1998-04-01T00:00:00.0000000'
p2='1998-05-01T00:00:00.0000000'

SELECT [m].[CustomerID], [m].[Address], [m].[City], [m].[CompanyName], [m].[ContactName], [m].[ContactTitle], [m].[Country], [m].[Fax], [m].[Phone], [m].[PostalCode], [m].[Region], [m0].[OrderID], [m0].[CustomerID], [m0].[EmployeeID], [m0].[OrderDate]
FROM (
    SELECT * FROM "Customers" WHERE "City" = @p0
) AS [m]
CROSS JOIN (
    SELECT * FROM "Orders" WHERE "OrderDate" BETWEEN @p1 AND @p2
) AS [m0]
WHERE [m].[CustomerID] = [m0].[CustomerID]
""");
    }

    public override async Task FromSqlRaw_queryable_with_null_parameter(bool async)
    {
        await base.FromSqlRaw_queryable_with_null_parameter(async);

        AssertSql(
            """
p0=NULL (Nullable = false)

SELECT * FROM "Employees" WHERE "ReportsTo" = @p0 OR ("ReportsTo" IS NULL AND @p0 IS NULL)
""");
    }

    public override async Task<string> FromSqlRaw_queryable_with_parameters_and_closure(bool async)
    {
        await base.FromSqlRaw_queryable_with_parameters_and_closure(async);

        AssertSql(
            """
p0='London' (Size = 4000)
@__contactTitle_1='Sales Representative' (Size = 30)

SELECT [m].[CustomerID], [m].[Address], [m].[City], [m].[CompanyName], [m].[ContactName], [m].[ContactTitle], [m].[Country], [m].[Fax], [m].[Phone], [m].[PostalCode], [m].[Region]
FROM (
    SELECT * FROM "Customers" WHERE "City" = @p0
) AS [m]
WHERE [m].[ContactTitle] = @__contactTitle_1
""");

        return null;
    }

    public override async Task FromSqlRaw_queryable_simple_cache_key_includes_query_string(bool async)
    {
        await base.FromSqlRaw_queryable_simple_cache_key_includes_query_string(async);

        AssertSql(
            """
SELECT * FROM "Customers" WHERE "City" = 'London'
""",
            //
            """
SELECT * FROM "Customers" WHERE "City" = 'Seattle'
""");
    }

    public override async Task FromSqlRaw_queryable_with_parameters_cache_key_includes_parameters(bool async)
    {
        await base.FromSqlRaw_queryable_with_parameters_cache_key_includes_parameters(async);

        AssertSql(
            """
p0='London' (Size = 4000)
p1='Sales Representative' (Size = 4000)

SELECT * FROM "Customers" WHERE "City" = @p0 AND "ContactTitle" = @p1
""",
            //
            """
p0='Madrid' (Size = 4000)
p1='Accounting Manager' (Size = 4000)

SELECT * FROM "Customers" WHERE "City" = @p0 AND "ContactTitle" = @p1
""");
    }

    public override async Task FromSqlRaw_queryable_simple_as_no_tracking_not_composed(bool async)
    {
        await base.FromSqlRaw_queryable_simple_as_no_tracking_not_composed(async);

        AssertSql(
            """
SELECT * FROM "Customers"
""");
    }

    public override async Task FromSqlRaw_queryable_simple_projection_composed(bool async)
    {
        await base.FromSqlRaw_queryable_simple_projection_composed(async);

        AssertSql(
            """
SELECT [m].[ProductName]
FROM (
    SELECT *
    FROM "Products"
    WHERE "Discontinued" <> CAST(1 AS bit)
    AND (("UnitsInStock" + "UnitsOnOrder") < "ReorderLevel")
) AS [m]
""");
    }

    public override async Task FromSqlRaw_queryable_simple_include(bool async)
    {
        await base.FromSqlRaw_queryable_simple_include(async);

        AssertSql(
            """
SELECT [m].[CustomerID], [m].[Address], [m].[City], [m].[CompanyName], [m].[ContactName], [m].[ContactTitle], [m].[Country], [m].[Fax], [m].[Phone], [m].[PostalCode], [m].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT * FROM "Customers"
) AS [m]
LEFT JOIN [Orders] AS [o] ON [m].[CustomerID] = [o].[CustomerID]
ORDER BY [m].[CustomerID]
""");
    }

    public override async Task FromSqlRaw_queryable_simple_composed_include(bool async)
    {
        await base.FromSqlRaw_queryable_simple_composed_include(async);

        AssertSql(
            """
SELECT [m].[CustomerID], [m].[Address], [m].[City], [m].[CompanyName], [m].[ContactName], [m].[ContactTitle], [m].[Country], [m].[Fax], [m].[Phone], [m].[PostalCode], [m].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT * FROM "Customers"
) AS [m]
LEFT JOIN [Orders] AS [o] ON [m].[CustomerID] = [o].[CustomerID]
WHERE [m].[City] = N'London'
ORDER BY [m].[CustomerID]
""");
    }

    public override async Task FromSqlRaw_annotations_do_not_affect_successive_calls(bool async)
    {
        await base.FromSqlRaw_annotations_do_not_affect_successive_calls(async);

        AssertSql(
            """
SELECT * FROM "Customers" WHERE "ContactName" LIKE '%z%'
""",
            //
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""");
    }

    public override async Task FromSqlRaw_composed_with_nullable_predicate(bool async)
    {
        await base.FromSqlRaw_composed_with_nullable_predicate(async);

        AssertSql(
            """
SELECT [m].[CustomerID], [m].[Address], [m].[City], [m].[CompanyName], [m].[ContactName], [m].[ContactTitle], [m].[Country], [m].[Fax], [m].[Phone], [m].[PostalCode], [m].[Region]
FROM (
    SELECT * FROM "Customers"
) AS [m]
WHERE [m].[ContactName] = [m].[CompanyName]
""");
    }

    public override async Task FromSqlRaw_with_dbParameter(bool async)
    {
        await base.FromSqlRaw_with_dbParameter(async);

        AssertSql(
            """
@city='London' (Nullable = false) (Size = 6)

SELECT * FROM "Customers" WHERE "City" = @city
""");
    }

    public override async Task FromSqlRaw_with_dbParameter_without_name_prefix(bool async)
    {
        await base.FromSqlRaw_with_dbParameter_without_name_prefix(async);
        AssertSql(
            """
city='London' (Nullable = false) (Size = 6)

SELECT * FROM "Customers" WHERE "City" = @city
""");
    }

    public override async Task FromSqlRaw_with_dbParameter_mixed(bool async)
    {
        await base.FromSqlRaw_with_dbParameter_mixed(async);

        AssertSql(
            """
p0='London' (Size = 4000)
@title='Sales Representative' (Nullable = false) (Size = 20)

SELECT * FROM "Customers" WHERE "City" = @p0 AND "ContactTitle" = @title
""",
            //
            """
@city='London' (Nullable = false) (Size = 6)
p1='Sales Representative' (Size = 4000)

SELECT * FROM "Customers" WHERE "City" = @city AND "ContactTitle" = @p1
""");
    }

    public override async Task FromSqlRaw_with_db_parameters_called_multiple_times(bool async)
    {
        await base.FromSqlRaw_with_db_parameters_called_multiple_times(async);

        AssertSql(
            """
@id='ALFKI' (Nullable = false) (Size = 5)

SELECT * FROM "Customers" WHERE "CustomerID" = @id
""",
            //
            """
@id='ALFKI' (Nullable = false) (Size = 5)

SELECT * FROM "Customers" WHERE "CustomerID" = @id
""");
    }

    public override async Task FromSqlRaw_with_SelectMany_and_include(bool async)
    {
        await base.FromSqlRaw_with_SelectMany_and_include(async);

        AssertSql(
            """
SELECT [m].[CustomerID], [m].[Address], [m].[City], [m].[CompanyName], [m].[ContactName], [m].[ContactTitle], [m].[Country], [m].[Fax], [m].[Phone], [m].[PostalCode], [m].[Region], [m0].[CustomerID], [m0].[Address], [m0].[City], [m0].[CompanyName], [m0].[ContactName], [m0].[ContactTitle], [m0].[Country], [m0].[Fax], [m0].[Phone], [m0].[PostalCode], [m0].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT * FROM "Customers" WHERE "CustomerID" = 'ALFKI'
) AS [m]
CROSS JOIN (
    SELECT * FROM "Customers" WHERE "CustomerID" = 'AROUT'
) AS [m0]
LEFT JOIN [Orders] AS [o] ON [m0].[CustomerID] = [o].[CustomerID]
ORDER BY [m].[CustomerID], [m0].[CustomerID]
""");
    }

    public override async Task FromSqlRaw_with_join_and_include(bool async)
    {
        await base.FromSqlRaw_with_join_and_include(async);

        AssertSql(
            """
SELECT [m].[CustomerID], [m].[Address], [m].[City], [m].[CompanyName], [m].[ContactName], [m].[ContactTitle], [m].[Country], [m].[Fax], [m].[Phone], [m].[PostalCode], [m].[Region], [m0].[OrderID], [m0].[CustomerID], [m0].[EmployeeID], [m0].[OrderDate], [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM (
    SELECT * FROM "Customers" WHERE "CustomerID" = 'ALFKI'
) AS [m]
INNER JOIN (
    SELECT * FROM "Orders" WHERE "OrderID" <> 1
) AS [m0] ON [m].[CustomerID] = [m0].[CustomerID]
LEFT JOIN [Order Details] AS [o] ON [m0].[OrderID] = [o].[OrderID]
ORDER BY [m].[CustomerID], [m0].[OrderID], [o].[OrderID]
""");
    }

    public override async Task FromSqlInterpolated_with_inlined_db_parameter(bool async)
    {
        await base.FromSqlInterpolated_with_inlined_db_parameter(async);

        AssertSql(
            """
@somename='ALFKI' (Nullable = false) (Size = 5)

SELECT * FROM "Customers" WHERE "CustomerID" = @somename
""");
    }

    public override async Task FromSql_with_inlined_db_parameter(bool async)
    {
        await base.FromSql_with_inlined_db_parameter(async);

        AssertSql(
            """
@somename='ALFKI' (Nullable = false) (Size = 5)

SELECT * FROM "Customers" WHERE "CustomerID" = @somename
""");
    }

    public override async Task FromSqlInterpolated_with_inlined_db_parameter_without_name_prefix(bool async)
    {
        await base.FromSqlInterpolated_with_inlined_db_parameter_without_name_prefix(async);

        AssertSql(
            """
somename='ALFKI' (Nullable = false) (Size = 5)

SELECT * FROM "Customers" WHERE "CustomerID" = @somename
""");
    }

    public override async Task FromSql_with_inlined_db_parameter_without_name_prefix(bool async)
    {
        await base.FromSql_with_inlined_db_parameter_without_name_prefix(async);

        AssertSql(
            """
somename='ALFKI' (Nullable = false) (Size = 5)

SELECT * FROM "Customers" WHERE "CustomerID" = @somename
""");
    }

    public override async Task FromSqlInterpolated_parameterization_issue_12213(bool async)
    {
        await base.FromSqlInterpolated_parameterization_issue_12213(async);

        AssertSql(
            """
p0='10300'

SELECT [m].[OrderID]
FROM (
    SELECT * FROM "Orders" WHERE "OrderID" >= @p0
) AS [m]
""",
            //
            """
@__max_0='10400'
p0='10300'

SELECT [o].[OrderID]
FROM [Orders] AS [o]
WHERE [o].[OrderID] <= @__max_0 AND [o].[OrderID] IN (
    SELECT [m].[OrderID]
    FROM (
        SELECT * FROM "Orders" WHERE "OrderID" >= @p0
    ) AS [m]
)
""",
            //
            """
@__max_0='10400'
p0='10300'

SELECT [o].[OrderID]
FROM [Orders] AS [o]
WHERE [o].[OrderID] <= @__max_0 AND [o].[OrderID] IN (
    SELECT [m].[OrderID]
    FROM (
        SELECT * FROM "Orders" WHERE "OrderID" >= @p0
    ) AS [m]
)
""");
    }

    public override async Task FromSqlRaw_does_not_parameterize_interpolated_string(bool async)
    {
        await base.FromSqlRaw_does_not_parameterize_interpolated_string(async);

        AssertSql(
            """
p0='10250'

SELECT * FROM "Orders" WHERE "OrderID" < @p0
""");
    }

    public override async Task Entity_equality_through_fromsql(bool async)
    {
        await base.Entity_equality_through_fromsql(async);

        AssertSql(
            """
SELECT [m].[OrderID], [m].[CustomerID], [m].[EmployeeID], [m].[OrderDate]
FROM (
    SELECT * FROM "Orders"
) AS [m]
LEFT JOIN [Customers] AS [c] ON [m].[CustomerID] = [c].[CustomerID]
WHERE [c].[CustomerID] = N'VINET'
""");
    }

    public override async Task FromSqlRaw_with_set_operation(bool async)
    {
        await base.FromSqlRaw_with_set_operation(async);

        AssertSql(
            """
SELECT [m].[CustomerID], [m].[Address], [m].[City], [m].[CompanyName], [m].[ContactName], [m].[ContactTitle], [m].[Country], [m].[Fax], [m].[Phone], [m].[PostalCode], [m].[Region]
FROM (
    SELECT * FROM "Customers" WHERE "City" = 'London'
) AS [m]
UNION ALL
SELECT [m0].[CustomerID], [m0].[Address], [m0].[City], [m0].[CompanyName], [m0].[ContactName], [m0].[ContactTitle], [m0].[Country], [m0].[Fax], [m0].[Phone], [m0].[PostalCode], [m0].[Region]
FROM (
    SELECT * FROM "Customers" WHERE "City" = 'Berlin'
) AS [m0]
""");
    }

    public override async Task Line_endings_after_Select(bool async)
    {
        await base.Line_endings_after_Select(async);

        AssertSql(
            """
SELECT [m].[CustomerID], [m].[Address], [m].[City], [m].[CompanyName], [m].[ContactName], [m].[ContactTitle], [m].[Country], [m].[Fax], [m].[Phone], [m].[PostalCode], [m].[Region]
FROM (
    SELECT
    * FROM "Customers"
) AS [m]
WHERE [m].[City] = N'Seattle'
""");
    }

    public override async Task FromSql_with_db_parameter_in_split_query(bool async)
    {
        await base.FromSql_with_db_parameter_in_split_query(async);

        AssertSql(
            """
customerID='ALFKI' (Nullable = false) (Size = 5)

SELECT [m].[CustomerID], [m].[Address], [m].[City], [m].[CompanyName], [m].[ContactName], [m].[ContactTitle], [m].[Country], [m].[Fax], [m].[Phone], [m].[PostalCode], [m].[Region]
FROM (
    SELECT * FROM "Customers" WHERE "CustomerID" = @customerID
) AS [m]
ORDER BY [m].[CustomerID]
""",
            //
            """
customerID='ALFKI' (Nullable = false) (Size = 5)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [m].[CustomerID]
FROM (
    SELECT * FROM "Customers" WHERE "CustomerID" = @customerID
) AS [m]
INNER JOIN [Orders] AS [o] ON [m].[CustomerID] = [o].[CustomerID]
ORDER BY [m].[CustomerID], [o].[OrderID]
""",
            //
            """
customerID='ALFKI' (Nullable = false) (Size = 5)

SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice], [m].[CustomerID], [o].[OrderID]
FROM (
    SELECT * FROM "Customers" WHERE "CustomerID" = @customerID
) AS [m]
INNER JOIN [Orders] AS [o] ON [m].[CustomerID] = [o].[CustomerID]
INNER JOIN [Order Details] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
ORDER BY [m].[CustomerID], [o].[OrderID]
""");
    }

    public override async Task FromSqlRaw_in_subquery_with_dbParameter(bool async)
    {
        await base.FromSqlRaw_in_subquery_with_dbParameter(async);

        AssertSql(
            """
@city='London' (Nullable = false) (Size = 6)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] IN (
    SELECT [m].[CustomerID]
    FROM (
        SELECT * FROM "Customers" WHERE "City" = @city
    ) AS [m]
)
""");
    }

    public override async Task FromSqlRaw_in_subquery_with_positional_dbParameter_without_name(bool async)
    {
        await base.FromSqlRaw_in_subquery_with_positional_dbParameter_without_name(async);

        AssertSql(
            """
p0='London' (Nullable = false) (Size = 6)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] IN (
    SELECT [m].[CustomerID]
    FROM (
        SELECT * FROM "Customers" WHERE "City" = @p0
    ) AS [m]
)
""");
    }

    public override async Task FromSqlRaw_in_subquery_with_positional_dbParameter_with_name(bool async)
    {
        await base.FromSqlRaw_in_subquery_with_positional_dbParameter_with_name(async);

        AssertSql(
            """
@city='London' (Nullable = false) (Size = 6)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] IN (
    SELECT [m].[CustomerID]
    FROM (
        SELECT * FROM "Customers" WHERE "City" = @city
    ) AS [m]
)
""");
    }

    public override async Task FromSqlRaw_with_dbParameter_mixed_in_subquery(bool async)
    {
        await base.FromSqlRaw_with_dbParameter_mixed_in_subquery(async);

        AssertSql(
            """
p0='London' (Size = 4000)
@title='Sales Representative' (Nullable = false) (Size = 20)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] IN (
    SELECT [m].[CustomerID]
    FROM (
        SELECT * FROM "Customers" WHERE "City" = @p0 AND "ContactTitle" = @title
    ) AS [m]
)
""",
            //
            """
@city='London' (Nullable = false) (Size = 6)
p1='Sales Representative' (Size = 4000)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] IN (
    SELECT [m].[CustomerID]
    FROM (
        SELECT * FROM "Customers" WHERE "City" = @city AND "ContactTitle" = @p1
    ) AS [m]
)
""");
    }

    public override async Task Multiple_occurrences_of_FromSql_with_db_parameter_adds_parameter_only_once(bool async)
    {
        await base.Multiple_occurrences_of_FromSql_with_db_parameter_adds_parameter_only_once(async);

        AssertSql(
            """
city='Seattle' (Nullable = false) (Size = 7)

SELECT [m].[CustomerID], [m].[Address], [m].[City], [m].[CompanyName], [m].[ContactName], [m].[ContactTitle], [m].[Country], [m].[Fax], [m].[Phone], [m].[PostalCode], [m].[Region]
FROM (
    SELECT * FROM "Customers" WHERE "City" = @city
) AS [m]
INTERSECT
SELECT [m0].[CustomerID], [m0].[Address], [m0].[City], [m0].[CompanyName], [m0].[ContactName], [m0].[ContactTitle], [m0].[Country], [m0].[Fax], [m0].[Phone], [m0].[PostalCode], [m0].[Region]
FROM (
    SELECT * FROM "Customers" WHERE "City" = @city
) AS [m0]
""");
    }

    public override async Task FromSqlRaw_composed_with_common_table_expression(bool async)
    {
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(() => base.FromSqlRaw_composed_with_common_table_expression(async));

        Assert.Equal(RelationalStrings.FromSqlNonComposable, exception.Message);
    }

    [ConditionalFact]
    public virtual void FromSql_output_parameter_works_with_transient_errors()
    {
        using var context = Fixture.CreateContext();
        var connection = (TestSqlServerConnection)context.GetService<ISqlServerConnection>();
        connection.ExecutionFailures.Enqueue(new bool?[] { true });
        connection.OpenFailures.Enqueue(new bool?[] { true });

        var output =
            new SqlParameter
            {
                ParameterName = "returnValue",
                Value = -1,
                Direction = ParameterDirection.InputOutput,
                SqlDbType = SqlDbType.Int,
            };

        var orders = context.Set<OrderQuery>()
                .FromSqlRaw(@"SET @returnValue = 3
SELECT * FROM [Customers] WHERE [CustomerID] = 'ALFKI'", [output])
                .ToList();

        Assert.Equal(1, orders.Count);
        Assert.Equal(3, output.Value);
    }

    protected override DbParameter CreateDbParameter(string name, object value)
        => new SqlParameter { ParameterName = name, Value = value };

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class FromSqlQuerySqlServerTestFixture : NorthwindQuerySqlServerFixture<NoopModelCustomizer>
    {
        protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
            => base.AddServices(serviceCollection)
                .AddSingleton<IRelationalTransactionFactory, TestRelationalTransactionFactory>()
                .AddScoped<ISqlServerConnection, TestSqlServerConnection>()
                .AddSingleton<IRelationalCommandBuilderFactory, TestRelationalCommandBuilderFactory>();
    }
}
