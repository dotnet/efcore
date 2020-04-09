// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class OwnedQueryTestBase<TFixture> : QueryTestBase<TFixture>
        where TFixture : OwnedQueryTestBase<TFixture>.OwnedQueryFixtureBase, new()
    {
        protected OwnedQueryTestBase(TFixture fixture)
            : base(fixture)
        {
            fixture.ListLoggerFactory.Clear();
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Query_with_owned_entity_equality_operator(bool async)
        {
            return AssertQuery(
                async,
                ss => from a in ss.Set<LeafA>()
                      from b in ss.Set<LeafB>()
                      where a.LeafAAddress == b.LeafBAddress
                      select a);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Query_with_owned_entity_equality_method(bool async)
        {
            return AssertQuery(
                async,
                ss => from a in ss.Set<LeafA>()
                      from b in ss.Set<LeafB>()
                      where a.LeafAAddress.Equals(b.LeafBAddress)
                      select a);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Query_with_owned_entity_equality_object_method(bool async)
        {
            return AssertQuery(
                async,
                ss => from a in ss.Set<LeafA>()
                      from b in ss.Set<LeafB>()
                      where Equals(a.LeafAAddress, b.LeafBAddress)
                      select a);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Query_for_base_type_loads_all_owned_navs(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task No_ignored_include_warning_when_implicit_load(bool async)
        {
            return AssertCount(
                async,
                ss => ss.Set<OwnedPerson>());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Query_for_branch_type_loads_all_owned_navs(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Branch>());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Query_for_branch_type_loads_all_owned_navs_tracking(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Branch>().AsTracking(),
                entryCount: 14);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Query_for_leaf_type_loads_all_owned_navs(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<LeafA>());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Query_when_subquery(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>().Distinct()
                    .OrderBy(p => p.Id)
                    .Take(5)
                    .Select(op => new { op }),
                assertOrder: true,
                elementAsserter: (e, a) => AssertEqual(e.op, a.op));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_rewrite_on_owned_reference_projecting_scalar(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>().Where(p => p.PersonAddress.Country.Name == "USA")
                    .Select(p => p.PersonAddress.Country.Name));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_rewrite_on_owned_reference_projecting_entity(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>().Where(p => p.PersonAddress.Country.Name == "USA"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_rewrite_on_owned_collection(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>().Where(p => p.Orders.Count > 0).OrderBy(p => p.Id).Select(p => p.Orders),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_rewrite_on_owned_collection_with_composition(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<OwnedPerson>().OrderBy(p => p.Id)
                    .Select(p => p.Orders.OrderBy(o => o.Id).Select(o => o.Id != 42).FirstOrDefault()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_rewrite_on_owned_collection_with_composition_complex(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>().Select(
                    p => p.Orders.OrderBy(o => o.Id).Select(o => o.Client.PersonAddress.Country.Name).FirstOrDefault()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_on_owned_collection(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>().SelectMany(p => p.Orders));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Set_throws_for_owned_type(bool async)
        {
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => AssertQuery(async, ss => ss.Set<OwnedAddress>()));

            Assert.Equal(
                CoreStrings.InvalidSetTypeWeak(nameof(OwnedAddress)),
                exception.Message);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>().Select(p => p.PersonAddress.Country.Planet));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filter_owned_entity_chained_with_regular_entity_followed_by_projecting_owned_collection(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>().Where(p => p.PersonAddress.Country.Planet.Id != 42).OrderBy(p => p.Id)
                    .Select(p => new { p.Orders }),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e.Orders, a.Orders));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_multiple_owned_navigations(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>().OrderBy(p => p.Id)
                    .Select(
                        p => new
                        {
                            p.Orders,
                            p.PersonAddress,
                            p.PersonAddress.Country.Planet
                        }),
                assertOrder: true,
                elementAsserter: (e, a) =>
                {
                    AssertCollection(e.Orders, a.Orders);
                    AssertEqual(e.PersonAddress, a.PersonAddress);
                    AssertEqual(e.Planet, a.Planet);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_multiple_owned_navigations_with_expansion_on_owned_collections(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>().OrderBy(p => p.Id).Select(
                    p => new
                    {
                        Count = p.Orders.Where(o => o.Client.PersonAddress.Country.Planet.Star.Id != 42).Count(),
                        p.PersonAddress.Country.Planet
                    }),
                assertOrder: true,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Count, a.Count);
                    AssertEqual(e.Planet, a.Planet);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_filter(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>().Where(p => p.PersonAddress.Country.Planet.Id != 7).Select(p => new { p }),
                elementSorter: e => e.p.Id,
                elementAsserter: (e, a) => AssertEqual(e.p, a.p));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_property(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<OwnedPerson>().Select(p => p.PersonAddress.Country.Planet.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>().OrderBy(p => p.Id).Select(p => p.PersonAddress.Country.Planet.Moons),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_on_owned_reference_followed_by_regular_entity_and_collection(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>().SelectMany(p => p.PersonAddress.Country.Planet.Moons));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_on_owned_reference_with_entity_in_between_ending_in_owned_collection(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>().SelectMany(p => p.PersonAddress.Country.Planet.Star.Composition));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection_count(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<OwnedPerson>().Select(p => p.PersonAddress.Country.Planet.Moons.Count));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>().Select(p => p.PersonAddress.Country.Planet.Star));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_and_scalar(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>().Select(p => p.PersonAddress.Country.Planet.Star.Name),
                elementSorter: e => e);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task
            Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_in_predicate_and_projection(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>().Where(p => p.PersonAddress.Country.Planet.Star.Name == "Sol")
                    .Select(p => p.PersonAddress.Country.Planet.Star));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Query_with_OfType_eagerly_loads_correct_owned_navigations(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>().OfType<LeafA>());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Query_loads_reference_nav_automatically_in_projection(bool async)
        {
            return AssertSingle(
                async,
                ss => ss.Set<Fink>().Select(e => e.Barton));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Throw_for_owned_entities_without_owner_in_tracking_query(bool async)
        {
            using var context = CreateContext();
            var query = context.Set<OwnedPerson>().Select(e => e.PersonAddress);
            var noTrackingQuery = query.AsNoTracking();
            var asTrackingQuery = query.AsTracking();

            var result = async
                ? await noTrackingQuery.ToListAsync()
                : query.AsNoTracking().ToList();

            Assert.Equal(4, result.Count);
            Assert.Empty(context.ChangeTracker.Entries());

            if (async)
            {
                await Assert.ThrowsAsync<InvalidOperationException>(() => asTrackingQuery.ToListAsync());
            }
            else
            {
                Assert.Throws<InvalidOperationException>(() => asTrackingQuery.ToList());
            }

            Assert.Empty(context.ChangeTracker.Entries());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Preserve_includes_when_applying_skip_take_after_anonymous_type_select(bool async)
        {
            using var context = CreateContext();
            var expectedQuery = Fixture.QueryAsserter.ExpectedData.Set<OwnedPerson>().OrderBy(p => p.Id);
            var expectedResult = expectedQuery.Select(q => new { Query = q, Count = expectedQuery.Count() }).Skip(0).Take(100).ToList();

            var baseQuery = context.Set<OwnedPerson>().OrderBy(p => p.Id);
            var query = baseQuery.Select(q => new { Query = q, Count = baseQuery.Count() }).Skip(0).Take(100);

            var result = async
                ? await query.ToListAsync()
                : query.ToList();

            Assert.Equal(expectedResult.Count, result.Count);
            for (var i = 0; i < result.Count; i++)
            {
                AssertEqual(expectedResult[i].Query, result[i].Query);
                Assert.Equal(expectedResult[i].Count, result[i].Count);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Unmapped_property_projection_loads_owned_navigations(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>().Where(e => e.Id == 1).AsTracking().Select(e => new { e.ReadOnlyProperty }),
                entryCount: 5);
        }

        // Issue#18140
        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Client_method_skip_loads_owned_navigations(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>().OrderBy(e => e.Id).Select(e => Map(e)).Skip(1));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Client_method_take_loads_owned_navigations(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>().OrderBy(e => e.Id).Select(e => Map(e)).Take(2));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Client_method_skip_take_loads_owned_navigations(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>().OrderBy(e => e.Id).Select(e => Map(e)).Skip(1).Take(2));
        }

        private static string Map(OwnedPerson person) => person.PersonAddress.Country.Name;

        // Issue#18734
        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Client_method_skip_loads_owned_navigations_variation_2(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>().OrderBy(e => e.Id).Select(e => Identity(e)).Skip(1));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Client_method_take_loads_owned_navigations_variation_2(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>().OrderBy(e => e.Id).Select(e => Identity(e)).Take(2));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Client_method_skip_take_loads_owned_navigations_variation_2(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>().OrderBy(e => e.Id).Select(e => Identity(e)).Skip(1).Take(2));
        }

        private static OwnedPerson Identity(OwnedPerson person) => person;

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_owned_collection_navigation_ToList_Count(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>()
                    .OrderBy(p => p.Id)
                    .Select(p => p.Orders.ToList())
                    .Where(e => e.Count() == 0),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_collection_navigation_ToArray_Count(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>()
                    .OrderBy(p => p.Id)
                    .Select(p => p.Orders.ToArray())
                    .Where(e => e.Count() == 0),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_collection_navigation_AsEnumerable_Count(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>()
                    .OrderBy(p => p.Id)
                    .Select(p => p.Orders.AsEnumerable())
                    .Where(e => e.Count() == 0),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_collection_navigation_ToList_Count_member(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>()
                    .OrderBy(p => p.Id)
                    .Select(p => p.Orders.ToList())
                    .Where(e => e.Count == 0),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_collection_navigation_ToArray_Length_member(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>()
                    .OrderBy(p => p.Id)
                    .Select(p => p.Orders.ToArray())
                    .Where(e => e.Length == 0),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_query_on_indexer_properties(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>().Where(c => (string)c["Name"] == "Mona Cy"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_query_on_owned_indexer_properties(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>().Where(c => (int)c.PersonAddress["ZipCode"] == 38654).Select(c => (string)c["Name"]));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_query_on_indexer_property_when_property_name_from_closure(bool async)
        {
            var propertyName = "Name";
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>().Where(c => (string)c[propertyName] == "Mona Cy").Select(c => (string)c["Name"]));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_project_indexer_properties(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>().Select(c => c["Name"]));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_project_owned_indexer_properties(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>().Select(c => c.PersonAddress["AddressLine"]));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_project_indexer_properties_converted(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>().Select(c => (string)c["Name"]));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_project_owned_indexer_properties_converted(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>().Select(c => (string)c.PersonAddress["AddressLine"]));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_OrderBy_indexer_properties(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>().OrderBy(c => c["Name"]).ThenBy(c => c.Id),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_OrderBy_indexer_properties_converted(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>().OrderBy(c => (string)c["Name"]).ThenBy(c => c.Id).Select(c => (string)c["Name"]),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_OrderBy_owned_indexer_properties(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>().OrderBy(c => c.PersonAddress["ZipCode"]).ThenBy(c => c.Id).Select(c => (string)c["Name"]),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_OrderBy_owened_indexer_properties_converted(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OwnedPerson>().OrderBy(c => (int)c.PersonAddress["ZipCode"]).ThenBy(c => c.Id).Select(c => (string)c["Name"]),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_group_by_indexer_property(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<OwnedPerson>().GroupBy(c => c["Name"]).Select(g => g.Count()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_group_by_converted_indexer_property(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<OwnedPerson>().GroupBy(c => (string)c["Name"]).Select(g => g.Count()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_group_by_owned_indexer_property(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<OwnedPerson>().GroupBy(c => c.PersonAddress["ZipCode"]).Select(g => g.Count()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_group_by_converted_owned_indexer_property(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<OwnedPerson>().GroupBy(c => (int)c.PersonAddress["ZipCode"]).Select(g => g.Count()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_join_on_indexer_property_on_query(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    (from c1 in ss.Set<OwnedPerson>()
                     join c2 in ss.Set<OwnedPerson>()
                         on c1.PersonAddress["ZipCode"] equals c2.PersonAddress["ZipCode"]
                     select new { c1.Id, c2.PersonAddress.Country.Name }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Projecting_indexer_property_ignores_include(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from c in ss.Set<OwnedPerson>().AsTracking()
                      select new { Nation = c.PersonAddress["ZipCode"] });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Projecting_indexer_property_ignores_include_converted(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from c in ss.Set<OwnedPerson>().AsTracking()
                      select new { Nation = (int)c.PersonAddress["ZipCode"] });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Indexer_property_is_pushdown_into_subquery(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<OwnedPerson>()
                    .Where(g => (string)ss.Set<OwnedPerson>().Where(c => c.Id == g.Id).FirstOrDefault()["Name"] == "Mona Cy")
                    .Select(c => (string)c["Name"]));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Can_query_indexer_property_on_owned_collection(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<OwnedPerson>().Where(ow => ow.Orders.Where(o => ((DateTime)o["OrderDate"]).Year == 2018).Count() == 1)
                    .Select(c => (string)c["Name"]));
        }

        protected virtual DbContext CreateContext() => Fixture.CreateContext();

        public abstract class OwnedQueryFixtureBase : SharedStoreFixtureBase<PoolableDbContext>, IQueryFixtureBase
        {
            private static void AssertAddress(OwnedAddress expectedAddress, OwnedAddress actualAddress)
            {
                Assert.Equal(expectedAddress["AddressLine"], actualAddress["AddressLine"]);
                Assert.Equal(expectedAddress["ZipCode"], actualAddress["ZipCode"]);
                Assert.Equal(expectedAddress["BranchName"], actualAddress["BranchName"]);
                Assert.Equal(expectedAddress["LeafType"], actualAddress["LeafType"]);
                Assert.Equal(expectedAddress["LeafBType"], actualAddress["LeafBType"]);
                Assert.Equal(expectedAddress.PlaceType, actualAddress.PlaceType);
                Assert.Equal(expectedAddress.Country.PlanetId, actualAddress.Country.PlanetId);
                Assert.Equal(expectedAddress.Country.Name, actualAddress.Country.Name);
            }

            private static void AssertOrders(ICollection<Order> expectedOrders, ICollection<Order> actualOrders)
            {
                Assert.Equal(expectedOrders.Count, actualOrders.Count);
                foreach (var element in expectedOrders.OrderBy(ee => ee.Id).Zip(actualOrders.OrderBy(aa => aa.Id), (e, a) => new { e, a }))
                {
                    Assert.Equal(element.e.Id, element.a.Id);
                    Assert.Equal(element.e["OrderDate"], element.a["OrderDate"]);
                    Assert.Equal(element.e.Client.Id, element.a.Client.Id);
                }
            }

            public OwnedQueryFixtureBase()
            {
                var entitySorters = new Dictionary<Type, Func<object, object>>
                {
                    { typeof(OwnedPerson), e => ((OwnedPerson)e)?.Id },
                    { typeof(Branch), e => ((Branch)e)?.Id },
                    { typeof(LeafA), e => ((LeafA)e)?.Id },
                    { typeof(LeafB), e => ((LeafB)e)?.Id },
                    { typeof(Planet), e => ((Planet)e)?.Id },
                    { typeof(Star), e => ((Star)e)?.Id },
                    { typeof(Moon), e => ((Moon)e)?.Id },
                    { typeof(Fink), e => ((Fink)e)?.Id },
                    { typeof(Barton), e => ((Barton)e)?.Id },

                    // owned entities - still need comparers in case they are projected directly
                    { typeof(Order), e => ((Order)e)?.Id },
                    { typeof(OwnedAddress), e => ((OwnedAddress)e)?.Country.Name },
                    { typeof(OwnedCountry), e => ((OwnedCountry)e)?.Name },
                    { typeof(Element), e => ((Element)e)?.Id },
                    { typeof(Throned), e => ((Throned)e)?.Property }
                }.ToDictionary(e => e.Key, e => (object)e.Value);
                ;

                var entityAsserters = new Dictionary<Type, Action<object, object>>
                {
                    {
                        typeof(OwnedPerson), (e, a) =>
                        {
                            Assert.Equal(e == null, a == null);
                            if (a != null)
                            {
                                var ee = (OwnedPerson)e;
                                var aa = (OwnedPerson)a;

                                Assert.Equal(ee.Id, aa.Id);
                                Assert.Equal(ee["Name"], aa["Name"]);
                                AssertAddress(ee.PersonAddress, aa.PersonAddress);
                                AssertOrders(ee.Orders, aa.Orders);
                            }

                            if (e is Branch branch)
                            {
                                AssertAddress(branch.BranchAddress, ((Branch)a).BranchAddress);
                            }

                            if (e is LeafA leafA)
                            {
                                AssertAddress(leafA.LeafAAddress, ((LeafA)a).LeafAAddress);
                            }

                            if (e is LeafB leafB)
                            {
                                AssertAddress(leafB.LeafBAddress, ((LeafB)a).LeafBAddress);
                            }
                        }
                    },
                    {
                        typeof(Branch), (e, a) =>
                        {
                            Assert.Equal(e == null, a == null);
                            if (a != null)
                            {
                                var ee = (Branch)e;
                                var aa = (Branch)a;

                                Assert.Equal(ee.Id, aa.Id);
                                AssertAddress(ee.PersonAddress, aa.PersonAddress);
                                AssertAddress(ee.BranchAddress, aa.BranchAddress);
                                AssertOrders(ee.Orders, aa.Orders);
                            }

                            if (e is LeafA leafA)
                            {
                                AssertAddress(leafA.LeafAAddress, ((LeafA)a).LeafAAddress);
                            }
                        }
                    },
                    {
                        typeof(LeafA), (e, a) =>
                        {
                            Assert.Equal(e == null, a == null);
                            if (a != null)
                            {
                                var ee = (LeafA)e;
                                var aa = (LeafA)a;

                                Assert.Equal(ee.Id, aa.Id);
                                AssertAddress(ee.PersonAddress, aa.PersonAddress);
                                AssertAddress(ee.BranchAddress, aa.BranchAddress);
                                AssertAddress(ee.LeafAAddress, aa.LeafAAddress);
                                AssertOrders(ee.Orders, aa.Orders);
                            }
                        }
                    },
                    {
                        typeof(LeafB), (e, a) =>
                        {
                            Assert.Equal(e == null, a == null);
                            if (a != null)
                            {
                                var ee = (LeafB)e;
                                var aa = (LeafB)a;

                                Assert.Equal(ee.Id, aa.Id);
                                AssertAddress(ee.PersonAddress, aa.PersonAddress);
                                AssertAddress(ee.LeafBAddress, aa.LeafBAddress);
                                AssertOrders(ee.Orders, aa.Orders);
                            }
                        }
                    },
                    {
                        typeof(Planet), (e, a) =>
                        {
                            Assert.Equal(e == null, a == null);
                            if (a != null)
                            {
                                var ee = (Planet)e;
                                var aa = (Planet)a;

                                Assert.Equal(ee.Id, aa.Id);
                                Assert.Equal(ee.StarId, aa.StarId);
                            }
                        }
                    },
                    {
                        typeof(Star), (e, a) =>
                        {
                            Assert.Equal(e == null, a == null);
                            if (a != null)
                            {
                                var ee = (Star)e;
                                var aa = (Star)a;

                                Assert.Equal(ee.Id, aa.Id);
                                Assert.Equal(ee.Name, aa.Name);
                                Assert.Equal(ee.Composition.Count, aa.Composition.Count);
                                for (var i = 0; i < ee.Composition.Count; i++)
                                {
                                    Assert.Equal(ee.Composition[i].Id, aa.Composition[i].Id);
                                    Assert.Equal(ee.Composition[i].Name, aa.Composition[i].Name);
                                    Assert.Equal(ee.Composition[i].StarId, aa.Composition[i].StarId);
                                }
                            }
                        }
                    },
                    {
                        typeof(Moon), (e, a) =>
                        {
                            Assert.Equal(e == null, a == null);
                            if (a != null)
                            {
                                var ee = (Moon)e;
                                var aa = (Moon)a;

                                Assert.Equal(ee.Id, aa.Id);
                                Assert.Equal(ee.PlanetId, aa.PlanetId);
                                Assert.Equal(ee.Diameter, aa.Diameter);
                            }
                        }
                    },
                    {
                        typeof(Fink), (e, a) =>
                        {
                            Assert.Equal(e == null, a == null);
                            if (a != null)
                            {
                                Assert.Equal(((Fink)e).Id, ((Fink)a).Id);
                            }
                        }
                    },
                    {
                        typeof(Barton), (e, a) =>
                        {
                            Assert.Equal(e == null, a == null);
                            if (a != null)
                            {
                                var ee = (Barton)e;
                                var aa = (Barton)a;

                                Assert.Equal(ee.Id, aa.Id);
                                Assert.Equal(ee.Simple, aa.Simple);
                                Assert.Equal(ee.Throned.Property, aa.Throned.Property);
                            }
                        }
                    },

                    // owned entities - still need comparers in case they are projected directly
                    {
                        typeof(Order), (e, a) =>
                        {
                            Assert.Equal(e == null, a == null);
                            if (a != null)
                            {
                                Assert.Equal(((Order)e).Id, ((Order)a).Id);
                            }
                        }
                    },
                    {
                        typeof(OwnedAddress), (e, a) =>
                        {
                            AssertAddress(((OwnedAddress)e), ((OwnedAddress)a));
                        }
                    },
                    {
                        typeof(OwnedCountry), (e, a) =>
                        {
                            Assert.Equal(e == null, a == null);
                            if (a != null)
                            {
                                var ee = (OwnedCountry)e;
                                var aa = (OwnedCountry)a;

                                Assert.Equal(ee.Name, aa.Name);
                                Assert.Equal(ee.PlanetId, aa.PlanetId);
                            }
                        }
                    },
                    {
                        typeof(Element), (e, a) =>
                        {
                            Assert.Equal(e == null, a == null);
                            if (a != null)
                            {
                                var ee = (Element)e;
                                var aa = (Element)a;

                                Assert.Equal(ee.Id, aa.Id);
                                Assert.Equal(ee.Name, aa.Name);
                                Assert.Equal(ee.StarId, aa.StarId);
                            }
                        }
                    },
                    {
                        typeof(Throned), (e, a) =>
                        {
                            Assert.Equal(e == null, a == null);
                            if (a != null)
                            {
                                Assert.Equal(((Throned)e).Property, ((Throned)a).Property);
                            }
                        }
                    }
                }.ToDictionary(e => e.Key, e => (object)e.Value);
                ;

                QueryAsserter = CreateQueryAsserter(entitySorters, entityAsserters);
            }

            protected virtual QueryAsserter<PoolableDbContext> CreateQueryAsserter(
                Dictionary<Type, object> entitySorters,
                Dictionary<Type, object> entityAsserters)
                => new QueryAsserter<PoolableDbContext>(
                    CreateContext,
                    new OwnedQueryData(),
                    entitySorters,
                    entityAsserters);

            protected override string StoreName { get; } = "OwnedQueryTest";

            public QueryAsserterBase QueryAsserter { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                modelBuilder.Entity<OwnedPerson>(
                    eb =>
                    {
                        eb.IndexerProperty<string>("Name");
                        var ownedPerson = new OwnedPerson { Id = 1 };
                        ownedPerson["Name"] = "Mona Cy";
                        eb.HasData(ownedPerson);

                        eb.OwnsOne(
                            p => p.PersonAddress, ab =>
                            {
                                ab.IndexerProperty<string>("AddressLine");
                                ab.IndexerProperty(typeof(int), "ZipCode");
                                ab.HasData(
                                    new { OwnedPersonId = 1, PlaceType = "Land", AddressLine = "804 S. Lakeshore Road", ZipCode = 38654 },
                                    new { OwnedPersonId = 2, PlaceType = "Land", AddressLine = "7 Church Dr.", ZipCode = 28655 },
                                    new { OwnedPersonId = 3, PlaceType = "Land", AddressLine = "72 Hickory Rd.", ZipCode = 07728 },
                                    new { OwnedPersonId = 4, PlaceType = "Land", AddressLine = "28 Strawberry St.", ZipCode = 19053 });

                                ab.OwnsOne(
                                    a => a.Country, cb =>
                                    {
                                        cb.HasData(
                                            new
                                            {
                                                OwnedAddressOwnedPersonId = 1,
                                                PlanetId = 1,
                                                Name = "USA"
                                            },
                                            new
                                            {
                                                OwnedAddressOwnedPersonId = 2,
                                                PlanetId = 1,
                                                Name = "USA"
                                            },
                                            new
                                            {
                                                OwnedAddressOwnedPersonId = 3,
                                                PlanetId = 1,
                                                Name = "USA"
                                            },
                                            new
                                            {
                                                OwnedAddressOwnedPersonId = 4,
                                                PlanetId = 1,
                                                Name = "USA"
                                            });

                                        cb.HasOne(cc => cc.Planet).WithMany().HasForeignKey(ee => ee.PlanetId)
                                            .OnDelete(DeleteBehavior.Restrict);
                                    });
                            });

                        eb.OwnsMany(
                            p => p.Orders, ob =>
                            {
                                ob.IndexerProperty<DateTime>("OrderDate");
                                ob.HasData(
                                    new { Id = -10, ClientId = 1, OrderDate = Convert.ToDateTime("2018-07-11 10:01:41") },
                                    new { Id = -11, ClientId = 1, OrderDate = Convert.ToDateTime("2015-03-03 04:37:59") },
                                    new { Id = -20, ClientId = 2, OrderDate = Convert.ToDateTime("2015-05-25 20:35:48") },
                                    new { Id = -30, ClientId = 3, OrderDate = Convert.ToDateTime("2014-11-10 04:32:42") },
                                    new { Id = -40, ClientId = 4, OrderDate = Convert.ToDateTime("2016-04-25 19:23:56") }
                                );
                            });
                    });

                modelBuilder.Entity<Branch>(
                    eb =>
                    {
                        eb.HasData(new { Id = 2, Name = "Antigonus Mitul" });

                        eb.OwnsOne(
                            p => p.BranchAddress, ab =>
                            {
                                ab.IndexerProperty<string>("BranchName");
                                ab.HasData(
                                    new { BranchId = 2, PlaceType = "Land", BranchName = "BranchA" },
                                    new { BranchId = 3, PlaceType = "Land", BranchName = "BranchB" });

                                ab.OwnsOne(
                                    a => a.Country, cb =>
                                    {
                                        cb.HasData(
                                            new
                                            {
                                                OwnedAddressBranchId = 2,
                                                PlanetId = 1,
                                                Name = "Canada"
                                            },
                                            new
                                            {
                                                OwnedAddressBranchId = 3,
                                                PlanetId = 1,
                                                Name = "Canada"
                                            });
                                    });
                            });
                    });

                modelBuilder.Entity<LeafA>(
                    eb =>
                    {
                        var leafA = new LeafA { Id = 3 };
                        leafA["Name"] = "Madalena Morana";
                        eb.HasData(leafA);

                        eb.OwnsOne(
                            p => p.LeafAAddress, ab =>
                            {
                                ab.IndexerProperty<int>("LeafType");

                                ab.HasData(
                                    new { LeafAId = 3, PlaceType = "Land", LeafType = 1 });

                                ab.OwnsOne(
                                    a => a.Country, cb =>
                                    {
                                        cb.HasOne(c => c.Planet).WithMany().HasForeignKey(c => c.PlanetId)
                                            .OnDelete(DeleteBehavior.Restrict);

                                        cb.HasData(
                                            new
                                            {
                                                OwnedAddressLeafAId = 3,
                                                PlanetId = 1,
                                                Name = "Mexico"
                                            });
                                    });
                            });
                    });

                modelBuilder.Entity<LeafB>(
                    eb =>
                    {
                        var leafB = new LeafB { Id = 4 };
                        leafB["Name"] = "Vanda Waldemar";
                        eb.HasData(leafB);

                        eb.OwnsOne(
                            p => p.LeafBAddress, ab =>
                            {
                                ab.IndexerProperty<string>("LeafBType");
                                ab.HasData(
                                    new { LeafBId = 4, PlaceType = "Land", LeafBType = "Green" });

                                ab.OwnsOne(
                                    a => a.Country, cb =>
                                    {
                                        cb.HasOne(c => c.Planet).WithMany().HasForeignKey(c => c.PlanetId)
                                            .OnDelete(DeleteBehavior.Restrict);

                                        cb.HasData(
                                            new
                                            {
                                                OwnedAddressLeafBId = 4,
                                                PlanetId = 1,
                                                Name = "Panama"
                                            });
                                    });
                            });
                    });

                modelBuilder.Entity<Planet>(pb => pb.HasData(new Planet { Id = 1, StarId = 1 }));

                modelBuilder.Entity<Moon>(
                    mb => mb.HasData(
                        new Moon
                        {
                            Id = 1,
                            PlanetId = 1,
                            Diameter = 3474
                        }));

                modelBuilder.Entity<Star>(
                    sb =>
                    {
                        sb.HasData(new Star { Id = 1, Name = "Sol" });
                        sb.OwnsMany(
                            s => s.Composition, ob =>
                            {
                                ob.HasKey(e => e.Id);
                                ob.HasData(
                                    new
                                    {
                                        Id = "H",
                                        Name = "Hydrogen",
                                        StarId = 1
                                    },
                                    new
                                    {
                                        Id = "He",
                                        Name = "Helium",
                                        StarId = 1
                                    });
                            });
                    });

                modelBuilder.Entity<Barton>(
                    b =>
                    {
                        b.OwnsOne(
                            e => e.Throned, b => b.HasData(
                                new { BartonId = 1, Property = "Property" }));
                        b.HasData(
                            new Barton { Id = 1, Simple = "Simple" });
                    });

                modelBuilder.Entity<Fink>().HasData(
                    new { Id = 1, BartonId = 1 });
            }

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder).ConfigureWarnings(wcb => wcb.Throw());

            public override PoolableDbContext CreateContext()
            {
                var context = base.CreateContext();
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                return context;
            }
        }

        protected class OwnedQueryData : ISetSource
        {
            private readonly IReadOnlyList<OwnedPerson> _ownedPeople;
            private readonly IReadOnlyList<Planet> _planets;
            private readonly IReadOnlyList<Star> _stars;
            private readonly IReadOnlyList<Moon> _moons;
            private readonly IReadOnlyList<Fink> _finks;
            private readonly IReadOnlyList<Barton> _bartons;

            public OwnedQueryData()
            {
                _ownedPeople = CreateOwnedPeople();
                _planets = CreatePlanets();
                _stars = CreateStars();
                _moons = CreateMoons();
                _finks = CreateFinks();
                _bartons = CreateBartons();

                WireUp(_ownedPeople, _planets, _stars, _moons, _finks, _bartons);
            }

            public virtual IQueryable<TEntity> Set<TEntity>()
                where TEntity : class
            {
                if (typeof(TEntity) == typeof(OwnedPerson))
                {
                    return (IQueryable<TEntity>)_ownedPeople.AsQueryable();
                }

                if (typeof(TEntity) == typeof(Branch))
                {
                    return (IQueryable<TEntity>)_ownedPeople.OfType<Branch>().AsQueryable();
                }

                if (typeof(TEntity) == typeof(LeafA))
                {
                    return (IQueryable<TEntity>)_ownedPeople.OfType<LeafA>().AsQueryable();
                }

                if (typeof(TEntity) == typeof(LeafB))
                {
                    return (IQueryable<TEntity>)_ownedPeople.OfType<LeafB>().AsQueryable();
                }

                if (typeof(TEntity) == typeof(Planet))
                {
                    return (IQueryable<TEntity>)_planets.AsQueryable();
                }

                if (typeof(TEntity) == typeof(Moon))
                {
                    return (IQueryable<TEntity>)_moons.AsQueryable();
                }

                if (typeof(TEntity) == typeof(Fink))
                {
                    return (IQueryable<TEntity>)_finks.AsQueryable();
                }

                if (typeof(TEntity) == typeof(Barton))
                {
                    return (IQueryable<TEntity>)_bartons.AsQueryable();
                }

                throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
            }

            private static IReadOnlyList<Planet> CreatePlanets()
                => new List<Planet> { new Planet { Id = 1, StarId = 1 } };

            private static IReadOnlyList<Star> CreateStars()
                => new List<Star>
                {
                    new Star
                    {
                        Id = 1,
                        Name = "Sol",
                        Composition = new List<Element>
                        {
                            new Element
                            {
                                Id = "H",
                                Name = "Hydrogen",
                                StarId = 1
                            },
                            new Element
                            {
                                Id = "He",
                                Name = "Helium",
                                StarId = 1
                            }
                        }
                    }
                };

            private static IReadOnlyList<Moon> CreateMoons()
                => new List<Moon>
                {
                    new Moon
                    {
                        Id = 1,
                        PlanetId = 1,
                        Diameter = 3474
                    }
                };

            private static IReadOnlyList<OwnedPerson> CreateOwnedPeople()
            {
                var personAddress1 = new OwnedAddress { PlaceType = "Land", Country = new OwnedCountry { Name = "USA", PlanetId = 1 } };
                personAddress1["AddressLine"] = "804 S. Lakeshore Road";
                personAddress1["ZipCode"] = 38654;
                var ownedPerson1 = new OwnedPerson
                {
                    Id = 1,
                    PersonAddress = personAddress1
                };
                ownedPerson1["Name"] = "Mona Cy";

                var personAddress2 = new OwnedAddress { PlaceType = "Land", Country = new OwnedCountry { Name = "USA", PlanetId = 1 } };
                personAddress2["AddressLine"] = "7 Church Dr.";
                personAddress2["ZipCode"] = 28655;
                var branchAddress2 = new OwnedAddress { PlaceType = "Land", Country = new OwnedCountry { Name = "Canada", PlanetId = 1 } };
                branchAddress2["BranchName"] = "BranchA";

                var ownedPerson2 = new Branch
                {
                    Id = 2,
                    PersonAddress = personAddress2,
                    BranchAddress = branchAddress2
                };
                ownedPerson2["Name"] = "Antigonus Mitul";

                var personAddress3 = new OwnedAddress { PlaceType = "Land", Country = new OwnedCountry { Name = "USA", PlanetId = 1 } };
                personAddress3["AddressLine"] = "72 Hickory Rd.";
                personAddress3["ZipCode"] = 07728;
                var branchAddress3 = new OwnedAddress { PlaceType = "Land", Country = new OwnedCountry { Name = "Canada", PlanetId = 1 } };
                branchAddress3["BranchName"] = "BranchB";
                var leafAAddress3 = new OwnedAddress { PlaceType = "Land", Country = new OwnedCountry { Name = "Mexico", PlanetId = 1 } };
                leafAAddress3["LeafType"] = 1;
                var ownedPerson3 = new LeafA
                {
                    Id = 3,
                    PersonAddress = personAddress3,
                    BranchAddress = branchAddress3,
                    LeafAAddress = leafAAddress3
                };
                ownedPerson3["Name"] = "Madalena Morana";

                var personAddress4 = new OwnedAddress { PlaceType = "Land", Country = new OwnedCountry { Name = "USA", PlanetId = 1 } };
                personAddress4["AddressLine"] = "28 Strawberry St.";
                personAddress4["ZipCode"] = 19053;
                var leafBAddress4 = new OwnedAddress { PlaceType = "Land", Country = new OwnedCountry { Name = "Panama", PlanetId = 1 } };
                leafBAddress4["LeafBType"] = "Green";
                var ownedPerson4 = new LeafB
                {
                    Id = 4,
                    PersonAddress = personAddress4,
                    LeafBAddress = leafBAddress4
                };
                ownedPerson4["Name"] = "Vanda Waldemar";

                var order1 = new Order { Id = -10, Client = ownedPerson1 };
                order1["OrderDate"] = Convert.ToDateTime("2018-07-11 10:01:41");
                var order2 = new Order { Id = -11, Client = ownedPerson1 };
                order2["OrderDate"] = Convert.ToDateTime("2015-03-03 04:37:59");
                ownedPerson1.Orders = new List<Order> { order1, order2 };

                var order3 = new Order { Id = -20, Client = ownedPerson2 };
                order3["OrderDate"] = Convert.ToDateTime("2015-05-25 20:35:48");
                ownedPerson2.Orders = new List<Order> { order3 };

                var order4 = new Order { Id = -30, Client = ownedPerson3 };
                order4["OrderDate"] = Convert.ToDateTime("2014-11-10 04:32:42");
                ownedPerson3.Orders = new List<Order> { order4 };

                var order5 = new Order { Id = -40, Client = ownedPerson4 };
                order5["OrderDate"] = Convert.ToDateTime("2016-04-25 19:23:56");
                ownedPerson4.Orders = new List<Order> { order5 };

                return new List<OwnedPerson>
                {
                    ownedPerson1,
                    ownedPerson2,
                    ownedPerson3,
                    ownedPerson4
                };
            }

            private static IReadOnlyList<Fink> CreateFinks()
                => new List<Fink> { new Fink { Id = 1 } };

            private static IReadOnlyList<Barton> CreateBartons()
                => new List<Barton>
                {
                    new Barton
                    {
                        Id = 1,
                        Simple = "Simple",
                        Throned = new Throned { Property = "Property" }
                    }
                };

            private static void WireUp(
                IReadOnlyList<OwnedPerson> ownedPeople,
                IReadOnlyList<Planet> planets,
                IReadOnlyList<Star> stars,
                IReadOnlyList<Moon> moons,
                IReadOnlyList<Fink> finks,
                IReadOnlyList<Barton> bartons)
            {
                ownedPeople[0].PersonAddress.Country.Planet = planets[0];

                var branch = (Branch)ownedPeople[1];
                branch.PersonAddress.Country.Planet = planets[0];
                branch.BranchAddress.Country.Planet = planets[0];

                var leafA = (LeafA)ownedPeople[2];
                leafA.PersonAddress.Country.Planet = planets[0];
                leafA.BranchAddress.Country.Planet = planets[0];
                leafA.LeafAAddress.Country.Planet = planets[0];

                var leafB = (LeafB)ownedPeople[3];
                leafB.PersonAddress.Country.Planet = planets[0];
                leafB.LeafBAddress.Country.Planet = planets[0];

                planets[0].Moons = new List<Moon> { moons[0] };
                planets[0].Star = stars[0];
                stars[0].Planets = new List<Planet> { planets[0] };

                finks[0].Barton = bartons[0];
            }
        }

        protected class OwnedAddress
        {
            private string _addressLine;
            private int _zipCode;
            private string _branchName;
            private int _leafAType;
            private string _leafBType;

            public string PlaceType { get; set; }
            public OwnedCountry Country { get; set; }
            public object this[string name]
            {
                get => name switch
                {
                    "AddressLine" => _addressLine,
                    "ZipCode" => _zipCode,
                    "BranchName" => _branchName,
                    "LeafType" => _leafAType,
                    "LeafBType" => _leafBType,
                    _ => throw new InvalidOperationException($"Indexer property with key {name} is not defined on {nameof(OwnedPerson)}."),
                };

                set
                {
                    switch (name)
                    {
                        case "AddressLine":
                            _addressLine = (string)value;
                            break;

                        case "ZipCode":
                            _zipCode = (int)value;
                            break;

                        case "BranchName":
                            _branchName = (string)value;
                            break;

                        case "LeafType":
                            _leafAType = (int)value;
                            break;

                        case "LeafBType":
                            _leafBType = (string)value;
                            break;

                        default:
                            throw new InvalidOperationException($"Indexer property with key {name} is not defined on {nameof(OwnedPerson)}.");
                    }
                }
            }
        }

        protected class OwnedCountry
        {
            public string Name { get; set; }

            public int PlanetId { get; set; }
            public Planet Planet { get; set; }
        }

        protected class OwnedPerson
        {
            private string _name;

            public int Id { get; set; }

            public object this[string name]
            {
                get
                {
                    if (string.Equals(name, "Name", StringComparison.Ordinal))
                    {
                        return _name;
                    }

                    throw new InvalidOperationException($"Indexer property with key {name} is not defined on {nameof(OwnedPerson)}.");
                }

                set
                {
                    if (string.Equals(name, "Name", StringComparison.Ordinal))
                    {
                        _name = (string)value;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Indexer property with key {name} is not defined on {nameof(OwnedPerson)}.");
                    }
                }
            }

            public OwnedAddress PersonAddress { get; set; }
            public int ReadOnlyProperty => 10;

            public ICollection<Order> Orders { get; set; }
        }

        protected class Order
        {
            private DateTime _orderDate;
            public int Id { get; set; }

            public object this[string name]
            {
                get
                {
                    if (string.Equals(name, "OrderDate", StringComparison.Ordinal))
                    {
                        return _orderDate;
                    }

                    throw new InvalidOperationException($"Indexer property with key {name} is not defined on {nameof(OwnedPerson)}.");
                }

                set
                {
                    if (string.Equals(name, "OrderDate", StringComparison.Ordinal))
                    {
                        _orderDate = (DateTime)value;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Indexer property with key {name} is not defined on {nameof(OwnedPerson)}.");
                    }
                }
            }

            public OwnedPerson Client { get; set; }
        }

        protected class Branch : OwnedPerson
        {
            public OwnedAddress BranchAddress { get; set; }
        }

        protected class LeafA : Branch
        {
            public OwnedAddress LeafAAddress { get; set; }
        }

        protected class LeafB : OwnedPerson
        {
            public OwnedAddress LeafBAddress { get; set; }
        }

        protected class Planet
        {
            public int Id { get; set; }

            public int StarId { get; set; }
            public Star Star { get; set; }

            public List<Moon> Moons { get; set; }
        }

        protected class Moon
        {
            public int Id { get; set; }
            public int Diameter { get; set; }

            public int PlanetId { get; set; }
        }

        protected class Star
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public List<Element> Composition { get; set; }

            public List<Planet> Planets { get; set; }
        }

        protected class Element
        {
            public string Id { get; set; }
            public string Name { get; set; }

            public int StarId { get; set; }
        }

        protected class Barton
        {
            public int Id { get; set; }

            public Throned Throned { get; set; }

            public string Simple { get; set; }
        }

        protected class Fink
        {
            public Barton Barton { get; set; }

            public int Id { get; set; }
        }

        protected class Throned
        {
            public string Property { get; set; }
        }
    }
}
