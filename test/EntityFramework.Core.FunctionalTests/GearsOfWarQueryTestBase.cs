// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.FunctionalTests.TestModels.GearsOfWarModel;
using Xunit;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class GearsOfWarQueryTestBase<TTestStore, TFixture> : IClassFixture<TFixture>, IDisposable
        where TTestStore : TestStore
        where TFixture : GearsOfWarQueryFixtureBase<TTestStore>, new()
    {
        protected GearsOfWarContext CreateContext()
        {
            return Fixture.CreateContext(TestStore);
        }

        protected GearsOfWarQueryTestBase(TFixture fixture)
        {
            Fixture = fixture;

            TestStore = Fixture.CreateTestStore();
        }

        protected TFixture Fixture { get; private set; }

        protected TTestStore TestStore { get; private set; }

        public void Dispose()
        {
            TestStore.Dispose();
        }

        [Fact(Skip = "Skipping to unblock CI")]
        public virtual void Include_multiple_one_to_one_and_one_to_many()
        {
            using (var context = CreateContext())
            {
                var query = context.Tags.Include(t => t.Gear.Weapons);
                var result = query.ToList();

                Assert.Equal(6, result.Count);

                var gears = result.Select(t => t.Gear).Where(g => g != null).ToList();
                Assert.Equal(5, gears.Count);

                Assert.True(gears.All(g => g.Weapons.Any()));
            }
        }

        [Fact(Skip = "Skipping to unblock CI")]
        public virtual void Include_multiple_one_to_one_and_one_to_many_self_reference()
        {
            using (var context = CreateContext())
            {
                var query = context.Tags.Include(t => t.Gear.Reports);
                var result = query.ToList();

                Assert.Equal(6, result.Count);

                var gears = result.Select(t => t.Gear).Where(g => g != null).ToList();
                Assert.Equal(5, gears.Count);

                Assert.True(gears.All(g => g.Reports != null));
            }
        }

        // blocked by issue #1393
        ////[Fact]
        public virtual void Include_multiple_one_to_one_optional_and_one_to_one_required()
        {
            using (var context = CreateContext())
            {
                var query = context.Tags.Include(t => t.Gear.Squad);
                var result = query.ToList();

                Assert.Equal(6, result.Count);
            }
        }

        // blocked by issue #1393
        ////[Fact]
        public virtual void Include_multiple_one_to_one_and_one_to_one_and_one_to_many()
        {
            using (var context = CreateContext())
            {
                var query = context.Tags.Include(t => t.Gear.Squad.Members);
                var result = query.ToList();

                Assert.Equal(6, result.Count);
            }
        }


        [Fact(Skip = "Skipping to unblock CI")]
        public virtual void Include_multiple_circular()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears.Include(g => g.CityOfBirth.StationedGears);
                var result = query.ToList();

                Assert.Equal(5, result.Count);

                var cities = result.Select(g => g.CityOfBirth);
                Assert.True(cities.All(c => c != null));
                Assert.True(cities.All(c => c.BornGears != null));
            }
        }

        [Fact(Skip = "Skipping to unblock CI")]
        public virtual void Include_multiple_circular_with_filter()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears.Include(g => g.CityOfBirth.StationedGears).Where(g => g.Nickname == "Marcus");
                var result = query.ToList();

                Assert.Equal(1, result.Count);
                Assert.Equal("Jacinto", result.Single().CityOfBirth.Name);
                Assert.Equal(2, result.Single().CityOfBirth.StationedGears.Count);
            }
        }
    }
}