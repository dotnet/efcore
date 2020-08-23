// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class ManyToManyQueryRelationalTestBase<TFixture> : ManyToManyQueryTestBase<TFixture>
        where TFixture : ManyToManyQueryFixtureBase, new()
    {
        protected ManyToManyQueryRelationalTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        protected virtual bool CanExecuteQueryString
            => false;

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_skip_navigation_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityCompositeKey>().Include(e => e.RootSkipShared).AsSplitQuery(),
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<EntityCompositeKey>(et => et.RootSkipShared)),
                entryCount: 76);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_skip_navigation_then_reference_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityTwo>().Include(e => e.OneSkip).ThenInclude(e => e.Reference).AsSplitQuery(),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<EntityTwo>(et => et.OneSkip),
                    new ExpectedInclude<EntityOne>(et => et.Reference, "OneSkip")),
                entryCount: 151);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_skip_navigation_then_include_skip_navigation_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityCompositeKey>().Include(e => e.LeafSkipFull).ThenInclude(e => e.OneSkip).AsSplitQuery(),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<EntityCompositeKey>(et => et.LeafSkipFull),
                    new ExpectedInclude<EntityLeaf>(et => et.OneSkip, "LeafSkipFull")),
                entryCount: 83);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_skip_navigation_then_include_reference_and_skip_navigation_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityThree>().Include(e => e.OneSkipPayloadFull).ThenInclude(e => e.Reference)
                    .Include(e => e.OneSkipPayloadFull).ThenInclude(e => e.SelfSkipPayloadRight).AsSplitQuery(),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<EntityThree>(et => et.OneSkipPayloadFull),
                    new ExpectedInclude<EntityOne>(et => et.Reference, "OneSkipPayloadFull"),
                    new ExpectedInclude<EntityOne>(et => et.SelfSkipPayloadRight, "OneSkipPayloadFull")),
                entryCount: 192);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_skip_navigation_and_reference_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityTwo>().Include(e => e.OneSkipShared).Include(e => e.Reference).AsSplitQuery(),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<EntityTwo>(et => et.OneSkipShared),
                    new ExpectedInclude<EntityTwo>(et => et.Reference)),
                entryCount: 93);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filtered_include_skip_navigation_where_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityThree>().Include(e => e.OneSkipPayloadFullShared.Where(i => i.Id < 10)).AsSplitQuery(),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedFilteredInclude<EntityThree, EntityOne>(
                        et => et.OneSkipPayloadFullShared, includeFilter: x => x.Where(i => i.Id < 10))),
                entryCount: 42);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filtered_include_skip_navigation_order_by_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityThree>().Include(e => e.TwoSkipFull.OrderBy(i => i.Id)).AsSplitQuery(),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedFilteredInclude<EntityThree, EntityTwo>(
                        et => et.TwoSkipFull, includeFilter: x => x.OrderBy(i => i.Id))),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filtered_include_skip_navigation_order_by_skip_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityTwo>().Include(e => e.SelfSkipSharedRight.OrderBy(i => i.Id).Skip(2)).AsSplitQuery(),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedFilteredInclude<EntityTwo, EntityTwo>(
                        et => et.SelfSkipSharedRight, includeFilter: x => x.OrderBy(i => i.Id).Skip(2))),
                entryCount: 31);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filtered_include_skip_navigation_order_by_take_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityCompositeKey>().Include(e => e.TwoSkipShared.OrderBy(i => i.Id).Take(2)).AsSplitQuery(),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedFilteredInclude<EntityCompositeKey, EntityTwo>(
                        et => et.TwoSkipShared, includeFilter: x => x.OrderBy(i => i.Id).Take(2))),
                entryCount: 63);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filtered_include_skip_navigation_order_by_skip_take_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityCompositeKey>().Include(e => e.ThreeSkipFull.OrderBy(i => i.Id).Skip(1).Take(2)).AsSplitQuery(),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedFilteredInclude<EntityCompositeKey, EntityThree>(
                        et => et.ThreeSkipFull, includeFilter: x => x.OrderBy(i => i.Id).Skip(1).Take(2))),
                entryCount: 57);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filtered_then_include_skip_navigation_where_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityRoot>().Include(e => e.ThreeSkipShared)
                    .ThenInclude(e => e.OneSkipPayloadFullShared.Where(i => i.Id < 10))
                    .AsSplitQuery(),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<EntityRoot>(et => et.ThreeSkipShared),
                    new ExpectedFilteredInclude<EntityThree, EntityOne>(
                        et => et.OneSkipPayloadFullShared, "ThreeSkipShared", includeFilter: x => x.Where(i => i.Id < 10))),
                entryCount: 78);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filtered_then_include_skip_navigation_order_by_skip_take_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityRoot>().Include(e => e.CompositeKeySkipShared)
                    .ThenInclude(e => e.ThreeSkipFull.OrderBy(i => i.Id).Skip(1).Take(2))
                    .AsSplitQuery(),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<EntityRoot>(et => et.CompositeKeySkipShared),
                    new ExpectedFilteredInclude<EntityCompositeKey, EntityThree>(
                        et => et.ThreeSkipFull, "CompositeKeySkipShared", includeFilter: x => x.OrderBy(i => i.Id).Skip(1).Take(2))),
                entryCount: 104);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filtered_include_skip_navigation_where_then_include_skip_navigation_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityLeaf>().Include(e => e.CompositeKeySkipFull.Where(i => i.Key1 < 5))
                    .ThenInclude(e => e.TwoSkipShared).AsSplitQuery(),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedFilteredInclude<EntityLeaf, EntityCompositeKey>(
                        et => et.CompositeKeySkipFull, includeFilter: x => x.Where(i => i.Key1 < 5)),
                    new ExpectedInclude<EntityCompositeKey>(et => et.TwoSkipShared, "CompositeKeySkipFull")),
                entryCount: 44);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityOne>().Include(e => e.TwoSkip.OrderBy(i => i.Id).Skip(1).Take(2))
                    .ThenInclude(e => e.ThreeSkipFull.Where(i => i.Id < 10))
                    .AsSplitQuery(),
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
        public virtual Task Filtered_include_skip_navigation_where_then_include_skip_navigation_order_by_skip_take_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityOne>().Include(e => e.TwoSkip.Where(i => i.Id < 10))
                    .ThenInclude(e => e.ThreeSkipFull.OrderBy(i => i.Id).Skip(1).Take(2))
                    .AsSplitQuery(),
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
        public virtual Task Filter_include_on_skip_navigation_combined_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityTwo>().Include(e => e.OneSkip.Where(i => i.Id < 10)).ThenInclude(e => e.Reference)
                    .Include(e => e.OneSkip).ThenInclude(e => e.Collection)
                    .AsSplitQuery(),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedFilteredInclude<EntityTwo, EntityOne>(et => et.OneSkip, includeFilter: x => x.Where(i => i.Id < 10)),
                    new ExpectedInclude<EntityOne>(et => et.Reference, "OneSkip"),
                    new ExpectedInclude<EntityOne>(et => et.Collection, "OneSkip")),
                entryCount: 88);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filter_include_on_skip_navigation_combined_with_filtered_then_includes_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityThree>()
                    .Include(e => e.OneSkipPayloadFull.Where(i => i.Id < 10))
                    .ThenInclude(e => e.TwoSkip.OrderBy(e => e.Id).Skip(1).Take(2))
                    .Include(e => e.OneSkipPayloadFull)
                    .ThenInclude(e => e.BranchSkip.Where(e => e.Id < 20))
                    .AsSplitQuery(),
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
        public virtual Task Filtered_include_on_skip_navigation_then_filtered_include_on_navigation_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityThree>().Include(e => e.OneSkipPayloadFull.Where(i => i.Id > 15))
                    .ThenInclude(e => e.Collection.Where(i => i.Id < 5))
                    .AsSplitQuery(),
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
        public virtual Task Filtered_include_on_navigation_then_filtered_include_on_skip_navigation_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<EntityOne>().Include(e => e.Collection.Where(i => i.Id > 15))
                    .ThenInclude(e => e.ThreeSkipFull.Where(i => i.Id < 5))
                    .AsSplitQuery(),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedFilteredInclude<EntityOne, EntityTwo>(et => et.Collection, includeFilter: x => x.Where(i => i.Id > 15)),
                    new ExpectedFilteredInclude<EntityTwo, EntityThree>(
                        et => et.ThreeSkipFull, "Collection", includeFilter: x => x.Where(i => i.Id < 5))),
                entryCount: 29);
        }

        protected override QueryAsserter CreateQueryAsserter(TFixture fixture)
            => new RelationalQueryAsserter(
                fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression, IgnoreEntryCount, CanExecuteQueryString);
    }
}
