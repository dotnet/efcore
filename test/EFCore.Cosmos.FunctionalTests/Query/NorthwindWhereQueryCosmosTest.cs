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

    public override async Task Where_simple(bool async)
    {
        await base.Where_simple(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["City"] = "London")
""");
    }

    private static readonly Expression<Func<Order, bool>> _filter = o => o.CustomerID == "ALFKI";

    public override async Task Where_as_queryable_expression(bool async)
    {
        // Uncorrelated subquery, not supported by Cosmos
        await AssertTranslationFailed(() => base.Where_as_queryable_expression(async));

        AssertSql();
    }

    public override async Task<string> Where_simple_closure(bool async)
    {
        var queryString = await base.Where_simple_closure(async);

        AssertSql(
            """
@city='London'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @city)
""");

        return null;
    }

    public override async Task Where_indexer_closure(bool async)
    {
        await base.Where_indexer_closure(async);

        AssertSql(
            """
@p='London'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @p)
""");
    }

    public override async Task Where_dictionary_key_access_closure(bool async)
    {
        await base.Where_dictionary_key_access_closure(async);

        AssertSql(
            """
@get_Item='London'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @get_Item)
""");
    }

    public override async Task Where_tuple_item_closure(bool async)
    {
        await base.Where_tuple_item_closure(async);

        AssertSql(
            """
@predicateTuple_Item2='London'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @predicateTuple_Item2)
""");
    }

    public override async Task Where_named_tuple_item_closure(bool async)
    {
        await base.Where_named_tuple_item_closure(async);

        AssertSql(
            """
@predicateTuple_Item2='London'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @predicateTuple_Item2)
""");
    }

    public override async Task Where_simple_closure_constant(bool async)
    {
        await base.Where_simple_closure_constant(async);

        AssertSql(
            """
@predicate='true'

SELECT VALUE c
FROM root c
WHERE @predicate
""");
    }

    public override async Task Where_simple_closure_via_query_cache(bool async)
    {
        await base.Where_simple_closure_via_query_cache(async);

        AssertSql(
            """
@city='London'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @city)
""",
            //
            """
@city='Seattle'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @city)
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
@GetCity='London'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @GetCity)
""",
            //
            """
@GetCity='Seattle'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @GetCity)
""");
    }

    public override async Task Where_field_access_closure_via_query_cache(bool async)
    {
        await base.Where_field_access_closure_via_query_cache(async);

        AssertSql(
            """
@city_InstanceFieldValue='London'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @city_InstanceFieldValue)
""",
            //
            """
@city_InstanceFieldValue='Seattle'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @city_InstanceFieldValue)
""");
    }

    public override async Task Where_property_access_closure_via_query_cache(bool async)
    {
        await base.Where_property_access_closure_via_query_cache(async);

        AssertSql(
            """
@city_InstancePropertyValue='London'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @city_InstancePropertyValue)
""",
            //
            """
@city_InstancePropertyValue='Seattle'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @city_InstancePropertyValue)
""");
    }

    public override async Task Where_static_field_access_closure_via_query_cache(bool async)
    {
        await base.Where_static_field_access_closure_via_query_cache(async);

        AssertSql(
            """
@StaticFieldValue='London'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @StaticFieldValue)
""",
            //
            """
@StaticFieldValue='Seattle'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @StaticFieldValue)
""");
    }

    public override async Task Where_static_property_access_closure_via_query_cache(bool async)
    {
        await base.Where_static_property_access_closure_via_query_cache(async);

        AssertSql(
            """
@StaticPropertyValue='London'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @StaticPropertyValue)
""",
            //
            """
@StaticPropertyValue='Seattle'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @StaticPropertyValue)
""");
    }

    public override async Task Where_nested_field_access_closure_via_query_cache(bool async)
    {
        await base.Where_nested_field_access_closure_via_query_cache(async);

        AssertSql(
            """
@city_Nested_InstanceFieldValue='London'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @city_Nested_InstanceFieldValue)
""",
            //
            """
@city_Nested_InstanceFieldValue='Seattle'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @city_Nested_InstanceFieldValue)
""");
    }

    public override async Task Where_nested_property_access_closure_via_query_cache(bool async)
    {
        await base.Where_nested_property_access_closure_via_query_cache(async);

        AssertSql(
            """
@city_Nested_InstancePropertyValue='London'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @city_Nested_InstancePropertyValue)
""",
            //
            """
@city_Nested_InstancePropertyValue='Seattle'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @city_Nested_InstancePropertyValue)
""");
    }

    public override async Task Where_new_instance_field_access_query_cache(bool async)
    {
        await base.Where_new_instance_field_access_query_cache(async);

        AssertSql(
            """
@InstanceFieldValue='London'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @InstanceFieldValue)
""",
            //
            """
@InstanceFieldValue='Seattle'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @InstanceFieldValue)
""");
    }

    public override async Task Where_new_instance_field_access_closure_via_query_cache(bool async)
    {
        await base.Where_new_instance_field_access_closure_via_query_cache(async);

        AssertSql(
            """
@InstanceFieldValue='London'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @InstanceFieldValue)
""",
            //
            """
@InstanceFieldValue='Seattle'

SELECT VALUE c
FROM root c
WHERE (c["City"] = @InstanceFieldValue)
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

    [ConditionalTheory(Skip = "Always uses sync code.")]
    public override Task Where_subquery_closure_via_query_cache(bool async)
        => Task.CompletedTask;

    public override async Task Where_simple_shadow(bool async)
    {
        await base.Where_simple_shadow(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Title"] = "Sales Representative")
""");
    }

    public override async Task Where_simple_shadow_projection(bool async)
    {
        await base.Where_simple_shadow_projection(async);

        AssertSql(
            """
SELECT VALUE c["Title"]
FROM root c
WHERE (c["Title"] = "Sales Representative")
""");
    }

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

    public override async Task Where_equals_method_int(bool async)
    {
        await base.Where_equals_method_int(async);

                AssertSql("ReadItem(None, 1)");
    }

    public override async Task Where_equals_using_object_overload_on_mismatched_types(bool async)
    {
        await base.Where_equals_using_object_overload_on_mismatched_types(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE false
""");
    }

    public override async Task Where_equals_using_int_overload_on_mismatched_types(bool async)
    {
        await base.Where_equals_using_int_overload_on_mismatched_types(async);

                AssertSql("ReadItem(None, 1)");
    }

    public override async Task Where_equals_on_mismatched_types_nullable_int_long(bool async)
    {
        await base.Where_equals_on_mismatched_types_nullable_int_long(async);

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
    }

    public override async Task Where_equals_on_mismatched_types_nullable_long_nullable_int(bool async)
    {
        await base.Where_equals_on_mismatched_types_nullable_long_nullable_int(async);

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
    }

    public override async Task Where_equals_on_mismatched_types_int_nullable_int(bool async)
    {
        await base.Where_equals_on_mismatched_types_int_nullable_int(async);

        AssertSql(
            """
@intPrm='2'

SELECT VALUE c
FROM root c
WHERE (c["ReportsTo"] = @intPrm)
""",
            //
            """
@intPrm='2'

SELECT VALUE c
FROM root c
WHERE (@intPrm = c["ReportsTo"])
""");
    }

    public override async Task Where_equals_on_matched_nullable_int_types(bool async)
    {
        await base.Where_equals_on_matched_nullable_int_types(async);

        AssertSql(
            """
@nullableIntPrm='2'

SELECT VALUE c
FROM root c
WHERE (@nullableIntPrm = c["ReportsTo"])
""",
            //
            """
@nullableIntPrm='2'

SELECT VALUE c
FROM root c
WHERE (c["ReportsTo"] = @nullableIntPrm)
""");
    }

    public override async Task Where_equals_on_null_nullable_int_types(bool async)
    {
        await base.Where_equals_on_null_nullable_int_types(async);

        AssertSql(
            """
@nullableIntPrm=null

SELECT VALUE c
FROM root c
WHERE (@nullableIntPrm = c["ReportsTo"])
""",
            //
            """
@nullableIntPrm=null

SELECT VALUE c
FROM root c
WHERE (c["ReportsTo"] = @nullableIntPrm)
""");
    }

    public override async Task Where_comparison_nullable_type_not_null(bool async)
    {
        await base.Where_comparison_nullable_type_not_null(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["ReportsTo"] = 2)
""");
    }

    public override async Task Where_comparison_nullable_type_null(bool async)
    {
        await base.Where_comparison_nullable_type_null(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["ReportsTo"] = null)
""");
    }

    public override async Task Where_simple_reversed(bool async)
    {
        await base.Where_simple_reversed(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ("London" = c["City"])
""");
    }

    public override async Task Where_is_null(bool async)
    {
        await base.Where_is_null(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Region"] = null)
""");
    }

    public override async Task Where_null_is_null(bool async)
    {
        await base.Where_null_is_null(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
""");
    }

    public override async Task Where_constant_is_null(bool async)
    {
        await base.Where_constant_is_null(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE false
""");
    }

    public override async Task Where_is_not_null(bool async)
    {
        await base.Where_is_not_null(async);
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["City"] != null)
""");
    }

    public override async Task Where_null_is_not_null(bool async)
    {
        await base.Where_null_is_not_null(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE false
""");
    }

    public override async Task Where_constant_is_not_null(bool async)
    {
        await base.Where_constant_is_not_null(async);
        AssertSql(
            """
SELECT VALUE c
FROM root c
""");
    }

    public override async Task Where_identity_comparison(bool async)
    {
        await base.Where_identity_comparison(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["City"] = c["City"])
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

    public override Task Where_primitive(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Where_primitive(async),
            CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override async Task Where_bool_member(bool async)
    {
        await base.Where_bool_member(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND c["Discontinued"])
""");
    }

    public override async Task Where_bool_member_false(bool async)
    {
        await base.Where_bool_member_false(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND NOT(c["Discontinued"]))
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
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND NOT(NOT((c["Discontinued"] = true))))
""");
    }

    public override async Task Where_bool_member_shadow(bool async)
    {
        await base.Where_bool_member_shadow(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND c["Discontinued"])
""");
    }

    public override async Task Where_bool_member_false_shadow(bool async)
    {
        await base.Where_bool_member_false_shadow(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND NOT(c["Discontinued"]))
""");
    }

    public override async Task Where_bool_member_equals_constant(bool async)
    {
        await base.Where_bool_member_equals_constant(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND (c["Discontinued"] = true))
""");
    }

    public override async Task Where_bool_member_in_complex_predicate(bool async)
    {
        await base.Where_bool_member_in_complex_predicate(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND (((c["ProductID"] > 100) AND c["Discontinued"]) OR (c["Discontinued"] = true)))
""");
    }

    public override async Task Where_bool_member_compared_to_binary_expression(bool async)
    {
        await base.Where_bool_member_compared_to_binary_expression(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND (c["Discontinued"] = (c["ProductID"] > 50)))
""");
    }

    public override async Task Where_not_bool_member_compared_to_not_bool_member(bool async)
    {
        await base.Where_not_bool_member_compared_to_not_bool_member(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND (NOT(c["Discontinued"]) = NOT(c["Discontinued"])))
""");
    }

    public override async Task Where_negated_boolean_expression_compared_to_another_negated_boolean_expression(bool async)
    {
        await base.Where_negated_boolean_expression_compared_to_another_negated_boolean_expression(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND (NOT((c["ProductID"] > 50)) = NOT((c["ProductID"] > 20))))
""");
    }

    public override async Task Where_not_bool_member_compared_to_binary_expression(bool async)
    {
        await base.Where_not_bool_member_compared_to_binary_expression(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND (NOT(c["Discontinued"]) = (c["ProductID"] > 50)))
""");
    }

    public override async Task Where_bool_parameter(bool async)
    {
        await base.Where_bool_parameter(async);

        AssertSql(
            """
@prm='true'

SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND @prm)
""");
    }

    public override async Task Where_bool_parameter_compared_to_binary_expression(bool async)
    {
        await base.Where_bool_parameter_compared_to_binary_expression(async);

        AssertSql(
            """
@prm='true'

SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND ((c["ProductID"] > 50) != @prm))
""");
    }

    public override async Task Where_bool_member_and_parameter_compared_to_binary_expression_nested(bool async)
    {
        await base.Where_bool_member_and_parameter_compared_to_binary_expression_nested(async);

        AssertSql(
            """
@prm='true'

SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND (c["Discontinued"] = ((c["ProductID"] > 50) != @prm)))
""");
    }

    public override async Task Where_de_morgan_or_optimized(bool async)
    {
        await base.Where_de_morgan_or_optimized(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND NOT((c["Discontinued"] OR (c["ProductID"] < 20))))
""");
    }

    public override async Task Where_de_morgan_and_optimized(bool async)
    {
        await base.Where_de_morgan_and_optimized(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND NOT((c["Discontinued"] AND (c["ProductID"] < 20))))
""");
    }

    public override async Task Where_complex_negated_expression_optimized(bool async)
    {
        await base.Where_complex_negated_expression_optimized(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND NOT((NOT((NOT(c["Discontinued"]) AND (c["ProductID"] < 60))) OR NOT((c["ProductID"] > 30)))))
""");
    }

    public override async Task Where_short_member_comparison(bool async)
    {
        await base.Where_short_member_comparison(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND (c["UnitsInStock"] > 10))
""");
    }

    public override async Task Where_comparison_to_nullable_bool(bool async)
    {
        await base.Where_comparison_to_nullable_bool(async);
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (ENDSWITH(c["id"], "KI") = true)
""");
    }

    public override async Task Where_true(bool async)
    {
        await base.Where_true(async);
        AssertSql(
            """
SELECT VALUE c
FROM root c
""");
    }

    public override async Task Where_false(bool async)
    {
        await base.Where_false(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE false
""");
    }

    public override async Task Where_bool_closure(bool async)
    {
        await base.Where_bool_closure(async);
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
    }

    public override async Task Where_default(bool async)
    {
        await base.Where_default(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Fax"] = null)
""");
    }

    public override async Task Where_expression_invoke_1(bool async)
    {
        await base.Where_expression_invoke_1(async);

                AssertSql("ReadItem(None, ALFKI)");
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

                AssertSql("ReadItem(None, ALFKI)");
    }

    public override async Task Where_ternary_boolean_condition_true(bool async)
    {
        await base.Where_ternary_boolean_condition_true(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND (c["UnitsInStock"] >= 20))
""");
    }

    public override async Task Where_ternary_boolean_condition_false(bool async)
    {
        await base.Where_ternary_boolean_condition_false(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND (c["UnitsInStock"] < 20))
""");
    }

    public override async Task Where_ternary_boolean_condition_with_another_condition(bool async)
    {
        await base.Where_ternary_boolean_condition_with_another_condition(async);

        AssertSql(
            """
@productId='15'

SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND ((c["ProductID"] < @productId) AND (c["UnitsInStock"] >= 20)))
""");
    }

    public override async Task Where_ternary_boolean_condition_with_false_as_result_true(bool async)
    {
        await base.Where_ternary_boolean_condition_with_false_as_result_true(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND (c["UnitsInStock"] >= 20))
""");
    }

    public override async Task Where_ternary_boolean_condition_with_false_as_result_false(bool async)
    {
        await base.Where_ternary_boolean_condition_with_false_as_result_false(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND false)
""");
    }

    public override async Task Where_ternary_boolean_condition_negated(bool async)
    {
        await base.Where_ternary_boolean_condition_negated(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND NOT(((c["UnitsInStock"] >= 20) ? false : true)))
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
SELECT VALUE c
FROM root c
WHERE ((c["Region"] = null) AND (c["Country"] = "UK"))
""");
    }

    public override async Task Where_Is_on_same_type(bool async)
    {
        await base.Where_Is_on_same_type(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
""");
    }

    public override async Task Where_chain(bool async)
    {
        await base.Where_chain(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (((c["$type"] = "Order") AND (c["CustomerID"] = "QUICK")) AND (c["OrderDate"] > "1998-01-01T00:00:00"))
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

                AssertSql("ReadItem(None, ALFKI)");
    }

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

    public override async Task TypeBinary_short_circuit(bool async)
    {
        await base.TypeBinary_short_circuit(async);

        AssertSql(
            """
@p='false'

SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Order") AND @p)
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
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Product") AND (true ? false : true))
""");
    }

    public override async Task Filter_non_nullable_value_after_FirstOrDefault_on_empty_collection(bool async)
    {
        // Uncorrelated subquery, not supported by Cosmos
        await AssertTranslationFailed(() => base.Filter_non_nullable_value_after_FirstOrDefault_on_empty_collection(async));

        AssertSql();
    }

    public override async Task Using_same_parameter_twice_in_query_generates_one_sql_parameter(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Using_same_parameter_twice_in_query_generates_one_sql_parameter(async));

        AssertSql();
    }

    public override async Task Two_parameters_with_same_name_get_uniquified(bool async)
    {
        // Concat with conversion, issue #34963.
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

    // TODO: The base implementations no longer compile since https://github.com/dotnet/runtime/pull/110197 (Contains overload added with
    // optional parameter, not supported in expression trees). #35547 is tracking on the EF side.
    //
    // public override async Task Where_collection_navigation_ToArray_Contains(bool async)
    // {
    //     // Cosmos client evaluation. Issue #17246.
    //     await AssertTranslationFailed(() => base.Where_collection_navigation_ToArray_Contains(async));
    //
    //     AssertSql();
    // }

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
@orderIds='[10248,10249]'

SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Order") AND ARRAY_CONTAINS(@orderIds, c["OrderID"]))
""");
    }

// TODO: The base implementations no longer compile since https://github.com/dotnet/runtime/pull/110197 (Contains overload added with
// optional parameter, not supported in expression trees). #35547 is tracking on the EF side.
//
//     public override Task Where_array_of_object_contains_over_value_type(bool async)
//         => Fixture.NoSyncTest(
//             async, async a =>
//             {
//                 await base.Where_array_of_object_contains_over_value_type(a);
//
//                 AssertSql(
//                     """
// @orderIds='[10248,10249]'
//
// SELECT VALUE c
// FROM root c
// WHERE ((c["$type"] = "Order") AND ARRAY_CONTAINS(@orderIds, c["OrderID"]))
// """);
//             });

    public override async Task Filter_with_EF_Property_using_closure_for_property_name(bool async)
    {
        await base.Filter_with_EF_Property_using_closure_for_property_name(async);

                AssertSql("ReadItem(None, ALFKI)");
    }

    public override async Task Filter_with_EF_Property_using_function_for_property_name(bool async)
    {
        await base.Filter_with_EF_Property_using_function_for_property_name(async);

                AssertSql("ReadItem(None, ALFKI)");
    }

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

    public override async Task Where_Contains_and_comparison(bool async)
    {
        await base.Where_Contains_and_comparison(async);

        AssertSql(
            """
@customerIds='["ALFKI","FISSA","WHITC"]'

SELECT VALUE c
FROM root c
WHERE (ARRAY_CONTAINS(@customerIds, c["id"]) AND (c["City"] = "Seattle"))
""");
    }

    public override async Task Where_Contains_or_comparison(bool async)
    {
        await base.Where_Contains_or_comparison(async);

        AssertSql(
            """
@customerIds='["ALFKI","FISSA"]'

SELECT VALUE c
FROM root c
WHERE (ARRAY_CONTAINS(@customerIds, c["id"]) OR (c["City"] = "Seattle"))
""");
    }

    public override async Task GetType_on_non_hierarchy1(bool async)
    {
        await base.GetType_on_non_hierarchy1(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
""");
    }

    public override async Task GetType_on_non_hierarchy2(bool async)
    {
        await base.GetType_on_non_hierarchy2(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE false
""");
    }

    public override async Task GetType_on_non_hierarchy3(bool async)
    {
        await base.GetType_on_non_hierarchy3(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE false
""");
    }

    public override async Task GetType_on_non_hierarchy4(bool async)
    {
        await base.GetType_on_non_hierarchy4(async);
        AssertSql(
            """
SELECT VALUE c
FROM root c
""");
    }

    public override async Task Case_block_simplification_works_correctly(bool async)
    {
        await base.Case_block_simplification_works_correctly(async);
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (((c["Region"] = null) ? "OR" : c["Region"]) = "OR")
""");
    }

    public override async Task Where_compare_null_with_cast_to_object(bool async)
    {
        await base.Where_compare_null_with_cast_to_object(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Region"] = null)
""");
    }

    public override async Task Where_compare_with_both_cast_to_object(bool async)
    {
        await base.Where_compare_with_both_cast_to_object(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["City"] = "London")
""");
    }

    public override async Task Where_projection(bool async)
    {
        await base.Where_projection(async);

        AssertSql(
            """
SELECT VALUE c["CompanyName"]
FROM root c
WHERE (c["City"] = "London")
""");
    }

    public override async Task Enclosing_class_settable_member_generates_parameter(bool async)
    {
        await base.Enclosing_class_settable_member_generates_parameter(async);

                AssertSql(
                    "ReadItem(None, Order|10274)",
                    //
                    "ReadItem(None, Order|10275)");
    }

    public override async Task Enclosing_class_readonly_member_generates_parameter(bool async)
    {
        await base.Enclosing_class_readonly_member_generates_parameter(async);

                AssertSql("ReadItem(None, Order|10275)");
    }

    public override async Task Enclosing_class_const_member_does_not_generate_parameter(bool async)
    {
        await base.Enclosing_class_const_member_does_not_generate_parameter(async);

                AssertSql("ReadItem(None, Order|10274)");
    }

    public override async Task Generic_Ilist_contains_translates_to_server(bool async)
    {
        await base.Generic_Ilist_contains_translates_to_server(async);
        AssertSql(
            """
@cities='["Seattle"]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@cities, c["City"])
""");
    }

    public override async Task Multiple_OrElse_on_same_column_converted_to_in_with_overlap(bool async)
    {
        await base.Multiple_OrElse_on_same_column_converted_to_in_with_overlap(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((((c["id"] = "ALFKI") OR (c["id"] = "ANATR")) OR (c["id"] = "ANTON")) OR (c["id"] = "ANATR"))
""");
    }

    public override async Task Multiple_OrElse_on_same_column_with_null_constant_comparison_converted_to_in(bool async)
    {
        await base.Multiple_OrElse_on_same_column_with_null_constant_comparison_converted_to_in(async);
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((((c["Region"] = "WA") OR (c["Region"] = "OR")) OR (c["Region"] = null)) OR (c["Region"] = "BC"))
""");
    }

    public override async Task Constant_array_Contains_OrElse_comparison_with_constant_gets_combined_to_one_in(bool async)
    {
        await base.Constant_array_Contains_OrElse_comparison_with_constant_gets_combined_to_one_in(async);
        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["id"] IN ("ALFKI", "ANATR") OR (c["id"] = "ANTON"))
""");
    }

    public override async Task Constant_array_Contains_OrElse_comparison_with_constant_gets_combined_to_one_in_with_overlap(bool async)
    {
        await base.Constant_array_Contains_OrElse_comparison_with_constant_gets_combined_to_one_in_with_overlap(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (((c["id"] = "ANTON") OR c["id"] IN ("ALFKI", "ANATR")) OR (c["id"] = "ALFKI"))
""");
    }

    public override async Task Constant_array_Contains_OrElse_another_Contains_gets_combined_to_one_in_with_overlap(bool async)
    {
        await base.Constant_array_Contains_OrElse_another_Contains_gets_combined_to_one_in_with_overlap(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["id"] IN ("ALFKI", "ANATR") OR c["id"] IN ("ALFKI", "ANTON"))
""");
    }

    public override async Task Constant_array_Contains_AndAlso_another_Contains_gets_combined_to_one_in_with_overlap(bool async)
    {
        await base.Constant_array_Contains_AndAlso_another_Contains_gets_combined_to_one_in_with_overlap(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["id"] NOT IN ("ALFKI", "ANATR") AND c["id"] NOT IN ("ALFKI", "ANTON"))
""");
    }

    public override async Task Multiple_AndAlso_on_same_column_converted_to_in_using_parameters(bool async)
    {
        await base.Multiple_AndAlso_on_same_column_converted_to_in_using_parameters(async);

        AssertSql(
            """
@prm1='ALFKI'
@prm2='ANATR'
@prm3='ANTON'

SELECT VALUE c
FROM root c
WHERE (((c["id"] != @prm1) AND (c["id"] != @prm2)) AND (c["id"] != @prm3))
""");
    }

    public override async Task Array_of_parameters_Contains_OrElse_comparison_with_constant_gets_combined_to_one_in(bool async)
    {
        await base.Array_of_parameters_Contains_OrElse_comparison_with_constant_gets_combined_to_one_in(async);

        AssertSql(
            """
@prm1='ALFKI'
@prm2='ANATR'

SELECT VALUE c
FROM root c
WHERE (c["id"] IN (@prm1, @prm2) OR (c["id"] = "ANTON"))
""");
    }

    public override async Task Multiple_OrElse_on_same_column_with_null_parameter_comparison_converted_to_in(bool async)
    {
        await base.Multiple_OrElse_on_same_column_with_null_parameter_comparison_converted_to_in(async);

        AssertSql(
            """
@prm=null

SELECT VALUE c
FROM root c
WHERE ((((c["Region"] = "WA") OR (c["Region"] = "OR")) OR (c["Region"] = @prm)) OR (c["Region"] = "BC"))
""");
    }

    public override async Task Parameter_array_Contains_OrElse_comparison_with_constant(bool async)
    {
        await base.Parameter_array_Contains_OrElse_comparison_with_constant(async);

        AssertSql(
            """
@array='["ALFKI","ANATR"]'

SELECT VALUE c
FROM root c
WHERE (ARRAY_CONTAINS(@array, c["id"]) OR (c["id"] = "ANTON"))
""");
    }

    public override async Task Parameter_array_Contains_OrElse_comparison_with_parameter_with_overlap(bool async)
    {
        await base.Parameter_array_Contains_OrElse_comparison_with_parameter_with_overlap(async);

        AssertSql(
            """
@prm1='ANTON'
@array='["ALFKI","ANATR"]'
@prm2='ALFKI'

SELECT VALUE c
FROM root c
WHERE (((c["id"] = @prm1) OR ARRAY_CONTAINS(@array, c["id"])) OR (c["id"] = @prm2))
""");
    }

    public override async Task Two_sets_of_comparison_combine_correctly(bool async)
    {
        await base.Two_sets_of_comparison_combine_correctly(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["id"] IN ("ALFKI", "ANATR") AND ((c["id"] = "ANATR") OR (c["id"] = "ANTON")))
""");
    }

    public override async Task Two_sets_of_comparison_combine_correctly2(bool async)
    {
        await base.Two_sets_of_comparison_combine_correctly2(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((((c["Region"] != "WA") AND (c["Region"] != "OR")) AND (c["Region"] != null)) OR ((c["Region"] != "WA") AND (c["Region"] != null)))
""");
    }

    public override async Task Filter_with_property_compared_to_null_wrapped_in_explicit_convert_to_object(bool async)
    {
        await base.Filter_with_property_compared_to_null_wrapped_in_explicit_convert_to_object(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Region"] = null)
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
SELECT VALUE
{
    "e" : c,
    "Title" : c["Title"]
}
FROM root c
WHERE (c["Title"] = "Sales Representative")
""");
    }

    public override Task Where_primitive_tracked(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Where_primitive_tracked(async),
            CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override Task Where_primitive_tracked2(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Where_primitive_tracked2(async),
            CosmosStrings.LimitOffsetNotSupportedInSubqueries);

    public override async Task Where_poco_closure(bool async)
    {
        await base.Where_poco_closure(async);
        AssertSql(
            """
@entity_equality_customer_CustomerID='ALFKI'

SELECT VALUE c["id"]
FROM root c
WHERE (c["id"] = @entity_equality_customer_CustomerID)
""",
            //
            """
@entity_equality_customer_CustomerID='ANATR'

SELECT VALUE c["id"]
FROM root c
WHERE (c["id"] = @entity_equality_customer_CustomerID)
""");
    }

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

    public override async Task EF_Parameter(bool async)
    {
        await base.EF_Parameter(async);

                AssertSql("ReadItem(None, ALFKI)");
    }

    public override async Task EF_Parameter_with_subtree(bool async)
    {
        await base.EF_Parameter_with_subtree(async);

                AssertSql("ReadItem(None, ALFKI)");
    }

    public override async Task EF_Parameter_does_not_parameterized_as_part_of_bigger_subtree(bool async)
    {
        await base.EF_Parameter_does_not_parameterized_as_part_of_bigger_subtree(async);

        AssertSql(
            """
@id='ALF'

SELECT VALUE c
FROM root c
WHERE (c["id"] = (@id || "KI"))
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
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Order") AND (c["CustomerID"] = "1337"))
""",
            //
            """
@prm_Value='1337'

SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Order") AND (c["CustomerID"] = @prm_Value))
""",
            //
            """
@ToString='1337'

SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Order") AND (c["CustomerID"] = @ToString))
""",
            //
            """
@p='1337'

SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Order") AND (c["CustomerID"] = @p))
""",
            //
            """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Order") AND (c["CustomerID"] = "1337"))
""");
    }

    public override async Task Interface_casting_though_generic_method(bool async)
    {
        await base.Interface_casting_though_generic_method(async);

        AssertSql(
            """
@id='10252'

SELECT VALUE c["OrderID"]
FROM root c
WHERE ((c["$type"] = "Order") AND (c["OrderID"] = @id))
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
    }

    public override async Task Simplifiable_coalesce_over_nullable(bool async)
    {
        await base.Simplifiable_coalesce_over_nullable(async);

        AssertSql(
            """
ReadItem(None, Order|10248)
""");
    }

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
