// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class NorthwindSelectQueryCosmosTest : NorthwindSelectQueryTestBase<NorthwindQueryCosmosFixture<NoopModelCustomizer>>
{
    public NorthwindSelectQueryCosmosTest(
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
    public virtual Task Projection_with_Value_Property(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await AssertQuery(
                    async,
                    ss => ss.Set<Order>().Select(o => new { Value = o.OrderID }),
                    e => e.Value);

                AssertSql(
                    """
SELECT VALUE {"Value" : c["OrderID"]}
FROM root c
WHERE (c["Discriminator"] = "Order")
""");
            });

    public override Task Projection_when_arithmetic_expression_precedence(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Projection_when_arithmetic_expression_precedence(a);

                AssertSql(
                    """
SELECT VALUE {"A" : (c["OrderID"] / (c["OrderID"] / 2)), "B" : ((c["OrderID"] / c["OrderID"]) / 2)}
FROM root c
WHERE (c["Discriminator"] = "Order")
""");
            });

    public override Task Projection_when_arithmetic_expressions(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Projection_when_arithmetic_expressions(a);

                AssertSql(
                    """
SELECT VALUE {"OrderID" : c["OrderID"], "Double" : (c["OrderID"] * 2), "Add" : (c["OrderID"] + 23), "Sub" : (100000 - c["OrderID"]), "Divide" : (c["OrderID"] / (c["OrderID"] / 2)), "Literal" : 42, "o" : c}
FROM root c
WHERE (c["Discriminator"] = "Order")
""");
            });

    public override async Task Projection_when_arithmetic_mixed(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Projection_when_arithmetic_mixed(async));

        AssertSql();
    }

    public override async Task Projection_when_arithmetic_mixed_subqueries(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Projection_when_arithmetic_mixed_subqueries(async));

        AssertSql();
    }

    public override Task Projection_when_null_value(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Projection_when_null_value(a);

                AssertSql(
                    """
SELECT c["Region"]
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override async Task Projection_when_client_evald_subquery(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Projection_when_client_evald_subquery(async));

        AssertSql();
    }

    public override Task Project_to_object_array(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Project_to_object_array(a);

                AssertSql(
                    """
SELECT c["EmployeeID"], c["ReportsTo"], c["Title"]
FROM root c
WHERE ((c["Discriminator"] = "Employee") AND (c["EmployeeID"] = 1))
""");
            });

    public override Task Projection_of_entity_type_into_object_array(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Projection_of_entity_type_into_object_array(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND STARTSWITH(c["CustomerID"], "A"))
ORDER BY c["CustomerID"]
""");
            });

    public override async Task Projection_of_multiple_entity_types_into_object_array(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Projection_of_multiple_entity_types_into_object_array(async));

        AssertSql();
    }

    public override Task Projection_of_entity_type_into_object_list(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Projection_of_entity_type_into_object_list(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY c["CustomerID"]
""");
            });

    public override Task Project_to_int_array(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Project_to_int_array(a);

                AssertSql(
                    """
SELECT c["EmployeeID"], c["ReportsTo"]
FROM root c
WHERE ((c["Discriminator"] = "Employee") AND (c["EmployeeID"] = 1))
""");
            });

    public override async Task Select_bool_closure_with_order_by_property_with_cast_to_nullable(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_bool_closure_with_order_by_property_with_cast_to_nullable(async));

        AssertSql();
    }

    public override async Task Select_bool_closure_with_order_parameter_with_cast_to_nullable(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await Assert.ThrowsAsync<CosmosException>(
                () => base.Select_bool_closure_with_order_parameter_with_cast_to_nullable(async));

            AssertSql(
                """
@__boolean_0='false'

SELECT VALUE {"c" : @__boolean_0}
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY @__boolean_0
""");
        }
    }

    public override Task Select_scalar(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_scalar(a);

                AssertSql(
                    """
SELECT c["City"]
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override Task Select_anonymous_one(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_anonymous_one(a);

                AssertSql(
                    """
SELECT c["City"]
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override Task Select_anonymous_two(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_anonymous_two(a);

                AssertSql(
                    """
SELECT c["City"], c["Phone"]
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override Task Select_anonymous_three(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_anonymous_three(a);

                AssertSql(
                    """
SELECT c["City"], c["Phone"], c["Country"]
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override Task Select_anonymous_bool_constant_true(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_anonymous_bool_constant_true(a);

                AssertSql(
                    """
SELECT VALUE {"CustomerID" : c["CustomerID"], "ConstantTrue" : true}
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override Task Select_anonymous_constant_in_expression(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_anonymous_constant_in_expression(a);

                AssertSql(
                    """
SELECT VALUE {"CustomerID" : c["CustomerID"], "Expression" : (LENGTH(c["CustomerID"]) + 5)}
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override Task Select_anonymous_conditional_expression(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_anonymous_conditional_expression(a);

                AssertSql(
                    """
SELECT VALUE {"ProductID" : c["ProductID"], "IsAvailable" : (c["UnitsInStock"] > 0)}
FROM root c
WHERE (c["Discriminator"] = "Product")
""");
            });

    public override Task Select_anonymous_with_object(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_anonymous_with_object(a);

                AssertSql(
                    """
SELECT c["City"], c
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override Task Select_constant_int(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_constant_int(a);

                AssertSql(
                    """
SELECT VALUE {"c" : 0}
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override Task Select_constant_null_string(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_constant_null_string(a);

                AssertSql(
                    """
SELECT VALUE {"c" : null}
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override Task Select_local(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_local(a);

                AssertSql(
                    """
@__x_0='10'

SELECT VALUE {"c" : @__x_0}
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override Task Select_scalar_primitive_after_take(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_scalar_primitive_after_take(a);

                AssertSql(
                    """
@__p_0='9'

SELECT c["EmployeeID"]
FROM root c
WHERE (c["Discriminator"] = "Employee")
OFFSET 0 LIMIT @__p_0
""");
            });

    public override Task Select_project_filter(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_project_filter(a);

                AssertSql(
                    """
SELECT c["CompanyName"]
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = "London"))
""");
            });

    public override Task Select_project_filter2(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_project_filter2(a);

                AssertSql(
                    """
SELECT c["City"]
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = "London"))
""");
            });

    public override async Task Select_nested_collection(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_nested_collection(async));

        AssertSql();
    }

    public override async Task Select_nested_collection_multi_level(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_nested_collection_multi_level(async));

        AssertSql();
    }

    public override async Task Select_nested_collection_multi_level2(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_nested_collection_multi_level2(async));

        AssertSql();
    }

    public override async Task Select_nested_collection_multi_level3(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_nested_collection_multi_level3(async));

        AssertSql();
    }

    public override async Task Select_nested_collection_multi_level4(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_nested_collection_multi_level4(async));

        AssertSql();
    }

    public override async Task Select_nested_collection_multi_level5(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_nested_collection_multi_level5(async));

        AssertSql();
    }

    public override async Task Select_nested_collection_multi_level6(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_nested_collection_multi_level6(async));

        AssertSql();
    }

    public override async Task Select_nested_collection_count_using_anonymous_type(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_nested_collection_count_using_anonymous_type(async));

        AssertSql();
    }

    public override Task New_date_time_in_anonymous_type_works(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.New_date_time_in_anonymous_type_works(a);

                AssertSql(
                    """
SELECT 1
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND STARTSWITH(c["CustomerID"], "A"))
""");
            });

    public override Task Select_non_matching_value_types_int_to_long_introduces_explicit_cast(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_non_matching_value_types_int_to_long_introduces_explicit_cast(a);

                AssertSql(
                    """
SELECT c["OrderID"]
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["CustomerID"] = "ALFKI"))
ORDER BY c["OrderID"]
""");
            });

    public override Task Select_non_matching_value_types_nullable_int_to_long_introduces_explicit_cast(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_non_matching_value_types_nullable_int_to_long_introduces_explicit_cast(a);

                AssertSql(
                    """
SELECT c["EmployeeID"]
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["CustomerID"] = "ALFKI"))
ORDER BY c["OrderID"]
""");
            });

    public override Task Select_non_matching_value_types_nullable_int_to_int_doesnt_introduce_explicit_cast(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_non_matching_value_types_nullable_int_to_int_doesnt_introduce_explicit_cast(a);

                AssertSql(
                    """
SELECT c["EmployeeID"]
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["CustomerID"] = "ALFKI"))
ORDER BY c["OrderID"]
""");
            });

    public override Task Select_non_matching_value_types_int_to_nullable_int_doesnt_introduce_explicit_cast(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_non_matching_value_types_int_to_nullable_int_doesnt_introduce_explicit_cast(a);

                AssertSql(
                    """
SELECT c["OrderID"]
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["CustomerID"] = "ALFKI"))
ORDER BY c["OrderID"]
""");
            });

    public override Task Select_non_matching_value_types_from_binary_expression_introduces_explicit_cast(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_non_matching_value_types_from_binary_expression_introduces_explicit_cast(a);

                AssertSql(
                    """
SELECT VALUE {"c" : (c["OrderID"] + c["OrderID"])}
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["CustomerID"] = "ALFKI"))
ORDER BY c["OrderID"]
""");
            });

    public override Task Select_non_matching_value_types_from_binary_expression_nested_introduces_top_level_explicit_cast(
        bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_non_matching_value_types_from_binary_expression_nested_introduces_top_level_explicit_cast(a);

                AssertSql(
                    """
SELECT c["OrderID"]
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["CustomerID"] = "ALFKI"))
ORDER BY c["OrderID"]
""");
            });

    public override Task Select_non_matching_value_types_from_unary_expression_introduces_explicit_cast1(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_non_matching_value_types_from_unary_expression_introduces_explicit_cast1(a);

                AssertSql(
                    """
SELECT VALUE {"c" : -(c["OrderID"])}
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["CustomerID"] = "ALFKI"))
ORDER BY c["OrderID"]
""");
            });

    public override Task Select_non_matching_value_types_from_unary_expression_introduces_explicit_cast2(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_non_matching_value_types_from_unary_expression_introduces_explicit_cast2(a);

                AssertSql(
                    """
SELECT c["OrderID"]
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["CustomerID"] = "ALFKI"))
ORDER BY c["OrderID"]
""");
            });

    public override Task Select_non_matching_value_types_from_length_introduces_explicit_cast(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_non_matching_value_types_from_length_introduces_explicit_cast(a);

                AssertSql(
                    """
SELECT LENGTH(c["CustomerID"]) AS c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["CustomerID"] = "ALFKI"))
ORDER BY c["OrderID"]
""");
            });

    public override Task Select_non_matching_value_types_from_method_call_introduces_explicit_cast(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_non_matching_value_types_from_method_call_introduces_explicit_cast(a);

                AssertSql(
                    """
SELECT ABS(c["OrderID"]) AS c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["CustomerID"] = "ALFKI"))
ORDER BY c["OrderID"]
""");
            });

    public override Task Select_non_matching_value_types_from_anonymous_type_introduces_explicit_cast(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_non_matching_value_types_from_anonymous_type_introduces_explicit_cast(a);

                AssertSql(
                    """
SELECT c["OrderID"]
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["CustomerID"] = "ALFKI"))
ORDER BY c["OrderID"]
""");
            });

    public override async Task
        Project_single_element_from_collection_with_OrderBy_Distinct_and_FirstOrDefault_followed_by_projecting_length(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(
            () => base
                .Project_single_element_from_collection_with_OrderBy_Distinct_and_FirstOrDefault_followed_by_projecting_length(async));

        AssertSql();
    }

    public override Task Select_conditional_with_null_comparison_in_test(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_conditional_with_null_comparison_in_test(a);

                AssertSql(
                    """
SELECT VALUE {"c" : ((c["CustomerID"] = null) ? true : (c["OrderID"] < 100))}
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["CustomerID"] = "ALFKI"))
""");
            });

    public override async Task Projection_in_a_subquery_should_be_liftable(bool async)
    {
        Assert.Equal(
            CosmosStrings.OffsetRequiresLimit,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Projection_in_a_subquery_should_be_liftable(async))).Message);

        AssertSql();
    }

    public override Task Projection_containing_DateTime_subtraction(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Projection_containing_DateTime_subtraction(a);

                AssertSql(
                    """
SELECT c["OrderDate"]
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderID"] < 10300))
""");
            });

    public override async Task Project_single_element_from_collection_with_OrderBy_Take_and_FirstOrDefault(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Project_single_element_from_collection_with_OrderBy_Take_and_FirstOrDefault(async));

        AssertSql();
    }

    public override async Task Project_single_element_from_collection_with_OrderBy_Skip_and_FirstOrDefault(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Project_single_element_from_collection_with_OrderBy_Skip_and_FirstOrDefault(async));

        AssertSql();
    }

    public override async Task Project_single_element_from_collection_with_OrderBy_Distinct_and_FirstOrDefault(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Project_single_element_from_collection_with_OrderBy_Distinct_and_FirstOrDefault(async));

        AssertSql();
    }

    public override async Task Project_single_element_from_collection_with_OrderBy_Take_and_SingleOrDefault(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Project_single_element_from_collection_with_OrderBy_Take_and_SingleOrDefault(async));

        AssertSql();
    }

    public override async Task Project_single_element_from_collection_with_OrderBy_Take_and_FirstOrDefault_with_parameter(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(
            () => base.Project_single_element_from_collection_with_OrderBy_Take_and_FirstOrDefault_with_parameter(async));

        AssertSql();
    }

    public override async Task Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(
            () => base.Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault(async));

        AssertSql();
    }

    public override async Task
        Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault_followed_by_projection_of_length_property(
            bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(
            () => base
                .Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault_followed_by_projection_of_length_property(
                    async));

        AssertSql();
    }

    public override async Task Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault_2(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(
            () => base.Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault_2(async));

        AssertSql();
    }

    public override async Task Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(
            () => base.Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault(async));

        AssertSql();
    }

    public override async Task Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault_2(
        bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(
            () => base.Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault_2(async));

        AssertSql();
    }

    public override Task Select_datetime_year_component(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_datetime_year_component(a);

                AssertSql(
                    """
SELECT c["OrderDate"]
FROM root c
WHERE (c["Discriminator"] = "Order")
""");
            });

    public override Task Select_datetime_month_component(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_datetime_month_component(a);

                AssertSql(
                    """
SELECT c["OrderDate"]
FROM root c
WHERE (c["Discriminator"] = "Order")
""");
            });

    public override Task Select_datetime_day_of_year_component(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_datetime_day_of_year_component(a);

                AssertSql(
                    """
SELECT c["OrderDate"]
FROM root c
WHERE (c["Discriminator"] = "Order")
""");
            });

    public override Task Select_datetime_day_component(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_datetime_day_component(a);

                AssertSql(
                    """
SELECT c["OrderDate"]
FROM root c
WHERE (c["Discriminator"] = "Order")
""");
            });

    public override Task Select_datetime_hour_component(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_datetime_hour_component(a);

                AssertSql(
                    """
SELECT c["OrderDate"]
FROM root c
WHERE (c["Discriminator"] = "Order")
""");
            });

    public override Task Select_datetime_minute_component(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_datetime_minute_component(a);

                AssertSql(
                    """
SELECT c["OrderDate"]
FROM root c
WHERE (c["Discriminator"] = "Order")
""");
            });

    public override Task Select_datetime_second_component(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_datetime_second_component(a);

                AssertSql(
                    """
SELECT c["OrderDate"]
FROM root c
WHERE (c["Discriminator"] = "Order")
""");
            });

    public override Task Select_datetime_millisecond_component(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_datetime_millisecond_component(a);

                AssertSql(
                    """
SELECT c["OrderDate"]
FROM root c
WHERE (c["Discriminator"] = "Order")
""");
            });

    public override Task Select_byte_constant(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_byte_constant(a);

                AssertSql(
                    """
SELECT VALUE {"c" : ((c["CustomerID"] = "ALFKI") ? 1 : 2)}
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override Task Select_short_constant(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_short_constant(a);

                AssertSql(
                    """
SELECT VALUE {"c" : ((c["CustomerID"] = "ALFKI") ? 1 : 2)}
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override Task Select_bool_constant(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_bool_constant(a);

                AssertSql(
                    """
SELECT VALUE {"c" : ((c["CustomerID"] = "ALFKI") ? true : false)}
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override Task Anonymous_projection_AsNoTracking_Selector(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Anonymous_projection_AsNoTracking_Selector(a);

                AssertSql(
                    """
SELECT c["OrderDate"]
FROM root c
WHERE (c["Discriminator"] = "Order")
""");
            });

    public override Task Anonymous_projection_with_repeated_property_being_ordered(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Anonymous_projection_with_repeated_property_being_ordered(a);

                AssertSql(
                    """
SELECT VALUE {"A" : c["CustomerID"]}
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY c["CustomerID"]
""");
            });

    public override async Task Anonymous_projection_with_repeated_property_being_ordered_2(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Anonymous_projection_with_repeated_property_being_ordered_2(async));

        AssertSql();
    }

    public override Task Select_GetValueOrDefault_on_DateTime(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_GetValueOrDefault_on_DateTime(a);

                AssertSql(
                    """
SELECT c["OrderDate"]
FROM root c
WHERE (c["Discriminator"] = "Order")
""");
            });

    public override async Task Select_GetValueOrDefault_on_DateTime_with_null_values(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_GetValueOrDefault_on_DateTime_with_null_values(async));

        AssertSql();
    }

    public override Task Client_method_in_projection_requiring_materialization_1(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Client_method_in_projection_requiring_materialization_1(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND STARTSWITH(c["CustomerID"], "A"))
""");
            });

    public override Task Client_method_in_projection_requiring_materialization_2(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Client_method_in_projection_requiring_materialization_2(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND STARTSWITH(c["CustomerID"], "A"))
""");
            });

    public override async Task Multiple_select_many_with_predicate(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Multiple_select_many_with_predicate(async));

        AssertSql();
    }

    public override async Task SelectMany_without_result_selector_naked_collection_navigation(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.SelectMany_without_result_selector_naked_collection_navigation(async));

        AssertSql();
    }

    public override async Task SelectMany_without_result_selector_collection_navigation_composed(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.SelectMany_without_result_selector_collection_navigation_composed(async));

        AssertSql();
    }

    public override async Task SelectMany_correlated_with_outer_1(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.SelectMany_correlated_with_outer_1(async));

        AssertSql();
    }

    public override async Task SelectMany_correlated_with_outer_2(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.SelectMany_correlated_with_outer_2(async));

        AssertSql();
    }

    public override async Task SelectMany_correlated_with_outer_3(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.SelectMany_correlated_with_outer_3(async));

        AssertSql();
    }

    public override async Task SelectMany_correlated_with_outer_4(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.SelectMany_correlated_with_outer_4(async));

        AssertSql();
    }

    public override async Task SelectMany_correlated_with_outer_5(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.SelectMany_correlated_with_outer_5(async));

        AssertSql();
    }

    public override async Task SelectMany_correlated_with_outer_6(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.SelectMany_correlated_with_outer_6(async));

        AssertSql();
    }

    public override async Task SelectMany_correlated_with_outer_7(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.SelectMany_correlated_with_outer_7(async));

        AssertSql();
    }

    public override async Task FirstOrDefault_over_empty_collection_of_value_type_returns_correct_results(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.FirstOrDefault_over_empty_collection_of_value_type_returns_correct_results(async));

        AssertSql();
    }

    public override async Task Project_non_nullable_value_after_FirstOrDefault_on_empty_collection(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Project_non_nullable_value_after_FirstOrDefault_on_empty_collection(async));

        AssertSql();
    }

    public override async Task Member_binding_after_ctor_arguments_fails_with_client_eval(bool async)
    {
        await AssertTranslationFailed(() => base.Member_binding_after_ctor_arguments_fails_with_client_eval(async));

        AssertSql();
    }

    public override async Task Filtered_collection_projection_is_tracked(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Member_binding_after_ctor_arguments_fails_with_client_eval(async));

        AssertSql();
    }

    public override async Task Filtered_collection_projection_with_to_list_is_tracked(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Filtered_collection_projection_with_to_list_is_tracked(async));

        AssertSql();
    }

    public override async Task SelectMany_with_collection_being_correlated_subquery_which_references_inner_and_outer_entity(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(
            () => base.SelectMany_with_collection_being_correlated_subquery_which_references_inner_and_outer_entity(async));

        AssertSql();
    }

    public override async Task Select_chained_entity_navigation_doesnt_materialize_intermittent_entities(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_chained_entity_navigation_doesnt_materialize_intermittent_entities(async));

        AssertSql();
    }

    public override async Task Select_entity_compared_to_null(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_entity_compared_to_null(async));

        AssertSql();
    }

    public override Task Explicit_cast_in_arithmetic_operation_is_preserved(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Explicit_cast_in_arithmetic_operation_is_preserved(a);

                AssertSql(
                    """
SELECT VALUE {"OrderID" : c["OrderID"], "c" : (c["OrderID"] + 1000)}
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderID"] = 10250))
""");
            });

    public override async Task SelectMany_whose_selector_references_outer_source(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.SelectMany_whose_selector_references_outer_source(async));

        AssertSql();
    }

    public override async Task Collection_FirstOrDefault_with_entity_equality_check_in_projection(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Collection_FirstOrDefault_with_entity_equality_check_in_projection(async));

        AssertSql();
    }

    public override async Task Collection_FirstOrDefault_with_nullable_unsigned_int_column(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Collection_FirstOrDefault_with_nullable_unsigned_int_column(async));

        AssertSql();
    }

    public override async Task ToList_Count_in_projection_works(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.ToList_Count_in_projection_works(async));

        AssertSql();
    }

    public override async Task LastOrDefault_member_access_in_projection_translates_to_server(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.LastOrDefault_member_access_in_projection_translates_to_server(async));

        AssertSql();
    }

    public override async Task Collection_projection_AsNoTracking_OrderBy(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Collection_projection_AsNoTracking_OrderBy(async));

        AssertSql();
    }

    public override Task Coalesce_over_nullable_uint(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Coalesce_over_nullable_uint(a);

                AssertSql(
                    """
SELECT VALUE {"c" : ((c["EmployeeID"] != null) ? c["EmployeeID"] : 0)}
FROM root c
WHERE (c["Discriminator"] = "Order")
""");
            });

    public override async Task Project_uint_through_collection_FirstOrDefault(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Project_uint_through_collection_FirstOrDefault(async));

        AssertSql();
    }

    public override async Task Project_keyless_entity_FirstOrDefault_without_orderby(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Project_keyless_entity_FirstOrDefault_without_orderby(async));

        AssertSql();
    }

    public override Task Reverse_changes_asc_order_to_desc(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Reverse_changes_asc_order_to_desc(a);

                AssertSql(
                    """
SELECT c["EmployeeID"]
FROM root c
WHERE (c["Discriminator"] = "Employee")
ORDER BY c["EmployeeID"] DESC
""");
            });

    public override Task Reverse_changes_desc_order_to_asc(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Reverse_changes_desc_order_to_asc(a);

                AssertSql(
                    """
SELECT c["EmployeeID"]
FROM root c
WHERE (c["Discriminator"] = "Employee")
ORDER BY c["EmployeeID"]
""");
            });

    public override async Task Projection_AsEnumerable_projection(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Projection_AsEnumerable_projection(async));

        AssertSql();
    }

    public override Task Projection_custom_type_in_both_sides_of_ternary(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Projection_custom_type_in_both_sides_of_ternary(a);

                AssertSql(
                    """
SELECT VALUE {"c" : (c["City"] = "Seattle")}
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY c["CustomerID"]
""");
            });

    public override async Task Projecting_multiple_collection_with_same_constant_works(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Projecting_multiple_collection_with_same_constant_works(async));

        AssertSql();
    }

    public override async Task Projecting_after_navigation_and_distinct(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Projecting_after_navigation_and_distinct(async));

        AssertSql();
    }

    public override async Task Correlated_collection_after_distinct_with_complex_projection_containing_original_identifier(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(
            () => base.Correlated_collection_after_distinct_with_complex_projection_containing_original_identifier(async));

        AssertSql();
    }

    public override async Task Correlated_collection_after_distinct_not_containing_original_identifier(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Correlated_collection_after_distinct_not_containing_original_identifier(async));

        AssertSql();
    }

    public override async Task Correlated_collection_after_distinct_with_complex_projection_not_containing_original_identifier(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(
            () => base.Correlated_collection_after_distinct_with_complex_projection_not_containing_original_identifier(async));

        AssertSql();
    }

    public override async Task Correlated_collection_after_groupby_with_complex_projection_containing_original_identifier(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(
            () => base.Correlated_collection_after_groupby_with_complex_projection_containing_original_identifier(async));

        AssertSql();
    }

    public override async Task Reverse_without_explicit_ordering(bool async)
    {
        await AssertTranslationFailedWithDetails(
            () => base.Reverse_without_explicit_ordering(async), CosmosStrings.MissingOrderingInSelectExpression);

        AssertSql();
    }

    public override async Task Custom_projection_reference_navigation_PK_to_FK_optimization(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Custom_projection_reference_navigation_PK_to_FK_optimization(async));

        AssertSql();
    }

    public override async Task Select_nested_collection_deep(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_nested_collection_deep(async));

        AssertSql();
    }

    public override async Task Projecting_Length_of_a_string_property_after_FirstOrDefault_on_correlated_collection(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(
            () => base.Projecting_Length_of_a_string_property_after_FirstOrDefault_on_correlated_collection(async));

        AssertSql();
    }

    public override Task Projection_take_predicate_projection(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Projection_take_predicate_projection(a);

                AssertSql(
                    """
@__p_0='10'

SELECT VALUE {"Aggregate" : ((c["CustomerID"] || " ") || c["City"])}
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND STARTSWITH(c["CustomerID"], "A"))
ORDER BY c["CustomerID"]
OFFSET 0 LIMIT @__p_0
""");
            });

    public override Task Projection_take_projection_doesnt_project_intermittent_column(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Projection_take_projection_doesnt_project_intermittent_column(a);

                AssertSql(
                    """
@__p_0='10'

SELECT VALUE {"Aggregate" : ((c["CustomerID"] || " ") || c["City"])}
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY c["CustomerID"]
OFFSET 0 LIMIT @__p_0
""");
            });

    public override async Task Projection_skip_projection_doesnt_project_intermittent_column(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Projection_skip_projection_doesnt_project_intermittent_column(async))).Message;

        Assert.Equal(CosmosStrings.OffsetRequiresLimit, message);
    }

    public override async Task Projection_Distinct_projection_preserves_columns_used_for_distinct_in_subquery(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Projection_Distinct_projection_preserves_columns_used_for_distinct_in_subquery(async));

        AssertSql();
    }

    public override async Task Projecting_count_of_navigation_which_is_generic_collection(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Projecting_count_of_navigation_which_is_generic_collection(async));

        AssertSql();
    }

    public override async Task Projecting_count_of_navigation_which_is_generic_list(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Projecting_count_of_navigation_which_is_generic_list(async));

        AssertSql();
    }

    public override async Task Do_not_erase_projection_mapping_when_adding_single_projection(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        Assert.Equal(
            CosmosStrings.NonEmbeddedIncludeNotSupported(
                "Navigation: Order.OrderDetails (ICollection<OrderDetail>) Collection ToDependent OrderDetail Inverse: Order"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Do_not_erase_projection_mapping_when_adding_single_projection(async)))
            .Message);

        AssertSql();
    }

    public override async Task Select_nested_collection_deep_distinct_no_identifiers(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_nested_collection_deep_distinct_no_identifiers(async));

        AssertSql();
    }

    public override async Task Correlated_collection_after_groupby_with_complex_projection_not_containing_original_identifier(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(
            () => base.Correlated_collection_after_groupby_with_complex_projection_not_containing_original_identifier(async));

        AssertSql();
    }

    public override Task Ternary_in_client_eval_assigns_correct_types(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Ternary_in_client_eval_assigns_correct_types(a);

                AssertSql(
                    """
SELECT VALUE {"CustomerID" : c["CustomerID"], "OrderDate" : c["OrderDate"], "c" : (c["OrderID"] - 10000)}
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderID"] < 10300))
ORDER BY c["OrderID"]
""");
            });

    public override async Task Collection_include_over_result_of_single_non_scalar(bool async)
    {
        // Cross collection join. Issue #17246.
        Assert.Equal(
            CosmosStrings.NonEmbeddedIncludeNotSupported(
                "Navigation: Customer.Orders (List<Order>) Collection ToDependent Order Inverse: Customer PropertyAccessMode.Field"),
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Collection_include_over_result_of_single_non_scalar(async)))
            .Message);

        AssertSql();
    }

    public override async Task Collection_projection_selecting_outer_element_followed_by_take(bool async)
    {
        // Cross collection join. Issue #17246.
        await AssertTranslationFailed(() => base.Collection_projection_selecting_outer_element_followed_by_take(async));

        AssertSql();
    }

    public override async Task Take_on_top_level_and_on_collection_projection_with_outer_apply(bool async)
    {
        // Cross collection join. Issue #17246.
        await AssertTranslationFailed(() => base.Take_on_top_level_and_on_collection_projection_with_outer_apply(async));

        AssertSql();
    }

    public override async Task Take_on_correlated_collection_in_first(bool async)
    {
        // Cross collection join. Issue #17246.
        await AssertTranslationFailed(() => base.Take_on_correlated_collection_in_first(async));

        AssertSql();
    }

    public override async Task Client_projection_via_ctor_arguments(bool async)
    {
        // Cross collection join. Issue #17246.
        await AssertTranslationFailed(() => base.Client_projection_via_ctor_arguments(async));

        AssertSql();
    }

    public override async Task Client_projection_with_string_initialization_with_scalar_subquery(bool async)
    {
        // Cross collection join. Issue #17246.
        await AssertTranslationFailed(() => base.Client_projection_with_string_initialization_with_scalar_subquery(async));

        AssertSql();
    }

    public override async Task Projecting_count_of_navigation_which_is_generic_collection_using_convert(bool async)
    {
        // Cross collection join. Issue #17246.
        await AssertTranslationFailed(() => base.Projecting_count_of_navigation_which_is_generic_collection_using_convert(async));

        AssertSql();
    }

    public override async Task MemberInit_in_projection_without_arguments(bool async)
    {
        // Cross collection join. Issue #17246.
        await AssertTranslationFailed(() => base.MemberInit_in_projection_without_arguments(async));

        AssertSql();
    }

    public override async Task Reverse_in_join_outer(bool async)
    {
        // Cross collection join. Issue #17246.
        await AssertTranslationFailed(() => base.Reverse_in_join_outer(async));

        AssertSql();
    }

    public override async Task Reverse_in_join_outer_with_take(bool async)
    {
        // Cross collection join. Issue #17246.
        await AssertTranslationFailed(() => base.Reverse_in_join_outer_with_take(async));

        AssertSql();
    }

    public override async Task Reverse_in_join_inner(bool async)
    {
        // Cross collection join. Issue #17246.
        await AssertTranslationFailed(() => base.Reverse_in_join_inner(async));

        AssertSql();
    }

    public override async Task Reverse_in_join_inner_with_skip(bool async)
    {
        Assert.Equal(
            CosmosStrings.ReverseAfterSkipTakeNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Reverse_in_join_inner_with_skip(async))).Message);

        AssertSql();
    }

    public override async Task Reverse_in_SelectMany(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Reverse_in_SelectMany(async));

        AssertSql();
    }

    public override async Task Reverse_in_SelectMany_with_Take(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Reverse_in_SelectMany_with_Take(async));

        AssertSql();
    }

    public override async Task Reverse_in_projection_subquery(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Reverse_in_projection_subquery(async));

        AssertSql();
    }

    public override async Task Reverse_in_projection_subquery_single_result(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Reverse_in_projection_subquery_single_result(async));

        AssertSql();
    }

    public override async Task Reverse_in_subquery_via_pushdown(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Reverse_in_subquery_via_pushdown(async));

        AssertSql();
    }

    public override async Task Reverse_in_projection_scalar_subquery(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Reverse_in_projection_scalar_subquery(async));

        AssertSql();
    }

    public override async Task Reverse_after_orderby_thenby(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await Assert.ThrowsAsync<CosmosException>(
                () => base.Reverse_after_orderby_thenby(async));

            AssertSql(
                """
SELECT c["EmployeeID"]
FROM root c
WHERE (c["Discriminator"] = "Employee")
ORDER BY c["EmployeeID"] DESC, c["City"]
""");
        }
    }

    public override async Task Reverse_after_orderBy_and_take(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Reverse_after_orderBy_and_take(async))).Message;

        Assert.Equal(CosmosStrings.ReverseAfterSkipTakeNotSupported, message);
    }

    public override async Task List_of_list_of_anonymous_type(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.List_of_list_of_anonymous_type(async));

        AssertSql();
    }

    public override async Task
        SelectMany_with_collection_being_correlated_subquery_which_references_non_mapped_properties_from_inner_and_outer_entity(
            bool async)
    {
        await AssertTranslationFailed(
            () => base
                .SelectMany_with_collection_being_correlated_subquery_which_references_non_mapped_properties_from_inner_and_outer_entity(
                    async));

        AssertSql();
    }

    public override Task Select_bool_closure(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_bool_closure(a);

                AssertSql(
                    """
SELECT 1
FROM root c
WHERE (c["Discriminator"] = "Customer")
""",
                    //
                    """
SELECT 1
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override Task Select_datetime_DayOfWeek_component(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_datetime_DayOfWeek_component(a);

                AssertSql(
                    """
SELECT c["OrderDate"]
FROM root c
WHERE (c["Discriminator"] = "Order")
""");
            });

    public override Task Reverse_after_multiple_orderbys(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Reverse_after_multiple_orderbys(a);

                AssertSql(
                    """
SELECT c["EmployeeID"]
FROM root c
WHERE (c["Discriminator"] = "Employee")
ORDER BY c["EmployeeID"]
""");
            });

    [ConditionalTheory(Skip = "Always does sync evaluation.")]
    public override async Task VisitLambda_should_not_be_visited_trivially(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await base.VisitLambda_should_not_be_visited_trivially(async);

            AssertSql(
                """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND STARTSWITH(c["CustomerID"], "A"))
""");
        }
    }

    public override Task Projecting_nullable_struct(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Projecting_nullable_struct(a);

                AssertSql(
                    """
SELECT c["CustomerID"], (c["CustomerID"] = "ALFKI") AS c, c["OrderID"], LENGTH(c["CustomerID"]) AS c0
FROM root c
WHERE (c["Discriminator"] = "Order")
""");
            });

    public override Task Select_customer_identity(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_customer_identity(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override Task Projection_with_parameterized_constructor(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Projection_with_parameterized_constructor(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = "ALFKI"))
""");
            });

    public override Task Select_anonymous_nested(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_anonymous_nested(a);

                AssertSql(
                    """
SELECT c["City"], c["Country"]
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override Task Cast_on_top_level_projection_brings_explicit_Cast(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Cast_on_top_level_projection_brings_explicit_Cast(a);

                AssertSql(
                    """
SELECT c["OrderID"]
FROM root c
WHERE (c["Discriminator"] = "Order")
""");
            });

    public override Task Select_anonymous_empty(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_anonymous_empty(a);

                AssertSql(
                    """
SELECT 1
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override Task Select_scalar_primitive(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_scalar_primitive(a);

                AssertSql(
                    """
SELECT c["EmployeeID"]
FROM root c
WHERE (c["Discriminator"] = "Employee")
""");
            });

    public override Task Select_into(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_into(a);

                AssertSql(
                    """
SELECT c["CustomerID"]
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = "ALFKI"))
""");
            });

    public override Task Projection_with_parameterized_constructor_with_member_assignment(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Projection_with_parameterized_constructor_with_member_assignment(a);

                AssertSql(
                    """
SELECT c, c["City"]
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = "ALFKI"))
""");
            });

    public override Task Select_datetime_TimeOfDay_component(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_datetime_TimeOfDay_component(a);

                AssertSql(
                    """
SELECT c["OrderDate"]
FROM root c
WHERE (c["Discriminator"] = "Order")
""");
            });

    public override Task Select_with_complex_expression_that_can_be_funcletized(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_with_complex_expression_that_can_be_funcletized(a);

                AssertSql(
                    """
SELECT INDEX_OF(c["ContactName"], "") AS c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = "ALFKI"))
""");
            });

    public override Task Select_datetime_Ticks_component(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_datetime_Ticks_component(a);

                AssertSql(
                    """
SELECT c["OrderDate"]
FROM root c
WHERE (c["Discriminator"] = "Order")
""");
            });

    public override Task Select_anonymous_literal(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_anonymous_literal(a);

                AssertSql(
                    """
SELECT VALUE {"X" : 10}
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override Task Select_customer_table(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_customer_table(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override Task Select_over_10_nested_ternary_condition(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_over_10_nested_ternary_condition(a);

                AssertSql(
                    """
SELECT VALUE {"c" : ((c["CustomerID"] = "1") ? "01" : ((c["CustomerID"] = "2") ? "02" : ((c["CustomerID"] = "3") ? "03" : ((c["CustomerID"] = "4") ? "04" : ((c["CustomerID"] = "5") ? "05" : ((c["CustomerID"] = "6") ? "06" : ((c["CustomerID"] = "7") ? "07" : ((c["CustomerID"] = "8") ? "08" : ((c["CustomerID"] = "9") ? "09" : ((c["CustomerID"] = "10") ? "10" : ((c["CustomerID"] = "11") ? "11" : null)))))))))))}
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override Task Using_enumerable_parameter_in_projection(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Using_enumerable_parameter_in_projection(a);

                AssertSql(
                    """
SELECT c["CustomerID"]
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND STARTSWITH(c["CustomerID"], "F"))
""");
            });

    [ConditionalTheory(Skip = "Cross collection join Issue#17246")]
    public override Task List_from_result_of_single_result(bool async)
        => base.List_from_result_of_single_result(async);

    [ConditionalTheory(Skip = "Cross collection join Issue#17246")]
    public override Task List_from_result_of_single_result_2(bool async)
        => base.List_from_result_of_single_result_2(async);

    [ConditionalTheory(Skip = "Cross collection join Issue#17246")]
    public override Task List_from_result_of_single_result_3(bool async)
        => base.List_from_result_of_single_result_3(async);

    public override Task Entity_passed_to_DTO_constructor_works(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Entity_passed_to_DTO_constructor_works(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override async Task Set_operation_in_pending_collection(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Set_operation_in_pending_collection(async));

        AssertSql();
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}
