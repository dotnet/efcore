// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit.Sdk;

#pragma warning disable RCS1202 // Avoid NullReferenceException.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class NorthwindMiscellaneousQueryCosmosTest : NorthwindMiscellaneousQueryTestBase<
    NorthwindQueryCosmosFixture<NoopModelCustomizer>>
{
    public NorthwindMiscellaneousQueryCosmosTest(
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
    public virtual Task Simple_IQueryable(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await AssertQuery(a, ss => ss.Set<Customer>());

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override Task Shaper_command_caching_when_parameter_names_different(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Shaper_command_caching_when_parameter_names_different(a);

                AssertSql(
                    """
SELECT COUNT(1) AS c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = "ALFKI"))
""",
                    //
                    """
SELECT COUNT(1) AS c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = "ALFKI"))
""");
            });

    public override async Task Lifting_when_subquery_nested_order_by_anonymous(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Lifting_when_subquery_nested_order_by_anonymous(async));

        AssertSql();
    }

    public override async Task Lifting_when_subquery_nested_order_by_simple(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Lifting_when_subquery_nested_order_by_simple(async));

        AssertSql();
    }

    public override Task Local_dictionary(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Local_dictionary(a);

                AssertSql(
                    """
@__p_0='ALFKI'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = @__p_0))
OFFSET 0 LIMIT 2
""");
            });

    public override Task Entity_equality_self(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Entity_equality_self(a);

                AssertSql(
                    """
SELECT c["CustomerID"]
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = c["CustomerID"]))
""");
            });

    public override Task Entity_equality_local(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Entity_equality_local(a);

                AssertSql(
                    """
@__entity_equality_local_0_CustomerID='ANATR'

SELECT c["CustomerID"]
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = @__entity_equality_local_0_CustomerID))
""");
            });

    public override Task Entity_equality_local_composite_key(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Entity_equality_local_composite_key(a);

                AssertSql(
                    """
@__entity_equality_local_0_OrderID='10248'
@__entity_equality_local_0_ProductID='11'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "OrderDetail") AND ((c["OrderID"] = @__entity_equality_local_0_OrderID) AND (c["ProductID"] = @__entity_equality_local_0_ProductID)))
""");
            });

    public override async Task Join_with_entity_equality_local_on_both_sources(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Join_with_entity_equality_local_on_both_sources(async));

        AssertSql();
    }

    public override Task Entity_equality_local_inline(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Entity_equality_local_inline(a);

                AssertSql(
                    """
SELECT c["CustomerID"]
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = "ANATR"))
""");
            });

    public override Task Entity_equality_local_inline_composite_key(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Entity_equality_local_inline_composite_key(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "OrderDetail") AND ((c["OrderID"] = 10248) AND (c["ProductID"] = 11)))
""");
            });

    public override Task Entity_equality_null(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Entity_equality_null(a);

                AssertSql(
                    """
SELECT c["CustomerID"]
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = null))
""");
            });

    public override Task Entity_equality_not_null(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Entity_equality_not_null(a);

                AssertSql(
                    """
SELECT c["CustomerID"]
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] != null))
""");
            });

    public override async Task Query_when_evaluatable_queryable_method_call_with_repository(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Query_when_evaluatable_queryable_method_call_with_repository(async));

        AssertSql();
    }

    public override async Task Queryable_reprojection(bool async)
    {
        await base.Queryable_reprojection(async);

        AssertSql();
    }

    public override async Task Default_if_empty_top_level(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Default_if_empty_top_level(async));

        AssertSql();
    }

    public override async Task Join_with_default_if_empty_on_both_sources(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Join_with_default_if_empty_on_both_sources(async));

        AssertSql();
    }

    public override async Task Default_if_empty_top_level_followed_by_projecting_constant(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Default_if_empty_top_level_followed_by_projecting_constant(async));

        AssertSql();
    }

    public override async Task Default_if_empty_top_level_positive(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Default_if_empty_top_level_positive(async));

        AssertSql();
    }

    public override async Task Default_if_empty_top_level_arg(bool async)
    {
        await base.Default_if_empty_top_level_arg(async);

        AssertSql();
    }

    public override async Task Default_if_empty_top_level_arg_followed_by_projecting_constant(bool async)
    {
        await base.Default_if_empty_top_level_arg_followed_by_projecting_constant(async);

        AssertSql();
    }

    public override async Task Default_if_empty_top_level_projection(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Default_if_empty_top_level_projection(async));

        AssertSql();
    }

    public override async Task Where_query_composition(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_query_composition(async));

        AssertSql();
    }

    public override async Task Where_query_composition_is_null(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_query_composition_is_null(async));

        AssertSql();
    }

    public override async Task Where_query_composition_is_not_null(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_query_composition_is_not_null(async));

        AssertSql();
    }

    public override async Task Where_query_composition_entity_equality_one_element_SingleOrDefault(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_query_composition_entity_equality_one_element_SingleOrDefault(async));

        AssertSql();
    }

    public override async Task Where_query_composition_entity_equality_one_element_Single(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_query_composition_entity_equality_one_element_Single(async));

        AssertSql();
    }

    public override async Task Where_query_composition_entity_equality_one_element_FirstOrDefault(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_query_composition_entity_equality_one_element_FirstOrDefault(async));

        AssertSql();
    }

    public override async Task Where_query_composition_entity_equality_one_element_First(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_query_composition_entity_equality_one_element_First(async));

        AssertSql();
    }

    public override async Task Where_query_composition_entity_equality_no_elements_SingleOrDefault(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_query_composition_entity_equality_no_elements_SingleOrDefault(async));

        AssertSql();
    }

    public override async Task Where_query_composition_entity_equality_no_elements_Single(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_query_composition_entity_equality_no_elements_Single(async));

        AssertSql();
    }

    public override async Task Where_query_composition_entity_equality_no_elements_FirstOrDefault(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_query_composition_entity_equality_no_elements_FirstOrDefault(async));

        AssertSql();
    }

    public override async Task Where_query_composition_entity_equality_no_elements_First(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_query_composition_entity_equality_no_elements_First(async));

        AssertSql();
    }

    public override async Task Where_query_composition_entity_equality_multiple_elements_SingleOrDefault(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_query_composition_entity_equality_multiple_elements_SingleOrDefault(async));

        AssertSql();
    }

    public override async Task Where_query_composition_entity_equality_multiple_elements_Single(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_query_composition_entity_equality_multiple_elements_Single(async));

        AssertSql();
    }

    public override async Task Where_query_composition_entity_equality_multiple_elements_FirstOrDefault(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_query_composition_entity_equality_multiple_elements_FirstOrDefault(async));

        AssertSql();
    }

    public override async Task Where_query_composition_entity_equality_multiple_elements_First(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_query_composition_entity_equality_multiple_elements_First(async));

        AssertSql();
    }

    public override async Task Where_query_composition2(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_query_composition2(async));

        AssertSql();
    }

    public override async Task Where_query_composition2_FirstOrDefault(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_query_composition2_FirstOrDefault(async));

        AssertSql();
    }

    public override async Task Where_query_composition2_FirstOrDefault_with_anonymous(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_query_composition2_FirstOrDefault_with_anonymous(async));

        AssertSql();
    }

    public override async Task Select_Subquery_Single(bool async)
    {
        // Cross collection join. Issue #17246.
        await AssertTranslationFailed(() => base.Select_Subquery_Single(async));

        AssertSql();
    }

    public override async Task Select_Where_Subquery_Deep_Single(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_Where_Subquery_Deep_Single(async));

        AssertSql();
    }

    public override async Task Select_Where_Subquery_Deep_First(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_Where_Subquery_Deep_First(async));

        AssertSql();
    }

    public override async Task Select_Where_Subquery_Equality(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_Where_Subquery_Equality(async));

        AssertSql();
    }

    public override async Task Where_subquery_anon(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_subquery_anon(async));

        AssertSql();
    }

    public override async Task Where_subquery_anon_nested(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_subquery_anon_nested(async));

        AssertSql();
    }

    public override async Task OrderBy_SelectMany(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.OrderBy_SelectMany(async));

        AssertSql();
    }

    public override async Task Let_any_subquery_anonymous(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Let_any_subquery_anonymous(async));

        AssertSql();
    }

    public override async Task OrderBy_arithmetic(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Cosmos client evaluation. Issue #17246.
            await Assert.ThrowsAsync<CosmosException>(
                async () => await base.OrderBy_arithmetic(async));

            AssertSql(
                """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Employee")
ORDER BY (c["EmployeeID"] - c["EmployeeID"])
""");
        }
    }

    public override async Task OrderBy_condition_comparison(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Cosmos client evaluation. Issue #17246.
            await Assert.ThrowsAsync<CosmosException>(
                async () => await base.OrderBy_condition_comparison(async));

            AssertSql(
                """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Product")
ORDER BY (c["UnitsInStock"] > 0), c["ProductID"]
""");
        }
    }

    public override async Task OrderBy_ternary_conditions(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Cosmos client evaluation. Issue #17246.
            await Assert.ThrowsAsync<CosmosException>(
                async () => await base.OrderBy_ternary_conditions(async));

            AssertSql(
                """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Product")
ORDER BY ((c["UnitsInStock"] > 10) ? (c["ProductID"] > 40) : (c["ProductID"] <= 40)), c["ProductID"]
""");
        }
    }

    public override async Task OrderBy_any(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.OrderBy_any(async));

        AssertSql();
    }

    public override async Task Skip(bool async)
    {
        Assert.Equal(
            CosmosStrings.OffsetRequiresLimit,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Skip_Distinct(async))).Message);

        AssertSql();
    }

    public override async Task Skip_no_orderby(bool async)
    {
        Assert.Equal(
            CosmosStrings.OffsetRequiresLimit,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Skip_no_orderby(async))).Message);

        AssertSql();
    }

    public override Task Skip_Take(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Skip_Take(a);

                AssertSql(
                    """
@__p_0='5'
@__p_1='10'

SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY c["ContactName"]
OFFSET @__p_0 LIMIT @__p_1
""");
            });

    public override async Task Join_Customers_Orders_Skip_Take(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Join_Customers_Orders_Skip_Take(async));

        AssertSql();
    }

    public override async Task Join_Customers_Orders_Skip_Take_followed_by_constant_projection(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Join_Customers_Orders_Skip_Take_followed_by_constant_projection(async));

        AssertSql();
    }

    public override async Task Join_Customers_Orders_Projection_With_String_Concat_Skip_Take(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Join_Customers_Orders_Projection_With_String_Concat_Skip_Take(async));

        AssertSql();
    }

    public override async Task Join_Customers_Orders_Orders_Skip_Take_Same_Properties(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Join_Customers_Orders_Orders_Skip_Take_Same_Properties(async));

        AssertSql();
    }

    public override async Task Take_Skip(bool async)
    {
        // Subquery pushdown. Issue #16156.
        await AssertTranslationFailedWithDetails(
            () => base.Take_Skip(async),
            CosmosStrings.NoSubqueryPushdown);

        AssertSql();
    }

    public override async Task Take_Skip_Distinct(bool async)
    {
        // Subquery pushdown. Issue #16156.
        await AssertTranslationFailedWithDetails(
            () => base.Take_Skip_Distinct(async),
            CosmosStrings.NoSubqueryPushdown);

        AssertSql();
    }

    public override async Task Take_Skip_Distinct_Caching(bool async)
    {
        // Subquery pushdown. Issue #16156.
        await AssertTranslationFailedWithDetails(
            () => base.Take_Skip_Distinct_Caching(async),
            CosmosStrings.NoSubqueryPushdown);

        AssertSql();
    }

    public override async Task Take_Distinct_Count(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Take_Distinct_Count(async));

        AssertSql();
    }

    public override async Task Take_Where_Distinct_Count(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Take_Where_Distinct_Count(async));

        AssertSql();
    }

    public override Task Queryable_simple(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Queryable_simple(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override Task Queryable_simple_anonymous(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Queryable_simple_anonymous(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override Task Queryable_nested_simple(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Queryable_nested_simple(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override Task Queryable_simple_anonymous_projection_subquery(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Queryable_simple_anonymous_projection_subquery(a);

                AssertSql(
                    """
@__p_0='91'

SELECT c["City"]
FROM root c
WHERE (c["Discriminator"] = "Customer")
OFFSET 0 LIMIT @__p_0
""");
            });

    public override Task Queryable_simple_anonymous_subquery(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Queryable_simple_anonymous_subquery(a);

                AssertSql(
                    """
@__p_0='91'

SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
OFFSET 0 LIMIT @__p_0
""");
            });

    public override Task Take_simple(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Take_simple(a);

                AssertSql(
                    """
@__p_0='10'

SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY c["CustomerID"]
OFFSET 0 LIMIT @__p_0
""");
            });

    public override Task Take_simple_parameterized(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Take_simple_parameterized(a);

                AssertSql(
                    """
@__p_0='10'

SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY c["CustomerID"]
OFFSET 0 LIMIT @__p_0
""");
            });

    public override Task Take_simple_projection(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Take_simple_projection(a);

                AssertSql(
                    """
@__p_0='10'

SELECT c["City"]
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY c["CustomerID"]
OFFSET 0 LIMIT @__p_0
""");
            });

    public override Task Take_subquery_projection(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Take_subquery_projection(a);

                AssertSql(
                    """
@__p_0='2'

SELECT c["City"]
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY c["CustomerID"]
OFFSET 0 LIMIT @__p_0
""");
            });

    public override async Task OrderBy_Take_Count(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.OrderBy_Take_Count(async));

        AssertSql();
    }

    public override async Task Take_OrderBy_Count(bool async)
    {
        // Subquery pushdown. Issue #16156.
        await AssertTranslationFailedWithDetails(
            () => base.Take_OrderBy_Count(async),
            CosmosStrings.NoSubqueryPushdown);

        AssertSql();
    }

    public override async Task Any_simple(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Any_simple(async));

        AssertSql();
    }

    public override async Task Any_predicate(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Any_predicate(async));

        AssertSql();
    }

    public override async Task Any_nested_negated(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Any_nested_negated(async));

        AssertSql();
    }

    public override async Task Any_nested_negated2(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Any_nested_negated2(async));

        AssertSql();
    }

    public override async Task Any_nested_negated3(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Any_nested_negated3(async));

        AssertSql();
    }

    public override async Task Any_nested(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Any_nested(async));

        AssertSql();
    }

    public override async Task Any_nested2(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Any_nested2(async));

        AssertSql();
    }

    public override async Task Any_nested3(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Any_nested3(async));

        AssertSql();
    }

    public override async Task Any_with_multiple_conditions_still_uses_exists(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Any_with_multiple_conditions_still_uses_exists(async));

        AssertSql();
    }

    public override async Task All_top_level(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.All_top_level(async));

        AssertSql();
    }

    public override async Task All_top_level_column(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.All_top_level_column(async));

        AssertSql();
    }

    public override async Task All_top_level_subquery(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.All_top_level_subquery(async));

        AssertSql();
    }

    public override async Task All_top_level_subquery_ef_property(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.All_top_level_subquery_ef_property(async));

        AssertSql();
    }

    public override async Task First_client_predicate(bool async)
    {
        await base.First_client_predicate(async);

        AssertSql();
    }

    public override async Task Where_select_many_or(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_select_many_or(async));

        AssertSql();
    }

    public override async Task Where_select_many_or2(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_select_many_or2(async));

        AssertSql();
    }

    public override async Task Where_select_many_or3(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_select_many_or3(async));

        AssertSql();
    }

    public override async Task Where_select_many_or4(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_select_many_or4(async));

        AssertSql();
    }

    public override async Task Where_select_many_or_with_parameter(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_select_many_or_with_parameter(async));

        AssertSql();
    }

    public override async Task SelectMany_mixed(bool async)
    {
        await base.SelectMany_mixed(async);

        AssertSql();
    }

    public override async Task SelectMany_simple_subquery(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.SelectMany_simple_subquery(async));

        AssertSql();
    }

    public override async Task SelectMany_simple1(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.SelectMany_simple1(async));

        AssertSql();
    }

    public override async Task SelectMany_simple2(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.SelectMany_simple2(async));

        AssertSql();
    }

    public override async Task SelectMany_entity_deep(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.SelectMany_entity_deep(async));

        AssertSql();
    }

    public override async Task SelectMany_projection1(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.SelectMany_projection1(async));

        AssertSql();
    }

    public override async Task SelectMany_projection2(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.SelectMany_projection2(async));

        AssertSql();
    }

    public override async Task SelectMany_customer_orders(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.SelectMany_customer_orders(async));

        AssertSql();
    }

    public override async Task SelectMany_Count(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.SelectMany_Count(async));

        AssertSql();
    }

    public override async Task SelectMany_LongCount(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.SelectMany_LongCount(async));

        AssertSql();
    }

    public override async Task SelectMany_OrderBy_ThenBy_Any(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.SelectMany_OrderBy_ThenBy_Any(async));

        AssertSql();
    }

    public override async Task Join_Where_Count(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Join_Where_Count(async));

        AssertSql();
    }

    public override async Task Where_Join_Any(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_Join_Any(async));

        AssertSql();
    }

    public override async Task Where_Join_Exists(bool async)
    {
        await base.Where_Join_Exists(async);

        AssertSql();
    }

    public override async Task Where_Join_Exists_Inequality(bool async)
    {
        await base.Where_Join_Exists_Inequality(async);

        AssertSql();
    }

    public override async Task Where_Join_Exists_Constant(bool async)
    {
        await base.Where_Join_Exists_Constant(async);

        AssertSql();
    }

    public override async Task Where_Join_Not_Exists(bool async)
    {
        await base.Where_Join_Not_Exists(async);

        AssertSql();
    }

    public override async Task Join_OrderBy_Count(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Join_OrderBy_Count(async));

        AssertSql();
    }

    public override async Task Multiple_joins_Where_Order_Any(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Multiple_joins_Where_Order_Any(async));

        AssertSql();
    }

    public override async Task Where_join_select(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_join_select(async));

        AssertSql();
    }

    public override async Task Where_orderby_join_select(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_orderby_join_select(async));

        AssertSql();
    }

    public override async Task Where_join_orderby_join_select(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_join_orderby_join_select(async));

        AssertSql();
    }

    public override async Task Where_select_many(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_select_many(async));

        AssertSql();
    }

    public override async Task Where_orderby_select_many(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_orderby_select_many(async));

        AssertSql();
    }

    public override async Task SelectMany_cartesian_product_with_ordering(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.SelectMany_cartesian_product_with_ordering(async));

        AssertSql();
    }

    public override async Task SelectMany_Joined_DefaultIfEmpty(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.SelectMany_Joined_DefaultIfEmpty(async));

        AssertSql();
    }

    public override async Task SelectMany_Joined_DefaultIfEmpty2(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.SelectMany_Joined_DefaultIfEmpty2(async));

        AssertSql();
    }

    public override async Task SelectMany_Joined_DefaultIfEmpty3(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.SelectMany_Joined_DefaultIfEmpty3(async));

        AssertSql();
    }

    public override async Task SelectMany_Joined(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.SelectMany_Joined(async));

        AssertSql();
    }

    public override async Task SelectMany_Joined_Take(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.SelectMany_Joined_Take(async));

        AssertSql();
    }

    public override async Task Take_with_single(bool async)
    {
        // Subquery pushdown. Issue #16156.
        await AssertTranslationFailedWithDetails(
            () => base.Take_with_single(async),
            CosmosStrings.NoSubqueryPushdown);

        AssertSql();
    }

    public override async Task Take_with_single_select_many(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Take_with_single_select_many(async));

        AssertSql();
    }

    public override async Task Distinct_Skip(bool async)
    {
        // Subquery pushdown. Issue #16156.
        await AssertTranslationFailedWithDetails(
            () => base.Distinct_Skip(async),
            CosmosStrings.NoSubqueryPushdown);

        AssertSql();
    }

    public override async Task Distinct_Skip_Take(bool async)
    {
        // Subquery pushdown. Issue #16156.
        await AssertTranslationFailedWithDetails(
            () => base.Distinct_Skip_Take(async),
            CosmosStrings.NoSubqueryPushdown);

        AssertSql();
    }

    public override async Task Skip_Distinct(bool async)
    {
        Assert.Equal(
            CosmosStrings.OffsetRequiresLimit,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Skip_Distinct(async))).Message);

        AssertSql();
    }

    public override Task Skip_Take_Distinct(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Skip_Take_Distinct(a);

                AssertSql(
                    """
@__p_0='5'
@__p_1='10'

SELECT DISTINCT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY c["ContactName"]
OFFSET @__p_0 LIMIT @__p_1
""");
            });

    public override async Task Skip_Take_Any(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Skip_Take_Any(async));

        AssertSql();
    }

    public override async Task Skip_Take_All(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Skip_Take_All(async));

        AssertSql();
    }

    public override async Task Take_All(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Take_All(async));

        AssertSql();
    }

    public override async Task Skip_Take_Any_with_predicate(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Skip_Take_Any_with_predicate(async));

        AssertSql();
    }

    public override async Task Take_Any_with_predicate(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Take_Any_with_predicate(async));

        AssertSql();
    }

    public override Task OrderBy(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.OrderBy(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY c["CustomerID"]
""");
            });

    public override async Task OrderBy_true(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Cosmos client evaluation. Issue #17246.
            await Assert.ThrowsAsync<CosmosException>(
                async () => await base.OrderBy_true(async));

            AssertSql(
                """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY true
""");
        }
    }

    public override async Task OrderBy_integer(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Cosmos client evaluation. Issue #17246.
            await Assert.ThrowsAsync<CosmosException>(
                async () => await base.OrderBy_integer(async));

            AssertSql(
                """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY 3
""");
        }
    }

    public override async Task OrderBy_parameter(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Cosmos client evaluation. Issue #17246.
            await Assert.ThrowsAsync<CosmosException>(
                async () => await base.OrderBy_parameter(async));

            AssertSql(
                """
@__param_0='5'

SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY @__param_0
""");
        }
    }

    public override Task OrderBy_anon(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.OrderBy_anon(a);

                AssertSql(
                    """
SELECT c["CustomerID"]
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY c["CustomerID"]
""");
            });

    public override Task OrderBy_anon2(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.OrderBy_anon2(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY c["CustomerID"]
""");
            });

    public override async Task OrderBy_client_mixed(bool async)
    {
        await base.OrderBy_client_mixed(async);

        AssertSql();
    }

    public override async Task OrderBy_multiple_queries(bool async)
    {
        await base.OrderBy_multiple_queries(async);

        AssertSql();
    }

    public override Task Take_Distinct(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Take_Distinct(a);

                AssertSql(
                    """
@__p_0='5'

SELECT DISTINCT c
FROM root c
WHERE (c["Discriminator"] = "Order")
ORDER BY c["OrderID"]
OFFSET 0 LIMIT @__p_0
""");
            });

    public override async Task Distinct_Take(bool async)
    {
        // Subquery pushdown. Issue #16156.
        await AssertTranslationFailedWithDetails(
            () => base.Distinct_Take(async),
            CosmosStrings.NoSubqueryPushdown);

        AssertSql();
    }

    public override async Task Distinct_Take_Count(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Distinct_Take_Count(async));

        AssertSql();
    }

    public override async Task OrderBy_shadow(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Cosmos client evaluation. Issue #17246.
            await Assert.ThrowsAsync<CosmosException>(
                async () => await base.OrderBy_shadow(async));

            AssertSql(
                """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Employee")
ORDER BY c["Title"], c["EmployeeID"]
""");
        }
    }

    public override async Task OrderBy_multiple(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Cosmos client evaluation. Issue #17246.
            await Assert.ThrowsAsync<CosmosException>(
                async () => await base.OrderBy_multiple(async));

            AssertSql(
                """
SELECT c["City"]
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND STARTSWITH(c["CustomerID"], "A"))
ORDER BY c["Country"], c["City"]
""");
        }
    }

    public override async Task OrderBy_ThenBy_Any(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.OrderBy_ThenBy_Any(async));

        AssertSql();
    }

    public override async Task OrderBy_correlated_subquery1(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.OrderBy_correlated_subquery1(async));

        AssertSql();
    }

    public override async Task OrderBy_correlated_subquery2(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.OrderBy_correlated_subquery2(async));

        AssertSql();
    }

    public override async Task Where_subquery_recursive_trivial(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_subquery_recursive_trivial(async));

        AssertSql();
    }

    public override async Task Where_query_composition4(bool async)
    {
        await base.Where_query_composition4(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Always does sync evaluation.")]
    public override Task Where_subquery_expression(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                // Cosmos client evaluation. Issue #17246.
                Assert.Equal(
                    CoreStrings.ExpressionParameterizationExceptionSensitive(
                        "value(Microsoft.EntityFrameworkCore.Query.NorthwindMiscellaneousQueryTestBase`1+<>c__DisplayClass107_0[Microsoft.EntityFrameworkCore.Query.NorthwindQueryCosmosFixture`1[Microsoft.EntityFrameworkCore.TestUtilities.NoopModelCustomizer]]).ss.Set().Where(value(Microsoft.EntityFrameworkCore.Query.NorthwindMiscellaneousQueryTestBase`1+<>c__DisplayClass107_0[Microsoft.EntityFrameworkCore.Query.NorthwindQueryCosmosFixture`1[Microsoft.EntityFrameworkCore.TestUtilities.NoopModelCustomizer]]).expr).Any()"),
                    (await Assert.ThrowsAsync<InvalidOperationException>(
                        () => base.Where_subquery_expression(async))).Message);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Order")
OFFSET 0 LIMIT 1
""");
            });

    [ConditionalTheory(Skip = "Always does sync evaluation.")]
    public override async Task Where_subquery_expression_same_parametername(bool async)
    {
        // Always throws
        if (async)
        {
            // Cosmos client evaluation. Issue #17246.
            await AssertTranslationFailed(() => base.Where_subquery_expression_same_parametername(async));

            AssertSql(
                """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Order")
ORDER BY c["OrderID"]
OFFSET 0 LIMIT 1
""");
        }
    }

    public override Task Select_DTO_distinct_translated_to_server(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_DTO_distinct_translated_to_server(a);

                AssertSql(
                    """
SELECT DISTINCT 1
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderID"] < 10300))
""");
            });

    public override Task Select_DTO_constructor_distinct_translated_to_server(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_DTO_constructor_distinct_translated_to_server(a);

                AssertSql(
                    """
SELECT DISTINCT c["CustomerID"]
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderID"] < 10300))
""");
            });

    public override Task Select_DTO_with_member_init_distinct_translated_to_server(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_DTO_with_member_init_distinct_translated_to_server(a);

                AssertSql(
                    """
SELECT DISTINCT VALUE {"Id" : c["CustomerID"], "Count" : c["OrderID"]}
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderID"] < 10300))
""");
            });

    public override async Task Select_nested_collection_count_using_DTO(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_nested_collection_count_using_DTO(async));

        AssertSql();
    }

    public override async Task Select_DTO_with_member_init_distinct_in_subquery_translated_to_server(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_DTO_with_member_init_distinct_in_subquery_translated_to_server(async));

        AssertSql();
    }

    public override async Task Select_DTO_with_member_init_distinct_in_subquery_used_in_projection_translated_to_server(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(
            () => base.Select_DTO_with_member_init_distinct_in_subquery_used_in_projection_translated_to_server(async));

        AssertSql();
    }

    public override async Task Select_correlated_subquery_filtered(bool async)
    {
        // Cross collection join. Issue #17246.
        await AssertTranslationFailed(() => base.Select_correlated_subquery_filtered(async));

        AssertSql();
    }

    public override async Task Select_correlated_subquery_ordered(bool async)
    {
        // Cross collection join. Issue #17246.
        await AssertTranslationFailed(() => base.Select_correlated_subquery_ordered(async));

        AssertSql();
    }

    public override async Task Select_nested_collection_in_anonymous_type(bool async)
    {
        // Cross collection join. Issue #17246.
        await AssertTranslationFailed(() => base.Select_nested_collection_in_anonymous_type(async));

        AssertSql();
    }

    public override async Task Select_subquery_recursive_trivial(bool async)
    {
        // Cross collection join. Issue #17246.
        await AssertTranslationFailed(() => base.Select_subquery_recursive_trivial(async));

        AssertSql();
    }

    public override async Task Where_subquery_on_bool(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_subquery_on_bool(async));

        AssertSql();
    }

    public override async Task Where_subquery_on_collection(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_subquery_on_collection(async));

        AssertSql();
    }

    public override async Task Select_many_cross_join_same_collection(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_many_cross_join_same_collection(async));

        AssertSql();
    }

    public override async Task OrderBy_null_coalesce_operator(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Cosmos client evaluation. Issue #17246.
            await Assert.ThrowsAsync<CosmosException>(
                async () => await base.OrderBy_null_coalesce_operator(async));

            AssertSql(
                """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY ((c["Region"] != null) ? c["Region"] : "ZZ"), c["CustomerID"]
""");
        }
    }

    public override async Task Select_null_coalesce_operator(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Cosmos client evaluation. Issue #17246.
            await Assert.ThrowsAsync<CosmosException>(
                async () => await base.Select_null_coalesce_operator(async));

            AssertSql(
                """
SELECT VALUE {"CustomerID" : c["CustomerID"], "CompanyName" : c["CompanyName"], "Region" : ((c["Region"] != null) ? c["Region"] : "ZZ")}
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY ((c["Region"] != null) ? c["Region"] : "ZZ"), c["CustomerID"]
""");
        }
    }

    public override async Task OrderBy_conditional_operator(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Cosmos client evaluation. Issue #17246.
            await Assert.ThrowsAsync<CosmosException>(
                async () => await base.OrderBy_conditional_operator(async));

            AssertSql(
                """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY ((c["Region"] = null) ? "ZZ" : c["Region"]), c["CustomerID"]
""");
        }
    }

    public override Task OrderBy_conditional_operator_where_condition_false(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.OrderBy_conditional_operator_where_condition_false(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY c["City"]
""");
            });

    public override async Task OrderBy_comparison_operator(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Cosmos client evaluation. Issue #17246.
            await Assert.ThrowsAsync<CosmosException>(
                async () => await base.OrderBy_comparison_operator(async));

            AssertSql(
                """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY (c["Region"] = "ASK")
""");
        }
    }

    public override Task Projection_null_coalesce_operator(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Projection_null_coalesce_operator(a);

                AssertSql(
                    """
SELECT VALUE {"CustomerID" : c["CustomerID"], "CompanyName" : c["CompanyName"], "Region" : ((c["Region"] != null) ? c["Region"] : "ZZ")}
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override Task Filter_coalesce_operator(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Filter_coalesce_operator(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (((c["CompanyName"] != null) ? c["CompanyName"] : c["ContactName"]) = "The Big Cheese"))
""");
            });

    public override async Task Take_skip_null_coalesce_operator(bool async)
    {
        // Subquery pushdown. Issue #16156.
        await AssertTranslationFailedWithDetails(
            () => base.Take_skip_null_coalesce_operator(async),
            CosmosStrings.NoSubqueryPushdown);

        AssertSql();
    }

    public override async Task Select_take_null_coalesce_operator(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Subquery pushdown. Issue #16156.
            await Assert.ThrowsAsync<CosmosException>(
                async () => await base.Select_take_null_coalesce_operator(async));

            AssertSql(
                """
@__p_0='5'

SELECT VALUE {"CustomerID" : c["CustomerID"], "CompanyName" : c["CompanyName"], "Region" : ((c["Region"] != null) ? c["Region"] : "ZZ")}
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY ((c["Region"] != null) ? c["Region"] : "ZZ")
OFFSET 0 LIMIT @__p_0
""");
        }
    }

    public override async Task Select_take_skip_null_coalesce_operator(bool async)
    {
        // Subquery pushdown. Issue #16156.
        await AssertTranslationFailedWithDetails(
            () => base.Select_take_skip_null_coalesce_operator(async),
            CosmosStrings.NoSubqueryPushdown);

        AssertSql();
    }

    public override async Task Select_take_skip_null_coalesce_operator2(bool async)
    {
        // Subquery pushdown. Issue #16156.
        await AssertTranslationFailedWithDetails(
            () => base.Select_take_skip_null_coalesce_operator2(async),
            CosmosStrings.NoSubqueryPushdown);

        AssertSql();
    }

    public override async Task Select_take_skip_null_coalesce_operator3(bool async)
    {
        // Subquery pushdown. Issue #16156.
        await AssertTranslationFailedWithDetails(
            () => base.Select_take_skip_null_coalesce_operator3(async),
            CosmosStrings.NoSubqueryPushdown);

        AssertSql();
    }

    public override async Task Selected_column_can_coalesce(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Unsupported ORDER BY clause.
            await Assert.ThrowsAsync<CosmosException>(
                () => base.Selected_column_can_coalesce(async));

            AssertSql(
                """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY ((c["Region"] != null) ? c["Region"] : "ZZ")
""");
        }
    }

    public override Task DateTime_parse_is_inlined(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.DateTime_parse_is_inlined(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderDate"] > "1998-01-01T12:00:00"))
""");
            });

    public override Task DateTime_parse_is_parameterized_when_from_closure(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.DateTime_parse_is_parameterized_when_from_closure(a);

                AssertSql(
                    """
@__Parse_0='1998-01-01T12:00:00'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderDate"] > @__Parse_0))
""");
            });

    public override Task New_DateTime_is_inlined(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.New_DateTime_is_inlined(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderDate"] > "1998-01-01T12:00:00"))
""");
            });

    public override Task New_DateTime_is_parameterized_when_from_closure(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.New_DateTime_is_parameterized_when_from_closure(a);

                AssertSql(
                    """
@__p_0='1998-01-01T12:00:00'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderDate"] > @__p_0))
""",
                    //
                    """
@__p_0='1998-01-01T11:00:00'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderDate"] > @__p_0))
""");
            });

    public override async Task Random_next_is_not_funcletized_1(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Random_next_is_not_funcletized_1(async));

        AssertSql();
    }

    public override async Task Random_next_is_not_funcletized_2(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Random_next_is_not_funcletized_2(async));

        AssertSql();
    }

    public override async Task Random_next_is_not_funcletized_3(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Random_next_is_not_funcletized_3(async));

        AssertSql();
    }

    public override async Task Random_next_is_not_funcletized_4(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Random_next_is_not_funcletized_4(async));

        AssertSql();
    }

    public override async Task Random_next_is_not_funcletized_5(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Random_next_is_not_funcletized_5(async));

        AssertSql();
    }

    public override async Task Random_next_is_not_funcletized_6(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Random_next_is_not_funcletized_6(async));

        AssertSql();
    }

    public override Task Environment_newline_is_funcletized(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Environment_newline_is_funcletized(a);

                var sql = Fixture.TestSqlLoggerFactory.SqlStatements[0];
                Assert.StartsWith("@__NewLine_0='", sql);
                Assert.EndsWith(
                    """
'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND CONTAINS(c["CustomerID"], @__NewLine_0))
""",
                    sql);
            });

    public override async Task String_concat_with_navigation1(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.String_concat_with_navigation1(async));

        AssertSql();
    }

    public override async Task String_concat_with_navigation2(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.String_concat_with_navigation2(async));

        AssertSql();
    }

    public override async Task Select_bitwise_or(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Bitwise operators on booleans. Issue #13168.
            await Assert.ThrowsAsync<InvalidOperationException>(() => base.Select_bitwise_or(async));

            AssertSql(
                """
SELECT VALUE {"CustomerID" : c["CustomerID"], "Value" : ((c["CustomerID"] = "ALFKI") | (c["CustomerID"] = "ANATR"))}
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY c["CustomerID"]
""");
        }
    }

    public override async Task Select_bitwise_or_multiple(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Bitwise operators on booleans. Issue #13168.
            await Assert.ThrowsAsync<InvalidOperationException>(() => base.Select_bitwise_or_multiple(async));

            AssertSql(
                """
SELECT VALUE {"CustomerID" : c["CustomerID"], "Value" : (((c["CustomerID"] = "ALFKI") | (c["CustomerID"] = "ANATR")) | (c["CustomerID"] = "ANTON"))}
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY c["CustomerID"]
""");
        }
    }

    public override async Task Select_bitwise_and(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Bitwise operators on booleans. Issue #13168.
            await Assert.ThrowsAsync<InvalidOperationException>(() => base.Select_bitwise_and(async));

            AssertSql(
                """
SELECT VALUE {"CustomerID" : c["CustomerID"], "Value" : ((c["CustomerID"] = "ALFKI") & (c["CustomerID"] = "ANATR"))}
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY c["CustomerID"]
""");
        }
    }

    public override async Task Select_bitwise_and_or(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Bitwise operators on booleans. Issue #13168.
            await Assert.ThrowsAsync<InvalidOperationException>(() => base.Select_bitwise_and_or(async));

            AssertSql(
                """
SELECT VALUE {"CustomerID" : c["CustomerID"], "Value" : (((c["CustomerID"] = "ALFKI") & (c["CustomerID"] = "ANATR")) | (c["CustomerID"] = "ANTON"))}
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY c["CustomerID"]
""");
        }
    }

    public override async Task Where_bitwise_or_with_logical_or(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Bitwise operators on booleans. Issue #13168.
            await Assert.ThrowsAsync<EqualException>(() => base.Where_bitwise_or_with_logical_or(async));

            AssertSql(
                """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (((c["CustomerID"] = "ALFKI") | (c["CustomerID"] = "ANATR")) OR (c["CustomerID"] = "ANTON")))
""");
        }
    }

    public override Task Where_bitwise_and_with_logical_and(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_bitwise_and_with_logical_and(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (((c["CustomerID"] = "ALFKI") & (c["CustomerID"] = "ANATR")) AND (c["CustomerID"] = "ANTON")))
""");
            });

    public override async Task Where_bitwise_or_with_logical_and(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Bitwise operators on booleans. Issue #13168.
            await Assert.ThrowsAsync<EqualException>(() => base.Where_bitwise_or_with_logical_and(async));

            AssertSql(
                """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (((c["CustomerID"] = "ALFKI") | (c["CustomerID"] = "ANATR")) AND (c["Country"] = "Germany")))
""");
        }
    }

    public override Task Where_bitwise_and_with_logical_or(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_bitwise_and_with_logical_or(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (((c["CustomerID"] = "ALFKI") & (c["CustomerID"] = "ANATR")) OR (c["CustomerID"] = "ANTON")))
""");
            });

    public override Task Where_bitwise_binary_not(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_bitwise_binary_not(a);

                AssertSql(
                    """
@__negatedId_0='-10249'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (~(c["OrderID"]) = @__negatedId_0))
""");
            });

    public override Task Where_bitwise_binary_and(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_bitwise_binary_and(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND ((c["OrderID"] & 10248) = 10248))
""");
            });

    public override Task Where_bitwise_binary_or(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_bitwise_binary_or(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND ((c["OrderID"] | 10248) = 10248))
""");
            });

    public override async Task Select_bitwise_or_with_logical_or(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Bitwise operators on booleans. Issue #13168.
            await Assert.ThrowsAsync<InvalidOperationException>(() => base.Select_bitwise_or_with_logical_or(async));

            AssertSql(
                """
SELECT VALUE {"CustomerID" : c["CustomerID"], "Value" : (((c["CustomerID"] = "ALFKI") | (c["CustomerID"] = "ANATR")) OR (c["CustomerID"] = "ANTON"))}
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY c["CustomerID"]
""");
        }
    }

    public override async Task Select_bitwise_and_with_logical_and(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Bitwise operators on booleans. Issue #13168.
            await Assert.ThrowsAsync<InvalidOperationException>(() => base.Select_bitwise_and_with_logical_and(async));

            AssertSql(
                """
SELECT VALUE {"CustomerID" : c["CustomerID"], "Value" : (((c["CustomerID"] = "ALFKI") & (c["CustomerID"] = "ANATR")) AND (c["CustomerID"] = "ANTON"))}
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY c["CustomerID"]
""");
        }
    }

    public override async Task Handle_materialization_properly_when_more_than_two_query_sources_are_involved(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Handle_materialization_properly_when_more_than_two_query_sources_are_involved(async));

        AssertSql();
    }

    public override async Task Parameter_extraction_short_circuits_1(bool async)
    {
        // Optimize query SQL. Issue #13159.
        await AssertTranslationFailed(() => base.Parameter_extraction_short_circuits_1(async));

        AssertSql();
    }

    public override async Task Parameter_extraction_short_circuits_2(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Parameter_extraction_short_circuits_2(async));

        AssertSql();
    }

    public override async Task Parameter_extraction_short_circuits_3(bool async)
    {
        // Optimize query SQL. Issue #13159.
        await AssertTranslationFailed(() => base.Parameter_extraction_short_circuits_3(async));

        AssertSql();
    }

    public override async Task Subquery_member_pushdown_does_not_change_original_subquery_model(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Subquery_member_pushdown_does_not_change_original_subquery_model(async));

        AssertSql();
    }

    public override async Task Query_expression_with_to_string_and_contains(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Query_expression_with_to_string_and_contains(async));

        AssertSql();
    }

    public override Task Select_expression_long_to_string(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_expression_long_to_string(a);

                AssertSql(
                    """
SELECT c["OrderID"]
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderDate"] != null))
""");
            });

    public override Task Select_expression_int_to_string(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_expression_int_to_string(a);

                AssertSql(
                    """
SELECT c["OrderID"]
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderDate"] != null))
""");
            });

    public override Task ToString_with_formatter_is_evaluated_on_the_client(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.ToString_with_formatter_is_evaluated_on_the_client(a);

                AssertSql(
                    """
SELECT c["OrderID"]
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderDate"] != null))
""",
                    //
                    """
SELECT c["OrderID"]
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderDate"] != null))
""");
            });

    public override Task Select_expression_other_to_string(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_expression_other_to_string(a);

                AssertSql(
                    """
SELECT c["OrderDate"]
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderDate"] != null))
""");
            });

    public override Task Select_expression_date_add_year(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_expression_date_add_year(a);

                AssertSql(
                    """
SELECT c["OrderDate"]
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderDate"] != null))
""");
            });

    public override Task Select_expression_datetime_add_month(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_expression_datetime_add_month(a);

                AssertSql(
                    """
SELECT c["OrderDate"]
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderDate"] != null))
""");
            });

    public override Task Select_expression_datetime_add_hour(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_expression_datetime_add_hour(a);

                AssertSql(
                    """
SELECT c["OrderDate"]
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderDate"] != null))
""");
            });

    public override Task Select_expression_datetime_add_minute(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_expression_datetime_add_minute(a);

                AssertSql(
                    """
SELECT c["OrderDate"]
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderDate"] != null))
""");
            });

    public override Task Select_expression_datetime_add_second(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_expression_datetime_add_second(a);

                AssertSql(
                    """
SELECT c["OrderDate"]
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderDate"] != null))
""");
            });

    public override Task Select_expression_date_add_milliseconds_above_the_range(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_expression_date_add_milliseconds_above_the_range(a);

                AssertSql(
                    """
SELECT c["OrderDate"]
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderDate"] != null))
""");
            });

    public override Task Select_expression_date_add_milliseconds_below_the_range(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_expression_date_add_milliseconds_below_the_range(a);

                AssertSql(
                    """
SELECT c["OrderDate"]
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderDate"] != null))
""");
            });

    public override Task Select_expression_date_add_milliseconds_large_number_divided(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_expression_date_add_milliseconds_large_number_divided(a);

                AssertSql(
                    """
SELECT c["OrderDate"]
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderDate"] != null))
""");
            });

    public override Task Add_minutes_on_constant_value(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Add_minutes_on_constant_value(a);

                AssertSql(
                    """
SELECT VALUE {"c" : (c["OrderID"] % 25)}
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderID"] < 10500))
ORDER BY c["OrderID"]
""");
            });

    public override async Task Select_expression_references_are_updated_correctly_with_subquery(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_expression_references_are_updated_correctly_with_subquery(async));

        AssertSql();
    }

    public override async Task DefaultIfEmpty_without_group_join(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.DefaultIfEmpty_without_group_join(async));

        AssertSql();
    }

    public override async Task DefaultIfEmpty_in_subquery(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.DefaultIfEmpty_in_subquery(async));

        AssertSql();
    }

    public override async Task DefaultIfEmpty_in_subquery_not_correlated(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.DefaultIfEmpty_in_subquery_not_correlated(async));

        AssertSql();
    }

    public override async Task DefaultIfEmpty_in_subquery_nested(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.DefaultIfEmpty_in_subquery_nested(async));

        AssertSql();
    }

    public override async Task DefaultIfEmpty_in_subquery_nested_filter_order_comparison(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.DefaultIfEmpty_in_subquery_nested_filter_order_comparison(async));

        AssertSql();
    }

    public override async Task OrderBy_skip_take(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Cosmos client evaluation. Issue #17246.
            await Assert.ThrowsAsync<CosmosException>(
                async () => await base.OrderBy_skip_take(async));

            AssertSql(
                """
@__p_0='5'
@__p_1='8'

SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY c["ContactTitle"], c["ContactName"]
OFFSET @__p_0 LIMIT @__p_1
""");
        }
    }

    public override async Task OrderBy_skip_skip_take(bool async)
    {
        // Subquery pushdown. Issue #16156.
        await AssertTranslationFailedWithDetails(
            () => base.OrderBy_skip_skip_take(async),
            CosmosStrings.NoSubqueryPushdown);

        AssertSql();
    }

    public override async Task OrderBy_skip_take_take(bool async)
    {
        // Subquery pushdown. Issue #16156.
        await AssertTranslationFailedWithDetails(
            () => base.OrderBy_skip_take_take(async),
            CosmosStrings.NoSubqueryPushdown);

        AssertSql();
    }

    public override async Task OrderBy_skip_take_take_take_take(bool async)
    {
        // Subquery pushdown. Issue #16156.
        await AssertTranslationFailedWithDetails(
            () => base.OrderBy_skip_take_take_take_take(async),
            CosmosStrings.NoSubqueryPushdown);

        AssertSql();
    }

    public override async Task OrderBy_skip_take_skip_take_skip(bool async)
    {
        // Subquery pushdown. Issue #16156.
        await AssertTranslationFailedWithDetails(
            () => base.OrderBy_skip_take_skip_take_skip(async),
            CosmosStrings.NoSubqueryPushdown);

        AssertSql();
    }

    public override async Task OrderBy_skip_take_distinct(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Cosmos client evaluation. Issue #17246.
            await Assert.ThrowsAsync<CosmosException>(
                async () => await base.OrderBy_skip_take_distinct(async));

            AssertSql(
                """
@__p_0='5'
@__p_1='15'

SELECT DISTINCT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY c["ContactTitle"], c["ContactName"]
OFFSET @__p_0 LIMIT @__p_1
""");
        }
    }

    public override async Task OrderBy_coalesce_take_distinct(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Cosmos client evaluation. Issue #17246.
            await Assert.ThrowsAsync<CosmosException>(
                async () => await base.OrderBy_coalesce_take_distinct(async));

            AssertSql(
                """
@__p_0='15'

SELECT DISTINCT c
FROM root c
WHERE (c["Discriminator"] = "Product")
ORDER BY ((c["UnitPrice"] != null) ? c["UnitPrice"] : 0.0)
OFFSET 0 LIMIT @__p_0
""");
        }
    }

    public override async Task OrderBy_coalesce_skip_take_distinct(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Cosmos client evaluation. Issue #17246.
            await Assert.ThrowsAsync<CosmosException>(
                async () => await base.OrderBy_coalesce_skip_take_distinct(async));

            AssertSql(
                """
@__p_0='5'
@__p_1='15'

SELECT DISTINCT c
FROM root c
WHERE (c["Discriminator"] = "Product")
ORDER BY ((c["UnitPrice"] != null) ? c["UnitPrice"] : 0.0)
OFFSET @__p_0 LIMIT @__p_1
""");
        }
    }

    public override async Task OrderBy_coalesce_skip_take_distinct_take(bool async)
    {
        // Subquery pushdown. Issue #16156.
        await AssertTranslationFailedWithDetails(
            () => base.OrderBy_coalesce_skip_take_distinct_take(async),
            CosmosStrings.NoSubqueryPushdown);

        AssertSql();
    }

    public override async Task OrderBy_skip_take_distinct_orderby_take(bool async)
    {
        // Subquery pushdown. Issue #16156.
        await AssertTranslationFailedWithDetails(
            () => base.OrderBy_skip_take_distinct_orderby_take(async),
            CosmosStrings.NoSubqueryPushdown);

        AssertSql();
    }

    public override async Task No_orderby_added_for_fully_translated_manually_constructed_LOJ(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.No_orderby_added_for_fully_translated_manually_constructed_LOJ(async));

        AssertSql();
    }

    public override async Task No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ(bool async)
    {
        await base.No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ(async);

        AssertSql();
    }

    public override async Task No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition1(
        bool async)
    {
        await base.No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition1(async);

        AssertSql();
    }

    public override async Task No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition2(
        bool async)
    {
        await base.No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition2(async);

        AssertSql();
    }

    public override async Task Orderby_added_for_client_side_GroupJoin_principal_to_dependent_LOJ(bool async)
    {
        await base.Orderby_added_for_client_side_GroupJoin_principal_to_dependent_LOJ(async);

        AssertSql();
    }

    public override async Task Contains_with_DateTime_Date(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Contains_with_DateTime_Date(async));

        AssertSql();
    }

    public override async Task Contains_with_subquery_involving_join_binds_to_correct_table(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Contains_with_subquery_involving_join_binds_to_correct_table(async));

        AssertSql();
    }

    public override async Task Complex_query_with_repeated_query_model_compiles_correctly(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Complex_query_with_repeated_query_model_compiles_correctly(async));

        AssertSql();
    }

    public override async Task Complex_query_with_repeated_nested_query_model_compiles_correctly(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Complex_query_with_repeated_nested_query_model_compiles_correctly(async));

        AssertSql();
    }

    public override Task Anonymous_member_distinct_where(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Anonymous_member_distinct_where(a);

                AssertSql(
                    """
SELECT DISTINCT c["CustomerID"]
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = "ALFKI"))
""");
            });

    public override async Task Anonymous_member_distinct_orderby(bool async)
    {
        // Subquery pushdown. Issue #16156.
        await AssertTranslationFailedWithDetails(
            () => base.Anonymous_member_distinct_orderby(async),
            CosmosStrings.NoSubqueryPushdown);

        AssertSql();
    }

    public override async Task Anonymous_member_distinct_result(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Anonymous_member_distinct_result(async));

        AssertSql();
    }

    public override Task Anonymous_complex_distinct_where(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Anonymous_complex_distinct_where(a);

                AssertSql(
                    """
SELECT DISTINCT VALUE {"A" : (c["CustomerID"] || c["City"])}
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND ((c["CustomerID"] || c["City"]) = "ALFKIBerlin"))
""");
            });

    public override async Task Anonymous_complex_distinct_orderby(bool async)
    {
        // Subquery pushdown. Issue #16156.
        await AssertTranslationFailedWithDetails(
            () => base.Anonymous_complex_distinct_orderby(async),
            CosmosStrings.NoSubqueryPushdown);

        AssertSql();
    }

    public override async Task Anonymous_complex_distinct_result(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Anonymous_complex_distinct_result(async));

        AssertSql();
    }

    public override async Task Anonymous_complex_orderby(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await Assert.ThrowsAsync<CosmosException>(
                async () => await base.Anonymous_complex_orderby(async));

            AssertSql(
                """
SELECT VALUE {"A" : (c["CustomerID"] || c["City"])}
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY (c["CustomerID"] || c["City"])
""");
        }
    }

    public override async Task Anonymous_subquery_orderby(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Anonymous_subquery_orderby(async));

        AssertSql();
    }

    public override Task DTO_member_distinct_where(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.DTO_member_distinct_where(a);

                AssertSql(
                    """
SELECT DISTINCT VALUE {"Property" : c["CustomerID"]}
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = "ALFKI"))
""");
            });

    public override async Task DTO_member_distinct_orderby(bool async)
    {
        // Subquery pushdown. Issue #16156.
        await AssertTranslationFailedWithDetails(
            () => base.DTO_member_distinct_orderby(async),
            CosmosStrings.NoSubqueryPushdown);

        AssertSql();
    }

    public override async Task DTO_member_distinct_result(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.DTO_member_distinct_result(async));

        AssertSql();
    }

    public override Task DTO_complex_distinct_where(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.DTO_complex_distinct_where(a);

                AssertSql(
                    """
SELECT DISTINCT VALUE {"Property" : (c["CustomerID"] || c["City"])}
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND ((c["CustomerID"] || c["City"]) = "ALFKIBerlin"))
""");
            });

    public override async Task DTO_complex_distinct_orderby(bool async)
    {
        // Subquery pushdown. Issue #16156.
        await AssertTranslationFailedWithDetails(
            () => base.DTO_complex_distinct_orderby(async),
            CosmosStrings.NoSubqueryPushdown);

        AssertSql();
    }

    public override async Task DTO_complex_distinct_result(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.DTO_complex_distinct_result(async));

        AssertSql();
    }

    public override async Task DTO_complex_orderby(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Cosmos client evaluation. Issue #17246.
            await Assert.ThrowsAsync<CosmosException>(
                async () => await base.DTO_complex_orderby(async));

            AssertSql(
                """
SELECT VALUE {"Property" : (c["CustomerID"] || c["City"])}
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY (c["CustomerID"] || c["City"])
""");
        }
    }

    public override async Task DTO_subquery_orderby(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.DTO_subquery_orderby(async));

        AssertSql();
    }

    public override async Task Include_with_orderby_skip_preserves_ordering(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        Assert.Equal(
            CosmosStrings.NonEmbeddedIncludeNotSupported(
                "Navigation: Customer.Orders (List<Order>) Collection ToDependent Order Inverse: Customer PropertyAccessMode.Field"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Include_with_orderby_skip_preserves_ordering(async)))
            .Message);

        AssertSql();
    }

    public override Task Int16_parameter_can_be_used_for_int_column(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Int16_parameter_can_be_used_for_int_column(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderID"] = 10300))
""");
            });

    public override async Task Subquery_is_null_translated_correctly(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Subquery_is_null_translated_correctly(async));

        AssertSql();
    }

    public override async Task Subquery_is_not_null_translated_correctly(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Subquery_is_not_null_translated_correctly(async));

        AssertSql();
    }

    public override async Task Select_take_average(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_take_average(async));

        AssertSql();
    }

    public override async Task Select_take_count(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_take_count(async));

        AssertSql();
    }

    public override async Task Select_orderBy_take_count(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_orderBy_take_count(async));

        AssertSql();
    }

    public override async Task Select_take_long_count(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_take_long_count(async));

        AssertSql();
    }

    public override async Task Select_orderBy_take_long_count(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_orderBy_take_long_count(async));

        AssertSql();
    }

    public override async Task Select_take_max(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_take_max(async));

        AssertSql();
    }

    public override async Task Select_take_min(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_take_min(async));

        AssertSql();
    }

    public override async Task Select_take_sum(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_take_sum(async));

        AssertSql();
    }

    public override async Task Select_skip_average(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_skip_average(async));

        AssertSql();
    }

    public override async Task Select_skip_count(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_skip_count(async));

        AssertSql();
    }

    public override async Task Select_orderBy_skip_count(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_orderBy_skip_count(async));

        AssertSql();
    }

    public override async Task Select_skip_long_count(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_skip_long_count(async));

        AssertSql();
    }

    public override async Task Select_orderBy_skip_long_count(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_orderBy_skip_long_count(async));

        AssertSql();
    }

    public override async Task Select_skip_max(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_skip_max(async));

        AssertSql();
    }

    public override async Task Select_skip_min(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_skip_min(async));

        AssertSql();
    }

    public override async Task Select_skip_sum(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_skip_sum(async));

        AssertSql();
    }

    public override async Task Select_distinct_average(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_distinct_average(async));

        AssertSql();
    }

    public override async Task Select_distinct_count(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_distinct_count(async));

        AssertSql();
    }

    public override async Task Select_distinct_long_count(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_distinct_long_count(async));

        AssertSql();
    }

    public override async Task Select_distinct_max(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_distinct_max(async));

        AssertSql();
    }

    public override async Task Select_distinct_min(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_distinct_min(async));

        AssertSql();
    }

    public override async Task Select_distinct_sum(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_distinct_sum(async));

        AssertSql();
    }

    public override Task Comparing_to_fixed_string_parameter(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Comparing_to_fixed_string_parameter(a);

                AssertSql(
                    """
@__prefix_0='A'

SELECT c["CustomerID"]
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND STARTSWITH(c["CustomerID"], @__prefix_0))
""");
            });

    public override async Task Comparing_entities_using_Equals(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Comparing_entities_using_Equals(async));

        AssertSql();
    }

    public override async Task Comparing_different_entity_types_using_Equals(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Comparing_different_entity_types_using_Equals(async));

        AssertSql();
    }

    public override Task Comparing_entity_to_null_using_Equals(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Comparing_entity_to_null_using_Equals(a);

                AssertSql(
                    """
SELECT c["CustomerID"]
FROM root c
WHERE (((c["Discriminator"] = "Customer") AND STARTSWITH(c["CustomerID"], "A")) AND NOT((c["CustomerID"] = null)))
ORDER BY c["CustomerID"]
""");
            });

    public override async Task Comparing_navigations_using_Equals(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Comparing_navigations_using_Equals(async));

        AssertSql();
    }

    public override async Task Comparing_navigations_using_static_Equals(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Comparing_navigations_using_static_Equals(async));

        AssertSql();
    }

    public override async Task Comparing_non_matching_entities_using_Equals(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Comparing_non_matching_entities_using_Equals(async));

        AssertSql();
    }

    public override async Task Comparing_non_matching_collection_navigations_using_Equals(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Comparing_non_matching_collection_navigations_using_Equals(async));

        AssertSql();
    }

    public override Task Comparing_collection_navigation_to_null(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Comparing_collection_navigation_to_null(a);

                AssertSql(
                    """
SELECT c["CustomerID"]
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = null))
""");
            });

    public override async Task Comparing_collection_navigation_to_null_complex(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Comparing_collection_navigation_to_null_complex(async));

        AssertSql();
    }

    public override Task Compare_collection_navigation_with_itself(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Compare_collection_navigation_with_itself(a);

                AssertSql(
                    """
SELECT c["CustomerID"]
FROM root c
WHERE (((c["Discriminator"] = "Customer") AND STARTSWITH(c["CustomerID"], "A")) AND (c["CustomerID"] = c["CustomerID"]))
""");
            });

    public override async Task Compare_two_collection_navigations_with_different_query_sources(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Compare_two_collection_navigations_with_different_query_sources(async));

        AssertSql();
    }

    public override async Task Compare_two_collection_navigations_using_equals(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Compare_two_collection_navigations_using_equals(async));

        AssertSql();
    }

    public override async Task Compare_two_collection_navigations_with_different_property_chains(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Compare_two_collection_navigations_with_different_property_chains(async));

        AssertSql();
    }

    public override Task OrderBy_ThenBy_same_column_different_direction(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.OrderBy_ThenBy_same_column_different_direction(a);

                AssertSql(
                    """
SELECT c["CustomerID"]
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND STARTSWITH(c["CustomerID"], "A"))
ORDER BY c["CustomerID"]
""");
            });

    public override Task OrderBy_OrderBy_same_column_different_direction(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.OrderBy_OrderBy_same_column_different_direction(a);

                AssertSql(
                    """
SELECT c["CustomerID"]
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND STARTSWITH(c["CustomerID"], "A"))
ORDER BY c["CustomerID"] DESC
""");
            });

    public override async Task Complex_nested_query_doesnt_try_binding_to_grandparent_when_parent_returns_complex_result(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(
            () => base.Complex_nested_query_doesnt_try_binding_to_grandparent_when_parent_returns_complex_result(async));

        AssertSql();
    }

    public override async Task Complex_nested_query_properly_binds_to_grandparent_when_parent_returns_scalar_result(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(
            () => base.Complex_nested_query_properly_binds_to_grandparent_when_parent_returns_scalar_result(async));

        AssertSql();
    }

    public override Task OrderBy_Dto_projection_skip_take(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.OrderBy_Dto_projection_skip_take(a);

                AssertSql(
                    """
@__p_0='5'
@__p_1='10'

SELECT VALUE {"Id" : c["CustomerID"]}
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY c["CustomerID"]
OFFSET @__p_0 LIMIT @__p_1
""");
            });

    public override async Task Join_take_count_works(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Join_take_count_works(async));

        AssertSql();
    }

    public override async Task OrderBy_empty_list_contains(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await Assert.ThrowsAsync<CosmosException>(
                async () => await base.OrderBy_empty_list_contains(async));

            AssertSql(
                """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY (true = false)
""");
        }
    }

    public override async Task OrderBy_empty_list_does_not_contains(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await Assert.ThrowsAsync<CosmosException>(
                async () => await base.OrderBy_empty_list_does_not_contains(async));

            AssertSql(
                """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY NOT((true = false))
""");
        }
    }

    public override async Task Manual_expression_tree_typed_null_equality(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Manual_expression_tree_typed_null_equality(async));

        AssertSql();
    }

    public override async Task Let_subquery_with_multiple_occurrences(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Let_subquery_with_multiple_occurrences(async));

        AssertSql();
    }

    public override async Task Let_entity_equality_to_null(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Let_entity_equality_to_null(async));

        AssertSql();
    }

    public override async Task Let_entity_equality_to_other_entity(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Let_entity_equality_to_other_entity(async));

        AssertSql();
    }

    public override async Task SelectMany_after_client_method(bool async)
    {
        await base.SelectMany_after_client_method(async);

        AssertSql();
    }

    public override async Task Collection_navigation_equal_to_null_for_subquery(bool async)
    {
        await AssertTranslationFailed(
            () => base.Collection_navigation_equal_to_null_for_subquery(async));

        AssertSql();
    }

    public override async Task Dependent_to_principal_navigation_equal_to_null_for_subquery(bool async)
    {
        // Left join translation. Issue #17314.
        await AssertTranslationFailed(() => base.Dependent_to_principal_navigation_equal_to_null_for_subquery(async));

        AssertSql();
    }

    public override async Task Collection_navigation_equality_rewrite_for_subquery(bool async)
    {
        await base.Collection_navigation_equality_rewrite_for_subquery(async);

        AssertSql();
    }

    public override async Task Entity_equality_through_nested_anonymous_type_projection(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Entity_equality_through_nested_anonymous_type_projection(async));

        AssertSql();
    }

    public override async Task Entity_equality_through_DTO_projection(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Entity_equality_through_DTO_projection(async));

        AssertSql();
    }

    public override async Task Entity_equality_through_subquery(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Entity_equality_through_subquery(async));

        AssertSql();
    }

    public override Task Can_convert_manually_build_expression_with_default(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Can_convert_manually_build_expression_with_default(a);

                AssertSql(
                    """
SELECT COUNT(1) AS c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] != null))
""",
                    //
                    """
SELECT COUNT(1) AS c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] != null))
""");
            });

    public override async Task Entity_equality_orderby_descending_composite_key(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Cosmos client evaluation. Issue #17246.
            await Assert.ThrowsAsync<CosmosException>(
                async () => await base.Entity_equality_orderby_descending_composite_key(async));

            AssertSql(
                """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "OrderDetail")
ORDER BY c["OrderID"] DESC, c["ProductID"] DESC
""");
        }
    }

    public override async Task Entity_equality_orderby_subquery(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Entity_equality_orderby_subquery(async));

        AssertSql();
    }

    public override async Task Entity_equality_orderby_descending_subquery_composite_key(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Entity_equality_orderby_descending_subquery_composite_key(async));

        AssertSql();
    }

    public override async Task Null_Coalesce_Short_Circuit(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Null_Coalesce_Short_Circuit(async));

        AssertSql();
    }

    public override async Task OrderByDescending_ThenBy(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Cosmos client evaluation. Issue #17246.
            await Assert.ThrowsAsync<CosmosException>(
                async () => await base.OrderByDescending_ThenBy(async));

            AssertSql(
                """
SELECT c["City"]
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY c["CustomerID"] DESC, c["Country"]
""");
        }
    }

    public override async Task OrderByDescending_ThenByDescending(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Cosmos client evaluation. Issue #17246.
            await Assert.ThrowsAsync<CosmosException>(
                async () => await base.OrderByDescending_ThenByDescending(async));

            AssertSql(
                """
SELECT c["City"]
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY c["CustomerID"] DESC, c["Country"] DESC
""");
        }
    }

    public override async Task OrderBy_Join(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.OrderBy_Join(async));

        AssertSql();
    }

    public override async Task OrderBy_ThenBy(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Cosmos client evaluation. Issue #17246.
            await Assert.ThrowsAsync<CosmosException>(
                async () => await base.OrderBy_ThenBy(async));

            AssertSql(
                """
SELECT c["City"]
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY c["CustomerID"], c["Country"]
""");
        }
    }

    public override async Task OrderBy_ThenBy_predicate(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Cosmos client evaluation. Issue #17246.
            await Assert.ThrowsAsync<CosmosException>(
                async () => await base.OrderBy_ThenBy_predicate(async));

            AssertSql(
                """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = "London"))
ORDER BY c["City"], c["CustomerID"]
""");
        }
    }

    public override async Task SelectMany_correlated_simple(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.SelectMany_correlated_simple(async));

        AssertSql();
    }

    public override async Task SelectMany_nested_simple(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.SelectMany_nested_simple(async));

        AssertSql();
    }

    public override async Task SelectMany_primitive(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.SelectMany_primitive(async));

        AssertSql();
    }

    public override async Task SelectMany_primitive_select_subquery(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        Assert.Equal(
            CoreStrings.ExpressionParameterizationExceptionSensitive(
                "value(Microsoft.EntityFrameworkCore.Query.NorthwindMiscellaneousQueryTestBase`1+<>c__DisplayClass169_0[Microsoft.EntityFrameworkCore.Query.NorthwindQueryCosmosFixture`1[Microsoft.EntityFrameworkCore.TestUtilities.NoopModelCustomizer]]).ss.Set().Any()"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.SelectMany_primitive_select_subquery(async))).Message);

        AssertSql();
    }

    public override async Task Select_DTO_constructor_distinct_with_navigation_translated_to_server(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(
            () => base.Select_DTO_constructor_distinct_with_navigation_translated_to_server(async));

        AssertSql();
    }

    public override async Task Select_DTO_constructor_distinct_with_collection_projection_translated_to_server(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(
            () => base.Select_DTO_constructor_distinct_with_collection_projection_translated_to_server(async));

        AssertSql();
    }

    public override async Task
        Select_DTO_constructor_distinct_with_collection_projection_translated_to_server_with_binding_after_client_eval(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(
            () => base
                .Select_DTO_constructor_distinct_with_collection_projection_translated_to_server_with_binding_after_client_eval(async));

        AssertSql();
    }

    public override Task Select_Property_when_shadow_unconstrained_generic_method(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_Property_when_shadow_unconstrained_generic_method(a);

                AssertSql(
                    """
SELECT c["Title"]
FROM root c
WHERE (c["Discriminator"] = "Employee")
""");
            });

    public override async Task Skip_orderby_const(bool async)
    {
        Assert.Equal(
            CosmosStrings.OffsetRequiresLimit,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Skip_orderby_const(async))).Message);

        AssertSql();
    }

    public override Task Where_Property_when_shadow_unconstrained_generic_method(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_Property_when_shadow_unconstrained_generic_method(a);

                AssertSql(
                    """
@__value_0='Sales Representative'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Employee") AND (c["Title"] = @__value_0))
""");
            });

    public override async Task Inner_parameter_in_nested_lambdas_gets_preserved(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Inner_parameter_in_nested_lambdas_gets_preserved(async));

        AssertSql();
    }

    public override async Task Navigation_inside_interpolated_string_is_expanded(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Navigation_inside_interpolated_string_is_expanded(async));

        AssertSql();
    }

    public override async Task Select_DTO_with_member_init_distinct_in_subquery_translated_to_server_2(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Select_DTO_with_member_init_distinct_in_subquery_translated_to_server_2(async));

        AssertSql();
    }

    public override async Task All_client(bool async)
    {
        await base.All_client(async);

        AssertSql();
    }

    public override async Task Client_OrderBy_GroupBy_Group_ordering_works(bool async)
    {
        await base.Client_OrderBy_GroupBy_Group_ordering_works(async);

        AssertSql();
    }

    public override async Task Subquery_member_pushdown_does_not_change_original_subquery_model2(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Subquery_member_pushdown_does_not_change_original_subquery_model2(async));

        AssertSql();
    }

    public override async Task Where_query_composition3(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await base.Where_query_composition3(async);

        AssertSql();
    }

    public override async Task OrderBy_object_type_server_evals(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.OrderBy_object_type_server_evals(async));

        AssertSql();
    }

    public override async Task AsQueryable_in_query_server_evals(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.AsQueryable_in_query_server_evals(async));

        AssertSql();
    }

    public override async Task SelectMany_correlated_subquery_simple(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.SelectMany_correlated_subquery_simple(async));

        AssertSql();
    }

    public override async Task SelectMany_correlated_subquery_hard(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.SelectMany_correlated_subquery_hard(async));

        AssertSql();
    }

    public override async Task Subquery_DefaultIfEmpty_Any(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Subquery_DefaultIfEmpty_Any(async));

        AssertSql();
    }

    public override async Task Projection_skip_collection_projection(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Projection_skip_collection_projection(async));

        AssertSql();
    }

    public override async Task Projection_take_collection_projection(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Projection_take_collection_projection(async));

        AssertSql();
    }

    public override async Task Projection_skip_take_collection_projection(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Projection_skip_take_collection_projection(async));

        AssertSql();
    }

    public override async Task Projection_skip_projection(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Projection_skip_projection(async));

        AssertSql();
    }

    public override async Task Projection_take_projection(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Projection_take_projection(async));

        AssertSql();
    }

    public override async Task Projection_skip_take_projection(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Projection_skip_take_projection(async));

        AssertSql();
    }

    public override async Task Collection_projection_skip(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Collection_projection_skip(async));

        AssertSql();
    }

    public override async Task Collection_projection_take(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Collection_projection_take(async));

        AssertSql();
    }

    public override async Task Collection_projection_skip_take(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Collection_projection_skip_take(async));

        AssertSql();
    }

    public override async Task Anonymous_projection_skip_empty_collection_FirstOrDefault(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Anonymous_projection_skip_empty_collection_FirstOrDefault(async));

        AssertSql();
    }

    public override async Task Anonymous_projection_take_empty_collection_FirstOrDefault(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Anonymous_projection_take_empty_collection_FirstOrDefault(async));

        AssertSql();
    }

    public override async Task Anonymous_projection_skip_take_empty_collection_FirstOrDefault(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Anonymous_projection_skip_take_empty_collection_FirstOrDefault(async));

        AssertSql();
    }

    public override Task Checked_context_with_arithmetic_does_not_fail(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Checked_context_with_arithmetic_does_not_fail(async);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "OrderDetail") AND ((((c["Quantity"] + 1) = 5) AND ((c["Quantity"] - 1) = 3)) AND ((c["Quantity"] * 1) = c["Quantity"])))
ORDER BY c["OrderID"]
""");
            });

    public override Task Checked_context_with_case_to_same_nullable_type_does_not_fail(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Checked_context_with_case_to_same_nullable_type_does_not_fail(async);

                AssertSql(
                    """
SELECT MAX(c["Quantity"]) AS c
FROM root c
WHERE (c["Discriminator"] = "OrderDetail")
""");
            });

    public override Task Entity_equality_with_null_coalesce_client_side(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Entity_equality_with_null_coalesce_client_side(a);

                AssertSql(
                    """
@__entity_equality_a_0_CustomerID='ALFKI'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = @__entity_equality_a_0_CustomerID))
""");
            });

    public override Task Entity_equality_contains_with_list_of_null(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Entity_equality_contains_with_list_of_null(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] IN ("ALFKI") OR (c["CustomerID"] = null)))
""");
            });

    public override async Task Perform_identity_resolution_reuses_same_instances(bool async, bool useAsTracking)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Perform_identity_resolution_reuses_same_instances(async, useAsTracking));

        AssertSql();
    }

    public override async Task Perform_identity_resolution_reuses_same_instances_across_joins(bool async, bool useAsTracking)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Perform_identity_resolution_reuses_same_instances_across_joins(async, useAsTracking));

        AssertSql();
    }

    public override async Task All_client_and_server_top_level(bool async)
    {
        await base.All_client_and_server_top_level(async);

        AssertSql();
    }

    public override async Task All_client_or_server_top_level(bool async)
    {
        await base.All_client_or_server_top_level(async);

        AssertSql();
    }

    public override async Task Single_non_scalar_projection_after_skip_uses_join(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Single_non_scalar_projection_after_skip_uses_join(async));

        AssertSql();
    }

    public override async Task Select_distinct_Select_with_client_bindings(bool async)
    {
        // No Select after Distinct. Issue #17246.
        await AssertTranslationFailed(() => base.Select_distinct_Select_with_client_bindings(async));

        AssertSql();
    }

    public override async Task Pending_selector_in_cardinality_reducing_method_is_applied_before_expanding_collection_navigation_member(
        bool async)
    {
        // Non embedded collection subquery. Issue #17246.
        await AssertTranslationFailed(
            () => base.Pending_selector_in_cardinality_reducing_method_is_applied_before_expanding_collection_navigation_member(async));

        AssertSql();
    }

    public override async Task Max_on_empty_sequence_throws(bool async)
    {
        await AssertTranslationFailed(() => base.Max_on_empty_sequence_throws(async));

        AssertSql();
    }

    public override async Task Distinct_followed_by_ordering_on_condition(bool async)
    {
        // Subquery pushdown. Issue #16156.
        await AssertTranslationFailedWithDetails(
            () => base.Distinct_followed_by_ordering_on_condition(async),
            CosmosStrings.NoSubqueryPushdown);

        AssertSql();
    }

    public override async Task DefaultIfEmpty_Sum_over_collection_navigation(bool async)
    {
        // Non embedded collection subquery. Issue #17246.
        await AssertTranslationFailed(() => base.DefaultIfEmpty_Sum_over_collection_navigation(async));

        AssertSql();
    }

    public override async Task Entity_equality_on_subquery_with_null_check(bool async)
    {
        // Non embedded collection subquery. Issue #17246.
        await AssertTranslationFailed(() => base.Entity_equality_on_subquery_with_null_check(async));

        AssertSql();
    }

    public override async Task DefaultIfEmpty_over_empty_collection_followed_by_projecting_constant(bool async)
    {
        // DefaultIfEmpty. Issue #17246.
        await AssertTranslationFailed(() => base.DefaultIfEmpty_over_empty_collection_followed_by_projecting_constant(async));

        AssertSql();
    }

    public override async Task FirstOrDefault_with_predicate_nested(bool async)
    {
        // Non embedded collection subquery. Issue #17246.
        await AssertTranslationFailed(() => base.FirstOrDefault_with_predicate_nested(async));

        AssertSql();
    }

    public override async Task First_on_collection_in_projection(bool async)
    {
        // Non embedded collection subquery. Issue #17246.
        await AssertTranslationFailed(() => base.First_on_collection_in_projection(async));

        AssertSql();
    }

    public override async Task Skip_0_Take_0_works_when_constant(bool async)
    {
        // Non embedded collection subquery. Issue #17246.
        await AssertTranslationFailed(() => base.Skip_0_Take_0_works_when_constant(async));

        AssertSql();
    }

    public override async Task Skip_1_Take_0_works_when_constant(bool async)
    {
        // Non embedded collection subquery. Issue #17246.
        await AssertTranslationFailed(() => base.Skip_1_Take_0_works_when_constant(async));

        AssertSql();
    }

    public override async Task Take_0_works_when_constant(bool async)
    {
        // Non embedded collection subquery. Issue #17246.
        await AssertTranslationFailed(() => base.Take_0_works_when_constant(async));

        AssertSql();
    }

    public override async Task Using_static_string_Equals_with_StringComparison_throws_informative_error(bool async)
    {
        await AssertTranslationFailedWithDetails(
            () => base.Using_static_string_Equals_with_StringComparison_throws_informative_error(async),
            CoreStrings.QueryUnableToTranslateStringEqualsWithStringComparison);

        AssertSql();
    }

    public override async Task Using_string_Equals_with_StringComparison_throws_informative_error(bool async)
    {
        await AssertTranslationFailedWithDetails(
            () => base.Using_string_Equals_with_StringComparison_throws_informative_error(async),
            CoreStrings.QueryUnableToTranslateStringEqualsWithStringComparison);

        AssertSql();
    }

    public override async Task Select_nested_collection_with_distinct(bool async)
    {
        // Cross collection join. Issue #17246.
        await AssertTranslationFailed(() => base.Select_nested_collection_with_distinct(async));

        AssertSql();
    }

    public override async Task Correlated_collection_with_distinct_without_default_identifiers_projecting_columns(bool async)
    {
        // Cross collection join. Issue #17246.
        await AssertTranslationFailed(() => base.Correlated_collection_with_distinct_without_default_identifiers_projecting_columns(async));

        AssertSql();
    }

    public override async Task Correlated_collection_with_distinct_without_default_identifiers_projecting_columns_with_navigation(
        bool async)
    {
        // Cross collection join. Issue #17246.
        await AssertTranslationFailed(
            () => base.Correlated_collection_with_distinct_without_default_identifiers_projecting_columns_with_navigation(async));

        AssertSql();
    }

    public override async Task Collection_projection_after_DefaultIfEmpty(bool async)
    {
        // Cross collection join. Issue #17246.
        await AssertTranslationFailed(() => base.Collection_projection_after_DefaultIfEmpty(async));

        AssertSql();
    }

    public override Task AsEnumerable_over_string(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.AsEnumerable_over_string(a);

                AssertSql(
                    """
SELECT c["City"]
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY c["CustomerID"]
""");
            });

    public override Task Select_Property_when_non_shadow(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_Property_when_non_shadow(a);

                AssertSql(
                    """
SELECT c["OrderID"]
FROM root c
WHERE (c["Discriminator"] = "Order")
""");
            });

    public override Task Cast_results_to_object(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Cast_results_to_object(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override Task Null_Coalesce_Short_Circuit_with_server_correlated_leftover(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Null_Coalesce_Short_Circuit_with_server_correlated_leftover(a);

                AssertSql(
                    """
SELECT VALUE {"Result" : false}
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override Task Concat_int_string(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Concat_int_string(a);

                AssertSql(
                    """
SELECT c["CustomerID"], c["OrderID"]
FROM root c
WHERE (c["Discriminator"] = "Order")
""");
            });

    public override Task Select_expression_datetime_add_ticks(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_expression_datetime_add_ticks(a);

                AssertSql(
                    """
SELECT c["OrderDate"]
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderDate"] != null))
""");
            });

    public override async Task Throws_on_concurrent_query_first(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await base.Throws_on_concurrent_query_first(async);

            AssertSql(
                """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
        }
    }

    public override Task Entity_equality_through_include(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Entity_equality_through_include(a);

                AssertSql(
                    """
SELECT c["CustomerID"]
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = null))
""");
            });

    public override Task Concat_constant_string_int(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Concat_constant_string_int(a);

                AssertSql(
                    """
SELECT c["OrderID"]
FROM root c
WHERE (c["Discriminator"] = "Order")
""");
            });

    public override Task OrderBy_scalar_primitive(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.OrderBy_scalar_primitive(a);

                AssertSql(
                    """
SELECT c["EmployeeID"]
FROM root c
WHERE (c["Discriminator"] = "Employee")
ORDER BY c["EmployeeID"]
""");
            });

    public override Task Where_Property_when_non_shadow(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_Property_when_non_shadow(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderID"] = 10248))
""");
            });

    public override Task OrderBy_Select(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.OrderBy_Select(a);

                AssertSql(
                    """
SELECT c["ContactName"]
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY c["CustomerID"]
""");
            });

    public override Task Concat_string_int(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Concat_string_int(a);

                AssertSql(
                    """
SELECT c["OrderID"], c["CustomerID"]
FROM root c
WHERE (c["Discriminator"] = "Order")
""");
            });

    public override async Task Non_nullable_property_through_optional_navigation(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await base.Non_nullable_property_through_optional_navigation(async);

            AssertSql(
                """
SELECT LENGTH(c["Region"]) AS Length
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
        }
    }

    public override Task ToList_over_string(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.ToList_over_string(a);

                AssertSql(
                    """
SELECT c["City"]
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY c["CustomerID"]
""");
            });

    public override Task Entity_equality_not_null_composite_key(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Entity_equality_not_null_composite_key(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "OrderDetail") AND ((c["OrderID"] != null) AND (c["ProductID"] != null)))
""");
            });

    public override void Query_composition_against_ienumerable_set()
        => Fixture.NoSyncTest(
            () =>
            {
                base.Query_composition_against_ienumerable_set();
            });

    public override Task ToListAsync_with_canceled_token()
        => Fixture.NoSyncTest(
            true, async _ =>
            {
                await base.ToListAsync_with_canceled_token();

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Employee")
""");
            });

    public override Task Ternary_should_not_evaluate_both_sides(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Ternary_should_not_evaluate_both_sides(a);

                AssertSql(
                    """
SELECT VALUE {"CustomerID" : c["CustomerID"], "Data1" : "none"}
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override Task Entity_equality_orderby(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Entity_equality_orderby(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY c["CustomerID"]
""");
            });

    public override Task Load_should_track_results(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Load_should_track_results(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override Task Null_parameter_name_works(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Null_parameter_name_works(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = null))
""");
            });

    public override Task Where_Property_shadow_closure(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_Property_shadow_closure(a);

                AssertSql(
                    """
@__value_0='Sales Representative'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Employee") AND (c["Title"] = @__value_0))
""",
                    //
                    """
@__value_0='Steven'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Employee") AND (c["FirstName"] = @__value_0))
""");
            });

    public override Task Entity_equality_local_double_check(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Entity_equality_local_double_check(a);

                AssertSql(
                    """
@__entity_equality_local_0_CustomerID='ANATR'

SELECT c["CustomerID"]
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND ((c["CustomerID"] = @__entity_equality_local_0_CustomerID) AND (@__entity_equality_local_0_CustomerID = c["CustomerID"])))
""");
            });

    public override Task ToArray_over_string(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.ToArray_over_string(a);

                AssertSql(
                    """
SELECT c["City"]
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY c["CustomerID"]
""");
            });

    public override Task MemberInitExpression_NewExpression_is_funcletized_even_when_bindings_are_not_evaluatable(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.MemberInitExpression_NewExpression_is_funcletized_even_when_bindings_are_not_evaluatable(a);

                AssertSql(
                    """
SELECT c["CustomerID"]
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND STARTSWITH(c["CustomerID"], "A"))
""");
            });

    public override Task Entity_equality_null_composite_key(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Entity_equality_null_composite_key(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "OrderDetail") AND ((c["OrderID"] = null) OR (c["ProductID"] = null)))
""");
            });

    public override Task Concat_parameter_string_int(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Concat_parameter_string_int(a);

                AssertSql(
                    """
SELECT c["OrderID"]
FROM root c
WHERE (c["Discriminator"] = "Order")
""");
            });

    // ReSharper disable once RedundantOverriddenMember
    public override Task ToListAsync_can_be_canceled()
        // May or may not generate SQL depending on when cancellation happens.
        => base.ToListAsync_can_be_canceled();

    public override Task Where_Property_when_shadow(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_Property_when_shadow(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Employee") AND (c["Title"] = "Sales Representative"))
""");
            });

    public override async Task Throws_on_concurrent_query_list(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await base.Throws_on_concurrent_query_list(async);

            AssertSql(
                """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
        }
    }

    public override Task Convert_to_nullable_on_nullable_value_is_ignored(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Convert_to_nullable_on_nullable_value_is_ignored(a);

                AssertSql(
                    """
SELECT c["OrderDate"]
FROM root c
WHERE (c["Discriminator"] = "Order")
""");
            });

    public override Task Ternary_should_not_evaluate_both_sides_with_parameter(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Ternary_should_not_evaluate_both_sides_with_parameter(a);

                AssertSql(
                    """
SELECT VALUE {"Data1" : true}
FROM root c
WHERE (c["Discriminator"] = "Order")
""");
            });

    public override Task Context_based_client_method(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Context_based_client_method(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
""",
                    //
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override Task OrderByDescending(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.OrderByDescending(a);

                AssertSql(
                    """
SELECT c["City"]
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY c["CustomerID"] DESC
""");
            });

    public override Task Select_Property_when_shadow(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Select_Property_when_shadow(a);

                AssertSql(
                    """
SELECT c["Title"]
FROM root c
WHERE (c["Discriminator"] = "Employee")
""");
            });

    public override Task Skip_0_Take_0_works_when_parameter(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Skip_0_Take_0_works_when_parameter(a);

                AssertSql(
                    """
@__p_0='0'

SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY c["CustomerID"]
OFFSET @__p_0 LIMIT @__p_0
""",
                    //
                    """
@__p_0='1'

SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY c["CustomerID"]
OFFSET @__p_0 LIMIT @__p_0
""");
            });

    public override Task Mixed_sync_async_in_query_cache()
        => Task.CompletedTask; // No sync on Cosmos

    public override async Task Client_code_using_instance_method_throws(bool async)
    {
        Assert.Equal(
            CoreStrings.ClientProjectionCapturingConstantInMethodInstance(
                "Microsoft.EntityFrameworkCore.Query.NorthwindMiscellaneousQueryCosmosTest",
                "InstanceMethod"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Client_code_using_instance_method_throws(async))).Message);

        AssertSql();
    }

    public override async Task Client_code_using_instance_in_static_method(bool async)
    {
        Assert.Equal(
            CoreStrings.ClientProjectionCapturingConstantInMethodArgument(
                "Microsoft.EntityFrameworkCore.Query.NorthwindMiscellaneousQueryCosmosTest",
                "StaticMethod"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Client_code_using_instance_in_static_method(async))).Message);

        AssertSql();
    }

    public override async Task Client_code_using_instance_in_anonymous_type(bool async)
    {
        Assert.Equal(
            CoreStrings.ClientProjectionCapturingConstantInTree(
                "Microsoft.EntityFrameworkCore.Query.NorthwindMiscellaneousQueryCosmosTest"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Client_code_using_instance_in_anonymous_type(async))).Message);

        AssertSql();
    }

    public override async Task Client_code_unknown_method(bool async)
    {
        await AssertTranslationFailedWithDetails(
            () => base.Client_code_unknown_method(async),
            CoreStrings.QueryUnableToTranslateMethod(
                "Microsoft.EntityFrameworkCore.Query.NorthwindMiscellaneousQueryTestBase<Microsoft.EntityFrameworkCore.Query.NorthwindQueryCosmosFixture<Microsoft.EntityFrameworkCore.TestUtilities.NoopModelCustomizer>>",
                nameof(UnknownMethod)));

        AssertSql();
    }

    public override async Task String_include_on_incorrect_property_throws(bool async)
    {
        await base.String_include_on_incorrect_property_throws(async);

        AssertSql();
    }

    public override async Task EF_Property_include_on_incorrect_property_throws(bool async)
    {
        await base.EF_Property_include_on_incorrect_property_throws(async);

        AssertSql();
    }

    public override async Task SkipWhile_throws_meaningful_exception(bool async)
    {
        await base.SkipWhile_throws_meaningful_exception(async);

        AssertSql();
    }

    public override async Task Mixed_sync_async_query()
    {
        await base.Mixed_sync_async_query();

        AssertSql();
    }

    public override async Task Parameter_extraction_can_throw_exception_from_user_code(bool async)
    {
        await base.Parameter_extraction_can_throw_exception_from_user_code(async);

        AssertSql();
    }

    public override async Task Parameter_extraction_can_throw_exception_from_user_code_2(bool async)
    {
        await base.Parameter_extraction_can_throw_exception_from_user_code_2(async);

        AssertSql();
    }

    public override async Task Where_query_composition5(bool async)
    {
        await base.Where_query_composition5(async);

        AssertSql();
    }

    public override async Task Where_query_composition6(bool async)
    {
        await base.Where_query_composition6(async);

        AssertSql();
    }

    public override void Can_cast_CreateQuery_result_to_IQueryable_T_bug_1730()
    {
        base.Can_cast_CreateQuery_result_to_IQueryable_T_bug_1730();

        AssertSql();
    }

    public override Task IQueryable_captured_variable()
        => AssertTranslationFailed(() => base.IQueryable_captured_variable());

    public override async Task Multiple_context_instances(bool async)
    {
        await base.Multiple_context_instances(async);

        AssertSql();
    }

    public override async Task Multiple_context_instances_2(bool async)
    {
        await base.Multiple_context_instances_2(async);

        AssertSql();
    }

    public override async Task Multiple_context_instances_set(bool async)
    {
        await base.Multiple_context_instances_set(async);

        AssertSql();
    }

    public override async Task Multiple_context_instances_parameter(bool async)
    {
        await base.Multiple_context_instances_parameter(async);

        AssertSql();
    }

    public override async Task Entity_equality_through_subquery_composite_key(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Entity_equality_through_subquery_composite_key(async));

        AssertSql();
    }

    public override async Task Select_correlated_subquery_filtered_returning_queryable_throws(bool async)
    {
        await base.Select_correlated_subquery_filtered_returning_queryable_throws(async);

        AssertSql();
    }

    public override async Task Select_correlated_subquery_ordered_returning_queryable_throws(bool async)
    {
        await base.Select_correlated_subquery_ordered_returning_queryable_throws(async);

        AssertSql();
    }

    public override async Task Select_correlated_subquery_ordered_returning_queryable_in_DTO_throws(bool async)
    {
        await base.Select_correlated_subquery_ordered_returning_queryable_in_DTO_throws(async);

        AssertSql();
    }

    public override async Task Select_nested_collection_in_anonymous_type_returning_ordered_queryable(bool async)
    {
        await base.Select_nested_collection_in_anonymous_type_returning_ordered_queryable(async);

        AssertSql();
    }

    public override async Task Select_subquery_recursive_trivial_returning_queryable(bool async)
    {
        await base.Select_subquery_recursive_trivial_returning_queryable(async);

        AssertSql();
    }

    public override async Task Collection_navigation_equal_to_null_for_subquery_using_ElementAtOrDefault_constant_zero(bool async)
    {
        await AssertTranslationFailed(
            () => base.Collection_navigation_equal_to_null_for_subquery_using_ElementAtOrDefault_constant_zero(async));

        AssertSql();
    }

    public override async Task Collection_navigation_equal_to_null_for_subquery_using_ElementAtOrDefault_constant_one(bool async)
    {
        await AssertTranslationFailed(
            () => base.Collection_navigation_equal_to_null_for_subquery_using_ElementAtOrDefault_constant_one(async));

        AssertSql();
    }

    public override async Task Collection_navigation_equal_to_null_for_subquery_using_ElementAtOrDefault_parameter(bool async)
    {
        await AssertTranslationFailed(
            () => base.Collection_navigation_equal_to_null_for_subquery_using_ElementAtOrDefault_parameter(async));

        AssertSql();
    }

    public override async Task Subquery_with_navigation_inside_inline_collection(bool async)
    {
        await AssertTranslationFailed(
            () => base.Subquery_with_navigation_inside_inline_collection(async));

        AssertSql();
    }

    public override async Task Parameter_collection_Contains_with_projection_and_ordering(bool async)
    {
        await AssertTranslationFailed(
            () => base.Parameter_collection_Contains_with_projection_and_ordering(async));

        AssertSql();
    }

    public override Task Contains_over_concatenated_columns_with_different_sizes(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_over_concatenated_columns_with_different_sizes(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] || c["CompanyName"]) IN ("ALFKIAlfreds Futterkiste", "ANATRAna Trujillo Emparedados y helados"))
""");
            });

    public override Task Contains_over_concatenated_column_and_constant(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_over_concatenated_column_and_constant(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] || "SomeConstant") IN ("ALFKISomeConstant", "ANATRSomeConstant", "ALFKIX"))
""");
            });

    public override async Task Contains_over_concatenated_columns_both_fixed_length(bool async)
    {
        await AssertTranslationFailed(
            () => base.Contains_over_concatenated_columns_both_fixed_length(async));

        AssertSql();
    }

    public override Task Contains_over_concatenated_column_and_parameter(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_over_concatenated_column_and_parameter(a);

                AssertSql(
                    """
@__someVariable_0='SomeVariable'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] || @__someVariable_0) IN ("ALFKISomeVariable", "ANATRSomeVariable", "ALFKIX"))
""");
            });

    public override Task Contains_over_concatenated_parameter_and_constant(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_over_concatenated_parameter_and_constant(a);

                AssertSql(
                    """
@__Contains_0='true'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND @__Contains_0)
""");
            });

    public override Task Compiler_generated_local_closure_produces_valid_parameter_name(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Compiler_generated_local_closure_produces_valid_parameter_name(a);

                AssertSql(
                    """
@__customerId_0='ALFKI'
@__details_City_1='Berlin'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND ((c["CustomerID"] = @__customerId_0) AND (c["City"] = @__details_City_1)))
""");
            });

    public override Task Static_member_access_gets_parameterized_within_larger_evaluatable(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Static_member_access_gets_parameterized_within_larger_evaluatable(a);

                AssertSql(
                    """
@__p_0='ALFKI'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = @__p_0))
""");
            });

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}
