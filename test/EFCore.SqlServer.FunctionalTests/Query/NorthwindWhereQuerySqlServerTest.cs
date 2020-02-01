// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindWhereQuerySqlServerTest : NorthwindWhereQueryTestBase<NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
    {
        public NorthwindWhereQuerySqlServerTest(NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            ClearLog();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task Where_simple(bool async)
        {
            await base.Where_simple(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'");
        }

        public override async Task Where_as_queryable_expression(bool async)
        {
            await base.Where_as_queryable_expression(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE ([c].[CustomerID] = [o].[CustomerID]) AND ([o].[CustomerID] = N'ALFKI'))");
        }

        public override async Task<string> Where_simple_closure(bool async)
        {
            var queryString = await base.Where_simple_closure(async);

            AssertSql(
                @"@__city_0='London' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_0");

            Assert.Equal(
                @"DECLARE @__city_0 nvarchar(4000) = N'London';

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_0", queryString, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);

            return null;
        }

        public override async Task Where_indexer_closure(bool async)
        {
            await base.Where_indexer_closure(async);

            AssertSql(
                @"@__p_0='London' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__p_0");
        }

        public override async Task Where_dictionary_key_access_closure(bool async)
        {
            await base.Where_dictionary_key_access_closure(async);

            AssertSql(
                @"@__get_Item_0='London' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__get_Item_0");
        }

        public override async Task Where_tuple_item_closure(bool async)
        {
            await base.Where_tuple_item_closure(async);

            AssertSql(
                @"@__predicateTuple_Item2_0='London' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__predicateTuple_Item2_0");
        }

        public override async Task Where_named_tuple_item_closure(bool async)
        {
            await base.Where_named_tuple_item_closure(async);

            AssertSql(
                @"@__predicateTuple_Item2_0='London' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__predicateTuple_Item2_0");
        }

        public override async Task Where_simple_closure_constant(bool async)
        {
            await base.Where_simple_closure_constant(async);

            AssertSql(
                @"@__predicate_0='True'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE @__predicate_0 = CAST(1 AS bit)");
        }

        public override async Task Where_simple_closure_via_query_cache(bool async)
        {
            await base.Where_simple_closure_via_query_cache(async);

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

        public override async Task Where_method_call_nullable_type_closure_via_query_cache(bool async)
        {
            await base.Where_method_call_nullable_type_closure_via_query_cache(async);

            AssertSql(
                @"@__p_0='2' (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE CAST([e].[ReportsTo] AS bigint) = @__p_0",
                //
                @"@__p_0='5' (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE CAST([e].[ReportsTo] AS bigint) = @__p_0");
        }

        public override async Task Where_method_call_nullable_type_reverse_closure_via_query_cache(bool async)
        {
            await base.Where_method_call_nullable_type_reverse_closure_via_query_cache(async);

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

        public override async Task Where_method_call_closure_via_query_cache(bool async)
        {
            await base.Where_method_call_closure_via_query_cache(async);

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

        public override async Task Where_field_access_closure_via_query_cache(bool async)
        {
            await base.Where_field_access_closure_via_query_cache(async);

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

        public override async Task Where_property_access_closure_via_query_cache(bool async)
        {
            await base.Where_property_access_closure_via_query_cache(async);

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

        public override async Task Where_static_field_access_closure_via_query_cache(bool async)
        {
            await base.Where_static_field_access_closure_via_query_cache(async);

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

        public override async Task Where_static_property_access_closure_via_query_cache(bool async)
        {
            await base.Where_static_property_access_closure_via_query_cache(async);

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

        public override async Task Where_nested_field_access_closure_via_query_cache(bool async)
        {
            await base.Where_nested_field_access_closure_via_query_cache(async);

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

        public override async Task Where_nested_property_access_closure_via_query_cache(bool async)
        {
            await base.Where_nested_property_access_closure_via_query_cache(async);

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

        public override async Task Where_new_instance_field_access_query_cache(bool async)
        {
            await base.Where_new_instance_field_access_query_cache(async);

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

        public override async Task Where_new_instance_field_access_closure_via_query_cache(bool async)
        {
            await base.Where_new_instance_field_access_closure_via_query_cache(async);

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

        public override async Task Where_simple_closure_via_query_cache_nullable_type(bool async)
        {
            await base.Where_simple_closure_via_query_cache_nullable_type(async);

            AssertSql(
                @"@__p_0='2' (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE CAST([e].[ReportsTo] AS bigint) = @__p_0",
                //
                @"@__p_0='5' (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE CAST([e].[ReportsTo] AS bigint) = @__p_0",
                //
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] IS NULL");
        }

        public override async Task Where_simple_closure_via_query_cache_nullable_type_reverse(bool async)
        {
            await base.Where_simple_closure_via_query_cache_nullable_type_reverse(async);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] IS NULL",
                //
                @"@__p_0='5' (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE CAST([e].[ReportsTo] AS bigint) = @__p_0",
                //
                @"@__p_0='2' (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE CAST([e].[ReportsTo] AS bigint) = @__p_0");
        }

        public override void Where_subquery_closure_via_query_cache()
        {
            base.Where_subquery_closure_via_query_cache();

            AssertSql(
                @"@__customerID_0='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE ([o].[CustomerID] = @__customerID_0) AND ([o].[CustomerID] = [c].[CustomerID]))",
                //
                @"@__customerID_0='ANATR' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE ([o].[CustomerID] = @__customerID_0) AND ([o].[CustomerID] = [c].[CustomerID]))");
        }

        public override async Task Where_bitwise_or(bool async)
        {
            await base.Where_bitwise_or(async);

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

        public override async Task Where_bitwise_and(bool async)
        {
            await base.Where_bitwise_and(async);

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

        public override async Task Where_bitwise_xor(bool async)
        {
            await base.Where_bitwise_xor(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Where_simple_shadow(bool async)
        {
            await base.Where_simple_shadow(async);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[Title] = N'Sales Representative'");
        }

        public override async Task Where_simple_shadow_projection(bool async)
        {
            await base.Where_simple_shadow_projection(async);

            AssertSql(
                @"SELECT [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[Title] = N'Sales Representative'");
        }

        public override async Task Where_shadow_subquery_FirstOrDefault(bool async)
        {
            await base.Where_shadow_subquery_FirstOrDefault(async);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE ([e].[Title] = (
    SELECT TOP(1) [e0].[Title]
    FROM [Employees] AS [e0]
    ORDER BY [e0].[Title])) OR ([e].[Title] IS NULL AND (
    SELECT TOP(1) [e0].[Title]
    FROM [Employees] AS [e0]
    ORDER BY [e0].[Title]) IS NULL)");
        }

        public override async Task Where_subquery_correlated(bool async)
        {
            await base.Where_subquery_correlated(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Customers] AS [c0]
    WHERE [c].[CustomerID] = [c0].[CustomerID])");
        }

        public override async Task Where_equals_method_string(bool async)
        {
            await base.Where_equals_method_string(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'");
        }

        public override async Task Where_equals_method_int(bool async)
        {
            await base.Where_equals_method_int(async);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = 1");
        }

        public override async Task Where_equals_using_object_overload_on_mismatched_types(bool async)
        {
            await base.Where_equals_using_object_overload_on_mismatched_types(async);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE 0 = 1");

            // See issue#17498
            //Assert.Contains(
            //    RelationalStrings.LogPossibleUnintendedUseOfEquals.GenerateMessage(
            //        "e.EmployeeID.Equals(Convert(__longPrm_0, Object))"), Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));
        }

        public override async Task Where_equals_using_int_overload_on_mismatched_types(bool async)
        {
            await base.Where_equals_using_int_overload_on_mismatched_types(async);

            AssertSql(
                @"@__p_0='1'

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = @__p_0");
        }

        public override async Task Where_equals_on_mismatched_types_nullable_int_long(bool async)
        {
            await base.Where_equals_on_mismatched_types_nullable_int_long(async);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE 0 = 1",
                //
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE 0 = 1");

            // See issue#17498
            //Assert.Contains(
            //    RelationalStrings.LogPossibleUnintendedUseOfEquals.GenerateMessage(
            //        "__longPrm_0.Equals(Convert(e.ReportsTo, Object))"), Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));

            //Assert.Contains(
            //    RelationalStrings.LogPossibleUnintendedUseOfEquals.GenerateMessage(
            //        "e.ReportsTo.Equals(Convert(__longPrm_0, Object))"), Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));
        }

        public override async Task Where_equals_on_mismatched_types_nullable_long_nullable_int(bool async)
        {
            await base.Where_equals_on_mismatched_types_nullable_long_nullable_int(async);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE 0 = 1",
                //
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE 0 = 1");

            // See issue#17498
            //Assert.Contains(
            //    RelationalStrings.LogPossibleUnintendedUseOfEquals.GenerateMessage(
            //        "__nullableLongPrm_0.Equals(Convert(e.ReportsTo, Object))"),
            //    Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));

            //Assert.Contains(
            //    RelationalStrings.LogPossibleUnintendedUseOfEquals.GenerateMessage(
            //        "e.ReportsTo.Equals(Convert(__nullableLongPrm_0, Object))"),
            //    Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));
        }

        public override async Task Where_equals_on_mismatched_types_int_nullable_int(bool async)
        {
            await base.Where_equals_on_mismatched_types_int_nullable_int(async);

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

        public override async Task Where_equals_on_matched_nullable_int_types(bool async)
        {
            await base.Where_equals_on_matched_nullable_int_types(async);

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

        public override async Task Where_equals_on_null_nullable_int_types(bool async)
        {
            await base.Where_equals_on_null_nullable_int_types(async);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] IS NULL",
                //
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] IS NULL");
        }

        public override async Task Where_comparison_nullable_type_not_null(bool async)
        {
            await base.Where_comparison_nullable_type_not_null(async);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] = 2");
        }

        public override async Task Where_comparison_nullable_type_null(bool async)
        {
            await base.Where_comparison_nullable_type_null(async);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] IS NULL");
        }

        public override async Task Where_string_length(bool async)
        {
            await base.Where_string_length(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE CAST(LEN([c].[City]) AS int) = 6");
        }

        public override async Task Where_string_indexof(bool async)
        {
            await base.Where_string_indexof(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (CASE
    WHEN N'Sea' = N'' THEN 0
    ELSE CAST(CHARINDEX(N'Sea', [c].[City]) AS int) - 1
END <> -1) OR CASE
    WHEN N'Sea' = N'' THEN 0
    ELSE CAST(CHARINDEX(N'Sea', [c].[City]) AS int) - 1
END IS NULL");
        }

        public override async Task Where_string_replace(bool async)
        {
            await base.Where_string_replace(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE REPLACE([c].[City], N'Sea', N'Rea') = N'Reattle'");
        }

        public override async Task Where_string_substring(bool async)
        {
            await base.Where_string_substring(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE SUBSTRING([c].[City], 1 + 1, 2) = N'ea'");
        }

        public override async Task Where_datetime_now(bool async)
        {
            await base.Where_datetime_now(async);

            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE GETDATE() <> @__myDatetime_0");
        }

        public override async Task Where_datetime_utcnow(bool async)
        {
            await base.Where_datetime_utcnow(async);

            AssertSql(
                @"@__myDatetime_0='2015-04-10T00:00:00'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE GETUTCDATE() <> @__myDatetime_0");
        }

        public override async Task Where_datetime_today(bool async)
        {
            await base.Where_datetime_today(async);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE CONVERT(date, GETDATE()) = CONVERT(date, GETDATE())");
        }

        public override async Task Where_datetime_date_component(bool async)
        {
            await base.Where_datetime_date_component(async);

            AssertSql(
                @"@__myDatetime_0='1998-05-04T00:00:00' (DbType = DateTime)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE CONVERT(date, [o].[OrderDate]) = @__myDatetime_0");
        }

        public override async Task Where_date_add_year_constant_component(bool async)
        {
            await base.Where_date_add_year_constant_component(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(year, DATEADD(year, CAST(-1 AS int), [o].[OrderDate])) = 1997");
        }

        public override async Task Where_datetime_year_component(bool async)
        {
            await base.Where_datetime_year_component(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(year, [o].[OrderDate]) = 1998");
        }

        public override async Task Where_datetime_month_component(bool async)
        {
            await base.Where_datetime_month_component(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(month, [o].[OrderDate]) = 4");
        }

        public override async Task Where_datetime_dayOfYear_component(bool async)
        {
            await base.Where_datetime_dayOfYear_component(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(dayofyear, [o].[OrderDate]) = 68");
        }

        public override async Task Where_datetime_day_component(bool async)
        {
            await base.Where_datetime_day_component(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(day, [o].[OrderDate]) = 4");
        }

        public override async Task Where_datetime_hour_component(bool async)
        {
            await base.Where_datetime_hour_component(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(hour, [o].[OrderDate]) = 14");
        }

        public override async Task Where_datetime_minute_component(bool async)
        {
            await base.Where_datetime_minute_component(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(minute, [o].[OrderDate]) = 23");
        }

        public override async Task Where_datetime_second_component(bool async)
        {
            await base.Where_datetime_second_component(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(second, [o].[OrderDate]) = 44");
        }

        public override async Task Where_datetime_millisecond_component(bool async)
        {
            await base.Where_datetime_millisecond_component(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(millisecond, [o].[OrderDate]) = 88");
        }

        public override async Task Where_datetimeoffset_now_component(bool async)
        {
            await base.Where_datetimeoffset_now_component(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE CAST([o].[OrderDate] AS datetimeoffset) = SYSDATETIMEOFFSET()");
        }

        public override async Task Where_datetimeoffset_utcnow_component(bool async)
        {
            await base.Where_datetimeoffset_utcnow_component(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE CAST([o].[OrderDate] AS datetimeoffset) = CAST(SYSUTCDATETIME() AS datetimeoffset)");
        }

        public override async Task Where_simple_reversed(bool async)
        {
            await base.Where_simple_reversed(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE N'London' = [c].[City]");
        }

        public override async Task Where_is_null(bool async)
        {
            await base.Where_is_null(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] IS NULL");
        }

        public override async Task Where_null_is_null(bool async)
        {
            await base.Where_null_is_null(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Where_constant_is_null(bool async)
        {
            await base.Where_constant_is_null(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 0 = 1");
        }

        public override async Task Where_is_not_null(bool async)
        {
            await base.Where_is_not_null(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] IS NOT NULL");
        }

        public override async Task Where_null_is_not_null(bool async)
        {
            await base.Where_null_is_not_null(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 0 = 1");
        }

        public override async Task Where_constant_is_not_null(bool async)
        {
            await base.Where_constant_is_not_null(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Where_identity_comparison(bool async)
        {
            await base.Where_identity_comparison(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[City] = [c].[City]) OR [c].[City] IS NULL");
        }

        public override async Task Where_in_optimization_multiple(bool async)
        {
            await base.Where_in_optimization_multiple(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE ((([c].[City] = N'London') OR ([c].[City] = N'Berlin')) OR ([c].[CustomerID] = N'ALFKI')) OR ([c].[CustomerID] = N'ABCDE')");
        }

        public override async Task Where_not_in_optimization1(bool async)
        {
            await base.Where_not_in_optimization1(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE (([c].[City] <> N'London') OR [c].[City] IS NULL) AND (([e].[City] <> N'London') OR [e].[City] IS NULL)");
        }

        public override async Task Where_not_in_optimization2(bool async)
        {
            await base.Where_not_in_optimization2(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE (([c].[City] <> N'London') OR [c].[City] IS NULL) AND (([c].[City] <> N'Berlin') OR [c].[City] IS NULL)");
        }

        public override async Task Where_not_in_optimization3(bool async)
        {
            await base.Where_not_in_optimization3(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE ((([c].[City] <> N'London') OR [c].[City] IS NULL) AND (([c].[City] <> N'Berlin') OR [c].[City] IS NULL)) AND (([c].[City] <> N'Seattle') OR [c].[City] IS NULL)");
        }

        public override async Task Where_not_in_optimization4(bool async)
        {
            await base.Where_not_in_optimization4(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE (((([c].[City] <> N'London') OR [c].[City] IS NULL) AND (([c].[City] <> N'Berlin') OR [c].[City] IS NULL)) AND (([c].[City] <> N'Seattle') OR [c].[City] IS NULL)) AND (([c].[City] <> N'Lisboa') OR [c].[City] IS NULL)");
        }

        public override async Task Where_select_many_and(bool async)
        {
            await base.Where_select_many_and(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE (([c].[City] = N'London') AND ([c].[Country] = N'UK')) AND (([e].[City] = N'London') AND ([e].[Country] = N'UK'))");
        }

        public override async Task Where_primitive(bool async)
        {
            await base.Where_primitive(async);

            AssertSql(
                @"@__p_0='9'

SELECT [t].[EmployeeID]
FROM (
    SELECT TOP(@__p_0) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
) AS [t]
WHERE [t].[EmployeeID] = 5");
        }

        public override async Task Where_bool_member(bool async)
        {
            await base.Where_bool_member(async);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = CAST(1 AS bit)");
        }

        public override async Task Where_bool_member_false(bool async)
        {
            await base.Where_bool_member_false(async);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] <> CAST(1 AS bit)");
        }

        public override async Task Where_bool_member_negated_twice(bool async)
        {
            await base.Where_bool_member_negated_twice(async);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = CAST(1 AS bit)");
        }

        public override async Task Where_bool_member_shadow(bool async)
        {
            await base.Where_bool_member_shadow(async);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = CAST(1 AS bit)");
        }

        public override async Task Where_bool_member_false_shadow(bool async)
        {
            await base.Where_bool_member_false_shadow(async);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] <> CAST(1 AS bit)");
        }

        public override async Task Where_bool_member_equals_constant(bool async)
        {
            await base.Where_bool_member_equals_constant(async);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = CAST(1 AS bit)");
        }

        public override async Task Where_bool_member_in_complex_predicate(bool async)
        {
            await base.Where_bool_member_in_complex_predicate(async);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE (([p].[ProductID] > 100) AND ([p].[Discontinued] = CAST(1 AS bit))) OR ([p].[Discontinued] = CAST(1 AS bit))");
        }

        public override async Task Where_bool_member_compared_to_binary_expression(bool async)
        {
            await base.Where_bool_member_compared_to_binary_expression(async);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = CASE
    WHEN [p].[ProductID] > 50 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task Where_not_bool_member_compared_to_not_bool_member(bool async)
        {
            await base.Where_not_bool_member_compared_to_not_bool_member(async);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]");
        }

        public override async Task Where_negated_boolean_expression_compared_to_another_negated_boolean_expression(bool async)
        {
            await base.Where_negated_boolean_expression_compared_to_another_negated_boolean_expression(async);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE CASE
    WHEN [p].[ProductID] <= 50 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END = CASE
    WHEN [p].[ProductID] <= 20 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task Where_not_bool_member_compared_to_binary_expression(bool async)
        {
            await base.Where_not_bool_member_compared_to_binary_expression(async);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] <> CASE
    WHEN [p].[ProductID] > 50 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task Where_bool_parameter(bool async)
        {
            await base.Where_bool_parameter(async);

            AssertSql(
                @"@__prm_0='True'

SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE @__prm_0 = CAST(1 AS bit)");
        }

        public override async Task Where_bool_parameter_compared_to_binary_expression(bool async)
        {
            await base.Where_bool_parameter_compared_to_binary_expression(async);

            AssertSql(
                @"@__prm_0='True'

SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE CASE
    WHEN [p].[ProductID] > 50 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END <> @__prm_0");
        }

        public override async Task Where_bool_member_and_parameter_compared_to_binary_expression_nested(bool async)
        {
            await base.Where_bool_member_and_parameter_compared_to_binary_expression_nested(async);

            AssertSql(
                @"@__prm_0='True'

SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = CASE
    WHEN CASE
        WHEN [p].[ProductID] > 50 THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END <> @__prm_0 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override async Task Where_de_morgan_or_optimized(bool async)
        {
            await base.Where_de_morgan_or_optimized(async);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE ([p].[Discontinued] <> CAST(1 AS bit)) AND ([p].[ProductID] >= 20)");
        }

        public override async Task Where_de_morgan_and_optimized(bool async)
        {
            await base.Where_de_morgan_and_optimized(async);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE ([p].[Discontinued] <> CAST(1 AS bit)) OR ([p].[ProductID] >= 20)");
        }

        public override async Task Where_complex_negated_expression_optimized(bool async)
        {
            await base.Where_complex_negated_expression_optimized(async);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE (([p].[Discontinued] <> CAST(1 AS bit)) AND ([p].[ProductID] < 60)) AND ([p].[ProductID] > 30)");
        }

        public override async Task Where_short_member_comparison(bool async)
        {
            await base.Where_short_member_comparison(async);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[UnitsInStock] > CAST(10 AS smallint)");
        }

        public override async Task Where_comparison_to_nullable_bool(bool async)
        {
            await base.Where_comparison_to_nullable_bool(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'%KI'");
        }

        public override async Task Where_true(bool async)
        {
            await base.Where_true(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Where_false(bool async)
        {
            await base.Where_false(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 0 = 1");
        }

        public override async Task Where_default(bool async)
        {
            await base.Where_default(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[Fax] IS NULL");
        }

        public override async Task Where_expression_invoke_1(bool async)
        {
            await base.Where_expression_invoke_1(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override async Task Where_expression_invoke_2(bool async)
        {
            await base.Where_expression_invoke_2(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override async Task Where_expression_invoke_3(bool async)
        {
            await base.Where_expression_invoke_3(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override async Task Where_concat_string_int_comparison1(bool async)
        {
            await base.Where_concat_string_int_comparison1(async);

            AssertSql(
                @"@__i_0='10'

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE ([c].[CustomerID] + CAST(@__i_0 AS nchar(5))) = [c].[CompanyName]");
        }

        public override async Task Where_concat_string_int_comparison2(bool async)
        {
            await base.Where_concat_string_int_comparison2(async);

            AssertSql(
                @"@__i_0='10'

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE (CAST(@__i_0 AS nchar(5)) + [c].[CustomerID]) = [c].[CompanyName]");
        }

        public override async Task Where_concat_string_int_comparison3(bool async)
        {
            await base.Where_concat_string_int_comparison3(async);

            AssertSql(
                @"@__p_0='30'
@__j_1='21'

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE (((CAST(@__p_0 AS nchar(5)) + [c].[CustomerID]) + CAST(@__j_1 AS nchar(5))) + CAST(42 AS nchar(5))) = [c].[CompanyName]");
        }

        public override async Task Where_concat_string_int_comparison4(bool async)
        {
            await base.Where_concat_string_int_comparison4(async);

            AssertSql(
                @"SELECT [o].[CustomerID]
FROM [Orders] AS [o]
WHERE ((CAST([o].[OrderID] AS nchar(5)) + [o].[CustomerID]) = [o].[CustomerID]) OR [o].[CustomerID] IS NULL");
        }

        public override async Task Where_concat_string_string_comparison(bool async)
        {
            await base.Where_concat_string_string_comparison(async);

            AssertSql(
                @"@__i_0='A' (Size = 5)

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE (@__i_0 + [c].[CustomerID]) = [c].[CompanyName]");
        }

        public override async Task Where_string_concat_method_comparison(bool async)
        {
            await base.Where_string_concat_method_comparison(async);

            AssertSql(
                @"@__i_0='A' (Size = 5)

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE (@__i_0 + [c].[CustomerID]) = [c].[CompanyName]");
        }

        public override async Task Where_ternary_boolean_condition_true(bool async)
        {
            await base.Where_ternary_boolean_condition_true(async);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[UnitsInStock] >= CAST(20 AS smallint)");
        }

        public override async Task Where_ternary_boolean_condition_false(bool async)
        {
            await base.Where_ternary_boolean_condition_false(async);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[UnitsInStock] < CAST(20 AS smallint)");
        }

        public override async Task Where_ternary_boolean_condition_with_another_condition(bool async)
        {
            await base.Where_ternary_boolean_condition_with_another_condition(async);

            AssertSql(
                @"@__productId_0='15'

SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE ([p].[ProductID] < @__productId_0) AND ([p].[UnitsInStock] >= CAST(20 AS smallint))");
        }

        public override async Task Where_ternary_boolean_condition_with_false_as_result_true(bool async)
        {
            await base.Where_ternary_boolean_condition_with_false_as_result_true(async);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[UnitsInStock] >= CAST(20 AS smallint)");
        }

        public override async Task Where_ternary_boolean_condition_with_false_as_result_false(bool async)
        {
            await base.Where_ternary_boolean_condition_with_false_as_result_false(async);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE 0 = 1");
        }

        public override async Task Where_compare_constructed_equal(bool async)
        {
            await base.Where_compare_constructed_equal(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Where_compare_constructed_multi_value_equal(bool async)
        {
            await base.Where_compare_constructed_multi_value_equal(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Where_compare_constructed_multi_value_not_equal(bool async)
        {
            await base.Where_compare_constructed_multi_value_not_equal(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Where_compare_tuple_constructed_equal(bool async)
        {
            await base.Where_compare_tuple_constructed_equal(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Where_compare_tuple_constructed_multi_value_equal(bool async)
        {
            await base.Where_compare_tuple_constructed_multi_value_equal(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Where_compare_tuple_constructed_multi_value_not_equal(bool async)
        {
            await base.Where_compare_tuple_constructed_multi_value_not_equal(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Where_compare_tuple_create_constructed_equal(bool async)
        {
            await base.Where_compare_tuple_create_constructed_equal(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Where_compare_tuple_create_constructed_multi_value_equal(bool async)
        {
            await base.Where_compare_tuple_create_constructed_multi_value_equal(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Where_compare_tuple_create_constructed_multi_value_not_equal(bool async)
        {
            await base.Where_compare_tuple_create_constructed_multi_value_not_equal(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Where_compare_null(bool async)
        {
            await base.Where_compare_null(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] IS NULL AND ([c].[Country] = N'UK')");
        }

        public override async Task Where_Is_on_same_type(bool async)
        {
            await base.Where_Is_on_same_type(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Where_chain(bool async)
        {
            await base.Where_chain(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'QUICK') AND ([o].[OrderDate] > '1998-01-01T00:00:00.000')");
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

        public override async Task Where_array_index(bool async)
        {
            await base.Where_array_index(async);

            AssertSql(
                @"@__p_0='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @__p_0");
        }

        public override async Task Where_multiple_contains_in_subquery_with_or(bool async)
        {
            await base.Where_multiple_contains_in_subquery_with_or(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE [o].[ProductID] IN (
    SELECT TOP(1) [p].[ProductID]
    FROM [Products] AS [p]
    ORDER BY [p].[ProductID]
) OR [o].[OrderID] IN (
    SELECT TOP(1) [o0].[OrderID]
    FROM [Orders] AS [o0]
    ORDER BY [o0].[OrderID]
)");
        }

        public override async Task Where_multiple_contains_in_subquery_with_and(bool async)
        {
            await base.Where_multiple_contains_in_subquery_with_and(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE [o].[ProductID] IN (
    SELECT TOP(20) [p].[ProductID]
    FROM [Products] AS [p]
    ORDER BY [p].[ProductID]
) AND [o].[OrderID] IN (
    SELECT TOP(10) [o0].[OrderID]
    FROM [Orders] AS [o0]
    ORDER BY [o0].[OrderID]
)");
        }

        public override async Task Where_contains_on_navigation(bool async)
        {
            await base.Where_contains_on_navigation(async);

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

        public override async Task Where_subquery_FirstOrDefault_is_null(bool async)
        {
            await base.Where_subquery_FirstOrDefault_is_null(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (
    SELECT TOP(1) [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID]) IS NULL");
        }

        public override async Task Where_subquery_FirstOrDefault_compared_to_entity(bool async)
        {
            await base.Where_subquery_FirstOrDefault_compared_to_entity(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (
    SELECT TOP(1) [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID]) = 10243");
        }

        public override async Task Time_of_day_datetime(bool async)
        {
            await base.Time_of_day_datetime(async);

            AssertSql(
                @"SELECT CAST([o].[OrderDate] AS time)
FROM [Orders] AS [o]");
        }

        public override async Task TypeBinary_short_circuit(bool async)
        {
            await base.TypeBinary_short_circuit(async);

            AssertSql(
                @"@__p_0='False'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE @__p_0 = CAST(1 AS bit)");
        }

        public override async Task Where_is_conditional(bool async)
        {
            await base.Where_is_conditional(async);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE 0 = 1");
        }

        public override async Task Enclosing_class_settable_member_generates_parameter(bool async)
        {
            await base.Enclosing_class_settable_member_generates_parameter(async);

            AssertSql(
                @"@__SettableProperty_0='4'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] = @__SettableProperty_0",
                //
                @"@__SettableProperty_0='10'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] = @__SettableProperty_0");
        }

        public override async Task Enclosing_class_readonly_member_generates_parameter(bool async)
        {
            await base.Enclosing_class_readonly_member_generates_parameter(async);

            AssertSql(
                @"@__ReadOnlyProperty_0='5'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] = @__ReadOnlyProperty_0");
        }

        public override async Task Enclosing_class_const_member_does_not_generate_parameter(bool async)
        {
            await base.Enclosing_class_const_member_does_not_generate_parameter(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] = 1");
        }

        public override async Task Generic_Ilist_contains_translates_to_server(bool async)
        {
            await base.Generic_Ilist_contains_translates_to_server(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] IN (N'Seattle')");
        }

        public override async Task Filter_non_nullable_value_after_FirstOrDefault_on_empty_collection(bool async)
        {
            await base.Filter_non_nullable_value_after_FirstOrDefault_on_empty_collection(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (
    SELECT TOP(1) CAST(LEN([o].[CustomerID]) AS int)
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] = N'John Doe') = 0");
        }

        public override async Task Like_with_non_string_column_using_ToString(bool async)
        {
            await base.Like_with_non_string_column_using_ToString(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE CONVERT(VARCHAR(11), [o].[OrderID]) LIKE N'%20%'");
        }

        public override async Task Like_with_non_string_column_using_double_cast(bool async)
        {
            await base.Like_with_non_string_column_using_double_cast(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE CAST([o].[OrderID] AS nvarchar(max)) LIKE N'%20%'");
        }

        public override async Task Using_same_parameter_twice_in_query_generates_one_sql_parameter(bool async)
        {
            await base.Using_same_parameter_twice_in_query_generates_one_sql_parameter(async);

            AssertSql(
                @"@__i_0='10'

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE ((CAST(@__i_0 AS nchar(5)) + [c].[CustomerID]) + CAST(@__i_0 AS nchar(5))) = [c].[CompanyName]");
        }

        public override async Task Where_Queryable_ToList_Count(bool async)
        {
            await base.Where_Queryable_ToList_Count(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE (
    SELECT COUNT(*)
    FROM [Orders] AS [o0]
    WHERE [o0].[CustomerID] = [c].[CustomerID]) = 0
ORDER BY [c].[CustomerID], [o].[OrderID]");
        }

        public override async Task Where_Queryable_ToList_Contains(bool async)
        {
            await base.Where_Queryable_ToList_Contains(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [o].[CustomerID], [o].[OrderID]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE N'ALFKI' IN (
    SELECT [o0].[CustomerID]
    FROM [Orders] AS [o0]
    WHERE [o0].[CustomerID] = [c].[CustomerID]
)
ORDER BY [c].[CustomerID], [o].[OrderID]");
        }

        public override async Task Where_Queryable_ToArray_Count(bool async)
        {
            await base.Where_Queryable_ToArray_Count(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE (
    SELECT COUNT(*)
    FROM [Orders] AS [o0]
    WHERE [o0].[CustomerID] = [c].[CustomerID]) = 0
ORDER BY [c].[CustomerID], [o].[OrderID]");
        }

        public override async Task Where_Queryable_ToArray_Contains(bool async)
        {
            await base.Where_Queryable_ToArray_Contains(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [o].[CustomerID], [o].[OrderID]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE N'ALFKI' IN (
    SELECT [o0].[CustomerID]
    FROM [Orders] AS [o0]
    WHERE [o0].[CustomerID] = [c].[CustomerID]
)
ORDER BY [c].[CustomerID], [o].[OrderID]");
        }

        public override async Task Where_Queryable_AsEnumerable_Count(bool async)
        {
            await base.Where_Queryable_AsEnumerable_Count(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE (
    SELECT COUNT(*)
    FROM [Orders] AS [o0]
    WHERE [o0].[CustomerID] = [c].[CustomerID]) = 0
ORDER BY [c].[CustomerID], [o].[OrderID]");
        }

        public override async Task Where_Queryable_AsEnumerable_Contains(bool async)
        {
            await base.Where_Queryable_AsEnumerable_Contains(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [o].[CustomerID], [o].[OrderID]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE N'ALFKI' IN (
    SELECT [o0].[CustomerID]
    FROM [Orders] AS [o0]
    WHERE [o0].[CustomerID] = [c].[CustomerID]
)
ORDER BY [c].[CustomerID], [o].[OrderID]");
        }

        public override async Task Where_Queryable_ToList_Count_member(bool async)
        {
            await base.Where_Queryable_ToList_Count_member(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE (
    SELECT COUNT(*)
    FROM [Orders] AS [o0]
    WHERE [o0].[CustomerID] = [c].[CustomerID]) = 0
ORDER BY [c].[CustomerID], [o].[OrderID]");
        }

        public override async Task Where_Queryable_ToArray_Length_member(bool async)
        {
            await base.Where_Queryable_ToArray_Length_member(async);

            AssertSql(" ");
        }

        public override async Task Where_collection_navigation_ToList_Count(bool async)
        {
            await base.Where_collection_navigation_ToList_Count(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
FROM [Orders] AS [o]
LEFT JOIN [Order Details] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
WHERE ([o].[OrderID] < 10300) AND ((
    SELECT COUNT(*)
    FROM [Order Details] AS [o1]
    WHERE [o].[OrderID] = [o1].[OrderID]) = 0)
ORDER BY [o].[OrderID], [o0].[OrderID], [o0].[ProductID]");
        }

        public override async Task Where_collection_navigation_ToList_Contains(bool async)
        {
            await base.Where_collection_navigation_ToList_Contains(async);

            AssertSql(
                @"@__entity_equality_order_0_OrderID='10248' (Nullable = true)

SELECT [c].[CustomerID], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE @__entity_equality_order_0_OrderID IN (
    SELECT [o0].[OrderID]
    FROM [Orders] AS [o0]
    WHERE [c].[CustomerID] = [o0].[CustomerID]
)
ORDER BY [c].[CustomerID], [o].[OrderID]");
        }

        public override async Task Where_collection_navigation_ToArray_Count(bool async)
        {
            await base.Where_collection_navigation_ToArray_Count(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
FROM [Orders] AS [o]
LEFT JOIN [Order Details] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
WHERE ([o].[OrderID] < 10300) AND ((
    SELECT COUNT(*)
    FROM [Order Details] AS [o1]
    WHERE [o].[OrderID] = [o1].[OrderID]) = 0)
ORDER BY [o].[OrderID], [o0].[OrderID], [o0].[ProductID]");
        }

        public override async Task Where_collection_navigation_ToArray_Contains(bool async)
        {
            await base.Where_collection_navigation_ToArray_Contains(async);

            AssertSql(" ");
        }

        public override async Task Where_collection_navigation_AsEnumerable_Count(bool async)
        {
            await base.Where_collection_navigation_AsEnumerable_Count(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
FROM [Orders] AS [o]
LEFT JOIN [Order Details] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
WHERE ([o].[OrderID] < 10300) AND ((
    SELECT COUNT(*)
    FROM [Order Details] AS [o1]
    WHERE [o].[OrderID] = [o1].[OrderID]) = 0)
ORDER BY [o].[OrderID], [o0].[OrderID], [o0].[ProductID]");
        }

        public override async Task Where_collection_navigation_AsEnumerable_Contains(bool async)
        {
            await base.Where_collection_navigation_AsEnumerable_Contains(async);

            AssertSql(
                @"@__entity_equality_order_0_OrderID='10248' (Nullable = true)

SELECT [c].[CustomerID], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE @__entity_equality_order_0_OrderID IN (
    SELECT [o0].[OrderID]
    FROM [Orders] AS [o0]
    WHERE [c].[CustomerID] = [o0].[CustomerID]
)
ORDER BY [c].[CustomerID], [o].[OrderID]");
        }

        public override async Task Where_collection_navigation_ToList_Count_member(bool async)
        {
            await base.Where_collection_navigation_ToList_Count_member(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
FROM [Orders] AS [o]
LEFT JOIN [Order Details] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
WHERE ([o].[OrderID] < 10300) AND ((
    SELECT COUNT(*)
    FROM [Order Details] AS [o1]
    WHERE [o].[OrderID] = [o1].[OrderID]) = 0)
ORDER BY [o].[OrderID], [o0].[OrderID], [o0].[ProductID]");
        }

        public override async Task Where_collection_navigation_ToArray_Length_member(bool async)
        {
            await base.Where_collection_navigation_ToArray_Length_member(async);

            AssertSql(" ");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
