// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.FunctionalTests.TestModels.GearsOfWarModel;
using Xunit;
using System.Collections.Generic;

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

        protected TFixture Fixture { get; }

        protected TTestStore TestStore { get; }

        protected virtual void ClearLog()
        {
        }

        public void Dispose()
        {
            TestStore.Dispose();
        }

        [Fact]
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

        [Fact]
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

        [Fact]
        public virtual void Include_multiple_one_to_one_optional_and_one_to_one_required()
        {
            using (var context = CreateContext())
            {
                var query = context.Tags.Include(t => t.Gear.Squad);
                var result = query.ToList();

                Assert.Equal(6, result.Count);
            }
        }

        [Fact]
        public virtual void Include_multiple_one_to_one_and_one_to_one_and_one_to_many()
        {
            using (var context = CreateContext())
            {
                var query = context.Tags.Include(t => t.Gear.Squad.Members);
                var result = query.ToList();

                Assert.Equal(6, result.Count);
            }
        }

        [Fact]
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

        [Fact]
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

        [Fact]
        public virtual void Include_multiple_include_then_include()
        {
            var gearAssignedCities = new Dictionary<string, string>();
            var gearCitiesOfBirth = new Dictionary<string, string>();
            var gearTagNotes = new Dictionary<string, string>();
            var cityStationedGears = new Dictionary<string, List<string>>();
            var cityBornGears = new Dictionary<string, List<string>>();

            using (var context = CreateContext())
            {
                gearAssignedCities = context.Gears
                    .Include(g => g.AssignedCity)
                    .ToDictionary(g => g.Nickname, g => g.AssignedCity?.Name);

                gearCitiesOfBirth = context.Gears
                    .Include(g => g.CityOfBirth)
                    .ToDictionary(g => g.Nickname, g => g.CityOfBirth?.Name);

                gearTagNotes = context.Gears
                    .Include(g => g.Tag)
                    .ToDictionary(g => g.Nickname, g => g.Tag.Note);

                cityBornGears = context.Cities
                    .Include(c => c.BornGears)
                    .ToDictionary(
                        c => c.Name, 
                        c => c.BornGears != null 
                            ? c.BornGears.Select(g => g.Nickname).ToList() 
                            : new List<string>());

                cityStationedGears = context.Cities
                    .Include(c => c.StationedGears)
                    .ToDictionary(
                        c => c.Name, 
                        c => c.StationedGears != null 
                            ? c.StationedGears.Select(g => g.Nickname).ToList()
                            : new List<string>());
            }

            ClearLog();
            using (var context = CreateContext())
            {
                var query = context.Gears
                    .Include(g => g.AssignedCity.BornGears).ThenInclude(g => g.Tag)
                    .Include(g => g.AssignedCity.StationedGears).ThenInclude(g => g.Tag)
                    .Include(g => g.CityOfBirth.BornGears).ThenInclude(g => g.Tag)
                    .Include(g => g.CityOfBirth.StationedGears).ThenInclude(g => g.Tag)
                    .OrderBy(g => g.Nickname);

                var result = query.ToList();

                var expectedGearCount = 5;
                Assert.Equal(expectedGearCount, result.Count);
                Assert.Equal("Baird", result[0].Nickname);
                Assert.Equal("Cole Train", result[1].Nickname);
                Assert.Equal("Dom", result[2].Nickname);
                Assert.Equal("Marcus", result[3].Nickname);
                Assert.Equal("Paduk", result[4].Nickname);

                for (int i = 0; i < expectedGearCount; i++)
                {
                    Assert.Equal(gearAssignedCities[result[i]?.Nickname], result[i].AssignedCity?.Name);
                    Assert.Equal(gearCitiesOfBirth[result[i]?.Nickname], result[i].CityOfBirth?.Name);

                    var assignedCity = result[i].AssignedCity;
                    if (assignedCity != null)
                    {
                        Assert.Equal(cityBornGears[assignedCity.Name].Count, assignedCity.BornGears.Count);
                        foreach (var bornGear in assignedCity.BornGears)
                        {
                            Assert.True(cityBornGears[assignedCity.Name].Contains(bornGear.Nickname));
                            Assert.Equal(gearTagNotes[bornGear.Nickname], bornGear.Tag.Note);
                        }

                        Assert.Equal(cityStationedGears[assignedCity.Name].Count, assignedCity.StationedGears.Count);
                        foreach (var stationedGear in assignedCity.StationedGears)
                        {
                            Assert.True(cityStationedGears[assignedCity.Name].Contains(stationedGear.Nickname));
                            Assert.Equal(gearTagNotes[stationedGear.Nickname], stationedGear.Tag.Note);
                        }
                    }

                    var cityOfBirth = result[i].CityOfBirth;
                    if (cityOfBirth != null)
                    {
                        Assert.Equal(cityBornGears[cityOfBirth.Name].Count, cityOfBirth.BornGears.Count);
                        foreach (var bornGear in cityOfBirth.BornGears)
                        {
                            Assert.True(cityBornGears[cityOfBirth.Name].Contains(bornGear.Nickname));
                            Assert.Equal(gearTagNotes[bornGear.Nickname], bornGear.Tag.Note);
                        }

                        Assert.Equal(cityStationedGears[cityOfBirth.Name].Count, cityOfBirth.StationedGears.Count);
                        foreach (var stationedGear in cityOfBirth.StationedGears)
                        {
                            Assert.True(cityStationedGears[cityOfBirth.Name].Contains(stationedGear.Nickname));
                            Assert.Equal(gearTagNotes[stationedGear.Nickname], stationedGear.Tag.Note);
                        }
                    }
                }
            }
        }
    }
}
