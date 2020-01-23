// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable ConvertClosureToMethodGroup
// ReSharper disable PossibleUnintendedReferenceComparison
// ReSharper disable ArgumentsStyleLiteral
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable UnusedVariable
// ReSharper disable EqualExpressionComparison
// ReSharper disable AccessToDisposedClosure
// ReSharper disable StringStartsWithIsCultureSpecific
// ReSharper disable InconsistentNaming
// ReSharper disable MergeConditionalExpression
// ReSharper disable ReplaceWithSingleCallToSingle
// ReSharper disable ReturnValueOfPureMethodIsNotUsed
// ReSharper disable ConvertToExpressionBodyWhenPossible
#pragma warning disable RCS1155 // Use StringComparison when comparing strings.
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class ComplexNavigationsQueryTestBase<TFixture> : QueryTestBase<TFixture>
        where TFixture : ComplexNavigationsQueryFixtureBase, new()
    {
        protected ComplexNavigationsContext CreateContext()
        {
            return Fixture.CreateContext();
        }

        protected ComplexNavigationsQueryTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Entity_equality_empty(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().Where(l => l.OneToOne_Optional_FK1 == new Level2()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Key_equality_when_sentinel_ef_property(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().Where(l => EF.Property<int>(l.OneToOne_Optional_FK1, "Id") == 0),
                ss => ss.Set<Level1>().Where(l => MaybeScalar<int>(l.OneToOne_Optional_FK1, () => l.OneToOne_Optional_FK1.Id) == 0));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Key_equality_using_property_method_required(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().Where(l => EF.Property<int>(l.OneToOne_Required_FK1, "Id") > 7),
                ss => ss.Set<Level1>().Where(l => MaybeScalar<int>(l.OneToOne_Required_FK1, () => l.OneToOne_Required_FK1.Id) > 7));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Key_equality_using_property_method_required2(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level2>().Where(l => EF.Property<int>(l.OneToOne_Required_FK_Inverse2, "Id") > 7),
                ss => ss.Set<Level2>().Where(l => l.OneToOne_Required_FK_Inverse2.Id > 7));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Key_equality_using_property_method_nested(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().Where(l => EF.Property<int>(EF.Property<Level2>(l, "OneToOne_Required_FK1"), "Id") == 7),
                ss => ss.Set<Level1>().Where(l => MaybeScalar<int>(l.OneToOne_Required_FK1, () => l.OneToOne_Required_FK1.Id) == 7));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Key_equality_using_property_method_nested2(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level2>().Where(l => EF.Property<int>(EF.Property<Level1>(l, "OneToOne_Required_FK_Inverse2"), "Id") == 7),
                ss => ss.Set<Level2>().Where(l => l.OneToOne_Required_FK_Inverse2.Id == 7));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Key_equality_using_property_method_and_member_expression1(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().Where(l => EF.Property<Level2>(l, "OneToOne_Required_FK1").Id == 7),
                ss => ss.Set<Level1>().Where(l => MaybeScalar<int>(l.OneToOne_Required_FK1, () => l.OneToOne_Required_FK1.Id) == 7));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Key_equality_using_property_method_and_member_expression2(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().Where(l => EF.Property<int>(l.OneToOne_Required_FK1, "Id") == 7),
                ss => ss.Set<Level1>().Where(l => MaybeScalar<int>(l.OneToOne_Required_FK1, () => l.OneToOne_Required_FK1.Id) == 7));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Key_equality_using_property_method_and_member_expression3(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level2>().Where(l => EF.Property<int>(l.OneToOne_Required_FK_Inverse2, "Id") == 7),
                ss => ss.Set<Level2>().Where(l => l.OneToOne_Required_FK_Inverse2.Id == 7));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Key_equality_navigation_converted_to_FK(bool async)
        {
            // TODO: remove this? it is testing optimization that is no longer there
            return AssertQuery(
                async,
                ss => ss.Set<Level2>().Where(l => l.OneToOne_Required_FK_Inverse2 == new Level1 { Id = 1 }),
                ss => ss.Set<Level2>().Where(l => l.OneToOne_Required_FK_Inverse2.Id == 1));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Key_equality_two_conditions_on_same_navigation(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().Where(
                    l => l.OneToOne_Required_FK1 == new Level2 { Id = 1 }
                        || l.OneToOne_Required_FK1 == new Level2 { Id = 2 }),
                ss => ss.Set<Level1>().Where(
                    l => MaybeScalar<int>(l.OneToOne_Required_FK1, () => l.OneToOne_Required_FK1.Id) == 1
                        || MaybeScalar<int>(l.OneToOne_Required_FK1, () => l.OneToOne_Required_FK1.Id) == 2));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Key_equality_two_conditions_on_same_navigation2(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level2>().Where(
                    l => l.OneToOne_Required_FK_Inverse2 == new Level1 { Id = 1 }
                        || l.OneToOne_Required_FK_Inverse2 == new Level1 { Id = 2 }),
                ss => ss.Set<Level2>().Where(
                    l => l.OneToOne_Required_FK_Inverse2.Id == 1
                        || l.OneToOne_Required_FK_Inverse2.Id == 2));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multi_level_include_one_to_many_optional_and_one_to_many_optional_produces_valid_sql(bool async)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToMany_Optional1, "OneToMany_Optional1"),
                new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional2, "OneToMany_Optional2", navigationPath: "OneToMany_Optional1")
            };

            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>().Include(e => e.OneToMany_Optional1).ThenInclude(e => e.OneToMany_Optional2),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multi_level_include_correct_PK_is_chosen_as_the_join_predicate_for_queries_that_join_same_table_multiple_times(
            bool async)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToMany_Optional1, "OneToMany_Optional1"),
                new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional2, "OneToMany_Optional2", navigationPath: "OneToMany_Optional1"),
                new ExpectedInclude<Level3>(
                    l3 => l3.OneToMany_Required_Inverse3, "OneToMany_Required_Inverse3",
                    navigationPath: "OneToMany_Optional1.OneToMany_Optional2"),
                new ExpectedInclude<Level2>(
                    l2 => l2.OneToMany_Optional2, "OneToMany_Optional2",
                    navigationPath: "OneToMany_Optional1.OneToMany_Optional2.OneToMany_Required_Inverse3")
            };

            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>().Include(e => e.OneToMany_Optional1).ThenInclude(e => e.OneToMany_Optional2)
                    .ThenInclude(e => e.OneToMany_Required_Inverse3.OneToMany_Optional2),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void Multi_level_include_with_short_circuiting()
        {
            using var context = CreateContext();
            var query = context.Fields
                .Include(x => x.Label.Globalizations)
                .ThenInclude(x => x.Language)
                .Include(x => x.Placeholder.Globalizations)
                .ThenInclude(x => x.Language);

            var result = query.ToList().OrderBy(e => e.Name).ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal("Field1", result[0].Name);
            Assert.Equal("Field2", result[1].Name);

            Assert.Equal("MLS1", result[0].Label.DefaultText);
            Assert.Equal("MLS3", result[1].Label.DefaultText);
            Assert.Null(result[0].Placeholder);
            Assert.Equal("MLS4", result[1].Placeholder.DefaultText);

            var globalizations_0_label = result[0].Label.Globalizations.OrderBy(g => g.Text).ToList();
            Assert.Equal(3, globalizations_0_label.Count);
            Assert.Equal("Globalization0", globalizations_0_label[0].Text);
            Assert.Equal("Language0", globalizations_0_label[0].Language.Name);
            Assert.Equal("Globalization1", globalizations_0_label[1].Text);
            Assert.Equal("Language1", globalizations_0_label[1].Language.Name);
            Assert.Equal("Globalization2", globalizations_0_label[2].Text);
            Assert.Equal("Language2", globalizations_0_label[2].Language.Name);

            var globalizations_1_label = result[1].Label.Globalizations.OrderBy(g => g.Text).ToList();
            Assert.Equal(3, globalizations_1_label.Count);
            Assert.Equal("Globalization6", globalizations_1_label[0].Text);
            Assert.Equal("Language6", globalizations_1_label[0].Language.Name);
            Assert.Equal("Globalization7", globalizations_1_label[1].Text);
            Assert.Equal("Language7", globalizations_1_label[1].Language.Name);
            Assert.Equal("Globalization8", globalizations_1_label[2].Text);
            Assert.Equal("Language8", globalizations_1_label[2].Language.Name);

            var globalizations_1_placeholder = result[1].Placeholder.Globalizations.OrderBy(g => g.Text).ToList();
            Assert.Single(globalizations_1_placeholder);
            Assert.Equal("Globalization9", globalizations_1_placeholder[0].Text);
            Assert.Equal("Language9", globalizations_1_placeholder[0].Language.Name);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_navigation_key_access_optional(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from e1 in ss.Set<Level1>()
                    join e2 in ss.Set<Level2>() on e1.Id equals e2.OneToOne_Optional_FK_Inverse2.Id
                    select new { Id1 = e1.Id, Id2 = e2.Id },
                ss =>
                    from e1 in ss.Set<Level1>()
                    join e2 in ss.Set<Level2>() on e1.Id equals MaybeScalar<int>(
                        e2.OneToOne_Optional_FK_Inverse2,
                        () => e2.OneToOne_Optional_FK_Inverse2.Id)
                    select new { Id1 = e1.Id, Id2 = e2.Id },
                e => (e.Id1, e.Id2));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_navigation_key_access_required(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from e1 in ss.Set<Level1>()
                    join e2 in ss.Set<Level2>() on e1.Id equals e2.OneToOne_Required_FK_Inverse2.Id
                    select new { Id1 = e1.Id, Id2 = e2.Id },
                ss =>
                    from e1 in ss.Set<Level1>()
                    join e2 in ss.Set<Level2>() on e1.Id equals e2.OneToOne_Required_FK_Inverse2.Id
                    select new { Id1 = e1.Id, Id2 = e2.Id },
                e => (e.Id1, e.Id2));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_key_access_optional_comparison(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => from e2 in ss.Set<Level2>()
                      where e2.OneToOne_Optional_PK_Inverse2.Id > 5
                      select e2.Id,
                ss => from e2 in ss.Set<Level2>()
                      where MaybeScalar<int>(e2.OneToOne_Optional_PK_Inverse2, () => e2.OneToOne_Optional_PK_Inverse2.Id) > 5
                      select e2.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Simple_level1_include(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().Include(l1 => l1.OneToOne_Required_PK1));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Simple_level1(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Simple_level1_level2_include(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().Include(l1 => l1.OneToOne_Required_PK1.OneToOne_Required_PK2));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Simple_level1_level2_GroupBy_Count(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Level1>().GroupBy(
                        l1 => l1.OneToOne_Required_PK1.OneToOne_Required_PK2.Name)
                    .Select(g => g.Count()),
                ss => ss.Set<Level1>().GroupBy(
                        l1 => Maybe(
                            l1.OneToOne_Required_PK1,
                            () => Maybe(
                                l1.OneToOne_Required_PK1.OneToOne_Required_PK2,
                                () => l1.OneToOne_Required_PK1.OneToOne_Required_PK2.Name)))
                    .Select(g => g.Count()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Simple_level1_level2_GroupBy_Having_Count(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Level1>().GroupBy(
                        l1 => l1.OneToOne_Required_PK1.OneToOne_Required_PK2.Name,
                        l1 => new { Id = ((int?)l1.OneToOne_Required_PK1.Id ?? 0) })
                    .Where(g => g.Min(l1 => l1.Id) > 0)
                    .Select(g => g.Count()),
                ss => ss.Set<Level1>().GroupBy(
                        l1 => Maybe(
                            l1.OneToOne_Required_PK1,
                            () => Maybe(
                                l1.OneToOne_Required_PK1.OneToOne_Required_PK2,
                                () => l1.OneToOne_Required_PK1.OneToOne_Required_PK2.Name)),
                        l1 => new { Id = (MaybeScalar<int>(l1.OneToOne_Required_PK1, () => l1.OneToOne_Required_PK1.Id) ?? 0) })
                    .Where(g => g.Min(l1 => l1.Id) > 0)
                    .Select(g => g.Count()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Simple_level1_level2_level3_include(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().Include(l1 => l1.OneToOne_Required_PK1.OneToOne_Required_PK2.OneToOne_Required_PK3));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_key_access_required_comparison(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => from e2 in ss.Set<Level2>()
                      where e2.OneToOne_Required_PK_Inverse2.Id > 5
                      select e2.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_inside_method_call_translated_to_join(bool async)
        {
            return AssertQuery(
                async,
                ss => from e1 in ss.Set<Level1>()
                      where e1.OneToOne_Required_FK1.Name.StartsWith("L")
                      select e1,
                ss => from e1 in ss.Set<Level1>()
                      where MaybeScalar<bool>(e1.OneToOne_Required_FK1, () => e1.OneToOne_Required_FK1.Name.StartsWith("L")) == true
                      select e1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_inside_method_call_translated_to_join2(bool async)
        {
            return AssertQuery(
                async,
                ss => from e3 in ss.Set<Level3>()
                      where e3.OneToOne_Required_FK_Inverse3.Name.StartsWith("L")
                      select e3);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_inside_method_call_translated_to_join(bool async)
        {
            return AssertQuery(
                async,
                ss => from e1 in ss.Set<Level1>()
                      where e1.OneToOne_Optional_FK1.Name.StartsWith("L")
                      select e1,
                ss => from e1 in ss.Set<Level1>()
                      where MaybeScalar<bool>(e1.OneToOne_Optional_FK1, () => e1.OneToOne_Optional_FK1.Name.StartsWith("L")) == true
                      select e1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_inside_property_method_translated_to_join(bool async)
        {
            return AssertQuery(
                async,
                ss => from e1 in ss.Set<Level1>()
                      where EF.Property<string>(EF.Property<Level2>(e1, "OneToOne_Optional_FK1"), "Name") == "L2 01"
                      select e1,
                ss => from e1 in ss.Set<Level1>()
                      where Maybe(e1.OneToOne_Optional_FK1, () => e1.OneToOne_Optional_FK1.Name.ToUpper()) == "L2 01"
                      select e1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_inside_nested_method_call_translated_to_join(bool async)
        {
            return AssertQuery(
                async,
                ss => from e1 in ss.Set<Level1>()
                      where e1.OneToOne_Optional_FK1.Name.ToUpper().StartsWith("L")
                      select e1,
                ss => from e1 in ss.Set<Level1>()
                      where MaybeScalar<bool>(e1.OneToOne_Optional_FK1, () => e1.OneToOne_Optional_FK1.Name.ToUpper().StartsWith("L"))
                          == true
                      select e1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Method_call_on_optional_navigation_translates_to_null_conditional_properly_for_arguments(bool async)
        {
            return AssertQuery(
                async,
                ss => from e1 in ss.Set<Level1>()
                      where e1.OneToOne_Optional_FK1.Name.StartsWith(e1.OneToOne_Optional_FK1.Name)
                      select e1,
                ss => from e1 in ss.Set<Level1>()
                      where MaybeScalar<bool>(
                              e1.OneToOne_Optional_FK1, () => e1.OneToOne_Optional_FK1.Name.StartsWith(e1.OneToOne_Optional_FK1.Name))
                          == true
                      select e1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_inside_method_call_translated_to_join_keeps_original_nullability(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from e1 in ss.Set<Level1>()
                    where e1.OneToOne_Optional_FK1.Date.AddDays(10) > new DateTime(2000, 2, 1)
                    select e1,
                ss =>
                    from e1 in ss.Set<Level1>()
                    where MaybeScalar<DateTime>(e1.OneToOne_Optional_FK1, () => e1.OneToOne_Optional_FK1.Date.AddDays(10))
                        > new DateTime(2000, 2, 1)
                    select e1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_inside_nested_method_call_translated_to_join_keeps_original_nullability(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from e1 in ss.Set<Level1>()
                    where e1.OneToOne_Optional_FK1.Date.AddDays(10).AddDays(15).AddMonths(2) > new DateTime(2002, 2, 1)
                    select e1,
                ss =>
                    from e1 in ss.Set<Level1>()
                    where MaybeScalar<DateTime>(
                            e1.OneToOne_Optional_FK1, () => e1.OneToOne_Optional_FK1.Date.AddDays(10).AddDays(15).AddMonths(2))
                        > new DateTime(2000, 2, 1)
                    select e1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_inside_nested_method_call_translated_to_join_keeps_original_nullability_also_for_arguments(
            bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from e1 in ss.Set<Level1>()
                    where e1.OneToOne_Optional_FK1.Date.AddDays(15).AddDays(e1.OneToOne_Optional_FK1.Id) > new DateTime(2002, 2, 1)
                    select e1,
                ss =>
                    from e1 in ss.Set<Level1>()
                    where MaybeScalar<DateTime>(
                            e1.OneToOne_Optional_FK1,
                            () => e1.OneToOne_Optional_FK1.Date.AddDays(15).AddDays(e1.OneToOne_Optional_FK1.Id))
                        > new DateTime(2000, 2, 1)
                    select e1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_navigation_in_outer_selector_translated_to_extra_join(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from e1 in ss.Set<Level1>()
                    join e2 in ss.Set<Level2>() on e1.OneToOne_Optional_FK1.Id equals e2.Id
                    select new { Id1 = e1.Id, Id2 = e2.Id },
                ss =>
                    from e1 in ss.Set<Level1>()
                    join e2 in ss.Set<Level2>() on MaybeScalar<int>(
                        e1.OneToOne_Optional_FK1, () => e1.OneToOne_Optional_FK1.Id) equals e2.Id
                    select new { Id1 = e1.Id, Id2 = e2.Id },
                e => (e.Id1, e.Id2));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_navigation_in_outer_selector_translated_to_extra_join_nested(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from e1 in ss.Set<Level1>()
                    join e3 in ss.Set<Level3>() on e1.OneToOne_Required_FK1.OneToOne_Optional_FK2.Id equals e3.Id
                    select new { Id1 = e1.Id, Id3 = e3.Id },
                ss =>
                    from e1 in ss.Set<Level1>()
                    join e3 in ss.Set<Level3>() on MaybeScalar(
                        e1.OneToOne_Required_FK1,
                        () => MaybeScalar<int>(
                            e1.OneToOne_Required_FK1.OneToOne_Optional_FK2,
                            () => e1.OneToOne_Required_FK1.OneToOne_Optional_FK2.Id)) equals e3.Id
                    select new { Id1 = e1.Id, Id3 = e3.Id },
                e => (e.Id1, e.Id3));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_navigation_in_outer_selector_translated_to_extra_join_nested2(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from e3 in ss.Set<Level3>()
                    join e1 in ss.Set<Level1>() on e3.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK_Inverse2.Id equals e1.Id
                    select new { Id3 = e3.Id, Id1 = e1.Id },
                ss =>
                    from e3 in ss.Set<Level3>()
                    join e1 in ss.Set<Level1>() on MaybeScalar<int>(
                        e3.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK_Inverse2,
                        () => e3.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK_Inverse2.Id) equals e1.Id
                    select new { Id3 = e3.Id, Id1 = e1.Id },
                e => (e.Id1, e.Id3));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_navigation_in_inner_selector(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from e2 in ss.Set<Level2>()
                    join e1 in ss.Set<Level1>() on e2.Id equals e1.OneToOne_Optional_FK1.Id
                    select new { Id2 = e2.Id, Id1 = e1.Id },
                ss =>
                    from e2 in ss.Set<Level2>()
                    join e1 in ss.Set<Level1>() on e2.Id equals MaybeScalar<int>(
                        e1.OneToOne_Optional_FK1, () => e1.OneToOne_Optional_FK1.Id)
                    select new { Id2 = e2.Id, Id1 = e1.Id },
                e => (e.Id2, e.Id1));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_navigations_in_inner_selector_translated_without_collision(bool async)
        {
            return AssertQuery(
                async,
                ss => from e2 in ss.Set<Level2>()
                      join e1 in ss.Set<Level1>() on e2.Id equals e1.OneToOne_Optional_FK1.Id
                      join e3 in ss.Set<Level3>() on e2.Id equals e3.OneToOne_Optional_FK_Inverse3.Id
                      select new
                      {
                          Id2 = e2.Id,
                          Id1 = e1.Id,
                          Id3 = e3.Id
                      },
                ss => from e2 in ss.Set<Level2>()
                      join e1 in ss.Set<Level1>() on e2.Id equals MaybeScalar<int>(
                          e1.OneToOne_Optional_FK1, () => e1.OneToOne_Optional_FK1.Id)
                      join e3 in ss.Set<Level3>() on e2.Id equals MaybeScalar<int>(
                          e3.OneToOne_Optional_FK_Inverse3, () => e3.OneToOne_Optional_FK_Inverse3.Id)
                      select new
                      {
                          Id2 = e2.Id,
                          Id1 = e1.Id,
                          Id3 = e3.Id
                      },
                e => (e.Id2, e.Id1, e.Id3));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_navigation_non_key_join(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from e2 in ss.Set<Level2>()
                    join e1 in ss.Set<Level1>() on e2.Name equals e1.OneToOne_Optional_FK1.Name
                    select new
                    {
                        Id2 = e2.Id,
                        Name2 = e2.Name,
                        Id1 = e1.Id,
                        Name1 = e1.Name
                    },
                ss =>
                    from e2 in ss.Set<Level2>()
                    join e1 in ss.Set<Level1>() on e2.Name equals Maybe(e1.OneToOne_Optional_FK1, () => e1.OneToOne_Optional_FK1.Name)
                    select new
                    {
                        Id2 = e2.Id,
                        Name2 = e2.Name,
                        Id1 = e1.Id,
                        Name1 = e1.Name
                    },
                e => (e.Id2, e.Name2, e.Id1, e.Name1));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_with_orderby_on_inner_sequence_navigation_non_key_join(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from e2 in ss.Set<Level2>()
                    join e1 in ss.Set<Level1>().OrderBy(l1 => l1.Id) on e2.Name equals e1.OneToOne_Optional_FK1.Name
                    select new
                    {
                        Id2 = e2.Id,
                        Name2 = e2.Name,
                        Id1 = e1.Id,
                        Name1 = e1.Name
                    },
                ss =>
                    from e2 in ss.Set<Level2>()
                    join e1 in ss.Set<Level1>().OrderBy(l1 => l1.Id) on e2.Name equals Maybe(
                        e1.OneToOne_Optional_FK1, () => e1.OneToOne_Optional_FK1.Name)
                    select new
                    {
                        Id2 = e2.Id,
                        Name2 = e2.Name,
                        Id1 = e1.Id,
                        Name1 = e1.Name
                    },
                e => (e.Id2, e.Name2, e.Id1, e.Name1));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_navigation_self_ref(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from e1 in ss.Set<Level1>()
                    join e2 in ss.Set<Level1>() on e1.Id equals e2.OneToMany_Optional_Self_Inverse1.Id
                    select new { Id1 = e1.Id, Id2 = e2.Id },
                ss =>
                    from e1 in ss.Set<Level1>()
                    join e2 in ss.Set<Level1>() on e1.Id equals MaybeScalar<int>(
                        e2.OneToMany_Optional_Self_Inverse1, () => e2.OneToMany_Optional_Self_Inverse1.Id)
                    select new { Id1 = e1.Id, Id2 = e2.Id },
                e => (e.Id1, e.Id2));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_navigation_nested(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from e3 in ss.Set<Level3>()
                    join e1 in ss.Set<Level1>() on e3.Id equals e1.OneToOne_Required_FK1.OneToOne_Optional_FK2.Id
                    select new { Id3 = e3.Id, Id1 = e1.Id },
                ss =>
                    from e3 in ss.Set<Level3>()
                    join e1 in ss.Set<Level1>() on e3.Id equals MaybeScalar(
                        e1.OneToOne_Required_FK1,
                        () => MaybeScalar<int>(
                            e1.OneToOne_Required_FK1.OneToOne_Optional_FK2,
                            () => e1.OneToOne_Required_FK1.OneToOne_Optional_FK2.Id))
                    select new { Id3 = e3.Id, Id1 = e1.Id },
                e => (e.Id3, e.Id1));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_navigation_nested2(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from e3 in ss.Set<Level3>()
                    join e1 in ss.Set<Level1>().OrderBy(ll => ll.Id) on e3.Id equals e1.OneToOne_Required_FK1.OneToOne_Optional_FK2.Id
                    select new { Id3 = e3.Id, Id1 = e1.Id },
                ss =>
                    from e3 in ss.Set<Level3>()
                    join e1 in ss.Set<Level1>().OrderBy(ll => ll.Id) on e3.Id equals MaybeScalar(
                        e1.OneToOne_Required_FK1,
                        () => MaybeScalar<int>(
                            e1.OneToOne_Required_FK1.OneToOne_Optional_FK2,
                            () => e1.OneToOne_Required_FK1.OneToOne_Optional_FK2.Id))
                    select new { Id3 = e3.Id, Id1 = e1.Id },
                e => (e.Id3, e.Id1));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_navigation_deeply_nested_non_key_join(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from e4 in ss.Set<Level4>()
                    join e1 in ss.Set<Level1>() on e4.Name equals e1.OneToOne_Required_FK1.OneToOne_Optional_FK2.OneToOne_Required_PK3.Name
                    select new
                    {
                        Id4 = e4.Id,
                        Name4 = e4.Name,
                        Id1 = e1.Id,
                        Name1 = e1.Name
                    },
                ss =>
                    from e4 in ss.Set<Level4>()
                    join e1 in ss.Set<Level1>() on e4.Name equals Maybe(
                        e1.OneToOne_Required_FK1,
                        () => Maybe(
                            e1.OneToOne_Required_FK1.OneToOne_Optional_FK2,
                            () => Maybe(
                                e1.OneToOne_Required_FK1.OneToOne_Optional_FK2.OneToOne_Required_PK3,
                                () => e1.OneToOne_Required_FK1.OneToOne_Optional_FK2.OneToOne_Required_PK3.Name)))
                    select new
                    {
                        Id4 = e4.Id,
                        Name4 = e4.Name,
                        Id1 = e1.Id,
                        Name1 = e1.Name
                    },
                e => (e.Id4, e.Name4, e.Id1, e.Name1));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_navigation_deeply_nested_required(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from e1 in ss.Set<Level1>()
                    join e4 in ss.Set<Level4>() on e1.Name equals e4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3
                        .OneToOne_Required_PK_Inverse2.Name
                    select new
                    {
                        Id4 = e4.Id,
                        Name4 = e4.Name,
                        Id1 = e1.Id,
                        Name1 = e1.Name
                    },
                e => (e.Id4, e.Name4, e.Id1, e.Name1));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_complex_includes(bool async)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1, "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level2>(
                    l2 => l2.OneToMany_Optional2, "OneToMany_Optional2", navigationPath: "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level1>(l1 => l1.OneToMany_Optional1, "OneToMany_Optional1"),
                new ExpectedInclude<Level2>(
                    l2 => l2.OneToOne_Optional_FK2, "OneToOne_Optional_FK2", navigationPath: "OneToMany_Optional1")
            };

            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>()
                    .Include(e => e.OneToOne_Optional_FK1)
                    .ThenInclude(e => e.OneToMany_Optional2)
                    .Include(e => e.OneToMany_Optional1)
                    .ThenInclude(e => e.OneToOne_Optional_FK2),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_complex_includes_self_ref(bool async)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_Self1, "OneToOne_Optional_Self1"),
                new ExpectedInclude<Level1>(
                    l2 => l2.OneToMany_Optional_Self1, "OneToMany_Optional_Self1", navigationPath: "OneToOne_Optional_Self1"),
                new ExpectedInclude<Level1>(l1 => l1.OneToMany_Optional_Self1, "OneToMany_Optional_Self1"),
                new ExpectedInclude<Level1>(
                    l2 => l2.OneToOne_Optional_Self1, "OneToOne_Optional_Self1", navigationPath: "OneToMany_Optional_Self1")
            };

            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>()
                    .Include(e => e.OneToOne_Optional_Self1)
                    .ThenInclude(e => e.OneToMany_Optional_Self1)
                    .Include(e => e.OneToMany_Optional_Self1)
                    .ThenInclude(e => e.OneToOne_Optional_Self1),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_complex_include_select(bool async)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1, "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level2>(
                    l2 => l2.OneToMany_Optional2, "OneToMany_Optional2", navigationPath: "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level1>(l1 => l1.OneToMany_Optional1, "OneToMany_Optional1"),
                new ExpectedInclude<Level2>(
                    l2 => l2.OneToOne_Optional_FK2, "OneToOne_Optional_FK2", navigationPath: "OneToMany_Optional1")
            };

            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>()
                    .Select(e => e)
                    .Include(e => e.OneToOne_Optional_FK1)
                    .ThenInclude(e => e.OneToMany_Optional2)
                    .Select(e => e)
                    .Include(e => e.OneToMany_Optional1)
                    .ThenInclude(e => e.OneToOne_Optional_FK2),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_nav_prop_collection_one_to_many_required(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().OrderBy(e => e.Id).Select(e => e.OneToMany_Required1.Select(i => i.Id)),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_nav_prop_reference_optional1(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().Select(e => e.OneToOne_Optional_FK1.Name),
                ss => ss.Set<Level1>().Select(e => Maybe(e.OneToOne_Optional_FK1, () => e.OneToOne_Optional_FK1.Name)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_nav_prop_reference_optional1_via_DefaultIfEmpty(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from l1 in ss.Set<Level1>()
                    join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into groupJoin
                    from l2 in groupJoin.DefaultIfEmpty()
#pragma warning disable IDE0031 // Use null propagation
                    select l2 == null ? null : l2.Name,
#pragma warning restore IDE0031 // Use null propagation
                ss =>
                    from l1 in ss.Set<Level1>()
                    join l2 in ss.Set<Level2>() on l1.Id equals MaybeScalar(l2, () => l2.Level1_Optional_Id) into groupJoin
                    from l2 in groupJoin.DefaultIfEmpty()
#pragma warning disable IDE0031 // Use null propagation
                    select l2 == null ? null : l2.Name);
#pragma warning restore IDE0031 // Use null propagation
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_nav_prop_reference_optional2(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Level1>().Select(e => (int?)e.OneToOne_Optional_FK1.Id),
                ss => ss.Set<Level1>().Select(e => MaybeScalar<int>(e.OneToOne_Optional_FK1, () => e.OneToOne_Optional_FK1.Id)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_nav_prop_reference_optional2_via_DefaultIfEmpty(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => from l1 in ss.Set<Level1>()
                      join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into groupJoin
                      from l2 in groupJoin.DefaultIfEmpty()
                      select l2 == null ? null : (int?)l2.Id,
                ss => from l1 in ss.Set<Level1>()
                      join l2 in ss.Set<Level2>() on l1.Id equals MaybeScalar(l2, () => l2.Level1_Optional_Id) into groupJoin
                      from l2 in Maybe(groupJoin, () => groupJoin.DefaultIfEmpty())
                      select l2 == null ? null : (int?)l2.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_nav_prop_reference_optional3(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level2>().Select(e => e.OneToOne_Optional_FK_Inverse2.Name),
                ss => ss.Set<Level2>().Select(e => Maybe(e.OneToOne_Optional_FK_Inverse2, () => e.OneToOne_Optional_FK_Inverse2.Name)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_nav_prop_reference_optional1(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Level1>()
                    .Where(e => e.OneToOne_Optional_FK1.Name == "L2 05" || e.OneToOne_Optional_FK1.Name == "L2 07")
                    .Select(e => e.Id),
                ss => ss.Set<Level1>()
                    .Where(
                        e => Maybe(e.OneToOne_Optional_FK1, () => e.OneToOne_Optional_FK1.Name) == "L2 05"
                            || Maybe(e.OneToOne_Optional_FK1, () => e.OneToOne_Optional_FK1.Name) == "L2 07")
                    .Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_nav_prop_reference_optional1_via_DefaultIfEmpty(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => from l1 in ss.Set<Level1>()
                      join l2Left in ss.Set<Level2>() on l1.Id equals l2Left.Level1_Optional_Id into groupJoinLeft
                      from l2Left in groupJoinLeft.DefaultIfEmpty()
                      join l2Right in ss.Set<Level2>() on l1.Id equals l2Right.Level1_Optional_Id into groupJoinRight
                      from l2Right in groupJoinRight.DefaultIfEmpty()
#pragma warning disable IDE0031 // Use null propagation
                      where (l2Left == null ? null : l2Left.Name) == "L2 05" || (l2Right == null ? null : l2Right.Name) == "L2 07"
#pragma warning restore IDE0031 // Use null propagation
                      select l1.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_nav_prop_reference_optional2(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Level1>()
                    .Where(e => e.OneToOne_Optional_FK1.Name == "L2 05" || e.OneToOne_Optional_FK1.Name != "L2 42")
                    .Select(e => e.Id),
                ss => ss.Set<Level1>()
                    .Where(
                        e => Maybe(e.OneToOne_Optional_FK1, () => e.OneToOne_Optional_FK1.Name) == "L2 05"
                            || Maybe(e.OneToOne_Optional_FK1, () => e.OneToOne_Optional_FK1.Name) != "L2 42")
                    .Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_nav_prop_reference_optional2_via_DefaultIfEmpty(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => from l1 in ss.Set<Level1>()
                      join l2Left in ss.Set<Level2>() on l1.Id equals l2Left.Level1_Optional_Id into groupJoinLeft
                      from l2Left in groupJoinLeft.DefaultIfEmpty()
                      join l2Right in ss.Set<Level2>() on l1.Id equals l2Right.Level1_Optional_Id into groupJoinRight
                      from l2Right in groupJoinRight.DefaultIfEmpty()
#pragma warning disable IDE0031 // Use null propagation
                      where (l2Left == null ? null : l2Left.Name) == "L2 05" || (l2Right == null ? null : l2Right.Name) != "L2 42"
#pragma warning restore IDE0031 // Use null propagation
                      select l1.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_multiple_nav_prop_reference_optional(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Level1>().Select(e => (int?)e.OneToOne_Optional_FK1.OneToOne_Optional_FK2.Id),
                ss => ss.Set<Level1>().Select(
                    e => MaybeScalar(
                        e.OneToOne_Optional_FK1,
                        () => MaybeScalar<int>(
                            e.OneToOne_Optional_FK1.OneToOne_Optional_FK2,
                            () => e.OneToOne_Optional_FK1.OneToOne_Optional_FK2.Id))));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_multiple_nav_prop_reference_optional_member_compared_to_value(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from l1 in ss.Set<Level1>()
                    where l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2.Name != "L3 05"
                    select l1,
                ss =>
                    from l1 in ss.Set<Level1>()
                    where Maybe(
                            l1.OneToOne_Optional_FK1,
                            () => Maybe(
                                l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2,
                                () => l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2.Name))
                        != "L3 05"
                    select l1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_multiple_nav_prop_reference_optional_member_compared_to_null(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from l1 in ss.Set<Level1>()
                    where l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2.Name != null
                    select l1,
                ss =>
                    from l1 in ss.Set<Level1>()
                    where Maybe(
                            l1.OneToOne_Optional_FK1,
                            () => Maybe(
                                l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2,
                                () => l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2.Name))
                        != null
                    select l1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_multiple_nav_prop_reference_optional_compared_to_null1(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from l1 in ss.Set<Level1>()
                    where l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2 == null
                    select l1,
                ss =>
                    from l1 in ss.Set<Level1>()
                    where Maybe(
                            l1.OneToOne_Optional_FK1,
                            () => l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2)
                        == null
                    select l1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_multiple_nav_prop_reference_optional_compared_to_null2(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from l3 in ss.Set<Level3>()
                    where l3.OneToOne_Optional_FK_Inverse3.OneToOne_Optional_FK_Inverse2 == null
                    select l3,
                ss =>
                    from l3 in ss.Set<Level3>()
                    where Maybe(
                            l3.OneToOne_Optional_FK_Inverse3,
                            () => l3.OneToOne_Optional_FK_Inverse3.OneToOne_Optional_FK_Inverse2)
                        == null
                    select l3);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_multiple_nav_prop_reference_optional_compared_to_null3(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from l1 in ss.Set<Level1>()
                    where null != l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2
                    select l1,
                ss =>
                    from l1 in ss.Set<Level1>()
                    where null
                        != Maybe(
                            l1.OneToOne_Optional_FK1,
                            () => l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2)
                    select l1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_multiple_nav_prop_reference_optional_compared_to_null4(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from l3 in ss.Set<Level3>()
                    where null != l3.OneToOne_Optional_FK_Inverse3.OneToOne_Optional_FK_Inverse2
                    select l3,
                ss =>
                    from l3 in ss.Set<Level3>()
                    where null
                        != Maybe(
                            l3.OneToOne_Optional_FK_Inverse3, () => l3.OneToOne_Optional_FK_Inverse3.OneToOne_Optional_FK_Inverse2)
                    select l3);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_multiple_nav_prop_reference_optional_compared_to_null5(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().Where(e => e.OneToOne_Optional_FK1.OneToOne_Required_FK2.OneToOne_Required_FK3 == null),
                ss => ss.Set<Level1>().Where(
                    e => Maybe(
                            e.OneToOne_Optional_FK1,
                            () => Maybe(
                                e.OneToOne_Optional_FK1.OneToOne_Required_FK2,
                                () => e.OneToOne_Optional_FK1.OneToOne_Required_FK2.OneToOne_Required_FK3))
                        == null));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_multiple_nav_prop_reference_required(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Level1>().Select(e => (int?)e.OneToOne_Required_FK1.OneToOne_Required_FK2.Id),
                ss => ss.Set<Level1>().Select(
                    e => MaybeScalar(
                        e.OneToOne_Required_FK1,
                        () => MaybeScalar<int>(
                            e.OneToOne_Required_FK1.OneToOne_Required_FK2,
                            () => e.OneToOne_Required_FK1.OneToOne_Required_FK2.Id))));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_multiple_nav_prop_reference_required2(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Level3>().Select(e => e.OneToOne_Required_FK_Inverse3.OneToOne_Required_FK_Inverse2.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_multiple_nav_prop_optional_required(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => from l1 in ss.Set<Level1>()
                      select (int?)l1.OneToOne_Optional_FK1.OneToOne_Required_FK2.Id,
                ss => from l1 in ss.Set<Level1>()
                      select MaybeScalar(
                          l1.OneToOne_Optional_FK1,
                          () => MaybeScalar<int>(
                              l1.OneToOne_Optional_FK1.OneToOne_Required_FK2,
                              () => l1.OneToOne_Optional_FK1.OneToOne_Required_FK2.Id)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_multiple_nav_prop_optional_required(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from l1 in ss.Set<Level1>()
                    where l1.OneToOne_Optional_FK1.OneToOne_Required_FK2.Name != "L3 05"
                    select l1,
                ss =>
                    from l1 in ss.Set<Level1>()
                    where Maybe(
                            l1.OneToOne_Optional_FK1,
                            () => Maybe(
                                l1.OneToOne_Optional_FK1.OneToOne_Required_FK2,
                                () => l1.OneToOne_Optional_FK1.OneToOne_Required_FK2.Name))
                        != "L3 05"
                    select l1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_navigation_comparison1(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from l11 in ss.Set<Level1>()
                    from l12 in ss.Set<Level1>()
                    where l11 == l12
                    select new { Id1 = l11.Id, Id2 = l12.Id },
                ss =>
                    from l11 in ss.Set<Level1>()
                    from l12 in ss.Set<Level1>()
                    where l11.Id == l12.Id
                    select new { Id1 = l11.Id, Id2 = l12.Id },
                e => (e.Id1, e.Id2));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_navigation_comparison2(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from l1 in ss.Set<Level1>()
                    from l2 in ss.Set<Level2>()
                    where l1 == l2.OneToOne_Optional_FK_Inverse2
                    select new { Id1 = l1.Id, Id2 = l2.Id },
                ss =>
                    from l1 in ss.Set<Level1>()
                    from l2 in ss.Set<Level2>()
                    where l1.Id == MaybeScalar<int>(l2.OneToOne_Optional_FK_Inverse2, () => l2.OneToOne_Optional_FK_Inverse2.Id)
                    select new { Id1 = l1.Id, Id2 = l2.Id },
                e => (e.Id1, e.Id2));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_navigation_comparison3(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from l1 in ss.Set<Level1>()
                    from l2 in ss.Set<Level2>()
                    where l1.OneToOne_Optional_FK1 == l2
                    select new { Id1 = l1.Id, Id2 = l2.Id },
                ss =>
                    from l1 in ss.Set<Level1>()
                    from l2 in ss.Set<Level2>()
                    where MaybeScalar<int>(l1.OneToOne_Optional_FK1, () => l1.OneToOne_Optional_FK1.Id) == l2.Id
                    select new { Id1 = l1.Id, Id2 = l2.Id },
                e => (e.Id1, e.Id2));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_complex_predicate_with_with_nav_prop_and_OrElse1(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from l1 in ss.Set<Level1>()
                    from l2 in ss.Set<Level2>()
                    where l1.OneToOne_Optional_FK1.Name == "L2 01" || l2.OneToOne_Required_FK_Inverse2.Name != "Bar"
                    select new { Id1 = (int?)l1.Id, Id2 = (int?)l2.Id },
                ss =>
                    from l1 in ss.Set<Level1>()
                    from l2 in ss.Set<Level2>()
                    where Maybe(l1.OneToOne_Optional_FK1, () => l1.OneToOne_Optional_FK1.Name) == "L2 01"
                        || l2.OneToOne_Required_FK_Inverse2.Name != "Bar"
                    select new { Id1 = (int?)l1.Id, Id2 = (int?)l2.Id },
                e => (e.Id1, e.Id2));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_complex_predicate_with_with_nav_prop_and_OrElse2(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => from l1 in ss.Set<Level1>()
                      where l1.OneToOne_Optional_FK1.OneToOne_Required_FK2.Name == "L3 05" || l1.OneToOne_Optional_FK1.Name != "L2 05"
                      select l1.Id,
                ss => from l1 in ss.Set<Level1>()
                      where Maybe(
                              l1.OneToOne_Optional_FK1,
                              () => Maybe(
                                  l1.OneToOne_Optional_FK1.OneToOne_Required_FK2,
                                  () => l1.OneToOne_Optional_FK1.OneToOne_Required_FK2.Name))
                          == "L3 05"
                          || Maybe(
                              l1.OneToOne_Optional_FK1,
                              () => l1.OneToOne_Optional_FK1.Name)
                          != "L2 05"
                      select l1.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_complex_predicate_with_with_nav_prop_and_OrElse3(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => from l1 in ss.Set<Level1>()
                      where l1.OneToOne_Optional_FK1.Name != "L2 05" || l1.OneToOne_Required_FK1.OneToOne_Optional_FK2.Name == "L3 05"
                      select l1.Id,
                ss => from l1 in ss.Set<Level1>()
                      where Maybe(
                              l1.OneToOne_Optional_FK1,
                              () => l1.OneToOne_Optional_FK1.Name)
                          != "L2 05"
                          || Maybe(
                              l1.OneToOne_Required_FK1,
                              () => Maybe(
                                  l1.OneToOne_Required_FK1.OneToOne_Optional_FK2,
                                  () => l1.OneToOne_Required_FK1.OneToOne_Optional_FK2.Name))
                          == "L3 05"
                      select l1.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_complex_predicate_with_with_nav_prop_and_OrElse4(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => from l3 in ss.Set<Level3>()
                      where l3.OneToOne_Optional_FK_Inverse3.Name != "L2 05"
                          || l3.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK_Inverse2.Name == "L1 05"
                      select l3.Id,
                ss => from l3 in ss.Set<Level3>()
                      where Maybe(
                              l3.OneToOne_Optional_FK_Inverse3,
                              () => l3.OneToOne_Optional_FK_Inverse3.Name)
                          != "L2 05"
                          || Maybe(
                              l3.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK_Inverse2,
                              () => l3.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK_Inverse2.Name)
                          == "L1 05"
                      select l3.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Complex_navigations_with_predicate_projected_into_anonymous_type(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>()
                    .Where(
                        e => e.OneToOne_Required_FK1.OneToOne_Required_FK2 == e.OneToOne_Required_FK1.OneToOne_Optional_FK2
                            && e.OneToOne_Required_FK1.OneToOne_Optional_FK2.Id != 7)
                    .Select(
                        e => new { e.Name, Id = (int?)e.OneToOne_Required_FK1.OneToOne_Optional_FK2.Id }),
                ss => ss.Set<Level1>()
                    .Where(
                        e => Maybe(e.OneToOne_Required_FK1, () => e.OneToOne_Required_FK1.OneToOne_Required_FK2)
                            == Maybe(
                                e.OneToOne_Required_FK1, () => e.OneToOne_Required_FK1.OneToOne_Optional_FK2)
                            && MaybeScalar(
                                e.OneToOne_Required_FK1,
                                () => MaybeScalar<int>(
                                    e.OneToOne_Required_FK1.OneToOne_Optional_FK2, () => e.OneToOne_Required_FK1.OneToOne_Optional_FK2.Id))
                            != 7)
                    .Select(
                        e => new
                        {
                            e.Name,
                            Id = MaybeScalar(
                                e.OneToOne_Required_FK1,
                                () => MaybeScalar<int>(
                                    e.OneToOne_Required_FK1.OneToOne_Optional_FK2, () => e.OneToOne_Required_FK1.OneToOne_Optional_FK2.Id))
                        }),
                elementSorter: e => (e.Name, e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Complex_navigations_with_predicate_projected_into_anonymous_type2(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from e in ss.Set<Level3>()
                    where e.OneToOne_Required_FK_Inverse3.OneToOne_Required_FK_Inverse2
                        == e.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK_Inverse2
                        && e.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK_Inverse2.Id != 7
                    select new { e.Name, Id = (int?)e.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK_Inverse2.Id },
                ss =>
                    from e in ss.Set<Level3>()
                    where e.OneToOne_Required_FK_Inverse3.OneToOne_Required_FK_Inverse2
                        == e.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK_Inverse2
                        && MaybeScalar<int>(
                            e.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK_Inverse2,
                            () => e.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK_Inverse2.Id)
                        != 7
                    select new
                    {
                        e.Name,
                        Id = MaybeScalar<int>(
                            e.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK_Inverse2,
                            () => e.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK_Inverse2.Id)
                    },
                e => (e.Name, e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_projected_into_DTO(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().Select(
                    e => new MyOuterDto
                    {
                        Id = e.Id,
                        Name = e.Name,
                        Inner = e.OneToOne_Optional_FK1 != null
                            ? new MyInnerDto { Id = e.OneToOne_Optional_FK1.Id, Name = e.OneToOne_Optional_FK1.Name }
                            : null
                    }),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal(e.Name, a.Name);
                    Assert.Equal(e.Inner?.Id, a.Inner?.Id);
                    Assert.Equal(e.Inner?.Name, a.Inner?.Name);
                });
        }

        public class MyOuterDto
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public MyInnerDto Inner { get; set; }
        }

        public class MyInnerDto
        {
            public int? Id { get; set; }
            public string Name { get; set; }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_nav_prop_reference_optional(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Level1>().OrderBy(e => e.OneToOne_Optional_FK1.Name).ThenBy(e => e.Id).Select(e => e.Id),
                ss => ss.Set<Level1>().OrderBy(e => Maybe(e.OneToOne_Optional_FK1, () => e.OneToOne_Optional_FK1.Name))
                    .ThenBy(e => e.Id)
                    .Select(e => e.Id),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_nav_prop_reference_optional_via_DefaultIfEmpty(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => from l1 in ss.Set<Level1>()
                      join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into groupJoin
                      from l2 in groupJoin.DefaultIfEmpty()
#pragma warning disable IDE0031 // Use null propagation
                      orderby l2 == null ? null : l2.Name, l1.Id
#pragma warning restore IDE0031 // Use null propagation
                      select l1.Id,
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Result_operator_nav_prop_reference_optional_Sum(bool async)
        {
            return AssertSum(
                async,
                ss => ss.Set<Level1>(),
                ss => ss.Set<Level1>(),
                actualSelector: e => (int?)e.OneToOne_Optional_FK1.Level1_Required_Id,
                expectedSelector: e => MaybeScalar<int>(e.OneToOne_Optional_FK1, () => e.OneToOne_Optional_FK1.Level1_Required_Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Result_operator_nav_prop_reference_optional_Min(bool async)
        {
            return AssertMin(
                async,
                ss => ss.Set<Level1>(),
                ss => ss.Set<Level1>(),
                actualSelector: e => (int?)e.OneToOne_Optional_FK1.Level1_Required_Id,
                expectedSelector: e => MaybeScalar<int>(e.OneToOne_Optional_FK1, () => e.OneToOne_Optional_FK1.Level1_Required_Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Result_operator_nav_prop_reference_optional_Max(bool async)
        {
            return AssertMax(
                async,
                ss => ss.Set<Level1>(),
                ss => ss.Set<Level1>(),
                actualSelector: e => (int?)e.OneToOne_Optional_FK1.Level1_Required_Id,
                expectedSelector: e => MaybeScalar<int>(e.OneToOne_Optional_FK1, () => e.OneToOne_Optional_FK1.Level1_Required_Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Result_operator_nav_prop_reference_optional_Average(bool async)
        {
            return AssertAverage(
                async,
                ss => ss.Set<Level1>(),
                ss => ss.Set<Level1>(),
                actualSelector: e => (int?)e.OneToOne_Optional_FK1.Level1_Required_Id,
                expectedSelector: e => MaybeScalar<int>(e.OneToOne_Optional_FK1, () => e.OneToOne_Optional_FK1.Level1_Required_Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Result_operator_nav_prop_reference_optional_Average_with_identity_selector(bool async)
        {
            return AssertAverage(
                async,
                ss => ss.Set<Level1>().Select(e => (int?)e.OneToOne_Optional_FK1.Level1_Required_Id),
                ss => ss.Set<Level1>().Select(
                    e => MaybeScalar<int>(e.OneToOne_Optional_FK1, () => e.OneToOne_Optional_FK1.Level1_Required_Id)),
                actualSelector: e => e,
                expectedSelector: e => e);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Result_operator_nav_prop_reference_optional_Average_without_selector(bool async)
        {
            return AssertAverage(
                async,
                ss => ss.Set<Level1>().Select(e => (int?)e.OneToOne_Optional_FK1.Level1_Required_Id),
                ss => ss.Set<Level1>().Select(
                    e => MaybeScalar<int>(e.OneToOne_Optional_FK1, () => e.OneToOne_Optional_FK1.Level1_Required_Id)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Result_operator_nav_prop_reference_optional_via_DefaultIfEmpty(bool async)
        {
            return AssertSum(
                async,
                ss => from l1 in ss.Set<Level1>()
                      join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into groupJoin
                      from l2 in groupJoin.DefaultIfEmpty()
                      select l2,
                selector: e => e == null ? 0 : e.Level1_Required_Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_with_optional_navigation(bool async)
        {
            return AssertIncludeQuery(
                async,
                ss => from l1 in ss.Set<Level1>().Include(e => e.OneToOne_Optional_FK1)
                      where l1.OneToOne_Optional_FK1.Name != "L2 05"
                      select l1,
                ss => from l1 in ss.Set<Level1>().Include(e => e.OneToOne_Optional_FK1)
                      where Maybe(l1.OneToOne_Optional_FK1, () => l1.OneToOne_Optional_FK1.Name) != "L2 05"
                      select l1,
                new List<IExpectedInclude> { new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1, "OneToOne_Optional_FK1") });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_nested_with_optional_navigation(bool async)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1, "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level2>(l1 => l1.OneToMany_Required2, "OneToMany_Required2", "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level3>(
                    l1 => l1.OneToOne_Required_FK3, "OneToOne_Required_FK3", "OneToOne_Optional_FK1.OneToMany_Required2")
            };

            return AssertIncludeQuery(
                async,
                ss => from l1 in ss.Set<Level1>()
                          .Include(e => e.OneToOne_Optional_FK1.OneToMany_Required2)
                          .ThenInclude(e => e.OneToOne_Required_FK3)
                      where l1.OneToOne_Optional_FK1.Name != "L2 09"
                      select l1,
                ss => from l1 in ss.Set<Level1>()
                      where Maybe(l1.OneToOne_Optional_FK1, () => l1.OneToOne_Optional_FK1.Name) != "L2 09"
                      select l1,
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_flattening_bug_4539(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from l1 in ss.Set<Level1>()
                    join l1_Optional in ss.Set<Level2>() on (int?)l1.Id equals l1_Optional.Level1_Optional_Id into grouping
                    from l1_Optional in grouping.DefaultIfEmpty()
                    from l2 in ss.Set<Level2>()
                    join l2_Required_Reverse in ss.Set<Level1>() on l2.Level1_Required_Id equals l2_Required_Reverse.Id
                    select new { l1_Optional, l2_Required_Reverse },
                elementSorter: e => (e.l1_Optional?.Id, e.l2_Required_Reverse.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Query_source_materialization_bug_4547(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => from e3 in ss.Set<Level3>()
                      join e1 in ss.Set<Level1>()
                          on
                          e3.Id
                          equals
                          (
                              from subQuery2 in ss.Set<Level2>()
                              join subQuery3 in ss.Set<Level3>()
                                  on
                                  subQuery2 != null ? (int?)subQuery2.Id : null
                                  equals
                                  subQuery3.Level2_Optional_Id
                                  into
                                  grouping
                              from subQuery3 in grouping.DefaultIfEmpty()
                              orderby subQuery3 != null ? (int?)subQuery3.Id : null
                              select subQuery3 != null ? (int?)subQuery3.Id : null
                          ).FirstOrDefault()
                      select e1.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_navigation_property(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().SelectMany(l1 => l1.OneToMany_Optional1));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_navigation_property_and_projection(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().SelectMany(l1 => l1.OneToMany_Optional1).Select(e => e.Name));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_navigation_property_and_filter_before(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().Where(e => e.Id == 1).SelectMany(l1 => l1.OneToMany_Optional1));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_navigation_property_and_filter_after(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().SelectMany(l1 => l1.OneToMany_Optional1).Where(e => e.Id != 6));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_nested_navigation_property_required(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().SelectMany(l1 => l1.OneToOne_Required_FK1.OneToMany_Optional2),
                ss => ss.Set<Level1>().SelectMany(
                    l1 => Maybe(
                            l1.OneToOne_Required_FK1,
                            () => l1.OneToOne_Required_FK1.OneToMany_Optional2)
                        ?? new List<Level3>()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_nested_navigation_property_optional_and_projection(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().SelectMany(l1 => l1.OneToOne_Optional_FK1.OneToMany_Optional2).Select(e => e.Name),
                ss => ss.Set<Level1>().SelectMany(
                    l1 => Maybe(
                            l1.OneToOne_Optional_FK1,
                            () => l1.OneToOne_Optional_FK1.OneToMany_Optional2)
                        ?? new List<Level3>()).Select(e => e.Name));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_SelectMany_calls(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().SelectMany(e => e.OneToMany_Optional1).SelectMany(e => e.OneToMany_Optional2));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_navigation_property_with_another_navigation_in_subquery(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().SelectMany(l1 => l1.OneToMany_Optional1.Select(l2 => l2.OneToOne_Optional_FK2)),
                ss => ss.Set<Level1>().SelectMany(
                    l1 => Maybe(
                            l1.OneToMany_Optional1,
                            () => l1.OneToMany_Optional1.Select(l2 => l2.OneToOne_Optional_FK2))
                        ?? new List<Level3>()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_navigation_property_to_collection(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().Where(l1 => l1.OneToOne_Required_FK1.OneToMany_Optional2.Count > 0),
                ss => ss.Set<Level1>().Where(
                    l1 => MaybeScalar(
                            l1.OneToOne_Required_FK1,
                            () => MaybeScalar<int>(
                                l1.OneToOne_Required_FK1.OneToMany_Optional2,
                                () => l1.OneToOne_Required_FK1.OneToMany_Optional2.Count))
                        > 0));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_navigation_property_to_collection2(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level3>().Where(l3 => l3.OneToOne_Required_FK_Inverse3.OneToMany_Optional2.Count > 0),
                ss => ss.Set<Level3>().Where(
                    l3 => MaybeScalar<int>(
                            l3.OneToOne_Required_FK_Inverse3.OneToMany_Optional2,
                            () => l3.OneToOne_Required_FK_Inverse3.OneToMany_Optional2.Count)
                        > 0));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_navigation_property_to_collection_of_original_entity_type(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level2>().Where(l2 => l2.OneToMany_Required_Inverse2.OneToMany_Optional1.Count() > 0),
                ss => ss.Set<Level2>().Where(
                    l2 => MaybeScalar<int>(
                            l2.OneToMany_Required_Inverse2.OneToMany_Optional1,
                            () => l2.OneToMany_Required_Inverse2.OneToMany_Optional1.Count())
                        > 0));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Complex_multi_include_with_order_by_and_paging(bool async)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Required_FK1, "OneToOne_Required_FK1"),
                new ExpectedInclude<Level2>(l1 => l1.OneToMany_Optional2, "OneToMany_Optional2", "OneToOne_Required_FK1"),
                new ExpectedInclude<Level2>(l1 => l1.OneToMany_Required2, "OneToMany_Required2", "OneToOne_Required_FK1")
            };

            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>()
                    .Include(e => e.OneToOne_Required_FK1).ThenInclude(e => e.OneToMany_Optional2)
                    .Include(e => e.OneToOne_Required_FK1).ThenInclude(e => e.OneToMany_Required2)
                    .OrderBy(t => t.Name)
                    .Skip(0).Take(10),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Complex_multi_include_with_order_by_and_paging_joins_on_correct_key(bool async)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1, "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional2, "OneToMany_Optional2", "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Required_FK1, "OneToOne_Required_FK1"),
                new ExpectedInclude<Level2>(l2 => l2.OneToMany_Required2, "OneToMany_Required2", "OneToOne_Required_FK1")
            };

            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>()
                    .Include(e => e.OneToOne_Optional_FK1).ThenInclude(e => e.OneToMany_Optional2)
                    .Include(e => e.OneToOne_Required_FK1).ThenInclude(e => e.OneToMany_Required2)
                    .OrderBy(t => t.Name)
                    .Skip(0).Take(10),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Complex_multi_include_with_order_by_and_paging_joins_on_correct_key2(bool async)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1, "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Required_FK2, "OneToOne_Required_FK2", "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level3>(
                    l3 => l3.OneToMany_Optional3, "OneToMany_Optional3", "OneToOne_Optional_FK1.OneToOne_Required_FK2")
            };

            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>()
                    .Include(e => e.OneToOne_Optional_FK1.OneToOne_Required_FK2).ThenInclude(e => e.OneToMany_Optional3)
                    .OrderBy(t => t.Name)
                    .Skip(0).Take(10),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_include_with_multiple_optional_navigations(bool async)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Required_FK1, "OneToOne_Required_FK1"),
                new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional2, "OneToMany_Optional2", "OneToOne_Required_FK1"),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2, "OneToOne_Optional_FK2", "OneToOne_Required_FK1"),
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1, "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2, "OneToOne_Optional_FK2", "OneToOne_Optional_FK1")
            };

            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>()
                    .Include(e => e.OneToOne_Required_FK1).ThenInclude(e => e.OneToMany_Optional2)
                    .Include(e => e.OneToOne_Required_FK1).ThenInclude(e => e.OneToOne_Optional_FK2)
                    .Include(e => e.OneToOne_Optional_FK1).ThenInclude(e => e.OneToOne_Optional_FK2)
                    .Where(e => e.OneToOne_Required_FK1.OneToOne_Optional_PK2.Name != "Foo")
                    .OrderBy(e => e.Id),
                ss => ss.Set<Level1>()
                    .Where(
                        e => Maybe(
                                e.OneToOne_Required_FK1,
                                () => Maybe(
                                    e.OneToOne_Required_FK1.OneToOne_Optional_PK2,
                                    () => e.OneToOne_Required_FK1.OneToOne_Optional_PK2.Name))
                            != "Foo")
                    .OrderBy(e => e.Id),
                expectedIncludes,
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_subquery_doesnt_project_unnecessary_columns_in_top_level(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    (from l1 in ss.Set<Level1>()
                     where ss.Set<Level2>().Any(l2 => l2.Level1_Required_Id == l1.Id)
                     select l1.Name).Distinct());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_subquery_doesnt_project_unnecessary_columns_in_top_level_join(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from e1 in ss.Set<Level1>()
                    join e2 in ss.Set<Level2>() on e1.Id equals e2.OneToOne_Optional_FK_Inverse2.Id
                    where ss.Set<Level2>().Any(l2 => l2.Level1_Required_Id == e1.Id)
                    select new { Name1 = e1.Name, Id2 = e2.Id },
                ss =>
                    from e1 in ss.Set<Level1>()
                    join e2 in ss.Set<Level2>() on e1.Id equals MaybeScalar<int>(
                        e2.OneToOne_Optional_FK_Inverse2, () => e2.OneToOne_Optional_FK_Inverse2.Id)
                    where ss.Set<Level2>().Any(l2 => l2.Level1_Required_Id == e1.Id)
                    select new { Name1 = e1.Name, Id2 = e2.Id },
                e => (e.Name1, e.Id2));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_nested_subquery_doesnt_project_unnecessary_columns_in_top_level(bool async)
        {
            return AssertQuery(
                async,
                ss => (from l1 in ss.Set<Level1>()
                       where ss.Set<Level2>().Any(l2 => ss.Set<Level3>().Select(l3 => l2.Id).Any())
                       select l1.Name).Distinct());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_nested_two_levels_up_subquery_doesnt_project_unnecessary_columns_in_top_level(bool async)
        {
            return AssertQuery(
                async,
                ss => (from l1 in ss.Set<Level1>()
                       where ss.Set<Level2>().Any(l2 => ss.Set<Level3>().Select(l3 => l1.Id).Any())
                       select l1.Name).Distinct()
            );
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_where_with_subquery(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().SelectMany(l1 => l1.OneToMany_Required1).Where(l2 => l2.OneToMany_Required2.Any()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access1(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level3>().OrderBy(l3 => l3.OneToOne_Required_FK_Inverse3.Id).Select(l3 => l3.OneToOne_Required_FK_Inverse3),
                elementAsserter: (e, a) => AssertEqual(e, a),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access2(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level3>().OrderBy(l3 => l3.OneToOne_Required_FK_Inverse3.Id)
                    .Select(l3 => EF.Property<Level2>(l3, "OneToOne_Required_FK_Inverse3")),
                ss => ss.Set<Level3>().OrderBy(l3 => l3.OneToOne_Required_FK_Inverse3.Id).Select(l3 => l3.OneToOne_Required_FK_Inverse3),
                elementAsserter: (e, a) => AssertEqual(e, a),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access3(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level3>().OrderBy(l3 => EF.Property<Level2>(l3, "OneToOne_Required_FK_Inverse3").Id)
                    .Select(l3 => l3.OneToOne_Required_FK_Inverse3),
                ss => ss.Set<Level3>().OrderBy(l3 => l3.OneToOne_Required_FK_Inverse3.Id).Select(l3 => l3.OneToOne_Required_FK_Inverse3),
                elementAsserter: (e, a) => AssertEqual(e, a),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Order_by_key_of_navigation_similar_to_projected_gets_optimized_into_FK_access(bool async)
        {
            return AssertQuery(
                async,
                ss => from l3 in ss.Set<Level3>()
                      orderby l3.OneToOne_Required_FK_Inverse3.Id
                      select l3.OneToOne_Required_FK_Inverse3.OneToOne_Required_FK_Inverse2,
                elementAsserter: (e, a) => AssertEqual(e, a),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access_subquery(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level3>()
                    .Select(l3 => l3.OneToOne_Required_FK_Inverse3)
                    .OrderBy(l2 => l2.Id)
                    .Take(10)
                    .Select(l2 => l2.OneToOne_Required_FK_Inverse2.Name),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Order_by_key_of_anonymous_type_projected_navigation_doesnt_get_optimized_into_FK_access_subquery(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level3>()
                    .Select(
                        l3 => new { l3.OneToOne_Required_FK_Inverse3, name = l3.Name })
                    .OrderBy(l3 => l3.OneToOne_Required_FK_Inverse3.Id)
                    .Take(10)
                    .Select(l2 => l2.OneToOne_Required_FK_Inverse3.Name),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_take_optional_navigation(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>()
                    .Select(l1 => l1.OneToOne_Optional_FK1)
                    .OrderBy(l2 => (int?)l2.Id)
                    .Take(10)
                    .Select(l2 => l2.OneToOne_Optional_FK2.Name),
                ss => ss.Set<Level1>()
                    .Select(l1 => l1.OneToOne_Optional_FK1)
                    .OrderBy(l2 => MaybeScalar<int>(l2, () => l2.Id))
                    .Take(10)
                    .Select(l2 => Maybe(l2, () => Maybe(l2.OneToOne_Optional_FK2, () => l2.OneToOne_Optional_FK2.Name))),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Projection_select_correct_table_from_subquery_when_materialization_is_not_required(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level2>().Where(l2 => l2.OneToOne_Required_FK_Inverse2.Name == "L1 03")
                    .OrderBy(l => l.Id).Take(3).Select(l2 => l2.Name));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Projection_select_correct_table_with_anonymous_projection_in_subquery(bool async)
        {
            return AssertQuery(
                async,
                ss => (from l2 in ss.Set<Level2>()
                       join l1 in ss.Set<Level1>() on l2.Level1_Required_Id equals l1.Id
                       join l3 in ss.Set<Level3>() on l1.Id equals l3.Level2_Required_Id
                       where l1.Name == "L1 03"
                       where l3.Name == "L3 08"
                       select new { l2, l1 })
                    .OrderBy(l => l.l1.Id)
                    .Take(3)
                    .Select(l => l.l2.Name));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Projection_select_correct_table_in_subquery_when_materialization_is_not_required_in_multiple_joins(bool async)
        {
            return AssertQuery(
                async,
                ss => (from l2 in ss.Set<Level2>()
                       join l1 in ss.Set<Level1>() on l2.Level1_Required_Id equals l1.Id
                       join l3 in ss.Set<Level3>() on l1.Id equals l3.Level2_Required_Id
                       where l1.Name == "L1 03"
                       where l3.Name == "L3 08"
                       select l1).OrderBy(l1 => l1.Id).Take(3).Select(l1 => l1.Name));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_predicate_on_optional_reference_navigation(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>()
                    .Where(l1 => l1.OneToOne_Required_FK1.Name == "L2 03")
                    .OrderBy(l1 => l1.Id)
                    .Take(3)
                    .Select(l1 => l1.Name),
                ss => ss.Set<Level1>()
                    .Where(l1 => Maybe(l1.OneToOne_Required_FK1, () => l1.OneToOne_Required_FK1.Name) == "L2 03")
                    .OrderBy(l1 => l1.Id)
                    .Take(3)
                    .Select(l1 => l1.Name));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_with_Include1(bool async)
        {
            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>()
                    .SelectMany(l1 => l1.OneToMany_Optional1)
                    .Include(l2 => l2.OneToMany_Optional2),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional2, "OneToMany_Optional2")
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Orderby_SelectMany_with_Include1(bool async)
        {
            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>().OrderBy(l1 => l1.Id)
                    .SelectMany(l1 => l1.OneToMany_Optional1)
                    .Include(l2 => l2.OneToMany_Optional2),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional2, "OneToMany_Optional2")
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_with_Include2(bool async)
        {
            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>()
                    .SelectMany(l1 => l1.OneToMany_Optional1)
                    .Include(l2 => l2.OneToOne_Required_FK2),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level2>(l2 => l2.OneToOne_Required_FK2, "OneToOne_Required_FK2")
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_with_Include_ThenInclude(bool async)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Required_FK2, "OneToOne_Required_FK2"),
                new ExpectedInclude<Level3>(l3 => l3.OneToMany_Optional3, "OneToMany_Optional3", "OneToOne_Required_FK2")
            };

            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>()
                    .SelectMany(l1 => l1.OneToMany_Optional1)
                    .Include(l2 => l2.OneToOne_Required_FK2)
                    .ThenInclude(l3 => l3.OneToMany_Optional3),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_SelectMany_with_Include(bool async)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level3>(l3 => l3.OneToOne_Required_FK3, "OneToOne_Required_FK3"),
                new ExpectedInclude<Level3>(l3 => l3.OneToMany_Optional3, "OneToMany_Optional3")
            };

            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>()
                    .SelectMany(l1 => l1.OneToMany_Optional1)
                    .SelectMany(l2 => l2.OneToMany_Optional2)
                    .Include(l3 => l3.OneToOne_Required_FK3)
                    .Include(l3 => l3.OneToMany_Optional3),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_with_string_based_Include1(bool async)
        {
            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>()
                    .SelectMany(l1 => l1.OneToMany_Optional1)
                    .Include("OneToOne_Required_FK2"),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level2>(l2 => l2.OneToOne_Required_FK2, "OneToOne_Required_FK2")
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_with_string_based_Include2(bool async)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Required_FK2, "OneToOne_Required_FK2"),
                new ExpectedInclude<Level3>(l3 => l3.OneToOne_Required_FK3, "OneToOne_Required_FK3", "OneToOne_Required_FK2")
            };

            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>()
                    .SelectMany(l1 => l1.OneToMany_Optional1)
                    .Include("OneToOne_Required_FK2.OneToOne_Required_FK3"),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_SelectMany_with_string_based_Include(bool async)
        {
            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>()
                    .SelectMany(l1 => l1.OneToMany_Optional1)
                    .SelectMany(l1 => l1.OneToMany_Optional2)
                    .Include("OneToOne_Required_FK3"),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level3>(l3 => l3.OneToOne_Required_FK3, "OneToOne_Required_FK3")
                });
        }

        [ConditionalTheory(Skip = "Issue#16752")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Required_navigation_with_Include(bool async)
        {
            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level3>()
                    .Select(l3 => l3.OneToOne_Required_FK_Inverse3)
                    .Include(l2 => l2.OneToMany_Required_Inverse2),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level2>(l2 => l2.OneToMany_Required_Inverse2, "OneToMany_Required_Inverse2")
                });
        }

        [ConditionalTheory(Skip = "Issue#16752")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Required_navigation_with_Include_ThenInclude(bool async)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level3>(l3 => l3.OneToMany_Required_Inverse3, "OneToMany_Required_Inverse3"),
                new ExpectedInclude<Level2>(
                    l2 => l2.OneToMany_Optional_Inverse2, "OneToMany_Optional_Inverse2", "OneToMany_Required_Inverse3")
            };

            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level4>()
                    .Select(l4 => l4.OneToOne_Required_FK_Inverse4)
                    .Include(l3 => l3.OneToMany_Required_Inverse3)
                    .ThenInclude(l2 => l2.OneToMany_Optional_Inverse2),
                expectedIncludes);
        }

        [ConditionalTheory(Skip = "Issue#16752")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_required_navigations_with_Include(bool async)
        {
            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level4>()
                    .Select(l4 => l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3)
                    .Include(l2 => l2.OneToOne_Optional_FK2),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2, "OneToOne_Optional_FK2")
                });
        }

        [ConditionalTheory(Skip = "Issue#16752")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_required_navigation_using_multiple_selects_with_Include(bool async)
        {
            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level4>()
                    .Select(l4 => l4.OneToOne_Required_FK_Inverse4)
                    .Select(l3 => l3.OneToOne_Required_FK_Inverse3)
                    .Include(l2 => l2.OneToOne_Optional_FK2),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2, "OneToOne_Optional_FK2")
                });
        }

        [ConditionalTheory(Skip = "Issue#16752")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_required_navigation_with_string_based_Include(bool async)
        {
            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level4>()
                    .Select(l4 => l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3)
                    .Include("OneToOne_Optional_FK2"),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2, "OneToOne_Optional_FK2")
                });
        }

        [ConditionalTheory(Skip = "Issue#16752")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_required_navigation_using_multiple_selects_with_string_based_Include(bool async)
        {
            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level4>()
                    .Select(l4 => l4.OneToOne_Required_FK_Inverse4)
                    .Select(l3 => l3.OneToOne_Required_FK_Inverse3)
                    .Include("OneToOne_Optional_FK2"),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2, "OneToOne_Optional_FK2")
                });
        }

        [ConditionalTheory(Skip = "Issue#16752")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_with_Include(bool async)
        {
            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>()
                    .Select(l1 => l1.OneToOne_Optional_FK1)
                    .Include(l2 => l2.OneToOne_Optional_FK2),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2, "OneToOne_Optional_FK2")
                });
        }

        [ConditionalTheory(Skip = "Issue#16752")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_with_Include_ThenInclude(bool async)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional2, "OneToMany_Optional2"),
                new ExpectedInclude<Level3>(l3 => l3.OneToOne_Optional_FK3, "OneToOne_Optional_FK3", "OneToMany_Optional2")
            };

            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>()
                    .Select(l1 => l1.OneToOne_Optional_FK1)
                    .Include(l2 => l2.OneToMany_Optional2)
                    .ThenInclude(l3 => l3.OneToOne_Optional_FK3),
                expectedIncludes);
        }

        [ConditionalTheory(Skip = "Issue#16752")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_optional_navigation_with_Include(bool async)
        {
            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>()
                    .Select(l1 => l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2)
                    .Include(l3 => l3.OneToMany_Optional3),
                ss => ss.Set<Level1>()
                    .Select(l1 => Maybe(l1.OneToOne_Optional_FK1, () => l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2)),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level3>(l3 => l3.OneToMany_Optional3, "OneToMany_Optional3")
                });
        }

        [ConditionalTheory(Skip = "Issue#16752")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_optional_navigation_with_string_based_Include(bool async)
        {
            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>()
                    .Select(l1 => l1.OneToOne_Optional_FK1)
                    .Select(l2 => l2.OneToOne_Optional_PK2)
                    .Include("OneToMany_Optional3"),
                ss => ss.Set<Level1>()
                    .Select(l1 => l1.OneToOne_Optional_FK1)
                    .Select(l2 => Maybe(l2, () => l2.OneToOne_Optional_PK2)),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level3>(l3 => l3.OneToMany_Optional3, "OneToMany_Optional3")
                });
        }

        [ConditionalTheory(Skip = "Issue#16752")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_with_order_by_and_Include(bool async)
        {
            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>()
                    .Select(l1 => l1.OneToOne_Optional_FK1)
                    .OrderBy(l2 => l2.Name)
                    .Include(l2 => l2.OneToMany_Optional2),
                ss => ss.Set<Level1>()
                    .Select(l1 => l1.OneToOne_Optional_FK1)
                    .OrderBy(l2 => Maybe(l2, () => l2.Name)),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional2, "OneToMany_Optional2")
                },
                assertOrder: true);
        }

        [ConditionalTheory(Skip = "Issue#16752")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_with_Include_and_order(bool async)
        {
            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>()
                    .Select(l1 => l1.OneToOne_Optional_FK1)
                    .Include(l2 => l2.OneToMany_Optional2)
                    .OrderBy(l2 => l2.Name),
                ss => ss.Set<Level1>()
                    .Select(l1 => l1.OneToOne_Optional_FK1)
                    .OrderBy(l2 => Maybe(l2, () => l2.Name)),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional2, "OneToMany_Optional2")
                },
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_with_order_by_and_Include(bool async)
        {
            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>()
                    .SelectMany(l1 => l1.OneToMany_Optional1)
                    .OrderBy(l2 => l2.Name)
                    .Include(l2 => l2.OneToMany_Optional2),
                ss => ss.Set<Level1>()
                    .SelectMany(l1 => l1.OneToMany_Optional1)
                    .OrderBy(l2 => Maybe(l2, () => l2.Name)),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional2, "OneToMany_Optional2")
                },
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_with_Include_and_order_by(bool async)
        {
            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>()
                    .SelectMany(l1 => l1.OneToMany_Optional1)
                    .Include(l2 => l2.OneToMany_Optional2)
                    .OrderBy(l2 => l2.Name),
                ss => ss.Set<Level1>()
                    .SelectMany(l1 => l1.OneToMany_Optional1)
                    .OrderBy(l2 => Maybe(l2, () => l2.Name)),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional2, "OneToMany_Optional2")
                },
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_with_navigation_and_explicit_DefaultIfEmpty(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from l1 in ss.Set<Level1>()
                    from l2 in l1.OneToMany_Optional1.DefaultIfEmpty()
                    where l2 != null
                    select l1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_with_navigation_and_Distinct(bool async)
        {
            return AssertIncludeQuery(
                async,
                ss => from l1 in ss.Set<Level1>().Include(l => l.OneToMany_Optional1)
                      from l2 in l1.OneToMany_Optional1.Distinct()
                      where l2 != null
                      select l1,
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level1>(l1 => l1.OneToMany_Optional1, "OneToMany_Optional1")
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_with_navigation_filter_and_explicit_DefaultIfEmpty(bool async)
        {
            return AssertQuery(
                async,
                ss => from l1 in ss.Set<Level1>()
                      from l2 in l1.OneToMany_Optional1.Where(l => l.Id > 5).DefaultIfEmpty()
                      where l2 != null
                      select l1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_with_nested_navigation_and_explicit_DefaultIfEmpty(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from l1 in ss.Set<Level1>()
                    from l3 in l1.OneToOne_Required_FK1.OneToMany_Optional2.DefaultIfEmpty()
                    where l3 != null
                    select l1,
                ss =>
                    from l1 in ss.Set<Level1>()
                    from l3 in Maybe(
                            l1.OneToOne_Required_FK1,
                            () => l1.OneToOne_Required_FK1.OneToMany_Optional2.DefaultIfEmpty())
                        ?? new List<Level3>()
                    where l3 != null
                    select l1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_with_nested_navigation_filter_and_explicit_DefaultIfEmpty(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from l1 in ss.Set<Level1>()
                    from l3 in l1.OneToOne_Optional_FK1.OneToMany_Optional2.Where(l => l.Id > 5).DefaultIfEmpty()
                    where l3 != null
                    select l1,
                ss =>
                    from l1 in ss.Set<Level1>().Where(l => l.OneToOne_Optional_FK1 != null)
                    from l3 in Maybe(
                        l1.OneToOne_Optional_FK1,
                        () => l1.OneToOne_Optional_FK1.OneToMany_Optional2.Where(l => l.Id > 5).DefaultIfEmpty())
                    where l3 != null
                    select l1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_with_nested_required_navigation_filter_and_explicit_DefaultIfEmpty(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from l1 in ss.Set<Level1>()
                    from l3 in l1.OneToOne_Required_FK1.OneToMany_Required2.Where(l => l.Id > 5).DefaultIfEmpty()
                    where l3 != null
                    select l1,
                ss =>
                    from l1 in ss.Set<Level1>().Where(l => l.OneToOne_Required_FK1 != null)
                    from l3 in Maybe(
                        l1.OneToOne_Required_FK1,
                        () => l1.OneToOne_Required_FK1.OneToMany_Required2.Where(l => l.Id > 5).DefaultIfEmpty())
                    where l3 != null
                    select l1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_with_nested_navigations_and_additional_joins_outside_of_SelectMany(bool async)
        {
            return AssertQuery(
                async,
                ss => from l1 in ss.Set<Level1>()
                      join l2 in ss.Set<Level4>().SelectMany(
                              l4 => l4.OneToOne_Required_FK_Inverse4.OneToOne_Optional_FK_Inverse3.OneToMany_Required_Self2) on l1.Id
                          equals l2.Level1_Optional_Id
                      select new { l1, l2 },
                ss => from l1 in ss.Set<Level1>()
                      join l2 in ss.Set<Level4>().SelectMany(
                              l4 => Maybe(
                                      l4.OneToOne_Required_FK_Inverse4,
                                      () => Maybe(
                                          l4.OneToOne_Required_FK_Inverse4.OneToOne_Optional_FK_Inverse3,
                                          () => l4.OneToOne_Required_FK_Inverse4.OneToOne_Optional_FK_Inverse3.OneToMany_Required_Self2))
                                  ?? new List<Level2>()) on l1.Id
                          equals l2.Level1_Optional_Id
                      select new { l1, l2 },
                elementSorter: e => (e.l1.Id, e.l2.Id),
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.l1, a.l1);
                    AssertEqual(e.l2, a.l2);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany(
            bool async)
        {
            return AssertQuery(
                async,
                ss
                    => from l1 in ss.Set<Level1>()
                       join l2 in ss.Set<Level4>().SelectMany(
                               l4 => l4.OneToOne_Required_FK_Inverse4.OneToOne_Optional_FK_Inverse3.OneToMany_Required_Self2
                                   .DefaultIfEmpty())
                           on l1.Id equals l2.Level1_Optional_Id
                       select new { l1, l2 },
                ss
                    => from l1 in ss.Set<Level1>()
                       join l2 in ss.Set<Level4>().SelectMany(
                               l4 => MaybeDefaultIfEmpty(
                                   Maybe(
                                       l4.OneToOne_Required_FK_Inverse4,
                                       () => Maybe(
                                           l4.OneToOne_Required_FK_Inverse4.OneToOne_Optional_FK_Inverse3,
                                           () => l4.OneToOne_Required_FK_Inverse4.OneToOne_Optional_FK_Inverse3
                                               .OneToMany_Required_Self2)))) on
                           l1.Id equals MaybeScalar(l2, () => l2.Level1_Optional_Id)
                       select new { l1, l2 },
                elementSorter: e => (e.l1?.Id, e.l2?.Id),
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.l1, a.l1);
                    AssertEqual(e.l2, a.l2);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany2(
            bool async)
        {
            return AssertQuery(
                async,
                ss
                    => from l2 in ss.Set<Level4>().SelectMany(
                           l4 => l4.OneToOne_Required_FK_Inverse4.OneToOne_Optional_FK_Inverse3.OneToMany_Required_Self2.DefaultIfEmpty())
                       join l1 in ss.Set<Level1>() on l2.Level1_Optional_Id equals l1.Id
                       select new { l2, l1 },
                ss
                    => from l2 in ss.Set<Level4>().SelectMany(
                           l4 => MaybeDefaultIfEmpty(
                               Maybe(
                                   l4.OneToOne_Required_FK_Inverse4,
                                   () => Maybe(
                                       l4.OneToOne_Required_FK_Inverse4.OneToOne_Optional_FK_Inverse3,
                                       () => l4.OneToOne_Required_FK_Inverse4.OneToOne_Optional_FK_Inverse3.OneToMany_Required_Self2))))
                       join l1 in ss.Set<Level1>() on MaybeScalar(l2, () => l2.Level1_Optional_Id) equals l1.Id
                       select new { l2, l1 },
                elementSorter: e => (e.l2?.Id, e.l1?.Id),
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.l2, a.l2);
                    AssertEqual(e.l1, a.l1);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany3(
            bool async)
        {
            return AssertQuery(
                async,
                ss => from l4 in ss.Set<Level1>().SelectMany(
                          l1 => l1.OneToOne_Required_FK1.OneToOne_Optional_FK2.OneToMany_Required3.DefaultIfEmpty())
                      join l2 in ss.Set<Level2>() on l4.Id equals l2.Id
                      select new { l4, l2 },
                ss => from l4 in ss.Set<Level1>().SelectMany(
                          l1 => MaybeDefaultIfEmpty(
                              Maybe(
                                  l1.OneToOne_Required_FK1,
                                  () => Maybe(
                                      l1.OneToOne_Required_FK1.OneToOne_Optional_FK2,
                                      () => l1.OneToOne_Required_FK1.OneToOne_Optional_FK2.OneToMany_Required3))))
                      join l2 in ss.Set<Level2>() on MaybeScalar<int>(l4, () => l4.Id) equals l2.Id
                      select new { l4, l2 },
                elementSorter: e => (e.l4?.Id, e.l2?.Id),
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.l4, a.l4);
                    AssertEqual(e.l2, a.l2);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany4(
            bool async)
        {
            return AssertQuery(
                async,
                ss
                    => from l4 in ss.Set<Level1>().SelectMany(
                           l1 => l1.OneToOne_Required_FK1.OneToOne_Optional_FK2.OneToMany_Required3.DefaultIfEmpty())
                       join l2 in ss.Set<Level2>() on l4.Id equals l2.Id into grouping
                       from l2 in grouping.DefaultIfEmpty()
                       select new { l4, l2 },
                ss
                    => from l4 in ss.Set<Level1>().SelectMany(
                           l1 => MaybeDefaultIfEmpty(
                               Maybe(
                                   l1.OneToOne_Required_FK1,
                                   () => Maybe(
                                       l1.OneToOne_Required_FK1.OneToOne_Optional_FK2,
                                       () => l1.OneToOne_Required_FK1.OneToOne_Optional_FK2.OneToMany_Required3))))
                       join l2 in ss.Set<Level2>() on MaybeScalar<int>(l4, () => l4.Id) equals l2.Id into grouping
                       from l2 in grouping.DefaultIfEmpty()
                       select new { l4, l2 },
                elementSorter: e => (e.l4?.Id, e.l2?.Id),
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.l4, a.l4);
                    AssertEqual(e.l2, a.l2);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_joined_together(bool async)
        {
            return AssertQuery(
                async,
                ss => from l4 in ss.Set<Level1>().SelectMany(
                          l1 => l1.OneToOne_Required_FK1.OneToOne_Optional_FK2.OneToMany_Required3.DefaultIfEmpty())
                      join l2 in ss.Set<Level4>().SelectMany(
                              l4 => l4.OneToOne_Required_FK_Inverse4.OneToOne_Optional_FK_Inverse3.OneToMany_Required_Self2
                                  .DefaultIfEmpty())
                          on l4.Id equals l2.Id
                      select new { l4, l2 },
                ss => from l4 in ss.Set<Level1>().SelectMany(
                          l1 => MaybeDefaultIfEmpty(
                              Maybe(
                                  l1.OneToOne_Required_FK1,
                                  () => Maybe(
                                      l1.OneToOne_Required_FK1.OneToOne_Optional_FK2,
                                      () => l1.OneToOne_Required_FK1.OneToOne_Optional_FK2.OneToMany_Required3))))
                      join l2 in ss.Set<Level4>().SelectMany(
                              l4 => MaybeDefaultIfEmpty(
                                  Maybe(
                                      l4.OneToOne_Required_FK_Inverse4,
                                      () => Maybe(
                                          l4.OneToOne_Required_FK_Inverse4.OneToOne_Optional_FK_Inverse3,
                                          () => l4.OneToOne_Required_FK_Inverse4.OneToOne_Optional_FK_Inverse3
                                              .OneToMany_Required_Self2))))
                          on MaybeScalar<int>(l4, () => l4.Id) equals MaybeScalar<int>(l2, () => l2.Id)
                      select new { l4, l2 },
                elementSorter: e => (e.l4?.Id, e.l2?.Id),
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.l4, a.l4);
                    AssertEqual(e.l2, a.l2);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task
            SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_followed_by_Select_required_navigation_using_same_navs(
                bool async)
        {
            return AssertQuery(
                async,
                ss => from l3 in ss.Set<Level4>().SelectMany(
                          l4 => l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToMany_Required2.DefaultIfEmpty())
                      select l3.OneToOne_Required_FK_Inverse3.OneToOne_Required_PK_Inverse2,
                ss => from l3 in ss.Set<Level4>().SelectMany(
                          l4 => MaybeDefaultIfEmpty(
                              Maybe(
                                  l4.OneToOne_Required_FK_Inverse4,
                                  () => Maybe(
                                      l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3,
                                      () => l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToMany_Required2))))
                      select Maybe(
                          l3,
                          () => l3.OneToOne_Required_FK_Inverse3.OneToOne_Required_PK_Inverse2));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task
            SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_followed_by_Select_required_navigation_using_different_navs(
                bool async)
        {
            return AssertQuery(
                async,
                ss => from l3 in ss.Set<Level1>().SelectMany(l1 => l1.OneToOne_Optional_FK1.OneToMany_Optional2.DefaultIfEmpty())
                      select l3.OneToOne_Required_FK_Inverse3.OneToOne_Required_PK_Inverse2,
                ss => from l3 in ss.Set<Level1>().SelectMany(
                          l1 => MaybeDefaultIfEmpty(
                              Maybe(
                                  l1.OneToOne_Optional_FK1,
                                  () => l1.OneToOne_Optional_FK1.OneToMany_Optional2)))
                      select Maybe(
                          l3,
                          () => l3.OneToOne_Required_FK_Inverse3.OneToOne_Required_PK_Inverse2));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task
            Complex_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_with_other_query_operators_composed_on_top(bool async)
        {
            return AssertQuery(
                async,
                ss => from l4 in ss.Set<Level1>().SelectMany(
                          l1 => l1.OneToOne_Required_FK1.OneToOne_Optional_FK2.OneToMany_Required3.DefaultIfEmpty())
                      join l2 in ss.Set<Level4>().SelectMany(
                              l4 => l4.OneToOne_Required_FK_Inverse4.OneToOne_Optional_FK_Inverse3.OneToMany_Required_Self2
                                  .DefaultIfEmpty())
                          on l4.Id equals l2.Id
                      join l3 in ss.Set<Level4>().SelectMany(
                              l4 => l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToMany_Required2.DefaultIfEmpty())
                          on l2.Id equals l3.Id into grouping
                      from l3 in grouping.DefaultIfEmpty()
                      where l4.OneToMany_Optional_Inverse4.Name != "Foo"
                      orderby l2.OneToOne_Optional_FK2.Id
                      select new
                      {
                          Entity = l4,
                          Collection = l2.OneToMany_Optional_Self2.Where(e => e.Id != 42).ToList(),
                          Property = l3.OneToOne_Optional_FK_Inverse3.OneToOne_Required_FK2.Name
                      },
                ss => from l4 in ss.Set<Level1>().SelectMany(
                          l1 => MaybeDefaultIfEmpty(
                              Maybe(
                                  l1.OneToOne_Required_FK1,
                                  () => Maybe(
                                      l1.OneToOne_Required_FK1.OneToOne_Optional_FK2,
                                      () => l1.OneToOne_Required_FK1.OneToOne_Optional_FK2.OneToMany_Required3))))
                      join l2 in ss.Set<Level4>().SelectMany(
                              l4 => MaybeDefaultIfEmpty(
                                  Maybe(
                                      l4.OneToOne_Required_FK_Inverse4,
                                      () => Maybe(
                                          l4.OneToOne_Required_FK_Inverse4.OneToOne_Optional_FK_Inverse3,
                                          () => l4.OneToOne_Required_FK_Inverse4.OneToOne_Optional_FK_Inverse3.OneToMany_Required_Self2))))
                          on MaybeScalar<int>(l4, () => l4.Id) equals MaybeScalar<int>(l2, () => l2.Id)
                      join l3 in ss.Set<Level4>().SelectMany(
                              l4 => MaybeDefaultIfEmpty(
                                  Maybe(
                                      l4.OneToOne_Required_FK_Inverse4,
                                      () => Maybe(
                                          l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3,
                                          () => l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToMany_Required2))))
                          on MaybeScalar<int>(l2, () => l2.Id) equals MaybeScalar<int>(l3, () => l3.Id) into grouping
                      from l3 in grouping.DefaultIfEmpty()
                      where Maybe(
                              l4,
                              () => Maybe(
                                  l4.OneToMany_Optional_Inverse4,
                                  () => l4.OneToMany_Optional_Inverse4.Name))
                          != "Foo"
                      orderby MaybeScalar(
                          l2,
                          () => MaybeScalar<int>(
                              l2.OneToOne_Optional_FK2,
                              () => l2.OneToOne_Optional_FK2.Id))
                      select new
                      {
                          Entity = l4,
                          Collection = Maybe(
                              l2,
                              () => Maybe(
                                  l2.OneToMany_Optional_Self2,
                                  () => l2.OneToMany_Optional_Self2.Where(e => e.Id != 42).ToList())),
                          Property = Maybe(
                              l3,
                              () => Maybe(
                                  l3.OneToOne_Optional_FK_Inverse3,
                                  () => Maybe(
                                      l3.OneToOne_Optional_FK_Inverse3.OneToOne_Required_FK2,
                                      () => l3.OneToOne_Optional_FK_Inverse3.OneToOne_Required_FK2.Name)))
                      },
                elementSorter: e => e.Entity.Id,
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.Entity, a.Entity);
                    AssertCollection(e.Collection, a.Collection);
                    Assert.Equal(e.Property, a.Property);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_SelectMany_with_navigation_and_explicit_DefaultIfEmpty(bool async)
        {
            return AssertQuery(
                async,
                ss => from l1 in ss.Set<Level1>()
                      from l2 in l1.OneToMany_Optional1
                      from l3 in l2.OneToMany_Optional2.Where(l => l.Id > 5).DefaultIfEmpty()
                      where l3 != null
                      select l1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_with_navigation_filter_paging_and_explicit_DefaultIfEmpty(bool async)
        {
            return AssertQuery(
                async,
                ss => from l1 in ss.Set<Level1>()
                      from l2 in l1.OneToMany_Required1.Where(l => l.Id > 5).OrderBy(l => l.Id).Take(3).DefaultIfEmpty()
                      where l2 != null
                      select l1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_join_subquery_containing_filter_and_distinct(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from l1 in ss.Set<Level1>()
                    join l2 in ss.Set<Level2>().Where(l => l.Id > 2).Distinct() on l1.Id equals l2.Level1_Optional_Id
                    select new { l1, l2 },
                elementSorter: e => (e.l1.Id, e.l2.Id),
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.l1, a.l1);
                    AssertEqual(e.l2, a.l2);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_join_with_key_selector_being_a_subquery(bool async)
        {
            return AssertQuery(
                async,
                ss => from l1 in ss.Set<Level1>()
                      join l2 in ss.Set<Level2>() on l1.Id equals ss.Set<Level2>().Select(l => l.Id).OrderBy(l => l).FirstOrDefault()
                      select new { l1, l2 },
                elementSorter: e => (e.l1.Id, e.l2.Id),
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.l1, a.l1);
                    AssertEqual(e.l2, a.l2);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_subquery_optional_navigation_and_constant_item(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().Where(l1 => l1.OneToOne_Optional_FK1.OneToMany_Optional2.Distinct().Select(l3 => l3.Id).Contains(1)),
                ss => ss.Set<Level1>().Where(
                    l1 => MaybeScalar<bool>(
                            l1.OneToOne_Optional_FK1,
                            () => l1.OneToOne_Optional_FK1.OneToMany_Optional2.Distinct().Select(l3 => l3.Id).Contains(1))
                        == true));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Complex_query_with_optional_navigations_and_client_side_evaluation(bool async)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(
                    async,
                    ss => ss.Set<Level1>().Where(
                        l1 => l1.Id < 3
                            && !l1.OneToMany_Optional1.Select(l2 => l2.OneToOne_Optional_FK2.OneToOne_Optional_FK3.Id)
                                .All(l4 => ClientMethod(l4))),
                    ss => ss.Set<Level1>().Where(
                        l1 => l1.Id < 3
                            && !l1.OneToMany_Optional1.Select(
                                l2 => MaybeScalar(
                                    l2.OneToOne_Optional_FK2,
                                    () => MaybeScalar<int>(
                                        l2.OneToOne_Optional_FK2.OneToOne_Optional_FK3,
                                        () => l2.OneToOne_Optional_FK2.OneToOne_Optional_FK3.Id))).All(a => true))))).Message;

            Assert.Contains("ClientMethod((Nullable<int>)", message);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Required_navigation_on_a_subquery_with_First_in_projection(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level2>()
                    .Where(l2o => l2o.Id == 7)
                    .Select(l2o => ss.Set<Level2>().OrderBy(l2i => l2i.Id).First().OneToOne_Required_FK_Inverse2.Name));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Required_navigation_on_a_subquery_with_complex_projection_and_First(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from l2o in ss.Set<Level2>()
                    where l2o.Id == 7
                    select
                        (from l2i in ss.Set<Level2>()
                         join l1i in ss.Set<Level1>()
                             on l2i.Level1_Required_Id equals l1i.Id
                         orderby l2i.Id
                         select new { Navigation = l2i.OneToOne_Required_FK_Inverse2, Constant = 7 }).First().Navigation.Name);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Required_navigation_on_a_subquery_with_First_in_predicate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level2>()
                    .Where(l2o => l2o.Id == 7)
                    .Where(
                        l1 => EF.Property<string>(ss.Set<Level2>().OrderBy(l2i => l2i.Id).First().OneToOne_Required_FK_Inverse2, "Name")
                            == "L1 02"),
                ss => ss.Set<Level2>()
                    .Where(l2o => l2o.Id == 7)
                    .Where(l1 => ss.Set<Level2>().OrderBy(l2i => l2i.Id).First().OneToOne_Required_FK_Inverse2.Name == "L1 02"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Manually_created_left_join_propagates_nullability_to_navigations(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from l1_manual in ss.Set<Level1>()
                    join l2_manual in ss.Set<Level2>() on l1_manual.Id equals l2_manual.Level1_Optional_Id into grouping
                    from l2_manual in grouping.DefaultIfEmpty()
                    where l2_manual.OneToOne_Required_FK_Inverse2.Name != "L3 02"
                    select l2_manual.OneToOne_Required_FK_Inverse2.Name,
                ss =>
                    from l1_manual in ss.Set<Level1>()
                    join l2_manual in ss.Set<Level2>() on l1_manual.Id equals l2_manual.Level1_Optional_Id into grouping
                    from l2_manual in grouping.DefaultIfEmpty()
                    where Maybe(l2_manual, () => l2_manual.OneToOne_Required_FK_Inverse2.Name) != "L3 02"
                    select Maybe(l2_manual, () => l2_manual.OneToOne_Required_FK_Inverse2.Name));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_propagates_nullability_to_manually_created_left_join1(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from l2_nav in ss.Set<Level1>().Select(ll => ll.OneToOne_Optional_FK1)
                    join l1 in ss.Set<Level2>() on l2_nav.Level1_Required_Id equals l1.Id into grouping
                    from l1 in grouping.DefaultIfEmpty()
                    select new { Id1 = (int?)l2_nav.Id, Id2 = (int?)l1.Id },
                ss =>
                    from l2_nav in ss.Set<Level1>().Select(ll => ll.OneToOne_Optional_FK1)
                    join l1 in ss.Set<Level2>() on MaybeScalar<int>(l2_nav, () => l2_nav.Level1_Required_Id) equals l1.Id into grouping
                    from l1 in grouping.DefaultIfEmpty()
                    select new { Id1 = MaybeScalar<int>(l2_nav, () => l2_nav.Id), Id2 = MaybeScalar<int>(l1, () => l1.Id) },
                elementSorter: e => e.Id1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_propagates_nullability_to_manually_created_left_join2(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from l3 in ss.Set<Level3>()
                    join l2_nav in ss.Set<Level1>().Select(ll => ll.OneToOne_Optional_FK1) on l3.Level2_Required_Id equals l2_nav.Id into
                        grouping
                    from l2_nav in grouping.DefaultIfEmpty()
                    select new { Name1 = l3.Name, Name2 = l2_nav.Name },
                ss =>
                    from l3 in ss.Set<Level3>()
                    join l2_nav in ss.Set<Level1>().Select(ll => ll.OneToOne_Optional_FK1) on l3.Level2_Required_Id equals MaybeScalar<int>(
                        l2_nav, () => l2_nav.Id) into grouping
                    from l2_nav in grouping.DefaultIfEmpty()
                    select new { Name1 = l3.Name, Name2 = Maybe(l2_nav, () => l2_nav.Name) },
                elementSorter: e => (e.Name1, e.Name2));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_reference_protection_complex(bool async)
        {
            return AssertQuery(
                async,
                ss => from l3 in ss.Set<Level3>()
                      join l2_outer in
                          from l1_inner in ss.Set<Level1>()
                          join l2_inner in ss.Set<Level2>() on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping_inner
                          from l2_inner in grouping_inner.DefaultIfEmpty()
                          select l2_inner
                          on l3.Level2_Required_Id equals l2_outer.Id into grouping_outer
                      from l2_outer in grouping_outer.DefaultIfEmpty()
                      select l2_outer.Name,
                ss => from l3 in ss.Set<Level3>()
                      join l2_outer in
                          from l1_inner in ss.Set<Level1>()
                          join l2_inner in ss.Set<Level2>() on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping_inner
                          from l2_inner in grouping_inner.DefaultIfEmpty()
                          select l2_inner
                          on l3.Level2_Required_Id equals MaybeScalar<int>(l2_outer, () => l2_outer.Id) into grouping_outer
                      from l2_outer in grouping_outer.DefaultIfEmpty()
                      select Maybe(l2_outer, () => l2_outer.Name));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_reference_protection_complex_materialization(bool async)
        {
            return AssertQuery(
                async,
                ss => from l3 in ss.Set<Level3>()
                      join l2_outer in
                          from l1_inner in ss.Set<Level1>()
                          join l2_inner in ss.Set<Level2>() on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping_inner
                          from l2_inner in grouping_inner.DefaultIfEmpty()
                          select l2_inner
                          on l3.Level2_Required_Id equals l2_outer.Id into grouping_outer
                      from l2_outer in grouping_outer.DefaultIfEmpty()
                      select new { entity = l2_outer, property = l2_outer.Name },
                ss => from l3 in ss.Set<Level3>()
                      join l2_outer in
                          from l1_inner in ss.Set<Level1>()
                          join l2_inner in ss.Set<Level2>() on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping_inner
                          from l2_inner in grouping_inner.DefaultIfEmpty()
                          select l2_inner
                          on l3.Level2_Required_Id equals MaybeScalar<int>(l2_outer, () => l2_outer.Id) into grouping_outer
                      from l2_outer in grouping_outer.DefaultIfEmpty()
                      select new { entity = l2_outer, property = Maybe(l2_outer, () => l2_outer.Name) },
                elementSorter: e => e.property,
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.entity, a.entity);
                    Assert.Equal(e.property, a.property);
                });
        }

        private static TResult ClientMethodReturnSelf<TResult>(TResult element)
        {
            return element;
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_reference_protection_complex_client_eval(bool async)
        {
            return AssertQuery(
                async,
                ss => from l3 in ss.Set<Level3>()
                      join l2_outer in
                          from l1_inner in ss.Set<Level1>()
                          join l2_inner in ss.Set<Level2>() on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping_inner
                          from l2_inner in grouping_inner.DefaultIfEmpty()
                          select l2_inner
                          on l3.Level2_Required_Id equals l2_outer.Id into grouping_outer
                      from l2_outer in grouping_outer.DefaultIfEmpty()
                      select ClientMethodReturnSelf(l2_outer.Name),
                ss => from l3 in ss.Set<Level3>()
                      join l2_outer in
                          from l1_inner in ss.Set<Level1>()
                          join l2_inner in ss.Set<Level2>() on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping_inner
                          from l2_inner in grouping_inner.DefaultIfEmpty()
                          select l2_inner
                          on l3.Level2_Required_Id equals MaybeScalar<int>(l2_outer, () => l2_outer.Id) into grouping_outer
                      from l2_outer in grouping_outer.DefaultIfEmpty()
                      select ClientMethodReturnSelf(Maybe(l2_outer, () => l2_outer.Name)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_with_complex_subquery_with_joins_does_not_get_flattened(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => from l1_outer in ss.Set<Level1>()
                      join subquery in
                          from l2_inner in ss.Set<Level2>()
                          join l1_inner in ss.Set<Level1>() on l2_inner.Level1_Required_Id equals l1_inner.Id
                          select l2_inner
                          on l1_outer.Id equals subquery.Level1_Optional_Id into grouping
                      from subquery in grouping.DefaultIfEmpty()
                      select (int?)subquery.Id,
                ss => from l1_outer in ss.Set<Level1>()
                      join subquery in
                          from l2_inner in ss.Set<Level2>()
                          join l1_inner in ss.Set<Level1>() on l2_inner.Level1_Required_Id equals l1_inner.Id
                          select l2_inner
                          on l1_outer.Id equals subquery.Level1_Optional_Id into grouping
                      from subquery in grouping.DefaultIfEmpty()
                      select MaybeScalar<int>(subquery, () => subquery.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_with_complex_subquery_with_joins_does_not_get_flattened2(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => from l1_outer in ss.Set<Level1>()
                      join subquery in
                          from l2_inner in ss.Set<Level2>()
                          join l1_inner in ss.Set<Level1>() on l2_inner.Level1_Required_Id equals l1_inner.Id
                          select l2_inner
                          on l1_outer.Id equals subquery.Level1_Optional_Id into grouping
                      from subquery in grouping.DefaultIfEmpty()
                      select subquery != null ? (int?)subquery.Id : null,
                ss => from l1_outer in ss.Set<Level1>()
                      join subquery in
                          from l2_inner in ss.Set<Level2>()
                          join l1_inner in ss.Set<Level1>() on l2_inner.Level1_Required_Id equals l1_inner.Id
                          select l2_inner
                          on l1_outer.Id equals subquery.Level1_Optional_Id into grouping
                      from subquery in grouping.DefaultIfEmpty()
                      select MaybeScalar<int>(subquery, () => subquery.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_with_complex_subquery_with_joins_does_not_get_flattened3(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => from l1_outer in ss.Set<Level1>()
                      join subquery in
                          from l2_inner in ss.Set<Level2>()
                          join l1_inner in ss.Set<Level1>() on l2_inner.Level1_Required_Id equals l1_inner.Id into grouping_inner
                          from l1_inner in grouping_inner.DefaultIfEmpty()
                          select l2_inner
                          on l1_outer.Id equals subquery.Level1_Required_Id into grouping
                      from subquery in grouping.DefaultIfEmpty()
                      select (int?)subquery.Id,
                ss => from l1_outer in ss.Set<Level1>()
                      join subquery in
                          from l2_inner in ss.Set<Level2>()
                          join l1_inner in ss.Set<Level1>() on l2_inner.Level1_Required_Id equals l1_inner.Id into grouping_inner
                          from l1_inner in grouping_inner.DefaultIfEmpty()
                          select l2_inner
                          on l1_outer.Id equals MaybeScalar<int>(subquery, () => subquery.Level1_Required_Id) into grouping
                      from subquery in grouping.DefaultIfEmpty()
                      select MaybeScalar<int>(subquery, () => subquery.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_outer(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from x in
                        (from l1 in ss.Set<Level1>()
                         join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into grouping
                         from l2 in grouping.DefaultIfEmpty()
                         orderby l1.Id
                         select l1).Take(2)
                    join l2_outer in ss.Set<Level2>() on x.Id equals l2_outer.Level1_Optional_Id into grouping_outer
                    from l2_outer in grouping_outer.DefaultIfEmpty()
                    select l2_outer.Name,
                ss =>
                    from x in
                        (from l1 in ss.Set<Level1>()
                         join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into grouping
                         from l2 in grouping.DefaultIfEmpty()
                         orderby l1.Id
                         select l1).Take(2)
                    join l2_outer in ss.Set<Level2>() on x.Id equals l2_outer.Level1_Optional_Id into grouping_outer
                    from l2_outer in grouping_outer.DefaultIfEmpty()
                    select Maybe(l2_outer, () => l2_outer.Name));
        }

        [ConditionalTheory(Skip = "Issue #17328")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_outer_with_client_method(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from x in
                        (from l1 in ss.Set<Level1>()
                         join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into grouping
                         from l2 in grouping.DefaultIfEmpty()
                         orderby l1.Id
                         select ClientLevel1(l1)).Take(2)
                    join l2_outer in ss.Set<Level2>() on x.Id equals l2_outer.Level1_Optional_Id into grouping_outer
                    from l2_outer in grouping_outer.DefaultIfEmpty()
                    select l2_outer.Name,
                ss =>
                    from x in
                        (from l1 in ss.Set<Level1>()
                         join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into grouping
                         from l2 in grouping.DefaultIfEmpty()
                         orderby l1.Id
                         select ClientLevel1(l1)).Take(2)
                    join l2_outer in ss.Set<Level2>() on x.Id equals l2_outer.Level1_Optional_Id into grouping_outer
                    from l2_outer in grouping_outer.DefaultIfEmpty()
                    select Maybe(l2_outer, () => l2_outer.Name));
        }

        private static Level1 ClientLevel1(Level1 arg)
        {
            return arg;
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_inner(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from x in
                        (from l1 in ss.Set<Level1>()
                         join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into grouping
                         from l2 in grouping.DefaultIfEmpty()
                         orderby l1.Id
                         select l2).Take(2)
                    join l1_outer in ss.Set<Level1>() on x.Level1_Optional_Id equals l1_outer.Id into grouping_outer
                    from l1_outer in grouping_outer.DefaultIfEmpty()
                    select l1_outer.Name,
                ss =>
                    from x in
                        (from l1 in ss.Set<Level1>()
                         join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into grouping
                         from l2 in grouping.DefaultIfEmpty()
                         orderby l1.Id
                         select l2).Take(2)
                    join l1_outer in ss.Set<Level1>() on MaybeScalar(x, () => x.Level1_Optional_Id) equals l1_outer.Id into grouping_outer
                    from l1_outer in grouping_outer.DefaultIfEmpty()
                    select Maybe(l1_outer, () => l1_outer.Name));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_on_a_subquery_containing_another_GroupJoin_with_orderby_on_inner_sequence_projecting_inner(
            bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from x in
                        (from l1 in ss.Set<Level1>()
                         join l2 in ss.Set<Level2>().OrderBy(ee => ee.Date) on l1.Id equals l2.Level1_Optional_Id into grouping
                         from l2 in grouping.DefaultIfEmpty()
                         orderby l1.Id
                         select l2).Take(2)
                    join l1_outer in ss.Set<Level1>() on x.Level1_Optional_Id equals l1_outer.Id into grouping_outer
                    from l1_outer in grouping_outer.DefaultIfEmpty()
                    select l1_outer.Name,
                ss =>
                    from x in
                        (from l1 in ss.Set<Level1>()
                         join l2 in ss.Set<Level2>().OrderBy(ee => ee.Date) on l1.Id equals l2.Level1_Optional_Id into grouping
                         from l2 in grouping.DefaultIfEmpty()
                         orderby l1.Id
                         select l2).Take(2)
                    join l1_outer in ss.Set<Level1>() on MaybeScalar(x, () => x.Level1_Optional_Id) equals l1_outer.Id into grouping_outer
                    from l1_outer in grouping_outer.DefaultIfEmpty()
                    select Maybe(l1_outer, () => l1_outer.Name));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_on_left_side_being_a_subquery(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().OrderBy(l1 => l1.OneToOne_Optional_FK1.Name)
                    .ThenBy(l1 => l1.Id)
                    .Take(2)
                    .Select(x => new { x.Id, Brand = x.OneToOne_Optional_FK1.Name }),
                ss => ss.Set<Level1>().OrderBy(l1 => Maybe(l1.OneToOne_Optional_FK1, () => l1.OneToOne_Optional_FK1.Name))
                    .ThenBy(l1 => l1.Id)
                    .Take(2)
                    .Select(x => new { x.Id, Brand = Maybe(x.OneToOne_Optional_FK1, () => x.OneToOne_Optional_FK1.Name) }),
                e => e.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_on_right_side_being_a_subquery(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from l2 in ss.Set<Level2>()
                    join l1 in ss.Set<Level1>().OrderBy(x => x.OneToOne_Optional_FK1.Name).Take(2) on l2.Level1_Optional_Id equals l1.Id
                        into grouping
                    from l1 in grouping.DefaultIfEmpty()
#pragma warning disable IDE0031 // Use null propagation
                    select new { l2.Id, Name = l1 != null ? l1.Name : null },
#pragma warning restore IDE0031 // Use null propagation
                ss =>
                    from l2 in ss.Set<Level2>()
                    join l1 in ss.Set<Level1>().OrderBy(x => Maybe(x.OneToOne_Optional_FK1, () => x.OneToOne_Optional_FK1.Name)).Take(2)
                        on l2.Level1_Optional_Id equals l1.Id into grouping
                    from l1 in grouping.DefaultIfEmpty()
#pragma warning disable IDE0031 // Use null propagation
                    select new { l2.Id, Name = l1 != null ? l1.Name : null },
#pragma warning restore IDE0031 // Use null propagation
                e => e.Id);
        }

        // ReSharper disable once UnusedParameter.Local
        private static bool ClientMethod(int? id)
        {
            return true;
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_in_subquery_with_client_result_operator(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from l1 in ss.Set<Level1>()
                    where (from l1_inner in ss.Set<Level1>()
                           join l2_inner in ss.Set<Level2>() on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping
                           from l2_inner in grouping.DefaultIfEmpty()
                           select l1_inner).Distinct().Count()
                        > 7
                    where l1.Id < 3
                    select l1.Name);
        }

        [ConditionalTheory(Skip = "Issue #17328")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_in_subquery_with_client_projection(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from l1 in ss.Set<Level1>()
                    where (from l1_inner in ss.Set<Level1>()
                           join l2_inner in ss.Set<Level2>() on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping
                           from l2_inner in grouping.DefaultIfEmpty()
                           select ClientStringMethod(l1_inner.Name)).Count()
                        > 7
                    where l1.Id < 3
                    select l1.Name);
        }

        [ConditionalTheory(Skip = "Issue #17328")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_in_subquery_with_client_projection_nested1(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from l1_outer in ss.Set<Level1>()
                    where (from l1_middle in ss.Set<Level1>()
                           join l2_middle in ss.Set<Level2>() on l1_middle.Id equals l2_middle.Level1_Optional_Id into grouping_middle
                           from l2_middle in grouping_middle.DefaultIfEmpty()
                           where (from l1_inner in ss.Set<Level1>()
                                  join l2_inner in ss.Set<Level2>() on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping_inner
                                  from l2_inner in grouping_inner.DefaultIfEmpty()
                                  select ClientStringMethod(l1_inner.Name)).Count()
                               > 7
                           select l1_middle).OrderBy(l1 => l1.Id).Take(10).Count()
                        > 4
                    where l1_outer.Id < 2
                    select l1_outer.Name);
        }

        [ConditionalTheory(Skip = "Issue #17328")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_in_subquery_with_client_projection_nested2(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from l1_outer in ss.Set<Level1>()
                    where (from l1_middle in ss.Set<Level1>()
                           join l2_middle in ss.Set<Level2>() on l1_middle.Id equals l2_middle.Level1_Optional_Id into grouping_middle
                           from l2_middle in grouping_middle.DefaultIfEmpty()
                           where (from l1_inner in ss.Set<Level1>()
                                  join l2_inner in ss.Set<Level2>() on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping_inner
                                  from l2_inner in grouping_inner.DefaultIfEmpty()
                                  select l1_inner.Name).Count()
                               > 7
                           select ClientStringMethod(l1_middle.Name)).Count()
                        > 4
                    where l1_outer.Id < 2
                    select l1_outer.Name);
        }

        private static string ClientStringMethod(string argument)
        {
            return argument;
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_client_method_on_outer(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from l1 in ss.Set<Level1>()
                    join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into groupJoin
                    from l2 in groupJoin.DefaultIfEmpty()
                    select new { l1.Id, client = ClientMethodNullableInt(l1.Id) },
                elementSorter: e => e.Id);
        }

        [ConditionalTheory(Skip = "Issue #17328")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_client_method_in_OrderBy(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => from l1 in ss.Set<Level1>()
                      join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into groupJoin
                      from l2 in groupJoin.DefaultIfEmpty()
                      orderby ClientMethodNullableInt(l1.Id), ClientMethodNullableInt(l2 != null ? l2.Id : (int?)null)
                      select l1.Id,
                assertOrder: true);
        }

        private static int ClientMethodNullableInt(int? id)
        {
            return id ?? 0;
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_without_DefaultIfEmpty(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => from l1 in ss.Set<Level1>()
                      join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into groupJoin
                      from l2 in groupJoin.Select(gg => gg)
                      select l1.Id);
        }

        [ConditionalTheory(Skip = "Issue #19015")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_with_subquery_on_inner(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => from l1 in ss.Set<Level1>()
                      join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into groupJoin
                      from l2 in groupJoin.Where(gg => gg.Id > 0).OrderBy(gg => gg.Id).Take(10).DefaultIfEmpty()
                      select l1.Id);
        }

        [ConditionalTheory(Skip = "Issue #19015")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_with_subquery_on_inner_and_no_DefaultIfEmpty(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => from l1 in ss.Set<Level1>()
                      join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into groupJoin
                      from l2 in groupJoin.Where(gg => gg.Id > 0).OrderBy(gg => gg.Id).Take(10)
                      select l1.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_in_subquery_with_unrelated_projection(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Level1>().Where(l1 => l1.OneToOne_Optional_FK1.Name != "Foo")
                    .OrderBy(l1 => l1.Id)
                    .Take(15)
                    .Select(l1 => l1.Id),
                ss => ss.Set<Level1>().Where(l1 => Maybe(l1.OneToOne_Optional_FK1, () => l1.OneToOne_Optional_FK1.Name) != "Foo")
                    .OrderBy(l1 => l1.Id)
                    .Take(15)
                    .Select(l1 => l1.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Explicit_GroupJoin_in_subquery_with_unrelated_projection(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => from l1 in (from l1 in ss.Set<Level1>()
                                  join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into grouping
                                  from l2 in grouping.DefaultIfEmpty()
#pragma warning disable IDE0031 // Use null propagation
                                  where (l2 != null ? l2.Name : null) != "Foo"
#pragma warning restore IDE0031 // Use null propagation
                                  select l1).OrderBy(l1 => l1.Id).Take(15)
                      select l1.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Explicit_GroupJoin_in_subquery_with_unrelated_projection2(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => from l1 in (from l1 in ss.Set<Level1>()
                                  join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into grouping
                                  from l2 in grouping.DefaultIfEmpty()
#pragma warning disable IDE0031 // Use null propagation
                                  where (l2 != null ? l2.Name : null) != "Foo"
#pragma warning restore IDE0031 // Use null propagation
                                  select l1).Distinct()
                      select l1.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Explicit_GroupJoin_in_subquery_with_unrelated_projection3(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => from l1 in (from l1 in ss.Set<Level1>()
                                  join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into grouping
                                  from l2 in grouping.DefaultIfEmpty()
#pragma warning disable IDE0031 // Use null propagation
                                  where (l2 != null ? l2.Name : null) != "Foo"
#pragma warning restore IDE0031 // Use null propagation
                                  select l1.Id).Distinct()
                      select l1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Explicit_GroupJoin_in_subquery_with_unrelated_projection4(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => from l1 in (from l1 in ss.Set<Level1>()
                                  join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into grouping
                                  from l2 in grouping.DefaultIfEmpty()
#pragma warning disable IDE0031 // Use null propagation
                                  where (l2 != null ? l2.Name : null) != "Foo"
#pragma warning restore IDE0031 // Use null propagation
                                  select l1.Id).Distinct().OrderBy(id => id).Take(20)
                      select l1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Explicit_GroupJoin_in_subquery_with_scalar_result_operator(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from l1 in ss.Set<Level1>()
                    where (from l1_inner in ss.Set<Level1>()
                           join l2 in ss.Set<Level2>() on l1_inner.Id equals l2.Level1_Optional_Id into grouping
                           from l2 in grouping.DefaultIfEmpty()
                           select l1_inner).Count()
                        > 4
                    select l1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Explicit_GroupJoin_in_subquery_with_multiple_result_operator_distinct_count_materializes_main_clause(
            bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from l1 in ss.Set<Level1>()
                    where (from l1_inner in ss.Set<Level1>()
                           join l2 in ss.Set<Level2>() on l1_inner.Id equals l2.Level1_Optional_Id into grouping
                           from l2 in grouping.DefaultIfEmpty()
                           select l1_inner).Distinct().Count()
                        > 4
                    select l1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_on_multilevel_reference_in_subquery_with_outer_projection(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level3>()
                    .Where(l3 => l3.OneToMany_Required_Inverse3.OneToOne_Required_FK_Inverse2.Name == "L1 03")
                    .OrderBy(l3 => l3.Level2_Required_Id)
                    .Skip(0)
                    .Take(10)
                    .Select(l3 => l3.Name));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_condition_optimizations_applied_correctly_when_anonymous_type_with_single_property(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from l1 in ss.Set<Level1>()
                    join l2 in ss.Set<Level2>()
                        on new { A = EF.Property<int?>(l1, "OneToMany_Optional_Self_Inverse1Id") }
                        equals new { A = EF.Property<int?>(l2, "Level1_Optional_Id") }
                    select l1,
                ss =>
                    from l1 in ss.Set<Level1>()
                    join l2 in ss.Set<Level2>()
                        on new { A = MaybeScalar<int>(l1.OneToMany_Optional_Self_Inverse1, () => l1.OneToMany_Optional_Self_Inverse1.Id) }
                        equals new { A = l2.Level1_Optional_Id }
                    select l1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_condition_optimizations_applied_correctly_when_anonymous_type_with_multiple_properties(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from l1 in ss.Set<Level1>()
                    join l2 in ss.Set<Level2>()
                        on new
                        {
                            A = EF.Property<int?>(l1, "OneToMany_Optional_Self_Inverse1Id"),
                            B = EF.Property<int?>(l1, "OneToOne_Optional_Self1Id")
                        }
                        equals new
                        {
                            A = EF.Property<int?>(l2, "Level1_Optional_Id"),
                            B = EF.Property<int?>(l2, "OneToMany_Optional_Self_Inverse2Id")
                        }
                    select l1,
                ss =>
                    from l1 in ss.Set<Level1>()
                    join l2 in ss.Set<Level2>()
                        on new
                        {
                            A = MaybeScalar<int>(l1.OneToMany_Optional_Self_Inverse1, () => l1.OneToMany_Optional_Self_Inverse1.Id),
                            B = MaybeScalar<int>(l1.OneToOne_Optional_Self1, () => l1.OneToOne_Optional_Self1.Id)
                        }
                        equals new
                        {
                            A = l2.Level1_Optional_Id,
                            B = MaybeScalar<int>(l2.OneToMany_Optional_Self_Inverse2, () => l2.OneToMany_Optional_Self_Inverse2.Id)
                        }
                    select l1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Nested_group_join_with_take(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from l1_outer in
                        (from l1_inner in ss.Set<Level1>()
                         orderby l1_inner.Id
                         join l2_inner in ss.Set<Level2>() on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping_inner
                         from l2_inner in grouping_inner.DefaultIfEmpty()
                         select l2_inner).Take(2)
                    join l2_outer in ss.Set<Level2>() on l1_outer.Id equals l2_outer.Level1_Optional_Id into grouping_outer
                    from l2_outer in grouping_outer.DefaultIfEmpty()
                    select l2_outer.Name,
                ss =>
                    from l1_outer in
                        (from l1_inner in ss.Set<Level1>()
                         orderby l1_inner.Id
                         join l2_inner in ss.Set<Level2>() on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping_inner
                         from l2_inner in grouping_inner.DefaultIfEmpty()
                         select l2_inner).Take(2)
                    join l2_outer in ss.Set<Level2>() on MaybeScalar<int>(l1_outer, () => l1_outer.Id) equals l2_outer.Level1_Optional_Id
                        into
                        grouping_outer
                    from l2_outer in grouping_outer.DefaultIfEmpty()
                    select Maybe(l2_outer, () => l2_outer.Name));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_with_same_navigation_compared_to_null(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => from l2 in ss.Set<Level2>()
                      where l2.OneToMany_Required_Inverse2.Name != "L1 07" && l2.OneToMany_Required_Inverse2 != null
                      select l2.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multi_level_navigation_compared_to_null(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => from l3 in ss.Set<Level3>()
                      where l3.OneToMany_Optional_Inverse3.OneToOne_Required_FK_Inverse2 != null
                      select l3.Id,
                ss => from l3 in ss.Set<Level3>()
                      where Maybe(l3.OneToMany_Optional_Inverse3, () => l3.OneToMany_Optional_Inverse3.OneToOne_Required_FK_Inverse2)
                          != null
                      select l3.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multi_level_navigation_with_same_navigation_compared_to_null(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => from l3 in ss.Set<Level3>()
                      where l3.OneToMany_Optional_Inverse3.OneToOne_Required_FK_Inverse2.Name != "L1 07"
                      where l3.OneToMany_Optional_Inverse3.OneToOne_Required_FK_Inverse2 != null
                      select l3.Id,
                ss => from l3 in ss.Set<Level3>()
                      where Maybe(
                              l3.OneToMany_Optional_Inverse3,
                              () => Maybe(
                                  l3.OneToMany_Optional_Inverse3.OneToOne_Required_FK_Inverse2,
                                  () => l3.OneToMany_Optional_Inverse3.OneToOne_Required_FK_Inverse2.Name))
                          != "L1 07"
                      where Maybe(l3.OneToMany_Optional_Inverse3, () => l3.OneToMany_Optional_Inverse3.OneToOne_Required_FK_Inverse2)
                          != null
                      select l3.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigations_compared_to_each_other1(bool async)
        {
            return AssertQuery(
                async,
                ss => from l2 in ss.Set<Level2>()
                      where l2.OneToMany_Required_Inverse2 == l2.OneToMany_Required_Inverse2
                      select l2.Name);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigations_compared_to_each_other2(bool async)
        {
            return AssertQuery(
                async,
                ss => from l2 in ss.Set<Level2>()
                      where l2.OneToMany_Required_Inverse2 == l2.OneToOne_Optional_PK_Inverse2
                      select l2.Name);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigations_compared_to_each_other3(bool async)
        {
            return AssertQuery(
                async,
                ss => from l2 in ss.Set<Level2>()
                      where l2.OneToMany_Optional2.Select(i => i.OneToOne_Optional_PK_Inverse3 == l2).Any()
                      select l2.Name);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigations_compared_to_each_other4(bool async)
        {
            return AssertQuery(
                async,
                ss => from l2 in ss.Set<Level2>()
                      where l2.OneToOne_Required_FK2.OneToMany_Optional3
                          .Select(i => i.OneToOne_Optional_PK_Inverse4 == l2.OneToOne_Required_FK2).Any()
                      select l2.Name,
                ss => from l2 in ss.Set<Level2>()
                      where MaybeScalar(
                              l2.OneToOne_Required_FK2,
                              () => MaybeScalar<bool>(
                                  l2.OneToOne_Required_FK2.OneToMany_Optional3,
                                  () => l2.OneToOne_Required_FK2.OneToMany_Optional3
                                      .Select(i => i.OneToOne_Optional_PK_Inverse4 == l2.OneToOne_Required_FK2).Any()))
                          == true
                      select l2.Name);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigations_compared_to_each_other5(bool async)
        {
            return AssertQuery(
                async,
                ss => from l2 in ss.Set<Level2>()
                      where l2.OneToOne_Required_FK2.OneToMany_Optional3
                          .Select(i => i.OneToOne_Optional_PK_Inverse4 == l2.OneToOne_Optional_PK2).Any()
                      select l2.Name,
                ss => from l2 in ss.Set<Level2>()
                      where MaybeScalar(
                              l2.OneToOne_Required_FK2,
                              () => MaybeScalar<bool>(
                                  l2.OneToOne_Required_FK2.OneToMany_Optional3,
                                  () => l2.OneToOne_Required_FK2.OneToMany_Optional3
                                      .Select(i => i.OneToOne_Optional_PK_Inverse4 == l2.OneToOne_Optional_PK2).Any()))
                          == true
                      select l2.Name);
        }

        [ConditionalTheory(Skip = "Issue#16752")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Level4_Include(bool async)
        {
            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>().Select(l1 => l1.OneToOne_Required_PK1)
                    .Where(t => t != null)
                    .Select(l2 => l2.OneToOne_Required_PK2)
                    .Where(t => t != null)
                    .Select(l3 => l3.OneToOne_Required_PK3)
                    .Where(t => t != null)
                    .Select(l4 => l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3)
                    .Include(l2 => l2.OneToOne_Optional_FK2),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2, "OneToOne_Optional_FK2")
                },
                elementSorter: e => e.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Comparing_collection_navigation_on_optional_reference_to_null(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Level1>().Where(l1 => l1.OneToOne_Optional_FK1.OneToMany_Optional2 == null).Select(l1 => l1.Id),
                ss => ss.Set<Level1>()
                    .Where(l1 => Maybe(l1.OneToOne_Optional_FK1, () => l1.OneToOne_Optional_FK1.OneToMany_Optional2) == null)
                    .Select(l1 => l1.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_with_client_eval_and_navigation1(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level2>().Select(l2 => ss.Set<Level2>().OrderBy(l => l.Id).First().OneToOne_Required_FK_Inverse2.Name));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_with_client_eval_and_navigation2(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Level2>().Select(
                    l2 => ss.Set<Level2>().OrderBy(l => l.Id).First().OneToOne_Required_FK_Inverse2.Name == "L1 02"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_with_client_eval_and_multi_level_navigation(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level3>().Select(
                    l3 => ss.Set<Level3>().OrderBy(l => l.Id).First().OneToOne_Required_FK_Inverse3.OneToOne_Required_FK_Inverse2.Name));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Member_doesnt_get_pushed_down_into_subquery_with_result_operator(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from l1 in ss.Set<Level1>()
                    where l1.Id < 3
                    select (from l3 in ss.Set<Level3>()
                            orderby l3.Id
                            select l3).Distinct().OrderBy(l => l.Id).Skip(1).FirstOrDefault().Name);
        }

        [ConditionalTheory(Skip = "issue #8523")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Subquery_with_Distinct_Skip_FirstOrDefault_without_OrderBy(bool async)
        {
            return AssertQuery(
                async,
                ss => from l1 in ss.Set<Level1>()
                      where l1.Id < 3
                      select (from l3 in ss.Set<Level3>()
                              orderby l3.Id
                              select l3).Distinct().Skip(1).FirstOrDefault().Name);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_collection_navigation(bool async)
        {
            return AssertQuery(
                async,
                ss => from l1 in ss.Set<Level1>()
                      select l1.OneToMany_Optional1,
                elementSorter: e => e != null ? e.Count : 0,
                elementAsserter: (e, a) => AssertCollection(e, a));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_collection_navigation_nested(bool async)
        {
            return AssertQuery(
                async,
                ss => from l1 in ss.Set<Level1>()
                      select l1.OneToOne_Optional_FK1.OneToMany_Optional2,
                ss => from l1 in ss.Set<Level1>()
                      select Maybe(l1.OneToOne_Optional_FK1, () => l1.OneToOne_Optional_FK1.OneToMany_Optional2) ?? new List<Level3>(),
                elementSorter: e => e.Count,
                elementAsserter: (e, a) => AssertCollection(e, a));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_collection_navigation_nested_with_take(bool async)
        {
            return AssertQuery(
                async,
                ss => from l1 in ss.Set<Level1>()
                      select l1.OneToOne_Optional_FK1.OneToMany_Optional2.Take(50),
                ss => from l1 in ss.Set<Level1>()
                      select Maybe(l1.OneToOne_Optional_FK1, () => l1.OneToOne_Optional_FK1.OneToMany_Optional2.Take(50))
                          ?? new List<Level3>(),
                elementSorter: e => e?.Count() ?? 0,
                elementAsserter: (e, a) => AssertCollection(e, a));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_collection_navigation_using_ef_property(bool async)
        {
            return AssertQuery(
                async,
                ss => from l1 in ss.Set<Level1>()
                      select EF.Property<ICollection<Level3>>(
                          EF.Property<Level2>(
                              l1,
                              "OneToOne_Optional_FK1"),
                          "OneToMany_Optional2"),
                ss => from l1 in ss.Set<Level1>()
                      select Maybe(l1.OneToOne_Optional_FK1, () => l1.OneToOne_Optional_FK1.OneToMany_Optional2) ?? new List<Level3>(),
                elementSorter: e => e.Count,
                elementAsserter: (e, a) => AssertCollection(e, a));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_collection_navigation_nested_anonymous(bool async)
        {
            return AssertQuery(
                async,
                ss => from l1 in ss.Set<Level1>()
                      select new { l1.Id, l1.OneToOne_Optional_FK1.OneToMany_Optional2 },
                ss => from l1 in ss.Set<Level1>()
                      select new
                      {
                          l1.Id,
                          OneToMany_Optional2 = Maybe(
                                  l1.OneToOne_Optional_FK1,
                                  () => l1.OneToOne_Optional_FK1.OneToMany_Optional2)
                              ?? new List<Level3>()
                      },
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    AssertCollection(e.OneToMany_Optional2, a.OneToMany_Optional2);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_collection_navigation_count(bool async)
        {
            return AssertQuery(
                async,
                ss => from l1 in ss.Set<Level1>()
                      select new { l1.Id, l1.OneToOne_Optional_FK1.OneToMany_Optional2.Count },
                ss => from l1 in ss.Set<Level1>()
                      select new
                      {
                          l1.Id,
                          Count = MaybeScalar(
                                  l1.OneToOne_Optional_FK1,
                                  () => MaybeScalar<int>(
                                      l1.OneToOne_Optional_FK1.OneToMany_Optional2,
                                      () => l1.OneToOne_Optional_FK1.OneToMany_Optional2.Count))
                              ?? 0
                      },
                elementSorter: e => e.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_collection_navigation_composed(bool async)
        {
            return AssertQuery(
                async,
                ss => from l1 in ss.Set<Level1>()
                      where l1.Id < 3
                      select new { l1.Id, collection = l1.OneToMany_Optional1.Where(l2 => l2.Name != "Foo").ToList() },
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    AssertCollection(e.collection, a.collection);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_collection_and_root_entity(bool async)
        {
            return AssertQuery(
                async,
                ss => from l1 in ss.Set<Level1>()
                      select new { l1, l1.OneToMany_Optional1 },
                elementSorter: e => e.l1.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.l1.Id, a.l1.Id);
                    AssertCollection(e.OneToMany_Optional1, a.OneToMany_Optional1);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_collection_and_include(bool async)
        {
            return AssertQuery(
                async,
                ss => from l1 in ss.Set<Level1>().Include(l => l.OneToMany_Optional1)
                      select new { l1, l1.OneToMany_Optional1 },
                elementSorter: e => e.l1.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.l1.Id, a.l1.Id);
                    AssertCollection(e.OneToMany_Optional1, a.OneToMany_Optional1);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_navigation_and_collection(bool async)
        {
            return AssertQuery(
                async,
                ss => from l1 in ss.Set<Level1>()
                      select new { l1.OneToOne_Optional_FK1, l1.OneToOne_Optional_FK1.OneToMany_Optional2 },
                ss => from l1 in ss.Set<Level1>()
                      select new
                      {
                          l1.OneToOne_Optional_FK1,
                          OneToMany_Optional2 = Maybe(l1.OneToOne_Optional_FK1, () => l1.OneToOne_Optional_FK1.OneToMany_Optional2)
                              ?? new List<Level3>()
                      },
                elementSorter: e => e.OneToOne_Optional_FK1?.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.OneToOne_Optional_FK1?.Id, a.OneToOne_Optional_FK1?.Id);
                    AssertCollection(e.OneToMany_Optional2, a.OneToMany_Optional2);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_inside_subquery(bool async)
        {
            // can't use AssertIncludeQuery here, see #18191
            return AssertQuery(
                async,
                ss => ss.Set<Level1>()
                    .Where(l1 => l1.Id < 3)
                    .OrderBy(l1 => l1.Id)
                    .Select(l1 => new { subquery = ss.Set<Level2>().Include(l => l.OneToMany_Optional2).Where(l => l.Id > 0).ToList() }),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e.subquery, a.subquery));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_optional_navigation_property_string_concat(bool async)
        {
            return AssertQuery(
                async,
                ss => from l1 in ss.Set<Level1>()
                      from l2 in l1.OneToMany_Optional1.Where(l => l.Id > 5).OrderByDescending(l => l.Name).DefaultIfEmpty()
                      select l1.Name + " " + (l2 != null ? l2.Name : "NULL"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_multiple_orderbys_member(bool async)
        {
            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level2>()
                    .Include(l2 => l2.OneToMany_Optional2)
                    .OrderBy(l2 => l2.Name)
                    .ThenBy(l2 => l2.Level1_Required_Id),
                new List<IExpectedInclude> { new ExpectedInclude<Level2>(e => e.OneToMany_Optional2, "OneToMany_Optional2") },
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_multiple_orderbys_property(bool async)
        {
            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level2>()
                    .Include(l2 => l2.OneToMany_Optional2)
                    .OrderBy(l2 => EF.Property<int>(l2, "Level1_Required_Id"))
                    .ThenBy(l2 => l2.Name),
                ss => ss.Set<Level2>()
                    .OrderBy(l2 => l2.Level1_Required_Id)
                    .ThenBy(l2 => l2.Name),
                new List<IExpectedInclude> { new ExpectedInclude<Level2>(e => e.OneToMany_Optional2, "OneToMany_Optional2") },
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_multiple_orderbys_methodcall(bool async)
        {
            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level2>()
                    .Include(l2 => l2.OneToMany_Optional2)
                    .OrderBy(l2 => Math.Abs(l2.Level1_Required_Id))
                    .ThenBy(l2 => l2.Name),
                new List<IExpectedInclude> { new ExpectedInclude<Level2>(e => e.OneToMany_Optional2, "OneToMany_Optional2") },
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_multiple_orderbys_complex(bool async)
        {
            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level2>()
                    .Include(l2 => l2.OneToMany_Optional2)
                    .OrderBy(l2 => Math.Abs(l2.Level1_Required_Id) + 7)
                    .ThenBy(l2 => l2.Name),
                new List<IExpectedInclude> { new ExpectedInclude<Level2>(e => e.OneToMany_Optional2, "OneToMany_Optional2") },
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_multiple_orderbys_complex_repeated(bool async)
        {
            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level2>()
                    .Include(l2 => l2.OneToMany_Optional2)
                    .OrderBy(l2 => -l2.Level1_Required_Id)
                    .ThenBy(l2 => -l2.Level1_Required_Id).ThenBy(l2 => l2.Name),
                new List<IExpectedInclude> { new ExpectedInclude<Level2>(e => e.OneToMany_Optional2, "OneToMany_Optional2") },
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Entries_for_detached_entities_are_removed()
        {
            using var context = CreateContext();
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
            var entity = Fixture.QueryAsserter.SetSourceCreator(context).Set<Level2>().OrderBy(l2 => l2.Id).First();
            var entry = context.ChangeTracker.Entries().Single();
            Assert.Same(entity, entry.Entity);

            entry.State = EntityState.Detached;

            Assert.Empty(context.ChangeTracker.Entries());

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        [ConditionalTheory(Skip = "Issue#12088")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_reference_with_groupby_in_subquery(bool async)
        {
            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>()
                    .Include(l1 => l1.OneToOne_Optional_FK1)
                    .GroupBy(g => g.Name)
                    .Select(g => g.OrderBy(e => e.Id).FirstOrDefault()),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level1>(e => e.OneToOne_Optional_FK1, "OneToOne_Optional_FK1")
                });
        }

        [ConditionalTheory(Skip = "Issue#12088")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_groupby_in_subquery(bool async)
        {
            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>()
                    .Include(l1 => l1.OneToMany_Optional1)
                    .GroupBy(g => g.Name)
                    .Select(g => g.OrderBy(e => e.Id).FirstOrDefault()),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level1>(e => e.OneToMany_Optional1, "OneToMany_Optional1")
                });
        }

        [ConditionalTheory(Skip = "Issue#12088")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multi_include_with_groupby_in_subquery(bool async)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(e => e.OneToOne_Optional_FK1, "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level2>(e => e.OneToMany_Optional2, "OneToMany_Optional2", "OneToOne_Optional_FK1")
            };

            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>()
                    .Include(l1 => l1.OneToOne_Optional_FK1.OneToMany_Optional2)
                    .GroupBy(g => g.Name)
                    .Select(g => g.OrderBy(e => e.Id).FirstOrDefault()),
                expectedIncludes);
        }

        [ConditionalTheory(Skip = "Issue#12088")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_groupby_in_subquery_and_filter_before_groupby(bool async)
        {
            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>()
                    .Include(l1 => l1.OneToMany_Optional1)
                    .Where(l1 => l1.Id > 3)
                    .GroupBy(g => g.Name)
                    .Select(g => g.OrderBy(e => e.Id).FirstOrDefault()),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level1>(e => e.OneToMany_Optional1, "OneToMany_Optional1")
                });
        }

        [ConditionalTheory(Skip = "Issue#12088")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_groupby_in_subquery_and_filter_after_groupby(bool async)
        {
            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>()
                    .Include(l1 => l1.OneToMany_Optional1)
                    .GroupBy(g => g.Name)
                    .Where(g => g.Key != "Foo")
                    .Select(g => g.OrderBy(e => e.Id).FirstOrDefault()),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level1>(e => e.OneToMany_Optional1, "OneToMany_Optional1")
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_include_multiple_derived_navigation_with_same_name_and_same_type(bool async)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<InheritanceDerived1>(e => e.ReferenceSameType, "ReferenceSameType"),
                new ExpectedInclude<InheritanceDerived2>(e => e.ReferenceSameType, "ReferenceSameType")
            };

            return AssertIncludeQuery(
                async,
                ss => ss.Set<InheritanceBase1>().Include("ReferenceSameType"),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_include_multiple_derived_navigation_with_same_name_and_different_type(bool async)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<InheritanceDerived1>(e => e.ReferenceDifferentType, "ReferenceDifferentType"),
                new ExpectedInclude<InheritanceDerived2>(e => e.ReferenceDifferentType, "ReferenceDifferentType")
            };

            return AssertIncludeQuery(
                async,
                ss => ss.Set<InheritanceBase1>().Include("ReferenceDifferentType"),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task
            String_include_multiple_derived_navigation_with_same_name_and_different_type_nested_also_includes_partially_matching_navigation_chains(
                bool async)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<InheritanceDerived1>(e => e.ReferenceDifferentType, "ReferenceDifferentType"),
                new ExpectedInclude<InheritanceDerived2>(e => e.ReferenceDifferentType, "ReferenceDifferentType"),
                new ExpectedInclude<InheritanceLeaf2>(e => e.BaseCollection, "BaseCollection", "ReferenceDifferentType")
            };

            return AssertIncludeQuery(
                async,
                ss => ss.Set<InheritanceBase1>().Include("ReferenceDifferentType.BaseCollection"),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_include_multiple_derived_collection_navigation_with_same_name_and_same_type(bool async)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<InheritanceDerived1>(e => e.CollectionSameType, "CollectionSameType"),
                new ExpectedInclude<InheritanceDerived2>(e => e.CollectionSameType, "CollectionSameType")
            };

            return AssertIncludeQuery(
                async,
                ss => ss.Set<InheritanceBase1>().Include("CollectionSameType"),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_include_multiple_derived_collection_navigation_with_same_name_and_different_type(bool async)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<InheritanceDerived1>(e => e.CollectionDifferentType, "CollectionDifferentType"),
                new ExpectedInclude<InheritanceDerived2>(e => e.CollectionDifferentType, "CollectionDifferentType")
            };

            return AssertIncludeQuery(
                async,
                ss => ss.Set<InheritanceBase1>().Include("CollectionDifferentType"),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task
            String_include_multiple_derived_collection_navigation_with_same_name_and_different_type_nested_also_includes_partially_matching_navigation_chains(
                bool async)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<InheritanceDerived1>(e => e.CollectionDifferentType, "CollectionDifferentType"),
                new ExpectedInclude<InheritanceDerived2>(e => e.CollectionDifferentType, "CollectionDifferentType"),
                new ExpectedInclude<InheritanceLeaf2>(e => e.BaseCollection, "BaseCollection", "CollectionDifferentType")
            };

            return AssertIncludeQuery(
                async,
                ss => ss.Set<InheritanceBase1>().Include("CollectionDifferentType.BaseCollection"),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_include_multiple_derived_navigations_complex(bool async)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<InheritanceBase2>(e => e.Reference, "Reference"),
                new ExpectedInclude<InheritanceDerived1>(e => e.CollectionDifferentType, "CollectionDifferentType", "Reference"),
                new ExpectedInclude<InheritanceDerived2>(e => e.CollectionDifferentType, "CollectionDifferentType", "Reference"),
                new ExpectedInclude<InheritanceBase2>(e => e.Collection, "Collection"),
                new ExpectedInclude<InheritanceDerived1>(e => e.ReferenceSameType, "ReferenceSameType", "Collection"),
                new ExpectedInclude<InheritanceDerived2>(e => e.ReferenceSameType, "ReferenceSameType", "Collection")
            };

            return AssertIncludeQuery(
                async,
                ss => ss.Set<InheritanceBase2>().Include("Reference.CollectionDifferentType").Include("Collection.ReferenceSameType"),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_reference_collection_order_by_reference_navigation(bool async)
        {
            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>()
                    .Include(l1 => l1.OneToOne_Optional_FK1.OneToMany_Optional2)
                    .OrderBy(l1 => (int?)l1.OneToOne_Optional_FK1.Id),
                ss => ss.Set<Level1>()
                    .Include(l1 => l1.OneToOne_Optional_FK1.OneToMany_Optional2)
                    .OrderBy(l1 => MaybeScalar<int>(l1.OneToOne_Optional_FK1, () => l1.OneToOne_Optional_FK1.Id)),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level1>(e => e.OneToOne_Optional_FK1, "OneToOne_Optional_FK1"),
                    new ExpectedInclude<Level2>(e => e.OneToMany_Optional2, "OneToMany_Optional2", "OneToOne_Optional_FK1")
                },
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Nav_rewrite_doesnt_apply_null_protection_for_function_arguments(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Level1>().Where(l1 => l1.OneToOne_Optional_PK1 != null)
                    .Select(l1 => Math.Max(l1.OneToOne_Optional_PK1.Level1_Required_Id, 7)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Accessing_optional_property_inside_result_operator_subquery(bool async)
        {
            var names = new[] { "Name1", "Name2" };

            return AssertQuery(
                async,
                ss => ss.Set<Level1>().Where(l1 => names.All(n => l1.OneToOne_Optional_FK1.Name != n)),
                ss => ss.Set<Level1>().Where(
                    l1 => names.All(n => Maybe(l1.OneToOne_Optional_FK1, () => l1.OneToOne_Optional_FK1.Name) != n)));
        }

        [ConditionalTheory(Skip = "Issue#16752")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_after_SelectMany_and_reference_navigation(bool async)
        {
            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>().SelectMany(l1 => l1.OneToMany_Required1).Select(l2 => l2.OneToOne_Optional_FK2)
                    .Include(l3 => l3.OneToMany_Optional3),
                new List<IExpectedInclude> { new ExpectedInclude<Level3>(l3 => l3.OneToMany_Optional3, "OneToMany_Optional3") });
        }

        [ConditionalTheory(Skip = "Issue#16752")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_after_multiple_SelectMany_and_reference_navigation(bool async)
        {
            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>().SelectMany(l1 => l1.OneToMany_Required1).SelectMany(l2 => l2.OneToMany_Optional2)
                    .Select(l3 => l3.OneToOne_Required_FK3).Include(l4 => l4.OneToMany_Required_Self4),
                new List<IExpectedInclude> { new ExpectedInclude<Level4>(l4 => l4.OneToMany_Required_Self4, "OneToMany_Required_Self4") });
        }

        [ConditionalTheory(Skip = "Issue#16752")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_after_SelectMany_and_multiple_reference_navigations(bool async)
        {
            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>().SelectMany(l1 => l1.OneToMany_Required1).Select(l2 => l2.OneToOne_Optional_FK2)
                    .Select(l3 => l3.OneToOne_Required_FK3).Include(l4 => l4.OneToMany_Optional_Self4),
                ss => ss.Set<Level1>().SelectMany(l1 => l1.OneToMany_Required1).Select(l2 => l2.OneToOne_Optional_FK2)
                    .Select(l3 => Maybe(l3, () => l3.OneToOne_Required_FK3)),
                new List<IExpectedInclude> { new ExpectedInclude<Level4>(l4 => l4.OneToMany_Optional_Self4, "OneToMany_Optional_Self4") });
        }

        [ConditionalTheory(Skip = "Issue#16752")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_after_SelectMany_and_reference_navigation_with_another_SelectMany_with_Distinct(bool async)
        {
            return AssertIncludeQuery(
                async,
                ss => from lOuter in ss.Set<Level1>().SelectMany(l1 => l1.OneToMany_Required1).Select(l2 => l2.OneToOne_Optional_FK2)
                          .Include(l3 => l3.OneToMany_Optional3)
                      from lInner in lOuter.OneToMany_Optional3.Distinct()
                      where lInner != null
                      select lOuter,
                ss => from lOuter in ss.Set<Level1>().SelectMany(l1 => l1.OneToMany_Required1).Select(l2 => l2.OneToOne_Optional_FK2)
                      where lOuter != null
                      from lInner in lOuter.OneToMany_Optional3.Distinct()
                      where lInner != null
                      select lOuter,
                new List<IExpectedInclude> { new ExpectedInclude<Level3>(l3 => l3.OneToMany_Optional3, "OneToMany_Optional") });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_subquery_with_custom_projection(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().OrderBy(l1 => l1.Id).SelectMany(
                    l1 => l1.OneToMany_Optional1.Select(
                        l2 => new { l2.Name })).Take(1));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_check_in_anonymous_type_projection_should_not_be_removed(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().OrderBy(l1 => l1.Id).Select(
                    l1 => new
                    {
                        Level2s = l1.OneToMany_Optional1.Select(
                            l2 => new
                            {
                                Level3 = l2.OneToOne_Required_FK2 == null
                                    ? null
                                    : new { l2.OneToOne_Required_FK2.Name }
                            }).ToList()
                    }),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(
                    e.Level2s,
                    a.Level2s,
                    elementSorter: ee => ee?.Level3.Name));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_check_in_Dto_projection_should_not_be_removed(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().OrderBy(l1 => l1.Id).Select(
                    l1 => new
                    {
                        Level2s = l1.OneToMany_Optional1.Select(
                            l2 => new
                            {
                                Level3 = l2.OneToOne_Required_FK2 == null
                                    ? null
                                    : new ProjectedDto<string> { Value = l2.OneToOne_Required_FK2.Name }
                            }).ToList()
                    }),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(
                    e.Level2s,
                    a.Level2s,
                    elementSorter: ee => ee.Level3?.Value,
                    elementAsserter: (ee, aa) => Assert.Equal(ee.Level3?.Value, aa.Level3?.Value)));
        }

        private class ProjectedDto<T>
        {
            public T Value { get; set; }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_navigation_property_followed_by_select_collection_navigation(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().SelectMany(l1 => l1.OneToMany_Optional1).Select(l2 => new { l2.Id, l2.OneToMany_Optional2 }),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    AssertCollection(e.OneToMany_Optional2, a.OneToMany_Optional2);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_SelectMany_navigation_property_followed_by_select_collection_navigation(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().SelectMany(l1 => l1.OneToMany_Optional1).SelectMany(l2 => l2.OneToMany_Optional2)
                    .Select(l2 => new { l2.Id, l2.OneToMany_Optional3 }),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    AssertCollection(e.OneToMany_Optional3, a.OneToMany_Optional3);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_navigation_property_with_include_and_followed_by_select_collection_navigation(bool async)
        {
            // can't use AssertIncludeQuery here, see #18191
            return AssertQuery(
                async,
                ss => ss.Set<Level1>()
                    .SelectMany(l1 => l1.OneToMany_Optional1)
                    .Include(l2 => l2.OneToMany_Required2)
                    .Select(l2 => new { l2, l2.OneToMany_Optional2 }),
                elementSorter: e => e.l2.Id,
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.l2, a.l2);
                    AssertCollection(e.l2.OneToMany_Required2, a.l2.OneToMany_Required2);
                    AssertCollection(e.OneToMany_Optional2, a.OneToMany_Optional2);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include1(bool async)
        {
            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>().Include(l1 => l1.OneToOne_Optional_FK1),
                new List<IExpectedInclude> { new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1, "OneToOne_Optional_FK1") });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include2(bool async)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1, "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1, "OneToOne_Optional_FK1")
            };

            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>().Include(l1 => l1.OneToOne_Optional_FK1).Include(l1 => l1.OneToOne_Optional_FK1),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include3(bool async)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1, "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_PK1, "OneToOne_Optional_PK1")
            };

            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>().Include(l1 => l1.OneToOne_Optional_FK1).Include(l1 => l1.OneToOne_Optional_PK1),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include4(bool async)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1, "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_PK2, "OneToOne_Optional_PK2", "OneToOne_Optional_FK1")
            };

            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>().Include(l1 => l1.OneToOne_Optional_FK1).ThenInclude(l1 => l1.OneToOne_Optional_PK2),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include5(bool async)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1, "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_PK2, "OneToOne_Optional_PK2", "OneToOne_Optional_FK1")
            };

            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>().Include(l1 => l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include6(bool async)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_PK2, "OneToOne_Optional_PK2")
            };

            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>().Include(l1 => l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2).Select(l1 => l1.OneToOne_Optional_FK1),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include7(bool async)
        {
            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>().Include(l1 => l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2).Select(l1 => l1.OneToOne_Optional_PK1),
                new List<IExpectedInclude>());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include8(bool async)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK_Inverse2, "OneToOne_Optional_FK_Inverse2")
            };

            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level2>().Where(l2 => l2.OneToOne_Optional_FK_Inverse2.Name != "Fubar")
                    .Include(l2 => l2.OneToOne_Optional_FK_Inverse2),
                ss => ss.Set<Level2>().Where(
                        l2 => Maybe(l2.OneToOne_Optional_FK_Inverse2, () => l2.OneToOne_Optional_FK_Inverse2.Name) != "Fubar")
                    .Include(l2 => l2.OneToOne_Optional_FK_Inverse2),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include9(bool async)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK_Inverse2, "OneToOne_Optional_FK_Inverse2")
            };

            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level2>().Include(l2 => l2.OneToOne_Optional_FK_Inverse2)
                    .Where(l2 => l2.OneToOne_Optional_FK_Inverse2.Name != "Fubar"),
                ss => ss.Set<Level2>().Include(l2 => l2.OneToOne_Optional_FK_Inverse2).Where(
                    l2 => Maybe(l2.OneToOne_Optional_FK_Inverse2, () => l2.OneToOne_Optional_FK_Inverse2.Name) != "Fubar"),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include10(bool async)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1, "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_PK2, "OneToOne_Optional_PK2", "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_PK1, "OneToOne_Optional_PK1"),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2, "OneToOne_Optional_FK2", "OneToOne_Optional_PK1"),
                new ExpectedInclude<Level3>(
                    l3 => l3.OneToOne_Optional_PK3, "OneToOne_Optional_PK3", "OneToOne_Optional_FK1.OneToOne_Optional_FK2")
            };

            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>()
                    .Include(l1 => l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2)
                    .Include(l1 => l1.OneToOne_Optional_PK1.OneToOne_Optional_FK2.OneToOne_Optional_PK3),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include11(bool async)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1, "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2, "OneToOne_Optional_FK2", "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_PK2, "OneToOne_Optional_PK2", "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_PK1, "OneToOne_Optional_PK1"),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2, "OneToOne_Optional_FK2", "OneToOne_Optional_PK1"),
                new ExpectedInclude<Level3>(
                    l3 => l3.OneToOne_Optional_FK3, "OneToOne_Optional_FK3", "OneToOne_Optional_PK1.OneToOne_Optional_FK2"),
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_PK1, "OneToOne_Optional_PK1"),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2, "OneToOne_Optional_FK2", "OneToOne_Optional_PK1"),
                new ExpectedInclude<Level3>(
                    l3 => l3.OneToOne_Optional_PK3, "OneToOne_Optional_PK3", "OneToOne_Optional_PK1.OneToOne_Optional_FK2"),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_PK2, "OneToOne_Optional_PK2", "OneToOne_Optional_PK1")
            };

            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>()
                    .Include(l1 => l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2)
                    .Include(l1 => l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2)
                    .Include(l1 => l1.OneToOne_Optional_PK1.OneToOne_Optional_FK2.OneToOne_Optional_FK3)
                    .Include(l1 => l1.OneToOne_Optional_PK1.OneToOne_Optional_FK2.OneToOne_Optional_PK3)
                    .Include(l1 => l1.OneToOne_Optional_PK1.OneToOne_Optional_PK2),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include12(bool async)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2, "OneToOne_Optional_FK2")
            };

            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>()
                    .Include(l1 => l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2)
                    .Select(l1 => l1.OneToOne_Optional_FK1),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include13(bool async)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1, "OneToOne_Optional_FK1")
            };

            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>()
                    .Include(l1 => l1.OneToOne_Optional_FK1)
                    .Select(l1 => new { one = l1, two = l1 }),
                expectedIncludes,
                clientProjections: new List<Func<dynamic, object>> { x => x.one, x => x.two },
                elementSorter: e => e.one.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include14(bool async)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1, "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2, "OneToOne_Optional_FK2", "OneToOne_Optional_FK1")
            };

            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>()
                    .Include(l1 => l1.OneToOne_Optional_FK1).ThenInclude(l2 => l2.OneToOne_Optional_FK2)
                    .Select(
                        l1 => new
                        {
                            one = l1,
                            two = l1.OneToOne_Optional_FK1,
                            three = l1.OneToOne_Optional_PK1
                        }),
                expectedIncludes,
                clientProjections: new List<Func<dynamic, object>>
                {
                    x => x.one
                    // issue #15368
                    //x => x.two,
                },
                elementSorter: e => e.one.Id);
        }

        [ConditionalFact(Skip = "Issue#16752")]
        public virtual void Include15()
        {
            using var ctx = CreateContext();
            var query = ctx.LevelOne.Select(l1 => new { foo = l1.OneToOne_Optional_FK1, bar = l1.OneToOne_Optional_PK1 })
                .Include(x => x.foo.OneToOne_Optional_FK2).Include(x => x.bar.OneToMany_Optional2);

            var result = query.ToList();
        }

        [ConditionalFact(Skip = "Issue#16752")]
        public virtual void Include16()
        {
            using var ctx = CreateContext();
            var query = ctx.LevelOne.Select(l1 => new { foo = l1.OneToOne_Optional_FK1, bar = l1.OneToOne_Optional_PK1 }).Distinct()
                .Include(x => x.foo.OneToOne_Optional_FK2).Include(x => x.bar.OneToMany_Optional2);

            var result = query.ToList();
        }

        [ConditionalFact(Skip = "Issue#16752")]
        public virtual void Include17()
        {
            using var ctx = CreateContext();
            var query = ctx.LevelOne.Select(l1 => new { foo = l1.OneToOne_Optional_FK1, bar = l1.OneToOne_Optional_PK1 })
                .Include(x => x.foo.OneToOne_Optional_FK2).Distinct();

            var result = query.ToList();
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include18_1(bool async)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1, "OneToOne_Optional_FK1")
            };

            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>().Include(x => x.OneToOne_Optional_FK1).Distinct(),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include18_1_1(bool async)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1, "OneToOne_Optional_FK1")
            };

            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>().OrderBy(x => x.OneToOne_Required_FK1.Name).Include(x => x.OneToOne_Optional_FK1).Take(10),
                ss => ss.Set<Level1>().OrderBy(x => Maybe(x.OneToOne_Required_FK1, () => x.OneToOne_Required_FK1.Name))
                    .Include(x => x.OneToOne_Optional_FK1).Take(10),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include18_2(bool async)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1, "OneToOne_Optional_FK1")
            };

            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>().Where(x => x.OneToOne_Required_FK1.Name != "Foo").Include(x => x.OneToOne_Optional_FK1).Distinct(),
                ss => ss.Set<Level1>().Where(x => Maybe(x.OneToOne_Required_FK1, () => x.OneToOne_Required_FK1.Name) != "Foo").Distinct(),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void Include18_3()
        {
            using var ctx = CreateContext();
            var query = ctx.LevelOne.OrderBy(x => x.OneToOne_Required_FK1.Name).Include(x => x.OneToOne_Optional_FK1)
                .Select(l1 => new { foo = l1, bar = l1 }).Take(10);

            var result = query.ToList();
        }

        [ConditionalFact]
        public virtual void Include18_3_1()
        {
            using var ctx = CreateContext();
            var query = ctx.LevelOne.OrderBy(x => x.OneToOne_Required_FK1.Name).Include(x => x.OneToOne_Optional_FK1)
                .Select(l1 => new { foo = l1, bar = l1 }).Take(10).Select(x => new { x.foo, x.bar });

            var result = query.ToList();
        }

        [ConditionalFact]
        public virtual void Include18_3_2()
        {
            using var ctx = CreateContext();
            var query = ctx.LevelOne.OrderBy(x => x.OneToOne_Required_FK1.Name).Include(x => x.OneToOne_Optional_FK1)
                .Select(l1 => new { outer_foo = new { inner_foo = l1, inner_bar = l1.Name }, outer_bar = l1 }).Take(10);

            var result = query.ToList();
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include18_3_3(bool async)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level2>(l1 => l1.OneToOne_Optional_FK2, "OneToOne_Optional_FK2")
            };

            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>().Include(x => x.OneToOne_Optional_FK1.OneToOne_Optional_FK2).Select(l1 => l1.OneToOne_Optional_FK1)
                    .Distinct(),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void Include18_4()
        {
            using var ctx = CreateContext();
            var query = ctx.LevelOne.Include(x => x.OneToOne_Optional_FK1).Select(l1 => new { foo = l1, bar = l1 }).Distinct();

            var result = query.ToList();
        }

        [ConditionalFact]
        public virtual void Include18()
        {
            using var ctx = CreateContext();
            var query = ctx.LevelOne.Include(x => x.OneToOne_Optional_FK1)
                .Select(l1 => new { foo = l1, bar = l1.OneToOne_Optional_PK1 }).OrderBy(x => x.foo.Id).Take(10);

            var result = query.ToList();
        }

        [ConditionalFact]
        public virtual void Include19()
        {
            using var ctx = CreateContext();
            var query = ctx.LevelOne.Include(x => x.OneToOne_Optional_FK1)
                .Select(l1 => new { foo = l1.OneToOne_Optional_FK1, bar = l1.OneToOne_Optional_PK1 }).Distinct();

            var result = query.ToList();
        }

        [ConditionalFact]
        public virtual void IncludeCollection1()
        {
            using var ctx = CreateContext();
            var query = ctx.LevelOne.Include(l1 => l1.OneToMany_Optional1);
            var result = query.ToList();
        }

        [ConditionalFact]
        public virtual void IncludeCollection2()
        {
            using var ctx = CreateContext();
            var query = ctx.LevelOne.Include(l1 => l1.OneToMany_Optional1).ThenInclude(l2 => l2.OneToOne_Optional_PK2);
            var result = query.ToList();
        }

        [ConditionalFact]
        public virtual void IncludeCollection3()
        {
            using var ctx = CreateContext();
            var query = ctx.LevelOne.Include(l1 => l1.OneToOne_Optional_FK1).ThenInclude(l2 => l2.OneToMany_Optional2);
            var result = query.ToList();
        }

        [ConditionalFact]
        public virtual void IncludeCollection4()
        {
            using var ctx = CreateContext();
            var query = ctx.LevelOne.Include(l1 => l1.OneToMany_Optional1).Select(l1 => l1.OneToMany_Optional1);
            var result = query.ToList();
        }

        [ConditionalFact]
        public virtual void IncludeCollection5()
        {
            using var ctx = CreateContext();
            var query = ctx.LevelOne.Include(l1 => l1.OneToMany_Optional1).ThenInclude(l2 => l2.OneToOne_Optional_PK2)
                .Select(l1 => l1.OneToMany_Optional1);
            var result = query.ToList();
        }

        [ConditionalFact]
        public virtual void IncludeCollection6()
        {
            using var ctx = CreateContext();
            var query = ctx.LevelOne.Include(l1 => l1.OneToMany_Optional1).ThenInclude(l2 => l2.OneToOne_Optional_PK2)
                .ThenInclude(l3 => l3.OneToOne_Optional_FK3)
                .Select(l1 => l1.OneToMany_Optional1);
            var result = query.ToList();
        }

        [ConditionalFact]
        public virtual void IncludeCollection6_1()
        {
            using var ctx = CreateContext();
            var query = ctx.LevelOne.Include(l1 => l1.OneToMany_Optional1).ThenInclude(l2 => l2.OneToOne_Optional_PK2)
                .ThenInclude(l3 => l3.OneToOne_Optional_FK3);
            var result = query.ToList();
        }

        [ConditionalFact]
        public virtual void IncludeCollection6_2()
        {
            using var ctx = CreateContext();
            var query = ctx.LevelOne.Include(l1 => l1.OneToMany_Optional1).ThenInclude(l2 => l2.OneToOne_Optional_PK2)
                .ThenInclude(l3 => l3.OneToOne_Optional_FK3)
                .Include(l1 => l1.OneToMany_Optional1).ThenInclude(l2 => l2.OneToOne_Optional_FK2)
                .ThenInclude(l3 => l3.OneToMany_Optional3)
                .Select(l1 => l1.OneToMany_Optional1);
            var result = query.ToList();
        }

        [ConditionalFact]
        public virtual void IncludeCollection6_3()
        {
            using var ctx = CreateContext();
            var query = ctx.LevelOne.Include(l1 => l1.OneToMany_Optional1).ThenInclude(l2 => l2.OneToOne_Optional_PK2)
                .ThenInclude(l3 => l3.OneToOne_Optional_FK3)
                .Include(l1 => l1.OneToMany_Optional1).ThenInclude(l2 => l2.OneToOne_Optional_FK2)
                .ThenInclude(l3 => l3.OneToMany_Optional3);

            var result = query.ToList();
        }

        [ConditionalFact]
        public virtual void IncludeCollection6_4()
        {
            using var ctx = CreateContext();
            var query = ctx.LevelOne.Include(l1 => l1.OneToMany_Optional1).ThenInclude(l2 => l2.OneToOne_Optional_PK2)
                .ThenInclude(l3 => l3.OneToOne_Optional_FK3)
                .Select(l1 => l1.OneToMany_Optional1.Select(l2 => l2.OneToOne_Optional_PK2));

            var result = query.ToList();
        }

        [ConditionalFact]
        public virtual void IncludeCollection7()
        {
            using var ctx = CreateContext();
            var query = ctx.LevelOne.Include(l1 => l1.OneToMany_Optional1).ThenInclude(l2 => l2.OneToOne_Optional_PK2)
                .Select(l1 => new { l1, l1.OneToMany_Optional1 });
            var result = query.ToList();
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task IncludeCollection8(bool async)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(e => e.OneToMany_Optional1, "OneToMany_Optional1"),
                new ExpectedInclude<Level2>(e => e.OneToOne_Optional_PK2, "OneToOne_Optional_PK2", "OneToMany_Optional1"),
                new ExpectedInclude<Level3>(
                    e => e.OneToOne_Optional_FK3, "OneToOne_Optional_FK3", "OneToMany_Optional1.OneToOne_Optional_PK2")
            };

            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>()
                    .Include(l1 => l1.OneToMany_Optional1)
                    .ThenInclude(l2 => l2.OneToOne_Optional_PK2)
                    .ThenInclude(l3 => l3.OneToOne_Optional_FK3)
                    .Where(l1 => l1.OneToMany_Optional1.Where(l2 => l2.OneToOne_Optional_PK2.Name != "Foo").Count() > 0),
                ss => ss.Set<Level1>()
                    .Include(l1 => l1.OneToMany_Optional1)
                    .ThenInclude(l2 => l2.OneToOne_Optional_PK2)
                    .ThenInclude(l3 => l3.OneToOne_Optional_FK3)
                    .Where(
                        l1 => l1.OneToMany_Optional1
                                .Where(l2 => Maybe(l2.OneToOne_Optional_PK2, () => l2.OneToOne_Optional_PK2.Name) != "Foo").Count()
                            > 0),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_with_all_method_include_gets_ignored(bool async)
        {
            return AssertAll(
                async,
                ss => ss.Set<Level1>().Include(l1 => l1.OneToOne_Optional_FK1).Include(l1 => l1.OneToMany_Optional1),
                predicate: l1 => l1.Name != "Foo");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_with_navigations_in_the_result_selector1(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().Join(
                    ss.Set<Level2>(), l1 => l1.Id, l2 => l2.Level1_Required_Id, (o, i) => new { o.OneToOne_Optional_FK1, i }));
        }

        [ConditionalFact]
        public virtual void Join_with_navigations_in_the_result_selector2()
        {
            using var ctx = CreateContext();
            var query = ctx.LevelOne.Join(
                ctx.LevelTwo, l1 => l1.Id, l2 => l2.Level1_Required_Id,
                (o, i) => new { o.OneToOne_Optional_FK1, i.OneToMany_Optional2 });
            var result = query.ToList();
        }

        [ConditionalFact(Skip = "issue #12200")]
        public virtual void GroupJoin_with_navigations_in_the_result_selector()
        {
            using var ctx = CreateContext();
            var query = ctx.LevelOne.GroupJoin(
                ctx.LevelTwo, l1 => l1.Id, l2 => l2.Level1_Required_Id, (o, i) => new { o.OneToOne_Optional_FK1, i });
            var result = query.ToList();
        }

        [ConditionalFact]
        public virtual void Member_pushdown_chain_3_levels_deep()
        {
            using var ctx = CreateContext();
            var query = from l1 in ctx.LevelOne
                        orderby l1.Id
                        where (from l2 in ctx.LevelTwo
                               orderby l2.Id
                               where l2.Level1_Optional_Id == l1.Id
                               select (from l3 in ctx.LevelThree
                                       orderby l3.Id
                                       where l3.Level2_Required_Id == l2.Id
                                       select (from l4 in ctx.LevelFour
                                               where l4.Level3_Required_Id == l3.Id
                                               orderby l4.Id
                                               select l4).FirstOrDefault()).FirstOrDefault()).FirstOrDefault().Name
                            != "Foo"
                        select l1;

            var result = query.ToList();
        }

        [ConditionalFact]
        public virtual void Member_pushdown_chain_3_levels_deep_entity()
        {
            using var ctx = CreateContext();
            var query = from l1 in ctx.LevelOne
                        orderby l1.Id
                        select (from l2 in ctx.LevelTwo
                                orderby l2.Id
                                where l2.Level1_Optional_Id == l1.Id
                                select (from l3 in ctx.LevelThree
                                        orderby l3.Id
                                        where l3.Level2_Required_Id == l2.Id
                                        select (from l4 in ctx.LevelFour
                                                where l4.Level3_Required_Id == l3.Id
                                                orderby l4.Id
                                                select l4).FirstOrDefault()).FirstOrDefault()).FirstOrDefault();

            var result = query.ToList();
        }

        [ConditionalFact]
        public virtual void Member_pushdown_with_collection_navigation_in_the_middle()
        {
            using var ctx = CreateContext();
            var query = from l1 in ctx.LevelOne
                        orderby l1.Id
                        select (from l2 in ctx.LevelTwo
                                orderby l2.Id
                                where l2.Level1_Required_Id == l1.Id
                                select l2.OneToMany_Optional2.Select(
                                    l3 => (from l4 in ctx.LevelFour
                                           where l4.Level3_Required_Id == l3.Id
                                           orderby l4.Id
                                           select l4).FirstOrDefault()).FirstOrDefault()).FirstOrDefault().Name;

            var result = query.ToList();
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Member_pushdown_with_multiple_collections(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().Select(
                    l1 => l1.OneToMany_Optional1.OrderBy(l2 => l2.Id).FirstOrDefault().OneToMany_Optional2.OrderBy(l3 => l3.Id)
                        .FirstOrDefault().Name),
                ss => ss.Set<Level1>().Select(
                    l1 => Maybe(
                        l1.OneToMany_Optional1.OrderBy(l2 => l2.Id).FirstOrDefault(),
                        () => Maybe(
                            l1.OneToMany_Optional1.OrderBy(l2 => MaybeScalar<int>(l2, () => l2.Id)).FirstOrDefault().OneToMany_Optional2
                                .OrderBy(l3 => l3.Id).FirstOrDefault(),
                            () => l1.OneToMany_Optional1.OrderBy(l2 => MaybeScalar<int>(l2, () => l2.Id)).FirstOrDefault()
                                .OneToMany_Optional2.OrderBy(l3 => l3.Id).FirstOrDefault().Name))));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_multiple_collections_on_same_level(bool async)
        {
            return AssertIncludeQuery(
                async,
                ss => ss.Set<Level1>().Include(l1 => l1.OneToMany_Optional1).Include(l1 => l1.OneToMany_Required1),
                new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level1>(l1 => l1.OneToMany_Optional1, "OneToMany_Optional1"),
                    new ExpectedInclude<Level1>(l1 => l1.OneToMany_Required1, "OneToMany_Required1")
                },
                assertOrder: true);
        }

        [ConditionalTheory(Skip = "Issue#17020")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_check_removal_applied_recursively(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().Where(
                    l1 =>
                        (((l1.OneToOne_Optional_FK1 == null
                                    ? null
                                    : l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2)
                                == null
                                    ? null
                                    : l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2.OneToOne_Optional_FK3)
                            == null
                                ? null
                                : l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2.OneToOne_Optional_FK3.Name)
                        == "L4 01"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_check_different_structure_does_not_remove_null_checks(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().Where(
                    l1 =>
                        (l1.OneToOne_Optional_FK1 == null
                            ? null
                            : l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2 == null
                                ? null
                                : l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2.OneToOne_Optional_FK3 == null
                                    ? null
                                    : l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2.OneToOne_Optional_FK3.Name)
                        == "L4 01"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Union_over_entities_with_different_nullability(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Level1>()
                    .GroupJoin(ss.Set<Level2>(), l1 => l1.Id, l2 => l2.Level1_Optional_Id, (l1, l2s) => new { l1, l2s })
                    .SelectMany(g => g.l2s.DefaultIfEmpty(), (g, l2) => new { g.l1, l2 })
                    .Concat(ss.Set<Level2>().GroupJoin(ss.Set<Level1>(), l2 => l2.Level1_Optional_Id, l1 => l1.Id, (l2, l1s) => new { l2, l1s })
                        .SelectMany(g => g.l1s.DefaultIfEmpty(), (g, l1) => new { l1, g.l2 })
                        .Where(e => e.l1.Equals(null)))
                    .Select(e => e.l1.Id),
                ss => ss.Set<Level1>()
                    .GroupJoin(ss.Set<Level2>(), l1 => l1.Id, l2 => l2.Level1_Optional_Id, (l1, l2s) => new { l1, l2s })
                    .SelectMany(g => g.l2s.DefaultIfEmpty(), (g, l2) => new { g.l1, l2 })
                    .Concat(ss.Set<Level2>().GroupJoin(ss.Set<Level1>(), l2 => l2.Level1_Optional_Id, l1 => l1.Id, (l2, l1s) => new { l2, l1s })
                        .SelectMany(g => g.l1s.DefaultIfEmpty(), (g, l1) => new { l1, g.l2 })
                        .Where(e => e.l1 == null))
                    .Select(e => MaybeScalar<int>(Maybe<Level1>(e, () => e.l1), () => e.l1.Id) ?? 0));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Lift_projection_mapping_when_pushing_down_subquery(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>()
                    .Take(25)
                    .Select(
                        l1 => new
                        {
                            l1.Id,
                            c1 = l1.OneToMany_Required1.Select(l2 => new { l2.Id }).FirstOrDefault(),
                            c2 = l1.OneToMany_Required1.Select(l2 => new { l2.Id })
                        }),
                elementSorter: t => t.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal(e.c1?.Id, a.c1?.Id);
                    AssertCollection(e.c2, a.c2, elementSorter: i => i.Id, elementAsserter: (ie, ia) => Assert.Equal(ie.Id, ia.Id));
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Including_reference_navigation_and_projecting_collection_navigation(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>()
                    .Include(e => e.OneToOne_Required_FK1)
                        .ThenInclude(e => e.OneToOne_Optional_FK2)
                    .Select(e => new Level1
                    {
                        Id = e.Id,
                        OneToOne_Required_FK1 = e.OneToOne_Required_FK1,
                        OneToMany_Required1 = e.OneToMany_Required1
                    }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Including_reference_navigation_and_projecting_collection_navigation_2(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>()
                    .Include(e => e.OneToOne_Required_FK1)
                    .Include(e => e.OneToMany_Required1)
                    .Select(e => new
                    {
                        e,
                        First = e.OneToMany_Required1.OrderByDescending(e => e.Id).FirstOrDefault()
                    }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_collection_count_ThenBy_reference_navigation(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>()
                    .OrderBy(l1 => l1.OneToOne_Required_FK1.OneToMany_Required2.Count())
                    .ThenBy(l1 => l1.OneToOne_Required_FK1.OneToOne_Required_FK2.Name),
                ss => ss.Set<Level1>()
                    .OrderBy(l1 => MaybeScalar<int>(Maybe(l1.OneToOne_Required_FK1, () => l1.OneToOne_Required_FK1.OneToMany_Required2),
                        () => l1.OneToOne_Required_FK1.OneToMany_Required2.Count()) ?? 0)
                    .ThenBy(l1 => Maybe(Maybe(l1.OneToOne_Required_FK1, () => l1.OneToOne_Required_FK1.OneToOne_Required_FK2),
                        () => l1.OneToOne_Required_FK1.OneToOne_Required_FK2.Name)),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_conditional_is_not_applied_explicitly_for_optional_navigation(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().Where(l1 => l1.OneToOne_Optional_FK1 != null && l1.OneToOne_Optional_FK1.Name == "L2 01"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task LeftJoin_with_Any_on_outer_source_and_projecting_collection_from_inner(bool async)
        {
            var validIds = new List<string> { "L1 01", "L1 02" };

            return AssertQuery(
                async,
                ss => from l1 in ss.Set<Level1>().Where(l1 => validIds.Any(e => e == l1.Name))
                      join l2 in ss.Set<Level2>()
                          on l1.Id equals l2.Level1_Required_Id into l2s
                      from l2 in l2s.DefaultIfEmpty()
                      select new Level2
                      {
                          Id = l2 == null ? 0 : l2.Id,
                          OneToMany_Required2 = l2 == null ? null : l2.OneToMany_Required2
                      });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_with_selector_cast_using_as(bool async)
        {
            return AssertSum(
                async,
                ss => ss.Set<Level1>().Select(s => s.Id as int?));
        }

        [ConditionalTheory(Skip = "Issue#12657")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_with_filter_with_include_selector_cast_using_as(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().Where(l1 => l1.Id > l1.OneToMany_Optional1.Select(l2 => l2.Id as int?).Sum()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_with_joined_where_clause_cast_using_as(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().Where(w => w.Id == w.OneToOne_Optional_FK1.Id as int?),
                ss => ss.Set<Level1>()
                    .Where(w => w.Id == (MaybeScalar<int>(w.OneToOne_Optional_FK1, () => w.OneToOne_Optional_FK1.Id) as int?)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_single_nested_subquery(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().OrderBy(l1 => l1.Id).Select(l1 => new
                {
                    Level2 = l1.OneToMany_Optional1.OrderBy(l2 => l2.Id).Select(l2 => new
                    {
                        Level3s = l2.OneToMany_Optional2.OrderBy(l3 => l3.Id).Select(l3 => new { l3.Id }).ToList()
                    }).FirstOrDefault()
                }),
                assertOrder: true,
                elementAsserter: (e, a) =>
                {
                    if (e.Level2 == null)
                    {
                        Assert.Null(a.Level2);
                    }
                    else
                    {
                        AssertCollection(e.Level2.Level3s, a.Level2.Level3s, ordered: true);
                    }
                });

        }
        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_single_nested_subquery2(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level1>().OrderBy(l1 => l1.Id).Select(l1 => new
                {
                    Level2s = l1.OneToMany_Optional1.OrderBy(l2 => l2.Id).Select(l2 => new
                    {
                        Level3 = l2.OneToMany_Optional2.OrderBy(l3 => l3.Id).Select(l3 => new
                        {
                            Level4s = l3.OneToMany_Optional3.OrderBy(l4 => l4.Id).Select(l4 => new { l4.Id }).ToList()
                        }).FirstOrDefault()
                    })
                }),
                assertOrder: true,
                elementAsserter: (e, a) =>
                {
                    AssertCollection(e.Level2s, a.Level2s, ordered: true, elementAsserter:
                        (e2, a2) =>
                        {
                            if (e2.Level3 == null)
                            {
                                Assert.Null(a2.Level3);
                            }
                            else
                            {
                                AssertCollection(e2.Level3.Level4s, a2.Level3.Level4s, ordered: true);
                            }
                        });
                });
        }
    }
}
