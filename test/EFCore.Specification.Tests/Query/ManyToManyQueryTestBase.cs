// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class ManyToManyQueryTestBase<TFixture> : QueryTestBase<TFixture>
        where TFixture : ManyToManyQueryFixtureBase, new()
    {
        protected ManyToManyQueryTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Skip_navigation_all(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityOne>().Where(e => e.TwoSkip.All(e => e.Name.Contains("B"))),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Skip_navigation_any_without_predicate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityOne>().Where(e => e.ThreeSkipPayloadFull.Where(e => e.Name.Contains("B")).Any()),
                entryCount: 0);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Skip_navigation_any_with_predicate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityOne>().Where(e => e.TwoSkipShared.Any(e => e.Name.Contains("B"))),
                entryCount: 0);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Skip_navigation_contains(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityOne>().Where(e => e.ThreeSkipPayloadFullShared.Contains(new EntityThree { Id = 1 })),
                ss => ss.Set<EntityOne>().Where(e => e.ThreeSkipPayloadFullShared.Select(i => i.Id).Contains(1)),
                entryCount: 3);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Skip_navigation_count_without_predicate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityOne>().Where(e => e.SelfSkipPayloadLeft.Count > 0),
                entryCount: 16);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Skip_navigation_count_with_predicate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityOne>().OrderBy(e => e.BranchSkip.Count(e => e.Name.StartsWith("L")))
                    .ThenBy(e => e.Id),
                assertOrder: true,
                entryCount: 20);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Skip_navigation_long_count_without_predicate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityTwo>().Where(e => e.ThreeSkipFull.LongCount() > 0),
                entryCount: 19);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Skip_navigation_long_count_with_predicate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityTwo>().OrderByDescending(e => e.SelfSkipSharedLeft.LongCount(e => e.Name.StartsWith("L")))
                    .ThenBy(e => e.Id),
                assertOrder: true,
                entryCount: 20);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Skip_navigation_select_many_average(bool async)
        {
            return AssertAverage(
                async,
                ss => ss.Set<EntityTwo>().SelectMany(e => e.CompositeKeySkipShared.Select(e => e.Key1)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Skip_navigation_select_many_max(bool async)
        {
            return AssertMax(
                async,
                ss => ss.Set<EntityThree>().SelectMany(e => e.CompositeKeySkipFull.Select(e => e.Key1)),
                entryCount: 0);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Skip_navigation_select_many_min(bool async)
        {
            return AssertMin(
                async,
                ss => ss.Set<EntityThree>().SelectMany(e => e.RootSkipShared.Select(e => e.Id)),
                entryCount: 0);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Skip_navigation_select_many_sum(bool async)
        {
            return AssertSum(
                async,
                ss => ss.Set<EntityRoot>().SelectMany(e => e.CompositeKeySkipShared.Select(e => e.Key1)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Skip_navigation_select_subquery_average(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<EntityLeaf>().Select(e => e.CompositeKeySkipFull.Average(e => e.Key1)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Skip_navigation_select_subquery_max(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<EntityTwo>().Select(e => e.OneSkip.Max(e => e.Id)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Skip_navigation_select_subquery_min(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<EntityThree>().Select(e => e.OneSkipPayloadFull.Min(e => e.Id)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Skip_navigation_select_subquery_sum(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<EntityTwo>().Select(e => e.OneSkipShared.Sum(e => e.Id)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Skip_navigation_order_by_first_or_default(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityThree>().Select(e => e.OneSkipPayloadFullShared.OrderBy(i => i.Id).FirstOrDefault()),
                entryCount: 12);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Skip_navigation_order_by_single_or_default(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityOne>().Select(e => e.SelfSkipPayloadRight.OrderBy(i => i.Id).Take(1).SingleOrDefault()),
                entryCount: 9);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Skip_navigation_order_by_last_or_default(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityBranch>().Select(e => e.OneSkip.OrderBy(i => i.Id).LastOrDefault()),
                entryCount: 6);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Skip_navigation_order_by_reverse_first_or_default(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityThree>().Select(e => e.TwoSkipFull.OrderBy(i => i.Id).Reverse().FirstOrDefault()),
                entryCount: 11);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Skip_navigation_cast(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityCompositeKey>().OrderBy(e => e.Key1).Select(e => e.LeafSkipFull.Cast<EntityRoot>()),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a),
                entryCount: 4);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Skip_navigation_of_type(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityCompositeKey>().OrderBy(e => e.Key1).Select(e => e.RootSkipShared.OfType<EntityLeaf>()),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a),
                entryCount: 3);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_with_skip_navigation(bool async)
        {
            return AssertQuery(
                async,
                ss => from t in ss.Set<EntityTwo>()
                      join s in ss.Set<EntityTwo>()
                          on t.Id equals s.SelfSkipSharedRight.OrderBy(e => e.Id).FirstOrDefault().Id
                      select new { t, s },
                elementSorter: e => (e.t.Id, e.s.Id),
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.t, a.t);
                    AssertEqual(e.s, a.s);
                },
                entryCount: 18);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Left_join_with_skip_navigation(bool async)
        {
            return AssertQuery(
                async,
                ss => from t in ss.Set<EntityCompositeKey>()
                      join s in ss.Set<EntityCompositeKey>()
                          on t.TwoSkipShared.OrderBy(e => e.Id).FirstOrDefault().Id equals s.ThreeSkipFull.OrderBy(e => e.Id)
                              .FirstOrDefault().Id into grouping
                      from s in grouping.DefaultIfEmpty()
                      orderby t.Key1, s.Key1, t.Key2, s.Key2
                      select new { t, s },
                ss => from t in ss.Set<EntityCompositeKey>()
                      join s in ss.Set<EntityCompositeKey>()
                          on t.TwoSkipShared.OrderBy(e => e.Id).FirstOrDefault().MaybeScalar(e => e.Id) equals s.ThreeSkipFull
                              .OrderBy(e => e.Id).FirstOrDefault().MaybeScalar(e => e.Id) into grouping
                      from s in grouping.DefaultIfEmpty()
                      orderby t.Key1, s.MaybeScalar(e => e.Key1), t.Key2, s.Key2
                      select new { t, s },
                assertOrder: true,
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.t, a.t);
                    AssertEqual(e.s, a.s);
                },
                entryCount: 20);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_many_over_skip_navigation(bool async)
        {
            return AssertQuery(
                async,
                ss => from r in ss.Set<EntityRoot>()
                      from t in r.ThreeSkipShared
                      select t,
                entryCount: 15);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_many_over_skip_navigation_where(bool async)
        {
            return AssertQuery(
                async,
                ss => from r in ss.Set<EntityOne>()
                      from t in r.TwoSkip.DefaultIfEmpty()
                      select t,
                entryCount: 20);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_many_over_skip_navigation_order_by_skip(bool async)
        {
            return AssertQuery(
                async,
                ss => from r in ss.Set<EntityOne>()
                      from t in r.ThreeSkipPayloadFull.OrderBy(e => e.Id).Skip(2)
                      select t,
                entryCount: 16);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_many_over_skip_navigation_order_by_take(bool async)
        {
            return AssertQuery(
                async,
                ss => from r in ss.Set<EntityOne>()
                      from t in r.TwoSkipShared.OrderBy(e => e.Id).Take(2)
                      select t,
                entryCount: 19);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_many_over_skip_navigation_order_by_skip_take(bool async)
        {
            return AssertQuery(
                async,
                ss => from r in ss.Set<EntityOne>()
                      from t in r.ThreeSkipPayloadFullShared.OrderBy(e => e.Id).Skip(2).Take(3)
                      select t,
                entryCount: 7);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_many_over_skip_navigation_of_type(bool async)
        {
            return AssertQuery(
                async,
                ss => from r in ss.Set<EntityThree>()
                      from t in r.RootSkipShared.OfType<EntityBranch>()
                      select t,
                entryCount: 9);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_many_over_skip_navigation_cast(bool async)
        {
            return AssertQuery(
                async,
                ss => from r in ss.Set<EntityOne>()
                      from t in r.BranchSkip.Cast<EntityRoot>()
                      select t,
                entryCount: 10);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_skip_navigation(bool async)
        {
            return AssertQuery(
                async,
                ss => from r in ss.Set<EntityOne>()
                      orderby r.Id
                      select r.SelfSkipPayloadLeft,
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a),
                entryCount: 13);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_skip_navigation_multiple(bool async)
        {
            return AssertQuery(
                async,
                ss => from r in ss.Set<EntityTwo>()
                      orderby r.Id
                      select new
                      {
                          r.ThreeSkipFull,
                          r.SelfSkipSharedLeft,
                          r.CompositeKeySkipShared
                      },
                assertOrder: true,
                elementAsserter: (e, a) =>
                {
                    AssertCollection(e.ThreeSkipFull, a.ThreeSkipFull);
                    AssertCollection(e.SelfSkipSharedLeft, a.SelfSkipSharedLeft);
                    AssertCollection(e.CompositeKeySkipShared, a.CompositeKeySkipShared);
                },
                entryCount: 50);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_skip_navigation_first_or_default(bool async)
        {
            return AssertQuery(
                async,
                ss => from r in ss.Set<EntityThree>()
                      orderby r.Id
                      select r.CompositeKeySkipFull.OrderBy(e => e.Key1).ThenBy(e => e.Key2).FirstOrDefault(),
                assertOrder: true,
                entryCount: 12);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_skip_navigation(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityCompositeKey>().Include(e => e.RootSkipShared),
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<EntityCompositeKey>(et => et.RootSkipShared)),
                entryCount: 76);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_skip_navigation_then_reference(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityTwo>().Include(e => e.OneSkip).ThenInclude(e => e.Reference),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<EntityTwo>(et => et.OneSkip),
                    new ExpectedInclude<EntityOne>(et => et.Reference, "OneSkip")),
                entryCount: 151);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_skip_navigation_then_include_skip_navigation(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityCompositeKey>().Include(e => e.LeafSkipFull).ThenInclude(e => e.OneSkip),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<EntityCompositeKey>(et => et.LeafSkipFull),
                    new ExpectedInclude<EntityLeaf>(et => et.OneSkip, "LeafSkipFull")),
                entryCount: 83);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_skip_navigation_then_include_reference_and_skip_navigation(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityThree>().Include(e => e.OneSkipPayloadFull).ThenInclude(e => e.Reference)
                    .Include(e => e.OneSkipPayloadFull).ThenInclude(e => e.SelfSkipPayloadRight),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<EntityThree>(et => et.OneSkipPayloadFull),
                    new ExpectedInclude<EntityOne>(et => et.Reference, "OneSkipPayloadFull"),
                    new ExpectedInclude<EntityOne>(et => et.SelfSkipPayloadRight, "OneSkipPayloadFull")),
                entryCount: 192);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_skip_navigation_and_reference(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityTwo>().Include(e => e.OneSkipShared).Include(e => e.Reference),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<EntityTwo>(et => et.OneSkipShared),
                    new ExpectedInclude<EntityTwo>(et => et.Reference)),
                entryCount: 93);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_skip_navigation_then_include_inverse_works_for_tracking_query(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityThree>().Include(e => e.OneSkipPayloadFullShared).ThenInclude(e => e.ThreeSkipPayloadFullShared),
                entryCount: 76,
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<EntityThree>(et => et.OneSkipPayloadFullShared),
                    new ExpectedInclude<EntityOne>(et => et.ThreeSkipPayloadFullShared, "OneSkipPayloadFullShared")));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filtered_include_skip_navigation_where(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityThree>().Include(e => e.OneSkipPayloadFullShared.Where(i => i.Id < 10)),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedFilteredInclude<EntityThree, EntityOne>(
                        et => et.OneSkipPayloadFullShared, includeFilter: x => x.Where(i => i.Id < 10))),
                entryCount: 42);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filtered_include_skip_navigation_order_by(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityThree>().Include(e => e.TwoSkipFull.OrderBy(i => i.Id)),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedFilteredInclude<EntityThree, EntityTwo>(
                        et => et.TwoSkipFull, includeFilter: x => x.OrderBy(i => i.Id))),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filtered_include_skip_navigation_order_by_skip(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityTwo>().Include(e => e.SelfSkipSharedRight.OrderBy(i => i.Id).Skip(2)),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedFilteredInclude<EntityTwo, EntityTwo>(
                        et => et.SelfSkipSharedRight, includeFilter: x => x.OrderBy(i => i.Id).Skip(2))),
                entryCount: 31);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filtered_include_skip_navigation_order_by_take(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityCompositeKey>().Include(e => e.TwoSkipShared.OrderBy(i => i.Id).Take(2)),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedFilteredInclude<EntityCompositeKey, EntityTwo>(
                        et => et.TwoSkipShared, includeFilter: x => x.OrderBy(i => i.Id).Take(2))),
                entryCount: 63);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filtered_include_skip_navigation_order_by_skip_take(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityCompositeKey>().Include(e => e.ThreeSkipFull.OrderBy(i => i.Id).Skip(1).Take(2)),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedFilteredInclude<EntityCompositeKey, EntityThree>(
                        et => et.ThreeSkipFull, includeFilter: x => x.OrderBy(i => i.Id).Skip(1).Take(2))),
                entryCount: 57);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filtered_then_include_skip_navigation_where(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityRoot>().Include(e => e.ThreeSkipShared)
                    .ThenInclude(e => e.OneSkipPayloadFullShared.Where(i => i.Id < 10)),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<EntityRoot>(et => et.ThreeSkipShared),
                    new ExpectedFilteredInclude<EntityThree, EntityOne>(
                        et => et.OneSkipPayloadFullShared, "ThreeSkipShared", includeFilter: x => x.Where(i => i.Id < 10))),
                entryCount: 78);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filtered_then_include_skip_navigation_order_by_skip_take(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityRoot>().Include(e => e.CompositeKeySkipShared)
                    .ThenInclude(e => e.ThreeSkipFull.OrderBy(i => i.Id).Skip(1).Take(2)),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<EntityRoot>(et => et.CompositeKeySkipShared),
                    new ExpectedFilteredInclude<EntityCompositeKey, EntityThree>(
                        et => et.ThreeSkipFull, "CompositeKeySkipShared", includeFilter: x => x.OrderBy(i => i.Id).Skip(1).Take(2))),
                entryCount: 104);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filtered_include_skip_navigation_where_then_include_skip_navigation(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityLeaf>().Include(e => e.CompositeKeySkipFull.Where(i => i.Key1 < 5)).ThenInclude(e => e.TwoSkipShared),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedFilteredInclude<EntityLeaf, EntityCompositeKey>(
                        et => et.CompositeKeySkipFull, includeFilter: x => x.Where(i => i.Key1 < 5)),
                    new ExpectedInclude<EntityCompositeKey>(et => et.TwoSkipShared, "CompositeKeySkipFull")),
                entryCount: 44);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityOne>().Include(e => e.TwoSkip.OrderBy(i => i.Id).Skip(1).Take(2))
                    .ThenInclude(e => e.ThreeSkipFull.Where(i => i.Id < 10)),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedFilteredInclude<EntityOne, EntityTwo>(
                        et => et.TwoSkip, includeFilter: x => x.OrderBy(i => i.Id).Skip(1).Take(2)),
                    new ExpectedFilteredInclude<EntityTwo, EntityThree>(
                        et => et.ThreeSkipFull, "TwoSkip", includeFilter: x => x.Where(i => i.Id < 10))),
                entryCount: 100);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filtered_include_skip_navigation_where_then_include_skip_navigation_order_by_skip_take(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityOne>().Include(e => e.TwoSkip.Where(i => i.Id < 10))
                    .ThenInclude(e => e.ThreeSkipFull.OrderBy(i => i.Id).Skip(1).Take(2)),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedFilteredInclude<EntityOne, EntityTwo>(
                        et => et.TwoSkip, includeFilter: x => x.Where(i => i.Id < 10)),
                    new ExpectedFilteredInclude<EntityTwo, EntityThree>(
                        et => et.ThreeSkipFull, "TwoSkip", includeFilter: x => x.OrderBy(i => i.Id).Skip(1).Take(2))),
                entryCount: 106);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filter_include_on_skip_navigation_combined(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityTwo>().Include(e => e.OneSkip.Where(i => i.Id < 10)).ThenInclude(e => e.Reference)
                    .Include(e => e.OneSkip).ThenInclude(e => e.Collection),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedFilteredInclude<EntityTwo, EntityOne>(et => et.OneSkip, includeFilter: x => x.Where(i => i.Id < 10)),
                    new ExpectedInclude<EntityOne>(et => et.Reference, "OneSkip"),
                    new ExpectedInclude<EntityOne>(et => et.Collection, "OneSkip")),
                entryCount: 88);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filter_include_on_skip_navigation_combined_with_filtered_then_includes(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityThree>().Include(e => e.OneSkipPayloadFull.Where(i => i.Id < 10))
                    .ThenInclude(e => e.TwoSkip.OrderBy(e => e.Id).Skip(1).Take(2))
                    .Include(e => e.OneSkipPayloadFull).ThenInclude(e => e.BranchSkip.Where(e => e.Id < 20)),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedFilteredInclude<EntityThree, EntityOne>(
                        et => et.OneSkipPayloadFull, includeFilter: x => x.Where(i => i.Id < 10)),
                    new ExpectedFilteredInclude<EntityOne, EntityTwo>(
                        et => et.TwoSkip, "OneSkipPayloadFull", includeFilter: x => x.OrderBy(e => e.Id).Skip(1).Take(2)),
                    new ExpectedFilteredInclude<EntityOne, EntityBranch>(
                        et => et.BranchSkip, "OneSkipPayloadFull", includeFilter: x => x.Where(e => e.Id < 20))),
                entryCount: 116);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Throws_when_different_filtered_include(bool async)
        {
            Assert.Equal(
                CoreStrings.MultipleFilteredIncludesOnSameNavigation(
                        "navigation    .Where(i => i.Id < 20)", "navigation    .Where(i => i.Id < 10)")
                    .Replace("\r", "").Replace("\n", ""),
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => AssertQuery(
                        async,
                        ss => ss.Set<EntityTwo>().Include(e => e.OneSkip.Where(i => i.Id < 10)).ThenInclude(e => e.BranchSkip)
                            .Include(e => e.OneSkip.Where(i => i.Id < 20)).ThenInclude(e => e.ThreeSkipPayloadFull)))).Message
                .Replace("\r", "").Replace("\n", ""));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Throws_when_different_filtered_then_include(bool async)
        {
            Assert.Equal(
                CoreStrings.MultipleFilteredIncludesOnSameNavigation(
                        "navigation    .Where(i => i.Id < 20)", "navigation    .Where(i => i.Id < 10)")
                    .Replace("\r", "").Replace("\n", ""),
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => AssertQuery(
                        async,
                        ss => ss.Set<EntityCompositeKey>()
                            .Include(e => e.TwoSkipShared)
                            .ThenInclude(e => e.OneSkip.Where(i => i.Id < 10)).ThenInclude(e => e.BranchSkip)
                            .Include(e => e.TwoSkipShared)
                            .ThenInclude(e => e.OneSkip.Where(i => i.Id < 20)).ThenInclude(e => e.ThreeSkipPayloadFull)))).Message
                .Replace("\r", "").Replace("\n", ""));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filtered_include_on_skip_navigation_then_filtered_include_on_navigation(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityThree>().Include(e => e.OneSkipPayloadFull.Where(i => i.Id > 15))
                    .ThenInclude(e => e.Collection.Where(i => i.Id < 5)),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedFilteredInclude<EntityThree, EntityOne>(
                        et => et.OneSkipPayloadFull, includeFilter: x => x.Where(i => i.Id > 15)),
                    new ExpectedFilteredInclude<EntityOne, EntityTwo>(
                        et => et.Collection, "OneSkipPayloadFull", includeFilter: x => x.Where(i => i.Id < 5))),
                entryCount: 61);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filtered_include_on_navigation_then_filtered_include_on_skip_navigation(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityOne>().Include(e => e.Collection.Where(i => i.Id > 15))
                    .ThenInclude(e => e.ThreeSkipFull.Where(i => i.Id < 5)),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedFilteredInclude<EntityOne, EntityTwo>(et => et.Collection, includeFilter: x => x.Where(i => i.Id > 15)),
                    new ExpectedFilteredInclude<EntityTwo, EntityThree>(
                        et => et.ThreeSkipFull, "Collection", includeFilter: x => x.Where(i => i.Id < 5))),
                entryCount: 29);
        }

        [ConditionalTheory(Skip = "Issue#21332")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Includes_accessed_via_different_path_are_merged(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityOne>().Include(e => e.ThreeSkipPayloadFull).ThenInclude(e => e.CollectionInverse)
                    .Include(e => e.JoinThreePayloadFull).ThenInclude(e => e.Three).ThenInclude(e => e.ReferenceInverse),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<EntityOne>(e => e.ThreeSkipPayloadFull),
                    new ExpectedInclude<EntityThree>(e => e.CollectionInverse, "ThreeSkipPayloadFull"),
                    new ExpectedInclude<EntityOne>(e => e.JoinThreePayloadFull),
                    new ExpectedInclude<JoinOneToThreePayloadFull>(e => e.Three, "JoinThreePayloadFull"),
                    new ExpectedInclude<EntityThree>(e => e.ReferenceInverse, "JoinThreePayloadFull.Three"),
                    new ExpectedInclude<EntityThree>(e => e.ReferenceInverse, "ThreeSkipPayloadFull"),
                    new ExpectedInclude<EntityThree>(e => e.CollectionInverse, "JoinThreePayloadFull.Three")),
                entryCount: 0);
        }

        [ConditionalTheory(Skip = "Issue#21332")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filered_includes_accessed_via_different_path_are_merged(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityThree>().Include(e => e.OneSkipPayloadFull).ThenInclude(e => e.Collection.Where(i => i.Id < 5))
                    .Include(e => e.JoinOnePayloadFull).ThenInclude(e => e.One).ThenInclude(e => e.Collection)
                    .ThenInclude(e => e.Reference),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<EntityThree>(e => e.OneSkipPayloadFull),
                    new ExpectedFilteredInclude<EntityOne, EntityTwo>(
                        e => e.Collection, "OneSkipPayloadFull", includeFilter: x => x.Where(i => i.Id < 5)),
                    new ExpectedInclude<EntityThree>(e => e.JoinOnePayloadFull),
                    new ExpectedInclude<JoinOneToThreePayloadFull>(e => e.One, "JoinOnePayloadFull"),
                    new ExpectedFilteredInclude<EntityOne, EntityTwo>(
                        e => e.Collection, "JoinOnePayloadFull.One", includeFilter: x => x.Where(i => i.Id < 5)),
                    new ExpectedInclude<EntityTwo>(e => e.Reference, "OneSkipPayloadFull.Collection"),
                    new ExpectedInclude<EntityTwo>(e => e.Reference, "JoinOnePayloadFull.One.Collection")),
                entryCount: 0);
        }

        [ConditionalTheory(Skip = "Issue#21332")]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Throws_when_different_filtered_then_include_via_different_paths(bool async)
        {
            Assert.Equal(
                CoreStrings.MultipleFilteredIncludesOnSameNavigation(
                        "navigation    .Where(i => i.Id < 20)", "navigation    .Where(i => i.Id < 10)")
                    .Replace("\r", "").Replace("\n", ""),
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => AssertQuery(
                        async,
                        ss => ss.Set<EntityThree>()
                            .Include(e => e.OneSkipPayloadFull)
                            .ThenInclude(e => e.Collection.Where(i => i.Id < 20))
                            .Include(e => e.JoinOnePayloadFull)
                            .ThenInclude(e => e.One)
                            .ThenInclude(e => e.Collection.Where(i => i.Id < 10))))).Message
                .Replace("\r", "").Replace("\n", ""));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_many_over_skip_navigation_where_non_equality(bool async)
        {
            return AssertQuery(
                async,
                ss => from r in ss.Set<EntityOne>()
                      from t in r.TwoSkip.Where(x => x.Id != r.Id).DefaultIfEmpty()
                      select t,
                entryCount: 20);
        }

        // When adding include test here always add a tracking version and a split version in relational layer.
        // Keep this line at the bottom for next dev writing tests to see.

        protected ManyToManyContext CreateContext()
            => Fixture.CreateContext();

        protected virtual void ClearLog()
        {
        }
    }
}
