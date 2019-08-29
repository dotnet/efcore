// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Query
{
    public partial class SimpleQuerySqlServerTest
    {
        public override async Task Where_simple(bool isAsync)
        {
            await base.Where_simple(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[City] = N'London') AND [c].[City] IS NOT NULL");
        }

        public override async Task Where_as_queryable_expression(bool isAsync)
        {
            await base.Where_as_queryable_expression(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE (([c].[CustomerID] = [o].[CustomerID]) AND [o].[CustomerID] IS NOT NULL) AND (([o].[CustomerID] = N'ALFKI') AND [o].[CustomerID] IS NOT NULL))");
        }

        public override async Task Where_simple_closure(bool isAsync)
        {
            await base.Where_simple_closure(isAsync);

            AssertSql(
                @"@__city_0='London' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (([c].[City] = @__city_0) AND ([c].[City] IS NOT NULL AND @__city_0 IS NOT NULL)) OR ([c].[City] IS NULL AND @__city_0 IS NULL)");
        }

        public override async Task Where_indexer_closure(bool isAsync)
        {
            await base.Where_indexer_closure(isAsync);

            AssertSql(
                @"@__p_0='London' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (([c].[City] = @__p_0) AND ([c].[City] IS NOT NULL AND @__p_0 IS NOT NULL)) OR ([c].[City] IS NULL AND @__p_0 IS NULL)");
        }

        public override async Task Where_dictionary_key_access_closure(bool isAsync)
        {
            await base.Where_dictionary_key_access_closure(isAsync);

            AssertSql(
                @"@__get_Item_0='London' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (([c].[City] = @__get_Item_0) AND ([c].[City] IS NOT NULL AND @__get_Item_0 IS NOT NULL)) OR ([c].[City] IS NULL AND @__get_Item_0 IS NULL)");
        }

        public override async Task Where_tuple_item_closure(bool isAsync)
        {
            await base.Where_tuple_item_closure(isAsync);

            AssertSql(
                @"@__predicateTuple_Item2_0='London' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (([c].[City] = @__predicateTuple_Item2_0) AND ([c].[City] IS NOT NULL AND @__predicateTuple_Item2_0 IS NOT NULL)) OR ([c].[City] IS NULL AND @__predicateTuple_Item2_0 IS NULL)");
        }

        public override async Task Where_named_tuple_item_closure(bool isAsync)
        {
            await base.Where_named_tuple_item_closure(isAsync);

            AssertSql(
                @"@__predicateTuple_Item2_0='London' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (([c].[City] = @__predicateTuple_Item2_0) AND ([c].[City] IS NOT NULL AND @__predicateTuple_Item2_0 IS NOT NULL)) OR ([c].[City] IS NULL AND @__predicateTuple_Item2_0 IS NULL)");
        }

        public override async Task Where_simple_closure_constant(bool isAsync)
        {
            await base.Where_simple_closure_constant(isAsync);

            AssertSql(
                @"@__predicate_0='True'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE @__predicate_0 = CAST(1 AS bit)");
        }

        public override async Task Where_simple_closure_via_query_cache(bool isAsync)
        {
            await base.Where_simple_closure_via_query_cache(isAsync);

            AssertSql(
                @"@__city_0='London' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (([c].[City] = @__city_0) AND ([c].[City] IS NOT NULL AND @__city_0 IS NOT NULL)) OR ([c].[City] IS NULL AND @__city_0 IS NULL)",
                //
                @"@__city_0='Seattle' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (([c].[City] = @__city_0) AND ([c].[City] IS NOT NULL AND @__city_0 IS NOT NULL)) OR ([c].[City] IS NULL AND @__city_0 IS NULL)");
        }

        public override async Task Where_method_call_nullable_type_closure_via_query_cache(bool isAsync)
        {
            await base.Where_method_call_nullable_type_closure_via_query_cache(isAsync);

            AssertSql(
                @"@__p_0='2' (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE ((CAST([e].[ReportsTo] AS bigint) = @__p_0) AND (CAST([e].[ReportsTo] AS bigint) IS NOT NULL AND @__p_0 IS NOT NULL)) OR (CAST([e].[ReportsTo] AS bigint) IS NULL AND @__p_0 IS NULL)",
                //
                @"@__p_0='5' (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE ((CAST([e].[ReportsTo] AS bigint) = @__p_0) AND (CAST([e].[ReportsTo] AS bigint) IS NOT NULL AND @__p_0 IS NOT NULL)) OR (CAST([e].[ReportsTo] AS bigint) IS NULL AND @__p_0 IS NULL)");
        }

        public override async Task Where_method_call_nullable_type_reverse_closure_via_query_cache(bool isAsync)
        {
            await base.Where_method_call_nullable_type_reverse_closure_via_query_cache(isAsync);

            AssertSql(
                @"@__p_0='1' (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE CAST([e].[EmployeeID] AS bigint) > @__p_0",
                //
                @"@__p_0='5' (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE CAST([e].[EmployeeID] AS bigint) > @__p_0");
        }

        public override async Task Where_method_call_closure_via_query_cache(bool isAsync)
        {
            await base.Where_method_call_closure_via_query_cache(isAsync);

            AssertSql(
                @"@__GetCity_0='London' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (([c].[City] = @__GetCity_0) AND ([c].[City] IS NOT NULL AND @__GetCity_0 IS NOT NULL)) OR ([c].[City] IS NULL AND @__GetCity_0 IS NULL)",
                //
                @"@__GetCity_0='Seattle' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (([c].[City] = @__GetCity_0) AND ([c].[City] IS NOT NULL AND @__GetCity_0 IS NOT NULL)) OR ([c].[City] IS NULL AND @__GetCity_0 IS NULL)");
        }

        public override async Task Where_field_access_closure_via_query_cache(bool isAsync)
        {
            await base.Where_field_access_closure_via_query_cache(isAsync);

            AssertSql(
                @"@__city_InstanceFieldValue_0='London' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (([c].[City] = @__city_InstanceFieldValue_0) AND ([c].[City] IS NOT NULL AND @__city_InstanceFieldValue_0 IS NOT NULL)) OR ([c].[City] IS NULL AND @__city_InstanceFieldValue_0 IS NULL)",
                //
                @"@__city_InstanceFieldValue_0='Seattle' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (([c].[City] = @__city_InstanceFieldValue_0) AND ([c].[City] IS NOT NULL AND @__city_InstanceFieldValue_0 IS NOT NULL)) OR ([c].[City] IS NULL AND @__city_InstanceFieldValue_0 IS NULL)");
        }

        public override async Task Where_property_access_closure_via_query_cache(bool isAsync)
        {
            await base.Where_property_access_closure_via_query_cache(isAsync);

            AssertSql(
                @"@__city_InstancePropertyValue_0='London' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (([c].[City] = @__city_InstancePropertyValue_0) AND ([c].[City] IS NOT NULL AND @__city_InstancePropertyValue_0 IS NOT NULL)) OR ([c].[City] IS NULL AND @__city_InstancePropertyValue_0 IS NULL)",
                //
                @"@__city_InstancePropertyValue_0='Seattle' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (([c].[City] = @__city_InstancePropertyValue_0) AND ([c].[City] IS NOT NULL AND @__city_InstancePropertyValue_0 IS NOT NULL)) OR ([c].[City] IS NULL AND @__city_InstancePropertyValue_0 IS NULL)");
        }

        public override async Task Where_static_field_access_closure_via_query_cache(bool isAsync)
        {
            await base.Where_static_field_access_closure_via_query_cache(isAsync);

            AssertSql(
                @"@__StaticFieldValue_0='London' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (([c].[City] = @__StaticFieldValue_0) AND ([c].[City] IS NOT NULL AND @__StaticFieldValue_0 IS NOT NULL)) OR ([c].[City] IS NULL AND @__StaticFieldValue_0 IS NULL)",
                //
                @"@__StaticFieldValue_0='Seattle' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (([c].[City] = @__StaticFieldValue_0) AND ([c].[City] IS NOT NULL AND @__StaticFieldValue_0 IS NOT NULL)) OR ([c].[City] IS NULL AND @__StaticFieldValue_0 IS NULL)");
        }

        public override async Task Where_static_property_access_closure_via_query_cache(bool isAsync)
        {
            await base.Where_static_property_access_closure_via_query_cache(isAsync);

            AssertSql(
                @"@__StaticPropertyValue_0='London' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (([c].[City] = @__StaticPropertyValue_0) AND ([c].[City] IS NOT NULL AND @__StaticPropertyValue_0 IS NOT NULL)) OR ([c].[City] IS NULL AND @__StaticPropertyValue_0 IS NULL)",
                //
                @"@__StaticPropertyValue_0='Seattle' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (([c].[City] = @__StaticPropertyValue_0) AND ([c].[City] IS NOT NULL AND @__StaticPropertyValue_0 IS NOT NULL)) OR ([c].[City] IS NULL AND @__StaticPropertyValue_0 IS NULL)");
        }

        public override async Task Where_nested_field_access_closure_via_query_cache(bool isAsync)
        {
            await base.Where_nested_field_access_closure_via_query_cache(isAsync);

            AssertSql(
                @"@__city_Nested_InstanceFieldValue_0='London' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (([c].[City] = @__city_Nested_InstanceFieldValue_0) AND ([c].[City] IS NOT NULL AND @__city_Nested_InstanceFieldValue_0 IS NOT NULL)) OR ([c].[City] IS NULL AND @__city_Nested_InstanceFieldValue_0 IS NULL)",
                //
                @"@__city_Nested_InstanceFieldValue_0='Seattle' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (([c].[City] = @__city_Nested_InstanceFieldValue_0) AND ([c].[City] IS NOT NULL AND @__city_Nested_InstanceFieldValue_0 IS NOT NULL)) OR ([c].[City] IS NULL AND @__city_Nested_InstanceFieldValue_0 IS NULL)");
        }

        public override async Task Where_nested_property_access_closure_via_query_cache(bool isAsync)
        {
            await base.Where_nested_property_access_closure_via_query_cache(isAsync);

            AssertSql(
                @"@__city_Nested_InstancePropertyValue_0='London' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (([c].[City] = @__city_Nested_InstancePropertyValue_0) AND ([c].[City] IS NOT NULL AND @__city_Nested_InstancePropertyValue_0 IS NOT NULL)) OR ([c].[City] IS NULL AND @__city_Nested_InstancePropertyValue_0 IS NULL)",
                //
                @"@__city_Nested_InstancePropertyValue_0='Seattle' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (([c].[City] = @__city_Nested_InstancePropertyValue_0) AND ([c].[City] IS NOT NULL AND @__city_Nested_InstancePropertyValue_0 IS NOT NULL)) OR ([c].[City] IS NULL AND @__city_Nested_InstancePropertyValue_0 IS NULL)");
        }

        public override async Task Where_new_instance_field_access_query_cache(bool isAsync)
        {
            await base.Where_new_instance_field_access_query_cache(isAsync);

            AssertSql(
                @"@__InstanceFieldValue_0='London' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (([c].[City] = @__InstanceFieldValue_0) AND ([c].[City] IS NOT NULL AND @__InstanceFieldValue_0 IS NOT NULL)) OR ([c].[City] IS NULL AND @__InstanceFieldValue_0 IS NULL)",
                //
                @"@__InstanceFieldValue_0='Seattle' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (([c].[City] = @__InstanceFieldValue_0) AND ([c].[City] IS NOT NULL AND @__InstanceFieldValue_0 IS NOT NULL)) OR ([c].[City] IS NULL AND @__InstanceFieldValue_0 IS NULL)");
        }

        public override async Task Where_new_instance_field_access_closure_via_query_cache(bool isAsync)
        {
            await base.Where_new_instance_field_access_closure_via_query_cache(isAsync);

            AssertSql(
                @"@__InstanceFieldValue_0='London' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (([c].[City] = @__InstanceFieldValue_0) AND ([c].[City] IS NOT NULL AND @__InstanceFieldValue_0 IS NOT NULL)) OR ([c].[City] IS NULL AND @__InstanceFieldValue_0 IS NULL)",
                //
                @"@__InstanceFieldValue_0='Seattle' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (([c].[City] = @__InstanceFieldValue_0) AND ([c].[City] IS NOT NULL AND @__InstanceFieldValue_0 IS NOT NULL)) OR ([c].[City] IS NULL AND @__InstanceFieldValue_0 IS NULL)");
        }

        public override async Task Where_simple_closure_via_query_cache_nullable_type(bool isAsync)
        {
            await base.Where_simple_closure_via_query_cache_nullable_type(isAsync);

            AssertSql(
                @"@__p_0='2' (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE ((CAST([e].[ReportsTo] AS bigint) = @__p_0) AND (CAST([e].[ReportsTo] AS bigint) IS NOT NULL AND @__p_0 IS NOT NULL)) OR (CAST([e].[ReportsTo] AS bigint) IS NULL AND @__p_0 IS NULL)",
                //
                @"@__p_0='5' (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE ((CAST([e].[ReportsTo] AS bigint) = @__p_0) AND (CAST([e].[ReportsTo] AS bigint) IS NOT NULL AND @__p_0 IS NOT NULL)) OR (CAST([e].[ReportsTo] AS bigint) IS NULL AND @__p_0 IS NULL)",
                //
                @"@__p_0='' (DbType = Int64)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE ((CAST([e].[ReportsTo] AS bigint) = @__p_0) AND (CAST([e].[ReportsTo] AS bigint) IS NOT NULL AND @__p_0 IS NOT NULL)) OR (CAST([e].[ReportsTo] AS bigint) IS NULL AND @__p_0 IS NULL)");
        }

        public override async Task Where_simple_closure_via_query_cache_nullable_type_reverse(bool isAsync)
        {
            await base.Where_simple_closure_via_query_cache_nullable_type_reverse(isAsync);

            AssertSql(
                @"@__p_0='' (DbType = Int64)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE ((CAST([e].[ReportsTo] AS bigint) = @__p_0) AND (CAST([e].[ReportsTo] AS bigint) IS NOT NULL AND @__p_0 IS NOT NULL)) OR (CAST([e].[ReportsTo] AS bigint) IS NULL AND @__p_0 IS NULL)",
                //
                @"@__p_0='5' (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE ((CAST([e].[ReportsTo] AS bigint) = @__p_0) AND (CAST([e].[ReportsTo] AS bigint) IS NOT NULL AND @__p_0 IS NOT NULL)) OR (CAST([e].[ReportsTo] AS bigint) IS NULL AND @__p_0 IS NULL)",
                //
                @"@__p_0='2' (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE ((CAST([e].[ReportsTo] AS bigint) = @__p_0) AND (CAST([e].[ReportsTo] AS bigint) IS NOT NULL AND @__p_0 IS NOT NULL)) OR (CAST([e].[ReportsTo] AS bigint) IS NULL AND @__p_0 IS NULL)");
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
    WHERE ((([o].[CustomerID] = @__customerID_0) AND ([o].[CustomerID] IS NOT NULL AND @__customerID_0 IS NOT NULL)) OR ([o].[CustomerID] IS NULL AND @__customerID_0 IS NULL)) AND (([o].[CustomerID] = [c].[CustomerID]) AND [o].[CustomerID] IS NOT NULL))",
                //
                @"@__customerID_0='ANATR' (Size = 5)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE ((([o].[CustomerID] = @__customerID_0) AND ([o].[CustomerID] IS NOT NULL AND @__customerID_0 IS NOT NULL)) OR ([o].[CustomerID] IS NULL AND @__customerID_0 IS NULL)) AND (([o].[CustomerID] = [c].[CustomerID]) AND [o].[CustomerID] IS NOT NULL))");
        }

        public override async Task Where_bitwise_or(bool isAsync)
        {
            await base.Where_bitwise_or(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (CASE
    WHEN [c].[CustomerID] = N'ALFKI' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END | CASE
    WHEN [c].[CustomerID] = N'ANATR' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END) = CAST(1 AS bit)");
        }

        public override async Task Where_bitwise_and(bool isAsync)
        {
            await base.Where_bitwise_and(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (CASE
    WHEN [c].[CustomerID] = N'ALFKI' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END & CASE
    WHEN [c].[CustomerID] = N'ANATR' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END) = CAST(1 AS bit)");
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
WHERE ([e].[Title] = N'Sales Representative') AND [e].[Title] IS NOT NULL");
        }

        public override async Task Where_simple_shadow_projection(bool isAsync)
        {
            await base.Where_simple_shadow_projection(isAsync);

            AssertSql(
                @"SELECT [e].[Title]
FROM [Employees] AS [e]
WHERE ([e].[Title] = N'Sales Representative') AND [e].[Title] IS NOT NULL");
        }

        public override async Task Where_shadow_subquery_FirstOrDefault(bool isAsync)
        {
            await base.Where_shadow_subquery_FirstOrDefault(isAsync);

            // issue #15994
//            AssertSql(
//                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
//FROM [Employees] AS [e]
//WHERE [e].[Title] = (
//    SELECT TOP(1) [e2].[Title]
//    FROM [Employees] AS [e2]
//    ORDER BY [e2].[Title]
//)");
        }

        public override async Task Where_subquery_correlated(bool isAsync)
        {
            await base.Where_subquery_correlated(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Customers] AS [c0]
    WHERE [c].[CustomerID] = [c0].[CustomerID])");
        }

        public override async Task Where_equals_method_string(bool isAsync)
        {
            await base.Where_equals_method_string(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[City] = N'London') AND [c].[City] IS NOT NULL");
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
WHERE CAST(0 AS bit) = CAST(1 AS bit)");

            //Assert.Contains(
            //    RelationalStrings.LogPossibleUnintendedUseOfEquals.GenerateMessage(
            //        "e.EmployeeID.Equals(Convert(__longPrm_0, Object))"), Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));
        }

        public override async Task Where_equals_using_int_overload_on_mismatched_types(bool isAsync)
        {
            await base.Where_equals_using_int_overload_on_mismatched_types(isAsync);

            AssertSql(
                @"@__p_0='1'

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE ([e].[EmployeeID] = @__p_0) AND @__p_0 IS NOT NULL");
        }

        public override async Task Where_equals_on_mismatched_types_nullable_int_long(bool isAsync)
        {
            await base.Where_equals_on_mismatched_types_nullable_int_long(isAsync);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE CAST(0 AS bit) = CAST(1 AS bit)",
                //
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE CAST(0 AS bit) = CAST(1 AS bit)");

            //Assert.Contains(
            //    RelationalStrings.LogPossibleUnintendedUseOfEquals.GenerateMessage(
            //        "__longPrm_0.Equals(Convert(e.ReportsTo, Object))"), Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));

            //Assert.Contains(
            //    RelationalStrings.LogPossibleUnintendedUseOfEquals.GenerateMessage(
            //        "e.ReportsTo.Equals(Convert(__longPrm_0, Object))"), Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));
        }

        public override async Task Where_equals_on_mismatched_types_nullable_long_nullable_int(bool isAsync)
        {
            await base.Where_equals_on_mismatched_types_nullable_long_nullable_int(isAsync);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE CAST(0 AS bit) = CAST(1 AS bit)",
                //
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE CAST(0 AS bit) = CAST(1 AS bit)");

            //Assert.Contains(
            //    RelationalStrings.LogPossibleUnintendedUseOfEquals.GenerateMessage(
            //        "__nullableLongPrm_0.Equals(Convert(e.ReportsTo, Object))"),
            //    Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));

            //Assert.Contains(
            //    RelationalStrings.LogPossibleUnintendedUseOfEquals.GenerateMessage(
            //        "e.ReportsTo.Equals(Convert(__nullableLongPrm_0, Object))"),
            //    Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));
        }

        public override async Task Where_equals_on_mismatched_types_int_nullable_int(bool isAsync)
        {
            await base.Where_equals_on_mismatched_types_int_nullable_int(isAsync);

            AssertSql(
                @"@__intPrm_0='2'

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE (([e].[ReportsTo] = @__intPrm_0) AND ([e].[ReportsTo] IS NOT NULL AND @__intPrm_0 IS NOT NULL)) OR ([e].[ReportsTo] IS NULL AND @__intPrm_0 IS NULL)",
                //
                @"@__intPrm_0='2'

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE ((@__intPrm_0 = [e].[ReportsTo]) AND (@__intPrm_0 IS NOT NULL AND [e].[ReportsTo] IS NOT NULL)) OR (@__intPrm_0 IS NULL AND [e].[ReportsTo] IS NULL)");
        }

        public override async Task Where_equals_on_matched_nullable_int_types(bool isAsync)
        {
            await base.Where_equals_on_matched_nullable_int_types(isAsync);

            AssertSql(
                @"@__nullableIntPrm_0='2' (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE ((@__nullableIntPrm_0 = [e].[ReportsTo]) AND (@__nullableIntPrm_0 IS NOT NULL AND [e].[ReportsTo] IS NOT NULL)) OR (@__nullableIntPrm_0 IS NULL AND [e].[ReportsTo] IS NULL)",
                //
                @"@__nullableIntPrm_0='2' (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE (([e].[ReportsTo] = @__nullableIntPrm_0) AND ([e].[ReportsTo] IS NOT NULL AND @__nullableIntPrm_0 IS NOT NULL)) OR ([e].[ReportsTo] IS NULL AND @__nullableIntPrm_0 IS NULL)");
        }

        public override async Task Where_equals_on_null_nullable_int_types(bool isAsync)
        {
            await base.Where_equals_on_null_nullable_int_types(isAsync);

            AssertSql(
                @"@__nullableIntPrm_0='' (DbType = Int32)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE ((@__nullableIntPrm_0 = [e].[ReportsTo]) AND (@__nullableIntPrm_0 IS NOT NULL AND [e].[ReportsTo] IS NOT NULL)) OR (@__nullableIntPrm_0 IS NULL AND [e].[ReportsTo] IS NULL)",
                //
                @"@__nullableIntPrm_0='' (DbType = Int64)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE (([e].[ReportsTo] = @__nullableIntPrm_0) AND ([e].[ReportsTo] IS NOT NULL AND @__nullableIntPrm_0 IS NOT NULL)) OR ([e].[ReportsTo] IS NULL AND @__nullableIntPrm_0 IS NULL)");
        }

        public override async Task Where_comparison_nullable_type_not_null(bool isAsync)
        {
            await base.Where_comparison_nullable_type_not_null(isAsync);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE ([e].[ReportsTo] = 2) AND [e].[ReportsTo] IS NOT NULL");
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
WHERE (CAST(LEN([c].[City]) AS int) = 6) AND CAST(LEN([c].[City]) AS int) IS NOT NULL");
        }

        public override async Task Where_string_indexof(bool isAsync)
        {
            await base.Where_string_indexof(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (CASE
    WHEN N'Sea' = N'' THEN 0
    ELSE CHARINDEX(N'Sea', [c].[City]) - 1
END <> -1) OR CASE
    WHEN N'Sea' = N'' THEN 0
    ELSE CHARINDEX(N'Sea', [c].[City]) - 1
END IS NULL");
        }

        public override async Task Where_string_replace(bool isAsync)
        {
            await base.Where_string_replace(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (REPLACE([c].[City], N'Sea', N'Rea') = N'Reattle') AND REPLACE([c].[City], N'Sea', N'Rea') IS NOT NULL");
        }

        public override async Task Where_string_substring(bool isAsync)
        {
            await base.Where_string_substring(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (SUBSTRING([c].[City], 1 + 1, 2) = N'ea') AND SUBSTRING([c].[City], 1 + 1, 2) IS NOT NULL");
        }

        public override async Task Where_datetime_now(bool isAsync)
        {
            await base.Where_datetime_now(isAsync);

            // issue #15994
//            AssertSql(
//                @"@__myDatetime_0='2015-04-10T00:00:00'

//SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE GETDATE() <> @__myDatetime_0");
        }

        public override async Task Where_datetime_utcnow(bool isAsync)
        {
            await base.Where_datetime_utcnow(isAsync);

            // issue #15994
//            AssertSql(
//                @"@__myDatetime_0='2015-04-10T00:00:00'

//SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE GETUTCDATE() <> @__myDatetime_0");
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
WHERE ((CONVERT(date, [o].[OrderDate]) = @__myDatetime_0) AND (CONVERT(date, [o].[OrderDate]) IS NOT NULL AND @__myDatetime_0 IS NOT NULL)) OR (CONVERT(date, [o].[OrderDate]) IS NULL AND @__myDatetime_0 IS NULL)");
        }

        public override async Task Where_date_add_year_constant_component(bool isAsync)
        {
            await base.Where_date_add_year_constant_component(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE (DATEPART(year, DATEADD(year, CAST(-1 AS int), [o].[OrderDate])) = 1997) AND DATEPART(year, DATEADD(year, CAST(-1 AS int), [o].[OrderDate])) IS NOT NULL");
        }

        public override async Task Where_datetime_year_component(bool isAsync)
        {
            await base.Where_datetime_year_component(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE (DATEPART(year, [o].[OrderDate]) = 1998) AND DATEPART(year, [o].[OrderDate]) IS NOT NULL");
        }

        public override async Task Where_datetime_month_component(bool isAsync)
        {
            await base.Where_datetime_month_component(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE (DATEPART(month, [o].[OrderDate]) = 4) AND DATEPART(month, [o].[OrderDate]) IS NOT NULL");
        }

        public override async Task Where_datetime_dayOfYear_component(bool isAsync)
        {
            await base.Where_datetime_dayOfYear_component(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE (DATEPART(dayofyear, [o].[OrderDate]) = 68) AND DATEPART(dayofyear, [o].[OrderDate]) IS NOT NULL");
        }

        public override async Task Where_datetime_day_component(bool isAsync)
        {
            await base.Where_datetime_day_component(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE (DATEPART(day, [o].[OrderDate]) = 4) AND DATEPART(day, [o].[OrderDate]) IS NOT NULL");
        }

        public override async Task Where_datetime_hour_component(bool isAsync)
        {
            await base.Where_datetime_hour_component(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE (DATEPART(hour, [o].[OrderDate]) = 14) AND DATEPART(hour, [o].[OrderDate]) IS NOT NULL");
        }

        public override async Task Where_datetime_minute_component(bool isAsync)
        {
            await base.Where_datetime_minute_component(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE (DATEPART(minute, [o].[OrderDate]) = 23) AND DATEPART(minute, [o].[OrderDate]) IS NOT NULL");
        }

        public override async Task Where_datetime_second_component(bool isAsync)
        {
            await base.Where_datetime_second_component(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE (DATEPART(second, [o].[OrderDate]) = 44) AND DATEPART(second, [o].[OrderDate]) IS NOT NULL");
        }

        public override async Task Where_datetime_millisecond_component(bool isAsync)
        {
            await base.Where_datetime_millisecond_component(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE (DATEPART(millisecond, [o].[OrderDate]) = 88) AND DATEPART(millisecond, [o].[OrderDate]) IS NOT NULL");
        }

        public override async Task Where_datetimeoffset_now_component(bool isAsync)
        {
            await base.Where_datetimeoffset_now_component(isAsync);

            // issue #15994
//            AssertSql(
//                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
//FROM [Orders] AS [o]
//WHERE [o].[OrderDate] = SYSDATETIMEOFFSET()");
        }

        public override async Task Where_datetimeoffset_utcnow_component(bool isAsync)
        {
            await base.Where_datetimeoffset_utcnow_component(isAsync);

            // issue #15994
//            AssertSql(
//                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
//FROM [Orders] AS [o]
//WHERE [o].[OrderDate] = CAST(SYSUTCDATETIME() AS datetimeoffset)");
        }

        public override async Task Where_simple_reversed(bool isAsync)
        {
            await base.Where_simple_reversed(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (N'London' = [c].[City]) AND [c].[City] IS NOT NULL");
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
WHERE CAST(0 AS bit) = CAST(1 AS bit)");
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
WHERE CAST(0 AS bit) = CAST(1 AS bit)");
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
WHERE (([c].[City] = [c].[City]) AND ([c].[City] IS NOT NULL AND [c].[City] IS NOT NULL)) OR ([c].[City] IS NULL AND [c].[City] IS NULL)");
        }

        public override async Task Where_in_optimization_multiple(bool isAsync)
        {
            await base.Where_in_optimization_multiple(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE (((([c].[City] = N'London') AND [c].[City] IS NOT NULL) OR (([c].[City] = N'Berlin') AND [c].[City] IS NOT NULL)) OR ([c].[CustomerID] = N'ALFKI')) OR ([c].[CustomerID] = N'ABCDE')");
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
WHERE (([c].[City] <> N'London') OR [c].[City] IS NULL) AND (([c].[City] <> N'Berlin') OR [c].[City] IS NULL)");
        }

        public override async Task Where_not_in_optimization3(bool isAsync)
        {
            await base.Where_not_in_optimization3(isAsync);

            AssertSql(
    @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE ((([c].[City] <> N'London') OR [c].[City] IS NULL) AND (([c].[City] <> N'Berlin') OR [c].[City] IS NULL)) AND (([c].[City] <> N'Seattle') OR [c].[City] IS NULL)");
        }

        public override async Task Where_not_in_optimization4(bool isAsync)
        {
            await base.Where_not_in_optimization4(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE (((([c].[City] <> N'London') OR [c].[City] IS NULL) AND (([c].[City] <> N'Berlin') OR [c].[City] IS NULL)) AND (([c].[City] <> N'Seattle') OR [c].[City] IS NULL)) AND (([c].[City] <> N'Lisboa') OR [c].[City] IS NULL)");
        }

        public override async Task Where_select_many_and(bool isAsync)
        {
            await base.Where_select_many_and(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE ((([c].[City] = N'London') AND [c].[City] IS NOT NULL) AND (([c].[Country] = N'UK') AND [c].[Country] IS NOT NULL)) AND ((([e].[City] = N'London') AND [e].[City] IS NOT NULL) AND (([e].[Country] = N'UK') AND [e].[Country] IS NOT NULL))");
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
WHERE [p].[Discontinued] = CAST(1 AS bit)");
        }

        public override async Task Where_bool_member_false(bool isAsync)
        {
            await base.Where_bool_member_false(isAsync);

            // issue #15994
//            AssertSql(
//                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
//FROM [Products] AS [p]
//WHERE [p].[Discontinued] = CAST(0 AS bit)");
        }

        //        public override async Task Where_bool_client_side_negated(bool isAsync)
        //        {
        //            await base.Where_bool_client_side_negated(isAsync);

        //            AssertSql(
        //                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
        //FROM [Products] AS [p]
        //WHERE [p].[Discontinued] = 1");
        //        }

        public override async Task Where_bool_member_negated_twice(bool isAsync)
        {
            await base.Where_bool_member_negated_twice(isAsync);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = CAST(1 AS bit)");
        }

        public override async Task Where_bool_member_shadow(bool isAsync)
        {
            await base.Where_bool_member_shadow(isAsync);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = CAST(1 AS bit)");
        }

        public override async Task Where_bool_member_false_shadow(bool isAsync)
        {
            await base.Where_bool_member_false_shadow(isAsync);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] <> CAST(1 AS bit)");
        }

        public override async Task Where_bool_member_equals_constant(bool isAsync)
        {
            await base.Where_bool_member_equals_constant(isAsync);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = CAST(1 AS bit)");
        }

        public override async Task Where_bool_member_in_complex_predicate(bool isAsync)
        {
            await base.Where_bool_member_in_complex_predicate(isAsync);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE (([p].[ProductID] > 100) AND ([p].[Discontinued] = CAST(1 AS bit))) OR ([p].[Discontinued] = CAST(1 AS bit))");
        }

        public override async Task Where_bool_member_compared_to_binary_expression(bool isAsync)
        {
            await base.Where_bool_member_compared_to_binary_expression(isAsync);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = CASE
    WHEN [p].[ProductID] > 50 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
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
    WHEN [p].[ProductID] > 50 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END = CASE
    WHEN [p].[ProductID] > 20 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task Where_not_bool_member_compared_to_binary_expression(bool isAsync)
        {
            await base.Where_not_bool_member_compared_to_binary_expression(isAsync);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] <> CASE
    WHEN [p].[ProductID] > 50 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task Where_bool_parameter(bool isAsync)
        {
            await base.Where_bool_parameter(isAsync);

            AssertSql(
                @"@__prm_0='True'

SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE @__prm_0 = CAST(1 AS bit)");
        }

        public override async Task Where_bool_parameter_compared_to_binary_expression(bool isAsync)
        {
            await base.Where_bool_parameter_compared_to_binary_expression(isAsync);

            AssertSql(
                @"@__prm_0='True'

SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE (CASE
    WHEN [p].[ProductID] > 50 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END <> @__prm_0) OR @__prm_0 IS NULL");
        }

        public override async Task Where_bool_member_and_parameter_compared_to_binary_expression_nested(bool isAsync)
        {
            await base.Where_bool_member_and_parameter_compared_to_binary_expression_nested(isAsync);

            AssertSql(
                @"@__prm_0='True'

SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = CASE
    WHEN (CASE
        WHEN [p].[ProductID] > 50 THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END <> @__prm_0) OR @__prm_0 IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task Where_de_morgan_or_optimized(bool isAsync)
        {
            await base.Where_de_morgan_or_optimized(isAsync);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE ([p].[Discontinued] <> CAST(1 AS bit)) AND ([p].[ProductID] >= 20)");
        }

        public override async Task Where_de_morgan_and_optimized(bool isAsync)
        {
            await base.Where_de_morgan_and_optimized(isAsync);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE ([p].[Discontinued] <> CAST(1 AS bit)) OR ([p].[ProductID] >= 20)");
        }

        public override async Task Where_complex_negated_expression_optimized(bool isAsync)
        {
            await base.Where_complex_negated_expression_optimized(isAsync);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE (([p].[Discontinued] <> CAST(1 AS bit)) AND ([p].[ProductID] < 60)) AND ([p].[ProductID] > 30)");
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
WHERE [c].[CustomerID] LIKE N'%KI'");
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
WHERE CAST(0 AS bit) = CAST(1 AS bit)");
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
WHERE ((([c].[CustomerID] + CAST(@__i_0 AS nchar(5))) = [c].[CompanyName]) AND ([c].[CustomerID] + CAST(@__i_0 AS nchar(5)) IS NOT NULL AND [c].[CompanyName] IS NOT NULL)) OR ([c].[CustomerID] + CAST(@__i_0 AS nchar(5)) IS NULL AND [c].[CompanyName] IS NULL)");
        }

        public override async Task Where_concat_string_int_comparison2(bool isAsync)
        {
            await base.Where_concat_string_int_comparison2(isAsync);

            AssertSql(
                @"@__i_0='10'

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE (((CAST(@__i_0 AS nchar(5)) + [c].[CustomerID]) = [c].[CompanyName]) AND (CAST(@__i_0 AS nchar(5)) + [c].[CustomerID] IS NOT NULL AND [c].[CompanyName] IS NOT NULL)) OR (CAST(@__i_0 AS nchar(5)) + [c].[CustomerID] IS NULL AND [c].[CompanyName] IS NULL)");
        }

        public override async Task Where_concat_string_int_comparison3(bool isAsync)
        {
            await base.Where_concat_string_int_comparison3(isAsync);

            AssertSql(
                @"@__p_0='30'
@__j_1='21'

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE (((((CAST(@__p_0 AS nchar(5)) + [c].[CustomerID]) + CAST(@__j_1 AS nchar(5))) + CAST(42 AS nchar(5))) = [c].[CompanyName]) AND (((CAST(@__p_0 AS nchar(5)) + [c].[CustomerID]) + CAST(@__j_1 AS nchar(5))) + CAST(42 AS nchar(5)) IS NOT NULL AND [c].[CompanyName] IS NOT NULL)) OR (((CAST(@__p_0 AS nchar(5)) + [c].[CustomerID]) + CAST(@__j_1 AS nchar(5))) + CAST(42 AS nchar(5)) IS NULL AND [c].[CompanyName] IS NULL)");
        }

        public override async Task Where_concat_string_int_comparison4(bool isAsync)
        {
            await base.Where_concat_string_int_comparison4(isAsync);

            AssertSql(
                @"SELECT [o].[CustomerID]
FROM [Orders] AS [o]
WHERE (((CAST([o].[OrderID] AS nchar(5)) + [o].[CustomerID]) = [o].[CustomerID]) AND (CAST([o].[OrderID] AS nchar(5)) + [o].[CustomerID] IS NOT NULL AND [o].[CustomerID] IS NOT NULL)) OR (CAST([o].[OrderID] AS nchar(5)) + [o].[CustomerID] IS NULL AND [o].[CustomerID] IS NULL)");
        }

        public override async Task Where_concat_string_string_comparison(bool isAsync)
        {
            await base.Where_concat_string_string_comparison(isAsync);

            AssertSql(
                @"@__i_0='A' (Size = 5)

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE (((@__i_0 + [c].[CustomerID]) = [c].[CompanyName]) AND (@__i_0 + [c].[CustomerID] IS NOT NULL AND [c].[CompanyName] IS NOT NULL)) OR (@__i_0 + [c].[CustomerID] IS NULL AND [c].[CompanyName] IS NULL)");
        }

        public override async Task Where_string_concat_method_comparison(bool isAsync)
        {
            await base.Where_string_concat_method_comparison(isAsync);

            AssertSql(
                @"@__i_0='A' (Size = 5)

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE (((@__i_0 + [c].[CustomerID]) = [c].[CompanyName]) AND (@__i_0 + [c].[CustomerID] IS NOT NULL AND [c].[CompanyName] IS NOT NULL)) OR (@__i_0 + [c].[CustomerID] IS NULL AND [c].[CompanyName] IS NULL)");
        }

        public override async Task Where_ternary_boolean_condition_true(bool isAsync)
        {
            await base.Where_ternary_boolean_condition_true(isAsync);

            AssertSql(
                @"@__flag_0='True'

SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE CASE
    WHEN @__flag_0 = CAST(1 AS bit) THEN CASE
        WHEN [p].[UnitsInStock] >= CAST(20 AS smallint) THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END
    ELSE CASE
        WHEN [p].[UnitsInStock] < CAST(20 AS smallint) THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END
END = CAST(1 AS bit)");
        }

        public override async Task Where_ternary_boolean_condition_false(bool isAsync)
        {
            await base.Where_ternary_boolean_condition_false(isAsync);

            // issue #15994
//            AssertSql(
//                @"@__flag_0='False'

//SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
//FROM [Products] AS [p]
//WHERE ((@__flag_0 = CAST(1 AS bit)) AND ([p].[UnitsInStock] >= CAST(20 AS smallint))) OR ((@__flag_0 <> CAST(1 AS bit)) AND ([p].[UnitsInStock] < CAST(20 AS smallint)))");
        }

        public override async Task Where_ternary_boolean_condition_with_another_condition(bool isAsync)
        {
            await base.Where_ternary_boolean_condition_with_another_condition(isAsync);

            // issue #15994
//            AssertSql(
//                @"@__productId_0='15'
//@__flag_1='True'

//SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
//FROM [Products] AS [p]
//WHERE ([p].[ProductID] < @__productId_0) AND (((@__flag_1 = CAST(1 AS bit)) AND ([p].[UnitsInStock] >= CAST(20 AS smallint))) OR ((@__flag_1 <> CAST(1 AS bit)) AND ([p].[UnitsInStock] < CAST(20 AS smallint))))");
        }

        public override async Task Where_ternary_boolean_condition_with_false_as_result_true(bool isAsync)
        {
            await base.Where_ternary_boolean_condition_with_false_as_result_true(isAsync);

            // issue #15994
//            AssertSql(
//                @"@__flag_0='True'

//SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
//FROM [Products] AS [p]
//WHERE (@__flag_0 = CAST(1 AS bit)) AND ([p].[UnitsInStock] >= CAST(20 AS smallint))");
        }

        public override async Task Where_ternary_boolean_condition_with_false_as_result_false(bool isAsync)
        {
            await base.Where_ternary_boolean_condition_with_false_as_result_false(isAsync);

            // issue #15994
//            AssertSql(
//                @"@__flag_0='False'

//SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
//FROM [Products] AS [p]
//WHERE (@__flag_0 = CAST(1 AS bit)) AND ([p].[UnitsInStock] >= CAST(20 AS smallint))");
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
WHERE [c].[City] IS NULL AND (([c].[Country] = N'UK') AND [c].[Country] IS NOT NULL)");
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
WHERE (([o].[CustomerID] = N'QUICK') AND [o].[CustomerID] IS NOT NULL) AND ([o].[OrderDate] > '1998-01-01T00:00:00.000')");
        }

        public override void Where_navigation_contains()
        {
            base.Where_navigation_contains();

            AssertSql(
                @"SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] = N'ALFKI'
) AS [t]
LEFT JOIN [Orders] AS [o] ON [t].[CustomerID] = [o].[CustomerID]
ORDER BY [t].[CustomerID], [o].[OrderID]",
                //
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
WHERE [o0].[OrderID] IN (10643, 10692, 10702, 10835, 10952, 11011)");
        }

        public override async Task Where_array_index(bool isAsync)
        {
            await base.Where_array_index(isAsync);

            AssertSql(
                @"@__p_0='ALFKI' (Size = 5)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[CustomerID] = @__p_0) AND @__p_0 IS NOT NULL");
        }

        public override async Task Where_multiple_contains_in_subquery_with_or(bool isAsync)
        {
            await base.Where_multiple_contains_in_subquery_with_or(isAsync);

            // issue #15994
//            AssertSql(
//                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
//FROM [Order Details] AS [od]
//WHERE [od].[ProductID] IN (
//    SELECT TOP(1) [p].[ProductID]
//    FROM [Products] AS [p]
//    ORDER BY [p].[ProductID]
//) OR [od].[OrderID] IN (
//    SELECT TOP(1) [o].[OrderID]
//    FROM [Orders] AS [o]
//    ORDER BY [o].[OrderID]
//)");
        }

        public override async Task Where_multiple_contains_in_subquery_with_and(bool isAsync)
        {
            await base.Where_multiple_contains_in_subquery_with_and(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE [o].[ProductID] IN (
    SELECT TOP(20) [p].[ProductID]
    FROM [Products] AS [p]
    ORDER BY [p].[ProductID]
)
 AND [o].[OrderID] IN (
    SELECT TOP(10) [o0].[OrderID]
    FROM [Orders] AS [o0]
    ORDER BY [o0].[OrderID]
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
        WHERE ([c].[CustomerID] = [o0].[CustomerID]) AND [o0].[CustomerID] IS NOT NULL
    )
)");
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
    WHERE ([c].[CustomerID] = [o].[CustomerID]) AND [o].[CustomerID] IS NOT NULL
    ORDER BY [o].[OrderID]) IS NULL");
        }

        public override async Task Where_subquery_FirstOrDefault_compared_to_entity(bool isAsync)
        {
            await base.Where_subquery_FirstOrDefault_compared_to_entity(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ((
    SELECT TOP(1) [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE ([c].[CustomerID] = [o].[CustomerID]) AND [o].[CustomerID] IS NOT NULL
    ORDER BY [o].[OrderID]) = 10243) AND (
    SELECT TOP(1) [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE ([c].[CustomerID] = [o].[CustomerID]) AND [o].[CustomerID] IS NOT NULL
    ORDER BY [o].[OrderID]) IS NOT NULL");
        }

        public override async Task Time_of_day_datetime(bool isAsync)
        {
            await base.Time_of_day_datetime(isAsync);

            AssertSql(
                @"SELECT CAST([o].[OrderDate] AS time)
FROM [Orders] AS [o]");
        }

        public override async Task TypeBinary_short_circuit(bool isAsync)
        {
            await base.TypeBinary_short_circuit(isAsync);

            AssertSql(
                @"@__p_0='False'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE @__p_0 = CAST(1 AS bit)");
        }

        public override async Task Where_is_conditional(bool isAsync)
        {
            await base.Where_is_conditional(isAsync);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE CASE
    WHEN CAST(1 AS bit) = CAST(1 AS bit) THEN CAST(0 AS bit)
    ELSE CAST(1 AS bit)
END = CAST(1 AS bit)");
        }

        public override async Task Enclosing_class_settable_member_generates_parameter(bool isAsync)
        {
            await base.Enclosing_class_settable_member_generates_parameter(isAsync);

            AssertSql(
                @"@__SettableProperty_0='4'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[OrderID] = @__SettableProperty_0) AND @__SettableProperty_0 IS NOT NULL",
                //
                @"@__SettableProperty_0='10'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[OrderID] = @__SettableProperty_0) AND @__SettableProperty_0 IS NOT NULL");
        }

        public override async Task Enclosing_class_readonly_member_generates_parameter(bool isAsync)
        {
            await base.Enclosing_class_readonly_member_generates_parameter(isAsync);

            AssertSql(
                @"@__ReadOnlyProperty_0='5'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[OrderID] = @__ReadOnlyProperty_0) AND @__ReadOnlyProperty_0 IS NOT NULL");
        }

        public override async Task Enclosing_class_const_member_does_not_generate_parameter(bool isAsync)
        {
            await base.Enclosing_class_const_member_does_not_generate_parameter(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] = 1");
        }

        public override async Task Project_non_nullable_value_after_FirstOrDefault_on_empty_collection(bool isAsync)
        {
            await base.Project_non_nullable_value_after_FirstOrDefault_on_empty_collection(isAsync);

            AssertSql(
                @"SELECT (
    SELECT TOP(1) CAST(LEN([o].[CustomerID]) AS int)
    FROM [Orders] AS [o]
    WHERE ([o].[CustomerID] = N'John Doe') AND [o].[CustomerID] IS NOT NULL)
FROM [Customers] AS [c]");
        }

        public override async Task Filter_non_nullable_value_after_FirstOrDefault_on_empty_collection(bool isAsync)
        {
            await base.Filter_non_nullable_value_after_FirstOrDefault_on_empty_collection(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ((
    SELECT TOP(1) CAST(LEN([o].[CustomerID]) AS int)
    FROM [Orders] AS [o]
    WHERE ([o].[CustomerID] = N'John Doe') AND [o].[CustomerID] IS NOT NULL) = 0) AND (
    SELECT TOP(1) CAST(LEN([o].[CustomerID]) AS int)
    FROM [Orders] AS [o]
    WHERE ([o].[CustomerID] = N'John Doe') AND [o].[CustomerID] IS NOT NULL) IS NOT NULL");
        }
    }
}
