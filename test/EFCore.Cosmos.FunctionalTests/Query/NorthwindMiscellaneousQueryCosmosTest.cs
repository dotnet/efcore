// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

#pragma warning disable RCS1202 // Avoid NullReferenceException.

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindMiscellaneousQueryCosmosTest : NorthwindMiscellaneousQueryTestBase<
        NorthwindQueryCosmosFixture<NoopModelCustomizer>>
    {
        public NorthwindMiscellaneousQueryCosmosTest(
            NorthwindQueryCosmosFixture<NoopModelCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            ClearLog();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Simple_IQueryable(bool async)
        {
            await AssertQuery(async, ss => ss.Set<Customer>(), entryCount: 91);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalFact(Skip = "Issue #17246")]
        public override void Shaper_command_caching_when_parameter_names_different()
        {
            base.Shaper_command_caching_when_parameter_names_different();

            AssertSql(
                @"SELECT c
FROM root c
WHERE (((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI"")) AND true)",
                //
                @"SELECT c
FROM root c
WHERE (((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI"")) AND true)");
        }

        [ConditionalFact(Skip = "Issue #17246")]
        public override void Lifting_when_subquery_nested_order_by_anonymous()
        {
            base.Lifting_when_subquery_nested_order_by_anonymous();

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalFact(Skip = "Issue #17246")]
        public override void Lifting_when_subquery_nested_order_by_simple()
        {
            base.Lifting_when_subquery_nested_order_by_simple();

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task Local_dictionary(bool async)
        {
            await base.Local_dictionary(async);

            AssertSql(
                @"@__p_0='ALFKI'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = @__p_0))
OFFSET 0 LIMIT 2");
        }

        public override async Task Entity_equality_self(bool async)
        {
            await base.Entity_equality_self(async);

            AssertSql(
                @"SELECT c[""CustomerID""]
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = c[""CustomerID""]))");
        }

        public override async Task Entity_equality_local(bool async)
        {
            await base.Entity_equality_local(async);

            AssertSql(
                @"@__entity_equality_local_0_CustomerID='ANATR'

SELECT c[""CustomerID""]
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = @__entity_equality_local_0_CustomerID))");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Entity_equality_local_composite_key(bool async)
        {
            await base.Entity_equality_local_composite_key(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Join_with_entity_equality_local_on_both_sources(bool async)
        {
            await base.Join_with_entity_equality_local_on_both_sources(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Entity_equality_local_inline(bool async)
        {
            await base.Entity_equality_local_inline(async);

            AssertSql(
                @"SELECT c[""CustomerID""]
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ANATR""))");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Entity_equality_local_inline_composite_key(bool async)
        {
            await base.Entity_equality_local_inline_composite_key(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Entity_equality_null(bool async)
        {
            await base.Entity_equality_null(async);

            AssertSql(
                @"SELECT c[""CustomerID""]
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = null))");
        }

        public override async Task Entity_equality_not_null(bool async)
        {
            await base.Entity_equality_not_null(async);

            AssertSql(
                @"SELECT c[""CustomerID""]
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] != null))");
        }

        [ConditionalFact(Skip = "Issue #17246")]
        public override void Query_when_evaluatable_queryable_method_call_with_repository()
        {
            using (var context = CreateContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                var customerRepository = new Repository<Customer>(context);
                var orderRepository = new Repository<Order>(context);

                var results
                    = customerRepository.Find().Where(c => c.CustomerID == "ALFKI")
                        .Where(c => orderRepository.Find().Any(o => o.CustomerID == c.CustomerID))
                        .ToList();

                Assert.Single(results);

                results
                    = (from c in customerRepository.Find().Where(c => c.CustomerID == "ALFKI")
                       where orderRepository.Find().Any(o => o.CustomerID == c.CustomerID)
                       select c)
                    .ToList();

                Assert.Single(results);

                var orderQuery = orderRepository.Find();

                results = customerRepository.Find().Where(c => c.CustomerID == "ALFKI")
                    .Where(c => orderQuery.Any(o => o.CustomerID == c.CustomerID))
                    .ToList();

                Assert.Single(results);

                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
            }

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Queryable_reprojection(bool async)
        {
            await base.Queryable_reprojection(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Default_if_empty_top_level(bool async)
        {
            await base.Default_if_empty_top_level(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND (c[""EmployeeID""] = 4294967295))");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Join_with_default_if_empty_on_both_sources(bool async)
        {
            await base.Join_with_default_if_empty_on_both_sources(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Default_if_empty_top_level_followed_by_projecting_constant(bool async)
        {
            await base.Default_if_empty_top_level_followed_by_projecting_constant(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Default_if_empty_top_level_positive(bool async)
        {
            await base.Default_if_empty_top_level_positive(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND (c[""EmployeeID""] > 0))");
        }

        [ConditionalTheory(Skip = "Issue #17783")]
        public override Task Default_if_empty_top_level_arg(bool async)
        {
            return base.Default_if_empty_top_level_arg(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Default_if_empty_top_level_arg_followed_by_projecting_constant(bool async)
        {
            return base.Default_if_empty_top_level_arg_followed_by_projecting_constant(async);
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Default_if_empty_top_level_projection(bool async)
        {
            await base.Default_if_empty_top_level_projection(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND (c[""EmployeeID""] = 4294967295))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_query_composition(bool async)
        {
            await base.Where_query_composition(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_query_composition_is_null(bool async)
        {
            await base.Where_query_composition_is_null(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_query_composition_is_not_null(bool async)
        {
            await base.Where_query_composition_is_null(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_query_composition_entity_equality_one_element_SingleOrDefault(bool async)
        {
            await base.Where_query_composition_entity_equality_one_element_SingleOrDefault(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Where_query_composition_entity_equality_one_element_Single(bool async)
        {
            return base.Where_query_composition_entity_equality_one_element_Single(async);
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Where_query_composition_entity_equality_one_element_FirstOrDefault(bool async)
        {
            await base.Where_query_composition_entity_equality_one_element_FirstOrDefault(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Where_query_composition_entity_equality_one_element_First(bool async)
        {
            return base.Where_query_composition_entity_equality_one_element_First(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_query_composition_entity_equality_no_elements_SingleOrDefault(bool async)
        {
            await base.Where_query_composition_entity_equality_no_elements_SingleOrDefault(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Where_query_composition_entity_equality_no_elements_Single(bool async)
        {
            return base.Where_query_composition_entity_equality_no_elements_Single(async);
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Where_query_composition_entity_equality_no_elements_FirstOrDefault(bool async)
        {
            await base.Where_query_composition_entity_equality_no_elements_FirstOrDefault(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Where_query_composition_entity_equality_no_elements_First(bool async)
        {
            return base.Where_query_composition_entity_equality_no_elements_First(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Where_query_composition_entity_equality_multiple_elements_SingleOrDefault(bool async)
        {
            return base.Where_query_composition_entity_equality_multiple_elements_SingleOrDefault(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Where_query_composition_entity_equality_multiple_elements_Single(bool async)
        {
            return base.Where_query_composition_entity_equality_multiple_elements_Single(async);
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Where_query_composition_entity_equality_multiple_elements_FirstOrDefault(bool async)
        {
            await base.Where_query_composition_entity_equality_multiple_elements_FirstOrDefault(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Where_query_composition_entity_equality_multiple_elements_First(bool async)
        {
            return base.Where_query_composition_entity_equality_multiple_elements_First(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_query_composition2(bool async)
        {
            await base.Where_query_composition2(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_query_composition2_FirstOrDefault(bool async)
        {
            await base.Where_query_composition2_FirstOrDefault(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_query_composition2_FirstOrDefault_with_anonymous(bool async)
        {
            await base.Where_query_composition2_FirstOrDefault_with_anonymous(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalFact(Skip = "Cross collection join Issue#17246")]
        public override void Select_Subquery_Single()
        {
            base.Select_Subquery_Single();

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""OrderDetail"")");
        }

        [ConditionalFact(Skip = "Issue #17246")]
        public override void Select_Where_Subquery_Deep_Single()
        {
            base.Select_Where_Subquery_Deep_Single();

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND (c[""OrderID""] = 10344))");
        }

        [ConditionalFact(Skip = "Issue #17246")]
        public override void Select_Where_Subquery_Deep_First()
        {
            base.Select_Where_Subquery_Deep_First();

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""OrderDetail"")");
        }

        [ConditionalFact(Skip = "Issue #17246")]
        public override void Select_Where_Subquery_Equality()
        {
            base.Select_Where_Subquery_Equality();

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_subquery_anon(bool async)
        {
            await base.Where_subquery_anon(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_subquery_anon_nested(bool async)
        {
            await base.Where_subquery_anon_nested(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_SelectMany(bool async)
        {
            await AssertQuery(
                async,
                ss =>
                    from c in ss.Set<Customer>().Where(c => c.CustomerID == "VINET")
                    from o in ss.Set<Order>().OrderBy(o => o.OrderID).Take(3)
                    where c.CustomerID == o.CustomerID
                    select new { c.ContactName, o.OrderID },
                ss =>
                    ss.Set<Customer>().Where(c => c.CustomerID == "VINET")
                        .SelectMany(
                            _ => ss.Set<Order>().OrderBy(o => o.OrderID).Take(3),
                            (c, o) => new { c, o }).Where(t => t.c.CustomerID == t.o.CustomerID)
                        .Select(
                            t => new { t.c.ContactName, t.o.OrderID }),
                assertOrder: true);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""VINET""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Let_any_subquery_anonymous(bool async)
        {
            await base.Let_any_subquery_anonymous(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_arithmetic(bool async)
        {
            await base.OrderBy_arithmetic(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_condition_comparison(bool async)
        {
            await base.OrderBy_condition_comparison(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Product"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_ternary_conditions(bool async)
        {
            await base.OrderBy_ternary_conditions(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Product"")");
        }

        [ConditionalFact(Skip = "Issue #17246")]
        public override void OrderBy_any()
        {
            base.OrderBy_any();

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Skip(bool async)
        {
            await base.Skip(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Skip_no_orderby(bool async)
        {
            await base.Skip_no_orderby(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Skip_Take(bool async)
        {
            await base.Skip_Take(async);

            AssertSql(
                @"@__p_0='5'
@__p_1='10'

SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")
ORDER BY c[""ContactName""]
OFFSET @__p_0 LIMIT @__p_1");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Join_Customers_Orders_Skip_Take(bool async)
        {
            await base.Join_Customers_Orders_Skip_Take(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Join_Customers_Orders_Skip_Take_followed_by_constant_projection(bool async)
        {
            await base.Join_Customers_Orders_Skip_Take_followed_by_constant_projection(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Join_Customers_Orders_Projection_With_String_Concat_Skip_Take(bool async)
        {
            await base.Join_Customers_Orders_Projection_With_String_Concat_Skip_Take(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Join_Customers_Orders_Orders_Skip_Take_Same_Properties(bool async)
        {
            await base.Join_Customers_Orders_Orders_Skip_Take_Same_Properties(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Take_Skip(bool async)
        {
            await base.Take_Skip(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Take_Skip_Distinct(bool async)
        {
            await base.Take_Skip_Distinct(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Take_Skip_Distinct_Caching(bool async)
        {
            await base.Take_Skip_Distinct_Caching(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Take_Distinct_Count(bool async)
        {
            await base.Take_Distinct_Count(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Take_Where_Distinct_Count(bool async)
        {
            await base.Take_Where_Distinct_Count(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""CustomerID""] = ""FRANK""))");
        }

        public override async Task Queryable_simple(bool async)
        {
            await base.Queryable_simple(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Queryable_simple_anonymous(bool async)
        {
            await base.Queryable_simple_anonymous(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Queryable_nested_simple(bool async)
        {
            await base.Queryable_nested_simple(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Queryable_simple_anonymous_projection_subquery(bool async)
        {
            await base.Queryable_simple_anonymous_projection_subquery(async);

            AssertSql(
                @"@__p_0='91'

SELECT c[""City""]
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")
OFFSET 0 LIMIT @__p_0");
        }

        public override async Task Queryable_simple_anonymous_subquery(bool async)
        {
            await base.Queryable_simple_anonymous_subquery(async);

            AssertSql(
                @"@__p_0='91'

SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")
OFFSET 0 LIMIT @__p_0");
        }

        public override async Task Take_simple(bool async)
        {
            await base.Take_simple(async);

            AssertSql(
                @"@__p_0='10'

SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")
ORDER BY c[""CustomerID""]
OFFSET 0 LIMIT @__p_0");
        }

        public override async Task Take_simple_parameterized(bool async)
        {
            await base.Take_simple_parameterized(async);

            AssertSql(
                @"@__p_0='10'

SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")
ORDER BY c[""CustomerID""]
OFFSET 0 LIMIT @__p_0");
        }

        public override async Task Take_simple_projection(bool async)
        {
            await base.Take_simple_projection(async);

            AssertSql(
                @"@__p_0='10'

SELECT c[""City""]
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")
ORDER BY c[""CustomerID""]
OFFSET 0 LIMIT @__p_0");
        }

        public override async Task Take_subquery_projection(bool async)
        {
            await base.Take_subquery_projection(async);

            AssertSql(
                @"@__p_0='2'

SELECT c[""City""]
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")
ORDER BY c[""CustomerID""]
OFFSET 0 LIMIT @__p_0");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_Take_Count(bool async)
        {
            await base.OrderBy_Take_Count(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Take_OrderBy_Count(bool async)
        {
            await base.Take_OrderBy_Count(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Any_simple(bool async)
        {
            await base.Any_simple(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Any_predicate(bool async)
        {
            await base.Any_predicate(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Any_nested_negated(bool async)
        {
            await base.Any_nested_negated(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Any_nested_negated2(bool async)
        {
            await base.Any_nested_negated2(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Any_nested_negated3(bool async)
        {
            await base.Any_nested_negated3(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Any_nested(bool async)
        {
            await base.Any_nested(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Any_nested2(bool async)
        {
            await base.Any_nested2(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Any_nested3(bool async)
        {
            await base.Any_nested3(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalFact(Skip = "Issue #17246")]
        public override void Any_with_multiple_conditions_still_uses_exists()
        {
            base.Any_with_multiple_conditions_still_uses_exists();

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task All_top_level(bool async)
        {
            await base.All_top_level(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task All_top_level_column(bool async)
        {
            await base.All_top_level_column(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task All_top_level_subquery(bool async)
        {
            await AssertSingleResult(
                async,
                syncQuery: ss => ss.Set<Customer>()
                    .All(
                        c1 => c1.CustomerID == "ALFKI"
                            && ss.Set<Customer>().Any(c2 => ss.Set<Customer>().Any(c3 => c1.CustomerID == c3.CustomerID))),
                asyncQuery: ss => ss.Set<Customer>()
                    .AllAsync(
                        c1 => c1.CustomerID == "ALFKI"
                            && ss.Set<Customer>().Any(c2 => ss.Set<Customer>().Any(c3 => c1.CustomerID == c3.CustomerID)),
                        default));

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task All_top_level_subquery_ef_property(bool async)
        {
            await AssertSingleResult(
                async,
                syncQuery: ss => ss.Set<Customer>()
                    .All(
                        c1 => c1.CustomerID == "ALFKI"
                            && ss.Set<Customer>()
                                .Any(
                                    c2 => ss.Set<Customer>()
                                        .Any(c3 => EF.Property<string>(c1, "CustomerID") == c3.CustomerID))),
                asyncQuery: ss => ss.Set<Customer>()
                    .AllAsync(
                        c1 => c1.CustomerID == "ALFKI"
                            && ss.Set<Customer>()
                                .Any(
                                    c2 => ss.Set<Customer>()
                                        .Any(c3 => EF.Property<string>(c1, "CustomerID") == c3.CustomerID)),
                        default));

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task First_client_predicate(bool async)
        {
            await base.First_client_predicate(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_select_many_or(bool async)
        {
            await base.Where_select_many_or(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_select_many_or2(bool async)
        {
            await base.Where_select_many_or2(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_select_many_or3(bool async)
        {
            await base.Where_select_many_or3(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_select_many_or4(bool async)
        {
            await base.Where_select_many_or4(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_select_many_or_with_parameter(bool async)
        {
            await base.Where_select_many_or_with_parameter(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task SelectMany_mixed(bool async)
        {
            await base.SelectMany_mixed(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task SelectMany_simple_subquery(bool async)
        {
            await base.SelectMany_simple_subquery(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task SelectMany_simple1(bool async)
        {
            await base.SelectMany_simple1(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task SelectMany_simple2(bool async)
        {
            await AssertQuery(
                async,
                ss =>
                    from e1 in ss.Set<Employee>().Where(ct => ct.City == "London")
                    from c in ss.Set<Customer>().Where(ct => ct.City == "London")
                    from e2 in ss.Set<Employee>().Where(ct => ct.City == "London")
                    select new
                    {
                        e1,
                        c,
                        e2.FirstName
                    },
                e => (e.e1.EmployeeID, e.c.CustomerID, e.FirstName),
                entryCount: 10);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND (c[""City""] = ""London""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task SelectMany_entity_deep(bool async)
        {
            await AssertQuery(
                async,
                ss => from e1 in ss.Set<Employee>().Where(e => e.EmployeeID == 1)
                      from e2 in ss.Set<Employee>()
                      from e3 in ss.Set<Employee>()
                      from e4 in ss.Set<Employee>()
                      select new
                      {
                          e2,
                          e3,
                          e1,
                          e4
                      },
                e => (e.e2.EmployeeID, e.e3.EmployeeID, e.e1.EmployeeID, e.e4.EmployeeID),
                entryCount: 9);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND (c[""EmployeeID""] = 1))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task SelectMany_projection1(bool async)
        {
            await base.SelectMany_projection1(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task SelectMany_projection2(bool async)
        {
            await base.SelectMany_projection2(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task SelectMany_customer_orders(bool async)
        {
            await AssertQuery(
                async,
                ss =>
                    from c in ss.Set<Customer>().Where(ct => ct.City == "London")
                    from o in ss.Set<Order>()
                    where c.CustomerID == o.CustomerID
                    select new { c.ContactName, o.OrderID },
                e => (e.ContactName, e.OrderID));

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = ""London""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task SelectMany_Count(bool async)
        {
            await AssertCount(
                async,
                ss => from c in ss.Set<Customer>().Where(ct => ct.City == "London")
                      from o in ss.Set<Order>()
                      select c.CustomerID);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = ""London""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task SelectMany_LongCount(bool async)
        {
            await AssertLongCount(
                async,
                ss => from c in ss.Set<Customer>().Where(ct => ct.City == "London")
                      from o in ss.Set<Order>()
                      select c.CustomerID);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = ""London""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task SelectMany_OrderBy_ThenBy_Any(bool async)
        {
            await base.SelectMany_OrderBy_ThenBy_Any(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Join_Where_Count(bool async)
        {
            await base.Join_Where_Count(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_Join_Any(bool async)
        {
            await base.Where_Join_Any(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_Join_Exists(bool async)
        {
            await base.Where_Join_Exists(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_Join_Exists_Inequality(bool async)
        {
            await base.Where_Join_Exists_Inequality(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_Join_Exists_Constant(bool async)
        {
            await base.Where_Join_Exists_Constant(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_Join_Not_Exists(bool async)
        {
            await base.Where_Join_Not_Exists(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Join_OrderBy_Count(bool async)
        {
            await base.Join_OrderBy_Count(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Multiple_joins_Where_Order_Any(bool async)
        {
            await base.Multiple_joins_Where_Order_Any(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_join_select(bool async)
        {
            await base.Where_join_select(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_orderby_join_select(bool async)
        {
            await base.Where_orderby_join_select(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] != ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_join_orderby_join_select(bool async)
        {
            await base.Where_join_orderby_join_select(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] != ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_select_many(bool async)
        {
            await base.Where_select_many(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_orderby_select_many(bool async)
        {
            await base.Where_orderby_select_many(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task SelectMany_cartesian_product_with_ordering(bool async)
        {
            await base.SelectMany_cartesian_product_with_ordering(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task SelectMany_Joined_DefaultIfEmpty(bool async)
        {
            await AssertQuery(
                async,
                ss =>
                    from c in ss.Set<Customer>().Where(cst => cst.CustomerID == "ALFKI")
                    from o in ss.Set<Order>().Where(o => o.CustomerID == c.CustomerID).DefaultIfEmpty()
                    select new { c.ContactName, o },
                e => (e.ContactName, +e.o?.OrderID),
                entryCount: 6);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task SelectMany_Joined_DefaultIfEmpty2(bool async)
        {
            await AssertQuery(
                async,
                ss =>
                    from c in ss.Set<Customer>().Where(cst => cst.CustomerID == "ALFKI")
                    from o in ss.Set<Order>().Where(o => o.CustomerID == c.CustomerID).DefaultIfEmpty()
                    select o,
                entryCount: 6);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task SelectMany_Joined_DefaultIfEmpty3(bool async)
        {
            return base.SelectMany_Joined_DefaultIfEmpty3(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task SelectMany_Joined(bool async)
        {
            await AssertQuery(
                async,
                ss =>
                    from c in ss.Set<Customer>().Where(cst => cst.CustomerID == "ALFKI")
                    from o in ss.Set<Order>().Where(o => o.CustomerID == c.CustomerID)
                    select new { c.ContactName, o.OrderDate },
                e => (e.ContactName, e.OrderDate));

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task SelectMany_Joined_Take(bool async)
        {
            await AssertQuery(
                async,
                ss =>
                    from c in ss.Set<Customer>().Where(cst => cst.CustomerID == "ALFKI")
                    from o in ss.Set<Order>().Where(o => o.CustomerID == c.CustomerID).Take(1000)
                    select new { c.ContactName, o },
                e => (e.ContactName, e.o.OrderID),
                entryCount: 6);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Take_with_single(bool async)
        {
            await base.Take_with_single(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Take_with_single_select_many(bool async)
        {
            await AssertSingle(
                async,
                ss => (from c in ss.Set<Customer>().Where(cu => cu.CustomerID == "ALFKI")
                       from o in ss.Set<Order>().Where(or => or.OrderID < 10300)
                       orderby c.CustomerID, o.OrderID
                       select new { c, o })
                    .Take(1)
                    .Cast<object>(),
                entryCount: 2);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Distinct_Skip(bool async)
        {
            await base.Distinct_Skip(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Distinct_Skip_Take(bool async)
        {
            await base.Distinct_Skip_Take(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Skip_Distinct(bool async)
        {
            await base.Skip_Distinct(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Skip_Take_Distinct(bool async)
        {
            await base.Skip_Take_Distinct(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Skip_Take_Any(bool async)
        {
            await base.Skip_Take_Any(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Skip_Take_All(bool async)
        {
            await base.Skip_Take_All(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Take_All(bool async)
        {
            await base.Take_All(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Skip_Take_Any_with_predicate(bool async)
        {
            await base.Skip_Take_Any_with_predicate(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Take_Any_with_predicate(bool async)
        {
            await base.Take_Any_with_predicate(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task OrderBy(bool async)
        {
            await base.OrderBy(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")
ORDER BY c[""CustomerID""]");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_true(bool async)
        {
            await base.OrderBy_true(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_integer(bool async)
        {
            await base.OrderBy_integer(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_parameter(bool async)
        {
            await base.OrderBy_parameter(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task OrderBy_anon(bool async)
        {
            await base.OrderBy_anon(async);

            AssertSql(
                @"SELECT c[""CustomerID""]
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")
ORDER BY c[""CustomerID""]");
        }

        public override async Task OrderBy_anon2(bool async)
        {
            await base.OrderBy_anon2(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")
ORDER BY c[""CustomerID""]");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_client_mixed(bool async)
        {
            await base.OrderBy_client_mixed(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_multiple_queries(bool async)
        {
            await base.OrderBy_multiple_queries(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Take_Distinct(bool async)
        {
            await base.Take_Distinct(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Distinct_Take(bool async)
        {
            await base.Distinct_Take(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Distinct_Take_Count(bool async)
        {
            await base.Distinct_Take_Count(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_shadow(bool async)
        {
            await base.OrderBy_shadow(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_multiple(bool async)
        {
            await base.OrderBy_multiple(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_ThenBy_Any(bool async)
        {
            await base.OrderBy_ThenBy_Any(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_correlated_subquery1(bool async)
        {
            await base.OrderBy_correlated_subquery1(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_correlated_subquery2(bool async)
        {
            await base.OrderBy_correlated_subquery2(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_subquery_recursive_trivial(bool async)
        {
            await base.Where_subquery_recursive_trivial(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_query_composition4(bool async)
        {
            await base.Where_query_composition4(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_subquery_expression(bool async)
        {
            await AssertQuery(
                async,
                ss =>
                {
                    var firstOrder = ss.Set<Order>().First();
                    Expression<Func<Order, bool>> expr = z => z.OrderID == firstOrder.OrderID;
                    return ss.Set<Order>().Where(x => x.OrderID < 10300 && ss.Set<Order>().Where(expr).Any());
                },
                entryCount: 52);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_subquery_expression_same_parametername(bool async)
        {
            await AssertQuery(
                async,
                ss =>
                {
                    var firstOrder = ss.Set<Order>().OrderBy(o => o.OrderID).First();
                    Expression<Func<Order, bool>> expr = x => x.OrderID == firstOrder.OrderID;
                    return ss.Set<Order>().Where(o => o.OrderID < 10250)
                        .Where(x => ss.Set<Order>().Where(expr).Where(o => o.CustomerID == x.CustomerID).Any());
                },
                entryCount: 1);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalFact(Skip = "Issue #17246")]
        public override void Select_DTO_distinct_translated_to_server()
        {
            base.Select_DTO_distinct_translated_to_server();

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderID""] < 10300))");
        }

        [ConditionalFact(Skip = "Issue #17246")]
        public override void Select_DTO_constructor_distinct_translated_to_server()
        {
            base.Select_DTO_constructor_distinct_translated_to_server();

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderID""] < 10300))");
        }

        [ConditionalFact(Skip = "Issue #17246")]
        public override void Select_DTO_with_member_init_distinct_translated_to_server()
        {
            base.Select_DTO_with_member_init_distinct_translated_to_server();

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderID""] < 10300))");
        }

        [ConditionalFact(Skip = "Issue #17246")]
        public override void Select_nested_collection_count_using_DTO()
        {
            base.Select_nested_collection_count_using_DTO();

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_DTO_with_member_init_distinct_in_subquery_translated_to_server(bool async)
        {
            await base.Select_DTO_with_member_init_distinct_in_subquery_translated_to_server(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderID""] < 10300))");
        }

        [ConditionalFact(Skip = "Issue #17246")]
        public override void Select_DTO_with_member_init_distinct_in_subquery_used_in_projection_translated_to_server()
        {
            base.Select_DTO_with_member_init_distinct_in_subquery_used_in_projection_translated_to_server();

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Cross collection join Issue#17246")]
        public override async Task Select_correlated_subquery_filtered(bool async)
        {
            await base.Select_correlated_subquery_filtered(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Cross collection join Issue#17246")]
        public override async Task Select_correlated_subquery_ordered(bool async)
        {
            await base.Select_correlated_subquery_ordered(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Cross collection join Issue#17246")]
        public override Task Select_nested_collection_in_anonymous_type(bool async)
        {
            return base.Select_nested_collection_in_anonymous_type(async);
        }

        [ConditionalTheory(Skip = "Cross collection join Issue#17246")]
        public override Task Select_subquery_recursive_trivial(bool async)
        {
            return base.Select_subquery_recursive_trivial(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_subquery_on_bool(bool async)
        {
            await base.Where_subquery_on_bool(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Product"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_subquery_on_collection(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Product>().Where(p => p.ProductID == 72)
                    .Where(
                        p => ss.Set<OrderDetail>()
                            .Where(o => o.ProductID == p.ProductID)
                            .Select(odd => odd.Quantity).Contains<short>(5)),
                entryCount: 1);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Product"") AND (c[""ProductID""] = 72))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_many_cross_join_same_collection(bool async)
        {
            await base.Select_many_cross_join_same_collection(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_null_coalesce_operator(bool async)
        {
            await base.OrderBy_null_coalesce_operator(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_null_coalesce_operator(bool async)
        {
            await base.Select_null_coalesce_operator(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_conditional_operator(bool async)
        {
            await base.OrderBy_conditional_operator(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_conditional_operator_where_condition_false(bool async)
        {
            await base.OrderBy_conditional_operator_where_condition_false(async);

            AssertSql(
                @"@__p_0='false'

SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")
ORDER BY c[""City""]");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_comparison_operator(bool async)
        {
            await base.OrderBy_comparison_operator(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory]
        public override async Task Projection_null_coalesce_operator(bool async)
        {
            await base.Projection_null_coalesce_operator(async);

            AssertSql(
                @"SELECT VALUE {""CustomerID"" : c[""CustomerID""], ""CompanyName"" : c[""CompanyName""], ""Region"" : ((c[""Region""] != null) ? c[""Region""] : ""ZZ"")}
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Filter_coalesce_operator(bool async)
        {
            await base.Filter_coalesce_operator(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (((c[""CompanyName""] != null) ? c[""CompanyName""] : c[""ContactName""]) = ""The Big Cheese""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Take_skip_null_coalesce_operator(bool async)
        {
            await base.Take_skip_null_coalesce_operator(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_take_null_coalesce_operator(bool async)
        {
            await base.Select_take_null_coalesce_operator(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_take_skip_null_coalesce_operator(bool async)
        {
            await base.Select_take_skip_null_coalesce_operator(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_take_skip_null_coalesce_operator2(bool async)
        {
            await base.Select_take_skip_null_coalesce_operator2(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_take_skip_null_coalesce_operator3(bool async)
        {
            await base.Select_take_skip_null_coalesce_operator3(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalFact(Skip = "Issue #17246")]
        public override void Selected_column_can_coalesce()
        {
            base.Selected_column_can_coalesce();

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task DateTime_parse_is_inlined(bool async)
        {
            await base.DateTime_parse_is_inlined(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderDate""] > ""1998-01-01T12:00:00""))");
        }

        public override async Task DateTime_parse_is_parameterized_when_from_closure(bool async)
        {
            await base.DateTime_parse_is_parameterized_when_from_closure(async);

            AssertSql(
                @"@__Parse_0='1998-01-01T12:00:00'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderDate""] > @__Parse_0))");
        }

        public override async Task New_DateTime_is_inlined(bool async)
        {
            await base.New_DateTime_is_inlined(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderDate""] > ""1998-01-01T12:00:00""))");
        }

        public override async Task New_DateTime_is_parameterized_when_from_closure(bool async)
        {
            await base.New_DateTime_is_parameterized_when_from_closure(async);

            AssertSql(
                @"@__p_0='1998-01-01T12:00:00'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderDate""] > @__p_0))",
                //
                @"@__p_0='1998-01-01T11:00:00'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderDate""] > @__p_0))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Random_next_is_not_funcletized_1(bool async)
        {
            return base.Random_next_is_not_funcletized_1(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Random_next_is_not_funcletized_2(bool async)
        {
            return base.Random_next_is_not_funcletized_2(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Random_next_is_not_funcletized_3(bool async)
        {
            return base.Random_next_is_not_funcletized_3(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Random_next_is_not_funcletized_4(bool async)
        {
            return base.Random_next_is_not_funcletized_4(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Random_next_is_not_funcletized_5(bool async)
        {
            return base.Random_next_is_not_funcletized_5(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Random_next_is_not_funcletized_6(bool async)
        {
            return base.Random_next_is_not_funcletized_6(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Environment_newline_is_funcletized(bool async)
        {
            await base.Environment_newline_is_funcletized(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task String_concat_with_navigation1(bool async)
        {
            await base.String_concat_with_navigation1(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task String_concat_with_navigation2(bool async)
        {
            await base.String_concat_with_navigation2(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalFact(Skip = "Issue #17246")]
        public override void Select_bitwise_or()
        {
            base.Select_bitwise_or();

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalFact(Skip = "Issue #17246")]
        public override void Select_bitwise_or_multiple()
        {
            base.Select_bitwise_or_multiple();

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalFact(Skip = "Issue #17246")]
        public override void Select_bitwise_and()
        {
            base.Select_bitwise_and();

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalFact(Skip = "Issue #17246")]
        public override void Select_bitwise_and_or()
        {
            base.Select_bitwise_and_or();

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#13168")]
        public override Task Where_bitwise_or_with_logical_or(bool async)
        {
            return base.Where_bitwise_or_with_logical_or(async);
        }

        public override async Task Where_bitwise_and_with_logical_and(bool async)
        {
            await base.Where_bitwise_and_with_logical_and(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (((c[""CustomerID""] = ""ALFKI"") & (c[""CustomerID""] = ""ANATR"")) AND (c[""CustomerID""] = ""ANTON"")))");
        }

        [ConditionalTheory(Skip = "Issue#13168")]
        public override Task Where_bitwise_or_with_logical_and(bool async)
        {
            return base.Where_bitwise_or_with_logical_and(async);
        }

        public override async Task Where_bitwise_and_with_logical_or(bool async)
        {
            await base.Where_bitwise_and_with_logical_or(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (((c[""CustomerID""] = ""ALFKI"") & (c[""CustomerID""] = ""ANATR"")) OR (c[""CustomerID""] = ""ANTON"")))");
        }

        public override async Task Where_bitwise_binary_not(bool async)
        {
            await base.Where_bitwise_binary_not(async);

            AssertSql(
                @"@__negatedId_0='-10249'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (~(c[""OrderID""]) = @__negatedId_0))");
        }

        public override async Task Where_bitwise_binary_and(bool async)
        {
            await base.Where_bitwise_binary_and(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND ((c[""OrderID""] & 10248) = 10248))");
        }

        public override async Task Where_bitwise_binary_or(bool async)
        {
            await base.Where_bitwise_binary_or(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND ((c[""OrderID""] | 10248) = 10248))");
        }

        [ConditionalFact(Skip = "Issue #17246")]
        public override void Select_bitwise_or_with_logical_or()
        {
            base.Select_bitwise_or_with_logical_or();

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalFact(Skip = "Issue #17246")]
        public override void Select_bitwise_and_with_logical_and()
        {
            base.Select_bitwise_and_with_logical_and();

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Handle_materialization_properly_when_more_than_two_query_sources_are_involved(bool async)
        {
            await base.Handle_materialization_properly_when_more_than_two_query_sources_are_involved(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#13159")]
        public override Task Parameter_extraction_short_circuits_1(bool async)
        {
            return base.Parameter_extraction_short_circuits_1(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Parameter_extraction_short_circuits_2(bool async)
        {
            await base.Parameter_extraction_short_circuits_2(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue#13159")]
        public override Task Parameter_extraction_short_circuits_3(bool async)
        {
            return base.Parameter_extraction_short_circuits_3(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Subquery_member_pushdown_does_not_change_original_subquery_model(bool async)
        {
            await base.Subquery_member_pushdown_does_not_change_original_subquery_model(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Query_expression_with_to_string_and_contains(bool async)
        {
            await base.Query_expression_with_to_string_and_contains(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task Select_expression_long_to_string(bool async)
        {
            await base.Select_expression_long_to_string(async);

            AssertSql(
                @"SELECT c[""OrderID""]
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderDate""] != null))");
        }

        public override async Task Select_expression_int_to_string(bool async)
        {
            await base.Select_expression_int_to_string(async);

            AssertSql(
                @"SELECT c[""OrderID""]
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderDate""] != null))");
        }

        public override async Task ToString_with_formatter_is_evaluated_on_the_client(bool async)
        {
            await base.ToString_with_formatter_is_evaluated_on_the_client(async);

            AssertSql(
                @"SELECT c[""OrderID""]
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderDate""] != null))",
                //
                @"SELECT c[""OrderID""]
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderDate""] != null))");
        }

        public override async Task Select_expression_other_to_string(bool async)
        {
            await base.Select_expression_other_to_string(async);

            AssertSql(
                @"SELECT c[""OrderDate""]
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderDate""] != null))");
        }

        public override async Task Select_expression_date_add_year(bool async)
        {
            await base.Select_expression_date_add_year(async);

            AssertSql(
                @"SELECT c[""OrderDate""]
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderDate""] != null))");
        }

        public override async Task Select_expression_datetime_add_month(bool async)
        {
            await base.Select_expression_datetime_add_month(async);

            AssertSql(
                @"SELECT c[""OrderDate""]
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderDate""] != null))");
        }

        public override async Task Select_expression_datetime_add_hour(bool async)
        {
            await base.Select_expression_datetime_add_hour(async);

            AssertSql(
                @"SELECT c[""OrderDate""]
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderDate""] != null))");
        }

        public override async Task Select_expression_datetime_add_minute(bool async)
        {
            await base.Select_expression_datetime_add_minute(async);

            AssertSql(
                @"SELECT c[""OrderDate""]
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderDate""] != null))");
        }

        public override async Task Select_expression_datetime_add_second(bool async)
        {
            await base.Select_expression_datetime_add_second(async);

            AssertSql(
                @"SELECT c[""OrderDate""]
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderDate""] != null))");
        }

        public override async Task Select_expression_date_add_milliseconds_above_the_range(bool async)
        {
            await base.Select_expression_date_add_milliseconds_above_the_range(async);

            AssertSql(
                @"SELECT c[""OrderDate""]
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderDate""] != null))");
        }

        public override async Task Select_expression_date_add_milliseconds_below_the_range(bool async)
        {
            await base.Select_expression_date_add_milliseconds_below_the_range(async);

            AssertSql(
                @"SELECT c[""OrderDate""]
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderDate""] != null))");
        }

        public override async Task Select_expression_date_add_milliseconds_large_number_divided(bool async)
        {
            await base.Select_expression_date_add_milliseconds_large_number_divided(async);

            AssertSql(
                @"SELECT c[""OrderDate""]
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderDate""] != null))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_expression_references_are_updated_correctly_with_subquery(bool async)
        {
            await base.Select_expression_references_are_updated_correctly_with_subquery(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderDate""] != null))");
        }

        [ConditionalFact(Skip = "Issue#17246")]
        public override void DefaultIfEmpty_without_group_join()
        {
            base.DefaultIfEmpty_without_group_join();

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = ""London""))");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task DefaultIfEmpty_in_subquery(bool async)
        {
            await AssertQuery(
                async,
                ss => from c in ss.Set<Customer>().Where(c => c.City == "London")
                      from o in ss.Set<Order>().Where(o => o.CustomerID == c.CustomerID).DefaultIfEmpty()
                      where o != null
                      select new { c.CustomerID, o.OrderID },
                elementSorter: e => (e.CustomerID, e.OrderID));

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = ""London""))");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override Task DefaultIfEmpty_in_subquery_not_correlated(bool async)
        {
            return base.DefaultIfEmpty_in_subquery_not_correlated(async);
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task DefaultIfEmpty_in_subquery_nested(bool async)
        {
            await AssertQuery(
                async,
                ss => from c in ss.Set<Customer>().Where(c => c.City == "Seattle")
                      from o1 in ss.Set<Order>().Where(o => o.OrderID > 11000).DefaultIfEmpty()
                      from o2 in ss.Set<Order>().Where(o => o.OrderID < 10250).Where(o => o.CustomerID == c.CustomerID).DefaultIfEmpty()
                      where o1 != null && o2 != null
                      orderby o1.OrderID, o2.OrderDate
                      select new
                      {
                          c.CustomerID,
                          o1.OrderID,
                          o2.OrderDate
                      },
                elementSorter: e => (e.CustomerID, e.OrderID));

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = ""Seattle""))");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override Task DefaultIfEmpty_in_subquery_nested_filter_order_comparison(bool async)
        {
            return base.DefaultIfEmpty_in_subquery_nested_filter_order_comparison(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_skip_take(bool async)
        {
            await base.OrderBy_skip_take(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_skip_skip_take(bool async)
        {
            await base.OrderBy_skip_skip_take(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_skip_take_take(bool async)
        {
            await base.OrderBy_skip_take_take(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_skip_take_take_take_take(bool async)
        {
            await base.OrderBy_skip_take_take_take_take(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_skip_take_skip_take_skip(bool async)
        {
            await base.OrderBy_skip_take_skip_take_skip(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_skip_take_distinct(bool async)
        {
            await base.OrderBy_skip_take_distinct(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_coalesce_take_distinct(bool async)
        {
            await base.OrderBy_coalesce_take_distinct(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Product"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_coalesce_skip_take_distinct(bool async)
        {
            await base.OrderBy_coalesce_skip_take_distinct(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Product"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_coalesce_skip_take_distinct_take(bool async)
        {
            await base.OrderBy_coalesce_skip_take_distinct_take(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Product"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_skip_take_distinct_orderby_take(bool async)
        {
            await base.OrderBy_skip_take_distinct_orderby_take(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task No_orderby_added_for_fully_translated_manually_constructed_LOJ(bool async)
        {
            await base.No_orderby_added_for_fully_translated_manually_constructed_LOJ(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        public override async Task No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ(bool async)
        {
            await base.No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition1(
            bool async)
        {
            await base.No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition1(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition2(
            bool async)
        {
            await base.No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition2(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task Orderby_added_for_client_side_GroupJoin_principal_to_dependent_LOJ(bool async)
        {
            await base.Orderby_added_for_client_side_GroupJoin_principal_to_dependent_LOJ(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Contains_with_DateTime_Date(bool async)
        {
            await base.Contains_with_DateTime_Date(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Contains_with_subquery_involving_join_binds_to_correct_table(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(
                    o => o.OrderID > 11002
                        && o.OrderID < 11004
                        && ss.Set<OrderDetail>()
                            .Where(od => od.Product.ProductName == "Chai")
                            .Select(od => od.OrderID)
                            .Contains(o.OrderID)),
                entryCount: 1);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Complex_query_with_repeated_query_model_compiles_correctly(bool async)
        {
            await base.Complex_query_with_repeated_query_model_compiles_correctly(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Complex_query_with_repeated_nested_query_model_compiles_correctly(bool async)
        {
            await base.Complex_query_with_repeated_nested_query_model_compiles_correctly(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue#16144")]
        public override async Task Anonymous_member_distinct_where(bool async)
        {
            await base.Anonymous_member_distinct_where(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Anonymous_member_distinct_orderby(bool async)
        {
            await base.Anonymous_member_distinct_orderby(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Anonymous_member_distinct_result(bool async)
        {
            await base.Anonymous_member_distinct_result(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#16144")]
        public override async Task Anonymous_complex_distinct_where(bool async)
        {
            await base.Anonymous_complex_distinct_where(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Anonymous_complex_distinct_orderby(bool async)
        {
            await base.Anonymous_complex_distinct_orderby(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Anonymous_complex_distinct_result(bool async)
        {
            await base.Anonymous_complex_distinct_result(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Anonymous_complex_orderby(bool async)
        {
            await base.Anonymous_complex_orderby(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Anonymous_subquery_orderby(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.City == "London").Where(c => c.Orders.Count > 1).Select(
                    c => new { A = c.Orders.OrderByDescending(o => o.OrderID).FirstOrDefault().OrderDate }).OrderBy(n => n.A),
                assertOrder: true);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = ""London""))");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task DTO_member_distinct_where(bool async)
        {
            await base.DTO_member_distinct_where(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task DTO_member_distinct_orderby(bool async)
        {
            await base.DTO_member_distinct_orderby(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task DTO_member_distinct_result(bool async)
        {
            await base.DTO_member_distinct_result(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task DTO_complex_distinct_where(bool async)
        {
            await base.DTO_complex_distinct_where(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task DTO_complex_distinct_orderby(bool async)
        {
            await base.DTO_complex_distinct_orderby(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task DTO_complex_distinct_result(bool async)
        {
            await base.DTO_complex_distinct_result(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task DTO_complex_orderby(bool async)
        {
            await base.DTO_complex_orderby(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task DTO_subquery_orderby(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI").Where(c => c.Orders.Count > 1).Select(
                        c => new DTO<DateTime?> { Property = c.Orders.OrderByDescending(o => o.OrderID).FirstOrDefault().OrderDate })
                    .OrderBy(n => n.Property),
                assertOrder: true,
                elementAsserter: (e, a) => Assert.Equal(e.Property, a.Property));

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Include_with_orderby_skip_preserves_ordering(bool async)
        {
            await base.Include_with_orderby_skip_preserves_ordering(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND ((c[""CustomerID""] != ""VAFFE"") AND (c[""CustomerID""] != ""DRACD"")))");
        }

        public override async Task Int16_parameter_can_be_used_for_int_column(bool async)
        {
            await base.Int16_parameter_can_be_used_for_int_column(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderID""] = 10300))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Subquery_is_null_translated_correctly(bool async)
        {
            await AssertQuery(
                async,
                ss => from c in ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI")
                      let lastOrder = c.Orders.OrderByDescending(o => o.OrderID)
                          .Select(o => o.CustomerID)
                          .FirstOrDefault()
                      where lastOrder == null
                      select c);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Subquery_is_not_null_translated_correctly(bool async)
        {
            await AssertQuery(
                async,
                ss => from c in ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI")
                      let lastOrder = c.Orders.OrderByDescending(o => o.OrderID)
                          .Select(o => o.CustomerID)
                          .FirstOrDefault()
                      where lastOrder != null
                      select c,
                entryCount: 1);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_take_average(bool async)
        {
            await base.Select_take_average(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_take_count(bool async)
        {
            await base.Select_take_count(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_orderBy_take_count(bool async)
        {
            await base.Select_orderBy_take_count(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_take_long_count(bool async)
        {
            await base.Select_take_long_count(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_orderBy_take_long_count(bool async)
        {
            await base.Select_orderBy_take_long_count(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_take_max(bool async)
        {
            await base.Select_take_max(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_take_min(bool async)
        {
            await base.Select_take_min(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_take_sum(bool async)
        {
            await base.Select_take_sum(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_skip_average(bool async)
        {
            await base.Select_skip_average(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_skip_count(bool async)
        {
            await base.Select_skip_count(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_orderBy_skip_count(bool async)
        {
            await base.Select_orderBy_skip_count(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_skip_long_count(bool async)
        {
            await base.Select_skip_long_count(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_orderBy_skip_long_count(bool async)
        {
            await base.Select_orderBy_skip_long_count(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_skip_max(bool async)
        {
            await base.Select_skip_max(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_skip_min(bool async)
        {
            await base.Select_skip_min(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_skip_sum(bool async)
        {
            await base.Select_skip_sum(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_distinct_average(bool async)
        {
            await base.Select_distinct_average(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_distinct_count(bool async)
        {
            await base.Select_distinct_count(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_distinct_long_count(bool async)
        {
            await base.Select_distinct_long_count(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_distinct_max(bool async)
        {
            await base.Select_distinct_max(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_distinct_min(bool async)
        {
            await base.Select_distinct_min(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_distinct_sum(bool async)
        {
            await base.Select_distinct_sum(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Comparing_to_fixed_string_parameter(bool async)
        {
            await base.Comparing_to_fixed_string_parameter(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Comparing_entities_using_Equals(bool async)
        {
            await base.Comparing_entities_using_Equals(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Comparing_different_entity_types_using_Equals(bool async)
        {
            await AssertQuery(
                async,
                ss => from c in ss.Set<Customer>()
                      where c.CustomerID == "ALFKI"
                      from o in ss.Set<Order>()
                      where o.CustomerID == "ALFKI"
                      where c.Equals(o)
                      select c.CustomerID);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Comparing_entity_to_null_using_Equals(bool async)
        {
            await base.Comparing_entity_to_null_using_Equals(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Comparing_navigations_using_Equals(bool async)
        {
            await AssertQuery(
                async,
                ss =>
                    from o1 in ss.Set<Order>()
                    where o1.CustomerID.StartsWith("A")
                    from o2 in ss.Set<Order>()
                    where o1.Customer.Equals(o2.Customer)
                    orderby o1.OrderID, o2.OrderID
                    select new { Id1 = o1.OrderID, Id2 = o2.OrderID },
                elementSorter: e => (e.Id1, e.Id2));

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Comparing_navigations_using_static_Equals(bool async)
        {
            await AssertQuery(
                async,
                ss =>
                    from o1 in ss.Set<Order>()
                    where o1.CustomerID.StartsWith("A")
                    from o2 in ss.Set<Order>()
                    where Equals(o1.Customer, o2.Customer)
                    orderby o1.OrderID, o2.OrderID
                    select new { Id1 = o1.OrderID, Id2 = o2.OrderID },
                elementSorter: e => (e.Id1, e.Id2));

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Comparing_non_matching_entities_using_Equals(bool async)
        {
            await AssertQuery(
                async,
                ss =>
                    from c in ss.Set<Customer>()
                    where c.CustomerID == "ALFKI"
                    from o in ss.Set<Order>()
                    where Equals(c, o)
                    select new { Id1 = c.CustomerID, Id2 = o.OrderID },
                elementSorter: e => (e.Id1, e.Id2));

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Comparing_non_matching_collection_navigations_using_Equals(bool async)
        {
            await AssertQuery(
                async,
                ss =>
                    from c in ss.Set<Customer>()
                    where c.CustomerID == "ALFKI"
                    from o in ss.Set<Order>()
                    where c.Orders.Equals(o.OrderDetails)
                    select new { Id1 = c.CustomerID, Id2 = o.OrderID },
                elementSorter: e => (e.Id1, e.Id2));

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Comparing_collection_navigation_to_null(bool async)
        {
            await base.Comparing_collection_navigation_to_null(async);

            AssertSql(
                @"SELECT c[""CustomerID""]
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = null))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Comparing_collection_navigation_to_null_complex(bool async)
        {
            await base.Comparing_collection_navigation_to_null_complex(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""OrderDetail"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Compare_collection_navigation_with_itself(bool async)
        {
            await base.Compare_collection_navigation_with_itself(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Compare_two_collection_navigations_with_different_query_sources(bool async)
        {
            await base.Compare_two_collection_navigations_with_different_query_sources(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Compare_two_collection_navigations_using_equals(bool async)
        {
            await base.Compare_two_collection_navigations_using_equals(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Compare_two_collection_navigations_with_different_property_chains(bool async)
        {
            await base.Compare_two_collection_navigations_with_different_property_chains(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_ThenBy_same_column_different_direction(bool async)
        {
            await base.OrderBy_ThenBy_same_column_different_direction(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_OrderBy_same_column_different_direction(bool async)
        {
            await base.OrderBy_OrderBy_same_column_different_direction(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Complex_nested_query_doesnt_try_binding_to_grandparent_when_parent_returns_complex_result(bool async)
        {
            await base.Complex_nested_query_doesnt_try_binding_to_grandparent_when_parent_returns_complex_result(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Complex_nested_query_properly_binds_to_grandparent_when_parent_returns_scalar_result(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI")
                    .Select(
                        c => new { c.CustomerID, OuterOrders = c.Orders.Where(o => o.OrderID < 10250).Count(o => c.Orders.Count() > 0) }));

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))",
                //
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderID""] < 10250))",
                //
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")",
                //
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task OrderBy_Dto_projection_skip_take(bool async)
        {
            await base.OrderBy_Dto_projection_skip_take(async);

            AssertSql(
                @"@__p_0='5'
@__p_1='10'

SELECT VALUE {""Id"" : c[""CustomerID""]}
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")
ORDER BY c[""CustomerID""]
OFFSET @__p_0 LIMIT @__p_1");
        }

        [ConditionalFact(Skip = "Cross collection join Issue#17246")]
        public override void Streaming_chained_sync_query()
        {
            using (var context = CreateContext())
            {
                var results
                    = (context.Customers.Where(c => c.CustomerID == "ALFKI")
                        .Select(
                            c => new { c.CustomerID, Orders = context.Orders.Where(o => o.Customer.CustomerID == c.CustomerID) }).ToList())
                    .Select(
                        x => new
                        {
                            Orders = x.Orders
                                .GroupJoin(
                                    new[] { "ALFKI" }, y => x.CustomerID, y => y, (h, id) => new { h.Customer })
                        })
                    .ToList();

                Assert.Equal(6, results.SelectMany(r => r.Orders).ToList().Count);
            }

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Join_take_count_works(bool async)
        {
            await base.Join_take_count_works(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND ((c[""OrderID""] > 690) AND (c[""OrderID""] < 710)))");
        }

        public override async Task OrderBy_empty_list_contains(bool async)
        {
            await base.OrderBy_empty_list_contains(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task OrderBy_empty_list_does_not_contains(bool async)
        {
            await base.OrderBy_empty_list_does_not_contains(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalFact(Skip = "Issue #17246")]
        public override void Manual_expression_tree_typed_null_equality()
        {
            base.Manual_expression_tree_typed_null_equality();

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Let_subquery_with_multiple_occurrences(bool async)
        {
            await AssertQuery(
                async,
                ss => from o in ss.Set<Order>().Where(or => or.OrderID < 10250)
                      let details =
                          from od in o.OrderDetails
                          where od.Quantity < 10
                          select od.Quantity
                      where details.Any()
                      select new { Count = details.Count() });

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderID""] < 10250))",
                //
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND (c[""Quantity""] < 10))",
                //
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND (c[""Quantity""] < 10))",
                //
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND (c[""Quantity""] < 10))",
                //
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND (c[""Quantity""] < 10))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Let_entity_equality_to_null(bool async)
        {
            await base.Let_entity_equality_to_null(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Let_entity_equality_to_other_entity(bool async)
        {
            await base.Let_entity_equality_to_other_entity(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task SelectMany_after_client_method(bool async)
        {
            await base.SelectMany_after_client_method(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Collection_navigation_equal_to_null_for_subquery(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(
                    c => c.Orders.Where(o => o.OrderID < 10250).OrderBy(o => o.OrderID).FirstOrDefault().OrderDetails == null),
                ss => ss.Set<Customer>().Where(
                    c => c.Orders.Where(o => o.OrderID < 10250).OrderBy(o => o.OrderID).FirstOrDefault() == null),
                entryCount: 89);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17314")]
        public override async Task Dependent_to_principal_navigation_equal_to_null_for_subquery(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(
                    c => c.Orders.Where(o => o.OrderID < 10250).OrderBy(o => o.OrderID).FirstOrDefault().Customer == null),
                ss => ss.Set<Customer>().Where(
                    c => c.Orders.Where(o => o.OrderID < 10250).OrderBy(o => o.OrderID).Select(o => o.CustomerID).FirstOrDefault() == null),
                entryCount: 89);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Collection_navigation_equality_rewrite_for_subquery(bool async)
        {
            await base.Collection_navigation_equality_rewrite_for_subquery(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override void Throws_on_concurrent_query_first()
        {
            // #13160
        }

        public override void Throws_on_concurrent_query_list()
        {
            // #13160
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override Task Entity_equality_through_nested_anonymous_type_projection(bool async)
        {
            return base.Entity_equality_through_nested_anonymous_type_projection(async);
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Entity_equality_through_DTO_projection(bool async)
        {
            await base.Entity_equality_through_DTO_projection(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Entity_equality_through_subquery(bool async)
        {
            return base.Entity_equality_through_subquery(async);
        }

        [ConditionalFact(Skip = "Issue #17246")]
        public override void Can_convert_manually_build_expression_with_default()
        {
            base.Can_convert_manually_build_expression_with_default();
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Entity_equality_orderby_descending_composite_key(bool async)
        {
            return base.Entity_equality_orderby_descending_composite_key(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Entity_equality_orderby_subquery(bool async)
        {
            return base.Entity_equality_orderby_subquery(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Entity_equality_orderby_descending_subquery_composite_key(bool async)
        {
            return base.Entity_equality_orderby_descending_subquery_composite_key(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Null_Coalesce_Short_Circuit(bool async)
        {
            return base.Null_Coalesce_Short_Circuit(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task OrderByDescending_ThenBy(bool async)
        {
            return base.OrderByDescending_ThenBy(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task OrderByDescending_ThenByDescending(bool async)
        {
            return base.OrderByDescending_ThenByDescending(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task OrderBy_Join(bool async)
        {
            return base.OrderBy_Join(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task OrderBy_ThenBy(bool async)
        {
            return base.OrderBy_ThenBy(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task OrderBy_ThenBy_predicate(bool async)
        {
            return base.OrderBy_ThenBy_predicate(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task SelectMany_correlated_simple(bool async)
        {
            return base.SelectMany_correlated_simple(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task SelectMany_nested_simple(bool async)
        {
            return base.SelectMany_nested_simple(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task SelectMany_primitive(bool async)
        {
            return base.SelectMany_primitive(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task SelectMany_primitive_select_subquery(bool async)
        {
            return base.SelectMany_primitive_select_subquery(async);
        }

        [ConditionalFact(Skip = "Issue #17246")]
        public override void Select_DTO_constructor_distinct_with_navigation_translated_to_server()
        {
            base.Select_DTO_constructor_distinct_with_navigation_translated_to_server();
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Select_Property_when_shadow_unconstrained_generic_method(bool async)
        {
            return base.Select_Property_when_shadow_unconstrained_generic_method(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Skip_orderby_const(bool async)
        {
            return base.Skip_orderby_const(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Where_Property_when_shadow_unconstrained_generic_method(bool async)
        {
            return base.Where_Property_when_shadow_unconstrained_generic_method(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Inner_parameter_in_nested_lambdas_gets_preserved(bool async)
        {
            return base.Inner_parameter_in_nested_lambdas_gets_preserved(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Navigation_inside_interpolated_string_is_expanded(bool async)
        {
            return base.Navigation_inside_interpolated_string_is_expanded(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Select_DTO_with_member_init_distinct_in_subquery_translated_to_server_2(bool async)
        {
            return base.Select_DTO_with_member_init_distinct_in_subquery_translated_to_server_2(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task All_client(bool async)
            => base.All_client(async);

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Client_OrderBy_GroupBy_Group_ordering_works(bool async)
            => base.Client_OrderBy_GroupBy_Group_ordering_works(async);

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Subquery_member_pushdown_does_not_change_original_subquery_model2(bool async)
            => base.Subquery_member_pushdown_does_not_change_original_subquery_model2(async);

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Where_query_composition3(bool async)
            => base.Where_query_composition3(async);

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task OrderBy_object_type_server_evals(bool async)
        {
            return base.OrderBy_object_type_server_evals(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task AsQueryable_in_query_server_evals(bool async)
        {
            return base.AsQueryable_in_query_server_evals(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task SelectMany_correlated_subquery_simple(bool async)
        {
            return base.SelectMany_correlated_subquery_simple(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task SelectMany_correlated_subquery_hard(bool async)
        {
            return base.SelectMany_correlated_subquery_hard(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Subquery_DefaultIfEmpty_Any(bool async)
        {
            return base.Subquery_DefaultIfEmpty_Any(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Projection_skip_collection_projection(bool async)
        {
            return base.Projection_skip_collection_projection(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Projection_take_collection_projection(bool async)
        {
            return base.Projection_take_collection_projection(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Projection_skip_take_collection_projection(bool async)
        {
            return base.Projection_skip_take_collection_projection(async);
        }

        public override Task Projection_skip_projection(bool async)
        {
            return AssertTranslationFailed(() => base.Projection_skip_projection(async));
        }

        public override Task Projection_take_projection(bool async)
        {
            return AssertTranslationFailed(() => base.Projection_take_projection(async));
        }

        public override Task Projection_skip_take_projection(bool async)
        {
            return AssertTranslationFailed(() => base.Projection_skip_take_projection(async));
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Collection_projection_skip(bool async)
        {
            return base.Collection_projection_skip(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Collection_projection_take(bool async)
        {
            return base.Collection_projection_take(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Collection_projection_skip_take(bool async)
        {
            return base.Collection_projection_skip_take(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Anonymous_projection_skip_empty_collection_FirstOrDefault(bool async)
        {
            return base.Anonymous_projection_skip_empty_collection_FirstOrDefault(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Anonymous_projection_take_empty_collection_FirstOrDefault(bool async)
        {
            return base.Anonymous_projection_take_empty_collection_FirstOrDefault(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Anonymous_projection_skip_take_empty_collection_FirstOrDefault(bool async)
        {
            return base.Anonymous_projection_skip_take_empty_collection_FirstOrDefault(async);
        }

        public override async Task Checked_context_with_arithmetic_does_not_fail(bool isAsync)
        {
            await base.Checked_context_with_arithmetic_does_not_fail(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND ((((c[""Quantity""] + 1) = 5) AND ((c[""Quantity""] - 1) = 3)) AND ((c[""Quantity""] * 1) = c[""Quantity""])))
ORDER BY c[""OrderID""]");
        }

        public override async Task Checked_context_with_case_to_same_nullable_type_does_not_fail(bool isAsync)
        {
            await base.Checked_context_with_case_to_same_nullable_type_does_not_fail(isAsync);

            AssertSql(
                @"SELECT MAX(c[""Quantity""]) AS c
FROM root c
WHERE (c[""Discriminator""] = ""OrderDetail"")");
        }

        public override async Task Entity_equality_with_null_coalesce_client_side(bool async)
        {
            await base.Entity_equality_with_null_coalesce_client_side(async);

            AssertSql(
                @"@__entity_equality_p_0_CustomerID='ALFKI'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = @__entity_equality_p_0_CustomerID))");
        }

        public override async Task Entity_equality_contains_with_list_of_null(bool async)
        {
            await base.Entity_equality_contains_with_list_of_null(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] IN (""ALFKI"") OR (c[""CustomerID""] = null)))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Perform_identity_resolution_reuses_same_instances(bool async)
        {
            return base.Perform_identity_resolution_reuses_same_instances(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Perform_identity_resolution_reuses_same_instances_across_joins(bool async)
        {
            return base.Perform_identity_resolution_reuses_same_instances_across_joins(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task All_client_and_server_top_level(bool async)
            => base.All_client_and_server_top_level(async);

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task All_client_or_server_top_level(bool async)
            => base.All_client_or_server_top_level(async);

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Single_non_scalar_projection_after_skip_uses_join(bool async)
        {
            return base.Single_non_scalar_projection_after_skip_uses_join(async);
        }

        [ConditionalTheory(Skip = "No Select after Distinct issue#17246")]
        public override Task Select_distinct_Select_with_client_bindings(bool async)
        {
            return base.Select_distinct_Select_with_client_bindings(async);
        }

        [ConditionalTheory(Skip = "Non embedded collection subquery Issue#17246")]
        public override Task Pending_selector_in_cardinality_reducing_method_is_applied_before_expanding_collection_navigation_member(
            bool async)
        {
            return base.Pending_selector_in_cardinality_reducing_method_is_applied_before_expanding_collection_navigation_member(async);
        }

        [ConditionalTheory(Skip = "Non embedded collection subquery Issue#17246")]
        public override Task Max_on_empty_sequence_throws(bool async)
        {
            return base.Max_on_empty_sequence_throws(async);
        }

        [ConditionalTheory(Skip = "string.IndexOf Issue#17246")]
        public override Task Distinct_followed_by_ordering_on_condition(bool async)
        {
            return base.Distinct_followed_by_ordering_on_condition(async);
        }

        [ConditionalTheory(Skip = "Non embedded collection subquery Issue#17246")]
        public override Task DefaultIfEmpty_Sum_over_collection_navigation(bool async)
        {
            return base.DefaultIfEmpty_Sum_over_collection_navigation(async);
        }

        [ConditionalTheory(Skip = "Non embedded collection subquery Issue#17246")]
        public override Task Entity_equality_on_subquery_with_null_check(bool async)
        {
            return base.Entity_equality_on_subquery_with_null_check(async);
        }

        [ConditionalTheory(Skip = "DefaultIfEmpty Issue#17246")]
        public override Task DefaultIfEmpty_over_empty_collection_followed_by_projecting_constant(bool async)
        {
            return base.DefaultIfEmpty_over_empty_collection_followed_by_projecting_constant(async);
        }

        [ConditionalTheory(Skip = "Non embedded collection subquery Issue#17246")]
        public override Task FirstOrDefault_with_predicate_nested(bool async)
        {
            return base.FirstOrDefault_with_predicate_nested(async);
        }

        [ConditionalTheory(Skip = "Non embedded collection subquery Issue#17246")]
        public override Task First_on_collection_in_projection(bool async)
        {
            return base.First_on_collection_in_projection(async);
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
