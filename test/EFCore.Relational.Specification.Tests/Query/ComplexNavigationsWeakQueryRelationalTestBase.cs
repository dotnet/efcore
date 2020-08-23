// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class ComplexNavigationsWeakQueryRelationalTestBase<TFixture> : ComplexNavigationsWeakQueryTestBase<TFixture>
        where TFixture : ComplexNavigationsWeakQueryRelationalFixtureBase, new()
    {
        protected ComplexNavigationsWeakQueryRelationalTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filtered_include_basic_Where_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().Include(l1 => l1.OneToMany_Optional1.Where(l2 => l2.Id > 5)).AsSplitQuery(),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedFilteredInclude<Level1, Level2>(
                        e => e.OneToMany_Optional1,
                        includeFilter: x => x.Where(l2 => l2.Id > 5))));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filtered_include_OrderBy_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().Include(l1 => l1.OneToMany_Optional1.OrderBy(x => x.Name)).AsSplitQuery(),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedFilteredInclude<Level1, Level2>(
                        e => e.OneToMany_Optional1,
                        includeFilter: x => x.OrderBy(x => x.Name))));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filtered_ThenInclude_OrderBy_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>()
                    .Include(l1 => l1.OneToMany_Optional1)
                    .ThenInclude(l2 => l2.OneToMany_Optional2.OrderBy(x => x.Name))
                    .AsSplitQuery(),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<Level1>(e => e.OneToMany_Optional1),
                    new ExpectedFilteredInclude<Level2, Level3>(
                        e => e.OneToMany_Optional2,
                        "OneToMany_Optional1",
                        includeFilter: x => x.OrderBy(x => x.Name))));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filtered_include_ThenInclude_OrderBy_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>()
                    .Include(l1 => l1.OneToMany_Optional1.OrderBy(x => x.Name))
                    .ThenInclude(l2 => l2.OneToMany_Optional2.OrderByDescending(x => x.Name))
                    .AsSplitQuery(),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedFilteredInclude<Level1, Level2>(
                        e => e.OneToMany_Optional1,
                        includeFilter: x => x.OrderBy(x => x.Name)),
                    new ExpectedFilteredInclude<Level2, Level3>(
                        e => e.OneToMany_Optional2,
                        "OneToMany_Optional1",
                        includeFilter: x => x.OrderByDescending(x => x.Name))));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filtered_include_basic_OrderBy_Take_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().Include(l1 => l1.OneToMany_Optional1.OrderBy(x => x.Name).Take(3)).AsSplitQuery(),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedFilteredInclude<Level1, Level2>(
                        e => e.OneToMany_Optional1,
                        includeFilter: x => x.OrderBy(x => x.Name).Take(3))));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filtered_include_basic_OrderBy_Skip_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().Include(l1 => l1.OneToMany_Optional1.OrderBy(x => x.Name).Skip(1)).AsSplitQuery(),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedFilteredInclude<Level1, Level2>(
                        e => e.OneToMany_Optional1,
                        includeFilter: x => x.OrderBy(x => x.Name).Skip(1))));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filtered_include_basic_OrderBy_Skip_Take_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().Include(l1 => l1.OneToMany_Optional1.OrderBy(x => x.Name).Skip(1).Take(3)).AsSplitQuery(),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedFilteredInclude<Level1, Level2>(
                        e => e.OneToMany_Optional1,
                        includeFilter: x => x.OrderBy(x => x.Name).Skip(1).Take(3))));
        }

        [ConditionalFact]
        public virtual void Filtered_include_Skip_without_OrderBy_split()
        {
            using var ctx = CreateContext();
            var query = ctx.LevelOne.Include(l1 => l1.OneToMany_Optional1.Skip(1)).AsSplitQuery();
            var result = query.ToList();
        }

        [ConditionalFact]
        public virtual void Filtered_include_Take_without_OrderBy_split()
        {
            using var ctx = CreateContext();
            var query = ctx.LevelOne.Include(l1 => l1.OneToMany_Optional1.Take(1)).AsSplitQuery();
            var result = query.ToList();
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filtered_include_on_ThenInclude_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>()
                    .Include(l1 => l1.OneToOne_Optional_FK1)
                    .ThenInclude(l2 => l2.OneToMany_Optional2.Where(x => x.Name != "Foo").OrderBy(x => x.Name).Skip(1).Take(3))
                    .AsSplitQuery(),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<Level1>(e => e.OneToOne_Optional_FK1),
                    new ExpectedFilteredInclude<Level2, Level3>(
                        e => e.OneToMany_Optional2,
                        "OneToOne_Optional_FK1",
                        x => x.Where(x => x.Name != "Foo").OrderBy(x => x.Name).Skip(1).Take(3))));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filtered_include_after_reference_navigation_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>()
                    .Include(
                        l1 => l1.OneToOne_Optional_FK1.OneToMany_Optional2.Where(x => x.Name != "Foo").OrderBy(x => x.Name).Skip(1).Take(3))
                    .AsSplitQuery(),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<Level1>(e => e.OneToOne_Optional_FK1),
                    new ExpectedFilteredInclude<Level2, Level3>(
                        e => e.OneToMany_Optional2,
                        "OneToOne_Optional_FK1",
                        x => x.Where(x => x.Name != "Foo").OrderBy(x => x.Name).Skip(1).Take(3))));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filtered_include_after_different_filtered_include_same_level_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>()
                    .Include(l1 => l1.OneToMany_Optional1.Where(x => x.Name != "Foo").OrderBy(x => x.Name).Take(3))
                    .Include(l1 => l1.OneToMany_Required1.Where(x => x.Name != "Bar").OrderByDescending(x => x.Name).Skip(1))
                    .AsSplitQuery(),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedFilteredInclude<Level1, Level2>(
                        e => e.OneToMany_Optional1,
                        includeFilter: x => x.Where(x => x.Name != "Foo").OrderBy(x => x.Name).Take(3)),
                    new ExpectedFilteredInclude<Level1, Level2>(
                        e => e.OneToMany_Required1,
                        includeFilter: x => x.Where(x => x.Name != "Bar").OrderByDescending(x => x.Name).Skip(1))));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filtered_include_after_different_filtered_include_different_level_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>()
                    .Include(l1 => l1.OneToMany_Optional1.Where(x => x.Name != "Foo").OrderBy(x => x.Name).Take(3))
                    .ThenInclude(l2 => l2.OneToMany_Required2.Where(x => x.Name != "Bar").OrderByDescending(x => x.Name).Skip(1))
                    .AsSplitQuery(),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedFilteredInclude<Level1, Level2>(
                        e => e.OneToMany_Optional1,
                        includeFilter: x => x.Where(x => x.Name != "Foo").OrderBy(x => x.Name).Take(3)),
                    new ExpectedFilteredInclude<Level2, Level3>(
                        e => e.OneToMany_Required2,
                        "OneToMany_Optional1",
                        includeFilter: x => x.Where(x => x.Name != "Bar").OrderByDescending(x => x.Name).Skip(1))));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Filtered_include_different_filter_set_on_same_navigation_twice_split(bool async)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(
                    async,
                    ss => ss.Set<Level1>()
                        .Include(l1 => l1.OneToMany_Optional1.Where(x => x.Name != "Foo").OrderBy(x => x.Id).Take(3))
                        .Include(l1 => l1.OneToMany_Optional1.Where(x => x.Name != "Bar").OrderByDescending(x => x.Name).Take(3))
                        .AsSplitQuery()))).Message;
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Filtered_include_different_filter_set_on_same_navigation_twice_multi_level_split(bool async)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(
                    async,
                    ss => ss.Set<Level1>()
                        .Include(l1 => l1.OneToMany_Optional1.Where(x => x.Name != "Foo")).ThenInclude(l2 => l2.OneToMany_Optional2)
                        .Include(l1 => l1.OneToMany_Optional1.Where(x => x.Name != "Bar")).ThenInclude(l2 => l2.OneToOne_Required_FK2)
                        .AsSplitQuery()))).Message;
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filtered_include_same_filter_set_on_same_navigation_twice_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>()
                    .Include(l1 => l1.OneToMany_Optional1.Where(x => x.Name != "Foo").OrderByDescending(x => x.Id).Take(2))
                    .Include(l1 => l1.OneToMany_Optional1.Where(x => x.Name != "Foo").OrderByDescending(x => x.Id).Take(2))
                    .AsSplitQuery(),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedFilteredInclude<Level1, Level2>(
                        e => e.OneToMany_Optional1,
                        includeFilter: x => x.Where(x => x.Name != "Foo").OrderByDescending(x => x.Id).Take(2))));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filtered_include_same_filter_set_on_same_navigation_twice_followed_by_ThenIncludes_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>()
                    .Include(l1 => l1.OneToMany_Optional1.Where(x => x.Name != "Foo").OrderBy(x => x.Id).Take(2))
                    .ThenInclude(l2 => l2.OneToMany_Optional2)
                    .Include(l1 => l1.OneToMany_Optional1.Where(x => x.Name != "Foo").OrderBy(x => x.Id).Take(2))
                    .ThenInclude(l2 => l2.OneToOne_Required_FK2)
                    .AsSplitQuery(),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedFilteredInclude<Level1, Level2>(
                        e => e.OneToMany_Optional1,
                        includeFilter: x => x.Where(x => x.Name != "Foo").OrderBy(x => x.Id).Take(2)),
                    new ExpectedInclude<Level2>(e => e.OneToMany_Optional2),
                    new ExpectedInclude<Level2>(e => e.OneToOne_Required_FK2)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task
            Filtered_include_multiple_multi_level_includes_with_first_level_using_filter_include_on_one_of_the_chains_only_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>()
                    .Include(l1 => l1.OneToMany_Optional1.Where(x => x.Name != "Foo").OrderBy(x => x.Id).Take(2))
                    .ThenInclude(l2 => l2.OneToMany_Optional2)
                    .Include(l1 => l1.OneToMany_Optional1)
                    .ThenInclude(l2 => l2.OneToOne_Required_FK2)
                    .AsSplitQuery(),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedFilteredInclude<Level1, Level2>(
                        e => e.OneToMany_Optional1,
                        includeFilter: x => x.Where(x => x.Name != "Foo").OrderBy(x => x.Id).Take(2)),
                    new ExpectedInclude<Level2>(e => e.OneToMany_Optional2, "OneToMany_Optional1"),
                    new ExpectedInclude<Level2>(e => e.OneToOne_Required_FK2, "OneToMany_Optional1")));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filtered_include_and_non_filtered_include_on_same_navigation1_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>()
                    .Include(l1 => l1.OneToMany_Optional1)
                    .Include(l1 => l1.OneToMany_Optional1.Where(x => x.Name != "Foo").OrderBy(x => x.Id).Take(3))
                    .AsSplitQuery(),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedFilteredInclude<Level1, Level2>(
                        e => e.OneToMany_Optional1,
                        includeFilter: x => x.Where(x => x.Name != "Foo").OrderBy(x => x.Id).Take(3))));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filtered_include_and_non_filtered_include_on_same_navigation2_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>()
                    .Include(l1 => l1.OneToMany_Optional1.Where(x => x.Name != "Foo").OrderBy(x => x.Id).Take(3))
                    .Include(l1 => l1.OneToMany_Optional1)
                    .AsSplitQuery(),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedFilteredInclude<Level1, Level2>(
                        e => e.OneToMany_Optional1,
                        includeFilter: x => x.Where(x => x.Name != "Foo").OrderBy(x => x.Id).Take(3))));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filtered_include_and_non_filtered_include_followed_by_then_include_on_same_navigation_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>()
                    .Include(l1 => l1.OneToMany_Optional1.Where(x => x.Name != "Foo").OrderBy(x => x.Id).Take(1))
                    .Include(l1 => l1.OneToMany_Optional1)
                    .ThenInclude(l2 => l2.OneToOne_Optional_PK2.OneToMany_Optional3.Where(x => x.Id > 1))
                    .AsSplitQuery(),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedFilteredInclude<Level1, Level2>(
                        e => e.OneToMany_Optional1,
                        includeFilter: x => x.Where(x => x.Name != "Foo").OrderBy(x => x.Id).Take(1)),
                    new ExpectedInclude<Level2>(e => e.OneToOne_Optional_PK2, "OneToMany_Optional1"),
                    new ExpectedFilteredInclude<Level3, Level4>(
                        e => e.OneToMany_Optional3,
                        "OneToMany_Optional1.OneToOne_Optional_PK2",
                        includeFilter: x => x.Where(x => x.Id > 1))));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filtered_include_complex_three_level_with_middle_having_filter1_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>()
                    .Include(l1 => l1.OneToMany_Optional1)
                    .ThenInclude(l2 => l2.OneToMany_Optional2.Where(x => x.Name != "Foo").OrderBy(x => x.Id).Take(1))
                    .ThenInclude(l3 => l3.OneToMany_Optional3)
                    .Include(l1 => l1.OneToMany_Optional1)
                    .ThenInclude(l2 => l2.OneToMany_Optional2.Where(x => x.Name != "Foo").OrderBy(x => x.Id).Take(1))
                    .ThenInclude(l3 => l3.OneToMany_Required3)
                    .AsSplitQuery(),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<Level1>(e => e.OneToMany_Optional1),
                    new ExpectedFilteredInclude<Level2, Level3>(
                        e => e.OneToMany_Optional2,
                        "OneToMany_Optional1",
                        includeFilter: x => x.Where(x => x.Name != "Foo").OrderBy(x => x.Id).Take(1)),
                    new ExpectedInclude<Level3>(e => e.OneToMany_Optional3, "OneToMany_Optional1.OneToMany_Optional2"),
                    new ExpectedInclude<Level3>(e => e.OneToMany_Required3, "OneToMany_Optional1.OneToMany_Optional2")));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filtered_include_complex_three_level_with_middle_having_filter2_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>()
                    .Include(l1 => l1.OneToMany_Optional1)
                    .ThenInclude(l2 => l2.OneToMany_Optional2.Where(x => x.Name != "Foo").OrderBy(x => x.Id).Take(1))
                    .ThenInclude(l3 => l3.OneToMany_Optional3)
                    .Include(l1 => l1.OneToMany_Optional1)
                    .ThenInclude(l2 => l2.OneToMany_Optional2)
                    .ThenInclude(l3 => l3.OneToMany_Required3)
                    .AsSplitQuery(),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<Level1>(e => e.OneToMany_Optional1),
                    new ExpectedFilteredInclude<Level2, Level3>(
                        e => e.OneToMany_Optional2,
                        "OneToMany_Optional1",
                        includeFilter: x => x.Where(x => x.Name != "Foo").OrderBy(x => x.Id).Take(1)),
                    new ExpectedInclude<Level3>(e => e.OneToMany_Optional3, "OneToMany_Optional1.OneToMany_Optional2"),
                    new ExpectedInclude<Level3>(e => e.OneToMany_Required3, "OneToMany_Optional1.OneToMany_Optional2")));
        }

        [ConditionalFact]
        public virtual void Filtered_include_variable_used_inside_filter_split()
        {
            using var ctx = CreateContext();
            var prm = "Foo";
            var query = ctx.LevelOne
                .Include(l1 => l1.OneToMany_Optional1.Where(x => x.Name != prm).OrderBy(x => x.Id).Take(3)).AsSplitQuery();
            var result = query.ToList();
        }

        [ConditionalFact]
        public virtual void Filtered_include_context_accessed_inside_filter_split()
        {
            using var ctx = CreateContext();
            var query = ctx.LevelOne
                .Include(l1 => l1.OneToMany_Optional1.Where(x => ctx.LevelOne.Count() > 7).OrderBy(x => x.Id).Take(3)).AsSplitQuery();
            var result = query.ToList();
        }

        [ConditionalFact]
        public virtual void Filtered_include_context_accessed_inside_filter_correlated_split()
        {
            using var ctx = CreateContext();
            var query = ctx.LevelOne
                .Include(l1 => l1.OneToMany_Optional1.Where(x => ctx.LevelOne.Count(xx => xx.Id != x.Id) > 1).OrderBy(x => x.Id).Take(3))
                .AsSplitQuery();
            var result = query.ToList();
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Filtered_include_include_parameter_used_inside_filter_throws_split(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(
                    async,
                    ss => ss.Set<Level1>()
                        .Select(l1 => ss.Set<Level2>().Include(l2 => l2.OneToMany_Optional2.Where(x => x.Id != l2.Id))).AsSplitQuery()));
        }

        [ConditionalFact(Skip = "Issue#21234")]
        public virtual void Filtered_include_outer_parameter_used_inside_filter_split()
        {
            // TODO: needs #18191 for result verification
            using var ctx = CreateContext();
            var query = ctx.LevelOne.AsSplitQuery().Select(
                l1 => new
                {
                    l1.Id,
                    FullInclude = ctx.LevelTwo.Include(l2 => l2.OneToMany_Optional2).ToList(),
                    FilteredInclude = ctx.LevelTwo.Include(l2 => l2.OneToMany_Optional2.Where(x => x.Id != l1.Id)).ToList()
                });
            var result = query.ToList();
        }

        [ConditionalFact]
        public virtual void Filtered_include_is_considered_loaded_split()
        {
            using var ctx = CreateContext();
            var query = ctx.LevelOne.AsTracking().Include(l1 => l1.OneToMany_Optional1.OrderBy(x => x.Id).Take(1)).AsSplitQuery();
            var result = query.ToList();
            foreach (var resultElement in result)
            {
                var entry = ctx.Entry(resultElement);
                Assert.True(entry.Navigation("OneToMany_Optional1").IsLoaded);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Filtered_include_with_Distinct_throws_split(bool async)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(
                    async,
                    ss => ss.Set<Level1>().Include(l1 => l1.OneToMany_Optional1.Distinct()).AsSplitQuery()))).Message;
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Filtered_include_calling_methods_directly_on_parameter_throws_split(bool async)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(
                    async,
                    ss => ss.Set<Level1>()
                        .Include(l1 => l1.OneToMany_Optional1)
                        .ThenInclude(l2 => l2.AsQueryable().Where(xx => xx.Id != 42))
                        .AsSplitQuery()))).Message;
        }

        protected virtual bool CanExecuteQueryString
            => false;

        protected override QueryAsserter CreateQueryAsserter(TFixture fixture)
            => new RelationalQueryAsserter(
                fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression, canExecuteQueryString: CanExecuteQueryString);
    }
}
