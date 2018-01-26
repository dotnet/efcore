// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public partial class SimpleQuerySqlServerTest
    {
        public override void Where_simple()
        {
            base.Where_simple();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'");
        }

        public override void Where_simple_closure()
        {
            base.Where_simple_closure();

            AssertSql(
                @"@__city_0='London' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_0");
        }

        public override void Where_indexer_closure()
        {
            base.Where_indexer_closure();

            AssertSql(
                @"@__p_0='London' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__p_0");
        }

        public override void Where_dictionary_key_access_closure()
        {
            base.Where_dictionary_key_access_closure();

            AssertSql(
                @"@__get_Item_0='London' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__get_Item_0");
        }

        public override void Where_tuple_item_closure()
        {
            base.Where_tuple_item_closure();

            AssertSql(
                @"@__predicateTuple_Item2_0='London' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__predicateTuple_Item2_0");
        }

        public override void Where_named_tuple_item_closure()
        {
            base.Where_named_tuple_item_closure();

            AssertSql(
                @"@__predicateTuple_Item2_0='London' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__predicateTuple_Item2_0");
        }

        public override void Where_simple_closure_constant()
        {
            base.Where_simple_closure_constant();

            AssertSql(
                @"@__predicate_0='True'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE @__predicate_0 = 1");
        }

        public override void Where_simple_closure_via_query_cache()
        {
            base.Where_simple_closure_via_query_cache();

            AssertSql(
                @"@__city_0='London' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_0",
                //
                @"@__city_0='Seattle' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_0");
        }

        public override void Where_method_call_nullable_type_closure_via_query_cache()
        {
            base.Where_method_call_nullable_type_closure_via_query_cache();

            AssertSql(
                @"@__city_Int_0='2'

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] = @__city_Int_0",
                //
                @"@__city_Int_0='5'

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] = @__city_Int_0");
        }

        public override void Where_method_call_nullable_type_reverse_closure_via_query_cache()
        {
            base.Where_method_call_nullable_type_reverse_closure_via_query_cache();

            AssertSql(
                @"@__city_NullableInt_0='1' (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] > @__city_NullableInt_0",
                //
                @"@__city_NullableInt_0='5' (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] > @__city_NullableInt_0");
        }

        public override void Where_method_call_closure_via_query_cache()
        {
            base.Where_method_call_closure_via_query_cache();

            AssertSql(
                @"@__GetCity_0='London' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__GetCity_0",
                //
                @"@__GetCity_0='Seattle' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__GetCity_0");
        }

        public override void Where_field_access_closure_via_query_cache()
        {
            base.Where_field_access_closure_via_query_cache();

            AssertSql(
                @"@__city_InstanceFieldValue_0='London' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_InstanceFieldValue_0",
                //
                @"@__city_InstanceFieldValue_0='Seattle' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_InstanceFieldValue_0");
        }

        public override void Where_property_access_closure_via_query_cache()
        {
            base.Where_property_access_closure_via_query_cache();

            AssertSql(
                @"@__city_InstancePropertyValue_0='London' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_InstancePropertyValue_0",
                //
                @"@__city_InstancePropertyValue_0='Seattle' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_InstancePropertyValue_0");
        }

        public override void Where_static_field_access_closure_via_query_cache()
        {
            base.Where_static_field_access_closure_via_query_cache();

            AssertSql(
                @"@__StaticFieldValue_0='London' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__StaticFieldValue_0",
                //
                @"@__StaticFieldValue_0='Seattle' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__StaticFieldValue_0");
        }

        public override void Where_static_property_access_closure_via_query_cache()
        {
            base.Where_static_property_access_closure_via_query_cache();

            AssertSql(
                @"@__StaticPropertyValue_0='London' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__StaticPropertyValue_0",
                //
                @"@__StaticPropertyValue_0='Seattle' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__StaticPropertyValue_0");
        }

        public override void Where_nested_field_access_closure_via_query_cache()
        {
            base.Where_nested_field_access_closure_via_query_cache();

            AssertSql(
                @"@__city_Nested_InstanceFieldValue_0='London' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_Nested_InstanceFieldValue_0",
                //
                @"@__city_Nested_InstanceFieldValue_0='Seattle' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_Nested_InstanceFieldValue_0");
        }

        public override void Where_nested_property_access_closure_via_query_cache()
        {
            base.Where_nested_property_access_closure_via_query_cache();

            AssertSql(
                @"@__city_Nested_InstancePropertyValue_0='London' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_Nested_InstancePropertyValue_0",
                //
                @"@__city_Nested_InstancePropertyValue_0='Seattle' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_Nested_InstancePropertyValue_0");
        }

        public override void Where_new_instance_field_access_closure_via_query_cache()
        {
            base.Where_new_instance_field_access_closure_via_query_cache();

            AssertSql(
                @"@__InstanceFieldValue_0='London' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__InstanceFieldValue_0",
                //
                @"@__InstanceFieldValue_0='Seattle' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__InstanceFieldValue_0");
        }

        public override void Where_simple_closure_via_query_cache_nullable_type()
        {
            base.Where_simple_closure_via_query_cache_nullable_type();

            AssertSql(
                @"@__reportsTo_0='2' (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] = @__reportsTo_0",
                //
                @"@__reportsTo_0='5' (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] = @__reportsTo_0",
                //
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] IS NULL");
        }

        public override void Where_simple_closure_via_query_cache_nullable_type_reverse()
        {
            base.Where_simple_closure_via_query_cache_nullable_type_reverse();

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] IS NULL",
                //
                @"@__reportsTo_0='5' (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] = @__reportsTo_0",
                //
                @"@__reportsTo_0='2' (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] = @__reportsTo_0");
        }

        public override void Where_subquery_closure_via_query_cache()
        {
            base.Where_subquery_closure_via_query_cache();

            AssertSql(
                @"@__customerID_0='ALFKI' (Size = 5)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE ([o].[CustomerID] = @__customerID_0) AND ([o].[CustomerID] = [c].[CustomerID]))",
                //
                @"@__customerID_0='ANATR' (Size = 5)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE ([o].[CustomerID] = @__customerID_0) AND ([o].[CustomerID] = [c].[CustomerID]))");
        }

        public override void Where_simple_shadow()
        {
            base.Where_simple_shadow();

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[Title] = N'Sales Representative'");
        }

        public override void Where_simple_shadow_projection()
        {
            base.Where_simple_shadow_projection();

            AssertSql(
                @"SELECT [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[Title] = N'Sales Representative'");
        }

        public override void Where_shadow_subquery_FirstOrDefault()
        {
            base.Where_shadow_subquery_FirstOrDefault();

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[Title] = (
    SELECT TOP(1) [e2].[Title]
    FROM [Employees] AS [e2]
    ORDER BY [e2].[Title]
)");
        }

        public override void Where_client()
        {
            base.Where_client();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Where_subquery_correlated()
        {
            base.Where_subquery_correlated();

            AssertSql(
                @"SELECT [c1].[CustomerID], [c1].[Address], [c1].[City], [c1].[CompanyName], [c1].[ContactName], [c1].[ContactTitle], [c1].[Country], [c1].[Fax], [c1].[Phone], [c1].[PostalCode], [c1].[Region]
FROM [Customers] AS [c1]
WHERE EXISTS (
    SELECT 1
    FROM [Customers] AS [c2]
    WHERE [c1].[CustomerID] = [c2].[CustomerID])");
        }

        public override void Where_subquery_correlated_client_eval()
        {
            base.Where_subquery_correlated_client_eval();

            AssertSql(
                @"@__p_0='5'

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
) AS [t]
ORDER BY [t].[CustomerID]",
                //
                @"@_outer_CustomerID='ALFKI' (Size = 5)

SELECT [c2].[CustomerID], [c2].[Address], [c2].[City], [c2].[CompanyName], [c2].[ContactName], [c2].[ContactTitle], [c2].[Country], [c2].[Fax], [c2].[Phone], [c2].[PostalCode], [c2].[Region]
FROM [Customers] AS [c2]
WHERE @_outer_CustomerID = [c2].[CustomerID]",
                //
                @"@_outer_CustomerID='ANATR' (Size = 5)

SELECT [c2].[CustomerID], [c2].[Address], [c2].[City], [c2].[CompanyName], [c2].[ContactName], [c2].[ContactTitle], [c2].[Country], [c2].[Fax], [c2].[Phone], [c2].[PostalCode], [c2].[Region]
FROM [Customers] AS [c2]
WHERE @_outer_CustomerID = [c2].[CustomerID]",
                //
                @"@_outer_CustomerID='ANTON' (Size = 5)

SELECT [c2].[CustomerID], [c2].[Address], [c2].[City], [c2].[CompanyName], [c2].[ContactName], [c2].[ContactTitle], [c2].[Country], [c2].[Fax], [c2].[Phone], [c2].[PostalCode], [c2].[Region]
FROM [Customers] AS [c2]
WHERE @_outer_CustomerID = [c2].[CustomerID]",
                //
                @"@_outer_CustomerID='AROUT' (Size = 5)

SELECT [c2].[CustomerID], [c2].[Address], [c2].[City], [c2].[CompanyName], [c2].[ContactName], [c2].[ContactTitle], [c2].[Country], [c2].[Fax], [c2].[Phone], [c2].[PostalCode], [c2].[Region]
FROM [Customers] AS [c2]
WHERE @_outer_CustomerID = [c2].[CustomerID]",
                //
                @"@_outer_CustomerID='BERGS' (Size = 5)

SELECT [c2].[CustomerID], [c2].[Address], [c2].[City], [c2].[CompanyName], [c2].[ContactName], [c2].[ContactTitle], [c2].[Country], [c2].[Fax], [c2].[Phone], [c2].[PostalCode], [c2].[Region]
FROM [Customers] AS [c2]
WHERE @_outer_CustomerID = [c2].[CustomerID]");
        }

        public override void Where_client_and_server_top_level()
        {
            base.Where_client_and_server_top_level();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <> N'AROUT'");
        }

        public override void Where_client_or_server_top_level()
        {
            base.Where_client_or_server_top_level();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Where_client_and_server_non_top_level()
        {
            base.Where_client_and_server_non_top_level();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Where_client_deep_inside_predicate_and_server_top_level()
        {
            base.Where_client_deep_inside_predicate_and_server_top_level();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <> N'ALFKI'");
        }

        public override void Where_equals_method_string()
        {
            base.Where_equals_method_string();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'");
        }

        public override void Where_equals_method_int()
        {
            base.Where_equals_method_int();

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = 1");
        }

        [FrameworkSkipCondition(RuntimeFrameworks.CoreCLR, SkipReason = "Failing after netcoreapp2.0 upgrade")]
        public override void Where_equals_using_object_overload_on_mismatched_types()
        {
            base.Where_equals_using_object_overload_on_mismatched_types();

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE 0 = 1");

            Assert.Contains(RelationalStrings.LogPossibleUnintendedUseOfEquals.GenerateMessage("e.EmployeeID.Equals(Convert(__longPrm_0))"), Fixture.TestSqlLoggerFactory.Log);
        }

        public override void Where_equals_using_int_overload_on_mismatched_types()
        {
            base.Where_equals_using_int_overload_on_mismatched_types();

            AssertSql(
                @"@__shortPrm_0='1'

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = @__shortPrm_0");
        }

        [FrameworkSkipCondition(RuntimeFrameworks.CoreCLR, SkipReason = "Failing after netcoreapp2.0 upgrade")]
        public override void Where_equals_on_mismatched_types_nullable_int_long()
        {
            base.Where_equals_on_mismatched_types_nullable_int_long();

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE 0 = 1",
                //
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE 0 = 1");

            Assert.Contains(RelationalStrings.LogPossibleUnintendedUseOfEquals.GenerateMessage("__longPrm_0.Equals(Convert(e.ReportsTo))"), Fixture.TestSqlLoggerFactory.Log);

            Assert.Contains(RelationalStrings.LogPossibleUnintendedUseOfEquals.GenerateMessage("e.ReportsTo.Equals(Convert(__longPrm_0))"), Fixture.TestSqlLoggerFactory.Log);
        }

        [FrameworkSkipCondition(RuntimeFrameworks.CoreCLR, SkipReason = "Failing after netcoreapp2.0 upgrade")]
        public override void Where_equals_on_mismatched_types_nullable_long_nullable_int()
        {
            base.Where_equals_on_mismatched_types_nullable_long_nullable_int();

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE 0 = 1",
                //
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE 0 = 1");

            Assert.Contains(RelationalStrings.LogPossibleUnintendedUseOfEquals.GenerateMessage("__nullableLongPrm_0.Equals(Convert(e.ReportsTo))"), Fixture.TestSqlLoggerFactory.Log);

            Assert.Contains(RelationalStrings.LogPossibleUnintendedUseOfEquals.GenerateMessage("e.ReportsTo.Equals(Convert(__nullableLongPrm_0))"), Fixture.TestSqlLoggerFactory.Log);
        }

        public override void Where_equals_on_mismatched_types_int_nullable_int()
        {
            base.Where_equals_on_mismatched_types_int_nullable_int();

            AssertSql(
                @"@__intPrm_0='2'

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] = @__intPrm_0",
                //
                @"@__intPrm_0='2'

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE @__intPrm_0 = [e].[ReportsTo]");
        }

        public override void Where_equals_on_matched_nullable_int_types()
        {
            base.Where_equals_on_matched_nullable_int_types();

            AssertSql(
                @"@__nullableIntPrm_0='2' (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE @__nullableIntPrm_0 = [e].[ReportsTo]",
                //
                @"@__nullableIntPrm_0='2' (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] = @__nullableIntPrm_0");
        }

        public override void Where_equals_on_null_nullable_int_types()
        {
            base.Where_equals_on_null_nullable_int_types();

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] IS NULL",
                //
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] IS NULL");
        }

        public override void Where_comparison_nullable_type_not_null()
        {
            base.Where_comparison_nullable_type_not_null();

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] = 2");
        }

        public override void Where_comparison_nullable_type_null()
        {
            base.Where_comparison_nullable_type_null();

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] IS NULL");
        }

        public override void Where_string_length()
        {
            base.Where_string_length();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE CAST(LEN([c].[City]) AS int) = 6");
        }

        public override void Where_string_indexof()
        {
            base.Where_string_indexof();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (CHARINDEX(N'Sea', [c].[City]) - 1) <> -1");
        }

        public override void Where_string_replace()
        {
            base.Where_string_replace();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE REPLACE([c].[City], N'Sea', N'Rea') = N'Reattle'");
        }

        public override void Where_string_substring()
        {
            base.Where_string_substring();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE SUBSTRING([c].[City], 2, 2) = N'ea'");
        }

        public override void Where_datetime_now()
        {
            base.Where_datetime_now();

            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE GETDATE() <> @__myDatetime_0");
        }

        public override void Where_datetime_utcnow()
        {
            base.Where_datetime_utcnow();

            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE GETUTCDATE() <> @__myDatetime_0");
        }

        public override void Where_datetime_today()
        {
            base.Where_datetime_today();

            AssertSql(
             @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE CONVERT(date, GETDATE()) = CONVERT(date, GETDATE())");
        }

        public override void Where_datetime_date_component()
        {
            base.Where_datetime_date_component();

            AssertSql(
                @"@__myDatetime_0='1998-05-04T00:00:00'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE CONVERT(date, [o].[OrderDate]) = @__myDatetime_0");
        }

        public override void Where_date_add_year_constant_component()
        {
            base.Where_date_add_year_constant_component();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(year, DATEADD(year, -1, [o].[OrderDate])) = 1997");
        }

        public override void Where_datetime_year_component()
        {
            base.Where_datetime_year_component();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(year, [o].[OrderDate]) = 1998");
        }

        public override void Where_datetime_month_component()
        {
            base.Where_datetime_month_component();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(month, [o].[OrderDate]) = 4");
        }

        public override void Where_datetime_dayOfYear_component()
        {
            base.Where_datetime_dayOfYear_component();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(dayofyear, [o].[OrderDate]) = 68");
        }

        public override void Where_datetime_day_component()
        {
            base.Where_datetime_day_component();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(day, [o].[OrderDate]) = 4");
        }

        public override void Where_datetime_hour_component()
        {
            base.Where_datetime_hour_component();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(hour, [o].[OrderDate]) = 14");
        }

        public override void Where_datetime_minute_component()
        {
            base.Where_datetime_minute_component();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(minute, [o].[OrderDate]) = 23");
        }

        public override void Where_datetime_second_component()
        {
            base.Where_datetime_second_component();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(second, [o].[OrderDate]) = 44");
        }

        public override void Where_datetime_millisecond_component()
        {
            base.Where_datetime_millisecond_component();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(millisecond, [o].[OrderDate]) = 88");
        }

        public override void Where_simple_reversed()
        {
            base.Where_simple_reversed();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE N'London' = [c].[City]");
        }

        public override void Where_is_null()
        {
            base.Where_is_null();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] IS NULL");
        }

        public override void Where_null_is_null()
        {
            base.Where_null_is_null();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Where_constant_is_null()
        {
            base.Where_constant_is_null();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 0 = 1");
        }

        public override void Where_is_not_null()
        {
            base.Where_is_not_null();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] IS NOT NULL");
        }

        public override void Where_null_is_not_null()
        {
            base.Where_null_is_not_null();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 0 = 1");
        }

        public override void Where_constant_is_not_null()
        {
            base.Where_constant_is_not_null();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Where_identity_comparison()
        {
            base.Where_identity_comparison();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[City] = [c].[City]) OR ([c].[City] IS NULL AND [c].[City] IS NULL)");
        }

        public override void Where_in_optimization_multiple()
        {
            base.Where_in_optimization_multiple();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE ([c].[City] IN (N'London', N'Berlin') OR ([c].[CustomerID] = N'ALFKI')) OR ([c].[CustomerID] = N'ABCDE')");
        }

        public override void Where_not_in_optimization1()
        {
            base.Where_not_in_optimization1();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE (([c].[City] <> N'London') OR [c].[City] IS NULL) AND (([e].[City] <> N'London') OR [e].[City] IS NULL)");
        }

        public override void Where_not_in_optimization2()
        {
            base.Where_not_in_optimization2();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE [c].[City] NOT IN (N'London', N'Berlin')");
        }

        public override void Where_not_in_optimization3()
        {
            base.Where_not_in_optimization3();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE [c].[City] NOT IN (N'London', N'Berlin', N'Seattle')");
        }

        public override void Where_not_in_optimization4()
        {
            base.Where_not_in_optimization4();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE [c].[City] NOT IN (N'London', N'Berlin', N'Seattle', N'Lisboa')");
        }

        public override void Where_select_many_and()
        {
            base.Where_select_many_and();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE (([c].[City] = N'London') AND ([c].[Country] = N'UK')) AND (([e].[City] = N'London') AND ([e].[Country] = N'UK'))");
        }

        public override void Where_primitive()
        {
            base.Where_primitive();

            AssertSql(
                @"@__p_0='9'

SELECT [t].[EmployeeID]
FROM (
    SELECT TOP(@__p_0) [e].[EmployeeID]
    FROM [Employees] AS [e]
) AS [t]
WHERE [t].[EmployeeID] = 5");
        }

        public override void Where_bool_member()
        {
            base.Where_bool_member();

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = 1");
        }

        public override void Where_bool_member_false()
        {
            base.Where_bool_member_false();

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = 0");
        }

        public override void Where_bool_client_side_negated()
        {
            base.Where_bool_client_side_negated();

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = 1");
        }

        public override void Where_bool_member_negated_twice()
        {
            base.Where_bool_member_negated_twice();

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = 1");
        }

        public override void Where_bool_member_shadow()
        {
            base.Where_bool_member_shadow();

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = 1");
        }

        public override void Where_bool_member_false_shadow()
        {
            base.Where_bool_member_false_shadow();

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = 0");
        }

        public override void Where_bool_member_equals_constant()
        {
            base.Where_bool_member_equals_constant();

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = 1");
        }

        public override void Where_bool_member_in_complex_predicate()
        {
            base.Where_bool_member_in_complex_predicate();

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE (([p].[ProductID] > 100) AND ([p].[Discontinued] = 1)) OR ([p].[Discontinued] = 1)");
        }

        public override void Where_bool_member_compared_to_binary_expression()
        {
            base.Where_bool_member_compared_to_binary_expression();

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = CASE
    WHEN [p].[ProductID] > 50
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override void Where_not_bool_member_compared_to_not_bool_member()
        {
            base.Where_not_bool_member_compared_to_not_bool_member();

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = [p].[Discontinued]");
        }

        public override void Where_negated_boolean_expression_compared_to_another_negated_boolean_expression()
        {
            base.Where_negated_boolean_expression_compared_to_another_negated_boolean_expression();

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE CASE
    WHEN [p].[ProductID] > 50
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END = CASE
    WHEN [p].[ProductID] > 20
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override void Where_not_bool_member_compared_to_binary_expression()
        {
            base.Where_not_bool_member_compared_to_binary_expression();

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] <> CASE
    WHEN [p].[ProductID] > 50
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override void Where_bool_parameter()
        {
            base.Where_bool_parameter();

            AssertSql(
                @"@__prm_0='True'

SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE @__prm_0 = 1");
        }

        public override void Where_bool_parameter_compared_to_binary_expression()
        {
            base.Where_bool_parameter_compared_to_binary_expression();

            AssertSql(
                @"@__prm_0='True'

SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE CASE
    WHEN [p].[ProductID] > 50
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END <> @__prm_0");
        }

        public override void Where_bool_member_and_parameter_compared_to_binary_expression_nested()
        {
            base.Where_bool_member_and_parameter_compared_to_binary_expression_nested();

            AssertSql(
                @"@__prm_0='True'

SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = CASE
    WHEN CASE
        WHEN [p].[ProductID] > 50
        THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
    END <> @__prm_0
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override void Where_de_morgan_or_optimizated()
        {
            base.Where_de_morgan_or_optimizated();

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE ([p].[Discontinued] = 0) AND ([p].[ProductID] >= 20)");
        }

        public override void Where_de_morgan_and_optimizated()
        {
            base.Where_de_morgan_and_optimizated();

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE ([p].[Discontinued] = 0) OR ([p].[ProductID] >= 20)");
        }

        public override void Where_complex_negated_expression_optimized()
        {
            base.Where_complex_negated_expression_optimized();

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE (([p].[Discontinued] = 0) AND ([p].[ProductID] < 60)) AND ([p].[ProductID] > 30)");
        }

        public override void Where_short_member_comparison()
        {
            base.Where_short_member_comparison();

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[UnitsInStock] > 10");
        }

        public override void Where_comparison_to_nullable_bool()
        {
            base.Where_comparison_to_nullable_bool();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE RIGHT([c].[CustomerID], LEN(N'KI')) = N'KI'");
        }

        public override void Where_true()
        {
            base.Where_true();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Where_false()
        {
            base.Where_false();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 0 = 1");
        }

        public override void Where_default()
        {
            base.Where_default();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[Fax] IS NULL");
        }

        public override void Where_expression_invoke()
        {
            base.Where_expression_invoke();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override void Where_concat_string_int_comparison1()
        {
            base.Where_concat_string_int_comparison1();

            AssertSql(
                @"@__i_0='10'

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE ([c].[CustomerID] + CAST(@__i_0 AS nvarchar(max))) = [c].[CompanyName]");
        }

        public override void Where_concat_string_int_comparison2()
        {
            base.Where_concat_string_int_comparison2();

            AssertSql(
                @"@__i_0='10'

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE (CAST(@__i_0 AS nvarchar(max)) + [c].[CustomerID]) = [c].[CompanyName]");
        }

        public override void Where_concat_string_int_comparison3()
        {
            base.Where_concat_string_int_comparison3();

            AssertSql(
                @"@__i_0='10'
@__j_1='21'

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE (((CAST(@__i_0 + 20 AS nvarchar(max)) + [c].[CustomerID]) + CAST(@__j_1 AS nvarchar(max))) + CAST(42 AS nvarchar(max))) = [c].[CompanyName]");
        }

        public override void Where_ternary_boolean_condition_true()
        {
            base.Where_ternary_boolean_condition_true();

            AssertSql(
                @"@__flag_0='True'

SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE ((@__flag_0 = 1) AND ([p].[UnitsInStock] >= 20)) OR ((@__flag_0 <> 1) AND ([p].[UnitsInStock] < 20))");
        }

        public override void Where_ternary_boolean_condition_false()
        {
            base.Where_ternary_boolean_condition_false();

            AssertSql(
                @"@__flag_0='False'

SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE ((@__flag_0 = 1) AND ([p].[UnitsInStock] >= 20)) OR ((@__flag_0 <> 1) AND ([p].[UnitsInStock] < 20))");
        }

        public override void Where_ternary_boolean_condition_with_another_condition()
        {
            base.Where_ternary_boolean_condition_with_another_condition();

            AssertSql(
                @"@__productId_0='15'
@__flag_1='True'

SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE ([p].[ProductID] < @__productId_0) AND (((@__flag_1 = 1) AND ([p].[UnitsInStock] >= 20)) OR ((@__flag_1 <> 1) AND ([p].[UnitsInStock] < 20)))");
        }

        public override void Where_ternary_boolean_condition_with_false_as_result_true()
        {
            base.Where_ternary_boolean_condition_with_false_as_result_true();

            AssertSql(
                @"@__flag_0='True'

SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE (@__flag_0 = 1) AND ([p].[UnitsInStock] >= 20)");
        }

        public override void Where_ternary_boolean_condition_with_false_as_result_false()
        {
            base.Where_ternary_boolean_condition_with_false_as_result_false();

            AssertSql(
                @"@__flag_0='False'

SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE (@__flag_0 = 1) AND ([p].[UnitsInStock] >= 20)");
        }

        public override void Where_compare_constructed_equal()
        {
            base.Where_compare_constructed_equal();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Where_compare_constructed_multi_value_equal()
        {
            base.Where_compare_constructed_multi_value_equal();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Where_compare_constructed_multi_value_not_equal()
        {
            base.Where_compare_constructed_multi_value_not_equal();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Where_compare_tuple_constructed_equal()
        {
            base.Where_compare_tuple_constructed_equal();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Where_compare_tuple_constructed_multi_value_equal()
        {
            base.Where_compare_tuple_constructed_multi_value_equal();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Where_compare_tuple_constructed_multi_value_not_equal()
        {
            base.Where_compare_tuple_constructed_multi_value_not_equal();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Where_compare_tuple_create_constructed_equal()
        {
            base.Where_compare_tuple_create_constructed_equal();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Where_compare_tuple_create_constructed_multi_value_equal()
        {
            base.Where_compare_tuple_create_constructed_multi_value_equal();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Where_compare_tuple_create_constructed_multi_value_not_equal()
        {
            base.Where_compare_tuple_create_constructed_multi_value_not_equal();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Where_compare_null()
        {
            base.Where_compare_null();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] IS NULL AND ([c].[Country] = N'UK')");
        }

        public override void Where_Is_on_same_type()
        {
            base.Where_Is_on_same_type();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Where_chain()
        {
            base.Where_chain();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'QUICK') AND ([o].[OrderDate] > '1998-01-01T00:00:00.000')");
        }

        public override void Where_navigation_contains()
        {
            base.Where_navigation_contains();

            AssertSql(
                @"SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]",
                //
                @"SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT TOP(1) [c0].[CustomerID]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] = N'ALFKI'
    ORDER BY [c0].[CustomerID]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID]",
                //
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice], [od.Order].[OrderID], [od.Order].[CustomerID], [od.Order].[EmployeeID], [od.Order].[OrderDate]
FROM [Order Details] AS [od]
INNER JOIN [Orders] AS [od.Order] ON [od].[OrderID] = [od.Order].[OrderID]");
        }

        public override void Where_array_index()
        {
            base.Where_array_index();

            AssertSql(
                @"@__p_0='ALFKI' (Size = 5)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @__p_0");
        }
    }
}
