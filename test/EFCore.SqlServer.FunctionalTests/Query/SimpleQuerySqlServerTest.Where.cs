// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public partial class SimpleQuerySqlServerTest
    {
        private const string ConvertParams =
#if NET461
            null;
#elif NETCOREAPP2_0 || NETCOREAPP3_0
            ", Object";
#else
#error target frameworks need to be updated.
#endif

        public override async Task Where_simple(bool isAsync)
        {
            await base.Where_simple(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'");
        }

        public override async Task Where_as_queryable_expression(bool isAsync)
        {
            await base.Where_as_queryable_expression(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
    ) AS [t]
    WHERE [t].[CustomerID] = N'ALFKI')");
        }

        public override async Task Where_simple_closure(bool isAsync)
        {
            await base.Where_simple_closure(isAsync);

            AssertSql(
                @"@__city_0='London' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_0");
        }

        public override async Task Where_indexer_closure(bool isAsync)
        {
            await base.Where_indexer_closure(isAsync);

            AssertSql(
                @"@__p_0='London' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__p_0");
        }

        public override async Task Where_dictionary_key_access_closure(bool isAsync)
        {
            await base.Where_dictionary_key_access_closure(isAsync);

            AssertSql(
                @"@__get_Item_0='London' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__get_Item_0");
        }

        public override async Task Where_tuple_item_closure(bool isAsync)
        {
            await base.Where_tuple_item_closure(isAsync);

            AssertSql(
                @"@__predicateTuple_Item2_0='London' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__predicateTuple_Item2_0");
        }

        public override async Task Where_named_tuple_item_closure(bool isAsync)
        {
            await base.Where_named_tuple_item_closure(isAsync);

            AssertSql(
                @"@__predicateTuple_Item2_0='London' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__predicateTuple_Item2_0");
        }

        public override async Task Where_simple_closure_constant(bool isAsync)
        {
            await base.Where_simple_closure_constant(isAsync);

            AssertSql(
                @"@__predicate_0='True'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE @__predicate_0 = 1");
        }

        public override async Task Where_simple_closure_via_query_cache(bool isAsync)
        {
            await base.Where_simple_closure_via_query_cache(isAsync);

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

        public override async Task Where_method_call_nullable_type_closure_via_query_cache(bool isAsync)
        {
            await base.Where_method_call_nullable_type_closure_via_query_cache(isAsync);

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

        public override async Task Where_method_call_nullable_type_reverse_closure_via_query_cache(bool isAsync)
        {
            await base.Where_method_call_nullable_type_reverse_closure_via_query_cache(isAsync);

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

        public override async Task Where_method_call_closure_via_query_cache(bool isAsync)
        {
            await base.Where_method_call_closure_via_query_cache(isAsync);

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

        public override async Task Where_field_access_closure_via_query_cache(bool isAsync)
        {
            await base.Where_field_access_closure_via_query_cache(isAsync);

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

        public override async Task Where_property_access_closure_via_query_cache(bool isAsync)
        {
            await base.Where_property_access_closure_via_query_cache(isAsync);

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

        public override async Task Where_static_field_access_closure_via_query_cache(bool isAsync)
        {
            await base.Where_static_field_access_closure_via_query_cache(isAsync);

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

        public override async Task Where_static_property_access_closure_via_query_cache(bool isAsync)
        {
            await base.Where_static_property_access_closure_via_query_cache(isAsync);

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

        public override async Task Where_nested_field_access_closure_via_query_cache(bool isAsync)
        {
            await base.Where_nested_field_access_closure_via_query_cache(isAsync);

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

        public override async Task Where_nested_property_access_closure_via_query_cache(bool isAsync)
        {
            await base.Where_nested_property_access_closure_via_query_cache(isAsync);

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

        public override async Task Where_new_instance_field_access_closure_via_query_cache(bool isAsync)
        {
            await base.Where_new_instance_field_access_closure_via_query_cache(isAsync);

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

        public override async Task Where_simple_closure_via_query_cache_nullable_type(bool isAsync)
        {
            await base.Where_simple_closure_via_query_cache_nullable_type(isAsync);

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

        public override async Task Where_simple_closure_via_query_cache_nullable_type_reverse(bool isAsync)
        {
            await base.Where_simple_closure_via_query_cache_nullable_type_reverse(isAsync);

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

        public override async Task Where_bitwise_or(bool isAsync)
        {
            await base.Where_bitwise_or(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (CASE
    WHEN [c].[CustomerID] = N'ALFKI'
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END | CASE
    WHEN [c].[CustomerID] = N'ANATR'
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END) = 1");
        }

        public override async Task Where_bitwise_and(bool isAsync)
        {
            await base.Where_bitwise_and(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (CASE
    WHEN [c].[CustomerID] = N'ALFKI'
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END & CASE
    WHEN [c].[CustomerID] = N'ANATR'
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END) = 1");
        }

        public override async Task Where_bitwise_xor(bool isAsync)
        {
            await base.Where_bitwise_xor(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Where_simple_shadow(bool isAsync)
        {
            await base.Where_simple_shadow(isAsync);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[Title] = N'Sales Representative'");
        }

        public override async Task Where_simple_shadow_projection(bool isAsync)
        {
            await base.Where_simple_shadow_projection(isAsync);

            AssertSql(
                @"SELECT [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[Title] = N'Sales Representative'");
        }

        public override async Task Where_shadow_subquery_FirstOrDefault(bool isAsync)
        {
            await base.Where_shadow_subquery_FirstOrDefault(isAsync);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[Title] = (
    SELECT TOP(1) [e2].[Title]
    FROM [Employees] AS [e2]
    ORDER BY [e2].[Title]
)");
        }

        public override async Task Where_client(bool isAsync)
        {
            await base.Where_client(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Where_subquery_correlated(bool isAsync)
        {
            await base.Where_subquery_correlated(isAsync);

            AssertSql(
                @"SELECT [c1].[CustomerID], [c1].[Address], [c1].[City], [c1].[CompanyName], [c1].[ContactName], [c1].[ContactTitle], [c1].[Country], [c1].[Fax], [c1].[Phone], [c1].[PostalCode], [c1].[Region]
FROM [Customers] AS [c1]
WHERE EXISTS (
    SELECT 1
    FROM [Customers] AS [c2]
    WHERE [c1].[CustomerID] = [c2].[CustomerID])");
        }

        public override async Task Where_subquery_correlated_client_eval(bool isAsync)
        {
            await base.Where_subquery_correlated_client_eval(isAsync);

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

        public override async Task Where_client_and_server_top_level(bool isAsync)
        {
            await base.Where_client_and_server_top_level(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <> N'AROUT'");
        }

        public override async Task Where_client_or_server_top_level(bool isAsync)
        {
            await base.Where_client_or_server_top_level(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Where_client_and_server_non_top_level(bool isAsync)
        {
            await base.Where_client_and_server_non_top_level(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Where_client_deep_inside_predicate_and_server_top_level(bool isAsync)
        {
            await base.Where_client_deep_inside_predicate_and_server_top_level(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <> N'ALFKI'");
        }

        public override async Task Where_equals_method_string(bool isAsync)
        {
            await base.Where_equals_method_string(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'");
        }

        public override async Task Where_equals_method_int(bool isAsync)
        {
            await base.Where_equals_method_int(isAsync);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = 1");
        }

        public override async Task Where_equals_using_object_overload_on_mismatched_types(bool isAsync)
        {
            await base.Where_equals_using_object_overload_on_mismatched_types(isAsync);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE 0 = 1");

            Assert.Contains(RelationalStrings.LogPossibleUnintendedUseOfEquals.GenerateMessage($"e.EmployeeID.Equals(Convert(__longPrm_0{ConvertParams}))"), Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));
        }

        public override async Task Where_equals_using_int_overload_on_mismatched_types(bool isAsync)
        {
            await base.Where_equals_using_int_overload_on_mismatched_types(isAsync);

            AssertSql(
                @"@__shortPrm_0='1'

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = @__shortPrm_0");
        }

        public override async Task Where_equals_on_mismatched_types_nullable_int_long(bool isAsync)
        {
            await base.Where_equals_on_mismatched_types_nullable_int_long(isAsync);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE 0 = 1",
                //
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE 0 = 1");

            Assert.Contains(RelationalStrings.LogPossibleUnintendedUseOfEquals.GenerateMessage($"__longPrm_0.Equals(Convert(e.ReportsTo{ConvertParams}))"), Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));

            Assert.Contains(RelationalStrings.LogPossibleUnintendedUseOfEquals.GenerateMessage($"e.ReportsTo.Equals(Convert(__longPrm_0{ConvertParams}))"), Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));
        }

        public override async Task Where_equals_on_mismatched_types_nullable_long_nullable_int(bool isAsync)
        {
            await base.Where_equals_on_mismatched_types_nullable_long_nullable_int(isAsync);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE 0 = 1",
                //
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE 0 = 1");

            Assert.Contains(RelationalStrings.LogPossibleUnintendedUseOfEquals.GenerateMessage($"__nullableLongPrm_0.Equals(Convert(e.ReportsTo{ConvertParams}))"), Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));

            Assert.Contains(RelationalStrings.LogPossibleUnintendedUseOfEquals.GenerateMessage($"e.ReportsTo.Equals(Convert(__nullableLongPrm_0{ConvertParams}))"), Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));
        }

        public override async Task Where_equals_on_mismatched_types_int_nullable_int(bool isAsync)
        {
            await base.Where_equals_on_mismatched_types_int_nullable_int(isAsync);

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

        public override async Task Where_equals_on_matched_nullable_int_types(bool isAsync)
        {
            await base.Where_equals_on_matched_nullable_int_types(isAsync);

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

        public override async Task Where_equals_on_null_nullable_int_types(bool isAsync)
        {
            await base.Where_equals_on_null_nullable_int_types(isAsync);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] IS NULL",
                //
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] IS NULL");
        }

        public override async Task Where_comparison_nullable_type_not_null(bool isAsync)
        {
            await base.Where_comparison_nullable_type_not_null(isAsync);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] = 2");
        }

        public override async Task Where_comparison_nullable_type_null(bool isAsync)
        {
            await base.Where_comparison_nullable_type_null(isAsync);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] IS NULL");
        }

        public override async Task Where_string_length(bool isAsync)
        {
            await base.Where_string_length(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE CAST(LEN([c].[City]) AS int) = 6");
        }

        public override async Task Where_string_indexof(bool isAsync)
        {
            await base.Where_string_indexof(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ((CHARINDEX(N'Sea', [c].[City]) - 1) <> -1) OR [c].[City] IS NULL");
        }

        public override async Task Where_string_replace(bool isAsync)
        {
            await base.Where_string_replace(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE REPLACE([c].[City], N'Sea', N'Rea') = N'Reattle'");
        }

        public override async Task Where_string_substring(bool isAsync)
        {
            await base.Where_string_substring(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE SUBSTRING([c].[City], 2, 2) = N'ea'");
        }

        public override async Task Where_datetime_now(bool isAsync)
        {
            await base.Where_datetime_now(isAsync);

            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE GETDATE() <> @__myDatetime_0");
        }

        public override async Task Where_datetime_utcnow(bool isAsync)
        {
            await base.Where_datetime_utcnow(isAsync);

            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE GETUTCDATE() <> @__myDatetime_0");
        }

        public override async Task Where_datetime_today(bool isAsync)
        {
            await base.Where_datetime_today(isAsync);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE CONVERT(date, GETDATE()) = CONVERT(date, GETDATE())");
        }

        public override async Task Where_datetime_date_component(bool isAsync)
        {
            await base.Where_datetime_date_component(isAsync);

            AssertSql(
                @"@__myDatetime_0='1998-05-04T00:00:00' (DbType = DateTime)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE CONVERT(date, [o].[OrderDate]) = @__myDatetime_0");
        }

        public override async Task Where_date_add_year_constant_component(bool isAsync)
        {
            await base.Where_date_add_year_constant_component(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(year, DATEADD(year, -1, [o].[OrderDate])) = 1997");
        }

        public override async Task Where_datetime_year_component(bool isAsync)
        {
            await base.Where_datetime_year_component(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(year, [o].[OrderDate]) = 1998");
        }

        public override async Task Where_datetime_month_component(bool isAsync)
        {
            await base.Where_datetime_month_component(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(month, [o].[OrderDate]) = 4");
        }

        public override async Task Where_datetime_dayOfYear_component(bool isAsync)
        {
            await base.Where_datetime_dayOfYear_component(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(dayofyear, [o].[OrderDate]) = 68");
        }

        public override async Task Where_datetime_day_component(bool isAsync)
        {
            await base.Where_datetime_day_component(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(day, [o].[OrderDate]) = 4");
        }

        public override async Task Where_datetime_hour_component(bool isAsync)
        {
            await base.Where_datetime_hour_component(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(hour, [o].[OrderDate]) = 14");
        }

        public override async Task Where_datetime_minute_component(bool isAsync)
        {
            await base.Where_datetime_minute_component(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(minute, [o].[OrderDate]) = 23");
        }

        public override async Task Where_datetime_second_component(bool isAsync)
        {
            await base.Where_datetime_second_component(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(second, [o].[OrderDate]) = 44");
        }

        public override async Task Where_datetime_millisecond_component(bool isAsync)
        {
            await base.Where_datetime_millisecond_component(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(millisecond, [o].[OrderDate]) = 88");
        }

        public override async Task Where_datetimeoffset_now_component(bool isAsync)
        {
            await base.Where_datetimeoffset_now_component(isAsync);
            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] = SYSDATETIMEOFFSET()");
        }

        public override async Task Where_datetimeoffset_utcnow_component(bool isAsync)
        {
            await base.Where_datetimeoffset_utcnow_component(isAsync);
            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] = CAST(SYSUTCDATETIME() AS datetimeoffset)");
        }

        public override async Task Where_simple_reversed(bool isAsync)
        {
            await base.Where_simple_reversed(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE N'London' = [c].[City]");
        }

        public override async Task Where_is_null(bool isAsync)
        {
            await base.Where_is_null(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] IS NULL");
        }

        public override async Task Where_null_is_null(bool isAsync)
        {
            await base.Where_null_is_null(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Where_constant_is_null(bool isAsync)
        {
            await base.Where_constant_is_null(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 0 = 1");
        }

        public override async Task Where_is_not_null(bool isAsync)
        {
            await base.Where_is_not_null(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] IS NOT NULL");
        }

        public override async Task Where_null_is_not_null(bool isAsync)
        {
            await base.Where_null_is_not_null(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 0 = 1");
        }

        public override async Task Where_constant_is_not_null(bool isAsync)
        {
            await base.Where_constant_is_not_null(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Where_identity_comparison(bool isAsync)
        {
            await base.Where_identity_comparison(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[City] = [c].[City]) OR [c].[City] IS NULL");
        }

        public override async Task Where_in_optimization_multiple(bool isAsync)
        {
            await base.Where_in_optimization_multiple(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE ([c].[City] IN (N'London', N'Berlin') OR ([c].[CustomerID] = N'ALFKI')) OR ([c].[CustomerID] = N'ABCDE')");
        }

        public override async Task Where_not_in_optimization1(bool isAsync)
        {
            await base.Where_not_in_optimization1(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE (([c].[City] <> N'London') OR [c].[City] IS NULL) AND (([e].[City] <> N'London') OR [e].[City] IS NULL)");
        }

        public override async Task Where_not_in_optimization2(bool isAsync)
        {
            await base.Where_not_in_optimization2(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE [c].[City] NOT IN (N'London', N'Berlin')");
        }

        public override async Task Where_not_in_optimization3(bool isAsync)
        {
            await base.Where_not_in_optimization3(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE [c].[City] NOT IN (N'London', N'Berlin', N'Seattle')");
        }

        public override async Task Where_not_in_optimization4(bool isAsync)
        {
            await base.Where_not_in_optimization4(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE [c].[City] NOT IN (N'London', N'Berlin', N'Seattle', N'Lisboa')");
        }

        public override async Task Where_select_many_and(bool isAsync)
        {
            await base.Where_select_many_and(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE (([c].[City] = N'London') AND ([c].[Country] = N'UK')) AND (([e].[City] = N'London') AND ([e].[Country] = N'UK'))");
        }

        public override async Task Where_primitive(bool isAsync)
        {
            await base.Where_primitive(isAsync);

            AssertSql(
                @"@__p_0='9'

SELECT [t].[EmployeeID]
FROM (
    SELECT TOP(@__p_0) [e].[EmployeeID]
    FROM [Employees] AS [e]
) AS [t]
WHERE [t].[EmployeeID] = 5");
        }

        public override async Task Where_bool_member(bool isAsync)
        {
            await base.Where_bool_member(isAsync);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = 1");
        }

        public override async Task Where_bool_member_false(bool isAsync)
        {
            await base.Where_bool_member_false(isAsync);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = 0");
        }

        public override async Task Where_bool_client_side_negated(bool isAsync)
        {
            await base.Where_bool_client_side_negated(isAsync);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = 1");
        }

        public override async Task Where_bool_member_negated_twice(bool isAsync)
        {
            await base.Where_bool_member_negated_twice(isAsync);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = 1");
        }

        public override async Task Where_bool_member_shadow(bool isAsync)
        {
            await base.Where_bool_member_shadow(isAsync);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = 1");
        }

        public override async Task Where_bool_member_false_shadow(bool isAsync)
        {
            await base.Where_bool_member_false_shadow(isAsync);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = 0");
        }

        public override async Task Where_bool_member_equals_constant(bool isAsync)
        {
            await base.Where_bool_member_equals_constant(isAsync);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = 1");
        }

        public override async Task Where_bool_member_in_complex_predicate(bool isAsync)
        {
            await base.Where_bool_member_in_complex_predicate(isAsync);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE (([p].[ProductID] > 100) AND ([p].[Discontinued] = 1)) OR ([p].[Discontinued] = 1)");
        }

        public override async Task Where_bool_member_compared_to_binary_expression(bool isAsync)
        {
            await base.Where_bool_member_compared_to_binary_expression(isAsync);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = CASE
    WHEN [p].[ProductID] > 50
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override async Task Where_not_bool_member_compared_to_not_bool_member(bool isAsync)
        {
            await base.Where_not_bool_member_compared_to_not_bool_member(isAsync);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = [p].[Discontinued]");
        }

        public override async Task Where_negated_boolean_expression_compared_to_another_negated_boolean_expression(bool isAsync)
        {
            await base.Where_negated_boolean_expression_compared_to_another_negated_boolean_expression(isAsync);

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

        public override async Task Where_not_bool_member_compared_to_binary_expression(bool isAsync)
        {
            await base.Where_not_bool_member_compared_to_binary_expression(isAsync);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] <> CASE
    WHEN [p].[ProductID] > 50
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END");
        }

        public override async Task Where_bool_parameter(bool isAsync)
        {
            await base.Where_bool_parameter(isAsync);

            AssertSql(
                @"@__prm_0='True'

SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE @__prm_0 = 1");
        }

        public override async Task Where_bool_parameter_compared_to_binary_expression(bool isAsync)
        {
            await base.Where_bool_parameter_compared_to_binary_expression(isAsync);

            AssertSql(
                @"@__prm_0='True'

SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE CASE
    WHEN [p].[ProductID] > 50
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END <> @__prm_0");
        }

        public override async Task Where_bool_member_and_parameter_compared_to_binary_expression_nested(bool isAsync)
        {
            await base.Where_bool_member_and_parameter_compared_to_binary_expression_nested(isAsync);

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

        public override async Task Where_de_morgan_or_optimizated(bool isAsync)
        {
            await base.Where_de_morgan_or_optimizated(isAsync);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE ([p].[Discontinued] = 0) AND ([p].[ProductID] >= 20)");
        }

        public override async Task Where_de_morgan_and_optimizated(bool isAsync)
        {
            await base.Where_de_morgan_and_optimizated(isAsync);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE ([p].[Discontinued] = 0) OR ([p].[ProductID] >= 20)");
        }

        public override async Task Where_complex_negated_expression_optimized(bool isAsync)
        {
            await base.Where_complex_negated_expression_optimized(isAsync);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE (([p].[Discontinued] = 0) AND ([p].[ProductID] < 60)) AND ([p].[ProductID] > 30)");
        }

        public override async Task Where_short_member_comparison(bool isAsync)
        {
            await base.Where_short_member_comparison(isAsync);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[UnitsInStock] > CAST(10 AS smallint)");
        }

        public override async Task Where_comparison_to_nullable_bool(bool isAsync)
        {
            await base.Where_comparison_to_nullable_bool(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE RIGHT([c].[CustomerID], LEN(N'KI')) = N'KI'");
        }

        public override async Task Where_true(bool isAsync)
        {
            await base.Where_true(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Where_false(bool isAsync)
        {
            await base.Where_false(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 0 = 1");
        }

        public override async Task Where_default(bool isAsync)
        {
            await base.Where_default(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[Fax] IS NULL");
        }

        public override async Task Where_expression_invoke(bool isAsync)
        {
            await base.Where_expression_invoke(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override async Task Where_concat_string_int_comparison1(bool isAsync)
        {
            await base.Where_concat_string_int_comparison1(isAsync);

            AssertSql(
                @"@__i_0='10'

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE ([c].[CustomerID] + CAST(@__i_0 AS nvarchar(max))) = [c].[CompanyName]");
        }

        public override async Task Where_concat_string_int_comparison2(bool isAsync)
        {
            await base.Where_concat_string_int_comparison2(isAsync);

            AssertSql(
                @"@__i_0='10'

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE (CAST(@__i_0 AS nvarchar(max)) + [c].[CustomerID]) = [c].[CompanyName]");
        }

        public override async Task Where_concat_string_int_comparison3(bool isAsync)
        {
            await base.Where_concat_string_int_comparison3(isAsync);

            AssertSql(
                @"@__i_0='10'
@__j_1='21'

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE (((CAST(@__i_0 + 20 AS nvarchar(max)) + [c].[CustomerID]) + CAST(@__j_1 AS nvarchar(max))) + CAST(42 AS nvarchar(max))) = [c].[CompanyName]");
        }

        public override async Task Where_ternary_boolean_condition_true(bool isAsync)
        {
            await base.Where_ternary_boolean_condition_true(isAsync);

            AssertSql(
                @"@__flag_0='True'

SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE ((@__flag_0 = 1) AND ([p].[UnitsInStock] >= CAST(20 AS smallint))) OR ((@__flag_0 <> 1) AND ([p].[UnitsInStock] < CAST(20 AS smallint)))");
        }

        public override async Task Where_ternary_boolean_condition_false(bool isAsync)
        {
            await base.Where_ternary_boolean_condition_false(isAsync);

            AssertSql(
                @"@__flag_0='False'

SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE ((@__flag_0 = 1) AND ([p].[UnitsInStock] >= CAST(20 AS smallint))) OR ((@__flag_0 <> 1) AND ([p].[UnitsInStock] < CAST(20 AS smallint)))");
        }

        public override async Task Where_ternary_boolean_condition_with_another_condition(bool isAsync)
        {
            await base.Where_ternary_boolean_condition_with_another_condition(isAsync);

            AssertSql(
                @"@__productId_0='15'
@__flag_1='True'

SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE ([p].[ProductID] < @__productId_0) AND (((@__flag_1 = 1) AND ([p].[UnitsInStock] >= CAST(20 AS smallint))) OR ((@__flag_1 <> 1) AND ([p].[UnitsInStock] < CAST(20 AS smallint))))");
        }

        public override async Task Where_ternary_boolean_condition_with_false_as_result_true(bool isAsync)
        {
            await base.Where_ternary_boolean_condition_with_false_as_result_true(isAsync);

            AssertSql(
                @"@__flag_0='True'

SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE (@__flag_0 = 1) AND ([p].[UnitsInStock] >= CAST(20 AS smallint))");
        }

        public override async Task Where_ternary_boolean_condition_with_false_as_result_false(bool isAsync)
        {
            await base.Where_ternary_boolean_condition_with_false_as_result_false(isAsync);

            AssertSql(
                @"@__flag_0='False'

SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE (@__flag_0 = 1) AND ([p].[UnitsInStock] >= CAST(20 AS smallint))");
        }

        public override async Task Where_compare_constructed_equal(bool isAsync)
        {
            await base.Where_compare_constructed_equal(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Ternary_should_not_evaluate_both_sides(bool isAsync)
        {
            await base.Ternary_should_not_evaluate_both_sides(isAsync);

            AssertSql(
                @"@__p_0='none' (Size = 4000)
@__p_1='none' (Size = 4000)
@__p_2='none' (Size = 4000)

SELECT [c].[CustomerID], @__p_0 AS [Data1], @__p_1 AS [Data2], @__p_2 AS [Data3]
FROM [Customers] AS [c]");
        }

        public override async Task Where_compare_constructed_multi_value_equal(bool isAsync)
        {
            await base.Where_compare_constructed_multi_value_equal(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Where_compare_constructed_multi_value_not_equal(bool isAsync)
        {
            await base.Where_compare_constructed_multi_value_not_equal(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Where_compare_tuple_constructed_equal(bool isAsync)
        {
            await base.Where_compare_tuple_constructed_equal(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Where_compare_tuple_constructed_multi_value_equal(bool isAsync)
        {
            await base.Where_compare_tuple_constructed_multi_value_equal(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Where_compare_tuple_constructed_multi_value_not_equal(bool isAsync)
        {
            await base.Where_compare_tuple_constructed_multi_value_not_equal(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Where_compare_tuple_create_constructed_equal(bool isAsync)
        {
            await base.Where_compare_tuple_create_constructed_equal(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Where_compare_tuple_create_constructed_multi_value_equal(bool isAsync)
        {
            await base.Where_compare_tuple_create_constructed_multi_value_equal(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Where_compare_tuple_create_constructed_multi_value_not_equal(bool isAsync)
        {
            await base.Where_compare_tuple_create_constructed_multi_value_not_equal(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Where_compare_null(bool isAsync)
        {
            await base.Where_compare_null(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] IS NULL AND ([c].[Country] = N'UK')");
        }

        public override async Task Where_Is_on_same_type(bool isAsync)
        {
            await base.Where_Is_on_same_type(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Where_chain(bool isAsync)
        {
            await base.Where_chain(isAsync);

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
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]");
        }

        public override async Task Where_array_index(bool isAsync)
        {
            await base.Where_array_index(isAsync);

            AssertSql(
                @"@__p_0='ALFKI' (Size = 5)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @__p_0");
        }

        public override async Task Where_multiple_contains_in_subquery_with_or(bool isAsync)
        {
            await base.Where_multiple_contains_in_subquery_with_or(isAsync);

            AssertSql(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
WHERE [od].[ProductID] IN (
    SELECT TOP(1) [p].[ProductID]
    FROM [Products] AS [p]
    ORDER BY [p].[ProductID]
) OR [od].[OrderID] IN (
    SELECT TOP(1) [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
)");
        }

        public override async Task Where_multiple_contains_in_subquery_with_and(bool isAsync)
        {
            await base.Where_multiple_contains_in_subquery_with_and(isAsync);

            AssertSql(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
WHERE [od].[ProductID] IN (
    SELECT TOP(20) [p].[ProductID]
    FROM [Products] AS [p]
    ORDER BY [p].[ProductID]
) AND [od].[OrderID] IN (
    SELECT TOP(10) [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
)");
        }

        public override async Task Where_contains_on_navigation(bool isAsync)
        {
            await base.Where_contains_on_navigation(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM [Customers] AS [c]
    WHERE [o].[OrderID] IN (
        SELECT [o0].[OrderID]
        FROM [Orders] AS [o0]
        WHERE [c].[CustomerID] = [o0].[CustomerID]
    ))");
        }

        public override async Task Where_subquery_FirstOrDefault_is_null(bool isAsync)
        {
            await base.Where_subquery_FirstOrDefault_is_null(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (
    SELECT TOP(1) [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID]
) IS NULL");
        }

        public override async Task Where_subquery_FirstOrDefault_compared_to_entity(bool isAsync)
        {
            await base.Where_subquery_FirstOrDefault_compared_to_entity(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (
    SELECT TOP(1) [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID]
) = 10243");
        }

        public override async Task Time_of_day_datetime(bool isAsync)
        {
            await base.Time_of_day_datetime(isAsync);

            AssertSql(@"SELECT CAST([c].[OrderDate] AS time)
FROM [Orders] AS [c]");
        }
    }
}
