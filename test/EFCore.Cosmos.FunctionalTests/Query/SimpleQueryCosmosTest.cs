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
    public partial class SimpleQueryCosmosTest : SimpleQueryTestBase<NorthwindQueryCosmosFixture<NoopModelCustomizer>>
    {
        public SimpleQueryCosmosTest(
            NorthwindQueryCosmosFixture<NoopModelCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            ClearLog();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [ConditionalFact(Skip = "See issue#13857")]
        public override void Auto_initialized_view_set()
        {
            base.Auto_initialized_view_set();
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Simple_IQueryable(bool isAsync)
        {
            await AssertQuery(isAsync, ss => ss.Set<Customer>(), entryCount: 91);

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

        public override async Task Local_dictionary(bool isAsync)
        {
            await base.Local_dictionary(isAsync);

            AssertSql(
                @"@__p_0='ALFKI'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = @__p_0))
OFFSET 0 LIMIT 2");
        }

        public override async Task Entity_equality_self(bool isAsync)
        {
            await base.Entity_equality_self(isAsync);

            AssertSql(
                @"SELECT c[""CustomerID""]
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = c[""CustomerID""]))");
        }

        public override async Task Entity_equality_local(bool isAsync)
        {
            await base.Entity_equality_local(isAsync);

            AssertSql(
                @"@__entity_equality_local_0_CustomerID='ANATR'

SELECT c[""CustomerID""]
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = @__entity_equality_local_0_CustomerID))");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Entity_equality_local_composite_key(bool isAsync)
        {
            await base.Entity_equality_local_composite_key(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Join_with_entity_equality_local_on_both_sources(bool isAsync)
        {
            await base.Join_with_entity_equality_local_on_both_sources(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Entity_equality_local_inline(bool isAsync)
        {
            await base.Entity_equality_local_inline(isAsync);

            AssertSql(
                @"SELECT c[""CustomerID""]
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ANATR""))");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Entity_equality_local_inline_composite_key(bool isAsync)
        {
            await base.Entity_equality_local_inline_composite_key(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Entity_equality_null(bool isAsync)
        {
            await base.Entity_equality_null(isAsync);

            AssertSql(
                @"SELECT c[""CustomerID""]
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = null))");
        }

        public override async Task Entity_equality_not_null(bool isAsync)
        {
            await base.Entity_equality_not_null(isAsync);

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
        public override async Task Queryable_reprojection(bool isAsync)
        {
            await base.Queryable_reprojection(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Default_if_empty_top_level(bool isAsync)
        {
            await base.Default_if_empty_top_level(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND (c[""EmployeeID""] = 4294967295))");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Join_with_default_if_empty_on_both_sources(bool isAsync)
        {
            await base.Join_with_default_if_empty_on_both_sources(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Default_if_empty_top_level_followed_by_projecting_constant(bool isAsync)
        {
            await base.Default_if_empty_top_level_followed_by_projecting_constant(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Default_if_empty_top_level_positive(bool isAsync)
        {
            await base.Default_if_empty_top_level_positive(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND (c[""EmployeeID""] > 0))");
        }

        public override Task Default_if_empty_top_level_arg(bool isAsync)
        {
            // Issue #13409
            return Task.CompletedTask;
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Default_if_empty_top_level_arg_followed_by_projecting_constant(bool isAsync)
        {
            return base.Default_if_empty_top_level_arg_followed_by_projecting_constant(isAsync);
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Default_if_empty_top_level_projection(bool isAsync)
        {
            await base.Default_if_empty_top_level_projection(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND (c[""EmployeeID""] = 4294967295))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_query_composition(bool isAsync)
        {
            await base.Where_query_composition(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_query_composition_is_null(bool isAsync)
        {
            await base.Where_query_composition_is_null(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_query_composition_is_not_null(bool isAsync)
        {
            await base.Where_query_composition_is_null(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_query_composition_entity_equality_one_element_SingleOrDefault(bool isAsync)
        {
            await base.Where_query_composition_entity_equality_one_element_SingleOrDefault(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Where_query_composition_entity_equality_one_element_Single(bool isAsync)
        {
            return base.Where_query_composition_entity_equality_one_element_Single(isAsync);
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Where_query_composition_entity_equality_one_element_FirstOrDefault(bool isAsync)
        {
            await base.Where_query_composition_entity_equality_one_element_FirstOrDefault(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Where_query_composition_entity_equality_one_element_First(bool isAsync)
        {
            return base.Where_query_composition_entity_equality_one_element_First(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_query_composition_entity_equality_no_elements_SingleOrDefault(bool isAsync)
        {
            await base.Where_query_composition_entity_equality_no_elements_SingleOrDefault(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Where_query_composition_entity_equality_no_elements_Single(bool isAsync)
        {
            return base.Where_query_composition_entity_equality_no_elements_Single(isAsync);
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Where_query_composition_entity_equality_no_elements_FirstOrDefault(bool isAsync)
        {
            await base.Where_query_composition_entity_equality_no_elements_FirstOrDefault(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Where_query_composition_entity_equality_no_elements_First(bool isAsync)
        {
            return base.Where_query_composition_entity_equality_no_elements_First(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Where_query_composition_entity_equality_multiple_elements_SingleOrDefault(bool isAsync)
        {
            return base.Where_query_composition_entity_equality_multiple_elements_SingleOrDefault(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Where_query_composition_entity_equality_multiple_elements_Single(bool isAsync)
        {
            return base.Where_query_composition_entity_equality_multiple_elements_Single(isAsync);
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Where_query_composition_entity_equality_multiple_elements_FirstOrDefault(bool isAsync)
        {
            await base.Where_query_composition_entity_equality_multiple_elements_FirstOrDefault(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Where_query_composition_entity_equality_multiple_elements_First(bool isAsync)
        {
            return base.Where_query_composition_entity_equality_multiple_elements_First(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_query_composition2(bool isAsync)
        {
            await base.Where_query_composition2(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_query_composition2_FirstOrDefault(bool isAsync)
        {
            await base.Where_query_composition2_FirstOrDefault(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_query_composition2_FirstOrDefault_with_anonymous(bool isAsync)
        {
            await base.Where_query_composition2_FirstOrDefault_with_anonymous(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

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

        [ConditionalTheory(Skip = "Issue #12086")]
        public override async Task Where_subquery_anon(bool isAsync)
        {
            await base.Where_subquery_anon(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_subquery_anon_nested(bool isAsync)
        {
            await base.Where_subquery_anon_nested(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_SelectMany(bool isAsync)
        {
            await AssertQuery(
                isAsync,
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
        public override async Task Let_any_subquery_anonymous(bool isAsync)
        {
            await base.Let_any_subquery_anonymous(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_arithmetic(bool isAsync)
        {
            await base.OrderBy_arithmetic(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_condition_comparison(bool isAsync)
        {
            await base.OrderBy_condition_comparison(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Product"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_ternary_conditions(bool isAsync)
        {
            await base.OrderBy_ternary_conditions(isAsync);

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
        public override async Task Skip(bool isAsync)
        {
            await base.Skip(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Skip_no_orderby(bool isAsync)
        {
            await base.Skip_no_orderby(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Skip_Take(bool isAsync)
        {
            await base.Skip_Take(isAsync);

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
        public override async Task Join_Customers_Orders_Skip_Take(bool isAsync)
        {
            await base.Join_Customers_Orders_Skip_Take(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Join_Customers_Orders_Skip_Take_followed_by_constant_projection(bool isAsync)
        {
            await base.Join_Customers_Orders_Skip_Take_followed_by_constant_projection(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Join_Customers_Orders_Projection_With_String_Concat_Skip_Take(bool isAsync)
        {
            await base.Join_Customers_Orders_Projection_With_String_Concat_Skip_Take(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Join_Customers_Orders_Orders_Skip_Take_Same_Properties(bool isAsync)
        {
            await base.Join_Customers_Orders_Orders_Skip_Take_Same_Properties(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Take_Skip(bool isAsync)
        {
            await base.Take_Skip(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Take_Skip_Distinct(bool isAsync)
        {
            await base.Take_Skip_Distinct(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Take_Skip_Distinct_Caching(bool isAsync)
        {
            await base.Take_Skip_Distinct_Caching(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Take_Distinct_Count(bool isAsync)
        {
            await base.Take_Distinct_Count(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Take_Where_Distinct_Count(bool isAsync)
        {
            await base.Take_Where_Distinct_Count(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""CustomerID""] = ""FRANK""))");
        }

        public override async Task Queryable_simple(bool isAsync)
        {
            await base.Queryable_simple(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Queryable_simple_anonymous(bool isAsync)
        {
            await base.Queryable_simple_anonymous(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Queryable_nested_simple(bool isAsync)
        {
            await base.Queryable_nested_simple(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Queryable_simple_anonymous_projection_subquery(bool isAsync)
        {
            await base.Queryable_simple_anonymous_projection_subquery(isAsync);

            AssertSql(
                @"@__p_0='91'

SELECT c[""City""]
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")
OFFSET 0 LIMIT @__p_0");
        }

        public override async Task Queryable_simple_anonymous_subquery(bool isAsync)
        {
            await base.Queryable_simple_anonymous_subquery(isAsync);

            AssertSql(
                @"@__p_0='91'

SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")
OFFSET 0 LIMIT @__p_0");
        }

        public override async Task Take_simple(bool isAsync)
        {
            await base.Take_simple(isAsync);

            AssertSql(
                @"@__p_0='10'

SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")
ORDER BY c[""CustomerID""]
OFFSET 0 LIMIT @__p_0");
        }

        public override async Task Take_simple_parameterized(bool isAsync)
        {
            await base.Take_simple_parameterized(isAsync);

            AssertSql(
                @"@__p_0='10'

SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")
ORDER BY c[""CustomerID""]
OFFSET 0 LIMIT @__p_0");
        }

        public override async Task Take_simple_projection(bool isAsync)
        {
            await base.Take_simple_projection(isAsync);

            AssertSql(
                @"@__p_0='10'

SELECT c[""City""]
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")
ORDER BY c[""CustomerID""]
OFFSET 0 LIMIT @__p_0");
        }

        public override async Task Take_subquery_projection(bool isAsync)
        {
            await base.Take_subquery_projection(isAsync);

            AssertSql(
                @"@__p_0='2'

SELECT c[""City""]
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")
ORDER BY c[""CustomerID""]
OFFSET 0 LIMIT @__p_0");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_Take_Count(bool isAsync)
        {
            await base.OrderBy_Take_Count(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Take_OrderBy_Count(bool isAsync)
        {
            await base.Take_OrderBy_Count(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Any_simple(bool isAsync)
        {
            await base.Any_simple(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Any_predicate(bool isAsync)
        {
            await base.Any_predicate(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Any_nested_negated(bool isAsync)
        {
            await base.Any_nested_negated(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Any_nested_negated2(bool isAsync)
        {
            await base.Any_nested_negated2(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Any_nested_negated3(bool isAsync)
        {
            await base.Any_nested_negated3(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Any_nested(bool isAsync)
        {
            await base.Any_nested(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Any_nested2(bool isAsync)
        {
            await base.Any_nested2(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Any_nested3(bool isAsync)
        {
            await base.Any_nested3(isAsync);

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
        public override async Task All_top_level(bool isAsync)
        {
            await base.All_top_level(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task All_top_level_column(bool isAsync)
        {
            await base.All_top_level_column(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task All_top_level_subquery(bool isAsync)
        {
            await AssertSingleResult(
                isAsync,
                syncQuery: ss => ss.Set<Customer>()
                    .All(
                        c1 => c1.CustomerID == "ALFKI"
                            && ss.Set<Customer>().Any(c2 => ss.Set<Customer>().Any(c3 => c1.CustomerID == c3.CustomerID))),
                asyncQuery: ss => ss.Set<Customer>()
                    .AllAsync(
                        c1 => c1.CustomerID == "ALFKI"
                            && ss.Set<Customer>().Any(c2 => ss.Set<Customer>().Any(c3 => c1.CustomerID == c3.CustomerID))));

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task All_top_level_subquery_ef_property(bool isAsync)
        {
            await AssertSingleResult(
                isAsync,
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
                                        .Any(c3 => EF.Property<string>(c1, "CustomerID") == c3.CustomerID))));

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task First_client_predicate(bool isAsync)
        {
            await base.First_client_predicate(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_select_many_or(bool isAsync)
        {
            await base.Where_select_many_or(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_select_many_or2(bool isAsync)
        {
            await base.Where_select_many_or2(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_select_many_or3(bool isAsync)
        {
            await base.Where_select_many_or3(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_select_many_or4(bool isAsync)
        {
            await base.Where_select_many_or4(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_select_many_or_with_parameter(bool isAsync)
        {
            await base.Where_select_many_or_with_parameter(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task SelectMany_mixed(bool isAsync)
        {
            await base.SelectMany_mixed(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task SelectMany_simple_subquery(bool isAsync)
        {
            await base.SelectMany_simple_subquery(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task SelectMany_simple1(bool isAsync)
        {
            await base.SelectMany_simple1(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task SelectMany_simple2(bool isAsync)
        {
            await AssertQuery(
                isAsync,
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
        public override async Task SelectMany_entity_deep(bool isAsync)
        {
            await AssertQuery(
                isAsync,
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
        public override async Task SelectMany_projection1(bool isAsync)
        {
            await base.SelectMany_projection1(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task SelectMany_projection2(bool isAsync)
        {
            await base.SelectMany_projection2(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task SelectMany_customer_orders(bool isAsync)
        {
            await AssertQuery(
                isAsync,
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
        public override async Task SelectMany_Count(bool isAsync)
        {
            await AssertCount(
                isAsync,
                ss => from c in ss.Set<Customer>().Where(ct => ct.City == "London")
                      from o in ss.Set<Order>()
                      select c.CustomerID);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = ""London""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task SelectMany_LongCount(bool isAsync)
        {
            await AssertLongCount(
                isAsync,
                ss => from c in ss.Set<Customer>().Where(ct => ct.City == "London")
                      from o in ss.Set<Order>()
                      select c.CustomerID);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = ""London""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task SelectMany_OrderBy_ThenBy_Any(bool isAsync)
        {
            await base.SelectMany_OrderBy_ThenBy_Any(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Join_Where_Count(bool isAsync)
        {
            await base.Join_Where_Count(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_Join_Any(bool isAsync)
        {
            await base.Where_Join_Any(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_Join_Exists(bool isAsync)
        {
            await base.Where_Join_Exists(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_Join_Exists_Inequality(bool isAsync)
        {
            await base.Where_Join_Exists_Inequality(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_Join_Exists_Constant(bool isAsync)
        {
            await base.Where_Join_Exists_Constant(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_Join_Not_Exists(bool isAsync)
        {
            await base.Where_Join_Not_Exists(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Join_OrderBy_Count(bool isAsync)
        {
            await base.Join_OrderBy_Count(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Multiple_joins_Where_Order_Any(bool isAsync)
        {
            await base.Multiple_joins_Where_Order_Any(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_join_select(bool isAsync)
        {
            await base.Where_join_select(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_orderby_join_select(bool isAsync)
        {
            await base.Where_orderby_join_select(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] != ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_join_orderby_join_select(bool isAsync)
        {
            await base.Where_join_orderby_join_select(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] != ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_select_many(bool isAsync)
        {
            await base.Where_select_many(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_orderby_select_many(bool isAsync)
        {
            await base.Where_orderby_select_many(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task SelectMany_cartesian_product_with_ordering(bool isAsync)
        {
            await base.SelectMany_cartesian_product_with_ordering(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task SelectMany_Joined_DefaultIfEmpty(bool isAsync)
        {
            await AssertQuery(
                isAsync,
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
        public override async Task SelectMany_Joined_DefaultIfEmpty2(bool isAsync)
        {
            await AssertQuery(
                isAsync,
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
        public override async Task SelectMany_Joined(bool isAsync)
        {
            await AssertQuery(
                isAsync,
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
        public override async Task SelectMany_Joined_Take(bool isAsync)
        {
            await AssertQuery(
                isAsync,
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
        public override async Task Take_with_single(bool isAsync)
        {
            await base.Take_with_single(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Take_with_single_select_many(bool isAsync)
        {
            await AssertSingle(
                isAsync,
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
        public override async Task Distinct_Skip(bool isAsync)
        {
            await base.Distinct_Skip(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Distinct_Skip_Take(bool isAsync)
        {
            await base.Distinct_Skip_Take(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Skip_Distinct(bool isAsync)
        {
            await base.Skip_Distinct(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Skip_Take_Distinct(bool isAsync)
        {
            await base.Skip_Take_Distinct(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Skip_Take_Any(bool isAsync)
        {
            await base.Skip_Take_Any(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Skip_Take_All(bool isAsync)
        {
            await base.Skip_Take_All(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Take_All(bool isAsync)
        {
            await base.Take_All(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Skip_Take_Any_with_predicate(bool isAsync)
        {
            await base.Skip_Take_Any_with_predicate(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Take_Any_with_predicate(bool isAsync)
        {
            await base.Take_Any_with_predicate(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task OrderBy(bool isAsync)
        {
            await base.OrderBy(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")
ORDER BY c[""CustomerID""]");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_true(bool isAsync)
        {
            await base.OrderBy_true(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_integer(bool isAsync)
        {
            await base.OrderBy_integer(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_parameter(bool isAsync)
        {
            await base.OrderBy_parameter(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task OrderBy_anon(bool isAsync)
        {
            await base.OrderBy_anon(isAsync);

            AssertSql(
                @"SELECT c[""CustomerID""]
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")
ORDER BY c[""CustomerID""]");
        }

        public override async Task OrderBy_anon2(bool isAsync)
        {
            await base.OrderBy_anon2(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")
ORDER BY c[""CustomerID""]");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_client_mixed(bool isAsync)
        {
            await base.OrderBy_client_mixed(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_multiple_queries(bool isAsync)
        {
            await base.OrderBy_multiple_queries(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Take_Distinct(bool isAsync)
        {
            await base.Take_Distinct(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Distinct_Take(bool isAsync)
        {
            await base.Distinct_Take(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Distinct_Take_Count(bool isAsync)
        {
            await base.Distinct_Take_Count(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_shadow(bool isAsync)
        {
            await base.OrderBy_shadow(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_multiple(bool isAsync)
        {
            await base.OrderBy_multiple(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_ThenBy_Any(bool isAsync)
        {
            await base.OrderBy_ThenBy_Any(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_correlated_subquery1(bool isAsync)
        {
            await base.OrderBy_correlated_subquery1(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_correlated_subquery2(bool isAsync)
        {
            await base.OrderBy_correlated_subquery2(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_subquery_recursive_trivial(bool isAsync)
        {
            await base.Where_subquery_recursive_trivial(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_query_composition4(bool isAsync)
        {
            await base.Where_query_composition4(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_subquery_expression(bool isAsync)
        {
            await AssertQuery(
                isAsync,
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
        public override async Task Where_subquery_expression_same_parametername(bool isAsync)
        {
            await AssertQuery(
                isAsync,
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
        public override async Task Select_DTO_with_member_init_distinct_in_subquery_translated_to_server(bool isAsync)
        {
            await base.Select_DTO_with_member_init_distinct_in_subquery_translated_to_server(isAsync);

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

        public override async Task Select_correlated_subquery_projection(bool isAsync)
        {
            await base.Select_correlated_subquery_projection(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Select_correlated_subquery_filtered(bool isAsync)
        {
            await base.Select_correlated_subquery_filtered(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Select_correlated_subquery_ordered(bool isAsync)
        {
            await base.Select_correlated_subquery_ordered(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_subquery_on_bool(bool isAsync)
        {
            await base.Where_subquery_on_bool(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Product"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_subquery_on_collection(bool isAsync)
        {
            await AssertQuery(
                isAsync,
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
        public override async Task Select_many_cross_join_same_collection(bool isAsync)
        {
            await base.Select_many_cross_join_same_collection(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_null_coalesce_operator(bool isAsync)
        {
            await base.OrderBy_null_coalesce_operator(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_null_coalesce_operator(bool isAsync)
        {
            await base.Select_null_coalesce_operator(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_conditional_operator(bool isAsync)
        {
            await base.OrderBy_conditional_operator(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_conditional_operator_where_condition_false(bool isAsync)
        {
            await base.OrderBy_conditional_operator_where_condition_false(isAsync);

            AssertSql(
                @"@__p_0='false'

SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")
ORDER BY c[""City""]");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_comparison_operator(bool isAsync)
        {
            await base.OrderBy_comparison_operator(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory]
        public override async Task Projection_null_coalesce_operator(bool isAsync)
        {
            await base.Projection_null_coalesce_operator(isAsync);

            AssertSql(
                @"SELECT c[""CustomerID""], c[""CompanyName""], ((c[""Region""] != null) ? c[""Region""] : ""ZZ"") AS Region
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Filter_coalesce_operator(bool isAsync)
        {
            await base.Filter_coalesce_operator(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (((c[""CompanyName""] != null) ? c[""CompanyName""] : c[""ContactName""]) = ""The Big Cheese""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Take_skip_null_coalesce_operator(bool isAsync)
        {
            await base.Take_skip_null_coalesce_operator(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_take_null_coalesce_operator(bool isAsync)
        {
            await base.Select_take_null_coalesce_operator(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_take_skip_null_coalesce_operator(bool isAsync)
        {
            await base.Select_take_skip_null_coalesce_operator(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_take_skip_null_coalesce_operator2(bool isAsync)
        {
            await base.Select_take_skip_null_coalesce_operator2(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_take_skip_null_coalesce_operator3(bool isAsync)
        {
            await base.Select_take_skip_null_coalesce_operator3(isAsync);

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

        public override async Task DateTime_parse_is_inlined(bool isAsync)
        {
            await base.DateTime_parse_is_inlined(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderDate""] > ""1998-01-01T12:00:00""))");
        }

        public override async Task DateTime_parse_is_parameterized_when_from_closure(bool isAsync)
        {
            await base.DateTime_parse_is_parameterized_when_from_closure(isAsync);

            AssertSql(
                @"@__Parse_0='1998-01-01T12:00:00'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderDate""] > @__Parse_0))");
        }

        public override async Task New_DateTime_is_inlined(bool isAsync)
        {
            await base.New_DateTime_is_inlined(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderDate""] > ""1998-01-01T12:00:00""))");
        }

        public override async Task New_DateTime_is_parameterized_when_from_closure(bool isAsync)
        {
            await base.New_DateTime_is_parameterized_when_from_closure(isAsync);

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
        public override Task Random_next_is_not_funcletized_1(bool isAsync)
        {
            return base.Random_next_is_not_funcletized_1(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Random_next_is_not_funcletized_2(bool isAsync)
        {
            return base.Random_next_is_not_funcletized_2(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Random_next_is_not_funcletized_3(bool isAsync)
        {
            return base.Random_next_is_not_funcletized_3(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Random_next_is_not_funcletized_4(bool isAsync)
        {
            return base.Random_next_is_not_funcletized_4(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Random_next_is_not_funcletized_5(bool isAsync)
        {
            return base.Random_next_is_not_funcletized_5(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Random_next_is_not_funcletized_6(bool isAsync)
        {
            return base.Random_next_is_not_funcletized_6(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Environment_newline_is_funcletized(bool isAsync)
        {
            await base.Environment_newline_is_funcletized(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task String_concat_with_navigation1(bool isAsync)
        {
            await base.String_concat_with_navigation1(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task String_concat_with_navigation2(bool isAsync)
        {
            await base.String_concat_with_navigation2(isAsync);

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

        public override Task Where_bitwise_or_with_logical_or(bool isAsync)
        {
            // #13168
            //await base.Where_bitwise_or_with_logical_or(isAsync);

            return Task.CompletedTask;
        }

        public override async Task Where_bitwise_and_with_logical_and(bool isAsync)
        {
            await base.Where_bitwise_and_with_logical_and(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (((c[""CustomerID""] = ""ALFKI"") & (c[""CustomerID""] = ""ANATR"")) AND (c[""CustomerID""] = ""ANTON"")))");
        }

        public override Task Where_bitwise_or_with_logical_and(bool isAsync)
        {
            // #13168
            //await base.Where_bitwise_or_with_logical_and(isAsync);

            return Task.CompletedTask;
        }

        public override async Task Where_bitwise_and_with_logical_or(bool isAsync)
        {
            await base.Where_bitwise_and_with_logical_or(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (((c[""CustomerID""] = ""ALFKI"") & (c[""CustomerID""] = ""ANATR"")) OR (c[""CustomerID""] = ""ANTON"")))");
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
        public override async Task Handle_materialization_properly_when_more_than_two_query_sources_are_involved(bool isAsync)
        {
            await base.Handle_materialization_properly_when_more_than_two_query_sources_are_involved(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override Task Parameter_extraction_short_circuits_1(bool isAsync)
        {
            // #13159
            //await base.Parameter_extraction_short_circuits_1(isAsync);

            return Task.CompletedTask;
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Parameter_extraction_short_circuits_2(bool isAsync)
        {
            await base.Parameter_extraction_short_circuits_2(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override Task Parameter_extraction_short_circuits_3(bool isAsync)
        {
            // #13159
            //await base.Parameter_extraction_short_circuits_3(isAsync);

            return Task.CompletedTask;
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Subquery_member_pushdown_does_not_change_original_subquery_model(bool isAsync)
        {
            await base.Subquery_member_pushdown_does_not_change_original_subquery_model(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Query_expression_with_to_string_and_contains(bool isAsync)
        {
            await base.Query_expression_with_to_string_and_contains(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task Select_expression_long_to_string(bool isAsync)
        {
            await base.Select_expression_long_to_string(isAsync);

            AssertSql(
                @"SELECT c[""OrderID""]
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderDate""] != null))");
        }

        public override async Task Select_expression_int_to_string(bool isAsync)
        {
            await base.Select_expression_int_to_string(isAsync);

            AssertSql(
                @"SELECT c[""OrderID""]
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderDate""] != null))");
        }

        public override async Task ToString_with_formatter_is_evaluated_on_the_client(bool isAsync)
        {
            await base.ToString_with_formatter_is_evaluated_on_the_client(isAsync);

            AssertSql(
                @"SELECT c[""OrderID""]
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderDate""] != null))",
                //
                @"SELECT c[""OrderID""]
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderDate""] != null))");
        }

        public override async Task Select_expression_other_to_string(bool isAsync)
        {
            await base.Select_expression_other_to_string(isAsync);

            AssertSql(
                @"SELECT c[""OrderDate""]
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderDate""] != null))");
        }

        public override async Task Select_expression_date_add_year(bool isAsync)
        {
            await base.Select_expression_date_add_year(isAsync);

            AssertSql(
                @"SELECT c[""OrderDate""]
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderDate""] != null))");
        }

        public override async Task Select_expression_datetime_add_month(bool isAsync)
        {
            await base.Select_expression_datetime_add_month(isAsync);

            AssertSql(
                @"SELECT c[""OrderDate""]
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderDate""] != null))");
        }

        public override async Task Select_expression_datetime_add_hour(bool isAsync)
        {
            await base.Select_expression_datetime_add_hour(isAsync);

            AssertSql(
                @"SELECT c[""OrderDate""]
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderDate""] != null))");
        }

        public override async Task Select_expression_datetime_add_minute(bool isAsync)
        {
            await base.Select_expression_datetime_add_minute(isAsync);

            AssertSql(
                @"SELECT c[""OrderDate""]
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderDate""] != null))");
        }

        public override async Task Select_expression_datetime_add_second(bool isAsync)
        {
            await base.Select_expression_datetime_add_second(isAsync);

            AssertSql(
                @"SELECT c[""OrderDate""]
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderDate""] != null))");
        }

        public override async Task Select_expression_date_add_milliseconds_above_the_range(bool isAsync)
        {
            await base.Select_expression_date_add_milliseconds_above_the_range(isAsync);

            AssertSql(
                @"SELECT c[""OrderDate""]
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderDate""] != null))");
        }

        public override async Task Select_expression_date_add_milliseconds_below_the_range(bool isAsync)
        {
            await base.Select_expression_date_add_milliseconds_below_the_range(isAsync);

            AssertSql(
                @"SELECT c[""OrderDate""]
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderDate""] != null))");
        }

        public override async Task Select_expression_date_add_milliseconds_large_number_divided(bool isAsync)
        {
            await base.Select_expression_date_add_milliseconds_large_number_divided(isAsync);

            AssertSql(
                @"SELECT c[""OrderDate""]
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderDate""] != null))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_expression_references_are_updated_correctly_with_subquery(bool isAsync)
        {
            await base.Select_expression_references_are_updated_correctly_with_subquery(isAsync);

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
        public override async Task DefaultIfEmpty_in_subquery(bool isAsync)
        {
            await AssertQuery(
                isAsync,
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
        public override Task DefaultIfEmpty_in_subquery_not_correlated(bool isAsync)
        {
            return base.DefaultIfEmpty_in_subquery_not_correlated(isAsync);
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task DefaultIfEmpty_in_subquery_nested(bool isAsync)
        {
            await AssertQuery(
                isAsync,
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

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_skip_take(bool isAsync)
        {
            await base.OrderBy_skip_take(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_skip_skip_take(bool isAsync)
        {
            await base.OrderBy_skip_skip_take(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_skip_take_take(bool isAsync)
        {
            await base.OrderBy_skip_take_take(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_skip_take_take_take_take(bool isAsync)
        {
            await base.OrderBy_skip_take_take_take_take(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_skip_take_skip_take_skip(bool isAsync)
        {
            await base.OrderBy_skip_take_skip_take_skip(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_skip_take_distinct(bool isAsync)
        {
            await base.OrderBy_skip_take_distinct(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_coalesce_take_distinct(bool isAsync)
        {
            await base.OrderBy_coalesce_take_distinct(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Product"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_coalesce_skip_take_distinct(bool isAsync)
        {
            await base.OrderBy_coalesce_skip_take_distinct(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Product"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_coalesce_skip_take_distinct_take(bool isAsync)
        {
            await base.OrderBy_coalesce_skip_take_distinct_take(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Product"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_skip_take_distinct_orderby_take(bool isAsync)
        {
            await base.OrderBy_skip_take_distinct_orderby_take(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task No_orderby_added_for_fully_translated_manually_constructed_LOJ(bool isAsync)
        {
            await base.No_orderby_added_for_fully_translated_manually_constructed_LOJ(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        public override async Task No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ(bool isAsync)
        {
            await base.No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition1(
            bool isAsync)
        {
            await base.No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition1(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition2(
            bool isAsync)
        {
            await base.No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition2(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task Orderby_added_for_client_side_GroupJoin_principal_to_dependent_LOJ(bool isAsync)
        {
            await base.Orderby_added_for_client_side_GroupJoin_principal_to_dependent_LOJ(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Contains_with_DateTime_Date(bool isAsync)
        {
            await base.Contains_with_DateTime_Date(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Contains_with_subquery_involving_join_binds_to_correct_table(bool isAsync)
        {
            await AssertQuery(
                isAsync,
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
        public override async Task Complex_query_with_repeated_query_model_compiles_correctly(bool isAsync)
        {
            await base.Complex_query_with_repeated_query_model_compiles_correctly(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Complex_query_with_repeated_nested_query_model_compiles_correctly(bool isAsync)
        {
            await base.Complex_query_with_repeated_nested_query_model_compiles_correctly(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue#16144")]
        public override async Task Anonymous_member_distinct_where(bool isAsync)
        {
            await base.Anonymous_member_distinct_where(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Anonymous_member_distinct_orderby(bool isAsync)
        {
            await base.Anonymous_member_distinct_orderby(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Anonymous_member_distinct_result(bool isAsync)
        {
            await base.Anonymous_member_distinct_result(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#16144")]
        public override async Task Anonymous_complex_distinct_where(bool isAsync)
        {
            await base.Anonymous_complex_distinct_where(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Anonymous_complex_distinct_orderby(bool isAsync)
        {
            await base.Anonymous_complex_distinct_orderby(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Anonymous_complex_distinct_result(bool isAsync)
        {
            await base.Anonymous_complex_distinct_result(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Anonymous_complex_orderby(bool isAsync)
        {
            await base.Anonymous_complex_orderby(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Anonymous_subquery_orderby(bool isAsync)
        {
            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.City == "London").Where(c => c.Orders.Count > 1).Select(
                    c => new { A = c.Orders.OrderByDescending(o => o.OrderID).FirstOrDefault().OrderDate }).OrderBy(n => n.A),
                assertOrder: true);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = ""London""))");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task DTO_member_distinct_where(bool isAsync)
        {
            await base.DTO_member_distinct_where(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task DTO_member_distinct_orderby(bool isAsync)
        {
            await base.DTO_member_distinct_orderby(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task DTO_member_distinct_result(bool isAsync)
        {
            await base.DTO_member_distinct_result(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task DTO_complex_distinct_where(bool isAsync)
        {
            await base.DTO_complex_distinct_where(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task DTO_complex_distinct_orderby(bool isAsync)
        {
            await base.DTO_complex_distinct_orderby(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task DTO_complex_distinct_result(bool isAsync)
        {
            await base.DTO_complex_distinct_result(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task DTO_complex_orderby(bool isAsync)
        {
            await base.DTO_complex_orderby(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task DTO_subquery_orderby(bool isAsync)
        {
            await AssertQuery(
                isAsync,
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
        public override async Task Include_with_orderby_skip_preserves_ordering(bool isAsync)
        {
            await base.Include_with_orderby_skip_preserves_ordering(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND ((c[""CustomerID""] != ""VAFFE"") AND (c[""CustomerID""] != ""DRACD"")))");
        }

        public override async Task Int16_parameter_can_be_used_for_int_column(bool isAsync)
        {
            await base.Int16_parameter_can_be_used_for_int_column(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderID""] = 10300))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Subquery_is_null_translated_correctly(bool isAsync)
        {
            await AssertQuery(
                isAsync,
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
        public override async Task Subquery_is_not_null_translated_correctly(bool isAsync)
        {
            await AssertQuery(
                isAsync,
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
        public override async Task Select_take_average(bool isAsync)
        {
            await base.Select_take_average(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_take_count(bool isAsync)
        {
            await base.Select_take_count(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_orderBy_take_count(bool isAsync)
        {
            await base.Select_orderBy_take_count(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_take_long_count(bool isAsync)
        {
            await base.Select_take_long_count(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_orderBy_take_long_count(bool isAsync)
        {
            await base.Select_orderBy_take_long_count(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_take_max(bool isAsync)
        {
            await base.Select_take_max(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_take_min(bool isAsync)
        {
            await base.Select_take_min(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_take_sum(bool isAsync)
        {
            await base.Select_take_sum(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_skip_average(bool isAsync)
        {
            await base.Select_skip_average(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_skip_count(bool isAsync)
        {
            await base.Select_skip_count(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_orderBy_skip_count(bool isAsync)
        {
            await base.Select_orderBy_skip_count(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_skip_long_count(bool isAsync)
        {
            await base.Select_skip_long_count(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_orderBy_skip_long_count(bool isAsync)
        {
            await base.Select_orderBy_skip_long_count(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_skip_max(bool isAsync)
        {
            await base.Select_skip_max(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_skip_min(bool isAsync)
        {
            await base.Select_skip_min(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_skip_sum(bool isAsync)
        {
            await base.Select_skip_sum(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_distinct_average(bool isAsync)
        {
            await base.Select_distinct_average(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_distinct_count(bool isAsync)
        {
            await base.Select_distinct_count(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_distinct_long_count(bool isAsync)
        {
            await base.Select_distinct_long_count(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_distinct_max(bool isAsync)
        {
            await base.Select_distinct_max(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_distinct_min(bool isAsync)
        {
            await base.Select_distinct_min(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Select_distinct_sum(bool isAsync)
        {
            await base.Select_distinct_sum(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Comparing_to_fixed_string_parameter(bool isAsync)
        {
            await base.Comparing_to_fixed_string_parameter(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Comparing_entities_using_Equals(bool isAsync)
        {
            await base.Comparing_entities_using_Equals(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Comparing_different_entity_types_using_Equals(bool isAsync)
        {
            await AssertQuery(
                isAsync,
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
        public override async Task Comparing_entity_to_null_using_Equals(bool isAsync)
        {
            await base.Comparing_entity_to_null_using_Equals(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Comparing_navigations_using_Equals(bool isAsync)
        {
            await AssertQuery(
                isAsync,
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
        public override async Task Comparing_navigations_using_static_Equals(bool isAsync)
        {
            await AssertQuery(
                isAsync,
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
        public override async Task Comparing_non_matching_entities_using_Equals(bool isAsync)
        {
            await AssertQuery(
                isAsync,
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
        public override async Task Comparing_non_matching_collection_navigations_using_Equals(bool isAsync)
        {
            await AssertQuery(
                isAsync,
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
        public override async Task Comparing_collection_navigation_to_null(bool isAsync)
        {
            await base.Comparing_collection_navigation_to_null(isAsync);

            AssertSql(
                @"SELECT c[""CustomerID""]
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = null))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Comparing_collection_navigation_to_null_complex(bool isAsync)
        {
            await base.Comparing_collection_navigation_to_null_complex(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""OrderDetail"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Compare_collection_navigation_with_itself(bool isAsync)
        {
            await base.Compare_collection_navigation_with_itself(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Compare_two_collection_navigations_with_different_query_sources(bool isAsync)
        {
            await base.Compare_two_collection_navigations_with_different_query_sources(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Compare_two_collection_navigations_using_equals(bool isAsync)
        {
            await base.Compare_two_collection_navigations_using_equals(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Compare_two_collection_navigations_with_different_property_chains(bool isAsync)
        {
            await base.Compare_two_collection_navigations_with_different_property_chains(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_ThenBy_same_column_different_direction(bool isAsync)
        {
            await base.OrderBy_ThenBy_same_column_different_direction(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task OrderBy_OrderBy_same_column_different_direction(bool isAsync)
        {
            await base.OrderBy_OrderBy_same_column_different_direction(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Complex_nested_query_doesnt_try_binding_to_grandparent_when_parent_returns_complex_result(bool isAsync)
        {
            await base.Complex_nested_query_doesnt_try_binding_to_grandparent_when_parent_returns_complex_result(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Complex_nested_query_properly_binds_to_grandparent_when_parent_returns_scalar_result(bool isAsync)
        {
            await AssertQuery(
                isAsync,
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

        public override async Task OrderBy_Dto_projection_skip_take(bool isAsync)
        {
            await base.OrderBy_Dto_projection_skip_take(isAsync);

            AssertSql(
                @"@__p_0='5'
@__p_1='10'

SELECT c[""CustomerID""] AS Id
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")
ORDER BY c[""CustomerID""]
OFFSET @__p_0 LIMIT @__p_1");
        }

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
        public override async Task Join_take_count_works(bool isAsync)
        {
            await base.Join_take_count_works(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND ((c[""OrderID""] > 690) AND (c[""OrderID""] < 710)))");
        }

        public override async Task OrderBy_empty_list_contains(bool isAsync)
        {
            await base.OrderBy_empty_list_contains(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task OrderBy_empty_list_does_not_contains(bool isAsync)
        {
            await base.OrderBy_empty_list_does_not_contains(isAsync);

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
        public override async Task Let_subquery_with_multiple_occurrences(bool isAsync)
        {
            await AssertQuery(
                isAsync,
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
        public override async Task Let_entity_equality_to_null(bool isAsync)
        {
            await base.Let_entity_equality_to_null(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Let_entity_equality_to_other_entity(bool isAsync)
        {
            await base.Let_entity_equality_to_other_entity(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task SelectMany_after_client_method(bool isAsync)
        {
            await base.SelectMany_after_client_method(isAsync);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Collection_navigation_equal_to_null_for_subquery(bool isAsync)
        {
            await AssertQuery(
                isAsync,
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
        public override async Task Dependent_to_principal_navigation_equal_to_null_for_subquery(bool isAsync)
        {
            await AssertQuery(
                isAsync,
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
        public override async Task Collection_navigation_equality_rewrite_for_subquery(bool isAsync)
        {
            await base.Collection_navigation_equality_rewrite_for_subquery(isAsync);

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
        public override Task Entity_equality_through_nested_anonymous_type_projection(bool isAsync)
        {
            return base.Entity_equality_through_nested_anonymous_type_projection(isAsync);
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override async Task Entity_equality_through_DTO_projection(bool isAsync)
        {
            await base.Entity_equality_through_DTO_projection(isAsync);
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override Task GroupJoin_customers_employees_shadow(bool isAsync)
        {
            return base.GroupJoin_customers_employees_shadow(isAsync);
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override Task GroupJoin_customers_employees_subquery_shadow(bool isAsync)
        {
            return base.GroupJoin_customers_employees_subquery_shadow(isAsync);
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override Task GroupJoin_customers_employees_subquery_shadow_take(bool isAsync)
        {
            return base.GroupJoin_customers_employees_subquery_shadow_take(isAsync);
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override Task GroupJoin_projection(bool isAsync)
        {
            return base.GroupJoin_projection(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Cast_before_aggregate_is_preserved(bool isAsync)
        {
            return base.Cast_before_aggregate_is_preserved(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Decimal_cast_to_double_works(bool isAsync)
        {
            return base.Decimal_cast_to_double_works(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Entity_equality_through_subquery(bool isAsync)
        {
            return base.Entity_equality_through_subquery(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Enumerable_min_is_mapped_to_Queryable_1(bool isAsync)
        {
            return base.Enumerable_min_is_mapped_to_Queryable_1(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Enumerable_min_is_mapped_to_Queryable_2(bool isAsync)
        {
            return base.Enumerable_min_is_mapped_to_Queryable_2(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Join_customers_orders_entities_same_entity_twice(bool isAsync)
        {
            return base.Join_customers_orders_entities_same_entity_twice(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Multiple_collection_navigation_with_FirstOrDefault_chained_projecting_scalar(bool isAsync)
        {
            return base.Multiple_collection_navigation_with_FirstOrDefault_chained_projecting_scalar(isAsync);
        }

        [ConditionalFact(Skip = "Issue #17246")]
        public override void Can_convert_manually_build_expression_with_default()
        {
            base.Can_convert_manually_build_expression_with_default();
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Count_with_no_predicate(bool isAsync)
        {
            return base.Count_with_no_predicate(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Count_with_order_by(bool isAsync)
        {
            return base.Count_with_order_by(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Entity_equality_orderby_descending_composite_key(bool isAsync)
        {
            return base.Entity_equality_orderby_descending_composite_key(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Null_Coalesce_Short_Circuit(bool isAsync)
        {
            return base.Null_Coalesce_Short_Circuit(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task OrderByDescending_ThenBy(bool isAsync)
        {
            return base.OrderByDescending_ThenBy(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task OrderByDescending_ThenByDescending(bool isAsync)
        {
            return base.OrderByDescending_ThenByDescending(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task OrderBy_Join(bool isAsync)
        {
            return base.OrderBy_Join(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task OrderBy_ThenBy(bool isAsync)
        {
            return base.OrderBy_ThenBy(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task OrderBy_ThenBy_predicate(bool isAsync)
        {
            return base.OrderBy_ThenBy_predicate(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task SelectMany_correlated_simple(bool isAsync)
        {
            return base.SelectMany_correlated_simple(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task SelectMany_nested_simple(bool isAsync)
        {
            return base.SelectMany_nested_simple(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task SelectMany_primitive(bool isAsync)
        {
            return base.SelectMany_primitive(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task SelectMany_primitive_select_subquery(bool isAsync)
        {
            return base.SelectMany_primitive_select_subquery(isAsync);
        }

        [ConditionalFact(Skip = "Issue #17246")]
        public override void Select_DTO_constructor_distinct_with_navigation_translated_to_server()
        {
            base.Select_DTO_constructor_distinct_with_navigation_translated_to_server();
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Select_Property_when_shadow_unconstrained_generic_method(bool isAsync)
        {
            return base.Select_Property_when_shadow_unconstrained_generic_method(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Skip_orderby_const(bool isAsync)
        {
            return base.Skip_orderby_const(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Sum_with_no_arg_empty(bool isAsync)
        {
            return base.Sum_with_no_arg_empty(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Sum_with_no_data_nullable(bool isAsync)
        {
            return base.Sum_with_no_data_nullable(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Where_Property_when_shadow_unconstrained_generic_method(bool isAsync)
        {
            return base.Where_Property_when_shadow_unconstrained_generic_method(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Where_concat_string_int_comparison4(bool isAsync)
        {
            return base.Where_concat_string_int_comparison4(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Where_simple_shadow_subquery(bool isAsync)
        {
            return base.Where_simple_shadow_subquery(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Where_string_concat_method_comparison(bool isAsync)
        {
            return base.Where_string_concat_method_comparison(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Inner_parameter_in_nested_lambdas_gets_preserved(bool isAsync)
        {
            return base.Inner_parameter_in_nested_lambdas_gets_preserved(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Navigation_inside_interpolated_string_is_expanded(bool isAsync)
        {
            return base.Navigation_inside_interpolated_string_is_expanded(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Select_DTO_with_member_init_distinct_in_subquery_translated_to_server_2(bool isAsync)
        {
            return base.Select_DTO_with_member_init_distinct_in_subquery_translated_to_server_2(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task SelectMany_without_result_selector_naked_collection_navigation(bool isAsync)
        {
            return base.SelectMany_without_result_selector_naked_collection_navigation(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task SelectMany_without_result_selector_collection_navigation_composed(bool isAsync)
        {
            return base.SelectMany_without_result_selector_collection_navigation_composed(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task SelectMany_correlated_subquery_simple(bool isAsync)
        {
            return base.SelectMany_correlated_subquery_simple(isAsync);
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override Task Multiple_select_many_with_predicate(bool isAsync)
        {
            return base.Multiple_select_many_with_predicate(isAsync);
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override Task SelectMany_correlated_with_outer_1(bool isAsync)
        {
            return base.SelectMany_correlated_with_outer_1(isAsync);
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override Task SelectMany_correlated_with_outer_2(bool isAsync)
        {
            return base.SelectMany_correlated_with_outer_2(isAsync);
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override Task SelectMany_correlated_with_outer_3(bool isAsync)
        {
            return base.SelectMany_correlated_with_outer_3(isAsync);
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override Task SelectMany_correlated_with_outer_4(bool isAsync)
        {
            return base.SelectMany_correlated_with_outer_4(isAsync);
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override Task FirstOrDefault_over_empty_collection_of_value_type_returns_correct_results(bool isAsync)
        {
            return base.FirstOrDefault_over_empty_collection_of_value_type_returns_correct_results(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task All_client(bool isAsync) => base.All_client(isAsync);

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Client_OrderBy_GroupBy_Group_ordering_works(bool isAsync)
            => base.Client_OrderBy_GroupBy_Group_ordering_works(isAsync);

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Subquery_member_pushdown_does_not_change_original_subquery_model2(bool isAsync)
            => base.Subquery_member_pushdown_does_not_change_original_subquery_model2(isAsync);

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Where_query_composition3(bool isAsync) => base.Where_query_composition3(isAsync);

        public override Task Member_binding_after_ctor_arguments_fails_with_client_eval(bool isAsync)
        {
            return AssertTranslationFailed(() => base.Member_binding_after_ctor_arguments_fails_with_client_eval(isAsync));
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task OrderBy_object_type_server_evals(bool isAsync)
        {
            return base.OrderBy_object_type_server_evals(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task AsQueryable_in_query_server_evals(bool isAsync)
        {
            return base.AsQueryable_in_query_server_evals(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task SelectMany_correlated_subquery_hard(bool isAsync)
        {
            return base.SelectMany_correlated_subquery_hard(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task SelectMany_with_collection_being_correlated_subquery_which_references_inner_and_outer_entity(bool isAsync)
        {
            return base.SelectMany_with_collection_being_correlated_subquery_which_references_inner_and_outer_entity(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Select_chained_entity_navigation_doesnt_materialize_intermittent_entities(bool isAsync)
        {
            return base.Select_chained_entity_navigation_doesnt_materialize_intermittent_entities(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Select_entity_compared_to_null(bool isAsync)
        {
            return base.Select_entity_compared_to_null(isAsync);
        }

        public override async Task Explicit_cast_in_arithmatic_operation_is_preserved(bool isAsync)
        {
            await base.Explicit_cast_in_arithmatic_operation_is_preserved(isAsync);

            AssertSql(
                @"SELECT c[""OrderID""], (c[""OrderID""] + 1000) AS c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""OrderID""] = 10243))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task SelectMany_whose_selector_references_outer_source(bool isAsync)
        {
            return base.SelectMany_whose_selector_references_outer_source(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Collection_FirstOrDefault_with_entity_equality_check_in_projection(bool isAsync)
        {
            return base.Collection_FirstOrDefault_with_entity_equality_check_in_projection(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Collection_FirstOrDefault_with_nullable_unsigned_int_column(bool isAsync)
        {
            return base.Collection_FirstOrDefault_with_nullable_unsigned_int_column(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task IsNullOrWhiteSpace_in_predicate_on_non_nullable_column(bool isAsync)
        {
            return base.IsNullOrWhiteSpace_in_predicate_on_non_nullable_column(isAsync);
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
