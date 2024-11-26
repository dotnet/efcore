// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class NorthwindAggregateOperatorsQueryCosmosTest
    : NorthwindAggregateOperatorsQueryTestBase<NorthwindQueryCosmosFixture<NoopModelCustomizer>>
{
    public NorthwindAggregateOperatorsQueryCosmosTest(
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

    public override Task Average_over_default_returns_default(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Average_over_default_returns_default(a);

                AssertSql(
                    """
SELECT VALUE AVG((c["OrderID"] - 10248))
FROM root c
WHERE ((c["$type"] = "Order") AND (c["OrderID"] = 10248))
""");
            });

    public override async Task Contains_over_keyless_entity_throws(bool async)
    {
        // TODO: #33931
        // The subquery inside the Contains gets executed separately during shaper generation - and synchronously (even in
        // the async variant of the test), but Cosmos doesn't support sync I/O. So both sync and async variants fail because of unsupported
        // sync I/O.
        await CosmosTestHelpers.Instance.NoSyncTest(
            async: false, a => base.Contains_over_keyless_entity_throws(a));

        AssertSql();
    }

    public override Task Contains_with_local_non_primitive_list_closure_mix(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_non_primitive_list_closure_mix(a);

                AssertSql(
                    """
@Select='["ABCDE","ALFKI"]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@Select, c["id"])
""");
            });

    public override Task Contains_with_local_non_primitive_list_inline_closure_mix(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_non_primitive_list_inline_closure_mix(a);

                AssertSql(
                    """
@Select='["ABCDE","ALFKI"]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@Select, c["id"])
""",
                    //
                    """
@Select='["ABCDE","ANATR"]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@Select, c["id"])
""");
            });

    public override Task Count_on_projection_with_client_eval(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Count_on_projection_with_client_eval(a);

                AssertSql(
                    """
SELECT VALUE COUNT(1)
FROM root c
WHERE (c["$type"] = "Order")
""",
                    //
                    """
SELECT VALUE COUNT(1)
FROM root c
WHERE (c["$type"] = "Order")
""",
                    //
                    """
SELECT VALUE COUNT(1)
FROM root c
WHERE (c["$type"] = "Order")
""");
            });

    public override Task First(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.First(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
ORDER BY c["ContactName"]
OFFSET 0 LIMIT 1
""");
            });

    public override Task Max_over_default_returns_default(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Max_over_default_returns_default(a);

                AssertSql(
                    """
SELECT VALUE MAX((c["OrderID"] - 10248))
FROM root c
WHERE ((c["$type"] = "Order") AND (c["OrderID"] = 10248))
""");
            });

    public override Task Min_over_default_returns_default(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Min_over_default_returns_default(a);

                AssertSql(
                    """
SELECT VALUE MIN((c["OrderID"] - 10248))
FROM root c
WHERE ((c["$type"] = "Order") AND (c["OrderID"] = 10248))
""");
            });

    public override Task Sum_over_empty_returns_zero(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Sum_over_empty_returns_zero(a);

                AssertSql(
                    """
SELECT VALUE SUM(c["OrderID"])
FROM root c
WHERE ((c["$type"] = "Order") AND (c["OrderID"] = 42))
""");
            });

    public override Task First_Predicate(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.First_Predicate(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["City"] = "London")
ORDER BY c["ContactName"]
OFFSET 0 LIMIT 1
""");
            });

    public override async Task Single_Throws(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await base.Single_Throws(async);

            AssertSql(
                """
SELECT VALUE c
FROM root c
OFFSET 0 LIMIT 2
""");
        }
    }

    public override Task Where_First(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_First(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["City"] = "London")
ORDER BY c["ContactName"]
OFFSET 0 LIMIT 1
""");
            });

    public override Task Where_Single(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_Single(a);

                AssertSql("ReadItem(None, ALFKI)");
            });

    public override Task FirstOrDefault(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.FirstOrDefault(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
ORDER BY c["ContactName"]
OFFSET 0 LIMIT 1
""");
            });

    public override Task Array_cast_to_IEnumerable_Contains_with_constant(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Array_cast_to_IEnumerable_Contains_with_constant(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE c["id"] IN ("ALFKI", "WRONG")
""");
            });

    public override Task FirstOrDefault_Predicate(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.FirstOrDefault_Predicate(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["City"] = "London")
ORDER BY c["ContactName"]
OFFSET 0 LIMIT 1
""");
            });

    public override Task SingleOrDefault_Predicate(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.SingleOrDefault_Predicate(a);

                AssertSql("ReadItem(None, ALFKI)");
            });

    public override async Task SingleOrDefault_Throws(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await base.SingleOrDefault_Throws(async);

            AssertSql(
                """
SELECT VALUE c
FROM root c
OFFSET 0 LIMIT 2
""");
        }
    }

    public override Task Where_FirstOrDefault(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_FirstOrDefault(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["City"] = "London")
ORDER BY c["ContactName"]
OFFSET 0 LIMIT 1
""");
            });

    public override Task Where_SingleOrDefault(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_SingleOrDefault(a);

                AssertSql("ReadItem(None, ALFKI)");
            });

    public override async Task Select_All(bool async)
    {
        // Always throws sync-not-support for sync.
        if (async)
        {
            // Contains over subquery. Issue #17246.
            await AssertTranslationFailed(() => base.Select_All(async));

            AssertSql();
        }
    }

    public override Task Sum_with_no_arg(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Sum_with_no_arg(a);

                AssertSql(
                    """
SELECT VALUE SUM(c["OrderID"])
FROM root c
WHERE (c["$type"] = "Order")
""");
            });

    public override Task Sum_with_no_data_cast_to_nullable(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Sum_with_no_data_cast_to_nullable(a);

                AssertSql(
                    """
SELECT VALUE SUM(c["OrderID"])
FROM root c
WHERE ((c["$type"] = "Order") AND (c["OrderID"] < 0))
""");
            });

    public override Task Sum_with_binary_expression(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Sum_with_binary_expression(a);

                AssertSql(
                    """
SELECT VALUE SUM((c["OrderID"] * 2))
FROM root c
WHERE (c["$type"] = "Order")
""");
            });

    public override Task Sum_with_no_arg_empty(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Sum_with_no_arg_empty(a);

                AssertSql(
                    """
SELECT VALUE SUM(c["OrderID"])
FROM root c
WHERE ((c["$type"] = "Order") AND (c["OrderID"] = 42))
""");
            });

    public override Task Sum_with_no_data_nullable(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Sum_with_no_data_nullable(a);

                AssertSql(
                    """
SELECT VALUE SUM(c["SupplierID"])
FROM root c
WHERE (c["$type"] = "Product")
""");
            });

    public override Task Sum_with_arg(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Sum_with_arg(a);

                AssertSql(
                    """
SELECT VALUE SUM(c["OrderID"])
FROM root c
WHERE (c["$type"] = "Order")
""");
            });

    public override Task Sum_with_arg_expression(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Sum_with_arg_expression(a);

                AssertSql(
                    """
SELECT VALUE SUM((c["OrderID"] + c["OrderID"]))
FROM root c
WHERE (c["$type"] = "Order")
""");
            });

    public override async Task Sum_with_division_on_decimal(bool async)
    {
        // Aggregate selecting non-mapped type. Issue #20677.
        await Assert.ThrowsAsync<KeyNotFoundException>(async () => await base.Sum_with_division_on_decimal(async));

        AssertSql();
    }

    public override async Task Sum_with_division_on_decimal_no_significant_digits(bool async)
    {
        // Aggregate selecting non-mapped type. Issue #20677.
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await base.Sum_with_division_on_decimal_no_significant_digits(async));

        AssertSql();
    }

    public override Task Sum_with_coalesce(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Sum_with_coalesce(a);

                AssertSql(
                    """
SELECT VALUE SUM(((c["UnitPrice"] != null) ? c["UnitPrice"] : 0.0))
FROM root c
WHERE ((c["$type"] = "Product") AND (c["ProductID"] < 40))
""");
            });

    public override async Task Sum_over_subquery(bool async)
    {
        // Aggregates. Issue #16146.
        await AssertTranslationFailed(() => base.Sum_over_subquery(async));

        AssertSql();
    }

    public override async Task Sum_over_nested_subquery(bool async)
    {
        // Aggregates. Issue #16146.
        await AssertTranslationFailed(() => base.Sum_over_nested_subquery(async));

        AssertSql();
    }

    public override async Task Sum_over_min_subquery(bool async)
    {
        // Aggregates. Issue #16146.
        await AssertTranslationFailed(() => base.Sum_over_min_subquery(async));

        AssertSql();
    }

    public override async Task Sum_over_scalar_returning_subquery(bool async)
    {
        // Aggregates. Issue #16146.
        await AssertTranslationFailed(() => base.Sum_over_scalar_returning_subquery(async));

        AssertSql();
    }

    public override async Task Sum_over_Any_subquery(bool async)
    {
        // Aggregates. Issue #16146.
        await AssertTranslationFailed(() => base.Sum_over_Any_subquery(async));

        AssertSql();
    }

    public override async Task Sum_over_uncorrelated_subquery(bool async)
    {
        // Aggregates. Issue #16146.
        await AssertTranslationFailed(() => base.Sum_over_uncorrelated_subquery(async));

        AssertSql();
    }

    public override Task Sum_on_float_column(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Sum_on_float_column(a);

                AssertSql(
                    """
SELECT VALUE SUM(c["Discount"])
FROM root c
WHERE ((c["$type"] = "OrderDetail") AND (c["ProductID"] = 1))
""");
            });

    public override async Task Sum_on_float_column_in_subquery(bool async)
    {
        // Aggregates. Issue #16146.
        await AssertTranslationFailed(() => base.Sum_on_float_column_in_subquery(async));

        AssertSql();
    }

    public override async Task Average_no_data(bool async)
    {
        // Sync always throws before getting to exception being tested.
        if (async)
        {
            await base.Average_no_data(async);

            AssertSql(
                """
SELECT VALUE AVG(c["OrderID"])
FROM root c
WHERE ((c["$type"] = "Order") AND (c["OrderID"] = -1))
""");
        }
    }

    public override Task Average_no_data_nullable(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Average_no_data_nullable(a);

                AssertSql(
                    """
SELECT VALUE AVG(c["SupplierID"])
FROM root c
WHERE ((c["$type"] = "Product") AND (c["SupplierID"] = -1))
""");
            });

    public override Task Average_no_data_cast_to_nullable(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Average_no_data_cast_to_nullable(a);

                AssertSql(
                    """
SELECT VALUE AVG(c["OrderID"])
FROM root c
WHERE ((c["$type"] = "Order") AND (c["OrderID"] = -1))
""");
            });

    public override async Task Min_no_data(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await base.Min_no_data(async);

            AssertSql(
                """
SELECT VALUE MIN(c["OrderID"])
FROM root c
WHERE ((c["$type"] = "Order") AND (c["OrderID"] = -1))
""");
        }
    }

    public override async Task Max_no_data(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await base.Max_no_data(async);

            AssertSql(
                """
SELECT VALUE MAX(c["OrderID"])
FROM root c
WHERE ((c["$type"] = "Order") AND (c["OrderID"] = -1))
""");
        }
    }

    public override async Task Average_no_data_subquery(bool async)
    {
        // Aggregates. Issue #16146.
        await AssertTranslationFailed(() => base.Average_no_data_subquery(async));

        AssertSql();
    }

    public override async Task Max_no_data_subquery(bool async)
    {
        // Aggregates. Issue #16146.
        await AssertTranslationFailed(() => base.Max_no_data_subquery(async));

        AssertSql();
    }

    public override Task Max_no_data_nullable(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Max_no_data_nullable(a);

                AssertSql(
                    """
SELECT VALUE MAX(c["SupplierID"])
FROM root c
WHERE ((c["$type"] = "Product") AND (c["SupplierID"] = -1))
""");
            });

    public override Task Max_no_data_cast_to_nullable(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Max_no_data_cast_to_nullable(a);

                AssertSql(
                    """
SELECT VALUE MAX(c["OrderID"])
FROM root c
WHERE ((c["$type"] = "Order") AND (c["OrderID"] = -1))
""");
            });

    public override async Task Min_no_data_subquery(bool async)
    {
        // Aggregates. Issue #16146.
        await AssertTranslationFailed(() => base.Min_no_data_subquery(async));

        AssertSql();
    }

    public override Task Average_with_no_arg(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Average_with_no_arg(a);

                AssertSql(
                    """
SELECT VALUE AVG(c["OrderID"])
FROM root c
WHERE (c["$type"] = "Order")
""");
            });

    public override Task Average_with_binary_expression(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Average_with_binary_expression(a);

                AssertSql(
                    """
SELECT VALUE AVG((c["OrderID"] * 2))
FROM root c
WHERE (c["$type"] = "Order")
""");
            });

    public override Task Average_with_arg(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Average_with_arg(a);

                AssertSql(
                    """
SELECT VALUE AVG(c["OrderID"])
FROM root c
WHERE (c["$type"] = "Order")
""");
            });

    public override Task Average_with_arg_expression(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Average_with_arg_expression(a);

                AssertSql(
                    """
SELECT VALUE AVG((c["OrderID"] + c["OrderID"]))
FROM root c
WHERE (c["$type"] = "Order")
""");
            });

    public override async Task Average_with_division_on_decimal(bool async)
    {
        // Aggregate selecting non-mapped type. Issue #20677.
        await Assert.ThrowsAsync<KeyNotFoundException>(async () => await base.Average_with_division_on_decimal(async));

        AssertSql();
    }

    public override async Task Average_with_division_on_decimal_no_significant_digits(bool async)
    {
        // Aggregate selecting non-mapped type. Issue #20677.
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await base.Average_with_division_on_decimal_no_significant_digits(async));

        AssertSql();
    }

    public override Task Average_with_coalesce(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Average_with_coalesce(a);

                AssertSql(
                    """
SELECT VALUE AVG(((c["UnitPrice"] != null) ? c["UnitPrice"] : 0.0))
FROM root c
WHERE ((c["$type"] = "Product") AND (c["ProductID"] < 40))
""");
            });

    public override async Task Average_over_subquery(bool async)
    {
        // Aggregates. Issue #16146.
        await AssertTranslationFailed(() => base.Average_over_subquery(async));

        AssertSql();
    }

    public override async Task Average_over_nested_subquery(bool async)
    {
        // Aggregates. Issue #16146.
        await AssertTranslationFailed(() => base.Average_over_nested_subquery(async));

        AssertSql();
    }

    public override async Task Average_over_max_subquery(bool async)
    {
        // Aggregates. Issue #16146.
        await AssertTranslationFailed(() => base.Average_over_max_subquery(async));

        AssertSql();
    }

    public override Task Average_on_float_column(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Average_on_float_column(a);

                AssertSql(
                    """
SELECT VALUE AVG(c["Discount"])
FROM root c
WHERE ((c["$type"] = "OrderDetail") AND (c["ProductID"] = 1))
""");
            });

    public override async Task Average_on_float_column_in_subquery(bool async)
    {
        // Aggregates. Issue #16146.
        await AssertTranslationFailed(() => base.Average_on_float_column_in_subquery(async));

        AssertSql();
    }

    public override async Task Average_on_float_column_in_subquery_with_cast(bool async)
    {
        // Aggregates. Issue #16146.
        await AssertTranslationFailed(() => base.Average_on_float_column_in_subquery_with_cast(async));

        AssertSql();
    }

    public override Task Min_with_no_arg(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Min_with_no_arg(a);

                AssertSql(
                    """
SELECT VALUE MIN(c["OrderID"])
FROM root c
WHERE (c["$type"] = "Order")
""");
            });

    public override Task Min_with_arg(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Min_with_arg(a);

                AssertSql(
                    """
SELECT VALUE MIN(c["OrderID"])
FROM root c
WHERE (c["$type"] = "Order")
""");
            });

    public override  Task Min_no_data_nullable(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Min_no_data_nullable(a);

                AssertSql(
                    """
SELECT VALUE MIN(c["SupplierID"])
FROM root c
WHERE ((c["$type"] = "Product") AND (c["SupplierID"] = -1))
""");
            });

    public override Task Min_no_data_cast_to_nullable(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Min_no_data_cast_to_nullable(a);

                AssertSql(
                    """
SELECT VALUE MIN(c["OrderID"])
FROM root c
WHERE ((c["$type"] = "Order") AND (c["OrderID"] = -1))
""");
            });

    public override Task Min_with_coalesce(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Min_with_coalesce(a);

                AssertSql(
                    """
SELECT VALUE MIN(((c["UnitPrice"] != null) ? c["UnitPrice"] : 0.0))
FROM root c
WHERE ((c["$type"] = "Product") AND (c["ProductID"] < 40))
""");
            });

    public override async Task Min_over_subquery(bool async)
    {
        // Aggregates. Issue #16146.
        await AssertTranslationFailed(() => base.Min_over_subquery(async));

        AssertSql();
    }

    public override async Task Min_over_nested_subquery(bool async)
    {
        // Aggregates. Issue #16146.
        await AssertTranslationFailed(() => base.Min_over_nested_subquery(async));

        AssertSql();
    }

    public override async Task Min_over_max_subquery(bool async)
    {
        // Aggregates. Issue #16146.
        await AssertTranslationFailed(() => base.Min_over_max_subquery(async));

        AssertSql();
    }

    public override Task Max_with_no_arg(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Max_with_no_arg(a);

                AssertSql(
                    """
SELECT VALUE MAX(c["OrderID"])
FROM root c
WHERE (c["$type"] = "Order")
""");
            });

    public override Task Max_with_arg(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Max_with_arg(a);

                AssertSql(
                    """
SELECT VALUE MAX(c["OrderID"])
FROM root c
WHERE (c["$type"] = "Order")
""");
            });

    public override Task Max_with_coalesce(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Max_with_coalesce(a);

                AssertSql(
                    """
SELECT VALUE MAX(((c["UnitPrice"] != null) ? c["UnitPrice"] : 0.0))
FROM root c
WHERE ((c["$type"] = "Product") AND (c["ProductID"] < 40))
""");
            });

    public override async Task Max_over_subquery(bool async)
    {
        // Aggregates. Issue #16146.
        await AssertTranslationFailed(() => base.Max_over_subquery(async));

        AssertSql();
    }

    public override async Task Max_over_nested_subquery(bool async)
    {
        // Aggregates. Issue #16146.
        await AssertTranslationFailed(() => base.Max_over_nested_subquery(async));

        AssertSql();
    }

    public override async Task Max_over_sum_subquery(bool async)
    {
        // Aggregates. Issue #16146.
        await AssertTranslationFailed(() => base.Max_over_sum_subquery(async));

        AssertSql();
    }

    public override Task Count_with_no_predicate(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Count_with_no_predicate(a);

                AssertSql(
                    """
SELECT VALUE COUNT(1)
FROM root c
WHERE (c["$type"] = "Order")
""");
            });

    public override Task Count_with_predicate(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Count_with_predicate(a);

                AssertSql(
                    """
SELECT VALUE COUNT(1)
FROM root c
WHERE ((c["$type"] = "Order") AND (c["CustomerID"] = "ALFKI"))
""");
            });

    public override Task Count_with_order_by(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Count_with_order_by(a);

                AssertSql(
                    """
SELECT VALUE COUNT(1)
FROM root c
WHERE (c["$type"] = "Order")
""");
            });

    public override Task Where_OrderBy_Count(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_OrderBy_Count(a);

                AssertSql(
                    """
SELECT VALUE COUNT(1)
FROM root c
WHERE ((c["$type"] = "Order") AND (c["CustomerID"] = "ALFKI"))
""");
            });

    public override Task OrderBy_Where_Count(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.OrderBy_Where_Count(a);

                AssertSql(
                    """
SELECT VALUE COUNT(1)
FROM root c
WHERE ((c["$type"] = "Order") AND (c["CustomerID"] = "ALFKI"))
""");
            });

    public override Task OrderBy_Count_with_predicate(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.OrderBy_Count_with_predicate(a);

                AssertSql(
                    """
SELECT VALUE COUNT(1)
FROM root c
WHERE ((c["$type"] = "Order") AND (c["CustomerID"] = "ALFKI"))
""");
            });

    public override Task OrderBy_Where_Count_with_predicate(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.OrderBy_Where_Count_with_predicate(a);

                AssertSql(
                    """
SELECT VALUE COUNT(1)
FROM root c
WHERE (((c["$type"] = "Order") AND (c["OrderID"] > 10)) AND (c["CustomerID"] != "ALFKI"))
""");
            });

    public override async Task Where_OrderBy_Count_client_eval(bool async)
    {
        await base.Where_OrderBy_Count_client_eval(async);

        AssertSql();
    }

    public override async Task OrderBy_Where_Count_client_eval(bool async)
    {
        await base.OrderBy_Where_Count_client_eval(async);

        AssertSql();
    }

    public override async Task OrderBy_Where_Count_client_eval_mixed(bool async)
    {
        await base.OrderBy_Where_Count_client_eval_mixed(async);

        AssertSql();
    }

    public override async Task OrderBy_Count_with_predicate_client_eval(bool async)
    {
        await base.OrderBy_Count_with_predicate_client_eval(async);

        AssertSql();
    }

    public override async Task OrderBy_Count_with_predicate_client_eval_mixed(bool async)
    {
        await base.OrderBy_Count_with_predicate_client_eval_mixed(async);

        AssertSql();
    }

    public override async Task OrderBy_Where_Count_with_predicate_client_eval(bool async)
    {
        await base.OrderBy_Where_Count_with_predicate_client_eval(async);

        AssertSql();
    }

    public override async Task OrderBy_Where_Count_with_predicate_client_eval_mixed(bool async)
    {
        await base.OrderBy_Where_Count_with_predicate_client_eval_mixed(async);

        AssertSql();
    }

    public override async Task Average_on_nav_subquery_in_projection(bool async)
    {
        // Aggregates. Issue #16146.
        await AssertTranslationFailed(() => base.Average_on_nav_subquery_in_projection(async));

        AssertSql();
    }

    public override async Task Count_after_client_projection(bool async)
    {
        // Aggregates. Issue #16146.
        await AssertTranslationFailed(() => base.Count_after_client_projection(async));

        AssertSql();
    }

    public override async Task OrderBy_client_Take(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await Assert.ThrowsAsync<CosmosException>(
                async () => await base.OrderBy_client_Take(async));

            AssertSql(
                """
@p='10'

SELECT VALUE c
FROM root c
ORDER BY 42
OFFSET 0 LIMIT @p
""");
        }
    }

    public override Task Distinct(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Distinct(a);

                AssertSql(
                    """
SELECT DISTINCT VALUE c
FROM root c
""");
            });

    [ConditionalTheory(Skip = "Fails on CI #27688")]
    public override Task Distinct_Scalar(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Distinct_Scalar(a);

                AssertSql(
                    """
SELECT DISTINCT c[""City""]
FROM root c
WHERE (c[""$type""] = ""Customer"")
""");
            });

    [ConditionalTheory(Skip = "Fails on emulator https://github.com/Azure/azure-cosmos-dotnet-v3/issues/4339")]
    public override Task OrderBy_Distinct(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.OrderBy_Distinct(a);

                AssertSql(
                    """
SELECT DISTINCT c["City"]
FROM root c
WHERE (c["$type"] = "Customer")
ORDER BY c["id"]
""");
            });

    public override async Task Distinct_OrderBy(bool async)
        // Subquery pushdown. Issue #16156.
        => await AssertTranslationFailedWithDetails(
            () => base.Distinct_OrderBy(async),
            CosmosStrings.NoSubqueryPushdown);

    public override async Task Distinct_OrderBy2(bool async)
        // Subquery pushdown. Issue #16156.
        => await AssertTranslationFailedWithDetails(
            () => base.Distinct_OrderBy2(async),
            CosmosStrings.NoSubqueryPushdown);

    public override async Task Distinct_OrderBy3(bool async)
    {
        // Subquery pushdown. Issue #16156.
        await AssertTranslationFailedWithDetails(
            () => base.Distinct_OrderBy(async),
            CosmosStrings.NoSubqueryPushdown);

        AssertSql();
    }

    public override async Task Distinct_Count(bool async)
    {
        // Aggregates. Issue #16146.
        await AssertTranslationFailed(() => base.Distinct_Count(async));

        AssertSql();
    }

    public override async Task Select_Select_Distinct_Count(bool async)
    {
        // Aggregates. Issue #16146.
        await AssertTranslationFailed(() => base.Select_Select_Distinct_Count(async));

        AssertSql();
    }

    public override Task Single_Predicate(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Single_Predicate(a);

                AssertSql("ReadItem(None, ALFKI)");
            });

    public override async Task FirstOrDefault_inside_subquery_gets_server_evaluated(bool async)
    {
        // Contains over subquery. Issue #17246.
        await AssertTranslationFailed(() => base.FirstOrDefault_inside_subquery_gets_server_evaluated(async));

        AssertSql();
    }

    public override async Task Multiple_collection_navigation_with_FirstOrDefault_chained_projecting_scalar(bool async)
    {
        // Contains over subquery. Issue #17246.
        await AssertTranslationFailed(() => base.Multiple_collection_navigation_with_FirstOrDefault_chained_projecting_scalar(async));

        AssertSql();
    }

    public override async Task First_inside_subquery_gets_client_evaluated(bool async)
    {
        // Contains over subquery. Issue #17246.
        await AssertTranslationFailed(() => base.First_inside_subquery_gets_client_evaluated(async));

        AssertSql();
    }

    public override Task Last(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Last(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
ORDER BY c["ContactName"] DESC
OFFSET 0 LIMIT 1
""");
            });

    public override Task Last_when_no_order_by(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Last_when_no_order_by(a);

                AssertSql("ReadItem(None, ALFKI)");
            });

    public override Task LastOrDefault_when_no_order_by(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.LastOrDefault_when_no_order_by(a);

                AssertSql("ReadItem(None, ALFKI)");
            });

    public override Task Last_Predicate(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Last_Predicate(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["City"] = "London")
ORDER BY c["ContactName"] DESC
OFFSET 0 LIMIT 1
""");
            });

    public override Task Where_Last(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_Last(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["City"] = "London")
ORDER BY c["ContactName"] DESC
OFFSET 0 LIMIT 1
""");
            });

    public override Task LastOrDefault(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.LastOrDefault(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
ORDER BY c["ContactName"] DESC
OFFSET 0 LIMIT 1
""");
            });

    public override Task LastOrDefault_Predicate(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.LastOrDefault_Predicate(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["City"] = "London")
ORDER BY c["ContactName"] DESC
OFFSET 0 LIMIT 1
""");
            });

    public override Task Where_LastOrDefault(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_LastOrDefault(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["City"] = "London")
ORDER BY c["ContactName"] DESC
OFFSET 0 LIMIT 1
""");
            });

    public override async Task Contains_with_subquery(bool async)
    {
        // Contains over subquery. Issue #17246.
        await AssertTranslationFailed(() => base.Contains_with_subquery(async));

        AssertSql();
    }

    public override Task Contains_with_local_array_closure(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_array_closure(a);

                AssertSql(
                    """
@ids='["ABCDE","ALFKI"]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@ids, c["id"])
""",
                    //
                    """
@ids='["ABCDE"]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@ids, c["id"])
""");
            });

    public override async Task Contains_with_subquery_and_local_array_closure(bool async)
    {
        // Contains over subquery. Issue #17246.
        await AssertTranslationFailed(() => base.Contains_with_subquery_and_local_array_closure(async));

        AssertSql();
    }

    public override Task Contains_with_local_uint_array_closure(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_uint_array_closure(a);

                AssertSql(
                    """
@ids='[0,1]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@ids, c["EmployeeID"])
""",
                    //
                    """
@ids='[0]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@ids, c["EmployeeID"])
""");
            });

    public override Task Contains_with_local_nullable_uint_array_closure(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_nullable_uint_array_closure(a);

                AssertSql(
                    """
@ids='[0,1]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@ids, c["EmployeeID"])
""",
                    //
                    """
@ids='[0]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@ids, c["EmployeeID"])
""");
            });

    public override Task Contains_with_local_array_inline(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_array_inline(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE c["id"] IN ("ABCDE", "ALFKI")
""");
            });

    public override Task Contains_with_local_list_closure(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_list_closure(a);

                AssertSql(
                    """
@ids='["ABCDE","ALFKI"]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@ids, c["id"])
""");
            });

    public override Task Contains_with_local_object_list_closure(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_object_list_closure(a);

                AssertSql(
                    """
@ids='["ABCDE","ALFKI"]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@ids, c["id"])
""");
            });

    public override Task Contains_with_local_list_closure_all_null(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_list_closure_all_null(a);

                AssertSql(
                    """
@ids='[null,null]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@ids, c["id"])
""");
            });

    public override Task Contains_with_local_list_inline(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_list_inline(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE c["id"] IN ("ABCDE", "ALFKI")
""");
            });

    public override Task Contains_with_local_list_inline_closure_mix(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_list_inline_closure_mix(a);

                AssertSql(
                    """
@p='["ABCDE","ALFKI"]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@p, c["id"])
""",
                    //
                    """
@p='["ABCDE","ANATR"]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@p, c["id"])
""");
            });

    public override Task Contains_with_local_enumerable_closure(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_enumerable_closure(a);
                AssertSql(
                    """
@ids='["ABCDE","ALFKI"]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@ids, c["id"])
""",
                    //
                    """
@ids='["ABCDE"]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@ids, c["id"])
""");
            });

    public override Task Contains_with_local_object_enumerable_closure(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_object_enumerable_closure(a);

                AssertSql(
                    """
@ids='["ABCDE","ALFKI"]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@ids, c["id"])
""");
            });

    public override Task Contains_with_local_enumerable_closure_all_null(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_enumerable_closure_all_null(a);

                AssertSql(
                    """
@ids='[]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@ids, c["id"])
""");
            });

    public override Task Contains_with_local_enumerable_inline(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_enumerable_inline(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE EXISTS (
    SELECT 1
    FROM a IN (SELECT VALUE ["ABCDE", "ALFKI"])
    WHERE ((a != null) AND (a = c["id"])))
""");
            });

    public override Task Contains_with_local_enumerable_inline_closure_mix(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_enumerable_inline_closure_mix(a);

                AssertSql(
                    """
@p='["ABCDE","ALFKI"]'

SELECT VALUE c
FROM root c
WHERE EXISTS (
    SELECT 1
    FROM p IN (SELECT VALUE @p)
    WHERE ((p != null) AND (p = c["id"])))
""",
                    //
                    """
@p='["ABCDE","ANATR"]'

SELECT VALUE c
FROM root c
WHERE EXISTS (
    SELECT 1
    FROM p IN (SELECT VALUE @p)
    WHERE ((p != null) AND (p = c["id"])))
""");
            });

    public override Task Contains_with_local_ordered_enumerable_closure(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_ordered_enumerable_closure(a);

                AssertSql(
                    """
@ids='["ABCDE","ALFKI"]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@ids, c["id"])
""",
                    //
                    """
@ids='["ABCDE"]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@ids, c["id"])
""");
            });

    public override Task Contains_with_local_object_ordered_enumerable_closure(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_object_ordered_enumerable_closure(a);

                AssertSql(
                    """
@ids='["ABCDE","ALFKI"]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@ids, c["id"])
""");
            });

    public override Task Contains_with_local_ordered_enumerable_closure_all_null(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_ordered_enumerable_closure_all_null(a);

                AssertSql(
                    """
@ids='[null,null]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@ids, c["id"])
""");
            });

    public override Task Contains_with_local_ordered_enumerable_inline(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_ordered_enumerable_inline(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE c["id"] IN ("ABCDE", "ALFKI")
""");
            });

    public override Task Contains_with_local_ordered_enumerable_inline_closure_mix(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_ordered_enumerable_inline_closure_mix(a);

                AssertSql(
                    """
@Order='["ABCDE","ALFKI"]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@Order, c["id"])
""",
                    //
                    """
@Order='["ABCDE","ANATR"]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@Order, c["id"])
""");
            });

    public override Task Contains_with_local_read_only_collection_closure(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_read_only_collection_closure(a);

                AssertSql(
                    """
@ids='["ABCDE","ALFKI"]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@ids, c["id"])
""",
                    //
                    """
@ids='["ABCDE"]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@ids, c["id"])
""");
            });

    public override Task Contains_with_local_object_read_only_collection_closure(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_object_read_only_collection_closure(a);

                AssertSql(
                    """
@ids='["ABCDE","ALFKI"]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@ids, c["id"])
""");
            });

    public override Task Contains_with_local_ordered_read_only_collection_all_null(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_ordered_read_only_collection_all_null(a);

                AssertSql(
                    """
@ids='[null,null]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@ids, c["id"])
""");
            });

    public override Task Contains_with_local_read_only_collection_inline(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_read_only_collection_inline(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE c["id"] IN ("ABCDE", "ALFKI")
""");
            });

    public override Task Contains_with_local_read_only_collection_inline_closure_mix(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_read_only_collection_inline_closure_mix(a);

                AssertSql(
                    """
@AsReadOnly='["ABCDE","ALFKI"]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@AsReadOnly, c["id"])
""",
                    //
                    """
@AsReadOnly='["ABCDE","ANATR"]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@AsReadOnly, c["id"])
""");
            });

    public override Task Contains_with_local_collection_false(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_collection_false(a);

                AssertSql(
                    """
@ids='["ABCDE","ALFKI"]'

SELECT VALUE c
FROM root c
WHERE NOT(ARRAY_CONTAINS(@ids, c["id"]))
""");
            });

    public override Task Contains_with_local_collection_complex_predicate_and(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_collection_complex_predicate_and(a);

                AssertSql(
                    """
@ids='["ABCDE","ALFKI"]'

SELECT VALUE c
FROM root c
WHERE (((c["id"] = "ALFKI") OR (c["id"] = "ABCDE")) AND ARRAY_CONTAINS(@ids, c["id"]))
""");
            });

    public override Task Contains_with_local_collection_complex_predicate_or(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_collection_complex_predicate_or(a);

                AssertSql(
                    """
@ids='["ABCDE","ALFKI"]'

SELECT VALUE c
FROM root c
WHERE (ARRAY_CONTAINS(@ids, c["id"]) OR ((c["id"] = "ALFKI") OR (c["id"] = "ABCDE")))
""");
            });

    public override Task Contains_with_local_collection_complex_predicate_not_matching_ins1(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_collection_complex_predicate_not_matching_ins1(a);

                AssertSql(
                    """
@ids='["ABCDE","ALFKI"]'

SELECT VALUE c
FROM root c
WHERE (((c["id"] = "ALFKI") OR (c["id"] = "ABCDE")) OR NOT(ARRAY_CONTAINS(@ids, c["id"])))
""");
            });

    public override Task Contains_with_local_collection_complex_predicate_not_matching_ins2(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_collection_complex_predicate_not_matching_ins2(a);

                AssertSql(
                    """
@ids='["ABCDE","ALFKI"]'

SELECT VALUE c
FROM root c
WHERE (ARRAY_CONTAINS(@ids, c["id"]) AND ((c["id"] != "ALFKI") AND (c["id"] != "ABCDE")))
""");
            });

    public override Task Contains_with_local_collection_sql_injection(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_collection_sql_injection(a);

                AssertSql(
                    """
@ids='["ALFKI","ABC')); GO; DROP TABLE Orders; GO; --"]'

SELECT VALUE c
FROM root c
WHERE (ARRAY_CONTAINS(@ids, c["id"]) OR ((c["id"] = "ALFKI") OR (c["id"] = "ABCDE")))
""");
            });

    public override Task Contains_with_local_collection_empty_closure(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_collection_empty_closure(a);

                AssertSql(
                    """
@ids='[]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@ids, c["id"])
""");
            });

    public override Task Contains_with_local_collection_empty_inline(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_collection_empty_inline(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE NOT(false)
""");
            });

    public override async Task Contains_top_level(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Top-level Any(), see #33854.
            var exception = await Assert.ThrowsAsync<CosmosException>(() => base.Contains_top_level(async));

            Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);

            AssertSql(
                """
@p='ALFKI'

SELECT VALUE EXISTS (
    SELECT 1
    FROM root c
    WHERE (c["id"] = @p))
""");
        }
    }

    public override async Task Contains_with_local_tuple_array_closure(bool async)
    {
        // Contains over subquery. Issue #17246.
        await AssertTranslationFailed(() => base.Contains_with_local_tuple_array_closure(async));

        AssertSql();
    }

    public override async Task Contains_with_local_anonymous_type_array_closure(bool async)
    {
        // Contains over subquery. Issue #17246.
        await AssertTranslationFailed(() => base.Contains_with_local_anonymous_type_array_closure(async));

        AssertSql();
    }

    public override async Task OfType_Select(bool async)
    {
        // Contains over subquery. Issue #17246.
        await AssertTranslationFailed(() => base.OfType_Select(async));

        AssertSql();
    }

    public override async Task OfType_Select_OfType_Select(bool async)
    {
        // Contains over subquery. Issue #15937.
        await AssertTranslationFailed(() => base.OfType_Select_OfType_Select(async));

        AssertSql();
    }

    public override async Task Average_with_non_matching_types_in_projection_doesnt_produce_second_explicit_cast(bool async)
    {
        // Aggregate selecting non-mapped type. Issue #20677.
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await base.Average_with_non_matching_types_in_projection_doesnt_produce_second_explicit_cast(async));

        AssertSql();
    }

    public override async Task Max_with_non_matching_types_in_projection_introduces_explicit_cast(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Aggregate selecting non-mapped type. Issue #20677.
            await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await base.Max_with_non_matching_types_in_projection_introduces_explicit_cast(async));

            AssertSql();
        }
    }

    public override async Task Min_with_non_matching_types_in_projection_introduces_explicit_cast(bool async)
    {
        // Aggregate selecting non-mapped type. Issue #20677.
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await base.Min_with_non_matching_types_in_projection_introduces_explicit_cast(async));

        AssertSql();
    }

    public override async Task OrderBy_Take_Last_gives_correct_result(bool async)
    {
        // Always throws sync-not-support for sync.
        if (async)
        {
            Assert.Equal(
                CosmosStrings.ReverseAfterSkipTakeNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    async () => await base.OrderBy_Take_Last_gives_correct_result(async))).Message);

            AssertSql();
        }
    }

    public override async Task OrderBy_Skip_Last_gives_correct_result(bool async)
    {
        Assert.Equal(
            CosmosStrings.ReverseAfterSkipTakeNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await base.OrderBy_Skip_Last_gives_correct_result(async))).Message);

        AssertSql();
    }

    public override async Task Contains_over_entityType_should_rewrite_to_identity_equality(bool async)
    {
        // Inner query is always sync.
        if (!async)
        {
            await Fixture.NoSyncTest(
                async, async a =>
                {
                    // Contains over subquery. Issue #17246.
                    await base.Contains_over_entityType_should_rewrite_to_identity_equality(a);
                }
            );
        }
    }

    public override async Task List_Contains_over_entityType_should_rewrite_to_identity_equality(bool async)
    {
        // Contains over subquery. Issue #17246.
        await AssertTranslationFailed(() => base.List_Contains_over_entityType_should_rewrite_to_identity_equality(async));

        AssertSql();
    }

    public override Task List_Contains_with_constant_list(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.List_Contains_with_constant_list(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE c["id"] IN ("ALFKI", "ANATR")
""");
            });

    public override Task List_Contains_with_parameter_list(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.List_Contains_with_parameter_list(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE c["id"] IN ("ALFKI", "ANATR")
""");
            });

    public override Task Contains_with_parameter_list_value_type_id(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_parameter_list_value_type_id(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Order") AND c["OrderID"] IN (10248, 10249))
""");
            });

    public override Task Contains_with_constant_list_value_type_id(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_constant_list_value_type_id(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Order") AND c["OrderID"] IN (10248, 10249))
""");
            });

    public override Task IImmutableSet_Contains_with_parameter(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.IImmutableSet_Contains_with_parameter(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["id"] = "ALFKI")
""");
            });

    public override Task IReadOnlySet_Contains_with_parameter(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.IReadOnlySet_Contains_with_parameter(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["id"] = "ALFKI")
""");
            });

    public override Task HashSet_Contains_with_parameter(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.HashSet_Contains_with_parameter(a);

                AssertSql(
                    """
@ids='["ALFKI"]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@ids, c["id"])
""");
            });

    public override Task ImmutableHashSet_Contains_with_parameter(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.ImmutableHashSet_Contains_with_parameter(a);

                AssertSql(
                    """
@ids='["ALFKI"]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@ids, c["id"])
""");
            });

    public override async Task Contains_over_entityType_with_null_should_rewrite_to_false(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Top-level Any(), see #33854.
            var exception =
                await Assert.ThrowsAsync<CosmosException>(() => base.Contains_over_entityType_with_null_should_rewrite_to_false(async));

            Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);

            AssertSql(
                """
@entity_equality_p_OrderID=null

SELECT VALUE EXISTS (
    SELECT 1
    FROM root c
    WHERE (((c["$type"] = "Order") AND (c["CustomerID"] = "VINET")) AND (c["OrderID"] = @entity_equality_p_OrderID)))
""");
        }
    }

    public override async Task Contains_over_entityType_with_null_in_projection(bool async)
    {
        // Contains over subquery. Issue #17246.
        await AssertTranslationFailed(() => base.Contains_over_entityType_with_null_in_projection(async));

        AssertSql();
    }

    public override Task String_FirstOrDefault_in_projection_does_not_do_client_eval(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.String_FirstOrDefault_in_projection_does_not_do_client_eval(a);

                AssertSql(
                    """
SELECT VALUE LEFT(c["id"], 1)
FROM root c
""");
            });

    public override Task Project_constant_Sum(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Project_constant_Sum(a);

                AssertSql(
                    """
SELECT VALUE SUM(1)
FROM root c
""");
            });

    public override Task Where_subquery_any_equals_operator(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_subquery_any_equals_operator(a);

                AssertSql(
                    """
@ids='["ABCDE","ALFKI","ANATR"]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@ids, c["id"])
""");
            });

    public override Task Where_subquery_any_equals(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_subquery_any_equals(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE c["id"] IN ("ABCDE", "ALFKI", "ANATR")
""");
            });

    public override Task Where_subquery_any_equals_static(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_subquery_any_equals_static(a);

                AssertSql(
                    """
@ids='["ABCDE","ALFKI","ANATR"]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@ids, c["id"])
""");
            });

    public override Task Where_subquery_where_any(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_subquery_where_any(a);

                AssertSql(
                    """
@ids='["ABCDE","ALFKI","ANATR"]'

SELECT VALUE c
FROM root c
WHERE ((c["City"] = "México D.F.") AND ARRAY_CONTAINS(@ids, c["id"]))
""",
                    //
                    """
@ids='["ABCDE","ALFKI","ANATR"]'

SELECT VALUE c
FROM root c
WHERE ((c["City"] = "México D.F.") AND ARRAY_CONTAINS(@ids, c["id"]))
""");
            });

    public override Task Where_subquery_all_not_equals_operator(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_subquery_all_not_equals_operator(a);

                AssertSql(
                    """
@ids='["ABCDE","ALFKI","ANATR"]'

SELECT VALUE c
FROM root c
WHERE NOT(ARRAY_CONTAINS(@ids, c["id"]))
""");
            });

    public override Task Where_subquery_all_not_equals(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_subquery_all_not_equals(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE c["id"] NOT IN ("ABCDE", "ALFKI", "ANATR")
""");
            });

    public override Task Where_subquery_all_not_equals_static(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_subquery_all_not_equals_static(a);

                AssertSql(
                    """
@ids='["ABCDE","ALFKI","ANATR"]'

SELECT VALUE c
FROM root c
WHERE NOT(ARRAY_CONTAINS(@ids, c["id"]))
""");
            });

    public override Task Where_subquery_where_all(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_subquery_where_all(a);

                AssertSql(
                    """
@ids='["ABCDE","ALFKI","ANATR"]'

SELECT VALUE c
FROM root c
WHERE ((c["City"] = "México D.F.") AND NOT(ARRAY_CONTAINS(@ids, c["id"])))
""",
                    //
                    """
@ids='["ABCDE","ALFKI","ANATR"]'

SELECT VALUE c
FROM root c
WHERE ((c["City"] = "México D.F.") AND NOT(ARRAY_CONTAINS(@ids, c["id"])))
""");
            });

    public override Task Cast_to_same_Type_Count_works(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Cast_to_same_Type_Count_works(a);

                AssertSql(
                    """
SELECT VALUE COUNT(1)
FROM root c
""");
            });

    public override async Task Cast_before_aggregate_is_preserved(bool async)
    {
        // Contains over subquery. Issue #17246.
        await AssertTranslationFailed(() => base.Cast_before_aggregate_is_preserved(async));

        AssertSql();
    }

    public override async Task Enumerable_min_is_mapped_to_Queryable_1(bool async)
    {
        // Contains over subquery. Issue #17246.
        await AssertTranslationFailed(() => base.Enumerable_min_is_mapped_to_Queryable_1(async));

        AssertSql();
    }

    public override async Task Enumerable_min_is_mapped_to_Queryable_2(bool async)
    {
        // Contains over subquery. Issue #17246.
        await AssertTranslationFailed(() => base.Enumerable_min_is_mapped_to_Queryable_2(async));

        AssertSql();
    }

    public override async Task DefaultIfEmpty_selects_only_required_columns(bool async)
    {
        // Contains over subquery. Issue #17246.
        await AssertTranslationFailed(() => base.DefaultIfEmpty_selects_only_required_columns(async));

        AssertSql();
    }

    public override async Task Collection_Last_member_access_in_projection_translated(bool async)
    {
        // Contains over subquery. Issue #17246.
        await AssertTranslationFailed(() => base.Collection_Last_member_access_in_projection_translated(async));

        AssertSql();
    }

    public override async Task Collection_LastOrDefault_member_access_in_projection_translated(bool async)
    {
        // Contains over subquery. Issue #17246.
        await AssertTranslationFailed(() => base.Collection_LastOrDefault_member_access_in_projection_translated(async));

        AssertSql();
    }

    public override async Task Sum_over_explicit_cast_over_column(bool async)
    {
        // Aggregate selecting non-mapped type. Issue #20677.
        await Assert.ThrowsAsync<KeyNotFoundException>(async () => await base.Sum_over_explicit_cast_over_column(async));

        AssertSql();
    }

    public override async Task Contains_over_scalar_with_null_should_rewrite_to_identity_equality_subquery(bool async)
    {
        // Contains over subquery. Issue #17246.
        await AssertTranslationFailed(
            () => base.Contains_over_scalar_with_null_should_rewrite_to_identity_equality_subquery(async));

        AssertSql();
    }

    public override async Task Contains_over_nullable_scalar_with_null_in_subquery_translated_correctly(bool async)
    {
        // Contains over subquery. Issue #17246.
        await AssertTranslationFailed(
            () => base.Contains_over_nullable_scalar_with_null_in_subquery_translated_correctly(async));

        AssertSql();
    }

    public override async Task Contains_over_non_nullable_scalar_with_null_in_subquery_simplifies_to_false(bool async)
    {
        // Contains over subquery. Issue #17246.
        await AssertTranslationFailed(
            () => base.Contains_over_non_nullable_scalar_with_null_in_subquery_simplifies_to_false(async));

        AssertSql();
    }

    public override async Task Contains_over_entityType_with_null_should_rewrite_to_identity_equality_subquery(bool async)
    {
        // Contains over subquery. Issue #17246.
        await AssertTranslationFailed(
            () => base.Contains_over_entityType_with_null_should_rewrite_to_identity_equality_subquery(async));

        AssertSql();
    }

    public override async Task Contains_over_entityType_with_null_should_rewrite_to_identity_equality_subquery_complex(bool async)
    {
        // Contains over subquery. Issue #17246.
        await AssertTranslationFailed(
            () => base.Contains_over_entityType_with_null_should_rewrite_to_identity_equality_subquery_complex(async));

        AssertSql();
    }

    public override async Task Contains_over_entityType_with_null_should_rewrite_to_identity_equality_subquery_negated(bool async)
    {
        // Contains over subquery. Issue #17246.
        await AssertTranslationFailed(
            () => base.Contains_over_entityType_with_null_should_rewrite_to_identity_equality_subquery_negated(async));

        AssertSql();
    }

    public override async Task Contains_over_entityType_should_materialize_when_composite(bool async)
    {
        // Contains over subquery. Issue #17246.
        await AssertTranslationFailed(() => base.Contains_over_entityType_should_materialize_when_composite(async));

        AssertSql();
    }

    public override async Task Contains_over_entityType_should_materialize_when_composite2(bool async)
    {
        // Contains over subquery. Issue #17246.
        await AssertTranslationFailed(() => base.Contains_over_entityType_should_materialize_when_composite2(async));

        AssertSql();
    }

    public override async Task Average_after_default_if_empty_does_not_throw(bool async)
    {
        // Contains over subquery. Issue #17246.
        await AssertTranslationFailed(() => base.Average_after_default_if_empty_does_not_throw(async));

        AssertSql();
    }

    public override async Task Max_after_default_if_empty_does_not_throw(bool async)
    {
        // Contains over subquery. Issue #17246.
        await AssertTranslationFailed(() => base.Max_after_default_if_empty_does_not_throw(async));

        AssertSql();
    }

    public override async Task Min_after_default_if_empty_does_not_throw(bool async)
    {
        // Contains over subquery. Issue #17246.
        await AssertTranslationFailed(() => base.Min_after_default_if_empty_does_not_throw(async));

        AssertSql();
    }

    public override async Task Average_with_unmapped_property_access_throws_meaningful_exception(bool async)
    {
        // Aggregate selecting non-mapped type. Issue #20677.
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => AssertAverage(
                async,
                ss => ss.Set<Order>(),
                selector: c => c.ShipVia));

        AssertSql();
    }

    public override async Task Multiple_collection_navigation_with_FirstOrDefault_chained(bool async)
    {
        // Contains over subquery. Issue #17246.
        await AssertTranslationFailed(() => base.Multiple_collection_navigation_with_FirstOrDefault_chained(async));

        AssertSql();
    }

    public override async Task All_true(bool async)
    {
        // Aggregates. Issue #16146.
        await AssertTranslationFailed(() => base.All_true(async));

        AssertSql();
    }

    public override async Task Not_Any_false(bool async)
    {
        // Aggregates. Issue #16146.
        await AssertTranslationFailed(() => base.Not_Any_false(async));

        AssertSql();
    }

    public override async Task Contains_inside_aggregate_function_with_GroupBy(bool async)
    {
        // GroupBy. Issue #17313.
        await AssertTranslationFailed(() => base.Contains_inside_aggregate_function_with_GroupBy(async));

        AssertSql();
    }

    public override Task Contains_inside_Average_without_GroupBy(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_inside_Average_without_GroupBy(a);

                AssertSql(
                    """
@cities='["London","Berlin"]'

SELECT VALUE AVG((ARRAY_CONTAINS(@cities, c["City"]) ? 1.0 : 0.0))
FROM root c
""");
            });

    public override Task Contains_inside_Sum_without_GroupBy(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_inside_Sum_without_GroupBy(a);

                AssertSql(
                    """
@cities='["London","Berlin"]'

SELECT VALUE SUM((ARRAY_CONTAINS(@cities, c["City"]) ? 1 : 0))
FROM root c
""");
            });

    public override Task Contains_inside_Count_without_GroupBy(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_inside_Count_without_GroupBy(a);

                AssertSql(
                    """
@cities='["London","Berlin"]'

SELECT VALUE COUNT(1)
FROM root c
WHERE ARRAY_CONTAINS(@cities, c["City"])
""");
            });

    public override Task Contains_inside_LongCount_without_GroupBy(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_inside_LongCount_without_GroupBy(a);

                AssertSql(
                    """
@cities='["London","Berlin"]'

SELECT VALUE COUNT(1)
FROM root c
WHERE ARRAY_CONTAINS(@cities, c["City"])
""");
            });

    public override Task Contains_inside_Max_without_GroupBy(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_inside_Max_without_GroupBy(a);

                AssertSql(
                    """
@cities='["London","Berlin"]'

SELECT VALUE MAX((ARRAY_CONTAINS(@cities, c["City"]) ? 1 : 0))
FROM root c
""");
            });

    public override Task Contains_inside_Min_without_GroupBy(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_inside_Min_without_GroupBy(a);

                AssertSql(
                    """
@cities='["London","Berlin"]'

SELECT VALUE MIN((ARRAY_CONTAINS(@cities, c["City"]) ? 1 : 0))
FROM root c
""");
            });

    public override Task Return_type_of_singular_operator_is_preserved(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Return_type_of_singular_operator_is_preserved(a);

                AssertSql(
                    """
SELECT c["id"], c["City"]
FROM root c
WHERE (c["id"] = "ALFKI")
OFFSET 0 LIMIT 1
""",
                    //
                    """
SELECT c["id"], c["City"]
FROM root c
WHERE (c["id"] = "ALFKI")
OFFSET 0 LIMIT 1
""",
                    //
                    """
SELECT c["id"], c["City"]
FROM root c
WHERE (c["id"] = "ALFKI")
OFFSET 0 LIMIT 2
""",
                    //
                    """
SELECT c["id"], c["City"]
FROM root c
WHERE (c["id"] = "ALFKI")
OFFSET 0 LIMIT 2
""",
                    //
                    """
SELECT c["id"], c["City"]
FROM root c
WHERE STARTSWITH(c["id"], "A")
ORDER BY c["id"] DESC
OFFSET 0 LIMIT 1
""",
                    //
                    """
SELECT c["id"], c["City"]
FROM root c
WHERE STARTSWITH(c["id"], "A")
ORDER BY c["id"] DESC
OFFSET 0 LIMIT 1
""");
            });

    [ConditionalTheory(Skip = "Issue #20677")]
    public override async Task Type_casting_inside_sum(bool async)
    {
        await base.Type_casting_inside_sum(async);

        AssertSql();
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}
