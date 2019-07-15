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
        public virtual Task Entity_equality_empty(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => l1s.Where(l => l.OneToOne_Optional_FK1 == new Level2()),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Key_equality_when_sentinel_ef_property(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => l1s.Where(l => EF.Property<int>(l.OneToOne_Optional_FK1, "Id") == 0),
                l1s => l1s.Where(l => MaybeScalar<int>(l.OneToOne_Optional_FK1, () => l.OneToOne_Optional_FK1.Id) == 0),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Key_equality_using_property_method_required(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => l1s.Where(l => EF.Property<int>(l.OneToOne_Required_FK1, "Id") > 7),
                l1s => l1s.Where(l => MaybeScalar<int>(l.OneToOne_Required_FK1, () => l.OneToOne_Required_FK1.Id) > 7),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Key_equality_using_property_method_required2(bool isAsync)
        {
            return AssertQuery<Level2>(
                isAsync,
                l2s => l2s.Where(l => EF.Property<int>(l.OneToOne_Required_FK_Inverse2, "Id") > 7),
                l2s => l2s.Where(l => l.OneToOne_Required_FK_Inverse2.Id > 7),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Key_equality_using_property_method_nested(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => l1s.Where(l => EF.Property<int>(EF.Property<Level2>(l, "OneToOne_Required_FK1"), "Id") == 7),
                l1s => l1s.Where(l => MaybeScalar<int>(l.OneToOne_Required_FK1, () => l.OneToOne_Required_FK1.Id) == 7),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Key_equality_using_property_method_nested2(bool isAsync)
        {
            return AssertQuery<Level2>(
                isAsync,
                l2s => l2s.Where(l => EF.Property<int>(EF.Property<Level1>(l, "OneToOne_Required_FK_Inverse2"), "Id") == 7),
                l2s => l2s.Where(l => l.OneToOne_Required_FK_Inverse2.Id == 7),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Key_equality_using_property_method_and_member_expression1(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => l1s.Where(l => EF.Property<Level2>(l, "OneToOne_Required_FK1").Id == 7),
                l1s => l1s.Where(l => MaybeScalar<int>(l.OneToOne_Required_FK1, () => l.OneToOne_Required_FK1.Id) == 7),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Key_equality_using_property_method_and_member_expression2(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => l1s.Where(l => EF.Property<int>(l.OneToOne_Required_FK1, "Id") == 7),
                l1s => l1s.Where(l => MaybeScalar<int>(l.OneToOne_Required_FK1, () => l.OneToOne_Required_FK1.Id) == 7),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Key_equality_using_property_method_and_member_expression3(bool isAsync)
        {
            return AssertQuery<Level2>(
                isAsync,
                l2s => l2s.Where(l => EF.Property<int>(l.OneToOne_Required_FK_Inverse2, "Id") == 7),
                l2s => l2s.Where(l => l.OneToOne_Required_FK_Inverse2.Id == 7),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Key_equality_navigation_converted_to_FK(bool isAsync)
        {
            // TODO: remove this? it is testing optimization that is no longer there
            return AssertQuery<Level2>(
                isAsync,
                l2s => l2s.Where(
                    l => l.OneToOne_Required_FK_Inverse2 == new Level1
                    {
                        Id = 1
                    }),
                l2s => l2s.Where(l => l.OneToOne_Required_FK_Inverse2.Id == 1),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Key_equality_two_conditions_on_same_navigation(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => l1s.Where(
                    l => l.OneToOne_Required_FK1 == new Level2
                         {
                             Id = 1
                         }
                         || l.OneToOne_Required_FK1 == new Level2
                         {
                             Id = 2
                         }),
                l1s => l1s.Where(
                    l => MaybeScalar<int>(l.OneToOne_Required_FK1, () => l.OneToOne_Required_FK1.Id) == 1
                         || MaybeScalar<int>(l.OneToOne_Required_FK1, () => l.OneToOne_Required_FK1.Id) == 2),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Key_equality_two_conditions_on_same_navigation2(bool isAsync)
        {
            return AssertQuery<Level2>(
                isAsync,
                l2s => l2s.Where(
                    l => l.OneToOne_Required_FK_Inverse2 == new Level1
                         {
                             Id = 1
                         }
                         || l.OneToOne_Required_FK_Inverse2 == new Level1
                         {
                             Id = 2
                         }),
                l2s => l2s.Where(
                    l => l.OneToOne_Required_FK_Inverse2.Id == 1
                         || l.OneToOne_Required_FK_Inverse2.Id == 2),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Data_reader_is_closed_correct_number_of_times_for_include_queries_on_optional_navigations()
        {
            using (var context = CreateContext())
            {
                // reader for the last include is not opened because there is no data one level below - we should only try to close connection as many times as we tried to open it
                // if we close it too many times, consecutive query will not work
                // see issue #1457 for more details
                context.LevelOne.Include(e => e.OneToMany_Optional1).ThenInclude(e => e.OneToMany_Optional2)
                    .ThenInclude(e => e.OneToMany_Optional_Inverse3.OneToMany_Optional2).ToList();

                context.LevelOne.ToList();
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multi_level_include_one_to_many_optional_and_one_to_many_optional_produces_valid_sql(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToMany_Optional1, "OneToMany_Optional1"),
                new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional2, "OneToMany_Optional2", navigationPath: "OneToMany_Optional1")
            };

            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s.Include(e => e.OneToMany_Optional1).ThenInclude(e => e.OneToMany_Optional2),
                expectedIncludes,
                elementSorter: e => e.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multi_level_include_correct_PK_is_chosen_as_the_join_predicate_for_queries_that_join_same_table_multiple_times(bool isAsync)
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

            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s.Include(e => e.OneToMany_Optional1).ThenInclude(e => e.OneToMany_Optional2)
                    .ThenInclude(e => e.OneToMany_Required_Inverse3.OneToMany_Optional2),
                expectedIncludes,
                elementSorter: e => e.Id);
        }

        [ConditionalFact]
        public virtual void Multi_level_include_reads_key_values_from_data_reader_rather_than_incorrect_reader_deep_into_the_stack()
        {
            using (var context = CreateContext())
            {
                // #1433
                context.LevelOne.Include(e => e.OneToMany_Optional1).ToList();
                context.LevelOne.Include(e => e.OneToMany_Optional_Self1).ToList();

                // #1478
                context.LevelOne
                    .Include(e => e.OneToMany_Optional1)
                    .ThenInclude(e => e.OneToMany_Optional_Inverse2.OneToMany_Optional_Self_Inverse1.OneToOne_Optional_FK1).ToList();

                context.LevelOne
                    .Include(e => e.OneToMany_Optional1)
                    .ThenInclude(e => e.OneToMany_Optional_Inverse2.OneToMany_Optional_Self_Inverse1.OneToOne_Optional_PK1).ToList();

                // #1487
                context.LevelOne
                    .Include(e => e.OneToMany_Optional1)
                    .ThenInclude(e => e.OneToMany_Optional_Inverse2.OneToOne_Optional_PK1.OneToOne_Optional_FK2).ToList();

                context.LevelOne
                    .Include(e => e.OneToMany_Optional1)
                    .ThenInclude(e => e.OneToMany_Optional_Inverse2.OneToOne_Optional_PK1.OneToOne_Optional_FK_Inverse2).ToList();

                // #1488
                context.LevelOne
                    .Include(e => e.OneToMany_Optional1)
                    .ThenInclude(e => e.OneToMany_Optional_Inverse2.OneToOne_Optional_PK1.OneToOne_Required_FK2).ToList();

                context.LevelOne
                    .Include(e => e.OneToMany_Optional1)
                    .ThenInclude(e => e.OneToMany_Optional_Inverse2.OneToOne_Optional_PK1.OneToOne_Required_FK_Inverse2).ToList();
            }
        }

        [ConditionalFact]
        public virtual void Multi_level_include_with_short_circuiting()
        {
            using (var context = CreateContext())
            {
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
                Assert.Equal(1, globalizations_1_placeholder.Count);
                Assert.Equal("Globalization9", globalizations_1_placeholder[0].Text);
                Assert.Equal("Language9", globalizations_1_placeholder[0].Language.Name);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_navigation_key_access_optional(bool isAsync)
        {
            return AssertQuery<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from e1 in l1s
                    join e2 in l2s on e1.Id equals e2.OneToOne_Optional_FK_Inverse2.Id
                    select new
                    {
                        Id1 = e1.Id,
                        Id2 = e2.Id
                    },
                (l1s, l2s) =>
                    from e1 in l1s
                    join e2 in l2s on e1.Id equals MaybeScalar<int>(
                        e2.OneToOne_Optional_FK_Inverse2,
                        () => e2.OneToOne_Optional_FK_Inverse2.Id)
                    select new
                    {
                        Id1 = e1.Id,
                        Id2 = e2.Id
                    },
                e => e.Id1 + " " + e.Id2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_navigation_key_access_required(bool isAsync)
        {
            return AssertQuery<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from e1 in l1s
                    join e2 in l2s on e1.Id equals e2.OneToOne_Required_FK_Inverse2.Id
                    select new
                    {
                        Id1 = e1.Id,
                        Id2 = e2.Id
                    },
                (l1s, l2s) =>
                    from e1 in l1s
                    join e2 in l2s on e1.Id equals e2.OneToOne_Required_FK_Inverse2.Id
                    select new
                    {
                        Id1 = e1.Id,
                        Id2 = e2.Id
                    },
                e => e.Id1 + " " + e.Id2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_key_access_optional_comparison(bool isAsync)
        {
            return AssertQueryScalar<Level2>(
                isAsync,
                l2s =>
                    from e2 in l2s
                    where e2.OneToOne_Optional_PK_Inverse2.Id > 5
                    select e2.Id,
                l2s =>
                    from e2 in l2s
                    where MaybeScalar<int>(e2.OneToOne_Optional_PK_Inverse2, () => e2.OneToOne_Optional_PK_Inverse2.Id) > 5
                    select e2.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Simple_level1_include(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => l1s.Include(l1 => l1.OneToOne_Required_PK1), elementSorter: e => e.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Simple_level1(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => l1s, elementSorter: e => e.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Simple_level1_level2_include(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => l1s.Include(l1 => l1.OneToOne_Required_PK1.OneToOne_Required_PK2), elementSorter: e => e.Id);
        }

        [ConditionalTheory(Skip = "issue #15249")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Simple_level1_level2_GroupBy_Count(bool isAsync)
        {
            return AssertQueryScalar<Level1>(
                isAsync,
                l1s => l1s.GroupBy(
                        l1 => l1.OneToOne_Required_PK1.OneToOne_Required_PK2.Name)
                    .Select(g => g.Count()),
                l1s => l1s.GroupBy(
                        l1 => Maybe(
                            l1.OneToOne_Required_PK1,
                            () => Maybe(
                                l1.OneToOne_Required_PK1.OneToOne_Required_PK2,
                                () => l1.OneToOne_Required_PK1.OneToOne_Required_PK2.Name)))
                    .Select(g => g.Count()));
        }

        [ConditionalTheory(Skip = "issue #15249")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Simple_level1_level2_GroupBy_Having_Count(bool isAsync)
        {
            return AssertQueryScalar<Level1>(
                isAsync,
                l1s => l1s.GroupBy(
                        l1 => l1.OneToOne_Required_PK1.OneToOne_Required_PK2.Name,
                        l1 => new
                        {
                            Id = ((int?)l1.OneToOne_Required_PK1.Id ?? 0)
                        })
                    .Where(g => g.Min(l1 => l1.Id) > 0)
                    .Select(g => g.Count()),
                l1s => l1s.GroupBy(
                        l1 => Maybe(
                            l1.OneToOne_Required_PK1,
                            () => Maybe(
                                l1.OneToOne_Required_PK1.OneToOne_Required_PK2,
                                () => l1.OneToOne_Required_PK1.OneToOne_Required_PK2.Name)),
                        l1 => new
                        {
                            Id = (MaybeScalar<int>(l1.OneToOne_Required_PK1, () => l1.OneToOne_Required_PK1.Id) ?? 0)
                        })
                    .Where(g => g.Min(l1 => l1.Id) > 0)
                    .Select(g => g.Count()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Simple_level1_level2_level3_include(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => l1s.Include(l1 => l1.OneToOne_Required_PK1.OneToOne_Required_PK2.OneToOne_Required_PK3),
                elementSorter: e => e.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_key_access_required_comparison(bool isAsync)
        {
            return AssertQueryScalar<Level2>(
                isAsync,
                l2s =>
                    from e2 in l2s
                    where e2.OneToOne_Required_PK_Inverse2.Id > 5
                    select e2.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_inside_method_call_translated_to_join(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s =>
                    from e1 in l1s
                    where e1.OneToOne_Required_FK1.Name.StartsWith("L")
                    select e1,
                l1s =>
                    from e1 in l1s
                    where MaybeScalar<bool>(e1.OneToOne_Required_FK1, () => e1.OneToOne_Required_FK1.Name.StartsWith("L")) == true
                    select e1,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_inside_method_call_translated_to_join2(bool isAsync)
        {
            return AssertQuery<Level3>(
                isAsync,
                l3s =>
                    from e3 in l3s
                    where e3.OneToOne_Required_FK_Inverse3.Name.StartsWith("L")
                    select e3,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_inside_method_call_translated_to_join(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s =>
                    from e1 in l1s
                    where e1.OneToOne_Optional_FK1.Name.StartsWith("L")
                    select e1,
                l1s =>
                    from e1 in l1s
                    where MaybeScalar<bool>(e1.OneToOne_Optional_FK1, () => e1.OneToOne_Optional_FK1.Name.StartsWith("L")) == true
                    select e1,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_inside_property_method_translated_to_join(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s =>
                    from e1 in l1s
                    where EF.Property<string>(EF.Property<Level2>(e1, "OneToOne_Optional_FK1"), "Name") == "L2 01"
                    select e1,
                l1s =>
                    from e1 in l1s
                    where Maybe(e1.OneToOne_Optional_FK1, () => e1.OneToOne_Optional_FK1.Name.ToUpper()) == "L2 01"
                    select e1,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_inside_nested_method_call_translated_to_join(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s =>
                    from e1 in l1s
                    where e1.OneToOne_Optional_FK1.Name.ToUpper().StartsWith("L")
                    select e1,
                l1s =>
                    from e1 in l1s
                    where MaybeScalar<bool>(e1.OneToOne_Optional_FK1, () => e1.OneToOne_Optional_FK1.Name.ToUpper().StartsWith("L")) == true
                    select e1,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Method_call_on_optional_navigation_translates_to_null_conditional_properly_for_arguments(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s =>
                    from e1 in l1s
                    where e1.OneToOne_Optional_FK1.Name.StartsWith(e1.OneToOne_Optional_FK1.Name)
                    select e1,
                l1s =>
                    from e1 in l1s
                    where MaybeScalar<bool>(
                              e1.OneToOne_Optional_FK1, () => e1.OneToOne_Optional_FK1.Name.StartsWith(e1.OneToOne_Optional_FK1.Name))
                          == true
                    select e1,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_inside_method_call_translated_to_join_keeps_original_nullability(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s =>
                    from e1 in l1s
                    where e1.OneToOne_Optional_FK1.Date.AddDays(10) > new DateTime(2000, 2, 1)
                    select e1,
                l1s =>
                    from e1 in l1s
                    where MaybeScalar<DateTime>(e1.OneToOne_Optional_FK1, () => e1.OneToOne_Optional_FK1.Date.AddDays(10))
                          > new DateTime(2000, 2, 1)
                    select e1,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_inside_nested_method_call_translated_to_join_keeps_original_nullability(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s =>
                    from e1 in l1s
                    where e1.OneToOne_Optional_FK1.Date.AddDays(10).AddDays(15).AddMonths(2) > new DateTime(2002, 2, 1)
                    select e1,
                l1s =>
                    from e1 in l1s
                    where MaybeScalar<DateTime>(
                              e1.OneToOne_Optional_FK1, () => e1.OneToOne_Optional_FK1.Date.AddDays(10).AddDays(15).AddMonths(2))
                          > new DateTime(2000, 2, 1)
                    select e1,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_inside_nested_method_call_translated_to_join_keeps_original_nullability_also_for_arguments(
            bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s =>
                    from e1 in l1s
                    where e1.OneToOne_Optional_FK1.Date.AddDays(15).AddDays(e1.OneToOne_Optional_FK1.Id) > new DateTime(2002, 2, 1)
                    select e1,
                l1s =>
                    from e1 in l1s
                    where MaybeScalar<DateTime>(
                              e1.OneToOne_Optional_FK1,
                              () => e1.OneToOne_Optional_FK1.Date.AddDays(15).AddDays(e1.OneToOne_Optional_FK1.Id))
                          > new DateTime(2000, 2, 1)
                    select e1,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_navigation_in_outer_selector_translated_to_extra_join(bool isAsync)
        {
            return AssertQuery<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from e1 in l1s
                    join e2 in l2s on e1.OneToOne_Optional_FK1.Id equals e2.Id
                    select new
                    {
                        Id1 = e1.Id,
                        Id2 = e2.Id
                    },
                (l1s, l2s) =>
                    from e1 in l1s
                    join e2 in l2s on MaybeScalar<int>(e1.OneToOne_Optional_FK1, () => e1.OneToOne_Optional_FK1.Id) equals e2.Id
                    select new
                    {
                        Id1 = e1.Id,
                        Id2 = e2.Id
                    },
                e => e.Id1 + " " + e.Id2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_navigation_in_outer_selector_translated_to_extra_join_nested(bool isAsync)
        {
            return AssertQuery<Level1, Level3>(
                isAsync,
                (l1s, l3s) =>
                    from e1 in l1s
                    join e3 in l3s on e1.OneToOne_Required_FK1.OneToOne_Optional_FK2.Id equals e3.Id
                    select new
                    {
                        Id1 = e1.Id,
                        Id3 = e3.Id
                    },
                (l1s, l3s) =>
                    from e1 in l1s
                    join e3 in l3s on MaybeScalar(
                        e1.OneToOne_Required_FK1,
                        () => MaybeScalar<int>(
                            e1.OneToOne_Required_FK1.OneToOne_Optional_FK2,
                            () => e1.OneToOne_Required_FK1.OneToOne_Optional_FK2.Id)) equals e3.Id
                    select new
                    {
                        Id1 = e1.Id,
                        Id3 = e3.Id
                    },
                e => e.Id1 + " " + e.Id3);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_navigation_in_outer_selector_translated_to_extra_join_nested2(bool isAsync)
        {
            return AssertQuery<Level1, Level3>(
                isAsync,
                (l1s, l3s) =>
                    from e3 in l3s
                    join e1 in l1s on e3.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK_Inverse2.Id equals e1.Id
                    select new
                    {
                        Id3 = e3.Id,
                        Id1 = e1.Id
                    },
                (l1s, l3s) =>
                    from e3 in l3s
                    join e1 in l1s on MaybeScalar<int>(
                        e3.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK_Inverse2,
                        () => e3.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK_Inverse2.Id) equals e1.Id
                    select new
                    {
                        Id3 = e3.Id,
                        Id1 = e1.Id
                    },
                e => e.Id1 + " " + e.Id3);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_navigation_in_inner_selector(bool isAsync)
        {
            return AssertQuery<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from e2 in l2s
                    join e1 in l1s on e2.Id equals e1.OneToOne_Optional_FK1.Id
                    select new
                    {
                        Id2 = e2.Id,
                        Id1 = e1.Id
                    },
                (l1s, l2s) =>
                    from e2 in l2s
                    join e1 in l1s on e2.Id equals MaybeScalar<int>(e1.OneToOne_Optional_FK1, () => e1.OneToOne_Optional_FK1.Id)
                    select new
                    {
                        Id2 = e2.Id,
                        Id1 = e1.Id
                    },
                e => e.Id2 + " " + e.Id1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_navigations_in_inner_selector_translated_without_collision(bool isAsync)
        {
            return AssertQuery<Level1, Level2, Level3>(
                isAsync,
                (l1s, l2s, l3s) =>
                    from e2 in l2s
                    join e1 in l1s on e2.Id equals e1.OneToOne_Optional_FK1.Id
                    join e3 in l3s on e2.Id equals e3.OneToOne_Optional_FK_Inverse3.Id
                    select new
                    {
                        Id2 = e2.Id,
                        Id1 = e1.Id,
                        Id3 = e3.Id
                    },
                (l1s, l2s, l3s) =>
                    from e2 in l2s
                    join e1 in l1s on e2.Id equals MaybeScalar<int>(e1.OneToOne_Optional_FK1, () => e1.OneToOne_Optional_FK1.Id)
                    join e3 in l3s on e2.Id equals MaybeScalar<int>(
                        e3.OneToOne_Optional_FK_Inverse3, () => e3.OneToOne_Optional_FK_Inverse3.Id)
                    select new
                    {
                        Id2 = e2.Id,
                        Id1 = e1.Id,
                        Id3 = e3.Id
                    },
                e => e.Id2 + " " + e.Id1 + " " + e.Id3);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_navigation_non_key_join(bool isAsync)
        {
            return AssertQuery<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from e2 in l2s
                    join e1 in l1s on e2.Name equals e1.OneToOne_Optional_FK1.Name
                    select new
                    {
                        Id2 = e2.Id,
                        Name2 = e2.Name,
                        Id1 = e1.Id,
                        Name1 = e1.Name
                    },
                (l1s, l2s) =>
                    from e2 in l2s
                    join e1 in l1s on e2.Name equals Maybe(e1.OneToOne_Optional_FK1, () => e1.OneToOne_Optional_FK1.Name)
                    select new
                    {
                        Id2 = e2.Id,
                        Name2 = e2.Name,
                        Id1 = e1.Id,
                        Name1 = e1.Name
                    },
                e => e.Id2 + " " + e.Name2 + " " + e.Id1 + " " + e.Name1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_with_orderby_on_inner_sequence_navigation_non_key_join(bool isAsync)
        {
            return AssertQuery<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from e2 in l2s
                    join e1 in l1s.OrderBy(l1 => l1.Id) on e2.Name equals e1.OneToOne_Optional_FK1.Name
                    select new
                    {
                        Id2 = e2.Id,
                        Name2 = e2.Name,
                        Id1 = e1.Id,
                        Name1 = e1.Name
                    },
                (l1s, l2s) =>
                    from e2 in l2s
                    join e1 in l1s.OrderBy(l1 => l1.Id) on e2.Name equals Maybe(
                        e1.OneToOne_Optional_FK1, () => e1.OneToOne_Optional_FK1.Name)
                    select new
                    {
                        Id2 = e2.Id,
                        Name2 = e2.Name,
                        Id1 = e1.Id,
                        Name1 = e1.Name
                    },
                e => e.Id2 + " " + e.Name2 + " " + e.Id1 + " " + e.Name1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_navigation_self_ref(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s =>
                    from e1 in l1s
                    join e2 in l1s on e1.Id equals e2.OneToMany_Optional_Self_Inverse1.Id
                    select new
                    {
                        Id1 = e1.Id,
                        Id2 = e2.Id
                    },
                l1s =>
                    from e1 in l1s
                    join e2 in l1s on e1.Id equals MaybeScalar<int>(
                        e2.OneToMany_Optional_Self_Inverse1, () => e2.OneToMany_Optional_Self_Inverse1.Id)
                    select new
                    {
                        Id1 = e1.Id,
                        Id2 = e2.Id
                    },
                e => e.Id1 + " " + e.Id2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_navigation_nested(bool isAsync)
        {
            return AssertQuery<Level1, Level3>(
                isAsync,
                (l1s, l3s) =>
                    from e3 in l3s
                    join e1 in l1s on e3.Id equals e1.OneToOne_Required_FK1.OneToOne_Optional_FK2.Id
                    select new
                    {
                        Id3 = e3.Id,
                        Id1 = e1.Id
                    },
                (l1s, l3s) =>
                    from e3 in l3s
                    join e1 in l1s on e3.Id equals MaybeScalar(
                        e1.OneToOne_Required_FK1,
                        () => MaybeScalar<int>(
                            e1.OneToOne_Required_FK1.OneToOne_Optional_FK2,
                            () => e1.OneToOne_Required_FK1.OneToOne_Optional_FK2.Id))
                    select new
                    {
                        Id3 = e3.Id,
                        Id1 = e1.Id
                    },
                e => e.Id3 + " " + e.Id1);
        }

        // issue #12787
        //[ConditionalTheory]
        //[MemberData(nameof(IsAsyncData))]
        public virtual Task Join_navigation_nested2(bool isAsync)
        {
            return AssertQuery<Level1, Level3>(
                isAsync,
                (l1s, l3s) =>
                    from e3 in l3s
                    join e1 in l1s.OrderBy(ll => ll.Id) on e3.Id equals e1.OneToOne_Required_FK1.OneToOne_Optional_FK2.Id
                    select new
                    {
                        Id3 = e3.Id,
                        Id1 = e1.Id
                    },
                (l1s, l3s) =>
                    from e3 in l3s
                    join e1 in l1s.OrderBy(ll => ll.Id) on e3.Id equals MaybeScalar(
                        e1.OneToOne_Required_FK1,
                        () => MaybeScalar<int>(
                            e1.OneToOne_Required_FK1.OneToOne_Optional_FK2,
                            () => e1.OneToOne_Required_FK1.OneToOne_Optional_FK2.Id))
                    select new
                    {
                        Id3 = e3.Id,
                        Id1 = e1.Id
                    },
                e => e.Id3 + " " + e.Id1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_navigation_deeply_nested_non_key_join(bool isAsync)
        {
            return AssertQuery<Level1, Level4>(
                isAsync,
                (l1s, l4s) =>
                    from e4 in l4s
                    join e1 in l1s on e4.Name equals e1.OneToOne_Required_FK1.OneToOne_Optional_FK2.OneToOne_Required_PK3.Name
                    select new
                    {
                        Id4 = e4.Id,
                        Name4 = e4.Name,
                        Id1 = e1.Id,
                        Name1 = e1.Name
                    },
                (l1s, l4s) =>
                    from e4 in l4s
                    join e1 in l1s on e4.Name equals Maybe(
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
                e => e.Id4 + " " + e.Name4 + " " + e.Id1 + " " + e.Name1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_navigation_deeply_nested_required(bool isAsync)
        {
            return AssertQuery<Level1, Level4>(
                isAsync,
                (l1s, l4s) =>
                    from e1 in l1s
                    join e4 in l4s on e1.Name equals e4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3
                        .OneToOne_Required_PK_Inverse2.Name
                    select new
                    {
                        Id4 = e4.Id,
                        Name4 = e4.Name,
                        Id1 = e1.Id,
                        Name1 = e1.Name
                    },
                e => e.Id4 + " " + e.Name4 + " " + e.Id1 + " " + e.Name1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_complex_includes(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1, "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional2, "OneToMany_Optional2", navigationPath: "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level1>(l1 => l1.OneToMany_Optional1, "OneToMany_Optional1"),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2, "OneToOne_Optional_FK2", navigationPath: "OneToMany_Optional1")
            };

            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s
                    .Include(e => e.OneToOne_Optional_FK1)
                    .ThenInclude(e => e.OneToMany_Optional2)
                    .Include(e => e.OneToMany_Optional1)
                    .ThenInclude(e => e.OneToOne_Optional_FK2),
                expectedIncludes,
                elementSorter: e => e.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_complex_includes_self_ref(bool isAsync)
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

            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s
                    .Include(e => e.OneToOne_Optional_Self1)
                    .ThenInclude(e => e.OneToMany_Optional_Self1)
                    .Include(e => e.OneToMany_Optional_Self1)
                    .ThenInclude(e => e.OneToOne_Optional_Self1),
                expectedIncludes,
                elementSorter: e => e.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_complex_include_select(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1, "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional2, "OneToMany_Optional2", navigationPath: "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level1>(l1 => l1.OneToMany_Optional1, "OneToMany_Optional1"),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2, "OneToOne_Optional_FK2", navigationPath: "OneToMany_Optional1")
            };

            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s
                    .Select(e => e)
                    .Include(e => e.OneToOne_Optional_FK1)
                    .ThenInclude(e => e.OneToMany_Optional2)
                    .Select(e => e)
                    .Include(e => e.OneToMany_Optional1)
                    .ThenInclude(e => e.OneToOne_Optional_FK2),
                expectedIncludes,
                elementSorter: e => e.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_nav_prop_collection_one_to_many_required(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => l1s.OrderBy(e => e.Id).Select(e => e.OneToMany_Required1.Select(i => i.Id)),
                assertOrder: true,
                elementAsserter: (e, a) =>
                {
                    var expectedList = ((IEnumerable<int>)e).OrderBy(ee => ee).ToList();
                    var actualList = ((IEnumerable<int>)a).OrderBy(aa => aa).ToList();
                    Assert.Equal(expectedList.Count, actualList.Count);
                    for (var i = 0; i < expectedList.Count; i++)
                    {
                        Assert.Equal(expectedList[i], actualList[i]);
                    }
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_nav_prop_reference_optional1(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => l1s.Select(e => e.OneToOne_Optional_FK1.Name),
                l1s => l1s.Select(e => Maybe(e.OneToOne_Optional_FK1, () => e.OneToOne_Optional_FK1.Name)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_nav_prop_reference_optional1_via_DefaultIfEmpty(bool isAsync)
        {
            return AssertQuery<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from l1 in l1s
                    join l2 in l2s on l1.Id equals l2.Level1_Optional_Id into groupJoin
                    from l2 in groupJoin.DefaultIfEmpty()
#pragma warning disable IDE0031 // Use null propagation
                    select l2 == null ? null : l2.Name,
#pragma warning restore IDE0031 // Use null propagation
                (l1s, l2s) =>
                    from l1 in l1s
                    join l2 in l2s on l1.Id equals MaybeScalar(l2, () => l2.Level1_Optional_Id) into groupJoin
                    from l2 in groupJoin.DefaultIfEmpty()
#pragma warning disable IDE0031 // Use null propagation
                    select l2 == null ? null : l2.Name);
#pragma warning restore IDE0031 // Use null propagation
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_nav_prop_reference_optional2(bool isAsync)
        {
            return AssertQueryScalar<Level1>(
                isAsync,
                l1s => l1s.Select(e => (int?)e.OneToOne_Optional_FK1.Id),
                l1s => l1s.Select(e => MaybeScalar<int>(e.OneToOne_Optional_FK1, () => e.OneToOne_Optional_FK1.Id)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_nav_prop_reference_optional2_via_DefaultIfEmpty(bool isAsync)
        {
            return AssertQueryScalar<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from l1 in l1s
                    join l2 in l2s on l1.Id equals l2.Level1_Optional_Id into groupJoin
                    from l2 in groupJoin.DefaultIfEmpty()
                    select l2 == null ? null : (int?)l2.Id,
                (l1s, l2s) =>
                    from l1 in l1s
                    join l2 in l2s on l1.Id equals MaybeScalar(l2, () => l2.Level1_Optional_Id) into groupJoin
                    from l2 in Maybe(groupJoin, () => groupJoin.DefaultIfEmpty())
                    select l2 == null ? null : (int?)l2.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_nav_prop_reference_optional3(bool isAsync)
        {
            return AssertQuery<Level2>(
                isAsync,
                l2s => l2s.Select(e => e.OneToOne_Optional_FK_Inverse2.Name),
                l2s => l2s.Select(e => Maybe(e.OneToOne_Optional_FK_Inverse2, () => e.OneToOne_Optional_FK_Inverse2.Name)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_nav_prop_reference_optional1(bool isAsync)
        {
            return AssertQueryScalar<Level1>(
                isAsync,
                l1s => l1s
                    .Where(e => e.OneToOne_Optional_FK1.Name == "L2 05" || e.OneToOne_Optional_FK1.Name == "L2 07")
                    .Select(e => e.Id),
                l1s => l1s
                    .Where(
                        e => Maybe(e.OneToOne_Optional_FK1, () => e.OneToOne_Optional_FK1.Name) == "L2 05"
                             || Maybe(e.OneToOne_Optional_FK1, () => e.OneToOne_Optional_FK1.Name) == "L2 07")
                    .Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_nav_prop_reference_optional1_via_DefaultIfEmpty(bool isAsync)
        {
            return AssertQueryScalar<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from l1 in l1s
                    join l2Left in l2s on l1.Id equals l2Left.Level1_Optional_Id into groupJoinLeft
                    from l2Left in groupJoinLeft.DefaultIfEmpty()
                    join l2Right in l2s on l1.Id equals l2Right.Level1_Optional_Id into groupJoinRight
                    from l2Right in groupJoinRight.DefaultIfEmpty()
#pragma warning disable IDE0031 // Use null propagation
                    where (l2Left == null ? null : l2Left.Name) == "L2 05" || (l2Right == null ? null : l2Right.Name) == "L2 07"
#pragma warning restore IDE0031 // Use null propagation
                    select l1.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_nav_prop_reference_optional2(bool isAsync)
        {
            return AssertQueryScalar<Level1>(
                isAsync,
                l1s => l1s
                    .Where(e => e.OneToOne_Optional_FK1.Name == "L2 05" || e.OneToOne_Optional_FK1.Name != "L2 42")
                    .Select(e => e.Id),
                l1s => l1s
                    .Where(
                        e => Maybe(e.OneToOne_Optional_FK1, () => e.OneToOne_Optional_FK1.Name) == "L2 05"
                             || Maybe(e.OneToOne_Optional_FK1, () => e.OneToOne_Optional_FK1.Name) != "L2 42")
                    .Select(e => e.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_nav_prop_reference_optional2_via_DefaultIfEmpty(bool isAsync)
        {
            return AssertQueryScalar<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from l1 in l1s
                    join l2Left in l2s on l1.Id equals l2Left.Level1_Optional_Id into groupJoinLeft
                    from l2Left in groupJoinLeft.DefaultIfEmpty()
                    join l2Right in l2s on l1.Id equals l2Right.Level1_Optional_Id into groupJoinRight
                    from l2Right in groupJoinRight.DefaultIfEmpty()
#pragma warning disable IDE0031 // Use null propagation
                    where (l2Left == null ? null : l2Left.Name) == "L2 05" || (l2Right == null ? null : l2Right.Name) != "L2 42"
#pragma warning restore IDE0031 // Use null propagation
                    select l1.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_multiple_nav_prop_reference_optional(bool isAsync)
        {
            return AssertQueryScalar<Level1>(
                isAsync,
                l1s => l1s.Select(e => (int?)e.OneToOne_Optional_FK1.OneToOne_Optional_FK2.Id),
                l1s => l1s.Select(
                    e => MaybeScalar(
                        e.OneToOne_Optional_FK1,
                        () => MaybeScalar<int>(
                            e.OneToOne_Optional_FK1.OneToOne_Optional_FK2,
                            () => e.OneToOne_Optional_FK1.OneToOne_Optional_FK2.Id))));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_multiple_nav_prop_reference_optional_member_compared_to_value(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s =>
                    from l1 in l1s
                    where l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2.Name != "L3 05"
                    select l1,
                l1s =>
                    from l1 in l1s
                    where Maybe(
                              l1.OneToOne_Optional_FK1,
                              () => Maybe(
                                  l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2,
                                  () => l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2.Name)) != "L3 05"
                    select l1,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_multiple_nav_prop_reference_optional_member_compared_to_null(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s =>
                    from l1 in l1s
                    where l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2.Name != null
                    select l1,
                l1s =>
                    from l1 in l1s
                    where Maybe(
                              l1.OneToOne_Optional_FK1,
                              () => Maybe(
                                  l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2,
                                  () => l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2.Name)) != null
                    select l1,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_multiple_nav_prop_reference_optional_compared_to_null1(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s =>
                    from l1 in l1s
                    where l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2 == null
                    select l1,
                l1s =>
                    from l1 in l1s
                    where Maybe(
                              l1.OneToOne_Optional_FK1,
                              () => l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2) == null
                    select l1,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_multiple_nav_prop_reference_optional_compared_to_null2(bool isAsync)
        {
            return AssertQuery<Level3>(
                isAsync,
                l3s =>
                    from l3 in l3s
                    where l3.OneToOne_Optional_FK_Inverse3.OneToOne_Optional_FK_Inverse2 == null
                    select l3,
                l3s =>
                    from l3 in l3s
                    where Maybe(
                              l3.OneToOne_Optional_FK_Inverse3,
                              () => l3.OneToOne_Optional_FK_Inverse3.OneToOne_Optional_FK_Inverse2) == null
                    select l3,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_multiple_nav_prop_reference_optional_compared_to_null3(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s =>
                    from l1 in l1s
                    where null != l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2
                    select l1,
                l1s =>
                    from l1 in l1s
                    where null != Maybe(
                              l1.OneToOne_Optional_FK1,
                              () => l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2)
                    select l1,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_multiple_nav_prop_reference_optional_compared_to_null4(bool isAsync)
        {
            return AssertQuery<Level3>(
                isAsync,
                l3s =>
                    from l3 in l3s
                    where null != l3.OneToOne_Optional_FK_Inverse3.OneToOne_Optional_FK_Inverse2
                    select l3,
                l3s =>
                    from l3 in l3s
                    where null != Maybe(
                              l3.OneToOne_Optional_FK_Inverse3, () => l3.OneToOne_Optional_FK_Inverse3.OneToOne_Optional_FK_Inverse2)
                    select l3,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_multiple_nav_prop_reference_optional_compared_to_null5(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => l1s.Where(e => e.OneToOne_Optional_FK1.OneToOne_Required_FK2.OneToOne_Required_FK3 == null),
                l1s => l1s.Where(
                    e => Maybe(
                             e.OneToOne_Optional_FK1,
                             () => Maybe(
                                 e.OneToOne_Optional_FK1.OneToOne_Required_FK2,
                                 () => e.OneToOne_Optional_FK1.OneToOne_Required_FK2.OneToOne_Required_FK3)) == null),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_multiple_nav_prop_reference_required(bool isAsync)
        {
            return AssertQueryScalar<Level1>(
                isAsync,
                l1s => l1s.Select(e => (int?)e.OneToOne_Required_FK1.OneToOne_Required_FK2.Id),
                l1s => l1s.Select(
                    e => MaybeScalar(
                        e.OneToOne_Required_FK1,
                        () => MaybeScalar<int>(
                            e.OneToOne_Required_FK1.OneToOne_Required_FK2,
                            () => e.OneToOne_Required_FK1.OneToOne_Required_FK2.Id))));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_multiple_nav_prop_reference_required2(bool isAsync)
        {
            return AssertQueryScalar<Level3>(
                isAsync,
                l3s => l3s.Select(e => e.OneToOne_Required_FK_Inverse3.OneToOne_Required_FK_Inverse2.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_multiple_nav_prop_optional_required(bool isAsync)
        {
            return AssertQueryScalar<Level1>(
                isAsync,
                l1s =>
                    from l1 in l1s
                    select (int?)l1.OneToOne_Optional_FK1.OneToOne_Required_FK2.Id,
                l1s =>
                    from l1 in l1s
                    select MaybeScalar(
                        l1.OneToOne_Optional_FK1,
                        () => MaybeScalar<int>(
                            l1.OneToOne_Optional_FK1.OneToOne_Required_FK2,
                            () => l1.OneToOne_Optional_FK1.OneToOne_Required_FK2.Id)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_multiple_nav_prop_optional_required(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s =>
                    from l1 in l1s
                    where l1.OneToOne_Optional_FK1.OneToOne_Required_FK2.Name != "L3 05"
                    select l1,
                l1s =>
                    from l1 in l1s
                    where Maybe(
                              l1.OneToOne_Optional_FK1,
                              () => Maybe(
                                  l1.OneToOne_Optional_FK1.OneToOne_Required_FK2,
                                  () => l1.OneToOne_Optional_FK1.OneToOne_Required_FK2.Name)) != "L3 05"
                    select l1,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_navigation_comparison1(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s =>
                    from l11 in l1s
                    from l12 in l1s
                    where l11 == l12
                    select new
                    {
                        Id1 = l11.Id,
                        Id2 = l12.Id
                    },
                l1s =>
                    from l11 in l1s
                    from l12 in l1s
                    where l11.Id == l12.Id
                    select new
                    {
                        Id1 = l11.Id,
                        Id2 = l12.Id
                    },
                e => e.Id1 + " " + e.Id2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_navigation_comparison2(bool isAsync)
        {
            return AssertQuery<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from l1 in l1s
                    from l2 in l2s
                    where l1 == l2.OneToOne_Optional_FK_Inverse2
                    select new
                    {
                        Id1 = l1.Id,
                        Id2 = l2.Id
                    },
                (l1s, l2s) =>
                    from l1 in l1s
                    from l2 in l2s
                    where l1.Id == MaybeScalar<int>(l2.OneToOne_Optional_FK_Inverse2, () => l2.OneToOne_Optional_FK_Inverse2.Id)
                    select new
                    {
                        Id1 = l1.Id,
                        Id2 = l2.Id
                    },
                e => e.Id1 + " " + e.Id2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_navigation_comparison3(bool isAsync)
        {
            return AssertQuery<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from l1 in l1s
                    from l2 in l2s
                    where l1.OneToOne_Optional_FK1 == l2
                    select new
                    {
                        Id1 = l1.Id,
                        Id2 = l2.Id
                    },
                (l1s, l2s) =>
                    from l1 in l1s
                    from l2 in l2s
                    where MaybeScalar<int>(l1.OneToOne_Optional_FK1, () => l1.OneToOne_Optional_FK1.Id) == l2.Id
                    select new
                    {
                        Id1 = l1.Id,
                        Id2 = l2.Id
                    },
                e => e.Id1 + " " + e.Id2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_complex_predicate_with_with_nav_prop_and_OrElse1(bool isAsync)
        {
            return AssertQuery<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from l1 in l1s
                    from l2 in l2s
                    where l1.OneToOne_Optional_FK1.Name == "L2 01" || l2.OneToOne_Required_FK_Inverse2.Name != "Bar"
                    select new
                    {
                        Id1 = (int?)l1.Id,
                        Id2 = (int?)l2.Id
                    },
                (l1s, l2s) =>
                    from l1 in l1s
                    from l2 in l2s
                    where Maybe(l1.OneToOne_Optional_FK1, () => l1.OneToOne_Optional_FK1.Name) == "L2 01"
                          || l2.OneToOne_Required_FK_Inverse2.Name != "Bar"
                    select new
                    {
                        Id1 = (int?)l1.Id,
                        Id2 = (int?)l2.Id
                    },
                e => e.Id1 + " " + e.Id2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_complex_predicate_with_with_nav_prop_and_OrElse2(bool isAsync)
        {
            return AssertQueryScalar<Level1>(
                isAsync,
                l1s =>
                    from l1 in l1s
                    where l1.OneToOne_Optional_FK1.OneToOne_Required_FK2.Name == "L3 05" || l1.OneToOne_Optional_FK1.Name != "L2 05"
                    select l1.Id,
                l1s =>
                    from l1 in l1s
                    where Maybe(
                              l1.OneToOne_Optional_FK1,
                              () => Maybe(
                                  l1.OneToOne_Optional_FK1.OneToOne_Required_FK2,
                                  () => l1.OneToOne_Optional_FK1.OneToOne_Required_FK2.Name)) == "L3 05"
                          || Maybe(
                              l1.OneToOne_Optional_FK1,
                              () => l1.OneToOne_Optional_FK1.Name) != "L2 05"
                    select l1.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_complex_predicate_with_with_nav_prop_and_OrElse3(bool isAsync)
        {
            return AssertQueryScalar<Level1>(
                isAsync,
                l1s =>
                    from l1 in l1s
                    where l1.OneToOne_Optional_FK1.Name != "L2 05" || l1.OneToOne_Required_FK1.OneToOne_Optional_FK2.Name == "L3 05"
                    select l1.Id,
                l1s =>
                    from l1 in l1s
                    where Maybe(
                              l1.OneToOne_Optional_FK1,
                              () => l1.OneToOne_Optional_FK1.Name) != "L2 05"
                          || Maybe(
                              l1.OneToOne_Required_FK1,
                              () => Maybe(
                                  l1.OneToOne_Required_FK1.OneToOne_Optional_FK2,
                                  () => l1.OneToOne_Required_FK1.OneToOne_Optional_FK2.Name)) == "L3 05"
                    select l1.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_complex_predicate_with_with_nav_prop_and_OrElse4(bool isAsync)
        {
            return AssertQueryScalar<Level3>(
                isAsync,
                l3s =>
                    from l3 in l3s
                    where l3.OneToOne_Optional_FK_Inverse3.Name != "L2 05"
                          || l3.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK_Inverse2.Name == "L1 05"
                    select l3.Id,
                l3s =>
                    from l3 in l3s
                    where Maybe(
                              l3.OneToOne_Optional_FK_Inverse3,
                              () => l3.OneToOne_Optional_FK_Inverse3.Name) != "L2 05"
                          || Maybe(
                              l3.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK_Inverse2,
                              () => l3.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK_Inverse2.Name) == "L1 05"
                    select l3.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Complex_navigations_with_predicate_projected_into_anonymous_type(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => l1s
                    .Where(
                        e => e.OneToOne_Required_FK1.OneToOne_Required_FK2 == e.OneToOne_Required_FK1.OneToOne_Optional_FK2
                             && e.OneToOne_Required_FK1.OneToOne_Optional_FK2.Id != 7)
                    .Select(
                        e => new
                        {
                            e.Name,
                            Id = (int?)e.OneToOne_Required_FK1.OneToOne_Optional_FK2.Id
                        }),
                l1s => l1s
                    .Where(
                        e => Maybe(e.OneToOne_Required_FK1, () => e.OneToOne_Required_FK1.OneToOne_Required_FK2) == Maybe(
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
                elementSorter: e => e.Name + " " + e.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Name, a.Name);
                    Assert.Equal(e.Id, a.Id);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Complex_navigations_with_predicate_projected_into_anonymous_type2(bool isAsync)
        {
            return AssertQuery<Level3>(
                isAsync,
                l3s =>
                    from e in l3s
                    where e.OneToOne_Required_FK_Inverse3.OneToOne_Required_FK_Inverse2
                          == e.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK_Inverse2
                          && e.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK_Inverse2.Id != 7
                    select new
                    {
                        e.Name,
                        Id = (int?)e.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK_Inverse2.Id
                    },
                l3s =>
                    from e in l3s
                    where e.OneToOne_Required_FK_Inverse3.OneToOne_Required_FK_Inverse2
                          == e.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK_Inverse2
                          && MaybeScalar<int>(
                              e.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK_Inverse2,
                              () => e.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK_Inverse2.Id) != 7
                    select new
                    {
                        e.Name,
                        Id = MaybeScalar<int>(
                            e.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK_Inverse2,
                            () => e.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK_Inverse2.Id)
                    },
                e => e.Name + "" + e.Id);
        }

        [ConditionalFact]
        public virtual void Optional_navigation_projected_into_DTO()
        {
            using (var context = CreateContext())
            {
                var actual = Fixture.QueryAsserter.SetExtractor.Set<Level1>(context).Select(
                    e => new MyOuterDto
                    {
                        Id = e.Id,
                        Name = e.Name,
                        Inner = e.OneToOne_Optional_FK1 != null
                            ? new MyInnerDto
                            {
                                Id = (int?)e.OneToOne_Optional_FK1.Id,
                                Name = e.OneToOne_Optional_FK1.Name
                            }
                            : null
                    }).ToList().OrderBy(e => e.Id + " " + e.Name + " " + e.Inner).ToList();

                var expected = Fixture.QueryAsserter.ExpectedData.Set<Level1>().Select(
                    e => new MyOuterDto
                    {
                        Id = e.Id,
                        Name = e.Name,
                        Inner = e.OneToOne_Optional_FK1 != null
                            ? new MyInnerDto
                            {
                                Id = (int?)e.OneToOne_Optional_FK1.Id,
                                Name = e.OneToOne_Optional_FK1.Name
                            }
                            : null
                    }).ToList().OrderBy(e => e.Id + " " + e.Name + " " + e.Inner).ToList();

                Assert.Equal(expected.Count, actual.Count);
                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i].Id, actual[i].Id);
                    Assert.Equal(expected[i].Name, actual[i].Name);

                    if (expected[i].Inner == null)
                    {
                        Assert.Null(actual[i].Inner);
                    }
                    else
                    {
                        Assert.Equal(expected[i].Inner.Id, actual[i].Inner.Id);
                        Assert.Equal(expected[i].Inner.Name, actual[i].Inner.Name);
                    }
                }
            }
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
        public virtual Task OrderBy_nav_prop_reference_optional(bool isAsync)
        {
            return AssertQueryScalar<Level1>(
                isAsync,
                l1s =>
                    l1s.OrderBy(e => e.OneToOne_Optional_FK1.Name).ThenBy(e => e.Id).Select(e => e.Id),
                l1s =>
                    l1s.OrderBy(e => Maybe(e.OneToOne_Optional_FK1, () => e.OneToOne_Optional_FK1.Name)).ThenBy(e => e.Id)
                        .Select(e => e.Id),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_nav_prop_reference_optional_via_DefaultIfEmpty(bool isAsync)
        {
            return AssertQueryScalar<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from l1 in l1s
                    join l2 in l2s on l1.Id equals l2.Level1_Optional_Id into groupJoin
                    from l2 in groupJoin.DefaultIfEmpty()
#pragma warning disable IDE0031 // Use null propagation
                    orderby l2 == null ? null : l2.Name, l1.Id
#pragma warning restore IDE0031 // Use null propagation
                    select l1.Id,
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Result_operator_nav_prop_reference_optional_Sum(bool isAsync)
        {
            return AssertSum<Level1, Level1>(
                isAsync,
                l1s => l1s,
                l1s => l1s,
                actualSelector: e => (int?)e.OneToOne_Optional_FK1.Level1_Required_Id,
                expectedSelector: e => MaybeScalar<int>(e.OneToOne_Optional_FK1, () => e.OneToOne_Optional_FK1.Level1_Required_Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Result_operator_nav_prop_reference_optional_Min(bool isAsync)
        {
            return AssertMin<Level1, Level1>(
                isAsync,
                l1s => l1s,
                l1s => l1s,
                actualSelector: e => (int?)e.OneToOne_Optional_FK1.Level1_Required_Id,
                expectedSelector: e => MaybeScalar<int>(e.OneToOne_Optional_FK1, () => e.OneToOne_Optional_FK1.Level1_Required_Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Result_operator_nav_prop_reference_optional_Max(bool isAsync)
        {
            return AssertMax<Level1, Level1>(
                isAsync,
                l1s => l1s,
                l1s => l1s,
                actualSelector: e => (int?)e.OneToOne_Optional_FK1.Level1_Required_Id,
                expectedSelector: e => MaybeScalar<int>(e.OneToOne_Optional_FK1, () => e.OneToOne_Optional_FK1.Level1_Required_Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Result_operator_nav_prop_reference_optional_Average(bool isAsync)
        {
            return AssertAverage<Level1, Level1>(
                isAsync,
                l1s => l1s,
                l1s => l1s,
                actualSelector: e => (int?)e.OneToOne_Optional_FK1.Level1_Required_Id,
                expectedSelector: e => MaybeScalar<int>(e.OneToOne_Optional_FK1, () => e.OneToOne_Optional_FK1.Level1_Required_Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Result_operator_nav_prop_reference_optional_Average_with_identity_selector(bool isAsync)
        {
            return AssertAverage<Level1, int?>(
                isAsync,
                l1s => l1s.Select(e => (int?)e.OneToOne_Optional_FK1.Level1_Required_Id),
                l1s => l1s.Select(e => MaybeScalar<int>(e.OneToOne_Optional_FK1, () => e.OneToOne_Optional_FK1.Level1_Required_Id)),
                actualSelector: e => e,
                expectedSelector: e => e);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Result_operator_nav_prop_reference_optional_Average_without_selector(bool isAsync)
        {
            return AssertAverage<Level1>(
                isAsync,
                l1s => l1s.Select(e => (int?)e.OneToOne_Optional_FK1.Level1_Required_Id),
                l1s => l1s.Select(e => MaybeScalar<int>(e.OneToOne_Optional_FK1, () => e.OneToOne_Optional_FK1.Level1_Required_Id)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Result_operator_nav_prop_reference_optional_via_DefaultIfEmpty(bool isAsync)
        {
            return AssertSum<Level1, Level2, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from l1 in l1s
                    join l2 in l2s on l1.Id equals l2.Level1_Optional_Id into groupJoin
                    from l2 in groupJoin.DefaultIfEmpty()
                    select l2,
                selector: e => e == null ? 0 : e.Level1_Required_Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_with_optional_navigation(bool isAsync)
        {
            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => from l1 in l1s.Include(e => e.OneToOne_Optional_FK1)
                       where l1.OneToOne_Optional_FK1.Name != "L2 05"
                       select l1,
                l1s => from l1 in l1s.Include(e => e.OneToOne_Optional_FK1)
                       where Maybe(l1.OneToOne_Optional_FK1, () => l1.OneToOne_Optional_FK1.Name) != "L2 05"
                       select l1,
                new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1, "OneToOne_Optional_FK1")
                },
                elementSorter: e => e.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_nested_with_optional_navigation(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1, "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level2>(l1 => l1.OneToMany_Required2, "OneToMany_Required2", "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level3>(
                    l1 => l1.OneToOne_Required_FK3, "OneToOne_Required_FK3", "OneToOne_Optional_FK1.OneToMany_Required2")
            };

            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => from l1 in l1s
                           .Include(e => e.OneToOne_Optional_FK1.OneToMany_Required2)
                           .ThenInclude(e => e.OneToOne_Required_FK3)
                       where l1.OneToOne_Optional_FK1.Name != "L2 09"
                       select l1,
                l1s => from l1 in l1s
                       where Maybe(l1.OneToOne_Optional_FK1, () => l1.OneToOne_Optional_FK1.Name) != "L2 09"
                       select l1,
                expectedIncludes,
                elementSorter: l1 => l1.Id);
        }

        [ConditionalTheory(Skip = "Issue #14935. Cannot eval 'join Level3 l2.OneToOne_Required_PK2 in value(Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryable`1[Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel.Level3]) on Property([l2], \"Id\") equals Property([l2.OneToOne_Required_PK2], \"Id\")'")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_with_groupjoin_skip_and_take(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToMany_Optional1, "OneToMany_Optional1"),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2, "OneToOne_Optional_FK2", "OneToMany_Optional1"),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Required_PK2, "OneToOne_Required_PK2")
            };

            return AssertIncludeQuery<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    (from l1 in l1s
                         .Include(e => e.OneToMany_Optional1)
                         .ThenInclude(e => e.OneToOne_Optional_FK2)
                     join l2 in l2s.Include(e => e.OneToOne_Required_PK2)
#pragma warning disable IDE0031 // Use null propagation
                         on (int?)l1.Id equals l2 != null ? l2.Level1_Optional_Id : null into grouping
#pragma warning restore IDE0031 // Use null propagation
                     where l1.Name != "L1 03"
                     orderby l1.Id
                     select new
                     {
                         l1,
                         grouping
                     }).Skip(1).Take(5),
                expectedIncludes,
                clientProjections: new List<Func<dynamic, object>>
                {
                    e => new KeyValuePair<Level1, IEnumerable<Level2>>(e.l1, ((IEnumerable<Level2>)e.grouping).ToList())
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_flattening_bug_4539(bool isAsync)
        {
            return AssertQuery<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from l1 in l1s
                    join l1_Optional in l2s on (int?)l1.Id equals l1_Optional.Level1_Optional_Id into grouping
                    from l1_Optional in grouping.DefaultIfEmpty()
                    from l2 in l2s
                    join l2_Required_Reverse in l1s on l2.Level1_Required_Id equals l2_Required_Reverse.Id
                    select new
                    {
                        l1_Optional,
                        l2_Required_Reverse
                    },
                elementSorter: e => e.l1_Optional?.Id + " " + e.l2_Required_Reverse.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Query_source_materialization_bug_4547(bool isAsync)
        {
            return AssertQueryScalar<Level1, Level2, Level3>(
                isAsync,
                (l1s, l2s, l3s) =>
                    from e3 in l3s
                    join e1 in l1s
                        on
                        (int?)e3.Id
                        equals
                        (
                            from subQuery2 in l2s
                            join subQuery3 in l3s
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

        [ConditionalTheory(Skip = "Issue#15711")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_navigation_property(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => l1s.SelectMany(l1 => l1.OneToMany_Optional1),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_navigation_property_and_projection(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => l1s.SelectMany(l1 => l1.OneToMany_Optional1).Select(e => e.Name));
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_navigation_property_and_filter_before(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => l1s.Where(e => e.Id == 1).SelectMany(l1 => l1.OneToMany_Optional1),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_navigation_property_and_filter_after(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => l1s.SelectMany(l1 => l1.OneToMany_Optional1).Where(e => e.Id != 6),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_nested_navigation_property_required(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => l1s.SelectMany(l1 => l1.OneToOne_Required_FK1.OneToMany_Optional2),
                l1s => l1s.SelectMany(
                    l1 => Maybe(
                              l1.OneToOne_Required_FK1,
                              () => l1.OneToOne_Required_FK1.OneToMany_Optional2) ?? new List<Level3>()),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_nested_navigation_property_optional_and_projection(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => l1s.SelectMany(l1 => l1.OneToOne_Optional_FK1.OneToMany_Optional2).Select(e => e.Name),
                l1s => l1s.SelectMany(
                    l1 => Maybe(
                              l1.OneToOne_Optional_FK1,
                              () => l1.OneToOne_Optional_FK1.OneToMany_Optional2) ?? new List<Level3>()).Select(e => e.Name));
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_SelectMany_calls(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => l1s.SelectMany(e => e.OneToMany_Optional1).SelectMany(e => e.OneToMany_Optional2),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory(Skip = "issue #15081")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_navigation_property_with_another_navigation_in_subquery(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => l1s.SelectMany(l1 => l1.OneToMany_Optional1.Select(l2 => l2.OneToOne_Optional_FK2)),
                l1s => l1s.SelectMany(
                    l1 => Maybe(
                              l1.OneToMany_Optional1,
                              () => l1.OneToMany_Optional1.Select(l2 => l2.OneToOne_Optional_FK2)) ?? new List<Level3>()),
                e => e?.Id,
                (e, a) =>
                {
                    if (e == null)
                    {
                        Assert.Null(a);
                    }
                    else
                    {
                        Assert.Equal(e.Id, a.Id);
                    }
                });
        }

        [ConditionalTheory(Skip = " Issue#16093")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_navigation_property_to_collection(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => l1s.Where(l1 => l1.OneToOne_Required_FK1.OneToMany_Optional2.Count > 0),
                l1s => l1s.Where(
                    l1 => MaybeScalar(
                              l1.OneToOne_Required_FK1,
                              () => MaybeScalar<int>(
                                  l1.OneToOne_Required_FK1.OneToMany_Optional2,
                                  () => l1.OneToOne_Required_FK1.OneToMany_Optional2.Count)) > 0),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_navigation_property_to_collection2(bool isAsync)
        {
            return AssertQuery<Level3>(
                isAsync,
                l3s => l3s.Where(l3 => l3.OneToOne_Required_FK_Inverse3.OneToMany_Optional2.Count > 0),
                l3s => l3s.Where(
                    l3 => MaybeScalar<int>(
                              l3.OneToOne_Required_FK_Inverse3.OneToMany_Optional2,
                              () => l3.OneToOne_Required_FK_Inverse3.OneToMany_Optional2.Count) > 0),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_navigation_property_to_collection_of_original_entity_type(bool isAsync)
        {
            return AssertQuery<Level2>(
                isAsync,
                l2s => l2s.Where(l2 => l2.OneToMany_Required_Inverse2.OneToMany_Optional1.Count() > 0),
                l2s => l2s.Where(
                    l2 => MaybeScalar<int>(
                              l2.OneToMany_Required_Inverse2.OneToMany_Optional1,
                              () => l2.OneToMany_Required_Inverse2.OneToMany_Optional1.Count()) > 0),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Complex_multi_include_with_order_by_and_paging(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Required_FK1, "OneToOne_Required_FK1"),
                new ExpectedInclude<Level2>(l1 => l1.OneToMany_Optional2, "OneToMany_Optional2", "OneToOne_Required_FK1"),
                new ExpectedInclude<Level2>(l1 => l1.OneToMany_Required2, "OneToMany_Required2", "OneToOne_Required_FK1")
            };

            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s
                    .Include(e => e.OneToOne_Required_FK1).ThenInclude(e => e.OneToMany_Optional2)
                    .Include(e => e.OneToOne_Required_FK1).ThenInclude(e => e.OneToMany_Required2)
                    .OrderBy(t => t.Name)
                    .Skip(0).Take(10),
                expectedIncludes,
                elementSorter: e => e.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Complex_multi_include_with_order_by_and_paging_joins_on_correct_key(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1, "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional2, "OneToMany_Optional2", "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Required_FK1, "OneToOne_Required_FK1"),
                new ExpectedInclude<Level2>(l2 => l2.OneToMany_Required2, "OneToMany_Required2", "OneToOne_Required_FK1")
            };

            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s
                    .Include(e => e.OneToOne_Optional_FK1).ThenInclude(e => e.OneToMany_Optional2)
                    .Include(e => e.OneToOne_Required_FK1).ThenInclude(e => e.OneToMany_Required2)
                    .OrderBy(t => t.Name)
                    .Skip(0).Take(10),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Complex_multi_include_with_order_by_and_paging_joins_on_correct_key2(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1, "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Required_FK2, "OneToOne_Required_FK2", "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level3>(
                    l3 => l3.OneToMany_Optional3, "OneToMany_Optional3", "OneToOne_Optional_FK1.OneToOne_Required_FK2")
            };

            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s
                    .Include(e => e.OneToOne_Optional_FK1.OneToOne_Required_FK2).ThenInclude(e => e.OneToMany_Optional3)
                    .OrderBy(t => t.Name)
                    .Skip(0).Take(10),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_include_with_multiple_optional_navigations(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Required_FK1, "OneToOne_Required_FK1"),
                new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional2, "OneToMany_Optional2", "OneToOne_Required_FK1"),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2, "OneToOne_Optional_FK2", "OneToOne_Required_FK1"),
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1, "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2, "OneToOne_Optional_FK2", "OneToOne_Optional_FK1")
            };

            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s
                    .Include(e => e.OneToOne_Required_FK1).ThenInclude(e => e.OneToMany_Optional2)
                    .Include(e => e.OneToOne_Required_FK1).ThenInclude(e => e.OneToOne_Optional_FK2)
                    .Include(e => e.OneToOne_Optional_FK1).ThenInclude(e => e.OneToOne_Optional_FK2)
                    .Where(e => e.OneToOne_Required_FK1.OneToOne_Optional_PK2.Name != "Foo")
                    .OrderBy(e => e.Id),
                l1s => l1s
                    .Where(
                        e => Maybe(
                                 e.OneToOne_Required_FK1,
                                 () => Maybe(
                                     e.OneToOne_Required_FK1.OneToOne_Optional_PK2,
                                     () => e.OneToOne_Required_FK1.OneToOne_Optional_PK2.Name)) != "Foo")
                    .OrderBy(e => e.Id),
                expectedIncludes,
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_subquery_doesnt_project_unnecessary_columns_in_top_level(bool isAsync)
        {
            return AssertQuery<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    (from l1 in l1s
                     where l2s.Any(l2 => l2.Level1_Required_Id == l1.Id)
                     select l1.Name).Distinct());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_subquery_doesnt_project_unnecessary_columns_in_top_level_join(bool isAsync)
        {
            return AssertQuery<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from e1 in l1s
                    join e2 in l2s on e1.Id equals e2.OneToOne_Optional_FK_Inverse2.Id
                    where l2s.Any(l2 => l2.Level1_Required_Id == e1.Id)
                    select new
                    {
                        Name1 = e1.Name,
                        Id2 = e2.Id
                    },
                (l1s, l2s) =>
                    from e1 in l1s
                    join e2 in l2s on e1.Id equals MaybeScalar<int>(
                        e2.OneToOne_Optional_FK_Inverse2, () => e2.OneToOne_Optional_FK_Inverse2.Id)
                    where l2s.Any(l2 => l2.Level1_Required_Id == e1.Id)
                    select new
                    {
                        Name1 = e1.Name,
                        Id2 = e2.Id
                    },
                e => e.Name1 + " " + e.Id2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_nested_subquery_doesnt_project_unnecessary_columns_in_top_level(bool isAsync)
        {
            return AssertQuery<Level1, Level2, Level3>(
                isAsync,
                (l1s, l2s, l3s) =>
                    (from l1 in l1s
                     where l2s.Any(l2 => l3s.Select(l3 => l2.Id).Any())
                     select l1.Name).Distinct()
            );
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Correlated_nested_two_levels_up_subquery_doesnt_project_unnecessary_columns_in_top_level(bool isAsync)
        {
            return AssertQuery<Level1, Level2, Level3>(
                isAsync,
                (l1s, l2s, l3s) =>
                    (from l1 in l1s
                     where l2s.Any(l2 => l3s.Select(l3 => l1.Id).Any())
                     select l1.Name).Distinct()
            );
        }

        [ConditionalTheory(Skip = "Issue #14935. Cannot eval 'Any()'")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_on_subquery_and_set_operation_on_grouping_but_nothing_from_grouping_is_projected(bool isAsync)
        {
            return AssertQuery<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    l1s.GroupJoin(
                            l2s.Where(l2 => l2.Name != "L2 01"),
                            l1 => l1.Id,
                            l2 => l2.Level1_Optional_Id,
                            (l1, l2g) => new
                            {
                                l1,
                                l2g
                            })
                        .Where(r => r.l2g.Any())
                        .Select(r => r.l1),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory(Skip = "Issue #14935. Cannot eval 'Any()'")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_on_complex_subquery_and_set_operation_on_grouping_but_nothing_from_grouping_is_projected(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s =>
                    l1s.GroupJoin(
                            l1s.Where(l1 => l1.Name != "L1 01").Select(l1 => l1.OneToOne_Required_FK1),
                            l1 => l1.Id,
                            l2 => l2 != null ? l2.Level1_Optional_Id : null,
                            (l1, l2s) => new
                            {
                                l1,
                                l2s
                            })
                        .Where(r => r.l2s.Any())
                        .Select(r => r.l1),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory(Skip = "Issue #14935. Cannot eval 'join Level2 l2 in {from Level1 l1 in value(Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryable`1[Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel.Level1]) join Level2 l1.OneToOne_Required_FK1 in value(Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryable`1[Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel.Level2]) on Property([l1], \"Id\") equals Property([l1.OneToOne_Required_FK1], \"Level1_Required_Id\") into IEnumerable`1 l1.OneToOne_Required_FK1_group from Level2 l1.OneToOne_Required_FK1 in {[l1.OneToOne_Required_FK1_group] => DefaultIfEmpty()} select [l1.OneToOne_Required_FK1]} on Convert([l1].Id, Nullable`1) equals MaybeScalar([l2], () => [l2]?.Level1_Optional_Id)'")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_protection_logic_work_for_inner_key_access_of_manually_created_GroupJoin1(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s =>
                    l1s.GroupJoin(
                            l1s.Select(l1 => l1.OneToOne_Required_FK1),
                            l1 => l1.Id,
                            l2 => MaybeScalar(l2, () => l2.Level1_Optional_Id),
                            (l1, l2s) => new
                            {
                                l1,
                                l2s
                            })
                        .Select(r => r.l1),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory(Skip = "Issue#15872")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_protection_logic_work_for_inner_key_access_of_manually_created_GroupJoin2(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s =>
                    l1s.GroupJoin(
                            l1s.Select(l1 => l1.OneToOne_Required_FK1),
                            l1 => l1.Id,
                            l2 => EF.Property<int?>(l2, "Level1_Optional_Id"),
                            (l1, l2s) => new
                            {
                                l1,
                                l2s
                            })
                        .Select(r => r.l1),
                l1s =>
                    l1s.GroupJoin(
                            l1s.Select(l1 => l1.OneToOne_Required_FK1),
                            l1 => l1.Id,
                            l2 => MaybeScalar(l2, () => l2.Level1_Optional_Id),
                            (l1, l2s) => new
                            {
                                l1,
                                l2s
                            })
                        .Select(r => r.l1),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_protection_logic_work_for_outer_key_access_of_manually_created_GroupJoin(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s =>
                    l1s.Select(l1 => l1.OneToOne_Required_FK1).GroupJoin(
                            l1s,
                            l2 => l2.Level1_Optional_Id,
                            l1 => l1.Id,
                            (l2, l1g) => new
                            {
                                l2,
                                l1g
                            })
                        .Select(r => r.l2),
                l1s =>
                    l1s.Select(l1 => l1.OneToOne_Required_FK1).GroupJoin(
                            l1s,
                            l2 => MaybeScalar(l2, () => l2.Level1_Optional_Id),
                            l1 => l1.Id,
                            (l2, l1g) => new
                            {
                                l2,
                                l1g
                            })
                        .Select(r => r.l2),
                e => e?.Id,
                (e, a) =>
                {
                    if (e == null)
                    {
                        Assert.Null(a);
                    }
                    else
                    {
                        Assert.Equal(e.Id, a.Id);
                    }
                });
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_where_with_subquery(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => l1s.SelectMany(l1 => l1.OneToMany_Required1).Where(l2 => l2.OneToMany_Required2.Any()),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access1(bool isAsync)
        {
            return AssertQuery<Level3>(
                isAsync,
                l3s => l3s.OrderBy(l3 => l3.OneToOne_Required_FK_Inverse3.Id).Select(l3 => l3.OneToOne_Required_FK_Inverse3),
                elementAsserter: (e, a) => Assert.Equal(e.Id, a.Id),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access2(bool isAsync)
        {
            return AssertQuery<Level3>(
                isAsync,
                l3s => l3s.OrderBy(l3 => l3.OneToOne_Required_FK_Inverse3.Id)
                    .Select(l3 => EF.Property<Level2>(l3, "OneToOne_Required_FK_Inverse3")),
                l3s => l3s.OrderBy(l3 => l3.OneToOne_Required_FK_Inverse3.Id).Select(l3 => l3.OneToOne_Required_FK_Inverse3),
                elementAsserter: (e, a) => Assert.Equal(e.Id, a.Id),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access3(bool isAsync)
        {
            return AssertQuery<Level3>(
                isAsync,
                l3s => l3s.OrderBy(l3 => EF.Property<Level2>(l3, "OneToOne_Required_FK_Inverse3").Id)
                    .Select(l3 => l3.OneToOne_Required_FK_Inverse3),
                l3s => l3s.OrderBy(l3 => l3.OneToOne_Required_FK_Inverse3.Id).Select(l3 => l3.OneToOne_Required_FK_Inverse3),
                elementAsserter: (e, a) => Assert.Equal(e.Id, a.Id),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Order_by_key_of_navigation_similar_to_projected_gets_optimized_into_FK_access(bool isAsync)
        {
            return AssertQuery<Level3>(
                isAsync,
                l3s => from l3 in l3s
                       orderby l3.OneToOne_Required_FK_Inverse3.Id
                       select l3.OneToOne_Required_FK_Inverse3.OneToOne_Required_FK_Inverse2,
                elementAsserter: (e, a) => Assert.Equal(e.Id, a.Id),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access_subquery(bool isAsync)
        {
            return AssertQuery<Level3>(
                isAsync,
                l3s => l3s
                    .Select(l3 => l3.OneToOne_Required_FK_Inverse3)
                    .OrderBy(l2 => l2.Id)
                    .Take(10)
                    .Select(l2 => l2.OneToOne_Required_FK_Inverse2.Name),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Order_by_key_of_anonymous_type_projected_navigation_doesnt_get_optimized_into_FK_access_subquery(bool isAsync)
        {
            return AssertQuery<Level3>(
                isAsync,
                l3s => l3s
                    .Select(
                        l3 => new
                        {
                            l3.OneToOne_Required_FK_Inverse3,
                            name = l3.Name
                        })
                    .OrderBy(l3 => l3.OneToOne_Required_FK_Inverse3.Id)
                    .Take(10)
                    .Select(l2 => l2.OneToOne_Required_FK_Inverse3.Name),
                assertOrder: true);
        }

        [ConditionalTheory(Skip = "Issue#15872")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_take_optional_navigation(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => l1s
                    .Select(l1 => l1.OneToOne_Optional_FK1)
                    .OrderBy(l2 => (int?)l2.Id)
                    .Take(10)
                    .Select(l2 => l2.OneToOne_Optional_FK2.Name),
                l1s => l1s
                    .Select(l1 => l1.OneToOne_Optional_FK1)
                    .OrderBy(l2 => MaybeScalar<int>(l2, () => l2.Id))
                    .Take(10)
                    .Select(l2 => Maybe(l2, () => Maybe(l2.OneToOne_Optional_FK2, () => l2.OneToOne_Optional_FK2.Name))),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Projection_select_correct_table_from_subquery_when_materialization_is_not_required(bool isAsync)
        {
            return AssertQuery<Level2>(
                isAsync,
                l2s => l2s.Where(l2 => l2.OneToOne_Required_FK_Inverse2.Name == "L1 03")
                    .OrderBy(l => l.Id).Take(3).Select(l2 => l2.Name));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Projection_select_correct_table_with_anonymous_projection_in_subquery(bool isAsync)
        {
            return AssertQuery<Level1, Level2, Level3>(
                isAsync,
                (l1s, l2s, l3s) =>
                    (from l2 in l2s
                     join l1 in l1s
                         on l2.Level1_Required_Id equals l1.Id
                     join l3 in l3s
                         on l1.Id equals l3.Level2_Required_Id
                     where l1.Name == "L1 03"
                     where l3.Name == "L3 08"
                     select new
                     {
                         l2,
                         l1
                     })
                    .OrderBy(l => l.l1.Id)
                    .Take(3)
                    .Select(l => l.l2.Name)
            );
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Projection_select_correct_table_in_subquery_when_materialization_is_not_required_in_multiple_joins(bool isAsync)
        {
            return AssertQuery<Level1, Level2, Level3>(
                isAsync,
                (l1s, l2s, l3s) =>
                    (from l2 in l2s
                     join l1 in l1s
                         on l2.Level1_Required_Id equals l1.Id
                     join l3 in l3s
                         on l1.Id equals l3.Level2_Required_Id
                     where l1.Name == "L1 03"
                     where l3.Name == "L3 08"
                     select l1).OrderBy(l1 => l1.Id).Take(3).Select(l1 => l1.Name)
            );
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_predicate_on_optional_reference_navigation(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => l1s
                    .Where(l1 => l1.OneToOne_Required_FK1.Name == "L2 03")
                    .OrderBy(l1 => l1.Id)
                    .Take(3)
                    .Select(l1 => l1.Name),
                l1s => l1s
                    .Where(l1 => Maybe(l1.OneToOne_Required_FK1, () => l1.OneToOne_Required_FK1.Name) == "L2 03")
                    .OrderBy(l1 => l1.Id)
                    .Take(3)
                    .Select(l1 => l1.Name));
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_with_Include1(bool isAsync)
        {
            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s
                    .SelectMany(l1 => l1.OneToMany_Optional1)
                    .Include(l2 => l2.OneToMany_Optional2),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional2, "OneToMany_Optional2")
                },
                elementSorter: e => e.Id);
        }

        // issue #12794
        //[ConditionalTheory]
        //[MemberData(nameof(IsAsyncData))]
        public virtual Task Orderby_SelectMany_with_Include1(bool isAsync)
        {
            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s.OrderBy(l1 => l1.Id)
                    .SelectMany(l1 => l1.OneToMany_Optional1)
                    .Include(l2 => l2.OneToMany_Optional2),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional2, "OneToMany_Optional2")
                },
                elementSorter: e => e.Id);
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_with_Include2(bool isAsync)
        {
            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s
                    .SelectMany(l1 => l1.OneToMany_Optional1)
                    .Include(l2 => l2.OneToOne_Required_FK2),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level2>(l2 => l2.OneToOne_Required_FK2, "OneToOne_Required_FK2")
                },
                elementSorter: e => e.Id);
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_with_Include_ThenInclude(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Required_FK2, "OneToOne_Required_FK2"),
                new ExpectedInclude<Level3>(l3 => l3.OneToMany_Optional3, "OneToMany_Optional3", "OneToOne_Required_FK2")
            };

            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s
                    .SelectMany(l1 => l1.OneToMany_Optional1)
                    .Include(l2 => l2.OneToOne_Required_FK2)
                    .ThenInclude(l3 => l3.OneToMany_Optional3),
                expectedIncludes,
                elementSorter: e => e.Id);
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_SelectMany_with_Include(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level3>(l3 => l3.OneToOne_Required_FK3, "OneToOne_Required_FK3"),
                new ExpectedInclude<Level3>(l3 => l3.OneToMany_Optional3, "OneToMany_Optional3")
            };

            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s
                    .SelectMany(l1 => l1.OneToMany_Optional1)
                    .SelectMany(l2 => l2.OneToMany_Optional2)
                    .Include(l3 => l3.OneToOne_Required_FK3)
                    .Include(l3 => l3.OneToMany_Optional3),
                expectedIncludes,
                elementSorter: e => e.Id);
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_with_string_based_Include1(bool isAsync)
        {
            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s
                    .SelectMany(l1 => l1.OneToMany_Optional1)
                    .Include("OneToOne_Required_FK2"),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level2>(l2 => l2.OneToOne_Required_FK2, "OneToOne_Required_FK2")
                },
                elementSorter: e => e.Id);
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_with_string_based_Include2(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Required_FK2, "OneToOne_Required_FK2"),
                new ExpectedInclude<Level3>(l3 => l3.OneToOne_Required_FK3, "OneToOne_Required_FK3", "OneToOne_Required_FK2")
            };

            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s
                    .SelectMany(l1 => l1.OneToMany_Optional1)
                    .Include("OneToOne_Required_FK2.OneToOne_Required_FK3"),
                expectedIncludes,
                elementSorter: e => e.Id);
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_SelectMany_with_string_based_Include(bool isAsync)
        {
            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s
                    .SelectMany(l1 => l1.OneToMany_Optional1)
                    .SelectMany(l1 => l1.OneToMany_Optional2)
                    .Include("OneToOne_Required_FK3"),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level3>(l3 => l3.OneToOne_Required_FK3, "OneToOne_Required_FK3")
                },
                elementSorter: e => e.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Required_navigation_with_Include(bool isAsync)
        {
            return AssertIncludeQuery<Level3>(
                isAsync,
                l3s => l3s
                    .Select(l3 => l3.OneToOne_Required_FK_Inverse3)
                    .Include(l2 => l2.OneToMany_Required_Inverse2),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level2>(l2 => l2.OneToMany_Required_Inverse2, "OneToMany_Required_Inverse2")
                },
                elementSorter: e => e.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Required_navigation_with_Include_ThenInclude(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level3>(l3 => l3.OneToMany_Required_Inverse3, "OneToMany_Required_Inverse3"),
                new ExpectedInclude<Level2>(
                    l2 => l2.OneToMany_Optional_Inverse2, "OneToMany_Optional_Inverse2", "OneToMany_Required_Inverse3")
            };

            return AssertIncludeQuery<Level4>(
                isAsync,
                l4s => l4s
                    .Select(l4 => l4.OneToOne_Required_FK_Inverse4)
                    .Include(l3 => l3.OneToMany_Required_Inverse3)
                    .ThenInclude(l2 => l2.OneToMany_Optional_Inverse2),
                expectedIncludes,
                elementSorter: e => e.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_required_navigations_with_Include(bool isAsync)
        {
            return AssertIncludeQuery<Level4>(
                isAsync,
                l4s => l4s
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
        public virtual Task Multiple_required_navigation_using_multiple_selects_with_Include(bool isAsync)
        {
            return AssertIncludeQuery<Level4>(
                isAsync,
                l4s => l4s
                    .Select(l4 => l4.OneToOne_Required_FK_Inverse4)
                    .Select(l3 => l3.OneToOne_Required_FK_Inverse3)
                    .Include(l2 => l2.OneToOne_Optional_FK2),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2, "OneToOne_Optional_FK2")
                },
                elementSorter: e => e.Id);
        }

        [ConditionalTheory(Skip = "Issue#16090")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_required_navigation_with_string_based_Include(bool isAsync)
        {
            return AssertIncludeQuery<Level4>(
                isAsync,
                l4s => l4s
                    .Select(l4 => l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3)
                    .Include("OneToOne_Optional_FK2"),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2, "OneToOne_Optional_FK2")
                },
                elementSorter: e => e.Id);
        }

        [ConditionalTheory(Skip = "Issue#16090")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_required_navigation_using_multiple_selects_with_string_based_Include(bool isAsync)
        {
            return AssertIncludeQuery<Level4>(
                isAsync,
                l4s => l4s
                    .Select(l4 => l4.OneToOne_Required_FK_Inverse4)
                    .Select(l3 => l3.OneToOne_Required_FK_Inverse3)
                    .Include("OneToOne_Optional_FK2"),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2, "OneToOne_Optional_FK2")
                },
                elementSorter: e => e.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_with_Include(bool isAsync)
        {
            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s
                    .Select(l1 => l1.OneToOne_Optional_FK1)
                    .Include(l2 => l2.OneToOne_Optional_FK2),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2, "OneToOne_Optional_FK2")
                },
                elementSorter: e => e != null ? e.Id : 0);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_with_Include_ThenInclude(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional2, "OneToMany_Optional2"),
                new ExpectedInclude<Level3>(l3 => l3.OneToOne_Optional_FK3, "OneToOne_Optional_FK3", "OneToMany_Optional2")
            };

            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s
                    .Select(l1 => l1.OneToOne_Optional_FK1)
                    .Include(l2 => l2.OneToMany_Optional2)
                    .ThenInclude(l3 => l3.OneToOne_Optional_FK3),
                expectedIncludes,
                elementSorter: e => e != null ? e.Id : 0);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_optional_navigation_with_Include(bool isAsync)
        {
            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s
                    .Select(l1 => l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2)
                    .Include(l3 => l3.OneToMany_Optional3),
                l1s => l1s
                    .Select(l1 => Maybe(l1.OneToOne_Optional_FK1, () => l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2)),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level3>(l3 => l3.OneToMany_Optional3, "OneToMany_Optional3")
                },
                elementSorter: e => e != null ? e.Id : 0);
        }

        [ConditionalTheory(Skip = "Issue#16090")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_optional_navigation_with_string_based_Include(bool isAsync)
        {
            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s
                    .Select(l1 => l1.OneToOne_Optional_FK1)
                    .Select(l2 => l2.OneToOne_Optional_PK2)
                    .Include("OneToMany_Optional3"),
                l1s => l1s
                    .Select(l1 => l1.OneToOne_Optional_FK1)
                    .Select(l2 => Maybe(l2, () => l2.OneToOne_Optional_PK2)),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level3>(l3 => l3.OneToMany_Optional3, "OneToMany_Optional3")
                },
                elementSorter: e => e != null ? e.Id : 0);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_with_order_by_and_Include(bool isAsync)
        {
            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s
                    .Select(l1 => l1.OneToOne_Optional_FK1)
                    .OrderBy(l2 => l2.Name)
                    .Include(l2 => l2.OneToMany_Optional2),
                l1s => l1s
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
        public virtual Task Optional_navigation_with_Include_and_order(bool isAsync)
        {
            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s
                    .Select(l1 => l1.OneToOne_Optional_FK1)
                    .Include(l2 => l2.OneToMany_Optional2)
                    .OrderBy(l2 => l2.Name),
                l1s => l1s
                    .Select(l1 => l1.OneToOne_Optional_FK1)
                    .OrderBy(l2 => Maybe(l2, () => l2.Name)),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional2, "OneToMany_Optional2")
                },
                assertOrder: true);
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_with_order_by_and_Include(bool isAsync)
        {
            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s
                    .SelectMany(l1 => l1.OneToMany_Optional1)
                    .OrderBy(l2 => l2.Name)
                    .Include(l2 => l2.OneToMany_Optional2),
                l1s => l1s
                    .SelectMany(l1 => l1.OneToMany_Optional1)
                    .OrderBy(l2 => Maybe(l2, () => l2.Name)),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional2, "OneToMany_Optional2")
                },
                assertOrder: true);
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_with_Include_and_order_by(bool isAsync)
        {
            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s
                    .SelectMany(l1 => l1.OneToMany_Optional1)
                    .Include(l2 => l2.OneToMany_Optional2)
                    .OrderBy(l2 => l2.Name),
                l1s => l1s
                    .SelectMany(l1 => l1.OneToMany_Optional1)
                    .OrderBy(l2 => Maybe(l2, () => l2.Name)),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional2, "OneToMany_Optional2")
                },
                assertOrder: true);
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_with_navigation_and_explicit_DefaultIfEmpty(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s =>
                    from l1 in l1s
                    from l2 in l1.OneToMany_Optional1.DefaultIfEmpty()
                    where l2 != null
                    select l1,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory(Skip = "Issue #14935. Cannot eval 'Distict()'")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_with_navigation_and_Distinct(bool isAsync)
        {
            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => from l1 in l1s.Include(l => l.OneToMany_Optional1)
                       from l2 in l1.OneToMany_Optional1.Distinct()
                       where l2 != null
                       select l1,
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level1>(l1 => l1.OneToMany_Optional1, "OneToMany_Optional1")
                },
                elementSorter: e => e.Id);
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_with_navigation_filter_and_explicit_DefaultIfEmpty(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => from l1 in l1s
                       from l2 in l1.OneToMany_Optional1.Where(l => l.Id > 5).DefaultIfEmpty()
                       where l2 != null
                       select l1,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_with_nested_navigation_and_explicit_DefaultIfEmpty(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s =>
                    from l1 in l1s
                    from l3 in l1.OneToOne_Required_FK1.OneToMany_Optional2.DefaultIfEmpty()
                    where l3 != null
                    select l1,
                l1s =>
                    from l1 in l1s
                    from l3 in Maybe(
                                   l1.OneToOne_Required_FK1,
                                   () => l1.OneToOne_Required_FK1.OneToMany_Optional2.DefaultIfEmpty()) ?? new List<Level3>()
                    where l3 != null
                    select l1,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_with_nested_navigation_filter_and_explicit_DefaultIfEmpty(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s =>
                    from l1 in l1s
                    from l3 in l1.OneToOne_Optional_FK1.OneToMany_Optional2.Where(l => l.Id > 5).DefaultIfEmpty()
                    where l3 != null
                    select l1,
                l1s =>
                    from l1 in l1s.Where(l => l.OneToOne_Optional_FK1 != null)
                    from l3 in Maybe(
                        l1.OneToOne_Optional_FK1,
                        () => l1.OneToOne_Optional_FK1.OneToMany_Optional2.Where(l => l.Id > 5).DefaultIfEmpty())
                    where l3 != null
                    select l1,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_with_nested_required_navigation_filter_and_explicit_DefaultIfEmpty(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s =>
                    from l1 in l1s
                    from l3 in l1.OneToOne_Required_FK1.OneToMany_Required2.Where(l => l.Id > 5).DefaultIfEmpty()
                    where l3 != null
                    select l1,
                l1s =>
                    from l1 in l1s.Where(l => l.OneToOne_Required_FK1 != null)
                    from l3 in Maybe(
                        l1.OneToOne_Required_FK1,
                        () => l1.OneToOne_Required_FK1.OneToMany_Required2.Where(l => l.Id > 5).DefaultIfEmpty())
                    where l3 != null
                    select l1,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact(Skip = "Issue#15711")]
        public virtual void SelectMany_with_nested_navigations_and_additional_joins_outside_of_SelectMany()
        {
            using (var ctx = CreateContext())
            {
                var query = from l1 in ctx.LevelOne
                            join l2 in ctx.LevelFour.SelectMany(
                                    l4 => l4.OneToOne_Required_FK_Inverse4.OneToOne_Optional_FK_Inverse3.OneToMany_Required_Self2) on l1.Id
                                equals l2.Level1_Optional_Id
                            select new { l1, l2 };

                var result = query.ToList();

                Assert.Equal(2, result.Count);
            }
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany(
            bool isAsync)
        {
            return AssertQuery<Level1, Level4>(
                isAsync,
                (l1s, l4s)
                    => from l1 in l1s
                       join l2 in l4s.SelectMany(
                               l4 => l4.OneToOne_Required_FK_Inverse4.OneToOne_Optional_FK_Inverse3.OneToMany_Required_Self2
                                   .DefaultIfEmpty())
                           on l1.Id equals l2.Level1_Optional_Id
                       select new { l1, l2 },
                (l1s, l4s)
                    => from l1 in l1s
                       join l2 in l4s.SelectMany(
                               l4 => MaybeDefaultIfEmpty(
                                   Maybe(
                                       l4.OneToOne_Required_FK_Inverse4,
                                       () => Maybe(
                                           l4.OneToOne_Required_FK_Inverse4.OneToOne_Optional_FK_Inverse3,
                                           () => l4.OneToOne_Required_FK_Inverse4.OneToOne_Optional_FK_Inverse3
                                               .OneToMany_Required_Self2)))) on
                           l1.Id equals MaybeScalar(l2, () => l2.Level1_Optional_Id)
                       select new { l1, l2 },
                elementSorter: e => e.l1?.Id + " " + e.l2?.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.l1.Id, a.l1.Id);
                    Assert.Equal(e.l2.Id, a.l2.Id);
                });
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany2(
            bool isAsync)
        {
            return AssertQuery<Level4, Level1>(
                isAsync,
                (l4s, l1s)
                    => from l2 in l4s.SelectMany(
                           l4 => l4.OneToOne_Required_FK_Inverse4.OneToOne_Optional_FK_Inverse3.OneToMany_Required_Self2.DefaultIfEmpty())
                       join l1 in l1s on l2.Level1_Optional_Id equals l1.Id
                       select new { l2, l1 },
                (l4s, l1s)
                    => from l2 in l4s.SelectMany(
                           l4 => MaybeDefaultIfEmpty(
                               Maybe(
                                   l4.OneToOne_Required_FK_Inverse4,
                                   () => Maybe(
                                       l4.OneToOne_Required_FK_Inverse4.OneToOne_Optional_FK_Inverse3,
                                       () => l4.OneToOne_Required_FK_Inverse4.OneToOne_Optional_FK_Inverse3.OneToMany_Required_Self2))))
                       join l1 in l1s on MaybeScalar(l2, () => l2.Level1_Optional_Id) equals l1.Id
                       select new { l2, l1 },
                elementSorter: e => e.l2?.Id + " " + e.l1?.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.l2.Id, a.l2.Id);
                    Assert.Equal(e.l1.Id, a.l1.Id);
                });
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany3(
            bool isAsync)
        {
            return AssertQuery<Level1, Level2>(
                isAsync,
                (l1s, l2s)
                    => from l4 in l1s.SelectMany(l1 => l1.OneToOne_Required_FK1.OneToOne_Optional_FK2.OneToMany_Required3.DefaultIfEmpty())
                       join l2 in l2s on l4.Id equals l2.Id
                       select new { l4, l2 },
                (l1s, l2s)
                    => from l4 in l1s.SelectMany(
                           l1 => MaybeDefaultIfEmpty(
                               Maybe(
                                   l1.OneToOne_Required_FK1,
                                   () => Maybe(
                                       l1.OneToOne_Required_FK1.OneToOne_Optional_FK2,
                                       () => l1.OneToOne_Required_FK1.OneToOne_Optional_FK2.OneToMany_Required3))))
                       join l2 in l2s on MaybeScalar<int>(l4, () => l4.Id) equals l2.Id
                       select new { l4, l2 },
                elementSorter: e => e.l4?.Id + " " + e.l2?.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.l4.Id, a.l4.Id);
                    Assert.Equal(e.l2.Id, a.l2.Id);
                });
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany4(
            bool isAsync)
        {
            return AssertQuery<Level1, Level2>(
                isAsync,
                (l1s, l2s)
                    => from l4 in l1s.SelectMany(l1 => l1.OneToOne_Required_FK1.OneToOne_Optional_FK2.OneToMany_Required3.DefaultIfEmpty())
                       join l2 in l2s on l4.Id equals l2.Id into grouping
                       from l2 in grouping.DefaultIfEmpty()
                       select new { l4, l2 },
                (l1s, l2s)
                    => from l4 in l1s.SelectMany(
                           l1 => MaybeDefaultIfEmpty(
                               Maybe(
                                   l1.OneToOne_Required_FK1,
                                   () => Maybe(
                                       l1.OneToOne_Required_FK1.OneToOne_Optional_FK2,
                                       () => l1.OneToOne_Required_FK1.OneToOne_Optional_FK2.OneToMany_Required3))))
                       join l2 in l2s on MaybeScalar<int>(l4, () => l4.Id) equals l2.Id into grouping
                       from l2 in grouping.DefaultIfEmpty()
                       select new { l4, l2 },
                elementSorter: e => e.l4?.Id + " " + e.l2?.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.l4?.Id, a.l4?.Id);
                    Assert.Equal(e.l2?.Id, a.l2?.Id);
                });
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_joined_together(bool isAsync)
        {
            return AssertQuery<Level1, Level4>(
                isAsync,
                (l1s, l4s)
                    => from l4 in l1s.SelectMany(l1 => l1.OneToOne_Required_FK1.OneToOne_Optional_FK2.OneToMany_Required3.DefaultIfEmpty())
                       join l2 in l4s.SelectMany(
                               l4 => l4.OneToOne_Required_FK_Inverse4.OneToOne_Optional_FK_Inverse3.OneToMany_Required_Self2
                                   .DefaultIfEmpty())
                           on l4.Id equals l2.Id
                       select new { l4, l2 },
                (l1s, l4s)
                    => from l4 in l1s.SelectMany(
                           l1 => MaybeDefaultIfEmpty(
                               Maybe(
                                   l1.OneToOne_Required_FK1,
                                   () => Maybe(
                                       l1.OneToOne_Required_FK1.OneToOne_Optional_FK2,
                                       () => l1.OneToOne_Required_FK1.OneToOne_Optional_FK2.OneToMany_Required3))))
                       join l2 in l4s.SelectMany(
                               l4 => MaybeDefaultIfEmpty(
                                   Maybe(
                                       l4.OneToOne_Required_FK_Inverse4,
                                       () => Maybe(
                                           l4.OneToOne_Required_FK_Inverse4.OneToOne_Optional_FK_Inverse3,
                                           () => l4.OneToOne_Required_FK_Inverse4.OneToOne_Optional_FK_Inverse3
                                               .OneToMany_Required_Self2)))) on
                           MaybeScalar<int>(l4, () => l4.Id) equals MaybeScalar<int>(l2, () => l2.Id)
                       select new { l4, l2 },
                elementSorter: e => e.l4?.Id + " " + e.l2?.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.l4.Id, a.l4.Id);
                    Assert.Equal(e.l2.Id, a.l2.Id);
                });
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task
            SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_followed_by_Select_required_navigation_using_same_navs(
                bool isAsync)
        {
            return AssertQuery<Level4>(
                isAsync,
                l4s => from l3 in l4s.SelectMany(
                           l4 => l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToMany_Required2.DefaultIfEmpty())
                       select l3.OneToOne_Required_FK_Inverse3.OneToOne_Required_PK_Inverse2,
                l4s => from l3 in l4s.SelectMany(
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

        [ConditionalTheory(Skip = "Issue#15711")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task
            SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_followed_by_Select_required_navigation_using_different_navs(
                bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => from l3 in l1s.SelectMany(l1 => l1.OneToOne_Optional_FK1.OneToMany_Optional2.DefaultIfEmpty())
                       select l3.OneToOne_Required_FK_Inverse3.OneToOne_Required_PK_Inverse2,
                l1s => from l3 in l1s.SelectMany(
                           l1 => MaybeDefaultIfEmpty(
                               Maybe(
                                   l1.OneToOne_Optional_FK1,
                                   () => l1.OneToOne_Optional_FK1.OneToMany_Optional2)))
                       select Maybe(
                           l3,
                           () => l3.OneToOne_Required_FK_Inverse3.OneToOne_Required_PK_Inverse2));
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task
            Complex_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_with_other_query_operators_composed_on_top(bool isAsync)
        {
            return AssertQuery<Level1, Level4>(
                isAsync,
                (l1s, l4s)
                    => from l4 in l1s.SelectMany(l1 => l1.OneToOne_Required_FK1.OneToOne_Optional_FK2.OneToMany_Required3.DefaultIfEmpty())
                       join l2 in l4s.SelectMany(
                               l4 => l4.OneToOne_Required_FK_Inverse4.OneToOne_Optional_FK_Inverse3.OneToMany_Required_Self2
                                   .DefaultIfEmpty())
                           on l4.Id equals l2.Id
                       join l3 in l4s.SelectMany(
                               l4 => l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToMany_Required2.DefaultIfEmpty()) on
                           l2.Id equals l3.Id into grouping
                       from l3 in grouping.DefaultIfEmpty()
                       where l4.OneToMany_Optional_Inverse4.Name != "Foo"
                       orderby l2.OneToOne_Optional_FK2.Id
                       select new
                       {
                           Entity = l4, Collection = l2.OneToMany_Optional_Self2.Where(e => e.Id != 42).ToList(),
                           Property = l3.OneToOne_Optional_FK_Inverse3.OneToOne_Required_FK2.Name
                       },
                (l1s, l4s)
                    => from l4 in l1s.SelectMany(
                           l1 => MaybeDefaultIfEmpty(
                               Maybe(
                                   l1.OneToOne_Required_FK1,
                                   () => Maybe(
                                       l1.OneToOne_Required_FK1.OneToOne_Optional_FK2,
                                       () => l1.OneToOne_Required_FK1.OneToOne_Optional_FK2.OneToMany_Required3))))
                       join l2 in l4s.SelectMany(
                               l4 => MaybeDefaultIfEmpty(
                                   Maybe(
                                       l4.OneToOne_Required_FK_Inverse4,
                                       () => Maybe(
                                           l4.OneToOne_Required_FK_Inverse4.OneToOne_Optional_FK_Inverse3,
                                           () => l4.OneToOne_Required_FK_Inverse4.OneToOne_Optional_FK_Inverse3
                                               .OneToMany_Required_Self2)))) on
                           MaybeScalar<int>(l4, () => l4.Id) equals MaybeScalar<int>(l2, () => l2.Id)
                       join l3 in l4s.SelectMany(
                               l4 => MaybeDefaultIfEmpty(
                                   Maybe(
                                       l4.OneToOne_Required_FK_Inverse4,
                                       () => Maybe(
                                           l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3,
                                           () => l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToMany_Required2)))) on
                           MaybeScalar<int>(l2, () => l2.Id) equals MaybeScalar<int>(l3, () => l3.Id) into grouping
                       from l3 in grouping.DefaultIfEmpty()
                       where Maybe(
                                 l4,
                                 () => Maybe(
                                     l4.OneToMany_Optional_Inverse4,
                                     () => l4.OneToMany_Optional_Inverse4.Name)) != "Foo"
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
                    Assert.Equal(e.Entity.Id, a.Entity.Id);
                    CollectionAsserter<Level2>(ee => ee.Id, (ee, aa) => Assert.Equal(ee.Id, aa.Id))(e.Collection, a.Collection);
                    Assert.Equal(e.Property, a.Property);
                });
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_SelectMany_with_navigation_and_explicit_DefaultIfEmpty(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => from l1 in l1s
                       from l2 in l1.OneToMany_Optional1
                       from l3 in l2.OneToMany_Optional2.Where(l => l.Id > 5).DefaultIfEmpty()
                       where l3 != null
                       select l1,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory(Skip = "Issue #14935. Cannot eval 'where ([l1.OneToMany_Required1_groupItem].Id > 5)'")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_with_navigation_filter_paging_and_explicit_DefaultIfEmpty(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => from l1 in l1s
                       from l2 in l1.OneToMany_Required1.Where(l => l.Id > 5).OrderBy(l => l.Id).Take(3).DefaultIfEmpty()
                       where l2 != null
                       select l1,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_join_subquery_containing_filter_and_distinct(bool isAsync)
        {
            return AssertQuery<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from l1 in l1s
                    join l2 in l2s.Where(l => l.Id > 2).Distinct() on l1.Id equals l2.Level1_Optional_Id
                    select new
                    {
                        l1,
                        l2
                    },
                elementSorter: e => e.l1.Id + " " + e.l2.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.l1.Id, a.l1.Id);
                    Assert.Equal(e.l2.Id, a.l2.Id);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_join_with_key_selector_being_a_subquery(bool isAsync)
        {
            return AssertQuery<Level1, Level2>(
                isAsync,
                (l1s, l2s) => from l1 in l1s
                              join l2 in l2s on l1.Id equals l2s.Select(l => l.Id).OrderBy(l => l).FirstOrDefault()
                              select new
                              {
                                  l1,
                                  l2
                              },
                elementSorter: e => e.l1.Id + " " + e.l2.Id,
                elementAsserter: (e, a) => Assert.Equal(e.l1.Name + " " + e.l2.Name, a.l1.Name + " " + a.l2.Name));
        }

        [ConditionalTheory(Skip = " Issue#16093")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_subquery_optional_navigation_and_constant_item(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => l1s.Where(l1 => l1.OneToOne_Optional_FK1.OneToMany_Optional2.Distinct().Select(l3 => l3.Id).Contains(1)),
                l1s => l1s.Where(
                    l1 => MaybeScalar<bool>(
                              l1.OneToOne_Optional_FK1,
                              () => l1.OneToOne_Optional_FK1.OneToMany_Optional2.Distinct().Select(l3 => l3.Id).Contains(1)) == true),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory(Skip = "Issue #14935. Cannot eval 'All(ClientMethod([l2.OneToOne_Optional_FK2.OneToOne_Optional_FK3]?.Id))'")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Complex_query_with_optional_navigations_and_client_side_evaluation(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => l1s.Where(
                    l1 => l1.Id < 3 && !l1.OneToMany_Optional1.Select(l2 => l2.OneToOne_Optional_FK2.OneToOne_Optional_FK3.Id)
                              .All(l4 => ClientMethod(l4))),
                l1s => l1s.Where(
                    l1 => l1.Id < 3 && !l1.OneToMany_Optional1.Select(
                              l2 => MaybeScalar(
                                  l2.OneToOne_Optional_FK2,
                                  () => MaybeScalar<int>(
                                      l2.OneToOne_Optional_FK2.OneToOne_Optional_FK3,
                                      () => l2.OneToOne_Optional_FK2.OneToOne_Optional_FK3.Id))).All(a => true)),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory(Skip = "Issue #14935. Cannot eval 'First()'")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Required_navigation_on_a_subquery_with_First_in_projection(bool isAsync)
        {
            return AssertQuery<Level2>(
                isAsync,
                l2s => l2s
                    .Where(l2o => l2o.Id == 7)
                    .Select(l2o => l2s.OrderBy(l2i => l2i.Id).First().OneToOne_Required_FK_Inverse2.Name));
        }

        [ConditionalTheory(Skip = "Issue #14935. Cannot eval 'First()'")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Required_navigation_on_a_subquery_with_complex_projection_and_First(bool isAsync)
        {
            return AssertQuery<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from l2o in l2s
                    where l2o.Id == 7
                    select
                        (from l2i in l2s
                         join l1i in l1s
                             on l2i.Level1_Required_Id equals l1i.Id
                         orderby l2i.Id
                         select new
                         {
                             Navigation = l2i.OneToOne_Required_FK_Inverse2,
                             Constant = 7
                         }).First().Navigation.Name);
        }

        [ConditionalTheory(Skip = "Issue #14935. Cannot eval 'First()'")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Required_navigation_on_a_subquery_with_First_in_predicate(bool isAsync)
        {
            return AssertQuery<Level2>(
                isAsync,
                l2s => l2s
                    .Where(l2o => l2o.Id == 7)
                    .Where(l1 => EF.Property<string>(l2s.OrderBy(l2i => l2i.Id).First().OneToOne_Required_FK_Inverse2, "Name") == "L1 02"),
                l2s => l2s
                    .Where(l2o => l2o.Id == 7)
                    .Where(l1 => l2s.OrderBy(l2i => l2i.Id).First().OneToOne_Required_FK_Inverse2.Name == "L1 02"));
        }

        [ConditionalTheory(Skip = "Issue#16088")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Manually_created_left_join_propagates_nullability_to_navigations(bool isAsync)
        {
            return AssertQuery<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from l1_manual in l1s
                    join l2_manual in l2s on l1_manual.Id equals l2_manual.Level1_Optional_Id into grouping
                    from l2_manual in grouping.DefaultIfEmpty()
                    where l2_manual.OneToOne_Required_FK_Inverse2.Name != "L3 02"
                    select l2_manual.OneToOne_Required_FK_Inverse2.Name,
                (l1s, l2s) =>
                    from l1_manual in l1s
                    join l2_manual in l2s on l1_manual.Id equals l2_manual.Level1_Optional_Id into grouping
                    from l2_manual in grouping.DefaultIfEmpty()
                    where Maybe(l2_manual, () => l2_manual.OneToOne_Required_FK_Inverse2.Name) != "L3 02"
                    select Maybe(l2_manual, () => l2_manual.OneToOne_Required_FK_Inverse2.Name));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_propagates_nullability_to_manually_created_left_join1(bool isAsync)
        {
            return AssertQuery<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from l2_nav in l1s.Select(ll => ll.OneToOne_Optional_FK1)
                    join l1 in l2s on l2_nav.Level1_Required_Id equals l1.Id into grouping
                    from l1 in grouping.DefaultIfEmpty()
                    select new
                    {
                        Id1 = (int?)l2_nav.Id,
                        Id2 = (int?)l1.Id
                    },
                (l1s, l2s) =>
                    from l2_nav in l1s.Select(ll => ll.OneToOne_Optional_FK1)
                    join l1 in l2s on MaybeScalar<int>(l2_nav, () => l2_nav.Level1_Required_Id) equals l1.Id into grouping
                    from l1 in grouping.DefaultIfEmpty()
                    select new
                    {
                        Id1 = MaybeScalar<int>(l2_nav, () => l2_nav.Id),
                        Id2 = MaybeScalar<int>(l1, () => l1.Id)
                    },
                elementSorter: e => e.Id1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_propagates_nullability_to_manually_created_left_join2(bool isAsync)
        {
            return AssertQuery<Level3, Level1>(
                isAsync,
                (l3s, l1s) =>
                    from l3 in l3s
                    join l2_nav in l1s.Select(ll => ll.OneToOne_Optional_FK1) on l3.Level2_Required_Id equals l2_nav.Id into grouping
                    from l2_nav in grouping.DefaultIfEmpty()
                    select new
                    {
                        Name1 = l3.Name,
                        Name2 = l2_nav.Name
                    },
                (l3s, l1s) =>
                    from l3 in l3s
                    join l2_nav in l1s.Select(ll => ll.OneToOne_Optional_FK1) on l3.Level2_Required_Id equals MaybeScalar<int>(
                        l2_nav, () => l2_nav.Id) into grouping
                    from l2_nav in grouping.DefaultIfEmpty()
                    select new
                    {
                        Name1 = l3.Name,
                        Name2 = Maybe(l2_nav, () => l2_nav.Name)
                    },
                elementSorter: e => e.Name1 + e.Name2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_reference_protection_complex(bool isAsync)
        {
            return AssertQuery<Level1, Level2, Level3>(
                isAsync,
                (l1s, l2s, l3s) =>
                    from l3 in l3s
                    join l2_outer in
                        (from l1_inner in l1s
                         join l2_inner in l2s on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping_inner
                         from l2_inner in grouping_inner.DefaultIfEmpty()
                         select l2_inner)
                        on l3.Level2_Required_Id equals l2_outer.Id into grouping_outer
                    from l2_outer in grouping_outer.DefaultIfEmpty()
                    select l2_outer.Name,
                (l1s, l2s, l3s) =>
                    from l3 in l3s
                    join l2_outer in
                        (from l1_inner in l1s
                         join l2_inner in l2s on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping_inner
                         from l2_inner in grouping_inner.DefaultIfEmpty()
                         select l2_inner)
                        on l3.Level2_Required_Id equals MaybeScalar<int>(l2_outer, () => l2_outer.Id) into grouping_outer
                    from l2_outer in grouping_outer.DefaultIfEmpty()
                    select Maybe(l2_outer, () => l2_outer.Name));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_reference_protection_complex_materialization(bool isAsync)
        {
            return AssertQuery<Level1, Level2, Level3>(
                isAsync,
                (l1s, l2s, l3s) =>
                    from l3 in l3s
                    join l2_outer in
                        (from l1_inner in l1s
                         join l2_inner in l2s on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping_inner
                         from l2_inner in grouping_inner.DefaultIfEmpty()
                         select l2_inner)
                        on l3.Level2_Required_Id equals l2_outer.Id into grouping_outer
                    from l2_outer in grouping_outer.DefaultIfEmpty()
                    select new
                    {
                        entity = l2_outer,
                        property = l2_outer.Name
                    },
                (l1s, l2s, l3s) =>
                    from l3 in l3s
                    join l2_outer in
                        (from l1_inner in l1s
                         join l2_inner in l2s on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping_inner
                         from l2_inner in grouping_inner.DefaultIfEmpty()
                         select l2_inner)
                        on l3.Level2_Required_Id equals MaybeScalar<int>(l2_outer, () => l2_outer.Id) into grouping_outer
                    from l2_outer in grouping_outer.DefaultIfEmpty()
                    select new
                    {
                        entity = l2_outer,
                        property = Maybe(l2_outer, () => l2_outer.Name)
                    },
                elementSorter: e => e.property,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.entity?.Id, a.entity?.Id);
                    Assert.Equal(e.property, a.property);
                });
        }

        private static TResult ClientMethodReturnSelf<TResult>(TResult element)
        {
            return element;
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_reference_protection_complex_client_eval(bool isAsync)
        {
            return AssertQuery<Level1, Level2, Level3>(
                isAsync,
                (l1s, l2s, l3s) =>
                    from l3 in l3s
                    join l2_outer in
                        (from l1_inner in l1s
                         join l2_inner in l2s on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping_inner
                         from l2_inner in grouping_inner.DefaultIfEmpty()
                         select l2_inner)
                        on l3.Level2_Required_Id equals l2_outer.Id into grouping_outer
                    from l2_outer in grouping_outer.DefaultIfEmpty()
                    select ClientMethodReturnSelf(l2_outer.Name),
                (l1s, l2s, l3s) =>
                    from l3 in l3s
                    join l2_outer in
                        (from l1_inner in l1s
                         join l2_inner in l2s on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping_inner
                         from l2_inner in grouping_inner.DefaultIfEmpty()
                         select l2_inner)
                        on l3.Level2_Required_Id equals MaybeScalar<int>(l2_outer, () => l2_outer.Id) into grouping_outer
                    from l2_outer in grouping_outer.DefaultIfEmpty()
                    select ClientMethodReturnSelf(Maybe(l2_outer, () => l2_outer.Name)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_with_complex_subquery_with_joins_does_not_get_flattened(bool isAsync)
        {
            return AssertQueryScalar<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from l1_outer in l1s
                    join subquery in
                        (
                            from l2_inner in l2s
                            join l1_inner in l1s on l2_inner.Level1_Required_Id equals l1_inner.Id
                            select l2_inner
                        )
                        on l1_outer.Id equals subquery.Level1_Optional_Id into grouping
                    from subquery in grouping.DefaultIfEmpty()
                    select (int?)subquery.Id,
                (l1s, l2s) =>
                    from l1_outer in l1s
                    join subquery in
                        (
                            from l2_inner in l2s
                            join l1_inner in l1s on l2_inner.Level1_Required_Id equals l1_inner.Id
                            select l2_inner
                        )
                        on l1_outer.Id equals subquery.Level1_Optional_Id into grouping
                    from subquery in grouping.DefaultIfEmpty()
                    select MaybeScalar<int>(subquery, () => subquery.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_with_complex_subquery_with_joins_does_not_get_flattened2(bool isAsync)
        {
            return AssertQueryScalar<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from l1_outer in l1s
                    join subquery in
                        (
                            from l2_inner in l2s
                            join l1_inner in l1s on l2_inner.Level1_Required_Id equals l1_inner.Id
                            select l2_inner
                        )
                        on l1_outer.Id equals subquery.Level1_Optional_Id into grouping
                    from subquery in grouping.DefaultIfEmpty()
                    select subquery != null ? (int?)subquery.Id : null,
                (l1s, l2s) =>
                    from l1_outer in l1s
                    join subquery in
                        (
                            from l2_inner in l2s
                            join l1_inner in l1s on l2_inner.Level1_Required_Id equals l1_inner.Id
                            select l2_inner
                        )
                        on l1_outer.Id equals subquery.Level1_Optional_Id into grouping
                    from subquery in grouping.DefaultIfEmpty()
                    select MaybeScalar<int>(subquery, () => subquery.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_with_complex_subquery_with_joins_does_not_get_flattened3(bool isAsync)
        {
            return AssertQueryScalar<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from l1_outer in l1s
                    join subquery in
                        (
                            from l2_inner in l2s
                            join l1_inner in l1s on l2_inner.Level1_Required_Id equals l1_inner.Id into grouping_inner
                            from l1_inner in grouping_inner.DefaultIfEmpty()
                            select l2_inner
                        )
                        on l1_outer.Id equals subquery.Level1_Required_Id into grouping
                    from subquery in grouping.DefaultIfEmpty()
                    select (int?)subquery.Id,
                (l1s, l2s) =>
                    from l1_outer in l1s
                    join subquery in
                        (
                            from l2_inner in l2s
                            join l1_inner in l1s on l2_inner.Level1_Required_Id equals l1_inner.Id into grouping_inner
                            from l1_inner in grouping_inner.DefaultIfEmpty()
                            select l2_inner
                        )
                        on l1_outer.Id equals MaybeScalar<int>(subquery, () => subquery.Level1_Required_Id) into grouping
                    from subquery in grouping.DefaultIfEmpty()
                    select MaybeScalar<int>(subquery, () => subquery.Id));
        }

        [ConditionalTheory(Skip = "Issue #14935. Cannot eval 'Any()'")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_with_complex_subquery_with_joins_with_reference_to_grouping1(bool isAsync)
        {
            return AssertQueryScalar<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from l1_outer in l1s
                    join subquery in
                        (
                            from l2_inner in l2s
                            join l1_inner in l1s on l2_inner.Level1_Required_Id equals l1_inner.Id
                            select l2_inner
                        )
                        on l1_outer.Id equals subquery.Level1_Optional_Id into grouping
                    where grouping.Any()
                    from subquery in grouping.DefaultIfEmpty()
                    select subquery.Id);
        }

        [ConditionalTheory(Skip = "Issue #14935. Cannot eval 'DefaultIfEmpty()'")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_with_complex_subquery_with_joins_with_reference_to_grouping2(bool isAsync)
        {
            return AssertQueryScalar<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from l1_outer in l1s
                    join subquery in
                        (
                            from l2_inner in l2s
                            join l1_inner in l1s on l2_inner.Level1_Required_Id equals l1_inner.Id
                            select l2_inner
                        )
                        on l1_outer.Id equals subquery.Level1_Optional_Id into grouping
                    from subquery in grouping.DefaultIfEmpty()
                    where grouping.Any()
                    select subquery.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_outer(bool isAsync)
        {
            return AssertQuery<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from x in
                        (from l1 in l1s
                         join l2 in l2s on l1.Id equals l2.Level1_Optional_Id into grouping
                         from l2 in grouping.DefaultIfEmpty()
                         orderby l1.Id
                         select l1).Take(2)
                    join l2_outer in l2s on x.Id equals l2_outer.Level1_Optional_Id into grouping_outer
                    from l2_outer in grouping_outer.DefaultIfEmpty()
                    select l2_outer.Name,
                (l1s, l2s) =>
                    from x in
                        (from l1 in l1s
                         join l2 in l2s on l1.Id equals l2.Level1_Optional_Id into grouping
                         from l2 in grouping.DefaultIfEmpty()
                         orderby l1.Id
                         select l1).Take(2)
                    join l2_outer in l2s on x.Id equals l2_outer.Level1_Optional_Id into grouping_outer
                    from l2_outer in grouping_outer.DefaultIfEmpty()
                    select Maybe(l2_outer, () => l2_outer.Name));
        }

        [ConditionalTheory(Skip = "Issue #14935. Cannot eval 'join Level2 l2_outer in value(Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryable`1[Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel.Level2]) on [x]?.Id equals [l2_outer].Level1_Optional_Id'")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_outer_with_client_method(bool isAsync)
        {
            return AssertQuery<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from x in
                        (from l1 in l1s
                         join l2 in l2s on l1.Id equals l2.Level1_Optional_Id into grouping
                         from l2 in grouping.DefaultIfEmpty()
                         orderby l1.Id
                         select ClientLevel1(l1)).Take(2)
                    join l2_outer in l2s on x.Id equals l2_outer.Level1_Optional_Id into grouping_outer
                    from l2_outer in grouping_outer.DefaultIfEmpty()
                    select l2_outer.Name,
                (l1s, l2s) =>
                    from x in
                        (from l1 in l1s
                         join l2 in l2s on l1.Id equals l2.Level1_Optional_Id into grouping
                         from l2 in grouping.DefaultIfEmpty()
                         orderby l1.Id
                         select ClientLevel1(l1)).Take(2)
                    join l2_outer in l2s on x.Id equals l2_outer.Level1_Optional_Id into grouping_outer
                    from l2_outer in grouping_outer.DefaultIfEmpty()
                    select Maybe(l2_outer, () => l2_outer.Name));
        }

        private static Level1 ClientLevel1(Level1 arg)
        {
            return arg;
        }

        [ConditionalTheory(Skip = "Issue#15872")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_inner(bool isAsync)
        {
            return AssertQuery<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from x in
                        (from l1 in l1s
                         join l2 in l2s on l1.Id equals l2.Level1_Optional_Id into grouping
                         from l2 in grouping.DefaultIfEmpty()
                         orderby l1.Id
                         select l2).Take(2)
                    join l1_outer in l1s on x.Level1_Optional_Id equals l1_outer.Id into grouping_outer
                    from l1_outer in grouping_outer.DefaultIfEmpty()
                    select l1_outer.Name,
                (l1s, l2s) =>
                    from x in
                        (from l1 in l1s
                         join l2 in l2s on l1.Id equals l2.Level1_Optional_Id into grouping
                         from l2 in grouping.DefaultIfEmpty()
                         orderby l1.Id
                         select l2).Take(2)
                    join l1_outer in l1s on MaybeScalar(x, () => x.Level1_Optional_Id) equals l1_outer.Id into grouping_outer
                    from l1_outer in grouping_outer.DefaultIfEmpty()
                    select Maybe(l1_outer, () => l1_outer.Name));
        }

        // issue #12806
        //[ConditionalTheory]
        //[MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_on_a_subquery_containing_another_GroupJoin_with_orderby_on_inner_sequence_projecting_inner(
            bool isAsync)
        {
            return AssertQuery<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from x in
                        (from l1 in l1s
                         join l2 in l2s.OrderBy(ee => ee.Date) on l1.Id equals l2.Level1_Optional_Id into grouping
                         from l2 in grouping.DefaultIfEmpty()
                         orderby l1.Id
                         select l2).Take(2)
                    join l1_outer in l1s on x.Level1_Optional_Id equals l1_outer.Id into grouping_outer
                    from l1_outer in grouping_outer.DefaultIfEmpty()
                    select l1_outer.Name,
                (l1s, l2s) =>
                    from x in
                        (from l1 in l1s
                         join l2 in l2s.OrderBy(ee => ee.Date) on l1.Id equals l2.Level1_Optional_Id into grouping
                         from l2 in grouping.DefaultIfEmpty()
                         orderby l1.Id
                         select l2).Take(2)
                    join l1_outer in l1s on MaybeScalar(x, () => x.Level1_Optional_Id) equals l1_outer.Id into grouping_outer
                    from l1_outer in grouping_outer.DefaultIfEmpty()
                    select Maybe(l1_outer, () => l1_outer.Name));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_on_left_side_being_a_subquery(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => l1s.OrderBy(l1 => l1.OneToOne_Optional_FK1.Name)
                    .ThenBy(l1 => l1.Id)
                    .Take(2)
                    .Select(
                        x => new
                        {
                            x.Id,
                            Brand = x.OneToOne_Optional_FK1.Name
                        }),
                l1s => l1s.OrderBy(l1 => Maybe(l1.OneToOne_Optional_FK1, () => l1.OneToOne_Optional_FK1.Name))
                    .ThenBy(l1 => l1.Id)
                    .Take(2)
                    .Select(
                        x => new
                        {
                            x.Id,
                            Brand = Maybe(x.OneToOne_Optional_FK1, () => x.OneToOne_Optional_FK1.Name)
                        }),
                e => e.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_on_right_side_being_a_subquery(bool isAsync)
        {
            return AssertQuery<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from l2 in l2s
                    join l1 in l1s.OrderBy(x => x.OneToOne_Optional_FK1.Name).Take(2) on l2.Level1_Optional_Id equals l1.Id into grouping
                    from l1 in grouping.DefaultIfEmpty()
#pragma warning disable IDE0031 // Use null propagation
                    select new
                    {
                        l2.Id,
                        Name = l1 != null ? l1.Name : null
                    },
#pragma warning restore IDE0031 // Use null propagation
                (l1s, l2s) =>
                    from l2 in l2s
                    join l1 in l1s.OrderBy(x => Maybe(x.OneToOne_Optional_FK1, () => x.OneToOne_Optional_FK1.Name)).Take(2)
                        on l2.Level1_Optional_Id equals l1.Id into grouping
                    from l1 in grouping.DefaultIfEmpty()
#pragma warning disable IDE0031 // Use null propagation
                    select new
                    {
                        l2.Id,
                        Name = l1 != null ? l1.Name : null
                    },
#pragma warning restore IDE0031 // Use null propagation
                e => e.Id);
        }

        // ReSharper disable once UnusedParameter.Local
        private static bool ClientMethod(int? id)
        {
            return true;
        }

        [ConditionalTheory(Skip = "AlreadyFixed")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_in_subquery_with_client_result_operator(bool isAsync)
        {
            return AssertQuery<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from l1 in l1s
                    where (from l1_inner in l1s
                           join l2_inner in l2s on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping
                           from l2_inner in grouping.DefaultIfEmpty()
                           select l1_inner).Distinct().Count() > 7
                    where l1.Id < 3
                    select l1.Name);
        }

        [ConditionalTheory(Skip = "Issue #14935. Cannot eval 'where ({from Level1 l1_inner in value(Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryable`1[Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel.Level1]) join Level2 l2_inner in value(Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryable`1[Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel.Level2]) on Convert([l1_inner].Id, Nullable`1) equals [l2_inner].Level1_Optional_Id into IEnumerable`1 grouping from Level2 l2_inner in {[grouping] => DefaultIfEmpty()} select ClientStringMethod([l1_inner].Name) => Count()} > 7)'")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_in_subquery_with_client_projection(bool isAsync)
        {
            return AssertQuery<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from l1 in l1s
                    where (from l1_inner in l1s
                           join l2_inner in l2s on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping
                           from l2_inner in grouping.DefaultIfEmpty()
                           select ClientStringMethod(l1_inner.Name)).Count() > 7
                    where l1.Id < 3
                    select l1.Name);
        }

        [ConditionalTheory(Skip = "Issue #14935. Cannot eval 'where ({from Level1 l1_inner in value(Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryable`1[Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel.Level1]) join Level2 l2_inner in value(Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryable`1[Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel.Level2]) on Convert([l1_inner].Id, Nullable`1) equals [l2_inner].Level1_Optional_Id into IEnumerable`1 grouping_inner from Level2 l2_inner in {[grouping_inner] => DefaultIfEmpty()} select ClientStringMethod([l1_inner].Name) => Count()} > 7)'")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_in_subquery_with_client_projection_nested1(bool isAsync)
        {
            return AssertQuery<Level1, Level2>
            (
                isAsync,
                (l1s, l2s) =>
                    from l1_outer in l1s
                    where (from l1_middle in l1s
                           join l2_middle in l2s on l1_middle.Id equals l2_middle.Level1_Optional_Id into grouping_middle
                           from l2_middle in grouping_middle.DefaultIfEmpty()
                           where (from l1_inner in l1s
                                  join l2_inner in l2s on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping_inner
                                  from l2_inner in grouping_inner.DefaultIfEmpty()
                                  select ClientStringMethod(l1_inner.Name)).Count() > 7
                           select l1_middle).OrderBy(l1 => l1.Id).Take(10).Count() > 4
                    where l1_outer.Id < 2
                    select l1_outer.Name);
        }

        [ConditionalTheory(Skip = "Issue #14935. Cannot eval 'where ({from Level1 l1_middle in value(Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryable`1[Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel.Level1]) join Level2 l2_middle in value(Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryable`1[Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel.Level2]) on Convert([l1_middle].Id, Nullable`1) equals [l2_middle].Level1_Optional_Id into IEnumerable`1 grouping_middle from Level2 l2_middle in {[grouping_middle] => DefaultIfEmpty()} where ({from Level1 l1_inner in value(Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryable`1[Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel.Level1]) join Level2 l2_inner in value(Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryable`1[Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel.Level2]) on Convert([l1_inner].Id, Nullable`1) equals [l2_inner].Level1_Optional_Id into IEnumerable`1 grouping_inner from Level2 l2_inner in {[grouping_inner] => DefaultIfEmpty()} select [l1_inner].Name => Count()} > 7) select ClientStringMethod([l1_middle].Name) => Count()} > 4)'")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_in_subquery_with_client_projection_nested2(bool isAsync)
        {
            return AssertQuery<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from l1_outer in l1s
                    where (from l1_middle in l1s
                           join l2_middle in l2s on l1_middle.Id equals l2_middle.Level1_Optional_Id into grouping_middle
                           from l2_middle in grouping_middle.DefaultIfEmpty()
                           where (from l1_inner in l1s
                                  join l2_inner in l2s on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping_inner
                                  from l2_inner in grouping_inner.DefaultIfEmpty()
                                  select l1_inner.Name).Count() > 7
                           select ClientStringMethod(l1_middle.Name)).Count() > 4
                    where l1_outer.Id < 2
                    select l1_outer.Name);
        }

        private static string ClientStringMethod(string argument)
        {
            return argument;
        }

        [ConditionalTheory(Skip = "Issue #14935. Cannot eval 'DefaultIfEmpty()'")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_reference_to_group_in_OrderBy(bool isAsync)
        {
            return AssertQueryScalar<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from l1 in l1s
                    join l2 in l2s on l1.Id equals l2.Level1_Optional_Id into groupJoin
                    from l2 in groupJoin.DefaultIfEmpty()
                    orderby groupJoin.Count()
                    select l1.Id,
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_client_method_on_outer(bool isAsync)
        {
            return AssertQuery<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from l1 in l1s
                    join l2 in l2s on l1.Id equals l2.Level1_Optional_Id into groupJoin
                    from l2 in groupJoin.DefaultIfEmpty()
                    select new
                    {
                        l1.Id,
                        client = ClientMethodNullableInt(l1.Id)
                    },
                elementSorter: e => e.Id,
                elementAsserter: (e, a) => Assert.Equal(e.Id + " " + e.client, a.Id + " " + a.client));
        }

        [ConditionalTheory(Skip = "Issue #14935. Cannot eval 'orderby ClientMethodNullableInt(Convert([l1].Id, Nullable`1)) asc, ClientMethodNullableInt(?[l2] | [l2]?.Id?) asc'")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_client_method_in_OrderBy(bool isAsync)
        {
            return AssertQueryScalar<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from l1 in l1s
                    join l2 in l2s on l1.Id equals l2.Level1_Optional_Id into groupJoin
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
        public virtual Task GroupJoin_without_DefaultIfEmpty(bool isAsync)
        {
            return AssertQueryScalar<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from l1 in l1s
                    join l2 in l2s on l1.Id equals l2.Level1_Optional_Id into groupJoin
                    from l2 in groupJoin.Select(gg => gg)
                    select l1.Id);
        }

        [ConditionalTheory(Skip = "Issue #14935. Cannot eval 'where ([gg].Id > 0)'")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_with_subquery_on_inner(bool isAsync)
        {
            return AssertQueryScalar<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from l1 in l1s
                    join l2 in l2s on l1.Id equals l2.Level1_Optional_Id into groupJoin
                    from l2 in groupJoin.Where(gg => gg.Id > 0).OrderBy(gg => gg.Id).Take(10).DefaultIfEmpty()
                    select l1.Id);
        }

        [ConditionalTheory(Skip = "Issue #14935. Cannot eval 'where ([gg].Id > 0)'")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_with_subquery_on_inner_and_no_DefaultIfEmpty(bool isAsync)
        {
            return AssertQueryScalar<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from l1 in l1s
                    join l2 in l2s on l1.Id equals l2.Level1_Optional_Id into groupJoin
                    from l2 in groupJoin.Where(gg => gg.Id > 0).OrderBy(gg => gg.Id).Take(10)
                    select l1.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Optional_navigation_in_subquery_with_unrelated_projection(bool isAsync)
        {
            return AssertQueryScalar<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    l1s.Where(l1 => l1.OneToOne_Optional_FK1.Name != "Foo")
                        .OrderBy(l1 => l1.Id)
                        .Take(15)
                        .Select(l1 => l1.Id),
                (l1s, l2s) =>
                    l1s.Where(l1 => Maybe(l1.OneToOne_Optional_FK1, () => l1.OneToOne_Optional_FK1.Name) != "Foo")
                        .OrderBy(l1 => l1.Id)
                        .Take(15)
                        .Select(l1 => l1.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Explicit_GroupJoin_in_subquery_with_unrelated_projection(bool isAsync)
        {
            return AssertQueryScalar<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from l1 in (from l1 in l1s
                                join l2 in l2s on l1.Id equals l2.Level1_Optional_Id into grouping
                                from l2 in grouping.DefaultIfEmpty()
#pragma warning disable IDE0031 // Use null propagation
                                where (l2 != null ? l2.Name : null) != "Foo"
#pragma warning restore IDE0031 // Use null propagation
                                select l1).OrderBy(l1 => l1.Id).Take(15)
                    select l1.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Explicit_GroupJoin_in_subquery_with_unrelated_projection2(bool isAsync)
        {
            return AssertQueryScalar<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from l1 in (from l1 in l1s
                                join l2 in l2s on l1.Id equals l2.Level1_Optional_Id into grouping
                                from l2 in grouping.DefaultIfEmpty()
#pragma warning disable IDE0031 // Use null propagation
                                where (l2 != null ? l2.Name : null) != "Foo"
#pragma warning restore IDE0031 // Use null propagation
                                select l1).Distinct()
                    select l1.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Explicit_GroupJoin_in_subquery_with_unrelated_projection3(bool isAsync)
        {
            return AssertQueryScalar<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from l1 in (from l1 in l1s
                                join l2 in l2s on l1.Id equals l2.Level1_Optional_Id into grouping
                                from l2 in grouping.DefaultIfEmpty()
#pragma warning disable IDE0031 // Use null propagation
                                where (l2 != null ? l2.Name : null) != "Foo"
#pragma warning restore IDE0031 // Use null propagation
                                select l1.Id).Distinct()
                    select l1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Explicit_GroupJoin_in_subquery_with_unrelated_projection4(bool isAsync)
        {
            return AssertQueryScalar<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from l1 in (from l1 in l1s
                                join l2 in l2s on l1.Id equals l2.Level1_Optional_Id into grouping
                                from l2 in grouping.DefaultIfEmpty()
#pragma warning disable IDE0031 // Use null propagation
                                where (l2 != null ? l2.Name : null) != "Foo"
#pragma warning restore IDE0031 // Use null propagation
                                select l1.Id).Distinct().OrderBy(id => id).Take(20)
                    select l1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Explicit_GroupJoin_in_subquery_with_scalar_result_operator(bool isAsync)
        {
            return AssertQuery<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from l1 in l1s
                    where (from l1_inner in l1s
                           join l2 in l2s on l1_inner.Id equals l2.Level1_Optional_Id into grouping
                           from l2 in grouping.DefaultIfEmpty()
                           select l1_inner).Count() > 4
                    select l1,
                elementSorter: e => e.Id,
                elementAsserter: (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Explicit_GroupJoin_in_subquery_with_multiple_result_operator_distinct_count_materializes_main_clause(
            bool isAsync)
        {
            return AssertQuery<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from l1 in l1s
                    where (from l1_inner in l1s
                           join l2 in l2s on l1_inner.Id equals l2.Level1_Optional_Id into grouping
                           from l2 in grouping.DefaultIfEmpty()
                           select l1_inner).Distinct().Count() > 4
                    select l1,
                elementSorter: e => e.Id,
                elementAsserter: (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_on_multilevel_reference_in_subquery_with_outer_projection(bool isAsync)
        {
            return AssertQuery<Level3>(
                isAsync,
                l3s => l3s
                    .Where(l3 => l3.OneToMany_Required_Inverse3.OneToOne_Required_FK_Inverse2.Name == "L1 03")
                    .OrderBy(l3 => l3.Level2_Required_Id)
                    .Skip(0)
                    .Take(10)
                    .Select(l3 => l3.Name));
        }

        [ConditionalTheory(Skip = " Issue#16093")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_condition_optimizations_applied_correctly_when_anonymous_type_with_single_property(bool isAsync)
        {
            return AssertQuery<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from l1 in l1s
                    join l2 in l2s
                        on new
                        {
                            A = EF.Property<int?>(l1, "OneToMany_Optional_Self_Inverse1Id")
                        }
                        equals new
                        {
                            A = EF.Property<int?>(l2, "Level1_Optional_Id")
                        }
                    select l1,
                (l1s, l2s) =>
                    from l1 in l1s
                    join l2 in l2s
                        on new
                        {
                            A = MaybeScalar<int>(l1.OneToMany_Optional_Self_Inverse1, () => l1.OneToMany_Optional_Self_Inverse1.Id)
                        }
                        equals new
                        {
                            A = l2.Level1_Optional_Id
                        }
                    select l1,
                elementSorter: e => e.Id,
                elementAsserter: (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_condition_optimizations_applied_correctly_when_anonymous_type_with_multiple_properties(bool isAsync)
        {
            return AssertQuery<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from l1 in l1s
                    join l2 in l2s
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
                (l1s, l2s) =>
                    from l1 in l1s
                    join l2 in l2s
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
                    select l1,
                elementSorter: e => e.Id,
                elementAsserter: (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalTheory(Skip = "Issue #14935. Cannot eval 'GroupBy([l2.OneToMany_Required_Self_Inverse2].Name, [l2])'")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_filter_navigation_grouping_ordering_by_group_key(bool isAsync)
        {
            var level1Id = 1;
            return AssertQuery<Level2>(
                isAsync,
                l2s => l2s
                    .Where(l2 => l2.OneToMany_Required_Inverse2.Id == level1Id)
                    .GroupBy(l2 => l2.OneToMany_Required_Self_Inverse2.Name)
                    .OrderBy(g => g.Key),
                elementAsserter: (l2oResults, efResults) =>
                {
                    var efGrouping = efResults as IGrouping<string, dynamic>;
                    var l2oGrouping = l2oResults as IGrouping<string, dynamic>;

                    Assert.Equal(l2oGrouping?.Key, efGrouping?.Key);

                    // Since l2o query has all navigations loaded in memory.
                    Assert.Equal(
                        l2oGrouping?.OrderBy(o => o.Id).Select(o => o.Id),
                        efGrouping?.OrderBy(o => o.Id).Select(o => o.Id));
                },
                assertOrder: true);
        }

        [ConditionalTheory(Skip = "Issue#15872")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Nested_group_join_with_take(bool isAsync)
        {
            return AssertQuery<Level1, Level2>(
                isAsync,
                (l1s, l2s) =>
                    from l1_outer in
                        (from l1_inner in l1s
                         orderby l1_inner.Id
                         join l2_inner in l2s on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping_inner
                         from l2_inner in grouping_inner.DefaultIfEmpty()
                         select l2_inner).Take(2)
                    join l2_outer in l2s on l1_outer.Id equals l2_outer.Level1_Optional_Id into grouping_outer
                    from l2_outer in grouping_outer.DefaultIfEmpty()
                    select l2_outer.Name,
                (l1s, l2s) =>
                    from l1_outer in
                        (from l1_inner in l1s
                         orderby l1_inner.Id
                         join l2_inner in l2s on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping_inner
                         from l2_inner in grouping_inner.DefaultIfEmpty()
                         select l2_inner).Take(2)
                    join l2_outer in l2s on MaybeScalar<int>(l1_outer, () => l1_outer.Id) equals l2_outer.Level1_Optional_Id into
                        grouping_outer
                    from l2_outer in grouping_outer.DefaultIfEmpty()
                    select Maybe(l2_outer, () => l2_outer.Name));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_with_same_navigation_compared_to_null(bool isAsync)
        {
            return AssertQueryScalar<Level2>(
                isAsync,
                l2s => from l2 in l2s
                       where l2.OneToMany_Required_Inverse2.Name != "L1 07" && l2.OneToMany_Required_Inverse2 != null
                       select l2.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multi_level_navigation_compared_to_null(bool isAsync)
        {
            return AssertQueryScalar<Level3>(
                isAsync,
                l3s => from l3 in l3s
                       where l3.OneToMany_Optional_Inverse3.OneToOne_Required_FK_Inverse2 != null
                       select l3.Id,
                l3s => from l3 in l3s
                       where Maybe(l3.OneToMany_Optional_Inverse3, () => l3.OneToMany_Optional_Inverse3.OneToOne_Required_FK_Inverse2)
                             != null
                       select l3.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multi_level_navigation_with_same_navigation_compared_to_null(bool isAsync)
        {
            return AssertQueryScalar<Level3>(
                isAsync,
                l3s => from l3 in l3s
                       where l3.OneToMany_Optional_Inverse3.OneToOne_Required_FK_Inverse2.Name != "L1 07"
                       where l3.OneToMany_Optional_Inverse3.OneToOne_Required_FK_Inverse2 != null
                       select l3.Id,
                l3s => from l3 in l3s
                       where Maybe(
                                 l3.OneToMany_Optional_Inverse3,
                                 () => Maybe(
                                     l3.OneToMany_Optional_Inverse3.OneToOne_Required_FK_Inverse2,
                                     () => l3.OneToMany_Optional_Inverse3.OneToOne_Required_FK_Inverse2.Name)) != "L1 07"
                       where Maybe(l3.OneToMany_Optional_Inverse3, () => l3.OneToMany_Optional_Inverse3.OneToOne_Required_FK_Inverse2)
                             != null
                       select l3.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigations_compared_to_each_other1(bool isAsync)
        {
            return AssertQuery<Level2>(
                isAsync,
                l2s => from l2 in l2s
                       where l2.OneToMany_Required_Inverse2 == l2.OneToMany_Required_Inverse2
                       select l2.Name);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigations_compared_to_each_other2(bool isAsync)
        {
            return AssertQuery<Level2>(
                isAsync,
                l2s => from l2 in l2s
                       where l2.OneToMany_Required_Inverse2 == l2.OneToOne_Optional_PK_Inverse2
                       select l2.Name);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigations_compared_to_each_other3(bool isAsync)
        {
            return AssertQuery<Level2>(
                isAsync,
                l2s => from l2 in l2s
                       where l2.OneToMany_Optional2.Select(i => i.OneToOne_Optional_PK_Inverse3 == l2).Any()
                       select l2.Name);
        }

        [ConditionalTheory(Skip = " Issue#16093")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigations_compared_to_each_other4(bool isAsync)
        {
            return AssertQuery<Level2>(
                isAsync,
                l2s => from l2 in l2s
                       where l2.OneToOne_Required_FK2.OneToMany_Optional3
                           .Select(i => i.OneToOne_Optional_PK_Inverse4 == l2.OneToOne_Required_FK2).Any()
                       select l2.Name,
                l2s => from l2 in l2s
                       where MaybeScalar(
                                 l2.OneToOne_Required_FK2,
                                 () => MaybeScalar<bool>(
                                     l2.OneToOne_Required_FK2.OneToMany_Optional3,
                                     () => l2.OneToOne_Required_FK2.OneToMany_Optional3
                                         .Select(i => i.OneToOne_Optional_PK_Inverse4 == l2.OneToOne_Required_FK2).Any())) == true
                       select l2.Name);
        }

        [ConditionalTheory(Skip = " Issue#16093")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigations_compared_to_each_other5(bool isAsync)
        {
            return AssertQuery<Level2>(
                isAsync,
                l2s => from l2 in l2s
                       where l2.OneToOne_Required_FK2.OneToMany_Optional3
                           .Select(i => i.OneToOne_Optional_PK_Inverse4 == l2.OneToOne_Optional_PK2).Any()
                       select l2.Name,
                l2s => from l2 in l2s
                       where MaybeScalar(
                                 l2.OneToOne_Required_FK2,
                                 () => MaybeScalar<bool>(
                                     l2.OneToOne_Required_FK2.OneToMany_Optional3,
                                     () => l2.OneToOne_Required_FK2.OneToMany_Optional3
                                         .Select(i => i.OneToOne_Optional_PK_Inverse4 == l2.OneToOne_Optional_PK2).Any())) == true
                       select l2.Name);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Level4_Include(bool isAsync)
        {
            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s.Select(l1 => l1.OneToOne_Required_PK1)
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
        public virtual Task Comparing_collection_navigation_on_optional_reference_to_null(bool isAsync)
        {
            return AssertQueryScalar<Level1>(
                isAsync,
                l1s => l1s.Where(l1 => l1.OneToOne_Optional_FK1.OneToMany_Optional2 == null).Select(l1 => l1.Id),
                l1s => l1s.Where(l1 => Maybe(l1.OneToOne_Optional_FK1, () => l1.OneToOne_Optional_FK1.OneToMany_Optional2) == null)
                    .Select(l1 => l1.Id));
        }

        [ConditionalTheory(Skip = "Issue #14935. Cannot eval 'First()'")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_with_client_eval_and_navigation1(bool isAsync)
        {
            return AssertQuery<Level2>(
                isAsync,
                l2s => l2s.Select(l2 => l2s.OrderBy(l => l.Id).First().OneToOne_Required_FK_Inverse2.Name));
        }

        [ConditionalTheory(Skip = "Issue #14935. Cannot eval 'First()'")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_with_client_eval_and_navigation2(bool isAsync)
        {
            return AssertQueryScalar<Level2>(
                isAsync,
                l2s => l2s.Select(l2 => l2s.OrderBy(l => l.Id).First().OneToOne_Required_FK_Inverse2.Name == "L1 02"));
        }

        [ConditionalTheory(Skip = "issue #8526")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_with_client_eval_and_multi_level_navigation(bool isAsync)
        {
            return AssertQuery<Level3>(
                isAsync,
                l3s => l3s.Select(l3 => l3s.OrderBy(l => l.Id).First().OneToOne_Required_FK_Inverse3.OneToOne_Required_FK_Inverse2.Name));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Member_doesnt_get_pushed_down_into_subquery_with_result_operator(bool isAsync)
        {
            return AssertQuery<Level1, Level3>(
                isAsync,
                (l1s, l3s) =>
                    from l1 in l1s
                    where l1.Id < 3
                    select (from l3 in l3s
                            orderby l3.Id
                            select l3).Distinct().OrderBy(l => l.Id).Skip(1).FirstOrDefault().Name);
        }

        [ConditionalFact(Skip = "issue #8523")]
        public virtual void Subquery_with_Distinct_Skip_FirstOrDefault_without_OrderBy()
        {
            using (var ctx = CreateContext())
            {
                var query = from l1 in ctx.LevelOne
                            where l1.Id < 3
                            select (from l3 in ctx.LevelThree
                                    orderby l3.Id
                                    select l3).Distinct().Skip(1).FirstOrDefault().Name;

                var result = query.ToList();
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_collection_navigation(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => from l1 in l1s
                       select l1.OneToMany_Optional1,
                elementSorter: e => e != null ? e.Count : 0,
                elementAsserter: (e, a) =>
                {
                    var actualCollection = new List<Level2>();
                    foreach (var actualElement in a)
                    {
                        actualCollection.Add(actualElement);
                    }

                    Assert.Equal(((IEnumerable<Level2>)e)?.Count() ?? 0, actualCollection.Count);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_collection_navigation_nested(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => from l1 in l1s
                       select l1.OneToOne_Optional_FK1.OneToMany_Optional2,
                l1s => from l1 in l1s
                       select Maybe(l1.OneToOne_Optional_FK1, () => l1.OneToOne_Optional_FK1.OneToMany_Optional2),
                elementSorter: e => e != null ? e.Count : 0,
                elementAsserter: (e, a) =>
                {
                    var actualCollection = new List<Level3>();
                    foreach (var actualElement in a)
                    {
                        actualCollection.Add(actualElement);
                    }

                    Assert.Equal(((IEnumerable<Level3>)e)?.Count() ?? 0, actualCollection.Count);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_collection_navigation_using_ef_property(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => from l1 in l1s
                       select EF.Property<ICollection<Level3>>(
                           EF.Property<Level2>(
                               l1,
                               "OneToOne_Optional_FK1"),
                           "OneToMany_Optional2"),
                l1s => from l1 in l1s
                       select Maybe(l1.OneToOne_Optional_FK1, () => l1.OneToOne_Optional_FK1.OneToMany_Optional2),
                elementSorter: e => e != null ? e.Count : 0,
                elementAsserter: (e, a) =>
                {
                    var actualCollection = new List<Level3>();
                    foreach (var actualElement in a)
                    {
                        actualCollection.Add(actualElement);
                    }

                    Assert.Equal(((IEnumerable<Level3>)e)?.Count() ?? 0, actualCollection.Count);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_collection_navigation_nested_anonymous(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => from l1 in l1s
                       select new
                       {
                           l1.Id,
                           l1.OneToOne_Optional_FK1.OneToMany_Optional2
                       },
                l1s => from l1 in l1s
                       select new
                       {
                           l1.Id,
                           OneToMany_Optional2 = Maybe(
                               l1.OneToOne_Optional_FK1,
                               () => l1.OneToOne_Optional_FK1.OneToMany_Optional2)
                       },
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);

                    var actualCollection = new List<Level3>();
                    foreach (var actualElement in a.OneToMany_Optional2)
                    {
                        actualCollection.Add(actualElement);
                    }

                    Assert.Equal(((IEnumerable<Level3>)e.OneToMany_Optional2)?.Count() ?? 0, actualCollection.Count);
                });
        }

        [ConditionalTheory(Skip = " Issue#16093")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_collection_navigation_count(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => from l1 in l1s
                       select new
                       {
                           l1.Id,
                           l1.OneToOne_Optional_FK1.OneToMany_Optional2.Count
                       },
                l1s => from l1 in l1s
                       select new
                       {
                           l1.Id,
                           Count = MaybeScalar(
                                       l1.OneToOne_Optional_FK1,
                                       () => MaybeScalar<int>(
                                           l1.OneToOne_Optional_FK1.OneToMany_Optional2,
                                           () => l1.OneToOne_Optional_FK1.OneToMany_Optional2.Count)) ?? 0
                       },
                elementSorter: e => e.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_collection_navigation_composed(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => from l1 in l1s
                       where l1.Id < 3
                       select new
                       {
                           l1.Id,
                           collection = l1.OneToMany_Optional1.Where(l2 => l2.Name != "Foo").ToList()
                       },
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);

                    var actualCollection = new List<Level2>();
                    foreach (var actualElement in a.collection)
                    {
                        actualCollection.Add(actualElement);
                    }

                    Assert.Equal(((IEnumerable<Level2>)e.collection).Count(), actualCollection.Count);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_collection_and_root_entity(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => from l1 in l1s
                       select new
                       {
                           l1,
                           l1.OneToMany_Optional1
                       },
                elementSorter: e => e.l1.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.l1.Id, a.l1.Id);

                    var actualCollection = new List<Level2>();
                    foreach (var actualElement in a.OneToMany_Optional1)
                    {
                        actualCollection.Add(actualElement);
                    }

                    Assert.Equal(((IEnumerable<Level2>)e.OneToMany_Optional1).Count(), actualCollection.Count);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_collection_and_include(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => from l1 in l1s.Include(l => l.OneToMany_Optional1)
                       select new
                       {
                           l1,
                           l1.OneToMany_Optional1
                       },
                elementSorter: e => e.l1.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.l1.Id, a.l1.Id);

                    var actualCollection = new List<Level2>();
                    foreach (var actualElement in a.OneToMany_Optional1)
                    {
                        actualCollection.Add(actualElement);
                    }

                    Assert.Equal(((IEnumerable<Level2>)e.OneToMany_Optional1).Count(), actualCollection.Count);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_navigation_and_collection(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => from l1 in l1s
                       select new
                       {
                           l1.OneToOne_Optional_FK1,
                           l1.OneToOne_Optional_FK1.OneToMany_Optional2
                       },
                l1s => from l1 in l1s
                       select new
                       {
                           l1.OneToOne_Optional_FK1,
                           OneToMany_Optional2 = Maybe(l1.OneToOne_Optional_FK1, () => l1.OneToOne_Optional_FK1.OneToMany_Optional2)
                       },
                elementSorter: e => e.OneToOne_Optional_FK1?.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.OneToOne_Optional_FK1?.Id, a.OneToOne_Optional_FK1?.Id);

                    var actualCollection = new List<Level3>();
                    foreach (var actualElement in a.OneToMany_Optional2)
                    {
                        actualCollection.Add(actualElement);
                    }

                    Assert.Equal(((IEnumerable<Level3>)e.OneToMany_Optional2)?.Count() ?? 0, actualCollection.Count);
                });
        }

        [ConditionalFact(Skip = "issue #8722")]
        public virtual void Include_inside_subquery()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.LevelOne
                    .Where(l1 => l1.Id < 3)
                    .Select(
                        l1 => new
                        {
                            subquery = ctx.LevelTwo.Include(l => l.OneToMany_Optional2).Where(l => l.Id > 0)
                        });

                var result = query.ToList();
            }
        }

        [ConditionalTheory(Skip = "Issue #14935. Cannot eval 'where ([l1.OneToMany_Required1_groupItem].Id > 5)'")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_optional_navigation_property_string_concat(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => from l1 in l1s
                       from l2 in l1.OneToMany_Optional1.Where(l => l.Id > 5).OrderByDescending(l => l.Name).DefaultIfEmpty()
                       select l1.Name + " " + (l2 != null ? l2.Name : "NULL"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_multiple_orderbys_member(bool isAsync)
        {
            return AssertIncludeQuery<Level2>(
                isAsync,
                l2s => l2s
                    .Include(l2 => l2.OneToMany_Optional2)
                    .OrderBy(l2 => l2.Name)
                    .ThenBy(l2 => l2.Level1_Required_Id),
                new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level2>(e => e.OneToMany_Optional2, "OneToMany_Optional2")
                },
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_multiple_orderbys_property(bool isAsync)
        {
            return AssertIncludeQuery<Level2>(
                isAsync,
                l2s => l2s
                    .Include(l2 => l2.OneToMany_Optional2)
                    .OrderBy(l2 => EF.Property<int>(l2, "Level1_Required_Id"))
                    .ThenBy(l2 => l2.Name),
                l2s => l2s
                    .OrderBy(l2 => l2.Level1_Required_Id)
                    .ThenBy(l2 => l2.Name),
                new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level2>(e => e.OneToMany_Optional2, "OneToMany_Optional2")
                },
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_multiple_orderbys_methodcall(bool isAsync)
        {
            return AssertIncludeQuery<Level2>(
                isAsync,
                l2s => l2s
                    .Include(l2 => l2.OneToMany_Optional2)
                    .OrderBy(l2 => Math.Abs(l2.Level1_Required_Id))
                    .ThenBy(l2 => l2.Name),
                new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level2>(e => e.OneToMany_Optional2, "OneToMany_Optional2")
                },
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_multiple_orderbys_complex(bool isAsync)
        {
            return AssertIncludeQuery<Level2>(
                isAsync,
                l2s => l2s
                    .Include(l2 => l2.OneToMany_Optional2)
                    .OrderBy(l2 => Math.Abs(l2.Level1_Required_Id) + 7)
                    .ThenBy(l2 => l2.Name),
                new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level2>(e => e.OneToMany_Optional2, "OneToMany_Optional2")
                },
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_multiple_orderbys_complex_repeated(bool isAsync)
        {
            return AssertIncludeQuery<Level2>(
                isAsync,
                l2s => l2s
                    .Include(l2 => l2.OneToMany_Optional2)
                    .OrderBy(l2 => -l2.Level1_Required_Id)
                    .ThenBy(l2 => -l2.Level1_Required_Id).ThenBy(l2 => l2.Name),
                new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level2>(e => e.OneToMany_Optional2, "OneToMany_Optional2")
                },
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Entries_for_detached_entities_are_removed()
        {
            using (var context = CreateContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
                var entity = Fixture.QueryAsserter.SetExtractor.Set<Level2>(context).OrderBy(l2 => l2.Id).First();
                var entry = context.ChangeTracker.Entries().Single();
                Assert.Same(entity, entry.Entity);

                entry.State = EntityState.Detached;

                Assert.Empty(context.ChangeTracker.Entries());

                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            }
        }

        [ConditionalTheory(Skip = "Issue #14935. Cannot eval 'GroupBy([l1].Name, _Include(queryContext, [l1], new [] {[l1.OneToOne_Optional_FK1]}, (queryContext, entity, included) => { ... }))'")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_reference_with_groupby_in_subquery(bool isAsync)
        {
            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s
                    .Include(l1 => l1.OneToOne_Optional_FK1)
                    .GroupBy(g => g.Name)
                    .Select(g => g.OrderBy(e => e.Id).FirstOrDefault()),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level1>(e => e.OneToOne_Optional_FK1, "OneToOne_Optional_FK1")
                });
        }

        [ConditionalTheory(Skip = "Issue #14935. Cannot eval 'GroupBy([l1].Name, _Include(queryContext, [l1], new [] {}, (queryContext, entity, included) => { ... }))' could not be translated and will be evaluated locally.'")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_groupby_in_subquery(bool isAsync)
        {
            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s
                    .Include(l1 => l1.OneToMany_Optional1)
                    .GroupBy(g => g.Name)
                    .Select(g => g.OrderBy(e => e.Id).FirstOrDefault()),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level1>(e => e.OneToMany_Optional1, "OneToMany_Optional1")
                });
        }

        [ConditionalTheory(Skip = "Issue #14935. Cannot eval 'GroupBy([l1].Name, _Include(queryContext, [l1], new [] {[l1.OneToOne_Optional_FK1]}, (queryContext, entity, included) => { ... }))'")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multi_include_with_groupby_in_subquery(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(e => e.OneToOne_Optional_FK1, "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level2>(e => e.OneToMany_Optional2, "OneToMany_Optional2", "OneToOne_Optional_FK1")
            };

            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s
                    .Include(l1 => l1.OneToOne_Optional_FK1.OneToMany_Optional2)
                    .GroupBy(g => g.Name)
                    .Select(g => g.OrderBy(e => e.Id).FirstOrDefault()),
                expectedIncludes);
        }

        [ConditionalTheory(Skip = "Issue #14935. Cannot eval 'GroupBy([l1].Name, _Include(queryContext, [l1], new [] {}, (queryContext, entity, included) => { ... }))'")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_groupby_in_subquery_and_filter_before_groupby(bool isAsync)
        {
            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s
                    .Include(l1 => l1.OneToMany_Optional1)
                    .Where(l1 => l1.Id > 3)
                    .GroupBy(g => g.Name)
                    .Select(g => g.OrderBy(e => e.Id).FirstOrDefault()),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level1>(e => e.OneToMany_Optional1, "OneToMany_Optional1")
                });
        }

        [ConditionalTheory(Skip = "Issue #14935. Cannot eval 'GroupBy([l1].Name, _Include(queryContext, [l1], new [] {}, (queryContext, entity, included) => { ... }))'")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_groupby_in_subquery_and_filter_after_groupby(bool isAsync)
        {
            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s
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
        public virtual Task String_include_multiple_derived_navigation_with_same_name_and_same_type(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<InheritanceDerived1>(e => e.ReferenceSameType, "ReferenceSameType"),
                new ExpectedInclude<InheritanceDerived2>(e => e.ReferenceSameType, "ReferenceSameType")
            };

            return AssertIncludeQuery<InheritanceBase1>(
                isAsync,
                i1s => i1s.Include("ReferenceSameType"),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_include_multiple_derived_navigation_with_same_name_and_different_type(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<InheritanceDerived1>(e => e.ReferenceDifferentType, "ReferenceDifferentType"),
                new ExpectedInclude<InheritanceDerived2>(e => e.ReferenceDifferentType, "ReferenceDifferentType")
            };

            return AssertIncludeQuery<InheritanceBase1>(
                isAsync,
                i1s => i1s.Include("ReferenceDifferentType"),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_include_multiple_derived_navigation_with_same_name_and_different_type_nested_also_includes_partially_matching_navigation_chains(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<InheritanceDerived1>(e => e.ReferenceDifferentType, "ReferenceDifferentType"),
                new ExpectedInclude<InheritanceDerived2>(e => e.ReferenceDifferentType, "ReferenceDifferentType"),
                new ExpectedInclude<InheritanceLeaf2>(e => e.BaseCollection, "BaseCollection", "ReferenceDifferentType")
            };

            return AssertIncludeQuery<InheritanceBase1>(
                isAsync,
                i1s => i1s.Include("ReferenceDifferentType.BaseCollection"),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_include_multiple_derived_collection_navigation_with_same_name_and_same_type(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<InheritanceDerived1>(e => e.CollectionSameType, "CollectionSameType"),
                new ExpectedInclude<InheritanceDerived2>(e => e.CollectionSameType, "CollectionSameType")
            };

            return AssertIncludeQuery<InheritanceBase1>(
                isAsync,
                i1s => i1s.Include("CollectionSameType"),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_include_multiple_derived_collection_navigation_with_same_name_and_different_type(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<InheritanceDerived1>(e => e.CollectionDifferentType, "CollectionDifferentType"),
                new ExpectedInclude<InheritanceDerived2>(e => e.CollectionDifferentType, "CollectionDifferentType")
            };

            return AssertIncludeQuery<InheritanceBase1>(
                isAsync,
                i1s => i1s.Include("CollectionDifferentType"),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_include_multiple_derived_collection_navigation_with_same_name_and_different_type_nested_also_includes_partially_matching_navigation_chains(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<InheritanceDerived1>(e => e.CollectionDifferentType, "CollectionDifferentType"),
                new ExpectedInclude<InheritanceDerived2>(e => e.CollectionDifferentType, "CollectionDifferentType"),
                new ExpectedInclude<InheritanceLeaf2>(e => e.BaseCollection, "BaseCollection", "CollectionDifferentType")
            };

            return AssertIncludeQuery<InheritanceBase1>(
                isAsync,
                i1s => i1s.Include("CollectionDifferentType.BaseCollection"),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_include_multiple_derived_navigations_complex(bool isAsync)
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

            return AssertIncludeQuery<InheritanceBase2>(
                isAsync,
                i2s => i2s.Include("Reference.CollectionDifferentType").Include("Collection.ReferenceSameType"),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_reference_collection_order_by_reference_navigation(bool isAsync)
        {
            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s
                    .Include(l1 => l1.OneToOne_Optional_FK1.OneToMany_Optional2)
                    .OrderBy(l1 => (int?)l1.OneToOne_Optional_FK1.Id),
                l1s => l1s
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
        public virtual Task Nav_rewrite_doesnt_apply_null_protection_for_function_arguments(bool isAsync)
        {
            return AssertQueryScalar<Level1>(
                isAsync,
                l1s => l1s.Where(l1 => l1.OneToOne_Optional_PK1 != null)
                    .Select(l1 => Math.Max(l1.OneToOne_Optional_PK1.Level1_Required_Id, 7)));
        }

        [ConditionalTheory(Skip = "See issue#11464")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Accessing_optional_property_inside_result_operator_subquery(bool isAsync)
        {
            var names = new[] { "Name1", "Name2" };

            return AssertQuery<Level1>(
                isAsync,
                l1s => l1s.Where(l1 => names.All(n => l1.OneToOne_Optional_FK1.Name != n)),
                l1s => l1s.Where(l1 => names.All(n => Maybe(l1.OneToOne_Optional_FK1, () => l1.OneToOne_Optional_FK1.Name) != n)));
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_after_SelectMany_and_reference_navigation(bool isAsync)
        {
            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s.SelectMany(l1 => l1.OneToMany_Required1).Select(l2 => l2.OneToOne_Optional_FK2)
                    .Include(l3 => l3.OneToMany_Optional3),
                new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level3>(l3 => l3.OneToMany_Optional3, "OneToMany_Optional3")
                });
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_after_multiple_SelectMany_and_reference_navigation(bool isAsync)
        {
            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s.SelectMany(l1 => l1.OneToMany_Required1).SelectMany(l2 => l2.OneToMany_Optional2)
                    .Select(l3 => l3.OneToOne_Required_FK3).Include(l4 => l4.OneToMany_Required_Self4),
                new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level4>(l4 => l4.OneToMany_Required_Self4, "OneToMany_Required_Self4")
                });
        }

        [ConditionalTheory(Skip = "Issue#15711")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_after_SelectMany_and_multiple_reference_navigations(bool isAsync)
        {
            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s.SelectMany(l1 => l1.OneToMany_Required1).Select(l2 => l2.OneToOne_Optional_FK2)
                    .Select(l3 => l3.OneToOne_Required_FK3).Include(l4 => l4.OneToMany_Optional_Self4),
                l1s => l1s.SelectMany(l1 => l1.OneToMany_Required1).Select(l2 => l2.OneToOne_Optional_FK2)
                    .Select(l3 => Maybe(l3, () => l3.OneToOne_Required_FK3)),
                new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level4>(l4 => l4.OneToMany_Optional_Self4, "OneToMany_Optional_Self4")
                });
        }

        // also #15081
        [ConditionalTheory(Skip = "Issue #14935. Cannot eval 'Distinct()'")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_after_SelectMany_and_reference_navigation_with_another_SelectMany_with_Distinct(bool isAsync)
        {
            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => from lOuter in l1s.SelectMany(l1 => l1.OneToMany_Required1).Select(l2 => l2.OneToOne_Optional_FK2)
                           .Include(l3 => l3.OneToMany_Optional3)
                       from lInner in lOuter.OneToMany_Optional3.Distinct()
                       where lInner != null
                       select lOuter,
                l1s => from lOuter in l1s.SelectMany(l1 => l1.OneToMany_Required1).Select(l2 => l2.OneToOne_Optional_FK2)
                       where lOuter != null
                       from lInner in lOuter.OneToMany_Optional3.Distinct()
                       where lInner != null
                       select lOuter,
                new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level3>(l3 => l3.OneToMany_Optional3, "OneToMany_Optional")
                });
        }

        [ConditionalTheory(Skip = "issue #15081")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_subquery_with_custom_projection(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => l1s.OrderBy(l1 => l1.Id).SelectMany(
                    l1 => l1.OneToMany_Optional1.Select(
                        l2 => new
                        {
                            l2.Name
                        })).Take(1));
        }

        [ConditionalTheory(Skip = "issue #15081")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_check_in_anonymous_type_projection_should_not_be_removed(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => l1s.OrderBy(l1 => l1.Id).Select(
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
                elementAsserter: (e, a) =>
                {
                    CollectionAsserter<dynamic>(
                        elementSorter: e1 => e1.Level3.Name,
                        elementAsserter: (e1, a1) => Assert.Equal(e1.Level3.Name, a1.Level3.Name))(e.Level2s, a.Level2s);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_check_in_Dto_projection_should_not_be_removed(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => l1s.OrderBy(l1 => l1.Id).Select(
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
                elementAsserter: (e, a) =>
                {
                    CollectionAsserter<dynamic>(
                        elementSorter: e1 => e1.Level3.Value,
                        elementAsserter: (e1, a1) => Assert.Equal(e1.Level3.Value, a1.Level3.Value))(e.Level2s, a.Level2s);
                });
        }

        private class ProjectedDto<T>
        {
            public T Value { get; set; }
        }

        [ConditionalTheory(Skip = "issue #15081")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_navigation_property_followed_by_select_collection_navigation(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => l1s.SelectMany(l1 => l1.OneToMany_Optional1).Select(l2 => new { l2.Id, l2.OneToMany_Optional2 }),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    CollectionAsserter<Level3>(ee => ee.Id, (ee, aa) => Assert.Equal(ee.Id, aa.Id))(
                        e.OneToMany_Optional2, a.OneToMany_Optional2);
                });
        }

        [ConditionalTheory(Skip = "issue #15081")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_SelectMany_navigation_property_followed_by_select_collection_navigation(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => l1s.SelectMany(l1 => l1.OneToMany_Optional1).SelectMany(l2 => l2.OneToMany_Optional2)
                    .Select(l2 => new { l2.Id, l2.OneToMany_Optional3 }),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    CollectionAsserter<Level4>(ee => ee.Id, (ee, aa) => Assert.Equal(ee.Id, aa.Id))(
                        e.OneToMany_Optional3, a.OneToMany_Optional3);
                });
        }

        [ConditionalFact(Skip = "issue #15081")]
        public virtual void SelectMany_navigation_property_with_include_and_followed_by_select_collection_navigation()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.LevelOne.SelectMany(l1 => l1.OneToMany_Optional1).Include(l2 => l2.OneToMany_Required2)
                    .Select(l2 => new { l2, l2.OneToMany_Optional2 });
                var result = query.ToList();

                Assert.True(result.All(r => r.l2.OneToMany_Required2 != null));
                Assert.True(result.Any(r => r.OneToMany_Optional2.Count > 0));
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include1(bool isAsync)
        {
            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s.Include(l1 => l1.OneToOne_Optional_FK1),
                new List<IExpectedInclude> { new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1, "OneToOne_Optional_FK1") });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include2(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1, "OneToOne_Optional_FK1"),
                    new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1, "OneToOne_Optional_FK1")
                };

            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s.Include(l1 => l1.OneToOne_Optional_FK1).Include(l1 => l1.OneToOne_Optional_FK1),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include3(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1, "OneToOne_Optional_FK1"),
                    new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_PK1, "OneToOne_Optional_PK1")
                };

            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s.Include(l1 => l1.OneToOne_Optional_FK1).Include(l1 => l1.OneToOne_Optional_PK1),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include4(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1, "OneToOne_Optional_FK1"),
                    new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_PK2, "OneToOne_Optional_PK2", "OneToOne_Optional_FK1")
                };

            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s.Include(l1 => l1.OneToOne_Optional_FK1).ThenInclude(l1 => l1.OneToOne_Optional_PK2),
                expectedIncludes);
        }


        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include5(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1, "OneToOne_Optional_FK1"),
                    new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_PK2, "OneToOne_Optional_PK2", "OneToOne_Optional_FK1")
                };

            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s.Include(l1 => l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include6(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_PK2, "OneToOne_Optional_PK2")
                };

            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s.Include(l1 => l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2).Select(l1 => l1.OneToOne_Optional_FK1),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include7(bool isAsync)
        {
            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s.Include(l1 => l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2).Select(l1 => l1.OneToOne_Optional_PK1),
                new List<IExpectedInclude>());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include8(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK_Inverse2, "OneToOne_Optional_FK_Inverse2")
                };

            return AssertIncludeQuery<Level2>(
                isAsync,
                l2s => l2s.Where(l2 => l2.OneToOne_Optional_FK_Inverse2.Name != "Fubar").Include(l2 => l2.OneToOne_Optional_FK_Inverse2),
                l2s => l2s.Where(l2 => Maybe(l2.OneToOne_Optional_FK_Inverse2, () => l2.OneToOne_Optional_FK_Inverse2.Name) != "Fubar").Include(l2 => l2.OneToOne_Optional_FK_Inverse2),
                expectedIncludes);
        }


        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include9(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK_Inverse2, "OneToOne_Optional_FK_Inverse2")
                };

            return AssertIncludeQuery<Level2>(
                isAsync,
                l2s => l2s.Include(l2 => l2.OneToOne_Optional_FK_Inverse2).Where(l2 => l2.OneToOne_Optional_FK_Inverse2.Name != "Fubar"),
                l2s => l2s.Include(l2 => l2.OneToOne_Optional_FK_Inverse2).Where(l2 => Maybe(l2.OneToOne_Optional_FK_Inverse2, () => l2.OneToOne_Optional_FK_Inverse2.Name) != "Fubar"),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include10(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1, "OneToOne_Optional_FK1"),
                    new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_PK2, "OneToOne_Optional_PK2", "OneToOne_Optional_FK1"),
                    new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_PK1, "OneToOne_Optional_PK1"),
                    new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2, "OneToOne_Optional_FK2", "OneToOne_Optional_PK1"),
                    new ExpectedInclude<Level3>(l3 => l3.OneToOne_Optional_PK3, "OneToOne_Optional_PK3", "OneToOne_Optional_FK1.OneToOne_Optional_FK2")
                };

            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s
                    .Include(l1 => l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2)
                    .Include(l1 => l1.OneToOne_Optional_PK1.OneToOne_Optional_FK2.OneToOne_Optional_PK3),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include11(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1, "OneToOne_Optional_FK1"),
                    new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2, "OneToOne_Optional_FK2", "OneToOne_Optional_FK1"),
                    new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_PK2, "OneToOne_Optional_PK2", "OneToOne_Optional_FK1"),
                    new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_PK1, "OneToOne_Optional_PK1"),
                    new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2, "OneToOne_Optional_FK2", "OneToOne_Optional_PK1"),
                    new ExpectedInclude<Level3>(l3 => l3.OneToOne_Optional_FK3, "OneToOne_Optional_FK3", "OneToOne_Optional_PK1.OneToOne_Optional_FK2"),
                    new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_PK1, "OneToOne_Optional_PK1"),
                    new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2, "OneToOne_Optional_FK2", "OneToOne_Optional_PK1"),
                    new ExpectedInclude<Level3>(l3 => l3.OneToOne_Optional_PK3, "OneToOne_Optional_PK3", "OneToOne_Optional_PK1.OneToOne_Optional_FK2"),
                    new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_PK2, "OneToOne_Optional_PK2", "OneToOne_Optional_PK1"),
                };

            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s
                    .Include(l1 => l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2)
                    .Include(l1 => l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2)
                    .Include(l1 => l1.OneToOne_Optional_PK1.OneToOne_Optional_FK2.OneToOne_Optional_FK3)
                    .Include(l1 => l1.OneToOne_Optional_PK1.OneToOne_Optional_FK2.OneToOne_Optional_PK3)
                    .Include(l1 => l1.OneToOne_Optional_PK1.OneToOne_Optional_PK2),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include12(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2, "OneToOne_Optional_FK2")
                };

            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s
                    .Include(l1 => l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2)
                    .Select(l1 => l1.OneToOne_Optional_FK1),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include13(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1, "OneToOne_Optional_FK1")
                };

            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s
                    .Include(l1 => l1.OneToOne_Optional_FK1)
                    .Select(l1 => new { one = l1, two = l1 }),
                expectedIncludes,
                clientProjections: new List<Func<dynamic, object>>
                {
                    x => x.one,
                    x => x.two
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include14(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1, "OneToOne_Optional_FK1"),
                    new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2, "OneToOne_Optional_FK2", "OneToOne_Optional_FK1"),
                };

            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s
                    .Include(l1 => l1.OneToOne_Optional_FK1).ThenInclude(l2 => l2.OneToOne_Optional_FK2)
                    .Select(l1 => new { one = l1, two = l1.OneToOne_Optional_FK1, three = l1.OneToOne_Optional_PK1 }),
                expectedIncludes,
                clientProjections: new List<Func<dynamic, object>>
                {
                    x => x.one,
                    // issue #15368
                    //x => x.two,
                });
        }

        [ConditionalFact]
        public virtual void Include15()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.LevelOne.Select(l1 => new { foo = l1.OneToOne_Optional_FK1, bar = l1.OneToOne_Optional_PK1 }).Include(x => x.foo.OneToOne_Optional_FK2).Include(x => x.bar.OneToMany_Optional2);

                var result = query.ToList();
            }
        }

        [ConditionalFact]
        public virtual void Include16()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.LevelOne.Select(l1 => new { foo = l1.OneToOne_Optional_FK1, bar = l1.OneToOne_Optional_PK1 }).Distinct().Include(x => x.foo.OneToOne_Optional_FK2).Include(x => x.bar.OneToMany_Optional2);

                var result = query.ToList();
            }
        }

        [ConditionalFact]
        public virtual void Include17()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.LevelOne.Select(l1 => new { foo = l1.OneToOne_Optional_FK1, bar = l1.OneToOne_Optional_PK1 }).Include(x => x.foo.OneToOne_Optional_FK2).Distinct();

                var result = query.ToList();
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include18_1(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1, "OneToOne_Optional_FK1")
                };

            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s.Include(x => x.OneToOne_Optional_FK1).Distinct(),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include18_1_1(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1, "OneToOne_Optional_FK1")
                };

            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s.OrderBy(x => x.OneToOne_Required_FK1.Name).Include(x => x.OneToOne_Optional_FK1).Take(10),
                l1s => l1s.OrderBy(x => Maybe(x.OneToOne_Required_FK1, () => x.OneToOne_Required_FK1.Name)).Include(x => x.OneToOne_Optional_FK1).Take(10),
                expectedIncludes);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include18_2(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1, "OneToOne_Optional_FK1")
                };

            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s.Where(x => x.OneToOne_Required_FK1.Name != "Foo").Include(x => x.OneToOne_Optional_FK1).Distinct(),
                l1s => l1s.Where(x => Maybe(x.OneToOne_Required_FK1, () => x.OneToOne_Required_FK1.Name) != "Foo").Distinct(),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void Include18_3()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.LevelOne.OrderBy(x => x.OneToOne_Required_FK1.Name).Include(x => x.OneToOne_Optional_FK1).Select(l1 => new { foo = l1, bar = l1 }).Take(10);

                var result = query.ToList();
            }
        }

        [ConditionalFact]
        public virtual void Include18_3_1()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.LevelOne.OrderBy(x => x.OneToOne_Required_FK1.Name).Include(x => x.OneToOne_Optional_FK1).Select(l1 => new { foo = l1, bar = l1 }).Take(10).Select(x => new { x.foo, x.bar });

                var result = query.ToList();
            }
        }

        [ConditionalFact]
        public virtual void Include18_3_2()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.LevelOne.OrderBy(x => x.OneToOne_Required_FK1.Name).Include(x => x.OneToOne_Optional_FK1).Select(l1 => new { outer_foo = new { inner_foo = l1, inner_bar = l1.Name }, outer_bar = l1 }).Take(10);

                var result = query.ToList();
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include18_3_3(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level2>(l1 => l1.OneToOne_Optional_FK2, "OneToOne_Optional_FK2")
                };

            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s.Include(x => x.OneToOne_Optional_FK1.OneToOne_Optional_FK2).Select(l1 => l1.OneToOne_Optional_FK1).Distinct(),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void Include18_4()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.LevelOne.Include(x => x.OneToOne_Optional_FK1).Select(l1 => new { foo = l1, bar = l1 }).Distinct();

                var result = query.ToList();
            }
        }

        [ConditionalFact]
        public virtual void Include18()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.LevelOne.Include(x => x.OneToOne_Optional_FK1).Select(l1 => new { foo = l1, bar = l1.OneToOne_Optional_PK1 }).OrderBy(x => x.foo.Id).Take(10);

                var result = query.ToList();
            }
        }

        [ConditionalFact]
        public virtual void Include19()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.LevelOne.Include(x => x.OneToOne_Optional_FK1).Select(l1 => new { foo = l1.OneToOne_Optional_FK1, bar = l1.OneToOne_Optional_PK1 }).Distinct();

                var result = query.ToList();
            }
        }

        [ConditionalFact]
        public virtual void IncludeCollection1()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.LevelOne.Include(l1 => l1.OneToMany_Optional1);
                var result = query.ToList();
            }
        }

        [ConditionalFact]
        public virtual void IncludeCollection2()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.LevelOne.Include(l1 => l1.OneToMany_Optional1).ThenInclude(l2 => l2.OneToOne_Optional_PK2);
                var result = query.ToList();
            }
        }

        [ConditionalFact]
        public virtual void IncludeCollection3()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.LevelOne.Include(l1 => l1.OneToOne_Optional_FK1).ThenInclude(l2 => l2.OneToMany_Optional2);
                var result = query.ToList();
            }
        }

        [ConditionalFact]
        public virtual void IncludeCollection4()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.LevelOne.Include(l1 => l1.OneToMany_Optional1).Select(l1 => l1.OneToMany_Optional1);
                var result = query.ToList();
            }
        }

        [ConditionalFact]
        public virtual void IncludeCollection5()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.LevelOne.Include(l1 => l1.OneToMany_Optional1).ThenInclude(l2 => l2.OneToOne_Optional_PK2).Select(l1 => l1.OneToMany_Optional1);
                var result = query.ToList();
            }
        }

        [ConditionalFact]
        public virtual void IncludeCollection6()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.LevelOne.Include(l1 => l1.OneToMany_Optional1).ThenInclude(l2 => l2.OneToOne_Optional_PK2).ThenInclude(l3 => l3.OneToOne_Optional_FK3)
                    .Select(l1 => l1.OneToMany_Optional1);
                var result = query.ToList();
            }
        }

        [ConditionalFact]
        public virtual void IncludeCollection6_1()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.LevelOne.Include(l1 => l1.OneToMany_Optional1).ThenInclude(l2 => l2.OneToOne_Optional_PK2).ThenInclude(l3 => l3.OneToOne_Optional_FK3);
                var result = query.ToList();
            }
        }

        [ConditionalFact]
        public virtual void IncludeCollection6_2()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.LevelOne.Include(l1 => l1.OneToMany_Optional1).ThenInclude(l2 => l2.OneToOne_Optional_PK2).ThenInclude(l3 => l3.OneToOne_Optional_FK3)
                    .Include(l1 => l1.OneToMany_Optional1).ThenInclude(l2 => l2.OneToOne_Optional_FK2).ThenInclude(l3 => l3.OneToMany_Optional3)
                    .Select(l1 => l1.OneToMany_Optional1);
                var result = query.ToList();
            }
        }

        [ConditionalFact]
        public virtual void IncludeCollection6_3()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.LevelOne.Include(l1 => l1.OneToMany_Optional1).ThenInclude(l2 => l2.OneToOne_Optional_PK2).ThenInclude(l3 => l3.OneToOne_Optional_FK3)
                    .Include(l1 => l1.OneToMany_Optional1).ThenInclude(l2 => l2.OneToOne_Optional_FK2).ThenInclude(l3 => l3.OneToMany_Optional3);

                var result = query.ToList();
            }
        }

        [ConditionalFact]
        public virtual void IncludeCollection6_4()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.LevelOne.Include(l1 => l1.OneToMany_Optional1).ThenInclude(l2 => l2.OneToOne_Optional_PK2).ThenInclude(l3 => l3.OneToOne_Optional_FK3)
                    .Select(l1 => l1.OneToMany_Optional1.Select(l2 => l2.OneToOne_Optional_PK2));

                var result = query.ToList();
            }
        }

        [ConditionalFact]
        public virtual void IncludeCollection7()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.LevelOne.Include(l1 => l1.OneToMany_Optional1).ThenInclude(l2 => l2.OneToOne_Optional_PK2).Select(l1 => new { l1, l1.OneToMany_Optional1 });
                var result = query.ToList();
            }
        }

        [ConditionalFact]
        public virtual void IncludeCollection8()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.LevelOne.Include(l1 => l1.OneToMany_Optional1).ThenInclude(l2 => l2.OneToOne_Optional_PK2).ThenInclude(l3 => l3.OneToOne_Optional_FK3)
                    .Where(l1 => l1.OneToMany_Optional1.Where(l2 => l2.OneToOne_Optional_PK2.Name != "Foo").Count() > 0);

                var result = query.ToList();
            }
        }

        [ConditionalFact]
        public virtual void Include_with_all_method_include_gets_ignored()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.LevelOne.Include(l1 => l1.OneToOne_Optional_FK1).Include(l1 => l1.OneToMany_Optional1).All(l1 => l1.Name != "Foo");
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_with_navigations_in_the_result_selector1(bool isAsync)
        {
            return AssertQuery<Level1, Level2>(
                isAsync,
                (l1s, l2s) => l1s.Join(l2s, l1 => l1.Id, l2 => l2.Level1_Required_Id, (o, i) => new { o.OneToOne_Optional_FK1, i }));
        }

        [ConditionalFact]
        public virtual void Join_with_navigations_in_the_result_selector2()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.LevelOne.Join(ctx.LevelTwo, l1 => l1.Id, l2 => l2.Level1_Required_Id, (o, i) => new { o.OneToOne_Optional_FK1, i.OneToMany_Optional2 });
                var result = query.ToList();
            }
        }

        [ConditionalFact(Skip = "issue #12200")]
        public virtual void GroupJoin_with_navigations_in_the_result_selector()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.LevelOne.GroupJoin(ctx.LevelTwo, l1 => l1.Id, l2 => l2.Level1_Required_Id, (o, i) => new { o.OneToOne_Optional_FK1, i });
                var result = query.ToList();
            }
        }

        [ConditionalFact(Skip = "issue #15412")]
        public virtual void GroupJoin_with_grouping_composed_on1()
        {
            using (var ctx = CreateContext())
            {
                var query = from l1 in ctx.LevelOne
                            join l2 in ctx.LevelTwo.Where(x => x.OneToOne_Optional_FK2.Name != "Foo") on l1.Id equals l2.Level1_Optional_Id into grouping
                            from l2 in grouping.DefaultIfEmpty()
                            select new { l1, l2, grouping };

                var result = query.ToList();
            }
        }

        [ConditionalFact(Skip = "issue #15412")]
        public virtual void GroupJoin_with_grouping_composed_on2()
        {
            using (var ctx = CreateContext())
            {
                var query = from l1 in ctx.LevelOne
                            join l2 in ctx.LevelTwo.Where(x => x.OneToOne_Optional_FK2.Name != "Foo") on l1.Id equals l2.Level1_Optional_Id into grouping
                            from l2 in grouping.DefaultIfEmpty()
                            select new { l1, l2, grouping = grouping.Select(x => x.OneToOne_Optional_FK2.OneToOne_Required_FK3.Name) };

                var result = query.ToList();
            }
        }

        [ConditionalFact(Skip = "issue #15412")]
        public virtual void GroupJoin_with_grouping_composed_on3()
        {
            using (var ctx = CreateContext())
            {
                var query = from l1 in ctx.LevelOne
                            join l2 in ctx.LevelTwo.Where(x => x.OneToOne_Optional_FK2.Name != "Foo") on l1.Id equals l2.Level1_Optional_Id into grouping
                            from l2 in grouping.DefaultIfEmpty()
                            select new { l1, l2.OneToOne_Required_FK2, grouping };

                var result = query.ToList();
            }
        }

        [ConditionalFact(Skip = "issue #15412")]
        public virtual void GroupJoin_with_grouping_composed_on4()
        {
            using (var ctx = CreateContext())
            {
                // TODO: this is broken - when we add navigation for OneToOne_Required_FK2 we don't remap source of the grouping correctly since we already removed the binding
                var query = from l1 in ctx.LevelOne
                            join l2 in ctx.LevelTwo.Where(x => x.OneToOne_Optional_FK2.Name != "Foo") on l1.Id equals l2.Level1_Optional_Id into grouping
                            from l2 in grouping.DefaultIfEmpty()
                            select new { l1, l2.OneToOne_Required_FK2, grouping = grouping.Select(x => x.OneToOne_Required_PK2.Name) };

                var result = query.ToList();
            }
        }

        [ConditionalFact]
        public virtual void Member_pushdown_chain_3_levels_deep()
        {
            using (var ctx = CreateContext())
            {
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
                                                   select l4).FirstOrDefault()).FirstOrDefault()).FirstOrDefault().Name != "Foo"
                            select l1;

                var result = query.ToList();
            }
        }

        [ConditionalFact]
        public virtual void Member_pushdown_with_collection_navigation_in_the_middle()
        {
            using (var ctx = CreateContext())
            {
                var query = from l1 in ctx.LevelOne
                            orderby l1.Id
                            select (from l2 in ctx.LevelTwo
                                    orderby l2.Id
                                    where l2.Level1_Required_Id == l1.Id
                                    select l2.OneToMany_Optional2.Select(l3 => (from l4 in ctx.LevelFour
                                                                                where l4.Level3_Required_Id == l3.Id
                                                                                orderby l4.Id
                                                                                select l4).FirstOrDefault()).FirstOrDefault()).FirstOrDefault().Name;

                var result = query.ToList();
            }
        }

        [ConditionalTheory(Skip = "issue #15798")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Member_pushdown_with_multiple_collections(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => l1s.Select(l1 => l1.OneToMany_Optional1.OrderBy(l2 => l2.Id).FirstOrDefault().OneToMany_Optional2.OrderBy(l3 => l3.Id).FirstOrDefault().Name),
                l1s => l1s.Select(l1s => Maybe(
                    l1s.OneToMany_Optional1.OrderBy(l2 => l2.Id).FirstOrDefault(),
                    () => Maybe(l1s.OneToMany_Optional1.OrderBy(l2 => MaybeScalar<int>(l2, () => l2.Id)).FirstOrDefault().OneToMany_Optional2.OrderBy(l3 => l3.Id).FirstOrDefault(),
                        () => l1s.OneToMany_Optional1.OrderBy(l2 => MaybeScalar<int>(l2, () => l2.Id)).FirstOrDefault().OneToMany_Optional2.OrderBy(l3 => l3.Id).FirstOrDefault().Name))));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_multiple_collections_on_same_level(bool isAsync)
        {
            return AssertIncludeQuery<Level1>(
                isAsync,
                l1s => l1s.Include(l1 => l1.OneToMany_Optional1).Include(l1 => l1.OneToMany_Required1),
                new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level1>(l1 => l1.OneToMany_Optional1, "OneToMany_Optional1"),
                    new ExpectedInclude<Level1>(l1 => l1.OneToMany_Required1, "OneToMany_Required1")
                },
                assertOrder: true);
        }

        [ConditionalTheory(Skip = "Issue#16088")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_check_removal_applied_recursively(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => l1s.Where(l1 =>
                    ((((l1.OneToOne_Optional_FK1 == null
                        ? null
                        : l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2) == null
                            ? null
                            : l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2.OneToOne_Optional_FK3) == null
                                ? null
                                : l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2.OneToOne_Optional_FK3) == null
                                    ? null
                                    : l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2.OneToOne_Optional_FK3.Name) == "L4 01"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_check_different_structure_does_not_remove_null_checks(bool isAsync)
        {
            return AssertQuery<Level1>(
                isAsync,
                l1s => l1s.Where(l1 =>
                    (l1.OneToOne_Optional_FK1 == null
                        ? null
                        : l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2 == null
                            ? null
                            : l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2.OneToOne_Optional_FK3 == null
                                ? null
                                : l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2.OneToOne_Optional_FK3 == null
                                    ? null
                                    : l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2.OneToOne_Optional_FK3.Name) == "L4 01"));
        }
    }
}
