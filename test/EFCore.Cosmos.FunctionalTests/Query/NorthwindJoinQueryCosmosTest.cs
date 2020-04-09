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
    public class NorthwindJoinQueryCosmosTest : NorthwindJoinQueryTestBase<NorthwindQueryCosmosFixture<NoopModelCustomizer>>
    {
        public NorthwindJoinQueryCosmosTest(
            NorthwindQueryCosmosFixture<NoopModelCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            ClearLog();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Join_customers_orders_projection(bool async)
        {
            await base.Join_customers_orders_projection(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Join_customers_orders_entities(bool async)
        {
            await base.Join_customers_orders_entities(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Join_customers_orders_entities_same_entity_twice(bool async)
        {
            return base.Join_customers_orders_entities_same_entity_twice(async);
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Join_select_many(bool async)
        {
            await AssertQuery(
                async,
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
        public override async Task Client_Join_select_many(bool async)
        {
            await base.Client_Join_select_many(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Join_customers_orders_select(bool async)
        {
            await base.Join_customers_orders_select(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Join_customers_orders_with_subquery(bool async)
        {
            await base.Join_customers_orders_with_subquery(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Join_customers_orders_with_subquery_with_take(bool async)
        {
            await base.Join_customers_orders_with_subquery_with_take(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Join_customers_orders_with_subquery_anonymous_property_method(bool async)
        {
            await base.Join_customers_orders_with_subquery_anonymous_property_method(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Join_customers_orders_with_subquery_anonymous_property_method_with_take(bool async)
        {
            await base.Join_customers_orders_with_subquery_anonymous_property_method_with_take(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Join_customers_orders_with_subquery_predicate(bool async)
        {
            await base.Join_customers_orders_with_subquery_predicate(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Join_customers_orders_with_subquery_predicate_with_take(bool async)
        {
            await base.Join_customers_orders_with_subquery_predicate_with_take(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Join_composite_key(bool async)
        {
            await base.Join_composite_key(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Join_complex_condition(bool async)
        {
            await base.Join_complex_condition(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Join_same_collection_multiple(bool async)
        {
            await base.Join_same_collection_multiple(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Join_same_collection_force_alias_uniquefication(bool async)
        {
            await base.Join_same_collection_force_alias_uniquefication(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override Task GroupJoin_customers_employees_shadow(bool async)
        {
            return base.GroupJoin_customers_employees_shadow(async);
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override Task GroupJoin_customers_employees_subquery_shadow(bool async)
        {
            return base.GroupJoin_customers_employees_subquery_shadow(async);
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override Task GroupJoin_customers_employees_subquery_shadow_take(bool async)
        {
            return base.GroupJoin_customers_employees_subquery_shadow_take(async);
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override Task GroupJoin_projection(bool async)
        {
            return base.GroupJoin_projection(async);
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_simple(bool async)
        {
            await base.GroupJoin_simple(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_simple2(bool async)
        {
            await base.GroupJoin_simple2(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_simple3(bool async)
        {
            await base.GroupJoin_simple3(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_simple_ordering(bool async)
        {
            await base.GroupJoin_simple_ordering(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_simple_subquery(bool async)
        {
            await base.GroupJoin_simple_subquery(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_DefaultIfEmpty(bool async)
        {
            await base.GroupJoin_DefaultIfEmpty(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_DefaultIfEmpty_multiple(bool async)
        {
            await base.GroupJoin_DefaultIfEmpty_multiple(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_DefaultIfEmpty2(bool async)
        {
            await base.GroupJoin_DefaultIfEmpty2(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_DefaultIfEmpty3(bool async)
        {
            await base.GroupJoin_DefaultIfEmpty3(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_Where(bool async)
        {
            await base.GroupJoin_Where(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_Where_OrderBy(bool async)
        {
            await base.GroupJoin_Where_OrderBy(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_DefaultIfEmpty_Where(bool async)
        {
            await base.GroupJoin_DefaultIfEmpty_Where(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Join_GroupJoin_DefaultIfEmpty_Where(bool async)
        {
            await base.Join_GroupJoin_DefaultIfEmpty_Where(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_DefaultIfEmpty_Project(bool async)
        {
            await base.GroupJoin_DefaultIfEmpty_Project(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_SelectMany_subquery_with_filter(bool async)
        {
            await base.GroupJoin_SelectMany_subquery_with_filter(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_SelectMany_subquery_with_filter_orderby(bool async)
        {
            await base.GroupJoin_SelectMany_subquery_with_filter_orderby(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_SelectMany_subquery_with_filter_and_DefaultIfEmpty(bool async)
        {
            await base.GroupJoin_SelectMany_subquery_with_filter_and_DefaultIfEmpty(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_SelectMany_subquery_with_filter_orderby_and_DefaultIfEmpty(bool async)
        {
            await base.GroupJoin_SelectMany_subquery_with_filter_orderby_and_DefaultIfEmpty(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task GroupJoin_subquery_projection_outer_mixed(bool async)
        {
            await AssertQuery(
                async,
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
        public override Task GroupJoin_Subquery_with_Take_Then_SelectMany_Where(bool async)
        {
            return base.GroupJoin_Subquery_with_Take_Then_SelectMany_Where(async);
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override Task Inner_join_with_tautology_predicate_converts_to_cross_join(bool async)
        {
            return base.Inner_join_with_tautology_predicate_converts_to_cross_join(async);
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override Task Left_join_with_tautology_predicate_doesnt_convert_to_cross_join(bool async)
        {
            return base.Left_join_with_tautology_predicate_doesnt_convert_to_cross_join(async);
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
