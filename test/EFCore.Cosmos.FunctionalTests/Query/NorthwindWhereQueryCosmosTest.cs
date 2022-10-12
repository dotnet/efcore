// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindWhereQueryCosmosTest : NorthwindWhereQueryTestBase<NorthwindQueryCosmosFixture<NoopModelCustomizer>>
{
    public NorthwindWhereQueryCosmosTest(
        NorthwindQueryCosmosFixture<NoopModelCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        ClearLog();
        //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_add(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID + 10 == 10258),
            entryCount: 1);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND ((c["OrderID"] + 10) = 10258))
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_subtract(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID - 10 == 10238),
            entryCount: 1);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND ((c["OrderID"] - 10) = 10238))
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_multiply(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID * 1 == 10248),
            entryCount: 1);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND ((c["OrderID"] * 1) = 10248))
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_divide(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID / 1 == 10248),
            entryCount: 1);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND ((c["OrderID"] / 1) = 10248))
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_modulo(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID % 10248 == 0),
            entryCount: 1);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND ((c["OrderID"] % 10248) = 0))
""");
    }

    public override async Task Where_bitwise_or(bool async)
    {
        // Bitwise operators on booleans. Issue #13168.
        Assert.Equal(
            "0",
            (await Assert.ThrowsAsync<EqualException>(() => base.Where_bitwise_or(async))).Actual);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND ((c["CustomerID"] = "ALFKI") | (c["CustomerID"] = "ANATR")))
""");
    }

    public override async Task Where_bitwise_and(bool async)
    {
        await base.Where_bitwise_and(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND ((c["CustomerID"] = "ALFKI") & (c["CustomerID"] = "ANATR")))
""");
    }

    public override async Task Where_bitwise_xor(bool async)
    {
        // Bitwise operators on booleans. Issue #13168.
        Assert.Equal(
            CosmosStrings.UnsupportedOperatorForSqlExpression("ExclusiveOr", "SqlBinaryExpression"),
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Where_bitwise_xor(async))).Message);

        AssertSql();
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_bitwise_leftshift(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => (o.OrderID << 1) == 20496),
            entryCount: 1);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND ((c["OrderID"] << 1) = 20496))
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_bitwise_rightshift(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => (o.OrderID >> 1) == 5124),
            entryCount: 2);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND ((c["OrderID"] >> 1) = 5124))
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_logical_and(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City == "Seattle" && c.ContactTitle == "Owner"),
            entryCount: 1);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND ((c["City"] = "Seattle") AND (c["ContactTitle"] = "Owner")))
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_logical_or(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI" || c.CustomerID == "ANATR"),
            entryCount: 2);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND ((c["CustomerID"] = "ALFKI") OR (c["CustomerID"] = "ANATR")))
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_logical_not(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => !(c.City != "Seattle")),
            entryCount: 1);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND NOT((c["City"] != "Seattle")))
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_equality(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => e.ReportsTo == 2),
            entryCount: 5);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Employee") AND (c["ReportsTo"] = 2))
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_inequality(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => e.ReportsTo != 2),
            entryCount: 4);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Employee") AND (c["ReportsTo"] != 2))
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_greaterthan(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => e.ReportsTo > 2),
            entryCount: 3);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Employee") AND (c["ReportsTo"] > 2))
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_greaterthanorequal(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => e.ReportsTo >= 2),
            entryCount: 8);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Employee") AND (c["ReportsTo"] >= 2))
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_lessthan(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => e.ReportsTo < 2));

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Employee") AND (c["ReportsTo"] < 2))
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_lessthanorequal(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => e.ReportsTo <= 2),
            entryCount: 5);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Employee") AND (c["ReportsTo"] <= 2))
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_string_concat(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID + "END" == "ALFKIEND"),
            entryCount: 1);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND ((c["CustomerID"] || "END") = "ALFKIEND"))
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_unary_minus(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => -o.OrderID == -10248),
            entryCount: 1);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (-(c["OrderID"]) = -10248))
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_bitwise_not(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => ~o.OrderID == -10249),
            entryCount: 1);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (~(c["OrderID"]) = -10249))
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_ternary(bool async)
    {
        await AssertQuery(
            async,
#pragma warning disable IDE0029 // Use coalesce expression
            ss => ss.Set<Customer>().Where(c => (c.Region != null ? c.Region : "SP") == "BC"),
#pragma warning restore IDE0029 // Use coalesce expression
            entryCount: 2);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (((c["Region"] != null) ? c["Region"] : "SP") = "BC"))
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_coalesce(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => (c.Region ?? "SP") == "BC"),
            entryCount: 2);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (((c["Region"] != null) ? c["Region"] : "SP") = "BC"))
""");
    }

    public override async Task Where_simple(bool async)
    {
        await base.Where_simple(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = "London"))
""");
    }

    private static readonly Expression<Func<Order, bool>> _filter = o => o.CustomerID == "ALFKI";

    public override async Task Where_as_queryable_expression(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_as_queryable_expression(async));

        AssertSql();
    }

    public override async Task<string> Where_simple_closure(bool async)
    {
        var queryString = await base.Where_simple_closure(async);

        AssertSql(
"""
@__city_0='London'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = @__city_0))
""");

        Assert.Equal(
"""
-- @__city_0='London'
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = @__city_0))
""", queryString, ignoreLineEndingDifferences: true,
            ignoreWhiteSpaceDifferences: true);

        return null;
    }

    public override async Task Where_indexer_closure(bool async)
    {
        await base.Where_indexer_closure(async);

        AssertSql(
"""
@__p_0='London'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = @__p_0))
""");
    }

    public override async Task Where_dictionary_key_access_closure(bool async)
    {
        await base.Where_dictionary_key_access_closure(async);

        AssertSql(
"""
@__get_Item_0='London'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = @__get_Item_0))
""");
    }

    public override async Task Where_tuple_item_closure(bool async)
    {
        await base.Where_tuple_item_closure(async);

        AssertSql(
"""
@__predicateTuple_Item2_0='London'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = @__predicateTuple_Item2_0))
""");
    }

    public override async Task Where_named_tuple_item_closure(bool async)
    {
        await base.Where_named_tuple_item_closure(async);

        AssertSql(
"""
@__predicateTuple_Item2_0='London'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = @__predicateTuple_Item2_0))
""");
    }

    public override async Task Where_simple_closure_constant(bool async)
    {
        await base.Where_simple_closure_constant(async);

        AssertSql(
"""
@__predicate_0='true'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND @__predicate_0)
""");
    }

    public override async Task Where_simple_closure_via_query_cache(bool async)
    {
        await base.Where_simple_closure_via_query_cache(async);

        AssertSql(
"""
@__city_0='London'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = @__city_0))
""",
                //
"""
@__city_0='Seattle'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = @__city_0))
""");
    }

    public override async Task Where_method_call_nullable_type_closure_via_query_cache(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_method_call_nullable_type_closure_via_query_cache(async));

        AssertSql();
    }

    public override async Task Where_method_call_nullable_type_reverse_closure_via_query_cache(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_method_call_nullable_type_reverse_closure_via_query_cache(async));

        AssertSql();
    }

    public override async Task Where_method_call_closure_via_query_cache(bool async)
    {
        await base.Where_method_call_closure_via_query_cache(async);

        AssertSql(
"""
@__GetCity_0='London'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = @__GetCity_0))
""",
                //
"""
@__GetCity_0='Seattle'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = @__GetCity_0))
""");
    }

    public override async Task Where_field_access_closure_via_query_cache(bool async)
    {
        await base.Where_field_access_closure_via_query_cache(async);

        AssertSql(
"""
@__city_InstanceFieldValue_0='London'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = @__city_InstanceFieldValue_0))
""",
                //
"""
@__city_InstanceFieldValue_0='Seattle'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = @__city_InstanceFieldValue_0))
""");
    }

    public override async Task Where_property_access_closure_via_query_cache(bool async)
    {
        await base.Where_property_access_closure_via_query_cache(async);

        AssertSql(
"""
@__city_InstancePropertyValue_0='London'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = @__city_InstancePropertyValue_0))
""",
                //
"""
@__city_InstancePropertyValue_0='Seattle'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = @__city_InstancePropertyValue_0))
""");
    }

    public override async Task Where_static_field_access_closure_via_query_cache(bool async)
    {
        await base.Where_static_field_access_closure_via_query_cache(async);

        AssertSql(
"""
@__StaticFieldValue_0='London'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = @__StaticFieldValue_0))
""",
                //
"""
@__StaticFieldValue_0='Seattle'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = @__StaticFieldValue_0))
""");
    }

    public override async Task Where_static_property_access_closure_via_query_cache(bool async)
    {
        await base.Where_static_property_access_closure_via_query_cache(async);

        AssertSql(
"""
@__StaticPropertyValue_0='London'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = @__StaticPropertyValue_0))
""",
                //
"""
@__StaticPropertyValue_0='Seattle'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = @__StaticPropertyValue_0))
""");
    }

    public override async Task Where_nested_field_access_closure_via_query_cache(bool async)
    {
        await base.Where_nested_field_access_closure_via_query_cache(async);

        AssertSql(
"""
@__city_Nested_InstanceFieldValue_0='London'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = @__city_Nested_InstanceFieldValue_0))
""",
                //
"""
@__city_Nested_InstanceFieldValue_0='Seattle'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = @__city_Nested_InstanceFieldValue_0))
""");
    }

    public override async Task Where_nested_property_access_closure_via_query_cache(bool async)
    {
        await base.Where_nested_property_access_closure_via_query_cache(async);

        AssertSql(
"""
@__city_Nested_InstancePropertyValue_0='London'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = @__city_Nested_InstancePropertyValue_0))
""",
                //
"""
@__city_Nested_InstancePropertyValue_0='Seattle'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = @__city_Nested_InstancePropertyValue_0))
""");
    }

    public override async Task Where_new_instance_field_access_query_cache(bool async)
    {
        await base.Where_new_instance_field_access_query_cache(async);

        AssertSql(
"""
@__InstanceFieldValue_0='London'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = @__InstanceFieldValue_0))
""",
                //
"""
@__InstanceFieldValue_0='Seattle'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = @__InstanceFieldValue_0))
""");
    }

    public override async Task Where_new_instance_field_access_closure_via_query_cache(bool async)
    {
        await base.Where_new_instance_field_access_closure_via_query_cache(async);

        AssertSql(
"""
@__InstanceFieldValue_0='London'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = @__InstanceFieldValue_0))
""",
                //
"""
@__InstanceFieldValue_0='Seattle'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = @__InstanceFieldValue_0))
""");
    }

    public override async Task Where_simple_closure_via_query_cache_nullable_type(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_simple_closure_via_query_cache_nullable_type(async));

        AssertSql();
    }

    public override async Task Where_simple_closure_via_query_cache_nullable_type_reverse(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_simple_closure_via_query_cache_nullable_type_reverse(async));

        AssertSql();
    }

    public override async Task Where_subquery_closure_via_query_cache(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_subquery_closure_via_query_cache(async));

        AssertSql();
    }

    public override async Task Where_simple_shadow(bool async)
    {
        await base.Where_simple_shadow(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Employee") AND (c["Title"] = "Sales Representative"))
""");
    }

    public override async Task Where_simple_shadow_projection(bool async)
    {
        await base.Where_simple_shadow_projection(async);

        AssertSql(
"""
SELECT c["Title"]
FROM root c
WHERE ((c["Discriminator"] = "Employee") AND (c["Title"] = "Sales Representative"))
""");
    }

    public override async Task Where_simple_shadow_subquery(bool async)
    {
        Assert.Equal(
            "5",
            (await Assert.ThrowsAsync<EqualException>(() => base.Where_simple_shadow_subquery(async))).Actual);

        AssertSql(
"""
@__p_0='5'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Employee") AND (c["Title"] = "Sales Representative"))
ORDER BY c["EmployeeID"]
OFFSET 0 LIMIT @__p_0
""");
    }

    public override async Task Where_shadow_subquery_FirstOrDefault(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_shadow_subquery_FirstOrDefault(async));

        AssertSql();
    }

    public override async Task Where_client(bool async)
    {
        await base.Where_client(async);

        AssertSql();
    }

    public override async Task Where_subquery_correlated(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_subquery_correlated(async));

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

    public override async Task Where_equals_method_string(bool async)
    {
        await base.Where_equals_method_string(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = "London"))
""");
    }

    public override async Task Where_equals_method_string_with_ignore_case(bool async)
    {
        await base.Where_equals_method_string_with_ignore_case(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND STRINGEQUALS(c["City"], "London", true))
""");
    }

    public override async Task Where_equals_method_int(bool async)
    {
        await base.Where_equals_method_int(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Employee") AND (c["EmployeeID"] = 1))
""");
    }

    public override async Task Where_equals_using_object_overload_on_mismatched_types(bool async)
    {
        await base.Where_equals_using_object_overload_on_mismatched_types(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Employee") AND false)
""");
    }

    public override async Task Where_equals_using_int_overload_on_mismatched_types(bool async)
    {
        await base.Where_equals_using_int_overload_on_mismatched_types(async);

        AssertSql(
"""
@__p_0='1'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Employee") AND (c["EmployeeID"] = @__p_0))
""");
    }

    public override async Task Where_equals_on_mismatched_types_nullable_int_long(bool async)
    {
        await base.Where_equals_on_mismatched_types_nullable_int_long(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Employee") AND false)
""",
                //
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Employee") AND false)
""");
    }

    public override async Task Where_equals_on_mismatched_types_nullable_long_nullable_int(bool async)
    {
        await base.Where_equals_on_mismatched_types_nullable_long_nullable_int(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Employee") AND false)
""",
                //
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Employee") AND false)
""");
    }

    public override async Task Where_equals_on_mismatched_types_int_nullable_int(bool async)
    {
        await base.Where_equals_on_mismatched_types_int_nullable_int(async);

        AssertSql(
"""
@__intPrm_0='2'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Employee") AND (c["ReportsTo"] = @__intPrm_0))
""",
                //
"""
@__intPrm_0='2'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Employee") AND (@__intPrm_0 = c["ReportsTo"]))
""");
    }

    public override async Task Where_equals_on_matched_nullable_int_types(bool async)
    {
        await base.Where_equals_on_matched_nullable_int_types(async);

        AssertSql(
"""
@__nullableIntPrm_0='2'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Employee") AND (@__nullableIntPrm_0 = c["ReportsTo"]))
""",
                //
"""
@__nullableIntPrm_0='2'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Employee") AND (c["ReportsTo"] = @__nullableIntPrm_0))
""");
    }

    public override async Task Where_equals_on_null_nullable_int_types(bool async)
    {
        await base.Where_equals_on_null_nullable_int_types(async);

        AssertSql(
"""
@__nullableIntPrm_0=null

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Employee") AND (@__nullableIntPrm_0 = c["ReportsTo"]))
""",
                //
"""
@__nullableIntPrm_0=null

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Employee") AND (c["ReportsTo"] = @__nullableIntPrm_0))
""");
    }

    public override async Task Where_comparison_nullable_type_not_null(bool async)
    {
        await base.Where_comparison_nullable_type_not_null(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Employee") AND (c["ReportsTo"] = 2))
""");
    }

    public override async Task Where_comparison_nullable_type_null(bool async)
    {
        await base.Where_comparison_nullable_type_null(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Employee") AND (c["ReportsTo"] = null))
""");
    }

    public override async Task Where_string_length(bool async)
    {
        await base.Where_string_length(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (LENGTH(c["City"]) = 6))
""");
    }

    public override async Task Where_string_indexof(bool async)
    {
        await base.Where_string_indexof(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (INDEX_OF(c["City"], "Sea") != -1))
""");
    }

    public override async Task Where_string_replace(bool async)
    {
        await base.Where_string_replace(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (REPLACE(c["City"], "Sea", "Rea") = "Reattle"))
""");
    }

    public override async Task Where_string_substring(bool async)
    {
        await base.Where_string_substring(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (SUBSTRING(c["City"], 1, 2) = "ea"))
""");
    }

    public override async Task Where_datetime_now(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_datetime_now(async));

        AssertSql();
    }

    public override async Task Where_datetime_utcnow(bool async)
    {
        await base.Where_datetime_utcnow(async);

        AssertSql(
"""
@__myDatetime_0='2015-04-10T00:00:00'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (GetCurrentDateTime() != @__myDatetime_0))
""");
    }

    public override async Task Where_datetimeoffset_utcnow(bool async)
    {
        await base.Where_datetimeoffset_utcnow(async);

        AssertSql(
"""
@__myDatetimeOffset_0='2015-04-10T00:00:00-08:00'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (GetCurrentDateTime() != @__myDatetimeOffset_0))
""");
    }

    public override async Task Where_datetime_today(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_datetime_today(async));

        AssertSql();
    }

    public override async Task Where_datetime_date_component(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_datetime_date_component(async));

        AssertSql();
    }

    public override async Task Where_date_add_year_constant_component(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_date_add_year_constant_component(async));

        AssertSql();
    }

    public override async Task Where_datetime_year_component(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_datetime_year_component(async));

        AssertSql();
    }

    public override async Task Where_datetime_month_component(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_datetime_month_component(async));

        AssertSql();
    }

    public override async Task Where_datetime_dayOfYear_component(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_datetime_dayOfYear_component(async));

        AssertSql();
    }

    public override async Task Where_datetime_day_component(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_datetime_day_component(async));

        AssertSql();
    }

    public override async Task Where_datetime_hour_component(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_datetime_hour_component(async));

        AssertSql();
    }

    public override async Task Where_datetime_minute_component(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_datetime_minute_component(async));

        AssertSql();
    }

    public override async Task Where_datetime_second_component(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_datetime_second_component(async));

        AssertSql();
    }

    public override async Task Where_datetime_millisecond_component(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_datetime_millisecond_component(async));

        AssertSql();
    }

    public override async Task Where_datetimeoffset_now_component(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_datetimeoffset_now_component(async));

        AssertSql();
    }

    public override async Task Where_datetimeoffset_utcnow_component(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_datetimeoffset_utcnow_component(async));

        AssertSql();
    }

    public override async Task Where_simple_reversed(bool async)
    {
        await base.Where_simple_reversed(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND ("London" = c["City"]))
""");
    }

    public override async Task Where_is_null(bool async)
    {
        await base.Where_is_null(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = null))
""");
    }

    public override async Task Where_null_is_null(bool async)
    {
        await base.Where_null_is_null(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
    }

    public override async Task Where_constant_is_null(bool async)
    {
        await base.Where_constant_is_null(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND false)
""");
    }

    public override async Task Where_is_not_null(bool async)
    {
        await base.Where_is_not_null(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] != null))
""");
    }

    public override async Task Where_null_is_not_null(bool async)
    {
        await base.Where_null_is_not_null(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND false)
""");
    }

    public override async Task Where_constant_is_not_null(bool async)
    {
        await base.Where_constant_is_not_null(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
    }

    public override async Task Where_identity_comparison(bool async)
    {
        await base.Where_identity_comparison(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = c["City"]))
""");
    }

    public override async Task Where_in_optimization_multiple(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_in_optimization_multiple(async));

        AssertSql();
    }

    public override async Task Where_not_in_optimization1(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_not_in_optimization1(async));

        AssertSql();
    }

    public override async Task Where_not_in_optimization2(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_not_in_optimization2(async));

        AssertSql();
    }

    public override async Task Where_not_in_optimization3(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_not_in_optimization3(async));

        AssertSql();
    }

    public override async Task Where_not_in_optimization4(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_not_in_optimization4(async));

        AssertSql();
    }

    public override async Task Where_select_many_and(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_select_many_and(async));

        AssertSql();
    }

    public override async Task Where_primitive(bool async)
    {
        await base.Where_primitive(async);

        AssertSql(
"""
@__p_0='9'

SELECT c["EmployeeID"]
FROM root c
WHERE ((c["Discriminator"] = "Employee") AND (c["EmployeeID"] = 5))
OFFSET 0 LIMIT @__p_0
""");
    }

    public override async Task Where_bool_member(bool async)
    {
        await base.Where_bool_member(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Product") AND c["Discontinued"])
""");
    }

    public override async Task Where_bool_member_false(bool async)
    {
        await base.Where_bool_member_false(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Product") AND NOT(c["Discontinued"]))
""");
    }

    public override async Task Where_bool_client_side_negated(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_bool_client_side_negated(async));

        AssertSql();
    }

    public override async Task Where_bool_member_negated_twice(bool async)
    {
        await base.Where_bool_member_negated_twice(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Product") AND NOT(NOT((c["Discontinued"] = true))))
""");
    }

    public override async Task Where_bool_member_shadow(bool async)
    {
        await base.Where_bool_member_shadow(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Product") AND c["Discontinued"])
""");
    }

    public override async Task Where_bool_member_false_shadow(bool async)
    {
        await base.Where_bool_member_false_shadow(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Product") AND NOT(c["Discontinued"]))
""");
    }

    public override async Task Where_bool_member_equals_constant(bool async)
    {
        await base.Where_bool_member_equals_constant(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Product") AND (c["Discontinued"] = true))
""");
    }

    public override async Task Where_bool_member_in_complex_predicate(bool async)
    {
        await base.Where_bool_member_in_complex_predicate(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Product") AND (((c["ProductID"] > 100) AND c["Discontinued"]) OR (c["Discontinued"] = true)))
""");
    }

    public override async Task Where_bool_member_compared_to_binary_expression(bool async)
    {
        await base.Where_bool_member_compared_to_binary_expression(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Product") AND (c["Discontinued"] = (c["ProductID"] > 50)))
""");
    }

    public override async Task Where_not_bool_member_compared_to_not_bool_member(bool async)
    {
        await base.Where_not_bool_member_compared_to_not_bool_member(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Product") AND (NOT(c["Discontinued"]) = NOT(c["Discontinued"])))
""");
    }

    public override async Task Where_negated_boolean_expression_compared_to_another_negated_boolean_expression(bool async)
    {
        await base.Where_negated_boolean_expression_compared_to_another_negated_boolean_expression(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Product") AND (NOT((c["ProductID"] > 50)) = NOT((c["ProductID"] > 20))))
""");
    }

    public override async Task Where_not_bool_member_compared_to_binary_expression(bool async)
    {
        await base.Where_not_bool_member_compared_to_binary_expression(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Product") AND (NOT(c["Discontinued"]) = (c["ProductID"] > 50)))
""");
    }

    public override async Task Where_bool_parameter(bool async)
    {
        await base.Where_bool_parameter(async);

        AssertSql(
"""
@__prm_0='true'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Product") AND @__prm_0)
""");
    }

    public override async Task Where_bool_parameter_compared_to_binary_expression(bool async)
    {
        await base.Where_bool_parameter_compared_to_binary_expression(async);

        AssertSql(
"""
@__prm_0='true'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Product") AND ((c["ProductID"] > 50) != @__prm_0))
""");
    }

    public override async Task Where_bool_member_and_parameter_compared_to_binary_expression_nested(bool async)
    {
        await base.Where_bool_member_and_parameter_compared_to_binary_expression_nested(async);

        AssertSql(
"""
@__prm_0='true'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Product") AND (c["Discontinued"] = ((c["ProductID"] > 50) != @__prm_0)))
""");
    }

    public override async Task Where_de_morgan_or_optimized(bool async)
    {
        await base.Where_de_morgan_or_optimized(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Product") AND NOT((c["Discontinued"] OR (c["ProductID"] < 20))))
""");
    }

    public override async Task Where_de_morgan_and_optimized(bool async)
    {
        await base.Where_de_morgan_and_optimized(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Product") AND NOT((c["Discontinued"] AND (c["ProductID"] < 20))))
""");
    }

    public override async Task Where_complex_negated_expression_optimized(bool async)
    {
        await base.Where_complex_negated_expression_optimized(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Product") AND NOT((NOT((NOT(c["Discontinued"]) AND (c["ProductID"] < 60))) OR NOT((c["ProductID"] > 30)))))
""");
    }

    public override async Task Where_short_member_comparison(bool async)
    {
        await base.Where_short_member_comparison(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Product") AND (c["UnitsInStock"] > 10))
""");
    }

    public override async Task Where_comparison_to_nullable_bool(bool async)
    {
        await base.Where_comparison_to_nullable_bool(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (((c["CustomerID"] != null) AND (("KI" != null) AND ENDSWITH(c["CustomerID"], "KI"))) = true))
""");
    }

    public override async Task Where_true(bool async)
    {
        await base.Where_true(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
    }

    public override async Task Where_false(bool async)
    {
        await base.Where_false(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND false)
""");
    }

    public override async Task Where_bool_closure(bool async)
    {
        await base.Where_bool_closure(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND false)
""",
                //
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND ((c["CustomerID"] = "ALFKI") AND true))
""");
    }

    public override async Task Where_default(bool async)
    {
        await base.Where_default(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["Fax"] = null))
""");
    }

    public override async Task Where_expression_invoke_1(bool async)
    {
        await base.Where_expression_invoke_1(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = "ALFKI"))
""");
    }

    public override async Task Where_expression_invoke_2(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_expression_invoke_2(async));

        AssertSql();
    }

    public override async Task Where_expression_invoke_3(bool async)
    {
        await base.Where_expression_invoke_3(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = "ALFKI"))
""");
    }

    public override async Task Where_concat_string_int_comparison1(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_concat_string_int_comparison1(async));

        AssertSql();
    }

    public override async Task Where_concat_string_int_comparison2(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_concat_string_int_comparison2(async));

        AssertSql();
    }

    public override async Task Where_concat_string_int_comparison3(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_concat_string_int_comparison3(async));

        AssertSql();
    }

    public override async Task Where_concat_string_int_comparison4(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_concat_string_int_comparison4(async));

        AssertSql();
    }

    public override async Task Where_string_concat_method_comparison(bool async)
    {
        await base.Where_string_concat_method_comparison(async);

        AssertSql(
"""
@__i_0='A'

SELECT c["CustomerID"]
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND ((@__i_0 || c["CustomerID"]) = c["CompanyName"]))
""");
    }

    public override async Task Where_string_concat_method_comparison_2(bool async)
    {
        await base.Where_string_concat_method_comparison_2(async);

        AssertSql(
"""
@__i_0='A'
@__j_1='B'

SELECT c["CustomerID"]
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND ((@__i_0 || (@__j_1 || c["CustomerID"])) = c["CompanyName"]))
""");
    }

    public override async Task Where_string_concat_method_comparison_3(bool async)
    {
        await base.Where_string_concat_method_comparison_3(async);

        AssertSql(
"""
@__i_0='A'
@__j_1='B'
@__k_2='C'

SELECT c["CustomerID"]
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND ((@__i_0 || (@__j_1 || (@__k_2 || c["CustomerID"]))) = c["CompanyName"]))
""");
    }

    public override async Task Where_ternary_boolean_condition_true(bool async)
    {
        await base.Where_ternary_boolean_condition_true(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Product") AND (c["UnitsInStock"] >= 20))
""");
    }

    public override async Task Where_ternary_boolean_condition_false(bool async)
    {
        await base.Where_ternary_boolean_condition_false(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Product") AND (c["UnitsInStock"] < 20))
""");
    }

    public override async Task Where_ternary_boolean_condition_with_another_condition(bool async)
    {
        await base.Where_ternary_boolean_condition_with_another_condition(async);

        AssertSql(
"""
@__productId_0='15'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Product") AND ((c["ProductID"] < @__productId_0) AND (c["UnitsInStock"] >= 20)))
""");
    }

    public override async Task Where_ternary_boolean_condition_with_false_as_result_true(bool async)
    {
        await base.Where_ternary_boolean_condition_with_false_as_result_true(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Product") AND (c["UnitsInStock"] >= 20))
""");
    }

    public override async Task Where_ternary_boolean_condition_with_false_as_result_false(bool async)
    {
        await base.Where_ternary_boolean_condition_with_false_as_result_false(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Product") AND false)
""");
    }

    public override async Task Where_compare_constructed_equal(bool async)
    {
        // Anonymous type to constant comparison. Issue #14672.
        await AssertTranslationFailed(() => base.Where_compare_constructed_equal(async));

        AssertSql();
    }

    public override async Task Where_compare_constructed_multi_value_equal(bool async)
    {
        // Anonymous type to constant comparison. Issue #14672.
        await AssertTranslationFailed(() => base.Where_compare_constructed_multi_value_equal(async));

        AssertSql();
    }

    public override async Task Where_compare_constructed_multi_value_not_equal(bool async)
    {
        // Anonymous type to constant comparison. Issue #14672.
        await AssertTranslationFailed(() => base.Where_compare_constructed_multi_value_not_equal(async));

        AssertSql();
    }

    public override async Task Where_compare_tuple_constructed_equal(bool async)
    {
        // Anonymous type to constant comparison. Issue #14672.
        await AssertTranslationFailed(() => base.Where_compare_tuple_constructed_equal(async));

        AssertSql();
    }

    public override async Task Where_compare_tuple_constructed_multi_value_equal(bool async)
    {
        // Anonymous type to constant comparison. Issue #14672.
        await AssertTranslationFailed(() => base.Where_compare_tuple_constructed_multi_value_equal(async));

        AssertSql();
    }

    public override async Task Where_compare_tuple_constructed_multi_value_not_equal(bool async)
    {
        // Anonymous type to constant comparison. Issue #14672.
        await AssertTranslationFailed(() => base.Where_compare_tuple_constructed_multi_value_not_equal(async));

        AssertSql();
    }

    public override async Task Where_compare_tuple_create_constructed_equal(bool async)
    {
        // Anonymous type to constant comparison. Issue #14672.
        await AssertTranslationFailed(() => base.Where_compare_tuple_create_constructed_equal(async));

        AssertSql();
    }

    public override async Task Where_compare_tuple_create_constructed_multi_value_equal(bool async)
    {
        // Anonymous type to constant comparison. Issue #14672.
        await AssertTranslationFailed(() => base.Where_compare_tuple_create_constructed_multi_value_equal(async));

        AssertSql();
    }

    public override async Task Where_compare_tuple_create_constructed_multi_value_not_equal(bool async)
    {
        // Anonymous type to constant comparison. Issue #14672.
        await AssertTranslationFailed(() => base.Where_compare_tuple_create_constructed_multi_value_not_equal(async));

        AssertSql();
    }

    public override async Task Where_compare_null(bool async)
    {
        await base.Where_compare_null(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND ((c["City"] = null) AND (c["Country"] = "UK")))
""");
    }

    public override async Task Where_Is_on_same_type(bool async)
    {
        await base.Where_Is_on_same_type(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
    }

    public override async Task Where_chain(bool async)
    {
        await base.Where_chain(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE (((c["Discriminator"] = "Order") AND (c["CustomerID"] = "QUICK")) AND (c["OrderDate"] > "1998-01-01T00:00:00"))
""");
    }

    public override async Task Where_navigation_contains(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Where_navigation_contains(async))).Message;

        Assert.Equal(
            CosmosStrings.NonEmbeddedIncludeNotSupported(
                @"Navigation: Customer.Orders (List<Order>) Collection ToDependent Order Inverse: Customer PropertyAccessMode.Field"),
            message);

        AssertSql();
    }

    public override async Task Where_array_index(bool async)
    {
        await base.Where_array_index(async);

        AssertSql(
"""
@__p_0='ALFKI'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = @__p_0))
""");
    }

    public override async Task Where_multiple_contains_in_subquery_with_or(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_multiple_contains_in_subquery_with_or(async));

        AssertSql();
    }

    public override async Task Where_multiple_contains_in_subquery_with_and(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_multiple_contains_in_subquery_with_and(async));

        AssertSql();
    }

    public override async Task Where_contains_on_navigation(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_contains_on_navigation(async));

        AssertSql();
    }

    public override async Task Where_subquery_FirstOrDefault_is_null(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_subquery_FirstOrDefault_is_null(async));

        AssertSql();
    }

    public override async Task Where_subquery_FirstOrDefault_compared_to_entity(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_subquery_FirstOrDefault_compared_to_entity(async));

        AssertSql();
    }

    public override async Task Time_of_day_datetime(bool async)
    {
        await base.Time_of_day_datetime(async);

        AssertSql(
"""
SELECT c["OrderDate"]
FROM root c
WHERE (c["Discriminator"] = "Order")
""");
    }

    public override async Task TypeBinary_short_circuit(bool async)
    {
        await base.TypeBinary_short_circuit(async);

        AssertSql(
"""
@__p_0='false'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND @__p_0)
""");
    }

    public override async Task Decimal_cast_to_double_works(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Decimal_cast_to_double_works(async));

        AssertSql();
    }

    public override async Task Where_is_conditional(bool async)
    {
        await base.Where_is_conditional(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Product") AND (true ? false : true))
""");
    }

    public override async Task Filter_non_nullable_value_after_FirstOrDefault_on_empty_collection(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Filter_non_nullable_value_after_FirstOrDefault_on_empty_collection(async));

        AssertSql();
    }

    public override async Task Like_with_non_string_column_using_ToString(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Like_with_non_string_column_using_ToString(async));

        AssertSql();
    }

    public override async Task Like_with_non_string_column_using_double_cast(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Like_with_non_string_column_using_double_cast(async));

        AssertSql();
    }

    public override async Task Using_same_parameter_twice_in_query_generates_one_sql_parameter(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Using_same_parameter_twice_in_query_generates_one_sql_parameter(async));

        AssertSql();
    }

    public override async Task Where_Queryable_ToList_Count(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_Queryable_ToList_Count(async));

        AssertSql();
    }

    public override async Task Where_Queryable_ToList_Contains(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_Queryable_ToList_Contains(async));

        AssertSql();
    }

    public override async Task Where_Queryable_ToArray_Count(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_Queryable_ToArray_Count(async));

        AssertSql();
    }

    public override async Task Where_Queryable_ToArray_Contains(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_Queryable_ToArray_Contains(async));

        AssertSql();
    }

    public override async Task Where_Queryable_AsEnumerable_Count(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_Queryable_AsEnumerable_Count(async));

        AssertSql();
    }

    public override async Task Where_Queryable_AsEnumerable_Contains(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_Queryable_AsEnumerable_Contains(async));

        AssertSql();
    }

    public override async Task Where_Queryable_ToList_Count_member(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_Queryable_ToList_Count_member(async));

        AssertSql();
    }

    public override async Task Where_Queryable_ToArray_Length_member(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_Queryable_ToArray_Length_member(async));

        AssertSql();
    }

    public override async Task Where_collection_navigation_ToList_Count(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_collection_navigation_ToList_Count(async));

        AssertSql();
    }

    public override async Task Where_collection_navigation_ToList_Contains(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_collection_navigation_ToList_Contains(async));

        AssertSql();
    }

    public override async Task Where_collection_navigation_ToArray_Count(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_collection_navigation_ToArray_Count(async));

        AssertSql();
    }

    public override async Task Where_collection_navigation_ToArray_Contains(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_collection_navigation_ToArray_Contains(async));

        AssertSql();
    }

    public override async Task Where_collection_navigation_AsEnumerable_Count(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_collection_navigation_AsEnumerable_Count(async));

        AssertSql();
    }

    public override async Task Where_collection_navigation_AsEnumerable_Contains(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_collection_navigation_AsEnumerable_Contains(async));

        AssertSql();
    }

    public override async Task Where_collection_navigation_ToList_Count_member(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_collection_navigation_ToList_Count_member(async));

        AssertSql();
    }

    public override async Task Where_collection_navigation_ToArray_Length_member(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_collection_navigation_ToArray_Length_member(async));

        AssertSql();
    }

    public override async Task Where_Queryable_AsEnumerable_Contains_negated(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_Queryable_AsEnumerable_Contains_negated(async));

        AssertSql();
    }

    public override async Task Where_list_object_contains_over_value_type(bool async)
    {
        await base.Where_list_object_contains_over_value_type(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND c["OrderID"] IN (10248, 10249))
""");
    }

    public override async Task Where_array_of_object_contains_over_value_type(bool async)
    {
        await base.Where_array_of_object_contains_over_value_type(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND c["OrderID"] IN (10248, 10249))
""");
    }

    public override async Task Filter_with_EF_Property_using_closure_for_property_name(bool async)
    {
        await base.Filter_with_EF_Property_using_closure_for_property_name(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = "ALFKI"))
""");
    }

    public override async Task Filter_with_EF_Property_using_function_for_property_name(bool async)
    {
        await base.Filter_with_EF_Property_using_function_for_property_name(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = "ALFKI"))
""");
    }

    public override async Task FirstOrDefault_over_scalar_projection_compared_to_null(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.FirstOrDefault_over_scalar_projection_compared_to_null(async));

        AssertSql();
    }

    public override async Task FirstOrDefault_over_scalar_projection_compared_to_not_null(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.FirstOrDefault_over_scalar_projection_compared_to_not_null(async));

        AssertSql();
    }

    public override async Task FirstOrDefault_over_custom_projection_compared_to_null(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.FirstOrDefault_over_custom_projection_compared_to_null(async));

        AssertSql();
    }

    public override async Task FirstOrDefault_over_custom_projection_compared_to_not_null(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.FirstOrDefault_over_custom_projection_compared_to_not_null(async));

        AssertSql();
    }

    public override async Task SingleOrDefault_over_custom_projection_compared_to_null(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.SingleOrDefault_over_custom_projection_compared_to_null(async));

        AssertSql();
    }

    public override async Task SingleOrDefault_over_custom_projection_compared_to_not_null(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.SingleOrDefault_over_custom_projection_compared_to_not_null(async));

        AssertSql();
    }

    public override async Task LastOrDefault_over_custom_projection_compared_to_null(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.LastOrDefault_over_custom_projection_compared_to_null(async));

        AssertSql();
    }

    public override async Task LastOrDefault_over_custom_projection_compared_to_not_null(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.LastOrDefault_over_custom_projection_compared_to_not_null(async));

        AssertSql();
    }

    public override async Task First_over_custom_projection_compared_to_null(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.First_over_custom_projection_compared_to_null(async));

        AssertSql();
    }

    public override async Task First_over_custom_projection_compared_to_not_null(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.First_over_custom_projection_compared_to_not_null(async));

        AssertSql();
    }

    public override async Task Single_over_custom_projection_compared_to_null(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Single_over_custom_projection_compared_to_null(async));

        AssertSql();
    }

    public override async Task Single_over_custom_projection_compared_to_not_null(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Single_over_custom_projection_compared_to_not_null(async));

        AssertSql();
    }

    public override async Task Last_over_custom_projection_compared_to_null(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Last_over_custom_projection_compared_to_null(async));

        AssertSql();
    }

    public override async Task Last_over_custom_projection_compared_to_not_null(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Last_over_custom_projection_compared_to_not_null(async));

        AssertSql();
    }

    public override async Task Where_Contains_and_comparison(bool async)
    {
        await base.Where_Contains_and_comparison(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] IN ("ALFKI", "FISSA") AND (c["City"] = "Seattle")))
""");
    }

    public override async Task Where_Contains_or_comparison(bool async)
    {
        await base.Where_Contains_or_comparison(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] IN ("ALFKI", "FISSA") OR (c["City"] = "Seattle")))
""");
    }

    public override async Task Where_Like_and_comparison(bool async)
    {
        await AssertTranslationFailed(() => base.Where_Like_and_comparison(async));

        AssertSql();
    }

    public override async Task Where_Like_or_comparison(bool async)
    {
        await AssertTranslationFailed(() => base.Where_Like_or_comparison(async));

        AssertSql();
    }

    public override async Task GetType_on_non_hierarchy1(bool async)
    {
        await base.GetType_on_non_hierarchy1(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
    }

    public override async Task GetType_on_non_hierarchy2(bool async)
    {
        await base.GetType_on_non_hierarchy2(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND false)
""");
    }

    public override async Task GetType_on_non_hierarchy3(bool async)
    {
        await base.GetType_on_non_hierarchy3(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND false)
""");
    }

    public override async Task GetType_on_non_hierarchy4(bool async)
    {
        await base.GetType_on_non_hierarchy4(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
    }

    public override async Task Case_block_simplification_works_correctly(bool async)
    {
        await base.Case_block_simplification_works_correctly(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (((c["Region"] = null) ? "OR" : c["Region"]) = "OR"))
""");
    }

    public override async Task Where_compare_null_with_cast_to_object(bool async)
    {
        await base.Where_compare_null_with_cast_to_object(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = null))
""");
    }

    public override async Task Where_compare_with_both_cast_to_object(bool async)
    {
        await base.Where_compare_with_both_cast_to_object(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = "London"))
""");
    }

    public override async Task Where_projection(bool async)
    {
        await base.Where_projection(async);

        AssertSql(
"""
SELECT c["CompanyName"]
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = "London"))
""");
    }

    public override async Task Enclosing_class_settable_member_generates_parameter(bool async)
    {
        await base.Enclosing_class_settable_member_generates_parameter(async);

        AssertSql(
"""
@__SettableProperty_0='4'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderID"] = @__SettableProperty_0))
""",
                //
"""
@__SettableProperty_0='10'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderID"] = @__SettableProperty_0))
""");
    }

    public override async Task Enclosing_class_readonly_member_generates_parameter(bool async)
    {
        await base.Enclosing_class_readonly_member_generates_parameter(async);

        AssertSql(
"""
@__ReadOnlyProperty_0='5'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderID"] = @__ReadOnlyProperty_0))
""");
    }

    public override async Task Enclosing_class_const_member_does_not_generate_parameter(bool async)
    {
        await base.Enclosing_class_const_member_does_not_generate_parameter(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderID"] = 1))
""");
    }

    public override async Task Generic_Ilist_contains_translates_to_server(bool async)
    {
        await base.Generic_Ilist_contains_translates_to_server(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["City"] IN ("Seattle"))
""");
    }

    public override async Task Multiple_OrElse_on_same_column_converted_to_in_with_overlap(bool async)
    {
        await base.Multiple_OrElse_on_same_column_converted_to_in_with_overlap(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND ((((c["CustomerID"] = "ALFKI") OR (c["CustomerID"] = "ANATR")) OR (c["CustomerID"] = "ANTON")) OR (c["CustomerID"] = "ANATR")))
""");
    }

    public override async Task Multiple_OrElse_on_same_column_with_null_constant_comparison_converted_to_in(bool async)
    {
        await base.Multiple_OrElse_on_same_column_with_null_constant_comparison_converted_to_in(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND ((((c["Region"] = "WA") OR (c["Region"] = "OR")) OR (c["Region"] = null)) OR (c["Region"] = "BC")))
""");
    }

    public override async Task Constant_array_Contains_OrElse_comparison_with_constant_gets_combined_to_one_in(bool async)
    {
        await base.Constant_array_Contains_OrElse_comparison_with_constant_gets_combined_to_one_in(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] IN ("ALFKI", "ANATR") OR (c["CustomerID"] = "ANTON")))
""");
    }

    public override async Task Constant_array_Contains_OrElse_comparison_with_constant_gets_combined_to_one_in_with_overlap(bool async)
    {
        await base.Constant_array_Contains_OrElse_comparison_with_constant_gets_combined_to_one_in_with_overlap(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (((c["CustomerID"] = "ANTON") OR c["CustomerID"] IN ("ALFKI", "ANATR")) OR (c["CustomerID"] = "ALFKI")))
""");
    }

    public override async Task Constant_array_Contains_OrElse_another_Contains_gets_combined_to_one_in_with_overlap(bool async)
    {
        await base.Constant_array_Contains_OrElse_another_Contains_gets_combined_to_one_in_with_overlap(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] IN ("ALFKI", "ANATR") OR c["CustomerID"] IN ("ALFKI", "ANTON")))
""");
    }

    public override async Task Constant_array_Contains_AndAlso_another_Contains_gets_combined_to_one_in_with_overlap(bool async)
    {
        await base.Constant_array_Contains_AndAlso_another_Contains_gets_combined_to_one_in_with_overlap(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (NOT(c["CustomerID"] IN ("ALFKI", "ANATR")) AND NOT(c["CustomerID"] IN ("ALFKI", "ANTON"))))
""");
    }

    public override async Task Multiple_AndAlso_on_same_column_converted_to_in_using_parameters(bool async)
    {
        await base.Multiple_AndAlso_on_same_column_converted_to_in_using_parameters(async);

        AssertSql(
"""
@__prm1_0='ALFKI'
@__prm2_1='ANATR'
@__prm3_2='ANTON'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (((c["CustomerID"] != @__prm1_0) AND (c["CustomerID"] != @__prm2_1)) AND (c["CustomerID"] != @__prm3_2)))
""");
    }

    public override async Task Array_of_parameters_Contains_OrElse_comparison_with_constant_gets_combined_to_one_in(bool async)
    {
        await base.Array_of_parameters_Contains_OrElse_comparison_with_constant_gets_combined_to_one_in(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] IN ("ALFKI", "ANATR") OR (c["CustomerID"] = "ANTON")))
""");
    }

    public override async Task Multiple_OrElse_on_same_column_with_null_parameter_comparison_converted_to_in(bool async)
    {
        await base.Multiple_OrElse_on_same_column_with_null_parameter_comparison_converted_to_in(async);

        AssertSql(
"""
@__prm_0=null

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND ((((c["Region"] = "WA") OR (c["Region"] = "OR")) OR (c["Region"] = @__prm_0)) OR (c["Region"] = "BC")))
""");
    }

    public override async Task Parameter_array_Contains_OrElse_comparison_with_constant(bool async)
    {
        await base.Parameter_array_Contains_OrElse_comparison_with_constant(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] IN ("ALFKI", "ANATR") OR (c["CustomerID"] = "ANTON")))
""");
    }

    public override async Task Parameter_array_Contains_OrElse_comparison_with_parameter_with_overlap(bool async)
    {
        await base.Parameter_array_Contains_OrElse_comparison_with_parameter_with_overlap(async);

        AssertSql(
"""
@__prm1_0='ANTON'
@__prm2_2='ALFKI'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (((c["CustomerID"] = @__prm1_0) OR c["CustomerID"] IN ("ALFKI", "ANATR")) OR (c["CustomerID"] = @__prm2_2)))
""");
    }

    public override async Task Two_sets_of_comparison_combine_correctly(bool async)
    {
        await base.Two_sets_of_comparison_combine_correctly(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] IN ("ALFKI", "ANATR") AND ((c["CustomerID"] = "ANATR") OR (c["CustomerID"] = "ANTON"))))
""");
    }

    public override async Task Two_sets_of_comparison_combine_correctly2(bool async)
    {
        await base.Two_sets_of_comparison_combine_correctly2(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND ((((c["Region"] != "WA") AND (c["Region"] != "OR")) AND (c["Region"] != null)) OR ((c["Region"] != "WA") AND (c["Region"] != null))))
""");
    }

    public override async Task Filter_with_property_compared_to_null_wrapped_in_explicit_convert_to_object(bool async)
    {
        await base.Filter_with_property_compared_to_null_wrapped_in_explicit_convert_to_object(async);

        AssertSql(
"""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["Region"] = null))
""");
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

    public override async Task Where_simple_shadow_projection_mixed(bool async)
    {
        await base.Where_simple_shadow_projection_mixed(async);

        AssertSql(
"""
SELECT VALUE {"e" : c, "Title" : c["Title"]}
FROM root c
WHERE ((c["Discriminator"] = "Employee") AND (c["Title"] = "Sales Representative"))
""");
    }

    public override async Task Where_primitive_tracked(bool async)
    {
        await base.Where_primitive_tracked(async);

        AssertSql(
"""
@__p_0='9'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Employee") AND (c["EmployeeID"] = 5))
OFFSET 0 LIMIT @__p_0
""");
    }

    public override async Task Where_primitive_tracked2(bool async)
    {
        await base.Where_primitive_tracked2(async);

        AssertSql(
"""
@__p_0='9'

SELECT VALUE {"e" : c}
FROM root c
WHERE ((c["Discriminator"] = "Employee") AND (c["EmployeeID"] = 5))
OFFSET 0 LIMIT @__p_0
""");
    }

    public override async Task Where_poco_closure(bool async)
    {
        await base.Where_poco_closure(async);

        AssertSql(
"""
@__entity_equality_customer_0_CustomerID='ALFKI'

SELECT c["CustomerID"]
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = @__entity_equality_customer_0_CustomerID))
""",
                //
"""
@__entity_equality_customer_0_CustomerID='ANATR'

SELECT c["CustomerID"]
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = @__entity_equality_customer_0_CustomerID))
""");
    }

    public override async Task Where_concat_string_string_comparison(bool async)
    {
        await base.Where_concat_string_string_comparison(async);

        AssertSql(
"""
@__i_0='A'

SELECT c["CustomerID"]
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND ((@__i_0 || c["CustomerID"]) = c["CompanyName"]))
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}
