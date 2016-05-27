// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.GearsOfWarModel;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Xunit;

// ReSharper disable ReplaceWithSingleCallToSingle

namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public abstract class GearsOfWarQueryTestBase<TTestStore, TFixture> : IClassFixture<TFixture>, IDisposable
        where TTestStore : TestStore
        where TFixture : GearsOfWarQueryFixtureBase<TTestStore>, new()
    {
        [ConditionalFact]
        public virtual void Entity_equality_empty()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears.Where(g => g == new Gear());
                var result = query.ToList();

                Assert.Equal(0, result.Count);
            }
        }

        [ConditionalFact]
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

        [ConditionalFact]
        public virtual void Include_multiple_one_to_one_and_one_to_many_self_reference()
        {
            using (var context = CreateContext())
            {
                var query = context.Tags.Include(t => t.Gear.Weapons);
                var result = query.ToList();

                Assert.Equal(6, result.Count);

                var gears = result.Select(t => t.Gear).Where(g => g != null).ToList();
                Assert.Equal(5, gears.Count);

                Assert.True(gears.All(g => g.Weapons != null));
            }
        }

        [ConditionalFact]
        public virtual void Include_multiple_one_to_one_optional_and_one_to_one_required()
        {
            using (var context = CreateContext())
            {
                var query = context.Tags.Include(t => t.Gear.Squad);
                var result = query.ToList();

                Assert.Equal(6, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Include_multiple_one_to_one_and_one_to_one_and_one_to_many()
        {
            using (var context = CreateContext())
            {
                var query = context.Tags.Include(t => t.Gear.Squad.Members);
                var result = query.ToList();

                Assert.Equal(6, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Include_multiple_circular()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears.Include(g => g.CityOfBirth.StationedGears);
                var result = query.ToList();

                Assert.Equal(5, result.Count);

                var cities = result.Select(g => g.CityOfBirth).ToList();
                Assert.True(cities.All(c => c != null));
                Assert.True(cities.All(c => c.BornGears != null));
            }
        }

        [ConditionalFact]
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

        [ConditionalFact]
        public virtual void Include_using_alternate_key()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears.Include(g => g.Weapons).Where(g => g.Nickname == "Marcus");
                var result = query.ToList();

                Assert.Equal(1, result.Count);

                var weapons = result.Single().Weapons.ToList();
                Assert.Equal(2, weapons.Count);
                Assert.Contains("Marcus' Lancer", weapons.Select(w => w.Name));
                Assert.Contains("Marcus' Gnasher", weapons.Select(w => w.Name));
            }
        }

        [ConditionalFact]
        public virtual void Include_multiple_include_then_include()
        {
            Dictionary<string, string> gearAssignedCities;
            Dictionary<string, string> gearCitiesOfBirth;
            Dictionary<string, string> gearTagNotes;
            Dictionary<string, List<string>> cityStationedGears;
            Dictionary<string, List<string>> cityBornGears;

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

                for (var i = 0; i < expectedGearCount; i++)
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

        [ConditionalFact]
        public virtual void Include_navigation_on_derived_type()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears.OfType<Officer>().Include(o => o.Reports);
                var result = query.ToList();

                Assert.Equal(2, result.Count);

                var marcusReports = result.Where(e => e.Nickname == "Marcus").Single().Reports.ToList();
                Assert.Equal(3, marcusReports.Count);
                Assert.Contains("Baird", marcusReports.Select(g => g.Nickname));
                Assert.Contains("Cole Train", marcusReports.Select(g => g.Nickname));
                Assert.Contains("Dom", marcusReports.Select(g => g.Nickname));

                var bairdReports = result.Where(e => e.Nickname == "Baird").Single().Reports.ToList();
                Assert.Equal(1, bairdReports.Count);
                Assert.Contains("Paduk", bairdReports.Select(g => g.Nickname));
            }
        }

        [ConditionalFact]
        public virtual void Select_Where_Navigation_Included()
        {
            using (var context = CreateContext())
            {
                var cogTags
                    = (from ct in context.Set<CogTag>().Include(o => o.Gear)
                       where ct.Gear.Nickname == "Marcus"
                       select ct).ToList();

                Assert.Equal(1, cogTags.Count);
                Assert.True(cogTags.All(o => o.Gear != null));
            }
        }

        [ConditionalFact]
        public virtual void Include_with_join_reference1()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears.Join(
                    context.Tags,
                    g => new { SquadId = (int?)g.SquadId, g.Nickname },
                    t => new { SquadId = t.GearSquadId, Nickname = t.GearNickName },
                    (g, t) => g).Include(g => g.CityOfBirth);

                var result = query.ToList();
                Assert.Equal(5, result.Count);
                Assert.True(result.All(g => g.CityOfBirth != null));
            }
        }

        [ConditionalFact]
        public virtual void Include_with_join_reference2()
        {
            using (var context = CreateContext())
            {
                var query = context.Tags.Join(
                    context.Gears,
                    t => new { SquadId = t.GearSquadId, Nickname = t.GearNickName },
                    g => new { SquadId = (int?)g.SquadId, g.Nickname },
                    (t, g) => g).Include(g => g.CityOfBirth);

                var result = query.ToList();
                Assert.Equal(5, result.Count);
                Assert.True(result.All(g => g.CityOfBirth != null));
            }
        }

        [ConditionalFact]
        public virtual void Include_with_join_collection1()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears.Join(
                    context.Tags,
                    g => new { SquadId = (int?)g.SquadId, g.Nickname },
                    t => new { SquadId = t.GearSquadId, Nickname = t.GearNickName },
                    (g, t) => g).Include(g => g.Weapons);

                var result = query.ToList();
                Assert.Equal(5, result.Count);
                Assert.True(result.All(g => g.Weapons.Count > 0));
            }
        }

        [ConditionalFact]
        public virtual void Include_with_join_collection2()
        {
            using (var context = CreateContext())
            {
                var query = context.Tags.Join(
                    context.Gears,
                    t => new { SquadId = t.GearSquadId, Nickname = t.GearNickName },
                    g => new { SquadId = (int?)g.SquadId, g.Nickname },
                    (t, g) => g).Include(g => g.Weapons);

                var result = query.ToList();
                Assert.Equal(5, result.Count);
                Assert.True(result.All(g => g.Weapons.Count > 0));
            }
        }

        [ConditionalFact]
        public virtual void Include_where_list_contains_navigation()
        {
            using (var context = CreateContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

                var tags = context.Tags.Select(t => (Guid?)t.Id).ToList();

                var gears = context.Gears
                    .Include(g => g.Tag)
                    .Where(g => g.Tag != null && tags.Contains(g.Tag.Id))
                    .ToList();

                Assert.Equal(5, gears.Count);
            }
        }

        [ConditionalFact]
        public virtual void Include_with_join_multi_level()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears.Join(
                    context.Tags,
                    g => new { SquadId = (int?)g.SquadId, g.Nickname },
                    t => new { SquadId = t.GearSquadId, Nickname = t.GearNickName },
                    (g, t) => g).Include(g => g.CityOfBirth.StationedGears);

                var result = query.ToList();
                Assert.Equal(5, result.Count);
                Assert.True(result.All(g => g.CityOfBirth != null));
            }
        }

        [ConditionalFact]
        public virtual void Include_with_join_and_inheritance1()
        {
            using (var context = CreateContext())
            {
                var query = context.Tags.Join(
                    context.Gears.OfType<Officer>(),
                    t => new { SquadId = t.GearSquadId, Nickname = t.GearNickName },
                    o => new { SquadId = (int?)o.SquadId, o.Nickname },
                    (t, o) => o).Include(o => o.CityOfBirth);

                var result = query.ToList();
                Assert.Equal(2, result.Count);
                Assert.True(result.All(o => o.CityOfBirth != null));
            }
        }

        [ConditionalFact]
        public virtual void Include_with_join_and_inheritance2()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears.OfType<Officer>().Join(
                    context.Tags,
                    o => new { SquadId = (int?)o.SquadId, o.Nickname },
                    t => new { SquadId = t.GearSquadId, Nickname = t.GearNickName },
                    (o, t) => o).Include(g => g.Weapons);

                var result = query.ToList();
                Assert.Equal(2, result.Count);
                Assert.True(result.All(o => o.Weapons.Count > 0));
            }
        }

        [ConditionalFact]
        public virtual void Include_with_join_and_inheritance3()
        {
            using (var context = CreateContext())
            {
                var query = context.Tags.Join(
                    context.Gears.OfType<Officer>(),
                    t => new { SquadId = t.GearSquadId, Nickname = t.GearNickName },
                    g => new { SquadId = (int?)g.SquadId, g.Nickname },
                    (t, o) => o).Include(o => o.Reports);

                var result = query.ToList();
                Assert.Equal(2, result.Count);
                Assert.True(result.All(o => o.Reports.Count > 0));
            }
        }

        [ConditionalFact]
        public virtual void Include_with_nested_navigation_in_order_by()
        {
            using (var context = CreateContext())
            {
                var query = context.Weapons
                    .Include(w => w.Owner)
                    .OrderBy(e => e.Owner.CityOfBirth.Name);

                var result = query.ToList();
                Assert.Equal(10, result.Count);
                Assert.Equal(null, result[0].Owner?.CityOrBirthName);
                Assert.Equal("Ephyra", result[1].Owner.CityOrBirthName);
                Assert.Equal("Ephyra", result[2].Owner.CityOrBirthName);
                Assert.Equal("Hanover", result[3].Owner.CityOrBirthName);
                Assert.Equal("Hanover", result[4].Owner.CityOrBirthName);
                Assert.Equal("Jacinto", result[5].Owner.CityOrBirthName);
                Assert.Equal("Jacinto", result[6].Owner.CityOrBirthName);
                Assert.Equal("Unknown", result[7].Owner.CityOrBirthName);
                Assert.Equal("Unknown", result[8].Owner.CityOrBirthName);
                Assert.Equal("Unknown", result[9].Owner.CityOrBirthName);
            }
        }

        [ConditionalFact]
        public virtual void Where_enum()
        {
            using (var context = CreateContext())
            {
                var gears = context.Gears
                    .Where(g => g.Rank == MilitaryRank.Sergeant)
                    .ToList();

                Assert.Equal(1, gears.Count);
            }
        }

        [ConditionalFact]
        public virtual void Where_nullable_enum_with_constant()
        {
            using (var context = CreateContext())
            {
                var weapons = context.Weapons
                    .Where(w => w.AmmunitionType == AmmunitionType.Cartridge)
                    .ToList();

                Assert.Equal(5, weapons.Count);
            }
        }

        [ConditionalFact]
        public virtual void Where_nullable_enum_with_null_constant()
        {
            using (var context = CreateContext())
            {
                var weapons = context.Weapons
                    .Where(w => w.AmmunitionType == null)
                    .ToList();

                Assert.Equal(1, weapons.Count);
            }
        }

        [ConditionalFact]
        public virtual void Where_nullable_enum_with_non_nullable_parameter()
        {
            var ammunitionType = AmmunitionType.Cartridge;

            using (var context = CreateContext())
            {
                var weapons = context.Weapons
                    .Where(w => w.AmmunitionType == ammunitionType)
                    .ToList();

                Assert.Equal(5, weapons.Count);
            }
        }

        [ConditionalFact]
        public virtual void Where_nullable_enum_with_nullable_parameter()
        {
            AmmunitionType? ammunitionType = AmmunitionType.Cartridge;

            using (var context = CreateContext())
            {
                var weapons = context.Weapons
                    .Where(w => w.AmmunitionType == ammunitionType)
                    .ToList();

                Assert.Equal(5, weapons.Count);
            }

            ammunitionType = null;

            using (var context = CreateContext())
            {
                var weapons = context.Weapons
                    .Where(w => w.AmmunitionType == ammunitionType)
                    .ToList();

                Assert.Equal(1, weapons.Count);
            }
        }

        [ConditionalFact]
        public virtual void Where_count_subquery_without_collision()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears.Where(w => w.Weapons.Count == 2);
                var result = query.ToList();

                Assert.Equal(4, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Where_any_subquery_without_collision()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears.Where(w => w.Weapons.Any());
                var result = query.ToList();

                Assert.Equal(5, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Select_inverted_boolean()
        {
            using (var context = CreateContext())
            {
                var automaticWeapons = context.Weapons
                    .Where(w => w.IsAutomatic)
                    .Select(w => new { w.Id, Manual = !w.IsAutomatic })
                    .ToList();

                Assert.True(automaticWeapons.All(t => t.Manual == false));
            }
        }

        [ConditionalFact]
        public virtual void Select_comparison_with_null()
        {
            AmmunitionType? ammunitionType = AmmunitionType.Cartridge;
            using (var context = CreateContext())
            {
                var cartridgeWeapons = context.Weapons
                    .Where(w => w.AmmunitionType == ammunitionType)
                    .Select(w => new { w.Id, Cartidge = w.AmmunitionType == ammunitionType })
                    .ToList();

                Assert.True(cartridgeWeapons.All(t => t.Cartidge));
            }

            ammunitionType = null;
            using (var context = CreateContext())
            {
                var cartridgeWeapons = context.Weapons
                    .Where(w => w.AmmunitionType == ammunitionType)
                    .Select(w => new { w.Id, Cartidge = w.AmmunitionType == ammunitionType })
                    .ToList();

                Assert.True(cartridgeWeapons.All(t => t.Cartidge));
            }
        }

        [ConditionalFact]
        public virtual void Select_ternary_operation_with_boolean()
        {
            using (var context = CreateContext())
            {
                var weapons = context.Weapons
                    .Select(w => new { w.Id, Num = w.IsAutomatic ? 1 : 0 })
                    .ToList();

                Assert.Equal(3, weapons.Count(w => w.Num == 1));
            }
        }

        [ConditionalFact]
        public virtual void Select_ternary_operation_with_inverted_boolean()
        {
            using (var context = CreateContext())
            {
                var weapons = context.Weapons
                    .Select(w => new { w.Id, Num = !w.IsAutomatic ? 1 : 0 })
                    .ToList();

                Assert.Equal(7, weapons.Count(w => w.Num == 1));
            }
        }

        [ConditionalFact]
        public virtual void Select_ternary_operation_with_has_value_not_null()
        {
            using (var context = CreateContext())
            {
                var cartridgeWeapons = context.Weapons
                    .Where(w => w.AmmunitionType.HasValue && w.AmmunitionType == AmmunitionType.Cartridge)
                    .Select(w => new { w.Id, IsCartidge = w.AmmunitionType.HasValue && w.AmmunitionType.Value == AmmunitionType.Cartridge ? "Yes" : "No" })
                    .ToList();

                Assert.All(cartridgeWeapons,
                    t => Assert.Equal("Yes", t.IsCartidge));
            }
        }

        [ConditionalFact]
        public virtual void Select_ternary_operation_multiple_conditions()
        {
            using (var context = CreateContext())
            {
                var cartridgeWeapons = context.Weapons
                    .Select(w => new { w.Id, IsCartidge = w.AmmunitionType == AmmunitionType.Shell && w.SynergyWithId == 1 ? "Yes" : "No" })
                    .ToList();

                Assert.Equal(9, cartridgeWeapons.Count(w => w.IsCartidge == "No"));
            }
        }

        [ConditionalFact]
        public virtual void Select_ternary_operation_multiple_conditions_2()
        {
            using (var context = CreateContext())
            {
                var cartridgeWeapons = context.Weapons
                    .Select(w => new { w.Id, IsCartidge = !w.IsAutomatic && w.SynergyWithId == 1 ? "Yes" : "No" })
                    .ToList();

                Assert.Equal(9, cartridgeWeapons.Count(w => w.IsCartidge == "No"));
            }
        }

        [ConditionalFact]
        public virtual void Select_multiple_conditions()
        {
            using (var context = CreateContext())
            {
                var cartridgeWeapons = context.Weapons
                    .Select(w => new { w.Id, IsCartidge = !w.IsAutomatic && w.SynergyWithId == 1 })
                    .ToList();

                Assert.Equal(9, cartridgeWeapons.Count(w => !w.IsCartidge));
            }
        }

        [ConditionalFact]
        public virtual void Select_nested_ternary_operations()
        {
            using (var context = CreateContext())
            {
                var cartridgeWeapons = context.Weapons
                    .Select(w => new { w.Id, IsManualCartidge = !w.IsAutomatic ? w.AmmunitionType == AmmunitionType.Cartridge ? "ManualCartridge" : "Manual" : "Auto" })
                    .ToList();

                Assert.Equal(2, cartridgeWeapons.Count(w => w.IsManualCartidge == "ManualCartridge"));
            }
        }

        [ConditionalFact]
        public virtual void Null_propagation_optimization1()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears
                    .Where(g => (g == null ? (bool?)null : (bool?)(g.LeaderNickname == "Marcus")) == (bool?)true)
                    .ToList();

                var result = query.ToList();

                Assert.Equal(3, result.Count);

                var nickNames = result.Select(r => r.Nickname);
                Assert.True(nickNames.Contains("Dom"));
                Assert.True(nickNames.Contains("Cole Train"));
                Assert.True(nickNames.Contains("Baird"));
            }
        }

        [ConditionalFact]
        public virtual void Null_propagation_optimization2()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears
                    .Where(g => (g.LeaderNickname == null ? (bool?)null : (bool?)g.LeaderNickname.EndsWith("us")) == (bool?)true)
                    .ToList();

                var result = query.ToList();

                Assert.Equal(3, result.Count);

                var nickNames = result.Select(r => r.Nickname);
                Assert.True(nickNames.Contains("Dom"));
                Assert.True(nickNames.Contains("Cole Train"));
                Assert.True(nickNames.Contains("Baird"));
            }
        }

        [ConditionalFact]
        public virtual void Null_propagation_optimization3()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears
                    .Where(g => (g.LeaderNickname != null ? (bool?)g.LeaderNickname.EndsWith("us") : (bool?)null) == (bool?)true)
                    .ToList();

                var result = query.ToList();

                Assert.Equal(3, result.Count);

                var nickNames = result.Select(r => r.Nickname);
                Assert.True(nickNames.Contains("Dom"));
                Assert.True(nickNames.Contains("Cole Train"));
                Assert.True(nickNames.Contains("Baird"));
            }
        }

        [ConditionalFact]
        public virtual void Null_propagation_optimization4()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears
                    .Where(g => (null ==  EF.Property<string>(g, "LeaderNickname") ? (bool?)null : (bool?)(g.LeaderNickname.Length == 5)) == (bool?)true)
                    .ToList();

                var result = query.ToList();

                Assert.Equal(1, result.Count);

                var nickNames = result.Select(r => r.Nickname);
                Assert.True(nickNames.Contains("Paduk"));
            }
        }

        [ConditionalFact]
        public virtual void Null_propagation_optimization5()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears
                    .Where(g => (null != g.LeaderNickname ? (bool?)(EF.Property<string>(g, "LeaderNickname").Length == 5) : (bool?)null) == (bool?)true)
                    .ToList();

                var result = query.ToList();

                Assert.Equal(1, result.Count);

                var nickNames = result.Select(r => r.Nickname);
                Assert.True(nickNames.Contains("Paduk"));
            }
        }

        [ConditionalFact]
        public virtual void Select_null_propagation_negative1()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears
                    .Select(g => g.LeaderNickname != null ? (bool?)(g.Nickname.Length == 5) : (bool?)null) 
                    .ToList();

                var result = query.ToList();

                Assert.Equal(5, result.Count);
                Assert.Equal(1, result.Count(r => r == null));
                Assert.Equal(2, result.Count(r => r == true));
                Assert.Equal(2, result.Count(r => r == false));
            }
        }

        [ConditionalFact]
        public virtual void Select_null_propagation_negative2()
        {
            using (var context = CreateContext())
            {
                var query = from g1 in context.Gears
                            from g2 in context.Gears
                            select g1.LeaderNickname != null ? g2.LeaderNickname : (string)null;

                var result = query.ToList();

                Assert.Equal(25, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Select_Where_Navigation()
        {
            using (var context = CreateContext())
            {
                var cogTags
                    = (from ct in context.Set<CogTag>()
                       where ct.Gear.Nickname == "Marcus"
                       select ct).ToList();

                Assert.Equal(1, cogTags.Count);
            }
        }

        // issue 4539
        ////[ConditionalFact]
        public virtual void Select_Where_Navigation_Scalar_Equals_Navigation_Scalar()
        {
            List<KeyValuePair<Guid, Guid>> expected;
            using (var context = CreateContext())
            {
                expected = (from ct1 in context.Tags.Include(t => t.Gear).ToList()
                            from ct2 in context.Tags.Include(t => t.Gear).ToList()
                            where ct1.Gear?.Nickname == ct2.Gear?.Nickname
                            select new KeyValuePair<Guid, Guid>(ct1.Id, ct2.Id)).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from ct1 in context.Tags
                            from ct2 in context.Tags
                            where ct1.Gear.Nickname == ct2.Gear.Nickname
                            select new { Tag1 = ct1, Tag2 = ct2 };

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                Assert.True(expected.All(e => result.Any(r => r.Tag1.Id == e.Key && r.Tag2.Id == e.Value)));
            }
        }

        // issue 4539
        ////[ConditionalFact]
        public virtual void Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected()
        {
            List<KeyValuePair<Guid, Guid>> expected;
            using (var context = CreateContext())
            {
                expected = (from ct1 in context.Tags.Include(t => t.Gear).ToList()
                            from ct2 in context.Tags.Include(t => t.Gear).ToList()
                            where ct1.Gear?.Nickname == ct2.Gear?.Nickname
                            select new KeyValuePair<Guid, Guid>(ct1.Id, ct2.Id)).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from ct1 in context.Tags
                            from ct2 in context.Tags
                            where ct1.Gear.Nickname == ct2.Gear.Nickname
                            select new { Id1 = ct1.Id, Id2 = ct2.Id };

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
                Assert.True(expected.All(e => result.Any(r => r.Id1 == e.Key && r.Id2 == e.Value)));
            }
        }

        [ConditionalFact]
        public virtual void Optional_Navigation_Null_Coalesce_To_Clr_Type()
        {
            using (var context = CreateContext())
            {
                var query = context.Weapons.Select(w =>
                    new Weapon
                    {
                        IsAutomatic = (bool?)w.SynergyWith.IsAutomatic ?? false
                    });
                var result = query.First();

                Assert.False(result.IsAutomatic);
            }
        }

        [ConditionalFact]
        public virtual void Select_Where_Navigation_Client()
        {
            using (var context = CreateContext())
            {
                var cogTags
                    = (from o in context.Set<CogTag>()
                       where o.Gear != null && o.Gear.IsMarcus
                       select o).ToList();

                Assert.Equal(1, cogTags.Count);
            }
        }

        [ConditionalFact]
        public virtual void Select_Where_Navigation_Null()
        {
            using (var context = CreateContext())
            {
                var cogTags
                    = (from ct in context.Set<CogTag>()
                       where ct.Gear == null
                       select ct).ToList();

                Assert.Equal(1, cogTags.Count);
            }
        }

        [ConditionalFact]
        public virtual void Select_Where_Navigation_Null_Reverse()
        {
            using (var context = CreateContext())
            {
                var cogTags
                    = (from ct in context.Set<CogTag>()
                       where null == ct.Gear
                       select ct).ToList();

                Assert.Equal(1, cogTags.Count);
            }
        }

        // issue 4539
        ////[ConditionalFact]
        public virtual void Select_Where_Navigation_Equals_Navigation()
        {
            using (var context = CreateContext())
            {
                var cogTags
                    = (from ct1 in context.Set<CogTag>()
                       from ct2 in context.Set<CogTag>()
                       where ct1.Gear == ct2.Gear
                       select new { ct1, ct2 }).ToList();

                Assert.Equal(6, cogTags.Count);
            }
        }

        [ConditionalFact]
        public virtual void Singleton_Navigation_With_Member_Access()
        {
            using (var context = CreateContext())
            {
                var cogTags
                    = (from ct in context.Set<CogTag>()
                       where ct.Gear.Nickname == "Marcus"
                       where ct.Gear.CityOrBirthName != "Ephyra"
                       select new { B = ct.Gear.CityOrBirthName }).ToList();

                Assert.Equal(1, cogTags.Count);
                Assert.True(cogTags.All(o => o.B != null));
            }
        }

        [ConditionalFact]
        public virtual void Select_Singleton_Navigation_With_Member_Access()
        {
            using (var context = CreateContext())
            {
                var cogTags
                    = (from ct in context.Set<CogTag>()
                       where ct.Gear.Nickname == "Marcus"
                       where ct.Gear.CityOrBirthName != "Ephyra"
                       select new { A = ct.Gear, B = ct.Gear.CityOrBirthName }).ToList();

                Assert.Equal(1, cogTags.Count);
                Assert.True(cogTags.All(o => o.A != null && o.B != null));
            }
        }

        [ConditionalFact]
        public virtual void GroupJoin_Composite_Key()
        {
            using (var context = CreateContext())
            {
                var gears
                    = (from ct in context.Set<CogTag>()
                       join g in context.Set<Gear>()
                           on new { N = ct.GearNickName, S = ct.GearSquadId }
                           equals new { N = g.Nickname, S = (int?)g.SquadId } into gs
                       from g in gs
                       select g).ToList();

                Assert.Equal(5, gears.Count);
            }
        }

        [ConditionalFact]
        public virtual void Join_navigation_translated_to_subquery_composite_key()
        {
            List<Gear> gears;
            List<CogTag> tags;
            using (var context = CreateContext())
            {
                gears = context.Gears.ToList();
                tags = context.Tags.Include(e => e.Gear).ToList();
            }

            ClearLog();

            using (var context = CreateContext())
            {
                var query = from g in context.Gears
                            join t in context.Tags on g.FullName equals t.Gear.FullName
                            select new { g.FullName, t.Note };

                var result = query.ToList();

                var expected = (from g in gears
                                join t in tags on g.FullName equals t.Gear?.FullName
                                select new { g.FullName, t.Note }).ToList();

                Assert.Equal(expected.Count, result.Count);
                foreach (var resultItem in result)
                {
                    Assert.True(expected.Contains(resultItem));
                }
            }
        }

        [ConditionalFact]
        public virtual void Collection_with_inheritance_and_join_include_joined()
        {
            using (var context = CreateContext())
            {
                var query = (from t in context.Tags
                             join g in context.Gears.OfType<Officer>() on new { id1 = t.GearSquadId, id2 = t.GearNickName }
                                 equals new { id1 = (int?)g.SquadId, id2 = g.Nickname }
                             select g).Include(g => g.Tag);

                var result = query.ToList();

                Assert.NotNull(result);
            }
        }

        [ConditionalFact]
        public virtual void Collection_with_inheritance_and_join_include_source()
        {
            using (var context = CreateContext())
            {
                var query = (from g in context.Gears.OfType<Officer>()
                             join t in context.Tags on new { id1 = (int?)g.SquadId, id2 = g.Nickname }
                                 equals new { id1 = t.GearSquadId, id2 = t.GearNickName }
                             select g).Include(g => g.Tag);

                var result = query.ToList();

                Assert.NotNull(result);
            }
        }

        [ConditionalFact]
        public virtual void Non_unicode_string_literal_is_used_for_non_unicode_column()
        {
            using (var context = CreateContext())
            {
                var query = from c in context.Cities
                            where c.Location == "Unknown"
                            select c;

                var result = query.ToList();

                Assert.Equal(1, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Non_unicode_string_literal_is_used_for_non_unicode_column_right()
        {
            using (var context = CreateContext())
            {
                var query = from c in context.Cities
                            where "Unknown" == c.Location
                            select c;

                var result = query.ToList();

                Assert.Equal(1, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Non_unicode_parameter_is_used_for_non_unicode_column()
        {
            using (var context = CreateContext())
            {
                var value = "Unknown";
                var query = from c in context.Cities
                            where c.Location == value
                            select c;

                var result = query.ToList();

                Assert.Equal(1, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Non_unicode_string_literals_in_contains_is_used_for_non_unicode_column()
        {
            using (var context = CreateContext())
            {
                var cities = new List<string> { "Unknown", "Jacinto's location", "Ephyra's location" };
                var query = from c in context.Cities
                            where cities.Contains(c.Location)
                            select c;

                var result = query.ToList();

                Assert.Equal(3, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Non_unicode_string_literals_is_used_for_non_unicode_column_with_subquery()
        {
            using (var context = CreateContext())
            {
                var query = from c in context.Cities
                            where (c.Location == "Unknown") && (c.BornGears.Count(g => g.Nickname == "Paduk") == 1)
                            select c;

                var result = query.ToList();

                Assert.Equal(1, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Non_unicode_string_literals_is_used_for_non_unicode_column_in_subquery()
        {
            using (var context = CreateContext())
            {
                var query = from g in context.Gears
                            where g.Nickname == "Marcus" && g.CityOfBirth.Location == "Jacinto's location"
                            select g;

                var result = query.ToList();

                Assert.Equal(1, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Non_unicode_string_literals_is_used_for_non_unicode_column_with_contains()
        {
            using (var context = CreateContext())
            {
                var query = from c in context.Cities
                            where c.Location.Contains("Jacinto")
                            select c;

                var result = query.ToList();

                Assert.Equal(1, result.Count);
            }
        }

        //[ConditionalFact]
        public virtual void Non_unicode_string_literals_is_used_for_non_unicode_column_with_concat()
        {
            using (var context = CreateContext())
            {
                var query = from c in context.Cities
                            where (c.Location + "Added").Contains("Add")
                            select c;

                var result = query.ToList();

                Assert.Equal(4, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result1()
        {
            using (var context = CreateContext())
            {
                var query = from g1 in context.Gears.Include(g => g.Weapons)
                            join g2 in context.Gears 
                            on g1.LeaderNickname equals g2.Nickname into grouping
                            from g2 in grouping.DefaultIfEmpty()
                            select g2 ?? g1;

                var result = query.ToList();

                Assert.True(result.Count(r => r.Weapons.Count > 0) >= 4);
            }
        }

        [ConditionalFact]
        public virtual void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result2()
        {
            using (var context = CreateContext())
            {
                var query = from g1 in context.Gears
                            join g2 in context.Gears.Include(g => g.Weapons) 
                            on g1.LeaderNickname equals g2.Nickname into grouping
                            from g2 in grouping.DefaultIfEmpty()
                            select g2 ?? g1;

                var result = query.ToList();

                Assert.True(result.All(r => r.Weapons.Count > 0));
            }
        }

        [ConditionalFact]
        public virtual void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result3()
        {
            using (var context = CreateContext())
            {
                var query = from g1 in context.Gears.Include(g => g.Weapons)
                            join g2 in context.Gears.Include(g => g.Weapons)
                            on g1.LeaderNickname equals g2.Nickname into grouping
                            from g2 in grouping.DefaultIfEmpty()
                            select g2 ?? g1;

                var result = query.ToList();

                Assert.True(result.All(r => r.Weapons.Count > 0));
            }
        }

        [ConditionalFact]
        public virtual void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_inheritance_and_coalesce_result()
        {
            using (var context = CreateContext())
            {
                var query = from g1 in context.Gears.Include(g => g.Weapons)
                            join g2 in context.Gears.OfType<Officer>().Include(g => g.Weapons)
                            on g1.LeaderNickname equals g2.Nickname into grouping
                            from g2 in grouping.DefaultIfEmpty()
                            select g2 ?? g1;

                var result = query.ToList();

                Assert.True(result.All(r => r.Weapons.Count > 0));
            }
        }

        [ConditionalFact]
        public virtual void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_conditional_result()
        {
            using (var context = CreateContext())
            {
                var query = from g1 in context.Gears.Include(g => g.Weapons)
                            join g2 in context.Gears.Include(g => g.Weapons)
                            on g1.LeaderNickname equals g2.Nickname into grouping
                            from g2 in grouping.DefaultIfEmpty()
                            select g2 != null ? g2 : g1;

                var result = query.ToList();

                Assert.True(result.All(r => r.Weapons.Count > 0));
            }
        }

        [ConditionalFact]
        public virtual void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_complex_projection_result()
        {
            using (var context = CreateContext())
            {
                var query = from g1 in context.Gears.Include(g => g.Weapons)
                            join g2 in context.Gears.Include(g => g.Weapons)
                            on g1.LeaderNickname equals g2.Nickname into grouping
                            from g2 in grouping.DefaultIfEmpty()
                            select new { g1, g2, coalesce = g2 ?? g1, conditional = g2 != null ? g2 : g1 };

                var result = query.ToList();

                Assert.True(result.All(r => r.coalesce.Weapons.Count > 0));
                Assert.True(result.All(r => r.conditional.Weapons.Count > 0));
            }
        }

        protected GearsOfWarContext CreateContext() => Fixture.CreateContext(TestStore);

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

        public void Dispose() => TestStore.Dispose();
    }
}
