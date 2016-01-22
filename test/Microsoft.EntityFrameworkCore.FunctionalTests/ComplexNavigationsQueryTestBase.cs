// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.FunctionalTests.TestModels.ComplexNavigationsModel;
using Microsoft.EntityFrameworkCore.FunctionalTests.TestUtilities.Xunit;
using Xunit;
using System.Collections.Generic;
// ReSharper disable MergeConditionalExpression
// ReSharper disable ReplaceWithSingleCallToSingle
// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace Microsoft.EntityFrameworkCore.FunctionalTests
{
    [MonoVersionCondition(Min = "4.2.0", SkipReason = "Queries fail on Mono < 4.2.0 due to differences in the implementation of LINQ")]
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
                var result = context.LevelOne.Include(e => e.OneToMany_Optional).ThenInclude(e => e.OneToMany_Optional).ToList();

                Assert.Equal(10, result.Count);

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
                var result = context.LevelOne.Include(e => e.OneToMany_Optional).ThenInclude(e => e.OneToMany_Optional).ThenInclude(e => e.OneToMany_Required_Inverse.OneToMany_Optional).ToList();

                Assert.Equal(10, result.Count);

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

        // issue #3186
        ////[ConditionalFact]
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

        // issue #3186
        ////[ConditionalFact]
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
                                join e1 in levelOnes on e3.Id equals e1?.OneToOne_Required_FK.OneToOne_Optional_FK?.Id
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
                                join e1 in levelOnes on e4.Name equals e1?.OneToOne_Required_FK.OneToOne_Optional_FK?.OneToOne_Required_PK?.Name
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

        // issue #3186
        ////[ConditionalFact]
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
                for (var i = 0; i < result.Count; i++)
                {
                    Assert.True(expected.Contains(result[i]));
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
                for (var i = 0; i < result.Count; i++)
                {
                    Assert.True(expected.Contains(result[i]));
                }
            }
        }

        // issue #3186
        ////[ConditionalFact]
        public virtual void Select_nav_prop_reference_optional2()
        {
            List<int> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(e => e.OneToOne_Optional_FK)
                    .ToList()
                    .Select(e => e.OneToOne_Optional_FK.Id).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne.Select(e => e.OneToOne_Optional_FK.Id);
                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                for (var i = 0; i < result.Count; i++)
                {
                    Assert.True(expected.Contains(result[i]));
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
                for (var i = 0; i < result.Count; i++)
                {
                    Assert.True(expected.Contains(result[i]));
                }
            }
        }

        // issue #3186
        ////[ConditionalFact]
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
                for (var i = 0; i < result.Count; i++)
                {
                    Assert.True(expected.Contains(result[i]));
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
                for (var i = 0; i < result.Count; i++)
                {
                    Assert.True(expected.Contains(result[i]));
                }
            }
        }

        // issue #3186
        ////[ConditionalFact]
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
                for (var i = 0; i < result.Count; i++)
                {
                    Assert.True(expected.Contains(result[i]));
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
                for (var i = 0; i < result.Count; i++)
                {
                    Assert.True(expected.Contains(result[i]));
                }
            }
        }

        // issue #3186
        ////[ConditionalFact]
        public virtual void OrderBy_nav_prop_reference_optional()
        {
            List<int> expected;
            using (var context = CreateContext())
            {
                expected = context.LevelOne
                    .Include(e => e.OneToOne_Optional_FK)
                    .ToList()
                    .OrderBy(e => e?.OneToOne_Optional_FK?.Name)
                    .Select(e => e.Id)
                    .ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = context.LevelOne.OrderBy(e => e.OneToOne_Optional_FK.Name).Select(e => e.Id);
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
                            orderby l2 == null ? null : l2.Name
                            select l1.Id).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from l1 in context.LevelOne
                            join l2 in context.LevelTwo on l1.Id equals l2.Level1_Optional_Id into groupJoin
                            from l2 in groupJoin.DefaultIfEmpty()
                            orderby l2 == null ? null : l2.Name
                            select l1.Id;

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                for (var i = 0; i < result.Count; i++)
                {
                    Assert.True(expected.Contains(result[i]));
                }
            }
        }

        // issue #3186
        ////[ConditionalFact]
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
                var result = context.LevelOne.Sum(e => e.OneToOne_Optional_FK.Level1_Required_Id);

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
    }
}
