// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class NorthwindWhereQuerySqlServerTest : NorthwindWhereQueryRelationalTestBase<
    NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
{
    public NorthwindWhereQuerySqlServerTest(
        NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        ClearLog();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Where_simple(bool async)
    {
        await base.Where_simple(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'
""");
    }

    public override async Task Where_as_queryable_expression(bool async)
    {
        await base.Where_as_queryable_expression(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID] AND [o].[CustomerID] = N'ALFKI')
""");
    }

    public override async Task<string> Where_simple_closure(bool async)
    {
        var queryString = await base.Where_simple_closure(async);

        AssertSql(
            """
@__city_0='London' (Size = 15)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_0
""");

        Assert.Equal(
            @"DECLARE @__city_0 nvarchar(15) = N'London';

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_0", queryString, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);

        return null;
    }

    public override async Task Where_indexer_closure(bool async)
    {
        await base.Where_indexer_closure(async);

        AssertSql(
            """
@__p_0='London' (Size = 15)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__p_0
""");
    }

    public override async Task Where_dictionary_key_access_closure(bool async)
    {
        await base.Where_dictionary_key_access_closure(async);

        AssertSql(
            """
@__get_Item_0='London' (Size = 15)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__get_Item_0
""");
    }

    public override async Task Where_tuple_item_closure(bool async)
    {
        await base.Where_tuple_item_closure(async);

        AssertSql(
            """
@__predicateTuple_Item2_0='London' (Size = 15)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__predicateTuple_Item2_0
""");
    }

    public override async Task Where_named_tuple_item_closure(bool async)
    {
        await base.Where_named_tuple_item_closure(async);

        AssertSql(
            """
@__predicateTuple_Item2_0='London' (Size = 15)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__predicateTuple_Item2_0
""");
    }

    public override async Task Where_simple_closure_constant(bool async)
    {
        await base.Where_simple_closure_constant(async);

        AssertSql(
            """
@__predicate_0='True'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE @__predicate_0 = CAST(1 AS bit)
""");
    }

    public override async Task Where_simple_closure_via_query_cache(bool async)
    {
        await base.Where_simple_closure_via_query_cache(async);

        AssertSql(
            """
@__city_0='London' (Size = 15)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_0
""",
            //
            """
@__city_0='Seattle' (Size = 15)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_0
""");
    }

    public override async Task Where_method_call_nullable_type_closure_via_query_cache(bool async)
    {
        await base.Where_method_call_nullable_type_closure_via_query_cache(async);

        AssertSql(
            """
@__p_0='2' (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE CAST([e].[ReportsTo] AS bigint) = @__p_0
""",
            //
            """
@__p_0='5' (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE CAST([e].[ReportsTo] AS bigint) = @__p_0
""");
    }

    public override async Task Where_method_call_nullable_type_reverse_closure_via_query_cache(bool async)
    {
        await base.Where_method_call_nullable_type_reverse_closure_via_query_cache(async);

        AssertSql(
            """
@__p_0='1' (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE CAST([e].[EmployeeID] AS bigint) > @__p_0
""",
            //
            """
@__p_0='5' (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE CAST([e].[EmployeeID] AS bigint) > @__p_0
""");
    }

    public override async Task Where_method_call_closure_via_query_cache(bool async)
    {
        await base.Where_method_call_closure_via_query_cache(async);

        AssertSql(
            """
@__GetCity_0='London' (Size = 15)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__GetCity_0
""",
            //
            """
@__GetCity_0='Seattle' (Size = 15)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__GetCity_0
""");
    }

    public override async Task Where_field_access_closure_via_query_cache(bool async)
    {
        await base.Where_field_access_closure_via_query_cache(async);

        AssertSql(
            """
@__city_InstanceFieldValue_0='London' (Size = 15)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_InstanceFieldValue_0
""",
            //
            """
@__city_InstanceFieldValue_0='Seattle' (Size = 15)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_InstanceFieldValue_0
""");
    }

    public override async Task Where_property_access_closure_via_query_cache(bool async)
    {
        await base.Where_property_access_closure_via_query_cache(async);

        AssertSql(
            """
@__city_InstancePropertyValue_0='London' (Size = 15)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_InstancePropertyValue_0
""",
            //
            """
@__city_InstancePropertyValue_0='Seattle' (Size = 15)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_InstancePropertyValue_0
""");
    }

    public override async Task Where_static_field_access_closure_via_query_cache(bool async)
    {
        await base.Where_static_field_access_closure_via_query_cache(async);

        AssertSql(
            """
@__StaticFieldValue_0='London' (Size = 15)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__StaticFieldValue_0
""",
            //
            """
@__StaticFieldValue_0='Seattle' (Size = 15)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__StaticFieldValue_0
""");
    }

    public override async Task Where_static_property_access_closure_via_query_cache(bool async)
    {
        await base.Where_static_property_access_closure_via_query_cache(async);

        AssertSql(
            """
@__StaticPropertyValue_0='London' (Size = 15)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__StaticPropertyValue_0
""",
            //
            """
@__StaticPropertyValue_0='Seattle' (Size = 15)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__StaticPropertyValue_0
""");
    }

    public override async Task Where_nested_field_access_closure_via_query_cache(bool async)
    {
        await base.Where_nested_field_access_closure_via_query_cache(async);

        AssertSql(
            """
@__city_Nested_InstanceFieldValue_0='London' (Size = 15)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_Nested_InstanceFieldValue_0
""",
            //
            """
@__city_Nested_InstanceFieldValue_0='Seattle' (Size = 15)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_Nested_InstanceFieldValue_0
""");
    }

    public override async Task Where_nested_property_access_closure_via_query_cache(bool async)
    {
        await base.Where_nested_property_access_closure_via_query_cache(async);

        AssertSql(
            """
@__city_Nested_InstancePropertyValue_0='London' (Size = 15)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_Nested_InstancePropertyValue_0
""",
            //
            """
@__city_Nested_InstancePropertyValue_0='Seattle' (Size = 15)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__city_Nested_InstancePropertyValue_0
""");
    }

    public override async Task Where_new_instance_field_access_query_cache(bool async)
    {
        await base.Where_new_instance_field_access_query_cache(async);

        AssertSql(
            """
@__InstanceFieldValue_0='London' (Size = 15)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__InstanceFieldValue_0
""",
            //
            """
@__InstanceFieldValue_0='Seattle' (Size = 15)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__InstanceFieldValue_0
""");
    }

    public override async Task Where_new_instance_field_access_closure_via_query_cache(bool async)
    {
        await base.Where_new_instance_field_access_closure_via_query_cache(async);

        AssertSql(
            """
@__InstanceFieldValue_0='London' (Size = 15)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__InstanceFieldValue_0
""",
            //
            """
@__InstanceFieldValue_0='Seattle' (Size = 15)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = @__InstanceFieldValue_0
""");
    }

    public override async Task Where_simple_closure_via_query_cache_nullable_type(bool async)
    {
        await base.Where_simple_closure_via_query_cache_nullable_type(async);

        AssertSql(
            """
@__p_0='2' (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE CAST([e].[ReportsTo] AS bigint) = @__p_0
""",
            //
            """
@__p_0='5' (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE CAST([e].[ReportsTo] AS bigint) = @__p_0
""",
            //
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] IS NULL
""");
    }

    public override async Task Where_simple_closure_via_query_cache_nullable_type_reverse(bool async)
    {
        await base.Where_simple_closure_via_query_cache_nullable_type_reverse(async);

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] IS NULL
""",
            //
            """
@__p_0='5' (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE CAST([e].[ReportsTo] AS bigint) = @__p_0
""",
            //
            """
@__p_0='2' (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE CAST([e].[ReportsTo] AS bigint) = @__p_0
""");
    }

    public override async Task Where_subquery_closure_via_query_cache(bool async)
    {
        await base.Where_subquery_closure_via_query_cache(async);

        AssertSql(
            """
@__customerID_0='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] = @__customerID_0 AND [o].[CustomerID] = [c].[CustomerID])
""",
            //
            """
@__customerID_0='ANATR' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] = @__customerID_0 AND [o].[CustomerID] = [c].[CustomerID])
""");
    }

    public override async Task Where_bitwise_or(bool async)
    {
        await base.Where_bitwise_or(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE CASE
    WHEN [c].[CustomerID] = N'ALFKI' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END | CASE
    WHEN [c].[CustomerID] = N'ANATR' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END = CAST(1 AS bit)
""");
    }

    public override async Task Where_bitwise_and(bool async)
    {
        await base.Where_bitwise_and(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE CASE
    WHEN [c].[CustomerID] = N'ALFKI' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END & CASE
    WHEN [c].[CustomerID] = N'ANATR' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END = CAST(1 AS bit)
""");
    }

    public override async Task Where_bitwise_xor(bool async)
    {
        // Cannot eval 'where (([c].CustomerID == \"ALFKI\") ^ True)'. Issue #16645.
        await AssertTranslationFailed(() => base.Where_bitwise_xor(async));

        AssertSql();
    }

    public override async Task Where_simple_shadow(bool async)
    {
        await base.Where_simple_shadow(async);

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[Title] = N'Sales Representative'
""");
    }

    public override async Task Where_simple_shadow_projection(bool async)
    {
        await base.Where_simple_shadow_projection(async);

        AssertSql(
            """
SELECT [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[Title] = N'Sales Representative'
""");
    }

    public override async Task Where_shadow_subquery_FirstOrDefault(bool async)
    {
        await base.Where_shadow_subquery_FirstOrDefault(async);

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[Title] = (
    SELECT TOP(1) [e0].[Title]
    FROM [Employees] AS [e0]
    ORDER BY [e0].[Title]) OR ([e].[Title] IS NULL AND (
    SELECT TOP(1) [e0].[Title]
    FROM [Employees] AS [e0]
    ORDER BY [e0].[Title]) IS NULL)
""");
    }

    public override async Task Where_subquery_correlated(bool async)
    {
        await base.Where_subquery_correlated(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Customers] AS [c0]
    WHERE [c].[CustomerID] = [c0].[CustomerID])
""");
    }

    public override async Task Where_equals_method_string(bool async)
    {
        await base.Where_equals_method_string(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'
""");
    }

    public override async Task Where_equals_method_int(bool async)
    {
        await base.Where_equals_method_int(async);

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = 1
""");
    }

    public override async Task Where_equals_using_object_overload_on_mismatched_types(bool async)
    {
        await base.Where_equals_using_object_overload_on_mismatched_types(async);

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE 0 = 1
""");

        Assert.Contains(
            "Possible unintended use of method 'Equals' for arguments 'e.EmployeeID' and '@__longPrm_0' of different types in a query. This comparison will always return false.",
            Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));
    }

    public override async Task Where_equals_using_int_overload_on_mismatched_types(bool async)
    {
        await base.Where_equals_using_int_overload_on_mismatched_types(async);

        AssertSql(
            """
@__p_0='1'

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = @__p_0
""");
    }

    public override async Task Where_equals_on_mismatched_types_nullable_int_long(bool async)
    {
        await base.Where_equals_on_mismatched_types_nullable_int_long(async);

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE 0 = 1
""",
            //
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE 0 = 1
""");

        Assert.Contains(
            "Possible unintended use of method 'Equals' for arguments 'e.ReportsTo' and '@__longPrm_0' of different types in a query. This comparison will always return false.",
            Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));
        Assert.Contains(
            "Possible unintended use of method 'Equals' for arguments '@__longPrm_0' and 'e.ReportsTo' of different types in a query. This comparison will always return false.",
            Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));
    }

    public override async Task Where_equals_on_mismatched_types_nullable_long_nullable_int(bool async)
    {
        await base.Where_equals_on_mismatched_types_nullable_long_nullable_int(async);

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE 0 = 1
""",
            //
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE 0 = 1
""");

        Assert.Contains(
            "Possible unintended use of method 'Equals' for arguments 'e.ReportsTo' and '@__nullableLongPrm_0' of different types in a query. This comparison will always return false.",
            Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));
        Assert.Contains(
            "Possible unintended use of method 'Equals' for arguments '@__nullableLongPrm_0' and 'e.ReportsTo' of different types in a query. This comparison will always return false.",
            Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));
    }

    public override async Task Where_equals_on_mismatched_types_int_nullable_int(bool async)
    {
        await base.Where_equals_on_mismatched_types_int_nullable_int(async);

        AssertSql(
            """
@__intPrm_0='2'

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] = @__intPrm_0
""",
            //
            """
@__intPrm_0='2'

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE @__intPrm_0 = [e].[ReportsTo]
""");
    }

    public override async Task Where_equals_on_matched_nullable_int_types(bool async)
    {
        await base.Where_equals_on_matched_nullable_int_types(async);

        AssertSql(
            """
@__nullableIntPrm_0='2' (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE @__nullableIntPrm_0 = [e].[ReportsTo]
""",
            //
            """
@__nullableIntPrm_0='2' (Nullable = true)

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] = @__nullableIntPrm_0
""");
    }

    public override async Task Where_equals_on_null_nullable_int_types(bool async)
    {
        await base.Where_equals_on_null_nullable_int_types(async);

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] IS NULL
""",
            //
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] IS NULL
""");
    }

    public override async Task Where_comparison_nullable_type_not_null(bool async)
    {
        await base.Where_comparison_nullable_type_not_null(async);

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] = 2
""");
    }

    public override async Task Where_comparison_nullable_type_null(bool async)
    {
        await base.Where_comparison_nullable_type_null(async);

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] IS NULL
""");
    }

    public override async Task Where_string_length(bool async)
    {
        await base.Where_string_length(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE CAST(LEN([c].[City]) AS int) = 6
""");
    }

    public override async Task Where_string_indexof(bool async)
    {
        await base.Where_string_indexof(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE CHARINDEX(N'Sea', [c].[City]) - 1 <> -1 OR [c].[City] IS NULL
""");
    }

    public override async Task Where_string_replace(bool async)
    {
        await base.Where_string_replace(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE REPLACE([c].[City], N'Sea', N'Rea') = N'Reattle'
""");
    }

    public override async Task Where_string_substring(bool async)
    {
        await base.Where_string_substring(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE SUBSTRING([c].[City], 1 + 1, 2) = N'ea'
""");
    }

    public override async Task Where_datetime_now(bool async)
    {
        await base.Where_datetime_now(async);

        AssertSql(
            """
@__myDatetime_0='2015-04-10T00:00:00.0000000'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE GETDATE() <> @__myDatetime_0
""");
    }

    public override async Task Where_datetime_utcnow(bool async)
    {
        await base.Where_datetime_utcnow(async);

        AssertSql(
            """
@__myDatetime_0='2015-04-10T00:00:00.0000000'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE GETUTCDATE() <> @__myDatetime_0
""");
    }

    public override async Task Where_datetimeoffset_utcnow(bool async)
    {
        await base.Where_datetimeoffset_utcnow(async);

        AssertSql(
            """
@__myDatetimeOffset_0='2015-04-10T00:00:00.0000000-08:00'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE CAST(SYSUTCDATETIME() AS datetimeoffset) <> @__myDatetimeOffset_0
""");
    }

    public override async Task Where_datetime_today(bool async)
    {
        await base.Where_datetime_today(async);

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE CONVERT(date, GETDATE()) = CONVERT(date, GETDATE())
""");
    }

    public override async Task Where_datetime_date_component(bool async)
    {
        await base.Where_datetime_date_component(async);

        AssertSql(
            """
@__myDatetime_0='1998-05-04T00:00:00.0000000' (DbType = DateTime)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE CONVERT(date, [o].[OrderDate]) = @__myDatetime_0
""");
    }

    public override async Task Where_date_add_year_constant_component(bool async)
    {
        await base.Where_date_add_year_constant_component(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(year, DATEADD(year, CAST(-1 AS int), [o].[OrderDate])) = 1997
""");
    }

    public override async Task Where_datetime_year_component(bool async)
    {
        await base.Where_datetime_year_component(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(year, [o].[OrderDate]) = 1998
""");
    }

    public override async Task Where_datetime_month_component(bool async)
    {
        await base.Where_datetime_month_component(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(month, [o].[OrderDate]) = 4
""");
    }

    public override async Task Where_datetime_dayOfYear_component(bool async)
    {
        await base.Where_datetime_dayOfYear_component(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(dayofyear, [o].[OrderDate]) = 68
""");
    }

    public override async Task Where_datetime_day_component(bool async)
    {
        await base.Where_datetime_day_component(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(day, [o].[OrderDate]) = 4
""");
    }

    public override async Task Where_datetime_hour_component(bool async)
    {
        await base.Where_datetime_hour_component(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(hour, [o].[OrderDate]) = 0
""");
    }

    public override async Task Where_datetime_minute_component(bool async)
    {
        await base.Where_datetime_minute_component(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(minute, [o].[OrderDate]) = 0
""");
    }

    public override async Task Where_datetime_second_component(bool async)
    {
        await base.Where_datetime_second_component(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(second, [o].[OrderDate]) = 0
""");
    }

    public override async Task Where_datetime_millisecond_component(bool async)
    {
        await base.Where_datetime_millisecond_component(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE DATEPART(millisecond, [o].[OrderDate]) = 0
""");
    }

    public override async Task Where_datetimeoffset_now_component(bool async)
    {
        await base.Where_datetimeoffset_now_component(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE CAST([o].[OrderDate] AS datetimeoffset) < SYSDATETIMEOFFSET()
""");
    }

    public override async Task Where_datetimeoffset_utcnow_component(bool async)
    {
        await base.Where_datetimeoffset_utcnow_component(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE CAST([o].[OrderDate] AS datetimeoffset) <> CAST(SYSUTCDATETIME() AS datetimeoffset) OR [o].[OrderDate] IS NULL
""");
    }

    public override async Task Where_simple_reversed(bool async)
    {
        await base.Where_simple_reversed(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE N'London' = [c].[City]
""");
    }

    public override async Task Where_is_null(bool async)
    {
        await base.Where_is_null(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[Region] IS NULL
""");
    }

    public override async Task Where_null_is_null(bool async)
    {
        await base.Where_null_is_null(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""");
    }

    public override async Task Where_constant_is_null(bool async)
    {
        await base.Where_constant_is_null(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 0 = 1
""");
    }

    public override async Task Where_is_not_null(bool async)
    {
        await base.Where_is_not_null(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] IS NOT NULL
""");
    }

    public override async Task Where_null_is_not_null(bool async)
    {
        await base.Where_null_is_not_null(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 0 = 1
""");
    }

    public override async Task Where_constant_is_not_null(bool async)
    {
        await base.Where_constant_is_not_null(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""");
    }

    public override async Task Where_identity_comparison(bool async)
    {
        await base.Where_identity_comparison(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = [c].[City] OR [c].[City] IS NULL
""");
    }

    public override async Task Where_in_optimization_multiple(bool async)
    {
        await base.Where_in_optimization_multiple(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE [c].[City] IN (N'London', N'Berlin') OR [c].[CustomerID] = N'ALFKI' OR [c].[CustomerID] = N'ABCDE'
""");
    }

    public override async Task Where_not_in_optimization1(bool async)
    {
        await base.Where_not_in_optimization1(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE ([c].[City] <> N'London' OR [c].[City] IS NULL) AND ([e].[City] <> N'London' OR [e].[City] IS NULL)
""");
    }

    public override async Task Where_not_in_optimization2(bool async)
    {
        await base.Where_not_in_optimization2(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE [c].[City] NOT IN (N'London', N'Berlin') OR [c].[City] IS NULL
""");
    }

    public override async Task Where_not_in_optimization3(bool async)
    {
        await base.Where_not_in_optimization3(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE [c].[City] NOT IN (N'London', N'Berlin', N'Seattle') OR [c].[City] IS NULL
""");
    }

    public override async Task Where_not_in_optimization4(bool async)
    {
        await base.Where_not_in_optimization4(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE [c].[City] NOT IN (N'London', N'Berlin', N'Seattle', N'Lisboa') OR [c].[City] IS NULL
""");
    }

    public override async Task Where_select_many_and(bool async)
    {
        await base.Where_select_many_and(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
CROSS JOIN [Employees] AS [e]
WHERE [c].[City] = N'London' AND [c].[Country] = N'UK' AND [e].[City] = N'London' AND [e].[Country] = N'UK'
""");
    }

    public override async Task Where_primitive(bool async)
    {
        await base.Where_primitive(async);

        AssertSql(
            """
@__p_0='9'

SELECT [e0].[EmployeeID]
FROM (
    SELECT TOP(@__p_0) [e].[EmployeeID]
    FROM [Employees] AS [e]
) AS [e0]
WHERE [e0].[EmployeeID] = 5
""");
    }

    public override async Task Where_bool_member(bool async)
    {
        await base.Where_bool_member(async);

        AssertSql(
            """
SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = CAST(1 AS bit)
""");
    }

    public override async Task Where_bool_member_false(bool async)
    {
        await base.Where_bool_member_false(async);

        AssertSql(
            """
SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = CAST(0 AS bit)
""");
    }

    public override async Task Where_bool_member_negated_twice(bool async)
    {
        await base.Where_bool_member_negated_twice(async);

        AssertSql(
            """
SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = CAST(1 AS bit)
""");
    }

    public override async Task Where_bool_member_shadow(bool async)
    {
        await base.Where_bool_member_shadow(async);

        AssertSql(
            """
SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = CAST(1 AS bit)
""");
    }

    public override async Task Where_bool_member_false_shadow(bool async)
    {
        await base.Where_bool_member_false_shadow(async);

        AssertSql(
            """
SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = CAST(0 AS bit)
""");
    }

    public override async Task Where_bool_member_equals_constant(bool async)
    {
        await base.Where_bool_member_equals_constant(async);

        AssertSql(
            """
SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = CAST(1 AS bit)
""");
    }

    public override async Task Where_bool_member_in_complex_predicate(bool async)
    {
        await base.Where_bool_member_in_complex_predicate(async);

        AssertSql(
            """
SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE ([p].[ProductID] > 100 AND [p].[Discontinued] = CAST(1 AS bit)) OR [p].[Discontinued] = CAST(1 AS bit)
""");
    }

    public override async Task Where_bool_member_compared_to_binary_expression(bool async)
    {
        await base.Where_bool_member_compared_to_binary_expression(async);

        AssertSql(
            """
SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = CASE
    WHEN [p].[ProductID] > 50 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task Where_not_bool_member_compared_to_not_bool_member(bool async)
    {
        await base.Where_not_bool_member_compared_to_not_bool_member(async);

        AssertSql(
            """
SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
""");
    }

    public override async Task Where_negated_boolean_expression_compared_to_another_negated_boolean_expression(bool async)
    {
        await base.Where_negated_boolean_expression_compared_to_another_negated_boolean_expression(async);

        AssertSql(
            """
SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE CASE
    WHEN [p].[ProductID] <= 50 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END = CASE
    WHEN [p].[ProductID] <= 20 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task Where_not_bool_member_compared_to_binary_expression(bool async)
    {
        await base.Where_not_bool_member_compared_to_binary_expression(async);

        AssertSql(
            """
SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] <> CASE
    WHEN [p].[ProductID] > 50 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task Where_bool_parameter(bool async)
    {
        await base.Where_bool_parameter(async);

        AssertSql(
            """
@__prm_0='True'

SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE @__prm_0 = CAST(1 AS bit)
""");
    }

    public override async Task Where_bool_parameter_compared_to_binary_expression(bool async)
    {
        await base.Where_bool_parameter_compared_to_binary_expression(async);

        AssertSql(
            """
@__prm_0='True'

SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE CASE
    WHEN [p].[ProductID] > 50 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END <> @__prm_0
""");
    }

    public override async Task Where_bool_member_and_parameter_compared_to_binary_expression_nested(bool async)
    {
        await base.Where_bool_member_and_parameter_compared_to_binary_expression_nested(async);

        AssertSql(
            """
@__prm_0='True'

SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = CASE
    WHEN CASE
        WHEN [p].[ProductID] > 50 THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END <> @__prm_0 THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    public override async Task Where_de_morgan_or_optimized(bool async)
    {
        await base.Where_de_morgan_or_optimized(async);

        AssertSql(
            """
SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = CAST(0 AS bit) AND [p].[ProductID] >= 20
""");
    }

    public override async Task Where_de_morgan_and_optimized(bool async)
    {
        await base.Where_de_morgan_and_optimized(async);

        AssertSql(
            """
SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = CAST(0 AS bit) OR [p].[ProductID] >= 20
""");
    }

    public override async Task Where_complex_negated_expression_optimized(bool async)
    {
        await base.Where_complex_negated_expression_optimized(async);

        AssertSql(
            """
SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[Discontinued] = CAST(0 AS bit) AND [p].[ProductID] < 60 AND [p].[ProductID] > 30
""");
    }

    public override async Task Where_short_member_comparison(bool async)
    {
        await base.Where_short_member_comparison(async);

        AssertSql(
            """
SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[UnitsInStock] > CAST(10 AS smallint)
""");
    }

    public override async Task Where_comparison_to_nullable_bool(bool async)
    {
        await base.Where_comparison_to_nullable_bool(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'%KI'
""");
    }

    public override async Task Where_true(bool async)
    {
        await base.Where_true(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""");
    }

    public override async Task Where_false(bool async)
    {
        await base.Where_false(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 0 = 1
""");
    }

    public override async Task Where_default(bool async)
    {
        await base.Where_default(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[Fax] IS NULL
""");
    }

    public override async Task Where_expression_invoke_1(bool async)
    {
        await base.Where_expression_invoke_1(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
""");
    }

    public override async Task Where_expression_invoke_2(bool async)
    {
        await base.Where_expression_invoke_2(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [c].[CustomerID] = N'ALFKI'
""");
    }

    public override async Task Where_expression_invoke_3(bool async)
    {
        await base.Where_expression_invoke_3(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
""");
    }

    public override async Task Where_concat_string_int_comparison1(bool async)
    {
        await base.Where_concat_string_int_comparison1(async);

        AssertSql(
            """
@__i_0='10'

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] + CAST(@__i_0 AS nvarchar(max)) = [c].[CompanyName]
""");
    }

    public override async Task Where_concat_string_int_comparison2(bool async)
    {
        await base.Where_concat_string_int_comparison2(async);

        AssertSql(
            """
@__i_0='10'

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE CAST(@__i_0 AS nvarchar(max)) + [c].[CustomerID] = [c].[CompanyName]
""");
    }

    public override async Task Where_concat_string_int_comparison3(bool async)
    {
        await base.Where_concat_string_int_comparison3(async);

        AssertSql(
            """
@__p_0='30'
@__j_1='21'

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE CAST(@__p_0 AS nvarchar(max)) + [c].[CustomerID] + CAST(@__j_1 AS nvarchar(max)) + CAST(42 AS nvarchar(max)) = [c].[CompanyName]
""");
    }

    public override async Task Where_concat_string_int_comparison4(bool async)
    {
        await base.Where_concat_string_int_comparison4(async);

        AssertSql(
            """
SELECT [o].[CustomerID]
FROM [Orders] AS [o]
WHERE CAST([o].[OrderID] AS nvarchar(max)) + COALESCE([o].[CustomerID], N'') = [o].[CustomerID]
""");
    }

    public override async Task Where_concat_string_string_comparison(bool async)
    {
        await base.Where_concat_string_string_comparison(async);

        AssertSql(
            """
@__i_0='A' (Size = 4000)

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE @__i_0 + [c].[CustomerID] = N'AALFKI'
""");
    }

    public override async Task Where_string_concat_method_comparison(bool async)
    {
        await base.Where_string_concat_method_comparison(async);

        AssertSql(
            """
@__i_0='A' (Size = 4000)

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE @__i_0 + [c].[CustomerID] = N'AAROUT'
""");
    }

    public override async Task Where_string_concat_method_comparison_2(bool async)
    {
        await base.Where_string_concat_method_comparison_2(async);

        AssertSql(
            """
@__i_0='A' (Size = 4000)
@__j_1='B' (Size = 4000)

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE @__i_0 + @__j_1 + [c].[CustomerID] = N'ABANATR'
""");
    }

    public override async Task Where_string_concat_method_comparison_3(bool async)
    {
        await base.Where_string_concat_method_comparison_3(async);

        AssertSql(
            """
@__i_0='A' (Size = 4000)
@__j_1='B' (Size = 4000)
@__k_2='C' (Size = 4000)

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE @__i_0 + @__j_1 + @__k_2 + [c].[CustomerID] = N'ABCANTON'
""");
    }

    public override async Task Where_ternary_boolean_condition_true(bool async)
    {
        await base.Where_ternary_boolean_condition_true(async);

        AssertSql(
            """
SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[UnitsInStock] >= CAST(20 AS smallint)
""");
    }

    public override async Task Where_ternary_boolean_condition_false(bool async)
    {
        await base.Where_ternary_boolean_condition_false(async);

        AssertSql(
            """
SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[UnitsInStock] < CAST(20 AS smallint)
""");
    }

    public override async Task Where_ternary_boolean_condition_with_another_condition(bool async)
    {
        await base.Where_ternary_boolean_condition_with_another_condition(async);

        AssertSql(
            """
@__productId_0='15'

SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[ProductID] < @__productId_0 AND [p].[UnitsInStock] >= CAST(20 AS smallint)
""");
    }

    public override async Task Where_ternary_boolean_condition_with_false_as_result_true(bool async)
    {
        await base.Where_ternary_boolean_condition_with_false_as_result_true(async);

        AssertSql(
            """
SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[UnitsInStock] >= CAST(20 AS smallint)
""");
    }

    public override async Task Where_ternary_boolean_condition_with_false_as_result_false(bool async)
    {
        await base.Where_ternary_boolean_condition_with_false_as_result_false(async);

        AssertSql(
            """
SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE 0 = 1
""");
    }

    public override async Task Where_compare_constructed_equal(bool async)
    {
        //  Anonymous type to constant comparison. Issue #14672.
        await AssertTranslationFailed(() => base.Where_compare_constructed_equal(async));

        AssertSql();
    }

    public override async Task Where_compare_constructed_multi_value_equal(bool async)
    {
        //  Anonymous type to constant comparison. Issue #14672.
        await AssertTranslationFailed(() => base.Where_compare_constructed_multi_value_equal(async));

        AssertSql();
    }

    public override async Task Where_compare_constructed_multi_value_not_equal(bool async)
    {
        //  Anonymous type to constant comparison. Issue #14672.
        await AssertTranslationFailed(() => base.Where_compare_constructed_multi_value_not_equal(async));

        AssertSql();
    }

    public override async Task Where_compare_tuple_constructed_equal(bool async)
    {
        //  Anonymous type to constant comparison. Issue #14672.
        await AssertTranslationFailed(() => base.Where_compare_tuple_constructed_equal(async));

        AssertSql();
    }

    public override async Task Where_compare_tuple_constructed_multi_value_equal(bool async)
    {
        //  Anonymous type to constant comparison. Issue #14672.
        await AssertTranslationFailed(() => base.Where_compare_tuple_constructed_multi_value_equal(async));

        AssertSql();
    }

    public override async Task Where_compare_tuple_constructed_multi_value_not_equal(bool async)
    {
        //  Anonymous type to constant comparison. Issue #14672.
        await AssertTranslationFailed(() => base.Where_compare_tuple_constructed_multi_value_not_equal(async));

        AssertSql();
    }

    public override async Task Where_compare_tuple_create_constructed_equal(bool async)
    {
        //  Anonymous type to constant comparison. Issue #14672.
        await AssertTranslationFailed(() => base.Where_compare_tuple_create_constructed_equal(async));

        AssertSql();
    }

    public override async Task Where_compare_tuple_create_constructed_multi_value_equal(bool async)
    {
        //  Anonymous type to constant comparison. Issue #14672.
        await AssertTranslationFailed(() => base.Where_compare_tuple_create_constructed_multi_value_equal(async));

        AssertSql();
    }

    public override async Task Where_compare_tuple_create_constructed_multi_value_not_equal(bool async)
    {
        //  Anonymous type to constant comparison. Issue #14672.
        await AssertTranslationFailed(() => base.Where_compare_tuple_create_constructed_multi_value_not_equal(async));

        AssertSql();
    }

    public override async Task Where_compare_null(bool async)
    {
        await base.Where_compare_null(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[Region] IS NULL AND [c].[Country] = N'UK'
""");
    }

    public override async Task Where_compare_null_with_cast_to_object(bool async)
    {
        await base.Where_compare_null_with_cast_to_object(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[Region] IS NULL
""");
    }

    public override async Task Where_compare_with_both_cast_to_object(bool async)
    {
        await base.Where_compare_with_both_cast_to_object(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'
""");
    }

    public override async Task Where_Is_on_same_type(bool async)
    {
        await base.Where_Is_on_same_type(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""");
    }

    public override async Task Where_chain(bool async)
    {
        await base.Where_chain(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'QUICK' AND [o].[OrderDate] > '1998-01-01T00:00:00.000'
""");
    }

    public override async Task Where_navigation_contains(bool async)
    {
        await base.Where_navigation_contains(async);

        AssertSql(
            """
SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] = N'ALFKI'
) AS [c0]
LEFT JOIN [Orders] AS [o] ON [c0].[CustomerID] = [o].[CustomerID]
ORDER BY [c0].[CustomerID]
""",
            //
            """
SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
WHERE [o0].[OrderID] IN (10643, 10692, 10702, 10835, 10952, 11011)
""");
    }

    public override async Task Where_array_index(bool async)
    {
        await base.Where_array_index(async);

        AssertSql(
            """
@__p_0='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @__p_0
""");
    }

    public override async Task Where_multiple_contains_in_subquery_with_or(bool async)
    {
        await base.Where_multiple_contains_in_subquery_with_or(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE [o].[ProductID] IN (
    SELECT TOP(1) [p].[ProductID]
    FROM [Products] AS [p]
    ORDER BY [p].[ProductID]
) OR [o].[OrderID] IN (
    SELECT TOP(1) [o0].[OrderID]
    FROM [Orders] AS [o0]
    ORDER BY [o0].[OrderID]
)
""");
    }

    public override async Task Where_multiple_contains_in_subquery_with_and(bool async)
    {
        await base.Where_multiple_contains_in_subquery_with_and(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE [o].[ProductID] IN (
    SELECT TOP(20) [p].[ProductID]
    FROM [Products] AS [p]
    ORDER BY [p].[ProductID]
) AND [o].[OrderID] IN (
    SELECT TOP(10) [o0].[OrderID]
    FROM [Orders] AS [o0]
    ORDER BY [o0].[OrderID]
)
""");
    }

    public override async Task Where_contains_on_navigation(bool async)
    {
        await base.Where_contains_on_navigation(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM [Customers] AS [c]
    WHERE [o].[OrderID] IN (
        SELECT [o0].[OrderID]
        FROM [Orders] AS [o0]
        WHERE [c].[CustomerID] = [o0].[CustomerID]
    ))
""");
    }

    public override async Task Where_subquery_FirstOrDefault_is_null(bool async)
    {
        await base.Where_subquery_FirstOrDefault_is_null(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE NOT EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID])
""");
    }

    public override async Task Where_subquery_FirstOrDefault_compared_to_entity(bool async)
    {
        await base.Where_subquery_FirstOrDefault_compared_to_entity(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (
    SELECT TOP(1) [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID]) = 10276
""");
    }

    public override async Task Time_of_day_datetime(bool async)
    {
        await base.Time_of_day_datetime(async);

        AssertSql(
            """
SELECT CONVERT(time, [o].[OrderDate])
FROM [Orders] AS [o]
""");
    }

    public override async Task TypeBinary_short_circuit(bool async)
    {
        await base.TypeBinary_short_circuit(async);

        AssertSql(
            """
@__p_0='False'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE @__p_0 = CAST(1 AS bit)
""");
    }

    public override async Task Where_is_conditional(bool async)
    {
        await base.Where_is_conditional(async);

        AssertSql(
            """
SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE 0 = 1
""");
    }

    public override async Task Enclosing_class_settable_member_generates_parameter(bool async)
    {
        await base.Enclosing_class_settable_member_generates_parameter(async);

        AssertSql(
            """
@__SettableProperty_0='10274'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] = @__SettableProperty_0
""",
                //
                """
@__SettableProperty_0='10275'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] = @__SettableProperty_0
""");
    }

    public override async Task Enclosing_class_readonly_member_generates_parameter(bool async)
    {
        await base.Enclosing_class_readonly_member_generates_parameter(async);

        AssertSql(
            """
@__ReadOnlyProperty_0='10275'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] = @__ReadOnlyProperty_0
""");
    }

    public override async Task Enclosing_class_const_member_does_not_generate_parameter(bool async)
    {
        await base.Enclosing_class_const_member_does_not_generate_parameter(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] = 10274
""");
    }

    public override async Task Generic_Ilist_contains_translates_to_server(bool async)
    {
        await base.Generic_Ilist_contains_translates_to_server(async);

        AssertSql(
            """
@__cities_0='["Seattle"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] IN (
    SELECT [c0].[value]
    FROM OPENJSON(@__cities_0) WITH ([value] nvarchar(15) '$') AS [c0]
)
""");
    }

    public override async Task Filter_non_nullable_value_after_FirstOrDefault_on_empty_collection(bool async)
    {
        await base.Filter_non_nullable_value_after_FirstOrDefault_on_empty_collection(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (
    SELECT TOP(1) CAST(LEN([o].[CustomerID]) AS int)
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] = N'John Doe') = 0
""");
    }

    public override async Task Like_with_non_string_column_using_ToString(bool async)
    {
        await base.Like_with_non_string_column_using_ToString(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE CONVERT(varchar(11), [o].[OrderID]) LIKE '%20%'
""");
    }

    public override async Task Like_with_non_string_column_using_double_cast(bool async)
    {
        await base.Like_with_non_string_column_using_double_cast(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE CAST([o].[OrderID] AS nvarchar(max)) LIKE N'%20%'
""");
    }

    [ConditionalTheory(Skip = "issue #31917")]
    public override async Task Using_same_parameter_twice_in_query_generates_one_sql_parameter(bool async)
    {
        await base.Using_same_parameter_twice_in_query_generates_one_sql_parameter(async);

        AssertSql(
            """
@__i_0='10'

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE CAST(@__i_0 AS nchar(5)) + [c].[CustomerID] + CAST(@__i_0 AS nchar(5)) = N'10ALFKI10'
""");
    }

    public override async Task Where_Queryable_ToList_Count(bool async)
    {
        await base.Where_Queryable_ToList_Count(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
WHERE (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] = [c].[CustomerID]) = 0
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Where_Queryable_ToList_Contains(bool async)
    {
        await base.Where_Queryable_ToList_Contains(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [o0].[CustomerID], [o0].[OrderID]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
WHERE N'ALFKI' IN (
    SELECT [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] = [c].[CustomerID]
)
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Where_Queryable_ToArray_Count(bool async)
    {
        await base.Where_Queryable_ToArray_Count(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
WHERE (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] = [c].[CustomerID]) = 0
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Where_Queryable_ToArray_Contains(bool async)
    {
        await base.Where_Queryable_ToArray_Contains(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [o0].[CustomerID], [o0].[OrderID]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
WHERE N'ALFKI' IN (
    SELECT [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] = [c].[CustomerID]
)
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Where_Queryable_AsEnumerable_Count(bool async)
    {
        await base.Where_Queryable_AsEnumerable_Count(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
WHERE (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] = [c].[CustomerID]) = 0
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Where_Queryable_AsEnumerable_Contains(bool async)
    {
        await base.Where_Queryable_AsEnumerable_Contains(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [o0].[CustomerID], [o0].[OrderID]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
WHERE N'ALFKI' IN (
    SELECT [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] = [c].[CustomerID]
)
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Where_Queryable_AsEnumerable_Contains_negated(bool async)
    {
        await base.Where_Queryable_AsEnumerable_Contains_negated(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [o0].[CustomerID], [o0].[OrderID]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
WHERE NOT EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] = [c].[CustomerID] AND [o].[CustomerID] = N'ALFKI')
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Where_Queryable_ToList_Count_member(bool async)
    {
        await base.Where_Queryable_ToList_Count_member(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
WHERE (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] = [c].[CustomerID]) = 0
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Where_Queryable_ToArray_Length_member(bool async)
    {
        await base.Where_Queryable_ToArray_Length_member(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
WHERE (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] = [c].[CustomerID]) = 0
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Where_collection_navigation_ToList_Count(bool async)
    {
        await base.Where_collection_navigation_ToList_Count(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o1].[OrderID], [o1].[ProductID], [o1].[Discount], [o1].[Quantity], [o1].[UnitPrice]
FROM [Orders] AS [o]
LEFT JOIN [Order Details] AS [o1] ON [o].[OrderID] = [o1].[OrderID]
WHERE [o].[OrderID] < 10300 AND (
    SELECT COUNT(*)
    FROM [Order Details] AS [o0]
    WHERE [o].[OrderID] = [o0].[OrderID]) = 4
ORDER BY [o].[OrderID], [o1].[OrderID]
""");
    }

    public override async Task Where_collection_navigation_ToList_Contains(bool async)
    {
        await base.Where_collection_navigation_ToList_Contains(async);

        AssertSql(
            """
@__entity_equality_order_0_OrderID='10248' (Nullable = true)

SELECT [c].[CustomerID], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID] AND [o].[OrderID] = @__entity_equality_order_0_OrderID)
ORDER BY [c].[CustomerID], [o0].[OrderID]
""");
    }

    public override async Task Where_collection_navigation_ToArray_Count(bool async)
    {
        await base.Where_collection_navigation_ToArray_Count(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o1].[OrderID], [o1].[ProductID], [o1].[Discount], [o1].[Quantity], [o1].[UnitPrice]
FROM [Orders] AS [o]
LEFT JOIN [Order Details] AS [o1] ON [o].[OrderID] = [o1].[OrderID]
WHERE [o].[OrderID] < 10300 AND (
    SELECT COUNT(*)
    FROM [Order Details] AS [o0]
    WHERE [o].[OrderID] = [o0].[OrderID]) = 4
ORDER BY [o].[OrderID], [o1].[OrderID]
""");
    }

    public override async Task Where_collection_navigation_ToArray_Contains(bool async)
    {
        await base.Where_collection_navigation_ToArray_Contains(async);

        AssertSql(
            """
@__entity_equality_order_0_OrderID='10248' (Nullable = true)

SELECT [c].[CustomerID], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID] AND [o].[OrderID] = @__entity_equality_order_0_OrderID)
ORDER BY [c].[CustomerID], [o0].[OrderID]
""");
    }

    public override async Task Where_collection_navigation_AsEnumerable_Count(bool async)
    {
        await base.Where_collection_navigation_AsEnumerable_Count(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o1].[OrderID], [o1].[ProductID], [o1].[Discount], [o1].[Quantity], [o1].[UnitPrice]
FROM [Orders] AS [o]
LEFT JOIN [Order Details] AS [o1] ON [o].[OrderID] = [o1].[OrderID]
WHERE [o].[OrderID] < 10300 AND (
    SELECT COUNT(*)
    FROM [Order Details] AS [o0]
    WHERE [o].[OrderID] = [o0].[OrderID]) = 5
ORDER BY [o].[OrderID], [o1].[OrderID]
""");
    }

    public override async Task Where_collection_navigation_AsEnumerable_Contains(bool async)
    {
        await base.Where_collection_navigation_AsEnumerable_Contains(async);

        AssertSql(
            """
@__entity_equality_order_0_OrderID='10248' (Nullable = true)

SELECT [c].[CustomerID], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID] AND [o].[OrderID] = @__entity_equality_order_0_OrderID)
ORDER BY [c].[CustomerID], [o0].[OrderID]
""");
    }

    public override async Task Where_collection_navigation_ToList_Count_member(bool async)
    {
        await base.Where_collection_navigation_ToList_Count_member(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o1].[OrderID], [o1].[ProductID], [o1].[Discount], [o1].[Quantity], [o1].[UnitPrice]
FROM [Orders] AS [o]
LEFT JOIN [Order Details] AS [o1] ON [o].[OrderID] = [o1].[OrderID]
WHERE [o].[OrderID] < 10300 AND (
    SELECT COUNT(*)
    FROM [Order Details] AS [o0]
    WHERE [o].[OrderID] = [o0].[OrderID]) = 3
ORDER BY [o].[OrderID], [o1].[OrderID]
""");
    }

    public override async Task Where_collection_navigation_ToArray_Length_member(bool async)
    {
        await base.Where_collection_navigation_ToArray_Length_member(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o1].[OrderID], [o1].[ProductID], [o1].[Discount], [o1].[Quantity], [o1].[UnitPrice]
FROM [Orders] AS [o]
LEFT JOIN [Order Details] AS [o1] ON [o].[OrderID] = [o1].[OrderID]
WHERE [o].[OrderID] < 10300 AND (
    SELECT COUNT(*)
    FROM [Order Details] AS [o0]
    WHERE [o].[OrderID] = [o0].[OrderID]) = 3
ORDER BY [o].[OrderID], [o1].[OrderID]
""");
    }

    public override async Task Where_list_object_contains_over_value_type(bool async)
    {
        await base.Where_list_object_contains_over_value_type(async);

        AssertSql(
            """
@__orderIds_0='[10248,10249]' (Size = 4000)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] IN (
    SELECT [o0].[value]
    FROM OPENJSON(@__orderIds_0) WITH ([value] int '$') AS [o0]
)
""");
    }

    public override async Task Where_array_of_object_contains_over_value_type(bool async)
    {
        await base.Where_array_of_object_contains_over_value_type(async);

        AssertSql(
            """
@__orderIds_0='[10248,10249]' (Size = 4000)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] IN (
    SELECT [o0].[value]
    FROM OPENJSON(@__orderIds_0) WITH ([value] int '$') AS [o0]
)
""");
    }

    public override async Task Multiple_OrElse_on_same_column_converted_to_in_with_overlap(bool async)
    {
        await base.Multiple_OrElse_on_same_column_converted_to_in_with_overlap(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ALFKI', N'ANATR', N'ANTON')
""");
    }

    public override async Task Multiple_OrElse_on_same_column_with_null_constant_comparison_converted_to_in(bool async)
    {
        await base.Multiple_OrElse_on_same_column_with_null_constant_comparison_converted_to_in(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[Region] IN (N'WA', N'OR', N'BC') OR [c].[Region] IS NULL
""");
    }

    public override async Task Constant_array_Contains_OrElse_comparison_with_constant_gets_combined_to_one_in(bool async)
    {
        await base.Constant_array_Contains_OrElse_comparison_with_constant_gets_combined_to_one_in(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ALFKI', N'ANATR', N'ANTON')
""");
    }

    public override async Task Constant_array_Contains_OrElse_comparison_with_constant_gets_combined_to_one_in_with_overlap(bool async)
    {
        await base.Constant_array_Contains_OrElse_comparison_with_constant_gets_combined_to_one_in_with_overlap(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ANTON', N'ALFKI', N'ANATR')
""");
    }

    public override async Task Constant_array_Contains_OrElse_another_Contains_gets_combined_to_one_in_with_overlap(bool async)
    {
        await base.Constant_array_Contains_OrElse_another_Contains_gets_combined_to_one_in_with_overlap(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ALFKI', N'ANATR', N'ANTON')
""");
    }

    public override async Task Constant_array_Contains_AndAlso_another_Contains_gets_combined_to_one_in_with_overlap(bool async)
    {
        await base.Constant_array_Contains_AndAlso_another_Contains_gets_combined_to_one_in_with_overlap(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] NOT IN (N'ALFKI', N'ANATR', N'ANTON')
""");
    }

    public override async Task Multiple_AndAlso_on_same_column_converted_to_in_using_parameters(bool async)
    {
        await base.Multiple_AndAlso_on_same_column_converted_to_in_using_parameters(async);

        // issue #21462
        AssertSql(
            """
@__prm1_0='ALFKI' (Size = 5) (DbType = StringFixedLength)
@__prm2_1='ANATR' (Size = 5) (DbType = StringFixedLength)
@__prm3_2='ANTON' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <> @__prm1_0 AND [c].[CustomerID] <> @__prm2_1 AND [c].[CustomerID] <> @__prm3_2
""");
    }

    public override async Task Array_of_parameters_Contains_OrElse_comparison_with_constant_gets_combined_to_one_in(bool async)
    {
        await base.Array_of_parameters_Contains_OrElse_comparison_with_constant_gets_combined_to_one_in(async);

        // issue #21462
        AssertSql(
            """
@__prm1_0='ALFKI' (Size = 5) (DbType = StringFixedLength)
@__prm2_1='ANATR' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (@__prm1_0, @__prm2_1, N'ANTON')
""");
    }

    public override async Task Multiple_OrElse_on_same_column_with_null_parameter_comparison_converted_to_in(bool async)
    {
        await base.Multiple_OrElse_on_same_column_with_null_parameter_comparison_converted_to_in(async);

        // issue #21462
        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[Region] IN (N'WA', N'OR') OR [c].[Region] IS NULL OR [c].[Region] = N'BC'
""");
    }

    public override async Task Parameter_array_Contains_OrElse_comparison_with_constant(bool async)
    {
        await base.Parameter_array_Contains_OrElse_comparison_with_constant(async);

        AssertSql(
            """
@__array_0='["ALFKI","ANATR"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [a].[value]
    FROM OPENJSON(@__array_0) WITH ([value] nchar(5) '$') AS [a]
) OR [c].[CustomerID] = N'ANTON'
""");
    }

    public override async Task Parameter_array_Contains_OrElse_comparison_with_parameter_with_overlap(bool async)
    {
        await base.Parameter_array_Contains_OrElse_comparison_with_parameter_with_overlap(async);

        AssertSql(
            """
@__prm1_0='ANTON' (Size = 5) (DbType = StringFixedLength)
@__array_1='["ALFKI","ANATR"]' (Size = 4000)
@__prm2_2='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @__prm1_0 OR [c].[CustomerID] IN (
    SELECT [a].[value]
    FROM OPENJSON(@__array_1) WITH ([value] nchar(5) '$') AS [a]
) OR [c].[CustomerID] = @__prm2_2
""");
    }

    public override async Task Two_sets_of_comparison_combine_correctly(bool async)
    {
        await base.Two_sets_of_comparison_combine_correctly(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ANATR'
""");
    }

    public override async Task Two_sets_of_comparison_combine_correctly2(bool async)
    {
        await base.Two_sets_of_comparison_combine_correctly2(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[Region] IS NOT NULL AND [c].[Region] <> N'WA'
""");
    }

    public override async Task Filter_with_EF_Property_using_closure_for_property_name(bool async)
    {
        await base.Filter_with_EF_Property_using_closure_for_property_name(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
""");
    }

    public override async Task Filter_with_EF_Property_using_function_for_property_name(bool async)
    {
        await base.Filter_with_EF_Property_using_function_for_property_name(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
""");
    }

    public override async Task FirstOrDefault_over_scalar_projection_compared_to_null(bool async)
    {
        await base.FirstOrDefault_over_scalar_projection_compared_to_null(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (
    SELECT TOP(1) [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]) IS NULL
""");
    }

    public override async Task FirstOrDefault_over_scalar_projection_compared_to_not_null(bool async)
    {
        await base.FirstOrDefault_over_scalar_projection_compared_to_not_null(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (
    SELECT TOP(1) [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]) IS NOT NULL
""");
    }

    public override async Task FirstOrDefault_over_custom_projection_compared_to_null(bool async)
    {
        await base.FirstOrDefault_over_custom_projection_compared_to_null(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE NOT EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID])
""");
    }

    public override async Task FirstOrDefault_over_custom_projection_compared_to_not_null(bool async)
    {
        await base.FirstOrDefault_over_custom_projection_compared_to_not_null(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID])
""");
    }

    public override async Task SingleOrDefault_over_custom_projection_compared_to_null(bool async)
    {
        await base.SingleOrDefault_over_custom_projection_compared_to_null(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE NOT EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID])
""");
    }

    public override async Task SingleOrDefault_over_custom_projection_compared_to_not_null(bool async)
    {
        await base.SingleOrDefault_over_custom_projection_compared_to_not_null(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID])
""");
    }

    public override async Task LastOrDefault_over_custom_projection_compared_to_null(bool async)
    {
        await base.LastOrDefault_over_custom_projection_compared_to_null(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE NOT EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID])
""");
    }

    public override async Task LastOrDefault_over_custom_projection_compared_to_not_null(bool async)
    {
        await base.LastOrDefault_over_custom_projection_compared_to_not_null(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID])
""");
    }

    public override async Task First_over_custom_projection_compared_to_null(bool async)
    {
        await base.First_over_custom_projection_compared_to_null(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE NOT EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID])
""");
    }

    public override async Task First_over_custom_projection_compared_to_not_null(bool async)
    {
        await base.First_over_custom_projection_compared_to_not_null(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID])
""");
    }

    public override async Task ElementAt_over_custom_projection_compared_to_not_null(bool async)
    {
        await base.ElementAt_over_custom_projection_compared_to_not_null(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY (SELECT 1)
    OFFSET 3 ROWS)
""");
    }

    public override async Task ElementAtOrDefault_over_custom_projection_compared_to_null(bool async)
    {
        await base.ElementAtOrDefault_over_custom_projection_compared_to_null(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE NOT EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY (SELECT 1)
    OFFSET 7 ROWS)
""");
    }

    public override async Task Single_over_custom_projection_compared_to_null(bool async)
    {
        await base.Single_over_custom_projection_compared_to_null(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE NOT EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID])
""");
    }

    public override async Task Single_over_custom_projection_compared_to_not_null(bool async)
    {
        await base.Single_over_custom_projection_compared_to_not_null(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID])
""");
    }

    public override async Task Last_over_custom_projection_compared_to_null(bool async)
    {
        await base.Last_over_custom_projection_compared_to_null(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE NOT EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID])
""");
    }

    public override async Task Last_over_custom_projection_compared_to_not_null(bool async)
    {
        await base.Last_over_custom_projection_compared_to_not_null(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID])
""");
    }

    public override async Task Where_Contains_and_comparison(bool async)
    {
        await base.Where_Contains_and_comparison(async);

        AssertSql(
            """
@__customerIds_0='["ALFKI","FISSA","WHITC"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [c0].[value]
    FROM OPENJSON(@__customerIds_0) WITH ([value] nchar(5) '$') AS [c0]
) AND [c].[City] = N'Seattle'
""");
    }

    public override async Task Where_Contains_or_comparison(bool async)
    {
        await base.Where_Contains_or_comparison(async);

        AssertSql(
            """
@__customerIds_0='["ALFKI","FISSA"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [c0].[value]
    FROM OPENJSON(@__customerIds_0) WITH ([value] nchar(5) '$') AS [c0]
) OR [c].[City] = N'Seattle'
""");
    }

    public override async Task Where_Like_and_comparison(bool async)
    {
        await base.Where_Like_and_comparison(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%' AND [c].[City] = N'Seattle'
""");
    }

    public override async Task Where_Like_or_comparison(bool async)
    {
        await base.Where_Like_or_comparison(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%' OR [c].[City] = N'Seattle'
""");
    }

    public override async Task GetType_on_non_hierarchy1(bool async)
    {
        await base.GetType_on_non_hierarchy1(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""");
    }

    public override async Task GetType_on_non_hierarchy2(bool async)
    {
        await base.GetType_on_non_hierarchy2(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 0 = 1
""");
    }

    public override async Task GetType_on_non_hierarchy3(bool async)
    {
        await base.GetType_on_non_hierarchy3(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 0 = 1
""");
    }

    public override async Task GetType_on_non_hierarchy4(bool async)
    {
        await base.GetType_on_non_hierarchy4(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""");
    }

    public override async Task Case_block_simplification_works_correctly(bool async)
    {
        await base.Case_block_simplification_works_correctly(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE CASE
    WHEN [c].[Region] IS NULL THEN N'OR'
    ELSE [c].[Region]
END = N'OR'
""");
    }

    public override async Task Where_poco_closure(bool async)
    {
        await base.Where_poco_closure(async);

        AssertSql(
            """
@__entity_equality_customer_0_CustomerID='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @__entity_equality_customer_0_CustomerID
""",
            //
            """
@__entity_equality_customer_0_CustomerID='ANATR' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @__entity_equality_customer_0_CustomerID
""");
    }

    public override async Task Filter_with_property_compared_to_null_wrapped_in_explicit_convert_to_object(bool async)
    {
        await base.Filter_with_property_compared_to_null_wrapped_in_explicit_convert_to_object(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[Region] IS NULL
""");
    }

    public override async Task Where_simple_shadow_subquery(bool async)
    {
        await base.Where_simple_shadow_subquery(async);

        AssertSql(
            """
@__p_0='5'

SELECT [e0].[EmployeeID], [e0].[City], [e0].[Country], [e0].[FirstName], [e0].[ReportsTo], [e0].[Title]
FROM (
    SELECT TOP(@__p_0) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
    ORDER BY [e].[EmployeeID]
) AS [e0]
WHERE [e0].[Title] = N'Sales Representative'
ORDER BY [e0].[EmployeeID]
""");
    }

    public override async Task Where_primitive_tracked2(bool async)
    {
        await base.Where_primitive_tracked2(async);

        AssertSql(
            """
@__p_0='9'

SELECT [e0].[EmployeeID], [e0].[City], [e0].[Country], [e0].[FirstName], [e0].[ReportsTo], [e0].[Title]
FROM (
    SELECT TOP(@__p_0) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
) AS [e0]
WHERE [e0].[EmployeeID] = 5
""");
    }

    public override async Task Where_projection(bool async)
    {
        await base.Where_projection(async);

        AssertSql(
            """
SELECT [c].[CompanyName]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'
""");
    }

    public override async Task Where_bool_closure(bool async)
    {
        await base.Where_bool_closure(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 0 = 1
""",
            //
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
""",
            //
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
""",
            //
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""");
    }

    public override async Task Where_primitive_tracked(bool async)
    {
        await base.Where_primitive_tracked(async);

        AssertSql(
            """
@__p_0='9'

SELECT [e0].[EmployeeID], [e0].[City], [e0].[Country], [e0].[FirstName], [e0].[ReportsTo], [e0].[Title]
FROM (
    SELECT TOP(@__p_0) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
) AS [e0]
WHERE [e0].[EmployeeID] = 5
""");
    }

    public override async Task Where_simple_shadow_projection_mixed(bool async)
    {
        await base.Where_simple_shadow_projection_mixed(async);

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[Title] = N'Sales Representative'
""");
    }

    public override async Task Decimal_cast_to_double_works(bool async)
    {
        await base.Decimal_cast_to_double_works(async);

        AssertSql(
            """
SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE CAST([p].[UnitPrice] AS float) > 100.0E0
""");
    }

    public override async Task Where_bool_client_side_negated(bool async)
    {
        await base.Where_bool_client_side_negated(async);

        AssertSql();
    }

    public override async Task Where_equals_method_string_with_ignore_case(bool async)
    {
        await base.Where_equals_method_string_with_ignore_case(async);

        AssertSql();
    }

    public override async Task Where_nested_field_access_closure_via_query_cache_error_null(bool async)
    {
        await base.Where_nested_field_access_closure_via_query_cache_error_null(async);

        AssertSql();
    }

    public override async Task Where_nested_field_access_closure_via_query_cache_error_method_null(bool async)
    {
        await base.Where_nested_field_access_closure_via_query_cache_error_method_null(async);

        AssertSql();
    }

    public override async Task Where_client(bool async)
    {
        await base.Where_client(async);

        AssertSql();
    }

    public override async Task Where_subquery_correlated_client_eval(bool async)
    {
        await base.Where_subquery_correlated_client_eval(async);

        AssertSql();
    }

    public override async Task Where_client_and_server_top_level(bool async)
    {
        await base.Where_client_and_server_top_level(async);

        AssertSql();
    }

    public override async Task Where_client_or_server_top_level(bool async)
    {
        await base.Where_client_or_server_top_level(async);

        AssertSql();
    }

    public override async Task Where_client_and_server_non_top_level(bool async)
    {
        await base.Where_client_and_server_non_top_level(async);

        AssertSql();
    }

    public override async Task Where_client_deep_inside_predicate_and_server_top_level(bool async)
    {
        await base.Where_client_deep_inside_predicate_and_server_top_level(async);

        AssertSql();
    }

    public override async Task EF_Constant(bool async)
    {
        await base.EF_Constant(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
""");
    }

    public override async Task EF_Constant_with_subtree(bool async)
    {
        await base.EF_Constant_with_subtree(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
""");
    }

    public override async Task EF_Constant_does_not_parameterized_as_part_of_bigger_subtree(bool async)
    {
        await base.EF_Constant_does_not_parameterized_as_part_of_bigger_subtree(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALF' + N'KI'
""");
    }

    public override async Task EF_Constant_with_non_evaluatable_argument_throws(bool async)
    {
        await base.EF_Constant_with_non_evaluatable_argument_throws(async);

        AssertSql();
    }

    public override async Task EF_Parameter(bool async)
    {
        await base.EF_Parameter(async);

        AssertSql(
            """
@__p_0='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @__p_0
""");
    }

    public override async Task EF_Parameter_with_subtree(bool async)
    {
        await base.EF_Parameter_with_subtree(async);

        AssertSql(
            """
@__p_0='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @__p_0
""");
    }

    public override async Task EF_Parameter_does_not_parameterized_as_part_of_bigger_subtree(bool async)
    {
        await base.EF_Parameter_does_not_parameterized_as_part_of_bigger_subtree(async);

        AssertSql(
            """
@__id_0='ALF' (Size = 5)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @__id_0 + N'KI'
""");
    }

    public override async Task EF_Parameter_with_non_evaluatable_argument_throws(bool async)
    {
        await base.EF_Parameter_with_non_evaluatable_argument_throws(async);

        AssertSql();
    }

    public override async Task Implicit_cast_in_predicate(bool async)
    {
        await base.Implicit_cast_in_predicate(async);

        AssertSql(
"""
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'1337'
""",
                //
                """
@__prm_Value_0='1337' (Size = 5)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = @__prm_Value_0
""",
                //
                """
@__ToString_0='1337' (Size = 5)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = @__ToString_0
""",
                //
                """
@__p_0='1337' (Size = 5)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = @__p_0
""",
                //
                """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'1337'
""");
    }

    public override async Task Interface_casting_though_generic_method(bool async)
    {
        await base.Interface_casting_though_generic_method(async);

        AssertSql(
"""
@__id_0='10252'

SELECT [o].[OrderID] AS [Id]
FROM [Orders] AS [o]
WHERE [o].[OrderID] = @__id_0
""",
                //
                """
SELECT [o].[OrderID] AS [Id]
FROM [Orders] AS [o]
WHERE [o].[OrderID] = 10252
""",
                //
                """
SELECT [o].[OrderID] AS [Id]
FROM [Orders] AS [o]
WHERE [o].[OrderID] = 10252
""",
                //
                """
SELECT [o].[OrderID] AS [Id]
FROM [Orders] AS [o]
WHERE [o].[OrderID] = 10252
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}
