// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public partial class SimpleQueryCosmosTest
    {
        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Join_customers_orders_projection(bool isAsync)
        {
            await base.Join_customers_orders_projection(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Join_customers_orders_entities(bool isAsync)
        {
            await base.Join_customers_orders_entities(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Join_select_many(bool isAsync)
        {
            await AssertQuery(
                isAsync,
                ss => from c in ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI")
                      join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID
                      from e in ss.Set<Employee>()
                      select new
                      {
                          c,
                          o,
                          e
                      },
                e => e.c.CustomerID + " " + e.o.OrderID + " " + e.e.EmployeeID,
                entryCount: 16);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))",
                //
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")",
                //
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")",
                //
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")",
                //
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")",
                //
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")",
                //
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")",
                //
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Client_Join_select_many(bool isAsync)
        {
            await base.Client_Join_select_many(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Join_customers_orders_select(bool isAsync)
        {
            await base.Join_customers_orders_select(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Join_customers_orders_with_subquery(bool isAsync)
        {
            await base.Join_customers_orders_with_subquery(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Join_customers_orders_with_subquery_with_take(bool isAsync)
        {
            await base.Join_customers_orders_with_subquery_with_take(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Join_customers_orders_with_subquery_anonymous_property_method(bool isAsync)
        {
            await base.Join_customers_orders_with_subquery_anonymous_property_method(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Join_customers_orders_with_subquery_anonymous_property_method_with_take(bool isAsync)
        {
            await base.Join_customers_orders_with_subquery_anonymous_property_method_with_take(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Join_customers_orders_with_subquery_predicate(bool isAsync)
        {
            await base.Join_customers_orders_with_subquery_predicate(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Join_customers_orders_with_subquery_predicate_with_take(bool isAsync)
        {
            await base.Join_customers_orders_with_subquery_predicate_with_take(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Join_composite_key(bool isAsync)
        {
            await base.Join_composite_key(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Join_complex_condition(bool isAsync)
        {
            await base.Join_complex_condition(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Join_client_new_expression(bool isAsync)
        {
            await base.Join_client_new_expression(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Join_same_collection_multiple(bool isAsync)
        {
            await base.Join_same_collection_multiple(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Join_same_collection_force_alias_uniquefication(bool isAsync)
        {
            await base.Join_same_collection_force_alias_uniquefication(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_customers_orders(bool isAsync)
        {
            await base.GroupJoin_customers_orders(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_customers_orders_count(bool isAsync)
        {
            await base.GroupJoin_customers_orders_count(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_customers_orders_count_preserves_ordering(bool isAsync)
        {
            await base.GroupJoin_customers_orders_count_preserves_ordering(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND ((c[""CustomerID""] != ""VAFFE"") AND (c[""CustomerID""] != ""DRACD"")))");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_simple(bool isAsync)
        {
            await base.GroupJoin_simple(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_simple2(bool isAsync)
        {
            await base.GroupJoin_simple2(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_simple3(bool isAsync)
        {
            await base.GroupJoin_simple3(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_tracking_groups(bool isAsync)
        {
            await base.GroupJoin_tracking_groups(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_simple_ordering(bool isAsync)
        {
            await base.GroupJoin_simple_ordering(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_simple_subquery(bool isAsync)
        {
            await base.GroupJoin_simple_subquery(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_DefaultIfEmpty(bool isAsync)
        {
            await base.GroupJoin_DefaultIfEmpty(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_DefaultIfEmpty_multiple(bool isAsync)
        {
            await base.GroupJoin_DefaultIfEmpty_multiple(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_DefaultIfEmpty2(bool isAsync)
        {
            await base.GroupJoin_DefaultIfEmpty2(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_DefaultIfEmpty3(bool isAsync)
        {
            await base.GroupJoin_DefaultIfEmpty3(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_Where(bool isAsync)
        {
            await base.GroupJoin_Where(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_Where_OrderBy(bool isAsync)
        {
            await base.GroupJoin_Where_OrderBy(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_DefaultIfEmpty_Where(bool isAsync)
        {
            await base.GroupJoin_DefaultIfEmpty_Where(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Join_GroupJoin_DefaultIfEmpty_Where(bool isAsync)
        {
            await base.Join_GroupJoin_DefaultIfEmpty_Where(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_DefaultIfEmpty_Project(bool isAsync)
        {
            await base.GroupJoin_DefaultIfEmpty_Project(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_with_different_outer_elements_with_same_key(bool isAsync)
        {
            await base.GroupJoin_with_different_outer_elements_with_same_key(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_with_different_outer_elements_with_same_key_with_predicate(bool isAsync)
        {
            await base.GroupJoin_with_different_outer_elements_with_same_key_with_predicate(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderID""] > 11500))");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_with_different_outer_elements_with_same_key_projected_from_another_entity(bool isAsync)
        {
            await base.GroupJoin_with_different_outer_elements_with_same_key_projected_from_another_entity(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""OrderDetail"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_SelectMany_subquery_with_filter(bool isAsync)
        {
            await base.GroupJoin_SelectMany_subquery_with_filter(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_SelectMany_subquery_with_filter_orderby(bool isAsync)
        {
            await base.GroupJoin_SelectMany_subquery_with_filter_orderby(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_SelectMany_subquery_with_filter_and_DefaultIfEmpty(bool isAsync)
        {
            await base.GroupJoin_SelectMany_subquery_with_filter_and_DefaultIfEmpty(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_SelectMany_subquery_with_filter_orderby_and_DefaultIfEmpty(bool isAsync)
        {
            await base.GroupJoin_SelectMany_subquery_with_filter_orderby_and_DefaultIfEmpty(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_with_order_by_key_descending1(bool isAsync)
        {
            await base.GroupJoin_with_order_by_key_descending1(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_with_order_by_key_descending2(bool isAsync)
        {
            await base.GroupJoin_with_order_by_key_descending2(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_outer_projection(bool isAsync)
        {
            await base.GroupJoin_outer_projection(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_outer_projection2(bool isAsync)
        {
            await base.GroupJoin_outer_projection2(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_outer_projection3(bool isAsync)
        {
            await base.GroupJoin_outer_projection3(isAsync);
            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_outer_projection4(bool isAsync)
        {
            await base.GroupJoin_outer_projection4(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_outer_projection_reverse(bool isAsync)
        {
            await base.GroupJoin_outer_projection_reverse(isAsync);
            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")",
                //
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_outer_projection_reverse2(bool isAsync)
        {
            await base.GroupJoin_outer_projection_reverse2(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")",
                //
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_subquery_projection_outer_mixed(bool isAsync)
        {
            await AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI")
                    from o0 in ss.Set<Order>().OrderBy(o => o.OrderID).Take(1)
                    join o1 in ss.Set<Order>() on c.CustomerID equals o1.CustomerID into orders
                    from o2 in orders
                    select new
                    {
                        A = c.CustomerID,
                        B = o0.CustomerID,
                        C = o2.CustomerID
                    },
                e => (e.A, e.B, e.C));

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))",
                //
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")",
                //
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override Task GroupJoin_Subquery_with_Take_Then_SelectMany_Where(bool isAsync)
        {
            return base.GroupJoin_Subquery_with_Take_Then_SelectMany_Where(isAsync);
        }
    }
}
