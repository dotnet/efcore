// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.ComplexNavigationsModel;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Xunit;
// ReSharper disable InconsistentNaming
// ReSharper disable MergeConditionalExpression
// ReSharper disable ReplaceWithSingleCallToSingle
// ReSharper disable ReturnValueOfPureMethodIsNotUsed

// performance in VS editor is really bad if all the methods get converted to expression bodies
// ReSharper disable ConvertToExpressionBodyWhenPossible
namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public abstract class ComplexNavigationsQueryTestBase<TTestStore, TFixture> : IClassFixture<TFixture>, IDisposable
        where TTestStore : TestStore
        where TFixture : ComplexNavigationsQueryFixtureBase<TTestStore>, new()
    {
        protected ComplexNavigationsContext CreateContext() => Fixture.CreateContext(TestStore);

        protected ComplexNavigationsQueryTestBase(TFixture fixture)
        {
            Fixture = fixture;

            TestStore = Fixture.CreateTestStore();
        }

        protected TFixture Fixture { get; }

        protected TTestStore TestStore { get; }

        protected virtual void ClearLog()
        {
        }

        public void Dispose() => TestStore.Dispose();

        public virtual void Entity_equality_empty()
            => AssertQuery<Level1>(
                l1s => l1s.Where(l => l.OneToOne_Optional_FK == new Level2()),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));

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
                  l1s => l1s.Where(l => l.OneToOne_Required_FK == new Level2 { Id = 1 }
                      || l.OneToOne_Required_FK == new Level2 { Id = 2 }),
                  l1s => l1s.Where(l => MaybeScalar<int>(l.OneToOne_Required_FK, () => l.OneToOne_Required_FK.Id) == 1
                      || MaybeScalar<int>(l.OneToOne_Required_FK, () => l.OneToOne_Required_FK.Id) == 2),
                  e => e.Id,
                  (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Key_equality_two_conditions_on_same_navigation2()
        {
            AssertQuery<Level2>(
                  l2s => l2s.Where(l => l.OneToOne_Required_FK_Inverse == new Level1 { Id = 1 }
                      || l.OneToOne_Required_FK_Inverse == new Level1 { Id = 2 }),
                  l2s => l2s.Where(l => l.OneToOne_Required_FK_Inverse.Id == 1
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
            using (var context = CreateContext())
            {
                var expectedCount = context.LevelOne.Count();

                ClearLog();

                var result = context.LevelOne.Include(e => e.OneToMany_Optional).ThenInclude(e => e.OneToMany_Optional).ToList();

                Assert.Equal(expectedCount, result.Count);

                var level1 = result.Single(e => e.Name == "L1 01");

                Assert.Equal(5, level1.OneToMany_Optional.Count);
                Assert.True(level1.OneToMany_Optional.Select(e => e.Name).Contains("L2 02"));
                Assert.True(level1.OneToMany_Optional.Select(e => e.Name).Contains("L2 04"));
                Assert.True(level1.OneToMany_Optional.Select(e => e.Name).Contains("L2 06"));
                Assert.True(level1.OneToMany_Optional.Select(e => e.Name).Contains("L2 08"));
                Assert.True(level1.OneToMany_Optional.Select(e => e.Name).Contains("L2 10"));

                var level2 = level1.OneToMany_Optional.Single(e => e.Name == "L2 02");

                Assert.Equal(2, level2.OneToMany_Optional.Count);
                Assert.True(level2.OneToMany_Optional.Select(e => e.Name).Contains("L3 04"));
                Assert.True(level2.OneToMany_Optional.Select(e => e.Name).Contains("L3 08"));
            }
        }

        [ConditionalFact]
        public virtual void Multi_level_include_correct_PK_is_chosen_as_the_join_predicate_for_queries_that_join_same_table_multiple_times()
        {
            using (var context = CreateContext())
            {
                var expectedCount = context.LevelOne.Count();

                ClearLog();

                var result = context.LevelOne.Include(e => e.OneToMany_Optional).ThenInclude(e => e.OneToMany_Optional).ThenInclude(e => e.OneToMany_Required_Inverse.OneToMany_Optional).ToList();

                Assert.Equal(expectedCount, result.Count);

                var level1 = result.Single(e => e.Name == "L1 01");

                Assert.Equal(5, level1.OneToMany_Optional.Count);
                Assert.True(level1.OneToMany_Optional.Select(e => e.Name).Contains("L2 02"));
                Assert.True(level1.OneToMany_Optional.Select(e => e.Name).Contains("L2 04"));
                Assert.True(level1.OneToMany_Optional.Select(e => e.Name).Contains("L2 06"));
                Assert.True(level1.OneToMany_Optional.Select(e => e.Name).Contains("L2 08"));
                Assert.True(level1.OneToMany_Optional.Select(e => e.Name).Contains("L2 10"));

                var level2 = level1.OneToMany_Optional.Single(e => e.Name == "L2 02");

                Assert.Equal(2, level2.OneToMany_Optional.Count);
                Assert.True(level2.OneToMany_Optional.Select(e => e.Name).Contains("L3 04"));
                Assert.True(level2.OneToMany_Optional.Select(e => e.Name).Contains("L3 08"));

                Assert.True(level2.OneToMany_Optional.Select(e => e.OneToMany_Required_Inverse).All(e => e.Name == "L2 01"));

                var level2Reverse = level2.OneToMany_Optional.Select(e => e.OneToMany_Required_Inverse).First();

                Assert.Equal(3, level2Reverse.OneToMany_Optional.Count);
                Assert.True(level2Reverse.OneToMany_Optional.Select(e => e.Name).Contains("L3 02"));
                Assert.True(level2Reverse.OneToMany_Optional.Select(e => e.Name).Contains("L3 06"));
                Assert.True(level2Reverse.OneToMany_Optional.Select(e => e.Name).Contains("L3 10"));
            }
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
            Dictionary<string, string> fieldLabels;
            Dictionary<string, string> fieldPlaceholders;
            Dictionary<string, List<string>> stringGlobalizations;
            Dictionary<string, string> globalizationLanguages;

            using (var context = CreateContext())
            {
                fieldLabels = context.Fields
                    .Include(f => f.Label)
                    .ToDictionary(f => f.Name, f => f.Label?.DefaultText);

                fieldPlaceholders = context.Fields
                    .Include(f => f.Placeholder)
                    .ToDictionary(f => f.Name, f => f.Placeholder?.DefaultText);

                stringGlobalizations = context.MultilingualStrings
                    .Include(s => s.Globalizations)
                    .ToDictionary(
                        s => s.DefaultText,
                        s => s.Globalizations != null
                            ? s.Globalizations.Select(g => g.Text).ToList()
                            : new List<string>());

                globalizationLanguages = context.Globalizations
                    .Include(g => g.Language)
                    .ToDictionary(g => g.Text, g => g.Language?.Name);
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.Fields
                    .Include(x => x.Label.Globalizations)
                    .ThenInclude(x => x.Language)
                    .Include(x => x.Placeholder.Globalizations)
                    .ThenInclude(x => x.Language);

                var result = query.ToList();

                var expectedFieldCount = 2;
                Assert.Equal(expectedFieldCount, result.Count);
                Assert.True(result.Select(r => r.Name).Contains("Field1"));
                Assert.True(result.Select(r => r.Name).Contains("Field2"));

                for (var i = 0; i < expectedFieldCount; i++)
                {
                    Assert.Equal(fieldLabels[result[i]?.Name], result[i].Label?.DefaultText);
                    Assert.Equal(fieldPlaceholders[result[i]?.Name], result[i].Placeholder?.DefaultText);

                    var label = result[i].Label;
                    if (label != null)
                    {
                        Assert.Equal(stringGlobalizations[label.DefaultText].Count, label.Globalizations.Count);
                        foreach (var globalization in label.Globalizations)
                        {
                            Assert.True(stringGlobalizations[label.DefaultText].Contains(globalization.Text));
                            Assert.Equal(globalizationLanguages[globalization.Text], globalization.Language?.Name);
                        }
                    }

                    var placeholder = result[i].Placeholder;
                    if (placeholder != null)
                    {
                        Assert.Equal(stringGlobalizations[placeholder.DefaultText].Count, placeholder.Globalizations.Count);
                        foreach (var globalization in placeholder.Globalizations)
                        {
                            Assert.True(stringGlobalizations[placeholder.DefaultText].Contains(globalization.Text));
                            Assert.Equal(globalizationLanguages[globalization.Text], globalization.Language?.Name);
                        }
                    }
                }
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
            AssertQueryScalar<Level2, int>(
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
        public virtual void Navigation_key_access_required_comparison()
        {
            AssertQueryScalar<Level2, int>(
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

        // issue #3180
        [ConditionalFact]
        public virtual void Multiple_complex_includes()
        {
            List<Level1> levelOnes;
            List<Level2> levelTwos;
            using (var context = CreateContext())
            {
                levelOnes = context.LevelOne
                    .Include(e => e.OneToOne_Optional_FK)
                    .Include(e => e.OneToMany_Optional).ToList();

                levelTwos = context.LevelTwo
                    .Include(e => e.OneToMany_Optional)
                    .Include(e => e.OneToOne_Optional_FK).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne
                    .Include(e => e.OneToOne_Optional_FK)
                    .ThenInclude(e => e.OneToMany_Optional)
                    .Include(e => e.OneToMany_Optional)
                    .ThenInclude(e => e.OneToOne_Optional_FK);

                var result = query.ToList();

                Assert.Equal(levelOnes.Count, result.Count);
                foreach (var resultItem in result)
                {
                    var expectedLevel1 = levelOnes.Where(e => e.Id == resultItem.Id).Single();
                    Assert.Equal(expectedLevel1.OneToOne_Optional_FK?.Id, resultItem.OneToOne_Optional_FK?.Id);
                    Assert.Equal(expectedLevel1.OneToMany_Optional?.Count, resultItem.OneToMany_Optional?.Count);

                    var oneToOne_Optional_FK = resultItem.OneToOne_Optional_FK;
                    if (oneToOne_Optional_FK != null)
                    {
                        var expectedReferenceLevel2 = levelTwos.Where(e => e.Id == oneToOne_Optional_FK.Id).Single();
                        Assert.Equal(expectedReferenceLevel2.OneToMany_Optional?.Count, oneToOne_Optional_FK.OneToMany_Optional?.Count);
                    }
                }
            }
        }

        // issue #3180
        [ConditionalFact]
        public virtual void Multiple_complex_includes_self_ref()
        {
            List<Level1> levelOnes1;
            List<Level1> levelOnes2;
            using (var context = CreateContext())
            {
                levelOnes1 = context.LevelOne.Include(e => e.OneToOne_Optional_Self).ToList();
                levelOnes2 = context.LevelOne.Include(e => e.OneToMany_Optional_Self).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne
                    .Include(e => e.OneToOne_Optional_Self)
                    .ThenInclude(e => e.OneToMany_Optional_Self)
                    .Include(e => e.OneToMany_Optional_Self)
                    .ThenInclude(e => e.OneToOne_Optional_Self);

                var result = query.ToList();

                foreach (var resultItem in result)
                {
                    var expected1 = levelOnes1.Where(e => e.Id == resultItem.Id).Single();
                    var expected2 = levelOnes2.Where(e => e.Id == resultItem.Id).Single();

                    Assert.Equal(expected1.OneToOne_Optional_Self?.Id, resultItem.OneToOne_Optional_Self?.Id);
                    Assert.Equal(expected2.OneToMany_Optional_Self?.Count, resultItem.OneToMany_Optional_Self?.Count);
                }
            }
        }

        [ConditionalFact]
        public virtual void Multiple_complex_include_select()
        {
            List<Level1> levelOnes;
            List<Level2> levelTwos;
            using (var context = CreateContext())
            {
                levelOnes = context.LevelOne
                    .Include(e => e.OneToOne_Optional_FK)
                    .Include(e => e.OneToMany_Optional).ToList();

                levelTwos = context.LevelTwo
                    .Include(e => e.OneToMany_Optional)
                    .Include(e => e.OneToOne_Optional_FK).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne
                    .Select(e => e)
                    .Include(e => e.OneToOne_Optional_FK)
                    .ThenInclude(e => e.OneToMany_Optional)
                    .Select(e => e)
                    .Include(e => e.OneToMany_Optional)
                    .ThenInclude(e => e.OneToOne_Optional_FK);

                var result = query.ToList();

                Assert.Equal(levelOnes.Count, result.Count);
                foreach (var resultItem in result)
                {
                    var expectedLevel1 = levelOnes.Where(e => e.Id == resultItem.Id).Single();
                    Assert.Equal(expectedLevel1.OneToOne_Optional_FK?.Id, resultItem.OneToOne_Optional_FK?.Id);
                    Assert.Equal(expectedLevel1.OneToMany_Optional?.Count, resultItem.OneToMany_Optional?.Count);

                    var oneToOne_Optional_FK = resultItem.OneToOne_Optional_FK;
                    if (oneToOne_Optional_FK != null)
                    {
                        var expectedReferenceLevel2 = levelTwos.Where(e => e.Id == oneToOne_Optional_FK.Id).Single();
                        Assert.Equal(expectedReferenceLevel2.OneToMany_Optional?.Count, oneToOne_Optional_FK.OneToMany_Optional?.Count);
                    }
                }
            }
        }

        [ConditionalFact]
        public virtual void Select_nav_prop_collection_one_to_many_required()
        {
            List<List<int>> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(e => e.OneToMany_Required)
                    .ToList()
                    .OrderBy(e => e.Id)
                    .Select(e => e.OneToMany_Required?.Select(i => i.Id).ToList()).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne.OrderBy(e => e.Id).Select(e => e.OneToMany_Required.Select(i => i.Id));
                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                for (var i = 0; i < result.Count; i++)
                {
                    Assert.Equal(expected[i].Count, result[i].Count());
                }
            }
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
                      select l2 == null ? null : l2.Name,
                  (l1s, l2s) =>
                      from l1 in l1s
                      join l2 in l2s on l1.Id equals MaybeScalar(l2, () => l2.Level1_Optional_Id) into groupJoin
                      from l2 in groupJoin.DefaultIfEmpty()
                      select l2 == null ? null : l2.Name);
        }

        [ConditionalFact]
        public virtual void Select_nav_prop_reference_optional2()
        {
            AssertQueryNullableScalar<Level1, int>(
                  l1s => l1s.Select(e => (int?)e.OneToOne_Optional_FK.Id),
                  l1s => l1s.Select(e => MaybeScalar<int>(e.OneToOne_Optional_FK, () => e.OneToOne_Optional_FK.Id)));
        }

        [ConditionalFact]
        public virtual void Select_nav_prop_reference_optional2_via_DefaultIfEmpty()
        {
            AssertQueryNullableScalar<Level1, Level2, int>(
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
            AssertQueryScalar<Level1, int>(
                  l1s => l1s
                      .Where(e => e.OneToOne_Optional_FK.Name == "L2 05" || e.OneToOne_Optional_FK.Name == "L2 07")
                      .Select(e => e.Id),
                  l1s => l1s
                      .Where(e => Maybe(e.OneToOne_Optional_FK, () => e.OneToOne_Optional_FK.Name) == "L2 05"
                          || Maybe(e.OneToOne_Optional_FK, () => e.OneToOne_Optional_FK.Name) == "L2 07")
                  .Select(e => e.Id));
        }

        [ConditionalFact]
        public virtual void Where_nav_prop_reference_optional1_via_DefaultIfEmpty()
        {
            AssertQueryScalar<Level1, Level2, int>(
                  (l1s, l2s) =>
                      from l1 in l1s
                      join l2Left in l2s on l1.Id equals l2Left.Level1_Optional_Id into groupJoinLeft
                      from l2Left in groupJoinLeft.DefaultIfEmpty()
                      join l2Right in l2s on l1.Id equals l2Right.Level1_Optional_Id into groupJoinRight
                      from l2Right in groupJoinRight.DefaultIfEmpty()
                      where (l2Left == null ? null : l2Left.Name) == "L2 05" || (l2Right == null ? null : l2Right.Name) == "L2 07"
                      select l1.Id);
        }

        [ConditionalFact]
        public virtual void Where_nav_prop_reference_optional2()
        {
            AssertQueryScalar<Level1, int>(
                  l1s => l1s
                      .Where(e => e.OneToOne_Optional_FK.Name == "L2 05" || e.OneToOne_Optional_FK.Name != "L2 42")
                      .Select(e => e.Id),
                  l1s => l1s
                      .Where(e => Maybe(e.OneToOne_Optional_FK, () => e.OneToOne_Optional_FK.Name) == "L2 05"
                          || Maybe(e.OneToOne_Optional_FK, () => e.OneToOne_Optional_FK.Name) != "L2 42")
                  .Select(e => e.Id));
        }

        [ConditionalFact]
        public virtual void Where_nav_prop_reference_optional2_via_DefaultIfEmpty()
        {
            AssertQueryScalar<Level1, Level2, int>(
                  (l1s, l2s) =>
                      from l1 in l1s
                      join l2Left in l2s on l1.Id equals l2Left.Level1_Optional_Id into groupJoinLeft
                      from l2Left in groupJoinLeft.DefaultIfEmpty()
                      join l2Right in l2s on l1.Id equals l2Right.Level1_Optional_Id into groupJoinRight
                      from l2Right in groupJoinRight.DefaultIfEmpty()
                      where (l2Left == null ? null : l2Left.Name) == "L2 05" || (l2Right == null ? null : l2Right.Name) != "L2 42"
                      select l1.Id);
        }

        [ConditionalFact]
        public virtual void Select_multiple_nav_prop_reference_optional()
        {
            AssertQueryNullableScalar<Level1, int>(
                  l1s => l1s.Select(e => (int?)e.OneToOne_Optional_FK.OneToOne_Optional_FK.Id),
                  l1s => l1s.Select(e => MaybeScalar(
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
                  l1s => l1s.Where(e => Maybe(
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
            AssertQueryNullableScalar<Level1, int>(
                  l1s => l1s.Select(e => (int?)e.OneToOne_Required_FK.OneToOne_Required_FK.Id),
                  l1s => l1s.Select(e => MaybeScalar(
                      e.OneToOne_Required_FK,
                      () => MaybeScalar<int>(
                          e.OneToOne_Required_FK.OneToOne_Required_FK,
                          () => e.OneToOne_Required_FK.OneToOne_Required_FK.Id))));
        }

        [ConditionalFact]
        public virtual void Select_multiple_nav_prop_reference_required2()
        {
            AssertQueryScalar<Level3, int>(
                  l3s => l3s.Select(e => e.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.Id));
        }

        [ConditionalFact]
        public virtual void Select_multiple_nav_prop_optional_required()
        {
            AssertQueryNullableScalar<Level1, int>(
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
            AssertQueryScalar<Level1, int>(
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
            AssertQueryScalar<Level1, int>(
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
            AssertQueryScalar<Level3, int>(
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
            List<KeyValuePair<string, int?>> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(e => e.OneToOne_Required_FK.OneToOne_Required_FK)
                    .Include(e => e.OneToOne_Required_FK.OneToOne_Optional_FK)
                    .ToList()
                    .Where(e =>
                        e.OneToOne_Required_FK?.OneToOne_Required_FK?.Id == e.OneToOne_Required_FK?.OneToOne_Optional_FK?.Id
                        && e.OneToOne_Required_FK?.OneToOne_Optional_FK?.Id != 7)
                    .Select(e => new KeyValuePair<string, int?>
                    (
                        e.Name,
                        e.OneToOne_Required_FK?.OneToOne_Optional_FK?.Id
                    )).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne.Where(e =>
                        e.OneToOne_Required_FK.OneToOne_Required_FK == e.OneToOne_Required_FK.OneToOne_Optional_FK
                        && e.OneToOne_Required_FK.OneToOne_Optional_FK.Id != 7)
                    .Select(e => new
                    {
                        e.Name,
                        Id = (int?)e.OneToOne_Required_FK.OneToOne_Optional_FK.Id
                    });

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                var names = expected.Select(e => e.Key).ToList();
                var ids = expected.Select(e => e.Value).ToList();
                foreach (var resultItem in result)
                {
                    Assert.True(names.Contains(resultItem.Name));
                    Assert.True(ids.Contains(resultItem.Id));
                }
            }
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
            AssertQuery<Level1>(
                  l1s =>
                      l1s.Select(e => new MyOuterDto
                      {
                          Id = e.Id,
                          Name = e.Name,
                          Inner = e.OneToOne_Optional_FK != null ? new MyInnerDto
                          {
                              Id = (int?)e.OneToOne_Optional_FK.Id,
                              Name = e.OneToOne_Optional_FK.Name
                          } : null
                      }),
                  e => e.Id + " " + e.Name + " " + e.Inner,
                  (e, a) =>
                      {
                          Assert.Equal(e.Id, a.Id);
                          Assert.Equal(e.Name, a.Name);
                          if (e.Inner == null)
                          {
                              Assert.Null(a.Inner);
                          }
                          else
                          {
                              Assert.Equal(e.Inner.Id, a.Inner.Id);
                              Assert.Equal(e.Inner.Name, a.Inner.Name);
                          }
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

        [ConditionalFact]
        public virtual void OrderBy_nav_prop_reference_optional()
        {
            AssertQueryScalar<Level1, int>(
                  l1s =>
                      l1s.OrderBy(e => e.OneToOne_Optional_FK.Name).ThenBy(e => e.Id).Select(e => e.Id),
                  l1s =>
                      l1s.OrderBy(e => Maybe(e.OneToOne_Optional_FK, () => e.OneToOne_Optional_FK.Name)).ThenBy(e => e.Id).Select(e => e.Id),
                  verifyOrdered: true);
        }

        [ConditionalFact]
        public virtual void OrderBy_nav_prop_reference_optional_via_DefaultIfEmpty()
        {
            AssertQueryScalar<Level1, Level2, int>(
                  (l1s, l2s) =>
                      from l1 in l1s
                      join l2 in l2s on l1.Id equals l2.Level1_Optional_Id into groupJoin
                      from l2 in groupJoin.DefaultIfEmpty()
                      orderby l2 == null ? null : l2.Name, l1.Id
                      select l1.Id,
                  verifyOrdered: true);
        }

        [ConditionalFact]
        public virtual void Result_operator_nav_prop_reference_optional()
        {
            int expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(e => e.OneToOne_Optional_FK)
                    .ToList()
                    .Sum(e => e.OneToOne_Optional_FK?.Level1_Required_Id ?? 0);
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var result = context.LevelOne.Sum(e => (int?)e.OneToOne_Optional_FK.Level1_Required_Id);

                Assert.Equal(expected, result);
            }
        }

        [ConditionalFact]
        public virtual void Result_operator_nav_prop_reference_optional_via_DefaultIfEmpty()
        {
            int expected;
            using (var context = CreateContext())
            {
                var l1s = context.LevelOne.ToList();
                var l2s = context.LevelTwo.ToList();

                expected = (from l1 in l1s
                            join l2 in l2s on l1.Id equals l2.Level1_Optional_Id into groupJoin
                            from l2 in groupJoin.DefaultIfEmpty()
                            select l2).Sum(e => e == null ? 0 : e.Level1_Required_Id);
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var result = (from l1 in context.LevelOne
                              join l2 in context.LevelTwo on l1.Id equals l2.Level1_Optional_Id into groupJoin
                              from l2 in groupJoin.DefaultIfEmpty()
                              select l2).Sum(e => e == null ? 0 : e.Level1_Required_Id);

                Assert.Equal(expected, result);
            }
        }

        [ConditionalFact]
        public virtual void Include_with_optional_navigation()
        {
            List<Level1> expected;
            using (var context = CreateContext())
            {
                expected = (from l1 in context.LevelOne.Include(e => e.OneToOne_Optional_FK).ToList()
                            where l1.OneToOne_Optional_FK?.Name != "L2 05"
                            select l1).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from l1 in context.LevelOne.Include(e => e.OneToOne_Optional_FK)
                            where l1.OneToOne_Optional_FK.Name != "L2 05"
                            select l1;

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    var expectedElement = expected.Where(e => e.Id == resultItem.Id).Single();

                    Assert.Equal(expectedElement.OneToOne_Optional_FK?.Id, resultItem.OneToOne_Optional_FK?.Id);
                    Assert.Equal(expectedElement.OneToOne_Optional_FK?.Name, resultItem.OneToOne_Optional_FK?.Name);
                }
            }
        }

        [ConditionalFact]
        public virtual void Include_nested_with_optional_navigation()
        {
            List<Level1> expected;
            using (var context = CreateContext())
            {
                expected = (from l1 in context.LevelOne
                                .Include(e => e.OneToOne_Optional_FK.OneToMany_Required)
                                .ThenInclude(e => e.OneToOne_Required_FK).ToList()
                            where l1.OneToOne_Optional_FK?.Name != "L2 09"
                            select l1).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from l1 in context.LevelOne
                                .Include(e => e.OneToOne_Optional_FK.OneToMany_Required)
                                .ThenInclude(e => e.OneToOne_Required_FK)
                            where l1.OneToOne_Optional_FK.Name != "L2 09"
                            select l1;

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    var expectedElement = expected.Where(e => e.Id == resultItem.Id).Single();

                    Assert.Equal(expectedElement.OneToOne_Optional_FK?.Id, resultItem.OneToOne_Optional_FK?.Id);
                    Assert.Equal(expectedElement.OneToOne_Optional_FK?.Name, resultItem.OneToOne_Optional_FK?.Name);

                    var resultCollection = resultItem.OneToOne_Optional_FK?.OneToMany_Required;
                    Assert.Equal(expectedElement.OneToOne_Optional_FK?.OneToMany_Required?.Count, resultCollection?.Count);

                    if (resultCollection != null)
                    {
                        foreach (var inner in resultCollection)
                        {
                            Assert.True(expectedElement.OneToOne_Optional_FK.OneToMany_Required.Select(e => e.Id).Contains(inner.Id));
                        }
                    }
                }
            }
        }

        [ConditionalFact]
        public virtual void Include_with_groupjoin_skip_and_take()
        {
            List<KeyValuePair<Level1, IEnumerable<Level2>>> expected;
            using (var context = CreateContext())
            {
                expected = (from l1 in context.LevelOne
                                .Include(e => e.OneToMany_Optional)
                                .ThenInclude(e => e.OneToOne_Optional_FK)
                                .ToList()
                            join l2 in context.LevelTwo
                                .Include(e => e.OneToOne_Required_PK)
                                .ToList()
                            on (int?)l1.Id equals l2 != null ? l2.Level1_Optional_Id : null into grouping
                            where l1.Name != "L1 03" || l1.Name == null
                            select new KeyValuePair<Level1, IEnumerable<Level2>>(l1, grouping)).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = (from l1 in context.LevelOne
                                 .Include(e => e.OneToMany_Optional)
                                 .ThenInclude(e => e.OneToOne_Optional_FK)
                             join l2 in context.LevelTwo.Include(e => e.OneToOne_Required_PK)
                             on (int?)l1.Id equals l2 != null ? l2.Level1_Optional_Id : null into grouping
                             where l1.Name != "L1 03"
                             select new { l1, grouping }).Skip(1).Take(5);

                var result = query.ToList();

                Assert.Equal(5, result.Count);
                foreach (var resultItem in result)
                {
                    var expectedElement = expected.Where(e => e.Key.Id == resultItem.l1.Id).Single();

                    var expectedOneToManyOptional = expectedElement.Key.OneToMany_Optional?.ToList();
                    var actualOneToManyOptional = resultItem.l1.OneToMany_Optional?.ToList();

                    Assert.Equal(expectedOneToManyOptional?.Count, actualOneToManyOptional?.Count);
                    if (expectedOneToManyOptional != null)
                    {
                        for (var j = 0; j < expectedOneToManyOptional.Count; j++)
                        {
                            Assert.Equal(expectedOneToManyOptional[j].OneToOne_Optional_FK.Id, actualOneToManyOptional[j].OneToOne_Optional_FK.Id);
                        }
                    }

                    var expectedGrouping = expectedElement.Value?.ToList();
                    var actualGrouping = resultItem.grouping?.ToList();
                    Assert.Equal(expectedGrouping?.Count(), resultItem.grouping?.Count());
                    if (expectedGrouping != null)
                    {
                        for (var j = 0; j < expectedGrouping.Count(); j++)
                        {
                            Assert.Equal(expectedGrouping[j].Id, actualGrouping[j].Id);
                            Assert.Equal(expectedGrouping[j].OneToOne_Required_PK.Id, actualGrouping[j].OneToOne_Required_PK.Id);
                        }
                    }
                }
            }
        }

        [ConditionalFact]
        public virtual void Join_flattening_bug_4539()
        {
            using (var context = CreateContext())
            {
                var query = from l1 in context.LevelOne
                            join l1_Optional in context.LevelTwo on (int?)l1.Id equals l1_Optional.Level1_Optional_Id into grouping
                            from l1_Optional in grouping.DefaultIfEmpty()
                            from l2 in context.LevelTwo
                            join l2_Required_Reverse in context.LevelOne on l2.Level1_Required_Id equals l2_Required_Reverse.Id
                            select new { l1_Optional, l2_Required_Reverse };

                var result = query.ToList();
            }
        }

        [ConditionalFact]
        public virtual void Query_source_materialization_bug_4547()
        {
            AssertQueryScalar<Level1, Level2, Level3, int>(
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
                  l1s => l1s.SelectMany(l1 => Maybe(
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
                  l1s => l1s.SelectMany(l1 => Maybe(
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
                  l1s => l1s.SelectMany(l1 => Maybe(
                      l1.OneToMany_Optional,
                      () => l1.OneToMany_Optional.Select(l2 => l2.OneToOne_Optional_FK)) ?? new List<Level3>()),
                  e => e == null ? null : e.Id,
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
                  l1s => l1s.Where(l1 => MaybeScalar(
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
                  l3s => l3s.Where(l1 => MaybeScalar<int>(
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
                  l2s => l2s.Where(l2 => MaybeScalar<int>(
                      l2.OneToMany_Required_Inverse.OneToMany_Optional,
                      () => l2.OneToMany_Required_Inverse.OneToMany_Optional.Count()) > 0),
                  e => e.Id,
                  (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void Complex_multi_include_with_order_by_and_paging()
        {
            List<string> expected;

            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(e => e.OneToOne_Required_FK).ThenInclude(e => e.OneToMany_Optional)
                    .Include(e => e.OneToOne_Required_FK).ThenInclude(e => e.OneToMany_Required)
                    .ToList()
                    .OrderBy(t => t.Name)
                    .Skip(0).Take(10)
                    .Select(e => e.Name)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne
                    .Include(e => e.OneToOne_Required_FK).ThenInclude(e => e.OneToMany_Optional)
                    .Include(e => e.OneToOne_Required_FK).ThenInclude(e => e.OneToMany_Required)
                    .OrderBy(t => t.Name)
                    .Skip(0).Take(10);

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem?.Name));
                }
            }
        }

        [ConditionalFact]
        public virtual void Complex_multi_include_with_order_by_and_paging_joins_on_correct_key()
        {
            List<string> expected;

            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(e => e.OneToOne_Optional_FK).ThenInclude(e => e.OneToMany_Optional)
                    .Include(e => e.OneToOne_Required_FK).ThenInclude(e => e.OneToMany_Required)
                    .ToList()
                    .OrderBy(t => t.Name)
                    .Skip(0).Take(10)
                    .Select(e => e.Name)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne
                    .Include(e => e.OneToOne_Optional_FK).ThenInclude(e => e.OneToMany_Optional)
                    .Include(e => e.OneToOne_Required_FK).ThenInclude(e => e.OneToMany_Required)
                    .OrderBy(t => t.Name)
                    .Skip(0).Take(10);

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem?.Name));
                }
            }
        }

        [ConditionalFact]
        public virtual void Complex_multi_include_with_order_by_and_paging_joins_on_correct_key2()
        {
            List<string> expected;

            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(e => e.OneToOne_Optional_FK.OneToOne_Required_FK).ThenInclude(e => e.OneToMany_Optional)
                    .ToList()
                    .OrderBy(t => t.Name)
                    .Skip(0).Take(10)
                    .Select(e => e.Name)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne
                    .Include(e => e.OneToOne_Optional_FK.OneToOne_Required_FK).ThenInclude(e => e.OneToMany_Optional)
                    .OrderBy(t => t.Name)
                    .Skip(0).Take(10);

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem?.Name));
                }
            }
        }

        [ConditionalFact]
        public virtual void Multiple_include_with_multiple_optional_navigations()
        {
            List<Level1> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(e => e.OneToOne_Required_FK).ThenInclude(e => e.OneToMany_Optional)
                    .Include(e => e.OneToOne_Required_FK).ThenInclude(e => e.OneToOne_Optional_FK)
                    .Include(e => e.OneToOne_Optional_FK).ThenInclude(e => e.OneToOne_Optional_FK)
                    .ToList()
                    .Where(e => e.OneToOne_Required_FK?.OneToOne_Optional_PK?.Name != "Foo")
                    .OrderBy(e => e.Id)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne
                    .Include(e => e.OneToOne_Required_FK).ThenInclude(e => e.OneToMany_Optional)
                    .Include(e => e.OneToOne_Required_FK).ThenInclude(e => e.OneToOne_Optional_FK)
                    .Include(e => e.OneToOne_Optional_FK).ThenInclude(e => e.OneToOne_Optional_FK)
                    .Where(e => e.OneToOne_Required_FK.OneToOne_Optional_PK.Name != "Foo")
                    .OrderBy(e => e.Id);

                var result = query.ToList();
                Assert.Equal(expected.Count, result.Count);
                for (var i = 0; i < result.Count; i++)
                {
                    Assert.True(expected[i].Id == result[i].Id);
                    Assert.True(expected[i].Name == result[i].Name);
                    Assert.True(expected[i].OneToOne_Required_FK?.Id == result[i].OneToOne_Required_FK?.Id);
                    Assert.True(expected[i].OneToOne_Required_FK?.OneToOne_Optional_FK?.Id == result[i].OneToOne_Required_FK?.OneToOne_Optional_FK?.Id);

                    Assert.True(expected[i].OneToOne_Optional_FK?.Id == result[i].OneToOne_Optional_FK?.Id);
                    Assert.True(expected[i].OneToOne_Optional_FK?.OneToOne_Optional_FK?.Id == result[i].OneToOne_Optional_FK?.OneToOne_Optional_FK?.Id);
                }
            }
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
                  e => e != null ? e.Id : null,
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
                  verifyOrdered: true);
        }

        [ConditionalFact]
        public virtual void Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access2()
        {
            AssertQuery<Level3>(
                  l3s => l3s.OrderBy(l3 => l3.OneToOne_Required_FK_Inverse.Id).Select(l3 => EF.Property<Level2>(l3, "OneToOne_Required_FK_Inverse")),
                  l3s => l3s.OrderBy(l3 => l3.OneToOne_Required_FK_Inverse.Id).Select(l3 => l3.OneToOne_Required_FK_Inverse),
                  elementAsserter: (e, a) => Assert.Equal(e.Id, a.Id),
                  verifyOrdered: true);
        }

        [ConditionalFact]
        public virtual void Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access3()
        {
            AssertQuery<Level3>(
                  l3s => l3s.OrderBy(l3 => EF.Property<Level2>(l3, "OneToOne_Required_FK_Inverse").Id).Select(l3 => l3.OneToOne_Required_FK_Inverse),
                  l3s => l3s.OrderBy(l3 => l3.OneToOne_Required_FK_Inverse.Id).Select(l3 => l3.OneToOne_Required_FK_Inverse),
                  elementAsserter: (e, a) => Assert.Equal(e.Id, a.Id),
                  verifyOrdered: true);
        }

        [ConditionalFact]
        public virtual void Order_by_key_of_navigation_similar_to_projected_gets_optimized_into_FK_access()
        {
            AssertQuery<Level3>(
                  l3s => from l3 in l3s
                         orderby l3.OneToOne_Required_FK_Inverse.Id
                         select l3.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse,
                  elementAsserter: (e, a) => Assert.Equal(e.Id, a.Id),
                  verifyOrdered: true);
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
                  verifyOrdered: true);
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
                  verifyOrdered: true);
        }

        [ConditionalFact(Skip = "Test does not pass.")] // TODO: See Issue#6618
        public virtual void Optional_navigation_take_optional_navigation()
        {
            List<string> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne.Include(l1 => l1.OneToOne_Optional_FK).ThenInclude(l2 => l2.OneToOne_Optional_FK)
                    .ToList()
                    .Select(l1 => l1.OneToOne_Optional_FK)
                    .OrderBy(l2 => l2?.Id)
                    .Take(10)
                    .Select(l2 => l2?.OneToOne_Optional_FK?.Name)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne
                    .Select(l1 => l1.OneToOne_Optional_FK)
                    .OrderBy(l2 => l2.Id)
                    .Take(10)
                    .Select(l2 => l2.OneToOne_Optional_FK.Name);

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void Projection_select_correct_table_from_subquery_when_materialization_is_not_required()
        {
            AssertQuery<Level2>(
                  l2s => l2s.Where(l2 => l2.OneToOne_Required_FK_Inverse.Name == "L1 03").Take(3).Select(l2 => l2.Name));
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
                       select l1).Take(3).Select(l1 => l1.Name)
                  );
        }

        [ConditionalFact(Skip = "Test does not pass.")] // TODO: See issue#6782
        public virtual void Where_predicate_on_optional_reference_navigation()
        {
            List<string> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne.Include(l1 => l1.OneToOne_Required_FK).ToList()
                    .Where(l1 => l1.OneToOne_Required_FK?.Name == "L2 03")
                    .Take(3)
                    .Select(l1 => l1.Name)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne
                    .Where(l1 => l1.OneToOne_Required_FK.Name == "L2 03")
                    .Take(3)
                    .Select(l1 => l1.Name);

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void SelectMany_with_Include1()
        {
            List<Level2> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(l1 => l1.OneToMany_Optional)
                    .ThenInclude(l2 => l2.OneToMany_Optional)
                    .ToList()
                    .SelectMany(l1 => l1.OneToMany_Optional)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne
                    .SelectMany(l1 => l1.OneToMany_Optional)
                    .Include(l2 => l2.OneToMany_Optional);

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                for (var i = 0; i < result.Count; i++)
                {
                    var expectedElement = expected.Single(e => e.Name == result[i].Name);
                    var expectedInnerNames = expectedElement.OneToMany_Optional.Select(e => e.Name).ToList();
                    for (var j = 0; j < expectedInnerNames.Count; j++)
                    {
                        Assert.True(result[i].OneToMany_Optional.Select(e => e.Name).Contains(expectedInnerNames[i]));
                    }
                }
            }
        }

        [ConditionalFact]
        public virtual void SelectMany_with_Include2()
        {
            List<Level2> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(l1 => l1.OneToMany_Optional)
                    .ThenInclude(l2 => l2.OneToOne_Required_FK)
                    .ToList()
                    .SelectMany(l1 => l1.OneToMany_Optional)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne
                    .SelectMany(l1 => l1.OneToMany_Optional)
                    .Include(l2 => l2.OneToOne_Required_FK);

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                for (var i = 0; i < result.Count; i++)
                {
                    var expectedElement = expected.Single(e => e.Name == result[i].Name);
                    Assert.Equal(expectedElement.OneToOne_Required_FK?.Name, result[i].OneToOne_Required_FK?.Name);
                }
            }
        }

        [ConditionalFact]
        public virtual void SelectMany_with_Include_ThenInclude()
        {
            List<Level2> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(l1 => l1.OneToMany_Optional)
                    .ThenInclude(l2 => l2.OneToOne_Required_FK)
                    .ThenInclude(l3 => l3.OneToMany_Optional)
                    .ToList()
                    .SelectMany(l1 => l1.OneToMany_Optional)
                    .OrderBy(l2 => l2.Id)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne
                    .SelectMany(l1 => l1.OneToMany_Optional)
                    .Include(l2 => l2.OneToOne_Required_FK)
                    .ThenInclude(l3 => l3.OneToMany_Optional);

                var result = query.ToList().OrderBy(l2 => l2.Id).ToList();

                Assert.Equal(expected.Count, result.Count);
                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i].OneToOne_Required_FK?.Name, result[i].OneToOne_Required_FK?.Name);

                    if (expected[i].OneToOne_Required_FK != null)
                    {
                        var expectedInnerNames = expected[i].OneToOne_Required_FK.OneToMany_Optional?.Select(e => e.Name).ToList();
                        Assert.Equal(expectedInnerNames?.Count, result[i]?.OneToOne_Required_FK?.OneToMany_Optional?.Count);
                        if (expectedInnerNames != null)
                        {
                            var actualInnerNames = result[i]?.OneToOne_Required_FK?.OneToMany_Optional?.Select(e => e.Name).ToList();
                            foreach (string expectedName in expectedInnerNames)
                            {
                                Assert.True(actualInnerNames?.Contains(expectedName));
                            }
                        }
                    }
                }
            }
        }

        [ConditionalFact]
        public virtual void Multiple_SelectMany_with_Include()
        {
            List<Level3> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(l1 => l1.OneToMany_Optional)
                    .ThenInclude(l2 => l2.OneToMany_Optional)
                    .ThenInclude(l3 => l3.OneToOne_Required_FK)
                    .Include(l1 => l1.OneToMany_Optional)
                    .ThenInclude(l2 => l2.OneToMany_Optional)
                    .ThenInclude(l3 => l3.OneToMany_Optional)
                    .ToList()
                    .SelectMany(l1 => l1.OneToMany_Optional)
                    .SelectMany(l2 => l2.OneToMany_Optional)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne
                    .SelectMany(l1 => l1.OneToMany_Optional)
                    .SelectMany(l2 => l2.OneToMany_Optional)
                    .Include(l3 => l3.OneToOne_Required_FK)
                    .Include(l3 => l3.OneToMany_Optional);

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);

                for (var i = 0; i < result.Count; i++)
                {
                    var expectedElement = expected.Single(e => e.Name == result[i].Name);
                    Assert.Equal(expectedElement.OneToOne_Required_FK?.Name, result[i].OneToOne_Required_FK?.Name);

                    var expectedInnerNames = expectedElement.OneToMany_Optional?.Select(e => e.Name).ToList();
                    Assert.Equal(expectedInnerNames?.Count, result[i].OneToMany_Optional?.Count);
                    if (expectedInnerNames != null)
                    {
                        var actualInnerNames = result[i]?.OneToMany_Optional?.Select(e => e.Name).ToList();
                        for (var j = 0; j < expectedInnerNames.Count; j++)
                        {
                            Assert.True(actualInnerNames?.Contains(expectedInnerNames[j]));
                        }
                    }
                }
            }
        }

        [ConditionalFact]
        public virtual void SelectMany_with_string_based_Include1()
        {
            List<Level2> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(l1 => l1.OneToMany_Optional)
                    .ThenInclude(l2 => l2.OneToOne_Required_FK)
                    .ToList()
                    .SelectMany(l1 => l1.OneToMany_Optional)
                    .OrderBy(l2 => l2.Id)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne
                    .SelectMany(l1 => l1.OneToMany_Optional)
                    .Include("OneToOne_Required_FK");

                var result = query.ToList().OrderBy(l1 => l1.Id).ToList();

                Assert.Equal(expected.Count, result.Count);
                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i].Name, result[i].Name);
                    Assert.Equal(expected[i].OneToOne_Required_FK?.Name, result[i].OneToOne_Required_FK?.Name);
                }
            }
        }

        [ConditionalFact]
        public virtual void SelectMany_with_string_based_Include2()
        {
            List<Level2> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(l1 => l1.OneToMany_Optional)
                    .ThenInclude(l2 => l2.OneToOne_Required_FK)
                    .ThenInclude(l3 => l3.OneToOne_Required_FK)
                    .ToList()
                    .SelectMany(l1 => l1.OneToMany_Optional)
                    .OrderBy(l2 => l2.Id)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne
                    .SelectMany(l1 => l1.OneToMany_Optional)
                    .Include("OneToOne_Required_FK.OneToOne_Required_FK");

                var result = query.ToList().OrderBy(l1 => l1.Id).ToList();

                Assert.Equal(expected.Count, result.Count);
                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i].Name, result[i].Name);
                    Assert.Equal(expected[i].OneToOne_Required_FK?.Name, result[i].OneToOne_Required_FK?.Name);
                    Assert.Equal(expected[i].OneToOne_Required_FK?.OneToOne_Required_FK?.Name, result[i].OneToOne_Required_FK?.OneToOne_Required_FK?.Name);
                }
            }
        }

        [ConditionalFact]
        public virtual void Multiple_SelectMany_with_string_based_Include()
        {
            List<Level3> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(l1 => l1.OneToMany_Optional)
                    .ThenInclude(l2 => l2.OneToMany_Optional)
                    .ThenInclude(l3 => l3.OneToOne_Required_FK)
                    .ToList()
                    .SelectMany(l1 => l1.OneToMany_Optional)
                    .SelectMany(l2 => l2.OneToMany_Optional)
                    .OrderBy(l3 => l3.Id)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne
                    .SelectMany(l1 => l1.OneToMany_Optional)
                    .SelectMany(l1 => l1.OneToMany_Optional)
                    .Include("OneToOne_Required_FK");

                var result = query.ToList().OrderBy(l1 => l1.Id).ToList();

                Assert.Equal(expected.Count, result.Count);
                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i].Name, result[i].Name);
                    Assert.Equal(expected[i].OneToOne_Required_FK?.Name, result[i].OneToOne_Required_FK?.Name);
                }
            }
        }

        [ConditionalFact]
        public virtual void Required_navigation_with_Include()
        {
            List<Level2> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelThree
                    .Include(l3 => l3.OneToOne_Required_FK_Inverse.OneToMany_Required_Inverse)
                    .ToList()
                    .Select(l3 => l3.OneToOne_Required_FK_Inverse)
                    .OrderBy(l2 => l2.Id)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelThree.Select(l3 => l3.OneToOne_Required_FK_Inverse).Include(l2 => l2.OneToMany_Required_Inverse);
                var result = query.ToList().OrderBy(l2 => l2.Id).ToList();

                Assert.Equal(expected.Count, result.Count);
                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i].Name, result[i].Name);
                    Assert.Equal(expected[i].OneToMany_Required_Inverse?.Name, result[i].OneToMany_Required_Inverse?.Name);
                }
            }
        }

        [ConditionalFact]
        public virtual void Required_navigation_with_Include_ThenInclude()
        {
            List<Level3> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelFour
                    .Include(l4 => l4.OneToOne_Required_FK_Inverse.OneToMany_Required_Inverse.OneToMany_Optional_Inverse)
                    .ToList()
                    .Select(l4 => l4.OneToOne_Required_FK_Inverse)
                    .OrderBy(l3 => l3.Id)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelFour
                    .Select(l4 => l4.OneToOne_Required_FK_Inverse)
                    .Include(l3 => l3.OneToMany_Required_Inverse)
                    .ThenInclude(l2 => l2.OneToMany_Optional_Inverse);

                var result = query.ToList().OrderBy(l2 => l2.Id).ToList();

                Assert.Equal(expected.Count, result.Count);
                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i].Name, result[i].Name);
                    Assert.Equal(expected[i].OneToMany_Required_Inverse?.Name, result[i].OneToMany_Required_Inverse?.Name);
                    Assert.Equal(expected[i].OneToMany_Required_Inverse?.OneToMany_Optional_Inverse?.Name, result[i].OneToMany_Required_Inverse?.OneToMany_Optional_Inverse?.Name);
                }
            }
        }

        [ConditionalFact]
        public virtual void Multiple_required_navigations_with_Include()
        {
            List<Level2> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelFour
                    .Include(l4 => l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK)
                    .ToList()
                    .Select(l4 => l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse)
                    .OrderBy(l2 => l2.Id)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelFour.Select(l4 => l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse).Include(l2 => l2.OneToOne_Optional_FK);
                var result = query.ToList().OrderBy(l2 => l2.Id).ToList();

                Assert.Equal(expected.Count, result.Count);
                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i].Name, result[i].Name);
                    Assert.Equal(expected[i].OneToOne_Optional_FK?.Name, result[i].OneToOne_Optional_FK?.Name);
                }
            }
        }

        [ConditionalFact]
        public virtual void Multiple_required_navigation_using_multiple_selects_with_Include()
        {
            List<Level2> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelFour
                    .Include(l4 => l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK)
                    .ToList()
                    .Select(l4 => l4.OneToOne_Required_FK_Inverse)
                    .Select(l3 => l3.OneToOne_Required_FK_Inverse)
                    .OrderBy(l2 => l2.Id)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelFour
                    .Select(l4 => l4.OneToOne_Required_FK_Inverse)
                    .Select(l3 => l3.OneToOne_Required_FK_Inverse)
                    .Include(l2 => l2.OneToOne_Optional_FK);

                var result = query.ToList().OrderBy(l2 => l2.Id).ToList();

                Assert.Equal(expected.Count, result.Count);
                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i].Name, result[i].Name);
                    Assert.Equal(expected[i].OneToOne_Optional_FK?.Name, result[i].OneToOne_Optional_FK?.Name);
                }
            }
        }

        [ConditionalFact]
        public virtual void Multiple_required_navigation_with_string_based_Include()
        {
            List<Level2> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelFour
                    .Include(l4 => l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK)
                    .ToList()
                    .Select(l4 => l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse)
                    .OrderBy(l2 => l2.Id)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelFour.Select(l4 => l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse).Include("OneToOne_Optional_FK");
                var result = query.ToList().OrderBy(l2 => l2.Id).ToList();

                Assert.Equal(expected.Count, result.Count);
                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i].Name, result[i].Name);
                    Assert.Equal(expected[i].OneToOne_Optional_FK?.Name, result[i].OneToOne_Optional_FK?.Name);
                }
            }
        }

        [ConditionalFact]
        public virtual void Multiple_required_navigation_using_multiple_selects_with_string_based_Include()
        {
            List<Level2> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelFour
                    .Include(l4 => l4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK)
                    .ToList()
                    .Select(l4 => l4.OneToOne_Required_FK_Inverse)
                    .Select(l3 => l3.OneToOne_Required_FK_Inverse)
                    .OrderBy(l2 => l2.Id)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelFour
                    .Select(l4 => l4.OneToOne_Required_FK_Inverse)
                    .Select(l3 => l3.OneToOne_Required_FK_Inverse)
                    .Include("OneToOne_Optional_FK");

                var result = query.ToList().OrderBy(l2 => l2.Id).ToList();

                Assert.Equal(expected.Count, result.Count);
                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i].Name, result[i].Name);
                    Assert.Equal(expected[i].OneToOne_Optional_FK?.Name, result[i].OneToOne_Optional_FK?.Name);
                }
            }
        }

        [ConditionalFact]
        public virtual void Optional_navigation_with_Include()
        {
            List<Level2> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(l1 => l1.OneToOne_Optional_FK.OneToOne_Optional_FK)
                    .ToList()
                    .Select(l4 => l4.OneToOne_Optional_FK)
                    .OrderBy(l2 => l2?.Id)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne.Select(l1 => l1.OneToOne_Optional_FK).Include(l2 => l2.OneToOne_Optional_FK);
                var result = query.ToList().OrderBy(l2 => l2?.Id).ToList();

                Assert.Equal(expected.Count, result.Count);
                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i]?.Name, result[i]?.Name);
                    Assert.Equal(expected[i]?.OneToOne_Optional_FK?.Name, result[i]?.OneToOne_Optional_FK?.Name);
                }
            }
        }

        [ConditionalFact]
        public virtual void Optional_navigation_with_Include_ThenInclude()
        {
            List<Level2> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(l1 => l1.OneToOne_Optional_FK.OneToMany_Optional).ThenInclude(l3 => l3.OneToOne_Optional_FK)
                    .ToList()
                    .Select(l4 => l4.OneToOne_Optional_FK)
                    .OrderBy(l2 => l2?.Id)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne
                    .Select(l1 => l1.OneToOne_Optional_FK)
                    .Include(l2 => l2.OneToMany_Optional)
                    .ThenInclude(l3 => l3.OneToOne_Optional_FK);

                var result = query.ToList().OrderBy(l2 => l2?.Id).ToList();

                Assert.Equal(expected.Count, result.Count);
                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i]?.Name, result[i]?.Name);

                    var expectedLevelThrees = expected[i]?.OneToMany_Optional?.ToList();
                    Assert.Equal(expectedLevelThrees?.Count, result[i]?.OneToMany_Optional?.Count);
                    if (expectedLevelThrees != null)
                    {
                        foreach (var expectedLevelThree in expectedLevelThrees)
                        {
                            var actualLevelThree = result[i]?.OneToMany_Optional?.Where(e => e.Name == expectedLevelThree.Name).Single();
                            Assert.Equal(expectedLevelThree.OneToOne_Optional_FK?.Name, actualLevelThree?.OneToOne_Optional_FK?.Name);
                        }
                    }
                }
            }
        }

        [ConditionalFact]
        public virtual void Multiple_optional_navigation_with_Include()
        {
            List<Level3> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(l1 => l1.OneToOne_Optional_FK.OneToOne_Optional_PK.OneToMany_Optional)
                    .ToList()
                    .Select(l1 => l1.OneToOne_Optional_FK?.OneToOne_Optional_PK)
                    .OrderBy(l3 => l3?.Id)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne
                    .Select(l1 => l1.OneToOne_Optional_FK.OneToOne_Optional_PK)
                    .Include(l2 => l2.OneToMany_Optional);

                var result = query.ToList().OrderBy(l3 => l3?.Id).ToList();

                Assert.Equal(expected.Count, result.Count);
                for (int i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i]?.Name, result[i]?.Name);
                    var expectedCollection = expected[i]?.OneToMany_Optional?.ToList();
                    var resultCollection = result[i]?.OneToMany_Optional?.ToList();

                    Assert.Equal(expectedCollection?.Count, resultCollection?.Count);
                    if (expectedCollection != null)
                    {
                        for (var j = 0; j < expectedCollection.Count; j++)
                        {
                            Assert.Equal(expectedCollection[j].Name, resultCollection?[j].Name);
                        }
                    }
                }
            }
        }

        [ConditionalFact]
        public virtual void Multiple_optional_navigation_with_string_based_Include()
        {
            List<Level3> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(l1 => l1.OneToOne_Optional_FK.OneToOne_Optional_PK.OneToMany_Optional)
                    .ToList()
                    .Select(l1 => l1.OneToOne_Optional_FK)
                    .Select(l2 => l2?.OneToOne_Optional_PK)
                    .OrderBy(l3 => l3?.Id)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne
                    .Select(l1 => l1.OneToOne_Optional_FK)
                    .Select(l2 => l2.OneToOne_Optional_PK)
                    .Include("OneToMany_Optional");

                var result = query.ToList().OrderBy(l3 => l3?.Id).ToList();

                Assert.Equal(expected.Count, result.Count);
                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i]?.Name, result[i]?.Name);
                    var expectedCollection = expected[i]?.OneToMany_Optional?.ToList();
                    var resultCollection = result[i]?.OneToMany_Optional?.ToList();

                    Assert.Equal(expectedCollection?.Count, resultCollection?.Count);
                    if (expectedCollection != null)
                    {
                        for (var j = 0; j < expectedCollection.Count; j++)
                        {
                            Assert.Equal(expectedCollection[j].Name, resultCollection?[j].Name);
                        }
                    }
                }
            }
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
            List<Level1> expected;
            using (var ctx = CreateContext())
            {
                expected = (from l1 in ctx.LevelOne.Include(l => l.OneToMany_Optional).ToList()
                            from l2 in l1.OneToMany_Optional.Distinct()
                            where l2 != null
                            select l1).ToList().OrderBy(l => l.Id).ToList();
            }

            ClearLog();

            using (var ctx = CreateContext())
            {
                var query = from l1 in ctx.LevelOne.Include(l => l.OneToMany_Optional)
                            from l2 in l1.OneToMany_Optional.Distinct()
                            where l2 != null
                            select l1;

                var result = query.ToList().OrderBy(l => l.Id).ToList();

                Assert.Equal(expected.Count, result.Count);
                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i].Id, result[i].Id);
                }
            }
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
        public virtual void SelectMany_with_navigation_filter_paging_and_explicit_DefautltIfEmpty()
        {
            AssertQuery<Level1>(
                  l1s => from l1 in l1s
                         from l2 in l1.OneToMany_Required.Where(l => l.Id > 5).Take(3).DefaultIfEmpty()
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
                  e => e.l1.Id + " " + e.l2.Id,
                  (e, a) =>
                  {
                      Assert.Equal(e.l1.Id, a.l1.Id);
                      Assert.Equal(e.l2.Id, a.l2.Id);
                  });
        }

        [ConditionalFact]
        public virtual void Select_join_with_key_selector_being_a_subquery()
        {
            List<string> expected;
            using (var ctx = CreateContext())
            {
                expected = (from l1 in ctx.LevelOne.ToList()
                            join l2 in ctx.LevelTwo.ToList() on l1.Id equals ctx.LevelTwo.ToList().Select(l => l.Id).OrderBy(l => l).FirstOrDefault()
                            select new { l1, l2 }).ToList().OrderBy(l => l.l1.Id).ThenBy(l => l.l2.Id).ToList().Select(r => r.l1.Name + " " + r.l2.Name).ToList();
            }

            ClearLog();

            using (var ctx = CreateContext())
            {
                var query = from l1 in ctx.LevelOne
                            join l2 in ctx.LevelTwo on l1.Id equals ctx.LevelTwo.Select(l => l.Id).OrderBy(l => l).FirstOrDefault()
                            select new { l1, l2 };

                var result = query.ToList().OrderBy(l => l.l1.Id).ThenBy(l => l.l2.Id).Select(r => r.l1.Name + " " + r.l2.Name).ToList();

                Assert.Equal(expected.Count, result.Count);
                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i], result[i]);
                }
            }
        }

        [ConditionalFact]
        public virtual void Contains_with_subquery_optional_navigation_and_constant_item()
        {
            AssertQuery<Level1>(
                l1s => l1s.Where(l1 => l1.OneToOne_Optional_FK.OneToMany_Optional.Distinct().Select(l3 => l3.Id).Contains(1)),
                l1s => l1s.Where(l1 => MaybeScalar<bool>(
                    l1.OneToOne_Optional_FK,
                    () => l1.OneToOne_Optional_FK.OneToMany_Optional.Distinct().Select(l3 => l3.Id).Contains(1)) == true),
                e => e.Id,
                (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact(Skip = "Test does not pass.")] // TODO: See issue#6997
        public virtual void Complex_query_with_optional_navigations_and_client_side_evaluation()
        {
            AssertQuery<Level1>(
                l1s => l1s.Where(l1 => !l1.OneToMany_Optional.Select(l2 => l2.OneToOne_Optional_FK.OneToOne_Optional_FK.Id).All(l4 => ClientMethod(l4))),
                l1s => l1s.Where(l1 => l1.OneToMany_Optional.Select(l2 => MaybeScalar(
                    l2.OneToOne_Optional_FK,
                    () => MaybeScalar<int>(
                        l2.OneToOne_Optional_FK.OneToOne_Optional_FK,
                        () => l2.OneToOne_Optional_FK.OneToOne_Optional_FK.Id))).All(a => true)),
                    e => e.Id,
                    (e, a) => Assert.Equal(e.Id, a.Id));
        }

        [ConditionalFact]
        public virtual void GroupJoin_on_left_side_being_a_subquery()
        {
            AssertQuery<Level1>(
                l1s => l1s.OrderBy(l1 => l1.OneToOne_Optional_FK.Name)
                    .Take(2)
                    .Select(x => new { Id = x.Id, Brand = x.OneToOne_Optional_FK.Name }),
                l1s => l1s.OrderBy(l1 => Maybe(l1.OneToOne_Optional_FK, () => l1.OneToOne_Optional_FK.Name))
                    .Take(2)
                    .Select(x => new { Id = x.Id, Brand = Maybe(x.OneToOne_Optional_FK, () => x.OneToOne_Optional_FK.Name) }),
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
                    select new { Id = l2.Id, Nane = l1 != null ? l1.Name : null },
                (l1s, l2s) =>
                    from l2 in l2s
                    join l1 in l1s.OrderBy(x => Maybe(x.OneToOne_Optional_FK, () => x.OneToOne_Optional_FK.Name)).Take(2) 
                        on l2.Level1_Optional_Id equals l1.Id into grouping
                    from l1 in grouping.DefaultIfEmpty()
                    select new { Id = l2.Id, Nane = l1 != null ? l1.Name : null },
                e => e.Id);
        }

        private bool ClientMethod(int? id)
        {
            return true;
        }

        private static TResult Maybe<TResult>(object caller, Func<TResult> expression) where TResult : class
        {
            if (caller == null)
            {
                return null;
            }

            return expression();
        }

        private static TResult? MaybeScalar<TResult>(object caller, Func<TResult?> expression) where TResult : struct
        {
            if (caller == null)
            {
                return null;
            }

            return expression();
        }

        #region AssertQuery

        private void AssertQuery<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<object>> query,
            Func<dynamic, object> elementSorter = null,
            Action<dynamic, dynamic> elementAsserter = null,
            bool verifyOrdered = false)
            where TItem1 : class
            => AssertQuery(query, query, elementSorter, elementAsserter, verifyOrdered);

        private void AssertQuery<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<object>> efQuery,
            Func<IQueryable<TItem1>, IQueryable<object>> l2oQuery,
            Func<dynamic, object> elementSorter = null,
            Action<dynamic, dynamic> elementAsserter = null,
            bool verifyOrdered = false)
            where TItem1 : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    l2oQuery(ComplexNavigationsData.Set<TItem1>()).ToArray(),
                    efQuery(context.Set<TItem1>()).ToArray(),
                    elementSorter ?? (e => e),
                    elementAsserter ?? ((e, a) => Assert.Equal(e, a)),
                    verifyOrdered);
            }
        }

        private void AssertQuery<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> query,
            Func<dynamic, object> elementSorter = null,
            Action<dynamic, dynamic> elementAsserter = null,
            bool verifyOrdered = false)
            where TItem1 : class
            where TItem2 : class
            => AssertQuery(query, query, elementSorter, elementAsserter, verifyOrdered);

        private void AssertQuery<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> efQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> l2oQuery,
            Func<dynamic, object> elementSorter = null,
            Action<dynamic, dynamic> elementAsserter = null,
            bool verifyOrdered = false)
            where TItem1 : class
            where TItem2 : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    l2oQuery(ComplexNavigationsData.Set<TItem1>(), ComplexNavigationsData.Set<TItem2>()).ToArray(),
                    efQuery(context.Set<TItem1>(), context.Set<TItem2>()).ToArray(),
                    elementSorter ?? (e => e),
                    elementAsserter ?? ((e, a) => Assert.Equal(e, a)),
                    verifyOrdered);
            }
        }

        private void AssertQuery<TItem1, TItem2, TItem3>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<object>> query,
            Func<dynamic, object> elementSorter = null,
            Action<dynamic, dynamic> elementAsserter = null,
            bool verifyOrdered = false)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class
            => AssertQuery(query, query, elementSorter, elementAsserter, verifyOrdered);

        private void AssertQuery<TItem1, TItem2, TItem3>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<object>> efQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<object>> l2oQuery,
            Func<dynamic, object> elementSorter = null,
            Action<dynamic, dynamic> elementAsserter = null,
            bool verifyOrdered = false)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    l2oQuery(ComplexNavigationsData.Set<TItem1>(), ComplexNavigationsData.Set<TItem2>(), ComplexNavigationsData.Set<TItem3>()).ToArray(),
                    efQuery(context.Set<TItem1>(), context.Set<TItem2>(), context.Set<TItem3>()).ToArray(),
                    elementSorter ?? (e => e),
                    elementAsserter ?? ((e, a) => Assert.Equal(e, a)),
                    verifyOrdered);
            }
        }

        #endregion

        #region AssertQueryScalar

        private void AssertQueryScalar<TItem1, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TResult>> query,
            bool verifyOrdered = false)
            where TItem1 : class
            where TResult : struct
            => AssertQueryScalar(query, query, verifyOrdered);

        private void AssertQueryScalar<TItem1, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TResult>> efQuery,
            Func<IQueryable<TItem1>, IQueryable<TResult>> l2oQuery,
            bool verifyOrdered = false)
            where TItem1 : class
            where TResult : struct
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    l2oQuery(ComplexNavigationsData.Set<TItem1>()).ToArray(),
                    efQuery(context.Set<TItem1>()).ToArray(),
                    e => e,
                    Assert.Equal,
                    verifyOrdered);
            }
        }

        private void AssertQueryScalar<TItem1, TItem2, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TResult>> query,
            bool verifyOrdered = false)
            where TItem1 : class
            where TItem2 : class
            where TResult : struct
            => AssertQueryScalar(query, query, verifyOrdered);

        private void AssertQueryScalar<TItem1, TItem2, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TResult>> efQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TResult>> l2oQuery,
            bool verifyOrdered = false)
            where TItem1 : class
            where TItem2 : class
            where TResult : struct
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    l2oQuery(ComplexNavigationsData.Set<TItem1>(), ComplexNavigationsData.Set<TItem2>()).ToArray(),
                    efQuery(context.Set<TItem1>(), context.Set<TItem2>()).ToArray(),
                    e => e,
                    Assert.Equal,
                    verifyOrdered);
            }
        }

        private void AssertQueryScalar<TItem1, TItem2, TItem3, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<TResult>> query,
            bool verifyOrdered = false)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class
            where TResult : struct
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    query(ComplexNavigationsData.Set<TItem1>(), ComplexNavigationsData.Set<TItem2>(), ComplexNavigationsData.Set<TItem3>()).ToArray(),
                    query(context.Set<TItem1>(), context.Set<TItem2>(), context.Set<TItem3>()).ToArray(),
                    e => e,
                    Assert.Equal,
                    verifyOrdered);
            }
        }

        private void AssertQueryScalar<TItem1, TItem2, TItem3, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<TResult>> efQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<TResult>> l2oQuery,
            bool verifyOrdered = false)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class
            where TResult : struct
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    l2oQuery(ComplexNavigationsData.Set<TItem1>(), ComplexNavigationsData.Set<TItem2>(), ComplexNavigationsData.Set<TItem3>()).ToArray(),
                    efQuery(context.Set<TItem1>(), context.Set<TItem2>(), context.Set<TItem3>()).ToArray(),
                    e => e,
                    Assert.Equal,
                    verifyOrdered);
            }
        }

        #endregion

        #region AssertQueryNullableScalar

        private void AssertQueryNullableScalar<TItem1, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TResult?>> query,
            bool verifyOrdered = false)
            where TItem1 : class
            where TResult : struct
            => AssertQueryNullableScalar(query, query, verifyOrdered);

        private void AssertQueryNullableScalar<TItem1, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TResult?>> efQuery,
            Func<IQueryable<TItem1>, IQueryable<TResult?>> l2oQuery,
            bool verifyOrdered = false)
            where TItem1 : class
            where TResult : struct
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResultsNullable(
                    l2oQuery(ComplexNavigationsData.Set<TItem1>()).ToArray(),
                    efQuery(context.Set<TItem1>()).ToArray(),
                    e => e,
                    Assert.Equal,
                    verifyOrdered);
            }
        }

        private void AssertQueryNullableScalar<TItem1, TItem2, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TResult?>> query,
            bool verifyOrdered = false)
            where TItem1 : class
            where TItem2 : class
            where TResult : struct
            => AssertQueryNullableScalar(query, query, verifyOrdered);

        private void AssertQueryNullableScalar<TItem1, TItem2, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TResult?>> efQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TResult?>> l2oQuery,
            bool verifyOrdered = false)
            where TItem1 : class
            where TItem2 : class
            where TResult : struct
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResultsNullable(
                    l2oQuery(ComplexNavigationsData.Set<TItem1>(), ComplexNavigationsData.Set<TItem2>()).ToArray(),
                    efQuery(context.Set<TItem1>(), context.Set<TItem2>()).ToArray(),
                    e => e,
                    Assert.Equal,
                    verifyOrdered);
            }
        }

        #endregion
    }
}
