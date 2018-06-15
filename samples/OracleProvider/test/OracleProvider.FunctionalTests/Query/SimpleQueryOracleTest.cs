// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable UnusedParameter.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public class SimpleQueryOracleTest : SimpleQueryTestBase<NorthwindQueryOracleFixture<NoopModelCustomizer>>
    {
        public SimpleQueryOracleTest(NorthwindQueryOracleFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override void Query_backed_by_database_view()
        {
            // TODO: #10680
            //base.Query_backed_by_database_view();
        }

        public override Task QueryType_with_defining_query()
        {
            // TODO: #10680
            //await base.QueryType_with_defining_query();
            return Task.CompletedTask;
        }

        public override Task QueryType_select_where_navigation()
        {
            // TODO: #10680
            //await base.QueryType_select_where_navigation();
            return Task.CompletedTask;
        }

        public override Task QueryType_select_where_navigation_multi_level()
        {
            // TODO: #10680
            //await base.QueryType_select_where_navigation_multi_level();
            return Task.CompletedTask;
        }

        public override Task QueryType_with_included_nav()
        {
            // TODO: #10680
            //await base.QueryType_with_included_nav();
            return Task.CompletedTask;
        }

        public override Task QueryType_with_included_navs_multi_level()
        {
            // TODO: #10680
            //await base.QueryType_with_included_navs_multi_level();
            return Task.CompletedTask;
        }

        public override Task QueryType_with_mixed_tracking()
        {
            // TODO: #10680
            //await base.QueryType_with_mixed_tracking();
            return Task.CompletedTask;
        }

        public override void Select_nested_collection_multi_level5()
        {
            // Throws: ORA-00600: internal error code
            // Oracle engine bug?
        }

        public override Task Where_math_log_new_base()
        {
            // Oracle doesn't short-circuit AND
            return Task.CompletedTask;
        }

        public override Task Where_math_log()
        {
            // Oracle doesn't short-circuit AND
            return Task.CompletedTask;
        }

        public override Task Where_math_log10()
        {
            // Oracle doesn't short-circuit AND
            return Task.CompletedTask;
        }

        [ConditionalFact(Skip = "See issue#10563")]
        public override void Indexof_with_emptystring()
        {
            base.Indexof_with_emptystring();
        }

        public override Task DefaultIfEmpty_in_subquery_nested()
        {
            return AssertQueryAsync<Customer, Order>(
                (cs, os) =>
                    (from c in cs.Where(c => c.City == "Seattle")
                     from o1 in os.Where(o => o.OrderID > 11000).DefaultIfEmpty()
                     from o2 in os.Where(o => o.CustomerID == c.CustomerID).DefaultIfEmpty()
                     where o1 != null && o2 != null
                     orderby o1.CustomerID, o1.OrderID, o2.OrderDate
                     select new { c.CustomerID, o1.OrderID, o2.OrderDate }),
                e => e.CustomerID + " " + e.OrderID);
        }

        [ConditionalFact(Skip = "See issue#10520")]
        public override Task Where_datetime_today()
        {
            return base.Where_datetime_today();
        }

        public override void Shaper_command_caching_when_parameter_names_different()
        {
            base.Shaper_command_caching_when_parameter_names_different();

            AssertSql(
                @"SELECT COUNT(*)
FROM ""Customers"" ""e""
WHERE ""e"".""CustomerID"" = N'ALFKI'",
                //
                @"SELECT COUNT(*)
FROM ""Customers"" ""e""
WHERE ""e"".""CustomerID"" = N'ALFKI'");
        }

        public override async Task Entity_equality_self()
        {
            await base.Entity_equality_self();

            AssertSql(
                @"SELECT ""c"".""CustomerID""
FROM ""Customers"" ""c""
WHERE ""c"".""CustomerID"" = ""c"".""CustomerID""");
        }

        public override async Task Entity_equality_local()
        {
            await base.Entity_equality_local();

            AssertSql(
                @":local_0_CustomerID='ANATR' (Nullable = false) (Size = 5)

SELECT ""c"".""CustomerID""
FROM ""Customers"" ""c""
WHERE ""c"".""CustomerID"" = :local_0_CustomerID");
        }

        public override async Task Entity_equality_local_inline()
        {
            await base.Entity_equality_local_inline();

            AssertSql(
                @"SELECT ""c"".""CustomerID""
FROM ""Customers"" ""c""
WHERE ""c"".""CustomerID"" = N'ANATR'");
        }

        public override async Task Entity_equality_null()
        {
            await base.Entity_equality_null();

            AssertSql(
                @"SELECT ""c"".""CustomerID""
FROM ""Customers"" ""c""
WHERE ""c"".""CustomerID"" IS NULL");
        }

        public override async Task Entity_equality_not_null()
        {
            await base.Entity_equality_not_null();

            AssertSql(
                @"SELECT ""c"".""CustomerID""
FROM ""Customers"" ""c""
WHERE ""c"".""CustomerID"" IS NOT NULL");
        }

        public override async Task Queryable_reprojection()
        {
            await base.Queryable_reprojection();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""");
        }

        public override async Task Default_if_empty_top_level()
        {
            await base.Default_if_empty_top_level();

            AssertSql(
                @"SELECT ""t"".""EmployeeID"", ""t"".""City"", ""t"".""Country"", ""t"".""FirstName"", ""t"".""ReportsTo"", ""t"".""Title""
FROM (
    SELECT NULL ""empty"" FROM DUAL
) ""empty""
LEFT JOIN (
    SELECT ""c"".""EmployeeID"", ""c"".""City"", ""c"".""Country"", ""c"".""FirstName"", ""c"".""ReportsTo"", ""c"".""Title""
    FROM ""Employees"" ""c""
    WHERE ""c"".""EmployeeID"" = 4294967295
) ""t"" ON 1 = 1");
        }

        public override async Task Default_if_empty_top_level_positive()
        {
            await base.Default_if_empty_top_level_positive();

            AssertSql(
                @"SELECT ""t"".""EmployeeID"", ""t"".""City"", ""t"".""Country"", ""t"".""FirstName"", ""t"".""ReportsTo"", ""t"".""Title""
FROM (
    SELECT NULL ""empty"" FROM DUAL
) ""empty""
LEFT JOIN (
    SELECT ""c"".""EmployeeID"", ""c"".""City"", ""c"".""Country"", ""c"".""FirstName"", ""c"".""ReportsTo"", ""c"".""Title""
    FROM ""Employees"" ""c""
    WHERE ""c"".""EmployeeID"" > 0
) ""t"" ON 1 = 1");
        }

        public override async Task Default_if_empty_top_level_arg()
        {
            await base.Default_if_empty_top_level_arg();

            AssertSql(
                @"SELECT ""c"".""EmployeeID"", ""c"".""City"", ""c"".""Country"", ""c"".""FirstName"", ""c"".""ReportsTo"", ""c"".""Title""
FROM ""Employees"" ""c""
WHERE ""c"".""EmployeeID"" = 4294967295");
        }

        public override async Task Where_subquery_anon()
        {
            await base.Where_subquery_anon();

            AssertSql(
                @":p_0='3'

SELECT ""t"".""EmployeeID"", ""t"".""City"", ""t"".""Country"", ""t"".""FirstName"", ""t"".""ReportsTo"", ""t"".""Title"", ""t0"".""OrderID"", ""t0"".""CustomerID"", ""t0"".""EmployeeID"", ""t0"".""OrderDate""
FROM (
    SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
    FROM ""Employees"" ""e""
) ""t""
CROSS JOIN (
    SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
    FROM ""Orders"" ""o""
) ""t0""");
        }

        public override async Task Let_any_subquery_anonymous()
        {
            await base.Let_any_subquery_anonymous();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
WHERE ""c"".""CustomerID"" LIKE N'A' || N'%' AND (SUBSTR(""c"".""CustomerID"", 1, LENGTH(N'A')) = N'A')
ORDER BY ""c"".""CustomerID"" NULLS FIRST",
                //
                @":outer_CustomerID='ALFKI' (Size = 5)

SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM ""Orders"" ""o0""
        WHERE ""o0"".""CustomerID"" = :outer_CustomerID)
    THEN 1 ELSE 0
END FROM DUAL",
                //
                @":outer_CustomerID='ANATR' (Size = 5)

SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM ""Orders"" ""o0""
        WHERE ""o0"".""CustomerID"" = :outer_CustomerID)
    THEN 1 ELSE 0
END FROM DUAL",
                //
                @":outer_CustomerID='ANTON' (Size = 5)

SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM ""Orders"" ""o0""
        WHERE ""o0"".""CustomerID"" = :outer_CustomerID)
    THEN 1 ELSE 0
END FROM DUAL",
                //
                @":outer_CustomerID='AROUT' (Size = 5)

SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM ""Orders"" ""o0""
        WHERE ""o0"".""CustomerID"" = :outer_CustomerID)
    THEN 1 ELSE 0
END FROM DUAL");
        }

        public override async Task OrderBy_arithmetic()
        {
            await base.OrderBy_arithmetic();

            AssertSql(
                @"SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Employees"" ""e""
ORDER BY ""e"".""EmployeeID"" - ""e"".""EmployeeID"" NULLS FIRST");
        }

        public override async Task OrderBy_condition_comparison()
        {
            await base.OrderBy_condition_comparison();

            AssertSql(
                @"SELECT ""p"".""ProductID"", ""p"".""Discontinued"", ""p"".""ProductName"", ""p"".""SupplierID"", ""p"".""UnitPrice"", ""p"".""UnitsInStock""
FROM ""Products"" ""p""
ORDER BY CASE
    WHEN ""p"".""UnitsInStock"" > 0
    THEN 1 ELSE 0
END NULLS FIRST, ""p"".""ProductID"" NULLS FIRST");
        }

        public override async Task OrderBy_ternary_conditions()
        {
            await base.OrderBy_ternary_conditions();

            AssertSql(
                @"SELECT ""p"".""ProductID"", ""p"".""Discontinued"", ""p"".""ProductName"", ""p"".""SupplierID"", ""p"".""UnitPrice"", ""p"".""UnitsInStock""
FROM ""Products"" ""p""
ORDER BY CASE
    WHEN ((""p"".""UnitsInStock"" > 10) AND (""p"".""ProductID"" > 40)) OR ((""p"".""UnitsInStock"" <= 10) AND (""p"".""ProductID"" <= 40))
    THEN 1 ELSE 0
END NULLS FIRST, ""p"".""ProductID"" NULLS FIRST");
        }

        public override void OrderBy_any()
        {
            base.OrderBy_any();

            AssertSql(
                @"SELECT ""p"".""CustomerID"", ""p"".""Address"", ""p"".""City"", ""p"".""CompanyName"", ""p"".""ContactName"", ""p"".""ContactTitle"", ""p"".""Country"", ""p"".""Fax"", ""p"".""Phone"", ""p"".""PostalCode"", ""p"".""Region""
FROM ""Customers"" ""p""
ORDER BY (
    SELECT CASE
        WHEN EXISTS (
            SELECT 1
            FROM ""Orders"" ""o""
            WHERE (""o"".""OrderID"" > 11000) AND (""p"".""CustomerID"" = ""o"".""CustomerID""))
        THEN 1 ELSE 0
    END FROM DUAL
) NULLS FIRST, ""p"".""CustomerID"" NULLS FIRST");
        }

        public override async Task Skip()
        {
            await base.Skip();

            AssertSql(
                @":p_0='5'

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
ORDER BY ""c"".""CustomerID"" NULLS FIRST
OFFSET :p_0 ROWS");
        }

        public override async Task Skip_no_orderby()
        {
            await base.Skip_no_orderby();

            AssertSql(
                @":p_0='5'

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
OFFSET :p_0 ROWS");
        }

        public override async Task Skip_Take()
        {
            await base.Skip_Take();

            AssertSql(
                @":p_0='5'
:p_1='10'

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
ORDER BY ""c"".""ContactName"" NULLS FIRST
OFFSET :p_0 ROWS FETCH NEXT :p_1 ROWS ONLY");
        }

        public override async Task Join_Customers_Orders_Skip_Take()
        {
            await base.Join_Customers_Orders_Skip_Take();

            AssertSql(
                @":p_0='10'
:p_1='5'

SELECT ""c"".""ContactName"", ""o"".""OrderID""
FROM ""Customers"" ""c""
INNER JOIN ""Orders"" ""o"" ON ""c"".""CustomerID"" = ""o"".""CustomerID""
ORDER BY ""o"".""OrderID"" NULLS FIRST
OFFSET :p_0 ROWS FETCH NEXT :p_1 ROWS ONLY");
        }

        public override async Task Join_Customers_Orders_Orders_Skip_Take_Same_Properties()
        {
            await base.Join_Customers_Orders_Orders_Skip_Take_Same_Properties();

            AssertSql(
                @":p_0='10'
:p_1='5'

SELECT ""o"".""OrderID"", ""ca"".""CustomerID"" ""CustomerIDA"", ""cb"".""CustomerID"" ""CustomerIDB"", ""ca"".""ContactName"" ""ContactNameA"", ""cb"".""ContactName"" ""ContactNameB""
FROM ""Orders"" ""o""
INNER JOIN ""Customers"" ""ca"" ON ""o"".""CustomerID"" = ""ca"".""CustomerID""
INNER JOIN ""Customers"" ""cb"" ON ""o"".""CustomerID"" = ""cb"".""CustomerID""
ORDER BY ""o"".""OrderID"" NULLS FIRST
OFFSET :p_0 ROWS FETCH NEXT :p_1 ROWS ONLY");
        }

        public override async Task Take_Skip_Distinct_Caching()
        {
            await base.Take_Skip_Distinct_Caching();

            AssertSql(
                @":p_0='10'
:p_1='5'

SELECT DISTINCT ""t0"".*
FROM (
    SELECT ""t"".*
    FROM (
        SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
        FROM ""Customers"" ""c""
        ORDER BY ""c"".""ContactName"" NULLS FIRST
        FETCH FIRST :p_0 ROWS ONLY
    ) ""t""
    ORDER BY ""t"".""ContactName"" NULLS FIRST
    OFFSET :p_1 ROWS
) ""t0""",
                //
                @":p_0='15'
:p_1='10'

SELECT DISTINCT ""t0"".*
FROM (
    SELECT ""t"".*
    FROM (
        SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
        FROM ""Customers"" ""c""
        ORDER BY ""c"".""ContactName"" NULLS FIRST
        FETCH FIRST :p_0 ROWS ONLY
    ) ""t""
    ORDER BY ""t"".""ContactName"" NULLS FIRST
    OFFSET :p_1 ROWS
) ""t0""");
        }

        public void Skip_when_no_OrderBy()
        {
            Assert.Throws<Exception>(() => CreateContext().Set<Customer>().Skip(5).Take(10).ToList());
        }

        public override async Task Null_conditional_simple()
        {
            await base.Null_conditional_simple();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
WHERE ""c"".""CustomerID"" = N'ALFKI'");
        }

        public override async Task Queryable_simple()
        {
            await base.Queryable_simple();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""");
        }

        public override async Task Queryable_simple_anonymous()
        {
            await base.Queryable_simple_anonymous();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""");
        }

        public override async Task Queryable_nested_simple()
        {
            await base.Queryable_nested_simple();

            AssertSql(
                @"SELECT ""c3"".""CustomerID"", ""c3"".""Address"", ""c3"".""City"", ""c3"".""CompanyName"", ""c3"".""ContactName"", ""c3"".""ContactTitle"", ""c3"".""Country"", ""c3"".""Fax"", ""c3"".""Phone"", ""c3"".""PostalCode"", ""c3"".""Region""
FROM ""Customers"" ""c3""");
        }

        public override async Task Take_simple()
        {
            await base.Take_simple();

            AssertSql(
                @":p_0='10'

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
ORDER BY ""c"".""CustomerID"" NULLS FIRST
FETCH FIRST :p_0 ROWS ONLY");
        }

        public override void Any_simple()
        {
            base.Any_simple();

            AssertSql(
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM ""Customers"" ""c"")
    THEN 1 ELSE 0
END FROM DUAL");
        }

        public override void Any_predicate()
        {
            base.Any_predicate();

            AssertSql(
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM ""Customers"" ""c""
        WHERE ""c"".""ContactName"" LIKE N'A' || N'%' AND (SUBSTR(""c"".""ContactName"", 1, LENGTH(N'A')) = N'A'))
    THEN 1 ELSE 0
END FROM DUAL");
        }

        public override async Task Any_nested_negated()
        {
            await base.Any_nested_negated();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
WHERE NOT EXISTS (
    SELECT 1
    FROM ""Orders"" ""o""
    WHERE ""o"".""CustomerID"" LIKE N'A' || N'%' AND (SUBSTR(""o"".""CustomerID"", 1, LENGTH(N'A')) = N'A'))");
        }

        public override async Task Any_nested_negated2()
        {
            await base.Any_nested_negated2();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
WHERE ((""c"".""City"" <> N'London') OR ""c"".""City"" IS NULL) AND NOT EXISTS (
    SELECT 1
    FROM ""Orders"" ""o""
    WHERE ""o"".""CustomerID"" LIKE N'A' || N'%' AND (SUBSTR(""o"".""CustomerID"", 1, LENGTH(N'A')) = N'A'))");
        }

        public override async Task Any_nested_negated3()
        {
            await base.Any_nested_negated3();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
WHERE NOT EXISTS (
    SELECT 1
    FROM ""Orders"" ""o""
    WHERE ""o"".""CustomerID"" LIKE N'A' || N'%' AND (SUBSTR(""o"".""CustomerID"", 1, LENGTH(N'A')) = N'A')) AND ((""c"".""City"" <> N'London') OR ""c"".""City"" IS NULL)");
        }

        public override async Task Any_nested()
        {
            await base.Any_nested();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
WHERE EXISTS (
    SELECT 1
    FROM ""Orders"" ""o""
    WHERE ""o"".""CustomerID"" LIKE N'A' || N'%' AND (SUBSTR(""o"".""CustomerID"", 1, LENGTH(N'A')) = N'A'))");
        }

        public override async Task Any_nested2()
        {
            await base.Any_nested2();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
WHERE ((""c"".""City"" <> N'London') OR ""c"".""City"" IS NULL) AND EXISTS (
    SELECT 1
    FROM ""Orders"" ""o""
    WHERE ""o"".""CustomerID"" LIKE N'A' || N'%' AND (SUBSTR(""o"".""CustomerID"", 1, LENGTH(N'A')) = N'A'))");
        }

        public override async Task Any_nested3()
        {
            await base.Any_nested3();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
WHERE EXISTS (
    SELECT 1
    FROM ""Orders"" ""o""
    WHERE ""o"".""CustomerID"" LIKE N'A' || N'%' AND (SUBSTR(""o"".""CustomerID"", 1, LENGTH(N'A')) = N'A')) AND ((""c"".""City"" <> N'London') OR ""c"".""City"" IS NULL)");
        }

        public override void Any_with_multiple_conditions_still_uses_exists()
        {
            base.Any_with_multiple_conditions_still_uses_exists();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
WHERE (""c"".""City"" = N'London') AND EXISTS (
    SELECT 1
    FROM ""Orders"" ""o""
    WHERE (""o"".""EmployeeID"" = 1) AND (""c"".""CustomerID"" = ""o"".""CustomerID""))");
        }

        public override void All_top_level()
        {
            base.All_top_level();

            AssertSql(
                @"SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM ""Customers"" ""c""
        WHERE NOT (""c"".""ContactName"" LIKE N'A' || N'%') OR (SUBSTR(""c"".""ContactName"", 1, LENGTH(N'A')) <> N'A'))
    THEN 1 ELSE 0
END FROM DUAL");
        }

        public override void All_top_level_column()
        {
            base.All_top_level_column();

            AssertSql(
                @"SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM ""Customers"" ""c""
        WHERE (NOT (""c"".""ContactName"" LIKE ""c"".""ContactName"" || N'%') OR (SUBSTR(""c"".""ContactName"", 1, LENGTH(""c"".""ContactName"")) <> ""c"".""ContactName"")) AND ((""c"".""ContactName"" <> N'') OR ""c"".""ContactName"" IS NULL))
    THEN 1 ELSE 0
END FROM DUAL");
        }

        public override void First_client_predicate()
        {
            base.First_client_predicate();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
ORDER BY ""c"".""CustomerID"" NULLS FIRST");
        }

        public override async Task Where_select_many_or()
        {
            await base.Where_select_many_or();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Customers"" ""c""
CROSS JOIN ""Employees"" ""e""
WHERE (""c"".""City"" = N'London') OR (""e"".""City"" = N'London')");
        }

        public override async Task Where_select_many_or2()
        {
            await base.Where_select_many_or2();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Customers"" ""c""
CROSS JOIN ""Employees"" ""e""
WHERE ""c"".""City"" IN (N'London', N'Berlin')");
        }

        public override async Task Where_select_many_or3()
        {
            await base.Where_select_many_or3();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Customers"" ""c""
CROSS JOIN ""Employees"" ""e""
WHERE ""c"".""City"" IN (N'London', N'Berlin', N'Seattle')");
        }

        public override async Task Where_select_many_or4()
        {
            await base.Where_select_many_or4();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Customers"" ""c""
CROSS JOIN ""Employees"" ""e""
WHERE ""c"".""City"" IN (N'London', N'Berlin', N'Seattle', N'Lisboa')");
        }

        public override async Task Where_select_many_or_with_parameter()
        {
            await base.Where_select_many_or_with_parameter();

            AssertSql(
                @":london_0='London' (Size = 2000)
:lisboa_1='Lisboa' (Size = 2000)

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Customers"" ""c""
CROSS JOIN ""Employees"" ""e""
WHERE ""c"".""City"" IN (:london_0, N'Berlin', N'Seattle', :lisboa_1)");
        }

        public override async Task SelectMany_simple1()
        {
            await base.SelectMany_simple1();

            AssertSql(
                @"SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title"", ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Employees"" ""e""
CROSS JOIN ""Customers"" ""c""");
        }

        public override async Task SelectMany_simple2()
        {
            await base.SelectMany_simple2();

            AssertSql(
                @"SELECT ""e1"".""EmployeeID"", ""e1"".""City"", ""e1"".""Country"", ""e1"".""FirstName"", ""e1"".""ReportsTo"", ""e1"".""Title"", ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", ""e2"".""FirstName"" ""FirstName0""
FROM ""Employees"" ""e1""
CROSS JOIN ""Customers"" ""c""
CROSS JOIN ""Employees"" ""e2""");
        }

        public override async Task SelectMany_entity_deep()
        {
            await base.SelectMany_entity_deep();

            AssertSql(
                @"SELECT ""e1"".""EmployeeID"", ""e1"".""City"", ""e1"".""Country"", ""e1"".""FirstName"", ""e1"".""ReportsTo"", ""e1"".""Title"", ""e2"".""EmployeeID"", ""e2"".""City"", ""e2"".""Country"", ""e2"".""FirstName"", ""e2"".""ReportsTo"", ""e2"".""Title"", ""e3"".""EmployeeID"", ""e3"".""City"", ""e3"".""Country"", ""e3"".""FirstName"", ""e3"".""ReportsTo"", ""e3"".""Title"", ""e4"".""EmployeeID"", ""e4"".""City"", ""e4"".""Country"", ""e4"".""FirstName"", ""e4"".""ReportsTo"", ""e4"".""Title""
FROM ""Employees"" ""e1""
CROSS JOIN ""Employees"" ""e2""
CROSS JOIN ""Employees"" ""e3""
CROSS JOIN ""Employees"" ""e4""");
        }

        public override async Task SelectMany_projection1()
        {
            await base.SelectMany_projection1();

            AssertSql(
                @"SELECT ""e1"".""City"", ""e2"".""Country""
FROM ""Employees"" ""e1""
CROSS JOIN ""Employees"" ""e2""");
        }

        public override async Task SelectMany_projection2()
        {
            await base.SelectMany_projection2();

            AssertSql(
                @"SELECT ""e1"".""City"", ""e2"".""Country"", ""e3"".""FirstName""
FROM ""Employees"" ""e1""
CROSS JOIN ""Employees"" ""e2""
CROSS JOIN ""Employees"" ""e3""");
        }

        public override void SelectMany_Count()
        {
            base.SelectMany_Count();

            AssertSql(
                @"SELECT COUNT(*)
FROM ""Customers"" ""c""
CROSS JOIN ""Orders"" ""o""");
        }

        public override void SelectMany_LongCount()
        {
            base.SelectMany_LongCount();

            AssertSql(
                @"SELECT COUNT(*)
FROM ""Customers"" ""c""
CROSS JOIN ""Orders"" ""o""");
        }

        public override void SelectMany_OrderBy_ThenBy_Any()
        {
            base.SelectMany_OrderBy_ThenBy_Any();

            AssertSql(
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM ""Customers"" ""c""
        CROSS JOIN ""Orders"" ""o"")
    THEN 1 ELSE 0
END FROM DUAL");
        }

        public override void Join_Where_Count()
        {
            base.Join_Where_Count();

            AssertSql(
                @"SELECT COUNT(*)
FROM ""Customers"" ""c""
INNER JOIN ""Orders"" ""o"" ON ""c"".""CustomerID"" = ""o"".""CustomerID""
WHERE ""c"".""CustomerID"" = N'ALFKI'");
        }

        public override void Join_OrderBy_Count()
        {
            base.Join_OrderBy_Count();

            AssertSql(
                @"SELECT COUNT(*)
FROM ""Customers"" ""c""
INNER JOIN ""Orders"" ""o"" ON ""c"".""CustomerID"" = ""o"".""CustomerID""");
        }

        public override void Multiple_joins_Where_Order_Any()
        {
            base.Multiple_joins_Where_Order_Any();

            AssertSql(
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM ""Customers"" ""c""
        INNER JOIN ""Orders"" ""or"" ON ""c"".""CustomerID"" = ""or"".""CustomerID""
        INNER JOIN ""Order Details"" ""od"" ON ""or"".""OrderID"" = ""od"".""OrderID""
        WHERE ""c"".""City"" = N'London')
    THEN 1 ELSE 0
END FROM DUAL");
        }

        public override async Task Where_join_select()
        {
            await base.Where_join_select();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
INNER JOIN ""Orders"" ""o"" ON ""c"".""CustomerID"" = ""o"".""CustomerID""
WHERE ""c"".""CustomerID"" = N'ALFKI'");
        }

        public override async Task Where_orderby_join_select()
        {
            await base.Where_orderby_join_select();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
INNER JOIN ""Orders"" ""o"" ON ""c"".""CustomerID"" = ""o"".""CustomerID""
WHERE ""c"".""CustomerID"" <> N'ALFKI'
ORDER BY ""c"".""CustomerID"" NULLS FIRST");
        }

        public override async Task Where_join_orderby_join_select()
        {
            await base.Where_join_orderby_join_select();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
INNER JOIN ""Orders"" ""o"" ON ""c"".""CustomerID"" = ""o"".""CustomerID""
INNER JOIN ""Order Details"" ""od"" ON ""o"".""OrderID"" = ""od"".""OrderID""
WHERE ""c"".""CustomerID"" <> N'ALFKI'
ORDER BY ""c"".""CustomerID"" NULLS FIRST");
        }

        public override async Task Where_select_many()
        {
            await base.Where_select_many();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
CROSS JOIN ""Orders"" ""o""
WHERE ""c"".""CustomerID"" = N'ALFKI'");
        }

        public override async Task Where_orderby_select_many()
        {
            await base.Where_orderby_select_many();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
CROSS JOIN ""Orders"" ""o""
WHERE ""c"".""CustomerID"" = N'ALFKI'
ORDER BY ""c"".""CustomerID"" NULLS FIRST");
        }

        public override async Task SelectMany_cartesian_product_with_ordering()
        {
            await base.SelectMany_cartesian_product_with_ordering();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", ""e"".""City"" ""City0""
FROM ""Customers"" ""c""
CROSS JOIN ""Employees"" ""e""
WHERE (""c"".""City"" = ""e"".""City"") OR (""c"".""City"" IS NULL AND ""e"".""City"" IS NULL)
ORDER BY ""City0"" NULLS FIRST, ""c"".""CustomerID"" DESC");
        }

        public override async Task SelectMany_Joined_DefaultIfEmpty()
        {
            await base.SelectMany_Joined_DefaultIfEmpty();

            AssertSql(
                @"SELECT ""t0"".""OrderID"", ""t0"".""CustomerID"", ""t0"".""EmployeeID"", ""t0"".""OrderDate"", ""c"".""ContactName""
FROM ""Customers"" ""c""
CROSS APPLY (
    SELECT ""t"".""OrderID"", ""t"".""CustomerID"", ""t"".""EmployeeID"", ""t"".""OrderDate""
    FROM (
        SELECT NULL ""empty"" FROM DUAL
    ) ""empty""
    LEFT JOIN (
        SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
        FROM ""Orders"" ""o""
        WHERE ""o"".""CustomerID"" = ""c"".""CustomerID""
    ) ""t"" ON 1 = 1
) ""t0""");
        }

        public override void Skip_Take_Any()
        {
            base.Skip_Take_Any();

            AssertSql(
                @":p_0='5'
:p_1='10'

SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM ""Customers"" ""c""
        ORDER BY ""c"".""ContactName"" NULLS FIRST
        OFFSET :p_0 ROWS FETCH NEXT :p_1 ROWS ONLY)
    THEN 1 ELSE 0
END FROM DUAL");
        }

        public override async Task OrderBy()
        {
            await base.OrderBy();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
ORDER BY ""c"".""CustomerID"" NULLS FIRST");
        }

        public override async Task OrderBy_true()
        {
            await base.OrderBy_true();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""");
        }

        public override async Task OrderBy_integer()
        {
            await base.OrderBy_integer();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""");
        }

        public override async Task OrderBy_parameter()
        {
            await base.OrderBy_parameter();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""");
        }

        public override async Task OrderBy_anon()
        {
            await base.OrderBy_anon();

            AssertSql(
                @"SELECT ""c"".""CustomerID""
FROM ""Customers"" ""c""
ORDER BY ""c"".""CustomerID"" NULLS FIRST");
        }

        public override async Task OrderBy_anon2()
        {
            await base.OrderBy_anon2();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
ORDER BY ""c"".""CustomerID"" NULLS FIRST");
        }

        public override async Task OrderBy_client_mixed()
        {
            await base.OrderBy_client_mixed();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""");
        }

        public override async Task OrderBy_shadow()
        {
            await base.OrderBy_shadow();

            AssertSql(
                @"SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Employees"" ""e""
ORDER BY ""e"".""Title"" NULLS FIRST, ""e"".""EmployeeID"" NULLS FIRST");
        }

        public override async Task OrderBy_multiple()
        {
            await base.OrderBy_multiple();

            AssertSql(
                @"SELECT ""c"".""City""
FROM ""Customers"" ""c""
ORDER BY ""c"".""Country"" NULLS FIRST, ""c"".""CustomerID"" NULLS FIRST");
        }

        public override void OrderBy_ThenBy_Any()
        {
            base.OrderBy_ThenBy_Any();

            AssertSql(
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM ""Customers"" ""c"")
    THEN 1 ELSE 0
END FROM DUAL");
        }

        public override async Task Where_subquery_recursive_trivial()
        {
            await base.Where_subquery_recursive_trivial();

            AssertSql(
                @"SELECT ""e1"".""EmployeeID"", ""e1"".""City"", ""e1"".""Country"", ""e1"".""FirstName"", ""e1"".""ReportsTo"", ""e1"".""Title""
FROM ""Employees"" ""e1""
WHERE EXISTS (
    SELECT 1
    FROM ""Employees"" ""e2""
    WHERE EXISTS (
        SELECT 1
        FROM ""Employees"" ""e3""))
ORDER BY ""e1"".""EmployeeID"" NULLS FIRST");
        }

        public override void Select_DTO_distinct_translated_to_server()
        {
            base.Select_DTO_distinct_translated_to_server();

            AssertSql(
                @"SELECT 1
FROM ""Orders"" ""o""
WHERE ""o"".""OrderID"" < 10300");
        }

        public override void Select_DTO_constructor_distinct_translated_to_server()
        {
            base.Select_DTO_constructor_distinct_translated_to_server();

            AssertSql(
                @"SELECT DISTINCT ""o"".""CustomerID""
FROM ""Orders"" ""o""
WHERE ""o"".""OrderID"" < 10300");
        }

        public override void Select_DTO_with_member_init_distinct_translated_to_server()
        {
            base.Select_DTO_with_member_init_distinct_translated_to_server();

            AssertSql(
                @"SELECT DISTINCT ""o"".""CustomerID"" ""Id"", ""o"".""OrderID"" ""Count""
FROM ""Orders"" ""o""
WHERE ""o"".""OrderID"" < 10300");
        }

        public override async Task Select_DTO_with_member_init_distinct_in_subquery_translated_to_server()
        {
            await base.Select_DTO_with_member_init_distinct_in_subquery_translated_to_server();

            AssertSql(
                @"SELECT ""t"".""Id"", ""t"".""Count"", ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM (
    SELECT DISTINCT ""o"".""CustomerID"" ""Id"", ""o"".""OrderID"" ""Count""
    FROM ""Orders"" ""o""
    WHERE ""o"".""OrderID"" < 10300
) ""t""
CROSS JOIN ""Customers"" ""c""
WHERE ""c"".""CustomerID"" = ""t"".""Id""");
        }

        public override void Select_DTO_with_member_init_distinct_in_subquery_used_in_projection_translated_to_server()
        {
            base.Select_DTO_with_member_init_distinct_in_subquery_used_in_projection_translated_to_server();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", ""t"".""Id"", ""t"".""Count""
FROM ""Customers"" ""c""
CROSS JOIN (
    SELECT DISTINCT ""o"".""CustomerID"" ""Id"", ""o"".""OrderID"" ""Count""
    FROM ""Orders"" ""o""
    WHERE ""o"".""OrderID"" < 10300
) ""t""
WHERE ""c"".""CustomerID"" LIKE N'A' || N'%' AND (SUBSTR(""c"".""CustomerID"", 1, LENGTH(N'A')) = N'A')");
        }

        public override async Task Select_correlated_subquery_filtered()
        {
            await base.Select_correlated_subquery_filtered();

            AssertSql(
                @"SELECT ""c"".""CustomerID""
FROM ""Customers"" ""c""
WHERE ""c"".""CustomerID"" LIKE N'A' || N'%' AND (SUBSTR(""c"".""CustomerID"", 1, LENGTH(N'A')) = N'A')
ORDER BY ""c"".""CustomerID"" NULLS FIRST",
                //
                @":outer_CustomerID='ALFKI' (Size = 5)

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""
WHERE ""o"".""CustomerID"" = :outer_CustomerID",
                //
                @":outer_CustomerID='ANATR' (Size = 5)

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""
WHERE ""o"".""CustomerID"" = :outer_CustomerID",
                //
                @":outer_CustomerID='ANTON' (Size = 5)

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""
WHERE ""o"".""CustomerID"" = :outer_CustomerID",
                //
                @":outer_CustomerID='AROUT' (Size = 5)

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""
WHERE ""o"".""CustomerID"" = :outer_CustomerID");
        }

        public override async Task Where_subquery_on_bool()
        {
            await base.Where_subquery_on_bool();

            AssertSql(
                @"SELECT ""p"".""ProductID"", ""p"".""Discontinued"", ""p"".""ProductName"", ""p"".""SupplierID"", ""p"".""UnitPrice"", ""p"".""UnitsInStock""
FROM ""Products"" ""p""
WHERE N'Chai' IN (
    SELECT ""p2"".""ProductName""
    FROM ""Products"" ""p2""
)");
        }

        public override async Task Where_subquery_on_collection()
        {
            await base.Where_subquery_on_collection();

            AssertSql(
                @"SELECT ""p"".""ProductID"", ""p"".""Discontinued"", ""p"".""ProductName"", ""p"".""SupplierID"", ""p"".""UnitPrice"", ""p"".""UnitsInStock""
FROM ""Products"" ""p""
WHERE 5 IN (
    SELECT ""o"".""Quantity""
    FROM ""Order Details"" ""o""
    WHERE ""o"".""ProductID"" = ""p"".""ProductID""
)");
        }

        public override async Task Select_many_cross_join_same_collection()
        {
            await base.Select_many_cross_join_same_collection();

            AssertSql(
                @"SELECT ""c0"".""CustomerID"", ""c0"".""Address"", ""c0"".""City"", ""c0"".""CompanyName"", ""c0"".""ContactName"", ""c0"".""ContactTitle"", ""c0"".""Country"", ""c0"".""Fax"", ""c0"".""Phone"", ""c0"".""PostalCode"", ""c0"".""Region""
FROM ""Customers"" ""c""
CROSS JOIN ""Customers"" ""c0""");
        }

        public override async Task OrderBy_null_coalesce_operator()
        {
            await base.OrderBy_null_coalesce_operator();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
ORDER BY COALESCE(""c"".""Region"", N'ZZ') NULLS FIRST");
        }

        public override async Task Select_null_coalesce_operator()
        {
            await base.Select_null_coalesce_operator();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""CompanyName"", COALESCE(""c"".""Region"", N'ZZ') ""Region""
FROM ""Customers"" ""c""
ORDER BY ""Region"" NULLS FIRST");
        }

        public override async Task Projection_null_coalesce_operator()
        {
            await base.Projection_null_coalesce_operator();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""CompanyName"", COALESCE(""c"".""Region"", N'ZZ') ""Region""
FROM ""Customers"" ""c""");
        }

        public override async Task Filter_coalesce_operator()
        {
            await base.Filter_coalesce_operator();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
WHERE COALESCE(""c"".""CompanyName"", ""c"".""ContactName"") = N'The Big Cheese'");
        }

        public override async Task Select_take_skip_null_coalesce_operator2()
        {
            await base.Select_take_skip_null_coalesce_operator2();

            AssertSql(
                @":p_0='10'
:p_1='5'

SELECT ""t"".*
FROM (
    SELECT ""c"".""CustomerID"", ""c"".""CompanyName"", ""c"".""Region"", COALESCE(""c"".""Region"", N'ZZ') ""c""
    FROM ""Customers"" ""c""
    ORDER BY ""c"" NULLS FIRST
    FETCH FIRST :p_0 ROWS ONLY
) ""t""
ORDER BY ""t"".""c"" NULLS FIRST
OFFSET :p_1 ROWS");
        }

        public override void Selected_column_can_coalesce()
        {
            base.Selected_column_can_coalesce();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
ORDER BY COALESCE(""c"".""Region"", N'ZZ') NULLS FIRST");
        }

        public override void Random_next_is_not_funcletized_1()
        {
            base.Random_next_is_not_funcletized_1();

            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""");
        }

        public override void Random_next_is_not_funcletized_2()
        {
            base.Random_next_is_not_funcletized_2();

            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""");
        }

        public override void Random_next_is_not_funcletized_3()
        {
            base.Random_next_is_not_funcletized_3();

            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""");
        }

        public override void Random_next_is_not_funcletized_4()
        {
            base.Random_next_is_not_funcletized_4();

            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""");
        }

        public override void Random_next_is_not_funcletized_5()
        {
            base.Random_next_is_not_funcletized_5();

            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""");
        }

        public override void Random_next_is_not_funcletized_6()
        {
            base.Random_next_is_not_funcletized_6();

            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""");
        }

        public override async Task Environment_newline_is_funcletized()
        {
            await base.Environment_newline_is_funcletized();

            AssertSql(
                @":NewLine_0='
' (Size = 2000)

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
WHERE INSTR(""c"".""CustomerID"", :NewLine_0) > 0");
        }

        public override async Task String_concat_with_navigation1()
        {
            await base.String_concat_with_navigation1();

            AssertSql(
                @"SELECT (""o"".""CustomerID"" || N' ') || ""o.Customer"".""City""
FROM ""Orders"" ""o""
LEFT JOIN ""Customers"" ""o.Customer"" ON ""o"".""CustomerID"" = ""o.Customer"".""CustomerID""");
        }

        public override async Task String_concat_with_navigation2()
        {
            await base.String_concat_with_navigation2();

            AssertSql(
                @"SELECT (""o.Customer"".""City"" || N' ') || ""o.Customer"".""City""
FROM ""Orders"" ""o""
LEFT JOIN ""Customers"" ""o.Customer"" ON ""o"".""CustomerID"" = ""o.Customer"".""CustomerID""");
        }

        public override async Task Where_bitwise_or()
        {
            await base.Where_bitwise_or();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
WHERE (CASE
    WHEN ""c"".""CustomerID"" = N'ALFKI'
    THEN 1 ELSE 0
END - BITAND(CASE
    WHEN ""c"".""CustomerID"" = N'ALFKI'
    THEN 1 ELSE 0
END, CASE
    WHEN ""c"".""CustomerID"" = N'ANATR'
    THEN 1 ELSE 0
END) + CASE
    WHEN ""c"".""CustomerID"" = N'ANATR'
    THEN 1 ELSE 0
END) = 1");
        }

        public override async Task Where_bitwise_and()
        {
            await base.Where_bitwise_and();

            AssertSql(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
WHERE (BITAND(CASE
    WHEN ""c"".""CustomerID"" = N'ALFKI'
    THEN 1 ELSE 0
END, CASE
    WHEN ""c"".""CustomerID"" = N'ANATR'
    THEN 1 ELSE 0
END)) = 1");
        }

        public override async Task Query_expression_with_to_string_and_contains()
        {
            await base.Query_expression_with_to_string_and_contains();

            AssertSql(
                @"SELECT ""o"".""CustomerID""
FROM ""Orders"" ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL AND (INSTR(CAST(""o"".""EmployeeID"" AS VARCHAR2(10)), N'10') > 0)");
        }

        public override async Task Select_expression_long_to_string()
        {
            await base.Select_expression_long_to_string();

            AssertSql(
                @"SELECT CAST(""o"".""OrderID"" AS VARCHAR2(20)) ""ShipName""
FROM ""Orders"" ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL");
        }

        public override async Task Select_expression_int_to_string()
        {
            await base.Select_expression_int_to_string();

            AssertSql(
                @"SELECT CAST(""o"".""OrderID"" AS VARCHAR2(11)) ""ShipName""
FROM ""Orders"" ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL");
        }

        public override async Task ToString_with_formatter_is_evaluated_on_the_client()
        {
            await base.ToString_with_formatter_is_evaluated_on_the_client();

            AssertSql(
                @"SELECT ""o"".""OrderID""
FROM ""Orders"" ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL",
                //
                @"SELECT ""o"".""OrderID""
FROM ""Orders"" ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL");
        }

        public override async Task Select_expression_other_to_string()
        {
            await base.Select_expression_other_to_string();

            AssertSql(
                @"SELECT CAST(""o"".""OrderDate"" AS VARCHAR2(100)) ""ShipName""
FROM ""Orders"" ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL");
        }

        public override async Task Select_expression_date_add_milliseconds_above_the_range()
        {
            await base.Select_expression_date_add_milliseconds_above_the_range();

            AssertSql(
                @"SELECT ""o"".""OrderDate""
FROM ""Orders"" ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL");
        }

        public override async Task Select_expression_date_add_milliseconds_below_the_range()
        {
            await base.Select_expression_date_add_milliseconds_below_the_range();

            AssertSql(
                @"SELECT ""o"".""OrderDate""
FROM ""Orders"" ""o""
WHERE ""o"".""OrderDate"" IS NOT NULL");
        }

        public override async Task Select_expression_references_are_updated_correctly_with_subquery()
        {
            await base.Select_expression_references_are_updated_correctly_with_subquery();

            AssertSql(
                @":nextYear_0='2017'

SELECT ""t"".""c""
FROM (
    SELECT DISTINCT EXTRACT(YEAR FROM ""o"".""OrderDate"") ""c""
    FROM ""Orders"" ""o""
    WHERE ""o"".""OrderDate"" IS NOT NULL
) ""t""
WHERE ""t"".""c"" < :nextYear_0");
        }

        public override async Task OrderBy_skip_take()
        {
            await base.OrderBy_skip_take();

            AssertSql(
                @":p_0='5'
:p_1='8'

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
ORDER BY ""c"".""ContactTitle"" NULLS FIRST, ""c"".""ContactName"" NULLS FIRST
OFFSET :p_0 ROWS FETCH NEXT :p_1 ROWS ONLY");
        }

        public override async Task OrderBy_skip_take_distinct()
        {
            await base.OrderBy_skip_take_distinct();

            AssertSql(
                @":p_0='5'
:p_1='15'

SELECT DISTINCT ""t"".*
FROM (
    SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
    FROM ""Customers"" ""c""
    ORDER BY ""c"".""ContactTitle"" NULLS FIRST, ""c"".""ContactName"" NULLS FIRST
    OFFSET :p_0 ROWS FETCH NEXT :p_1 ROWS ONLY
) ""t""");
        }

        public override async Task OrderBy_coalesce_skip_take_distinct()
        {
            await base.OrderBy_coalesce_skip_take_distinct();

            AssertSql(
                @":p_0='5'
:p_1='15'

SELECT DISTINCT ""t"".*
FROM (
    SELECT ""p"".""ProductID"", ""p"".""Discontinued"", ""p"".""ProductName"", ""p"".""SupplierID"", ""p"".""UnitPrice"", ""p"".""UnitsInStock""
    FROM ""Products"" ""p""
    ORDER BY COALESCE(""p"".""UnitPrice"", 0.0) NULLS FIRST
    OFFSET :p_0 ROWS FETCH NEXT :p_1 ROWS ONLY
) ""t""");
        }

        public override Task OrderBy_coalesce_skip_take_distinct_take()
        {
            // Disabled, Distinct no order by
            return Task.CompletedTask;
        }

        public override async Task No_orderby_added_for_fully_translated_manually_constructed_LOJ()
        {
            await base.No_orderby_added_for_fully_translated_manually_constructed_LOJ();

            AssertSql(
                @"SELECT ""e1"".""City"" ""City1"", ""e2"".""City"" ""City2""
FROM ""Employees"" ""e1""
LEFT JOIN ""Employees"" ""e2"" ON ""e1"".""EmployeeID"" = ""e2"".""ReportsTo""");
        }

        public override async Task No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ()
        {
            await base.No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ();

            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"" ""Id1"", ""o"".""EmployeeID"", ""o"".""OrderDate"", ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Orders"" ""o""
LEFT JOIN ""Customers"" ""c"" ON ""o"".""CustomerID"" = ""c"".""CustomerID""");
        }

        public override async Task No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition1()
        {
            await base.No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition1();

            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"" ""Id1"", ""o"".""EmployeeID"", ""o"".""OrderDate"", ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Orders"" ""o""
LEFT JOIN ""Customers"" ""c"" ON (""o"".""CustomerID"" = ""c"".""CustomerID"") AND (""o"".""OrderID"" = 10000)");
        }

        public override async Task No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition2()
        {
            await base.No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition2();

            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"" ""Id1"", ""o"".""EmployeeID"", ""o"".""OrderDate"", ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Orders"" ""o""
LEFT JOIN ""Customers"" ""c"" ON (""o"".""OrderID"" = 10000) AND (""o"".""CustomerID"" = ""c"".""CustomerID"")");
        }

        public override async Task Orderby_added_for_client_side_GroupJoin_principal_to_dependent_LOJ()
        {
            await base.Orderby_added_for_client_side_GroupJoin_principal_to_dependent_LOJ();

            AssertSql(
                @"SELECT ""e1"".""EmployeeID"", ""e1"".""City"" ""City1"", ""e1"".""Country"", ""e1"".""FirstName"", ""e1"".""ReportsTo"", ""e1"".""Title"", ""e2"".""EmployeeID"", ""e2"".""City"", ""e2"".""Country"", ""e2"".""FirstName"", ""e2"".""ReportsTo"", ""e2"".""Title""
FROM ""Employees"" ""e1""
LEFT JOIN ""Employees"" ""e2"" ON ""e1"".""EmployeeID"" = ""e2"".""ReportsTo""
ORDER BY ""e1"".""EmployeeID"" NULLS FIRST");
        }

        public override async Task Contains_with_subquery_involving_join_binds_to_correct_table()
        {
            await base.Contains_with_subquery_involving_join_binds_to_correct_table();

            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""
WHERE (""o"".""OrderID"" > 11000) AND ""o"".""OrderID"" IN (
    SELECT ""od"".""OrderID""
    FROM ""Order Details"" ""od""
    INNER JOIN ""Products"" ""od.Product"" ON ""od"".""ProductID"" = ""od.Product"".""ProductID""
    WHERE ""od.Product"".""ProductName"" = N'Chai'
)");
        }

        public override async Task Complex_query_with_repeated_query_model_compiles_correctly()
        {
            await base.Complex_query_with_repeated_query_model_compiles_correctly();

            AssertSql(
                @"SELECT ""outer"".""CustomerID"", ""outer"".""Address"", ""outer"".""City"", ""outer"".""CompanyName"", ""outer"".""ContactName"", ""outer"".""ContactTitle"", ""outer"".""Country"", ""outer"".""Fax"", ""outer"".""Phone"", ""outer"".""PostalCode"", ""outer"".""Region""
FROM ""Customers"" ""outer""
WHERE ""outer"".""CustomerID"" = N'ALFKI'",
                //
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM ""Customers"" ""c0""
        WHERE EXISTS (
            SELECT 1
            FROM ""Customers"" ""cc1""))
    THEN 1 ELSE 0
END FROM DUAL");
        }

        public override async Task Complex_query_with_repeated_nested_query_model_compiles_correctly()
        {
            await base.Complex_query_with_repeated_nested_query_model_compiles_correctly();

            AssertSql(
                @"SELECT ""outer"".""CustomerID"", ""outer"".""Address"", ""outer"".""City"", ""outer"".""CompanyName"", ""outer"".""ContactName"", ""outer"".""ContactTitle"", ""outer"".""Country"", ""outer"".""Fax"", ""outer"".""Phone"", ""outer"".""PostalCode"", ""outer"".""Region""
FROM ""Customers"" ""outer""
WHERE ""outer"".""CustomerID"" = N'ALFKI'",
                //
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM ""Customers"" ""c0""
        WHERE EXISTS (
            SELECT 1
            FROM ""Customers"" ""cc1""
            WHERE EXISTS (
                SELECT DISTINCT 1
                FROM (
                    SELECT ""inner1"".*
                    FROM ""Customers"" ""inner1""
                    ORDER BY ""inner1"".""CustomerID"" NULLS FIRST
                    FETCH FIRST 10 ROWS ONLY
                ) ""t1"")))
    THEN 1 ELSE 0
END FROM DUAL");
        }

        public override async Task Anonymous_member_distinct_where()
        {
            await base.Anonymous_member_distinct_where();

            AssertSql(
                @"SELECT ""t"".""CustomerID""
FROM (
    SELECT DISTINCT ""c"".""CustomerID""
    FROM ""Customers"" ""c""
) ""t""
WHERE ""t"".""CustomerID"" = N'ALFKI'");
        }

        public override async Task Anonymous_member_distinct_orderby()
        {
            await base.Anonymous_member_distinct_orderby();

            AssertSql(
                @"SELECT ""t"".""CustomerID""
FROM (
    SELECT DISTINCT ""c"".""CustomerID""
    FROM ""Customers"" ""c""
) ""t""
ORDER BY ""t"".""CustomerID"" NULLS FIRST");
        }

        public override void Anonymous_member_distinct_result()
        {
            base.Anonymous_member_distinct_result();

            AssertSql(
                @"SELECT COUNT(*)
FROM (
    SELECT DISTINCT ""c"".""CustomerID""
    FROM ""Customers"" ""c""
) ""t""
WHERE ""t"".""CustomerID"" LIKE N'A' || N'%' AND (SUBSTR(""t"".""CustomerID"", 1, LENGTH(N'A')) = N'A')");
        }

        public override async Task Anonymous_complex_distinct_orderby()
        {
            await base.Anonymous_complex_distinct_orderby();

            AssertSql(
                @"SELECT ""t"".""A""
FROM (
    SELECT DISTINCT ""c"".""CustomerID"" || ""c"".""City"" ""A""
    FROM ""Customers"" ""c""
) ""t""
ORDER BY ""t"".""A"" NULLS FIRST");
        }

        public override void Anonymous_complex_distinct_result()
        {
            base.Anonymous_complex_distinct_result();

            AssertSql(
                @"SELECT COUNT(*)
FROM (
    SELECT DISTINCT ""c"".""CustomerID"" || ""c"".""City"" ""A""
    FROM ""Customers"" ""c""
) ""t""
WHERE ""t"".""A"" LIKE N'A' || N'%' AND (SUBSTR(""t"".""A"", 1, LENGTH(N'A')) = N'A')");
        }

        public override async Task DTO_member_distinct_where()
        {
            await base.DTO_member_distinct_where();

            AssertSql(
                @"SELECT ""t"".""Property""
FROM (
    SELECT DISTINCT ""c"".""CustomerID"" ""Property""
    FROM ""Customers"" ""c""
) ""t""
WHERE ""t"".""Property"" = N'ALFKI'");
        }

        public override async Task DTO_member_distinct_orderby()
        {
            await base.DTO_member_distinct_orderby();

            AssertSql(
                @"SELECT ""t"".""Property""
FROM (
    SELECT DISTINCT ""c"".""CustomerID"" ""Property""
    FROM ""Customers"" ""c""
) ""t""
ORDER BY ""t"".""Property"" NULLS FIRST");
        }

        public override void DTO_member_distinct_result()
        {
            base.DTO_member_distinct_result();

            AssertSql(
                @"SELECT COUNT(*)
FROM (
    SELECT DISTINCT ""c"".""CustomerID"" ""Property""
    FROM ""Customers"" ""c""
) ""t""
WHERE ""t"".""Property"" LIKE N'A' || N'%' AND (SUBSTR(""t"".""Property"", 1, LENGTH(N'A')) = N'A')");
        }

        public override async Task DTO_complex_orderby()
        {
            await base.DTO_complex_orderby();

            AssertSql(
                @"SELECT ""c"".""CustomerID"" || ""c"".""City"" ""Property""
FROM ""Customers"" ""c""
ORDER BY ""Property"" NULLS FIRST");
        }

        public override async Task DTO_subquery_orderby()
        {
            await base.DTO_subquery_orderby();

            AssertSql(
                @"SELECT (
    SELECT ""o1"".""OrderDate""
    FROM ""Orders"" ""o1""
    WHERE ""c"".""CustomerID"" = ""o1"".""CustomerID""
    ORDER BY ""o1"".""OrderID"" DESC
    FETCH FIRST 1 ROWS ONLY
) ""Property""
FROM ""Customers"" ""c""
WHERE (
    SELECT COUNT(*)
    FROM ""Orders"" ""o""
    WHERE ""c"".""CustomerID"" = ""o"".""CustomerID""
) > 1
ORDER BY (
    SELECT ""o0"".""OrderDate""
    FROM ""Orders"" ""o0""
    WHERE ""c"".""CustomerID"" = ""o0"".""CustomerID""
    ORDER BY ""o0"".""OrderID"" DESC
    FETCH FIRST 1 ROWS ONLY
) NULLS FIRST");
        }

        public override async Task Include_with_orderby_skip_preserves_ordering()
        {
            await base.Include_with_orderby_skip_preserves_ordering();

            AssertSql(
                @":p_0='40'
:p_1='5'

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" ""c""
WHERE ""c"".""CustomerID"" NOT IN (N'VAFFE', N'DRACD')
ORDER BY ""c"".""City"" NULLS FIRST, ""c"".""CustomerID"" NULLS FIRST
OFFSET :p_0 ROWS FETCH NEXT :p_1 ROWS ONLY",
                //
                @":p_0='40'
:p_1='5'

SELECT ""c.Orders"".""OrderID"", ""c.Orders"".""CustomerID"", ""c.Orders"".""EmployeeID"", ""c.Orders"".""OrderDate""
FROM ""Orders"" ""c.Orders""
INNER JOIN (
    SELECT ""c0"".""CustomerID"", ""c0"".""City""
    FROM ""Customers"" ""c0""
    WHERE ""c0"".""CustomerID"" NOT IN (N'VAFFE', N'DRACD')
    ORDER BY ""c0"".""City"" NULLS FIRST, ""c0"".""CustomerID"" NULLS FIRST
    OFFSET :p_0 ROWS FETCH NEXT :p_1 ROWS ONLY
) ""t"" ON ""c.Orders"".""CustomerID"" = ""t"".""CustomerID""
ORDER BY ""t"".""City"" NULLS FIRST, ""t"".""CustomerID"" NULLS FIRST");
        }

        public override async Task Int16_parameter_can_be_used_for_int_column()
        {
            await base.Int16_parameter_can_be_used_for_int_column();

            AssertSql(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" ""o""
WHERE ""o"".""OrderID"" = 10300");
        }

        public override void Select_skip_count()
        {
            base.Select_skip_count();

            AssertSql(
                @":p_0='7'

SELECT COUNT(*)
FROM (
    SELECT ""c"".*
    FROM ""Customers"" ""c""
    OFFSET :p_0 ROWS
) ""t""");
        }

        public override void Select_orderBy_skip_count()
        {
            base.Select_orderBy_skip_count();

            AssertSql(
                @":p_0='7'

SELECT COUNT(*)
FROM (
    SELECT ""c"".*
    FROM ""Customers"" ""c""
    ORDER BY ""c"".""Country"" NULLS FIRST
    OFFSET :p_0 ROWS
) ""t""");
        }

        public override void Select_skip_long_count()
        {
            base.Select_skip_long_count();

            AssertSql(
                @":p_0='7'

SELECT COUNT(*)
FROM (
    SELECT ""c"".*
    FROM ""Customers"" ""c""
    OFFSET :p_0 ROWS
) ""t""");
        }

        public override void Select_orderBy_skip_long_count()
        {
            base.Select_orderBy_skip_long_count();

            AssertSql(
                @":p_0='7'

SELECT COUNT(*)
FROM (
    SELECT ""c"".*
    FROM ""Customers"" ""c""
    ORDER BY ""c"".""Country"" NULLS FIRST
    OFFSET :p_0 ROWS
) ""t""");
        }

        public override void Select_skip_max()
        {
            base.Select_skip_max();

            AssertSql(
                @":p_0='10'

SELECT MAX(""t"".""OrderID"")
FROM (
    SELECT ""o"".""OrderID""
    FROM ""Orders"" ""o""
    ORDER BY ""o"".""OrderID"" NULLS FIRST
    OFFSET :p_0 ROWS
) ""t""");
        }

        public override void Select_skip_min()
        {
            base.Select_skip_min();

            AssertSql(
                @":p_0='10'

SELECT MIN(""t"".""OrderID"")
FROM (
    SELECT ""o"".""OrderID""
    FROM ""Orders"" ""o""
    ORDER BY ""o"".""OrderID"" NULLS FIRST
    OFFSET :p_0 ROWS
) ""t""");
        }

        public override void Select_skip_sum()
        {
            base.Select_skip_sum();

            AssertSql(
                @":p_0='10'

SELECT SUM(""t"".""OrderID"")
FROM (
    SELECT ""o"".""OrderID""
    FROM ""Orders"" ""o""
    ORDER BY ""o"".""OrderID"" NULLS FIRST
    OFFSET :p_0 ROWS
) ""t""");
        }

        public override void Select_distinct_count()
        {
            base.Select_distinct_count();

            AssertSql(
                @"SELECT COUNT(*)
FROM (
    SELECT DISTINCT ""c"".*
    FROM ""Customers"" ""c""
) ""t""");
        }

        public override void Select_distinct_long_count()
        {
            base.Select_distinct_long_count();

            AssertSql(
                @"SELECT COUNT(*)
FROM (
    SELECT DISTINCT ""c"".*
    FROM ""Customers"" ""c""
) ""t""");
        }

        public override void Select_distinct_max()
        {
            base.Select_distinct_max();

            AssertSql(
                @"SELECT MAX(""t"".""OrderID"")
FROM (
    SELECT DISTINCT ""o"".""OrderID""
    FROM ""Orders"" ""o""
) ""t""");
        }

        public override void Select_distinct_min()
        {
            base.Select_distinct_min();

            AssertSql(
                @"SELECT MIN(""t"".""OrderID"")
FROM (
    SELECT DISTINCT ""o"".""OrderID""
    FROM ""Orders"" ""o""
) ""t""");
        }

        public override void Select_distinct_sum()
        {
            base.Select_distinct_sum();

            AssertSql(
                @"SELECT SUM(""t"".""OrderID"")
FROM (
    SELECT DISTINCT ""o"".""OrderID""
    FROM ""Orders"" ""o""
) ""t""");
        }

        public override async Task Comparing_to_fixed_string_parameter()
        {
            await base.Comparing_to_fixed_string_parameter();

            AssertSql(
                @":prefix_0='A' (Size = 5)

SELECT ""c"".""CustomerID""
FROM ""Customers"" ""c""
WHERE (""c"".""CustomerID"" LIKE :prefix_0 || N'%' AND (SUBSTR(""c"".""CustomerID"", 1, LENGTH(:prefix_0)) = :prefix_0)) OR (:prefix_0 = N'')");
        }

        public override async Task Comparing_entities_using_Equals()
        {
            await base.Comparing_entities_using_Equals();

            AssertSql(
                @"SELECT ""c1"".""CustomerID"" ""Id1"", ""c2"".""CustomerID"" ""Id2""
FROM ""Customers"" ""c1""
CROSS JOIN ""Customers"" ""c2""
WHERE (""c1"".""CustomerID"" LIKE N'ALFKI' || N'%' AND (SUBSTR(""c1"".""CustomerID"", 1, LENGTH(N'ALFKI')) = N'ALFKI')) AND (""c1"".""CustomerID"" = ""c2"".""CustomerID"")
ORDER BY ""Id1"" NULLS FIRST");
        }

        public override async Task Comparing_different_entity_types_using_Equals()
        {
            await base.Comparing_different_entity_types_using_Equals();

            AssertSql(
                @"SELECT ""c"".""CustomerID""
FROM ""Customers"" ""c""
CROSS JOIN ""Orders"" ""o""
WHERE 0 = 1");
        }

        public override async Task Comparing_entity_to_null_using_Equals()
        {
            await base.Comparing_entity_to_null_using_Equals();

            AssertSql(
                @"SELECT ""c"".""CustomerID""
FROM ""Customers"" ""c""
WHERE (""c"".""CustomerID"" LIKE N'A' || N'%' AND (SUBSTR(""c"".""CustomerID"", 1, LENGTH(N'A')) = N'A')) AND ""c"".""CustomerID"" IS NOT NULL
ORDER BY ""c"".""CustomerID"" NULLS FIRST");
        }

        public override async Task Comparing_navigations_using_Equals()
        {
            await base.Comparing_navigations_using_Equals();

            AssertSql(
                @"SELECT ""o1"".""OrderID"" ""Id1"", ""o2"".""OrderID"" ""Id2""
FROM ""Orders"" ""o1""
CROSS JOIN ""Orders"" ""o2""
WHERE (""o1"".""CustomerID"" LIKE N'A' || N'%' AND (SUBSTR(""o1"".""CustomerID"", 1, LENGTH(N'A')) = N'A')) AND ((""o1"".""CustomerID"" = ""o2"".""CustomerID"") OR (""o1"".""CustomerID"" IS NULL AND ""o2"".""CustomerID"" IS NULL))
ORDER BY ""Id1"" NULLS FIRST, ""Id2"" NULLS FIRST");
        }

        public override async Task Comparing_navigations_using_static_Equals()
        {
            await base.Comparing_navigations_using_static_Equals();

            AssertSql(
                @"SELECT ""o1"".""OrderID"" ""Id1"", ""o2"".""OrderID"" ""Id2""
FROM ""Orders"" ""o1""
CROSS JOIN ""Orders"" ""o2""
WHERE (""o1"".""CustomerID"" LIKE N'A' || N'%' AND (SUBSTR(""o1"".""CustomerID"", 1, LENGTH(N'A')) = N'A')) AND ((""o1"".""CustomerID"" = ""o2"".""CustomerID"") OR (""o1"".""CustomerID"" IS NULL AND ""o2"".""CustomerID"" IS NULL))
ORDER BY ""Id1"" NULLS FIRST, ""Id2"" NULLS FIRST");
        }

        public override async Task Comparing_non_matching_entities_using_Equals()
        {
            await base.Comparing_non_matching_entities_using_Equals();

            AssertSql(
                @"SELECT ""c"".""CustomerID"" ""Id1"", ""o"".""OrderID"" ""Id2""
FROM ""Customers"" ""c""
CROSS JOIN ""Orders"" ""o""
WHERE 0 = 1");
        }

        public override async Task Comparing_non_matching_collection_navigations_using_Equals()
        {
            await base.Comparing_non_matching_collection_navigations_using_Equals();

            AssertSql(
                @"SELECT ""c"".""CustomerID"" ""Id1"", ""o"".""OrderID"" ""Id2""
FROM ""Customers"" ""c""
CROSS JOIN ""Orders"" ""o""
WHERE 0 = 1");
        }

        public override async Task Comparing_collection_navigation_to_null()
        {
            await base.Comparing_collection_navigation_to_null();

            AssertSql(
                @"SELECT ""c"".""CustomerID""
FROM ""Customers"" ""c""
WHERE ""c"".""CustomerID"" IS NULL");
        }

        public override async Task Comparing_collection_navigation_to_null_complex()
        {
            await base.Comparing_collection_navigation_to_null_complex();

            AssertSql(
                @"SELECT ""od"".""ProductID"", ""od"".""OrderID""
FROM ""Order Details"" ""od""
INNER JOIN ""Orders"" ""od.Order"" ON ""od"".""OrderID"" = ""od.Order"".""OrderID""
WHERE (""od"".""OrderID"" < 10250) AND ""od.Order"".""CustomerID"" IS NOT NULL
ORDER BY ""od"".""OrderID"" NULLS FIRST, ""od"".""ProductID"" NULLS FIRST");
        }

        public override async Task Compare_collection_navigation_with_itself()
        {
            await base.Compare_collection_navigation_with_itself();

            AssertSql(
                @"SELECT ""c"".""CustomerID""
FROM ""Customers"" ""c""
WHERE (""c"".""CustomerID"" LIKE N'A' || N'%' AND (SUBSTR(""c"".""CustomerID"", 1, LENGTH(N'A')) = N'A')) AND (""c"".""CustomerID"" = ""c"".""CustomerID"")");
        }

        public override async Task Compare_two_collection_navigations_with_different_query_sources()
        {
            await base.Compare_two_collection_navigations_with_different_query_sources();

            AssertSql(
                @"SELECT ""c1"".""CustomerID"" ""Id1"", ""c2"".""CustomerID"" ""Id2""
FROM ""Customers"" ""c1""
CROSS JOIN ""Customers"" ""c2""
WHERE ((""c1"".""CustomerID"" = N'ALFKI') AND (""c2"".""CustomerID"" = N'ALFKI')) AND (""c1"".""CustomerID"" = ""c2"".""CustomerID"")");
        }

        public override async Task Compare_two_collection_navigations_using_equals()
        {
            await base.Compare_two_collection_navigations_using_equals();

            AssertSql(
                @"SELECT ""c1"".""CustomerID"" ""Id1"", ""c2"".""CustomerID"" ""Id2""
FROM ""Customers"" ""c1""
CROSS JOIN ""Customers"" ""c2""
WHERE ((""c1"".""CustomerID"" = N'ALFKI') AND (""c2"".""CustomerID"" = N'ALFKI')) AND (""c1"".""CustomerID"" = ""c2"".""CustomerID"")");
        }

        public override async Task Compare_two_collection_navigations_with_different_property_chains()
        {
            await base.Compare_two_collection_navigations_with_different_property_chains();

            AssertSql(
                @"SELECT ""c"".""CustomerID"" ""Id1"", ""o"".""OrderID"" ""Id2""
FROM ""Customers"" ""c""
CROSS JOIN ""Orders"" ""o""
WHERE (""c"".""CustomerID"" = N'ALFKI') AND (""c"".""CustomerID"" = ""o"".""CustomerID"")
ORDER BY ""Id1"" NULLS FIRST, ""Id2"" NULLS FIRST");
        }

        public override async Task OrderBy_ThenBy_same_column_different_direction()
        {
            await base.OrderBy_ThenBy_same_column_different_direction();

            AssertSql(
                @"SELECT ""c"".""CustomerID""
FROM ""Customers"" ""c""
WHERE ""c"".""CustomerID"" LIKE N'A' || N'%' AND (SUBSTR(""c"".""CustomerID"", 1, LENGTH(N'A')) = N'A')
ORDER BY ""c"".""CustomerID"" NULLS FIRST");
        }

        public override async Task OrderBy_OrderBy_same_column_different_direction()
        {
            await base.OrderBy_OrderBy_same_column_different_direction();

            AssertSql(
                @"SELECT ""c"".""CustomerID""
FROM ""Customers"" ""c""
WHERE ""c"".""CustomerID"" LIKE N'A' || N'%' AND (SUBSTR(""c"".""CustomerID"", 1, LENGTH(N'A')) = N'A')
ORDER BY ""c"".""CustomerID"" DESC");
        }

        [ConditionalFact(Skip = "See issue#10513")]
        public override Task OrderBy_empty_list_contains()
        {
            return base.OrderBy_empty_list_contains();
        }

        [ConditionalFact(Skip = "See issue#10513")]
        public override Task OrderBy_empty_list_does_not_contains()
        {
            return base.OrderBy_empty_list_does_not_contains();
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected.Select(s => s.Replace("\r\n", "\n")).ToArray());

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
