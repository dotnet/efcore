// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class NorthwindWhereQueryCosmosTest : NorthwindWhereQueryTestBase<NorthwindQueryCosmosFixture<NoopModelCustomizer>>
{
    public NorthwindWhereQueryCosmosTest(
        NorthwindQueryCosmosFixture<NoopModelCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        ClearLog();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_add(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await AssertQuery(
                    a,
                    ss => ss.Set<Order>().Where(o => o.OrderID + 10 == 10258));

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Order") AND ((c["OrderID"] + 10) = 10258))
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_subtract(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await AssertQuery(
                    a,
                    ss => ss.Set<Order>().Where(o => o.OrderID - 10 == 10238));

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Order") AND ((c["OrderID"] - 10) = 10238))
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_multiply(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await AssertQuery(
                    a,
                    ss => ss.Set<Order>().Where(o => o.OrderID * 1 == 10248));

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Order") AND ((c["OrderID"] * 1) = 10248))
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_divide(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await AssertQuery(
                    a,
                    ss => ss.Set<Order>().Where(o => o.OrderID / 1 == 10248));

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Order") AND ((c["OrderID"] / 1) = 10248))
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_modulo(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await AssertQuery(
                    a,
                    ss => ss.Set<Order>().Where(o => o.OrderID % 10248 == 0));

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Order") AND ((c["OrderID"] % 10248) = 0))
""");
            });

    public override async Task Where_bitwise_or(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await Fixture.NoSyncTest(
                async, async a =>
                {
                    // Bitwise operators on booleans. Issue #13168.
                    await Assert.ThrowsAsync<EqualException>(() => base.Where_bitwise_or(async));

                    AssertSql(
                        """
SELECT VALUE c
FROM root c
WHERE ((c["id"] = "ALFKI") | (c["id"] = "ANATR"))
""");
                });
        }
    }

    public override Task Where_bitwise_and(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_bitwise_and(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["id"] = "ALFKI") & (c["id"] = "ANATR"))
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Where_bitwise_xor(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_bitwise_xor(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["id"] = "ALFKI") != true)
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_bitwise_leftshift(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await AssertQuery(
                    a,
                    ss => ss.Set<Order>().Where(o => (o.OrderID << 1) == 20496));

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Order") AND ((c["OrderID"] << 1) = 20496))
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_bitwise_rightshift(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await AssertQuery(
                    a,
                    ss => ss.Set<Order>().Where(o => (o.OrderID >> 1) == 5124));

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Order") AND ((c["OrderID"] >> 1) = 5124))
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_logical_and(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await AssertQuery(
                    a,
                    ss => ss.Set<Customer>().Where(c => c.City == "Seattle" && c.ContactTitle == "Owner"));

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["City"] = "Seattle") AND (c["ContactTitle"] = "Owner"))
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_logical_or(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await AssertQuery(
                    a,
                    ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI" || c.CustomerID == "ANATR"));

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["id"] = "ALFKI") OR (c["id"] = "ANATR"))
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_logical_not(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await AssertQuery(
                    a,
                    ss => ss.Set<Customer>().Where(c => !(c.City != "Seattle")));

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE NOT((c["City"] != "Seattle"))
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_equality(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await AssertQuery(
                    a,
                    ss => ss.Set<Employee>().Where(e => e.ReportsTo == 2));

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["ReportsTo"] = 2)
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_inequality(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await AssertQuery(
                    a,
                    ss => ss.Set<Employee>().Where(e => e.ReportsTo != 2));

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["ReportsTo"] != 2)
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_greaterthan(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await AssertQuery(
                    a,
                    ss => ss.Set<Employee>().Where(e => e.ReportsTo > 2));

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["ReportsTo"] > 2)
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_greaterthanorequal(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await AssertQuery(
                    a,
                    ss => ss.Set<Employee>().Where(e => e.ReportsTo >= 2));

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["ReportsTo"] >= 2)
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_lessthan(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await AssertQuery(
                    a,
                    ss => ss.Set<Employee>().Where(e => e.ReportsTo < 3));

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["ReportsTo"] < 3)
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_lessthanorequal(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await AssertQuery(
                    a,
                    ss => ss.Set<Employee>().Where(e => e.ReportsTo <= 2));

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["ReportsTo"] <= 2)
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_string_concat(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await AssertQuery(
                    a,
                    ss => ss.Set<Customer>().Where(c => c.CustomerID + "END" == "ALFKIEND"));

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["id"] || "END") = "ALFKIEND")
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_unary_minus(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await AssertQuery(
                    a,
                    ss => ss.Set<Order>().Where(o => -o.OrderID == -10248));

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Order") AND (-(c["OrderID"]) = -10248))
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_bitwise_not(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await AssertQuery(
                    a,
                    ss => ss.Set<Order>().Where(o => ~o.OrderID == -10249));

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Order") AND (~(c["OrderID"]) = -10249))
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_ternary(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await AssertQuery(
                    a,
#pragma warning disable IDE0029 // Use coalesce expression
                    ss => ss.Set<Customer>().Where(c => (c.Region != null ? c.Region : "SP") == "BC"));
#pragma warning restore IDE0029 // Use coalesce expression

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (((c["Region"] != null) ? c["Region"] : "SP") = "BC")
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_coalesce(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await AssertQuery(
                    a,
                    ss => ss.Set<Customer>().Where(c => (c.Region ?? "SP") == "BC"));

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (((c["Region"] != null) ? c["Region"] : "SP") = "BC")
""");
            });

    public override Task Where_simple(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_simple(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["City"] = "London")
""");
            });

    private static readonly Expression<Func<Order, bool>> _filter = o => o.CustomerID == "ALFKI";

    public override async Task Where_as_queryable_expression(bool async)
    {
        // Uncorrelated subquery, not supported by Cosmos
        await AssertTranslationFailed(() => base.Where_as_queryable_expression(async));

        AssertSql();
    }

    public override async Task<string> Where_simple_closure(bool async)
    {
        await Fixture.NoSyncTest(
            async, async a =>
            {
                var queryString = await base.Where_simple_closure(a);

                AssertSql(
                    """
@__city_0='London'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @__city_0)
""");
            });

        return null;
    }

    public override Task Where_indexer_closure(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_indexer_closure(a);

                AssertSql(
                    """
@__p_0='London'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @__p_0)
""");
            });

    public override Task Where_dictionary_key_access_closure(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_dictionary_key_access_closure(a);

                AssertSql(
                    """
@__get_Item_0='London'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @__get_Item_0)
""");
            });

    public override Task Where_tuple_item_closure(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_tuple_item_closure(a);

                AssertSql(
                    """
@__predicateTuple_Item2_0='London'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @__predicateTuple_Item2_0)
""");
            });

    public override Task Where_named_tuple_item_closure(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_named_tuple_item_closure(a);

                AssertSql(
                    """
@__predicateTuple_Item2_0='London'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @__predicateTuple_Item2_0)
""");
            });

    public override Task Where_simple_closure_constant(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_simple_closure_constant(a);

                AssertSql(
                    """
@__predicate_0='true'

SELECT VALUE c
FROM root c
WHERE @__predicate_0
""");
            });

    public override Task Where_simple_closure_via_query_cache(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_simple_closure_via_query_cache(a);

                AssertSql(
                    """
@__city_0='London'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @__city_0)
""",
                    //
                    """
@__city_0='Seattle'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @__city_0)
""");
            });

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

    public override Task Where_method_call_closure_via_query_cache(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_method_call_closure_via_query_cache(a);

                AssertSql(
                    """
@__GetCity_0='London'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @__GetCity_0)
""",
                    //
                    """
@__GetCity_0='Seattle'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @__GetCity_0)
""");
            });

    public override Task Where_field_access_closure_via_query_cache(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_field_access_closure_via_query_cache(a);

                AssertSql(
                    """
@__city_InstanceFieldValue_0='London'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @__city_InstanceFieldValue_0)
""",
                    //
                    """
@__city_InstanceFieldValue_0='Seattle'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @__city_InstanceFieldValue_0)
""");
            });

    public override Task Where_property_access_closure_via_query_cache(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_property_access_closure_via_query_cache(a);

                AssertSql(
                    """
@__city_InstancePropertyValue_0='London'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @__city_InstancePropertyValue_0)
""",
                    //
                    """
@__city_InstancePropertyValue_0='Seattle'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @__city_InstancePropertyValue_0)
""");
            });

    public override Task Where_static_field_access_closure_via_query_cache(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_static_field_access_closure_via_query_cache(a);

                AssertSql(
                    """
@__StaticFieldValue_0='London'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @__StaticFieldValue_0)
""",
                    //
                    """
@__StaticFieldValue_0='Seattle'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @__StaticFieldValue_0)
""");
            });

    public override Task Where_static_property_access_closure_via_query_cache(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_static_property_access_closure_via_query_cache(a);

                AssertSql(
                    """
@__StaticPropertyValue_0='London'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @__StaticPropertyValue_0)
""",
                    //
                    """
@__StaticPropertyValue_0='Seattle'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @__StaticPropertyValue_0)
""");
            });

    public override Task Where_nested_field_access_closure_via_query_cache(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_nested_field_access_closure_via_query_cache(a);

                AssertSql(
                    """
@__city_Nested_InstanceFieldValue_0='London'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @__city_Nested_InstanceFieldValue_0)
""",
                    //
                    """
@__city_Nested_InstanceFieldValue_0='Seattle'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @__city_Nested_InstanceFieldValue_0)
""");
            });

    public override Task Where_nested_property_access_closure_via_query_cache(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_nested_property_access_closure_via_query_cache(a);

                AssertSql(
                    """
@__city_Nested_InstancePropertyValue_0='London'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @__city_Nested_InstancePropertyValue_0)
""",
                    //
                    """
@__city_Nested_InstancePropertyValue_0='Seattle'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @__city_Nested_InstancePropertyValue_0)
""");
            });

    public override Task Where_new_instance_field_access_query_cache(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_new_instance_field_access_query_cache(a);

                AssertSql(
                    """
@__InstanceFieldValue_0='London'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @__InstanceFieldValue_0)
""",
                    //
                    """
@__InstanceFieldValue_0='Seattle'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @__InstanceFieldValue_0)
""");
            });

    public override Task Where_new_instance_field_access_closure_via_query_cache(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_new_instance_field_access_closure_via_query_cache(a);

                AssertSql(
                    """
@__InstanceFieldValue_0='London'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @__InstanceFieldValue_0)
""",
                    //
                    """
@__InstanceFieldValue_0='Seattle'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @__InstanceFieldValue_0)
""");
            });

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

    [ConditionalTheory(Skip = "Always uses sync code.")]
    public override Task Where_subquery_closure_via_query_cache(bool async)
        => Task.CompletedTask;

    public override Task Where_simple_shadow(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_simple_shadow(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Title"] = "Sales Representative")
""");
            });

    public override Task Where_simple_shadow_projection(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_simple_shadow_projection(a);

                AssertSql(
                    """
SELECT VALUE c["Title"]
FROM root c
WHERE (c["Title"] = "Sales Representative")
""");
            });

    public override Task Where_simple_shadow_subquery(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Where_simple_shadow_subquery(async),
            CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override async Task Where_shadow_subquery_FirstOrDefault(bool async)
    {
        // Uncorrelated subquery, not supported by Cosmos
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
        // Uncorrelated subquery, not supported by Cosmos
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

    public override Task Where_equals_method_string(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_equals_method_string(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["City"] = "London")
""");
            });

    public override Task Where_equals_method_string_with_ignore_case(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_equals_method_string_with_ignore_case(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE STRINGEQUALS(c["City"], "London", true)
""");
            });

    public override Task Where_equals_method_int(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_equals_method_int(a);

                AssertSql("ReadItem(None, 1)");
            });

    public override Task Where_equals_using_object_overload_on_mismatched_types(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_equals_using_object_overload_on_mismatched_types(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE false
""");
            });

    public override Task Where_equals_using_int_overload_on_mismatched_types(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_equals_using_int_overload_on_mismatched_types(a);

                AssertSql("ReadItem(None, 1)");
            });

    public override Task Where_equals_on_mismatched_types_nullable_int_long(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_equals_on_mismatched_types_nullable_int_long(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE false
""",
                    //
                    """
SELECT VALUE c
FROM root c
WHERE false
""");
            });

    public override Task Where_equals_on_mismatched_types_nullable_long_nullable_int(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_equals_on_mismatched_types_nullable_long_nullable_int(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE false
""",
                    //
                    """
SELECT VALUE c
FROM root c
WHERE false
""");
            });

    public override Task Where_equals_on_mismatched_types_int_nullable_int(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_equals_on_mismatched_types_int_nullable_int(a);

                AssertSql(
                    """
@__intPrm_0='2'

SELECT VALUE c
FROM root c
WHERE (c["ReportsTo"] = @__intPrm_0)
""",
                    //
                    """
@__intPrm_0='2'

SELECT VALUE c
FROM root c
WHERE (@__intPrm_0 = c["ReportsTo"])
""");
            });

    public override Task Where_equals_on_matched_nullable_int_types(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_equals_on_matched_nullable_int_types(a);

                AssertSql(
                    """
@__nullableIntPrm_0='2'

SELECT VALUE c
FROM root c
WHERE (@__nullableIntPrm_0 = c["ReportsTo"])
""",
                    //
                    """
@__nullableIntPrm_0='2'

SELECT VALUE c
FROM root c
WHERE (c["ReportsTo"] = @__nullableIntPrm_0)
""");
            });

    public override Task Where_equals_on_null_nullable_int_types(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_equals_on_null_nullable_int_types(a);

                AssertSql(
                    """
@__nullableIntPrm_0=null

SELECT VALUE c
FROM root c
WHERE (@__nullableIntPrm_0 = c["ReportsTo"])
""",
                    //
                    """
@__nullableIntPrm_0=null

SELECT VALUE c
FROM root c
WHERE (c["ReportsTo"] = @__nullableIntPrm_0)
""");
            });

    public override Task Where_comparison_nullable_type_not_null(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_comparison_nullable_type_not_null(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["ReportsTo"] = 2)
""");
            });

    public override Task Where_comparison_nullable_type_null(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_comparison_nullable_type_null(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["ReportsTo"] = null)
""");
            });

    public override Task Where_string_length(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_string_length(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (LENGTH(c["City"]) = 6)
""");
            });

    public override Task Where_string_indexof(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_string_indexof(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (INDEX_OF(c["City"], "Sea") != -1)
""");
            });

    public override Task Where_string_replace(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_string_replace(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (REPLACE(c["City"], "Sea", "Rea") = "Reattle")
""");
            });

    public override Task Where_string_substring(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_string_substring(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (SUBSTRING(c["City"], 1, 2) = "ea")
""");
            });

    public override async Task Where_datetime_now(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_datetime_now(async));

        AssertSql();
    }

    public override Task Where_datetime_utcnow(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_datetime_utcnow(a);

                AssertSql(
                    """
@__myDatetime_0='2015-04-10T00:00:00'

SELECT VALUE c
FROM root c
WHERE (GetCurrentDateTime() != @__myDatetime_0)
""");
            });

    public override Task Where_datetimeoffset_utcnow(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_datetimeoffset_utcnow(a);

                AssertSql(
                    """
@__myDatetimeOffset_0='2015-04-10T00:00:00-08:00'

SELECT VALUE c
FROM root c
WHERE (GetCurrentDateTime() != @__myDatetimeOffset_0)
""");
            });

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

    public override Task Where_date_add_year_constant_component(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_date_add_year_constant_component(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Order") AND (DateTimePart("yyyy", DateTimeAdd("yyyy", -1, c["OrderDate"])) = 1997))
""");
            });

    public override Task Where_datetime_year_component(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_datetime_year_component(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Order") AND (DateTimePart("yyyy", c["OrderDate"]) = 1998))
""");
            });

    public override Task Where_datetime_month_component(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_datetime_month_component(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Order") AND (DateTimePart("mm", c["OrderDate"]) = 4))
""");
            });

    public override async Task Where_datetime_dayOfYear_component(bool async)
    {
        // DateTime.DayOfYear not supported by Cosmos
        await AssertTranslationFailed(() => base.Where_datetime_dayOfYear_component(async));

        AssertSql();
    }

    public override Task Where_datetime_day_component(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_datetime_day_component(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Order") AND (DateTimePart("dd", c["OrderDate"]) = 4))
""");
            });

    public override Task Where_datetime_hour_component(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_datetime_hour_component(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Order") AND (DateTimePart("hh", c["OrderDate"]) = 0))
""");
            });

    public override Task Where_datetime_minute_component(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_datetime_minute_component(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Order") AND (DateTimePart("mi", c["OrderDate"]) = 0))
""");
            });

    public override Task Where_datetime_second_component(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_datetime_second_component(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Order") AND (DateTimePart("ss", c["OrderDate"]) = 0))
""");
            });

    public override Task Where_datetime_millisecond_component(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_datetime_millisecond_component(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Order") AND (DateTimePart("ms", c["OrderDate"]) = 0))
""");
            });

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

    public override Task Where_simple_reversed(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_simple_reversed(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ("London" = c["City"])
""");
            });

    public override Task Where_is_null(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_is_null(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Region"] = null)
""");
            });

    public override Task Where_null_is_null(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_null_is_null(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
""");
            });

    public override Task Where_constant_is_null(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_constant_is_null(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE false
""");
            });

    public override Task Where_is_not_null(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_is_not_null(a);
                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["City"] != null)
""");
            });

    public override Task Where_null_is_not_null(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_null_is_not_null(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE false
""");
            });

    public override Task Where_constant_is_not_null(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_constant_is_not_null(a);
                AssertSql(
                    """
SELECT VALUE c
FROM root c
""");
            });

    public override Task Where_identity_comparison(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_identity_comparison(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["City"] = c["City"])
""");
            });

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

    public override Task Where_primitive(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Where_primitive(async),
            CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override Task Where_bool_member(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_bool_member(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND c["Discontinued"])
""");
            });

    public override Task Where_bool_member_false(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_bool_member_false(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND NOT(c["Discontinued"]))
""");
            });

    public override async Task Where_bool_client_side_negated(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_bool_client_side_negated(async));

        AssertSql();
    }

    public override Task Where_bool_member_negated_twice(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_bool_member_negated_twice(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND NOT(NOT((c["Discontinued"] = true))))
""");
            });

    public override Task Where_bool_member_shadow(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_bool_member_shadow(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND c["Discontinued"])
""");
            });

    public override Task Where_bool_member_false_shadow(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_bool_member_false_shadow(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND NOT(c["Discontinued"]))
""");
            });

    public override Task Where_bool_member_equals_constant(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_bool_member_equals_constant(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND (c["Discontinued"] = true))
""");
            });

    public override Task Where_bool_member_in_complex_predicate(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_bool_member_in_complex_predicate(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND (((c["ProductID"] > 100) AND c["Discontinued"]) OR (c["Discontinued"] = true)))
""");
            });

    public override Task Where_bool_member_compared_to_binary_expression(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_bool_member_compared_to_binary_expression(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND (c["Discontinued"] = (c["ProductID"] > 50)))
""");
            });

    public override Task Where_not_bool_member_compared_to_not_bool_member(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_not_bool_member_compared_to_not_bool_member(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND (NOT(c["Discontinued"]) = NOT(c["Discontinued"])))
""");
            });

    public override Task Where_negated_boolean_expression_compared_to_another_negated_boolean_expression(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_negated_boolean_expression_compared_to_another_negated_boolean_expression(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND (NOT((c["ProductID"] > 50)) = NOT((c["ProductID"] > 20))))
""");
            });

    public override Task Where_not_bool_member_compared_to_binary_expression(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_not_bool_member_compared_to_binary_expression(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND (NOT(c["Discontinued"]) = (c["ProductID"] > 50)))
""");
            });

    public override Task Where_bool_parameter(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_bool_parameter(a);

                AssertSql(
                    """
@__prm_0='true'

SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND @__prm_0)
""");
            });

    public override Task Where_bool_parameter_compared_to_binary_expression(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_bool_parameter_compared_to_binary_expression(a);

                AssertSql(
                    """
@__prm_0='true'

SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND ((c["ProductID"] > 50) != @__prm_0))
""");
            });

    public override Task Where_bool_member_and_parameter_compared_to_binary_expression_nested(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_bool_member_and_parameter_compared_to_binary_expression_nested(a);

                AssertSql(
                    """
@__prm_0='true'

SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND (c["Discontinued"] = ((c["ProductID"] > 50) != @__prm_0)))
""");
            });

    public override Task Where_de_morgan_or_optimized(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_de_morgan_or_optimized(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND NOT((c["Discontinued"] OR (c["ProductID"] < 20))))
""");
            });

    public override Task Where_de_morgan_and_optimized(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_de_morgan_and_optimized(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND NOT((c["Discontinued"] AND (c["ProductID"] < 20))))
""");
            });

    public override Task Where_complex_negated_expression_optimized(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_complex_negated_expression_optimized(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND NOT((NOT((NOT(c["Discontinued"]) AND (c["ProductID"] < 60))) OR NOT((c["ProductID"] > 30)))))
""");
            });

    public override Task Where_short_member_comparison(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_short_member_comparison(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND (c["UnitsInStock"] > 10))
""");
            });

    public override Task Where_comparison_to_nullable_bool(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_comparison_to_nullable_bool(a);
                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (ENDSWITH(c["id"], "KI") = true)
""");
            });

    public override Task Where_true(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_true(a);
                AssertSql(
                    """
SELECT VALUE c
FROM root c
""");
            });

    public override Task Where_false(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_false(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE false
""");
            });

    public override Task Where_bool_closure(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_bool_closure(a);
                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE false
""",
                    //
                    "ReadItem(None, ALFKI)",
                    //
                    "ReadItem(None, ALFKI)",
                    //
                    """
SELECT VALUE c
FROM root c
""");
            });

    public override Task Where_default(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_default(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Fax"] = null)
""");
            });

    public override Task Where_expression_invoke_1(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_expression_invoke_1(a);

                AssertSql("ReadItem(None, ALFKI)");
            });

    public override async Task Where_expression_invoke_2(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_expression_invoke_2(async));

        AssertSql();
    }

    public override Task Where_expression_invoke_3(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_expression_invoke_3(a);

                AssertSql("ReadItem(None, ALFKI)");
            });

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

        AssertSql(
        );
    }

    public override Task Where_string_concat_method_comparison(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_string_concat_method_comparison(a);

                AssertSql(
                    """
@__i_0='A'

SELECT VALUE c["id"]
FROM root c
WHERE ((@__i_0 || c["id"]) = "AAROUT")
""");
            });

    public override Task Where_string_concat_method_comparison_2(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_string_concat_method_comparison_2(a);

                AssertSql(
                    """
@__i_0='A'
@__j_1='B'

SELECT VALUE c["id"]
FROM root c
WHERE ((@__i_0 || (@__j_1 || c["id"])) = "ABANATR")
""");
            });

    public override Task Where_string_concat_method_comparison_3(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_string_concat_method_comparison_3(a);

                AssertSql(
                    """
@__i_0='A'
@__j_1='B'
@__k_2='C'

SELECT VALUE c["id"]
FROM root c
WHERE ((@__i_0 || (@__j_1 || (@__k_2 || c["id"]))) = "ABCANTON")
""");
            });

    public override Task Where_ternary_boolean_condition_true(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_ternary_boolean_condition_true(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND (c["UnitsInStock"] >= 20))
""");
            });

    public override Task Where_ternary_boolean_condition_false(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_ternary_boolean_condition_false(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND (c["UnitsInStock"] < 20))
""");
            });

    public override Task Where_ternary_boolean_condition_with_another_condition(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_ternary_boolean_condition_with_another_condition(a);

                AssertSql(
                    """
@__productId_0='15'

SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND ((c["ProductID"] < @__productId_0) AND (c["UnitsInStock"] >= 20)))
""");
            });

    public override Task Where_ternary_boolean_condition_with_false_as_result_true(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_ternary_boolean_condition_with_false_as_result_true(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND (c["UnitsInStock"] >= 20))
""");
            });

    public override Task Where_ternary_boolean_condition_with_false_as_result_false(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_ternary_boolean_condition_with_false_as_result_false(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND false)
""");
            });

    public override Task Where_ternary_boolean_condition_negated(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_ternary_boolean_condition_negated(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND NOT(((c["UnitsInStock"] >= 20) ? false : true)))
""");
            });

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

    public override Task Where_compare_null(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_compare_null(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Region"] = null) AND (c["Country"] = "UK"))
""");
            });

    public override Task Where_Is_on_same_type(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_Is_on_same_type(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
""");
            });

    public override Task Where_chain(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_chain(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (((c["$type"] = "Order") AND (c["CustomerID"] = "QUICK")) AND (c["OrderDate"] > "1998-01-01T00:00:00"))
""");
            });

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

    public override Task Where_array_index(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_array_index(a);

                AssertSql("ReadItem(None, ALFKI)");
            });

    public override async Task Where_multiple_contains_in_subquery_with_or(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_multiple_contains_in_subquery_with_or(async));

        AssertSql(
        );
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
        // Uncorrelated subquery, not supported by Cosmos
        await AssertTranslationFailed(() => base.Where_subquery_FirstOrDefault_is_null(async));

        AssertSql();
    }

    public override async Task Where_subquery_FirstOrDefault_compared_to_entity(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_subquery_FirstOrDefault_compared_to_entity(async));

        AssertSql();
    }

    public override Task Time_of_day_datetime(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Time_of_day_datetime(a);

                AssertSql(
                    """
SELECT VALUE c["OrderDate"]
FROM root c
WHERE (c["$type"] = "Order")
""");
            });

    public override Task TypeBinary_short_circuit(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.TypeBinary_short_circuit(a);

                AssertSql(
                    """
@__p_0='false'

SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Order") AND @__p_0)
""");
            });

    public override async Task Decimal_cast_to_double_works(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Decimal_cast_to_double_works(async));

        AssertSql();
    }

    public override Task Where_is_conditional(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_is_conditional(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND (true ? false : true))
""");
            });

    public override async Task Filter_non_nullable_value_after_FirstOrDefault_on_empty_collection(bool async)
    {
        // Uncorrelated subquery, not supported by Cosmos
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

    public override Task Where_list_object_contains_over_value_type(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_list_object_contains_over_value_type(a);

                AssertSql(
                    """
@__orderIds_0='[10248,10249]'

SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Order") AND ARRAY_CONTAINS(@__orderIds_0, c["OrderID"]))
""");
            });

    public override Task Where_array_of_object_contains_over_value_type(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_array_of_object_contains_over_value_type(a);

                AssertSql(
                    """
@__orderIds_0='[10248,10249]'

SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Order") AND ARRAY_CONTAINS(@__orderIds_0, c["OrderID"]))
""");
            });

    public override Task Filter_with_EF_Property_using_closure_for_property_name(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Filter_with_EF_Property_using_closure_for_property_name(a);

                AssertSql("ReadItem(None, ALFKI)");
            });

    public override Task Filter_with_EF_Property_using_function_for_property_name(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Filter_with_EF_Property_using_function_for_property_name(a);

                AssertSql("ReadItem(None, ALFKI)");
            });

    public override async Task FirstOrDefault_over_scalar_projection_compared_to_null(bool async)
    {
        // Uncorrelated subquery, not supported by Cosmos
        await AssertTranslationFailed(() => base.FirstOrDefault_over_scalar_projection_compared_to_null(async));

        AssertSql();
    }

    public override async Task FirstOrDefault_over_scalar_projection_compared_to_not_null(bool async)
    {
        // Uncorrelated subquery, not supported by Cosmos
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

    public override async Task ElementAt_over_custom_projection_compared_to_not_null(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.ElementAt_over_custom_projection_compared_to_not_null(async));

        AssertSql();
    }

    public override async Task ElementAtOrDefault_over_custom_projection_compared_to_null(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.ElementAtOrDefault_over_custom_projection_compared_to_null(async));

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

    public override Task Where_Contains_and_comparison(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_Contains_and_comparison(a);

                AssertSql(
                    """
@__customerIds_0='["ALFKI","FISSA","WHITC"]'

SELECT VALUE c
FROM root c
WHERE (ARRAY_CONTAINS(@__customerIds_0, c["id"]) AND (c["City"] = "Seattle"))
""");
            });

    public override Task Where_Contains_or_comparison(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_Contains_or_comparison(a);

                AssertSql(
                    """
@__customerIds_0='["ALFKI","FISSA"]'

SELECT VALUE c
FROM root c
WHERE (ARRAY_CONTAINS(@__customerIds_0, c["id"]) OR (c["City"] = "Seattle"))
""");
            });

    public override async Task Where_Like_and_comparison(bool async)
    {
        await AssertTranslationFailed(() => base.Where_Like_and_comparison(async));

        AssertSql();
    }

    public override async Task Where_Like_or_comparison(bool async)
    {
        await AssertTranslationFailed(() => base.Where_Like_or_comparison(async));

        AssertSql(
        );
    }

    public override Task GetType_on_non_hierarchy1(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.GetType_on_non_hierarchy1(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
""");
            });

    public override Task GetType_on_non_hierarchy2(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.GetType_on_non_hierarchy2(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE false
""");
            });

    public override Task GetType_on_non_hierarchy3(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.GetType_on_non_hierarchy3(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE false
""");
            });

    public override Task GetType_on_non_hierarchy4(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.GetType_on_non_hierarchy4(a);
                AssertSql(
                    """
SELECT VALUE c
FROM root c
""");
            });

    public override Task Case_block_simplification_works_correctly(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Case_block_simplification_works_correctly(a);
                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (((c["Region"] = null) ? "OR" : c["Region"]) = "OR")
""");
            });

    public override Task Where_compare_null_with_cast_to_object(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_compare_null_with_cast_to_object(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Region"] = null)
""");
            });

    public override Task Where_compare_with_both_cast_to_object(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_compare_with_both_cast_to_object(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["City"] = "London")
""");
            });

    public override Task Where_projection(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_projection(a);

                AssertSql(
                    """
SELECT VALUE c["CompanyName"]
FROM root c
WHERE (c["City"] = "London")
""");
            });

    public override Task Enclosing_class_settable_member_generates_parameter(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Enclosing_class_settable_member_generates_parameter(a);

                AssertSql(
                    "ReadItem(None, Order|10274)",
                    //
                    "ReadItem(None, Order|10275)");
            });

    public override Task Enclosing_class_readonly_member_generates_parameter(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Enclosing_class_readonly_member_generates_parameter(a);

                AssertSql("ReadItem(None, Order|10275)");
            });

    public override Task Enclosing_class_const_member_does_not_generate_parameter(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Enclosing_class_const_member_does_not_generate_parameter(a);

                AssertSql("ReadItem(None, Order|10274)");
            });

    public override Task Generic_Ilist_contains_translates_to_server(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Generic_Ilist_contains_translates_to_server(a);
                AssertSql(
                    """
@__cities_0='["Seattle"]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@__cities_0, c["City"])
""");
            });

    public override Task Multiple_OrElse_on_same_column_converted_to_in_with_overlap(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Multiple_OrElse_on_same_column_converted_to_in_with_overlap(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((((c["id"] = "ALFKI") OR (c["id"] = "ANATR")) OR (c["id"] = "ANTON")) OR (c["id"] = "ANATR"))
""");
            });

    public override Task Multiple_OrElse_on_same_column_with_null_constant_comparison_converted_to_in(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Multiple_OrElse_on_same_column_with_null_constant_comparison_converted_to_in(a);
                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((((c["Region"] = "WA") OR (c["Region"] = "OR")) OR (c["Region"] = null)) OR (c["Region"] = "BC"))
""");
            });

    public override Task Constant_array_Contains_OrElse_comparison_with_constant_gets_combined_to_one_in(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Constant_array_Contains_OrElse_comparison_with_constant_gets_combined_to_one_in(a);
                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["id"] IN ("ALFKI", "ANATR") OR (c["id"] = "ANTON"))
""");
            });

    public override Task Constant_array_Contains_OrElse_comparison_with_constant_gets_combined_to_one_in_with_overlap(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Constant_array_Contains_OrElse_comparison_with_constant_gets_combined_to_one_in_with_overlap(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (((c["id"] = "ANTON") OR c["id"] IN ("ALFKI", "ANATR")) OR (c["id"] = "ALFKI"))
""");
            });

    public override Task Constant_array_Contains_OrElse_another_Contains_gets_combined_to_one_in_with_overlap(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Constant_array_Contains_OrElse_another_Contains_gets_combined_to_one_in_with_overlap(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["id"] IN ("ALFKI", "ANATR") OR c["id"] IN ("ALFKI", "ANTON"))
""");
            });

    public override Task Constant_array_Contains_AndAlso_another_Contains_gets_combined_to_one_in_with_overlap(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Constant_array_Contains_AndAlso_another_Contains_gets_combined_to_one_in_with_overlap(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["id"] NOT IN ("ALFKI", "ANATR") AND c["id"] NOT IN ("ALFKI", "ANTON"))
""");
            });

    public override Task Multiple_AndAlso_on_same_column_converted_to_in_using_parameters(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Multiple_AndAlso_on_same_column_converted_to_in_using_parameters(a);

                AssertSql(
                    """
@__prm1_0='ALFKI'
@__prm2_1='ANATR'
@__prm3_2='ANTON'

SELECT VALUE c
FROM root c
WHERE (((c["id"] != @__prm1_0) AND (c["id"] != @__prm2_1)) AND (c["id"] != @__prm3_2))
""");
            });

    public override Task Array_of_parameters_Contains_OrElse_comparison_with_constant_gets_combined_to_one_in(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Array_of_parameters_Contains_OrElse_comparison_with_constant_gets_combined_to_one_in(a);

                AssertSql(
                    """
@__prm1_0='ALFKI'
@__prm2_1='ANATR'

SELECT VALUE c
FROM root c
WHERE (c["id"] IN (@__prm1_0, @__prm2_1) OR (c["id"] = "ANTON"))
""");
            });

    public override Task Multiple_OrElse_on_same_column_with_null_parameter_comparison_converted_to_in(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Multiple_OrElse_on_same_column_with_null_parameter_comparison_converted_to_in(a);

                AssertSql(
                    """
@__prm_0=null

SELECT VALUE c
FROM root c
WHERE ((((c["Region"] = "WA") OR (c["Region"] = "OR")) OR (c["Region"] = @__prm_0)) OR (c["Region"] = "BC"))
""");
            });

    public override Task Parameter_array_Contains_OrElse_comparison_with_constant(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Parameter_array_Contains_OrElse_comparison_with_constant(a);

                AssertSql(
                    """
@__array_0='["ALFKI","ANATR"]'

SELECT VALUE c
FROM root c
WHERE (ARRAY_CONTAINS(@__array_0, c["id"]) OR (c["id"] = "ANTON"))
""");
            });

    public override Task Parameter_array_Contains_OrElse_comparison_with_parameter_with_overlap(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Parameter_array_Contains_OrElse_comparison_with_parameter_with_overlap(a);

                AssertSql(
                    """
@__prm1_0='ANTON'
@__array_1='["ALFKI","ANATR"]'
@__prm2_2='ALFKI'

SELECT VALUE c
FROM root c
WHERE (((c["id"] = @__prm1_0) OR ARRAY_CONTAINS(@__array_1, c["id"])) OR (c["id"] = @__prm2_2))
""");
            });

    public override Task Two_sets_of_comparison_combine_correctly(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Two_sets_of_comparison_combine_correctly(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["id"] IN ("ALFKI", "ANATR") AND ((c["id"] = "ANATR") OR (c["id"] = "ANTON")))
""");
            });

    public override Task Two_sets_of_comparison_combine_correctly2(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Two_sets_of_comparison_combine_correctly2(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((((c["Region"] != "WA") AND (c["Region"] != "OR")) AND (c["Region"] != null)) OR ((c["Region"] != "WA") AND (c["Region"] != null)))
""");
            });

    public override Task Filter_with_property_compared_to_null_wrapped_in_explicit_convert_to_object(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Filter_with_property_compared_to_null_wrapped_in_explicit_convert_to_object(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Region"] = null)
""");
            });

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

    public override Task Where_simple_shadow_projection_mixed(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_simple_shadow_projection_mixed(a);

                AssertSql(
                    """
SELECT VALUE
{
    "e" : c,
    "Title" : c["Title"]
}
FROM root c
WHERE (c["Title"] = "Sales Representative")
""");
            });

    public override Task Where_primitive_tracked(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Where_primitive_tracked(async),
            CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override Task Where_primitive_tracked2(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Where_primitive_tracked2(async),
            CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override Task Where_poco_closure(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_poco_closure(a);
                AssertSql(
                    """
@__entity_equality_customer_0_CustomerID='ALFKI'

SELECT VALUE c["id"]
FROM root c
WHERE (c["id"] = @__entity_equality_customer_0_CustomerID)
""",
                    //
                    """
@__entity_equality_customer_0_CustomerID='ANATR'

SELECT VALUE c["id"]
FROM root c
WHERE (c["id"] = @__entity_equality_customer_0_CustomerID)
""");
            });

    public override Task Where_concat_string_string_comparison(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_concat_string_string_comparison(a);

                AssertSql(
                    """
@__i_0='A'

SELECT VALUE c["id"]
FROM root c
WHERE ((@__i_0 || c["id"]) = "AALFKI")
""");
            });

    public override async Task EF_Constant(bool async)
    {
        // #34327
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.EF_Constant(async));
        Assert.Equal(CoreStrings.EFConstantNotSupported, exception.Message);
    }

    public override async Task EF_Constant_with_subtree(bool async)
    {
        // #34327
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.EF_Constant_with_subtree(async));
        Assert.Equal(CoreStrings.EFConstantNotSupported, exception.Message);
    }

    public override async Task EF_Constant_does_not_parameterized_as_part_of_bigger_subtree(bool async)
    {
        // #34327
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.EF_Constant_does_not_parameterized_as_part_of_bigger_subtree(async));
        Assert.Equal(CoreStrings.EFConstantNotSupported, exception.Message);
    }

    public override async Task EF_Constant_with_non_evaluatable_argument_throws(bool async)
    {
        await base.EF_Constant_with_non_evaluatable_argument_throws(async);
        AssertSql(
        );
    }

    public override Task EF_Parameter(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.EF_Parameter(a);

                AssertSql("ReadItem(None, ALFKI)");
            });

    public override Task EF_Parameter_with_subtree(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.EF_Parameter_with_subtree(a);

                AssertSql("ReadItem(None, ALFKI)");
            });

    public override Task EF_Parameter_does_not_parameterized_as_part_of_bigger_subtree(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.EF_Parameter_does_not_parameterized_as_part_of_bigger_subtree(a);

                AssertSql(
                    """
@__id_0='ALF'

SELECT VALUE c
FROM root c
WHERE (c["id"] = (@__id_0 || "KI"))
""");
            });

    public override async Task EF_Parameter_with_non_evaluatable_argument_throws(bool async)
    {
        await base.EF_Parameter_with_non_evaluatable_argument_throws(async);

        AssertSql();
    }

    public override Task Implicit_cast_in_predicate(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Implicit_cast_in_predicate(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Order") AND (c["CustomerID"] = "1337"))
""",
                    //
                    """
@__prm_Value_0='1337'

SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Order") AND (c["CustomerID"] = @__prm_Value_0))
""",
                    //
                    """
@__ToString_0='1337'

SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Order") AND (c["CustomerID"] = @__ToString_0))
""",
                    //
                    """
@__p_0='1337'

SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Order") AND (c["CustomerID"] = @__p_0))
""",
                    //
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Order") AND (c["CustomerID"] = "1337"))
""");
            });

    public override Task Interface_casting_though_generic_method(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Interface_casting_though_generic_method(a);

                AssertSql(
                    """
@__id_0='10252'

SELECT VALUE c["OrderID"]
FROM root c
WHERE ((c["$type"] = "Order") AND (c["OrderID"] = @__id_0))
""",
                    //
                    """
SELECT VALUE c["OrderID"]
FROM root c
WHERE ((c["$type"] = "Order") AND (c["OrderID"] = 10252))
""",
                    //
                    """
SELECT VALUE c["OrderID"]
FROM root c
WHERE ((c["$type"] = "Order") AND (c["OrderID"] = 10252))
""",
                    //
                    """
SELECT VALUE c["OrderID"]
FROM root c
WHERE ((c["$type"] = "Order") AND (c["OrderID"] = 10252))
""");
            });

    public override Task Simplifiable_coalesce_over_nullable(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Simplifiable_coalesce_over_nullable(a);

                AssertSql(
                    """
ReadItem(None, Order|10248)
""");
            });

    #region Evaluation order of predicates

    public override Task Take_and_Where_evaluation_order(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Take_and_Where_evaluation_order(async),
            CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override Task Skip_and_Where_evaluation_order(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Skip_and_Where_evaluation_order(async),
            CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override Task Take_and_Distinct_evaluation_order(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Take_and_Distinct_evaluation_order(async),
            CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    #endregion Evaluation order of predicates

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}
