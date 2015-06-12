// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.FunctionalTests.TestModels.ComplexNavigationsModel;
using Xunit;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class ComplexNavigationsQueryTestBase<TTestStore, TFixture> : IClassFixture<TFixture>, IDisposable
        where TTestStore : TestStore
        where TFixture : ComplexNavigationsQueryFixtureBase<TTestStore>, new()
    {
        protected ComplexNavigationsContext CreateContext()
        {
            return Fixture.CreateContext(TestStore);
        }

        protected ComplexNavigationsQueryTestBase(TFixture fixture)
        {
            Fixture = fixture;

            TestStore = Fixture.CreateTestStore();
        }

        protected TFixture Fixture { get; }

        protected TTestStore TestStore { get; }

        public void Dispose()
        {
            TestStore.Dispose();
        }

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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
    }
}
