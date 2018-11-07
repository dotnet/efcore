using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query
{
    public partial class SimpleQueryCosmosTest
    {
        public override async Task Union_with_custom_projection(bool isAsync)
        {
            await base.Union_with_custom_projection(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override void Select_All()
        {
            base.Select_All();

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task Sum_with_no_arg(bool isAsync)
        {
            await base.Sum_with_no_arg(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task Sum_with_binary_expression(bool isAsync)
        {
            await base.Sum_with_binary_expression(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task Sum_with_arg(bool isAsync)
        {
            await base.Sum_with_arg(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task Sum_with_arg_expression(bool isAsync)
        {
            await base.Sum_with_arg_expression(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task Sum_with_division_on_decimal(bool isAsync)
        {
            await base.Sum_with_division_on_decimal(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""OrderDetail"")");
        }

        public override async Task Sum_with_division_on_decimal_no_significant_digits(bool isAsync)
        {
            await base.Sum_with_division_on_decimal_no_significant_digits(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""OrderDetail"")");
        }

        public override async Task Sum_with_coalesce(bool isAsync)
        {
            await base.Sum_with_coalesce(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Product"") AND (c[""ProductID""] < 40))");
        }

        public override async Task Sum_over_subquery_is_client_eval(bool isAsync)
        {
            await AssertSum<Customer, Customer>(
                isAsync,
                cs => cs.Where(c => c.CustomerID == "ALFKI"),
                selector: c => c.Orders.Sum(o => o.OrderID));

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        public override async Task Sum_over_nested_subquery_is_client_eval(bool isAsync)
        {
            await AssertSum<Customer, Customer>(
                isAsync,
                cs => cs.Where(c => c.CustomerID == "CENTC"),
                selector: c => c.Orders.Sum(o => 5 + o.OrderDetails.Where(od => od.OrderID >= 10250 && od.OrderID <= 10300).Sum(od => od.ProductID)));

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""CENTC""))");
        }

        public override async Task Sum_over_min_subquery_is_client_eval(bool isAsync)
        {
            await AssertSum<Customer, Customer>(
                isAsync,
                cs => cs.Where(c => c.CustomerID == "CENTC"),
                selector: c => c.Orders.Sum(o => 5 + o.OrderDetails.Where(od => od.OrderID >= 10250 && od.OrderID <= 10300).Min(od => od.ProductID)));

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""CENTC""))");
        }

        public override async Task Sum_on_float_column(bool isAsync)
        {
            await base.Sum_on_float_column(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND (c[""ProductID""] = 1))");
        }

        public override async Task Sum_on_float_column_in_subquery(bool isAsync)
        {
            await AssertQuery<Order>(
                isAsync,
                os => os.Where(o => o.OrderID < 10250).Select(
                    o => new
                    {
                        o.OrderID,
                        Sum = o.OrderDetails.Sum(od => od.Discount)
                    }),
                e => e.OrderID);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderID""] < 10250))");
        }

        public override async Task Average_with_no_arg(bool isAsync)
        {
            await base.Average_with_no_arg(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task Average_with_binary_expression(bool isAsync)
        {
            await base.Average_with_binary_expression(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task Average_with_arg(bool isAsync)
        {
            await base.Average_with_arg(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task Average_with_arg_expression(bool isAsync)
        {
            await base.Average_with_arg_expression(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task Average_with_division_on_decimal(bool isAsync)
        {
            await base.Average_with_division_on_decimal(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""OrderDetail"")");
        }

        public override async Task Average_with_division_on_decimal_no_significant_digits(bool isAsync)
        {
            await base.Average_with_division_on_decimal_no_significant_digits(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""OrderDetail"")");
        }

        public override async Task Average_with_coalesce(bool isAsync)
        {
            await base.Average_with_coalesce(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Product"") AND (c[""ProductID""] < 40))");
        }

        public override async Task Average_over_subquery_is_client_eval(bool isAsync)
        {
            await AssertAverage<Customer, Customer>(
                isAsync,
                cs => cs.Where(c => c.CustomerID == "ALFKI"),
                selector: c => c.Orders.Where(o => o.OrderID < 10250).Sum(o => o.OrderID));

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        public override async Task Average_over_nested_subquery_is_client_eval(bool isAsync)
        {
            await AssertAverage<Customer, Customer>(
                isAsync,
                cs => cs.Where(c => c.CustomerID == "CENTC"),
                selector: c => (decimal)c.Orders
                    .Average(o => 5 + o.OrderDetails.Where(od => od.OrderID > 10250 && od.OrderID < 10300).Average(od => od.ProductID)));

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""CENTC""))");
        }

        public override async Task Average_over_max_subquery_is_client_eval(bool isAsync)
        {
            await AssertAverage<Customer, Customer>(
                isAsync,
                cs => cs.Where(c => c.CustomerID == "CENTC"),
                selector: c => (decimal)c.Orders
                    .Average(o => 5 + o.OrderDetails.Where(od => od.OrderID > 10250 && od.OrderID < 10300).Max(od => od.ProductID)));

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""CENTC""))");
        }

        public override async Task Average_on_float_column(bool isAsync)
        {
            await base.Average_on_float_column(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND (c[""ProductID""] = 1))");
        }

        public override async Task Average_on_float_column_in_subquery(bool isAsync)
        {
            await AssertQuery<Order>(
                isAsync,
                os => os.Where(o => o.OrderID < 10250).Select(
                    o => new
                    {
                        o.OrderID,
                        Sum = o.OrderDetails.Average(od => od.Discount)
                    }),
                e => e.OrderID);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderID""] < 10250))");
        }

        public override async Task Average_on_float_column_in_subquery_with_cast(bool isAsync)
        {
            await AssertQuery<Order>(
                isAsync,
                os => os.Where(o => o.OrderID < 10250)
                    .Select(
                        o => new
                        {
                            o.OrderID,
                            Sum = o.OrderDetails.Average(od => (float?)od.Discount)
                        }),
                e => e.OrderID);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderID""] < 10250))");
        }

        public override async Task Min_with_no_arg(bool isAsync)
        {
            await base.Min_with_no_arg(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task Min_with_arg(bool isAsync)
        {
            await base.Min_with_arg(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task Min_with_coalesce(bool isAsync)
        {
            await base.Min_with_coalesce(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Product"") AND (c[""ProductID""] < 40))");
        }

        public override async Task Min_over_subquery_is_client_eval(bool isAsync)
        {
            await AssertMin<Customer, Customer>(
                isAsync,
                cs => cs.Where(c => c.CustomerID == "ALFKI"),
                selector: c => c.Orders.Sum(o => o.OrderID));

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        public override async Task Min_over_nested_subquery_is_client_eval(bool isAsync)
        {
            await AssertMin<Customer, Customer>(
                isAsync,
                cs => cs.Where(c => c.CustomerID == "CENTC"),
                selector: c => c.Orders.Min(o => 5 + o.OrderDetails.Where(od => od.OrderID > 10250 && od.OrderID < 10300).Min(od => od.ProductID)));

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""CENTC""))");
        }

        public override async Task Min_over_max_subquery_is_client_eval(bool isAsync)
        {
            await AssertMin<Customer, Customer>(
                isAsync,
                cs => cs.Where(c => c.CustomerID == "CENTC"),
                selector: c => c.Orders.Min(o => 5 + o.OrderDetails.Where(od => od.OrderID > 10250 && od.OrderID < 10300).Max(od => od.ProductID)));

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""CENTC""))");
        }

        public override async Task Max_with_no_arg(bool isAsync)
        {
            await base.Max_with_no_arg(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task Max_with_arg(bool isAsync)
        {
            await base.Max_with_arg(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task Max_with_coalesce(bool isAsync)
        {
            await base.Max_with_coalesce(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Product"") AND (c[""ProductID""] < 40))");
        }

        public override async Task Max_over_subquery_is_client_eval(bool isAsync)
        {
            await AssertMax<Customer, Customer>(
                isAsync,
                cs => cs.Where(c => c.CustomerID == "ALFKI"),
                selector: c => c.Orders.Sum(o => o.OrderID));

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        public override async Task Max_over_nested_subquery_is_client_eval(bool isAsync)
        {
            await AssertMax<Customer, Customer>(
               isAsync,
                cs => cs.Where(c => c.CustomerID == "CENTC"),
               selector: c => c.Orders.Max(o => 5 + o.OrderDetails.Where(od => od.OrderID > 10250 && od.OrderID < 10300).Max(od => od.ProductID)));

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""CENTC""))");
        }

        public override async Task Max_over_sum_subquery_is_client_eval(bool isAsync)
        {
            await AssertMax<Customer, Customer>(
               isAsync,
                cs => cs.Where(c => c.CustomerID == "CENTC"),
               selector: c => c.Orders.Max(o => 5 + o.OrderDetails.Where(od => od.OrderID > 10250 && od.OrderID < 10300).Sum(od => od.ProductID)));

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""CENTC""))");
        }

        public override async Task Count_with_predicate(bool isAsync)
        {
            await base.Count_with_predicate(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        public override async Task Where_OrderBy_Count(bool isAsync)
        {
            await base.Where_OrderBy_Count(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        public override async Task OrderBy_Where_Count(bool isAsync)
        {
            await base.OrderBy_Where_Count(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        public override async Task OrderBy_Count_with_predicate(bool isAsync)
        {
            await base.OrderBy_Count_with_predicate(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        public override async Task OrderBy_Where_Count_with_predicate(bool isAsync)
        {
            await base.OrderBy_Where_Count_with_predicate(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (((c[""Discriminator""] = ""Order"") AND (c[""OrderID""] > 10)) AND (c[""CustomerID""] != ""ALFKI""))");
        }

        public override async Task Where_OrderBy_Count_client_eval(bool isAsync)
        {
            await base.Where_OrderBy_Count_client_eval(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task Where_OrderBy_Count_client_eval_mixed(bool isAsync)
        {
            await base.Where_OrderBy_Count_client_eval_mixed(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderID""] > 10))");
        }

        public override async Task OrderBy_Where_Count_client_eval(bool isAsync)
        {
            await base.OrderBy_Where_Count_client_eval(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task OrderBy_Where_Count_client_eval_mixed(bool isAsync)
        {
            await base.OrderBy_Where_Count_client_eval_mixed(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task OrderBy_Count_with_predicate_client_eval(bool isAsync)
        {
            await base.OrderBy_Count_with_predicate_client_eval(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task OrderBy_Count_with_predicate_client_eval_mixed(bool isAsync)
        {
            await base.OrderBy_Count_with_predicate_client_eval_mixed(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task OrderBy_Where_Count_with_predicate_client_eval(bool isAsync)
        {
            await base.OrderBy_Where_Count_with_predicate_client_eval(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task OrderBy_Where_Count_with_predicate_client_eval_mixed(bool isAsync)
        {
            await base.OrderBy_Where_Count_with_predicate_client_eval_mixed(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task OrderBy_client_Take(bool isAsync)
        {
            await base.OrderBy_client_Take(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        public override async Task Distinct(bool isAsync)
        {
            await base.Distinct(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Distinct_Scalar(bool isAsync)
        {
            await base.Distinct_Scalar(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task OrderBy_Distinct(bool isAsync)
        {
            await base.OrderBy_Distinct(isAsync);

            // Ordering not preserved by distinct when ordering columns not projected.
            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Distinct_OrderBy(bool isAsync)
        {
            await base.Distinct_OrderBy(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Distinct_OrderBy2(bool isAsync)
        {
            await base.Distinct_OrderBy2(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Distinct_OrderBy3(bool isAsync)
        {
            await base.Distinct_OrderBy3(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Distinct_Count(bool isAsync)
        {
            await base.Distinct_Count(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Select_Select_Distinct_Count(bool isAsync)
        {
            await base.Select_Select_Distinct_Count(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Single_Predicate(bool isAsync)
        {
            await base.Single_Predicate(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        public override async Task FirstOrDefault_inside_subquery_gets_server_evaluated(bool isAsync)
        {
            await base.FirstOrDefault_inside_subquery_gets_server_evaluated(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task First_inside_subquery_gets_client_evaluated(bool isAsync)
        {
            await base.First_inside_subquery_gets_client_evaluated(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Last(bool isAsync)
        {
            await base.Last(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Last_when_no_order_by(bool isAsync)
        {
            await base.Last_when_no_order_by(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        public override async Task Last_Predicate(bool isAsync)
        {
            await base.Last_Predicate(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Where_Last(bool isAsync)
        {
            await base.Where_Last(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task LastOrDefault(bool isAsync)
        {
            await base.LastOrDefault(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task LastOrDefault_Predicate(bool isAsync)
        {
            await base.LastOrDefault_Predicate(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Where_LastOrDefault(bool isAsync)
        {
            await base.Where_LastOrDefault(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Contains_with_subquery(bool isAsync)
        {
            await base.Contains_with_subquery(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Contains_with_local_array_closure(bool isAsync)
        {
            await base.Contains_with_local_array_closure(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Contains_with_subquery_and_local_array_closure(bool isAsync)
        {
            await base.Contains_with_subquery_and_local_array_closure(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Contains_with_local_int_array_closure(bool isAsync)
        {
            await base.Contains_with_local_int_array_closure(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        public override async Task Contains_with_local_nullable_int_array_closure(bool isAsync)
        {
            await base.Contains_with_local_nullable_int_array_closure(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        public override async Task Contains_with_local_array_inline(bool isAsync)
        {
            await base.Contains_with_local_array_inline(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Contains_with_local_list_closure(bool isAsync)
        {
            await base.Contains_with_local_list_closure(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Contains_with_local_list_closure_all_null(bool isAsync)
        {
            await base.Contains_with_local_list_closure_all_null(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Contains_with_local_list_inline(bool isAsync)
        {
            await base.Contains_with_local_list_inline(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Contains_with_local_list_inline_closure_mix(bool isAsync)
        {
            await base.Contains_with_local_list_inline_closure_mix(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Contains_with_local_collection_false(bool isAsync)
        {
            await base.Contains_with_local_collection_false(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Contains_with_local_collection_complex_predicate_and(bool isAsync)
        {
            await base.Contains_with_local_collection_complex_predicate_and(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Contains_with_local_collection_complex_predicate_or(bool isAsync)
        {
            await base.Contains_with_local_collection_complex_predicate_or(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Contains_with_local_collection_complex_predicate_not_matching_ins1(bool isAsync)
        {
            await base.Contains_with_local_collection_complex_predicate_not_matching_ins1(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Contains_with_local_collection_complex_predicate_not_matching_ins2(bool isAsync)
        {
            await base.Contains_with_local_collection_complex_predicate_not_matching_ins2(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Contains_with_local_collection_sql_injection(bool isAsync)
        {
            await base.Contains_with_local_collection_sql_injection(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Contains_with_local_collection_empty_closure(bool isAsync)
        {
            await base.Contains_with_local_collection_empty_closure(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Contains_with_local_collection_empty_inline(bool isAsync)
        {
            await base.Contains_with_local_collection_empty_inline(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Contains_top_level(bool isAsync)
        {
            await base.Contains_top_level(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Contains_with_local_tuple_array_closure(bool isAsync)
        {
            await base.Contains_with_local_tuple_array_closure(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""OrderDetail"")");
        }

        public override async Task Contains_with_local_anonymous_type_array_closure(bool isAsync)
        {
            await base.Contains_with_local_anonymous_type_array_closure(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""OrderDetail"")");
        }

        public override void OfType_Select()
        {
            base.OfType_Select();

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override void OfType_Select_OfType_Select()
        {
            base.OfType_Select_OfType_Select();

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task Average_with_non_matching_types_in_projection_doesnt_produce_second_explicit_cast(bool isAsync)
        {
            await base.Average_with_non_matching_types_in_projection_doesnt_produce_second_explicit_cast(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task Max_with_non_matching_types_in_projection_introduces_explicit_cast(bool isAsync)
        {
            await base.Max_with_non_matching_types_in_projection_introduces_explicit_cast(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task Min_with_non_matching_types_in_projection_introduces_explicit_cast(bool isAsync)
        {
            await base.Min_with_non_matching_types_in_projection_introduces_explicit_cast(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task OrderBy_Take_Last_gives_correct_result(bool isAsync)
        {
            await base.OrderBy_Take_Last_gives_correct_result(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task OrderBy_Skip_Last_gives_correct_result(bool isAsync)
        {
            await base.OrderBy_Skip_Last_gives_correct_result(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override void Contains_over_entityType_should_rewrite_to_identity_equality()
        {
            base.Contains_over_entityType_should_rewrite_to_identity_equality();

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderID""] = 10248))");
        }

        public override void Contains_over_entityType_should_materialize_when_composite()
        {
            base.Contains_over_entityType_should_materialize_when_composite();

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND ((c[""OrderID""] = 10248) AND (c[""ProductID""] = 42)))");
        }

        public override void Paging_operation_on_string_doesnt_issue_warning()
        {
            base.Paging_operation_on_string_doesnt_issue_warning();

            Assert.DoesNotContain(
                CoreStrings.LogFirstWithoutOrderByAndFilter.GenerateMessage(
                    "(from char <generated>_1 in [c].CustomerID select [<generated>_1]).FirstOrDefault()"),
                Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));
        }

        public override async Task Project_constant_Sum(bool isAsync)
        {
            await base.Project_constant_Sum(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        public override async Task Where_subquery_any_equals_operator(bool isAsync)
        {
            await base.Where_subquery_any_equals_operator(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Where_subquery_any_equals(bool isAsync)
        {
            await base.Where_subquery_any_equals(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Where_subquery_any_equals_static(bool isAsync)
        {
            await base.Where_subquery_any_equals_static(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Where_subquery_where_any(bool isAsync)
        {
            await base.Where_subquery_where_any(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = ""México D.F.""))");
        }

        public override async Task Where_subquery_all_not_equals_operator(bool isAsync)
        {
            await base.Where_subquery_all_not_equals_operator(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Where_subquery_all_not_equals(bool isAsync)
        {
            await base.Where_subquery_all_not_equals(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Where_subquery_all_not_equals_static(bool isAsync)
        {
            await base.Where_subquery_all_not_equals_static(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Where_subquery_where_all(bool isAsync)
        {
            await base.Where_subquery_where_all(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = ""México D.F.""))");
        }

        public override async Task Cast_to_same_Type_Count_works(bool isAsync)
        {
            await base.Cast_to_same_Type_Count_works(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }
    }
}
