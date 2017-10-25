// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
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

        [ConditionalFact]
        public virtual void Entity_equality_empty()
        {
            AssertQuery<Level1>(
                l1s => l1s.Where(l => l.OneToOne_Optional_FK == new Level2()),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Key_equality_when_sentinel_ef_property()
        {
            AssertQuery<Level1>(
                l1s => l1s.Where(l => EF.Property<int>(l.OneToOne_Optional_FK, "Id") == 0),
                l1s => l1s.Where(l => MaybeScalar<int>(l.OneToOne_Optional_FK, () => l.OneToOne_Optional_FK.Id) == 0),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Key_equality_using_property_method_required()
        {
            AssertQuery<Level1>(
                l1s => l1s.Where(l => EF.Property<int>(l.OneToOne_Required_FK, "Id") > 7),
                l1s => l1s.Where(l => MaybeScalar<int>(l.OneToOne_Required_FK, () => l.OneToOne_Required_FK.Id) > 7),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Key_equality_using_property_method_required2()
        {
            AssertQuery<Level2>(
                l2s => l2s.Where(l => EF.Property<int>(l.OneToOne_Required_FK_Inverse, "Id") > 7),
                l2s => l2s.Where(l => l.OneToOne_Required_FK_Inverse.Id > 7),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Key_equality_using_property_method_nested()
        {
            AssertQuery<Level1>(
                l1s => l1s.Where(l => EF.Property<int>(EF.Property<Level2>(l, "OneToOne_Required_FK"), "Id") == 7),
                l1s => l1s.Where(l => MaybeScalar<int>(l.OneToOne_Required_FK, () => l.OneToOne_Required_FK.Id) == 7),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Key_equality_using_property_method_nested2()
        {
            AssertQuery<Level2>(
                l2s => l2s.Where(l => EF.Property<int>(EF.Property<Level1>(l, "OneToOne_Required_FK_Inverse"), "Id") == 7),
                l2s => l2s.Where(l => l.OneToOne_Required_FK_Inverse.Id == 7),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Key_equality_using_property_method_and_member_expression1()
        {
            AssertQuery<Level1>(
                l1s => l1s.Where(l => EF.Property<Level2>(l, "OneToOne_Required_FK").Id == 7),
                l1s => l1s.Where(l => MaybeScalar<int>(l.OneToOne_Required_FK, () => l.OneToOne_Required_FK.Id) == 7),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Key_equality_using_property_method_and_member_expression2()
        {
            AssertQuery<Level1>(
                l1s => l1s.Where(l => EF.Property<int>(l.OneToOne_Required_FK, "Id") == 7),
                l1s => l1s.Where(l => MaybeScalar<int>(l.OneToOne_Required_FK, () => l.OneToOne_Required_FK.Id) == 7),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Key_equality_using_property_method_and_member_expression3()
        {
            AssertQuery<Level2>(
                l2s => l2s.Where(l => EF.Property<int>(l.OneToOne_Required_FK_Inverse, "Id") == 7),
                l2s => l2s.Where(l => l.OneToOne_Required_FK_Inverse.Id == 7),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Key_equality_navigation_converted_to_FK()
        {
            AssertQuery<Level2>(
                l2s => l2s.Where(l => l.OneToOne_Required_FK_Inverse == new Level1 { Id = 1 }),
                l2s => l2s.Where(l => l.OneToOne_Required_FK_Inverse.Id == 1),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Key_equality_two_conditions_on_same_navigation()
        {
            AssertQuery<Level1>(
                l1s => l1s.Where(
                    l => l.OneToOne_Required_FK == new Level2 { Id = 1 }
                         || l.OneToOne_Required_FK == new Level2 { Id = 2 }),
                l1s => l1s.Where(
                    l => MaybeScalar<int>(l.OneToOne_Required_FK, () => l.OneToOne_Required_FK.Id) == 1
                         || MaybeScalar<int>(l.OneToOne_Required_FK, () => l.OneToOne_Required_FK.Id) == 2),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Key_equality_two_conditions_on_same_navigation2()
        {
            AssertQuery<Level2>(
                l2s => l2s.Where(
                    l => l.OneToOne_Required_FK_Inverse == new Level1 { Id = 1 }
                         || l.OneToOne_Required_FK_Inverse == new Level1 { Id = 2 }),
                l2s => l2s.Where(
                    l => l.OneToOne_Required_FK_Inverse.Id == 1
                         || l.OneToOne_Required_FK_Inverse.Id == 2),
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
                context.LevelOne.Include(e => e.OneToMany_Optional).ThenInclude(e => e.OneToMany_Optional).ThenInclude(e => e.OneToMany_Optional_Inverse.OneToMany_Optional).ToList();

                context.LevelOne.ToList();
            }
        }

        [ConditionalFact]
        public virtual void Multi_level_include_one_to_many_optional_and_one_to_many_optional_produces_valid_sql()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToMany_Optional, "OneToMany_Optional"),
                new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional, "OneToMany_Optional", navigationPath: "OneToMany_Optional")
            };

            AssertIncludeQuery<Level1>(
                l1s => l1s.Include(e => e.OneToMany_Optional).ThenInclude(e => e.OneToMany_Optional),
                expectedIncludes,
                elementSorter: e => e.Id);
        }

        [ConditionalFact]
        public virtual void Multi_level_include_correct_PK_is_chosen_as_the_join_predicate_for_queries_that_join_same_table_multiple_times()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToMany_Optional, "OneToMany_Optional"),
                new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional, "OneToMany_Optional", navigationPath: "OneToMany_Optional"),
                new ExpectedInclude<Level3>(l3 => l3.OneToMany_Required_Inverse, "OneToMany_Required_Inverse", navigationPath: "OneToMany_Optional.OneToMany_Optional"),
                new ExpectedInclude<Level3>(l2 => l2.OneToMany_Optional, "OneToMany_Optional", navigationPath: "OneToMany_Optional.OneToMany_Optional.OneToMany_Required_Inverse")
            };

            AssertIncludeQuery<Level1>(
                l1s => l1s.Include(e => e.OneToMany_Optional).ThenInclude(e => e.OneToMany_Optional).ThenInclude(e => e.OneToMany_Required_Inverse.OneToMany_Optional),
                expectedIncludes,
                elementSorter: e => e.Id);
        }

        [ConditionalFact]
        public virtual void Multi_level_include_reads_key_values_from_data_reader_rather_than_incorrect_reader_deep_into_the_stack()
        {
            using (var context = CreateContext())
            {
                // #1433
                context.LevelOne.Include(e => e.OneToMany_Optional).ToList();
                context.LevelOne.Include(e => e.OneToMany_Optional_Self).ToList();

                //# 1478
                context.LevelOne
                    .Include(e => e.OneToMany_Optional)
                    .ThenInclude(e => e.OneToMany_Optional_Inverse.OneToMany_Optional_Self_Inverse.OneToOne_Optional_FK).ToList();

                context.LevelOne
                    .Include(e => e.OneToMany_Optional)
                    .ThenInclude(e => e.OneToMany_Optional_Inverse.OneToMany_Optional_Self_Inverse.OneToOne_Optional_PK).ToList();

                // #1487
                context.LevelOne
                    .Include(e => e.OneToMany_Optional)
                    .ThenInclude(e => e.OneToMany_Optional_Inverse.OneToOne_Optional_PK.OneToOne_Optional_FK).ToList();

                context.LevelOne
                    .Include(e => e.OneToMany_Optional)
                    .ThenInclude(e => e.OneToMany_Optional_Inverse.OneToOne_Optional_PK.OneToOne_Optional_FK_Inverse).ToList();

                // #1488
                context.LevelOne
                    .Include(e => e.OneToMany_Optional)
                    .ThenInclude(e => e.OneToMany_Optional_Inverse.OneToOne_Optional_PK.OneToOne_Required_FK).ToList();

                context.LevelOne
                    .Include(e => e.OneToMany_Optional)
                    .ThenInclude(e => e.OneToMany_Optional_Inverse.OneToOne_Optional_PK.OneToOne_Required_FK_Inverse).ToList();
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

        [ConditionalFact]
        public virtual void Join_navigation_key_access_optional()
        {
            AssertQuery<Level1, Level2>(
                (l1s, l2s) =>
                    from e1 in l1s
                    join e2 in l2s on e1.Id equals e2.OneToOne_Optional_FK_Inverse.Id
                    select new { Id1 = e1.Id, Id2 = e2.Id },
                (l1s, l2s) =>
                    from e1 in l1s
                    join e2 in l2s on e1.Id equals MaybeScalar<int>(
                        e2.OneToOne_Optional_FK_Inverse,
                        () => e2.OneToOne_Optional_FK_Inverse.Id)
                    select new { Id1 = e1.Id, Id2 = e2.Id },
                e => e.Id1 + " " + e.Id2);
        }

        [ConditionalFact]
        public virtual void Join_navigation_key_access_required()
        {
            AssertQuery<Level1, Level2>(
                (l1s, l2s) =>
                    from e1 in l1s
                    join e2 in l2s on e1.Id equals e2.OneToOne_Required_FK_Inverse.Id
                    select new { Id1 = e1.Id, Id2 = e2.Id },
                (l1s, l2s) =>
                    from e1 in l1s
                    join e2 in l2s on e1.Id equals e2.OneToOne_Required_FK_Inverse.Id
                    select new { Id1 = e1.Id, Id2 = e2.Id },
                e => e.Id1 + " " + e.Id2);
        }

        [ConditionalFact]
        public virtual void Navigation_key_access_optional_comparison()
        {
            AssertQueryScalar<Level2>(
                l2s =>
                    from e2 in l2s
                    where e2.OneToOne_Optional_PK_Inverse.Id > 5
                    select e2.Id,
                l2s =>
                    from e2 in l2s
                    where MaybeScalar<int>(e2.OneToOne_Optional_PK_Inverse, () => e2.OneToOne_Optional_PK_Inverse.Id) > 5
                    select e2.Id);
        }

        [ConditionalFact]
        public virtual void Simple_owned_level1()
        {
            AssertQuery<Level1>(l1s => l1s.Include(l1 => l1.OneToOne_Required_PK), elementSorter: e => e.Id);
        }

        [ConditionalFact]
        public virtual void Simple_owned_level1_convention()
        {
            AssertQuery<Level1>(l1s => l1s, elementSorter: e => e.Id);
        }

        [ConditionalFact]
        public virtual void Simple_owned_level1_level2()
        {
            AssertQuery<Level1>(l1s => l1s.Include(l1 => l1.OneToOne_Required_PK.OneToOne_Required_PK), elementSorter: e => e.Id);
        }

        [ConditionalFact]
        public virtual void Simple_owned_level1_level2_level3()
        {
            AssertQuery<Level1>(
                l1s
                    => l1s.Include(l1 => l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK),
                elementSorter: e => e.Id);
        }

        [ConditionalFact]
        public virtual void Navigation_key_access_required_comparison()
        {
            AssertQueryScalar<Level2>(
                l2s =>
                    from e2 in l2s
                    where e2.OneToOne_Required_PK_Inverse.Id > 5
                    select e2.Id);
        }

        [ConditionalFact]
        public virtual void Navigation_inside_method_call_translated_to_join()
        {
            AssertQuery<Level1>(
                l1s =>
                    from e1 in l1s
                    where e1.OneToOne_Required_FK.Name.StartsWith("L")
                    select e1,
                l1s =>
                    from e1 in l1s
                    where MaybeScalar<bool>(e1.OneToOne_Required_FK, () => e1.OneToOne_Required_FK.Name.StartsWith("L")) == true
                    select e1,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Navigation_inside_method_call_translated_to_join2()
        {
            AssertQuery<Level3>(
                l3s =>
                    from e3 in l3s
                    where e3.OneToOne_Required_FK_Inverse.Name.StartsWith("L")
                    select e3,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Optional_navigation_inside_method_call_translated_to_join()
        {
            AssertQuery<Level1>(
                l1s =>
                    from e1 in l1s
                    where e1.OneToOne_Optional_FK.Name.StartsWith("L")
                    select e1,
                l1s =>
                    from e1 in l1s
                    where MaybeScalar<bool>(e1.OneToOne_Optional_FK, () => e1.OneToOne_Optional_FK.Name.StartsWith("L")) == true
                    select e1,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Optional_navigation_inside_property_method_translated_to_join()
        {
            AssertQuery<Level1>(
                l1s =>
                    from e1 in l1s
                    where EF.Property<string>(EF.Property<Level2>(e1, "OneToOne_Optional_FK"), "Name") == "L2 01"
                    select e1,
                l1s =>
                    from e1 in l1s
                    where Maybe(e1.OneToOne_Optional_FK, () => e1.OneToOne_Optional_FK.Name.ToUpper()) == "L2 01"
                    select e1,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Optional_navigation_inside_nested_method_call_translated_to_join()
        {
            AssertQuery<Level1>(
                l1s =>
                    from e1 in l1s
                    where e1.OneToOne_Optional_FK.Name.ToUpper().StartsWith("L")
                    select e1,
                l1s =>
                    from e1 in l1s
                    where MaybeScalar<bool>(e1.OneToOne_Optional_FK, () => e1.OneToOne_Optional_FK.Name.ToUpper().StartsWith("L")) == true
                    select e1,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Method_call_on_optional_navigation_translates_to_null_conditional_properly_for_arguments()
        {
            AssertQuery<Level1>(
                l1s =>
                    from e1 in l1s
                    where e1.OneToOne_Optional_FK.Name.StartsWith(e1.OneToOne_Optional_FK.Name)
                    select e1,
                l1s =>
                    from e1 in l1s
                    where MaybeScalar<bool>(e1.OneToOne_Optional_FK, () => e1.OneToOne_Optional_FK.Name.StartsWith(e1.OneToOne_Optional_FK.Name)) == true
                    select e1,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Optional_navigation_inside_method_call_translated_to_join_keeps_original_nullability()
        {
            AssertQuery<Level1>(
                l1s =>
                    from e1 in l1s
                    where e1.OneToOne_Optional_FK.Date.AddDays(10) > new DateTime(2000, 2, 1)
                    select e1,
                l1s =>
                    from e1 in l1s
                    where MaybeScalar<DateTime>(e1.OneToOne_Optional_FK, () => e1.OneToOne_Optional_FK.Date.AddDays(10)) > new DateTime(2000, 2, 1)
                    select e1,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Optional_navigation_inside_nested_method_call_translated_to_join_keeps_original_nullability()
        {
            AssertQuery<Level1>(
                l1s =>
                    from e1 in l1s
                    where e1.OneToOne_Optional_FK.Date.AddDays(10).AddDays(15).AddMonths(2) > new DateTime(2002, 2, 1)
                    select e1,
                l1s =>
                    from e1 in l1s
                    where MaybeScalar<DateTime>(e1.OneToOne_Optional_FK, () => e1.OneToOne_Optional_FK.Date.AddDays(10).AddDays(15).AddMonths(2)) > new DateTime(2000, 2, 1)
                    select e1,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Optional_navigation_inside_nested_method_call_translated_to_join_keeps_original_nullability_also_for_arguments()
        {
            AssertQuery<Level1>(
                l1s =>
                    from e1 in l1s
                    where e1.OneToOne_Optional_FK.Date.AddDays(15).AddDays(e1.OneToOne_Optional_FK.Id) > new DateTime(2002, 2, 1)
                    select e1,
                l1s =>
                    from e1 in l1s
                    where MaybeScalar<DateTime>(e1.OneToOne_Optional_FK, () => e1.OneToOne_Optional_FK.Date.AddDays(15).AddDays(e1.OneToOne_Optional_FK.Id)) > new DateTime(2000, 2, 1)
                    select e1,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Join_navigation_in_outer_selector_translated_to_extra_join()
        {
            AssertQuery<Level1, Level2>(
                (l1s, l2s) =>
                    from e1 in l1s
                    join e2 in l2s on e1.OneToOne_Optional_FK.Id equals e2.Id
                    select new { Id1 = e1.Id, Id2 = e2.Id },
                (l1s, l2s) =>
                    from e1 in l1s
                    join e2 in l2s on MaybeScalar<int>(e1.OneToOne_Optional_FK, () => e1.OneToOne_Optional_FK.Id) equals e2.Id
                    select new { Id1 = e1.Id, Id2 = e2.Id },
                e => e.Id1 + " " + e.Id2);
        }

        [ConditionalFact]
        public virtual void Join_navigation_in_outer_selector_translated_to_extra_join_nested()
        {
            AssertQuery<Level1, Level3>(
                (l1s, l3s) =>
                    from e1 in l1s
                    join e3 in l3s on e1.OneToOne_Required_FK.OneToOne_Optional_FK.Id equals e3.Id
                    select new { Id1 = e1.Id, Id3 = e3.Id },
                (l1s, l3s) =>
                    from e1 in l1s
                    join e3 in l3s on MaybeScalar(
                        e1.OneToOne_Required_FK,
                        () => MaybeScalar<int>(
                            e1.OneToOne_Required_FK.OneToOne_Optional_FK,
                            () => e1.OneToOne_Required_FK.OneToOne_Optional_FK.Id)) equals e3.Id
                    select new { Id1 = e1.Id, Id3 = e3.Id },
                e => e.Id1 + " " + e.Id3);
        }

        [ConditionalFact]
        public virtual void Join_navigation_in_outer_selector_translated_to_extra_join_nested2()
        {
            AssertQuery<Level1, Level3>(
                (l1s, l3s) =>
                    from e3 in l3s
                    join e1 in l1s on e3.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK_Inverse.Id equals e1.Id
                    select new { Id3 = e3.Id, Id1 = e1.Id },
                (l1s, l3s) =>
                    from e3 in l3s
                    join e1 in l1s on MaybeScalar<int>(
                        e3.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK_Inverse,
                        () => e3.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK_Inverse.Id) equals e1.Id
                    select new { Id3 = e3.Id, Id1 = e1.Id },
                e => e.Id1 + " " + e.Id3);
        }

        [ConditionalFact]
        public virtual void Join_navigation_in_inner_selector_translated_to_subquery()
        {
            AssertQuery<Level1, Level2>(
                (l1s, l2s) =>
                    from e2 in l2s
                    join e1 in l1s on e2.Id equals e1.OneToOne_Optional_FK.Id
                    select new { Id2 = e2.Id, Id1 = e1.Id },
                (l1s, l2s) =>
                    from e2 in l2s
                    join e1 in l1s on e2.Id equals MaybeScalar<int>(e1.OneToOne_Optional_FK, () => e1.OneToOne_Optional_FK.Id)
                    select new { Id2 = e2.Id, Id1 = e1.Id },
                e => e.Id2 + " " + e.Id1);
        }

        [ConditionalFact]
        public virtual void Join_navigations_in_inner_selector_translated_to_multiple_subquery_without_collision()
        {
            AssertQuery<Level1, Level2, Level3>(
                (l1s, l2s, l3s) =>
                    from e2 in l2s
                    join e1 in l1s on e2.Id equals e1.OneToOne_Optional_FK.Id
                    join e3 in l3s on e2.Id equals e3.OneToOne_Optional_FK_Inverse.Id
                    select new { Id2 = e2.Id, Id1 = e1.Id, Id3 = e3.Id },
                (l1s, l2s, l3s) =>
                    from e2 in l2s
                    join e1 in l1s on e2.Id equals MaybeScalar<int>(e1.OneToOne_Optional_FK, () => e1.OneToOne_Optional_FK.Id)
                    join e3 in l3s on e2.Id equals MaybeScalar<int>(e3.OneToOne_Optional_FK_Inverse, () => e3.OneToOne_Optional_FK_Inverse.Id)
                    select new { Id2 = e2.Id, Id1 = e1.Id, Id3 = e3.Id },
                e => e.Id2 + " " + e.Id1 + " " + e.Id3);
        }

        [ConditionalFact]
        public virtual void Join_navigation_translated_to_subquery_non_key_join()
        {
            AssertQuery<Level1, Level2>(
                (l1s, l2s) =>
                    from e2 in l2s
                    join e1 in l1s on e2.Name equals e1.OneToOne_Optional_FK.Name
                    select new { Id2 = e2.Id, Name2 = e2.Name, Id1 = e1.Id, Name1 = e1.Name },
                (l1s, l2s) =>
                    from e2 in l2s
                    join e1 in l1s on e2.Name equals Maybe(e1.OneToOne_Optional_FK, () => e1.OneToOne_Optional_FK.Name)
                    select new { Id2 = e2.Id, Name2 = e2.Name, Id1 = e1.Id, Name1 = e1.Name },
                e => e.Id2 + " " + e.Name2 + " " + e.Id1 + " " + e.Name1);
        }

        [ConditionalFact]
        public virtual void Join_navigation_translated_to_subquery_self_ref()
        {
            AssertQuery<Level1>(
                l1s =>
                    from e1 in l1s
                    join e2 in l1s on e1.Id equals e2.OneToMany_Optional_Self_Inverse.Id
                    select new { Id1 = e1.Id, Id2 = e2.Id },
                l1s =>
                    from e1 in l1s
                    join e2 in l1s on e1.Id equals MaybeScalar<int>(e2.OneToMany_Optional_Self_Inverse, () => e2.OneToMany_Optional_Self_Inverse.Id)
                    select new { Id1 = e1.Id, Id2 = e2.Id },
                e => e.Id1 + " " + e.Id2);
        }

        [ConditionalFact]
        public virtual void Join_navigation_translated_to_subquery_nested()
        {
            AssertQuery<Level1, Level3>(
                (l1s, l3s) =>
                    from e3 in l3s
                    join e1 in l1s on e3.Id equals e1.OneToOne_Required_FK.OneToOne_Optional_FK.Id
                    select new { Id3 = e3.Id, Id1 = e1.Id },
                (l1s, l3s) =>
                    from e3 in l3s
                    join e1 in l1s on e3.Id equals MaybeScalar(
                        e1.OneToOne_Required_FK,
                        () => MaybeScalar<int>(
                            e1.OneToOne_Required_FK.OneToOne_Optional_FK,
                            () => e1.OneToOne_Required_FK.OneToOne_Optional_FK.Id))
                    select new { Id3 = e3.Id, Id1 = e1.Id },
                e => e.Id3 + " " + e.Id1);
        }

        [ConditionalFact]
        public virtual void Join_navigation_translated_to_subquery_deeply_nested_non_key_join()
        {
            AssertQuery<Level1, Level4>(
                (l1s, l4s) =>
                    from e4 in l4s
                    join e1 in l1s on e4.Name equals e1.OneToOne_Required_FK.OneToOne_Optional_FK.OneToOne_Required_PK.Name
                    select new { Id4 = e4.Id, Name4 = e4.Name, Id1 = e1.Id, Name1 = e1.Name },
                (l1s, l4s) =>
                    from e4 in l4s
                    join e1 in l1s on e4.Name equals Maybe(
                        e1.OneToOne_Required_FK,
                        () => Maybe(
                            e1.OneToOne_Required_FK.OneToOne_Optional_FK,
                            () => Maybe(
                                e1.OneToOne_Required_FK.OneToOne_Optional_FK.OneToOne_Required_PK,
                                () => e1.OneToOne_Required_FK.OneToOne_Optional_FK.OneToOne_Required_PK.Name)))
                    select new { Id4 = e4.Id, Name4 = e4.Name, Id1 = e1.Id, Name1 = e1.Name },
                e => e.Id4 + " " + e.Name4 + " " + e.Id1 + " " + e.Name1);
        }

        [ConditionalFact]
        public virtual void Join_navigation_translated_to_subquery_deeply_nested_required()
        {
            AssertQuery<Level1, Level4>(
                (l1s, l4s) =>
                    from e1 in l1s
                    join e4 in l4s on e1.Name equals e4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Required_PK_Inverse.Name
                    select new { Id4 = e4.Id, Name4 = e4.Name, Id1 = e1.Id, Name1 = e1.Name },
                e => e.Id4 + " " + e.Name4 + " " + e.Id1 + " " + e.Name1);
        }

        [ConditionalFact]
        public virtual void Multiple_complex_includes()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK, "OneToOne_Optional_FK"),
                new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional, "OneToMany_Optional", navigationPath: "OneToOne_Optional_FK"),
                new ExpectedInclude<Level1>(l1 => l1.OneToMany_Optional, "OneToMany_Optional"),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK, "OneToOne_Optional_FK", navigationPath: "OneToMany_Optional")
            };

            AssertIncludeQuery<Level1>(
                l1s => l1s
                    .Include(e => e.OneToOne_Optional_FK)
                    .ThenInclude(e => e.OneToMany_Optional)
                    .Include(e => e.OneToMany_Optional)
                    .ThenInclude(e => e.OneToOne_Optional_FK),
                expectedIncludes,
                elementSorter: e => e.Id);
        }

        [ConditionalFact]
        public virtual void Multiple_complex_includes_self_ref()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_Self, "OneToOne_Optional_Self"),
                new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional_Self, "OneToMany_Optional_Self", navigationPath: "OneToOne_Optional_Self"),
                new ExpectedInclude<Level1>(l1 => l1.OneToMany_Optional_Self, "OneToMany_Optional_Self"),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_Self, "OneToOne_Optional_Self", navigationPath: "OneToMany_Optional_Self")
            };

            AssertIncludeQuery<Level1>(
                l1s => l1s
                    .Include(e => e.OneToOne_Optional_Self)
                    .ThenInclude(e => e.OneToMany_Optional_Self)
                    .Include(e => e.OneToMany_Optional_Self)
                    .ThenInclude(e => e.OneToOne_Optional_Self),
                expectedIncludes,
                elementSorter: e => e.Id);
        }

        [ConditionalFact]
        public virtual void Multiple_complex_include_select()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK, "OneToOne_Optional_FK"),
                new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional, "OneToMany_Optional", navigationPath: "OneToOne_Optional_FK"),
                new ExpectedInclude<Level1>(l1 => l1.OneToMany_Optional, "OneToMany_Optional"),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK, "OneToOne_Optional_FK", navigationPath: "OneToMany_Optional")
            };

            AssertIncludeQuery<Level1>(
                l1s => l1s
                    .Select(e => e)
                    .Include(e => e.OneToOne_Optional_FK)
                    .ThenInclude(e => e.OneToMany_Optional)
                    .Select(e => e)
                    .Include(e => e.OneToMany_Optional)
                    .ThenInclude(e => e.OneToOne_Optional_FK),
                expectedIncludes,
                elementSorter: e => e.Id);
        }

        [ConditionalFact]
        public virtual void Select_nav_prop_collection_one_to_many_required()
        {
            AssertQuery<Level1>(
                l1s => l1s.OrderBy(e => e.Id).Select(e => e.OneToMany_Required.Select(i => i.Id)),
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

        [ConditionalFact]
        public virtual void Select_nav_prop_reference_optional1()
        {
            AssertQuery<Level1>(
                l1s => l1s.Select(e => e.OneToOne_Optional_FK.Name),
                l1s => l1s.Select(e => Maybe(e.OneToOne_Optional_FK, () => e.OneToOne_Optional_FK.Name)));
        }

        [ConditionalFact]
        public virtual void Select_nav_prop_reference_optional1_via_DefaultIfEmpty()
        {
            AssertQuery<Level1, Level2>(
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

        [ConditionalFact]
        public virtual void Select_nav_prop_reference_optional2()
        {
            AssertQueryScalar<Level1>(
                l1s => l1s.Select(e => (int?)e.OneToOne_Optional_FK.Id),
                l1s => l1s.Select(e => MaybeScalar<int>(e.OneToOne_Optional_FK, () => e.OneToOne_Optional_FK.Id)));
        }

        [ConditionalFact]
        public virtual void Select_nav_prop_reference_optional2_via_DefaultIfEmpty()
        {
            AssertQueryScalar<Level1, Level2>(
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

        [ConditionalFact]
        public virtual void Select_nav_prop_reference_optional3()
        {
            AssertQuery<Level2>(
                l2s => l2s.Select(e => e.OneToOne_Optional_FK_Inverse.Name),
                l2s => l2s.Select(e => Maybe(e.OneToOne_Optional_FK_Inverse, () => e.OneToOne_Optional_FK_Inverse.Name)));
        }

        [ConditionalFact]
        public virtual void Where_nav_prop_reference_optional1()
        {
            AssertQueryScalar<Level1>(
                l1s => l1s
                    .Where(e => e.OneToOne_Optional_FK.Name == "L2 05" || e.OneToOne_Optional_FK.Name == "L2 07")
                    .Select(e => e.Id),
                l1s => l1s
                    .Where(
                        e => Maybe(e.OneToOne_Optional_FK, () => e.OneToOne_Optional_FK.Name) == "L2 05"
                             || Maybe(e.OneToOne_Optional_FK, () => e.OneToOne_Optional_FK.Name) == "L2 07")
                    .Select(e => e.Id));
        }

        [ConditionalFact]
        public virtual void Where_nav_prop_reference_optional1_via_DefaultIfEmpty()
        {
            AssertQueryScalar<Level1, Level2>(
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

        [ConditionalFact]
        public virtual void Where_nav_prop_reference_optional2()
        {
            AssertQueryScalar<Level1>(
                l1s => l1s
                    .Where(e => e.OneToOne_Optional_FK.Name == "L2 05" || e.OneToOne_Optional_FK.Name != "L2 42")
                    .Select(e => e.Id),
                l1s => l1s
                    .Where(
                        e => Maybe(e.OneToOne_Optional_FK, () => e.OneToOne_Optional_FK.Name) == "L2 05"
                             || Maybe(e.OneToOne_Optional_FK, () => e.OneToOne_Optional_FK.Name) != "L2 42")
                    .Select(e => e.Id));
        }

        [ConditionalFact]
        public virtual void Where_nav_prop_reference_optional2_via_DefaultIfEmpty()
        {
            AssertQueryScalar<Level1, Level2>(
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

        [ConditionalFact]
        public virtual void Select_multiple_nav_prop_reference_optional()
        {
            AssertQueryScalar<Level1>(
                l1s => l1s.Select(e => (int?)e.OneToOne_Optional_FK.OneToOne_Optional_FK.Id),
                l1s => l1s.Select(
                    e => MaybeScalar(
                        e.OneToOne_Optional_FK,
                        () => MaybeScalar<int>(
                            e.OneToOne_Optional_FK.OneToOne_Optional_FK,
                            () => e.OneToOne_Optional_FK.OneToOne_Optional_FK.Id))));
        }

        [ConditionalFact]
        public virtual void Where_multiple_nav_prop_reference_optional_member_compared_to_value()
        {
            AssertQuery<Level1>(
                l1s =>
                    from l1 in l1s
                    where l1.OneToOne_Optional_FK.OneToOne_Optional_FK.Name != "L3 05"
                    select l1,
                l1s =>
                    from l1 in l1s
                    where Maybe(
                              l1.OneToOne_Optional_FK,
                              () => Maybe(
                                  l1.OneToOne_Optional_FK.OneToOne_Optional_FK,
                                  () => l1.OneToOne_Optional_FK.OneToOne_Optional_FK.Name)) != "L3 05"
                    select l1,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Where_multiple_nav_prop_reference_optional_member_compared_to_null()
        {
            AssertQuery<Level1>(
                l1s =>
                    from l1 in l1s
                    where l1.OneToOne_Optional_FK.OneToOne_Optional_FK.Name != null
                    select l1,
                l1s =>
                    from l1 in l1s
                    where Maybe(
                              l1.OneToOne_Optional_FK,
                              () => Maybe(
                                  l1.OneToOne_Optional_FK.OneToOne_Optional_FK,
                                  () => l1.OneToOne_Optional_FK.OneToOne_Optional_FK.Name)) != null
                    select l1,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Where_multiple_nav_prop_reference_optional_compared_to_null1()
        {
            AssertQuery<Level1>(
                l1s =>
                    from l1 in l1s
                    where l1.OneToOne_Optional_FK.OneToOne_Optional_FK == null
                    select l1,
                l1s =>
                    from l1 in l1s
                    where Maybe(
                              l1.OneToOne_Optional_FK,
                              () => l1.OneToOne_Optional_FK.OneToOne_Optional_FK) == null
                    select l1,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Where_multiple_nav_prop_reference_optional_compared_to_null2()
        {
            AssertQuery<Level3>(
                l3s =>
                    from l3 in l3s
                    where l3.OneToOne_Optional_FK_Inverse.OneToOne_Optional_FK_Inverse == null
                    select l3,
                l3s =>
                    from l3 in l3s
                    where Maybe(
                              l3.OneToOne_Optional_FK_Inverse,
                              () => l3.OneToOne_Optional_FK_Inverse.OneToOne_Optional_FK_Inverse) == null
                    select l3,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Where_multiple_nav_prop_reference_optional_compared_to_null3()
        {
            AssertQuery<Level1>(
                l1s =>
                    from l1 in l1s
                    where null != l1.OneToOne_Optional_FK.OneToOne_Optional_FK
                    select l1,
                l1s =>
                    from l1 in l1s
                    where null != Maybe(
                              l1.OneToOne_Optional_FK,
                              () => l1.OneToOne_Optional_FK.OneToOne_Optional_FK)
                    select l1,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Where_multiple_nav_prop_reference_optional_compared_to_null4()
        {
            AssertQuery<Level3>(
                l3s =>
                    from l3 in l3s
                    where null != l3.OneToOne_Optional_FK_Inverse.OneToOne_Optional_FK_Inverse
                    select l3,
                l3s =>
                    from l3 in l3s
                    where null != Maybe(l3.OneToOne_Optional_FK_Inverse, () => l3.OneToOne_Optional_FK_Inverse.OneToOne_Optional_FK_Inverse)
                    select l3,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Where_multiple_nav_prop_reference_optional_compared_to_null5()
        {
            AssertQuery<Level1>(
                l1s => l1s.Where(e => e.OneToOne_Optional_FK.OneToOne_Required_FK.OneToOne_Required_FK == null),
                l1s => l1s.Where(
                    e => Maybe(
                             e.OneToOne_Optional_FK,
                             () => Maybe(
                                 e.OneToOne_Optional_FK.OneToOne_Required_FK,
                                 () => e.OneToOne_Optional_FK.OneToOne_Required_FK.OneToOne_Required_FK)) == null),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Select_multiple_nav_prop_reference_required()
        {
            AssertQueryScalar<Level1>(
                l1s => l1s.Select(e => (int?)e.OneToOne_Required_FK.OneToOne_Required_FK.Id),
                l1s => l1s.Select(
                    e => MaybeScalar(
                        e.OneToOne_Required_FK,
                        () => MaybeScalar<int>(
                            e.OneToOne_Required_FK.OneToOne_Required_FK,
                            () => e.OneToOne_Required_FK.OneToOne_Required_FK.Id))));
        }

        [ConditionalFact]
        public virtual void Select_multiple_nav_prop_reference_required2()
        {
            AssertQueryScalar<Level3>(
                l3s => l3s.Select(e => e.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.Id));
        }

        [ConditionalFact]
        public virtual void Select_multiple_nav_prop_optional_required()
        {
            AssertQueryScalar<Level1>(
                l1s =>
                    from l1 in l1s
                    select (int?)l1.OneToOne_Optional_FK.OneToOne_Required_FK.Id,
                l1s =>
                    from l1 in l1s
                    select MaybeScalar(
                        l1.OneToOne_Optional_FK,
                        () => MaybeScalar<int>(
                            l1.OneToOne_Optional_FK.OneToOne_Required_FK,
                            () => l1.OneToOne_Optional_FK.OneToOne_Required_FK.Id)));
        }

        [ConditionalFact]
        public virtual void Where_multiple_nav_prop_optional_required()
        {
            AssertQuery<Level1>(
                l1s =>
                    from l1 in l1s
                    where l1.OneToOne_Optional_FK.OneToOne_Required_FK.Name != "L3 05"
                    select l1,
                l1s =>
                    from l1 in l1s
                    where Maybe(
                              l1.OneToOne_Optional_FK,
                              () => Maybe(
                                  l1.OneToOne_Optional_FK.OneToOne_Required_FK,
                                  () => l1.OneToOne_Optional_FK.OneToOne_Required_FK.Name)) != "L3 05"
                    select l1,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void SelectMany_navigation_comparison1()
        {
            AssertQuery<Level1>(
                l1s =>
                    from l11 in l1s
                    from l12 in l1s
                    where l11 == l12
                    select new { Id1 = l11.Id, Id2 = l12.Id },
                l1s =>
                    from l11 in l1s
                    from l12 in l1s
                    where l11.Id == l12.Id
                    select new { Id1 = l11.Id, Id2 = l12.Id },
                e => e.Id1 + " " + e.Id2);
        }

        [ConditionalFact]
        public virtual void SelectMany_navigation_comparison2()
        {
            AssertQuery<Level1, Level2>(
                (l1s, l2s) =>
                    from l1 in l1s
                    from l2 in l2s
                    where l1 == l2.OneToOne_Optional_FK_Inverse
                    select new { Id1 = l1.Id, Id2 = l2.Id },
                (l1s, l2s) =>
                    from l1 in l1s
                    from l2 in l2s
                    where l1.Id == MaybeScalar<int>(l2.OneToOne_Optional_FK_Inverse, () => l2.OneToOne_Optional_FK_Inverse.Id)
                    select new { Id1 = l1.Id, Id2 = l2.Id },
                e => e.Id1 + " " + e.Id2);
        }

        [ConditionalFact]
        public virtual void SelectMany_navigation_comparison3()
        {
            AssertQuery<Level1, Level2>(
                (l1s, l2s) =>
                    from l1 in l1s
                    from l2 in l2s
                    where l1.OneToOne_Optional_FK == l2
                    select new { Id1 = l1.Id, Id2 = l2.Id },
                (l1s, l2s) =>
                    from l1 in l1s
                    from l2 in l2s
                    where MaybeScalar<int>(l1.OneToOne_Optional_FK, () => l1.OneToOne_Optional_FK.Id) == l2.Id
                    select new { Id1 = l1.Id, Id2 = l2.Id },
                e => e.Id1 + " " + e.Id2);
        }

        [ConditionalFact]
        public virtual void Where_complex_predicate_with_with_nav_prop_and_OrElse1()
        {
            AssertQuery<Level1, Level2>(
                (l1s, l2s) =>
                    from l1 in l1s
                    from l2 in l2s
                    where l1.OneToOne_Optional_FK.Name == "L2 01" || l2.OneToOne_Required_FK_Inverse.Name != "Bar"
                    select new { Id1 = (int?)l1.Id, Id2 = (int?)l2.Id },
                (l1s, l2s) =>
                    from l1 in l1s
                    from l2 in l2s
                    where Maybe(l1.OneToOne_Optional_FK, () => l1.OneToOne_Optional_FK.Name) == "L2 01"
                          || l2.OneToOne_Required_FK_Inverse.Name != "Bar"
                    select new { Id1 = (int?)l1.Id, Id2 = (int?)l2.Id },
                e => e.Id1 + " " + e.Id2);
        }

        [ConditionalFact]
        public virtual void Where_complex_predicate_with_with_nav_prop_and_OrElse2()
        {
            AssertQueryScalar<Level1>(
                l1s =>
                    from l1 in l1s
                    where l1.OneToOne_Optional_FK.OneToOne_Required_FK.Name == "L3 05" || l1.OneToOne_Optional_FK.Name != "L2 05"
                    select l1.Id,
                l1s =>
                    from l1 in l1s
                    where Maybe(
                              l1.OneToOne_Optional_FK,
                              () => Maybe(
                                  l1.OneToOne_Optional_FK.OneToOne_Required_FK,
                                  () => l1.OneToOne_Optional_FK.OneToOne_Required_FK.Name)) == "L3 05"
                          || Maybe(
                              l1.OneToOne_Optional_FK,
                              () => l1.OneToOne_Optional_FK.Name) != "L2 05"
                    select l1.Id);
        }

        [ConditionalFact]
        public virtual void Where_complex_predicate_with_with_nav_prop_and_OrElse3()
        {
            AssertQueryScalar<Level1>(
                l1s =>
                    from l1 in l1s
                    where l1.OneToOne_Optional_FK.Name != "L2 05" || l1.OneToOne_Required_FK.OneToOne_Optional_FK.Name == "L3 05"
                    select l1.Id,
                l1s =>
                    from l1 in l1s
                    where Maybe(
                              l1.OneToOne_Optional_FK,
                              () => l1.OneToOne_Optional_FK.Name) != "L2 05"
                          || Maybe(
                              l1.OneToOne_Required_FK,
                              () => Maybe(
                                  l1.OneToOne_Required_FK.OneToOne_Optional_FK,
                                  () => l1.OneToOne_Required_FK.OneToOne_Optional_FK.Name)) == "L3 05"
                    select l1.Id);
        }

        [ConditionalFact]
        public virtual void Where_complex_predicate_with_with_nav_prop_and_OrElse4()
        {
            AssertQueryScalar<Level3>(
                l3s =>
                    from l3 in l3s
                    where l3.OneToOne_Optional_FK_Inverse.Name != "L2 05" || l3.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK_Inverse.Name == "L1 05"
                    select l3.Id,
                l3s =>
                    from l3 in l3s
                    where Maybe(
                              l3.OneToOne_Optional_FK_Inverse,
                              () => l3.OneToOne_Optional_FK_Inverse.Name) != "L2 05"
                          || Maybe(
                              l3.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK_Inverse,
                              () => l3.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK_Inverse.Name) == "L1 05"
                    select l3.Id);
        }

        [ConditionalFact]
        public virtual void Complex_navigations_with_predicate_projected_into_anonymous_type()
        {
            AssertQuery<Level1>(
                l1s => l1s
                    .Where(
                        e => e.OneToOne_Required_FK.OneToOne_Required_FK == e.OneToOne_Required_FK.OneToOne_Optional_FK
                             && e.OneToOne_Required_FK.OneToOne_Optional_FK.Id != 7)
                    .Select(
                        e => new
                        {
                            e.Name,
                            Id = (int?)e.OneToOne_Required_FK.OneToOne_Optional_FK.Id
                        }),
                l1s => l1s
                    .Where(
                        e => Maybe(e.OneToOne_Required_FK, () => e.OneToOne_Required_FK.OneToOne_Required_FK) == Maybe(e.OneToOne_Required_FK, () => e.OneToOne_Required_FK.OneToOne_Optional_FK)
                             && MaybeScalar(e.OneToOne_Required_FK, () => MaybeScalar<int>(e.OneToOne_Required_FK.OneToOne_Optional_FK, () => e.OneToOne_Required_FK.OneToOne_Optional_FK.Id)) != 7)
                    .Select(
                        e => new
                        {
                            e.Name,
                            Id = MaybeScalar(e.OneToOne_Required_FK, () => MaybeScalar<int>(e.OneToOne_Required_FK.OneToOne_Optional_FK, () => e.OneToOne_Required_FK.OneToOne_Optional_FK.Id))
                        }),
                elementSorter: e => e.Name + " " + e.Id,
                elementAsserter: (e, a) =>
                    {
                        Assert.Equal(e.Name, a.Name);
                        Assert.Equal(e.Id, a.Id);
                    });
        }

        [ConditionalFact]
        public virtual void Complex_navigations_with_predicate_projected_into_anonymous_type2()
        {
            AssertQuery<Level3>(
                l3s =>
                    from e in l3s
                    where e.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse == e.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK_Inverse
                          && e.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK_Inverse.Id != 7
                    select new
                    {
                        e.Name,
                        Id = (int?)e.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK_Inverse.Id
                    },
                l3s =>
                    from e in l3s
                    where e.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse == e.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK_Inverse
                          && MaybeScalar<int>(
                              e.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK_Inverse,
                              () => e.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK_Inverse.Id) != 7
                    select new
                    {
                        e.Name,
                        Id = MaybeScalar<int>(
                            e.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK_Inverse,
                            () => e.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK_Inverse.Id)
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
                        Inner = e.OneToOne_Optional_FK != null
                            ? new MyInnerDto
                            {
                                Id = (int?)e.OneToOne_Optional_FK.Id,
                                Name = e.OneToOne_Optional_FK.Name
                            }
                            : null
                    }).ToList().OrderBy(e => e.Id + " " + e.Name + " " + e.Inner).ToList();

                var expected = Fixture.QueryAsserter.ExpectedData.Set<Level1>().Select(
                    e => new MyOuterDto
                    {
                        Id = e.Id,
                        Name = e.Name,
                        Inner = e.OneToOne_Optional_FK != null
                            ? new MyInnerDto
                            {
                                Id = (int?)e.OneToOne_Optional_FK.Id,
                                Name = e.OneToOne_Optional_FK.Name
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

        [ConditionalFact]
        public virtual void OrderBy_nav_prop_reference_optional()
        {
            AssertQueryScalar<Level1>(
                l1s =>
                    l1s.OrderBy(e => e.OneToOne_Optional_FK.Name).ThenBy(e => e.Id).Select(e => e.Id),
                l1s =>
                    l1s.OrderBy(e => Maybe(e.OneToOne_Optional_FK, () => e.OneToOne_Optional_FK.Name)).ThenBy(e => e.Id).Select(e => e.Id),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void OrderBy_nav_prop_reference_optional_via_DefaultIfEmpty()
        {
            AssertQueryScalar<Level1, Level2>(
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

        [ConditionalFact]
        public virtual void Result_operator_nav_prop_reference_optional_Sum()
        {
            AssertSingleResult<Level1>(
                l1s => l1s.Sum(e => (int?)e.OneToOne_Optional_FK.Level1_Required_Id),
                l1s => l1s.Sum(e => MaybeScalar<int>(e.OneToOne_Optional_FK, () => e.OneToOne_Optional_FK.Level1_Required_Id)));
        }

        [ConditionalFact]
        public virtual void Result_operator_nav_prop_reference_optional_Min()
        {
            AssertSingleResult<Level1>(
                l1s => l1s.Min(e => (int?)e.OneToOne_Optional_FK.Level1_Required_Id),
                l1s => l1s.Min(e => MaybeScalar<int>(e.OneToOne_Optional_FK, () => e.OneToOne_Optional_FK.Level1_Required_Id)));
        }

        [ConditionalFact]
        public virtual void Result_operator_nav_prop_reference_optional_Max()
        {
            AssertSingleResult<Level1>(
                l1s => l1s.Max(e => (int?)e.OneToOne_Optional_FK.Level1_Required_Id),
                l1s => l1s.Max(e => MaybeScalar<int>(e.OneToOne_Optional_FK, () => e.OneToOne_Optional_FK.Level1_Required_Id)));
        }

        [ConditionalFact]
        public virtual void Result_operator_nav_prop_reference_optional_Average()
        {
            AssertSingleResult<Level1>(
                l1s => l1s.Average(e => (int?)e.OneToOne_Optional_FK.Level1_Required_Id),
                l1s => l1s.Average(e => MaybeScalar<int>(e.OneToOne_Optional_FK, () => e.OneToOne_Optional_FK.Level1_Required_Id)));
        }

        [ConditionalFact]
        public virtual void Result_operator_nav_prop_reference_optional_via_DefaultIfEmpty()
        {
            AssertSingleResult<Level1, Level2>(
                (l1s, l2s) =>
                    (from l1 in l1s
                     join l2 in l2s on l1.Id equals l2.Level1_Optional_Id into groupJoin
                     from l2 in groupJoin.DefaultIfEmpty()
                     select l2).Sum(e => e == null ? 0 : e.Level1_Required_Id));
        }

        [ConditionalFact]
        public virtual void Include_with_optional_navigation()
        {
            AssertIncludeQuery<Level1>(
                l1s => from l1 in l1s.Include(e => e.OneToOne_Optional_FK)
                       where l1.OneToOne_Optional_FK.Name != "L2 05"
                       select l1,
                l1s => from l1 in l1s.Include(e => e.OneToOne_Optional_FK)
                       where Maybe(l1.OneToOne_Optional_FK, () => l1.OneToOne_Optional_FK.Name) != "L2 05"
                       select l1,
                new List<IExpectedInclude> { new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK, "OneToOne_Optional_FK") },
                elementSorter: e => e.Id);
        }

        [ConditionalFact]
        public virtual void Include_nested_with_optional_navigation()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK, "OneToOne_Optional_FK"),
                new ExpectedInclude<Level2>(l1 => l1.OneToMany_Required, "OneToMany_Required", "OneToOne_Optional_FK"),
                new ExpectedInclude<Level3>(l1 => l1.OneToOne_Required_FK, "OneToOne_Required_FK", "OneToOne_Optional_FK.OneToMany_Required")
            };

            AssertIncludeQuery<Level1>(
                l1s => from l1 in l1s
                           .Include(e => e.OneToOne_Optional_FK.OneToMany_Required)
                           .ThenInclude(e => e.OneToOne_Required_FK)
                       where l1.OneToOne_Optional_FK.Name != "L2 09"
                       select l1,
                l1s => from l1 in l1s
                       where Maybe(l1.OneToOne_Optional_FK, () => l1.OneToOne_Optional_FK.Name) != "L2 09"
                       select l1,
                expectedIncludes,
                elementSorter: l1 => l1.Id);
        }

        [ConditionalFact]
        public virtual void Include_with_groupjoin_skip_and_take()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToMany_Optional, "OneToMany_Optional"),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK, "OneToOne_Optional_FK", "OneToMany_Optional"),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Required_PK, "OneToOne_Required_PK")
            };

            AssertIncludeQuery<Level1, Level2>(
                (l1s, l2s) =>
                    (from l1 in l1s
                         .Include(e => e.OneToMany_Optional)
                         .ThenInclude(e => e.OneToOne_Optional_FK)
                     join l2 in l2s.Include(e => e.OneToOne_Required_PK)
#pragma warning disable IDE0031 // Use null propagation
                         on (int?)l1.Id equals l2 != null ? l2.Level1_Optional_Id : null into grouping
#pragma warning restore IDE0031 // Use null propagation
                     where l1.Name != "L1 03"
                     orderby l1.Id
                     select new { l1, grouping }).Skip(1).Take(5),
                expectedIncludes,
                clientProjections: new List<Func<dynamic, object>> { e => new KeyValuePair<Level1, IEnumerable<Level2>>(e.l1, ((IEnumerable<Level2>)e.grouping).ToList()) });
        }

        [ConditionalFact]
        public virtual void Join_flattening_bug_4539()
        {
            AssertQuery<Level1, Level2>(
                (l1s, l2s) =>
                    from l1 in l1s
                    join l1_Optional in l2s on (int?)l1.Id equals l1_Optional.Level1_Optional_Id into grouping
                    from l1_Optional in grouping.DefaultIfEmpty()
                    from l2 in l2s
                    join l2_Required_Reverse in l1s on l2.Level1_Required_Id equals l2_Required_Reverse.Id
                    select new { l1_Optional, l2_Required_Reverse },
                elementSorter: e => e.l1_Optional?.Id + " " + e.l2_Required_Reverse.Id);
        }

        [ConditionalFact]
        public virtual void Query_source_materialization_bug_4547()
        {
            AssertQueryScalar<Level1, Level2, Level3>(
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

        [ConditionalFact]
        public virtual void SelectMany_navigation_property()
        {
            AssertQuery<Level1>(
                l1s => l1s.SelectMany(l1 => l1.OneToMany_Optional),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void SelectMany_navigation_property_and_projection()
        {
            AssertQuery<Level1>(
                l1s => l1s.SelectMany(l1 => l1.OneToMany_Optional).Select(e => e.Name));
        }

        [ConditionalFact]
        public virtual void SelectMany_navigation_property_and_filter_before()
        {
            AssertQuery<Level1>(
                l1s => l1s.Where(e => e.Id == 1).SelectMany(l1 => l1.OneToMany_Optional),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void SelectMany_navigation_property_and_filter_after()
        {
            AssertQuery<Level1>(
                l1s => l1s.SelectMany(l1 => l1.OneToMany_Optional).Where(e => e.Id != 6),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void SelectMany_nested_navigation_property_required()
        {
            AssertQuery<Level1>(
                l1s => l1s.SelectMany(l1 => l1.OneToOne_Required_FK.OneToMany_Optional),
                l1s => l1s.SelectMany(
                    l1 => Maybe(
                              l1.OneToOne_Required_FK,
                              () => l1.OneToOne_Required_FK.OneToMany_Optional) ?? new List<Level3>()),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void SelectMany_nested_navigation_property_optional_and_projection()
        {
            AssertQuery<Level1>(
                l1s => l1s.SelectMany(l1 => l1.OneToOne_Optional_FK.OneToMany_Optional).Select(e => e.Name),
                l1s => l1s.SelectMany(
                    l1 => Maybe(
                              l1.OneToOne_Optional_FK,
                              () => l1.OneToOne_Optional_FK.OneToMany_Optional) ?? new List<Level3>()).Select(e => e.Name));
        }

        [ConditionalFact]
        public virtual void Multiple_SelectMany_calls()
        {
            AssertQuery<Level1>(
                l1s => l1s.SelectMany(e => e.OneToMany_Optional).SelectMany(e => e.OneToMany_Optional),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void SelectMany_navigation_property_with_another_navigation_in_subquery()
        {
            AssertQuery<Level1>(
                l1s => l1s.SelectMany(l1 => l1.OneToMany_Optional.Select(l2 => l2.OneToOne_Optional_FK)),
                l1s => l1s.SelectMany(
                    l1 => Maybe(
                              l1.OneToMany_Optional,
                              () => l1.OneToMany_Optional.Select(l2 => l2.OneToOne_Optional_FK)) ?? new List<Level3>()),
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

        [ConditionalFact]
        public virtual void Where_navigation_property_to_collection()
        {
            AssertQuery<Level1>(
                l1s => l1s.Where(l1 => l1.OneToOne_Required_FK.OneToMany_Optional.Count > 0),
                l1s => l1s.Where(
                    l1 => MaybeScalar(
                              l1.OneToOne_Required_FK,
                              () => MaybeScalar<int>(
                                  l1.OneToOne_Required_FK.OneToMany_Optional,
                                  () => l1.OneToOne_Required_FK.OneToMany_Optional.Count)) > 0),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Where_navigation_property_to_collection2()
        {
            AssertQuery<Level3>(
                l3s => l3s.Where(l1 => l1.OneToOne_Required_FK_Inverse.OneToMany_Optional.Count > 0),
                l3s => l3s.Where(
                    l1 => MaybeScalar<int>(
                              l1.OneToOne_Required_FK_Inverse.OneToMany_Optional,
                              () => l1.OneToOne_Required_FK_Inverse.OneToMany_Optional.Count) > 0),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Where_navigation_property_to_collection_of_original_entity_type()
        {
            AssertQuery<Level2>(
                l2s => l2s.Where(l2 => l2.OneToMany_Required_Inverse.OneToMany_Optional.Count() > 0),
                l2s => l2s.Where(
                    l2 => MaybeScalar<int>(
                              l2.OneToMany_Required_Inverse.OneToMany_Optional,
                              () => l2.OneToMany_Required_Inverse.OneToMany_Optional.Count()) > 0),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Complex_multi_include_with_order_by_and_paging()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Required_FK, "OneToOne_Required_FK"),
                new ExpectedInclude<Level2>(l1 => l1.OneToMany_Optional, "OneToMany_Optional", "OneToOne_Required_FK"),
                new ExpectedInclude<Level2>(l1 => l1.OneToMany_Required, "OneToMany_Required", "OneToOne_Required_FK")
            };

            AssertIncludeQuery<Level1>(
                l1s => l1s
                    .Include(e => e.OneToOne_Required_FK).ThenInclude(e => e.OneToMany_Optional)
                    .Include(e => e.OneToOne_Required_FK).ThenInclude(e => e.OneToMany_Required)
                    .OrderBy(t => t.Name)
                    .Skip(0).Take(10),
                expectedIncludes,
                elementSorter: e => e.Id);
        }

        [ConditionalFact]
        public virtual void Complex_multi_include_with_order_by_and_paging_joins_on_correct_key()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK, "OneToOne_Optional_FK"),
                new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional, "OneToMany_Optional", "OneToOne_Optional_FK"),
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Required_FK, "OneToOne_Required_FK"),
                new ExpectedInclude<Level2>(l2 => l2.OneToMany_Required, "OneToMany_Required", "OneToOne_Required_FK")
            };

            AssertIncludeQuery<Level1>(
                l1s => l1s
                    .Include(e => e.OneToOne_Optional_FK).ThenInclude(e => e.OneToMany_Optional)
                    .Include(e => e.OneToOne_Required_FK).ThenInclude(e => e.OneToMany_Required)
                    .OrderBy(t => t.Name)
                    .Skip(0).Take(10),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void Complex_multi_include_with_order_by_and_paging_joins_on_correct_key2()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK, "OneToOne_Optional_FK"),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Required_FK, "OneToOne_Required_FK", "OneToOne_Optional_FK"),
                new ExpectedInclude<Level3>(l3 => l3.OneToMany_Optional, "OneToMany_Optional", "OneToOne_Optional_FK.OneToOne_Optional_FK")
            };

            AssertIncludeQuery<Level1>(
                l1s => l1s
                    .Include(e => e.OneToOne_Optional_FK.OneToOne_Required_FK).ThenInclude(e => e.OneToMany_Optional)
                    .OrderBy(t => t.Name)
                    .Skip(0).Take(10),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void Multiple_include_with_multiple_optional_navigations()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Required_FK, "OneToOne_Required_FK"),
                new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional, "OneToMany_Optional", "OneToOne_Required_FK"),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK, "OneToOne_Optional_FK", "OneToOne_Required_FK"),
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK, "OneToOne_Optional_FK"),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK, "OneToOne_Optional_FK", "OneToOne_Optional_FK")
            };

            AssertIncludeQuery<Level1>(
                l1s => l1s
                    .Include(e => e.OneToOne_Required_FK).ThenInclude(e => e.OneToMany_Optional)
                    .Include(e => e.OneToOne_Required_FK).ThenInclude(e => e.OneToOne_Optional_FK)
                    .Include(e => e.OneToOne_Optional_FK).ThenInclude(e => e.OneToOne_Optional_FK)
                    .Where(e => e.OneToOne_Required_FK.OneToOne_Optional_PK.Name != "Foo")
                    .OrderBy(e => e.Id),
                l1s => l1s
                    .Where(
                        e => Maybe(
                                 e.OneToOne_Required_FK,
                                 () => Maybe(e.OneToOne_Required_FK.OneToOne_Optional_PK, () => e.OneToOne_Required_FK.OneToOne_Optional_PK.Name)) != "Foo")
                    .OrderBy(e => e.Id),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void Correlated_subquery_doesnt_project_unnecessary_columns_in_top_level()
        {
            AssertQuery<Level1, Level2>(
                (l1s, l2s) =>
                    (from l1 in l1s
                     where l2s.Any(l2 => l2.Level1_Required_Id == l1.Id)
                     select l1.Name).Distinct());
        }

        [ConditionalFact]
        public virtual void Correlated_subquery_doesnt_project_unnecessary_columns_in_top_level_join()
        {
            AssertQuery<Level1, Level2>(
                (l1s, l2s) =>
                    from e1 in l1s
                    join e2 in l2s on e1.Id equals e2.OneToOne_Optional_FK_Inverse.Id
                    where l2s.Any(l2 => l2.Level1_Required_Id == e1.Id)
                    select new { Name1 = e1.Name, Id2 = e2.Id },
                (l1s, l2s) =>
                    from e1 in l1s
                    join e2 in l2s on e1.Id equals MaybeScalar<int>(e2.OneToOne_Optional_FK_Inverse, () => e2.OneToOne_Optional_FK_Inverse.Id)
                    where l2s.Any(l2 => l2.Level1_Required_Id == e1.Id)
                    select new { Name1 = e1.Name, Id2 = e2.Id },
                e => e.Name1 + " " + e.Id2);
        }

        [ConditionalFact]
        public virtual void Correlated_nested_subquery_doesnt_project_unnecessary_columns_in_top_level()
        {
            AssertQuery<Level1, Level2, Level3>(
                (l1s, l2s, l3s) =>
                    (from l1 in l1s
                     where l2s.Any(l2 => l3s.Select(l3 => l2.Id).Any())
                     select l1.Name).Distinct()
            );
        }

        [ConditionalFact]
        public virtual void Correlated_nested_two_levels_up_subquery_doesnt_project_unnecessary_columns_in_top_level()
        {
            AssertQuery<Level1, Level2, Level3>(
                (l1s, l2s, l3s) =>
                    (from l1 in l1s
                     where l2s.Any(l2 => l3s.Select(l3 => l1.Id).Any())
                     select l1.Name).Distinct()
            );
        }

        [ConditionalFact]
        public virtual void GroupJoin_on_subquery_and_set_operation_on_grouping_but_nothing_from_grouping_is_projected()
        {
            AssertQuery<Level1, Level2>(
                (l1s, l2s) =>
                    l1s.GroupJoin(
                            l2s.Where(l2 => l2.Name != "L2 01"),
                            l1 => l1.Id,
                            l2 => l2.Level1_Optional_Id,
                            (l1, l2g) => new { l1, l2g })
                        .Where(r => r.l2g.Any())
                        .Select(r => r.l1),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void GroupJoin_on_complex_subquery_and_set_operation_on_grouping_but_nothing_from_grouping_is_projected()
        {
            AssertQuery<Level1>(
                l1s =>
                    l1s.GroupJoin(
                            l1s.Where(l1 => l1.Name != "L1 01").Select(l1 => l1.OneToOne_Required_FK),
                            l1 => l1.Id,
                            l2 => l2 != null ? l2.Level1_Optional_Id : null,
                            (l1, l2s) => new { l1, l2s })
                        .Where(r => r.l2s.Any())
                        .Select(r => r.l1),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Null_protection_logic_work_for_inner_key_access_of_manually_created_GroupJoin1()
        {
            AssertQuery<Level1>(
                l1s =>
                    l1s.GroupJoin(
                            l1s.Select(l1 => l1.OneToOne_Required_FK),
                            l1 => l1.Id,
                            l2 => MaybeScalar(l2, () => l2.Level1_Optional_Id),
                            (l1, l2s) => new { l1, l2s })
                        .Select(r => r.l1),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Null_protection_logic_work_for_inner_key_access_of_manually_created_GroupJoin2()
        {
            AssertQuery<Level1>(
                l1s =>
                    l1s.GroupJoin(
                            l1s.Select(l1 => l1.OneToOne_Required_FK),
                            l1 => l1.Id,
                            l2 => EF.Property<int?>(l2, "Level1_Optional_Id"),
                            (l1, l2s) => new { l1, l2s })
                        .Select(r => r.l1),
                l1s =>
                    l1s.GroupJoin(
                            l1s.Select(l1 => l1.OneToOne_Required_FK),
                            l1 => l1.Id,
                            l2 => MaybeScalar(l2, () => l2.Level1_Optional_Id),
                            (l1, l2s) => new { l1, l2s })
                        .Select(r => r.l1),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Null_protection_logic_work_for_outer_key_access_of_manually_created_GroupJoin()
        {
            AssertQuery<Level1>(
                l1s =>
                    l1s.Select(l1 => l1.OneToOne_Required_FK).GroupJoin(
                            l1s,
                            l2 => l2.Level1_Optional_Id,
                            l1 => l1.Id,
                            (l2, l1g) => new { l2, l1g })
                        .Select(r => r.l2),
                l1s =>
                    l1s.Select(l1 => l1.OneToOne_Required_FK).GroupJoin(
                            l1s,
                            l2 => MaybeScalar(l2, () => l2.Level1_Optional_Id),
                            l1 => l1.Id,
                            (l2, l1g) => new { l2, l1g })
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

        [ConditionalFact]
        public virtual void SelectMany_where_with_subquery()
        {
            AssertQuery<Level1>(
                l1s => l1s.SelectMany(l1 => l1.OneToMany_Required).Where(l2 => l2.OneToMany_Required.Any()),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access1()
        {
            AssertQuery<Level3>(
                l3s => l3s.OrderBy(l3 => l3.OneToOne_Required_FK_Inverse.Id).Select(l3 => l3.OneToOne_Required_FK_Inverse),
                elementAsserter: (e, a) => Assert.Equal(e.Id, a.Id),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access2()
        {
            AssertQuery<Level3>(
                l3s => l3s.OrderBy(l3 => l3.OneToOne_Required_FK_Inverse.Id).Select(l3 => EF.Property<Level2>(l3, "OneToOne_Required_FK_Inverse")),
                l3s => l3s.OrderBy(l3 => l3.OneToOne_Required_FK_Inverse.Id).Select(l3 => l3.OneToOne_Required_FK_Inverse),
                elementAsserter: (e, a) => Assert.Equal(e.Id, a.Id),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access3()
        {
            AssertQuery<Level3>(
                l3s => l3s.OrderBy(l3 => EF.Property<Level2>(l3, "OneToOne_Required_FK_Inverse").Id).Select(l3 => l3.OneToOne_Required_FK_Inverse),
                l3s => l3s.OrderBy(l3 => l3.OneToOne_Required_FK_Inverse.Id).Select(l3 => l3.OneToOne_Required_FK_Inverse),
                elementAsserter: (e, a) => Assert.Equal(e.Id, a.Id),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Order_by_key_of_navigation_similar_to_projected_gets_optimized_into_FK_access()
        {
            AssertQuery<Level3>(
                l3s => from l3 in l3s
                       orderby l3.OneToOne_Required_FK_Inverse.Id
                       select l3.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse,
                elementAsserter: (e, a) => Assert.Equal(e.Id, a.Id),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access_subquery()
        {
            AssertQuery<Level3>(
                l3s => l3s
                    .Select(l3 => l3.OneToOne_Required_FK_Inverse)
                    .OrderBy(l2 => l2.Id)
                    .Take(10)
                    .Select(l2 => l2.OneToOne_Required_FK_Inverse.Name),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Order_by_key_of_anonymous_type_projected_navigation_doesnt_get_optimized_into_FK_access_subquery()
        {
            AssertQuery<Level3>(
                l3s => l3s
                    .Select(l3 => new { l3.OneToOne_Required_FK_Inverse, name = l3.Name })
                    .OrderBy(l3 => l3.OneToOne_Required_FK_Inverse.Id)
                    .Take(10)
                    .Select(l2 => l2.OneToOne_Required_FK_Inverse.Name),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Optional_navigation_take_optional_navigation()
        {
            AssertQuery<Level1>(
                l1s => l1s
                    .Select(l1 => l1.OneToOne_Optional_FK)
                    .OrderBy(l2 => (int?)l2.Id)
                    .Take(10)
                    .Select(l2 => l2.OneToOne_Optional_FK.Name),
                l1s => l1s
                    .Select(l1 => l1.OneToOne_Optional_FK)
                    .OrderBy(l2 => MaybeScalar<int>(l2, () => l2.Id))
                    .Take(10)
                    .Select(l2 => Maybe(l2, () => Maybe(l2.OneToOne_Optional_FK, () => l2.OneToOne_Optional_FK.Name))),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Projection_select_correct_table_from_subquery_when_materialization_is_not_required()
        {
            AssertQuery<Level2>(
                l2s => l2s.Where(l2 => l2.OneToOne_Required_FK_Inverse.Name == "L1 03")
                    .OrderBy(l => l.Id).Take(3).Select(l2 => l2.Name));
        }

        [ConditionalFact]
        public virtual void Projection_select_correct_table_with_anonymous_projection_in_subquery()
        {
            AssertQuery<Level1, Level2, Level3>(
                (l1s, l2s, l3s) =>
                    (from l2 in l2s
                     join l1 in l1s
                         on l2.Level1_Required_Id equals l1.Id
                     join l3 in l3s
                         on l1.Id equals l3.Level2_Required_Id
                     where l1.Name == "L1 03"
                     where l3.Name == "L3 08"
                     select new { l2, l1 })
                    .OrderBy(l => l.l1.Id)
                    .Take(3)
                    .Select(l => l.l2.Name)
            );
        }

        [ConditionalFact]
        public virtual void Projection_select_correct_table_in_subquery_when_materialization_is_not_required_in_multiple_joins()
        {
            AssertQuery<Level1, Level2, Level3>(
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

        [ConditionalFact]
        public virtual void Where_predicate_on_optional_reference_navigation()
        {
            AssertQuery<Level1>(
                l1s => l1s
                    .Where(l1 => l1.OneToOne_Required_FK.Name == "L2 03")
                    .OrderBy(l1 => l1.Id)
                    .Take(3)
                    .Select(l1 => l1.Name),
                l1s => l1s
                    .Where(l1 => Maybe(l1.OneToOne_Required_FK, () => l1.OneToOne_Required_FK.Name) == "L2 03")
                    .OrderBy(l1 => l1.Id)
                    .Take(3)
                    .Select(l1 => l1.Name));
        }

        [ConditionalFact]
        public virtual void SelectMany_with_Include1()
        {
            AssertIncludeQuery<Level1>(
                l1s => l1s
                    .SelectMany(l1 => l1.OneToMany_Optional)
                    .Include(l2 => l2.OneToMany_Optional),
                expectedIncludes: new List<IExpectedInclude> { new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional, "OneToMany_Optional") },
                elementSorter: e => e.Id);
        }

        [ConditionalFact]
        public virtual void SelectMany_with_Include2()
        {
            AssertIncludeQuery<Level1>(
                l1s => l1s
                    .SelectMany(l1 => l1.OneToMany_Optional)
                    .Include(l2 => l2.OneToOne_Required_FK),
                expectedIncludes: new List<IExpectedInclude> { new ExpectedInclude<Level2>(l2 => l2.OneToOne_Required_FK, "OneToOne_Required_FK") },
                elementSorter: e => e.Id);
        }

        [ConditionalFact]
        public virtual void SelectMany_with_Include_ThenInclude()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Required_FK, "OneToOne_Required_FK"),
                new ExpectedInclude<Level3>(l3 => l3.OneToMany_Optional, "OneToMany_Optional", "OneToOne_Required_FK")
            };

            AssertIncludeQuery<Level1>(
                l1s => l1s
                    .SelectMany(l1 => l1.OneToMany_Optional)
                    .Include(l2 => l2.OneToOne_Required_FK)
                    .ThenInclude(l3 => l3.OneToMany_Optional),
                expectedIncludes,
                elementSorter: e => e.Id);
        }

        [ConditionalFact]
        public virtual void Multiple_SelectMany_with_Include()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level3>(l3 => l3.OneToOne_Required_FK, "OneToOne_Required_FK"),
                new ExpectedInclude<Level3>(l3 => l3.OneToMany_Optional, "OneToMany_Optional")
            };

            AssertIncludeQuery<Level1>(
                l1s => l1s
                    .SelectMany(l1 => l1.OneToMany_Optional)
                    .SelectMany(l2 => l2.OneToMany_Optional)
                    .Include(l3 => l3.OneToOne_Required_FK)
                    .Include(l3 => l3.OneToMany_Optional),
                expectedIncludes,
                elementSorter: e => e.Id);
        }

        [ConditionalFact]
        public virtual void SelectMany_with_string_based_Include1()
        {
            AssertIncludeQuery<Level1>(
                l1s => l1s
                    .SelectMany(l1 => l1.OneToMany_Optional)
                    .Include("OneToOne_Required_FK"),
                expectedIncludes: new List<IExpectedInclude> { new ExpectedInclude<Level2>(l2 => l2.OneToOne_Required_FK, "OneToOne_Required_FK") },
                elementSorter: e => e.Id);
        }

        [ConditionalFact]
        public virtual void SelectMany_with_string_based_Include2()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Required_FK, "OneToOne_Required_FK"),
                new ExpectedInclude<Level3>(l3 => l3.OneToOne_Required_FK, "OneToOne_Required_FK", "OneToOne_Required_FK")
            };

            AssertIncludeQuery<Level1>(
                l1s => l1s
                    .SelectMany(l1 => l1.OneToMany_Optional)
                    .Include("OneToOne_Required_FK.OneToOne_Required_FK"),
                expectedIncludes,
                elementSorter: e => e.Id);
        }

        [ConditionalFact]
        public virtual void Multiple_SelectMany_with_string_based_Include()
        {
            AssertIncludeQuery<Level1>(
                l1s => l1s
                    .SelectMany(l1 => l1.OneToMany_Optional)
                    .SelectMany(l1 => l1.OneToMany_Optional)
                    .Include("OneToOne_Required_FK"),
                expectedIncludes: new List<IExpectedInclude> { new ExpectedInclude<Level3>(l3 => l3.OneToOne_Required_FK, "OneToOne_Required_FK") },
                elementSorter: e => e.Id);
        }

        [ConditionalFact]
        public virtual void Required_navigation_with_Include()
        {
            AssertIncludeQuery<Level3>(
                l3s => l3s
                    .Select(l3 => l3.OneToOne_Required_FK_Inverse)
                    .Include(l2 => l2.OneToMany_Required_Inverse),
                expectedIncludes: new List<IExpectedInclude> { new ExpectedInclude<Level2>(l2 => l2.OneToMany_Required_Inverse, "OneToMany_Required_Inverse") },
                elementSorter: e => e.Id);
        }

        [ConditionalFact]
        public virtual void Required_navigation_with_Include_ThenInclude()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level3>(l3 => l3.OneToMany_Required_Inverse, "OneToMany_Required_Inverse"),
                new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional_Inverse, "OneToMany_Optional_Inverse", "OneToMany_Required_Inverse")
            };

            AssertIncludeQuery<Level4>(
                l4s => l4s
                    .Select(l4 => l4.OneToOne_Required_FK_Inverse)
                    .Include(l3 => l3.OneToMany_Required_Inverse)
                    .ThenInclude(l2 => l2.OneToMany_Optional_Inverse),
                expectedIncludes,
                elementSorter: e => e.Id);
        }

        [ConditionalFact]
        public virtual void Multiple_required_navigations_with_Include()
        {
            AssertIncludeQuery<Level4>(
                l4s => l4s
                    .Select(l4 => l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse)
                    .Include(l2 => l2.OneToOne_Optional_FK),
                expectedIncludes: new List<IExpectedInclude> { new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK, "OneToOne_Optional_FK") },
                elementSorter: e => e.Id);
        }

        [ConditionalFact]
        public virtual void Multiple_required_navigation_using_multiple_selects_with_Include()
        {
            AssertIncludeQuery<Level4>(
                l4s => l4s
                    .Select(l4 => l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse)
                    .Include(l2 => l2.OneToOne_Optional_FK),
                expectedIncludes: new List<IExpectedInclude> { new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK, "OneToOne_Optional_FK") },
                elementSorter: e => e.Id);
        }

        [ConditionalFact]
        public virtual void Multiple_required_navigation_with_string_based_Include()
        {
            AssertIncludeQuery<Level4>(
                l4s => l4s
                    .Select(l4 => l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse)
                    .Include("OneToOne_Optional_FK"),
                expectedIncludes: new List<IExpectedInclude> { new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK, "OneToOne_Optional_FK") },
                elementSorter: e => e.Id);
        }

        [ConditionalFact]
        public virtual void Multiple_required_navigation_using_multiple_selects_with_string_based_Include()
        {
            AssertIncludeQuery<Level4>(
                l4s => l4s
                    .Select(l4 => l4.OneToOne_Required_FK_Inverse)
                    .Select(l3 => l3.OneToOne_Required_FK_Inverse)
                    .Include("OneToOne_Optional_FK"),
                expectedIncludes: new List<IExpectedInclude> { new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK, "OneToOne_Optional_FK") },
                elementSorter: e => e.Id);
        }

        [ConditionalFact]
        public virtual void Optional_navigation_with_Include()
        {
            AssertIncludeQuery<Level1>(
                l1s => l1s
                    .Select(l1 => l1.OneToOne_Optional_FK)
                    .Include(l2 => l2.OneToOne_Optional_FK),
                expectedIncludes: new List<IExpectedInclude> { new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK, "OneToOne_Optional_FK") },
                elementSorter: e => e != null ? e.Id : 0);
        }

        [ConditionalFact]
        public virtual void Optional_navigation_with_Include_ThenInclude()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional, "OneToMany_Optional"),
                new ExpectedInclude<Level3>(l3 => l3.OneToOne_Optional_FK, "OneToOne_Optional_FK", "OneToMany_Optional")
            };

            AssertIncludeQuery<Level1>(
                l1s => l1s
                    .Select(l1 => l1.OneToOne_Optional_FK)
                    .Include(l2 => l2.OneToMany_Optional)
                    .ThenInclude(l3 => l3.OneToOne_Optional_FK),
                expectedIncludes,
                elementSorter: e => e != null ? e.Id : 0);
        }

        [ConditionalFact]
        public virtual void Multiple_optional_navigation_with_Include()
        {
            AssertIncludeQuery<Level1>(
                l1s => l1s
                    .Select(l1 => l1.OneToOne_Optional_FK.OneToOne_Optional_PK)
                    .Include(l3 => l3.OneToMany_Optional),
                l1s => l1s
                    .Select(l1 => Maybe(l1.OneToOne_Optional_FK, () => l1.OneToOne_Optional_FK.OneToOne_Optional_PK)),
                expectedIncludes: new List<IExpectedInclude> { new ExpectedInclude<Level3>(l3 => l3.OneToMany_Optional, "OneToMany_Optional") },
                elementSorter: e => e != null ? e.Id : 0);
        }

        [ConditionalFact]
        public virtual void Multiple_optional_navigation_with_string_based_Include()
        {
            AssertIncludeQuery<Level1>(
                l1s => l1s
                    .Select(l1 => l1.OneToOne_Optional_FK)
                    .Select(l2 => l2.OneToOne_Optional_PK)
                    .Include("OneToMany_Optional"),
                l1s => l1s
                    .Select(l1 => l1.OneToOne_Optional_FK)
                    .Select(l2 => Maybe(l2, () => l2.OneToOne_Optional_PK)),
                expectedIncludes: new List<IExpectedInclude> { new ExpectedInclude<Level3>(l3 => l3.OneToMany_Optional, "OneToMany_Optional") },
                elementSorter: e => e != null ? e.Id : 0);
        }

        [ConditionalFact]
        public virtual void SelectMany_with_navigation_and_explicit_DefaultIfEmpty()
        {
            AssertQuery<Level1>(
                l1s =>
                    from l1 in l1s
                    from l2 in l1.OneToMany_Optional.DefaultIfEmpty()
                    where l2 != null
                    select l1,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void SelectMany_with_navigation_and_Distinct()
        {
            AssertIncludeQuery<Level1>(
                l1s => from l1 in l1s.Include(l => l.OneToMany_Optional)
                       from l2 in l1.OneToMany_Optional.Distinct()
                       where l2 != null
                       select l1,
                expectedIncludes: new List<IExpectedInclude> { new ExpectedInclude<Level1>(l1 => l1.OneToMany_Optional, "OneToMany_Optional") },
                elementSorter: e => e.Id);
        }

        [ConditionalFact]
        public virtual void SelectMany_with_navigation_filter_and_explicit_DefaultIfEmpty()
        {
            AssertQuery<Level1>(
                l1s => from l1 in l1s
                       from l2 in l1.OneToMany_Optional.Where(l => l.Id > 5).DefaultIfEmpty()
                       where l2 != null
                       select l1,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void SelectMany_with_nested_navigation_and_explicit_DefaultIfEmpty()
        {
            AssertQuery<Level1>(
                l1s =>
                    from l1 in l1s
                    from l3 in l1.OneToOne_Required_FK.OneToMany_Optional.DefaultIfEmpty()
                    where l3 != null
                    select l1,
                l1s =>
                    from l1 in l1s
                    from l3 in Maybe(
                                   l1.OneToOne_Required_FK,
                                   () => l1.OneToOne_Required_FK.OneToMany_Optional.DefaultIfEmpty()) ?? new List<Level3>()
                    where l3 != null
                    select l1,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void SelectMany_with_nested_navigation_filter_and_explicit_DefaultIfEmpty()
        {
            AssertQuery<Level1>(
                l1s =>
                    from l1 in l1s
                    from l3 in l1.OneToOne_Optional_FK.OneToMany_Optional.Where(l => l.Id > 5).DefaultIfEmpty()
                    where l3 != null
                    select l1,
                l1s =>
                    from l1 in l1s.Where(l => l.OneToOne_Optional_FK != null)
                    from l3 in Maybe(
                        l1.OneToOne_Optional_FK,
                        () => l1.OneToOne_Optional_FK.OneToMany_Optional.Where(l => l.Id > 5).DefaultIfEmpty())
                    where l3 != null
                    select l1,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Multiple_SelectMany_with_navigation_and_explicit_DefaultIfEmpty()
        {
            AssertQuery<Level1>(
                l1s => from l1 in l1s
                       from l2 in l1.OneToMany_Optional
                       from l3 in l2.OneToMany_Optional.Where(l => l.Id > 5).DefaultIfEmpty()
                       where l3 != null
                       select l1,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void SelectMany_with_navigation_filter_paging_and_explicit_DefaultIfEmpty()
        {
            AssertQuery<Level1>(
                l1s => from l1 in l1s
                       from l2 in l1.OneToMany_Required.Where(l => l.Id > 5).OrderBy(l => l.Id).Take(3).DefaultIfEmpty()
                       where l2 != null
                       select l1,
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Select_join_subquery_containing_filter_and_distinct()
        {
            AssertQuery<Level1, Level2>(
                (l1s, l2s) =>
                    from l1 in l1s
                    join l2 in l2s.Where(l => l.Id > 2).Distinct() on l1.Id equals l2.Level1_Optional_Id
                    select new { l1, l2 },
                elementSorter: e => e.l1.Id + " " + e.l2.Id,
                elementAsserter: (e, a) =>
                    {
                        Assert.Equal(e.l1.Id, a.l1.Id);
                        Assert.Equal(e.l2.Id, a.l2.Id);
                    });
        }

        [ConditionalFact]
        public virtual void Select_join_with_key_selector_being_a_subquery()
        {
            AssertQuery<Level1, Level2>(
                (l1s, l2s) => from l1 in l1s
                              join l2 in l2s on l1.Id equals l2s.Select(l => l.Id).OrderBy(l => l).FirstOrDefault()
                              select new { l1, l2 },
                elementSorter: e => e.l1.Id + " " + e.l2.Id,
                elementAsserter: (e, a) => Assert.Equal(e.l1.Name + " " + e.l2.Name, a.l1.Name + " " + a.l2.Name));
        }

        [ConditionalFact]
        public virtual void Contains_with_subquery_optional_navigation_and_constant_item()
        {
            AssertQuery<Level1>(
                l1s => l1s.Where(l1 => l1.OneToOne_Optional_FK.OneToMany_Optional.Distinct().Select(l3 => l3.Id).Contains(1)),
                l1s => l1s.Where(
                    l1 => MaybeScalar<bool>(
                              l1.OneToOne_Optional_FK,
                              () => l1.OneToOne_Optional_FK.OneToMany_Optional.Distinct().Select(l3 => l3.Id).Contains(1)) == true),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Complex_query_with_optional_navigations_and_client_side_evaluation()
        {
            AssertQuery<Level1>(
                l1s => l1s.Where(l1 => l1.Id < 3 && !l1.OneToMany_Optional.Select(l2 => l2.OneToOne_Optional_FK.OneToOne_Optional_FK.Id).All(l4 => ClientMethod(l4))),
                l1s => l1s.Where(
                    l1 => l1.Id < 3 && !l1.OneToMany_Optional.Select(
                              l2 => MaybeScalar(
                                  l2.OneToOne_Optional_FK,
                                  () => MaybeScalar<int>(
                                      l2.OneToOne_Optional_FK.OneToOne_Optional_FK,
                                      () => l2.OneToOne_Optional_FK.OneToOne_Optional_FK.Id))).All(a => true)),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Required_navigation_on_a_subquery_with_First_in_projection()
        {
            AssertQuery<Level2>(
                l2s => l2s
                    .Where(l2o => l2o.Id == 7)
                    .Select(l2o => l2s.OrderBy(l2i => l2i.Id).First().OneToOne_Required_FK_Inverse.Name));
        }

        [ConditionalFact]
        public virtual void Required_navigation_on_a_subquery_with_complex_projection_and_First()
        {
            AssertQuery<Level1, Level2>(
                (l1s, l2s) =>
                    from l2o in l2s
                    where l2o.Id == 7
                    select
                    (from l2i in l2s
                     join l1i in l1s
                         on l2i.Level1_Required_Id equals l1i.Id
                     orderby l2i.Id
                     select new { Navigation = l2i.OneToOne_Required_FK_Inverse, Contant = 7 }).First().Navigation.Name);
        }

        [ConditionalFact]
        public virtual void Required_navigation_on_a_subquery_with_First_in_predicate()
        {
            AssertQuery<Level2>(
                l2s => l2s
                    .Where(l2o => l2o.Id == 7)
                    .Where(l1 => EF.Property<string>(l2s.OrderBy(l2i => l2i.Id).First().OneToOne_Required_FK_Inverse, "Name") == "L1 02"),
                l2s => l2s
                    .Where(l2o => l2o.Id == 7)
                    .Where(l1 => l2s.OrderBy(l2i => l2i.Id).First().OneToOne_Required_FK_Inverse.Name == "L1 02"));
        }

        [ConditionalFact]
        public virtual void Manually_created_left_join_propagates_nullability_to_navigations()
        {
            AssertQuery<Level1, Level2>(
                (l1s, l2s) =>
                    from l1_manual in l1s
                    join l2_manual in l2s on l1_manual.Id equals l2_manual.Level1_Optional_Id into grouping
                    from l2_manual in grouping.DefaultIfEmpty()
                    where l2_manual.OneToOne_Required_FK_Inverse.Name != "L3 02"
                    select l2_manual.OneToOne_Required_FK_Inverse.Name,
                (l1s, l2s) =>
                    from l1_manual in l1s
                    join l2_manual in l2s on l1_manual.Id equals l2_manual.Level1_Optional_Id into grouping
                    from l2_manual in grouping.DefaultIfEmpty()
                    where Maybe(l2_manual, () => l2_manual.OneToOne_Required_FK_Inverse.Name) != "L3 02"
                    select Maybe(l2_manual, () => l2_manual.OneToOne_Required_FK_Inverse.Name));
        }

        [ConditionalFact]
        public virtual void Optional_navigation_propagates_nullability_to_manually_created_left_join1()
        {
            AssertQuery<Level1, Level2>(
                (l1s, l2s) =>
                    from l2_nav in l1s.Select(ll => ll.OneToOne_Optional_FK)
                    join l1 in l2s on l2_nav.Level1_Required_Id equals l1.Id into grouping
                    from l1 in grouping.DefaultIfEmpty()
                    select new { Id1 = (int?)l2_nav.Id, Id2 = (int?)l1.Id },
                (l1s, l2s) =>
                    from l2_nav in l1s.Select(ll => ll.OneToOne_Optional_FK)
                    join l1 in l2s on MaybeScalar<int>(l2_nav, () => l2_nav.Level1_Required_Id) equals l1.Id into grouping
                    from l1 in grouping.DefaultIfEmpty()
                    select new
                    {
                        Id1 = MaybeScalar<int>(l2_nav, () => l2_nav.Id),
                        Id2 = MaybeScalar<int>(l1, () => l1.Id)
                    },
                elementSorter: e => e.Id1);
        }

        [ConditionalFact]
        public virtual void Optional_navigation_propagates_nullability_to_manually_created_left_join2()
        {
            AssertQuery<Level3, Level1>(
                (l3s, l1s) =>
                    from l3 in l3s
                    join l2_nav in l1s.Select(ll => ll.OneToOne_Optional_FK) on l3.Level2_Required_Id equals l2_nav.Id into grouping
                    from l2_nav in grouping.DefaultIfEmpty()
                    select new { Name1 = l3.Name, Name2 = l2_nav.Name },
                (l3s, l1s) =>
                    from l3 in l3s
                    join l2_nav in l1s.Select(ll => ll.OneToOne_Optional_FK) on l3.Level2_Required_Id equals MaybeScalar<int>(l2_nav, () => l2_nav.Id) into grouping
                    from l2_nav in grouping.DefaultIfEmpty()
                    select new { Name1 = l3.Name, Name2 = Maybe(l2_nav, () => l2_nav.Name) },
                elementSorter: e => e.Name1 + e.Name2);
        }

        [ConditionalFact]
        public virtual void Null_reference_protection_complex()
        {
            AssertQuery<Level1, Level2, Level3>(
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

        [ConditionalFact]
        public virtual void Null_reference_protection_complex_materialization()
        {
            AssertQuery<Level1, Level2, Level3>(
                (l1s, l2s, l3s) =>
                    from l3 in l3s
                    join l2_outer in
                        (from l1_inner in l1s
                         join l2_inner in l2s on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping_inner
                         from l2_inner in grouping_inner.DefaultIfEmpty()
                         select l2_inner)
                        on l3.Level2_Required_Id equals l2_outer.Id into grouping_outer
                    from l2_outer in grouping_outer.DefaultIfEmpty()
                    select new { entity = l2_outer, property = l2_outer.Name },
                (l1s, l2s, l3s) =>
                    from l3 in l3s
                    join l2_outer in
                        (from l1_inner in l1s
                         join l2_inner in l2s on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping_inner
                         from l2_inner in grouping_inner.DefaultIfEmpty()
                         select l2_inner)
                        on l3.Level2_Required_Id equals MaybeScalar<int>(l2_outer, () => l2_outer.Id) into grouping_outer
                    from l2_outer in grouping_outer.DefaultIfEmpty()
                    select new { entity = l2_outer, property = Maybe(l2_outer, () => l2_outer.Name) },
                elementSorter: e => e.property,
                elementAsserter: (e, a) =>
                    {
                        Assert.Equal(e.entity?.Id, a.entity?.Id);
                        Assert.Equal(e.property, a.property);
                    });
        }

        private TResult ClientMethodReturnSelf<TResult>(TResult element)
        {
            return element;
        }

        [ConditionalFact]
        public virtual void Null_reference_protection_complex_client_eval()
        {
            AssertQuery<Level1, Level2, Level3>(
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

        [ConditionalFact]
        public virtual void GroupJoin_with_complex_subquery_with_joins_does_not_get_flattened()
        {
            AssertQueryScalar<Level1, Level2>(
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

        [ConditionalFact]
        public virtual void GroupJoin_with_complex_subquery_with_joins_does_not_get_flattened2()
        {
            AssertQueryScalar<Level1, Level2>(
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

        [ConditionalFact]
        public virtual void GroupJoin_with_complex_subquery_with_joins_does_not_get_flattened3()
        {
            AssertQueryScalar<Level1, Level2>(
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

        [ConditionalFact]
        public virtual void GroupJoin_with_complex_subquery_with_joins_with_reference_to_grouping1()
        {
            AssertQueryScalar<Level1, Level2>(
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

        [ConditionalFact]
        public virtual void GroupJoin_with_complex_subquery_with_joins_with_reference_to_grouping2()
        {
            AssertQueryScalar<Level1, Level2>(
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

        [ConditionalFact]
        public virtual void GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_outer()
        {
            AssertQuery<Level1, Level2>(
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

        [ConditionalFact]
        public virtual void GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_outer_with_client_method()
        {
            AssertQuery<Level1, Level2>(
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

        private Level1 ClientLevel1(Level1 arg)
        {
            return arg;
        }

        [ConditionalFact]
        public virtual void GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_inner()
        {
            AssertQuery<Level1, Level2>(
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

        [ConditionalFact]
        public virtual void GroupJoin_on_left_side_being_a_subquery()
        {
            AssertQuery<Level1>(
                l1s => l1s.OrderBy(l1 => l1.OneToOne_Optional_FK.Name)
                    .ThenBy(l1 => l1.Id)
                    .Take(2)
                    .Select(x => new { x.Id, Brand = x.OneToOne_Optional_FK.Name }),
                l1s => l1s.OrderBy(l1 => Maybe(l1.OneToOne_Optional_FK, () => l1.OneToOne_Optional_FK.Name))
                    .ThenBy(l1 => l1.Id)
                    .Take(2)
                    .Select(x => new { x.Id, Brand = Maybe(x.OneToOne_Optional_FK, () => x.OneToOne_Optional_FK.Name) }),
                e => e.Id);
        }

        [ConditionalFact]
        public virtual void GroupJoin_on_right_side_being_a_subquery()
        {
            AssertQuery<Level1, Level2>(
                (l1s, l2s) =>
                    from l2 in l2s
                    join l1 in l1s.OrderBy(x => x.OneToOne_Optional_FK.Name).Take(2) on l2.Level1_Optional_Id equals l1.Id into grouping
                    from l1 in grouping.DefaultIfEmpty()
#pragma warning disable IDE0031 // Use null propagation
                    select new { l2.Id, Name = l1 != null ? l1.Name : null },
#pragma warning restore IDE0031 // Use null propagation
                (l1s, l2s) =>
                    from l2 in l2s
                    join l1 in l1s.OrderBy(x => Maybe(x.OneToOne_Optional_FK, () => x.OneToOne_Optional_FK.Name)).Take(2)
                        on l2.Level1_Optional_Id equals l1.Id into grouping
                    from l1 in grouping.DefaultIfEmpty()
#pragma warning disable IDE0031 // Use null propagation
                    select new { l2.Id, Name = l1 != null ? l1.Name : null },
#pragma warning restore IDE0031 // Use null propagation
                e => e.Id);
        }

        // ReSharper disable once UnusedParameter.Local
        private bool ClientMethod(int? id)
        {
            return true;
        }

        [ConditionalFact]
        public virtual void GroupJoin_in_subquery_with_client_result_operator()
        {
            AssertQuery<Level1, Level2>(
                (l1s, l2s) =>
                    from l1 in l1s
                    where (from l1_inner in l1s
                           join l2_inner in l2s on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping
                           from l2_inner in grouping.DefaultIfEmpty()
                           select l1_inner).Distinct().Count() > 7
                    where l1.Id < 3
                    select l1.Name);
        }

        [ConditionalFact]
        public virtual void GroupJoin_in_subquery_with_client_projection()
        {
            AssertQuery<Level1, Level2>(
                (l1s, l2s) =>
                    from l1 in l1s
                    where (from l1_inner in l1s
                           join l2_inner in l2s on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping
                           from l2_inner in grouping.DefaultIfEmpty()
                           select ClientStringMethod(l1_inner.Name)).Count() > 7
                    where l1.Id < 3
                    select l1.Name);
        }

        [ConditionalFact]
        public virtual void GroupJoin_in_subquery_with_client_projection_nested1()
        {
            AssertQuery<Level1, Level2>
            (
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

        [ConditionalFact]
        public virtual void GroupJoin_in_subquery_with_client_projection_nested2()
        {
            AssertQuery<Level1, Level2>(
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

        [ConditionalFact]
        public virtual void GroupJoin_reference_to_group_in_OrderBy()
        {
            AssertQueryScalar<Level1, Level2>(
                (l1s, l2s) =>
                    from l1 in l1s
                    join l2 in l2s on l1.Id equals l2.Level1_Optional_Id into groupJoin
                    from l2 in groupJoin.DefaultIfEmpty()
                    orderby groupJoin.Count()
                    select l1.Id,
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void GroupJoin_client_method_on_outer()
        {
            AssertQuery<Level1, Level2>(
                (l1s, l2s) =>
                    from l1 in l1s
                    join l2 in l2s on l1.Id equals l2.Level1_Optional_Id into groupJoin
                    from l2 in groupJoin.DefaultIfEmpty()
                    select new { l1.Id, client = ClientMethodNullableInt(l1.Id) },
                elementSorter: e => e.Id,
                elementAsserter: (e, a) => Assert.Equal(e.Id + " " + e.client, a.Id + " " + a.client));
        }

        [ConditionalFact]
        public virtual void GroupJoin_client_method_in_OrderBy()
        {
            AssertQueryScalar<Level1, Level2>(
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

        [ConditionalFact]
        public virtual void GroupJoin_without_DefaultIfEmpty()
        {
            AssertQueryScalar<Level1, Level2>(
                (l1s, l2s) =>
                    from l1 in l1s
                    join l2 in l2s on l1.Id equals l2.Level1_Optional_Id into groupJoin
                    from l2 in groupJoin.Select(gg => gg)
                    select l1.Id);
        }

        [ConditionalFact]
        public virtual void GroupJoin_with_subquery_on_inner()
        {
            AssertQueryScalar<Level1, Level2>(
                (l1s, l2s) =>
                    from l1 in l1s
                    join l2 in l2s on l1.Id equals l2.Level1_Optional_Id into groupJoin
                    from l2 in groupJoin.Where(gg => gg.Id > 0).OrderBy(gg => gg.Id).Take(10).DefaultIfEmpty()
                    select l1.Id);
        }

        [ConditionalFact]
        public virtual void GroupJoin_with_subquery_on_inner_and_no_DefaultIfEmpty()
        {
            AssertQueryScalar<Level1, Level2>(
                (l1s, l2s) =>
                    from l1 in l1s
                    join l2 in l2s on l1.Id equals l2.Level1_Optional_Id into groupJoin
                    from l2 in groupJoin.Where(gg => gg.Id > 0).OrderBy(gg => gg.Id).Take(10)
                    select l1.Id);
        }

        [ConditionalFact]
        public virtual void Optional_navigation_in_subquery_with_unrelated_projection()
        {
            AssertQueryScalar<Level1, Level2>(
                (l1s, l2s) =>
                    l1s.Where(l1 => l1.OneToOne_Optional_FK.Name != "Foo")
                        .OrderBy(l1 => l1.Id)
                        .Take(15)
                        .Select(l1 => l1.Id),
                (l1s, l2s) =>
                    l1s.Where(l1 => Maybe(l1.OneToOne_Optional_FK, () => l1.OneToOne_Optional_FK.Name) != "Foo")
                        .OrderBy(l1 => l1.Id)
                        .Take(15)
                        .Select(l1 => l1.Id));
        }

        [ConditionalFact]
        public virtual void Explicit_GroupJoin_in_subquery_with_unrelated_projection()
        {
            AssertQueryScalar<Level1, Level2>(
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

        [ConditionalFact]
        public virtual void Explicit_GroupJoin_in_subquery_with_unrelated_projection2()
        {
            AssertQueryScalar<Level1, Level2>(
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

        [ConditionalFact]
        public virtual void Explicit_GroupJoin_in_subquery_with_unrelated_projection3()
        {
            AssertQueryScalar<Level1, Level2>(
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

        [ConditionalFact]
        public virtual void Explicit_GroupJoin_in_subquery_with_unrelated_projection4()
        {
            AssertQueryScalar<Level1, Level2>(
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

        [ConditionalFact]
        public virtual void Explicit_GroupJoin_in_subquery_with_scalar_result_operator()
        {
            AssertQuery<Level1, Level2>(
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

        [ConditionalFact]
        public virtual void Explicit_GroupJoin_in_subquery_with_multiple_result_operator_distinct_count_materializes_main_clause()
        {
            AssertQuery<Level1, Level2>(
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

        [ConditionalFact]
        public virtual void Where_on_multilevel_reference_in_subquery_with_outer_projection()
        {
            AssertQuery<Level3>(
                l3s => l3s
                    .Where(l3 => l3.OneToMany_Required_Inverse.OneToOne_Required_FK_Inverse.Name == "L1 03")
                    .OrderBy(l3 => l3.Level2_Required_Id)
                    .Skip(0)
                    .Take(10)
                    .Select(l3 => l3.Name));
        }

        [ConditionalFact]
        public virtual void Join_condition_optimizations_applied_correctly_when_anonymous_type_with_single_property()
        {
            AssertQuery<Level1, Level2>(
                (l1s, l2s) =>
                    from l1 in l1s
                    join l2 in l2s
                        on new
                        {
                            A = EF.Property<int?>(l1, "OneToMany_Optional_Self_InverseId")
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
                            A = MaybeScalar<int>(l1.OneToMany_Optional_Self_Inverse, () => l1.OneToMany_Optional_Self_Inverse.Id)
                        }
                        equals new
                        {
                            A = l2.Level1_Optional_Id
                        }
                    select l1,
                elementSorter: e => e.Id,
                elementAsserter: (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Join_condition_optimizations_applied_correctly_when_anonymous_type_with_multiple_properties()
        {
            AssertQuery<Level1, Level2>(
                (l1s, l2s) =>
                    from l1 in l1s
                    join l2 in l2s
                        on new
                        {
                            A = EF.Property<int?>(l1, "OneToMany_Optional_Self_InverseId"),
                            B = EF.Property<int?>(l1, "OneToOne_Optional_SelfId")
                        }
                        equals new
                        {
                            A = EF.Property<int?>(l2, "Level1_Optional_Id"),
                            B = EF.Property<int?>(l2, "OneToMany_Optional_Self_InverseId")
                        }
                    select l1,
                (l1s, l2s) =>
                    from l1 in l1s
                    join l2 in l2s
                        on new
                        {
                            A = MaybeScalar<int>(l1.OneToMany_Optional_Self_Inverse, () => l1.OneToMany_Optional_Self_Inverse.Id),
                            B = MaybeScalar<int>(l1.OneToOne_Optional_Self, () => l1.OneToOne_Optional_Self.Id)
                        }
                        equals new
                        {
                            A = l2.Level1_Optional_Id,
                            B = MaybeScalar<int>(l2.OneToMany_Optional_Self_Inverse, () => l2.OneToMany_Optional_Self_Inverse.Id)
                        }
                    select l1,
                elementSorter: e => e.Id,
                elementAsserter: (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Navigation_filter_navigation_grouping_ordering_by_group_key()
        {
            var level1Id = 1;
            AssertQuery<Level2>(
                l2s => l2s
                    .Where(l2 => l2.OneToMany_Required_Inverse.Id == level1Id)
                    .GroupBy(l2 => l2.OneToMany_Required_Self_Inverse.Name)
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

        [ConditionalFact]
        public virtual void Nested_group_join_with_take()
        {
            AssertQuery<Level1, Level2>(
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
                    join l2_outer in l2s on MaybeScalar<int>(l1_outer, () => l1_outer.Id) equals l2_outer.Level1_Optional_Id into grouping_outer
                    from l2_outer in grouping_outer.DefaultIfEmpty()
                    select Maybe(l2_outer, () => l2_outer.Name));
        }

        [ConditionalFact]
        public virtual void Navigation_with_same_navigation_compared_to_null()
        {
            AssertQueryScalar<Level2>(
                l2s => from l2 in l2s
                       where l2.OneToMany_Required_Inverse.Name != "L1 07" && l2.OneToMany_Required_Inverse != null
                       select l2.Id);
        }

        [ConditionalFact]
        public virtual void Multi_level_navigation_compared_to_null()
        {
            AssertQueryScalar<Level3>(
                l3s => from l3 in l3s
                       where l3.OneToMany_Optional_Inverse.OneToOne_Required_FK_Inverse != null
                       select l3.Id,
                l3s => from l3 in l3s
                       where Maybe(l3.OneToMany_Optional_Inverse, () => l3.OneToMany_Optional_Inverse.OneToOne_Required_FK_Inverse) != null
                       select l3.Id);
        }

        [ConditionalFact]
        public virtual void Multi_level_navigation_with_same_navigation_compared_to_null()
        {
            AssertQueryScalar<Level3>(
                l3s => from l3 in l3s
                       where l3.OneToMany_Optional_Inverse.OneToOne_Required_FK_Inverse.Name != "L1 07"
                       where l3.OneToMany_Optional_Inverse.OneToOne_Required_FK_Inverse != null
                       select l3.Id,
                l3s => from l3 in l3s
                       where Maybe(
                                 l3.OneToMany_Optional_Inverse,
                                 () => Maybe(
                                     l3.OneToMany_Optional_Inverse.OneToOne_Required_FK_Inverse,
                                     () => l3.OneToMany_Optional_Inverse.OneToOne_Required_FK_Inverse.Name)) != "L1 07"
                       where Maybe(l3.OneToMany_Optional_Inverse, () => l3.OneToMany_Optional_Inverse.OneToOne_Required_FK_Inverse) != null
                       select l3.Id);
        }

        [ConditionalFact]
        public virtual void Navigations_compared_to_each_other1()
        {
            AssertQuery<Level2>(
                l2s => from l2 in l2s
                       where l2.OneToMany_Required_Inverse == l2.OneToMany_Required_Inverse
                       select l2.Name);
        }

        [ConditionalFact]
        public virtual void Navigations_compared_to_each_other2()
        {
            AssertQuery<Level2>(
                l2s => from l2 in l2s
                       where l2.OneToMany_Required_Inverse == l2.OneToOne_Optional_PK_Inverse
                       select l2.Name);
        }

        [ConditionalFact]
        public virtual void Navigations_compared_to_each_other3()
        {
            AssertQuery<Level2>(
                l2s => from l2 in l2s
                       where l2.OneToMany_Optional.Select(i => i.OneToOne_Optional_PK_Inverse == l2).Any()
                       select l2.Name);
        }

        [ConditionalFact]
        public virtual void Navigations_compared_to_each_other4()
        {
            AssertQuery<Level2>(
                l2s => from l2 in l2s
                       where l2.OneToOne_Required_FK.OneToMany_Optional.Select(i => i.OneToOne_Optional_PK_Inverse == l2.OneToOne_Required_FK).Any()
                       select l2.Name,
                l2s => from l2 in l2s
                       where MaybeScalar(
                                 l2.OneToOne_Required_FK,
                                 () => MaybeScalar<bool>(
                                     l2.OneToOne_Required_FK.OneToMany_Optional,
                                     () => l2.OneToOne_Required_FK.OneToMany_Optional.Select(i => i.OneToOne_Optional_PK_Inverse == l2.OneToOne_Required_FK).Any())) == true
                       select l2.Name);
        }

        [ConditionalFact]
        public virtual void Navigations_compared_to_each_other5()
        {
            AssertQuery<Level2>(
                l2s => from l2 in l2s
                       where l2.OneToOne_Required_FK.OneToMany_Optional.Select(i => i.OneToOne_Optional_PK_Inverse == l2.OneToOne_Optional_PK).Any()
                       select l2.Name,
                l2s => from l2 in l2s
                       where MaybeScalar(
                                 l2.OneToOne_Required_FK,
                                 () => MaybeScalar<bool>(
                                     l2.OneToOne_Required_FK.OneToMany_Optional,
                                     () => l2.OneToOne_Required_FK.OneToMany_Optional.Select(i => i.OneToOne_Optional_PK_Inverse == l2.OneToOne_Optional_PK).Any())) == true
                       select l2.Name);
        }

        [ConditionalFact]
        public virtual void Level4_Include()
        {
            AssertIncludeQuery<Level1>(
                l1s => l1s.Select(l1 => l1.OneToOne_Required_PK)
                    .Where(t => t != null)
                    .Select(l2 => l2.OneToOne_Required_PK)
                    .Where(t => t != null)
                    .Select(l3 => l3.OneToOne_Required_PK)
                    .Where(t => t != null)
                    .Select(l4 => l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse)
                    .Include(l2 => l2.OneToOne_Optional_FK),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK, "OneToOne_Optional_FK")
                },
                elementSorter: e => e.Id);
        }

        [ConditionalFact]
        public virtual void Comparing_collection_navigation_on_optional_reference_to_null()
        {
            AssertQueryScalar<Level1>(
                l1s => l1s.Where(l1 => l1.OneToOne_Optional_FK.OneToMany_Optional == null).Select(l1 => l1.Id),
                l1s => l1s.Where(l1 => Maybe(l1.OneToOne_Optional_FK, () => l1.OneToOne_Optional_FK.OneToMany_Optional) == null).Select(l1 => l1.Id));
        }

        [ConditionalFact]
        public virtual void Select_subquery_with_client_eval_and_navigation1()
        {
            AssertQuery<Level2>(
                l2s => l2s.Select(l2 => l2s.OrderBy(l => l.Id).First().OneToOne_Required_FK_Inverse.Name));
        }

        [ConditionalFact]
        public virtual void Select_subquery_with_client_eval_and_navigation2()
        {
            AssertQueryScalar<Level2>(
                l2s => l2s.Select(l2 => l2s.OrderBy(l => l.Id).First().OneToOne_Required_FK_Inverse.Name == "L1 02"));
        }

        [ConditionalFact(Skip = "issue #8526")]
        public virtual void Select_subquery_with_client_eval_and_multi_level_navigation()
        {
            AssertQuery<Level3>(
                l3s => l3s.Select(l3 => l3s.OrderBy(l => l.Id).First().OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.Name));
        }

        [ConditionalFact]
        public virtual void Member_doesnt_get_pushed_down_into_subquery_with_result_operator()
        {
            AssertQuery<Level1, Level3>(
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

        [ConditionalFact]
        public virtual void Project_collection_navigation()
        {
            AssertQuery<Level1>(
                l1s => from l1 in l1s
                       select l1.OneToMany_Optional,
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

        [ConditionalFact]
        public virtual void Project_collection_navigation_nested()
        {
            AssertQuery<Level1>(
                l1s => from l1 in l1s
                       select l1.OneToOne_Optional_FK.OneToMany_Optional,
                l1s => from l1 in l1s
                       select Maybe(l1.OneToOne_Optional_FK, () => l1.OneToOne_Optional_FK.OneToMany_Optional),
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

        [ConditionalFact]
        public virtual void Project_collection_navigation_using_ef_property()
        {
            AssertQuery<Level1>(
                l1s => from l1 in l1s
                       select EF.Property<ICollection<Level3>>(
                           EF.Property<Level2>(
                               l1,
                               "OneToOne_Optional_FK"),
                           "OneToMany_Optional"),
                l1s => from l1 in l1s
                       select Maybe(l1.OneToOne_Optional_FK, () => l1.OneToOne_Optional_FK.OneToMany_Optional),
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

        [ConditionalFact]
        public virtual void Project_collection_navigation_nested_anonymous()
        {
            AssertQuery<Level1>(
                l1s => from l1 in l1s
                       select new { l1.Id, l1.OneToOne_Optional_FK.OneToMany_Optional },
                l1s => from l1 in l1s
                       select new
                       {
                           l1.Id,
                           OneToMany_Optional = Maybe(
                               l1.OneToOne_Optional_FK,
                               () => l1.OneToOne_Optional_FK.OneToMany_Optional)
                       },
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                    {
                        Assert.Equal(e.Id, a.Id);

                        var actualCollection = new List<Level3>();
                        foreach (var actualElement in a.OneToMany_Optional)
                        {
                            actualCollection.Add(actualElement);
                        }

                        Assert.Equal(((IEnumerable<Level3>)e.OneToMany_Optional)?.Count() ?? 0, actualCollection.Count);
                    });
        }

        [ConditionalFact]
        public virtual void Project_collection_navigation_count()
        {
            AssertQuery<Level1>(
                l1s => from l1 in l1s
                       select new { l1.Id, l1.OneToOne_Optional_FK.OneToMany_Optional.Count },
                l1s => from l1 in l1s
                       select new
                       {
                           l1.Id,
                           Count = MaybeScalar(
                                       l1.OneToOne_Optional_FK,
                                       () => MaybeScalar<int>(
                                           l1.OneToOne_Optional_FK.OneToMany_Optional,
                                           () => l1.OneToOne_Optional_FK.OneToMany_Optional.Count)) ?? 0
                       },
                elementSorter: e => e.Id);
        }

        [ConditionalFact]
        public virtual void Project_collection_navigation_composed()
        {
            AssertQuery<Level1>(
                l1s => from l1 in l1s
                       where l1.Id < 3
                       select new { l1.Id, collection = l1.OneToMany_Optional.Where(l2 => l2.Name != "Foo").ToList() },
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

        [ConditionalFact]
        public virtual void Project_collection_and_root_entity()
        {
            AssertQuery<Level1>(
                l1s => from l1 in l1s
                       select new { l1, l1.OneToMany_Optional },
                elementSorter: e => e.l1.Id,
                elementAsserter: (e, a) =>
                    {
                        Assert.Equal(e.l1.Id, a.l1.Id);

                        var actualCollection = new List<Level2>();
                        foreach (var actualElement in a.OneToMany_Optional)
                        {
                            actualCollection.Add(actualElement);
                        }

                        Assert.Equal(((IEnumerable<Level2>)e.OneToMany_Optional).Count(), actualCollection.Count);
                    });
        }

        [ConditionalFact]
        public virtual void Project_collection_and_include()
        {
            AssertQuery<Level1>(
                l1s => from l1 in l1s.Include(l => l.OneToMany_Optional)
                       select new { l1, l1.OneToMany_Optional },
                elementSorter: e => e.l1.Id,
                elementAsserter: (e, a) =>
                    {
                        Assert.Equal(e.l1.Id, a.l1.Id);

                        var actualCollection = new List<Level2>();
                        foreach (var actualElement in a.OneToMany_Optional)
                        {
                            actualCollection.Add(actualElement);
                        }

                        Assert.Equal(((IEnumerable<Level2>)e.OneToMany_Optional).Count(), actualCollection.Count);
                    });
        }

        [ConditionalFact]
        public virtual void Project_navigation_and_collection()
        {
            AssertQuery<Level1>(
                l1s => from l1 in l1s
                       select new { l1.OneToOne_Optional_FK, l1.OneToOne_Optional_FK.OneToMany_Optional },
                l1s => from l1 in l1s
                       select new { l1.OneToOne_Optional_FK, OneToMany_Optional = Maybe(l1.OneToOne_Optional_FK, () => l1.OneToOne_Optional_FK.OneToMany_Optional) },
                elementSorter: e => e.OneToOne_Optional_FK?.Id,
                elementAsserter: (e, a) =>
                    {
                        Assert.Equal(e.OneToOne_Optional_FK?.Id, a.OneToOne_Optional_FK?.Id);

                        var actualCollection = new List<Level3>();
                        foreach (var actualElement in a.OneToMany_Optional)
                        {
                            actualCollection.Add(actualElement);
                        }

                        Assert.Equal(((IEnumerable<Level3>)e.OneToMany_Optional)?.Count() ?? 0, actualCollection.Count);
                    });
        }

        [ConditionalFact(Skip = "issue #8722")]
        public virtual void Include_inside_subquery()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.LevelOne
                    .Where(l1 => l1.Id < 3)
                    .Select(l1 => new { subquery = ctx.LevelTwo.Include(l => l.OneToMany_Optional).Where(l => l.Id > 0) });

                var result = query.ToList();
            }
        }

        [ConditionalFact]
        public virtual void Select_optional_navigation_property_string_concat()
        {
            AssertQuery<Level1>(
                l1s => from l1 in l1s
                       from l2 in l1.OneToMany_Optional.Where(l => l.Id > 5).OrderByDescending(l => l.Name).DefaultIfEmpty()
                       select l1.Name + " " + (l2 != null ? l2.Name : "NULL"));
        }

        [ConditionalFact]
        public virtual void Include_collection_with_multiple_orderbys_member()
        {
            AssertIncludeQuery<Level2>(
                l2s => l2s
                    .Include(l2 => l2.OneToMany_Optional)
                    .OrderBy(l2 => l2.Name)
                    .ThenBy(l2 => l2.Level1_Required_Id),
                new List<IExpectedInclude> { new ExpectedInclude<Level2>(e => e.OneToMany_Optional, "OneToMany_Optional") });
        }

        [ConditionalFact]
        public virtual void Include_collection_with_multiple_orderbys_property()
        {
            AssertIncludeQuery<Level2>(
                l2s => l2s
                    .Include(l2 => l2.OneToMany_Optional)
                    .OrderBy(l2 => EF.Property<int>(l2, "Level1_Required_Id"))
                    .ThenBy(l2 => l2.Name),
                l2s => l2s
                    .OrderBy(l2 => l2.Level1_Required_Id)
                    .ThenBy(l2 => l2.Name),
                new List<IExpectedInclude> { new ExpectedInclude<Level2>(e => e.OneToMany_Optional, "OneToMany_Optional") });
        }

        [ConditionalFact]
        public virtual void Include_collection_with_multiple_orderbys_methodcall()
        {
            AssertIncludeQuery<Level2>(
                l2s => l2s
                    .Include(l2 => l2.OneToMany_Optional)
                    .OrderBy(l2 => Math.Abs(l2.Level1_Required_Id))
                    .ThenBy(l2 => l2.Name),
                new List<IExpectedInclude> { new ExpectedInclude<Level2>(e => e.OneToMany_Optional, "OneToMany_Optional") });
        }

        [ConditionalFact]
        public virtual void Include_collection_with_multiple_orderbys_complex()
        {
            AssertIncludeQuery<Level2>(
                l2s => l2s
                    .Include(l2 => l2.OneToMany_Optional)
                    .OrderBy(l2 => Math.Abs(l2.Level1_Required_Id) + 7)
                    .ThenBy(l2 => l2.Name),
                new List<IExpectedInclude> { new ExpectedInclude<Level2>(e => e.OneToMany_Optional, "OneToMany_Optional") });
        }

        [ConditionalFact]
        public virtual void Include_collection_with_multiple_orderbys_complex_repeated()
        {
            AssertIncludeQuery<Level2>(
                l2s => l2s
                    .Include(l2 => l2.OneToMany_Optional)
                    .OrderBy(l2 => -l2.Level1_Required_Id)
                    .ThenBy(l2 => -l2.Level1_Required_Id).ThenBy(l2 => l2.Name),
                new List<IExpectedInclude> { new ExpectedInclude<Level2>(e => e.OneToMany_Optional, "OneToMany_Optional") });
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
            }
        }

        [ConditionalFact]
        public virtual void Include_reference_with_groupby_in_subquery()
        {
            AssertIncludeQuery<Level1>(
                l1s => l1s
                    .Include(l1 => l1.OneToOne_Optional_FK)
                    .GroupBy(g => g.Name)
                    .Select(g => g.OrderBy(e => e.Id).FirstOrDefault()),
                expectedIncludes: new List<IExpectedInclude> { new ExpectedInclude<Level1>(e => e.OneToOne_Optional_FK, "OneToOne_Optional_FK") });
        }

        [ConditionalFact]
        public virtual void Include_collection_with_groupby_in_subquery()
        {
            AssertIncludeQuery<Level1>(
                l1s => l1s
                    .Include(l1 => l1.OneToMany_Optional)
                    .GroupBy(g => g.Name)
                    .Select(g => g.OrderBy(e => e.Id).FirstOrDefault()),
                expectedIncludes: new List<IExpectedInclude> { new ExpectedInclude<Level1>(e => e.OneToMany_Optional, "OneToMany_Optional") });
        }

        [ConditionalFact]
        public virtual void Multi_include_with_groupby_in_subquery()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(e => e.OneToOne_Optional_FK, "OneToOne_Optional_FK"),
                new ExpectedInclude<Level2>(e => e.OneToMany_Optional, "OneToMany_Optional", "OneToOne_Optional_FK"),
            };

            AssertIncludeQuery<Level1>(
                l1s => l1s
                    .Include(l1 => l1.OneToOne_Optional_FK.OneToMany_Optional)
                    .GroupBy(g => g.Name)
                    .Select(g => g.OrderBy(e => e.Id).FirstOrDefault()),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void Include_collection_with_groupby_in_subquery_and_filter_before_groupby()
        {
            AssertIncludeQuery<Level1>(
                l1s => l1s
                    .Include(l1 => l1.OneToMany_Optional)
                    .Where(l1 => l1.Id > 3)
                    .GroupBy(g => g.Name)
                    .Select(g => g.OrderBy(e => e.Id).FirstOrDefault()),
                expectedIncludes: new List<IExpectedInclude> { new ExpectedInclude<Level1>(e => e.OneToMany_Optional, "OneToMany_Optional") });
        }

        [ConditionalFact]
        public virtual void Include_collection_with_groupby_in_subquery_and_filter_after_groupby()
        {
            AssertIncludeQuery<Level1>(
                l1s => l1s
                    .Include(l1 => l1.OneToMany_Optional)
                    .GroupBy(g => g.Name)
                    .Where(g => g.Key != "Foo")
                    .Select(g => g.OrderBy(e => e.Id).FirstOrDefault()),
                expectedIncludes: new List<IExpectedInclude> { new ExpectedInclude<Level1>(e => e.OneToMany_Optional, "OneToMany_Optional") });
        }

        public virtual void String_include_multiple_derived_navigation_with_same_name_and_same_type()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<InheritanceDerived1>(e => e.ReferenceSameType, "ReferenceSameType"),
                new ExpectedInclude<InheritanceDerived2>(e => e.ReferenceSameType, "ReferenceSameType")
            };

            AssertIncludeQuery<InheritanceBase1>(
                i1s => i1s.Include("ReferenceSameType"),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void String_include_multiple_derived_navigation_with_same_name_and_different_type()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<InheritanceDerived1>(e => e.ReferenceDifferentType, "ReferenceDifferentType"),
                new ExpectedInclude<InheritanceDerived2>(e => e.ReferenceDifferentType, "ReferenceDifferentType")
            };

            AssertIncludeQuery<InheritanceBase1>(
                i1s => i1s.Include("ReferenceDifferentType"),
                expectedIncludes);
       }

        [ConditionalFact]
        public virtual void String_include_multiple_derived_navigation_with_same_name_and_different_type_nested_also_includes_partially_matching_navigation_chains()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<InheritanceDerived1>(e => e.ReferenceDifferentType, "ReferenceDifferentType"),
                new ExpectedInclude<InheritanceDerived2>(e => e.ReferenceDifferentType, "ReferenceDifferentType"),
                new ExpectedInclude<InheritanceLeaf2>(e => e.BaseCollection, "BaseCollection", "ReferenceDifferentType")
            };

            AssertIncludeQuery<InheritanceBase1>(
                i1s => i1s.Include("ReferenceDifferentType.BaseCollection"),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void String_include_multiple_derived_collection_navigation_with_same_name_and_same_type()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<InheritanceDerived1>(e => e.CollectionSameType, "CollectionSameType"),
                new ExpectedInclude<InheritanceDerived2>(e => e.CollectionSameType, "CollectionSameType")
            };

            AssertIncludeQuery<InheritanceBase1>(
                i1s => i1s.Include("CollectionSameType"),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void String_include_multiple_derived_collection_navigation_with_same_name_and_different_type()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<InheritanceDerived1>(e => e.CollectionDifferentType, "CollectionDifferentType"),
                new ExpectedInclude<InheritanceDerived2>(e => e.CollectionDifferentType, "CollectionDifferentType")
            };

            AssertIncludeQuery<InheritanceBase1>(
                i1s => i1s.Include("CollectionDifferentType"),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void String_include_multiple_derived_collection_navigation_with_same_name_and_different_type_nested_also_includes_partially_matching_navigation_chains()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<InheritanceDerived1>(e => e.CollectionDifferentType, "CollectionDifferentType"),
                new ExpectedInclude<InheritanceDerived2>(e => e.CollectionDifferentType, "CollectionDifferentType"),
                new ExpectedInclude<InheritanceLeaf2>(e => e.BaseCollection, "BaseCollection", "CollectionDifferentType")
            };

            AssertIncludeQuery<InheritanceBase1>(
                i1s => i1s.Include("CollectionDifferentType.BaseCollection"),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void String_include_multiple_derived_navigations_complex()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<InheritanceBase2>(e => e.Reference, "Reference"),
                new ExpectedInclude<InheritanceDerived1>(e => e.CollectionDifferentType, "CollectionDifferentType", "Reference"),
                new ExpectedInclude<InheritanceDerived2>(e => e.CollectionDifferentType, "CollectionDifferentType", "Reference"),

                new ExpectedInclude<InheritanceBase2>(e => e.Collection, "Collection"),
                new ExpectedInclude<InheritanceDerived1>(e => e.ReferenceSameType, "ReferenceSameType", "Collection"),
                new ExpectedInclude<InheritanceDerived2>(e => e.ReferenceSameType, "ReferenceSameType", "Collection"),
            };

            AssertIncludeQuery<InheritanceBase2>(
                i2s => i2s.Include("Reference.CollectionDifferentType").Include("Collection.ReferenceSameType"),
                expectedIncludes);
        }

        [ConditionalFact]
        public virtual void Include_reference_collection_order_by_reference_navigation()
        {
            AssertIncludeQuery<Level1>(
                l1s => l1s
                    .Include(l1 => l1.OneToOne_Optional_FK.OneToMany_Optional)
                    .OrderBy(l1 => l1.OneToOne_Optional_FK.Id),
                l1s => l1s
                    .Include(l1 => l1.OneToOne_Optional_FK.OneToMany_Optional)
                    .OrderBy(l1 => MaybeScalar<int>(l1.OneToOne_Optional_FK, () => l1.OneToOne_Optional_FK.Id)),
                expectedIncludes: new List<IExpectedInclude>
                {
                    new ExpectedInclude<Level1>(e => e.OneToOne_Optional_FK, "OneToOne_Optional_FK"),
                    new ExpectedInclude<Level2>(e => e.OneToMany_Optional, "OneToMany_Optional", "OneToOne_Optional_FK")
                });
        }
    }
}
