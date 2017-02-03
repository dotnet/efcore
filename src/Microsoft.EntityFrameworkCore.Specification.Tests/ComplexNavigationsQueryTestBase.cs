// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.ComplexNavigationsModel;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Xunit;

// ReSharper disable MergeConditionalExpression
// ReSharper disable ReplaceWithSingleCallToSingle
// ReSharper disable ReturnValueOfPureMethodIsNotUsed
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

        [ConditionalFact]
        public virtual void Entity_equality_empty()
        {
            using (var context = CreateContext())
            {
                var query = context.LevelOne.Where(l => l.OneToOne_Optional_FK == new Level2());
                var result = query.ToList();

                Assert.Equal(0, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Key_equality_when_sentinel_ef_property()
        {
            using (var context = CreateContext())
            {
                var query = context.LevelOne
                    .Where(l => EF.Property<int>(l.OneToOne_Optional_FK, "Id") == 0);

                var result = query.ToList();

                Assert.Equal(0, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Key_equality_using_property_method_required()
        {
            using (var context = CreateContext())
            {
                var query = context.LevelOne
                    .Where(l => EF.Property<int>(l.OneToOne_Required_FK, "Id") > 7);

                var result = query.ToList();

                Assert.Equal(4, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Key_equality_using_property_method_required2()
        {
            using (var context = CreateContext())
            {
                var query = context.LevelTwo
                    .Where(l => EF.Property<int>(l.OneToOne_Required_FK_Inverse, "Id") > 7);

                var result = query.ToList();

                Assert.Equal(4, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Key_equality_using_property_method_nested()
        {
            using (var context = CreateContext())
            {
                var query = context.LevelOne
                    .Where(l => EF.Property<int>(EF.Property<Level2>(l, "OneToOne_Required_FK"), "Id") == 7);

                var result = query.ToList();

                Assert.Equal(1, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Key_equality_using_property_method_nested2()
        {
            using (var context = CreateContext())
            {
                var query = context.LevelTwo
                    .Where(l => EF.Property<int>(EF.Property<Level1>(l, "OneToOne_Required_FK_Inverse"), "Id") == 7);

                var result = query.ToList();

                Assert.Equal(1, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Key_equality_using_property_method_and_member_expression1()
        {
            using (var context = CreateContext())
            {
                var query = context.LevelOne
                    .Where(l => EF.Property<Level2>(l, "OneToOne_Required_FK").Id == 7);

                var result = query.ToList();

                Assert.Equal(1, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Key_equality_using_property_method_and_member_expression2()
        {
            using (var context = CreateContext())
            {
                var query = context.LevelOne
                    .Where(l => EF.Property<int>(l.OneToOne_Required_FK, "Id") == 7);

                var result = query.ToList();

                Assert.Equal(1, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Key_equality_using_property_method_and_member_expression3()
        {
            using (var context = CreateContext())
            {
                var query = context.LevelTwo
                    .Where(l => EF.Property<int>(l.OneToOne_Required_FK_Inverse, "Id") == 7);

                var result = query.ToList();

                Assert.Equal(1, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Key_equality_navigation_converted_to_FK()
        {
            using (var context = CreateContext())
            {
                var query = context.LevelTwo.Where(l => l.OneToOne_Required_FK_Inverse == new Level1 { Id = 1 });
                var result = query.ToList();

                Assert.Equal(1, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Key_equality_two_conditions_on_same_navigation()
        {
            using (var context = CreateContext())
            {
                var query = context.LevelOne.Where(l => l.OneToOne_Required_FK == new Level2 { Id = 1 }
                                                        || l.OneToOne_Required_FK == new Level2 { Id = 2 });

                var result = query.ToList();

                Assert.Equal(2, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Key_equality_two_conditions_on_same_navigation2()
        {
            using (var context = CreateContext())
            {
                var query = context.LevelTwo.Where(l => l.OneToOne_Required_FK_Inverse == new Level1 { Id = 1 }
                                                        || l.OneToOne_Required_FK_Inverse == new Level1 { Id = 2 });

                var result = query.ToList();

                Assert.Equal(2, result.Count);
            }
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
            List<Level1> levelOnes;
            List<Level2> levelTwos;
            using (var context = CreateContext())
            {
                levelOnes = context.LevelOne.ToList();
                levelTwos = context.LevelTwo.Include(e => e.OneToOne_Optional_FK_Inverse).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from e1 in context.LevelOne
                            join e2 in context.LevelTwo on e1.Id equals e2.OneToOne_Optional_FK_Inverse.Id
                            select new { Id1 = e1.Id, Id2 = e2.Id };

                var result = query.ToList();

                var expected = (from l1 in levelOnes
                                join l2 in levelTwos on l1.Id equals l2.OneToOne_Optional_FK_Inverse?.Id
                                select new { Id1 = l1.Id, Id2 = l2.Id }).ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void Join_navigation_key_access_required()
        {
            List<Level1> levelOnes;
            List<Level2> levelTwos;
            using (var context = CreateContext())
            {
                levelOnes = context.LevelOne.ToList();
                levelTwos = context.LevelTwo.Include(e => e.OneToOne_Required_FK_Inverse).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from e1 in context.LevelOne
                            join e2 in context.LevelTwo on e1.Id equals e2.OneToOne_Required_FK_Inverse.Id
                            select new { Id1 = e1.Id, Id2 = e2.Id };

                var result = query.ToList();

                var expected = (from l1 in levelOnes
                                join l2 in levelTwos on l1.Id equals l2.OneToOne_Required_FK_Inverse?.Id
                                select new { Id1 = l1.Id, Id2 = l2.Id }).ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void Navigation_key_access_optional_comparison()
        {
            List<Level2> levelTwos;
            using (var context = CreateContext())
            {
                levelTwos = (from l2 in context.LevelTwo.Include(e => e.OneToOne_Optional_PK_Inverse)
                             select l2).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from e2 in context.LevelTwo
                            where e2.OneToOne_Optional_PK_Inverse.Id > 5
                            select e2.Id;

                var result = query.ToList();

                var expected = (from l2 in levelTwos
                                where l2.OneToOne_Optional_PK_Inverse?.Id > 5
                                select l2.Id).ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void Navigation_key_access_required_comparison()
        {
            List<Level2> levelTwos;
            using (var context = CreateContext())
            {
                levelTwos = (from l2 in context.LevelTwo.Include(e => e.OneToOne_Required_PK_Inverse)
                             select l2).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from e2 in context.LevelTwo
                            where e2.OneToOne_Required_PK_Inverse.Id > 5
                            select e2.Id;

                var result = query.ToList();

                var expected = (from l2 in levelTwos
                                where l2.OneToOne_Required_PK_Inverse?.Id > 5
                                select l2.Id).ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void Navigation_inside_method_call_translated_to_join()
        {
            List<int> expected;
            using (var context = CreateContext())
            {
                expected = (from e1 in context.LevelOne.Include(e => e.OneToOne_Required_FK).ToList()
                            where e1.OneToOne_Required_FK != null && e1.OneToOne_Required_FK.Name.StartsWith("L")
                            select e1.Id).ToList();
            }

            ClearLog();
            using (var context = CreateContext())
            {
                var query = from e1 in context.LevelOne
                            where e1.OneToOne_Required_FK.Name.StartsWith("L")
                            select e1;

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem.Id));
                }
            }
        }

        [ConditionalFact]
        public virtual void Navigation_inside_method_call_translated_to_join2()
        {
            List<int> expected;
            using (var context = CreateContext())
            {
                expected = (from e3 in context.LevelThree.Include(e => e.OneToOne_Required_FK_Inverse).ToList()
                            where e3.OneToOne_Required_FK_Inverse != null && e3.OneToOne_Required_FK_Inverse.Name.StartsWith("L")
                            select e3.Id).ToList();
            }

            ClearLog();
            using (var context = CreateContext())
            {
                var query = from e3 in context.LevelThree
                            where e3.OneToOne_Required_FK_Inverse.Name.StartsWith("L")
                            select e3;

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem.Id));
                }
            }
        }

        [ConditionalFact]
        public virtual void Optional_navigation_inside_method_call_translated_to_join()
        {
            List<int> expected;
            using (var context = CreateContext())
            {
                expected = (from e1 in context.LevelOne.Include(e => e.OneToOne_Optional_FK).ToList()
                            where e1.OneToOne_Optional_FK != null && e1.OneToOne_Optional_FK.Name.StartsWith("L")
                            select e1.Id).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from e1 in context.LevelOne
                            where e1.OneToOne_Optional_FK.Name.StartsWith("L")
                            select e1;

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem.Id));
                }
            }
        }

        [ConditionalFact]
        public virtual void Optional_navigation_inside_property_method_translated_to_join()
        {
            List<int> expected;
            using (var context = CreateContext())
            {
                expected = (from e1 in context.LevelOne.Include(e => e.OneToOne_Optional_FK).ToList()
                            where e1.OneToOne_Optional_FK != null && e1.OneToOne_Optional_FK.Name.ToUpper() == "L2 01"
                            select e1.Id).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from e1 in context.LevelOne
                            where EF.Property<string>(EF.Property<Level2>(e1, "OneToOne_Optional_FK"), "Name") == "L2 01"
                            select e1;

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem.Id));
                }
            }
        }

        [ConditionalFact]
        public virtual void Optional_navigation_inside_nested_method_call_translated_to_join()
        {
            List<int> expected;
            using (var context = CreateContext())
            {
                expected = (from e1 in context.LevelOne.Include(e => e.OneToOne_Optional_FK).ToList()
                            where e1.OneToOne_Optional_FK != null && e1.OneToOne_Optional_FK.Name.ToUpper().StartsWith("L")
                            select e1.Id).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from e1 in context.LevelOne
                            where e1.OneToOne_Optional_FK.Name.ToUpper().StartsWith("L")
                            select e1;

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem.Id));
                }
            }
        }

        [ConditionalFact]
        public virtual void Method_call_on_optional_navigation_translates_to_null_conditional_properly_for_arguments()
        {
            List<int> expected;
            using (var context = CreateContext())
            {
                expected = (from e1 in context.LevelOne.Include(e => e.OneToOne_Optional_FK).ToList()
                            where e1.OneToOne_Optional_FK != null && e1.OneToOne_Optional_FK.Name.StartsWith(e1.OneToOne_Optional_FK.Name)
                            select e1.Id).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from e1 in context.LevelOne
                            where e1.OneToOne_Optional_FK.Name.StartsWith(e1.OneToOne_Optional_FK.Name)
                            select e1;

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem.Id));
                }
            }
        }

        [ConditionalFact]
        public virtual void Optional_navigation_inside_method_call_translated_to_join_keeps_original_nullability()
        {
            List<int> expected;
            using (var context = CreateContext())
            {
                expected = (from e1 in context.LevelOne.Include(e => e.OneToOne_Optional_FK).ToList()
                            where e1.OneToOne_Optional_FK != null && e1.OneToOne_Optional_FK.Date.AddDays(10) > new DateTime(2000, 2, 1)
                            select e1.Id).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from e1 in context.LevelOne
                            where e1.OneToOne_Optional_FK.Date.AddDays(10) > new DateTime(2000, 2, 1)
                            select e1;

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem.Id));
                }
            }
        }

        [ConditionalFact]
        public virtual void Optional_navigation_inside_nested_method_call_translated_to_join_keeps_original_nullability()
        {
            List<int> expected;
            using (var context = CreateContext())
            {
                expected = (from e1 in context.LevelOne.Include(e => e.OneToOne_Optional_FK).ToList()
                            where e1.OneToOne_Optional_FK != null && e1.OneToOne_Optional_FK.Date.AddDays(10).AddDays(15).AddMonths(2) > new DateTime(2000, 2, 1)
                            select e1.Id).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from e1 in context.LevelOne
                            where e1.OneToOne_Optional_FK.Date.AddDays(10).AddDays(15).AddMonths(2) > new DateTime(2002, 2, 1)
                            select e1;

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem.Id));
                }
            }
        }

        [ConditionalFact]
        public virtual void Optional_navigation_inside_nested_method_call_translated_to_join_keeps_original_nullability_also_for_arguments()
        {
            List<int> expected;
            using (var context = CreateContext())
            {
                expected = (from e1 in context.LevelOne.Include(e => e.OneToOne_Optional_FK).ToList()
                            where e1.OneToOne_Optional_FK != null && e1.OneToOne_Optional_FK.Date.AddDays(15).AddDays(e1.OneToOne_Optional_FK.Id) > new DateTime(2002, 2, 1)
                            select e1.Id).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from e1 in context.LevelOne
                            where e1.OneToOne_Optional_FK.Date.AddDays(15).AddDays(e1.OneToOne_Optional_FK.Id) > new DateTime(2002, 2, 1)
                            select e1;

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem.Id));
                }
            }
        }

        [ConditionalFact]
        public virtual void Join_navigation_in_outer_selector_translated_to_extra_join()
        {
            List<Level1> levelOnes;
            List<Level2> levelTwos;
            using (var context = CreateContext())
            {
                levelOnes = context.LevelOne.Include(e => e.OneToOne_Optional_FK).ToList();
                levelTwos = context.LevelTwo.ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from e1 in context.LevelOne
                            join e2 in context.LevelTwo on e1.OneToOne_Optional_FK.Id equals e2.Id
                            select new { Id1 = e1.Id, Id2 = e2.Id };

                var result = query.ToList();

                var expected = (from e1 in levelOnes
                                join e2 in levelTwos on e1.OneToOne_Optional_FK?.Id equals e2.Id
                                select new { Id1 = e1.Id, Id2 = e2.Id }).ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void Join_navigation_in_outer_selector_translated_to_extra_join_nested()
        {
            List<Level1> levelOnes;
            List<Level3> levelThrees;
            using (var context = CreateContext())
            {
                levelOnes = context.LevelOne.Include(e => e.OneToOne_Required_FK.OneToOne_Optional_FK).ToList();
                levelThrees = context.LevelThree.ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from e1 in context.LevelOne
                            join e3 in context.LevelThree on e1.OneToOne_Required_FK.OneToOne_Optional_FK.Id equals e3.Id
                            select new { Id1 = e1.Id, Id3 = e3.Id };

                var result = query.ToList();

                var expected = (from e1 in levelOnes
                                join e3 in levelThrees on e1.OneToOne_Required_FK?.OneToOne_Optional_FK?.Id equals e3.Id
                                select new { Id1 = e1.Id, Id3 = e3.Id }).ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void Join_navigation_in_outer_selector_translated_to_extra_join_nested2()
        {
            List<Level1> levelOnes;
            List<Level3> levelThrees;
            using (var context = CreateContext())
            {
                levelThrees = context.LevelThree.Include(e => e.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK_Inverse).ToList();
                levelOnes = context.LevelOne.ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from e3 in context.LevelThree
                            join e1 in context.LevelOne on e3.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK_Inverse.Id equals e1.Id
                            select new { Id3 = e3.Id, Id1 = e1.Id };

                var result = query.ToList();

                var expected = (from e3 in levelThrees
                                join e1 in levelOnes on e3.OneToOne_Required_FK_Inverse?.OneToOne_Optional_FK_Inverse?.Id equals e1.Id
                                select new { Id3 = e3.Id, Id1 = e1.Id }).ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void Join_navigation_in_inner_selector_translated_to_subquery()
        {
            List<Level1> levelOnes;
            List<Level2> levelTwos;
            using (var context = CreateContext())
            {
                levelOnes = context.LevelOne.Include(e => e.OneToOne_Optional_FK).ToList();
                levelTwos = context.LevelTwo.ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from e2 in context.LevelTwo
                            join e1 in context.LevelOne on e2.Id equals e1.OneToOne_Optional_FK.Id
                            select new { Id2 = e2.Id, Id1 = e1.Id };

                var result = query.ToList();

                var expected = (from e2 in levelTwos
                                join e1 in levelOnes on e2.Id equals e1.OneToOne_Optional_FK?.Id
                                select new { Id2 = e2.Id, Id1 = e1.Id }).ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void Join_navigations_in_inner_selector_translated_to_multiple_subquery_without_collision()
        {
            List<Level1> levelOnes;
            List<Level2> levelTwos;
            List<Level3> levelThrees;
            using (var context = CreateContext())
            {
                levelOnes = context.LevelOne.Include(e => e.OneToOne_Optional_FK).ToList();
                levelTwos = context.LevelTwo.ToList();
                levelThrees = context.LevelThree.Include(e => e.OneToOne_Optional_FK_Inverse).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from e2 in context.LevelTwo
                            join e1 in context.LevelOne on e2.Id equals e1.OneToOne_Optional_FK.Id
                            join e3 in context.LevelThree on e2.Id equals e3.OneToOne_Optional_FK_Inverse.Id
                            select new { Id2 = e2.Id, Id1 = e1.Id, Id3 = e3.Id };

                var result = query.ToList();

                var expected = (from e2 in levelTwos
                                join e1 in levelOnes on e2.Id equals e1.OneToOne_Optional_FK?.Id
                                join e3 in levelThrees on e2.Id equals e3.OneToOne_Optional_FK_Inverse?.Id
                                select new { Id2 = e2.Id, Id1 = e1.Id, Id3 = e3.Id }).ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void Join_navigation_translated_to_subquery_non_key_join()
        {
            List<Level1> levelOnes;
            List<Level2> levelTwos;
            using (var context = CreateContext())
            {
                levelOnes = context.LevelOne.Include(e => e.OneToOne_Optional_FK).ToList();
                levelTwos = context.LevelTwo.ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from e2 in context.LevelTwo
                            join e1 in context.LevelOne on e2.Name equals e1.OneToOne_Optional_FK.Name
                            select new { Id2 = e2.Id, Name2 = e2.Name, Id1 = e1.Id, Name1 = e1.Name };

                var result = query.ToList();

                var expected = (from e2 in levelTwos
                                join e1 in levelOnes on e2.Name equals e1.OneToOne_Optional_FK?.Name
                                select new { Id2 = e2.Id, Name2 = e2.Name, Id1 = e1.Id, Name1 = e1.Name }).ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void Join_navigation_translated_to_subquery_self_ref()
        {
            List<Level1> levelOnes1;
            List<Level1> levelOnes2;
            using (var context = CreateContext())
            {
                levelOnes1 = context.LevelOne.ToList();
                levelOnes2 = context.LevelOne.Include(e => e.OneToMany_Optional_Self_Inverse).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from e1 in context.LevelOne
                            join e2 in context.LevelOne on e1.Id equals e2.OneToMany_Optional_Self_Inverse.Id
                            select new { Id1 = e1.Id, Id2 = e2.Id };

                var result = query.ToList();

                var expected = (from e1 in levelOnes1
                                join e2 in levelOnes2 on e1.Id equals e2.OneToMany_Optional_Self_Inverse?.Id
                                select new { Id1 = e1.Id, Id2 = e2.Id }).ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void Join_navigation_translated_to_subquery_nested()
        {
            List<Level1> levelOnes;
            List<Level3> levelThrees;
            using (var context = CreateContext())
            {
                levelOnes = context.LevelOne.Include(e => e.OneToOne_Required_FK.OneToOne_Optional_FK).ToList();
                levelThrees = context.LevelThree.ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from e3 in context.LevelThree
                            join e1 in context.LevelOne on e3.Id equals e1.OneToOne_Required_FK.OneToOne_Optional_FK.Id
                            select new { Id3 = e3.Id, Id1 = e1.Id };

                var result = query.ToList();

                var expected = (from e3 in levelThrees
                                join e1 in levelOnes on e3.Id equals e1?.OneToOne_Required_FK?.OneToOne_Optional_FK?.Id
                                select new { Id3 = e3.Id, Id1 = e1.Id }).ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void Join_navigation_translated_to_subquery_deeply_nested_non_key_join()
        {
            List<Level1> levelOnes;
            List<Level4> levelFours;
            using (var context = CreateContext())
            {
                levelOnes = context.LevelOne.Include(e => e.OneToOne_Required_FK.OneToOne_Optional_FK.OneToOne_Required_PK).ToList();
                levelFours = context.LevelFour.ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from e4 in context.LevelFour
                            join e1 in context.LevelOne on e4.Name equals e1.OneToOne_Required_FK.OneToOne_Optional_FK.OneToOne_Required_PK.Name
                            select new { Id4 = e4.Id, Name4 = e4.Name, Id1 = e1.Id, Name1 = e1.Name };

                var result = query.ToList();

                var expected = (from e4 in levelFours
                                join e1 in levelOnes on e4.Name equals e1?.OneToOne_Required_FK?.OneToOne_Optional_FK?.OneToOne_Required_PK?.Name
                                select new { Id4 = e4.Id, Name4 = e4.Name, Id1 = e1.Id, Name1 = e1.Name }).ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void Join_navigation_translated_to_subquery_deeply_nested_required()
        {
            List<Level1> levelOnes;
            List<Level4> levelFours;
            using (var context = CreateContext())
            {
                levelFours = context.LevelFour.Include(e => e.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Required_PK_Inverse).ToList();
                levelOnes = context.LevelOne.ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from e1 in context.LevelOne
                            join e4 in context.LevelFour on e1.Name equals e4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Required_PK_Inverse.Name
                            select new { Id4 = e4.Id, Name4 = e4.Name, Id1 = e1.Id, Name1 = e1.Name };

                var result = query.ToList();

                var expected = (from e1 in levelOnes
                                join e4 in levelFours on e1.Name equals e4.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Required_PK_Inverse.Name
                                select new { Id4 = e4.Id, Name4 = e4.Name, Id1 = e1.Id, Name1 = e1.Name }).ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
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
            List<string> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(e => e.OneToOne_Optional_FK)
                    .ToList()
                    .Select(e => e.OneToOne_Optional_FK?.Name).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne.Select(e => e.OneToOne_Optional_FK.Name);
                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void Select_nav_prop_reference_optional1_via_DefaultIfEmpty()
        {
            List<string> expected;
            using (var context = CreateContext())
            {
                var l1s = context.LevelOne.ToList();
                var l2s = context.LevelTwo.ToList();

                expected = (from l1 in l1s
                            join l2 in l2s on l1.Id equals l2.Level1_Optional_Id into groupJoin
                            from l2 in groupJoin.DefaultIfEmpty()
                            select l2 == null ? null : l2.Name).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from l1 in context.LevelOne
                            join l2 in context.LevelTwo on l1.Id equals l2.Level1_Optional_Id into groupJoin
                            from l2 in groupJoin.DefaultIfEmpty()
                            select l2 == null ? null : l2.Name;

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void Select_nav_prop_reference_optional2()
        {
            List<int?> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(e => e.OneToOne_Optional_FK)
                    .ToList()
                    .Select(e => e.OneToOne_Optional_FK?.Id).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne.Select(e => (int?)e.OneToOne_Optional_FK.Id);
                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void Select_nav_prop_reference_optional2_via_DefaultIfEmpty()
        {
            List<int?> expected;
            using (var context = CreateContext())
            {
                var l1s = context.LevelOne.ToList();
                var l2s = context.LevelTwo.ToList();

                expected = (from l1 in l1s
                            join l2 in l2s on l1.Id equals l2.Level1_Optional_Id into groupJoin
                            from l2 in groupJoin.DefaultIfEmpty()
                            select l2 == null ? null : (int?)l2.Id).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from l1 in context.LevelOne
                            join l2 in context.LevelTwo on l1.Id equals l2.Level1_Optional_Id into groupJoin
                            from l2 in groupJoin.DefaultIfEmpty()
                            select l2 == null ? null : (int?)l2.Id;

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void Select_nav_prop_reference_optional3()
        {
            List<string> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelTwo
                    .Include(e => e.OneToOne_Optional_FK_Inverse)
                    .ToList()
                    .Select(e => e.OneToOne_Optional_FK_Inverse?.Name).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelTwo.Select(e => e.OneToOne_Optional_FK_Inverse.Name);
                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void Where_nav_prop_reference_optional1()
        {
            List<int> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(e => e.OneToOne_Optional_FK)
                    .ToList()
                    .Where(e => e.OneToOne_Optional_FK?.Name == "L2 05" || e.OneToOne_Optional_FK?.Name == "L2 07")
                    .Select(e => e.Id).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne
                    .Where(e => e.OneToOne_Optional_FK.Name == "L2 05" || e.OneToOne_Optional_FK.Name == "L2 07")
                    .Select(e => e.Id);

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void Where_nav_prop_reference_optional1_via_DefaultIfEmpty()
        {
            List<int> expected;
            using (var context = CreateContext())
            {
                var l1s = context.LevelOne.ToList();
                var l2s = context.LevelTwo.ToList();

                expected = (from l1 in l1s
                            join l2Left in l2s on l1.Id equals l2Left.Level1_Optional_Id into groupJoinLeft
                            from l2Left in groupJoinLeft.DefaultIfEmpty()
                            join l2Right in l2s on l1.Id equals l2Right.Level1_Optional_Id into groupJoinRight
                            from l2Right in groupJoinRight.DefaultIfEmpty()
                            where (l2Left == null ? null : l2Left.Name) == "L2 05" || (l2Right == null ? null : l2Right.Name) == "L2 07"
                            select l1.Id).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from l1 in context.LevelOne
                            join l2Left in context.LevelTwo on l1.Id equals l2Left.Level1_Optional_Id into groupJoinLeft
                            from l2Left in groupJoinLeft.DefaultIfEmpty()
                            join l2Right in context.LevelTwo on l1.Id equals l2Right.Level1_Optional_Id into groupJoinRight
                            from l2Right in groupJoinRight.DefaultIfEmpty()
                            where (l2Left == null ? null : l2Left.Name) == "L2 05" || (l2Right == null ? null : l2Right.Name) == "L2 07"
                            select l1.Id;

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void Where_nav_prop_reference_optional2()
        {
            List<int> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(e => e.OneToOne_Optional_FK)
                    .ToList()
                    .Where(e => e.OneToOne_Optional_FK?.Name == "L2 05" || e.OneToOne_Optional_FK?.Name != "L2 42")
                    .Select(e => e.Id).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne
                    .Where(e => e.OneToOne_Optional_FK.Name == "L2 05" || e.OneToOne_Optional_FK.Name != "L2 42")
                    .Select(e => e.Id);

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void Where_nav_prop_reference_optional2_via_DefaultIfEmpty()
        {
            List<int> expected;
            using (var context = CreateContext())
            {
                var l1s = context.LevelOne.ToList();
                var l2s = context.LevelTwo.ToList();

                expected = (from l1 in l1s
                            join l2Left in l2s on l1.Id equals l2Left.Level1_Optional_Id into groupJoinLeft
                            from l2Left in groupJoinLeft.DefaultIfEmpty()
                            join l2Right in l2s on l1.Id equals l2Right.Level1_Optional_Id into groupJoinRight
                            from l2Right in groupJoinRight.DefaultIfEmpty()
                            where (l2Left == null ? null : l2Left.Name) == "L2 05" || (l2Right == null ? null : l2Right.Name) != "L2 42"
                            select l1.Id).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from l1 in context.LevelOne
                            join l2Left in context.LevelTwo on l1.Id equals l2Left.Level1_Optional_Id into groupJoinLeft
                            from l2Left in groupJoinLeft.DefaultIfEmpty()
                            join l2Right in context.LevelTwo on l1.Id equals l2Right.Level1_Optional_Id into groupJoinRight
                            from l2Right in groupJoinRight.DefaultIfEmpty()
                            where (l2Left == null ? null : l2Left.Name) == "L2 05" || (l2Right == null ? null : l2Right.Name) != "L2 42"
                            select l1.Id;

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void Select_multiple_nav_prop_reference_optional()
        {
            List<int?> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(e => e.OneToOne_Optional_FK.OneToOne_Optional_FK)
                    .ToList()
                    .Select(e => e.OneToOne_Optional_FK?.OneToOne_Optional_FK?.Id).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne.Select(e => (int?)e.OneToOne_Optional_FK.OneToOne_Optional_FK.Id);
                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void Where_multiple_nav_prop_reference_optional_member_compared_to_value()
        {
            List<int> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(e => e.OneToOne_Optional_FK.OneToOne_Optional_FK)
                    .ToList()
                    .Where(e => e.OneToOne_Optional_FK?.OneToOne_Optional_FK?.Name != "L3 05")
                    .Select(e => e.Id).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from l1 in context.LevelOne
                            where l1.OneToOne_Optional_FK.OneToOne_Optional_FK.Name != "L3 05"
                            select l1;

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem.Id));
                }
            }
        }

        [ConditionalFact]
        public virtual void Where_multiple_nav_prop_reference_optional_member_compared_to_null()
        {
            List<int> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(e => e.OneToOne_Optional_FK.OneToOne_Optional_FK)
                    .ToList()
                    .Where(e => e.OneToOne_Optional_FK?.OneToOne_Optional_FK?.Name != null)
                    .Select(e => e.Id).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from l1 in context.LevelOne
                            where l1.OneToOne_Optional_FK.OneToOne_Optional_FK.Name != null
                            select l1;

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem.Id));
                }
            }
        }

        [ConditionalFact]
        public virtual void Where_multiple_nav_prop_reference_optional_compared_to_null1()
        {
            List<int> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(e => e.OneToOne_Optional_FK.OneToOne_Optional_FK)
                    .ToList()
                    .Where(e => e.OneToOne_Optional_FK?.OneToOne_Optional_FK == null)
                    .Select(e => e.Id).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from l1 in context.LevelOne
                            where l1.OneToOne_Optional_FK.OneToOne_Optional_FK == null
                            select l1;

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem.Id));
                }
            }
        }

        [ConditionalFact]
        public virtual void Where_multiple_nav_prop_reference_optional_compared_to_null2()
        {
            List<int> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelThree
                    .Include(e => e.OneToOne_Optional_FK_Inverse.OneToOne_Optional_FK_Inverse)
                    .ToList()
                    .Where(e => e.OneToOne_Optional_FK_Inverse?.OneToOne_Optional_FK_Inverse == null)
                    .Select(e => e.Id).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from l3 in context.LevelThree
                            where l3.OneToOne_Optional_FK_Inverse.OneToOne_Optional_FK_Inverse == null
                            select l3;

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem.Id));
                }
            }
        }

        [ConditionalFact]
        public virtual void Where_multiple_nav_prop_reference_optional_compared_to_null3()
        {
            List<int> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(e => e.OneToOne_Optional_FK.OneToOne_Optional_FK)
                    .ToList()
                    .Where(e => null != e.OneToOne_Optional_FK?.OneToOne_Optional_FK)
                    .Select(e => e.Id).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from l1 in context.LevelOne
                            where null != l1.OneToOne_Optional_FK.OneToOne_Optional_FK
                            select l1;

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem.Id));
                }
            }
        }

        [ConditionalFact]
        public virtual void Where_multiple_nav_prop_reference_optional_compared_to_null4()
        {
            List<int> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelThree
                    .Include(e => e.OneToOne_Optional_FK_Inverse.OneToOne_Optional_FK_Inverse)
                    .ToList()
                    .Where(e => null != e.OneToOne_Optional_FK_Inverse?.OneToOne_Optional_FK_Inverse)
                    .Select(e => e.Id).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from l3 in context.LevelThree
                            where null != l3.OneToOne_Optional_FK_Inverse.OneToOne_Optional_FK_Inverse
                            select l3;

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem.Id));
                }
            }
        }

        [ConditionalFact]
        public virtual void Where_multiple_nav_prop_reference_optional_compared_to_null5()
        {
            List<int> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(e => e.OneToOne_Optional_FK.OneToOne_Required_FK.OneToOne_Required_FK)
                    .ToList()
                    .Where(e => e.OneToOne_Optional_FK?.OneToOne_Required_FK.OneToOne_Required_FK == null)
                    .Select(e => e.Id).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne.Where(e => e.OneToOne_Optional_FK.OneToOne_Required_FK.OneToOne_Required_FK == null);
                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem.Id));
                }
            }
        }

        [ConditionalFact]
        public virtual void Select_multiple_nav_prop_reference_required()
        {
            List<int?> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(e => e.OneToOne_Required_FK.OneToOne_Required_FK)
                    .ToList()
                    .Select(e => e.OneToOne_Required_FK?.OneToOne_Required_FK?.Id).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne.Select(e => (int?)e.OneToOne_Required_FK.OneToOne_Required_FK.Id);
                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void Select_multiple_nav_prop_reference_required2()
        {
            List<int?> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelThree
                    .Include(e => e.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse)
                    .ToList()
                    .Select(e => e.OneToOne_Required_FK_Inverse?.OneToOne_Required_FK_Inverse?.Id).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelThree.Select(e => (int?)e.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.Id);
                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void Select_multiple_nav_prop_optional_required()
        {
            List<int?> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(e => e.OneToOne_Optional_FK.OneToOne_Required_FK)
                    .ToList()
                    .Select(e => e.OneToOne_Optional_FK?.OneToOne_Required_FK?.Id).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from l1 in context.LevelOne
                            select (int?)l1.OneToOne_Optional_FK.OneToOne_Required_FK.Id;

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void Where_multiple_nav_prop_optional_required()
        {
            List<int> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(e => e.OneToOne_Optional_FK.OneToOne_Required_FK)
                    .ToList()
                    .Where(e => e.OneToOne_Optional_FK?.OneToOne_Required_FK?.Name != "L3 05")
                    .Select(e => e.Id).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from l1 in context.LevelOne
                            where l1.OneToOne_Optional_FK.OneToOne_Required_FK.Name != "L3 05"
                            select l1;

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem.Id));
                }
            }
        }

        [ConditionalFact]
        public virtual void SelectMany_navigation_comparison1()
        {
            List<KeyValuePair<int, int>> expected;
            using (var context = CreateContext())
            {
                expected = (from l11 in context.LevelOne.ToList()
                            from l12 in context.LevelOne.ToList()
                            where l11.Id == l12.Id
                            select new KeyValuePair<int, int>(l11.Id, l12.Id)).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from l11 in context.LevelOne
                            from l12 in context.LevelOne
                            where l11 == l12
                            select new { Id1 = l11.Id, Id2 = l12.Id };

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                var id1s = expected.Select(e => e.Key);
                var id2s = expected.Select(e => e.Value);
                foreach (var resultItem in result)
                {
                    Assert.True(id1s.Contains(resultItem.Id1));
                    Assert.True(id2s.Contains(resultItem.Id2));
                }
            }
        }

        // TODO: broken currently
        ////[ConditionalFact]
        public virtual void SelectMany_navigation_comparison2()
        {
            List<KeyValuePair<int, int>> expected;
            using (var context = CreateContext())
            {
                expected = (from l1 in context.LevelOne.ToList()
                            from l2 in context.LevelTwo.Include(e => e.OneToOne_Optional_FK_Inverse).ToList()
                            where l1.Id == l2.OneToOne_Optional_FK_Inverse?.Id
                            select new KeyValuePair<int, int>(l1.Id, l2.Id)).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from l1 in context.LevelOne
                            from l2 in context.LevelTwo
                            where l1 == l2.OneToOne_Optional_FK_Inverse
                            select new { Id1 = l1.Id, Id2 = l2.Id };

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                var id1s = expected.Select(e => e.Key);
                var id2s = expected.Select(e => e.Value);
                foreach (var resultItem in result)
                {
                    Assert.True(id1s.Contains(resultItem.Id1));
                    Assert.True(id2s.Contains(resultItem.Id2));
                }
            }
        }

        // TODO: broken currently
        ////[ConditionalFact]
        public virtual void SelectMany_navigation_comparison3()
        {
            List<KeyValuePair<int, int>> expected;
            using (var context = CreateContext())
            {
                expected = (from l1 in context.LevelOne.ToList()
                            from l2 in context.LevelTwo.Include(e => e.OneToOne_Optional_FK_Inverse).ToList()
                            where l1.OneToOne_Optional_FK.Id == l2.Id
                            select new KeyValuePair<int, int>(l1.Id, l2.Id)).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from l1 in context.LevelOne
                            from l2 in context.LevelTwo
                            where l1.OneToOne_Optional_FK == l2
                            select new { Id1 = l1.Id, Id2 = l2.Id };

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                var id1s = expected.Select(e => e.Key);
                var id2s = expected.Select(e => e.Value);
                foreach (var resultItem in result)
                {
                    Assert.True(id1s.Contains(resultItem.Id1));
                    Assert.True(id2s.Contains(resultItem.Id2));
                }
            }
        }

        // broken
        ////[ConditionalFact]
        public virtual void Where_complex_predicate_with_with_nav_prop_and_OrElse1()
        {
            List<KeyValuePair<int?, int?>> expected;
            using (var context = CreateContext())
            {
                expected = (from l1 in context.LevelOne.Include(e => e.OneToOne_Optional_FK).ToList()
                            from l2 in context.LevelTwo.Include(e => e.OneToOne_Required_FK_Inverse).ToList()
                            where l1.OneToOne_Optional_FK?.Name == "L2 01" || l2.OneToOne_Required_FK_Inverse.Name != "Bar"
                            select new KeyValuePair<int?, int?>(l1.Id, l2.Id)).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from l1 in context.LevelOne
                            from l2 in context.LevelTwo
                            where l1.OneToOne_Optional_FK.Name == "L2 01" || l2.OneToOne_Required_FK_Inverse.Name != "Bar"
                            select new { Id1 = (int?)l1.Id, Id2 = (int?)l2.Id };

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                var id1s = expected.Select(e => e.Key);
                var id2s = expected.Select(e => e.Value);
                foreach (var resultItem in result)
                {
                    Assert.True(id1s.Contains(resultItem.Id1));
                    Assert.True(id2s.Contains(resultItem.Id2));
                }
            }
        }

        [ConditionalFact]
        public virtual void Where_complex_predicate_with_with_nav_prop_and_OrElse2()
        {
            List<int> expected;
            using (var context = CreateContext())
            {
                expected = (from l1 in context.LevelOne.Include(e => e.OneToOne_Optional_FK.OneToOne_Required_FK).ToList()
                            where l1.OneToOne_Optional_FK?.OneToOne_Required_FK?.Name == "L3 05" || l1.OneToOne_Optional_FK?.Name != "L2 05"
                            select l1.Id).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from l1 in context.LevelOne
                            where l1.OneToOne_Optional_FK.OneToOne_Required_FK.Name == "L3 05" || l1.OneToOne_Optional_FK.Name != "L2 05"
                            select l1.Id;

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void Where_complex_predicate_with_with_nav_prop_and_OrElse3()
        {
            List<int?> expected;
            using (var context = CreateContext())
            {
                expected = (from l1 in context.LevelOne
                                .Include(e => e.OneToOne_Optional_FK)
                                .Include(e => e.OneToOne_Required_FK.OneToOne_Optional_FK)
                                .ToList()
                            where l1.OneToOne_Optional_FK?.Name != "L2 05" || l1.OneToOne_Required_FK.OneToOne_Optional_FK?.Name == "L3 05"
                            select l1?.Id).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from l1 in context.LevelOne
                            where l1.OneToOne_Optional_FK.Name != "L2 05" || l1.OneToOne_Required_FK.OneToOne_Optional_FK.Name == "L3 05"
                            select l1.Id;

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void Where_complex_predicate_with_with_nav_prop_and_OrElse4()
        {
            List<int?> expected;
            using (var context = CreateContext())
            {
                expected = (from l3 in context.LevelThree
                                .Include(e => e.OneToOne_Optional_FK_Inverse)
                                .Include(e => e.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK_Inverse)
                                .ToList()
                            where l3.OneToOne_Optional_FK_Inverse?.Name != "L2 05" || l3.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK_Inverse?.Name == "L1 05"
                            select l3?.Id).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from l3 in context.LevelThree
                            where l3.OneToOne_Optional_FK_Inverse.Name != "L2 05" || l3.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK_Inverse.Name == "L1 05"
                            select l3.Id;

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
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
                var names = expected.Select(e => e.Key);
                var ids = expected.Select(e => e.Value);
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
            List<KeyValuePair<string, int?>> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelThree
                    .Include(e => e.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse)
                    .Include(e => e.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK_Inverse)
                    .ToList()
                    .Where(e =>
                        e.OneToOne_Required_FK_Inverse?.OneToOne_Required_FK_Inverse?.Id == e.OneToOne_Required_FK_Inverse?.OneToOne_Optional_FK_Inverse?.Id
                        && e.OneToOne_Required_FK_Inverse?.OneToOne_Optional_FK_Inverse?.Id != 7)
                    .Select(e => new KeyValuePair<string, int?>
                    (
                        e.Name,
                        e.OneToOne_Required_FK_Inverse?.OneToOne_Optional_FK_Inverse?.Id
                    )).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelThree.Where(e =>
                        e.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse == e.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK_Inverse
                        && e.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK_Inverse.Id != 7)
                    .Select(e => new
                    {
                        e.Name,
                        Id = (int?)e.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK_Inverse.Id
                    });

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                var names = expected.Select(e => e.Key);
                var ids = expected.Select(e => e.Value);
                foreach (var resultItem in result)
                {
                    Assert.True(names.Contains(resultItem.Name));
                    Assert.True(ids.Contains(resultItem.Id));
                }
            }
        }

        [ConditionalFact]
        public virtual void Optional_navigation_projected_into_DTO()
        {
            List<MyOuterDto> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(e => e.OneToOne_Optional_FK)
                    .Select(e => new MyOuterDto
                    {
                        Id = e.Id,
                        Name = e.Name,
                        Inner = e.OneToOne_Optional_FK != null ? new MyInnerDto
                        {
                            Id = e.OneToOne_Optional_FK.Id,
                            Name = e.OneToOne_Optional_FK.Name
                        } : null
                    }).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne.Select(e => new MyOuterDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Inner = e.OneToOne_Optional_FK != null ? new MyInnerDto
                    {
                        Id = (int?)e.OneToOne_Optional_FK.Id,
                        Name = e.OneToOne_Optional_FK.Name
                    } : null
                });

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    var expectedElement = expected.Where(e => e.Id == resultItem.Id).Single();
                    Assert.True(expectedElement.Name == resultItem.Name);
                    Assert.True(expectedElement.Inner?.Id == resultItem.Inner?.Id);
                    Assert.True(expectedElement.Inner?.Name == resultItem.Inner?.Name);
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
            List<int> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(e => e.OneToOne_Optional_FK)
                    .ToList()
                    .OrderBy(e => e?.OneToOne_Optional_FK?.Name)
                    .ThenBy(e => e.Id)
                    .Select(e => e.Id)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne.OrderBy(e => e.OneToOne_Optional_FK.Name).ThenBy(e => e.Id).Select(e => e.Id);
                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                for (var i = 0; i < result.Count; i++)
                {
                    Assert.Equal(expected[i], result[i]);
                }
            }
        }

        [ConditionalFact]
        public virtual void OrderBy_nav_prop_reference_optional_via_DefaultIfEmpty()
        {
            List<int> expected;
            using (var context = CreateContext())
            {
                var l1s = context.LevelOne.ToList();
                var l2s = context.LevelTwo.ToList();

                expected = (from l1 in l1s
                            join l2 in l2s on l1.Id equals l2.Level1_Optional_Id into groupJoin
                            from l2 in groupJoin.DefaultIfEmpty()
                            orderby l2 == null ? null : l2.Name, l1.Id
                            select l1.Id).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from l1 in context.LevelOne
                            join l2 in context.LevelTwo on l1.Id equals l2.Level1_Optional_Id into groupJoin
                            from l2 in groupJoin.DefaultIfEmpty()
                            orderby l2 == null ? null : l2.Name, l1.Id
                            select l1.Id;

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
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
            List<int> expected;
            using (var context = CreateContext())
            {
                expected = (from e3 in context.LevelThree.ToList()
                            join e1 in context.LevelOne.ToList()
                            on
                            (int?)e3.Id
                            equals
                            (
                                from subQuery2 in context.LevelTwo.ToList()
                                join subQuery3 in context.LevelThree.ToList()
                                on
                                subQuery2 != null ? (int?)subQuery2.Id : null
                                equals
                                subQuery3.Level2_Optional_Id
                                into
                                grouping
                                from subQuery3 in grouping.DefaultIfEmpty()
                                select subQuery3 != null ? (int?)subQuery3.Id : null
                            ).FirstOrDefault()
                            select e1.Id).ToList();

            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from e3 in context.LevelThree
                            join e1 in context.LevelOne
                            on
                            (int?)e3.Id
                            equals
                            (
                                from subQuery2 in context.LevelTwo
                                join subQuery3 in context.LevelThree
                                on
                                subQuery2 != null ? (int?)subQuery2.Id : null
                                equals
                                subQuery3.Level2_Optional_Id
                                into
                                grouping
                                from subQuery3 in grouping.DefaultIfEmpty()
                                select subQuery3 != null ? (int?)subQuery3.Id : null
                            ).FirstOrDefault()
                            select e1.Id;

                var result = query.ToList();

                Assert.Equal(expected, result);
            }
        }

        [ConditionalFact]
        public virtual void SelectMany_navigation_property()
        {
            List<int> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(e => e.OneToMany_Optional)
                    .ToList()
                    .SelectMany(e => e.OneToMany_Optional).Select(e => e.Id)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne.SelectMany(l1 => l1.OneToMany_Optional);

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem.Id));
                }
            }
        }

        [ConditionalFact]
        public virtual void SelectMany_navigation_property_and_projection()
        {
            List<string> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(e => e.OneToMany_Optional)
                    .ToList()
                    .SelectMany(e => e.OneToMany_Optional).Select(e => e.Name)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne.SelectMany(l1 => l1.OneToMany_Optional).Select(e => e.Name);

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void SelectMany_navigation_property_and_filter_before()
        {
            List<int> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(e => e.OneToMany_Optional)
                    .ToList()
                    .Where(e => e.Id == 1)
                    .SelectMany(e => e.OneToMany_Optional).Select(e => e.Id)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne
                    .Where(e => e.Id == 1)
                    .SelectMany(l1 => l1.OneToMany_Optional);

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem.Id));
                }
            }
        }

        [ConditionalFact]
        public virtual void SelectMany_navigation_property_and_filter_after()
        {
            List<int> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(e => e.OneToMany_Optional)
                    .ToList()
                    .SelectMany(e => e.OneToMany_Optional).Select(e => e.Id)
                    .Where(e => e != 6)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne
                    .SelectMany(l1 => l1.OneToMany_Optional)
                    .Where(e => e.Id != 6);

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem.Id));
                }
            }
        }

        [ConditionalFact]
        public virtual void SelectMany_nested_navigation_property_required()
        {
            List<int> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(e => e.OneToOne_Required_FK.OneToMany_Optional)
                    .ToList()
                    .SelectMany(e => e.OneToOne_Required_FK?.OneToMany_Optional ?? new List<Level3>()).Select(e => e.Id)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne.SelectMany(l1 => l1.OneToOne_Required_FK.OneToMany_Optional);

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem.Id));
                }
            }
        }

        [ConditionalFact]
        public virtual void SelectMany_nested_navigation_property_optional_and_projection()
        {
            List<string> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(e => e.OneToOne_Optional_FK.OneToMany_Optional)
                    .ToList()
                    .Where(e => e.OneToOne_Optional_FK != null)
                    .SelectMany(e => e.OneToOne_Optional_FK.OneToMany_Optional).Select(e => e.Name)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne.SelectMany(l1 => l1.OneToOne_Optional_FK.OneToMany_Optional).Select(e => e.Name);

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void Multiple_SelectMany_calls()
        {
            List<string> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(e => e.OneToMany_Optional).ThenInclude(e => e.OneToMany_Optional)
                    .ToList()
                    .SelectMany(e => e.OneToMany_Optional)
                    .SelectMany(e => e.OneToMany_Optional)
                    .Select(e => e.Name)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne.SelectMany(e => e.OneToMany_Optional).SelectMany(e => e.OneToMany_Optional);
                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem.Name));
                }
            }
        }

        [ConditionalFact]
        public virtual void SelectMany_navigation_property_with_another_navigation_in_subquery()
        {
            List<string> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(e => e.OneToMany_Optional).ThenInclude(e => e.OneToOne_Optional_FK)
                    .ToList()
                    .SelectMany(e => e.OneToMany_Optional.Select(l2 => l2.OneToOne_Optional_FK))
                    .Select(e => e?.Name)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne.SelectMany(l1 => l1.OneToMany_Optional.Select(l2 => l2.OneToOne_Optional_FK));
                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem?.Name));
                }
            }
        }

        [ConditionalFact]
        public virtual void Where_navigation_property_to_collection()
        {
            List<string> expected;

            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(l1 => l1.OneToOne_Required_FK)
                    .ThenInclude(l1 => l1.OneToMany_Optional)
                    .ToList()
                    .Where(l1 => l1?.OneToOne_Required_FK?.OneToMany_Optional?.Count > 0)
                    .Select(e => e?.Name)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne.Where(l1 => l1.OneToOne_Required_FK.OneToMany_Optional.Count > 0);
                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem?.Name));
                }
            }
        }

        [ConditionalFact]
        public virtual void Where_navigation_property_to_collection2()
        {
            List<string> expected;

            using (var context = CreateContext())
            {
                expected = context.LevelThree
                    .Include(l1 => l1.OneToOne_Required_FK_Inverse)
                    .ThenInclude(l1 => l1.OneToMany_Optional)
                    .ToList()
                    .Where(l1 => l1?.OneToOne_Required_FK_Inverse?.OneToMany_Optional?.Count > 0)
                    .Select(e => e?.Name)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelThree.Where(l1 => l1.OneToOne_Required_FK_Inverse.OneToMany_Optional.Count > 0);
                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem?.Name));
                }
            }
        }

        [ConditionalFact]
        public virtual void Where_navigation_property_to_collection_of_original_entity_type()
        {
            List<string> expected;

            using (var context = CreateContext())
            {
                expected = context.LevelTwo
                    .Include(l2 => l2.OneToMany_Required_Inverse)
                    .ThenInclude(l1 => l1.OneToMany_Optional)
                    .ToList()
                    .Where(l2 => l2?.OneToMany_Required_Inverse.OneToMany_Optional?.Count() > 0)
                    .Select(e => e?.Name)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelTwo.Where(l2 => l2.OneToMany_Required_Inverse.OneToMany_Optional.Count() > 0);
                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem?.Name));
                }
            }
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
            List<string> expected;

            using (var context = CreateContext())
            {
                expected = (from l1 in context.LevelOne.ToList()
                            where context.LevelTwo.ToList().Any(l2 => l2.Level1_Required_Id == l1.Id)
                            select l1.Name).Distinct().ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = (from l1 in context.LevelOne
                             where context.LevelTwo.Any(l2 => l2.Level1_Required_Id == l1.Id)
                             select l1.Name).Distinct();

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void Correlated_subquery_doesnt_project_unnecessary_columns_in_top_level_join()
        {
            List<Level1> levelOnes;
            List<Level2> levelTwos;
            using (var context = CreateContext())
            {
                levelOnes = context.LevelOne.ToList();
                levelTwos = context.LevelTwo.Include(e => e.OneToOne_Optional_FK_Inverse).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from e1 in context.LevelOne
                            join e2 in context.LevelTwo on e1.Id equals e2.OneToOne_Optional_FK_Inverse.Id
                            where context.LevelTwo.Any(l2 => l2.Level1_Required_Id == e1.Id)
                            select new { Name1 = e1.Name, Id2 = e2.Id };

                var result = query.ToList();

                var expected = (from l1 in levelOnes
                                join l2 in levelTwos on l1.Id equals l2.OneToOne_Optional_FK_Inverse?.Id
                                where levelTwos.Any(l2 => l2.Level1_Required_Id == l1.Id)
                                select new { Name1 = l1.Name, Id2 = l2.Id }).ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void Correlated_nested_subquery_doesnt_project_unnecessary_columns_in_top_level()
        {
            List<string> expected;

            using (var context = CreateContext())
            {
                expected = (from l1 in context.LevelOne.ToList()
                            where context.LevelTwo.ToList().Any(l2 => context.LevelThree.ToList().Select(l3 => l1.Id).Any())
                            select l1.Name).Distinct().ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = (from l1 in context.LevelOne
                             where context.LevelTwo.Any(l2 => context.LevelThree.Select(l3 => l2.Id).Any())
                             select l1.Name).Distinct();

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void Correlated_nested_two_levels_up_subquery_doesnt_project_unnecessary_columns_in_top_level()
        {
            List<string> expected;

            using (var context = CreateContext())
            {
                expected = (from l1 in context.LevelOne.ToList()
                            where context.LevelTwo.ToList().Any(l2 => context.LevelThree.ToList().Select(l3 => l1.Id).Any())
                            select l1.Name).Distinct().ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = (from l1 in context.LevelOne
                             where context.LevelTwo.Any(l2 => context.LevelThree.Select(l3 => l1.Id).Any())
                             select l1.Name).Distinct();

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void GroupJoin_on_subquery_and_set_operation_on_grouping_but_nothing_from_grouping_is_projected()
        {
            List<string> expected;

            using (var context = CreateContext())
            {
                expected = context.LevelOne.ToList()
                    .GroupJoin(
                        context.LevelTwo.ToList().Where(l2 => l2.Name != "L2 01"),
                        l1 => l1.Id,
                        l2 => l2.Level1_Optional_Id,
                        (l1, l2s) => new { l1, l2s })
                    .Where(r => r.l2s.Any())
                    .Select(r => r.l1.Name)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne
                    .GroupJoin(
                        context.LevelTwo.Where(l2 => l2.Name != "L2 01"),
                        l1 => l1.Id,
                        l2 => l2.Level1_Optional_Id,
                        (l1, l2s) => new { l1, l2s })
                    .Where(r => r.l2s.Any())
                    .Select(r => r.l1);

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem.Name));
                }
            }
        }

        [ConditionalFact]
        public virtual void GroupJoin_on_complex_subquery_and_set_operation_on_grouping_but_nothing_from_grouping_is_projected()
        {
            List<string> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne.ToList()
                    .GroupJoin(
                        context.LevelOne
                            .Include(l1 => l1.OneToOne_Required_FK)
                            .ToList()
                            .Where(l1 => l1.Name != "L1 01")
                            .Select(l1 => l1.OneToOne_Required_FK),
                        l1 => l1.Id,
                        l2 => l2?.Level1_Optional_Id,
                        (l1, l2s) => new
                        {
                            l1,
                            l2s
                        })
                    .Where(r => r.l2s.Any())
                    .Select(r => r.l1.Name)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne
                    .GroupJoin(
                        context.LevelOne.Where(l1 => l1.Name != "L1 01").Select(l1 => l1.OneToOne_Required_FK),
                        l1 => l1.Id,
                        l2 => l2 != null ? l2.Level1_Optional_Id : null,
                        (l1, l2s) => new { l1, l2s })
                    .Where(r => r.l2s.Any())
                    .Select(r => r.l1);

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem.Name));
                }
            }
        }

        // issue #6429
        [ConditionalFact]
        public virtual void Null_protection_logic_work_for_inner_key_access_of_manually_created_GroupJoin1()
        {
            List<string> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne.ToList()
                    .GroupJoin(
                        context.LevelOne
                            .Include(l1 => l1.OneToOne_Required_FK)
                            .ToList()
                            .Select(l1 => l1.OneToOne_Required_FK),
                        l1 => l1.Id,
                        l2 => l2?.Level1_Optional_Id,
                        (l1, l2s) => new
                        {
                            l1,
                            l2s
                        })
                    .Select(r => r.l1.Name)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne
                    .GroupJoin(
                        context.LevelOne.Select(l1 => l1.OneToOne_Required_FK),
                        l1 => l1.Id,
                        l2 => l2.Level1_Optional_Id,
                        (l1, l2s) => new { l1, l2s })
                    .Select(r => r.l1);

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem.Name));
                }
            }
        }

        [ConditionalFact]
        public virtual void Null_protection_logic_work_for_inner_key_access_of_manually_created_GroupJoin2()
        {
            List<string> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne.ToList()
                    .GroupJoin(
                        context.LevelOne
                            .Include(l1 => l1.OneToOne_Required_FK)
                            .ToList()
                            .Select(l1 => l1.OneToOne_Required_FK),
                        l1 => l1.Id,
                        l2 => l2?.Level1_Optional_Id,
                        (l1, l2s) => new
                        {
                            l1,
                            l2s
                        })
                    .Select(r => r.l1.Name)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne
                    .GroupJoin(
                        context.LevelOne.Select(l1 => l1.OneToOne_Required_FK),
                        l1 => l1.Id,
                        l2 => EF.Property<int?>(l2, "Level1_Optional_Id"),
                        (l1, l2s) => new { l1, l2s })
                    .Select(r => r.l1);

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem.Name));
                }
            }
        }

        [ConditionalFact]
        public virtual void Null_protection_logic_work_for_outer_key_access_of_manually_created_GroupJoin()
        {
            List<string> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne.Include(l1 => l1.OneToOne_Required_FK).ToList().Select(l1 => l1.OneToOne_Required_FK)
                    .GroupJoin(
                        context.LevelOne,
                        l2 => l2?.Level1_Optional_Id,
                        l1 => l1.Id,
                        (l2, l1s) => new
                        {
                            l2,
                            l1s
                        })
                    .Select(r => r.l2?.Name)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne.Select(l1 => l1.OneToOne_Required_FK)
                    .GroupJoin(
                        context.LevelOne,
                        l2 => l2.Level1_Optional_Id,
                        l1 => l1.Id,
                        (l2, l1s) => new { l2, l1s })
                    .Select(r => r.l2);

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem?.Name));
                }
            }
        }

        [ConditionalFact]
        public virtual void SelectMany_where_with_subquery()
        {
            List<string> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne.Include(l1 => l1.OneToMany_Required).ThenInclude(l2 => l2.OneToMany_Required)
                    .ToList()
                    .SelectMany(l1 => l1.OneToMany_Required).Where(l2 => l2.OneToMany_Required.Any())
                    .Select(l2 => l2.Name)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne.SelectMany(l1 => l1.OneToMany_Required).Where(l2 => l2.OneToMany_Required.Any());
                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem?.Name));
                }
            }
        }

        [ConditionalFact]
        public virtual void Required_navigation_take_required_navigation()
        {
            List<string> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelThree.Include(l3 => l3.OneToOne_Required_FK_Inverse).ThenInclude(l2 => l2.OneToOne_Required_FK_Inverse)
                    .ToList()
                    .Select(l3 => l3.OneToOne_Required_FK_Inverse)
                    .OrderBy(l2 => l2.Id)
                    .Take(10)
                    .Select(l2 => l2.OneToOne_Required_FK_Inverse.Name)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelThree
                    .Select(l3 => l3.OneToOne_Required_FK_Inverse)
                    .OrderBy(l3 => l3.Id)
                    .Take(10)
                    .Select(l2 => l2.OneToOne_Required_FK_Inverse.Name);

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        // issue #6618
        ////[ConditionalFact]
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
            List<string> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelTwo.Include(l2 => l2.OneToOne_Required_FK_Inverse).ToList()
                    .Where(l2 => l2.OneToOne_Required_FK_Inverse.Name == "L1 03")
                    .Take(3)
                    .Select(l2 => l2.Name)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelTwo
                    .Where(l2 => l2.OneToOne_Required_FK_Inverse.Name == "L1 03")
                    .Take(3)
                    .Select(l2 => l2.Name);

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void Projection_select_correct_table_with_anonymous_projection_in_subquery()
        {
            List<string> expected;
            using (var context = CreateContext())
            {
                expected = (from l2 in context.LevelTwo.ToList()
                            join l1 in context.LevelOne.ToList()
                               on l2.Level1_Required_Id equals l1.Id
                            join l3 in context.LevelThree.ToList()
                               on l1.Id equals l3.Level2_Required_Id
                            where l1.Name == "L1 03"
                            where l3.Name == "L3 08"
                            select new { l2, l1 })
                    .Take(3)
                    .Select(l => l.l2.Name)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = (from l2 in context.LevelTwo
                             join l1 in context.LevelOne
                                on l2.Level1_Required_Id equals l1.Id
                             join l3 in context.LevelThree
                                on l1.Id equals l3.Level2_Required_Id
                             where l1.Name == "L1 03"
                             where l3.Name == "L3 08"
                             select new { l2, l1 })
                    .Take(3)
                    .Select(l => l.l2.Name);

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void Projection_select_correct_table_in_subquery_when_materialization_is_not_required_in_multiple_joins()
        {
            List<string> expected;
            using (var context = CreateContext())
            {
                expected = (from l2 in context.LevelTwo.ToList()
                            join l1 in context.LevelOne.ToList()
                               on l2.Level1_Required_Id equals l1.Id
                            join l3 in context.LevelThree.ToList()
                               on l1.Id equals l3.Level2_Required_Id
                            where l1.Name == "L1 03"
                            where l3.Name == "L3 08"
                            select l1)
                    .Take(3)
                    .Select(l1 => l1.Name)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = (from l2 in context.LevelTwo
                             join l1 in context.LevelOne
                                on l2.Level1_Required_Id equals l1.Id
                             join l3 in context.LevelThree
                                on l1.Id equals l3.Level2_Required_Id
                             where l1.Name == "L1 03"
                             where l3.Name == "L3 08"
                             select l1)
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

        //[ConditionalFact] TODO: See issue#6782
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
        public virtual void GroupJoin_in_subquery_with_client_result_operator()
        {
            List<string> expected;
            using (var context = CreateContext())
            {
                expected = (from l1 in context.LevelOne.ToList()
                            where (from l1_inner in context.LevelOne.ToList()
                                   join l2_inner in context.LevelTwo.ToList() on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping
                                   from l2_inner in grouping.DefaultIfEmpty()
                                   select l1_inner).Distinct().Count() > 7
                            where l1.Id < 3
                            select l1.Name).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from l1 in context.LevelOne
                            where (from l1_inner in context.LevelOne
                                   join l2_inner in context.LevelTwo on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping
                                   from l2_inner in grouping.DefaultIfEmpty()
                                   select l1_inner).Distinct().Count() > 7
                            where l1.Id < 3
                            select l1.Name;

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void GroupJoin_in_subquery_with_client_projection()
        {
            List<string> expected;
            using (var context = CreateContext())
            {
                expected = (from l1 in context.LevelOne.ToList()
                            where (from l1_inner in context.LevelOne.ToList()
                                   join l2_inner in context.LevelTwo.ToList() on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping
                                   from l2_inner in grouping.DefaultIfEmpty()
                                   select l1_inner).Distinct().Count() > 7
                            where l1.Id < 3
                            select l1.Name).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from l1 in context.LevelOne
                            where (from l1_inner in context.LevelOne
                                   join l2_inner in context.LevelTwo on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping
                                   from l2_inner in grouping.DefaultIfEmpty()
                                   select ClientStringMethod(l1_inner.Name)).Count() > 7
                            where l1.Id < 3
                            select l1.Name;

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void GroupJoin_in_subquery_with_client_projection_nested1()
        {
            List<string> expected;
            using (var context = CreateContext())
            {
                expected = (from l1_outer in context.LevelOne.ToList()
                            where (from l1_middle in context.LevelOne.ToList()
                                   join l2_middle in context.LevelTwo.ToList() on l1_middle.Id equals l2_middle.Level1_Optional_Id into grouping_middle
                                   from l2_middle in grouping_middle.DefaultIfEmpty()
                                   where (from l1_inner in context.LevelOne.ToList()
                                          join l2_inner in context.LevelTwo.ToList() on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping_inner
                                          from l2_inner in grouping_inner.DefaultIfEmpty()
                                          select ClientStringMethod(l1_inner.Name)).Count() > 7
                                   select l1_middle).Take(10).Count() > 4
                            where l1_outer.Id < 2
                            select l1_outer.Name).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from l1_outer in context.LevelOne
                            where (from l1_middle in context.LevelOne
                                   join l2_middle in context.LevelTwo on l1_middle.Id equals l2_middle.Level1_Optional_Id into grouping_middle
                                   from l2_middle in grouping_middle.DefaultIfEmpty()
                                   where (from l1_inner in context.LevelOne
                                          join l2_inner in context.LevelTwo on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping_inner
                                          from l2_inner in grouping_inner.DefaultIfEmpty()
                                          select ClientStringMethod(l1_inner.Name)).Count() > 7
                                   select l1_middle).Take(10).Count() > 4
                            where l1_outer.Id < 2
                            select l1_outer.Name;

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void GroupJoin_in_subquery_with_client_projection_nested2()
        {
            List<string> expected;
            using (var context = CreateContext())
            {
                expected = (from l1_outer in context.LevelOne.ToList()
                            where (from l1_middle in context.LevelOne.ToList()
                                   join l2_middle in context.LevelTwo.ToList() on l1_middle.Id equals l2_middle.Level1_Optional_Id into grouping_middle
                                   from l2_middle in grouping_middle.DefaultIfEmpty()
                                   where (from l1_inner in context.LevelOne.ToList()
                                          join l2_inner in context.LevelTwo.ToList() on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping_inner
                                          from l2_inner in grouping_inner.DefaultIfEmpty()
                                          select l1_inner.Name).Count() > 7
                                   select ClientStringMethod(l1_middle.Name)).Count() > 4
                            where l1_outer.Id < 2
                            select l1_outer.Name).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from l1_outer in context.LevelOne
                            where (from l1_middle in context.LevelOne
                                   join l2_middle in context.LevelTwo on l1_middle.Id equals l2_middle.Level1_Optional_Id into grouping_middle
                                   from l2_middle in grouping_middle.DefaultIfEmpty()
                                   where (from l1_inner in context.LevelOne
                                          join l2_inner in context.LevelTwo on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping_inner
                                          from l2_inner in grouping_inner.DefaultIfEmpty()
                                          select l1_inner.Name).Count() > 7
                                   select ClientStringMethod(l1_middle.Name)).Count() > 4
                            where l1_outer.Id < 2
                            select l1_outer.Name;

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        private static string ClientStringMethod(string argument)
        {
            return argument;
        }
    }
}
