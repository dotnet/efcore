// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.SqlClient;

namespace Microsoft.EntityFrameworkCore.Query;

public class SqlQuerySqlServerTest : SqlQueryTestBase<NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
{
    public SqlQuerySqlServerTest(NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task SqlQueryRaw_queryable_simple(bool async)
    {
        await base.SqlQueryRaw_queryable_simple(async);

        AssertSql(
            """
SELECT * FROM "Customers" WHERE "ContactName" LIKE '%z%'
""");
    }

    public override async Task SqlQueryRaw_queryable_simple_columns_out_of_order(bool async)
    {
        await base.SqlQueryRaw_queryable_simple_columns_out_of_order(async);

        AssertSql(
            """
SELECT "Region", "PostalCode", "Phone", "Fax", "CustomerID", "Country", "ContactTitle", "ContactName", "CompanyName", "City", "Address" FROM "Customers"
""");
    }

    public override async Task SqlQueryRaw_queryable_simple_columns_out_of_order_and_extra_columns(bool async)
    {
        await base.SqlQueryRaw_queryable_simple_columns_out_of_order_and_extra_columns(async);

        AssertSql(
            """
SELECT "Region", "PostalCode", "PostalCode" AS "Foo", "Phone", "Fax", "CustomerID", "Country", "ContactTitle", "ContactName", "CompanyName", "City", "Address" FROM "Customers"
""");
    }

    public override async Task SqlQueryRaw_queryable_composed(bool async)
    {
        await base.SqlQueryRaw_queryable_composed(async);

        AssertSql(
            """
SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[Region], [c].[PostalCode]
FROM (
    SELECT * FROM "Customers"
) AS [c]
WHERE [c].[ContactName] LIKE N'%z%'
""");
    }

    public override async Task SqlQueryRaw_queryable_composed_after_removing_whitespaces(bool async)
    {
        await base.SqlQueryRaw_queryable_composed_after_removing_whitespaces(async);

        AssertSql(
            """
SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[Region], [c].[PostalCode]
FROM (

        


    SELECT
    * FROM "Customers"
) AS [c]
WHERE [c].[ContactName] LIKE N'%z%'
""");
    }

    public override async Task SqlQueryRaw_queryable_composed_compiled(bool async)
    {
        await base.SqlQueryRaw_queryable_composed_compiled(async);

        AssertSql(
            """
SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[Region], [c].[PostalCode]
FROM (
    SELECT * FROM "Customers"
) AS [c]
WHERE [c].[ContactName] LIKE N'%z%'
""");
    }

    public override async Task SqlQueryRaw_queryable_composed_compiled_with_DbParameter(bool async)
    {
        await base.SqlQueryRaw_queryable_composed_compiled_with_DbParameter(async);

        AssertSql(
            """
customer='CONSH' (Nullable = false) (Size = 5)

SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[Region], [c].[PostalCode]
FROM (
    SELECT * FROM "Customers" WHERE "CustomerID" = @customer
) AS [c]
WHERE [c].[ContactName] LIKE N'%z%'
""");
    }

    public override async Task SqlQueryRaw_queryable_composed_compiled_with_nameless_DbParameter(bool async)
    {
        await base.SqlQueryRaw_queryable_composed_compiled_with_nameless_DbParameter(async);

        AssertSql(
            """
p0='CONSH' (Nullable = false) (Size = 5)

SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[Region], [c].[PostalCode]
FROM (
    SELECT * FROM "Customers" WHERE "CustomerID" = @p0
) AS [c]
WHERE [c].[ContactName] LIKE N'%z%'
""");
    }

    public override async Task SqlQueryRaw_queryable_composed_compiled_with_parameter(bool async)
    {
        await base.SqlQueryRaw_queryable_composed_compiled_with_parameter(async);

        AssertSql(
            """
SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[Region], [c].[PostalCode]
FROM (
    SELECT * FROM "Customers" WHERE "CustomerID" = N'CONSH'
) AS [c]
WHERE [c].[ContactName] LIKE N'%z%'
""");
    }

    public override async Task SqlQueryRaw_composed_contains(bool async)
    {
        await base.SqlQueryRaw_composed_contains(async);

        AssertSql(
            """
SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[Region], [c].[PostalCode]
FROM (
    SELECT * FROM "Customers"
) AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [o].[CustomerID]
    FROM (
        SELECT * FROM "Orders"
    ) AS [o]
)
""");
    }

    public override async Task SqlQueryRaw_queryable_multiple_composed(bool async)
    {
        await base.SqlQueryRaw_queryable_multiple_composed(async);

        AssertSql(
            """
SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[Region], [c].[PostalCode], [o].[CustomerID], [o].[EmployeeID], [o].[Freight], [o].[OrderDate], [o].[OrderID], [o].[RequiredDate], [o].[ShipAddress], [o].[ShipCity], [o].[ShipCountry], [o].[ShipName], [o].[ShipPostalCode], [o].[ShipRegion], [o].[ShipVia], [o].[ShippedDate]
FROM (
    SELECT * FROM "Customers"
) AS [c]
CROSS JOIN (
    SELECT * FROM "Orders"
) AS [o]
WHERE [c].[CustomerID] = [o].[CustomerID]
""");
    }

    public override async Task SqlQueryRaw_queryable_multiple_composed_with_closure_parameters(bool async)
    {
        await base.SqlQueryRaw_queryable_multiple_composed_with_closure_parameters(async);

        AssertSql(
            """
p0='1997-01-01T00:00:00.0000000'
p1='1998-01-01T00:00:00.0000000'

SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[Region], [c].[PostalCode], [o].[CustomerID], [o].[EmployeeID], [o].[Freight], [o].[OrderDate], [o].[OrderID], [o].[RequiredDate], [o].[ShipAddress], [o].[ShipCity], [o].[ShipCountry], [o].[ShipName], [o].[ShipPostalCode], [o].[ShipRegion], [o].[ShipVia], [o].[ShippedDate]
FROM (
    SELECT * FROM "Customers"
) AS [c]
CROSS JOIN (
    SELECT * FROM "Orders" WHERE "OrderDate" BETWEEN @p0 AND @p1
) AS [o]
WHERE [c].[CustomerID] = [o].[CustomerID]
""");
    }

    public override async Task SqlQueryRaw_queryable_multiple_composed_with_parameters_and_closure_parameters(bool async)
    {
        await base.SqlQueryRaw_queryable_multiple_composed_with_parameters_and_closure_parameters(async);

        AssertSql(
            """
p0='London' (Size = 4000)
p1='1997-01-01T00:00:00.0000000'
p2='1998-01-01T00:00:00.0000000'

SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[Region], [c].[PostalCode], [o].[CustomerID], [o].[EmployeeID], [o].[Freight], [o].[OrderDate], [o].[OrderID], [o].[RequiredDate], [o].[ShipAddress], [o].[ShipCity], [o].[ShipCountry], [o].[ShipName], [o].[ShipPostalCode], [o].[ShipRegion], [o].[ShipVia], [o].[ShippedDate]
FROM (
    SELECT * FROM "Customers" WHERE "City" = @p0
) AS [c]
CROSS JOIN (
    SELECT * FROM "Orders" WHERE "OrderDate" BETWEEN @p1 AND @p2
) AS [o]
WHERE [c].[CustomerID] = [o].[CustomerID]
""",
            //
            """
p0='Berlin' (Size = 4000)
p1='1998-04-01T00:00:00.0000000'
p2='1998-05-01T00:00:00.0000000'

SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[Region], [c].[PostalCode], [o].[CustomerID], [o].[EmployeeID], [o].[Freight], [o].[OrderDate], [o].[OrderID], [o].[RequiredDate], [o].[ShipAddress], [o].[ShipCity], [o].[ShipCountry], [o].[ShipName], [o].[ShipPostalCode], [o].[ShipRegion], [o].[ShipVia], [o].[ShippedDate]
FROM (
    SELECT * FROM "Customers" WHERE "City" = @p0
) AS [c]
CROSS JOIN (
    SELECT * FROM "Orders" WHERE "OrderDate" BETWEEN @p1 AND @p2
) AS [o]
WHERE [c].[CustomerID] = [o].[CustomerID]
""");
    }

    public override async Task SqlQueryRaw_queryable_multiple_line_query(bool async)
    {
        await base.SqlQueryRaw_queryable_multiple_line_query(async);

        AssertSql(
            """
SELECT *
FROM "Customers"
WHERE "City" = 'London'
""");
    }

    public override async Task SqlQueryRaw_queryable_composed_multiple_line_query(bool async)
    {
        await base.SqlQueryRaw_queryable_composed_multiple_line_query(async);

        AssertSql(
            """
SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[Region], [c].[PostalCode]
FROM (
    SELECT *
    FROM "Customers"
) AS [c]
WHERE [c].[City] = N'London'
""");
    }

    public override async Task SqlQueryRaw_queryable_with_parameters(bool async)
    {
        await base.SqlQueryRaw_queryable_with_parameters(async);

        AssertSql(
            """
p0='London' (Size = 4000)
p1='Sales Representative' (Size = 4000)

SELECT * FROM "Customers" WHERE "City" = @p0 AND "ContactTitle" = @p1
""");
    }

    public override async Task SqlQueryRaw_queryable_with_parameters_inline(bool async)
    {
        await base.SqlQueryRaw_queryable_with_parameters_inline(async);

        AssertSql(
            """
p0='London' (Size = 4000)
p1='Sales Representative' (Size = 4000)

SELECT * FROM "Customers" WHERE "City" = @p0 AND "ContactTitle" = @p1
""");
    }

    public override async Task SqlQuery_queryable_with_parameters_interpolated(bool async)
    {
        await base.SqlQuery_queryable_with_parameters_interpolated(async);

        AssertSql(
            """
p0='London' (Size = 4000)
p1='Sales Representative' (Size = 4000)

SELECT * FROM "Customers" WHERE "City" = @p0 AND "ContactTitle" = @p1
""");
    }

    public override async Task SqlQuery_queryable_with_parameters_inline_interpolated(bool async)
    {
        await base.SqlQuery_queryable_with_parameters_inline_interpolated(async);

        AssertSql(
            """
p0='London' (Size = 4000)
p1='Sales Representative' (Size = 4000)

SELECT * FROM "Customers" WHERE "City" = @p0 AND "ContactTitle" = @p1
""");
    }

    public override async Task SqlQuery_queryable_multiple_composed_with_parameters_and_closure_parameters_interpolated(
        bool async)
    {
        await base.SqlQuery_queryable_multiple_composed_with_parameters_and_closure_parameters_interpolated(async);

        AssertSql(
            """
p0='London' (Size = 4000)
p1='1997-01-01T00:00:00.0000000'
p2='1998-01-01T00:00:00.0000000'

SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[Region], [c].[PostalCode], [o].[CustomerID], [o].[EmployeeID], [o].[Freight], [o].[OrderDate], [o].[OrderID], [o].[RequiredDate], [o].[ShipAddress], [o].[ShipCity], [o].[ShipCountry], [o].[ShipName], [o].[ShipPostalCode], [o].[ShipRegion], [o].[ShipVia], [o].[ShippedDate]
FROM (
    SELECT * FROM "Customers" WHERE "City" = @p0
) AS [c]
CROSS JOIN (
    SELECT * FROM "Orders" WHERE "OrderDate" BETWEEN @p1 AND @p2
) AS [o]
WHERE [c].[CustomerID] = [o].[CustomerID]
""",
            //
            """
p0='Berlin' (Size = 4000)
p1='1998-04-01T00:00:00.0000000'
p2='1998-05-01T00:00:00.0000000'

SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[Region], [c].[PostalCode], [o].[CustomerID], [o].[EmployeeID], [o].[Freight], [o].[OrderDate], [o].[OrderID], [o].[RequiredDate], [o].[ShipAddress], [o].[ShipCity], [o].[ShipCountry], [o].[ShipName], [o].[ShipPostalCode], [o].[ShipRegion], [o].[ShipVia], [o].[ShippedDate]
FROM (
    SELECT * FROM "Customers" WHERE "City" = @p0
) AS [c]
CROSS JOIN (
    SELECT * FROM "Orders" WHERE "OrderDate" BETWEEN @p1 AND @p2
) AS [o]
WHERE [c].[CustomerID] = [o].[CustomerID]
""");
    }

    public override async Task SqlQueryRaw_queryable_with_null_parameter(bool async)
    {
        await base.SqlQueryRaw_queryable_with_null_parameter(async);

        AssertSql(
            """
p0=NULL (Nullable = false)

SELECT * FROM "Employees" WHERE "ReportsTo" = @p0 OR ("ReportsTo" IS NULL AND @p0 IS NULL)
""");
    }

    public override async Task<string> SqlQueryRaw_queryable_with_parameters_and_closure(bool async)
    {
        var queryString = await base.SqlQueryRaw_queryable_with_parameters_and_closure(async);

        AssertSql(
            """
p0='London' (Size = 4000)
@__contactTitle_1='Sales Representative' (Size = 30)

SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[Region], [c].[PostalCode]
FROM (
    SELECT * FROM "Customers" WHERE "City" = @p0
) AS [c]
WHERE [c].[ContactTitle] = @__contactTitle_1
""");

        return null;
    }

    public override async Task SqlQueryRaw_queryable_simple_cache_key_includes_query_string(bool async)
    {
        await base.SqlQueryRaw_queryable_simple_cache_key_includes_query_string(async);

        AssertSql(
            """
SELECT * FROM "Customers" WHERE "City" = 'London'
""",
            //
            """
SELECT * FROM "Customers" WHERE "City" = 'Seattle'
""");
    }

    public override async Task SqlQueryRaw_queryable_with_parameters_cache_key_includes_parameters(bool async)
    {
        await base.SqlQueryRaw_queryable_with_parameters_cache_key_includes_parameters(async);

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

    public override async Task SqlQueryRaw_queryable_simple_as_no_tracking_not_composed(bool async)
    {
        await base.SqlQueryRaw_queryable_simple_as_no_tracking_not_composed(async);

        AssertSql(
            """
SELECT * FROM "Customers"
""");
    }

    public override async Task SqlQueryRaw_queryable_simple_projection_composed(bool async)
    {
        await base.SqlQueryRaw_queryable_simple_projection_composed(async);

        AssertSql(
            """
SELECT [u].[ProductName]
FROM (
    SELECT *
    FROM "Products"
    WHERE "Discontinued" <> CAST(1 AS bit)
    AND (("UnitsInStock" + "UnitsOnOrder") < "ReorderLevel")
) AS [u]
""");
    }

    public override async Task SqlQueryRaw_annotations_do_not_affect_successive_calls(bool async)
    {
        await base.SqlQueryRaw_annotations_do_not_affect_successive_calls(async);

        AssertSql(
            """
SELECT * FROM "Customers" WHERE "ContactName" LIKE '%z%'
""",
            //
            """
SELECT * FROM "Customers"
""");
    }

    public override async Task SqlQueryRaw_composed_with_predicate(bool async)
    {
        await base.SqlQueryRaw_composed_with_predicate(async);

        AssertSql(
            """
SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[Region], [c].[PostalCode]
FROM (
    SELECT * FROM "Customers"
) AS [c]
WHERE SUBSTRING([c].[ContactName], 0 + 1, 1) = SUBSTRING([c].[CompanyName], 0 + 1, 1)
""");
    }

    public override async Task SqlQueryRaw_composed_with_empty_predicate(bool async)
    {
        await base.SqlQueryRaw_composed_with_empty_predicate(async);

        AssertSql(
            """
SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[Region], [c].[PostalCode]
FROM (
    SELECT * FROM "Customers"
) AS [c]
WHERE [c].[ContactName] = [c].[CompanyName]
""");
    }

    public override async Task SqlQueryRaw_with_dbParameter(bool async)
    {
        await base.SqlQueryRaw_with_dbParameter(async);

        AssertSql(
            """
@city='London' (Nullable = false) (Size = 6)

SELECT * FROM "Customers" WHERE "City" = @city
""");
    }

    public override async Task SqlQueryRaw_with_dbParameter_without_name_prefix(bool async)
    {
        await base.SqlQueryRaw_with_dbParameter_without_name_prefix(async);
        AssertSql(
            """
city='London' (Nullable = false) (Size = 6)

SELECT * FROM "Customers" WHERE "City" = @city
""");
    }

    public override async Task SqlQueryRaw_with_dbParameter_mixed(bool async)
    {
        await base.SqlQueryRaw_with_dbParameter_mixed(async);

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

    public override async Task SqlQueryRaw_with_db_parameters_called_multiple_times(bool async)
    {
        await base.SqlQueryRaw_with_db_parameters_called_multiple_times(async);

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

    public override async Task SqlQuery_with_inlined_db_parameter(bool async)
    {
        await base.SqlQuery_with_inlined_db_parameter(async);

        AssertSql(
            """
@somename='ALFKI' (Nullable = false) (Size = 5)

SELECT * FROM "Customers" WHERE "CustomerID" = @somename
""");
    }

    public override async Task SqlQuery_with_inlined_db_parameter_without_name_prefix(bool async)
    {
        await base.SqlQuery_with_inlined_db_parameter_without_name_prefix(async);

        AssertSql(
            """
somename='ALFKI' (Nullable = false) (Size = 5)

SELECT * FROM "Customers" WHERE "CustomerID" = @somename
""");
    }

    public override async Task SqlQuery_parameterization_issue_12213(bool async)
    {
        await base.SqlQuery_parameterization_issue_12213(async);

        AssertSql(
            """
p0='10300'

SELECT [o].[OrderID]
FROM (
    SELECT * FROM "Orders" WHERE "OrderID" >= @p0
) AS [o]
""",
            //
            """
@__max_1='10400'
p0='10300'

SELECT [o].[OrderID]
FROM (
    SELECT * FROM "Orders"
) AS [o]
WHERE [o].[OrderID] <= @__max_1 AND [o].[OrderID] IN (
    SELECT [o0].[OrderID]
    FROM (
        SELECT * FROM "Orders" WHERE "OrderID" >= @p0
    ) AS [o0]
)
""",
            //
            """
@__max_1='10400'
p0='10300'

SELECT [o].[OrderID]
FROM (
    SELECT * FROM "Orders"
) AS [o]
WHERE [o].[OrderID] <= @__max_1 AND [o].[OrderID] IN (
    SELECT [o0].[OrderID]
    FROM (
        SELECT * FROM "Orders" WHERE "OrderID" >= @p0
    ) AS [o0]
)
""");
    }

    public override async Task SqlQueryRaw_does_not_parameterize_interpolated_string(bool async)
    {
        await base.SqlQueryRaw_does_not_parameterize_interpolated_string(async);

        AssertSql(
            """
p0='10250'

SELECT * FROM "Orders" WHERE "OrderID" < @p0
""");
    }

    public override async Task SqlQueryRaw_with_set_operation(bool async)
    {
        await base.SqlQueryRaw_with_set_operation(async);

        AssertSql(
            """
SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[Region], [c].[PostalCode]
FROM (
    SELECT * FROM "Customers" WHERE "City" = 'London'
) AS [c]
UNION ALL
SELECT [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[CustomerID], [c0].[Fax], [c0].[Phone], [c0].[Region], [c0].[PostalCode]
FROM (
    SELECT * FROM "Customers" WHERE "City" = 'Berlin'
) AS [c0]
""");
    }

    public override async Task Line_endings_after_Select(bool async)
    {
        await base.Line_endings_after_Select(async);

        AssertSql(
            """
SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[Region], [c].[PostalCode]
FROM (
    SELECT
    * FROM "Customers"
) AS [c]
WHERE [c].[City] = N'Seattle'
""");
    }

    public override async Task SqlQueryRaw_in_subquery_with_dbParameter(bool async)
    {
        await base.SqlQueryRaw_in_subquery_with_dbParameter(async);

        AssertSql(
            """
@city='London' (Nullable = false) (Size = 6)

SELECT [o].[CustomerID], [o].[EmployeeID], [o].[Freight], [o].[OrderDate], [o].[OrderID], [o].[RequiredDate], [o].[ShipAddress], [o].[ShipCity], [o].[ShipCountry], [o].[ShipName], [o].[ShipPostalCode], [o].[ShipRegion], [o].[ShipVia], [o].[ShippedDate]
FROM (
    SELECT * FROM "Orders"
) AS [o]
WHERE [o].[CustomerID] IN (
    SELECT [c].[CustomerID]
    FROM (
        SELECT * FROM "Customers" WHERE "City" = @city
    ) AS [c]
)
""");
    }

    public override async Task SqlQueryRaw_in_subquery_with_positional_dbParameter_without_name(bool async)
    {
        await base.SqlQueryRaw_in_subquery_with_positional_dbParameter_without_name(async);

        AssertSql(
            """
p0='London' (Nullable = false) (Size = 6)

SELECT [o].[CustomerID], [o].[EmployeeID], [o].[Freight], [o].[OrderDate], [o].[OrderID], [o].[RequiredDate], [o].[ShipAddress], [o].[ShipCity], [o].[ShipCountry], [o].[ShipName], [o].[ShipPostalCode], [o].[ShipRegion], [o].[ShipVia], [o].[ShippedDate]
FROM (
    SELECT * FROM "Orders"
) AS [o]
WHERE [o].[CustomerID] IN (
    SELECT [c].[CustomerID]
    FROM (
        SELECT * FROM "Customers" WHERE "City" = @p0
    ) AS [c]
)
""");
    }

    public override async Task SqlQueryRaw_in_subquery_with_positional_dbParameter_with_name(bool async)
    {
        await base.SqlQueryRaw_in_subquery_with_positional_dbParameter_with_name(async);

        AssertSql(
            """
@city='London' (Nullable = false) (Size = 6)

SELECT [o].[CustomerID], [o].[EmployeeID], [o].[Freight], [o].[OrderDate], [o].[OrderID], [o].[RequiredDate], [o].[ShipAddress], [o].[ShipCity], [o].[ShipCountry], [o].[ShipName], [o].[ShipPostalCode], [o].[ShipRegion], [o].[ShipVia], [o].[ShippedDate]
FROM (
    SELECT * FROM "Orders"
) AS [o]
WHERE [o].[CustomerID] IN (
    SELECT [c].[CustomerID]
    FROM (
        SELECT * FROM "Customers" WHERE "City" = @city
    ) AS [c]
)
""");
    }

    public override async Task SqlQueryRaw_with_dbParameter_mixed_in_subquery(bool async)
    {
        await base.SqlQueryRaw_with_dbParameter_mixed_in_subquery(async);

        AssertSql(
            """
p0='London' (Size = 4000)
@title='Sales Representative' (Nullable = false) (Size = 20)

SELECT [o].[CustomerID], [o].[EmployeeID], [o].[Freight], [o].[OrderDate], [o].[OrderID], [o].[RequiredDate], [o].[ShipAddress], [o].[ShipCity], [o].[ShipCountry], [o].[ShipName], [o].[ShipPostalCode], [o].[ShipRegion], [o].[ShipVia], [o].[ShippedDate]
FROM (
    SELECT * FROM "Orders"
) AS [o]
WHERE [o].[CustomerID] IN (
    SELECT [c].[CustomerID]
    FROM (
        SELECT * FROM "Customers" WHERE "City" = @p0 AND "ContactTitle" = @title
    ) AS [c]
)
""",
            //
            """
@city='London' (Nullable = false) (Size = 6)
p1='Sales Representative' (Size = 4000)

SELECT [o].[CustomerID], [o].[EmployeeID], [o].[Freight], [o].[OrderDate], [o].[OrderID], [o].[RequiredDate], [o].[ShipAddress], [o].[ShipCity], [o].[ShipCountry], [o].[ShipName], [o].[ShipPostalCode], [o].[ShipRegion], [o].[ShipVia], [o].[ShippedDate]
FROM (
    SELECT * FROM "Orders"
) AS [o]
WHERE [o].[CustomerID] IN (
    SELECT [c].[CustomerID]
    FROM (
        SELECT * FROM "Customers" WHERE "City" = @city AND "ContactTitle" = @p1
    ) AS [c]
)
""");
    }

    public override async Task Multiple_occurrences_of_SqlQuery_with_db_parameter_adds_parameter_only_once(bool async)
    {
        await base.Multiple_occurrences_of_SqlQuery_with_db_parameter_adds_parameter_only_once(async);

        AssertSql(
            """
city='Seattle' (Nullable = false) (Size = 7)

SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[Region], [c].[PostalCode]
FROM (
    SELECT * FROM "Customers" WHERE "City" = @city
) AS [c]
INTERSECT
SELECT [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[CustomerID], [c0].[Fax], [c0].[Phone], [c0].[Region], [c0].[PostalCode]
FROM (
    SELECT * FROM "Customers" WHERE "City" = @city
) AS [c0]
""");
    }

    public override async Task SqlQueryRaw_composed_with_common_table_expression(bool async)
    {
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(() => base.SqlQueryRaw_composed_with_common_table_expression(async));

        Assert.Equal(RelationalStrings.FromSqlNonComposable, exception.Message);
    }

    protected override DbParameter CreateDbParameter(string name, object value)
        => new SqlParameter { ParameterName = name, Value = value };

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
