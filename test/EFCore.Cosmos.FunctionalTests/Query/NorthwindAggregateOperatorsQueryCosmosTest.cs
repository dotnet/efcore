// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
SELECT AVG((c["OrderID"] - 10248)) AS c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderID"] = 10248))
""");
            });

    public override Task Contains_over_keyless_entity_throws(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                // The `First()` query is always executed synchronously. The outer query does not translate.
                if (!a)
                {
                    // Aggregates. Issue #16146.
                    await base.Contains_over_keyless_entity_throws(a);

                    AssertSql(
                        """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
OFFSET 0 LIMIT 1
""");
                }
            });

    public override Task Contains_with_local_non_primitive_list_closure_mix(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_non_primitive_list_closure_mix(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["CustomerID"] IN ("ABCDE", "ALFKI"))
""");
            });

    public override Task Contains_with_local_non_primitive_list_inline_closure_mix(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_non_primitive_list_inline_closure_mix(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["CustomerID"] IN ("ABCDE", "ALFKI"))
""",
                    //
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["CustomerID"] IN ("ABCDE", "ANATR"))
""");
            });

    public override Task Count_on_projection_with_client_eval(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Count_on_projection_with_client_eval(a);

                AssertSql(
                    """
SELECT COUNT(1) AS c
FROM root c
WHERE (c["Discriminator"] = "Order")
""",
                    //
                    """
SELECT COUNT(1) AS c
FROM root c
WHERE (c["Discriminator"] = "Order")
""",
                    //
                    """
SELECT COUNT(1) AS c
FROM root c
WHERE (c["Discriminator"] = "Order")
""");
            });

    public override Task First(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.First(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
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
SELECT MAX((c["OrderID"] - 10248)) AS c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderID"] = 10248))
""");
            });

    public override Task Min_over_default_returns_default(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Min_over_default_returns_default(a);

                AssertSql(
                    """
SELECT MIN((c["OrderID"] - 10248)) AS c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderID"] = 10248))
""");
            });

    public override Task Sum_over_empty_returns_zero(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Sum_over_empty_returns_zero(a);

                AssertSql(
                    """
SELECT SUM(c["OrderID"]) AS c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderID"] = 42))
""");
            });

    public override Task First_Predicate(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.First_Predicate(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = "London"))
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
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
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
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = "London"))
ORDER BY c["ContactName"]
OFFSET 0 LIMIT 1
""");
            });

    public override Task Where_Single(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_Single(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = "ALFKI"))
OFFSET 0 LIMIT 2
""");
            });

    public override Task FirstOrDefault(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.FirstOrDefault(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
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
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["CustomerID"] IN ("ALFKI", "WRONG"))
""");
            });

    public override Task FirstOrDefault_Predicate(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.FirstOrDefault_Predicate(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = "London"))
ORDER BY c["ContactName"]
OFFSET 0 LIMIT 1
""");
            });

    public override Task SingleOrDefault_Predicate(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.SingleOrDefault_Predicate(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = "ALFKI"))
OFFSET 0 LIMIT 2
""");
            });

    public override async Task SingleOrDefault_Throws(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await base.SingleOrDefault_Throws(async);

            AssertSql(
                """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
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
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = "London"))
ORDER BY c["ContactName"]
OFFSET 0 LIMIT 1
""");
            });

    public override Task Where_SingleOrDefault(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_SingleOrDefault(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = "ALFKI"))
OFFSET 0 LIMIT 2
""");
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
SELECT SUM(c["OrderID"]) AS c
FROM root c
WHERE (c["Discriminator"] = "Order")
""");
            });

    public override Task Sum_with_no_data_cast_to_nullable(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Sum_with_no_data_cast_to_nullable(a);

                AssertSql(
                    """
SELECT SUM(c["OrderID"]) AS c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderID"] < 0))
""");
            });

    public override Task Sum_with_binary_expression(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Sum_with_binary_expression(a);

                AssertSql(
                    """
SELECT SUM((c["OrderID"] * 2)) AS c
FROM root c
WHERE (c["Discriminator"] = "Order")
""");
            });

    public override Task Sum_with_no_arg_empty(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Sum_with_no_arg_empty(a);

                AssertSql(
                    """
SELECT SUM(c["OrderID"]) AS c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderID"] = 42))
""");
            });

    public override Task Sum_with_no_data_nullable(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Sum_with_no_data_nullable(a);

                AssertSql(
                    """
SELECT SUM(c["SupplierID"]) AS c
FROM root c
WHERE (c["Discriminator"] = "Product")
""");
            });

    public override Task Sum_with_arg(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Sum_with_arg(a);

                AssertSql(
                    """
SELECT SUM(c["OrderID"]) AS c
FROM root c
WHERE (c["Discriminator"] = "Order")
""");
            });

    public override Task Sum_with_arg_expression(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Sum_with_arg_expression(a);

                AssertSql(
                    """
SELECT SUM((c["OrderID"] + c["OrderID"])) AS c
FROM root c
WHERE (c["Discriminator"] = "Order")
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
SELECT SUM(((c["UnitPrice"] != null) ? c["UnitPrice"] : 0.0)) AS c
FROM root c
WHERE ((c["Discriminator"] = "Product") AND (c["ProductID"] < 40))
""");
            });

    public override async Task Sum_over_subquery_is_client_eval(bool async)
    {
        // Aggregates. Issue #16146.
        await AssertTranslationFailed(() => base.Sum_over_subquery_is_client_eval(async));

        AssertSql();
    }

    public override async Task Sum_over_nested_subquery_is_client_eval(bool async)
    {
        // Aggregates. Issue #16146.
        await AssertTranslationFailed(() => base.Sum_over_nested_subquery_is_client_eval(async));

        AssertSql();
    }

    public override async Task Sum_over_min_subquery_is_client_eval(bool async)
    {
        // Aggregates. Issue #16146.
        await AssertTranslationFailed(() => base.Sum_over_min_subquery_is_client_eval(async));

        AssertSql();
    }

    public override Task Sum_on_float_column(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Sum_on_float_column(a);

                AssertSql(
                    """
SELECT SUM(c["Discount"]) AS c
FROM root c
WHERE ((c["Discriminator"] = "OrderDetail") AND (c["ProductID"] = 1))
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
SELECT AVG(c["OrderID"]) AS c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderID"] = -1))
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
SELECT AVG(c["SupplierID"]) AS c
FROM root c
WHERE ((c["Discriminator"] = "Product") AND (c["SupplierID"] = -1))
""");
            });

    public override Task Average_no_data_cast_to_nullable(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Average_no_data_cast_to_nullable(a);

                AssertSql(
                    """
SELECT AVG(c["OrderID"]) AS c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderID"] = -1))
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
SELECT MIN(c["OrderID"]) AS c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderID"] = -1))
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
SELECT MAX(c["OrderID"]) AS c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderID"] = -1))
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
SELECT MAX(c["SupplierID"]) AS c
FROM root c
WHERE ((c["Discriminator"] = "Product") AND (c["SupplierID"] = -1))
""");
            });

    public override Task Max_no_data_cast_to_nullable(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Max_no_data_cast_to_nullable(a);

                AssertSql(
                    """
SELECT MAX(c["OrderID"]) AS c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderID"] = -1))
""");
            });

    public override async Task Min_no_data_subquery(bool async)
    {
        // Aggregates. Issue #16146.
        await AssertTranslationFailed(() => base.Min_no_data_subquery(async));

        AssertSql();
    }

    public override async Task Average_with_no_arg(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Average truncates. Issue #26378.
            await Assert.ThrowsAsync<EqualException>(async () => await base.Average_with_no_arg(async));

            AssertSql(
                """
SELECT AVG(c["OrderID"]) AS c
FROM root c
WHERE (c["Discriminator"] = "Order")
""");
        }
    }

    public override Task Average_with_binary_expression(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Average_with_binary_expression(a);

                AssertSql(
                    """
SELECT AVG((c["OrderID"] * 2)) AS c
FROM root c
WHERE (c["Discriminator"] = "Order")
""");
            });

    public override async Task Average_with_arg(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Average truncates. Issue #26378.
            await Assert.ThrowsAsync<EqualException>(async () => await base.Average_with_arg(async));

            AssertSql(
                """
SELECT AVG(c["OrderID"]) AS c
FROM root c
WHERE (c["Discriminator"] = "Order")
""");
        }
    }

    public override Task Average_with_arg_expression(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Average_with_arg_expression(a);

                AssertSql(
                    """
SELECT AVG((c["OrderID"] + c["OrderID"])) AS c
FROM root c
WHERE (c["Discriminator"] = "Order")
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
SELECT AVG(((c["UnitPrice"] != null) ? c["UnitPrice"] : 0.0)) AS c
FROM root c
WHERE ((c["Discriminator"] = "Product") AND (c["ProductID"] < 40))
""");
            });

    public override async Task Average_over_subquery_is_client_eval(bool async)
    {
        // Aggregates. Issue #16146.
        await AssertTranslationFailed(() => base.Average_over_subquery_is_client_eval(async));

        AssertSql();
    }

    public override async Task Average_over_nested_subquery_is_client_eval(bool async)
    {
        // Aggregates. Issue #16146.
        await AssertTranslationFailed(() => base.Average_over_nested_subquery_is_client_eval(async));

        AssertSql();
    }

    public override async Task Average_over_max_subquery_is_client_eval(bool async)
    {
        // Aggregates. Issue #16146.
        await AssertTranslationFailed(() => base.Average_over_max_subquery_is_client_eval(async));

        AssertSql();
    }

    public override Task Average_on_float_column(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Average_on_float_column(a);

                AssertSql(
                    """
SELECT AVG(c["Discount"]) AS c
FROM root c
WHERE ((c["Discriminator"] = "OrderDetail") AND (c["ProductID"] = 1))
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
SELECT MIN(c["OrderID"]) AS c
FROM root c
WHERE (c["Discriminator"] = "Order")
""");
            });

    public override Task Min_with_arg(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Min_with_arg(a);

                AssertSql(
                    """
SELECT MIN(c["OrderID"]) AS c
FROM root c
WHERE (c["Discriminator"] = "Order")
""");
            });

    public override Task Min_no_data_nullable(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Min_no_data_nullable(a);

                AssertSql(
                    """
SELECT MIN(c["SupplierID"]) AS c
FROM root c
WHERE ((c["Discriminator"] = "Product") AND (c["SupplierID"] = -1))
""");
            });

    public override Task Min_no_data_cast_to_nullable(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Min_no_data_cast_to_nullable(a);

                AssertSql(
                    """
SELECT MIN(c["OrderID"]) AS c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["OrderID"] = -1))
""");
            });

    public override Task Min_with_coalesce(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Min_with_coalesce(a);

                AssertSql(
                    """
SELECT MIN(((c["UnitPrice"] != null) ? c["UnitPrice"] : 0.0)) AS c
FROM root c
WHERE ((c["Discriminator"] = "Product") AND (c["ProductID"] < 40))
""");
            });

    public override async Task Min_over_subquery_is_client_eval(bool async)
    {
        // Aggregates. Issue #16146.
        await AssertTranslationFailed(() => base.Min_over_subquery_is_client_eval(async));

        AssertSql();
    }

    public override async Task Min_over_nested_subquery_is_client_eval(bool async)
    {
        // Aggregates. Issue #16146.
        await AssertTranslationFailed(() => base.Min_over_nested_subquery_is_client_eval(async));

        AssertSql();
    }

    public override async Task Min_over_max_subquery_is_client_eval(bool async)
    {
        // Aggregates. Issue #16146.
        await AssertTranslationFailed(() => base.Min_over_max_subquery_is_client_eval(async));

        AssertSql();
    }

    public override Task Max_with_no_arg(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Max_with_no_arg(a);

                AssertSql(
                    """
SELECT MAX(c["OrderID"]) AS c
FROM root c
WHERE (c["Discriminator"] = "Order")
""");
            });

    public override Task Max_with_arg(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Max_with_arg(a);

                AssertSql(
                    """
SELECT MAX(c["OrderID"]) AS c
FROM root c
WHERE (c["Discriminator"] = "Order")
""");
            });

    public override Task Max_with_coalesce(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Max_with_coalesce(a);

                AssertSql(
                    """
SELECT MAX(((c["UnitPrice"] != null) ? c["UnitPrice"] : 0.0)) AS c
FROM root c
WHERE ((c["Discriminator"] = "Product") AND (c["ProductID"] < 40))
""");
            });

    public override async Task Max_over_subquery_is_client_eval(bool async)
    {
        // Aggregates. Issue #16146.
        await AssertTranslationFailed(() => base.Max_over_subquery_is_client_eval(async));

        AssertSql();
    }

    public override async Task Max_over_nested_subquery_is_client_eval(bool async)
    {
        // Aggregates. Issue #16146.
        await AssertTranslationFailed(() => base.Max_over_nested_subquery_is_client_eval(async));

        AssertSql();
    }

    public override async Task Max_over_sum_subquery_is_client_eval(bool async)
    {
        // Aggregates. Issue #16146.
        await AssertTranslationFailed(() => base.Max_over_sum_subquery_is_client_eval(async));

        AssertSql();
    }

    public override Task Count_with_no_predicate(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Count_with_no_predicate(a);

                AssertSql(
                    """
SELECT COUNT(1) AS c
FROM root c
WHERE (c["Discriminator"] = "Order")
""");
            });

    public override Task Count_with_predicate(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Count_with_predicate(a);

                AssertSql(
                    """
SELECT COUNT(1) AS c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["CustomerID"] = "ALFKI"))
""");
            });

    public override Task Count_with_order_by(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Count_with_order_by(a);

                AssertSql(
                    """
SELECT COUNT(1) AS c
FROM root c
WHERE (c["Discriminator"] = "Order")
""");
            });

    public override Task Where_OrderBy_Count(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_OrderBy_Count(a);

                AssertSql(
                    """
SELECT COUNT(1) AS c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["CustomerID"] = "ALFKI"))
""");
            });

    public override Task OrderBy_Where_Count(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.OrderBy_Where_Count(a);

                AssertSql(
                    """
SELECT COUNT(1) AS c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["CustomerID"] = "ALFKI"))
""");
            });

    public override Task OrderBy_Count_with_predicate(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.OrderBy_Count_with_predicate(a);

                AssertSql(
                    """
SELECT COUNT(1) AS c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["CustomerID"] = "ALFKI"))
""");
            });

    public override Task OrderBy_Where_Count_with_predicate(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.OrderBy_Where_Count_with_predicate(a);

                AssertSql(
                    """
SELECT COUNT(1) AS c
FROM root c
WHERE (((c["Discriminator"] = "Order") AND (c["OrderID"] > 10)) AND (c["CustomerID"] != "ALFKI"))
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
@__p_0='10'

SELECT c
FROM root c
WHERE (c["Discriminator"] = "Employee")
ORDER BY 42
OFFSET 0 LIMIT @__p_0
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
SELECT DISTINCT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
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
WHERE (c[""Discriminator""] = ""Customer"")
""");
            });

    public override Task OrderBy_Distinct(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.OrderBy_Distinct(a);

                AssertSql(
                    """
SELECT DISTINCT c["City"]
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY c["CustomerID"]
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

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = "ALFKI"))
OFFSET 0 LIMIT 2
""");
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
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
ORDER BY c["ContactName"] DESC
OFFSET 0 LIMIT 1
""");
            });

    public override Task Last_when_no_order_by(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Last_when_no_order_by(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = "ALFKI"))
OFFSET 0 LIMIT 1
""");
            });

    public override Task LastOrDefault_when_no_order_by(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.LastOrDefault_when_no_order_by(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = "ALFKI"))
OFFSET 0 LIMIT 1
""");
            });

    public override Task Last_Predicate(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Last_Predicate(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = "London"))
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
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = "London"))
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
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
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
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = "London"))
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
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = "London"))
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
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["CustomerID"] IN ("ABCDE", "ALFKI"))
""",
                    //
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["CustomerID"] IN ("ABCDE"))
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
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Employee") AND c["EmployeeID"] IN (0, 1))
""",
                    //
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Employee") AND c["EmployeeID"] IN (0))
""");
            });

    public override Task Contains_with_local_nullable_uint_array_closure(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_nullable_uint_array_closure(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Employee") AND c["EmployeeID"] IN (0, 1))
""",
                    //
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Employee") AND c["EmployeeID"] IN (0))
""");
            });

    public override Task Contains_with_local_array_inline(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_array_inline(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["CustomerID"] IN ("ABCDE", "ALFKI"))
""");
            });

    public override Task Contains_with_local_list_closure(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_list_closure(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["CustomerID"] IN ("ABCDE", "ALFKI"))
""");
            });

    public override Task Contains_with_local_object_list_closure(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_object_list_closure(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["CustomerID"] IN ("ABCDE", "ALFKI"))
""");
            });

    public override Task Contains_with_local_list_closure_all_null(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_list_closure_all_null(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = null))
""");
            });

    public override Task Contains_with_local_list_inline(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_list_inline(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["CustomerID"] IN ("ABCDE", "ALFKI"))
""");
            });

    public override Task Contains_with_local_list_inline_closure_mix(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_list_inline_closure_mix(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["CustomerID"] IN ("ABCDE", "ALFKI"))
""",
                    //
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["CustomerID"] IN ("ABCDE", "ANATR"))
""");
            });

    public override Task Contains_with_local_enumerable_closure(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_enumerable_closure(a);
                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["CustomerID"] IN ("ABCDE", "ALFKI"))
""",
                    //
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["CustomerID"] IN ("ABCDE"))
""");
            });

    public override Task Contains_with_local_object_enumerable_closure(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_object_enumerable_closure(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["CustomerID"] IN ("ABCDE", "ALFKI"))
"""
                );
            });

    public override Task Contains_with_local_enumerable_closure_all_null(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_enumerable_closure_all_null(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (true = false))
"""
                );
            });

    public override async Task Contains_with_local_enumerable_inline(bool async)
    {
        // Issue #31776
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await base.Contains_with_local_enumerable_inline(async));

        AssertSql();
    }

    public override async Task Contains_with_local_enumerable_inline_closure_mix(bool async)
    {
        // Issue #31776
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await base.Contains_with_local_enumerable_inline_closure_mix(async));

        AssertSql();
    }

    public override Task Contains_with_local_ordered_enumerable_closure(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_ordered_enumerable_closure(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["CustomerID"] IN ("ABCDE", "ALFKI"))
""",
//
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["CustomerID"] IN ("ABCDE"))
"""
                );
            });

    public override Task Contains_with_local_object_ordered_enumerable_closure(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_object_ordered_enumerable_closure(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["CustomerID"] IN ("ABCDE", "ALFKI"))
"""
                );
            });

    public override Task Contains_with_local_ordered_enumerable_closure_all_null(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_ordered_enumerable_closure_all_null(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = null))
"""
                );
            });

    public override Task Contains_with_local_ordered_enumerable_inline(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_ordered_enumerable_inline(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["CustomerID"] IN ("ABCDE", "ALFKI"))
"""
                );
            });

    public override Task Contains_with_local_ordered_enumerable_inline_closure_mix(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_ordered_enumerable_inline_closure_mix(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["CustomerID"] IN ("ABCDE", "ALFKI"))
""",
//
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["CustomerID"] IN ("ABCDE", "ANATR"))
"""
                );
            });

    public override Task Contains_with_local_read_only_collection_closure(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_read_only_collection_closure(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["CustomerID"] IN ("ABCDE", "ALFKI"))
""",
//
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["CustomerID"] IN ("ABCDE"))
"""
                );
            });

    public override Task Contains_with_local_object_read_only_collection_closure(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_object_read_only_collection_closure(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["CustomerID"] IN ("ABCDE", "ALFKI"))
"""
                );
            });

    public override Task Contains_with_local_ordered_read_only_collection_all_null(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_ordered_read_only_collection_all_null(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = null))
"""
                );
            });

    public override Task Contains_with_local_read_only_collection_inline(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_read_only_collection_inline(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["CustomerID"] IN ("ABCDE", "ALFKI"))
"""
                );
            });

    public override Task Contains_with_local_read_only_collection_inline_closure_mix(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_read_only_collection_inline_closure_mix(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["CustomerID"] IN ("ABCDE", "ALFKI"))
""",
//
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["CustomerID"] IN ("ABCDE", "ANATR"))
"""
                );
            });

    public override Task Contains_with_local_collection_false(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_collection_false(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["CustomerID"] NOT IN ("ABCDE", "ALFKI"))
""");
            });

    public override Task Contains_with_local_collection_complex_predicate_and(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_collection_complex_predicate_and(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (((c["CustomerID"] = "ALFKI") OR (c["CustomerID"] = "ABCDE")) AND c["CustomerID"] IN ("ABCDE", "ALFKI")))
""");
            });

    public override Task Contains_with_local_collection_complex_predicate_or(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_collection_complex_predicate_or(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] IN ("ABCDE", "ALFKI") OR ((c["CustomerID"] = "ALFKI") OR (c["CustomerID"] = "ABCDE"))))
""");
            });

    public override Task Contains_with_local_collection_complex_predicate_not_matching_ins1(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_collection_complex_predicate_not_matching_ins1(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (((c["CustomerID"] = "ALFKI") OR (c["CustomerID"] = "ABCDE")) OR c["CustomerID"] NOT IN ("ABCDE", "ALFKI")))
""");
            });

    public override Task Contains_with_local_collection_complex_predicate_not_matching_ins2(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_collection_complex_predicate_not_matching_ins2(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] IN ("ABCDE", "ALFKI") AND ((c["CustomerID"] != "ALFKI") AND (c["CustomerID"] != "ABCDE"))))
""");
            });

    public override Task Contains_with_local_collection_sql_injection(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_collection_sql_injection(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] IN ("ALFKI", "ABC')); GO; DROP TABLE Orders; GO; --") OR ((c["CustomerID"] = "ALFKI") OR (c["CustomerID"] = "ABCDE"))))
""");
            });

    public override Task Contains_with_local_collection_empty_closure(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_collection_empty_closure(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (true = false))
""");
            });

    public override Task Contains_with_local_collection_empty_inline(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_local_collection_empty_inline(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND NOT((true = false)))
""");
            });

    public override async Task Contains_top_level(bool async)
    {
        // Contains over subquery. Issue #17246.
        await AssertTranslationFailed(() => base.Contains_top_level(async));

        AssertSql();
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
        // Contains over subquery. Issue #15937.
        await AssertTranslationFailed(() => base.OfType_Select(async));

        AssertSql();
    }

    public override async Task OfType_Select_OfType_Select(bool async)
    {
        // Contains over subquery. Issue #17246.
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
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["CustomerID"] IN ("ALFKI", "ANATR"))
""");
            });

    public override Task List_Contains_with_parameter_list(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.List_Contains_with_parameter_list(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["CustomerID"] IN ("ALFKI", "ANATR"))
""");
            });

    public override Task Contains_with_parameter_list_value_type_id(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_parameter_list_value_type_id(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND c["OrderID"] IN (10248, 10249))
""");
            });

    public override Task Contains_with_constant_list_value_type_id(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_with_constant_list_value_type_id(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND c["OrderID"] IN (10248, 10249))
""");
            });

    public override Task IImmutableSet_Contains_with_parameter(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.IImmutableSet_Contains_with_parameter(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["CustomerID"] IN ("ALFKI"))
""");
            });

    public override Task IReadOnlySet_Contains_with_parameter(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.IReadOnlySet_Contains_with_parameter(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["CustomerID"] IN ("ALFKI"))
""");
            });

    public override Task HashSet_Contains_with_parameter(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.HashSet_Contains_with_parameter(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["CustomerID"] IN ("ALFKI"))
""");
            });

    public override Task ImmutableHashSet_Contains_with_parameter(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.ImmutableHashSet_Contains_with_parameter(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["CustomerID"] IN ("ALFKI"))
""");
            });

    public override async Task Contains_over_entityType_with_null_should_rewrite_to_false(bool async)
    {
        // Contains over subquery. Issue #17246.
        await AssertTranslationFailed(() => base.Contains_over_entityType_with_null_should_rewrite_to_false(async));

        AssertSql();
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
SELECT LEFT(c["CustomerID"], 1) AS c
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override Task Project_constant_Sum(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Project_constant_Sum(a);

                AssertSql(
                    """
SELECT SUM(1) AS c
FROM root c
WHERE (c["Discriminator"] = "Employee")
""");
            });

    public override Task Where_subquery_any_equals_operator(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_subquery_any_equals_operator(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["CustomerID"] IN ("ABCDE", "ALFKI", "ANATR"))
""");
            });

    public override Task Where_subquery_any_equals(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_subquery_any_equals(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["CustomerID"] IN ("ABCDE", "ALFKI", "ANATR"))
""");
            });

    public override Task Where_subquery_any_equals_static(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_subquery_any_equals_static(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["CustomerID"] IN ("ABCDE", "ALFKI", "ANATR"))
""");
            });

    public override Task Where_subquery_where_any(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_subquery_where_any(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (((c["Discriminator"] = "Customer") AND (c["City"] = "México D.F.")) AND c["CustomerID"] IN ("ABCDE", "ALFKI", "ANATR"))
""",
                    //
                    """
SELECT c
FROM root c
WHERE (((c["Discriminator"] = "Customer") AND (c["City"] = "México D.F.")) AND c["CustomerID"] IN ("ABCDE", "ALFKI", "ANATR"))
""");
            });

    public override Task Where_subquery_all_not_equals_operator(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_subquery_all_not_equals_operator(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["CustomerID"] NOT IN ("ABCDE", "ALFKI", "ANATR"))
""");
            });

    public override Task Where_subquery_all_not_equals(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_subquery_all_not_equals(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["CustomerID"] NOT IN ("ABCDE", "ALFKI", "ANATR"))
""");
            });

    public override Task Where_subquery_all_not_equals_static(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_subquery_all_not_equals_static(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["CustomerID"] NOT IN ("ABCDE", "ALFKI", "ANATR"))
""");
            });

    public override Task Where_subquery_where_all(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_subquery_where_all(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (((c["Discriminator"] = "Customer") AND (c["City"] = "México D.F.")) AND c["CustomerID"] NOT IN ("ABCDE", "ALFKI", "ANATR"))
""",
                    //
                    """
SELECT c
FROM root c
WHERE (((c["Discriminator"] = "Customer") AND (c["City"] = "México D.F.")) AND c["CustomerID"] NOT IN ("ABCDE", "ALFKI", "ANATR"))
""");
            });

    public override Task Cast_to_same_Type_Count_works(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Cast_to_same_Type_Count_works(a);

                AssertSql(
                    """
SELECT COUNT(1) AS c
FROM root c
WHERE (c["Discriminator"] = "Customer")
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
SELECT AVG((c["City"] IN ("London", "Berlin") ? 1.0 : 0.0)) AS c
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override Task Contains_inside_Sum_without_GroupBy(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_inside_Sum_without_GroupBy(a);

                AssertSql(
                    """
SELECT SUM((c["City"] IN ("London", "Berlin") ? 1 : 0)) AS c
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override Task Contains_inside_Count_without_GroupBy(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_inside_Count_without_GroupBy(a);

                AssertSql(
                    """
SELECT COUNT(1) AS c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["City"] IN ("London", "Berlin"))
""");
            });

    public override Task Contains_inside_LongCount_without_GroupBy(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_inside_LongCount_without_GroupBy(a);

                AssertSql(
                    """
SELECT COUNT(1) AS c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND c["City"] IN ("London", "Berlin"))
""");
            });

    public override Task Contains_inside_Max_without_GroupBy(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_inside_Max_without_GroupBy(a);

                AssertSql(
                    """
SELECT MAX((c["City"] IN ("London", "Berlin") ? 1 : 0)) AS c
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override Task Contains_inside_Min_without_GroupBy(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Contains_inside_Min_without_GroupBy(a);

                AssertSql(
                    """
SELECT MIN((c["City"] IN ("London", "Berlin") ? 1 : 0)) AS c
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override Task Return_type_of_singular_operator_is_preserved(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Return_type_of_singular_operator_is_preserved(a);

                AssertSql(
                    """
SELECT c["CustomerID"], c["City"]
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = "ALFKI"))
OFFSET 0 LIMIT 1
""",
                    //
                    """
SELECT c["CustomerID"], c["City"]
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = "ALFKI"))
OFFSET 0 LIMIT 1
""",
                    //
                    """
SELECT c["CustomerID"], c["City"]
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = "ALFKI"))
OFFSET 0 LIMIT 2
""",
                    //
                    """
SELECT c["CustomerID"], c["City"]
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["CustomerID"] = "ALFKI"))
OFFSET 0 LIMIT 2
""",
                    //
                    """
SELECT c["CustomerID"], c["City"]
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND STARTSWITH(c["CustomerID"], "A"))
ORDER BY c["CustomerID"] DESC
OFFSET 0 LIMIT 1
""",
                    //
                    """
SELECT c["CustomerID"], c["City"]
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND STARTSWITH(c["CustomerID"], "A"))
ORDER BY c["CustomerID"] DESC
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
