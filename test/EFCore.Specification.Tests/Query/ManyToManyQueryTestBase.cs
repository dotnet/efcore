// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

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
        => AssertQuery(
            async,
            ss => ss.Set<EntityOne>().Where(e => e.TwoSkip.All(e => e.Name.Contains("B"))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_navigation_any_without_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityOne>().Where(e => e.ThreeSkipPayloadFull.Where(e => e.Name.Contains("B")).Any()),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_navigation_any_with_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityOne>().Where(e => e.TwoSkipShared.Any(e => e.Name.Contains("B"))),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_navigation_contains(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityOne>().Where(e => e.ThreeSkipPayloadFullShared.Contains(new EntityThree { Id = 1 })),
            ss => ss.Set<EntityOne>().Where(e => e.ThreeSkipPayloadFullShared.Select(i => i.Id).Contains(1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_navigation_count_without_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityOne>().Where(e => e.SelfSkipPayloadLeft.Count > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_navigation_count_with_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityOne>().OrderBy(e => e.BranchSkip.Count(e => e.Name.StartsWith("L")))
                .ThenBy(e => e.Id),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_navigation_long_count_without_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityTwo>().Where(e => e.ThreeSkipFull.LongCount() > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_navigation_long_count_with_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityTwo>().OrderByDescending(e => e.SelfSkipSharedLeft.LongCount(e => e.Name.StartsWith("L")))
                .ThenBy(e => e.Id),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_navigation_select_many_average(bool async)
        => AssertAverage(
            async,
            ss => ss.Set<EntityTwo>().SelectMany(e => e.CompositeKeySkipShared.Select(e => e.Key1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_navigation_select_many_max(bool async)
        => AssertMax(
            async,
            ss => ss.Set<EntityThree>().SelectMany(e => e.CompositeKeySkipFull.Select(e => e.Key1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_navigation_select_many_min(bool async)
        => AssertMin(
            async,
            ss => ss.Set<EntityThree>().SelectMany(e => e.RootSkipShared.Select(e => e.Id)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_navigation_select_many_sum(bool async)
        => AssertSum(
            async,
            ss => ss.Set<EntityRoot>().SelectMany(e => e.CompositeKeySkipShared.Select(e => e.Key1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_navigation_select_subquery_average(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<EntityLeaf>().Select(e => e.CompositeKeySkipFull.Average(e => e.Key1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_navigation_select_subquery_max(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<EntityTwo>().Select(e => e.OneSkip.Max(e => e.Id)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_navigation_select_subquery_min(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<EntityThree>().Select(e => e.OneSkipPayloadFull.Min(e => e.Id)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_navigation_select_subquery_sum(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<EntityTwo>().Select(e => e.OneSkipShared.Sum(e => e.Id)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_navigation_order_by_first_or_default(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityThree>().Select(e => e.OneSkipPayloadFullShared.OrderBy(i => i.Id).FirstOrDefault()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_navigation_order_by_single_or_default(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityOne>().Select(e => e.SelfSkipPayloadRight.OrderBy(i => i.Id).Take(1).SingleOrDefault()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_navigation_order_by_last_or_default(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityBranch>().Select(e => e.OneSkip.OrderBy(i => i.Id).LastOrDefault()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_navigation_order_by_reverse_first_or_default(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityThree>().Select(e => e.TwoSkipFull.OrderBy(i => i.Id).Reverse().FirstOrDefault()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_navigation_cast(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityCompositeKey>().OrderBy(e => e.Key1).Select(e => e.LeafSkipFull.Cast<EntityRoot>()),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_navigation_of_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityCompositeKey>().OrderBy(e => e.Key1).Select(e => e.RootSkipShared.OfType<EntityLeaf>()),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_with_skip_navigation(bool async)
        => AssertQuery(
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
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Left_join_with_skip_navigation(bool async)
        => AssertQuery(
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
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_many_over_skip_navigation(bool async)
        => AssertQuery(
            async,
            ss => from r in ss.Set<EntityRoot>()
                  from t in r.ThreeSkipShared
                  select t);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_many_over_skip_navigation_where(bool async)
        => AssertQuery(
            async,
            ss => from r in ss.Set<EntityOne>()
                  from t in r.TwoSkip.DefaultIfEmpty()
                  select t);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_many_over_skip_navigation_order_by_skip(bool async)
        => AssertQuery(
            async,
            ss => from r in ss.Set<EntityOne>()
                  from t in r.ThreeSkipPayloadFull.OrderBy(e => e.Id).Skip(2)
                  select t);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_many_over_skip_navigation_order_by_take(bool async)
        => AssertQuery(
            async,
            ss => from r in ss.Set<EntityOne>()
                  from t in r.TwoSkipShared.OrderBy(e => e.Id).Take(2)
                  select t);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_many_over_skip_navigation_order_by_skip_take(bool async)
        => AssertQuery(
            async,
            ss => from r in ss.Set<EntityOne>()
                  from t in r.ThreeSkipPayloadFullShared.OrderBy(e => e.Id).Skip(2).Take(3)
                  select t);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_many_over_skip_navigation_of_type(bool async)
        => AssertQuery(
            async,
            ss => from r in ss.Set<EntityThree>()
                  from t in r.RootSkipShared.OfType<EntityBranch>()
                  select t);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_many_over_skip_navigation_cast(bool async)
        => AssertQuery(
            async,
            ss => from r in ss.Set<EntityOne>()
                  from t in r.BranchSkip.Cast<EntityRoot>()
                  select t);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_skip_navigation(bool async)
        => AssertQuery(
            async,
            ss => from r in ss.Set<EntityOne>()
                  orderby r.Id
                  select r.SelfSkipPayloadLeft,
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_skip_navigation_multiple(bool async)
        => AssertQuery(
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
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_skip_navigation_first_or_default(bool async)
        => AssertQuery(
            async,
            ss => from r in ss.Set<EntityThree>()
                  orderby r.Id
                  select r.CompositeKeySkipFull.OrderBy(e => e.Key1).ThenBy(e => e.Key2).FirstOrDefault(),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_skip_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityCompositeKey>().Include(e => e.RootSkipShared),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<EntityCompositeKey>(et => et.RootSkipShared)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_skip_navigation_then_reference(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityTwo>().Include(e => e.OneSkip).ThenInclude(e => e.Reference),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<EntityTwo>(et => et.OneSkip),
                new ExpectedInclude<EntityOne>(et => et.Reference, "OneSkip")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_skip_navigation_then_include_skip_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityCompositeKey>().Include(e => e.LeafSkipFull).ThenInclude(e => e.OneSkip),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<EntityCompositeKey>(et => et.LeafSkipFull),
                new ExpectedInclude<EntityLeaf>(et => et.OneSkip, "LeafSkipFull")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_skip_navigation_then_include_reference_and_skip_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityThree>().Include(e => e.OneSkipPayloadFull).ThenInclude(e => e.Reference)
                .Include(e => e.OneSkipPayloadFull).ThenInclude(e => e.SelfSkipPayloadRight),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<EntityThree>(et => et.OneSkipPayloadFull),
                new ExpectedInclude<EntityOne>(et => et.Reference, "OneSkipPayloadFull"),
                new ExpectedInclude<EntityOne>(et => et.SelfSkipPayloadRight, "OneSkipPayloadFull")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_skip_navigation_and_reference(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityTwo>().Include(e => e.OneSkipShared).Include(e => e.Reference),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<EntityTwo>(et => et.OneSkipShared),
                new ExpectedInclude<EntityTwo>(et => et.Reference)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_skip_navigation_then_include_inverse_works_for_tracking_query(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityThree>().Include(e => e.OneSkipPayloadFullShared).ThenInclude(e => e.ThreeSkipPayloadFullShared),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<EntityThree>(et => et.OneSkipPayloadFullShared),
                new ExpectedInclude<EntityOne>(et => et.ThreeSkipPayloadFullShared, "OneSkipPayloadFullShared")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_skip_navigation_where(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityThree>().Include(e => e.OneSkipPayloadFullShared.Where(i => i.Id < 10)),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedFilteredInclude<EntityThree, EntityOne>(
                    et => et.OneSkipPayloadFullShared, includeFilter: x => x.Where(i => i.Id < 10))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_skip_navigation_order_by(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityThree>().Include(e => e.TwoSkipFull.OrderBy(i => i.Id)),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedFilteredInclude<EntityThree, EntityTwo>(
                    et => et.TwoSkipFull, includeFilter: x => x.OrderBy(i => i.Id))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_skip_navigation_order_by_skip(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityTwo>().Include(e => e.SelfSkipSharedRight.OrderBy(i => i.Id).Skip(2)),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedFilteredInclude<EntityTwo, EntityTwo>(
                    et => et.SelfSkipSharedRight, includeFilter: x => x.OrderBy(i => i.Id).Skip(2))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_skip_navigation_order_by_take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityCompositeKey>().Include(e => e.TwoSkipShared.OrderBy(i => i.Id).Take(2)),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedFilteredInclude<EntityCompositeKey, EntityTwo>(
                    et => et.TwoSkipShared, includeFilter: x => x.OrderBy(i => i.Id).Take(2))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_skip_navigation_order_by_take_EF_Property(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityCompositeKey>().Include(
                e => EF.Property<ICollection<EntityTwo>>(e, "TwoSkipShared").OrderBy(i => i.Id).Take(2)),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedFilteredInclude<EntityCompositeKey, EntityTwo>(
                    et => et.TwoSkipShared, includeFilter: x => x.OrderBy(i => i.Id).Take(2))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_skip_navigation_order_by_skip_take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityCompositeKey>().Include(e => e.ThreeSkipFull.OrderBy(i => i.Id).Skip(1).Take(2)),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedFilteredInclude<EntityCompositeKey, EntityThree>(
                    et => et.ThreeSkipFull, includeFilter: x => x.OrderBy(i => i.Id).Skip(1).Take(2))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_then_include_skip_navigation_where(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityRoot>().Include(e => e.ThreeSkipShared)
                .ThenInclude(e => e.OneSkipPayloadFullShared.Where(i => i.Id < 10)),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<EntityRoot>(et => et.ThreeSkipShared),
                new ExpectedFilteredInclude<EntityThree, EntityOne>(
                    et => et.OneSkipPayloadFullShared, "ThreeSkipShared", includeFilter: x => x.Where(i => i.Id < 10))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_then_include_skip_navigation_order_by_skip_take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityRoot>().Include(e => e.CompositeKeySkipShared)
                .ThenInclude(e => e.ThreeSkipFull.OrderBy(i => i.Id).Skip(1).Take(2)),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<EntityRoot>(et => et.CompositeKeySkipShared),
                new ExpectedFilteredInclude<EntityCompositeKey, EntityThree>(
                    et => et.ThreeSkipFull, "CompositeKeySkipShared", includeFilter: x => x.OrderBy(i => i.Id).Skip(1).Take(2))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_skip_navigation_where_then_include_skip_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityLeaf>().Include(e => e.CompositeKeySkipFull.Where(i => i.Key1 < 5)).ThenInclude(e => e.TwoSkipShared),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedFilteredInclude<EntityLeaf, EntityCompositeKey>(
                    et => et.CompositeKeySkipFull, includeFilter: x => x.Where(i => i.Key1 < 5)),
                new ExpectedInclude<EntityCompositeKey>(et => et.TwoSkipShared, "CompositeKeySkipFull")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityOne>().Include(e => e.TwoSkip.OrderBy(i => i.Id).Skip(1).Take(2))
                .ThenInclude(e => e.ThreeSkipFull.Where(i => i.Id < 10)),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedFilteredInclude<EntityOne, EntityTwo>(
                    et => et.TwoSkip, includeFilter: x => x.OrderBy(i => i.Id).Skip(1).Take(2)),
                new ExpectedFilteredInclude<EntityTwo, EntityThree>(
                    et => et.ThreeSkipFull, "TwoSkip", includeFilter: x => x.Where(i => i.Id < 10))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where_EF_Property(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityOne>().Include(e => EF.Property<ICollection<EntityTwo>>(e, "TwoSkip").OrderBy(i => i.Id).Skip(1).Take(2))
                .ThenInclude<EntityOne, EntityTwo, IEnumerable<EntityThree>>(
                    e => EF.Property<ICollection<EntityThree>>(e, "ThreeSkipFull").Where(i => i.Id < 10)),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedFilteredInclude<EntityOne, EntityTwo>(
                    et => et.TwoSkip, includeFilter: x => x.OrderBy(i => i.Id).Skip(1).Take(2)),
                new ExpectedFilteredInclude<EntityTwo, EntityThree>(
                    et => et.ThreeSkipFull, "TwoSkip", includeFilter: x => x.Where(i => i.Id < 10))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_skip_navigation_where_then_include_skip_navigation_order_by_skip_take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityOne>().Include(e => e.TwoSkip.Where(i => i.Id < 10))
                .ThenInclude(e => e.ThreeSkipFull.OrderBy(i => i.Id).Skip(1).Take(2)),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedFilteredInclude<EntityOne, EntityTwo>(
                    et => et.TwoSkip, includeFilter: x => x.Where(i => i.Id < 10)),
                new ExpectedFilteredInclude<EntityTwo, EntityThree>(
                    et => et.ThreeSkipFull, "TwoSkip", includeFilter: x => x.OrderBy(i => i.Id).Skip(1).Take(2))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_include_on_skip_navigation_combined(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityTwo>().Include(e => e.OneSkip.Where(i => i.Id < 10)).ThenInclude(e => e.Reference)
                .Include(e => e.OneSkip).ThenInclude(e => e.Collection),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedFilteredInclude<EntityTwo, EntityOne>(et => et.OneSkip, includeFilter: x => x.Where(i => i.Id < 10)),
                new ExpectedInclude<EntityOne>(et => et.Reference, "OneSkip"),
                new ExpectedInclude<EntityOne>(et => et.Collection, "OneSkip")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_include_on_skip_navigation_combined_with_filtered_then_includes(bool async)
        => AssertQuery(
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
                    et => et.BranchSkip, "OneSkipPayloadFull", includeFilter: x => x.Where(e => e.Id < 20))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Throws_when_different_filtered_include(bool async)
        => Assert.Equal(
            CoreStrings.MultipleFilteredIncludesOnSameNavigation(
                    "navigation    .Where(i => i.Id < 20)", "navigation    .Where(i => i.Id < 10)")
                .Replace("\r", "").Replace("\n", ""),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(
                    async,
                    ss => ss.Set<EntityTwo>().Include(e => e.OneSkip.Where(i => i.Id < 10)).ThenInclude(e => e.BranchSkip)
                        .Include(e => e.OneSkip.Where(i => i.Id < 20)).ThenInclude(e => e.ThreeSkipPayloadFull)))).Message
            .Replace("\r", "").Replace("\n", ""));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Throws_when_different_filtered_then_include(bool async)
        => Assert.Equal(
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_on_skip_navigation_then_filtered_include_on_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityThree>().Include(e => e.OneSkipPayloadFull.Where(i => i.Id > 15))
                .ThenInclude(e => e.Collection.Where(i => i.Id < 5)),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedFilteredInclude<EntityThree, EntityOne>(
                    et => et.OneSkipPayloadFull, includeFilter: x => x.Where(i => i.Id > 15)),
                new ExpectedFilteredInclude<EntityOne, EntityTwo>(
                    et => et.Collection, "OneSkipPayloadFull", includeFilter: x => x.Where(i => i.Id < 5))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_on_navigation_then_filtered_include_on_skip_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityOne>().Include(e => e.Collection.Where(i => i.Id > 15))
                .ThenInclude(e => e.ThreeSkipFull.Where(i => i.Id < 5)),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedFilteredInclude<EntityOne, EntityTwo>(et => et.Collection, includeFilter: x => x.Where(i => i.Id > 15)),
                new ExpectedFilteredInclude<EntityTwo, EntityThree>(
                    et => et.ThreeSkipFull, "Collection", includeFilter: x => x.Where(i => i.Id < 5))));

    [ConditionalTheory(Skip = "Issue#21332")]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Includes_accessed_via_different_path_are_merged(bool async)
        => AssertQuery(
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
                new ExpectedInclude<EntityThree>(e => e.CollectionInverse, "JoinThreePayloadFull.Three")));

    [ConditionalTheory(Skip = "Issue#21332")]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_includes_accessed_via_different_path_are_merged(bool async)
        => AssertQuery(
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
                new ExpectedInclude<EntityTwo>(e => e.Reference, "JoinOnePayloadFull.One.Collection")));

    [ConditionalTheory(Skip = "Issue#21332")]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Throws_when_different_filtered_then_include_via_different_paths(bool async)
        => Assert.Equal(
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_many_over_skip_navigation_where_non_equality(bool async)
        => AssertQuery(
            async,
            ss => from r in ss.Set<EntityOne>()
                  from t in r.TwoSkip.Where(x => x.Id != r.Id).DefaultIfEmpty()
                  select t);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_on_skip_collection_navigation(bool async)
    {
        var two = new EntityTwo { Id = 1 };

        return AssertQuery(
            async,
            ss => ss.Set<EntityOne>().Where(e => e.TwoSkip.Contains(two)),
            ss => ss.Set<EntityOne>().Where(e => e.TwoSkip.Select(i => i.Id).Contains(two.Id)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GetType_in_hierarchy_in_base_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityRoot>().Where(e => e.GetType() == typeof(EntityRoot)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GetType_in_hierarchy_in_intermediate_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityRoot>().Where(e => e.GetType() == typeof(EntityBranch)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GetType_in_hierarchy_in_leaf_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityRoot>().Where(e => e.GetType() == typeof(EntityLeaf)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GetType_in_hierarchy_in_querying_base_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityBranch>().Where(e => e.GetType() == typeof(EntityRoot)),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_navigation_all_unidirectional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<UnidirectionalEntityOne>().Where(e => e.TwoSkip.All(e => e.Name.Contains("B"))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_navigation_any_with_predicate_unidirectional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<UnidirectionalEntityOne>().Where(e => e.TwoSkipShared.Any(e => e.Name.Contains("B"))),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_navigation_contains_unidirectional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<UnidirectionalEntityOne>()
                .Where(e => e.ThreeSkipPayloadFullShared.Contains(new UnidirectionalEntityThree { Id = 1 })),
            ss => ss.Set<UnidirectionalEntityOne>().Where(e => e.ThreeSkipPayloadFullShared.Select(i => i.Id).Contains(1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_navigation_count_without_predicate_unidirectional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<UnidirectionalEntityOne>().Where(e => e.SelfSkipPayloadLeft.Count > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_navigation_count_with_predicate_unidirectional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<UnidirectionalEntityOne>().OrderBy(e => e.BranchSkip.Count(e => e.Name.StartsWith("L")))
                .ThenBy(e => e.Id),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_navigation_select_subquery_average_unidirectional(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<UnidirectionalEntityLeaf>().Select(e => e.CompositeKeySkipFull.Average(e => e.Key1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_navigation_order_by_reverse_first_or_default_unidirectional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<UnidirectionalEntityThree>().Select(e => e.TwoSkipFull.OrderBy(i => i.Id).Reverse().FirstOrDefault()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_navigation_of_type_unidirectional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<UnidirectionalEntityCompositeKey>().OrderBy(e => e.Key1)
                .Select(e => e.RootSkipShared.OfType<UnidirectionalEntityLeaf>()),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_with_skip_navigation_unidirectional(bool async)
        => AssertQuery(
            async,
            ss => from t in ss.Set<UnidirectionalEntityTwo>()
                  join s in ss.Set<UnidirectionalEntityTwo>()
                      on t.Id equals s.SelfSkipSharedRight.OrderBy(e => e.Id).FirstOrDefault().Id
                  select new { t, s },
            elementSorter: e => (e.t.Id, e.s.Id),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.t, a.t);
                AssertEqual(e.s, a.s);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Left_join_with_skip_navigation_unidirectional(bool async)
        => AssertQuery(
            async,
            ss => from t in ss.Set<UnidirectionalEntityCompositeKey>()
                  join s in ss.Set<UnidirectionalEntityCompositeKey>()
                      on t.TwoSkipShared.OrderBy(e => e.Id).FirstOrDefault().Id equals s.ThreeSkipFull.OrderBy(e => e.Id)
                          .FirstOrDefault().Id into grouping
                  from s in grouping.DefaultIfEmpty()
                  orderby t.Key1, s.Key1, t.Key2, s.Key2
                  select new { t, s },
            ss => from t in ss.Set<UnidirectionalEntityCompositeKey>()
                  join s in ss.Set<UnidirectionalEntityCompositeKey>()
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
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_many_over_skip_navigation_unidirectional(bool async)
        => AssertQuery(
            async,
            ss => from r in ss.Set<UnidirectionalEntityRoot>()
                  from t in r.ThreeSkipShared
                  select t);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_many_over_skip_navigation_where_unidirectional(bool async)
        => AssertQuery(
            async,
            ss => from r in ss.Set<UnidirectionalEntityOne>()
                  from t in r.TwoSkip.DefaultIfEmpty()
                  select t);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_many_over_skip_navigation_order_by_take_unidirectional(bool async)
        => AssertQuery(
            async,
            ss => from r in ss.Set<UnidirectionalEntityOne>()
                  from t in r.TwoSkipShared.OrderBy(e => e.Id).Take(2)
                  select t);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_many_over_skip_navigation_order_by_skip_take_unidirectional(bool async)
        => AssertQuery(
            async,
            ss => from r in ss.Set<UnidirectionalEntityOne>()
                  from t in r.ThreeSkipPayloadFullShared.OrderBy(e => e.Id).Skip(2).Take(3)
                  select t);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_many_over_skip_navigation_cast_unidirectional(bool async)
        => AssertQuery(
            async,
            ss => from r in ss.Set<UnidirectionalEntityOne>()
                  from t in r.BranchSkip.Cast<UnidirectionalEntityRoot>()
                  select t);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_skip_navigation_unidirectional(bool async)
        => AssertQuery(
            async,
            ss => from r in ss.Set<UnidirectionalEntityOne>()
                  orderby r.Id
                  select r.SelfSkipPayloadLeft,
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_skip_navigation_unidirectional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<UnidirectionalEntityCompositeKey>().Include(e => e.RootSkipShared),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<UnidirectionalEntityCompositeKey>(et => et.RootSkipShared)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_skip_navigation_then_reference_unidirectional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<UnidirectionalEntityTwo>().Include("UnidirectionalEntityOne1.Reference"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_skip_navigation_then_include_skip_navigation_unidirectional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<UnidirectionalEntityCompositeKey>().Include("UnidirectionalEntityLeaf.UnidirectionalEntityOne"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_skip_navigation_then_include_reference_and_skip_navigation_unidirectional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<UnidirectionalEntityThree>().Include("UnidirectionalEntityOne.Reference")
                .Include("UnidirectionalEntityOne.UnidirectionalEntityOne"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_skip_navigation_and_reference_unidirectional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<UnidirectionalEntityTwo>().Include("UnidirectionalEntityOne").Include(e => e.Reference));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_skip_navigation_then_include_inverse_works_for_tracking_query_unidirectional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<UnidirectionalEntityThree>().Include("UnidirectionalEntityOne1.ThreeSkipPayloadFullShared"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_skip_navigation_where_unidirectional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityThree>().Include(e => e.OneSkipPayloadFullShared.Where(i => i.Id < 10)),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedFilteredInclude<EntityThree, EntityOne>(
                    et => et.OneSkipPayloadFullShared, includeFilter: x => x.Where(i => i.Id < 10))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_skip_navigation_order_by_unidirectional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<UnidirectionalEntityThree>().Include(e => e.TwoSkipFull.OrderBy(i => i.Id)),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedFilteredInclude<UnidirectionalEntityThree, UnidirectionalEntityTwo>(
                    et => et.TwoSkipFull, includeFilter: x => x.OrderBy(i => i.Id))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_skip_navigation_order_by_skip_unidirectional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<UnidirectionalEntityTwo>().Include(e => e.SelfSkipSharedRight.OrderBy(i => i.Id).Skip(2)),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedFilteredInclude<UnidirectionalEntityTwo, UnidirectionalEntityTwo>(
                    et => et.SelfSkipSharedRight, includeFilter: x => x.OrderBy(i => i.Id).Skip(2))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_skip_navigation_order_by_take_unidirectional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<UnidirectionalEntityCompositeKey>().Include(e => e.TwoSkipShared.OrderBy(i => i.Id).Take(2)),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedFilteredInclude<UnidirectionalEntityCompositeKey, UnidirectionalEntityTwo>(
                    et => et.TwoSkipShared, includeFilter: x => x.OrderBy(i => i.Id).Take(2))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_skip_navigation_order_by_skip_take_unidirectional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<UnidirectionalEntityCompositeKey>().Include(e => e.ThreeSkipFull.OrderBy(i => i.Id).Skip(1).Take(2)),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedFilteredInclude<UnidirectionalEntityCompositeKey, UnidirectionalEntityThree>(
                    et => et.ThreeSkipFull, includeFilter: x => x.OrderBy(i => i.Id).Skip(1).Take(2))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_skip_navigation_where_then_include_skip_navigation_unidirectional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<UnidirectionalEntityLeaf>().Include(e => e.CompositeKeySkipFull.Where(i => i.Key1 < 5))
                .ThenInclude(e => e.TwoSkipShared),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedFilteredInclude<UnidirectionalEntityLeaf, UnidirectionalEntityCompositeKey>(
                    et => et.CompositeKeySkipFull, includeFilter: x => x.Where(i => i.Key1 < 5)),
                new ExpectedInclude<UnidirectionalEntityCompositeKey>(et => et.TwoSkipShared, "CompositeKeySkipFull")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_include_on_skip_navigation_combined_unidirectional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<EntityTwo>().Include(e => e.OneSkip.Where(i => i.Id < 10)).ThenInclude(e => e.Reference)
                .Include(e => e.OneSkip).ThenInclude(e => e.Collection));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Throws_when_different_filtered_include_unidirectional(bool async)
        => Assert.Equal(
            CoreStrings.MultipleFilteredIncludesOnSameNavigation(
                    "navigation    .Where(i => i.Id < 20)", "navigation    .Where(i => i.Id < 10)")
                .Replace("\r", "").Replace("\n", ""),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(
                    async,
                    ss => ss.Set<UnidirectionalEntityTwo>()
                        .Include(e => EF.Property<IEnumerable<UnidirectionalEntityOne>>(e, "UnidirectionalEntityOne").Where(i => i.Id < 10))
                        .ThenInclude(e => e.BranchSkip)
                        .Include(e => EF.Property<IEnumerable<UnidirectionalEntityOne>>(e, "UnidirectionalEntityOne").Where(i => i.Id < 20))
                        .ThenInclude<UnidirectionalEntityTwo, UnidirectionalEntityOne, ICollection<UnidirectionalEntityThree>>(
                            e => EF.Property<ICollection<UnidirectionalEntityThree>>(e, "UnidirectionalEntityThree"))))).Message
            .Replace("\r", "").Replace("\n", ""));

    [ConditionalTheory(Skip = "Issue#21332")]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Includes_accessed_via_different_path_are_merged_unidirectional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<UnidirectionalEntityOne>().Include("ThreeSkipPayloadFull.CollectionInverse")
                .Include(e => e.JoinThreePayloadFull).ThenInclude(e => e.Three).ThenInclude(e => e.ReferenceInverse));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_many_over_skip_navigation_where_non_equality_unidirectional(bool async)
        => AssertQuery(
            async,
            ss => from r in ss.Set<UnidirectionalEntityOne>()
                  from t in r.TwoSkip.Where(x => x.Id != r.Id).DefaultIfEmpty()
                  select t);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_on_skip_collection_navigation_unidirectional(bool async)
    {
        var two = new UnidirectionalEntityTwo { Id = 1 };

        return AssertQuery(
            async,
            ss => ss.Set<UnidirectionalEntityOne>().Where(e => e.TwoSkip.Contains(two)),
            ss => ss.Set<UnidirectionalEntityOne>().Where(e => e.TwoSkip.Select(i => i.Id).Contains(two.Id)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GetType_in_hierarchy_in_base_type_unidirectional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<UnidirectionalEntityRoot>().Where(e => e.GetType() == typeof(UnidirectionalEntityRoot)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GetType_in_hierarchy_in_intermediate_type_unidirectional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<UnidirectionalEntityRoot>().Where(e => e.GetType() == typeof(UnidirectionalEntityBranch)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GetType_in_hierarchy_in_leaf_type_unidirectional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<UnidirectionalEntityRoot>().Where(e => e.GetType() == typeof(UnidirectionalEntityLeaf)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GetType_in_hierarchy_in_querying_base_type_unidirectional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<UnidirectionalEntityBranch>().Where(e => e.GetType() == typeof(UnidirectionalEntityRoot)),
            assertEmpty: true);

    // When adding include test here always add a tracking version and a split version in relational layer.
    // Keep this line at the bottom for next dev writing tests to see.

    protected ManyToManyContext CreateContext()
        => Fixture.CreateContext();

    protected virtual void ClearLog()
    {
    }
}
