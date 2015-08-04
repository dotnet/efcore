// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind;
using Microsoft.Data.Entity.Storage;
using Xunit;

#if DNXCORE50
using System.Threading;
#endif

namespace Microsoft.Data.Entity.Sqlite.FunctionalTests
{
    public class QuerySqliteTest : QueryTestBase<NorthwindQuerySqliteFixture>
    {
        public override void Where_simple_closure()
        {
            base.Where_simple_closure();

            Assert.Equal(
                @"@__city_0: London

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""City"" = @__city_0",
                Sql);
        }

        public override void Where_simple_closure_constant()
        {
            base.Where_simple_closure_constant();

            Assert.Equal(
                @"@__predicate_0: True

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE @__predicate_0 = 1",
                Sql);
        }

        public override void Where_simple_closure_via_query_cache_nullable_type_reverse()
        {
            base.Where_simple_closure_via_query_cache_nullable_type_reverse();

            Assert.Equal(
                @"SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Employees"" AS ""e""
WHERE ""e"".""ReportsTo"" IS NULL

@__reportsTo_0: 5

SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Employees"" AS ""e""
WHERE ""e"".""ReportsTo"" = @__reportsTo_0

@__reportsTo_0: 2

SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Employees"" AS ""e""
WHERE ""e"".""ReportsTo"" = @__reportsTo_0",
                Sql);
        }

        public override void Where_simple_closure_via_query_cache_nullable_type()
        {
            base.Where_simple_closure_via_query_cache_nullable_type();

            Assert.Equal(
                @"@__reportsTo_0: 2

SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Employees"" AS ""e""
WHERE ""e"".""ReportsTo"" = @__reportsTo_0

@__reportsTo_0: 5

SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Employees"" AS ""e""
WHERE ""e"".""ReportsTo"" = @__reportsTo_0

SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Employees"" AS ""e""
WHERE ""e"".""ReportsTo"" IS NULL",
                Sql);
        }

        public override void Where_new_instance_field_access_closure_via_query_cache()
        {
            base.Where_new_instance_field_access_closure_via_query_cache();

            Assert.Equal(
                @"@__p_InstanceFieldValue_0: London

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""City"" = @__p_InstanceFieldValue_0

@__p_InstanceFieldValue_0: Seattle

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""City"" = @__p_InstanceFieldValue_0",
                Sql);
        }

        public override void Where_nested_property_access_closure_via_query_cache()
        {
            base.Where_nested_property_access_closure_via_query_cache();

            Assert.Equal(
                @"@__city_Nested_InstancePropertyValue_0: London

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""City"" = @__city_Nested_InstancePropertyValue_0

@__city_Nested_InstancePropertyValue_0: Seattle

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""City"" = @__city_Nested_InstancePropertyValue_0",
                Sql);
        }

        public override void Where_nested_field_access_closure_via_query_cache()
        {
            base.Where_nested_field_access_closure_via_query_cache();

            Assert.Equal(
                @"@__city_Nested_InstanceFieldValue_0: London

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""City"" = @__city_Nested_InstanceFieldValue_0

@__city_Nested_InstanceFieldValue_0: Seattle

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""City"" = @__city_Nested_InstanceFieldValue_0",
                Sql);
        }

        public override void Where_static_property_access_closure_via_query_cache()
        {
            base.Where_static_property_access_closure_via_query_cache();

            Assert.Equal(
                @"@__StaticPropertyValue_0: London

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""City"" = @__StaticPropertyValue_0

@__StaticPropertyValue_0: Seattle

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""City"" = @__StaticPropertyValue_0",
                Sql);
        }

        public override void Where_property_access_closure_via_query_cache()
        {
            base.Where_property_access_closure_via_query_cache();

            Assert.Equal(
                @"@__city_InstancePropertyValue_0: London

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""City"" = @__city_InstancePropertyValue_0

@__city_InstancePropertyValue_0: Seattle

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""City"" = @__city_InstancePropertyValue_0",
                Sql);
        }

        public override void Where_static_field_access_closure_via_query_cache()
        {
            base.Where_static_field_access_closure_via_query_cache();

            Assert.Equal(
                @"@__StaticFieldValue_0: London

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""City"" = @__StaticFieldValue_0

@__StaticFieldValue_0: Seattle

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""City"" = @__StaticFieldValue_0",
                Sql);
        }

        public override void Where_field_access_closure_via_query_cache()
        {
            base.Where_field_access_closure_via_query_cache();

            Assert.Equal(
                @"@__city_InstanceFieldValue_0: London

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""City"" = @__city_InstanceFieldValue_0

@__city_InstanceFieldValue_0: Seattle

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""City"" = @__city_InstanceFieldValue_0",
                Sql);
        }

        public override void Where_method_call_closure_via_query_cache()
        {
            base.Where_method_call_closure_via_query_cache();

            Assert.Equal(
                @"@__GetCity_0: London

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""City"" = @__GetCity_0

@__GetCity_0: Seattle

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""City"" = @__GetCity_0",
                Sql);
        }

        public override void Where_method_call_nullable_type_reverse_closure_via_query_cache()
        {
            base.Where_method_call_nullable_type_reverse_closure_via_query_cache();

            Assert.Equal(
                @"@__city_NullableInt_0: 1

SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Employees"" AS ""e""
WHERE ""e"".""EmployeeID"" > @__city_NullableInt_0

@__city_NullableInt_0: 5

SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Employees"" AS ""e""
WHERE ""e"".""EmployeeID"" > @__city_NullableInt_0",
                Sql);
        }

        public override void Where_method_call_nullable_type_closure_via_query_cache()
        {
            base.Where_method_call_nullable_type_closure_via_query_cache();

            Assert.Equal(
                @"@__city_Int_0: 2

SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Employees"" AS ""e""
WHERE ""e"".""ReportsTo"" = @__city_Int_0

@__city_Int_0: 5

SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Employees"" AS ""e""
WHERE ""e"".""ReportsTo"" = @__city_Int_0",
                Sql);
        }

        public override void Where_simple_closure_via_query_cache()
        {
            base.Where_simple_closure_via_query_cache();

            Assert.Equal(
                @"@__city_0: London

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""City"" = @__city_0

@__city_0: Seattle

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""City"" = @__city_0",
                Sql);
        }

        public override void Count_with_predicate()
        {
            base.Count_with_predicate();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM ""Orders"" AS ""o""
WHERE ""o"".""CustomerID"" = 'ALFKI'",
                Sql);
        }

        public override void Where_OrderBy_Count()
        {
            base.Where_OrderBy_Count();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM ""Orders"" AS ""o""
WHERE ""o"".""CustomerID"" = 'ALFKI'",
                Sql);
        }

        public override void OrderBy_Where_Count()
        {
            base.OrderBy_Where_Count();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM ""Orders"" AS ""o""
WHERE ""o"".""CustomerID"" = 'ALFKI'",
                Sql);
        }

        public override void OrderBy_Count_with_predicate()
        {
            base.OrderBy_Count_with_predicate();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM ""Orders"" AS ""o""
WHERE ""o"".""CustomerID"" = 'ALFKI'",
                Sql);
        }

        public override void OrderBy_Where_Count_with_predicate()
        {
            base.OrderBy_Where_Count_with_predicate();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM ""Orders"" AS ""o""
WHERE (""o"".""OrderID"" > 10 AND (""o"".""CustomerID"" <> 'ALFKI' OR ""o"".""CustomerID"" IS NULL))",
                Sql);
        }

        public override void Where_OrderBy_Count_client_eval()
        {
            base.Where_OrderBy_Count_client_eval();

            Assert.Equal(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""",
                Sql);
        }

        public override void Where_OrderBy_Count_client_eval_mixed()
        {
            base.Where_OrderBy_Count_client_eval_mixed();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM ""Orders"" AS ""o""
WHERE ""o"".""OrderID"" > 10",
                Sql);
        }

        public override void OrderBy_Where_Count_client_eval()
        {
            base.OrderBy_Where_Count_client_eval();

            Assert.Equal(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""",
                Sql);
        }

        public override void OrderBy_Where_Count_client_eval_mixed()
        {
            base.OrderBy_Where_Count_client_eval_mixed();

            Assert.Equal(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""",
                Sql);
        }

        public override void OrderBy_Count_with_predicate_client_eval()
        {
            base.OrderBy_Count_with_predicate_client_eval();

            Assert.Equal(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""",
                Sql);
        }

        public override void OrderBy_Count_with_predicate_client_eval_mixed()
        {
            base.OrderBy_Count_with_predicate_client_eval_mixed();

            Assert.Equal(
                @"@__ClientEvalPredicateStateless_0: True

SELECT COUNT(*)
FROM ""Orders"" AS ""o""
WHERE @__ClientEvalPredicateStateless_0 = 1",
                Sql);
        }

        public override void OrderBy_Where_Count_with_predicate_client_eval()
        {
            base.OrderBy_Where_Count_with_predicate_client_eval();

            Assert.Equal(
                @"@__ClientEvalPredicateStateless_1: True

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE @__ClientEvalPredicateStateless_1 = 1",
                Sql);
        }

        public override void OrderBy_Where_Count_with_predicate_client_eval_mixed()
        {
            base.OrderBy_Where_Count_with_predicate_client_eval_mixed();

            Assert.Equal(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE (""o"".""CustomerID"" <> 'ALFKI' OR ""o"".""CustomerID"" IS NULL)",
                Sql);
        }

        public override void Sum_with_no_arg()
        {
            base.Sum_with_no_arg();

            Assert.Equal(
                @"SELECT SUM(""o"".""OrderID"")
FROM ""Orders"" AS ""o""",
                Sql);
        }

        public override void Sum_with_arg()
        {
            base.Sum_with_arg();

            Assert.Equal(
                @"SELECT SUM(""o"".""OrderID"")
FROM ""Orders"" AS ""o""",
                Sql);
        }

        public override void Sum_with_arg_expression()
        {
            base.Sum_with_arg_expression();

            Assert.Equal(
                @"SELECT SUM(""o"".""OrderID"" + ""o"".""OrderID"")
FROM ""Orders"" AS ""o""",
                Sql);
        }

        public override void Sum_with_binary_expression()
        {
            base.Sum_with_binary_expression();

            Assert.Equal(
                @"SELECT SUM(""o"".""OrderID"" * 2)
FROM ""Orders"" AS ""o""",
                Sql);
        }

        public override void Min_with_no_arg()
        {
            base.Min_with_no_arg();

            Assert.Equal(
                @"SELECT MIN(""o"".""OrderID"")
FROM ""Orders"" AS ""o""",
                Sql);
        }

        public override void Min_with_arg()
        {
            base.Min_with_arg();

            Assert.Equal(
                @"SELECT MIN(""o"".""OrderID"")
FROM ""Orders"" AS ""o""",
                Sql);
        }

        public override void Max_with_no_arg()
        {
            base.Max_with_no_arg();

            Assert.Equal(
                @"SELECT MAX(""o"".""OrderID"")
FROM ""Orders"" AS ""o""",
                Sql);
        }

        public override void Max_with_arg()
        {
            base.Max_with_arg();

            Assert.Equal(
                @"SELECT MAX(""o"".""OrderID"")
FROM ""Orders"" AS ""o""",
                Sql);
        }

        public override void Distinct_Count()
        {
            base.Distinct_Count();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM (
    SELECT DISTINCT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
    FROM ""Customers"" AS ""c""
) AS ""t0""",
                Sql);
        }

        [Fact]
        public override void Select_Distinct_Count()
        {
            base.Select_Distinct_Count();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM (
    SELECT DISTINCT ""c"".""City""
    FROM ""Customers"" AS ""c""
) AS ""t0""",
                Sql);
        }

        public override void Skip()
        {
            base.Skip();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
ORDER BY ""c"".""CustomerID""
LIMIT -1 OFFSET 5",
                Sql);
        }

        public override void Skip_Take()
        {
            base.Skip_Take();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
ORDER BY ""c"".""ContactName""
LIMIT 10 OFFSET 5",
                Sql);
        }

        public override void Take_Skip()
        {
            base.Take_Skip();

            Assert.Equal(
                @"SELECT ""t0"".*
FROM (
    SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
    FROM ""Customers"" AS ""c""
    ORDER BY ""c"".""ContactName""
    LIMIT 10
) AS ""t0""
ORDER BY ""t0"".""ContactName""
LIMIT -1 OFFSET 5",
                Sql);
        }

        public override void Take_Skip_Distinct()
        {
            base.Take_Skip_Distinct();

            Assert.Equal(
                @"SELECT DISTINCT ""t1"".*
FROM (
    SELECT ""t0"".*
    FROM (
        SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
        FROM ""Customers"" AS ""c""
        ORDER BY ""c"".""ContactName""
        LIMIT 10
    ) AS ""t0""
    ORDER BY ""t0"".""ContactName""
    LIMIT -1 OFFSET 5
) AS ""t1""",
                Sql);
        }

        public void Skip_when_no_order_by()
        {
            Assert.Throws<Exception>(() => AssertQuery<Customer>(cs => cs.Skip(5).Take(10)));
        }

        public override void Take_Distinct_Count()
        {
            base.Take_Distinct_Count();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM (
    SELECT DISTINCT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
    FROM ""Orders"" AS ""o""
    LIMIT 5
) AS ""t0""",
                Sql);
        }

        public override void Queryable_simple()
        {
            base.Queryable_simple();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""",
                Sql);
        }

        public override void Queryable_simple_anonymous()
        {
            base.Queryable_simple_anonymous();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""",
                Sql);
        }

        public override void Queryable_nested_simple()
        {
            base.Queryable_nested_simple();

            Assert.Equal(
                @"SELECT ""c3"".""CustomerID"", ""c3"".""Address"", ""c3"".""City"", ""c3"".""CompanyName"", ""c3"".""ContactName"", ""c3"".""ContactTitle"", ""c3"".""Country"", ""c3"".""Fax"", ""c3"".""Phone"", ""c3"".""PostalCode"", ""c3"".""Region""
FROM ""Customers"" AS ""c3""",
                Sql);
        }

        public override void Take_simple()
        {
            base.Take_simple();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
ORDER BY ""c"".""CustomerID""
LIMIT 10",
                Sql);
        }

        public override void Take_simple_projection()
        {
            base.Take_simple_projection();

            Assert.Equal(
                @"SELECT ""c"".""City""
FROM ""Customers"" AS ""c""
ORDER BY ""c"".""CustomerID""
LIMIT 10",
                Sql);
        }

        public override void OrderBy_Take_Count()
        {
            base.OrderBy_Take_Count();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM (
    SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
    FROM ""Orders"" AS ""o""
    ORDER BY ""o"".""OrderID""
    LIMIT 5
) AS ""t0""",
                Sql);
        }

        public override void Take_OrderBy_Count()
        {
            base.Take_OrderBy_Count();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM (
    SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
    FROM ""Orders"" AS ""o""
    LIMIT 5
) AS ""t0""",
                Sql);
        }

        public override void Any_simple()
        {
            base.Any_simple();

            Assert.Equal(
                @"SELECT CASE
    WHEN
        (EXISTS (
            SELECT 1
            FROM ""Customers"" AS ""c"")
        )
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                Sql);
        }

        public override void Any_predicate()
        {
            base.Any_predicate();

            Assert.Equal(
                @"SELECT CASE
    WHEN
        (EXISTS (
            SELECT 1
            FROM ""Customers"" AS ""c""
            WHERE ""c"".""ContactName"" LIKE 'A' || '%')
        )
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                Sql);
        }

        public override void All_top_level()
        {
            base.All_top_level();

            Assert.Equal(
                @"SELECT CASE
    WHEN
        (NOT (EXISTS (
            SELECT 1
            FROM ""Customers"" AS ""c""
            WHERE NOT (""c"".""ContactName"" LIKE 'A' || '%'))
        ))
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                Sql);
        }

        public override void Select_scalar()
        {
            base.Select_scalar();

            Assert.Equal(
                @"SELECT ""c"".""City""
FROM ""Customers"" AS ""c""",
                Sql);
        }

        public override void Select_scalar_primitive_after_take()
        {
            base.Select_scalar_primitive_after_take();

            Assert.Equal(
                @"SELECT ""t0"".""EmployeeID""
FROM (
    SELECT ""e"".*
    FROM ""Employees"" AS ""e""
    LIMIT 9
) AS ""t0""",
                Sql);
        }

        public override void Select_constant_null_string()
        {
            base.Select_constant_null_string();

            Assert.Equal(
                @"SELECT 1
FROM ""Customers"" AS ""c""",
                Sql);
        }

        public override void Select_local()
        {
            base.Select_local();

            Assert.Equal(
                @"@__x_0: 10

SELECT @__x_0
FROM ""Customers"" AS ""c""",
                Sql);
        }

        public override void Where_simple()
        {
            base.Where_simple();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""City"" = 'London'",
                Sql);
        }

        public override void Where_simple_shadow()
        {
            base.Where_simple_shadow();

            Assert.Equal(
                @"SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Employees"" AS ""e""
WHERE ""e"".""Title"" = 'Sales Representative'",
                Sql);
        }

        public override void Where_simple_shadow_projection()
        {
            base.Where_simple_shadow_projection();

            Assert.Equal(
                @"SELECT ""e"".""Title""
FROM ""Employees"" AS ""e""
WHERE ""e"".""Title"" = 'Sales Representative'",
                Sql);
        }

        public override void Where_comparison_nullable_type_not_null()
        {
            base.Where_comparison_nullable_type_not_null();

            Assert.Equal(
                @"SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Employees"" AS ""e""
WHERE ""e"".""ReportsTo"" = 2",
                Sql);
        }

        public override void Where_comparison_nullable_type_null()
        {
            base.Where_comparison_nullable_type_null();

            Assert.Equal(
                @"SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Employees"" AS ""e""
WHERE ""e"".""ReportsTo"" IS NULL",
                Sql);
        }

        public override void Where_client()
        {
            base.Where_client();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""",
                Sql);
        }

        public override void Where_client_and_server_top_level()
        {
            base.Where_client_and_server_top_level();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CustomerID"" <> 'AROUT'",
                Sql);
        }

        public override void Where_client_or_server_top_level()
        {
            base.Where_client_or_server_top_level();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""",
                Sql);
        }

        public override void Where_client_and_server_non_top_level()
        {
            base.Where_client_and_server_non_top_level();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""",
                Sql);
        }

        public override void Where_client_deep_inside_predicate_and_server_top_level()
        {
            base.Where_client_deep_inside_predicate_and_server_top_level();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CustomerID"" <> 'ALFKI'",
                Sql);
        }

        public override void First_client_predicate()
        {
            base.First_client_predicate();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
ORDER BY ""c"".""CustomerID""",
                Sql);
        }

        public override void Last()
        {
            base.Last();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
ORDER BY ""c"".""ContactName"" DESC
LIMIT 1",
                Sql);
        }

        public override void Last_when_no_order_by()
        {
            base.Last_when_no_order_by();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CustomerID"" = 'ALFKI'",
                Sql);
        }

        public override void Last_Predicate()
        {
            base.Last_Predicate();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""City"" = 'London'
ORDER BY ""c"".""ContactName"" DESC
LIMIT 1",
                Sql);
        }

        public override void Where_Last()
        {
            base.Where_Last();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""City"" = 'London'
ORDER BY ""c"".""ContactName"" DESC
LIMIT 1",
                Sql);
        }

        public override void LastOrDefault()
        {
            base.LastOrDefault();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
ORDER BY ""c"".""ContactName"" DESC
LIMIT 1",
                Sql);
        }

        public override void LastOrDefault_Predicate()
        {
            base.LastOrDefault_Predicate();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""City"" = 'London'
ORDER BY ""c"".""ContactName"" DESC
LIMIT 1",
                Sql);
        }

        public override void Where_LastOrDefault()
        {
            base.Where_LastOrDefault();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""City"" = 'London'
ORDER BY ""c"".""ContactName"" DESC
LIMIT 1",
                Sql);
        }

        public override void Where_equals_method_string()
        {
            base.Where_equals_method_string();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""City"" = 'London'",
                Sql);
        }

        public override void Where_equals_method_int()
        {
            base.Where_equals_method_int();

            Assert.Equal(
                @"SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Employees"" AS ""e""
WHERE ""e"".""EmployeeID"" = 1",
                Sql);
        }

        public override void Where_equals_using_object_overload_on_mismatched_types()
        {
            base.Where_equals_using_object_overload_on_mismatched_types();

            Assert.Equal(
                @"SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Employees"" AS ""e""
WHERE 1 = 0",
                Sql);

            Assert.True(TestSqlLoggerFactory.Log.Contains(
                "Possible unintended use of method Equals(object) for arguments of different types: 'e.EmployeeID', '__longPrm_0'. This comparison will always return 'false'."));
        }

        public override void Where_equals_using_int_overload_on_mismatched_types()
        {
            base.Where_equals_using_int_overload_on_mismatched_types();

            Assert.Equal(
                @"@__p_0: 1

SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Employees"" AS ""e""
WHERE ""e"".""EmployeeID"" = @__p_0",
                Sql);
        }

        public override void Where_equals_on_mismatched_types_int_nullable_int()
        {
            base.Where_equals_on_mismatched_types_int_nullable_int();

            Assert.Equal(
                @"@__intPrm_0: 2

SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Employees"" AS ""e""
WHERE ""e"".""ReportsTo"" = @__intPrm_0

@__intPrm_0: 2

SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Employees"" AS ""e""
WHERE @__intPrm_0 = ""e"".""ReportsTo""",
                Sql);
        }

        public override void Where_equals_on_mismatched_types_nullable_int_long()
        {
            base.Where_equals_on_mismatched_types_nullable_int_long();

            Assert.Equal(
                @"SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Employees"" AS ""e""
WHERE 1 = 0

SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Employees"" AS ""e""
WHERE 1 = 0",
                Sql);

            Assert.True(TestSqlLoggerFactory.Log.Contains(
                "Possible unintended use of method Equals(object) for arguments of different types: 'e.ReportsTo', '__longPrm_0'. This comparison will always return 'false'."));

            Assert.True(TestSqlLoggerFactory.Log.Contains(
                "Possible unintended use of method Equals(object) for arguments of different types: '__longPrm_0', 'e.ReportsTo'. This comparison will always return 'false'."));
        }

        public override void Where_equals_on_mismatched_types_nullable_long_nullable_int()
        {
            base.Where_equals_on_mismatched_types_nullable_long_nullable_int();

            Assert.Equal(
                @"SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Employees"" AS ""e""
WHERE 1 = 0

SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Employees"" AS ""e""
WHERE 1 = 0",
                Sql);

            Assert.True(TestSqlLoggerFactory.Log.Contains(
                "Possible unintended use of method Equals(object) for arguments of different types: '__nullableLongPrm_0', 'e.ReportsTo'. This comparison will always return 'false'."));

            Assert.True(TestSqlLoggerFactory.Log.Contains(
                "Possible unintended use of method Equals(object) for arguments of different types: 'e.ReportsTo', '__nullableLongPrm_0'. This comparison will always return 'false'."));
        }

        public override void Where_equals_on_matched_nullable_int_types()
        {
            base.Where_equals_on_matched_nullable_int_types();

            Assert.Equal(
                @"@__nullableIntPrm_0: 2

SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Employees"" AS ""e""
WHERE @__nullableIntPrm_0 = ""e"".""ReportsTo""

@__nullableIntPrm_0: 2

SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Employees"" AS ""e""
WHERE ""e"".""ReportsTo"" = @__nullableIntPrm_0",
                Sql);
        }

        public override void Where_string_length()
        {
            base.Where_string_length();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (length(""c"".""City"") = 6)",
                Sql);
        }

        public override void Where_is_null()
        {
            base.Where_is_null();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""City"" IS NULL",
                Sql);
        }

        public override void Where_is_not_null()
        {
            base.Where_is_not_null();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""City"" IS NOT NULL",
                Sql);
        }

        public override void Where_null_is_null()
        {
            base.Where_null_is_null();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE 1 = 1",
                Sql);
        }

        public override void Where_constant_is_null()
        {
            base.Where_constant_is_null();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE 1 = 0",
                Sql);
        }

        public override void Where_null_is_not_null()
        {
            base.Where_null_is_not_null();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE 1 = 0",
                Sql);
        }

        public override void Where_constant_is_not_null()
        {
            base.Where_constant_is_not_null();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE 1 = 1",
                Sql);
        }

        public override void Where_simple_reversed()
        {
            base.Where_simple_reversed();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE 'London' = ""c"".""City""",
                Sql);
        }

        public override void Where_identity_comparison()
        {
            base.Where_identity_comparison();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (""c"".""City"" = ""c"".""City"" OR (""c"".""City"" IS NULL AND ""c"".""City"" IS NULL))",
                Sql);
        }

        public override void Where_select_many_or()
        {
            base.Where_select_many_or();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Customers"" AS ""c""
CROSS JOIN ""Employees"" AS ""e""
WHERE (""c"".""City"" = 'London' OR ""e"".""City"" = 'London')",
                Sql);
        }

        public override void Where_select_many_or2()
        {
            base.Where_select_many_or2();

            Assert.StartsWith(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Customers"" AS ""c""
CROSS JOIN ""Employees"" AS ""e""
WHERE ""c"".""City"" IN ('London', 'Berlin')",
                Sql);
        }

        public override void Where_select_many_or3()
        {
            base.Where_select_many_or3();

            Assert.StartsWith(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Customers"" AS ""c""
CROSS JOIN ""Employees"" AS ""e""
WHERE ""c"".""City"" IN ('London', 'Berlin', 'Seattle')",
                Sql);
        }

        public override void Where_select_many_or4()
        {
            base.Where_select_many_or4();

            Assert.StartsWith(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Customers"" AS ""c""
CROSS JOIN ""Employees"" AS ""e""
WHERE ""c"".""City"" IN ('London', 'Berlin', 'Seattle', 'Lisboa')",
                Sql);
        }

        public override void Where_select_many_or_with_parameter()
        {
            base.Where_select_many_or_with_parameter();

            Assert.StartsWith(
                @"@__london_0: London
@__lisboa_1: Lisboa

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Customers"" AS ""c""
CROSS JOIN ""Employees"" AS ""e""
WHERE ""c"".""City"" IN (@__london_0, 'Berlin', 'Seattle', @__lisboa_1)",
                Sql);
        }

        public override void Where_in_optimization_multiple()
        {
            base.Where_in_optimization_multiple();

            Assert.StartsWith(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Customers"" AS ""c""
CROSS JOIN ""Employees"" AS ""e""
WHERE ((""c"".""City"" IN ('London', 'Berlin') OR ""c"".""CustomerID"" = 'ALFKI') OR ""c"".""CustomerID"" = 'ABCDE')",
                Sql);
        }

        public override void Where_not_in_optimization1()
        {
            base.Where_not_in_optimization1();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Customers"" AS ""c""
CROSS JOIN ""Employees"" AS ""e""
WHERE ((""c"".""City"" <> 'London' OR ""c"".""City"" IS NULL) AND (""e"".""City"" <> 'London' OR ""e"".""City"" IS NULL))",
                Sql);
        }

        public override void Where_not_in_optimization2()
        {
            base.Where_not_in_optimization2();

            Assert.StartsWith(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Customers"" AS ""c""
CROSS JOIN ""Employees"" AS ""e""
WHERE ""c"".""City"" NOT IN ('London', 'Berlin')",
                Sql);
        }

        public override void Where_not_in_optimization3()
        {
            base.Where_not_in_optimization3();

            Assert.StartsWith(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Customers"" AS ""c""
CROSS JOIN ""Employees"" AS ""e""
WHERE ""c"".""City"" NOT IN ('London', 'Berlin', 'Seattle')",
                Sql);
        }

        public override void Where_not_in_optimization4()
        {
            base.Where_not_in_optimization4();

            Assert.StartsWith(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Customers"" AS ""c""
CROSS JOIN ""Employees"" AS ""e""
WHERE ""c"".""City"" NOT IN ('London', 'Berlin', 'Seattle', 'Lisboa')",
                Sql);
        }

        public override void Where_select_many_and()
        {
            base.Where_select_many_and();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Customers"" AS ""c""
CROSS JOIN ""Employees"" AS ""e""
WHERE ((""c"".""City"" = 'London' AND ""c"".""Country"" = 'UK') AND (""e"".""City"" = 'London' AND ""e"".""Country"" = 'UK'))",
                Sql);
        }

        public override void Select_project_filter()
        {
            base.Select_project_filter();

            Assert.Equal(
                @"SELECT ""c"".""CompanyName""
FROM ""Customers"" AS ""c""
WHERE ""c"".""City"" = 'London'",
                Sql);
        }

        public override void Select_project_filter2()
        {
            base.Select_project_filter2();

            Assert.Equal(
                @"SELECT ""c"".""City""
FROM ""Customers"" AS ""c""
WHERE ""c"".""City"" = 'London'",
                Sql);
        }

        public override void SelectMany_mixed()
        {
            base.SelectMany_mixed();

            Assert.Equal(3873, Sql.Length);
            Assert.StartsWith(
                @"SELECT ""e1"".""EmployeeID"", ""e1"".""City"", ""e1"".""Country"", ""e1"".""FirstName"", ""e1"".""ReportsTo"", ""e1"".""Title""
FROM ""Employees"" AS ""e1""

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""",
                Sql);
        }

        public override void SelectMany_simple1()
        {
            base.SelectMany_simple1();

            Assert.Equal(
                @"SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title"", ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Employees"" AS ""e""
CROSS JOIN ""Customers"" AS ""c""",
                Sql);
        }

        public override void SelectMany_simple2()
        {
            base.SelectMany_simple2();

            Assert.Equal(
                @"SELECT ""e1"".""EmployeeID"", ""e1"".""City"", ""e1"".""Country"", ""e1"".""FirstName"", ""e1"".""ReportsTo"", ""e1"".""Title"", ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", ""e2"".""FirstName""
FROM ""Employees"" AS ""e1""
CROSS JOIN ""Customers"" AS ""c""
CROSS JOIN ""Employees"" AS ""e2""",
                Sql);
        }

        public override void SelectMany_entity_deep()
        {
            base.SelectMany_entity_deep();

            Assert.Equal(
                @"SELECT ""e1"".""EmployeeID"", ""e1"".""City"", ""e1"".""Country"", ""e1"".""FirstName"", ""e1"".""ReportsTo"", ""e1"".""Title"", ""e2"".""EmployeeID"", ""e2"".""City"", ""e2"".""Country"", ""e2"".""FirstName"", ""e2"".""ReportsTo"", ""e2"".""Title"", ""e3"".""EmployeeID"", ""e3"".""City"", ""e3"".""Country"", ""e3"".""FirstName"", ""e3"".""ReportsTo"", ""e3"".""Title""
FROM ""Employees"" AS ""e1""
CROSS JOIN ""Employees"" AS ""e2""
CROSS JOIN ""Employees"" AS ""e3""",
                Sql);
        }

        public override void SelectMany_Joined_Take()
        {
            base.SelectMany_Joined_Take();

            Assert.StartsWith(
                @"SELECT ""c"".""CustomerID"", ""c"".""ContactName""
FROM ""Customers"" AS ""c""

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""",
                Sql);
        }

        public override void SelectMany_projection1()
        {
            base.SelectMany_projection1();

            Assert.Equal(
                @"SELECT ""e1"".""City"", ""e2"".""Country""
FROM ""Employees"" AS ""e1""
CROSS JOIN ""Employees"" AS ""e2""",
                Sql);
        }

        public override void SelectMany_projection2()
        {
            base.SelectMany_projection2();

            Assert.Equal(
                @"SELECT ""e1"".""City"", ""e2"".""Country"", ""e3"".""FirstName""
FROM ""Employees"" AS ""e1""
CROSS JOIN ""Employees"" AS ""e2""
CROSS JOIN ""Employees"" AS ""e3""",
                Sql);
        }

        public override void SelectMany_Count()
        {
            base.SelectMany_Count();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM ""Customers"" AS ""c""
CROSS JOIN ""Orders"" AS ""o""", Sql);
        }

        public override void SelectMany_LongCount()
        {
            base.SelectMany_LongCount();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM ""Customers"" AS ""c""
CROSS JOIN ""Orders"" AS ""o""", Sql);
        }

        public override void SelectMany_OrderBy_ThenBy_Any()
        {
            base.SelectMany_OrderBy_ThenBy_Any();

            Assert.Equal(
                @"SELECT CASE
    WHEN
        (EXISTS (
            SELECT 1
            FROM ""Customers"" AS ""c""
            CROSS JOIN ""Orders"" AS ""o"")
        )
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END", Sql);
        }

        public override void Join_customers_orders_projection()
        {
            base.Join_customers_orders_projection();

            Assert.Equal(
                @"SELECT ""c"".""ContactName"", ""o"".""OrderID""
FROM ""Customers"" AS ""c""
INNER JOIN ""Orders"" AS ""o"" ON ""c"".""CustomerID"" = ""o"".""CustomerID""",
                Sql);
        }

        public override void Join_customers_orders_entities()
        {
            base.Join_customers_orders_entities();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Customers"" AS ""c""
INNER JOIN ""Orders"" AS ""o"" ON ""c"".""CustomerID"" = ""o"".""CustomerID""",
                Sql);
        }

        public override void Join_composite_key()
        {
            base.Join_composite_key();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Customers"" AS ""c""
INNER JOIN ""Orders"" AS ""o"" ON (""c"".""CustomerID"" = ""o"".""CustomerID"" AND ""c"".""CustomerID"" = ""o"".""CustomerID"")",
                Sql);
        }

        public override void Join_client_new_expression()
        {
            base.Join_client_new_expression();

            Assert.Equal(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""",
                Sql);
        }

        public override void Join_select_many()
        {
            base.Join_select_many();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate"", ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Customers"" AS ""c""
INNER JOIN ""Orders"" AS ""o"" ON ""c"".""CustomerID"" = ""o"".""CustomerID""
CROSS JOIN ""Employees"" AS ""e""",
                Sql);
        }

        public override void Join_Where_Count()
        {
            base.Join_Where_Count();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM ""Customers"" AS ""c""
INNER JOIN ""Orders"" AS ""o"" ON ""c"".""CustomerID"" = ""o"".""CustomerID""
WHERE ""c"".""CustomerID"" = 'ALFKI'", Sql);
        }

        public override void Join_OrderBy_Count()
        {
            base.Join_OrderBy_Count();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM ""Customers"" AS ""c""
INNER JOIN ""Orders"" AS ""o"" ON ""c"".""CustomerID"" = ""o"".""CustomerID""", Sql);
        }

        public override void Multiple_joins_Where_Order_Any()
        {
            base.Multiple_joins_Where_Order_Any();

            Assert.Equal(
                @"SELECT CASE
    WHEN
        (EXISTS (
            SELECT 1
            FROM ""Customers"" AS ""c""
            INNER JOIN ""Orders"" AS ""or"" ON ""c"".""CustomerID"" = ""or"".""CustomerID""
            INNER JOIN ""Order Details"" AS ""od"" ON ""or"".""OrderID"" = ""od"".""OrderID""
            WHERE ""c"".""City"" = 'London')
        )
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END", Sql);
        }

        public override void GroupBy_Distinct()
        {
            base.GroupBy_Distinct();

            Assert.Equal(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""",
                Sql);
        }

        public override void GroupBy_Count()
        {
            base.GroupBy_Count();

            Assert.Equal(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""",
                Sql);
        }

        public override void GroupBy_LongCount()
        {
            base.GroupBy_LongCount();

            Assert.Equal(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""",
                Sql);
        }

        public override void SelectMany_cartesian_product_with_ordering()
        {
            base.SelectMany_cartesian_product_with_ordering();

            Assert.StartsWith(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", ""e"".""City""
FROM ""Customers"" AS ""c""
CROSS JOIN ""Employees"" AS ""e""
WHERE (""c"".""City"" = ""e"".""City"" OR (""c"".""City"" IS NULL AND ""e"".""City"" IS NULL))
ORDER BY ""e"".""City"", ""c"".""CustomerID"" DESC",
                Sql);
        }

        public override void GroupJoin_customers_orders_count()
        {
            base.GroupJoin_customers_orders_count();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Customers"" AS ""c""
LEFT JOIN ""Orders"" AS ""o"" ON ""c"".""CustomerID"" = ""o"".""CustomerID""",
                Sql);
        }

        public override void Take_with_single()
        {
            base.Take_with_single();

            Assert.Equal(
                @"SELECT ""t0"".*
FROM (
    SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
    FROM ""Customers"" AS ""c""
    ORDER BY ""c"".""CustomerID""
    LIMIT 1
) AS ""t0""
LIMIT 2",
                Sql);
        }

        public override void Take_with_single_select_many()
        {
            base.Take_with_single_select_many();

            Assert.Equal(
                @"SELECT ""t0"".*
FROM (
    SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region"", ""o"".""OrderID"", ""o"".""CustomerID"" AS ""c0"", ""o"".""EmployeeID"", ""o"".""OrderDate""
    FROM ""Customers"" AS ""c""
    CROSS JOIN ""Orders"" AS ""o""
    ORDER BY ""c"".""CustomerID"", ""o"".""OrderID""
    LIMIT 1
) AS ""t0""
LIMIT 2",
                Sql);
        }

        public override void Distinct()
        {
            base.Distinct();

            Assert.Equal(
                @"SELECT DISTINCT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""",
                Sql);
        }

        public override void Distinct_Scalar()
        {
            base.Distinct_Scalar();

            Assert.Equal(
                @"SELECT DISTINCT ""c"".""City""
FROM ""Customers"" AS ""c""",
                Sql);
        }

        public override void OrderBy_client_mixed()
        {
            base.OrderBy_client_mixed();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""",
                Sql);
        }

        public override void OrderBy_multiple_queries()
        {
            base.OrderBy_multiple_queries();

            Assert.Equal(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""",
                Sql);
        }

        public override void OrderBy_Distinct()
        {
            base.OrderBy_Distinct();

            Assert.Equal(
                @"SELECT DISTINCT ""c"".""City""
FROM ""Customers"" AS ""c""", // Ordering not preserved by distinct
                Sql);
        }

        public override void Distinct_OrderBy()
        {
            base.Distinct_OrderBy();

            Assert.Equal(
                @"SELECT ""t0"".""Country""
FROM (
    SELECT DISTINCT ""c"".""Country""
    FROM ""Customers"" AS ""c""
) AS ""t0""
ORDER BY ""t0"".""Country""",
                Sql);
        }

        public override void OrderBy_shadow()
        {
            base.OrderBy_shadow();

            Assert.Equal(
                @"SELECT ""e"".""EmployeeID"", ""e"".""City"", ""e"".""Country"", ""e"".""FirstName"", ""e"".""ReportsTo"", ""e"".""Title""
FROM ""Employees"" AS ""e""
ORDER BY ""e"".""Title"", ""e"".""EmployeeID""",
                Sql);
        }

        public override void OrderBy_multiple()
        {
            base.OrderBy_multiple();

            Assert.Equal(
                @"SELECT ""c"".""City""
FROM ""Customers"" AS ""c""
ORDER BY ""c"".""Country"", ""c"".""CustomerID""",
                Sql);
        }

        public override void OrderBy_ThenBy_Any()
        {
            base.OrderBy_ThenBy_Any();

            Assert.Equal(
                @"SELECT CASE
    WHEN
        (EXISTS (
            SELECT 1
            FROM ""Customers"" AS ""c"")
        )
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                Sql);
        }

        public override void Where_subquery_recursive_trivial()
        {
            base.Where_subquery_recursive_trivial();

            Assert.Equal(
                @"SELECT ""e1"".""EmployeeID"", ""e1"".""City"", ""e1"".""Country"", ""e1"".""FirstName"", ""e1"".""ReportsTo"", ""e1"".""Title""
FROM ""Employees"" AS ""e1""
WHERE (
    SELECT CASE
        WHEN
            (EXISTS (
                SELECT 1
                FROM ""Employees"" AS ""e2""
                WHERE (
                    SELECT CASE
                        WHEN
                            (EXISTS (
                                SELECT 1
                                FROM ""Employees"" AS ""e3"")
                            )
                        THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
                    END
                ) = 1)
            )
        THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
    END
) = 1
ORDER BY ""e1"".""EmployeeID""",
                Sql);
        }

        public override void Where_false()
        {
            base.Where_false();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE 1 = 0",
                Sql);
        }

        public override void Where_primitive()
        {
            base.Where_primitive();

            Assert.Equal(
                @"SELECT ""t0"".""EmployeeID""
FROM (
    SELECT ""e"".""EmployeeID""
    FROM ""Employees"" AS ""e""
    LIMIT 9
) AS ""t0""
WHERE ""t0"".""EmployeeID"" = 5",
                Sql);
        }

        public override void Where_bool_member()
        {
            base.Where_bool_member();

            Assert.Equal(
                @"SELECT ""p"".""ProductID"", ""p"".""Discontinued"", ""p"".""ProductName"", ""p"".""UnitsInStock""
FROM ""Products"" AS ""p""
WHERE ""p"".""Discontinued"" = 1",
                Sql);
        }

        public override void Where_bool_member_false()
        {
            base.Where_bool_member_false();

            Assert.Equal(
                @"SELECT ""p"".""ProductID"", ""p"".""Discontinued"", ""p"".""ProductName"", ""p"".""UnitsInStock""
FROM ""Products"" AS ""p""
WHERE ""p"".""Discontinued"" = 0",
                Sql);
        }

        public override void Where_bool_member_negated_twice()
        {
            base.Where_bool_member_negated_twice();

            Assert.Equal(
                @"SELECT ""p"".""ProductID"", ""p"".""Discontinued"", ""p"".""ProductName"", ""p"".""UnitsInStock""
FROM ""Products"" AS ""p""
WHERE ""p"".""Discontinued"" = 1",
                Sql);
        }

        public override void Where_bool_member_shadow()
        {
            base.Where_bool_member_shadow();

            Assert.Equal(
                @"SELECT ""p"".""ProductID"", ""p"".""Discontinued"", ""p"".""ProductName"", ""p"".""UnitsInStock""
FROM ""Products"" AS ""p""
WHERE ""p"".""Discontinued"" = 1",
                Sql);
        }

        public override void Where_bool_member_false_shadow()
        {
            base.Where_bool_member_false_shadow();

            Assert.Equal(
                @"SELECT ""p"".""ProductID"", ""p"".""Discontinued"", ""p"".""ProductName"", ""p"".""UnitsInStock""
FROM ""Products"" AS ""p""
WHERE ""p"".""Discontinued"" = 0",
                Sql);
        }

        public override void Where_bool_member_equals_constant()
        {
            base.Where_bool_member_equals_constant();

            Assert.Equal(
                @"SELECT ""p"".""ProductID"", ""p"".""Discontinued"", ""p"".""ProductName"", ""p"".""UnitsInStock""
FROM ""Products"" AS ""p""
WHERE ""p"".""Discontinued"" = 1",
                Sql);
        }

        public override void Where_bool_member_in_complex_predicate()
        {
            base.Where_bool_member_in_complex_predicate();

            Assert.Equal(
                @"SELECT ""p"".""ProductID"", ""p"".""Discontinued"", ""p"".""ProductName"", ""p"".""UnitsInStock""
FROM ""Products"" AS ""p""
WHERE ((""p"".""ProductID"" > 100 AND ""p"".""Discontinued"" = 1) OR ""p"".""Discontinued"" = 1)",
                Sql);
        }

        public override void Where_bool_member_compared_to_binary_expression()
        {
            base.Where_bool_member_compared_to_binary_expression();

            Assert.Equal(
                @"SELECT ""p"".""ProductID"", ""p"".""Discontinued"", ""p"".""ProductName"", ""p"".""UnitsInStock""
FROM ""Products"" AS ""p""
WHERE (""p"".""Discontinued"" = CASE
    WHEN
        (""p"".""ProductID"" > 50)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END)",
                Sql);
        }

        public override void Where_not_bool_member_compared_to_binary_expression()
        {
            base.Where_not_bool_member_compared_to_binary_expression();

            Assert.Equal(
                @"SELECT ""p"".""ProductID"", ""p"".""Discontinued"", ""p"".""ProductName"", ""p"".""UnitsInStock""
FROM ""Products"" AS ""p""
WHERE (""p"".""Discontinued"" <> CASE
    WHEN
        (""p"".""ProductID"" > 50)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END)",
                Sql);
        }

        public override void Where_not_bool_member_compared_to_not_bool_member()
        {
            base.Where_not_bool_member_compared_to_not_bool_member();

            Assert.Equal(
                @"SELECT ""p"".""ProductID"", ""p"".""Discontinued"", ""p"".""ProductName"", ""p"".""UnitsInStock""
FROM ""Products"" AS ""p""
WHERE ""p"".""Discontinued"" = ""p"".""Discontinued""",
                Sql);
        }

        public override void Where_negated_boolean_expression_compared_to_another_negated_boolean_expression()
        {
            base.Where_negated_boolean_expression_compared_to_another_negated_boolean_expression();

            Assert.Equal(
                @"SELECT ""p"".""ProductID"", ""p"".""Discontinued"", ""p"".""ProductName"", ""p"".""UnitsInStock""
FROM ""Products"" AS ""p""
WHERE (CASE
    WHEN
        (""p"".""ProductID"" > 50)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END = CASE
    WHEN
        (""p"".""ProductID"" > 20)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END)",
                Sql);
        }

        public override void Where_bool_parameter_compared_to_binary_expression()
        {
            base.Where_bool_parameter_compared_to_binary_expression();

            Assert.Equal(
                @"@__prm_0: True

SELECT ""p"".""ProductID"", ""p"".""Discontinued"", ""p"".""ProductName"", ""p"".""UnitsInStock""
FROM ""Products"" AS ""p""
WHERE (CASE
    WHEN
        (""p"".""ProductID"" > 50)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END <> @__prm_0)",
                Sql);
        }

        public override void Where_bool_member_and_parameter_compared_to_binary_expression_nested()
        {
            base.Where_bool_member_and_parameter_compared_to_binary_expression_nested();

            Assert.Equal(
                @"@__prm_0: True

SELECT ""p"".""ProductID"", ""p"".""Discontinued"", ""p"".""ProductName"", ""p"".""UnitsInStock""
FROM ""Products"" AS ""p""
WHERE (""p"".""Discontinued"" = CASE
    WHEN
        ((CASE
            WHEN
                (""p"".""ProductID"" > 50)
            THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
        END <> @__prm_0))
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END)",
                Sql);
        }

        public override void Where_de_morgan_or_optimizated()
        {
            base.Where_de_morgan_or_optimizated();

            Assert.Equal(
                @"SELECT ""p"".""ProductID"", ""p"".""Discontinued"", ""p"".""ProductName"", ""p"".""UnitsInStock""
FROM ""Products"" AS ""p""
WHERE (""p"".""Discontinued"" = 0 AND ""p"".""ProductID"" >= 20)",
                Sql);
        }

        public override void Where_de_morgan_and_optimizated()
        {
            base.Where_de_morgan_and_optimizated();

            Assert.Equal(
                @"SELECT ""p"".""ProductID"", ""p"".""Discontinued"", ""p"".""ProductName"", ""p"".""UnitsInStock""
FROM ""Products"" AS ""p""
WHERE (""p"".""Discontinued"" = 0 OR ""p"".""ProductID"" >= 20)",
                Sql);
        }

        public override void Where_complex_negated_expression_optimized()
        {
            base.Where_complex_negated_expression_optimized();

            Assert.Equal(
                @"SELECT ""p"".""ProductID"", ""p"".""Discontinued"", ""p"".""ProductName"", ""p"".""UnitsInStock""
FROM ""Products"" AS ""p""
WHERE ((""p"".""Discontinued"" = 0 AND ""p"".""ProductID"" < 60) AND ""p"".""ProductID"" > 30)",
                Sql);
        }

        public override void Where_short_member_comparison()
        {
            base.Where_short_member_comparison();

            Assert.Equal(
                @"SELECT ""p"".""ProductID"", ""p"".""Discontinued"", ""p"".""ProductName"", ""p"".""UnitsInStock""
FROM ""Products"" AS ""p""
WHERE ""p"".""UnitsInStock"" > 10",
                Sql);
        }

        public override void Where_true()
        {
            base.Where_true();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE 1 = 1",
                Sql);
        }

        public override void Where_compare_constructed_equal()
        {
            base.Where_compare_constructed_equal();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""",
                Sql);
        }

        public override void Where_compare_constructed_multi_value_equal()
        {
            base.Where_compare_constructed_multi_value_equal();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""",
                Sql);
        }

        public override void Where_compare_constructed_multi_value_not_equal()
        {
            base.Where_compare_constructed_multi_value_not_equal();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""",
                Sql);
        }

        public override void Where_compare_constructed()
        {
            base.Where_compare_constructed();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""",
                Sql);
        }

        public override void Where_compare_null()
        {
            base.Where_compare_null();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (""c"".""City"" IS NULL AND ""c"".""Country"" = 'UK')",
                Sql);
        }

        public override void Single_Predicate()
        {
            base.Single_Predicate();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CustomerID"" = 'ALFKI'
LIMIT 2",
                Sql);
        }

        // TODO: Complex projection translation.

        public override void Projection_when_arithmetic_expressions()
        {
            //            base.Projection_when_arithmetic_expressions();
            //
            //            Assert.Equal(
            //                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate"", ""o"".""OrderID"", ""o"".""OrderID"" * 2, ""o"".""OrderID"" + 23, 100000 - ""o"".""OrderID"", ""o"".""OrderID"" / (""o"".""OrderID"" / 2)
            //FROM ""Orders"" AS ""o""",
            //                Sql);
        }

        public override void Projection_when_arithmetic_mixed()
        {
            //base.Projection_when_arithmetic_mixed();
        }

        public override void Projection_when_arithmetic_mixed_subqueries()
        {
            //base.Projection_when_arithmetic_mixed_subqueries();
        }

        public override void Projection_when_null_value()
        {
            base.Projection_when_null_value();

            Assert.Equal(
                @"SELECT ""c"".""Region""
FROM ""Customers"" AS ""c""",
                Sql);
        }

        public override void String_StartsWith_Literal()
        {
            base.String_StartsWith_Literal();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE 'M' || '%'",
                Sql);
        }

        public override void String_StartsWith_Identity()
        {
            base.String_StartsWith_Identity();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE ""c"".""ContactName"" || '%'",
                Sql);
        }

        public override void String_StartsWith_Column()
        {
            base.String_StartsWith_Column();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE ""c"".""ContactName"" || '%'",
                Sql);
        }

        public override void String_StartsWith_MethodCall()
        {
            base.String_StartsWith_MethodCall();

            Assert.Equal(
                @"@__LocalMethod1_0: M

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE @__LocalMethod1_0 || '%'",
                Sql);
        }

        public override void String_EndsWith_Literal()
        {
            base.String_EndsWith_Literal();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE '%' || 'b'",
                Sql);
        }

        public override void String_EndsWith_Identity()
        {
            base.String_EndsWith_Identity();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE '%' || ""c"".""ContactName""",
                Sql);
        }

        public override void String_EndsWith_Column()
        {
            base.String_EndsWith_Column();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE '%' || ""c"".""ContactName""",
                Sql);
        }

        public override void String_EndsWith_MethodCall()
        {
            base.String_EndsWith_MethodCall();

            Assert.Equal(
                @"@__LocalMethod2_0: m

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE '%' || @__LocalMethod2_0",
                Sql);
        }

        public override void String_Contains_Literal()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.Contains("M")), // case-insensitive
                cs => cs.Where(c => c.ContactName.Contains("M") || c.ContactName.Contains("m")), // case-sensitive
                entryCount: 34);

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE ('%' || 'M' || '%')",
                Sql);
        }

        public override void String_Contains_Identity()
        {
            base.String_Contains_Identity();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE ('%' || ""c"".""ContactName"" || '%')",
                Sql);
        }

        public override void String_Contains_Column()
        {
            base.String_Contains_Column();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE ('%' || ""c"".""ContactName"" || '%')",
                Sql);
        }

        public override void String_Contains_MethodCall()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.Contains(LocalMethod1())), // case-insensitive
                cs => cs.Where(c => c.ContactName.Contains(LocalMethod1()) || c.ContactName.Contains(LocalMethod2())), // case-sensitive
                entryCount: 34);

            Assert.Equal(
                @"@__LocalMethod1_0: M

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""ContactName"" LIKE ('%' || @__LocalMethod1_0 || '%')",
                Sql);
        }

        public override void Where_string_to_lower()
        {
            base.Where_string_to_lower();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (lower(""c"".""CustomerID"") = 'alfki')",
                Sql);
        }

        public override void Where_string_to_upper()
        {
            base.Where_string_to_upper();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (upper(""c"".""CustomerID"") = 'ALFKI')",
                Sql);
        }

        public override void Select_nested_collection()
        {
            base.Select_nested_collection();

            Assert.StartsWith(
                @"SELECT ""c"".""CustomerID""
FROM ""Customers"" AS ""c""
WHERE ""c"".""City"" = 'London'
ORDER BY ""c"".""CustomerID""

SELECT ""o"".""CustomerID"", ""o"".""OrderDate"", ""o"".""OrderID""
FROM ""Orders"" AS ""o""
ORDER BY ""o"".""OrderID""

",
                Sql);
        }

        public override void Select_correlated_subquery_projection()
        {
            base.Select_correlated_subquery_projection();

            Assert.StartsWith(
                @"SELECT ""c"".""CustomerID""
FROM ""Customers"" AS ""c""

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""

",
                Sql);
        }

        public override void Select_correlated_subquery_ordered()
        {
            base.Select_correlated_subquery_ordered();

            Assert.StartsWith(
                @"SELECT ""c"".""CustomerID""
FROM ""Customers"" AS ""c""

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""

",
                Sql);
        }

        public override void Select_many_cross_join_same_collection()
        {
            base.Select_many_cross_join_same_collection();

            Assert.Equal(
                @"SELECT ""c0"".""CustomerID"", ""c0"".""Address"", ""c0"".""City"", ""c0"".""CompanyName"", ""c0"".""ContactName"", ""c0"".""ContactTitle"", ""c0"".""Country"", ""c0"".""Fax"", ""c0"".""Phone"", ""c0"".""PostalCode"", ""c0"".""Region""
FROM ""Customers"" AS ""c""
CROSS JOIN ""Customers"" AS ""c0""",
                Sql);
        }

        public override void Join_same_collection_multiple()
        {
            base.Join_same_collection_multiple();

            Assert.Equal(
                @"SELECT ""c3"".""CustomerID"", ""c3"".""Address"", ""c3"".""City"", ""c3"".""CompanyName"", ""c3"".""ContactName"", ""c3"".""ContactTitle"", ""c3"".""Country"", ""c3"".""Fax"", ""c3"".""Phone"", ""c3"".""PostalCode"", ""c3"".""Region""
FROM ""Customers"" AS ""o""
INNER JOIN ""Customers"" AS ""c2"" ON ""o"".""CustomerID"" = ""c2"".""CustomerID""
INNER JOIN ""Customers"" AS ""c3"" ON ""o"".""CustomerID"" = ""c3"".""CustomerID""",
                Sql);
        }

        public override void Join_same_collection_force_alias_uniquefication()
        {
            base.Join_same_collection_force_alias_uniquefication();

            Assert.Equal(
                @"SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate"", ""o0"".""OrderID"", ""o0"".""CustomerID"", ""o0"".""EmployeeID"", ""o0"".""OrderDate""
FROM ""Orders"" AS ""o""
INNER JOIN ""Orders"" AS ""o0"" ON ""o"".""CustomerID"" = ""o0"".""CustomerID""",
                Sql);
        }

        public override void Where_chain()
        {
            base.Where_chain();

            Assert.Equal(
                @"@__p_0: 01/01/1998 00:00:00

SELECT ""o"".""OrderID"", ""o"".""CustomerID"", ""o"".""EmployeeID"", ""o"".""OrderDate""
FROM ""Orders"" AS ""o""
WHERE (""o"".""CustomerID"" = 'QUICK' AND ""o"".""OrderDate"" > @__p_0)",
                Sql);
        }

        public override void OrderBy_null_coalesce_operator()
        {
            base.OrderBy_null_coalesce_operator();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
ORDER BY COALESCE(""c"".""Region"", 'ZZ')",
                Sql);
        }

        public override void Select_null_coalesce_operator()
        {
            base.Select_null_coalesce_operator();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""CompanyName"", COALESCE(""c"".""Region"", 'ZZ') AS ""Coalesce""
FROM ""Customers"" AS ""c""
ORDER BY ""Coalesce""",
                Sql);
        }

        public override void OrderBy_conditional_operator()
        {
            base.OrderBy_conditional_operator();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
ORDER BY CASE
    WHEN
        (""c"".""Region"" IS NULL)
    THEN 'ZZ' ELSE ""c"".""Region""
END",
                Sql);
        }

        public override void Contains_with_local_array_closure()
        {
            base.Contains_with_local_array_closure();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CustomerID"" IN ('ABCDE', 'ALFKI')",
                Sql);
        }

        public override void Contains_with_local_array_inline()
        {
            base.Contains_with_local_array_inline();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CustomerID"" IN ('ABCDE', 'ALFKI')",
                Sql);
        }

        public override void Contains_with_local_list_closure()
        {
            base.Contains_with_local_list_closure();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CustomerID"" IN ('ABCDE', 'ALFKI')",
                Sql);
        }

        public override void Contains_with_local_list_inline()
        {
            base.Contains_with_local_list_inline();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CustomerID"" IN ('ABCDE', 'ALFKI')",
                Sql);
        }

        public override void Contains_with_local_list_inline_closure_mix()
        {
            base.Contains_with_local_list_inline_closure_mix();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CustomerID"" IN ('ABCDE', 'ALFKI')",
                Sql);
        }

        public override void Contains_with_local_collection_false()
        {
            base.Contains_with_local_collection_false();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CustomerID"" NOT IN ('ABCDE', 'ALFKI')",
                Sql);
        }

        public override void Contains_with_local_collection_complex_predicate_and()
        {
            base.Contains_with_local_collection_complex_predicate_and();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (""c"".""CustomerID"" IN ('ALFKI', 'ABCDE') AND ""c"".""CustomerID"" IN ('ABCDE', 'ALFKI'))",
                Sql);
        }

        public override void Contains_with_local_collection_complex_predicate_or()
        {
            base.Contains_with_local_collection_complex_predicate_or();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CustomerID"" IN ('ABCDE', 'ALFKI', 'ALFKI', 'ABCDE')",
                Sql);
        }

        public override void Contains_with_local_collection_complex_predicate_not_matching_ins1()
        {
            base.Contains_with_local_collection_complex_predicate_not_matching_ins1();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (""c"".""CustomerID"" IN ('ALFKI', 'ABCDE') OR ""c"".""CustomerID"" NOT IN ('ABCDE', 'ALFKI'))",
                Sql);
        }

        public override void Contains_with_local_collection_complex_predicate_not_matching_ins2()
        {
            base.Contains_with_local_collection_complex_predicate_not_matching_ins2();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (""c"".""CustomerID"" IN ('ABCDE', 'ALFKI') AND ""c"".""CustomerID"" NOT IN ('ALFKI', 'ABCDE'))",
                Sql);
        }

        public override void Contains_with_local_collection_sql_injection()
        {
            base.Contains_with_local_collection_sql_injection();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CustomerID"" IN ('ALFKI', 'ABC'')); GO; DROP TABLE Orders; GO; --', 'ALFKI', 'ABCDE')",
                Sql);
        }

        public override void Contains_with_local_collection_empty_closure()
        {
            base.Contains_with_local_collection_empty_closure();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE 1 = 0",
                Sql);
        }

        public override void Contains_with_local_collection_empty_inline()
        {
            base.Contains_with_local_collection_empty_inline();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE 1 = 1",
                Sql);
        }

        public override void Projection_null_coalesce_operator()
        {
            base.Projection_null_coalesce_operator();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""CompanyName"", COALESCE(""c"".""Region"", 'ZZ') AS ""Coalesce""
FROM ""Customers"" AS ""c""",
                Sql);
        }

        public override void Filter_coalesce_operator()
        {
            base.Filter_coalesce_operator();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE (COALESCE(""c"".""CompanyName"", ""c"".""ContactName"") = 'The Big Cheese')",
                Sql);
        }

        public override void Take_skip_null_coalesce_operator()
        {
            base.Take_skip_null_coalesce_operator();

            Assert.Equal(@"SELECT DISTINCT ""t1"".*
FROM (
    SELECT ""t0"".*
    FROM (
        SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
        FROM ""Customers"" AS ""c""
        ORDER BY COALESCE(""c"".""Region"", 'ZZ')
        LIMIT 10
    ) AS ""t0""
    ORDER BY COALESCE(""t0"".""Region"", 'ZZ')
    LIMIT -1 OFFSET 5
) AS ""t1""", Sql);
        }

        public override void Select_take_null_coalesce_operator()
        {
            base.Select_take_null_coalesce_operator();

            Assert.Equal(@"SELECT ""c"".""CustomerID"", ""c"".""CompanyName"", COALESCE(""c"".""Region"", 'ZZ') AS ""Coalesce""
FROM ""Customers"" AS ""c""
ORDER BY ""Coalesce""
LIMIT 5", Sql);
        }

        public override void Select_take_skip_null_coalesce_operator()
        {
            base.Select_take_skip_null_coalesce_operator();

            Assert.Equal(
                @"SELECT ""t0"".*
FROM (
    SELECT ""c"".""CustomerID"", ""c"".""CompanyName"", COALESCE(""c"".""Region"", 'ZZ') AS ""Coalesce""
    FROM ""Customers"" AS ""c""
    ORDER BY ""Coalesce""
    LIMIT 10
) AS ""t0""
ORDER BY ""Coalesce""
LIMIT -1 OFFSET 5",
                Sql);
        }

        public override void Selected_column_can_coalesce()
        {
            base.Selected_column_can_coalesce();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
ORDER BY COALESCE(""c"".""Region"", 'ZZ')",
                Sql);
        }

        public QuerySqliteTest(NorthwindQuerySqliteFixture fixture)
            : base(fixture)
        {
        }

        private static string Sql => TestSqlLoggerFactory.Sql;
    }
}
