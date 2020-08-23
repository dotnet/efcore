// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindAggregateOperatorsQueryCosmosTest : NorthwindAggregateOperatorsQueryTestBase<
        NorthwindQueryCosmosFixture<NoopModelCustomizer>>
    {
        public NorthwindAggregateOperatorsQueryCosmosTest(
            NorthwindQueryCosmosFixture<NoopModelCustomizer> fixture,
#pragma warning disable IDE0060 // Remove unused parameter
            ITestOutputHelper testOutputHelper)
#pragma warning restore IDE0060 // Remove unused parameter
            : base(fixture)
        {
            ClearLog();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [ConditionalFact(Skip = "Issue#17246")]
        public override void Select_All()
        {
            base.Select_All();

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task Sum_with_no_arg(bool async)
        {
            await base.Sum_with_no_arg(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override Task Sum_with_no_data_cast_to_nullable(bool async)
        {
            return base.Sum_with_no_data_cast_to_nullable(async);
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task Sum_with_binary_expression(bool async)
        {
            await base.Sum_with_binary_expression(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Sum_with_no_arg_empty(bool async)
        {
            return base.Sum_with_no_arg_empty(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Sum_with_no_data_nullable(bool async)
        {
            return base.Sum_with_no_data_nullable(async);
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task Sum_with_arg(bool async)
        {
            await base.Sum_with_arg(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task Sum_with_arg_expression(bool async)
        {
            await base.Sum_with_arg_expression(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task Sum_with_division_on_decimal(bool async)
        {
            await base.Sum_with_division_on_decimal(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""OrderDetail"")");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task Sum_with_division_on_decimal_no_significant_digits(bool async)
        {
            await base.Sum_with_division_on_decimal_no_significant_digits(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""OrderDetail"")");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task Sum_with_coalesce(bool async)
        {
            await base.Sum_with_coalesce(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Product"") AND (c[""ProductID""] < 40))");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task Sum_over_subquery_is_client_eval(bool async)
        {
            await AssertSum(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI"),
                selector: c => c.Orders.Sum(o => o.OrderID));

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task Sum_over_nested_subquery_is_client_eval(bool async)
        {
            await AssertSum(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "CENTC"),
                selector: c => c.Orders.Sum(
                    o => 5 + o.OrderDetails.Where(od => od.OrderID >= 10250 && od.OrderID <= 10300).Sum(od => od.ProductID)));

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""CENTC""))");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task Sum_over_min_subquery_is_client_eval(bool async)
        {
            await AssertSum(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "CENTC"),
                selector: c => c.Orders.Sum(
                    o => 5 + o.OrderDetails.Where(od => od.OrderID >= 10250 && od.OrderID <= 10300).Min(od => od.ProductID)));

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""CENTC""))");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task Sum_on_float_column(bool async)
        {
            await base.Sum_on_float_column(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND (c[""ProductID""] = 1))");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task Sum_on_float_column_in_subquery(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(o => o.OrderID < 10250)
                    .Select(o => new { o.OrderID, Sum = o.OrderDetails.Sum(od => od.Discount) }),
                e => e.OrderID);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderID""] < 10250))");
        }

        [ConditionalFact(Skip = "Issue#16146")]
        public override void Average_no_data()
        {
            base.Average_no_data();
        }

        [ConditionalFact(Skip = "Issue#16146")]
        public override void Average_no_data_nullable()
        {
        }

        [ConditionalFact(Skip = "Issue#16146")]
        public override void Average_no_data_cast_to_nullable()
        {
        }

        [ConditionalFact(Skip = "Issue#16146")]
        public override void Min_no_data()
        {
            base.Min_no_data();
        }

        [ConditionalFact(Skip = "Issue#16146")]
        public override void Max_no_data()
        {
            base.Max_no_data();
        }

        [ConditionalFact(Skip = "Issue#16146")]
        public override void Average_no_data_subquery()
        {
            base.Average_no_data_subquery();
        }

        [ConditionalFact(Skip = "Issue#16146")]
        public override void Max_no_data_subquery()
        {
            base.Max_no_data_subquery();
        }

        [ConditionalFact(Skip = "Issue#16146")]
        public override void Max_no_data_nullable()
        {
        }

        [ConditionalFact(Skip = "Issue#16146")]
        public override void Max_no_data_cast_to_nullable()
        {
        }

        [ConditionalFact(Skip = "Issue#16146")]
        public override void Min_no_data_subquery()
        {
            base.Min_no_data_subquery();
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task Average_with_no_arg(bool async)
        {
            await base.Average_with_no_arg(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task Average_with_binary_expression(bool async)
        {
            await base.Average_with_binary_expression(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task Average_with_arg(bool async)
        {
            await base.Average_with_arg(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task Average_with_arg_expression(bool async)
        {
            await base.Average_with_arg_expression(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task Average_with_division_on_decimal(bool async)
        {
            await base.Average_with_division_on_decimal(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""OrderDetail"")");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task Average_with_division_on_decimal_no_significant_digits(bool async)
        {
            await base.Average_with_division_on_decimal_no_significant_digits(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""OrderDetail"")");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task Average_with_coalesce(bool async)
        {
            await base.Average_with_coalesce(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Product"") AND (c[""ProductID""] < 40))");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task Average_over_subquery_is_client_eval(bool async)
        {
            await AssertAverage(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI"),
                selector: c => c.Orders.Where(o => o.OrderID < 10250).Sum(o => o.OrderID));

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task Average_over_nested_subquery_is_client_eval(bool async)
        {
            await AssertAverage(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "CENTC"),
                selector: c => (decimal)c.Orders
                    .Average(o => 5 + o.OrderDetails.Where(od => od.OrderID > 10250 && od.OrderID < 10300).Average(od => od.ProductID)));

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""CENTC""))");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task Average_over_max_subquery_is_client_eval(bool async)
        {
            await AssertAverage(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "CENTC"),
                selector: c => (decimal)c.Orders
                    .Average(o => 5 + o.OrderDetails.Where(od => od.OrderID > 10250 && od.OrderID < 10300).Max(od => od.ProductID)));

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""CENTC""))");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task Average_on_float_column(bool async)
        {
            await base.Average_on_float_column(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND (c[""ProductID""] = 1))");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task Average_on_float_column_in_subquery(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(o => o.OrderID < 10250).Select(
                    o => new { o.OrderID, Sum = o.OrderDetails.Average(od => od.Discount) }),
                e => e.OrderID);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderID""] < 10250))");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task Average_on_float_column_in_subquery_with_cast(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(o => o.OrderID < 10250)
                    .Select(
                        o => new { o.OrderID, Sum = o.OrderDetails.Average(od => (float?)od.Discount) }),
                e => e.OrderID);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderID""] < 10250))");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task Min_with_no_arg(bool async)
        {
            await base.Min_with_no_arg(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task Min_with_arg(bool async)
        {
            await base.Min_with_arg(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalFact(Skip = "Issue#16146")]
        public override void Min_no_data_nullable()
        {
        }

        [ConditionalFact(Skip = "Issue#16146")]
        public override void Min_no_data_cast_to_nullable()
        {
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task Min_with_coalesce(bool async)
        {
            await base.Min_with_coalesce(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Product"") AND (c[""ProductID""] < 40))");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task Min_over_subquery_is_client_eval(bool async)
        {
            await AssertMin(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI"),
                selector: c => c.Orders.Sum(o => o.OrderID));

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task Min_over_nested_subquery_is_client_eval(bool async)
        {
            await AssertMin(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "CENTC"),
                selector: c =>
                    c.Orders.Min(o => 5 + o.OrderDetails.Where(od => od.OrderID > 10250 && od.OrderID < 10300).Min(od => od.ProductID)));

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""CENTC""))");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task Min_over_max_subquery_is_client_eval(bool async)
        {
            await AssertMin(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "CENTC"),
                selector: c =>
                    c.Orders.Min(o => 5 + o.OrderDetails.Where(od => od.OrderID > 10250 && od.OrderID < 10300).Max(od => od.ProductID)));

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""CENTC""))");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task Max_with_no_arg(bool async)
        {
            await base.Max_with_no_arg(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task Max_with_arg(bool async)
        {
            await base.Max_with_arg(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task Max_with_coalesce(bool async)
        {
            await base.Max_with_coalesce(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Product"") AND (c[""ProductID""] < 40))");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task Max_over_subquery_is_client_eval(bool async)
        {
            await AssertMax(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI"),
                selector: c => c.Orders.Sum(o => o.OrderID));

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task Max_over_nested_subquery_is_client_eval(bool async)
        {
            await AssertMax(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "CENTC"),
                selector: c =>
                    c.Orders.Max(o => 5 + o.OrderDetails.Where(od => od.OrderID > 10250 && od.OrderID < 10300).Max(od => od.ProductID)));

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""CENTC""))");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task Max_over_sum_subquery_is_client_eval(bool async)
        {
            await AssertMax(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "CENTC"),
                selector: c =>
                    c.Orders.Max(o => 5 + o.OrderDetails.Where(od => od.OrderID > 10250 && od.OrderID < 10300).Sum(od => od.ProductID)));

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""CENTC""))");
        }

        [ConditionalTheory]
        public override async Task Count_with_no_predicate(bool async)
        {
            await base.Count_with_no_predicate(async);

            AssertSql(
                @"SELECT COUNT(1) AS c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task Count_with_predicate(bool async)
        {
            await base.Count_with_predicate(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Count_with_order_by(bool async)
        {
            return base.Count_with_order_by(async);
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task Where_OrderBy_Count(bool async)
        {
            await base.Where_OrderBy_Count(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task OrderBy_Where_Count(bool async)
        {
            await base.OrderBy_Where_Count(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task OrderBy_Count_with_predicate(bool async)
        {
            await base.OrderBy_Count_with_predicate(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task OrderBy_Where_Count_with_predicate(bool async)
        {
            await base.OrderBy_Where_Count_with_predicate(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (((c[""Discriminator""] = ""Order"") AND (c[""OrderID""] > 10)) AND (c[""CustomerID""] != ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task Where_OrderBy_Count_client_eval(bool async)
        {
            await base.Where_OrderBy_Count_client_eval(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        //        public override async Task Where_OrderBy_Count_client_eval_mixed(bool async)
        //        {
        //            await base.Where_OrderBy_Count_client_eval_mixed(async);

        //            AssertSql(
        //                @"SELECT c
        //FROM root c
        //WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderID""] > 10))");
        //        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task OrderBy_Where_Count_client_eval(bool async)
        {
            await base.OrderBy_Where_Count_client_eval(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task OrderBy_Where_Count_client_eval_mixed(bool async)
        {
            await base.OrderBy_Where_Count_client_eval_mixed(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task OrderBy_Count_with_predicate_client_eval(bool async)
        {
            await base.OrderBy_Count_with_predicate_client_eval(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task OrderBy_Count_with_predicate_client_eval_mixed(bool async)
        {
            await base.OrderBy_Count_with_predicate_client_eval_mixed(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task OrderBy_Where_Count_with_predicate_client_eval(bool async)
        {
            await base.OrderBy_Where_Count_with_predicate_client_eval(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override async Task OrderBy_Where_Count_with_predicate_client_eval_mixed(bool async)
        {
            await base.OrderBy_Where_Count_with_predicate_client_eval_mixed(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task OrderBy_client_Take(bool async)
        {
            await base.OrderBy_client_Take(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue#16144")]
        public override async Task Distinct(bool async)
        {
            await base.Distinct(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#16144")]
        public override async Task Distinct_Scalar(bool async)
        {
            await base.Distinct_Scalar(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#16144")]
        public override async Task OrderBy_Distinct(bool async)
        {
            await base.OrderBy_Distinct(async);

            // Ordering not preserved by distinct when ordering columns not projected.
            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#16144")]
        public override async Task Distinct_OrderBy(bool async)
        {
            await base.Distinct_OrderBy(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#16144")]
        public override async Task Distinct_OrderBy2(bool async)
        {
            await base.Distinct_OrderBy2(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#16144")]
        public override async Task Distinct_OrderBy3(bool async)
        {
            await base.Distinct_OrderBy3(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#16144")]
        public override async Task Distinct_Count(bool async)
        {
            await base.Distinct_Count(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#16144")]
        public override async Task Select_Select_Distinct_Count(bool async)
        {
            await base.Select_Select_Distinct_Count(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Single_Predicate(bool async)
        {
            await base.Single_Predicate(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))
OFFSET 0 LIMIT 2");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task FirstOrDefault_inside_subquery_gets_server_evaluated(bool async)
        {
            await base.FirstOrDefault_inside_subquery_gets_server_evaluated(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Multiple_collection_navigation_with_FirstOrDefault_chained_projecting_scalar(bool async)
        {
            return base.Multiple_collection_navigation_with_FirstOrDefault_chained_projecting_scalar(async);
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task First_inside_subquery_gets_client_evaluated(bool async)
        {
            await base.First_inside_subquery_gets_client_evaluated(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Last(bool async)
        {
            await base.Last(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")
ORDER BY c[""ContactName""] DESC
OFFSET 0 LIMIT 1");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Last_when_no_order_by(bool async)
        {
            await base.Last_when_no_order_by(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task LastOrDefault_when_no_order_by(bool async)
        {
            await base.LastOrDefault_when_no_order_by(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        public override async Task Last_Predicate(bool async)
        {
            await base.Last_Predicate(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = ""London""))
ORDER BY c[""ContactName""] DESC
OFFSET 0 LIMIT 1");
        }

        public override async Task Where_Last(bool async)
        {
            await base.Where_Last(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = ""London""))
ORDER BY c[""ContactName""] DESC
OFFSET 0 LIMIT 1");
        }

        public override async Task LastOrDefault(bool async)
        {
            await base.LastOrDefault(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")
ORDER BY c[""ContactName""] DESC
OFFSET 0 LIMIT 1");
        }

        public override async Task LastOrDefault_Predicate(bool async)
        {
            await base.LastOrDefault_Predicate(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = ""London""))
ORDER BY c[""ContactName""] DESC
OFFSET 0 LIMIT 1");
        }

        public override async Task Where_LastOrDefault(bool async)
        {
            await base.Where_LastOrDefault(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = ""London""))
ORDER BY c[""ContactName""] DESC
OFFSET 0 LIMIT 1");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Contains_with_subquery(bool async)
        {
            await base.Contains_with_subquery(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Contains_with_local_array_closure(bool async)
        {
            await base.Contains_with_local_array_closure(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND c[""CustomerID""] IN (""ABCDE"", ""ALFKI""))",
                //
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND c[""CustomerID""] IN (""ABCDE""))");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Contains_with_subquery_and_local_array_closure(bool async)
        {
            await base.Contains_with_subquery_and_local_array_closure(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Contains_with_local_uint_array_closure(bool async)
        {
            await base.Contains_with_local_uint_array_closure(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND c[""EmployeeID""] IN (0, 1))",
                //
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND c[""EmployeeID""] IN (0))");
        }

        public override async Task Contains_with_local_nullable_uint_array_closure(bool async)
        {
            await base.Contains_with_local_nullable_uint_array_closure(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND c[""EmployeeID""] IN (0, 1))",
                //
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND c[""EmployeeID""] IN (0))");
        }

        public override async Task Contains_with_local_array_inline(bool async)
        {
            await base.Contains_with_local_array_inline(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND c[""CustomerID""] IN (""ABCDE"", ""ALFKI""))");
        }

        public override async Task Contains_with_local_list_closure(bool async)
        {
            await base.Contains_with_local_list_closure(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND c[""CustomerID""] IN (""ABCDE"", ""ALFKI""))");
        }

        public override async Task Contains_with_local_object_list_closure(bool async)
        {
            await base.Contains_with_local_object_list_closure(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND c[""CustomerID""] IN (""ABCDE"", ""ALFKI""))");
        }

        public override async Task Contains_with_local_list_closure_all_null(bool async)
        {
            await base.Contains_with_local_list_closure_all_null(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = null))");
        }

        public override async Task Contains_with_local_list_inline(bool async)
        {
            await base.Contains_with_local_list_inline(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND c[""CustomerID""] IN (""ABCDE"", ""ALFKI""))");
        }

        public override async Task Contains_with_local_list_inline_closure_mix(bool async)
        {
            await base.Contains_with_local_list_inline_closure_mix(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND c[""CustomerID""] IN (""ABCDE"", ""ALFKI""))",
                //
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND c[""CustomerID""] IN (""ABCDE"", ""ANATR""))");
        }

        public override async Task Contains_with_local_collection_false(bool async)
        {
            await base.Contains_with_local_collection_false(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND NOT(c[""CustomerID""] IN (""ABCDE"", ""ALFKI"")))");
        }

        public override async Task Contains_with_local_collection_complex_predicate_and(bool async)
        {
            await base.Contains_with_local_collection_complex_predicate_and(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (((c[""CustomerID""] = ""ALFKI"") OR (c[""CustomerID""] = ""ABCDE"")) AND c[""CustomerID""] IN (""ABCDE"", ""ALFKI"")))");
        }

        public override async Task Contains_with_local_collection_complex_predicate_or(bool async)
        {
            await base.Contains_with_local_collection_complex_predicate_or(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] IN (""ABCDE"", ""ALFKI"") OR ((c[""CustomerID""] = ""ALFKI"") OR (c[""CustomerID""] = ""ABCDE""))))");
        }

        public override async Task Contains_with_local_collection_complex_predicate_not_matching_ins1(bool async)
        {
            await base.Contains_with_local_collection_complex_predicate_not_matching_ins1(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (((c[""CustomerID""] = ""ALFKI"") OR (c[""CustomerID""] = ""ABCDE"")) OR NOT(c[""CustomerID""] IN (""ABCDE"", ""ALFKI""))))");
        }

        public override async Task Contains_with_local_collection_complex_predicate_not_matching_ins2(bool async)
        {
            await base.Contains_with_local_collection_complex_predicate_not_matching_ins2(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] IN (""ABCDE"", ""ALFKI"") AND ((c[""CustomerID""] != ""ALFKI"") AND (c[""CustomerID""] != ""ABCDE""))))");
        }

        public override async Task Contains_with_local_collection_sql_injection(bool async)
        {
            await base.Contains_with_local_collection_sql_injection(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] IN (""ALFKI"", ""ABC')); GO; DROP TABLE Orders; GO; --"") OR ((c[""CustomerID""] = ""ALFKI"") OR (c[""CustomerID""] = ""ABCDE""))))");
        }

        public override async Task Contains_with_local_collection_empty_closure(bool async)
        {
            await base.Contains_with_local_collection_empty_closure(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (true = false))");
        }

        public override async Task Contains_with_local_collection_empty_inline(bool async)
        {
            await base.Contains_with_local_collection_empty_inline(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND NOT((true = false)))");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Contains_top_level(bool async)
        {
            await base.Contains_top_level(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Contains_with_local_tuple_array_closure(bool async)
        {
            await base.Contains_with_local_tuple_array_closure(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""OrderDetail"")");
        }

        public override async Task Contains_with_local_anonymous_type_array_closure(bool async)
        {
            await base.Contains_with_local_anonymous_type_array_closure(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""OrderDetail"")");
        }

        [ConditionalFact(Skip = "Issue #17246")]
        public override void OfType_Select()
        {
            base.OfType_Select();

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalFact(Skip = "Issue #17246")]
        public override void OfType_Select_OfType_Select()
        {
            base.OfType_Select_OfType_Select();

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Average_with_non_matching_types_in_projection_doesnt_produce_second_explicit_cast(bool async)
        {
            await base.Average_with_non_matching_types_in_projection_doesnt_produce_second_explicit_cast(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Max_with_non_matching_types_in_projection_introduces_explicit_cast(bool async)
        {
            await base.Max_with_non_matching_types_in_projection_introduces_explicit_cast(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Min_with_non_matching_types_in_projection_introduces_explicit_cast(bool async)
        {
            await base.Min_with_non_matching_types_in_projection_introduces_explicit_cast(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task OrderBy_Take_Last_gives_correct_result(bool async)
        {
            await base.OrderBy_Take_Last_gives_correct_result(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task OrderBy_Skip_Last_gives_correct_result(bool async)
        {
            await base.OrderBy_Skip_Last_gives_correct_result(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalFact(Skip = "Issue#17246 (Contains over subquery is not supported)")]
        public override void Contains_over_entityType_should_rewrite_to_identity_equality()
        {
            base.Contains_over_entityType_should_rewrite_to_identity_equality();

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderID""] = 10248))");
        }

        [ConditionalTheory(Skip = "Issue#17246 (Contains over subquery is not supported)")]
        public override async Task List_Contains_over_entityType_should_rewrite_to_identity_equality(bool async)
        {
            await base.List_Contains_over_entityType_should_rewrite_to_identity_equality(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderID""] = 10248))");
        }

        public override async Task List_Contains_with_constant_list(bool async)
        {
            await base.List_Contains_with_constant_list(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND c[""CustomerID""] IN (""ALFKI"", ""ANATR""))");
        }

        public override async Task List_Contains_with_parameter_list(bool async)
        {
            await base.List_Contains_with_parameter_list(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND c[""CustomerID""] IN (""ALFKI"", ""ANATR""))");
        }

        public override async Task Contains_with_parameter_list_value_type_id(bool async)
        {
            await base.Contains_with_parameter_list_value_type_id(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND c[""OrderID""] IN (10248, 10249))");
        }

        public override async Task Contains_with_constant_list_value_type_id(bool async)
        {
            await base.Contains_with_constant_list_value_type_id(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND c[""OrderID""] IN (10248, 10249))");
        }

        public override async Task HashSet_Contains_with_parameter(bool async)
        {
            await base.HashSet_Contains_with_parameter(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND c[""CustomerID""] IN (""ALFKI""))");
        }

        public override async Task ImmutableHashSet_Contains_with_parameter(bool async)
        {
            await base.ImmutableHashSet_Contains_with_parameter(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND c[""CustomerID""] IN (""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue#17246 (Contains over subquery is not supported)")]
        public override Task Contains_over_entityType_with_null_should_rewrite_to_false(bool async)
        {
            return base.Contains_over_entityType_with_null_should_rewrite_to_false(async);
        }

        public override async Task String_FirstOrDefault_in_projection_does_not_do_client_eval(bool async)
        {
            await base.String_FirstOrDefault_in_projection_does_not_do_client_eval(async);

            AssertSql(
                @"SELECT LEFT(c[""CustomerID""], 1) AS c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Project_constant_Sum(bool async)
        {
            await base.Project_constant_Sum(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        public override async Task Where_subquery_any_equals_operator(bool async)
        {
            await base.Where_subquery_any_equals_operator(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND c[""CustomerID""] IN (""ABCDE"", ""ALFKI"", ""ANATR""))");
        }

        public override async Task Where_subquery_any_equals(bool async)
        {
            await base.Where_subquery_any_equals(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND c[""CustomerID""] IN (""ABCDE"", ""ALFKI"", ""ANATR""))");
        }

        public override async Task Where_subquery_any_equals_static(bool async)
        {
            await base.Where_subquery_any_equals_static(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND c[""CustomerID""] IN (""ABCDE"", ""ALFKI"", ""ANATR""))");
        }

        public override async Task Where_subquery_where_any(bool async)
        {
            await base.Where_subquery_where_any(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (((c[""Discriminator""] = ""Customer"") AND (c[""City""] = ""México D.F."")) AND c[""CustomerID""] IN (""ABCDE"", ""ALFKI"", ""ANATR""))",
                //
                @"SELECT c
FROM root c
WHERE (((c[""Discriminator""] = ""Customer"") AND (c[""City""] = ""México D.F."")) AND c[""CustomerID""] IN (""ABCDE"", ""ALFKI"", ""ANATR""))");
        }

        public override async Task Where_subquery_all_not_equals_operator(bool async)
        {
            await base.Where_subquery_all_not_equals_operator(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND NOT(c[""CustomerID""] IN (""ABCDE"", ""ALFKI"", ""ANATR"")))");
        }

        public override async Task Where_subquery_all_not_equals(bool async)
        {
            await base.Where_subquery_all_not_equals(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND NOT(c[""CustomerID""] IN (""ABCDE"", ""ALFKI"", ""ANATR"")))");
        }

        public override async Task Where_subquery_all_not_equals_static(bool async)
        {
            await base.Where_subquery_all_not_equals_static(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND NOT(c[""CustomerID""] IN (""ABCDE"", ""ALFKI"", ""ANATR"")))");
        }

        public override async Task Where_subquery_where_all(bool async)
        {
            await base.Where_subquery_where_all(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (((c[""Discriminator""] = ""Customer"") AND (c[""City""] = ""México D.F."")) AND NOT(c[""CustomerID""] IN (""ABCDE"", ""ALFKI"", ""ANATR"")))",
                //
                @"SELECT c
FROM root c
WHERE (((c[""Discriminator""] = ""Customer"") AND (c[""City""] = ""México D.F."")) AND NOT(c[""CustomerID""] IN (""ABCDE"", ""ALFKI"", ""ANATR"")))");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Cast_to_same_Type_Count_works(bool async)
        {
            await base.Cast_to_same_Type_Count_works(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Cast_before_aggregate_is_preserved(bool async)
        {
            return base.Cast_before_aggregate_is_preserved(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Enumerable_min_is_mapped_to_Queryable_1(bool async)
        {
            return base.Enumerable_min_is_mapped_to_Queryable_1(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Enumerable_min_is_mapped_to_Queryable_2(bool async)
        {
            return base.Enumerable_min_is_mapped_to_Queryable_2(async);
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override Task DefaultIfEmpty_selects_only_required_columns(bool async)
        {
            return base.DefaultIfEmpty_selects_only_required_columns(async);
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override Task Collection_Last_member_access_in_projection_translated(bool async)
        {
            return base.Collection_Last_member_access_in_projection_translated(async);
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override Task Collection_LastOrDefault_member_access_in_projection_translated(bool async)
        {
            return base.Collection_LastOrDefault_member_access_in_projection_translated(async);
        }

        [ConditionalTheory(Skip = "Issue#16146")]
        public override Task Sum_over_explicit_cast_over_column(bool async)
        {
            return base.Sum_over_explicit_cast_over_column(async);
        }

        [ConditionalTheory(Skip = "Issue#17246 (Contains over subquery is not supported)")]
        public override Task Contains_over_scalar_with_null_should_rewrite_to_identity_equality_subquery(bool async)
        {
            return base.Contains_over_scalar_with_null_should_rewrite_to_identity_equality_subquery(async);
        }

        [ConditionalTheory(Skip = "Issue#17246 (Contains over subquery is not supported)")]
        public override Task Contains_over_nullable_scalar_with_null_in_subquery_translated_correctly(bool async)
        {
            return base.Contains_over_nullable_scalar_with_null_in_subquery_translated_correctly(async);
        }

        [ConditionalTheory(Skip = "Issue#17246 (Contains over subquery is not supported)")]
        public override Task Contains_over_non_nullable_scalar_with_null_in_subquery_simplifies_to_false(bool async)
        {
            return base.Contains_over_non_nullable_scalar_with_null_in_subquery_simplifies_to_false(async);
        }

        [ConditionalTheory(Skip = "Issue#17246 (Contains over subquery is not supported)")]
        public override Task Contains_over_entityType_with_null_should_rewrite_to_identity_equality_subquery(bool async)
        {
            return base.Contains_over_entityType_with_null_should_rewrite_to_identity_equality_subquery(async);
        }

        [ConditionalTheory(Skip = "Issue#17246 (Contains over subquery is not supported)")]
        public override Task Contains_over_entityType_with_null_should_rewrite_to_identity_equality_subquery_complex(bool async)
        {
            return base.Contains_over_entityType_with_null_should_rewrite_to_identity_equality_subquery_complex(async);
        }

        [ConditionalTheory(Skip = "Issue#17246 (Contains over subquery is not supported)")]
        public override Task Contains_over_entityType_with_null_should_rewrite_to_identity_equality_subquery_negated(bool async)
        {
            return base.Contains_over_entityType_with_null_should_rewrite_to_identity_equality_subquery_negated(async);
        }

        [ConditionalTheory(Skip = "Issue#17246 (Contains over subquery is not supported)")]
        public override Task Contains_over_entityType_should_materialize_when_composite(bool async)
        {
            return base.Contains_over_entityType_should_materialize_when_composite(async);
        }

        [ConditionalTheory(Skip = "Issue#17246 (Contains over subquery is not supported)")]
        public override Task Contains_over_entityType_should_materialize_when_composite2(bool async)
        {
            return base.Contains_over_entityType_should_materialize_when_composite2(async);
        }

        [ConditionalTheory(Skip = "Issue#17246 (DefaultIfEmpty is not translated)")]
        public override Task Average_after_default_if_empty_does_not_throw(bool isAsync)
        {
            return base.Average_after_default_if_empty_does_not_throw(isAsync);
        }

        [ConditionalTheory(Skip = "Issue#17246 (DefaultIfEmpty is not translated)")]
        public override Task Max_after_default_if_empty_does_not_throw(bool isAsync)
        {
            return base.Max_after_default_if_empty_does_not_throw(isAsync);
        }

        [ConditionalTheory(Skip = "Issue#17246 (DefaultIfEmpty is not translated)")]
        public override Task Min_after_default_if_empty_does_not_throw(bool isAsync)
        {
            return base.Min_after_default_if_empty_does_not_throw(isAsync);
        }

        [ConditionalTheory(Skip = "Issue#20677")]
        public override Task Average_with_unmapped_property_access_throws_meaningful_exception(bool async)
        {
            return base.Average_with_unmapped_property_access_throws_meaningful_exception(async);
        }

        [ConditionalTheory(Skip = "Cross collection join Issue#17246")]
        public override Task Multiple_collection_navigation_with_FirstOrDefault_chained(bool async)
        {
            return base.Multiple_collection_navigation_with_FirstOrDefault_chained(async);
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
